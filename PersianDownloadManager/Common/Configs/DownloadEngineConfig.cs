using Downloader;
using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace PersianDownloadManager.Common;

[GenerateAutoSaveOnChange]
public partial class DownloadEngineConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private string fileName { get; set; } = Constants.DownloadEngineConfigPath;

    private string downloadFolderPath { get; set; } = PathHelper.GetKnownFolderPath(Microsoft.Windows.Storage.Pickers.PickerLocationId.Downloads);

    private int bufferBlockSize { get; set; } = Constants.DefaultBufferBlockSize;
    private int chunkCount { get; set; } = Constants.DefaultChunkCount;
    private long maximumBytesPerSecond { get; set; } = Constants.DefaultMaximumBytesPerSecond;
    private int maxTryAgainOnFailure { get; set; } = Constants.DefaultMaxTryAgainOnFailure;
    private long maximumMemoryBufferBytes { get; set; } = Constants.DefaultMaximumMemoryBufferBytes;
    private bool parallelDownload { get; set; } = Constants.DefaultParallelDownload;
    private int parallelCount { get; set; } = Constants.DefaultParallelCount;
    private int blockTimeout { get; set; } = Constants.DefaultBlockTimeout;
    private int httpClientTimeout { get; set; } = Constants.DefaultHttpClientTimeout;
    private bool clearPackageOnCompletionWithFailure { get; set; } = Constants.DefaultClearPackageOnCompletionWithFailure;
    private long minimumSizeOfChunking { get; set; } = Constants.DefaultMinimumSizeOfChunking;
    private long minimumChunkSize { get; set; } = Constants.DefaultMinimumChunkSize;
    private FileExistPolicy fileExistPolicy { get; set; } = Constants.DefaultFileExistPolicy;
    private bool enableAutoResumeDownload { get; set; } = Constants.DefaultEnableAutoResumeDownload;
    private string downloadFileExtension { get; set; } = Constants.DefaultDownloadFileExtension;
}
