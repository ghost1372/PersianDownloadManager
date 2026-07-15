namespace PersianDownloadManager.Views;

public sealed partial class AddGroupLinkDialog : ContentDialogWindow
{
    public IEnumerable<string> Urls { get; private set; }

    public AddGroupLinkDialog()
    {
        InitializeComponent();

        Owner = App.MainWindow;
    }

    private void ContentDialogWindow_CloseButtonClick(object sender, EventArgs e)
    {
        TryClose();
    }

    private void ContentDialogWindow_PrimaryButtonClick(object sender, EventArgs e)
    {
        TryClose();
    }

    private void NBFrom_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateUrls();
    }

    private void NBTo_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateUrls();
    }

    private void NBSize_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        UpdateUrls();
    }
    private void TxtUrl_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateUrls();
    }
    private async void UpdateUrls()
    {
        if (NBFrom == null || NBTo == null || NBSize == null || TxtFirstFile == null || TxtSecondFile == null || TxtLastFile == null)
            return;

        if (!IsValidLink(TxtUrl.Text))
            return;

        Urls = GenerateUrls(TxtUrl.Text, from: (int)NBFrom.Value, to: (int)NBTo.Value, sequenceSize: (int)NBSize.Value);

        if (Urls.Count() > 1)
        {
            TxtFirstFile.Text = Urls.FirstOrDefault();
            TxtSecondFile.Text = Urls.ElementAt(1);
            TxtLastFile.Text = Urls.LastOrDefault();
        }
    }
    public static IEnumerable<string> GenerateUrls(string template, int from, int to, int sequenceSize)
    {
        if (string.IsNullOrWhiteSpace(template))
            yield break;

        // No placeholder -> just return the original URL
        if (!template.Contains('*'))
        {
            yield return template;
            yield break;
        }

        for (int i = from; i <= to; i++)
        {
            string number = i.ToString().PadLeft(sequenceSize, '0');
            yield return template.Replace("*", number);
        }
    }
}
