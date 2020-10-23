Imports System.Net

Public Class Message

    Private _IP As IPAddress

    Public Property IP() As IPAddress
        Get
            Return _IP
        End Get
        Set(ByVal value As IPAddress)
            _IP = value
            Label2.Text = _IP.ToString
        End Set
    End Property

    Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k net send " & _IP.ToString & " " & TextBox1.Text
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k nbtstat -A " & _IP.ToString
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k ping " & _IP.ToString
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k psshutdown -r -f \\" & _IP.ToString
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k psshutdown -k -f \\" & _IP.ToString
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/c start ""Default Share"" \\" & _IP.ToString & "\c$"
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        Dim Start As New ProcessStartInfo
        Start.FileName = "cmd"
        Start.Arguments = "/k ping -t " & _IP.ToString
        Start.UseShellExecute = True
        Process.Start(Start)
    End Sub
End Class