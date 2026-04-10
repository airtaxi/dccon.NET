using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dccon.NET.Models;

namespace dccon.NET;

/// <summary>
/// 디시콘 클라이언트 인터페이스
/// </summary>
public interface IDcconClient
{
    /// <summary>
    /// 디시콘을 검색한다.
    /// </summary>
    /// <param name="query">검색어</param>
    /// <param name="searchType">검색 유형 (디시콘명, 닉네임, 태그)</param>
    /// <param name="sort">정렬 방식 (인기순, 최신순)</param>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<SearchResult> SearchAsync(
        string query,
        SearchType searchType = SearchType.Title,
        SearchSort sort = SearchSort.Hot,
        int page = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 인기 디시콘 목록을 가져온다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<SearchResult> GetHotListAsync(int page = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 최신 디시콘 목록을 가져온다.
    /// </summary>
    /// <param name="page">페이지 번호 (1부터 시작)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<SearchResult> GetNewListAsync(int page = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// 디시콘 패키지 상세 정보를 가져온다.
    /// </summary>
    /// <param name="packageIndex">패키지 고유 번호</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<DcconPackageDetail> GetPackageDetailAsync(int packageIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// 스티커 이미지를 바이트 배열로 다운로드한다.
    /// </summary>
    /// <param name="sticker">다운로드할 스티커</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<byte[]> DownloadStickerAsync(DcconSticker sticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// 스티커 이미지를 스트림으로 다운로드한다.
    /// </summary>
    /// <param name="sticker">다운로드할 스티커</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task<Stream> DownloadStickerStreamAsync(DcconSticker sticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// 패키지 내 모든 스티커를 지정 폴더에 다운로드한다.
    /// </summary>
    /// <param name="packageIndex">패키지 고유 번호</param>
    /// <param name="outputDirectory">저장할 폴더 경로</param>
    /// <param name="progress">진행 상태 콜백 (현재 완료 수 / 전체 수)</param>
    /// <param name="cancellationToken">취소 토큰</param>
    Task DownloadPackageAsync(
        int packageIndex,
        string outputDirectory,
        IProgress<(int Completed, int Total)>? progress = null,
        CancellationToken cancellationToken = default);
}
