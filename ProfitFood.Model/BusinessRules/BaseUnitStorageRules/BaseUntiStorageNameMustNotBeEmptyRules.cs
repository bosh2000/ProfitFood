using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.BaseUnitStorage
{
    internal class BaseUntiStorageNameMustNotBeEmptyRules : IBusinessRule
    {
        private readonly string _name;
        public BaseUntiStorageNameMustNotBeEmptyRules(string name)
        {
            _name = name;
        }
        public string Message => "Наименование не может быть пустым";

        public bool IsBroken() => string.IsNullOrWhiteSpace(_name);
    }
}
