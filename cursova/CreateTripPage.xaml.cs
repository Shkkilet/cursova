using System;
using System.Formats.Tar;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using cursova.Models;
using cursova.Services;
using Microsoft.Maui.Controls;
using System.Security.Cryptography;
using System.Text;

namespace cursova;

public partial class CreateTripPage : ContentPage
{
    private bool _mapLoaded = false;
    private readonly TripsApiService _service = new();

    string startTripCord, endTripCord;
    public CreateTripPage()
    {
        InitializeComponent();
        tripTypePicker.ItemsSource = Enum.GetNames(typeof(TripType));

        // Підписуємося на події, якщо hybridView є WebView-похідним
        if (hybridView is WebView web)
        {
            web.Navigating += MapWebView_Navigating;
            web.Navigated += MapWebView_Navigated;
        }

        // Якщо HybridWebView сам завантажує map.html (MainFile) — нічого додатково не потрібно.
        // Якщо ні — можна завантажити вручну (рядок нижче показовий; зазвичай не потрібен коли MainFile в XAML).
        // _ = LoadMapHtmlIfNeeded();
    }

    // --- Якщо потрібно вручну прочитати map.html з пакету, використайте це ---
    private async Task LoadMapHtmlIfNeeded()
    {
        try
        {
            // Якщо HybridWebView не підхоплює MainFile автоматично, розкоментуйте і підставте код для завантаження.
            using var stream = await FileSystem.OpenAppPackageFileAsync("hybrid_root/map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            // Спробуємо встановити Source через рефлексію (якщо HybridWebView має властивість Source/Html)
            var t = hybridView.GetType().GetProperty("Source");
            if (t != null && t.CanWrite)
            {
                // створюємо HtmlWebViewSource динамічно
                var htmlSourceType = typeof(HtmlWebViewSource);
                var htmlSource = Activator.CreateInstance(htmlSourceType);
                htmlSourceType.GetProperty("Html")!.SetValue(htmlSource, html);
                t.SetValue(hybridView, htmlSource);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LoadMapHtmlIfNeeded error: " + ex);
        }
    }

    private void MapWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        _mapLoaded = true;
    }

    // Перехоплення JS -> MAUI (через зміну location.href на invoke://...)
    private async void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        var url = e.Url ?? string.Empty;

        if (url.StartsWith("invoke://", StringComparison.InvariantCultureIgnoreCase))
        {
            // зупиняємо навігацію
            e.Cancel = true;

            try
            {
                var uri = new Uri(url);
                var query = uri.Query; // ?type=start&lat=...&lon=...
                var q = System.Web.HttpUtility.ParseQueryString(query);

                if (uri.Host.Equals("set", StringComparison.InvariantCultureIgnoreCase) || uri.AbsolutePath.Contains("set"))
                {
                    var type = q["type"];
                    var latS = q["lat"];
                    var lonS = q["lon"];
                    var name = q["name"];

                    if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(latS) && !string.IsNullOrEmpty(lonS))
                    {
                        var coordText = $"{latS},{lonS}";
                        if (type == "start")
                        {
                            startEntry.Text = name;
                            startTripCord = coordText;
                        }
                        else if (type == "end")
                        {
                            endEntry.Text = name;
                            endTripCord = coordText;
                        }
                        // якщо обидва полі заповнені — викликаємо JS buildRoute(...)
                        if (!string.IsNullOrWhiteSpace(startEntry.Text) && !string.IsNullOrWhiteSpace(endEntry.Text))
                        {
                            if (TryParseCoords(startEntry.Text, out var sLat, out var sLon) &&
                                TryParseCoords(endEntry.Text, out var eLat, out var eLon))
                            {
                                var js = $"buildRoute([{sLat},{sLon}], [{eLat},{eLon}], []);";
                                await EnsureMapLoadedAndEval(js);
                            }
                        }
                    }
                }
                else if (uri.Host.Equals("action", StringComparison.InvariantCultureIgnoreCase) || uri.AbsolutePath.Contains("action"))
                {
                    var action = q["action"];
                    if (action == "cleared")
                    {
                        startEntry.Text = string.Empty;
                        endEntry.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Invoke URL parse error: " + ex);
            }
        }
    }

    private static bool TryParseCoords(string text, out double lat, out double lon)
    {
        lat = lon = 0;
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        return double.TryParse(parts[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lat)
            && double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lon);
    }

    private async Task EnsureMapLoadedAndEval(string js)
    {
        var tries = 0;
        while (!_mapLoaded && tries < 50)
        {
            await Task.Delay(100);
            tries++;
        }
        if (!_mapLoaded) return;

        await EvalJavascriptOnHybridView(js);
    }

    // Спробуємо виконати JS: спочатку - через WebView.EvaluateJavaScriptAsync, інакше - через метод EvaluateJavaScriptAsync/Eval на самому контролі
    private async Task EvalJavascriptOnHybridView(string js)
    {
        try
        {
            if (hybridView is WebView web)
            {
                await web.EvaluateJavaScriptAsync(js);
                return;
            }

            // Рефлексією шукаємо EvaluateJavaScriptAsync
            var t = hybridView.GetType();
            var mi = t.GetMethod("EvaluateJavaScriptAsync", new[] { typeof(string) });
            if (mi != null)
            {
                var taskObj = mi.Invoke(hybridView, new object[] { js }) as Task;
                if (taskObj != null) await taskObj;
                return;
            }

            // Спробуємо стару Eval(string)
            var mi2 = t.GetMethod("Eval", new[] { typeof(string) });
            if (mi2 != null)
            {
                mi2.Invoke(hybridView, new object[] { js });
                return;
            }

            System.Diagnostics.Debug.WriteLine("No suitable JS evaluation method found on HybridWebView.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("EvalJavascriptOnHybridView error: " + ex);
        }
    }

    private async void OnBuildRouteClicked(object sender, EventArgs e)
    {
        var name = tripName.Text;
        var desc = tripDescription.Text;
        var start = startEntry.Text;
        var end = endEntry.Text;
        var dateStart = startDatePicker.Date;
        var selectedTypeStr = tripTypePicker.SelectedItem?.ToString();
        TripType selectedType = TripType.Vehicle;

        if (!string.IsNullOrEmpty(selectedTypeStr))
        {
            selectedType = Enum.Parse<TripType>(selectedTypeStr);
        }

        var trip = new Trip
        {
            Name = name,
            Description = desc,
            StartDate = dateStart,
            StartTrip = startTripCord,
            StartTripName = start,
            EndTrip = endTripCord,
            EndTripName = end,
            Type = selectedType
        };

        try
        {
            var response = await _service.CreateTrip(trip);

            if (response != null && response.IsSuccessStatusCode)
            {
                await Shell.Current.GoToAsync("///main");
            }
            else
            {
                var error = response?.StatusCode.ToString() ?? "невідома помилка";
                await DisplayAlert("Помилка", $"Не вдалося створити подорож: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateTrip виняток: {ex.Message}");
            await DisplayAlert("Помилка", $"Виняток при створенні: {ex.Message}", "OK");
        }
    }

    private void OnToggleMapClicked(object sender, EventArgs e)
    {
        MapContainer.IsVisible = !MapContainer.IsVisible;

        if (MapContainer.IsVisible)
        {
            toggleMapButton.Text = "Сховати карту";
        }
        else
        {
            toggleMapButton.Text = "Показати карту";
        }
    }

}
