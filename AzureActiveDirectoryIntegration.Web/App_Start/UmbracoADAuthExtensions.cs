using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Serilog;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.IdentityExtensions;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace AzureActiveDirectoryIntegration.Web
{
    public static class UmbracoADAuthExtensions
    {
        /// <summary>
        ///     Configure ActiveDirectory sign-in
        /// </summary>
        /// <param name="app"></param>
        /// <param name="tenant">
        ///     Your tenant ID i.e. YOURDIRECTORYNAME.onmicrosoft.com OR this could be the GUID of your tenant ID
        /// </param>
        /// <param name="clientId">
        ///     Also known as the Application Id in the azure portal
        /// </param>
        /// <param name="postLoginRedirectUri">
        ///     The URL that will be redirected to after login is successful, example: http://mydomain.com/umbraco/;
        /// </param>
        /// <param name="issuerId">
        ///     This is the "Issuer Id" for you Azure AD application. This is a GUID value of your tenant ID.
        ///     If this value is not set correctly then accounts won't be able to be detected
        ///     for un-linking in the back office.
        /// </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///     ActiveDirectory account documentation for ASP.Net Identity can be found:
        ///     https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
        /// </remarks>
        public static void ConfigureBackOfficeAzureActiveDirectoryAuth(this IAppBuilder app,
            string tenant, string clientId, string postLoginRedirectUri, Guid issuerId,
            string caption = "Active Directory", string style = "btn-microsoft", string icon = "fa-windows")
        {

            var authority = string.Format(
                CultureInfo.InvariantCulture,
                "https://login.windows.net/{0}",
                tenant);

            var adOptions = new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                ClientId = clientId,
                Authority = authority,
                RedirectUri = postLoginRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async context =>
                    {
                        var userService = Current.Services.UserService;

                        var email = context.JwtSecurityToken.Claims.First(x => x.Type.Equals("unique_name")).Value;
                        var issuer = context.JwtSecurityToken.Claims.First(x => x.Type.Equals("iss")).Value;
                        var providerKey = context.JwtSecurityToken.Claims.First(x => x.Type.Equals("sub")).Value;
                        var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

                        var user = userService.GetByEmail(email);

                        if (user == null)
                        {
                            Log.Logger.Error($"The user {email} does not exist in Umbraco.");
                            return;
                        }

                        var identity = await userManager.FindByEmailAsync(email);
                        var identityClaims = await userManager.GenerateUserIdentityAsync(identity);
                        context.OwinContext.Authentication.SignIn(identityClaims);
               
                        //if (identity.Logins.All(x => {
                        //    return !x.ProviderKey.Equals(providerKey);
                        //}))
                        //{
                        //    identity.Logins.Add(new IdentityUserLogin(issuer, providerKey, user.Id));
                        //    await userManager.UpdateAsync(identity);
                        //}
                    }
                }
            };

            adOptions.ForUmbracoBackOffice(style, icon);
            adOptions.Caption = caption;
            //adOptions.AuthenticationType = string.Format(
            //    CultureInfo.InvariantCulture,
            //    "https://sts.windows.net/{0}/",
            //    issuerId);

            app.UseOpenIdConnectAuthentication(adOptions);

            adOptions.SetExternalSignInAutoLinkOptions(new ExternalSignInAutoLinkOptions(true));
        }
        public static void UseUmbracoBackOfficeTokenAuth(this IAppBuilder app,
    BackOfficeAuthServerProviderOptions backofficeAuthServerProviderOptions = null)
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/umbraco/oauth/token"),

                //set as different auth type to not interfere with anyone doing this on the front-end
                AuthenticationType = Constants.Security.BackOfficeTokenAuthenticationType,
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new BackOfficeAuthServerProvider(backofficeAuthServerProviderOptions)
                {
                    OnValidateClientAuthentication = context =>
                    {
                        context.Validated();
                        return Task.FromResult(0);
                    }
                }
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}