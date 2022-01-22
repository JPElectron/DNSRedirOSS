Imports System.ComponentModel

Namespace Collections

    ''' <summary>
    ''' A collection that can be bound to Form UI controls.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class SortableBindingList(Of T)
        Inherits System.ComponentModel.BindingList(Of T)

        Private _Sorted As Boolean = False
        Private _SortDirection As ListSortDirection
        Private _SortProperty As PropertyDescriptor

#Region "Sorting Support"
        Protected Overrides Sub ApplySortCore(ByVal prop As System.ComponentModel.PropertyDescriptor, _
          ByVal direction As System.ComponentModel.ListSortDirection)

            ' Get list to sort
            Dim items As List(Of T) = TryCast(Me.Items, List(Of T))

            ' Apply and set the sort, if items to sort
            If items IsNot Nothing Then

                _SortDirection = direction
                _SortProperty = prop

                Dim pc As PropertyComparer(Of T) = _
                  New PropertyComparer(Of T)(prop, direction)

                items.Sort(pc)
                _Sorted = True

            Else
                _Sorted = False

            End If

            Me.OnListChanged(New ListChangedEventArgs(ListChangedType.Reset, -1))
        End Sub

        Protected Overrides ReadOnly Property SupportsSortingCore() As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property IsSortedCore() As Boolean
            Get
                Return _Sorted
            End Get
        End Property

        Protected Overrides Sub RemoveSortCore()
            _Sorted = False
        End Sub

        Protected Overrides ReadOnly Property SortDirectionCore() As System.ComponentModel.ListSortDirection
            Get
                Return _SortDirection
            End Get
        End Property

        Protected Overrides ReadOnly Property SortPropertyCore() As System.ComponentModel.PropertyDescriptor
            Get
                Return _SortProperty
            End Get
        End Property

#End Region

    End Class
End Namespace

