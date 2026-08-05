using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using NewRelic.MAUI.Plugin;

namespace MauiDemoApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.ConfigureLifecycleEvents(appLifecycle =>
		{
#if ANDROID
			appLifecycle.AddAndroid(android => android
				.OnCreate((activity, savedInstanceState) => StartNewRelic()));
#endif
#if IOS
			appLifecycle.AddiOS(iOS => iOS.WillFinishLaunching((_, __) =>
			{
				StartNewRelic();
				return false;
			}));
#endif
		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static void StartNewRelic()
	{
		CrossNewRelic.Current.HandleUncaughtException();

		if (DeviceInfo.Current.Platform == DevicePlatform.Android)
		{
			CrossNewRelic.Current.Start(Secrets.NewRelicAndroidToken);
		}
		else if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
		{
			CrossNewRelic.Current.Start(Secrets.NewRelicIosToken);
		}
	}
}
