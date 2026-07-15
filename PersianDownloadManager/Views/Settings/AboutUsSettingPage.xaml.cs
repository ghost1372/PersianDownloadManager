namespace PersianDownloadManager.Views;

public sealed partial class AboutUsSettingPage : Page
{
    public AboutUsSettingPage()
    {
        InitializeComponent();
        SettingsWindow.Instance.GetBreadcrumbNavigator().AddNewItem(typeof(AboutUsSettingPage));
    }
}
