using N2.Core.Exceptions;

namespace N2.Core.Identity
{
    /// <summary>
    /// The web token exception.
    /// </summary>
    public class WebTokenException : N2CoreException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebTokenException"/> class.
        /// </summary>
        public WebTokenException() : base(Commands.ResponseStatus.Unauthorized)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTokenException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public WebTokenException(string message) : base(Commands.ResponseStatus.Unauthorized, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebTokenException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The exception.</param>
        public WebTokenException(string message, Exception e) : base(Commands.ResponseStatus.Unauthorized, message, e)
        {
        }
    }
}
