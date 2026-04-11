using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dccon.NET.Json;

/// <summary>
/// NativeAOT 호환 소스 제너레이션 JSON 직렬화 컨텍스트
/// </summary>
[JsonSerializable(typeof(PackageDetailResponse))]
[JsonSerializable(typeof(List<PopularDcconResponse>))]
internal partial class DcconJsonContext : JsonSerializerContext
{
}
