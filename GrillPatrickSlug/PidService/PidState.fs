namespace PidService
module PidState = 

  open System
  open System.IO
  open System.Text.Json

  type PidStateData = {
    TimeStamp:DateTime;
    LastError:double;
    IntegrationSum:double;
  }


  let getPidState (sessionId:Guid) = 
    let fileName = "./"+sessionId.ToString()+".dat"
    if File.Exists fileName then
        File.ReadAllText ("./"+sessionId.ToString()+".dat")
        |> JsonSerializer.Deserialize<PidStateData>
        |> Option.Some
    else
        Option.None

  let putPidState (sessionId:Guid) (stateData:PidStateData) = 
    JsonSerializer.Serialize stateData
    |> (fun d -> File.WriteAllText ("./"+sessionId.ToString()+".dat",d))

