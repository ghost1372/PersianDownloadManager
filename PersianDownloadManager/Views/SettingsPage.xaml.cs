namespace PersianDownloadManager.Views;
public sealed partial class SettingsPage : Page
{
    public NavigationParameterExtension GeneralParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(GeneralSettingPage),
        BreadCrumbHeader = Strings.SettingsPageGeneral_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension DownloadParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(DownloadSettingPage),
        BreadCrumbHeader = Strings.SettingsPageDownload_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension ThemeParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(ThemeSettingPage),
        BreadCrumbHeader = Strings.SettingsPageTheme_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension UpdateParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(AppUpdateSettingPage),
        BreadCrumbHeader = Strings.SettingsPageUpdate_Header.GetLocalizedResource()
    };
    public NavigationParameterExtension AboutParam { get; set; } = new NavigationParameterExtension
    {
        PageType = typeof(AboutUsSettingPage),
        BreadCrumbHeader = Strings.SettingsPageAbout_Header.GetLocalizedResource()
    };

    public SettingsPage()
    {
        this.InitializeComponent();     
    }
}
