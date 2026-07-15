namespace PersianDownloadManager.Views;

public sealed partial class AppUpdateSettingPage : Page
{
    public AppUpdateSettingViewModel ViewModel { get; }
    public AppUpdateSettingPage()
    {
        ViewModel = App.GetService<AppUpdateSettingViewModel>();
        InitializeComponent();
        SettingsWindow.Instance.GetBreadcrumbNavigator().AddNewItem(typeof(AppUpdateSettingPage));
    }
}
