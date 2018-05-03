namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class SitePropertiesFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.Site _site;

        private string _siteName;
        private Guid _tenantId;
        private string _location;
        private string _notes;

        private bool _isValid;
        private bool _isDirty;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation);
        }

        public Model.Site Site
        {
            get => _site;
            set
            {
                _site = value;
                SiteChanged();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Site"));
            }
        }

        public string SiteName
        {
            get => _siteName;
            set => PropertyChanged.ChangeAndNotify(ref _siteName, value, () => SiteName);
        }

        public Guid TenantId
        {
            get => _tenantId;
            set => PropertyChanged.ChangeAndNotify(ref _tenantId, value, () => TenantId);
        }

        public string Location
        {
            get => _location;
            set => PropertyChanged.ChangeAndNotify(ref _location, value, () => Location);
        }

        public string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public void Clear()
        {
            SiteName = string.Empty;
            TenantId = Guid.Empty;
            Location = string.Empty;
            Notes = string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
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

        private void SiteChanged()
        {
            if (Site == null)
            {
                Clear();
                return;
            }

            TenantId = Site.TenantId;
            SiteName = Site.Name.BlankIfNull();
            Location = Site.Location.BlankIfNull();
            Notes = Site.Notes.BlankIfNull();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                Site = new Model.Site
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    Name = SiteName.TrimEnd(),
                    Location = Location.TrimEnd(),
                    Notes = Notes.TrimEnd()
                };
            }
            else
            {
                Site.Name = SiteName.TrimEnd();
                Site.Location = Location.TrimEnd();
                Site.Notes = Notes.TrimEnd();
            }
        }

        public ValidityState FieldIsValid(string name)
        {
            switch (name)
            {
                case "SiteName":
                    var valid = !string.IsNullOrWhiteSpace(SiteName);
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                        (
                            Site == null && !string.IsNullOrWhiteSpace(SiteName)
                        ) || (
                            Site != null &&
                            SiteName.TrimEnd() != Site.Name
                        ),
                        Message = valid ? string.Empty : "Site name is empty"
                    };

                case "Location":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                Site == null && !string.IsNullOrWhiteSpace(Location)
                            ) || (
                                Site != null &&
                                Location.TrimEnd() != Site.Location
                            ),
                    };

                case "Notes":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                Site == null && !string.IsNullOrWhiteSpace(Notes)
                            ) || (
                                Site != null &&
                                Notes.TrimEnd() != Site.Notes
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
                    Name = "TenantId",
                    IsValid = true,
                }
                );

            result.Add(FieldIsValid("SiteName"));
            result.Add(FieldIsValid("Location"));
            result.Add(FieldIsValid("Notes"));

            IsValid = result.Count(x => !x.IsValid) == 0;
            IsDirty = result.Count(x => x.IsChanged) > 0;

            return result;
        }
    }
}
