Imports System.IO
Imports System.Net
Imports System.Net.NetworkInformation

Namespace Dhcp


    Public Enum MessageType
        DhcpRequest = 3
        DhcpAck = 5
        DhcpInform = 8
    End Enum

    Public Enum OptionType
        Padding = 0
        DomainNameServer = 6
        HostName = 12
        MessageType = 53
        ParameterRequestList = 55
        ClientIdentifier = 61
        EndOptions = 255
    End Enum

    Friend NotInheritable Class DhcpMessage

        Public Class DhcpOptionCollection
            Inherits ObjectModel.KeyedCollection(Of OptionType, DhcpOption)

            Private _MagicCookie() As Byte = {99, 130, 83, 99}

            Protected Overrides Function GetKeyForItem(ByVal item As DhcpOption) As OptionType
                Return item.Type
            End Function

            Public Function GetData() As Byte()

                Using MS As New MemoryStream
                    Using BR As New BinaryWriter(MS)
                        With BR
                            'Always start with the magic cookie
                            BR.Write(_MagicCookie)

                            For Each Opt In Me
                                BR.Write(Convert.ToByte(Opt.Type))
                                BR.Write(Opt.Length)
                                BR.Write(Opt.Value)
                            Next

                            'Always end with the end option
                            BR.Write(255)
                        End With
                    End Using

                    GetData = MS.ToArray
                End Using

            End Function

            Public Sub New()

            End Sub

            Public Sub New(ByVal data() As Byte)
                For i = 4 To data.Length - 1
                    Dim OptType As Byte = data(i)
                    i += 1
                    If OptType = OptionType.EndOptions Then Exit For
                    If OptType = OptionType.Padding Then Continue For

                    Dim Opt As New DhcpOption(OptType)
                    Dim OptLength As Integer = data(i)
                    i += 1
                    Dim OptValue(OptLength - 1) As Byte
                    Array.Copy(data, i, OptValue, 0, OptLength)
                    i += OptLength - 1
                    Opt.Value = OptValue

                    Me.Add(Opt)
                Next
            End Sub

        End Class

        Public op As Byte = 1
        Public htype As Byte = 1
        Public hlen As Byte
        Public hops As Byte
        Private _xid(3) As Byte
        Public secs(1) As Byte
        Public flags(1) As Byte
        Private _ciaddr(3) As Byte
        Public yiaddr(3) As Byte
        Public siaddr(3) As Byte
        Public giaddr(3) As Byte
        Private _chaddr(15) As Byte
        Public sname(63) As Byte
        Public file(127) As Byte
        Public options As New DhcpOptionCollection


        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709")> _
        Public Property ciaddr() As IPAddress
            Get
                Return New IPAddress(_ciaddr)
            End Get
            Set(ByVal value As IPAddress)
                _ciaddr = value.GetAddressBytes
            End Set
        End Property

        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709")> _
        Public Property chaddr() As PhysicalAddress
            Get
                Return New PhysicalAddress(_chaddr)
            End Get
            Set(ByVal value As PhysicalAddress)
                Array.Copy(value.GetAddressBytes, _chaddr, value.GetAddressBytes.Length)
                hlen = value.GetAddressBytes.Length
            End Set
        End Property

        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709")> _
        Public ReadOnly Property xid() As UInt32
            Get
                Return BitConverter.ToUInt32(_xid, 0)
            End Get
        End Property


        Public Function GetData() As Byte()

            Using MS As New MemoryStream
                Using BR As New BinaryWriter(MS)
                    With BR
                        .Write(op)
                        .Write(htype)
                        .Write(hlen)
                        .Write(hops)
                        .Write(_xid)
                        .Write(secs)
                        .Write(flags)
                        .Write(_ciaddr)
                        .Write(yiaddr)
                        .Write(siaddr)
                        .Write(giaddr)
                        .Write(_chaddr)
                        .Write(sname)
                        .Write(file)
                        .Write(options.GetData)
                    End With
                End Using

                GetData = MS.ToArray
            End Using

        End Function

        Public Sub New(ByVal type As MessageType)
            'Set the random id for this message
            Dim Rand = New Random
            _xid = BitHelpers.WriteUInt32(CUInt(Rand.Next(0, Integer.MaxValue)))

            'add the message type to options
            Dim MessageType As New DhcpOption(OptionType.MessageType)
            MessageType.Value = New Byte(0) {type}
            options.Add(MessageType)
        End Sub

        Public Sub New(ByVal data() As Byte)
            op = data(0)
            htype = data(1)
            hlen = data(2)
            hops = data(3)
            Array.Copy(data, 4, _xid, 0, 4)
            secs(0) = data(8)
            secs(1) = data(9)
            flags(0) = data(10)
            flags(1) = data(11)
            Array.Copy(data, 12, _ciaddr, 0, 4)
            Array.Copy(data, 16, yiaddr, 0, 4)
            Array.Copy(data, 20, siaddr, 0, 4)
            Array.Copy(data, 24, giaddr, 0, 4)
            Array.Copy(data, 28, _chaddr, 0, 16)
            Array.Copy(data, 44, sname, 0, 64)
            Array.Copy(data, 108, file, 0, 128)
            Dim optionsBytes(data.Length - 236) As Byte
            Array.Copy(data, 236, optionsBytes, 0, optionsBytes.Length - 1)
            options = New DhcpOptionCollection(optionsBytes)
        End Sub


    End Class

    Public Structure DhcpOption
        Public Type As OptionType
        Public Value() As Byte
        Public ReadOnly Property Length() As Byte
            Get
                Return Value.Length
            End Get
        End Property

        Public Sub New(ByVal optionType As OptionType)
            Type = optionType
        End Sub
    End Structure
End Namespace
