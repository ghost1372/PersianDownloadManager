using Downloader;

namespace PersianDownloadManager.Models;

public partial class LocalDownloadItem
{
    public Guid Id { get; set; }

    public string FilePath { get; set; }

    public Uri Url { get; set; }

    public long TotalBytes { get; set; }

    public DownloadStatus Status { get; set; }

    public string AddedDate { get; set; }

    public string LastDate { get; set; }

    public bool IsQueue { get; set; }
    public bool IsResumeCapabilitySupported { get; set; }

    public double Progress { get; set; }

    public long DownloadedBytes { get; set; }
}
