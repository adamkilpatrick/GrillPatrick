namespace SensorService

module ArduinoSerial =
    open Microsoft.AspNetCore.Http
    open System.IO.Ports
    open SensorServiceModel.Model
    open System

    let monitor = new Object()

    type ProbeConfig = {
        VoltageIn:double; 
        Resolution:int; 
        ResistorValue:int; 
        Port:string; 
        Baud:int;
        SteinhardA:double;
        SteinhardB:double;
        SteinhardC:double;
    }

    let calculateVoltageRead (voltageIn:double) (resolution:int) (serialInput: string) =
        serialInput
        |> double
        |> fun s -> s*voltageIn/double(resolution)
        
    let calculateProbeResistance (voltageIn:double) (resistorResistance: int) (voltageOut:double) =
        (voltageIn - voltageOut) * double(resistorResistance) / (voltageOut)
        

    let readInput (port:string) (baud:int) =
        lock monitor (fun () -> 
            use portStream = new SerialPort(port, baud)
            portStream.ReadTimeout <- 1500
            portStream.RtsEnable <- true
            portStream.Open() |> ignore
            let line = portStream.ReadLine()
            portStream.Close() |> ignore
            line.Trim()
        )

    let readProbeResistance (config:ProbeConfig) =
        readInput config.Port config.Baud
        |>calculateVoltageRead config.VoltageIn config.Resolution
        |> calculateProbeResistance config.VoltageIn config.ResistorValue

    let steinhartCalculation (constantA:double) (constantB:double) (constantC:double) (resistance:double) =
        let aMultiplier = pown 10.0 -3 |> double
        let bMultiplier = pown 10.0 -4 |> double
        let cMultiplier = pown 10.0 -7 |> double
        let inverse = (constantA * aMultiplier) + 
                      (constantB * bMultiplier * (log resistance) ) +
                      (constantC * cMultiplier * (pown (log resistance) 3))
        double(1.0 / inverse - 273.15)


    let buildResponseElement (config: ProbeConfig) =
        let probeResistance = readProbeResistance config
        let celsTemp = probeResistance 
                        |> steinhartCalculation config.SteinhardA config.SteinhardB config.SteinhardC
        let fahrTemp = celsTemp * 9.0 / 5.0 + 32.0
        {
            ProbeResistance=probeResistance;
            CelsiusTemperature=celsTemp;
            FahrenheitTemperature=fahrTemp;
        }


    let requestDelegate(context: HttpContext) =
        let probeId = "Probe0"
        let config:ProbeConfig = {
                                    VoltageIn=5.0; 
                                    Resolution=1024; 
                                    ResistorValue=100000; 
                                    Port="/dev/ttyACM0"; 
                                    Baud=9600; 
                                    SteinhardA=(-1.197580389);
                                    SteinhardB=(5.086328360);
                                    SteinhardC=(-9.230709475);
                                    }
        buildResponseElement config
        |> (fun r -> Map.ofArray[|(probeId,r)|])
        |> context.Response.WriteAsJsonAsync

