﻿using Microsoft.Extensions.Logging;
using ProfitFood.Model.BusinessRules.Product;
using ProfitFood.Model.BusinessRules.ProductGroupRules;
using ProfitFood.Model.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProfitFood.Model.DBModel
{
    public class Product : EntityBase
    {
        private Product(string Name, string Description, string FullName, Guid GroupId)
        {
            this.Name = Name;
            this.Description = Description;
            this.FullName = FullName;
            this.GroupId = GroupId;
        }

        [Required]
        public string Name { get; private set; }

        public string Description { get; private set; }
        public string FullName { get; private set; }
        public Guid GroupId { get; protected set; }

        public Guid BaseUnitStorageId { get; protected set; }
        public virtual BaseUnitStorage BaseUnitStorage { get; protected set; }

        [Required]
        [ForeignKey(nameof(GroupId))]
        public virtual ProductGroup Group { get; protected set; }

        public Guid BaseUnitId { get; protected set; }

        [ForeignKey(nameof(BaseUnitId))]
        public virtual BaseUnit BaseUnit { get; protected set; }

        private static OperationResult<Product> Create(string name, string description, string fullname, ProductGroup group, ILogger log)
        {
            var errors = new List<Error>();

            var ruleEmptyName = new ProductNameMustNotBeEmptyRule(name);
            if (ruleEmptyName.IsBroken())
                errors.Add(new Error(nameof(name), ruleEmptyName.Message));

            var ruleGroupName = new ProductGroupMustNotBeNullRules(group);
            if (ruleGroupName.IsBroken())
                errors.Add(new Error(nameof(group), ruleGroupName.Message));

            if (errors.Any())
                return OperationResult<Product>.Failure(errors);

            return OperationResult<Product>.Success(new Product(
                name.Trim(),
                description?.Trim(),
                fullname,
                group.Id));
        }
    }
}