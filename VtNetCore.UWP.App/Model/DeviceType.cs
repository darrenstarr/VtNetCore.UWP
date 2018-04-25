namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.ComponentModel;

    public class DeviceType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid _id;
        public string _name;
        public string _vendor;
        public string _model;
        public DateTimeOffset _endOfSale;
        public DateTimeOffset _endOfSupport;
        public string _notes;

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
    }
}
