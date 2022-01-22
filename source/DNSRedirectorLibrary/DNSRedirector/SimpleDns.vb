Imports System.Net
Imports System.Collections.ObjectModel
Imports System.Text.RegularExpressions


Namespace DnsRedirector

    ''' <summary>
    ''' List of key value pairs to assocate a name with an IP address
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class SimpleDns
        Inherits KeyedCollection(Of String, KeyValuePair(Of String, IPAddress))

        Private _RegExes As New Dictionary(Of String, Regex)
        Private _HaseAnyWildCard = False

        Protected Overrides Function GetKeyForItem(ByVal item As System.Collections.Generic.KeyValuePair(Of String, System.Net.IPAddress)) As String
            Return item.Key

        End Function

        Public Overloads Sub Add(ByVal domain As String, ByVal ip As IPAddress)
            Me.Add(New KeyValuePair(Of String, IPAddress)(domain, ip))
        End Sub

        Public Overloads Sub Add(ByVal simpleDnsEntry As KeyValuePair(Of String, IPAddress))
            If simpleDnsEntry.Key.Equals("*") Then
                'dont make this a regex or any regex rules below it in the file wont match
                _HaseAnyWildCard = True
            ElseIf simpleDnsEntry.Key.IndexOf("*", StringComparison.OrdinalIgnoreCase) > -1 Then
                _RegExes.Add(simpleDnsEntry.Key, New Regex(simpleDnsEntry.Key.Replace("*", ".*?"), RegexOptions.Compiled Or RegexOptions.Singleline))
            ElseIf simpleDnsEntry.Key.StartsWith("^", StringComparison.OrdinalIgnoreCase) Then
                _RegExes.Add(simpleDnsEntry.Key, New Regex(simpleDnsEntry.Key, RegexOptions.Compiled Or RegexOptions.Singleline))
            End If

            MyBase.Add(simpleDnsEntry)

        End Sub

        Public Overloads Function Contains(ByVal domain As String) As Boolean

            If _HaseAnyWildCard Then
                'Any key will match
                Return True
            End If

            If MyBase.Contains(domain) Then
                Return True
            End If

            For Each Pattern As Regex In _RegExes.Values
                If Pattern.Match(domain).Success Then
                    Return True
                    Exit For
                End If
            Next

            Return False
        End Function

        Default Public Overloads ReadOnly Property Item(ByVal domain As String) As KeyValuePair(Of String, IPAddress)
            Get
                If MyBase.Contains(domain) Then
                    Return MyBase.Item(domain)
                End If

                For Each Pattern As KeyValuePair(Of String, Regex) In _RegExes
                    If Pattern.Value.Match(domain).Success Then
                        Return MyBase.Item(Pattern.Key)
                        Exit For
                    End If
                Next

                If _HaseAnyWildCard Then
                    'Nothing else was matched so the item that matches everythign is left
                    Return MyBase.Item("*")
                End If

                Throw New KeyNotFoundException
            End Get
        End Property




    End Class

End Namespace

