using System.ComponentModel.DataAnnotations.Schema;

namespace InvestLens.Data.Entities.Index;

public class Engine : BaseEntity
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;
}

public class Boards
{

}

public class BoardGroups
{

}

public class Durations
{

}

public class SecurityTypes
{

}

public class SecurityGroups
{

}

public class SecurityCollections
{

}