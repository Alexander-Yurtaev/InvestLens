using System.Text.Json.Serialization;

namespace InvestLens.Abstraction.Telegram.Models;

public class GetMeResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("result")]
    public GetMeResponseResult? Result { get; set; }
}

public class GetMeResponseResult
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("can_join_groups")]
    public bool CanJoinGroups { get; set; }

    [JsonPropertyName("can_read_all_group_messages")]
    public bool CanReadAllGroupMessages { get; set; }

    [JsonPropertyName("supports_inline_queries")]
    public bool SupportsInlineQueries { get; set; }

    [JsonPropertyName("can_connect_to_business")]
    public bool CanConnectToBusiness { get; set; }

    [JsonPropertyName("has_main_web_app")]
    public bool HasMainWebApp { get; set; }

    [JsonPropertyName("has_topics_enabled")]
    public bool HasTopicsEnabled { get; set; }
}
