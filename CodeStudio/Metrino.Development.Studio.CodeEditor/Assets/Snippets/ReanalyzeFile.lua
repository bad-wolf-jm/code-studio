
local olm_measurement = LoadOlmFile("Fiber1.iolm")
ApplyOlmAnalysis {
    OlmMeasurement = olm_measurement.OlmMeasures[0],
    Wavelengths = { 1310, 1550 },
    SkipValidation = false,
    KeepEchoes = false,
    DoNotMergeEvents = false 
}