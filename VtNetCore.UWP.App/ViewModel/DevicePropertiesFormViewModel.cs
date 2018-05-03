namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
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

        private bool _isValid;
        private bool _isDirty;

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

        public bool IsDirty
        {
            get => _isDirty;
            set => PropertyChanged.ChangeAndNotify(ref _isDirty, value, () => IsDirty);
        }

        public bool IsValid
        {
            get => _isValid;
            set => PropertyChanged.ChangeAndNotify(ref _isValid, value, () => IsValid);
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

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                Device = new Model.Device
                {
                    Id = Guid.NewGuid(),
                    SiteId = SiteId,
                    Name = DeviceName.TrimEnd(),
                    Destination = Destination.Trim(),
                    DeviceTypeId = DeviceTypeId,
                    AuthenticationMethod = DeviceAuthenticationMethod,
                    AuthenticationProfileId = AuthenticationProfileId,
                    Username = Username.TrimEnd(),
                    Password = Password.TrimEnd(),
                    Notes = Notes.TrimEnd()
                };
            }
            else
            {
                Device.Name = DeviceName.TrimEnd();
                Device.Destination = Destination.Trim();
                Device.DeviceTypeId = DeviceTypeId;
                Device.AuthenticationMethod = DeviceAuthenticationMethod;
                Device.AuthenticationProfileId = AuthenticationProfileId;
                Device.Username = Username.TrimEnd();
                Device.Password = Password.TrimEnd();
                Device.Notes = Notes.TrimEnd();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public ValidityState FieldIsValid(string name)
        {
            bool valid;
            switch(name)
            {
                case "DeviceName":
                    valid = !string.IsNullOrWhiteSpace(DeviceName);
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null && !string.IsNullOrWhiteSpace(DeviceName)
                        ) || (
                            Device != null &&
                            DeviceName.TrimEnd() != Device.Name
                        ),
                        Message = valid ? string.Empty : "Device name is empty"
                    };

                case "Destination":
                    valid = Uri.IsWellFormedUriString(Destination, UriKind.Absolute);
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null && !string.IsNullOrWhiteSpace(Destination)
                        ) || (
                            Device != null &&
                            Destination.TrimEnd() != Device.Name
                        ),
                        Message = valid ? string.Empty : "Destination is not a properly formed URI"
                    };

                case "DeviceTypeId":
                    valid = DeviceTypeId != Guid.Empty;
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null && DeviceTypeId != Guid.Empty
                        ) || (
                            Device != null &&
                            DeviceTypeId != Device.DeviceTypeId
                        ),
                        Message = valid ? string.Empty : "No device type selected"
                    };

                case "DeviceAuthenticationMethod":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                Device == null && DeviceAuthenticationMethod != Model.EAuthenticationMethod.NoAuthentication
                            ) || (
                                Device != null &&
                                DeviceAuthenticationMethod != Device.AuthenticationMethod
                            ),
                    };

                case "Username":
                    valid = !(DeviceAuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Username));
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null && !string.IsNullOrWhiteSpace(Username)
                        ) || (
                            Device != null &&
                            Username.TrimEnd() != Device.Username
                        ),
                        Message = valid ? string.Empty : "Username is empty"
                    };

                case "Password":
                    valid = !(DeviceAuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Password));
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null && !string.IsNullOrWhiteSpace(Password)
                        ) || (
                            Device != null &&
                            Password.TrimEnd() != Device.Password
                        ),
                        Message = valid ? string.Empty : "Password is empty"
                    };

                case "AuthenticationProfileId":
                    valid = DeviceAuthenticationMethod != Model.EAuthenticationMethod.AuthenticationProfile || AuthenticationProfileId != Guid.Empty;
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Device == null &&
                            DeviceAuthenticationMethod == Model.EAuthenticationMethod.AuthenticationProfile &&
                            AuthenticationProfileId != Guid.Empty
                        ) || (
                            Device != null &&
                            DeviceAuthenticationMethod == Model.EAuthenticationMethod.AuthenticationProfile &&
                            AuthenticationProfileId != Device.AuthenticationProfileId
                        ),
                        Message = valid ? string.Empty : "No authentication profile selected"
                    };

                case "Notes":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                Device == null && !string.IsNullOrWhiteSpace(Notes)
                            ) || (
                                Device != null &&
                                Notes.TrimEnd() != Device.Notes
                            ),
                    };
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

            result.Add(FieldIsValid("DeviceName"));
            result.Add(FieldIsValid("Destination"));
            result.Add(FieldIsValid("DeviceTypeId"));
            result.Add(FieldIsValid("DeviceAuthenticationMethod"));
            result.Add(FieldIsValid("Username"));
            result.Add(FieldIsValid("Password"));
            result.Add(FieldIsValid("AuthenticationProfileId"));
            result.Add(FieldIsValid("Notes"));

            IsValid = result.Count(x => !x.IsValid) == 0;
            IsDirty = result.Count(x => x.IsChanged) > 0;

            return result;
        }
    }
}
