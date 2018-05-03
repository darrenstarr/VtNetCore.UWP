namespace VtNetCore.UWP.App.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using VtNetCore.UWP.App.ViewModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;

    public sealed partial class DeviceTypePropertiesForm : UserControl
    {
        public class DeviceTypeChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.DeviceType DeviceType { get; set; }
        };

        public class IsDirtyEventArgs : EventArgs
        {
            public bool IsDirty { get; set; }
        }

        public event EventHandler<DeviceTypeChangedEventArgs> OnDeviceTypeChanged;

        public event EventHandler OnCancelled;

        public event EventHandler<IsDirtyEventArgs> OnLockForm;

        private DeviceTypePropertiesFormViewModel ViewModel { get; set; } = new DeviceTypePropertiesFormViewModel();

        private List<ValidationRectangle> AllValidationRectangles = new List<ValidationRectangle>();

        public FormOperation Operation
        {
            get => ViewModel.Operation;
            set => ViewModel.Operation = value;
        }

        public Model.DeviceType DeviceType
        {
            get => ViewModel.DeviceType;
            set => ViewModel.DeviceType = value;
        }

        public bool IsDirty => ViewModel.IsDirty;

        public bool IsValidAndClean => ViewModel.IsValidAndClean;

        public DeviceTypePropertiesForm()
        {
            InitializeComponent();

            FindChildren(AllValidationRectangles, FieldsPanel);
            Validate();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "EndOfSaleScheduled":
                    EndOfSaleScheduledChanged();
                    break;

                case "EndOfSupportScheduled":
                    EndOfSupportScheduledChanged();
                    break;

                case "IsDirty":
                    OnLockForm?.Invoke(this, new IsDirtyEventArgs { IsDirty = ViewModel.IsDirty });
                    break;
            }

            if(!(e.PropertyName == "IsValid" || e.PropertyName == "IsDirty"))
                Validate();

            DoneButton.IsEnabled = ViewModel.IsDirty;
            ResetButton.IsEnabled = ViewModel.IsDirty;
        }

        private void EndOfSupportScheduledChanged()
        {
            if (ViewModel.EndOfSupportScheduled)
            {
                if (ViewModel.EndOfSupport == DateTimeOffset.MinValue)
                    ViewModel.EndOfSupport = DateTimeOffset.Now.Date;

                EndOfSupportField.Visibility = Visibility.Visible;
            }
            else
            {
                EndOfSupportField.Visibility = Visibility.Collapsed;
                ViewModel.EndOfSupport = DateTimeOffset.MinValue;
            }
        }

        private void EndOfSaleScheduledChanged()
        {
            if (ViewModel.EndOfSaleScheduled)
            {
                if (ViewModel.EndOfSale == DateTimeOffset.MinValue)
                    ViewModel.EndOfSale = DateTimeOffset.Now.Date;

                EndOfSaleField.Visibility = Visibility.Visible;
            }
            else
            {
                EndOfSaleField.Visibility = Visibility.Collapsed;
                ViewModel.EndOfSale = DateTimeOffset.MinValue;
            }
        }

        private void OperationChanged()
        {
            switch (Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add device type";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit device type";
                    break;
            }
        }

        public void ClearForm()
        {
            ViewModel.Clear();
        }

        private void DoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO : throw on validation error here.

            ViewModel.Commit();

            OnDeviceTypeChanged?.Invoke(
                    this,
                    new DeviceTypeChangedEventArgs
                    {
                        Operation = ViewModel.Operation,
                        DeviceType = ViewModel.DeviceType
                    }
                );
        }

        private async void CancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.IsDirty)
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

                ViewModel.DeviceType = null;
            }

            Visibility = Visibility.Collapsed;
            OnCancelled?.Invoke(this, new EventArgs());
        }

        private async void ResetButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!ViewModel.IsDirty)
                throw new InvalidOperationException("Reset button should not be enabled if the form isn't dirty");

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

            ViewModel.Reset();
        }

        private bool UpdateIsValid(ValidityState state)
        {
            var namedObjects = AllValidationRectangles.Where(x => x.PropertyName == state.Name);
            foreach (var namedObject in namedObjects)
            {
                namedObject.IsValid = state.IsValid;
                namedObject.IsChanged = state.IsChanged;
            }

            return state.IsValid;
        }

        public bool Validate()
        {
            var isValid = true;

            var items = ViewModel.Validate();
            foreach (var item in items)
                if (!UpdateIsValid(item))
                    isValid = false;

            return isValid;
        }

        // TODO : Fix FindChildren to support scrollviewer
        internal static void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);

            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren(results, current);
            }
        }

        public void SetInitialFocus()
        {
            NameField.Focus(FocusState.Programmatic);
        }
    }
}
