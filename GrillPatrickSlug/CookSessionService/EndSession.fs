module EndSession
    open CookSessionServiceModel.Model
    open CookSessionService.Metrics
    open Microsoft.AspNetCore.Http
    open System

    let endSessionHandler (sessionEnder:Guid->unit) (endSessionRequest: EndSessionRequest) =
        tempGauge.WithLabels([|string(endSessionRequest.SessionId)|]).Unpublish()
        targetGauge.WithLabels([|string(endSessionRequest.SessionId)|]).Unpublish()
        endSessionRequest.SessionId
        |> sessionEnder

    let endSessionDelegate (sesionEnder:Guid->unit) (httpContext:HttpContext) =
        let request = httpContext.Request.ReadFromJsonAsync<EndSessionRequest>() |> (fun n -> n.AsTask() |> Async.AwaitTask |> Async.RunSynchronously)
        request |> endSessionHandler sesionEnder
        httpContext.Response.WriteAsync "OK"


