using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Concurrent;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace PersianDownloadManager.Common;

public static partial class FileIconProvider
{
    private static BitmapImage ArchiveIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Archive.png");
    private static BitmapImage VideoIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Video.png");
    private static BitmapImage AudioIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Audio.png");
    private static BitmapImage ImageIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Image.png");
    private static BitmapImage ApplicationIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/App.png");
    private static BitmapImage DocumentIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Document.png");
    private static BitmapImage FileIcon = CreateAsset("ms-appx:///Assets/Fluent/Extensions/File.png");

    private static readonly Dictionary<string, BitmapImage> SpecificIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        [".rar"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Rar.png"),
        [".msix"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Package.png"),
        [".appx"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Package.png"),
        [".appxbundle"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Package.png"),
        [".iso"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Iso.png"),
        [".bin"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Document.png"),
        [".psd"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/PSD.png"),
        [".txt"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Text.png"),
        [".mdb"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Access.png"),
        [".xls"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Excel.png"),
        [".xlsx"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Excel.png"),
        [".doc"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Word.png"),
        [".docx"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Word.png"),
        [".ppt"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/PowerPoint.png"),
        [".pptx"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/PowerPoint.png"),
        [".pdf"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/PDF.png"),
        [".apk"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/APK.png"),
        [".json"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/JSON.png"),
        [".xml"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/XML.png"),
        [".md"] = CreateAsset("ms-appx:///Assets/Fluent/Extensions/Markdown.png"),
    };

    private sealed record FileGroup(BitmapImage Icon, SidebarDownloadCategoryModel Category, string[] Extensions);
    private static readonly FileGroup[] Groups =
    [
        new(ArchiveIcon, SidebarDownloadCategoryModel.Archive,
            [".zip", ".7z", ".tar", ".gz", ".r0", ".r1", ".arj", ".sit", ".sitx", ".sea", ".ace", ".bz2"]),

        new(VideoIcon, SidebarDownloadCategoryModel.Media,
            [".mp4", ".webm", ".mkv", ".mov", ".m4v", ".ts", ".m2ts", ".3gp", ".avi", ".flv", ".ogv", ".wmv"]),

        new(AudioIcon, SidebarDownloadCategoryModel.Music,
            [".mp3", ".m4a", ".aac", ".wav", ".flac", ".ogg", ".opus", ".wma", ".aiff", ".alac", ".amr"]),

        new(ImageIcon, SidebarDownloadCategoryModel.Media,
            [".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif", ".svg", ".bmp", ".ico", ".apng", ".jxl", ".heic", ".heif", ".tif", ".tiff"]),

        new(ApplicationIcon, SidebarDownloadCategoryModel.App,
            [".exe", ".msi", ".aab", ".xapk", ".ipa", ".dmg", ".pkg", ".deb", ".rpm", ".appimage", ".snap", ".flatpak", ".jar"]),

        new(DocumentIcon, SidebarDownloadCategoryModel.Document,
            [".csv", ".rtf", ".odt", ".ods", ".odp", ".epub"])
    ];


    private static readonly ConcurrentDictionary<string, FileTypeInfo> ShellCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, FileTypeInfo> BuiltInCache = new(StringComparer.OrdinalIgnoreCase);
    private static SidebarDownloadCategoryModel GetCategory(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return SidebarDownloadCategoryModel.Unknown;

        foreach (var group in Groups)
        {
            if (group.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return group.Category;
        }

        return SidebarDownloadCategoryModel.Unknown;
    }
    public static FileTypeInfo GetFileTypeInfo(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (Settings.UseWindowsShellIcons)
            return GetShellFileTypeInfo(extension);

        return GetBuiltInFileTypeInfo(extension);
    }

    private static FileTypeInfo GetBuiltInFileTypeInfo(string extension)
    {
        return BuiltInCache.GetOrAdd(extension, CreateBuiltInFileTypeInfo);
    }

    private static FileTypeInfo CreateBuiltInFileTypeInfo(string extension)
    {
        return new FileTypeInfo
        {
            Icon = GetBuiltInIcon(extension),
            TypeName = IconHelper.GetFileTypeName(extension),
            Category = GetCategory(extension)
        };
    }
    private static ImageSource GetBuiltInIcon(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return FileIcon;

        if (SpecificIcons.TryGetValue(extension, out var icon))
            return icon;

        foreach (var group in Groups)
        {
            if (group.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return group.Icon;
        }

        return FileIcon;
    }
    private static FileTypeInfo GetShellFileTypeInfo(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return new FileTypeInfo
            {
                Icon = FileIcon,
                Category = SidebarDownloadCategoryModel.Unknown,
                TypeName = string.Empty
            };
        }

        return ShellCache.GetOrAdd(extension, CreateShellFileTypeInfo);
    }
    private unsafe static FileTypeInfo CreateShellFileTypeInfo(string extension)
    {
        var file = IconHelper.GetFileInfo(extension);

        ImageSource icon = FileIcon;

        if (file.hIcon != IntPtr.Zero)
        {
            try
            {
                ICONINFO iconInfo;
                PInvoke.GetIconInfo(file.hIcon, &iconInfo);

                try
                {
                    icon = IconHelper.GetCaptureWriteableBitmap(iconInfo.hbmColor);
                }
                finally
                {
                    if (iconInfo.hbmColor != IntPtr.Zero)
                        PInvoke.DeleteObject(iconInfo.hbmColor);

                    if (iconInfo.hbmMask != IntPtr.Zero)
                        PInvoke.DeleteObject(iconInfo.hbmMask);
                }
            }
            finally
            {
                PInvoke.DestroyIcon(file.hIcon);
            }
        }

        return new FileTypeInfo
        {
            Icon = icon,
            TypeName = file.szTypeName.ToString(),
            Category = GetCategory(extension)
        };
    }
    public static void ClearShellCache()
    {
        ShellCache.Clear();
    }
}
