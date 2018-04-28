namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using VtNetCore.UWP.App.JsonDatastore;

    public class ObjectStoreContext : Context
    {
        private static readonly string ObjectStoreRoot = "local:pits:of:hell";

        public ObjectStoreContext()
        {
            var tenants = ObjectStore.Current.ReadAllObjects<Tenant>(ObjectStoreRoot);
            foreach (var tenant in tenants)
                Tenants.Add(tenant);

            var sites = ObjectStore.Current.ReadAllObjects<Site>(ObjectStoreRoot);
            foreach (var site in sites)
                Sites.Add(site);

            var devices = ObjectStore.Current.ReadAllObjects<Device>(ObjectStoreRoot);
            foreach (var device in devices)
                Devices.Add(device);

            var authenticationProfiles = ObjectStore.Current.ReadAllObjects<AuthenticationProfile>(ObjectStoreRoot);
            foreach (var authenticationProfile in authenticationProfiles)
                AuthenticationProfiles.Add(authenticationProfile);

            var deviceTypes = ObjectStore.Current.ReadAllObjects<DeviceType>(ObjectStoreRoot);
            foreach (var deviceType in deviceTypes)
                DeviceTypes.Add(deviceType);

            var unknownDeviceType = DeviceTypes.SingleOrDefault(x => x.Id == Guid.Empty);
            if (unknownDeviceType == null)
            {
                unknownDeviceType = new DeviceType
                {
                    Id = Guid.Empty,
                    Name = "{Not set}",
                    Notes = "This device type is reserved for items which have no device type set"
                };

                DeviceTypes.Add(unknownDeviceType);
            }

            Tenants.CollectionChanged += Tenants_CollectionChanged;
            Sites.CollectionChanged += Sites_CollectionChanged;
            Devices.CollectionChanged += Devices_CollectionChanged;
            AuthenticationProfiles.CollectionChanged += AuthenticationProfiles_CollectionChanged;
            DeviceTypes.CollectionChanged += DeviceTypes_CollectionChanged;

            var localTenant = Tenants.SingleOrDefault(x => x.Name.ToLowerInvariant() == "{local}");
            if (localTenant == null)
            {
                localTenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "{Local}",
                    Notes = "Local tenant"
                };

                Tenants.Add(localTenant);
            }

            var localSite = Sites.SingleOrDefault(x => x.Name.ToLowerInvariant() == "{local site}");
            if (localSite == null)
            {
                localSite = new Site
                {
                    Id = Guid.NewGuid(),
                    TenantId = localTenant.Id,
                    Name = "{Local Site}",
                    Notes = "Local site"
                };

                Sites.Add(localSite);
            }
        }

        private void Tenants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Tenant>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Tenant>())
                        ObjectStore.Current.RemoveObject(ObjectStoreRoot, item);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Sites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Site>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Site>())
                        ObjectStore.Current.RemoveObject(ObjectStoreRoot, item);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Device>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Device>())
                        ObjectStore.Current.RemoveObject(ObjectStoreRoot, item);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void AuthenticationProfiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<AuthenticationProfile>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<AuthenticationProfile>())
                        ObjectStore.Current.RemoveObject(ObjectStoreRoot, item);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void DeviceTypes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<DeviceType>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<DeviceType>())
                        ObjectStore.Current.RemoveObject(ObjectStoreRoot, item);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public async override Task SaveChanges<T>(T item)
        {
            await ObjectStore.Current.WriteObjectAsync (ObjectStoreRoot, item);
        }
    }
}
