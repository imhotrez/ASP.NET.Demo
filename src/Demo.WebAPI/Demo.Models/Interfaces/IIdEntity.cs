using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для сущности EF
    /// </summary>
    public interface IIdEntity
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        [DisplayName("Идентификатор записи")]
        [Key]
        long Id { get; set; }
    }
}
