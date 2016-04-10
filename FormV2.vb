Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel
Imports System.Runtime.InteropServices

Public Class FormV2
    Inherits TrackerForm

    '***************** WIN32 REFERENCES *************************
    Private Declare Function SendMessage Lib "User32" _
                         Alias "SendMessageA" (ByVal hWnd As IntPtr, _
                                               ByVal wMsg As Integer, _
                                               ByVal wParam As IntPtr, _
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
    Dim WithEvents TimerShowHide As New Timer With {.Interval = 15}
    Dim TimerMode As ShowHideMode

    Dim isDialogMode As Boolean = False

    Dim _borderWidth As Integer
    Dim _TitleHeight As Integer
    Dim _borderColor As Color
    Dim _edgeColor As Color
    Dim _TitleForeColor As Color
    Dim _titleFont As Font

    Dim _MinMaxIconColorIdle As Color
    Dim _MinMaxIconColorOver As Color
    Dim _MinMaxIconBackColorOver As Color

    'bto salir
    Dim _ForeColorExitIdle As Color
    Dim _BackColorExitIdle As Color

    Private Enum Overbutton
        Ninguno = -1
        Salir = 0
        Maximizar = 1
        Minimizar = 2
    End Enum

    Private Enum IconButton
        Maximizar = 0
        Minimizar = 1
        Restaurar = 2
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
        Public Activado As Boolean
        Public Modo As ResizeMode
    End Structure

    Dim _TitleAlign As TitleFormAlign
    Dim OverState As Overbutton = Overbutton.Ninguno

    Dim rectSalir As New Rectangle(0, 0, 45, 22)
    Dim rectMinimizar As New Rectangle(0, 0, 30, 22)
    Dim rectMaximizar As New Rectangle(0, 0, 30, 22)

    Dim WithEvents timerDblClk As New Timer With {.Interval = SystemInformation.DoubleClickTime}
    Dim _UseResize As Boolean

    Dim resizeInfo As New ResizeData

    'barras de escalabilidad
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

    Public Event OnLoadEffectFinished(ByVal sender As Object, ByVal e As EventArgs)

    Sub New()
        MyBase.New()
        BorderWidth = 3
        TitleHeight = 29
        TitleFont = Me.Font
        _TitleForeColor = Color.Black
        _borderColor = Color.Silver
        _edgeColor = Color.Silver
        _ForeColorExitIdle = Color.Black
        _BackColorExitIdle = Color.LightSkyBlue
        _MinMaxIconColorOver = Color.White
        _MinMaxIconColorIdle = Color.Black
        _MinMaxIconBackColorOver = Color.SteelBlue
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        _TitleAlign = TitleFormAlign.Center
        _UseResize = True
        rectResizeTop.X = _borderWidth
        ActualizaRectPosition()
    End Sub

#Region "Propiedades"

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

    Public Property UseResize As Boolean
        Get
            Return _UseResize
        End Get
        Set(ByVal value As Boolean)
            If value <> _UseResize Then
                _UseResize = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property TitleAlign As TitleFormAlign
        Get
            Return _TitleAlign
        End Get
        Set(ByVal value As TitleFormAlign)
            If value <> _TitleAlign Then
                _TitleAlign = value
                InvalidarCabecera()
            End If
        End Set
    End Property

    Public Property MinMaxIconBackColorOver As Color
        Get
            Return _MinMaxIconBackColorOver
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconBackColorOver Then
                _MinMaxIconBackColorOver = value
                Me.Invalidate(rectMaximizar)
                Me.Invalidate(rectMinimizar)
            End If
        End Set
    End Property

    Public Property MinMaxIconColorIdle As Color
        Get
            Return _MinMaxIconColorIdle
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconColorIdle Then
                _MinMaxIconColorIdle = value
                Me.Invalidate(rectMaximizar)
                Me.Invalidate(rectMinimizar)
            End If
        End Set
    End Property

    Public Property MinMaxIconColorOver As Color
        Get
            Return _MinMaxIconColorOver
        End Get
        Set(ByVal value As Color)
            If value <> _MinMaxIconColorOver Then
                _MinMaxIconColorOver = value
                Me.Invalidate(rectMaximizar)
                Me.Invalidate(rectMinimizar)
            End If
        End Set
    End Property

    Public Shadows Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            If value <> MyBase.Text Then
                MyBase.Text = value
                InvalidarCabecera()
            End If
        End Set
    End Property

    Public Property ForeColorExitIdle As Color
        Get
            Return _ForeColorExitIdle
        End Get
        Set(ByVal value As Color)
            If value <> _ForeColorExitIdle Then
                _ForeColorExitIdle = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property BackColorExitIdle As Color
        Get
            Return _BackColorExitIdle
        End Get
        Set(ByVal value As Color)
            If value <> _BackColorExitIdle Then
                _BackColorExitIdle = value
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

    Public Property TitleHeight As Integer
        Get
            Return _TitleHeight
        End Get
        Set(ByVal value As Integer)
            If value < 1 Then value = 29
            If value <> _TitleHeight Then
                _TitleHeight = value
                If _TitleHeight < 22 Then
                    rectSalir.Height = _TitleHeight
                    rectMinimizar.Height = _TitleHeight
                    rectMaximizar.Height = _TitleHeight
                Else
                    rectSalir.Height = 22
                    rectMinimizar.Height = 22
                    rectMaximizar.Height = 22
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
                ActualizaRectPosition()
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
                If value = False AndAlso Not MinimizeBox Then rectMaximizar.Height = 0 Else rectMaximizar.Height = IIf(_TitleHeight < 22, _TitleHeight, 22)
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
                If value = False Then rectMinimizar.Height = 0 Else rectMinimizar.Height = IIf(_TitleHeight < 22, _TitleHeight, 22)
            End If
        End Set
    End Property

    Public Shadows Function ShowDialog()
        isDialogMode = True
        Return MyBase.ShowDialog
    End Function
#End Region


    ''' <summary>
    ''' Actualiza la posición del botón Salir
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ActualizaRectPosition()
        rectSalir.Location = New Point(Me.Width - rectSalir.Width - BorderWidth - 1, 0)
        rectMaximizar.Location = New Point(rectSalir.X - 1 - rectMaximizar.Width, rectSalir.Y)
        rectMinimizar.Location = New Point(rectMaximizar.X - 1 - rectMinimizar.Width, rectSalir.Y)
        'barras de resize
        rectResizeRight.Location = New Point(Me.Width - _borderWidth, 0)
        rectResizeBottom.Location = New Point(0, Me.Height - _borderWidth)
        rectResizeBottomLeft.Location = New Point(0, Height - rectResizeBottomLeft.Height)
        rectResizeBottomRight.Location = New Point(Me.Width - rectResizeBottomRight.Width, Me.Height - rectResizeBottomRight.Height)
        rectResizeTopRight.Location = New Point(Me.Width - rectResizeTopRight.Width, 0)
    End Sub

#Region "Pintar"
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)

        Using g As Graphics = e.Graphics
            Dim penEdge As New Pen(Me._edgeColor)
            Dim brush As New SolidBrush(Me.BorderColor)
            g.FillRectangle(brush, New Rectangle(0, 0, Me.Width, Me.TitleHeight)) 'barra título
            If _UseResize AndAlso WindowState = FormWindowState.Normal Then
                g.FillRectangle(brush, New Rectangle(0, Me.TitleHeight, BorderWidth, Me.Height - Me.TitleHeight)) 'izquierda
                g.FillRectangle(brush, New Rectangle(BorderWidth, Height - BorderWidth, Width - BorderWidth, BorderWidth)) 'abajo
                g.FillRectangle(brush, New Rectangle(Width - BorderWidth, TitleHeight, BorderWidth, Height - TitleHeight - BorderWidth)) 'derecha
            End If
            'edge
            g.DrawRectangle(penEdge, New Rectangle(0, 0, Me.Width - 1, Me.Height - 1))

            Dim medicionTitulo As Size = TextRenderer.MeasureText(Me.Text, Me.TitleFont)
            Dim puntoX As Integer
            Select Case Me.TitleAlign
                Case TitleFormAlign.Center
                    puntoX = (Me.Width - medicionTitulo.Width) / 2
                Case TitleFormAlign.Left
                    If Me.ShowIcon AndAlso Me.Icon IsNot Nothing Then
                        puntoX = _borderWidth + 1 + 16 + 1
                    Else
                        puntoX = _borderWidth + 1
                    End If
                Case TitleFormAlign.Right
                    puntoX = rectMinimizar.X - medicionTitulo.Width - 1
            End Select
            g.DrawString(Me.Text, Me.TitleFont, New SolidBrush(Me.TitleForeColor), puntoX, (Me.TitleHeight - medicionTitulo.Height) / 2)

            'dibujo del botón salir
            If Me.ControlBox Then
                Dim colorFondo As New SolidBrush(Me._BackColorExitIdle)
                Dim colorIcon As New SolidBrush(Me._ForeColorExitIdle)
                ActualizaRectPosition()
                If OverState = Overbutton.Salir Then
                    colorFondo.Color = Color.Firebrick
                    colorIcon.Color = Color.White
                End If
                g.FillRectangle(colorFondo, rectSalir)
                Dim medidaIcon As Size = TextRenderer.MeasureText("×", New Font("Microsoft Sans Serif", 14.0!))
                g.DrawString("×", New Font("Microsoft Sans Serif", 14.0!), colorIcon,
                             New Point((rectSalir.Width - medidaIcon.Width) / 2 + rectSalir.X + 2,
                                       (rectSalir.Height - medidaIcon.Height) / 2 + rectSalir.Y))

                Dim rectIcon As New Rectangle(0, 0, 10, 10)
                If Me.MaximizeBox Then
                    colorFondo = New SolidBrush(Me.BackColor)
                    colorIcon = New SolidBrush(Me._MinMaxIconColorIdle)
                    If OverState = Overbutton.Maximizar Then
                        colorFondo.Color = _MinMaxIconBackColorOver
                        colorIcon.Color = Me._MinMaxIconColorOver
                    End If
                    If colorFondo.Color <> Me.BackColor Then g.FillRectangle(colorFondo, rectMaximizar)
                    rectIcon.X = rectMaximizar.X + Math.Floor((rectMaximizar.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMaximizar.Y + Math.Floor((rectMaximizar.Height - rectIcon.Height) / 2.0F)
                    DibujarIcono(IIf(WindowState = FormWindowState.Maximized, IconButton.Restaurar, IconButton.Maximizar), colorIcon.Color, rectIcon, g)
                ElseIf Me.MinimizeBox Then 'hay que dibujarlo deshabilitado
                    colorIcon.Color = Color.Gray
                    rectIcon.X = rectMaximizar.X + Math.Floor((rectMaximizar.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMaximizar.Y + Math.Floor((rectMaximizar.Height - rectIcon.Height) / 2.0F)
                    DibujarIcono(IconButton.Maximizar, colorIcon.Color, rectIcon, g)
                End If

                If Me.MinimizeBox Then
                    colorFondo = New SolidBrush(Me.BackColor)
                    colorIcon = New SolidBrush(Me._MinMaxIconColorIdle)
                    If OverState = Overbutton.Minimizar Then
                        colorFondo.Color = _MinMaxIconBackColorOver
                        colorIcon.Color = Me._MinMaxIconColorOver
                    End If
                    If colorFondo.Color <> Me.BackColor Then g.FillRectangle(colorFondo, rectMinimizar)
                    rectIcon.X = rectMinimizar.X + Math.Floor((rectMinimizar.Width - rectIcon.Width) / 2.0F)
                    rectIcon.Y = rectMinimizar.Y + Math.Floor((rectMinimizar.Height - rectIcon.Height) / 2.0F)
                    DibujarIcono(IconButton.Minimizar, colorIcon.Color, rectIcon, g)
                End If
                colorFondo.Dispose()
                colorIcon.Dispose()
            End If

            'dibujar icono
            If Me.ShowIcon AndAlso Me.Icon IsNot Nothing Then
                g.DrawIcon(Me.Icon, New Rectangle(_borderWidth + 1, (_TitleHeight - 16) / 2, 16, 16))
            End If

            brush.Dispose()
            penEdge.Dispose()

        End Using
    End Sub
#End Region

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

    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Me.ControlBox And Not resizeInfo.Activado Then
            'botón Salir
            If IntersectaConRect(e, rectSalir) Then
                If OverState <> Overbutton.Salir Then
                    Dim anterior As Overbutton = OverState
                    OverState = Overbutton.Salir
                    InvalidarBoton(anterior)
                    Me.Invalidate(rectSalir)
                End If
            ElseIf Me.MaximizeBox AndAlso IntersectaConRect(e, rectMaximizar) Then
                If OverState <> Overbutton.Maximizar Then
                    Dim anterior As Overbutton = OverState
                    OverState = Overbutton.Maximizar
                    InvalidarBoton(anterior)
                    Me.Invalidate(rectMaximizar)
                End If
            ElseIf Me.MinimizeBox AndAlso IntersectaConRect(e, rectMinimizar) Then
                If OverState <> Overbutton.Minimizar Then
                    Dim anterior As Overbutton = OverState
                    OverState = Overbutton.Minimizar
                    InvalidarBoton(anterior)
                    Me.Invalidate(rectMinimizar)
                End If
            Else
                If OverState <> Overbutton.Ninguno Then
                    ActualizarAnterior()
                End If
            End If
        End If

        'cambiar cursor y hacer resize
        If WindowState = FormWindowState.Normal AndAlso _UseResize Then
            resizeInfo.Activado = True
            If IntersectaConRect(e, rectResizeBottomRight) Then
                Cursor = Cursors.SizeNWSE
                resizeInfo.Modo = ResizeMode.BottomRight
            ElseIf IntersectaConRect(e, rectResizeBottomLeft) Then
                Cursor = Cursors.SizeNESW
                resizeInfo.Modo = ResizeMode.BottomLeft
            ElseIf IntersectaConRect(e, rectResizeTopLeft) Then
                Cursor = Cursors.SizeNWSE
                resizeInfo.Modo = ResizeMode.TopLeft
            ElseIf IntersectaConRect(e, rectResizeTopRight) Then
                Cursor = Cursors.SizeNESW
                resizeInfo.Modo = ResizeMode.TopRight
            ElseIf IntersectaConRect(e, rectResizeRight) Then
                Cursor = Cursors.SizeWE
                resizeInfo.Modo = ResizeMode.Right
            ElseIf IntersectaConRect(e, rectResizeLeft) Then
                Cursor = Cursors.SizeWE
                resizeInfo.Modo = ResizeMode.Left
            ElseIf IntersectaConRect(e, rectResizeBottom) Then
                Cursor = Cursors.SizeNS
                resizeInfo.Modo = ResizeMode.Bottom
            ElseIf IntersectaConRect(e, rectResizeTop) Then
                Cursor = Cursors.SizeNS
                resizeInfo.Modo = ResizeMode.Top
            Else
                Cursor = Cursors.Default
                resizeInfo.Activado = False
            End If
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
        Cursor = Cursors.Default
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnDoubleClick(ByVal e As System.EventArgs)
        MyBase.OnDoubleClick(e)
        Dim CurPos As New Point(Windows.Forms.Cursor.Position.X - Me.Left, Windows.Forms.Cursor.Position.Y - Me.Top)
        If (MaximizeBox) AndAlso
            ((WindowState = FormWindowState.Normal AndAlso IntersectaConRect(CurPos, New Rectangle(_borderWidth, rectResizeTop.Height + 1, rectMinimizar.X - _borderWidth, _TitleHeight - rectResizeTop.Height))) Or
            (WindowState = FormWindowState.Maximized AndAlso IntersectaConRect(CurPos, New Rectangle(_borderWidth, 0, rectMinimizar.X - _borderWidth, _TitleHeight)))) Then
            If WindowState = FormWindowState.Normal Then
                WindowState = FormWindowState.Maximized
            ElseIf WindowState = FormWindowState.Maximized Then
                WindowState = FormWindowState.Normal
            End If
            Dim f As New MouseEventArgs(Windows.Forms.MouseButtons.Left, 1, CurPos.X, CurPos.Y, 0)
            OnMouseMove(f)
        End If
    End Sub

    Protected Overrides Sub OnMouseClick(ByVal e As System.Windows.Forms.MouseEventArgs)
        MyBase.OnMouseClick(e)
    End Sub

    Protected Overrides Sub OnResize(ByVal e As System.EventArgs)
        'tamaño de barras resize
        rectResizeRight.Size = New Size(BorderWidth, Height)
        rectResizeLeft.Size = New Size(BorderWidth, Height)
        rectResizeBottom.Size = New Size(Width, BorderWidth)
        ActualizaRectPosition()
        rectResizeTop.Size = New Size(rectMinimizar.X - _borderWidth, 8)
        MyBase.OnResize(e)
        Me.Invalidate()
    End Sub

    Dim CurPos1, CurPos2 As Rectangle
    Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        If IntersectaConRect(e, rectMinimizar) Then CurPos1 = rectMinimizar Else If IntersectaConRect(e, rectMaximizar) Then CurPos1 = rectMaximizar Else If IntersectaConRect(e, rectSalir) Then CurPos1 = rectSalir
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
                Not IntersectaConRect(e, rectSalir) AndAlso
                Not IntersectaConRect(e, rectMaximizar) AndAlso
                Not IntersectaConRect(e, rectMinimizar) Then
                'mover ventana
                ReleaseCapture()
                SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0)
            End If
        ElseIf resizeInfo.Activado Then
            ReleaseCapture()
            SendMessage(Handle, WM_NCLBUTTONDOWN, resizeInfo.Modo, 0)
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        If IntersectaConRect(e, rectMinimizar) Then CurPos2 = rectMinimizar Else If IntersectaConRect(e, rectMaximizar) Then CurPos2 = rectMaximizar Else If IntersectaConRect(e, rectSalir) Then CurPos2 = rectSalir
        If Not resizeInfo.Activado AndAlso CurPos1 = CurPos2 Then
            Select Case OverState
                Case Overbutton.Salir
                    Me.Close()
                Case Overbutton.Maximizar
                    If WindowState = FormWindowState.Maximized Then
                        WindowState = FormWindowState.Normal
                        Me.Invalidate()
                    Else
                        Dim rect As New Rectangle(0, 0, Me.Width, Me.Height)
                        WindowState = FormWindowState.Maximized
                        Me.Invalidate(rect)
                    End If
                Case Overbutton.Minimizar
                    WindowState = FormWindowState.Minimized
            End Select
            OverState = Overbutton.Ninguno
            'OnMouseMove(e)
        End If
        MyBase.OnMouseUp(e)
        resizeInfo.Activado = False
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
    ''' Indica si la posición del MouseEventArgs intersecta con la posición del botón Salir
    ''' </summary>
    ''' <param name="e">Argumentos de Mouse a analizar</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function IntersectaConRect(ByRef e As System.Windows.Forms.MouseEventArgs, ByRef rect As Rectangle)
        If e.X >= rect.X AndAlso e.X <= rect.X + rect.Width AndAlso e.Y >= rect.Y AndAlso e.Y <= rect.Y + rect.Height Then Return True
        Return False
    End Function

    Private Function IntersectaConRect(ByRef CursorPosition As Point, ByRef rect As Rectangle)
        If CursorPosition.X >= rect.X AndAlso CursorPosition.X <= rect.X + rect.Width AndAlso CursorPosition.Y >= rect.Y AndAlso CursorPosition.Y <= rect.Y + rect.Height Then Return True
        Return False
    End Function

    Private Sub DibujarIcono(ByVal icono As IconButton, ByVal color As Color, ByRef rect As Rectangle, ByRef g As Graphics)
        Dim pen As New Pen(color)
        Select Case icono
            Case IconButton.Restaurar
                'cuadro
                g.DrawRectangle(pen, New Rectangle(rect.X, rect.Y + 2, 8, 8))
                g.DrawLine(pen, New Point(rect.X + 1, rect.Y + 3), New Point(rect.X + 9 - 2, rect.Y + 3))
                'afuera
                g.DrawLine(pen, New Point(rect.X + 3, rect.Y + 0), New Point(rect.X + 10, rect.Y + 0))
                g.DrawLine(pen, New Point(rect.X + 10, rect.Y + 1), New Point(rect.X + 10, rect.Y + 7))
            Case IconButton.Minimizar
                g.FillRectangle(pen.Brush, New Rectangle(rect.X + 1, rect.Y + 7, 8, 2))
            Case IconButton.Maximizar
                g.DrawRectangle(pen, New Rectangle(rect.X + 0, rect.Y + 1, 10, 8))
                g.DrawLine(pen, New Point(rect.X + 1, rect.Y + 2), New Point(rect.X + 9, rect.Y + 2))
        End Select
    End Sub

    Private Sub ActualizarAnterior()
        Dim anterior As Overbutton = OverState
        OverState = Overbutton.Ninguno
        Select Case anterior
            Case Overbutton.Maximizar
                Me.Invalidate(rectMaximizar)
            Case Overbutton.Minimizar
                Me.Invalidate(rectMinimizar)
            Case Overbutton.Salir
                Me.Invalidate(rectSalir)
        End Select
    End Sub

    Private Sub InvalidarBoton(ByVal state As Overbutton)
        Select Case state
            Case Overbutton.Maximizar
                Invalidate(rectMaximizar)
            Case Overbutton.Minimizar
                Invalidate(rectMinimizar)
            Case Overbutton.Salir
                Invalidate(rectSalir)
        End Select
    End Sub

    Private Sub InvalidarCabecera()
        Me.Invalidate(New Rectangle(0, 0, Me.Width, Me.TitleHeight))
    End Sub

    Dim timerDBLCLK_running As Boolean = False
    Private Sub TimerDBLCLK_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles timerDblClk.Tick
        timerDBLCLK_running = False
        timerDblClk.Stop()
    End Sub

    'Public Function CreateFont(family As String, size As Single, Optional style As FontStyle = FontStyle.Regular, Optional unit As GraphicsUnit = GraphicsUnit.Point, Optional b As Byte = 0) As Font
    '    Return CreateFont(family, size, style, unit, b)
    'End Function

End Class

