Imports System.Windows.Forms
Imports System.Drawing

Public Class BooleanControl
    Inherits Control

    Public Enum BooleanControlStyles
        Rounded = 0
        SquareText = 1
    End Enum

    Dim _style As BooleanControlStyles
    Dim _timer As Timer
    Dim _value As Boolean
    Dim Rectangle As Rectangle
    Dim estado As Byte = 0
    Dim velocity As Single
    Dim _txtTrue As String
    Dim _txtFalse As String
    Dim _hover As Boolean

    ''' <summary>
    ''' Se desencadena cuando se cambia el valor del Control.
    ''' </summary>
    ''' <param name="sender">Objeto original que cambió su Valor y desencadenó el evento</param>
    ''' <remarks></remarks>
    Public Event ValueChanged(ByVal sender As Object, ByVal e As EventArgs)

    Sub New()
        MyBase.New()
        _txtFalse = "No"
        _txtTrue = "Yes"
        _hover = False
        Rectangle = New Rectangle(0, 0, Me.Width * 0.5F, Me.Height)
        UpdateVelocity()
        _timer = New Timer With {.Interval = 10}
        AddHandler _timer.Tick, AddressOf Timer_Tick
        _value = False
        BooleanControlStyle = BooleanControlStyles.SquareText
    End Sub

    Private Sub UpdateVelocity()
        'la idea es q recorra en 500 milisengundos
        velocity = Me.Width * 0.5F / 20.0F
    End Sub

    Public Property BooleanControlStyle As BooleanControlStyles
        Get
            Return _style
        End Get
        Set(ByVal value As BooleanControlStyles)
            If value <> _style Then
                _style = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Property TextFalse As String
        Get
            Return _txtFalse
        End Get
        Set(ByVal value As String)
            _txtFalse = value
            Me.Invalidate()
        End Set
    End Property

    Public Property TextTrue As String
        Get
            Return _txtTrue
        End Get
        Set(ByVal value As String)
            _txtTrue = value
            Me.Invalidate()
        End Set
    End Property

    Public Property Value As Boolean
        Get
            Return _value
        End Get
        Set(ByVal value As Boolean)
            If value <> _value Then
                _value = value
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
                Me.Invalidate()
            End If
        End Set
    End Property

    Protected Overrides Sub OnSizeChanged(ByVal e As System.EventArgs)
        MyBase.OnSizeChanged(e)
        Rectangle.Size = New Size(Me.Width * 0.5F, Me.Height - 1)
        UpdateVelocity()
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnMouseEnter(ByVal e As System.EventArgs)
        MyBase.OnMouseEnter(e)
        _hover = True
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
        MyBase.OnMouseLeave(e)
        _hover = False
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnClick(ByVal e As System.EventArgs)
        If estado = 0 Then
            Value = Not Value
            _timer.Start()
        End If
        MyBase.OnClick(e)
    End Sub

    Protected Sub Timer_Tick(ByVal sender As Timer, ByVal e As EventArgs)
        'principio: Rectangle.Location = New Point(0, 0)
        'final : Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, 0)
        estado = 1
        Select Case Me.BooleanControlStyle
            Case BooleanControlStyles.Rounded
                Select Case _value
                    Case True
                        'de false a true
                        If Rectangle.Location.X < Me.Width - Rectangle.Width - 1 Then 'en pleno vuelo
                            Rectangle.Location = New Point(Rectangle.X + velocity, Rectangle.Y)
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2.0F, Rectangle.Height))
                        Else 'llegó
                            Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, Rectangle.Y)
                            estado = 0
                            sender.Stop()
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2.0F, Rectangle.Height))
                        End If
                    Case False
                        'de true a false
                        If Rectangle.Location.X > 0 Then
                            Rectangle.Location = New Point(Rectangle.X - velocity, Rectangle.Y)
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3.0F, Rectangle.Height))
                        Else
                            Rectangle.Location = New Point(0, Rectangle.Y)
                            estado = 0
                            sender.Stop()
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3.0F, Rectangle.Height))
                        End If
                End Select
            Case BooleanControlStyles.SquareText
                estado = 0
                sender.Stop()
                'Me.Invalidate()
        End Select
    End Sub

    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        MyBase.OnPaint(e)
        Dim multEllipse As Single = 0.4F
        Dim diffHeight As Single = 0.05F * Me.Height '25%
        Dim diffWidth As Single = 0.1F * Me.Width

        Using g As Graphics = e.Graphics
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            

            Select Case BooleanControlStyle
                Case BooleanControlStyles.Rounded
                    Dim borde As Pen = New Pen(Brushes.Silver)
                    Dim fondoColor As New SolidBrush(Color.Aquamarine)
                    Select Case _value
                        Case True 'dibujar verde
                            fondoColor.Color = Color.LimeGreen
                            borde = New Pen(Brushes.LimeGreen)
                        Case False 'dibujar gris
                            fondoColor.Color = Color.WhiteSmoke 'gainsboro , lightgray
                            borde = New Pen(Brushes.Silver)
                    End Select
                    'dibujar fondo
                    g.FillEllipse(fondoColor, New Rectangle(New Point(diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight)))
                    g.FillEllipse(fondoColor, New Rectangle(New Point(Me.Width - Me.Width * multEllipse - diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight)))
                    g.FillRectangle(fondoColor, New Rectangle(New Point(Me.Width * (multEllipse / 2.0F) + diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * (1.0F - multEllipse) - diffWidth, Me.Height - diffHeight)))
                    'dibujar borde
                    Dim rectEllipse As New Rectangle(New Point(diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight))
                    g.DrawArc(borde, rectEllipse, 180, 90) 'sup. izq
                    g.DrawArc(borde, rectEllipse, 90, 90) 'inf. izq

                    rectEllipse.Location = New Point(Me.Width * (1.0F - multEllipse) - diffWidth / 2.0F, diffHeight / 2.0F)

                    g.DrawArc(borde, rectEllipse, 270, 90) 'sup. der
                    g.DrawArc(borde, rectEllipse, 0, 90) 'inf. der

                    g.DrawLine(borde, New Point(diffWidth / 2.0F + Me.Width * multEllipse * 0.5F, diffHeight / 2.0F), New Point(Me.Width - diffWidth / 2.0F - Me.Width * multEllipse * 0.5F, diffHeight / 2.0F)) 'línea superior
                    g.DrawLine(borde, New Point(diffWidth / 2.0F + Me.Width * multEllipse * 0.5F, Me.Height - diffHeight / 2.0F + 0), New Point(Me.Width - diffWidth / 2.0F - Me.Width * multEllipse * 0.5F, Me.Height - diffHeight / 2.0F + 0)) 'línea inferior


                    borde.Dispose()
                    fondoColor.Dispose()
                    'g.FillRectangle(Brushes.Transparent, New Rectangle(0, 0, Me.Width, Me.Height))
                    Dim borde1 As Pen = Nothing
                    Select Case Value
                        Case False
                            'Rectangle.Location = New Point(0, 0)
                            borde1 = New Pen(Brushes.Silver)
                        Case True
                            'Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, 0)
                            borde1 = New Pen(Brushes.LimeGreen)
                    End Select

                    g.FillEllipse(New SolidBrush(Color.WhiteSmoke), Rectangle)
                    g.DrawEllipse(borde1, Rectangle)
                    borde.Dispose()
                Case BooleanControlStyles.SquareText

                    Dim fondoBase As New SolidBrush(Color.DarkGray) 'With {.Color = Color.FromArgb(128, Color.Silver)}
                    Dim fondoBaseChico As New SolidBrush(Color.FromArgb(fondoBase.Color.R * 0.9F, fondoBase.Color.G * 0.9F, fondoBase.Color.B * 0.9F))
                    Dim fondoCuadroPalabra As New SolidBrush(Color.DarkGray)

                    Dim anchoCuadro As Single = 0.6F
                    Dim RectCuadro As New Rectangle(Point.Empty, New Size(Me.Width * anchoCuadro, Me.Height - 2))
                    Dim RectCuadroChico As New Rectangle(New Point((Me.Width - Me.Width * 0.7F) / 2.0F, RectCuadro.Height * 0.25F + 1), New Size(Me.Width * 0.7F, RectCuadro.Height / 2.0F))

                    Dim bruLineaSuperior As SolidBrush = Nothing
                    Dim bruLineaInferior As SolidBrush = Nothing

                    Select Case Value
                        Case False
                            fondoBase.Color = Color.FromArgb(255, Color.DarkGray)
                            fondoCuadroPalabra.Color = Color.FromArgb(255, Color.Firebrick)
                            RectCuadro.Location = New Point(0, 1)
                            bruLineaSuperior = New SolidBrush(Color.FromArgb(255, Color.IndianRed))
                            bruLineaInferior = New SolidBrush(Color.FromArgb(255, Color.DarkRed))
                        Case True
                            fondoBase.Color = Color.FromArgb(255, Color.DarkGray) 'forestgreen
                            fondoCuadroPalabra.Color = Color.FromArgb(255, Color.ForestGreen)
                            RectCuadro.Location = New Point(Me.Width * (1.0F - anchoCuadro), 1)
                            bruLineaSuperior = New SolidBrush(Color.FromArgb(255, Color.LimeGreen))
                            bruLineaInferior = New SolidBrush(Color.FromArgb(255, Color.DarkGreen))
                    End Select

                    'dibujar fondo gris
                    Dim espaciadoBorde As Integer = Me.Height * 0.1F
                    Dim pathFondo As New Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                    pathFondo.AddLines({New Point(0 + espaciadoBorde, 0),
                                        New Point(Me.Width - 1 - espaciadoBorde, 0),
                                        New Point(Me.Width - 1, espaciadoBorde),
                                        New Point(Me.Width - 1, Me.Height - 1 - espaciadoBorde),
                                        New Point(Me.Width - 1 - espaciadoBorde, Me.Height - 1),
                                        New Point(0 + espaciadoBorde, Me.Height - 1),
                                        New Point(0, Me.Height - espaciadoBorde),
                                        New Point(0, espaciadoBorde),
                                        New Point(0 + espaciadoBorde, 0)
                                       })

                    Dim espaciadoBorde2 As Single = espaciadoBorde / 2.0F
                    Dim pathChico As New Drawing2D.GraphicsPath
                    pathChico.AddLines({New Point(RectCuadroChico.Location.X + espaciadoBorde2, RectCuadroChico.Location.Y),
                                   New Point(RectCuadroChico.Location.X + RectCuadroChico.Width - espaciadoBorde2, RectCuadroChico.Location.Y),
                                   New Point(RectCuadroChico.Location.X + RectCuadroChico.Width, RectCuadroChico.Y + espaciadoBorde2),
                                   New Point(RectCuadroChico.Location.X + RectCuadroChico.Width, RectCuadroChico.Location.Y + RectCuadroChico.Height - espaciadoBorde2),
                                   New Point(RectCuadroChico.Location.X + RectCuadroChico.Width - espaciadoBorde2, RectCuadroChico.Location.Y + RectCuadroChico.Height),
                                   New Point(RectCuadroChico.Location.X + espaciadoBorde2, RectCuadroChico.Location.Y + RectCuadroChico.Height),
                                   New Point(RectCuadroChico.Location.X, RectCuadroChico.Location.Y + RectCuadroChico.Height - espaciadoBorde2),
                                   New Point(RectCuadroChico.Location.X, RectCuadroChico.Location.Y + espaciadoBorde2),
                                   New Point(RectCuadroChico.Location.X + espaciadoBorde2, RectCuadroChico.Location.Y)
                                       })

                    g.FillPath(fondoBase, pathFondo)

                    g.FillPath(fondoBaseChico, pathChico)
                    'fin dibujo fondo gris

                    'dibujar cuadro que contiene palabra
                    Dim path As New Drawing2D.GraphicsPath
                    path.AddLines({New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y),
                                   New Point(RectCuadro.Location.X + RectCuadro.Width - espaciadoBorde, RectCuadro.Location.Y),
                                   New Point(RectCuadro.Location.X + RectCuadro.Width, RectCuadro.Y + espaciadoBorde),
                                   New Point(RectCuadro.Location.X + RectCuadro.Width, RectCuadro.Location.Y + RectCuadro.Height - espaciadoBorde),
                                   New Point(RectCuadro.Location.X + RectCuadro.Width - espaciadoBorde, RectCuadro.Location.Y + RectCuadro.Height),
                                   New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y + RectCuadro.Height),
                                   New Point(RectCuadro.Location.X, RectCuadro.Location.Y + RectCuadro.Height - espaciadoBorde),
                                   New Point(RectCuadro.Location.X, RectCuadro.Location.Y + espaciadoBorde),
                                   New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y)
                                  })

                    g.FillPath(fondoCuadroPalabra, path)
                    'fin dbujo cuadro

                    'dibujar efecto de luz si mouse está encima
                    If _hover Then
                        Dim pathSup As New Drawing2D.GraphicsPath
                        Dim pathInf As New Drawing2D.GraphicsPath

                        pathSup.AddLines({New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y),
                                          New Point(RectCuadro.Location.X + RectCuadro.Width - espaciadoBorde, RectCuadro.Location.Y),
                                          New Point(RectCuadro.Location.X + RectCuadro.Width, RectCuadro.Y + espaciadoBorde),
                                          New Point(RectCuadro.Location.X, RectCuadro.Location.Y + espaciadoBorde),
                                          New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y)
                                         })
                        pathInf.AddLines({New Point(RectCuadro.Location.X + RectCuadro.Width, RectCuadro.Location.Y + RectCuadro.Height - espaciadoBorde),
                                          New Point(RectCuadro.Location.X + RectCuadro.Width - espaciadoBorde, RectCuadro.Location.Y + RectCuadro.Height),
                                          New Point(RectCuadro.Location.X + espaciadoBorde, RectCuadro.Location.Y + RectCuadro.Height),
                                          New Point(RectCuadro.Location.X, RectCuadro.Location.Y + RectCuadro.Height - espaciadoBorde),
                                          New Point(RectCuadro.Location.X + RectCuadro.Width, RectCuadro.Location.Y + RectCuadro.Height - espaciadoBorde)
                                         })
                        g.FillPath(bruLineaSuperior, pathSup)
                        g.FillPath(bruLineaInferior, pathInf)

                        pathSup.Dispose()
                        pathInf.Dispose()
                    End If
                    'fin efecto luz

                    'dibujar palabra
                    Dim fontPalabra As Font = Core.FuncionesGenerales.CreateFont("Arial", Me.Height * 0.5, FontStyle.Bold, GraphicsUnit.Pixel)
                    Dim medicion As Size = TextRenderer.MeasureText(IIf(Value, TextTrue, TextFalse), fontPalabra)
                    g.DrawString(IIf(Value, TextTrue, TextFalse), fontPalabra, Brushes.White, New Point(RectCuadro.Location.X + (RectCuadro.Width - medicion.Width) / 2, (RectCuadro.Height - medicion.Height) / 2 + RectCuadro.Height * 0.05F))
                    'fin dibujar palabra

                    'liberar recursos
                    path.Dispose()
                    pathFondo.Dispose()
                    bruLineaInferior.Dispose()
                    bruLineaSuperior.Dispose()
                    fondoBase.Dispose()
                    fondoCuadroPalabra.Dispose()
            End Select
        End Using
    End Sub
End Class
