namespace VtNetCore.UWP.App.Pages
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class DeviceTypesPage : 
        Page,
        INotifyPropertyChanged
    {
        public AdvancedCollectionView DeviceTypes = new AdvancedCollectionView(Model.Context.Current.DeviceTypes, true);

        public event PropertyChangedEventHandler PropertyChanged;

        private Guid _selectedDeviceTypeClass = Guid.Empty;
        private Guid SelectedDeviceTypeClass
        {
            get => _selectedDeviceTypeClass;
            set => PropertyChanged.ChangeAndNotify(ref _selectedDeviceTypeClass, value, () => SelectedDeviceTypeClass);
        }

        private Visibility _deviceTypeFormVisibility = Visibility.Collapsed;
        private Visibility DeviceTypeFormVisibility
        {
            get => _deviceTypeFormVisibility;
            set => PropertyChanged.ChangeAndNotify(ref _deviceTypeFormVisibility, value, () => DeviceTypeFormVisibility);
        }

        private bool _formDirty = false;
        private bool FormDirty
        {
            get => _formDirty;
            set => PropertyChanged.ChangeAndNotify(ref _formDirty, value, () => FormDirty);
        }

        private Model.DeviceType _currentDeviceType;
        private Model.DeviceType CurrentDeviceType
        {
            get => _currentDeviceType;
            set => PropertyChanged.ChangeAndNotify(ref _currentDeviceType, value, () => CurrentDeviceType);
        }

        public DeviceTypesPage()
        {
            InitializeComponent();

            PropertyChanged += DeviceTypesPage_PropertyChanged;

            DeviceTypes.Filter = x => { return ((Model.DeviceType)x).Id != Guid.Empty; };
        }

        private void DeviceTypeEndOfSaleCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            DeviceTypeEndOfSaleField.Visibility = DeviceTypeEndOfSaleCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            // TODO : For edit mode

            var item = (Model.DeviceType)DeviceTypeList.SelectedItem;
            FormDirty = FormChanged(item);
        }

        private void DeviceTypeEndOfSupportCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DeviceTypeEndOfSupportField.Visibility = DeviceTypeEndOfSupportCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;

            // TODO : For edit mode

            var item = (Model.DeviceType)DeviceTypeList.SelectedItem;
            FormDirty = FormChanged(item);
        }

        private bool FormChanged (Model.DeviceType other)
        {
            return
                other == null ||
                !(
                    other.Name.Equals(DeviceTypeNameField.Text) &&
                    other.DeviceClassId.Equals((SelectedDeviceTypeClass == null) ? Guid.Empty : SelectedDeviceTypeClass) &&
                    other.EndOfSale.Equals(DeviceTypeEndOfSaleCheckBox.IsChecked.Value ? DeviceTypeEndOfSaleField.Date : DateTimeOffset.MinValue) &&
                    other.EndOfSupport.Equals(DeviceTypeEndOfSupportCheckBox.IsChecked.Value ? DeviceTypeEndOfSupportField.Date : DateTimeOffset.MinValue) &&
                    other.Vendor.Equals(DeviceTypeVendorField.Text) &&
                    other.Model.Equals(DeviceTypeModelField.Text) &&
                    other.Notes.Equals(DeviceTypeNotesField.Text)
                );
        }

        private async void DeviceTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await SaveBeforeContinuing();

            if (DeviceTypeList.SelectedIndex == -1)
            {
                DeviceTypeFormVisibility = Visibility.Collapsed;
                return;
            }

            CurrentDeviceType = (Model.DeviceType)e.AddedItems.First();
            var item = CurrentDeviceType;

            DeviceTypeFormHeading.Text = "Edit";

            ResetForm();

            DeviceTypeFormVisibility = Visibility.Visible;
        }

        private async Task SaveBeforeContinuing()
        {
            if (FormDirty)
            {
                var saveChangesDialog = new ContentDialog
                {
                    Title = "Save changes?",
                    Content = "There are changes to the current entry. Would you like to save them before continuing",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };

                var result = await saveChangesDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                    await SaveChanges();
            }
        }

        // Dirty code to fake bindings I can't get to work
        private void DeviceTypesPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "DeviceTypeFormVisibility":
                    DeviceTypeFormHeading.Visibility = DeviceTypeFormVisibility;
                    DeviceTypeForm.Visibility = DeviceTypeFormVisibility;
                    break;

                case "FormDirty":
                    SaveDeviceTypeButton.IsEnabled = FormDirty;
                    AddDeviceTypeButton.IsEnabled = !FormDirty;
                    DeviceTypeList.IsEnabled = !FormDirty;
                    RemoveDeviceTypeButton.IsEnabled = !FormDirty && CurrentDeviceType != null;
                    ResetDeviceTypeFormButton.IsEnabled = FormDirty;
                    CancelDeviceTypeChangesButton.IsEnabled = (CurrentDeviceType == null || FormDirty);
                    break;

                case "CurrentDeviceType":
                    RemoveDeviceTypeButton.IsEnabled = CurrentDeviceType != null;
                    ResetDeviceTypeFormButton.IsEnabled = FormDirty;
                    CancelDeviceTypeChangesButton.IsEnabled = (CurrentDeviceType == null || FormDirty);
                    break;
            }
        }

        private async Task SaveChanges()
        {
            var item = CurrentDeviceType;
            if (item == null)
                item = new Model.DeviceType { Id = Guid.NewGuid() };

            if (CurrentDeviceType == null || FormChanged(item))
            {
                item.Name = DeviceTypeNameField.Text;
                item.DeviceClassId = (SelectedDeviceTypeClass == null) ? Guid.Empty : SelectedDeviceTypeClass;
                item.EndOfSale = DeviceTypeEndOfSaleCheckBox.IsChecked.Value ? DeviceTypeEndOfSaleField.Date : DateTimeOffset.MinValue;
                item.EndOfSupport = DeviceTypeEndOfSupportCheckBox.IsChecked.Value ? DeviceTypeEndOfSupportField.Date : DateTimeOffset.MinValue;
                item.Vendor = DeviceTypeVendorField.Text;
                item.Model = DeviceTypeModelField.Text;
                item.Notes = DeviceTypeNotesField.Text;

                if (CurrentDeviceType == null)
                    Model.Context.Current.DeviceTypes.Add(item);
                else
                    await Model.Context.Current.SaveChanges(item);

                FormDirty = false;
                DeviceTypeList.SelectedItem = item;
            }
        }

        private async void SaveDeviceTypeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await SaveChanges();
        }

        private void FormTextChanged(object sender, TextChangedEventArgs e)
        {
            FormDirty = FormChanged(CurrentDeviceType);
        }

        private void FormDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            FormDirty = FormChanged(CurrentDeviceType);
        }

        private void DeviceTypeClassField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormDirty = FormChanged(CurrentDeviceType);
        }

        private void ResetForm()
        {
            if(CurrentDeviceType == null)
            {
                DeviceTypeFormHeading.Text = "Add";

                DeviceTypeNameField.Text = string.Empty;
                SelectedDeviceTypeClass = Guid.Empty;
                DeviceTypeEndOfSaleCheckBox.IsChecked = false;
                DeviceTypeEndOfSaleField.Date = DateTimeOffset.Now;
                DeviceTypeEndOfSupportCheckBox.IsChecked = false;
                DeviceTypeEndOfSupportField.Date = DateTimeOffset.Now;
                DeviceTypeVendorField.Text = string.Empty;
                DeviceTypeModelField.Text = string.Empty;
                DeviceTypeNotesField.Text = string.Empty;

                FormDirty = true;
            }
            else
            {
                var item = CurrentDeviceType;

                DeviceTypeNameField.Text = item.Name.BlankIfNull();
                SelectedDeviceTypeClass = item.DeviceClassId;
                DeviceTypeEndOfSaleCheckBox.IsChecked = !(item.EndOfSale == null || item.EndOfSale.Equals(DateTimeOffset.MinValue));
                DeviceTypeEndOfSaleField.Date = (item.EndOfSale == null || item.EndOfSale.Equals(DateTimeOffset.MinValue)) ? DateTimeOffset.Now : item.EndOfSale;
                DeviceTypeEndOfSupportCheckBox.IsChecked = !(item.EndOfSupport == null || item.EndOfSupport.Equals(DateTimeOffset.MinValue));
                DeviceTypeEndOfSupportField.Date = (item.EndOfSupport == null || item.EndOfSupport.Equals(DateTimeOffset.MinValue)) ? DateTimeOffset.Now : item.EndOfSupport;
                DeviceTypeVendorField.Text = item.Vendor.BlankIfNull();
                DeviceTypeModelField.Text = item.Model.BlankIfNull();
                DeviceTypeNotesField.Text = item.Notes.BlankIfNull();

                FormDirty = false;
            }
        }

        private async void AddDeviceTypeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await SaveBeforeContinuing();

            CurrentDeviceType = null;

            DeviceTypeFormHeading.Text = "Add";

            ResetForm();

            DeviceTypeFormVisibility = Visibility.Visible;
        }

        private async void RemoveDeviceTypeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (CurrentDeviceType == null)
                throw new InvalidOperationException("The remove device type button should not be enabled when there is no device type selected");

            var confirmDeletion = new ContentDialog
            {
                Title = "Confirm deletion?",
                Content = "Are you sure you wish to delete this device type from the system?",
                SecondaryButtonText = "Confirm",
                PrimaryButtonText = "Cancel"
            };

            var result = await confirmDeletion.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                try
                {
                    Model.Context.Current.RemoveDeviceType(CurrentDeviceType);
                }
                catch(InvalidOperationException ex)
                {
                    // TODO : Replace this with a "Record in use" in use type exception

                    var itemInUseDialog = new ContentDialog
                    {
                        Title = "Invalid operation",
                        Content = ex.Message,
                        PrimaryButtonText = "Close"
                    };

                    await itemInUseDialog.ShowAsync();

                    return;
                }

                CurrentDeviceType = null;
                FormDirty = false;

                DeviceTypeFormVisibility = Visibility.Collapsed;
            }
        }

        private async void CancelDeviceTypeChangesButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var abandonChangesDialog = new ContentDialog
            {
                Title = "Abandon changes?",
                Content = "There are changes to the current entry. Are you sure you wish to abandon them?",
                SecondaryButtonText = "Yes",
                PrimaryButtonText = "No"
            };

            var result = await abandonChangesDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                return;

            CurrentDeviceType = null;
            FormDirty = false;

            DeviceTypeFormVisibility = Visibility.Collapsed;
        }

        private async void ResetDeviceTypeFormButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var abandonChangesDialog = new ContentDialog
            {
                Title = "Abandon changes?",
                Content = "There are changes to the current entry. Are you sure you wish to abandon them?",
                SecondaryButtonText = "Yes",
                PrimaryButtonText = "No"
            };

            var result = await abandonChangesDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                return;

            ResetForm();
        }
    }
}
