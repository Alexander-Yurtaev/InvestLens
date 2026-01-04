using InvestLens.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace InvestLens.Abstraction.Services;

public interface IDataService
{
    Task<List<Security>> GetSecurities();
}