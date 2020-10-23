Imports System

#Region "Rfc info"
'
'3.3.14. TXT RDATA format
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / TXT-DATA /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'TXT-DATA One or more <character-string>s.
'
'TXT RRs are used to hold descriptive text. The semantics of the text
'depends on the domain where it is found.
' * 
'

#End Region

Namespace Dns

    Public Class RecordTXT
        Inherits Record
        Public TXT As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim StringBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(TXT)
                Dim bytes As New List(Of Byte)
                'bytes.AddRange(BitHelpers.WriteShort(StringBytes.Length))
                bytes.Add(Convert.ToByte(StringBytes.Length))
                bytes.AddRange(StringBytes)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            TXT = rr.ReadString()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("""{0}""", TXT)
        End Function

    End Class
End Namespace
