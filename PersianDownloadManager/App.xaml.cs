using Microsoft.UI.Windowing;
using Microsoft.Windows.AppLifecycle;
using WinRT;

namespace PersianDownloadManager;
public partial class App : Application
{
    public static Window MainWindow = Window.Current;
    public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }
    public IThemeService ThemeService => GetService<IThemeService>();
    internal static SystemTrayIcon TrayIcon { get; set; }
    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }
    public App()
    {
        Services = ConfigureServices();
        InitializeComponent();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();

        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<DownloadSettingViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ScheduleWindowViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();

        this.ThemeService.Initialize(MainWindow);

#if DEBUG
        ProcessInfoHelper.IsDebug = true;
#else
        ProcessInfoHelper.IsDebug = false;
#endif
        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");
        MainWindow.AppWindow.SetTaskbarIcon("Assets/AppIcon.ico");

        if (AppLanguageHelper.PreferredLanguage.Code == "fa")
        {
            if (Settings.UIFont == null)
            {
                FontHelper.SetUIFontFamily(Constants.DefaultUIFont);
                Settings.UIFont = Constants.DefaultUIFont;
            }
            else
            {
                FontHelper.SetUIFontFamily(Settings.UIFont);
            }

            Application.Current.Resources.MergedDictionaries.AddIfNotExists(new ResourceDictionary() { Source = new Uri("ms-appx:///Themes/PersianFontStyle.xaml") });
        }

        SingleInstanceWindowActivator.Register(MainWindow);

        AddTrayIcon(Strings.TrayIconTooltip.GetLocalizedResource());

        AppActivationArguments appActivationArguments = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

        if (appActivationArguments.Kind is ExtendedActivationKind.StartupTask)
        {
            if (MainWindow.AppWindow.Presenter is OverlappedPresenter presenter)
            {
                MainWindow.AppWindow.Hide();
            }
        }
        else
        {
            MainWindow.Activate();
        }

        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }

    #region TrayIcon
    private void AddTrayIcon(string toolTip)
    {
        uint iconId = 7777;
        if (TrayIcon is null)
        {
            var icon = WindowHelper.GetWindowIcon(MainWindow);
            TrayIcon = new SystemTrayIcon(iconId, icon, toolTip);
            TrayIcon.LeftClick += OnTrayIconLeftClick;
            TrayIcon.RightClick += OnTrayIconRightClick;
        }
        TrayIcon.IsVisible = true;
    }
    
    public void RemoveTrayIcon()
    {
        if (TrayIcon is not null)
        {
            TrayIcon.LeftClick -= OnTrayIconLeftClick;
            TrayIcon.RightClick -= OnTrayIconRightClick;
            TrayIcon.IsVisible = false;
            TrayIcon.Dispose();
            TrayIcon = null;
        }
    }
  
    private void OnTrayIconRightClick(SystemTrayIcon sender, SystemTrayIconEventArgs args)
    {
        var flyout = new MenuFlyout();
        var openItem = new MenuFlyoutItem
        {
            Text = Strings.TrayIconOpenItem.GetLocalizedResource()
        };

        openItem.Click += (s, e) => MainWindow.Activate();

        var aboutItem = new MenuFlyoutItem
        {
            Text = Strings.TrayIconAboutItem.GetLocalizedResource()
        };
        aboutItem.Click += (s, e) => PersianDownloadManager.MainWindow.Instance.OpenWindow<AboutWindow>();

        var exitItem = new MenuFlyoutItem
        {
            Text = Strings.TrayIconExitItem.GetLocalizedResource()
        };
        exitItem.Click += (s, e) => Exit();

        flyout.Items.Add(openItem);
        flyout.Items.Add(aboutItem);
        flyout.Items.Add(new MenuFlyoutSeparator());
        flyout.Items.Add(exitItem);
        args.Flyout = flyout;
    }

    private void OnTrayIconLeftClick(SystemTrayIcon sender, SystemTrayIconEventArgs args)
    {
        MainWindow.AppWindow.Presenter.As<OverlappedPresenter>().Restore();
        WindowHelper.SetForegroundWindow(MainWindow);
    }

    #endregion
}
