using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventCampusAPI.Data;

namespace EventCampusAPI.Services
{
    public class TokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public TokenService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
    }
}