using System;
using System.Collections.Generic;

namespace EventCampusAPI.Models
{
    public class ValidationException : Exception
    {
        public List<ValidationError> Errors { get; set; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<ValidationError>();
        }

        public ValidationException(List<ValidationError> errors) : base("Validation failed")
        {
            Errors = errors;
        }
    }

    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }
} 