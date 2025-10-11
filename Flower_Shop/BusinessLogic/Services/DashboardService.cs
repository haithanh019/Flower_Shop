using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Dashboard;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardDto> GetDashboardStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalRevenue = await _unitOfWork
                .Order.GetQueryable()
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            var newOrders = await _unitOfWork
                .Order.GetQueryable()
                .CountAsync(o => o.CreatedAt.Date == today);

            var pendingOrders = await _unitOfWork
                .Order.GetQueryable()
                .CountAsync(o => o.Status == OrderStatus.Pending);

            var newUsers = await _unitOfWork
                .User.GetQueryable()
                .CountAsync(u => u.CreatedAt.Date == today);

            return new DashboardDto
            {
                TotalRevenue = totalRevenue,
                NewOrders = newOrders,
                PendingOrders = pendingOrders,
                NewUsers = newUsers,
            };
        }
    }
}
