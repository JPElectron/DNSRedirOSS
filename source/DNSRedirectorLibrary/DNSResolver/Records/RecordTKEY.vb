Imports System
'
' * http://tools.ietf.org/rfc/rfc2930.txt
' * 
'2. The TKEY Resource Record
'
' The TKEY resource record (RR) has the structure given below. Its RR
' type code is 249.
'
' Field Type Comment
' ----- ---- -------
' Algorithm: domain
' Inception: u_int32_t
' Expiration: u_int32_t
' Mode: u_int16_t
' Error: u_int16_t
' Key Size: u_int16_t
' Key Data: octet-stream
' Other Size: u_int16_t
' Other Data: octet-stream undefined by this specification
'
' 


Namespace Dns

    Public Class RecordTKEY
        Inherits Record
        Public ALGORITHM As String
        Public INCEPTION As UInt32
        Public EXPIRATION As UInt32
        Public MODE As UInt16
        Public [ERROR] As UInt16
        Public KEYSIZE As UInt16
        Public KEYDATA As Byte()
        Public OTHERSIZE As UInt16
        Public OTHERDATA As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            ALGORITHM = rr.ReadDomainName()
            INCEPTION = rr.ReadUInt32()
            EXPIRATION = rr.ReadUInt32()
            MODE = rr.ReadUInt16()
            [ERROR] = rr.ReadUInt16()
            KEYSIZE = rr.ReadUInt16()
            KEYDATA = rr.ReadBytes(KEYSIZE)
            OTHERSIZE = rr.ReadUInt16()
            OTHERDATA = rr.ReadBytes(OTHERSIZE)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0} {1} {2} {3} {4}", ALGORITHM, INCEPTION, EXPIRATION, MODE, [ERROR])
        End Function

    End Class
End Namespace
