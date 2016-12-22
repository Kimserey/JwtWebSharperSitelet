namespace Website

open WebSharper
open WebSharper.Resources

module Resources =
    
    type TestCss() =
        inherit BaseResource("test-embeddedres.css")

    type TestContentCss() =
        inherit BaseResource("css/test-content.css")

    [<assembly: System.Web.UI.WebResource("test-embeddedres.css", "html/css");
      assembly: Require(typeof<TestCss>)>]
    do()