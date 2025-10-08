namespace BusinessLogic.Services.FacadeService
{
    public class FacadeService : IFacadeService
    {
        //public IServiceRequestService ServiceRequestService { get; }
        //public IMaterialService MaterialService { get; }


        public FacadeService(CoreDependencies coreDeps, InfraDependencies infraDeps)
        {
            //CategoryService = new CategoryService(coreDeps.UnitOfWork, coreDeps.Mapper);
            //BrandService = new BrandService(coreDeps.UnitOfWork, coreDeps.Mapper);
            //ImageService = new ImageService(coreDeps.UnitOfWork);
        }
    }
}
