namespace Demo.SPA.Services {
    public class CommonStateService {
        public string AccessToken { get; set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
    }
}