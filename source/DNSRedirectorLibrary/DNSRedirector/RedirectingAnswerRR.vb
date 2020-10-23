Imports PTSoft.Dns


Namespace DnsRedirector

    Public Enum RedirectType
        None
        AppVersion
        ClientCount
        AuthClientCount
        ReleaseCheck
        SimpleDns
        Unauthorized
        Blocked
    End Enum

    <Flags()> _
    Public Enum VerificationType
        None = 0
        Authorize = 1 << 1
        BlockBypassToggle = 1 << 3
        ResetClientToggle = 1 << 4
    End Enum

    ''' <summary>
    ''' Extention of a DNS answer that indicates why the answer was redirected
    ''' </summary>
    ''' <remarks></remarks>
    Public Class RedirectableAnswerRR
        Inherits AnswerRR

        Public RedirectType As RedirectType = DnsRedirector.RedirectType.None

    End Class
End Namespace

