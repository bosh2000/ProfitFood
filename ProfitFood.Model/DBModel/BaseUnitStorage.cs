using ProfitFood.Model;
using ProfitFood.Model.BusinessRules;
using ProfitFood.Model.BusinessRules.BaseUnitStorage;
using ProfitFood.Model.BusinessRules.BaseUnitStorageRules;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.DBModel
{
    public class BaseUnitStorage : EntityBase
    {
        private BaseUnitStorage(string name, string Description)
        {
            this.Name = name;
            this.Description = Description;
        }

        [Required]
        [MaxLength(ModelConstant.MAX_LENGTH_NAME_BASEUNIT_STORAGE)]
        public string Name { get; private set; }

        public string Description { get; private set; }

        public OperationResult SetName(string newName)
        {
            var errors = CheckName(newName);
            if (errors.Any())
                return OperationResult.Failure(errors);
            this.Name = newName;
            return OperationResult.Success();
        }

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public static OperationResult<BaseUnitStorage> Create(string name, string description)
        {
            List<Error> errors = CheckName(name);
            if (errors.Any())
                return OperationResult<BaseUnitStorage>.Failure(errors);
            return OperationResult<BaseUnitStorage>
                .Success(new BaseUnitStorage(name, description));
        }

        private static List<Error> CheckName(string name)
        {
            var errors = new List<Error>();
            var rulesName = new BaseUntiStorageNameMustNotBeEmptyRules(name);
            if (rulesName.IsBroken())
                errors.Add(new Error(nameof(name), rulesName.Message));
            var rulesNameLegth = new BaseUntiStorageLengthRules(name);
            if (!rulesNameLegth.IsBroken())
                errors.Add(new Error(nameof(name), rulesNameLegth.Message));
            return errors;
        }
    }
}