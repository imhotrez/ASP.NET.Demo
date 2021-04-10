using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models.Dto {
    public class RestorePasswordRequest {
        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        [Required]
        [EmailAddress]
        [DisplayName("Адрес электронной почты")]
        [RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "Must be a valid email")]
        public string Email { get; set; }
    }
}