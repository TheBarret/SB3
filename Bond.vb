Namespace Bonding
    Public Class Bond
        Public Property Top As Particle
        Public Property Bottom As Particle
        Public Property Length As Single
        Public Property Visible As Boolean
        Public Property Stiffness As Single

        Sub New(top As Particle, bottom As Particle, length As Single, stiffness As Single)
            Me.Visible = True
            Me.Top = top
            Me.Bottom = bottom
            Me.Stiffness = stiffness
            Me.Length = length
        End Sub

        Public Sub Update(dt As Single)
            Dim top As Particle = Me.Top
            Dim bottom As Particle = Me.Bottom
            Dim displacement As Single = Me.Displacement
            Dim dir As Vector2 = Vector2.Normalize(bottom.Position - top.Position)
            Dim force As Vector2 = dir * Me.Stiffness * displacement
            top.Velocity += force / top.Mass * dt
            bottom.Velocity -= force / bottom.Mass * dt
        End Sub

        Public ReadOnly Property Distance As Single
            Get
                Return Vector2.Distance(Me.Top.Position, Me.Bottom.Position)
            End Get
        End Property

        Public ReadOnly Property Displacement As Single
            Get
                Return (Me.Distance - Me.Length)
            End Get
        End Property

        Public ReadOnly Property Breakpoint(limit As Single) As Boolean
            Get
                Return Me.Distance > Me.Length * limit Or
                   Me.Distance < Me.Length / limit
            End Get
        End Property

    End Class
End Namespace