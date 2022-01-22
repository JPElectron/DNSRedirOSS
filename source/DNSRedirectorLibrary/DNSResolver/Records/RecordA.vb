Imports System
'
' 3.4.1. A RDATA format
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' | ADDRESS |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'ADDRESS A 32 bit Internet address.
'
'Hosts that have multiple Internet addresses will have multiple A
'records.
' * 
' 

Namespace Dns
    Public Class RecordA

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
            If Not System.Net.IPAddress.TryParse(String.Format(Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", rr.ReadByte(), rr.ReadByte(), rr.ReadByte(), rr.ReadByte()), Me.Address) Then
                Me.Address = System.Net.IPAddress.None
            End If
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return Address.ToString()
        End Function

    End Class
End Namespace