using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Категория блюда.
    /// </summary>
    public sealed class DishCategory : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Название категории блюда
        public int SortOrder { get; set; } // Сортировка

        public ICollection<Dish> Dishes { get; set; } = new List<Dish>(); // Блюда категории
    }
}