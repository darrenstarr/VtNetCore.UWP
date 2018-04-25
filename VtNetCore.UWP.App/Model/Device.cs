namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;

    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private Guid _siteId;
        private string _name;
        private string _destination;
        private EAuthenticationMethod _authenticationMethod;
        private string _username;
        private string _password;
        private Guid _authenticationProfileId;
        private string _notes;
        private Guid _deviceTypeId;
        private bool _connected;

        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        public Guid SiteId
        {
            get => _siteId;
            set { PropertyChanged.ChangeAndNotify(ref _siteId, value, () => SiteId); }
        }

        public string Name
        {
            get => _name;
            set { PropertyChanged.ChangeAndNotify(ref _name, value, () => Name); }
        }

        public string Destination
        {
            get => _destination;
            set { PropertyChanged.ChangeAndNotify(ref _destination, value, () => Destination); }
        }

        public EAuthenticationMethod AuthenticationMethod
        {
            get => _authenticationMethod;
            set { PropertyChanged.ChangeAndNotify(ref _authenticationMethod, value, () => AuthenticationMethod); }
        }

        public string Username
        {
            get => _username;
            set { PropertyChanged.ChangeAndNotify(ref _username, value, () => Username); }
        }

        public string Password
        {
            get => _password;
            set { PropertyChanged.ChangeAndNotify(ref _password, value, () => Password); }
        }

        public Guid AuthenticationProfileId
        {
            get => _authenticationProfileId;
            set { PropertyChanged.ChangeAndNotify(ref _authenticationProfileId, value, () => AuthenticationProfileId); }
        }

        public string Notes
        {
            get => _notes;
            set { PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes); }
        }

        public Guid DeviceTypeId
        {
            get => _deviceTypeId;
            set { PropertyChanged.ChangeAndNotify(ref _deviceTypeId, value, () => DeviceTypeId); }
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool Connected
        {
            get => _connected;
            set { PropertyChanged.ChangeAndNotify(ref _connected, value, () => Connected); }
        }
    }
}
