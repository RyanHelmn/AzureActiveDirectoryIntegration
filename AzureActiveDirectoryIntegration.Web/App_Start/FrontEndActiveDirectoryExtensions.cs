using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using OpenIdConnectMessage = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectMessage;

namespace AzureActiveDirectoryIntegration.Web.App_Start
{
    public static class FrontEndActiveDirectoryExtensions
    {
        public static void AuthenticateFrontEndWithActiveDirectory(this IAppBuilder app, string clientId,
            string redirectUrl,
            string tenantId, string authorizeUrl)
        {
            app.UseExternalSignInCookie();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions(), PipelineStage.Authenticate);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions

            {
                ClientId = clientId,
                Resource = clientId,
                Authority = $"{authorizeUrl}{tenantId}",
                PostLogoutRedirectUri = redirectUrl,
                RedirectUri = redirectUrl,
                ResponseType = OpenIdConnectResponseTypes.CodeIdToken,
                Scope = OpenIdConnectScopes.OpenIdProfile,
                Caption = "Active Directory Front End",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = SecurityTokenValidated
                }
            });
        }

        private static Task SecurityTokenValidated(
            SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> n)
        {
            var newClaimsIdentity = new ClaimsIdentity(n.AuthenticationTicket.Identity.AuthenticationType);

            newClaimsIdentity.AddClaims(n.AuthenticationTicket.Identity.Claims);

            if (!string.IsNullOrEmpty(n.ProtocolMessage.AccessToken))
            {
                newClaimsIdentity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));
            }

            if (!string.IsNullOrEmpty(n.ProtocolMessage.IdToken))
            {
                newClaimsIdentity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
            }

            n.AuthenticationTicket = new AuthenticationTicket(newClaimsIdentity, n.AuthenticationTicket.Properties);

            return Task.CompletedTask;
        }
    }
}