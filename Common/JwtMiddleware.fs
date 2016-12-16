namespace Common.Owin

open Common
open System
open System.Security
open System.Security.Claims
open global.Owin
open Microsoft.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.FileSystems
open Newtonsoft.Json
open System.Threading.Tasks
open Microsoft.Owin.Security.Infrastructure

module JwtToken =
    // TODO authenticate and generate Principal then jsn payload

    let generate key (subject: string) (expiry: DateTime) =
        let payload = 
            {
                Issuer = "com.kimserey"
                Subject = subject
                Expiry = expiry
                IssuedAtTime = DateTime.UtcNow
                Id = Guid.NewGuid().ToString("N")
                Principal = Unchecked.defaultof<UserPrincipal>
            }
        Jose.JWT.Encode(JsonConvert.SerializeObject(payload), Convert.FromBase64String(key), Jose.JwsAlgorithm.HS256);

    let decode key token =
        JsonConvert.DeserializeObject<JwtPayload>(Jose.JWT.Decode(token, Convert.FromBase64String(key)))

type JwtMiddlewareOptions(privateKey) =
    inherit AuthenticationOptions("Bearer")

    member val PrivateKey = privateKey

type private JwtAuthenticationHandler() =
    inherit AuthenticationHandler<JwtMiddlewareOptions>()

    // The core authentication logic which must be provided by the handler. Will be invoked at most once per request. Do not call directly, call the wrapping Authenticate method instead.(Inherited from AuthenticationHandler.)
    override self.AuthenticateCoreAsync() =
        let prefix = "Bearer "

        match self.Context.Request.Headers.Get("Authorization") with
        | token when not (String.IsNullOrWhiteSpace(token)) && token.StartsWith(prefix) -> 
            let payload =
                token.Substring(prefix.Length)
                |> JwtToken.decode self.Options.PrivateKey
                
            if payload.Expiry > DateTime.UtcNow then
                Task.FromResult(null)
            else
                Task.FromResult(new AuthenticationTicket(new ClaimsIdentity(payload.Principal.Identity, payload.Principal.Claims |> List.map (fun claim -> Claim(ClaimTypes.Role, claim))), new AuthenticationProperties()))
        | _ -> 
            Task.FromResult(null)

    // Decides whether to invoke or not the middleware.
    // If true, stop further processing.
    // If false, pass through to next middleware.
    override self.InvokeAsync() =
        match self.Request.Path.ToString() with
        | "token" -> 
            // generate token
            Task.FromResult(true)
        | _ -> Task.FromResult(false)
            

type JwtMiddleware(next, options) =
    inherit AuthenticationMiddleware<JwtMiddlewareOptions>(next, options)

    override __.CreateHandler() =
        JwtAuthenticationHandler() :> AuthenticationHandler<JwtMiddlewareOptions>
