Imports System.Net
Imports PTSoft.DnsRedirector
Imports PTSoft.Logging
Imports System.IO

Public Class Form1

    Friend _Server As DnsServer

    Private _LogStream As FileStream
    Private _LogWriter As StreamWriter
    Friend WithEvents NotifyIcon As System.Windows.Forms.NotifyIcon
    Private NotifytMenu As System.Windows.Forms.ContextMenu
    Friend WithEvents MenuExitItem As System.Windows.Forms.MenuItem
    Private _OverrideClose As Boolean = False
    Friend _OverrideShow As Boolean = False

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'Initiallize the notification icon
        components = New System.ComponentModel.Container
        NotifyIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        NotifyIcon.Icon = New Icon(Me.GetType, "icon.ico")
        'Initialize menu
        NotifytMenu = New System.Windows.Forms.ContextMenu
        MenuExitItem = New System.Windows.Forms.MenuItem

        NotifytMenu.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {MenuExitItem})

        'Initialize menuItem
        MenuExitItem.Index = 0
        MenuExitItem.Text = "E&xit"

        NotifyIcon.ContextMenu = NotifytMenu

    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If _Server.Settings.CloseToTray Then
            Me.Visible = False
        End If

        Me.DataGridView1.DataSource = _Server.Clients
        With Me.DataGridView1
            .Columns("IP").DisplayIndex = 0
            .Columns("IP").Width = 115
            .Columns("Name").DisplayIndex = 1
            .Columns("Name").Width = 115
            .Columns("Authorized").SortMode = DataGridViewColumnSortMode.Automatic
            .Columns("Blocking").SortMode = DataGridViewColumnSortMode.Automatic
            .Columns("LastAccess").HeaderText = "Last Access"
            .Columns("LastAccess").Width = 130
        End With

        _Server.StartListening()

        UpdateClientCountDisplay()

        'Catch any unhandled exceptions
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf HandleUnhandledException
    End Sub

    Sub HandleUnhandledException(ByVal sender As Object, ByVal args As UnhandledExceptionEventArgs)
        Dim e As Exception = DirectCast(args.ExceptionObject, Exception)
        _Server.Log.NotifyEvent(EventType.Error, "[Unhandled Exception] " + e.Message)
    End Sub

    Public Sub UpdateClientCountDisplay()
        Label1.Text = String.Format("{0} client{1} online", Me.DataGridView1.RowCount.ToString, If(Me.DataGridView1.RowCount = 1, "", "s"))
        Label2.Text = String.Format("v" & My.Application.Info.Version.ToString(4))
        'JP ADD3
        'If Me.DataGridView1.RowCount > 2875 Then
        'MessageBox.Show("Concurrent client license exceeded, please purchase a sufficient license. DNS Listener has been stopped, please exit the software.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        'End If

    End Sub

    'A type safe function pointer so we can use the UI thread to manage log updates
    Public Delegate Sub UpdateLogMethod(ByVal Message As String)

    Public Sub OnLogEventNotification(ByVal sender As Object, ByVal e As EventNotificationEventArgs)
        'My.Application.Log.WriteEntry(e.Message)

        If Me.InvokeRequired Then
            'The log event can be raised from another thread
            'UI controls can only be updated by the thread that created them
            'So we need to invoke the method to update the log textbox from the thread the form is on using the forms invloke method

            'Or if writing to a file, we want this to happen on 1 thread at a time so we will just reuse the UI thread
            Dim Params(0) As Object
            Params(0) = Now.ToString("HH:mm:ss") & " " & e.Message
            'Pass the pointer to our log function to the UI thread
            Me.Invoke(New UpdateLogMethod(AddressOf UpdateLog), Params)
        Else
            'The event was raised by the same thread as the form so no need to invoke
            UpdateLog(Now.ToString("HH:mm:ss") & " " & e.Message)
        End If

    End Sub

    Public Sub UpdateLog(ByVal Message As String)

        Try
            'Do we need a new log?
            If _LogStream Is Nothing OrElse _LogStream IsNot Nothing AndAlso Path.GetFileName(_LogStream.Name) <> Now.Date.ToString("MMddyy") & ".txt" Then
                'Yes, so out with the old
                If _LogStream IsNot Nothing Then
                    _LogStream.Dispose()
                End If
                If _LogWriter IsNot Nothing Then
                    _LogWriter.Dispose()
                End If
                'and in with the new
                If Not Directory.Exists("DailyLogs") Then Directory.CreateDirectory("DailyLogs")
                _LogStream = New FileStream("DailyLogs" & Path.DirectorySeparatorChar & Now.Date.ToString("MMddyy") & ".txt", FileMode.Append, FileAccess.Write, FileShare.Read)
                _LogWriter = New StreamWriter(_LogStream)
                _LogWriter.AutoFlush = True
            End If

            _LogWriter.WriteLine(Message)
        Catch ex As Exception
            'Nothing to do
        End Try
    End Sub

    Private Sub DataGridView1_CellDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellDoubleClick
        'Ignore clicks that are not on a row 
        If e.RowIndex < 0 Then Exit Sub

        'Get the IP of the row
        Dim IP As IPAddress = DirectCast(DataGridView1(DataGridView1.Columns("IP").Index, e.RowIndex).Value, IPAddress)

        'Show the message form for the IP
        Dim Message As New Message
        Message.IP = IP
        Message.ShowDialog()
    End Sub

    Private Sub DataGridView1_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError
        'We can ignore data errors
        e.ThrowException = False
    End Sub

    Private Sub DataGridView1_RowsAdded(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewRowsAddedEventArgs) Handles DataGridView1.RowsAdded
        UpdateClientCountDisplay()
    End Sub

    Private Sub DataGridView1_RowsRemoved(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewRowsRemovedEventArgs) Handles DataGridView1.RowsRemoved
        UpdateClientCountDisplay()
    End Sub

    Private Sub Form1_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Try
            'Clean up the log stream
            If _LogStream IsNot Nothing Then _LogStream.Dispose()
            If _LogWriter IsNot Nothing Then _LogWriter.Dispose()
            'Clean up the notify icon
            If (components IsNot Nothing) Then
                components.Dispose()
            End If
        Catch ex As Exception
            'Nothing to do
        End Try
    End Sub

    Private Sub Form1_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        'Handle the minimize to tray if set
        If Me.WindowState = FormWindowState.Minimized AndAlso _Server IsNot Nothing AndAlso _Server.Settings.MinToTray Then
            Me.Hide()
            NotifyIcon.Visible = True
        End If
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        'If close to tray is set, cancel the close command and hide the form and show the notification icon
        If _Server IsNot Nothing AndAlso _Server.Settings.CloseToTray AndAlso Not _OverrideClose Then
            e.Cancel = True
            Me.Hide()
            NotifyIcon.Visible = True
        End If
    End Sub

    Private Sub NotifyIcon_DoubleClick(ByVal Sender As Object, ByVal e As EventArgs) Handles NotifyIcon.DoubleClick
        ' Show the form when the user double clicks on the notify icon.

        ' Set the WindowState to normal if the form is minimized.
        Me.Visible = True
        Me.WindowState = FormWindowState.Normal
        Me.NotifyIcon.Visible = False
    End Sub

    Private Sub MenuExitItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles MenuExitItem.Click
        'The notify icons exit menu overrides the close behavior to close to tray, because it is already closed to the tray
        _OverrideClose = True
        Me.Close()
    End Sub

    Private Sub Form1_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        'This is called automaticaaly on startup so we will want to make sure not to show the form if close to tray is set
        If _OverrideShow Then
            Me.WindowState = FormWindowState.Minimized
            _OverrideShow = False
        End If
    End Sub

End Class
