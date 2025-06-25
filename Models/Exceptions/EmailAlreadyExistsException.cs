using System;

namespace EventCampusAPI.Models.Exceptions
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException() : base("Bu email adresi zaten kullanılıyor.")
        {
        }

        public EmailAlreadyExistsException(string message) : base(message)
        {
        }

        public EmailAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 