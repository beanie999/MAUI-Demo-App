using System.Text.Json;
using NewRelic.MAUI.Plugin;

namespace MauiDemoApp;

public partial class MainPage : ContentPage
{
	private static readonly Func<Exception>[] HandledExceptions =
	{
		() => new InvalidOperationException("Simulated invalid operation"),
		() => new NullReferenceException("Simulated null reference"),
		() => new TimeoutException("Simulated timeout"),
	};

	private static readonly string[] CrashMessages =
	{
		"Simulated crash: out of bounds",
		"Simulated crash: null dereference",
		"Simulated crash: divide by zero",
	};

	private static readonly string[] NetworkErrorUrls =
	{
		"https://newrelic.com/error",
		"https://httpstat.us/404",
		"https://httpstat.us/500",
		"https://httpstat.us/503",
		"https://this-domain-does-not-exist-nr-demo.invalid",
	};

	private const string LoginServiceUrl = "http://localhost:3000/login";

	public MainPage()
	{
		InitializeComponent();

		var buttonColor = new Color(0f, 0.67f, 0.343f);
		ButtonOne.BackgroundColor = buttonColor;
		ButtonTwo.BackgroundColor = buttonColor;
		ButtonThree.BackgroundColor = buttonColor;
		ButtonFour.BackgroundColor = buttonColor;
		ButtonFive.BackgroundColor = buttonColor;
	}

	private async void OnCustomEventButtonClicked(object? sender, EventArgs e)
	{
		var interactionId = CrossNewRelic.Current.StartInteraction("Open Custom Event Screen");
		CrossNewRelic.Current.EndInteraction(interactionId);

		await Navigation.PushAsync(new CustomEventPage());
	}

	private void OnUserIdEntryCompleted(object? sender, EventArgs e)
	{
		var userId = UserIdEntry.Text;
		if (string.IsNullOrWhiteSpace(userId))
			return;

		var interactionId = CrossNewRelic.Current.StartInteraction("Set User ID");

		CrossNewRelic.Current.SetUserId(userId);
		DropBreadcrumb($"User name set to {userId}", LogLevel.INFO);
		StatusLabel.Text = $"New Relic user ID set to: {userId}";
		SemanticScreenReader.Announce(StatusLabel.Text);

		CrossNewRelic.Current.EndInteraction(interactionId);
	}

	private void OnCustomAttributeEntryCompleted(object? sender, EventArgs e)
	{
		var value = CustomAttributeEntry.Text;
		if (string.IsNullOrWhiteSpace(value))
			return;

		var interactionId = CrossNewRelic.Current.StartInteraction("Set Custom Attribute");

		CrossNewRelic.Current.SetAttribute("CustomAttribute", value);
		DropBreadcrumb($"Custom attribute set to {value}", LogLevel.INFO);
		StatusLabel.Text = $"New Relic custom attribute set to: {value}";
		SemanticScreenReader.Announce(StatusLabel.Text);

		CrossNewRelic.Current.EndInteraction(interactionId);
	}

	private void OnHandledExceptionButtonClicked(object? sender, EventArgs e)
	{
		var interactionId = CrossNewRelic.Current.StartInteraction("Handled Exception");
		CrossNewRelic.Current.EndInteraction(interactionId);

		DropBreadcrumb("Handled Exception button clicked", LogLevel.WARNING);

		var createException = HandledExceptions[Random.Shared.Next(HandledExceptions.Length)];
		try
		{
			throw createException();
		}
		catch (Exception exception)
		{
			CrossNewRelic.Current.RecordException(exception);

			StatusLabel.Text = $"Recorded handled exception: {exception.GetType().Name}";
			SemanticScreenReader.Announce(StatusLabel.Text);
		}
	}

	private void OnCrashButtonClicked(object? sender, EventArgs e)
	{
		var message = CrashMessages[Random.Shared.Next(CrashMessages.Length)];
		var interactionId = CrossNewRelic.Current.StartInteraction("Crash");
		CrossNewRelic.Current.EndInteraction(interactionId);

		DropBreadcrumb("Crash button clicked", LogLevel.ERROR);
		CrossNewRelic.Current.CrashNow(message);
	}

	private async void OnNetworkErrorButtonClicked(object? sender, EventArgs e)
	{
		DropBreadcrumb("Network Error button clicked", LogLevel.WARNING);

		var interactionId = CrossNewRelic.Current.StartInteraction("Network Error Requests");
		CrossNewRelic.Current.EndInteraction(interactionId);

		using var client = new HttpClient(CrossNewRelic.Current.GetHttpMessageHandler());

		var urlCount = Random.Shared.Next(1, NetworkErrorUrls.Length + 1);
		var urls = NetworkErrorUrls.OrderBy(_ => Random.Shared.Next()).Take(urlCount);

		foreach (var url in urls)
		{
			try
			{
				var response = await client.GetAsync(url);
				StatusLabel.Text = $"{url} -> {(int)response.StatusCode}";
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"{url} -> {ex.GetType().Name}";
			}
		}

		StatusLabel.Text = $"Sent {urlCount} network requests";
		SemanticScreenReader.Announce(StatusLabel.Text);
	}

	private async void OnDistributedTraceButtonClicked(object? sender, EventArgs e)
	{
		var interactionId = CrossNewRelic.Current.StartInteraction("Distributed Trace Login");
		CrossNewRelic.Current.EndInteraction(interactionId);

		DropBreadcrumb("Distributed Trace button clicked", LogLevel.INFO);

		StatusLabel.Text = "Calling login micro service";
		SemanticScreenReader.Announce(StatusLabel.Text);

		using var client = new HttpClient(CrossNewRelic.Current.GetHttpMessageHandler());
		var request = new HttpRequestMessage(HttpMethod.Get, LoginServiceUrl);

		var userId = UserIdEntry.Text;
		if (!string.IsNullOrWhiteSpace(userId))
		{
			request.Headers.Add("X-User-Id", userId);
		}

		try
		{
			var response = await client.SendAsync(request);
			var body = await response.Content.ReadAsStringAsync();
			var message = ExtractMessage(body);

			StatusLabel.Text = message;
			DropBreadcrumb($"Login microservice responded: {message}", LogLevel.INFO);
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Login microservice call failed: {ex.Message}";
			DropBreadcrumb($"Login microservice call failed: {ex.Message}", LogLevel.ERROR);
		}

		SemanticScreenReader.Announce(StatusLabel.Text);
	}

	private static string ExtractMessage(string jsonBody)
	{
		using var document = JsonDocument.Parse(jsonBody);
		return document.RootElement.GetProperty("message").GetString() ?? jsonBody;
	}

	private static void DropBreadcrumb(string message, LogLevel level)
	{
		CrossNewRelic.Current.RecordBreadcrumb(message, new Dictionary<string, object>());

		switch (level)
		{
			case LogLevel.ERROR:
				CrossNewRelic.Current.LogError(message);
				break;
			case LogLevel.WARNING:
				CrossNewRelic.Current.LogWarning(message);
				break;
			case LogLevel.VERBOSE:
				CrossNewRelic.Current.LogVerbose(message);
				break;
			case LogLevel.DEBUG:
				CrossNewRelic.Current.LogDebug(message);
				break;
			default:
				CrossNewRelic.Current.LogInfo(message);
				break;
		}
	}
}
