Public Class Node
    Public Property Rectangle As Rectangle
    Sub New(x As Integer, y As Integer, width As Integer, height As Integer)
        Me.Rectangle = New Rectangle(x, y, width, height)
    End Sub
End Class
