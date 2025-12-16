using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace cursova.Services
{
    public class BookingService
    {
        private readonly string _apiToken;

        public BookingService(string apiToken)
        {
            _apiToken = apiToken;
        }

        public async Task<List<Hotel>> SearchHotelsAsync(double lat, double lon, DateTime checkIn, DateTime checkOut)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);

            var url = $"https://distribution-xml.booking.com/json/bookings.getHotels?latitude={lat}&longitude={lon}&checkin_date={checkIn:yyyy-MM-dd}&checkout_date={checkOut:yyyy-MM-dd}&room1=A";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var hotels = new List<Hotel>();
            foreach (var item in doc.RootElement.GetProperty("hotels").EnumerateArray())
            {
                hotels.Add(new Hotel
                {
                    Id = item.GetProperty("hotel_id").GetString(),
                    Name = item.GetProperty("hotel_name").GetString(),
                    Latitude = item.GetProperty("latitude").GetDouble(),
                    Longitude = item.GetProperty("longitude").GetDouble(),
                    Price = item.TryGetProperty("price", out var p) ? p.GetDecimal() : 0
                });
            }

            return hotels;
        }
    }

    public class Hotel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal Price { get; set; }
    }
}
