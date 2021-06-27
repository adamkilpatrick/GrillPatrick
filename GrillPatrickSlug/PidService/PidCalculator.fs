module PidCalculator 
    open Microsoft.AspNetCore.Http
    open System
    open PidService.PidState
    open PidServiceModel.Models
    let calculatePidOutputs (constantP:double)
                            (constantI:double) 
                            (constantD:double)
                            (input:double)
                            (setpoint:double)
                            (previousError:double)
                            (previousIntegral:double)
                            (deltaTime:double)
                            =

        let newError = setpoint - input
        let newIntegral = previousIntegral + newError * deltaTime
        let derivative = (newError - previousError) / deltaTime
        let output = constantP * newError + constantI * newIntegral + constantD * derivative
        (output, newIntegral, newError)

    let initializePidSession (storagePutter: Guid->PidStateData->unit) (sessionid: Guid)  =
        let defaultStateData:PidStateData = {
            TimeStamp = DateTime.UtcNow;
            LastError = 0.0;
            IntegrationSum = 0.0;
        }
        storagePutter sessionid defaultStateData
        defaultStateData

    let calculatePidFromStorage (storageGetter: Guid->PidStateData Option)
                                (storagePutter: Guid->PidStateData -> unit)
                                (request: PidServiceRequest)
                                =
        let lastState = storageGetter request.PidSession
                        |> Option.defaultWith (fun () -> initializePidSession storagePutter request.PidSession)

        let timeDelta = (DateTime.UtcNow - lastState.TimeStamp).Seconds
                        |> (fun d -> Math.Max(1,d))
                        |> double

        let (output, newIntegral, newError) = calculatePidOutputs request.ConstantD
                                                                    request.ConstantI
                                                                    request.ConstantD
                                                                    request.InputValue
                                                                    request.TargetValue
                                                                    lastState.LastError
                                                                    lastState.IntegrationSum
                                                                    timeDelta
        let newState:PidStateData = {
            TimeStamp = DateTime.UtcNow;
            LastError = newError;
            IntegrationSum = newIntegral;
        }
        storagePutter request.PidSession newState

        let response: PidServiceResponse = {
            OutputValue = output
        }
        response

    let requestDelegate (httpContext: HttpContext) = 
        let request = httpContext.Request.ReadFromJsonAsync<PidServiceRequest>()
                        |> (fun t -> t.AsTask())
                        |> Async.AwaitTask
                        |> Async.RunSynchronously
        let response = calculatePidFromStorage getPidState putPidState request
        response
        |> httpContext.Response.WriteAsJsonAsync


