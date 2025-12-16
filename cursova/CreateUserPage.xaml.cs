using cursova.Models;
using cursova.Services;
using System.Security.Cryptography;
using System.Text;

namespace cursova
{
    public partial class CreateUserPage : ContentPage
    {
        private readonly UserApiService _service = new();

        public CreateUserPage()
        {
            InitializeComponent();
        }

        private async void OnCreateUserClicked(object sender, EventArgs e)
        {
            var name = NameEntry.Text;
            var surname = SurnameEntry.Text;
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all required fields.", "OK");
                return;
            }
            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                PasswordEntry.Text = string.Empty;
                ConfirmPasswordEntry.Text = string.Empty;
                return;
            }
            var passwordHash = HashPassword(password);

            var user = new User
            {
                Name = name,
                Surname = surname,
                Email = email,
                PasswordHash = passwordHash
            };

            var response = await _service.CreateUser(user);

            if (response.IsSuccessStatusCode)
            {
                StatusLabel.Text = $"✅ User '{name}' created successfully!";
                StatusLabel.TextColor = Colors.Green;
                NameEntry.Text = SurnameEntry.Text = EmailEntry.Text = PasswordEntry.Text = "";
            }
            else
            {
                StatusLabel.Text = "❌ Error creating user!";
                StatusLabel.TextColor = Colors.Red;
            }
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
