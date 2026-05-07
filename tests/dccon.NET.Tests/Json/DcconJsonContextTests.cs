using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using dccon.NET.Json;
using Xunit;

namespace dccon.NET.Tests.Json;

/// <summary>
/// NativeAOT 호환 소스 제너레이션 JSON 직렬화/역직렬화 테스트
/// </summary>
public class DcconJsonContextTests : IDisposable
{
    private const string DailyPopularRequestUri = "https://json2.dcinside.com/json1/dccon_day_top100.php?jsoncallback=day_top100";

    private readonly HttpClient _httpClient = new();

    private async Task<string> FetchPackageDetailJsonAsync(int packageIndex)
    {
        var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("package_idx", packageIndex.ToString())]);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://dccon.dcinside.com/index/package_detail") { Content = content };
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    private async Task<int> FetchFirstPopularPackageIndexAsync()
    {
        var response = await _httpClient.GetAsync(DailyPopularRequestUri);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        var jsonPaddingResponse = Encoding.UTF8.GetString(bytes);
        var popularPackages = dccon.NET.Parsing.JsonpResponseParser.ParsePopularDccon(jsonPaddingResponse);
        return popularPackages.First().PackageIndex;
    }

    [Fact]
    public async Task Deserialize_WithLivePackageDetailJson_ReturnsPackageDetailResponse()
    {
        var packageIndex = await FetchFirstPopularPackageIndexAsync();
        var json = await FetchPackageDetailJsonAsync(packageIndex);

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse);

        Assert.NotNull(response);
        Assert.NotNull(response.Info);
    }

    [Fact]
    public async Task Deserialize_WithLivePackageDetailJson_ParsesInfoCorrectly()
    {
        var packageIndex = await FetchFirstPopularPackageIndexAsync();
        var json = await FetchPackageDetailJsonAsync(packageIndex);

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.True(response.Info!.PackageIndex > 0);
        Assert.False(string.IsNullOrEmpty(response.Info.Title));
        Assert.False(string.IsNullOrEmpty(response.Info.SellerName));
    }

    [Fact]
    public async Task Deserialize_WithLivePackageDetailJson_ParsesStickersCorrectly()
    {
        var packageIndex = await FetchFirstPopularPackageIndexAsync();
        var json = await FetchPackageDetailJsonAsync(packageIndex);

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Detail);
        Assert.NotEmpty(response.Detail);
        Assert.All(response.Detail, sticker =>
        {
            Assert.False(string.IsNullOrEmpty(sticker.Path));
            Assert.False(string.IsNullOrEmpty(sticker.Extension));
        });
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_ReturnsNullProperties()
    {
        var json = """{"info": {"package_idx": 1}}""";

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Info);
        Assert.Equal(1, response.Info.PackageIndex);
        Assert.Null(response.Info.Title);
        Assert.Null(response.Info.Description);
        Assert.Null(response.Detail);
        Assert.Null(response.Tags);
    }

    [Fact]
    public void Deserialize_WithNullInfoField_ReturnsNullInfo()
    {
        var json = """{"info": null, "detail": [], "tags": []}""";

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.Null(response.Info);
    }

    [Fact]
    public void SourceGeneratedContext_HasPackageDetailResponseTypeInfo()
    {
        var typeInfo = DcconJsonContext.Default.PackageDetailResponse;

        Assert.NotNull(typeInfo);
        Assert.Equal(typeof(PackageDetailResponse), typeInfo.Type);
    }

    [Fact]
    public void SourceGeneratedContext_HasListPopularDcconResponseTypeInfo()
    {
        var typeInfo = DcconJsonContext.Default.ListPopularDcconResponse;

        Assert.NotNull(typeInfo);
    }

    [Fact]
    public void Serialize_ThenDeserialize_RoundTripsCorrectly()
    {
        var original = new PackageDetailResponse
        {
            Info = new PackageInfoResponse
            {
                PackageIndex = 12345,
                Title = "라운드트립 테스트",
                SellerName = "판매자"
            },
            Detail =
            [
                new StickerDetailResponse
                {
                    Path = "path1",
                    Title = "스티커",
                    Extension = "png",
                    SortNumber = 1
                }
            ],
            Tags = [new TagItemResponse { Tag = "태그" }]
        };

        var json = JsonSerializer.Serialize(original, DcconJsonContext.Default.PackageDetailResponse);
        var deserialized = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Info);
        Assert.Equal(original.Info.PackageIndex, deserialized.Info.PackageIndex);
        Assert.Equal(original.Info.Title, deserialized.Info.Title);
        Assert.Equal(original.Info.SellerName, deserialized.Info.SellerName);
        Assert.NotNull(deserialized.Detail);
        Assert.Single(deserialized.Detail);
        Assert.Equal("path1", deserialized.Detail[0].Path);
        Assert.NotNull(deserialized.Tags);
        Assert.Single(deserialized.Tags);
        Assert.Equal("태그", deserialized.Tags[0].Tag);
    }

    [Fact]
    public void Deserialize_WithStringPackageIdx_ParsesAsInt()
    {
        var json = """{"info": {"package_idx": "168838"}}""";

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Info);
        Assert.Equal(168838, response.Info.PackageIndex);
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "not valid json";

        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize(invalidJson, DcconJsonContext.Default.PackageDetailResponse));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
