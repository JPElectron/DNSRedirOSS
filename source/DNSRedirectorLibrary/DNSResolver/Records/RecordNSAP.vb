Imports System
Imports System.Text
'
' * http://tools.ietf.org/rfc/rfc1348.txt 
' * http://tools.ietf.org/html/rfc1706
' * 
' * |--------------|
' | <-- IDP --> |
' |--------------|-------------------------------------|
' | AFI | IDI | <-- DSP --> |
' |-----|--------|-------------------------------------|
' | 47 | 0005 | DFI | AA |Rsvd | RD |Area | ID |Sel |
' |-----|--------|-----|----|-----|----|-----|----|----|
' octets | 1 | 2 | 1 | 3 | 2 | 2 | 2 | 6 | 1 |
' |-----|--------|-----|----|-----|----|-----|----|----|
'
' IDP Initial Domain Part
' AFI Authority and Format Identifier
' IDI Initial Domain Identifier
' DSP Domain Specific Part
' DFI DSP Format Identifier
' AA Administrative Authority
' Rsvd Reserved
' RD Routing Domain Identifier
' Area Area Identifier
' ID System Identifier
' SEL NSAP Selector
'
' Figure 1: GOSIP Version 2 NSAP structure.
'
'
' 


Namespace Dns

    Public Class RecordNSAP
        Inherits Record
        Public LENGTH As UShort
        Public NSAPADDRESS As Byte()

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            LENGTH = rr.ReadUInt16()
            NSAPADDRESS = rr.ReadBytes(LENGTH)
        End Sub

        Public Overloads Overrides Function ToString() As String
            Dim sb As New StringBuilder()
            sb.AppendFormat("{0} ", LENGTH)
            For intI As Integer = 0 To NSAPADDRESS.Length - 1
                sb.AppendFormat("{0:X00}", NSAPADDRESS(intI))
            Next
            Return sb.ToString()
        End Function

        Public Function ToGOSIPV2() As String
            ' AFI
            ' IDI
            ' DFI
            ' AA
            ' Rsvd
            ' RD
            ' Area
            ' ID-High
            ' ID-Low
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0:X}.{1:X}.{2:X}.{3:X}.{4:X}.{5:X}.{6:X}{7:X}.{8:X}", NSAPADDRESS(0), NSAPADDRESS(1) << 8 Or NSAPADDRESS(2), NSAPADDRESS(3), NSAPADDRESS(4) << 16 Or NSAPADDRESS(5) << 8 Or NSAPADDRESS(6), NSAPADDRESS(7) << 8 Or NSAPADDRESS(8), _
            NSAPADDRESS(9) << 8 Or NSAPADDRESS(10), NSAPADDRESS(11) << 8 Or NSAPADDRESS(12), NSAPADDRESS(13) << 16 Or NSAPADDRESS(14) << 8 Or NSAPADDRESS(15), NSAPADDRESS(16) << 16 Or NSAPADDRESS(17) << 8 Or NSAPADDRESS(18), NSAPADDRESS(19))
        End Function

    End Class
End Namespace
