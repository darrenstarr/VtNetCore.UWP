namespace VtNetCore.UWP.App.Controls
{
    using System;
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class SitePropertiesForm : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class SiteChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Site Site { get; set; }
        };

        public event EventHandler<SiteChangedEventArgs> OnSiteChanged;

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
            set => PropertyChanged.ChangeAndNotify(ref _site, value, () => Site, true);
        }

        private string SiteName
        {
            get => _siteName;
            set => PropertyChanged.ChangeAndNotify(ref _siteName, value, () => SiteName);
        }

        public Guid TenantId
        {
            get => _tenantId;
            set => PropertyChanged.ChangeAndNotify(ref _tenantId, value, () => TenantId);
        }

        private string Location
        {
            get => _location;
            set => PropertyChanged.ChangeAndNotify(ref _location, value, () => Location);
        }

        private string Notes
        {
            get => _notes;
            set => PropertyChanged.ChangeAndNotify(ref _notes, value, () => Notes);
        }

        public SitePropertiesForm()
        {
            InitializeComponent();

            PropertyChanged += SitePropertiesForm_PropertyChanged;
        }

        private void SitePropertiesForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "Site":
                    SiteChanged();
                    break;
            }

            DoneButton.IsEnabled = IsDirty();
        }

        public void ClearForm()
        {
            SiteName = string.Empty;
            TenantId = Guid.Empty;
            Location = string.Empty;
            Notes = string.Empty;
        }

        private bool IsDirty()
        {
            return
                Site == null ||
                !(
                    SiteName == Site.Name.BlankIfNull() &&
                    Location == Site.Location.BlankIfNull() &&
                    Notes == Site.Notes.BlankIfNull()
                );
        }

        private void OperationChanged()
        {
            switch (Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add Site";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit Site";
                    break;
            }
        }

        private void SiteChanged()
        {
            if (Site == null)
            {
                ClearForm();
                return;
            }

            TenantId = Site.TenantId;
            SiteName = Site.Name.BlankIfNull();
            Location = Site.Location.BlankIfNull();
            Notes = Site.Notes.BlankIfNull();
        }

        private void DoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Operation == FormOperation.Add)
            {
                OnSiteChanged?.Invoke(
                    this,
                    new SiteChangedEventArgs
                    {
                        Operation = FormOperation.Add,
                        Site = new Model.Site
                        {
                            Id = Guid.NewGuid(),
                            TenantId = TenantId,
                            Name = SiteName,
                            Location = Location,
                            Notes = Notes
                        }
                    }
                    );

                Visibility = Visibility.Collapsed;
            }
            else
            {
                Site.Name = SiteName;
                Site.Location = Location;
                Site.Notes = Notes;

                OnSiteChanged?.Invoke(
                    this,
                    new SiteChangedEventArgs
                    {
                        Operation = FormOperation.Edit,
                        Site = Site
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
