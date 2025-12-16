namespace cursova
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }



        private async void OnCreateTripClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CreateTripPage());
        }    private async void OnGetTripsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AllTripsPage());
        }


        //private async void HybridWebView_RawMessageReceived_1(object sender, HybridWebView.HybridWebViewRawMessageReceivedEventArgs e)
        //{
        //    var message = e.Message;

        //    if(message == "internet")
        //    {
        //        await Dispatcher.DispatchAsync(async () =>
        //        {
        //            var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        //            var internetType = Connectivity.Current.ConnectionProfiles.FirstOrDefault();

        //            await Application.Current.MainPage.DisplayAlert("Internet", $"Status: {hasInternet} of type {internetType}", "OK");
        //        });
        //    }
        //    else
        //    {
        //    await Dispatcher.DispatchAsync(async () =>
        //    {
        //        await DisplayAlert("JS Message", message, "OK");
        //    });
        //    }
        //}
        //private async void OnAddUserClicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync(nameof(CreateUserPage));
        //}

    }

}
