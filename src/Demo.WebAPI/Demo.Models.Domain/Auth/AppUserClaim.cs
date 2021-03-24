using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppUserClaim), Schema = "auth")]
    public class AppUserClaim : IdentityUserClaim<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }
    }
}
