namespace InvestLens.Data.Entities;

public class SecurityEntityWithPageInfo : SecurityEntity
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}