using Microsoft.Extensions.Logging;
using ProfitFood.Model.BusinessRules;
using ProfitFood.Model.BusinessRules.ProductGroupRules;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProfitFood.Model.DBModel
{
    public class ProductGroup : EntityBase
    {
        private ProductGroup(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        [MaxLength(1000)]
        public string? Description { get; private set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; private set; }

        public virtual ICollection<Product> Products { get; protected set; } = new List<Product>();

        public OperationResult SetName(string newName)
        {
            List<Error> error = CheckName(newName);

            if (error.Any())
                return OperationResult<ProductGroup>.Failure(error);
            Name = newName;
            return OperationResult.Success();
        }

        private static List<Error> CheckName(string name)
        {
            var error = new List<Error>();

            var ruleName = new ProductGroupNameMustNotBeEmpty(name);
            if (ruleName.IsBroken())
                error.Add(new Error(nameof(name), ruleName.Message));
            var ruleLength = new ProductGroupNameLengthRules(name);
            if (ruleLength.IsBroken())
                error.Add(new Error(nameof(name), ruleLength.Message));
            return error;
        }

        private static OperationResult<ProductGroup> Create(string? name, string? desc)
        {
            var error = CheckName(name);

            if (error.Any())
                return OperationResult<ProductGroup>.Failure(error);
            return OperationResult<ProductGroup>
                .Success(new ProductGroup(name, desc));
        }
    }
}