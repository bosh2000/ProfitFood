using ProfitFood.Model.DBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.References
{
    /// <summary>
    /// Категория продуктов (молочные, мясо, овощи).
    /// </summary>
    public sealed class ProductCategory : ReferenceEntityBase
    {
        public string Name { get; set; } = null!; // Наименование категории
        public Guid? ParentCategoryId { get; set; } // Родительская категория
        public int SortOrder { get; set; } // Порядок сортировки

        public ProductCategory? ParentCategory { get; set; } // Родитель
        public ICollection<ProductCategory> Children { get; set; } = new List<ProductCategory>(); // Дочерние категории
        public ICollection<Product> Products { get; set; } = new List<Product>(); // Продукты категории
    }
}