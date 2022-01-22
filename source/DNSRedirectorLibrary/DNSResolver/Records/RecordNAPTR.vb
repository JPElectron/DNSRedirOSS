Imports System
'
' * http://www.faqs.org/rfcs/rfc2915.html
' * 
' 8. DNS Packet Format
'
' The packet format for the NAPTR record is:
'
' 1 1 1 1 1 1
' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' | ORDER |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' | PREFERENCE |
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / FLAGS /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / SERVICES /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / REGEXP /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / REPLACEMENT /
' / /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
' where:
'
' FLAGS A <character-string> which contains various flags.
'
' SERVICES A <character-string> which contains protocol and service
' identifiers.
'
' REGEXP A <character-string> which contains a regular expression.
'
' REPLACEMENT A <domain-name> which specifies the new value in the
' case where the regular expression is a simple replacement
' operation.
'
' <character-string> and <domain-name> as used here are defined in
' RFC1035 [1].
'
' 


Namespace Dns

    Public Class RecordNAPTR
        Inherits Record
        Public ORDER As UShort
        Public PREFERENCE As UShort
        Public FLAGS As String
        Public SERVICES As String
        Public REGEXP As String
        Public REPLACEMENT As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            ORDER = rr.ReadUInt16()
            PREFERENCE = rr.ReadUInt16()
            FLAGS = rr.ReadString()
            SERVICES = rr.ReadString()
            REGEXP = rr.ReadString()
            REPLACEMENT = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0} {1} ""{2}"" ""{3}"" ""{4}"" {5}", ORDER, PREFERENCE, FLAGS, SERVICES, REGEXP, _
            REPLACEMENT)
        End Function

    End Class
End Namespace