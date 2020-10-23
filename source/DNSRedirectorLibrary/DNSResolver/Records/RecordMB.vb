Imports System
'
'3.3.3. MB RDATA format (EXPERIMENTAL)
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / MADNAME /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'MADNAME A <domain-name> which specifies a host which has the
' specified mailbox.
'
'MB records cause additional section processing which looks up an A type
'RRs corresponding to MADNAME.
'

Namespace Dns

    Public Class RecordMB
        Inherits Record
        Public MADNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            MADNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return MADNAME
        End Function

    End Class
End Namespace