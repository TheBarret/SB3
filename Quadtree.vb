Public Class Quadtree
    Public Const MAXP As Integer = 4
    Public Const MAXD As Integer = 2
    Public Property Lock As Object
    Public Property Depth As Integer
    Public Property Bounds As RectangleF
    Public Property Regions As List(Of Quadtree)
    Public Property Cached As List(Of Particle)

    Sub New(depth As Integer, bound As Rectangle)
        Me.Depth = depth
        Me.Bounds = bound
        Me.Lock = New Object
        Me.Regions = New List(Of Quadtree)
        Me.Cached = New List(Of Particle)
    End Sub

    Sub New(depth As Integer, bound As RectangleF)
        Me.Depth = depth
        Me.Bounds = bound
        Me.Lock = New Object
        Me.Regions = New List(Of Quadtree)
        Me.Cached = New List(Of Particle)
    End Sub

    Public Sub Draw(g As Graphics)
        For Each n As Quadtree In Me.Regions
            n.Draw(g)
        Next
        Using p As New Pen(Color.LightGray, 1) With {.DashStyle = Drawing2D.DashStyle.Dot}
            g.DrawRectangle(p, Me.Bounds.X, Me.Bounds.Y, Me.Bounds.Width, Me.Bounds.Height)
        End Using
    End Sub

    Public Function GetEntities(a As Particle) As HashSet(Of Particle)
        Return Me.GetEntities(a, New HashSet(Of Particle))
    End Function

    Public Function GetEntities(a As Particle, result As HashSet(Of Particle)) As HashSet(Of Particle)
        Dim index As Integer = Me.IndexOf(a)
        If (result Is Nothing) Then result = New HashSet(Of Particle)
        If (index <> -1 AndAlso index < Me.Regions.Count) Then
            Me.Regions(index).GetEntities(a, result)
        End If
        If (Me.Cached.Count > 0) Then
            result.UnionWith(Me.Cached)
            result.Remove(a)
        End If
        Return result
    End Function

    Public Function GetQuadrant(a As Particle, result As List(Of Quadtree)) As Quadtree
        Dim index As Integer = Me.IndexOf(a)
        If (result Is Nothing) Then result = New List(Of Quadtree)
        If (index <> -1 AndAlso index < Me.Regions.Count) Then
            Me.Regions(index).GetQuadrant(a, result)
        End If
        If (Me.Regions.Count > 0) Then
            result.Add(Me.Regions.OrderBy(Function(x) x.Depth).Last)
        End If
        Return result.OrderBy(Function(x) x.Depth).Last
    End Function

    Public Sub Prerender(entities As IEnumerable(Of Particle))
        For Each p As Particle In entities
            Me.Cache(p)
        Next
    End Sub

    Public Sub Cache(p As Particle)
        If (Me.Regions.Count > 0) Then
            Dim index As Integer = Me.IndexOf(p)
            If (index <> -1) Then
                If (index < Me.Regions.Count) Then
                    Me.Regions(index).Cache(p)
                    Return
                End If
            End If
        End If
        Me.Cached.Add(p)
        If Me.Cached.Count > Quadtree.MAXP AndAlso Me.Depth < Quadtree.MAXD Then
            If Me.Regions.Count = 0 Then Me.Divide()
            Dim i As Integer = 0
            While i < Me.Cached.Count
                Dim index As Integer = Me.IndexOf(Me.Cached(i))
                If index <> -1 Then
                    Dim p2 As Particle = Me.Cached(i)
                    If (Me.Cached.Remove(p2)) Then
                        Me.Regions(index).Cache(p2)
                    End If
                Else
                    i += 1
                End If
            End While
        End If
    End Sub

    Public Function IndexOf(p As Particle) As Integer
        Dim index As Integer = -1
        Dim vMpoint As Double = Me.Bounds.X + (Me.Bounds.Width / 2)
        Dim hMpoint As Double = Me.Bounds.Y + (Me.Bounds.Height / 2)
        Dim tQuadrant As Boolean = (p.Position.Y < hMpoint And p.Position.Y + p.Radius < hMpoint)
        Dim bQuadrant As Boolean = (p.Position.Y > hMpoint)

        If (p.Position.X < vMpoint And p.Position.X + p.Radius < vMpoint) Then
            If (tQuadrant) Then
                index = 1
            ElseIf (bQuadrant) Then
                index = 2
            End If
        ElseIf (p.Position.X > vMpoint) Then
            If (tQuadrant) Then
                index = 0
            ElseIf (bQuadrant) Then
                index = 3
            End If
        End If
        Return index
    End Function

    Public Sub Divide()
        SyncLock Me.Lock
            Dim width As Single = CSng(Me.Bounds.Width / 2)
            Dim height As Single = CSng(Me.Bounds.Height / 2)
            Dim x As Single = Me.Bounds.X
            Dim y As Single = Me.Bounds.Y
            Me.Regions.Insert(0, New Quadtree(Me.Depth + 1, New RectangleF(x + width, y, width, height)))
            Me.Regions.Insert(1, New Quadtree(Me.Depth + 1, New RectangleF(x, y, width, height)))
            Me.Regions.Insert(2, New Quadtree(Me.Depth + 1, New RectangleF(x, y + height, width, height)))
            Me.Regions.Insert(3, New Quadtree(Me.Depth + 1, New RectangleF(x + width, y + height, width, height)))
        End SyncLock
    End Sub

    Public Sub Cleanup()
        SyncLock Me.Lock
            Me.Cached.Clear()
            For i As Integer = 0 To Me.Regions.Count - 1
                Me.Regions(i).Cleanup()
            Next
            Me.Regions.Clear()
        End SyncLock
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0} Cached | {1} Regions | Depth {2}", Me.Cached.Count, Me.RegionsT, Me.DepthT)
    End Function

    Public ReadOnly Property RegionsT As Integer
        Get
            Dim count As Integer = Me.Regions.Count
            For Each r As Quadtree In Me.Regions
                count += r.RegionsT
            Next
            Return count
        End Get
    End Property

    Public ReadOnly Property DepthT As Integer
        Get
            Dim max As Integer = 0
            For Each n As Quadtree In Me.Regions
                max += n.Depth
            Next
            Return max + 1
        End Get
    End Property

End Class
