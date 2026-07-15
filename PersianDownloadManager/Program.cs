using Microsoft.Windows.AppLifecycle;

namespace PersianDownloadManager;

public partial class Program : SingleInstanceApp
{
    [STAThread]
    static int Main(string[] args)
    {
        return Run(args, "PersianDownloadManagerInstance", () => new Program(), () => new App());
    }

    protected override void OnActivated(AppActivationArguments args)
    {
        SingleInstanceWindowActivator.Activate();
    }
}