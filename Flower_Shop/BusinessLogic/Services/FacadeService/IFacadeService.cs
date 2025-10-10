using BusinessLogic.Services.Interfaces;

namespace BusinessLogic.Services.FacadeService
{
    public interface IFacadeService
    {
        ICategoryService CategoryService { get; }
        IProductService ProductService { get; }
        IAuthService AuthService { get; }
        ICartService CartService { get; }
        IOrderService OrderService { get; }
        IUserService UserService { get; }
        IPaymentService PaymentService { get; }
        IUtilityService UtilityService { get; }
        IDashboardService DashboardService { get; }
    }
}
