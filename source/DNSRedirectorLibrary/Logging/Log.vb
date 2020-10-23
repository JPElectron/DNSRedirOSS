
Namespace Logging

    ''' <summary>
    ''' Facillitates notification of events that may be added to a loggin store
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Log


        Public Event EventNotification As EventNotificationEventHandler '(ByVal sender As Object, ByVal e As EventNotificationEventArgs)
        Public Delegate Sub EventNotificationEventHandler(ByVal sender As Object, ByVal e As EventNotificationEventArgs)

        Private _Sender As Object
        Private _EventTypes As EventType = EventType.Pending

        Private _PendingLogNotifications As New List(Of KeyValuePair(Of EventType, String))

        Public Property EventTypes() As EventType
            Get
                Return _EventTypes
            End Get
            Set(ByVal value As EventType)
                Dim OldEventTypes As EventType = _EventTypes
                _EventTypes = value
                If OldEventTypes = EventType.Pending AndAlso Not _EventTypes = EventType.Pending Then
                    'Notify the pending events for whatever event type the log has been changed to
                    For Each Entry As KeyValuePair(Of EventType, String) In _PendingLogNotifications
                        NotifyEvent(Entry.Key, Entry.Value)
                    Next
                    _PendingLogNotifications.Clear()
                End If
            End Set
        End Property

        Public Sub New(ByVal sender As Object, Optional ByVal messageTypes As EventType = EventType.All)
            _Sender = sender
            _EventTypes = messageTypes
        End Sub


        Public Sub NotifyEvent(ByVal type As EventType, ByVal message As String)
            Debug.WriteLine(message)
            'Only raise event types that the log is setup to raise
            If _EventTypes = EventType.Pending Then
                'store the logged events until we know what kind of logging we will be doing.
                _PendingLogNotifications.Add(New KeyValuePair(Of EventType, String)(type, message))
            Else
                If type And _EventTypes Then
                    RaiseEvent EventNotification(_Sender, New EventNotificationEventArgs(type, message))
                End If
            End If
        End Sub


    End Class

    Public Class EventNotificationEventArgs
        Inherits EventArgs

        Public Message As String
        Public NotificaitonType As EventType

        Public Sub New(ByVal type As EventType, ByVal notificationMessage As String)
            Message = notificationMessage
            NotificaitonType = type
        End Sub
    End Class

    <Flags()> _
    Public Enum EventType
        Pending = -1
        None
        Information
        InformationVerbose
        Warning
        [Error]
        ErrorVerbose

        'Make sure to append an Or with every value of the enum
        All = Information Or InformationVerbose Or Warning Or [Error] Or ErrorVerbose
    End Enum
End Namespace
