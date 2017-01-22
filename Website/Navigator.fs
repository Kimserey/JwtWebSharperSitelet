namespace Website

open Common
open Storage 
open System.IO

type PageNavigator =
    {
        GoHome: unit -> unit
        GoAuth: unit -> unit
    }