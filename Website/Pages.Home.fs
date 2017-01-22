namespace Website

open System
open WebSharper
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
open WebSharper.JavaScript
open WebSharper.JQuery
open Common

module Home = 
    
    [<JavaScript>]
    module Client =

        let page (navigator: PageNavigator) =
            
            let message =
                Var.Create ""

            match TokenStorage.get() with
            | Some payload ->
                message.Value <- payload.principal.Identity.Name
            | None -> 
                TokenStorage.clear()
                navigator.GoAuth()

            divAttr
                [ attr.``class`` "container my-3" ]
                [ h1Attr [ attr.``class`` "display-3" ]  [ text "Welcome!" ]
                  pAttr [ attr.``class`` "lead" ] [ message.View |> Doc.BindView (sprintf "You successfuly logged in as %s" >> text) ]
                  hrAttr [ attr.``class`` "my-4" ] []
                  pAttr 
                    [ attr.``class`` "lead" ]
                    [ Doc.Button 
                        "Log out" 
                        [ attr.``class`` "btn btn-primary" ] 
                        (fun () -> 
                            TokenStorage.clear()
                            navigator.GoAuth()) ] ]