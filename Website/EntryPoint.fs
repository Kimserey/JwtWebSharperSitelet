﻿module Website.EntryPoint

open global.Owin
open Microsoft.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Hosting
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.FileSystems
open WebSharper.Owin
open WebSharper.Web
open Common.Owin
open System.IO
open FSharp.Data
open Website
open Storage
open NLog

type Configurations = JsonProvider<"configs.json">

[<EntryPoint>]
let main args =
    
    Logger.SqliteLogTarget.RegisterTarget()
    Logger.HttpSSELogTarget.RegisterTarget()
    let logger = LogManager.GetCurrentClassLogger()

    // gets the core configurations from configs.json
    let coreCfg = Configurations.GetSample()

    // website startup
    let startup (app: IAppBuilder) = 
        try
            logger.Trace("Startup starts.")

            let webSharperOptions = 
                WebSharperOptions<_>(
                    ServerRootDirectory = coreCfg.Sitelet.RootDir,
                    Sitelet = Some Root.sitelet,
                    BinDirectory = coreCfg.Sitelet.BinDir,
                    Debug = true
                )
    
            let authenticator = new Authenticator(coreCfg.Sitelet.DataDir)

            app.Use<JwtMiddleware>(new JwtMiddlewareOptions(authenticator.Authenticate, coreCfg.Jwt.PrivateKey, float coreCfg.Jwt.TokenLifeSpanInMinutes)) 
                .UseWebSharper(webSharperOptions)
                .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem(coreCfg.Sitelet.RootDir)))
                .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem("resources")))
            |> ignore

            logger.Trace("Startup completed.")
        with
        | ex -> 
            logger.Fatal("Startup failed with unexpected error. {0}", ex.Message)

    use server = WebApp.Start(coreCfg.Sitelet.Url, startup)
    logger.Trace("Serving {0}", coreCfg.Sitelet.Url)
    stdin.ReadLine() |> ignore
    0
