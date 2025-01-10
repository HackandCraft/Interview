open System
open System.Net.Http
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open Giraffe.SerilogExtensions
open Thoth.Json.Net
open Api.Logging
open Api.Routes


let extraCoders =
    Extra.empty
    |> Extra.withInt64
    |> Extra.withDecimal
    |> Extra.withBigInt

let appWithLogger = SerilogAdapter.Enable (routes, serilogMiddlewareConfig)

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError (ex, "An unhandled exception has occurred")

    clearResponse
    >=> ServerErrors.INTERNAL_ERROR ex.Message

type Startup() =
    member __.ConfigureServices (services: IServiceCollection) =
        // Register default Giraffe dependencies
        services.AddCors () |> ignore

        services.AddGiraffe ()
        |> ignore

        services.AddSingleton<Json.ISerializer> (
            Thoth.Json.Giraffe.ThothSerializer (extra = extraCoders, caseStrategy = CaseStrategy.CamelCase)
        )
        |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =
        // Add Giraffe to the ASP.NET Core pipeline
        app.UseGiraffeErrorHandler errorHandler
        |> ignore

        let arr =
            [|
                "http://localhost:5000"
                "http://localhost:5174"
            |]

        app.UseCors (fun builder ->
            builder
                .WithOrigins(arr)
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader()
                .SetIsOriginAllowed (fun x -> true)
            |> ignore
        )
        |> ignore

        app.UseMiddleware<LogRequestMiddleware> ()
        |> ignore

        app.UseGiraffe appWithLogger

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder.UseStartup<Startup> ()
            |> ignore
        )
        .Build()
        .Run ()

    0
