namespace VtNetCore.UWP.App.Pages
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class DeviceTypesPage : 
        Page
    {
        public AdvancedCollectionView DeviceTypes = new AdvancedCollectionView(Model.Context.Current.DeviceTypes, true);

        public DeviceTypesPage()
        {
            InitializeComponent();

            DeviceTypeForm.OnDeviceTypeChanged += DeviceTypeForm_OnDeviceTypeChanged;
            DeviceTypeForm.OnCancelled += DeviceTypeForm_OnCancelled;
            DeviceTypeForm.OnLockForm += DeviceTypeForm_OnLockForm;

            DeviceTypes.Filter = x => { return ((Model.DeviceType)x).Id != default; };
        }

        private void DeviceTypeForm_OnLockForm(object sender, Controls.DeviceTypePropertiesForm.IsDirtyEventArgs e)
        {
            AddDeviceTypeButton.IsEnabled = !e.IsDirty;
            RemoveDeviceTypeButton.IsEnabled = !e.IsDirty;
            DeviceTypeList.IsEnabled = !e.IsDirty;
        }

        private void DeviceTypeForm_OnCancelled(object sender, EventArgs e)
        {
            DeviceTypeList.SelectedItem = null;
            AddDeviceTypeButton.IsEnabled = true;
            RemoveDeviceTypeButton.IsEnabled = true;
            DeviceTypeList.IsEnabled = true;
        }

        private async void DeviceTypeForm_OnDeviceTypeChanged(object sender, Controls.DeviceTypePropertiesForm.DeviceTypeChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    Model.Context.Current.DeviceTypes.Add(e.DeviceType);

                    AddDeviceTypeButton.IsEnabled = true;
                    DeviceTypeList.IsEnabled = true;

                    DeviceTypeList.SelectedItem = e.DeviceType;
                    ActivateSelectedItem();
                    break;

                case Controls.FormOperation.Edit:
                    await Model.Context.Current.SaveChanges(e.DeviceType);

                    AddDeviceTypeButton.IsEnabled = true;
                    DeviceTypeList.IsEnabled = true;

                    DeviceTypeList.SelectedItem = e.DeviceType;
                    ActivateSelectedItem();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void DeviceTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() == DeviceTypeForm.DeviceType)
                return;

            if (DeviceTypeList.SelectedItem == null)
            {
                DeviceTypeForm.Visibility = Visibility.Collapsed;
                return;
            }

            ActivateSelectedItem();
        }

        private void ActivateSelectedItem()
        {
            DeviceTypeForm.Operation = Controls.FormOperation.Edit;
            DeviceTypeForm.DeviceType = (Model.DeviceType)DeviceTypeList.SelectedItem;

            DeviceTypeForm.Visibility = Visibility.Visible;
            DeviceTypeForm.SetInitialFocus();

            RemoveDeviceTypeButton.IsEnabled = true;
        }

        private void AddDeviceTypeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DeviceTypeForm.Operation = Controls.FormOperation.Add;

            DeviceTypeForm.DeviceType = null;
            DeviceTypeForm.ClearForm();

            DeviceTypeForm.Visibility = Visibility.Visible;
            DeviceTypeForm.SetInitialFocus();
        }

        private async void RemoveDeviceTypeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {            
            if (DeviceTypeForm.DeviceType == null)
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
                    Model.Context.Current.RemoveDeviceType(DeviceTypeForm.DeviceType);
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

                DeviceTypeForm.DeviceType = null;

                DeviceTypeForm.Visibility = Visibility.Collapsed;
            }
        }
    }
}
