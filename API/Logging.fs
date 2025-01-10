module Api.Logging

open System
open System.IO
open Microsoft.AspNetCore.Http
open Serilog
open Serilog.Formatting.Json
open Giraffe
open Giraffe.SerilogExtensions


let getEnvironment () =
    Environment.GetEnvironmentVariable ("ENVIRONMENT")

let getSeqApiKey () =
    Environment.GetEnvironmentVariable ("SEQ_API_KEY")

let configureSerilog () =
    printfn "Configuring Serilog"
    printfn "Environment: %s" (getEnvironment ())
    printfn "Seq API Key: %s" (getSeqApiKey ())

    Log.Logger <-
        LoggerConfiguration()
            .Destructure.FSharpTypes()
            .Enrich.WithProperty("Application", "Interview")
            .Enrich.WithProperty("Service", "Api")
            .Enrich.WithProperty("Env", getEnvironment ())
            .Enrich.WithMachineName()
            .MinimumLevel.Information()
            .WriteTo.Seq("https://hnc-seq.hncdev.co.uk/", apiKey = getSeqApiKey ())
            .WriteTo.Console(JsonFormatter ())
            .Enrich.FromLogContext()
            .CreateLogger ()

let readRequestBody (context: HttpContext) =
    context.ReadBodyBufferedFromRequestAsync ()
    |> Async.AwaitTask

type LogRequestMiddleware(next: RequestDelegate) =
    member this.Invoke (context: HttpContext) =
        async {
            let! requestBodyPayload = readRequestBody (context)

            // Copy a pointer to the original response body stream
            let originalBodyStream = context.Response.Body

            use responseBody = new MemoryStream ()
            // Point the response body to a memory stream
            context.Response.Body <- responseBody

            do!
                next.Invoke (context)
                |> Async.AwaitTask

            // Read and log the response body data
            context.Response.Body.Seek (0L, SeekOrigin.Begin)
            |> ignore

            use reader = new StreamReader (context.Response.Body)

            let! responseBodyPayload =
                reader.ReadToEndAsync ()
                |> Async.AwaitTask

            context.Response.Body.Seek (0L, SeekOrigin.Begin)
            |> ignore

            let method = context.Request.Method
            let path = context.Request.Path

            // Log.Information (
            //     "{method} {path} Request: {requestBodyPayload} Response {statusCode}: {responseBodyPayload}",
            //     method,
            //     path,
            //     requestBodyPayload,
            //     context.Response.StatusCode,
            //     responseBodyPayload
            // )

            // Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
            do!
                responseBody.CopyToAsync (originalBodyStream)
                |> Async.AwaitTask
        }
        |> Async.StartAsTask

let serilogMiddlewareConfig =
    { SerilogConfig.defaults with
        ErrorHandler =
            fun ex context ->
                setStatusCode 500
                >=> text ex.Message
    }
