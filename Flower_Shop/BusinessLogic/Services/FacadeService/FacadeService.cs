using BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Options;
using Ultitity.Options;

namespace BusinessLogic.Services.FacadeService
{
    public class FacadeService : IFacadeService
    {
        public ICategoryService CategoryService { get; }
        public IProductService ProductService { get; }
        public IAuthService AuthService { get; }
        public ICartService CartService { get; }
        public IOrderService OrderService { get; }
        public IUserService UserService { get; }
        public IPaymentService PaymentService { get; }
        public IUtilityService UtilityService { get; }
        public IDashboardService DashboardService { get; }
        public IAddressService AddressService { get; }

        public FacadeService(
            CoreDependencies coreDependencies,
            InfraDependencies infraDependencies,
            IOptions<JwtOptions> jwtOptions,
            IVietQRService vietQRService
        )
        {
            CategoryService = new CategoryService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            );
            ProductService = new ProductService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            );
            ITokenService tokenService = new TokenService(jwtOptions);
            AuthService = new AuthService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper,
                tokenService
            );
            CartService = new CartService(coreDependencies.UnitOfWork, coreDependencies.Mapper);
            OrderService = new OrderService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper,
                infraDependencies.EmailQueue,
                vietQRService
            );
            UserService = new UserService(coreDependencies.UnitOfWork, coreDependencies.Mapper);
            PaymentService = new PaymentService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            );
            UtilityService = new UtilityService();
            DashboardService = new DashboardService(coreDependencies.UnitOfWork);
            AddressService = new AddressService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            );
        }
    }
}
