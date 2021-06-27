module SessionRunner
    open CookSessionServiceModel.Model
    open Newtonsoft.Json
    open PidServiceModel.Models
    open SensorServiceModel.Model
    open Serilog
    open System
    open System.Net.Http
    open System.Text.Json

    type PidConfiguration = {
        ConstantP:double;
        ConstantI:double;
        ConstantD:double;
        TargetValue:double;
    }

    type SessionConfiguration = {
        SessionId:Guid;
        PidConfiguration:PidConfiguration;
    }

    // TODO: Refactor these external calls into separate module with configurable urls etc.
    // TODO: Wire in an HTTPClientFactory somehow
    let handlePidOutput (pidOutput:double) =
        let log = Log.Logger
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(10.0)
        async {
            let content = new StringContent("",Text.Encoding.Default, "application/json")
            let method = if pidOutput >0.0 then "TurnOn" else "TurnOff"
            log.Information("Sending {method} to fanController",method)
            let! response = client.PostAsync("http://fancontroller/"+method, content) |> Async.AwaitTask
            if response.IsSuccessStatusCode then
                ignore()
            else
                raise (new HttpRequestException(response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously))
        }

    let getActiveSessionInfo() =
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(10.0)
        async {
            let! response = client.GetAsync("http://cooksessionservice/GetSession")
                                |> Async.AwaitTask
            if response.StatusCode = Net.HttpStatusCode.NotFound then
                return None
            else
                let! responseString = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                let sessionInfo = responseString |> JsonConvert.DeserializeObject<GetActiveSessionResponse>
                return sessionInfo|>Option.Some
        }

    let getPidOutput (request:PidServiceRequest)=
        let client = new HttpClient()
        client.Timeout <- TimeSpan.FromSeconds(10.0)
        async {
            let requestString = JsonSerializer.Serialize<PidServiceRequest>(request)
            let requestObject = new StringContent(requestString, Text.Encoding.Default, "application/json")
            let! response = client.PostAsync("http://pidservice", requestObject)
                            |> Async.AwaitTask
            let! responseString = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let result = responseString
                            |> (fun r -> JsonConvert.DeserializeObject<PidServiceResponse>(r))
            return result
        }

    let processSessionTick (getPidOutput:PidServiceRequest->Async<PidServiceResponse>) 
                           (getActiveSessionInfo:unit->Async<GetActiveSessionResponse option>)
                           (pidOutputHandler:double->Async<unit>) =
        let log = Log.Logger
        log.Information("Processing session tick")
        async {
            let! sessionInfo = getActiveSessionInfo()
            log.Information("Got sessionInfo {sessionInfo}",sessionInfo)
            match sessionInfo with
            | Some sessionInfo ->
                                let pidRequest:PidServiceRequest = {
                                    PidSession=sessionInfo.SessionId;
                                    ConstantP=1.0;
                                    ConstantI=1.0;
                                    ConstantD=1.0;
                                    TargetValue=sessionInfo.TargetValue;
                                    InputValue=sessionInfo.CurrentValue
                                }
                                let! pidOutput = getPidOutput pidRequest
                                do! pidOutputHandler pidOutput.OutputValue
            | None -> ignore()
        }
        


