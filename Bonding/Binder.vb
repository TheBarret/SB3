Namespace Bonding
    Public MustInherit Class Binder

        Public Property Bond As Bond

        Public Sub Attach(e As Engine, p As Particle)

        End Sub

        Public Sub Update(e As Engine, dt As Single)
            Me.Bond.Update(dt)
        End Sub

    End Class
End Namespace