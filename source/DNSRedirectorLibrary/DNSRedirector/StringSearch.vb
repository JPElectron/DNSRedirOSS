' Aho-Corasick text search algorithm implementation
' For more information visit http://www.cs.uku.fi/~kilpelai/BSA05/lectures/slides04.pdf

Imports System
Imports System.Collections

Namespace Text


    ''' <summary>
    ''' Interface containing all methods to be implemented
    ''' by string search algorithm
    ''' </summary>
    Public Interface IStringSearchAlgorithm
#Region "Methods & Properties"

        ''' <summary>
        ''' List of keywords to search for
        ''' </summary>
        Property Keywords() As IEnumerable(Of String)


        ''' <summary>
        ''' Searches passed text and returns all occurrences of any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>Array of occurrences</returns>
        Function FindAll(ByVal text As String) As List(Of StringSearchResult)

        ''' <summary>
        ''' Searches passed text and returns first occurrence of any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
        Function FindFirst(ByVal text As String) As StringSearchResult

        ''' <summary>
        ''' Searches passed text and returns true if text contains any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>True when text contains any keyword</returns>
        Function ContainsAny(ByVal text As String) As Boolean

#End Region
    End Interface

    ''' <summary>
    ''' Structure containing results of search 
    ''' (keyword and position in original text)
    ''' </summary>
    Public Structure StringSearchResult
#Region "Members"

        Private _index As Integer
        Private _keyword As String

        ''' <summary>
        ''' Initialize string search result
        ''' </summary>
        ''' <param name="index">Index in text</param>
        ''' <param name="keyword">Found keyword</param>
        Public Sub New(ByVal index As Integer, ByVal keyword As String)
            _index = index
            _keyword = keyword
        End Sub


        ''' <summary>
        ''' Returns index of found keyword in original text
        ''' </summary>
        Public ReadOnly Property Index() As Integer
            Get
                Return _index
            End Get
        End Property


        ''' <summary>
        ''' Returns keyword found by this result
        ''' </summary>
        Public ReadOnly Property Keyword() As String
            Get
                Return _keyword
            End Get
        End Property


        ''' <summary>
        ''' Returns empty search result
        ''' </summary>
        Public Shared ReadOnly Property Empty() As StringSearchResult
            Get
                Return New StringSearchResult(-1, "")
            End Get
        End Property

#End Region
    End Structure


    ''' <summary>
    ''' Class for searching string for one or multiple 
    ''' keywords using efficient Aho-Corasick search algorithm
    ''' </summary>
    Public Class StringSearch
        Implements IStringSearchAlgorithm
#Region "Objects"

        ''' <summary>
        ''' Tree node representing character and its 
        ''' transition and failure function
        ''' </summary>
        Private Class TreeNode
#Region "Constructor & Methods"

            ''' <summary>
            ''' Initialize tree node with specified character
            ''' </summary>
            ''' <param name="parent">Parent node</param>
            ''' <param name="c">Character</param>
            Public Sub New(ByVal parent As TreeNode, ByVal c As Char)
                _char = c
                _parent = parent
                _results = New List(Of String)(1)
                '_resultsAr = New String() {}

                _transitionsAr = New TreeNode() {}
                _transHash = New Dictionary(Of Char, TreeNode)(1)
            End Sub


            ''' <summary>
            ''' Adds pattern ending in this node
            ''' </summary>
            ''' <param name="result">Pattern</param>
            Public Sub AddResult(ByVal result As String)
                If _results.Contains(result) Then
                    Exit Sub
                End If
                _results.Add(result)
                '_resultsAr = _results.ToArray()
            End Sub

            ''' <summary>
            ''' Adds trabsition node
            ''' </summary>
            ''' <param name="node">Node</param>
            Public Sub AddTransition(ByVal node As TreeNode)
                _transHash.Add(node.[Char], node)
                Dim ar As TreeNode() = New TreeNode(_transHash.Values.Count - 1) {}
                _transHash.Values.CopyTo(ar, 0)
                _transitionsAr = ar
            End Sub


            ''' <summary>
            ''' Returns transition to specified character (if exists)
            ''' </summary>
            ''' <param name="c">Character</param>
            ''' <returns>Returns TreeNode or null</returns>
            Public Function GetTransition(ByVal c As Char) As TreeNode
                Dim value As TreeNode = Nothing
                _transHash.TryGetValue(c, value)
                Return value
            End Function


            ''' <summary>
            ''' Returns true if node contains transition to specified character
            ''' </summary>
            ''' <param name="c">Character</param>
            ''' <returns>True if transition exists</returns>
            Public Function ContainsTransition(ByVal c As Char) As Boolean
                Return _transHash.ContainsKey(c)
            End Function

#End Region
#Region "Properties"

            Private _char As Char
            Private _parent As TreeNode
            Private _failure As TreeNode
            Private _results As New List(Of String)
            Private _transitionsAr As TreeNode()
            'Private _resultsAr As String()
            Private _transHash As Dictionary(Of Char, TreeNode)

            ''' <summary>
            ''' Character
            ''' </summary>
            Public ReadOnly Property [Char]() As Char
                Get
                    Return _char
                End Get
            End Property


            ''' <summary>
            ''' Parent tree node
            ''' </summary>
            Public ReadOnly Property Parent() As TreeNode
                Get
                    Return _parent
                End Get
            End Property


            ''' <summary>
            ''' Failure function - descendant node
            ''' </summary>
            Public Property Failure() As TreeNode
                Get
                    Return _failure
                End Get
                Set(ByVal value As TreeNode)
                    _failure = value
                End Set
            End Property


            ''' <summary>
            ''' Transition function - list of descendant nodes
            ''' </summary>
            Public ReadOnly Property Transitions() As TreeNode()
                Get
                    Return _transitionsAr
                End Get
            End Property


            ''' <summary>
            ''' Returns list of patterns ending by this letter
            ''' </summary>
            Public ReadOnly Property Results() As List(Of String)
                Get
                    'Return _resultsAr
                    Return _results
                End Get
            End Property

#End Region
        End Class





#End Region
#Region "Local fields"

        ''' <summary>
        ''' Root of keyword tree
        ''' </summary>
        Private _root As TreeNode

        ''' <summary>
        ''' Keywords to search for
        ''' </summary>
        Private _keywords As IEnumerable(Of String)

#End Region

#Region "Initialization"

        ''' <summary>
        ''' Initialize search algorithm (Build keyword tree)
        ''' </summary>
        ''' <param name="keywordsArr">Keywords to search for</param>
        Public Sub New(ByVal keywordsArr As IEnumerable(Of String))
            Keywords = keywordsArr
        End Sub


        ''' <summary>
        ''' Initialize search algorithm with no keywords
        ''' (Use Keywords property)
        ''' </summary>
        Public Sub New()
        End Sub

#End Region
#Region "Implementation"

        ''' <summary>
        ''' Build tree from specified keywords
        ''' </summary>
        Private Sub BuildTree()
            ' Build keyword tree and transition function
            _root = New TreeNode(Nothing, " "c)
            For Each p As String In _keywords
                ' add pattern to tree
                Dim nd As TreeNode = _root
                For Each c As Char In p
                    Dim ndNew As TreeNode = Nothing
                    For Each trans As TreeNode In nd.Transitions
                        If trans.[Char] = c Then
                            ndNew = trans
                            Exit For
                        End If
                    Next

                    If ndNew Is Nothing Then
                        ndNew = New TreeNode(nd, c)
                        nd.AddTransition(ndNew)
                    End If
                    nd = ndNew
                Next
                nd.AddResult(p)
            Next

            ' Find failure functions
            Dim nodes As New List(Of TreeNode)
            ' level 1 nodes - fail to root node
            For Each nd As TreeNode In _root.Transitions
                nd.Failure = _root
                For Each trans As TreeNode In nd.Transitions
                    nodes.Add(trans)
                Next
            Next
            ' other nodes - using BFS
            While nodes.Count <> 0
                Dim newNodes As New List(Of TreeNode)
                For Each nd As TreeNode In nodes
                    Dim r As TreeNode = nd.Parent.Failure
                    Dim c As Char = nd.[Char]

                    While r IsNot Nothing AndAlso Not r.ContainsTransition(c)
                        r = r.Failure
                    End While
                    If r Is Nothing Then
                        nd.Failure = _root
                    Else
                        nd.Failure = r.GetTransition(c)
                        For Each result As String In nd.Failure.Results
                            nd.AddResult(result)
                        Next
                    End If

                    ' add child nodes to BFS list 
                    For Each child As TreeNode In nd.Transitions
                        newNodes.Add(child)
                    Next
                Next
                nodes = newNodes
            End While
            _root.Failure = _root
        End Sub


#End Region
#Region "Methods & Properties"

        ''' <summary>
        ''' Keywords to search for (setting this property is slow, because
        ''' it requieres rebuilding of keyword tree)
        ''' </summary>
        Public Property Keywords() As IEnumerable(Of String) Implements IStringSearchAlgorithm.Keywords
            Get
                Return _keywords
            End Get
            Set(ByVal value As IEnumerable(Of String))
                _keywords = value
                BuildTree()
            End Set
        End Property


        ''' <summary>
        ''' Searches passed text and returns all occurrences of any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>Array of occurrences</returns>
        Public Function FindAll(ByVal text As String) As List(Of StringSearchResult) Implements IStringSearchAlgorithm.FindAll
            Dim ret As New List(Of StringSearchResult)
            Dim ptr As TreeNode = _root
            Dim index As Integer = 0

            While index < text.Length
                Dim trans As TreeNode = Nothing
                While trans Is Nothing
                    trans = ptr.GetTransition(text(index))
                    If Object.ReferenceEquals(ptr, _root) Then
                        Exit While
                    End If
                    If trans Is Nothing Then
                        ptr = ptr.Failure
                    End If
                End While
                If trans IsNot Nothing Then
                    ptr = trans
                End If

                For Each found As String In ptr.Results
                    ret.Add(New StringSearchResult(index - found.Length + 1, found))
                Next
                index += 1
            End While
            Return ret
        End Function


        ''' <summary>
        ''' Searches passed text and returns first occurrence of any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
        Public Function FindFirst(ByVal text As String) As StringSearchResult Implements IStringSearchAlgorithm.FindFirst
            'Dim ret As New List(Of StringSearchResult)
            Dim ptr As TreeNode = _root
            Dim index As Integer = 0

            While index < text.Length
                Dim trans As TreeNode = Nothing
                While trans Is Nothing
                    trans = ptr.GetTransition(text(index))
                    If Object.ReferenceEquals(ptr, _root) Then
                        Exit While
                    End If
                    If trans Is Nothing Then
                        ptr = ptr.Failure
                    End If
                End While
                If trans IsNot Nothing Then
                    ptr = trans
                End If

                For Each found As String In ptr.Results
                    Return New StringSearchResult(index - found.Length + 1, found)
                Next
                index += 1
            End While
            Return StringSearchResult.Empty
        End Function


        ''' <summary>
        ''' Searches passed text and returns true if text contains any keyword
        ''' </summary>
        ''' <param name="text">Text to search</param>
        ''' <returns>True when text contains any keyword</returns>
        Public Function ContainsAny(ByVal text As String) As Boolean Implements IStringSearchAlgorithm.ContainsAny
            Dim ptr As TreeNode = _root
            Dim index As Integer = 0

            While index < text.Length
                Dim trans As TreeNode = Nothing
                While trans Is Nothing
                    trans = ptr.GetTransition(text(index))
                    If Object.ReferenceEquals(ptr, _root) Then
                        Exit While
                    End If
                    If trans Is Nothing Then
                        ptr = ptr.Failure
                    End If
                End While
                If trans IsNot Nothing Then
                    ptr = trans
                End If

                'If ptr.Results.Length > 0 Then
                If ptr.Results.Count > 0 Then
                    Return True
                End If
                index += 1
            End While
            Return False
        End Function

#End Region
    End Class
End Namespace