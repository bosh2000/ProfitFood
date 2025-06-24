using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.BaseUnit
{
    public class BaseUnitNameMustNotBeEmprtyRules : IBusinessRule
    {
        private readonly string _name;
        public BaseUnitNameMustNotBeEmprtyRules(string name)
        {
            _name = name;
        }
        public string Message => "Наименование обязательно";

        public bool IsBroken() =>string.IsNullOrEmpty(_name);
    }
}
