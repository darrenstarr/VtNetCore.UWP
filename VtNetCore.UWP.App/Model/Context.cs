namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class Context
    {
        private static readonly Lazy<Context> lazy =
            new Lazy<Context>(
                () => new ObjectStoreContext()
            );

        public static Context Current { get { return lazy.Value; } }

        protected Context()
        {
        }

        /// <summary>
        /// Provides access to a list of tenants that can be observed for changes
        /// </summary>
        public ObservableCollection<Tenant> Tenants { get; set; } = new ObservableCollection<Tenant>();

        /// <summary>
        /// Provides access to a list of sites that can be observed for changes
        /// </summary>
        public ObservableCollection<Site> Sites { get; set; } = new ObservableCollection<Site>();

        /// <summary>
        /// Provides access to a list of devices that can be observed for changes
        /// </summary>
        public ObservableCollection<Device> Devices { get; set; } = new ObservableCollection<Device>();

        /// <summary>
        /// Provides access to a list of authentication profiles that can be observed for changes
        /// </summary>
        public ObservableCollection<AuthenticationProfile> AuthenticationProfiles { get; set; } = new ObservableCollection<AuthenticationProfile>();

        /// <summary>
        /// Provides access to a list of device types that can be observed for changes
        /// </summary>
        public ObservableCollection<DeviceType> DeviceTypes { get; set; } = new ObservableCollection<DeviceType>();

        /// <summary>
        /// Removes a tenant as well as the sites, devices and authentication profiles associated to it.
        /// </summary>
        /// <param name="tenant">The site to remove</param>
        public void RemoveTenant(Tenant tenant)
        {
            var sites = Sites.Where(x => x.TenantId == tenant.Id).ToList();
            foreach (var site in sites)
                RemoveSite(site);

            Tenants.Remove(tenant);
        }

        /// <summary>
        /// Removes a site as well as the devices and authentication profiles associated to it.
        /// </summary>
        /// <param name="site">The site to remove</param>
        public void RemoveSite(Site site)
        {
            var devices = Devices.Where(x => x.SiteId == site.Id).ToList();
            foreach (var device in devices)
                RemoveDevice(device);

            Sites.Remove(site);
        }

        /// <summary>
        /// Removes a device as well as the authentication profiles associated to it.
        /// </summary>
        /// <param name="device">The device to remove</param>
        public void RemoveDevice(Device device)
        {
            Devices.Remove(device);
        }

        /// <summary>
        /// Removes the device type specified
        /// </summary>
        /// <param name="deviceType"></param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the device type is known to be in use by other records
        /// </exception>
        public void RemoveDeviceType(DeviceType deviceType)
        {
            if (Devices.Count(x => x.DeviceTypeId == deviceType.Id) > 0)
                throw new InvalidOperationException("The given device type " + deviceType.Name + " is currently in use and cannot be removed");

            DeviceTypes.Remove(deviceType);
        }

        public abstract Task SaveChanges<T>(T item);
    }
}
