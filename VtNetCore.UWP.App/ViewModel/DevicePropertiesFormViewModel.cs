namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class DevicePropertiesFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.Device _device;

        private string _deviceName;
        private Guid _siteId = Guid.Empty;
        private string _destination;
        private Guid _deviceTypeId = Guid.Empty;
        private Model.EAuthenticationMethod _deviceAuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite;
        private string _username;
        private string _password;
        private Guid _authenticationProfileId = Guid.Empty;
        private string _notes;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation);
        }

        public Model.Device Device
        {
            get => _device;
            set
            {
                _device = value;
                DeviceChanged();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Device"));
            }
        }

        [Required]
        public string DeviceName
        {
            get => _deviceName;
            set => PropertyChanged.ChangeAndNotify(ref _deviceName, value, () => DeviceName);
        }

        public Guid SiteId
        {
            get => _siteId;
            set => PropertyChanged.ChangeAndNotify(ref _siteId, value, () => SiteId);
        }

        public string Destination
        {
            get => _destination;
            set => PropertyChanged.ChangeAndNotify(ref _destination, value, () => Destination);
        }

        public Guid DeviceTypeId
        {
            get => _deviceTypeId;
            set => PropertyChanged.ChangeAndNotify(ref _deviceTypeId, value, () => DeviceTypeId);
        }

        public Model.EAuthenticationMethod DeviceAuthenticationMethod
        {
            get => _deviceAuthenticationMethod;
            set => PropertyChanged.ChangeAndNotify(ref _deviceAuthenticationMethod, value, () => DeviceAuthenticationMethod);
        }

        public string Username
        {
            get => _username;
            set => PropertyChanged.ChangeAndNotify(ref _username, value, () => Username);
        }

        public string Password
        {
            get => _password;
            set => PropertyChanged.ChangeAndNotify(ref _password, value, () => Password);
        }

        public  Guid AuthenticationProfileId
        {
            get => _authenticationProfileId;
            set => PropertyChanged.ChangeAndNotify(ref _authenticationProfileId, value, () => AuthenticationProfileId);
        }

        public string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public void Clear()
        {
            DeviceName = string.Empty;
            SiteId = Guid.Empty;
            Destination = string.Empty;
            DeviceTypeId = Guid.Empty;
            DeviceAuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication;
            Username = string.Empty;
            Password = string.Empty;
            AuthenticationProfileId = Guid.Empty;
            Notes = string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        private void DeviceChanged()
        {
            if (Device == null)
            {
                Clear();
                return;
            }

            DeviceName = Device.Name.BlankIfNull();
            SiteId = Device.SiteId;
            Destination = Device.Destination.BlankIfNull();
            DeviceTypeId = Device.DeviceTypeId;
            DeviceAuthenticationMethod = Device.AuthenticationMethod;
            Username = Device.Username.BlankIfNull();
            Password = Device.Password.BlankIfNull(); ;
            AuthenticationProfileId = Device.AuthenticationProfileId;
            Notes = Device.Notes.BlankIfNull();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public bool IsDirty
        {
            get
            {
                return
                    Device == null ||
                    !(
                        DeviceName == Device.Name.BlankIfNull() &&
                        SiteId == Device.SiteId &&
                        Destination == Device.Destination.BlankIfNull() &&
                        DeviceTypeId == Device.DeviceTypeId &&
                        DeviceAuthenticationMethod == Device.AuthenticationMethod &&
                        Username == Device.Username.BlankIfNull() &&
                        Password == Device.Password.BlankIfNull() &&
                        AuthenticationProfileId == Device.AuthenticationProfileId &&
                        Notes == Device.Notes.BlankIfNull()
                    );
            }
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                Device = new Model.Device
                {
                    Id = Guid.NewGuid(),
                    SiteId = SiteId,
                    Name = DeviceName,
                    Destination = Destination,
                    DeviceTypeId = DeviceTypeId,
                    AuthenticationMethod = DeviceAuthenticationMethod,
                    AuthenticationProfileId = AuthenticationProfileId,
                    Username = Username,
                    Password = Password,
                    Notes = Notes
                };
            }
            else
            {
                Device.Name = DeviceName;
                Device.Destination = Destination;
                Device.DeviceTypeId = DeviceTypeId;
                Device.AuthenticationMethod = DeviceAuthenticationMethod;
                Device.AuthenticationProfileId = AuthenticationProfileId;
                Device.Username = Username;
                Device.Password = Password;
                Device.Notes = Notes;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public ValidityState IsValid(string name)
        {
            switch(name)
            {
                case "DeviceName":
                    if (string.IsNullOrWhiteSpace(DeviceName))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Device name is empty"
                        };
                    break;

                case "Destination":
                    if (!Uri.IsWellFormedUriString(Destination, UriKind.Absolute))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Destination is not a properly formed URI"
                        };
                    break;

                case "DeviceTypeId":
                    if (DeviceTypeId == Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "No device type selected"
                        };
                    break;

                case "DeviceAuthenticationMethod":
                    break;

                case "Username":
                    if (DeviceAuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword &&  string.IsNullOrWhiteSpace(Username))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Username is empty"
                        };
                    break;

                case "Password":
                    if (DeviceAuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Password))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Password is empty"
                        };
                    break;

                case "AuthenticationProfileId":
                    if (DeviceAuthenticationMethod == Model.EAuthenticationMethod.AuthenticationProfile && AuthenticationProfileId == Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "No authentication profile selected"
                        };
                    break;

                case "Notes":
                    break;
            }

            return
                new ValidityState
                {
                    Name = name,
                    IsValid = true
                };
        }

        public IEnumerable<ValidityState> Validate()
        {
            List<ValidityState> result = new List<ValidityState>();

            // TODO : Validate Id ?

            if (Operation == FormOperation.Add && SiteId != Guid.Empty)
            {
                result.Add(
                    new ValidityState
                    {
                        Name = "Operation",
                        IsValid = false,
                        Message = "SiteId must be empty when adding"
                    }
                    );

                result.Add(
                    new ValidityState
                    {
                        Name = "SiteId",
                        IsValid = false,
                        Message = "SiteId must be empty when adding"
                    }
                    );
            }
            else if(Operation == FormOperation.Edit && SiteId == Guid.Empty)
            {
                result.Add(
                    new ValidityState
                    {
                        Name = "Operation",
                        IsValid = false,
                        Message = "SiteId must not be empty when editing"
                    }
                    );

                result.Add(
                    new ValidityState
                    {
                        Name = "SiteId",
                        IsValid = false,
                        Message = "SiteId must not be empty when editing"
                    }
                    );
            }
            else
            {
                result.Add(
                    new ValidityState
                    {
                        Name = "Operation",
                        IsValid = true,
                    }
                    );

                result.Add(
                    new ValidityState
                    {
                        Name = "SiteId",
                        IsValid = true,
                    }
                    );
            }

            result.Add(IsValid("DeviceName"));
            result.Add(IsValid("Destination"));
            result.Add(IsValid("DeviceTypeId"));
            result.Add(IsValid("DeviceAuthenticationMethod"));
            result.Add(IsValid("Username"));
            result.Add(IsValid("Password"));
            result.Add(IsValid("AuthenticationProfileId"));
            result.Add(IsValid("Notes"));

            return result;
        }
    }
}
