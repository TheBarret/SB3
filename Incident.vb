Public Class Incident
    Public Property Victim As Particle
    Public Property Assailant As Particle
    Public Property Impact As Vector2

    Sub New(victim As Particle, assailant As Particle, impact As Vector2)
        Me.Victim = victim
        Me.Assailant = assailant
        Me.Impact = impact
    End Sub
End Class
