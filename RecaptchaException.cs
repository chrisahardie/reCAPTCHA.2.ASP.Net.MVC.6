using System;

namespace reCAPTCHA
{
    public class reCAPTCHAException : Exception
    {
        public reCAPTCHAException()
        {
        }

        public reCAPTCHAException(string message)
        : base(message)
        {
        }

        public reCAPTCHAException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
