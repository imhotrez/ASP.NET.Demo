using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    public class AppUserClaim : IdentityUserClaim<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }
    }
}
