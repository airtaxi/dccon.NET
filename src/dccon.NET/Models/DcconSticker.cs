namespace dccon.NET.Models
{
    /// <summary>
    /// 개별 디시콘 스티커 정보
    /// </summary>
    public class DcconSticker
    {
        /// <summary>이미지 경로 (dccon.php의 no 파라미터 값)</summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>스티커 제목</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>파일 확장자 (png, gif, jpg 등)</summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>정렬 순번</summary>
        public int SortNumber { get; set; }

        /// <summary>스티커 이미지의 전체 다운로드 URL</summary>
        public string ImageUrl => $"https://dcimg5.dcinside.com/dccon.php?no={Path}";
    }
}
