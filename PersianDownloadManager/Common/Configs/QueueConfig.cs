using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace PersianDownloadManager.Common;


[GenerateAutoSaveOnChange]
public partial class QueueConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private string fileName { get; set; } = Constants.QueueConfigPath;

    private IList<Guid> queueOrder { get; set; } = new List<Guid>();
}
