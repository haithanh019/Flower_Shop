using BusinessLogic.DTOs.Dashboard;

namespace BusinessLogic.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardStatisticsAsync();
    }
}
