namespace Website

open System
open Microsoft.Owin
open WebSharper
open WebSharper.Web
open WebSharper.Sitelets
open WebSharper.UI.Next
open WebSharper.UI.Next.Html
open WebSharper.UI.Next.Server
open Common
open NLog

type MainTemplate = Templating.Template<"Main.html">

module WebSite =
    open System
    open System.Collections
    open System.Collections.Generic
    open System.Security.Cryptography
    
    let logger = LogManager.GetCurrentClassLogger()

    module private Rpcs =

        [<Rpc>]
        let token() =
            logger.Trace("{0} - {1}", "RPC", "Request token")
            let ctx = Remoting.GetContext()
            let owinContext = unbox<OwinContext> <| ctx.Environment.["OwinContext"]
            
            async { 
                return ""
            }
    

    [<JavaScript>]
    module private Client = 
        open WebSharper.JavaScript
        open WebSharper.JQuery
        open WebSharper.UI.Next.Client
            
        module private Remoting =
            open WebSharper.JavaScript

            let private originalProvider = WebSharper.Remoting.AjaxProvider

            type CustomXhrProvider () =
                member this.AddHeaders(headers) =
                    JS.Set headers "Authorization" "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJjb20ua2ltc2VyZXkiLCJzdWIiOiJzb21lX3VzZXIiLCJleHAiOiIyMDE2LTEyLTE0VDIzOjUzOjQxLjA0NTA3OThaIiwiaWF0IjoiMjAxNi0xMi0xNFQyMzo1MTo0MS4wNDUwNzk4WiIsImp0aSI6IjJmMDhlZTAxMDgwNjQ5NzdhMjE5Yjk0NmNiNWU0YWJkIn0.UsRwXaqKrnIENQFw6jp78P79u1R-caGg-NnldvoK0I0"
                    headers

                interface WebSharper.Remoting.IAjaxProvider with
                    member this.Async url headers data ok err =
                        originalProvider.Async url (this.AddHeaders headers) data ok err
                    member this.Sync url headers data =
                        originalProvider.Sync url (this.AddHeaders headers) data
            
            let installBearer() =
                WebSharper.Remoting.AjaxProvider <- CustomXhrProvider()

        module private AjaxHelper =

            type AjaxResult =
            | Success of result: obj
            | Error of errorMessage: string

            type AjaxOptions = {
                Url:         string
                RequestType: RequestType
                Headers:     (string * string) [] option
                Data:        obj option
                ContentType: string option
            } 
            with 
                static member GET =
                    { RequestType = RequestType.GET;   Url = ""; Headers = None; Data = None; ContentType = Some "application/json" }
        
                static member POST =
                    { AjaxOptions.GET with RequestType = RequestType.POST }
    
            let httpRequest options =
                async {
                    try
                        let! result = 
                            Async.FromContinuations
                            <| fun (ok, ko, _) ->
                                let settings = JQuery.AjaxSettings(
                                                Url = options.Url,
                                                Type = options.RequestType,
                                                DataType = JQuery.DataType.Json,
                                                Success = (fun (result, _, _) ->
                                                            ok result),
                                                Error = (fun (jqXHR, _, _) ->
                                                            ko (System.Exception(string jqXHR.Status)))
                                                )
                                options.ContentType |> Option.iter (fun c -> settings.ContentType <- c)
                                options.Headers     |> Option.iter (fun h -> settings.Headers <- (new Object<string>(h)))
                                options.Data        |> Option.iter (fun d -> settings.Data <- d)
                                JQuery.Ajax(settings) |> ignore
                        return AjaxResult.Success result
                    with ex -> 
                        Console.Log <| ex.JS.ToString()
                        return AjaxResult.Error ex.Message
                }
        
        open AjaxHelper

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

        let getToken () =
            async {
                let! result = httpRequest { AjaxOptions.POST with Url = "token"; Data = Some <| box (JSON.Stringify (New [ "UserId" => "admin"; "Password" => "helloworld" ])) }
                match result with
                | AjaxResult.Success res ->
                    let token = string res
                    JS.Alert token
                | AjaxResult.Error err ->
                    ()
            } |> Async.StartImmediate

        [<Require(typeof<Resources.TestContentCss>)>]
        let page() =
            Remoting.installBearer()

            let token = Var.Create ""
            let result = Var.Create ""

            divAttr 
                [ attr.``class`` "box" ] 
                [ text "Hello world"
                  div [ Doc.TextView token.View ] 
                  div [ Doc.TextView result.View ] 
                  div [ Doc.Button "Rpc" [] (fun () -> onClick (Var.Set token)) ]
                  div [ Doc.Button "Ajax call" [] (fun () -> onAjaxClick (Var.Set result)) ]
                  div [ Doc.Button "Request token" [] (fun () -> getToken()) ] ]

    let sitelet = 
        Application.MultiPage(fun ctx endpoint -> 
            match endpoint with
            | "something" -> 
                logger.Trace("{0} - {1}", "Endpoint", "something")
                Content.Json "hello world"
            
            | _ -> 
                logger.Trace("{0} - {1}", "Endpoint", "default")
                Content.Page(
                    MainTemplate.Doc(
                        "Test", 
                        [ 
                            client <@ Client.page() @> 
                        ]
                    )
                )
            )