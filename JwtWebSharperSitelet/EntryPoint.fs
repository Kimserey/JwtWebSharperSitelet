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

        // Better not to be stored in code 256 bytes encoded in base64
        let privateKey = 
            "Lu0YxSQm4UNb+MG1hZA1xhMJaaenYVMVm8U1I4N7Hm7BdkATU05XZz02y1bAvrrf/6ie1ZRo/6Mb1Oqxg0QJs2QgCBpClD/xup/2AZ3mBetJ0YxDIozYsalGiifNpAKNAayOUz+VgEYgQBh8lOsdiA9mTvr1g0VeNlUAktcTdzb9SqleZnnVZKA8BPWff/gcSXtbFtxEnJM5YJJPayEMyDf1HHfxUmC/0Zu6aQIorxe8puwfYmXgNhzJdAgNT65exM3XBKNzBAv/GcOvw0xIkhqkcoBBYq7Avd/vXjVSIaFmF5tyyRNrkTGkEJ34pl1qWgeU9tHrxNQ1rdmutDSLZg=="

        let startup (app: IAppBuilder) =
            let webSharperOptions = 
                WebSharperOptions<_>(
                    ServerRootDirectory = rootDirectory,
                    Sitelet = Some WebSite.sitelet,
                    BinDirectory = ".",
                    Debug = true
                )
                
            app.Use<JwtMiddleware>(new JwtMiddlewareOptions(privateKey)) 
               .UseWebSharper(webSharperOptions)
               .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem(rootDirectory)))
               .UseStaticFiles(StaticFileOptions(FileSystem = PhysicalFileSystem("resources")))
            |> ignore

        use server = WebApp.Start(url, startup)

        stdout.WriteLine("Serving {0}", url)
        stdin.ReadLine() |> ignore
        0
