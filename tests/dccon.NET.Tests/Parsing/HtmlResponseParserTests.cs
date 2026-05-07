using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using dccon.NET.Parsing;
using Xunit;

namespace dccon.NET.Tests.Parsing;

public class HtmlResponseParserTests : IDisposable
{
    private readonly HttpClient _httpClient = new();

    private async Task<string> FetchNewListHtmlAsync()
    {
        var response = await _httpClient.GetAsync("https://dccon.dcinside.com/new/1");
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes);
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ReturnsNonEmptyPackages()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.NotEmpty(result.Packages);
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ReturnsCurrentPageOne()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ReturnsTotalPagesGreaterThanZero()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.True(result.TotalPages > 0);
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ParsesPackageIndexAsPositiveNumber()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.All(result.Packages, package => Assert.True(package.PackageIndex > 0));
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ParsesTitleAsNonEmpty()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.All(result.Packages, package => Assert.False(string.IsNullOrEmpty(package.Title)));
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ParsesSellerNameAsNonEmpty()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.All(result.Packages, package => Assert.False(string.IsNullOrEmpty(package.SellerName)));
    }

    [Fact]
    public async Task ParseSearchResult_WithLiveNewListHtml_ParsesThumbnailUrlAsNonEmpty()
    {
        var html = await FetchNewListHtmlAsync();

        var result = HtmlResponseParser.ParseSearchResult(html, 1);

        Assert.All(result.Packages, package => Assert.False(string.IsNullOrEmpty(package.ThumbnailUrl)));
    }

    [Fact]
    public void ParseSearchResult_WithEmptyHtml_ThrowsDcconParsingException()
    {
        Assert.Throws<dccon.NET.Exceptions.DcconParsingException>(
            () => HtmlResponseParser.ParseSearchResult("", 1));
    }

    [Fact]
    public void ParseSearchResult_WithNullHtml_ThrowsDcconParsingException()
    {
        Assert.Throws<dccon.NET.Exceptions.DcconParsingException>(
            () => HtmlResponseParser.ParseSearchResult(null!, 1));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
