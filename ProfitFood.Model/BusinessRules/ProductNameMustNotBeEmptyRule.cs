using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Services.BusinessRules
{
    public class ProductNameMustNotBeEmptyRule : IBusinessRule
    {
        private readonly string _name;
        public ProductNameMustNotBeEmptyRule(string name)
        {
            _name = name;
        }
        public bool IsBroken() =>string.IsNullOrWhiteSpace(_name);
        public string Message => "Имя продукта обязательно";
    }
}
