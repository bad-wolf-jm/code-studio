function get_otdr_data(link_description, wavelengths)
    local root = "D:\\Work\\Git\\OTDR\\OtdrInstrument\\Metrino.Otdr.Instrument\\SimulatorConfigs"
    local device_handle = PhysicalDevice { Address = "file://" .. root .. "/" .. "M-715D-SM8-OPM2-EA.bin" }

    return OtdrAcquisition {
        DeviceHandle = device_handle,
        LinkDescription = link_description,
        LaunchFiberLength = 0,
        Configuration = { 
            Wavelengths = wavelengths, 
            Duration = 60, 
            PulseWidth = 30, 
            Range = 15000, 
            Mode = AcquisitionMode.Time, 
            UseHighResolution = false 
        }
    }
end

function perform_axs_analysis(base_olm_measurement, otdr_data)
    otdr_multi_wavelength_analysis = Metrino.Olm.SignalProcessing.OtdrMultiWavelengthAnalysis(base_olm_measurement);
    otdr_multi_wavelength_analysis:Analyse(MakeSinglePulseTraceCollection(otdr_data), 1550e-9);

    return otdr_multi_wavelength_analysis.OlmMeasurement
end