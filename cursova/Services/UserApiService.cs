using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using cursova.Models;
namespace cursova.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;

        public UserApiService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7104/api/")
            };

        }
        public Task<HttpResponseMessage> CreateUser(User user)
        {
            return _http.PostAsJsonAsync("users", user);
        }
    }
}
