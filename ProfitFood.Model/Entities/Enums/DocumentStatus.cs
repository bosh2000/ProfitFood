using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Enums
{
    /// <summary>
    /// Общий статус документов.
    /// </summary>
    public enum DocumentStatus
    {
        Draft = 0,     // Черновик
        Posted = 1,    // Проведен
        Closed = 2,    // Закрыт
        Cancelled = 3  // Отменен
    }
}