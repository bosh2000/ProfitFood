using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Domain.Entities.Enums
{
    /// <summary>
    /// Статус документа меню-требования.
    /// </summary>
    public enum DailyMenuStatus
    {
        Draft = 0,      // Черновик.
        Calculated = 1, // Выполнен расчет продуктов.
        Approved = 2,    // Меню утверждено.
        Closed = 3,      // Меню закрыто.
    }
}