namespace VtNetCore.UWP.App.ViewModel.AuthenticationProfilesViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class Owner : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid Id
        {
            get
            {
                if (_item is Model.Tennant)
                    return (_item as Model.Tennant).Id;

                if (_item is Model.Site)
                    return (_item as Model.Site).Id;

                if (_item is Model.Device)
                        return (_item as Model.Device).Id;

                throw new Exception("Item is of an invalid type");
            }
        }

        public string Name
        {
            get
            {
                if (_item is Model.Tennant)
                    return (_item as Model.Tennant).Name;

                if (_item is Model.Site)
                    return (_item as Model.Site).Name;

                if (_item is Model.Device)
                    return (_item as Model.Device).Name;

                throw new Exception("Item is of an invalid type");
            }
        }

        private INotifyPropertyChanged _item;
        public INotifyPropertyChanged Item
        {
            get => _item;

            set
            {
                if (_item != null)
                    _item.PropertyChanged -= ItemPropertyChanged;

                _item = value;

                if (_item != null)
                    _item.PropertyChanged += ItemPropertyChanged;
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Id":
                case "Name":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
                    break;

                default:
                    break;
            }
        }

        private ObservableCollection<Model.AuthenticationProfile> _authenticationProfiles;
        public ObservableCollection<Model.AuthenticationProfile> AuthenticationProfiles
        {
            get => _authenticationProfiles;
            set
            {
                //if (_authenticationProfiles != null)
                //    _authenticationProfiles.CollectionChanged -= AuthenticationProfilesChanged;

                _authenticationProfiles = value;

                //if (_authenticationProfiles != null)
                //    _authenticationProfiles.CollectionChanged += AuthenticationProfilesChanged;
            }
        }

        private void AuthenticationProfilesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AuthenticationProfiles"));
        }
    }
}
