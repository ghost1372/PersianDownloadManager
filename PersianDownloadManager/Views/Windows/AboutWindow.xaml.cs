using Microsoft.UI.Windowing;
using WinRT;

namespace PersianDownloadManager.Views;

public sealed partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;

        AppWindow.IsShownInSwitchers = false;
        var presenter = AppWindow.Presenter.As<OverlappedPresenter>();
        presenter.IsAlwaysOnTop = true;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.IsResizable = false;

        presenter.PreferredMaximumWidth = 700;
        presenter.PreferredMaximumHeight = 270;

        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
        WindowHelper.CenterOnScreen(this);
    }

    private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        this.Close();
    }
}
