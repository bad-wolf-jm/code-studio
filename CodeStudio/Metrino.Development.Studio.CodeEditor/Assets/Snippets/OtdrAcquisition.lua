-- Simple multi-wavelength OTDR acquisition
function get_otdr_data(link_description, wavelengths)
    local device_address = "path_or_device_address"
    local device_handle = PhysicalDevice { Address = device_address }
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
