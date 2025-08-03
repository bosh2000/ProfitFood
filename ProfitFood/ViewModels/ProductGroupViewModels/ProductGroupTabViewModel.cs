using AutoMapper;
using ProfitFood.DAL.Repository.Interfaces;
using ProfitFood.Model.DBModel;
using ProfitFood.Model.Infrastructure;
using ProfitFood.UI.Infrastructure.Commands;
using ProfitFood.UI.Models.View;
using ProfitFood.UI.ViewModels.Base;
using ProfitFood.UI.ViewModels.BaseUnitStorageViewModels;
using ProfitFood.UI.Views.BaseUnitStorage;
using ProfitFood.UI.Views.ProductGroup;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.ProductGroupViewModels
{
    public class ProductGroupTabViewModel : ViewModel
    {
        private readonly IProfitDbRepository _repository;
        private readonly IMapper _mapper;

        public ICommand AddProductGroupCommand { get; }
        public ICommand DeleteProductGroupCommand { get; }
        public ICommand EditProductGroupCommand { get; }
        private ProductGroupItemView _selectedProductGroup;
        public string SearchProductGroup { get; set; }

        public ProductGroupItemView SelectedProductGroup
        {
            get => _selectedProductGroup;
            set
            {
                _selectedProductGroup = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ProductGroupItemView> ProductGroups { get; } = new ObservableCollection<ProductGroupItemView>();

        public ProductGroupTabViewModel(IProfitDbRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
            LoadProductGroupItems();
            AddProductGroupCommand = new LambdaCommand(AddProductGroup);
            DeleteProductGroupCommand = new LambdaCommand(DeleteProductGroup, CanEditDeleteProductGroup);
            EditProductGroupCommand = new LambdaCommand(EditProductGroup, CanEditDeleteProductGroup);
        }

        private async void LoadProductGroupItems()
        {
            var productGroupList = await _repository.ProductGroupRepository.ToListAsync();
            foreach (var productGroup in productGroupList)
            {
                ProductGroups.Add(_mapper.Map<ProductGroupItemView>(productGroup));
            }
        }

        private void AddProductGroup(object param)
        {
            var addProductGroupWindows = new ProductGroupsItemWindow();
            var viewModel = (ProductGroupItemWindowViewModel)addProductGroupWindows.DataContext;
            viewModel.CreateProductGroup += async productGroup =>
            {
                var result = ProductGroup.Create(productGroup.Name, productGroup.Description);
                if (!result.IsSuccess)
                {
                    ShowErrors(result.Errors);
                    return;
                }
                var productGroupCreated = await _repository.ProductGroupRepository.CreateASync(result.Value);
                var productGroupView = _mapper.Map<ProductGroupItemView>(productGroupCreated);
                ProductGroups.Add(productGroupView);
                SelectedProductGroup = productGroup;
                addProductGroupWindows.Close();
            };
            addProductGroupWindows.Owner = Application.Current.MainWindow;
            addProductGroupWindows.ShowDialog();
        }

        private void ShowErrors(IReadOnlyCollection<Error> errors)
        {
        }

        private async void DeleteProductGroup(object param)
        {
            MessageBoxResult result = MessageBox.Show(
                        "Вы уверенны что хотите удалить запись?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var productGroup = await _repository.ProductGroupRepository.FirstOfDefaultAsync(x => x.Id == SelectedProductGroup.Id);
                if (productGroup != null)
                {
                    await _repository.ProductGroupRepository.DeleteAsync(productGroup);
                    ProductGroups.Remove(SelectedProductGroup);
                }
            }
        }

        private void EditProductGroup(object param)
        {
            if (SelectedProductGroup == null) return;

            var productGroupItemWindow = new ProductGroupsItemWindow();
            var viewModel = (ProductGroupItemWindowViewModel)productGroupItemWindow.DataContext;
            viewModel.InitializeEdit(SelectedProductGroup);
            viewModel.UpdateProductGroup += async productGroup =>
            {
                var item = await _repository.ProductGroupRepository.FirstOfDefaultAsync(x => x.Id == productGroup.Id);
                if (item != null)
                {
                    var result = item.SetName(productGroup.Name);
                    if (!result.IsSuccess)
                    {
                        ShowErrors(result.Errors);
                        return;
                    }
                    result = item.SetDescription(productGroup.Description);
                    if (!result.IsSuccess)
                    {
                        ShowErrors(result.Errors);
                        return;
                    }
                    await _repository.ProductGroupRepository.UpdateAsync(item);
                    var index = ProductGroups.IndexOf(SelectedProductGroup);
                    ProductGroups[index] = _mapper.Map<ProductGroupItemView>(item);
                }
                productGroupItemWindow.Close();
            };
            productGroupItemWindow.Owner = Application.Current.MainWindow;
            productGroupItemWindow.ShowDialog();
        }

        private bool CanEditDeleteProductGroup(object param)
        {
            return SelectedProductGroup != null;
        }
    }
}