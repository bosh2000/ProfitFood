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
    internal class ProductGroup
    {

        private ProductGroup(string name, string desc)
        {
            this.Name = name;
            this.Description = desc;
        }


        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }



        static Responce<ProductGroup> Create(string name, string desc,ILogger log) {
            if (string.IsNullOrEmpty(name)) return new Responce<ProductGroup>(log)
            {
                IsSuccess = false,
                ErrorMessage = "Не заполнено наименование"
            };
            return new Responce<ProductGroup>(log){ Data = new ProductGroup(name, desc) };
        }
    }
}
