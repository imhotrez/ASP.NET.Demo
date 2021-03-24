using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppUserToken), Schema = "auth")]
    public class AppUserToken : IdentityUserToken<long>
    {
        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName("Пользователь")]
        public virtual AppUser User { get; set; }
    }
}
