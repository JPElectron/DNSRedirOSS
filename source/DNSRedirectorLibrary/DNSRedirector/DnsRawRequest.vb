Imports System.Net
Imports System.Net.Sockets
Imports PTSoft.Dns

Namespace DnsRedirector

    ''' <summary>
    ''' Data recieved on a client request for a DNS lookup
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure DnsRawRequest
        Public ClientEndpoint As IPEndPoint
        Public Data() As Byte
        Public Socket As UdpClient
        Public Verification As Dictionary(Of Question, VerificationType)
        Private _Request As Dns.Request

        ''' <summary>
        ''' A parsed object representing the data in the raw DNS request
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Request() As Request
            Get
                If _Request Is Nothing Then
                    _Request = New Request(Data)
                End If
                Return _Request
            End Get
        End Property

        Public Sub New(ByVal requestData() As Byte, ByVal client As IPEndPoint, ByVal network As UdpClient)
            Data = requestData
            ClientEndpoint = client
            Socket = network
            Verification = New Dictionary(Of Question, VerificationType)
        End Sub
    End Structure

End Namespace
