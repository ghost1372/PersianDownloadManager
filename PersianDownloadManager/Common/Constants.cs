using Downloader;

namespace PersianDownloadManager.Common;

public static class Constants
{
    public static readonly string RootDirectoryPath = Path.Combine(PathHelper.GetAppDataFolderPath(), ProcessInfoHelper.ProductName);
    public static readonly string AppConfigPath = Path.Combine(RootDirectoryPath, "AppConfig.json");
    public static readonly string DownloadEngineConfigPath = Path.Combine(RootDirectoryPath, "DownloadEngineConfig.json");
    public static readonly string DownloadConfigPath = Path.Combine(RootDirectoryPath, "DownloadConfig.json");
    public static readonly string QueueConfigPath = Path.Combine(RootDirectoryPath, "QueueConfig.json");
    public static readonly string LogDirectoryPath = Path.Combine(RootDirectoryPath, "Log");
    public static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "Log.txt");
    public static readonly string RepoName = "PersianDownloadManager";
    public static readonly string Username = "Ghost1372";
    public static readonly string RepoUrl = $"https://github.com/{Username}/{RepoName}";
    public static readonly string RepoReleaseUrl = $"https://github.com/{Username}/{RepoName}/releases";
    public static readonly FontOption DefaultUIFont = FontHelper.UIFonts.LastOrDefault();

    public static readonly Dictionary<DownloadFilterType, HashSet<string>> Filters = new()
    {
        [DownloadFilterType.Archives] = new()
        {
            "zip", "rar", "7z", "tar", "gz", "r0", "r1",
            "arj", "sit", "sitx", "sea", "ace", "bz2"
        },

        [DownloadFilterType.Media] = new()
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".avif", ".svg", ".bmp", ".ico", ".apng", ".jxl", ".heic", ".heif", ".tif", ".tiff",
            ".mp4", ".webm", ".mkv", ".mov", ".m4v", ".ts", ".m2ts", ".3gp", ".avi", ".flv", ".ogv", ".wmv"
        },

        [DownloadFilterType.Music] = new()
        {
            ".mp3", ".m4a", ".aac", ".wav", ".flac", ".ogg", ".opus", ".wma", ".aiff", ".alac", ".amr"
        },

        [DownloadFilterType.Apps] = new()
        {
            ".exe", "appxbundle", ".msi", ".msix", ".appx", ".apk", ".aab", ".xapk", ".ipa", ".dmg", ".pkg", ".deb", ".rpm", ".appimage", ".snap", ".flatpak", ".jar", ".iso"
        },

        [DownloadFilterType.Documents] = new()
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".csv", ".txt", ".rtf", ".odt", ".ods", ".odp", ".epub", ".md", ".xml", ".json"
        }
    };

    public static string DefaultDownloadFolderPath { get; set; } = PathHelper.GetKnownFolderPath(Microsoft.Windows.Storage.Pickers.PickerLocationId.Downloads);
    public static int DefaultBufferBlockSize { get; set; } = 8000;
    public static int DefaultChunkCount { get; set; } = 8;
    public static long DefaultMaximumBytesPerSecond { get; set; } = 0;
    public static int DefaultMaxTryAgainOnFailure { get; set; } = 5;
    public static long DefaultMaximumMemoryBufferBytes { get; set; } = 50 * 1024 * 1024;
    public static bool DefaultParallelDownload { get; set; } = true;
    public static int DefaultParallelCount { get; set; } = 4;
    public static int DefaultBlockTimeout { get; set; } = 1000;
    public static int DefaultHttpClientTimeout { get; set; } = 100 * 1000;
    public static bool DefaultClearPackageOnCompletionWithFailure { get; set; } = false;
    public static long DefaultMinimumSizeOfChunking { get; set; } = 512;
    public static long DefaultMinimumChunkSize { get; set; } = 0;
    public static FileExistPolicy DefaultFileExistPolicy { get; set; } = FileExistPolicy.Rename;
    public static bool DefaultEnableAutoResumeDownload { get; set; } = true;
    public static string DefaultDownloadFileExtension { get; set; } = ".download";
}
