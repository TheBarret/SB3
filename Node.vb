Public Class Node
    Public Property Tint As Color
    Public Property Line As Pen
    Public Property Brush As Brush
    Public Property Row As Integer
    Public Property Index As Integer
    Public Property Column As Integer
    Public Property Children As List(Of Node)
    Public Property Parent As Node
    Public Property Capacity As Integer
    Public Property Rectangle As Rectangle
    Public Property Alignment As StringFormat

    Sub New(index As Integer, row As Integer, column As Integer, x As Integer, y As Integer, width As Integer, height As Integer)
        Me.Index = index
        Me.Row = row
        Me.Column = column
        Me.Children = New List(Of Node)
        Me.Tint = Color.FromArgb(0, 102, 0)
        Me.Rectangle = New Rectangle(x, y, width, height)
        Me.Brush = New SolidBrush(Me.Tint)
        Me.Line = New Pen(Me.Tint, 1)
        Me.Alignment = New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
    End Sub

    Public Sub Draw(e As Engine, g As Graphics)
        g.DrawString(Me.Sector, e.Font, Me.Brush, Me.Rectangle, Me.Alignment)
    End Sub

    Public ReadOnly Property Sector As String
        Get
            Return String.Format("{0:#000}", Me.Index)
        End Get
    End Property

    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Me.Children Is Nothing
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.Sector
    End Function
End Class
