namespace Website

open System
open WebSharper
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
open WebSharper.JavaScript
open WebSharper.JQuery
open Common

type Route =
    | Auth
    | Home

[<JavaScript>]
module Index = 
    open Remoting

    let page() =
        
        RouteMap.Create 
            (
                function 
                | Auth -> [ "auth" ] 
                | Home -> [ "home" ]
            ) 
            (
                function 
                | [ "auth" ] -> Auth 
                | [ "home" ] -> Home | _ -> Home
            )
        |> RouteMap.Install
        |> View.FromVar
        |> Doc.BindView (
            function
            | Auth -> Website.Auth.Client.page() 
            | Home -> Website.Home.Client.page()
        )