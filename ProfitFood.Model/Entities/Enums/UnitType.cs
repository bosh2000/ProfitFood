using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Enums
{
    /// <summary>
    /// Тип единицы измерения.
    /// </summary>
    public enum UnitType
    {
        Weigth = 1,  // Весовые единицы (г,кг)
        Volume = 2,  // Объемные единицы (л, мл)
        Piece = 3    // Штучные единицы (шт)
    }
}