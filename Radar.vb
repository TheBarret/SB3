Public Class Radar
    Public Property Parent As Particle
    Public Property RayIndex As Integer
    Public Property RayCount As Integer
    Public Property RayLength As Integer
    Public Property RayAngle As Single
    Public Property Rays As List(Of Ray)


    Sub New(p As Particle, amount As Integer, length As Single)
        Me.RayIndex = 0
        Me.RayAngle = Engine.RRange(0, 360)
        Me.Parent = p
        Me.RayCount = amount
        Me.RayLength = length
        Me.Rays = New List(Of Ray)
        SyncLock Me.Rays
            Dim angle As Single = 0
            Dim increment As Single = (2 * Math.PI) / amount
            For i As Integer = 0 To amount - 1
                Dim x As Single = p.Center.X + (length * Math.Sin(angle))
                Dim y As Single = p.Center.Y + (length * Math.Cos(angle))
                Me.Rays.Add(New Ray(p, New Vector2(x, y), length))
                angle += increment
            Next
        End SyncLock
    End Sub

    Public Sub Update(dt As Single)
        Dim increment As Single = (2 * Math.PI) / Me.Rays.Count
        Me.RayAngle += (1 * dt) * dt
        For i As Integer = 0 To Me.Rays.Count - 1
            Dim x As Single = Me.Parent.Center.X + (Me.Rays(i).Length * Math.Sin(increment * i + Me.RayAngle))
            Dim y As Single = Me.Parent.Center.Y + (Me.Rays(i).Length * Math.Cos(increment * i + Me.RayAngle))
            Me.Rays(i).Endpoint = New Vector2(x, y)
        Next
        Me.RayIndex += 1
        If (Me.RayIndex > Me.RayCount - 1) Then Me.RayIndex = 0
    End Sub

    Public Sub Draw(g As Graphics)

    End Sub

    'Private Sub UpdateRays(dt As Single)
    '    Dim increment As Single = (2 * Math.PI) / Me.Rays.Count
    '    Dim speed As Single = Me.Sensor * dt
    '    Me.RayAngle += speed * dt
    '    For i As Integer = 0 To Me.Rays.Count - 1
    '        Dim x As Single = Me.Center.X + (Me.Rays(i).Length * Math.Sin(increment * i + Me.RayAngle))
    '        Dim y As Single = Me.Center.Y + (Me.Rays(i).Length * Math.Cos(increment * i + Me.RayAngle))
    '        Me.Rays(i).Length = Me.Sensor
    '        Me.Rays(i).Endpoint = New Vector2(x, y)
    '    Next
    'End Sub

    'Private Function HandleRays(other As Particle, ByRef contact As Vector2, dt As Single) As Boolean
    '    For Each ray As Ray In Me.Rays
    '        Dim endpoint As Vector2 = ray.Particle.Center + Vector2.Normalize(ray.Endpoint - ray.Particle.Center) * ray.Length
    '        If Vector2.Distance(endpoint, other.Center) < Me.Sensor + other.Sensor Then
    '            contact = ray.Endpoint
    '            Return True
    '        End If
    '    Next
    '    Return False
    'End Function

End Class
