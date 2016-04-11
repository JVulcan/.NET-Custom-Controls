
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

public class BooleanControl : Control
{

	//
	//       WARNING !!
	//
	//       ROUNDED TYPE IS UGLY AND UNSUITABLE FOR WORK
	//       THE FORMS "ENGINE" DOESNT FIT WITH I WAS TRYING TO DO FOR THE ROUNDED ONE
	//       BUT YOU CAN CHECK IT OUT

	public enum BooleanControlStyles
	{
		Rounded = 0,
		SquareText = 1
	}

	BooleanControlStyles _style;
	Timer _timer;
	bool _value;
	Rectangle Rectangle;
	byte state = 0;
	float velocity;
	string _txtTrue;
	string _txtFalse;

	bool _hover;
	/// <summary>
	/// Triggered when the value of Control has changed.
	/// </summary>
	/// <param name="sender">original Object that changed its value and triggered the event</param>
	/// <remarks></remarks>
	public event ValueChangedEventHandler ValueChanged;
	public delegate void ValueChangedEventHandler(object sender, EventArgs e);

	public BooleanControl() : base()
	{
		_txtFalse = "No";
		_txtTrue = "Yes";
		_hover = false;
		Rectangle = new Rectangle(0, 0, this.Width * 0.5f, this.Height);
		UpdateVelocity();
		_timer = new Timer { Interval = 10 };
		_timer.Tick += Timer_Tick;
		_value = false;
		BooleanControlStyle = BooleanControlStyles.SquareText;
	}

	// just for rounded type
	private void UpdateVelocity()
	{
		//the idea is it travels in 500 milliseconds
		velocity = this.Width * 0.5f / 20f;
	}

	public BooleanControlStyles BooleanControlStyle {
		get { return _style; }
		set {
			if (value != _style) {
				_style = value;
				this.Invalidate();
			}
		}
	}

	/// <summary>
	/// (SquareText-Style only) What the Text says when the value represents FALSE (default: NO)
	/// </summary>
	/// <returns></returns>
	public string TextFalse {
		get { return _txtFalse; }
		set {
			_txtFalse = value;
			this.Invalidate();
		}
	}
	/// <summary>
	/// (SquareText-Style only) What the Text says when the value represents TRUE (default: YES)
	/// </summary>
	/// <returns></returns>
	public string TextTrue {
		get { return _txtTrue; }
		set {
			_txtTrue = value;
			this.Invalidate();
		}
	}
	/// <summary>
	/// Actual value (true or false)
	/// </summary>
	/// <returns></returns>
	public bool Value {
		get { return _value; }
		set {
			if (value != _value) {
				_value = value;
				if (ValueChanged != null) {
					ValueChanged(this, EventArgs.Empty);
				}
				this.Invalidate();
			}
		}
	}

	protected override void OnSizeChanged(System.EventArgs e)
	{
		base.OnSizeChanged(e);
		Rectangle.Size = new Size(this.Width * 0.5f, this.Height - 1);
		UpdateVelocity();
		this.Invalidate();
	}

	protected override void OnMouseEnter(System.EventArgs e)
	{
		base.OnMouseEnter(e);
		_hover = true;
		this.Invalidate();
	}

	protected override void OnMouseLeave(System.EventArgs e)
	{
		base.OnMouseLeave(e);
		_hover = false;
		this.Invalidate();
	}

	protected override void OnClick(System.EventArgs e)
	{
		if (state == 0) {
			Value = !Value;
			_timer.Start();
		}
		base.OnClick(e);
	}

	protected void Timer_Tick(Timer sender, EventArgs e)
	{
		//Start: Rectangle.Location = New Point(0, 0)
		//End  : Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, 0)
		state = 1;
		switch (this.BooleanControlStyle) {
			case BooleanControlStyles.Rounded:
				switch (_value) {
					case true:
						//from false to true
						//in "midair"
						if (Rectangle.Location.X < this.Width - Rectangle.Width - 1) {
							Rectangle.Location = new Point(Rectangle.X + velocity, Rectangle.Y);
							this.Invalidate(new Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2f, Rectangle.Height));
						//it arrived
						} else {
							Rectangle.Location = new Point(this.Width - Rectangle.Width - 1, Rectangle.Y);
							state = 0;
							sender.Stop();
							this.Invalidate(new Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2f, Rectangle.Height));
						}
						break;
					case false:
						//from true to false
						if (Rectangle.Location.X > 0) {
							Rectangle.Location = new Point(Rectangle.X - velocity, Rectangle.Y);
							this.Invalidate(new Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3f, Rectangle.Height));
						} else {
							Rectangle.Location = new Point(0, Rectangle.Y);
							state = 0;
							sender.Stop();
							this.Invalidate(new Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3f, Rectangle.Height));
						}
						break;
				}
				break;
			case BooleanControlStyles.SquareText:
				state = 0;
				sender.Stop();
				break;
		}
	}

	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
	{
		base.OnPaint(e);
		float multEllipse = 0.4f;
		float diffHeight = 0.05f * this.Height;
		//25%
		float diffWidth = 0.1f * this.Width;

		using (Graphics g = e.Graphics) {
			g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias;


			switch (BooleanControlStyle) {
				case BooleanControlStyles.Rounded:
					Pen border = new Pen(Brushes.Silver);
					SolidBrush _backColor = new SolidBrush(Color.Aquamarine);
					switch (_value) {
						case true:
							//paint green
							_backColor.Color = Color.LimeGreen;
							border = new Pen(Brushes.LimeGreen);
							break;
						case false:
							//paint gray
							_backColor.Color = Color.WhiteSmoke;
							//gainsboro , lightgray
							border = new Pen(Brushes.Silver);
							break;
					}
					//draw background
					g.FillEllipse(_backColor, new Rectangle(new Point(diffWidth / 2f, diffHeight / 2f), new Size(this.Width * multEllipse, this.Height - diffHeight)));
					g.FillEllipse(_backColor, new Rectangle(new Point(this.Width - this.Width * multEllipse - diffWidth / 2f, diffHeight / 2f), new Size(this.Width * multEllipse, this.Height - diffHeight)));
					g.FillRectangle(_backColor, new Rectangle(new Point(this.Width * (multEllipse / 2f) + diffWidth / 2f, diffHeight / 2f), new Size(this.Width * (1f - multEllipse) - diffWidth, this.Height - diffHeight)));
					//draw border
					Rectangle rectEllipse = new Rectangle(new Point(diffWidth / 2f, diffHeight / 2f), new Size(this.Width * multEllipse, this.Height - diffHeight));
					g.DrawArc(border, rectEllipse, 180, 90);
					//upper left
					g.DrawArc(border, rectEllipse, 90, 90);
					//bottom left

					rectEllipse.Location = new Point(this.Width * (1f - multEllipse) - diffWidth / 2f, diffHeight / 2f);

					g.DrawArc(border, rectEllipse, 270, 90);
					//upper right
					g.DrawArc(border, rectEllipse, 0, 90);
					//bottom right

					g.DrawLine(border, new Point(diffWidth / 2f + this.Width * multEllipse * 0.5f, diffHeight / 2f), new Point(this.Width - diffWidth / 2f - this.Width * multEllipse * 0.5f, diffHeight / 2f));
					//línea superior
					g.DrawLine(border, new Point(diffWidth / 2f + this.Width * multEllipse * 0.5f, this.Height - diffHeight / 2f + 0), new Point(this.Width - diffWidth / 2f - this.Width * multEllipse * 0.5f, this.Height - diffHeight / 2f + 0));
					//línea inferior


					border.Dispose();
					_backColor.Dispose();

					Pen border1 = null;
					switch (Value) {
						case false:
							border1 = new Pen(Brushes.Silver);
							break;
						case true:
							border1 = new Pen(Brushes.LimeGreen);
							break;
					}

					g.FillEllipse(new SolidBrush(Color.WhiteSmoke), Rectangle);
					g.DrawEllipse(border1, Rectangle);
					border.Dispose();
					break;
				case BooleanControlStyles.SquareText:

					SolidBrush backBase = new SolidBrush(Color.DarkGray);
					//With {.Color = Color.FromArgb(128, Color.Silver)}
					SolidBrush backSmallBase = new SolidBrush(Color.FromArgb(backBase.Color.R * 0.9f, backBase.Color.G * 0.9f, backBase.Color.B * 0.9f));
					SolidBrush backTextBox = new SolidBrush(Color.DarkGray);

					float widthBox = 0.6f;
					Rectangle RectBox = new Rectangle(Point.Empty, new Size(this.Width * widthBox, this.Height - 2));
					Rectangle RectSmallBox = new Rectangle(new Point((this.Width - this.Width * 0.7f) / 2f, RectBox.Height * 0.25f + 1), new Size(this.Width * 0.7f, RectBox.Height / 2f));

					SolidBrush bruSuperiorLine = null;
					SolidBrush bruInferiorLine = null;

					switch (Value) {
						case false:
							backBase.Color = Color.FromArgb(255, Color.DarkGray);
							backTextBox.Color = Color.FromArgb(255, Color.Firebrick);
							RectBox.Location = new Point(0, 1);
							bruSuperiorLine = new SolidBrush(Color.FromArgb(255, Color.IndianRed));
							bruInferiorLine = new SolidBrush(Color.FromArgb(255, Color.DarkRed));
							break;
						case true:
							backBase.Color = Color.FromArgb(255, Color.DarkGray);
							//forestgreen
							backTextBox.Color = Color.FromArgb(255, Color.ForestGreen);
							RectBox.Location = new Point(this.Width * (1f - widthBox), 1);
							bruSuperiorLine = new SolidBrush(Color.FromArgb(255, Color.LimeGreen));
							bruInferiorLine = new SolidBrush(Color.FromArgb(255, Color.DarkGreen));
							break;
					}

					//draw gray background
					int BorderSpacing = this.Height * 0.1f;
					Drawing2D.GraphicsPath pathBack = new Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate);
					pathBack.AddLines({
						new Point(0 + BorderSpacing, 0),
						new Point(this.Width - 1 - BorderSpacing, 0),
						new Point(this.Width - 1, BorderSpacing),
						new Point(this.Width - 1, this.Height - 1 - BorderSpacing),
						new Point(this.Width - 1 - BorderSpacing, this.Height - 1),
						new Point(0 + BorderSpacing, this.Height - 1),
						new Point(0, this.Height - BorderSpacing),
						new Point(0, BorderSpacing),
						new Point(0 + BorderSpacing, 0)
					});

					float BorderSpacing2 = BorderSpacing / 2f;
					Drawing2D.GraphicsPath pathSmall = new Drawing2D.GraphicsPath();
					pathSmall.AddLines({
						new Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y),
						new Point(RectSmallBox.Location.X + RectSmallBox.Width - BorderSpacing2, RectSmallBox.Location.Y),
						new Point(RectSmallBox.Location.X + RectSmallBox.Width, RectSmallBox.Y + BorderSpacing2),
						new Point(RectSmallBox.Location.X + RectSmallBox.Width, RectSmallBox.Location.Y + RectSmallBox.Height - BorderSpacing2),
						new Point(RectSmallBox.Location.X + RectSmallBox.Width - BorderSpacing2, RectSmallBox.Location.Y + RectSmallBox.Height),
						new Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y + RectSmallBox.Height),
						new Point(RectSmallBox.Location.X, RectSmallBox.Location.Y + RectSmallBox.Height - BorderSpacing2),
						new Point(RectSmallBox.Location.X, RectSmallBox.Location.Y + BorderSpacing2),
						new Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y)
					});

					g.FillPath(backBase, pathBack);

					g.FillPath(backSmallBase, pathSmall);
					//end draw gray background

					//draw box that contains the text
					Drawing2D.GraphicsPath path = new Drawing2D.GraphicsPath();
					path.AddLines({
						new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y),
						new Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y),
						new Point(RectBox.Location.X + RectBox.Width, RectBox.Y + BorderSpacing),
						new Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing),
						new Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y + RectBox.Height),
						new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y + RectBox.Height),
						new Point(RectBox.Location.X, RectBox.Location.Y + RectBox.Height - BorderSpacing),
						new Point(RectBox.Location.X, RectBox.Location.Y + BorderSpacing),
						new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y)
					});

					g.FillPath(backTextBox, path);
					//end draw box

					//draw effect of highlight if mouse is hover
					if (_hover) {
						Drawing2D.GraphicsPath pathSup = new Drawing2D.GraphicsPath();
						Drawing2D.GraphicsPath pathInf = new Drawing2D.GraphicsPath();

						pathSup.AddLines({
							new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y),
							new Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y),
							new Point(RectBox.Location.X + RectBox.Width, RectBox.Y + BorderSpacing),
							new Point(RectBox.Location.X, RectBox.Location.Y + BorderSpacing),
							new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y)
						});
						pathInf.AddLines({
							new Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing),
							new Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y + RectBox.Height),
							new Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y + RectBox.Height),
							new Point(RectBox.Location.X, RectBox.Location.Y + RectBox.Height - BorderSpacing),
							new Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing)
						});
						g.FillPath(bruSuperiorLine, pathSup);
						g.FillPath(bruInferiorLine, pathInf);

						pathSup.Dispose();
						pathInf.Dispose();
					}
					//end highlight effect

					//draw text
					Font fontPalabra = Core.FuncionesGenerales.CreateFont("Arial", this.Height * 0.5, FontStyle.Bold, GraphicsUnit.Pixel);
					Size medicion = TextRenderer.MeasureText((Value ? TextTrue : TextFalse), fontPalabra);
					g.DrawString((Value ? TextTrue : TextFalse), fontPalabra, Brushes.White, new Point(RectBox.Location.X + (RectBox.Width - medicion.Width) / 2, (RectBox.Height - medicion.Height) / 2 + RectBox.Height * 0.05f));
					//end draw text

					//free resources
					path.Dispose();
					pathBack.Dispose();
					bruInferiorLine.Dispose();
					bruSuperiorLine.Dispose();
					backBase.Dispose();
					backTextBox.Dispose();
					break;
			}
		}
	}
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
