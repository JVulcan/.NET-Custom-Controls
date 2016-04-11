Imports System.Windows.Forms
Imports System.Drawing

Public Class BooleanControl
    Inherits Control

    '
    '       WARNING !!
    '
    '       ROUNDED TYPE IS UGLY AND UNSUITABLE FOR WORK
    '       THE FORMS "ENGINE" DOESNT FIT WITH I WAS TRYING TO DO FOR THE ROUNDED ONE
    '       BUT YOU CAN CHECK IT OUT

    Public Enum BooleanControlStyles
        Rounded = 0
        SquareText = 1
    End Enum

    Dim _style As BooleanControlStyles
    Dim _timer As Timer
    Dim _value As Boolean
    Dim Rectangle As Rectangle
    Dim state As Byte = 0
    Dim velocity As Single
    Dim _txtTrue As String
    Dim _txtFalse As String
    Dim _hover As Boolean

    ''' <summary>
    ''' Triggered when the value of Control has changed.
    ''' </summary>
    ''' <param name="sender">original Object that changed its value and triggered the event</param>
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

    ' just for rounded type
    Private Sub UpdateVelocity()
        'the idea is it travels in 500 milliseconds
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

    ''' <summary>
    ''' (SquareText-Style only) What the Text says when the value represents FALSE (default: NO)
    ''' </summary>
    ''' <returns></returns>
    Public Property TextFalse As String
        Get
            Return _txtFalse
        End Get
        Set(ByVal value As String)
            _txtFalse = value
            Me.Invalidate()
        End Set
    End Property
    ''' <summary>
    ''' (SquareText-Style only) What the Text says when the value represents TRUE (default: YES)
    ''' </summary>
    ''' <returns></returns>
    Public Property TextTrue As String
        Get
            Return _txtTrue
        End Get
        Set(ByVal value As String)
            _txtTrue = value
            Me.Invalidate()
        End Set
    End Property
    ''' <summary>
    ''' Actual value (true or false)
    ''' </summary>
    ''' <returns></returns>
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
        If state = 0 Then
            Value = Not Value
            _timer.Start()
        End If
        MyBase.OnClick(e)
    End Sub

    Protected Sub Timer_Tick(ByVal sender As Timer, ByVal e As EventArgs)
        'Start: Rectangle.Location = New Point(0, 0)
        'End  : Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, 0)
        state = 1
        Select Case Me.BooleanControlStyle
            Case BooleanControlStyles.Rounded
                Select Case _value
                    Case True
                        'from false to true
                        If Rectangle.Location.X < Me.Width - Rectangle.Width - 1 Then 'in "midair"
                            Rectangle.Location = New Point(Rectangle.X + velocity, Rectangle.Y)
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2.0F, Rectangle.Height))
                        Else 'it arrived
                            Rectangle.Location = New Point(Me.Width - Rectangle.Width - 1, Rectangle.Y)
                            state = 0
                            sender.Stop()
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 2.0F, Rectangle.Height))
                        End If
                    Case False
                        'from true to false
                        If Rectangle.Location.X > 0 Then
                            Rectangle.Location = New Point(Rectangle.X - velocity, Rectangle.Y)
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3.0F, Rectangle.Height))
                        Else
                            Rectangle.Location = New Point(0, Rectangle.Y)
                            state = 0
                            sender.Stop()
                            Me.Invalidate(New Rectangle(Rectangle.Location.X - velocity, 0, Rectangle.Width + velocity * 3.0F, Rectangle.Height))
                        End If
                End Select
            Case BooleanControlStyles.SquareText
                state = 0
                sender.Stop()
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
                    Dim border As Pen = New Pen(Brushes.Silver)
                    Dim _backColor As New SolidBrush(Color.Aquamarine)
                    Select Case _value
                        Case True 'paint green
                            _backColor.Color = Color.LimeGreen
                            border = New Pen(Brushes.LimeGreen)
                        Case False 'paint gray
                            _backColor.Color = Color.WhiteSmoke 'gainsboro , lightgray
                            border = New Pen(Brushes.Silver)
                    End Select
                    'draw background
                    g.FillEllipse(_backColor, New Rectangle(New Point(diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight)))
                    g.FillEllipse(_backColor, New Rectangle(New Point(Me.Width - Me.Width * multEllipse - diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight)))
                    g.FillRectangle(_backColor, New Rectangle(New Point(Me.Width * (multEllipse / 2.0F) + diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * (1.0F - multEllipse) - diffWidth, Me.Height - diffHeight)))
                    'draw border
                    Dim rectEllipse As New Rectangle(New Point(diffWidth / 2.0F, diffHeight / 2.0F), New Size(Me.Width * multEllipse, Me.Height - diffHeight))
                    g.DrawArc(border, rectEllipse, 180, 90) 'upper left
                    g.DrawArc(border, rectEllipse, 90, 90) 'bottom left

                    rectEllipse.Location = New Point(Me.Width * (1.0F - multEllipse) - diffWidth / 2.0F, diffHeight / 2.0F)

                    g.DrawArc(border, rectEllipse, 270, 90) 'upper right
                    g.DrawArc(border, rectEllipse, 0, 90) 'bottom right

                    g.DrawLine(border, New Point(diffWidth / 2.0F + Me.Width * multEllipse * 0.5F, diffHeight / 2.0F), New Point(Me.Width - diffWidth / 2.0F - Me.Width * multEllipse * 0.5F, diffHeight / 2.0F)) 'línea superior
                    g.DrawLine(border, New Point(diffWidth / 2.0F + Me.Width * multEllipse * 0.5F, Me.Height - diffHeight / 2.0F + 0), New Point(Me.Width - diffWidth / 2.0F - Me.Width * multEllipse * 0.5F, Me.Height - diffHeight / 2.0F + 0)) 'línea inferior


                    border.Dispose()
                    _backColor.Dispose()

                    Dim border1 As Pen = Nothing
                    Select Case Value
                        Case False
                            border1 = New Pen(Brushes.Silver)
                        Case True
                            border1 = New Pen(Brushes.LimeGreen)
                    End Select

                    g.FillEllipse(New SolidBrush(Color.WhiteSmoke), Rectangle)
                    g.DrawEllipse(border1, Rectangle)
                    border.Dispose()
                Case BooleanControlStyles.SquareText

                    Dim backBase As New SolidBrush(Color.DarkGray) 'With {.Color = Color.FromArgb(128, Color.Silver)}
                    Dim backSmallBase As New SolidBrush(Color.FromArgb(backBase.Color.R * 0.9F, backBase.Color.G * 0.9F, backBase.Color.B * 0.9F))
                    Dim backTextBox As New SolidBrush(Color.DarkGray)

                    Dim widthBox As Single = 0.6F
                    Dim RectBox As New Rectangle(Point.Empty, New Size(Me.Width * widthBox, Me.Height - 2))
                    Dim RectSmallBox As New Rectangle(New Point((Me.Width - Me.Width * 0.7F) / 2.0F, RectBox.Height * 0.25F + 1), New Size(Me.Width * 0.7F, RectBox.Height / 2.0F))

                    Dim bruSuperiorLine As SolidBrush = Nothing
                    Dim bruInferiorLine As SolidBrush = Nothing

                    Select Case Value
                        Case False
                            backBase.Color = Color.FromArgb(255, Color.DarkGray)
                            backTextBox.Color = Color.FromArgb(255, Color.Firebrick)
                            RectBox.Location = New Point(0, 1)
                            bruSuperiorLine = New SolidBrush(Color.FromArgb(255, Color.IndianRed))
                            bruInferiorLine = New SolidBrush(Color.FromArgb(255, Color.DarkRed))
                        Case True
                            backBase.Color = Color.FromArgb(255, Color.DarkGray) 'forestgreen
                            backTextBox.Color = Color.FromArgb(255, Color.ForestGreen)
                            RectBox.Location = New Point(Me.Width * (1.0F - widthBox), 1)
                            bruSuperiorLine = New SolidBrush(Color.FromArgb(255, Color.LimeGreen))
                            bruInferiorLine = New SolidBrush(Color.FromArgb(255, Color.DarkGreen))
                    End Select

                    'draw gray background
                    Dim BorderSpacing As Integer = Me.Height * 0.1F
                    Dim pathBack As New Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                    pathBack.AddLines({New Point(0 + BorderSpacing, 0),
                                        New Point(Me.Width - 1 - BorderSpacing, 0),
                                        New Point(Me.Width - 1, BorderSpacing),
                                        New Point(Me.Width - 1, Me.Height - 1 - BorderSpacing),
                                        New Point(Me.Width - 1 - BorderSpacing, Me.Height - 1),
                                        New Point(0 + BorderSpacing, Me.Height - 1),
                                        New Point(0, Me.Height - BorderSpacing),
                                        New Point(0, BorderSpacing),
                                        New Point(0 + BorderSpacing, 0)
                                       })

                    Dim BorderSpacing2 As Single = BorderSpacing / 2.0F
                    Dim pathSmall As New Drawing2D.GraphicsPath
                    pathSmall.AddLines({New Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y),
                                   New Point(RectSmallBox.Location.X + RectSmallBox.Width - BorderSpacing2, RectSmallBox.Location.Y),
                                   New Point(RectSmallBox.Location.X + RectSmallBox.Width, RectSmallBox.Y + BorderSpacing2),
                                   New Point(RectSmallBox.Location.X + RectSmallBox.Width, RectSmallBox.Location.Y + RectSmallBox.Height - BorderSpacing2),
                                   New Point(RectSmallBox.Location.X + RectSmallBox.Width - BorderSpacing2, RectSmallBox.Location.Y + RectSmallBox.Height),
                                   New Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y + RectSmallBox.Height),
                                   New Point(RectSmallBox.Location.X, RectSmallBox.Location.Y + RectSmallBox.Height - BorderSpacing2),
                                   New Point(RectSmallBox.Location.X, RectSmallBox.Location.Y + BorderSpacing2),
                                   New Point(RectSmallBox.Location.X + BorderSpacing2, RectSmallBox.Location.Y)
                                       })

                    g.FillPath(backBase, pathBack)

                    g.FillPath(backSmallBase, pathSmall)
                    'end draw gray background

                    'draw box that contains the text
                    Dim path As New Drawing2D.GraphicsPath
                    path.AddLines({New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y),
                                   New Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y),
                                   New Point(RectBox.Location.X + RectBox.Width, RectBox.Y + BorderSpacing),
                                   New Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing),
                                   New Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y + RectBox.Height),
                                   New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y + RectBox.Height),
                                   New Point(RectBox.Location.X, RectBox.Location.Y + RectBox.Height - BorderSpacing),
                                   New Point(RectBox.Location.X, RectBox.Location.Y + BorderSpacing),
                                   New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y)
                                  })

                    g.FillPath(backTextBox, path)
                    'end draw box

                    'draw effect of highlight if mouse is hover
                    If _hover Then
                        Dim pathSup As New Drawing2D.GraphicsPath
                        Dim pathInf As New Drawing2D.GraphicsPath

                        pathSup.AddLines({New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y),
                                          New Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y),
                                          New Point(RectBox.Location.X + RectBox.Width, RectBox.Y + BorderSpacing),
                                          New Point(RectBox.Location.X, RectBox.Location.Y + BorderSpacing),
                                          New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y)
                                         })
                        pathInf.AddLines({New Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing),
                                          New Point(RectBox.Location.X + RectBox.Width - BorderSpacing, RectBox.Location.Y + RectBox.Height),
                                          New Point(RectBox.Location.X + BorderSpacing, RectBox.Location.Y + RectBox.Height),
                                          New Point(RectBox.Location.X, RectBox.Location.Y + RectBox.Height - BorderSpacing),
                                          New Point(RectBox.Location.X + RectBox.Width, RectBox.Location.Y + RectBox.Height - BorderSpacing)
                                         })
                        g.FillPath(bruSuperiorLine, pathSup)
                        g.FillPath(bruInferiorLine, pathInf)

                        pathSup.Dispose()
                        pathInf.Dispose()
                    End If
                    'end highlight effect

                    'draw text
                    Dim fontPalabra As Font = Core.FuncionesGenerales.CreateFont("Arial", Me.Height * 0.5, FontStyle.Bold, GraphicsUnit.Pixel)
                    Dim medicion As Size = TextRenderer.MeasureText(IIf(Value, TextTrue, TextFalse), fontPalabra)
                    g.DrawString(IIf(Value, TextTrue, TextFalse), fontPalabra, Brushes.White, New Point(RectBox.Location.X + (RectBox.Width - medicion.Width) / 2, (RectBox.Height - medicion.Height) / 2 + RectBox.Height * 0.05F))
                    'end draw text

                    'free resources
                    path.Dispose()
                    pathBack.Dispose()
                    bruInferiorLine.Dispose()
                    bruSuperiorLine.Dispose()
                    backBase.Dispose()
                    backTextBox.Dispose()
            End Select
        End Using
    End Sub
End Class
