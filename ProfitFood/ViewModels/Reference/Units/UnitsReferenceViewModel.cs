using ProfitFood.Domain.ModelsViewModels;
using ProfitFood.Infrastructure.Services.Interfaces;
using ProfitFood.UI.Views.Reference.Base;

namespace ProfitFood.UI.ViewModels.Reference.Units
{
    public sealed class UnitsReferenceViewModel
        : ReferenceCrudViewModelBase<UnitListItemViewModel, UnitEditModel>
    {
        private readonly IUnitAppService _unitAppService;

        public UnitsReferenceViewModel(IUnitAppService unitAppService)
        {
            _unitAppService = unitAppService;
            Title = "Единицы измерения";

            UnitTypes = new List<string>
        {
            "Базовая",
            "Складская",
            "Технологическая"
        };
        }

        public IReadOnlyCollection<string> UnitTypes { get; }

        public override async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                var items = await _unitAppService.GetAllAsync();
                ReplaceItems(items);

                StatusMessage = $"Загружено записей: {Items.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override async Task SearchAsync()
        {
            try
            {
                IsBusy = true;

                var items = string.IsNullOrWhiteSpace(SearchText)
                    ? await _unitAppService.GetAllAsync()
                    : await _unitAppService.SearchAsync(SearchText);

                ReplaceItems(items);
                StatusMessage = $"Найдено записей: {Items.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка поиска: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override UnitEditModel CreateNewEditModel()
        {
            return new UnitEditModel
            {
                BaseFactor = 1m,
                IsBase = false,
                UnitType = "Базовая"
            };
        }

        protected override UnitEditModel BuildEditModel(UnitListItemViewModel item)
        {
            return new UnitEditModel
            {
                Id = item.Id,
                Name = item.Name,
                ShortName = item.ShortName,
                UnitType = item.UnitTypeName,
                BaseFactor = 1m,
                IsBase = item.IsBase
            };
        }

        protected override async Task SaveCoreAsync(UnitEditModel model)
        {
            await _unitAppService.SaveAsync(model);
        }

        protected override async Task DeleteCoreAsync(UnitListItemViewModel item)
        {
            await _unitAppService.DeleteAsync(item.Id);
        }
    }
}