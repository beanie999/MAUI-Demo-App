using System.Diagnostics;
using NewRelic.MAUI.Plugin;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    int count = 0;
    private bool _isCheckingLocation;
    private CancellationTokenSource _cancelTokenSource;
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        CrossNewRelic.Current.RecordBreadcrumb("Counter Clicked.", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        var attr = new Dictionary<string, object>
        {
            { "application", "Test APP" },
            { "DeviceId", 1234555 },
            { "UserId", "Test USER" ?? "" }
        };
        attr.Add("level", "DEBUG");
        attr.Add("message", "This is From Maui");
        CrossNewRelic.Current.LogAttributes(attr);


        CrossNewRelic.Current.LogInfo("This is From Maui");
        CrossNewRelic.Current.LogDebug("This is From Maui");
        CrossNewRelic.Current.LogVerbose("This is From Maui");
        CrossNewRelic.Current.LogWarning("This is From Maui");
        CrossNewRelic.Current.LogError("This is From Maui");
        CrossNewRelic.Current.Log(LogLevel.VERBOSE,"This is From Maui");

        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
        keyValuePairs.Add("message", "This is From Attribute");
        keyValuePairs.Add("eat", "Pizza");
        keyValuePairs.Add("food", "tell me");

        CrossNewRelic.Current.LogAttributes(keyValuePairs);

        Console.WriteLine("This is Auto Instrumentation");

        try
        {
            throw new Exception("This is Error");
        } catch (Exception ex)
        {
            CrossNewRelic.Current.RecordException(ex, keyValuePairs);
        }

        Uri uri = new("https://reactnative.dev/movies.json");
        await callURI(uri);

        count++;
        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    
    private  void OnANRClicked(object sender, EventArgs e)
    {
        CrossNewRelic.Current.RecordBreadcrumb("ANR Clicked.", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        Task.Delay(10000).Wait();
    }
    
    private  void OnHandleExceptionClicked(object sender, EventArgs e)
    {
        CrossNewRelic.Current.RecordBreadcrumb("Handle Exception Clicked.", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        try
        {
            throw new Exception("This is a test exception");
        }
        catch (Exception ex)
        {
            CrossNewRelic.Current.RecordException(ex);
        }
    }
    
    private  void OnRecordEvents(object sender, EventArgs e)
    {
        CrossNewRelic.Current.RecordBreadcrumb("Record Events Clicked. Look at TestEvent", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        CrossNewRelic.Current.RecordCustomEvent("TestEvent", "EventsApplication",new Dictionary<string, object> { { "key1", "value1" }, { "key2", "value2" } });
        CrossNewRelic.Current.RecordBreadcrumb("TestBreadCrumb", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
    }
    
    private void OnCrashClicked(object sender, EventArgs e)
    {
        CrossNewRelic.Current.RecordBreadcrumb("Crash Clicked!", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        // Deliberately throw an unhandled exception to crash the application
        throw new Exception("This is a test crash");
    }
    
    private async void OnNetErrorClicked(object sender, EventArgs e) {
        CrossNewRelic.Current.RecordBreadcrumb("Network Error Clicked", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        Uri uri = new("https://newrelic.com/error");
        await callURI(uri);
    }

    private async void OnTraceClicked(object sender, EventArgs e) {
        CrossNewRelic.Current.RecordBreadcrumb("Distributed Tracing Clicked", new Dictionary<string, object> { { "test", "value1" }, { "test1", "value2" } });
        // **** Change the URI to your distributed tracing endpoint.
        Uri uri = new("https://localhost");
        await callURI(uri);
    }

    private async Task callURI(Uri uri) {
        try
        {
            HttpClientHandler httpClientHandler = CrossNewRelic.Current.GetHttpMessageHandler();
            httpClientHandler.AllowAutoRedirect = false;
            HttpClient myClient = new HttpClient(httpClientHandler);

            HttpResponseMessage response = await myClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                String content = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            await this.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
                Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
}