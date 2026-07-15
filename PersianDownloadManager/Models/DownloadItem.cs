using CommunityToolkit.Mvvm.ComponentModel;
using Downloader;
using Microsoft.UI.Xaml.Media;

namespace PersianDownloadManager.Models;

public partial class DownloadItem : ObservableObject
{
    [ObservableProperty]
    public partial int Index { get; set; }
    
    [ObservableProperty]
    public partial Guid Id { get; set; }
    
    [NotifyPropertyChangedFor(nameof(FileName))]
    [NotifyPropertyChangedFor(nameof(FileDirectory))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(FileInfo))]
    [NotifyPropertyChangedFor(nameof(Category))]
    [ObservableProperty]
    public partial string FilePath { get; set; }

    public string FileName => Path.GetFileName(FilePath);

    public string FileDirectory => Path.GetDirectoryName(FilePath);


    [ObservableProperty]
    public partial Uri Url { get; set; }

    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(RemainingTime))]
    [NotifyPropertyChangedFor(nameof(DownloadedSize))]
    [NotifyPropertyChangedFor(nameof(DownloadedSizeWithProgress))]
    [ObservableProperty]
    public partial long DownloadedBytes { get; set; }

    [NotifyPropertyChangedFor(nameof(Size))]
    [NotifyPropertyChangedFor(nameof(RemainingTime))]
    [ObservableProperty]
    public partial long TotalBytes { get; set; }

    [NotifyPropertyChangedFor(nameof(Speed))]
    [NotifyPropertyChangedFor(nameof(RemainingTime))]
    [ObservableProperty]
    public partial double SpeedBytesPerSecond { get; set; }

    [NotifyPropertyChangedFor(nameof(StatusTextWithPercent))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(Speed))]
    [NotifyPropertyChangedFor(nameof(RemainingTime))]
    [NotifyPropertyChangedFor(nameof(ShowProgressBar))]
    [NotifyPropertyChangedFor(nameof(ShowError))]
    [NotifyPropertyChangedFor(nameof(CanOpenFile))]
    [NotifyPropertyChangedFor(nameof(CanStop))]
    [NotifyPropertyChangedFor(nameof(CanResume))]
    [NotifyPropertyChangedFor(nameof(CanReDownload))]
    [ObservableProperty]
    public partial DownloadStatus Status { get; set; }

    [ObservableProperty]
    public partial string AddedDate { get; set; }

    [ObservableProperty]
    public partial string LastDate { get; set; }

    [ObservableProperty]
    public partial bool IsQueue { get; set; }

    [NotifyPropertyChangedFor(nameof(StatusTextWithPercent))]
    [NotifyPropertyChangedFor(nameof(FormattedProgress))]
    [ObservableProperty]
    public partial double Progress { get; set; }

    [NotifyPropertyChangedFor(nameof(IsResumeCapabilitySupportedText))]
    [ObservableProperty]
    public partial bool IsResumeCapabilitySupported { get; set; }

    public string IsResumeCapabilitySupportedText => IsResumeCapabilitySupported ? Strings.ResumeCapabilitySupported.GetLocalizedResource() : Strings.ResumeCapabilityNotSupported.GetLocalizedResource();

    public string Size => FileHelper.GetFileSize(TotalBytes);
    public string DownloadedSize => FileHelper.GetFileSize(DownloadedBytes);
    public string DownloadedSizeWithProgress => $"{FileHelper.GetFileSize(DownloadedBytes)} ( {FormattedProgress} )";
    public string Speed => Status == DownloadStatus.Running ? FormatSpeed(SpeedBytesPerSecond) : "-";
    public string RemainingTime
    {
        get
        {
            //if (Status != DownloadStatus.Running)
            //    return Strings.RemainingTime_Idle.GetLocalizedResource();

            var remainingBytes = TotalBytes - DownloadedBytes;
            if (remainingBytes <= 0)
                return string.Format(Strings.RemainingTime_SecondsFormat.GetLocalizedResource(), 0);

            if (SpeedBytesPerSecond <= 0 || TotalBytes <= 0)
                return Strings.RemainingTime_Calculating.GetLocalizedResource();

            var seconds = Math.Ceiling(remainingBytes / SpeedBytesPerSecond);

            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
                return Strings.RemainingTime_Calculating.GetLocalizedResource();

            return FormatRemainingTime(TimeSpan.FromSeconds(seconds));
        }
    }

    private static string FormatRemainingTime(TimeSpan remaining)
    {
        if (remaining.TotalMinutes < 1)
            return string.Format(Strings.RemainingTime_SecondsFormat.GetLocalizedResource(), Math.Max(1, remaining.Seconds));

        if (remaining.TotalHours < 1)
            return string.Format(Strings.RemainingTime_MinutesSecondsFormat.GetLocalizedResource(), (int)remaining.TotalMinutes, remaining.Seconds);

        if (remaining.TotalDays < 1)
            return string.Format(Strings.RemainingTime_HoursMinutesSecondsFormat.GetLocalizedResource(), (int)remaining.TotalHours, remaining.Minutes, remaining.Seconds);

        return string.Format(Strings.RemainingTime_DaysHoursMinutesSecondsFormat.GetLocalizedResource(), (int)remaining.TotalDays, remaining.Hours, remaining.Minutes, remaining.Seconds);
    }

    public string FormattedProgress
    {
        get
        {
            return $"{Progress:F0}%";
        }
    }
    public string StatusTextWithPercent
    {
        get
        {
            return $"{StatusText} {Progress:F0}%";
        }
    }
    public string StatusText
    {
        get
        {
            string value = string.Empty;
            switch (Status)
            {
                case DownloadStatus.None:
                    value = Strings.DownloadStatus_None.GetLocalizedResource();
                    break;
                case DownloadStatus.Created:
                    value = Strings.DownloadStatus_Created.GetLocalizedResource();
                    break;
                case DownloadStatus.Running:
                    value = Strings.DownloadStatus_Running.GetLocalizedResource();
                    break;
                case DownloadStatus.Stopped:
                    value = Strings.DownloadStatus_Stopped.GetLocalizedResource();
                    break;
                case DownloadStatus.Paused:
                    value = Strings.DownloadStatus_Paused.GetLocalizedResource();
                    break;
                case DownloadStatus.Completed:
                    value = Strings.DownloadStatus_Completed.GetLocalizedResource();
                    break;
                case DownloadStatus.Failed:
                    value = Strings.DownloadStatus_Failed.GetLocalizedResource();
                    break;
            }
            return $"{value}";
        }
    }
    public bool ShowProgressBar => Status != DownloadStatus.None && Status != DownloadStatus.Completed;

    public bool ShowError => Status is DownloadStatus.Paused or DownloadStatus.Failed or DownloadStatus.Stopped ? true : false;
    private FileTypeInfo TypeInfo => FileIconProvider.GetFileTypeInfo(FileName);
    public ImageSource Icon => TypeInfo.Icon;
    public string FileInfo => TypeInfo.TypeName;
    public SidebarDownloadCategoryModel Category => TypeInfo.Category;


    public bool CanOpenFile => Status == DownloadStatus.Completed;
    public bool CanStop => Status == DownloadStatus.Running;
    public bool CanResume => Status != DownloadStatus.Running
                          && Status != DownloadStatus.Completed
                          && Status != DownloadStatus.None;
    public bool CanReDownload => Status != DownloadStatus.Running;
}