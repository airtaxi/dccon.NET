using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using dccon.NET.Exceptions;
using dccon.NET.Http;
using dccon.NET.Models;
using dccon.NET.Parsing;
using Newtonsoft.Json.Linq;

namespace dccon.NET
{
    /// <summary>
    /// 비공식 디시콘 클라이언트 구현체
    /// </summary>
    public class DcconClient : IDcconClient, IDisposable
    {
        private readonly DcconHttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private HttpClient? _innerHttpClient;
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
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            _httpClient = new DcconHttpClient(httpClient);
            _ownsHttpClient = false;
        }

        /// <inheritdoc />
        public async Task<SearchResult> SearchAsync(
            string query,
            SearchType searchType = SearchType.Title,
            SearchSort sort = SearchSort.Hot,
            int page = 1,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("검색어가 비어있습니다.", nameof(query));

            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

            var sortPath = sort == SearchSort.Hot ? "hot" : "new";
            var typePath = ConvertSearchTypeToPath(searchType);
            var encodedQuery = Uri.EscapeDataString(query);
            var path = $"{sortPath}/{page}/{typePath}/{encodedQuery}";

            var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
            return HtmlResponseParser.ParseSearchResult(html, page);
        }

        /// <inheritdoc />
        public async Task<SearchResult> GetHotListAsync(int page = 1, CancellationToken cancellationToken = default)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

            var path = $"hot/{page}";
            var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
            return HtmlResponseParser.ParseSearchResult(html, page);
        }

        /// <inheritdoc />
        public async Task<SearchResult> GetNewListAsync(int page = 1, CancellationToken cancellationToken = default)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");

            var path = $"new/{page}";
            var html = await _httpClient.GetListPageHtmlAsync(path, cancellationToken).ConfigureAwait(false);
            return HtmlResponseParser.ParseSearchResult(html, page);
        }

        /// <inheritdoc />
        public async Task<DcconPackageDetail> GetPackageDetailAsync(int packageIndex, CancellationToken cancellationToken = default)
        {
            if (packageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");

            var json = await _httpClient.GetPackageDetailJsonAsync(packageIndex, cancellationToken).ConfigureAwait(false);
            return ParsePackageDetailJson(json);
        }

        /// <inheritdoc />
        public async Task<byte[]> DownloadStickerAsync(DcconSticker sticker, CancellationToken cancellationToken = default)
        {
            if (sticker == null)
                throw new ArgumentNullException(nameof(sticker));

            if (string.IsNullOrEmpty(sticker.Path))
                throw new ArgumentException("스티커의 Path가 비어있습니다.", nameof(sticker));

            return await _httpClient.DownloadImageBytesAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<Stream> DownloadStickerStreamAsync(DcconSticker sticker, CancellationToken cancellationToken = default)
        {
            if (sticker == null)
                throw new ArgumentNullException(nameof(sticker));

            if (string.IsNullOrEmpty(sticker.Path))
                throw new ArgumentException("스티커의 Path가 비어있습니다.", nameof(sticker));

            return await _httpClient.DownloadImageStreamAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DownloadPackageAsync(
            int packageIndex,
            string outputDirectory,
            IProgress<(int Completed, int Total)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (packageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(packageIndex), "패키지 번호는 양수여야 합니다.");

            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentException("출력 디렉토리가 비어있습니다.", nameof(outputDirectory));

            var packageDetail = await GetPackageDetailAsync(packageIndex, cancellationToken).ConfigureAwait(false);

            // 패키지명으로 하위 폴더 생성
            var safeDirectoryName = SanitizeFileName(packageDetail.Title);
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
                    var fileName = SanitizeFileName(sticker.Title);
                    if (string.IsNullOrWhiteSpace(fileName))
                        fileName = $"sticker_{sticker.SortNumber}";

                    var filePath = Path.Combine(packageDirectory, $"{fileName}.{sticker.Extension}");

                    var imageData = await _httpClient.DownloadImageBytesAsync(sticker.Path, cancellationToken).ConfigureAwait(false);
                    File.WriteAllBytes(filePath, imageData);

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report((currentCompleted, total));
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static DcconPackageDetail ParsePackageDetailJson(string json)
        {
            try
            {
                var root = JObject.Parse(json);
                var info = root["info"];
                var detailArray = root["detail"] as JArray;
                var tagsArray = root["tags"] as JArray;

                if (info == null)
                    throw new DcconParsingException("패키지 상세 JSON에 'info' 필드가 없습니다.");

                var detail = new DcconPackageDetail
                {
                    PackageIndex = info["package_idx"]?.ToObject<int>() ?? 0,
                    Title = info["title"]?.ToString() ?? string.Empty,
                    Description = info["description"]?.ToString() ?? string.Empty,
                    MainImagePath = info["main_img_path"]?.ToString() ?? string.Empty,
                    SellerName = info["seller_name"]?.ToString() ?? string.Empty,
                    RegistrationDate = info["reg_date_short"]?.ToString() ?? string.Empty,
                    State = info["state"]?.ToString() ?? string.Empty
                };

                if (detailArray != null)
                {
                    foreach (var item in detailArray)
                    {
                        var sticker = new DcconSticker
                        {
                            Path = item["path"]?.ToString() ?? string.Empty,
                            Title = item["title"]?.ToString() ?? string.Empty,
                            Extension = item["ext"]?.ToString() ?? string.Empty,
                            SortNumber = item["sort_no"]?.ToObject<int>() ?? 0
                        };
                        detail.Stickers.Add(sticker);
                    }
                }

                if (tagsArray != null)
                {
                    foreach (var tagItem in tagsArray)
                    {
                        var tag = tagItem["tag"]?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(tag))
                            detail.Tags.Add(tag);
                    }
                }

                return detail;
            }
            catch (DcconParsingException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new DcconParsingException("패키지 상세 JSON 파싱 중 오류가 발생했습니다.", exception);
            }
        }

        private static string ConvertSearchTypeToPath(SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.Title: return "title";
                case SearchType.NickName: return "nick_name";
                case SearchType.Tags: return "tags";
                default: return "title";
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(character => !invalidChars.Contains(character)).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "unnamed" : sanitized;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_ownsHttpClient)
                    _innerHttpClient?.Dispose();

                _disposed = true;
            }
        }
    }
}
