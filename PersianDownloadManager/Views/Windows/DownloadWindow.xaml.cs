using Downloader;
using Microsoft.UI.Windowing;
using Microsoft.Windows.Storage.Pickers;
using WinRT;

namespace PersianDownloadManager.Views;

public sealed partial class DownloadWindow : Window
{
    public string Url { get; set; }
    private DownloadItem? downloadItem;
    public string WindowTitle = "Download File Info";
    public DownloadWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        SetWindowIcon(this);
        Title = AppWindow.Title = WindowTitle;

        var presenter = AppWindow.Presenter.As<OverlappedPresenter>();
        presenter.IsMaximizable = false;
        presenter.IsResizable = false;

        presenter.PreferredMaximumHeight = 330;
        presenter.PreferredMinimumHeight = 330;

        presenter.PreferredMaximumWidth = 720;
        presenter.PreferredMinimumWidth = 720;
        
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
        WindowHelper.CenterOnScreen(this);
    }

    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(Url))
            return;

        using var service = new DownloadService();

        var info = await GetFileInfo(service, Url, downloadItem);

        downloadItem = new DownloadItem
        {
            Id = Guid.NewGuid(),
            Url = new Uri(Url),
            FilePath = info.AvailableFileName.FilePath,
            TotalBytes = info.RemoteFileInfo?.FileSize ?? 0,
            AddedDate = DateTime.Now.ToShortDateString(),
            IsQueue = true,
            IsResumeCapabilitySupported = info.RemoteFileInfo.SupportsRange,
            Status = DownloadStatus.None
        };

        TxtFileSize.Text = downloadItem.Size;
        TxtSaveAs.Text = downloadItem.FilePath;
        ImgIcon.Source = downloadItem.Icon;
    }

    private void OnDownloadLater(object sender, RoutedEventArgs e)
    {
        if (downloadItem == null)
            return;

        MainWindow.Instance.AddSingleDownload(downloadItem, false);

        Close();
    }
    private void OnStartDownload(object sender, RoutedEventArgs e)
    {
        if (downloadItem == null)
            return;

        MainWindow.Instance.AddSingleDownload(downloadItem, true);

        var currentDownload = new CurrentDownloadWindow();
        currentDownload.CurrentTask = MainWindow.Instance.downloadTasks.Where(x => x.Item.Id == downloadItem.Id).FirstOrDefault();
        WindowHelper.TrackWindow(currentDownload);
        currentDownload.Activate();

        Close();

    }
    private void OnCancelDownload(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BtnSaveFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker(App.MainWindow.AppWindow.Id);
        picker.FileTypeChoices.Add(downloadItem?.FileInfo, new string[] { Path.GetExtension(downloadItem?.FileName) });
        var result = await picker.PickSaveFileAsync();
        if (result != null)
        {
            downloadItem.FilePath = result.Path;
            TxtSaveAs.Text = downloadItem.FilePath;
        }
    }
}
