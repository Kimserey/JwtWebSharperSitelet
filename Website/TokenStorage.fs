namespace Website

open System
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module TokenStorage =
    
    let private authTokenKey = "access_token"
    let private storage = JS.Window.LocalStorage

    let get() = 
        match storage.GetItem authTokenKey with
        | null -> None
        | token -> Some <| token

    let set (token: string) = 
        storage.SetItem(authTokenKey, token)

    let clear() =
        storage.RemoveItem(authTokenKey)
