Imports System
'
' * 
'3.3.1. CNAME RDATA format
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / CNAME /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'CNAME A <domain-name> which specifies the canonical or primary
' name for the owner. The owner name is an alias.
'
'CNAME RRs cause no additional section processing, but name servers may
'choose to restart the query at the canonical name in certain cases. See
'the description of name server logic in [RFC-1034] for details.
'
' * 
' 

Namespace Dns
    Public Class RecordCNAME
        Inherits Record
        Public CNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Return BitHelpers.WriteName(CNAME)
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            CNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return CNAME
        End Function

    End Class
End Namespace
