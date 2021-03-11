using Demo.Models.Enums;

namespace Demo.Models.Filters {
    public class BaseFilter : PagingFilter {
        private const string DefaultOrderProperty = "Id";

        private string _orderProperty;

        public BaseFilter() {
            OrderProperty = DefaultOrderProperty;
            OrderDirection = OrderDirection.Asc;
        }

        /// <summary>
        /// Сортировать по свойству (ToLower)
        /// </summary>
        public string OrderProperty {
            get => _orderProperty;
            set => _orderProperty = string.IsNullOrEmpty(value) ? DefaultOrderProperty : value.ToLower();
        }

        /// <summary>
        /// Направление сортировки (asc, desc)
        /// </summary>
        public OrderDirection OrderDirection { get; set; }
    }
}