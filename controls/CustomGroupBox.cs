
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
/// <summary>
/// Custom GroupBox Control
/// </summary>
/// <remarks></remarks>
public class CustomGroupBox : GroupBox
{

	private Color _BorderColor;
	private ushort _BorderWidth;

	private Label _lblText;
	public CustomGroupBox() : base()
	{
		_BorderColor = Color.Black;
		_BorderWidth = 3;
		this.ForeColor = Color.White;
		_lblText = new Label {
			Location = new Point(3, 3),
			AutoSize = true,
			Font = this.Font,
			ForeColor = this.ForeColor,
			Text = this.Text,
			BackColor = Color.Transparent
		};
		this.Controls.Add(_lblText);
	}

	public Color BorderColor {
		get { return _BorderColor; }
		set {
			_BorderColor = value;
			this.Invalidate();
		}
	}

	public ushort BorderWidth {
		get { return _BorderWidth; }
		set {
			_BorderWidth = value;
			this.Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		_lblText.Text = this.Text;
		_lblText.Font = this.Font;
		_lblText.ForeColor = this.ForeColor;
		Size tSize = TextRenderer.MeasureText(this.Text, this.Font);

		SolidBrush bru = default(SolidBrush);
		if (Enabled) {
			bru = new SolidBrush(this._BorderColor);
		} else {
			bru = new SolidBrush(Color.FromArgb(190, _BorderColor.R, _BorderColor.G, _BorderColor.B));
		}
		SolidBrush back = new SolidBrush(BackColor);
		e.Graphics.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(0, 0, Width, Height));

		e.Graphics.FillRectangle(bru, new Rectangle(_BorderWidth, 0, this.Width - _BorderWidth * 2, tSize.Height + 6));
		e.Graphics.FillRectangle(bru, new Rectangle(0, 0, this._BorderWidth, this.Height - _BorderWidth));
		e.Graphics.FillRectangle(bru, new Rectangle(0, this.Height - this._BorderWidth, this.Width, this._BorderWidth));
		e.Graphics.FillRectangle(bru, new Rectangle(this.Width - this._BorderWidth, 0, this._BorderWidth, this.Height - _BorderWidth));
		e.Graphics.FillRectangle(back, new Rectangle(_BorderWidth, tSize.Height + 6, this.Width - _BorderWidth * 2, this.Height - _BorderWidth - tSize.Height - 6));
		bru.Dispose();
		tSize = null;
	}

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
