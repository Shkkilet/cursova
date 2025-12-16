using cursova.Models;
using cursova.Services;

namespace cursova;

public partial class AllTripsPage : ContentPage
{
    private readonly TripsApiService _service = new TripsApiService();

    public AllTripsPage()
	{
		InitializeComponent();
}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTrips();
    }

    private async void OnTripSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Trip selectedTrip)
        {
            // Переходимо на сторінку редагування і передаємо id
            await Navigation.PushAsync(new EditTripPage(selectedTrip.Id));

            // очищаємо вибір, щоб можна було клікнути той самий елемент ще раз
            tripsCollection.SelectedItem = null;
        }
    }
    private async Task LoadTrips()
    {
        try
        {
            var trips = await _service.GetAllTrips();
            tripsCollection.ItemsSource = trips;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося завантажити подорожі: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        if (button?.CommandParameter is not int tripId)
            return;

        bool confirm = await DisplayAlert(
            "Підтвердження",
            "Видалити цю подорож?",
            "Так",
            "Ні"
        );

        if (!confirm)
            return;

        var response = await _service.DeleteTrip(tripId);

        if (response.IsSuccessStatusCode)
        {
            await DisplayAlert("OK", "Подорож видалено", "OK");
            await LoadTrips();
        }
        else
        {
            await DisplayAlert("Помилка", "Не вдалося видалити", "OK");
        }
    }
    private async void OnIsDoneCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is Trip trip)
        {
            var response = await _service.ChangeIsDone(trip.Id, e.Value);
            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Помилка", "Не вдалося змінити стан", "OK");
                cb.IsChecked = !e.Value;
            }
        }
    }

}