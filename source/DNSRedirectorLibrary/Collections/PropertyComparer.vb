Imports System.ComponentModel

Namespace Collections

    ''' <summary>
    ''' Provides generic comparison for types to facilitate sorting.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class PropertyComparer(Of T)
        Inherits System.Collections.Generic.Comparer(Of T)

        Private _property As PropertyDescriptor
        Private _direction As ListSortDirection

        Public Sub New(ByVal [property] As PropertyDescriptor, ByVal direction As ListSortDirection)
            _property = [property]
            _direction = direction
        End Sub

#Region "IComparer<T>"
        Public Overrides Function Compare(ByVal xWord As T, ByVal yWord As T) As Integer
            ' Get property values
            Dim xValue As Object = GetPropertyValue(xWord, _property.Name)
            Dim yValue As Object = GetPropertyValue(yWord, _property.Name)
            ' Determine sort order
            If _direction = ListSortDirection.Ascending Then
                Return CompareAscending(xValue, yValue)
            Else
                Return CompareDescending(xValue, yValue)
            End If
        End Function

        Public Overloads Function Equals(ByVal xWord As T, ByVal yWord As T) As Boolean
            Return xWord.Equals(yWord)
        End Function

        Public Overloads Function GetHashCode(ByVal obj As T) As Integer
            Return obj.GetHashCode()
        End Function
#End Region

        ' Compare two property values of any type
        Private Function CompareAscending(ByVal xValue As Object, ByVal yValue As Object) As Integer
            Dim result As Integer
            ' If values implement IComparer
            If TypeOf xValue Is IComparable Then
                result = (DirectCast(xValue, IComparable)).CompareTo(yValue)
            Else
                If xValue.Equals(yValue) Then
                    ' If values don't implement IComparer but are equivalent
                    result = 0
                Else
                    result = xValue.ToString().CompareTo(yValue.ToString())
                End If
                ' Values don't implement IComparer and are not equivalent, so compare as string values
            End If

            Return result
        End Function
        Private Function CompareDescending(ByVal xValue As Object, ByVal yValue As Object) As Integer
            ' Return result adjusted for ascending or descending sort order ie
            ' multiplied by 1 for ascending or -1 for descending
            Return CompareAscending(xValue, yValue) * -1
        End Function

        Private Function GetPropertyValue(ByVal value As T, ByVal [property] As String) As Object
            ' Get property
            Dim propertyInfo As Reflection.PropertyInfo = value.[GetType]().GetProperty([property])
            ' Return value
            Return propertyInfo.GetValue(value, Nothing)
        End Function
    End Class
End Namespace

