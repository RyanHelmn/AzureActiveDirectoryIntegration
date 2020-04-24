# Packages Installed
- UmbracoCms.IdentityExtensions
- UmbracoCms.IdentityExtensions.AzureActiveDirectory

# Files Deleted
Delete any owin startup and AD extension files that the UmbracoCms.IdentityExtensions may add

# Files Added
~/App_Start

- ActiveDirectoryComponent
(Hooks into the Umbraco Middleware)
- FrontEndActiveDirectoryExtensions
(Handles all front end logic for hooking into the active directory)
- Startup Composer
(Registers the ActiveDirectoryComponent and sets our CustomRenderMvcController)
- UmbracoADAuthExtensions
(Handles all the backend logic for hooking into the active directory)

~/Attributes

- ActiveDirectoryAuthorizeAttribute
(Forces the user to login to Active Directory)

~/Controllers

- CustomRenderMvcController
(Our custom base controller, that simply has the ActiveDirectoryAuthorizeAttribute)

# Changes to web.config

- Add the following keys so the Azure Active Directory can actually connect with the application
<add key="OpenId.AuthorizeUrl" value=""/>
<add key="AzureAd.TenantId" value=""/>
<add key="AzureAd.ClientId" value=""/>
<add key="AzureAd.RedirectUrl" value=""/>

- Set authentication mode to none (example below) so that when not authorized they don't return to a 404 page
<authentication mode="None">
  <!--<forms name="yourAuthCookie" loginUrl="login.aspx" protection="All" path="/" />-->
</authentication>