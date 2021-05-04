using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    public class AppUserLogin : IdentityUserLogin<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }
    }
}
