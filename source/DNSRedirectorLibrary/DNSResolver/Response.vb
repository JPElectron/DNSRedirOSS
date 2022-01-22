Imports System
Imports System.IO
Imports System.Net
Imports System.Collections
Imports System.Collections.Generic
Imports System.Text

Namespace Dns
    Public Class Response
        ''' <summary>
        ''' List of Question records
        ''' </summary>
        Public Questions As New List(Of Question)
        ''' <summary>
        ''' List of AnswerRR records
        ''' </summary>
        Public Answers As New List(Of AnswerRR)
        ''' <summary>
        ''' List of AuthorityRR records
        ''' </summary>
        Public Authorities As New List(Of AuthorityRR)
        ''' <summary>
        ''' List of AdditionalRR records
        ''' </summary>
        Public Additionals As New List(Of AdditionalRR)

        Public header As Header

        ''' <summary>
        ''' Error message, empty when no error
        ''' </summary>
        Public [Error] As String

        ''' <summary>
        ''' The Size of the message
        ''' </summary>
        Public MessageSize As Integer

        ''' <summary>
        ''' TimeStamp when cached
        ''' </summary>
        Public TimeStamp As DateTime

        ''' <summary>
        ''' Server which delivered this response
        ''' </summary>
        Public Server As IPEndPoint

        Public Sub New()
            Server = New IPEndPoint(0, 0)
            [Error] = ""
            MessageSize = 0
            TimeStamp = DateTime.Now
            header = New Header()
            header.QR = True
        End Sub

        Public Sub New(ByVal aHeader As Header)
            Server = New IPEndPoint(0, 0)
            [Error] = ""
            MessageSize = 0
            TimeStamp = DateTime.Now
            header = aHeader
            header.QR = True
            header.QDCOUNT = 0
            header.ANCOUNT = 0
            header.ARCOUNT = 0
            header.NSCOUNT = 0
        End Sub

        Public Sub New(ByVal iPEndPoint As IPEndPoint, ByVal data As Byte())
            Try
                [Error] = ""
                Server = iPEndPoint
                TimeStamp = DateTime.Now
                MessageSize = data.Length
                Dim rr As New RecordReader(data)

                header = New Header(rr)

                For intI As Integer = 0 To header.QDCOUNT - 1
                    Questions.Add(New Question(rr))
                Next

                For intI As Integer = 0 To header.ANCOUNT - 1
                    Answers.Add(New AnswerRR(rr))
                Next

                For intI As Integer = 0 To header.NSCOUNT - 1
                    Authorities.Add(New AuthorityRR(rr))
                Next
                For intI As Integer = 0 To header.ARCOUNT - 1
                    Additionals.Add(New AdditionalRR(rr))
                Next
            Catch ex As Exception
                'Dont do anything, the response was malformed so work with what could be parsed
            End Try
        End Sub

        Public Sub AddQuestion(ByVal question As Question)
            Questions.Add(question)
            header.QDCOUNT += 1
        End Sub

        Public Sub AddAnswer(ByVal answer As AnswerRR)
            Answers.Add(answer)
            header.ANCOUNT += 1
        End Sub

        Public Sub AddAnswer(ByVal answer As IEnumerable(Of AnswerRR))
            For Each An As AnswerRR In answer
                Answers.Add(An)
                header.ANCOUNT += 1
            Next
        End Sub

        Public Sub RemoveAnswer(ByVal Answer As AnswerRR)
            Answers.Remove(Answer)
            header.ANCOUNT -= 1
        End Sub

        Public ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)()
                bytes.AddRange(header.GetData)
                For Each Question As Question In Questions
                    bytes.AddRange(Question.Data)
                Next

                For Each Answer As AnswerRR In Answers
                    bytes.AddRange(Answer.Data)
                Next

                'NS
                For Each Authority As AuthorityRR In Authorities
                    bytes.AddRange(Authority.Data)
                Next

                'AR
                For Each Additional As AdditionalRR In Additionals
                    bytes.AddRange(Additional.Data)
                Next

                Return bytes.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordMX in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsMX() As RecordMX()
            Get
                Dim list As New List(Of RecordMX)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordMX = TryCast(answerRR.RECORD, RecordMX)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                list.Sort()
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordTXT in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsTXT() As RecordTXT()
            Get
                Dim list As New List(Of RecordTXT)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordTXT = TryCast(answerRR.RECORD, RecordTXT)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordA in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsA() As RecordA()
            Get
                Dim list As New List(Of RecordA)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordA = TryCast(answerRR.RECORD, RecordA)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordPTR in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsPTR() As RecordPTR()
            Get
                Dim list As New List(Of RecordPTR)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordPTR = TryCast(answerRR.RECORD, RecordPTR)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordCNAME in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsCNAME() As RecordCNAME()
            Get
                Dim list As New List(Of RecordCNAME)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordCNAME = TryCast(answerRR.RECORD, RecordCNAME)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordAAAA in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsAAAA() As RecordAAAA()
            Get
                Dim list As New List(Of RecordAAAA)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordAAAA = TryCast(answerRR.RECORD, RecordAAAA)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordNS in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsNS() As RecordNS()
            Get
                Dim list As New List(Of RecordNS)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordNS = TryCast(answerRR.RECORD, RecordNS)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' List of RecordSOA in Response.Answers
        ''' </summary>
        Public ReadOnly Property RecordsSOA() As RecordSOA()
            Get
                Dim list As New List(Of RecordSOA)()
                For Each answerRR As AnswerRR In Me.Answers
                    Dim record As RecordSOA = TryCast(answerRR.RECORD, RecordSOA)
                    If record IsNot Nothing Then
                        list.Add(record)
                    End If
                Next
                Return list.ToArray()
            End Get
        End Property

        Public ReadOnly Property RecordsRR() As RR()
            Get
                Dim list As New List(Of RR)()
                'For Each rr As RR In Me.Answers
                '    list.Add(rr)
                'Next
                For Each rr As RR In Me.Answers
                    list.Add(rr)
                Next
                For Each rr As RR In Me.Authorities
                    list.Add(rr)
                Next
                For Each rr As RR In Me.Additionals
                    list.Add(rr)
                Next
                Return list.ToArray()
            End Get
        End Property
    End Class
End Namespace
