using AzureActiveDirectoryIntegration.Web.Attributes;
using Umbraco.Web.Mvc;

namespace AzureActiveDirectoryIntegration.Web.Controllers
{
    [ActiveDirectoryAuthorize]
    public class CustomRenderMvcController : RenderMvcController
    {
    }
}