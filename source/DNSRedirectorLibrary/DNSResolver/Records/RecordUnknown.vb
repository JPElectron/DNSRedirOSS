Imports System

Namespace Dns

    Public Class RecordUnknown
        Inherits Record
        Public RDATA As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)
                If RDATA.Length > 0 Then
                    bytes.AddRange(BitHelpers.WriteShort(RDATA.Length - 1))
                Else
                    bytes.AddRange(BitHelpers.WriteShort(0))
                End If

                bytes.AddRange(RDATA)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            ' re-read length
            Dim RDLENGTH As UShort = rr.ReadUInt16(-2)
            RDATA = rr.ReadBytes(RDLENGTH)
        End Sub
    End Class
End Namespace