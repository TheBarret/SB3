Imports SB3.Bonding

Public Class Engine
    Public Property Rows As Integer
    Public Property Columns As Integer
    Public Property Margin As Integer
    Public Property Lock As Object
    Public Property Font As Font
    Public Property Bounds As Rectangle
    Public Property Nodes As List(Of Node)
    Public Property Bonds As List(Of Bond)
    Public Property PInput As Queue(Of Particle)
    Public Property DOuput As Queue(Of Particle)
    Public Property Particles As List(Of Particle)

    Sub New(bounds As Rectangle, margin As Integer, width As Integer, height As Integer)
        Me.Lock = New Object
        Me.Bounds = bounds
        Me.Bonds = New List(Of Bond)
        Me.Font = New Font("Consolas", 8)
        Me.PInput = New Queue(Of Particle)
        Me.DOuput = New Queue(Of Particle)
        Me.Particles = New List(Of Particle)
        Me.CreateNodes(margin, width, height)
        Me.Randomize()
    End Sub

    Public Sub CreateNodes(margin As Integer, columns As Integer, rows As Integer)
        Me.Margin = margin
        Me.Rows = rows
        Me.Columns = columns
        Me.Nodes = Me.Create
    End Sub

    Public Sub Enqueue(p As Particle)
        Me.PInput.Enqueue(p)
    End Sub

    Public Sub Dequeue(p As Particle)
        Me.DOuput.Enqueue(p)
    End Sub

    Public Sub Randomize()
        Static cap As Single = Engine.RFloat(5, 100)
        SyncLock Me.Particles
            For i As Integer = 1 To 50
                Dim x As Single = Engine.Randomizer.Next(100, Me.Bounds.Width - 50)
                Dim y As Single = Engine.Randomizer.Next(100, Me.Bounds.Height - 50)
                Me.Particles.Add(New Boid(New Vector2(x, y)))
                Me.Particles.Last.AddForce(New Vector2(Engine.Randomizer.Next(-cap, cap), Engine.Randomizer.Next(-cap, cap)))
            Next
        End SyncLock
    End Sub

    Public Function GetByPosition(x As Single, y As Single, ByRef result As Node) As Boolean
        For Each n As Node In Me.Nodes
            If (n.Rectangle.Contains(x, y)) Then
                result = n
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub Update(dt As Single)
        Me.UpdateQueue()
        Me.UpdateParticles(dt)
    End Sub

    Public Sub Draw(g As Graphics)
        Dim buffer As List(Of Particle) = Me.Particles
        For Each b As Bond In Me.Bonds
            b.Draw(g, b.Top.GetTint)
        Next
        For Each b As Particle In buffer
            b.Draw(Me, g)
        Next
    End Sub

    Private Sub UpdateParticles(dt As Single)
        Dim buffer As List(Of Particle) = Me.Particles
        For Each b As Particle In buffer
            b.Update(Me, dt)
        Next
        For Each b As Bond In Me.Bonds
            b.Update(dt)
        Next
    End Sub

    Private Sub UpdateQueue()
        If (Me.PInput.Any) Then
            Do
                SyncLock Me.Particles
                    Me.Particles.Add(Me.PInput.Dequeue)
                End SyncLock
            Loop While Me.PInput.Any
        End If
        If (Me.DOuput.Any) Then
            Do
                SyncLock Me.Particles
                    Me.Particles.Remove(Me.DOuput.Dequeue)
                End SyncLock
            Loop While Me.DOuput.Any
        End If
    End Sub

    Private Function Create() As List(Of Node)
        SyncLock Me.Particles
            Dim counter As Integer = 0
            Dim nodes As New List(Of Node)
            Dim width As Integer = (Me.Bounds.Width - (2 * Me.Margin)) \ Me.Columns
            Dim height As Integer = (Me.Bounds.Height - (2 * Me.Margin)) \ Me.Rows
            For r As Integer = 0 To Me.Rows - 1
                For c As Integer = 0 To Me.Columns - 1
                    Dim x As Integer = Me.Margin + c * width
                    Dim y As Integer = Me.Margin + r * height
                    nodes.Add(New Node(counter, r, c, x, y, width, height))
                    counter += 1
                Next
            Next
            Return nodes
        End SyncLock
    End Function

    Public Function GetLeader(p As Particle) As Particle
        Dim group = Me.Bonds.
                            Where(Function(x) x.Top Is p Or x.Bottom Is p).
                            Select(Function(y) y.GetByWeight).
                            OrderBy(Function(z) z.Mass).
                            Where(Function(i) i IsNot p).
                            Distinct()
        If (group.Any) Then Return group.Last Else Return p
    End Function

    Public ReadOnly Property HasBonded(p As Particle) As Boolean
        Get
            Return Me.Bonds.Any(Function(x) x.Top Is p Or x.Bottom Is p)
        End Get
    End Property

    Public Shared Function RRange(min As Single, max As Single) As Single
        Return min + CSng(Engine.Randomizer.NextDouble) * (max - min)
    End Function

    Public Shared Function RFloat(minValue As Single, maxValue As Single) As Single
        Return CSng((maxValue - minValue) * Engine.Randomizer.NextDouble) + minValue
    End Function

    Public Shared ReadOnly Property Randomizer As Random
        Get
            Static r As New Random(DateTime.Now.Millisecond)
            Return r
        End Get
    End Property

End Class
