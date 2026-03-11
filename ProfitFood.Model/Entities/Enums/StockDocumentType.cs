using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Enums
{
    /// <summary>
    /// Тип складского документа.
    /// </summary>
    public enum StockDocumentType
    {
        Receipt = 1,    // Приход продуктов.
        Issue = 2,      // Выдача продуктов.
        WriteOff = 3,   // Списание.
        Inventory = 4   // Инвенторизация.
    }
}