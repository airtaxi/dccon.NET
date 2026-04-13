# dccon.NET

비공식 디시인사이드 디시콘(DCcon) .NET 라이브러리

[![NuGet](https://img.shields.io/nuget/v/dccon.NET.svg)](https://www.nuget.org/packages/dccon.NET)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/dotnet/standard/net-standard)

## 소개

dccon.NET은 디시인사이드의 디시콘(DCcon) 스티커를 프로그래밍적으로 검색, 조회, 다운로드할 수 있는 .NET 라이브러리입니다.

> ⚠️ **주의**: 이 라이브러리는 비공식이며, 디시인사이드의 사이트 구조 변경 시 동작하지 않을 수 있습니다. 과도한 자동 요청은 IP 차단의 원인이 될 수 있으니 주의하세요.

## 기능

- 🔍 **디시콘 검색** — 디시콘명, 닉네임, 태그로 검색 (인기순/최신순 정렬)
- 📋 **목록 조회** — 인기 디시콘 / 최신 디시콘 목록
- 🔥 **인기 디시콘** — 일간/주간 인기 디시콘 Top 5 조회
- 📦 **패키지 상세** — 패키지 정보, 스티커 목록, 태그 조회
- ⬇️ **이미지 다운로드** — 개별 스티커 또는 패키지 전체 일괄 다운로드
- ⚡ **병렬 다운로드** — 패키지 전체 다운로드 시 병렬 처리 + 진행 상태 콜백
- 🚀 **NativeAOT 호환** — System.Text.Json 소스 제너레이션 기반, NativeAOT 배포 지원

## 설치

```bash
dotnet add package dccon.NET
```

또는 NuGet Package Manager에서 `dccon.NET`을 검색하세요.

## 사용법

### 기본 사용

```csharp
using dccon.NET;
using dccon.NET.Models;

using var client = new DcconClient();

// 디시콘 검색
var result = await client.SearchAsync("페페");
foreach (var package in result.Packages)
    Console.WriteLine($"[{package.PackageIndex}] {package.Title} - {package.SellerName}");
```

### 인기/최신 목록 조회

```csharp
// 인기 디시콘
var hotList = await client.GetHotListAsync(page: 1);

// 최신 디시콘
var newList = await client.GetNewListAsync(page: 1);
```

### 일간/주간 인기 디시콘

```csharp
// 일간 인기 디시콘 Top 5
var dailyPopular = await client.GetDailyPopularAsync();
foreach (var package in dailyPopular)
    Console.WriteLine($"[{package.PackageIndex}] {package.Title} - {package.SellerName}");

// 주간 인기 디시콘 Top 5
var weeklyPopular = await client.GetWeeklyPopularAsync();
```

### 검색 옵션

```csharp
// 태그로 검색, 최신순 정렬, 2페이지
var result = await client.SearchAsync(
    query: "고양이",
    searchType: SearchType.Tags,
    sort: SearchSort.New,
    page: 2);
```

### 패키지 상세 조회

```csharp
var detail = await client.GetPackageDetailAsync(packageIndex: 42885);

Console.WriteLine($"제목: {detail.Title}");
Console.WriteLine($"제작자: {detail.SellerName}");
Console.WriteLine($"스티커 수: {detail.Stickers.Count}개");
Console.WriteLine($"태그: {string.Join(", ", detail.Tags)}");
```

### 스티커 다운로드

```csharp
// 개별 스티커 다운로드 (byte[])
var sticker = detail.Stickers[0];
byte[] imageData = await client.DownloadStickerAsync(sticker);

// 스트림으로 다운로드
using var stream = await client.DownloadStickerStreamAsync(sticker);
```

### 패키지 전체 다운로드

```csharp
var progress = new Progress<(int Completed, int Total)>(report =>
    Console.WriteLine($"다운로드 중... {report.Completed}/{report.Total}"));

await client.DownloadPackageAsync(
    packageIndex: 42885,
    outputDirectory: @"C:\Downloads",
    progress: progress);
```

### 다운로드 파일명 예측

`DcconFileNameHelper`를 사용하면 다운로드 시 사용되는 파일명을 사전에 알 수 있습니다:

```csharp
var detail = await client.GetPackageDetailAsync(packageIndex: 42885);

foreach (var sticker in detail.Stickers)
{
    // DownloadPackageAsync에서 저장되는 파일명과 동일
    string fileName = DcconFileNameHelper.GetStickerFileName(sticker);
    Console.WriteLine(fileName); // 예: "페페웃음.png"
}

// 파일명 안전 변환만 필요한 경우
string safeName = DcconFileNameHelper.SanitizeFileName("잘못된/파일:명");
```

### HttpClient 주입 (DI 패턴)

```csharp
// IHttpClientFactory 패턴과 호환
var httpClient = httpClientFactory.CreateClient();
using var client = new DcconClient(httpClient);
```

### CancellationToken 지원

모든 비동기 메서드에서 `CancellationToken`을 지원합니다:

```csharp
using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await client.SearchAsync("페페", cancellationToken: cancellationTokenSource.Token);
```

## API 참조

### `IDcconClient` 인터페이스

| 메서드 | 설명 |
|--------|------|
| `SearchAsync` | 디시콘 검색 (디시콘명/닉네임/태그, 인기순/최신순) |
| `GetHotListAsync` | 인기 디시콘 목록 조회 |
| `GetNewListAsync` | 최신 디시콘 목록 조회 |
| `GetDailyPopularAsync` | 일간 인기 디시콘 Top 5 조회 |
| `GetWeeklyPopularAsync` | 주간 인기 디시콘 Top 5 조회 |
| `GetPackageDetailAsync` | 패키지 상세 정보 조회 |
| `DownloadStickerAsync` | 스티커 이미지 byte[] 다운로드 |
| `DownloadStickerStreamAsync` | 스티커 이미지 Stream 다운로드 |
| `DownloadPackageAsync` | 패키지 전체 일괄 다운로드 |

### 유틸리티

| 클래스 | 메서드 | 설명 |
|--------|--------|------|
| `DcconFileNameHelper` | `SanitizeFileName` | 파일명에 사용할 수 없는 문자를 제거 |
| `DcconFileNameHelper` | `GetStickerFileName` | 스티커 다운로드 시 사용되는 파일명 반환 |

### 모델

| 클래스 | 설명 |
|--------|------|
| `SearchResult` | 검색 결과 (패키지 목록 + 페이지네이션) |
| `DcconPackageSummary` | 패키지 요약 (검색 결과 항목) |
| `DcconPackageDetail` | 패키지 상세 (스티커 목록 + 태그 포함) |
| `DcconSticker` | 개별 스티커 정보 |
| `SearchSort` | 정렬 방식 (Hot, New) |
| `SearchType` | 검색 유형 (Title, NickName, Tags) |

## 요구 사항

- .NET Standard 2.0 호환 런타임 (.NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)

## 의존성

- [AngleSharp](https://anglesharp.github.io/) — HTML 파싱
- [System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview) — JSON 역직렬화 (소스 제너레이션 기반, NativeAOT 호환)

## 라이선스

MIT License

## 작성자

**이호원**

## 감사의 말

이 프로젝트는 [GitHub Copilot](https://github.com/features/copilot)의 도움을 받아 작성되었습니다.
