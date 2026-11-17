using ProfitFood.Applications.Dto.Menus;

namespace ProfitFood.Applications.Services.Interfaces
{
    public interface IDailyMenuAttendanceAppService
    {
        /// <summary>Возвращает посещаемость по группам детей для меню на дату.</summary>
        Task<IReadOnlyCollection<DailyMenuAttendanceDto>> GetByMenuAsync(Guid dailyMenuId, CancellationToken cancellationToken = default);

        /// <summary>Сохраняет посещаемость по группам детей для меню на дату.</summary>
        Task SaveAsync(Guid dailyMenuId, IReadOnlyCollection<DailyMenuAttendanceDto> rows, CancellationToken cancellationToken = default);
    }
}
