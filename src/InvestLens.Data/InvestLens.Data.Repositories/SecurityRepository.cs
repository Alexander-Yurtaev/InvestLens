using InvestLens.Abstraction.Repositories;
using InvestLens.Data.DataContext;
using InvestLens.Data.Entities;
using InvestLens.Shared.Repositories;
using Microsoft.Extensions.Logging;

namespace InvestLens.Data.Repositories
{
    public class SecurityRepository : BaseRepository<Security>, ISecurityRepository
    {
        public SecurityRepository(InvestLensDataContext context, ILogger<SecurityRepository> logger) : base(context, logger)
        {
        }
    }
}