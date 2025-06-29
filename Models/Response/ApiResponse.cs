using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Models.Response
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
} 