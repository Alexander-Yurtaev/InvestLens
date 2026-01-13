namespace InvestLens.Data.Entities;

public abstract class BaseEntity<TKey> where TKey : struct
{
    public TKey Id { get; set; }
}