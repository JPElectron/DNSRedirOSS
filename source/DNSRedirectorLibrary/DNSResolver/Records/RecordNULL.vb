Imports System
'
'3.3.10. NULL RDATA format (EXPERIMENTAL)
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / <anything> /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'Anything at all may be in the RDATA field so long as it is 65535 octets
'or less.
'
'NULL records cause no additional section processing. NULL RRs are not
'allowed in master files. NULLs are used as placeholders in some
'experimental extensions of the DNS.
'

Namespace Dns

    Public Class RecordNULL
        Inherits Record
        Public ANYTHING As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)
                bytes.AddRange(BitHelpers.WriteShort(ANYTHING.Length))
                bytes.AddRange(ANYTHING)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            rr.Position -= 2
            ' re-read length
            Dim RDLENGTH As UShort = rr.ReadUInt16()
            ANYTHING = New Byte(RDLENGTH - 1) {}
            ANYTHING = rr.ReadBytes(RDLENGTH)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("...binary data... ({0}) bytes", ANYTHING.Length)
        End Function

    End Class
End Namespace