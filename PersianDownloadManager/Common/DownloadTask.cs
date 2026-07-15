using Downloader;
using Microsoft.UI.Dispatching;
using System.ComponentModel;

namespace PersianDownloadManager.Common;

public sealed partial class DownloadTask : IDisposable
{
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    private readonly object _lock = new();
    private bool _startedInThisSession;

    private double _progress;
    private double _speed;
    private long _downloaded;

    public DownloadItem Item { get; }
    public DownloadService Service { get; }

    private readonly DispatcherQueueTimer _timer;
    public event EventHandler<DownloadStatus> StatusChanged;
    public DownloadTask(DownloadItem item)
    {
        Item = item;
        var downloadOpt = new DownloadConfiguration()
        {
            BufferBlockSize = DownloadEngineSettings.BufferBlockSize,
            
            ChunkCount = DownloadEngineSettings.ChunkCount,
            
            MaximumBytesPerSecond = DownloadEngineSettings.MaximumBytesPerSecond,
            
            MaxTryAgainOnFailure = DownloadEngineSettings.MaxTryAgainOnFailure,

            MaximumMemoryBufferBytes = DownloadEngineSettings.MaximumMemoryBufferBytes,

            ParallelDownload = DownloadEngineSettings.ParallelDownload,

            ParallelCount = DownloadEngineSettings.ParallelCount,

            BlockTimeout = DownloadEngineSettings.BlockTimeout,
            
            HttpClientTimeout = DownloadEngineSettings.HttpClientTimeout,

            ClearPackageOnCompletionWithFailure = DownloadEngineSettings.ClearPackageOnCompletionWithFailure,

            MinimumSizeOfChunking = DownloadEngineSettings.MinimumSizeOfChunking, 

            MinimumChunkSize = DownloadEngineSettings.MinimumChunkSize,
            
            FileExistPolicy = DownloadEngineSettings.FileExistPolicy,

            EnableAutoResumeDownload = DownloadEngineSettings.EnableAutoResumeDownload,

            DownloadFileExtension = DownloadEngineSettings.DownloadFileExtension,

            CheckDiskSizeBeforeDownload = true,
        };

        Service = new DownloadService(downloadOpt);

        _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);

        _timer.Tick += Timer_Tick;
        
        Service.DownloadStarted += Service_DownloadStarted;
        Service.DownloadProgressChanged += Service_DownloadProgressChanged;
        Service.DownloadFileCompleted += Service_DownloadCompleted;
    }

    private void Timer_Tick(DispatcherQueueTimer sender, object args)
    {
        double progress;
        double speed;
        long downloaded;

        lock (_lock)
        {
            progress = _progress;
            speed = _speed;
            downloaded = _downloaded;
        }

        var currentStatus = Service.Status;
        if (Item.Status != currentStatus)
        {
            Item.Status = currentStatus;
            StatusChanged?.Invoke(this, currentStatus);
        }

        Item.Progress = progress;
        Item.SpeedBytesPerSecond = speed;
        Item.DownloadedBytes = downloaded;
    }

    private void Service_DownloadStarted(object? sender, Downloader.DownloadStartedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            Item.LastDate = DateTime.Now.ToShortDateString();
            Item.Status = Service.Status;
            Item.TotalBytes = e.TotalBytesToReceive;
            _timer.Start();
            StatusChanged?.Invoke(this, Service.Status);
        });
    }

    private void Service_DownloadProgressChanged(object? sender, Downloader.DownloadProgressChangedEventArgs e)
    {
        lock (_lock)
        {
            _progress = e.ProgressPercentage;
            _speed = e.BytesPerSecondSpeed;
            _downloaded = e.ReceivedBytesSize;
        }
    }

    private void Service_DownloadCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            _timer.Stop();
            Item.Status = Service.Status;
            StatusChanged?.Invoke(this, Service.Status);
        });
    }

    public async Task StartAsync()
    {
        _startedInThisSession = true;

        // If file info is missing or this is a fresh download, get file info first.
        if (Item.Status == DownloadStatus.None || Item.Status == DownloadStatus.Created || string.IsNullOrWhiteSpace(Item.FilePath) || Item.TotalBytes == 0)
        {
            try
            {
                var info = await AppHelper.GetFileInfo(Service, Item.Url.ToString(), Item);
                if (string.IsNullOrWhiteSpace(Item.FilePath))
                {
                    Item.FilePath = info.AvailableFileName.FilePath;
                }
                Item.TotalBytes = info.RemoteFileInfo?.FileSize ?? 0;
                Item.IsResumeCapabilitySupported = info.RemoteFileInfo?.SupportsRange ?? false;
            }
            catch
            {
                // Ignore errors here; service will surface them when download starts.
            }
        }

        await Service.DownloadFileTaskAsync(Item.Url.ToString(), Item.FilePath);
    }

    public void Pause()
    {
        Service.Pause();
        Item.Status = Service.Status;
        _timer?.Stop();

        StatusChanged?.Invoke(this, Service.Status);
    }

    public async Task ResumeAsync()
    {
        if (_startedInThisSession)
        {
            Service.Resume();
            Item.Status = Service.Status;
            _timer.Start();
            StatusChanged?.Invoke(this, Service.Status);
        }
        else
        {
            await StartAsync();
        }
    }

    public void Dispose()
    {
        try
        {
            _timer?.Stop();
            _timer?.Tick -= Timer_Tick;

            Service.CancelAsync();
            Service?.DownloadStarted -= Service_DownloadStarted;
            Service?.DownloadProgressChanged -= Service_DownloadProgressChanged;
            Service?.DownloadFileCompleted -= Service_DownloadCompleted;
            StatusChanged?.Invoke(this, Service.Status);
            Service?.Dispose();
        }
        catch (Exception)
        {
        }
    }
}