using System.Text.Json.Serialization;

namespace dccon.NET.Json;

/// <summary>
/// 일간/주간/월간 인기 디시콘 API 응답 JSON 항목
/// </summary>
internal class PopularDcconResponse
{
    [JsonPropertyName("package_idx")]
    public string? PackageIndex { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("nick_name")]
    public string? NickName { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("img")]
    public string? ImageUrl { get; set; }
}
