Public Class Ray
    Public Property Length As Single
    Public Property Center As Vector2
    Public Property Sensor As Vector2

    Sub New(center As Vector2, endpoint As Vector2, length As Single)
        Me.Center = center
        Me.Sensor = endpoint
        Me.Length = length
    End Sub

End Class
