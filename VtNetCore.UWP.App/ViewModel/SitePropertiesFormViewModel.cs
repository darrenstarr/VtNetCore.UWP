namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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
            get
            {
                return
                    Site == null ||
                    !(
                        SiteName == Site.Name.BlankIfNull() &&
                        Location == Site.Location.BlankIfNull() &&
                        Notes == Site.Notes.BlankIfNull()
                    );
            }
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
                    Name = SiteName,
                    Location = Location,
                    Notes = Notes
                };
            }
            else
            {
                Site.Name = SiteName;
                Site.Location = Location;
                Site.Notes = Notes;
            }
        }

        public ValidityState IsValid(string name)
        {
            switch (name)
            {
                case "SiteName":
                    if (string.IsNullOrWhiteSpace(SiteName))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Site name is empty"
                        };
                    break;

                case "Location":
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

            if (Operation == FormOperation.Add && TenantId != Guid.Empty)
            {
                result.Add(
                    new ValidityState
                    {
                        Name = "Operation",
                        IsValid = false,
                        Message = "TenantId must be empty when adding"
                    }
                    );

                result.Add(
                    new ValidityState
                    {
                        Name = "TenantId",
                        IsValid = false,
                        Message = "TenantId must be empty when adding"
                    }
                    );
            }
            else if (Operation == FormOperation.Edit && TenantId == Guid.Empty)
            {
                result.Add(
                    new ValidityState
                    {
                        Name = "Operation",
                        IsValid = false,
                        Message = "TenantId must not be empty when editing"
                    }
                    );

                result.Add(
                    new ValidityState
                    {
                        Name = "TenantId",
                        IsValid = false,
                        Message = "TenantId must not be empty when editing"
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
                        Name = "TenantId",
                        IsValid = true,
                    }
                    );
            }

            result.Add(IsValid("SiteName"));
            result.Add(IsValid("Location"));
            result.Add(IsValid("Notes"));

            return result;
        }
    }
}
