Imports System
'
'3.3.8. MR RDATA format (EXPERIMENTAL)
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / NEWNAME /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'NEWNAME A <domain-name> which specifies a mailbox which is the
' proper rename of the specified mailbox.
'
'MR records cause no additional section processing. The main use for MR
'is as a forwarding entry for a user who has moved to a different
'mailbox.
'

Namespace Dns

    Public Class RecordMR
        Inherits Record
        Public NEWNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            NEWNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return NEWNAME
        End Function

    End Class
End Namespace
