using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using dccon.NET.Parsing;
using Xunit;

namespace dccon.NET.Tests.Parsing;

public class JsonpResponseParserTests : IDisposable
{
    private const string DailyPopularUrl = "https://json2.dcinside.com/json1/dccon_day_top5.php?jsoncallback=day_top5";
    private const string WeeklyPopularUrl = "https://json2.dcinside.com/json1/dccon_week_top5.php?jsoncallback=week_top5";

    private readonly HttpClient _httpClient = new();

    private async Task<string> FetchJsonpAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveDailyResponse_ReturnsUpToFiveItems()
    {
        var jsonp = await FetchJsonpAsync(DailyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.NotEmpty(result);
        Assert.True(result.Count <= 5);
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveDailyResponse_ParsesPackageIndexAsPositiveNumber()
    {
        var jsonp = await FetchJsonpAsync(DailyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.All(result, package => Assert.True(package.PackageIndex > 0));
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveDailyResponse_ParsesTitleAsNonEmpty()
    {
        var jsonp = await FetchJsonpAsync(DailyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.All(result, package => Assert.False(string.IsNullOrEmpty(package.Title)));
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveDailyResponse_ParsesSellerNameAsNonEmpty()
    {
        var jsonp = await FetchJsonpAsync(DailyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.All(result, package => Assert.False(string.IsNullOrEmpty(package.SellerName)));
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveDailyResponse_PrependsHttpsToThumbnailUrl()
    {
        var jsonp = await FetchJsonpAsync(DailyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.All(result, package => Assert.StartsWith("https://", package.ThumbnailUrl));
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveWeeklyResponse_ReturnsUpToFiveItems()
    {
        var jsonp = await FetchJsonpAsync(WeeklyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.NotEmpty(result);
        Assert.True(result.Count <= 5);
    }

    [Fact]
    public async Task ParsePopularDccon_WithLiveWeeklyResponse_ParsesPackageIndexAsPositiveNumber()
    {
        var jsonp = await FetchJsonpAsync(WeeklyPopularUrl);

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.All(result, package => Assert.True(package.PackageIndex > 0));
    }

    [Fact]
    public void ParsePopularDccon_WithEmptyString_ThrowsDcconParsingException()
    {
        Assert.Throws<dccon.NET.Exceptions.DcconParsingException>(
            () => JsonpResponseParser.ParsePopularDccon(""));
    }

    [Fact]
    public void ParsePopularDccon_WithNullString_ThrowsDcconParsingException()
    {
        Assert.Throws<dccon.NET.Exceptions.DcconParsingException>(
            () => JsonpResponseParser.ParsePopularDccon(null!));
    }

    [Fact]
    public void ParsePopularDccon_WithInvalidFormat_ThrowsDcconParsingException()
    {
        Assert.Throws<dccon.NET.Exceptions.DcconParsingException>(
            () => JsonpResponseParser.ParsePopularDccon("not jsonp format"));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
