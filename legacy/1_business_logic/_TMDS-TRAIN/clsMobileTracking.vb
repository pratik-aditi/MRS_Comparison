Imports System.Timers
Imports TMDS.GraphicComponent : Imports TMDS.TrainComponents : Imports TMDS.TrackComponents : Imports TMDS.SwitchComponents : Imports TMDS.SignalComponents : Imports TMDS.TrackTagComponents : Imports TMDS.TrackOtherComponents
Imports TMDS.ManagementClasses
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.NetworkListener


Namespace MobileTracking

    Public Delegate Sub UpdatePositionData(ByVal PosData() As String)

#Region "MOBILE VEHICLE CLASS"

    Public Class Vehicle

#Region "ENUMS"

        Public Enum AlarmStatuses
            Exceed
            NearLimits
            NearerLimits
            OK
        End Enum

        Public Enum MessageStatuses
            DataError
            ThumbWheelError
            OK
        End Enum

#End Region

#Region "FIELDS"

        Public VehicleID As String = String.Empty
        Public TerrID As Int32
        Public TrackGUID As Int32
        Public thisCurrentTrack As Track
        Public MilePost As String
        Public MilePostSuffix As String
        Public Subdivision As String
        Public DirectionOfTravel As String
        Public DirectionOfTimeTable As String
        Public Speed As String
        Public TrackName As String
        Public ReportTime As Date
        Public TimeZone As String
        Public HyrailMode As String
        Public ThumbWheel As String
        Public ThumbWheelError As Boolean
        Public TrackAuthType As String
        Public Emergency As Boolean
        Public EmergencyPreviouslyReceived As Boolean
        Public ServerUID As Integer
        Public MessageID As Date
        Public CurrentTrackDesignator As String
        Public LastMilepostPosition As Decimal
        Public CalculatedDirectionOfTravel As String
        Public LastTrackGUID As Int32
        Dim Dither As Decimal = 0.02

        'Public AlarmStatus As AlarmStatuses
        Public pAlarmStatus As AlarmStatuses
        Public MessageStatus As MessageStatuses
        Public CurrentMilePost As Decimal

        Private vehicles As ArrayList
        Private events As Queue
        Private thisVehicle As Vehicle
        Private transmitQueue As Queue
        Private thisWrkStn As WorkStation
        Private KeepAlive As Timers.Timer
        Public PreviouslyExceeded As Boolean
        Public StartNewExceedAlarm As Boolean
        Public PreviousTrack As Track
        Public ExceedAlert As Alert
        Public AuthUID As String = 0
        Public AuthIssueTo As String = String.Empty
        Public AuthMobileTerminalUID As String = String.Empty

        Public Property AlarmStatus() As AlarmStatuses
            Get
                Return pAlarmStatus
            End Get
            Set(ByVal Value As AlarmStatuses)
                pAlarmStatus = Value
            End Set
        End Property
#End Region

#Region "NEW VEHICLE"

        Sub New(ByRef VehicleCollection As ArrayList,
                ByVal PosData() As String, ByRef EventQueue As Queue,
                ByRef XmtQueue As Queue, ByVal thisWorkstation As WorkStation)

            Try
                vehicles = VehicleCollection
                events = EventQueue
                transmitQueue = XmtQueue
                thisWrkStn = thisWorkstation
                thisVehicle = Me
                KeepAlive = New Timers.Timer()
                'Me.UpdatePosition(PosData)
                AddHandler KeepAlive.Elapsed, AddressOf KeepAliveElasped

                KeepAlive.Interval = 15000 '120000 ' 2 MINUTES
                KeepAlive.Start()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "VEHICLE EVENTS"

        Private Sub KeepAliveElasped(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                Dim TimeOfReport As Date
                Select Case Me.TimeZone
                    Case "M"
                        TimeOfReport = Me.ReportTime.AddHours(1)
                    Case "P"
                        TimeOfReport = Me.ReportTime.AddHours(2)
                    Case Else
                        TimeOfReport = Me.ReportTime
                End Select

                If TimeOfReport < Now.AddMinutes(-2) Then
                    Me.AlarmStatus = AlarmStatuses.OK
                    'Me.SendResponseToReport()
                    Me.KeepAlive.Enabled = False
                    Me.Dispose()
                Else
                    Me.KeepAlive.Enabled = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Me.Dispose(True)
            End Try
        End Sub

        Public Sub Dispose(Optional ByVal ForceDisposal As Boolean = False)
            Try
                Me.MobileEvent("TERMINATE HLCS VEHICLE:" & Me.VehicleID)

                Try
                    RemoveHandler KeepAlive.Elapsed, AddressOf KeepAliveElasped
                Catch
                End Try

                SyncLock vehicles
                    Me.vehicles.Remove(Me)
                End SyncLock
                Me.ClearExceedAlarm(True)
                'TMDS.SystemForms.MySystemForms.frmSystemSummary.UpdateMobileSummary(Me, True)
                If Not Me.KeepAlive Is Nothing Then
                    Me.KeepAlive.Stop()
                    Me.KeepAlive.Enabled = False
                End If
                Me.KeepAlive = Nothing
                Me.thisVehicle = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


#End Region

#Region "POSITION REPORT"

        Public Sub UpdatePosition(ByVal ReportData() As String)
            Try
                If ReportData.Length < 16 Then
                    Dim ErrorString As String = String.Empty
                    Dim i As Integer
                    For i = 0 To ReportData.Length - 1
                        ErrorString = ErrorString & "(" & ReportData(i) & ") "
                    Next
                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, "REPORT ERROR:" & ErrorString, events)
                    Exit Sub
                End If
                With thisVehicle
                    'If IsNumeric(ReportData(0)) Then
                    .VehicleID = ReportData(0)
                    'Else
                    '    Exit Sub
                    'End If

                    If IsNumeric(ReportData(1)) Then
                        .TerrID = ReportData(1)
                    Else
                        Exit Sub
                    End If

                    If IsNumeric(ReportData(2)) Then
                        .TrackGUID = ReportData(2)
                    Else
                        Exit Sub
                    End If

                    If IsNumeric(ReportData(15)) Then
                        .ServerUID = ReportData(15)
                    Else
                        Exit Sub
                    End If

                    .MilePost = ReportData(3)
                    .MilePostSuffix = ReportData(4)
                    .Subdivision = ReportData(5)
                    .DirectionOfTravel = ReportData(6)
                    .DirectionOfTimeTable = ReportData(7)
                    .Speed = ReportData(8)
                    .TrackName = ReportData(9)
                    .ReportTime = ReportData(10)
                    .TimeZone = ReportData(11)
                    .HyrailMode = ReportData(12).ToUpper
                    .ThumbWheel = ReportData(13)
                    .TrackAuthType = ReportData(14)
                    Try
                        If ReportData.Length >= 15 Then
                            .Emergency = CBool(ReportData(16).ToUpper)
                        End If
                    Catch
                    End Try
                End With

                Me.CheckPositionReport()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckPositionReport()
            Try
                Dim CurrentTrack As Track = FindCurrentTrack()

                'check for emergency flag
                If Me.Emergency Then
                    Dim DataString As String = Me.VehicleID & "|" & Format(Me.ReportTime, "MM-dd-yy HH:mm:ss")
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(Alert.AlertType.EmergencyAlarm, DataString, Nothing, Me, Nothing, Nothing)
                End If

                If Me.ThumbWheel = "NA" Then
                    Me.AlarmStatus = AlarmStatuses.OK
                    Me.MessageStatus = MessageStatuses.OK
                    If Not Me.ExceedAlert Is Nothing Then
                        ClearExceedAlarm()
                    Else
                        SendResponseToReport()
                    End If
                    'added in 317
                    If Me.HyrailMode = "FALSE" Then
                        Me.Dispose()
                    End If
                    Exit Sub
                End If

                If Me.HyrailMode = "FALSE" Then
                    Me.AlarmStatus = AlarmStatuses.OK
                    Me.MessageStatus = MessageStatuses.OK
                    If Not Me.ExceedAlert Is Nothing Then
                        Me.Dispose()
                        Exit Sub
                    Else
                        Me.Dispose()
                        Exit Sub
                    End If
                End If


                If CurrentTrack Is Nothing Then
                    Me.MessageStatus = MessageStatuses.DataError
                    SetExceedAlarm()
                    'TryToMoveMarker(True)
                    Exit Sub
                Else
                    thisCurrentTrack = CurrentTrack
                    If IsNumeric(Me.MilePost) Then
                        CurrentMilePost = CDec(Me.MilePost)
                        Me.MessageStatus = MessageStatuses.OK
                        'TryToMoveMarker(False)
                    Else
                        Me.MessageStatus = MessageStatuses.DataError
                        SetExceedAlarm()
                        'TryToMoveMarker(False)
                        Exit Sub
                    End If
                End If

                Me.CurrentTrackDesignator = ""

                If optEnableTrackSidingBit Then
                    'NEW SIDING LOGIC CODE
                    If CurrentTrack.EnableControlledSiding Or CurrentTrack.EnableNonControlledSiding Then
                        Me.CurrentTrackDesignator = "S"
                    Else
                        Select Case CurrentTrack.TrackName.ToUpper
                            Case "MAIN", "MAIN TRACK"
                                Me.CurrentTrackDesignator = "M"
                            Case "MAIN 1", "MAIN ONE"
                                Me.CurrentTrackDesignator = "M1"
                            Case "MAIN 2", "MAIN TWO"
                                Me.CurrentTrackDesignator = "M2"
                            Case "MAIN 3", "MAIN THREE"
                                Me.CurrentTrackDesignator = "M3"
                            Case "MAIN 4", "MAIN FOUR"
                                Me.CurrentTrackDesignator = "M4"
                            Case "MAIN 5", "MAIN FIVE"
                                Me.CurrentTrackDesignator = "M5"
                            Case "MAIN 6", "MAIN SIX"
                                Me.CurrentTrackDesignator = "M6"
                            Case Else
                                Me.CurrentTrackDesignator = ""
                        End Select
                    End If
                Else
                    'OLD LEGACY SIDING CODE
                    Select Case CurrentTrack.TrackName.ToUpper
                        Case "SIDING"
                            Me.CurrentTrackDesignator = "S"
                        Case "MAIN", "MAIN TRACK"
                            Me.CurrentTrackDesignator = "M"
                        Case "MAIN 1", "MAIN ONE"
                            Me.CurrentTrackDesignator = "M1"
                        Case "MAIN 2", "MAIN TWO"
                            Me.CurrentTrackDesignator = "M2"
                        Case "MAIN 3", "MAIN THREE"
                            Me.CurrentTrackDesignator = "M3"
                        Case "MAIN 4", "MAIN FOUR"
                            Me.CurrentTrackDesignator = "M4"
                        Case "MAIN 5", "MAIN FIVE"
                            Me.CurrentTrackDesignator = "M5"
                        Case "MAIN 6", "MAIN SIX"
                            Me.CurrentTrackDesignator = "M6"
                        Case Else
                            Me.CurrentTrackDesignator = ""
                    End Select
                End If


                If Me.CurrentTrackDesignator = "" Then
                    Me.CurrentTrackDesignator = CurrentTrack.sAlternateTrackAbbreviation
                End If


                If Not CheckForAuthority(CurrentTrack) Then
                    If Not NearTracksCheck(CurrentTrack) Then
                        SetExceedAlarm()
                        Exit Sub
                    Else
                        If Not Me.ExceedAlert Is Nothing Then
                            ClearExceedAlarm()
                            Exit Sub
                        Else
                            SendResponseToReport()
                            Exit Sub
                        End If
                    End If
                Else
                    If Not Me.ExceedAlert Is Nothing Then
                        ClearExceedAlarm()
                    Else
                        SendResponseToReport()
                        Exit Sub
                    End If
                End If

                If CurrentTrack.TrackAuthorities.Count = 0 Then
                    'If Not NearTrackCheck(CurrentTrack) Then
                    If Not NearTracksCheck(CurrentTrack) Then
                        SetExceedAlarm()
                        Exit Sub
                    Else
                        If Not Me.ExceedAlert Is Nothing Then
                            ClearExceedAlarm()
                            Exit Sub
                        Else
                            SendResponseToReport()
                            Exit Sub
                        End If
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "PROCESS POSITION REPORT"
        Private Sub AssociateUserFromAuthority(ByVal auth As Object)
            Try
                AuthIssueTo = ""
                AuthMobileTerminalUID = ""
                If auth.GetType.ToString = "TMDS.TrackAuthority.SFAuthority" Then
                    Dim sfa As TrackAuthority.SFAuthority = CType(auth, TrackAuthority.SFAuthority)
                    AuthIssueTo = sfa.IssueTo
                    If Not IsNothing(sfa.MobileClientForeman.TerminalID) Then
                        AuthMobileTerminalUID = sfa.MobileClientForeman.TerminalID
                    End If
                ElseIf auth.GetType.ToString = "TMDS.TrackAuthority.TrackAuthority" Then
                    Dim tnt As TrackAuthority.TrackAuthority = CType(auth, TrackAuthority.TrackAuthority)
                    AuthIssueTo = tnt.IssueTo
                    If tnt IsNot Nothing AndAlso tnt.MobileClientForeman IsNot Nothing AndAlso Not IsNothing(tnt.MobileClientForeman.TerminalID) Then
                        AuthMobileTerminalUID = tnt.MobileClientForeman.TerminalID
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckForAuthority(ByVal trkTocheck As Track) As Boolean
            Dim auth As Object

            Try
                Me.AuthUID = 0
                Dim AscendingTrack As Boolean
                If trkTocheck.LeftLimitMilePost < trkTocheck.RightLimitMilePost Then
                    AscendingTrack = True
                End If

                For Each auth In trkTocheck.TrackAuthorities

                    If Not auth.VehicleAssociations Is Nothing Then
                        If auth.VehicleAssociations.Count > 0 Then
                            Dim VehID As String
                            For Each VehID In auth.VehicleAssociations
                                If VehID = Me.VehicleID.ToString Then
                                    If Me.ThumbWheel = Me.CurrentTrackDesignator Then
                                        Me.AuthUID = auth.AuthorityUID
                                        AssociateUserFromAuthority(auth)
                                        'check to make sure milepost position fall between the milepost range of the track
                                        If AscendingTrack Then
                                            If Me.CurrentMilePost + Dither >= trkTocheck.LeftLimitMilePost And Me.CurrentMilePost - Dither <= trkTocheck.RightLimitMilePost Then
                                                Me.MessageStatus = MessageStatuses.OK
                                            Else
                                                Me.MessageStatus = MessageStatuses.DataError
                                            End If
                                        Else
                                            If Me.CurrentMilePost - Dither <= trkTocheck.LeftLimitMilePost And Me.CurrentMilePost + Dither >= trkTocheck.RightLimitMilePost Then
                                                Me.MessageStatus = MessageStatuses.OK
                                            Else
                                                Me.MessageStatus = MessageStatuses.DataError
                                            End If
                                        End If
                                        If Me.MessageStatus = MessageStatuses.DataError Then
                                            Return False
                                        End If

                                        Me.CheckForNearLimitsCondition(trkTocheck)
                                        Return True
                                    Else
                                        Me.MessageStatus = MessageStatuses.ThumbWheelError
                                        Return False
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Sub CheckForNearLimitsCondition(ByVal trk As Track)
            Try
                Dim AscendingLeftToRight As Boolean
                Dim DitherSmall As Decimal = 0.05
                Dim ApproachDistance As Decimal = 1.2
                Dim CurrentMilepost As Decimal
                If IsNumeric(Me.MilePost) Then
                    CurrentMilepost = CDec(Me.MilePost)
                Else
                    Exit Sub
                End If


                Me.MobileEvent("POSITION REPORT - TRACK GUID:" & trk.GUID.ToString & " MILEPOST:" & CurrentMilepost.ToString)

                'Find the direction of the track being checking the reported track
                If trk.LeftLimitMilePost <= trk.RightLimitMilePost Then ' mp 0 to mp 10 --->
                    AscendingLeftToRight = True
                Else
                    AscendingLeftToRight = False    'mp 10 to mp 0 <----
                End If

                'Find the direction of the vehicle
                If Me.LastTrackGUID = Me.TrackGUID Then
                    'Me.CalculatedDirectionOfTravel = ""
                    If AscendingLeftToRight Then
                        If Me.LastMilepostPosition <= CurrentMilepost Then
                            If CurrentMilepost >= Me.LastMilepostPosition + DitherSmall Then
                                Me.CalculatedDirectionOfTravel = "RIGHT"
                                'VehicleAscendingLeftToRight = True 'mp 0 to mp 10 --->
                            Else
                                Me.CalculatedDirectionOfTravel = ""
                            End If
                        ElseIf Me.LastMilepostPosition >= CurrentMilepost Then
                            If CurrentMilepost <= Me.LastMilepostPosition - DitherSmall Then ' change from add to subtract
                                'VehicleAscendingLeftToRight = False 'mp 10 to mp 0 <----
                                Me.CalculatedDirectionOfTravel = "LEFT"
                            Else
                                Me.CalculatedDirectionOfTravel = ""
                            End If
                        End If
                    Else
                        If Me.LastMilepostPosition <= CurrentMilepost Then
                            If CurrentMilepost >= Me.LastMilepostPosition + DitherSmall Then
                                Me.CalculatedDirectionOfTravel = "LEFT"
                                'VehicleAscendingLeftToRight = True 'mp 0 to mp 10 --->
                            Else
                                Me.CalculatedDirectionOfTravel = ""
                            End If
                        ElseIf Me.LastMilepostPosition >= CurrentMilepost Then
                            If CurrentMilepost <= Me.LastMilepostPosition - DitherSmall Then 'change from add to subtract
                                'VehicleAscendingLeftToRight = False 'mp 10 to mp 0 <----
                                Me.CalculatedDirectionOfTravel = "RIGHT"
                            Else
                                Me.CalculatedDirectionOfTravel = ""
                            End If
                        End If

                    End If
                End If

                Dim DistanceToEndOfLimits As Decimal
                If AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "RIGHT" Then
                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.RightLink, "RIGHT")

                    If DistanceToEndOfLimits <= 1.2 Then
                        If CurrentMilepost >= trk.RightLimitMilePost - ApproachDistance + DistanceToEndOfLimits Then
                            Dim Difference As Decimal = trk.RightLimitMilePost - CurrentMilepost + DistanceToEndOfLimits
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If
                ElseIf AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "LEFT" Then
                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.LeftLink, "LEFT")
                    If DistanceToEndOfLimits <= 1.2 Then
                        If CurrentMilepost <= trk.LeftLimitMilePost + ApproachDistance - DistanceToEndOfLimits Then
                            Dim Difference As Decimal = CurrentMilepost - trk.LeftLimitMilePost + DistanceToEndOfLimits
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If

                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If
                ElseIf Not AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "LEFT" Then
                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.LeftLink, "LEFT")
                    If DistanceToEndOfLimits <= 1.2 Then
                        If CurrentMilepost >= trk.LeftLimitMilePost - ApproachDistance + DistanceToEndOfLimits Then
                            Dim Difference As Decimal = trk.LeftLimitMilePost + DistanceToEndOfLimits - CurrentMilepost
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If
                ElseIf Not AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "RIGHT" Then
                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.RightLink, "RIGHT")

                    If DistanceToEndOfLimits <= 1.2 Then
                        'If CurrentMilepost <= trk.RightLimitMilePost + ApproachDistance + DistanceToEndOfLimits Then
                        If CurrentMilepost <= trk.RightLimitMilePost + ApproachDistance - DistanceToEndOfLimits Then
                            Dim Difference As Decimal = CurrentMilepost - trk.RightLimitMilePost + DistanceToEndOfLimits
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If
                ElseIf Not AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "" Then

                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.RightLink, "RIGHT")
                    If DistanceToEndOfLimits <= 1.2 Then
                        'If CurrentMilepost <= trk.RightLimitMilePost + ApproachDistance + DistanceToEndOfLimits Then
                        If CurrentMilepost <= trk.RightLimitMilePost + ApproachDistance - DistanceToEndOfLimits Then
                            Dim Difference As Decimal = CurrentMilepost - trk.RightLimitMilePost + DistanceToEndOfLimits
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If

                    If Me.AlarmStatus = AlarmStatuses.OK Then
                        DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.LeftLink, "LEFT")
                        If DistanceToEndOfLimits <= 1.2 Then
                            If CurrentMilepost >= trk.LeftLimitMilePost - ApproachDistance + DistanceToEndOfLimits Then
                                Dim Difference As Decimal = trk.LeftLimitMilePost + DistanceToEndOfLimits - CurrentMilepost
                                If Difference <= 0.5 Then
                                    Me.AlarmStatus = AlarmStatuses.NearerLimits
                                Else
                                    Me.AlarmStatus = AlarmStatuses.NearLimits
                                End If
                            Else
                                Me.AlarmStatus = AlarmStatuses.OK
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    End If
                ElseIf AscendingLeftToRight And Me.CalculatedDirectionOfTravel = "" Then
                    DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.RightLink, "RIGHT")
                    If DistanceToEndOfLimits <= 1.2 Then
                        If CurrentMilepost >= trk.RightLimitMilePost - ApproachDistance + DistanceToEndOfLimits Then
                            Dim Difference As Decimal = trk.RightLimitMilePost - CurrentMilepost + DistanceToEndOfLimits
                            If Difference <= 0.5 Then
                                Me.AlarmStatus = AlarmStatuses.NearerLimits
                            Else
                                Me.AlarmStatus = AlarmStatuses.NearLimits
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    Else
                        Me.AlarmStatus = AlarmStatuses.OK
                    End If

                    If Me.AlarmStatus = AlarmStatuses.OK Then
                        DistanceToEndOfLimits = DoesVehicleHaveAuthorityOnNextTrack(trk.LeftLink, "LEFT")
                        If DistanceToEndOfLimits <= 1.2 Then
                            If CurrentMilepost <= trk.LeftLimitMilePost + ApproachDistance - DistanceToEndOfLimits Then
                                Dim Difference As Decimal = CurrentMilepost - trk.LeftLimitMilePost + DistanceToEndOfLimits
                                If Difference <= 0.5 Then
                                    Me.AlarmStatus = AlarmStatuses.NearerLimits
                                Else
                                    Me.AlarmStatus = AlarmStatuses.NearLimits
                                End If
                            Else
                                Me.AlarmStatus = AlarmStatuses.OK
                            End If
                        Else
                            Me.AlarmStatus = AlarmStatuses.OK
                        End If
                    End If

                Else
                    Me.AlarmStatus = AlarmStatuses.OK
                End If

                Me.MobileEvent("CurrentMilepost=" & CurrentMilepost.ToString & "|LastMilepostPosition=" & LastMilepostPosition.ToString & "|CalculatedDirectionOfTravel=" & CalculatedDirectionOfTravel & "|DistanceToEndOfLimits=" & DistanceToEndOfLimits.ToString & "|AlarmStatus=" & AlarmStatus.ToString)

                Me.LastMilepostPosition = CDec(Me.MilePost)
                Me.LastTrackGUID = Me.TrackGUID

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function DoesVehicleHaveAuthorityOnNextTrack(ByVal GUID As Int32, ByVal DirectionToSearch As String) As Decimal
            Try

                Dim TrackFound As Boolean
                Dim VehicleFound As Boolean
                Dim TrackList As New ArrayList()
                Dim LoopControl As Integer
                Dim NextComp As Object = Nothing
                If Me.thisCurrentTrack.NonTransverable And Me.thisCurrentTrack.OStype Then
                    Dim ParallelComp As Track = FindParallelTrackComponent(Me.thisCurrentTrack)
                    If Not ParallelComp Is Nothing Then
                        NextComp = ParallelComp
                    End If
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(GUID)
                End If

                Dim Comp As Object = Me.thisCurrentTrack
                Do While Not TrackFound
                    If NextComp Is Nothing Then
                        Exit Do
                    End If
                    If gDebugMobileTracking Then Me.MobileEvent("AUTH SEARCH - COMP TYPE:" & NextComp.GetType.ToString & "|COMP GUID:" & NextComp.guid.ToString)
                    If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Comp Is Nothing Then Return Nothing
                        Dim trk1 As Track = CType(NextComp, Track)
                        If trk1.Name = Comp.Name And trk1.StationNumber = Comp.StationNumber And trk1.LeftLimitMilePost = Comp.LeftLimitMilePost Then

                            If DirectionToSearch = "RIGHT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If
                        Else
                            Dim trkToCheck As Track = NextComp
                            If trkToCheck.TrackAuthorities.Count = 0 Then
                                Exit Do
                            Else
                                Dim VehNotFound As Boolean = True
                                Dim auth As Object
                                For Each auth In trkToCheck.TrackAuthorities
                                    Dim vehToCheck As String
                                    If Not auth.VehicleAssociations Is Nothing Then
                                        For Each vehToCheck In auth.VehicleAssociations
                                            If vehToCheck.ToString = Me.VehicleID Then
                                                VehicleFound = True
                                                VehNotFound = False
                                                If Not TrackList.Contains(NextComp) Then
                                                    TrackList.Add(NextComp)
                                                End If
                                                If gDebugMobileTracking Then Me.MobileEvent("VEHICLE FOUND:" & trkToCheck.Name & "|TRACK GUID:" & trkToCheck.GUID.ToString)
                                                Exit For
                                            End If
                                        Next
                                    End If
                                    If VehicleFound Then
                                        VehicleFound = False
                                    End If
                                Next
                                If VehNotFound Then
                                    Exit Do
                                End If
                                If DirectionToSearch = "RIGHT" Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If
                            End If
                        End If
                    ElseIf NextComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        If NextComp.ControlledReverse Then
                            If DirectionToSearch = "RIGHT" Then
                                If NextComp.RightAltLink <> 0 Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                                End If
                            Else
                                If NextComp.LeftAltLink <> 0 Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                                End If
                            End If
                        Else
                            If DirectionToSearch = "RIGHT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                            End If
                        End If
                    Else
                        If DirectionToSearch = "RIGHT" Then
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                        End If

                    End If
                    LoopControl = LoopControl + 1
                    If LoopControl > 50 Then
                        Exit Do
                    End If
                Loop

                Dim trk As Track
                Dim TotalMiles As Decimal
                For Each trk In TrackList
                    If gDebugMobileTracking Then Me.MobileEvent("TRACK LIST - COMP GUID:" & trk.GUID.ToString)
                    If trk.RightLimitMilePost >= trk.LeftLimitMilePost Then
                        TotalMiles = TotalMiles + (trk.RightLimitMilePost - trk.LeftLimitMilePost)
                    Else
                        TotalMiles = TotalMiles + (trk.LeftLimitMilePost - trk.RightLimitMilePost)
                    End If
                    If gDebugMobileTracking Then Me.MobileEvent("TOTAL MILES:" & TotalMiles.ToString)
                Next
                If TrackList.Count > 0 Then
                    Me.MobileEvent("TOTAL MILES:" & TotalMiles.ToString)
                End If
                TrackList = Nothing

                Return TotalMiles
            Catch ex As Exception
                SystemException(ex.ToString)
                Return 0
            End Try
        End Function

        Private Function NearTracksCheck(ByVal CurrentTrack As Track) As Boolean
            Dim RTrackToCheck As New ArrayList()
            Dim LTrackToCheck As New ArrayList()
            Dim comp As Track
            Try
                If Not Me.thisCurrentTrack Is Nothing Then
                    If (CurrentMilePost >= Me.thisCurrentTrack.RightLimitMilePost And CurrentMilePost <= Me.thisCurrentTrack.LeftLimitMilePost) Then
                        If CurrentMilePost <= Me.thisCurrentTrack.RightLimitMilePost + Dither Then
                            RTrackToCheck = Me.FindNearTracks(CurrentTrack.RightLink, "RIGHT")
                            'Return True
                        End If

                        If CurrentMilePost >= Me.thisCurrentTrack.LeftLimitMilePost - Dither Then
                            LTrackToCheck = Me.FindNearTracks(CurrentTrack.LeftLink, "LEFT")
                            'Return True
                        End If

                    ElseIf CurrentMilePost <= Me.thisCurrentTrack.RightLimitMilePost And CurrentMilePost >= Me.thisCurrentTrack.LeftLimitMilePost Then
                        If CurrentMilePost >= Me.thisCurrentTrack.RightLimitMilePost - Dither Then
                            RTrackToCheck = Me.FindNearTracks(CurrentTrack.RightLink, "RIGHT")
                            'Return True
                        End If

                        If CurrentMilePost <= Me.thisCurrentTrack.LeftLimitMilePost + Dither Then
                            LTrackToCheck = Me.FindNearTracks(CurrentTrack.LeftLink, "LEFT")
                            'Return True
                        End If
                    End If
                End If

                If Not LTrackToCheck Is Nothing Then
                    For Each comp In LTrackToCheck
                        If Me.CheckForNearAuthority(comp) Then
                            Return True
                        End If
                    Next
                End If

                If Not RTrackToCheck Is Nothing Then
                    For Each comp In RTrackToCheck
                        If Me.CheckForNearAuthority(comp) Then
                            Return True
                        End If
                    Next
                End If


                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Function FindNearTracks(ByVal GUID As Int32, ByVal DirectionToSearch As String) As ArrayList
            Try
                Dim TrackFound As Boolean
                Dim LoopControl As Integer
                Dim CompList As New ArrayList()
                Dim NextComp As Object = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(GUID)
                Dim Comp As Object = Me.thisCurrentTrack
                If Me.thisCurrentTrack.OStype Then

                    Dim OSComp As Object
                    For Each OSComp In Me.thisCurrentTrack.OsComponentCollection
                        If DirectionToSearch = "RIGHT" Then
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(OSComp.RightLink)
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(OSComp.LeftLink)
                        End If
                        Do While Not TrackFound

                            If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                If Not Comp Is Nothing Then
                                    If NextComp.Name = Comp.Name And NextComp.StationNumber = Comp.StationNumber Then
                                        If DirectionToSearch = "RIGHT" Then
                                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                        End If
                                    Else
                                        If Not CompList.Contains(NextComp) Then
                                            CompList.Add(NextComp)
                                        End If
                                        Exit Do
                                    End If
                                Else
                                    Return Nothing
                                End If
                            Else
                                If DirectionToSearch = "RIGHT" Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If

                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then
                                LoopControl = 0
                                Exit Do
                            End If
                        Loop
                    Next

                Else
                    Do While Not TrackFound
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            If Not Comp Is Nothing Then
                                If NextComp.Name = Comp.Name And NextComp.StationNumber = Comp.StationNumber Then
                                    If DirectionToSearch = "RIGHT" Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    If Not CompList.Contains(NextComp) Then
                                        CompList.Add(NextComp)
                                    End If
                                    Return CompList
                                End If
                            Else
                                Return Nothing
                            End If
                        Else
                            If DirectionToSearch = "RIGHT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If

                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Return Nothing
                    Loop
                End If
                Return CompList
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function CheckForNearAuthority(ByVal trkTocheck As Track) As Boolean
            Dim auth As Object
            Try
                Me.AuthUID = 0
                For Each auth In trkTocheck.TrackAuthorities
                    If Not auth.VehicleAssociations Is Nothing Then
                        If auth.VehicleAssociations.Count > 0 Then
                            Dim VehID As String
                            For Each VehID In auth.VehicleAssociations
                                If VehID = Me.VehicleID.ToString Then
                                    Me.AuthUID = auth.AuthorityUID
                                    AssociateUserFromAuthority(auth)
                                    If Me.ThumbWheel = Me.CurrentTrackDesignator Then
                                        Me.AlarmStatus = AlarmStatuses.NearerLimits
                                        Return True
                                    Else
                                        Me.MessageStatus = MessageStatuses.ThumbWheelError
                                        Return False
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next

                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

#End Region

#Region "UTILITIES"

        Private Function FindCurrentTrack() As Track
            Try
                Dim trk As Track
                For Each trk In TMDS.GlobalDeclarations.TrackCollection
                    If trk.GUID = Me.TrackGUID Then
                        Return trk
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindParallelTrackComponent(ByVal trk As Track) As Track
            Try
                Dim comp As Object = Nothing
                Dim sw As Switch = Nothing
                For Each comp In trk.OsComponentCollection
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        sw = comp
                    End If
                Next
                If Not comp Is Nothing Then
                    If sw.ReverseRouteCollection.Contains(trk) Then
                        Return sw.NormalRouteCollection.Item(0)
                    ElseIf sw.NormalRouteCollection.Contains(trk) Then
                        Return sw.ReverseRouteCollection.Item(0)
                    End If
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

#End Region

#Region "ALARMS EVENTS"

        Private Sub SetExceedAlarm()
            Try
                If Me.ExceedAlert Is Nothing Then
                    Me.CreateAlert()
                End If
                PreviouslyExceeded = True
                Me.AlarmStatus = AlarmStatuses.Exceed
                SendResponseToReport()
                Me.LastMilepostPosition = CDec(Me.MilePost)
                Me.LastTrackGUID = Me.TrackGUID
                If Not Me.ExceedAlert Is Nothing Then
                    Me.UpdateAlert()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CreateAlert()
            Try
                Dim DataString As String = Me.VehicleID & "|" & Me.MilePost & Me.MilePostSuffix & "|" & Me.Subdivision & "|" & Me.Speed & "|" & Format(Me.ReportTime, "MM-dd-yy HH:mm:ss") & "|" & Me.ThumbWheelError.ToString.ToUpper
                Me.ExceedAlert = TMDS.ManagementClasses.AlertUtilities.CreateAlert(Alert.AlertType.ExceedAlarm, DataString, Nothing, Me, Nothing, Nothing)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub UpdateAlert()
            Try
                Dim DataString As String = Me.VehicleID & "|" & Me.MilePost & Me.MilePostSuffix & "|" & Me.Subdivision & "|" & Me.Speed & "|" & Format(Me.ReportTime, "MM-dd-yy HH:mm:ss") & "|" & Me.ThumbWheelError.ToString.ToUpper
                Me.ExceedAlert.AlertData = DataString
                'TMDS.SystemForms.MySystemForms.frmSystemSummary.UpdateAlert(Me.ExceedAlert)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub ClearExceedAlarm(Optional ByVal ClearVehicle As Boolean = False)
            Try

                PreviouslyExceeded = False
                If Me.AlarmStatus = AlarmStatuses.Exceed Then
                    Me.AlarmStatus = AlarmStatuses.OK
                End If
                If Not Me.ExceedAlert Is Nothing Then
                    Me.ExceedAlert = Nothing
                End If
                SendResponseToReport(ClearVehicle)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub SendResponseToReport(Optional ByVal ClearVehicle As Boolean = False)
            Try
                Dim AlarmStat As String = String.Empty
                Dim MessageStat As String = String.Empty

                'HLCS;REPORT ACK;[MM-dd-yy HH:mm:ss.fff];[DATA];[ASCII CHARACTER CODE-EOT]
                Dim response As String = "HLCS;REPORT ACK;" & Format(Now, "MM-dd-yy HH:mm:ss.fff") & ";"
                response = response & Me.VehicleID & "," & Me.TerrID.ToString & "," & Me.TrackGUID.ToString & ","
                ThumbWheelError = False
                Select Case Me.MessageStatus
                    Case MessageStatuses.DataError
                        MessageStat = "DATA ERROR"
                    Case MessageStatuses.OK
                        MessageStat = "OK"
                    Case MessageStatuses.ThumbWheelError
                        MessageStat = "THUMB WHEEL ERROR"
                        ThumbWheelError = True
                End Select

                response = response & MessageStat & ","

                Select Case Me.AlarmStatus
                    Case AlarmStatuses.Exceed
                        AlarmStat = "EXCEED"
                    Case AlarmStatuses.NearLimits
                        AlarmStat = "NEAR LIMITS"
                    Case AlarmStatuses.NearerLimits
                        AlarmStat = "NEARER LIMITS"
                    Case AlarmStatuses.OK
                        AlarmStat = "ALARM OFF"
                End Select
                response = response & AlarmStat & "," & thisWrkStn.WrkStnNumber.ToString & "," & Me.ServerUID.ToString
                response = response & "," & Me.ReportTime.ToString & "," & Me.AuthUID & ";" & Chr(4)
                SyncLock transmitQueue.SyncRoot
                    transmitQueue.Enqueue(response)
                End SyncLock

                Dim Action As String
                If ClearVehicle Then
                    Action = "REMOVE"
                Else
                    Action = "UPDATE"
                End If

                Dim SummItem As New SummaryItem("VEHICLE", Action, Me)
                SyncLock gSummaryQueue.SyncRoot
                    gSummaryQueue.Enqueue(SummItem)
                End SyncLock
                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, "HLCS VEHICLE RESPONSE:" & response, events)

                'SMC CLIENT UPDATE: ADDED VEHICLEID TO TRANSMIT MESSAGE

                If AuthIssueTo <> "" Then
                    Dim data() As String = AuthIssueTo.Split(",")
                    If data.Length = 2 Then
                        Dim ForemanName As String = Trim(data(0)) & " " & Trim(data(1))
                        Dim Message As String = "HLCSPOSITIONINFO;" & AuthMobileTerminalUID & "|" & ForemanName & "|" & Me.MilePost.ToString & "|" & Me.TrackGUID.ToString & "|" & Action & "|" & Me.VehicleID
                        SyncLock TMDS.GlobalDeclarations.gMSCTransmitQueue
                            TMDS.GlobalDeclarations.gMSCTransmitQueue.Enqueue(Message)
                        End SyncLock
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub MobileEvent(ByVal EventNote As String)

            Try
                Dim ClickEvent As New SystemEventRecord(Now, ManagementClasses.SystemEventRecord.EventEnum.SystemEvent, EventNote, SystemEventQueue)
                ClickEvent = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region

    End Class

#End Region

#Region "MOBILE LOCOMOTIVE CLASS"

    Public Class Locomotive

#Region "FIELDS"

        Public Symbol As String
        Public LocomotiveID As String
        Public TrainSheetRoute As String

        Public TerrID As Int32
        Public TrackGUID As Int32
        Private _currentTrack As Track
        Public MilePost As String = String.Empty
        Public MilePostPrefix As String = String.Empty
        Public MilePostSuffix As String = String.Empty
        Public MilePostTrackName As String = String.Empty
        Public SubID As String
        Public BlockUID As String
        Public Subdivision As String
        Public DirectionOfTravel As String
        Public DirectionOfTimeTable As String
        Public Speed As String
        Public TrackName As String
        Public ReportTime As Date
        Public TimeZone As String
        Public TrackAuthType As String
        Public Emergency As Boolean
        Public ServerUID As Integer
        Public MessageID As Date
        Public CurrentTrackDesignator As String
        Public LastMilepostPosition As Decimal
        Public CalculatedDirectionOfTravel As String
        Public LastTrackGUID As Int32
        Dim Dither As Decimal = 0.02

        Public CurrentMilePost As Decimal

        Private Locomotives As ArrayList
        Private events As Queue
        Private transmitQueue As Queue
        Private thisWrkStn As WorkStation
        Private KeepAlive As Timers.Timer
        Public PreviousTrack As Track

        Public thisLoco As Locomotive

        Public myTrainID As TrainID

        Public OnBoardState As StateOfOnBoard

        Public CutInState As Boolean
        Public CutOutState As Boolean = True
        Public ClearLocomotive As Boolean

        Public PTCCapable As Boolean
        Public OnPTCTerritory As Boolean
        Public TrackingEnabled As Boolean
        Public OutOfComm As Boolean
        Public OutOfCommMins As String
        Public LocomotiveIDColor As Color = Color.Black
        Public isLocoInitMessageRecvd As Boolean = False

        Public EndTrackGUID As Int32
        Private _endTrack As Track
        Public EndMilePost As String = String.Empty
        Public EndMilePostTrackName As String = String.Empty
        Public EndMilePostSuffix As String = String.Empty
        Public EndMilePostPrefix As String = String.Empty
        Public myTrackHeadEnd As Track
        Public myTrackRearEnd As Track
        Public myDirection As String

        ''' <summary>
        ''' End Track
        ''' </summary>
        ''' <returns>End Track</returns>
        Public Property EndTrack() As Track
            Get
                Return _endTrack
            End Get
            Set(ByVal Value As Track)
                _endTrack = Value
                If myTrainID IsNot Nothing AndAlso
                    myTrainID.thisCurrentLicense IsNot Nothing AndAlso
                    Not myTrainID.thisCurrentLicense.PTCElectronicTransmissionInProgress Then
                    SystemEvent($"EndTrack1={_endTrack.GUID}:{_endTrack.Name}")
                    If optEnableLicenseGenerationForPTC AndAlso
                        Value IsNot Nothing Then
                        myTrainID.IsInTrackReleaseMode = True
                        myTrainID.CheckToReleaseTrackBehindTrain(thisCurrentTrack, EndTrack)
                    End If
                    myTrainID.myRearEndTrack = _endTrack
                    myTrainID.IsInTrackReleaseMode = False
                ElseIf myTrainID IsNot Nothing AndAlso
                    myTrainID.thisCurrentLicense Is Nothing AndAlso
                    Value IsNot Nothing Then
                    SystemEvent($"EndTrack2={_endTrack.GUID}:{_endTrack.Name}")
                    myTrainID.myRearEndTrack = _endTrack
                    If myTrainID.thisRestrictiveLicense IsNot Nothing Then
                        myTrainID.CheckToReleaseTrackBehindTrain(thisCurrentTrack, EndTrack)
                    End If
                End If
            End Set
        End Property

        ''' <summary>
        ''' This Current Track
        ''' </summary>
        ''' <returns>Current track</returns>
        Public Property thisCurrentTrack() As Track
            Get
                Return _currentTrack
            End Get
            Set(ByVal Value As Track)
                _currentTrack = Value
                If myTrainID IsNot Nothing AndAlso
                    myTrainID.thisCurrentLicense IsNot Nothing AndAlso
                    Not myTrainID.thisCurrentLicense.PTCElectronicTransmissionInProgress Then
                    SystemEvent($"CurrentTrack1={_currentTrack.GUID}:{_currentTrack.Name}")
                    If optEnableLicenseGenerationForPTC Then
                        myTrainID.IsInTrackReleaseMode = True
                        myTrainID.CheckToReleaseTrackBehindTrain(thisCurrentTrack, EndTrack)
                    End If
                    myTrainID.IsInTrackReleaseMode = False
                    myTrainID.myHeadEndTrack = _currentTrack
                ElseIf myTrainID IsNot Nothing AndAlso
                     myTrainID.thisCurrentLicense Is Nothing AndAlso
                    Value IsNot Nothing Then
                    SystemEvent($"CurrentTrack2={_currentTrack.GUID}:{_currentTrack.Name}")
                    myTrainID.myHeadEndTrack = _currentTrack
                    If myTrainID.thisRestrictiveLicense IsNot Nothing Then
                        myTrainID.CheckToReleaseTrackBehindTrain(thisCurrentTrack, EndTrack)
                    End If
                End If
            End Set
        End Property

        Public Enum StateOfOnBoard
            InactiveSelfTest
            InactiveIniting
            InactiveIsolated
            InactiveFailed
            InactiveWaitingDisp
            InactiveOutOfComm
            InactiveTrail
            ActiveStopped
            ActiveMovingLost
            ActiveMoving
            ActiveBraking
            ActiveLowSpeedStopped
            ActiveLowSpeed
        End Enum

        Public rControlBrake As ControlBrake
        Public rBrakePressure As String
        Public rThrottle As String
        Public rBrakeStatus As LocoStatus
        Public rSoftwareVersion As String
        Public subState As String
        Public rsubState As SubStateLoco

        Public LocoStatusID As String = ""
        Public rLocoStatus As PTCLocomotiveStatus

        Public LocoInitStatusID As String = ""
        Public rLocoInitStatus As PTCLocomotiveStatus

        Public LocosubState As String
        Public rLocosubState As PTCLocoSubStateLoco

        Public rTerritoryType As String

        Public Enum ControlBrake
            NotUsed
            LeadCutIn
            LeadCutOut
            TrailCutOut
            LeadCutInDPU
        End Enum

        Public Enum LocoStatus
            DynamicBrakeOn
            DynamicBrakeDerated
            Reverser
            BrakeOverride
        End Enum

        Public Enum SubStateLoco
            SELFTEST
            SLEEP
            INITIALIZING
            DISENGAGED
            CUTOUT
            ACTIVE
            NONE
            BRAKING
            STOPPED
            REVERSE
            TRAILING
            FAILED
            INVALID
        End Enum

        Public Enum LocoInitStatus
            NOERROR
            GTBERROR
            MAXSPEEDERROR
            DISENGAGED
            CUTOUT
            ACTIVE
            NONE
            BRAKING
            STOPPED
            REVERSE
            TRAILING
            FAILED
            INVALID
        End Enum

        Public Enum PTCLocomotiveStatus
            NO_ERROR = 0
            GTB_ERROR = 1
            MAX_SPEED_ERROR = 2
            CHECK_CAD_TRAIN_SHEET = 3
            ENGINEER_LOGIN_ERROR = 4
            DATASET_ERROR = 5
            TIMEOUT_ERROR = 6
            NON_COMM_ERROR = 7
            BRAKE_TEST_ERROR = 8
            DEPARTURE_TEST_ERROR = 9
            CONFIGURATION_ERROR = 10
            FAILED_INIT = 11
            UNKNOWN = 99
        End Enum

        Public Enum PTCLocoSubStateLoco
            UNKNOWN = 0
            SELF_TEST = 1
            INITIALIZING = 2
            DISENGAGED = 3
            CUTOUT = 4
            ACTIVE = 5
            BRAKING = 6
            STOPPED = 7
            FAILED = 8
            REVERSE = 9
            TRAILING = 10
            SLEEPING = 11
        End Enum

        Public RequestedStatusReceived As Boolean = False
        Public InformationFuelLevel As String
        Public InformationFuelLevelTimeStamp As Date

        Public PositionRequested As Boolean
#End Region

#Region "NEW LOCOMOTIVE"

        Sub New(ByRef LocomotiveCollection As ArrayList, ByRef EventQueue As Queue,
                ByRef XmtQueue As Queue, ByVal thisWorkstation As WorkStation, Optional ByVal PosData() As String = Nothing, Optional ByVal IsFuelReport As Boolean = False)
            Dim Loco As Object
            Try
                If optEnablePTCBOSAPIMode Then
                    For Each Loco In LocomotiveCollection
                        If Locomotives Is Nothing Then
                            Locomotives = New ArrayList
                        End If
                        Locomotives.Add(Loco)
                    Next
                Else
                    Locomotives = LocomotiveCollection
                End If

                thisLoco = Me
                events = EventQueue
                transmitQueue = XmtQueue
                thisWrkStn = thisWorkstation
                If IsFuelReport AndAlso optEnableFXESpecificOptions Then 'Added this test FXE FUEL Report. 
                Else
                    KeepAlive = New Timers.Timer()
                    AddHandler KeepAlive.Elapsed, AddressOf KeepAliveElasped

                    KeepAlive.Interval = optLocomotiveReportKeepAliveIntervalCheck * 1000
                    KeepAlive.Start()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally

            End Try
        End Sub

#End Region

#Region "LOCOMOTIVE EVENTS"

        Private Sub SetOnBoardStatus(ByVal Status As String)
            Select Case CInt(Status)
                Case 0
                    Me.OnBoardState = StateOfOnBoard.InactiveSelfTest
                Case 1
                    Me.OnBoardState = StateOfOnBoard.InactiveIniting
                Case 2
                    Me.OnBoardState = StateOfOnBoard.InactiveTrail
                Case 3
                    Me.OnBoardState = StateOfOnBoard.InactiveFailed
                Case 4
                    Me.OnBoardState = StateOfOnBoard.InactiveOutOfComm
                Case 5
                    Me.OnBoardState = StateOfOnBoard.InactiveIsolated
                Case 6
                    Me.OnBoardState = StateOfOnBoard.ActiveMoving
                Case 7
                    Me.OnBoardState = StateOfOnBoard.ActiveMovingLost
                Case 8
                    Me.OnBoardState = StateOfOnBoard.ActiveBraking
                Case 9
                    Me.OnBoardState = StateOfOnBoard.ActiveStopped
                Case 10
                    Me.OnBoardState = StateOfOnBoard.ActiveLowSpeedStopped
                Case 11
                    Me.OnBoardState = StateOfOnBoard.ActiveLowSpeed
            End Select

        End Sub

        Private Sub KeepAliveElasped(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                Dim TimeOfReport As Date
                Select Case Me.TimeZone
                    Case "M"
                        TimeOfReport = Me.ReportTime.AddHours(1)
                    Case "P"
                        TimeOfReport = Me.ReportTime.AddHours(2)
                    Case Else
                        TimeOfReport = Me.ReportTime
                End Select

                If TimeOfReport < Now.AddMilliseconds(-optLocomotivePositionTimeOut) Then
                    If Not IsNothing(myTrainID) Then
                        myTrainID.IsTrackingWithGPS = False
                        myTrainID.myTrack.InitTrainDisplay()

                    End If
                    ClearLocomotive = True
                    Me.SummaryUpdate()
                    Me.KeepAlive.Enabled = False
                    Me.Dispose()
                Else
                    Me.KeepAlive.Enabled = True
                End If

                If optEnableAutomaticPositionReportRequesting Then
                    CheckForPositionReportUpdate()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Me.Dispose(True)
            End Try
        End Sub

        Private Sub CheckForPositionReportUpdate()
            Try
                If Not gSimulationMode Then Exit Sub
                If Not IsNothing(myTrainID) Then
                    If myTrainID.IsActivePTC AndAlso myTrainID.IsPresenceOnNetwork Then
                        If ReportTime < Now.AddSeconds(-15) AndAlso Not PositionRequested Then
                            PositionRequested = True
                            TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrainPositionRequest(myTrainID)
                        End If
                    End If
                End If
                If PositionRequested AndAlso ReportTime < Now.AddSeconds(-60) Then
                    PositionRequested = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub Killme()
            Try
                ClearLocomotive = True
                Me.SummaryUpdate()

                Try
                    Me.KeepAlive.Enabled = False
                Catch ex As Exception

                End Try

                Me.Dispose()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub Dispose(Optional ByVal ForceDisposal As Boolean = False)
            Try
                Me.LocoEventLog("TERMINATE ETMS LOCOMOTIVE")

                Try
                    RemoveHandler KeepAlive.Elapsed, AddressOf KeepAliveElasped
                Catch
                End Try

                Try
                    If Not Me.KeepAlive Is Nothing Then
                        Me.KeepAlive.Stop()
                        Me.KeepAlive.Enabled = False
                    End If
                    Me.KeepAlive = Nothing
                Catch ex As Exception
                    SystemException(ex.ToString)
                End Try
                If Not Locomotives Is Nothing Then
                    Try
                        SyncLock Locomotives
                            Me.Locomotives.Remove(Me)
                        End SyncLock
                    Catch ex As Exception
                        SystemException(ex.ToString)
                    End Try
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Loco event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub LocoEventLog(message As String)
            Try
                SystemEvent($"{NameOf(Locomotive)} {message}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "UTILITIES"

        Private Function FindCurrentTrack() As Track
            Try
                Dim trk As Track
                For Each trk In TMDS.GlobalDeclarations.TrackCollection
                    If trk.GUID = Me.TrackGUID Then
                        Return trk
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindParallelTrackComponent(ByVal trk As Track) As Track
            Try
                Dim comp As Object = Nothing
                Dim sw As Switch = Nothing
                For Each comp In trk.OsComponentCollection
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        sw = comp
                    End If
                Next
                If Not comp Is Nothing Then
                    If sw.ReverseRouteCollection.Contains(trk) Then
                        Return sw.NormalRouteCollection.Item(0)
                    ElseIf sw.NormalRouteCollection.Contains(trk) Then
                        Return sw.ReverseRouteCollection.Item(0)
                    End If
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindTrainID(ByVal EngID As String) As TrainID
            Try
                Dim trn As TrainID
                For Each trn In TMDS.GlobalDeclarations.TrainCollection
                    If trn.IdentifingUnit = EngID Then
                        Return trn
                    End If

                    Dim Eng As String
                    For Each Eng In trn.Units
                        If Eng = EngID Then
                            Return trn
                        End If
                    Next
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Sub CreateTrain()
            Try
                Dim trn As New TrainID()
                trn.Name = Me.LocomotiveID
                trn.IdentifingUnit = Me.LocomotiveID
                trn.Direction = Me.DirectionOfTravel
                trn.TrackGUID = Me.thisCurrentTrack.GUID
                trn.TypeOfID = "TRAIN"
                trn.ApplyTrainAbbreviation(gTrainAbbreviation)

                Me.myTrainID = trn
                Me.myTrainID.MilePost = Me.MilePost
                Me.myTrainID.MilePostSuffix = Me.MilePostSuffix
                Me.myTrainID.MilePostPrefix = Me.MilePostPrefix
                Me.myTrainID.Speed = Me.Speed
                Me.myTrainID.EndMilePost = Me.EndMilePost
                Me.myTrainID.EndMilePostSuffix = Me.EndMilePostSuffix
                Me.myTrainID.EndMilePostPrefix = Me.EndMilePostPrefix
                Me.thisCurrentTrack.InitTrainDisplay()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "POSITION REPORT"

        Public Sub UpdateReportETMS(ByVal data() As String)
            Try
                SystemEvent("Locomotive-UpdateReportETMS:" & Me.LocomotiveID)

                'MessageToBuild = MessageToBuild & Me.UID & ","
                'MessageToBuild = MessageToBuild & Me.TrainSymbol & ","
                'MessageToBuild = MessageToBuild & Me.LocoID & ","
                'MessageToBuild = MessageToBuild & Me.rBlockUID & ","
                'MessageToBuild = MessageToBuild & Me.rSubID & ","
                'MessageToBuild = MessageToBuild & Me.TrainSheetRoute & ","
                'MessageToBuild = MessageToBuild & Me.rMilePost & ","
                'MessageToBuild = MessageToBuild & Me.rMilePostSuffix & ","
                'MessageToBuild = MessageToBuild & Me.rSpeed & ","
                'MessageToBuild = MessageToBuild & Me.rDirection & ","
                'MessageToBuild = MessageToBuild & Me.CutOut.ToString & ","
                'MessageToBuild = MessageToBuild & Me.mySubState & ","
                'MessageToBuild = MessageToBuild & Me.rBrakePressure & ","
                'MessageToBuild = MessageToBuild & Me.rBrakeStatus & ","
                'MessageToBuild = MessageToBuild & Me.rControlBrake & ","
                'MessageToBuild = MessageToBuild & Me.rThrottle & ","
                'MessageToBuild = MessageToBuild & Format(Me.KeepAliveTime, "MM-dd-yyyy HH:mm:ss") & ","
                'MessageToBuild = MessageToBuild & Me.rTerritory & ","
                'MessageToBuild = MessageToBuild & Me.rSoftwareVersion & ","

                'ETMS;LOCOMOTIVE REPORT;,W FORFOR1 20A,BNSF  2112,584708,7098,FORHET,1027.377, ,0, ,False,2,,3,0,,04-20-2009 14:20:46,3,,,
                If data.Length < 14 Then
                    Dim ErrorString As String = String.Empty
                    Dim i As Integer
                    For i = 0 To data.Length - 1
                        ErrorString = ErrorString & "(" & data(i) & ") "
                    Next
                    SystemEvent("Locomotive-UpdateReportETMS-ErrorInData:" & ErrorString)
                    Exit Sub
                End If
                With thisLoco
                    .Symbol = data(1)
                    .LocomotiveID = data(2)
                    .BlockUID = data(3)
                    .SubID = data(4)
                    .TrainSheetRoute = data(5)
                    .MilePost = data(6)
                    .MilePostSuffix = data(7)
                    .Speed = data(8)
                    .DirectionOfTravel = data(9)
                    .CutOutState = CBool(data(10))
                    .rsubState = data(11)
                    .rBrakePressure = data(12)
                    .rBrakeStatus = data(13)
                    .rControlBrake = data(14)
                    .rThrottle = data(15)
                    .ReportTime = CDate(data(16))
                    .rTerritoryType = data(17)
                End With

                Dim trn As TrainID = Me.FindTrainID(Me.LocomotiveID)
                If Not IsNothing(trn) Then
                    Me.TrackGUID = trn.TrackGUID
                    Dim PTCBlkTrkName As String = GetPTCTrackName(Me.BlockUID)
                    If PTCBlkTrkName.Trim = String.Empty Then
                        Me.TrackName = trn.myTrack.TrackName
                    Else
                        Me.TrackName = PTCBlkTrkName
                    End If
                    SystemEvent("Locomotive-UpdateReportETMS-TrackName:" & TrackName)
                    Dim mySub As Subdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(trn.myTrack.SubdivisionID)
                    If Not IsNothing(mySub) Then
                        Me.Subdivision = mySub.Name
                    End If
                Else
                    SystemEvent("Locomotive-UpdateReportETMS-TrainNotFound:" & LocomotiveID)
                End If

                Me.CheckPositionReport()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function GetPTCTrackName(ByVal PTCBlockID As String) As String

            Dim trk As Track
            Try
                For Each trk In TMDS.GlobalDeclarations.TrackCollection
                    If trk.PTCBlockID = PTCBlockID Then
                        Return trk.TrackName
                        Exit For
                    End If
                Next
            Catch ex As Exception
                Return String.Empty
            Finally
                trk = Nothing
            End Try

            Return String.Empty
        End Function

        Public Sub UpdatePosition(ByVal ReportData() As String)
            Try
                If ReportData.Length < 14 Then
                    Dim ErrorString As String = String.Empty
                    Dim i As Integer
                    For i = 0 To ReportData.Length - 1
                        ErrorString = ErrorString & "(" & ReportData(i) & ") "
                    Next
                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, "REPORT ERROR:" & ErrorString, events)
                    Exit Sub
                End If
                With thisLoco
                    'If IsNumeric(ReportData(0)) Then
                    .LocomotiveID = ReportData(0)
                    'Else
                    '    Exit Sub
                    'End If

                    If IsNumeric(ReportData(1)) Then
                        .TerrID = ReportData(1)
                    Else
                        Exit Sub
                    End If

                    If IsNumeric(ReportData(2)) Then
                        .TrackGUID = ReportData(2)
                    Else
                        Exit Sub
                    End If

                    .MilePost = ReportData(3)
                    .MilePostSuffix = ReportData(4)
                    .Subdivision = ReportData(5)
                    .DirectionOfTravel = ReportData(6)
                    .DirectionOfTimeTable = ReportData(7)
                    .Speed = ReportData(8)
                    .TrackName = ReportData(9)
                    .ReportTime = ReportData(10)
                    .TimeZone = ReportData(11)
                    .TrackAuthType = ReportData(12)

                    If IsNumeric(ReportData(13)) Then
                        .ServerUID = ReportData(13)
                    Else
                        Exit Sub
                    End If

                    If ReportData.Length >= 16 Then
                        Me.SetOnBoardStatus(ReportData(14))
                        '.OnBoardState = ReportData(14)
                        If .CutInState <> CBool(ReportData(15)) And .CutInState Then
                            TMDS.ManagementClasses.AlertUtilities.CreateAlert(Alert.AlertType.CutOutStateChange, "CUTOUT", Me.myTrainID)
                        End If
                        .CutInState = CBool(ReportData(15))
                    End If
                End With

                Me.CheckPositionReport()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function DoesTrainExist(ByVal TrnName As String) As Boolean
            Try
                Dim trn As TrainID
                For Each trn In TMDS.GlobalDeclarations.TrainCollection
                    If trn.Name = TrnName Then
                        Return True
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Sub CheckPositionReport()
            Try
                SystemEvent("Locomotive-CheckPositionReport...Start")
                If optDisableTrainTrackingViaGPS Then Exit Sub
                Dim CurrentTrack As Track = FindCurrentTrack()

                If CurrentTrack Is Nothing Then
                    SystemEvent("Locomotive-CheckPositionReport:Track Not Found....")
                    Exit Sub
                Else

                    thisCurrentTrack = CurrentTrack
                    If Not IsNothing(Me.myTrainID) Then
                        If Not DoesTrainExist(Me.myTrainID.Name) Then
                            Me.myTrainID = Nothing
                        End If
                    End If
                    If IsNothing(Me.myTrainID) Then
                        Me.myTrainID = Me.FindTrainID(Me.LocomotiveID)
                        If IsNothing(Me.myTrainID) Then
                            Me.CreateTrain()
                            If Not Me.thisCurrentTrack.Indicating Then
                                Me.thisCurrentTrack.Occuppied = True
                                Me.thisCurrentTrack.UpdateStatus()
                            End If
                            Me.thisCurrentTrack.AddTrainID(Me.myTrainID)
                        Else
                            Me.myTrainID.MilePost = Me.MilePost
                            Me.myTrainID.MilePostSuffix = Me.MilePostSuffix
                            Me.myTrainID.MilePostPrefix = Me.MilePostPrefix
                            Me.myTrainID.Speed = Me.Speed
                            Me.myTrainID.EndMilePost = Me.EndMilePost
                            Me.myTrainID.EndMilePostSuffix = Me.EndMilePostSuffix
                            Me.myTrainID.EndMilePostPrefix = Me.EndMilePostPrefix
                            Me.myTrainID.PositionReportTime = Me.ReportTime
                            If Mid(CurrentTrack.AuthorityType, 1, 3) <> "CTC" Or gSimulationMode Then
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False)
                            End If
                        End If
                    Else
                        Me.myTrainID = Me.FindTrainID(Me.LocomotiveID)
                        'Me.myTrainID.PTCStatus = Me.subState
                        Me.myTrainID.myLocomotiveReport = Me
                        Me.myTrainID.MilePost = Me.MilePost
                        Me.myTrainID.MilePostSuffix = Me.MilePostSuffix
                        Me.myTrainID.MilePostPrefix = Me.MilePostPrefix
                        Me.myTrainID.Speed = Me.Speed
                        Me.myTrainID.PositionReportTime = Me.ReportTime
                        Me.myTrainID.EndMilePost = Me.EndMilePost
                        Me.myTrainID.EndMilePostSuffix = Me.EndMilePostSuffix
                        Me.myTrainID.EndMilePostPrefix = Me.EndMilePostPrefix
                        Me.myTrainID.Speed = Me.Speed
                        Me.myTrainID.PositionReportTime = Me.ReportTime
                        If Mid(CurrentTrack.AuthorityType, 1, 3) <> "CTC" Or gSimulationMode Then
                            If Me.myTrainID.EmeregencyFlag Or Me.myTrainID.BrakingNotificationFlag Then
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False)
                            Else
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False)
                            End If
                        End If
                    End If
                End If

                If optEnableTrackSidingBit Then
                    'NEW SIDING LOGIC CODE
                    If CurrentTrack.EnableControlledSiding Or CurrentTrack.EnableNonControlledSiding Then
                        Me.CurrentTrackDesignator = "S"
                    Else
                        Select Case CurrentTrack.TrackName.ToUpper
                            Case "MAIN", "MAIN TRACK"
                                Me.CurrentTrackDesignator = "M"
                            Case "MAIN 1", "MAIN ONE"
                                Me.CurrentTrackDesignator = "M1"
                            Case "MAIN 2", "MAIN TWO"
                                Me.CurrentTrackDesignator = "M2"
                            Case "MAIN 3", "MAIN THREE"
                                Me.CurrentTrackDesignator = "M3"
                            Case "MAIN 4", "MAIN FOUR"
                                Me.CurrentTrackDesignator = "M4"
                            Case "MAIN 5", "MAIN FIVE"
                                Me.CurrentTrackDesignator = "M5"
                            Case "MAIN 6", "MAIN SIX"
                                Me.CurrentTrackDesignator = "M6"
                            Case Else
                                Me.CurrentTrackDesignator = ""
                        End Select
                    End If

                Else
                    'OLD LEGACY SIDING CODE
                    Select Case CurrentTrack.TrackName.ToUpper
                        Case "SIDING", "SDG TK"
                            Me.CurrentTrackDesignator = "S"
                        Case "MAIN", "MAIN TRACK"
                            Me.CurrentTrackDesignator = "M"
                        Case "MAIN 1", "MAIN ONE"
                            Me.CurrentTrackDesignator = "M1"
                        Case "MAIN 2", "MAIN TWO"
                            Me.CurrentTrackDesignator = "M2"
                        Case "MAIN 3", "MAIN THREE"
                            Me.CurrentTrackDesignator = "M3"
                        Case "MAIN 4", "MAIN FOUR"
                            Me.CurrentTrackDesignator = "M4"
                        Case "MAIN 5", "MAIN FIVE"
                            Me.CurrentTrackDesignator = "M5"
                        Case "MAIN 6", "MAIN SIX"
                            Me.CurrentTrackDesignator = "M6"
                        Case Else
                            Me.CurrentTrackDesignator = ""
                    End Select
                End If


                SummaryUpdate()
                SystemEvent("Locomotive-CheckPositionReport...Finished")

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SummaryUpdate()
            Try

                If optEnablePTCBOSAPIMode Then Exit Sub
                Dim Action As String
                If ClearLocomotive Then
                    Action = "REMOVE"
                Else
                    Action = "UPDATE"
                End If
                If gPTCType = "ETMS" Then
                    Action = Action & "-" & gPTCType
                End If

                Dim SummItem As New SummaryItem("LOCOMOTIVE", Action, Me)
                SyncLock gLocomotiveSummaryQueue.SyncRoot
                    gLocomotiveSummaryQueue.Enqueue(SummItem)
                End SyncLock
                SystemEvent("Locomotive-SummaryUpdate...Completed")

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ProcessPositionReportForTrainTracking()
            Try

                Dim CurrentTrack As Track = FindCurrentTrack()
                If CurrentTrack Is Nothing Then
                    If Not IsNothing(Me.myTrainID) Then
                        If myTrainID.IsActivePTC Then
                            TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrainPositionRequest(myTrainID)
                        End If
                    End If
                    SystemEvent("ProcessPositionReportForTrainTracking:Track Not Found....")
                    Exit Sub
                Else
                    SystemEvent("ProcessPositionReportForTrainTracking:" & Me.Symbol & " TO TRACK:" & CurrentTrack.GUID & ":" & CurrentTrack.Name)

                    thisCurrentTrack = CurrentTrack
                    If Not IsNothing(Me.myTrainID) Then
                        If Not DoesTrainExist(Me.myTrainID.Name) Then
                            Me.myTrainID = Nothing
                        End If
                    End If
                    If IsNothing(Me.myTrainID) Then
                        Me.myTrainID = Me.FindTrainID(Me.LocomotiveID)
                        If IsNothing(Me.myTrainID) Then
                            Me.CreateTrain()
                            If Not Me.thisCurrentTrack.Indicating Then
                                Me.thisCurrentTrack.Occuppied = True
                                Me.thisCurrentTrack.UpdateStatus()
                            End If
                            Me.thisCurrentTrack.AddTrainID(Me.myTrainID)
                        Else
                            Me.myTrainID.MilePost = Me.MilePost
                            Me.myTrainID.MilePostSuffix = Me.MilePostSuffix
                            Me.myTrainID.MilePostPrefix = Me.MilePostPrefix
                            Me.myTrainID.Speed = Me.Speed
                            Me.myTrainID.EndMilePost = Me.EndMilePost
                            Me.myTrainID.EndMilePostSuffix = Me.EndMilePostSuffix
                            Me.myTrainID.EndMilePostSuffix = Me.EndMilePostPrefix
                            Me.myTrainID.PositionReportTime = Me.ReportTime
                            If Mid(CurrentTrack.AuthorityType, 1, 3) <> "CTC" Or gSimulationMode Then
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False)
                            End If
                        End If
                    Else
                        Me.myTrainID = Me.FindTrainID(Me.LocomotiveID)
                        Me.myTrainID.myLocomotiveReport = Me
                        Me.myTrainID.MilePost = Me.MilePost
                        Me.myTrainID.MilePostSuffix = Me.MilePostSuffix
                        Me.myTrainID.MilePostPrefix = Me.MilePostPrefix
                        Me.myTrainID.Speed = Me.Speed
                        Me.myTrainID.PositionReportTime = Me.ReportTime
                        Me.myTrainID.EndMilePost = Me.EndMilePost
                        Me.myTrainID.EndMilePostSuffix = Me.EndMilePostSuffix
                        Me.myTrainID.EndMilePostPrefix = Me.EndMilePostPrefix
                        Me.myTrainID.Speed = Me.Speed
                        Me.myTrainID.PositionReportTime = Me.ReportTime
                        Me.myTrainID.myHeadEndSubdivisionPTCUID = Me.thisCurrentTrack.mySubdivision.PTCUID
                        Me.myTrainID.myRearEndSubdivisionPTCUID = Me.thisCurrentTrack.mySubdivision.PTCUID
                        Dim SubdivisionPTCUID As Int32 = Me.thisCurrentTrack.mySubdivision.PTCUID
                        If gSimulationMode Then
                            If Me.myTrainID.myTrack.AuthorityType = "CTC" Then
                                'initialization the train symbol position to this end track and move the train to the begin track
                                Me.myTrackRearEnd = TMDS.SystemUtilities.FetchData.FindTrackFromMilePostWithTrackName(Me.EndMilePost, Me.EndMilePostSuffix, Me.EndMilePostPrefix, Me.EndMilePostTrackName, SubdivisionPTCUID)
                                Me.myTrackHeadEnd = TMDS.SystemUtilities.FetchData.FindTrackFromMilePostWithTrackName(Me.MilePost, Me.MilePostSuffix, Me.MilePostPrefix, Me.MilePostTrackName, SubdivisionPTCUID)
                                If IsNothing(myTrackRearEnd) Then
                                    myTrackRearEnd = myTrackHeadEnd
                                End If
                                Dim CompList As ArrayList = GetListOfTracks(Me.myTrackRearEnd, Me.myTrackHeadEnd)
                                If IsNothing(CompList) AndAlso Not IsNothing(Me.myTrackHeadEnd) AndAlso Not Me.myTrackHeadEnd.TurnOutType Then
                                    CompList = New ArrayList
                                    CompList.Add(Me.myTrackHeadEnd)
                                End If
                                TrackTheTrain(CompList)
                            End If
                        End If

                        If Mid(CurrentTrack.AuthorityType, 1, 3) <> "CTC" Or gSimulationMode Then
                            If Me.myTrainID.EmeregencyFlag Or Me.myTrainID.BrakingNotificationFlag Then
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False)
                            Else
                                Me.myTrainID.MoveTrainToNextTrack(TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.myTrainID.TrackGUID), Me.thisCurrentTrack, False, gSimulationMode)
                            End If
                        End If
                    End If
                End If

                If optEnableTrackSidingBit Then
                    'NEW SIDING LOGIC CODE
                    If CurrentTrack.EnableControlledSiding Or CurrentTrack.EnableNonControlledSiding Then
                        Me.CurrentTrackDesignator = "S"
                    Else
                        Select Case CurrentTrack.TrackName.ToUpper
                            Case "MAIN", "MAIN TRACK"
                                Me.CurrentTrackDesignator = "M"
                            Case "MAIN 1", "MAIN ONE"
                                Me.CurrentTrackDesignator = "M1"
                            Case "MAIN 2", "MAIN TWO"
                                Me.CurrentTrackDesignator = "M2"
                            Case "MAIN 3", "MAIN THREE"
                                Me.CurrentTrackDesignator = "M3"
                            Case "MAIN 4", "MAIN FOUR"
                                Me.CurrentTrackDesignator = "M4"
                            Case "MAIN 5", "MAIN FIVE"
                                Me.CurrentTrackDesignator = "M5"
                            Case "MAIN 6", "MAIN SIX"
                                Me.CurrentTrackDesignator = "M6"
                            Case Else
                                Me.CurrentTrackDesignator = ""
                        End Select
                    End If

                Else
                    'OLD LEGACY SIDING CODE
                    Select Case CurrentTrack.TrackName.ToUpper
                        Case "SIDING", "SDG TK"
                            Me.CurrentTrackDesignator = "S"
                        Case "MAIN", "MAIN TRACK"
                            Me.CurrentTrackDesignator = "M"
                        Case "MAIN 1", "MAIN ONE"
                            Me.CurrentTrackDesignator = "M1"
                        Case "MAIN 2", "MAIN TWO"
                            Me.CurrentTrackDesignator = "M2"
                        Case "MAIN 3", "MAIN THREE"
                            Me.CurrentTrackDesignator = "M3"
                        Case "MAIN 4", "MAIN FOUR"
                            Me.CurrentTrackDesignator = "M4"
                        Case "MAIN 5", "MAIN FIVE"
                            Me.CurrentTrackDesignator = "M5"
                        Case "MAIN 6", "MAIN SIX"
                            Me.CurrentTrackDesignator = "M6"
                        Case Else
                            Me.CurrentTrackDesignator = ""
                    End Select
                End If

                SummaryUpdate()
                SystemEvent("Locomotive-CheckPositionReport...Finished")

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function GetListOfTracks(ByVal BeginTrack As Track, ByVal EndTrack As Track) As ArrayList
            Dim CompList As New ArrayList
            Try
                If IsNothing(BeginTrack) Then Return Nothing
                Dim DirectionToRight As Boolean
                Dim LoopControl As Integer
                Dim NextComp As Object = BeginTrack

                If IsNothing(Me.myTrainID) Then Return Nothing
                If BeginTrack.TurnOutType Then Return Nothing

                CompList.Add(BeginTrack)
                If BeginTrack Is EndTrack OrElse IsNothing(EndTrack) Then Return CompList

                Select Case Me.myTrainID.Direction
                    Case optDirectionEast, optDirectionNorth
                        DirectionToRight = True
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)

                    Case optDirectionWest, optDirectionSouth
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                    Case optDirectionNone
                        Return CompList
                End Select

                If DirectionToRight Then
                    Do Until LoopControl > 15
                        LoopControl += 1

                        If IsNothing(NextComp) Then Exit Do
                        If NextComp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.GUID = EndTrack.GUID Then
                                CompList.Add(NextComp)
                                Exit Do
                            End If
                            CompList.Add(NextComp)
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)

                        ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                            'CompList.Add(NextComp)
                            Dim sw As Switch = CType(NextComp, Switch)
                            If sw.RightAltLink <> 0 Then
                                If sw.ControlledReverse Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.RightAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.RightLink)
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.RightLink)
                            End If
                        Else '
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End If
                    Loop

                Else
                    Do Until LoopControl > 15
                        LoopControl += 1
                        If IsNothing(NextComp) Then Exit Do
                        If NextComp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.GUID = EndTrack.GUID Then
                                CompList.Add(NextComp)
                                Exit Do
                            End If
                            CompList.Add(NextComp)
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                            Dim sw As Switch = CType(NextComp, Switch)

                            If sw.RightAltLink <> 0 Then
                                If sw.ControlledReverse Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.LeftAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.LeftLink)
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sw.LeftLink)
                            End If
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End If
                    Loop

                End If

                Dim trk1 As Track
                Dim CloneList As ArrayList = CompList.Clone
                For Each trk1 In CloneList
                    Dim t As Track
                    For Each t In TrackCollection
                        If trk1.StationNumber = t.StationNumber AndAlso trk1.TrackName = t.TrackName AndAlso trk1.Name = t.Name AndAlso trk1.GUID <> t.GUID Then
                            If Not CompList.Contains(t) Then
                                CompList.Insert(CompList.IndexOf(trk1), t)
                            End If
                        End If
                    Next

                Next
                Return CompList

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Sub TrackTheTrain(ByRef CompList As ArrayList)
            Try
                If IsNothing(CompList) Then Exit Sub
                CompList.Reverse()
                Dim trk As Track
                For Each trk In CompList
                    DoTrackIndicatation(True, trk)
                Next
                Dim RemoveList As New ArrayList
                For Each trk In Me.myTrainID.myTrackList
                    If Not CompList.Contains(trk) Then
                        RemoveList.Add(trk)
                    End If
                Next

                For Each trk In RemoveList
                    DoTrackIndicatation(False, trk)
                Next
                Me.myTrainID.myTrackList = CompList.Clone


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DoTrackIndicatation(ByVal IndicateTrack As Boolean, ByVal LastTrack As Track)
            Try


                If LastTrack.Indicating Then
                    LastTrack.myControlPoint.FlashName()
                    Dim trk As Object
                    If Not LastTrack.OsComponentCollection Is Nothing Then
                        For Each trk In LastTrack.OsComponentCollection
                            If trk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim thisTrack As Track = CType(trk, Track)
                                If trk.TrackIndicationANDED Or trk.TrackIndicationORED Then
                                    If Not IsNothing(trk.Ind(1)) Then
                                        trk.Ind(1).BitState = IndicateTrack
                                    End If
                                End If

                                If IndicateTrack And Not thisTrack.IsLinkToFlipFlopToRight And Not thisTrack.IsLinkToFlipFlopToLeft Then
                                    thisTrack.TrafficDirection = Me.myTrainID.GetDirection
                                Else
                                    thisTrack.TrafficDirection = ""
                                End If

                                thisTrack.ApplyTraffic()
                                trk.Occuppied = IndicateTrack
                                trk.Ind(0).BitState = IndicateTrack
                                trk.ApplyIndicationChange()
                                trk.UpdateStatus()
                            End If
                        Next
                    Else
                        If IndicateTrack Then
                            LastTrack.TrafficDirection = Me.myTrainID.GetDirection
                        Else
                            LastTrack.TrafficDirection = ""
                        End If
                        LastTrack.ApplyTraffic()
                        LastTrack.Ind(0).BitState = IndicateTrack
                        LastTrack.ApplyIndicationChange()
                        LastTrack.UpdateStatus()
                        Dim OtherComp As Object
                        For Each OtherComp In LastTrack.Ind(0).ComponentRefCollection
                            If OtherComp.GetType.ToString = gTrackComponentName Then
                                If Not OtherComp Is LastTrack Then
                                    OtherComp.Ind(0).BitState = IndicateTrack
                                    If OtherComp.TrackIndicationANDED Or OtherComp.TrackIndicationORED Then
                                        If Not IsNothing(OtherComp.Ind(1)) Then
                                            OtherComp.Ind(1).BitState = IndicateTrack
                                        End If
                                    End If
                                    OtherComp.ApplyIndicationChange()
                                    OtherComp.UpdateStatus()
                                End If
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region

#Region "POSITION & FUEL REPORT"

        Public Sub UpdatePositionFuel(ByVal theMessage As String)
            Try
                'Dim theReport As New StringBuilder
                '0'theReport.Append("LOCOMOTIVE-INFORMATION;")
                '1'theReport.Append("POSITION-FUEL;")
                '2'theReport.Append(TrainSymbol)
                'theReport.Append(";")
                '3'theReport.Append(LocomotiveID)
                'theReport.Append(";")
                '4'theReport.Append(LocationSubdivisionName)
                'theReport.Append(";")
                '5'theReport.Append(LocationMilePost)
                'theReport.Append(";")
                '6'theReport.Append(LocationMilePostSuffix)
                'theReport.Append(";")
                '7'theReport.Append(Format(LocationTimeStamp, "MM-dd-yy HH:mm:ss"))
                'theReport.Append(";")
                '8'theReport.Append(Speed)
                'theReport.Append(";")
                '9'theReport.Append(FuelLevel)
                'theReport.Append(";")
                '10'theReport.Append(Format(FuelTimeStamp, "MM-dd-yy HH:mm:ss"))

                Dim data() As String = theMessage.Split(";")
                Dim trn As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromSymbol(data(2))
                With thisLoco
                    .myTrainID = trn
                    .Symbol = data(2)
                    .LocomotiveID = data(3)
                    If Trim(data(4)) <> "" Then .Subdivision = data(4)
                    If Trim(data(5)) <> "" Then .MilePost = data(5)
                    If Trim(data(6)) <> "" Then .MilePostSuffix = data(6)
                    If Trim(data(7)) <> "" Then
                        Dim rptdt As Date
                        If Date.TryParse(data(7), rptdt) Then
                            .ReportTime = rptdt
                        End If
                    End If
                    ' .ReportTime = CDate(data(7))
                    If Trim(data(8)) <> "" Then .Speed = data(8)
                    If Trim(data(9)) <> "" Then .InformationFuelLevel = data(9)
                    If Trim(data(10)) <> "" Then
                        Dim Infodt As Date
                        If Date.TryParse(data(10), Infodt) Then
                            .InformationFuelLevelTimeStamp = Infodt
                        End If
                    End If
                    '.InformationFuelLevelTimeStamp = CDate(data(10))
                End With

                SummaryUpdate()

                SystemEvent("UpdatePositionFuel Message Processing Completed:" & theMessage)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

    End Class
#End Region


End Namespace


