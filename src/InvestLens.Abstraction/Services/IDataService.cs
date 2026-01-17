using InvestLens.Abstraction.Repositories;
using InvestLens.Data.Entities;

namespace InvestLens.Abstraction.Services;

public interface IDataService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="page">номер страницы</param>
    /// <param name="pageSize">количество элементов на странице</param>
    /// <param name="sort">сортиовка (опционально)</param>
    /// <param name="filter">фильтрация (опционально)</param>
    /// <returns></returns>
    Task<IGetResult<Security, Guid>> GetSecurities(int page, int pageSize, string? sort = "", string? filter = "");
}