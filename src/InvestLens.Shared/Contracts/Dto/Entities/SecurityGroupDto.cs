namespace InvestLens.Shared.Contracts.Dto.Entities;

public class SecurityGroupDto : DictionaryBaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}