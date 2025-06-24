using ProfitFood.Model.BusinessRules;
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
        [MaxLength(100)]
        public string Name { get; private set; }

        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

        public OperationResult SetName(string newName)
        {
            var rule = new BaseUnitNameMustNotBeEmprtyRules(newName);
            if (rule.IsBroken())
                return OperationResult.Failure(new Error(nameof(newName),rule.Message));
            Name= newName;
            return OperationResult.Success();
        }

        public static OperationResult<BaseUnit> Create(string name)
        {
            var error=new List<Error>();
            var nameRules = new BaseUnitNameMustNotBeEmprtyRules(name);
            if (nameRules.IsBroken())
                error.Add(new Error(nameof(name),nameRules.Message));
            if (error.Any())
                return OperationResult<BaseUnit>.Failure(error);
            return OperationResult<BaseUnit>.Success(new BaseUnit(name));
        }
    }
}
