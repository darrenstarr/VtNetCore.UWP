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

    public sealed partial class TenantPropertiesForm : UserControl
    {
        public class TenantChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Tenant Tenant { get; set; }
        };

        public event EventHandler<TenantChangedEventArgs> OnTenantChanged;

        public TenantPropertiesFormViewModel ViewModel { get; set; } = new TenantPropertiesFormViewModel();

        private List<ValidationRectangle> AllValidationRectangles = new List<ValidationRectangle>();

        public Model.Tenant Tenant
        {
            get => ViewModel.Tenant;
            set => ViewModel.Tenant = value;
        }

        public FormOperation Operation
        {
            get => ViewModel.Operation;
            set => ViewModel.Operation = value;
        }

        public TenantPropertiesForm()
        {
            InitializeComponent();
            FindChildren(AllValidationRectangles, this);

            Validate();
            ViewModel.PropertyChanged += TenantPropertiesForm_PropertyChanged;
        }

        private void TenantPropertiesForm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;
            }

            Validate();
            DoneButton.IsEnabled = ViewModel.IsDirty;
        }

        public void ClearForm()
        {
            ViewModel.Clear();
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

        private void DoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO : throw on validation error here.

            ViewModel.Commit();

            OnTenantChanged?.Invoke(
                    this,
                    new TenantChangedEventArgs
                    {
                        Operation = ViewModel.Operation,
                        Tenant = ViewModel.Tenant
                    }
                );

            Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
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
                FindChildren(results, current);
            }
        }
    }
}
