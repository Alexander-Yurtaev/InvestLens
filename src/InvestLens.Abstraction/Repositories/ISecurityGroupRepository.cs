using InvestLens.Data.Entities.Index;

namespace InvestLens.Abstraction.Repositories;

public interface ISecurityGroupRepository : IBaseReadOnlyRepository<SecurityGroup>
{
    Task<IEnumerable<SecurityGroup>> GetAll();
}