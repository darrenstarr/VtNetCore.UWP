namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class TenantPropertiesFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.Tenant _tenant;
        private string _tenantName = string.Empty;
        private string _notes = string.Empty;

        bool _isDirty;
        bool _isValid;

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

        public bool IsValid
        {
            get => _isValid;
            set => PropertyChanged.ChangeAndNotify(ref _isValid, value, () => IsValid);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => PropertyChanged.ChangeAndNotify(ref _isDirty, value, () => IsDirty);
        }

        public bool IsValidAndClean => IsValid && !IsDirty;

        public void Clear()
        {
            TenantName = string.Empty;
            Notes = string.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
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
                    Name = TenantName.TrimEnd(),
                    Notes = Notes.TrimEnd()
                };
            }
            else
            {
                Tenant.Name = TenantName.TrimEnd();
                Tenant.Notes = Notes.TrimEnd();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public ValidityState FieldIsValid(string name)
        {
            switch (name)
            {
                case "TenantName":
                    var valid = !string.IsNullOrWhiteSpace(TenantName);
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = valid,
                        IsChanged =
                            (
                                Tenant == null && !string.IsNullOrWhiteSpace(TenantName)
                            ) || (
                                Tenant != null &&
                                TenantName.TrimEnd() != Tenant.Name
                            ),
                        Message = valid ? string.Empty : "Tenant name is empty"
                    };

                case "Notes":
                    return new ValidityState
                    {
                        Name = name,
                        IsValid = true,
                        IsChanged =
                            (
                                Tenant == null && !string.IsNullOrWhiteSpace(Notes)
                            ) || (
                                Tenant != null &&
                                Notes.TrimEnd() != Tenant.Notes
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
            List<ValidityState> result = new List<ValidityState>
            {
                // TODO : Validate Id?

                new ValidityState
                {
                    Name = "Operation",
                    IsValid = true,
                },

                FieldIsValid("TenantName"),
                FieldIsValid("Notes")
            };

            IsValid = result.Count(x => !x.IsValid) == 0;
            IsDirty =
                Tenant == null ||
                !(
                    TenantName.TrimEnd() == Tenant.Name.BlankIfNull() &&
                    Notes.TrimEnd() == Tenant.Notes.BlankIfNull()
                );

            return result;
        }
    }
}
