Imports System
Imports System.Text
'
' * http://tools.ietf.org/rfc/rfc3658.txt
' * 
'2.4. Wire Format of the DS record
'
' The DS (type=43) record contains these fields: key tag, algorithm,
' digest type, and the digest of a public key KEY record that is
' allowed and/or used to sign the child's apex KEY RRset. Other keys
' MAY sign the child's apex KEY RRset.
'
' 1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | key tag | algorithm | Digest type |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | digest (length depends on type) |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | (SHA-1 digest is 20 bytes) |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
' | |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-|
' | |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-|
' | |
' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
'
' 


Namespace Dns
    Public Class RecordDS
        Inherits Record
        Public KEYTAG As UInt16
        Public ALGORITHM As Byte
        Public DIGESTTYPE As Byte
        Public DIGEST As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            Dim length As UShort = rr.ReadUInt16(-2)
            KEYTAG = rr.ReadUInt16()
            ALGORITHM = rr.ReadByte()
            DIGESTTYPE = rr.ReadByte()
            length -= 4
            DIGEST = New Byte(length - 1) {}
            DIGEST = rr.ReadBytes(length)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Dim sb As New StringBuilder()
            For intI As Integer = 0 To DIGEST.Length - 1
                sb.AppendFormat("{0:x2}", DIGEST(intI))
            Next
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0} {1} {2} {3}", KEYTAG, ALGORITHM, DIGESTTYPE, sb.ToString())
        End Function

    End Class
End Namespace