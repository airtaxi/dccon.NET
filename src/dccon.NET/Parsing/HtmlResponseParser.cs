using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Parser;
using dccon.NET.Exceptions;
using dccon.NET.Models;

namespace dccon.NET.Parsing
{
    /// <summary>
    /// DCcon 검색/목록 페이지 HTML을 파싱하여 구조화된 데이터로 변환
    /// </summary>
    internal static class HtmlResponseParser
    {
        /// <summary>
        /// 검색/목록 HTML에서 SearchResult를 추출한다.
        /// </summary>
        public static SearchResult ParseSearchResult(string html, int currentPage)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new DcconParsingException("파싱할 HTML이 비어있습니다.");

            try
            {
                var parser = new HtmlParser();
                var document = parser.ParseDocument(html);

                var result = new SearchResult
                {
                    CurrentPage = currentPage,
                    Packages = new List<DcconPackageSummary>()
                };

                // 총 건수 파싱: <span class="search_num">(155건)</span>
                var searchNumberElement = document.QuerySelector("span.search_num");
                if (searchNumberElement != null)
                {
                    var numberText = searchNumberElement.TextContent.Trim();
                    numberText = numberText.Replace("(", "").Replace("건)", "").Replace(",", "");
                    if (int.TryParse(numberText, out int totalCount))
                        result.TotalCount = totalCount;
                }

                // 패키지 목록 파싱
                var packageElements = document.QuerySelectorAll("li.div_package");
                foreach (var packageElement in packageElements)
                {
                    var packageIndexAttribute = packageElement.GetAttribute("package_idx");
                    if (string.IsNullOrEmpty(packageIndexAttribute))
                        continue;

                    if (!int.TryParse(packageIndexAttribute, out int packageIndex))
                        continue;

                    var titleElement = packageElement.QuerySelector(".dcon_name");
                    var sellerElement = packageElement.QuerySelector(".dcon_seller");
                    var thumbnailElement = packageElement.QuerySelector(".thumb_img");

                    var summary = new DcconPackageSummary
                    {
                        PackageIndex = packageIndex,
                        Title = titleElement?.TextContent?.Trim() ?? string.Empty,
                        SellerName = sellerElement?.TextContent?.Trim() ?? string.Empty,
                        ThumbnailUrl = thumbnailElement?.GetAttribute("src") ?? string.Empty
                    };

                    result.Packages.Add(summary);
                }

                // 페이징 파싱: .bottom_paging_box 내 링크들에서 마지막 페이지 번호 추출
                var pagingBox = document.QuerySelector(".bottom_paging_box");
                if (pagingBox != null)
                {
                    var pageLinks = pagingBox.QuerySelectorAll("a")
                        .Where(a => !a.ClassList.Contains("sp_pagingicon"))
                        .ToList();

                    // .page_end 링크에서 마지막 페이지 번호 추출
                    var endLink = pagingBox.QuerySelector("a.page_end");
                    if (endLink != null)
                    {
                        var href = endLink.GetAttribute("href") ?? string.Empty;
                        result.TotalPages = ExtractPageNumberFromUrl(href);
                    }
                    else if (pageLinks.Any())
                    {
                        // page_end가 없으면 마지막 일반 페이지 링크
                        var lastLink = pageLinks.Last();
                        var lastText = lastLink.TextContent.Trim();
                        if (int.TryParse(lastText, out int lastPage))
                            result.TotalPages = lastPage;
                    }

                    // 페이징이 없으면 (결과가 1페이지 이하) 현재 페이지 = 총 페이지
                    if (result.TotalPages == 0 && result.Packages.Count > 0)
                        result.TotalPages = 1;
                }
                else if (result.Packages.Count > 0)
                {
                    result.TotalPages = 1;
                }

                return result;
            }
            catch (DcconParsingException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new DcconParsingException("HTML 파싱 중 오류가 발생했습니다.", exception);
            }
        }

        private static int ExtractPageNumberFromUrl(string url)
        {
            // URL 형식: https://dccon.dcinside.com/hot/11/title/페페
            // 또는: https://dccon.dcinside.com/hot/5
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // segments: ["hot", "11", "title", "페페"] 또는 ["hot", "5"]
                if (segments.Length >= 2 && int.TryParse(segments[1], out int pageNumber))
                    return pageNumber;
            }
            catch
            {
                // URL 파싱 실패 시 무시
            }

            return 0;
        }
    }
}
