using System.IO;
using System.Linq;
using dccon.NET.Models;

namespace dccon.NET;

/// <summary>
/// 디시콘 파일명 관련 유틸리티
/// </summary>
public static class DcconFileNameHelper
{
    /// <summary>
    /// 파일명에 사용할 수 없는 문자를 제거하여 안전한 파일명을 반환한다.
    /// </summary>
    /// <param name="fileName">원본 파일명</param>
    public static string SanitizeFileName(string fileName)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(character => !invalidCharacters.Contains(character)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "unnamed" : sanitized;
    }

    /// <summary>
    /// 스티커가 다운로드될 때 사용되는 파일명(확장자 포함)을 반환한다.
    /// <see cref="DcconClient.DownloadPackageAsync"/>에서 사용하는 것과 동일한 로직이다.
    /// </summary>
    /// <param name="sticker">대상 스티커</param>
    public static string GetStickerFileName(DcconSticker sticker)
    {
        var sanitizedTitle = SanitizeFileName(sticker.Title);
        if (string.IsNullOrWhiteSpace(sanitizedTitle) || sanitizedTitle == "unnamed")
            sanitizedTitle = $"sticker_{sticker.SortNumber}";

        return $"{sanitizedTitle}.{sticker.Extension}";
    }
}
