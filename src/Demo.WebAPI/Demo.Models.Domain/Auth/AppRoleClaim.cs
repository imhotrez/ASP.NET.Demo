using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppRoleClaim), Schema = "auth")]
    public class AppRoleClaim : IdentityRoleClaim<long>
    {
        /// <summary>
        /// Роль
        /// </summary>
        [DisplayName("Роль")]
        public virtual AppRole Role { get; set; }
    }
}
