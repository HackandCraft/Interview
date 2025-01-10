module Client.Services

open Fable.Core.JS
open Thoth.Fetch
open Thoth.Json

[<RequireQualifiedAccess>]
module Generic =
    let extraCoders =
        Extra.empty |> Extra.withInt64 |> Extra.withDecimal |> Extra.withBigInt

    let caseStrategy: CaseStrategy = CamelCase

    let properties: Fetch.Types.RequestProperties list = [
        Fetch.Types.RequestProperties.Credentials Fetch.Types.RequestCredentials.Include
    ]

    let inline Get<'Response> (url: string) : Promise<'Response> =
        promise { return! Fetch.get (url, extra = extraCoders, caseStrategy = caseStrategy, properties = properties) }

    let inline Post<'Request, 'Response> (url: string) (request: 'Request) : Promise<'Response> =
        promise {
            return!
                Fetch.post (
                    url,
                    data = request,
                    extra = extraCoders,
                    caseStrategy = caseStrategy,
                    properties = properties
                )
        }

    let inline Put<'Request, 'Response> (url: string) (request: 'Request) : Promise<'Response> =
        promise {
            return!
                Fetch.put (
                    url,
                    data = request,
                    extra = extraCoders,
                    caseStrategy = caseStrategy,
                    properties = properties
                )
        }

    let inline Delete<'Response> (url: string) : Promise<'Response> =
        promise {
            return! Fetch.delete (url, extra = extraCoders, caseStrategy = caseStrategy, properties = properties)
        }

[<Literal>]
let apiBaseUrl = "http://localhost:5000"

type MovementDTO = {
    ShipFromLocationId: string
    ShipFromType: string
    ShipFromLongitude: float
    ShipFromLatitude: float
    ShipToLocationId: string
    ShipToType: string
    ShipToLongitude: float
    ShipToLatitude: float
    ModeOfTransport: string
    Quantity: int64
}

let getMovements () =
    Generic.Get<MovementDTO array> $"{apiBaseUrl}/movements"
