using ProfitFood.Model.DBModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.Models.View
{
    public class ProductItemView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public string FullName { get; set; }

        public string BaseUnitStorage { get; set; }

        public string Group { get; set; }

        public string BaseUnit { get; set; }
    }
}