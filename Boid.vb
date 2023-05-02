Public Class Boid
    Inherits Navigation

    Sub New(position As Vector2)
        MyBase.New(position)
    End Sub

    Public Overrides Sub OnDrawing(e As Engine, g As Graphics, srcRect As RectangleF)
        Dim center As Vector2 = Me.Center
        Dim half As Single = Me.Sensor * 0.5
        Dim heading As Vector2 = Me.Heading(half * 0.5)
        Using sb As New SolidBrush(Me.GetTint)
            g.FillEllipse(sb, srcRect)
            Using p As New Pen(Me.GetTint, 1)
                g.DrawEllipse(p, Me.Center.X - half, Me.Center.Y - half, Me.Sensor, Me.Sensor)
                g.DrawLine(p, center.X, center.Y, center.X + heading.X, center.Y + heading.Y)
            End Using
            g.DrawEllipse(Pens.Black, srcRect)
        End Using
    End Sub

    Public Overrides Sub OnUpdate(e As Engine, dt As Single)

    End Sub

    Public Overrides Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)
        Me.Evade(other, 0.007)
        If (Me.Mass < other.Mass) Then
            Me.Seek(other, 0.005)
        Else
            Me.Flee(other, 0.005)
        End If
    End Sub

End Class
