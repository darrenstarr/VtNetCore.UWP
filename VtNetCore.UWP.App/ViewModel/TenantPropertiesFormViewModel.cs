namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class TenantPropertiesFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.Tenant _tenant;
        private string _tenantName;
        private string _notes;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation);
        }

        public Model.Tenant Tenant
        {
            get => _tenant;
            set
            {
                _tenant = value;
                TenantChanged();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tenant"));
            }
        }

        public string TenantName
        {
            get => _tenantName;
            set => PropertyChanged.ChangeAndNotify(ref _tenantName, value, () => TenantName);
        }

        public string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public void Clear()
        {
            TenantName = string.Empty;
            Notes = string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public bool IsDirty
        {
            get
            {
                return
                    Tenant == null ||
                    !(
                        TenantName == Tenant.Name.BlankIfNull() &&
                        Notes == Tenant.Notes.BlankIfNull()
                    );
            }
        }

        private void TenantChanged()
        {
            if (Tenant == null)
            {
                Clear();
                return;
            }

            TenantName = Tenant.Name.BlankIfNull();
            Notes = Tenant.Notes.BlankIfNull();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                Tenant = new Model.Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = TenantName,
                    Notes = Notes
                };
            }
            else
            {
                Tenant.Name = TenantName;
                Tenant.Notes = Notes;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public ValidityState IsValid(string name)
        {
            switch (name)
            {
                case "TenantName":
                    if (string.IsNullOrWhiteSpace(TenantName))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            Message = "Tenant name is empty"
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
            List<ValidityState> result = new List<ValidityState>
            {
                // TODO : Validate Id?

                new ValidityState
                {
                    Name = "Operation",
                    IsValid = true,
                },

                IsValid("TenantName"),
                IsValid("Notes")
            };

            return result;
        }
    }
}
