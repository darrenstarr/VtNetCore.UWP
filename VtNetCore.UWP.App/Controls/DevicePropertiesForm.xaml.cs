namespace VtNetCore.UWP.App.Controls
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class DevicePropertiesForm : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class DeviceChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Device Device { get; set; }
        };

        public event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;

        private AdvancedCollectionView AuthenticationProfiles { get; } = new AdvancedCollectionView(Model.Context.Current.AuthenticationProfiles);

        private AdvancedCollectionView DeviceTypes { get; } = new AdvancedCollectionView(Model.Context.Current.DeviceTypes);

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
            set => PropertyChanged.ChangeAndNotify(ref _device, value, () => Device, true);
        }

        private string DeviceName
        {
            get => _deviceName;
            set => PropertyChanged.ChangeAndNotify(ref _deviceName, value, () => DeviceName);
        }

        public Guid SiteId
        {
            get => _siteId;
            set => PropertyChanged.ChangeAndNotify(ref _siteId, value, () => SiteId);
        }

        private string Destination
        {
            get => _destination;
            set => PropertyChanged.ChangeAndNotify(ref _destination, value, () => Destination);
        }

        private Guid DeviceTypeId
        {
            get => _deviceTypeId;
            set => PropertyChanged.ChangeAndNotify(ref _deviceTypeId, value, () => DeviceTypeId);
        }

        private Model.EAuthenticationMethod DeviceAuthenticationMethod
        {
            get => _deviceAuthenticationMethod;
            set => PropertyChanged.ChangeAndNotify(ref _deviceAuthenticationMethod, value, () => DeviceAuthenticationMethod);
        }

        private string Username
        {
            get => _username;
            set => PropertyChanged.ChangeAndNotify(ref _username, value, () => Username);
        }

        private string Password
        {
            get => _password;
            set => PropertyChanged.ChangeAndNotify(ref _password, value, () => Password);
        }

        private Guid AuthenticationProfileId
        {
            get => _authenticationProfileId;
            set => PropertyChanged.ChangeAndNotify(ref _authenticationProfileId, value, () => AuthenticationProfileId);
        }

        private string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public DevicePropertiesForm()
        {
            InitializeComponent();

            PropertyChanged += DevicePropertiesForm_PropertyChanged;
        }

        private void DevicePropertiesForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "Device":
                    DeviceChanged();
                    break;

                case "DeviceAuthenticationMethod":
                    AuthenticationMethodChanged();
                    break;
            }

            DoneButton.IsEnabled = IsDirty();
        }

        private void AuthenticationMethodChanged()
        {
            switch(DeviceAuthenticationMethod)
            {
                case Model.EAuthenticationMethod.UsernamePassword:
                    UsernameLabel.Visibility = Visibility.Visible;
                    UsernameField.Visibility = Visibility.Visible;
                    PasswordLabel.Visibility = Visibility.Visible;
                    PasswordField.Visibility = Visibility.Visible;
                    AuthenticationProfileLabel.Visibility = Visibility.Collapsed;
                    AuthenticationProfileField.Visibility = Visibility.Collapsed;
                    break;

                case Model.EAuthenticationMethod.AuthenticationProfile:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    AuthenticationProfileLabel.Visibility = Visibility.Visible;
                    AuthenticationProfileField.Visibility = Visibility.Visible;
                    break;

                default:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    AuthenticationProfileLabel.Visibility = Visibility.Collapsed;
                    AuthenticationProfileField.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void OperationChanged()
        {
            switch(Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add device";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit device";
                    break;
            }
        }

        public void ClearForm()
        {
            DeviceName = string.Empty;
            SiteId = Guid.Empty;
            Destination = string.Empty;
            DeviceTypeId = Guid.Empty;
            DeviceAuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite;
            Username = string.Empty;
            Password = string.Empty;
            AuthenticationProfileId = Guid.Empty;
            Notes = string.Empty;
        }

        private void DeviceChanged()
        {
            if(Device == null)
            {
                ClearForm();
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
        }

        private bool IsDirty()
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

        private void DoneButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (Operation == FormOperation.Add)
            {
                OnDeviceChanged?.Invoke(
                    this,
                    new DeviceChangedEventArgs
                    {
                        Operation = FormOperation.Add,
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
                        }
                    }
                    );

                Visibility = Visibility.Collapsed;
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

                OnDeviceChanged?.Invoke(
                    this,
                    new DeviceChangedEventArgs
                    {
                        Operation = FormOperation.Edit,
                        Device = Device
                    }
                    );

                Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
