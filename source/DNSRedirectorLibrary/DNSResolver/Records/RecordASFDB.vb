Imports System
' http://tools.ietf.org/rfc/rfc1183.txt
'
' * 1. AFS Data Base location
'
' This section defines an extension of the DNS to locate servers both
' for AFS (AFS is a registered trademark of Transarc Corporation) and
' for the Open Software Foundation's (OSF) Distributed Computing
' Environment (DCE) authenticated naming system using HP/Apollo's NCA,
' both to be components of the OSF DCE. The discussion assumes that
' the reader is familiar with AFS [5] and NCA [6].
'
' The AFS (originally the Andrew File System) system uses the DNS to
' map from a domain name to the name of an AFS cell database server.
' The DCE Naming service uses the DNS for a similar function: mapping
' from the domain name of a cell to authenticated name servers for that
' cell. The method uses a new RR type with mnemonic AFSDB and type
' code of 18 (decimal).
'
' AFSDB has the following format:
'
' <owner> <ttl> <class> AFSDB <subtype> <hostname>
'
' Both RDATA fields are required in all AFSDB RRs. The <subtype> field
' is a 16 bit integer. The <hostname> field is a domain name of a host
' that has a server for the cell named by the owner name of the RR.
'
' 


Namespace Dns
    Public Class RecordAFSDB
        Inherits Record
        Public SUBTYPE As UShort
        Public HOSTNAME As String

        Public Overrides ReadOnly Property Data() As Byte()
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Sub New(ByVal rr As RecordReader)
            SUBTYPE = rr.ReadUInt16()
            'HOSTNAME = rr.ReadString();
            HOSTNAME = rr.ReadDomainName()
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return String.Format("{0} {1}", SUBTYPE, HOSTNAME)
        End Function

    End Class
End Namespace
