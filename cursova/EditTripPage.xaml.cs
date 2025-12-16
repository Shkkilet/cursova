using cursova.Models;
using cursova.Services;

namespace cursova;

public partial class EditTripPage : ContentPage
{

    private readonly TripsApiService _service = new();
    private int _tripId;
    public EditTripPage(int tripId)
	{
		InitializeComponent();

        _tripId = tripId;

        LoadTrip();
    }

    private async void LoadTrip()
    {
        var trip = await _service.GetTrip(_tripId);

        tripName.Text = trip.Name;
        tripDescription.Text = trip.Description;
        startEntry.Text = trip.StartTripName;
        endEntry.Text = trip.EndTripName;
        startDatePicker.Date = trip.StartDate;

        tripTypePicker.ItemsSource = Enum.GetValues(typeof(TripType)).Cast<TripType>().ToList();
        tripTypePicker.SelectedItem = trip.Type;

        hybridView.Navigated += async (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(trip.StartTrip) && !string.IsNullOrWhiteSpace(trip.EndTrip))
            {
                var startParts = trip.StartTrip.Split(','); // ["50.45","30.52"]
                var endParts = trip.EndTrip.Split(',');     // ["50.50","30.55"]

                var js = $"buildRoute([{startParts[0]}, {startParts[1]}], [{endParts[0]}, {endParts[1]}], []);";
                await hybridView.EvaluateJavaScriptAsync(js);
            }
        };
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
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var updated = new Trip
        {
            Id = _tripId,
            Name = tripName.Text,
            Description = tripDescription.Text,
            StartTrip = startEntry.Text,
            EndTrip = endEntry.Text,
            StartDate = startDatePicker.Date,
            Type = (TripType)tripTypePicker.SelectedItem
        };

        await _service.UpdateTrip(_tripId, updated);

        await DisplayAlert("OK", "Подорож оновлено", "Гаразд");
        await Shell.Current.GoToAsync(".."); // назад
    }
}