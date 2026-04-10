using System;
using System.Linq;
using System.Threading.Tasks;
using dccon.NET;
using dccon.NET.Models;

namespace dccon.NET.Sample;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var client = new DcconClient();

        // 1. 인기 디시콘 목록
        Console.WriteLine("=== 인기 디시콘 목록 ===");
        var hotList = await client.GetHotListAsync();
        Console.WriteLine($"총 {hotList.Packages.Count}개 (페이지 {hotList.CurrentPage}/{hotList.TotalPages})");
        foreach (var package in hotList.Packages.Take(5))
            Console.WriteLine($"  [{package.PackageIndex}] {package.Title} - {package.SellerName}");

        Console.WriteLine();

        // 2. 디시콘 검색
        Console.Write("검색어를 입력하세요: ");
        var query = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(query))
            query = "페페";

        Console.WriteLine($"\n=== '{query}' 검색 결과 ===");
        var searchResult = await client.SearchAsync(query);
        Console.WriteLine($"총 {searchResult.TotalCount}건 (페이지 {searchResult.CurrentPage}/{searchResult.TotalPages})");
        foreach (var package in searchResult.Packages)
            Console.WriteLine($"  [{package.PackageIndex}] {package.Title} - {package.SellerName}");

        if (searchResult.Packages.Count == 0)
        {
            Console.WriteLine("검색 결과가 없습니다.");
            return;
        }

        // 3. 패키지 상세 조회
        var firstPackage = searchResult.Packages.First();
        Console.WriteLine($"\n=== 패키지 상세: {firstPackage.Title} ===");
        var detail = await client.GetPackageDetailAsync(firstPackage.PackageIndex);
        Console.WriteLine($"제목: {detail.Title}");
        Console.WriteLine($"설명: {detail.Description}");
        Console.WriteLine($"제작자: {detail.SellerName}");
        Console.WriteLine($"등록일: {detail.RegistrationDate}");
        Console.WriteLine($"태그: {string.Join(", ", detail.Tags)}");
        Console.WriteLine($"스티커 수: {detail.Stickers.Count}개");
        foreach (var sticker in detail.Stickers)
            Console.WriteLine($"  - {sticker.Title}.{sticker.Extension}");

        // 4. 패키지 전체 다운로드
        Console.Write("\n이 패키지를 다운로드하시겠습니까? (y/n): ");
        var answer = Console.ReadLine();
        if (string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
        {
            var outputDirectory = Environment.CurrentDirectory;
            Console.WriteLine($"다운로드 위치: {outputDirectory}");

            var progress = new Progress<(int Completed, int Total)>(report =>
                Console.WriteLine($"  다운로드 중... {report.Completed}/{report.Total}"));

            await client.DownloadPackageAsync(firstPackage.PackageIndex, outputDirectory, progress);
            Console.WriteLine("다운로드 완료!");
        }
    }
}
