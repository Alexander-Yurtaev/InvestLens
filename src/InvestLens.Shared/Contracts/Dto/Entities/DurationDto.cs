namespace InvestLens.Shared.Contracts.Dto.Entities;

public class DurationDto : DictionaryBaseDto
{
    public int Interval { get; set; }
    public int DurationValue { get; set; }
    public int Days { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;
}