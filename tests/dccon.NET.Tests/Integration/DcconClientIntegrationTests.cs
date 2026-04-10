using System;
using System.Linq;
using System.Threading.Tasks;
using dccon.NET.Models;
using Xunit;

namespace dccon.NET.Tests.Integration;

/// <summary>
/// 실제 DCcon API를 호출하는 통합 테스트.
/// 환경변수 DCCON_INTEGRATION_TESTS=true 설정 시에만 실행됨.
/// </summary>
[Trait("Category", "Integration")]
public class DcconClientIntegrationTests : IDisposable
{
    private readonly DcconClient _client;
    private readonly bool _shouldRun;

    public DcconClientIntegrationTests()
    {
        _client = new DcconClient();
        _shouldRun = string.Equals(
            Environment.GetEnvironmentVariable("DCCON_INTEGRATION_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsResults()
    {
        if (!_shouldRun) return;

        var result = await _client.SearchAsync("페페", SearchType.Title, SearchSort.Hot);

        Assert.True(result.TotalCount > 0);
        Assert.NotEmpty(result.Packages);
        Assert.All(result.Packages, package =>
        {
            Assert.True(package.PackageIndex > 0);
            Assert.False(string.IsNullOrEmpty(package.Title));
        });
    }

    [Fact]
    public async Task GetHotListAsync_ReturnsResults()
    {
        if (!_shouldRun) return;

        var result = await _client.GetHotListAsync();

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task GetNewListAsync_ReturnsResults()
    {
        if (!_shouldRun) return;

        var result = await _client.GetNewListAsync();

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task GetPackageDetailAsync_WithValidIndex_ReturnsDetail()
    {
        if (!_shouldRun) return;

        // 먼저 검색으로 유효한 패키지 인덱스를 가져온다
        var searchResult = await _client.GetHotListAsync();
        var firstPackage = searchResult.Packages.First();

        var detail = await _client.GetPackageDetailAsync(firstPackage.PackageIndex);

        Assert.False(string.IsNullOrEmpty(detail.Title));
        Assert.NotEmpty(detail.Stickers);
        Assert.All(detail.Stickers, sticker =>
        {
            Assert.False(string.IsNullOrEmpty(sticker.Path));
            Assert.False(string.IsNullOrEmpty(sticker.Extension));
        });
    }

    [Fact]
    public async Task DownloadStickerAsync_WithValidSticker_ReturnsBytes()
    {
        if (!_shouldRun) return;

        var searchResult = await _client.GetHotListAsync();
        var firstPackage = searchResult.Packages.First();
        var detail = await _client.GetPackageDetailAsync(firstPackage.PackageIndex);
        var firstSticker = detail.Stickers.First();

        var imageData = await _client.DownloadStickerAsync(firstSticker);

        Assert.True(imageData.Length > 0);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
