module SessionStore
    open Newtonsoft.Json
    open System
    open System.IO

    type SessionInfo = {
        SessionName: string;
        SessionId: Guid;
        TargetValue: double;
        StartDate: DateTime;
    }
    let activeSessionPath = "./data/active"
    let endedSessionPath = "./data/ended"

    let initStore() = 
        Directory.CreateDirectory activeSessionPath |> ignore
        Directory.CreateDirectory endedSessionPath |> ignore

    let storeSession (sessionInfo: SessionInfo) =
        let sessionPath = activeSessionPath + "/" + string(sessionInfo.SessionId) + ".dat"
        let sessionInfoString = JsonConvert.SerializeObject sessionInfo
        File.WriteAllText (sessionPath, sessionInfoString)

    let getActiveSession() = 
        let sessionPath = activeSessionPath
        let activeSessionPath = Directory.EnumerateFiles sessionPath
                                |> Seq.sortByDescending (fun n -> (new FileInfo(n)).LastWriteTimeUtc)
                                |> Seq.tryHead
        activeSessionPath
        |> Option.map File.ReadAllText
        |> Option.map JsonConvert.DeserializeObject<SessionInfo>

    let getSession (sessionId: Guid) = 
        let sessionPath = activeSessionPath + "/" + string(sessionId) + ".dat"
        let sessionInfoString = File.ReadAllText sessionPath
        JsonConvert.DeserializeObject<SessionInfo> sessionInfoString

    let endSession (sessionId: Guid) =
        let sessionPath = activeSessionPath + "/" + string(sessionId) + ".dat"
        let endSessionPath = endedSessionPath + "/" + string(sessionId) + ".dat"
        File.Move(sessionPath,endSessionPath)
