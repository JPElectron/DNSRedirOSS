Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Net
Imports System.Text

Namespace Dns

#Region "RFC specification"
    '
    ' 4.1.1. Header section format
    '
    ' The header contains the following fields:
    '
    ' 1 1 1 1 1 1
    ' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | ID |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' |QR| Opcode |AA|TC|RD|RA| Z | RCODE |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | QDCOUNT |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | ANCOUNT |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | NSCOUNT |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    ' | ARCOUNT |
    ' +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    '
    ' where:
    '
    ' ID A 16 bit identifier assigned by the program that
    ' generates any kind of query. This identifier is copied
    ' the corresponding reply and can be used by the requester
    ' to match up replies to outstanding queries.
    '
    ' QR A one bit field that specifies whether this message is a
    ' query (0), or a response (1).
    '
    ' OPCODE A four bit field that specifies kind of query in this
    ' message. This value is set by the originator of a query
    ' and copied into the response. The values are:
    '
    ' 0 a standard query (QUERY)
    '
    ' 1 an inverse query (IQUERY)
    '
    ' 2 a server status request (STATUS)
    '
    ' 3-15 reserved for future use
    '
    ' AA Authoritative Answer - this bit is valid in responses,
    ' and specifies that the responding name server is an
    ' authority for the domain name in question section.
    '
    ' Note that the contents of the answer section may have
    ' multiple owner names because of aliases. The AA bit
    ' corresponds to the name which matches the query name, or
    ' the first owner name in the answer section.
    '
    ' TC TrunCation - specifies that this message was truncated
    ' due to length greater than that permitted on the
    ' transmission channel.
    '
    ' RD Recursion Desired - this bit may be set in a query and
    ' is copied into the response. If RD is set, it directs
    ' the name server to pursue the query recursively.
    ' Recursive query support is optional.
    '
    ' RA Recursion Available - this be is set or cleared in a
    ' response, and denotes whether recursive query support is
    ' available in the name server.
    '
    ' Z Reserved for future use. Must be zero in all queries
    ' and responses.
    '
    ' RCODE Response code - this 4 bit field is set as part of
    ' responses. The values have the following
    ' interpretation:
    '
    ' 0 No error condition
    '
    ' 1 Format error - The name server was
    ' unable to interpret the query.
    '
    ' 2 Server failure - The name server was
    ' unable to process this query due to a
    ' problem with the name server.
    '
    ' 3 Name Error - Meaningful only for
    ' responses from an authoritative name
    ' server, this code signifies that the
    ' domain name referenced in the query does
    ' not exist.
    '
    ' 4 Not Implemented - The name server does
    ' not support the requested kind of query.
    '
    ' 5 Refused - The name server refuses to
    ' perform the specified operation for
    ' policy reasons. For example, a name
    ' server may not wish to provide the
    ' information to the particular requester,
    ' or a name server may not wish to perform
    ' a particular operation (e.g., zone
    ' transfer) for particular data.
    '
    ' 6-15 Reserved for future use.
    '
    ' QDCOUNT an unsigned 16 bit integer specifying the number of
    ' entries in the question section.
    '
    ' ANCOUNT an unsigned 16 bit integer specifying the number of
    ' resource records in the answer section.
    '
    ' NSCOUNT an unsigned 16 bit integer specifying the number of name
    ' server resource records in the authority records
    ' section.
    '
    ' ARCOUNT an unsigned 16 bit integer specifying the number of
    ' resource records in the additional records section.
    '
    ' 

#End Region

    Public Class Header
        ''' <summary>
        ''' An identifier assigned by the program
        ''' </summary>
        Public ID As UShort

        ' internal flag
        Private Flags As UShort

        ''' <summary>
        ''' the number of entries in the question section
        ''' </summary>
        Public QDCOUNT As UShort

        ''' <summary>
        ''' the number of resource records in the answer section
        ''' </summary>
        Public ANCOUNT As UShort

        ''' <summary>
        ''' the number of name server resource records in the authority records section
        ''' </summary>
        Public NSCOUNT As UShort

        ''' <summary>
        ''' the number of resource records in the additional records section
        ''' </summary>
        Public ARCOUNT As UShort

        Public Sub New()
        End Sub

        Public Sub New(ByVal rr As RecordReader)
            ID = rr.ReadUInt16()
            Flags = rr.ReadUInt16()
            QDCOUNT = rr.ReadUInt16()
            ANCOUNT = rr.ReadUInt16()
            NSCOUNT = rr.ReadUInt16()
            ARCOUNT = rr.ReadUInt16()
        End Sub

        Public Shared Function HostToNetworkOrder(ByVal host As Byte) As Byte
            Return CByte((((host And &HFF) << 8) OrElse ((host >> 8) And &HFF)))
        End Function


        Private Function SetBits(ByVal oldValue As UShort, ByVal position As Integer, ByVal length As Integer, ByVal blnValue As Boolean) As UShort
            Return SetBits(oldValue, position, length, If(blnValue, CUShort(1), CUShort(0)))
        End Function

        Private Shared Function SetBits(ByVal oldValue As UShort, ByVal position As Integer, ByVal length As Integer, ByVal newValue As UShort) As UShort
            ' sanity check
            If length <= 0 OrElse position >= 16 Then
                Return oldValue
            End If

            ' get some mask to put on
            Dim mask As Integer = (2 << (length - 1)) - 1

            ' clear out value
            oldValue = CUShort(oldValue And Not (mask << position))

            ' set new value
            oldValue = CUShort(oldValue Or ((newValue And mask) << position))
            Return oldValue
        End Function

        Private Shared Function GetBits(ByVal oldValue As UShort, ByVal position As Integer, ByVal length As Integer) As UShort
            ' sanity check
            If length <= 0 OrElse position >= 16 Then
                Return 0
            End If

            ' get some mask to put on
            Dim mask As Integer = (2 << (length - 1)) - 1

            ' shift down to get some value and mask it
            Return CUShort(((oldValue >> position) And mask))
        End Function

        ''' <summary>
        ''' Represents the header as a byte array
        ''' </summary>
        Public Function GetData() As Byte()

            Dim bytes As New List(Of Byte)()
            bytes.AddRange(WriteShort(ID))
            bytes.AddRange(WriteShort(Flags))
            bytes.AddRange(WriteShort(QDCOUNT))
            bytes.AddRange(WriteShort(ANCOUNT))
            bytes.AddRange(WriteShort(NSCOUNT))
            bytes.AddRange(WriteShort(ARCOUNT))
            Return bytes.ToArray()

        End Function

        Private Shared Function WriteShort(ByVal sValue As UShort) As Byte()
            'HostToNetworkOrder returns for bytes. The value passed in is a short so we know the last 2 elements of the array are the 16 bit (2 byte) number
            Dim IntBytes() As Byte = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(CUShort(sValue)))
            Return New Byte() {IntBytes(2), IntBytes(3)}
        End Function

        ''' <summary>
        ''' query (false), or a response (true)
        ''' </summary>
        Public Property QR() As Boolean
            Get
                Return GetBits(Flags, 15, 1) = 1
            End Get
            Set(ByVal value As Boolean)
                Flags = SetBits(Flags, 15, 1, value)
            End Set
        End Property

        ''' <summary>
        ''' Specifies kind of query
        ''' </summary>
        Public Property OPCODE() As OPCode
            Get
                Return CType(GetBits(Flags, 11, 4), OPCode)
            End Get
            Set(ByVal value As OPCode)
                Flags = SetBits(Flags, 11, 4, CUShort(value))
            End Set
        End Property

        ''' <summary>
        ''' Authoritative Answer
        ''' </summary>
        Public Property AA() As Boolean
            Get
                Return GetBits(Flags, 10, 1) = 1
            End Get
            Set(ByVal value As Boolean)
                Flags = SetBits(Flags, 10, 1, value)
            End Set
        End Property

        ''' <summary>
        ''' TrunCation
        ''' </summary>
        Public Property TC() As Boolean
            Get
                Return GetBits(Flags, 9, 1) = 1
            End Get
            Set(ByVal value As Boolean)
                Flags = SetBits(Flags, 9, 1, value)
            End Set
        End Property

        ''' <summary>
        ''' Recursion Desired
        ''' </summary>
        Public Property RD() As Boolean
            Get
                Return GetBits(Flags, 8, 1) = 1
            End Get
            Set(ByVal value As Boolean)
                Flags = SetBits(Flags, 8, 1, value)
            End Set
        End Property

        ''' <summary>
        ''' Recursion Available
        ''' </summary>
        Public Property RA() As Boolean
            Get
                Return GetBits(Flags, 7, 1) = 1
            End Get
            Set(ByVal value As Boolean)
                Flags = SetBits(Flags, 7, 1, value)
            End Set
        End Property

        ''' <summary>
        ''' Reserved for future use
        ''' </summary>
        Public Property Z() As UShort
            Get
                Return GetBits(Flags, 4, 3)
            End Get
            Set(ByVal value As UShort)
                Flags = SetBits(Flags, 4, 3, value)
            End Set
        End Property

        ''' <summary>
        ''' Response code
        ''' </summary>
        Public Property RCODE() As RCode
            Get
                Return GetBits(Flags, 0, 4)
            End Get
            Set(ByVal value As RCode)
                Flags = SetBits(Flags, 0, 4, CUShort(value))
            End Set
        End Property
    End Class
End Namespace