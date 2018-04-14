namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using VtNetCore.UWP.App.JsonDatastore;

    public class Context
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
        /// Provides access to a list of tennants that can be observed for changes
        /// </summary>
        public ObservableCollection<Tennant> Tennants { get; set; } = new ObservableCollection<Tennant>();

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
        /// Removes a tennant as well as the sites, devices and authentication profiles associated to it.
        /// </summary>
        /// <param name="tennant">The site to remove</param>
        public void RemoveTennant(Tennant tennant)
        {
            var sites = Sites.Where(x => x.TennantId == tennant.Id).ToList();
            foreach (var site in sites)
                RemoveSite(site);

            Tennants.Remove(tennant);
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
    }
}
