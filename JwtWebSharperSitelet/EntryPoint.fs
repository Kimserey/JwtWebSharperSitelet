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
open NLog
open NLog.Config
open NLog.Targets
open Storage

type SqliteLogTarget() =
    inherit TargetWithLayout()

    let mutable database = ""

    [<RequiredParameter>]
    member self.Database 
        with get () = database
        and set value = 
            database <- value

    override self.Write(logEvent: LogEventInfo) =
        let message = self.Layout.Render logEvent
        LogRegistry.log self.Database logEvent.TimeStamp logEvent.Level.Name logEvent.LoggerName logEvent.Message

type Configurations = JsonProvider<"configs.json">

[<EntryPoint>]
let main args =
    
    // gets the core configurations from configs.json
    let coreCfg = Configurations.GetSample()

    // register logger sqlite
    ConfigurationItemFactory.Default.Targets.RegisterDefinition("SqliteLog", typeof<SqliteLogTarget>)

    // website startup
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
