using InvestLens.Data.Entities.Dictionaries;

namespace InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;

public interface ISecurityGroupRepository : IBaseReadOnlyRepository<SecurityGroupEntity>
{
    Task<IEnumerable<SecurityGroupEntity>> GetAll();
}