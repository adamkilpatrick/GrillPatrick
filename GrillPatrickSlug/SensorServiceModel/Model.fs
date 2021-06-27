

namespace SensorServiceModel

module Model =
    type SensorServiceResponseElement = {FahrenheitTemperature:double; CelsiusTemperature:double; ProbeResistance:double}

    type SensorServiceResponse = Map<string, SensorServiceResponseElement>

    let deSerializeResponse (sensorServiceResponseString: string) = 
        System.Text.Json.JsonSerializer.Deserialize<SensorServiceResponse> sensorServiceResponseString
