using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Demo.Models.Dto
{
    public class LoginRequest : RefreshTokenRequest
    {
        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        [Required]
        [EmailAddress]
        [DisplayName("Адрес электронной почты")]
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
        /// Запомнить меня
        /// </summary>
        [Required]
        [DisplayName("Запомнить меня")]
        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }
    }
}
