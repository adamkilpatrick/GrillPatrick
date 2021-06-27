module BeginSession
    open CookSessionServiceModel.Model
    open Microsoft.AspNetCore.Http
    open SessionStore
    open System

    let beginSessionHandle (storeSession: SessionInfo -> unit) (beginSessionRequest: BeginSessionRequest) =
        let sessionId = Guid.NewGuid()
        let (newSessionInfo: SessionInfo) = {
            SessionId = sessionId;
            SessionName = beginSessionRequest.SessionName;
            TargetValue = beginSessionRequest.TargetValue;
            StartDate = DateTime.UtcNow;
        }
        storeSession newSessionInfo
        let (response: BeginSessionResponse) = {
            SessionId = sessionId;
            SessionName = beginSessionRequest.SessionName;
        }
        response

    let beginSessionDelegate (storeSession: SessionInfo -> unit) (httpContext: HttpContext) =
        let request = httpContext.Request.ReadFromJsonAsync<BeginSessionRequest>()
                        |> (fun t -> t.AsTask())
                        |> Async.AwaitTask
                        |> Async.RunSynchronously
        let response = beginSessionHandle storeSession request
        response
        |> httpContext.Response.WriteAsJsonAsync
        

