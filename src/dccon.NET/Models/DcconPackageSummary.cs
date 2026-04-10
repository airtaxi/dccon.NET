namespace dccon.NET.Models;

/// <summary>
/// 검색 결과에 표시되는 디시콘 패키지 요약 정보
/// </summary>
public class DcconPackageSummary
{
    /// <summary>패키지 고유 번호</summary>
    public int PackageIndex { get; set; }

    /// <summary>디시콘 패키지명</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>판매자/제작자 이름</summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>썸네일 이미지 URL</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;
}
