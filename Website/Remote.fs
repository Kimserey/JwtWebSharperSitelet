namespace Website

open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery

[<JavaScript>]
module Remoting =
    open WebSharper.JavaScript

    let private originalProvider = WebSharper.Remoting.AjaxProvider

    let getToken() =
        Some ""

    type CustomXhrProvider () =
        member this.AddHeaders(headers) =
            getToken()
            |> Option.iter (fun token ->
                JS.Set headers "Authorization" <| sprintf "Bearer %s" token
            )

            headers

        interface WebSharper.Remoting.IAjaxProvider with
            member this.Async url headers data ok err =
                originalProvider.Async url (this.AddHeaders headers) data ok err
            member this.Sync url headers data =
                originalProvider.Sync url (this.AddHeaders headers) data
            
    let installBearer() =
        WebSharper.Remoting.AjaxProvider <- CustomXhrProvider()

[<JavaScript>]
module AjaxHelper =

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
        