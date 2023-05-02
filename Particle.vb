Imports System.Drawing.Drawing2D

Public MustInherit Class Particle
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
    Public Property IsPinned As Boolean
    Public Property Neighbors As HashSet(Of Particle)
    Public MustOverride Sub OnUpdate(e As Engine, dt As Single)
    Public MustOverride Sub OnDrawing(e As Engine, g As Graphics, srcRect As RectangleF)
    Public MustOverride Sub OnContact(e As Engine, other As Particle, contact As Vector2, force As Vector2, dt As Single)
    Public MustOverride Sub OnInteraction(e As Engine, other As Particle, distance As Single, ByRef react As Boolean, dt As Single)

    Sub New(position As Vector2, Optional isPinned As Boolean = False)
        Me.Mass = 1.0F
        Me.Max = 100.0F
        Me.Radius = 15.0F
        Me.Friction = 0.003F
        Me.Restitution = 0.5F
        Me.Position = position
        Me.IsPinned = isPinned
        Me.Neighbors = New HashSet(Of Particle)
        Me.Reset()
    End Sub

    Public Sub Reset()
        Me.Elapsed = 0F
        Me.Label = String.Empty
        Me.LabelTimeout = Date.Now
        Me.LabelFont = New Font("Consolas", 8)
        Me.LabelBrush = New SolidBrush(Color.Black)
        Me.Velocity = Vector2.One
        Me.Velocity = Vector2.Null
        Me.Mass = Engine.RFloat(Vector2.Epsilon, 1.0F)
        SyncLock Me.Neighbors
            Me.Neighbors.Clear()
        End SyncLock
    End Sub

    Public Sub AddForce(force As Vector2)
        Me.Velocity += force / Me.Mass
    End Sub

    Public Sub Update(e As Engine, dt As Single)
        ' Return when we are pinned
        If (Me.IsPinned) Then Return
        ' update time accumulation
        Me.Elapsed = +dt
        ' Calculate sensor range
        Dim length As Single = Me.Velocity.Length
        Dim range As Single = Me.Radius + Me.Radius + (Me.Neighbors.Count + Math.Min(Me.Sensor, 0.5 * length))
        If (range < Me.Sensor) Then Me.Sensor -= 0.8
        If (range > Me.Sensor) Then Me.Sensor += 0.8
        ' Start with sensor range
        If (length > 0F) Then
            ' Scan neighborhood within sensor range
            SyncLock Me.Neighbors
                Me.Neighbors.Clear()
                For Each other As Particle In e.Particles
                    If (other Is Me) Then Continue For
                    If (other.IsPinned) Then Continue For
                    Dim distance As Single = Vector2.Distance(Me.Position, other.Position)
                    If (distance <= Me.Sensor) Then
                        Me.Neighbors.Add(other)
                    End If
                Next
            End SyncLock
            ' Handle Collision pool
            For Each other As Particle In Me.Neighbors
                Me.HandleCollision(e, other, dt)
            Next
            ' Update top level
            Me.OnUpdate(e, dt)
            ' Process velocity vectors
            Me.HandleFriction(Me.Friction, dt)
            Me.HandleSpeedLimit(Me.Velocity, Me.Max)
            Me.UpdateVelocity(dt)
            Me.KeepBounds(e)
        End If
    End Sub

    Public Sub Draw(e As Engine, g As Graphics)
        Me.OnDrawing(e, g, New RectangleF(Me.Position.X - Me.Radius, Me.Position.Y - Me.Radius, Me.Radius, Me.Radius))
        Me.DrawMessage(e, g)
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0} [{1}]", Me.Name, Me.Position)
    End Function

#Region "Mechanics"
    Private Sub HandleCollision(e As Engine, other As Particle, dt As Single)
        If (Me.Neighbors.Any) Then
            ' Reaction state
            Dim react As Boolean = True
            ' Describes and calculate contact point where the collision occurs.
            Dim contact As Vector2 = Me.Position - other.Position
            Dim distance As Single = contact.Length
            Dim normal As Vector2 = contact / distance
            Static padding As Single = Me.Radius * 0.1
            If (distance <= Me.Radius + padding) Then
                ' Calculate the impulse vector applied to this contact pair to resolve the collision.
                ' (To work out the total force applied you can divide the total impulse by the last frame's fixedDeltaTime)
                Dim relativeVelocity As Vector2 = Me.Velocity - other.Velocity
                Dim speedAlongNormal As Single = Vector2.Dot(relativeVelocity, normal)
                ' The particle are moving apart or already colliding
                If speedAlongNormal > 0F Then Exit Sub
                Dim restitution As Single = (Me.Restitution + other.Restitution) * 0.5F
                Dim impulseScalar As Single = -(1 + restitution) * speedAlongNormal / (1 / Me.Mass + 1 / other.Mass)
                Dim impulse As Vector2 = impulseScalar * normal
                ' Call top layer
                Me.OnContact(e, other, contact, impulse, dt)
                ' React to impulse
                If (react) Then
                    'Compute how fast Each one was moving along the collision normal, or zero if normal.
                    Dim incidentVelocitySelf As Vector2 = Me.Velocity - impulse / Me.Mass
                    Dim incidentVelocityOther As Vector2 = other.Velocity + impulse / other.Mass
                    Dim approachSelf As Single = Vector2.Dot(incidentVelocitySelf, normal)
                    Dim approachOther As Single = Vector2.Dot(incidentVelocityOther, normal)
                    ' Calculate damage and raise event to delegate the process for down the chain
                    Dim incidentForce As Single = Math.Max(0F, approachOther - approachSelf - 0.1F)
                    Me.Velocity += impulse / Me.Mass
                    Me.Velocity += Me.Velocity / Me.Mass
                End If
                Return
            End If
            ' Call top layer
            Me.OnInteraction(e, other, distance, react, dt)
        End If
    End Sub

    Private Sub UpdateVelocity(dt As Single)
        If (Single.IsNaN(Me.Velocity.X) Or Single.IsNaN(Me.Velocity.Y)) Then
            Me.Velocity.X = 0F
            Me.Velocity.Y = 0F
        ElseIf (Single.IsInfinity(Me.Velocity.X) Or Single.IsInfinity(Me.Velocity.Y)) Then
            Me.Velocity.X = 0F
            Me.Velocity.Y = 0F
        End If
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

    Private Sub KeepBounds(e As Engine, Optional brake As Single = -1.0F)
        Dim padding As Double = Me.Radius + 10.0F
        If Me.Position.X < e.Bounds.Left + padding Then
            Me.Position.X = padding
            Me.Velocity.X *= brake
        End If
        If Me.Position.X > e.Bounds.Right - padding Then
            Me.Position.X = e.Bounds.Right - padding
            Me.Velocity.X *= brake
        End If
        If Me.Position.Y < e.Bounds.Top + padding Then
            Me.Position.Y = padding
            Me.Velocity.Y *= brake
        End If
        If Me.Position.Y > e.Bounds.Bottom - padding Then
            Me.Position.Y = e.Bounds.Bottom - padding
            Me.Velocity.Y *= brake
        End If
    End Sub
#End Region
#Region "Control"
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
#End Region
#Region "Message Box"
    Public Sub ShowLabel(duration As Single, message As String, ParamArray format As String())
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
            Return New Vector2(Me.Position.X - (Me.Radius / 2), Me.Position.Y - (Me.Radius / 2))
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
            Dim value As Single = Math.Max(0, Math.Min(1.0F, Me.Mass))
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
