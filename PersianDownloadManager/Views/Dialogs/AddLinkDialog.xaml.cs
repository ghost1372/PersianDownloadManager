namespace PersianDownloadManager.Views;

public sealed partial class AddLinkDialog : ContentDialogWindow
{
    public string NewLink { get; private set; }
    public AddLinkDialog()
    {
        InitializeComponent();
        Owner = App.MainWindow;
    }

    private async void ContentDialogWindow_PrimaryButtonClick(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(TxtLink.Text))
        {
            await MessageBox.ShowErrorAsync(Strings.AddLinkDialogMessageBoxEmptyMessage.GetLocalizedResource(), Strings.AddLinkDialogMessageBoxTitle.GetLocalizedResource());
            return;
        }

        if (!IsValidLink(TxtLink.Text))
        {
            await MessageBox.ShowErrorAsync(Strings.AddLinkDialogMessageBoxNotValidMessage.GetLocalizedResource(), Strings.AddLinkDialogMessageBoxTitle.GetLocalizedResource());
            return;
        }

        NewLink = TxtLink.Text;

        TryClose();
    }

    private void ContentDialogWindow_CloseButtonClick(object sender, EventArgs e)
    {
        TryClose();
    }
}
