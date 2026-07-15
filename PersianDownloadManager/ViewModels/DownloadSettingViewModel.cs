using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using System.Collections.ObjectModel;
using Windows.System;

namespace PersianDownloadManager.ViewModels;

public partial class DownloadSettingViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string DownloadFolderPath { get; set; } = DownloadEngineSettings.DownloadFolderPath;
    
    [ObservableProperty]
    public partial double MinimumSizeOfChunking { get; set; } = DownloadEngineSettings.MinimumSizeOfChunking;

    partial void OnMinimumSizeOfChunkingChanged(double value)
    {
        DownloadEngineSettings.MinimumSizeOfChunking = (long)value;
    }
    
    [ObservableProperty]
    public partial double MinimumChunkSize { get; set; } = DownloadEngineSettings.MinimumChunkSize;

    partial void OnMinimumChunkSizeChanged(double value)
    {
        DownloadEngineSettings.MinimumChunkSize = (long)value;
    }
   
    [ObservableProperty]
    public partial double MaximumBytesPerSecond { get; set; } = Settings.SpeedLimit;

    partial void OnMaximumBytesPerSecondChanged(double value)
    {
        Settings.SpeedLimit = (long)value;
    }

    [ObservableProperty]
    public partial double MaximumMemoryBufferBytes { get; set; } = DownloadEngineSettings.MaximumMemoryBufferBytes;

    partial void OnMaximumMemoryBufferBytesChanged(double value)
    {
        DownloadEngineSettings.MaximumMemoryBufferBytes = (long)value;
    }

    [ObservableProperty]
    public partial string DownloadFileExtension { get; set; } = DownloadEngineSettings.DownloadFileExtension;

    partial void OnDownloadFileExtensionChanged(string value)
    {
        if(string.IsNullOrEmpty(value) || !value.StartsWith("."))
            return;
        
        DownloadEngineSettings.DownloadFileExtension = value;
    }

    [ObservableProperty]
    public partial ObservableCollection<FileExistPolicy> FileExistPolicy { get; set; } = new ObservableCollection<FileExistPolicy>(Enum.GetValues<FileExistPolicy>());
    
    [ObservableProperty]
    public partial int FileExistPolicySelectedIndex { get; set; } = (int)DownloadEngineSettings.FileExistPolicy;
    partial void OnFileExistPolicySelectedIndexChanged(int value)
    {
        DownloadEngineSettings.FileExistPolicy = (FileExistPolicy)value;
    }

    [RelayCommand]
    public async Task ChooseFolderAsync()
    {
        var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(App.MainWindow.AppWindow.Id);
        var result = await picker.PickSingleFolderAsync();
        if (result is not null)
        {
            DownloadEngineSettings.DownloadFolderPath = result.Path;
            DownloadFolderPath = result.Path;
        }
    }

    [RelayCommand]
    public async Task GoToFolderInExplorerAsync()
    {
        await Launcher.LaunchUriAsync(new Uri(DownloadEngineSettings.DownloadFolderPath));
    }
    
    [RelayCommand]
    public async Task ResetSettings()
    {
        var result = await MessageBox.ShowWarningAsync(Strings.DownloadSettingPage_ResetConfirmDialog_Message.GetLocalizedResource(), Strings.DownloadSettingPage_ResetConfirmDialog_Title.GetLocalizedResource(), MessageBoxButtons.YesNo);
        if (result == MessageBoxResult.Yes)
        {
            ResetDownloadEngineSettingsToDefault();

            await MessageBox.ShowSuccessAsync(Strings.DownloadSettingPage_ResetSuccessDialog_Message.GetLocalizedResource(), Strings.DownloadSettingPage_ResetSuccessDialog_Title.GetLocalizedResource(), MessageBoxButtons.OK);
        }
    }
}
