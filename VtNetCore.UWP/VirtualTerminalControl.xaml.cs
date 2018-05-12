namespace VtNetCore.UWP
{
    using Microsoft.Graphics.Canvas;
    using Microsoft.Graphics.Canvas.Text;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;
    using VtNetCore.VirtualTerminal;
    using VtNetCore.XTermParser;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation;
    using Windows.UI;
    using Windows.UI.Core;
    using Windows.UI.Text;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class VirtualTerminalControl : 
        UserControl,
        INotifyPropertyChanged
    {
        private VirtualTerminalController _terminal;
        public VirtualTerminalController Terminal
        {
            get
            {
                return _terminal;
            }
            set
            {
                if(value == null)
                {
                    if(_terminal != null)
                    {
                        lock (_terminal)
                        {
                            _terminal.WindowTitleChanged -= OnWindowTitleChanged;
                        }
                    }
                }

                _terminal = value;

                if (_terminal != null)
                {
                    WindowTitle = _terminal.WindowTitle;
                    _terminal.WindowTitleChanged += OnWindowTitleChanged;
                    ViewTop = _terminal.ViewPort.TopRow;
                }
            }
        }

        public DataConsumer _consumer;
        public DataConsumer Consumer
        {
            get
            {
                return _consumer;
            }
            set
            {
                _consumer = value;
            }
        }
        public int ViewTop { get; set; } = 0;
        public event PropertyChangedEventHandler PropertyChanged;

        private int BlinkShowMs { get; set; } = 600;
        private int BlinkHideMs { get; set; } = 300;

        private string _windowTitle = "Session";
        public string WindowTitle {
            get { return _windowTitle; }
            private set
            {
                _windowTitle = value;
                if (PropertyChanged != null)
                {
                    Task.Factory.StartNew(() =>
                        Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowTitle"));
                        })
                    );
                }
            }
        }

        public bool ViewDebugging { get; set; }
        public bool DebugMouse { get; set; }
        public bool DebugSelect { get; set; }

        private bool Scrolling { get; set; }
        public int ScrollValue
        {
            get { return ViewTop; }
            set
            {
                Scrolling = true;
                ViewTop = value;
                canvas.Invalidate();
            }
        }

        public int MaxScrollValue
        {
            get { return Terminal.ViewPort.TopRow; }
        }

        public string LogText { get; set; } = string.Empty;

        DispatcherTimer blinkDispatcher;

        // Use Euclid's algorithm to calculate the
        // greatest common divisor (GCD) of two numbers.
        private long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            // Pull out remainders.
            for (; ; )
            {
                long remainder = a % b;
                if (remainder == 0) return b;
                a = b;
                b = remainder;
            };
        }

        public VirtualTerminalControl()
        {
            InitializeComponent();

            blinkDispatcher = new DispatcherTimer();
            blinkDispatcher.Tick += BlinkTimerHandler;
            blinkDispatcher.Interval = TimeSpan.FromMilliseconds(Math.Min(150, GCD(BlinkShowMs, BlinkHideMs)));
            blinkDispatcher.Start();
        }

        void BlinkTimerHandler(object sender, object e)
        {
            canvas.Invalidate();
        }

        private void OnLog(object sender, TextEventArgs e)
        {
            LogText += (e.Text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\n");
        }

        private void OnWindowTitleChanged(object sender, TextEventArgs e)
        {
            WindowTitle = e.Text;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            blinkDispatcher.Stop();
            blinkDispatcher.Tick -= BlinkTimerHandler;
            blinkDispatcher = null;

            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        public double CharacterWidth = -1;

        public double CharacterHeight = -1;

        public int Columns = -1;

        public int Rows = -1;

        string InputBuffer { get; set; } = "";

        public void PushText(string text)
        {
            lock(Terminal)
            {
                int oldTopRow = Terminal.ViewPort.TopRow;

                Consumer.Push(Encoding.UTF8.GetBytes(text));

                if (Terminal.Changed)
                {
                    Terminal.ClearChanges();

                    if (oldTopRow != Terminal.ViewPort.TopRow && oldTopRow >= ViewTop)
                    {
                        ViewTop = Terminal.ViewPort.TopRow;
                        Task.Factory.StartNew(() =>
                            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ScrollValue"));
                            })
                        );
                    }

                    canvas.Invalidate();
                }
            }
        }

        public int BottomRow
        {
            get
            {
                return ViewTop + Rows - 1;
            }
        }

        private bool BlinkVisible()
        {
            var blinkCycle = BlinkShowMs + BlinkHideMs;

            return (DateTime.Now.Subtract(DateTime.MinValue).Milliseconds % blinkCycle) < BlinkHideMs;
        }

        public Color GetSolidColorBrush(string hex)
        {
            byte a = 255; // (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(1, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(3, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(5, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }

        private void PaintBackgroundLayer(CanvasDrawingSession drawingSession, List<VirtualTerminal.Layout.LayoutRow> spans)
        {
            double lineY = 0;
            foreach (var textRow in spans)
            {
                drawingSession.Transform =
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),  // Scale double width
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0) // Scale double high
                    );

                var drawY =
                    (lineY - (textRow.DoubleHeightBottom ? CharacterHeight : 0)) *      // Offset position upwards for bottom of double high char
                    ((textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0); // Scale position for double height

                double drawX = 0;
                foreach (var textSpan in textRow.Spans)
                {
                    var bounds =
                        new Rect(
                            drawX,
                            drawY,
                            CharacterWidth * (textSpan.Text.Length) + 0.9,
                            CharacterHeight + 0.9
                        );

                    drawingSession.FillRectangle(bounds, GetSolidColorBrush(textSpan.BackgroundColor));

                    drawX += CharacterWidth * (textSpan.Text.Length);
                }

                lineY += CharacterHeight;
            }
        }

        private void PaintTextLayer(CanvasDrawingSession drawingSession, List<VirtualTerminal.Layout.LayoutRow> spans, CanvasTextFormat textFormat, bool showBlink)
        {
            var dipToDpiRatio = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96;

            double lineY = 0;
            foreach (var textRow in spans)
            {
                drawingSession.Transform =
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),  // Scale double width
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0) // Scale double high
                    );

                var drawY =
                    (lineY - (textRow.DoubleHeightBottom ? CharacterHeight : 0)) *      // Offset position upwards for bottom of double high char
                    ((textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0); // Scale position for double height

                double drawX = 0;
                foreach (var textSpan in textRow.Spans)
                {
                    var runWidth = CharacterWidth * (textSpan.Text.Length);

                    if (textSpan.Hidden || (textSpan.Blink && !showBlink))
                    {
                        drawX += runWidth;
                        continue;
                    }

                    var color = GetSolidColorBrush(textSpan.ForgroundColor);
                    textFormat.FontWeight = textSpan.Bold ? FontWeights.Bold : FontWeights.Light;

                    var textLayout = new CanvasTextLayout(drawingSession, textSpan.Text, textFormat, 0.0f, 0.0f);
                    drawingSession.DrawTextLayout(
                        textLayout,
                        (float)drawX,
                        (float)drawY,
                        color
                    );

                    // TODO : Come up with a better means of identifying line weight and offset
                    double underlineOffset = textLayout.LineMetrics[0].Baseline * dipToDpiRatio * 1.07;

                    if (textSpan.Underline)
                    {
                        drawingSession.DrawLine(
                            new Vector2(
                                (float)drawX,
                                (float)(drawY + underlineOffset)
                            ),
                            new Vector2(
                                (float)(drawX + runWidth),
                                (float)(drawY + underlineOffset)
                            ),
                            color
                        );
                    }

                    drawX += CharacterWidth * (textSpan.Text.Length);
                }

                lineY += CharacterHeight;
            }
        }

        private void PaintCursor(CanvasDrawingSession drawingSession, List<VirtualTerminal.Layout.LayoutRow> spans, CanvasTextFormat textFormat, TextPosition cursorPosition, Color cursorColor)
        {
            var cursorY = cursorPosition.Row;
            if (cursorY < 0 || cursorY >= Rows)
                return;

            var drawX = cursorPosition.Column * CharacterWidth;
            var drawY = (cursorY * CharacterHeight);

            if (cursorY < spans.Count)
            {
                var textRow = spans[cursorY];

                drawingSession.Transform =
                    Matrix3x2.CreateTranslation(
                        1.0f,
                        (float)(textRow.DoubleHeightBottom ? -CharacterHeight : 0)
                    ) *
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0)
                    );

                drawY *= (textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0;
            }


            var cursorRect = new Rect(
                drawX,
                drawY,
                CharacterWidth,
                CharacterHeight + 0.9
            );

            drawingSession.DrawRectangle(cursorRect, cursorColor);
        }

        private void OnCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            CanvasDrawingSession drawingSession = args.DrawingSession;

            CanvasTextFormat textFormat =
                new CanvasTextFormat
                {
                    FontSize = Convert.ToSingle(canvas.FontSize),
                    FontFamily = canvas.FontFamily.Source,
                    FontWeight = canvas.FontWeight,
                    WordWrapping = CanvasWordWrapping.NoWrap
                };

            ProcessTextFormat(drawingSession, textFormat);

            var showBlink = BlinkVisible();

            List<VirtualTerminal.Layout.LayoutRow> spans = null;
            TextPosition cursorPosition = null;
            bool showCursor = false;
            Color cursorColor = Colors.Green;
            int TerminalTop = -1;

            lock (Terminal)
            {
                TerminalTop = Terminal.ViewPort.TopRow;

                if (Terminal.ViewPort.CursorPosition.Row > Rows)
                    throw new Exception("We should never be here");

                spans = Terminal.ViewPort.GetPageSpans(ViewTop, Rows, Columns, TextSelection);
                showCursor = Terminal.CursorState.ShowCursor;
                cursorPosition = Terminal.ViewPort.CursorPosition.Clone();
                cursorColor = GetSolidColorBrush(Terminal.CursorState.Attributes.WebColor);
            }

            if (!Scrolling && ViewTop != TerminalTop)
                ViewTop = TerminalTop;

            var defaultTransform = drawingSession.Transform;

            PaintBackgroundLayer(drawingSession, spans);

            PaintTextLayer(drawingSession, spans, textFormat, showBlink);

            if (showCursor)
                PaintCursor(
                    drawingSession,
                    spans,
                    textFormat,
                    cursorPosition.OffsetBy(0, TerminalTop - ViewTop),
                    cursorColor
                    );
            
            drawingSession.Transform = defaultTransform;

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

        private void ProcessTextFormat(CanvasDrawingSession drawingSession, CanvasTextFormat format)
        {
            CanvasTextLayout textLayout = new CanvasTextLayout(drawingSession, "\u2560", format, 0.0f, 0.0f);
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



                //if (VtConnection != null)
                //    VtConnection.SetTerminalWindowSize(columns, rows, 800, 600);

            }
        }

        private void ResizeTerminal()
        {
            //System.Diagnostics.Debug.WriteLine("ResizeTerminal()");
            //System.Diagnostics.Debug.WriteLine("  Character size " + CharacterWidth.ToString() + "," + CharacterHeight.ToString());
            //System.Diagnostics.Debug.WriteLine("  Terminal size " + Columns.ToString() + "," + Rows.ToString());

            Terminal.ResizeView(Columns, Rows);
        }

        //public void ClearRawText()
        //{
        //    lock (_rawText)
        //    {
        //        _rawTextLength = 0;
        //        _rawTextChanged = true;
        //    }
        //}

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            //if (!Connected)
            //    return;

            var ch = char.ConvertFromUtf32((int)args.KeyCode);
            switch (ch)
            {
                case "\b":
                case "\t":
                case "\n":
                    return;

                case "\r":
                    ////LogText = "";
                    //lock(_rawText)
                    //{
                    //    _rawTextLength = 0;
                    //    _rawTextChanged = true;
                    //}
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
            //if (!Connected)
            //    return;

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

            Scrolling = false;

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = Terminal.GetKeySequence(e.Key.ToString(), controlPressed, shiftPressed);
            if(code != null)
                e.Handled = Terminal.KeyPressed(e.Key.ToString(), controlPressed, shiftPressed);

            if (ViewTop != Terminal.ViewPort.TopRow)
            {
                ViewTop = Terminal.ViewPort.TopRow;
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

            if (Terminal != null)
                Terminal.FocusIn();
        }

        private void TerminalLostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;

            if (Terminal != null)
                Terminal.FocusOut();
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

                if (newFontSize > 24)
                    newFontSize = 24;

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
                Scrolling = true;

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

            var textPosition = position.OffsetBy(0, ViewTop);

            if (/*Connected &&*/ (Terminal.UseAllMouseTracking || Terminal.CellMotionMouseTracking) && position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
                var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

                var button =
                    pointer.Properties.IsLeftButtonPressed ? 0 :
                        pointer.Properties.IsRightButtonPressed ? 1 :
                            pointer.Properties.IsMiddleButtonPressed ? 2 : 
                            3;  // No button

                Terminal.MouseMove(position.Column, position.Row, button, controlPressed, shiftPressed);

                if(button == 3 && !Terminal.UseAllMouseTracking)
                    return;
            }

            if (MouseOver != null && MouseOver == position)
                return;

            MouseOver = position;

            if (pointer.Properties.IsLeftButtonPressed && MousePressedAt != null)
            {
                TextRange newSelection;

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

            if (/*!Connected || (Connected && */!Terminal.MouseTrackingEnabled) //)
            {
                if (pointer.Properties.IsLeftButtonPressed)
                    MousePressedAt = textPosition;
                else if (pointer.Properties.IsRightButtonPressed)
                    PasteClipboard();
            }

            if (/*Connected && */position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
                var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

                var button =
                    pointer.Properties.IsLeftButtonPressed ? 0 :
                        pointer.Properties.IsRightButtonPressed ? 1 :
                            pointer.Properties.IsMiddleButtonPressed ? 2 :
                                3;  // No button

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
                    try
                    {
                        Clipboard.SetContent(dataPackage);
                    }
                    catch(Exception clipException)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to post to clipboard: " + clipException.Message);
                    }
                }
                else
                {
                    TextSelection = null;
                    canvas.Invalidate();
                }
            }

            if (/*Connected && */position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
                var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

                Terminal.MouseRelease(position.Column, position.Row, controlPressed, shiftPressed);
            }
        }

        private void PasteText(string text)
        {
            //if (!Connected)
            //    return;

            Terminal.Paste(Encoding.UTF8.GetBytes(text));
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
        public void ContentChanged()
        {
            canvas.Invalidate();
        }
    }
}
