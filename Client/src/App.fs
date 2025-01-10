module App

open System
open Sutil
open Sutil.CoreElements
open Fable.Core


let getMovements () =
    promise {
        let! movements = Client.Services.getMovements ()
        Browser.Dom.console.log (movements)
        return movements
    }

let private HomePage () =
    Html.div [
        Html.text "Hello, World!"
        Html.button [
            Html.text "Click me!"

            Ev.onClick (fun _ ->

                Bind.promise (
                    getMovements (),
                    fun a ->
                        Browser.Dom.console.log a
                        fragment []
                )
                |> ignore
            )
        ]
    ]

let app () = Html.div [ HomePage() ]

app () |> Program.mount
