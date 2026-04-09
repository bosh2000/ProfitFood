using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.ModelsViewModels
{
    public sealed class UnitEditModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal BaseFactor { get; set; } = 1m;
        public bool IsBase { get; set; }
    }
}