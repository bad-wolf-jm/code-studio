
-- Launch OTDR acquisition using connected device and connected fiber
function real_otdr_acquisition()
    local connected_devices = GetConnectedDevices { Wait = true }
    local requested_device = connected_devices[0]
    print(requested_device.DeviceHandle.Address)
    return OtdrAcquisition {
        DeviceHandle = requested_device,
        LaunchFiberLength = 100,
        Configuration = {
            Wavelengths = { 1650 },
            Duration = 5,
            PulseWidth = 30,
            Range = 5000,
            Mode = AcquisitionMode.Time,
            UseHighResolution = false
        }
    }
end