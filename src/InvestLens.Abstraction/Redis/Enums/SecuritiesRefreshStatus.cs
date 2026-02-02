using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InvestLens.Abstraction.Redis.Enums;

public enum SecuritiesRefreshStatus
{
    [Display(Name = "Не начато")]
    [Description("Процесс обновления данных ещё не запущен")]
    None,

    [Display(Name = "Запланировано")]
    [Description("Задача на обновление данных поставлена в очередь на выполнение")]
    Scheduled,

    [Display(Name = "Обработка данных")]
    [Description("Запущен процесс обработки данных")]
    Processing,

    [Display(Name = "Завершено")]
    [Description("Процесс обновления данных успешно завершён")]
    Completed,

    [Display(Name = "Ошибка")]
    [Description("В процессе обновления данных произошла ошибка")]
    Failed
}