module GetSession
    open CookSessionServiceModel.Model
    open CookSessionService.Metrics
    open Microsoft.AspNetCore.Http
    open Newtonsoft.Json
    open SensorServiceModel.Model
    open SessionStore
    open System
    open System.Net.Http
    open Serilog


    let emitSessionMetrics (sessionResponse: GetActiveSessionResponse) = 
        tempGauge.WithLabels([|string(sessionResponse.SessionId)|])
        |> (fun n -> n.Set sessionResponse.CurrentValue)

        targetGauge.WithLabels([|string(sessionResponse.SessionId)|])
        |> (fun n -> n.Set sessionResponse.TargetValue)

    let getSensorOutput() =
        let log = Log.Logger
        log.Debug("calling sensoroutput")
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(10.0)
        async {
            let! response = client.GetStringAsync "http://sensorservice" |> Async.AwaitTask
            let result = response
                            |> (fun r -> JsonConvert.DeserializeObject<SensorServiceResponse>(r))
            return result
        }

    let getSessionHandler (getActiveSession: unit -> SessionInfo Option) (getCurrentTemp: unit -> Async<SensorServiceResponse>) =
        let log = Log.Logger
        let sessionInfo = getActiveSession()
        log.Debug("Got sessionInfo {sessionInfo}", sessionInfo)
        let temp = getCurrentTemp() |> Async.RunSynchronously
        let mapResponse (someSessionInfo: SessionInfo) =
            let response: GetActiveSessionResponse = {
                SessionName=someSessionInfo.SessionName;
                SessionId=someSessionInfo.SessionId;
                StartDate=someSessionInfo.StartDate;
                TargetValue=someSessionInfo.TargetValue;
                CurrentValue=temp|>Seq.head |>(fun n ->n.Value.FahrenheitTemperature)
            }
            response
        sessionInfo
        |> Option.map mapResponse


    let getSessionDelegate (getActiveSession: unit -> SessionInfo Option) (getCurrentTemp: unit -> Async<SensorServiceResponse>) (httpContext: HttpContext) =
        let response = getSessionHandler getActiveSession getCurrentTemp
        match response with
        | Some a ->
                    emitSessionMetrics a
                    httpContext.Response.WriteAsJsonAsync a
        | None ->
                    httpContext.Response.StatusCode <- 404
                    httpContext.Response.WriteAsync "No active session"
        



