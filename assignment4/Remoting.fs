﻿namespace assignment4

open WebSharper
open System.Security.Cryptography
open System.Text

module Server =

    [<Rpc>]
    let Tweet input =
        let R (tweetMsg: string) = System.String(Array.rev(tweetMsg.ToCharArray()))
        async {
            return R input
        }

    [<Rpc>]
    let LoginUser userpass = 
        let ctx = Web.Remoting.GetContext() 
        if userpass.Pass = "correct" then
            ctx.UserSession.LoginUser(userpass.User, persistent=true) |> Async.Ignore
        else 
            async.Return()

    [<Rpc>]
    let LogoutUser() = 
        let ctx = Web.Remoting.GetContext()
        ctx.UserSession.Logout()
