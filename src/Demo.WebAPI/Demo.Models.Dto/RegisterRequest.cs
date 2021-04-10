using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Demo.Models.Dto {
    public class RegisterRequest : RefreshTokenRequest {
        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        [Required]
        [EmailAddress]
        [DisplayName("Адрес электронной почты")]
        [RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "Must be a valid email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [DisplayName("Пароль")]
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Required]
        [DisplayName("Подтверждение пароля")]
        [Compare(nameof(Password))]
        [JsonProperty("passwordConfirm")]
        public string PasswordConfirm { get; set; }
    }
}