using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.BaseUnitStorageViewModels
{
    public class BaseUnitStorageItemWindowViewModel : ViewModel
    {
        private string _baseUnitStorageItemName;
        private string _baseUnitStorageItemDescription;

        public string BaseUnitStorageItemName
        {
            get => _baseUnitStorageItemName;
            set
            {
                _baseUnitStorageItemName = value;
                OnPropertyChanged();
            }
        }

        public string BaseUnitStorageItemDescription
        {
            get => _baseUnitStorageItemDescription;
            set
            {
                _baseUnitStorageItemDescription = value;
                OnPropertyChanged();
            }
        }

        public event Action<BaseUnitStorageItemView> BaseUnitStorageItemCreate;

        public event Action<BaseUnitStorageItemView> BaseUnitStorageItemUpdate;

        private bool _isEditMode;
        private BaseUnitStorageItemView _editBaseUnitStorageItem;

        public ICommand SaveBaseUnitStorageCommand { get; }

        public BaseUnitStorageItemWindowViewModel()
        {
            SaveBaseUnitStorageCommand = new LambdaCommand(SaveBaseUnitStorage, CanSave);
        }

        public void InitializeEdit(BaseUnitStorageItemView viewModel)
        {
            _isEditMode = true;
            _editBaseUnitStorageItem = viewModel;
            BaseUnitStorageItemDescription = viewModel.Description;
            BaseUnitStorageItemName = viewModel.Name;
        }

        private void SaveBaseUnitStorage(object param)
        {
            var baseUnitStorage = new BaseUnitStorageItemView
            {
                Description = BaseUnitStorageItemDescription,
                Name = BaseUnitStorageItemName,
                Id = _isEditMode ? _editBaseUnitStorageItem.Id : Guid.Empty,
            };
            if (_isEditMode)
                BaseUnitStorageItemUpdate?.Invoke(baseUnitStorage);
            else
                BaseUnitStorageItemCreate?.Invoke(baseUnitStorage);
        }

        private bool CanSave(object param)
        {
            return !string.IsNullOrWhiteSpace(BaseUnitStorageItemName) && !string.IsNullOrWhiteSpace(BaseUnitStorageItemDescription);
        }
    }
}