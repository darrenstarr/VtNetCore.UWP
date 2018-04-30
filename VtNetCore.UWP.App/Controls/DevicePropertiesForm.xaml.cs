namespace VtNetCore.UWP.App.Controls
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using VtNetCore.UWP.App.Utility.Helpers;
    using VtNetCore.UWP.App.ViewModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class DevicePropertiesForm : UserControl
    {
        public class DeviceChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Device Device { get; set; }
        };

        public event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;

        private AdvancedCollectionView AuthenticationProfiles { get; } = new AdvancedCollectionView(Model.Context.Current.AuthenticationProfiles);

        private AdvancedCollectionView DeviceTypes { get; } = new AdvancedCollectionView(Model.Context.Current.DeviceTypes);

        public DevicePropertiesFormViewModel ViewModel = new DevicePropertiesFormViewModel();

        private List<ValidationRectangle> AllValidationRectangles = new List<ValidationRectangle>();

        public Model.Device Device
        {
            get => ViewModel.Device;
            set => ViewModel.Device = value;
        }

        public FormOperation Operation
        {
            get => ViewModel.Operation;
            set => ViewModel.Operation = value;
        }

        public Guid SiteId
        {
            get => ViewModel.SiteId;
            set => ViewModel.SiteId = value;
        }

        public DevicePropertiesForm()
        {
            InitializeComponent();

            FindChildren<ValidationRectangle>(AllValidationRectangles, this);
            Validate();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "DeviceAuthenticationMethod":
                    AuthenticationMethodChanged();
                    break;
            }

            Validate();
            DoneButton.IsEnabled = ViewModel.IsDirty;
        }

        private void AuthenticationMethodChanged()
        {
            switch(ViewModel.DeviceAuthenticationMethod)
            {
                case Model.EAuthenticationMethod.UsernamePassword:
                    UsernameLabel.Visibility = Visibility.Visible;
                    UsernameField.Visibility = Visibility.Visible;
                    PasswordLabel.Visibility = Visibility.Visible;
                    PasswordField.Visibility = Visibility.Visible;
                    AuthenticationProfileLabel.Visibility = Visibility.Collapsed;
                    AuthenticationProfileField.Visibility = Visibility.Collapsed;
                    break;

                case Model.EAuthenticationMethod.AuthenticationProfile:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    AuthenticationProfileLabel.Visibility = Visibility.Visible;
                    AuthenticationProfileField.Visibility = Visibility.Visible;
                    break;

                default:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    AuthenticationProfileLabel.Visibility = Visibility.Collapsed;
                    AuthenticationProfileField.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void OperationChanged()
        {
            switch(ViewModel.Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add device";
                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit device";
                    break;
            }
        }

        private void DoneButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // TODO : throw on validation error here.

            ViewModel.Commit();

            OnDeviceChanged?.Invoke(
                    this,
                    new DeviceChangedEventArgs
                    {
                        Operation = ViewModel.Operation,
                        Device = ViewModel.Device
                    }
                );

            Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        public void ClearForm()
        {
            ViewModel.Clear();
        }

        private bool UpdateIsValid(ValidityState state)
        {
            var namedObjects = AllValidationRectangles.Where(x => x.PropertyName == state.Name);
            foreach (var namedObject in namedObjects)
                namedObject.IsValid = state.IsValid;

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
                FindChildren<T>(results, current);
            }
        }
    }
}
