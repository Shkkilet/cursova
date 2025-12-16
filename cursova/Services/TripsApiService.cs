using cursova.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace cursova.Services
{
    public class TripsApiService
    {
        private readonly HttpClient _http;

        public TripsApiService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://0.0.0.0:5256/api/")
                //BaseAddress = new Uri("http://localhost:5256/api/")

            };

        }
        public async Task<HttpResponseMessage> CreateTrip(Trip trip)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("trips", trip);
                Debug.WriteLine($"Статус: {response.StatusCode}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка CreateTrip: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Trip>> GetAllTrips()
        {
            try
            {
                var trips = await _http.GetFromJsonAsync<List<Trip>>("trips");
                return trips;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка: {ex.Message}");
                return new List<Trip>();
            }
        }

        public async Task<Trip> GetTrip(int id)
        {
            return await _http.GetFromJsonAsync<Trip>($"trips/{id}");
        }


        public Task<HttpResponseMessage> UpdateTrip(int id, Trip trip)
        {
            return _http.PutAsJsonAsync($"trips/{id}", trip);
        }
        public Task<HttpResponseMessage> DeleteTrip(int id)
        {
            return _http.DeleteAsync($"trips/{id}");
        }
        public Task<HttpResponseMessage> ChangeIsDone(int id, bool isDone)
        {
            return _http.PatchAsJsonAsync($"trips/{id}/done", isDone);
        }

    }
}
