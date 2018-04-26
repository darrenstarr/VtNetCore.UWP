namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A model representing classes of devices. For example Router, Switch or Workstation
    /// </summary>
    /// <remarks>
    /// In this version, the classes table is a static entry, but in the future there will be code in the system to support
    /// expanding it through the user interface.
    /// </remarks>
    public class DeviceClass
    {
        /// <summary>
        /// Layer-3 routing device
        /// </summary>
        public static readonly Guid Router = Guid.Parse("0004d396-8ff8-4a79-975b-ec1f464e3036");

        /// <summary>
        /// Layer-2 only workgroup switch
        /// </summary>
        public static readonly Guid Layer2Switch = Guid.Parse("195fbf19-2afc-4af2-9754-e94788cf743f");

        /// <summary>
        /// Multilayer switch
        /// </summary>
        public static readonly Guid MultiLayerSwitch = Guid.Parse("3eb5704b-6310-41dd-b2af-9ec5b73179ea");

        /// <summary>
        /// A generic workstation
        /// </summary>
        public static readonly Guid Workstation = Guid.Parse("55b5a361-7829-42e1-afc8-b55d9ee5506c");

        /// <summary>
        /// A static list of device classes with fixed Guids
        /// </summary>
        public static readonly List<DeviceClass> Classes = new List<DeviceClass>
        {
            new DeviceClass
            {
                Id = Router,
                Name = "Router"
            },
            new DeviceClass
            {
                Id = Layer2Switch,
                Name = "Layer-2 Switch"
            },
            new DeviceClass
            {
                Id = MultiLayerSwitch,
                Name = "Multi-layer Switch"
            },
            new DeviceClass
            {
                Id = Workstation,
                Name = "Workstation"
            }
        };

        /// <summary>
        /// The unique identifier or record key for device type.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name given to this device type.
        /// </summary>
        public string Name { get; set; }
    }
}
