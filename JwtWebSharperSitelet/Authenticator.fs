namespace JwtWebSharperSitelet

open Common
open Storage 
open System.IO

type Authenticator(dataDir) =
    let userAccountApi = 
        UserRegistry.api (Path.Combine(dataDir, "user_accounts.db"))
    
    member self.Authenticate (credentials: Credentials) =
        match userAccountApi.Get (UserId credentials.UserId) with
        | Some userAccount ->
            let (Password pwd) = userAccount.Password
            Cryptography.verify pwd credentials.Password
        | None ->
            false