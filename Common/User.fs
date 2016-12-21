namespace Common

open System
open System.Security
open System.Security.Principal

type UserAccount = 
    {
        Id: UserId
        FullName: string
        Email: string
        Password: Password
        PasswordTimestamp : DateTime
        Enabled:bool
        CreationDate: DateTime
        Claims: string list
    }
and UserId = UserId of string
and Password = Password of string

type AuthenticateResult =
    | Success of UserAccount
    | Failure

type Credentials =
    {
        UserId: string
        Password: string
    }

type UserIdentity = 
    {
        Name: string
        IsAuthenticated: bool
        AuthenticationType: string
    } with
        interface IIdentity with
            member self.AuthenticationType = self.AuthenticationType
            member self.IsAuthenticated = self.IsAuthenticated
            member self.Name = self.Name

type UserPrincipal =
    {
        Identity: UserIdentity
        Claims: string list
    } with
        interface IPrincipal with
            member self.Identity with get() = self.Identity :> IIdentity 
            member self.IsInRole role = self.Claims |> List.exists ((=) role)