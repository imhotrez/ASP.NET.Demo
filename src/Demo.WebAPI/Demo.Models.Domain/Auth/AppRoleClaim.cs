using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    public class AppRoleClaim : IdentityRoleClaim<long>
    {
        /// <summary>
        /// Роль
        /// </summary>
        [DisplayName("Роль")]
        public virtual AppRole Role { get; set; }
    }
}
