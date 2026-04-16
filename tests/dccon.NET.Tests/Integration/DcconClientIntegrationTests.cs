using System;
using System.Linq;
using System.Threading.Tasks;
using dccon.NET.Models;
using Xunit;

namespace dccon.NET.Tests.Integration;

/// <summary>
/// 실제 DCcon API를 호출하는 통합 테스트.
/// </summary>
[Trait("Category", "Integration")]
public class DcconClientIntegrationTests : IDisposable
{
    private readonly DcconClient _client = new();

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsResults()
    {
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
        var result = await _client.GetHotListAsync();

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task GetNewListAsync_ReturnsResults()
    {
        var result = await _client.GetNewListAsync();

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task GetDailyPopularAsync_ReturnsResults()
    {
        var result = await _client.GetDailyPopularAsync();

        Assert.NotEmpty(result);
        Assert.True(result.Count <= 5);
        Assert.All(result, package =>
        {
            Assert.True(package.PackageIndex > 0);
            Assert.False(string.IsNullOrEmpty(package.Title));
            Assert.False(string.IsNullOrEmpty(package.SellerName));
            Assert.False(string.IsNullOrEmpty(package.ThumbnailUrl));
        });
    }

    [Fact]
    public async Task GetWeeklyPopularAsync_ReturnsResults()
    {
        var result = await _client.GetWeeklyPopularAsync();

        Assert.NotEmpty(result);
        Assert.True(result.Count <= 5);
        Assert.All(result, package =>
        {
            Assert.True(package.PackageIndex > 0);
            Assert.False(string.IsNullOrEmpty(package.Title));
            Assert.False(string.IsNullOrEmpty(package.SellerName));
            Assert.False(string.IsNullOrEmpty(package.ThumbnailUrl));
        });
    }

    [Fact]
    public async Task GetPackageDetailAsync_WithValidIndex_ReturnsDetail()
    {
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
