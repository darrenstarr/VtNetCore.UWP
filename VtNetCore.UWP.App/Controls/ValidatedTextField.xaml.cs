namespace VtNetCore.UWP.App.Controls
{
    using System.ComponentModel;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class ValidatedTextField : 
        UserControl,
        INotifyPropertyChanged
    {
        private static Brush RedBrush = new SolidColorBrush(Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Colors.Green);
        private static Brush GrayBrush = new SolidColorBrush(Colors.Gray);

        public event PropertyChangedEventHandler PropertyChanged;

        private string _label;
        private string _currentText;
        private string _text;
        private bool _isChanged;
        private bool _isValid;
        private bool _emptyAllowed = true;
        private bool _multiline;

        public string Label
        {
            get => _label;
            set { PropertyChanged.ChangeAndNotify(ref _label, value, () => Label); }
        }

        public string CurrentText
        {
            get => _currentText;
            set
            {
                PropertyChanged.ChangeAndNotify(ref _currentText, value, () => CurrentText);
                Validate();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                CurrentText = value;
                PropertyChanged.ChangeAndNotify(ref _text, value, () => Text);
                Validate();
            }
        }

        public bool IsChanged
        {
            get => _isChanged;
            set => PropertyChanged.ChangeAndNotify(ref _isChanged, value, () => IsChanged);
        }

        public bool IsValid
        {
            get => _isValid;
            set => PropertyChanged.ChangeAndNotify(ref _isValid, value, () => IsValid);
        }

        public bool EmptyAllowed
        {
            get => _emptyAllowed;
            set => PropertyChanged.ChangeAndNotify(ref _emptyAllowed, value, () => EmptyAllowed);
        }

        public bool Multiline
        {
            get => _multiline;
            set
            {
                TextField.TextWrapping = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
                TextField.MinHeight = value ? 80 : 0;
                TextField.AcceptsReturn = value;

                PropertyChanged.ChangeAndNotify(ref _multiline, value, () => Multiline);
            }
        }

        public ValidatedTextField()
        {
            InitializeComponent();
        }

        public void Commit()
        {
            Text = CurrentText;
            Validate();
        }

        private void Validate()
        {
            IsValid =
                EmptyAllowed ||
                (
                    !EmptyAllowed &&
                    CurrentText.TrimEnd() != string.Empty
                );

            IsChanged = CurrentText.TrimEnd() != Text;

            StateView.Fill =
                (IsValid && IsChanged) ?
                    GreenBrush :
                    (IsValid) ?
                        GrayBrush : RedBrush;
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                "Label",
                typeof(string),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty CurrentTextProperty =
            DependencyProperty.Register(
                "CurrentText",
                typeof(string),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty IsChangedProperty =
            DependencyProperty.Register(
                "IsChanged",
                typeof(bool),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty EmptyAllowedProperty =
            DependencyProperty.Register(
                "EmptyAllowed",
                typeof(bool),
                typeof(ValidatedTextField),
                null
            );

        public static readonly DependencyProperty MultilineProperty =
            DependencyProperty.Register(
                "Multiline",
                typeof(bool),
                typeof(ValidatedTextField),
                null
            );
    }
}
