namespace Common.Owin

open Common
open System
open System.Security
open System.Security.Claims
open System.IO
open global.Owin
open Microsoft.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.FileSystems
open Newtonsoft.Json
open System.Threading.Tasks
open Microsoft.Owin.Security.Infrastructure

module JwtToken =

    // Server dictates the algorithm used for encode/decode to prevent vulnerability
    // https://auth0.com/blog/critical-vulnerabilities-in-json-web-token-libraries/
    let algorithm = Jose.JwsAlgorithm.HS256

    let generate key (principal: UserPrincipal) (expiry: DateTime) =
        let payload = 
            {
                Id = Guid.NewGuid().ToString("N")
                Issuer = "com.kimserey"
                Subject = principal.Identity.Name
                Expiry = expiry
                IssuedAtTime = DateTime.UtcNow
                Principal = principal
            }
        Jose.JWT.Encode(JsonConvert.SerializeObject(payload), Convert.FromBase64String(key), algorithm)

    let decode key token =
        JsonConvert.DeserializeObject<JwtPayload>(Jose.JWT.Decode(token, Convert.FromBase64String(key), algorithm))

type JwtMiddlewareOptions(authenticate, privateKey, tokenLifeSpanInMinutes) =
    inherit AuthenticationOptions("Bearer")

    member val Authenticate = authenticate
    member val PrivateKey = privateKey
    member val TokenLifeSpanInMinutes = tokenLifeSpanInMinutes

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
                try 
                    new AuthenticationTicket(
                        new ClaimsIdentity(
                            payload.Principal.Identity, 
                            payload.Principal.Claims 
                            |> List.map (fun claim -> Claim(ClaimTypes.Role, claim))), 
                        new AuthenticationProperties()
                    )
                    |> Task.FromResult
                with
                | ex ->
                    
                    Task.FromResult(null)
        | _ -> 
            Task.FromResult(null)

    // Decides whether to invoke or not the middleware.
    // If true, stop further processing.
    // If false, pass through to next middleware.
    override self.InvokeAsync() =
        if self.Request.Path.HasValue && self.Request.Path.Value = "/token" then
            if self.Request.ContentType = "application/json" then 
                use streamReader = new StreamReader(self.Request.Body)
                let cred = JsonConvert.DeserializeObject<Credentials>(streamReader.ReadToEnd())
                match self.Options.Authenticate cred with
                | AuthenticateResult.Success userAccount ->
                    let (UserId name) = userAccount.Id
                    let principal =
                        {
                            Identity = 
                                {
                                    Name = name
                                    IsAuthenticated = true
                                    AuthenticationType = self.Options.AuthenticationType
                                }
                            Claims = userAccount.Claims
                        }

                    let token = JwtToken.generate self.Options.PrivateKey principal  (DateTime.UtcNow.AddMinutes(self.Options.TokenLifeSpanInMinutes))
                    use writer = new StreamWriter(self.Response.Body)
                    self.Response.StatusCode <- 200
                    writer.WriteLine(token)
                    Task.FromResult(true)
                | AuthenticateResult.Failure ->
                    self.Response.StatusCode <- 401
                    Task.FromResult(true)
            else
                self.Response.StatusCode <- 401
                Task.FromResult(true)
        
        else
            Task.FromResult(false)
            

type JwtMiddleware(next, options) =
    inherit AuthenticationMiddleware<JwtMiddlewareOptions>(next, options)

    override __.CreateHandler() =
        JwtAuthenticationHandler() :> AuthenticationHandler<JwtMiddlewareOptions>
