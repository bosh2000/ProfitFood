using ProfitFood.Model.DBModel;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.ProductGroupRules
{
    public class ProductGroupMustNotBeNullRules : IBusinessRule
    {
        private readonly ProductGroup _productGroup;
        public ProductGroupMustNotBeNullRules(ProductGroup productGroup)
        {
            _productGroup = productGroup;
        }
        public string Message => "Группа обязательна";

        public bool IsBroken() => _productGroup is null;
    }
}
