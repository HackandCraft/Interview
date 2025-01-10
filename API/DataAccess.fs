module Api.DataAccess

open Serilog
open Dapper
open Microsoft.Data.SqlClient


let connectionString =
    "Server=tcp:hncdev.database.windows.net,1433;Database=Interview;User ID=InterviewUser;Password=09bt4aS^pp#M{!))2P[B;Encrypt=true;Connection Timeout=30;"

let getConnection () = new SqlConnection(connectionString)

let fetchData<'T> (query: string) (parameters: obj option) =
    try
        use connection = new SqlConnection(connectionString)
        connection.Open()

        match parameters with
        | Some p -> connection.Query<'T>(query, p) |> Seq.toList
        | None -> connection.Query<'T>(query) |> Seq.toList
    with ex ->
        Log.Error(ex, "Error fetching data")
        failwith ex.Message

type MovementDTO =
    { ShipFromLocationId: string
      ShipFromType: string
      ShipFromLongitude: float
      ShipFromLatitude: float
      ShipToLocationId: string
      ShipToType: string
      ShipToLongitude: float
      ShipToLatitude: float
      ModeOfTransport: string
      Quantity: int64 }

let getMovements () : MovementDTO list =
    let sql =
        """
            declare @total int

            select
                @total = sum(quantity)
            from [dbo].[SyntheticAggregatedMovements]

            select top 2
                [ShipFromLocationId],
                [ShipFromType],
                [ShipFromLongitude],
                [ShipFromLatitude],
                [ShipToLocationId],
                [ShipToType],
                [ShipToLongitude],
                [ShipToLatitude],
                [TransportMode] as ModeOfTransport,
                [Quantity]
            from [dbo].[SyntheticAggregatedMovements]
            where quantity > (@total * 0.0001)
            and [ShipFromLongitude] != 0
            and [ShipFromLatitude] != 0
            and [ShipToLongitude] != 0
            and [ShipToLatitude] != 0
        """

    fetchData<MovementDTO> sql None
