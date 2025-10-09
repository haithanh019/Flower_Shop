using BusinessLogic.Services.Interfaces;

namespace BusinessLogic.Services.FacadeService
{
    public interface IFacadeService
    {
        ICategoryService CategoryService { get; }
        IProductService ProductService { get; }
    }
}
