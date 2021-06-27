namespace CookSessionWorker

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events

module Program =
    let config = (new LoggerConfiguration()).MinimumLevel.Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext().WriteTo.Console()
    Log.Logger <- config.CreateLogger()
    Log.Logger.Debug"foo"
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices(fun hostContext services ->
                services.AddHostedService<Worker>() |> ignore)

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // exit code