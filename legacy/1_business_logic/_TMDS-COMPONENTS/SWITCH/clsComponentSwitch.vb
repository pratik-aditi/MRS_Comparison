Imports System.Data.OleDb
Imports System.Drawing.Drawing2D
Imports System.Timers
Imports TMDS.CodeServerClient
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.SystemUtilities.UtilitiesDisplayForm
Imports TMDS.TrackAuthority
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrainComponents
Imports TMDS.SignalComponents
Imports System.Reflection.MethodBase

Namespace SwitchComponents

#Region "SWITCH"

    Public Class Switch
        Inherits GenericComponent

        Sub New()
            'Build the flasher timer for the signal bulb
            Flasher = New Windows.Forms.Timer() 'New Timers.Timer()
            Flasher.Interval = 250
            AddHandler Flasher.Tick, AddressOf FlashElapsedEvent
            'Build the simulator timer for the signal bulb
            IndSimulator = New Timers.Timer()
            IndSimulator.Interval = 5000
            AddHandler IndSimulator.Elapsed, AddressOf SimulateElaspedEvent
            'ReControl Timer
            tmrReControl = New Timers.Timer()
            tmrReControl.Interval = 60000
            tmrReControl.AutoReset = False
            AddHandler tmrReControl.Elapsed, AddressOf ReControlElaspedEvent
            Visible = False
        End Sub

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                Try
                    Try
                        Flasher.Enabled = False
                        Flasher.Stop()
                        RemoveHandler Flasher.Tick, AddressOf FlashElapsedEvent
                        Flasher.Dispose()
                        Flasher = Nothing
                    Catch
                    End Try
                    Try
                        RemoveHandler MouseEnter, AddressOf Switch_MouseEnter
                        RemoveHandler MouseLeave, AddressOf Switch_MouseLeave
                        RemoveHandler MouseHover, AddressOf Switch_MouseHover
                    Catch
                    End Try
                    Try
                        tmrReControl.Enabled = False
                        tmrReControl.Stop()
                        RemoveHandler tmrReControl.Elapsed, AddressOf ReControlElaspedEvent
                        tmrReControl.Dispose()
                        tmrReControl = Nothing
                    Catch
                    End Try
                    If Not IsNothing(IndSimulator) Then
                        RemoveHandler IndSimulator.Elapsed, AddressOf SimulateElaspedEvent
                        IndSimulator.Dispose()
                        IndSimulator = Nothing
                    End If
                    Path = Nothing
                    AuthNumberRegion = Nothing
                    ColorStatusBlocking = Nothing
                    ColorSummarySelectionBlocking = Nothing
                    ColorStatusRouteRequesting = Nothing
                    FlashOffColorStatus = Nothing
                    OutLinePath = Nothing
                    AuthNumberPath = Nothing
                    NoPath = Nothing
                    SwitchPath = Nothing
                    ColorSummarySelectionBlocking = Nothing
                    ColorSummarySelectionRequesting = Nothing
                    Try
                        mySwitchLock.Dispose()
                    Catch
                    End Try
                    mySwitchLock = Nothing
                Catch ex As Exception
                    SystemException(ex.ToString)
                End Try
            End If
            MyBase.Dispose(disposing)
        End Sub

        Public Overloads Sub Dispose()
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#Region "VARIABLES"

        Public ColorStatusBlocking As New SolidBrush(Color.SteelBlue)
        Public ColorStatusRouteRequesting As New SolidBrush(Color.YellowGreen)
        Public ColorSummarySelectionBlocking As New SolidBrush(Color.LightSteelBlue)
        Public ColorSummarySelectionRequesting As New SolidBrush(Color.Yellow)
        Public ColorStatusTagRequesting As New SolidBrush(Color.Cyan)
        Public SwitchOrientation As String
        Private pFlashState As Boolean
        Private pSwitchStatus As SolidBrush
        Private pControlled As Boolean
        Private pTrackName As String
        Private pNormCorespondence As Boolean
        Private pRevCorespondence As Boolean
        Private pNormalRoute(5) As Integer
        Private pReverseRoute(5) As Integer
        Public TypeOfSwitch As String
        Public OSType As Boolean = True
        Public ConfirmationToMoveSwitch As Boolean
        Private pLimitsForTrain As Boolean
        Private pLimitsForMOW As Boolean
        Private pOccuppied As Boolean
        Private pActiveTrackWarrant As Boolean
        Public PowerSwitchInTWC As Boolean
        Public BiDirectionalABS As Boolean
        Public SingleABSEast As Boolean
        Public SingleABSWest As Boolean
        Public BoxElevenRestriction As Boolean
        Public BoxEightRestriction As Boolean
        Public TrainIDs As ArrayList
        Public Labels As ArrayList
        Public Authorities As New ArrayList()
        Private pAuthCounts As Integer
        Public Restrictions As ArrayList
        Private AuthNumberTextCollection As New AuthNumberTextCollection()
        Private AuthNumberPath As New GraphicsPath()
        Public AuthNumberRegion As New Region()
        Private pLeftTraffic As Boolean
        Private pRightTraffic As Boolean
        Public TrafficDirection As String
        Public TrackAndTime As Boolean
        Public TrackPermit As Boolean
        Public FoulTime As Boolean
        Public DTCAuthority As Boolean
        Public TrackTagRestrictive As Boolean
        Public trackTagAlarm As Boolean
        Public trackTagPlan As Boolean
        Public TrackAuthority As Boolean
        Public pSignalStatOnMe As SignalStatus
        Public pSwitchIndStatus As SwIndStatus
        Public SwitchIndStatusPrevious As SwIndStatus
        Public PTC_DeviceName As String = ""
        Public PTC_ProperName As String = ""
        Public PTC_WIUStatusIndex As String = ""
        Public PTC_DeviceBlockID As String = ""
        Public PTC_Enable_EMT As Boolean
        Public SwitchPositionStatus As PositionStatus
        Public PreviousStatus As SwIndStatus
        Public NormalRouteCollection As New ArrayList()
        Public ReverseRouteCollection As New ArrayList()
        Public pLastControl As Boolean
        Public AllowBlocking As Boolean
        Public AllowSwitchIndBlock As Boolean
        Public SwitchBlockInd As BitPosition
        Public SwitchBlockControlOn As BitPosition
        Public SwitchBlockControlOff As BitPosition
        Public AllowFieldBlocking As Boolean
        Public SwitchLockInd As BitPosition
        Public SwitchLockBit As Boolean
        Public RouteLock As Boolean
        Public SwitchLockBitPrevious As Boolean
        Public NonTransverable As Boolean
        Public OsComponentCollection As ArrayList
        Public FlashOffColorStatus As New SolidBrush(Color.Black)
        Public BrotherSwitches As ArrayList
        Public ItemSevenAllowedOnLeftLimits As Boolean
        Public ItemSevenAllowedOnRightLimits As Boolean
        Public FlipMe As Boolean
        Public CallOn As Boolean
        Public NoValidate As Boolean
        Public FlipOperations As Boolean
        Public IgnoreOS As Boolean
        Public LeaveReversePermission As Boolean
        Public pProtectOpenSwitchEnabled As Boolean = True
        Public PreviousControlStatus As SwCtlStatus
        Public PreviouSIndStatus As SwIndStatus
        Public CurrentControlStatus As SwCtlStatus
        Public MouseEntered As Boolean
        Public MouseLefted As Boolean
        Public SwitchPath As GraphicsPath
        Public OutLinePath As GraphicsPath
        Public NoPath As GraphicsPath
        Public SwitchPathRelative As GraphicsPath
        Public OutLinePathRelative As GraphicsPath
        Public NoPathRelative As GraphicsPath
        Public TurnOutPath As GraphicsPath
        Public IndeterminatePath As GraphicsPath
        Public mySwitchLock As SwitchLock
        Public mySwitchPositionIndicator As SwitchPositionIndicator
        Public mySwitchPositionIndicatorCoordinate As Int32
        Public CurrentColorStatusNoPath As SolidBrush
        Public MatchMode As Boolean
        Public tmrReControl As Timers.Timer
        Public ReControlCount As Integer
        Public ReControlStart As Boolean
        Public ControlNormalTime As Date '= Now
        Public ControlReverseTime As Date '= Now
        Public mnuDBCreate As MenuItem
        Public mnuDBRemove As MenuItem
        Public mnuDBOS As MenuItem
        Public mnuTest As MenuItem
        Public AllowBlueFlag As Boolean
        Public AllowOCS As Boolean
        Public SpringSwitch As Boolean
        Public AllowSwitchRestoralOnAuthRelease As Boolean
        Public AllowIndependentControlSwitch As Boolean
        Public mnuICSwitch As MenuItem
        Public pInitialized As Boolean
        Public IgnoreBothIndicationReceipt As Boolean
        Public PuzzleSwitch As Boolean
        Public UpdateTimeSpan As TimeSpan
        Public UpdateCount As Integer
        Public UpdateStatusTime As Date
        Public LockUpdateString As String = "LOCK"
        Public pBoxThirteenControlled As Boolean
        Public SetOutOfCorespondenceNormal As Boolean
        Public SetOutOfCorespondenceReverse As Boolean
        Public RCPS As Boolean
        Public LockControlRightDirection As BitPosition
        Public LockControlLeftDirection As BitPosition
        Public LockControlBiDirectionNoRestriction As BitPosition
        Public LockControlBiDirectionRestricting As BitPosition
        Public LockIndRightDirection As BitPosition
        Public LockIndLeftDirection As BitPosition
        Public LockIndBiDirectionNoRestriction As BitPosition
        Public LockIndBiDirectionRestricting As BitPosition
        Public TimeLockIndication As BitPosition
        Public ForceProtectionOnRCPS As Boolean
        Public DirectionalRCPSLockOn As Boolean
        Public TimeLockingOn As Boolean
        Public SwitchPointerMonitorMode As Boolean
        Public NonOperationSwitchPointMonitor As Boolean
        Public myTurnOutTrack As Track
        Public mnuSPMVerifyAdd As MenuItem
        Public mnuSPMVerfiyRemove As MenuItem
        Public mnuSPMOverride As MenuItem
        Public mnuSPMOverrideRemove As MenuItem
        Public ManualSwitchPointVerification As Boolean
        Public LastReportTime As Date = Now.AddMinutes(-10)
        Public AlarmTime As TimeSpan
        Public CurrentSwitchPointStatus As String = "UNKNOWN"
        Public StatusRecordExists As Boolean
        Public SwitchOverrideList As New ArrayList
        Private pPointStatusRecordTime As Date
        Public pSwitchPointInitialized As Boolean
        Public NoIndicationAssigned As Boolean
        Public PositionReportTimeOut As Integer = 900 '15
        Public mnuTrainerIndNormal As MenuItem
        Public mnuTrainerIndReverse As MenuItem
        Public mnuTrainerIndNoCorespondence As MenuItem
        Public mnuTrainerIndDisable As MenuItem
        Public DisableSimulatedIndication As Boolean
        Public StatusPreviousControlReverse As Boolean
        Public StatusPreviousPosition As SwIndStatus
        Public StatusPreviousBlock As Boolean
        Public SwitchLockNoControl As Boolean
        Public HatchItInformational As Boolean
        Public HatchItSpeed As Boolean
        Public HatchItWorkZone As Boolean
        Public HatchItFormO As Boolean
        Public Bulletins As ArrayList = ArrayList.Synchronized(New ArrayList())
        Public UsedForEntryToMain As Boolean
        Public PTCUID As Int32
        Public MilePost As String = "0000.000"
        Public MilePostSuffix As String = ""
        Public MilePostPrefix As String = ""
        Public PTCAuthorityPending As Boolean = False
        Public NormalTrackTrailingPortion As Track
        Public NormalTrackFacingPortion As Track
        Public ReverseTrackTrailingPortion As Track
        Public PublishStatusToPTC As Boolean
        Public DevicePTCUID As String
        Public JointControl As Boolean
        Public EnableJointControl As Boolean = True
        Private _OSLocking As Boolean

#End Region

#Region "PROPERTIES"

        Public Property SignalStatOnMe() As SignalStatus
            Get
                Return pSignalStatOnMe
            End Get
            Set(ByVal Value As SignalStatus)
                pSignalStatOnMe = Value
            End Set
        End Property

        Public Property SwitchIndStatus() As SwIndStatus
            Get
                Return pSwitchIndStatus
            End Get
            Set(ByVal Value As SwIndStatus)
                SwitchIndStatusPrevious = pSwitchIndStatus
                pSwitchIndStatus = Value
            End Set
        End Property

        Public Property Initialized() As Boolean
            Get
                Return pInitialized
            End Get
            Set(ByVal Value As Boolean)
                pInitialized = Value
            End Set
        End Property

        Public Property BoxThirteenControlled() As Boolean
            Get
                Return pBoxThirteenControlled
            End Get
            Set(ByVal Value As Boolean)
                pBoxThirteenControlled = Value
            End Set
        End Property

        Public Property LastControl() As Boolean
            Get
                Return pLastControl
            End Get
            Set(ByVal Value As Boolean)
                pLastControl = Value
            End Set
        End Property

        Public Property ProtectOpenSwitchEnabled() As Boolean
            Get
                Return pProtectOpenSwitchEnabled
            End Get
            Set(ByVal Value As Boolean)
                pProtectOpenSwitchEnabled = Value
            End Set
        End Property

        ''' <summary>
        ''' This designates that switch will not block the control when exist a stored route
        ''' </summary>
        ''' <returns><see langword="True"/>Switch doesn´t allow to move with stored route , <see langword="False"/> otherwise.</returns>
        Public Property OSLocking() As Boolean
            Get
                Return _OSLocking
            End Get
            Set(ByVal Value As Boolean)
                _OSLocking = Value
            End Set
        End Property

        Public Property SwitchPointInitialized() As Boolean
            Get
                Return pSwitchPointInitialized
            End Get
            Set(ByVal Value As Boolean)
                pSwitchPointInitialized = Value
            End Set
        End Property

        Public Property PointStatusRecordTime() As Date
            Get
                Return pPointStatusRecordTime
            End Get
            Set(ByVal Value As Date)
                pPointStatusRecordTime = Value
            End Set
        End Property

        Public Property TrackName() As String
            Get
                Return pTrackName
            End Get
            Set(ByVal Value As String)
                pTrackName = Value
            End Set
        End Property

        Public ReadOnly Property AuthorityCounts() As Integer
            Get
                Return Authorities.Count
            End Get
        End Property

        Public Property ActiveTrackWarrant() As Boolean
            Get
                Return pActiveTrackWarrant
            End Get
            Set(ByVal Value As Boolean)
                pActiveTrackWarrant = Value
            End Set
        End Property

        Public Property Occuppied() As Boolean
            Get
                Return pOccuppied
            End Get
            Set(ByVal Value As Boolean)
                pOccuppied = Value
            End Set
        End Property

        Public Property LimitsForMOW() As Boolean
            Get
                Return pLimitsForMOW
            End Get
            Set(ByVal Value As Boolean)
                pLimitsForMOW = Value
            End Set
        End Property

        Public Property LimitsForTrain() As Boolean
            Get
                Return pLimitsForTrain
            End Get
            Set(ByVal Value As Boolean)
                pLimitsForTrain = Value
            End Set
        End Property

        Public Property NormalCorespondence() As Boolean
            Get
                Return pNormCorespondence
            End Get
            Set(ByVal Value As Boolean)
                pNormCorespondence = Value
            End Set
        End Property

        Public Property ControlledReverse() As Boolean
            Get
                Return pControlled
            End Get
            Set(ByVal Value As Boolean)
                pControlled = Value
            End Set
        End Property

        Public Property NormalRoute(ByVal Index As Integer) As Integer

            Get
                Return pNormalRoute(Index)
            End Get
            Set(ByVal Value As Integer)
                pNormalRoute(Index) = Value
            End Set
        End Property

        Public Property ReverseRoute(ByVal Index As Integer) As Integer
            Get
                Return pReverseRoute(Index)
            End Get
            Set(ByVal Value As Integer)
                pReverseRoute(Index) = Value
            End Set
        End Property

#End Region

#Region "ENUMERATIONS"

        Enum SignalStatus
            SignalAtStop
            SignalRequested
            SignalCleared
            SignalInTime
            SignalUnSolicated
            PendingStateChange
            SignalRequestedDrill
            SignalClearedDrill
            SignalOfficeLock
        End Enum

        Enum PositionStatus
            Normal
            Reverse
            Indeterminate
        End Enum

        Enum SwIndStatus
            NormalInd
            ReverseInd
            NoIndication
        End Enum

        Enum SwCtlStatus
            NormalCtl
            ReverseCtl
        End Enum

#End Region

#Region "MOUSE EVENTS"

        Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            Dim button As Short = e.Button \ &H100000
            Try
                SwitchEventLog($"CLICKED - MOUSE DOWN SWITCH (BUTTON={IIf(button = 1, "LEFT", "RIGHT")})")
                If e.Button = Windows.Forms.MouseButtons.Right Then
                    If (gPermissionPassRedSignal AndAlso
                        Not gPermissionPassRedSignalSwitchSelectionMode) OrElse
                        gCreateTrainPassRed564 OrElse
                        gCPTNTRollUpForMow OrElse
                        gCPTNTRollUpForTrain Then
                        ContextMenuEnabled(False)
                        Exit Sub
                    End If
                End If
                If gOptDisableSignalControlOnManualOperateMode AndAlso
                    AuthorityType.Equals("CTC") AndAlso
                    myControlPoint.ManualOperateMode AndAlso
                    Not gMaintenanceMode Then
                    SwitchEventLog($"SWITCH CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT {myControlPoint.Name _
                                   } IS CURRENTLY IN MAINTENANCE MODE")
                    MessageBox.Show($"SWITCH CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT.{vbCrLf}{myControlPoint.Name _
                                    } IS CURRENTLY IN MAINTENANCE MODE", "SYSTEM MESSAGE",
                                                                         MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Exit Sub
                End If
                If gPermissionPassRedSignal AndAlso
                    Not gPermissionPassRedSignalSwitchSelectionMode Then Exit Sub
                If gCreateIssueClearanceProceed OrElse
                    gCreateIssueTrackWarrantProceedForTrain OrElse
                    gCreateIssueTrackWarrantWorkForTrain OrElse
                    gCreateIssueTrackWarrantWorkForMOW OrElse
                    gCreateIssueTrackWarrantRollUpForTrain OrElse
                    gCreateIssueTrackWarrantRollUpForMow Then Exit Sub
                If gCritcalPromptExists Then
                    SwitchEventLog("CRITICAL PROMPT EXISTS - EXITING MOUSE SELECTION")
                    Exit Sub
                End If
                If myControlPoint.MaintenanceMode AndAlso
                    Not gMaintenanceMode Then
                    AddMessageToFeedback($"SWITCH CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT {myControlPoint.Name _
                                         } IS CURRENTLY IN MAINTENANCE MODE")
                    Exit Sub
                End If
                If RouteLock Then
                    SwitchEventLog("SWITCH-ROUTE-LOCKED")
                    RouteLock = False
                    Exit Sub
                End If
                If SelectionMode = ModeSelection.SummarySelectionRouting Then
                    SwitchEventLog("SWITCH-ROUTE-LOCKED BY SELECTION")
                    RouteLock = False
                    Exit Sub
                End If
                For Each brotherSwitch As Switch In BrotherSwitches
                    If brotherSwitch.SelectionMode = ModeSelection.SummarySelectionRouting Then
                        SwitchEventLog($"BROTHER SWITCH-ROUTE-LOCKED BY SELECTION {brotherSwitch.GUID}")
                        RouteLock = False
                        Exit Sub
                    End If
                Next
                If HasAuthority() Then
                    SwitchEventLog($"{GetCurrentMethod.Name} Switch with authority")
                    Exit Sub
                End If
                If HasStoredRoute() Then
                    SwitchEventLog($"{GetCurrentMethod.Name} Switch with stored route")
                    Exit Sub
                End If
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    If Me.NonOperationSwitchPointMonitor Then Exit Sub
                    If (gAssistMode OrElse (Me.IsShadowMode AndAlso Not Me.JointControl)) Then Exit Sub
                    If Me.IndicateOnly Then Exit Sub
                    If gCreateTrackTime Or gCreateFoulTime Or gCreateTrackPermit Or gEntExit Then Exit Sub
                    If gBlockAllDevices Or gBlockSwitchDevices Or gPermissionPassRedSignalSwitchSelectionMode Then
                        If Not Me.AllowBlocking Then Exit Sub
                        If gPermissionPassRedSignalSwitchSelectionMode Then

                            '684.0 P1-5078 - If this is an electric lock switch, the user can't put it in hand (RPBJR)
                            If optRailRoadName.Equals("BNSF") Then
                                If Me.TypeOfSwitch.Equals("ELECTRIC LOCK") Then
                                    Exit Sub
                                End If
                            End If
                            'END 684.0

                            If gPermissionPassRedSignalControlPoint.GUID <> Me.myControlPoint.GUID Then
                                Exit Sub
                            Else
                                If Not gEntExtTrackSelection.Contains(Me) Then
                                    gEntExtTrackSelection.Add(Me)
                                    If Not PSSSelectedSwitchCollection.Contains(Me) Then PSSSelectedSwitchCollection.Add(Me)
                                    Me.SelectionMode = ModeSelection.DeviceBlocking
                                    Me.Flasher.Interval = 150
                                    Me.FlashComponent = True

                                    Dim sw As Switch
                                    For Each sw In Me.BrotherSwitches
                                        If Not sw Is Me Then
                                            If Not gEntExtTrackSelection.Contains(sw) Then gEntExtTrackSelection.Add(sw)
                                            If Not PSSSelectedSwitchCollection.Contains(sw) Then PSSSelectedSwitchCollection.Add(sw)
                                            sw.SelectionMode = ModeSelection.DeviceBlocking
                                            sw.Flasher.Interval = 150
                                            sw.FlashComponent = True
                                        End If
                                    Next

                                    TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                                End If
                            End If
                        End If

                        If Not gPermissionPassRedSignalSwitchSelectionMode Then

                            If Not gDeviceBlockSelection.Contains(Me) Then
                                If Not gDeviceBlockSelection.Contains(Me) Then gDeviceBlockSelection.Add(Me)
                                Me.SelectionMode = ModeSelection.DeviceBlocking
                                Me.Flasher.Interval = 150
                                Me.FlashComponent = True

                                Dim sw As Switch
                                For Each sw In Me.BrotherSwitches
                                    If Not sw Is Me Then
                                        If Not gDeviceBlockSelection.Contains(sw) Then gDeviceBlockSelection.Add(sw)
                                        sw.SelectionMode = ModeSelection.DeviceBlocking
                                        sw.Flasher.Interval = 150
                                        sw.FlashComponent = True
                                    End If
                                Next
                                TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                            Else
                                gDeviceBlockSelection.Remove(Me)
                                Me.SelectionMode = ModeSelection.Regular
                                Me.FlasherMode(False)
                                Me.Flasher.Interval = 250
                                Dim sw As Switch
                                For Each sw In Me.BrotherSwitches
                                    If Not sw Is Me Then
                                        gDeviceBlockSelection.Remove(sw)
                                        sw.SelectionMode = ModeSelection.Regular
                                        sw.FlasherMode(False)
                                        sw.Refresh()
                                        sw.SetOutOfCorespondenceReverse = False
                                        sw.SetOutOfCorespondenceNormal = False
                                        sw.UpdateStatus()
                                    End If
                                Next
                                Me.SetOutOfCorespondenceReverse = False
                                Me.SetOutOfCorespondenceNormal = False
                                Me.UpdateStatus()

                            End If

                        End If
                    ElseIf gCreateBlueFlagBlock Then
                        If Not Me.AllowBlueFlag Or Not Me.AllowBlocking Then Exit Sub

                        If Not gDeviceBlockSelection.Contains(Me) Then
                            gDeviceBlockSelection.Add(Me)
                            Me.SelectionMode = ModeSelection.DeviceBlocking
                            Me.Flasher.Interval = 150
                            Me.FlashComponent = True

                            Dim sw As Switch
                            For Each sw In Me.BrotherSwitches
                                If Not sw Is Me Then
                                    gDeviceBlockSelection.Add(sw)
                                    sw.SelectionMode = ModeSelection.DeviceBlocking
                                    sw.Flasher.Interval = 150
                                    sw.FlashComponent = True
                                End If
                            Next
                            TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                        Else
                            gDeviceBlockSelection.Remove(Me)
                            Me.SelectionMode = ModeSelection.Regular
                            Me.FlasherMode(False)
                            Me.Flasher.Interval = 250
                            Dim sw As Switch
                            For Each sw In Me.BrotherSwitches
                                If Not sw Is Me Then
                                    gDeviceBlockSelection.Remove(sw)
                                    sw.SelectionMode = ModeSelection.Regular
                                    sw.FlasherMode(False)
                                    sw.FlashOn = False
                                    sw.Flasher.Interval = 250
                                End If
                            Next

                        End If
                    Else
                        If Not optDisableCheckForReversePermission Then
                            If Not Me.CheckForReversePermission Then
                                Exit Sub
                            End If
                        End If

                        If Not Me.IsSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionStatic, True, e) Then

                            If gSendCTC Then
                                SetCursor(curSend)
                            Else
                                If Not gQuickRouteMode Then
                                    SetCursor(curDefault)
                                Else
                                    SetCursor(curQuickRoute)
                                End If
                            End If
                        End If
                    End If

                ElseIf e.Button = Windows.Forms.MouseButtons.Right Then

                    If Me.JointControl And Me.myControlPoint.IsShadowMode Then
                        ContextMenuEnabled(EnableJointControl)
                    Else
                        ContextMenuEnabled(True)
                    End If

                    RefreshContextMenu()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                button = Nothing
            End Try
        End Sub

        Public Sub Switch_MouseEnter(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = True
            MouseLefted = False
            Refresh()
        End Sub

        Public Sub Switch_MouseLeave(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = False
            MouseLefted = True
            Refresh()
        End Sub

        Public Sub Switch_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                If ToolTipMessage = "" Then
                    If optTrackToolTipFormatWithControlPointMilepost Then
                        ToolTipMessage = ToolTipMessage & "(" & Me.Name.ToUpper & ") (" & Me.myControlPoint.Name & ") (" & Me.MilePost & ")"
                    ElseIf optEnableUsingTooltipField Then
                        ToolTipMessage = ToolTipMessage & Me.myToolTip
                    Else
                        ToolTipMessage = ToolTipMessage & Me.Name.ToUpper
                    End If
                End If


                If Me.SwitchPointerMonitorMode Then
                    ToolTipMessage = ToolTipMessage & vbCrLf & "LAST REPORT @ " & Format(Me.LastReportTime, "HH:mm:ss") & " (" & Mid(Me.AlarmTime.TotalMinutes.ToString, 1, 4) & ")"
                    If ManualSwitchPointVerification Then
                        ToolTipMessage = ToolTipMessage & vbCrLf & "(MANUALLY VERIFIED NORMAL)"
                    End If
                End If
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub DoDeviceBlock()
            Try
                If Me.AuthorityType = "SHELF" Then Exit Sub
                If Not Me.AllowBlocking Then Exit Sub
                If Not gDeviceBlockSelection.Contains(Me) Then
                    gDeviceBlockSelection.Add(Me)
                    If gBlockOSDevices Then
                        Dim comp As Object
                        If Not Me.OsComponentCollection Is Nothing Then

                            For Each comp In Me.OsComponentCollection
                                If Not gDeviceBlockSelection.Contains(comp) Then
                                    gDeviceBlockSelection.Add(comp)
                                    comp.SelectionMode = ModeSelection.DeviceBlocking
                                    comp.FlashComponent = True
                                End If
                            Next
                        End If
                    Else
                        Dim sw As Switch
                        For Each sw In Me.BrotherSwitches
                            If Not sw Is Me Then
                                gDeviceBlockSelection.Add(sw)
                                sw.SelectionMode = ModeSelection.DeviceBlocking
                                sw.Flasher.Interval = 150
                                sw.FlashComponent = True
                            End If
                        Next
                    End If

                    Me.SelectionMode = ModeSelection.DeviceBlocking
                    Me.FlashComponent = True
                    TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                Else

                    If gBlockOSDevices Then
                        If Not Me.OsComponentCollection Is Nothing Then
                            Dim comp As Object
                            For Each comp In Me.OsComponentCollection
                                gDeviceBlockSelection.Remove(comp)
                                comp.SelectionMode = ModeSelection.Regular
                                comp.FlasherMode(False)
                                comp.FlashComponent = False
                                comp.Flasher.Interval = 250
                            Next
                        End If
                    Else
                        Dim sw As Switch
                        For Each sw In Me.BrotherSwitches
                            If Not sw Is Me Then
                                gDeviceBlockSelection.Remove(sw)
                                sw.SelectionMode = ModeSelection.Regular
                                sw.FlasherMode(False)
                                sw.FlashComponent = False
                                sw.Refresh()
                            End If
                        Next
                    End If
                    gDeviceBlockSelection.Remove(Me)
                    Me.SelectionMode = ModeSelection.Regular
                    Me.FlasherMode(False)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ReRoutePssForCP()
            Dim OldRouteCollection As New ArrayList
            Dim comp As Object = Nothing
            Dim ChangeSwPosDetected As Boolean = False
            Try
                CopyArrayCollection(OldRouteCollection, gEntExtTrackSelection)
                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()

                For Each comp In OldRouteCollection
                    If comp.GUID = Me.GUID Then
                        'ChangeSwPosDetected = True
                        Exit For
                    Else
                        If Not gEntExtTrackSelection.Contains(comp) Then gEntExtTrackSelection.Add(comp)
                    End If

                Next
                FindAlternateRoute(gEntExtTrackSelection)
                For Each comp In gEntExtTrackSelection
                    'comp.TrafficDirection = "LEFT"
                    comp.UpdateStatus()
                Next
                TMDS.SystemForms.MySystemForms.frmMainMDI.EntExtFlasherReset()
                If Not gPermissionPassRedSignalSwitches.Contains(Me) Then gPermissionPassRedSignalSwitches.Add(Me)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try


        End Sub

        Private Sub FindAlternateRoute(ByRef SelectionList As ArrayList)
            Dim NextComp As Object = Nothing
            Dim SearchComplete As Boolean = False

            Select Case gSearchDirection
                Case "LEFT"
                    If Me.ControlledReverse Then
                        NextComp = FindCompFromGUID(Me.LeftLink)
                    Else
                        NextComp = FindCompFromGUID(Me.LeftAltLink)
                    End If

                Case "RIGHT"
                    If Me.ControlledReverse Then
                        NextComp = FindCompFromGUID(Me.RightLink)
                    Else
                        NextComp = FindCompFromGUID(Me.RightAltLink)
                    End If

            End Select

            SelectionList.Add(Me)
            Try
                Select Case gSearchDirection
                    Case "LEFT"
                        'NextComp = FindCompFromGUID(Comp.LeftLink)

                        Do Until SearchComplete
                            If SearchComplete Then Exit Sub
                            If NextComp Is Nothing Then Exit Sub

                            Select Case NextComp.GetType.ToString
                                Case gTrackComponentName
                                    SelectionList.Add(NextComp)
                                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                                Case gSignalComponentName

                                    If (NextComp.ComponentType = "L" Or NextComp.ComponentType = "U") Then
                                        SearchComplete = True
                                        NextComp = Nothing
                                    Else
                                        SelectionList.Add(NextComp)
                                        NextComp = FindCompFromGUID(NextComp.LeftLink)
                                    End If
                                Case Else
                                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                            End Select
                        Loop

                    Case "RIGHT"
                        Do Until SearchComplete
                            If SearchComplete Then Exit Sub
                            If NextComp Is Nothing Then Exit Sub

                            Select Case NextComp.GetType.ToString
                                Case gTrackComponentName
                                    SelectionList.Add(NextComp)
                                    NextComp = FindCompFromGUID(NextComp.RightLink)
                                Case gSignalComponentName
                                    If (NextComp.ComponentType = "R" Or NextComp.ComponentType = "D") Then
                                        SearchComplete = True
                                        NextComp = Nothing
                                    Else
                                        SelectionList.Add(NextComp)
                                        NextComp = FindCompFromGUID(NextComp.RightLink)
                                    End If
                                Case Else
                                    NextComp = FindCompFromGUID(NextComp.RightLink)
                            End Select
                        Loop

                End Select
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try



        End Sub


#End Region

#Region "CONTEXT MENU"

        Public Sub ContextMenuEnabled(ByVal EnableMenuItem As Boolean)
            Try
                Dim mi As MenuItem
                For Each mi In Me.ContextMenu.MenuItems
                    mi.Enabled = EnableMenuItem
                Next
            Catch
            End Try
        End Sub

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)
            'SelectStation()
            MouseEntered = True
            MouseLefted = False
            'Me.Invalidate()
            'Me.Update()
            Me.Refresh()

        End Sub

        Public Sub BuildContextMenu()
            Try


                If gTrainerMode Then
                    Me.ContextMenuControl = New ContextMenu()
                    AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                    Me.mnuTrainerIndNormal = New MenuItem("SIMULATE-NORMAL-INDICATION")
                    AddHandler mnuTrainerIndNormal.Click, AddressOf Me.mnuTrainerIndNormal_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerIndNormal)

                    Me.mnuTrainerIndReverse = New MenuItem("SIMULATE-REVERSE-INDICATION")
                    AddHandler mnuTrainerIndReverse.Click, AddressOf Me.mnuTrainerIndReverse_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerIndReverse)

                    Me.mnuTrainerIndNoCorespondence = New MenuItem("SIMULATE-NO-CORESPONDENCE")
                    AddHandler mnuTrainerIndNoCorespondence.Click, AddressOf Me.mnuTrainerIndNoCorespondence_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerIndNoCorespondence)

                    Me.mnuTrainerIndDisable = New MenuItem("DISABLE-SIMULATED-INDICATION")
                    AddHandler mnuTrainerIndDisable.Click, AddressOf Me.mnuTrainerIndDisable_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerIndDisable)


                    Me.ContextMenu = Me.ContextMenuControl
                    Exit Sub
                ElseIf gAssistMode Then
                    If optUseTroubleReportingEnhanced AndAlso optEnableSpecificFunctionalityForDenver Then 'CMV DTP-115
                        Me.ContextMenuControl = New ContextMenu()
                        AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp
                        AddTroubleReportMenuItem()
                        Me.ContextMenu = Me.ContextMenuControl
                    End If
                    Exit Sub
                End If

                'Me.SwitchEvent("BuildContextMenu:" & Me.GUID.ToString)
                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                mnuDBCreate = New MenuItem(optGraphicalCompContextMenuSwBlockCreate)
                AddHandler mnuDBCreate.Click, AddressOf Me.mnuDBCreate_Click
                ContextMenuControl.MenuItems.Add(mnuDBCreate)

                mnuDBRemove = New MenuItem(optGraphicalCompContextMenuSwBlockRemove)
                AddHandler mnuDBRemove.Click, AddressOf Me.mnuDBRemove_Click
                ContextMenuControl.MenuItems.Add(mnuDBRemove)

                mnuDBOS = New MenuItem(optGraphicalCompContextMenuSwBlockOS)
                AddHandler mnuDBOS.Click, AddressOf Me.mnuDBOS_Click
                ContextMenuControl.MenuItems.Add(mnuDBOS)

                If Me.SwitchPointerMonitorMode Then
                    Dim mnuDiv5 As MenuItem = New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuDiv5)

                    mnuSPMVerifyAdd = New MenuItem("VERFIED NORMAL")
                    AddHandler mnuSPMVerifyAdd.Click, AddressOf Me.mnuSPMVerifyAdd_Click
                    ContextMenuControl.MenuItems.Add(mnuSPMVerifyAdd)

                    mnuSPMVerfiyRemove = New MenuItem("REMOVE VERFICATION")
                    AddHandler mnuSPMVerfiyRemove.Click, AddressOf Me.mnuSPMVerfiyRemove_Click
                    ContextMenuControl.MenuItems.Add(mnuSPMVerfiyRemove)

                    mnuSPMOverride = New MenuItem("APPLY OVERRIDE POSITION REPORTING")
                    AddHandler mnuSPMOverride.Click, AddressOf Me.mnuSPMOverride_Click
                    ContextMenuControl.MenuItems.Add(mnuSPMOverride)

                    mnuSPMOverrideRemove = New MenuItem("REMOVE OVERRIDE POSITION REPORTING")
                    AddHandler mnuSPMOverrideRemove.Click, AddressOf Me.mnuSPMOverrideRemove_Click
                    ContextMenuControl.MenuItems.Add(mnuSPMOverrideRemove)
                End If

                If AllowIndependentControlSwitch Then
                    mnuICSwitch = New MenuItem("CONTROL SWITCH END")
                    AddHandler mnuICSwitch.Click, AddressOf Me.mnuICSwitch_Click
                    ContextMenuControl.MenuItems.Add(mnuICSwitch)
                End If

                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem()
                End If

                Me.ContextMenu = Me.ContextMenuControl

                If (Me.IsShadowMode AndAlso Not Me.JointControl) Then
                    Dim m As MenuItem
                    For Each m In ContextMenu.MenuItems
                        m.Visible = False
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RefreshContextMenu()
            Try

                BuildBlockMenus()

                If Me.SwitchPointerMonitorMode Then
                    BuildSwitchPointMenu()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

#Region "MENU BUILDS"

        Private Sub BuildBlockMenus()
            'SwitchEvent("BuildBlockMenus")

            Try
                Dim miRemove As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuDBRemove))
                miRemove.MenuItems.Clear()

                If Me.DeviceBlocks.Count > 0 Then
                    Dim auth As TrackAuthority.DeviceBlock
                    miRemove.Visible = True
                    For Each auth In Me.DeviceBlocks
                        'Me.mnuDBRemove.MenuItems.Add(New MenuItem(auth.AuthorityUID & ", (" & auth.IssueTo & ")", AddressOf mnuDBRemove_Click))
                        Me.mnuDBRemove.MenuItems.Add(New MenuItem(Format(auth.AuthorityDate, "HH:mm:ss") & ", (" & auth.IssueTo & ")", AddressOf mnuDBRemove_Click))
                    Next
                Else
                    miRemove.Visible = False
                    'ContextMenuControl.MenuItems.Remove(mnuDBRemove)
                End If

                If (Me.IsShadowMode AndAlso Not Me.JointControl) Then
                    Dim m As MenuItem
                    For Each m In ContextMenu.MenuItems
                        m.Visible = False
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub BuildSwitchPointMenu()

            Try
                If ManualSwitchPointVerification Then
                    Me.mnuSPMVerifyAdd.Visible = False
                    Me.mnuSPMVerfiyRemove.Visible = True
                ElseIf Me.SwitchIndStatus <> SwIndStatus.NormalInd Or Me.FailState Then
                    Me.mnuSPMVerifyAdd.Visible = True
                    Me.mnuSPMVerfiyRemove.Visible = False
                Else
                    Me.mnuSPMVerifyAdd.Visible = False
                    Me.mnuSPMVerfiyRemove.Visible = False
                End If

                Dim miOverride As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuSPMOverride))
                miOverride.MenuItems.Clear()

                If Me.TrackAuthorities.Count > 0 Then
                    Dim auth As TrackAuthority.SFAuthority
                    miOverride.Visible = True
                    For Each auth In Me.TrackAuthorities
                        If Not SwitchOverrideExists(auth.AuthorityUID) Then
                            Me.mnuSPMOverride.MenuItems.Add(New MenuItem(auth.AuthorityUID & ", (" & auth.IssueTo & ")", AddressOf mnuSPMOverride_Click))
                        End If
                    Next
                    If mnuSPMOverride.MenuItems.Count = 0 Then
                        miOverride.Visible = False
                    End If
                Else
                    miOverride.Visible = False
                End If


                Dim miOverrideRemove As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuSPMOverrideRemove))
                miOverrideRemove.MenuItems.Clear()

                If Me.TrackAuthorities.Count > 0 Then
                    Dim auth As TrackAuthority.SFAuthority
                    miOverrideRemove.Visible = True
                    For Each auth In Me.TrackAuthorities
                        If SwitchOverrideExists(auth.AuthorityUID) Then
                            Me.mnuSPMOverrideRemove.MenuItems.Add(New MenuItem(auth.AuthorityUID & ", (" & auth.IssueTo & ")", AddressOf mnuSPMOverrideRemove_Click))
                        End If
                    Next
                    If mnuSPMOverrideRemove.MenuItems.Count = 0 Then
                        miOverrideRemove.Visible = False
                    End If
                Else
                    miOverrideRemove.Visible = False
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Function SwitchOverrideExists(ByVal AuthUID As String) As Boolean
            Try
                Dim so As SwitchPointOverride
                For Each so In Me.SwitchOverrideList
                    If so.AuthUID = AuthUID Then
                        Return True
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function
#End Region

#Region "BLOCK MENU"

        Private Sub mnuDBCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - Create Device Block from CM")
                gCreateDeviceBlock = True
                gBlockAllDevices = True
                SetCursor(curCreateDB)
                Me.DoDeviceBlock()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuDBOS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - Create Device OS Block from CM")
                gCreateDeviceBlock = True
                gBlockAllDevices = True
                gBlockOSDevices = True
                SetCursor(curCreateDB)
                Me.DoDeviceBlock()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuDBRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Dim mi As MenuItem = DirectCast(sender, MenuItem)
                Dim BlkData() As String = mi.Text.Split(",")
                Dim blk As TrackAuthority.DeviceBlock = FindTrackBlock(BlkData(0))
                If blk Is Nothing Then Exit Sub
                SwitchEventLog("Clicked - Remove Device Block from CM:" & blk.AuthorityUID)
                If Not IsNothing(SystemForms.MySystemForms.frmReportClearDeviceBlock) Then
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.Dispose()
                    SystemForms.MySystemForms.frmReportClearDeviceBlock = Nothing
                End If
                SystemForms.MySystemForms.frmReportClearDeviceBlock = New frmReportClearDeviceBlock()
                SystemForms.MySystemForms.frmReportClearDeviceBlock.Location = Me.CurrentDrawingForm.myLaunchPoint
                SystemForms.MySystemForms.frmReportClearDeviceBlock.LoadForm()
                If optDeviceBlockingSimpleRelease Then
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.SimpleReportClearEvent(blk)
                Else
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.SingleReportClearEvent(blk)
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.ShowDialog()
                End If
                SystemForms.MySystemForms.frmReportClearDeviceBlock.Dispose()
                SystemForms.MySystemForms.frmReportClearDeviceBlock = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "SWITCH POINT MENU"

        Private Sub mnuSPMVerifyAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - VERIFY SWITCH NORMAL FROM CM")
                Me.DoVerifySwitchNormalPosition()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuSPMVerfiyRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - REMOVE SWITCH VERIFICATION FROM CM")
                Me.DoRemoveSwitchVerification()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuSPMOverride_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - APPLY OVERRIDE POSITION REPORTING FROM CM")
                Dim mi As MenuItem = DirectCast(sender, MenuItem)
                Dim data() As String = mi.Text.Split(",")

                Dim so As New SwitchPointOverride
                so.OverrideTime = Now
                so.AuthUID = data(0)

                Me.SwitchOverrideList.Add(so)
                If Me.SwitchOverrideList.Contains(so) Then
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS BEEN ADDED!")
                    Me.AcknowledgeSwitchOpenAlertFromOverride(so.AuthUID)
                    Me.UpdateSwitchPointStatus()
                Else
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS NOT BEEN ADDED!")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuSPMOverrideRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - REMOVE OVERRIDE POSITION REPORTING FROM CM")
                Dim mi As MenuItem = DirectCast(sender, MenuItem)
                Dim data() As String = mi.Text.Split(",")
                Dim so As SwitchPointOverride = FindSwitchOverride(data(0))
                If IsNothing(so) Then Exit Sub

                Me.SwitchOverrideList.Remove(so)
                If Not Me.SwitchOverrideList.Contains(so) Then
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS BEEN REMOVED!")
                    If Me.SwitchIndStatus <> SwIndStatus.NormalInd And Not Me.ManualSwitchPointVerification Then
                        Me.CheckForExistingAuthorities()
                    End If

                    Me.UpdateSwitchPointStatus()
                Else
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS NOT BEEN REMOVED!")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function FindSwitchOverride(ByVal AuthUID As String) As SwitchPointOverride
            Try

                Dim so As SwitchPointOverride
                For Each so In Me.SwitchOverrideList
                    If so.AuthUID = AuthUID Then
                        Return so
                    End If
                Next
                Return Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function
#End Region

#Region "TRAINER MODE MENUS"

        Private Sub mnuTrainerIndNormal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("NORMAL")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuTrainerIndReverse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("REVERSE")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub mnuTrainerIndNoCorespondence_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("NO-CORESPONDENCE")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub mnuTrainerIndDisable_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("DISABLE-INDICATION")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

#End Region

#Region "ICS"

        Private Sub mnuICSwitch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SwitchEventLog("Clicked - Control Independent End Of Switch:" & Me.GUID.ToString)
                If gCritcalPromptExists Then
                    SwitchEventLog("CRITICAL PROMPT EXISTS - EXITING MOUSE SELECTION:" & Me.GUID.ToString)
                    Exit Sub
                End If

                If Not Me.IsSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionStatic, True, Nothing, True) Then



                    If gSendCTC Then
                        SetCursor(curSend)
                    Else
                        If Not gQuickRouteMode Then
                            SetCursor(curDefault)
                        Else
                            SetCursor(curQuickRoute)
                        End If
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "MENU FUNCTIONS"

        Private Function FindTrackBlock(ByVal BlockTime As String) As TrackAuthority.DeviceBlock
            Try
                Dim blk As TrackAuthority.DeviceBlock
                For Each blk In Me.DeviceBlocks
                    'If blk.AuthorityUID = BlockUID Then
                    '    Return blk
                    'End If
                    If Format(blk.AuthorityDate, "HH:mm:ss") = BlockTime Then
                        Return blk
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

#End Region

#End Region

#Region "GRAPHIC METHODS"

        Public Sub RefreshRemotely()
            Try
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub FlashElapsedEvent(ByVal source As Object, ByVal e As EventArgs)

        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()


        End Sub

        Public Sub DrawIt()
            Dim g As Graphics
            Try
                If IsNothing(Me.Graphical) Then
                    Me.Graphical = Me.CreateGraphics
                End If
                g = Me.Graphical

                If FlashComponent Then
                    If FlashOn Then
                        If SelectionMode = ModeSelection.DeviceBlocking Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusBlocking, Me.NoPath)
                                    Else
                                        'g.FillPath(ColorStatusBlocking, SwitchPath)
                                        g.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusBlocking, SwitchPath)
                                    g.FillPath(ColorStatusBlocking, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.RouteRequesting Or SelectionMode = ModeSelection.SummarySelectionRouting Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusRouteRequesting, Me.NoPath)
                                    Else
                                        'g.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                        g.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                    g.FillPath(ColorStatusRouteRequesting, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.SummarySelectionBlocking Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorSummarySelectionBlocking, Me.NoPath)
                                    Else
                                        g.FillPath(ColorSummarySelectionBlocking, SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorSummarySelectionBlocking, SwitchPath)
                                    g.FillPath(ColorSummarySelectionBlocking, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCIssuanceImminent Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusPTCIssuanceImminent, Me.NoPath)
                                    Else
                                        g.FillPath(ColorStatusPTCIssuanceImminent, SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusPTCIssuanceImminent, SwitchPath)
                                    g.FillPath(ColorStatusPTCIssuanceImminent, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCIssuancePending Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusPTCIssuancePending, Me.NoPath)
                                    Else
                                        g.FillPath(ColorStatusPTCIssuancePending, SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusPTCIssuancePending, SwitchPath)
                                    g.FillPath(ColorStatusPTCIssuancePending, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCReleasePending Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusPTCReleasePending, Me.NoPath)
                                    Else
                                        g.FillPath(ColorStatusPTCReleasePending, SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusPTCReleasePending, SwitchPath)
                                    g.FillPath(ColorStatusPTCReleasePending, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCReleaseImminent Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(ColorStatusPTCReleaseImminent, Me.NoPath)
                                    Else
                                        g.FillPath(ColorStatusPTCReleaseImminent, SwitchPath)
                                    End If
                                Case Else
                                    g.FillPath(ColorStatusPTCReleaseImminent, SwitchPath)
                                    g.FillPath(ColorStatusPTCReleaseImminent, Me.NoPath)
                            End Select
                        Else
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        g.FillPath(New SolidBrush(Color.LightGray), Me.NoPath)
                                        'g.FillPath(CurrentColorStatus, Me.NoPath)
                                    Else
                                        'g.FillPath(CurrentColorStatus, SwitchPath)
                                        g.FillPath(New SolidBrush(Color.Gray), SwitchPath)
                                    End If
                                Case Else
                                    If Me.NonTransverable Then
                                        g.FillPath(CurrentColorStatusNoPath, SwitchPath)
                                        g.FillPath(CurrentColorStatusNoPath, Me.NoPath)
                                    Else
                                        g.FillPath(CurrentColorStatus, SwitchPath)
                                        g.FillPath(CurrentColorStatusNoPath, Me.NoPath)

                                    End If
                            End Select
                        End If
                    Else
                        Select Case Me.TypeOfSwitch
                            Case "DERAIL"
                                If Not Me.ControlledReverse Then
                                    If Not IsNothing(Me.NoPath) Then
                                        g.FillPath(FlashOffColorStatus, Me.NoPath)
                                    End If
                                Else
                                    g.FillPath(FlashOffColorStatus, SwitchPath)
                                    'g.FillPath(New SolidBrush(Color.Gray), SwitchPath)
                                End If

                            Case Else
                                If Me.NonTransverable Then
                                    g.FillPath(FlashOffColorStatus, SwitchPath)
                                    g.FillPath(FlashOffColorStatus, Me.NoPath)
                                Else
                                    g.FillPath(FlashOffColorStatus, SwitchPath)
                                    g.FillPath(FlashOffColorStatus, Me.NoPath)
                                    'g.FillPath(CurrentColorStatusNoPath, Me.NoPath)
                                End If
                        End Select
                    End If
                Else
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(New SolidBrush(Color.LightGray), Me.NoPath)
                                'g.FillPath(CurrentColorStatus, Me.NoPath)
                            Else
                                g.FillPath(New SolidBrush(Color.Gray), Me.SwitchPath)
                            End If

                        Case Else
                            If Me.NonTransverable Then
                                g.FillPath(New SolidBrush(Color.Gray), Me.SwitchPath)
                                g.FillPath(New SolidBrush(Color.Gray), Me.NoPath)
                            Else
                                g.FillPath(CurrentColorStatus, Me.SwitchPath)
                                g.FillPath(CurrentColorStatusNoPath, Me.NoPath)

                            End If
                    End Select
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                If SelectionMode = ModeSelection.DeviceBlocking Then
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(ColorStatusBlocking, Me.NoPathRelative)
                            Else
                                'G.FillPath(ColorStatusBlocking, SwitchPathRelative)
                                g.FillPath(New SolidBrush(Color.LightGray), Me.SwitchPathRelative)
                            End If
                        Case Else
                            g.FillPath(ColorStatusBlocking, SwitchPathRelative)
                            g.FillPath(ColorStatusBlocking, Me.NoPathRelative)
                    End Select
                ElseIf SelectionMode = ModeSelection.RouteRequesting Then
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(ColorStatusRouteRequesting, Me.NoPathRelative)
                            Else
                                'G.FillPath(ColorStatusRouteRequesting, SwitchPathRelative)
                                g.FillPath(New SolidBrush(Color.LightGray), SwitchPathRelative)
                            End If
                        Case Else
                            g.FillPath(ColorStatusRouteRequesting, SwitchPathRelative)
                            g.FillPath(ColorStatusRouteRequesting, Me.NoPathRelative)
                    End Select
                ElseIf SelectionMode = ModeSelection.SummarySelectionRouting Then
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(Me.ColorSummarySelectionRequesting, Me.NoPathRelative)
                            Else
                                'G.FillPath(ColorStatusRouteRequesting, SwitchPathRelative)
                                g.FillPath(New SolidBrush(Color.LightGray), SwitchPathRelative)
                            End If
                        Case Else
                            g.FillPath(ColorSummarySelectionRequesting, SwitchPathRelative)
                            g.FillPath(ColorSummarySelectionRequesting, Me.NoPathRelative)
                    End Select

                ElseIf SelectionMode = ModeSelection.SummarySelectionBlocking Then
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(ColorSummarySelectionBlocking, Me.NoPathRelative)
                            Else
                                g.FillPath(ColorSummarySelectionBlocking, SwitchPathRelative)
                            End If
                        Case Else
                            g.FillPath(ColorSummarySelectionBlocking, SwitchPathRelative)
                            g.FillPath(ColorSummarySelectionBlocking, Me.NoPathRelative)
                    End Select
                Else
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                g.FillPath(New SolidBrush(Color.LightGray), Me.NoPathRelative)
                                'G.FillPath(CurrentColorStatus, Me.NoPathRelative)
                            Else
                                'G.FillPath(CurrentColorStatus, SwitchPathRelative)
                                g.FillPath(New SolidBrush(Color.Gray), SwitchPathRelative)
                            End If
                        Case Else
                            If Me.NonTransverable Then
                                g.FillPath(CurrentColorStatusNoPath, SwitchPathRelative)
                                g.FillPath(CurrentColorStatusNoPath, Me.NoPathRelative)
                            Else
                                g.FillPath(CurrentColorStatus, SwitchPathRelative)
                                g.FillPath(CurrentColorStatusNoPath, Me.NoPathRelative)

                            End If
                    End Select
                End If
                g.Dispose()
                g = Nothing
                If Not Me.mySwitchLock Is Nothing Then
                    Me.mySwitchLock.DrawToBitmap(bp)
                End If
            Catch ex As Exception
            Finally
                g = Nothing
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                If CurrentDrawingForm.Initializing Then Exit Sub

                If Me.ControlledReverse And Me.SwitchIndStatus <> SwIndStatus.ReverseInd Then
                    If Not FlashComponent Then
                        'Me.SwitchEvent("DEBUG-OnPaint-SetSwitchToOutOfCorespondenceReverse:" & Me.GUID.ToString)
                        'Me.SetSwitchToOutOfCorespondenceReverse()
                    End If
                ElseIf Not Me.ControlledReverse And Me.SwitchIndStatus <> SwIndStatus.NormalInd Then
                    If Not FlashComponent Then
                        'Me.SwitchEvent("DEBUG-OnPaint-SetSwitchToOutOfCorespondenceNormal:" & Me.GUID.ToString)
                        'Me.SetSwitchToOutOfCorespondenceNormal()
                    End If
                End If
                If FlashComponent Then
                    If FlashOn Then
                        If SelectionMode = ModeSelection.DeviceBlocking Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusBlocking, Me.NoPath)
                                    Else
                                        'e.Graphics.FillPath(ColorStatusBlocking, SwitchPath)
                                        e.Graphics.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusBlocking, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusBlocking, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.TagBlocking Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusTagRequesting, Me.NoPath)
                                    Else
                                        'e.Graphics.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                        e.Graphics.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusTagRequesting, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusTagRequesting, Me.NoPath)
                            End Select

                        ElseIf SelectionMode = ModeSelection.RouteRequesting Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusRouteRequesting, Me.NoPath)
                                    Else
                                        'e.Graphics.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                        e.Graphics.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusRouteRequesting, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.SummarySelectionRouting Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(Me.ColorSummarySelectionRequesting, Me.NoPath)
                                    Else
                                        'e.Graphics.FillPath(ColorStatusRouteRequesting, SwitchPath)
                                        e.Graphics.FillPath(New SolidBrush(Color.LightGray), SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorSummarySelectionRequesting, SwitchPath)
                                    e.Graphics.FillPath(ColorSummarySelectionRequesting, Me.NoPath)
                            End Select

                        ElseIf SelectionMode = ModeSelection.SummarySelectionBlocking Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorSummarySelectionBlocking, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(ColorSummarySelectionBlocking, SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorSummarySelectionBlocking, SwitchPath)
                                    e.Graphics.FillPath(ColorSummarySelectionBlocking, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCIssuanceImminent Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusPTCIssuanceImminent, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(ColorStatusPTCIssuanceImminent, SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusPTCIssuanceImminent, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusPTCIssuanceImminent, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCIssuancePending Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusPTCIssuancePending, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(ColorStatusPTCIssuancePending, SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusPTCIssuancePending, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusPTCIssuancePending, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCReleasePending Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusPTCReleasePending, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(ColorStatusPTCReleasePending, SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusPTCReleasePending, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusPTCReleasePending, Me.NoPath)
                            End Select
                        ElseIf SelectionMode = ModeSelection.PTCReleaseImminent Then
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(ColorStatusPTCReleaseImminent, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(ColorStatusPTCReleaseImminent, SwitchPath)
                                    End If
                                Case Else
                                    e.Graphics.FillPath(ColorStatusPTCReleaseImminent, SwitchPath)
                                    e.Graphics.FillPath(ColorStatusPTCReleaseImminent, Me.NoPath)
                            End Select
                        Else
                            Select Case Me.TypeOfSwitch
                                Case "DERAIL"
                                    If Not Me.ControlledReverse Then
                                        e.Graphics.FillPath(New SolidBrush(Color.LightGray), Me.NoPath)
                                        'e.Graphics.FillPath(CurrentColorStatus, Me.NoPath)
                                    Else
                                        'e.Graphics.FillPath(CurrentColorStatus, SwitchPath)
                                        e.Graphics.FillPath(New SolidBrush(Color.Gray), SwitchPath)
                                    End If
                                Case Else
                                    If Me.NonTransverable Then
                                        e.Graphics.FillPath(CurrentColorStatusNoPath, SwitchPath)
                                        e.Graphics.FillPath(CurrentColorStatusNoPath, Me.NoPath)
                                    Else
                                        e.Graphics.FillPath(CurrentColorStatus, SwitchPath)
                                        e.Graphics.FillPath(CurrentColorStatusNoPath, Me.NoPath)

                                    End If
                            End Select
                        End If
                    Else
                        Select Case Me.TypeOfSwitch
                            Case "DERAIL"
                                If Not Me.ControlledReverse Then
                                    e.Graphics.FillPath(FlashOffColorStatus, Me.NoPath)
                                Else
                                    e.Graphics.FillPath(FlashOffColorStatus, SwitchPath)
                                    'e.Graphics.FillPath(New SolidBrush(Color.Gray), SwitchPath)
                                End If

                            Case Else
                                If Me.NonTransverable Then
                                    e.Graphics.FillPath(FlashOffColorStatus, SwitchPath)
                                    e.Graphics.FillPath(FlashOffColorStatus, Me.NoPath)
                                Else
                                    e.Graphics.FillPath(FlashOffColorStatus, SwitchPath)
                                    e.Graphics.FillPath(FlashOffColorStatus, Me.NoPath)
                                    'e.Graphics.FillPath(CurrentColorStatusNoPath, Me.NoPath)
                                End If
                        End Select
                    End If
                Else
                    Select Case Me.TypeOfSwitch
                        Case "DERAIL"
                            If Not Me.ControlledReverse Then
                                e.Graphics.FillPath(New SolidBrush(Color.LightGray), Me.NoPath)
                                'e.Graphics.FillPath(CurrentColorStatus, Me.NoPath)
                            Else
                                e.Graphics.FillPath(New SolidBrush(Color.Gray), Me.SwitchPath)
                            End If

                        Case Else
                            If Me.NonTransverable Then
                                If HatchItSpeed Or HatchItWorkZone Or HatchItFormO Then
                                    Dim myHatch As HatchBrush = Nothing
                                    If HatchItWorkZone Then
                                        myHatch = New HatchBrush(HatchStyle.DarkVertical, Color.Black, CurrentColorStatus.Color)
                                    ElseIf HatchItSpeed Then
                                        myHatch = New HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Black, CurrentColorStatus.Color)
                                    ElseIf HatchItFormO Then
                                        myHatch = New HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, CurrentColorStatus.Color)
                                    End If
                                    e.Graphics.FillPath(myHatch, Me.SwitchPath)
                                    e.Graphics.FillPath(myHatch, Me.NoPath)
                                Else
                                    e.Graphics.FillPath(New SolidBrush(Color.Gray), Me.SwitchPath)
                                    e.Graphics.FillPath(New SolidBrush(Color.Gray), Me.NoPath)
                                End If
                            Else
                                If HatchItSpeed Or HatchItWorkZone OrElse HatchItFormO Then
                                    Dim myHatch1 As HatchBrush = Nothing
                                    Dim myHatch2 As HatchBrush = Nothing
                                    If HatchItWorkZone Then
                                        myHatch1 = New HatchBrush(HatchStyle.DarkVertical, Color.Black, CurrentColorStatus.Color)
                                        myHatch2 = New HatchBrush(HatchStyle.DarkVertical, Color.Black, CurrentColorStatusNoPath.Color)
                                    ElseIf HatchItSpeed Then
                                        myHatch1 = New HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Black, CurrentColorStatus.Color)
                                        myHatch2 = New HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Black, CurrentColorStatusNoPath.Color)
                                    ElseIf HatchItFormO Then
                                        myHatch1 = New HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, CurrentColorStatus.Color)
                                        myHatch2 = New HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, CurrentColorStatusNoPath.Color)
                                    End If
                                    e.Graphics.FillPath(myHatch1, Me.SwitchPath)
                                    e.Graphics.FillPath(myHatch2, Me.NoPath)
                                Else
                                    e.Graphics.FillPath(CurrentColorStatus, Me.SwitchPath)
                                    e.Graphics.FillPath(CurrentColorStatusNoPath, Me.NoPath)
                                    If optEnableSwitchMuteColor AndAlso Me.TypeOfSwitch = "SPM ONLY" Then
                                        e.Graphics.FillPath(gColorSwitchMonitorMute, Me.SwitchPath)
                                    End If
                                    If Me.JointControl Then
                                        'e.Graphics.DrawPath(New Pen(Color.Red, 2), GraphicalPath)
                                    End If

                                End If
                            End If
                    End Select
                End If
                If Me.MouseEntered Then
                    e.Graphics.DrawPath(New Pen(Color.Red, 4), OutLinePath)
                ElseIf Me.MouseLefted Then
                    Me.MouseLefted = False
                End If

                If Me.SwitchPointerMonitorMode And Not Me.FailState Then
                    If Me.SwitchIndStatus = SwIndStatus.NoIndication Then
                        e.Graphics.FillPath(gColorTrackNotUsable, SwitchPath)
                        e.Graphics.FillPath(gColorTrackNotUsable, Me.NoPath)

                    ElseIf Me.ControlledReverse And Me.SwitchIndStatus <> SwIndStatus.ReverseInd Then
                        e.Graphics.FillPath(gColorTrackNotUsable, SwitchPath)
                        e.Graphics.FillPath(gColorTrackNotUsable, Me.NoPath)
                    ElseIf Not Me.ControlledReverse And Me.SwitchIndStatus <> SwIndStatus.NormalInd Then
                        e.Graphics.FillPath(gColorTrackNotUsable, SwitchPath)
                        e.Graphics.FillPath(gColorTrackNotUsable, Me.NoPath)
                    End If
                End If

            Catch ex As Exception
                If gDebug Then
                    SystemException(ex.ToString)
                End If
            End Try
        End Sub

        Overrides Sub RefreshPath()
            Try

                Me.Visible = False
                SwitchPath = New GraphicsPath()
                OutLinePath = New GraphicsPath()
                NoPath = New GraphicsPath()
                Path = New GraphicsPath()
                Dim TurnOutTrackIndex As Integer
                Dim gpath As New GraphicsPath()
                Dim pt1 As New Point()
                Dim pt2 As New Point()
                Dim pt3 As New Point()
                Dim pt4 As New Point()
                Dim ShowSwitchLock As Boolean = True
                If Me.SwitchPointerMonitorMode Then
                    If Not optShowSwitchLockWithSwitchPointMonitors Then
                        ShowSwitchLock = False
                    End If
                    If optSwitchPointMonitoringForBNSF Then
                        Me.TypeOfSwitch = "SWITCH"
                    End If
                End If

                'optDisableSPMSwitchLock
                Select Case Me.TypeOfSwitch
                    Case "SPM ONLY"
                        If optHideSPMSwitchLock Then
                            ShowSwitchLock = False
                        ElseIf optHideSPMSwitchLockOnNoLink Then
                            If Me.TypeOfSwitch = "SPM ONLY" Then
                                If Me.RightLink = 0 AndAlso Me.LeftLink = 0 Then
                                    ShowSwitchLock = False
                                End If
                            End If
                        End If
                End Select


                '***********************************************Vertical Switch ****************************************
                Dim VertSwitch As Boolean = False
                Dim MyLeftTrkLink As Integer = Me.LeftLink
                Dim MyRighTrktLink As Integer = Me.RightLink
                Dim MyLeftTrkIndex As Integer = FindTrackIndexFromGUID(MyLeftTrkLink)
                Dim MyRighTrktIndex As Integer = FindTrackIndexFromGUID(MyRighTrktLink)
                Try
                    ' Need to uncomment this and comment the above IF clause. To prevent NULL reference exception. But need to test before implementing. 
                    If TrackComponent(MyLeftTrkIndex) IsNot Nothing Then
                        If TrackComponent(MyLeftTrkIndex).LX = TrackComponent(MyLeftTrkIndex).RX Then VertSwitch = True
                    ElseIf TrackComponent(MyRighTrktIndex) IsNot Nothing Then
                        If TrackComponent(MyRighTrktIndex).RX = TrackComponent(MyLeftTrkIndex).RX Then VertSwitch = True
                    End If
                Catch ex As Exception
                End Try
                '****************************************************************************************

                Select Case Me.TypeOfSwitch
                    Case "DERAIL", "SPM ONLY"
                        SwitchPath.AddLine(New Point(0, 3), New Point(11, 3))
                        SwitchPath.AddLine(New Point(11, 3), New Point(11, 6))
                        SwitchPath.AddLine(New Point(11, 6), New Point(0, 6))
                        SwitchPath.AddLine(New Point(0, 6), New Point(0, 3))
                        NoPath.AddLine(New Point(0, 1), New Point(11, 1))
                        NoPath.AddLine(New Point(11, 1), New Point(11, 8))
                        NoPath.AddLine(New Point(11, 8), New Point(0, 8))
                        NoPath.AddLine(New Point(0, 8), New Point(0, 1))
                        OutLinePath.AddLine(New Point(0, 0), New Point(11, 0))
                        OutLinePath.AddLine(New Point(11, 0), New Point(11, 9))
                        OutLinePath.AddLine(New Point(11, 9), New Point(0, 9))
                        OutLinePath.AddLine(New Point(0, 9), New Point(0, 0))

                        If TypeOfSwitch = "SPM ONLY" Then
                            NonOperationSwitchPointMonitor = True
                            myTurnOutTrack = New Track
                            myTurnOutTrack.SPMTrack = True
                            myTurnOutTrack.AuthorityType = "OTHER"
                            Select Case Me.ComponentType
                                Case "SPLT"
                                    myTurnOutTrack.LX = Me.LX - 17
                                    myTurnOutTrack.LY = Me.LY - 17
                                    myTurnOutTrack.RX = Me.LX + 3
                                    myTurnOutTrack.RY = Me.LY
                                Case "SPRT"
                                    myTurnOutTrack.LX = Me.LX - 3
                                    myTurnOutTrack.LY = Me.LY
                                    myTurnOutTrack.RX = Me.LX + 18
                                    myTurnOutTrack.RY = Me.LY - 17
                                Case "SPRB"
                                    myTurnOutTrack.LX = Me.LX - 3
                                    myTurnOutTrack.LY = Me.LY + 9
                                    myTurnOutTrack.RX = Me.LX + 18
                                    myTurnOutTrack.RY = Me.LY + 26
                                Case "SPLB"
                                    myTurnOutTrack.LX = Me.LX - 17
                                    myTurnOutTrack.LY = Me.LY + 26
                                    myTurnOutTrack.RX = Me.LX + 3
                                    myTurnOutTrack.RY = Me.LY + 9
                            End Select
                            Me.CurrentDrawingForm.Controls.Add(myTurnOutTrack)
                            myTurnOutTrack.CurrentDrawingForm = Me.CurrentDrawingForm
                            myTurnOutTrack.Graphical = myTurnOutTrack.CurrentDrawingForm.CreateGraphics
                            myTurnOutTrack.InitializeGraphicsFromScreenCoordinates()
                            myTurnOutTrack.RefreshPathTurnOutTrack(Me.ComponentType)
                            myTurnOutTrack.InitGraphics()
                            myTurnOutTrack.GlobalFlasherCollectionReference = FlashCollection
                            myTurnOutTrack.Visible = True
                            If optHideTurnOutTracksOnSPMSwitch Then
                                myTurnOutTrack.Visible = False
                            End If

                            myTurnOutTrack.myControlPoint = Me.myControlPoint
                            Me.ControlledReverse = False
                            Me.Left = LX - 5
                            Me.Top = LY
                            Me.Width = 11
                            Me.Height = 9
                            If ShowSwitchLock Then
                                Select Case Me.ComponentType
                                    Case "SPLT", "SPRT"
                                        mySwitchLock = New SwitchLock(Me.LX - 5, Me.LY, "RU", Me)
                                    Case Else
                                        mySwitchLock = New SwitchLock(Me.LX - 5, Me.LY, "RD", Me)
                                End Select
                                Me.CurrentDrawingForm.Controls.Add(Me.mySwitchLock)
                            End If
                        Else
                            Me.ControlledReverse = True
                            Me.Left = LX - 5
                            Me.Top = LY
                            Me.Width = 11
                            Me.Height = 9
                            If ShowSwitchLock Then
                                mySwitchLock = New SwitchLock(Me.LX - 5, Me.LY, "RU", Me)
                                Me.CurrentDrawingForm.Controls.Add(Me.mySwitchLock)
                            End If
                        End If
                        Path = New GraphicsPath()
                        Path.StartFigure()
                        Path.AddPath(OutLinePath, True)
                        Path.AddPath(SwitchPath, True)
                        Path.AddPath(NoPath, True)
                        Path.CloseFigure()
                        Me.Region = New Region(OutLinePath)
                        GraphicalPath = Path
                        Me.BringToFront()
                        If ShowSwitchLock Then
                            mySwitchLock.Visible = True
                        End If

                    Case Else
                        If VertSwitch = False Then
                            If RightAltLink <> 0 Then
                                TurnOutTrackIndex = FindTrackIndexFromGUID(RightAltLink)
                                If TrackComponent(TurnOutTrackIndex).LY > TrackComponent(TurnOutTrackIndex).RY Then
                                    SwitchPath.AddLine(New Point(0, 0), New Point(10, 0))
                                    SwitchPath.AddLine(New Point(10, 0), New Point(2, 9))
                                    SwitchPath.AddLine(New Point(2, 9), New Point(0, 9))
                                    SwitchPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(8, 0), New Point(10, 0))
                                    NoPath.AddLine(New Point(10, 0), New Point(10, 9))
                                    NoPath.AddLine(New Point(10, 9), New Point(1, 9))
                                    NoPath.AddLine(New Point(1, 9), New Point(8, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(10, 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(0, 9))
                                    OutLinePath.AddLine(New Point(0, 9), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "RU", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "RU", Me)
                                    End If
                                Else
                                    SwitchPath.AddLine(New Point(0, 0), New Point(0 + 2, 0))
                                    SwitchPath.AddLine(New Point(0 + 2, 0), New Point(10, 0 + 9))
                                    SwitchPath.AddLine(New Point(10, 0 + 9), New Point(0, 0 + 9))
                                    SwitchPath.AddLine(New Point(0, 0 + 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(0 + 1, 0), New Point(10, 0))
                                    NoPath.AddLine(New Point(10, 0), New Point(10, 0 + 9))
                                    NoPath.AddLine(New Point(10, 9), New Point(8, 0 + 9))
                                    NoPath.AddLine(New Point(8, 0 + 9), New Point(0 + 1, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(10, 0 + 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "RD", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "RD", Me)
                                    End If
                                End If
                                Me.Left = LX
                                Me.Top = LY
                                Me.Width = 11
                                Me.Height = 9
                                If Me.TypeOfSwitch = "PUZZLE" Then
                                    PuzzleSwitch = True
                                End If
                            End If
                            If LeftAltLink <> 0 Then
                                TurnOutTrackIndex = FindTrackIndexFromGUID(LeftAltLink)
                                If TrackComponent(TurnOutTrackIndex).LY < TrackComponent(TurnOutTrackIndex).RY Then

                                    SwitchPath.AddLine(New Point(0, 0), New Point(10, 0))
                                    SwitchPath.AddLine(New Point(10, 0), New Point(10, 9))
                                    SwitchPath.AddLine(New Point(10, 9), New Point(7, 9))
                                    SwitchPath.AddLine(New Point(7, 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(0, 0), New Point(7, 9))
                                    NoPath.AddLine(New Point(7, 9), New Point(0, 9))
                                    NoPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(10, 0 + 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "LU", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "LU", Me)
                                    End If
                                Else
                                    NoPath.AddLine(New Point(0, 0), New Point(7, 0))
                                    NoPath.AddLine(New Point(7, 0), New Point(0, 9))
                                    NoPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    SwitchPath.AddLine(New Point(7, 0), New Point(10, 0))
                                    SwitchPath.AddLine(New Point(10, 0), New Point(10, 9))
                                    SwitchPath.AddLine(New Point(10, 9), New Point(0, 9))
                                    SwitchPath.AddLine(New Point(0, 9), New Point(7, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(10, 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "LD", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "LD", Me)
                                    End If
                                End If
                                Me.Left = LX - 9
                                Me.Top = LY
                                Me.Width = 11
                                Me.Height = 9
                            End If
                        Else
                            '***********************************************Vertical Switch ****************************************
                            If RightAltLink <> 0 Then
                                TurnOutTrackIndex = FindTrackIndexFromGUID(RightAltLink)
                                If TrackComponent(TurnOutTrackIndex).LY > TrackComponent(TurnOutTrackIndex).RY Then
                                    NoPath.AddLine(New Point(0, 0), New Point(7, 0))
                                    NoPath.AddLine(New Point(7, 0), New Point(0, 9))
                                    NoPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    SwitchPath.AddLine(New Point(7, 0), New Point(10, 0))
                                    SwitchPath.AddLine(New Point(10, 0), New Point(10, 9))
                                    SwitchPath.AddLine(New Point(10, 9), New Point(0, 9))
                                    SwitchPath.AddLine(New Point(0, 9), New Point(7, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(10, 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "VRU", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "VRU", Me)
                                    End If
                                    Me.Left = LX - 5
                                    Me.Top = LY - 3
                                    Me.Width = 9
                                    Me.Height = 20
                                Else
                                    SwitchPath.AddLine(New Point(0, 0), New Point(0 + 2, 0))
                                    SwitchPath.AddLine(New Point(0 + 2, 0), New Point(10, 0 + 9))
                                    SwitchPath.AddLine(New Point(10, 0 + 9), New Point(0, 0 + 9))
                                    SwitchPath.AddLine(New Point(0, 0 + 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(0 + 1, 0), New Point(10, 0))
                                    NoPath.AddLine(New Point(10, 0), New Point(10, 0 + 9))
                                    NoPath.AddLine(New Point(10, 9), New Point(8, 0 + 9))
                                    NoPath.AddLine(New Point(8, 0 + 9), New Point(0 + 1, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(10, 0 + 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "VRD", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "VRD", Me)
                                    End If
                                    Me.Left = LX - 5
                                    Me.Top = LY - 3
                                    Me.Width = 9
                                    Me.Height = 20
                                End If
                                If Me.TypeOfSwitch = "PUZZLE" Then
                                    PuzzleSwitch = True
                                End If
                            End If
                            If LeftAltLink <> 0 Then
                                TurnOutTrackIndex = FindTrackIndexFromGUID(LeftAltLink)
                                If TrackComponent(TurnOutTrackIndex).LY < TrackComponent(TurnOutTrackIndex).RY Then

                                    SwitchPath.AddLine(New Point(0, 0), New Point(10, 0))
                                    SwitchPath.AddLine(New Point(10, 0), New Point(10, 9))
                                    SwitchPath.AddLine(New Point(10, 9), New Point(7, 9))
                                    SwitchPath.AddLine(New Point(7, 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(0, 0), New Point(7, 9))
                                    NoPath.AddLine(New Point(7, 9), New Point(0, 9))
                                    NoPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    OutLinePath.AddLine(New Point(0, 0), New Point(0, 0 + 9))
                                    OutLinePath.AddLine(New Point(0, 0 + 9), New Point(10, 0 + 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "VLU", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "VLU", Me)
                                    End If
                                    Me.Left = LX - 5
                                    Me.Top = LY + 5
                                    Me.Width = 9
                                    Me.Height = 20
                                Else
                                    'Vertical LD
                                    SwitchPath.AddLine(New Point(0, 0), New Point(20, 0))
                                    SwitchPath.AddLine(New Point(15, 0), New Point(2, 9))
                                    SwitchPath.AddLine(New Point(2, 9), New Point(0, 9))
                                    SwitchPath.AddLine(New Point(0, 9), New Point(0, 0))
                                    NoPath.AddLine(New Point(8, 0), New Point(20, 0))
                                    NoPath.AddLine(New Point(25, 0), New Point(15, 9))
                                    NoPath.AddLine(New Point(10, 9), New Point(1, 9))
                                    NoPath.AddLine(New Point(1, 9), New Point(10, 0))

                                    OutLinePath.AddLine(New Point(0, 0), New Point(10, 0))
                                    OutLinePath.AddLine(New Point(10, 0), New Point(10, 9))
                                    OutLinePath.AddLine(New Point(10, 9), New Point(0, 9))
                                    OutLinePath.AddLine(New Point(0, 9), New Point(0, 0))
                                    If ShowSwitchLock Then
                                        mySwitchLock = New SwitchLock(Me.LX, Me.LY, "VLD", Me)
                                    ElseIf optSwitchPointMonitoringForBNSF And SwitchPointerMonitorMode Then
                                        mySwitchPositionIndicator = New SwitchPositionIndicator(Me.LX, Me.LY, "VLD", Me)
                                    End If
                                    Me.Left = LX - 5
                                    Me.Top = LY + 2
                                    Me.Width = 9
                                    Me.Height = 20
                                End If
                            End If

                            '*******************************************************************************************************
                        End If
                        Path = New GraphicsPath()
                        Path.StartFigure()
                        Try
                            If OutLinePath IsNot Nothing Then Path.AddPath(OutLinePath, True) Else SystemEvent("OUTLINEPATH NOT DEFINED FOR SWTICH: " & Me.GUID)
                            If SwitchPath IsNot Nothing Then Path.AddPath(SwitchPath, True) Else SystemEvent("SWITCHPATH NOT DEFINED FOR SWTICH: " & Me.GUID)
                            If NoPath IsNot Nothing Then Path.AddPath(NoPath, True) Else SystemEvent("NOPATH NOT DEFINED FOR SWTICH: " & Me.GUID)
                            Path.CloseFigure()
                        Catch ex As Exception
                            GetErrorDetails(ex, "SWITCH NAME:" & Me.Name & " SWITCH GUID:" & Me.GUID.ToString)
                        End Try


                        Me.Region = New Region(OutLinePath)
                        GraphicalPath = Path
                        'build paths for bitmap imaging
                        Me.SwitchPathRelative = Me.SwitchPath.Clone
                        Me.NoPathRelative = Me.NoPath.Clone
                        Dim mat As New Matrix()
                        mat.Translate(Me.Left, Me.Top)
                        SwitchPathRelative.Transform(mat)
                        NoPathRelative.Transform(mat)

                        'this is a very bad thing. it seems to lock up the process
                        'Me.ParentForm.Controls.Add(Me.mySwitchLock)
                        If ShowSwitchLock And Not Me.mySwitchLock Is Nothing Then
                            Me.CurrentDrawingForm.Controls.Add(Me.mySwitchLock)
                            Me.mySwitchLock.Visible = True
                            Me.mySwitchLock.BringToFront()
                        End If
                        If Not IsNothing(Me.mySwitchPositionIndicator) Then
                            Me.CurrentDrawingForm.Controls.Add(Me.mySwitchPositionIndicator)
                            Me.mySwitchPositionIndicator.Visible = True
                            Me.mySwitchPositionIndicator.BringToFront()
                        End If
                        Dim PerformRouteAssignment As Boolean = True
                        If PerformRouteAssignment And Not Me.NonOperationSwitchPointMonitor Then
                            AssignReverseRoute(Me.FlipOperations)
                            AssignNormalRoute(Me.FlipOperations)
                        Else
                            Me.IndicateOnly = True
                            If Me.SwitchPointerMonitorMode Then
                                Me.RightAltLink = 0
                                Me.LeftAltLink = 0
                            End If
                        End If
                        Me.BringToFront()
                End Select

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub BuildGraphics()
            Try
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            Try
                If FailState And Not Me.SimulateMode And Not Me.SpringSwitch And Not optDisableShowingCodeFailureOnComponents Then
                    CurrentColorStatus = gColorFailedState
                    CurrentColorStatusNoPath = gColorFailedState
                Else
                    CurrentColorStatus = gSystemColorWhite
                    If Me.ControlledReverse Then
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        Else
                            CurrentColorStatusNoPath = CurrentColorStatus
                        End If
                    Else
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = CurrentColorStatus
                        Else
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        End If
                    End If
                End If

                UpdateSwitchRoutesInTracks()
                Me.CheckForNonTranversableRoute()

                'Verison 228
                'Initialize the control bit based on switch position
                If Me.ControlledReverse Then
                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = True
                    End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    If Me.SimulateMode Then
                        If Not Me.Ind(1) Is Nothing Then
                            Me.Ind(1).BitState = True
                        End If
                        If Not Me.Ind(0) Is Nothing Then
                            Me.Ind(0).BitState = False
                        End If
                    End If
                Else
                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = True
                    End If
                    If Me.SimulateMode Then
                        If Not Me.Ind(1) Is Nothing Then
                            Me.Ind(1).BitState = False
                        End If
                        If Not Me.Ind(0) Is Nothing Then
                            Me.Ind(0).BitState = True
                        End If
                    End If

                End If

                Dim SimulateTheSwitchMove As Boolean
                If Me.SpringSwitch Then
                    If ControlledReverse Then
                        If IsNothing(Me.Ind(1)) Then
                            SimulateTheSwitchMove = True
                        End If
                    Else
                        If IsNothing(Me.Ind(0)) Then
                            SimulateTheSwitchMove = True
                        End If

                    End If
                End If


                If (Me.AuthorityType <> "CTC" And Me.AuthorityType <> "OTHER") Or SimulateTheSwitchMove Then
                    If Not Me.RCPS Then
                        If Not Me.SwitchPointerMonitorMode Then
                            FailState = False
                            Me.SimulateSwitchMove()
                        End If
                    End If
                End If
                If optEnableSwitchMuteColor AndAlso Me.TypeOfSwitch = "SPM ONLY" Then
                    CurrentColorStatus = gColorSwitchMonitorMute
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub Draw(ByRef g As Graphics)
            Try
                If CurrentDrawingForm.Initializing Then Exit Sub
                Me.Invalidate()
                Me.Update()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RefreshMe()
            Try
                If CurrentDrawingForm.Initializing Then Exit Sub

                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "TRAINER/TRAINEE MODE"

        Private Sub SimulateSwitchMove()
            Try
                If Me.ControlledReverse Then
                    If Not Me.SwitchPointerMonitorMode Then
                        Me.SwitchIndStatus = SwIndStatus.ReverseInd
                        Me.SwitchPositionStatus = PositionStatus.Reverse

                        If Not IsNothing(Me.Ind(1)) Then
                            Me.Ind(1).BitState = True
                        End If
                        If Not IsNothing(Me.Ind(0)) Then
                            Me.Ind(0).BitState = False
                        End If
                    End If
                Else
                    If Not Me.SwitchPointerMonitorMode Then
                        Me.SwitchIndStatus = SwIndStatus.NormalInd
                        Me.SwitchPositionStatus = PositionStatus.Normal
                        If Not IsNothing(Me.Ind(1)) Then
                            Me.Ind(1).BitState = False
                        End If
                        If Not IsNothing(Me.Ind(0)) Then
                            Me.Ind(0).BitState = True
                        End If
                    End If
                End If
                'Me.ApplyIndicationChange()
                SetSwitchToInCorespondence()
                'Me.UpdateStatus()

                Me.IndSimulator.Enabled = False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub SimulateElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                Me.myControlPoint.FlashName()
                If Me.ControlledReverse Then
                    If Not Me.Ind(1) Is Nothing Then
                        Me.Ind(1).BitState = True
                    End If
                    If Not Me.Ind(0) Is Nothing Then
                        Me.Ind(0).BitState = False
                    End If

                Else
                    If Not Me.Ind(1) Is Nothing Then
                        Me.Ind(1).BitState = False
                    End If
                    If Not Me.Ind(0) Is Nothing Then
                        Me.Ind(0).BitState = True
                    End If

                End If
                Me.ApplyIndicationChange()
                Me.UpdateStatus()
                Me.IndSimulator.Enabled = False
                If gTraineeMode Then
                    'Me.SendTraineeStatusUpdate()
                    Me.myControlPoint.SendSimulatedIndication()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub SendTrainerCommand(ByVal IndCommand As String)
            Try
                Dim Message As String = "TRC" & Chr(6) & "TRAINERCOMMAND;"
                Message = Message & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                Message = Message & "|SW"
                Message = Message & "|" & IndCommand
                Message = Me.TerritoryID.ToString & Chr(12) & Message
                TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overrides Sub SendTraineeStatusUpdate()
            Try
                Dim Message As String = "DC" & Chr(6) & "STATUS;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                Message = Message & "|SW"
                Message = Message & "|IND=" & Me.SwitchIndStatus
                Message = Message & "|CTL=" & Me.ControlledReverse
                Message = Message & "|LK=" & Me.SwitchLockBit
                Message = Message & "|TF=" & Me.TrafficDirection
                Message = Message & "|OC=" & Me.Occuppied
                Message = Message & "|TT=" & Me.TrackAndTime
                Message = Message & "|AB=" & Me.BlockAbsolute
                Message = Message & "|DB=" & Me.DeviceBlocked
                Message = Message & "|SS=" & Me.SignalStatOnMe.ToString
                Message = Me.TerritoryID.ToString & Chr(12) & Message
                TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "SWITCH METHODS"

#Region "SWITCH POINT MONITORING"

        Public Sub PerformSwitchOverride(ByVal AuthUID As String)
            Try
                SwitchEventLog("APPLY OVERRIDE POSITION REPORTING FROM SUMMARY")

                Dim so As New SwitchPointOverride
                so.OverrideTime = Now
                so.AuthUID = AuthUID

                Me.SwitchOverrideList.Add(so)
                If Me.SwitchOverrideList.Contains(so) Then
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS BEEN ADDED!")
                    Me.AcknowledgeSwitchOpenAlertFromOverride(so.AuthUID)
                    Me.UpdateSwitchPointStatus()
                Else
                    SwitchEventLog("SWITCH OVERRIDE FOR AUTHORITY:" & so.AuthUID & " HAS NOT BEEN ADDED!")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckToRemoveAlertFromAuthorityRelease()
            Dim al As Alert
            'Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo & "|" & auth.AuthorityUID & "|" & LastStation
            Dim RemoveList As New ArrayList
            Try
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.SwitchPointOpen
                                Dim data() As String = al.AlertData.Split("|")
                                If data.Length >= 7 Then
                                    If data(0) = Me.Name Then
                                        If Not Me.DoesAuthorityExistOnSwitch(data(5)) Then
                                            al.AlertAcknowledged = True
                                            Dim Message As String = "SWITCH OPEN ALERT WAS ACKNOWLEDGED BY SYSTEM. SWITCH REPORTED NORMAL POSITION:" & al.AlertGUID
                                            al.AlertAckedByPerson = Message
                                            SwitchEventLog(Message)
                                            gSystemAlertExists = False
                                            Dim SummItem As New SummaryItem("ALERT", "ACKNOWLEDGE", al)
                                            SyncLock TMDS.GlobalDeclarations.gSummaryAlertQueue.SyncRoot
                                                TMDS.GlobalDeclarations.gSummaryAlertQueue.Enqueue(SummItem)
                                            End SyncLock
                                            RemoveList.Add(al)

                                        End If
                                    End If
                                End If
                        End Select
                    Next
                    For Each al In RemoveList
                        TMDS.GlobalDeclarations.gSystemAlertList.Remove(al)
                    Next
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function DoesAuthorityExistOnSwitch(ByVal AuthUID As String) As Boolean
            Try
                Dim auth As SFAuthority
                For Each auth In Me.TrackAuthorities
                    If auth.AuthorityUID = AuthUID Then
                        Return True
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub CheckForExistingAuthorities()
            Try
                If Not Me.SwitchPointInitialized Then Exit Sub
                If Me.Ind(0).BitState Then
                    If Not Me.FailState Then
                        If Not IsNothing(Me.Ind(1)) Then
                            If Not Me.Ind(1).BitState Then
                                Exit Sub
                            End If
                        End If
                    End If
                End If

                Dim auth As SFAuthority
                Dim TrainAuthorityFound As Boolean
                Dim OverrideCheck As Boolean = False
                For Each auth In Me.TrackAuthorities
                    Dim so As SwitchPointOverride
                    For Each so In Me.SwitchOverrideList
                        If auth.AuthorityUID = so.AuthUID Then
                            OverrideCheck = True
                            Exit For
                        End If
                    Next

                    If Not OverrideCheck Then
                        Dim DoAlarm As Boolean = True
                        If auth.AuthType1 = "ItemFourToMOW" And Not TrainAuthorityFound Then
                            DoAlarm = False
                        Else
                            TrainAuthorityFound = True
                        End If
                        Dim ProtectedSwitch As Boolean
                        If auth.Box12Switches.Contains(Me.Name) Then
                            DoAlarm = False
                            ProtectedSwitch = True
                        End If
                        If CheckForExclusiveBeginEndAuthorityPoints(auth) Then
                            DoAlarm = False
                            If Me.CheckForNoSubsequentAuthorities Then
                                DoAlarm = False
                            Else
                                If Not ProtectedSwitch And Not Me.Ind(0).BitState Then
                                    DoAlarm = True
                                End If

                                'check to see if auth has this switch in box 13 and if there is an exclusive authority on the siding
                                If CheckForExclusiveAuthorityOnSiding(auth) Then
                                    DoAlarm = False
                                End If
                            End If
                        ElseIf TrainAuthorityFound And Not ProtectedSwitch Then
                            DoAlarm = True
                        End If
                        ProtectedSwitch = True
                        If DoAlarm Then
                            Dim LastStation As String = FindLastStation(auth)
                            'P1-5003: Show OpenSwitch Alarm only for Train not for MOW.
                            Select Case auth.AuthType1.Trim.ToUpper
                                Case "ITEMTWOTOTRAIN", "ITEMFOURTOTRAIN"
                                    Me.ShowAlarmForOpenSwitch(Me, auth, LastStation)
                            End Select
                            'Me.ShowAlarmForOpenSwitch(Me, auth, LastStation)
                        Else
                            CheckToAcknowledgeAlerts()
                        End If
                    Else
                        OverrideCheck = False
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckForExclusiveAuthorityOnSiding(ByVal thisAuthority As SFAuthority) As Boolean

            Dim SidingTrack As Track = GetFirstSidingTrack()
            Try
                Dim auth As Object
                For Each auth In TMDS.GlobalDeclarations.OtherTrackAuthorities
                    If auth.GetType.ToString = "TMDS.TrackAuthority.SFAuthority" Then
                        Dim sf As TrackAuthority.SFAuthority = CType(auth, TrackAuthority.SFAuthority)
                        If sf.AuthorityUID <> thisAuthority.AuthorityUID And sf.IssuedToTrain And Not sf.Components.Contains(SidingTrack) Then
                            If Mid(thisAuthority.IssueTo, 1, 10) = Mid(sf.IssueTo, 1, 10) Then
                                Return False
                            End If
                        End If
                    End If
                Next

                If IsNothing(SidingTrack) Then Return False
                Dim sfa As TrackAuthority.SFAuthority
                For Each sfa In SidingTrack.TrackAuthorities
                    If sfa.IssuedToTrain Then
                        If Mid(thisAuthority.IssueTo, 1, 10) = Mid(sfa.IssueTo, 1, 10) Then
                            Return True
                        End If
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Private Function GetFirstSidingTrack() As Track
            Dim LoopControl As Integer
            Dim NextComp As Object
            Try

                If Me.LeftAltLink <> 0 Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Me.LeftAltLink)
                    Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)
                            If optEnableTrackSidingBit Then
                                'NEW SIDING LOGIC CODE
                                If trk.EnableControlledSiding Or trk.EnableNonControlledSiding Then
                                    Return trk
                                End If
                            Else
                                'OLD LEGACY SIDING CODE
                                If trk.TrackName = "SIDING" Then
                                    Return trk
                                End If
                            End If
                        End If
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        LoopControl += 1
                        If LoopControl > 10 Then Exit Do
                    Loop
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Me.RightAltLink)
                    Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)
                            If optEnableTrackSidingBit Then
                                'NEW SIDING LOGIC CODE
                                If trk.EnableControlledSiding Or trk.EnableNonControlledSiding Then
                                    Return trk
                                End If
                            Else
                                'OLD LEGACY SIDING CODE
                                If trk.TrackName = "SIDING" Then
                                    Return trk
                                End If
                            End If
                        End If
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        LoopControl += 1
                        If LoopControl > 10 Then Exit Do
                    Loop
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(" CheckSPMSSameTrain : " & ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function CheckForExclusiveBeginEndAuthorityPoints(ByVal authToChk As SFAuthority) As Boolean
            Try
                If Me.TrackAuthorities.Count > 0 Then
                    Dim auth As SFAuthority
                    For Each auth In Me.TrackAuthorities
                        If authToChk Is auth Then
                            Dim AuthOK As Boolean = False
                            If auth.Box2From1 = Me.Name Or auth.Box2From2 = Me.Name _
                            Or auth.Box2To1 = Me.Name Or auth.Box2To2 = Me.Name Or
                            auth.Box4From1 = Me.Name Or auth.Box4From2 = Me.Name Or
                            auth.Box4To1 = Me.Name Or auth.Box4From2 = Me.Name Then
                                AuthOK = True
                            End If
                            If Not AuthOK Then
                                Return False
                            End If
                        End If



                    Next
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Private Function CheckForNoSubsequentAuthorities() As Boolean
            Try
                Dim IssueToList As New ArrayList
                Dim TrackList As New ArrayList
                If Me.TrackAuthorities.Count > 0 Then
                    Dim auth As SFAuthority
                    For Each auth In Me.TrackAuthorities
                        If Not IssueToList.Contains(auth.IssueTo) Then
                            IssueToList.Add(auth.IssueTo)
                        End If
                    Next
                End If

                Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)

                If Not IsNothing(LeftTrack) Then
                    If LeftTrack.OStype Then
                        If Not IsNothing(LeftTrack.OsComponentCollection) Then
                            Dim comp As Object
                            For Each comp In LeftTrack.OsComponentCollection
                                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    Dim LTK As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(comp.LeftLink)
                                    Dim RTK As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(comp.RightLink)
                                    If Not IsNothing(LTK) Then
                                        If Not LeftTrack.OsComponentCollection.Contains(LTK) Then
                                            If Not TrackList.Contains(LTK) Then
                                                TrackList.Add(LTK)
                                            End If
                                        End If
                                    End If
                                    If Not IsNothing(RTK) Then
                                        If Not LeftTrack.OsComponentCollection.Contains(RTK) Then
                                            If Not TrackList.Contains(RTK) Then
                                                TrackList.Add(RTK)
                                            End If
                                        End If
                                    End If
                                End If
                            Next

                        End If
                    ElseIf LeftTrack.TrackAuthorities.Count > 0 Then
                        If Not TrackList.Contains(LeftTrack) Then
                            TrackList.Add(LeftTrack)
                        End If
                    End If
                End If

                If Not IsNothing(RightTrack) Then
                    If RightTrack.OStype Then
                        If Not IsNothing(RightTrack.OsComponentCollection) Then
                            Dim comp As Object
                            For Each comp In RightTrack.OsComponentCollection
                                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    Dim LTK As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(comp.LeftLink)
                                    Dim RTK As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(comp.RightLink)
                                    If Not IsNothing(LTK) Then
                                        If Not RightTrack.OsComponentCollection.Contains(LTK) Then
                                            If Not TrackList.Contains(LTK) Then
                                                TrackList.Add(LTK)
                                            End If
                                        End If
                                    End If
                                    If Not IsNothing(RTK) Then
                                        If Not RightTrack.OsComponentCollection.Contains(RTK) Then
                                            If Not TrackList.Contains(RTK) Then
                                                TrackList.Add(RTK)
                                            End If
                                        End If
                                    End If
                                End If
                            Next

                        End If
                    ElseIf RightTrack.TrackAuthorities.Count > 0 Then
                        If Not TrackList.Contains(RightTrack) Then
                            TrackList.Add(RightTrack)
                        End If
                    End If
                End If


                Dim AuthList As New ArrayList
                Dim trk As Track
                For Each trk In TrackList
                    Dim auth As SFAuthority
                    For Each auth In trk.TrackAuthorities
                        If Not Me.TrackAuthorities.Contains(auth) Then
                            If auth.AuthType1 = "ItemFourToTrain" Then
                                AuthList.Add(auth.IssueTo)
                            Else
                                'If auth.Direction = "LEFT" Then

                                If auth.Box2Track1 <> "SIDING" And auth.Box2Track1 <> "" Then
                                    If Not AuthList.Contains(auth.IssueTo) Then
                                        AuthList.Add(auth.IssueTo)
                                    End If
                                End If
                                If auth.Box4Track1 <> "SIDING" And auth.Box4Track1 <> "" Then
                                    If Not AuthList.Contains(auth.IssueTo) Then
                                        AuthList.Add(auth.IssueTo)
                                    End If
                                End If

                                'End If
                            End If
                        End If
                    Next
                Next

                Dim AuthTo As String
                For Each AuthTo In AuthList
                    Dim IssueTo As String
                    For Each IssueTo In IssueToList
                        If InStr(AuthTo, IssueTo) > 0 Then
                            Return False
                        End If
                        If InStr(IssueTo, AuthTo) > 0 Then
                            Return False
                        End If
                    Next
                Next


                Return True

            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Private Function FindLastStation(ByVal auth As SFAuthority) As String
            Try
                Dim LastStation As String = String.Empty
                Dim TrainDirection As String = auth.Direction
                Dim trn As TrainID
                Dim trnFound As TrainID = Nothing
                For Each trn In TMDS.GlobalDeclarations.TrainCollection
                    If trn.IdentifingUnit = Mid(auth.IssueTo, 1, trn.IdentifingUnit.Length) Then
                        trnFound = trn
                        Exit For
                    End If
                Next
                If Not IsNothing(trnFound) Then
                    If trnFound.LastOS <> "" Then
                        Dim data() As String = trnFound.LastOS.Split("(")
                        If data.Length = 2 Then
                            LastStation = data(1).Replace(")", "")
                        End If
                    Else
                        If Not IsNothing(trnFound.myTrack) Then
                            Select Case TrainDirection
                                Case "LEFT"
                                    LastStation = trnFound.myTrack.RightLimitName
                                Case Else
                                    LastStation = trnFound.myTrack.LeftLimitName
                            End Select
                        End If
                    End If
                End If

                Return LastStation
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Sub CheckToAcknowledgeAlerts()
            Dim al As Alert
            'Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo & "|" & auth.AuthorityUID & "|" & LastStation
            Dim RemoveList As New ArrayList
            Try
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.SwitchPointOpen
                                Dim data() As String = al.AlertData.Split("|")
                                If data.Length >= 7 Then
                                    If data(0) = Me.Name Then
                                        al.AlertAcknowledged = True
                                        Dim Message As String = "SWITCH OPEN ALERT WAS ACKNOWLEDGED BY SYSTEM. SWITCH REPORTED NORMAL POSITION:" & al.AlertGUID
                                        al.AlertAckedByPerson = Message
                                        SwitchEventLog(Message)
                                        gSystemAlertExists = False
                                        Dim SummItem As New SummaryItem("ALERT", "ACKNOWLEDGE", al)
                                        SyncLock TMDS.GlobalDeclarations.gSummaryAlertQueue.SyncRoot
                                            TMDS.GlobalDeclarations.gSummaryAlertQueue.Enqueue(SummItem)
                                        End SyncLock
                                        RemoveList.Add(al)
                                    End If
                                End If
                        End Select
                    Next
                    For Each al In RemoveList
                        TMDS.GlobalDeclarations.gSystemAlertList.Remove(al)
                    Next
                End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub AcknowledgeSwitchOpenAlertFromOverride(ByVal AuthUID As String)
            Dim al As Alert
            'Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo & "|" & auth.AuthorityUID & "|" & LastStation
            Dim RemoveList As New ArrayList
            Try
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.SwitchPointOpen
                                Dim data() As String = al.AlertData.Split("|")
                                If data.Length >= 7 Then
                                    If data(0) = Me.Name And data(5) = AuthUID Then
                                        al.AlertAcknowledged = True
                                        Dim Message As String = "SWITCH OPEN ALERT WAS ACKNOWLEDGED BY SYSTEM. SWITCH REPORTED NORMAL POSITION:" & al.AlertGUID
                                        al.AlertAckedByPerson = Message
                                        SwitchEventLog(Message)
                                        gSystemAlertExists = False
                                        Dim SummItem As New SummaryItem("ALERT", "ACKNOWLEDGE", al)
                                        SyncLock TMDS.GlobalDeclarations.gSummaryAlertQueue.SyncRoot
                                            TMDS.GlobalDeclarations.gSummaryAlertQueue.Enqueue(SummItem)
                                        End SyncLock
                                        RemoveList.Add(al)

                                    End If
                                End If
                        End Select
                    Next
                    For Each al In RemoveList
                        TMDS.GlobalDeclarations.gSystemAlertList.Remove(al)
                    Next
                End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DoVerifySwitchNormalPosition()
            Try
                Dim LaunchPoint As Point = New Point(Me.CurrentDrawingForm.Left + 200, Me.CurrentDrawingForm.Top + 200)
                Dim frmSwitchDialog As New frmSwitchDialog
                frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "", Me)
                frmSwitchDialog.ShowDialog(Me.CurrentDrawingForm)
                If frmSwitchDialog.UserResponse Then
                    Me.SwitchIndStatus = SwIndStatus.NormalInd
                    Me.FailState = False
                    Me.ManualSwitchPointVerification = True
                    Me.Occuppied = False
                    Me.UpdateStatus()
                    UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                    UpdateSwitchPointStatus()
                    CheckToAcknowledgeAlerts()
                    If Not IsNothing(Me.mySwitchPositionIndicator) Then
                        Me.mySwitchPositionIndicator.SetPosition()
                    End If
                End If
                frmSwitchDialog.Dispose()
                frmSwitchDialog = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SetSwitchVerification()
            Try
                Me.FailState = False
                Me.ManualSwitchPointVerification = True
                Me.Occuppied = False
                Me.UpdateStatus()
                UpdateSwitchPointStatus()
                CheckToAcknowledgeAlerts()
                If Not IsNothing(Me.mySwitchPositionIndicator) Then
                    Me.mySwitchPositionIndicator.SetPosition()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DoRemoveSwitchVerification()
            Try
                Me.SwitchIndStatus = SwIndStatus.NoIndication
                'Me.FailState = True
                Me.ManualSwitchPointVerification = False
                'Me.Occuppied = True
                'Me.UpdateStatus()
                UpdateSwitchPointStatus()
                If Not IsNothing(Me.mySwitchPositionIndicator) Then
                    Me.mySwitchPositionIndicator.SetPosition()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CalculateFailureTime()
            Try
                Me.AlarmTime = LastReportTime.Subtract(Now.AddSeconds(-1 * Me.PositionReportTimeOut))
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub UpdateSwitchPointStatus()
            Try
                If StatusRecordExists Then
                    Dim Update As String = "UPDATE tblSwitchPointStatuses"
                    Dim Status As String = "UNKNOWN"
                    If Me.ManualSwitchPointVerification Then
                        Status = "VERIFIED NORMAL"
                    ElseIf Me.SwitchIndStatus = SwIndStatus.NormalInd Then
                        Status = "NORMAL"
                    End If
                    If Me.FailState And Me.LastReportTime < Now.AddSeconds(-1 * Me.PositionReportTimeOut) Then
                        Status = "UNKNOWN"
                    End If

                    Update = Update & " SET Status = '" & Status & "',"
                    Update = Update & " LastUpdate = '" & Me.LastReportTime.ToString & "',"

                    Dim OverrideString As String = String.Empty
                    Dim so As SwitchPointOverride
                    Dim i As Integer
                    For Each so In Me.SwitchOverrideList
                        If i = 0 Then
                            OverrideString = so.AuthUID & "|" & so.OverrideTime.ToString
                            i = 1
                        Else
                            OverrideString = OverrideString & ";" & so.AuthUID & "|" & so.OverrideTime.ToString
                        End If
                    Next
                    Update = Update & " AuthorityOverride = '" & OverrideString & "'"
                    Update = Update & " WHERE SwitchUID = " & Me.GUID
                    SystemDatabaseCommandToDynamic("SWITCH:UpdateSwitchPointStatus", Update)
                    SwitchEventLog("SPMS EVENT-STATUS UPDATED INTO DATABASE @:" & Format(Me.LastReportTime.ToString, "MM-dd-yy HH:mm:ss") & " FOR " & Me.Name)

                Else
                    Dim Status As String = "UNKNOWN"
                    If Me.ManualSwitchPointVerification Then
                        Status = "VERIFIED NORMAL"
                    ElseIf Me.SwitchIndStatus = SwIndStatus.NormalInd And Not Me.FailState Then
                        Status = "NORMAL"
                    End If
                    Dim OverrideString As String = String.Empty
                    Dim so As SwitchPointOverride
                    Dim i As Integer
                    For Each so In Me.SwitchOverrideList
                        If i = 0 Then
                            OverrideString = so.AuthUID & "|" & so.OverrideTime.ToString
                            i = 1
                        Else
                            OverrideString = OverrideString & ";" & so.AuthUID & "|" & so.OverrideTime.ToString
                        End If
                    Next
                    Dim Insert As String = "INSERT INTO tblSwitchPointStatuses"
                    Insert = Insert & " (SwitchUID,Status,LastUpdate,AuthorityOverride,TerrUID)"
                    Insert = Insert & "VALUES (" & Me.GUID & ", '" & Status & "', '" & Me.LastReportTime.ToString & "', '" & OverrideString & "', " & Me.TerritoryID & ")"
                    SystemDatabaseCommandToDynamic("SWITCH:UpdateSwitchPointStatus", Insert)
                    Me.StatusRecordExists = True
                    SwitchEventLog("SPMS EVENT-STATUS INSERTED INTO DATABASE @:" & Format(Me.LastReportTime.ToString, "MM-dd-yy HH:mm:ss") & " FOR " & Me.Name)
                End If
                PointStatusRecordTime = Now

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitSwitchPoint()
            Try
                If Me.NoIndicationAssigned Then
                    Me.CurrentSwitchPointStatus = "NORMAL"
                End If
                Select Case Me.CurrentSwitchPointStatus
                    Case "VERIFIED NORMAL"
                        Me.ManualSwitchPointVerification = True
                        Me.FailState = False
                    Case "NORMAL"
                        Me.SwitchIndStatus = SwIndStatus.NormalInd
                    Case "UNKNOWN"
                        Me.SwitchIndStatus = SwIndStatus.NoIndication
                    Case Else
                        Me.SwitchIndStatus = SwIndStatus.NoIndication
                End Select

                If Not Me.ManualSwitchPointVerification Then
                    If LastReportTime < Now.AddSeconds(-1 * PositionReportTimeOut) Then
                        CheckForExistingAuthorities()
                    End If
                End If
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckSwitchPointStatus()
            Try

                If Not Me.SwitchPointInitialized Then
                    If Me.LastReportTime > Now.AddSeconds(-1 * Me.PositionReportTimeOut) Then
                        Me.CalculateFailureTime()
                        Exit Sub
                    Else
                        SwitchPointInitialized = True
                        'CheckForExistingAuthorities()
                        Me.FailState = True
                        Me.UpdateStatus()
                        If Not IsNothing(Me.mySwitchPositionIndicator) Then
                            Me.mySwitchPositionIndicator.SetPosition()
                        End If
                        Me.CalculateFailureTime()
                    End If
                End If

                Dim so As SwitchPointOverride
                Dim UpdateRequired As Boolean
                If Me.SwitchOverrideList.Count > 0 Then
                    Dim DeleteList As New ArrayList

                    For Each so In Me.SwitchOverrideList
                        If so.OverrideTime < Now.AddMinutes(-45) Then
                            DeleteList.Add(so)
                            UpdateRequired = True
                        End If
                    Next
                    For Each so In DeleteList
                        Me.SwitchOverrideList.Remove(so)
                    Next
                End If
                If LastReportTime < Now.AddMinutes(-5) Then
                    UpdateRequired = True
                End If

                If UpdateRequired Then
                    Me.UpdateSwitchPointStatus()
                End If

                If Not Me.ManualSwitchPointVerification Then
                    If LastReportTime < Now.AddSeconds(-1 * PositionReportTimeOut) Then
                        CheckForExistingAuthorities()
                        Me.FailState = True
                        Me.UpdateStatus()
                        If Not IsNothing(Me.mySwitchPositionIndicator) Then
                            Me.mySwitchPositionIndicator.SetPosition()
                        End If
                    End If
                Else
                    Me.FailState = False
                    Me.UpdateStatus()
                    If Not IsNothing(Me.mySwitchPositionIndicator) Then
                        Me.mySwitchPositionIndicator.SetPosition()
                    End If
                End If
                Me.CalculateFailureTime()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


#End Region

        ''' <summary>
        ''' Check if swith has an occupancy
        ''' </summary>
        ''' <returns>If occupancy over me, return <c>True</c>, if not, return <c>False</c></returns>
        Public Function HasOccupancy() As Boolean
            Try
                If Occuppied OrElse
                    (OsComponentCollection IsNot Nothing AndAlso
                    OsComponentCollection.OfType(Of Track).Where(Function(x) x.Occuppied).Any()) OrElse
                    (BrotherSwitches IsNot Nothing AndAlso
                    BrotherSwitches.OfType(Of Switch).Where(Function(x) x.Occuppied).Any()) Then Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        ''' <summary>
        ''' Check if swith has an authority
        ''' </summary>
        ''' <returns>If authority over me, return <c>True</c>, if not, return <c>False</c></returns>
        Public Function HasAuthority() As Boolean
            Try
                If TrackAuthorities.Count > 0 OrElse
                    (BrotherSwitches IsNot Nothing AndAlso
                    BrotherSwitches.OfType(Of Switch).Where(Function(x) x.TrackAuthorities.Count > 0).Any()) Then
                    Return True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        ''' <summary>
        ''' Check if swith has a stored route
        ''' </summary>
        ''' <returns>If stored routes over me, return <c>True</c>, if not, return <c>False</c></returns>
        Public Function HasStoredRoute() As Boolean
            Try
                If Not OSLocking Then Return False
                For Each storedRoute As StoredRoute In myControlPoint.StoredRoutes
                    If storedRoute.ComponentSelection.Contains(Me) OrElse
                        (BrotherSwitches IsNot Nothing AndAlso
                        BrotherSwitches.OfType(Of Switch).Where(Function(x) storedRoute.ComponentSelection.Contains(x)).Any()) Then
                        Return True
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        Public Function DoesSwitchHaveLicense(ByVal theTrain As TrainID) As Boolean
            Try

                If Me.TrackAuthorities.Count > 0 Then
                    Dim auth As TrackAuthority.TrackAuthority = Me.TrackAuthorities.Item(0)
                    If auth.IssueTo <> theTrain.IdentifingUnit Then
                        Return True
                    End If
                Else
                    Return False
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Sub CheckToRemoveRCPSControls()
            Try
                If Me.TrackAuthorities.Count = 0 Then
                    If Not IsNothing(Me.LockControlBiDirectionNoRestriction) Then
                        Me.LockControlBiDirectionNoRestriction.BitState = False
                    End If
                    If Not IsNothing(Me.LockControlBiDirectionRestricting) Then
                        Me.LockControlBiDirectionRestricting.BitState = False
                    End If
                    If Not IsNothing(Me.LockControlLeftDirection) Then
                        Me.LockControlLeftDirection.BitState = False
                    End If
                    If Not IsNothing(Me.LockControlRightDirection) Then
                        Me.LockControlRightDirection.BitState = False
                    End If
                    Me.myControlPoint.SendCodeStationControlSpecial(True, True)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckForReversePermission() As Boolean
            Dim PermissionToMoveSwitch As Boolean = True
            Try
                If Mid(Me.AuthorityType, 1, 3) = "TWC" Then
                    If gSFAMode Then
                        Dim sfa As SFAuthority
                        For Each sfa In Me.TrackAuthorities
                            If Not optProtectOpenSwitchForCP Then
                                If sfa.CheckBoxItemsStatus(12) Then
                                    Dim Box13 As String
                                    For Each Box13 In sfa.Box13Switches
                                        If Box13 = Me.Name Then
                                            PermissionToMoveSwitch = False
                                            Exit For
                                        End If
                                    Next
                                End If
                            Else
                                Dim Box13 As String
                                If Not sfa.Box13Switches Is Nothing AndAlso sfa.Box13Switches.Count > 0 Then
                                    For Each Box13 In sfa.Box13Switches
                                        If Box13 = Me.Name Then
                                            PermissionToMoveSwitch = False
                                            Exit For
                                        End If
                                    Next
                                End If
                            End If
                            If Not PermissionToMoveSwitch Then
                                Exit For
                            End If
                        Next
                    Else
                        Dim warr As Warrant
                        For Each warr In Me.Authorities
                            If warr.CheckBox(21) Then
                                PermissionToMoveSwitch = False
                            End If
                        Next
                    End If
                End If
                If Not PermissionToMoveSwitch Then
                    Dim frmMessageBox As frmMessageBox
                    Dim MessageList As New ArrayList()
                    Dim TopPoint As Int32
                    If Me.Top + 300 < Me.CurrentDrawingForm.Height Then
                        TopPoint = Me.Top + 100
                    Else
                        TopPoint = 150
                    End If
                    MessageList.Add(Me.Name & " SWITCH HAS CURRENT PERMISSION TO LEAVE SWITCH REVERSE.")
                    MessageList.Add("CANNOT REPORT IT IN A NORMAL POSITION AT THIS TIME!")
                    MessageList.Add("")
                    frmMessageBox = New frmMessageBox()
                    frmMessageBox.SetMessage(MessageList, "SWITCH POSITION INFORMATION", False, Me.CurrentDrawingForm.myLaunchPoint)
                    frmMessageBox.ShowDialog()
                    frmMessageBox.Dispose()
                    frmMessageBox = Nothing
                    Return False
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Function

        Public Function TrackAuthorityDuplicateCheck(ByVal authToCheck As TrackAuthority.TrackAuthority) As Boolean
            Try
                Dim auth As TrackAuthority.TrackAuthority
                For Each auth In Me.TrackAuthorities
                    If Trim(auth.AuthGUID) = Trim(authToCheck.AuthGUID) Then
                        Return False
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return True
            End Try
        End Function

        Public Sub TrackAuthorityRemoveFromMe(ByVal AuthToRemove As TrackAuthority.TrackAuthority)
            Try
                Dim auth As TrackAuthority.TrackAuthority
                Dim DeleteList As New ArrayList()
                Dim CloneList As ArrayList = Me.TrackAuthorities.Clone
                For Each auth In CloneList
                    If Trim(auth.AuthGUID) = Trim(AuthToRemove.AuthGUID) Then
                        DeleteList.Add(auth)
                    End If
                Next
                For Each auth In DeleteList
                    Me.TrackAuthorities.Remove(auth)
                Next
                If Me.TrackAuthorities.Count = 0 Then
                    Me.TrackAuthority = False
                End If

                DeleteList.Clear()
                DeleteList = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ReControlElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                If Not Me.myControlPoint.EnableResendControlLogic Then Exit Sub

                If Me.BrotherSwitches.Count > 0 Then
                    Dim sw As Switch = Me.BrotherSwitches.Item(0)
                    If sw.GUID > Me.GUID Then
                        Exit Sub
                    End If
                End If

                If Me.ReControlCount >= 3 Then
                    ReControlCount = 0
                    Me.tmrReControl.Enabled = False
                    Me.ReControlStart = False
                    SwitchEventLog("SWITCH CONTROL FAILED:" & Me.Name & "-" & Me.GUID.ToString)
                    Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                    LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.FAIL, Nothing, Nothing)

                Else
                    Me.tmrReControl.Enabled = True
                    Me.tmrReControl.Interval = Me.myControlPoint.ResendControlTime 'optRetryTimeOutValue

                    'Me.tmrCheckRoute.Enabled = True
                    ReControlCount = ReControlCount + 1
                    Me.FindCodeStationToSend()
                    Me.myControlPoint.SendCodeStationControls(optEnableResendControlSpecial)
                    SwitchEventLog("SWITCH CONTROL RESEND:" & Me.Name & "-" & Me.GUID.ToString)

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Me.ReControlCount = 0
                Me.ReControlStart = False
            End Try
        End Sub

        Public Sub CheckForBrotherSwitches()
            Try
                If IsNothing(Me.myControlPoint) Then Exit Sub
                If IsNothing(Me.myControlPoint.AssignedComponenets) Then Exit Sub
                Dim swComp As Object
                For Each swComp In Me.myControlPoint.AssignedComponenets
                    If swComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        If swComp.StationNumber = Me.StationNumber And swComp.Name = Me.Name Then
                            If Me.BrotherSwitches Is Nothing Then Me.BrotherSwitches = New ArrayList()
                            If Not Me.BrotherSwitches.Contains(swComp) Then
                                If Not Me Is swComp Then
                                    Me.BrotherSwitches.Add(swComp)
                                End If
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SetFieldBlockingControl(ByRef SetControl As Boolean)
            Dim SendControl As Boolean
            Try
                If Not IsNothing(Me.SwitchBlockControlOn) Then
                    If SetControl <> Me.SwitchBlockControlOn.BitState Then
                        SendControl = True
                    End If
                    Me.SwitchBlockControlOn.BitState = SetControl
                End If

                If Not IsNothing(Me.SwitchBlockControlOff) Then
                    If SetControl Then
                        Me.SwitchBlockControlOff.BitState = False
                    Else
                        Me.SwitchBlockControlOff.BitState = True
                    End If
                End If
                If SendControl Then
                    Me.myControlPoint.SendCodeStationControlUpdate()
                    Me.myControlPoint.SendCodeStationControlSpecial(True, False)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ApplyIndicationChangeForRCPS()
            Try
                If Me.LockIndBiDirectionNoRestriction.BitState Or
                Me.LockIndBiDirectionRestricting.BitState Or
                Me.LockIndLeftDirection.BitState Or
                Me.LockIndRightDirection.BitState Then
                    Me.SwitchLockBit = True
                    DirectionalRCPSLockOn = True
                ElseIf Me.TimeLockIndication.BitState Then
                    Me.SwitchLockBit = True
                    DirectionalRCPSLockOn = False
                Else
                    If Not Me.SwitchLockBit Then
                        Me.SwitchLockBit = False
                        DirectionalRCPSLockOn = False
                    End If
                End If
                If Not IsNothing(Me.TimeLockIndication) Then
                    If Me.TimeLockIndication.BitState Then
                        TimeLockingOn = True
                    Else
                        TimeLockingOn = False
                    End If
                End If

            Catch ex As Exception
                'Me.SwitchLockBit = True
                Me.ForceProtectionOnRCPS = True
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckForPositiveLockIndication()
            Try

                Dim SendControl As Boolean
                If Me.LockControlBiDirectionNoRestriction.BitState Then
                    If Not Me.LockIndBiDirectionNoRestriction.BitState Then
                        SendControl = True
                    End If
                End If

                If Me.LockControlBiDirectionRestricting.BitState Then
                    If Not Me.LockIndBiDirectionRestricting.BitState Then
                        SendControl = True
                    End If
                End If

                If Me.LockControlLeftDirection.BitState Then
                    If Not Me.LockIndLeftDirection.BitState Then
                        SendControl = True
                    End If
                End If

                If Me.LockControlRightDirection.BitState Then
                    If Not Me.LockIndRightDirection.BitState Then
                        SendControl = True
                    End If
                End If

                If SendControl Then
                    Me.myControlPoint.SendCodeStationControlSpecial(True)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ApplyIndicationChange()
            'DebugEvent("DEBUG-ApplyIndicationChange:" & Me.GUID.ToString)

            Try
                Dim RevIndBit, NormIndBit As Boolean
                If Not Me.Ind(0) Is Nothing Then
                    If Me.Ind(0).BitState = True Then
                        NormIndBit = True
                    Else
                        NormIndBit = False
                    End If
                Else
                    NormIndBit = False
                End If

                If Not Me.Ind(1) Is Nothing Then
                    If Me.Ind(1).BitState = True Then
                        RevIndBit = True
                    Else
                        RevIndBit = False
                    End If
                Else
                    If Me.MatchMode Then
                        If Not NormIndBit Then
                            RevIndBit = True
                        Else
                            RevIndBit = False
                        End If
                    Else
                        RevIndBit = False
                    End If

                End If

                If Not Me.SwitchLockInd Is Nothing Then
                    If Me.SwitchLockInd.BitState = True Then
                        SwitchLockBit = True
                    Else
                        SwitchLockBit = False
                    End If
                Else
                    SwitchLockBit = False
                End If

                If Me.RCPS Then
                    ApplyIndicationChangeForRCPS()
                    CheckForPositiveLockIndication()
                End If


                If Me.SwitchPointerMonitorMode Then
                    If Me.ManualSwitchPointVerification Then 'And NormIndBit Then
                        Me.ManualSwitchPointVerification = False
                        Me.FailState = False
                        Me.UpdateSwitchPointStatus()
                    End If
                    If (Not NormIndBit And Not Me.ProtectOpenSwitchEnabled) Or (NormIndBit And RevIndBit) Or (Me.ControlledReverse And Me.ProtectOpenSwitchEnabled And Not RevIndBit And Not NormIndBit) Or (Not Me.ControlledReverse And Me.ProtectOpenSwitchEnabled And Not NormIndBit) Or (Me.ControlledReverse And Me.ProtectOpenSwitchEnabled And RevIndBit) Then
                        If Not ManualSwitchPointVerification Then
                            Me.SwitchPointInitialized = True
                            'Me.Occuppied = True
                            SwitchLockBit = True
                            CheckForExistingAuthorities()
                        ElseIf ManualSwitchPointVerification Then
                            NormIndBit = True
                        End If
                    ElseIf (NormIndBit And Not RevIndBit) Or (Not NormIndBit And RevIndBit) Then
                        Me.ManualSwitchPointVerification = False
                        If NormIndBit And Not RevIndBit Then
                            Me.SwitchIndStatus = SwIndStatus.NormalInd
                        End If

                        Me.Occuppied = False
                        SwitchLockBit = False
                        CheckToAcknowledgeAlerts()
                    End If
                    If Not Me.SwitchPointInitialized Then
                        Me.FailState = False
                        Me.UpdateSwitchPointStatus()
                        Me.SwitchPointInitialized = True
                    End If

                    If Me.PointStatusRecordTime < Now.AddMinutes(-2) Then
                        Me.UpdateSwitchPointStatus()
                    ElseIf Me.FailState Then
                        Me.LastReportTime = Now
                        Me.FailState = False
                        Me.UpdateSwitchPointStatus()
                    End If
                    Me.FailState = False
                    Me.LastReportTime = Now
                    Me.CalculateFailureTime()
                    SwitchEventLog("SPMS EVENT-INDICATION STATUS RCVD @:" & Format(Me.LastReportTime.ToString, "MM-dd-yy HH:mm:ss") & " FOR " & Me.Name & " TIME TO FAILURE:" & Me.AlarmTime.TotalMinutes.ToString & " MINS")
                    If Not IsNothing(Me.mySwitchPositionIndicator) Then
                        Me.mySwitchPositionIndicator.SetPosition()
                    End If

                End If


                'set the switch indication status from indication bits
                If NormIndBit And RevIndBit Then
                    If IgnoreBothIndicationReceipt Then
                        If Me.ControlledReverse Then
                            Me.SwitchIndStatus = SwIndStatus.ReverseInd
                        Else
                            Me.SwitchIndStatus = SwIndStatus.NormalInd
                        End If
                    Else
                        Me.SwitchIndStatus = SwIndStatus.NoIndication
                    End If
                ElseIf Not NormIndBit And Not RevIndBit Then
                    Me.SwitchIndStatus = SwIndStatus.NoIndication
                ElseIf NormIndBit And Not RevIndBit Then
                    Me.SwitchIndStatus = SwIndStatus.NormalInd
                ElseIf Not NormIndBit And RevIndBit Then
                    Me.SwitchIndStatus = SwIndStatus.ReverseInd
                End If


                Dim EventMessage As String = "SWITCH INDICATION RECEIVED. (UPDATE STATUS:" & Me.SwitchIndStatus.ToString.ToUpper & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")(CP:" & TMDS.SystemUtilities.FetchData.FindControlPointName(Me.StationNumber) & ")"
                Me.SwitchEventLog(EventMessage)


                Dim SimulateTheSwitchMove As Boolean
                If Me.SpringSwitch Then
                    If ControlledReverse Then
                        If IsNothing(Me.Ind(1)) Then
                            SimulateTheSwitchMove = True
                        End If
                    Else
                        If IsNothing(Me.Ind(0)) Then
                            SimulateTheSwitchMove = True
                        End If

                    End If
                End If

                If Me.FailState Or Me.DoMonitorMode Or Me.MatchMode Or Not Initialized Or Me.SwitchPointerMonitorMode Then
                    Dim EventMessage1 As String = "SWITCH INITIALIZATION STATE: (FAILSTATE=" & Me.FailState.ToString & ")(INITIALIZATION=" & Me.Initialized.ToString & ")"
                    Me.SwitchEventLog(EventMessage1)

                    If Me.NonOperationSwitchPointMonitor Then
                        Me.ControlledReverse = False
                    End If
                    Select Case Me.SwitchIndStatus
                        Case SwIndStatus.NormalInd ', SwIndStatus.NoIndication
                            Me.SetControlBit(False)
                        Case SwIndStatus.ReverseInd
                            SetControlBit(True)
                    End Select
                    If Me.PreviouSIndStatus <> Me.SwitchIndStatus Then
                        Me.myControlPoint.InitializeSignalRoutes()
                        Me.PreviouSIndStatus = Me.SwitchIndStatus
                    End If

                ElseIf SimulateTheSwitchMove Then
                    Me.SimulateSwitchMove()
                End If

                If PreviousStatus <> Me.SwitchIndStatus Then
                    'PreviousStatus = Me.SwitchIndStatus
                    Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                    LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.IND, Nothing, Nothing)
                    If Me.PublishStatusToPTC Then
                        TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_DeviceStatusBroadcast(Nothing, Me)
                    End If
                End If

                If SwitchLockBitPrevious <> Me.SwitchLockBit Then
                    Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                    LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.LCK, Nothing, Nothing)
                End If

                If Me.SwitchPointerMonitorMode Then
                    If (Me.ControlledReverse And NormIndBit) Or (Not Me.ControlledReverse And RevIndBit) And Me.TrackAuthorities.Count > 0 Then
                        SwitchLockBit = True
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SetControlBit(ByVal SetToReverse As Boolean)
            Try
                If Not SetToReverse Then
                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = True
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = True
                    End If

                    Me.InitSwitchControl(False)
                Else
                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = True
                    End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = True
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If

                    Me.InitSwitchControl(True)
                End If

                'FOR ARRC WIERD TUNNEL OPERATIONS
                If Me.myControlPoint.TunnelAutomaticMode AndAlso Me.myControlPoint.TunnelAutomaticControlled Then
                    'If Not Me.Ctl(1) Is Nothing Then
                    '    Me.Ctl(1).BitState = False
                    'End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    'If Not Me.Ctl(3) Is Nothing Then
                    '    Me.Ctl(3).BitState = False
                    'End If
                    'If Not Me.Ctl(4) Is Nothing Then
                    '    Me.Ctl(4).BitState = False
                    'End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SendSwitchControlUpdate()
            Try
                Dim cp As ControlPoint = Nothing
                Dim myControlPoint As ControlPoint = Nothing
                For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                    If Me.StationNumber = cp.StationNumber Then
                        myControlPoint = cp
                        Exit For
                    End If
                Next
                If Not myControlPoint Is Nothing Then
                    'myControlPoint.SendCodeStationControlUpdate()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function CheckForLockingRCPS() As Boolean
            Try
                Dim SwitchInCorespondence As Boolean
                Dim DirectionalLockEnabled As Boolean
                If Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.ReverseInd Then
                    SwitchInCorespondence = True
                ElseIf Not Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.NormalInd Then
                    SwitchInCorespondence = True
                Else
                    SwitchInCorespondence = False
                End If

                Select Case TMDS.GlobalDeclarations.gWarrantType
                    Case "ItemFourToTrain"
                        If Not IsNothing(Me.LockIndBiDirectionRestricting) Then
                            If Me.LockIndBiDirectionRestricting.BitState Then
                                DirectionalLockEnabled = True
                            End If
                        End If
                        If Not IsNothing(Me.LockIndBiDirectionNoRestriction) Then
                            If Me.LockIndBiDirectionNoRestriction.BitState Then
                                DirectionalLockEnabled = True
                            End If
                        End If

                    Case "ItemTwoToTrain"
                        Select Case TMDS.GlobalDeclarations.gSearchDirection
                            Case "RIGHT"
                                If Not IsNothing(Me.LockIndRightDirection) Then
                                    If Me.LockIndRightDirection.BitState Then
                                        DirectionalLockEnabled = True
                                    End If
                                End If
                            Case "LEFT"
                                If Not IsNothing(Me.LockIndLeftDirection) Then
                                    If Me.LockIndLeftDirection.BitState Then
                                        DirectionalLockEnabled = True
                                    End If
                                End If
                        End Select
                    Case "ItemFourToMOW"
                        DirectionalLockEnabled = True
                End Select


                If Not Me.FailState And SwitchInCorespondence And DirectionalLockEnabled Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Overrides Sub UpdateStatus(Optional ByVal pAuthority As Object = Nothing)
            Dim TypeOfAuthority As String = String.Empty
            Dim ClearMainTypeWarrant As Boolean = False
            Dim isTWCSpecialLicense As Boolean = False
            Try
                If Me.AuthorityType <> "CTC" And Me.AuthorityType <> "OTHER" And Not gSFAMode Then
                    Dim warr As Warrant
                    Dim BoxElevenRestrictionFound As Boolean
                    If AuthorityCounts > 0 Then
                        ActiveTrackWarrant = True
                        LimitsForMOW = False
                        LimitsForTrain = False
                        For Each warr In Authorities
                            If TypeOfAuthority = "" Then
                                TypeOfAuthority = warr.WarrantType
                            End If
                            Select Case warr.WarrantType
                                Case "ItemTwoToMOW", "ItemFourToMOW"
                                    LimitsForMOW = True
                                Case "ItemFourToTrain", "ItemTwoToTrain"
                                    LimitsForTrain = True
                                Case Else
                                    LimitsForTrain = True
                                    LimitsForMOW = True
                            End Select
                            'New Box Eleven Restriction Code
                            Dim BoxElevenRestriction As Object
                            For Each BoxElevenRestriction In warr.BoxElevenComponents
                                If BoxElevenRestriction Is Me Then
                                    BoxElevenRestrictionFound = True
                                End If
                            Next
                            'New Box Eleven Restriction Code
                        Next
                    Else
                        ActiveTrackWarrant = False
                        LimitsForTrain = False
                        LimitsForMOW = False
                        If Me.Occuppied Then
                            CurrentColorStatus = New SolidBrush(Color.Red)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.White)
                        End If
                    End If
                    'New Box Eleven Restriction Code
                    If BoxElevenRestrictionFound Then
                        Me.BoxElevenRestriction = True
                    Else
                        Me.BoxElevenRestriction = False
                    End If
                    'New Box Eleven Restriction Code
                    warr = Nothing
                    Select Case Me.ControlledReverse
                        Case True
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.NormalInd
                                    SetSwitchToOutOfCorrespondenceReverse()
                                Case SwIndStatus.ReverseInd
                                    If Me.PreviousStatus <> SwIndStatus.ReverseInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-1")
                                        SetSwitchToInCorespondence()
                                    End If
                            End Select
                            If Not Me.Initialized Then
                                Me.PreviouSIndStatus = Me.SwitchIndStatus
                            End If
                            Initialized = True
                        Case False
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.ReverseInd
                                    SetSwitchToOutOfCorespondenceNormal()
                                Case SwIndStatus.NormalInd
                                    If Me.PreviousStatus <> SwIndStatus.NormalInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-2")
                                        SetSwitchToInCorespondence()
                                    End If
                            End Select
                            If Not Me.Initialized Then
                                Me.PreviouSIndStatus = Me.SwitchIndStatus
                            End If
                            Initialized = True
                    End Select
                    'Switch Lock
                    If Not Me.mySwitchLock Is Nothing Then
                        If Me.DeviceBlocked And Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.Magenta)
                        ElseIf Me.DeviceBlocked And Not Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.MediumSlateBlue)
                        ElseIf Not Me.DeviceBlocked And Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.Red)
                        Else
                            mySwitchLock.myBrush = New SolidBrush(Color.Black)
                        End If
                        If Me.FailState Then
                            mySwitchLock.myBrush = New SolidBrush(Color.DarkGoldenrod)
                        End If
                        mySwitchLock.Refresh()
                    End If
                ElseIf Me.AuthorityType <> "CTC" And Me.AuthorityType <> "OTHER" And gSFAMode Then
                    Dim warr As SFAuthority
                    Dim BoxEightRestrictionFound As Boolean
                    If Me.TrackAuthorities.Count > 0 Then
                        Dim FirstWarrant As SFAuthority
                        FirstWarrant = Me.TrackAuthorities(0)
                        If FirstWarrant.CheckBoxItemsStatus(2) Then
                            If optUseClearMainTrackColorOption Then
                                ClearMainTypeWarrant = True
                            End If
                        End If
                        ActiveTrackWarrant = True
                        LimitsForMOW = False
                        LimitsForTrain = False
                        isTWCSpecialLicense = FirstWarrant.isSpecialLicense
                        Dim OverRideLimitsForTrain As Boolean
                        If optUseSingleFormAuthorityVersionII Or optUseSingleFormAuthorityVersionCN Or optUseSingleFormAuthorityVersionMRL Or optUseSingleFormAuthorityVersionARRC OrElse optUseSingleFormAuthorityVersionNCTD Then
                            If Me.TrackAuthorities.Count = 1 Then
                                If (FirstWarrant.IssuedToTrain And Not FirstWarrant.ShowItemFourComponents And FirstWarrant.Box4Components.Contains(Me)) Or
                                (FirstWarrant.IssuedToTrain And Not FirstWarrant.ShowItemFourComponents And FirstWarrant.CheckBoxItemsStatus(3) And Not FirstWarrant.CheckBoxItemsStatus(1)) Then
                                    OverRideLimitsForTrain = True
                                    ActiveTrackWarrant = False
                                End If
                            End If
                        End If
                        For Each warr In Me.TrackAuthorities
                            If TypeOfAuthority = "" Then
                                TypeOfAuthority = warr.AuthType1
                            End If
                            Select Case warr.AuthType1
                                Case "ItemTwoToMOW", "ItemFourToMOW"
                                    LimitsForMOW = True
                                Case "ItemFourToTrain", "ItemTwoToTrain"
                                    If Not OverRideLimitsForTrain Then
                                        LimitsForTrain = True
                                    End If
                                Case Else
                                    If Not OverRideLimitsForTrain Then
                                        LimitsForTrain = True
                                    End If
                                    LimitsForMOW = True
                            End Select
                            'New Box Eleven Restriction Code
                            Dim comp As Object
                            For Each comp In warr.BoxEightRestrictionComponents
                                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    Dim trk As Track = CType(comp, Track)
                                    If Not IsNothing(trk.OsComponentCollection) Then
                                        If trk.OsComponentCollection.Contains(Me) Then
                                            BoxEightRestrictionFound = True
                                        End If
                                    End If
                                End If
                            Next
                        Next
                    Else
                        ActiveTrackWarrant = False
                        LimitsForTrain = False
                        LimitsForMOW = False
                        If Me.Occuppied Then
                            CurrentColorStatus = New SolidBrush(Color.Red)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.White)
                            If Me.RCPS Or Me.SwitchPointerMonitorMode Then
                                If Me.FailState And Not Me.SimulateMode And Not optDisableShowingCodeFailureOnComponents Then
                                    CurrentColorStatus = gColorFailedState
                                End If
                            End If
                        End If
                    End If
                    'New Box Eleven Restriction Code
                    If BoxEightRestrictionFound Then
                        Me.BoxEightRestriction = True
                    Else
                        Me.BoxEightRestriction = False
                    End If
                    'New Box Eleven Restriction Code
                    warr = Nothing
                    Select Case Me.ControlledReverse
                        Case True
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.NormalInd
                                    SetSwitchToOutOfCorrespondenceReverse()
                                Case SwIndStatus.ReverseInd
                                    If Me.PreviousStatus <> SwIndStatus.ReverseInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-3")
                                        'SetSwitchToInCorespondence()
                                    End If
                            End Select
                            If Not Me.Initialized Then
                                Me.PreviouSIndStatus = Me.SwitchIndStatus
                            End If
                            Initialized = True
                        Case False
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.ReverseInd
                                    SetSwitchToOutOfCorespondenceNormal()
                                Case SwIndStatus.NormalInd
                                    If Me.PreviousStatus <> SwIndStatus.NormalInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-4")
                                        SetSwitchToInCorespondence()
                                    End If
                            End Select
                            If Not Me.Initialized Then
                                Me.PreviouSIndStatus = Me.SwitchIndStatus
                            End If
                            Initialized = True
                    End Select
                    'Switch Lock
                    If Not Me.mySwitchLock Is Nothing Then
                        'ARRC-1202
                        'mySwitchLock.Visible = True
                        If Me.DeviceBlocked And Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.Magenta)
                        ElseIf Me.DeviceBlocked And Not Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.MediumSlateBlue)
                        ElseIf Not Me.DeviceBlocked And Me.SwitchLockBit Then
                            If Me.DirectionalRCPSLockOn Then
                                mySwitchLock.myBrush = New SolidBrush(Color.LimeGreen)
                            ElseIf Me.TimeLockingOn Then
                                mySwitchLock.myBrush = New SolidBrush(Color.DarkSalmon)
                            ElseIf Me.ManualSwitchPointVerification Then
                                mySwitchLock.myBrush = New SolidBrush(Color.Magenta)
                            Else
                                mySwitchLock.myBrush = New SolidBrush(Color.Red)
                            End If
                        ElseIf Me.ManualSwitchPointVerification Then
                            mySwitchLock.myBrush = New SolidBrush(Color.Cyan)
                        Else
                            mySwitchLock.myBrush = New SolidBrush(Color.Black)
                            'mySwitchLock.Visible = False
                        End If
                        If Me.FailState And Not Me.ManualSwitchPointVerification Then
                            ' If optEnableCTCTAGDispalyonCodeBrown = False Then mySwitchLock.myBrush = New SolidBrush(Color.DarkGoldenrod)
                            mySwitchLock.myBrush = New SolidBrush(Color.DarkGoldenrod)
                        End If
                        mySwitchLock.Invoke(New MethodInvoker(AddressOf Me.mySwitchLock.Refresh))
                    End If
                    If Me.RCPS Then
                        Try
                            Dim SwitchInCorespondence As Boolean
                            Dim DirectionalLockEnabled As Boolean
                            If Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.ReverseInd Then
                                SwitchInCorespondence = True
                            ElseIf Not Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.NormalInd Then
                                SwitchInCorespondence = True
                            Else
                                SwitchInCorespondence = False
                            End If
                            Select Case TMDS.GlobalDeclarations.gWarrantType
                                Case "ItemFourToTrain"
                                    If Not IsNothing(Me.LockIndBiDirectionRestricting) Then
                                        If Me.LockIndBiDirectionRestricting.BitState Then
                                            DirectionalLockEnabled = True
                                        End If
                                    End If
                                    If Not IsNothing(Me.LockIndBiDirectionNoRestriction) Then
                                        If Me.LockIndBiDirectionNoRestriction.BitState Then
                                            DirectionalLockEnabled = True
                                        End If
                                    End If
                                Case "ItemTwoToTrain"
                                    Select Case TMDS.GlobalDeclarations.gSearchDirection
                                        Case "RIGHT"
                                            If Not IsNothing(Me.LockIndRightDirection) Then
                                                If Me.LockIndRightDirection.BitState Then
                                                    DirectionalLockEnabled = True
                                                End If
                                            End If
                                        Case "LEFT"
                                            If Not IsNothing(Me.LockIndLeftDirection) Then
                                                If Me.LockIndLeftDirection.BitState Then
                                                    DirectionalLockEnabled = True
                                                End If
                                            End If
                                    End Select
                                Case "ItemFourToMOW"
                                    DirectionalLockEnabled = True
                            End Select
                            If Not Me.FailState And SwitchInCorespondence And DirectionalLockEnabled Then
                                'send command to sfa form to remove switch from Item 12
                                If Not IsNothing(TMDS.SystemForms.MySystemForms.frmSingleFormAuthority) Then
                                    TMDS.SystemForms.MySystemForms.frmSingleFormAuthority.UpdateStatusOfRCPS()
                                End If
                            End If
                            If Not Me.FailState Then
                                If Not Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.NormalInd Then
                                    Me.ForceProtectionOnRCPS = False
                                Else
                                    Me.ForceProtectionOnRCPS = True
                                End If
                            Else
                                Me.ForceProtectionOnRCPS = True
                            End If
                            If Not Me.Occuppied And Not SwitchInCorespondence Then
                                Dim OsTrackOccuppied As Boolean
                                If Not IsNothing(Me.OsComponentCollection) Then
                                    Dim comp As Object
                                    For Each comp In Me.OsComponentCollection
                                        If comp.Occuppied Then
                                            OsTrackOccuppied = True
                                        End If
                                    Next
                                End If
                                If Not OsTrackOccuppied Then
                                    Dim auth As SFAuthority
                                    For Each auth In Me.TrackAuthorities
                                        If auth.IssuedToTrain Then
                                            Dim OverrideConflict As Boolean = False
                                            If auth.CheckBoxItemsStatus(13) And InStr(auth.Box14Instructions, "YOU ARE INSTRUCTED TO HAND OPERATE THE DUAL CONTROL SWITCH AT") > 0 Then
                                                OverrideConflict = True
                                            End If
                                            If Not OverrideConflict Then
                                                Me.ShowAlarmForOutOfCorrespondenceRCPS(Me, auth)
                                                Exit For
                                            Else
                                                OverrideConflict = False
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Catch ex As Exception
                            Me.SwitchLockBit = True
                            Me.ForceProtectionOnRCPS = True
                            SystemException(ex.ToString)
                        End Try
                    End If
                Else 'CTC
                    'see if there are any track authority type on this component
                    Dim auth As TrackAuthority.TrackAuthority
                    Me.TrackAndTime = False
                    Me.TrackPermit = False
                    Me.BlockAbsolute = False
                    For Each auth In Me.TrackAuthorities
                        Me.BlockAbsolute = True
                        If Mid(auth.AuthorityType, 1, 2) = "TT" Then
                            Me.TrackAndTime = True
                        ElseIf Mid(auth.AuthorityType, 1, 2) = "TP" Then
                            Me.TrackPermit = True
                        ElseIf Mid(auth.AuthorityType, 1, 2) = "TA" Then
                            Me.TrackAuthority = True
                            If Me.TrackAndTime Or TrackPermit Then
                                Me.BlockAbsolute = True
                            Else
                                Me.BlockAbsolute = False
                                Me.TrafficDirection = auth.Direction
                            End If
                        End If
                    Next
                    'check for device blocks
                    If Me.DeviceBlocks.Count > 0 Then
                        Me.BlockAbsolute = True
                        Me.DeviceBlocked = True
                    End If
                    If optEnablePSSEMTAuthorityNoDeviceBlk And Me.OtherSplAuthorities.Count > 0 Then
                        Me.BlockAbsolute = True
                        Me.DeviceBlocked = True
                    ElseIf optEnablePTCBOSVETMSMode And optEnablePTCOperations Then
                        Dim splAuth As TrackAuthority.OtherSpecialAuthorityType
                        For Each splAuth In Me.OtherSplAuthorities
                            Me.BlockAbsolute = optEnablePSSEMTAuthorityApplyBlocking
                            Me.TrafficDirection = splAuth.Direction
                            SpecialAuthorityBlocking = True
                        Next
                    End If
                    If Me.BlockAbsolute <> Me.BlockAbsolutePrevious Then
                        Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                        LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.BLK, Nothing, Nothing)
                        If Me.AllowFieldBlocking Then
                            If Me.DeviceBlocks.Count > 0 Then
                                Me.SetFieldBlockingControl(True)
                            Else
                                Me.SetFieldBlockingControl(False)
                            End If
                        End If
                        Me.BlockAbsolutePrevious = Me.BlockAbsolute
                    End If
                    If Me.AllowSwitchIndBlock Then
                        If Not IsNothing(Me.SwitchBlockInd) Then
                            If Me.SwitchBlockInd.BitState Then
                                Me.DeviceBlocked = True
                                Me.BlockAbsolute = True
                            Else
                                If Me.DeviceBlocks.Count > 0 Then
                                    Me.BlockAbsolute = True
                                    Me.DeviceBlocked = True
                                Else
                                    If Me.TrackAuthorities.Count = 0 Then
                                        Me.BlockAbsolute = False
                                    End If
                                    Me.DeviceBlocked = False
                                    If optEnablePSSEMTAuthorityNoDeviceBlk And Me.OtherSplAuthorities.Count > 0 Then
                                        Me.BlockAbsolute = True
                                        Me.DeviceBlocked = True
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If Me.BlockAbsolute <> Me.BlockAbsolutePrevious Then
                        Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                        LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.BLK, Nothing, Nothing)
                        If Me.AllowFieldBlocking Then
                            If Me.DeviceBlocks.Count > 0 Then
                                Me.SetFieldBlockingControl(True)
                            End If
                        End If
                    End If
                End If
                'Set the color scheme for the current switch condition
                If ActiveTrackWarrant Then
                    If LimitsForTrain And LimitsForMOW And (Not Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.Teal)  'light blue/green
                    ElseIf LimitsForTrain And LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128)) 'hot pink
                    ElseIf LimitsForTrain And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                        Else
                            If Mid(TypeOfAuthority, 1, 7) = "ItemTwo" Then
                                If ClearMainTypeWarrant Then
                                    CurrentColorStatus = New SolidBrush(Color.Yellow)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.Green)
                                    If Me.BoxEightRestriction And optShowJointLimitsColor Then
                                        CurrentColorStatus = New SolidBrush(Color.DarkOliveGreen)
                                    End If
                                    If isTWCSpecialLicense Then 'Special license for TWC no occupancy
                                        CurrentColorStatus = New SolidBrush(Color.DarkTurquoise)
                                    End If
                                End If
                            Else
                                CurrentColorStatus = New SolidBrush(Color.SpringGreen)
                                If Me.BoxEightRestriction And optShowJointLimitsColor Then
                                    CurrentColorStatus = New SolidBrush(Color.DarkOliveGreen)
                                End If
                            End If
                        End If
                    ElseIf LimitsForMOW And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.BlueViolet)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Blue)
                        End If
                    ElseIf LimitsForTrain And (Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.IndianRed)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Orange)
                        End If
                    ElseIf LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 255)) ' magenta
                    Else
                        CurrentColorStatus = New SolidBrush(Color.DarkBlue)
                    End If
                    If Me.ControlledReverse Then
                        CurrentColorStatusNoPath = gColorTrackNotUsable
                    Else
                        CurrentColorStatusNoPath = CurrentColorStatus
                    End If
                ElseIf Me.AuthorityType = "CTC" Or Me.AuthorityType = "OTHER" Then
                    'Look at signal status across switch and update accordingly
                    Select Case SignalStatOnMe
                        Case SignalStatus.SignalCleared, SignalStatus.SignalClearedDrill
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                'CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                                If pAuthority IsNot Nothing Then
                                    CurrentColorStatus = GetColorStatusForPTC(pAuthority)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                                End If

                            ElseIf Me.TrackAuthority Then
                                CurrentColorStatus = GetColorStatusForPTC(pAuthority)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Green)
                            End If
                        Case SignalStatus.SignalRequested, SignalStatus.SignalRequestedDrill
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightBlue)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Yellow)
                            End If
                        Case SignalStatus.PendingStateChange
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightGoldenrodYellow)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Yellow)
                            End If
                        Case SignalStatus.SignalInTime
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.DarkSalmon)
                            End If
                        Case SignalStatus.SignalOfficeLock
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Orange)
                            End If
                        Case SignalStatus.SignalUnSolicated
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.DarkViolet)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.MediumSpringGreen)
                            End If
                        Case SignalStatus.SignalAtStop
                            If Me.BlockAbsolute And Not Me.Occuppied And Not Me.DeviceBlocked Then
                                'CurrentColorStatus = New SolidBrush(Color.DodgerBlue)
                                If optDisableFlashingTrackForPTC Then
                                    CurrentColorStatus = GetColorStatusForPTC(pAuthority)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.DodgerBlue)
                                End If
                            ElseIf Me.BlockAbsolute And Not Me.Occuppied And Me.DeviceBlocked Then
                                'CurrentColorStatus = New SolidBrush(Color.MediumSlateBlue)
                                If optDisableFlashingTrackForPTC Then
                                    CurrentColorStatus = GetColorStatusForPTC(pAuthority, True)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.MediumSlateBlue)
                                End If
                            ElseIf Not Me.BlockAbsolute And Not Me.Occuppied And Me.TrackTagRestrictive Then
                                If Me.trackTagAlarm Then
                                    CurrentColorStatus = New SolidBrush(Color.DarkOrange)
                                ElseIf Me.trackTagPlan Then
                                    CurrentColorStatus = New SolidBrush(Color.White)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.Cyan)
                                End If

                            ElseIf Me.BlockAbsolute And Me.Occuppied Then
                                CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128))
                            Else
                                If Me.TrackTagRestrictive Then
                                    CurrentColorStatus = New SolidBrush(Color.Cyan)
                                Else
                                    If Me.TrackAuthority Then
                                        If Me.Occuppied Then
                                            CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128))
                                        Else
                                            CurrentColorStatus = GetColorStatusForPTC(pAuthority)
                                        End If
                                    Else
                                        CurrentColorStatus = New SolidBrush(Color.White)
                                    End If
                                End If
                            End If
                        Case Else
                            If Me.Occuppied Then
                                CurrentColorStatus = New SolidBrush(Color.Red)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.White)
                                If Me.TrackTagRestrictive Then
                                    CurrentColorStatus = New SolidBrush(Color.Cyan)
                                End If
                            End If
                    End Select
                    'override all states if the track is occuppied
                    If Me.Occuppied And Not Me.BlockAbsolute Then
                        CurrentColorStatus = New SolidBrush(Color.Red)
                    ElseIf Me.Occuppied And (Me.BlockAbsolute Or Me.TrackTagRestrictive) Then
                        CurrentColorStatus = New SolidBrush(Color.Magenta)
                    End If
                    If Me.NonTransverable Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(128, 128, 128)) 'gray
                    End If
                    'override all states if the track is in failed state
                    If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) And Not optDisableShowingCodeFailureOnComponents Then
                        CurrentColorStatus = gColorFailedState
                    End If
                    If Me.ControlledReverse Then
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        Else
                            CurrentColorStatusNoPath = CurrentColorStatus
                        End If
                    Else
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = CurrentColorStatus
                        Else
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        End If
                    End If
                    Select Case Me.ControlledReverse
                        Case True
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.NormalInd
                                    If Not Me.SetOutOfCorespondenceReverse Then
                                        SetSwitchToOutOfCorrespondenceReverse()
                                        Me.SetOutOfCorespondenceNormal = False
                                    End If
                                    If Not Me.Initialized Then
                                        Me.PreviouSIndStatus = Me.SwitchIndStatus
                                    End If
                                    Initialized = True
                                Case SwIndStatus.ReverseInd
                                    If Me.PreviousStatus <> SwIndStatus.ReverseInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Or Not Initialized Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-5")
                                        SetSwitchToInCorespondence()
                                        If Not Me.Initialized Then
                                            Me.PreviouSIndStatus = Me.SwitchIndStatus
                                        End If
                                        Initialized = True
                                    End If
                            End Select
                        Case False
                            Select Case Me.SwitchIndStatus
                                Case SwIndStatus.NoIndication, SwIndStatus.ReverseInd
                                    If Not Me.SetOutOfCorespondenceNormal Then
                                        SetSwitchToOutOfCorespondenceNormal()
                                        Me.SetOutOfCorespondenceReverse = False
                                    End If
                                    If Not Me.Initialized Then
                                        Me.PreviouSIndStatus = Me.SwitchIndStatus
                                    End If
                                    Initialized = True
                                Case SwIndStatus.NormalInd
                                    If Me.PreviousStatus <> SwIndStatus.NormalInd Or Me.PreviousControlStatus <> Me.CurrentControlStatus Or Not Initialized Then
                                        SwitchEventLog("SWITCH-UPDATESTATUS-SetSwitchToInCorespondence-6")
                                        SetSwitchToInCorespondence()
                                        If Not Me.Initialized Then
                                            Me.PreviouSIndStatus = Me.SwitchIndStatus
                                        End If
                                        Initialized = True
                                    End If
                            End Select
                    End Select
                    'Switch Lock
                    If Not Me.mySwitchLock Is Nothing Then
                        If Me.DeviceBlocked And Me.SwitchLockBit Then
                            mySwitchLock.myBrush = New SolidBrush(Color.Magenta)
                        ElseIf Me.DeviceBlocked And Not Me.SwitchLockBit Then
                            'mySwitchLock.Visible = True
                            Dim BlockIndicated As Boolean
                            If Not IsNothing(Me.SwitchBlockInd) Then
                                If Me.SwitchBlockInd.BitState Then
                                    BlockIndicated = True
                                End If
                            End If
                            If BlockIndicated Then
                                mySwitchLock.myBrush = New SolidBrush(Color.Magenta)
                            Else
                                mySwitchLock.myBrush = New SolidBrush(Color.MediumSlateBlue)
                            End If
                        ElseIf Not Me.DeviceBlocked And Me.SwitchLockBit Then

                            mySwitchLock.myBrush = New SolidBrush(Color.Red)
                        Else
                            mySwitchLock.myBrush = New SolidBrush(Color.Black)
                        End If
                        mySwitchLock.Invoke(New MethodInvoker(AddressOf Me.mySwitchLock.Refresh))
                    End If
                Else
                    If Not Me.Occuppied Then
                        CurrentColorStatus = New SolidBrush(Color.White)
                        If Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.Cyan)
                        End If

                        If Me.RCPS Or Me.SwitchPointerMonitorMode Then
                            If Me.FailState And Not optDisableShowingCodeFailureOnComponents Then
                                CurrentColorStatus = gColorFailedState
                            End If
                        End If
                    Else
                        CurrentColorStatus = New SolidBrush(Color.Red)
                    End If
                    If Me.ControlledReverse Then
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        Else
                            CurrentColorStatusNoPath = CurrentColorStatus
                        End If
                    Else
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = CurrentColorStatus
                        Else
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        End If
                    End If
                End If
                'This is to position the switch based on the way the track warrant is placed
                If Authorities.Count > 0 Then
                    If RightAltLink <> 0 Then
                        Dim trk As Track = FindTrackItemFromGUID(RightAltLink)
                        If trk.Authorities.Count > 0 And trk.TrafficDirection = "LEFT" Then
                            Dim RightAltTrackWarrant As Warrant = trk.Authorities(0)
                            Dim ThisTrackWarrant As Warrant = Authorities(0)
                            If ThisTrackWarrant.WarrantNumber = RightAltTrackWarrant.WarrantNumber Then
                                If Not pControlled And LeaveReversePermission Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                End If
                            End If
                        End If
                    ElseIf LeftAltLink <> 0 Then
                        Dim trk As Track = FindTrackItemFromGUID(LeftAltLink)
                        If trk.Authorities.Count > 0 And trk.TrafficDirection = "RIGHT" Then
                            Dim LeftAltTrackWarrant As Warrant = trk.Authorities(0)
                            Dim ThisTrackWarrant As Warrant = Authorities(0)
                            If ThisTrackWarrant.WarrantNumber = LeftAltTrackWarrant.WarrantNumber Then
                                If Not pControlled And LeaveReversePermission Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                End If
                            End If
                        End If
                    End If
                End If
                PreviousStatus = Me.SwitchIndStatus
                Me.PreviousControlStatus = Me.CurrentControlStatus
                SwitchLockBitPrevious = Me.SwitchLockBit
                If optAllowPOSCheck Then
                    ProtectOpenSwitchCheck()
                End If
                If Not Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.NormalInd And Not Me.DoSimulate Then
                    Me.tmrReControl.Enabled = False
                    Me.ReControlStart = False
                    Me.ReControlCount = 0
                ElseIf Me.ControlledReverse And Me.SwitchIndStatus = SwIndStatus.ReverseInd And Not Me.DoSimulate Then
                    Me.tmrReControl.Enabled = False
                    Me.ReControlStart = False
                    Me.ReControlCount = 0
                End If
                If Me.myControlPoint.AllowStatusPublishing Then
                    Me.PublishStatus()
                End If
                If Me.IsHandleCreated Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If
            Catch ex As Exception
                SystemException(GetExceptionAllFrameMsg(ex, New StackTrace(True)))
            End Try
        End Sub

        ''' <summary>
        ''' Get color status for ptc
        ''' </summary>
        ''' <param name="pTATAuthority"></param>
        ''' <param name="pIsDeviceBlock"></param>
        ''' <returns></returns>
        Private Function GetColorStatusForPTC(ByVal pTATAuthority As Object, Optional ByVal pIsDeviceBlock As Boolean = False) As SolidBrush
            Dim auth As Object = Nothing
            Try
                If pTATAuthority IsNot Nothing AndAlso pTATAuthority.GetType.ToString = gTTComponentName Then
                    auth = pTATAuthority
                ElseIf pTATAuthority IsNot Nothing AndAlso pTATAuthority.GetType.ToString = gOtherSplAuthorityName Then
                    auth = pTATAuthority
                Else
                    If Me.TrackAuthorities IsNot Nothing AndAlso Me.TrackAuthorities.Count > 0 Then auth = Me.TrackAuthorities.Item(Me.TrackAuthorities.Count - 1)
                End If
                If auth IsNot Nothing Then
                    Select Case CType(auth.PTCSTatusSelectionMode, TrackComponents.Track.ModeSelection)
                        Case ModeSelection.PTCIssuanceImminent
                            Return ColorStatusPTCIssuanceImminent
                        Case ModeSelection.PTCIssuancePending
                            Return ColorStatusPTCIssuancePending
                        Case ModeSelection.PTCReleasePending
                            Return ColorStatusPTCReleasePending
                        Case ModeSelection.PTCReleaseImminent
                            Return ColorStatusPTCReleaseImminent
                    End Select
                End If
                If optEnableMRSSpecificOptions Then
                    Return New SolidBrush(Color.SeaGreen)
                Else
                    If pIsDeviceBlock Then
                        Return New SolidBrush(gDeviceBlockColor)
                    Else
                        Return New SolidBrush(Color.DodgerBlue)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                If optEnableMRSSpecificOptions Then Return New SolidBrush(Color.SeaGreen) Else Return New SolidBrush(Color.DodgerBlue)
            Finally
                auth = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Get color status for ptc
        ''' </summary>
        ''' <param name="pCurrentColor"></param>
        ''' <returns></returns>
        Private Function GetColorStatusForPTC(ByVal pCurrentColor As Color) As SolidBrush
            Dim auth As Object = Nothing
            Dim tat As TMDS.TrackAuthority.TrackAuthority = Nothing
            Try
                If Me.TrackAuthorities.Count = 0 Then Return New SolidBrush(pCurrentColor)
                auth = Me.TrackAuthorities.Item(Me.TrackAuthorities.Count - 1)
                If Not IsNothing(auth) AndAlso auth.GetType.ToString = gTTComponentName Then
                    tat = auth
                    Select Case tat.PTCSTatusSelectionMode
                        Case ModeSelection.PTCIssuanceImminent
                            Return ColorStatusPTCIssuanceImminent
                        Case ModeSelection.PTCIssuancePending
                            Return ColorStatusPTCIssuancePending
                        Case ModeSelection.PTCReleasePending
                            Return ColorStatusPTCReleasePending
                        Case ModeSelection.PTCReleaseImminent
                            Return ColorStatusPTCReleaseImminent
                    End Select
                    If tat.PTCTypeAuthority AndAlso tat.PTCSTatusSelectionMode = ModeSelection.Regular AndAlso optEnableLicenseGenerationForPTC Then
                        Return New SolidBrush(Color.SeaGreen)
                    End If
                End If
                Return New SolidBrush(pCurrentColor)
            Catch ex As Exception
                SystemException(ex.ToString)
                Return New SolidBrush(pCurrentColor)
            Finally
                auth = Nothing
                tat = Nothing
            End Try
        End Function

        Sub UpdateColorStatusOnly()
            Dim isTWCSpecialLicense As Boolean = False
            Try
                'Look at signal status across switch and update accordingly
                Dim ClearMainTypeWarrant As Boolean
                If ActiveTrackWarrant And gSFAMode Then
                    Dim warr As SFAuthority
                    Dim FirstWarrant As SFAuthority = Nothing
                    If Me.TrackAuthorities IsNot Nothing AndAlso Me.TrackAuthorities.Count > 0 Then FirstWarrant = Me.TrackAuthorities(0)
                    If FirstWarrant IsNot Nothing AndAlso FirstWarrant.CheckBoxItemsStatus(2) Then
                        If optUseClearMainTrackColorOption Then
                            ClearMainTypeWarrant = True
                        End If
                    End If
                    If FirstWarrant IsNot Nothing Then
                        isTWCSpecialLicense = FirstWarrant.isSpecialLicense
                    End If
                    Dim TypeOfAuthority As String = String.Empty
                    For Each warr In Me.TrackAuthorities
                        If TypeOfAuthority = "" Then
                            TypeOfAuthority = warr.AuthType1
                        End If
                        Select Case warr.AuthType1

                            Case "ItemTwoToMOW", "ItemFourToMOW"
                                LimitsForMOW = True
                            Case "ItemFourToTrain", "ItemTwoToTrain"
                                LimitsForTrain = True
                            Case Else
                                LimitsForTrain = True
                                LimitsForMOW = True
                        End Select
                    Next
                    If LimitsForTrain And LimitsForMOW And (Not Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.Teal)  'light blue/green
                    ElseIf LimitsForTrain And LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128)) 'hot pink
                    ElseIf LimitsForTrain And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                        Else
                            If Mid(TypeOfAuthority, 1, 7) = "ItemTwo" Then
                                If ClearMainTypeWarrant Then
                                    CurrentColorStatus = New SolidBrush(Color.Yellow)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.Green)
                                    If Me.BoxEightRestriction And optShowJointLimitsColor Then
                                        CurrentColorStatus = New SolidBrush(Color.DarkOliveGreen)
                                    End If
                                    If isTWCSpecialLicense Then 'Special license for TWC no occupancy
                                        CurrentColorStatus = New SolidBrush(Color.DarkTurquoise)
                                    End If
                                End If
                            Else
                                CurrentColorStatus = New SolidBrush(Color.SpringGreen)
                                If Me.BoxEightRestriction And optShowJointLimitsColor Then
                                    CurrentColorStatus = New SolidBrush(Color.DarkOliveGreen)
                                End If
                            End If
                        End If
                    ElseIf LimitsForMOW And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.BlueViolet)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Blue)
                        End If

                    ElseIf LimitsForTrain And (Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.IndianRed)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Orange)
                        End If
                    ElseIf LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 255)) ' magenta
                    Else
                        CurrentColorStatus = New SolidBrush(Color.DarkBlue)
                    End If
                    If Me.ControlledReverse Then
                        CurrentColorStatusNoPath = gColorTrackNotUsable
                    Else
                        CurrentColorStatusNoPath = CurrentColorStatus
                    End If
                ElseIf Not ActiveTrackWarrant And gSFAMode And Me.AuthorityType <> "CTC" And Me.AuthorityType <> "OTHER" Then
                    If Me.Occuppied Then
                        CurrentColorStatus = New SolidBrush(Color.Red)
                    Else
                        CurrentColorStatus = New SolidBrush(Color.White)
                        If Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.Cyan)
                        End If
                        If Me.FailState And Not optDisableShowingCodeFailureOnComponents Then
                            If Me.OverRideCodeBrown = False Then
                                CurrentColorStatus = gColorFailedState
                                CurrentColorStatusNoPath = gColorFailedState
                            End If
                        End If
                    End If
                    If Me.ControlledReverse Then
                        CurrentColorStatusNoPath = gColorTrackNotUsable
                    Else
                        CurrentColorStatusNoPath = CurrentColorStatus
                    End If
                ElseIf ActiveTrackWarrant And Not gSFAMode Then
                    Dim warr As Warrant = Nothing
                    Dim FirstWarrant As Warrant = Nothing
                    FirstWarrant = Me.Authorities(0)
                    Dim TypeOfAuthority As String = String.Empty
                    For Each warr In Me.Authorities
                        If TypeOfAuthority = "" Then
                            TypeOfAuthority = warr.WarrantType
                        End If
                        Select Case warr.WarrantType

                            Case "ItemTwoToMOW", "ItemFourToMOW"
                                LimitsForMOW = True
                            Case "ItemFourToTrain", "ItemTwoToTrain"
                                LimitsForTrain = True
                            Case Else
                                LimitsForTrain = True
                                LimitsForMOW = True
                        End Select
                    Next
                    If LimitsForTrain And LimitsForMOW And (Not Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.Teal)  'light blue/green
                    ElseIf LimitsForTrain And LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128)) 'hot pink
                    ElseIf LimitsForTrain And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                        Else
                            If Mid(TypeOfAuthority, 1, 7) = "ItemTwo" Then
                                If ClearMainTypeWarrant Then
                                    CurrentColorStatus = New SolidBrush(Color.Yellow)
                                Else
                                    CurrentColorStatus = New SolidBrush(Color.Green)
                                End If
                            Else
                                CurrentColorStatus = New SolidBrush(Color.SpringGreen)
                            End If
                        End If
                    ElseIf LimitsForMOW And (Not Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.BlueViolet)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Blue)
                        End If

                    ElseIf LimitsForTrain And (Occuppied) Then
                        If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                            CurrentColorStatus = New SolidBrush(Color.IndianRed)
                        Else
                            CurrentColorStatus = New SolidBrush(Color.Orange)
                        End If
                    ElseIf LimitsForMOW And (Occuppied) Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 255)) ' magenta
                    Else
                        CurrentColorStatus = New SolidBrush(Color.DarkBlue)
                    End If
                    If Me.ControlledReverse Then
                        CurrentColorStatusNoPath = gColorTrackNotUsable
                    Else
                        CurrentColorStatusNoPath = CurrentColorStatus
                    End If
                Else
                    Select Case SignalStatOnMe
                        Case SignalStatus.SignalCleared, SignalStatus.SignalClearedDrill
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                'CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                                CurrentColorStatus = GetColorStatusForPTC(Color.Green)
                            Else
                                'CurrentColorStatus = New SolidBrush(Color.Green)
                                CurrentColorStatus = GetColorStatusForPTC(Color.Green)
                            End If
                        Case SignalStatus.PendingStateChange
                            CurrentColorStatus = New SolidBrush(Color.LightYellow)
                        Case SignalStatus.SignalRequested, SignalStatus.SignalRequestedDrill
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightBlue)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Yellow)
                            End If
                            'CurrentColorStatus = New SolidBrush(Color.LightYellow)
                        Case SignalStatus.SignalInTime
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.DarkSalmon)
                            End If
                        Case SignalStatus.SignalOfficeLock
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.LightSeaGreen)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.Orange)
                            End If
                        Case SignalStatus.SignalUnSolicated
                            If Me.BlockAbsolute Or Me.TrackTagRestrictive Then
                                CurrentColorStatus = New SolidBrush(Color.DarkViolet)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.MediumSpringGreen)
                            End If
                        Case SignalStatus.SignalAtStop
                            If Me.BlockAbsolute And Not Me.Occuppied And Not Me.DeviceBlocked Then
                                CurrentColorStatus = New SolidBrush(Color.DodgerBlue)
                            ElseIf Me.BlockAbsolute And Not Me.Occuppied And Me.DeviceBlocked Then
                                CurrentColorStatus = New SolidBrush(Color.MediumSlateBlue)

                            ElseIf (Me.BlockAbsolute Or Me.TrackTagRestrictive) And Me.Occuppied Then
                                CurrentColorStatus = New SolidBrush(Color.FromArgb(255, 0, 128))
                            Else
                                If Me.TrackTagRestrictive Then
                                    If Me.trackTagAlarm Then
                                        CurrentColorStatus = New SolidBrush(Color.DarkOrange)
                                    ElseIf Me.trackTagPlan Then
                                        CurrentColorStatus = New SolidBrush(Color.White)
                                    Else
                                        CurrentColorStatus = New SolidBrush(Color.Cyan)
                                    End If
                                Else
                                    CurrentColorStatus = GetColorStatusForPTC(Color.White)
                                End If
                            End If
                        Case Else
                            If Me.Occuppied Then
                                CurrentColorStatus = New SolidBrush(Color.Red)
                            Else
                                CurrentColorStatus = New SolidBrush(Color.White)
                                If Me.TrackTagRestrictive Then
                                    CurrentColorStatus = New SolidBrush(Color.Cyan)
                                End If
                            End If
                    End Select
                    'override all states if the track is occuppied
                    If Me.Occuppied And Not Me.BlockAbsolute Then
                        CurrentColorStatus = New SolidBrush(Color.Red)
                    ElseIf Me.Occuppied And (Me.BlockAbsolute Or Me.TrackTagRestrictive) Then
                        CurrentColorStatus = New SolidBrush(Color.Magenta)
                    End If
                    If Me.ControlledReverse Then
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        Else
                            CurrentColorStatusNoPath = CurrentColorStatus
                        End If
                    Else
                        If Not Me.FlipOperations Then
                            CurrentColorStatusNoPath = CurrentColorStatus
                        Else
                            CurrentColorStatusNoPath = gColorTrackNotUsable
                        End If
                    End If
                    If Me.NonTransverable Then
                        CurrentColorStatus = New SolidBrush(Color.FromArgb(128, 128, 128)) 'gray
                    End If
                    If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) And Not optDisableShowingCodeFailureOnComponents Then
                        If Me.OverRideCodeBrown = False Then
                            CurrentColorStatus = gColorFailedState
                            CurrentColorStatusNoPath = gColorFailedState
                        End If
                    End If
                End If
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ApplyTraffic()
            'nothing here to do, just a dummy sub to match up to the track class
        End Sub

        ''' <summary>
        ''' Set Switch To Out Of Correspondence Reverse
        ''' </summary>
        Sub SetSwitchToOutOfCorrespondenceReverse()
            Try
                If Not SwitchPointerMonitorMode Then
                    SwitchEventLog($"{GetCurrentMethod.Name}:NotSwitchPointerMonitorMode")
                    RemoveFromFlashCollection(Me)
                    For Each oneTrack As Track In ReverseRouteCollection
                        AddToFlashCollection(oneTrack)
                        oneTrack.FlashComponent = True
                        oneTrack.Invoke(New MethodInvoker(AddressOf oneTrack.Refresh))
                    Next
                    For Each oneTrack As Track In NormalRouteCollection
                        RemoveFromFlashCollection(oneTrack)
                        oneTrack.FlashComponent = False
                        oneTrack.Invoke(New MethodInvoker(AddressOf oneTrack.Refresh))
                    Next
                    FlasherMode(True)
                    SetOutOfCorespondenceReverse = True
                Else
                    SwitchEventLog($"{GetCurrentMethod.Name}:SwitchPointerMonitorMode")
                    SetOutOfCorespondenceReverse = True
                    If mySwitchPositionIndicator IsNot Nothing Then
                        mySwitchPositionIndicator.SetPosition()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Set Switch To Out Of Correspondence Normal
        ''' </summary>
        Sub SetSwitchToOutOfCorespondenceNormal()
            Try
                If Not SwitchPointerMonitorMode Then
                    SwitchEventLog($"{GetCurrentMethod.Name}:NotSwitchPointerMonitorMode")
                    For Each oneTrack As Track In NormalRouteCollection
                        AddToFlashCollection(oneTrack)
                        oneTrack.FlashComponent = True
                        oneTrack.Invoke(New MethodInvoker(AddressOf oneTrack.Refresh))
                    Next
                    For Each oneTrack As Track In ReverseRouteCollection
                        RemoveFromFlashCollection(oneTrack)
                        oneTrack.FlashComponent = False
                        oneTrack.Invoke(New MethodInvoker(AddressOf oneTrack.Refresh))
                    Next
                    FlasherMode(True)
                    SetOutOfCorespondenceNormal = True
                Else
                    SwitchEventLog($"{GetCurrentMethod.Name}:SwitchPointerMonitorMode")
                    SetOutOfCorespondenceNormal = True
                    If mySwitchPositionIndicator IsNot Nothing Then
                        mySwitchPositionIndicator.SetPosition()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetSwitchToInCorespondence()
            Try
                If Not Me.SwitchPointerMonitorMode Then
                    SwitchEventLog("BEGIN-SetSwitchToInCorespondence")

                    Dim trk As Track
                    For Each trk In Me.ReverseRouteCollection
                        If trk IsNot Nothing Then
                            Me.RemoveFromFlashCollection(trk)
                            trk.FlashComponent = False
                        End If
                    Next
                    SwitchEventLog("STEP1-SetSwitchToInCorespondence")

                    For Each trk In Me.ReverseRouteCollection
                        If trk IsNot Nothing Then trk.UpdateStatus()
                    Next

                    SwitchEventLog("STEP2-SetSwitchToInCorespondence")

                    For Each trk In Me.NormalRouteCollection
                        If trk IsNot Nothing Then
                            Me.RemoveFromFlashCollection(trk)
                            trk.FlashComponent = False
                        End If
                    Next
                    SwitchEventLog("STEP3-SetSwitchToInCorespondence")

                    For Each trk In Me.NormalRouteCollection
                        If trk IsNot Nothing Then trk.UpdateStatus()
                    Next
                    SwitchEventLog("STEP4-SetSwitchToInCorespondence")

                    FlasherMode(False, True)
                    SwitchEventLog("END-SetSwitchToInCorespondence")
                    Me.SetOutOfCorespondenceReverse = False
                    Me.SetOutOfCorespondenceNormal = False

                    If Me.Occuppied Then
                        Dim LogThisEvent As New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                        LogThisEvent.BeginInvoke(EventTypeSwitch.OOC, Nothing, Nothing)
                    End If
                Else
                    Me.SetOutOfCorespondenceReverse = False
                    Me.SetOutOfCorespondenceNormal = False
                    If Not IsNothing(Me.mySwitchPositionIndicator) Then
                        Me.mySwitchPositionIndicator.SetPosition()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function CheckBrotherSwitchesForLock(ByVal ConfirmedToMove As Boolean, Optional ByVal OKtoMoveSwitch As Boolean = False) As Boolean
            Try
                Dim sw As Switch
                For Each sw In Me.BrotherSwitches
                    If Not sw Is Me Then
                        If sw.Occuppied Then
                            'SetCursor(curDefault)
                            'gSendCTC = False
                            Return False
                        End If
                        'If sw.ControlledReverse <> ReverseControl Then
                        If sw.SwitchLockBit Then
                            'SetCursor(curDefault)
                            'gSendCTC = False
                            Return False
                        End If
                        If sw.SignalStatOnMe <> SignalStatus.SignalAtStop Then
                            Return False
                        End If
                        'End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Function CheckBrotherSwitchesToMove(ByVal ConfirmedToMove As Boolean, Optional ByVal OKtoMoveSwitch As Boolean = False) As Boolean
            Try
                DebugEvent("DEBUG-CheckBrotherSwitchesToMove:" & Me.GUID.ToString)

                Dim sw As Switch
                For Each sw In Me.BrotherSwitches
                    If Not sw Is Me Then
                        If sw.Occuppied Then
                            SetCursor(curDefault)
                            'sw.ParentForm.Cursor = curDefault
                            gSendCTC = False
                            Return False
                        End If
                        'If sw.ControlledReverse <> ReverseControl Then
                        If sw.DeviceBlocked Or sw.SwitchLockBit Then
                            SetCursor(curDefault)
                            'sw.ParentForm.Cursor = curDefault
                            gSendCTC = False
                            Return False
                        End If
                        If sw.SignalStatOnMe = SignalStatus.SignalAtStop Then
                            If (sw.TrackAndTime Or sw.FoulTime Or sw.TrackPermit) And Not ConfirmedToMove Then
                                Dim UserMessage As Integer
                                Dim ControlPointName As String = RetrieveName(sw.StationNumber)
                                If sw.myControlPoint IsNot Nothing Then ControlPointName = sw.myControlPoint.Name
                                Dim MessageString As String = "A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT." & vbCrLf & vbCrLf
                                MessageString = MessageString & "ARE YOU SURE IT IS SAFE TO PROCEED?"
                                UserMessage = MessageBox.Show(sw.CurrentDrawingForm, MessageString, "PROTECT SWITCH VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                If UserMessage <> 6 Then
                                    Return False
                                End If
                            End If
                        Else
                            Return False
                        End If
                        'End If

                        If Not OKtoMoveSwitch Then
                            If Not sw.TrackBlockedOK() Then
                                SetCursor(curDefault)
                                'sw.ParentForm.Cursor = curDefault
                                gSendCTC = False
                                Return False
                            End If
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Function CheckBrotherICSSwitchesToMove(ByVal ConfirmedToMove As Boolean, Optional ByVal OKtoMoveSwitch As Boolean = False) As Boolean
            Try
                DebugEvent("DEBUG-CheckBrotherICSSwitchesToMove:" & Me.GUID.ToString)

                Dim sw As Switch
                For Each sw In Me.BrotherSwitches
                    If Not sw Is Me Then
                        If sw.Occuppied Then
                            Return False
                        End If
                        If sw.DeviceBlocked Or sw.SwitchLockBit Then
                            Return False
                        End If
                        Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.RightLink)
                        If Not IsNothing(RightTrack) Then
                            If RightTrack.DeviceBlocked Then
                                Return False
                            End If
                        End If
                        If sw.SignalStatOnMe = SignalStatus.SignalAtStop Then
                            If (sw.TrackAndTime Or sw.FoulTime Or sw.TrackPermit) And Not ConfirmedToMove Then
                                Return False
                            End If
                        Else
                            Return False
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Overloads Function IsSwitch(ByVal RequestPositionChange As Boolean, ByVal e As System.Windows.Forms.MouseEventArgs) As Boolean
            Try
                ConfirmationToMoveSwitch = False
                'Version 227
                'if in failed state return false
                If Me.FailState Then Return False

                If RequestPositionChange Then
                    If Me.AuthorityType <> "CTC" Then
                        If Me.DeviceBlocked Then
                            Return False
                        End If
                        'If Not ActiveTrackWarrant Then
                        If ControlledReverse And LeaveReversePermission And ProtectOpenSwitchEnabled Then
                            Dim LaunchPoint As Point = (Me.PointToScreen(New Point(e.X, e.Y + 80)))
                            Dim frmSwitchDialog As frmSwitchDialog
                            frmSwitchDialog = New frmSwitchDialog()
                            frmSwitchDialog.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                            frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "")
                            frmSwitchDialog.ShowDialog(Me.CurrentDrawingForm)
                            If frmSwitchDialog.UserResponse Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    LeaveReversePermission = False
                                Else
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    'MsgBox(ErrorMessage, vbCritical)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If
                            'SystemForms.MySystemForms.frmSwitchDialog.Dispose()
                            SystemForms.MySystemForms.frmSwitchDialog = Nothing
                            frmSwitchDialog.Dispose()
                            frmSwitchDialog = Nothing
                        ElseIf Not ProtectOpenSwitchEnabled Then
                            ControlSwitch()
                            ControlBrotherSwitches(Index, Me.ControlledReverse)
                            If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                            End If
                            'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                        ElseIf Not ProtectOpenSwitchEnabled And ControlledReverse Then
                            ControlSwitch()
                            ControlBrotherSwitches(Index, Me.ControlledReverse)
                            If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                            End If
                            'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                        Else
                            Dim UserMessage As Integer
                            Dim MessageString As String = Name & " HAS BEEN SELECTED FOR REVERSE POSITION." & vbCrLf & vbCrLf &
                            "SWITCH WILL BE PROTECTED UNTIL VERIFIED IN THE NORMAL POSITION."
                            UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT OPEN SWITCH VERIFICATION", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                            'UserMessage = MsgBox(MessageString, MsgBoxStyle.OKCancel, "PROTECT OPEN SWITCH VERIFICATION")
                            If UserMessage = 1 Then
                                Dim ErrorMessage As String
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)

                                If Not UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    'MsgBox(ErrorMessage, vbCritical)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                Else
                                    'IsSwitch = True
                                End If
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If

                        End If

                    Else


                        If Me.Occuppied Or Me.DeviceBlocked Or Me.SwitchLockBit Then
                            SetCursor(curDefault)
                            'Me.ParentForm.Cursor = curDefault
                            gSendCTC = False
                            Return False
                        End If

                        If Not TrackBlockedOK() Then
                            SetCursor(curDefault)
                            'Me.ParentForm.Cursor = curDefault
                            gSendCTC = False
                            Return False
                        End If

                        If Me.SignalStatOnMe = SignalStatus.SignalAtStop Then
                            If Me.TrackAndTime Or Me.FoulTime Then
                                Dim UserMessage As Integer
                                Dim ControlPointName As String = RetrieveName(Me.StationNumber)
                                Dim MessageString As String = "A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT." & vbCrLf & vbCrLf
                                MessageString = MessageString & "ARE YOU SURE IT IS SAFE TO PROCEED?"
                                UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT SWITCH VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                If UserMessage <> 6 Then
                                    Return False
                                End If
                            End If

                            If Me.CheckBrotherSwitchesToMove(Me.ControlledReverse, ConfirmationToMoveSwitch) Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                Return True
                            End If
                        Else
                            Return False
                        End If
                    End If
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Find Brother Switch By Name
        ''' </summary>
        ''' <param name="componentList">component List</param>
        ''' <returns><c>True</c>Found switch<c>False</c>Not found</returns>
        Private Function FindBrotherSwitchByName(componentList As ArrayList) As Boolean
            Try
                For Each oneSwitch As Switch In componentList.OfType(Of Switch)
                    If oneSwitch.Name.Equals(Name) Then
                        SwitchEventLog($"BrotherSwitchFound={oneSwitch.GUID}:{oneSwitch.Name}")
                        Return True
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        Public Overloads Function IsSwitch(ByRef pDBConnection As OleDbConnection,
                                           pRequestPositionChange As Boolean,
                                           pEvent As MouseEventArgs,
                                           Optional pDoICS As Boolean = False) As Boolean
            Dim LPoint As Point = New Point(CurrentDrawingForm.Left + 200, CurrentDrawingForm.Top + 200)
            Try
                SwitchEventLog($"{GetCurrentMethod.Name} RequestPositionChange={pRequestPositionChange}")
                ConfirmationToMoveSwitch = False
                If FailState AndAlso
                    Not optEnableAllowSwitchControlMovementWhileInCodeFailure AndAlso
                    Not SwitchPointerMonitorMode AndAlso
                    Not gSimulationMode Then Return False
                If RouteLock Then
                    SwitchEventLog("SWITCH-ROUTE-LOCKED")
                    RouteLock = False
                    Return False
                End If
                If SelectionMode = ModeSelection.SummarySelectionRouting Then
                    SwitchEventLog("SWITCH-ROUTE-LOCKED BY SELECTION")
                    RouteLock = False
                    Return False
                End If
                'Avoiding change position of switches with join authorization requested or confirmed
                For Each oneSignal As Signal In myControlPoint.AssignedComponenets.OfType(Of Signal)
                    If oneSignal.JointControlAuthorizationEnabled AndAlso
                        oneSignal.JointControlAuthorizationOfficeMode AndAlso
                        (oneSignal.JointControlAuthorizationOfficeModeConfirmed OrElse
                        oneSignal.JointControlAuthorizationOfficeModeRequested) AndAlso
                        (oneSignal.JoinControlSignalRouteCollection.Contains(Me) OrElse
                        BrotherSwitches.Count > 0 AndAlso 'Check if exist a brother switch with same name
                        FindBrotherSwitchByName(oneSignal.JoinControlSignalRouteCollection)) Then
                        SwitchEventLog("JOIN AUTHORIZATION PENDING")
                        RouteLock = False
                        Return False
                    End If
                Next
                For Each brotherSwitch As Switch In BrotherSwitches
                    If brotherSwitch.SelectionMode = ModeSelection.SummarySelectionRouting Then
                        SwitchEventLog($"BROTHER SWITCH-ROUTE-LOCKED BY SELECTION {brotherSwitch.GUID}")
                        RouteLock = False
                        Return False
                    End If
                Next
                If HasOccupancy() Then
                    SwitchEventLog($"{GetCurrentMethod.Name} Switch with occupancy")
                    Return False
                End If
                If HasAuthority() Then
                    SwitchEventLog($"{GetCurrentMethod.Name} Switch with authority")
                    Return False
                End If
                If HasStoredRoute() Then
                    SwitchEventLog($"{GetCurrentMethod.Name} Switch with stored route")
                    Return False
                End If
                If pRequestPositionChange Then
                    If (AuthorityType <> "CTC" AndAlso
                        AuthorityType <> "OTHER") OrElse
                        (optEnableAllowSwitchControlMovementWhileInCodeFailure AndAlso
                        AuthorityType = "CTC" AndAlso
                        FailState) Then
                        If DeviceBlocked Then Return False
                        If Me.PowerSwitchInTWC And Not Me.RCPS Then
                            If Not IsNothing(Me.SwitchLockInd) AndAlso Me.SwitchLockInd.BitState = True Then
                                Return False
                            End If
                        End If
                        If Not Me.RCPS Then
                            Dim FormTop As Integer = Me.Top + 80
                            Dim FormLeft As Integer = TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + Me.Left + 20
                            If FormLeft + 420 > TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + Me.CurrentDrawingForm.Width Then
                                FormLeft = TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + Me.CurrentDrawingForm.Width - 420
                            End If
                            If FormTop + 240 > Me.CurrentDrawingForm.Height Then
                                FormTop = Me.CurrentDrawingForm.Height - 240
                            End If
                            If ControlledReverse And LeaveReversePermission And ProtectOpenSwitchEnabled Then
                                If optProtectOpenSwitchForCP Then
                                    Dim MessageList As New ArrayList
                                    Dim IssutoClearingPOS As String = ""
                                    Dim IssutoTypeClearingPOS As String = ""
                                    With MessageList
                                        .Add(Me.Name)
                                        .Add("will be restored to normal position. ")
                                        .Add("Is this correct? ")
                                        .Add(" ")
                                    End With
                                    Dim frmMessagebox As New frmMessageBox
                                    Try
                                        frmMessagebox = New frmMessageBox()
                                        frmMessagebox.Owner = Me.CurrentDrawingForm
                                        frmMessagebox.SetMessage(MessageList, "SWITCH POSITION INFORMATION", True, Me.CurrentDrawingForm.myLaunchPoint)
                                        frmMessagebox.ShowDialog()
                                        If frmMessagebox.FormFeedBack <> 6 Then
                                            Return False
                                        End If
                                    Catch ex As Exception
                                        GetErrorDetails(ex)
                                    Finally
                                        frmMessagebox.Close()
                                        frmMessagebox.Dispose()
                                        frmMessagebox = Nothing
                                        MessageList = New ArrayList
                                    End Try
                                    Dim frmMessageboxPOS As New frmMessageBox
                                    Try
                                        frmMessageboxPOS = New frmMessageBox()
                                        frmMessageboxPOS.Owner = Me.CurrentDrawingForm
                                        frmMessageboxPOS.TrainList = New ArrayList
                                        frmMessageboxPOS.ForemanList = New ArrayList
                                        Dim sfa As SFAuthority
                                        For Each sfa In Me.TrackAuthorities
                                            Dim Box20 As String
                                            For Each Box20 In sfa.Box12Switches
                                                If Box20 = Me.Name Then
                                                    If sfa.IssuedToTrain Then
                                                        frmMessageboxPOS.TrainList.Add(sfa.IssueTo)
                                                    End If
                                                End If
                                            Next
                                        Next
                                        frmMessageboxPOS.SetMessage_CPPOS(Me.CurrentDrawingForm.myLaunchPoint, "SWITCH POSITION INFORMATION")
                                        frmMessageboxPOS.ShowDialog()
                                        If frmMessageboxPOS.FormFeedBack = 1 Then
                                            Return False
                                        Else
                                            IssutoClearingPOS = frmMessageboxPOS.POSIssueTO
                                            IssutoTypeClearingPOS = frmMessageboxPOS.POSIssueTOType
                                        End If
                                    Catch ex As Exception
                                        GetErrorDetails(ex)
                                    Finally
                                        frmMessageboxPOS.Close()
                                        frmMessageboxPOS.Dispose()
                                        frmMessageboxPOS = Nothing
                                    End Try
                                ElseIf ProtectOpenSwitchEnabled And optDisablePOSSwitchDialog And Mid(Me.AuthorityType, 1, 3) = "TWC" Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                    If UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                        LeaveReversePermission = False
                                    Else
                                        ControlSwitch()
                                        ControlBrotherSwitches(Index, Me.ControlledReverse)
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                            Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                            MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                        End If
                                    End If
                                Else
                                    Dim frmSwitchDialog As frmSwitchDialog
                                    frmSwitchDialog = New frmSwitchDialog()
                                    frmSwitchDialog.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                                    frmSwitchDialog.SetUpProperties(Me.CurrentDrawingForm.myLaunchPoint, Name, "", Me)
                                    frmSwitchDialog.ShowDialog(Me.CurrentDrawingForm)
                                    If frmSwitchDialog.UserResponse Then
                                        ControlSwitch()
                                        ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                        If UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                            LeaveReversePermission = False
                                        Else
                                            ControlSwitch()
                                            ControlBrotherSwitches(Index, Me.ControlledReverse)
                                            Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                            MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                        End If
                                        If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                            If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                                Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                                MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                            End If
                                        End If
                                    End If
                                    SystemForms.MySystemForms.frmSwitchDialog = Nothing
                                    frmSwitchDialog.Dispose()
                                    frmSwitchDialog = Nothing
                                End If
                            ElseIf Not ProtectOpenSwitchEnabled And Not Me.SwitchPointerMonitorMode Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                            ElseIf Not ProtectOpenSwitchEnabled And ControlledReverse Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                            ElseIf ProtectOpenSwitchEnabled And optDisablePOSSwitchDialog And Mid(Me.AuthorityType, 1, 3) = "TWC" Then
                                Dim ErrorMessage As String
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                If Not IsNothing(Me.BrotherSwitches) Then
                                    If Me.BrotherSwitches.Count > 0 Then
                                        Dim bsw As Switch
                                        For Each bsw In Me.BrotherSwitches
                                            If Not bsw.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                                ControlSwitch()
                                                ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                                ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                                MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                            End If
                                        Next
                                    End If
                                End If
                                If Not UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                    ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                Else
                                End If
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                            ElseIf ProtectOpenSwitchEnabled Then
                                Dim frmMessageBox As frmMessageBox
                                Dim MessageList As New ArrayList()
                                frmMessageBox = New frmMessageBox()
                                frmMessageBox.Owner = Me.CurrentDrawingForm
                                MessageList.Add(Me.Name & " HAS BEEN SELECTED FOR REVERSE POSITION.")
                                MessageList.Add("SWITCH WILL BE PROTECTED UNTIL VERIFIED IN THE NORMAL POSITION.")
                                MessageList.Add("")
                                frmMessageBox.SetMessage(MessageList, "PROTECT OPEN SWITCH VERIFICATION", True, Me.CurrentDrawingForm.myLaunchPoint)
                                frmMessageBox.ShowDialog()
                                If frmMessageBox.FormFeedBack = 6 Then
                                    Dim ErrorMessage As String
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                    If Not IsNothing(Me.BrotherSwitches) Then
                                        If Me.BrotherSwitches.Count > 0 Then
                                            Dim bsw As Switch
                                            For Each bsw In Me.BrotherSwitches
                                                If Not bsw.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                                    ControlSwitch()
                                                    ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                                    ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                                End If
                                            Next
                                        End If
                                    End If
                                    If Not UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                        ControlSwitch()
                                        ControlBrotherSwitches(Index, Me.ControlledReverse, False, True)
                                        ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    Else
                                    End If
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                            ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                            MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                        End If
                                    End If
                                End If
                                frmMessageBox.Dispose()
                                frmMessageBox = Nothing
                            End If
                        ElseIf Me.RCPS Then
                            If Me.SwitchLockInd.BitState Then
                                Return False
                            End If
                            If Me.DirectionalRCPSLockOn Then
                                Return False
                            End If
                            If Me.TimeLockingOn Then
                                Return False
                            End If
                            If Me.Occuppied Then
                                Return False
                            End If
                            If Me.TrackAuthorities.Count > 0 Then
                                Dim sfa As SFAuthority
                                For Each sfa In Me.TrackAuthorities
                                    If sfa.AuthType1 <> "ItemFourToMOW" Then
                                        Return False
                                    End If
                                Next
                                Dim UserMessage As Integer
                                Dim ControlPointName As String = RetrieveName(Me.StationNumber)
                                Dim MessageString As String = "A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT." & vbCrLf & vbCrLf
                                MessageString = MessageString & "ARE YOU SURE IT IS SAFE TO PROCEED?"
                                UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT SWITCH VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                If UserMessage <> 6 Then
                                    Return False
                                End If
                            End If
                            Me.ForceProtectionOnRCPS = True
                            Me.SwitchIndStatus = SwIndStatus.NoIndication
                            ControlSwitch()
                            Return True
                        End If
                    Else 'CTC SWITCHES
                        Dim ConfirmedToMove As Boolean
                        If optEnbaleSwitchMoveOnNonTranversable = False And IsTrackNonTRANVERSABLE() = True Then Return False
                        If Me.SignalStatOnMe = SignalStatus.SignalAtStop Then
                            If (Me.Occuppied And Not Me.IgnoreOS) Or Me.DeviceBlocked Or Me.SwitchLockBit Then
                                SetCursor(curDefault)
                                'Me.ParentForm.Cursor = curDefault
                                gSendCTC = False
                                Return False
                            End If
                            If Not TrackBlockedOK() Then
                                'Me.ParentForm.Cursor = curDefault
                                SetCursor(curDefault)
                                gSendCTC = False
                                Return False
                            End If
                            If Me.TrackAndTime Or Me.FoulTime Or Me.TrackPermit Then
                                If optEnbaleSwitchMoveOnTNT = False Then Return False
                                Dim MessList As New ArrayList
                                Dim ControlPointName As String = RetrieveName(Me.StationNumber)
                                With MessList
                                    If optEnableARRCSpecificOptions Then
                                        .Add("SWITCH: " & Me.Name & " HAS TRACK AUTHORITY ISSUED OVER IT.")
                                        .Add("ARE YOU SURE IT IS SAFE TO PROCEED?")
                                    Else
                                        .Add("A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT.")
                                        .Add("ARE YOU SURE IT IS SAFE TO PROCEED?")
                                    End If
                                End With
                                Dim UserMessage As Integer
                                UserMessage = ShowCustomMessageBox(MessList, "PROTECT SWITCH VERIFICATION - AUTHORITY", True, LPoint, False, True, Me.CurrentDrawingForm)
                                If UserMessage <> 6 Then
                                    Return False
                                Else
                                    ConfirmedToMove = True
                                End If
                                If optPromptFormBOnSwitchReverse AndAlso Not DoDialogForWorkZoneApproval() Then
                                    Return False
                                End If
                            End If
                            If optPromptFormBOnSwitchReverse AndAlso TMDS.SystemUtilities.BulletinFunction.CheckIfSwitchIsInBulletinLimits(Me) Then
                                Dim MessList As New ArrayList
                                With MessList
                                    MessList.Add("SWITCH: " & Me.Name & " HAS A FORM B ISSUED OVER IT.")
                                    MessList.Add("ARE YOU SURE IT IS SAFE TO MOVE?")
                                End With
                                If ShowCustomMessageBox(MessList, "PROTECT SWITCH VERIFICATION - BULLETIN", True, LPoint, False, True, Me.CurrentDrawingForm) <> 6 Then
                                    Return False
                                End If
                            End If
                            If Me.ControlledReverse Then
                                If Not Me.ControlReverseTime < Now.AddSeconds(-3) Then
                                    Me.SwitchEventLog("SWITCH REVERSE CONTROL TIME UNDER 3 SECONDS THEN LAST CONTROL REVERSE. NO REVERSE CONTROL EXECUTED UNTIL TIME EXPIRES.")
                                    Exit Function
                                Else
                                    Me.ControlNormalTime = Now
                                End If
                            Else
                                If Not Me.ControlNormalTime < Now.AddSeconds(-3) Then
                                    Me.SwitchEventLog("SWITCH NORMAL CONTROL TIME UNDER 3 SECONDS THEN LAST CONTROL REVERSE. NO NORMAL CONTROL EXECUTED UNTIL TIME EXPIRES.")
                                    Exit Function
                                Else
                                    Me.ControlReverseTime = Now
                                End If
                            End If
                            If Me.CheckBrotherSwitchesToMove(ConfirmedToMove, ConfirmationToMoveSwitch) And Not pDoICS Then
                                Me.SwitchIndStatus = SwIndStatus.NoIndication
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    SwitchEventLog("Error in record data in database")
                                End If
                                Return True
                            ElseIf pDoICS Then
                                Me.SwitchIndStatus = SwIndStatus.NoIndication
                                ControlSwitch()
                            End If
                        Else
                            Return False
                        End If
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                LPoint = Nothing
            End Try
        End Function

        Private Function DoDialogForWorkZoneApproval() As Boolean
            Try
                If CheckForWorkZoneBulletin() Then Return True
                Dim ConfirmationOnSwitch As Boolean = False
                Dim frmMessageBox As frmMessageBox
                Dim MessageList As New ArrayList()
                Dim TopPoint As Int32
                If Me.Top + 300 < Me.CurrentDrawingForm.Height Then
                    TopPoint = Me.Top + 100
                Else
                    TopPoint = 150
                End If
                MessageList.Add(Me.Name & " SWITCH HAS WORK ZONE ISSUED OVER IT.")
                MessageList.Add("ARE YOU SURE IT IS SAFE TO PROCEED?")
                MessageList.Add("")
                frmMessageBox = New frmMessageBox()
                frmMessageBox.SetMessage(MessageList, "PROTECT SWITCH VERIFICATION", True, Me.CurrentDrawingForm.myLaunchPoint)
                frmMessageBox.ShowDialog()
                If frmMessageBox.FormFeedBack = 6 Then
                    ConfirmationOnSwitch = True
                End If
                frmMessageBox.Dispose()
                frmMessageBox = Nothing
                Return ConfirmationOnSwitch
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Function CheckForWorkZoneBulletin() As Boolean
            Try
                Dim b As GeneralBulletins.SubBulletin
                Dim comp As Object
                For Each comp In Me.OsComponentCollection
                    If comp.GetType.ToString = gTrackComponentName Then
                        Dim t As Track = CType(comp, Track)
                        If t.Bulletins.Count > 0 Then
                            For Each b In t.Bulletins
                                If b.mysType = "WORKZONE" Then
                                    Return False
                                End If
                            Next
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Overloads Function IsSwitch(ByVal e As System.Windows.Forms.MouseEventArgs, ByRef dbConnection As OleDbConnection, ByVal RequestPositionChange As Boolean) As Boolean
            Try
                ConfirmationToMoveSwitch = False

                If GraphicalPath.IsVisible(e.X, e.Y) Then

                    'Version 227
                    'if in failed state return false
                    If Me.FailState Then Return False

                    If RequestPositionChange Then
                        If Me.AuthorityType <> "CTC" Then
                            If Me.DeviceBlocked Then
                                Return False
                            End If

                            'If Not ActiveTrackWarrant Then
                            If ControlledReverse And LeaveReversePermission And ProtectOpenSwitchEnabled Then
                                'Dim LaunchPoint As New Point(e.X, e.Y + 80)
                                Dim LaunchPoint As Point = (Me.PointToScreen(New Point(e.X, e.Y + 80)))
                                Dim frmSwitchDialog As frmSwitchDialog
                                'SystemForms.MySystemForms.frmSwitchDialog = New frmSwitchDialog()
                                'SystemForms.MySystemForms.frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "")
                                'SystemForms.MySystemForms.frmSwitchDialog.ShowDialog(SystemForms.MySystemForms.frmDisplay)
                                frmSwitchDialog = New frmSwitchDialog()
                                frmSwitchDialog.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                                frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "")
                                frmSwitchDialog.ShowDialog(Me.CurrentDrawingForm)
                                If frmSwitchDialog.UserResponse Then
                                    'If SystemForms.MySystemForms.frmSwitchDialog.UserResponse Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    If UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                        LeaveReversePermission = False
                                    Else
                                        ControlSwitch()
                                        ControlBrotherSwitches(Index, Me.ControlledReverse)
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        'MsgBox(ErrorMessage, vbCritical)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                            Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                            MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                        End If
                                    End If
                                    'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                                End If
                                'SystemForms.MySystemForms.frmSwitchDialog.Dispose()
                                SystemForms.MySystemForms.frmSwitchDialog = Nothing
                                frmSwitchDialog.Dispose()
                                frmSwitchDialog = Nothing
                            ElseIf Not ProtectOpenSwitchEnabled Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                            ElseIf Not ProtectOpenSwitchEnabled And ControlledReverse Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                            Else
                                Dim UserMessage As Integer
                                Dim MessageString As String = Name & " HAS BEEN SELECTED FOR REVERSE POSITION." & vbCrLf & vbCrLf &
                                "SWITCH WILL BE PROTECTED UNTIL VERIFIED IN THE NORMAL POSITION."
                                UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT OPEN SWITCH VERIFICATION", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                                'UserMessage = MsgBox(MessageString, MsgBoxStyle.OKCancel, "PROTECT OPEN SWITCH VERIFICATION")
                                If UserMessage = 1 Then
                                    Dim ErrorMessage As String
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)

                                    If Not UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                        ControlSwitch()
                                        ControlBrotherSwitches(Index, Me.ControlledReverse)
                                        ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        'MsgBox(ErrorMessage, vbCritical)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    Else
                                        'IsSwitch = True
                                    End If
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                            ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                            MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                        End If
                                    End If
                                    'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                                End If

                            End If

                        Else

                            If (Me.Occuppied And Not Me.IgnoreOS) Or Me.DeviceBlocked Or Me.SwitchLockBit Then
                                SetCursor(curDefault)
                                'Me.ParentForm.Cursor = curDefault
                                gSendCTC = False
                                Return Nothing
                            End If

                            If Not TrackBlockedOK() Then
                                SetCursor(curDefault)
                                'Me.ParentForm.Cursor = curDefault
                                gSendCTC = False
                                Return False
                            End If

                            If Me.SignalStatOnMe = SignalStatus.SignalAtStop Then
                                If Me.TrackAndTime Then
                                    Dim UserMessage As Integer
                                    Dim ControlPointName As String = RetrieveName(Me.StationNumber)
                                    Dim MessageString As String = "A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT." & vbCrLf & vbCrLf
                                    MessageString = MessageString & "ARE YOU SURE IT IS SAFE TO PROCEED?"
                                    UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT SWITCH VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                    If UserMessage <> 6 Then
                                        Return False
                                        Exit Function
                                    End If
                                End If
                                If Me.CheckBrotherSwitchesToMove(Me.ControlledReverse, ConfirmationToMoveSwitch) Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    Return True
                                End If
                            End If
                        End If

                    End If
                    IsSwitch = True
                Else
                    IsSwitch = False
                End If
                Return Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Overloads Function TrySwitch(ByVal RequestPositionChange As Boolean) As Boolean
            Try
                'SyncLock GraphicalPath
                'If GraphicalPath.IsVisible(E.X, E.Y) Then
                ConfirmationToMoveSwitch = False
                'Version 227
                'if in failed state return false
                If Me.FailState Then Return False
                If Me.IndicateOnly Then Return False

                If RequestPositionChange Then
                    If Me.AuthorityType <> "CTC" Then
                        If Me.DeviceBlocked Then
                            Return False
                        End If
                        'If Not ActiveTrackWarrant Then
                        If ControlledReverse And LeaveReversePermission And ProtectOpenSwitchEnabled Then
                            'Dim LaunchPoint As New Point(e.X, e.Y + 80)
                            'Dim LaunchPoint As Point = (Me.PointToScreen(New Point(E.X, E.Y + 80)))
                            Dim frmSwitchDialog As frmSwitchDialog
                            'SystemForms.MySystemForms.frmSwitchDialog = New frmSwitchDialog()
                            'SystemForms.MySystemForms.frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "")
                            'SystemForms.MySystemForms.frmSwitchDialog.ShowDialog(SystemForms.MySystemForms.frmDisplay)
                            frmSwitchDialog = New frmSwitchDialog()
                            frmSwitchDialog.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI

                            'frmSwitchDialog.SetUpProperties(LaunchPoint, Name, "")
                            frmSwitchDialog.ShowDialog(Me.CurrentDrawingForm)
                            If frmSwitchDialog.UserResponse Then
                                'If SystemForms.MySystemForms.frmSwitchDialog.UserResponse Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                If UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    LeaveReversePermission = False
                                Else
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    'MsgBox(ErrorMessage, vbCritical)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If
                            'SystemForms.MySystemForms.frmSwitchDialog.Dispose()
                            SystemForms.MySystemForms.frmSwitchDialog = Nothing
                            frmSwitchDialog.Dispose()
                            frmSwitchDialog = Nothing
                        ElseIf Not ProtectOpenSwitchEnabled Then
                            ControlSwitch()
                            ControlBrotherSwitches(Index, Me.ControlledReverse)
                            If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                            End If
                            'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                        ElseIf Not ProtectOpenSwitchEnabled And ControlledReverse Then
                            ControlSwitch()
                            ControlBrotherSwitches(Index, Me.ControlledReverse)
                            If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                End If
                            End If
                            'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)

                        Else
                            Dim UserMessage As Integer
                            Dim MessageString As String = Name & " HAS BEEN SELECTED FOR REVERSE POSITION." & vbCrLf & vbCrLf &
                            "SWITCH WILL BE PROTECTED UNTIL VERIFIED IN THE NORMAL POSITION."
                            UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT OPEN SWITCH VERIFICATION", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)
                            'UserMessage = MsgBox(MessageString, MsgBoxStyle.OKCancel, "PROTECT OPEN SWITCH VERIFICATION")
                            If UserMessage = 1 Then
                                Dim ErrorMessage As String
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)

                                If Not UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    ControlSwitch()
                                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                                    ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                    MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    'MsgBox(ErrorMessage, vbCritical)
                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                Else
                                    'IsSwitch = True
                                End If
                                If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                    If Not UpdateSwitchPositionInDatabaseWithFeedBack(TMDS.GlobalDeclarations.gDatabaseConnectionStatic) Then
                                        ErrorMessage = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                        MessageBox.Show(Me.CurrentDrawingForm, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, ErrorMessage, SystemEventQueue)
                                    End If
                                End If
                                'UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If

                        End If

                    Else

                        Dim ConfirmedToMove As Boolean

                        If Me.SignalStatOnMe = SignalStatus.SignalAtStop Then
                            If Me.Occuppied Or Me.DeviceBlocked Or Me.SwitchLockBit Then
                                SetCursor(curDefault)
                                'Me.ParentForm.Cursor = curDefault
                                gSendCTC = False
                                Return False
                            End If

                            If Not TrackBlockedOK() Then
                                'Me.ParentForm.Cursor = curDefault
                                SetCursor(curDefault)
                                gSendCTC = False
                                Return False
                            End If

                            If Me.TrackAndTime Or Me.FoulTime Then
                                Dim UserMessage As Integer
                                Dim ControlPointName As String = RetrieveName(Me.StationNumber)
                                Dim MessageString As String = "A SWITCH @ " & ControlPointName.ToUpper & " HAS TRACK AUTHORITY ISSUED OVER IT." & vbCrLf & vbCrLf
                                MessageString = MessageString & "ARE YOU SURE IT IS SAFE TO PROCEED?"
                                UserMessage = MessageBox.Show(Me.CurrentDrawingForm, MessageString, "PROTECT SWITCH VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                If UserMessage <> 6 Then
                                    Return False
                                Else
                                    ConfirmedToMove = True
                                End If
                            End If

                            If Me.CheckBrotherSwitchesToMove(ConfirmedToMove, ConfirmationToMoveSwitch) Then
                                ControlSwitch()
                                ControlBrotherSwitches(Index, Me.ControlledReverse)
                                Return True
                            End If
                        Else
                            Return False
                        End If
                    End If
                End If
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Track Blocked OK
        ''' </summary>
        ''' <returns></returns>
        Public Function TrackBlockedOK() As Boolean
            Dim trk As Track = Nothing
            Dim linkToCheck As Int32
            Dim i As Integer
            Dim userMessage As Integer
            Dim controlPointName As String
            Dim messageList As New ArrayList
            Dim frmMessageBox As frmMessageBox
            Try
                For i = 1 To 4
                    If i = 1 Then
                        linkToCheck = Me.LeftLink
                    ElseIf i = 2 Then
                        linkToCheck = Me.LeftAltLink
                    ElseIf i = 3 Then
                        linkToCheck = Me.RightAltLink
                    ElseIf i = 4 Then
                        linkToCheck = Me.RightLink
                    End If
                    trk = Me.FindTrackItemFromGUID(linkToCheck)
                    If Not trk Is Nothing Then
                        If trk.DeviceBlocked Then
                            controlPointName = RetrieveName(Me.StationNumber)
                            messageList.Add(optGraphicalCompProtectSwitch1 & " '" & Me.Name & "' NO PÁTIO '" & controlPointName.ToUpper & "' " & optGraphicalCompProtectSwitch4)
                            messageList.Add("")
                            messageList.Add(optGraphicalCompProtectSwitch3)
                            messageList.Add("")
                            frmMessageBox = New frmMessageBox()
                            frmMessageBox.SetMessage(messageList, optLanguageMessageBoxCaptionDefault1, True, Me.CurrentDrawingForm.myLaunchPoint)
                            frmMessageBox.ShowDialog()
                            If frmMessageBox.FormFeedBack = 6 Then
                                ConfirmationToMoveSwitch = True
                                SwitchEventLog("CLICKED - YES TO TRACK BLOCKED")
                                Return True
                            Else
                                ConfirmationToMoveSwitch = False
                                SwitchEventLog("CLICKED - NO TO TRACK BLOCKED")
                                Return False
                            End If
                            frmMessageBox.Dispose()
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                trk = Nothing
                linkToCheck = Nothing
                i = Nothing
                userMessage = Nothing
                controlPointName = Nothing
                messageList = Nothing
                frmMessageBox = Nothing
            End Try
        End Function

        Public Function IsTrackNonTRANVERSABLE() As Boolean
            Dim LinkDirection As String = ""
            If Me.LeftAltLink > 0 Then
                LinkDirection = "L"
            ElseIf Me.RightAltLink > 0 Then
                LinkDirection = "R"
            End If
            If LinkDirection.Trim <> String.Empty Then
                Dim Track1 As Track = Nothing
                Dim Track2 As Track = Nothing
                Dim TrackAlt As Track = Nothing
                Try
                    Track1 = CType(Me.FindObjectTypeFromGUID(Me.RightLink), Track)
                    Track2 = CType(Me.FindObjectTypeFromGUID(Me.LeftLink), Track)
                    Select Case LinkDirection.Trim.ToUpper
                        Case "R"
                            TrackAlt = CType(Me.FindObjectTypeFromGUID(Me.RightAltLink), Track)
                        Case "L"
                            TrackAlt = CType(Me.FindObjectTypeFromGUID(Me.LeftAltLink), Track)
                    End Select
                    If Not Track1 Is Nothing And Not Track2 Is Nothing And Not TrackAlt Is Nothing Then
                        If Track1.NonTransverable = True And Track2.NonTransverable = True And TrackAlt.NonTransverable = True Then
                            Return True
                        End If
                    End If
                Catch ex As Exception
                    SystemException(ex.ToString)
                    'Return False
                Finally
                    Track1 = Nothing
                    Track2 = Nothing
                    TrackAlt = Nothing
                End Try
                Return False
            End If
        End Function

        Public Sub ProtectOpenSwitchCheck()
            Try
                If LeaveReversePermission And Not pControlled Then
                    ControlSwitch()
                    ControlBrotherSwitches(Index, Me.ControlledReverse)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub UpdateSwitchPositionInDatabase(ByRef pConnection As OleDbConnection)
            Dim dbConn As OleDbConnection = Nothing
            Dim query As String
            Dim s As String = ","
            Dim q As String = "'"
            Dim dbComm As OleDbCommand = Nothing
            Dim publishThisEvent As DoPublishSwitchEvent
            Dim logThisEvent As DoEventLogSwitch
            Try
                dbConn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                query = "UPDATE tblCompSwitches"
                query = query & " SET Controlled = " & CInt(ControlledReverse)
                query = query & " WHERE UID = " & GUID
                dbComm = New OleDbCommand(query, dbConn)
                dbComm.Connection.Open()
                dbComm.ExecuteNonQuery()
                dbConn.Close()
                publishThisEvent = New DoPublishSwitchEvent(AddressOf Me.PublishSwitchEventUpdate)
                If Me.ControlledReverse Then
                    publishThisEvent.BeginInvoke(EventTypeSw.CONTROLREVERSE, Nothing, Nothing)
                Else
                    publishThisEvent.BeginInvoke(EventTypeSw.CONTROLNORMAL, Nothing, Nothing)
                End If
                logThisEvent = New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                logThisEvent.BeginInvoke(EventTypeSwitch.CTL, Nothing, Nothing)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                If dbConn IsNot Nothing Then dbConn.Dispose()
                dbConn = Nothing
                If dbComm IsNot Nothing Then dbComm.Dispose()
                dbComm = Nothing
                query = Nothing
                s = Nothing
                q = Nothing
                publishThisEvent = Nothing
                logThisEvent = Nothing
            End Try
        End Sub

        Function UpdateSwitchPositionInDatabaseWithFeedBack(ByRef pConnection As OleDbConnection) As Boolean
            Dim dbConn As OleDbConnection = Nothing
            Dim dbComm As OleDbCommand = Nothing
            Dim query As String
            Dim s As String = ","
            Dim q As String = "'"
            Dim publishThisEvent As DoPublishSwitchEvent
            Dim logThisEvent As DoEventLogSwitch
            Try
                query = "UPDATE tblCompSwitches"
                query = query & " SET Controlled = " & CInt(ControlledReverse)
                query = query & " WHERE UID = " & GUID
                dbConn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                dbComm = New OleDbCommand(query, dbConn)
                dbComm.Connection.Open()
                dbComm.ExecuteNonQuery()
                dbConn.Close()
                publishThisEvent = New DoPublishSwitchEvent(AddressOf Me.PublishSwitchEventUpdate)
                If Me.ControlledReverse Then
                    publishThisEvent.BeginInvoke(EventTypeSw.CONTROLREVERSE, Nothing, Nothing)
                Else
                    publishThisEvent.BeginInvoke(EventTypeSw.CONTROLNORMAL, Nothing, Nothing)
                End If
                logThisEvent = New DoEventLogSwitch(AddressOf Me.LogTheSwitchEvent)
                logThisEvent.BeginInvoke(EventTypeSwitch.CTL, Nothing, Nothing)
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            Finally
                If dbConn IsNot Nothing Then dbConn.Dispose()
                dbConn = Nothing
                If dbComm IsNot Nothing Then dbComm.Dispose()
                dbComm = Nothing
                query = Nothing
                s = Nothing
                q = Nothing
                publishThisEvent = Nothing
                logThisEvent = Nothing
            End Try
        End Function

        Function UpdateProtectOpenSwitch(ByRef pConnection As OleDbConnection) As Boolean
            Dim dbConn As OleDbConnection = Nothing
            Dim sqlFields As String
            Dim sqlValues As String
            Dim setSwitchPositionCommand As String
            Dim s As String = ","
            Dim q As String = "'"
            Dim dbComm As OleDbCommand = Nothing
            Try
                If LeaveReversePermission Then
                    sqlFields = "(SwitchGUID, NAME, Subdivision, TERRITORYID)"
                    sqlValues = " VALUES ("
                    sqlValues = sqlValues & q & GUID & q & s & q & Name & q & s & q & Subdivision & q & s & q & TerritoryID & q
                    sqlValues = sqlValues & ")"
                    setSwitchPositionCommand = "INSERT Into " & gtblItem20Switches & sqlFields & sqlValues
                Else
                    setSwitchPositionCommand = "DELETE FROM " & gtblItem20Switches & " WHERE SwitchGUID = " & GUID & " AND TERRITORYID = " & TerritoryID
                End If
                dbConn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringAuthority)
                dbComm = New OleDbCommand(setSwitchPositionCommand, dbConn)
                dbComm.Connection.Open()
                dbComm.ExecuteNonQuery()
                dbConn.Close()
                UpdateProtectOpenSwitch = True
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                If dbConn IsNot Nothing Then dbConn.Dispose()
                dbConn = Nothing
                If dbComm IsNot Nothing Then dbComm.Dispose()
                dbComm = Nothing
                sqlFields = Nothing
                sqlValues = Nothing
                setSwitchPositionCommand = Nothing
                s = Nothing
                q = Nothing
            End Try
        End Function

        Public Overloads Sub InitSwitchControl(ByVal ReverseControl As Boolean)
            Try
                DebugEvent("DEBUG-InitSwitchControl:" & Me.GUID.ToString)
                If Me.SwitchPointerMonitorMode Then Me.UpdateStatus() : Exit Sub
                If Me.ProtectOpenSwitchEnabled And SwitchPointerMonitorMode Then
                    If Me.TrackAuthorities.Count > 0 Then
                        Exit Sub
                    End If
                End If
                Dim ControlStatus As String
                If Not ReverseControl Then
                    ControlledReverse = False
                    LeaveReversePermission = False
                    ControlStatus = "CONTROLLED-NORMAL"
                    Me.CurrentControlStatus = SwCtlStatus.NormalCtl
                    Me.LastControl = False
                Else
                    ControlledReverse = True
                    If Me.AuthorityType <> "CTC" Then
                        LeaveReversePermission = True
                    End If
                    ControlStatus = "CONTROLLED-REVERSE"
                    Me.CurrentControlStatus = SwCtlStatus.ReverseCtl
                    Me.LastControl = True
                End If
                'InitNonTraversableTracks()
                UpdateSwitchRoutesInTracks()
                Me.CheckForNonTranversableRoute()
                'New CTC Control added
                If Me.AuthorityType = "CTC" Then
                    If ControlledReverse Then
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = False
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = True
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = False
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = True
                        End If
                    Else
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = True
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = False
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = True
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = False
                        End If
                    End If
                    Dim posbit As BitPosition
                    For Each posbit In CtlIndBits
                        If Not Me.Ctl(0) Is Nothing Then
                            If Me.Ctl(0).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(0).BitState
                            End If
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            If Me.Ctl(3).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(3).BitState
                            End If
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            If Me.Ctl(1).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(1).BitState
                            End If
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            If Me.Ctl(4).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(4).BitState
                            End If
                        End If
                    Next
                    'Verison 230
                    'added some event logs
                    Dim EventMessage As String = "SWITCH INITIALIZED FROM INDICATION. (STATUS:" & ControlStatus & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                    Me.SwitchEventLog(EventMessage)
                    Me.UpdateStatus()
                    SendSwitchControlUpdate()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub ControlSwitch(ByVal ReverseControl As Boolean, ByVal StoreRoutedExecute As Boolean)
            Try
                Dim ControlStatus As String
                If ReverseControl = Me.ControlledReverse Then
                    Exit Sub
                End If

                If Not ReverseControl Then
                    ControlledReverse = False
                    LeaveReversePermission = False
                    ControlStatus = "CONTROLLED-NORMAL"
                    Me.CurrentControlStatus = SwCtlStatus.NormalCtl
                Else
                    ControlledReverse = True

                    If Me.AuthorityType <> "CTC" Then
                        LeaveReversePermission = True
                    End If
                    ControlStatus = "CONTROLLED-REVERSE"
                    Me.CurrentControlStatus = SwCtlStatus.ReverseCtl
                End If
                'InitNonTraversableTracks()
                Dim EventMessage As String = "ControlSwitch1-(STATUS:" & ControlStatus & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                Me.SwitchEventLog(EventMessage)

                UpdateSwitchRoutesInTracks()
                CheckForNonTranversableRoute()

                'New CTC Control added
                Dim SimulateTheSwitchMove As Boolean
                Dim SpringSwitchNoControl As Boolean
                If Me.SpringSwitch Then
                    If ControlledReverse Then
                        If IsNothing(Me.Ind(1)) Then
                            SimulateTheSwitchMove = True
                        End If
                    Else
                        If IsNothing(Me.Ind(0)) Then
                            SimulateTheSwitchMove = True
                        End If

                    End If

                    If IsNothing(Me.Ctl(0)) And IsNothing(Me.Ctl(1)) Then
                        SpringSwitchNoControl = True
                    End If

                End If
                Dim SwitchFailureState As Boolean = False
                If optEnableAllowSwitchControlMovementWhileInCodeFailure AndAlso Me.AuthorityType = "CTC" AndAlso Me.FailState Then SwitchFailureState = True


                'New CTC Control added
                'If Me.AuthorityType = "CTC" Then
                If (Me.AuthorityType = "CTC" Or Me.PowerSwitchInTWC Or Me.AuthorityType = "OTHER") AndAlso Not SpringSwitchNoControl AndAlso Not SwitchFailureState Then
                    If ControlledReverse Then
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = False
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = True
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = False
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = True
                        End If
                    Else
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = True
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = False
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = True
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = False
                        End If
                    End If

                    Dim posbit As BitPosition
                    For Each posbit In CtlIndBits
                        If Not Me.Ctl(0) Is Nothing Then
                            If Me.Ctl(0).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(0).BitState
                            End If
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            If Me.Ctl(3).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(3).BitState
                            End If
                        End If

                        If Not Me.Ctl(1) Is Nothing Then
                            If Me.Ctl(1).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(1).BitState
                            End If
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            If Me.Ctl(4).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(4).BitState
                            End If
                        End If

                    Next
                    Me.ReControlCount = 0
                    Me.ReControlStart = True
                    'Verison 230
                    'added some event logs


                    If Not Me.DoMonitorMode And Not Me.FailState And Not StoreRoutedExecute AndAlso Not (gAssistMode OrElse Me.IsShadowMode) Then
                        If LastControl <> ControlledReverse Then
                            Me.AddToSendCollection("ControlSwitch")
                            SwitchEventLog("ADD TO SEND COLLECTION FROM CONTROL SWITCH 3:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        End If
                        'Me.AddToSendCollection()
                    Else
                        If LastControl <> ControlledReverse Then
                            Me.FindCodeStationToSend()
                        End If
                    End If

                    If Me.SimulateMode Then
                        Me.DoSimulate = True
                    End If
                Else
                    SimulateSwitchMove()
                End If
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub ControlSwitch(ByVal ReverseControl As Boolean)
            Try
                Dim ControlStatus As String
                If Not ReverseControl Then
                    ControlledReverse = False
                    LeaveReversePermission = False
                    ControlStatus = "CONTROLLED-NORMAL"
                    Me.CurrentControlStatus = SwCtlStatus.NormalCtl
                Else
                    ControlledReverse = True
                    If Me.AuthorityType <> "CTC" Then
                        LeaveReversePermission = True
                    End If
                    ControlStatus = "CONTROLLED-REVERSE"
                    Me.CurrentControlStatus = SwCtlStatus.ReverseCtl
                End If
                Dim EventMessage As String = "ControlSwitch2-(STATUS:" & ControlStatus & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                Me.SwitchEventLog(EventMessage)

                'InitNonTraversableTracks()
                UpdateSwitchRoutesInTracks()
                CheckForNonTranversableRoute()

                'New CTC Control added
                If Me.AuthorityType = "CTC" Then
                    If ControlledReverse Then
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = False
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = True
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = False
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = True
                        End If
                    Else
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = True
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = False
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = True
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = False
                        End If
                    End If

                    Dim posbit As BitPosition
                    'For Each posbit In CtlIndBits
                    '    If Not Me.Ctl(0) Is Nothing Then
                    '        If Me.Ctl(0).GUID = posbit.GUID Then
                    '            posbit.BitState = Me.Ctl(0).BitState
                    '        End If
                    '    End If
                    '    If Not Me.Ctl(1) Is Nothing Then
                    '        If Me.Ctl(1).GUID = posbit.GUID Then
                    '            posbit.BitState = Me.Ctl(1).BitState
                    '        End If
                    '    End If
                    'Next
                    For Each posbit In CtlIndBits
                        If Not Me.Ctl(0) Is Nothing Then
                            If Me.Ctl(0).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(0).BitState
                            End If
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            If Me.Ctl(3).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(3).BitState
                            End If
                        End If

                        If Not Me.Ctl(1) Is Nothing Then
                            If Me.Ctl(1).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(1).BitState
                            End If
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            If Me.Ctl(4).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(4).BitState
                            End If
                        End If

                    Next
                    Me.ReControlStart = True
                    Me.ReControlCount = 0


                    If Not Me.DoMonitorMode And Not Me.FailState Then
                        If LastControl <> ControlledReverse Then
                            Me.AddToSendCollection("ControlSwitch2")
                            SwitchEventLog("ADD TO SEND COLLECTION FROM CONTROL SWITCH 2:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        End If
                        'Me.AddToSendCollection()
                    Else
                        If LastControl <> ControlledReverse Then
                            Me.FindCodeStationToSend()
                        End If
                    End If

                    If Me.SimulateMode Then
                        Me.DoSimulate = True
                    End If
                End If
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub ControlSwitch()
            Try
                Dim ControlStatus As String

                If ControlledReverse Then
                    ControlledReverse = False
                    LeaveReversePermission = False
                    ControlStatus = "CONTROLLED-NORMAL"
                    Me.CurrentControlStatus = SwCtlStatus.NormalCtl

                Else
                    ControlledReverse = True
                    If Me.AuthorityType <> "CTC" Then
                        LeaveReversePermission = True
                    End If
                    ControlStatus = "CONTROLLED-REVERSE"
                    Me.CurrentControlStatus = SwCtlStatus.ReverseCtl

                End If

                Dim EventMessage As String = "ControlSwitch3-(STATUS:" & ControlStatus & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                Me.SwitchEventLog(EventMessage)

                'InitNonTraversableTracks()
                UpdateSwitchRoutesInTracks()
                CheckForNonTranversableRoute()

                'New CTC Control added
                Dim SimulateTheSwitchMove As Boolean
                Dim SpringSwitchNoControl As Boolean
                If Me.SpringSwitch Then
                    If ControlledReverse Then
                        If IsNothing(Me.Ind(1)) Then
                            SimulateTheSwitchMove = True
                        End If
                    Else
                        If IsNothing(Me.Ind(0)) Then
                            SimulateTheSwitchMove = True
                        End If

                    End If

                    If IsNothing(Me.Ctl(0)) And IsNothing(Me.Ctl(1)) Then
                        SpringSwitchNoControl = True
                    End If
                End If

                Dim SwitchFailureState As Boolean = False
                If optEnableAllowSwitchControlMovementWhileInCodeFailure AndAlso Me.AuthorityType = "CTC" AndAlso Me.FailState Then SwitchFailureState = True

                If (Me.AuthorityType = "CTC" OrElse Me.PowerSwitchInTWC OrElse Me.AuthorityType = "OTHER") AndAlso Not SpringSwitchNoControl AndAlso Not SwitchFailureState Then
                    If ControlledReverse Then
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = False
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = True
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = False
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = True
                        End If
                    Else
                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = True
                        End If
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = False
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = True
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = False
                        End If
                    End If

                    Dim posbit As BitPosition
                    For Each posbit In CtlIndBits
                        If Not Me.Ctl(0) Is Nothing Then
                            If Me.Ctl(0).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(0).BitState
                            End If
                        End If
                        If Not Me.Ctl(3) Is Nothing Then
                            If Me.Ctl(3).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(3).BitState
                            End If
                        End If

                        If Not Me.Ctl(1) Is Nothing Then
                            If Me.Ctl(1).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(1).BitState
                            End If
                        End If
                        If Not Me.Ctl(4) Is Nothing Then
                            If Me.Ctl(4).GUID = posbit.GUID Then
                                posbit.BitState = Me.Ctl(4).BitState
                            End If
                        End If
                    Next


                    Me.ReControlStart = True
                    Me.ReControlCount = 0


                    If LastControl <> ControlledReverse Then
                        Me.AddToSendCollection("ControlSwitch3")
                        SwitchEventLog("ADD TO SEND COLLECTION FROM CONTROL SWITCH 1:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        gSendCTC = True
                        SetCursor(curSend)

                    End If

                    If Me.SimulateMode Then
                        Me.DoSimulate = True
                    End If

                    If SimulateTheSwitchMove Then
                        SimulateSwitchMove()
                    End If
                Else
                    SimulateSwitchMove()
                End If
                Me.UpdateStatus()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlBrotherSwitches(ByVal BrotherIndex As Integer, ByVal ControlReverse As Boolean, Optional ByVal StoreRouteExecute As Boolean = False, Optional ByVal UpdateDatabase As Boolean = False)
            Try
                Dim sw As Switch
                For Each sw In Me.BrotherSwitches
                    If sw.GUID <> Me.GUID Then
                        If sw.ControlledReverse <> ControlReverse AndAlso sw.TypeOfSwitch <> "DERAIL" Then
                            sw.SwitchIndStatus = SwIndStatus.NoIndication
                            sw.ControlSwitch(ControlReverse, StoreRouteExecute)
                            If UpdateDatabase Then
                                sw.UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If
                        ElseIf sw.TypeOfSwitch = "DERAIL" AndAlso sw.IndicateOnly Then
                            sw.ControlMatchIndicateOnlyDerailBrother(Me)
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlMatchIndicateOnlyDerail(ControlledReversed As Boolean)
            Try
                Select Case ControlledReversed
                    Case True
                        ControlledReverse = False
                        LeaveReversePermission = False
                        Me.CurrentControlStatus = SwCtlStatus.NormalCtl
                    Case Else
                        ControlledReverse = True
                        Me.CurrentControlStatus = SwCtlStatus.ReverseCtl
                End Select
                UpdateSwitchRoutesInTracks()
                CheckForNonTranversableRoute()
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ControlMatchIndicateOnlyDerailBrother(myBrotherSw As Switch)
            Try
                Select Case myBrotherSw.ControlledReverse
                    Case True
                        ControlledReverse = False
                        LeaveReversePermission = False
                        Me.CurrentControlStatus = SwCtlStatus.NormalCtl
                    Case Else
                        ControlledReverse = True
                        Me.CurrentControlStatus = SwCtlStatus.ReverseCtl
                End Select
                UpdateSwitchRoutesInTracks()
                CheckForNonTranversableRoute()
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub LockBrotherSwitches(ByVal LockSwitch As Boolean)
            Try
                Dim sw As Switch
                For Each sw In Me.BrotherSwitches
                    'If Not sw Is Me Then
                    sw.RouteLock = LockSwitch
                    'End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForNonTranversableRoute()
            Dim GrayOutConditionExists As Boolean

            Try
                ''Get all the track components link to the switch
                Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                Dim LeftAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftAltLink)
                Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                Dim RightAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightAltLink)

                Me.NonTransverable = False
                'Me.Refresh()
                If Me.LeftAltLink <> 0 Then        'LeftHand Switches
                    If LeftTrack.NonTransverable And (LeftAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If LeftAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or LeftAltTrack.NonTransverable) Then GrayOutConditionExists = True
                    Dim NextRightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightTrack.RightLink)
                    If Not NextRightTrack Is Nothing Then
                        If NextRightTrack.NonTransverable And NextRightTrack.Name = RightTrack.Name Then
                            GrayOutConditionExists = True
                        End If
                    End If
                ElseIf Me.RightAltLink <> 0 Then    'righthand switches
                    If LeftTrack.NonTransverable And (RightAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or RightAltTrack.NonTransverable) Then GrayOutConditionExists = True
                    Dim NextLeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftTrack.LeftLink)

                    If Not NextLeftTrack Is Nothing Then
                        If NextLeftTrack.NonTransverable And NextLeftTrack.Name = LeftTrack.Name Then
                            GrayOutConditionExists = True
                        End If
                    End If
                End If

                Dim NextComp As Track
                Dim LoopControl As Integer
                If GrayOutConditionExists Then
                    Me.NonTransverable = True
                    If Me.LeftAltLink <> 0 Then
                        If Not LeftAltTrack.NonTransverable Then
                            LeftAltTrack.NonTransverable = True
                            LeftAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftAltTrack.LeftLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = LeftAltTrack.StationNumber And NextComp.Name = LeftAltTrack.Name Then
                                NextComp.NonTransverable = True
                                NextComp.UpdateStatus()
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop
                    ElseIf Me.RightAltLink <> 0 Then
                        LoopControl = 0
                        If Not RightAltTrack.NonTransverable Then
                            RightAltTrack.NonTransverable = True
                            RightAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightAltTrack.RightLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = RightAltTrack.StationNumber And NextComp.Name = RightAltTrack.Name Then
                                NextComp.NonTransverable = True
                                NextComp.UpdateStatus()
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop

                    End If

                    LoopControl = 0
                    If Not RightTrack.NonTransverable Then
                        RightTrack.NonTransverable = True
                        RightTrack.UpdateStatus()
                    End If

                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightTrack.RightLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = RightTrack.StationNumber And NextComp.Name = RightTrack.Name Then
                            NextComp.NonTransverable = True
                            NextComp.UpdateStatus()
                            NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop

                    LoopControl = 0
                    If Not LeftTrack.NonTransverable Then
                        LeftTrack.NonTransverable = True
                        LeftTrack.UpdateStatus()
                    End If

                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftTrack.LeftLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = LeftTrack.StationNumber And NextComp.Name = LeftTrack.Name Then
                            NextComp.NonTransverable = True
                            NextComp.UpdateStatus()
                            NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop
                Else
                    UpdateSwitchRoutesInTracks()
                End If

                Dim comp As Object
                If Not Me.OsComponentCollection Is Nothing Then
                    For Each comp In Me.OsComponentCollection
                        If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            Dim sw As Switch = CType(comp, Switch)
                            If sw.GUID <> Me.GUID Then
                                If (sw.Name = Me.Name) And (sw.StationNumber = Me.StationNumber) Then
                                    'do nothing
                                Else
                                    Dim thisSw As Switch = CType(sw, Switch)
                                    If Not thisSw.PuzzleSwitch Then
                                        DebugEvent("DEBUG-CheckForNonTranversableRoute3:" & Me.GUID.ToString)
                                        sw.CheckForNonTranversableThisSwitchOnly()
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
                'SwitchEvent("CheckForNonTranversableRoute-Complete:" & Me.GUID.ToString)
                'If Me.InvokeRequired Then
                '    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                'Else
                '    Me.Refresh()
                'End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForNonTranversableTracks()
            Dim GrayOutConditionExists As Boolean
            Me.SwitchEventLog("CheckForNonTranversableTracks:" & Me.GUID.ToString)

            Try
                ''Get all the track components link to the switch
                Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                Dim LeftAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftAltLink)
                Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                Dim RightAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightAltLink)

                Me.NonTransverable = False
                If Me.LeftAltLink <> 0 Then        'LeftHand Switches
                    If LeftTrack.NonTransverable And (LeftAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If LeftAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or LeftAltTrack.NonTransverable) Then GrayOutConditionExists = True
                ElseIf Me.RightAltLink <> 0 Then    'righthand switches
                    If LeftTrack.NonTransverable And (RightAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or RightAltTrack.NonTransverable) Then GrayOutConditionExists = True
                End If

                Dim NextComp As Track
                Dim LoopControl As Integer
                If GrayOutConditionExists Then
                    Me.NonTransverable = True
                    If Me.LeftAltLink <> 0 Then
                        If Not LeftAltTrack.NonTransverable Then
                            LeftAltTrack.NonTransverable = True
                            LeftAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftAltTrack.LeftLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = LeftAltTrack.StationNumber And NextComp.Name = LeftAltTrack.Name Then
                                NextComp.NonTransverable = True
                                NextComp.UpdateStatus()
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop
                    ElseIf Me.RightAltLink <> 0 Then
                        LoopControl = 0
                        If Not RightAltTrack.NonTransverable Then
                            RightAltTrack.NonTransverable = True
                            RightAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightAltTrack.RightLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = RightAltTrack.StationNumber And NextComp.Name = RightAltTrack.Name Then
                                NextComp.NonTransverable = True
                                NextComp.UpdateStatus()
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop

                    End If

                    LoopControl = 0
                    If Not RightTrack.NonTransverable Then
                        RightTrack.NonTransverable = True
                        RightTrack.UpdateStatus()
                    End If

                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightTrack.RightLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = RightTrack.StationNumber And NextComp.Name = RightTrack.Name Then
                            NextComp.NonTransverable = True
                            NextComp.UpdateStatus()
                            NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop

                    LoopControl = 0
                    If Not LeftTrack.NonTransverable Then
                        LeftTrack.NonTransverable = True
                        LeftTrack.UpdateStatus()
                    End If

                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftTrack.LeftLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = LeftTrack.StationNumber And NextComp.Name = LeftTrack.Name Then
                            NextComp.NonTransverable = True
                            NextComp.UpdateStatus()
                            NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop
                Else
                    UpdateSwitchRoutesInTracks()
                End If

                Dim sw As Object
                For Each sw In Me.OsComponentCollection
                    If sw.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        If Not sw Is Me And Me.TypeOfSwitch <> "DERAIL" Then
                            If (sw.Name = Me.Name) And (sw.StationNumber = Me.StationNumber) Then
                                'do nothing
                            Else
                                sw.CheckForNonTranversableThisSwitchOnly()
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForNonTranversableThisSwitchOnly()
            Dim GrayOutConditionExists As Boolean
            Dim LastComp As Object = Nothing
            Try
                UpdateSwitchRoutesInTracks()
                ''Get all the track components link to the switch
                Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                Dim LeftAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftAltLink)
                Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                Dim RightAltTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightAltLink)

                'Me.NonTransverable = False
                'Me.Refresh()
                If Me.LeftAltLink <> 0 Then        'LeftHand Switches
                    'If LeftTrack.NonTransverable And (LeftAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    'If LeftAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or LeftAltTrack.NonTransverable) Then GrayOutConditionExists = True
                ElseIf Me.RightAltLink <> 0 Then    'righthand switches
                    If LeftTrack.NonTransverable And (RightAltTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    'If RightAltTrack.NonTransverable And (LeftTrack.NonTransverable Or RightTrack.NonTransverable) Then GrayOutConditionExists = True
                    'If RightTrack.NonTransverable And (LeftTrack.NonTransverable Or RightAltTrack.NonTransverable) Then GrayOutConditionExists = True
                End If

                Dim NextComp As Track
                Dim LoopControl As Integer
                If GrayOutConditionExists Then
                    Me.NonTransverable = True
                    If Me.LeftAltLink <> 0 Then
                        If Not LeftAltTrack.NonTransverable Then
                            LeftAltTrack.NonTransverable = True
                            LeftAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftAltTrack.LeftLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = LeftAltTrack.StationNumber And NextComp.Name = LeftAltTrack.Name Then
                                If Not NextComp.NonTransverable Then
                                    NextComp.NonTransverable = True
                                    NextComp.UpdateStatus()
                                    LastComp = NextComp
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                                    If IsNothing(NextComp) Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompRRXINGFromGUID(LastComp.LeftLink)
                                    End If
                                End If
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop
                    ElseIf Me.RightAltLink <> 0 Then
                        LoopControl = 0
                        If Not RightAltTrack.NonTransverable Then
                            RightAltTrack.NonTransverable = True
                            RightAltTrack.UpdateStatus()
                        End If

                        NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightAltTrack.RightLink)
                        Do
                            If NextComp Is Nothing Then Exit Do
                            If NextComp.StationNumber = RightAltTrack.StationNumber And NextComp.Name = RightAltTrack.Name Then
                                If Not NextComp.NonTransverable Then
                                    NextComp.NonTransverable = True
                                    NextComp.UpdateStatus()
                                    LastComp = NextComp
                                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                                    If IsNothing(NextComp) Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompRRXINGFromGUID(LastComp.RightLink)
                                    End If
                                End If
                            Else
                                Exit Do
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 10 Then Exit Do
                        Loop

                    End If

                    'right track
                    LoopControl = 0
                    If Not RightTrack.NonTransverable Then
                        RightTrack.NonTransverable = True
                        RightTrack.UpdateStatus()
                    End If

                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(RightTrack.RightLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = RightTrack.StationNumber And NextComp.Name = RightTrack.Name Then
                            If Not NextComp.NonTransverable Then
                                NextComp.NonTransverable = True
                                NextComp.UpdateStatus()
                                LastComp = NextComp
                                NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.RightLink)
                                If IsNothing(NextComp) Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompRRXINGFromGUID(LastComp.RightLink)
                                End If
                            End If
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop

                    'left track
                    LoopControl = 0
                    If Not LeftTrack.NonTransverable Then
                        LeftTrack.NonTransverable = True
                        LeftTrack.UpdateStatus()
                    End If
                    NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(LeftTrack.LeftLink)
                    Do
                        If NextComp Is Nothing Then Exit Do
                        If NextComp.StationNumber = LeftTrack.StationNumber And NextComp.Name = LeftTrack.Name Then
                            NextComp.NonTransverable = True
                            NextComp.UpdateStatus()
                            LastComp = NextComp
                            NextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextComp.LeftLink)
                            If IsNothing(NextComp) Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompRRXINGFromGUID(LastComp.LeftLink)
                            End If
                        Else
                            Exit Do
                        End If
                        LoopControl = LoopControl + 1
                        If LoopControl > 10 Then Exit Do
                    Loop
                Else
                    DebugEvent("DEBUG-CheckForNonTranversableThisSwitchOnly3:" & Me.GUID.ToString)
                    UpdateSwitchRoutesInTracks()
                End If
                DebugEvent("DEBUG-CheckForNonTranversableThisSwitchOnly-COMPLETE:" & Me.GUID.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForOtherNonTransverableRoutes(ByVal EntryTrack As Track)
            Me.SwitchEventLog("CheckForOtherNonTransverableRoutes:" & Me.GUID.ToString)

            Try
                If Me.RightAltLink <> 0 Then
                    If EntryTrack.GUID = Me.LeftLink And EntryTrack.NonTransverable Then
                        SetRoutesToNonTransversable(True)
                        Me.NonTransverable = True
                    Else
                        SetRoutesToNonTransversable(False)
                        Me.NonTransverable = False
                    End If
                Else
                    If EntryTrack.GUID = Me.RightLink And EntryTrack.NonTransverable Then
                        SetRoutesToNonTransversable(True)
                        Me.NonTransverable = True
                    Else
                        SetRoutesToNonTransversable(False)
                        Me.NonTransverable = False
                    End If
                End If
                Me.UpdateStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SetRoutesToNonTransversable(ByVal SetAllToNonTransversable As Boolean)
            DebugEvent("DEBUG-SetRoutesToNonTransversable:" & Me.GUID.ToString)

            Try
                Dim TrackIndex As Integer
                Dim RightSwitch, LeftSwitch As Integer

                If SetAllToNonTransversable Then
                    For TrackIndex = 0 To 5
                        If ReverseRoute(TrackIndex) <> 0 Then
                            TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = True
                            TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                            'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).ReverseRoute(TrackIndex)).RegionPath)
                        End If
                        If NormalRoute(TrackIndex) <> 0 Then
                            TrackComponent(NormalRoute(TrackIndex)).NonTransverable = True
                            TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                            'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).NormalRoute(TrackIndex)).RegionPath)
                        End If
                    Next TrackIndex
                Else
                    If ControlledReverse Then
                        For TrackIndex = 0 To 5
                            If ReverseRoute(TrackIndex) <> 0 Then
                                TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = False
                                TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                                'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).ReverseRoute(TrackIndex)).RegionPath)
                            End If
                            If NormalRoute(TrackIndex) <> 0 Then
                                TrackComponent(NormalRoute(TrackIndex)).NonTransverable = True
                                TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                                'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).NormalRoute(TrackIndex)).RegionPath)
                            End If

                        Next TrackIndex
                    Else
                        For TrackIndex = 0 To 5
                            If ReverseRoute(TrackIndex) <> 0 Then
                                TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = True
                                TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                                'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).ReverseRoute(TrackIndex)).RegionPath)
                            End If
                            If NormalRoute(TrackIndex) <> 0 Then
                                'See if this track is link between two switches, if so, then check the reverse status of both switches
                                'before showing the route normal
                                RightSwitch = FindSwitchIndexFromGUID(TrackComponent(NormalRoute(TrackIndex)).RightLink)
                                LeftSwitch = FindSwitchIndexFromGUID(TrackComponent(NormalRoute(TrackIndex)).LeftLink)
                                'if either end of the track is not hook to a switch, go ahead and show the route normal
                                If RightSwitch = 0 Or LeftSwitch = 0 Then
                                    TrackComponent(NormalRoute(TrackIndex)).NonTransverable = False
                                    TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                                    'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).NormalRoute(TrackIndex)).RegionPath)
                                Else
                                    If (SwitchComponent(RightSwitch).ControlledReverse Or SwitchComponent(LeftSwitch).ControlledReverse) Then
                                        TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = True
                                        TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                                        'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).ReverseRoute(TrackIndex)).RegionPath)
                                    Else
                                        TrackComponent(NormalRoute(TrackIndex)).NonTransverable = False
                                        TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                                        'SystemForms.MySystemForms.frmDisplay().Invalidate(TrackComponent(SwitchComponent(i).NormalRoute(TrackIndex)).RegionPath)
                                    End If
                                End If
                            End If
                        Next TrackIndex
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub UpdateSwitchRoutesInTracks()
            Dim TrackIndex As Integer
            Dim RightSwitch, LeftSwitch As Integer

            'SwitchEvent("UpdateSwitchRoutesInTracks:" & Me.GUID.ToString)

            Try
                Me.NonTransverable = False
                'Me.Refresh
                If ControlledReverse Then
                    If Me.LeftAltLink <> 0 Then
                        Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                        If Not RightTrack Is Nothing Then
                            RightTrack.NonTransverable = False
                            RightTrack.UpdateStatus()
                            Me.NonTransverable = False
                            'Me.Refresh()
                        End If
                    ElseIf Me.RightAltLink <> 0 Then
                        Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                        If Not LeftTrack Is Nothing Then
                            LeftTrack.NonTransverable = False
                            LeftTrack.UpdateStatus()
                            'ADDED CODE FOR WHEN THE SWITCH IS REVERSE AND A ROUTE EXISTS BEYOND THE FIRST NON-PIVTOL TRACK OF A SWITCH 
                            Dim nextcomp As Object = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(LeftTrack.LeftLink)
                            Dim LeftCount As Integer
                            Do
                                If IsNothing(nextcomp) Then
                                    Exit Do
                                End If
                                'Added to address MRS364. Do loop was looping till it reached its count which caused to add other switches track comp to move :(
                                If Me.OsComponentCollection IsNot Nothing AndAlso Me.OsComponentCollection.Contains(nextcomp) Then
                                Else
                                    Exit Do
                                End If
                                If nextcomp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    nextcomp.NonTransverable = False
                                    nextcomp.UpdateStatus()
                                    nextcomp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(nextcomp.LeftLink)
                                ElseIf nextcomp.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then
                                    nextcomp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(nextcomp.LeftLink)
                                Else
                                    Exit Do
                                End If
                                LeftCount = LeftCount + 1
                                If LeftCount > 4 Then
                                    Exit Do
                                End If
                            Loop
                            'ADDED CODE FOR WHEN THE SWITCH IS REVERSE AND A ROUTE EXISTS BEYOND THE FIRST NON-PIVTOL TRACK OF A SWITCH 
                            Me.NonTransverable = False
                            'Me.Refresh()
                        End If
                    ElseIf Me.TypeOfSwitch = "DERAIL" Then
                        Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                        Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                        If Not IsNothing(LeftTrack) Then
                            LeftTrack.NonTransverable = True
                            LeftTrack.UpdateStatus()
                        End If
                        If Not IsNothing(RightTrack) Then
                            RightTrack.NonTransverable = True
                            RightTrack.UpdateStatus()
                        End If

                    End If

                    For TrackIndex = 0 To 5
                        If ReverseRoute(TrackIndex) <> 0 Then
                            TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = False
                            TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                            If Not Me.PuzzleSwitch Then
                                TrackComponent(ReverseRoute(TrackIndex)).NonTranversableSearch(Me, False)
                            End If
                            'DebugEvent("SWCONTROL1:" & TrackComponent(ReverseRoute(TrackIndex)).Name & "-" & TrackComponent(ReverseRoute(TrackIndex)).GUID & " NON:" & TrackComponent(ReverseRoute(TrackIndex)).NonTransverable.ToString)
                        End If
                        If NormalRoute(TrackIndex) <> 0 Then
                            TrackComponent(NormalRoute(TrackIndex)).NonTransverable = True
                            'TrackComponent(NormalRoute(TrackIndex)).BringToFront()
                            TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                            If Not Me.PuzzleSwitch Then
                                TrackComponent(NormalRoute(TrackIndex)).NonTranversableSearch(Me, True)
                            End If
                            'DebugEvent("SWCONTROL2:" & TrackComponent(NormalRoute(TrackIndex)).Name & "-" & TrackComponent(NormalRoute(TrackIndex)).GUID & " NON:" & TrackComponent(NormalRoute(TrackIndex)).NonTransverable.ToString)
                        End If

                    Next TrackIndex
                Else

                    If Me.LeftAltLink <> 0 Then
                        Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                        If Not LeftTrack Is Nothing Then
                            LeftTrack.NonTransverable = False
                            LeftTrack.UpdateStatus()
                            Me.NonTransverable = False
                            'Me.Refresh()
                        End If
                    ElseIf Me.RightAltLink <> 0 Then
                        Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                        If Not RightTrack Is Nothing Then
                            RightTrack.NonTransverable = False
                            RightTrack.UpdateStatus()
                            Me.NonTransverable = False
                            'Me.Refresh()
                        End If
                    ElseIf Me.TypeOfSwitch = "DERAIL" Then
                        Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                        Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                        If Not IsNothing(LeftTrack) Then
                            LeftTrack.NonTransverable = False
                            LeftTrack.UpdateStatus()
                        End If
                        If Not IsNothing(RightTrack) Then
                            RightTrack.NonTransverable = False
                            RightTrack.UpdateStatus()
                        End If
                    End If

                    For TrackIndex = 0 To 5
                        If ReverseRoute(TrackIndex) <> 0 Then
                            TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = True
                            TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                            If Not Me.PuzzleSwitch Then
                                TrackComponent(ReverseRoute(TrackIndex)).NonTranversableSearch(Me, True)
                            End If
                        End If
                        If NormalRoute(TrackIndex) <> 0 Then
                            'See if this track is link between two switches, if so, then check the reverse status of both switches
                            'before showing the route normal
                            RightSwitch = FindSwitchIndexFromGUID(TrackComponent(NormalRoute(TrackIndex)).RightLink)
                            LeftSwitch = FindSwitchIndexFromGUID(TrackComponent(NormalRoute(TrackIndex)).LeftLink)
                            'if either end of the track is not hook to a switch, go ahead and show the route normal
                            If RightSwitch = 0 Or LeftSwitch = 0 Then
                                TrackComponent(NormalRoute(TrackIndex)).NonTransverable = False
                                TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                                TrackComponent(NormalRoute(TrackIndex)).NonTranversableSearch(Me, False)
                            Else
                                If (SwitchComponent(RightSwitch).ControlledReverse Or SwitchComponent(LeftSwitch).ControlledReverse) Then
                                    TrackComponent(ReverseRoute(TrackIndex)).NonTransverable = True
                                    TrackComponent(ReverseRoute(TrackIndex)).UpdateStatus()
                                    If Not Me.PuzzleSwitch Then
                                        TrackComponent(ReverseRoute(TrackIndex)).NonTranversableSearch(Me, True)
                                    End If
                                Else
                                    TrackComponent(NormalRoute(TrackIndex)).NonTransverable = False
                                    TrackComponent(NormalRoute(TrackIndex)).UpdateStatus()
                                    If Not Me.PuzzleSwitch Then
                                        TrackComponent(NormalRoute(TrackIndex)).NonTranversableSearch(Me, False)
                                    End If
                                End If
                            End If
                        End If
                    Next TrackIndex
                End If
                'If Me.InvokeRequired Then
                '    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                'Else
                '    Me.Refresh()
                'End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Sub AssignReverseRoute(Optional ByVal FlipOperation As Boolean = False)
            Try
                Dim TrackCounter, NextTrackIndex, LastTrackIndex, NumOfTracks As Integer
                Dim ReverseSwRoute As Integer
                Dim TypeOfTurnOut As String = String.Empty
                Dim TrackSearchingComplete As Boolean

                If LeftAltLink <> 0 Then
                    ReverseSwRoute = LeftAltLink
                    TypeOfTurnOut = "L"
                ElseIf RightAltLink <> 0 Then
                    ReverseSwRoute = RightAltLink
                    TypeOfTurnOut = "R"
                End If
                NextTrackIndex = FindTrackIndexFromGUID(ReverseSwRoute)
                If Not FlipOperation Then
                    ReverseRoute(NumOfTracks) = NextTrackIndex
                    ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                Else
                    NormalRoute(NumOfTracks) = NextTrackIndex
                    NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                End If

                Do While Not TrackSearchingComplete
                    LastTrackIndex = NextTrackIndex
                    If TypeOfTurnOut = "L" Then
                        NextTrackIndex = FindTrackIndexFromGUID(TrackComponent(LastTrackIndex).LeftLink)
                        If NextTrackIndex = 0 Then
                            Dim NextComp As Object = FindCompFromGUID(TrackComponent(LastTrackIndex).LeftLink)
                            If Not IsNothing(NextComp) Then
                                Select Case NextComp.GetType.ToString
                                    Case "TMDS.TrackOtherComponents.RRXING"
                                        Dim ExitGUID As Integer = 0
                                        ExitGUID = GetRRXingExitGUID(NextComp, LastTrackIndex)
                                        If (optEnableBRCSpecificChecks Or optApplyConfigurationForKCT) AndAlso ExitGUID > 0 Then ' Or optEnableARRCSpecificOptions
                                            NextTrackIndex = FindTrackIndexFromGUID(ExitGUID)
                                        Else
                                            NextTrackIndex = FindTrackIndexFromGUID(NextComp.LeftLink)
                                        End If
                                        'NextTrackIndex = FindTrackIndexFromGUID(NextComp.LeftLink)
                                End Select
                            End If
                        End If

                    ElseIf TypeOfTurnOut = "R" Then
                        NextTrackIndex = FindTrackIndexFromGUID(TrackComponent(LastTrackIndex).RightLink)
                        If NextTrackIndex = 0 Then
                            Dim NextComp As Object = FindCompFromGUID(TrackComponent(LastTrackIndex).RightLink)
                            If Not IsNothing(NextComp) Then
                                Select Case NextComp.GetType.ToString
                                    Case "TMDS.TrackOtherComponents.RRXING"
                                        Dim ExitGUID As Integer = 0
                                        ExitGUID = GetRRXingExitGUID(NextComp, LastTrackIndex)
                                        If (optEnableBRCSpecificChecks Or optApplyConfigurationForKCT) AndAlso ExitGUID > 0 Then ' Or optEnableARRCSpecificOptions
                                            NextTrackIndex = FindTrackIndexFromGUID(ExitGUID)
                                        Else
                                            NextTrackIndex = FindTrackIndexFromGUID(NextComp.RightLink)
                                        End If
                                        'NextTrackIndex = FindTrackIndexFromGUID(NextComp.RightLink)
                                End Select
                            End If
                        End If

                    End If
                    If NextTrackIndex = 0 Then Exit Do
                    Dim OkToProcess As Boolean = True
                    If optEnableBRCSpecificChecks Then ' Or optEnableARRCSpecificOptions 
                        OkToProcess = OkToAddToRouteCollection(TrackComponent(NextTrackIndex), TrackComponent(LastTrackIndex))
                    Else
                        If TrackComponent(NextTrackIndex).LeftLink <> 0 And
                        (TrackComponent(NextTrackIndex).StationNumber = TrackComponent(LastTrackIndex).StationNumber) And
                        (TrackComponent(NextTrackIndex).Name = TrackComponent(LastTrackIndex).Name) Then
                        Else
                            OkToProcess = False
                        End If
                    End If
                    If OkToProcess Then
                        NumOfTracks = NumOfTracks + 1
                        If Not FlipOperation Then
                            ReverseRoute(NumOfTracks) = NextTrackIndex
                            ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                        Else
                            NormalRoute(NumOfTracks) = NextTrackIndex
                            NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                        End If
                    Else
                        Exit Do
                    End If


                    'If TrackComponent(NextTrackIndex).LeftLink <> 0 And _
                    '(TrackComponent(NextTrackIndex).StationNumber = TrackComponent(LastTrackIndex).StationNumber) And _
                    '(TrackComponent(NextTrackIndex).Name = TrackComponent(LastTrackIndex).Name) Then

                    '    NumOfTracks = NumOfTracks + 1
                    '    If Not FlipOperation Then
                    '        ReverseRoute(NumOfTracks) = NextTrackIndex
                    '        ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                    '    Else
                    '        NormalRoute(NumOfTracks) = NextTrackIndex
                    '        NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                    '    End If
                    'Else
                    '    Exit Do
                    'End If
                    TrackCounter = TrackCounter + 1
                    If TrackCounter = 5 Then
                        Exit Do
                    End If
                Loop

                Dim trk As Track
                If Not FlipOperation Then
                    If Not ReverseRouteCollection Is Nothing Then
                        For Each trk In ReverseRouteCollection
                            If Not trk Is Nothing Then
                                trk.TrainIDPlace = False
                                trk.OSReporting = False
                            End If
                        Next
                    End If
                Else
                    If Not NormalRouteCollection Is Nothing Then
                        For Each trk In NormalRouteCollection
                            If Not trk Is Nothing Then
                                trk.TrainIDPlace = False
                                trk.OSReporting = False
                            End If
                        Next
                    End If

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Sub AssignNormalRoute(Optional ByVal FlipOperation As Boolean = False)
            Try
                Dim TrackCounter, NextTrackIndex, LastTrackIndex, NumOfTracks As Integer
                Dim NormalSwRoute As Integer
                Dim TypeOfTurnOut As String = String.Empty
                Dim TrackSearchingComplete As Boolean

                If LeftAltLink <> 0 Then
                    NormalSwRoute = LeftLink
                    TypeOfTurnOut = "L"
                ElseIf RightAltLink <> 0 Then
                    NormalSwRoute = RightLink
                    TypeOfTurnOut = "R"
                End If


                NextTrackIndex = FindTrackIndexFromGUID(NormalSwRoute)
                If FlipOperation Then
                    ReverseRoute(NumOfTracks) = NextTrackIndex
                    ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                Else
                    NormalRoute(NumOfTracks) = NextTrackIndex
                    NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                End If

                'NormalRoute(NumOfTracks) = NextTrackIndex
                'NormalRouteCollection.Add(TrackComponent(NextTrackIndex))

                Do While Not TrackSearchingComplete

                    LastTrackIndex = NextTrackIndex
                    If TypeOfTurnOut = "L" Then
                        NextTrackIndex = FindTrackIndexFromGUID(TrackComponent(LastTrackIndex).LeftLink)
                        If NextTrackIndex = 0 Then
                            Dim NextComp As Object = FindCompFromGUID(TrackComponent(LastTrackIndex).LeftLink)
                            If NextComp IsNot Nothing Then
                                Select Case NextComp.GetType.ToString
                                    Case "TMDS.TrackOtherComponents.RRXING"
                                        Dim ExitGUID As Integer = 0
                                        ExitGUID = GetRRXingExitGUID(NextComp, LastTrackIndex)
                                        If (optEnableBRCSpecificChecks Or optApplyConfigurationForKCT Or optEnableMetroLinkSpecificOptions) AndAlso ExitGUID > 0 Then ' Or optEnableARRCSpecificOptions
                                            NextTrackIndex = FindTrackIndexFromGUID(ExitGUID)
                                        Else
                                            NextTrackIndex = FindTrackIndexFromGUID(NextComp.LeftLink)
                                        End If
                                        'NextTrackIndex = FindTrackIndexFromGUID(NextComp.LeftLink)
                                End Select
                            End If
                        End If
                    ElseIf TypeOfTurnOut = "R" Then
                        NextTrackIndex = FindTrackIndexFromGUID(TrackComponent(LastTrackIndex).RightLink)
                        If NextTrackIndex = 0 Then
                            Dim NextComp As Object = FindCompFromGUID(TrackComponent(LastTrackIndex).RightLink)
                            If NextComp IsNot Nothing Then
                                Select Case NextComp.GetType.ToString
                                    Case "TMDS.TrackOtherComponents.RRXING"
                                        Dim ExitGUID As Integer = 0
                                        ExitGUID = GetRRXingExitGUID(NextComp, LastTrackIndex)
                                        If (optEnableBRCSpecificChecks Or optApplyConfigurationForKCT Or optEnableMetroLinkSpecificOptions) AndAlso ExitGUID > 0 Then ' Or optEnableARRCSpecificOptions
                                            NextTrackIndex = FindTrackIndexFromGUID(ExitGUID)
                                        Else
                                            NextTrackIndex = FindTrackIndexFromGUID(NextComp.RightLink)
                                        End If
                                        'NextTrackIndex = FindTrackIndexFromGUID(NextComp.RightLink)
                                End Select
                            End If
                        End If
                    End If
                    'End If
                    If NextTrackIndex = 0 Then Exit Do
                    Dim OkToProcess As Boolean = True

                    If (optEnableBRCSpecificChecks Or optApplyConfigurationForKCT Or optEnableMetroLinkSpecificOptions) Then ' Or optEnableARRCSpecificOptions
                        OkToProcess = OkToAddToRouteCollection(TrackComponent(NextTrackIndex), TrackComponent(LastTrackIndex))
                    Else
                        If TrackComponent(NextTrackIndex).LeftLink <> 0 And
                   (TrackComponent(NextTrackIndex).StationNumber = TrackComponent(LastTrackIndex).StationNumber) And
                   (TrackComponent(NextTrackIndex).Name = TrackComponent(LastTrackIndex).Name) Then
                        Else
                            OkToProcess = False
                        End If
                    End If


                    If OkToProcess Then
                        NumOfTracks = NumOfTracks + 1
                        If FlipOperation Then
                            ReverseRoute(NumOfTracks) = NextTrackIndex
                            ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                        Else
                            NormalRoute(NumOfTracks) = NextTrackIndex
                            NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                        End If
                    Else
                        Exit Do
                    End If

                    'If TrackComponent(NextTrackIndex).LeftLink <> 0 And _
                    '(TrackComponent(NextTrackIndex).StationNumber = TrackComponent(LastTrackIndex).StationNumber) And _
                    '(TrackComponent(NextTrackIndex).Name = TrackComponent(LastTrackIndex).Name) Then

                    '    NumOfTracks = NumOfTracks + 1
                    '    If FlipOperation Then
                    '        ReverseRoute(NumOfTracks) = NextTrackIndex
                    '        ReverseRouteCollection.Add(TrackComponent(NextTrackIndex))
                    '    Else
                    '        NormalRoute(NumOfTracks) = NextTrackIndex
                    '        NormalRouteCollection.Add(TrackComponent(NextTrackIndex))
                    '    End If

                    '    'NormalRoute(NumOfTracks) = NextTrackIndex
                    '    'NormalRouteCollection.Add(TrackComponent(NextTrackIndex))

                    'Else
                    '    Exit Do
                    'End If
                    TrackCounter = TrackCounter + 1
                    If TrackCounter = 5 Then
                        Exit Do
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString & " SWITCH GUID:" & Me.GUID.ToString)
            End Try

        End Sub

        Private Function FindSwitchIndexFromGUID(ByVal UniqueID As Int32) As Integer
            Try
                'This function finds the current trackcomponent index from the global unique ID
                Dim I As Integer
                For I = 1 To UBound(SwitchComponent)
                    If SwitchComponent(I).GUID = UniqueID Then
                        FindSwitchIndexFromGUID = I
                        Exit Function
                    End If
                Next I
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Function

        Private Function FindTrackItemFromGUID(ByVal UniqueID As Int32) As Track
            Try
                'This function finds the current trackcomponent index from the global unique ID
                Dim I As Integer
                For I = 1 To UBound(TrackComponent)
                    If TrackComponent(I).GUID = UniqueID Then
                        FindTrackItemFromGUID = TrackComponent(I)
                        Exit Function
                    End If
                Next I
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try

        End Function

        Private Function FindObjectTypeFromGUID(ByVal UniqueID As Int32) As Object
            Try
                'This function finds the current trackcomponent index from the global unique ID
                Dim I As Integer
                For I = 1 To UBound(TrackComponent)
                    If TrackComponent(I).GUID = UniqueID Then
                        FindObjectTypeFromGUID = TrackComponent(I)
                        Exit Function
                    End If
                Next I

                For I = 1 To UBound(SwitchComponent)
                    If SwitchComponent(I).GUID = UniqueID Then
                        FindObjectTypeFromGUID = SwitchComponent(I)
                        Exit Function
                    End If
                Next I

                For I = 1 To UBound(SignalComponent)
                    If SignalComponent(I).GUID = UniqueID Then
                        FindObjectTypeFromGUID = SignalComponent(I)
                        Exit Function
                    End If
                Next I
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindAllTracksThatConnect() As ArrayList
            Try
                Dim I As Integer
                Dim TrackToCompare As Track = Me.FindObjectTypeFromGUID(Me.RightLink)
                Dim CompToCheck As Object = Nothing
                Dim FoundLastLikeTrack As Boolean
                Dim LoopControl As Integer
                Dim TempArrayList As New ArrayList()

                For I = 1 To 3

                    Select Case I
                        Case 1
                            CompToCheck = Me.FindObjectTypeFromGUID(Me.RightLink)
                        Case 2
                            CompToCheck = Me.FindObjectTypeFromGUID(Me.LeftLink)
                        Case 3
                            If Me.RightAltLink <> 0 Then
                                CompToCheck = Me.FindObjectTypeFromGUID(Me.RightAltLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                CompToCheck = Me.FindObjectTypeFromGUID(Me.LeftAltLink)
                            Else
                                CompToCheck = Nothing
                            End If
                    End Select
                    Do While Not FoundLastLikeTrack

                        If CompToCheck Is Nothing Then Exit Do

                        If CompToCheck.GetType.ToString <> "TMDS.TrackComponents.Track" Then Exit Do
                        If TrackToCompare.TrackName = CompToCheck.TrackName _
                        And CompToCheck.StationNumber = TrackToCompare.StationNumber _
                        And TrackToCompare.Name = CompToCheck.Name Then

                            If Not TempArrayList.Contains(CompToCheck) Then
                                TempArrayList.Add(CompToCheck)
                            End If
                            Select Case I
                                Case 1
                                    CompToCheck = Me.FindObjectTypeFromGUID(CompToCheck.RightLink)
                                Case 2
                                    CompToCheck = Me.FindObjectTypeFromGUID(CompToCheck.LeftLink)
                                Case 3
                                    If Me.RightAltLink <> 0 Then
                                        CompToCheck = Me.FindObjectTypeFromGUID(CompToCheck.RightLink)
                                    ElseIf Me.LeftAltLink <> 0 Then
                                        CompToCheck = Me.FindObjectTypeFromGUID(CompToCheck.LeftLink)
                                    Else
                                        CompToCheck = Nothing
                                    End If
                            End Select

                        Else
                            Exit Do
                        End If

                        If LoopControl >= 100 Then Exit Do
                        LoopControl = LoopControl + 1
                    Loop
                Next
                Return TempArrayList

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindRouteThroughSwitchIgnoreSwitchPosition(ByVal TravelDirection As String) As Track
            Dim trk As Object = Nothing
            Try
                If TravelDirection Is Nothing Then Return Nothing
                If TravelDirection = "RIGHT" Then
                    If Not Me.FlipOperations Then
                        If Not Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.RightLink)
                        ElseIf Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                'Return FindTrackItemFromGUID(Me.RightAltLink)
                                If optIgnoreSwitchPostionOnTNTIssue Then
                                    trk = FindTrackItemFromGUID(Me.RightAltLink)
                                    If trk IsNot Nothing Then
                                        For i As Integer = 0 To 20
                                            If trk.GetType.ToString = gTrackComponentName Then
                                                If Me.OsComponentCollection.Contains(trk) Then
                                                    trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.RightLink)
                                                Else
                                                    Exit For
                                                End If
                                            Else
                                                trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.RightLink)
                                            End If
                                        Next

                                        If optEnableTrackSidingBit Then
                                            'NEW SIDING LOGIC CODE
                                            If trk.EnableNonControlledSiding Or trk.EnableControlledSiding Then
                                                Return FindTrackItemFromGUID(Me.RightLink)
                                            Else
                                                Return trk
                                            End If
                                        Else
                                            'OLD LEGACY SIDING CODE
                                            If trk.TrackName.Trim.ToUpper.Contains("SIDING") Then
                                                Return FindTrackItemFromGUID(Me.RightLink)
                                            Else
                                                Return trk
                                            End If
                                        End If

                                    End If
                                Else
                                    Return FindTrackItemFromGUID(Me.RightAltLink)
                                End If
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        If Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.RightLink)
                        ElseIf Not Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                '    Return FindTrackItemFromGUID(Me.RightAltLink)
                                '*************************************************************
                                If optIgnoreSwitchPostionOnTNTIssue Then
                                    trk = FindTrackItemFromGUID(Me.RightAltLink)
                                    If trk IsNot Nothing Then
                                        For i As Integer = 0 To 20
                                            If trk.GetType.ToString = gTrackComponentName Then
                                                If Me.OsComponentCollection.Contains(trk) Then
                                                    trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.RightLink)
                                                Else
                                                    Exit For
                                                End If
                                            Else
                                                trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.RightLink)
                                            End If
                                        Next

                                        If optEnableTrackSidingBit Then
                                            'NEW SIDING LOGIC CODE
                                            If trk.EnableNonControlledSiding Or trk.EnableControlledSiding Then
                                                Return FindTrackItemFromGUID(Me.RightLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.RightAltLink)
                                            End If
                                        Else
                                            'OLD LEGACY SIDING CODE
                                            If trk.TrackName.Trim.ToUpper.Contains("SIDING") Then
                                                Return FindTrackItemFromGUID(Me.RightLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.RightAltLink)
                                            End If
                                        End If

                                    End If
                                Else
                                    Return FindTrackItemFromGUID(Me.RightAltLink)
                                End If
                                '*************************************************************
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If

                    End If

                ElseIf TravelDirection = "LEFT" Then
                    If Not Me.FlipOperations Then
                        If Not Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.LeftLink)
                        ElseIf Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                'Return FindTrackItemFromGUID(Me.LeftAltLink)
                                '*************************************************************
                                If optIgnoreSwitchPostionOnTNTIssue Then
                                    trk = FindTrackItemFromGUID(Me.LeftAltLink)
                                    If trk IsNot Nothing Then
                                        For i As Integer = 0 To 20
                                            If trk.GetType.ToString = gTrackComponentName Then
                                                If Me.OsComponentCollection.Contains(trk) Then
                                                    trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.LeftLink)
                                                Else
                                                    Exit For
                                                End If
                                            Else
                                                trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.LeftLink)
                                            End If
                                        Next
                                        If optEnableTrackSidingBit Then
                                            'NEW SIDING LOGIC CODE
                                            If trk.EnableNonControlledSiding Or trk.EnableControlledSiding Then
                                                Return FindTrackItemFromGUID(Me.LeftLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                                            End If
                                        Else
                                            'OLD LEGACY SIDING CODE
                                            If trk.TrackName.Trim.ToUpper.Contains("SIDING") Then
                                                Return FindTrackItemFromGUID(Me.LeftLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                                            End If
                                        End If
                                    End If
                                Else
                                    Return FindTrackItemFromGUID(Me.LeftAltLink)
                                End If
                                '*************************************************************
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        If Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.LeftLink)
                        ElseIf Not Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                'Return FindTrackItemFromGUID(Me.LeftAltLink)
                                '*************************************************************
                                If optIgnoreSwitchPostionOnTNTIssue Then
                                    trk = FindTrackItemFromGUID(Me.LeftAltLink)
                                    If trk IsNot Nothing Then
                                        For i As Integer = 0 To 20
                                            If trk.GetType.ToString = gTrackComponentName Then
                                                If Me.OsComponentCollection.Contains(trk) Then
                                                    trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.LeftLink)
                                                Else
                                                    Exit For
                                                End If
                                            Else
                                                trk = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(trk.LeftLink)
                                            End If
                                        Next

                                        If optEnableTrackSidingBit Then
                                            'NEW SIDING LOGIC CODE
                                            If trk.EnableNonControlledSiding Or trk.EnableControlledSiding Then
                                                Return FindTrackItemFromGUID(Me.LeftLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                                            End If
                                        Else
                                            'OLD LEGACY SIDING CODE
                                            If trk.TrackName.Trim.ToUpper.Contains("SIDING") Then
                                                Return FindTrackItemFromGUID(Me.LeftLink)
                                            Else
                                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                                            End If
                                        End If

                                    End If
                                Else
                                    Return FindTrackItemFromGUID(Me.LeftAltLink)
                                End If
                                '*************************************************************
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindRouteThroughSwitch(ByVal TravelDirection As String) As Track

            Try
                If TravelDirection Is Nothing Then Return Nothing
                If optIgnoreSwitchPostionOnTNTIssue Then
                    If TravelDirection = "RIGHT" Then
                        Return FindTrackItemFromGUID(Me.RightLink)
                    Else
                        Return FindTrackItemFromGUID(Me.LeftLink)
                    End If
                    'Return FindRouteThroughSwitchIgnoreSwitchPosition(TravelDirection)
                End If

                If optEnableARRCSpecificOptions AndAlso (gCreateAuthorityTypeMOW = "BIDIRECTIONAL" OrElse gCreateAuthorityTypeTrain = "BIDIRECTIONAL") AndAlso Me.AuthorityType = "TWC" Then
                    If TravelDirection = "RIGHT" Then
                        Return FindTrackItemFromGUID(Me.RightLink)
                    Else
                        Return FindTrackItemFromGUID(Me.LeftLink)
                    End If
                End If

                If TravelDirection = "RIGHT" Then
                    If Not Me.FlipOperations Then
                        If Not Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.RightLink)
                        ElseIf Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightAltLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        If Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.RightLink)
                        ElseIf Not Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightAltLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.RightLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If

                    End If

                ElseIf TravelDirection = "LEFT" Then
                    If Not Me.FlipOperations Then
                        If Not Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.LeftLink)
                        ElseIf Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    Else
                        If Me.ControlledReverse Then
                            Return FindTrackItemFromGUID(Me.LeftLink)
                        ElseIf Not Me.ControlledReverse Then
                            If Me.RightAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftLink)
                            ElseIf Me.LeftAltLink <> 0 Then
                                Return FindTrackItemFromGUID(Me.LeftAltLink)
                            Else
                                Return Nothing
                            End If
                        Else
                            Return Nothing
                        End If
                    End If
                Else
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindSidingTrackThroughReverseLeg(ByVal ExitTrack As Track, ByVal EnterTrack As Track, ByRef CompList As ArrayList) As Boolean
            Try
                Dim NextComp As Object
                Dim theList As New ArrayList
                Dim LoopControl As Integer

                Dim IsEntryTrackSiding As Boolean = False
                Dim IsExitTrackSiding As Boolean = False

                If optEnableTrackSidingBit Then
                    If ExitTrack.EnableNonControlledSiding Or ExitTrack.EnableControlledSiding Then IsEntryTrackSiding = True
                    If EnterTrack.EnableNonControlledSiding Or EnterTrack.EnableControlledSiding Then IsExitTrackSiding = True
                End If

                If Me.RightAltLink <> 0 Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.RightAltLink)
                    Do
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)

                            If optEnableTrackSidingBit Then
                                'NEW SIDING LOGIC CODE
                                If (trk.GUID = ExitTrack.GUID And IsExitTrackSiding) Then  'Or (trk.GUID = EnterTrack.GUID And EnterTrack.TrackName = "SIDING")
                                    Exit Do
                                ElseIf (trk.GUID = ExitTrack.GUID And IsExitTrackSiding) Then
                                    Exit Do
                                End If
                            Else
                                'OLD LEGACY SIDING CODE
                                If (trk.GUID = ExitTrack.GUID And ExitTrack.TrackName = "SIDING") Then  'Or (trk.GUID = EnterTrack.GUID And EnterTrack.TrackName = "SIDING")
                                    Exit Do
                                ElseIf (trk.GUID = ExitTrack.GUID And ExitTrack.TrackName.Trim.ToUpper.Contains("SIDING")) Then
                                    Exit Do
                                End If
                            End If

                            theList.Add(NextComp)
                        ElseIf NextComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            theList.Add(NextComp)
                        End If
                        LoopControl += 1
                        If LoopControl > 10 Then
                            Return False
                        End If
                    Loop
                ElseIf Me.LeftAltLink <> 0 Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.LeftAltLink)
                    Do
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)

                            If optEnableTrackSidingBit Then
                                'NEW SIDING LOGIC CODE
                                If (trk.GUID = ExitTrack.GUID And IsExitTrackSiding) Then
                                    'If trk.GUID = SidingTrack.GUID Then
                                    Exit Do
                                ElseIf (trk.GUID = ExitTrack.GUID And IsExitTrackSiding) Then
                                    Exit Do
                                End If
                            Else
                                'OLD LEGACY SIDING CODE
                                If (trk.GUID = ExitTrack.GUID And ExitTrack.TrackName = "SIDING") Then 'Or (trk.GUID = EnterTrack.GUID And EnterTrack.TrackName = "SIDING")
                                    'If trk.GUID = SidingTrack.GUID Then
                                    Exit Do
                                ElseIf (trk.GUID = ExitTrack.GUID And ExitTrack.TrackName.Trim.ToUpper.Contains("SIDING")) Then
                                    Exit Do
                                End If
                            End If

                            theList.Add(NextComp)
                        ElseIf NextComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            theList.Add(NextComp)
                        End If
                        LoopControl += 1
                        If LoopControl > 10 Then
                            Return False
                        End If
                    Loop

                End If





                Dim comp As Object
                For Each comp In theList
                    If Not CompList.Contains(comp) Then
                        CompList.Add(comp)
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function RetrieveName(ByVal GUID As Int32) As String
            Try
                Dim obj As Object
                For Each obj In TMDS.GlobalDeclarations.gComponentsCollection
                    If GUID = obj.GUID Then
                        Return obj.Name
                    End If
                Next

                Return "UNKNOWN"
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Switch event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Public Sub SwitchEventLog(message As String)
            Try
                SystemEvent($"{NameOf(Switch)} {Name}:{GUID}:{message}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Debug event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub DebugEvent(message As String)
            If gDebug Then SystemEvent(message)
        End Sub

        Public Function FindExitGUID(ByVal EntryGUID As Int32) As Integer
            Dim GUIDtoFind As Integer
            Try
                If Me.ControlledReverse Then
                    If EntryGUID = Me.LeftLink Then
                        If Me.RightAltLink > 0 Then
                            GUIDtoFind = Me.RightAltLink
                        ElseIf Me.LeftAltLink > 0 Then
                            GUIDtoFind = Me.LeftAltLink
                        End If
                    Else
                        If Me.RightAltLink > 0 Then
                            GUIDtoFind = Me.RightAltLink
                        ElseIf Me.LeftAltLink > 0 Then
                            GUIDtoFind = Me.LeftAltLink
                        End If
                    End If
                Else
                    If EntryGUID = Me.LeftLink Then
                        GUIDtoFind = Me.RightLink
                    Else
                        GUIDtoFind = Me.LeftLink
                    End If
                End If



            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
            Return GUIDtoFind
        End Function

        Private Function OkToAddToRouteCollection(NextTrack As Object, LastTrack As Object) As Boolean
            Try
                If NextTrack Is Nothing Then Return False
                If LastTrack Is Nothing Then Return False
                If NextTrack.GetType.ToString <> gTrackComponentName Then Return False
                If LastTrack.GetType.ToString <> gTrackComponentName Then Return False
                If NextTrack IsNot Nothing AndAlso LastTrack IsNot Nothing AndAlso NextTrack.LeftLink <> 0 Then
                    If NextTrack.StationNumber = LastTrack.StationNumber AndAlso NextTrack.Name = LastTrack.Name Then
                        Return True
                    ElseIf NextTrack.StationNumber = LastTrack.StationNumber AndAlso (NextTrack.Ind(0) Is LastTrack.Ind(0) AndAlso NextTrack.Ind(1) Is LastTrack.Ind(1)) Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
                Return True
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Function

        Private Function GetRRXingExitGUID(Comp As Object, LastTrackIndex As Integer) As Integer
            Dim RR As RRXING = Nothing
            Dim objExitGUID As Object = Nothing
            Try
                If Comp Is Nothing Then Return 0
                'If LastTrackIndex<1 then Return 0
                If Comp.GetType.ToString = gRRXINGComponentName Then
                    RR = Comp
                Else
                    Return 0
                End If
                Dim ExitGUID As Integer = 0
                objExitGUID = RR.FindExitGUID(TrackComponent(LastTrackIndex).GUID)
                If objExitGUID IsNot Nothing AndAlso Integer.TryParse(objExitGUID, ExitGUID) Then
                End If
                Return ExitGUID
            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                RR = Nothing
                objExitGUID = Nothing
            End Try
            Return 0
        End Function

#End Region

#Region "EVENT LOGGING"

        Public Enum EventTypeSwitch
            IND
            CTL
            BLK
            LCK
            FAIL
            OOC
        End Enum

        Private Delegate Sub DoEventLogSwitch(ByVal EventType As EventTypeSwitch)

        ''' <summary>
        ''' Log Switch Event
        ''' </summary>
        ''' <param name="eventType">Event Type</param>
        Public Sub LogTheSwitchEvent(eventType As EventTypeSwitch)
            Try
                Dim curUser As String = $"{TMDS.GlobalDeclarations.gCurrentUser.LastName},{TMDS.GlobalDeclarations.gCurrentUser.FirstName}"
                Dim curWrkstn As Integer = Convert.ToInt32(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim curTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(TerritoryID)
                Dim logString As String = "INSERT INTO tblSystemEvents "
                Dim sqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim sqlValues As String
                Dim c As String = ", "
                Dim q As String = "'"
                Dim eventTypeString As String = String.Empty
                sqlValues = $" VALUES ({q}{Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{q}{c}{TerritoryID}{c}{curWrkstn}{c}{q}{curUser}{q}{c}"
                sqlValues = $"{sqlValues}{q}{curTerr.Dbversion}{q}{c}{q}SWITCH{q}{c}{q}{GUID.ToString}{q}{c}"
                Select Case eventType
                    Case EventTypeSwitch.CTL
                        If ControlledReverse Then
                            sqlValues = $"{sqlValues}{q}CTL-REVERSE{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "CTL-REVERSE"
                        Else
                            sqlValues = $"{sqlValues}{q}CTL-NORMAL{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "CTL-NORMAL"
                        End If
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSwitch.IND
                        Select Case SwitchIndStatus
                            Case SwIndStatus.NoIndication
                                sqlValues = $"{sqlValues}{q}NO-CORRESPONDENCE{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "NO-CORRESPONDENCE"
                            Case SwIndStatus.NormalInd
                                sqlValues = $"{sqlValues}{q}IND-NORMAL{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-NORMAL"
                            Case SwIndStatus.ReverseInd
                                sqlValues = $"{sqlValues}{q}IND-REVERSE{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-REVERSE"
                            Case Else
                                sqlValues = $"{sqlValues}{q}UNKNOWN{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "UNKNOWN"
                        End Select
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSwitch.LCK
                        If SwitchLockBit Then
                            sqlValues = $"{sqlValues}{q}IND-LOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "IND-LOCKED"
                        Else
                            sqlValues = $"{sqlValues}{q}IND-UNLOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "IND-UNLOCKED"
                        End If
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSwitch.BLK
                        If BlockAbsolute Then
                            sqlValues = $"{sqlValues}{q}SWITCH-BLOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "SWITCH-BLOCKED"
                        Else
                            sqlValues = $"{sqlValues}{q}SWITCH-UNBLOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "SWITCH-UNBLOCKED"
                        End If
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSwitch.FAIL
                        If ControlledReverse Then
                            sqlValues = $"{sqlValues}{q}CTL-REVERSE-FAILED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "CTL-REVERSE-FAILED"
                        Else
                            sqlValues = $"{sqlValues}{q}CTL-NORMAL-FAILED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "CTL-NORMAL-FAILED"
                        End If
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSwitch.OOC
                        If ControlledReverse Then
                            sqlValues = $"{sqlValues}{q}OUT-OF-CORRESPONDENCE-REVERSE{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "OUT-OF-CORRESPONDENCE-REVERSE"
                        Else
                            sqlValues = $"{sqlValues}{q}OUT-OF-CORRESPONDENCE-NORMAL{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "OUT-OF-CORRESPONDENCE-NORMAL"
                        End If
                        sqlValues = $"{sqlValues})"
                End Select
                TMDS.NetworkMessages.CadMessages.SendSwitchEventToWabtecOne(Me, eventTypeString)
                SyncLock TMDS.GlobalDeclarations.gSystemEventLogQueue
                    TMDS.GlobalDeclarations.gSystemEventLogQueue.Enqueue(logString & sqlFields & sqlValues)
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "STATUS UPDATES"

        Public Enum EventTypeSw
            CONTROLNORMAL
            CONTROLREVERSE
        End Enum

        Public Delegate Sub DoPublishSwitchEvent(ByVal EventType As EventTypeSw)

        Public Sub PublishSwitchEventUpdate(ByVal EventType As EventTypeSw)
            Try
                Dim Message As String = "DC" & Chr(6) & "SWITCH;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString

                Select Case EventType
                    Case EventTypeSw.CONTROLNORMAL
                        Message = Message & "|" & "SWN"
                    Case EventTypeSw.CONTROLREVERSE
                        Message = Message & "|" & "SWR"
                End Select
                If optEnableARRCSpecificOptions Then Message = Message & Chr(10) & gWorkStationName
                Message = Me.TerritoryID.ToString & Chr(12) & Message

                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                End SyncLock
            Catch EX As Exception
                SystemException(EX.ToString)
            End Try
        End Sub

        Public Sub PublishStatus(Optional ByVal ForceUpdate As Boolean = False)
            Try
                If Not ForceUpdate Then
                    If Me.StatusPreviousBlock = Me.BlockAbsolute And Me.StatusPreviousControlReverse = Me.ControlledReverse And Me.StatusPreviousPosition = Me.SwitchIndStatus Then
                        Exit Sub
                    Else
                        Me.StatusPreviousBlock = Me.BlockAbsolute
                        Me.StatusPreviousControlReverse = Me.ControlledReverse
                        Me.StatusPreviousPosition = Me.SwitchIndStatus
                    End If
                End If
                Dim Message As String = "DC" & Chr(6) & "SWSTAT;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.myControlPoint.GUID.ToString
                Message = Message & "|" & Me.GUID.ToString

                If Me.ControlledReverse Then
                    Message = Message & "|CTL=TRUE"
                Else
                    Message = Message & "|CTL=FALSE"
                End If

                Select Case Me.SwitchIndStatus
                    Case SwIndStatus.NoIndication
                        Message = Message & "|IND=NONE"
                    Case SwIndStatus.NormalInd
                        Message = Message & "|IND=TRUE"
                    Case SwIndStatus.ReverseInd
                        Message = Message & "|IND=FALSE"
                End Select

                If Me.BlockAbsolute Then
                    Message = Message & "|BLKCTL=TRUE|BLKIND=TRUE"
                Else
                    Message = Message & "|BLKCTL=FALSE|BLKIND=FALSE"
                End If


                Message = Me.TerritoryID.ToString & Chr(12) & Message

                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "DEVICE BLOCK SWITCH"

        Public Function FindCurrentTerritory(ByVal CurrentTerritoryID As Int32) As Territory
            Dim terr As Territory
            Try
                For Each terr In ControlledTerritories
                    If terr.TerritoryID = CurrentTerritoryID Then
                        Return terr
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Apply Device Block From Switch
        ''' </summary>
        ''' <param name="pIssueNote"></param>
        ''' <param name="pBlockNote"></param>
        ''' <returns></returns>
        Public Function ApplyDeviceBlockFromSwitch(ByVal pIssueNote As String, ByVal pBlockNote As String) As Boolean
            Dim index As Integer
            Dim gHistoryMessage As String
            Dim blk As New TrackAuthority.DeviceBlock()
            Dim terr As Territory
            Dim deviceBlockSelection As New ArrayList()
            Dim summItem As SummaryItem
            Try
                SwitchEventLog("ApplyDeviceBlockFromSwitch:pIssueNote=" & pIssueNote & ":pBlockNote=" & pBlockNote)
                terr = FindCurrentTerritory(Me.TerritoryID)
                With blk
                    .AuthorityLimits = "SWITCH DEVICE BLOCK"
                    .BlockNotes = pBlockNote
                    .OKTime = Now
                    .IssueTo = Me.Name & "-" & pIssueNote
                    .RailRoadName = "NONE"
                    .DispatcherName = gCurrentUser.FirstName & " " & gCurrentUser.LastName
                    .AuthorityDate = Now
                    .AuthorityNumber = terr.GetNextAuthorityNumber()
                    .Direction = "NONE"
                    .AuthorityType = "DEVICE BLOCK"
                    .UntilTime = Now
                    .AuthorityUID = terr.TerritoryID.ToString & " - " & .AuthorityNumber.ToString
                    .TerritoryAssignment = Me.TerritoryID
                    .ComponentsInSelection.Add(Me)
                End With
                deviceBlockSelection.Add(Me)
                For index = 1 To 3
                    Select Case index
                        Case 1
                            If blk.AddSwitchDeviceBlockToSystem(deviceBlockSelection, TMDS.GlobalDeclarations.gDeviceBlocks) Then
                                gHistoryMessage = "Device Block Applied to the System: " & blk.AuthorityUID
                                SwitchEventLog(gHistoryMessage)
                            Else
                                gHistoryMessage = "System Error! Please Consider Device Block " & blk.AuthorityUID & " NOT in Effect!"
                                SwitchEventLog(gHistoryMessage)
                                MessageBox.Show(Me, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                            End If
                        Case 2 'Check to see if track warrant data is written to database OK
                            If blk.AddDeviceBlockToDB() Then
                                gHistoryMessage = "Device Block Applied to the Database: " & blk.AuthorityUID
                                SwitchEventLog(gHistoryMessage)
                                blk.ApplyDeviceBlockToRemoteLinks()
                            Else
                                gHistoryMessage = "System Error! Please Consider Device Block " & blk.AuthorityUID & " NOT in Effect!"
                                SwitchEventLog(gHistoryMessage)
                                MessageBox.Show(Me, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                            End If
                        Case 3
                            summItem = New SummaryItem("BLOCK", "ADD", blk)
                            SyncLock gSummaryBlockQueue.SyncRoot
                                gSummaryBlockQueue.Enqueue(summItem)
                            End SyncLock
                    End Select
                Next
                SwitchEventLog("ApplyDeviceBlockFromSwitch:Device=" & blk.BlockGUID & ":" & blk.IssueTo)
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            Finally
                index = Nothing
                gHistoryMessage = Nothing
                blk = Nothing
                terr = Nothing
                deviceBlockSelection = Nothing
                summItem = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Remove device block from sw
        ''' </summary>
        ''' <param name="pDeviceBlock"></param>
        Public Sub RemoveDeviceBlockFromSwitch(pDeviceBlock As TrackAuthority.DeviceBlock)
            Dim summItem As SummaryItem
            Try
                If pDeviceBlock Is Nothing Then
                    Exit Sub
                End If
                SwitchEventLog("RemoveDeviceBlockFromSwitch:Device=" & pDeviceBlock.BlockGUID & ":" & pDeviceBlock.IssueTo)
                If pDeviceBlock.RemoveDeviceBlockFromDB() Then
                    pDeviceBlock.RemoveDeviceBlockFromRemoteLinks()
                    If pDeviceBlock.RemoveDeviceBlockFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        gDeviceBlocks.Remove(pDeviceBlock)
                        summItem = New SummaryItem("BLOCK", "REMOVE", pDeviceBlock)
                        SyncLock gSummaryBlockQueue.SyncRoot
                            gSummaryBlockQueue.Enqueue(summItem)
                        End SyncLock
                    End If
                Else
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                summItem = Nothing
            End Try
        End Sub

#End Region

    End Class

    #End Region

#Region "SWITCH LOCK"

    Public Class SwitchLock
        'Inherits GenericComponent
        Inherits UserControl
        Sub New(ByVal rlx As Int32, ByVal rly As Int32, ByVal LockPosition As String, theSwitch As Switch)
            Me.Visible = False
            Me.Left = rlx
            Me.Top = rly
            Me.Position = LockPosition
            ForeColor = Color.Red
            mySwitch = theSwitch
            BuildLockGraphics()
        End Sub

        Public LockBitPath As GraphicsPath
        Public Position As String
        Public myBrush As New SolidBrush(Color.Black)
        Public SwitchLockPathRelative As GraphicsPath
        Public mySwitch As Switch

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                If mySwitch.CurrentDrawingForm.Initializing Then Exit Sub
                If Me.LockBitPath IsNot Nothing Then e.Graphics.FillPath(myBrush, Me.LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub BuildLockGraphics()
            Try
                LockBitPath = New GraphicsPath()
                If Position = "RU" Then
                    Me.Top = Me.Top + 10
                    Me.Width = 16 '10
                    Me.Height = 10 '6
                    Me.Left = Left - 3

                    LockBitPath.AddLine(New Point(0, 0), New Point(16, 0))
                    LockBitPath.AddLine(New Point(16, 0), New Point(8, 8))
                    LockBitPath.AddLine(New Point(8, 8), New Point(0, 0))

                    'LockBitPath.AddLine(New Point(0, 0), New Point(10, 0))
                    'LockBitPath.AddLine(New Point(10, 0), New Point(5, 5))
                    'LockBitPath.AddLine(New Point(5, 5), New Point(0, 0))
                ElseIf Position = "RD" Then
                    Me.Top = Me.Top - 9 '7
                    Me.Left = Me.Left - 4 '1
                    Me.Width = 16 '11
                    Me.Height = 10 '6
                    LockBitPath.AddLine(New Point(0, 8), New Point(16, 8))
                    LockBitPath.AddLine(New Point(16, 8), New Point(8, 0))
                    LockBitPath.AddLine(New Point(8, 0), New Point(0, 8))
                    'LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    'LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    'LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))

                ElseIf Position = "LU" Then
                    Me.Top = Me.Top + 10
                    Me.Left = Left - 12 '9
                    Me.Width = 16 '10
                    Me.Height = 10 '6
                    LockBitPath.AddLine(New Point(0, 0), New Point(16, 0))
                    LockBitPath.AddLine(New Point(16, 0), New Point(8, 8))
                    LockBitPath.AddLine(New Point(8, 8), New Point(0, 0))
                    'LockBitPath.AddLine(New Point(0, 0), New Point(10, 0))
                    'LockBitPath.AddLine(New Point(10, 0), New Point(5, 5))
                    'LockBitPath.AddLine(New Point(5, 5), New Point(0, 0))
                ElseIf Position = "LD" Then
                    Me.Top = Me.Top - 9 '7
                    Me.Left = Me.Left - 12 '9
                    Me.Width = 16 '11
                    Me.Height = 10 '6
                    LockBitPath.AddLine(New Point(0, 8), New Point(16, 8))
                    LockBitPath.AddLine(New Point(16, 8), New Point(8, 0))
                    LockBitPath.AddLine(New Point(8, 0), New Point(0, 8))
                    'LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    'LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    'LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))

                ElseIf Position = "VRD" Then
                    Me.Top = Me.Top
                    Me.Left = Me.Left
                    Me.Width = 11
                    Me.Height = 6
                    LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))
                ElseIf Position = "VRU" Then
                    Me.Top = Me.Top
                    Me.Left = Me.Left + 5
                    Me.Width = 11
                    Me.Height = 6
                    LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))
                ElseIf Position = "VLU" Then
                    Me.Top = Me.Top + 10
                    Me.Left = Me.Left '- 9
                    Me.Width = 10
                    Me.Height = 6
                    LockBitPath.AddLine(New Point(0, 0), New Point(10, 0))
                    LockBitPath.AddLine(New Point(10, 0), New Point(5, 5))
                    LockBitPath.AddLine(New Point(5, 5), New Point(0, 0))
                ElseIf Position = "VLD" Then
                    'Me.Top = Me.Top - 70
                    'Me.Left = Me.Left - 90
                    'Me.Height = 6
                    'LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    'LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    'LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))

                    Me.Top = Me.Top + 4 ' - 70
                    Me.Left = Me.Left - 18 ' - 90
                    Me.Height = 6
                    LockBitPath.AddLine(New Point(0, 6), New Point(11, 6))
                    LockBitPath.AddLine(New Point(11, 6), New Point(6, 0))
                    LockBitPath.AddLine(New Point(6, 0), New Point(0, 6))

                End If
                Me.Region = New Region(LockBitPath)

                'initialize relative path for switch lock 
                Me.SwitchLockPathRelative = Me.LockBitPath.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.Left, Me.Top)
                SwitchLockPathRelative.Transform(mat)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                g.FillPath(myBrush, Me.SwitchLockPathRelative)

                g.Dispose()
                g = Nothing
            Catch ex As Exception
            Finally
                g = Nothing
            End Try
        End Sub

    End Class
#End Region

#Region "SWITCH POSITION INDICATOR"

    Public Class SwitchPositionIndicator
        Inherits UserControl
        Public IndPosition As String = "U"
        Public IndicatorPath As GraphicsPath
        Public Position As String
        Public mySwitch As Switch

#Region "Fields"

        Private pAlphaFont As Font = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
        Private pAlphaBox As RectangleF
        Private pAlphaLocation As Point
        Private pAlphaBoxSize As SizeF
        Private pAlphaColor As Color = Color.Red
        Private pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)

        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()
        Private pStringSize As SizeF
        Public TextColor As String
        Public BoxSize As Int32 = 150
        Public FontString As String

#End Region

#Region "Properties"

        Public Property AlphaFont() As Font
            Get
                Return pAlphaFont
            End Get
            Set(ByVal Value As Font)
                pAlphaFont = Value
            End Set
        End Property
        Public Property AlphaLocation() As Point
            Get
                Return pAlphaLocation
            End Get
            Set(ByVal Value As Point)
                pAlphaLocation = Value
            End Set
        End Property
        Public Property AlphaBoxSize() As SizeF
            Get
                Return pAlphaBoxSize
            End Get
            Set(ByVal Value As SizeF)
                pAlphaBoxSize = Value
            End Set
        End Property
        Public Property AlphaColor() As Color
            Get
                Return pAlphaColor
            End Get
            Set(ByVal Value As Color)
                pAlphaColor = Value
            End Set
        End Property

#End Region

        Sub New(ByVal rlx As Int32, ByVal rly As Int32, ByVal LockPosition As String, ByVal thisSwitch As Switch)
            Me.mySwitch = thisSwitch
            pAlphaFont = New Font("ARIAL", 11.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            BoxSize = 100
            pAlphaBrush = New SolidBrush(pAlphaColor)
            Dim g As Graphics = Me.CreateGraphics
            pStringFormat = New StringFormat()
            pStringFormat.Alignment = StringAlignment.Center
            pStringFormat.LineAlignment = StringAlignment.Center
            pStringSize = g.MeasureString(Me.IndPosition, pAlphaFont, BoxSize)

            pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            pAlphaBox = New RectangleF(New PointF(0.0, 0.0), pAlphaBoxSize)

            Me.Width = pStringSize.Width + 2
            Me.Height = pStringSize.Height - 1

            Me.Visible = True
            Me.Left = rlx
            Me.Top = rly
            Me.BackColor = Color.Black
            Me.Position = LockPosition
            ForeColor = Color.Red
            BuildLockGraphics()
            SetPosition()
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                If mySwitch.CurrentDrawingForm.Initializing Then Exit Sub

                e.Graphics.DrawString(IndPosition, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                'e.Graphics.FillPath(myBrush, Me.LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SetPosition()
            If Not IsNothing(mySwitch) Then
                If Me.mySwitch.FailState Or Me.mySwitch.SwitchIndStatus = Switch.SwIndStatus.NoIndication Then
                    If Me.mySwitch.ManualSwitchPointVerification Then
                        Me.IndPosition = "N"
                    Else
                        Me.IndPosition = "U"
                    End If
                    If Me.mySwitch.FailState Then
                        Me.ForeColor = Color.DarkGoldenrod
                    Else
                        Me.ForeColor = Color.Red
                    End If
                ElseIf Me.mySwitch.SwitchIndStatus = Switch.SwIndStatus.NormalInd Then 'And Not Me.mySwitch.ControlledReverse Then
                    If Me.mySwitch.ManualSwitchPointVerification Then
                        Me.IndPosition = "N"
                        Me.ForeColor = Color.DarkGoldenrod
                    Else
                        Me.IndPosition = "N"
                        Me.ForeColor = Color.SpringGreen
                    End If
                ElseIf Me.mySwitch.SwitchIndStatus = Switch.SwIndStatus.ReverseInd Then 'And Me.mySwitch.ControlledReverse Then
                    Me.IndPosition = "R"
                    Me.ForeColor = Color.Yellow
                Else
                    Me.IndPosition = "U"
                    If Me.mySwitch.FailState Then
                        Me.ForeColor = Color.DarkGoldenrod
                    Else
                        Me.ForeColor = Color.Red
                    End If
                End If
            End If


            If Me.InvokeRequired Then
                Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
            Else
                Me.Refresh()
            End If
        End Sub

        Sub BuildLockGraphics()
            Try
                IndicatorPath = New GraphicsPath()
                IndicatorPath.AddRectangle(pAlphaBox)
                If Position = "RU" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = 10
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 2
                ElseIf Position = "RD" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = (-20)
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 2
                ElseIf Position = "LU" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = 10
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Left - 10
                ElseIf Position = "LD" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = (-20)
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 10
                ElseIf Position = "VRD" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = (-20)
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 2
                ElseIf Position = "VRU" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = (-20)
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 10
                ElseIf Position = "VLU" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = 10
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Left - 100
                ElseIf Position = "VLD" Then
                    If Me.mySwitch.mySwitchPositionIndicatorCoordinate = 0 Then
                        Me.mySwitch.mySwitchPositionIndicatorCoordinate = (-20)
                    End If
                    Me.Top = Me.Top + Me.mySwitch.mySwitchPositionIndicatorCoordinate
                    Me.Left = Me.Left - 10
                End If
                Me.Region = New Region(IndicatorPath)

                ''build paths for bitmap imaging
                'Me.GraphicalPathRelative = Path.Clone
                'Dim mat As New Matrix()
                'mat.Translate(Me.LX, Me.LY)
                'GraphicalPathRelative.Transform(mat)

                Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

    

End Namespace
