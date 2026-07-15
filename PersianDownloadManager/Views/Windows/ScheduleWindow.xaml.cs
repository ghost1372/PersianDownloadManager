using Microsoft.UI.Windowing;
using WinRT;

namespace PersianDownloadManager.Views;

public sealed partial class ScheduleWindow : Window
{
    public string WindowTitle = Strings.ScheduleWindow_ScheduleTab_Header.GetLocalizedResource();
    public ScheduleWindowViewModel ViewModel { get; }
    public ScheduleWindow()
    {
        ViewModel = App.GetService<ScheduleWindowViewModel>();
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Standard;

        Title = AppWindow.Title = WindowTitle;
        SetWindowIcon(this);

        var presenter = AppWindow.Presenter.As<OverlappedPresenter>();
        presenter.IsMaximizable = false;
        presenter.IsResizable = false;

        presenter.PreferredMaximumHeight = 700;
        presenter.PreferredMinimumHeight = 700;

        presenter.PreferredMaximumWidth = 950;
        presenter.PreferredMinimumWidth = 950;

        WindowHelper.CenterOnScreen(this);
    }

    private void ChkStartTime_Checked(object sender, RoutedEventArgs e)
    {
        Settings.IsScheduleStartTimeEnabled = ChkStartTime.IsChecked.Value;
    }

    private void ChkStopTime_Checked(object sender, RoutedEventArgs e)
    {
        Settings.IsScheduleStopTimeEnabled = ChkStopTime.IsChecked.Value;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void OnStopClick(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.StopQueue();
    }
    private void OnStartClick(object sender, RoutedEventArgs e)
    {
        _ = MainWindow.Instance.StartQueueAsync();
    }

    private void ScheduleTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectedItems = ScheduleTableView.SelectedItems;
    }
}
