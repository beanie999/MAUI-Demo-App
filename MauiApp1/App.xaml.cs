using NewRelic.MAUI.Plugin;

namespace MauiApp1;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        CrossNewRelic.Current.TrackShellNavigatedEvents();
    }
}