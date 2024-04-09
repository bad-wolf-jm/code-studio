-- Launch Olm acquisition on simulated link with diven device
function simulated_olm_acquisition(link_description, base_olm_measurement)
    local device_address = "path_or_device_address"
    local device_handle = PhysicalDevice { Address = device_address }

    return = OlmAcquisition {
        DeviceHandle = device_handle,
        LinkDescription = link_description,
        BaseOlmMeasurement = base_olm_measurement,
        LaunchFiberLength = 100,
        MeasurementType = MeasurementType.LinkMapper,
        ProgressCallback = function(p)
            pring("Progress", p .. "%")
        end
    }
end
