Imports System.Windows.Forms
Imports System.Drawing
''' <summary>
''' Special button that shows a description when the mouse is over it
''' </summary>
''' <remarks>the button should be large enough to fit the whole description</remarks>
Public Class DescButton
    Inherits Control 'NM = Dont show
    'shared
    Private Shared Count As UInteger = 0 'NM
    'back
    Private _BackFont As Font
    Private _BackText As String
    Private _BackColor As Color
    Private _BackForeColor As Color
    'front
    Private _FrontFont As Font
    Private _FrontText As String
    Private _FrontColor As Color
    Private _FrontForeColor As Color
    'misc
    Private _BorderWidth As Integer
    'objs:
    Private WithEvents _BackPanel As Panel 'NM
    Private WithEvents _FrontPanel As Panel 'NM
    Private WithEvents _BackLabel As Label 'NM
    Private WithEvents _FrontLabel As Label 'NM
    'general:
    Private WithEvents _timer As Timer 'NM
    Private _over As Boolean
    Private _velocity As Integer

    Sub New()
        MyBase.New()
        updateVelocity()
        Count += 1
        Me.Name = "DescButton" & Count
        _BackColor = Color.Black
        _BackText = "Description" & Count
        _BackFont = Me.Font
        _BackForeColor = Color.White
        _FrontText = "DescButton" & Count
        _FrontFont = Me.Font
        _FrontForeColor = Me.ForeColor
        _FrontColor = Me.BackColor
        _over = False
        _timer = New Timer With {.Interval = 1}
        _BorderWidth = 1
        Dim tSize As Size = TextRenderer.MeasureText(_FrontText, _FrontFont)
        _BackPanel = New Panel With {.Size = Me.Size, .BackColor = _BackColor, .Location = New Point(0, 0)}
        _BackLabel = New Label With {.Font = _BackFont, .AutoSize = False, .ForeColor = _BackForeColor, .TextAlign = ContentAlignment.MiddleCenter, .Size = New Size(_BackPanel.Width - _BorderWidth * 2, _BackPanel.Height - tSize.Height), .Location = New Point(_BorderWidth, tSize.Height + 1), .Text = _BackText}
        _BackPanel.Controls.Add(_BackLabel)
        _FrontPanel = New Panel With {.Size = New Size(Me.Size.Width - _BorderWidth * 2, Me.Size.Height - _BorderWidth * 2), .BackColor = _FrontColor, .Location = New Point(1, 1)}
        _FrontLabel = New Label With {.Font = _FrontFont, .AutoSize = True, .ForeColor = _FrontForeColor, .Location = New Point((_FrontPanel.Width - tSize.Width) / 2, (_FrontPanel.Height - tSize.Height) / 2)}
        _FrontPanel.Controls.Add(_FrontLabel)
        Me.Controls.AddRange({_FrontPanel, _BackPanel})
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)
        Dim tSize As Size = TextRenderer.MeasureText(_FrontText, _FrontFont)
        'BACK
        _BackPanel.Size = Me.Size
        _BackPanel.BackColor = _BackColor
        _BackLabel.Font = _BackFont
        _BackLabel.Text = _BackText
        _BackLabel.ForeColor = _BackForeColor
        'FRONT
        _FrontPanel.Size = New Size(_BackPanel.Size.Width - _BorderWidth * 2, _BackPanel.Size.Height - _BorderWidth * 2)
        _FrontPanel.Location = New Point(_BorderWidth, _BorderWidth)
        _FrontPanel.BackColor = Me.BackColor
        _FrontLabel.Font = _FrontFont
        _FrontLabel.Text = _FrontText
        _FrontLabel.ForeColor = Me.ForeColor
        _FrontLabel.Location = New Point((_FrontPanel.Width - tSize.Width) / 2, (_FrontPanel.Height - tSize.Height) / 2)

        _BackLabel.Location = New Point(_BorderWidth, tSize.Height + 1)
        _BackLabel.Size = New Size(_BackPanel.Width - _BorderWidth * 2, _BackPanel.Height - tSize.Height - _BorderWidth)
    End Sub

    Public Shadows Sub MouseDown(ByVal sender As Object, ByVal e As EventArgs) Handles _FrontLabel.MouseDown, _FrontPanel.MouseDown, _BackLabel.MouseDown, _BackPanel.MouseDown
        _BackPanel.BackColor = Color.FromArgb(255, CInt(_BackColor.R / 2), CInt(_BackColor.G / 2), CInt(_BackColor.B / 2))
        _FrontLabel.ForeColor = Color.FromArgb(255, CInt(_FrontForeColor.R / 2), CInt(_FrontForeColor.G / 2), CInt(_FrontForeColor.B / 2))
        MyBase.OnMouseDown(e)
    End Sub

    Public Shadows Sub MouseUp(ByVal sender As Object, ByVal e As EventArgs) Handles _FrontLabel.MouseUp, _FrontPanel.MouseUp, _BackLabel.MouseUp, _BackPanel.MouseUp
        _BackPanel.BackColor = _BackColor
        _FrontLabel.ForeColor = _FrontForeColor
        MyBase.OnMouseUp(e)
    End Sub

    Public Shadows Sub MouseEnter(ByVal sender As Object, ByVal e As EventArgs) Handles _FrontPanel.MouseEnter, _FrontLabel.MouseEnter
        _over = True
        _timer.Start()
    End Sub

    Public Shadows Sub MouseLeave(ByVal sender As Object, ByVal e As EventArgs) Handles _BackLabel.MouseLeave, _BackPanel.MouseLeave
        _over = False
        _timer.Start()
    End Sub

    Public Shadows Sub OnClick(ByVal sender As Object, ByVal e As EventArgs) Handles _FrontLabel.Click, _FrontPanel.Click, _BackLabel.Click, _BackPanel.Click
        MyBase.OnClick(e)
    End Sub

    Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
        updateVelocity()
        MyBase.OnResize(e)
    End Sub

    Private Sub TimerTick(ByVal sender As Timer, ByVal e As EventArgs) Handles _timer.Tick
        If _over Then
            If _FrontLabel.Top > 0 Then
                _FrontPanel.Height -= _velocity
                _FrontLabel.Top -= CInt(_velocity / 2)
            Else
                _FrontLabel.Top = 0
                _FrontPanel.Height = _FrontLabel.Height + 1
                sender.Stop()
            End If
        Else
            If _FrontPanel.Height < _BackPanel.Height Then
                _FrontPanel.Height += _velocity
                _FrontLabel.Top += CInt(_velocity / 2)
            Else
                _FrontPanel.Height = _BackPanel.Height - _BorderWidth * 2
                _FrontLabel.Top = (_FrontPanel.Height - _FrontLabel.Height) / 2
                sender.Stop()
            End If
        End If
    End Sub

    Private Sub updateVelocity()
        Me._velocity = CInt(Me.Height / 20)
    End Sub

    Public Property BorderWidth As Integer
        Get
            Return _BorderWidth
        End Get
        Set(ByVal value As Integer)
            If value > 0 Then
                If value <> _BorderWidth Then
                    _BorderWidth = value
                    Me.Invalidate()
                End If
            Else : _BorderWidth = 1
            End If
        End Set
    End Property
    'All properties that starts with:
    ' BACK = are properties that modify the back part of the control (the description text, color, forecolor)
    ' FRONT = are properties that modify the front part of the control (title text, color, forecolor)
    Public Property BackGColor As Color
        Get
            Return _BackColor
        End Get
        Set(ByVal value As Color)
            _BackColor = value
            _BackPanel.BackColor = _BackColor
        End Set
    End Property

    Public Property BackText As String
        Get
            Return _BackText
        End Get
        Set(ByVal value As String)
            _BackText = value
            _BackLabel.Text = _BackText
        End Set
    End Property

    Public Property BackFont As Font
        Get
            Return _BackFont
        End Get
        Set(ByVal value As Font)
            _BackFont = value
            _BackLabel.Font = value
        End Set
    End Property

    Public Property BackForeColor As Color
        Get
            Return _BackForeColor
        End Get
        Set(ByVal value As Color)
            _BackForeColor = value
            _BackLabel.ForeColor = value
        End Set
    End Property

    Public Property FrontColor As Color
        Get
            Return _FrontColor
        End Get
        Set(ByVal value As Color)
            _FrontColor = value
            Me.BackColor = value
            _FrontPanel.BackColor = value
        End Set
    End Property

    Public Property FrontText As String
        Get
            Return _FrontText
        End Get
        Set(ByVal value As String)
            _FrontText = value
            Me.Text = value
            _FrontLabel.Text = _FrontText
            CenterLabel(_FrontLabel)
        End Set
    End Property

    Public Property FrontFont As Font
        Get
            Return _FrontFont
        End Get
        Set(ByVal value As Font)
            _FrontFont = value
            _FrontLabel.Font = _FrontFont
            CenterLabel(_FrontLabel)
        End Set
    End Property

    Public Property FrontForeColor As Color
        Get
            Return _FrontForeColor
        End Get
        Set(ByVal value As Color)
            _FrontForeColor = value
            Me.ForeColor = value
            _FrontLabel.ForeColor = value
        End Set
    End Property

    Private Sub CenterLabel(ByVal lbl As Label)
        Dim tSize As Size = TextRenderer.MeasureText(lbl.Text, lbl.Font)
        lbl.Location = New Point((lbl.Parent.Width - tSize.Width) / 2, (lbl.Parent.Height - tSize.Height) / 2)
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyBase.Dispose(disposing)
    End Sub
End Class
