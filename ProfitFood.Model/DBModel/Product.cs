using Microsoft.Extensions.Logging;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Name { get; protected set; }

        public string Description { get; protected set; }
        public string FullName { get; protected set; }
        public Guid GroupId { get; protected set; }

        [Required]
        [ForeignKey(nameof(GroupId))]
        public virtual ProductGroup Group { get; protected set; }

        private static OperationResult<Product> Create(string name, string description, string fullname, ProductGroup group, ILogger log)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(name))
                errors.Add(new Error("Product.EmptyName", "Имя продукта обязательно"));

            if (group is null)
                errors.Add(new Error("Product.MissingGroup", "Группа обязательна"));

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