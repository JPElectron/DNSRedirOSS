Imports PTSoft.DnsRedirector

Public Class DnsRedirectorApp

    ''' <summary>
    ''' This method is the main entry point when the application starts
    ''' </summary>
    <STAThread()> _
    Shared Sub Main()
        Dim Splash As New SplashScreen
        Dim MainForm As New Form1

        Splash.Show()
        Splash.Refresh()

        'The dependency on win forms has been removed from DNSRedirector library so this project will not work, the following line is changed to allow the entire solution to still compile
        'MainForm._Server = New DnsServer(AppDomain.CurrentDomain.BaseDirectory, MainForm, AddressOf MainForm.OnLogEventNotification) 'Set the settings file to be read from the application directory
        MainForm._Server = New DnsServer(AppDomain.CurrentDomain.BaseDirectory, AddressOf MainForm.OnLogEventNotification) 'Set the settings file to be read from the application directory

        Splash.Close()

        If MainForm._Server.Settings.SettingsErrors.Count > 0 Then
            MessageBox.Show("The following errors were enountered while reading the settings file " & ServerSettings.DefaultIniName & ControlChars.NewLine & ControlChars.NewLine & String.Join(ControlChars.NewLine, MainForm._Server.Settings.SettingsErrors.ToArray), _
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

        If MainForm._Server.Settings.CloseToTray Then
            'Tell the form that is should hide when the form automatically reaises the show event
            MainForm._OverrideShow = True
        End If

        'Start a message loop for the main form. When this loop exits the app will close
        Application.Run(MainForm)

    End Sub



End Class
