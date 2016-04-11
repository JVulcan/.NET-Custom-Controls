
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class FormV2 : System.Windows.Forms.Form
{
	[DllImport("User32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	//***************** WIN32 REFERENCES *************************
	private static extern long SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
	[DllImport("User32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	private static extern void ReleaseCapture();

	const  WM_NCLBUTTONDOWN = 0xa1;
	const  HTCAPTION = 2;
	const int HTBORDER = 18;
	const int HTBOTTOM = 15;
	const int HTBOTTOMLEFT = 16;
	const int HTBOTTOMRIGHT = 17;
	const int HTLEFT = 10;
	const int HTRIGHT = 11;
	const int HTTOP = 12;
	const int HTTOPLEFT = 13;
	const int HTTOPRIGHT = 14;
	//***********************************************************
	//Timer for the fade-in and fade-out effect
	private Timer withEventsField_TimerShowHide = new Timer { Interval = 15 };
	public Timer TimerShowHide {
		get { return withEventsField_TimerShowHide; }
		set {
			if (withEventsField_TimerShowHide != null) {
				withEventsField_TimerShowHide.Tick -= Timer_Tick;
			}
			withEventsField_TimerShowHide = value;
			if (withEventsField_TimerShowHide != null) {
				withEventsField_TimerShowHide.Tick += Timer_Tick;
			}
		}
	}

	ShowHideMode TimerMode;

	bool isDialogMode = false;
	int _borderWidth;
	int _TitleHeight;
	Color _borderColor;
	Color _edgeColor;
	Color _TitleForeColor;

	Font _titleFont;
	//MinMax states for Minimize and Maximize buttons
	Color _MinMaxIconColorIdle;
	Color _MinMaxIconColorHover;

	Color _MinMaxIconBackColorHover;
	//btn exit
	Color _ExitIconColorIdle;

	Color _ExitIconBackColorIdle;
	private enum Hoverbutton
	{
		None = -1,
		Exit_ = 0,
		Maximize = 1,
		Minimize = 2
	}

	private enum IconButton
	{
		Maximize = 0,
		Minimize = 1,
		Restore = 2
	}

	public enum TitleFormAlign
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	private enum ShowHideMode
	{
		Show = 0,
		Hide = 1
	}
	// WIN32
	private enum ResizeMode
	{
		Left = FormV2.HTLEFT,
		Right = FormV2.HTRIGHT,
		Bottom = FormV2.HTBOTTOM,
		BottomLeft = FormV2.HTBOTTOMLEFT,
		BottomRight = FormV2.HTBOTTOMRIGHT,
		Top = FormV2.HTTOP,
		TopLeft = FormV2.HTTOPLEFT,
		TopRight = FormV2.HTTOPRIGHT
	}

	private struct ResizeData
	{
		public bool Activated;
		public ResizeMode Mode;
	}

	TitleFormAlign _TitleAlign;

	Hoverbutton HoverState = Hoverbutton.None;
	private const int MinTitleBarHeight = 22;
	Rectangle rectExit = new Rectangle(0, 0, 45, MinTitleBarHeight);
	Rectangle rectMinimize = new Rectangle(0, 0, 30, MinTitleBarHeight);

	Rectangle rectMaximize = new Rectangle(0, 0, 30, MinTitleBarHeight);
	// double click event Hover title bar was bugged, it never worked so
	// I re-made this behaviour in order to be able to Maximize the Form is a Double Click is performed Hover the Title
	private Timer withEventsField_timerDblClk = new Timer { Interval = SystemInformation.DoubleClickTime };
	public Timer timerDblClk {
		get { return withEventsField_timerDblClk; }
		set {
			if (withEventsField_timerDblClk != null) {
				withEventsField_timerDblClk.Tick -= TimerDBLCLK_Tick;
			}
			withEventsField_timerDblClk = value;
			if (withEventsField_timerDblClk != null) {
				withEventsField_timerDblClk.Tick += TimerDBLCLK_Tick;
			}
		}

	}

	bool _CanResize;

	ResizeData resizeInfo = new ResizeData();
	//resize rectangle area bars
	Rectangle rectResizeRight = new Rectangle(0, 0, 3, 0);
	Rectangle rectResizeBottom = new Rectangle(0, 0, 3, 0);
	Rectangle rectResizeLeft = new Rectangle(0, 0, 3, 0);
	Rectangle rectResizeBottomRight = new Rectangle(0, 0, 8, 8);
	Rectangle rectResizeBottomLeft = new Rectangle(0, 0, 8, 8);
	Rectangle rectResizeTop = new Rectangle(0, 0, 10, 8);
	Rectangle rectResizeTopLeft = new Rectangle(0, 0, 8, 8);

	Rectangle rectResizeTopRight = new Rectangle(0, 0, 8, 8);
	bool _useShowEffect = true;

	bool _useCloseEffect = true;
	// Event triggered once the fade-in effect finished
	public event OnLoadEffectFinishedEventHandler OnLoadEffectFinished;
	public delegate void OnLoadEffectFinishedEventHandler(object sender, EventArgs e);

	public FormV2() : base()
	{
		BorderWidth = 3;
		TitleHeight = 29;
		TitleFont = this.Font;
		_TitleForeColor = Color.Black;
		_borderColor = Color.Silver;
		_edgeColor = Color.Silver;
		_ExitIconColorIdle = Color.Black;
		_ExitIconBackColorIdle = Color.LightSkyBlue;
		_MinMaxIconColorHover = Color.White;
		_MinMaxIconColorIdle = Color.Black;
		_MinMaxIconBackColorHover = Color.SteelBlue;
		this.FormBorderStyle = Windows.Forms.FormBorderStyle.None;
		_TitleAlign = TitleFormAlign.Center;
		_CanResize = true;
		rectResizeTop.X = _borderWidth;
		UpdateRectPosition();
	}

	#region "Properties"
	/// <summary>
	/// Use Fade-in effect
	/// </summary>
	/// <returns></returns>
	public bool UseShowEffect {
		get { return _useShowEffect; }
		set {
			if (value != _useShowEffect) {
				_useShowEffect = value;
			}
		}
	}
	/// <summary>
	/// Use Fade-out effect
	/// </summary>
	/// <returns></returns>
	public bool UseCloseEffect {
		get { return _useCloseEffect; }
		set {
			if (value != _useCloseEffect) {
				_useCloseEffect = value;
			}
		}
	}
	/// <summary>
	/// If this is a resizable Form
	/// </summary>
	/// <returns></returns>
	public bool CanResize {
		get { return _CanResize; }
		set {
			if (value != _CanResize) {
				_CanResize = value;
				this.Invalidate();
			}
		}
	}
	/// <summary>
	/// Position of the Title text
	/// </summary>
	/// <returns></returns>
	public TitleFormAlign TitleAlign {
		get { return _TitleAlign; }
		set {
			if (value != _TitleAlign) {
				_TitleAlign = value;
				InvalidateHeader();
			}
		}
	}
	/// <summary>
	/// Background Color of the Minimize and Maximize buttons when the mouse is Hover them
	/// </summary>
	/// <returns></returns>
	public Color MinMaxIconBackColorHover {
		get { return _MinMaxIconBackColorHover; }
		set {
			if (value != _MinMaxIconBackColorHover) {
				_MinMaxIconBackColorHover = value;
				this.Invalidate(rectMaximize);
				this.Invalidate(rectMinimize);
			}
		}
	}
	/// <summary>
	/// Background Color of the Minimize and Maximize buttons when the mouse is not Hover them
	/// </summary>
	/// <returns></returns>
	public Color MinMaxIconColorIdle {
		get { return _MinMaxIconColorIdle; }
		set {
			if (value != _MinMaxIconColorIdle) {
				_MinMaxIconColorIdle = value;
				this.Invalidate(rectMaximize);
				this.Invalidate(rectMinimize);
			}
		}
	}
	/// <summary>
	/// Icon Color of the Minimize and Maximize buttons when the mouse is Hover them
	/// </summary>
	/// <returns></returns>
	public Color MinMaxIconColorHover {
		get { return _MinMaxIconColorHover; }
		set {
			if (value != _MinMaxIconColorHover) {
				_MinMaxIconColorHover = value;
				this.Invalidate(rectMaximize);
				this.Invalidate(rectMinimize);
			}
		}
	}
	/// <summary>
	/// Title Text
	/// </summary>
	/// <returns></returns>
	public new string Text {
		get { return base.Text; }
		set {
			if (value != base.Text) {
				base.Text = value;
				InvalidateHeader();
			}
		}
	}
	/// <summary>
	/// Color of the × when the mouse is not Hover it
	/// </summary>
	/// <returns></returns>
	public Color ExitIconColorIdle {
		get { return _ExitIconColorIdle; }
		set {
			if (value != _ExitIconColorIdle) {
				_ExitIconColorIdle = value;
				this.Invalidate();
			}
		}
	}
	/// <summary>
	/// Background Color of the × icon when the mouse is not Hover
	/// </summary>
	/// <returns></returns>
	public Color ExitIconBackColorIdle {
		get { return _ExitIconBackColorIdle; }
		set {
			if (value != _ExitIconBackColorIdle) {
				_ExitIconBackColorIdle = value;
				this.Invalidate();
			}
		}
	}

	public Font TitleFont {
		get { return _titleFont; }
		set {
			_titleFont = value;
			this.Invalidate();
		}
	}

	public Color TitleForeColor {
		get { return _TitleForeColor; }
		set {
			if (value != _TitleForeColor) {
				_TitleForeColor = value;
				this.Invalidate();
			}
		}
	}
	/// <summary>
	/// Height of the Title Bar
	/// </summary>
	/// <returns></returns>
	public int TitleHeight {
		get { return _TitleHeight; }
		set {
			if (value < 1)
				value = MinTitleBarHeight;
			if (value != _TitleHeight) {
				_TitleHeight = value;
				if (_TitleHeight < MinTitleBarHeight) {
					rectExit.Height = _TitleHeight;
					rectMinimize.Height = _TitleHeight;
					rectMaximize.Height = _TitleHeight;
				} else {
					rectExit.Height = MinTitleBarHeight;
					rectMinimize.Height = MinTitleBarHeight;
					rectMaximize.Height = MinTitleBarHeight;
				}
				this.Invalidate();
			}
		}
	}

	public Color BorderColor {
		get { return _borderColor; }
		set {
			if (_borderColor != value) {
				_borderColor = value;
				this.Invalidate();
			}
		}
	}

	public Color EdgeColor {
		get { return _edgeColor; }
		set {
			if (_edgeColor != value) {
				_edgeColor = value;
				this.Invalidate();
			}
		}
	}

	public int BorderWidth {
		get { return _borderWidth; }
		set {
			if (value < 1)
				value = 3;
			if (value != _borderWidth) {
				_borderWidth = value;
				rectResizeBottomLeft.Size = new Size(Math.Max(8, value), Math.Max(8, value));
				rectResizeBottomRight.Size = rectResizeBottomLeft.Size;
				rectResizeTop.X = value;
				rectResizeTopLeft.Width = value;
				rectResizeTopRight.Width = value;
				UpdateRectPosition();
				this.Invalidate();
			}
		}
	}

	public new bool ControlBox {
		get { return base.ControlBox; }
		set {
			if (value != base.ControlBox) {
				base.ControlBox = value;
				this.Invalidate();
			}
		}
	}

	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new FormBorderStyle FormBorderStyle {
		get { return base.FormBorderStyle; }
		set { base.FormBorderStyle = Windows.Forms.FormBorderStyle.None; }
	}

	public new bool MaximizeBox {
		get { return base.MaximizeBox; }
		set {
			if (value != base.MaximizeBox) {
				base.MaximizeBox = value;
				if (value == false && !MinimizeBox)
					rectMaximize.Height = 0;
				else
					rectMaximize.Height = (_TitleHeight < MinTitleBarHeight ? _TitleHeight : MinTitleBarHeight);
			}
		}
	}

	public new bool MinimizeBox {
		get { return base.MinimizeBox; }
		set {
			if (value != base.MinimizeBox) {
				base.MinimizeBox = value;
				if (value == false)
					rectMinimize.Height = 0;
				else
					rectMinimize.Height = (_TitleHeight < MinTitleBarHeight ? _TitleHeight : MinTitleBarHeight);
			}
		}
	}

	public new object ShowDialog()
	{
		isDialogMode = true;
		return base.ShowDialog;
	}
	#endregion


	/// <summary>
	/// Updates lthe position of the Exit button
	/// </summary>
	/// <remarks></remarks>
	private void UpdateRectPosition()
	{
		rectExit.Location = new Point(this.Width - rectExit.Width - BorderWidth - 1, 0);
		rectMaximize.Location = new Point(rectExit.X - 1 - rectMaximize.Width, rectExit.Y);
		rectMinimize.Location = new Point(rectMaximize.X - 1 - rectMinimize.Width, rectExit.Y);
		//resize bars
		rectResizeRight.Location = new Point(this.Width - _borderWidth, 0);
		rectResizeBottom.Location = new Point(0, this.Height - _borderWidth);
		rectResizeBottomLeft.Location = new Point(0, Height - rectResizeBottomLeft.Height);
		rectResizeBottomRight.Location = new Point(this.Width - rectResizeBottomRight.Width, this.Height - rectResizeBottomRight.Height);
		rectResizeTopRight.Location = new Point(this.Width - rectResizeTopRight.Width, 0);
	}

	#region "Paint"
	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
	{
		base.OnPaint(e);

		using (Graphics g = e.Graphics) {
			Pen penEdge = new Pen(this._edgeColor);
			SolidBrush brush = new SolidBrush(this.BorderColor);
			g.FillRectangle(brush, new Rectangle(0, 0, this.Width, this.TitleHeight));
			//barra título
			if (_CanResize && WindowState == FormWindowState.Normal) {
				g.FillRectangle(brush, new Rectangle(0, this.TitleHeight, BorderWidth, this.Height - this.TitleHeight));
				//izquierda
				g.FillRectangle(brush, new Rectangle(BorderWidth, Height - BorderWidth, Width - BorderWidth, BorderWidth));
				//abajo
				g.FillRectangle(brush, new Rectangle(Width - BorderWidth, TitleHeight, BorderWidth, Height - TitleHeight - BorderWidth));
				//derecha
			}
			//edge
			g.DrawRectangle(penEdge, new Rectangle(0, 0, this.Width - 1, this.Height - 1));

			Size titleMeasure = TextRenderer.MeasureText(this.Text, this.TitleFont);
			int pointX = 0;
			switch (this.TitleAlign) {
				case TitleFormAlign.Center:
					pointX = (this.Width - titleMeasure.Width) / 2;
					break;
				case TitleFormAlign.Left:
					if (this.ShowIcon && this.Icon != null) {
						pointX = _borderWidth + 1 + 16 + 1;
					} else {
						pointX = _borderWidth + 1;
					}
					break;
				case TitleFormAlign.Right:
					pointX = rectMinimize.X - titleMeasure.Width - 1;
					break;
			}
			g.DrawString(this.Text, this.TitleFont, new SolidBrush(this.TitleForeColor), pointX, (this.TitleHeight - titleMeasure.Height) / 2);

			//draw exit button
			if (this.ControlBox) {
				SolidBrush _backColor = new SolidBrush(this._ExitIconBackColorIdle);
				SolidBrush colorIcon = new SolidBrush(this._ExitIconColorIdle);
				UpdateRectPosition();
				if (HoverState == Hoverbutton.Exit_) {
					_backColor.Color = Color.Firebrick;
					colorIcon.Color = Color.White;
				}
				g.FillRectangle(_backColor, rectExit);
				Size IconMeasure = TextRenderer.MeasureText("×", new Font("Microsoft Sans Serif", 14f));
				g.DrawString("×", new Font("Microsoft Sans Serif", 14f), colorIcon, new Point((rectExit.Width - IconMeasure.Width) / 2 + rectExit.X + 2, (rectExit.Height - IconMeasure.Height) / 2 + rectExit.Y));

				Rectangle rectIcon = new Rectangle(0, 0, 10, 10);
				if (this.MaximizeBox) {
					_backColor = new SolidBrush(this.BackColor);
					colorIcon = new SolidBrush(this._MinMaxIconColorIdle);
					if (HoverState == Hoverbutton.Maximize) {
						_backColor.Color = _MinMaxIconBackColorHover;
						colorIcon.Color = this._MinMaxIconColorHover;
					}
					if (_backColor.Color != this.BackColor)
						g.FillRectangle(_backColor, rectMaximize);
					rectIcon.X = rectMaximize.X + Math.Floor((rectMaximize.Width - rectIcon.Width) / 2f);
					rectIcon.Y = rectMaximize.Y + Math.Floor((rectMaximize.Height - rectIcon.Height) / 2f);
					DrawIcon((WindowState == FormWindowState.Maximized ? IconButton.Restore : IconButton.Maximize), colorIcon.Color, ref rectIcon, ref g);
				//hay que dibujarlo deshabilitado
				} else if (this.MinimizeBox) {
					colorIcon.Color = Color.Gray;
					rectIcon.X = rectMaximize.X + Math.Floor((rectMaximize.Width - rectIcon.Width) / 2f);
					rectIcon.Y = rectMaximize.Y + Math.Floor((rectMaximize.Height - rectIcon.Height) / 2f);
					DrawIcon(IconButton.Maximize, colorIcon.Color, ref rectIcon, ref g);
				}

				if (this.MinimizeBox) {
					_backColor = new SolidBrush(this.BackColor);
					colorIcon = new SolidBrush(this._MinMaxIconColorIdle);
					if (HoverState == Hoverbutton.Minimize) {
						_backColor.Color = _MinMaxIconBackColorHover;
						colorIcon.Color = this._MinMaxIconColorHover;
					}
					if (_backColor.Color != this.BackColor)
						g.FillRectangle(_backColor, rectMinimize);
					rectIcon.X = rectMinimize.X + Math.Floor((rectMinimize.Width - rectIcon.Width) / 2f);
					rectIcon.Y = rectMinimize.Y + Math.Floor((rectMinimize.Height - rectIcon.Height) / 2f);
					DrawIcon(IconButton.Minimize, colorIcon.Color, ref rectIcon, ref g);
				}
				_backColor.Dispose();
				colorIcon.Dispose();
			}

			//draw icon
			if (this.ShowIcon && this.Icon != null) {
				g.DrawIcon(this.Icon, new Rectangle(_borderWidth + 1, (_TitleHeight - 16) / 2, 16, 16));
			}

			brush.Dispose();
			penEdge.Dispose();

		}
	}
	#endregion
	// There was a bug when at maximize the Form used all the screen (even Hover the taskbar XD)
	// so this is the fix for that bug

	protected override void WndProc(ref System.Windows.Forms.Message m)
	{
		bool SkipMessage = false;
		switch (m.Msg) {
			case 0x24:
				WmGetMinMaxInfo(m.HWnd, m.LParam);
				SkipMessage = true;
				break;
		}
		if (!SkipMessage) {
			base.WndProc(m);
		}
	}

	private struct MINMAXINFO
	{
		public Point ptReserved;
		public Point ptMaxSize;
		public Point ptMaxPosition;
		public Point ptMinTrackSize;
		public Point ptMaxTrackSize;
	}

	private void WmGetMinMaxInfo(IntPtr hWnd, IntPtr lParam)
	{
		MINMAXINFO mmi = default(MINMAXINFO);
		mmi = Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
		Screen monitor = Screen.FromHandle(hWnd);

		Rectangle rectWorkArea = monitor.WorkingArea;
		Rectangle rectMonitor = monitor.Bounds;

		mmi.ptMaxPosition.X = Math.Abs(rectWorkArea.Left - rectMonitor.Left);
		mmi.ptMaxPosition.Y = Math.Abs(rectWorkArea.Top - rectMonitor.Top);
		mmi.ptMaxSize.X = Math.Abs(rectWorkArea.Right - rectMonitor.Left);
		mmi.ptMaxSize.Y = Math.Abs(rectWorkArea.Bottom - rectMonitor.Top);
		mmi.ptMinTrackSize.X = MinimumSize.Width;
		mmi.ptMinTrackSize.Y = MinimumSize.Height;
		mmi.ptMaxTrackSize.X = MaximumSize.Width;
		mmi.ptMaxTrackSize.Y = MaximumSize.Height;

		Marshal.StructureToPtr(mmi, lParam, true);
	}

	// Implementation of events

	protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (this.ControlBox & !resizeInfo.Activated) {
			//Exit button
			if (IntersectsWithRect(ref e, ref rectExit)) {
				if (HoverState != Hoverbutton.Exit_) {
					Hoverbutton previous = HoverState;
					HoverState = Hoverbutton.Exit_;
					InvalidateButton(previous);
					this.Invalidate(rectExit);
				}
			} else if (this.MaximizeBox && IntersectsWithRect(ref e, ref rectMaximize)) {
				if (HoverState != Hoverbutton.Maximize) {
					Hoverbutton previous = HoverState;
					HoverState = Hoverbutton.Maximize;
					InvalidateButton(previous);
					this.Invalidate(rectMaximize);
				}
			} else if (this.MinimizeBox && IntersectsWithRect(ref e, ref rectMinimize)) {
				if (HoverState != Hoverbutton.Minimize) {
					Hoverbutton previous = HoverState;
					HoverState = Hoverbutton.Minimize;
					InvalidateButton(previous);
					this.Invalidate(rectMinimize);
				}
			} else {
				if (HoverState != Hoverbutton.None) {
					UpdatePrevious();
				}
			}
		}

		//change cursor icon and do resize
		if (WindowState == FormWindowState.Normal && _CanResize) {
			resizeInfo.Activated = true;
			if (IntersectsWithRect(ref e, ref rectResizeBottomRight)) {
				Cursor = Cursors.SizeNWSE;
				resizeInfo.Mode = ResizeMode.BottomRight;
			} else if (IntersectsWithRect(ref e, ref rectResizeBottomLeft)) {
				Cursor = Cursors.SizeNESW;
				resizeInfo.Mode = ResizeMode.BottomLeft;
			} else if (IntersectsWithRect(ref e, ref rectResizeTopLeft)) {
				Cursor = Cursors.SizeNWSE;
				resizeInfo.Mode = ResizeMode.TopLeft;
			} else if (IntersectsWithRect(ref e, ref rectResizeTopRight)) {
				Cursor = Cursors.SizeNESW;
				resizeInfo.Mode = ResizeMode.TopRight;
			} else if (IntersectsWithRect(ref e, ref rectResizeRight)) {
				Cursor = Cursors.SizeWE;
				resizeInfo.Mode = ResizeMode.Right;
			} else if (IntersectsWithRect(ref e, ref rectResizeLeft)) {
				Cursor = Cursors.SizeWE;
				resizeInfo.Mode = ResizeMode.Left;
			} else if (IntersectsWithRect(ref e, ref rectResizeBottom)) {
				Cursor = Cursors.SizeNS;
				resizeInfo.Mode = ResizeMode.Bottom;
			} else if (IntersectsWithRect(ref e, ref rectResizeTop)) {
				Cursor = Cursors.SizeNS;
				resizeInfo.Mode = ResizeMode.Top;
			} else {
				Cursor = Cursors.Default;
				resizeInfo.Activated = false;
			}
		}
	}

	protected override void OnMouseLeave(System.EventArgs e)
	{
		Cursor = Cursors.Default;
		base.OnMouseLeave(e);
	}
	// Maximize at Double Click! (yeah! it never has worked using FormStyle->None)
	protected override void OnDoubleClick(System.EventArgs e)
	{
		base.OnDoubleClick(e);
		Point CurPos = new Point(Windows.Forms.Cursor.Position.X - this.Left, Windows.Forms.Cursor.Position.Y - this.Top);
		if ((MaximizeBox) && ((WindowState == FormWindowState.Normal && IntersectsWithRect(ref CurPos, ref new Rectangle(_borderWidth, rectResizeTop.Height + 1, rectMinimize.X - _borderWidth, _TitleHeight - rectResizeTop.Height))) | (WindowState == FormWindowState.Maximized && IntersectsWithRect(ref CurPos, ref new Rectangle(_borderWidth, 0, rectMinimize.X - _borderWidth, _TitleHeight))))) {
			if (WindowState == FormWindowState.Normal) {
				WindowState = FormWindowState.Maximized;
			} else if (WindowState == FormWindowState.Maximized) {
				WindowState = FormWindowState.Normal;
			}
			MouseEventArgs f = new MouseEventArgs(Windows.Forms.MouseButtons.Left, 1, CurPos.X, CurPos.Y, 0);
			OnMouseMove(f);
			//pass the event to the user's implementation
		}
	}

	protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
	{
		base.OnMouseClick(e);
	}

	protected override void OnResize(System.EventArgs e)
	{
		//size of resize bars
		rectResizeRight.Size = new Size(BorderWidth, Height);
		rectResizeLeft.Size = new Size(BorderWidth, Height);
		rectResizeBottom.Size = new Size(Width, BorderWidth);
		UpdateRectPosition();
		rectResizeTop.Size = new Size(rectMinimize.X - _borderWidth, 8);
		base.OnResize(e);
		this.Invalidate();
	}

	// Here is the trick to generate a DoubleClick function from ZERO xD
	// or just behave like a normal click
	Rectangle CurPos1;
	Rectangle CurPos2;
	protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
	{
		if (IntersectsWithRect(ref e, ref rectMinimize))
			CurPos1 = rectMinimize;
		else
			if (IntersectsWithRect(ref e, ref rectMaximize))
				CurPos1 = rectMaximize;
			else
				if (IntersectsWithRect(ref e, ref rectExit))
					CurPos1 = rectExit;
		base.OnMouseDown(e);
		if (WindowState == FormWindowState.Normal) {
			if (timerDBLCLK_running) {
				timerDBLCLK_running = false;
				timerDblClk.Stop();
				OnDoubleClick(EventArgs.Empty);
			} else {
				timerDBLCLK_running = true;
				timerDblClk.Start();
			}
		}

		if (e.X >= this.BorderWidth && e.X <= this.Width - this.BorderWidth && e.Y >= rectResizeTop.Height && e.Y <= this.TitleHeight) {
			if (WindowState == FormWindowState.Normal && !IntersectsWithRect(ref e, ref rectExit) && !IntersectsWithRect(ref e, ref rectMaximize) && !IntersectsWithRect(ref e, ref rectMinimize)) {
				//move window
				ReleaseCapture();
				SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
			}
		} else if (resizeInfo.Activated) {
			ReleaseCapture();
			SendMessage(Handle, WM_NCLBUTTONDOWN, resizeInfo.Mode, 0);
		}
	}

	protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
	{
		if (IntersectsWithRect(ref e, ref rectMinimize))
			CurPos2 = rectMinimize;
		else
			if (IntersectsWithRect(ref e, ref rectMaximize))
				CurPos2 = rectMaximize;
			else
				if (IntersectsWithRect(ref e, ref rectExit))
					CurPos2 = rectExit;
		if (!resizeInfo.Activated && CurPos1 == CurPos2) {
			switch (HoverState) {
				case Hoverbutton.Exit_:
					this.Close();
					break;
				case Hoverbutton.Maximize:
					if (WindowState == FormWindowState.Maximized) {
						WindowState = FormWindowState.Normal;
						this.Invalidate();
					} else {
						Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
						WindowState = FormWindowState.Maximized;
						this.Invalidate(rect);
					}
					break;
				case Hoverbutton.Minimize:
					WindowState = FormWindowState.Minimized;
					break;
			}
			HoverState = Hoverbutton.None;
			//OnMouseMove(e)
		}
		base.OnMouseUp(e);
		resizeInfo.Activated = false;
		Cursor = Cursors.Default;
		OnMouseMove(e);
	}

	protected override void OnClosed(System.EventArgs e)
	{
		timerDblClk.Stop();
		TimerShowHide.Stop();
		if (UseCloseEffect) {
			do {
				this.Opacity -= 0.1f;
				Threading.Thread.Sleep(20);
			} while (!(this.Opacity == 0f));
		}
		base.OnClosed(e);
	}

	protected override void OnLoad(System.EventArgs e)
	{
		if (UseShowEffect) {
			this.Opacity = 0.1f;
			TimerMode = ShowHideMode.Show;
			TimerShowHide.Start();
		}
		base.OnLoad(e);
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		if (TimerMode == ShowHideMode.Show && this.Opacity == 1f) {
			TimerShowHide.Stop();
			if (OnLoadEffectFinished != null) {
				OnLoadEffectFinished(this, EventArgs.Empty);
			}
		} else if (TimerMode == ShowHideMode.Hide && this.Opacity == 0.1f) {
			TimerShowHide.Stop();
		}
		switch (TimerMode) {
			case ShowHideMode.Hide:
				this.Opacity -= 0.1f;
				break;
			case ShowHideMode.Show:
				this.Opacity += 0.1f;
				break;
		}
	}

	/// <summary>
	/// Indicates if the MouseEventArgs position intersects with the  provided rectangle position
	/// </summary>
	/// <param name="e">Arguments of Mouse to analize</param>
	/// <returns></returns>
	/// <remarks></remarks>
	private object IntersectsWithRect(ref System.Windows.Forms.MouseEventArgs e, ref Rectangle rect)
	{
		if (e.X >= rect.X && e.X <= rect.X + rect.Width && e.Y >= rect.Y && e.Y <= rect.Y + rect.Height)
			return true;
		return false;
	}

	private object IntersectsWithRect(ref Point CursorPosition, ref Rectangle rect)
	{
		if (CursorPosition.X >= rect.X && CursorPosition.X <= rect.X + rect.Width && CursorPosition.Y >= rect.Y && CursorPosition.Y <= rect.Y + rect.Height)
			return true;
		return false;
	}

	// Here the ICON DRAW is made, just using lines and rectangles :3
	private void DrawIcon(IconButton icon, Color color, ref Rectangle rect, ref Graphics g)
	{
		Pen pen = new Pen(color);
		switch (icon) {
			case IconButton.Restore:
				//box
				g.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + 2, 8, 8));
				g.DrawLine(pen, new Point(rect.X + 1, rect.Y + 3), new Point(rect.X + 9 - 2, rect.Y + 3));
				//outside
				g.DrawLine(pen, new Point(rect.X + 3, rect.Y + 0), new Point(rect.X + 10, rect.Y + 0));
				g.DrawLine(pen, new Point(rect.X + 10, rect.Y + 1), new Point(rect.X + 10, rect.Y + 7));
				break;
			case IconButton.Minimize:
				g.FillRectangle(pen.Brush, new Rectangle(rect.X + 1, rect.Y + 7, 8, 2));
				break;
			case IconButton.Maximize:
				g.DrawRectangle(pen, new Rectangle(rect.X + 0, rect.Y + 1, 10, 8));
				g.DrawLine(pen, new Point(rect.X + 1, rect.Y + 2), new Point(rect.X + 9, rect.Y + 2));
				break;
		}
	}

	private void UpdatePrevious()
	{
		Hoverbutton previous = HoverState;
		HoverState = Hoverbutton.None;
		switch (previous) {
			case Hoverbutton.Maximize:
				this.Invalidate(rectMaximize);
				break;
			case Hoverbutton.Minimize:
				this.Invalidate(rectMinimize);
				break;
			case Hoverbutton.Exit_:
				this.Invalidate(rectExit);
				break;
		}
	}

	private void InvalidateButton(Hoverbutton state)
	{
		switch (state) {
			case Hoverbutton.Maximize:
				Invalidate(rectMaximize);
				break;
			case Hoverbutton.Minimize:
				Invalidate(rectMinimize);
				break;
			case Hoverbutton.Exit_:
				Invalidate(rectExit);
				break;
		}
	}

	private void InvalidateHeader()
	{
		this.Invalidate(new Rectangle(0, 0, this.Width, this.TitleHeight));
	}

	bool timerDBLCLK_running = false;
	private void TimerDBLCLK_Tick(object sender, EventArgs e)
	{
		timerDBLCLK_running = false;
		timerDblClk.Stop();
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================