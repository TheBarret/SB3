Imports System.Text.RegularExpressions

Public Class Vector2
    Public Const Epsilon As Single = 0.001F
    Public Const Radians As Single = 57.2957795
    Public Property X As Single
    Public Property Y As Single

    Sub New(x As Single, y As Single)
        Me.X = x
        Me.Y = y
    End Sub

#Region "Functions"
    Public Function Magnitude() As Double
        Return Me.Length
    End Function

    Public Function Length() As Single
        Return CSng(Math.Sqrt(Me.X * Me.X + Me.Y * Me.Y))
    End Function

    Public Sub Normalize()
        Dim length As Single = Me.Length
        If (length <> 0.0F) Then
            Me.X /= length
            Me.Y /= length
        End If
    End Sub

    Public Function Rotate(radians As Single) As Vector2
        Dim cosAngle As Double = Math.Cos(radians)
        Dim sinAngle As Double = Math.Sin(radians)
        Return New Vector2(cosAngle * Me.X - sinAngle * Me.Y, sinAngle * Me.X + cosAngle * Me.Y)
    End Function

    Public Function Reflect(norm As Vector2) As Vector2
        Return Me - 2 * Vector2.Dot(Me, norm) * norm
    End Function

    Public Function Angle() As Single
        Dim value As Single = If(Me.X = 0, 90.0F, CSng(Math.Atan(Me.Y / Me.X) * Vector2.Radians))
        If Me.X < 0.0F Then value += 180.0F
        Return value
    End Function

    Public Function Angle(other As Vector2) As Single
        Dim offset As Vector2 = Me - other
        Return Math.Atan2(offset.Y, offset.X)
    End Function

    Public Shared Function ClampMagnitude(vector As Vector2, len As Single) As Vector2
        Dim sqrMagnitude As Single = vector.Length
        If sqrMagnitude > len * len Then
            Dim magnitude As Single = Math.Sqrt(sqrMagnitude)
            Dim normalizedVector As Vector2 = vector / magnitude
            Return normalizedVector * len
        Else
            Return vector
        End If
    End Function

    Public Shared Function Clamp(value As Vector2, min As Single, max As Single) As Vector2
        Dim x As Single = Vector2.Clamp(value.X, min, max)
        Dim y As Single = Vector2.Clamp(value.Y, min, max)
        Return New Vector2(x, y)
    End Function

    Public Shared Function Cross(v1 As Vector2, v2 As Vector2) As Single
        Return v1.X * v2.Y - v2.X * v1.Y
    End Function

    Public Shared Function Dot(a As Vector2, b As Vector2) As Single
        Return a.X * b.X + a.Y * b.Y
    End Function

    Public Shared Function Null() As Vector2
        Return New Vector2(0, 0)
    End Function

    Public Shared Function One() As Vector2
        Return New Vector2(1, 1)
    End Function

    Public Shared Function RotatedBy(vector As Vector2, radians As Single) As Vector2
        Return New Vector2(
        vector.X * Math.Cos(radians) - vector.Y * Math.Sin(radians),
        vector.X * Math.Sin(radians) + vector.Y * Math.Cos(radians))
    End Function

    Public Shared Function Clamp(v As Single, min As Single, max As Single) As Single
        Return Math.Max(min, Math.Min(max, v))
    End Function

    Public Shared Function Distance(a As Vector2, b As Vector2) As Single
        Dim dx As Single = a.X - b.X
        Dim dy As Single = a.Y - b.Y
        Return CSng(Math.Sqrt(dx * dx + dy * dy))
    End Function

    Public Shared Function Normalize(value As Vector2) As Vector2
        Dim ls As Single = value.X * value.X + value.Y * value.Y
        Dim invNorm As Single = 1.0F / CSng(Math.Sqrt(CDbl(ls)))
        Return New Vector2(value.X * invNorm, value.Y * invNorm)
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("{0}", Math.Round(Me.Length, 2))
    End Function
#End Region

#Region "Properties"
    Public Property Width As Single
        Get
            Return Me.X
        End Get
        Set(value As Single)
            Me.X = value
        End Set
    End Property

    Public Property Height As Single
        Get
            Return Me.Y
        End Get
        Set(value As Single)
            Me.Y = value
        End Set
    End Property

    Public ReadOnly Property IsNull As Boolean
        Get
            Return Me.X = 0 And Me.Y = 0
        End Get
    End Property
    Public ReadOnly Property IsOne As Boolean
        Get
            Return Me.X = 1 And Me.Y = 1
        End Get
    End Property
    Public ReadOnly Property ToPointF As PointF
        Get
            Return New PointF(Me.X, Me.Y)
        End Get
    End Property
    Public ReadOnly Property ToSizeF As SizeF
        Get
            Return New SizeF(Me.X, Me.Y)
        End Get
    End Property
    Public ReadOnly Property Half As Vector2
        Get
            Return New Vector2(Me.X / 2, Me.Y / 2)
        End Get
    End Property
    Public ReadOnly Property Negative As Vector2

    Public ReadOnly Property Heading() As Vector2
        Get
            Return Vector2.Normalize(Me)
        End Get
    End Property
    Public ReadOnly Property Heading(len As Single) As Vector2
        Get
            Dim vnow As Vector2 = Me
            If vnow.Length = 0 Then
                Return Vector2.Null
            Else
                Return Vector2.Normalize(vnow) * len
            End If
        End Get
    End Property
#End Region

#Region "Operators"

    Public Shared Operator +(v As Vector2, v2 As Vector2) As Vector2
        Return New Vector2(v.X + v2.X, v.Y + v2.Y)
    End Operator
    Public Shared Operator +(v As Vector2, v2 As Single) As Vector2
        Return New Vector2(v.X + v2, v.Y + v2)
    End Operator
    Public Shared Operator +(v As Vector2, v2 As Double) As Vector2
        Return New Vector2(v.X + v2, v.Y + v2)
    End Operator
    Public Shared Operator +(v As Vector2, v2 As Integer) As Vector2
        Return New Vector2(v.X + v2, v.Y + v2)
    End Operator

    Public Shared Operator -(v As Vector2, v2 As Vector2) As Vector2
        Return New Vector2(v.X - v2.X, v.Y - v2.Y)
    End Operator
    Public Shared Operator -(v As Vector2, v2 As Single) As Vector2
        Return New Vector2(v.X - v2, v.Y - v2)
    End Operator
    Public Shared Operator -(v As Vector2, v2 As Double) As Vector2
        Return New Vector2(v.X - v2, v.Y - v2)
    End Operator
    Public Shared Operator -(v As Vector2, v2 As Integer) As Vector2
        Return New Vector2(v.X - v2, v.Y - v2)
    End Operator

    Public Shared Operator *(v As Vector2, v2 As Vector2) As Vector2
        Return New Vector2(v.X * v2.X, v.Y * v2.Y)
    End Operator
    Public Shared Operator *(v As Vector2, v2 As Single) As Vector2
        Return New Vector2(v.X * v2, v.Y * v2)
    End Operator
    Public Shared Operator *(v As Vector2, v2 As Double) As Vector2
        Return New Vector2(v.X * v2, v.Y * v2)
    End Operator
    Public Shared Operator *(v As Vector2, v2 As Integer) As Vector2
        Return New Vector2(v.X * v2, v.Y * v2)
    End Operator
    Public Shared Operator *(v2 As Single, v As Vector2) As Vector2
        Return New Vector2(v2 * v.X, v2 * v.Y)
    End Operator

    Public Shared Operator /(v As Vector2, v2 As Vector2) As Vector2
        Return New Vector2(v.X / v2.X, v.Y / v2.Y)
    End Operator
    Public Shared Operator /(v As Vector2, v2 As Single) As Vector2
        Return New Vector2(v.X / v2, v.Y / v2)
    End Operator
    Public Shared Operator /(v As Vector2, v2 As Double) As Vector2
        Return New Vector2(v.X / v2, v.Y / v2)
    End Operator
    Public Shared Operator /(v As Vector2, v2 As Integer) As Vector2
        Return New Vector2(v.X / v2, v.Y / v2)
    End Operator
    Public Shared Operator /(v2 As Single, v As Vector2) As Vector2
        Return New Vector2(v2 / v.X, v2 / v.Y)
    End Operator
    Public Shared Operator =(v As Vector2, v2 As Vector2) As Boolean
        Return v.X = v2.X And v.Y = v2.Y
    End Operator
    Public Shared Operator =(v As Vector2, v2 As Single) As Boolean
        Return v.X = v2 And v.Y = v2
    End Operator
    Public Shared Operator =(v As Vector2, v2 As Double) As Boolean
        Return v.X = v2 And v.Y = v2
    End Operator
    Public Shared Operator =(v As Vector2, v2 As Integer) As Boolean
        Return v.X = v2 And v.Y = v2
    End Operator

    Public Shared Operator <>(v As Vector2, v2 As Vector2) As Boolean
        Return v.X <> v2.X And v.Y <> v2.Y
    End Operator
    Public Shared Operator <>(v As Vector2, v2 As Single) As Boolean
        Return v.X <> v2 And v.Y <> v2
    End Operator
    Public Shared Operator <>(v As Vector2, v2 As Double) As Boolean
        Return v.X <> v2 And v.Y <> v2
    End Operator
    Public Shared Operator <>(v As Vector2, v2 As Integer) As Boolean
        Return v.X <> v2 And v.Y <> v2
    End Operator

    Public Shared Operator <(v As Vector2, v2 As Vector2) As Boolean
        Return v.X < v2.X And v.Y < v2.Y
    End Operator
    Public Shared Operator <(v As Vector2, v2 As Single) As Boolean
        Return v.X < v2 And v.Y < v2
    End Operator
    Public Shared Operator <(v As Vector2, v2 As Double) As Boolean
        Return v.X < v2 And v.Y < v2
    End Operator
    Public Shared Operator <(v As Vector2, v2 As Integer) As Boolean
        Return v.X < v2 And v.Y < v2
    End Operator

    Public Shared Operator >(v As Vector2, v2 As Vector2) As Boolean
        Return v.X > v2.X And v.Y > v2.Y
    End Operator
    Public Shared Operator >(v As Vector2, v2 As Single) As Boolean
        Return v.X > v2 And v.Y > v2
    End Operator
    Public Shared Operator >(v As Vector2, v2 As Double) As Boolean
        Return v.X > v2 And v.Y > v2
    End Operator
    Public Shared Operator >(v As Vector2, v2 As Integer) As Boolean
        Return v.X > v2 And v.Y > v2
    End Operator

    Public Shared Operator <=(v As Vector2, v2 As Vector2) As Boolean
        Return v.X <= v2.X And v.Y <= v2.Y
    End Operator
    Public Shared Operator <=(v As Vector2, v2 As Single) As Boolean
        Return v.X <= v2 And v.Y <= v2
    End Operator
    Public Shared Operator <=(v As Vector2, v2 As Double) As Boolean
        Return v.X <= v2 And v.Y <= v2
    End Operator
    Public Shared Operator <=(v As Vector2, v2 As Integer) As Boolean
        Return v.X <= v2 And v.Y <= v2
    End Operator

    Public Shared Operator >=(v As Vector2, v2 As Vector2) As Boolean
        Return v.X >= v2.X And v.Y >= v2.Y
    End Operator
    Public Shared Operator >=(v As Vector2, v2 As Single) As Boolean
        Return v.X >= v2 And v.Y >= v2
    End Operator
    Public Shared Operator >=(v As Vector2, v2 As Double) As Boolean
        Return v.X >= v2 And v.Y >= v2
    End Operator
    Public Shared Operator >=(v As Vector2, v2 As Integer) As Boolean
        Return v.X >= v2 And v.Y >= v2
    End Operator
#End Region

End Class
