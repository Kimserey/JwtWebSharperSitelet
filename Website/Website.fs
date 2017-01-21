module Website.Root

open System
open System.IO
open Microsoft.Owin
open WebSharper
open WebSharper.Web
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Server
open Common
open NLog
open Website

type MainTemplate = Templating.Template<"Main.html">

type Endpoint =
| [<EndPoint "GET /">] Home
| [<EndPoint "GET /register">] Register
| [<EndPoint "GET /logs">] Logs
| [<EndPoint "GET /logevents">] LogEvents

let sitelet = 
    Application.MultiPage(fun ctx endpoint -> 
        let logger = LogManager.GetCurrentClassLogger()

        match endpoint with
        | Home -> 
            logger.Trace "Home"
            Content.Page(MainTemplate.Doc("Home", [ client <@ Home.page() @> ]))

        | Register ->
            logger.Trace "Register"
            Content.Page(MainTemplate.Doc("Register", [ client <@ Register.Client.page() @> ]))

        | Logs ->
            logger.Trace "Logs"
            Content.Page(MainTemplate.Doc( "Logs", [ client <@ Logs.page() @> ]))

        | LogEvents ->
            Content.Custom(
                Status = Http.Status.Ok,
                Headers = 
                    [ 
                        Http.Header.Custom "Content-type" "text/event-stream" 
                        Http.Header.Custom "Cache-control" "no-cache" 
                        Http.Header.Custom "Connection" "keep-alive" 
                    ],
                WriteBody = 
                    (fun stream ->
                        let msgs = Logger.HttpLogAgent.PostAndReply(Logger.MessageLog.GetUnread)
                        use writer = new StreamWriter(stream)
                            
                        for msg in msgs do
                            writer.WriteLine("data:" + msg + "\n\n")
                    )
                )
    )