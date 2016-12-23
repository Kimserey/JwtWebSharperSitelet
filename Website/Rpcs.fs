namespace Website

open System
open Microsoft.Owin
open WebSharper
open WebSharper.Web
open WebSharper.Sitelets
open Common
open NLog

module Rpcs =
    let logger = LogManager.GetCurrentClassLogger()

    [<Rpc>]
    let token() =
        logger.Trace("{0} - {1}", "RPC", "Request token")
        let ctx = Remoting.GetContext()
        let owinContext = unbox<OwinContext> <| ctx.Environment.["OwinContext"]
            
        async { 
            return ""
        }