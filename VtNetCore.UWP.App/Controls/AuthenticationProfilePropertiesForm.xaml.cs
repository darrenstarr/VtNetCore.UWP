namespace VtNetCore.UWP.App.Controls
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using VtNetCore.UWP.App.ViewModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class AuthenticationProfilePropertiesForm : 
        UserControl
    {
        public class AuthenticationProfileChangedEventArgs : EventArgs
        {
            public FormOperation Operation { get; set; }
            public Model.AuthenticationProfile AuthenticationProfile { get; set; }
        };

        public event EventHandler<AuthenticationProfileChangedEventArgs> OnAuthenticationProfileChanged;

        private AdvancedCollectionView Tenants { get; } = new AdvancedCollectionView(Model.Context.Current.Tenants);
        private AdvancedCollectionView Sites { get; } = new AdvancedCollectionView(Model.Context.Current.Sites, true);

        public AuthenticationProfileFormViewModel ViewModel { get; } = new AuthenticationProfileFormViewModel();

        public Model.AuthenticationProfile AuthenticationProfile
        {
            get => ViewModel.AuthenticationProfile;
            set => ViewModel.AuthenticationProfile = value;
        }

        public FormOperation Operation
        {
            get => ViewModel.Operation;
            set => ViewModel.Operation = value;
        }

        private List<ValidationRectangle> AllValidationRectangles = new List<ValidationRectangle>();

        public AuthenticationProfilePropertiesForm()
        {
            InitializeComponent();

            Sites.Filter = x =>
            {
                if (ViewModel.TenantId == Guid.Empty)
                    return false;

                return ((Model.Site)x).TenantId == ViewModel.TenantId;
            };

            FindChildren(AllValidationRectangles, this);
            Validate();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            ScopeGlobal.Checked += ScopeRadioChecked;
            ScopeTenant.Checked += ScopeRadioChecked;
            ScopeSite.Checked += ScopeRadioChecked;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Operation":
                    OperationChanged();
                    break;

                case "AuthenticationMethod":
                    AuthenticationMethodChanged();
                    break;

                case "TenantId":
                    TenantIdChanged();
                    break;

                case "Scope":
                    ScopeChanged();
                    break;
            }

            Validate();

            DoneButton.IsEnabled = ViewModel.IsDirty;
        }

        private void TenantIdChanged()
        {
            Sites.RefreshFilter();
        }

        private void ScopeChanged()
        {
            switch(ViewModel.Scope)
            {
                case EScope.Tenant:
                    ScopeTenant.IsChecked = true;
                    break;

                case EScope.Site:
                    ScopeSite.IsChecked = true;
                    break;

                default:
                    ScopeGlobal.IsChecked = true;
                    break;
            }
        }

        private void ScopeRadioChecked(object sender, RoutedEventArgs e)
        {
            if(ScopeGlobal.IsChecked.Value)
            {
                ViewModel.Scope = EScope.Global;

                TenantLabel.Visibility = Visibility.Collapsed;
                TenantField.Visibility = Visibility.Collapsed;
                ViewModel.TenantId = Guid.Empty;

                SiteLabel.Visibility = Visibility.Collapsed;
                SiteField.Visibility = Visibility.Collapsed;
                ViewModel.SiteId = Guid.Empty;
            }
            else if(ScopeTenant.IsChecked.Value)
            {
                ViewModel.Scope = EScope.Tenant;

                TenantLabel.Visibility = Visibility.Visible;
                TenantField.Visibility = Visibility.Visible;

                SiteLabel.Visibility = Visibility.Collapsed;
                SiteField.Visibility = Visibility.Collapsed;
                ViewModel.SiteId = Guid.Empty;
            }
            else if(ScopeSite.IsChecked.Value)
            {
                ViewModel.Scope = EScope.Site;

                TenantLabel.Visibility = Visibility.Visible;
                TenantField.Visibility = Visibility.Visible;

                SiteLabel.Visibility = Visibility.Visible;
                SiteField.Visibility = Visibility.Visible;
            }

            Validate();
        }

        private void OperationChanged()
        {
            switch (Operation)
            {
                case FormOperation.Add:
                    FormHeadingLabel.Text = "Add profile";

                    ScopeGlobal.IsEnabled = true;
                    ScopeTenant.IsEnabled = true;
                    ScopeSite.IsEnabled = true;
                    TenantField.IsEnabled = true;
                    SiteField.IsEnabled = true;

                    break;

                case FormOperation.Edit:
                    FormHeadingLabel.Text = "Edit profile";

                    ScopeGlobal.IsEnabled = false;
                    ScopeTenant.IsEnabled = false;
                    ScopeSite.IsEnabled = false;
                    TenantField.IsEnabled = false;
                    SiteField.IsEnabled = false;

                    break;
            }
        }

        public void ClearForm()
        {
            ViewModel.Clear();
        }

        private void AuthenticationMethodChanged()
        {
            switch (ViewModel.AuthenticationMethod)
            {
                case Model.EAuthenticationMethod.UsernamePassword:
                    UsernameLabel.Visibility = Visibility.Visible;
                    UsernameField.Visibility = Visibility.Visible;
                    PasswordLabel.Visibility = Visibility.Visible;
                    PasswordField.Visibility = Visibility.Visible;
                    break;

                case Model.EAuthenticationMethod.AuthenticationProfile:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    break;

                default:
                    UsernameLabel.Visibility = Visibility.Collapsed;
                    UsernameField.Visibility = Visibility.Collapsed;
                    PasswordLabel.Visibility = Visibility.Collapsed;
                    PasswordField.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void DoneButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // TODO : throw on validation error here.

            ViewModel.Commit();

            OnAuthenticationProfileChanged?.Invoke(
                    this,
                    new AuthenticationProfileChangedEventArgs
                    {
                        Operation = ViewModel.Operation,
                        AuthenticationProfile = ViewModel.AuthenticationProfile
                    }
                );

            Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
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
