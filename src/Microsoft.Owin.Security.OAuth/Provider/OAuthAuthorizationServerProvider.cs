// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Default implementation of IOAuthAuthorizationServerProvider used by Authorization
    /// Server to communicate with the web application while processing requests. OAuthAuthorizationServerProvider provides some default behavior, 
    /// may be used as a virtual base class, and offers delegate properties which may be used to 
    /// handle individual calls without declaring a new class type.
    /// </summary>
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        /// <summary>
        /// Creates new instance of default provider behavior
        /// </summary>
        public OAuthAuthorizationServerProvider()
        {
            OnMatchEndpoint = context => Task.FromResult<object>(null);
            OnValidateClientRedirectUri = context => Task.FromResult<object>(null);
            OnValidateClientAuthentication = context => Task.FromResult<object>(null);

            OnValidateAuthorizeRequest = DefaultBehavior.ValidateAuthorizeRequest;
            OnValidateTokenRequest = DefaultBehavior.ValidateTokenRequest;

            OnGrantAuthorizationCode = DefaultBehavior.GrantAuthorizationCode;
            OnGrantResourceOwnerCredentials = context => Task.FromResult<object>(null);
            OnGrantRefreshToken = DefaultBehavior.GrantRefreshToken;
            OnGrantClientCredentials = context => Task.FromResult<object>(null);
            OnGrantCustomExtension = context => Task.FromResult<object>(null);

            OnAuthorizeEndpoint = context => Task.FromResult<object>(null);
            OnTokenEndpoint = context => Task.FromResult<object>(null);

            OnAuthorizationEndpointResponse = context => Task.FromResult<object>(null);

            OnTokenEndpointResponse = context => Task.FromResult<object>(null);
        }

        /// <summary>
        /// Called to determine if an incoming request is treated as an Authorize or Token
        /// endpoint. If Options.AuthorizeEndpointPath or Options.TokenEndpointPath
        /// are assigned values, then handling this event is optional and context.IsAuthorizeEndpoint and context.IsTokenEndpoint
        /// will already be true if the request path matches.
        /// </summary>
        public Func<OAuthMatchEndpointContext, Task> OnMatchEndpoint { get; set; }

        /// <summary>
        /// Called to validate that the context.ClientId is a registered "client_id", and that the context.RedirectUri a "redirect_uri" 
        /// registered for that client. This only occurs when processing the Authorize endpoint. The application MUST implement this
        /// call, and it MUST validate both of those factors before calling context.Validated. If the context.Validated method is called
        /// with a given redirectUri parameter, then IsValidated will only become true if the incoming redirect URI matches the given redirect URI. 
        /// If context.Validated is not called the request will not proceed further. 
        /// </summary>
        public Func<OAuthValidateClientRedirectUriContext, Task> OnValidateClientRedirectUri { get; set; }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
        /// present on the request. If the web application accepts Basic authentication credentials, 
        /// context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
        /// application accepts "client_id" and "client_secret" as form encoded POST parameters, 
        /// context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
        /// If context.Validated is not called the request will not proceed further. 
        /// </summary>
        public Func<OAuthValidateClientAuthenticationContext, Task> OnValidateClientAuthentication { get; set; }

        /// <summary>
        /// Called for each request to the Authorize endpoint to determine if the request is valid and should continue. 
        /// The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with 
        /// validated client redirect URI, should continue processing. An application may add any additional constraints.
        /// </summary>
        public Func<OAuthValidateAuthorizeRequestContext, Task> OnValidateAuthorizeRequest { get; set; }

        /// <summary>
        /// Called for each request to the Token endpoint to determine if the request is valid and should continue. 
        /// The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with 
        /// validated client credentials, should continue processing. An application may add any additional constraints.
        /// </summary>
        public Func<OAuthValidateTokenRequestContext, Task> OnValidateTokenRequest { get; set; }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "authorization_code". This occurs after the Authorize
        /// endpoint as redirected the user-agent back to the client with a "code" parameter, and the client is exchanging that for an "access_token".
        /// The claims and properties 
        /// associated with the authorization code are present in the context.Ticket. The application must call context.Validated to instruct the Authorization
        /// Server middleware to issue an access token based on those claims and properties. The call to context.Validated may be given a different
        /// AuthenticationTicket or ClaimsIdentity in order to control which information flows from authorization code to access token.
        /// The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the authorization code to 
        /// the access token unmodified.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.1.3
        /// </summary>
        public Func<OAuthGrantAuthorizationCodeContext, Task> OnGrantAuthorizationCode { get; set; }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "password". This occurs when the user has provided name and password
        /// credentials directly into the client application's user interface, and the client application is using those to acquire an "access_token" and 
        /// optional "refresh_token". If the web application supports the
        /// resource owner credentials grant type it must validate the context.Username and context.Password as appropriate. To issue an
        /// access token the context.Validated must be called with a new ticket containing the claims about the resource owner which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.3.2
        /// </summary>
        public Func<OAuthGrantResourceOwnerCredentialsContext, Task> OnGrantResourceOwnerCredentials { get; set; }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "client_credentials". This occurs when a registered client
        /// application wishes to acquire an "access_token" to interact with protected resources on it's own behalf, rather than on behalf of an authenticated user. 
        /// If the web application supports the client credentials it may assume the context.ClientId has been validated by the ValidateClientAuthentication call.
        /// To issue an access token the context.Validated must be called with a new ticket containing the claims about the client application which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.4.2
        /// </summary>
        public Func<OAuthGrantClientCredentialsContext, Task> OnGrantClientCredentials { get; set; }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token". This occurs if your application has issued a "refresh_token" 
        /// along with the "access_token", and the client is attempting to use the "refresh_token" to acquire a new "access_token", and possibly a new "refresh_token".
        /// To issue a refresh token the an Options.RefreshTokenProvider must be assigned to create the value which is returned. The claims and properties 
        /// associated with the refresh token are present in the context.Ticket. The application must call context.Validated to instruct the 
        /// Authorization Server middleware to issue an access token based on those claims and properties. The call to context.Validated may 
        /// be given a different AuthenticationTicket or ClaimsIdentity in order to control which information flows from the refresh token to 
        /// the access token. The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the refresh token to 
        /// the access token unmodified.
        /// See also http://tools.ietf.org/html/rfc6749#section-6
        /// </summary>
        public Func<OAuthGrantRefreshTokenContext, Task> OnGrantRefreshToken { get; set; }

        /// <summary>
        /// Called when a request to the Token andpoint arrives with a "grant_type" of any other value. If the application supports custom grant types
        /// it is entirely responsible for determining if the request should result in an access_token. If context.Validated is called with ticket
        /// information the response body is produced in the same way as the other standard grant types. If additional response parameters must be
        /// included they may be added in the final TokenEndpoint call.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.5
        /// </summary>
        public Func<OAuthGrantCustomExtensionContext, Task> OnGrantCustomExtension { get; set; }

        /// <summary>
        /// Called at the final stage of an incoming Authorize endpoint request before the execution continues on to the web application component 
        /// responsible for producing the html response. Anything present in the OWIN pipeline following the Authorization Server may produce the
        /// response for the Authorize page. If running on IIS any ASP.NET technology running on the server may produce the response for the 
        /// Authorize page. If the web application wishes to produce the response directly in the AuthorizeEndpoint call it may write to the 
        /// context.Response directly and should call context.RequestCompleted to stop other handlers from executing. If the web application wishes
        /// to grant the authorization directly in the AuthorizeEndpoint call it cay call context.OwinContext.Authentication.SignIn with the
        /// appropriate ClaimsIdentity and should call context.RequestCompleted to stop other handlers from executing.
        /// </summary>
        public Func<OAuthAuthorizeEndpointContext, Task> OnAuthorizeEndpoint { get; set; }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application may implement this call in order to do any final 
        /// modification of the claims being used to issue access or refresh tokens. This call may also be used in order to add additional 
        /// response parameters to the Token endpoint's json response body.
        /// </summary>
        public Func<OAuthTokenEndpointContext, Task> OnTokenEndpoint { get; set; }

        /// <summary>
        /// Called before the AuthorizationEndpoint redirects its response to the caller. The response could be the
        /// token, when using implicit flow or the AuthorizationEndpoint when using authorization code flow.  
        /// An application may implement this call in order to do any final modification of the claims being used 
        /// to issue access or refresh tokens. This call may also be used in order to add additional 
        /// response parameters to the authorization endpoint's response.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public Func<OAuthAuthorizationEndpointResponseContext, Task> OnAuthorizationEndpointResponse { get; set; }

        /// <summary>
        /// Called before the TokenEndpoint redirects its response to the caller. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Func<OAuthTokenEndpointResponseContext, Task> OnTokenEndpointResponse { get; set; }

        /// <summary>
        /// Called to determine if an incoming request is treated as an Authorize or Token
        /// endpoint. If Options.AuthorizeEndpointPath or Options.TokenEndpointPath
        /// are assigned values, then handling this event is optional and context.IsAuthorizeEndpoint and context.IsTokenEndpoint
        /// will already be true if the request path matches.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            return OnMatchEndpoint.Invoke(context);
        }

        /// <summary>
        /// Called to validate that the context.ClientId is a registered "client_id", and that the context.RedirectUri a "redirect_uri" 
        /// registered for that client. This only occurs when processing the Authorize endpoint. The application MUST implement this
        /// call, and it MUST validate both of those factors before calling context.Validated. If the context.Validated method is called
        /// with a given redirectUri parameter, then IsValidated will only become true if the incoming redirect URI matches the given redirect URI. 
        /// If context.Validated is not called the request will not proceed further. 
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            return OnValidateClientRedirectUri.Invoke(context);
        }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
        /// present on the request. If the web application accepts Basic authentication credentials, 
        /// context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
        /// application accepts "client_id" and "client_secret" as form encoded POST parameters, 
        /// context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
        /// If context.Validated is not called the request will not proceed further. 
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            return OnValidateClientAuthentication.Invoke(context);
        }

        /// <summary>
        /// Called for each request to the Authorize endpoint to determine if the request is valid and should continue. 
        /// The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with 
        /// validated client redirect URI, should continue processing. An application may add any additional constraints.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context)
        {
            return OnValidateAuthorizeRequest.Invoke(context);
        }

        /// <summary>
        /// Called for each request to the Token endpoint to determine if the request is valid and should continue. 
        /// The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with 
        /// validated client credentials, should continue processing. An application may add any additional constraints.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            return OnValidateTokenRequest.Invoke(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "authorization_code". This occurs after the Authorize
        /// endpoint as redirected the user-agent back to the client with a "code" parameter, and the client is exchanging that for an "access_token".
        /// The claims and properties 
        /// associated with the authorization code are present in the context.Ticket. The application must call context.Validated to instruct the Authorization
        /// Server middleware to issue an access token based on those claims and properties. The call to context.Validated may be given a different
        /// AuthenticationTicket or ClaimsIdentity in order to control which information flows from authorization code to access token.
        /// The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the authorization code to 
        /// the access token unmodified.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.1.3
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context)
        {
            return OnGrantAuthorizationCode.Invoke(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token". This occurs if your application has issued a "refresh_token" 
        /// along with the "access_token", and the client is attempting to use the "refresh_token" to acquire a new "access_token", and possibly a new "refresh_token".
        /// To issue a refresh token the an Options.RefreshTokenProvider must be assigned to create the value which is returned. The claims and properties 
        /// associated with the refresh token are present in the context.Ticket. The application must call context.Validated to instruct the 
        /// Authorization Server middleware to issue an access token based on those claims and properties. The call to context.Validated may 
        /// be given a different AuthenticationTicket or ClaimsIdentity in order to control which information flows from the refresh token to 
        /// the access token. The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the refresh token to 
        /// the access token unmodified.
        /// See also http://tools.ietf.org/html/rfc6749#section-6
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            return OnGrantRefreshToken.Invoke(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "password". This occurs when the user has provided name and password
        /// credentials directly into the client application's user interface, and the client application is using those to acquire an "access_token" and 
        /// optional "refresh_token". If the web application supports the
        /// resource owner credentials grant type it must validate the context.Username and context.Password as appropriate. To issue an
        /// access token the context.Validated must be called with a new ticket containing the claims about the resource owner which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.3.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return OnGrantResourceOwnerCredentials.Invoke(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "client_credentials". This occurs when a registered client
        /// application wishes to acquire an "access_token" to interact with protected resources on it's own behalf, rather than on behalf of an authenticated user. 
        /// If the web application supports the client credentials it may assume the context.ClientId has been validated by the ValidateClientAuthentication call.
        /// To issue an access token the context.Validated must be called with a new ticket containing the claims about the client application which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.4.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            return OnGrantClientCredentials.Invoke(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of any other value. If the application supports custom grant types
        /// it is entirely responsible for determining if the request should result in an access_token. If context.Validated is called with ticket
        /// information the response body is produced in the same way as the other standard grant types. If additional response parameters must be
        /// included they may be added in the final TokenEndpoint call.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.5
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            return OnGrantCustomExtension.Invoke(context);
        }

        /// <summary>
        /// Called at the final stage of an incoming Authorize endpoint request before the execution continues on to the web application component 
        /// responsible for producing the html response. Anything present in the OWIN pipeline following the Authorization Server may produce the
        /// response for the Authorize page. If running on IIS any ASP.NET technology running on the server may produce the response for the 
        /// Authorize page. If the web application wishes to produce the response directly in the AuthorizeEndpoint call it may write to the 
        /// context.Response directly and should call context.RequestCompleted to stop other handlers from executing. If the web application wishes
        /// to grant the authorization directly in the AuthorizeEndpoint call it cay call context.OwinContext.Authentication.SignIn with the
        /// appropriate ClaimsIdentity and should call context.RequestCompleted to stop other handlers from executing.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return OnAuthorizeEndpoint.Invoke(context);
        }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application may implement this call in order to do any final 
        /// modification of the claims being used to issue access or refresh tokens. This call may also be used in order to add additional 
        /// response parameters to the Token endpoint's json response body.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            return OnTokenEndpoint.Invoke(context);
        }

        /// <summary>
        /// Called before the AuthorizationEndpoint redirects its response to the caller. The response could be the
        /// token, when using implicit flow or the AuthorizationEndpoint when using authorization code flow.  
        /// An application may implement this call in order to do any final modification of the claims being used 
        /// to issue access or refresh tokens. This call may also be used in order to add additional 
        /// response parameters to the authorization endpoint's response.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public virtual Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            return OnAuthorizationEndpointResponse.Invoke(context);
        }

        /// <summary>
        /// Called before the TokenEndpoint redirects its response to the caller. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            return OnTokenEndpointResponse.Invoke(context);
        }
    }
}
