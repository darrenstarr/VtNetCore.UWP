namespace VtNetCore.UWP.App.Controls
{
    using System;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class TenantPropertiesForm : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class TenantChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Tenant Tenant { get; set; }
        };

        public event EventHandler<TenantChangedEventArgs> OnTenantChanged;

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
            set => PropertyChanged.ChangeAndNotify(ref _tenant, value, () => Tenant, true);
        }

        private string TenantName
        {
            get => _tenantName;
            set => PropertyChanged.ChangeAndNotify(ref _tenantName, value, () => TenantName);
        }

        private string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public TenantPropertiesForm()
        {
            InitializeComponent();

            PropertyChanged += TenantPropertiesForm_PropertyChanged;
        }

        private void TenantPropertiesForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "Tenant":
                    TenantChanged();
                    break;
            }

            DoneButton.IsEnabled = IsDirty();
        }

        public void ClearForm()
        {
            TenantName = string.Empty;
            Notes = string.Empty;
        }

        private bool IsDirty()
        {
            return
                Tenant == null ||
                !(
                    TenantName == Tenant.Name.BlankIfNull() &&
                    Notes == Tenant.Notes.BlankIfNull()
                );
        }

        private void OperationChanged()
        {
            switch (Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add tenant";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit tenant";
                    break;
            }
        }

        private void TenantChanged()
        {
            if (Tenant == null)
            {
                ClearForm();
                return;
            }

            TenantName = Tenant.Name.BlankIfNull();
            Notes = Tenant.Notes.BlankIfNull();
        }

        private void DoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Operation == FormOperation.Add)
            {
                OnTenantChanged?.Invoke(
                    this,
                    new TenantChangedEventArgs
                    {
                        Operation = FormOperation.Add,
                        Tenant = new Model.Tenant
                        {
                            Id = Guid.NewGuid(),
                            Name = TenantName,
                            Notes = Notes
                        }
                    }
                    );

                Visibility = Visibility.Collapsed;
            }
            else
            {
                Tenant.Name = TenantName;
                Tenant.Notes = Notes;

                OnTenantChanged?.Invoke(
                    this,
                    new TenantChangedEventArgs
                    {
                        Operation = FormOperation.Edit,
                        Tenant = Tenant
                    }
                    );

                Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
