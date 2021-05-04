using System.ComponentModel;

namespace Demo.Models.Interfaces {
    public interface INamedEntity : IIdEntity, IDatedEntity {
        /// <summary>
        /// Наименование
        /// </summary>
        [DisplayName("Наименование")]
        string Name { get; set; }

        /// <summary>
        /// Приведённое к верхнему регистру, нормализованное имя
        /// </summary>
        [DisplayName("Приведённое к верхнему регистру нормализованное имя")]
        string NormalizedName { get; set; }
    }
}