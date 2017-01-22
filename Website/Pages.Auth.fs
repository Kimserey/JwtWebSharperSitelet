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

type RegisterData = 
    {
        UserId: string
        Password: string
        Fullname: string
        Email: string
        Claims: string list
    }
    
type Credentials =
    { UserId: string
      Password: string }

module Auth =
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
                return true
            }
    
    [<JavaScript>]
    module Client =
        open WebSharper.UI.Next.Client
        open WebSharper.JavaScript
        open WebSharper.JQuery
        open AjaxHelper
        
        module Service =
            let login (cred: Credentials) =
                async {
                    let! result = 
                        httpRequest 
                            { AjaxOptions.POST 
                                with 
                                    Url = "token"
                                    DataType = JQuery.DataType.Text
                                    Data = Some <| box (JSON.Stringify cred) }
                    match result with
                    | AjaxResult.Success res ->
                        let token = string res
                        JS.Alert token
                    | AjaxResult.Error err ->
                        ()
                } |> Async.StartImmediate


        type Message =
            | Success of string
            | Failure of string
            | Empty
            with
                static member Embbed (x: Var<Message>) = 
                    x.View
                    |> Doc.BindView (
                        function 
                        | Success str -> divAttr [ attr.``class`` "alert alert-success" ] [ text str ] :> Doc
                        | Failure str -> divAttr [ attr.``class`` "alert alert-danger" ] [ text str ] :> Doc
                        | Empty -> Doc.Empty)

        let register() =
            let registerMessage = 
                Var.Create Empty

            let data = 
                Var.Create 
                    { UserId = ""
                      Password = ""
                      Fullname = ""
                      Email = ""
                      Claims = [] } 
            
            
            form
                [ h3 [ text "Register" ]
                  registerMessage |> Message.Embbed
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "UserId" ] (data.Lens (fun x -> x.UserId) (fun x n -> { x with UserId = n }))
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "Password"; attr.``type`` "password" ] (data.Lens (fun x -> x.Password) (fun x n -> { x with Password = n }))
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "Fullname" ] (data.Lens (fun x -> x.Fullname) (fun x n -> { x with Fullname = n }))
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "Email"; attr.``type`` "email" ] (data.Lens (fun x -> x.Email) (fun x n -> { x with Email = n }))
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "Claims comma separated" ] (data.Lens (fun x -> x.Claims |> String.concat ",") (fun x n -> { x with Claims = n.Split(',') |> Array.map (fun str -> str.Trim()) |> Array.toList })) 
                  Doc.Button "Create"
                    [ attr.``class`` "btn btn-primary"; attr.``type`` "submit" ] 
                    (fun () -> 
                        async {
                            let! result = Rpc.createAccount data.Value
                            if result then 
                                registerMessage.Value <- Success "Successfuly created user."
                            else
                                registerMessage.Value <- Failure "Failed to create user."
                        } |> Async.StartImmediate) :> Doc ]

        let login() =
            let cred = 
                Var.Create 
                    { UserId = ""
                      Password = "" }
                    
            form
                [ h3 [ text "Log in" ]
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "UserId" ] (cred.Lens (fun x -> x.UserId) (fun x n -> { x with UserId = n }))
                  Doc.Input [ attr.``class`` "form-control my-3"; attr.placeholder "Password"; attr.``type`` "password" ] (cred.Lens (fun x -> x.Password) (fun x n -> { x with Password = n }))
                  Doc.Button "Log In" [ attr.``class`` "btn btn-primary"; attr.style "submit" ] (fun () -> Service.login cred.Value) ]

        let page()=
            divAttr
                [ attr.``class`` "container" ]
                [ divAttr
                    [ attr.``class`` "card my-3" ]
                    [ divAttr
                        [ attr.``class`` "card-block" ]
                        [ login() ] ]
                        
                  divAttr
                    [ attr.``class`` "card my-3" ]
                    [ divAttr
                        [ attr.``class`` "card-block" ]
                        [ register() ] ] ]