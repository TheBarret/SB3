Imports System.Collections.ObjectModel

Public Class Boid
    Inherits Particle
    Public Property Grouped As Boolean
    Sub New(position As Vector2)
        MyBase.New(position, 20, False)
        Me.Max = 75.0F
        Me.Radius = 10.0F
        Me.Friction = 0.01F
        Me.Tint = If(Me.Charge > 0.5, Color.Red, Color.Blue)
    End Sub

    Public Overrides Sub OnDrawing(e As Engine, g As Graphics, center As Vector2, srcRect As RectangleF)
        Dim heading As Vector2 = Me.Heading(Me.Radius * 0.5)

        Using sb As New SolidBrush(Me.Tint)
            g.FillEllipse(sb, srcRect)
            Using p As New Pen(Color.WhiteSmoke, 2)
                g.DrawLine(p, center.X, center.Y, center.X + heading.X, center.Y + heading.Y)
            End Using
            g.DrawEllipse(Pens.Black, srcRect)
        End Using
    End Sub

    Public Overrides Function OnRange(e As Engine, length As Single, dt As Single) As Single
        Dim r As Single = Me.Radius
        If Me.Neighbors.Count >= 1 Then
            Dim total As Double = 0
            For Each n In Me.Neighbors
                Dim offset As Double = (length - ((n.Center - Me.Center).Length)) / length
                r += n.Mass * n.Sensor * 1.5
                total += offset
            Next
            r /= total
        End If
        Return Me.Radius + 5 + Math.Min(50, r)
    End Function

    Public Overrides Sub OnUpdate(e As Engine, dt As Single)
        'Me.Velocity += Me.Signal(0.01, 1, dt)
    End Sub

    Public Overrides Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)
        Me.Velocity += Me.Alignment(0.2)
        Me.Velocity += Me.Cohesion(0.2)
        Me.Evade(other, 0.03, 0.5)
    End Sub

    Public Overrides Sub OnCollision(e As Engine, other As Particle, contact As Vector2, force As Vector2, dt As Single)
        Me.Evade(other, 0.06, 0.5)
    End Sub

End Class
