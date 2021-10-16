using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Models
{
    public class AdminViewModel
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public List<string> UserRoles { get; set; }
    }
}
