using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.ProductGroupRules
{
    internal class ProductGroupNameLengthRules : IBusinessRule
    {
        private readonly string _name;
        public ProductGroupNameLengthRules(string name)
        {
            _name = name;
        }
        public string Message => "Длина наименования более 200 символов";

        public bool IsBroken() => _name.Length > 200;

    }
}
