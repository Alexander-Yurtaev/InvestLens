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

    [Display(Name = "Загрузка данных")]
    [Description("Идёт получение данных из внешних источников")]
    Downloading,

    [Display(Name = "Обработка данных")]
    [Description("Загруженные данные проходят валидацию, трансформацию и подготовку к сохранению в базе данных")]
    Processing,

    [Display(Name = "Сохранение в БД")]
    [Description("Обработанные данные записываются в базу данных")]
    Saving,

    [Display(Name = "Завершено")]
    [Description("Процесс обновления данных успешно завершён")]
    Completed,

    [Display(Name = "Ошибка")]
    [Description("В процессе обновления данных произошла ошибка")]
    Failed
}