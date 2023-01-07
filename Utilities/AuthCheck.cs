using MedicalSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedicalSystem.Utilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizer : Attribute, IAllowAnonymous, IAuthorizationFilter
    {
        private readonly string Roles;
        private readonly AuthorizationCheckType CheckType;
        public CustomAuthorizer(string roles, AuthorizationCheckType authorizationCheckType)
        {
            Roles = roles;
            CheckType = authorizationCheckType;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            switch (CheckType)
            {
                case AuthorizationCheckType.AuthorizeByRole:
                    {
                        var result = AuthorizeByRole(context);
                        if (result != null)
                            return;
                        break;
                    }
            }

            return;
        }

        private AuthorizationFilterContext AuthorizeByRole(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext
                  .RequestServices
                  .GetService(typeof(AppDbContext)) as AppDbContext;

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var roles = Roles.Split(',');

            foreach(var _role in roles)
            {
                var check = context.HttpContext.User.IsInRole(_role);
                if (check){
                    continue;
                }

                context.Result = new UnauthorizedResult();
                return context;
            }

            return null;
        }
    }

    public enum AuthorizationCheckType
    {
        AuthorizeByRole,
        AuthorizeByUser
    }
}
