namespace PersianDownloadManager.Views;

public sealed partial class PropertiesDialog : ContentDialogWindow
{
    public DownloadItem Item { get; set; }
    public PropertiesDialog()
    {
        InitializeComponent();

        Owner = App.MainWindow;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Item != null)
        {
            switch (Item.Status)
            {
                case Downloader.DownloadStatus.None:
                case Downloader.DownloadStatus.Created:
                case Downloader.DownloadStatus.Running:
                case Downloader.DownloadStatus.Stopped:
                case Downloader.DownloadStatus.Paused:
                case Downloader.DownloadStatus.Failed:
                    StatusInfoBar.Severity = InfoBarSeverity.Informational;
                    break;
                case Downloader.DownloadStatus.Completed:
                    StatusInfoBar.Severity = InfoBarSeverity.Success;
                    break;
            }
        }
    }

    private void ContentDialogWindow_PrimaryButtonClick(object sender, EventArgs e)
    {
        TryClose();
    }
    private void ContentDialogWindow_SecondaryButtonClick(object sender, EventArgs e)
    {
        if (Item != null && File.Exists(Item.FilePath))
        {
            ExecuteFile(Item.FilePath);

            TryClose();
        }
    }
}
