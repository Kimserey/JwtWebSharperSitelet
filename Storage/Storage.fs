namespace Storage

open System
open System.Linq
open System.IO
open System.Text
open SQLite
open Common
open Newtonsoft.Json

[<Table("user_accounts"); CLIMutable>]
type UserAccount =
    {
        [<Column "id"; PrimaryKey; Collation "nocase">]
        Id: string
        [<Column("full_name")>]
        FullName: string
        [<Column("email")>]
        Email: string
        [<Column("password")>]
        Password: string
        [<Column "passwordtimestamp">]                          
        PasswordTimestamp : DateTime
        [<Column("enabled")>]
        Enabled:bool
        [<Column("creation_date")>]
        CreationDate: DateTime
        [<Column("claims")>]
        Claims: string
    }
    
[<Table("logs"); CLIMutable>]
type Log =
    {
        [<Column "id"; PrimaryKey; AutoIncrement>]
        Id: int
        [<Column "timestamp">]
        Timestamp: DateTime
        [<Column "level">]
        Level: string
        [<Column "logger">]
        Logger: string
        [<Column "message">]
        Message: string
    }

module LogRegistry =    
    
    let private getConnection (database: string) =
        let conn = new SQLiteConnection(database, false)
        conn.CreateTable<Log>() |> ignore
        conn
        
    let log database timestamp level logger message =
        use conn = getConnection database
        conn.Insert {
            Id = 0
            Timestamp = timestamp
            Level = level
            Logger = logger
            Message = message
        }
        |> ignore

module UserRegistry =

    type UserRegistryApi =
        {
            Get: UserId -> Common.UserAccount option
            Create: UserId -> Password -> FullName -> Email -> Claims -> unit
        }
    and FullName = string
    and Email = string
    and Claims = string list
        
    let private getConnection (database: string) =
        let conn = new SQLiteConnection(database, false)
        conn.CreateTable<UserAccount>() |> ignore
        conn

    let private get database (UserId userId) =
        use conn = getConnection database
        let user = conn.Find<UserAccount>(userId)
        if not <| Object.ReferenceEquals(user, Unchecked.defaultof<UserAccount>) then
            Some ({ Id = UserId user.Id
                    Email = user.Email
                    FullName = user.FullName
                    Password = Password user.Password
                    PasswordTimestamp = user.PasswordTimestamp
                    Enabled = user.Enabled
                    CreationDate = user.CreationDate 
                    Claims = JsonConvert.DeserializeObject<string list> user.Claims } : Common.UserAccount)
        else
            None

    let private create database (UserId userId) (Password pwd) (fullname: string) (email: string) (claims: string list) =
        use conn = getConnection database
        let timestamp = DateTime.UtcNow
        conn.Insert 
            ({ Id = userId
               FullName = fullname
               Email = email
               Password= pwd
               PasswordTimestamp = timestamp
               CreationDate = timestamp
               Enabled = true
               Claims = JsonConvert.SerializeObject claims } : UserAccount) 
        |> ignore

    let api databasePath =
        {
            Get = get databasePath
            Create = create databasePath
        }