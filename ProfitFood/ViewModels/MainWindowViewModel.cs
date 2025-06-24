using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitFood.UI.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        public string TitleWindows { get; } = "Profit Питание";
        private readonly IProfitDbRepository _profitDbRepository;

        public MainWindowViewModel()
        { }

        public MainWindowViewModel(IProfitDbRepository repository)
        {
            _profitDbRepository = repository;
        }
    }
}