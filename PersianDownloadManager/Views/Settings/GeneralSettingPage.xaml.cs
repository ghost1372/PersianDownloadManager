using WinRT;

namespace PersianDownloadManager.Views;

public sealed partial class GeneralSettingPage : Page
{
    public GeneralSettingViewModel ViewModel { get; }
    public GeneralSettingPage()
    {
        ViewModel = App.GetService<GeneralSettingViewModel>();
        InitializeComponent();
        SettingsWindow.Instance.GetBreadcrumbNavigator().AddNewItem(typeof(GeneralSettingPage));
        Loaded += GeneralSettingPage_Loaded;
    }

    private void GeneralSettingPage_Loaded(object sender, RoutedEventArgs e)
    {
        var uiFont = CmbUIFont.Items.OfType<FontOption>().Where(x => x.FontKey == Settings.UIFont.FontKey).FirstOrDefault();
        CmbUIFont.SelectedItem = uiFont;
    }

    private async void NavigateToLogPath_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = (sender as HyperlinkButton).Content.ToString();
        if (Directory.Exists(folderPath))
        {
            Windows.Storage.StorageFolder folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folderPath);
            await Windows.System.Launcher.LaunchFolderAsync(folder);
        }
    }

    private async void OnUIFontSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbUIFont.SelectedItem is FontOption font)
        {
            if (font.FontKey == Settings.UIFont.FontKey)
                return;

            Settings.UIFont = font;
            if (AppLanguageHelper.PreferredLanguage.Code == "fa")
            {
                FontHelper.SetUIFontFamily(font);

                ViewModel.ShowRestartForFont = true;
            }
        }
    }

    private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = CmbLanguage.SelectedItem.As<AppLanguageItem>();
        FontCard.IsEnabled = item.Code == "fa";
    }
}
