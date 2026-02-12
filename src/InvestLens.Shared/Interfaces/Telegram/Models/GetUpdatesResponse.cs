using System.Text.Json.Serialization;

namespace InvestLens.Shared.Interfaces.Telegram.Models;

public class GetUpdatesResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public GetUpdatesResult[] Result { get; set; } = [];
}

public class GetUpdatesResult
{
    [JsonPropertyName("update_id")]
    public int UpdateId { get; set; }

    [JsonPropertyName("message")]
    public required Message Message { get; set; }

    [JsonPropertyName("edited_message")]
    public EditedMessage? EditedMessage { get; set; }
}

public class Message
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("from")]
    public required From From { get; set; }

    [JsonPropertyName("chat")]
    public required Chat Chat { get; set; }

    [JsonPropertyName("date")]
    public int Date { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("entities")]
    public Entity[] Entities { get; set; } = [];

    [JsonPropertyName("sticker")]
    public Sticker? Sticker { get; set; }
}

public class From
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("first_name")]
    public required string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public required string LastName { get; set; }

    [JsonPropertyName("username")]
    public required string Username { get; set; }

    [JsonPropertyName("language_code")]
    public required string LanguageCode { get; set; }
}

public class Chat
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("first_name")]
    public required string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class Sticker
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = string.Empty;

    [JsonPropertyName("set_name")]
    public string SetName { get; set; } = string.Empty;

    [JsonPropertyName("is_animated")]
    public bool IsAnimated { get; set; }

    [JsonPropertyName("is_video")]
    public bool IsVideo { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("thumbnail")]
    public required Thumbnail Thumbnail { get; set; }

    [JsonPropertyName("thumb")]
    public required Thumb Thumb { get; set; }

    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    [JsonPropertyName("file_unique_id")]
    public string FileUniqueId { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public int FileSize { get; set; }
}

public class Thumbnail
{
    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    [JsonPropertyName("file_unique_id")]
    public string FileUniqueId { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public int FileSize { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class Thumb
{
    [JsonPropertyName("file_id")]
    public string FileId { get; set; } = string.Empty;

    [JsonPropertyName("file_unique_id")]
    public string FileUniqueId { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public int FileSize { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class Entity
{
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

public class EditedMessage
{
    [JsonPropertyName("message_id")]
    public int MessageId { get; set; }

    [JsonPropertyName("from")]
    public required From From { get; set; }

    [JsonPropertyName("chat")]
    public required Chat Chat { get; set; }

    [JsonPropertyName("date")]
    public int Date { get; set; }

    [JsonPropertyName("edit_date")]
    public int EditDate { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}