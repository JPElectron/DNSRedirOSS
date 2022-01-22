Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace Dns

    Public Class RecordReader
        Private _Data As Byte()
        Private _Position As Integer
        Public Sub New(ByVal data As Byte())
            _Data = data
            _Position = -1
        End Sub

        Public Property Position() As Integer
            Get
                Return _Position
            End Get
            Set(ByVal value As Integer)
                _Position = value
            End Set
        End Property

        Public Sub New(ByVal data As Byte(), ByVal Position As Integer)
            _Data = data
            _Position = Position - 1
        End Sub


        Public Function ReadByte() As Byte
            If _Position >= _Data.Length Then
                Return 0
            Else
                Return _Data(System.Math.Max(System.Threading.Interlocked.Increment(_Position), _Position - 1))
            End If
        End Function

        Public Shared Function HostToNetworkOrder(ByVal host As Byte) As Byte
            Return CByte((((host And &HFF) << 8) OrElse ((host >> 8) And &HFF)))
        End Function

        Public Function ReadChar() As Char
            Return Chr(ReadByte())
        End Function

        Public Function ReadUInt16() As UInt16
            'Need to make the first byte 16 bits or the shift will just get rid of the bytes
            Return Convert.ToUInt16(ReadByte()) << 8 Or ReadByte()
        End Function

        Public Function ReadUInt16(ByVal offset As Integer) As UInt16
            _Position += offset
            Return ReadUInt16()
        End Function

        Public Function ReadUInt32() As UInt32
            Return Convert.ToUInt32(ReadUInt16()) << 16 Or ReadUInt16()
        End Function

        Public Function ReadDomainName() As String
            Dim name As New StringBuilder()
            Dim length As Integer = 0

            ' get the length of the first label
            While (InlineAssignHelper(length, ReadByte())) <> 0
                ' top 2 bits set denotes domain name compression and to reference elsewhere
                If (length And &HC0) = &HC0 Then
                    ' work out the existing domain name, copy this pointer
                    Dim newRecordReader As New RecordReader(_Data, (length And &H3F) << 8 Or ReadByte())

                    name.Append(newRecordReader.ReadDomainName())
                    Return name.ToString()
                End If

                ' if not using compression, copy a char at a time to the domain name
                While length > 0
                    name.Append(ReadChar())
                    length -= 1
                End While
                name.Append("."c)
            End While
            If name.Length = 0 Then
                Return "."
            Else
                Return name.ToString()
            End If
        End Function

        Public Function ReadString() As String
            Dim length As Short = Me.ReadByte()

            Dim name As New StringBuilder()
            For intI As Integer = 0 To length - 1
                name.Append(ReadChar())
            Next
            Return name.ToString()
        End Function

        Public Function ReadBytes(ByVal intLength As Integer) As Byte()
            Dim list As New List(Of Byte)()
            For intI As Integer = 0 To intLength - 1
                list.Add(ReadByte())
            Next
            Return list.ToArray()
        End Function

        Public Function ReadRecord(ByVal type__1 As Type) As Record
            Select Case type__1
                Case Type.A
                    Return New RecordA(Me)
                Case Type.NS
                    Return New RecordNS(Me)
                Case Type.CNAME
                    Return New RecordCNAME(Me)
                Case Type.SOA
                    Return New RecordSOA(Me)
                Case Type.MB
                    Return New RecordMB(Me)
                Case Type.MG
                    Return New RecordMG(Me)
                Case Type.MR
                    Return New RecordMR(Me)
                Case Type.NULL
                    Return New RecordNULL(Me)
                Case Type.WKS
                    Return New RecordWKS(Me)
                Case Type.PTR
                    Return New RecordPTR(Me)
                Case Type.HINFO
                    Return New RecordHINFO(Me)
                Case Type.MINFO
                    Return New RecordMINFO(Me)
                Case Type.MX
                    Return New RecordMX(Me)
                Case Type.TXT
                    Return New RecordTXT(Me)
                Case Type.RP
                    Return New RecordRP(Me)
                Case Type.AFSDB
                    Return New RecordAFSDB(Me)
                Case Type.X25
                    Return New RecordX25(Me)
                Case Type.ISDN
                    Return New RecordISDN(Me)
                Case Type.RT
                    Return New RecordRT(Me)
                Case Type.NSAP
                    Return New RecordNSAP(Me)
                Case Type.SIG
                    Return New RecordSIG(Me)
                Case Type.KEY
                    Return New RecordKEY(Me)
                Case Type.PX
                    Return New RecordPX(Me)
                Case Type.AAAA
                    Return New RecordAAAA(Me)
                Case Type.LOC
                    Return New RecordLOC(Me)
                Case Type.SRV
                    Return New RecordSRV(Me)
                Case Type.NAPTR
                    Return New RecordNAPTR(Me)
                Case Type.KX
                    Return New RecordKX(Me)
                Case Type.DS
                    Return New RecordDS(Me)
                Case Type.TKEY
                    Return New RecordTKEY(Me)
                Case Type.TSIG
                    Return New RecordTSIG(Me)
                Case Else
                    Return New RecordUnknown(Me)
            End Select
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function

    End Class
End Namespace
