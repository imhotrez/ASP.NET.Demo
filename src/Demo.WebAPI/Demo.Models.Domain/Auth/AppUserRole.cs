using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppUserRole), Schema = "auth")]
    public class AppUserRole : IdentityUserRole<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }

        /// <summary>
        /// Роль
        /// </summary>
        [DisplayName("Роль")]
        public virtual AppRole Role { get; set; }
    }
}
