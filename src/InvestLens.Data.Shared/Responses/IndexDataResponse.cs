using System.Text.Json.Serialization;

namespace InvestLens.Data.Shared.Responses;

public class IndexDataResponse : IBaseResponse
{
    [JsonPropertyName("engines")]
    public required Engines Engines { get; set; }

    [JsonPropertyName("markets")]
    public required Markets Markets { get; set; }

    [JsonPropertyName("boards")]
    public required Boards Boards { get; set; }

    [JsonPropertyName("boardgroups")]
    public required Boardgroups BoardGroups { get; set; }

    [JsonPropertyName("durations")]
    public required Durations Durations { get; set; }

    [JsonPropertyName("securitytypes")]
    public required Securitytypes SecurityTypes { get; set; }

    [JsonPropertyName("securitygroups")]
    public required Securitygroups SecurityGroups { get; set; }

    [JsonPropertyName("securitycollections")]
    public required Securitycollections SecurityCollections { get; set; }
}

public class Engines
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Markets
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Boards
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Boardgroups
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Durations
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Securitytypes
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Securitygroups
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Securitycollections
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}
