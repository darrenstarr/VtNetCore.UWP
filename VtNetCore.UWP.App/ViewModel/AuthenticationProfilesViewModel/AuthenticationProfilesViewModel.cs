namespace VtNetCore.UWP.App.ViewModel.AuthenticationProfilesViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class AuthenticationProfilesViewModel : ObservableCollection<Owner>, IDisposable
    {
        public Owner GlobalOwner = new Owner
        {
            AuthenticationProfiles =
                new ObservableCollection<Model.AuthenticationProfile>
                (
                    Model.Context.Current.AuthenticationProfiles.Where(x => x.ParentId == Guid.Empty)
                )
        };

        public AuthenticationProfilesViewModel()
        {
            Add(GlobalOwner);

            foreach (var tenant in Model.Context.Current.Tenants)
                AddTenant(tenant);

            foreach (var site in Model.Context.Current.Sites)
                AddSite(site);

            foreach (var device in Model.Context.Current.Devices)
                AddDevice(device);

            Model.Context.Current.Tenants.CollectionChanged += Tenants_CollectionChanged;
            Model.Context.Current.Sites.CollectionChanged += Sites_CollectionChanged;
            Model.Context.Current.Devices.CollectionChanged += Devices_CollectionChanged;
            Model.Context.Current.AuthenticationProfiles.CollectionChanged += AuthenticationProfiles_CollectionChanged;
        }

        public void Dispose()
        {
            Model.Context.Current.Tenants.CollectionChanged -= Tenants_CollectionChanged;
            Model.Context.Current.Sites.CollectionChanged -= Sites_CollectionChanged;
            Model.Context.Current.Devices.CollectionChanged -= Devices_CollectionChanged;
            Model.Context.Current.AuthenticationProfiles.CollectionChanged -= AuthenticationProfiles_CollectionChanged;
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Model.Device>())
                        AddDevice(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Model.Device>().ToList())
                        RemoveDevice(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearDevices();
                    break;
            }
        }

        private void ClearDevices()
        {
            foreach (var item in this.OfType<Model.Device>().ToList())
                RemoveDevice(item);
        }

        private void RemoveDevice(Model.Device item)
        {
            var toRemove = this
                .SingleOrDefault(x => typeof(Model.Device)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Device).Id == item.Id);

            // The device wasn't found. This is normal since the device removal can be recursive.
            if (toRemove == null)
                return;

            Remove(toRemove);
        }

        private void AddDevice(Model.Device item)
        {
            Add(
                new Owner
                {
                    Item = item,
                    AuthenticationProfiles = new ObservableCollection<Model.AuthenticationProfile>
                    (
                        Model.Context.Current.AuthenticationProfiles.Where(x => x.ParentId == item.Id)
                    )
                }
                );
        }

        private void Sites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Model.Site>())
                        AddSite(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Model.Site>().ToList())
                        RemoveSite(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearSites();
                    break;
            }
        }

        private void ClearSites()
        {
            foreach (var item in this.OfType<Model.Site>().ToList())
                RemoveSite(item);
        }

        private void RemoveSite(Model.Site item)
        {
            var toRemove = this
                .SingleOrDefault(x => typeof(Model.Site)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Site).Id == item.Id);

            // The site wasn't found. This is normal since the site removal can be recursive.
            if (toRemove == null)
                return;

            Remove(toRemove);
        }

        private void AddSite(Model.Site item)
        {
            Add(
                new Owner
                {
                    Item = item,
                    AuthenticationProfiles = new ObservableCollection<Model.AuthenticationProfile>
                    (
                        Model.Context.Current.AuthenticationProfiles.Where(x => x.ParentId == item.Id)
                    )
                }
                );
        }

        private void Tenants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Model.Tenant>())
                        AddTenant(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Model.Tenant>().ToList())
                        RemoveTenant(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearTenants();
                    break;
            }
        }

        private void AuthenticationProfiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach(var item in e.NewItems.Cast<Model.AuthenticationProfile>())
                    {
                        var owner = this.Single(x => x.Id == item.ParentId);
                        owner.AuthenticationProfiles.Add(item);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Model.AuthenticationProfile>().ToList())
                    {
                        var owner = this.Single(x => x.Id == item.ParentId);
                        owner.AuthenticationProfiles.Remove(item);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ClearTenants()
        {
            foreach (var item in this.OfType<Model.Tenant>().ToList())
                RemoveTenant(item);
        }

        private void RemoveTenant(Model.Tenant item)
        {
            var toRemove = this
                .SingleOrDefault(x => typeof(Model.Tenant)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Tenant).Id == item.Id);

            // The tenant wasn't found. This is normal since the tenant removal can be recursive.
            if (toRemove == null)
                return;

            Remove(toRemove);
        }

        private void AddTenant(Model.Tenant item)
        {
            Add(
                new Owner
                {
                    Item = item,
                    AuthenticationProfiles = new ObservableCollection<Model.AuthenticationProfile>
                    (
                        Model.Context.Current.AuthenticationProfiles.Where(x => x.ParentId == item.Id)
                    )
                }
                );
        }

        public void SaveChanges(Model.AuthenticationProfile profile)
        {
            var oldOwner = this.SingleOrDefault(x => x.AuthenticationProfiles.Contains(profile));
            if(oldOwner != null)
            {
                if (oldOwner.Id != profile.ParentId)
                {
                    oldOwner.AuthenticationProfiles.Remove(profile);

                    var newOwner = this.SingleOrDefault(x => x.Id == profile.ParentId);
                    newOwner.AuthenticationProfiles.Add(profile);
                }
            }
        }
    }
}
