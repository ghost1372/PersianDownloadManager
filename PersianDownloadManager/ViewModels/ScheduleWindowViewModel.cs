using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PersianDownloadManager.ViewModels;

public partial class ScheduleWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<DownloadItem> Downloads { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    public partial DownloadItem SelectedItem { get; set; }

    [ObservableProperty]
    public partial IList<object> SelectedItems { get; set; }

    public ScheduleWindowViewModel()
    {
        // Initialize Downloads collection with queue items sorted by saved order
        var queuedItems = MainWindow.Instance.ViewModel.Downloads.Where(x => x.IsQueue).ToList();
        var savedOrder = AppHelper.QueueSettings.QueueOrder ?? new List<Guid>();

        // Sort by saved order, with unsaved items at the end
        var sortedItems = queuedItems.OrderBy(item =>
        {
            var index = savedOrder.IndexOf(item.Id);
            return index == -1 ? savedOrder.Count : index;
        }).ToList();

        Downloads = new ObservableCollection<DownloadItem>(sortedItems);

        Downloads.CollectionChanged += (s, e) =>
        {
            UpdateQueueOrder(Downloads);
            UpdateIndexes(Downloads);
            AppHelper.QueueSettings.Save(); // Explicitly save to ensure persistence
        };

        UpdateIndexes(Downloads);
    }


    [RelayCommand(CanExecute = nameof(OnCanExecute))]
    public void Delete()
    {
        if (SelectedItems == null || SelectedItems.Count == 0)
            return;

        var items = SelectedItems.OfType<DownloadItem>().ToList();

        foreach (var item in items)
        {
            var task = MainWindow.Instance.downloadTasks.Where(x=>x.Item.Id == item.Id).FirstOrDefault();
            task?.Pause();
            Downloads.Remove(item);
            var originalItem = MainWindow.Instance.ViewModel.Downloads.Where(x => x.Id == item.Id).FirstOrDefault();
            if (originalItem != null)
            {
                originalItem.IsQueue = false;
            }
        }

        // Ensure queue order is saved after deletion
        UpdateQueueOrder(Downloads);
        AppHelper.QueueSettings.Save();
    }


    [RelayCommand(CanExecute = nameof(OnCanExecute))]
    public void MoveDown()
    {
        if (SelectedItems == null || SelectedItems.Count == 0)
            return;

        var items = SelectedItems.OfType<DownloadItem>().OrderByDescending(item => Downloads.IndexOf(item)).ToList();

        foreach (var item in items)
        {
            int index = Downloads.IndexOf(item);

            if (index < Downloads.Count - 1 && !items.Contains(Downloads[index + 1]))
            {
                Downloads.Move(index, index + 1);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(OnCanExecute))]
    public void MoveUp()
    {
        if (SelectedItems == null || SelectedItems.Count == 0)
            return;

        var items = SelectedItems.OfType<DownloadItem>().OrderBy(item => Downloads.IndexOf(item)).ToList();

        foreach (var item in items)
        {
            int index = Downloads.IndexOf(item);

            if (index > 0 && !items.Contains(Downloads[index - 1]))
            {
                Downloads.Move(index, index - 1);                
            }
        }
    }

    private bool OnCanExecute()
    {
        if (SelectedItem != null)
            return true;

        return false;
    }
}
