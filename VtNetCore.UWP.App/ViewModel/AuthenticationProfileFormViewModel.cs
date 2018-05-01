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

        private string _profileName;
        private Model.EAuthenticationMethod _authenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
        private string _username;
        private string _password;
        private Guid _tenantId;
        private Guid _siteId;
        private string _notes;

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
                Model.Site site;

                if(Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == value) != null)
                {
                    Scope = EScope.Tenant;
                    TenantId = value;
                    SiteId = Guid.Empty;
                }
                else if((site = Model.Context.Current.Sites.SingleOrDefault(x => x.Id == value)) != null)
                {
                    // TODO : Make a .TrySingle() function
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

        public void Clear()
        {
            ProfileName = string.Empty;
            AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication;
            Username = string.Empty;
            Password = string.Empty;
            ParentId = Guid.Empty;
            Notes = string.Empty;
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
            Password = AuthenticationProfile.Password.BlankIfNull();
            Notes = AuthenticationProfile.Notes.BlankIfNull();
            ParentId = AuthenticationProfile.ParentId;
        }

        public bool IsDirty
        {
            get
            {
                return 
                    AuthenticationProfile == null ||
                    !(
                      ProfileName == AuthenticationProfile.Name.BlankIfNull() &&
                      AuthenticationMethod == AuthenticationProfile.AuthenticationMethod &&
                      Username == AuthenticationProfile.Username.BlankIfNull() &&
                      Password == AuthenticationProfile.Password.BlankIfNull() &&
                      Notes == AuthenticationProfile.Notes.BlankIfNull() &&
                      ParentId == AuthenticationProfile.ParentId
                    );
            }
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                AuthenticationProfile = new Model.AuthenticationProfile
                {
                    Id = Guid.NewGuid(),
                    Name = ProfileName,
                    AuthenticationMethod = AuthenticationMethod,
                    Username = Username,
                    Password = Password,
                    Notes = Notes,
                    ParentId = ParentId
                };
            }
            else
            {
                AuthenticationProfile.Name = ProfileName;
                AuthenticationProfile.AuthenticationMethod = AuthenticationMethod;
                AuthenticationProfile.Username = Username;
                AuthenticationProfile.Password = Password;
                AuthenticationProfile.Notes = Notes;
                AuthenticationProfile.ParentId = ParentId;
            }
        }

        public ValidityState IsValid(string name)
        {
            switch (name)
            {
                case "ProfileName":
                    if (string.IsNullOrWhiteSpace(ProfileName))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Profile name is empty"
                        };
                    break;

                case "DeviceAuthenticationMethod":
                    break;

                case "Username":
                    if (AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Username))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Username is empty"
                        };
                    break;

                case "Password":
                    if (AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword && string.IsNullOrWhiteSpace(Password))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Password is empty"
                        };
                    break;

                case "Notes":
                    break;

                case "ParentId":
                    // TODO : Consider validating scope is appropriately set to the right type for the parent Id
                    break;

                case "TenantId":
                    if ((Scope == EScope.Tenant || Scope == EScope.Site) && TenantId == Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Tenant is not set"
                        };

                    if (Scope == EScope.Global && TenantId != Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Tenant is set, but scope is global"
                        };

                    break;

                case "SiteId":
                    if (Scope == EScope.Site && SiteId == Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Site is not set"
                        };

                    if (Scope != EScope.Site && SiteId != Guid.Empty)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Site is set, but scope is not site"
                        };

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

                IsValid("ProfileName"),
                IsValid("AuthenticationMethod"),
                IsValid("Username"),
                IsValid("Password"),
                IsValid("AuthenticationProfileId"),
                IsValid("Notes"),
                IsValid("TenantId"),
                IsValid("SiteId"),
            };

            return result;
        }
    }
}
