using System;
using System.ComponentModel;

namespace Demo.Models.Interfaces
{
    public interface IDatedEntity
    {
        /// <summary>
        /// Дата создания записи
        /// </summary>
        [DisplayName("Дата создания записи")]
        DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата редактирования записи
        /// </summary>
        [DisplayName("Дата редактирования записи")]
        DateTime? UpdateDate { get; set; }
    }
}
