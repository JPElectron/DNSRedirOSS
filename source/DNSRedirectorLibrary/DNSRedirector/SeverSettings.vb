Imports System.IO
Imports System.Net
Imports System.Collections.ObjectModel
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Net.NetworkInformation
Imports System.Net.Sockets
Imports PTSoft.Dhcp
Imports PTSoft.Logging

Namespace DnsRedirector

    ''' <summary>
    ''' User defined settings for the DNS server
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class ServerSettings
        Implements IDisposable

        Public Enum OptimizeOption
            Memory
            Speed
        End Enum

        Public Enum JoinActionType
            None
            Online
            Auth
            Both
        End Enum

        Public Enum BlockResponseType
            Fast
            Lookup
        End Enum

        Public Enum DNSCaseOption
            Sensitive
            Insensitive
        End Enum

        Public Const DefaultIniName As String = "dnsredir.ini"
        Public Const DefaultOptimizeFor As OptimizeOption = OptimizeOption.Speed
        Public Const DefaultLogging As Logging.EventType = EventType.None
        Public Const DefaultActionNumber As Integer = 0
        Public Const DefaultJoinType As JoinActionType = JoinActionType.Online
        Public Const DefaultClientTimeout As Integer = 20 * 60000
        'Public Const DefaultMinToTray As Boolean = False
        'Public Const DefaultCloseToTray As Boolean = False
        Public Const DefaultBlockResponse As BlockResponseType = BlockResponseType.Lookup
        Public Const DefaultGetClientName = False

        Public SettingsErrors As New List(Of String)

        Private _Server As DnsServer
        Private _DhcpTimeout As Integer = 10000

        Private _PathToSettings As String
        Private _SettingsWatcher As FileSystemWatcher

        Private _ListenOnIPs As ReadOnlyCollection(Of IPEndPoint)

        Private _DnsServerIPs As ReadOnlyCollection(Of IPEndPoint)

        Private _OptimizeFor As OptimizeOption = -1

        Private _AuthClientsFileName As String
        Private _AuthClientsWatcher As FileSystemWatcher
        Private _AuthorizedClients As ReadOnlyCollection(Of IPNetwork)
        Private _LoadedAuthorizedClientsFileOn As New DateTime

        Private _SimpleDnsFileName As String
        Private _SimpleDns As New SimpleDns
        Private _SimpleDnsWatcher As FileSystemWatcher

        Private _RedirectIP As ReadOnlyCollection(Of IPAddress)

        Private _AuthKeywordsFileName As String
        Private _AuthKeywords As Text.KeywordsList
        Private _AuthKeywordsWatcher As FileSystemWatcher

        Private _AlwaysKeywordsFileName As String
        Private _AlwaysKeywords As Text.KeywordsList
        Private _AlwaysKeywordsWatcher As FileSystemWatcher

        Private _BlockedIP As ReadOnlyCollection(Of IPAddress)
        Private _BlockedKeywordsFileName As String
        Private _BlockedKeywords As Text.KeywordsList
        Private _BlockedKeywordsWatcher As FileSystemWatcher
        Private _BlockResponse As BlockResponseType = -1

        Private _AllowedKeywordsFileName As String
        Private _AllowedKeywords As Text.KeywordsList
        Private _AllowedKeywordsWatcher As FileSystemWatcher

        Private _BypassKeywordsFileName As String
        Private _BypassKeywords As Text.KeywordsList
        Private _BypassKeywordsWatcher As FileSystemWatcher

        Private _ResetClientFileName As String
        Private _ResetClient As Text.KeywordsList
        Private _ResetClientWatcher As FileSystemWatcher

        Private _ClientTimeout As Integer = DefaultClientTimeout

        Private _JoinType As JoinActionType = -1
        Private _ActionNumber As Integer = DefaultActionNumber
        Private _JoinAction As String
        Private _LeaveAction As String

        'Private _GetClientName As Nullable(Of Boolean)
        'Private _MinToTray As Boolean = DefaultMinToTray
        'Private _CloseToTray As Boolean = DefaultCloseToTray

        Private _NXDForceFileName As String
        Private _NXDForceWatcher As FileSystemWatcher
        Private _NXDForce As ReadOnlyCollection(Of IPNetwork)
        Private _LoadedNXDForceFileOn As New DateTime

        'Used when parsing text files
        Private Const CommentDelimiter = ";"
        Private _SettingKeyValyeDelimiter() As Char = {"="c}
        Private _listDelimiter() As Char = {","c}
        Private _SimpleDnsDelimiter() As Char = {ControlChars.Tab, " "}

        Private _dnsCase As DNSCaseOption

        ''' <summary>
        ''' The IP addresses and ports the server listens on for requests 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ListenEndpoints() As ReadOnlyCollection(Of IPEndPoint)
            Get
                Return _ListenOnIPs
            End Get
        End Property


        ''' <summary>
        ''' The IP addresses and ports the server uses to resolve requests
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ExternalDnsEndpoints() As ReadOnlyCollection(Of IPEndPoint)
            Get
                Return _DnsServerIPs
            End Get
        End Property

        ''' <summary>
        ''' List of IP Addresses that are always authorized
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AuthorizedClients() As ReadOnlyCollection(Of IPNetwork)
            Get
                If _AuthorizedClients Is Nothing Then
                    'Just make it a blank list
                    Dim Blank As New List(Of IPNetwork)
                    _AuthorizedClients = Blank.AsReadOnly
                End If
                Return _AuthorizedClients
            End Get
        End Property

        ''' <summary>
        ''' The last time the authorized client file was read
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Used to determine if clients should be reauthorized to the new list</remarks>
        Public ReadOnly Property LoadedAuthorizedClientsFileOn() As DateTime
            Get
                Return _LoadedAuthorizedClientsFileOn
            End Get
        End Property

        Public ReadOnly Property SimpleDns() As SimpleDns
            Get
                Return _SimpleDns
            End Get
        End Property

        ''' <summary>
        ''' Time in milliseconds that a client should be removed from the list of active clients
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ClientTimeout() As Double
            Get
                Return _ClientTimeout
            End Get
        End Property

        Public ReadOnly Property RedirectIP() As ReadOnlyCollection(Of IPAddress)
            Get
                If _RedirectIP Is Nothing Then
                    _RedirectIP = New List(Of IPAddress)(New IPAddress(0) {IPAddress.None}).AsReadOnly
                End If
                Return _RedirectIP
            End Get
        End Property

        Public ReadOnly Property AuthKeywords() As Text.KeywordsList
            Get
                If _AuthKeywords Is Nothing Then
                    _AuthKeywords = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _AuthKeywords
            End Get
        End Property

        Public ReadOnly Property AlwaysKeywords() As Text.KeywordsList
            Get
                If _AlwaysKeywords Is Nothing Then
                    _AlwaysKeywords = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _AlwaysKeywords
            End Get
        End Property


        Public ReadOnly Property BlockedIP() As ReadOnlyCollection(Of IPAddress)
            Get
                If _BlockedIP Is Nothing Then
                    _BlockedIP = New List(Of IPAddress)(New IPAddress(0) {IPAddress.None}).AsReadOnly
                End If
                Return _BlockedIP
            End Get
        End Property

        Public ReadOnly Property BlockedList() As Text.KeywordsList
            Get
                If _BlockedKeywords Is Nothing Then
                    _BlockedKeywords = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _BlockedKeywords
            End Get
        End Property

        Public ReadOnly Property BlockResponse() As BlockResponseType
            Get
                Return _BlockResponse
            End Get
        End Property

        Public ReadOnly Property AllowedList() As Text.KeywordsList
            Get
                If _AllowedKeywords Is Nothing Then
                    _AllowedKeywords = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _AllowedKeywords
            End Get
        End Property

        Public ReadOnly Property BypassList() As Text.KeywordsList
            Get
                If _BypassKeywords Is Nothing Then
                    _BypassKeywords = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _BypassKeywords
            End Get
        End Property

        Public ReadOnly Property ResetClientList() As Text.KeywordsList
            Get
                If _ResetClient Is Nothing Then
                    _ResetClient = New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
                End If
                Return _ResetClient
            End Get
        End Property

        Public ReadOnly Property JoinType() As JoinActionType
            Get
                Return _JoinType
            End Get
        End Property

        Public ReadOnly Property ActionNumber() As Integer
            Get
                Return _ActionNumber
            End Get
        End Property

        Public ReadOnly Property JoinAction() As String
            Get
                Return _JoinAction
            End Get
        End Property

        Public ReadOnly Property LeaveAction() As String
            Get
                Return _LeaveAction
            End Get
        End Property

        'Public ReadOnly Property GetClientName() As Boolean
        '    Get
        '        Return _GetClientName
        '    End Get
        'End Property

        'Public ReadOnly Property MinToTray() As Boolean
        '    Get
        '        Return _MinToTray
        '    End Get
        'End Property

        'Public ReadOnly Property CloseToTray() As Boolean
        '    Get
        '        Return _CloseToTray
        '    End Get
        'End Property

        Public ReadOnly Property NXDForce() As ReadOnlyCollection(Of IPNetwork)
            Get
                If _NXDForce Is Nothing Then
                    'Just make it a blank list
                    Dim Blank As New List(Of IPNetwork)
                    _NXDForce = Blank.AsReadOnly
                End If
                Return _NXDForce
            End Get
        End Property

        Public ReadOnly Property DNSCase As DNSCaseOption
            Get
                Return _dnsCase
            End Get
        End Property

        Friend Sub New(ByVal pathToSettingsIni As String, ByVal server As DnsServer)
            _PathToSettings = pathToSettingsIni
            _Server = server

            'Setup to watcher to notify the app if the settings file is modified
            '_SettingsWatcher = New FileSystemWatcher(_PathToSettings, DefaultIniName)
            'AddHandler _SettingsWatcher.Changed, AddressOf OnSettingsFileChanged
            '_SettingsWatcher.EnableRaisingEvents = True 'Start watching for changes to the file

        End Sub

        ''' <summary>
        ''' Parses the settings ini file
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend Sub ReadSettingsIni()

            If Not File.Exists(_PathToSettings & DefaultIniName) Then
                _Server.Log.NotifyEvent(EventType.Error, "Settings file: " & _PathToSettings & DefaultIniName & " does not exist.")
            End If

            Using SettingsStream As StreamReader = File.OpenText(_PathToSettings & DefaultIniName)

                '1st run through get all the setting that other settings depend on
                While Not SettingsStream.EndOfStream
                    Dim SettingsLine As String = SettingsStream.ReadLine()
                    If String.IsNullOrEmpty(SettingsLine) OrElse SettingsLine.StartsWith(CommentDelimiter) Then Continue While
                    Dim Setting() As String = SettingsLine.Split(_SettingKeyValyeDelimiter)
                    If Setting.Length = 2 Then
                        Select Case Setting(0).ToUpperInvariant
                            Case "LOGGING"
                                Select Case Setting(1).ToUpperInvariant
                                    Case "NORMAL"
                                        _Server.Log.EventTypes = EventType.Information Or EventType.Error
                                    Case "FULL"
                                        _Server.Log.EventTypes = EventType.All
                                    Case "OFF"
                                        _Server.Log.EventTypes = EventType.None
                                    Case Else
                                        _Server.Log.EventTypes = DefaultLogging
                                End Select
                                _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] Logging=" & Setting(1))

                            Case "OPTIMIZE"
                                Dim NewOptimizeOption As OptimizeOption
                                If String.IsNullOrEmpty(Setting(1)) OrElse Setting(1).Equals("MEMORY", StringComparison.OrdinalIgnoreCase) Then
                                    NewOptimizeOption = OptimizeOption.Memory
                                ElseIf Setting(1).Equals("SPEED", StringComparison.OrdinalIgnoreCase) Then
                                    NewOptimizeOption = OptimizeOption.Speed
                                Else
                                    NewOptimizeOption = OptimizeOption.Memory
                                End If
                                If NewOptimizeOption <> _OptimizeFor Then
                                    _OptimizeFor = NewOptimizeOption
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] Optimize=" & [Enum].GetName(GetType(OptimizeOption), _OptimizeFor))
                                End If

                            Case "LISTENONIP"
                                Dim IPs() As String = Setting(1).Split(_listDelimiter, StringSplitOptions.RemoveEmptyEntries)
                                Dim NewListenOnIPs As New List(Of IPEndPoint) 'Clear the existing list
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    For Each IP As String In IPs
                                        Dim ListenIP As IPAddress = Nothing 'Passed by ref to the parse function
                                        If IPAddress.TryParse(IP.Trim(" "c), ListenIP) Then
                                            Dim ListenEndPoint As New IPEndPoint(ListenIP, DnsServer.StandardDnsPort)
                                            If Not NewListenOnIPs.Contains(ListenEndPoint) Then NewListenOnIPs.Add(ListenEndPoint)
                                        Else
                                            _Server.Log.NotifyEvent(EventType.Warning, "Unable to listen on " & IP & " - not recognized as a valid IP address.")
                                        End If
                                    Next
                                Else 'Setting is blank
                                    For Each NIC As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
                                        If NIC.OperationalStatus = OperationalStatus.Up Then
                                            Dim IPProps As IPInterfaceProperties = NIC.GetIPProperties()
                                            Try
                                                For Each UnicastIP As UnicastIPAddressInformation In IPProps.UnicastAddresses
                                                    If UnicastIP.DuplicateAddressDetectionState = DuplicateAddressDetectionState.Preferred Then
                                                        'IPv4 for now
                                                        If UnicastIP.Address.AddressFamily = Sockets.AddressFamily.InterNetwork Then
                                                            NewListenOnIPs.Add(New IPEndPoint(UnicastIP.Address, DnsServer.StandardDnsPort))
                                                        End If
                                                    End If
                                                Next
                                            Catch ex As Exception
                                                'Above is only supported on XP or higher so use the crappy way
                                                For Each HostIP As IPAddress In System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName).AddressList
                                                    NewListenOnIPs.Add(New IPEndPoint(HostIP, DnsServer.StandardDnsPort))
                                                Next
                                            End Try

                                        End If
                                    Next
                                End If

                                Dim AreTheSame As Boolean = True
                                If _ListenOnIPs IsNot Nothing AndAlso NewListenOnIPs IsNot Nothing Then
                                    For Each IP As IPEndPoint In NewListenOnIPs
                                        If Not _ListenOnIPs.Contains(IP) Then
                                            AreTheSame = False
                                        End If
                                    Next
                                Else
                                    AreTheSame = False
                                End If

                                If Not AreTheSame Then
                                    _ListenOnIPs = NewListenOnIPs.AsReadOnly
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] ListenOnIP=" & String.Join(", ", NewListenOnIPs.ConvertAll(Of String)(New Converter(Of IPEndPoint, String)(Function(i As IPEndPoint) i.ToString)).ToArray))
                                End If

                            Case "SIMPLEDNS"
                                'Read this on the first pass because a * entry will determin if we look for DNS on DHCP

                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _SimpleDnsFileName Is Nothing Then
                                        _SimpleDnsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _SimpleDnsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _SimpleDnsFileName = Setting(1)
                                        If _SimpleDnsWatcher Is Nothing Then
                                            _SimpleDnsWatcher = New FileSystemWatcher(_PathToSettings, _SimpleDnsFileName)
                                            AddHandler _SimpleDnsWatcher.Changed, AddressOf OnSimpleDnsFileChanged
                                        Else
                                            _SimpleDnsWatcher.Filter = _SimpleDnsFileName
                                        End If

                                        ReadSimpleDnsFile()
                                        _SimpleDnsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] SimpleDNS=" & Setting(1) & " with " & _SimpleDns.Count.ToString & " entries")

                                    End If

                                ElseIf _SimpleDnsFileName Is Nothing Then
                                    _SimpleDnsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] SimpleDNS=Off")
                                End If

                            Case "DNSCASE"
                                If String.Equals(Setting(1), "insensitive", StringComparison.OrdinalIgnoreCase) Then
                                    _dnsCase = DNSCaseOption.Insensitive
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] DNSCase=" & Setting(1))
                                Else
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] DNSCase=Sensitive")
                                End If
                        End Select
                    End If
                End While

                'Set defaults incase they were left out
                If _OptimizeFor = -1 Then
                    _OptimizeFor = DefaultOptimizeFor
                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] Optimize=" & [Enum].GetName(GetType(OptimizeOption), _OptimizeFor))
                End If

                '2nd run through, start over at the begining
                SettingsStream.BaseStream.Seek(0, SeekOrigin.Begin)

                While Not SettingsStream.EndOfStream 'If the character is -1 then we are at the end of the file
                    Dim SettingsLine As String = SettingsStream.ReadLine()
                    'Skip comment lines
                    If String.IsNullOrEmpty(SettingsLine) OrElse SettingsLine.StartsWith(CommentDelimiter, StringComparison.OrdinalIgnoreCase) Then Continue While

                    Dim Setting() As String = SettingsLine.Split(_SettingKeyValyeDelimiter)

                    If Setting.Length = 2 Then 'if we have 2 items we have a key and value, else we don't know what we have

                        'Init the known settings
                        Select Case Setting(0).ToUpperInvariant 'Comparing uppercase strings is faster
                            Case "DNSSERVERIP"
                                Dim NewDnsServerIPs As New List(Of IPEndPoint)
                                If Setting(1) = String.Empty OrElse Setting(1).Equals("DHCP", StringComparison.OrdinalIgnoreCase) Then

                                    If _SimpleDns.Contains("*") Then
                                        NewDnsServerIPs.Add(New IPEndPoint(IPAddress.None, 0))
                                    Else
                                        'Query the DHCP servers on the listen on IPs

                                        Dim DhcpClient As New UdpClient
                                        DhcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _DhcpTimeout)
                                        DhcpClient.Client.Bind(New IPEndPoint(IPAddress.Any, 68))

                                        For Each NIC As NetworkInterface In NetworkInterface.GetAllNetworkInterfaces()
                                            If NIC.OperationalStatus = OperationalStatus.Up Then
                                                For Each IP As IPEndPoint In _ListenOnIPs
                                                    For Each NicIP As UnicastIPAddressInformation In NIC.GetIPProperties.UnicastAddresses
                                                        If IP.Address.Equals(NicIP.Address) Then
                                                            'Setup the DHCP request to send
                                                            Dim DhcpRequest As New DhcpMessage(MessageType.DhcpInform)
                                                            DhcpRequest.chaddr = NIC.GetPhysicalAddress
                                                            DhcpRequest.ciaddr = NicIP.Address
                                                            Dim ParamRequest As New DhcpOption(OptionType.ParameterRequestList)
                                                            ParamRequest.Value = New Byte(0) {6} '6 = domain name servers
                                                            DhcpRequest.options.Add(ParamRequest)
                                                            Dim HostName As New DhcpOption(OptionType.HostName)
                                                            HostName.Value = System.Text.Encoding.ASCII.GetBytes(Environment.UserDomainName)
                                                            DhcpRequest.options.Add(HostName)
                                                            Dim ClientID As New DhcpOption(OptionType.ClientIdentifier)
                                                            Dim ClientIDBytes As New List(Of Byte)
                                                            ClientIDBytes.Add(DhcpRequest.htype)
                                                            ClientIDBytes.AddRange(NIC.GetPhysicalAddress.GetAddressBytes)
                                                            ClientID.Value = ClientIDBytes.ToArray
                                                            DhcpRequest.options.Add(ClientID)

                                                            'Query every DHCP server that this IP is assocated with
                                                            For Each DhcpIP As IPAddress In NIC.GetIPProperties.DhcpServerAddresses
                                                                Dim Attempts As Integer = 0

                                                                While Attempts < 2 '1st attempt will be an Inform the 2nd will be a request
                                                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] DNSServerIP: Sending " & System.Enum.GetName(GetType(MessageType), DhcpRequest.options(OptionType.MessageType).Value(0)) & " message to DHCP server " & DhcpIP.ToString & " to acquire DNS servers.")
                                                                    Try
                                                                        DhcpClient.Send(DhcpRequest.GetData, DhcpRequest.GetData.Length, New IPEndPoint(DhcpIP, 67))
                                                                    Catch ex As Exception
                                                                        _Server.Log.NotifyEvent(EventType.Error, "[ReadINI] DNSServerIP: Error sending message to DHCP server " & DhcpIP.ToString & ": " & ex.Message)
                                                                        Exit While
                                                                    End Try

                                                                    While True
                                                                        Dim DhcpResponseBytes(0) As Byte
                                                                        Dim RemoteEP As New IPEndPoint(IPAddress.Any, 68)
                                                                        Try
                                                                            DhcpResponseBytes = DhcpClient.Receive(RemoteEP)
                                                                        Catch ex As Exception
                                                                            _Server.Log.NotifyEvent(EventType.Error, "[ReadINI] DNSServerIP: Error recieving message from DHCP server " & DhcpIP.ToString & ": " & ex.Message)
                                                                            'Try a request message
                                                                            DhcpRequest.options(OptionType.MessageType).Value(0) = MessageType.DhcpRequest
                                                                            Exit While
                                                                        End Try

                                                                        If IPAddress.Equals(RemoteEP.Address, DhcpIP) Then
                                                                            Dim DhcpResponse As New DhcpMessage(DhcpResponseBytes)
                                                                            If DhcpRequest.xid = DhcpResponse.xid _
                                                                            AndAlso DhcpResponse.options.Contains(OptionType.MessageType) AndAlso DhcpResponse.options(OptionType.MessageType).Value(0) = MessageType.DhcpAck Then
                                                                                If DhcpResponse.options.Contains(OptionType.DomainNameServer) Then
                                                                                    Dim DnsBytes() As Byte = DhcpResponse.options(OptionType.DomainNameServer).Value
                                                                                    For i As Integer = 0 To DnsBytes.Length - 1
                                                                                        Dim IPBytes(3) As Byte
                                                                                        Array.Copy(DnsBytes, i, IPBytes, 0, 4)
                                                                                        Dim DnsIP As New IPAddress(IPBytes)
                                                                                        Dim DnsServer As New IPEndPoint(DnsIP, DnsRedirector.DnsServer.StandardDnsPort)
                                                                                        If Not NewDnsServerIPs.Contains(DnsServer) Then
                                                                                            NewDnsServerIPs.Add(DnsServer)
                                                                                        End If
                                                                                        i += 3
                                                                                    Next
                                                                                End If
                                                                            End If
                                                                            Exit While
                                                                        End If
                                                                    End While

                                                                    Attempts += 1
                                                                End While

                                                            Next

                                                        End If
                                                    Next
                                                Next
                                            End If
                                        Next

                                        DhcpClient.Close()
                                    End If
                                Else
                                    Dim IPs() As String = Setting(1).Split(_listDelimiter, StringSplitOptions.RemoveEmptyEntries)
                                    'Clear the existing list
                                    For Each IP As String In IPs
                                        Dim DnsIP As IPAddress = Nothing 'Passed by ref to the parse function
                                        If IPAddress.TryParse(IP.Trim(" "c), DnsIP) Then
                                            Dim DnsEndPoint As New IPEndPoint(DnsIP, DnsServer.StandardDnsPort)
                                            If Not NewDnsServerIPs.Contains(DnsEndPoint) Then NewDnsServerIPs.Add(DnsEndPoint)
                                        Else
                                            _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] DNSServerIP: " & IP & " - not recognized as a valid IP address.")
                                        End If
                                    Next

                                End If

                                Dim AreTheSame As Boolean = True
                                If _DnsServerIPs IsNot Nothing AndAlso NewDnsServerIPs IsNot Nothing Then
                                    For Each IP As IPEndPoint In NewDnsServerIPs
                                        If Not _DnsServerIPs.Contains(IP) Then
                                            AreTheSame = False
                                        End If
                                    Next
                                Else
                                    AreTheSame = False
                                End If

                                If Not AreTheSame Then
                                    _DnsServerIPs = NewDnsServerIPs.AsReadOnly
                                    Dim DnsServerIPsString As String = String.Join(", ", NewDnsServerIPs.ConvertAll(Of String)(New Converter(Of IPEndPoint, String)(Function(i As IPEndPoint) i.ToString)).ToArray)
                                    If DnsServerIPsString = IPAddress.None.ToString Then
                                        DnsServerIPsString = "Off"
                                    End If
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] DNSServerIP=" & DnsServerIPsString)
                                End If


                            Case "AUTHCLIENTSFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _AuthClientsFileName Is Nothing Then
                                        _AuthClientsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _AuthClientsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _AuthClientsFileName = Setting(1)
                                        If _AuthClientsWatcher Is Nothing Then
                                            _AuthClientsWatcher = New FileSystemWatcher(_PathToSettings, _AuthClientsFileName)
                                            AddHandler _AuthClientsWatcher.Changed, AddressOf OnAuthClientsFileChanged
                                        Else
                                            _AuthClientsWatcher.Filter = _AuthClientsFileName
                                        End If

                                        ReadAuthClientsFile()
                                        _AuthClientsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AuthClientsFile=" & Setting(1) & " with " & _AuthorizedClients.Count.ToString & " entries")

                                    End If

                                ElseIf _AuthClientsFileName Is Nothing Then
                                    _AuthClientsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AuthClientsFile=Off")
                                End If

                            Case "CLIENTTIMEOUT"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    Dim NewClientTimeout As Integer
                                    If Not Integer.TryParse(Setting(1), NewClientTimeout) Then
                                        _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] ClientTimeout=" & DefaultClientTimeout.ToString & ": " & Setting(1) & " is not a valid value")
                                        _ClientTimeout = DefaultClientTimeout
                                    Else
                                        NewClientTimeout *= 60000 'convert the minutes in the file to milliseconds
                                        If NewClientTimeout <> _ClientTimeout Then
                                            _ClientTimeout = NewClientTimeout
                                            _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] ClientTimeout=" & (_ClientTimeout / 60000).ToString & " minutes")
                                        End If
                                    End If
                                ElseIf _ClientTimeout >= 0 Then
                                    _ClientTimeout = -1 'means we have set it and dont need to log when the file changes but the timeout doesn't
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] ClientTimeout=Off")
                                End If

                            Case "REDIRECTIP"
                                Dim IPs() As String = Setting(1).Split(_listDelimiter, StringSplitOptions.RemoveEmptyEntries)
                                Dim NewRedirectIP As New List(Of IPAddress)

                                For Each IP As String In IPs
                                    Dim RedirectIP As IPAddress = Nothing 'Passed by ref to the parse function
                                    If IPAddress.TryParse(IP.Trim(" "c), RedirectIP) Then
                                        If Not NewRedirectIP.Contains(RedirectIP) Then NewRedirectIP.Add(RedirectIP)
                                    Else
                                        _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] RedirectIP: " & IP & " - not recognized as a valid IP address.")
                                    End If
                                Next

                                If NewRedirectIP.Count = 0 Then
                                    NewRedirectIP.Add(IPAddress.None)
                                    _RedirectIP = NewRedirectIP.AsReadOnly
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] RedirectIP=Off")
                                ElseIf _RedirectIP Is Nothing OrElse NewRedirectIP.Count > 0 Then
                                    _RedirectIP = NewRedirectIP.AsReadOnly
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] RedirectIP=" & String.Join(", ", NewRedirectIP.ConvertAll(Of String)(New Converter(Of IPAddress, String)(Function(i As IPAddress) i.ToString)).ToArray))
                                End If

                            Case "AUTHKEYWORDSFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _AuthKeywordsFileName Is Nothing Then
                                        _AuthKeywordsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _AuthKeywordsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _AuthKeywordsFileName = Setting(1)
                                        If _AuthKeywordsWatcher Is Nothing Then
                                            _AuthKeywordsWatcher = New FileSystemWatcher(_PathToSettings, _AuthKeywordsFileName)
                                            AddHandler _AuthKeywordsWatcher.Changed, AddressOf OnAuthKeywordsFileChanged
                                        Else
                                            _AuthKeywordsWatcher.Filter = _AuthKeywordsFileName
                                        End If

                                        ReadAuthKeywordsFile()
                                        _AuthKeywordsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AuthKeywordsFile=" & Setting(1) & " with " & _AuthKeywords.Count.ToString & " entries")

                                    End If

                                ElseIf _AuthKeywordsFileName Is Nothing Then
                                    _AuthKeywordsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AuthKeywordsFile=Off")
                                End If

                            Case "ALWAYSKEYWORDSFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _AlwaysKeywordsFileName Is Nothing Then
                                        _AlwaysKeywordsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _AlwaysKeywordsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _AlwaysKeywordsFileName = Setting(1)
                                        If _AlwaysKeywordsWatcher Is Nothing Then
                                            _AlwaysKeywordsWatcher = New FileSystemWatcher(_PathToSettings, _AlwaysKeywordsFileName)
                                            AddHandler _AlwaysKeywordsWatcher.Changed, AddressOf OnAlwaysKeywordsFileChanged
                                        Else
                                            _AlwaysKeywordsWatcher.Filter = _AlwaysKeywordsFileName
                                        End If

                                        ReadAlwaysKeywordsFile()
                                        _AlwaysKeywordsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AlwaysKeywordsFile=" & Setting(1) & " with " & _AlwaysKeywords.Count.ToString & " entries")

                                    End If

                                ElseIf _AlwaysKeywordsFileName Is Nothing Then
                                    _AlwaysKeywordsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AlwaysKeywordsFile=Off")
                                End If

                            Case "BLOCKEDIP"
                                Dim IPs() As String = Setting(1).Split(_listDelimiter, StringSplitOptions.RemoveEmptyEntries)
                                Dim NewBlockedIP As New List(Of IPAddress)

                                For Each IP As String In IPs
                                    Dim BlockedIP As IPAddress = Nothing 'Passed by ref to the parse function
                                    If IPAddress.TryParse(IP.Trim(" "c), BlockedIP) Then
                                        If Not NewBlockedIP.Contains(BlockedIP) Then NewBlockedIP.Add(BlockedIP)
                                    Else
                                        _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] BlockedIP: " & IP & " - not recognized as a valid IP address.")
                                    End If
                                Next

                                If NewBlockedIP.Count = 0 Then
                                    NewBlockedIP.Add(IPAddress.None)
                                    _BlockedIP = NewBlockedIP.AsReadOnly
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockedIP=Off")
                                ElseIf _BlockedIP Is Nothing OrElse NewBlockedIP.Count > 0 Then
                                    _BlockedIP = NewBlockedIP.AsReadOnly
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockedIP=" & String.Join(", ", NewBlockedIP.ConvertAll(Of String)(New Converter(Of IPAddress, String)(Function(i As IPAddress) i.ToString)).ToArray))
                                End If

                            Case "BLOCKEDKEYWORDSFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _BlockedKeywordsFileName Is Nothing Then
                                        _BlockedKeywordsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _BlockedKeywordsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _BlockedKeywordsFileName = Setting(1)
                                        If _BlockedKeywordsWatcher Is Nothing Then
                                            _BlockedKeywordsWatcher = New FileSystemWatcher(_PathToSettings, _BlockedKeywordsFileName)
                                            AddHandler _BlockedKeywordsWatcher.Changed, AddressOf OnBlockedKeywordsFileChanged
                                        Else
                                            _BlockedKeywordsWatcher.Filter = _BlockedKeywordsFileName
                                        End If

                                        ReadBlockedKeywordsFile()
                                        _BlockedKeywordsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockedKeywordsFile=" & Setting(1) & " with " & _BlockedKeywords.Count.ToString & " entries")

                                    End If

                                ElseIf _BlockedKeywordsFileName Is Nothing Then
                                    _BlockedKeywordsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockedKeywordsFile=Off")
                                End If

                            Case "BLOCKRESPONSE"
                                Dim NewBlockResponseType As BlockResponseType

                                Select Case Setting(1).ToUpperInvariant
                                    Case "FAST"
                                        NewBlockResponseType = BlockResponseType.Fast
                                    Case "LOOKUP"
                                        NewBlockResponseType = BlockResponseType.Lookup
                                    Case Else
                                        NewBlockResponseType = DefaultBlockResponse
                                End Select

                                If NewBlockResponseType <> _BlockResponse Then
                                    _BlockResponse = NewBlockResponseType
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockResponse=" & [Enum].GetName(GetType(BlockResponseType), _BlockResponse))
                                End If

                            Case "ALLOWEDKEYWORDSFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _AllowedKeywordsFileName Is Nothing Then
                                        _AllowedKeywordsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _AllowedKeywordsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _AllowedKeywordsFileName = Setting(1)
                                        If _AllowedKeywordsWatcher Is Nothing Then
                                            _AllowedKeywordsWatcher = New FileSystemWatcher(_PathToSettings, _AllowedKeywordsFileName)
                                            AddHandler _AllowedKeywordsWatcher.Changed, AddressOf OnAllowedKeywordsFileChanged
                                        Else
                                            _AllowedKeywordsWatcher.Filter = _AllowedKeywordsFileName
                                        End If

                                        ReadAllowedKeywordsFile()
                                        _AllowedKeywordsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AllowedKeywordsFile=" & Setting(1) & " with " & _AllowedKeywords.Count.ToString & " entries")

                                    End If

                                ElseIf _AllowedKeywordsFileName Is Nothing Then
                                    _AllowedKeywordsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] AllowedKeywordsFile=Off")
                                End If

                            Case "BYPASSBLOCKFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _BypassKeywordsFileName Is Nothing Then
                                        _BypassKeywordsFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _BypassKeywordsFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _BypassKeywordsFileName = Setting(1)
                                        If _BypassKeywordsWatcher Is Nothing Then
                                            _BypassKeywordsWatcher = New FileSystemWatcher(_PathToSettings, _BypassKeywordsFileName)
                                            AddHandler _BypassKeywordsWatcher.Changed, AddressOf OnBypassKeywordsFileChanged
                                        Else
                                            _BypassKeywordsWatcher.Filter = _BypassKeywordsFileName
                                        End If

                                        ReadBypassKeywordsFile()
                                        _BypassKeywordsWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BypassBlockFile=" & Setting(1) & " with " & _BypassKeywords.Count.ToString & " entries")

                                    End If

                                ElseIf _BypassKeywordsFileName Is Nothing Then
                                    _BypassKeywordsFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BypassBlockFile=Off")
                                End If

                            Case "RESETCLIENTFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _ResetClientFileName Is Nothing Then
                                        _ResetClientFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _ResetClientFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _ResetClientFileName = Setting(1)
                                        If _ResetClientWatcher Is Nothing Then
                                            _ResetClientWatcher = New FileSystemWatcher(_PathToSettings, _ResetClientFileName)
                                            AddHandler _ResetClientWatcher.Changed, AddressOf OnResetClientFileChanged
                                        Else
                                            _ResetClientWatcher.Filter = _ResetClientFileName
                                        End If

                                        ReadResetClientFile()
                                        _ResetClientWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] ResetClientFile=" & Setting(1) & " with " & _ResetClient.Count.ToString & " entries")

                                    End If

                                ElseIf _ResetClientFileName Is Nothing Then
                                    _ResetClientFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] ResetClientFile=Off")
                                End If

                            Case "ACTIONNUMBER"
                                Dim NewActionNumber As Integer
                                If Not String.IsNullOrEmpty(Setting(1)) AndAlso Not Integer.TryParse(Setting(1), NewActionNumber) Then
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] ActionNumber=Off " & Setting(1) & " is not a valid value")
                                ElseIf NewActionNumber < 1 Then
                                    If _ActionNumber <> -1 Then
                                        _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] ActionNumber=Off")
                                    End If
                                    _ActionNumber = -1
                                ElseIf NewActionNumber <> _ActionNumber Then
                                    _ActionNumber = NewActionNumber
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] ActionNumber=" & _ActionNumber.ToString)
                                End If

                            Case "JOINTYPE"
                                Dim NewJoinType As JoinActionType
                                Select Case Setting(1).ToUpperInvariant
                                    Case "ONLINE"
                                        NewJoinType = JoinActionType.Online
                                    Case "AUTH"
                                        NewJoinType = JoinActionType.Auth
                                    Case "BOTH"
                                        NewJoinType = JoinActionType.Both
                                    Case Else
                                        NewJoinType = DefaultJoinType
                                End Select
                                If NewJoinType <> _JoinType Then
                                    _JoinType = NewJoinType
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] JoinType=" & [Enum].GetName(GetType(JoinActionType), _JoinType))
                                End If

                            Case "JOINACTION"
                                If _JoinAction Is Nothing AndAlso String.IsNullOrEmpty(Setting(1)) Then
                                    _JoinAction = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] JoinAction=Off")
                                ElseIf _JoinAction <> Setting(1) Then
                                    _JoinAction = Setting(1)
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] JoinAction=" & If(String.IsNullOrEmpty(_JoinAction), "Off", _JoinAction))
                                End If

                            Case "LEAVEACTION"
                                If _LeaveAction Is Nothing AndAlso String.IsNullOrEmpty(Setting(1)) Then
                                    _LeaveAction = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] LeaveAction=Off")
                                ElseIf _LeaveAction <> Setting(1) Then
                                    _LeaveAction = Setting(1)
                                    _Server.Log.NotifyEvent(EventType.Warning, "[ReadINI] LeaveAction=" & If(String.IsNullOrEmpty(_JoinAction), "Off", _LeaveAction))
                                End If

                            Case "NXDFORCEFILE"
                                If Not String.IsNullOrEmpty(Setting(1)) Then
                                    If _NXDForceFileName Is Nothing Then
                                        _NXDForceFileName = String.Empty
                                    End If

                                    If Not String.Equals(Setting(1), _NXDForceFileName, StringComparison.OrdinalIgnoreCase) Then

                                        _NXDForceFileName = Setting(1)
                                        If _NXDForceWatcher Is Nothing Then
                                            _NXDForceWatcher = New FileSystemWatcher(_PathToSettings, _NXDForceFileName)
                                            AddHandler _NXDForceWatcher.Changed, AddressOf OnNXDForceFileChanged
                                        Else
                                            _NXDForceWatcher.Filter = _NXDForceFileName
                                        End If

                                        ReadNXDForceFile()
                                        _NXDForceWatcher.EnableRaisingEvents = True
                                        _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] NXDForceFile=" & Setting(1) & " with " & _NXDForce.Count.ToString & " entries")

                                    End If

                                ElseIf _NXDForceFileName Is Nothing Then
                                    _NXDForceFileName = String.Empty
                                    _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] NXDForceFile=Off")
                                End If
                        End Select
                    End If
                End While
            End Using 'Closes the stream

            'Set defaults incase the value was not in the ini
            If _BlockResponse = -1 Then
                _BlockResponse = DefaultBlockResponse
                _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] BlockResponse=" & [Enum].GetName(GetType(BlockResponseType), _BlockResponse))
            End If

            If _JoinType = -1 Then
                _JoinType = DefaultJoinType
                _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] JoinType=" & [Enum].GetName(GetType(JoinActionType), _JoinType))
            End If

            'Check for any errors in the settings
            If _ListenOnIPs.Count = 0 OrElse _ListenOnIPs(0).Equals(New IPEndPoint(0, 0)) Then
                SettingsErrors.Add("ListenOnIP: Unable to bind to any IP addresses")
            End If

            If _DnsServerIPs.Count = 0 OrElse _DnsServerIPs(0).Equals(New IPEndPoint(0, 0)) Then
                SettingsErrors.Add("DNSServerIP: Extrenal DNS lookups are disabled, unable to acquire any external DNS servers")
            End If

            If _RedirectIP IsNot Nothing AndAlso Not _RedirectIP.Count = 0 AndAlso Not _RedirectIP(0).Equals(IPAddress.None) AndAlso String.IsNullOrEmpty(_AuthKeywordsFileName) Then
                SettingsErrors.Add("RedirectIP: Authorization is disabled, AuthKeywordsFile is required when using RedirectIP")
            End If

            If _RedirectIP Is Nothing OrElse _RedirectIP.Count = 0 OrElse _RedirectIP(0).Equals(IPAddress.None) Then
                If Not String.IsNullOrEmpty(_AuthKeywordsFileName) Then
                    SettingsErrors.Add("AuthKeywordsFile: Authorization is disabled, AuthKeywordsFile was specified without a RedirectIP")
                End If

                If Not String.IsNullOrEmpty(_AlwaysKeywordsFileName) Then
                    SettingsErrors.Add("AlwaysKeywordsFile: Authorization is disabled, AlwaysKeywordsFile was specified without a RedirectIP")
                End If

                If Not String.IsNullOrEmpty(_AuthClientsFileName) Then
                    SettingsErrors.Add("AuthClientsFile: Authorization is disabled, AuthClientsFile was specified without a RedirectIP")
                End If
            End If

            If _BlockedIP IsNot Nothing AndAlso Not _BlockedIP.Count = 0 AndAlso Not _BlockedIP(0).Equals(IPAddress.None) AndAlso String.IsNullOrEmpty(_BlockedKeywordsFileName) Then
                SettingsErrors.Add("BlockedIP: Blocking is disabled, BlockedKeywordsFile is required when using BlockedIP")
            End If

            If _BlockedIP Is Nothing OrElse _BlockedIP.Count = 0 OrElse _BlockedIP(0).Equals(IPAddress.None) Then
                If Not String.IsNullOrEmpty(_BlockedKeywordsFileName) Then
                    SettingsErrors.Add("BlockedKeywordsFile: Blocking is disabled, BlockedKeywordsFile was specified without a BlockedIP")
                End If

                If Not String.IsNullOrEmpty(_AllowedKeywordsFileName) Then
                    SettingsErrors.Add("AllowedKeywordsFile: Blocking is disabled, AllowedKeywordsFile was specified without a BlockedIP")
                End If

                If Not String.IsNullOrEmpty(_BypassKeywordsFileName) Then
                    SettingsErrors.Add("BypassBlockFile: Blocking is disabled, BypassBlockFile was specified without a BlockedIP")
                End If
            End If

            _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] Settings file location: " & _PathToSettings & DefaultIniName)
        End Sub

        Private Sub OnSettingsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            'Stop listening for file change events or it will get raised again
            _SettingsWatcher.EnableRaisingEvents = False

            _Server.Log.NotifyEvent(EventType.Information, "[ReadINI] The settings file: " & _PathToSettings & DefaultIniName & " has changed - Reloading")

            'We need to stop and start the server if it is running
            Dim StopStartServer As Boolean = _Server.Status = ServerStatus.Listening
            If StopStartServer Then
                _Server.StopListening()
            End If

            ReadSettingsIni()

            If StopStartServer Then
                _Server.StartListening()
            End If

            _SettingsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnAuthClientsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _AuthClientsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The AuthClientsFile: " & _AuthClientsFileName & " has changed - Reloading")

            ReadAuthClientsFile()
            Threading.Thread.Sleep(1000)

            _AuthClientsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnSimpleDnsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _SimpleDnsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The SimpleDNS file: " & _SimpleDnsFileName & " has changed - Reloading")

            'We need to stop and start the server if it is running
            Dim StopStartServer As Boolean = _Server.Status = ServerStatus.Listening
            If StopStartServer Then
                _Server.StopListening()
            End If

            ReadSimpleDnsFile()
            Threading.Thread.Sleep(1000)

            If StopStartServer Then
                _Server.StartListening()
            End If

            _SimpleDnsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnAuthKeywordsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _AuthKeywordsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The AuthKeywordsFile: " & _AuthKeywordsFileName & " has changed - Reloading")

            ReadAuthKeywordsFile()
            Threading.Thread.Sleep(1000)

            _AuthKeywordsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnAlwaysKeywordsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _AlwaysKeywordsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The AlwaysKeywordsFile: " & _AlwaysKeywordsFileName & " has changed - Reloading")

            ReadAlwaysKeywordsFile()
            Threading.Thread.Sleep(1000)

            _AlwaysKeywordsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnBlockedKeywordsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _BlockedKeywordsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The BlockedKeywordsFile: " & _BlockedKeywordsFileName & " has changed - Reloading")

            ReadBlockedKeywordsFile()
            Threading.Thread.Sleep(1000)

            _BlockedKeywordsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnAllowedKeywordsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _AllowedKeywordsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The AllowedKeywordsFile: " & _AllowedKeywordsFileName & " has changed - Reloading")

            ReadAllowedKeywordsFile()
            Threading.Thread.Sleep(1000)

            _AllowedKeywordsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnBypassKeywordsFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _BypassKeywordsWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The BypassBlockFile: " & _BypassKeywordsFileName & " has changed - Reloading")

            ReadBypassKeywordsFile()
            Threading.Thread.Sleep(1000)

            _BypassKeywordsWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnResetClientFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _ResetClientWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The ResetClientFile: " & _ResetClientFileName & " has changed - Reloading")

            ReadResetClientFile()
            Threading.Thread.Sleep(1000)

            _ResetClientWatcher.EnableRaisingEvents = True
        End Sub

        Private Sub OnNXDForceFileChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
            _NXDForceWatcher.EnableRaisingEvents = False
            Threading.Thread.Sleep(1000)

            _Server.Log.NotifyEvent(EventType.Information, "[ReadFile] The NXDForceFile: " & _NXDForceFileName & " has changed - Reloading")
            ReadNXDForceFile()
            Threading.Thread.Sleep(1000)

            _NXDForceWatcher.EnableRaisingEvents = True
        End Sub

        Protected Friend Sub ReadAuthClientsFile()

            Dim NewAuthorizedClients As New List(Of IPNetwork)
            If Not File.Exists(_PathToSettings & _AuthClientsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The AuthClientsFile: " & _PathToSettings & _AuthClientsFileName & " does not exist.")
            Else
                'Because this list isn't updated often, we will treat it as immutable which is more performant than using locks in this case

                Using AuthClientsStream As StreamReader = File.OpenText(_PathToSettings & _AuthClientsFileName)

                    While AuthClientsStream.Peek <> -1 'If the character is -1 then we are at the end of the file
                        Dim ClientIP As IPNetwork = Nothing 'This will be passsed by reference to the parse function
                        Dim ClientLine As String = AuthClientsStream.ReadLine
                        If Not ClientLine.Contains("/") Then
                            'To support CIDR networks in this list we treat anything not in cidr notation as being a single network of that ip
                            ClientLine += "/32"
                        End If
                        If IPNetwork.TryParse(ClientLine, ClientIP) Then
                            'IP was good so add it to the list
                            NewAuthorizedClients.Add(ClientIP)
                        Else
                            _Server.Log.NotifyEvent(EventType.Warning, "[ReadFile] Unable to add " & ClientLine & " to the AuthClients list - not recognized as a valid IP address.")
                        End If
                    End While

                End Using 'Closes the stream
            End If

            _LoadedAuthorizedClientsFileOn = Now
            _AuthorizedClients = NewAuthorizedClients.AsReadOnly
        End Sub

        Protected Friend Sub ReadSimpleDnsFile()

            If Not File.Exists(_PathToSettings & _SimpleDnsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The SimpleDNS file: " & _PathToSettings & _SimpleDnsFileName & " does not exist.")
                Exit Sub
            End If

            _SimpleDns = New SimpleDns
            Using SimpleDnsStream As StreamReader = File.OpenText(_PathToSettings & _SimpleDnsFileName)

                While SimpleDnsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim SimpleDnsLine As String = SimpleDnsStream.ReadLine
                    Dim SimpleDnsEntry() As String = SimpleDnsLine.Split(_SimpleDnsDelimiter, StringSplitOptions.RemoveEmptyEntries)
                    If SimpleDnsEntry.Length = 2 Then
                        Dim ServerIP As IPAddress = Nothing 'This will be passsed by reference to the parse function
                        If IPAddress.TryParse(SimpleDnsEntry(0), ServerIP) Then
                            'IP was good so add it to the list
                            If SimpleDnsEntry(1) <> "*" AndAlso Not SimpleDnsEntry(1).EndsWith(".", StringComparison.OrdinalIgnoreCase) Then SimpleDnsEntry(1) &= "." 'names always gotta end with a period
                            _SimpleDns.Add(SimpleDnsEntry(1), ServerIP)
                        Else
                            _Server.Log.NotifyEvent(EventType.Warning, "[ReadFile] Unable to add " & SimpleDnsLine & " to the SimpleDNS list - not recognized as a valid IP address.")
                        End If
                    Else
                        _Server.Log.NotifyEvent(EventType.Warning, "[ReadFile] Unable to parse SimpleDNS entry: " & SimpleDnsLine)
                    End If

                End While

            End Using 'Closes the stream
        End Sub

        Protected Friend Sub ReadAuthKeywordsFile()

            If Not File.Exists(_PathToSettings & _AuthKeywordsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The AuthKeywordsFile: " & _PathToSettings & _AuthKeywordsFileName & " does not exist.")
                Exit Sub
            End If

            'Because this list isn't updated often, we will treat it as immutable which is more performant than using locks in this case
            Dim NewAuthKeywords As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using AuthKeywordsStream As StreamReader = File.OpenText(_PathToSettings & _AuthKeywordsFileName)

                While AuthKeywordsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim AuthKeywordsLine As String = AuthKeywordsStream.ReadLine
                    If Not String.IsNullOrEmpty(AuthKeywordsLine) Then

                        If AuthKeywordsLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf AuthKeywordsLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If AuthKeywordsLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewAuthKeywords.AddRegex(AuthKeywordsLine)
                            Else
                                NewAuthKeywords.AddKeyword(AuthKeywordsLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewAuthKeywords.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _AuthKeywords = NewAuthKeywords
            _AuthKeywords.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadAlwaysKeywordsFile()

            If Not File.Exists(_PathToSettings & _AlwaysKeywordsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The AlwaysKeywordsFile: " & _PathToSettings & _AlwaysKeywordsFileName & " does not exist.")
                Exit Sub
            End If

            'Because this list isn't updated often, we will treat it as immutable which is more performant than using locks in this case
            Dim NewAlwaysKeywords As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using AlwaysKeywordsStream As StreamReader = File.OpenText(_PathToSettings & _AlwaysKeywordsFileName)

                While AlwaysKeywordsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim AlwaysKeywordsLine As String = AlwaysKeywordsStream.ReadLine
                    If Not String.IsNullOrEmpty(AlwaysKeywordsLine) Then

                        If AlwaysKeywordsLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf AlwaysKeywordsLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If AlwaysKeywordsLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewAlwaysKeywords.AddRegex(AlwaysKeywordsLine)
                            Else
                                NewAlwaysKeywords.AddKeyword(AlwaysKeywordsLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewAlwaysKeywords.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _AlwaysKeywords = NewAlwaysKeywords
            _AlwaysKeywords.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadBlockedKeywordsFile()

            If Not File.Exists(_PathToSettings & _BlockedKeywordsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The BlockedKeywordsFile: " & _PathToSettings & _BlockedKeywordsFileName & " does not exist.")
                Exit Sub
            End If

            'Instead of using locks to control access to this list we will make it immutable
            'Any clients making a request as this is being updated will read the old list
            'After this is done reading the new list will be used for subsequent requests
            Dim NewBlockedKeywords As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using BlockedKeywordsStream As StreamReader = File.OpenText(_PathToSettings & _BlockedKeywordsFileName)

                While BlockedKeywordsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim BlockedKeywordsLine As String = BlockedKeywordsStream.ReadLine
                    If Not String.IsNullOrEmpty(BlockedKeywordsLine) Then

                        If BlockedKeywordsLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf BlockedKeywordsLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If BlockedKeywordsLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewBlockedKeywords.AddRegex(BlockedKeywordsLine)
                            Else
                                NewBlockedKeywords.AddKeyword(BlockedKeywordsLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewBlockedKeywords.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _BlockedKeywords = NewBlockedKeywords
            _BlockedKeywords.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadAllowedKeywordsFile()

            If Not File.Exists(_PathToSettings & _AllowedKeywordsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The AllowedKeywordsFile: " & _PathToSettings & _AllowedKeywordsFileName & " does not exist.")
                Exit Sub
            End If

            'Instead of using locks to control access to this list we will make it immutable
            'Any clients making a request as this is being updated will read the old list
            'After this is done reading the new list will be used for subsequent requests
            Dim NewAllowedKeywords As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using AllowedKeywordsStream As StreamReader = File.OpenText(_PathToSettings & _AllowedKeywordsFileName)

                While AllowedKeywordsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim AllowedKeywordsLine As String = AllowedKeywordsStream.ReadLine
                    If Not String.IsNullOrEmpty(AllowedKeywordsLine) Then

                        If AllowedKeywordsLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf AllowedKeywordsLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If AllowedKeywordsLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewAllowedKeywords.AddRegex(AllowedKeywordsLine)
                            Else
                                NewAllowedKeywords.AddKeyword(AllowedKeywordsLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewAllowedKeywords.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _AllowedKeywords = NewAllowedKeywords
            _AllowedKeywords.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadBypassKeywordsFile()

            If Not File.Exists(_PathToSettings & _BypassKeywordsFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The BypassBlockFile: " & _PathToSettings & _BypassKeywordsFileName & " does not exist.")
                Exit Sub
            End If

            'Instead of using locks to control access to this list we will make it immutable
            'Any clients making a request as this is being updated will read the old list
            'After this is done reading the new list will be used for subsequent requests
            Dim NewBypassKeywords As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using BypassKeywordsStream As StreamReader = File.OpenText(_PathToSettings & _BypassKeywordsFileName)

                While BypassKeywordsStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim BypassKeywordsLine As String = BypassKeywordsStream.ReadLine
                    If Not String.IsNullOrEmpty(BypassKeywordsLine) Then

                        If BypassKeywordsLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf BypassKeywordsLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If BypassKeywordsLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewBypassKeywords.AddRegex(BypassKeywordsLine)
                            Else
                                NewBypassKeywords.AddKeyword(BypassKeywordsLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewBypassKeywords.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _BypassKeywords = NewBypassKeywords
            _BypassKeywords.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadResetClientFile()

            If Not File.Exists(_PathToSettings & _ResetClientFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The ResetClientFile: " & _PathToSettings & _ResetClientFileName & " does not exist.")
                Exit Sub
            End If

            'Instead of using locks to control access to this list we will make it immutable
            'Any clients making a request as this is being updated will read the old list
            'After this is done reading the new list will be used for subsequent requests
            Dim NewResetClient As New Text.KeywordsList(IIf(_dnsCase = DNSCaseOption.Sensitive, StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase))
            Using ResetClientStream As StreamReader = File.OpenText(_PathToSettings & _ResetClientFileName)

                While ResetClientStream.Peek <> -1 'If the character is -1 then we are at the end of the file

                    Dim ResetClientLine As String = ResetClientStream.ReadLine
                    If Not String.IsNullOrEmpty(ResetClientLine) Then

                        If ResetClientLine.Contains(";") Then
                            'ignore it and do not add
                        ElseIf ResetClientLine.Contains(" ") Then
                            'ignore it and do not add
                        Else
                            If ResetClientLine.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                                NewResetClient.AddRegex(ResetClientLine)
                            Else
                                NewResetClient.AddKeyword(ResetClientLine)
                            End If
                        End If
                    End If
                End While

            End Using 'Closes the stream

            Select Case _OptimizeFor
                Case OptimizeOption.Speed
                    NewResetClient.Method = Text.KeywordsList.SearchMethod.AhoCorasick
            End Select

            _ResetClient = NewResetClient
            _ResetClient.DoPreprocessing()
        End Sub

        Protected Friend Sub ReadNXDForceFile()

            Dim NewNXDForce As New List(Of IPNetwork)
            If Not File.Exists(_PathToSettings & _NXDForceFileName) Then
                _Server.Log.NotifyEvent(EventType.Error, "[ReadFile] The ReadNXDForceFile: " & _PathToSettings & _NXDForceFileName & " does not exist.")
            Else
                'Because this list isn't updated often, we will treat it as immutable which is more performant than using locks in this case

                Using NXDForceStream As StreamReader = File.OpenText(_PathToSettings & _NXDForceFileName)

                    While NXDForceStream.Peek <> -1 'If the character is -1 then we are at the end of the file
                        Dim ClientIP As IPNetwork = Nothing 'This will be passsed by reference to the parse function
                        Dim ClientLine As String = NXDForceStream.ReadLine
                        If Not ClientLine.Contains("/") Then
                            'To support CIDR networks in this list we treat anything not in cidr notation as being a single network of that ip
                            ClientLine += "/32"
                        End If
                        If IPNetwork.TryParse(ClientLine, ClientIP) Then
                            'IP was good so add it to the list
                            NewNXDForce.Add(ClientIP)
                        Else
                            _Server.Log.NotifyEvent(EventType.Warning, "[ReadFile] Unable to add " & ClientLine & " to the NXDForce list - not recognized as a valid IP address.")
                        End If
                    End While

                End Using 'Closes the stream
            End If

            _LoadedNXDForceFileOn = Now
            _NXDForce = NewNXDForce.AsReadOnly
        End Sub

        Private disposedValue As Boolean       ' To detect redundant calls

        ' IDisposable
        Protected Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    'free other state (managed objects).
                    _SimpleDnsWatcher.Dispose()
                    _SettingsWatcher.Dispose()
                    _AuthClientsWatcher.Dispose()
                    _AuthKeywordsWatcher.Dispose()
                    _AlwaysKeywordsWatcher.Dispose()
                    _BlockedKeywordsWatcher.Dispose()
                    _AllowedKeywordsWatcher.Dispose()
                    _BypassKeywordsWatcher.Dispose()
                    _ResetClientWatcher.Dispose()
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class


End Namespace

