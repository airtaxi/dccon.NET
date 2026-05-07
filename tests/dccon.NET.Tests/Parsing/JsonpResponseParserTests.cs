using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using dccon.NET.Models;
using dccon.NET.Parsing;
using Xunit;

namespace dccon.NET.Tests.Parsing;

public class JsonpResponseParserTests : IDisposable
{
    private const string DailyPopularRequestUri = "https://json2.dcinside.com/json1/dccon_day_top100.php?jsoncallback=day_top100";
    private const string WeeklyPopularRequestUri = "https://json2.dcinside.com/json1/dccon_week_top100.php?jsoncallback=week_top100";
    private const string MonthlyPopularRequestUri = "https://json2.dcinside.com/json1/dccon_month_top100.php?jsoncallback=month_top100";

    private readonly HttpClient _httpClient = new();

    private async Task<string> FetchJsonPaddingResponseAsync(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    private async Task<List<DcconPackageSummary>> FetchPopularPackagesAsync(string requestUri)
    {
        var jsonPaddingResponse = await FetchJsonPaddingResponseAsync(requestUri);
        return JsonpResponseParser.ParsePopularDccon(jsonPaddingResponse);
    }

    [Theory]
    [InlineData(DailyPopularRequestUri)]
    [InlineData(WeeklyPopularRequestUri)]
    [InlineData(MonthlyPopularRequestUri)]
    public async Task ParsePopularDccon_WithLiveTop100Response_ReturnsMoreThanFiveAndUpToOneHundredItems(string requestUri)
    {
        var result = await FetchPopularPackagesAsync(requestUri);

        Assert.NotEmpty(result);
        Assert.True(result.Count > 5);
        Assert.True(result.Count <= 100);
    }

    [Theory]
    [InlineData(DailyPopularRequestUri)]
    [InlineData(WeeklyPopularRequestUri)]
    [InlineData(MonthlyPopularRequestUri)]
    public async Task ParsePopularDccon_WithLiveTop100Response_ParsesPackageIndexAsPositiveNumber(string requestUri)
    {
        var result = await FetchPopularPackagesAsync(requestUri);

        Assert.All(result, package => Assert.True(package.PackageIndex > 0));
    }

    [Theory]
    [InlineData(DailyPopularRequestUri)]
    [InlineData(WeeklyPopularRequestUri)]
    [InlineData(MonthlyPopularRequestUri)]
    public async Task ParsePopularDccon_WithLiveTop100Response_ParsesTitleAsNonEmpty(string requestUri)
    {
        var result = await FetchPopularPackagesAsync(requestUri);

        Assert.All(result, package => Assert.False(string.IsNullOrEmpty(package.Title)));
    }

    [Theory]
    [InlineData(DailyPopularRequestUri)]
    [InlineData(WeeklyPopularRequestUri)]
    [InlineData(MonthlyPopularRequestUri)]
    public async Task ParsePopularDccon_WithLiveTop100Response_ParsesSellerNameAsNonEmpty(string requestUri)
    {
        var result = await FetchPopularPackagesAsync(requestUri);

        Assert.All(result, package => Assert.False(string.IsNullOrEmpty(package.SellerName)));
    }

    [Theory]
    [InlineData(DailyPopularRequestUri)]
    [InlineData(WeeklyPopularRequestUri)]
    [InlineData(MonthlyPopularRequestUri)]
    public async Task ParsePopularDccon_WithLiveTop100Response_PrependsHttpsToThumbnailUrl(string requestUri)
    {
        var result = await FetchPopularPackagesAsync(requestUri);

        Assert.All(result, package => Assert.StartsWith("https://", package.ThumbnailUrl));
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
