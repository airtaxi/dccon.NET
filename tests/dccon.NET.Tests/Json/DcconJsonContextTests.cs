using System.IO;
using System.Reflection;
using System.Text.Json;
using dccon.NET.Json;
using Xunit;

namespace dccon.NET.Tests.Json;

/// <summary>
/// NativeAOT 호환 소스 제너레이션 JSON 직렬화/역직렬화 테스트
/// </summary>
public class DcconJsonContextTests
{
    private static string LoadTestData(string fileName)
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var filePath = Path.Combine(assemblyDirectory, "TestData", fileName);
        return File.ReadAllText(filePath);
    }

    [Fact]
    public void Deserialize_WithValidJson_ReturnsPackageDetailResponse()
    {
        var json = LoadTestData("package_detail_sample.json");

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse);

        Assert.NotNull(response);
        Assert.NotNull(response.Info);
    }

    [Fact]
    public void Deserialize_WithValidJson_ParsesInfoCorrectly()
    {
        var json = LoadTestData("package_detail_sample.json");

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.Equal(42885, response.Info!.PackageIndex);
        Assert.Equal("테스트 디시콘", response.Info.Title);
        Assert.Equal("테스트 설명", response.Info.Description);
        Assert.Equal("test_main_path", response.Info.MainImagePath);
        Assert.Equal("테스트판매자", response.Info.SellerName);
        Assert.Equal("2024.01.01", response.Info.RegistrationDate);
        Assert.Equal("S", response.Info.State);
    }

    [Fact]
    public void Deserialize_WithValidJson_ParsesStickersCorrectly()
    {
        var json = LoadTestData("package_detail_sample.json");

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Detail);
        Assert.Equal(2, response.Detail.Count);
        Assert.Equal("sticker_path_1", response.Detail[0].Path);
        Assert.Equal("스티커1", response.Detail[0].Title);
        Assert.Equal("png", response.Detail[0].Extension);
        Assert.Equal(1, response.Detail[0].SortNumber);
        Assert.Equal("sticker_path_2", response.Detail[1].Path);
        Assert.Equal("gif", response.Detail[1].Extension);
    }

    [Fact]
    public void Deserialize_WithValidJson_ParsesTagsCorrectly()
    {
        var json = LoadTestData("package_detail_sample.json");

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Tags);
        Assert.Equal(3, response.Tags.Count);
        Assert.Equal("태그1", response.Tags[0].Tag);
        Assert.Equal("태그2", response.Tags[1].Tag);
        Assert.Equal("태그3", response.Tags[2].Tag);
    }

    [Fact]
    public void Deserialize_WithEmptyArrays_ReturnsEmptyCollections()
    {
        var json = LoadTestData("package_detail_empty_arrays.json");

        var response = JsonSerializer.Deserialize(json, DcconJsonContext.Default.PackageDetailResponse)!;

        Assert.NotNull(response.Info);
        Assert.Equal(99999, response.Info.PackageIndex);
        Assert.NotNull(response.Detail);
        Assert.Empty(response.Detail);
        Assert.NotNull(response.Tags);
        Assert.Empty(response.Tags);
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
    public void Deserialize_PopularDcconArray_ParsesCorrectly()
    {
        var json = """[{"package_idx":"12345","title":"테스트콘","nick_name":"테스터","price":"0","img":"//test.com/img.png"}]""";

        var result = JsonSerializer.Deserialize(json, DcconJsonContext.Default.ListPopularDcconResponse);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("12345", result[0].PackageIndex);
        Assert.Equal("테스트콘", result[0].Title);
        Assert.Equal("테스터", result[0].NickName);
        Assert.Equal("0", result[0].Price);
        Assert.Equal("//test.com/img.png", result[0].ImageUrl);
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
    public void Deserialize_WithInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "not valid json";

        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize(invalidJson, DcconJsonContext.Default.PackageDetailResponse));
    }
}
