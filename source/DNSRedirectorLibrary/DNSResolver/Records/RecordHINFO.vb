Imports System

'
' 3.3.2. HINFO RDATA format
'
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / CPU /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
' / OS /
' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
'
'where:
'
'CPU A <character-string> which specifies the CPU type.
'
'OS A <character-string> which specifies the operating
' system type.
'
'Standard values for CPU and OS can be found in [RFC-1010].
'
'HINFO records are used to acquire general information about a host. The
'main use is for protocols such as FTP that can use special procedures
'when talking between machines or operating systems of the same type.
' 


Namespace Dns

    Public Class RecordHINFO
        Inherits Record
        Public CPU As String
        Public OS As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim StringBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(CPU)
                Dim bytes As New List(Of Byte)
                'bytes.AddRange(BitHelpers.WriteShort(StringBytes.Length))
                bytes.Add(Convert.ToByte(StringBytes.Length))
                bytes.AddRange(StringBytes)

                StringBytes = System.Text.Encoding.ASCII.GetBytes(OS)
                'bytes.AddRange(BitHelpers.WriteShort(StringBytes.Length))
                bytes.Add(Convert.ToByte(StringBytes.Length))
                bytes.AddRange(StringBytes)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            CPU = rr.ReadString()
            OS = rr.ReadString()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "CPU={0} OS={1}", CPU, OS)
        End Function

    End Class
End Namespace
