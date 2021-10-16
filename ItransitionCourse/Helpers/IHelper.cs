using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ItransitionCourse.Helpers
{
    public interface IHelper
    {
        public Task<bool> PermissionToEdit(HttpContext httpContext, UserManager<IdentityUser> _userManager, string id);
        public Task<bool> IsInRole(ClaimsPrincipal User, UserManager<IdentityUser> _userManager, string role);
        public List<string> UserRoles(string userId, UserManager<IdentityUser> _userManager);
    }
}
