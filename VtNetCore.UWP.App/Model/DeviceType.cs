namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;

    public class DeviceType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _id;
        private string _name;
        private string _vendor;
        private string _model;
        private DateTimeOffset _endOfSale;
        private DateTimeOffset _endOfSupport;
        private string _notes;
        private Guid _deviceClassId = Guid.Empty;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var other = (obj as DeviceType);
            if (other == null)
                return false;

            return
                _id.Equals(other.Id) &&
                _name.Equals(other.Name) &&
                _vendor.Equals(other.Vendor) &&
                _model.Equals(other.Model) &&
                _endOfSale.Equals(other.EndOfSale) &&
                _endOfSupport.Equals(other.EndOfSupport) &&
                _notes.Equals(other.Notes) &&
                _deviceClassId.Equals(other.DeviceClassId);
        }

        /// <summary>
        /// Record Unique ID/Key
        /// </summary>
        public Guid Id
        {
            get => _id;
            set { PropertyChanged.ChangeAndNotify(ref _id, value, () => Id); }
        }

        /// <summary>
        /// Named identifier
        /// </summary>
        /// <remarks>
        /// This is a free-formed string which is used for identifying the device. There are additional
        /// fields for manufacturer and model.
        /// </remarks>
        public string Name
        {
            get => _name;
            set { PropertyChanged.ChangeAndNotify(ref _name, value, () => Name); }
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
        /// The final date of sale of the device
        /// </summary>
        public DateTimeOffset EndOfSale
        {
            get => _endOfSale;
            set { PropertyChanged.ChangeAndNotify(ref _endOfSale, value, () => EndOfSale); }
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
    }
}
