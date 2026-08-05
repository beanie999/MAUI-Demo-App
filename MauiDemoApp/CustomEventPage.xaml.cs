using NewRelic.MAUI.Plugin;

namespace MauiDemoApp;

public partial class CustomEventPage : ContentPage
{
	public CustomEventPage()
	{
		InitializeComponent();

		SendEventButton.BackgroundColor = new Color(0f, 0.67f, 0.343f);
	}

	private void OnSendEventClicked(object? sender, EventArgs e)
	{
		var interactionId = CrossNewRelic.Current.StartInteraction("Send Custom Event");
		CrossNewRelic.Current.EndInteraction(interactionId);

		var animal = AnimalEntry.Text;
		var country = CountryEntry.Text;

		var attributes = new Dictionary<string, object>
		{
			{ "animal", animal ?? string.Empty },
			{ "country", country ?? string.Empty },
		};

		CrossNewRelic.Current.RecordCustomEvent("MauiCustomMobileEvent", "MauiCustomMobileEvent", attributes);

		StatusLabel.Text = $"{animal} from {country} sent";
		SemanticScreenReader.Announce(StatusLabel.Text);
	}
}
