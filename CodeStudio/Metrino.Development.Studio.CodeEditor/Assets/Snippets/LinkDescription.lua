local test_link_0 = LinkDescription {
    FiberCode = FiberCode.B,
    OpticalElements = {
        OpticalElement { 
            Type = OpticalElementType.Fiber, 
            Length = 8500, 
            PhysicalEvents = {
                PhysicalEvent { Position = 0, Loss = 0.15, Reflectance = -100 },   -- Connector @    0m
                PhysicalEvent { Position = 1000, Loss = 1, Reflectance = -100 },   -- Splice    @ 1000m
                PhysicalEvent { Position = 2500, Loss = 0.15, Reflectance = -65 }, -- Connector @ 2500m
                PhysicalEvent { Position = 3500, Loss = 0.15, Reflectance = -70 }, -- Connector @ 3500m
                PhysicalEvent { Position = 4500, Loss = -1, Reflectance = -100 },  -- Splice    @ 4500m
                PhysicalEvent { Position = 6000, Loss = 0.5, Reflectance = -100 }, -- Splice    @ 6000m
                PhysicalEvent { Position = 7000, Loss = 0.15, Reflectance = -60 }, -- Connector @ 7000m
                PhysicalEvent { Position = 8500, Loss = 0.15, Reflectance = -70 }, -- Connector @ 8500m
            } 
        }
    }
}
