using System;

namespace Laboremus_AuthorizationService.Core.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Exception to pass message to client
    /// </summary>
    public class ClientFriendlyException : Exception
    {
        /// <inheritdoc />
        public ClientFriendlyException(string message) : base(message) { }
    }
}
