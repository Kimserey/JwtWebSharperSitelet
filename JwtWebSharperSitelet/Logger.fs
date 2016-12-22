module JwtWebSharperSitelet.Logger

open System
open Common
open NLog

let instance = 
    LogManager.GetCurrentClassLogger()