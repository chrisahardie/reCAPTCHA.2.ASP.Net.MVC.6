using System;

namespace Recaptcha
{
    public class RecaptchaException : Exception
    {
        public RecaptchaException()
        {
        }

        public RecaptchaException(string message)
        : base(message)
        {
        }

        public RecaptchaException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
