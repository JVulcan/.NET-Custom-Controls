Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel
Imports System.Runtime.InteropServices

Public Class FormV2
    Inherits System.Windows.Forms.Form

    '***************** WIN32 REFERENCES *************************
    Private Declare Function SendMessage Lib "User32" _
                         Alias "SendMessageA" (ByVal hWnd As IntPtr,
                                               ByVal wMsg As Integer,
                                               ByVal wParam As IntPtr,
                                               ByVal lParam As IntPtr) As Long

    Private Declare Sub ReleaseCapture Lib "User32" ()
    Const WM_NCLBUTTONDOWN = &HA1
    Const HTCAPTION = 2
    Const HTBORDER As Integer = 18
    Const HTBOTTOM As Integer = 15
    Const HTBOTTOMLEFT As Integer = 16
    Const HTBOTTOMRIGHT As Integer = 17
    Const HTLEFT As Integer = 10
    Const HTRIGHT As Integer = 11
    Const HTTOP As Integer = 12
    Const HTTOPLEFT As Integer = 13
    Const HTTOPRIGHT As Integer = 14
    '***********************************************************
    'Timer for the fade-in and fade-out effect
    Dim WithEvents TimerShowHide As New Timer With {.Interval = 15}
    Dim TimerMode As ShowHideMode

    Dim isDialogMode As Boolean = False

    Dim _borderWidth As Integer
    Dim _TitleHeight As Integer
    Dim _borderColor As Color
    Dim _edgeColor As Color
    Dim _TitleForeColor As Color
    Dim _titleFont As Font

    'MinMax states for Minimize and Maximize buttons
    Dim _MinMaxIconColorIdle As Color
    Dim _MinMaxIconColorOver As Color
    Dim _MinMaxIconBackColorOver As Color

    'bto salir
    Dim _ExitIconColorIdle As Color
    Dim _ExitIconBackColorIdle As Color

    Private Enum Overbutton
        None = -1
        Exit_ = 0
        Maximize = 1
        Minimize = 2
    End Enum

    Private Enum IconButton
        Maximize = 0
        Minimize = 1
        Restore = 2
    End Enum

    Public Enum TitleFormAlign
        Left = 0
        Center = 1
        Right = 2
    End Enum

    Private Enum ShowHideMode
        Show = 0
        Hide = 1
    End Enum
    ' WIN32
    Private Enum ResizeMode
        Left = HTLEFT
        Right = HTRIGHT
        Bottom = HTBOTTOM
        BottomLeft = HTBOTTOMLEFT
        BottomRight = HTBOTTOMRIGHT
        Top = HTTOP
        TopLeft = HTTOPLEFT
        TopRight = HTTOPRIGHT
    End Enum

    Private Structure ResizeData
        Public Activated As Boolean
        Public Mode As ResizeMode
    End Structure

    Dim _TitleAlign As TitleFormAlign
    Dim OverState As Overbutton = Overbutton.None

    Private Const MinTitleBarHeight As Integer = 22
    Dim rectExit As New Rectangle(0, 0, 45, MinTitleBarHeight)
    Dim rectMinimize As New Rectangle(0, 0, 30, MinTitleBarHeight)
    Dim rectMaximize As New Rectangle(0, 0, 30, MinTitleBarHeight)

    ' double click event over title bar was bugged, it never worked so
    ' I re-made this behaviour in order to be able to Maximize the Form is a Double Click is performed over the Title
    Dim WithEvents timerDblClk As New Timer With {.Interval = SystemInformation.DoubleClickTime}

    Dim _CanResize As Boolean

    Dim resizeInfo As New ResizeData

    'resize rectangle area bars
    Dim rectResizeRight As New Rectangle(0, 0, 3, 0)
    Dim rectResizeBottom As New Rectangle(0, 0, 3, 0)
    Dim rectResizeLeft As New Rectangle(0, 0, 3, 0)
    Dim rectResizeBottomRight As New Rectangle(0, 0, 8, 8)
    Dim rectResizeBottomLeft As New Rectangle(0, 0, 8, 8)
    Dim rectResizeTop As New Rectangle(0, 0, 10, 8)
    Dim rectResizeTopLeft As New Rectangle(0, 0, 8, 8)
    Dim rectResizeTopRight As New Rectangle(0, 0, 8, 8)

    Dim _useShowEffect As Boolean = True
    Dim _useCloseEffect As Boolean = True

    ' Event triggered once the fade-in effect finished
    Public Event OnLoadEffectFinished(ByVal sender As Object, ByVal e As EventArgs)

    Sub New()
        MyBase.New()
        BorderWidth = 3
        TitleHeight = 29
        TitleFont = Me.Font
        _TitleForeColor = Color.Black
        _borderColor = Color.Silver
        _edgeColor = Color.Silver
        _ExitIconColorIdle = Color.Black
        _ExitIconBackColorIdle = Color.LightSkyBlue
        _MinMaxIconColorOver = Color.White
        _MinMaxIconColorIdle = Color.Black
        _MinMaxIconBackColorOver = Color.SteelBlue
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        _TitleAlign = TitleFormAlign.Center
        _CanResize = True
        rectResizeTop.X = _borderWidth
        UpdateRectPosition()
    End Sub

#Region "Properties"
    ''' <summary>
    ''' Use Fade-in effect
    ''' </summary>
    ''' <returns></returns>
    Public Property UseShowEffect As Boolean
        Get
            Return _useShowEffect
        End Get
        Set(ByVal value As Boolean)
            If value <> _useShowEffect Then
                _useShowEffect = value
            End If
        End Set
    End Property
    ''' <summary>
    ''' Use Fade-out effect
    ''' </summary>
    ''' <returns></returns>
    Public Property UseCloseEffect As Boolean
        Get
            Return _useCloseEffect
        End Get
        Set(ByVal value As Boolean)
            If value <> _useCloseEffect Then
                _useCloseEffect = value
            End If
        End Set
    End Property
    ''' <summary>
    ''' If this is a resizable Form
    ''' </summary>
    ''' <returns></returns>
    Public Property CanResize As Boolean
        Get
            Return _CanResize
        End Get
        Set(ByVal value As Boolean)
            If value <> _CanResize Then
                _CanResize = value
                Me.Invalidate()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Position of the Title text
    ''' </summary>
    ''' <returns></returns>
    Public Property TitleAlign As TitleFormAlign
        Get
            Return _TitleAlign
        End Get
        Set(ByVal value As TitleFormAlign)
            If value <> _TitleAlign Then
                _TitleAlign = value
                InvalidateHeader()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Background Color of the Minimize and Maximize buttons when the mouse is over them
    ''' </summary>
    ''' <returns></returns>
    Public Property MinMaxIconBackColorOver As Color
        Get
            Return _MinMaxIconBackColorOver
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconBackColorOver Then
                _MinMaxIconBackColorOver = value
                Me.Invalidate(rectMaximize)
                Me.Invalidate(rectMinimize)
            End If
        End Set
    End Property
    ''' <summary>
    ''' Background Color of the Minimize and Maximize buttons when the mouse is not over them
    ''' </summary>
    ''' <returns></returns>
    Public Property MinMaxIconColorIdle As Color
        Get
            Return _MinMaxIconColorIdle
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconColorIdle Then
                _MinMaxIconColorIdle = value
                Me.Invalidate(rectMaximize)
                Me.Invalidate(rectMinimize)
            End If
        End Set
    End Property
    ''' <summary>
    ''' Icon Color of the Minimize and Maximize buttons when the mouse is over them
    ''' </summary>
    ''' <returns></returns>
    Public Property MinMaxIconColorOver As Color
        Get
            Return _MinMaxIconColorOver
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconColorOver Then
                _MinMaxIconColorOver = value
                Me.Invalidate(rectMaximize)
                Me.Invalidate(rectMinimize)
            End If
        End Set
    End Property
    ''' <summary>
    ''' Title Text
    ''' </summary>
    ''' <returns></returns>
    Public Shadows Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            If value <> MyBase.Text Then
                MyBase.Text = value
                InvalidateHeader()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Color of the × when the mouse is not over it
    ''' </summary>
    ''' <returns></returns>
    Public Property ExitIconColorIdle As Color
        Get
            Return _ExitIconColorIdle
        End Get
        Set(ByVal value As Color)
            If value <> _ExitIconColorIdle Then
                _ExitIconColorIdle = value
                Me.Invalidate()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Background Color of the × icon when the mouse is not over
    ''' </summary>
    ''' <returns></returns>
    Public Property ExitIconBackColorIdle As Color
        Get
            Return _ExitIconBackColorIdle
        End Get
        Set(ByVal value As Color)
            If value <> _ExitIconBackColorIdle Then
                _ExitIconBackColorIdle = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property TitleFont As Font
        Get
            Return _titleFont
        End Get
        Set(ByVal value As Font)
            _titleFont = value
            Me.Invalidate()
        End Set
    End Property

    Public Property TitleForeColor As Color
        Get
            Return _TitleForeColor
        End Get
        Set(ByVal value As Color)
            If value <> _TitleForeColor Then
                _TitleForeColor = value
                Me.Invalidate()
            End If
        End Set
    End Property
    ''' <summary>
    ''' Height of the Title Bar
    ''' </summary>
    ''' <returns></returns>
    Public Property TitleHeight As Integer
        Get
            Return _TitleHeight
        End Get
        Set(ByVal value As Integer)
            If value < 1 Then value = MinTitleBarHeight
            If value <> _TitleHeight Then
                _TitleHeight = value
                If _TitleHeight < MinTitleBarHeight Then
                    rectExit.Height = _TitleHeight
                    rectMinimize.Height = _TitleHeight
                    rectMaximize.Height = _TitleHeight
                Else
                    rectExit.Height = MinTitleBarHeight
                    rectMinimize.Height = MinTitleBarHeight
                    rectMaximize.Height = MinTitleBarHeight
                End If
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(ByVal value As Color)
            If _borderColor <> value Then
                _borderColor = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property EdgeColor As Color
        Get
            Return _edgeColor
        End Get
        Set(ByVal value As Color)
            If _edgeColor <> value Then
                _edgeColor = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(ByVal value As Integer)
            If value < 1 Then value = 3
            If value <> _borderWidth Then
                _borderWidth = value
                rectResizeBottomLeft.Size = New Size(Math.Max(8, value), Math.Max(8, value))
                rectResizeBottomRight.Size = rectResizeBottomLeft.Size
                rectResizeTop.X = value
                rectResizeTopLeft.Width = value
                rectResizeTopRight.Width = value
                UpdateRectPosition()
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Shadows Property ControlBox As Boolean
        Get
            Return MyBase.ControlBox
        End Get
        Set(ByVal value As Boolean)
            If value <> MyBase.ControlBox Then
                MyBase.ControlBox = value
                Me.Invalidate()
            End If
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property FormBorderStyle As FormBorderStyle
        Get
            Return MyBase.FormBorderStyle
        End Get
        Set(ByVal value As FormBorderStyle)
            MyBase.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        End Set
    End Property

    Public Shadows Property MaximizeBox As Boolean
        Get
            Return MyBase.MaximizeBox
        End Get
        Set(ByVal value As Boolean)
            If value <> MyBase.MaximizeBox Then
                MyBase.MaximizeBox = value
                If value = False AndAlso Not MinimizeBox Then rectMaximize.Height = 0 Else rectMaximize.Height = IIf(_TitleHeight < MinTitleBarHeight, _TitleHeight, MinTitleBarHeight)
            End If
        End Set
    End Property

    Public Shadows Property MinimizeBox As Boolean
        Get
            Return MyBase.MinimizeBox
        End Get
        Set(ByVal value As Boolean)
            If value <> MyBase.MinimizeBox Then
                MyBase.MinimizeBox = value
                If value = False Then rectMinimize.Height = 0 Else rectMinimize.Height = IIf(_TitleHeight < MinTitleBarHeight, _TitleHeight, MinTitleBarHeight)
            End If
        End Set
    End Property

    Public Shadows Function ShowDialog()
        isDialogMode = True
        Return MyBase.ShowDialog
    End Function
#End Region


    ''' <summary>
    ''' Updates lthe position of the Exit button
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateRectPosition()
        rectExit.Location = New Point(Me.Width - rectExit.Width - BorderWidth - 1, 0)
        rectMaximize.Location = New Point(rectExit.X - 1 - rectMaximize.Width, rectExit.Y)
        rectMinimize.Location = New Point(rectMaximize.X - 1 - rectMinimize.Width, rectExit.Y)
        'resize bars
        rectResizeRight.Location = New Point(Me.Width - _borderWidth, 0)
        rectResizeBottom.Location = New Point(0, Me.Height - _borderWidth)
        rectResizeBottomLeft.Location = New Point(0, Height - rectResizeBottomLeft.Height)
        rectResizeBottomRight.Location = New Point(Me.Width - rectResizeBottomRight.Width, Me.Height - rectResizeBottomRight.Height)
        rectResizeTopRight.Location = New Point(Me.Width - rectResizeTopRight.Width, 0)
    End Sub

#Region "Paint"
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)

        Using g As Graphics = e.Graphics
            Dim penEdge As New Pen(Me._edgeColor)
            Dim brush As New SolidBrush(Me.BorderColor)
            g.FillRectangle(brush, New Rectangle(0, 0, Me.Width, Me.TitleHeight)) 'barra título
            If _CanResize AndAlso WindowState = FormWindowState.Normal Then
                g.FillRectangle(brush, New Rectangle(0, Me.TitleHeight, BorderWidth, Me.Height - Me.TitleHeight)) 'izquierda
                g.FillRectangle(brush, New Rectangle(BorderWidth, Height - BorderWidth, Width - BorderWidth, BorderWidth)) 'abajo
                g.FillRectangle(brush, New Rectangle(Width - BorderWidth, TitleHeight, BorderWidth, Height - TitleHeight - BorderWidth)) 'derecha
            End If
            'edge
            g.DrawRectangle(penEdge, New Rectangle(0, 0, Me.Width - 1, Me.Height - 1))

            Dim titleMeasure As Size = TextRenderer.MeasureText(Me.Text, Me.TitleFont)
            Dim pointX As Integer
            Select Case Me.TitleAlign
                Case TitleFormAlign.Center
                    pointX = (Me.Width - titleMeasure.Width) / 2
                Case TitleFormAlign.Left
                    If Me.ShowIcon AndAlso Me.Icon IsNot Nothing Then
                        pointX = _borderWidth + 1 + 16 + 1
                    Else
                        pointX = _borderWidth + 1
                    End If
                Case TitleFormAlign.Right
                    pointX = rectMinimize.X - titleMeasure.Width - 1
            End Select
            g.DrawString(Me.Text, Me.TitleFont, New SolidBrush(Me.TitleForeColor), pointX, (Me.TitleHeight - titleMeasure.Height) / 2)

            'draw exit button
            If Me.ControlBox Then
                Dim _backColor As New SolidBrush(Me._ExitIconBackColorIdle)
                Dim colorIcon As New SolidBrush(Me._ExitIconColorIdle)
                UpdateRectPosition()
                If OverState = Overbutton.Exit_ Then
                    _backColor.Color = Color.Firebrick
                    colorIcon.Color = Color.White
                End If
                g.FillRectangle(_backColor, rectExit)
                Dim IconMeasure As Size = TextRenderer.MeasureText("×", New Font("Microsoft Sans Serif", 14.0!))
                g.DrawString("×", New Font("Microsoft Sans Serif", 14.0!), colorIcon,
                             New Point((rectExit.Width - IconMeasure.Width) / 2 + rectExit.X + 2,
                                       (rectExit.Height - IconMeasure.Height) / 2 + rectExit.Y))

                Dim rectIcon As New Rectangle(0, 0, 10, 10)
                If Me.MaximizeBox Then
                    _backColor = New SolidBrush(Me.BackColor)
                    colorIcon = New SolidBrush(Me._MinMaxIconColorIdle)
                    If OverState = Overbutton.Maximize Then
                        _backColor.Color = _MinMaxIconBackColorOver
                        colorIcon.Color = Me._MinMaxIconColorOver
                    End If
                    If _backColor.Color <> Me.BackColor Then g.FillRectangle(_backColor, rectMaximize)
                    rectIcon.X = rectMaximize.X + Math.Floor((rectMaximize.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMaximize.Y + Math.Floor((rectMaximize.Height - rectIcon.Height) / 2.0F)
                    DrawIcon(IIf(WindowState = FormWindowState.Maximized, IconButton.Restore, IconButton.Maximize), colorIcon.Color, rectIcon, g)
                ElseIf Me.MinimizeBox Then 'hay que dibujarlo deshabilitado
                    colorIcon.Color = Color.Gray
                    rectIcon.X = rectMaximize.X + Math.Floor((rectMaximize.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMaximize.Y + Math.Floor((rectMaximize.Height - rectIcon.Height) / 2.0F)
                    DrawIcon(IconButton.Maximize, colorIcon.Color, rectIcon, g)
                End If

                If Me.MinimizeBox Then
                    _backColor = New SolidBrush(Me.BackColor)
                    colorIcon = New SolidBrush(Me._MinMaxIconColorIdle)
                    If OverState = Overbutton.Minimize Then
                        _backColor.Color = _MinMaxIconBackColorOver
                        colorIcon.Color = Me._MinMaxIconColorOver
                    End If
                    If _backColor.Color <> Me.BackColor Then g.FillRectangle(_backColor, rectMinimize)
                    rectIcon.X = rectMinimize.X + Math.Floor((rectMinimize.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMinimize.Y + Math.Floor((rectMinimize.Height - rectIcon.Height) / 2.0F)
                    DrawIcon(IconButton.Minimize, colorIcon.Color, rectIcon, g)
                End If
                _backColor.Dispose()
                colorIcon.Dispose()
            End If

            'draw icon
            If Me.ShowIcon AndAlso Me.Icon IsNot Nothing Then
                g.DrawIcon(Me.Icon, New Rectangle(_borderWidth + 1, (_TitleHeight - 16) / 2, 16, 16))
            End If

            brush.Dispose()
            penEdge.Dispose()

        End Using
    End Sub
#End Region
    ' There was a bug when at maximize the Form used all the screen (even over the taskbar XD)
    ' so this is the fix for that bug

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Dim SkipMessage As Boolean = False
        Select Case m.Msg
            Case &H24
                WmGetMinMaxInfo(m.HWnd, m.LParam)
                SkipMessage = True
        End Select
        If Not SkipMessage Then
            MyBase.WndProc(m)
        End If
    End Sub

    Private Structure MINMAXINFO
        Public ptReserved As Point
        Public ptMaxSize As Point
        Public ptMaxPosition As Point
        Public ptMinTrackSize As Point
        Public ptMaxTrackSize As Point
    End Structure

    Private Sub WmGetMinMaxInfo(ByVal hWnd As IntPtr, ByVal lParam As IntPtr)
        Dim mmi As MINMAXINFO
        mmi = Marshal.PtrToStructure(lParam, GetType(MINMAXINFO))
        Dim monitor As Screen = Screen.FromHandle(hWnd)

        Dim rectWorkArea As Rectangle = monitor.WorkingArea
        Dim rectMonitor As Rectangle = monitor.Bounds

        mmi.ptMaxPosition.X = Math.Abs(rectWorkArea.Left - rectMonitor.Left)
        mmi.ptMaxPosition.Y = Math.Abs(rectWorkArea.Top - rectMonitor.Top)
        mmi.ptMaxSize.X = Math.Abs(rectWorkArea.Right - rectMonitor.Left)
        mmi.ptMaxSize.Y = Math.Abs(rectWorkArea.Bottom - rectMonitor.Top)
        mmi.ptMinTrackSize.X = MinimumSize.Width
        mmi.ptMinTrackSize.Y = MinimumSize.Height
        mmi.ptMaxTrackSize.X = MaximumSize.Width
        mmi.ptMaxTrackSize.Y = MaximumSize.Height

        Marshal.StructureToPtr(mmi, lParam, True)
    End Sub

    ' Implementation of events

    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Me.ControlBox And Not resizeInfo.Activated Then
            'Exit button
            If IntersectsWithRect(e, rectExit) Then
                If OverState <> Overbutton.Exit_ Then
                    Dim previous As Overbutton = OverState
                    OverState = Overbutton.Exit_
                    InvalidateButton(previous)
                    Me.Invalidate(rectExit)
                End If
            ElseIf Me.MaximizeBox AndAlso IntersectsWithRect(e, rectMaximize) Then
                If OverState <> Overbutton.Maximize Then
                    Dim previous As Overbutton = OverState
                    OverState = Overbutton.Maximize
                    InvalidateButton(previous)
                    Me.Invalidate(rectMaximize)
                End If
            ElseIf Me.MinimizeBox AndAlso IntersectsWithRect(e, rectMinimize) Then
                If OverState <> Overbutton.Minimize Then
                    Dim previous As Overbutton = OverState
                    OverState = Overbutton.Minimize
                    InvalidateButton(previous)
                    Me.Invalidate(rectMinimize)
                End If
            Else
                If OverState <> Overbutton.None Then
                    UpdatePrevious()
                End If
            End If
        End If

        'change cursor icon and do resize
        If WindowState = FormWindowState.Normal AndAlso _CanResize Then
            resizeInfo.Activated = True
            If IntersectsWithRect(e, rectResizeBottomRight) Then
                Cursor = Cursors.SizeNWSE
                resizeInfo.Mode = ResizeMode.BottomRight
            ElseIf IntersectsWithRect(e, rectResizeBottomLeft) Then
                Cursor = Cursors.SizeNESW
                resizeInfo.Mode = ResizeMode.BottomLeft
            ElseIf IntersectsWithRect(e, rectResizeTopLeft) Then
                Cursor = Cursors.SizeNWSE
                resizeInfo.Mode = ResizeMode.TopLeft
            ElseIf IntersectsWithRect(e, rectResizeTopRight) Then
                Cursor = Cursors.SizeNESW
                resizeInfo.Mode = ResizeMode.TopRight
            ElseIf IntersectsWithRect(e, rectResizeRight) Then
                Cursor = Cursors.SizeWE
                resizeInfo.Mode = ResizeMode.Right
            ElseIf IntersectsWithRect(e, rectResizeLeft) Then
                Cursor = Cursors.SizeWE
                resizeInfo.Mode = ResizeMode.Left
            ElseIf IntersectsWithRect(e, rectResizeBottom) Then
                Cursor = Cursors.SizeNS
                resizeInfo.Mode = ResizeMode.Bottom
            ElseIf IntersectsWithRect(e, rectResizeTop) Then
                Cursor = Cursors.SizeNS
                resizeInfo.Mode = ResizeMode.Top
            Else
                Cursor = Cursors.Default
                resizeInfo.Activated = False
            End If
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
        Cursor = Cursors.Default
        MyBase.OnMouseLeave(e)
    End Sub
    ' Maximize at Double Click! (yeah! it never has worked using FormStyle->None)
    Protected Overrides Sub OnDoubleClick(ByVal e As System.EventArgs)
        MyBase.OnDoubleClick(e)
        Dim CurPos As New Point(Windows.Forms.Cursor.Position.X - Me.Left, Windows.Forms.Cursor.Position.Y - Me.Top)
        If (MaximizeBox) AndAlso
            ((WindowState = FormWindowState.Normal AndAlso IntersectsWithRect(CurPos, New Rectangle(_borderWidth, rectResizeTop.Height + 1, rectMinimize.X - _borderWidth, _TitleHeight - rectResizeTop.Height))) Or
            (WindowState = FormWindowState.Maximized AndAlso IntersectsWithRect(CurPos, New Rectangle(_borderWidth, 0, rectMinimize.X - _borderWidth, _TitleHeight)))) Then
            If WindowState = FormWindowState.Normal Then
                WindowState = FormWindowState.Maximized
            ElseIf WindowState = FormWindowState.Maximized Then
                WindowState = FormWindowState.Normal
            End If
            Dim f As New MouseEventArgs(Windows.Forms.MouseButtons.Left, 1, CurPos.X, CurPos.Y, 0)
            OnMouseMove(f) 'pass the event to the user's implementation
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseClick(e)
    End Sub

    Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
        'size of resize bars
        rectResizeRight.Size = New Size(BorderWidth, Height)
        rectResizeLeft.Size = New Size(BorderWidth, Height)
        rectResizeBottom.Size = New Size(Width, BorderWidth)
        UpdateRectPosition()
        rectResizeTop.Size = New Size(rectMinimize.X - _borderWidth, 8)
        MyBase.OnResize(e)
        Me.Invalidate()
    End Sub

    ' Here is the trick to generate a DoubleClick function from ZERO xD
    ' or just behave like a normal click
    Dim CurPos1, CurPos2 As Rectangle
    Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        If IntersectsWithRect(e, rectMinimize) Then CurPos1 = rectMinimize Else If IntersectsWithRect(e, rectMaximize) Then CurPos1 = rectMaximize Else If IntersectsWithRect(e, rectExit) Then CurPos1 = rectExit
        MyBase.OnMouseDown(e)
        If WindowState = FormWindowState.Normal Then
            If timerDBLCLK_running Then
                timerDBLCLK_running = False
                timerDblClk.Stop()
                OnDoubleClick(EventArgs.Empty)
            Else
                timerDBLCLK_running = True
                timerDblClk.Start()
            End If
        End If

        If e.X >= Me.BorderWidth AndAlso e.X <= Me.Width - Me.BorderWidth AndAlso e.Y >= rectResizeTop.Height AndAlso e.Y <= Me.TitleHeight Then
            If WindowState = FormWindowState.Normal AndAlso
                Not IntersectsWithRect(e, rectExit) AndAlso
                Not IntersectsWithRect(e, rectMaximize) AndAlso
                Not IntersectsWithRect(e, rectMinimize) Then
                'move window
                ReleaseCapture()
                SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0)
            End If
        ElseIf resizeInfo.Activated Then
            ReleaseCapture()
            SendMessage(Handle, WM_NCLBUTTONDOWN, resizeInfo.Mode, 0)
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        If IntersectsWithRect(e, rectMinimize) Then CurPos2 = rectMinimize Else If IntersectsWithRect(e, rectMaximize) Then CurPos2 = rectMaximize Else If IntersectsWithRect(e, rectExit) Then CurPos2 = rectExit
        If Not resizeInfo.Activated AndAlso CurPos1 = CurPos2 Then
            Select Case OverState
                Case Overbutton.Exit_
                    Me.Close()
                Case Overbutton.Maximize
                    If WindowState = FormWindowState.Maximized Then
                        WindowState = FormWindowState.Normal
                        Me.Invalidate()
                    Else
                        Dim rect As New Rectangle(0, 0, Me.Width, Me.Height)
                        WindowState = FormWindowState.Maximized
                        Me.Invalidate(rect)
                    End If
                Case Overbutton.Minimize
                    WindowState = FormWindowState.Minimized
            End Select
            OverState = Overbutton.None
            'OnMouseMove(e)
        End If
        MyBase.OnMouseUp(e)
        resizeInfo.Activated = False
        Cursor = Cursors.Default
        OnMouseMove(e)
    End Sub

    Protected Overrides Sub OnClosed(ByVal e As System.EventArgs)
        timerDblClk.Stop()
        TimerShowHide.Stop()
        If UseCloseEffect Then
            Do
                Me.Opacity -= 0.1F
                Threading.Thread.Sleep(20)
            Loop Until Me.Opacity = 0.0F
        End If
        MyBase.OnClosed(e)
    End Sub

    Protected Overrides Sub OnLoad(ByVal e As System.EventArgs)
        If UseShowEffect Then
            Me.Opacity = 0.1F
            TimerMode = ShowHideMode.Show
            TimerShowHide.Start()
        End If
        MyBase.OnLoad(e)
    End Sub

    Private Sub Timer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles TimerShowHide.Tick
        If TimerMode = ShowHideMode.Show AndAlso Me.Opacity = 1.0F Then
            TimerShowHide.Stop()
            RaiseEvent OnLoadEffectFinished(Me, EventArgs.Empty)
        ElseIf TimerMode = ShowHideMode.Hide AndAlso Me.Opacity = 0.1F Then
            TimerShowHide.Stop()
        End If
        Select Case TimerMode
            Case ShowHideMode.Hide
                Me.Opacity -= 0.1F
            Case ShowHideMode.Show
                Me.Opacity += 0.1F
        End Select
    End Sub

    ''' <summary>
    ''' Indicates if the MouseEventArgs position intersects with the  provided rectangle position
    ''' </summary>
    ''' <param name="e">Arguments of Mouse to analize</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function IntersectsWithRect(ByRef e As System.Windows.Forms.MouseEventArgs, ByRef rect As Rectangle)
        If e.X >= rect.X AndAlso e.X <= rect.X + rect.Width AndAlso e.Y >= rect.Y AndAlso e.Y <= rect.Y + rect.Height Then Return True
        Return False
    End Function

    Private Function IntersectsWithRect(ByRef CursorPosition As Point, ByRef rect As Rectangle)
        If CursorPosition.X >= rect.X AndAlso CursorPosition.X <= rect.X + rect.Width AndAlso CursorPosition.Y >= rect.Y AndAlso CursorPosition.Y <= rect.Y + rect.Height Then Return True
        Return False
    End Function

    ' Here the ICON DRAW is made, just using lines and rectangles :3
    Private Sub DrawIcon(ByVal icon As IconButton, ByVal color As Color, ByRef rect As Rectangle, ByRef g As Graphics)
        Dim pen As New Pen(color)
        Select Case icon
            Case IconButton.Restore
                'box
                g.DrawRectangle(pen, New Rectangle(rect.X, rect.Y + 2, 8, 8))
                g.DrawLine(pen, New Point(rect.X + 1, rect.Y + 3), New Point(rect.X + 9 - 2, rect.Y + 3))
                'outside
                g.DrawLine(pen, New Point(rect.X + 3, rect.Y + 0), New Point(rect.X + 10, rect.Y + 0))
                g.DrawLine(pen, New Point(rect.X + 10, rect.Y + 1), New Point(rect.X + 10, rect.Y + 7))
            Case IconButton.Minimize
                g.FillRectangle(pen.Brush, New Rectangle(rect.X + 1, rect.Y + 7, 8, 2))
            Case IconButton.Maximize
                g.DrawRectangle(pen, New Rectangle(rect.X + 0, rect.Y + 1, 10, 8))
                g.DrawLine(pen, New Point(rect.X + 1, rect.Y + 2), New Point(rect.X + 9, rect.Y + 2))
        End Select
    End Sub

    Private Sub UpdatePrevious()
        Dim previous As Overbutton = OverState
        OverState = Overbutton.None
        Select Case previous
            Case Overbutton.Maximize
                Me.Invalidate(rectMaximize)
            Case Overbutton.Minimize
                Me.Invalidate(rectMinimize)
            Case Overbutton.Exit_
                Me.Invalidate(rectExit)
        End Select
    End Sub

    Private Sub InvalidateButton(ByVal state As Overbutton)
        Select Case state
            Case Overbutton.Maximize
                Invalidate(rectMaximize)
            Case Overbutton.Minimize
                Invalidate(rectMinimize)
            Case Overbutton.Exit_
                Invalidate(rectExit)
        End Select
    End Sub

    Private Sub InvalidateHeader()
        Me.Invalidate(New Rectangle(0, 0, Me.Width, Me.TitleHeight))
    End Sub

    Dim timerDBLCLK_running As Boolean = False
    Private Sub TimerDBLCLK_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timerDblClk.Tick
        timerDBLCLK_running = False
        timerDblClk.Stop()
    End Sub

End Class
