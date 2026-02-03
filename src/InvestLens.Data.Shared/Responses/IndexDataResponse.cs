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
    public required BoardGroups BoardGroups { get; set; }

    [JsonPropertyName("durations")]
    public required Durations Durations { get; set; }

    [JsonPropertyName("securitytypes")]
    public required SecurityTypes SecurityTypes { get; set; }

    [JsonPropertyName("securitygroups")]
    public required SecurityGroups SecurityGroups { get; set; }

    [JsonPropertyName("securitycollections")]
    public required SecurityCollections SecurityCollections { get; set; }
}

public class Engines
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Markets
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Boards
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class BoardGroups
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class Durations
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class SecurityTypes
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class SecurityGroups
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}

public class SecurityCollections
{
    [JsonPropertyName("metadata")]
    public Dictionary<string, ColumnMetadata> Metadata { get; set; } = new Dictionary<string, ColumnMetadata>();

    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = [];

    [JsonPropertyName("data")]
    public object[][] Data { get; set; } = [];
}
