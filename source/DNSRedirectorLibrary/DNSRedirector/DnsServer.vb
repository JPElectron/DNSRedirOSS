Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports PTSoft.Dns
Imports System.Timers
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.Windows.Forms
Imports PTSoft.Logging
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports Microsoft.Win32
Imports System.Threading.Tasks
Imports System.Linq
Imports System.Net.NetworkInformation

Namespace DnsRedirector

    Public Enum ServerStatus
        Stopped = 0
        Listening = 1
    End Enum

    ''' <summary>
    ''' Listens for DNS requests and sends responses based on the settings in the settings .ini file.
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class DnsServer
        Implements IDisposable

        Public Const StandardDnsPort As Integer = 53
        Friend Const _ThreadLockTimeout As Integer = 5000 'The amount of time a thread has when waiting for locks from other threads
        Private Const _ExternalDnsTimeout As Integer = 2000

        Private _Settings As ServerSettings
        Private _Status As ServerStatus = ServerStatus.Stopped
        Private _UdpClients As New List(Of UdpClient)
        Private _Log As New Log(Me)
        'Private _UI As Form
        Private _CurrentActionNumber As Integer
        Private _AuthClientCount As Integer
        'Used to syncronize access to the join/leave action of clients from multiple threads
        Private _JoinActionSyncRoot As New Object
        Private _LeaveActionSyncRoot As New Object

        Private _Clients As New ClientsCollection
        'Used to syncronize access to the list of clients from multiple threads
        Private _ClientsCollectionLock As New ReaderWriterLock

        Public Property MaxClients As Integer
        Public Property OverMaxClients As Integer
        Public Property DemoExpiresOn As Nullable(Of DateTime)
        Private _licenseString As String

        ''' <summary>
        ''' Status of the DNS server
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Status() As ServerStatus
            Get
                Return _Status
            End Get
        End Property

        ''' <summary>
        ''' User defined settings to control how the server responds to requests
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Settings() As ServerSettings
            Get
                Return _Settings
            End Get
        End Property

        ''' <summary>
        ''' Collection of active clients
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Clients() As ClientsCollection
            Get
                Return _Clients
            End Get
        End Property

        ''' <summary>
        ''' Used to raise events for the application to log
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Log() As Log
            Get
                Return _Log
            End Get
        End Property

        ''' <summary>
        ''' New instance of the DNS server
        ''' </summary>
        ''' <param name="pathToSettingsIni">The full path to the ini file containing user defined settings for the server</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal pathToSettingsIni As String)
            Me.New(pathToSettingsIni, Nothing)

            '_Settings = New ServerSettings(pathToSettingsIni, Me)

            ''Read and init the settings for the server
            '_Settings.ReadSettingsIni()
        End Sub

        ''' <summary>
        ''' New instance of the DNS server
        ''' </summary>
        ''' <param name="pathToSettingsIni">The full path to the ini file containing user defined settings for the server</param>
        ''' <param name="logEventHandler">The method to call when a server envent is logged</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal pathToSettingsIni As String, Optional ByVal logEventHandler As Log.EventNotificationEventHandler = Nothing)

            MaxClients = 9000

            _Settings = New ServerSettings(pathToSettingsIni, Me)

            If logEventHandler IsNot Nothing Then
                AddHandler _Log.EventNotification, logEventHandler
            End If

            _Log.NotifyEvent(EventType.Information, String.Format("[License] Supporting {0} clients.", MaxClients.ToString))

            OverMaxClients = MaxClients * 1.15

            AddHandler _Clients.ListChanged, AddressOf ClientsListChanged

            'Read and init the settings for the server
            _Settings.ReadSettingsIni()

            Dim b = True
        End Sub

        Public Sub ClientsListChanged(sender As Object, e As ListChangedEventArgs)
            If e.ListChangedType = ListChangedType.ItemAdded Then

                If _Clients.Count > MaxClients Then
                    _Log.NotifyEvent(EventType.Error, "[License] Concurrent clients of 9000 exceeded.")
                    If _Clients.Count > OverMaxClients Then
                        _Log.NotifyEvent(EventType.Error, "[License] Stopping because 15% over 9000 concurrent clients.")
                        StopListening()
                    End If

                End If
            End If
        End Sub

        ''' <summary>
        ''' Starts listeners on the DNS port for each IP specified in settings
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub StartListening()
            For Each RecieveEndPoint As IPEndPoint In _Settings.ListenEndpoints
                Try
                    Dim RecieveClient As New UdpClient(RecieveEndPoint)
                    _UdpClients.Add(RecieveClient)
                    _Log.NotifyEvent(EventType.Information, "[Initialize] Listening on " & RecieveEndPoint.Address.ToString & " port " & RecieveEndPoint.Port.ToString)
                Catch ex As Exception
                    _Log.NotifyEvent(EventType.Error, "[Initialize] Could not start listening on " & RecieveEndPoint.Address.ToString & " port " & RecieveEndPoint.Port.ToString)
                End Try
            Next

            If _UdpClients.Count > 0 Then
                _Log.NotifyEvent(EventType.Information, "[Initialize] DNS Redirector v" & My.Application.Info.Version.ToString(4) & " is almost ready")
                For Each RecieveClient As UdpClient In _UdpClients
                    RecieveClient.BeginReceive(New AsyncCallback(AddressOf OnRequestReceived), RecieveClient)
                Next
                _Status = ServerStatus.Listening
            Else
                _Log.NotifyEvent(EventType.Error, "[Initialize] DNS Redirector v" & My.Application.Info.Version.ToString(4) & " could not start")
            End If

        End Sub

        ''' <summary>
        ''' Stop the server listening for requests
        ''' </summary>
        ''' <remarks>This does not dispose of the server and it can be restarted by calling StartListening</remarks>
        Public Sub StopListening()
            For Each RecieveClient As UdpClient In _UdpClients
                RecieveClient.Close()
            Next
            _UdpClients.Clear()
            _Status = ServerStatus.Stopped

            _Log.NotifyEvent(EventType.Information, "[Initialize] DNS Listener has been stopped")
        End Sub

        ''' <summary>
        ''' Initiall processing of requests recieved
        ''' </summary>
        ''' <param name="async"></param>
        ''' <remarks></remarks>
        Protected Sub OnRequestReceived(ByVal async As IAsyncResult)
            Dim RecieveClient As UdpClient = DirectCast(async.AsyncState, UdpClient)

            Try
                Dim ClientEndPoint As IPEndPoint = New IPEndPoint(IPAddress.Any, StandardDnsPort) 'This is passed by reference to the udpclient and is set to the client's IP address when the call returns
                Dim Request() As Byte = RecieveClient.EndReceive(async, ClientEndPoint)

                If Not ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf OnProcessRequest), New DnsRawRequest(Request, ClientEndPoint, RecieveClient)) Then
                    'The request could not be queued
                    _Log.NotifyEvent(EventType.Warning, "[Timeout] Request from " & ClientEndPoint.Address.ToString & " cannot be handled, possibly because the server is too busy.")
                End If

            Catch ex As ObjectDisposedException
                'Calling close on the client calls this callback, this is a bug according to ms
                Exit Sub
            Catch ex As Exception
                'EndReceive will likley raise exceptions when clients send ICMP packets instructing the server not to respond to a request
                _Log.NotifyEvent(EventType.ErrorVerbose, "[Request] Error while recieving request: " & ex.Message)
            End Try

            'Keep trying to listen on the port again until it is availble
            Dim AsyncBegin As IAsyncResult = Nothing
            While AsyncBegin Is Nothing
                Try
                    AsyncBegin = RecieveClient.BeginReceive(New AsyncCallback(AddressOf OnRequestReceived), RecieveClient)
                Catch ex As Exception
                    _Log.NotifyEvent(EventType.ErrorVerbose, "[Request] Error while begining to recieve request: " & ex.Message)
                End Try
            End While

        End Sub

        ''' <summary>
        ''' Process the request data recieved from the client
        ''' </summary>
        ''' <param name="request"></param>
        ''' <remarks></remarks>
        Private Sub OnProcessRequest(ByVal request As Object)
            Dim RawRequest As DnsRawRequest = DirectCast(request, DnsRawRequest)

            Dim Client As Client = Nothing
            Dim IsClientNew As Boolean = False
            Dim RequestedNames As List(Of Question) = RawRequest.Request.Questions

            'Syncronize the list of clients with other threads
            'Acquire the shared reader lock, all threads can read the collection as long as there is not a write to the collection going on
            Try
                _ClientsCollectionLock.AcquireReaderLock(_ThreadLockTimeout)
            Catch ex As Exception
                'Lock timed out so the server is too busy to process atm
                Exit Sub
            End Try

            Try
                'Check if the client exists
                If Not _Clients.TryGetValue(RawRequest.ClientEndpoint.Address, Client) Then
                    'Get a write lock so other threads can not read the list while this one is writing to it
                    Try
                        _ClientsCollectionLock.UpgradeToWriterLock(_ThreadLockTimeout)
                    Catch ex As Exception
                        'Lock timed out so the server is too busy to process atm
                        Exit Sub
                    End Try
                    'Look at the collection again to make sure another thread didn't add this client
                    If Not _Clients.TryGetValue(RawRequest.ClientEndpoint.Address, Client) Then
                        'Need to add the client
                        Client = New Client(RawRequest.ClientEndpoint.Address)
                        'If Not _Settings.GetClientName Then
                        '    'Set to empty so it doesn't try and get the hostname
                        '    Client.Name = String.Empty
                        'End If
                        _Clients.Add(Client)
                        'Release the lock as early as posible
                        _ClientsCollectionLock.ReleaseWriterLock()
                        IsClientNew = True
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client Join] " & Clients.Count.ToString & " online")

                    End If
                End If
            Finally
                'The lock will be released even if we have an exception
                If _ClientsCollectionLock.IsReaderLockHeld OrElse _ClientsCollectionLock.IsWriterLockHeld Then
                    _ClientsCollectionLock.ReleaseLock()
                End If
            End Try

            Client.LastAccess = Now

            If IsClientNew Then
                'Set initial client state
                If _Settings.ClientTimeout > 0 Then
                    Client.SetTimeout(_Settings.ClientTimeout, AddressOf ClientTimeout)
                End If

                'Set if we are blocking or not
                If Not _Settings.BlockedIP(0).Equals(IPAddress.None) AndAlso _Settings.BlockedList.Count > 0 Then
                    Client.Blocking = True
                End If
            Else
                If _Settings.ClientTimeout > 0 Then
                    'Make sure the client isn't going to timeout
                    Client.UpdateTimeout()
                End If
            End If

            'Check to see if the query will allow the client to toggle a property
            For Each Question As Question In RequestedNames

                'Check if we are verifying clients
                If Not _Settings.RedirectIP(0).Equals(IPAddress.None) AndAlso _Settings.AuthKeywords.Count > 0 Then
                    'Is the client always authorized
                    If _Settings.AuthorizedClients.Any(Function(a) IPNetwork.Contains(a, RawRequest.ClientEndpoint.Address)) Then
                        Client.Authorized = True
                    Else
                        'Check to see if we are toggling authorization
                        Dim KeywordMatch As String = Nothing
                        If _Settings.AuthKeywords.Contains(Question.QName.TrimEnd("."c), KeywordMatch) Then

                            If Not Client.Authorized = True Then _AuthClientCount += 1

                            Client.Authorized = True
                            RawRequest.Verification.Add(Question, VerificationType.Authorize)

                            _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Authorization toggled: " & Question.QName)

                            'Do the JoinAction if JoinType is Auth
                            If _Settings.ActionNumber > 0 AndAlso Not String.IsNullOrEmpty(_Settings.JoinAction) _
                            AndAlso _Settings.JoinType = ServerSettings.JoinActionType.Auth _
                            OrElse _Settings.JoinType = ServerSettings.JoinActionType.Both Then
                                'Gotta be thread safe
                                SyncLock _JoinActionSyncRoot
                                    _CurrentActionNumber += 1
                                    If _CurrentActionNumber = _Settings.ActionNumber Then
                                        Dim Start As New ProcessStartInfo
                                        Start.UseShellExecute = True
                                        Start.ErrorDialog = False
                                        Start.FileName = _Settings.JoinAction
                                        Start.Arguments &= " " & Client.IP.ToString
                                        Try
                                            Process.Start(Start)
                                            _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [Client Join] Executed Join Action for authorized client")
                                        Catch ex As Exception
                                            _Log.NotifyEvent(EventType.Error, Client.IP.ToString & " [Client Join] Error while attempting to execute Join Action for authorized client: " & ex.Message)
                                        Finally
                                            _CurrentActionNumber = 0
                                        End Try
                                    ElseIf _CurrentActionNumber > _Settings.ActionNumber Then
                                        _CurrentActionNumber = 0
                                    End If
                                End SyncLock
                            End If

                        End If
                    End If
                ElseIf Not Client.Authorized Then
                    'We are not using authorization so automatically authorize the client so it can perform queries
                    Client.Authorized = True
                    _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Authorization automatic: " & Question.QName)
                End If

                'Check for a query that should reset the client
                If _Settings.ResetClientList.Contains(Question.QName.TrimEnd("."c), Nothing) Then
                    If Not RawRequest.Verification.ContainsKey(Question) Then
                        RawRequest.Verification.Add(Question, VerificationType.ResetClientToggle)
                    Else
                        RawRequest.Verification(Question) = RawRequest.Verification(Question) Or VerificationType.ResetClientToggle
                    End If
                    _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] ResetClient toggled: " & Question.QName)
                    ClientTimeout(Client)
                End If

                'Check for a query that toggles blocking
                If _Settings.BypassList.Contains(Question.QName.TrimEnd("."c), Nothing) Then
                    Client.Blocking = False
                    If Not RawRequest.Verification.ContainsKey(Question) Then
                        RawRequest.Verification.Add(Question, VerificationType.BlockBypassToggle)
                    Else
                        RawRequest.Verification(Question) = RawRequest.Verification(Question) Or VerificationType.BlockBypassToggle
                    End If
                    _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] BypassBlock toggled: " & Question.QName)
                End If

            Next

            Try
                'Resovle this request and send the response to the client
                ResolveDnsRequest(RawRequest, Client)
            Catch ex As Exception
                'Something went terribly wrong
            End Try

            If IsClientNew Then
                'Do the JoinAction if JoinType is Online
                If _Settings.ActionNumber > 0 AndAlso Not String.IsNullOrEmpty(_Settings.JoinAction) _
                AndAlso _Settings.JoinType = ServerSettings.JoinActionType.Online _
                OrElse _Settings.JoinType = ServerSettings.JoinActionType.Both Then
                    'Gotta be thread safe
                    SyncLock _JoinActionSyncRoot
                        _CurrentActionNumber += 1
                        If _CurrentActionNumber = _Settings.ActionNumber Then
                            Dim Start As New ProcessStartInfo
                            Start.UseShellExecute = True
                            Start.ErrorDialog = False
                            Start.FileName = _Settings.JoinAction
                            Start.Arguments &= " " & Client.IP.ToString
                            Try
                                Process.Start(Start)
                                _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [Client Join] Executed Join Action for online client")
                            Catch ex As Exception
                                _Log.NotifyEvent(EventType.Error, Client.IP.ToString & " [Client Join] Error while attempting to execute Join Action for online client: " & ex.Message)
                            Finally
                                _CurrentActionNumber = 0
                            End Try
                        ElseIf _CurrentActionNumber > _Settings.ActionNumber Then
                            _CurrentActionNumber = 0
                        End If
                    End SyncLock
                End If
            End If

        End Sub

        ''' <summary>
        ''' Send a response to the client request according to the user defined settings
        ''' </summary>
        ''' <param name="request"></param>
        ''' <param name="Client"></param>
        ''' <remarks></remarks>
        Private Sub ResolveDnsRequest(ByVal request As DnsRawRequest, ByVal Client As Client)

            Dim ExternalResponseBytes() As Byte = Nothing
            Dim ResponseBytes() As Byte = Nothing

            Dim Redirects As New Dictionary(Of Question, List(Of RedirectableAnswerRR))

            Dim IsQueryAllowed As Boolean = Client.Authorized

            For Each Question As Question In request.Request.Questions
                'Make sure we aren't looking at a request that has already been resolved
                If Not Redirects.ContainsKey(Question) Then

                    'Check to see if the entry is in the simple dns
                    If Question.QName.Equals("verchk.dnsr.local.", StringComparison.OrdinalIgnoreCase) Then
                        Dim Answer As New RedirectableAnswerRR
                        Answer.RedirectType = RedirectType.AppVersion
                        Answer.NAME = Question.QName
                        Answer.Class = Question.QClass
                        Answer.Type = Type.A
                        Answer.TTL = 0
                        Answer.RECORD = New RecordA(IPAddress.Parse(My.Application.Info.Version.Major.ToString & "." & My.Application.Info.Version.Minor.ToString & ".0." & My.Application.Info.Version.Revision))
                        Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Requested application version")
                        Continue For
                    ElseIf Question.QName.Equals("online.dnsr.local.", StringComparison.OrdinalIgnoreCase) Then
                        Dim Answer As New RedirectableAnswerRR
                        Answer.RedirectType = RedirectType.ClientCount
                        Answer.NAME = Question.QName
                        Answer.Class = Question.QClass
                        Answer.Type = Type.A
                        Answer.TTL = 0
                        Dim UserCountPadded() As Char = String.Format("{0:d4}", _Clients.Count).ToCharArray(0, 4)
                        Answer.RECORD = New RecordA(IPAddress.Parse(String.Format(Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", UserCountPadded(0), UserCountPadded(1), UserCountPadded(2), UserCountPadded(3))))
                        Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Requested online client count")
                        Continue For
                    ElseIf Question.QName.Equals("authonline.dnsr.local.", StringComparison.OrdinalIgnoreCase) Then
                        Dim Answer As New RedirectableAnswerRR
                        Answer.RedirectType = RedirectType.AuthClientCount
                        Answer.NAME = Question.QName
                        Answer.Class = Question.QClass
                        Answer.Type = Type.A
                        Answer.TTL = 0
                        Dim AuthCountPadded() As Char = String.Format("{0:d4}", _AuthClientCount).ToCharArray(0, 4)
                        Answer.RECORD = New RecordA(IPAddress.Parse(String.Format(Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", AuthCountPadded(0), AuthCountPadded(1), AuthCountPadded(2), AuthCountPadded(3))))
                        Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Requested authorized client count")
                        Continue For
                    ElseIf _Settings.SimpleDns.Contains(Question.QName) Then
                        Dim SimpleDnsIP As IPAddress = _Settings.SimpleDns(Question.QName).Value
                        Dim Answer As New RedirectableAnswerRR
                        Answer.RedirectType = RedirectType.SimpleDns
                        Answer.NAME = Question.QName
                        Answer.Class = Question.QClass
                        Answer.TTL = 0
                        Select Case SimpleDnsIP.AddressFamily
                            Case AddressFamily.InterNetwork
                                Answer.Type = Type.A
                                Answer.RECORD = New RecordA(SimpleDnsIP)
                            Case AddressFamily.InterNetworkV6
                                Answer.Type = Type.AAAA
                                Answer.RECORD = New RecordA(SimpleDnsIP)
                        End Select
                        Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] SimpleDNS response sent: " & Question.QName)
                        Continue For
                    End If

                    'Check to see if the client is requesting a domain in the always list
                    Dim MatchKeyword As String = Nothing
                    If _Settings.AlwaysKeywords.Contains(Question.QName.TrimEnd("."c), MatchKeyword) Then
                        IsQueryAllowed = True
                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [DNSRedir] AlwaysKeyword bypass, keyword " & MatchKeyword & ": " & Question.QName)
                        Continue For 'No need to check other factors for the request because it is always allowed
                    End If

                    If IsQueryAllowed Then
                        If Client.Blocking Then
                            'Check if this domain matches an entry in the blocked keywords list
                            Dim MatchedKeyword As String = Nothing
                            Dim Blocked As Boolean = _Settings.BlockedList.Contains(Question.QName.TrimEnd("."c), MatchedKeyword)

                            If Blocked Then
                                Dim MatchedAllowedKeyword As String = Nothing
                                'Now check if this name is in the allowed list
                                If _Settings.AllowedList.Contains(Question.QName.TrimEnd("."c), MatchedAllowedKeyword) Then
                                    Blocked = False
                                    _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [DNSRedir] AllowedKeyword bypass, keyword " & MatchedAllowedKeyword & ": " & Question.QName)
                                    Exit For 'No need to conintue checking
                                End If

                                If Blocked Then
                                    'Any other addresses resolve to the wildcard ip
                                    For Each IP As IPAddress In _Settings.BlockedIP
                                        Dim Answer As New RedirectableAnswerRR
                                        Answer.RedirectType = RedirectType.Blocked
                                        Answer.NAME = Question.QName
                                        Answer.Class = Question.QClass
                                        Answer.TTL = 0
                                        Select Case IP.AddressFamily
                                            Case AddressFamily.InterNetwork
                                                Answer.Type = Type.A
                                                Answer.RECORD = New RecordA(IP)
                                            Case AddressFamily.InterNetworkV6
                                                Answer.Type = Type.AAAA
                                                Answer.RECORD = New RecordAAAA(IP)
                                        End Select
                                        If Not Redirects.ContainsKey(Question) Then
                                            Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                                        Else
                                            Redirects(Question).Add(Answer)
                                        End If
                                    Next

                                    _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [DNSRedir] BlockedIP response sent, keyword " & MatchedKeyword & ": " & Question.QName & " -> " & ToCommaSeperateString(Of IPAddress)(_Settings.BlockedIP))
                                    Client.IncrementBlocked()
                                    Continue For
                                End If

                            End If
                        End If
                    ElseIf Not _Settings.RedirectIP(0).Equals(IPAddress.None) Then
                        'The query for this name is not allowed so redirect it to the redirect ip
                        For Each IP As IPAddress In _Settings.RedirectIP
                            Dim Answer As New RedirectableAnswerRR
                            Answer.RedirectType = RedirectType.Unauthorized
                            Answer.NAME = Question.QName
                            Answer.Class = Question.QClass
                            Answer.TTL = 0
                            Select Case IP.AddressFamily
                                Case AddressFamily.InterNetwork
                                    Answer.Type = Type.A
                                    Answer.RECORD = New RecordA(IP)
                                Case AddressFamily.InterNetworkV6
                                    Answer.Type = Type.AAAA
                                    Answer.RECORD = New RecordAAAA(IP)
                            End Select
                            If Not Redirects.ContainsKey(Question) Then
                                Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                            Else
                                Redirects(Question).Add(Answer)
                            End If
                        Next

                        _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client] Request for " & Question.QName & " redirected to " & ToCommaSeperateString(Of IPAddress)(_Settings.RedirectIP) & " because the client is not authorized")
                        Continue For
                    Else
                        'The client is not allowed to query the domain and there is nowhere to redirect to so block it
                        Exit Sub 'To do, figure out what to do in this case if it ever comes up
                    End If
                End If
            Next

            'Remove questions in the request that have already been resolved by SimpleDNS or redirected
            Dim BlockedNames As New List(Of String)
            For Each Resolved As KeyValuePair(Of Question, List(Of RedirectableAnswerRR)) In Redirects
                For Each Answer As RedirectableAnswerRR In Resolved.Value
                    'If block response is set to lookup, keep the blocked names in the request because we will want to see if they resolve
                    If Answer.RedirectType <> RedirectType.Blocked OrElse _Settings.BlockResponse = ServerSettings.BlockResponseType.Fast Then
                        request.Request.Questions.Remove(Resolved.Key)
                    Else
                        If Not BlockedNames.Contains(Resolved.Key.QName) Then BlockedNames.Add(Resolved.Key.QName)
                    End If
                Next
            Next


            Dim DnsServerIndex As Integer = 0
            If request.Request.Questions.Count > 0 Then
                'Entries are not in simple dns or redirected so get from the external name server
                Dim DnsQueryClient As New UdpClient
                DnsQueryClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _ExternalDnsTimeout)

                For DnsServerIndex = 0 To _Settings.ExternalDnsEndpoints.Count - 1
                    If Not _Settings.ExternalDnsEndpoints(DnsServerIndex).Address.Equals(IPAddress.None) Then
                        Try
                            DnsQueryClient.Send(request.Request.Data, request.Request.Data.Length, _Settings.ExternalDnsEndpoints(DnsServerIndex))
                            Dim DnsServer As New IPEndPoint(IPAddress.Any, 0)
                            ExternalResponseBytes = DnsQueryClient.Receive(DnsServer)
                        Catch ex As SocketException
                            _Log.NotifyEvent(EventType.Warning, Client.IP.ToString & " [DNS Server] " & _Settings.ExternalDnsEndpoints(DnsServerIndex).Address.ToString & ": " & ex.Message)
                            Continue For 'Try the next server
                        End Try
                        'If we made it here, then we don't need to check another server
                        Exit For
                    End If
                Next
            End If

            'Create a response object to send back to the client
            Dim Response As Response
            If ExternalResponseBytes IsNot Nothing Then
                'We have a response from and external DNS server
                'Construct the repsonse object with the data
                Response = New Response(_Settings.ExternalDnsEndpoints(DnsServerIndex), ExternalResponseBytes)
                Dim forcedNXD = False

                'Check for any blocked responses because this did resolve, otherwise it will pass NXDOMAIN
                If Response.header.RCODE = RCode.NoError Then
                    Dim BlockedAnswers As New List(Of AnswerRR)

                    'Add any CNAME responses that match a blocked keyword to the blocked keywords list so we can block CNAMEs too
                    For Each ResponseAnswer As AnswerRR In Response.Answers
                        If ResponseAnswer.Type = Type.CNAME Then
                            If BlockedNames.Contains(ResponseAnswer.NAME) Then
                                Dim CNameAnswer As RecordCNAME = DirectCast(ResponseAnswer.RECORD, RecordCNAME)
                                If Not BlockedNames.Contains(CNameAnswer.CNAME) Then BlockedNames.Add(CNameAnswer.CNAME)
                            End If
                        End If
                    Next

                    For Each ResponseAnswer As AnswerRR In Response.Answers
                        If BlockedNames.Contains(ResponseAnswer.NAME) Then
                            BlockedAnswers.Add(ResponseAnswer)
                        End If
                    Next

                    'Remove them, here because we cant remove them in the for loop enumerating the answer list of the response
                    For Each BlockedAnswer As AnswerRR In BlockedAnswers
                        Response.RemoveAnswer(BlockedAnswer)
                    Next


                    'If we still have answers, check if we are forcing NXDomain on them
                    If _Settings.NXDForce.Any() Then
                        For Each ResponseAnswer As AnswerRR In Response.Answers
                            Dim address As IPAddress = Nothing
                            Select Case ResponseAnswer.Type
                                Case Dns.Type.A
                                    address = DirectCast(ResponseAnswer.RECORD, RecordA).Address
                                Case Dns.Type.AAAA
                                    address = DirectCast(ResponseAnswer.RECORD, RecordAAAA).Address
                                Case Else

                            End Select

                            If address IsNot Nothing Then
                                If _Settings.NXDForce.Any(Function(f) IPNetwork.Contains(f, address)) Then
                                    Response.header.RCODE = RCode.NXDomain

                                    'Response.Answers.Clear()
                                    For Each toRemove In Response.Answers.ToList()
                                        Response.RemoveAnswer(toRemove)
                                    Next

                                    forcedNXD = True

                                    _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [DNSRedir] Response sent: " & String.Join(", ", Response.Questions.Select(Function(q) q.QName)) & " NXDomain by NXDForce " & address.ToString())

                                    Exit For
                                End If
                            End If
                        Next

                    End If
                End If

                For Each Question As Question In Response.Questions
                    'Check for NXDOMAIN
                    If Response.header.RCODE = RCode.NXDomain Then
                        'Check to see if the question matched a keywords list
                        For Each ResponseQuestion As Question In Response.Questions
                            For Each Verify As KeyValuePair(Of Question, VerificationType) In request.Verification
                                If Verify.Key.QName.Equals(ResponseQuestion.QName, StringComparison.OrdinalIgnoreCase) Then

                                    If (Verify.Value And VerificationType.Authorize) > 0 Then
                                        For Each IP As IPAddress In _Settings.RedirectIP
                                            Dim Answer As New RedirectableAnswerRR
                                            Answer.RedirectType = RedirectType.Unauthorized
                                            Answer.NAME = Question.QName
                                            Answer.Class = Question.QClass
                                            Answer.TTL = 0
                                            Select Case IP.AddressFamily
                                                Case AddressFamily.InterNetwork
                                                    Answer.Type = Type.A
                                                    Answer.RECORD = New RecordA(IP)
                                                Case AddressFamily.InterNetworkV6
                                                    Answer.Type = Type.AAAA
                                                    Answer.RECORD = New RecordAAAA(IP)
                                            End Select
                                            If Not Redirects.ContainsKey(Question) Then
                                                Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                                            Else
                                                Redirects(Question).Add(Answer)
                                            End If
                                        Next
                                        Response.header.RCODE = RCode.NoError
                                    End If

                                    If (Verify.Value And VerificationType.BlockBypassToggle) > 0 Then
                                        For Each IP As IPAddress In _Settings.BlockedIP
                                            Dim Answer As New RedirectableAnswerRR
                                            Answer.RedirectType = RedirectType.Blocked
                                            Answer.NAME = Question.QName
                                            Answer.Class = Question.QClass
                                            Answer.TTL = 0
                                            Select Case IP.AddressFamily
                                                Case AddressFamily.InterNetwork
                                                    Answer.Type = Type.A
                                                    Answer.RECORD = New RecordA(IP)
                                                Case AddressFamily.InterNetworkV6
                                                    Answer.Type = Type.AAAA
                                                    Answer.RECORD = New RecordAAAA(IP)
                                            End Select
                                            If Not Redirects.ContainsKey(Question) Then
                                                Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                                            Else
                                                Redirects(Question).Add(Answer)
                                            End If
                                        Next
                                        Response.header.RCODE = RCode.NoError
                                    End If

                                    If (Verify.Value And VerificationType.ResetClientToggle) > 0 Then
                                        For Each IP As IPAddress In _Settings.RedirectIP
                                            Dim Answer As New RedirectableAnswerRR
                                            Answer.RedirectType = RedirectType.Unauthorized
                                            Answer.NAME = Question.QName
                                            Answer.Class = Question.QClass
                                            Answer.TTL = 0
                                            Select Case IP.AddressFamily
                                                Case AddressFamily.InterNetwork
                                                    Answer.Type = Type.A
                                                    Answer.RECORD = New RecordA(IP)
                                                Case AddressFamily.InterNetworkV6
                                                    Answer.Type = Type.AAAA
                                                    Answer.RECORD = New RecordAAAA(IP)
                                            End Select
                                            If Not Redirects.ContainsKey(Question) Then
                                                Redirects.Add(Question, New List(Of RedirectableAnswerRR)(New RedirectableAnswerRR(0) {Answer}))
                                            Else
                                                Redirects(Question).Add(Answer)
                                            End If
                                        Next
                                        Response.header.RCODE = RCode.NoError
                                    End If

                                    Exit For
                                End If
                            Next

                            If Not forcedNXD Then
                                _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [DNSRedir] Response sent: " & Question.QName & " type NOT FOUND by " & _Settings.ExternalDnsEndpoints(DnsServerIndex).Address.ToString)
                            End If
                        Next

                    Else
                        _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [DNSRedir] Response sent: " & Question.QName & " type " & System.Enum.GetName(GetType(Dns.QType), Question.QType) & " by " & _Settings.ExternalDnsEndpoints(DnsServerIndex).Address.ToString)
                    End If
                Next
            Else
                'Create a new blank response
                Response = New Response(request.Request.header)
            End If

            'Add any questions and answer to the response we resolved with simple DNS or redirects
            For Each Resolved As KeyValuePair(Of Question, List(Of RedirectableAnswerRR)) In Redirects
                Dim HasQuestion As Boolean = False
                For Each Question As Question In Response.Questions
                    If Question.QName.Equals(Resolved.Key.QName, StringComparison.OrdinalIgnoreCase) Then
                        HasQuestion = True
                        Exit For
                    End If
                Next

                If Not HasQuestion Then
                    Response.AddQuestion(Resolved.Key)
                End If

                For Each Answer As RedirectableAnswerRR In Resolved.Value
                    If Response.header.RCODE = RCode.NoError OrElse Answer.RedirectType <> RedirectType.Blocked Then
                        Response.AddAnswer(Answer)
                    End If
                Next
            Next

            'If the request toggles an anthorization property and the TTL is 0 increase it
            For Each ResponseQuestion As Question In Response.Questions
                For Each Verify As KeyValuePair(Of Question, VerificationType) In request.Verification
                    If Verify.Key.QName.Equals(ResponseQuestion.QName, StringComparison.OrdinalIgnoreCase) Then
                        For Each Answer As AnswerRR In Response.Answers
                            If Answer.TTL = 0 AndAlso Answer.NAME.Equals(Verify.Key.QName, StringComparison.OrdinalIgnoreCase) Then
                                Answer.TTL = 10
                            End If
                        Next
                    End If
                Next
            Next


            Try
                ResponseBytes = Response.Data
                'Send the reply
                request.Socket.Send(ResponseBytes, ResponseBytes.Length, request.ClientEndpoint)
            Catch ex As Exception
                _Log.NotifyEvent(EventType.Error, Client.IP.ToString & " [Client] Error sending response to client: " & ex.Message)
            End Try

            'Increment redirection
            For Each Answer As AnswerRR In Response.Answers
                If Answer.Type = Type.A Then
                    If _Settings.RedirectIP.Contains(DirectCast(Answer.RECORD, RecordA).Address) Then
                        Client.IncrementRedirected()
                        Exit For
                    End If
                ElseIf Answer.Type = Type.AAAA Then
                    If _Settings.RedirectIP.Contains(DirectCast(Answer.RECORD, RecordAAAA).Address) Then
                        Client.IncrementRedirected()
                        Exit For
                    End If
                End If
            Next

        End Sub

        ''' <summary>
        ''' Called when a client timer has ellapsed meaning the client should be removed from the list of active clients
        ''' </summary>
        ''' <param name="TimeoutState"></param>
        ''' <remarks></remarks>
        Private Sub ClientTimeout(ByVal TimeoutState As Object)
            Dim Client As Client = DirectCast(TimeoutState, Client)

            'Ensure that other requests can't read the clients list while it is being modified
            Try
                _ClientsCollectionLock.AcquireWriterLock(_ThreadLockTimeout)
                _Clients.Remove(Client)
                Client.Dispose()
            Finally
                If _ClientsCollectionLock.IsWriterLockHeld Then
                    _ClientsCollectionLock.ReleaseWriterLock()
                End If
            End Try

            _AuthClientCount -= 1
            If _AuthClientCount < 0 Then _AuthClientCount = 0

            _Log.NotifyEvent(EventType.Information, Client.IP.ToString & " [Client Leave] " & Clients.Count.ToString & " online")

            If _Settings.ActionNumber = 1 AndAlso Not String.IsNullOrEmpty(_Settings.LeaveAction) Then
                'Gotta be thread safe
                SyncLock _LeaveActionSyncRoot
                    Dim Start As New ProcessStartInfo
                    Start.UseShellExecute = True
                    Start.ErrorDialog = False
                    Start.FileName = _Settings.LeaveAction
                    Start.Arguments &= " " & Client.IP.ToString
                    Try
                        Process.Start(Start)
                        _Log.NotifyEvent(EventType.InformationVerbose, Client.IP.ToString & " [Client Leave] Executed Leave Action for client")
                    Catch ex As Exception
                        _Log.NotifyEvent(EventType.Error, Client.IP.ToString & " [Client Leave] Error while attempting to execute Leave Action for client: " & ex.Message)
                    End Try
                End SyncLock
            End If

        End Sub

        ''' <summary>
        ''' Convert a list of objects to its .ToString equlivent seperated by commas
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="List"></param>
        ''' <returns></returns>
        ''' <remarks>Typically used for logging</remarks>
        Private Function ToCommaSeperateString(Of T)(ByVal List As IEnumerable(Of T)) As String
            Dim SB As New System.Text.StringBuilder
            For Each Item As T In List
                SB.Append(Item.ToString)
                SB.Append(", ")
            Next
            Return SB.ToString.TrimEnd(" "c).TrimEnd(","c)
        End Function

#Region "IDisposable"

        'When the server object is no longer needed, we want to make sure to stop the listeners

        Private _disposedValue As Boolean        ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(ByVal disposing As Boolean)
            If Not Me._disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                    If _Status = ServerStatus.Listening Then
                        StopListening()
                    End If
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me._disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
#End Region


    End Class
End Namespace
