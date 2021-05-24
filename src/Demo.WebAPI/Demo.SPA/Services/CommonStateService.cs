using System;

namespace Demo.SPA.Services {
    public class CommonStateService {
        private string accessToken;

        public string AccessToken {
            get => accessToken;
            set {
                accessToken = value;
                NotifyStateChanged();
            }
        }

        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

        public event Action OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}