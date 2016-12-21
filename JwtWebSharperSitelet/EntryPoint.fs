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

type Configurations = JsonProvider<"configs.json">

[<EntryPoint>]
let main args =
    
    let coreCfg = Configurations.GetSample()

    let startup (app: IAppBuilder) = 
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

    use server = WebApp.Start(coreCfg.Sitelet.Url, startup)
    stdout.WriteLine("Serving {0}", coreCfg.Sitelet.Url)
    stdin.ReadLine() |> ignore
    0
