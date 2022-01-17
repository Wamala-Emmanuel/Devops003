using System;
using System.Collections.Generic;

namespace Laboremus_AuthorizationService.Core.Exceptions
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message, List<string> errors) : base(message) { }
    }
}
