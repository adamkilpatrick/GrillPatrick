namespace CookSessionService

open BeginSession
open EndSession
open GetSession
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Prometheus

type Startup() =
    let storeSession = SessionStore.storeSession
    let getActiveSession = SessionStore.getActiveSession
    let endSession = SessionStore.endSession
    let getTemperature = getSensorOutput

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member _.ConfigureServices(services: IServiceCollection) =
        ()

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        SessionStore.initStore()
        app.UseHttpMetrics() |> ignore
        if env.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseRouting()
           .UseEndpoints(fun endpoints ->
                endpoints.MapMetrics() |> ignore
                endpoints.MapPost("/BeginSession", fun context -> beginSessionDelegate storeSession context) |> ignore
                endpoints.MapGet("/GetSession", fun context -> getSessionDelegate getActiveSession getTemperature context) |> ignore
                endpoints.MapPost("/EndSession", fun context -> endSessionDelegate endSession context) |> ignore
            ) |> ignore
