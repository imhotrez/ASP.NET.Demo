using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Demo.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppRole), Schema = "auth")]
    public class AppRole : IdentityRole<long>, INamedEntity
    {
        /// <summary>
        /// Дата создания
        /// </summary>
        [DisplayName("Дата создания")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата изменения
        /// </summary>
        [DisplayName("Дата изменения")]
        public DateTime? UpdateDate { get; set; }

        public virtual ICollection<AppUserRole> UserRoles { get; set; }
        public virtual ICollection<AppRoleClaim> RoleClaims { get; set; }
    }
}
