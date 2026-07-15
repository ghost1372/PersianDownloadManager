using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Windows.AppLifecycle;

namespace PersianDownloadManager.ViewModels;

public partial class GeneralSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int SelectedAppLanguageIndex { get; set; } = AppLanguageHelper.SupportedLanguages.IndexOf(AppLanguageHelper.PreferredLanguage);
    partial void OnSelectedAppLanguageIndexChanged(int value)
    {
        if (AppLanguageHelper.TryChange(value))
        {
            ShowRestartForLanguage = true;
        }
    }

    [ObservableProperty]
    public partial bool ShowRestartForLanguage { get; set; }

    [ObservableProperty]
    public partial bool ShowRestartForFont { get; set; }

    public GeneralSettingViewModel()
    {
        SelectedAppLanguageIndex = AppLanguageHelper.SupportedLanguages.IndexOf(AppLanguageHelper.PreferredLanguage);
    }

    [RelayCommand]
    public void OnRestart()
    {
        AppInstance.Restart(null);
    }

    [RelayCommand]
    public void OnCancelRestart()
    {
        ShowRestartForLanguage = false;
        ShowRestartForFont = false;
    }
}
