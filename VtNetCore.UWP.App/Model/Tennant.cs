namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class Tennant : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private string _name;
        private string _notes;

        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        public string Name
        {
            get => _name;
            set { PropertyChanged.ChangeAndNotify(ref _name, value, () => Name); }
        }

        public string Notes
        {
            get => _notes;
            set { PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes); }
        }
    }
}
