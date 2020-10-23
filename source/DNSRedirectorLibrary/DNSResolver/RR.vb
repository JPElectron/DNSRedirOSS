Imports System

Namespace Dns

#Region "RFC info"
    '
    ' 3.2. RR definitions
    '
    ' 3.2.1. Format
    '
    ' All RRs have the same top level format shown below:
    '
    ' 1 1 1 1 1 1
    ' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | |
    ' / /
    ' / NAME /
    ' | |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | TYPE |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | CLASS |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | TTL |
    ' | |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | RDLENGTH |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
    ' / RDATA /
    ' / /
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    '
    '
    ' where:
    '
    ' NAME an owner name, i.e., the name of the node to which this
    ' resource record pertains.
    '
    ' TYPE two octets containing one of the RR TYPE codes.
    '
    ' CLASS two octets containing one of the RR CLASS codes.
    '
    ' TTL a 32 bit signed integer that specifies the time interval
    ' that the resource record may be cached before the source
    ' of the information should again be consulted. Zero
    ' values are interpreted to mean that the RR can only be
    ' used for the transaction in progress, and should not be
    ' cached. For example, SOA records are always distributed
    ' with a zero TTL to prohibit caching. Zero values can
    ' also be used for extremely volatile data.
    '
    ' RDLENGTH an unsigned 16 bit integer that specifies the length in
    ' octets of the RDATA field.
    '
    ' RDATA a variable length string of octets that describes the
    ' resource. The format of this information varies
    ' according to the TYPE and CLASS of the resource record.
    ' 

#End Region

    ''' <summary>
    ''' Resource Record (rfc1034 3.6.)
    ''' </summary>
    Public Class RR
        ''' <summary>
        ''' The name of the node to which this resource record pertains
        ''' </summary>
        Public NAME As String

        ''' <summary>
        ''' Specifies type of resource record
        ''' </summary>
        Public Type As Type

        ''' <summary>
        ''' Specifies type class of resource record, mostly IN but can be CS, CH or HS 
        ''' </summary>
        Public [Class] As [Class]

        ''' <summary>
        ''' Time to live, the time interval that the resource record may be cached
        ''' </summary>
        Public Property TTL() As UInteger
            Get
                Return CUInt(Math.Max(0, _TTL - TimeLived))
            End Get
            Set(ByVal value As UInteger)
                _TTL = value
            End Set
        End Property

        Private _TTL As UInteger

        ''' <summary>
        ''' 
        ''' </summary>
        Public RDLENGTH As UShort

        ''' <summary>
        ''' One of the Record* classes
        ''' </summary>
        Public RECORD As Record

        Public TimeLived As Integer

        Public ReadOnly Property Data() As Byte()
            Get
                Dim bytes As New List(Of Byte)
                bytes.AddRange(BitHelpers.WriteName(NAME))
                bytes.AddRange(BitHelpers.WriteShort(Type))
                bytes.AddRange(BitHelpers.WriteShort([Class]))
                bytes.AddRange(BitHelpers.WriteUInt32(TTL))
                Dim RData() As Byte = RECORD.Data
                bytes.AddRange(BitHelpers.WriteShort(RData.Length))
                bytes.AddRange(RData)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New()

        End Sub

        Public Sub New(ByVal rr As RecordReader)
            TimeLived = 0
            NAME = rr.ReadDomainName()
            Type = DirectCast(rr.ReadUInt16(), Type)
            [Class] = DirectCast(rr.ReadUInt16(), [Class])
            TTL = rr.ReadUInt32()
            RDLENGTH = rr.ReadUInt16()
            RECORD = rr.ReadRecord(Type)
            RECORD.RR = Me
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0,-32} {1}" & vbTab & "{2}" & vbTab & "{3}" & vbTab & "{4}", NAME, TTL, [Class], Type, RECORD)
        End Function
    End Class

    Public Class AnswerRR
        Inherits RR

        Public Sub New()

        End Sub

        Public Sub New(ByVal br As RecordReader)
            MyBase.New(br)
        End Sub
    End Class

    Public Class AuthorityRR
        Inherits RR

        Public Sub New()
        End Sub

        Public Sub New(ByVal br As RecordReader)
            MyBase.New(br)
        End Sub
    End Class

    Public Class AdditionalRR
        Inherits RR
        Public Sub New(ByVal br As RecordReader)
            MyBase.New(br)
        End Sub
    End Class
End Namespace