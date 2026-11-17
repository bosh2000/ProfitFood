using ProfitFood.Applications.Dto.Recipes;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IRecipeCardAppService
    {
        Task<IReadOnlyCollection<RecipeCardListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<RecipeCardListItemDto>> SearchAsync(string searchText, CancellationToken cancellationToken = default);

        /// <summary>Загружает техкарту вместе с редактируемой версией и ингредиентами.</summary>
        Task<RecipeCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Сохраняет техкарту, её версию и ингредиенты. Возвращает идентификатор техкарты.</summary>
        Task<Guid> SaveAsync(RecipeCardDto model, CancellationToken cancellationToken = default);

        /// <summary>Удаляет техкарту вместе с версиями и ингредиентами.</summary>
        Task DeleteAsync(RecipeCardDto model, CancellationToken cancellationToken = default);
    }
}
