namespace CookSessionService

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events

module Program =
    let config = (new LoggerConfiguration()).MinimumLevel.Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext().WriteTo.Console()
    Log.Logger <- config.CreateLogger()
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // Exit code
