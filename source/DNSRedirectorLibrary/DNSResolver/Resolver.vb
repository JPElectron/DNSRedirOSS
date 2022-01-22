Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Net
Imports System.Text
Imports System.Net.Sockets

Imports System.Net.NetworkInformation

Imports System.Diagnostics
Imports System.Runtime.Remoting.Messaging


Namespace Dns
    ''' <summary>
    ''' Resolver is the main class to do DNS query lookups
    ''' </summary>
    Public Class Resolver
        ''' <summary>
        ''' Version of this set of routines, when not in a library
        ''' </summary>
        Public ReadOnly Property Version() As String
            Get
                Return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            End Get
        End Property

        ''' <summary>
        ''' Default DNS port
        ''' </summary>
        Public Const DefaultPort As Integer = 53

        ''' <summary>
        ''' Gets list of OPENDNS servers
        ''' </summary>
        Public Shared ReadOnly DefaultDnsServers As IPEndPoint() = {New IPEndPoint(IPAddress.Parse("208.67.222.222"), DefaultPort), New IPEndPoint(IPAddress.Parse("208.67.220.220"), DefaultPort)}

        Private _Unique As UShort
        Private _UseCache As Boolean
        Private _Recursion As Boolean
        Private _Retries As Integer
        Private _Timeout As Integer
        Private _TransportType As TransportType

        Private _DnsServers As List(Of IPEndPoint)

        Private _ResponseCache As Dictionary(Of String, Response)

        ''' <summary>
        ''' Constructor of Resolver using DNS servers specified.
        ''' </summary>
        ''' <param name="DnsServers">Set of DNS servers</param>
        Public Sub New(ByVal DnsServers As IPEndPoint())
            _ResponseCache = New Dictionary(Of String, Response)()
            _DnsServers = New List(Of IPEndPoint)()
            _DnsServers.AddRange(DnsServers)

            _Unique = CUShort(New Random().Next(UShort.MinValue, UShort.MaxValue))
            _Retries = 3
            _Timeout = 1
            _Recursion = True
            _UseCache = True
            _TransportType = TransportType.Udp
        End Sub

        ''' <summary>
        ''' Constructor of Resolver using DNS server specified.
        ''' </summary>
        ''' <param name="DnsServer">DNS server to use</param>
        Public Sub New(ByVal DnsServer As IPEndPoint)
            Me.New(New IPEndPoint() {DnsServer})
        End Sub

        ''' <summary>
        ''' Constructor of Resolver using DNS server and port specified.
        ''' </summary>
        ''' <param name="ServerIpAddress">DNS server to use</param>
        ''' <param name="ServerPortNumber">DNS port to use</param>
        Public Sub New(ByVal ServerIpAddress As IPAddress, ByVal ServerPortNumber As Integer)
            Me.New(New IPEndPoint(ServerIpAddress, ServerPortNumber))
        End Sub

        ''' <summary>
        ''' Constructor of Resolver using DNS address and port specified.
        ''' </summary>
        ''' <param name="ServerIpAddress">DNS server address to use</param>
        ''' <param name="ServerPortNumber">DNS port to use</param>
        Public Sub New(ByVal ServerIpAddress As String, ByVal ServerPortNumber As Integer)
            Me.New(IPAddress.Parse(ServerIpAddress), ServerPortNumber)
        End Sub

        ''' <summary>
        ''' Constructor of Resolver using DNS address.
        ''' </summary>
        ''' <param name="ServerIpAddress">DNS server address to use</param>
        Public Sub New(ByVal ServerIpAddress As String)
            Me.New(IPAddress.Parse(ServerIpAddress), DefaultPort)
        End Sub

        ''' <summary>
        ''' Resolver constructor, using DNS servers specified by Windows
        ''' </summary>
        Public Sub New()
            Me.New(GetDnsServers())
        End Sub

        Public Class VerboseOutputEventArgs
            Inherits EventArgs
            Public Message As String
            Public Sub New(ByVal Message As String)
                Me.Message = Message
            End Sub
        End Class

        Private Sub Verbose(ByVal format As String, ByVal ParamArray args As Object())
            RaiseEvent OnVerbose(Me, New VerboseEventArgs(String.Format(format, args)))
        End Sub

        ''' <summary>
        ''' Verbose messages from internal operations
        ''' </summary>
        Public Event OnVerbose As VerboseEventHandler
        Public Delegate Sub VerboseEventHandler(ByVal sender As Object, ByVal e As VerboseEventArgs)

        Public Class VerboseEventArgs
            Inherits EventArgs
            Public Message As String
            Public Sub New(ByVal message As String)
                Me.Message = message
            End Sub
        End Class


        ''' <summary>
        ''' Gets or sets timeout in milliseconds
        ''' </summary>
        Public Property Timeout() As Integer
            Get
                Return _Timeout
            End Get
            Set(ByVal value As Integer)
                _Timeout = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets number of retries before giving up
        ''' </summary>
        Public Property Retries() As Integer
            Get
                Return _Retries
            End Get
            Set(ByVal value As Integer)
                If value >= 1 Then
                    _Retries = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or set recursion for doing queries
        ''' </summary>
        Public Property Recursion() As Boolean
            Get
                Return _Recursion
            End Get
            Set(ByVal value As Boolean)
                _Recursion = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets protocol to use
        ''' </summary>
        Public Property TransportType() As TransportType
            Get
                Return _TransportType
            End Get
            Set(ByVal value As TransportType)
                _TransportType = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets list of DNS servers to use
        ''' </summary>
        Public Property DnsServers() As IPEndPoint()
            Get
                Return _DnsServers.ToArray()
            End Get
            Set(ByVal value As IPEndPoint())
                _DnsServers.Clear()
                _DnsServers.AddRange(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets first DNS server address or sets single DNS server to use
        ''' </summary>
        Public Property DnsServer() As String
            Get
                Return _DnsServers(0).Address.ToString()
            End Get
            Set(ByVal value As String)
                Dim ip As IPAddress = Nothing
                If IPAddress.TryParse(value, ip) Then
                    _DnsServers.Clear()
                    _DnsServers.Add(New IPEndPoint(ip, DefaultPort))
                    Exit Property
                End If
                Dim response As Response = Query(value, QType.A)
                If response.RecordsA.Length > 0 Then
                    _DnsServers.Clear()
                    _DnsServers.Add(New IPEndPoint(response.RecordsA(0).Address, DefaultPort))
                End If
            End Set
        End Property


        Public Property UseCache() As Boolean
            Get
                Return _UseCache
            End Get
            Set(ByVal value As Boolean)
                _UseCache = value
                If Not _UseCache Then
                    _ResponseCache.Clear()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Clear the resolver cache
        ''' </summary>
        Public Sub ClearCache()
            _ResponseCache.Clear()
        End Sub

        Private Function SearchInCache(ByVal question As Question) As Response
            If Not _UseCache Then
                Return Nothing
            End If

            Dim strKey As String = ((question.QClass & "-") + question.QType & "-") + question.QName

            Dim response As Response = Nothing

            SyncLock _ResponseCache
                If Not _ResponseCache.ContainsKey(strKey) Then
                    Return Nothing
                End If

                response = _ResponseCache(strKey)
            End SyncLock

            Dim TimeLived As Integer = CInt(((DateTime.Now.Ticks - response.TimeStamp.Ticks) / TimeSpan.TicksPerSecond))
            For Each rr As RR In response.RecordsRR
                rr.TimeLived = TimeLived
                ' The TTL property calculates its actual time to live
                If rr.TTL = 0 Then
                    Return Nothing
                    ' out of date
                End If
            Next
            Return response
        End Function

        Private Sub AddToCache(ByVal response As Response)
            If Not _UseCache Then
                Exit Sub
            End If

            ' No question, no caching
            If response.Questions.Count = 0 Then
                Exit Sub
            End If

            ' Only cached non-error responses
            If response.header.RCODE <> RCode.NoError Then
                Exit Sub
            End If

            Dim question As Question = response.Questions(0)

            Dim strKey As String = ((question.QClass & "-") + question.QType & "-") + question.QName

            SyncLock _ResponseCache
                If _ResponseCache.ContainsKey(strKey) Then
                    _ResponseCache.Remove(strKey)
                End If

                _ResponseCache.Add(strKey, response)
            End SyncLock
        End Sub

        Private Function UdpRequest(ByVal request As Request) As Response
            ' RFC1035 max. size of a UDP datagram is 512 bytes
            Dim responseMessage As Byte() = New Byte(511) {}

            For intAttempts As Integer = 0 To _Retries - 1
                For intDnsServer As Integer = 0 To _DnsServers.Count - 1
                    Dim socket As New Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _Timeout * 1000)

                    Try
                        socket.SendTo(request.Data, _DnsServers(intDnsServer))
                        Dim intReceived As Integer = socket.Receive(responseMessage)
                        Dim data As Byte() = New Byte(intReceived - 1) {}
                        Array.Copy(responseMessage, data, intReceived)
                        Dim response As New Response(_DnsServers(intDnsServer), data)
                        AddToCache(response)
                        Return response
                    Catch generatedExceptionName As SocketException
                        Verbose(String.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)))
                        ' next try
                        Continue For
                    Finally
                        _Unique += 1

                        ' close the socket
                        socket.Close()
                    End Try
                Next
            Next
            Dim responseTimeout As New Response()
            responseTimeout.[Error] = "Timeout Error"
            Return responseTimeout
        End Function

        Private Function TcpRequest(ByVal request As Request) As Response
            'System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            'sw.Start();

            Dim responseMessage As Byte() = New Byte(511) {}

            For intAttempts As Integer = 0 To _Retries - 1
                For intDnsServer As Integer = 0 To _DnsServers.Count - 1
                    Dim tcpClient As New TcpClient()
                    tcpClient.ReceiveTimeout = _Timeout * 1000

                    Try
                        Dim result As IAsyncResult = tcpClient.BeginConnect(_DnsServers(intDnsServer).Address, _DnsServers(intDnsServer).Port, Nothing, Nothing)

                        Dim success As Boolean = result.AsyncWaitHandle.WaitOne(_Timeout * 1000, True)

                        If Not success OrElse Not tcpClient.Connected Then
                            tcpClient.Close()
                            Verbose(String.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)))
                            Continue For
                        End If

                        Dim bs As New BufferedStream(tcpClient.GetStream())

                        Dim data As Byte() = request.Data
                        bs.WriteByte(CByte(((data.Length >> 8) And &HFF)))
                        bs.WriteByte(CByte((data.Length And &HFF)))
                        bs.Write(data, 0, data.Length)
                        bs.Flush()

                        Dim TransferResponse As New Response()
                        Dim intSoa As Integer = 0
                        Dim intMessageSize As Integer = 0

                        'Debug.WriteLine("Sending "+ (request.Length+2) + " bytes in "+ sw.ElapsedMilliseconds+" mS");

                        While True
                            Dim intLength As Integer = bs.ReadByte() << 8 Or bs.ReadByte()
                            If intLength <= 0 Then
                                tcpClient.Close()
                                Verbose(String.Format(";; Connection to nameserver {0} failed", (intDnsServer + 1)))
                                ' next try
                                Throw New SocketException()
                            End If

                            intMessageSize += intLength

                            data = New Byte(intLength - 1) {}
                            bs.Read(data, 0, intLength)
                            Dim response As New Response(_DnsServers(intDnsServer), data)

                            'Debug.WriteLine("Received "+ (intLength+2)+" bytes in "+sw.ElapsedMilliseconds +" mS");

                            If response.header.RCODE <> RCode.NoError Then
                                Return response
                            End If

                            If response.Questions(0).QType <> QType.AXFR Then
                                AddToCache(response)
                                Return response
                            End If

                            ' Zone transfer!!

                            If TransferResponse.Questions.Count = 0 Then
                                TransferResponse.Questions.AddRange(response.Questions)
                            End If
                            TransferResponse.Answers.AddRange(response.Answers)
                            TransferResponse.Authorities.AddRange(response.Authorities)
                            TransferResponse.Additionals.AddRange(response.Additionals)

                            If response.Answers(0).Type = Type.SOA Then
                                intSoa += 1
                            End If

                            If intSoa = 2 Then
                                TransferResponse.header.QDCOUNT = CUShort(TransferResponse.Questions.Count)
                                TransferResponse.header.ANCOUNT = CUShort(TransferResponse.Answers.Count)
                                TransferResponse.header.NSCOUNT = CUShort(TransferResponse.Authorities.Count)
                                TransferResponse.header.ARCOUNT = CUShort(TransferResponse.Additionals.Count)
                                TransferResponse.MessageSize = intMessageSize
                                Return TransferResponse
                            End If
                        End While
                    Catch generatedExceptionName As SocketException
                        ' try
                        ' next try
                        Continue For
                    Finally
                        _Unique += 1

                        ' close the socket
                        tcpClient.Close()
                    End Try
                Next
            Next
            Dim responseTimeout As New Response()
            responseTimeout.[Error] = "Timeout Error"
            Return responseTimeout
        End Function

        ''' <summary>
        ''' Do Query on specified DNS servers
        ''' </summary>
        ''' <param name="name">Name to query</param>
        ''' <param name="qtype">Question type</param>
        ''' <param name="qclass">Class type</param>
        ''' <returns>Response of the query</returns>
        Public Function Query(ByVal name As String, ByVal qtype As QType, ByVal qclass As QClass) As Response
            Dim question As New Question(name, qtype, qclass)
            Dim response As Response = SearchInCache(question)
            If response IsNot Nothing Then
                Return response
            End If

            Dim request As New Request()
            request.AddQuestion(question)
            Return GetResponse(request)
        End Function

        ''' <summary>
        ''' Do an QClass=IN Query on specified DNS servers
        ''' </summary>
        ''' <param name="name">Name to query</param>
        ''' <param name="qtype">Question type</param>
        ''' <returns>Response of the query</returns>
        Public Function Query(ByVal name As String, ByVal qtype As QType) As Response
            Dim question As New Question(name, qtype, QClass.[IN])
            Dim response As Response = SearchInCache(question)
            If response IsNot Nothing Then
                Return response
            End If

            Dim request As New Request()
            request.AddQuestion(question)
            Return GetResponse(request)
        End Function

        Private Function GetResponse(ByVal request As Request) As Response
            request.header.ID = _Unique
            request.header.RD = _Recursion

            If _TransportType = TransportType.Udp Then
                Return UdpRequest(request)
            End If

            If _TransportType = TransportType.Tcp Then
                Return TcpRequest(request)
            End If

            Dim response As New Response()
            response.[Error] = "Unknown TransportType"
            Return response
        End Function

        ''' <summary>
        ''' Gets a list of default DNS servers used on the Windows machine.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetDnsServers() As IPEndPoint()
            Dim list As New List(Of IPEndPoint)()

            Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
            For Each n As NetworkInterface In adapters
                If n.OperationalStatus = OperationalStatus.Up Then
                    Dim ipProps As IPInterfaceProperties = n.GetIPProperties()
                    ' thanks to Jon Webster on May 20, 2008
                    For Each ipAddr As IPAddress In ipProps.DnsAddresses
                        Dim entry As New IPEndPoint(ipAddr, DefaultPort)
                        If Not list.Contains(entry) Then
                            list.Add(entry)
                        End If

                    Next
                End If
            Next
            Return list.ToArray()
        End Function


        '

        Private Function MakeEntry(ByVal HostName As String) As IPHostEntry
            Dim entry As New IPHostEntry()

            entry.HostName = HostName

            Dim response As Response = Query(HostName, QType.A, QClass.[IN])

            ' fill AddressList and aliases
            Dim AddressList As New List(Of IPAddress)()
            Dim Aliases As New List(Of String)()
            For Each answerRR As AnswerRR In response.Answers
                If answerRR.Type = Type.A Then
                    ' answerRR.RECORD.ToString() == (answerRR.RECORD as RecordA).Address
                    AddressList.Add(IPAddress.Parse((answerRR.RECORD.ToString())))
                    entry.HostName = answerRR.NAME
                Else
                    If answerRR.Type = Type.CNAME Then
                        Aliases.Add(answerRR.NAME)
                    End If
                End If
            Next
            entry.AddressList = AddressList.ToArray()
            entry.Aliases = Aliases.ToArray()

            Return entry
        End Function

        ''' <summary>
        ''' Translates the IPV4 or IPV6 address into an arpa address
        ''' </summary>
        ''' <param name="ip">IP address to get the arpa address form</param>
        ''' <returns>The 'mirrored' IPV4 or IPV6 arpa address</returns>
        Public Shared Function GetArpaFromIp(ByVal ip As IPAddress) As String
            If ip.AddressFamily = AddressFamily.InterNetwork Then
                Dim sb As New StringBuilder()
                sb.Append("in-addr.arpa.")
                For Each b As Byte In ip.GetAddressBytes()
                    sb.Insert(0, String.Format(Globalization.CultureInfo.InvariantCulture, "{0}.", b))
                Next
                Return sb.ToString()
            End If
            If ip.AddressFamily = AddressFamily.InterNetworkV6 Then
                Dim sb As New StringBuilder()
                sb.Append("ip6.arpa.")
                For Each b As Byte In ip.GetAddressBytes()
                    sb.Insert(0, String.Format(Globalization.CultureInfo.InvariantCulture, "{0:x}.", (b >> 4) And &HF))
                    sb.Insert(0, String.Format(Globalization.CultureInfo.InvariantCulture, "{0:x}.", (b >> 0) And &HF))
                Next
                Return sb.ToString()
            End If
            Return "?"
        End Function

        Public Shared Function GetArpaFromEnum(ByVal strEnum As String) As String
            Dim sb As New StringBuilder()
            Dim Number As String = System.Text.RegularExpressions.Regex.Replace(strEnum, "[^0-9]", "")
            sb.Append("e164.arpa.")
            For Each c As Char In Number
                sb.Insert(0, String.Format("{0}.", c))
            Next
            Return sb.ToString()
        End Function

#Region "Deprecated methods in the original System.Net.DNS class"

        ''' <summary>
        ''' Returns the Internet Protocol (IP) addresses for the specified host.
        ''' </summary>
        ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        ''' <returns>
        ''' An array of type System.Net.IPAddress that holds the IP addresses for the
        ''' host that is specified by the hostNameOrAddress parameter. 
        '''</returns>
        Public Function GetHostAddresses(ByVal hostNameOrAddress As String) As IPAddress()
            Dim entry As IPHostEntry = GetHostEntry(hostNameOrAddress)
            Return entry.AddressList
        End Function

        Private Delegate Function GetHostAddressesDelegate(ByVal hostNameOrAddress As String) As IPAddress()

        ''' <summary>
        ''' Asynchronously returns the Internet Protocol (IP) addresses for the specified
        ''' host.
        ''' </summary>
        ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        ''' <param name="requestCallback">
        ''' An System.AsyncCallback delegate that references the method to invoke when
        ''' the operation is complete.
        ''' </param>
        ''' <param name="stateObject">
        ''' A user-defined object that contains information about the operation. This
        ''' object is passed to the requestCallback delegate when the operation is complete.
        '''</param>
        ''' <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        Public Function BeginGetHostAddresses(ByVal hostNameOrAddress As String, ByVal requestCallback As AsyncCallback, ByVal stateObject As Object) As IAsyncResult
            Dim g As New GetHostAddressesDelegate(AddressOf GetHostAddresses)
            Return g.BeginInvoke(hostNameOrAddress, requestCallback, stateObject)
        End Function

        ''' <summary>
        ''' Ends an asynchronous request for DNS information.
        ''' </summary>
        ''' <param name="AsyncResult">
        ''' An System.IAsyncResult instance returned by a call to the Heijden.Dns.Resolver.BeginGetHostAddresses(System.String,System.AsyncCallback,System.Object)
        ''' method.
        ''' </param>
        ''' <returns></returns>
        Public Function EndGetHostAddresses(ByVal AsyncResult As IAsyncResult) As IPAddress()
            Dim aResult As AsyncResult = DirectCast(AsyncResult, AsyncResult)
            Dim g As GetHostAddressesDelegate = DirectCast(aResult.AsyncDelegate, GetHostAddressesDelegate)
            Return g.EndInvoke(AsyncResult)
        End Function

        ''' <summary>
        ''' Creates an System.Net.IPHostEntry instance from the specified System.Net.IPAddress.
        ''' </summary>
        ''' <param name="ip">An System.Net.IPAddress.</param>
        ''' <returns>An System.Net.IPHostEntry.</returns>
        Public Function GetHostByAddress(ByVal ip As IPAddress) As IPHostEntry
            Return GetHostEntry(ip)
        End Function

        ''' <summary>
        ''' Creates an System.Net.IPHostEntry instance from an IP address.
        ''' </summary>
        ''' <param name="address">An IP address.</param>
        ''' <returns>An System.Net.IPHostEntry instance.</returns>
        Public Function GetHostByAddress(ByVal address As String) As IPHostEntry
            Return GetHostEntry(address)
        End Function

        ''' <summary>
        ''' Gets the DNS information for the specified DNS host name.
        ''' </summary>
        ''' <param name="hostName">The DNS name of the host</param>
        ''' <returns>An System.Net.IPHostEntry object that contains host information for the address specified in hostName.</returns>
        Public Function GetHostByName(ByVal hostName As String) As IPHostEntry
            Return MakeEntry(hostName)
        End Function

        Private Delegate Function GetHostByNameDelegate(ByVal hostName As String) As IPHostEntry

        ''' <summary>
        ''' Asynchronously resolves an IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="hostName">The DNS name of the host</param>
        ''' <param name="requestCallback">An System.AsyncCallback delegate that references the method to invoke when the operation is complete.</param>
        ''' <param name="stateObject">
        ''' A user-defined object that contains information about the operation. This
        ''' object is passed to the requestCallback delegate when the operation is complete.
        ''' </param>
        ''' <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        Public Function BeginGetHostByName(ByVal hostName As String, ByVal requestCallback As AsyncCallback, ByVal stateObject As Object) As IAsyncResult
            Dim g As New GetHostByNameDelegate(AddressOf GetHostByName)
            Return g.BeginInvoke(hostName, requestCallback, stateObject)
        End Function

        ''' <summary>
        ''' Ends an asynchronous request for DNS information.
        ''' </summary>
        ''' <param name="AsyncResult">
        ''' An System.IAsyncResult instance returned by a call to an 
        ''' Heijden.Dns.Resolver.BeginGetHostByName method.
        ''' </param>
        ''' <returns></returns>
        Public Function EndGetHostByName(ByVal AsyncResult As IAsyncResult) As IPHostEntry
            Dim aResult As AsyncResult = DirectCast(AsyncResult, AsyncResult)
            Dim g As GetHostByNameDelegate = DirectCast(aResult.AsyncDelegate, GetHostByNameDelegate)
            Return g.EndInvoke(AsyncResult)
        End Function

        ''' <summary>
        ''' Resolves a host name or IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="hostName">A DNS-style host name or IP address.</param>
        ''' <returns></returns>
        Public Function Resolve(ByVal hostName As String) As IPHostEntry
            Return MakeEntry(hostName)
        End Function

        Private Delegate Function ResolveDelegate(ByVal hostName As String) As IPHostEntry

        ''' <summary>
        ''' Begins an asynchronous request to resolve a DNS host name or IP address to
        ''' an System.Net.IPAddress instance.
        ''' </summary>
        ''' <param name="hostName">The DNS name of the host.</param>
        ''' <param name="requestCallback">
        ''' An System.AsyncCallback delegate that references the method to invoke when
        ''' the operation is complete.
        ''' </param>
        ''' <param name="stateObject">
        ''' A user-defined object that contains information about the operation. This
        ''' object is passed to the requestCallback delegate when the operation is complete.
        ''' </param>
        ''' <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        Public Function BeginResolve(ByVal hostName As String, ByVal requestCallback As AsyncCallback, ByVal stateObject As Object) As IAsyncResult
            Dim g As New ResolveDelegate(AddressOf Resolve)
            Return g.BeginInvoke(hostName, requestCallback, stateObject)
        End Function

        ''' <summary>
        ''' Ends an asynchronous request for DNS information.
        ''' </summary>
        ''' <param name="AsyncResult">
        ''' An System.IAsyncResult instance that is returned by a call to the System.Net.Dns.BeginResolve(System.String,System.AsyncCallback,System.Object)
        ''' method.
        ''' </param>
        ''' <returns>An System.Net.IPHostEntry object that contains DNS information about a host.</returns>
        Public Function EndResolve(ByVal AsyncResult As IAsyncResult) As IPHostEntry
            Dim aResult As AsyncResult = DirectCast(AsyncResult, AsyncResult)
            Dim g As ResolveDelegate = DirectCast(aResult.AsyncDelegate, ResolveDelegate)
            Return g.EndInvoke(AsyncResult)
        End Function
#End Region

        ''' <summary>
        ''' Resolves an IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="ip">An IP address.</param>
        ''' <returns>
        ''' An System.Net.IPHostEntry instance that contains address information about
        ''' the host specified in address.
        '''</returns>
        Public Function GetHostEntry(ByVal ip As IPAddress) As IPHostEntry
            Dim response As Response = Query(GetArpaFromIp(ip), QType.PTR, QClass.[IN])
            If response.RecordsPTR.Length > 0 Then
                Return MakeEntry(response.RecordsPTR(0).PTRDNAME)
            Else
                Return New IPHostEntry()
            End If
        End Function

        ''' <summary>
        ''' Resolves a host name or IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        ''' <returns>
        ''' An System.Net.IPHostEntry instance that contains address information about
        ''' the host specified in hostNameOrAddress. 
        '''</returns>
        Public Function GetHostEntry(ByVal hostNameOrAddress As String) As IPHostEntry
            Dim iPAddress__1 As IPAddress = Nothing
            If IPAddress.TryParse(hostNameOrAddress, iPAddress__1) Then
                Return GetHostEntry(iPAddress__1)
            Else
                Return MakeEntry(hostNameOrAddress)
            End If
        End Function

        Private Delegate Function GetHostEntryViaIPDelegate(ByVal ip As IPAddress) As IPHostEntry
        Private Delegate Function GetHostEntryDelegate(ByVal hostNameOrAddress As String) As IPHostEntry

        ''' <summary>
        ''' Asynchronously resolves a host name or IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        ''' <param name="requestCallback">
        ''' An System.AsyncCallback delegate that references the method to invoke when
        ''' the operation is complete.
        '''</param>
        ''' <param name="stateObject">
        ''' A user-defined object that contains information about the operation. This
        ''' object is passed to the requestCallback delegate when the operation is complete.
        ''' </param>
        ''' <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        Public Function BeginGetHostEntry(ByVal hostNameOrAddress As String, ByVal requestCallback As AsyncCallback, ByVal stateObject As Object) As IAsyncResult
            Dim g As New GetHostEntryDelegate(AddressOf GetHostEntry)
            Return g.BeginInvoke(hostNameOrAddress, requestCallback, stateObject)
        End Function

        ''' <summary>
        ''' Asynchronously resolves an IP address to an System.Net.IPHostEntry instance.
        ''' </summary>
        ''' <param name="ip">The IP address to resolve.</param>
        ''' <param name="requestCallback">
        ''' An System.AsyncCallback delegate that references the method to invoke when
        ''' the operation is complete.
        ''' </param>
        ''' <param name="stateObject">
        ''' A user-defined object that contains information about the operation. This
        ''' object is passed to the requestCallback delegate when the operation is complete.
        ''' </param>
        ''' <returns>An System.IAsyncResult instance that references the asynchronous request.</returns>
        Public Function BeginGetHostEntry(ByVal ip As IPAddress, ByVal requestCallback As AsyncCallback, ByVal stateObject As Object) As IAsyncResult
            Dim g As New GetHostEntryViaIPDelegate(AddressOf GetHostEntry)
            Return g.BeginInvoke(ip, requestCallback, stateObject)
        End Function

        ''' <summary>
        ''' Ends an asynchronous request for DNS information.
        ''' </summary>
        ''' <param name="AsyncResult">
        ''' An System.IAsyncResult instance returned by a call to an 
        ''' Overload:Heijden.Dns.Resolver.BeginGetHostEntry method.
        ''' </param>
        ''' <returns>
        ''' An System.Net.IPHostEntry instance that contains address information about
        ''' the host. 
        '''</returns>
        Public Function EndGetHostEntry(ByVal AsyncResult As IAsyncResult) As IPHostEntry
            Dim aResult As AsyncResult = DirectCast(AsyncResult, AsyncResult)
            If TypeOf aResult.AsyncDelegate Is GetHostEntryDelegate Then
                Dim g As GetHostEntryDelegate = DirectCast(aResult.AsyncDelegate, GetHostEntryDelegate)
                Return g.EndInvoke(AsyncResult)
            End If
            If TypeOf aResult.AsyncDelegate Is GetHostEntryViaIPDelegate Then
                Dim g As GetHostEntryViaIPDelegate = DirectCast(aResult.AsyncDelegate, GetHostEntryViaIPDelegate)
                Return g.EndInvoke(AsyncResult)
            End If
            Return Nothing
        End Function

        Private Enum RRRecordStatus
            UNKNOWN
            NAME
            TTL
            [CLASS]
            TYPE
            VALUE
        End Enum

        Public Shared Sub LoadRootFile(ByVal path As String)
            Dim sr As New StreamReader(path)
            While Not sr.EndOfStream
                Dim strLine As String = sr.ReadLine()
                If strLine Is Nothing Then
                    Exit While
                End If
                Dim intI As Integer = strLine.IndexOf(";"c)
                If intI >= 0 Then
                    strLine = strLine.Substring(0, intI)
                End If
                strLine = strLine.Trim()
                If strLine.Length = 0 Then
                    Continue While
                End If
                Dim status As RRRecordStatus = RRRecordStatus.NAME
                Dim Name As String = ""
                Dim Ttl As String = ""
                Dim [Class] As String = ""
                Dim Type As String = ""
                Dim Value As String = ""
                Dim W As String = ""
                For intI = 0 To strLine.Length - 1
                    Dim C As Char = strLine(intI)

                    If C <= " "c AndAlso W <> "" Then
                        Select Case status
                            Case RRRecordStatus.NAME
                                Name = W
                                status = RRRecordStatus.TTL
                                Exit Select
                            Case RRRecordStatus.TTL
                                Ttl = W
                                status = RRRecordStatus.[CLASS]
                                Exit Select
                            Case RRRecordStatus.[CLASS]
                                [Class] = W
                                status = RRRecordStatus.TYPE
                                Exit Select
                            Case RRRecordStatus.TYPE
                                Type = W
                                status = RRRecordStatus.VALUE
                                Exit Select
                            Case RRRecordStatus.VALUE
                                Value = W
                                status = RRRecordStatus.UNKNOWN
                                Exit Select
                            Case Else
                                Exit Select
                        End Select
                        W = ""
                    End If
                    If C > " "c Then
                        W += C
                    End If

                Next
            End While
            sr.Close()
        End Sub
    End Class
    ' class
End Namespace
