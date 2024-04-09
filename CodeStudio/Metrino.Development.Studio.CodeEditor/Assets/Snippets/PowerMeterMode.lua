
function power_meter_mode()
    local connected_devices = GetConnectedDevices { Wait = true }
    local requested_device = connected_devices[0]

    PowerMeterMode {
        DeviceHandle = requested_device,
        Enabled = true,
        Callback = function(power)
            WriteValue("Power", power .. " dB")
        end
    }
    Sleep(35000)
    PowerMeterMode { DeviceHandle = requested_device, Enabled = false }
end
