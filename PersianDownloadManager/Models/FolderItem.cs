using CommunityToolkit.Mvvm.ComponentModel;

namespace PersianDownloadManager.Models;

public sealed partial class FolderItem : ObservableObject, DevWinUI.ISidebarItemModel
{
    [ObservableProperty]
    public partial string FolderText { get; set; } = "";

    [ObservableProperty]
    public partial string Path { get; set; } = "";

    [ObservableProperty]
    public partial SidebarDownloadStatusModel DownloadStatus { get; set; }

    [ObservableProperty]
    public partial SidebarDownloadCategoryModel DownloadCategory { get; set; }

    [ObservableProperty]
    public partial ImageIconSource Icon { get; set; } = new ImageIconSource();

    public object? Children { get; set; } = null;

    public IconSource? IconSource => Icon;

    public bool IsExpanded { get; set; } = false;

    public object ToolTip => Path;

    public bool PaddedItem => false;

    public string Text => FolderText;

    partial void OnIconChanged(ImageIconSource value)
    {
        OnPropertyChanged(nameof(IconSource));
    }
}
