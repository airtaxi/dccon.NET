using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace dccon.NET.Json;

/// <summary>
/// 패키지 상세 API 응답 JSON 구조
/// </summary>
internal class PackageDetailResponse
{
    [JsonPropertyName("info")]
    public PackageInfoResponse? Info { get; set; }

    [JsonPropertyName("detail")]
    public List<StickerDetailResponse>? Detail { get; set; }

    [JsonPropertyName("tags")]
    public List<TagItemResponse>? Tags { get; set; }
}

/// <summary>
/// 패키지 기본 정보
/// </summary>
internal class PackageInfoResponse
{
    [JsonPropertyName("package_idx")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int PackageIndex { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("main_img_path")]
    public string? MainImagePath { get; set; }

    [JsonPropertyName("seller_name")]
    public string? SellerName { get; set; }

    [JsonPropertyName("reg_date_short")]
    public string? RegistrationDate { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

/// <summary>
/// 개별 스티커 상세 정보
/// </summary>
internal class StickerDetailResponse
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("ext")]
    public string? Extension { get; set; }

    [JsonPropertyName("sort")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int SortNumber { get; set; }
}

/// <summary>
/// 태그 항목
/// </summary>
internal class TagItemResponse
{
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }
}
