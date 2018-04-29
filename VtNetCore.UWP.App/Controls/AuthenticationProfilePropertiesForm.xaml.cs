namespace VtNetCore.UWP.App.Controls
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class AuthenticationProfilePropertiesForm : 
        UserControl,
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class AuthenticationProfileChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.AuthenticationProfile AuthenticationProfile { get; set; }
        };

        public event EventHandler<AuthenticationProfileChangedEventArgs> OnAuthenticationProfileChanged;

        private AdvancedCollectionView Tenants { get; } = new AdvancedCollectionView(Model.Context.Current.Tenants);
        private AdvancedCollectionView Sites { get; } = new AdvancedCollectionView(Model.Context.Current.Sites, true);

        private FormOperation _operation;
        private Model.AuthenticationProfile _authenticationProfile;
        private Model.EAuthenticationMethod _authenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
        private string _username;
        private string _password;
        private Guid _tenantId;
        private Guid _siteId;
        private string _notes;

        private string _profileName;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation);
        }

        public Model.AuthenticationProfile AuthenticationProfile
        {
            get => _authenticationProfile;
            set => PropertyChanged.ChangeAndNotify(ref _authenticationProfile, value, () => AuthenticationProfile, true);
        }

        private string ProfileName
        {
            get => _profileName;
            set => PropertyChanged.ChangeAndNotify(ref _profileName, value, () => ProfileName);
        }

        private Model.EAuthenticationMethod AuthenticationMethod
        {
            get => _authenticationMethod;
            set => PropertyChanged.ChangeAndNotify(ref _authenticationMethod, value, () => AuthenticationMethod);
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

        private Guid TenantId
        {
            get => _tenantId;
            set => PropertyChanged.ChangeAndNotify(ref _tenantId, value, () => TenantId);
        }

        private Guid SiteId
        {
            get => _siteId;
            set => PropertyChanged.ChangeAndNotify(ref _siteId, value, () => SiteId);
        }

        private string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public AuthenticationProfilePropertiesForm()
        {
            InitializeComponent();

            Sites.Filter = x =>
            {
                if (TenantId == Guid.Empty)
                    return false;

                return ((Model.Site)x).TenantId == TenantId;
            };

            PropertyChanged += AuthenticationProfilePropertiesForm_PropertyChanged;
            ScopeGlobal.Checked += ScopeRadioChecked;
            ScopeTenant.Checked += ScopeRadioChecked;
            ScopeSite.Checked += ScopeRadioChecked;
        }

        private void AuthenticationProfilePropertiesForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "AuthenticationProfile":
                    AuthenticationProfileChanged();
                    break;

                case "AuthenticationMethod":
                    AuthenticationMethodChanged();
                    break;

                case "TenantId":
                    TenantIdChanged();
                    break;
            }

            DoneButton.IsEnabled = IsDirty();
        }

        private void TenantIdChanged()
        {
            Sites.RefreshFilter();
        }

        private void ScopeRadioChecked(object sender, RoutedEventArgs e)
        {
            if(ScopeGlobal.IsChecked.Value)
            {
                TenantLabel.Visibility = Visibility.Collapsed;
                TenantField.Visibility = Visibility.Collapsed;
                TenantId = Guid.Empty;

                SiteLabel.Visibility = Visibility.Collapsed;
                SiteField.Visibility = Visibility.Collapsed;
                SiteId = Guid.Empty;
            }
            else if(ScopeTenant.IsChecked.Value)
            {
                TenantLabel.Visibility = Visibility.Visible;
                TenantField.Visibility = Visibility.Visible;
                TenantId = Guid.Empty;

                SiteLabel.Visibility = Visibility.Collapsed;
                SiteField.Visibility = Visibility.Collapsed;
                SiteId = Guid.Empty;
            }
            else if(ScopeSite.IsChecked.Value)
            {
                TenantLabel.Visibility = Visibility.Visible;
                TenantField.Visibility = Visibility.Visible;

                SiteLabel.Visibility = Visibility.Visible;
                SiteField.Visibility = Visibility.Visible;
                SiteId = Guid.Empty;
            }
        }

        private void OperationChanged()
        {
            switch (Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add profile";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit profile";
                    break;
            }
        }

        public void ClearForm()
        {
            ProfileName = string.Empty;
            AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication;
            Username = string.Empty;
            Password = string.Empty;
            ScopeGlobal.IsChecked = true;
            TenantId = Guid.Empty;
            SiteId = Guid.Empty;
            Notes = string.Empty;
        }

        private void AuthenticationProfileChanged()
        {
            if (AuthenticationProfile == null)
            {
                ClearForm();
                return;
            }

            ProfileName = AuthenticationProfile.Name.BlankIfNull();
            AuthenticationMethod = AuthenticationProfile.AuthenticationMethod;
            Username = AuthenticationProfile.Username.BlankIfNull();
            Password = AuthenticationProfile.Password.BlankIfNull();
            Notes = AuthenticationProfile.Notes.BlankIfNull();

            Model.Tenant tenant;
            Model.Site site;
            if ((tenant = Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == AuthenticationProfile.ParentId)) != null)
            {
                ScopeTenant.IsChecked = true;
                TenantId = tenant.Id;
                SiteId = Guid.Empty;
            }
            else if((site = Model.Context.Current.Sites.SingleOrDefault(x => x.Id == AuthenticationProfile.ParentId)) != null)
            {
                ScopeSite.IsChecked = true;
                TenantId = site.TenantId;
                SiteId = site.Id;
            }
            else
            {
                ScopeGlobal.IsChecked = true;
                TenantId = Guid.Empty;
                SiteId = Guid.Empty;
            }
        }

        private bool IsDirty()
        {
            return
                AuthenticationProfile == null ||
                !(
                    ProfileName == AuthenticationProfile.Name.BlankIfNull() &&
                    AuthenticationMethod == AuthenticationProfile.AuthenticationMethod &&
                    Username == AuthenticationProfile.Username.BlankIfNull() &&
                    Password == AuthenticationProfile.Password.BlankIfNull() &&
                    Notes == AuthenticationProfile.Notes.BlankIfNull() &&
                    (
                        (
                            ScopeGlobal.IsChecked.Value &&
                            AuthenticationProfile.ParentId == Guid.Empty
                        ) ||
                        (
                            ScopeTenant.IsChecked.Value &&
                            TenantId == AuthenticationProfile.ParentId
                        ) ||
                        (
                            ScopeSite.IsChecked.Value &&
                            SiteId == AuthenticationProfile.ParentId
                        )
                    )
                );
        }

        private void AuthenticationMethodChanged()
        {
            switch (AuthenticationMethod)
            {
                case Model.EAuthenticationMethod.UsernamePassword:
                    UsernameLabel.Visibility = Visibility.Visible;
                    UsernameField.Visibility = Visibility.Visible;
                    PasswordLabel.Visibility = Visibility.Visible;
                    PasswordField.Visibility = Visibility.Visible;
                    break;

                case Model.EAuthenticationMethod.AuthenticationProfile:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    break;

                default:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void DoneButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (Operation == FormOperation.Add)
            {
                OnAuthenticationProfileChanged?.Invoke(
                    this,
                    new AuthenticationProfileChangedEventArgs
                    {
                        Operation = FormOperation.Add,
                        AuthenticationProfile = new Model.AuthenticationProfile
                        {
                            Id = Guid.NewGuid(),
                            Name = ProfileName,
                            AuthenticationMethod = AuthenticationMethod,
                            Username = Username,
                            Password = Password,
                            Notes = Notes,
                            ParentId =
                                ScopeTenant.IsChecked.Value ? TenantId :
                                ScopeSite.IsChecked.Value ? SiteId :
                                Guid.Empty
                        }
                    }
                    );

                Visibility = Visibility.Collapsed;
            }
            else
            {
                AuthenticationProfile.Name = ProfileName;
                AuthenticationProfile.AuthenticationMethod = AuthenticationMethod;
                AuthenticationProfile.Username = Username;
                AuthenticationProfile.Password = Password;
                AuthenticationProfile.Notes = Notes;
                AuthenticationProfile.ParentId =
                    ScopeTenant.IsChecked.Value ? TenantId :
                    ScopeSite.IsChecked.Value ? SiteId :
                    Guid.Empty;

                OnAuthenticationProfileChanged?.Invoke(
                    this,
                    new AuthenticationProfileChangedEventArgs
                    {
                        Operation = FormOperation.Edit,
                        AuthenticationProfile = AuthenticationProfile
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
