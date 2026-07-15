namespace PersianDownloadManager.Views;
public sealed partial class DownloadSettingPage : Page
{
    public DownloadSettingViewModel ViewModel { get; }
    public DownloadSettingPage()
    {
        ViewModel = App.GetService<DownloadSettingViewModel>();
        InitializeComponent();
        SettingsWindow.Instance.GetBreadcrumbNavigator().AddNewItem(typeof(DownloadSettingPage));
    }

    private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (args.NewValue == 0)
        {
            NBLimit.Header = Strings.SpeedLimitDialog_NoLimit.GetLocalizedResource();
            return;
        }

        NBLimit.Header = $"{FileHelper.GetFileSize((long)args.NewValue)}/s";
    }
}
