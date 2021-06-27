namespace CookSessionWorker

open SessionRunner
open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Serilog

type Worker(logger: ILogger<Worker>) =
    inherit BackgroundService()

    override _.ExecuteAsync(ct: CancellationToken) =
        async {
            while not ct.IsCancellationRequested do
                logger.LogInformation("Worker start running at: {time}", DateTimeOffset.Now)
                do! processSessionTick getPidOutput getActiveSessionInfo handlePidOutput
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)
                do! Async.Sleep(20000)
        }
        |> Async.StartAsTask
        :> Task // need to convert into the parameter-less task