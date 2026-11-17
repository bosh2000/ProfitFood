using ProfitFood.Applications.Dto.References;
using ProfitFood.Applications.Services.Interfaces;
using ProfitFood.UI.Models.References;
using ProfitFood.UI.ViewModels.Reference.Base;
using System.Collections.ObjectModel;

namespace ProfitFood.UI.ViewModels.Reference.Products
{
    /// <summary>
    /// Справочник продуктов питания.
    /// </summary>
    public sealed class ProductsReferenceViewModel : ReferenceCrudViewModelBase<ProductDto, ProductDto>
    {
        private readonly IProductAppService _service;
        private readonly IProductCategoryAppService _productCategoryService;
        private readonly IUnitAppService _unitService;

        /// <summary>
        /// Инициализирует справочник продуктов.
        /// </summary>
        /// <param name="service">Сервис справочника продуктов.</param>
        /// <param name="productCategoryService">Сервис категорий продуктов для выпадающего списка.</param>
        /// <param name="unitService">Сервис единиц измерения для выпадающих списков.</param>
        /// <exception cref="ArgumentNullException">Возникает, если сервисы не переданы.</exception>
        public ProductsReferenceViewModel(
            IProductAppService service,
            IProductCategoryAppService productCategoryService,
            IUnitAppService unitService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _productCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));

            Title = "Продукты";
            ProductCategoryOptions = new ObservableCollection<ProductCategoryLookupDto>();
            UnitOptions = new ObservableCollection<UnitItemDto>();
        }

        /// <summary>
        /// Категории продуктов, доступные для выбора.
        /// </summary>
        public ObservableCollection<ProductCategoryLookupDto> ProductCategoryOptions { get; }

        /// <summary>
        /// Единицы измерения, доступные для выбора.
        /// </summary>
        public ObservableCollection<UnitItemDto> UnitOptions { get; }

        /// <summary>Загружает продукты и справочники для выпадающих списков.</summary>
        public override async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                await LoadOptionsAsync();
                ReplaceItems(await _service.GetAllAsync());

                StatusMessage = $"Загружено записей: {Items.Count}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка загрузки: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Ищет продукты по введённому тексту.</summary>
        protected override async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                await LoadOptionsAsync();
                ReplaceItems(await _service.SearchAsync(SearchText));

                StatusMessage = $"Найдено записей: {Items.Count}.";
            }
            catch (Exception exception)
            {
                StatusMessage = $"Ошибка поиска: {exception.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override ProductDto CreateNewEditModel() => new() { SortOrder = 1 };

        protected override ProductDto BuildEditModel(ProductDto item) => new()
        {
            Id = item.Id,
            Name = item.Name,
            FullName = item.FullName,
            Article = item.Article,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryName = item.ProductCategoryName,
            BaseUnitId = item.BaseUnitId,
            BaseUnitName = item.BaseUnitName,
            StorageUnitId = item.StorageUnitId,
            StorageUnitName = item.StorageUnitName,
            SortOrder = item.SortOrder,
            IsPerishable = item.IsPerishable,
            MinStock = item.MinStock
        };

        protected override Task SaveCoreAsync(ProductDto model) => _service.SaveAsync(model);

        protected override Task DeleteCoreAsync(ProductDto item) => _service.DeleteAsync(item);

        /// <summary>
        /// Перезаполняет списки категорий и единиц измерения для карточки записи.
        /// </summary>
        private async Task LoadOptionsAsync()
        {
            var categories = await _productCategoryService.GetLookupAsync();
            var units = await _unitService.GetAllAsync();

            ProductCategoryOptions.Clear();
            foreach (var category in categories)
            {
                ProductCategoryOptions.Add(category);
            }

            UnitOptions.Clear();
            foreach (var unit in units)
            {
                UnitOptions.Add(unit);
            }
        }
    }
}
