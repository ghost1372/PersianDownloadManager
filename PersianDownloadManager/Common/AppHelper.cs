using Downloader;
using Microsoft.UI.Xaml.Media.Imaging;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

namespace PersianDownloadManager.Common;
public static partial class AppHelper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AppConfig))]
    public static AppConfig Settings = JsonSettings.Configure<AppConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DownloadEngineConfig))]
    public static DownloadEngineConfig DownloadEngineSettings = JsonSettings.Configure<DownloadEngineConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(QueueConfig))]
    public static QueueConfig QueueSettings = JsonSettings.Configure<QueueConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DownloadConfig))]
    public static DownloadConfig DownloadSettings = JsonSettings.Configure<DownloadConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();

    public static string FormatSpeed(double bytesPerSecond)
    {
        string[] units = { "B/s", "KB/s", "MB/s", "GB/s", "TB/s" };

        double value = bytesPerSecond;
        int unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:F1} {units[unit]}";
    }

    public static BitmapImage CreateAsset(string uri)
    {
        return new BitmapImage(new Uri(uri));
    }

    [GeneratedRegex(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$", RegexOptions.IgnoreCase)]
    private static partial Regex DomainRegex();

    public static bool IsValidLink(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        if (!text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            text = "http://" + text;
        }

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
            return false;

        string host = uri.Host;

        // Valid IP?
        if (IPAddress.TryParse(host, out _))
            return true;

        // Must contain at least one dot
        if (!host.Contains('.'))
            return false;

        // Basic domain validation
        return DomainRegex().IsMatch(host);
    }
    public static void ResetDownloadEngineSettingsToDefault()
    {
        DownloadEngineSettings.DownloadFolderPath = Constants.DefaultDownloadFolderPath;
        DownloadEngineSettings.BufferBlockSize = Constants.DefaultBufferBlockSize;
        DownloadEngineSettings.ChunkCount = Constants.DefaultChunkCount;
        DownloadEngineSettings.MaximumBytesPerSecond = Constants.DefaultMaximumBytesPerSecond;
        DownloadEngineSettings.MaxTryAgainOnFailure = Constants.DefaultMaxTryAgainOnFailure;
        DownloadEngineSettings.MaximumMemoryBufferBytes = Constants.DefaultMaximumMemoryBufferBytes;
        DownloadEngineSettings.ParallelDownload = Constants.DefaultParallelDownload;
        DownloadEngineSettings.ParallelCount = Constants.DefaultParallelCount;
        DownloadEngineSettings.BlockTimeout = Constants.DefaultBlockTimeout;
        DownloadEngineSettings.HttpClientTimeout = Constants.DefaultHttpClientTimeout;
        DownloadEngineSettings.ClearPackageOnCompletionWithFailure = Constants.DefaultClearPackageOnCompletionWithFailure;
        DownloadEngineSettings.MinimumSizeOfChunking = Constants.DefaultMinimumSizeOfChunking;
        DownloadEngineSettings.MinimumChunkSize = Constants.DefaultMinimumChunkSize;
        DownloadEngineSettings.FileExistPolicy = Constants.DefaultFileExistPolicy;
        DownloadEngineSettings.EnableAutoResumeDownload = Constants.DefaultEnableAutoResumeDownload;
        DownloadEngineSettings.DownloadFileExtension = Constants.DefaultDownloadFileExtension;
    }
    public static bool ExecuteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });

            return true;
        }

        return false;
    }

    public static void SetWindowIcon(Window window)
    {
        window.AppWindow.SetIcon("Assets/AppIcon.ico");
        window.AppWindow.SetTaskbarIcon("Assets/AppIcon.ico");
    }

    public static (string FileName, string FilePath) GetAvailableFileName(string directoryPath, string fileName, IEnumerable<DownloadItem> downloads)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        string newFileName = fileName;
        string filePath = Path.Combine(directoryPath, newFileName);

        int index = 1;

        while (File.Exists(filePath) || downloads.Any(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase)))
        {
            newFileName = $"{name}_{index}{extension}";
            filePath = Path.Combine(directoryPath, newFileName);
            index++;
        }

        return (newFileName, filePath);
    }

    public static void UpdateIndexes(ObservableCollection<DownloadItem> items)
    {
        int index = 1;

        foreach (DownloadItem item in items)
        {
            item.Index = index++;
        }
    }

    public static void UpdateQueueOrder(ObservableCollection<DownloadItem> items)
    {
        QueueSettings.QueueOrder = items.Where(x => x.IsQueue).Select(x => x.Id).ToList();
    }
    public static void SaveDownloadItems(ObservableCollection<DownloadItem> items)
    {
        DownloadSettings.Items = items.Select(x => new LocalDownloadItem
        {
            Id = x.Id,
            FilePath = x.FilePath,
            Url = x.Url,
            TotalBytes = x.TotalBytes,
            Status = x.Status,
            AddedDate = x.AddedDate,
            LastDate = x.LastDate,
            IsQueue = x.IsQueue,
            Progress = x.Progress,
            DownloadedBytes = x.DownloadedBytes,
            IsResumeCapabilitySupported = x.IsResumeCapabilitySupported,
        }).ToList();
    }

    public static ObservableCollection<DownloadItem> LoadLocalItems()
    {
        var items = new ObservableCollection<DownloadItem>();
        var localItems = DownloadSettings.Items;
        foreach (var item in localItems)
        {
            var newItem = new DownloadItem
            {
                Id = item.Id,
                AddedDate = item.AddedDate,
                DownloadedBytes = item.DownloadedBytes,
                FilePath = item.FilePath,
                IsQueue = item.IsQueue,
                LastDate = item.LastDate,
                Progress = item.Progress,
                Status = item.Status,
                TotalBytes = item.TotalBytes,
                Url = item.Url
            };

            items.Add(newItem);
        }

        return items;
    }

    public async static Task<(RemoteFileInfo RemoteFileInfo, (string FileName, string FilePath) AvailableFileName)> GetFileInfo(DownloadService service, string url, DownloadItem? item)
    {
        var info = await service.GetFileInfoAsync(url);

        var fileName = info?.FileName;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = Path.GetFileName(new System.Uri(url).AbsolutePath);

            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "Unknown";
        }

        var availableFileName = GetAvailableFileName(DownloadEngineSettings.DownloadFolderPath, fileName, MainWindow.Instance.ViewModel.Downloads);

        // Do not modify the provided DownloadItem here; caller should apply the available file name if needed.
        return (info, availableFileName);
    }
}

