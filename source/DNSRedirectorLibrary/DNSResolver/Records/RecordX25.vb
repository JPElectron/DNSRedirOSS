﻿Imports System
' http://tools.ietf.org/rfc/rfc1183.txt
'
'3.1. The X25 RR
'
' The X25 RR is defined with mnemonic X25 and type code 19 (decimal).
'
' X25 has the following format:
'
' <owner> <ttl> <class> X25 <PSDN-address>
'
' <PSDN-address> is required in all X25 RRs.
'
' <PSDN-address> identifies the PSDN (Public Switched Data Network)
' address in the X.121 [10] numbering plan associated with <owner>.
' Its format in master files is a <character-string> syntactically
' identical to that used in TXT and HINFO.
'
' The format of X25 is class insensitive. X25 RRs cause no additional
' section processing.
'
' The <PSDN-address> is a string of decimal digits, beginning with the
' 4 digit DNIC (Data Network Identification Code), as specified in
' X.121. National prefixes (such as a 0) MUST NOT be used.
'
' For example:
'
' Relay.Prime.COM. X25 311061700956
'
'
' 


Namespace Dns

    Public Class RecordX25
        Inherits Record
        Public PSDNADDRESS As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Dim StringBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(PSDNADDRESS)
                Dim bytes As New List(Of Byte)
                bytes.Add(Convert.ToByte(StringBytes.Length))
                bytes.AddRange(StringBytes)
                Return bytes.ToArray
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            PSDNADDRESS = rr.ReadString()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0}", PSDNADDRESS)
        End Function

    End Class
End Namespace
