using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ProfitFood.UI.ViewModels.BaseUnitViewModels
{
    public class BaseUnitItemWindowViewModel : ViewModel
    {
        private string _baseUnitItemName;

        public string BaseUnitItemName
        {
            get => _baseUnitItemName;
            set
            {
                _baseUnitItemName = value;
                OnPropertyChanged();
            }
        }

        public event Action<BaseUnitItemView> BaseUnitCreated;

        public ICommand AddBaseUnitItemCommand { get; }

        public BaseUnitItemWindowViewModel()
        {
            AddBaseUnitItemCommand = new LambdaCommand(SaveBaseUnitUtem, _ => !string.IsNullOrWhiteSpace(BaseUnitItemName));
        }

        private void SaveBaseUnitUtem(object param)
        {
            var baseUnitItem = new BaseUnitItemView
            {
                Name = BaseUnitItemName,
            };
            BaseUnitCreated?.Invoke(baseUnitItem);
        }
    }
}