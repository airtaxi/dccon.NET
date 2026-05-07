using System;
using System.Collections.Generic;
using System.Text.Json;
using dccon.NET.Exceptions;
using dccon.NET.Json;
using dccon.NET.Models;

namespace dccon.NET.Parsing;

/// <summary>
/// 일간/주간/월간 인기 디시콘 JSONP 응답을 파싱하여 구조화된 데이터로 변환
/// </summary>
internal static class JsonpResponseParser
{
    /// <summary>
    /// JSONP 응답에서 인기 디시콘 목록을 추출한다.
    /// </summary>
    public static List<DcconPackageSummary> ParsePopularDccon(string jsonPaddingResponse)
    {
        if (string.IsNullOrWhiteSpace(jsonPaddingResponse)) throw new DcconParsingException("파싱할 JSONP 응답이 비어있습니다.");

        try
        {
            var jsonPayload = ExtractJsonFromJsonp(jsonPaddingResponse);

            var items = JsonSerializer.Deserialize(jsonPayload, DcconJsonContext.Default.ListPopularDcconResponse)
                ?? throw new DcconParsingException("인기 디시콘 JSON 파싱 결과가 null입니다.");

            var result = new List<DcconPackageSummary>();
            foreach (var item in items)
            {
                if (!int.TryParse(item.PackageIndex, out var packageIndex)) continue;

                var thumbnailUrl = item.ImageUrl ?? string.Empty;
                if (thumbnailUrl.StartsWith("//")) thumbnailUrl = "https:" + thumbnailUrl;

                result.Add(new DcconPackageSummary
                {
                    PackageIndex = packageIndex,
                    Title = item.Title ?? string.Empty,
                    SellerName = item.NickName ?? string.Empty,
                    ThumbnailUrl = thumbnailUrl
                });
            }

            return result;
        }
        catch (DcconParsingException) { throw; }
        catch (Exception exception) { throw new DcconParsingException("인기 디시콘 JSONP 파싱 중 오류가 발생했습니다.", exception); }
    }

    /// <summary>
    /// JSONP 래퍼를 제거하고 순수 JSON을 추출한다.
    /// </summary>
    private static string ExtractJsonFromJsonp(string jsonPaddingResponse)
    {
        var openParenthesisIndex = jsonPaddingResponse.IndexOf('(');
        var closeParenthesisIndex = jsonPaddingResponse.LastIndexOf(')');

        if (openParenthesisIndex < 0 || closeParenthesisIndex < 0 || closeParenthesisIndex <= openParenthesisIndex)
            throw new DcconParsingException("JSONP 응답 형식이 올바르지 않습니다.");

        return jsonPaddingResponse.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
    }
}
