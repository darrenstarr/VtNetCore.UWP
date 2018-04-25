namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Linq;
    using VtNetCore.UWP.App.JsonDatastore;

    public class ObjectStoreContext : Context
    {
        private static readonly string ObjectStoreRoot = "local:pits:of:hell";

        public ObjectStoreContext()
        {
            var tennants = ObjectStore.Current.ReadAllObjects<Tennant>(ObjectStoreRoot);
            foreach (var tennant in tennants)
                Tennants.Add(tennant);

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

            Tennants.CollectionChanged += Tennants_CollectionChanged;
            Sites.CollectionChanged += Sites_CollectionChanged;
            Devices.CollectionChanged += Devices_CollectionChanged;
            AuthenticationProfiles.CollectionChanged += AuthenticationProfiles_CollectionChanged;
            DeviceTypes.CollectionChanged += DeviceTypes_CollectionChanged;

            var localTennant = Tennants.SingleOrDefault(x => x.Name.ToLowerInvariant() == "{local}");
            if (localTennant == null)
            {
                localTennant = new Tennant
                {
                    Id = Guid.NewGuid(),
                    Name = "{Local}",
                    Notes = "Local tennant"
                };

                Tennants.Add(localTennant);
            }

            var localSite = Sites.SingleOrDefault(x => x.Name.ToLowerInvariant() == "{local site}");
            if (localSite == null)
            {
                localSite = new Site
                {
                    Id = Guid.NewGuid(),
                    TennantId = localTennant.Id,
                    Name = "{Local Site}",
                    Notes = "Local site"
                };

                Sites.Add(localSite);
            }
        }

        private void Tennants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Tennant>())
                        ObjectStore.Current.WriteObject(ObjectStoreRoot, item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Tennant>())
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
    }
}
