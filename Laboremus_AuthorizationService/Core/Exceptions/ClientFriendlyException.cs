using System;
using System.Collections.Generic;

namespace Laboremus_AuthorizationService.Core.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Invalid Model Exception
    /// </summary>
    public class InvalidModelException : Exception
    {
        /// <inheritdoc />
        public InvalidModelException(string message, List<string> errors) : base(message) { }
    }
}
