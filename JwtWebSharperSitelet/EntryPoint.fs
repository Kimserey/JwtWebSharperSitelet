module JwtWebSharperSitelet.EntryPoint

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
open Storage

type Configurations = JsonProvider<"configs.json">

[<EntryPoint>]
let main args =

    // gets the core configurations from configs.json
    let coreCfg = Configurations.GetSample()

    // website startup
    let startup (app: IAppBuilder) = 
        try
            Logger.instance.Trace("Startup starts.")

            let webSharperOptions = 
                WebSharperOptions<_>(
                    ServerRootDirectory = coreCfg.Sitelet.RootDir,
                    Sitelet = Some WebSite.sitelet,
                    BinDirectory = coreCfg.Sitelet.BinDir,
                    Debug = true
                )
    
            let authenticator = new Authenticator(Path.Combine(coreCfg.Sitelet.RootDir, coreCfg.Sitelet.DataDir))

            app.Use<JwtMiddleware>(new JwtMiddlewareOptions(authenticator.Authenticate, coreCfg.Jwt.PrivateKey, float coreCfg.Jwt.TokenLifeSpanInMinutes)) 
                .UseWebSharper(webSharperOptions)
                .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem(coreCfg.Sitelet.RootDir)))
                .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem("resources")))
            |> ignore

            Logger.instance.Trace("Startup completed.")
        with
        | ex -> 
            Logger.instance.Fatal("Startup failed with unexpected error. {0}", ex.Message)

    use server = WebApp.Start(coreCfg.Sitelet.Url, startup)
    Logger.instance.Trace("Serving {0}", coreCfg.Sitelet.Url)
    stdin.ReadLine() |> ignore
    0
