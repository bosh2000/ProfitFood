using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.Model.Infrastructure
{
    public interface IBusinessRule
    {
        bool IsBroken(); // Проверяет, нарушено ли правило
        string Message { get; } // Сообщение об ошибке
    }
}
