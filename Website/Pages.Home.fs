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

        let page() =
        
            divAttr 
                [ attr.``class`` "container" ] 
                [ text  "Welcome! You successfuly logged in" 
                  Doc.Button "Log out" [ attr.``class`` "btn btn-primary" ] (fun () -> ()) ]
