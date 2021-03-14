using static System.Math;

namespace Demo.Models.Filters {
    public class PagingFilter {
        public PagingFilter() {
            MaxRecordsCount = 100;
            RecordsCount = 100;
            PageNumber = 1;
        }

        private int _recordsCount;

        /// <summary>
        /// Максимально количество записей на странице
        /// </summary>
        protected int MaxRecordsCount;

        /// <summary>
        /// Всего страниц
        /// </summary>
        public decimal PagesTotal { get; set; }

        /// <summary>
        /// Количество записей на странице
        /// </summary>
        public int RecordsCount {
            get => _recordsCount;
            set => _recordsCount = value < 1 ? 1 : value > MaxRecordsCount ? MaxRecordsCount : value;
        }

        /// <summary>
        /// Номер страницы
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Колиество страниц
        /// </summary>
        public int PagesCount => (int) Ceiling(PagesTotal / RecordsCount);

        /// <summary>
        /// Сколько элементов пропустить
        /// </summary>
        public int Skip => PageNumber * RecordsCount;

        /// <summary>
        /// Сколько элементов получить
        /// </summary>
        public int Take => RecordsCount;
    }
}