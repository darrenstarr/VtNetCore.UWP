namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class Site : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private Guid _tenantId;
        private string _name;
        private string _location;
        private string _notes;

        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        public Guid TenantId
        {
            get => _tenantId;
            set { PropertyChanged.ChangeAndNotify(ref _tenantId, value, () => TenantId); }
        }

        public string Name
        {
            get => _name;
            set { PropertyChanged.ChangeAndNotify(ref _name, value, () => Name); }
        }

        public string Location
        {
            get => _location;
            set { PropertyChanged.ChangeAndNotify(ref _location, value, () => Location); }
        }

        public string Notes
        {
            get => _notes;
            set { PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes); }
        }
    }
}
