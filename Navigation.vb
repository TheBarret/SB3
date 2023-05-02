Imports System.Drawing.Drawing2D
Public Class Navigation
    Inherits Particle
    Public Property Rays As List(Of Ray)
    Public Property RaysCount As Integer
    Public Property RaysLength As Integer

    Sub New(position As Vector2)
        MyBase.New(position)
        Me.RaysCount = 10
        Me.RaysLength = 50
        Me.Max = 75.0F
        Me.Radius = 10.0F
        Me.Friction = 0.01F
        Me.Rays = New List(Of Ray)
    End Sub

#Region "Function Overrides"
    Public Overrides Sub OnUpdate(e As Engine, dt As Single)

    End Sub

    Public Overrides Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)

    End Sub

    Public Overrides Sub OnContact(e As Engine, other As Particle, contact As Vector2, force As Vector2, dt As Single)

    End Sub

    Public Overrides Sub OnDrawing(e As Engine, g As Graphics, srcRect As RectangleF)

    End Sub
#End Region
#Region "Raycast Detection Behaviors"
    Private Sub HandleRays(dt As Single)

    End Sub
#End Region
#Region "Steering Behaviors"
    Public Sub Pursue(target As Particle, multiplier As Single)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = (Me.Sensor + target.Sensor) - d
        Dim netforce As Single = overlap * multiplier
        Dim predicted As Vector2 = target.Position + target.Velocity * (d / Me.Max)
        Dim offset As Vector2 = predicted - Me.Position
        If (d > 0) Then r = (Vector2.Normalize(e) * Me.Max) - Me.Velocity
        Me.Velocity += Vector2.Normalize(offset) * netforce
    End Sub

    Public Sub Evade(target As Particle, multiplier As Single)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim predicted As Vector2 = (target.Position + target.Velocity * (d / Me.Max)) - Me.Position
        Dim overlap As Single = (Me.Sensor + target.Sensor) - d
        Dim netforce As Single = overlap * multiplier
        If (d > 0) Then r = (Vector2.Normalize(e) * Me.Max) - Me.Velocity
        Me.Velocity -= r * netforce
    End Sub

    Public Sub Arrival(target As Particle, multiplier As Single)
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        If (d > 0) Then
            e = Vector2.Normalize(e)
            If (d < Me.Sensor) Then
                e *= Me.Max * (d / Me.Sensor)
            Else
                e *= Me.Max
            End If
            Me.Velocity += (e - Me.Velocity) * multiplier
        End If
    End Sub

    Public Sub Departure(target As Particle, multiplier As Single)
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        If (d > 0) Then
            e = Vector2.Normalize(e)
            If (d < Me.Sensor) Then
                e *= Me.Max * (d / Me.Sensor)
            Else
                e *= Me.Max
            End If
            Me.Velocity -= (e - Me.Velocity) * multiplier
        End If
    End Sub

    Public Sub Seek(target As Particle, multiplier As Single)
        Dim v As Vector2 = Vector2.Null
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Center - Me.Center
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = (Me.Sensor) - d
        Dim netforce As Single = overlap * multiplier
        If (d > 0) Then
            e = Vector2.Normalize(e) * Me.Max
            r = e - Me.Velocity
        End If
        Me.Velocity += r * netforce
    End Sub

    Public Sub Flee(target As Particle, multiplier As Single)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = (Me.Sensor) - d
        Dim netforce As Single = overlap * multiplier
        If (d > 0) Then
            e = Vector2.Normalize(e) * Me.Max
            r = e - Me.Velocity
        End If
        Me.Velocity -= r * netforce
    End Sub
#End Region

#Region "Effects"
    Public Sub Explode(e As Engine, impact As Vector2, radius As Single, force As Single)
        For Each p As Particle In e.Particles
            If (p Is Me) Then Continue For
            Dim dist As Single = Vector2.Distance(p.Position, impact)
            If dist <= radius Then
                Dim direction As Vector2 = Vector2.Normalize(p.Position - impact)
                p.Velocity += direction * (force / (dist * dist))
            End If
        Next
    End Sub

    Public Sub Implode(e As Engine, impact As Vector2, radius As Single, force As Single)
        For Each p As Particle In e.Particles
            If (p Is Me) Then Continue For
            Dim dist As Single = Vector2.Distance(p.Position, impact)
            If dist <= radius Then
                Dim direction As Vector2 = Vector2.Normalize(p.Position - impact)
                p.Velocity -= direction * (force / (dist * dist))
            End If
        Next
    End Sub
#End Region
End Class
