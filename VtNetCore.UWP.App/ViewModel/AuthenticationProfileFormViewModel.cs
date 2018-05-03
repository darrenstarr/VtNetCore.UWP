namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class AuthenticationProfileFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.AuthenticationProfile _authenticationProfile;

        public EScope _scope = EScope.Global;

        private string _profileName = string.Empty;
        private Model.EAuthenticationMethod _authenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private Guid _tenantId = Guid.Empty;
        private Guid _siteId = Guid.Empty;
        private string _notes = string.Empty;

        private Guid ProfileTenantId { get; set; } = Guid.Empty;
        private Guid ProfileSiteId { get; set; } = Guid.Empty;

        private bool _isValid;
        private bool _isDirty;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation);
        }

        public Model.AuthenticationProfile AuthenticationProfile
        {
            get => _authenticationProfile;
            set
            {
                _authenticationProfile = value;
                AuthenticationProfileChanged();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AuthenticationProfile"));
            }
        }

        public EScope Scope
        {
            get => _scope;
            set => PropertyChanged.ChangeAndNotify(ref _scope, value, () => Scope, true);
        }

        public string ProfileName
        {
            get => _profileName;
            set => PropertyChanged.ChangeAndNotify(ref _profileName, value, () => ProfileName);
        }

        public Model.EAuthenticationMethod AuthenticationMethod
        {
            get => _authenticationMethod;
            set => PropertyChanged.ChangeAndNotify(ref _authenticationMethod, value, () => AuthenticationMethod);
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

        public Guid TenantId
        {
            get => _tenantId;
            set => PropertyChanged.ChangeAndNotify(ref _tenantId, value, () => TenantId);
        }

        public Guid SiteId
        {
            get => _siteId;
            set => PropertyChanged.ChangeAndNotify(ref _siteId, value, () => SiteId);
        }

        public string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        private Guid ParentId
        {
            get
            {
                return 
                    Scope == EScope.Tenant? TenantId :
                    Scope == EScope.Site ? SiteId :
                    Guid.Empty;
            }
            set
            {
                if(Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == value) != null)
                {
                    Scope = EScope.Tenant;
                    TenantId = value;
                    SiteId = Guid.Empty;
                }
                else if(Model.Context.Current.Sites.TrySingle(x => x.Id == value, out var site))
                {
                    Scope = EScope.Site;
                    TenantId = site.TenantId;
                    SiteId = value;
                }
                else
                {
                    Scope = EScope.Global;
                    TenantId = Guid.Empty;
                    SiteId = Guid.Empty;
                }
            }
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
            ProfileName = string.Empty;
            AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication;
            Username = string.Empty;
            Password = string.Empty;
            ParentId = Guid.Empty;
            Notes = string.Empty;

            ProfileTenantId = Guid.Empty;
            ProfileSiteId = Guid.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        private void AuthenticationProfileChanged()
        {
            if (AuthenticationProfile == null)
            {
                Clear();
                return;
            }

            ProfileName = AuthenticationProfile.Name.BlankIfNull();
            AuthenticationMethod = AuthenticationProfile.AuthenticationMethod;
            Username = AuthenticationProfile.Username.BlankIfNull();
            Password = AuthenticationProfile.Password.PresentablePassword();
            Notes = AuthenticationProfile.Notes.BlankIfNull();
            ParentId = AuthenticationProfile.ParentId;

            ProfileTenantId = TenantId;
            ProfileSiteId = SiteId;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                AuthenticationProfile = new Model.AuthenticationProfile
                {
                    Id = Guid.NewGuid(),
                    Name = ProfileName.TrimEnd(),
                    AuthenticationMethod = AuthenticationMethod,
                    Username = Username.TrimEnd(),
                    Password = Password.TrimEnd(),
                    Notes = Notes.TrimEnd(),
                    ParentId = ParentId
                };
            }
            else
            {
                AuthenticationProfile.Name = ProfileName.TrimEnd();
                AuthenticationProfile.AuthenticationMethod = AuthenticationMethod;
                AuthenticationProfile.Username = Username.TrimEnd();
                AuthenticationProfile.Password = Password.IsEncryptedPassword() ? AuthenticationProfile.Password : Password.TrimEnd();
                AuthenticationProfile.Notes = Notes.TrimEnd();
                AuthenticationProfile.ParentId = ParentId;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public ValidityState FieldIsValid(string name)
        {
            bool valid;
            switch (name)
            {
                case "ProfileName":
                    valid = !string.IsNullOrWhiteSpace(ProfileName);
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            AuthenticationProfile == null && !string.IsNullOrWhiteSpace(ProfileName)
                        ) || (
                            AuthenticationProfile != null &&
                            ProfileName.TrimEnd() != AuthenticationProfile.Name.BlankIfNull()
                        ),
                        Message = valid ? string.Empty : "Profile name is empty"
                    };

                case "AuthenticationMethod":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                AuthenticationProfile == null && AuthenticationMethod != Model.EAuthenticationMethod.NoAuthentication
                            ) || (
                                AuthenticationProfile != null &&
                                AuthenticationMethod != AuthenticationProfile.AuthenticationMethod
                            ),
                    };

                case "Username":
                    valid = !(AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Username));
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            AuthenticationProfile == null && !string.IsNullOrWhiteSpace(Username)
                        ) || (
                            AuthenticationProfile != null &&
                            Username.TrimEnd() != AuthenticationProfile.Username.BlankIfNull()
                        ),
                        Message = valid ? string.Empty : "Username is empty"
                    };

                case "Password":
                    valid = !(AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Password));
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            AuthenticationProfile == null && !string.IsNullOrWhiteSpace(Password)
                        ) || (
                            AuthenticationProfile != null &&
                            Password.TrimEnd() != AuthenticationProfile.Password.PresentablePassword()
                        ),
                        Message = valid ? string.Empty : "Password is empty"
                    };

                case "Notes":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                AuthenticationProfile == null && !string.IsNullOrWhiteSpace(Notes)
                            ) || (
                                AuthenticationProfile != null &&
                                Notes.TrimEnd() != AuthenticationProfile.Notes.BlankIfNull()
                            ),
                    };

                case "ParentId":
                    // TODO : Consider validating scope is appropriately set to the right type for the parent Id
                    break;

                case "TenantId":
                    valid =
                        (
                            (
                                Scope == EScope.Tenant ||
                                Scope == EScope.Site
                            ) &&
                            TenantId != Guid.Empty
                        )
                        ||
                        (
                            !(
                                Scope == EScope.Tenant ||
                                Scope == EScope.Site
                            ) &&
                            TenantId == Guid.Empty
                        );

                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                            (
                                AuthenticationProfile == null &&
                                (
                                    Scope == EScope.Tenant ||
                                    Scope == EScope.Site
                                ) &&
                                TenantId != Guid.Empty
                            ) ||
                            (
                                AuthenticationProfile != null &&
                                (
                                    Scope == EScope.Tenant ||
                                    Scope == EScope.Site
                                ) &&
                                TenantId != ProfileTenantId
                            ),
                        Message = valid ? string.Empty : "Tenant is not set"
                    };

                case "SiteId":
                    valid =
                        (
                            Scope == EScope.Site &&
                            SiteId != Guid.Empty
                        )
                        ||
                        (
                            Scope != EScope.Site &&
                            SiteId == Guid.Empty
                        );

                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                            (
                                AuthenticationProfile == null &&
                                Scope == EScope.Site &&
                                SiteId != Guid.Empty
                            ) ||
                            (
                                AuthenticationProfile != null &&
                                Scope == EScope.Site &&
                                SiteId != ProfileSiteId
                            ),
                        Message = valid ? string.Empty : "Site is not set"
                    };
            }

            return
                new ValidityState
                {
                    Name = name,
                    IsValid = true,
                    Message = string.Empty
                };
        }

        public IEnumerable<ValidityState> Validate()
        {
            List<ValidityState> result = new List<ValidityState>
            {
                new ValidityState
                {
                    Name = "Operation",
                    IsValid = true,
                },

                new ValidityState
                {
                    Name = "SiteId",
                    IsValid = true,
                },

                FieldIsValid("ProfileName"),
                FieldIsValid("AuthenticationMethod"),
                FieldIsValid("Username"),
                FieldIsValid("Password"),
                FieldIsValid("AuthenticationProfileId"),
                FieldIsValid("Notes"),
                FieldIsValid("TenantId"),
                FieldIsValid("SiteId"),
            };

            IsValid = result.Count(x => !x.IsValid) == 0;
            IsDirty = result.Count(x => x.IsChanged) > 0;

            return result;
        }
    }
}
