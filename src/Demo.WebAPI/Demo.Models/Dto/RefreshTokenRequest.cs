using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Demo.Models.Dto
{
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Уникальный идентификатор браузера
        /// </summary>
        [Required]
        [DisplayName("Уникальный идентификатор браузера")]
        [JsonProperty("fingerprint")]
        public string FingerPrint { get; set; }
    }
}
