Imports System
'
'3.3.6. MG RDATA format (EXPERIMENTAL)
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / MGMNAME /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'MGMNAME A <domain-name> which specifies a mailbox which is a
' member of the mail group specified by the domain name.
'
'MG records cause no additional section processing.
'

Namespace Dns

    Public Class RecordMG
        Inherits Record
        Public MGMNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            MGMNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return MGMNAME
        End Function

    End Class
End Namespace