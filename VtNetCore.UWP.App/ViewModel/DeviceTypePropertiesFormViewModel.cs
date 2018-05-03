namespace VtNetCore.UWP.App.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Controls;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class DeviceTypePropertiesFormViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FormOperation _operation;
        private Model.DeviceType _deviceType;

        private string _deviceTypeName = string.Empty;
        private string _vendor = string.Empty;
        private string _model = string.Empty;
        private bool _endOfSaleScheduled = false;
        private DateTimeOffset _endOfSale = DateTimeOffset.MinValue;
        private bool _endOfSupportScheduled = false;
        private DateTimeOffset _endOfSupport = DateTimeOffset.MinValue;
        private string _notes = string.Empty;
        private Guid _deviceClassId = Guid.Empty;

        private bool _isValid;
        private bool _isDirty;

        public FormOperation Operation
        {
            get => _operation;
            set => PropertyChanged.ChangeAndNotify(ref _operation, value, () => Operation, true);
        }

        public Model.DeviceType DeviceType
        {
            get => _deviceType;
            set
            {
                _deviceType = value;
                Reset();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DeviceType"));
            }
        }

        /// <summary>
        /// Named identifier
        /// </summary>
        /// <remarks>
        /// This is a free-formed string which is used for identifying the device. There are additional
        /// fields for manufacturer and model.
        /// </remarks>
        public string DeviceTypeName
        {
            get => _deviceTypeName;
            set => PropertyChanged.ChangeAndNotify(ref _deviceTypeName, value, () => DeviceTypeName);
        }

        /// <summary>
        /// The vendor/manufacturer of the device
        /// </summary>
        /// <remark>
        /// The vendor is the company who produces the device. For example, Cisco, HP, Dell, etc...
        /// </remark>
        public string Vendor
        {
            get => _vendor;
            set { PropertyChanged.ChangeAndNotify(ref _vendor, value, () => Vendor); }
        }

        /// <summary>
        /// The model of the device
        /// </summary>
        public string Model
        {
            get => _model;
            set { PropertyChanged.ChangeAndNotify(ref _model, value, () => Model); }
        }

        /// <summary>
        /// Specifies that the end of sale has been scheduled for this device type
        /// </summary>
        public bool EndOfSaleScheduled
        {
            get => _endOfSaleScheduled;
            set { PropertyChanged.ChangeAndNotify(ref _endOfSaleScheduled, value, () => EndOfSaleScheduled); }
        }

        /// <summary>
        /// The final date of sale of the device
        /// </summary>
        public DateTimeOffset EndOfSale
        {
            get => _endOfSale;
            set { PropertyChanged.ChangeAndNotify(ref _endOfSale, value, () => EndOfSale); }
        }

        /// <summary>
        /// Specifies that the end of support has been scheduled for this device type
        /// </summary>
        public bool EndOfSupportScheduled
        {
            get => _endOfSupportScheduled;
            set { PropertyChanged.ChangeAndNotify(ref _endOfSupportScheduled, value, () => EndOfSupportScheduled); }
        }

        /// <summary>
        /// The final date of manufacturer support of a device
        /// </summary>
        public DateTimeOffset EndOfSupport
        {
            get => _endOfSupport;
            set { PropertyChanged.ChangeAndNotify(ref _endOfSupport, value, () => EndOfSupport); }
        }

        /// <summary>
        /// Free-form notes field
        /// </summary>
        public string Notes
        {
            get => _notes;
            set { PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes); }
        }

        /// <summary>
        /// Specifies the device class. For example Router/Switch...
        /// </summary>
        public Guid DeviceClassId
        {
            get => _deviceClassId;
            set { PropertyChanged.ChangeAndNotify(ref _deviceClassId, value, () => DeviceClassId); }
        }

        public void Clear()
        {
            DeviceTypeName = string.Empty;
            Vendor = string.Empty;
            Model = string.Empty;
            EndOfSaleScheduled = false;
            EndOfSale = DateTimeOffset.MinValue;
            EndOfSupportScheduled = false;
            EndOfSupport = DateTimeOffset.MinValue;
            Notes = string.Empty;
            DeviceClassId = Guid.Empty;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public void Reset()
        {
            if (DeviceType == null)
            {
                Clear();
                return;
            }

            DeviceTypeName = DeviceType.Name.BlankIfNull();
            Vendor = DeviceType.Vendor.BlankIfNull();
            Model = DeviceType.Model.BlankIfNull();
            EndOfSaleScheduled = !(DeviceType.EndOfSale == DateTimeOffset.MinValue);
            EndOfSale = DeviceType.EndOfSale;
            EndOfSupportScheduled = !(DeviceType.EndOfSupport == DateTimeOffset.MinValue);
            EndOfSupport = DeviceType.EndOfSupport;
            Notes = DeviceType.Notes.BlankIfNull();
            DeviceClassId = DeviceType.DeviceClassId;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
        }

        public void Commit()
        {
            if (Operation == FormOperation.Add)
            {
                DeviceType = new Model.DeviceType
                {
                    Id = Guid.NewGuid(),
                    Name = DeviceTypeName,
                    Vendor = Vendor,
                    Model = Model,
                    EndOfSale = (EndOfSaleScheduled ? EndOfSale : DateTimeOffset.MinValue),
                    EndOfSupport = (EndOfSupportScheduled ? EndOfSupport : DateTimeOffset.MinValue),
                    Notes = Notes,
                    DeviceClassId = DeviceClassId
                };
            }
            else
            {
                DeviceType.Name = DeviceTypeName;
                DeviceType.Vendor = Vendor;
                DeviceType.Model = Model;
                DeviceType.EndOfSale = (EndOfSaleScheduled ? EndOfSale : DateTimeOffset.MinValue);
                DeviceType.EndOfSupport = (EndOfSupportScheduled ? EndOfSupport : DateTimeOffset.MinValue);
                DeviceType.Notes = Notes;
                DeviceType.DeviceClassId = DeviceClassId;
            }
        }

        public ValidityState FieldIsValid(string name)
        {
            switch (name)
            {
                case "DeviceTypeName":
                    if (string.IsNullOrWhiteSpace(DeviceTypeName))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            IsChanged = FieldIsChanged(name),
                            Message = "Device type name is empty"
                        };
                    break;

                case "Vendor":
                    if (string.IsNullOrWhiteSpace(Vendor))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            IsChanged = FieldIsChanged(name),
                            Message = "Vendor name is empty"
                        };
                    break;

                case "Model":
                    if (string.IsNullOrWhiteSpace(Model))
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            IsChanged = FieldIsChanged(name),
                            Message = "Model is empty"
                        };
                    break;

                case "EndOfSale":
                    break;

                case "EndOfSupport":
                    break;

                case "Notes":
                    break;

                case "DeviceClassId":
                    if (DeviceClassId == default)
                        return new ValidityState
                        {
                            Name = name,
                            IsValid = false,
                            IsChanged = FieldIsChanged(name),
                            Message = "Device class is not specified"
                        };
                    break;
            }

            return
                new ValidityState
                {
                    Name = name,
                    IsValid = true,
                    IsChanged = FieldIsChanged(name),
                };
        }

        public bool FieldIsChanged(string name)
        {
            switch (name)
            {
                case "DeviceTypeName":
                    return
                        DeviceTypeName.Trim() !=
                            ((DeviceType == null) ?
                                string.Empty :
                                DeviceType.Name.BlankIfNull()
                                );

                case "Vendor":
                    return
                        Vendor.Trim() !=
                            ((DeviceType == null) ?
                                string.Empty :
                                DeviceType.Vendor.BlankIfNull()
                                );

                case "Model":
                    return
                        Model.Trim() !=
                            ((DeviceType == null) ?
                                string.Empty :
                                DeviceType.Model.BlankIfNull()
                                );

                case "EndOfSale":
                    return
                        (
                            DeviceType == null &&
                            EndOfSaleScheduled
                        ) || (
                            DeviceType != null &&
                            (
                                (
                                    EndOfSaleScheduled &&
                                    EndOfSale != DeviceType.EndOfSale
                                ) || (
                                    !EndOfSaleScheduled &&
                                    DateTimeOffset.MinValue != DeviceType.EndOfSale
                                )
                            )
                        );

                case "EndOfSupport":
                    return
                        (
                            DeviceType == null &&
                            EndOfSupportScheduled
                        ) || (
                            DeviceType != null &&
                            (
                                (
                                    EndOfSupportScheduled &&
                                    EndOfSupport != DeviceType.EndOfSupport
                                ) || (
                                    !EndOfSupportScheduled &&
                                    DateTimeOffset.MinValue != DeviceType.EndOfSupport
                                )
                            )
                        );

                case "Notes":
                    return
                        Notes.Trim() !=
                            ((DeviceType == null) ?
                                string.Empty :
                                DeviceType.Notes.BlankIfNull()
                                );

                case "DeviceClassId":
                    return
                        DeviceClassId !=
                            ((DeviceType == null) ?
                                Guid.Empty :
                                DeviceType.DeviceClassId
                            );

                default:
                    return false;
            }
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

            result.Add(FieldIsValid("DeviceTypeName"));
            result.Add(FieldIsValid("Vendor"));
            result.Add(FieldIsValid("Model"));
            result.Add(FieldIsValid("DeviceClassId"));
            result.Add(FieldIsValid("EndOfSale"));
            result.Add(FieldIsValid("EndOfSupport"));
            result.Add(FieldIsValid("Notes"));

            IsValid = result.Count(x => x.IsValid == false) == 0;
            IsDirty = result.Count(x => x.IsChanged == true) > 0;

            return result;
        }

        public bool IsValid
        {
            get => _isValid;
            set => PropertyChanged.ChangeAndNotify(ref _isValid, value, () => IsValid, true);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => PropertyChanged.ChangeAndNotify(ref _isDirty, value, () => IsDirty, true);
        }

        public bool IsValidAndClean
        {
            get => _isValid && !_isDirty;
        }
    }
}
