using System.Collections.Generic;

namespace dccon.NET.Models;

/// <summary>
/// 디시콘 패키지 상세 정보
/// </summary>
public class DcconPackageDetail
{
    /// <summary>패키지 고유 번호</summary>
    public int PackageIndex { get; set; }

    /// <summary>디시콘 패키지명</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>설명</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>대표 이미지 경로 (dccon.php의 no 파라미터 값)</summary>
    public string MainImagePath { get; set; } = string.Empty;

    /// <summary>판매자/제작자 이름</summary>
    public string SellerName { get; set; } = string.Empty;

    /// <summary>등록일 (짧은 형식, 예: 2023.01.01)</summary>
    public string RegistrationDate { get; set; } = string.Empty;

    /// <summary>패키지 상태 (S: 판매중 등)</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>패키지에 포함된 스티커 목록</summary>
    public List<DcconSticker> Stickers { get; set; } = [];

    /// <summary>태그 목록</summary>
    public List<string> Tags { get; set; } = [];
}
