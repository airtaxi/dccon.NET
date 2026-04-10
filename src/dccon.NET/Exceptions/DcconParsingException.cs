using System;

namespace dccon.NET.Exceptions;

/// <summary>
/// HTML 응답 파싱 실패 시 발생하는 예외
/// </summary>
public class DcconParsingException : DcconException
{
    /// <inheritdoc />
    public DcconParsingException()
    {
    }

    /// <inheritdoc />
    public DcconParsingException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public DcconParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
