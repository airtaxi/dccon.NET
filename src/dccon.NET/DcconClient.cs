using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dccon.NET.Exceptions;
using dccon.NET.Http;
using dccon.NET.Json;
using dccon.NET.Models;
using dccon.NET.Parsing;

namespace dccon.NET;

/// <summary>
/// 비공식 디시콘 클라이언트 구현체
/// </summary>
public class DcconClient : IDcconClient, IDisposable
{
    private readonly DcconHttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly HttpClient? _innerHttpClient;
    private bool _disposed;

    /// <summary>
    /// 내부에서 HttpClient를 생성하여 사용하는 생성자
    /// </summary>
    public DcconClient()
    {
        _innerHttpClient = new HttpClient();
        _httpClient = new DcconHttpClient(_innerHttpClient);
        _ownsHttpClient = true;
    }

    /// <summary>
    /// 외부에서 주입받은 HttpClient를 사용하는 생성자 (IHttpClientFactory 패턴 호환)
    /// </summary>
    /// <param name="httpClient">사용할 HttpClient 인스턴스</param>
    public DcconClient(HttpClient httpClient)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

        _httpClient = new DcconHttpClient(httpClient);
        _ownsHttpClient = false;
    }

    /// <inheritdoc />
    public async Task<DcconSearchResult> SearchAsync(
        string query,
        DcconSearchType searchType = DcconSearchType.Title,
        DcconSearchSort sort = DcconSearchSort.Hot,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("검색어가 비어있습니다.", nameof(query));

        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

        var sortPath = sort == DcconSearchSort.Hot ? "hot" : "new";
        var typePath = ConvertSearchTypeToPath(searchType);
        var encodedQuery = Uri.EscapeDataString(query);
        var path = $"{sortPath}/{page}/{typePath}/{encodedQuery}";

        var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
        return HtmlResponseParser.ParseSearchResult(html, page);
    }

    /// <inheritdoc />
    public async Task<DcconSearchResult> GetHotListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

        var path = $"hot/{page}";
        var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
        return HtmlResponseParser.ParseSearchResult(html, page);
    }

    /// <inheritdoc />
    public async Task<List<DcconPackageSummary>> GetDailyPopularAsync(CancellationToken cancellationToken = default)
    {
        var jsonp = await _httpClient.GetPopularDcconJsonpAsync(
            "dccon_day_top5.php?jsoncallback=day_top5", cancellationToken).ConfigureAwait(false);
        return JsonpResponseParser.ParsePopularDccon(jsonp);
    }

    /// <inheritdoc />
    public async Task<List<DcconPackageSummary>> GetWeeklyPopularAsync(CancellationToken cancellationToken = default)
    {
        var jsonp = await _httpClient.GetPopularDcconJsonpAsync(
            "dccon_week_top5.php?jsoncallback=week_top5", cancellationToken).ConfigureAwait(false);
        return JsonpResponseParser.ParsePopularDccon(jsonp);
    }

    /// <inheritdoc />
    public async Task<DcconSearchResult> GetNewListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

        var path = $"new/{page}";
        var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
        return HtmlResponseParser.ParseSearchResult(html, page);
    }

    /// <inheritdoc />
    public async Task<DcconPackageDetail> GetPackageDetailAsync(int packageIndex, CancellationToken cancellationToken = default)
    {
        if (packageIndex <= 0) throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");

        var json = await _httpClient.GetPackageDetailJsonAsync(packageIndex, cancellationToken).ConfigureAwait(false);
        return ParsePackageDetailJson(json);
    }

    /// <inheritdoc />
    public async Task<byte[]> DownloadStickerAsync(DcconSticker sticker, CancellationToken cancellationToken = default)
    {
        if (sticker == null) throw new ArgumentNullException(nameof(sticker));

        if (string.IsNullOrEmpty(sticker.Path)) throw new ArgumentException("스티커의 Path가 비어있습니다.", nameof(sticker));

        return await _httpClient.DownloadImageBytesAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Stream> DownloadStickerStreamAsync(DcconSticker sticker, CancellationToken cancellationToken = default)
    {
        if (sticker == null) throw new ArgumentNullException(nameof(sticker));

        if (string.IsNullOrEmpty(sticker.Path)) throw new ArgumentException("스티커의 Path가 비어있습니다.", nameof(sticker));

        return await _httpClient.DownloadImageStreamAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DownloadPackageAsync(
        int packageIndex,
        string outputDirectory,
        IProgress<(int Completed, int Total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (packageIndex <= 0) throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");

        if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentException("출력 디렉토리가 비어있습니다.", nameof(outputDirectory));

        var packageDetail = await GetPackageDetailAsync(packageIndex, cancellationToken).ConfigureAwait(false);

        // 패키지명으로 하위 폴더 생성
        var safeDirectoryName = DcconFileNameHelper.SanitizeFileName(packageDetail.Title);
        var packageDirectory = Path.Combine(outputDirectory, safeDirectoryName);
        Directory.CreateDirectory(packageDirectory);

        var total = packageDetail.Stickers.Count;
        var completed = 0;

        // 병렬 다운로드 (최대 4개 동시)
        var semaphore = new SemaphoreSlim(4);
        var tasks = packageDetail.Stickers.Select(async sticker =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var filePath = Path.Combine(packageDirectory, DcconFileNameHelper.GetStickerFileName(sticker));

                var imageData = await _httpClient.DownloadImageBytesAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
                File.WriteAllBytes(filePath, imageData);

                var currentCompleted = Interlocked.Increment(ref completed);
                progress?.Report((currentCompleted, total));
            }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static DcconPackageDetail ParsePackageDetailJson(string json)
    {
        try
        {
            var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)
                ?? throw new DcconParsingException("패키지 상세 JSON 파싱 결과가 null입니다.");

            if (response.Info == null) throw new DcconParsingException("패키지 상세 JSON에 'info' 필드가 없습니다.");

            var detail = new DcconPackageDetail
            {
                PackageIndex = response.Info.PackageIndex,
                SaleCount = response.Info.SaleCount,
                IconCount = response.Info.IconCount,
                Title = response.Info.Title ?? string.Empty,
                Description = response.Info.Description ?? string.Empty,
                MainImagePath = response.Info.MainImagePath ?? string.Empty,
                SellerName = response.Info.SellerName ?? string.Empty,
                RegistrationDate = response.Info.RegistrationDate ?? string.Empty,
                RegistrationDateShort = response.Info.RegistrationDateShort ?? string.Empty,
                State = response.Info.State ?? string.Empty
            };

            if (response.Detail != null)
            {
                foreach (var item in response.Detail)
                {
                    var sticker = new DcconSticker
                    {
                        Path = item.Path ?? string.Empty,
                        Title = item.Title ?? string.Empty,
                        Extension = item.Extension ?? string.Empty,
                        SortNumber = item.SortNumber
                    };
                    detail.Stickers.Add(sticker);
                }
            }

            if (response.Tags != null)
            {
                foreach (var tagItem in response.Tags)
                {
                    var tag = tagItem.Tag ?? string.Empty;
                    if (!string.IsNullOrEmpty(tag)) detail.Tags.Add(tag);
                }
            }

            return detail;
        }
        catch (DcconParsingException) { throw; }
        catch (Exception exception) { throw new DcconParsingException("패키지 상세 JSON 파싱 중 오류가 발생했습니다.", exception); }
    }

    private static string ConvertSearchTypeToPath(DcconSearchType searchType) => searchType switch
    {
        DcconSearchType.Title => "title",
        DcconSearchType.NickName => "nick_name",
        DcconSearchType.Tags => "tags",
        _ => "title",
    };

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsHttpClient) _innerHttpClient?.Dispose();

            _disposed = true;
        }
    }
}
