namespace BusinessLogic.DTOs.Dashboard
{
    public class DashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public int NewOrders { get; set; }
        public int PendingOrders { get; set; }
        public int NewUsers { get; set; }
    }
}
