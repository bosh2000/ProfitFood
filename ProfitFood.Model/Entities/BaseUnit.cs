using ProfitFood.Model.BusinessRules;
using ProfitFood.Model.BusinessRules.BaseUnit;
using ProfitFood.Model.BusinessRules.BaseUnitRules;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.DBModel
{
    /// <summary>
    /// Базовая единица измерения
    /// </summary>
    public class BaseUnit : EntityBase
    {
        private BaseUnit(string name)
        {
            this.Name = name;
        }

        [Required]
        [MaxLength(ModelConstant.MAX_LENGTH_NAME_BASEUNIT)]
        public string Name { get; private set; }

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public OperationResult SetName(string newName)
        {
            var errors = CheckName(newName);
            if (errors.Any())
                return OperationResult.Failure(errors);

            Name = newName;
            return OperationResult.Success();
        }

        public static OperationResult<BaseUnit> Create(string name)
        {
            List<Error> errors = CheckName(name);

            if (errors.Any())
                return OperationResult<BaseUnit>.Failure(errors);
            return OperationResult<BaseUnit>.Success(new BaseUnit(name));
        }

        private static List<Error> CheckName(string name)
        {
            var errors = new List<Error>();
            var nameRules = new BaseUnitNameMustNotBeEmprtyRules(name);
            if (nameRules.IsBroken())
                errors.Add(new Error(nameof(name), nameRules.Message));
            var nameLegthRule = new BaseUnitNameLegthRules(name);
            if (nameLegthRule.IsBroken())
                errors.Add(new Error(nameof(name), nameLegthRule.Message));
            return errors;
        }
    }
}