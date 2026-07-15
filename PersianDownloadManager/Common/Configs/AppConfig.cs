using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace PersianDownloadManager.Common;

[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private string fileName { get; set; } = Constants.AppConfigPath;
    private bool useDeveloperMode { get; set; } = true;
    private string lastUpdateCheck { get; set; }
    private FontOption uIFont { get; set; }
    private Dictionary<DownloadFilterType, HashSet<string>> downloadFilters { get; set; } = Constants.Filters;

    private bool useWindowsShellIcons { get; set; } = false;
    private long speedLimit { get; set; } = 0;
    private TimeSpan? scheduleStartTime { get; set; }
    private TimeSpan? scheduleStopTime { get; set; }
    private bool isScheduleStartTimeEnabled { get; set; }
    private bool isScheduleStopTimeEnabled { get; set; }
    private int scheduleMaxRetry { get; set; } = 3;
    private bool isScheduleMaxRetryEnabled { get; set; }
    private bool isScheduleClosePDMEnabled { get; set; }
    private bool isScheduleShutdownEnabled { get; set; }
    private bool isScheduleForceCloseAppEnabled { get; set; }
    private int scheduleConcurrentCount { get; set; } = 1;
}
