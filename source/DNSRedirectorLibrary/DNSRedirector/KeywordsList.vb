Imports System.Text.RegularExpressions
Imports System.Linq

Namespace Text

    ''' <summary>
    ''' A list of strings and regular expression that can be compared against a string
    ''' to determine of that string contains any of the keywords
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KeywordsList

        Public Enum SearchMethod
            ''' <summary>
            ''' Slowest, best memory consuption
            ''' </summary>
            ''' <remarks></remarks>
            Linear
            ''' <summary>
            ''' Fastest, high memory consuption
            ''' </summary>
            ''' <remarks></remarks>
            AhoCorasick
        End Enum

        Private _Keywords As New List(Of String)
        Private _Regexes As New List(Of Regex)
        Private _StringSearch As StringSearch
        Private _SearchTreeRequiresRebuild As Boolean
        Private _Method As SearchMethod = SearchMethod.Linear
        Private _comparison As StringComparison = StringComparison.Ordinal

        ''' <summary>
        ''' The search algorithm used
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Method() As SearchMethod
            Get
                Return _Method
            End Get
            Set(ByVal value As SearchMethod)
                If value = SearchMethod.AhoCorasick AndAlso _Method = SearchMethod.Linear Then
                    _StringSearch = New StringSearch
                    _Method = value
                    DoPreprocessing()
                ElseIf value = SearchMethod.Linear AndAlso _Method = SearchMethod.AhoCorasick Then
                    _StringSearch = Nothing
                    _Method = value
                Else
                    _Method = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Number of keywords in the list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count()
            Get
                Return _Keywords.Count + _Regexes.Count
            End Get
        End Property

        Public Sub New(comparison As StringComparison)
            _comparison = comparison
        End Sub

        ''' <summary>
        ''' Add a keyword to the list
        ''' </summary>
        ''' <param name="keyword"></param>
        ''' <remarks>Keywords containing "*" will be treated as a regular expression</remarks>
        Public Sub AddKeyword(ByVal keyword As String)
            If keyword.IndexOf("*", StringComparison.OrdinalIgnoreCase) > -1 Then
                AddRegex(keyword.Replace("*", ".*?"))
            Else
                If _comparison = StringComparison.OrdinalIgnoreCase Then
                    keyword = keyword.ToUpperInvariant()
                End If

                If Not _Keywords.Contains(keyword) Then
                        _Keywords.Add(keyword)
                        'Will need to rebuild the string search dictionary
                        _SearchTreeRequiresRebuild = True
                    End If
                End If
        End Sub

        ''' <summary>
        ''' Add a regular expression object
        ''' </summary>
        ''' <param name="regex"></param>
        ''' <remarks></remarks>
        Public Sub AddRegex(ByVal regex As Regex)
            Dim HasRegex As Boolean = False
            For Each Pattern As Regex In _Regexes
                If Pattern.ToString.Equals(regex.ToString, StringComparison.InvariantCulture) Then
                    HasRegex = True
                    Exit For
                End If
            Next
            If Not HasRegex Then _Regexes.Add(regex)
        End Sub

        ''' <summary>
        ''' Converts the string to a regular expression
        ''' </summary>
        ''' <param name="pattern"></param>
        ''' <remarks></remarks>
        Public Sub AddRegex(ByVal pattern As String)
            Dim regexOptions As RegexOptions = RegexOptions.Compiled Or RegexOptions.Singleline
            If _comparison = StringComparison.OrdinalIgnoreCase Then
                regexOptions = regexOptions Or RegexOptions.IgnoreCase
            End If
            Dim Regex As New Regex(pattern, regexOptions)
            AddRegex(Regex)
        End Sub

        ''' <summary>
        ''' Determines if the searchString contains a keyword
        ''' </summary>
        ''' <param name="searchString"></param>
        ''' <param name="matchedKeyword">The value of this parameter is set to the first keyword that is matched if any</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Contains(ByVal searchString As String, ByRef matchedKeyword As String) As Boolean

            Dim sw As New Stopwatch
            sw.Start()

            If _comparison = StringComparison.OrdinalIgnoreCase Then
                searchString = searchString.ToUpperInvariant()
            End If

            Select Case Method
                Case SearchMethod.Linear
                    For Each Keyword As String In _Keywords
                        If searchString.IndexOf(Keyword, StringComparison.Ordinal) > -1 Then
                            sw.Stop()
                            Debug.WriteLine("Search took:" & sw.ElapsedTicks & " ticks")
                            matchedKeyword = Keyword
                            Return True
                        End If
                    Next

                Case SearchMethod.AhoCorasick
                    'Ensure the search algorthm has the freshest keywords and is built
                    DoPreprocessing()

                    If (_StringSearch.Keywords IsNot Nothing AndAlso _StringSearch.Keywords.Any()) Then
                        Dim sr As StringSearchResult = _StringSearch.FindFirst(searchString)

                        If sr.Index <> -1 Then
                            matchedKeyword = sr.Keyword
                            sw.Stop()
                            Debug.WriteLine("Search took:" & sw.ElapsedTicks & " ticks")
                            Return True
                        End If
                    End If
            End Select

            For Each Pattern As Regex In _Regexes
                If Pattern.Match(searchString).Success Then
                    matchedKeyword = Pattern.ToString
                    sw.Stop()
                    Debug.WriteLine("Search took:" & sw.ElapsedTicks & " ticks")
                    Return True
                    Exit For
                End If
            Next
            'End If


            sw.Stop()
            Debug.WriteLine("Search took:" & sw.ElapsedTicks & " ticks")
            matchedKeyword = Nothing
            Return False
        End Function

        ''' <summary>
        ''' Creates a prefix tree for the AhoCorasick
        ''' </summary>
        ''' <remarks>This does not happen on each search and only needs to be called when a keyword is added.</remarks>
        Public Sub DoPreprocessing()
            If _Method = SearchMethod.AhoCorasick AndAlso _SearchTreeRequiresRebuild Then
                _StringSearch.Keywords = _Keywords
                _SearchTreeRequiresRebuild = False
            End If
        End Sub

    End Class
End Namespace
