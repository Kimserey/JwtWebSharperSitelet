namespace Website

open WebSharper
open WebSharper.Resources

module Resources =
    
    type BootstrapCss() =
        inherit BaseResource("//maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css")

    [<assembly: Require(typeof<BootstrapCss>)>]
    do()