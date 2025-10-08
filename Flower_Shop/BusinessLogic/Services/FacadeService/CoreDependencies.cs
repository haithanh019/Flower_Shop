using AutoMapper;
using DataAccess.Data;
using DataAccess.UnitOfWork;

namespace BusinessLogic.Services.FacadeService
{
    public class CoreDependencies
    {
        public IUnitOfWork UnitOfWork { get; }
        public IMapper Mapper { get; }

        public CoreDependencies(IUnitOfWork unitOfWork, IMapper mapper)
        {
            UnitOfWork = unitOfWork;
            Mapper = mapper;
        }
    }
}
