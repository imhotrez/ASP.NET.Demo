using System.ComponentModel;
using Demo.Models.Interfaces;

namespace Demo.Models.Dto {
    public class AppRoleDto : IIdEntity {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [DisplayName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Наименование роли
        /// </summary>
        [DisplayName("Наименование роли")]
        public string Name { get; set; }
    }
}