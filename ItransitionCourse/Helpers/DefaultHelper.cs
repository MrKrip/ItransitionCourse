using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Helpers
{
    public class DefaultHelper:IHelper
    {
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
                result = ((await _userManager.GetUserAsync(httpContext.User)).Id == id) || (httpContext.User.IsInRole("Admin"));
            }
            return result;
        }


    }
}
