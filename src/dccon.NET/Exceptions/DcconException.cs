using System;

namespace dccon.NET.Exceptions
{
    /// <summary>
    /// 디시콘 라이브러리의 기본 예외
    /// </summary>
    public class DcconException : Exception
    {
        /// <inheritdoc />
        public DcconException()
        {
        }

        /// <inheritdoc />
        public DcconException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DcconException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
