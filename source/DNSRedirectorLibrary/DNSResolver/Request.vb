Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Collections.ObjectModel

Namespace Dns

    Public Class Request

        Public header As Header

        Private _Questions As List(Of Question)

        Public ReadOnly Property Questions() As List(Of Question)
            Get
                Return _Questions
            End Get
        End Property

        Public Additionals As New List(Of AdditionalRR)

        Public ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)()
                header.QDCOUNT = CUShort(_Questions.Count)
                bytes.AddRange(header.GetData)
                For Each q As Question In _Questions
                    bytes.AddRange(q.Data)
                Next

                header.ARCOUNT = CUShort(Additionals.Count)
                For Each a As AdditionalRR In Additionals
                    bytes.AddRange(a.Data)
                Next

                Return bytes.ToArray()
            End Get
        End Property

        Public Sub New()
            header = New Header()
            header.OPCODE = OPCode.Query
            header.QDCOUNT = 0

            _Questions = New List(Of Question)
        End Sub

        Public Sub New(ByVal bytes() As Byte)
            Try
                Dim Reader As New RecordReader(bytes)
                header = New Header(Reader)
                _Questions = New List(Of Question)(header.QDCOUNT)
                For i As Short = 0 To header.QDCOUNT - 1
                    _Questions.Add(New Question(Reader))
                Next

                For i As Short = 0 To header.ARCOUNT - 1
                    Additionals.Add(New AdditionalRR(Reader))
                Next
            Catch ex As Exception
                'Dont do anything, the request was malformed so work with what could be parsed
            End Try
        End Sub

        Public Overridable Sub AddQuestion(ByVal question As Question)
            _Questions.Add(question)
        End Sub


    End Class
End Namespace
