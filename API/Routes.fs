module Api.Routes

open Microsoft.AspNetCore.Http
open Giraffe


let getMovementsHandler (next: HttpFunc) (ctx: HttpContext) =
    task {
        let movements = DataAccess.getMovements ()
        return! json movements next ctx
    }

let routes: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [
            GET
            >=> route "/movements"
            >=> getMovementsHandler
        ]
