namespace InvestLens.Desktop.UI.Models
{
    public class MenuItem
    {
        public string Icon { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public MenuItemStat[] Stats { get; init; } = [];
    }
}