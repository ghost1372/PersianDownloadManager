using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using WinRT;
using WinRT.Interop;

namespace PersianDownloadManager.Views;

public sealed partial class CurrentDownloadWindow : Window
{
    public DownloadTask CurrentTask { get; set; }
    public string WindowTitle => $"{CurrentTask?.Item?.FormattedProgress} - {CurrentTask?.Item?.FileName}";
    private readonly IntPtr hwnd;
    public CurrentDownloadWindow()
    {
        InitializeComponent();
        hwnd = WindowNative.GetWindowHandle(this);
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        SetWindowIcon(this);

        var presenter = AppWindow.Presenter.As<OverlappedPresenter>();
        presenter.IsMaximizable = false;
        presenter.IsResizable = false;

        presenter.PreferredMaximumHeight = 500;
        presenter.PreferredMinimumHeight = 500;

        presenter.PreferredMaximumWidth = 950;
        presenter.PreferredMinimumWidth = 950;

        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
        WindowHelper.CenterOnScreen(this);


        UpdateTitle();

        Closed += OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        CurrentTask?.Item?.PropertyChanged -= Item_PropertyChanged;
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentTask.Item.IsResumeCapabilitySupported))
            UpdateTextBlock();

        if (e.PropertyName == nameof(CurrentTask.Item.TotalBytes))
            TxtFileSize.Text = CurrentTask.Item.Size;

        if (e.PropertyName == nameof(CurrentTask.Item.Progress) ||
            e.PropertyName == nameof(CurrentTask.Item.FileName) ||
            e.PropertyName == nameof(CurrentTask.Item.Status))
        {
            UpdateTitle();

            MainSpeedGraph.Points.Add(new System.Numerics.Vector2((float)CurrentTask.Item.Progress, (float)CurrentTask.Item.SpeedBytesPerSecond));
            switch (CurrentTask.Item.Status)
            {
                case Downloader.DownloadStatus.None:
                case Downloader.DownloadStatus.Created:
                case Downloader.DownloadStatus.Running:
                    MainSpeedGraph.SetGraphNormal();
                    TaskbarHelper.SetProgressState(hwnd, TaskbarStates.Normal);
                    TaskbarHelper.SetProgressValue(hwnd, CurrentTask.Item.Progress, 100);
                    break;
                case Downloader.DownloadStatus.Failed:
                case Downloader.DownloadStatus.Stopped:
                case Downloader.DownloadStatus.Paused:
                    MainSpeedGraph.SetGraphError();
                    TaskbarHelper.SetProgressState(hwnd, TaskbarStates.Paused);
                    break;
                case Downloader.DownloadStatus.Completed:
                    MainSpeedGraph.SetGraphSuccess();
                    TaskbarHelper.SetProgressState(hwnd, TaskbarStates.Normal);
                    break;                
            }
        }
    }

    private void UpdateTitle()
    {
        Title = AppWindow?.Title = WindowTitle;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        CurrentTask?.Pause();
        Close();
    }
    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        CurrentTask?.Item?.PropertyChanged -= Item_PropertyChanged;
        CurrentTask?.Item?.PropertyChanged += Item_PropertyChanged;

        UpdateTextBlock();
    }

    private void UpdateTextBlock()
    {
        TxtIsResume.Foreground = (Brush)Application.Current.Resources[CurrentTask.Item.IsResumeCapabilitySupported? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush"];
    }
    private void OnPauseChecked(object sender, RoutedEventArgs e)
    {
        if (PauseToggleButton.IsChecked.Value)
        {
            PauseToggleButton.Label = Strings.CurrentDownloadWindow_Resume_Label.GetLocalizedResource();
            CurrentTask?.Pause();
            StckTransfer.Visibility = Visibility.Collapsed;
        }
        else
        {
            PauseToggleButton.Label = Strings.CurrentDownloadWindow_Pause_Label.GetLocalizedResource();
            CurrentTask?.ResumeAsync();
            StckTransfer.Visibility = Visibility.Visible;
        }
    }
}
