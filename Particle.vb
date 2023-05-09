Imports System.Drawing.Drawing2D

Public MustInherit Class Particle
    Public Property Tint As Color
    Public Property Speed As Single
    Public Property Max As Single
    Public Property Mass As Single
    Public Property Radius As Single
    Public Property Sensor As Single
    Public Property Friction As Single
    Public Property Restitution As Single
    Public Property Position As Vector2
    Public Property Velocity As Vector2
    Public Property Elapsed As Single
    Public Property Label As String
    Public Property LabelFont As Font
    Public Property LabelSize As SizeF
    Public Property LabelBrush As SolidBrush
    Public Property LabelTimeout As Date
    Public Property Charge As Single
    Public Property IsPinned As Boolean
    Public Property Neighbors As HashSet(Of Particle)
    Public MustOverride Sub OnUpdate(e As Engine, dt As Single)
    Public MustOverride Function OnRange(e As Engine, length As Single, dt As Single) As Single
    Public MustOverride Sub OnDrawing(e As Engine, g As Graphics, center As Vector2, srcRect As RectangleF)
    Public MustOverride Sub OnCollision(e As Engine, other As Particle, contact As Vector2, force As Vector2, dt As Single)
    Public MustOverride Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)

    Sub New(position As Vector2, Optional rays As Integer = 4, Optional isPinned As Boolean = False)
        Me.Max = 100.0F
        Me.Radius = 15.0F
        Me.Friction = 0.003F
        Me.Restitution = 0.5F
        Me.Tint = Color.Black
        Me.Position = position
        Me.IsPinned = isPinned
        Me.Mass = 1.0F + Engine.RFloat(Vector2.Epsilon, 1.0F)
        Me.Neighbors = New HashSet(Of Particle)
        Me.Reset()
    End Sub

    Public Sub Reset()
        Me.Elapsed = 0F
        Me.Label = String.Empty
        Me.LabelTimeout = Date.Now
        Me.LabelFont = New Font("Consolas", 8)
        Me.LabelBrush = New SolidBrush(Color.Black)
        Me.Velocity = Vector2.Null
        SyncLock Me.Neighbors
            Me.Neighbors.Clear()
        End SyncLock
        Me.Charge = Engine.RFloat(0, 1.0)
    End Sub

    Public Sub AddForce(force As Vector2)
        Me.Velocity += force / Me.Mass
    End Sub

    Public Sub Update(e As Engine, dt As Single)
        ' Return when we are pinned
        If (Me.IsPinned) Then Return
        ' update time accumulation
        Me.Elapsed += dt
        ' Get sensor range
        Me.Speed = Me.Velocity.Length
        Dim range As Single = Me.OnRange(e, Me.Speed, dt)
        If (range < Me.Sensor) Then Me.Sensor -= 0.7
        If (range > Me.Sensor) Then Me.Sensor += 0.7
        If (Me.Speed > 0F) Then
            Me.ScanNeighbors(e, Me)
            Me.HandleNeighbors(e, dt)
            Me.OnUpdate(e, dt)
            Me.HandleFriction(Me.Friction, dt)
            Me.HandleSpeedLimit(Me.Velocity, Me.Max)
            Me.UpdateVelocity(dt)
            Me.KeepBounds(e, 0.999)
        End If
    End Sub

    Public Sub Draw(e As Engine, g As Graphics)
        Me.OnDrawing(e, g, Me.Center, New RectangleF(Me.Position.X - Me.Radius, Me.Position.Y - Me.Radius, Me.Radius, Me.Radius))
        Me.DrawMessage(e, g)
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0} [{1}/{2}]", Me.Name, Me.Position, Me.Neighbors.Count)
    End Function

#Region "Core Mechanics"
    Private Sub ScanNeighbors(e As Engine, n As Particle)
        SyncLock Me.Neighbors
            Me.Neighbors.Clear()
            For Each other As Particle In e.GetQuadrant(Me)
                If (other.IsPinned) Then Continue For
                Dim distance As Single = Vector2.Distance(Me.Position, other.Position)
                If (distance <= Me.Sensor) Then
                    Me.Neighbors.Add(other)
                End If
            Next
        End SyncLock
    End Sub

    Private Sub HandleNeighbors(e As Engine, dt As Single)
        If (Me.Neighbors.Count = 0) Then Return
        Dim rtest As Vector2 = Vector2.Null
        For Each other As Particle In Me.Neighbors
            Me.HandleCollision(e, other, dt)
        Next
    End Sub

    Private Sub HandleCollision(e As Engine, other As Particle, dt As Single)
        If (Me.Neighbors.Count = 0) Then Return
        Dim react As Boolean = True
        Dim contact As Vector2 = Me.Position - other.Position
        Dim distance As Single = contact.Length
        Dim normal As Vector2 = contact / distance
        Static padding As Single = Me.Radius * 0.2
        Me.OnInteraction(e, other, distance, react, dt)
        If (distance <= Me.Radius + padding) Then
            Dim relativeVelocity As Vector2 = Me.Velocity - other.Velocity
            Dim speedAlongNormal As Single = Vector2.Dot(relativeVelocity, normal)
            If speedAlongNormal > 0F Then Exit Sub
            Dim restitution As Single = (Me.Restitution + other.Restitution) * 0.5F
            Dim impulseScalar As Single = -(1 + restitution) * speedAlongNormal / (1 / Me.Mass + 1 / other.Mass)
            Dim impulse As Vector2 = impulseScalar * normal
            Me.OnCollision(e, other, contact, impulse, dt)
            If (react) Then
                Me.Velocity += impulse / Me.Mass
                Me.Velocity += Me.Velocity / Me.Mass
            End If
        End If
    End Sub

    Private Sub UpdateVelocity(dt As Single)
        Me.Position += Me.Velocity * dt
    End Sub

    Private Sub HandleFriction(ByRef factor As Single, dt As Single)
        If (factor > 0F) Then
            Me.Velocity += (-factor * Me.Velocity.Magnitude * Me.Velocity) * dt
        End If
    End Sub

    Private Sub HandleSpeedLimit(ByRef v As Vector2, max As Single)
        Dim result As Vector2 = v
        Dim magnitude As Single = v.Magnitude
        If magnitude > max Then
            result = Vector2.Normalize(v) * max
        End If
        v = result
    End Sub

    Private Sub KeepBounds(e As Engine, damping As Single)
        Dim padding As Double = Me.Radius + 1.0F
        If Me.Position.X < e.Bounds.Left + padding Then
            Dim mirrorPos As Vector2 = New Vector2(e.Bounds.Left + padding, Me.Position.Y)
            Dim awayVec As Vector2 = Me.Position - mirrorPos
            Me.Velocity = Me.Velocity.Reflect(awayVec) * damping
            Me.Position.X = e.Bounds.Left + padding
        End If
        If Me.Position.X > e.Bounds.Right - padding Then
            Dim mirrorPos As Vector2 = New Vector2(e.Bounds.Right - padding, Me.Position.Y)
            Dim awayVec As Vector2 = Me.Position - mirrorPos
            Me.Velocity = Me.Velocity.Reflect(awayVec) * damping
            Me.Position.X = e.Bounds.Right - padding
        End If
        If Me.Position.Y < e.Bounds.Top + padding Then
            Dim mirrorPos As Vector2 = New Vector2(Me.Position.X, e.Bounds.Top + padding)
            Dim awayVec As Vector2 = Me.Position - mirrorPos
            Me.Velocity = Me.Velocity.Reflect(awayVec) * damping
            Me.Position.Y = e.Bounds.Top + padding
        End If
        If Me.Position.Y > e.Bounds.Bottom - padding Then
            Dim mirrorPos As Vector2 = New Vector2(Me.Position.X, e.Bounds.Bottom - padding)
            Dim awayVec As Vector2 = Me.Position - mirrorPos
            Me.Velocity = Me.Velocity.Reflect(awayVec) * damping
            Me.Position.Y = e.Bounds.Bottom - padding
        End If
    End Sub

#End Region
#Region "Formation"


#End Region
#Region "Control"
    Public Function Signal(freq As Single, ampl As Single, dt As Single) As Vector2
        Dim period As Single = 1 / freq
        Dim time As Single = dt Mod period
        Dim value As Single = ((time / period) * 2) - 1
        Dim w As Single = ampl * value
        Dim dx As Single = w * Math.Sin(Me.Position.X * freq)
        Dim dy As Single = w * Math.Sin(Me.Position.Y * freq)
        Return New Vector2(dx, dy)
    End Function

    Public Sub Steer(target As Vector2, maxSteer As Single)
        Dim desired As Vector2 = target - Me.Position
        If desired.Magnitude > 0 Then
            desired.Normalize()
            desired *= Me.Max
            Dim heading As Vector2 = desired - Me.Velocity
            If heading.Magnitude > maxSteer Then
                heading.Normalize()
                heading *= maxSteer
            End If
            Me.Velocity += heading
        End If
    End Sub

    Public Function GetGroupCenter() As Vector2
        If (Me.Neighbors.Count = 0) Then Return Me.Center
        Dim center As Vector2 = Vector2.Null
        For Each neighbor In Me.Neighbors
            center += neighbor.Position
        Next
        center /= Neighbors.Count
        Return center
    End Function

    Public Function GetGroupDirection() As Vector2
        If (Me.Neighbors.Count = 0) Then Return Me.Center
        Dim average As Vector2 = Vector2.Null
        For Each neighbor In Me.Neighbors
            average += neighbor.Velocity
        Next
        average /= Neighbors.Count
        Return average
    End Function

    Public Function Alignment(weight As Single) As Vector2
        If (Me.Neighbors.Count = 0) Then Return Vector2.Null
        Dim average As Vector2 = Vector2.Null
        For Each neighbor In Me.Neighbors
            average += neighbor.Velocity
        Next
        average /= Neighbors.Count
        Return (average - Me.Velocity) * weight
    End Function

    Public Function Cohesion(weight As Single) As Vector2
        If (Me.Neighbors.Count = 0) Then Return Vector2.Null
        Dim center As Vector2 = Vector2.Null
        For Each neighbor In Me.Neighbors
            center += neighbor.Position
        Next
        center /= Neighbors.Count
        Return (center - Me.Position) * weight
    End Function

    Public Function Separation(weight As Single, distance As Single) As Vector2
        If (Me.Neighbors.Count = 0) Then Return Vector2.Null
        Dim repulsion As Vector2 = Vector2.Null
        For Each neighbor In Me.Neighbors
            Dim d As Single = Vector2.Distance(Me.Position, neighbor.Position)
            If d < distance Then
                repulsion -= (neighbor.Position - Me.Position) / d
            End If
        Next
        Return repulsion * weight
    End Function

    Public Sub Pursue(target As Particle, m1 As Single, Optional m2 As Single = 1.0)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = ((Me.Sensor + target.Sensor) * m2) - d
        Dim netforce As Single = overlap * m1
        Dim predicted As Vector2 = target.Position + target.Velocity * (d / Me.Max)
        Dim offset As Vector2 = predicted - Me.Position
        If (d > 0) Then r = (Vector2.Normalize(e) * Me.Max) - Me.Velocity
        Me.Velocity += Vector2.Normalize(offset) * netforce
    End Sub

    Public Sub Evade(target As Particle, m1 As Single, Optional m2 As Single = 1.0F)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim predicted As Vector2 = (target.Position + target.Velocity * (d / Me.Max)) - Me.Position
        Dim overlap As Single = ((Me.Sensor + target.Sensor) * m2) - d
        Dim netforce As Single = overlap * m1
        If (d > 0) Then r = (Vector2.Normalize(e) * Me.Max) - Me.Velocity
        Me.Velocity -= r * netforce
    End Sub

    Public Sub Arrival(target As Particle, m1 As Single, Optional m2 As Single = 1.0F)
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        If (d > 0) Then
            e = Vector2.Normalize(e)
            If (d < Me.Sensor) Then
                e *= Me.Max * (d / (Me.Sensor * m2))
            Else
                e *= Me.Max
            End If
            Me.Velocity += (e - Me.Velocity) * m1
        End If
    End Sub

    Public Sub Departure(target As Particle, m1 As Single, Optional m2 As Single = 1.0F)
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        If (d > 0) Then
            e = Vector2.Normalize(e)
            If (d < Me.Sensor) Then
                e *= Me.Max * (d / (Me.Sensor * m2))
            Else
                e *= Me.Max
            End If
            Me.Velocity -= (e - Me.Velocity) * m1
        End If
    End Sub

    Public Sub Seek(target As Particle, m1 As Single, Optional m2 As Single = 1.0F)
        Dim v As Vector2 = Vector2.Null
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Center - Me.Center
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = (Me.Sensor * m2) - d
        Dim netforce As Single = overlap * m1
        If (d > 0) Then
            e = Vector2.Normalize(e) * Me.Max
            r = e - Me.Velocity
        End If
        Me.Velocity += r * netforce
    End Sub

    Public Sub Flee(target As Particle, m1 As Single, Optional m2 As Single = 1.0F)
        Dim r As Vector2 = Vector2.Null
        Dim e As Vector2 = target.Position - Me.Position
        Dim d As Single = e.Magnitude
        Dim displacement As Vector2 = Vector2.Normalize(Me.Center - target.Center)
        Dim overlap As Single = (Me.Sensor * m2) - d
        Dim netforce As Single = overlap * m1
        If (d > 0) Then
            e = Vector2.Normalize(e) * Me.Max
            r = e - Me.Velocity
        End If
        Me.Velocity -= r * netforce
    End Sub

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
#Region "Message Box"
    Public Sub SetMessage(duration As Single, message As String, ParamArray format As String())
        Using bm As New Bitmap(1, 1)
            Using g As Graphics = Graphics.FromImage(bm)
                Me.Label = String.Format(message, format)
                Me.LabelTimeout = DateTime.Now.Add(TimeSpan.FromMilliseconds(duration))
                Me.LabelSize = g.MeasureString(Me.Label, Me.LabelFont)
            End Using
        End Using
    End Sub

    Private Sub DrawMessage(e As Engine, g As Graphics)
        If (Me.LabelTimeout > DateTime.Now) Then
            g.DrawString(Me.Label, e.Font, Me.LabelBrush, Me.Center.X - Me.LabelSize.Width / 2, Me.Center.Y - Me.LabelSize.Height / 2 - 30)
        End If
    End Sub
#End Region

#Region "Poperties"
    Public ReadOnly Property HasNeighbor(p As Particle) As Boolean
        Get
            If (Me.Neighbors.Count = 0) Then Return False
            Return Me.Neighbors.Contains(p)
        End Get
    End Property

    Public ReadOnly Property Center As Vector2
        Get
            Dim rhalf As Single = Me.Radius / 2
            Return New Vector2(Me.Position.X - rhalf, Me.Position.Y - rhalf)
        End Get
    End Property

    Public ReadOnly Property Heading() As Vector2
        Get
            Return Vector2.Normalize(Me.Velocity)
        End Get
    End Property

    Public ReadOnly Property Heading(len As Single) As Vector2
        Get
            Dim vnow As Vector2 = Me.Velocity
            If vnow.Length = 0 Then
                Return Vector2.Null
            Else
                Return Vector2.Normalize(vnow) * len
            End If
        End Get
    End Property

    Public ReadOnly Property GetTint() As Color
        Get
            Dim value As Single = Math.Abs(Me.Charge)
            Return Color.FromArgb((255 * value), 0, 0)
        End Get
    End Property

    Public Overridable ReadOnly Property Name As String
        Get
            Return "Particle"
        End Get
    End Property
#End Region

End Class
