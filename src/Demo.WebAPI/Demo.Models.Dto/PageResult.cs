using System.Collections.Generic;

namespace Demo.Models.Dto {
    /// <summary>
    /// Страница результата
    /// </summary>
    /// <typeparam name="TEntity">Сущность</typeparam>
    public class PageResult<TEntity> where TEntity : class {
        /// <summary>
        /// Общее количество
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Данные
        /// </summary>
        public IEnumerable<TEntity> Items { get; set; }
    }
}