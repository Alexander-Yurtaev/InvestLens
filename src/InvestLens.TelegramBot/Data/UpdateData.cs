namespace InvestLens.TelegramBot.Data;

public record UpdateData
{
    public UpdateData()
    {

    }

    public UpdateData(int nextUpdateId)
    {
        NextUpdateId = nextUpdateId;
    }

    public int NextUpdateId { get; set; }
}