using CommunityToolkit.Mvvm.ComponentModel;
using Downloader;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PersianDownloadManager.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    public ObservableCollection<DownloadItem> Downloads { get; } = new();

    // All save requests go through the debouncer — both structural changes
    // (add/remove items, Status, IsQueue, etc.) and noisy progress updates
    // (Progress, DownloadedBytes).  This prevents burst writes when multiple
    // downloads run concurrently; at most one JSON write per debounce window.
    //   - saves 2 s after the last change (quiet-period flush / debounce)
    //   - but never waits longer than 5 s during continuous activity (throttle)
    // Call FlushSave() on app close to persist any outstanding pending save.
    private readonly SaveDebouncer _saveDebouncer;

    public MainWindowViewModel()
    {
        var localItems = LoadLocalItems();
        foreach (var item in localItems)
        {
            if (item.Status == DownloadStatus.Running)
            {
                item.Status = DownloadStatus.Failed;
            }
        }

        Downloads = new(localItems);

        Downloads.CollectionChanged += Downloads_CollectionChanged;

        foreach (var item in Downloads)
        {
            item.PropertyChanged += DownloadItem_PropertyChanged;
        }

        UpdateIndexes(Downloads);
        UpdateQueueOrder(Downloads);

        _saveDebouncer = new SaveDebouncer(
            () => { UpdateQueueOrder(Downloads); SaveDownloadItems(Downloads); },
            debounceDelay: TimeSpan.FromSeconds(2),
            maxInterval: TimeSpan.FromSeconds(5));

    }

    private void Downloads_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (DownloadItem item in e.NewItems)
            {
                item.PropertyChanged += DownloadItem_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (DownloadItem item in e.OldItems)
            {
                item.PropertyChanged -= DownloadItem_PropertyChanged;
            }
        }

        OnPropertyChanged(nameof(IsEmpty));
        UpdateIndexes(Downloads);
        _saveDebouncer?.RequestSave();
    }

    private void DownloadItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DownloadItem.Id):
            case nameof(DownloadItem.FileName):
            case nameof(DownloadItem.FileDirectory):
            case nameof(DownloadItem.FilePath):
            case nameof(DownloadItem.Url):
            case nameof(DownloadItem.Status):
            case nameof(DownloadItem.AddedDate):
            case nameof(DownloadItem.LastDate):
            case nameof(DownloadItem.TotalBytes):
            case nameof(DownloadItem.IsQueue):
            case nameof(DownloadItem.Progress):
            case nameof(DownloadItem.DownloadedBytes):
                _saveDebouncer?.RequestSave();
                break;
        }
    }

    /// <summary>
    /// Flushes any pending debounced save immediately. Call on app close.
    /// </summary>
    public void FlushSave() => _saveDebouncer?.Flush();

    public void Dispose()
    {
        Downloads.CollectionChanged -= Downloads_CollectionChanged;
        foreach (var item in Downloads)
            item.PropertyChanged -= DownloadItem_PropertyChanged;
        _saveDebouncer?.Dispose();
    }

    [ObservableProperty]
    public partial DownloadItem? SelectedItem { get; set; }
    
    partial void OnSelectedItemChanged(DownloadItem? value)
    {
        NotifyPropertiesChanged();
    }

    public void NotifyPropertiesChanged()
    {
        OnPropertyChanged(nameof(CanResume));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(CanStop));
        OnPropertyChanged(nameof(CanPauseAll));
        OnPropertyChanged(nameof(CanDeleteFinished));
    }
    public bool CanResume => SelectedItem?.CanResume == true;
    public bool CanDelete => SelectedItem != null;
    public bool CanStop => SelectedItem?.CanStop == true;
    public bool CanPauseAll => Downloads.Any(x => x.Status == DownloadStatus.Running);
    public bool CanDeleteFinished => Downloads.Any(x => x.Status == DownloadStatus.Completed);
    public bool IsEmpty => Downloads.Count == 0;
}
