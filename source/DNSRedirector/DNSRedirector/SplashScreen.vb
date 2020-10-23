Public Class SplashScreen

    Private Sub SplashScreen_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Label2.Text = String.Format("v" & My.Application.Info.Version.ToString(4))
    End Sub

End Class