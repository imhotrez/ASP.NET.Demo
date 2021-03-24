using System.ComponentModel;
using Demo.Models.Interfaces;

namespace Demo.Models.Dto {
    public class AppUserDto : IIdEntity {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [DisplayName("id")]
        public long Id { get; set; }

        /// <summary>
        /// e-mail
        /// </summary>
        [DisplayName("e-mail")]
        public string Email { get; set; }

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
        /// Номер телефона
        /// </summary>
        [DisplayName("Номер телефона")]
        public string PhoneNumber { get; set; }
    }
}