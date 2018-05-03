namespace VtNetCore.UWP.App.Controls
{
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class ValidationRectangle : UserControl
    {
        public static Brush RedBrush = new SolidColorBrush(Colors.Red);
        public static Brush GreenBrush = new SolidColorBrush(Colors.Green);
        public static Brush GrayBrush = new SolidColorBrush(Colors.Gray);

        private string _propertyName;
        private bool _isValid = true;
        private bool _isChanged = false;

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register(
                "PropertyName", typeof(string),
                typeof(ValidationRectangle), null
            );

        public string PropertyName
        {
            get => _propertyName;
            set => _propertyName = value;
        }

        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;
                if (IsValid && IsChanged)
                    Box.Fill = GreenBrush;
                else if (IsValid)
                    Box.Fill = GrayBrush;
                else
                    Box.Fill = RedBrush;
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                if (IsValid && IsChanged)
                    Box.Fill = GreenBrush;
                else if(IsValid)
                    Box.Fill = GrayBrush;
                else
                    Box.Fill = RedBrush;
            }
        }

        public ValidationRectangle()
        {
            InitializeComponent();
        }
    }
}
