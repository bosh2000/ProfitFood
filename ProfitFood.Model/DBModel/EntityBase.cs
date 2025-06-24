using ProfitFood.Model.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.DBModel
{
    public abstract class EntityBase
    {
        [Key]
        public Guid Id { get; protected set; } = Guid.NewGuid();

        // Общие для всех сущностей методы
        protected static void CheckRule(IBusinessRule rule)
        {
            if (rule.IsBroken())
                throw new ArgumentException();
        }
    }
}
