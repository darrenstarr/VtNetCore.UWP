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

            foreach (var tennant in Model.Context.Current.Tennants)
                AddTennant(tennant);

            foreach (var site in Model.Context.Current.Sites)
                AddSite(site);

            foreach (var device in Model.Context.Current.Devices)
                AddDevice(device);

            Model.Context.Current.Tennants.CollectionChanged += Tennants_CollectionChanged;
            Model.Context.Current.Sites.CollectionChanged += Sites_CollectionChanged;
            Model.Context.Current.Devices.CollectionChanged += Devices_CollectionChanged;
            Model.Context.Current.AuthenticationProfiles.CollectionChanged += AuthenticationProfiles_CollectionChanged;
        }

        public void Dispose()
        {
            Model.Context.Current.Tennants.CollectionChanged -= Tennants_CollectionChanged;
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
                .Single(x => typeof(Model.Device)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Device).Id == item.Id);

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
                .Single(x => typeof(Model.Site)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Site).Id == item.Id);

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

        private void Tennants_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<Model.Tennant>())
                        AddTennant(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<Model.Tennant>().ToList())
                        RemoveTennant(item);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    ClearTennants();
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

        private void ClearTennants()
        {
            foreach (var item in this.OfType<Model.Tennant>().ToList())
                RemoveTennant(item);
        }

        private void RemoveTennant(Model.Tennant item)
        {
            var toRemove = this
                .Single(x => typeof(Model.Tennant)
                .IsAssignableFrom(x.Item.GetType()) && (x.Item as Model.Tennant).Id == item.Id);

            Remove(toRemove);
        }

        private void AddTennant(Model.Tennant item)
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
    }
}
