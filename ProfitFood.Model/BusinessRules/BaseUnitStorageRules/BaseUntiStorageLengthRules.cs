using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.BusinessRules.BaseUnitStorageRules
{
    internal class BaseUntiStorageLengthRules : IBusinessRule
    {
        private string _name;
        public BaseUntiStorageLengthRules(string name)
        {
            _name=name;
        }
        public string Message => $"Длина наименование не может быть больше {ModelConstant.MAX_LENGTH_NAME_BASEUNIT_STORAGE}";

        public bool IsBroken() => _name.Length > ModelConstant.MAX_LENGTH_NAME_BASEUNIT_STORAGE;
    }
}
