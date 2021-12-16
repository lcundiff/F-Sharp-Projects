module WebSharper.AspNetCore.Tests.WebSocketClient

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client
open WebSocketServer
open System

module Server = WebSocketServer

[<JavaScript>]
let WebSocketLogin (endpoint: WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer>) = 
    // Check on this
    let mutable server: WebSocketServer<Server.MessagesToClient, MessagesToServer> option = None
    let  userName = Var.Create ""



    let loginToServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Login)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.RemoveAttribute("disabled")
            subscribeBox.RemoveAttribute("disabled")
            unsubscribeBox.RemoveAttribute("disabled")
            queryBox.RemoveAttribute("disabled")
            tweetBtn.RemoveAttribute("disabled")
            subscribeBtn.RemoveAttribute("disabled")
            unsubscribeBtn.RemoveAttribute("disabled")
            queryBtn.RemoveAttribute("disabled")


            loginbtn.RemoveAttribute("disabled")
            logoutbtn.RemoveAttribute("style")
            loginbtn.SetAttribute("display", "none")
        } |> Async.Start

    let logoutOfServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Logout)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.SetAttribute("disabled", "true")
            subscribeBox.SetAttribute("disabled", "true")
            unsubscribeBox.SetAttribute("disabled", "true")
            queryBox.SetAttribute("disabled", "true")
            tweetBtn.SetAttribute("disabled", "true")
            subscribeBtn.SetAttribute("disabled", "true")
            unsubscribeBtn.SetAttribute("disabled", "true")
            queryBtn.SetAttribute("disabled", "true")


            loginbtn.SetAttribute("style","display:block")
            logoutbtn.SetAttribute("style","display:none")
            logoutbtn.SetAttribute("disabled", "true")
        } |> Async.Start

    let enableFunctionality () = 
        let btn = JS.Document.GetElementById("register")
        btn.InnerHTML <- "Account Registered"
        btn.SetAttribute("disabled", "true")
        let loginbtn = JS.Document.GetElementById("login")
        let logoutbtn = JS.Document.GetElementById("logout")
        loginbtn.RemoveAttribute("style")
        logoutbtn.RemoveAttribute("style")
        let logoutbtn = JS.Document.GetElementById("logout")
        logoutbtn.SetAttribute("style", "display:block")
        userName.Value <- ("Welcome to Twitter, @" + userName.Value)
        let form = JS.Document.GetElementById("register-form")
        form.SetAttribute("disabled", "true")
        let loginoutbtn = JS.Document.GetElementById("logout")
        loginoutbtn.RemoveAttribute("disabled")

    // Connects to Server and Waits for Tweets...?
    let connectToServer = 
        async { 
            printfn "In here!"
            return! ConnectStateful endpoint <| fun server -> async {
                return 0, fun state msg -> async {
                    printfn "In in here here! %A" msg
                    match msg with 
                    | Message data ->
                        match data with 
                        | Success succ ->
                            printfn "Success! %s" succ
                        | Failure fail ->
                            printfn "Failure Login Page! %s" fail
                            JS.Alert(fail)
                        | RegisteredFromServer ->
                            enableFunctionality()
                        | CurrentAccount account->
                            printfn "account retrieved from server"
                            userName.Value <- account
                        | _ -> printfn "A message was sent back"
                        
                    | _ -> printfn "Failure"
                    return (state + 1)
                    
                }    
            }
        }

    let registerAccount (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                printfn "Trying to connect."
                connectToServer.AsPromise().Then(fun x -> 
                    server <-Some(x)
                    printfn "Registering"
                    server.Value.Post(Server.Register userName.Value)
                ) |> ignore   
            else 
                server.Value.Post(Server.Register userName.Value)
            
            
            // userName.Value <- ""
        }
        |> Async.Start

    let logButtons = 
        div [] [
           button [
           
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "login"
                attr.``disabled`` "false"
                on.click (loginToServer) 
                ] [ a [attr.``href`` "/home"] [text "Login"]]
           button [
               attr.``class`` "tweetBox__tweetButton"
               attr.``id`` "logout"
               attr.``style`` "display:none"
               on.click (logoutOfServer) 
               ] [text "Logout"]
        ]
    let registerBox = 
        div [] [
                Doc.Input [
                    attr.``class`` "form-control"
                    attr.``id`` "register-form"
                ] userName 
                button [ 
                    attr.``class`` "tweetBox__tweetButton" 
                    attr.``id`` "register"
                    on.click (registerAccount) ] [ text "Register" ]
            ]

    if(server = None) then
        // start the server
        connectToServer.AsPromise().Then(fun x -> 
            server <- Some(x)
            printfn "Loading"
            async {
                server.Value.Post(Server.GetCurrentAccount)
            } |> Async.Start       
        ) |> ignore
    else 
        async {
            server.Value.Post(Server.GetCurrentAccount)
        } |> Async.Start 

    div []
        [
            registerBox
            logButtons
        ]

[<JavaScript>]
let WebSocketHome (endpoint: WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer>) = 
    // Check on this
    let mutable server: WebSocketServer<Server.MessagesToClient, MessagesToServer> option = None

    let serverMessagesContainer = Elt.pre [] []
    let messagesHeader = Elt.div [] [
        Elt.h3[][text "Twitter Feed"]
    ]

    let queryMessagesContainer = Elt.pre [] []
    let queryHeader = Elt.div[] [
        Elt.h3[][text "Queried Requests"]
    ]

    let tweetContainer = Elt.div[][]
    let queryContainer = Elt.div[][]
    let mutable queryRequested = ""


    let retweet msg =
        server.Value.Post(Server.Tweet msg)

    let addTweetToFeed user msg = 
        let retweetBtn = 
            Elt.button [
            
            attr.``class`` "retweet-handler btn btn-info"
            on.click (fun _ _ ->
                retweet (msg))
            ] [
                text "Retweet!"
            ]
        let tweetContainerDiv = JS.Document.CreateElement("div")
        let tweetContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let retweetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(retweetButtonDom)
        tweetContentDiv.InnerHTML <- String.Format("
            <div class=\"post\" style=\"display:flex;justify-content:center\"><div class=\"post__body\">{0}</div><div style=\"margin-left:10px\">{1}</div><div>",
            user, msg
        )
        
        
        let tweetContainerContent = tweetContainerDiv.AppendChild(tweetContentDiv)
        tweetContainerContent.AppendChild(retweetOption) |> ignore
        tweetContainer.Dom.AppendChild(tweetContainerContent)|>ignore

    let addQueries user msg = 
        let retweetBtn = 
            Elt.button [
            
            attr.``class`` "retweet-handler btn btn-info"
            on.click (fun _ _ ->
                retweet (msg))
            ] [
                text "Retweet!"
            ]
        let queryContainerDiv = JS.Document.CreateElement("div")
        let queryContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let retweetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(retweetButtonDom)
        queryContentDiv.InnerHTML <- String.Format("<div style=\"display:flex;justify-content:center\"><div>Query for {0}: {1}</div><div style=\"margin-left:10px\">{2}</div><div>",queryRequested,user,msg)
        let queryContainerContent = queryContainerDiv.AppendChild(queryContentDiv)
        queryContainerContent.AppendChild(retweetOption) |> ignore
        queryContainer.Dom.AppendChild(queryContainerContent)|>ignore

    let addMissedTweetToFeed user msg = 
        let retweetBtn = 
            Elt.button [
        
            attr.``class`` "retweet-handler btn btn-info"
            on.click (fun _ _ ->
                retweet (msg))
            ] [
                text "Retweet!"
            ]
        let tweetContainerDiv = JS.Document.CreateElement("div")
        let tweetContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let retweetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(retweetButtonDom)
        tweetContentDiv.InnerHTML <- String.Format("<div style=\"display:flex;justify-content:center\"><div>While you were away you missed: {0}</div><div style=\"margin-left:10px\">{1}</div><div>",user,msg)
        let tweetContainerContent = tweetContainerDiv.AppendChild(tweetContentDiv)
        tweetContainerContent.AppendChild(retweetOption) |> ignore
        tweetContainer.Dom.AppendChild(tweetContainerContent)|>ignore
    let userName = Var.Create ""

    let enableFunctionality () = 
        let btn = JS.Document.GetElementById("register")
        btn.InnerHTML <- "Account Registered"
        btn.SetAttribute("style", "display:none")

        let loginbtn = JS.Document.GetElementById("login")
        let logoutbtn = JS.Document.GetElementById("logout")
        loginbtn.RemoveAttribute("style")
        logoutbtn.RemoveAttribute("style")

        userName.Value <- ("Welcome to Twitter, @" + userName.Value)
        let form = JS.Document.GetElementById("register-form")
        form.SetAttribute("disabled", "true")
        let loginoutbtn = JS.Document.GetElementById("logout")
        loginoutbtn.RemoveAttribute("disabled")

        // enable everything on register
        let tweetBox = JS.Document.GetElementById("tweetbox")
        let subscribeBox = JS.Document.GetElementById("subscribebox")
        let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
        let queryBox = JS.Document.GetElementById("querybox")
        let tweetBtn = JS.Document.GetElementById("tweetbtn")
        let subscribeBtn = JS.Document.GetElementById("subscribebtn")
        let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
        let queryBtn = JS.Document.GetElementById("querybtn")
        tweetBox.RemoveAttribute("disabled")
        subscribeBox.RemoveAttribute("disabled")
        unsubscribeBox.RemoveAttribute("disabled")
        queryBox.RemoveAttribute("disabled")
        tweetBtn.RemoveAttribute("disabled")
        subscribeBtn.RemoveAttribute("disabled")
        unsubscribeBtn.RemoveAttribute("disabled")
        queryBtn.RemoveAttribute("disabled")

    // Connects to Server and Waits for Tweets...?
    let connectToServer = 
        async { 
            printfn "In here!"
            return! ConnectStateful endpoint <| fun server -> async {
                return 0, fun state msg -> async {
                    printfn "In in here here!"
                    match msg with 
                    | Message data ->
                        match data with 
                        | TweetFromServer response ->
                            let responseMsg = response.JS.Split(",")
                            printfn "ResponseMSG %A" (responseMsg)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addTweetToFeed user tweet
                        | MissedTweet response ->
                            let responseMsg = response.JS.Split(",")
                            printfn "ResponseMSG %A" (responseMsg)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addMissedTweetToFeed user tweet
                        | QueryFromServer hashtags ->
                            let responseMsg = hashtags.JS.Split(",")
                            printfn "ResponseMSG %A" (hashtags)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addQueries user tweet
                        | Success succ ->
                            printfn "Success! %s" succ
                        | Failure fail ->
                            printfn "Failure in home page! %s" fail
                            JS.Alert(fail)
                        | RegisteredFromServer ->
                            enableFunctionality()
                        | CurrentAccount account->
                            printfn "account retrieved from server"
                            userName.Value <- account
                        | _ -> printfn "A message was sent back"
                        
                    | _ -> printfn "Failure"
                    return (state + 1)
                    
                }    
            }
        }

    if(server = None) then
        // start the server
        connectToServer.AsPromise().Then(fun x -> 
            server <- Some(x)
            printfn "Loading"
            async {
                server.Value.Post(Server.GetCurrentAccount)
            } |> Async.Start       
        ) |> ignore
    else 
        async {
            server.Value.Post(Server.GetCurrentAccount)
        } |> Async.Start          

    let userToUnsubTo = Var.Create ""
    let tweetMessage = Var.Create ""
    let userToSubTo = Var.Create ""
    let query = Var.Create ""


    let registerAccount (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                printfn "Trying to connect."
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Registering"
            server.Value.Post(Server.Register userName.Value)
            // userName.Value <- ""
        }
        |> Async.Start

    let postTweet (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Tweeting %s" tweetMessage.Value
            server.Value.Post(Server.Tweet tweetMessage.Value)
            tweetMessage.Value <- ""
        }
        |> Async.Start

    let subscribeToUser (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Subscribing to %s" userToSubTo.Value
            server.Value.Post(Server.Subscribe userToSubTo.Value)
            userToSubTo.Value <- ""
        }
        |> Async.Start

    let unsubscribeToUser (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Unsubscribing to %s" userToUnsubTo.Value
            server.Value.Post(Server.Unsubscribe userToUnsubTo.Value)
            userToUnsubTo.Value <- ""
        }
        |> Async.Start

    let queryFromServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Querying #%s" query.Value
            server.Value.Post(Server.Query query.Value)
            queryRequested <- query.Value
            query.Value <- ""
        }
        |> Async.Start

    let loginToServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Login)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.RemoveAttribute("disabled")
            subscribeBox.RemoveAttribute("disabled")
            unsubscribeBox.RemoveAttribute("disabled")
            queryBox.RemoveAttribute("disabled")
            tweetBtn.RemoveAttribute("disabled")
            subscribeBtn.RemoveAttribute("disabled")
            unsubscribeBtn.RemoveAttribute("disabled")
            queryBtn.RemoveAttribute("disabled")

            loginbtn.SetAttribute("style", "display:none")
            logoutbtn.SetAttribute("style", "display:block")
        }

        |> Async.Start

    let logoutOfServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Logout)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.SetAttribute("disabled", "true")
            subscribeBox.SetAttribute("disabled", "true")
            unsubscribeBox.SetAttribute("disabled", "true")
            queryBox.SetAttribute("disabled", "true")
            tweetBtn.SetAttribute("disabled", "true")
            subscribeBtn.SetAttribute("disabled", "true")
            unsubscribeBtn.SetAttribute("disabled", "true")
            queryBtn.SetAttribute("disabled", "true")

            loginbtn.SetAttribute("style", "display:block")
            logoutbtn.SetAttribute("style", "display:none")
        }

        |> Async.Start

    let registerAccount (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                printfn "Trying to connect."
                connectToServer.AsPromise().Then(fun x -> 
                    server <-Some(x)
                    printfn "Registering"
                    server.Value.Post(Server.Register userName.Value)
                ) |> ignore   
            else 
                server.Value.Post(Server.Register userName.Value)
            
            
            // userName.Value <- ""
        }
        |> Async.Start

    let registerBox = 
        div [] [
                Doc.Input [
                    attr.``class`` "form-control"
                    attr.``id`` "register-form"
                ] userName 
                button [ 
                    attr.``class`` "tweetBox__tweetButton" 
                    attr.``id`` "register"
                    attr.``style`` "display:block"
                    on.click (registerAccount) ] [ text "Register" ]
            ]

    let tweetBox = 
        div [] [
            Doc.Input [
                attr.``class`` "form-control"
                attr.``id`` "tweetbox"
                ] tweetMessage
                
            button [
                attr.``class`` "tweetBox__tweetButton" 
                attr.``id`` "tweetbtn"
                on.click (postTweet) ] [text "Tweet"]
        ]
    
    let subscribeBox = 
        div [] [
            Doc.Input [
                attr.``class`` "form-control"
                attr.``id`` "subscribebox"
                ] userToSubTo
                
            button [
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "subscribebtn"

                on.click (subscribeToUser) ] [text "Subscribe"]
        ]

    let unsubscribeBox = 
        div [] [
            Doc.Input [
                attr.``class`` "form-control"
                attr.``id`` "unsubscribebox"
                ] userToUnsubTo
            button [
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "unsubscribebtn"
                
                on.click (unsubscribeToUser) ] [text "Unsubscribe"]
        ]

    let queryHashtagsBox = 
        div [] [
            Doc.Input [
                attr.``class`` "form-control"
                attr.``id`` "querybox"
                ] query
                
            button [
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "querybtn"
                on.click (queryFromServer) ] [text "Query"]
        ]

    let logButtons = 
        div [] [
           button [
           
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "login"
                attr.``style`` "display:none"
                on.click (loginToServer) ] [text "Login"]
           button [
               attr.``class`` "tweetBox__tweetButton"
               attr.``id`` "logout"
               attr.``style`` "display:none"
               on.click (logoutOfServer) ] [text "Logout"]
        ]

    div []
        [
            registerBox
            logButtons
            queryHashtagsBox
            subscribeBox
            unsubscribeBox
            tweetBox
            messagesHeader
            serverMessagesContainer
            tweetContainer
            queryHeader
            queryMessagesContainer
            queryContainer
        ]

    

    (*let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.DoSomething input
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The server responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
    *)
open WebSharper.AspNetCore.WebSocket

let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer> = 
    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)