using InvestLens.Data.Entities.Dictionaries;

namespace InvestLens.Data.Core.Abstraction.Repositories.Dictionaries;

public interface ISecurityTypeRepository : IBaseReadOnlyRepository<SecurityTypeEntity>
{
    Task<IEnumerable<SecurityTypeEntity>> GetAll();
}