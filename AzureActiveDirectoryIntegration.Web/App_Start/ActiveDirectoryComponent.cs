using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace AzureActiveDirectoryIntegration.Web.App_Start
{
    public class ActiveDirectoryComponent : IComponent
    {
        private readonly string _clientId;
        private readonly IGlobalSettings _globalSettings;
        private readonly ILogger _logger;
        private readonly IMemberService _memberService;
        private readonly string _openIdAuthorizeUrl;
        private readonly string _redirectUrl;
        private readonly IRuntimeState _runtimeState;
        private readonly ISecuritySection _securitySection;
        private readonly string _tenantId;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IUserService _userService;

        public ActiveDirectoryComponent(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor,
            IRuntimeState runtimeState, IUserService userService, IGlobalSettings globalSettings,
            ISecuritySection securitySection, IMemberService memberService)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _runtimeState = runtimeState;
            _userService = userService;
            _globalSettings = globalSettings;
            _securitySection = securitySection;
            _memberService = memberService;

            _openIdAuthorizeUrl = ConfigurationManager.AppSettings["OpenId.AuthorizeUrl"];
            _tenantId = ConfigurationManager.AppSettings["AzureAd.TenantId"];
            _clientId = ConfigurationManager.AppSettings["AzureAd.ClientId"];
            _redirectUrl = ConfigurationManager.AppSettings["AzureAd.RedirectUrl"];
        }

        public void Initialize()
        {
            UmbracoDefaultOwinStartup.MiddlewareConfigured += ConfigureActiveDirectoryMiddleware;
        }

        public void Terminate()
        {
        }

        private void ConfigureActiveDirectoryMiddleware(object sender, OwinMiddlewareConfiguredEventArgs args)
        {
            var app = args.AppBuilder;

            app.AuthenticateFrontEndWithActiveDirectory(_clientId, _redirectUrl, _tenantId, _openIdAuthorizeUrl);

            app
                .UseUmbracoBackOfficeCookieAuthentication(_umbracoContextAccessor, _runtimeState, _userService, _globalSettings, _securitySection, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(_umbracoContextAccessor, _runtimeState, _globalSettings, PipelineStage.Authenticate);

            app.UseUmbracoPreviewAuthentication(_umbracoContextAccessor, _runtimeState, _globalSettings, _securitySection, PipelineStage.PostAuthenticate);
        }
    }
}