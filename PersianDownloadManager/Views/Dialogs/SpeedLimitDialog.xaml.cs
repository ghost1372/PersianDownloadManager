namespace PersianDownloadManager.Views;

public sealed partial class SpeedLimitDialog : ContentDialogWindow
{
    public SpeedLimitDialog()
    {
        InitializeComponent();

        Owner = App.MainWindow;
        NBLimit.Value = Settings.SpeedLimit;
    }

    private void ContentDialogWindow_CloseButtonClick(object sender, EventArgs e)
    {
        TryClose();
    }

    private void ContentDialogWindow_PrimaryButtonClick(object sender, EventArgs e)
    {
        Settings.SpeedLimit = (long)NBLimit.Value;
        TryClose();
    }

    private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (args.NewValue == 0)
        {
            LimitInfoBar.Title = Strings.SpeedLimitDialog_NoLimit.GetLocalizedResource();
            return;
        }

        LimitInfoBar.Title = $"{FileHelper.GetFileSize((long)args.NewValue)}/s";
    }
}
