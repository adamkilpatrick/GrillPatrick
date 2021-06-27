module Tests

open System
open Xunit


[<Fact>]
let ``Steinhard doesn't produce infinity for known constants`` () =
    let s = SensorService.ArduinoSerial.steinhartCalculation 2.108 0.797 6.535 10000.0
    Assert.Equal(25.0,s,1)
