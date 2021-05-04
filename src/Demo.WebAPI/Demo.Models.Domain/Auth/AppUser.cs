using System;
using System.Collections.Generic;
using System.ComponentModel;
using Demo.Models.Domain.Image;
using Demo.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Demo.Models.Domain.Auth {
    public class AppUser : IdentityUser<long>, IDatedEntity, IIdEntity {
        /// <summary>
        /// Фамилия
        /// </summary>
        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [DisplayName("Имя")]
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [DisplayName("Отчество")]
        public string MiddleName { get; set; }

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

        public virtual ICollection<AppUserClaim> Claims { get; set; }

        public virtual ICollection<AppUserLogin> Logins { get; set; }

        public virtual ICollection<AppUserToken> Tokens { get; set; }

        public virtual ICollection<AppUserRole> UserRoles { get; set; }

        public virtual ICollection<RefreshSession> RefreshSessions { get; set; }

        public virtual ICollection<Metadata> ImageMetadatas { get; set; }
    }
}
