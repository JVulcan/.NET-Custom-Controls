
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
/// <summary>
/// Special button that shows a description when the mouse is over it
/// </summary>
/// <remarks>the button should be large enough to fit the whole description</remarks>
public class DescButton : Control
{
	//NM = Dont show
	//shared
		//NM
	private static uint Count = 0;
	//back
	private Font _BackFont;
	private string _BackText;
	private Color _BackColor;
	private Color _BackForeColor;
	//front
	private Font _FrontFont;
	private string _FrontText;
	private Color _FrontColor;
	private Color _FrontForeColor;
	//misc
	private int _BorderWidth;
	//objs:
	private Panel withEventsField__BackPanel;
	private Panel _BackPanel {
		get { return withEventsField__BackPanel; }
		set {
			if (withEventsField__BackPanel != null) {
				withEventsField__BackPanel.MouseDown -= MouseDown;
				withEventsField__BackPanel.MouseUp -= MouseUp;
				withEventsField__BackPanel.MouseLeave -= MouseLeave;
				withEventsField__BackPanel.Click -= OnClick;
			}
			withEventsField__BackPanel = value;
			if (withEventsField__BackPanel != null) {
				withEventsField__BackPanel.MouseDown += MouseDown;
				withEventsField__BackPanel.MouseUp += MouseUp;
				withEventsField__BackPanel.MouseLeave += MouseLeave;
				withEventsField__BackPanel.Click += OnClick;
			}
		}
		//NM
	}
	private Panel withEventsField__FrontPanel;
	private Panel _FrontPanel {
		get { return withEventsField__FrontPanel; }
		set {
			if (withEventsField__FrontPanel != null) {
				withEventsField__FrontPanel.MouseDown -= MouseDown;
				withEventsField__FrontPanel.MouseUp -= MouseUp;
				withEventsField__FrontPanel.MouseEnter -= MouseEnter;
				withEventsField__FrontPanel.Click -= OnClick;
			}
			withEventsField__FrontPanel = value;
			if (withEventsField__FrontPanel != null) {
				withEventsField__FrontPanel.MouseDown += MouseDown;
				withEventsField__FrontPanel.MouseUp += MouseUp;
				withEventsField__FrontPanel.MouseEnter += MouseEnter;
				withEventsField__FrontPanel.Click += OnClick;
			}
		}
		//NM
	}
	private Label withEventsField__BackLabel;
	private Label _BackLabel {
		get { return withEventsField__BackLabel; }
		set {
			if (withEventsField__BackLabel != null) {
				withEventsField__BackLabel.MouseDown -= MouseDown;
				withEventsField__BackLabel.MouseUp -= MouseUp;
				withEventsField__BackLabel.MouseLeave -= MouseLeave;
				withEventsField__BackLabel.Click -= OnClick;
			}
			withEventsField__BackLabel = value;
			if (withEventsField__BackLabel != null) {
				withEventsField__BackLabel.MouseDown += MouseDown;
				withEventsField__BackLabel.MouseUp += MouseUp;
				withEventsField__BackLabel.MouseLeave += MouseLeave;
				withEventsField__BackLabel.Click += OnClick;
			}
		}
		//NM
	}
	private Label withEventsField__FrontLabel;
	private Label _FrontLabel {
		get { return withEventsField__FrontLabel; }
		set {
			if (withEventsField__FrontLabel != null) {
				withEventsField__FrontLabel.MouseDown -= MouseDown;
				withEventsField__FrontLabel.MouseUp -= MouseUp;
				withEventsField__FrontLabel.MouseEnter -= MouseEnter;
				withEventsField__FrontLabel.Click -= OnClick;
			}
			withEventsField__FrontLabel = value;
			if (withEventsField__FrontLabel != null) {
				withEventsField__FrontLabel.MouseDown += MouseDown;
				withEventsField__FrontLabel.MouseUp += MouseUp;
				withEventsField__FrontLabel.MouseEnter += MouseEnter;
				withEventsField__FrontLabel.Click += OnClick;
			}
		}
		//NM
	}
	//general:
	private Timer withEventsField__timer;
	private Timer _timer {
		get { return withEventsField__timer; }
		set {
			if (withEventsField__timer != null) {
				withEventsField__timer.Tick -= TimerTick;
			}
			withEventsField__timer = value;
			if (withEventsField__timer != null) {
				withEventsField__timer.Tick += TimerTick;
			}
		}
		//NM
	}
	private bool _over;

	private int _velocity;
	public DescButton() : base()
	{
		updateVelocity();
		Count += 1;
		this.Name = "DescButton" + Count;
		_BackColor = Color.Black;
		_BackText = "Description" + Count;
		_BackFont = this.Font;
		_BackForeColor = Color.White;
		_FrontText = "DescButton" + Count;
		_FrontFont = this.Font;
		_FrontForeColor = this.ForeColor;
		_FrontColor = this.BackColor;
		_over = false;
		_timer = new Timer { Interval = 1 };
		_BorderWidth = 1;
		Size tSize = TextRenderer.MeasureText(_FrontText, _FrontFont);
		_BackPanel = new Panel {
			Size = this.Size,
			BackColor = _BackColor,
			Location = new Point(0, 0)
		};
		_BackLabel = new Label {
			Font = _BackFont,
			AutoSize = false,
			ForeColor = _BackForeColor,
			TextAlign = ContentAlignment.MiddleCenter,
			Size = new Size(_BackPanel.Width - _BorderWidth * 2, _BackPanel.Height - tSize.Height),
			Location = new Point(_BorderWidth, tSize.Height + 1),
			Text = _BackText
		};
		_BackPanel.Controls.Add(_BackLabel);
		_FrontPanel = new Panel {
			Size = new Size(this.Size.Width - _BorderWidth * 2, this.Size.Height - _BorderWidth * 2),
			BackColor = _FrontColor,
			Location = new Point(1, 1)
		};
		_FrontLabel = new Label {
			Font = _FrontFont,
			AutoSize = true,
			ForeColor = _FrontForeColor,
			Location = new Point((_FrontPanel.Width - tSize.Width) / 2, (_FrontPanel.Height - tSize.Height) / 2)
		};
		_FrontPanel.Controls.Add(_FrontLabel);
		this.Controls.AddRange({
			_FrontPanel,
			_BackPanel
		});
	}

	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
	{
		base.OnPaint(e);
		Size tSize = TextRenderer.MeasureText(_FrontText, _FrontFont);
		//BACK
		_BackPanel.Size = this.Size;
		_BackPanel.BackColor = _BackColor;
		_BackLabel.Font = _BackFont;
		_BackLabel.Text = _BackText;
		_BackLabel.ForeColor = _BackForeColor;
		//FRONT
		_FrontPanel.Size = new Size(_BackPanel.Size.Width - _BorderWidth * 2, _BackPanel.Size.Height - _BorderWidth * 2);
		_FrontPanel.Location = new Point(_BorderWidth, _BorderWidth);
		_FrontPanel.BackColor = this.BackColor;
		_FrontLabel.Font = _FrontFont;
		_FrontLabel.Text = _FrontText;
		_FrontLabel.ForeColor = this.ForeColor;
		_FrontLabel.Location = new Point((_FrontPanel.Width - tSize.Width) / 2, (_FrontPanel.Height - tSize.Height) / 2);

		_BackLabel.Location = new Point(_BorderWidth, tSize.Height + 1);
		_BackLabel.Size = new Size(_BackPanel.Width - _BorderWidth * 2, _BackPanel.Height - tSize.Height - _BorderWidth);
	}

	public new void MouseDown(object sender, EventArgs e)
	{
		_BackPanel.BackColor = Color.FromArgb(255, Convert.ToInt32(_BackColor.R / 2), Convert.ToInt32(_BackColor.G / 2), Convert.ToInt32(_BackColor.B / 2));
		_FrontLabel.ForeColor = Color.FromArgb(255, Convert.ToInt32(_FrontForeColor.R / 2), Convert.ToInt32(_FrontForeColor.G / 2), Convert.ToInt32(_FrontForeColor.B / 2));
		base.OnMouseDown(e);
	}

	public new void MouseUp(object sender, EventArgs e)
	{
		_BackPanel.BackColor = _BackColor;
		_FrontLabel.ForeColor = _FrontForeColor;
		base.OnMouseUp(e);
	}

	public new void MouseEnter(object sender, EventArgs e)
	{
		_over = true;
		_timer.Start();
	}

	public new void MouseLeave(object sender, EventArgs e)
	{
		_over = false;
		_timer.Start();
	}

	public new void OnClick(object sender, EventArgs e)
	{
		base.OnClick(e);
	}

	protected override void OnResize(System.EventArgs e)
	{
		updateVelocity();
		base.OnResize(e);
	}

	private void TimerTick(Timer sender, EventArgs e)
	{
		if (_over) {
			if (_FrontLabel.Top > 0) {
				_FrontPanel.Height -= _velocity;
				_FrontLabel.Top -= Convert.ToInt32(_velocity / 2);
			} else {
				_FrontLabel.Top = 0;
				_FrontPanel.Height = _FrontLabel.Height + 1;
				sender.Stop();
			}
		} else {
			if (_FrontPanel.Height < _BackPanel.Height) {
				_FrontPanel.Height += _velocity;
				_FrontLabel.Top += Convert.ToInt32(_velocity / 2);
			} else {
				_FrontPanel.Height = _BackPanel.Height - _BorderWidth * 2;
				_FrontLabel.Top = (_FrontPanel.Height - _FrontLabel.Height) / 2;
				sender.Stop();
			}
		}
	}

	private void updateVelocity()
	{
		this._velocity = Convert.ToInt32(this.Height / 20);
	}

	public int BorderWidth {
		get { return _BorderWidth; }
		set {
			if (value > 0) {
				if (value != _BorderWidth) {
					_BorderWidth = value;
					this.Invalidate();
				}
			} else {
				_BorderWidth = 1;
			}
		}
	}
	//All properties that starts with:
	// BACK = are properties that modify the back part of the control (the description text, color, forecolor)
	// FRONT = are properties that modify the front part of the control (title text, color, forecolor)
	public Color BackGColor {
		get { return _BackColor; }
		set {
			_BackColor = value;
			_BackPanel.BackColor = _BackColor;
		}
	}

	public string BackText {
		get { return _BackText; }
		set {
			_BackText = value;
			_BackLabel.Text = _BackText;
		}
	}

	public Font BackFont {
		get { return _BackFont; }
		set {
			_BackFont = value;
			_BackLabel.Font = value;
		}
	}

	public Color BackForeColor {
		get { return _BackForeColor; }
		set {
			_BackForeColor = value;
			_BackLabel.ForeColor = value;
		}
	}

	public Color FrontColor {
		get { return _FrontColor; }
		set {
			_FrontColor = value;
			this.BackColor = value;
			_FrontPanel.BackColor = value;
		}
	}

	public string FrontText {
		get { return _FrontText; }
		set {
			_FrontText = value;
			this.Text = value;
			_FrontLabel.Text = _FrontText;
			CenterLabel(_FrontLabel);
		}
	}

	public Font FrontFont {
		get { return _FrontFont; }
		set {
			_FrontFont = value;
			_FrontLabel.Font = _FrontFont;
			CenterLabel(_FrontLabel);
		}
	}

	public Color FrontForeColor {
		get { return _FrontForeColor; }
		set {
			_FrontForeColor = value;
			this.ForeColor = value;
			_FrontLabel.ForeColor = value;
		}
	}

	private void CenterLabel(Label lbl)
	{
		Size tSize = TextRenderer.MeasureText(lbl.Text, lbl.Font);
		lbl.Location = new Point((lbl.Parent.Width - tSize.Width) / 2, (lbl.Parent.Height - tSize.Height) / 2);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
