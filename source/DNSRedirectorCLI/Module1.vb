Imports System.IO
Imports System.Threading
Imports PTSoft.DnsRedirector
Imports PTSoft.Logging

Module Module1

    Private _Worker As New Worker()
    Private _DnsRedirector As DnsServer
    Private _LogStream As FileStream
    Private _LogWriter As StreamWriter
    'Used to pass a stop message from the service thread to the worker thread

    Private _Stop As New ManualResetEvent(False)

    Sub Main()
        Dim WorkerThread As System.Threading.Thread
        Dim WorkerStart As System.Threading.ThreadStart
        WorkerStart = AddressOf _Worker.DoWork
        WorkerThread = New System.Threading.Thread(WorkerStart)
        WorkerThread.Start()


    End Sub

    Public Class Worker
        Private _thMain As System.Threading.Thread
        Private _booMustStop As Boolean = False

        Public Sub DoWork()
            'Startup the server
            _DnsRedirector = New DnsServer(AppDomain.CurrentDomain.BaseDirectory, AddressOf OnLogEventNotification)
            _DnsRedirector.StartListening()

            AddHandler Console.CancelKeyPress, AddressOf CancelKeyPressHandler

            While Not _Stop.WaitOne(0)
                Console.ReadKey(True)
            End While

        End Sub

        Protected Shared Sub CancelKeyPressHandler(ByVal sender As Object, ByVal args As ConsoleCancelEventArgs)
            args.Cancel = True

            _DnsRedirector.Dispose()

            'Signal the worker thread to exit the DoWork method
            _Stop.Set()

            Console.WriteLine(Now.ToString("HH:mm:ss") & " " & "Stopped: Press any key to continue")
        End Sub


        ''' <summary>
        ''' Writes the log data from the DNS Redirector server
        ''' </summary>
        Public Sub OnLogEventNotification(ByVal sender As Object, ByVal e As EventNotificationEventArgs)

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

                Console.WriteLine(Now.ToString("HH:mm:ss") & " " & e.Message)
                _LogWriter.WriteLine(Now.ToString("HH:mm:ss") & " " & e.Message)
            Catch ex As Exception
                'Nothing to do
            End Try
        End Sub


    End Class

End Module
