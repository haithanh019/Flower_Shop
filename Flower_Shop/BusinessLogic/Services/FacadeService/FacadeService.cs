using BusinessLogic.Services.Interfaces;

namespace BusinessLogic.Services.FacadeService
{
    public class FacadeService : IFacadeService
    {
        public ICategoryService CategoryService { get; }
        public IProductService ProductService { get; }

        public FacadeService(CoreDependencies coreDependencies, InfraDependencies infraDependencies)
        {
            CategoryService = new CategoryService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            );
            ProductService = new ProductService(
                coreDependencies.UnitOfWork,
                coreDependencies.Mapper
            ); // Thêm dòng này
        }
    }
}
