using System;

namespace dccon.NET.Exceptions;

/// <summary>
/// 요청한 디시콘 패키지를 찾을 수 없을 때 발생하는 예외
/// </summary>
public class DcconNotFoundException : DcconException
{
    /// <inheritdoc />
    public DcconNotFoundException()
    {
    }

    /// <inheritdoc />
    public DcconNotFoundException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public DcconNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
