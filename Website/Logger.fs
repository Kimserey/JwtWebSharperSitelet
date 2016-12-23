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

    static member RegisterTarget() =
        ConfigurationItemFactory.Default.Targets.RegisterDefinition("SqliteLog", typeof<SqliteLogTarget>)
    
type MessageLog =
| Log of string
| GetUnread of AsyncReplyChannel<string list>
    
let HttpLogAgent = 
    MailboxProcessor.Start(fun inbox ->
        let rec processMessage unread =
            async {
                let! msg = inbox.Receive()
                let newState =
                    match msg with
                    | Log msg -> unread @ [ msg ]
                    | GetUnread reply -> reply.Reply unread; []

                return! processMessage newState
            }
        processMessage [])

type HttpSSELogTarget() =
    inherit TargetWithLayout()
    override self.Write(logEvent: LogEventInfo) =
        let message = self.Layout.Render logEvent
        HttpLogAgent.Post(Log message)

    static member RegisterTarget() =
        ConfigurationItemFactory.Default.Targets.RegisterDefinition("HttpLog", typeof<HttpSSELogTarget>)
