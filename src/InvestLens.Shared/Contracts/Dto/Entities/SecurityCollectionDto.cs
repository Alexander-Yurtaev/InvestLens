namespace InvestLens.Shared.Contracts.Dto.Entities;

public class SecurityCollectionDto : DictionaryBaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int SecurityGroupId { get; set; }
}