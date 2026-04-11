using System.IO;
using System.Reflection;
using dccon.NET.Parsing;
using Xunit;

namespace dccon.NET.Tests.Parsing;

public class JsonpResponseParserTests
{
    private static string LoadTestData(string fileName)
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var filePath = Path.Combine(assemblyDirectory, "TestData", fileName);
        return File.ReadAllText(filePath);
    }

    [Fact]
    public void ParsePopularDccon_WithDailySample_ReturnsCorrectCount()
    {
        var jsonp = LoadTestData("daily_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void ParsePopularDccon_WithDailySample_ParsesPackageIndexCorrectly()
    {
        var jsonp = LoadTestData("daily_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Equal(169601, result[0].PackageIndex);
        Assert.Equal(168838, result[1].PackageIndex);
        Assert.Equal(169458, result[2].PackageIndex);
    }

    [Fact]
    public void ParsePopularDccon_WithDailySample_ParsesTitleCorrectly()
    {
        var jsonp = LoadTestData("daily_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Equal("트릭컬 뎅글콘6알파", result[0].Title);
        Assert.Equal("이상한 동물농장", result[1].Title);
    }

    [Fact]
    public void ParsePopularDccon_WithDailySample_ParsesSellerNameCorrectly()
    {
        var jsonp = LoadTestData("daily_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Equal("거북새", result[0].SellerName);
        Assert.Equal("as12", result[1].SellerName);
        Assert.Equal("밤우", result[4].SellerName);
    }

    [Fact]
    public void ParsePopularDccon_WithDailySample_PrependHttpsToThumbnailUrl()
    {
        var jsonp = LoadTestData("daily_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.StartsWith("https://", result[0].ThumbnailUrl);
        Assert.Contains("abc123", result[0].ThumbnailUrl);
    }

    [Fact]
    public void ParsePopularDccon_WithWeeklySample_ReturnsCorrectCount()
    {
        var jsonp = LoadTestData("weekly_popular_sample.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ParsePopularDccon_WithEmptyArray_ReturnsEmptyList()
    {
        var jsonp = LoadTestData("popular_empty.jsonp");

        var result = JsonpResponseParser.ParsePopularDccon(jsonp);

        Assert.Empty(result);
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
}
