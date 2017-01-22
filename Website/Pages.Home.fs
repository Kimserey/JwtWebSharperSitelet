namespace Website

open System
open WebSharper
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
open WebSharper.JavaScript
open WebSharper.JQuery
open Common

type Credentials =
    { UserId: string
      Password: string }

[<JavaScript>]
module Home = 
    open AjaxHelper
    open Remoting

    let onClick (callback: string -> unit) =
        async {
            let! text = Rpcs.token()
            callback text
        } |> Async.StartImmediate

    let onAjaxClick (callback: string -> unit) =
        async {
            let! result = httpRequest { AjaxOptions.GET with Url = "something" }
            match result with
            | AjaxResult.Success res ->
                string res |> callback
            | AjaxResult.Error err ->
                ()     
        } |> Async.StartImmediate             

    let login (cred: Credentials) =
        async {
            let! result = httpRequest { AjaxOptions.POST with Url = "token"; Data = Some <| box (JSON.Stringify cred) }
            match result with
            | AjaxResult.Success res ->
                let token = string res
                JS.Alert token
            | AjaxResult.Error err ->
                ()
        } |> Async.StartImmediate

    let page() =
        Remoting.installBearer()

        let token = Var.Create ""
        let result = Var.Create ""

        let cred = Var.Create { UserId = ""; Password = "" }

        divAttr 
            [ attr.``class`` "container" ] 
            [ div [ Doc.TextView token.View ] 
              div [ Doc.TextView result.View ] 

              form
                [ Doc.Input [ attr.``class`` "form-control" ] (cred.Lens (fun x -> x.UserId) (fun x n -> { x with UserId = n }))
                  Doc.Input [ attr.``class`` "form-control" ] (cred.Lens (fun x -> x.Password) (fun x n -> { x with Password = n }))
                  Doc.Button "Log In" [ attr.``class`` "btn btn-primary"; attr.style "submit" ] (fun () -> login cred.Value) ] ]
//              div [ Doc.Button "Rpc" [] (fun () -> onClick (Var.Set token)) ]
//              div [ Doc.Button "Ajax call" [] (fun () -> onAjaxClick (Var.Set result)) ] ]