namespace FanController

open TpLinkFanControl
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Serilog.AspNetCore

type Startup() =
    let tpLinkHost = System.Environment.GetEnvironmentVariable("TP_LINK_HOST")
    let turnOn() = sendCommand tpLinkHost "on"
    let turnOff() = sendCommand tpLinkHost "off"

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member _.ConfigureServices(services: IServiceCollection) =
        ()

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseRouting()
           .UseEndpoints(fun endpoints ->
                endpoints.MapPost("/TurnOn", fun context -> turnOnDelegate turnOn context) |> ignore
                endpoints.MapPost("/TurnOff", fun context -> turnOffDelegate turnOff context) |> ignore
            ) |> ignore
