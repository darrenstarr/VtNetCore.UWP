namespace VtNetCore.UWP
{
    using Microsoft.Graphics.Canvas;
    using Microsoft.Graphics.Canvas.Text;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;
    using VtConnect;
    using VtNetCore.VirtualTerminal;
    using VtNetCore.VirtualTerminal.Model;
    using VtNetCore.XTermParser;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation;
    using Windows.UI;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;

    public sealed partial class VirtualTerminalControl : UserControl
    {
        public VirtualTerminalController Terminal { get; set; } = new VirtualTerminalController();
        public DataConsumer Consumer { get; set; }
        public int ViewTop { get; set; } = 0;
        public string WindowTitle { get; set; } = "Session";

        public bool ViewDebugging { get; set; }
        public bool DebugMouse { get; set; }
        public bool DebugSelect { get; set; }

        private char [] _rawText = new char[0];
        private int _rawTextLength = 0;
        private string _rawTextString = "";
        private bool _rawTextChanged = false;
        public DateTime TerminalIdleSince = DateTime.Now;
        public string RawText
        {
            get
            {
                if (_rawTextChanged)
                {
                    lock(_rawText)
                    {

                        _rawTextString = new string(_rawText, 0, _rawTextLength);
                        _rawTextChanged = false;
                    }
                }
                return _rawTextString;
            }
        }

        //public string LogText { get; set; } = string.Empty;

        public VirtualTerminalControl()
        {
            InitializeComponent();

            Consumer = new DataConsumer(Terminal);

            Terminal.SendData += OnSendData;
            Terminal.WindowTitleChanged += OnWindowTitleChanged;
            Terminal.OnLog += OnLog;
            Terminal.StoreRawText = true;
        }

        private void OnLog(object sender, TextEventArgs e)
        {
            //LogText += (e.Text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\n");
        }

        private void OnWindowTitleChanged(object sender, TextEventArgs e)
        {
            WindowTitle = e.Text;
        }

        private void OnSendData(object sender, SendDataEventArgs e)
        {
            if(!Connected)
                return;
            Task.Run(() =>
            {   
                VtConnection.SendData(e.Data);
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        public double CharacterWidth = -1;
        public double CharacterHeight = -1;
        public int Columns = -1;
        public int Rows = -1;

        public Connection VtConnection { get; set; }

        public bool Connected
        {
            get { return VtConnection != null && VtConnection.IsConnected; }
        }

        string InputBuffer { get; set; } = "";

        public void Disconnect()
        {
            if (!Connected)
                return;

            VtConnection.Disconnect();
            VtConnection.DataReceived -= OnDataReceived;
            VtConnection = null;
        }

        public bool ConnectTo(string uri, string username, string password)
        {
            if(Connected)
                return false;       // Already connected

            var credentials = new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            };

            var destination = new Uri(uri);

            VtConnection = Connection.CreateConnection(destination);
            VtConnection.SetTerminalWindowSize(Columns, Rows, 800, 600);

            VtConnection.DataReceived += OnDataReceived;

            var result = VtConnection.Connect(destination, credentials);

            return result;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (Terminal)
            {
                int oldTopRow = Terminal.ViewPort.TopRow;

                Consumer.Push(e.Data);

                if (Terminal.Changed)
                {
                    ProcessRawText();
                    Terminal.ClearChanges();

                    if (oldTopRow != Terminal.ViewPort.TopRow && oldTopRow >= ViewTop)
                        ViewTop = Terminal.ViewPort.TopRow;

                    canvas.Invalidate();
                }

                TerminalIdleSince = DateTime.Now;
            }
        }

        private void ProcessRawText()
        {
            var incoming = Terminal.RawText;

            lock (_rawText)
            {
                if ((_rawTextLength + incoming.Length) > _rawText.Length)
                    Array.Resize(ref _rawText, _rawText.Length + 1000000);

                for (var i = 0; i < incoming.Length; i++)
                    _rawText[_rawTextLength++] = incoming[i];

                _rawTextChanged = true;
            }
        }

        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            CanvasDrawingSession drawingSession = args.DrawingSession;

            CanvasTextFormat format =
                new CanvasTextFormat
                {
                    FontSize = Convert.ToSingle(canvas.FontSize),
                    FontFamily = canvas.FontFamily.Source,
                    FontWeight = canvas.FontWeight,
                    WordWrapping = CanvasWordWrapping.NoWrap
                };

            ProcessTextFormat(drawingSession, format);

            drawingSession.FillRectangle(new Rect(0, 0, canvas.RenderSize.Width, canvas.RenderSize.Height), GetBackgroundColor(Terminal.CursorState.Attributes, false));

            lock (Terminal)
            {
                int row = ViewTop;
                float verticalOffset = -row * (float)CharacterHeight;

                var lines = Terminal.ViewPort.GetLines(ViewTop, Rows);

                var defaultTransform = drawingSession.Transform;
                foreach (var line in lines)
                {
                    if (line == null)
                    {
                        row++;
                        continue;
                    }

                    int column = 0;

                    drawingSession.Transform = Matrix3x2.CreateScale(
                        (float)(line.DoubleWidth ? 2.0 : 1.0),
                        (float)(line.DoubleHeightBottom | line.DoubleHeightTop ? 2.0 : 1.0)
                    );

                    var spanStart = 0;
                    while (column < line.Count)
                    {
                        bool selected = TextSelection == null ? false : TextSelection.Within(column, row);
                        var backgroundColor = GetBackgroundColor(line[column].Attributes, selected);

                        if (column < (line.Count - 1) && GetBackgroundColor(line[column + 1].Attributes, TextSelection == null ? false : TextSelection.Within(column + 1, row)) == backgroundColor)
                        {
                            column++;
                            continue;
                        }

                        var rect = new Rect(
                            spanStart * CharacterWidth,
                            ((row - (line.DoubleHeightBottom ? 1 : 0)) * CharacterHeight + verticalOffset) * (line.DoubleHeightBottom | line.DoubleHeightTop ? 0.5 : 1.0),
                            ((column - spanStart + 1) * CharacterWidth) + 0.9,
                            CharacterHeight + 0.9
                        );

                        drawingSession.FillRectangle(rect, backgroundColor);

                        column++;
                        spanStart = column;
                    }

                    row++;
                }
                drawingSession.Transform = defaultTransform;

                row = ViewTop;
                foreach (var line in lines)
                {
                    if (line == null)
                    {
                        row++;
                        continue;
                    }

                    int column = 0;

                    drawingSession.Transform = Matrix3x2.CreateScale(
                        (float)(line.DoubleWidth ? 2.0 : 1.0),
                        (float)(line.DoubleHeightBottom | line.DoubleHeightTop ? 2.0 : 1.0)
                    );

                    var spanStart = 0;
                    string toDisplay = string.Empty;
                    while (column < line.Count)
                    {
                        bool selected = TextSelection == null ? false : TextSelection.Within(column, row);
                        var foregroundColor = GetForegroundColor(line[column].Attributes, selected);

                        toDisplay += line[column].Char.ToString() + line[column].CombiningCharacters;
                        if (
                            column < (line.Count - 1) && 
                            GetForegroundColor(line[column + 1].Attributes, TextSelection == null ? false : TextSelection.Within(column + 1, row)) == foregroundColor &&
                            line[column + 1].Attributes.Underscore == line[column].Attributes.Underscore &&
                            line[column + 1].Attributes.Reverse == line[column].Attributes.Reverse &&
                            line[column + 1].Attributes.Bright == line[column].Attributes.Bright
                            )
                        {
                            column++;
                            continue;
                        }

                        var rect = new Rect(
                            spanStart * CharacterWidth,
                            ((row - (line.DoubleHeightBottom ? 1 : 0)) * CharacterHeight + verticalOffset) * (line.DoubleHeightBottom | line.DoubleHeightTop ? 0.5 : 1.0),
                            ((column - spanStart + 1) * CharacterWidth) + 0.9,
                            CharacterHeight + 0.9
                        );

                        var textLayout = new CanvasTextLayout(drawingSession, toDisplay, format, 0.0f, 0.0f);

                        drawingSession.DrawTextLayout(
                            textLayout,
                            (float)rect.Left,
                            (float)rect.Top,
                            foregroundColor
                        );

                        if (line[column].Attributes.Underscore)
                        {
                            drawingSession.DrawLine(
                                new Vector2(
                                    (float)rect.Left,
                                    (float)rect.Bottom
                                ),
                                new Vector2(
                                    (float)rect.Right,
                                    (float)rect.Bottom
                                ),
                                foregroundColor
                            );
                        }

                        column++;
                        spanStart = column;
                        toDisplay = "";
                    }

                    //foreach (var character in line)
                    //{
                    //    bool selected = TextSelection == null ? false : TextSelection.Within(column, row);

                    //    var rect = new Rect(
                    //        column * CharacterWidth,
                    //        ((row - (line.DoubleHeightBottom ? 1 : 0)) * CharacterHeight + verticalOffset) * (line.DoubleHeightBottom | line.DoubleHeightTop ? 0.5 : 1.0),
                    //        CharacterWidth + 0.9,
                    //        CharacterHeight + 0.9
                    //    );

                    //    var toDisplay = character.Char.ToString() + character.CombiningCharacters;

                    //    var textLayout = new CanvasTextLayout(drawingSession, toDisplay, format, 0.0f, 0.0f);
                    //    var foregroundColor = GetForegroundColor(character.Attributes, selected);

                    //    drawingSession.DrawTextLayout(
                    //        textLayout,
                    //        (float)rect.Left,
                    //        (float)rect.Top,
                    //        foregroundColor
                    //    );

                    //    if (character.Attributes.Underscore)
                    //    {
                    //        drawingSession.DrawLine(
                    //            new Vector2(
                    //                (float)rect.Left,
                    //                (float)rect.Bottom
                    //            ),
                    //            new Vector2(
                    //                (float)rect.Right,
                    //                (float)rect.Bottom
                    //            ),
                    //            foregroundColor
                    //        );
                    //    }

                    //    column++;
                    //}
                    row++;
                }
                drawingSession.Transform = defaultTransform;

                if (Terminal.CursorState.ShowCursor)
                {
                    var cursorY = Terminal.ViewPort.TopRow - ViewTop + Terminal.CursorState.CurrentRow;
                    var cursorRect = new Rect(
                        Terminal.CursorState.CurrentColumn * CharacterWidth,
                        cursorY * CharacterHeight,
                        CharacterWidth + 0.9,
                        CharacterHeight + 0.9
                    );

                    drawingSession.DrawRectangle(cursorRect, GetForegroundColor(Terminal.CursorState.Attributes, false));
                }
            }

            if (ViewDebugging)
                AnnotateView(drawingSession);
        }

        private void AnnotateView(CanvasDrawingSession drawingSession)
        {
            CanvasTextFormat lineNumberFormat =
                                new CanvasTextFormat
                                {
                                    FontSize = Convert.ToSingle(canvas.FontSize / 2),
                                    FontFamily = canvas.FontFamily.Source,
                                    FontWeight = canvas.FontWeight,
                                    WordWrapping = CanvasWordWrapping.NoWrap
                                };

            for (var i = 0; i < Rows; i++)
            {
                string s = i.ToString();
                var textLayout = new CanvasTextLayout(drawingSession, s.ToString(), lineNumberFormat, 0.0f, 0.0f);
                float y = i * (float)CharacterHeight;
                drawingSession.DrawLine(0, y, (float)canvas.RenderSize.Width, y, Colors.Beige);
                drawingSession.DrawTextLayout(textLayout, (float)(canvas.RenderSize.Width - (CharacterWidth / 2 * s.Length)), y, Colors.Yellow);

                s = (i + 1).ToString();
                textLayout = new CanvasTextLayout(drawingSession, s.ToString(), lineNumberFormat, 0.0f, 0.0f);
                drawingSession.DrawTextLayout(textLayout, (float)(canvas.RenderSize.Width - (CharacterWidth / 2 * (s.Length + 3))), y, Colors.Green);
            }

            var bigText = Terminal.DebugText;
            var bigTextLayout = new CanvasTextLayout(drawingSession, bigText, lineNumberFormat, 0.0f, 0.0f);
            drawingSession.DrawTextLayout(bigTextLayout, (float)(canvas.RenderSize.Width - bigTextLayout.DrawBounds.Width - 100), 0, Colors.Yellow);
        }

        private static Color[] AttributeColors =
        {
            Color.FromArgb(255,0,0,0),        // Black
            Color.FromArgb(255,187,0,0),      // Red
            Color.FromArgb(255,0,187,0),      // Green
            Color.FromArgb(255,187,187,0),    // Yellow
            Color.FromArgb(255,0,0,187),      // Blue
            Color.FromArgb(255,187,0,187),    // Magenta
            Color.FromArgb(255,0,187,187),    // Cyan
            Color.FromArgb(255,187,187,187),  // White
            Color.FromArgb(255,85,85,85),     // Bright black
            Color.FromArgb(255,255,85,85),    // Bright red
            Color.FromArgb(255,85,255,85),    // Bright green
            Color.FromArgb(255,255,255,85),   // Bright yellow
            Color.FromArgb(255,85,85,255),    // Bright blue
            Color.FromArgb(255,255,85,255),   // Bright Magenta
            Color.FromArgb(255,85,255,255),   // Bright cyan
            Color.FromArgb(255,255,255,255),  // Bright white
        };

        private static SolidColorBrush[] AttributeBrushes =
        {
            new SolidColorBrush(AttributeColors[0]),
            new SolidColorBrush(AttributeColors[1]),
            new SolidColorBrush(AttributeColors[2]),
            new SolidColorBrush(AttributeColors[3]),
            new SolidColorBrush(AttributeColors[4]),
            new SolidColorBrush(AttributeColors[5]),
            new SolidColorBrush(AttributeColors[6]),
            new SolidColorBrush(AttributeColors[7]),
            new SolidColorBrush(AttributeColors[8]),
            new SolidColorBrush(AttributeColors[9]),
            new SolidColorBrush(AttributeColors[10]),
            new SolidColorBrush(AttributeColors[11]),
            new SolidColorBrush(AttributeColors[12]),
            new SolidColorBrush(AttributeColors[13]),
            new SolidColorBrush(AttributeColors[14]),
            new SolidColorBrush(AttributeColors[15]),
        };

        private Color GetBackgroundColor(TerminalAttribute attribute, bool invert)
        {
            var flip = Terminal.CursorState.ReverseVideoMode ^ attribute.Reverse ^ invert;

            if (flip)
            {
                if (attribute.Bright)
                    return AttributeColors[(int)attribute.ForegroundColor + 8];

                return AttributeColors[(int)attribute.ForegroundColor];
            }

            return AttributeColors[(int)attribute.BackgroundColor];
        }

        private Color GetForegroundColor(TerminalAttribute attribute, bool invert)
        {
            var flip = Terminal.CursorState.ReverseVideoMode ^ attribute.Reverse ^ invert;

            if (flip)
                return AttributeColors[(int)attribute.BackgroundColor];

            if (attribute.Bright)
                return AttributeColors[(int)attribute.ForegroundColor + 8];

            return AttributeColors[(int)attribute.ForegroundColor];
        }

        private void ProcessTextFormat(CanvasDrawingSession drawingSession, CanvasTextFormat format)
        {
            CanvasTextLayout textLayout = new CanvasTextLayout(drawingSession, "Q", format, 0.0f, 0.0f);
            if (CharacterWidth != textLayout.DrawBounds.Width || CharacterHeight != textLayout.DrawBounds.Height)
            {
                CharacterWidth = textLayout.DrawBounds.Right;
                CharacterHeight = textLayout.DrawBounds.Bottom;
            }

            int columns = Convert.ToInt32(Math.Floor(canvas.RenderSize.Width / CharacterWidth));
            int rows = Convert.ToInt32(Math.Floor(canvas.RenderSize.Height / CharacterHeight));
            if (Columns != columns || Rows != rows)
            {
                Columns = columns;
                Rows = rows;
                ResizeTerminal();

                if (VtConnection != null)
                    VtConnection.SetTerminalWindowSize(columns, rows, 800, 600);
            }
        }

        private void ResizeTerminal()
        {
            //System.Diagnostics.Debug.WriteLine("ResizeTerminal()");
            //System.Diagnostics.Debug.WriteLine("  Character size " + CharacterWidth.ToString() + "," + CharacterHeight.ToString());
            //System.Diagnostics.Debug.WriteLine("  Terminal size " + Columns.ToString() + "," + Rows.ToString());

            Terminal.ResizeView(Columns, Rows);
        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            if (!Connected)
                return;

            var ch = char.ConvertFromUtf32((int)args.KeyCode);
            switch (ch)
            {
                case "\b":
                case "\t":
                case "\n":
                    return;

                case "\r":
                    //LogText = "";
                    lock(_rawText)
                    {
                        _rawTextLength = 0;
                        _rawTextChanged = true;
                    }
                    return;

                default:
                    break;

            }

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = Terminal.GetKeySequence(ch, false, false);
            if(code == null)
                args.Handled = Terminal.KeyPressed(ch, false, false);
        }

        private void TerminalKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!Connected)
                return;

            var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
            var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

            if (controlPressed)
            {
                switch(e.Key)
                {
                    case Windows.System.VirtualKey.F10:
                        Consumer.SequenceDebugging = !Consumer.SequenceDebugging;
                        return;

                    case Windows.System.VirtualKey.F11:
                        ViewDebugging = !ViewDebugging;
                        canvas.Invalidate();
                        return;

                    case Windows.System.VirtualKey.F12:
                        Terminal.Debugging = !Terminal.Debugging;
                        return;
                }
            }

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = Terminal.GetKeySequence(e.Key.ToString(), controlPressed, shiftPressed);
            if(code != null)
                e.Handled = Terminal.KeyPressed(e.Key.ToString(), controlPressed, shiftPressed);

            if (ViewTop != Terminal.ViewPort.TopRow)
            {
                Terminal.ViewPort.SetTopLine(ViewTop);
                canvas.Invalidate();
            }

            //System.Diagnostics.Debug.WriteLine(e.Key.ToString() + ",S" + (shiftPressed ? "1" : "0") + ",C" + (controlPressed ? "1" : "0"));
        }

        private void TerminalTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Focus(FocusState.Pointer);
        }

        private void TerminalGotFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
        }

        private void TerminalLostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
        }

        private void TerminalWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(canvas);

            var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));

            if (controlPressed)
            {
                double scale = 0.9 * (pointer.Properties.MouseWheelDelta / 120);

                var newFontSize = FontSize;
                if (scale < 0)
                    newFontSize *= Math.Abs(scale);
                else
                    newFontSize /= scale;

                if (newFontSize < 2)
                    newFontSize = 2;
                if (newFontSize > 20)
                    newFontSize = 20;

                if (newFontSize != FontSize)
                {
                    FontSize = newFontSize;
                    canvas.Invalidate();
                }
            }
            else
            {
                int oldViewTop = ViewTop;

                ViewTop -= pointer.Properties.MouseWheelDelta / 40;

                if (ViewTop < 0)
                    ViewTop = 0;
                else if (ViewTop > Terminal.ViewPort.TopRow)
                    ViewTop = Terminal.ViewPort.TopRow;

                if (oldViewTop != ViewTop)
                    canvas.Invalidate();
            }
        }

        TextPosition MouseOver { get; set; } = new TextPosition();

        TextRange TextSelection { get; set; }
        bool Selecting = false;

        private TextPosition ToPosition(Point point)
        {
            int overColumn = (int)Math.Floor(point.X / CharacterWidth);
            if (overColumn >= Columns)
                overColumn = Columns - 1;

            int overRow = (int)Math.Floor(point.Y / CharacterHeight);
            if (overRow >= Rows)
                overRow = Rows - 1;

            return new TextPosition { Column = overColumn, Row = overRow };
        }

        private void TerminalPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(canvas);
            var position = ToPosition(pointer.Position);

            if (MouseOver != null && MouseOver == position)
                return;

            MouseOver = position;

            if (pointer.Properties.IsLeftButtonPressed)
            {
                TextRange newSelection;
                var textPosition = position.OffsetBy(0, ViewTop);

                if (MousePressedAt != textPosition)
                {
                    if (MousePressedAt <= textPosition)
                    {
                        newSelection = new TextRange
                        {
                            Start = MousePressedAt,
                            End = textPosition.OffsetBy(-1, 0)
                        };
                    }
                    else
                    {
                        newSelection = new TextRange
                        {
                            Start = textPosition,
                            End = MousePressedAt
                        };
                    }

                    Selecting = true;

                    if (TextSelection != newSelection)
                    {
                        TextSelection = newSelection;

                        if(DebugSelect)
                            System.Diagnostics.Debug.WriteLine("Selection: " + TextSelection.ToString());

                        canvas.Invalidate();
                    }
                }
            }

            if (DebugMouse)
                System.Diagnostics.Debug.WriteLine("Pointer Moved " + position.ToString());
        }

        private void TerminalPointerExited(object sender, PointerRoutedEventArgs e)
        {
            MouseOver = null;

            if(DebugMouse)
                System.Diagnostics.Debug.WriteLine("TerminalPointerExited()");

            canvas.Invalidate();
        }

        public TextPosition MousePressedAt { get; set; }

        private void TerminalPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(canvas);
            var position = ToPosition(pointer.Position);

            var textPosition = position.OffsetBy(0, ViewTop);
            if (pointer.Properties.IsLeftButtonPressed)
                MousePressedAt = textPosition;
            else if (pointer.Properties.IsRightButtonPressed)
                PasteClipboard();

            if (Connected && position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
                var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

                var button =
                    pointer.Properties.IsLeftButtonPressed ? 0 :
                        pointer.Properties.IsRightButtonPressed ? 1 :
                            2;  // Middle button

                Terminal.MousePress(position.Column, position.Row, button, controlPressed, shiftPressed);
            }
        }

        private void TerminalPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(canvas);
            var position = ToPosition(pointer.Position);
            var textPosition = position.OffsetBy(0, ViewTop);

            if (!pointer.Properties.IsLeftButtonPressed)
            {
                if (Selecting)
                {
                    MousePressedAt = null;
                    Selecting = false;

                    if(DebugSelect)
                        System.Diagnostics.Debug.WriteLine("Captured : " + Terminal.GetText(TextSelection.Start.Column, TextSelection.Start.Row, TextSelection.End.Column, TextSelection.End.Row));

                    var captured = Terminal.GetText(TextSelection.Start.Column, TextSelection.Start.Row, TextSelection.End.Column, TextSelection.End.Row);

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(captured);
                    dataPackage.Properties.EnterpriseId = "Terminal";
                    Clipboard.SetContent(dataPackage);
                }
                else
                {
                    TextSelection = null;
                    canvas.Invalidate();
                }
            }

            if (Connected && position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
                var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

                Terminal.MouseRelease(position.Column, position.Row, controlPressed, shiftPressed);
            }
        }

        private void PasteText(string text)
        {
            if (VtConnection == null)
                return;

            Task.Run(() =>
            {
                var buffer = Encoding.UTF8.GetBytes(text);
                Task.Run(() =>
                {
                    VtConnection.SendData(buffer);
                });
            });
        }

        private void PasteClipboard()
        {
            var package = Clipboard.GetContent();

            Task.Run(async () =>
            {
                string text = await package.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                    PasteText(text);
            });
        }
    }
}
