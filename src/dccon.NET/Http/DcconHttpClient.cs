using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dccon.NET.Exceptions;

namespace dccon.NET.Http
{
    /// <summary>
    /// DCcon 사이트와의 HTTP 통신을 담당하는 클래스
    /// </summary>
    internal class DcconHttpClient
    {
        private const string BaseUrl = "https://dccon.dcinside.com";
        private const string ImageBaseUrl = "https://dcimg5.dcinside.com/dccon.php";

        private readonly HttpClient _httpClient;

        public DcconHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// 검색/목록 페이지의 HTML을 가져온다.
        /// </summary>
        public async Task<string> GetListPageHtmlAsync(string path, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}/{path.TrimStart('/')}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 패키지 상세 정보 JSON을 가져온다.
        /// </summary>
        public async Task<string> GetPackageDetailJsonAsync(int packageIndex, CancellationToken cancellationToken = default)
        {
            var url = $"{BaseUrl}/index/package_detail";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("package_idx", packageIndex.ToString())
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new DcconNotFoundException($"패키지를 찾을 수 없습니다: {packageIndex}");

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 스티커 이미지를 바이트 배열로 다운로드한다.
        /// </summary>
        public async Task<byte[]> DownloadImageBytesAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var url = $"{ImageBaseUrl}?no={imagePath}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Referer", BaseUrl);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 스티커 이미지를 스트림으로 다운로드한다.
        /// </summary>
        public async Task<Stream> DownloadImageStreamAsync(string imagePath, CancellationToken cancellationToken = default)
        {
            var url = $"{ImageBaseUrl}?no={imagePath}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Referer", BaseUrl);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}
