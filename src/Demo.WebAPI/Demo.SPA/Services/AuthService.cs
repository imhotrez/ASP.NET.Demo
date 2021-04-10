using System.Threading;
using System.Threading.Tasks;
using AntDesign;
using Demo.Models.Dto;

namespace Demo.SPA.Services {
    public class AuthService {
        private readonly CommonStateService commonStateService;
        private readonly RestService restService;
        private readonly NotificationService notice;

        public AuthService(CommonStateService commonStateService, RestService restService, NotificationService notice) {
            this.commonStateService = commonStateService;
            this.restService = restService;
            this.notice = notice;
        }

        public async Task Login(LoginRequest loginRequest, CancellationToken cancellationToken) {
            var result =
                await restService.ExecutePost<LoginRequest, AuthResponse>("api/auth/login", loginRequest,
                    cancellationToken);

            if (result?.AccessToken != null) {
                commonStateService.AccessToken = result.AccessToken;
            }
        }

        public async Task Register(RegisterRequest registerRequest, CancellationToken cancellationToken) {
            var result =
                await restService.ExecutePost<RegisterRequest, AuthResponse>("api/auth/register", registerRequest,
                    cancellationToken);

            if (result?.AccessToken != null) {
                commonStateService.AccessToken = result.AccessToken;
            }
        }

        public async Task RefreshSession(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken) {
            var result =
                await restService.ExecutePost<RefreshTokenRequest, AuthResponse>("api/auth/refresh",
                    refreshTokenRequest,
                    cancellationToken);

            if (result?.AccessToken != null) {
                commonStateService.AccessToken = result.AccessToken;
            }
        }

        public async Task Logout(CancellationToken cancellationToken) {
            await restService.ExecuteGet("api/auth/logout", cancellationToken);
            commonStateService.AccessToken = null;
        }

        public async Task RestorePassword(RestorePasswordRequest restorePasswordRequest,
            CancellationToken cancellationToken) {
            await restService.ExecutePost<RestorePasswordRequest, AuthResponse>("api/auth/restorepassword",
                restorePasswordRequest,
                cancellationToken);
        }
    }
}