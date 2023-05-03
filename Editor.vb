Public Class Editor
    Private Const FRAMERATE As Integer = 16
    Public Property Frame As Double
    Public Property Engine As Engine

    Private Sub Editor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.DoubleBuffered = True
        Me.Engine = New Engine(Me.ClientRectangle, 1, 14, 16)
        Me.Frame = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
        Me.Clock.Interval = Editor.FRAMERATE
        Me.Clock.Start()
    End Sub

    Private Sub Editor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Me.Clock.Stop()
    End Sub

    Private Sub Clock_Tick(sender As Object, e As EventArgs) Handles Clock.Tick
        Dim current As Double = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond
        Me.Tick(CSng((current - Me.Frame) / 1000))
        Me.Frame = current
    End Sub

    Private Sub Tick(dt As Single)
        If (Me.InvokeRequired) Then
            Me.Invoke(Sub() Me.Tick(dt))
        Else
            Using bm As New Bitmap(Me.Engine.Bounds.Width, Me.Engine.Bounds.Height)
                Using g As Graphics = Graphics.FromImage(bm)
                    g.Clear(Color.White)
                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    Me.Engine.Update(dt)
                    Me.Engine.Draw(g)
                End Using
                Me.BackgroundImage = CType(bm.Clone, Image)
            End Using
        End If
    End Sub
End Class
