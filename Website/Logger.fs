module Website.Logger

open System
open Common
open NLog
open NLog.Targets
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
        LogRegistry.log 
            self.Database 
            logEvent.TimeStamp 
            logEvent.Level.Name 
            logEvent.LoggerName 
            message

let instance = 
    ConfigurationItemFactory.Default.Targets.RegisterDefinition("SqliteLog", typeof<SqliteLogTarget>)
    LogManager.GetCurrentClassLogger()
    