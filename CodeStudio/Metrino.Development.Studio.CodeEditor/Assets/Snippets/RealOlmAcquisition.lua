
function olm_acquisition_real_device_and_fiber()
    local connected_devices = GetConnectedDevices { Wait = true }
    local requested_device = connected_devices[0]
    print(requested_device.DeviceHandle.Address)
    local olm_data_1 = OlmAcquisition {
        DeviceHandle = requested_device,
        LaunchFiberLength = 100,
        MeasurementType = MeasurementType.LinkMapper
    }
end