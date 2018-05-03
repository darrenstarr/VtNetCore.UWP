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

    public sealed partial class SitePropertiesForm : UserControl
    {
        public class SiteChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.Site Site { get; set; }
        };

        public event EventHandler<SiteChangedEventArgs> OnSiteChanged;

        public event EventHandler OnCancelled;

        public SitePropertiesFormViewModel ViewModel { get; set; } = new SitePropertiesFormViewModel();

        private List<ValidationRectangle> AllValidationRectangles = new List<ValidationRectangle>();

        public FormOperation Operation
        {
            get => ViewModel.Operation;
            set => ViewModel.Operation = value;
        }

        public Model.Site Site
        {
            get => ViewModel.Site;
            set => ViewModel.Site = value;
        }

        public Guid TenantId
        {
            get => ViewModel.TenantId;
            set => ViewModel.TenantId = value;
        }

        public SitePropertiesForm()
        {
            InitializeComponent();

            FindChildren(AllValidationRectangles, this);
            Validate();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public void ClearForm()
        {
            ViewModel.Clear();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;
            }

            if (!(e.PropertyName == "IsValid" || e.PropertyName == "IsDirty"))
                Validate();

            DoneButton.IsEnabled = ViewModel.IsValid && ViewModel.IsDirty;
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

        private void DoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // TODO : throw on validation error here.

            ViewModel.Commit();

            OnSiteChanged?.Invoke(
                    this,
                    new SiteChangedEventArgs
                    {
                        Operation = ViewModel.Operation,
                        Site = ViewModel.Site
                    }
                );

            Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;

            OnCancelled?.Invoke(this, new EventArgs());
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
