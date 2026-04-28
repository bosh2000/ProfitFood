using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Dto.References.ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.Models.References;
using ProfitFood.UI.ViewModels;
using ProfitFood.UI.ViewModels.Reference;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProfitFood.UI.ViewModels.Reference.ProductCategories
{
    public sealed class ProductCategoriesViewModel : ViewModelBase, IInitializableViewModel
    {
        private readonly IProductCategoryAppService _service;

        private ProductCategoryTreeItemViewModel? _selectedCategory;
        private ProductCategoryEditModel _editModel = new();
        private string _statusMessage = "Готово";
        private bool _isBusy;
        private bool _isCreateChildMode;

        public ProductCategoriesViewModel(IProductCategoryAppService service)
        {
            _service = service;

            Categories = new ObservableCollection<ProductCategoryTreeItemViewModel>();
            ParentCategoryOptions = new ObservableCollection<ProductCategoryLookupDto>();

            AddRootCommand = new RelayCommand(_ => AddRoot(), _ => !IsBusy);
            AddChildCommand = new RelayCommand(_ => AddChild(), _ => SelectedCategory != null && !IsBusy);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedCategory != null && !IsBusy);
            RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !IsBusy);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => !IsBusy);
        }

        public ObservableCollection<ProductCategoryTreeItemViewModel> Categories { get; }

        public ObservableCollection<ProductCategoryLookupDto> ParentCategoryOptions { get; }

        public ProductCategoryTreeItemViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    if (value != null)
                    {
                        EditModel = new ProductCategoryEditModel
                        {
                            Id = value.Id,
                            Name = value.Name,
                            ParentCategoryId = value.ParentCategoryId,
                            SortOrder = value.SortOrder
                        };

                        StatusMessage = "Категория выбрана.";
                    }
                }
            }
        }

        public ProductCategoryEditModel EditModel
        {
            get => _editModel;
            set => SetProperty(ref _editModel, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand AddRootCommand { get; }
        public ICommand AddChildCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public async Task InitializeAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                Categories.Clear();

                var tree = await _service.GetTreeAsync();
                foreach (var item in tree)
                    Categories.Add(MapTreeItem(item));

                ParentCategoryOptions.Clear();
                ParentCategoryOptions.Add(new ProductCategoryLookupDto
                {
                    Id = null,
                    Name = "Без родителя"
                });

                var lookup = await _service.GetLookupAsync();
                foreach (var item in lookup)
                    ParentCategoryOptions.Add(item);

                StatusMessage = $"Загружено категорий: {ParentCategoryOptions.Count - 1}.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddRoot()
        {
            SelectedCategory = null;

            EditModel = new ProductCategoryEditModel
            {
                ParentCategoryId = null,
                SortOrder = Categories.Count + 1
            };

            StatusMessage = "Создание корневой категории.";
        }

        private void AddChild()
        {
            if (SelectedCategory == null)
                return;

            EditModel = new ProductCategoryEditModel
            {
                ParentCategoryId = SelectedCategory.Id,
                SortOrder = SelectedCategory.Children.Count + 1
            };

            StatusMessage = $"Создание дочерней категории для: {SelectedCategory.Name}.";
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(EditModel.Name))
            {
                StatusMessage = "Укажите наименование категории.";
                return;
            }

            var dto = new ProductCategorySaveDto
            {
                Id = EditModel.Id,
                Name = EditModel.Name.Trim(),
                ParentCategoryId = EditModel.ParentCategoryId,
                SortOrder = EditModel.SortOrder
            };

            await _service.SaveAsync(dto);
            await LoadAsync();

            StatusMessage = "Категория сохранена.";
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null)
                return;

            var canDelete = await _service.CanDeleteAsync(SelectedCategory.Id);
            if (!canDelete)
            {
                StatusMessage = "Категорию нельзя удалить: есть дочерние категории или продукты.";
                return;
            }

            await _service.DeleteAsync(SelectedCategory.Id);
            await LoadAsync();

            EditModel = new ProductCategoryEditModel();
            StatusMessage = "Категория удалена.";
        }

        private void Cancel()
        {
            if (SelectedCategory != null)
            {
                EditModel = new ProductCategoryEditModel
                {
                    Id = SelectedCategory.Id,
                    Name = SelectedCategory.Name,
                    ParentCategoryId = SelectedCategory.ParentCategoryId,
                    SortOrder = SelectedCategory.SortOrder
                };
            }
            else
            {
                EditModel = new ProductCategoryEditModel();
            }

            StatusMessage = "Изменения отменены.";
        }

        private static ProductCategoryTreeItemViewModel MapTreeItem(ProductCategoryTreeDto dto)
        {
            var item = new ProductCategoryTreeItemViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                ParentCategoryId = dto.ParentCategoryId,
                SortOrder = dto.SortOrder
            };

            foreach (var child in dto.Children
                         .OrderBy(x => x.SortOrder)
                         .ThenBy(x => x.Name))
            {
                item.Children.Add(MapTreeItem(child));
            }

            return item;
        }
    }
}