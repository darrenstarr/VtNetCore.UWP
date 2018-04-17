namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;

    public class AuthenticationProfile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private Guid _parentId;
        private string _name;
        private string _username;
        private string _password;
        private string _notes;

        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        public Guid ParentId
        {
            get => _parentId;
            set { PropertyChanged.ChangeAndNotify(ref _parentId, value, () => ParentId); }
        }

        public string Name
        {
            get => _name;
            set { PropertyChanged.ChangeAndNotify(ref _name, value, () => Name); }
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

        public string Notes
        {
            get => _notes;
            set { PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes); }
        }
    }
}
