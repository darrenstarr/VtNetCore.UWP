namespace VtNetCore.Cocoa
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using AppKit;
    using CoreGraphics;
    using CoreText;
    using Foundation;
    using ObjCRuntime;
    using VirtualTerminal;
    using VtConnect;
    using VtNetCore.VirtualTerminal.Model;

    [Register("TerminalView")]
    public class TerminalView : NSView
    {
        public VirtualTerminalController Terminal { get; set; } = new VirtualTerminalController();
        public XTermParser.DataConsumer Consumer { get; set; }
        public Connection VtConnection { get; set; }

        public double CharacterWidth { get; set; }
        public double CharacterHeight { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }

        public int ViewTop { get; set; }

        private TextRange TextSelection { get; set; }
        private TextPosition MouseOver { get; set; }
        public TextPosition MousePressedAt { get; set; }

        private bool Selecting { get; set; }

        public bool DebugMouse { get; set; }
        public bool DebugSelect { get; set; }

        public TerminalView()
        {
            Initialize();
        }

        public TerminalView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public override bool IsFlipped
        {
            get { return true; }
        }

		private void Initialize()
        {
            this.WantsLayer = true;
            this.LayerContentsRedrawPolicy = NSViewLayerContentsRedrawPolicy.OnSetNeedsDisplay;

            Consumer = new XTermParser.DataConsumer(Terminal);
            Terminal.SendData += OnSendData;
        }

		public override void SetFrameSize(CGSize newSize)
		{
            base.SetFrameSize(newSize);
            NeedsDisplay = true;
		}

		public override bool AcceptsFirstResponder()
		{
            return true;
		}

		public override bool BecomeFirstResponder()
		{
            return true;
		}

		public override bool ResignFirstResponder()
		{
            return true;
		}

		public bool Connected
        {
            get { return VtConnection != null && VtConnection.IsConnected; }
        }

        private void OnSendData(object sender, SendDataEventArgs e)
        {
            if(Connected)
            {
                Task.Run(() => VtConnection.SendData(e.Data));
            }
        }

        public bool ConnectTo(string uri, string username, string password)
        {
            if (Connected)
                return false;

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
            bool redraw = false;

            lock (Terminal)
            {
                int oldTopRow = Terminal.ViewPort.TopRow;

                Consumer.Push(e.Data);

                if (oldTopRow != Terminal.ViewPort.TopRow && oldTopRow >= ViewTop)
                    ViewTop = Terminal.ViewPort.TopRow;

                redraw = Terminal.Changed;
            }

            if (redraw)
            {
                Terminal.ClearChanges();
                InvokeOnMainThread(() =>
                {
                    NeedsDisplay = true;
                });
            }
        }

		public override void ScrollWheel(NSEvent theEvent)
		{
            int oldViewTop = ViewTop;

            System.Diagnostics.Debug.WriteLine("x:" + theEvent.ScrollingDeltaX.ToString() + ",y:" + theEvent.ScrollingDeltaY.ToString());
            ViewTop -= (int)Math.Floor(theEvent.ScrollingDeltaY) / 4;

            if (ViewTop < 0)
                ViewTop = 0;
            else if (ViewTop > Terminal.ViewPort.TopRow)
                ViewTop = Terminal.ViewPort.TopRow;

            if (oldViewTop != ViewTop)
                NeedsDisplay = true;
		}

		public override void KeyDown(NSEvent theEvent)
		{
            if(!Connected)
            {
                base.KeyDown(theEvent);
                return;
            }

            bool controlPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ControlKeyMask);
            bool shiftPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask);

            switch ((NSKey)theEvent.KeyCode)
            {
                case NSKey.UpArrow:
                    Terminal.KeyPressed("Up", controlPressed, shiftPressed);
                    break;
                case NSKey.DownArrow:
                    Terminal.KeyPressed("Down", controlPressed, shiftPressed);
                    break;
                case NSKey.LeftArrow:
                    Terminal.KeyPressed("Left", controlPressed, shiftPressed);
                    break;
                case NSKey.RightArrow:
                    Terminal.KeyPressed("Right", controlPressed, shiftPressed);
                    break;
                case NSKey.PageUp:
                    Terminal.KeyPressed("PageUp", controlPressed, shiftPressed);
                    break;
                case NSKey.PageDown:
                    Terminal.KeyPressed("PageDown", controlPressed, shiftPressed);
                    break;
                case NSKey.Home:
                    Terminal.KeyPressed("Home", controlPressed, shiftPressed);
                    break;
                case NSKey.End:
                    Terminal.KeyPressed("End", controlPressed, shiftPressed);
                    break;
                case NSKey.F1:
                    Terminal.KeyPressed("F1", controlPressed, shiftPressed);
                    break;
                case NSKey.F2:
                    Terminal.KeyPressed("F2", controlPressed, shiftPressed);
                    break;
                case NSKey.F3:
                    Terminal.KeyPressed("F3", controlPressed, shiftPressed);
                    break;
                case NSKey.F4:
                    Terminal.KeyPressed("F4", controlPressed, shiftPressed);
                    break;
                case NSKey.F5:
                    Terminal.KeyPressed("F5", controlPressed, shiftPressed);
                    break;
                case NSKey.F6:
                    Terminal.KeyPressed("F6", controlPressed, shiftPressed);
                    break;
                case NSKey.F7:
                    Terminal.KeyPressed("F7", controlPressed, shiftPressed);
                    break;
                case NSKey.F8:
                    Terminal.KeyPressed("F8", controlPressed, shiftPressed);
                    break;
                case NSKey.F9:
                    Terminal.KeyPressed("F9", controlPressed, shiftPressed);
                    break;
                case NSKey.F10:
                    Terminal.KeyPressed("F10", controlPressed, shiftPressed);
                    break;
                case NSKey.F11:
                    Terminal.KeyPressed("F11", controlPressed, shiftPressed);
                    break;
                case NSKey.F12:
                    Terminal.KeyPressed("F12", controlPressed, shiftPressed);
                    break;
                case NSKey.Tab:
                    Terminal.KeyPressed("Tab", controlPressed, shiftPressed);
                    break;
                case NSKey.Delete:
                    Terminal.KeyPressed("Back", controlPressed, shiftPressed);
                    break;

                default:
                    Terminal.KeyPressed(theEvent.Characters, false, false);
                    break;
            }

            if (ViewTop != Terminal.ViewPort.TopRow)
            {
                Terminal.ViewPort.TopRow = ViewTop;
                NeedsDisplay = true;
            }
		}

		public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            var attributes = new NSStringAttributes
            {
                ForegroundColor = NSColor.Blue,
                Font = NSFont.FromFontName("Andale Mono", 14)
            };

            ProcessTextFormat(attributes);

            NSColor.Black.SetFill();
            NSBezierPath.FillRect(dirtyRect);

            lock (Terminal)
            {
                int row = ViewTop;
                float verticalOffset = -row * (float)CharacterHeight;

                var lines = Terminal.ViewPort.GetLines(ViewTop, Rows);

                foreach (var line in lines)
                {
                    if (line == null)
                    {
                        row++;
                        continue;
                    }

                    int column = 0;

                    var xform = new NSAffineTransform();
                    xform.Scale((nfloat)(line.DoubleWidth ? 2.0 : 1.0), (nfloat)((line.DoubleHeightTop || line.DoubleHeightBottom) ? 2.0 : 1.0));
                    xform.Concat();

                    var spanStart = 0;
                    while (column < line.Count)
                    {
                        bool selected = TextSelection == null ? false : TextSelection.Contains(column, row);

                        var backgroundColor = GetBackgroundColor(line[column].Attributes, selected);

                        if (column < (line.Count - 1) && GetBackgroundColor(line[column + 1].Attributes, TextSelection == null ? false : TextSelection.Contains(column + 1, row)) == backgroundColor)
                        {
                            column++;
                            continue;
                        }

                        var rect = new CGRect(
                            spanStart * CharacterWidth,
                            ((row - (line.DoubleHeightBottom ? 1 : 0)) * CharacterHeight + verticalOffset) * (line.DoubleHeightBottom | line.DoubleHeightTop ? 0.5 : 1.0),
                            ((column - spanStart + 1) * CharacterWidth) + 0.9,
                            CharacterHeight + 0.9
                        );

                        backgroundColor.SetFill();
                        NSBezierPath.FillRect(rect);

                        column++;
                        spanStart = column;
                    }
                    xform.Invert();
                    xform.Concat();

                    row++;
                }

                row = ViewTop;
                foreach (var line in lines)
                {
                    if (line == null)
                    {
                        row++;
                        continue;
                    }

                    int column = 0;

                    var xform = new NSAffineTransform();
                    xform.Scale((nfloat)(line.DoubleWidth ? 2.0 : 1.0), (nfloat)((line.DoubleHeightTop || line.DoubleHeightBottom) ? 2.0 : 1.0));
                    xform.Concat();

                    var spanStart = 0;
                    string toDisplay = string.Empty;
                    while (column < line.Count)
                    {
                        bool selected = TextSelection == null ? false : TextSelection.Contains(column, row);
                        var foregroundColor = GetForegroundColor(line[column].Attributes, selected);

                        toDisplay += line[column].Char.ToString() + line[column].CombiningCharacters;
                        if (
                            column < (line.Count - 1) &&
                            GetForegroundColor(line[column + 1].Attributes, TextSelection == null ? false : TextSelection.Contains(column + 1, row)) == foregroundColor &&
                            line[column + 1].Attributes.Underscore == line[column].Attributes.Underscore &&
                            line[column + 1].Attributes.Reverse == line[column].Attributes.Reverse &&
                            line[column + 1].Attributes.Bright == line[column].Attributes.Bright
                            )
                        {
                            column++;
                            continue;
                        }

                        var textPosition = new CGPoint(
                            spanStart * CharacterWidth,
                            ((row - (line.DoubleHeightBottom ? 1 : 0)) * CharacterHeight + verticalOffset + attributes.Font.Descender) * (line.DoubleHeightBottom | line.DoubleHeightTop ? 0.5 : 1.0)
                        );

                        attributes.ForegroundColor = foregroundColor;
                        attributes.UnderlineStyle = line[column].Attributes.Underscore ? 1 : 0;
                        var nsStr = new NSString(toDisplay);

                        nsStr.DrawAtPoint(textPosition, attributes.Dictionary);


                        column++;
                        spanStart = column;
                        toDisplay = "";
                    }
                    xform.Invert();
                    xform.Concat();


                    row++;
                }

                if (Terminal.CursorState.ShowCursor)
                {
                    var cursorY = Terminal.ViewPort.TopRow - ViewTop + Terminal.CursorState.CurrentRow;
                    var cursorRect = new CGRect(
                        Terminal.CursorState.CurrentColumn * CharacterWidth,
                        cursorY * CharacterHeight,
                        CharacterWidth + 0.9,
                        CharacterHeight + 0.9
                    );

                    var path = NSBezierPath.FromRect(cursorRect);
                    GetForegroundColor(Terminal.CursorState.Attributes, false).Set();
                    path.Stroke();
                }
            }
        }

        private void ProcessTextFormat(NSStringAttributes attributes)
        {
            var str = new NSString("A");
            var bounds = str.BoundingRectWithSize(
                new CGSize(1000, 1000),
                0,
                attributes.Dictionary);

            if ((nfloat)CharacterWidth != bounds.Width || (nfloat)CharacterHeight != bounds.Height)
            {
                CharacterWidth = bounds.Right;
                CharacterHeight = //bounds.Bottom
                    attributes.Font.Ascender
                    - attributes.Font.Descender;
            }

            int columns = Convert.ToInt32(Math.Floor(Frame.Width / CharacterWidth));
            int rows = Convert.ToInt32(Math.Floor(Frame.Height / CharacterHeight));
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

        private static NSColor[] AttributeColors =
        {
            NSColor.FromRgb((byte)  0, (byte)  0, (byte)  0),  // Black
            NSColor.FromRgb((byte)187, (byte)  0, (byte)  0),  // Red
            NSColor.FromRgb((byte)  0, (byte)187, (byte)  0),  // Green
            NSColor.FromRgb((byte)187, (byte)187, (byte)  0),  // Yellow
            NSColor.FromRgb((byte)  0, (byte)  0, (byte)187),  // Blue
            NSColor.FromRgb((byte)187, (byte)  0, (byte)187),  // Magenta
            NSColor.FromRgb((byte)  0, (byte)187, (byte)187),  // Cyan
            NSColor.FromRgb((byte)187, (byte)187, (byte)187),  // White
            NSColor.FromRgb((byte) 85, (byte) 85, (byte) 85),  // Bright black
            NSColor.FromRgb((byte)255, (byte) 85, (byte) 85),  // Bright red
            NSColor.FromRgb((byte) 85, (byte)255, (byte) 85),  // Bright green
            NSColor.FromRgb((byte)255, (byte)255, (byte) 85),  // Bright yellow
            NSColor.FromRgb((byte) 85, (byte) 85, (byte)255),  // Bright blue
            NSColor.FromRgb((byte)255, (byte) 85, (byte)255),  // Bright Magenta
            NSColor.FromRgb((byte) 85, (byte)255, (byte)255),  // Bright cyan
            NSColor.FromRgb((byte)255, (byte)255, (byte)255),  // Bright white
        };

        private NSColor GetBackgroundColor(TerminalAttribute attribute, bool invert)
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

        private NSColor GetForegroundColor(TerminalAttribute attribute, bool invert)
        {
            var flip = Terminal.CursorState.ReverseVideoMode ^ attribute.Reverse ^ invert;

            if (flip)
                return AttributeColors[(int)attribute.BackgroundColor];

            if (attribute.Bright)
                return AttributeColors[(int)attribute.ForegroundColor + 8];

            return AttributeColors[(int)attribute.ForegroundColor];
        }

        private TextPosition ToPosition(CGPoint point)
        {
            int overColumn = (int)Math.Floor(point.X / CharacterWidth);
            if (overColumn >= Columns)
                overColumn = Columns - 1;

            int overRow = (int)Math.Floor((Frame.Height - point.Y) / CharacterHeight);
            if (overRow >= Rows)
                overRow = Rows - 1;

            return new TextPosition { Column = overColumn, Row = overRow };
        }

		public override void MouseDragged(NSEvent theEvent)
		{
            base.MouseDragged(theEvent);

            var position = ToPosition(theEvent.LocationInWindow);

            if (MouseOver != null && MouseOver == position)
                return;

            MouseOver = position;

            //if (theEvent.ButtonNumber == 0)
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

                        if (DebugSelect)
                            System.Diagnostics.Debug.WriteLine("Selection: " + TextSelection.ToString());

                        NeedsDisplay = true;
                    }
                }
            }

            if (DebugMouse)
                System.Diagnostics.Debug.WriteLine("Pointer Moved " + position.ToString());
        }

		public override void MouseExited(NSEvent theEvent)
		{
            MouseOver = null;

            if (DebugMouse)
                System.Diagnostics.Debug.WriteLine("TerminalPointerExited()");

            NeedsDisplay = true;
        }

		public override void MouseDown(NSEvent theEvent)
		{
            var position = ToPosition(theEvent.LocationInWindow);

            var textPosition = position.OffsetBy(0, ViewTop);
            if (theEvent.ButtonNumber == 0)
                MousePressedAt = textPosition;
            //else if (theEvent.ButtonNumber == 1)
            //    PasteClipboard();

            if (Connected && position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                bool controlPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ControlKeyMask);
                bool shiftPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask);

                Terminal.MousePress(position.Column, position.Row, 0, controlPressed, shiftPressed);
            }
        }

        public override void MouseUp(NSEvent theEvent)
        {
            base.MouseUp(theEvent);
        
            var position = ToPosition(theEvent.LocationInWindow);

            var textPosition = position.OffsetBy(0, ViewTop);

            //if (!pointer.Properties.IsLeftButtonPressed)
            {
                if (Selecting)
                {
                    MousePressedAt = null;
                    Selecting = false;

                    if (DebugSelect)
                        System.Diagnostics.Debug.WriteLine("Captured : " + Terminal.GetText(TextSelection.Start.Column, TextSelection.Start.Row, TextSelection.End.Column, TextSelection.End.Row));

                    var captured = Terminal.GetText(TextSelection.Start.Column, TextSelection.Start.Row, TextSelection.End.Column, TextSelection.End.Row);

                    var pasteboard = NSPasteboard.GeneralPasteboard;
                    Class[] classArray = { new Class("NSString") };

                    pasteboard.ClearContents();
                    pasteboard.WriteObjects(new NSString[] { new NSString(captured) });
                }
                else
                {
                    TextSelection = null;
                    NeedsDisplay = true;
                }
            }

            if (Connected && position.Column >= 0 && position.Row >= 0 && position.Column < Columns && position.Row < Rows)
            {
                bool controlPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ControlKeyMask);
                bool shiftPressed = theEvent.ModifierFlags.HasFlag(NSEventModifierMask.ShiftKeyMask);

                Terminal.MouseRelease(position.Column, position.Row, controlPressed, shiftPressed);
            }
        }

		public override void RightMouseUp(NSEvent theEvent)
		{
            base.RightMouseUp(theEvent);

            if (!Connected)
                return;

            var pasteboard = NSPasteboard.GeneralPasteboard;
            Class [] classArray = { new Class("NSString") };

            if(pasteboard.CanReadObjectForClasses(classArray, null))
            {
                NSObject[] objectsToPaste = pasteboard.ReadObjectsForClasses(classArray, null);
                NSString text = (NSString)objectsToPaste[0];

                var buffer = Encoding.UTF8.GetBytes(text.ToString());
                Task.Run(() =>
                {
                    VtConnection.SendData(buffer);
                });
            }
		}
	}
}
