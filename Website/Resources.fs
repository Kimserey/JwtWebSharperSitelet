namespace Website

open WebSharper
open WebSharper.Resources

module Resources =
    
    type TestCss() =
        inherit BaseResource("test-embeddedres.css")

    type TestContentCss() =
        inherit BaseResource("css/test-content.css")

    type BootstrapCss() =
        inherit BaseResource("//maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css")

    [<assembly: System.Web.UI.WebResource("test-embeddedres.css", "html/css");
      assembly: Require(typeof<BootstrapCss>);
      assembly: Require(typeof<TestCss>)>]
    do()