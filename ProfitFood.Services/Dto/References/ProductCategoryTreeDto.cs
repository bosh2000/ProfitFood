using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Applications.Dto.References
{
    namespace ProfitFood.Applications.Dto.References
    {
        public sealed class ProductCategoryTreeDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public Guid? ParentCategoryId { get; set; }

            public int SortOrder { get; set; }

            public List<ProductCategoryTreeDto> Children { get; set; } = new();
        }
    }
}