Imports System
'
' * http://www.ietf.org/rfc/rfc2845.txt
' * 
' * Field Name Data Type Notes
' --------------------------------------------------------------
' Algorithm Name domain-name Name of the algorithm
' in domain name syntax.
' Time Signed u_int48_t seconds since 1-Jan-70 UTC.
' Fudge u_int16_t seconds of error permitted
' in Time Signed.
' MAC Size u_int16_t number of octets in MAC.
' MAC octet stream defined by Algorithm Name.
' Original ID u_int16_t original message ID
' Error u_int16_t expanded RCODE covering
' TSIG processing.
' Other Len u_int16_t length, in octets, of
' Other Data.
' Other Data octet stream empty unless Error == BADTIME
'
' 


Namespace Dns

    Public Class RecordTSIG
        Inherits Record
        Public ALGORITHMNAME As String
        Public TIMESIGNED As Long
        Public FUDGE As UInt16
        Public MACSIZE As UInt16
        Public MAC As Byte()
        Public ORIGINALID As UInt16
        Public [ERROR] As UInt16
        Public OTHERLEN As UInt16
        Public OTHERDATA As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            ALGORITHMNAME = rr.ReadDomainName()
            TIMESIGNED = rr.ReadUInt32() << 32 Or rr.ReadUInt32()
            FUDGE = rr.ReadUInt16()
            MACSIZE = rr.ReadUInt16()
            MAC = rr.ReadBytes(MACSIZE)
            ORIGINALID = rr.ReadUInt16()
            [ERROR] = rr.ReadUInt16()
            OTHERLEN = rr.ReadUInt16()
            OTHERDATA = rr.ReadBytes(OTHERLEN)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Dim dateTime As New DateTime(1970, 1, 1, 0, 0, 0, _
            0)
            dateTime = dateTime.AddSeconds(TIMESIGNED)
            Dim printDate As String = (dateTime.ToShortDateString() & " ") + dateTime.ToShortTimeString()
            Return String.Format("{0} {1} {2} {3} {4}", ALGORITHMNAME, printDate, FUDGE, ORIGINALID, [ERROR])
        End Function

    End Class
End Namespace
