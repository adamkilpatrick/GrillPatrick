namespace CookSessionService
module Metrics =
    let tempGauge = Prometheus.Metrics.CreateGauge("temperatureGauge", "Temperature", [|"sessionId"|])
    let targetGauge = Prometheus.Metrics.CreateGauge("targetTempGauge", "TargetTemperature", [|"sessionId"|])

