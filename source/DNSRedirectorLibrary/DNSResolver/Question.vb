Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Net
Imports System.Text

Namespace Dns

#Region "Rfc 1034/1035"
    '
    ' 4.1.2. Question section format
    '
    ' The question section is used to carry the "question" in most queries,
    ' i.e., the parameters that define what is being asked. The section
    ' contains QDCOUNT (usually 1) entries, each of the following format:
    '
    ' 1 1 1 1 1 1
    ' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | |
    ' / QNAME /
    ' / /
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | QTYPE |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | QCLASS |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    '
    ' where:
    '
    ' QNAME a domain name represented as a sequence of labels, where
    ' each label consists of a length octet followed by that
    ' number of octets. The domain name terminates with the
    ' zero length octet for the null label of the root. Note
    ' that this field may be an odd number of octets; no
    ' padding is used.
    '
    ' QTYPE a two octet code which specifies the type of the query.
    ' The values for this field include all codes valid for a
    ' TYPE field, together with some more general codes which
    ' can match more than one type of RR.
    '
    '
    ' QCLASS a two octet code that specifies the class of the query.
    ' For example, the QCLASS field is IN for the Internet.
    ' 

#End Region

    Public Class Question
        Private _QName As String
        Public Property QName() As String
            Get
                Return _QName
            End Get
            Set(ByVal value As String)
                _QName = value
                If Not _QName.EndsWith(".") Then
                    _QName += "."
                End If
            End Set
        End Property
        Public QType As QType
        Public QClass As QClass

        Public Sub New(ByVal QName As String, ByVal QType As QType, ByVal QClass As QClass)
            Me.QName = QName
            Me.QType = QType
            Me.QClass = QClass
        End Sub

        Public Sub New(ByVal rr As RecordReader)
            QName = rr.ReadDomainName()
            QType = DirectCast(rr.ReadUInt16(), QType)
            QClass = DirectCast(rr.ReadUInt16(), QClass)
        End Sub



        Public ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)()
                bytes.AddRange(BitHelpers.WriteName(QName))
                bytes.AddRange(BitHelpers.WriteShort(CUShort(QType)))
                bytes.AddRange(BitHelpers.WriteShort(CUShort(QClass)))
                Return bytes.ToArray()
            End Get
        End Property




        Public Overloads Overrides Function ToString() As String
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0,-32}" & vbTab & "{1}" & vbTab & "{2}", QName, QClass, QType)
        End Function
    End Class
End Namespace