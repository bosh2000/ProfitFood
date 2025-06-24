using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.ProductGroupRules
{
    internal class ProductGroupNameMustNotBeEmpty : IBusinessRule
    {
        private readonly string _groupName;
        public ProductGroupNameMustNotBeEmpty(string groupName)
        {
            _groupName = groupName;
        }
        public string Message => "Имя группы обязательно";

        public bool IsBroken() =>string.IsNullOrWhiteSpace(_groupName);

    }
}
