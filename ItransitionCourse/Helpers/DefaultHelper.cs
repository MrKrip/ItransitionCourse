using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ItransitionCourse.Helpers
{
    public class DefaultHelper:IHelper
    {
        public async Task<bool> IsInRole(ClaimsPrincipal User, UserManager<IdentityUser> _userManager, string role)
        {            
            return (await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User))).Contains(role);
        }

        public async Task<bool> PermissionToEdit(HttpContext httpContext, UserManager<IdentityUser> _userManager, string id)
        {
            bool result;
            var CurrentUser = await _userManager.GetUserAsync(httpContext.User);
            if (CurrentUser == null)
            {
                result = false;
            }
            else
            {
                result = ((await _userManager.GetUserAsync(httpContext.User)).Id == id) || (await IsInRole(httpContext.User,_userManager,"Admin"));
            }
            return result;
        }

        public List<string> UserRoles(string userId, UserManager<IdentityUser> _userManager)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            return userRoles.ToList();
        }
    }
}
