using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.ModelsViewModels
{
    public sealed class UnitListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string UnitTypeName { get; set; } = string.Empty;
        public bool IsBase { get; set; }
    }
}