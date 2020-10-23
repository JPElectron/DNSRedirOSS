Imports System.Text
Imports System.Net


''' <summary>
''' Methods to convert data types to bytes
''' </summary>
''' <remarks></remarks>
Public Class BitHelpers

    ''' <summary>
    ''' Encodes a domain name as an array of bytes with "." replaced by the number of charachters that follow
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    ''' <remarks>Typically used by the DNS system</remarks>
    Public Shared Function WriteName(ByVal name As String) As Byte()
        If Not name.EndsWith(".", StringComparison.OrdinalIgnoreCase) Then
            name += "."
        End If

        If name = "." Then
            Return New Byte(0) {}
        End If

        Dim sb As New StringBuilder()
        Dim intI As Integer, intJ As Integer, intLen As Integer = name.Length
        sb.Append(ControlChars.NullChar)
        intI = 0
        intJ = 0
        While intI < intLen
            sb.Append(name(intI))
            If name(intI) = "."c Then
                sb(intI - intJ) = Convert.ToChar(intJ And &HFF)
                intJ = -1
            End If
            intI += 1
            intJ += 1
        End While
        sb(sb.Length - 1) = ControlChars.NullChar
        Return System.Text.Encoding.ASCII.GetBytes(sb.ToString())
    End Function

    Public Shared Function WriteShort(ByVal value As UShort) As Byte()
        Return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(CShort(value)))
    End Function

    Public Shared Function WriteUInt32(ByVal value As UInt32) As Byte()
        'IPAddress.HostToNetworkOrder returns a long so we need to only get the last 4 bytes, the first 4 will always be 0
        Dim bytes(3) As Byte
        Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 4, bytes, 0, 4)
        Return bytes
    End Function

    ''' <summary>
    ''' Reverses the bit order of a byte
    ''' </summary>
    ''' <param name="host"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function WriteByte(ByVal host As Byte) As Byte
        Dim Arr() As Byte = {host}
        Dim Bits As New BitArray(Arr)
        Dim TheByte As Byte = 0
        For i As Integer = 0 To 7
            If Bits(i) Then
                TheByte = TheByte Or 2 ^ ((i + 7 - (2 * i - 1)) - 1)
            End If
        Next
        Return TheByte

    End Function

End Class

