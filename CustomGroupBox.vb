Imports System.Windows.Forms
Imports System.Drawing
''' <summary>
''' Custom GroupBox Control
''' </summary>
''' <remarks></remarks>
Public Class CustomGroupBox
    Inherits GroupBox

    Private _BorderColor As Color
    Private _BorderWidth As UShort
    Private _lblText As Label

    Sub New()
        MyBase.New()
        _BorderColor = Color.Black
        _BorderWidth = 3
        Me.ForeColor = Color.White
        _lblText = New Label With {.Location = New Point(3, 3), .AutoSize = True, .Font = Me.Font, .ForeColor = Me.ForeColor, .Text = Me.Text, .BackColor = Color.Transparent}
        Me.Controls.Add(_lblText)
    End Sub

    Public Property BorderColor As Color
        Get
            Return _BorderColor
        End Get
        Set(ByVal value As Color)
            _BorderColor = value
            Me.Invalidate()
        End Set
    End Property

    Public Property BorderWidth As UShort
        Get
            Return _BorderWidth
        End Get
        Set(ByVal value As UShort)
            _BorderWidth = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        _lblText.Text = Me.Text
        _lblText.Font = Me.Font
        _lblText.ForeColor = Me.ForeColor
        Dim tSize As Size = TextRenderer.MeasureText(Me.Text, Me.Font)

        Dim bru As SolidBrush
        If Enabled Then
            bru = New SolidBrush(Me._BorderColor)
        Else
            bru = New SolidBrush(Color.FromArgb(190, _BorderColor.R, _BorderColor.G, _BorderColor.B))
        End If
        Dim back As New SolidBrush(BackColor)
        e.Graphics.FillRectangle(New SolidBrush(Color.Transparent), New Rectangle(0, 0, Width, Height))

        e.Graphics.FillRectangle(bru, New Rectangle(_BorderWidth, 0, Me.Width - _BorderWidth * 2, tSize.Height + 6))
        e.Graphics.FillRectangle(bru, New Rectangle(0, 0, Me._BorderWidth, Me.Height - _BorderWidth))
        e.Graphics.FillRectangle(bru, New Rectangle(0, Me.Height - Me._BorderWidth, Me.Width, Me._BorderWidth))
        e.Graphics.FillRectangle(bru, New Rectangle(Me.Width - Me._BorderWidth, 0, Me._BorderWidth, Me.Height - _BorderWidth))
        e.Graphics.FillRectangle(back, New Rectangle(_BorderWidth, tSize.Height + 6, Me.Width - _BorderWidth * 2, Me.Height - _BorderWidth - tSize.Height - 6))
        bru.Dispose()
        tSize = Nothing
    End Sub

End Class
