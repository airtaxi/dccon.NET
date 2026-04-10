using System.IO;
using System.Reflection;
using dccon.NET.Parsing;
using Xunit;

namespace dccon.NET.Tests.Parsing
{
    public class HtmlResponseParserTests
    {
        private static string LoadTestData(string fileName)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var filePath = Path.Combine(assemblyDirectory, "TestData", fileName);
            return File.ReadAllText(filePath);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ReturnsCorrectPackageCount()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal(3, result.Packages.Count);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ReturnsCorrectTotalCount()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ReturnsCorrectCurrentPage()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal(1, result.CurrentPage);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ReturnsCorrectTotalPages()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ParsesPackageIndexCorrectly()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal(12345, result.Packages[0].PackageIndex);
            Assert.Equal(67890, result.Packages[1].PackageIndex);
            Assert.Equal(11111, result.Packages[2].PackageIndex);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ParsesTitleCorrectly()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal("테스트콘1", result.Packages[0].Title);
            Assert.Equal("테스트콘2", result.Packages[1].Title);
            Assert.Equal("테스트콘3", result.Packages[2].Title);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ParsesSellerNameCorrectly()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Equal("판매자A", result.Packages[0].SellerName);
            Assert.Equal("판매자B", result.Packages[1].SellerName);
            Assert.Equal("판매자C", result.Packages[2].SellerName);
        }

        [Fact]
        public void ParseSearchResult_WithValidHtml_ParsesThumbnailUrlCorrectly()
        {
            var html = LoadTestData("search_result_sample.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Contains("abc123", result.Packages[0].ThumbnailUrl);
            Assert.Contains("def456", result.Packages[1].ThumbnailUrl);
        }

        [Fact]
        public void ParseSearchResult_WithEmptyResult_ReturnsZeroPackages()
        {
            var html = LoadTestData("search_result_empty.html");

            var result = HtmlResponseParser.ParseSearchResult(html, 1);

            Assert.Empty(result.Packages);
            Assert.Equal(0, result.TotalCount);
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
    }
}
