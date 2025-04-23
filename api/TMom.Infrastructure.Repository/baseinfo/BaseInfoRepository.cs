using TMom.Domain.IRepository;
using TMom.Domain.Model;
using TMom.Domain.Model.Entity;

namespace TMom.Infrastructure.Repository
{
	/// <summary>
	/// BaseInfoRepository
	/// </summary>
    public class BaseInfoRepository : BaseRepository<BaseInfo, int>, IBaseInfoRepository
    {
        public BaseInfoRepository(IUnitOfWork unitOfWork): base(unitOfWork)
        {
        }
    }
}