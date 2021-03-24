using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Demo.Models.Dto;

namespace Demo.SPA.Services {
    public class AuthService {
        private readonly CommonStateService commonStateService;
        private readonly HttpClient httpClient;
        public AuthService(CommonStateService commonStateService, HttpClient httpClient) {
            this.commonStateService = commonStateService;
            this.httpClient = httpClient;
        }

        public async Task Login(LoginRequest loginRequest) {
            var result = await httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            commonStateService.AccessToken = await result.Content.ReadAsStringAsync();
        }
    }
}