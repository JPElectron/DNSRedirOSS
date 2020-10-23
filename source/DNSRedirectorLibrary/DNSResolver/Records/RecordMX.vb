Imports System

Namespace Dns
    '
    ' 3.3.9. MX RDATA format
    '
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | PREFERENCE |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' / EXCHANGE /
    ' / /
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    '
    ' where:
    '
    ' PREFERENCE A 16 bit integer which specifies the preference given to
    ' this RR among others at the same owner. Lower values
    ' are preferred.
    '
    ' EXCHANGE A <domain-name> which specifies a host willing to act as
    ' a mail exchange for the owner name.
    '
    ' MX records cause type A additional section processing for the host
    ' specified by EXCHANGE. The use of MX RRs is explained in detail in
    ' [RFC-974].
    ' 


    Public Class RecordMX
        Inherits Record
        Implements IComparable
        Public PREFERENCE As UShort
        Public EXCHANGE As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)
                bytes.AddRange(BitHelpers.WriteShort(PREFERENCE))
                bytes.AddRange(BitHelpers.WriteName(EXCHANGE))
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            PREFERENCE = rr.ReadUInt16()
            EXCHANGE = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0} {1}", PREFERENCE, EXCHANGE)
        End Function

        Public Function CompareTo(ByVal objA As Object) As Integer Implements IComparable.CompareTo
            Dim recordMX As RecordMX = TryCast(objA, RecordMX)
            If recordMX Is Nothing Then
                Return -1
            ElseIf Me.PREFERENCE > recordMX.PREFERENCE Then
                Return 1
            ElseIf Me.PREFERENCE < recordMX.PREFERENCE Then
                Return -1
            Else
                ' they are the same, now compare case insensitive names
                Return String.Compare(Me.EXCHANGE, recordMX.EXCHANGE, True, Globalization.CultureInfo.InvariantCulture)
            End If
        End Function

    End Class
End Namespace