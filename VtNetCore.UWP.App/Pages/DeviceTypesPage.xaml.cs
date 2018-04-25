namespace VtNetCore.UWP.App.Pages
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class DeviceTypesPage : Page
    {
        public AdvancedCollectionView DeviceTypes = new AdvancedCollectionView(Model.Context.Current.DeviceTypes, true);

        public DeviceTypesPage()
        {
            InitializeComponent();

            DeviceTypes.Filter = x => { return ((Model.DeviceType)x).Id != Guid.Empty; };
        }

        private void DeviceTypeEndOfSaleCheckbox_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DeviceTypeEndOfSaleField.Visibility = DeviceTypeEndOfSaleCheckbox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DeviceTypeEndOfSupportCheckBox_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DeviceTypeEndOfSupportField.Visibility = DeviceTypeEndOfSupportCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
