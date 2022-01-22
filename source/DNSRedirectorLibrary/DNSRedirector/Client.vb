Imports System.Net
Imports System.Threading
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Windows.Forms
Imports System.Reflection
Imports PTSoft.Collections

Namespace DnsRedirector

    ''' <summary>
    ''' A client that is using the DNS Server
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Client
        Implements INotifyPropertyChanged, IDisposable
        'INotifyPropertyChanged lets use raise an event that bound controls will automatically subscribe to so they can update themselves when data changes

        Private _IP As SortableIP
        Private _HostName As String
        Private _LastAccess As DateTime
        Private _Timeout As Integer
        Private _TimeoutTimer As System.Threading.Timer
        Private _Authorized As Boolean
        Friend _LastVerifiedAuthorization As DateTime
        Private _Blocking As Boolean
        Private _Redirected As Integer
        Private _Blocked As Integer

        ''' <summary>
        ''' Is the client authorized.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Authorized() As Boolean
            Get
                Return _Authorized
            End Get
            Set(ByVal value As Boolean)
                _Authorized = value
                _LastVerifiedAuthorization = Now

                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Authorized"))
            End Set
        End Property

        ''' <summary>
        ''' Are requests from the client blocked
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Blocking() As Boolean
            Get
                Return _Blocking
            End Get
            Set(ByVal value As Boolean)
                _Blocking = value
                If Not _Blocking Then Authorized = True
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Blocking"))
            End Set
        End Property

        ''' <summary>
        ''' The number of requests redirected to the RedirectIP setting
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Redirected() As Integer
            Get
                Return _Redirected
            End Get
        End Property

        ''' <summary>
        ''' The number of requests that have been blocked
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Blocked() As Integer
            Get
                Return _Blocked
            End Get
        End Property

        ''' <summary>
        ''' The IP address of the client
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IP() As SortableIP
            Get
                Return _IP
            End Get
        End Property

        ''' <summary>
        ''' The last time this client sent a request to the server
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property LastAccess() As DateTime
            Get
                Return _LastAccess
            End Get
            Set(ByVal value As DateTime)
                _LastAccess = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LastAccess"))
            End Set
        End Property

        ''' <summary>
        ''' The host name the computer has registered on the network
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New(ByVal IP As IPAddress)
            _IP = New SortableIP(IP)
            LastAccess = Now
        End Sub

        ''' <summary>
        ''' Start a timer for the client
        ''' </summary>
        ''' <param name="timeout">Time in milliseconds to call the callback</param>
        ''' <param name="elapsedCallback">The TimerCallback method to call when the timeout has elapsed</param>
        ''' <remarks></remarks>
        Public Sub SetTimeout(ByVal timeout As Integer, ByVal elapsedCallback As TimerCallback)
            If timeout > 0 Then
                _Timeout = timeout
                _TimeoutTimer = New System.Threading.Timer(elapsedCallback, Me, timeout, Threading.Timeout.Infinite)
            End If
        End Sub

        ''' <summary>
        ''' Reset the timeout
        ''' </summary>
        ''' <remarks>Should only be called after SetTimeout</remarks>
        Public Sub UpdateTimeout()
            If _TimeoutTimer IsNot Nothing Then
                SyncLock _TimeoutTimer 'Ensure multiple threads aren't updating this
                    _TimeoutTimer.Change(_Timeout, -1)
                End SyncLock
            End If
        End Sub

        ''' <summary>
        ''' Thread safe call to add 1 to the Redirected count
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub IncrementRedirected()
            System.Threading.Interlocked.Increment(_Redirected)
            'Easy way to do this thready safley
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Redirected"))
        End Sub

        ''' <summary>
        ''' Thread safe call to add 1 to the Blocked count
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub IncrementBlocked()
            System.Threading.Interlocked.Increment(_Blocked)
            'Easy way to do this thready safley
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Blocked"))
        End Sub



#Region " IDisposable Support "

        Private disposedValue As Boolean        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    'Stop the timeout timer
                    _TimeoutTimer.Dispose()
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#Region " INotifyPropertyChanged Support "
        Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
#End Region

    End Class


    ''' <summary>
    ''' Collection of currently active clients
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ClientsCollection
        Inherits SortableBindingList(Of Client)

        Private _Dict As New Dictionary(Of IPAddress, Client)

        'Public UI As Form

        Public Function TryGetValue(ByVal ip As IPAddress, ByRef client As Client)
            Return _Dict.TryGetValue(ip, client)
        End Function

        Public Shadows Sub Add(ByVal client As Client)
            _Dict.Add(client.IP, client)
            MyBase.Add(client)
        End Sub

        Public Shadows Sub Remove(ByVal client As Client)
            _Dict.Remove(client.IP)
            MyBase.Remove(client)
        End Sub

        Protected Overrides Sub ClearItems()
            MyBase.ClearItems()
            _Dict.Clear()
        End Sub

        'Protected Overrides Sub OnListChanged(ByVal e As System.ComponentModel.ListChangedEventArgs)
        '    'MyBase.OnListChanged(e)
        '    'If UI IsNot Nothing Then
        '    '    If UI.InvokeRequired Then
        '    '        UI.Invoke(New RaiseChangedEvent(AddressOf MyBase.OnListChanged), e)
        '    '    Else
        '    '        MyBase.OnListChanged(e)
        '    '    End If
        '    'End If

        'End Sub

        Public Delegate Sub RaiseChangedEvent(ByVal e As System.ComponentModel.ListChangedEventArgs)

    End Class

    ''' <summary>
    ''' An IPAddress that implements IComparable so it can be sorted
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SortableIP
        Inherits IPAddress
        Implements IComparable, IComparable(Of IPAddress)

        Public Sub New(ByVal IP As IPAddress)
            MyBase.New(IP.GetAddressBytes)
        End Sub

        Public Function CompareTo(ByVal obj As Object) As Integer Implements System.IComparable.CompareTo
            Dim other As IPAddress = TryCast(obj, IPAddress)
            If other IsNot Nothing Then Return CompareTo(other)
            Return -1
        End Function

        Public Function CompareTo(ByVal other As System.Net.IPAddress) As Integer Implements System.IComparable(Of System.Net.IPAddress).CompareTo
            Return Me.Address.CompareTo(other.Address)
        End Function
    End Class

End Namespace



