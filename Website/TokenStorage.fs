namespace Website

open System
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module JwtHelper =

    type JwtUserIdentity = {
        Name: string
        IsAuthenticated: bool
        AuthenticationType: string
    } 

    type JwtUserPrincipal = {
        Identity: JwtUserIdentity
        Claims: string list
    }

    type JwtPayload = {
        principal : JwtUserPrincipal
        iss: string
        sub: string
        iat: int
        exp : int
        jti : string
    } with
        static member GetTimeSpent stamp x =
            Math.Round ((DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalSeconds) - stamp
        
        static member GetTimeRemaining stamp x = 
            x.exp - x.iat - JwtPayload.GetTimeSpent stamp x
    
    [<Direct """
        var decodeBase64 = function (s) {
            var e = {}, i, b = 0, c, x, l = 0, a, r = '', w = String.fromCharCode, L = s.length;
            var A = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            for (i = 0; i < 64; i++) { e[A.charAt(i)] = i; }
            for (x = 0; x < L; x++) {
                c = e[s.charAt(x)]; b = (b << 6) + c; l += 6;
                while (l >= 8) { ((a = (b >>> (l -= 8)) & 0xff) || (x < (L - 2))) && (r += w(a)); }
            }
            return r;
        };

        var segments = $token.split('.');
        if (segments.length !== 3) throw new Error('Not enough or too many segments');
        var payloadSeg = segments[1];
        var decodedPayloadSeg = decodeBase64(payloadSeg);

        return JSON.parse(decodedPayloadSeg);
    """>]
    let getPayload token = X<JwtPayload>

[<JavaScript>]
module TokenStorage =
    
    let private authTokenKey = "access_token"
    let private storage = JS.Window.LocalStorage

    let get() = 
        match storage.GetItem authTokenKey with
        | null -> None
        | token -> Some <| JwtHelper.getPayload token

    let set (token: string) = 
        storage.SetItem(authTokenKey, token)

    let clear() =
        storage.RemoveItem(authTokenKey)
