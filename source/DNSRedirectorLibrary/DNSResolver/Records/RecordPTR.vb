Imports System
'
' 3.3.12. PTR RDATA format
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / PTRDNAME /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'PTRDNAME A <domain-name> which points to some location in the
' domain name space.
'
'PTR records cause no additional section processing. These RRs are used
'in special domains to point to some other location in the domain space.
'These records are simple data, and don't imply any special processing
'similar to that performed by CNAME, which identifies aliases. See the
'description of the IN-ADDR.ARPA domain for an example.
' 


Namespace Dns

    Public Class RecordPTR
        Inherits Record
        Public PTRDNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Return BitHelpers.WriteName(PTRDNAME)
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            PTRDNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return PTRDNAME
        End Function

    End Class
End Namespace
