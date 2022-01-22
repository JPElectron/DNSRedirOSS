Imports System

#Region "Rfc info"
'
' * http://www.ietf.org/rfc/rfc2535.txt
' * 4.1 SIG RDATA Format
'
' The RDATA portion of a SIG RR is as shown below. The integrity of
' the RDATA information is protected by the signature field.
'
' 1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | type covered | algorithm | labels |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | original TTL |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | signature expiration |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | signature inception |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | key tag | |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ signer's name +
' | /
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-/
' / /
' / signature /
' / /
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
'
'
'

#End Region

Namespace Dns

    Public Class RecordSIG
        Inherits Record
        Public TYPECOVERED As UInt16
        Public ALGORITHM As Byte
        Public LABELS As Byte
        Public ORIGINALTTL As UInt32
        Public SIGNATUREEXPIRATION As UInt32
        Public SIGNATUREINCEPTION As UInt32
        Public KEYTAG As UInt16
        Public SIGNERSNAME As String
        Public SIGNATURE As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            TYPECOVERED = rr.ReadUInt16()
            ALGORITHM = rr.ReadByte()
            LABELS = rr.ReadByte()
            ORIGINALTTL = rr.ReadUInt32()
            SIGNATUREEXPIRATION = rr.ReadUInt32()
            SIGNATUREINCEPTION = rr.ReadUInt32()
            KEYTAG = rr.ReadUInt16()
            SIGNERSNAME = rr.ReadDomainName()
            SIGNATURE = rr.ReadString()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0} {1} {2} {3} {4} {5} {6} {7} ""{8}""", TYPECOVERED, ALGORITHM, LABELS, ORIGINALTTL, SIGNATUREEXPIRATION, _
            SIGNATUREINCEPTION, KEYTAG, SIGNERSNAME, SIGNATURE)
        End Function

    End Class
End Namespace