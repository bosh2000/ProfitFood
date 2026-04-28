using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.Models.References
{
    public sealed class ProductCategoryDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid? ParentCategoryId { get; set; }

        public int SortOrder { get; set; }

        public List<ProductCategoryDto> Children { get; set; } = new();
    }
}