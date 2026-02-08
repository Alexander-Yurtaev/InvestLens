namespace InvestLens.Shared.Models.Dictionaries;

public class DurationModel : BaseModel
{
    public int Interval { get; set; }
    public int DurationValue { get; set; }
    public string Title { get; set; } = string.Empty;
}