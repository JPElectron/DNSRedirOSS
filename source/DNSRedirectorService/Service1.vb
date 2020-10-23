Imports PTSoft.DnsRedirector
Imports PTSoft.Logging
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Public Class Service1

    Private _Worker As New Worker()
    Private Shared _DnsRedirector As DnsServer
    Private Shared _LogStream As FileStream
    Private Shared _LogWriter As StreamWriter
    'Used to pass a stop message from the service thread to the worker thread
    Private Shared _Stop As New ManualResetEvent(False)

    Private Shared _logLock As New Object()

    Protected Overrides Sub OnStart(ByVal args() As String)

        'Services cannot be debugged by the integrated debugger and the debugger must be manually attached
        'During the manual attaching process OnStart will have already run and cannot be debugged unless you uncomment the foloowing line
        'You will then be prompted to start the debugger automatically
        'Debugger.Break()

        'We need to start a new thread to do the work on so OnStart can return within 30 seconds (othwise the service manager will stop the service)
        Dim WorkerThread As System.Threading.Thread
        Dim WorkerStart As System.Threading.ThreadStart
        WorkerStart = AddressOf _Worker.DoWork
        WorkerThread = New System.Threading.Thread(WorkerStart)
        WorkerThread.Start()

    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
        _DnsRedirector.Dispose()

        'Signal the worker thread to exit the DoWork method
        _Stop.Set()
    End Sub


    Public Class Worker
        Private _thMain As System.Threading.Thread
        Private _booMustStop As Boolean = False

        Public Sub DoWork()
            'Startup the server
            _DnsRedirector = New DnsServer(AppDomain.CurrentDomain.BaseDirectory, AddressOf OnLogEventNotification)
            _DnsRedirector.StartListening()

            'Make this thread wait until _Stop is signaled. It will then continue execution from this point
            _Stop.WaitOne()

        End Sub


        ''' <summary>
        ''' Writes the log data from the DNS Redirector server
        ''' </summary>
        Public Sub OnLogEventNotification(ByVal sender As Object, ByVal e As EventNotificationEventArgs)

            Task.Run(Sub()
                         SyncLock _logLock
                             Try
                                 'Do we need a new log?
                                 If _LogStream Is Nothing OrElse _LogStream IsNot Nothing AndAlso Path.GetFileName(AppDomain.CurrentDomain.BaseDirectory & "DailyLogs" & Path.DirectorySeparatorChar & _LogStream.Name) <> Now.Date.ToString("MMddyy") & ".txt" Then
                                     'Yes, so out with the old
                                     If _LogStream IsNot Nothing Then
                                         _LogStream.Dispose()
                                     End If
                                     If _LogWriter IsNot Nothing Then
                                         _LogWriter.Dispose()
                                     End If
                                     'and in with the new
                                     If Not Directory.Exists(AppDomain.CurrentDomain.BaseDirectory & "DailyLogs") Then Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory & "DailyLogs")
                                     _LogStream = New FileStream(AppDomain.CurrentDomain.BaseDirectory & "DailyLogs" & Path.DirectorySeparatorChar & Now.Date.ToString("MMddyy") & ".txt", FileMode.Append, FileAccess.Write, FileShare.Read)
                                     _LogWriter = New StreamWriter(_LogStream)
                                     _LogWriter.AutoFlush = True
                                 End If

                                 _LogWriter.WriteLine(Now.ToString("HH:mm:ss") & " " & e.Message)
                             Catch ex As Exception
                                 'Nothing to do
                             End Try
                         End SyncLock
                     End Sub
                )
        End Sub


    End Class

End Class
