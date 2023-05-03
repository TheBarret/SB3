Public Class Ray
    Public Property Length As Single
    Public Property Particle As Particle
    Public Property Endpoint As Vector2

    Sub New(p As Particle, endpoint As Vector2, length As Single)
        Me.Particle = p
        Me.Length = length
        Me.Endpoint = endpoint
    End Sub
End Class
