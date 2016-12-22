namespace Storage

open System
open SQLite
open SQLiteNetExtensions.Attributes
open Common

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
        [<Column("claims"); TextBlob("ClaimsBlobbed")>]
        Claims: string list
        ClaimsBlobbed: string
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

[<AutoOpen>]
module Store =
    
    let private getConnection (database: string) =
        let conn = new SQLiteConnection(database, SQLiteOpenFlags.Create, false)
        conn.CreateTable<UserAccount>() |> ignore
        conn.CreateTable<Log>() |> ignore
        conn
    
    module LogRegistry =
        
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
                        Claims = user.Claims} : Common.UserAccount)
            else
                None
        let private create database (UserId userId) (Password pwd) fullname email claims =
            use conn = getConnection database
            let timestamp = DateTime.UtcNow
            conn.Insert 
                { 
                    Id = userId 
                    FullName = fullname
                    Email = email
                    Password = pwd
                    PasswordTimestamp = timestamp
                    Enabled = true
                    CreationDate = timestamp
                    Claims = claims
                    ClaimsBlobbed = Unchecked.defaultof<string>
                } 
            |> ignore

        let api databasePath =
            {
                Get = get databasePath
                Create = create databasePath
            }