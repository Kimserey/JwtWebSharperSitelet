namespace Website

open System
open WebSharper
open WebSharper.Web
open WebSharper.UI.Next
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.Pervasives
open Common

[<JavaScript>]
type EventSource() =
    [<Inline "new EventSource($source)">]
    new(source: string) = new EventSource()

    [<Inline "$0.addEventListener('message', function(e) { $callback(e.data); }, false)">]
    member self.AddEventListener(callback: string -> unit) = X<unit>

[<JavaScript>]
module Logs =

    let page() =
        let es = new EventSource("logevents")
            
        divAttr 
            [ attr.id "logs" 
              on.afterRender(fun el -> es.AddEventListener(fun logMsg -> el.AppendChild((div [ text logMsg ]).Dom) |> ignore)) ] 
            []