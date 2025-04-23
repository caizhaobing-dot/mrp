using TMom.Domain.Model;
using SqlSugar;
using System.Data;
using System.Linq.Expressions;
using TMom.Domain.Model.Entity;

namespace TMom.Domain.IRepository
{
	/// <summary>
	/// IBaseInfoRepository
	/// </summary>
    public interface IBaseInfoRepository : IBaseRepository<BaseInfo, int>
    {
    }
}