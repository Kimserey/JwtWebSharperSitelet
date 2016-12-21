namespace Storage

open System
open SQLite
open Common

[<Table("user_accounts"); CLIMutable>]
type UserAccount =
    {
        [<Column("id"); Unique; PrimaryKey>]
        Id: string
        
        [<Column("full_name")>]
        FullName: string
        
        [<Column("password")>]
        Password: string
        
        [<Column("enabled")>]
        Enabled:bool

        [<Column("creation_date")>]
        CreationDate: DateTime
    }

[<AutoOpen>]
module Store =
    
    let private getConnection (database: string) =
        let conn = new SQLiteConnection(database, false)
        conn.CreateTable<UserAccount>() |> ignore
        conn

    module UserRegistry =

        type UserRegistryApi =
            {
                Get: UserId -> Common.UserAccount option
                Create: UserId -> FullName -> Password -> unit
            }
        and FullName = string

        let private get database (UserId userId) =
            use conn = getConnection database
            try
                let user = conn.Find<UserAccount>(userId)
                Some ({ Id = UserId user.Id
                        FullName = user.FullName
                        Password = Password user.Password
                        Enabled = user.Enabled
                        CreationDate = user.CreationDate } : Common.UserAccount)
            with
            | ex -> None

        let private create database (UserId userId) fullname (Password pwd) =
            use conn = getConnection database
            conn.Insert 
                { 
                    Id = userId 
                    FullName = fullname
                    Password = pwd
                    Enabled = true
                    CreationDate = DateTime.UtcNow
                } 
            |> ignore

        let api databasePath =
            {
                Get = get databasePath
                Create = create databasePath
            }