using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureActiveDirectoryIntegration.Web.Attributes
{
    public class ActiveDirectoryAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContextBase)
        {
            return httpContextBase.User.Identity.IsAuthenticated || base.AuthorizeCore(httpContextBase);
        }
    }
}