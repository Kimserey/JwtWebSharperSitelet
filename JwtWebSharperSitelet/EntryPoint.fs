namespace JwtWebSharperSitelet

open global.Owin
open Microsoft.Owin
open Microsoft.Owin.Security
open Microsoft.Owin.Hosting
open Microsoft.Owin.StaticFiles
open Microsoft.Owin.FileSystems
open WebSharper.Owin
open WebSharper.Web
open Common.Owin

module EntryPoint =

    [<EntryPoint>]
    let main args =
        let rootDirectory, url =
            match args with
            | [| rootDirectory; url |] -> rootDirectory, url
            | [| url |] -> "httproot", url
            | [| |] -> "httproot", "http://localhost:9000/"
            | _ -> eprintfn "Usage: ResourceTestWebSharper ROOT_DIRECTORY URL"; exit 1

        JsonP
        
        let startup (app: IAppBuilder) =
            let webSharperOptions = 
                WebSharperOptions<_>(
                    ServerRootDirectory = rootDirectory,
                    Sitelet = Some WebSite.sitelet,
                    BinDirectory = ".",
                    Debug = true
                )
                    
            let authenticator = 
                
            app.Use<JwtMiddleware>(new JwtMiddlewareOptions(authenticate, privateKey)) 
               .UseWebSharper(webSharperOptions)
               .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem(rootDirectory)))
               .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem("resources")))
            |> ignore

        use server = WebApp.Start(url, startup)

        stdout.WriteLine("Serving {0}", url)
        stdin.ReadLine() |> ignore
        0
