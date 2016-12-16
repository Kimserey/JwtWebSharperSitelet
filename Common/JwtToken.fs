namespace Common

open System
open System.Security
open System.Security.Claims
open Newtonsoft.Json

type JwtPayload =
    {
        [<JsonProperty "principal">]
        Principal: UserPrincipal
        [<JsonProperty "iss">]
        Issuer: string
        [<JsonProperty "sub">]
        Subject: string
        [<JsonProperty "exp">]
        Expiry: DateTime
        [<JsonProperty "iat">]
        IssuedAtTime: DateTime
        [<JsonProperty "jti">]
        Id: string
    }


// TODO authenticate and generate Principal then jsn payload
module JwtToken =

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