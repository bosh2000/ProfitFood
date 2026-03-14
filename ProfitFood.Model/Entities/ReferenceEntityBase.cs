using ProfitFood.Model.DBModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities
{
    /// <summary>
    /// Базовая сущность для справочников.
    /// </summary>
    public abstract class ReferenceEntityBase : EntityBase
    {
        public bool IsActive { get; set; } = true; // Признак активности записи
    }
}