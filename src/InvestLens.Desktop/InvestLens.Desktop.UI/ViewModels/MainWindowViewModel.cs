using System.Collections.Generic;
using System.Collections.ObjectModel;
using InvestLens.Desktop.UI.Models;

namespace InvestLens.Desktop.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Icon => "📊";
        public string Title => "InvestLens";
        public string Description => "Ваш персональный гид в мире инвестиций";
        public ReadOnlyObservableCollection<MenuItem> MenuItems { get; } = new ReadOnlyObservableCollection<MenuItem>(GetMenuItems());
        public ReadOnlyObservableCollection<Dictionary> Dictionaries { get; } = new ReadOnlyObservableCollection<Dictionary>(GetDictionaries());


        private static ObservableCollection<MenuItem> GetMenuItems()
        {
            List<MenuItem> list =
            [
                new MenuItem
                {
                    Icon = "📈",
                    Title = "Ценные бумаги",
                    Description =
                        "Полный список доступных ценных бумаг с актуальной информацией о доходности, рисках и текущей стоимости",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "📊",
                            Title = "150+ инструментов"
                        },
                        new MenuItemStat
                        {
                            Icon = "🔄",
                            Title = "Обновление: сегодня"
                        }
                    ]
                },

                new MenuItem
                {
                    Icon = "🏢",
                    Title = "Эмитенты",
                    Description = "Информация о компаниях-эмитентах, их финансовых показателях, рейтингах и истории",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "🏭",
                            Title = "45 эмитентов"
                        },
                        new MenuItemStat
                        {
                            Icon = "⭐",
                            Title = "Топ-10 рейтинг"
                        }
                    ]
                },

                new MenuItem
                {
                    Icon = "🏭",
                    Title = "Отрасли",
                    Description = "Классификация отраслей экономики, тренды развития и ключевые показатели",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "📑",
                            Title = "12 секторов"
                        },
                        new MenuItemStat
                        {
                            Icon = "📈",
                            Title = "Динамика роста"
                        }
                    ]
                },

                new MenuItem
                {
                    Icon = "💱",
                    Title = "Валюты",
                    Description = "Курсы валют, кросс-курсы, динамика изменений и аналитические прогнозы",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "💵",
                            Title = "30+ валют"
                        },
                        new MenuItemStat
                        {
                            Icon = "📊",
                            Title = "Графики"
                        }
                    ]
                },

                new MenuItem
                {
                    Icon = "🏛️",
                    Title = "Биржи",
                    Description = "Информация о торговых площадках, режимах торгов и биржевых индексах",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "🌍",
                            Title = "15 бирж"
                        },
                        new MenuItemStat
                        {
                            Icon = "📊",
                            Title = "Индексы"
                        }
                    ]
                },

                new MenuItem
                {
                    Icon = "⭐",
                    Title = "Рейтинги",
                    Description = "Кредитные рейтинги, оценки надежности и инвестиционные рейтинги",
                    Stats =
                    [
                        new MenuItemStat
                        {
                            Icon = "📊",
                            Title = "3 агентства"
                        },
                        new MenuItemStat
                        {
                            Icon = "🏆",
                            Title = "Шкалы"
                        }
                    ]
                }
            ];

            return new ObservableCollection<MenuItem>(list);
        }

        private static ObservableCollection<Dictionary> GetDictionaries()
        {
            List<Dictionary> list = 
            [
                new Dictionary
                {
                    Icon = "🏢",
                    Title = "Эмитенты"
                },
                new Dictionary
                {
                    Icon = "🏭",
                    Title = "Отрасли"
                },
                new Dictionary
                {
                    Icon = "💱",
                    Title = "Валюты"
                },
                new Dictionary
                {
                    Icon = "🏛️",
                    Title = "Биржи"
                },
                new Dictionary
                {
                    Icon = "⭐",
                    Title = "Рейтинги"
                },
                new Dictionary
                {
                    Icon = "🌍",
                    Title = "Страны"
                },
                new Dictionary
                {
                    Icon = "📊",
                    Title = "Индексы"
                },
                new Dictionary
                {
                    Icon = "💵",
                    Title = "Дивиденды"
                },
                new Dictionary
                {
                    Icon = "📑",
                    Title = "Отчетность"
                },
                new Dictionary
                {
                    Icon = "📅",
                    Title = "Календарь"
                }
            ];

            return new ObservableCollection<Dictionary>(list);
        }
    }
}
