Imports System
'
' * http://tools.ietf.org/rfc/rfc2672.txt
' * 
'3. The DNAME Resource Record
'
' The DNAME RR has mnemonic DNAME and type code 39 (decimal).
' DNAME has the following format:
'
' <owner> <ttl> <class> DNAME <target>
'
' The format is not class-sensitive. All fields are required. The
' RDATA field <target> is a <domain-name> [DNSIS].
'
' * 
' 

Namespace Dns

    Public Class RecordDNAME
        Inherits Record
        Public TARGET As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            TARGET = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return TARGET
        End Function

    End Class
End Namespace