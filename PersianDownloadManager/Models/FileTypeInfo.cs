using Microsoft.UI.Xaml.Media;

namespace PersianDownloadManager.Models;

public sealed partial class FileTypeInfo
{
    public required ImageSource Icon { get; init; }
    public required string TypeName { get; init; }
    public required SidebarDownloadCategoryModel Category { get; init; }
}