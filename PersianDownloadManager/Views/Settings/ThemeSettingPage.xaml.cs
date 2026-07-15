namespace PersianDownloadManager.Views;

public sealed partial class ThemeSettingPage : Page
{
    public ThemeSettingPage()
    {
        InitializeComponent();
        SettingsWindow.Instance.GetBreadcrumbNavigator().AddNewItem(typeof(ThemeSettingPage));
    }
}
