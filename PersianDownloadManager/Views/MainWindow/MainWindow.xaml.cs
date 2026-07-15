using Downloader;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage.Pickers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using WinRT;
using WinUI.TableView;

namespace PersianDownloadManager;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<object, Window> windows = new();
    internal readonly List<DownloadTask> downloadTasks = new();
    public MainWindowViewModel ViewModel { get; }
    public static MainWindow Instance { get; private set; }

    public string Query { get; set; }

    private CancellationTokenSource? _token;
    private CancellationTokenSource? _queueCts;
    private bool _queueRunning = false;
    private bool canCloseApp = false;
    private FolderItem? _currentFilter;
    public MainWindow()
    {
        ViewModel = App.GetService<MainWindowViewModel>();
        InitializeComponent();

        Instance = this;

        MainTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, Filter));
        MainTableView.FilterDescriptions.Add(new FilterDescription(string.Empty, SidebarFilter));

        Closed += OnMainWindowClosed;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        foreach (var item in ViewModel.Downloads)
        {
            var task = new DownloadTask(item);
            task.StatusChanged += (s, e) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ViewModel.NotifyPropertiesChanged();
                });
            };
            downloadTasks.AddIfNotExists(task);
        }
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {        
        args.Handled = true;
        if (canCloseApp)
        {
            ViewModel.FlushSave();
            ViewModel.Dispose();

            args.Handled = false;

            App.Current.RemoveTrayIcon();
        }
        else
        {
            App.MainWindow.AppWindow.Hide();
        }
    }

    #region Filter/Search
    internal bool Filter(object? item)
    {
        if (string.IsNullOrWhiteSpace(Query)) return true;
        if (item is null) return false;

        var model = (DownloadItem)item;

        return model.FileName?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Size?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Status.ToString()?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.RemainingTime?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.Speed?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.AddedDate?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.LastDate?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true ||
               model.FileDirectory?.Contains(Query, StringComparison.OrdinalIgnoreCase) is true;
    }
    internal bool SidebarFilter(object? item)
    {
        if (_currentFilter is null)
            return true;

        if (item is not DownloadItem download)
            return false;

        // Category
        bool categoryMatch =
            _currentFilter.DownloadCategory == SidebarDownloadCategoryModel.All ||
            download.Category == _currentFilter.DownloadCategory;

        // Status
        bool statusMatch = _currentFilter.DownloadStatus switch
        {
            SidebarDownloadStatusModel.All => true,

            SidebarDownloadStatusModel.Finished =>
                download.Status == DownloadStatus.Completed,

            SidebarDownloadStatusModel.UnFinished =>
                download.Status != DownloadStatus.Completed,

            _ => true
        };

        return categoryMatch && statusMatch;
    }

    private async void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Query = sender.Text;

        if (_token is not null)
        {
            _token.Cancel();
            _token.Dispose();
            _token = null;
        }

        _token = new CancellationTokenSource();
        await RefreshFilter(_token.Token);
    }
    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {

    }
    private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
    {
        HeaderAutoSuggestBox.Focus(FocusState.Programmatic);
    }
    private async Task RefreshFilter(CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);
        }
        catch
        {
            return;
        }

        _token?.Dispose();
        _token = null;
        MainTableView.RefreshFilter();
        UpdateIndexes(ViewModel.Downloads);
    }
    #endregion

    #region SideBarView
    public ObservableCollection<FolderItem> SidebarViewItems { get; set; } = new ObservableCollection<FolderItem>()
    {
        new FolderItem()
        {
            FolderText = Strings.SidebarGroupAllDownload.GetLocalizedResource(),
            IsExpanded = true,
            DownloadStatus = SidebarDownloadStatusModel.All,
            Icon = new ImageIconSource()
            {
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Folder.png"))
            },
            Children = GetSubItems(SidebarDownloadStatusModel.All)
        },
        new FolderItem()
        {
            FolderText = Strings.SidebarGroupUnFinishedDownload.GetLocalizedResource(),
            DownloadStatus = SidebarDownloadStatusModel.UnFinished,
            Icon = new ImageIconSource()
            {
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Folder.png"))
            },
            Children = GetSubItems(SidebarDownloadStatusModel.UnFinished)
        },
        new FolderItem()
        {
            FolderText = Strings.SidebarGroupFinishedDownload.GetLocalizedResource(),
            DownloadStatus = SidebarDownloadStatusModel.Finished,
            Icon = new ImageIconSource()
            {
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Folder.png"))
            },
            Children = GetSubItems(SidebarDownloadStatusModel.Finished)
        },
    };

    private static ObservableCollection<FolderItem> GetSubItems(SidebarDownloadStatusModel status)
    {
        return new ObservableCollection<FolderItem>
        {
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupAll.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.All,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Folder.png"))
                }
            },
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupArchives.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.Archive,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Archive.png"))
                }
            },
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupDocuments.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.Document,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Documents.png"))
                }
            },
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupApps.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.App,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Apps.png"))
                }
            },
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupMusics.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.Music,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Music.png"))
                }
            },
            new FolderItem()
            {
                FolderText = Strings.SidebarGroupMedia.GetLocalizedResource(),
                DownloadStatus = status,
                DownloadCategory = SidebarDownloadCategoryModel.Media,
                Icon = new ImageIconSource()
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Fluent/Images.png"))
                }
            },
        };
    }
    private async void SidebarView_ItemInvoked(object sender, DevWinUI.ItemInvokedEventArgs e)
    {
        if(MainSidebarView.SelectedItem is FolderItem folderItem)
        {
            _currentFilter = folderItem;

            MainTableView.RefreshFilter();
        }
    }
    #endregion

    #region MenuItem/CommandBar/ContextMenu
    private async void OnMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        switch (element.Tag?.ToString())
        {
            case "Add":
                await ShowAddLinkDialog();
                break;
            case "AddGroup":
                await ShowAddGroupLinkDialog();
                break;
            case "AddClipboard":
                var result = await GetLinksFromClipboardAsync();
                foreach (var item in result)
                {
                    AddGroupDownload(item);
                }
                break;
            case "ImportPDM":
                await ImportFile(isPDM: true);
                break;
            case "ImportFile":
                await ImportFile(isPDM: false);
                break;
            case "ExportPDM":
                await ExportFile(isPDM: true);
                break;
            case "ExportFile":
                await ExportFile(isPDM: false);
                break;
            case "Exit":
                Exit();
                break;
            case "PauseAll":
                PauseAll();
                break;
            case "DeleteFinished":
                DeleteFinished();
                break;
            case "DownloadSchedule":
                OpenWindow<ScheduleWindow>();
                break;
            case "SpeedLimitActive":
                DownloadEngineSettings.MaximumBytesPerSecond = Settings.SpeedLimit;
                break;
            case "SpeedLimitDeActive":
                DownloadEngineSettings.MaximumBytesPerSecond = 0;
                break;
            case "SpeedLimitSettings":
                await ShowSpeedLimitDialog();
                break;
            case "Settings":
                OpenWindow<SettingsWindow>();
                break;
            case "About":
                OpenWindow<AboutWindow>();
                break;
            case "Update":
                await Launcher.LaunchUriAsync(new Uri(Constants.RepoReleaseUrl));
                break;
            case "Github":
                await Launcher.LaunchUriAsync(new Uri(Constants.RepoUrl));
                break;
        }
    }
    private async void OnCommandBarClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        var item = MainTableView.SelectedItem?.As<DownloadItem>();

        var task = downloadTasks.Where(x => x.Item.Id == item?.Id).FirstOrDefault();

        switch (element.Tag?.ToString())
        {
            case "Add":
                await ShowAddLinkDialog();
                break;
            case "Resume":
                task?.ResumeAsync();
                break;

            case "Stop":
                task?.Pause();
                break;
            case "Delete":
                await DeleteSelectedItems();
                break;
            case "DeleteFinished":
                DeleteFinished();
                break;
            case "Settings":
                OpenWindow<SettingsWindow>();
                break;
            case "Schedule":
                OpenWindow<ScheduleWindow>();
                break;
            case "StartQueue":
                // Start queue respecting schedule. Do not open any download windows.
                _ = StartQueueAsync();
                break;
            case "StopQueue":
                StopQueue();
                break;
        }
    }
    private async void OnContextMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        var item = element.DataContext.As<DownloadItem>();

        if (item == null)
            return;

        var task = downloadTasks.Where(x => x.Item.Id == item.Id).FirstOrDefault();

        switch (element.Tag?.ToString())
        {
            case "OpenFile":
                ExecuteFile(item.FilePath);
                break;
            case "OpenFolder":
                await Launcher.LaunchFolderPathAsync(item.FileDirectory);
                break;
            case "ReDownload":
                _ = task?.StartAsync();
                OpenCurrentDownloadWindow(task);
                break;
            case "Resume":
                task?.ResumeAsync();
                OpenCurrentDownloadWindow(task);
                break;
            case "Stop":
                task?.Pause();
                break;
            case "AddQueue":
                AddToQueue(item);
                break;
            case "RemoveQueue":
                RemoveFromQueue(item);
                break;
            case "Delete":
                await DeleteSelectedItems();
                break;
            case "Properties":
                await OpenFileProperties(item);
                break;
        }
    }
    #endregion

    #region CommandBar
    public void StopQueue()
    {
        if (!_queueRunning)
            return;

        // Pause all active downloads immediately before cancelling
        foreach (var task in downloadTasks)
        {
            try
            {
                if (task.Item.Status == DownloadStatus.Running)
                {
                    task.Pause();
                }
            }
            catch { }
        }

        // Then cancel the queue
        _queueCts?.Cancel();
    }
    public async Task StartQueueAsync()
    {
        if (_queueRunning)
            return;

        _queueRunning = true;
        _queueCts = new CancellationTokenSource();
        var cts = _queueCts;

        try
        {
            // Respect schedule start time
            if (AppHelper.Settings.IsScheduleStartTimeEnabled && AppHelper.Settings.ScheduleStartTime.HasValue)
            {
                var startTime = AppHelper.Settings.ScheduleStartTime.Value;
                var todayStart = DateTime.Today + startTime;
                var delay = todayStart - DateTime.Now;
                if (delay > TimeSpan.Zero)
                {
                    try { await Task.Delay(delay, cts.Token); } catch (OperationCanceledException) { return; }
                }
            }

            // Setup stop time cancellation if configured
            if (AppHelper.Settings.IsScheduleStopTimeEnabled && AppHelper.Settings.ScheduleStopTime.HasValue)
            {
                var endTime = AppHelper.Settings.ScheduleStopTime.Value;
                var todayEnd = DateTime.Today + endTime;
                var stopDelay = todayEnd - DateTime.Now;
                if (stopDelay > TimeSpan.Zero)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(stopDelay, cts.Token);
                            cts.Cancel();
                        }
                        catch { }
                    });
                }
            }

            var active = new List<(DownloadTask Task, Task RunTask, DownloadItem Item)>();
            var pending = new List<Guid>();

            while ((!pending.Any() || active.Count > 0) && !cts.IsCancellationRequested)
            {
                // Rebuild ordered list from settings so reorders are respected while running
                var order = AppHelper.QueueSettings.QueueOrder ?? new List<Guid>();
                if (order.Count == 0)
                {
                    order = ViewModel.Downloads.Where(x => x.IsQueue).Select(x => x.Id).ToList();
                }

                // Compose pending: follow the saved order, then append any previously pending items that are not in saved order (e.g. retries)
                var orderedPending = order.Where(id => ViewModel.Downloads.Any(x => x.Id == id && x.IsQueue)).ToList();
                var extraPending = pending.Where(id => !orderedPending.Contains(id) && ViewModel.Downloads.Any(x => x.Id == id && x.IsQueue)).ToList();
                pending = orderedPending.Concat(extraPending).Where(id => !active.Any(a => a.Item.Id == id)).ToList();

                // Get current concurrent limit (respects dynamic changes)
                int concurrentLimit = Math.Max(1, AppHelper.Settings.ScheduleConcurrentCount);

                // Pause excess tasks if concurrency was reduced
                if (active.Count > concurrentLimit)
                {
                    var toRemove = active.Count - concurrentLimit;
                    var tasksToStop = active.Skip(concurrentLimit).Take(toRemove).ToList();
                    foreach (var item in tasksToStop)
                    {
                        try { item.Task.Pause(); } catch { }
                        // Re-add the item to pending to retry later
                        if (!pending.Contains(item.Item.Id))
                        {
                            pending.Add(item.Item.Id);
                        }
                        active.Remove(item);
                    }
                }

                // Start new tasks up to concurrent limit (dynamic)
                while (!cts.IsCancellationRequested && active.Count < concurrentLimit && pending.Count > 0)
                {
                    var id = pending[0];
                    pending.RemoveAt(0);
                    var item = ViewModel.Downloads.FirstOrDefault(x => x.Id == id && x.IsQueue);
                    if (item == null)
                        continue;

                    var task = downloadTasks.FirstOrDefault(x => x.Item.Id == item.Id);
                    if (task == null)
                    {
                        task = new DownloadTask(item);
                        task.StatusChanged += (s, e) => DispatcherQueue.TryEnqueue(() => ViewModel.NotifyPropertiesChanged());
                        downloadTasks.Add(task);
                    }

                    // Start but don't await here
                    var run = task.StartAsync().WaitAsync(cts.Token);
                    active.Add((task, run, item));
                }

                if (active.Count == 0)
                    break;

                // Wait for any to complete or cancellation
                try
                {
                    await Task.WhenAny(active.Select(a => a.RunTask));
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // Handle all completed tasks
                var completed = active.Where(a => a.RunTask.IsCompleted).ToList();
                foreach (var comp in completed)
                {
                    try
                    {
                        await comp.RunTask; // observe exceptions
                    }
                    catch
                    {
                        // ignore per-item errors
                    }

                    // Decide next action based on status
                    var status = comp.Item.Status;
                    if (status == DownloadStatus.Completed)
                    {
                        // Completed: remove from queue
                        comp.Item.IsQueue = false;
                    }
                    else if (status == DownloadStatus.Failed || status == DownloadStatus.Paused || status == DownloadStatus.Stopped)
                    {
                        // Failed or paused/stopped by user: move to end of queue so it will be retried later
                        comp.Item.IsQueue = true; // ensure it's still queued
                        pending.Add(comp.Item.Id);
                    }
                    else
                    {
                        // Other statuses: keep queued
                        comp.Item.IsQueue = true;
                        pending.Add(comp.Item.Id);
                    }

                    AppHelper.UpdateQueueOrder(ViewModel.Downloads);

                    active.Remove(comp);
                }

                // If items were moved to pending at the end, ensure index skips already-started ids
                // (pending contains full list; index points to next id to start)
            }

            // If we were cancelled, pause any active downloads and keep them queued
            if (cts.IsCancellationRequested && active.Count > 0)
            {
                foreach (var a in active)
                {
                    try { a.Task.Pause(); } catch { }
                }
            }

            // If queue finished naturally (not canceled by schedule/user), apply post actions
            if (!_queueCts.IsCancellationRequested)
            {
                if (AppHelper.Settings.IsScheduleClosePDMEnabled)
                {
                    Exit();
                }
                else if (AppHelper.Settings.IsScheduleShutdownEnabled)
                {
                    try
                    {
                        var args = AppHelper.Settings.IsScheduleForceCloseAppEnabled ? "/s /t 0 /f" : "/s /t 0";
                        Process.Start(new ProcessStartInfo("shutdown", args) { CreateNoWindow = true, UseShellExecute = false });
                    }
                    catch { }
                }
            }
        }
        finally
        {
            _queueRunning = false;
            _queueCts?.Dispose();
            _queueCts = null;
        }
    }

    #endregion

    #region ContextMenu
    private async Task OpenFileProperties(DownloadItem item)
    {
        var dialog = new PropertiesDialog();
        dialog.Item = item;
        await dialog.ShowDialogAsync();
    }
    private void OpenCurrentDownloadWindow(DownloadTask task)
    {
        OpenWindow(
            key: (typeof(CurrentDownloadWindow), task.Item.Id),
            factory: () => new CurrentDownloadWindow
            {
                CurrentTask = task
            });
    }
    private void RemoveFromQueue(DownloadItem item)
    {
        item.IsQueue = false;
    }

    private void AddToQueue(DownloadItem item)
    {
        item.IsQueue = true;
    }
    #endregion

    #region MenuItem
    private void PauseAll()
    {
        foreach (var item in ViewModel.Downloads)
        {
            if (item.Status == Downloader.DownloadStatus.Running)
            {
                var task = downloadTasks.Where(x => x.Item.Id == item.Id).FirstOrDefault();
                task?.Pause();
            }
        }
        ViewModel.NotifyPropertiesChanged();
    }
    private async Task ShowSpeedLimitDialog()
    {
        var dialog = new SpeedLimitDialog();
        await dialog.ShowDialogAsync();
    }
    private async Task ImportFile(bool isPDM)
    {
        var picker = new FileOpenPicker(App.MainWindow.AppWindow.Id);

        if (isPDM)
        {
            picker.FileTypeChoices.Add("PDM File", new string[] { ".pdm" });
        }
        else
        {
            picker.FileTypeChoices.Add("Text File", new string[] { ".txt" });
        }

        var result = await picker.PickMultipleFilesAsync();
        foreach (var item in result)
        {
            foreach (string line in File.ReadLines(item.Path))
            {
                string url = line.Trim();
                if (string.IsNullOrWhiteSpace(url) || !IsValidLink(url))
                    continue;

                AddGroupDownload(url);
            }
        }
    }
    private async Task ExportFile(bool isPDM)
    {
        var picker = new FileSavePicker(App.MainWindow.AppWindow.Id);

        if (isPDM)
        {
            picker.FileTypeChoices.Add("PDM File", new string[] { ".pdm" });
        }
        else
        {
            picker.FileTypeChoices.Add("Text File", new string[] { ".txt" });
        }

        var result = await picker.PickSaveFileAsync();
        if (result != null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DownloadItem item in MainTableView.Items)
            {
                stringBuilder.AppendLine(item.Url.ToString());
            }

            File.WriteAllText(result.Path, stringBuilder.ToString());
        }
    }
    public static async Task<List<string>> GetLinksFromClipboardAsync()
    {
        var result = new List<string>();

        var dataPackageView = Clipboard.GetContent();

        if (!dataPackageView.Contains(StandardDataFormats.Text))
            return result;

        string text = await dataPackageView.GetTextAsync();

        var links = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

        foreach (var link in links)
        {
            if (!IsValidLink(link))
                continue;

            result.Add(link);
        }

        return result.Distinct().ToList();
    }
    private void AddGroupDownload(string url)
    {
        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "Unknown";

        var availableFileName = GetAvailableFileName(DownloadEngineSettings.DownloadFolderPath, fileName, ViewModel.Downloads);

        var item = new DownloadItem
        {
            Id = Guid.NewGuid(),
            AddedDate = DateTime.Now.ToShortDateString(),
            FilePath = availableFileName.FilePath,
            TotalBytes = 0,
            Url = new Uri(url),
            IsQueue = true,
            Status = DownloadStatus.None
        };

        var task = new DownloadTask(item);
        task.StatusChanged += (s, e) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.NotifyPropertiesChanged();
            });
        };
        downloadTasks.Add(task);
        ViewModel.Downloads.Add(item);
    }
    private async Task ShowAddGroupLinkDialog()
    {
        var dialog = new AddGroupLinkDialog();
        await dialog.ShowDialogAsync();

        if (dialog.Result == ContentDialogResult.Primary)
        {
            foreach (var item in dialog.Urls)
            {
                AddGroupDownload(item);
            }
        }
    }
    #endregion

    private async void Exit()
    {
        canCloseApp = true;

        foreach (var window in windows?.Values?.ToList())
        {
            window?.Close();
        }

        windows?.Clear();

        foreach (var item in downloadTasks)
        {
            item?.Dispose();
        }

        Close();
    }

    internal void OpenWindow<T>() where T : Window, new()
    {
        if (windows.TryGetValue(typeof(T), out var window))
        {
            window.Activate();
            return;
        }

        window = new T();

        WindowHelper.TrackWindow(window);

        windows[typeof(T)] = window;

        window.Closed += (_, _) => windows.Remove(typeof(T));

        window.Activate();
    }
    internal void OpenWindow<T>(object key, Func<T> factory) where T : Window
    {
        if (windows.TryGetValue(key, out var window))
        {
            window.Activate();
            return;
        }

        var newWindow = factory();

        WindowHelper.TrackWindow(newWindow);

        windows[key] = newWindow;

        newWindow.Closed += (_, _) => windows.Remove(key);

        newWindow.Activate();
    }

    private async Task ShowAddLinkDialog()
    {
        var dialog = new AddLinkDialog();
        await dialog.ShowDialogAsync();

        if (dialog.Result == ContentDialogResult.Primary)
        {
            OpenDownloadWindow(dialog.NewLink);
        }
    }
    private void OpenDownloadWindow(string url)
    {
        var window = new DownloadWindow();
        window.Url = url;
        WindowHelper.TrackWindow(window);
        window.Activate();
    }

    public void AddSingleDownload(DownloadItem item, bool startImmediately)
    {
        var task = new DownloadTask(item);

        task.StatusChanged += (s, e) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.NotifyPropertiesChanged();
            });
        };
        downloadTasks.Add(task);
        ViewModel.Downloads.Add(item);

        if (startImmediately)
            _ = task.StartAsync();
    }
    private async Task DeleteSelectedItems()
    {
        var result = await MessageBox.ShowWarningAsync(Strings.DeleteConfirmationDialog_Message.GetLocalizedResource(), Strings.DeleteConfirmationDialog_Title.GetLocalizedResource(), MessageBoxButtons.YesNo);
        if (result != MessageBoxResult.Yes)
            return;

        var selectedItems = MainTableView.SelectedItems
            .OfType<DownloadItem>()
            .ToList();

        foreach (var itemForDelete in selectedItems)
        {
            var taskForDelete = downloadTasks.FirstOrDefault(x => x.Item.Id == itemForDelete.Id);
            taskForDelete?.Pause();
            taskForDelete?.Dispose();

            if (taskForDelete is not null)
                downloadTasks.Remove(taskForDelete);

            ViewModel.Downloads.Remove(itemForDelete);
        }
    }
    private void DeleteFinished()
    {
        var itemsToRemove = ViewModel.Downloads
            .Where(x => x.Status == Downloader.DownloadStatus.Completed)
            .ToList();

        foreach (var downloadedItem in itemsToRemove)
        {
            var downloadTask = downloadTasks.FirstOrDefault(x => x.Item.Id == downloadedItem.Id);
            downloadTask?.Pause();
            downloadTask?.Dispose();

            if (downloadTask is not null)
                downloadTasks.Remove(downloadTask);

            ViewModel.Downloads.Remove(downloadedItem);
        }
    }
}
