Imports System

#Region "Rfc info"
'
'2.2 AAAA data format
'
' A 128 bit IPv6 address is encoded in the data portion of an AAAA
' resource record in network byte order (high-order byte first).
' 

#End Region

Namespace Dns
    Public Class RecordAAAA
        Inherits Record
        Public Address As System.Net.IPAddress

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Return Address.GetAddressBytes
            End Get
        End Property

        Public Sub New(ByVal IP As System.Net.IPAddress)
            Address = IP
        End Sub

        Public Sub New(ByVal rr As RecordReader)
            System.Net.IPAddress.TryParse(String.Format(Globalization.CultureInfo.InvariantCulture, "{0:x}:{1:x}:{2:x}:{3:x}:{4:x}:{5:x}:{6:x}:{7:x}", rr.ReadUInt16(), rr.ReadUInt16(), rr.ReadUInt16(), rr.ReadUInt16(), rr.ReadUInt16(), _
            rr.ReadUInt16(), rr.ReadUInt16(), rr.ReadUInt16()), Me.Address)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return Address.ToString()
        End Function

    End Class
End Namespace

