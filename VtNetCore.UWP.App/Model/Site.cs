namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Site : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private Guid _tennantId;
        private string _name;
        private string _location;
        private string _notes;

        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        public Guid TennantId
        {
            get => _tennantId;
            set { PropertyChanged.ChangeAndNotify(ref _tennantId, value, () => TennantId); }
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
