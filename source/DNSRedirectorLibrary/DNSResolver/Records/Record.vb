' Stuff records are made of

Namespace Dns
    Public MustInherit Class Record
        ''' <summary>
        ''' The Resource Record this RDATA record belongs to
        ''' </summary>
        Public RR As RR

        Public MustOverride ReadOnly Property Data() As Byte()

    End Class
End Namespace