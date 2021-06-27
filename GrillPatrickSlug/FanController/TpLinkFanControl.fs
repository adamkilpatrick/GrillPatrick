module TpLinkFanControl
    open Microsoft.AspNetCore.Http
    open Serilog
    open System.Diagnostics

    let sendCommand (host:string) (command: string) =
        let log = Log.Logger
        let proc = Process.Start("kasa", [|"--host="+host; command|])
        log.Debug("Process: {args}",proc.StartInfo.ArgumentList);
        let result = proc.WaitForExit(40000)
        log.Debug("Proc exit code {procCode}",proc.ExitCode)


    let turnOnDelegate (turnOn:unit->unit) (httpContext: HttpContext) = 
        turnOn()
        httpContext.Response.WriteAsync "OK"

    let turnOffDelegate (turnOff:unit->unit) (httpContext: HttpContext) = 
        turnOff()
        httpContext.Response.WriteAsync "OK"
        

