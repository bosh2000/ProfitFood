using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.BaseUnitViewModels
{
    public class BaseUnitItemWindowViewModel : ViewModel
    {
        private string _baseUnitItemName;
        private BaseUnitItemView _editingItem;
        private bool _isEditMode;

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

        public event Action<BaseUnitItemView> BaseUnitUpdated;

        public ICommand AddBaseUnitItemCommand { get; }

        public BaseUnitItemWindowViewModel()
        {
            AddBaseUnitItemCommand = new LambdaCommand(SaveBaseUnitUtem, _ => !string.IsNullOrWhiteSpace(BaseUnitItemName));
        }

        public void InitializeForEdit(BaseUnitItemView baseUnitItem)
        {
            _isEditMode = true;
            BaseUnitItemName = baseUnitItem.Name;
            _editingItem = baseUnitItem;
        }

        private void SaveBaseUnitUtem(object param)
        {
            var baseUnitItem = new BaseUnitItemView
            {
                Id = _isEditMode ? _editingItem.Id : Guid.Empty,
                Name = BaseUnitItemName
            };
            if (_isEditMode)
                BaseUnitUpdated?.Invoke(baseUnitItem);
            else
                BaseUnitCreated?.Invoke(baseUnitItem);
        }
    }
}