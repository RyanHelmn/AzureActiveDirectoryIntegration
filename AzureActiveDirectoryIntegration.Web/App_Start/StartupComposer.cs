using AzureActiveDirectoryIntegration.Web.Controllers;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;

namespace AzureActiveDirectoryIntegration.Web.App_Start
{
    public class StartupComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            // Components
            composition.Components()
                .Append<ActiveDirectoryComponent>();

            // Services

            composition.SetDefaultRenderMvcController<CustomRenderMvcController>();
        }
    }
}