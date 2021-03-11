using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppUserLogin), Schema = "auth")]
    public class AppUserLogin : IdentityUserLogin<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }
    }
}
