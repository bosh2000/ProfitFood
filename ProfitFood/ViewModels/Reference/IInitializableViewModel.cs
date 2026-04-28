using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.ViewModels.Reference
{
    public interface IInitializableViewModel
    {
        Task InitializeAsync();
    }
}