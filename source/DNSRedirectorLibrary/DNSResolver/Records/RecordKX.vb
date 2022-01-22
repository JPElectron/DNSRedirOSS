Imports System
'
' * http://tools.ietf.org/rfc/rfc2230.txt
' * 
' * 3.1 KX RDATA format
'
' The KX DNS record has the following RDATA format:
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' | PREFERENCE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / EXCHANGER /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
' where:
'
' PREFERENCE A 16 bit non-negative integer which specifies the
' preference given to this RR among other KX records
' at the same owner. Lower values are preferred.
'
' EXCHANGER A <domain-name> which specifies a host willing to
' act as a mail exchange for the owner name.
'
' KX records MUST cause type A additional section processing for the
' host specified by EXCHANGER. In the event that the host processing
' the DNS transaction supports IPv6, KX records MUST also cause type
' AAAA additional section processing.
'
' The KX RDATA field MUST NOT be compressed.
'
' 

Namespace Dns

    Public Class RecordKX
        Inherits Record
        Implements IComparable

        Public PREFERENCE As UShort
        Public EXCHANGER As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            PREFERENCE = rr.ReadUInt16()
            EXCHANGER = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0} {1}", PREFERENCE, EXCHANGER)
        End Function

        Public Function CompareTo(ByVal objA As Object) As Integer Implements IComparable.CompareTo
            Dim recordKX As RecordKX = TryCast(objA, RecordKX)
            If recordKX Is Nothing Then
                Return -1
            ElseIf Me.PREFERENCE > recordKX.PREFERENCE Then
                Return 1
            ElseIf Me.PREFERENCE < recordKX.PREFERENCE Then
                Return -1
            Else
                ' they are the same, now compare case insensitive names
                Return String.Compare(Me.EXCHANGER, recordKX.EXCHANGER, True, Globalization.CultureInfo.InvariantCulture)
            End If
        End Function

    End Class
End Namespace
