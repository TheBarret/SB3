Public Class Boid
    Inherits Particle
    Sub New(position As Vector2)
        MyBase.New(position, 15, False)
        Me.Max = 75.0F
        Me.Radius = 15.0F
        Me.Friction = 0.001F
    End Sub

    Public Overrides Sub OnDrawing(e As Engine, g As Graphics, srcRect As RectangleF)
        Dim center As Vector2 = Me.Center
        Dim half As Single = Me.Sensor * 0.5
        Dim heading As Vector2 = Me.Heading(Me.Radius * 0.5)
        Using sb As New SolidBrush(Me.GetTint)
            g.FillEllipse(sb, srcRect)
            Using p As New Pen(Color.Black, 2)
                g.DrawLine(p, center.X, center.Y, center.X + heading.X, center.Y + heading.Y)
                'For Each r As Ray In Me.Rays
                '    g.DrawLine(p, Me.Center.ToPointF, r.Endpoint.ToPointF)
                'Next
            End Using
            g.DrawEllipse(Pens.Black, srcRect)
        End Using
    End Sub

    Public Overrides Function OnRange(e As Engine, length As Single, dt As Single) As Single
        Dim radius As Single = Me.Radius
        If Me.Neighbors.Count >= 1 Then
            Dim total As Double = 0
            For Each n In Me.Neighbors
                Dim offset As Double = (length - ((n.Center - Me.Center).Length)) / length
                radius += n.Mass * n.Sensor * 1.5
                total += offset
            Next
            radius /= total
        End If
        Return Me.Radius + 5 + Math.Min(20.0F, radius)
    End Function

    Public Overrides Sub OnUpdate(e As Engine, dt As Single)
        Dim rng As Vector2 = Me.Signal(1, 1, dt)
        Me.Velocity += rng
        Me.RaysAngle = rng.Length * 0.1
    End Sub

    Public Overrides Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)
        Me.Evade(other, 0.001)
    End Sub

    Public Overrides Sub OnCollision(e As Engine, other As Particle, contact As Vector2, force As Vector2, dt As Single)
        Me.Evade(other, 0.1)
    End Sub

    Public Overrides Sub OnSensor(e As Engine, other As Particle, contact As Vector2, dt As Single)
        Me.Velocity += Me.Alignment(0.05)
        Me.Velocity += Me.Cohesion(0.05)
    End Sub

End Class
