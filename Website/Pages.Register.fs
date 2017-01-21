namespace Website

open System
open System.IO
open WebSharper
open WebSharper.Web
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.Pervasives
open Common
open Storage

[<JavaScript>]
type RegisterData = 
    {
        UserId: string
        Password: string
        Fullname: string
        Email: string
        Claims: string list
    }


module Register =
    module Rpc =
        [<Rpc>]
        let createAccount (data: RegisterData) =
            let ctx = WebSharper.Web.Remoting.GetContext()
            async {
                let userRegistry = UserRegistry.api (Path.Combine("data", "user_accounts.db"))
                userRegistry.Create 
                    (UserId data.UserId) 
                    (Password data.Password)
                    data.Fullname
                    data.Email
                    data.Claims
            }
    
    [<JavaScript>]
    module Client =
        open WebSharper.UI.Next.Client
        open WebSharper.JavaScript
        open WebSharper.JQuery

        let page() =
            let data = Var.Create <| { UserId = ""; Password = ""; Fullname = ""; Email = ""; Claims = [] } 
            form
                [ pre [ data.View |> Doc.BindView (fun data -> text <| sprintf "%A" data) ]
                  Doc.Input [ attr.``class`` "form-control" ] (data.Lens (fun x -> x.UserId) (fun x n -> { x with UserId = n }))
                  Doc.Input [ attr.``class`` "form-control" ] (data.Lens (fun x -> x.Password) (fun x n -> { x with Password = n }))
                  Doc.Input [ attr.``class`` "form-control" ] (data.Lens (fun x -> x.Fullname) (fun x n -> { x with Fullname = n }))
                  Doc.Input [ attr.``class`` "form-control" ] (data.Lens (fun x -> x.Email) (fun x n -> { x with Email = n }))
                  Doc.Input [ attr.``class`` "form-control" ] (data.Lens (fun x -> x.Claims |> String.concat ",") (fun x n -> { x with Claims = n.Split(',') |> Array.map (fun str -> str.Trim()) |> Array.toList })) 
                  Doc.Button "Create"
                    [ attr.``class`` "btn btn-primary"; attr.``type`` "submit" ] 
                    (fun () -> Rpc.createAccount data.Value |> Async.StartImmediate) :> Doc ]