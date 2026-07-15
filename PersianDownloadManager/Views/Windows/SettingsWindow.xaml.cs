namespace PersianDownloadManager.Views;

public sealed partial class SettingsWindow : Window
{
    public static Dictionary<Type, BreadcrumbPageConfig> pageDictionary = new()
    {
        {typeof(PersianDownloadManager.Views.SettingsPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageHeader.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(PersianDownloadManager.Views.AboutUsSettingPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageAbout_Header.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(PersianDownloadManager.Views.AppUpdateSettingPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageUpdate_Header.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(PersianDownloadManager.Views.GeneralSettingPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageGeneral_Header.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(PersianDownloadManager.Views.ThemeSettingPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageTheme_Header.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
        {typeof(PersianDownloadManager.Views.DownloadSettingPage), new BreadcrumbPageConfig { PageTitle = Strings.SettingsPageDownload_Header.GetLocalizedResource(), IsHeaderVisible = true, ClearNavigation = false}},
    };

    public IDelegateCommand NavigateToCommand { get; }
    internal static SettingsWindow Instance { get; private set; }
    public SettingsWindow()
    {
        InitializeComponent();
        Instance = this;
        
        MainFrame.Navigate(typeof(SettingsPage));

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

        Title = AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        SetWindowIcon(this);

        NavigateToCommand = DelegateCommand.Create(OnNavigateToCommand);

        BreadCrumbNav.Initialize(MainFrame, pageDictionary);

        BreadCrumbNav.AddNewItem(typeof(SettingsPage));

        WindowHelper.CenterOnScreen(this);
    }

    public BreadcrumbNavigator GetBreadcrumbNavigator()
    {
        return BreadCrumbNav;
    }

    private void OnNavigateToCommand(object? parameter)
    {
        if (parameter is NavigationParameterExtension navigationParameter)
        {
            MainFrame.Navigate(navigationParameter.PageType, navigationParameter.BreadCrumbHeader, navigationParameter.NavigationTransitionInfo);
        }
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (MainFrame.CanGoBack)
        {
            MainFrame.GoBack();
        }
    }
}
