using Microsoft.Extensions.Logging;
using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.DBModel
{
    public class ProductGroup : EntityBase
    {
        private ProductGroup(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }


        [Required]
        [MaxLength(200)]
        public string Name { get; protected set; }

        [MaxLength(1000)]
        public string? Description { get; protected set; }

        public virtual ICollection<Product> Products { get; protected set; } = new List<Product>();

        private static OperationResult<ProductGroup> Create(string? name, string? desc)
        {
            var error=new List<Error>();

            if (string.IsNullOrEmpty(name)) 
                error.Add(new Error(nameof(name),"Имя группы обязательно"));
            if (name?.Length > 200)
                error.Add(new Error(nameof(name), "Длина наименования более 200 символов"));
            if (error.Any())
                return OperationResult<ProductGroup>.Failure(error);
            return OperationResult<ProductGroup>
                .Success(new ProductGroup(name,desc));
        }
    }
}