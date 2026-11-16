Imports System.Data.OleDb
Imports System.Drawing.Drawing2D
Imports System.Text
Imports System.Timers
Imports TMDS.CodeServerClient
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.SystemUtilities.UtilitiesDisplayForm
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrackTagComponents
Imports TMDS.TrainComponents
Imports System.Reflection.MethodBase
Imports TMDS.SystemUtilities.FetchData

Namespace SignalComponents

#Region "SIGNAL"

    Public Class Signal
        Inherits GenericComponent

#Region "Properties"
        Private GraphicPathBulb As GraphicsPath
        Private GraphicPathStem As GraphicsPath

        Private PathArrowEntry As GraphicsPath
        Private PathArrowExit As GraphicsPath
        Private PathArrowEntryRelative As GraphicsPath
        Private PathArrowExitRelative As GraphicsPath


        Private RegionPathBulb As Region
        Public Bulb As RectangleF
        Private RegionPathStem As Region
        Private GraphicalBulb As Graphics
        Private GraphicalStem As Graphics
        Dim gBulbPath As New GraphicsPath()
        Dim gStemPath As New GraphicsPath()
        Dim gBulbPathRelative As GraphicsPath
        Dim gStemPathRelative As GraphicsPath


        'Public PTC_UID_KEY As String
        Public PTC_DeviceName As String
        Public PTC_ProperName As String
        Public PTC_WIUStatusIndex As String
        Public PTC_DeviceBlockID As String
        Public PTC_Enable_PSS As Boolean

        Dim CurrentBulbColorStatus As SolidBrush = New SolidBrush(Color.Red)
        Dim CurrentStemColorStatus As SolidBrush = New SolidBrush(Color.DarkGray)
        'Public SignalIndStatus As IndStatus
        Private pSignalIndStatus As IndStatus
        Public IndicationStatusUpdateTimeStamp As Date = Now

        Public Property SignalCalledOn() As Boolean
            Get
                Return pSignalCalledOn
            End Get
            Set(ByVal Value As Boolean)
                Dim PreviousValue As Boolean = SignalCalledOn
                pSignalCalledOn = Value
                If PreviousValue <> Value Then
                    If optEnableCallOnLatching Then
                        SignalCallOnUpdateDatabase()
                    End If
                    SignalEventLog("SIGNAL CALLON STATE CHANGE:" & Value.ToString.ToUpper)
                End If
            End Set
        End Property

        Private pCreateLicenseTrain As TrainID

        Public Property gJointLicenseForTrainTrain() As TrainID
            Get
                Return pCreateLicenseTrain
            End Get
            Set(ByVal value As TrainID)
                pCreateLicenseTrain = value
            End Set
        End Property

        Public Property SignalIndStatus() As IndStatus
            Get
                Return pSignalIndStatus
            End Get
            Set(ByVal Value As IndStatus)
                If pSignalIndStatus <> Value Then
                    SignalEventLog("SIGNAL STATE CHANGE:" & Value.ToString.ToUpper)
                    IndicationStatusUpdateTimeStamp = Now
                End If
                pSignalIndStatus = Value
            End Set
        End Property

        Public pSignalCtlStatus As CtlStatus
        Public pNoSignalControlInitialization As Boolean = True

        Public Property NoSignalControlInitialization() As Boolean
            Get
                Return pNoSignalControlInitialization
            End Get
            Set(ByVal Value As Boolean)
                pNoSignalControlInitialization = Value
            End Set
        End Property

        Public Property SignalCtlStatus() As CtlStatus
            Get
                Return pSignalCtlStatus
            End Get
            Set(ByVal Value As CtlStatus)
                pSignalCtlStatus = Value
            End Set
        End Property

        Public pLastCtlStatus As CtlStatus
        Public Property LastCtlStatus() As CtlStatus
            Get
                Return pLastCtlStatus
            End Get
            Set(ByVal Value As CtlStatus)
                pLastCtlStatus = Value
            End Set
        End Property

        Public SignalStatus As BulbStatus
        Public PreviousSignalStatus As BulbStatus
        Public SignalRouteCollection As New ArrayList()
        Public SignalRoutePendingCollection As ArrayList

        Public PreviousNonCompositeDetected As Boolean

        Public DefaultTraffic As String
        Public TrafficDirection As String
        'Public FlashSelection As Boolean
        Dim FlashOffColorStatus As SolidBrush = New SolidBrush(Color.Black)
        Public BrotherSignals As Boolean
        Public BrotherSignalCollection As ArrayList
        Public BrotherSignalFirst As Boolean
        Public InvalidateRegion As Region

        Public AllowFleeting As Boolean
        Public AllowStacking As Boolean
        Public AllowBlocking As Boolean
        Public AllowCallOn As Boolean
        Public CallOnControl As BitPosition
        Private pSignalCalledOn As Boolean
        Public CallOnAllowedWithoutOpposingTraffic As Boolean
        Public CallOnSendSignalClearBit As Boolean
        Public CallOnControlled As Boolean

        Public MatchMode As Boolean

        Public AllowDrilling As Boolean

        Public AllowSignalBlkInd As Boolean
        Public BlockIndBit As BitPosition

        Public InTimeType As TypeOfInTime
        Public InTimeBit As BitPosition

        Public DrillType As TypeOfDrill
        Public DrillControl As BitPosition
        Public DrillStop As BitPosition
        Public DrillIndOne As BitPosition
        Public DrillIndicatedOn As Boolean
        Public DrillIndTwo As BitPosition
        Public pDrilledSignal As Boolean
        Private pPreviousDrilledControlled As Boolean

        Public Property DrilledSignal() As Boolean
            Get
                Return pDrilledSignal
            End Get
            Set(ByVal Value As Boolean)
                pDrilledSignal = Value
            End Set
        End Property

        Public PDrillMode As Boolean
        Public Property LastControlSignal() As Boolean
            Get
                Return pLastControlSignal
            End Get
            Set(ByVal Value As Boolean)
                pLastControlSignal = Value
            End Set
        End Property
        Public Property DrillMode() As Boolean
            Get
                Return PDrillMode
            End Get
            Set(ByVal Value As Boolean)
                PDrillMode = Value
            End Set
        End Property

        Public Property PreviousDrilledControlled() As Boolean
            Get
                Return pPreviousDrilledControlled
            End Get
            Set(ByVal Value As Boolean)
                pPreviousDrilledControlled = Value
                If Not IsNothing(DrillPartner) Then
                    If Me.DrillPartner.PreviousDrilledControlled <> Value Then
                        Me.DrillPartner.PreviousDrilledControlled = Value
                    End If
                End If
            End Set
        End Property

        Public IsDrillSignal As Boolean
        Public DrillPartner As Signal
        Public DrillSharedComponents As ArrayList
        Public DrillRouteComponents As ArrayList
        Public myDrillTrack As Track
        Public DrillSignalMaster As Boolean

        'IN-TIME VARIABLES
        Public BrotherSignalIntimeCollection As New ArrayList()
        Private pLastControlSignal As Boolean
        Public Initialized As Boolean
        Public SlottedByOSTrack As Boolean
        Public SlottingOSTrack As Track
        Public BrotherSignalSharedStopBitCollection As ArrayList

        'Signal Indication Slotting
        Public SignalSlotting As Boolean
        Public PreviousSignalIndStatus As IndStatus
        Public InProgressOfSettingSignalToStop As Boolean
        Public ForceStopControl As Boolean
        Public SignalIndOnlyStatusPrevious As IndStatus

        'Non-Composite Control
        'Public CurrentRouteCollection As ArrayList
        Public tmrCheckRoute As Timers.Timer
        Public StoredEntrySignal As Boolean
        Public StoredExitSignal As Boolean
        Public SLFAEntrySignal As Boolean
        Public SLFAExitSignal As Boolean
        Public tmrShowPendingRoute As Timers.Timer
        Public SelectionPendingRoute As Boolean
        Public PendingRouteShow As Boolean
        Public NoAlarmOnPending As Boolean

        'Fleet
        Public FleetEnabled As Boolean
        Public tmrFleetRetry As Timers.Timer
        Public AttemptFleet As Boolean


        'ReControl Timer
        Public tmrReControl As Timers.Timer
        Public ReControlCount As Integer
        Public ReControlStart As Boolean

        'Flag to allow Item 7 warrants to be issued from left limits
        'temporarily part of TrackType as a "L"
        Public ItemSevenAllowedOnLeftLimits As Boolean

        'Flag to allow Item 7 warrants to be issued from right limits
        'temporarily part of TrackType as a "R"
        Public ItemSevenAllowedOnRightLimits As Boolean

        Public SigHead1Yellow As BitPosition
        Public SigHead1FYellow As BitPosition
        Public SigHead1FRed As BitPosition

        Public SigHead2Yellow As BitPosition
        Public SigHead2FYellow As BitPosition
        Public SigHead2FRed As BitPosition
        Public SigHead2Green As BitPosition
        Public SigHead2Lunar As BitPosition

        Public SigHead3Yellow As BitPosition
        Public SigHead3FYellow As BitPosition
        Public SigHead3FRed As BitPosition
        Public SigHead3Green As BitPosition
        Public SigHead3Lunar As BitPosition

        Public FleetControl As BitPosition
        Public ControlNotYetCommitted As Boolean
        Public Notes As String
        Public HoldingSignal As Boolean

        Public DisableSimulatedIndication As Boolean

        'Status Publishing
        Public StatusPreviousSignalStatusCtl As CtlStatus
        Public StatusPreviousSignalStatusInd As IndStatus

        'SIGNAL INDICATE-ONLY WITH CONTROL SET (IOWCS)
        Public IndicateOnlyWithControlSetEnabled As Boolean
        Public SignalControlled As Boolean

        'Movement Planning
        Public SignalClearAheadCount As Integer
        Public AllowAutomaticMode As Boolean
        Public AutomaticModeOn As Boolean
        Private mnuAutoRouting As MenuItem
        Public CurrentTacticalRoute As TacticalRoute
        Public ControlledByTacticalRoute As Boolean

        'joint control authorization
        Public JointControlAuthorizationEnabled As Boolean
        Public JointControlAuthorizationIndBit As BitPosition
        Public JointControlAuthorizationCtlBit As BitPosition
        Public JointControlAuthorizationCancelBit As BitPosition
        Public myAuthorizationIcon As SignalAuthorization
        Private mnuJointAuthorization As MenuItem
        Private mnuRequestTrackName As MenuItem
        Public RequestTrackNameEnabled As Boolean = True
        Public JointControlAuthorizationOfficeMode As Boolean
        Public JointControlAuthorizationOfficeModeRequested As Boolean
        Public JointControlAuthorizationOfficeModeConfirmed As Boolean


        Public NearSideSignalEnabled As Boolean
        Public NearSideSignalOn As Boolean
        Public NearSideSignalCtlBit As BitPosition
        Public NearSideSignalIndBit As BitPosition
        Private mnuNearSideSignalControl As MenuItem
        Public myControlArrow As SignalAuthorizationControlArrow
        Public myIndicationArrow As SignalAuthorizationIndicationArrow

        Public theGeoFenceInformation As String

        Public PublishStatusToPTC As Boolean
        Public DevicePTCUID As String = String.Empty

        Public AutomaticSignal As Boolean
        Public DisableTrafficProjection As Boolean

        'TUNNEL AUTOMATIC MODE
        Private mnuTunnelAutomaticMode As MenuItem
        Private TunnelAuto As Boolean

        'MRS-ALSTOM
        Public EnableDynamicSignalRouteControlBit As Boolean
        Public DynamicSignalRouteControlBitString As String
        Public EnablePulseStopControl As Boolean
        Public DynamicSignalRouteControlBitList As ArrayList

        Public DynamicSignalRouteCallOnBitString As String
        Public DynamicSignalRouteCallBitList As ArrayList

        Public Delegate Sub SendCommandControls(ByVal WaitTimeEnabled As Boolean)

        Public SupressSignalStopInterval As Integer = 20000
        Public SupressSignalStopWithOutOS As Boolean
        Public SupressSignalStopWithOutOsCount As Integer
        Public SignalIndicationSupressionStarted As Boolean = False
        Public SignalIndicationSupressionStatus As String = String.Empty

        'Signal Routes
        Public DisableEntryExitFunction As Boolean = False
        Public DisableStoreRouteFunction As Boolean = False
        Public IndicationSuppressTimer As System.Timers.Timer

        'TRACK WARRANT ENFORCEMENT 
        Public IsTrackWarrantEnforcementEnabled As Boolean

        'Remove Stop Target
        Public IsStopTargetApplied As Boolean = False

        Public FlipMe As Boolean = False
#End Region

        Sub New()
            'Build the flasher timer for the signal bulb
            Flasher = New Windows.Forms.Timer() 'New Timers.Timer()
            Flasher.Interval = 250

            AddHandler Flasher.Tick, AddressOf FlashElapsedEvent

            'Build the simulator timer for the signal bulb
            IndSimulator = New Timers.Timer()
            IndSimulator.Interval = 4000
            AddHandler IndSimulator.Elapsed, AddressOf SimulateElaspedEvent

            'Non-Composite Check Route Timer
            tmrCheckRoute = New Timers.Timer()
            Me.tmrCheckRoute.Interval = 1000
            Me.tmrCheckRoute.AutoReset = False
            AddHandler tmrCheckRoute.Elapsed, AddressOf Me.CheckRouteElaspedEvent

            'ReControl Timer
            Me.tmrReControl = New Timers.Timer()
            Me.tmrReControl.Interval = 60000
            Me.tmrReControl.AutoReset = False
            AddHandler tmrReControl.Elapsed, AddressOf Me.ReControlElaspedEvent

            'ReFleet Timer
            Me.tmrFleetRetry = New Timers.Timer()
            Me.tmrFleetRetry.Interval = 2000
            Me.tmrFleetRetry.AutoReset = False
            AddHandler tmrFleetRetry.Elapsed, AddressOf Me.FleetRetryElaspedEvent

            'Show Pending Route Timer
            tmrShowPendingRoute = New Timers.Timer()
            Me.tmrShowPendingRoute.AutoReset = False
            Me.tmrShowPendingRoute.Interval = 250
            AddHandler tmrShowPendingRoute.Elapsed, AddressOf Me.ShowPendingRoute

            'Set Default Drill Type
            Me.DrillType = TypeOfDrill.None

            'set default control mode
            Me.SignalCtlStatus = CtlStatus.RequestedStop

            Me.AuthorityType = "CTC"

        End Sub

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then

                Try
                    RemoveHandler Me.MouseHover, AddressOf Me.Signal_MouseHover
                Catch
                End Try
                Try
                    RemoveHandler Flasher.Tick, AddressOf FlashElapsedEvent
                    Flasher.Dispose()
                    Flasher = Nothing
                Catch
                End Try
                Try
                    RemoveHandler tmrCheckRoute.Elapsed, AddressOf Me.CheckRouteElaspedEvent
                    tmrCheckRoute.Dispose()
                    tmrCheckRoute = Nothing
                Catch
                End Try

                Try
                    RemoveHandler tmrReControl.Elapsed, AddressOf Me.ReControlElaspedEvent
                    tmrReControl.Dispose()
                    tmrReControl = Nothing
                Catch
                End Try

                Try
                    RemoveHandler tmrShowPendingRoute.Elapsed, AddressOf Me.ShowPendingRoute
                    tmrShowPendingRoute.Dispose()
                    tmrShowPendingRoute = Nothing
                Catch
                End Try
                If Not IsNothing(Me.IndSimulator) Then
                    RemoveHandler IndSimulator.Elapsed, AddressOf SimulateElaspedEvent
                    IndSimulator.Dispose()
                    IndSimulator = Nothing
                End If
                Try
                    Me.gBulbPath = Nothing
                    Me.RegionPathStem = Nothing
                    Me.PathArrowEntry = Nothing
                    Me.FlashOffColorStatus = Nothing
                    Me.gStemPath = Nothing
                    Me.CurrentBulbColorStatus = Nothing
                    Me.CurrentStemColorStatus = Nothing
                    Me.InvalidateRegion = Nothing
                    If Not IsNothing(myAuthorizationIcon) Then
                        myAuthorizationIcon.Dispose()
                        myAuthorizationIcon = Nothing
                    End If
                    If Not IsNothing(myControlArrow) Then
                        myControlArrow.Dispose()
                        myControlArrow = Nothing
                    End If
                    If Not IsNothing(myIndicationArrow) Then
                        myIndicationArrow.Dispose()
                        myIndicationArrow = Nothing
                    End If

                Catch
                End Try
            End If
            MyBase.Dispose(disposing)
        End Sub

        Public Overloads Sub Dispose()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#Region "SELECTION MODE"

        Public ColorStatusBlocking As New SolidBrush(Color.SteelBlue)
        Public ColorStatusRouteRequesting As New SolidBrush(Color.YellowGreen)
        Public ColorSummarySelectionBlocking As New SolidBrush(Color.LightSteelBlue)
        Public ColorSummarySelectionRequesting As New SolidBrush(Color.Yellow)

#End Region

#Region "CONTEXT MENU"

        Public mnuSignalStop As MenuItem
        Public mnuSignalDrill As MenuItem
        Public mnuSignalCallOn As MenuItem
        Public mnuFleetOn As MenuItem
        Public mnuCallOn As MenuItem

        Public mnuSignalSimIndClear As MenuItem
        Public mnuSignalSimIndStop As MenuItem
        Public mnuSignalSimIndInTime As MenuItem
        Public mnuSignalSimIndDisable As MenuItem

        Public mnuSignalClear As MenuItem
        Public mnuDBCreate As MenuItem
        Public mnuDBRemove As MenuItem

        Public Sub ContextMenuEnabled(ByVal EnableMenuItem As Boolean)
            Try
                Dim mi As MenuItem
                For Each mi In Me.ContextMenu.MenuItems
                    mi.Enabled = EnableMenuItem
                Next
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        ''' <summary>
        ''' ContextMenu_PopUp
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If gPermissionPassRedSignal Or gCreateTrainPassRed564 Then Exit Sub
                If Me.AllowAutomaticMode Then
                    If Me.AutomaticModeOn Then
                        mnuAutoRouting.Text = "DISABLE AUTO-ROUTING"
                    Else
                        mnuAutoRouting.Text = "ENABLE AUTO-ROUTING"
                    End If
                End If
                If JointControlAuthorizationEnabled AndAlso 'JOIN CTC MODE
                    Not JointControlAuthorizationOfficeMode AndAlso
                    Not myControlPoint.TunnelAutomaticMode Then
                    If JointControlAuthorizationCtlBit.BitState Then
                        mnuJointAuthorization.Text = mstrmnuCancelRequestAuthorization 'Cancel text
                    Else
                        mnuRequestTrackName.Text = optSignalAuth36
                        mnuJointAuthorization.Text = mstrmnuRequestAuthorization 'Request text
                    End If
                ElseIf JointControlAuthorizationOfficeMode Then 'JOIN OFFICE MODE
                    Dim theSignalRouteCollection As ArrayList = GetTheSignalRoute()
                    SetRequestsOptionsSignalMenu()
                    If theSignalRouteCollection IsNot Nothing AndAlso
                       theSignalRouteCollection.Count > 0 AndAlso
                        theSignalRouteCollection.Item(theSignalRouteCollection.Count - 1).GetType.ToString = gTrackComponentName Then
                        Dim theTrack As Track = CType(theSignalRouteCollection.Item(theSignalRouteCollection.Count - 1), Track)
                        If theTrack IsNot Nothing AndAlso
                            Not theTrack.IsRemoteTrack AndAlso
                            (theTrack.OverrideLeftTrafficCheck Or theTrack.OverrideRightTrafficCheck) Then
                            mnuJointAuthorization.Enabled = False
                        ElseIf Notes.Contains("ENABLETRACKNAMEREQUEST") Then
                            SetRequestsOptionsSignalMenu()
                        Else
                            mnuJointAuthorization.Enabled = True
                        End If
                    End If
                    If JointControlAuthorizationOfficeModeRequested Then
                        mnuJointAuthorization.Text = mstrmnuCancelRequestAuthorization 'Cancel text
                    Else
                        mnuJointAuthorization.Text = mstrmnuRequestAuthorization 'Request text
                    End If
                End If
                If Me.NearSideSignalEnabled Then
                    If Me.NearSideSignalOn Then
                        Me.mnuNearSideSignalControl.Text = "NSS LOCAL MODE OFF"
                    Else
                        Me.mnuNearSideSignalControl.Text = "NSS LOCAL MODE ON"
                    End If
                End If
                If Me.myControlPoint.TunnelAutomaticMode Then
                    If Me.myControlPoint.TunnelAutomaticControlled Then
                        If OptEnableSignalClearByMenu Then Me.mnuSignalClear.Visible = False
                        Me.mnuTunnelAutomaticMode.Visible = True
                        If Me.JointControlAuthorizationCtlBit.BitState Then
                            Me.mnuTunnelAutomaticMode.Text = "AUTOMATIC MODE STOP"
                            TunnelAuto = False
                        Else
                            Me.mnuTunnelAutomaticMode.Text = "AUTOMATIC MODE CLEAR"
                            TunnelAuto = True
                        End If
                    Else
                        If OptEnableSignalClearByMenu AndAlso Not Me.myControlPoint.FailState Then Me.mnuSignalClear.Visible = True
                        Me.NearSideSignalOn = True
                        Me.mnuTunnelAutomaticMode.Visible = False
                    End If
                ElseIf OptEnableSignalClearByMenu AndAlso (Me.myControlPoint.FailState AndAlso OptEnableSignalClearByMenuCheckControlPointStatus) Then
                    Me.mnuSignalClear.Enabled = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub BuildContextMenu()
            Try
                If gTrainerMode Then
                    Me.ContextMenuControl = New ContextMenu()
                    AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                    'Me.mnuTrainerIndNormal = New MenuItem("SIMULATE-NORMAL-INDICATION")
                    'AddHandler mnuTrainerIndNormal.Click, AddressOf Me.mnuTrainerIndNormal_Click
                    'ContextMenuControl.MenuItems.Add(mnuTrainerIndNormal)

                    Me.ContextMenu = Me.ContextMenuControl
                    Exit Sub
                ElseIf gAssistMode OrElse Me.IsShadowMode Then
                    If gAssistMode AndAlso optUseTroubleReportingEnhanced AndAlso optEnableSpecificFunctionalityForDenver Then 'CMV DTP-115
                        Me.ContextMenuControl = New ContextMenu()
                        AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp
                        AddTroubleReportMenuItem()
                        Me.ContextMenu = Me.ContextMenuControl
                    End If
                    Exit Sub
                End If


                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                If OptEnableSignalClearByMenu AndAlso Not Me.AutomaticSignal Then
                    If ContextMenuControl.MenuItems.Count > 0 Then
                        Dim signalSep As New MenuItem("-")
                        ContextMenuControl.MenuItems.Add(signalSep)
                    End If
                    mnuSignalClear = New MenuItem(mstrmnuSignalClear)
                    AddHandler mnuSignalClear.Click, AddressOf Me.mnuSignalClear_Click
                    ContextMenuControl.MenuItems.Add(mnuSignalClear)
                End If

                If Me.DrillType <> SignalComponents.Signal.TypeOfDrill.None Then
                    Me.mnuSignalDrill = New MenuItem()
                    AddHandler mnuSignalDrill.Click, AddressOf Me.mnuSignalMenu_Click
                    ContextMenuControl.MenuItems.Add(mnuSignalDrill)
                End If

                If Me.AllowCallOn Then
                    mnuSignalCallOn = New MenuItem()
                    mnuSignalCallOn.Text = mstrmnuSignalCallOn
                    AddHandler mnuSignalCallOn.Click, AddressOf Me.mnuSignalMenu_Click
                    ContextMenuControl.MenuItems.Add(mnuSignalCallOn)
                End If

                If OptEnableSignalFleetMenu AndAlso Me.DrillType = SignalComponents.Signal.TypeOfDrill.None And Not Me.IndicateOnly Then
                    mnuFleetOn = New MenuItem()
                    If Not Me.FleetEnabled And Not Me.DeviceBlocked Then
                        mnuFleetOn.Text = mstrmnuFleetOn
                    Else
                        mnuFleetOn.Text = mstrmnuFleetOFF
                    End If
                    AddHandler mnuFleetOn.Click, AddressOf Me.mnuSignalMenu_Click
                    ContextMenuControl.MenuItems.Add(mnuFleetOn)
                End If

                If Me.AllowAutomaticMode Then
                    Dim mnuSplit As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit)

                    mnuAutoRouting = New MenuItem("ENABLE AUTO-ROUTING")
                    AddHandler mnuAutoRouting.Click, AddressOf Me.mnuAutoRouting_Click
                    ContextMenuControl.MenuItems.Add(mnuAutoRouting)
                End If

                BuildSignalMenuChooseRequest()

                If Me.NearSideSignalEnabled Then
                    If ContextMenuControl.MenuItems.Count > 0 Then
                        Dim mnuSplit2 As New MenuItem("-")
                        ContextMenuControl.MenuItems.Add(mnuSplit2)
                    End If
                    Dim NearSideMenutext As String = "NSS LOCAL MODE ON"
                    If Me.NearSideSignalOn Then
                        NearSideMenutext = "NSS LOCAL MODE OFF"
                    End If
                    mnuNearSideSignalControl = New MenuItem(NearSideMenutext)
                    AddHandler mnuNearSideSignalControl.Click, AddressOf Me.mnuNearSideSignalControl_Click
                    ContextMenuControl.MenuItems.Add(mnuNearSideSignalControl)
                End If


                If Me.myControlPoint.TunnelAutomaticMode Then
                    Dim TunnelAutomaticModetext As String = "AUTOMATIC MODE ON"
                    If Me.NearSideSignalOn Then
                        TunnelAutomaticModetext = "AUTOMATIC MODE OFF"
                    End If

                    mnuTunnelAutomaticMode = New MenuItem(TunnelAutomaticModetext)
                    AddHandler mnuTunnelAutomaticMode.Click, AddressOf Me.mnuTunnelAutomaticMode_Click
                    ContextMenuControl.MenuItems.Add(mnuTunnelAutomaticMode)
                End If

                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem()
                End If

                If OptEnableSignalBlockMenu Then
                    mnuDBCreate = New MenuItem(optGraphicalCompContextMenuSigBlockCreate)
                    AddHandler mnuDBCreate.Click, AddressOf Me.mnuDBCreate_Click
                    ContextMenuControl.MenuItems.Add(mnuDBCreate)

                    mnuDBRemove = New MenuItem(optGraphicalCompContextMenuSigBlockRemove)
                    AddHandler mnuDBRemove.Click, AddressOf Me.mnuDBRemove_Click
                    ContextMenuControl.MenuItems.Add(mnuDBRemove)
                End If

                Me.ContextMenu = Me.ContextMenuControl
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RefreshContextMenu()
            Try
                If Me.DrillType <> SignalComponents.Signal.TypeOfDrill.None Then
                    Me.mnuSignalDrill.Enabled = True
                    If Me.DeviceBlocked Then
                        Me.mnuSignalDrill.Enabled = False
                    End If
                    If Me.DrillMode Then
                        If Not IsNothing(Me.myDrillTrack) And (Me.DrillSignalMaster Or Me.DrillPartner.DrillSignalMaster) Then
                            Me.mnuSignalDrill.Text = mstrmnuSignalDrillOFF
                        End If
                    Else
                        'If Not Me.DeviceBlocked Then
                        Me.mnuSignalDrill.Text = mstrmnuSignalDrillON
                        'End If
                    End If
                End If
                If Not Me.SignalCalledOn And Me.DrillType = SignalComponents.Signal.TypeOfDrill.None Then
                    If Me.AllowCallOn And Not Me.DeviceBlocked Then
                        If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                            mnuSignalCallOn.Enabled = False
                        Else
                            mnuSignalCallOn.Enabled = True
                        End If
                    Else
                        If Not IsNothing(mnuSignalCallOn) Then
                            mnuSignalCallOn.Enabled = False
                        End If
                    End If
                    If OptEnableSignalFleetMenu AndAlso
                        Not JointControlAuthorizationOfficeMode Then
                        Select Case SignalCtlStatus
                            Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                If Not Me.FleetEnabled And Not Me.DeviceBlocked Then
                                    mnuFleetOn.Text = mstrmnuFleetOn
                                Else
                                    mnuFleetOn.Text = mstrmnuFleetOFF
                                End If
                            Case Else
                                If FleetEnabled Then
                                    mnuFleetOn.Text = mstrmnuFleetOFF
                                End If
                        End Select
                    End If
                End If

                If OptEnableSignalFleetMenu Then
                    If (Me.IndicateOnly Or Me.SignalCalledOn Or Me.SignalIndStatus <> IndStatus.Cleared Or (Me.SignalIndStatus = IndStatus.Cleared And (Me.SignalCtlStatus = CtlStatus.RequestedStop Or Me.SignalCtlStatus = CtlStatus.RequestingStop))) And Not Me.FleetEnabled Then
                        If (Me.SignalIndStatus = IndStatus.Stopped And (Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear)) Then
                            Me.mnuFleetOn.Visible = True
                        Else
                            Me.mnuFleetOn.Visible = False
                        End If
                    Else
                        Me.mnuFleetOn.Visible = True
                    End If
                End If

                If (Me.SignalIndStatus <> IndStatus.Stopped AndAlso Me.AllowCallOn) Or Me.DeviceBlocked Then
                    If Me.mnuSignalCallOn IsNot Nothing Then Me.mnuSignalCallOn.Enabled = False
                ElseIf Me.AllowCallOn Then
                    If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                        If Me.mnuSignalCallOn IsNot Nothing Then mnuSignalCallOn.Enabled = False
                    Else
                        If Me.mnuSignalCallOn IsNot Nothing Then mnuSignalCallOn.Enabled = True
                    End If
                End If

                If Me.AllowAutomaticMode Then
                    If Me.AutomaticModeOn Then
                        mnuAutoRouting.Text = "DISABLE AUTO-ROUTING"
                    Else
                        mnuAutoRouting.Text = "ENABLE AUTO-ROUTING"
                    End If
                End If

                If Me.JointControlAuthorizationEnabled Then
                    If Me.JointControlAuthorizationOfficeMode Then
                        If Me.JointControlAuthorizationOfficeModeRequested Then
                            Me.mnuJointAuthorization.Text = mstrmnuCancelRequestAuthorization ' "CANCEL AUTHORIZATION REQUEST"
                        Else
                            Me.mnuJointAuthorization.Text = mstrmnuRequestAuthorization '"REQUEST AUTHORIZATION"
                        End If
                    Else
                        If Me.JointControlAuthorizationCtlBit.BitState Then
                            Me.mnuJointAuthorization.Text = mstrmnuCancelRequestAuthorization ' "CANCEL AUTHORIZATION REQUEST"
                        Else
                            Me.mnuJointAuthorization.Text = mstrmnuRequestAuthorization '"REQUEST AUTHORIZATION"
                        End If
                    End If
                End If

                If Me.NearSideSignalEnabled Then
                    Dim NearSideMenutext As String = "NSS LOCAL MODE ON"
                    If Me.NearSideSignalOn Then
                        NearSideMenutext = "NSS LOCAL MODE OFF"
                    End If
                    mnuNearSideSignalControl.Text = NearSideMenutext
                End If

                If OptEnableSignalClearByMenu Then
                    If OptEnableSignalClearByMenuCheckControlPointStatus Then
                        If Me.myControlPoint.FailState Then
                            Me.mnuSignalClear.Enabled = False
                        Else
                            Me.mnuSignalClear.Enabled = True
                        End If
                    Else
                        Me.mnuSignalClear.Enabled = True
                    End If

                    Select Case Me.SignalCtlStatus
                        Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                            Me.mnuSignalClear.Enabled = True
                        Case Else
                            Me.mnuSignalClear.Enabled = False
                    End Select
                End If

                If OptEnableSignalBlockMenu Then BuildBlockMenus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        ''' <summary>
        ''' Menu call on
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub mnuSignalMenu_Click(ByVal sender As System.Object,
                                        ByVal e As System.EventArgs)
            Try
                SignalEventLog($"SIGNAL MENU ITEM SELECTED:SIGNAL MENU")
                Dim mi As MenuItem = DirectCast(sender, MenuItem)
                Dim menuName As String = mi.Text
                If myControlPoint.FailState AndAlso
                    Not gSimulationMode Then Exit Sub
                If IndicateOnly Then
                    SignalEventLog("SIGNAL INDICATE ONLY")
                    SignalIndicateOnlyeAlert()
                    ClearSystemVariables()
                    Exit Sub
                End If
                If myControlPoint.MaintenanceMode Then
                    SignalEventLog("SIGNAL IN MAINTENANCE MODE")
                    Exit Sub
                End If
                If IsRequestPopup AndAlso
                    Not RequestPopup() Then Exit Sub
                Select Case mi.Text
                    Case "DRILL ON"
                        ControlSignalToDrill(True)
                    Case "DRILL OFF"
                        ControlSignalToDrillStop()
                    Case "CALL ON", mstrmnuSignalCallOn
                        If Not ControlSignalToClear(True, True, False, True, False, False, False, True) Then
                            Exit Sub
                        Else
                            myControlPoint.FlasherMode(True)
                            gSendCTC = True
                            SetCursor(curSend)
                            menuName = "CALL ON"
                        End If
                    Case "FLEET ON", mstrmnuFleetOn
                        menuName = "FLEET ON"
                        If DrillMode Or IndicateOnly Then
                            Exit Sub
                        End If
                        If Not IsNothing(CallOnControl) Then
                            If CallOnControl.BitState = True Then
                                Exit Sub
                            End If
                        End If
                        DoSetSignalFleeting(True, True)
                    Case "FLEET OFF", mstrmnuFleetOFF
                        DoSetSignalFleeting(False, True)
                        menuName = "FLEET OFF"
                    Case "SIMULATE CLEAR INDICATION"
                        SendTrainerCommand("CLEAR")
                    Case "SIMULATE STOP INDICATION"
                        SendTrainerCommand("STOP")
                    Case "SIMULATE IN-TIME INDICATION"
                        SendTrainerCommand("IN-TIME")
                    Case "DISABLE SIMULATED INDICATION"
                        SendTrainerCommand("DISABLE-INDICATION")
                End Select
                SignalEventLog($"SIGNAL MENU ITEM SELECTED:TYPE={menuName}")
            Catch ex As Exception
                SystemException(ex.ToString)
                ClearSystemVariables()
            End Try
        End Sub

        ''' <summary>
        ''' Menu signal clear
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Private Sub mnuSignalClear_Click(ByVal sender As System.Object,
                                         ByVal e As System.EventArgs)
            Try
                SignalEventLog($"SIGNAL MENU ITEM SELECTED:SIGNAL CLEAR")
                If DeviceBlocked Then Exit Sub
                If myControlPoint.MaintenanceMode Then
                    SignalEventLog("SIGNAL IN MAINTENANCE MODE")
                    Exit Sub
                End If
                If IsRequestPopup AndAlso
                    Not RequestPopup() Then Exit Sub
                If Not ControlSignalToClear(True, True, False, False, False, False, False, True, False, False, True, True) Then
                    Exit Sub
                Else
                    myControlPoint.FlasherMode(True)
                    gSendCTC = True
                    SetCursor(curSend)
                    If optEnableCallOnLatching Then
                        If SignalCalledOn Then
                            SignalCalledOn = False
                            If Not IsNothing(Me.CallOnControl) Then
                                CallOnControl.BitState = False
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#Region "SIGNAL DEVICE BLOCK "

        Private Sub mnuDBCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SignalEventLog("Clicked - Create Device Block from CM")
                gCreateDeviceBlock = True
                gBlockAllDevices = True
                SetCursor(curCreateDB)
                Me.DoDeviceBlock()
                Dim comp As Object = gDeviceBlockSelection.Item(0)

                SystemForms.MySystemForms.frmDeviceBlock = New frmDeviceBlock()
                SystemForms.MySystemForms.frmDeviceBlock.FindCurrentTerritory(comp.TerritoryID)
                Dim BlockName As String = String.Empty
                BlockName = Me.myControlPoint.Name

                If optApplyConfigurationForKCT Then BlockName = Me.Name & "@" & Me.myControlPoint.Name
                Dim DBblk As TMDS.TrackAuthority.DeviceBlock = Nothing
                If SystemForms.MySystemForms.frmDeviceBlock.BuildDeviceBlock(comp.TerritoryID, gDeviceBlockSelection, BlockName) Then
                    SystemForms.MySystemForms.frmDeviceBlock.SetUpProperties(gDeviceBlocks)
                    If SystemForms.MySystemForms.frmDeviceBlock.CheckForClearedSignals(Me.CurrentDrawingForm) Then
                        SystemForms.MySystemForms.frmDeviceBlock.ShowDialog()
                        DBblk = SystemForms.MySystemForms.frmDeviceBlock.ThisDeviceBlock
                    End If
                End If
                SystemForms.MySystemForms.frmDeviceBlock = Nothing
                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearDeviceSelection(DBblk)
                ClearSystemVariables()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub BuildBlockMenus()


            Try
                Dim miRemove As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuDBRemove))
                Dim miCreate As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuDBCreate))
                miRemove.MenuItems.Clear()
                If miCreate IsNot Nothing Then miCreate.Visible = True
                If Me.DeviceBlocks.Count > 0 Then
                    Dim auth As TrackAuthority.DeviceBlock
                    miRemove.Visible = True
                    For Each auth In Me.DeviceBlocks
                        'Me.mnuDBRemove.MenuItems.Add(New MenuItem(auth.AuthorityUID & ", (" & auth.IssueTo & ")", AddressOf mnuDBRemove_Click))
                        Me.mnuDBRemove.MenuItems.Add(New MenuItem(Format(auth.AuthorityDate, "HH:mm:ss") & ", (" & auth.IssueTo & ")", AddressOf mnuDBRemove_Click))
                    Next
                    miCreate.Visible = False
                Else
                    miRemove.Visible = False
                    'ContextMenuControl.MenuItems.Remove(mnuDBRemove)
                End If

                If (Me.IsShadowMode) Then
                    Dim m As MenuItem
                    For Each m In ContextMenu.MenuItems
                        m.Visible = False
                    Next
                End If

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
                SignalEventLog("Clicked - Remove Device Block from CM:" & blk.AuthorityUID)
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

        Private Function FindTrackBlock(ByVal BlockTime As String) As TrackAuthority.DeviceBlock
            Try
                Dim blk As TrackAuthority.DeviceBlock
                For Each blk In Me.DeviceBlocks
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

#Region "AUTOROUTING MENU"

        Public Sub mnuAutoRouting_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If mnuAutoRouting.Text = "ENABLE AUTO-ROUTING" Then
                    mnuAutoRouting.Text = "DISABLE AUTO-ROUTING"
                    ToggleAutoRouting(True, "MENU")
                Else
                    mnuAutoRouting.Text = "ENABLE AUTO-ROUTING"
                    ToggleAutoRouting(False, "MENU")
                End If
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "JOINT AUTHORIZATION"

        ''' <summary>
        ''' Joint Authorization
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Sub mnuJointAuthorization_Click(ByVal sender As System.Object,
                                               ByVal e As System.EventArgs)
            Try
                SignalEventLog("CLICKED - SEND AUTHORIZATION REQUEST")
                If JointControlAuthorizationOfficeMode Then
                    Dim theSignalRouteCollection As ArrayList = GetTheSignalRoute()
                    If theSignalRouteCollection Is Nothing Then Exit Sub
                    JoinControlSignalRouteCollection.Clear()
                    JoinControlSignalRouteCollection = theSignalRouteCollection.Clone
                    If DoesSignalRouteHaveJoinTrack(theSignalRouteCollection) Then 'Via join integration
                        SendJoinIntegrationOfficeMode(theSignalRouteCollection)
                    ElseIf DoesSignalRouteHaveRemoteTrack(theSignalRouteCollection) Then 'Via remote track
                        If myAuthorizationIcon IsNot Nothing Then
                            If JointControlAuthorizationOfficeModeRequested Then
                                JointControlAuthorizationOfficeModeRequested = False
                                JointControlAuthorizationOfficeModeConfirmed = False
                                RequestJointAuthForLicenseClick = False
                                If gJointLicenseForTrainTrain IsNot Nothing Then
                                    gJointLicenseForTrainTrain.RequestJointAuthForLicnese = False
                                End If
                                gJointLicenseForTrainTrain = Nothing
                            Else
                                JointControlAuthorizationOfficeModeRequested = True
                                RequestJointAuthForLicenseClick = True
                            End If
                            JointControlAuthorizationRequest(Me)
                        End If
                    End If
                    JointControlAuthorizationUpdateStatus()
                Else
                    SendJoinAuthorizationCtcMode()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Joint Control Authorization Off
        ''' </summary>
        ''' <param name="autoSend">auto send controls</param>
        Public Sub JointControlAuthorizationOff(autoSend As Boolean)
            Try
                SignalEventLog($"{GetCurrentMethod.Name}:AutoSend={autoSend}")
                If RequestJointAuthForLicenseClick Then RequestJointAuthForLicenseClick = False
                If RequestJointAuthForPSS Then RequestJointAuthForPSS = False
                If gJointLicenseForTrainTrain IsNot Nothing Then
                    SystemEvent($"{GetCurrentMethod.Name}:Train is nothing")
                    gJointLicenseForTrainTrain.RequestJointAuthForLicnese = False
                    gJointLicenseForTrainTrain = Nothing
                End If
                'Join authorization office mode
                If JointControlAuthorizationOfficeMode Then
                    JointControlAuthorizationOfficeModeConfirmed = False
                    JointControlAuthorizationOfficeModeRequested = False
                    If myAuthorizationIcon IsNot Nothing Then JointControlAuthorizationUpdateStatus()
                    PerformJointControlAuthorizationUpdateStatus()
                Else 'Join authorization CTC mode
                    If JointControlAuthorizationCtlBit IsNot Nothing Then
                        JointControlAuthorizationCtlBit.BitState = False
                        If JointControlAuthorizationCancelBit IsNot Nothing Then
                            JointControlAuthorizationCancelBit.BitState = True
                        End If
                    End If
                    If myAuthorizationIcon IsNot Nothing Then JointControlAuthorizationUpdateStatus()
                    If BrotherSignalCollection IsNot Nothing Then
                        For Each oneSignal As Signal In BrotherSignalCollection
                            oneSignal.JointControlAuthorizationCtlBit.BitState = False
                            If JointControlAuthorizationCancelBit IsNot Nothing Then
                                JointControlAuthorizationCancelBit.BitState = True
                            End If
                        Next
                    End If
                    If autoSend Then
                        SignalEventLog($"{GetCurrentMethod.Name}:Send controls")
                        myControlPoint.FlasherMode(True)
                        AddToSendCollection(GetCurrentMethod.Name, True)
                        Dim doSendControls As New SendCommandControls(AddressOf SystemForms.MySystemForms.frmMainMDI.SendControls)
                        doSendControls.BeginInvoke(True, Nothing, Nothing)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Delegate Sub DoUpdateDatabaseJointAuthorizationOfficeMode()

        Private Sub PerformJointControlAuthorizationUpdateStatus()
            Try
                Dim theUpdate As New DoUpdateDatabaseJointAuthorizationOfficeMode(AddressOf UpdateDatabaseJointAuthorization)
                theUpdate.BeginInvoke(Nothing, Nothing)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Joint Control Authorization Update Status
        ''' </summary>
        Public Sub JointControlAuthorizationUpdateStatus()
            Try
                If JointControlAuthorizationOfficeMode Then
                    If JointControlAuthorizationOfficeModeConfirmed Then
                        myAuthorizationIcon.myBrush = New SolidBrush(Color.Green)
                        myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                    Else
                        If JointControlAuthorizationOfficeModeRequested Then
                            myAuthorizationIcon.myBrush = New SolidBrush(Color.Yellow)
                            myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                        Else
                            If SignalIndStatus <> Signal.IndStatus.Cleared Then
                                RequestJointAuthForLicense = False
                            End If
                            If Not RequestJointAuthForLicense Then
                                If SignalIndStatus <> Signal.IndStatus.Cleared Then
                                    myAuthorizationIcon.myBrush = New SolidBrush(Color.Red)
                                    myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                                    ToggleTrackNameOption(False, Me)
                                End If
                            End If
                            If RequestJointAuthForLicense Then RequestJointAuthForLicense = False
                            If gJointLicenseForTrainTrain IsNot Nothing Then
                                If gJointLicenseForTrainTrain.RequestJointAuthForLicnese Then
                                    gJointLicenseForTrainTrain.RequestJointAuthForLicnese = False
                                End If
                            End If
                        End If
                    End If
                    UpdateDatabaseJointAuthorization()
                Else
                    If JointControlAuthorizationIndBit.BitState Then
                        myAuthorizationIcon.myBrush = New SolidBrush(Color.Green)
                        myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                    Else
                        If JointControlAuthorizationCtlBit.BitState Then
                            myAuthorizationIcon.myBrush = New SolidBrush(Color.Yellow)
                            If myAuthorizationIcon.InvokeRequired Then
                                myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                            Else
                                myAuthorizationIcon.Refresh()
                            End If
                        Else
                            myAuthorizationIcon.myBrush = New SolidBrush(Color.Red)
                            If myAuthorizationIcon.InvokeRequired Then
                                myAuthorizationIcon.Invalidate()
                                myAuthorizationIcon.Refresh()
                            Else
                                myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Invalidate))
                                myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                            End If
                        End If
                    End If
                    If BrotherSignalCollection IsNot Nothing Then
                        For Each oneSignal As Signal In BrotherSignalCollection
                            If JointControlAuthorizationIndBit.BitState Then
                                oneSignal.myAuthorizationIcon.myBrush = New SolidBrush(Color.Green)
                                oneSignal.myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf oneSignal.myAuthorizationIcon.Refresh))
                            Else
                                If JointControlAuthorizationCtlBit.BitState Then
                                    oneSignal.myAuthorizationIcon.myBrush = New SolidBrush(Color.Yellow)
                                    oneSignal.myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf oneSignal.myAuthorizationIcon.Refresh))
                                Else
                                    oneSignal.myAuthorizationIcon.myBrush = New SolidBrush(Color.Red)
                                    oneSignal.myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf oneSignal.myAuthorizationIcon.Refresh))
                                End If
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function GetSignalRoute(ByVal theSignal As Signal) As SignalRoute
            Dim theRoute As SignalRoute
            Try
                ' SystemEvent("GEOFENCE:GetSignalRoute:Signal-" & theSignal.GUID & " TRAIN:" & Me.Name)
                Dim comp As Object
                Dim theExitSignal As Signal = Nothing
                For Each comp In theSignal.SignalRouteCollection
                    If theSignal.ComponentType = "R" Then
                        Dim LeftSignal As Signal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(comp.RightLink)
                        If Not IsNothing(LeftSignal) Then
                            theExitSignal = LeftSignal
                            Exit For
                        End If
                    ElseIf theSignal.ComponentType = "L" Then
                        Dim RightSignal As Signal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(comp.LeftLink)

                        If Not IsNothing(RightSignal) Then
                            theExitSignal = RightSignal
                            Exit For
                        End If
                    End If

                Next
                If IsNothing(theExitSignal) Then
                    theExitSignal = FindTheExitSignal(theSignal)
                End If
                theRoute = FindSignalRoute(theSignal, theExitSignal)

                Return theRoute

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindTheExitSignal(ByVal theSignal As Signal) As Signal
            Try
                Dim theDirection As String = theSignal.ComponentType
                Dim NextComp As Object = Nothing
                Dim LoopControl As Integer

                Select Case theDirection
                    Case "L"
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(theSignal.LeftLink)
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(theSignal.RightLink)

                End Select


                Do Until LoopControl > 20
                    Select Case theDirection
                        Case "L"
                            If NextComp Is Nothing Then
                                SignalEventLog("FindTheExitSignal:NextComp is nothing")
                                Return Nothing
                            End If
                            If NextComp.GetType.ToString = gSignalComponentName Then
                                Dim sig As Signal = CType(NextComp, Signal)
                                Return sig
                            ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                                Dim sw As Switch = CType(NextComp, Switch)
                                If sw.LeftAltLink <> 0 Then
                                    If sw.ControlledReverse Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If
                            ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If

                        Case "R"
                            If NextComp Is Nothing Then
                                SignalEventLog("FindTheExitSignal:NextComp is nothing")
                                Return Nothing
                            End If
                            If NextComp.GetType.ToString = gSignalComponentName Then
                                Dim sig As Signal = CType(NextComp, Signal)
                                Return sig
                            ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                                Dim sw As Switch = CType(NextComp, Switch)
                                If sw.RightAltLink <> 0 Then
                                    If sw.ControlledReverse Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                End If
                            ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            End If
                    End Select
                    LoopControl += 1
                Loop
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindSignalRoute(ByVal EntrySignal As Signal, ByVal ExitSignal As Signal) As SignalRoute
            Try
                Dim sr As SignalRoute
                For Each sr In gSignalRouteList
                    If Not IsNothing(sr.EntrySignal) Then
                        If sr.EntrySignal.GUID = EntrySignal.GUID Then
                            If Not IsNothing(sr.ComponentList) AndAlso sr.ComponentList.Count > 0 Then
                                If sr.ComponentList.Contains(ExitSignal) Then
                                    Return sr
                                    'ElseIf EntrySignal.HoldingSignal Then
                                    '    Return sr
                                    'ElseIf sr.EntrySignal.AutomaticSignal Then
                                    '    Return sr
                                End If
                            End If
                        End If
                    End If
                Next
                For Each sr In gSignalRouteList
                    If Not IsNothing(sr.EntrySignal) Then
                        If sr.EntrySignal.GUID = EntrySignal.GUID Then
                            If Not IsNothing(sr.ComponentList) AndAlso sr.ComponentList.Count > 0 Then
                                If EntrySignal.HoldingSignal Then
                                    Return sr
                                ElseIf sr.EntrySignal.AutomaticSignal Then
                                    Return sr
                                End If
                            End If
                        End If
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Joint Control Authorization Request
        ''' </summary>
        ''' <param name="theSignal"></param>
        ''' <param name="theSymbol"></param>
        ''' <param name="isPSS"></param>
        Private Sub JointControlAuthorizationRequest(theSignal As Signal,
                                                     Optional theSymbol As String = "",
                                                     Optional isPSS As Boolean = False)
            Try
                Dim myRoute As SignalRoute = GetSignalRoute(theSignal)
                Dim theComponent As Object = FindExitTrackForRequests(myRoute)
                If myRoute Is Nothing Then
                    SignalEventLog($"{GetCurrentMethod.Name}:Signal Route is not found!")
                    Exit Sub
                End If
                If theComponent Is Nothing Then
                    SignalEventLog($"{GetCurrentMethod.Name}:Remote track NOT FOUND")
                    Exit Sub
                End If
                SendAuthorizationRequests(theComponent,
                                          theSignal,
                                          theSymbol,
                                          isPSS)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region

#Region "TUNNEL AUTOMATIC MODE"

        Public Sub mnuTunnelAutomaticMode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            DoTunnelAutomaticMode(False, TunnelAuto)
        End Sub

        Public Sub DoTunnelAutomaticMode(ByVal AutoSend As Boolean, ByVal TunnelAutoOn As Boolean)
            Try
                If TunnelAutoOn Then

                    Dim CompList As ArrayList = Me.RetreiveSignalRoute
                    Dim comp As Object
                    If Not IsNothing(CompList) Then
                        For Each comp In CompList
                            If comp.GetType.ToString = gTrackComponentName Then
                                Dim trk As Track = CType(comp, Track)
                                If trk.BlockAbsolute Then Exit Sub
                            End If
                        Next
                    End If

                    Me.JointControlAuthorizationCtlBit.BitState = True
                    If Not IsNothing(Me.JointControlAuthorizationCancelBit) Then
                        Me.JointControlAuthorizationCancelBit.BitState = False
                    End If
                Else
                    Me.JointControlAuthorizationCtlBit.BitState = False
                    If Not IsNothing(Me.JointControlAuthorizationCancelBit) Then
                        Me.JointControlAuthorizationCancelBit.BitState = True
                    End If
                End If

                If Not TunnelAutoOn Then

                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If

                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                Else
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = True
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = True
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = True
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = True
                    End If

                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = False
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If
                End If


                If Not IsNothing(Me.myAuthorizationIcon) Then
                    If Not JointControlAuthorizationIndBit.BitState Then
                        JointControlAuthorizationUpdateStatus()
                    End If
                End If

                Me.myControlPoint.FlasherMode(True)
                Me.AddToSendCollection("DoTunnelAutomaticMode", True)


                If Not AutoSend Then
                    gSendCTC = True
                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                Else
                    Dim DoSendControls As New SendCommandControls(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.SendControls)
                    DoSendControls.BeginInvoke(True, Nothing, Nothing)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "NEAR SIDE SIGNAL CONTROL"

        Public Sub mnuNearSideSignalControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Me.NearSideSignalOn Then
                    NearSideSignalOn = False
                Else
                    NearSideSignalOn = True
                End If
                NearSideSignalControlUpdateStatus()

                If Not IsNothing(Me.BrotherSignalCollection) Then
                    If Me.BrotherSignalCollection.Count > 0 Then
                        Dim s As Signal
                        For Each s In Me.BrotherSignalCollection
                            s.NearSideSignalOn = NearSideSignalOn
                            s.NearSideSignalControlUpdateStatus()
                        Next
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub NearSideSignalControlUpdateStatus()
            Try
                If Me.NearSideSignalOn Then
                    If Not IsNothing(Me.myControlArrow) Then
                        If Not IsNothing(Me.NearSideSignalCtlBit) Then
                            If Me.NearSideSignalCtlBit.BitState Then
                                Me.myControlArrow.myBrush = New SolidBrush(Color.GreenYellow)
                            Else
                                Me.myControlArrow.myBrush = New SolidBrush(Color.Red)
                            End If
                            Me.myControlArrow.Invoke(New MethodInvoker(AddressOf Me.myControlArrow.Refresh))
                        End If
                    End If
                Else
                    If Not IsNothing(Me.myControlArrow) Then
                        Me.myControlArrow.myBrush = New SolidBrush(Color.DarkGray)
                        If Not IsNothing(Me.NearSideSignalCtlBit) Then
                            If Me.NearSideSignalCtlBit.BitState Then
                                Me.NearSideSignalCtlBit.BitState = False
                                Me.FindCodeStationToSend()
                                Me.myControlPoint.SendCodeStationControls()
                            End If
                        End If
                        Me.myControlArrow.Invoke(New MethodInvoker(AddressOf Me.myControlArrow.Refresh))
                    End If
                End If

                If Not IsNothing(Me.myIndicationArrow) Then
                    If Not IsNothing(Me.NearSideSignalIndBit) Then
                        If Me.NearSideSignalIndBit.BitState Then
                            Me.myIndicationArrow.myBrush = New SolidBrush(Color.GreenYellow)
                        Else
                            Me.myIndicationArrow.myBrush = New SolidBrush(Color.DarkGray)
                        End If
                        Me.myIndicationArrow.Invoke(New MethodInvoker(AddressOf Me.myIndicationArrow.Refresh))
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#End Region

#Region "MOUSE EVENTS"

        ''' <summary>
        ''' Mouse over tooltip
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Sub Signal_MouseHover(ByVal sender As Object,
                                     ByVal e As EventArgs)
            Try
                Dim theMessage As String = String.Empty
                If optEnableShowSignalNameOnToolTip Then
                    If optEnableUsingTooltipField Then
                        theMessage = myToolTip
                        If optTrackToolTipFormatWithControlPointMilepost Then
                            theMessage &= $"({Name.ToUpper}) ({myControlPoint.Name}) ({myMilePost})"
                        End If
                    Else
                        If optTrackToolTipFormatWithControlPointMilepost Then
                            theMessage &= $"({Name.ToUpper}) ({myControlPoint.Name}) ({myMilePost})"
                        Else
                            theMessage &= Name.ToUpper
                        End If
                    End If
                End If
                If AutomaticModeOn Then
                    theMessage &= $" <AUTOROUTING>"
                End If
                CurrentDrawingForm.TrackToolTip.SetToolTip(Me, theMessage)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            Dim button As Short = e.Button \ &H100000
            Try
                Me.SignalEventLog("CLICKED - MOUSE DOWN SIGNAL (BUTTON=" & IIf(button = 1, "LEFT", "RIGHT") & ")")
                If (gAssistMode OrElse Me.IsShadowMode) AndAlso Not (gTrainerMode Or gCreateCPEMT Or gCreateCPTNT Or gCreateCPFT) Then
                    Exit Sub
                End If
                If Me.IndicateOnly Then
                    SignalEventLog("SIGNAL INDICATE ONLY")
                    SignalIndicateOnlyeAlert()
                    ClearSystemVariables()
                    Exit Sub
                End If
                If Me.DoMonitorMode Then
                    SignalEventLog("SIGNAL IN MONITOR MODE")
                    Exit Sub
                End If
                If gCPTNTRollUpForMow Or gCPTNTRollUpForTrain Or gCreateCPEMT Or gCreateCPTNT Or gCreateCPFT Then Exit Sub
                If gPermissionPassRedSignal OrElse gCreateTrainPassRed564 Then
                    ContextMenuEnabled(False)
                    RefreshContextMenu()
                    Exit Sub
                End If
                If Me.myControlPoint.MaintenanceMode And Not gMaintenanceMode Then
                    AddMessageToFeedback("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MAINTENANCE MODE")
                    Exit Sub
                End If
                If gOptDisableSignalControlOnManualOperateMode Then
                    If Not gMaintenanceMode Then
                        If optEnableARRCSpecificOptions Then
                            If Not ValidateManualOperateMode(Me.myControlPoint) Then
                                Me.SignalEventLog("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE " & Me.GUID.ToString)
                                MessageBox.Show("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT." & vbCrLf & Me.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE ", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Sub
                            Else
                                ContextMenuEnabled(False)
                                RefreshContextMenu()
                            End If
                        Else
                            If Me.myControlPoint.ManualOperateMode And Not gMaintenanceMode Then
                                'AddMessageToFeedback("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE")
                                Me.SignalEventLog("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE " & Me.GUID.ToString)
                                MessageBox.Show("SIGNAL CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT." & vbCrLf & Me.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE ", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Sub
                            ElseIf Me.myControlPoint.FailState Then
                                ContextMenuEnabled(False)
                                RefreshContextMenu()
                            End If
                        End If
                    End If
                End If

                If Me.MakeInvisible Then Exit Sub

                If gTrainerMode Then
                    Exit Sub
                End If

                Dim EventMessage As String = "SIGNAL BEING SELECTED. (STATUS:" & Me.SignalCtlStatus.ToString.ToUpper & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                Me.SignalEventLog(EventMessage)

                If gCritcalPromptExists Then
                    AddMessageToFeedback("CRITICAL PROMPT EXISTS: " & Me.Name & " (" & Me.GUID.ToString & ")")
                    Exit Sub
                End If

                If Me.DoMonitorMode Then
                    AddMessageToFeedback("MONITOR MODE: " & Me.Name & " (" & Me.GUID.ToString & ")")
                    Exit Sub
                End If

                If gBlockAllDevices Then

                    If Me.FleetEnabled Then
                        Exit Sub
                    End If

                    If Me.SignalStatus = BulbStatus.StopStatus Or Me.SignalStatus = BulbStatus.InTime Then
                        Dim sr As StoredRoute
                        For Each sr In Me.myControlPoint.StoredRoutes
                            Dim comp As Object
                            For Each comp In sr.ComponentSelection
                                If comp Is Me Then
                                    Exit Sub
                                End If
                            Next
                        Next

                        If Me.StoredEntrySignal = True Then
                            Exit Sub
                        End If
                        Me.DoDeviceBlock()
                        Exit Sub
                    Else
                        Exit Sub
                    End If
                End If
                If FailState And Not AutomaticSignal And Not gSimulationMode Then
                    AddMessageToFeedback("SIGNAL IN FAIL: " & Me.Name & " (" & Me.GUID.ToString & ")")
                    Exit Sub
                End If
                If e.Button = Windows.Forms.MouseButtons.Left Then
                    If AutomaticSignal Then
                        If optEnableLicenseGenerationForPTC AndAlso
                            gCancelLicenseToTrain Then
                            If gCreateLicenseTrain IsNot Nothing Then gCreateLicenseTrain.PtcCancelNormalLicense(Me)
                            ClearSystemVariables()
                            Exit Sub
                        End If
                        Exit Sub
                    End If
                    If myControlPoint.TunnelAutomaticControlled Then
                        Exit Sub
                    End If
                    If Me.DrillMode Then
                        AddMessageToFeedback("SIGNAL IN DRILL: " & Me.Name & " (" & Me.GUID.ToString & ")")
                        MessageBox.Show(Me.CurrentDrawingForm, "SIGNAL CURRENTLY DRILLED.", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
                        Exit Sub
                    End If
                    NoSignalControlInitialization = False
                    Select Case SignalCtlStatus
                        Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                            If optEnableLicenseGenerationForPTC AndAlso
                                gCancelLicenseToTrain Then
                                If gCreateLicenseTrain IsNot Nothing Then gCreateLicenseTrain.PtcCancelNormalLicense(Me)
                                ClearSystemVariables()
                                Exit Sub
                            End If
                            If SignalIndStatus = IndStatus.Cleared Then
                                Dim SignalDialog As New frmSignalDialog()
                                If optEnableARRCSpecificOptions Then
                                    If Me.CurrentDrawingForm IsNot Nothing Then SignalDialog.Owner = Me.CurrentDrawingForm
                                    SignalDialog.TopMost = True
                                End If
                                SignalDialog.SetUpProperties(Me)
                                SignalDialog.ShowDialog()
                                Dim UserResponse As Boolean = SignalDialog.UserResponse
                                SignalDialog.Dispose()
                                SignalDialog = Nothing
                                If UserResponse Then
                                    gSendCTC = True
                                    Me.myControlPoint.FlasherMode(True)
                                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                                    If Not Me.DrillMode Then
                                        Me.ControlSignalToStop()
                                    Else
                                        Me.ControlSignalToDrillStop()
                                    End If
                                    If optCancelStoredRouteWhenSignalStopRequested Then
                                        PerformSRandFleetCancelOnBlocking(Me.SignalRouteCollection, True)
                                    End If
                                Else
                                    Exit Sub
                                End If
                            ElseIf optEnablePromptToCancelRequestingClear And Me.SignalIndStatus = IndStatus.Stopped Then
                                Dim SignalDialog As New frmSignalDialog()
                                SignalDialog.SetUpProperties(Me)
                                SignalDialog.ShowDialog()
                                Dim UserResponse As Boolean = SignalDialog.UserResponse
                                SignalDialog.Dispose()
                                SignalDialog = Nothing
                                If UserResponse Then
                                    Me.myControlPoint.FlasherMode(True)
                                    gSendCTC = True
                                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                                    If Not Me.DrillMode Then
                                        Me.ControlSignalToStop()
                                        If optEnableCallOnLatching Then
                                            If Me.SignalCalledOn Then
                                                Me.SignalCalledOn = False
                                                If Not IsNothing(Me.CallOnControl) Then
                                                    Me.CallOnControl.BitState = False
                                                End If
                                            End If
                                        End If
                                    Else
                                        Me.ControlSignalToDrillStop()
                                    End If
                                Else
                                    Exit Sub
                                End If

                            Else
                                'gSendCTC = True
                                Me.myControlPoint.FlasherMode(True)
                                gSendCTC = True
                                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                                If Not Me.DrillMode Then
                                    Me.ControlSignalToStop()
                                Else
                                    Me.ControlSignalToDrillStop()
                                End If
                            End If

                        Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                            If optEnableLicenseGenerationForPTC Then
                                If gCancelLicenseToTrain Then
                                    ClearSystemVariables()
                                    Exit Sub
                                End If
                            End If

                            'Allows cancel unsolited signals
                            If Me.SignalStatus = BulbStatus.Unsolitated Then
                                Dim SignalDialog As New frmSignalDialog()
                                SignalDialog.SetUpProperties(Me)
                                SignalDialog.ShowDialog()
                                Dim UserResponse As Boolean = SignalDialog.UserResponse
                                SignalDialog.Dispose()
                                SignalDialog = Nothing
                                If UserResponse Then
                                    gSendCTC = True
                                    Me.myControlPoint.FlasherMode(True)
                                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                                    If Not Me.DrillMode Then
                                        SignalEventLog("OnMouseDown:1:Cancel unSolicited signal")
                                        Me.ControlSignalToStop()
                                    Else
                                        SignalEventLog("OnMouseDown:2:Cancel unSolicited signal")
                                        Me.ControlSignalToDrillStop()
                                    End If
                                Else
                                    Exit Sub
                                End If
                            End If

                            If Me.SignalStatus = BulbStatus.OfficeLock Then
                                AddMessageToFeedback("SIGNAL IN OFFICE LOCK: " & Me.Name & " (" & Me.GUID.ToString & ")")
                                Exit Sub
                            End If
                            If Me.BlockAbsolute Then
                                AddMessageToFeedback("SIGNAL BLOCKED: " & Me.Name & " (" & Me.GUID.ToString & ")")
                                Exit Sub
                            End If
                            If OptEnableSignalClearByMenu Then
                                If Me.SignalCtlStatus = CtlStatus.RequestingClear Then

                                Else
                                    Exit Sub
                                End If
                            End If


                            If Not Me.ControlSignalToClear(True, True, False, False, False, False, False, True, False, False, True) Then
                                Exit Sub
                            Else
                                Me.myControlPoint.FlasherMode(True)
                                gSendCTC = True
                                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)

                                If optEnableCallOnLatching Then
                                    If Me.SignalCalledOn Then
                                        Me.SignalCalledOn = False
                                        If Not IsNothing(Me.CallOnControl) Then
                                            Me.CallOnControl.BitState = False
                                        End If
                                    End If
                                End If
                            End If

                    End Select

                    EventMessage = "SIGNAL SELECTED. (STATUS:" & Me.SignalCtlStatus.ToString.ToUpper & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")"
                    Me.SignalEventLog(EventMessage)

                Else
                    ContextMenuEnabled(True)
                    RefreshContextMenu()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                button = Nothing
            End Try
        End Sub

        Private Sub DoDeviceBlock()
            Try

                If Not gDeviceBlockSelection.Contains(Me) Then

                    gDeviceBlockSelection.Add(Me)
                    Me.SelectionMode = ModeSelection.DeviceBlocking
                    Me.FlashComponent = True
                    TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                Else

                    gDeviceBlockSelection.Remove(Me)
                    Me.SelectionMode = ModeSelection.Regular
                    Me.FlasherMode(False)
                    TMDS.SystemForms.MySystemForms.frmMainMDI.DeviceBlockFlasherReset()
                    Me.UpdateStatus()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "Signal Enumerators"

        Public Enum CtlStatus
            RequestedStop
            RequestedClear
            RequestingStop
            RequestingClear
        End Enum

        Public Enum IndStatus
            Stopped
            Cleared
            Unsolitated
            InTime
            InTimeNoControlledSignal
            Drilled
        End Enum

        Public Enum BulbStatus
            RequestingStop
            RequestingClear
            ClearStatus
            StopStatus
            InTime
            Failed
            Unsolitated
            InTimeNoControlledSignal
            OfficeLock
        End Enum

        Public Enum TypeOfInTime
            Opposing
            LockBit
            InTimeBit
        End Enum

        Public Enum TypeOfDrill
            Controlled
            NonControlled
            None
        End Enum

        'Signal Control & Indication
        '
        '_________________________________________________________________
        '       | REQUESTED STOP    |  REQUESTED CLEAR     
        '-------|-------------------|-------------------------------------
        'STOP   | (STOP)            |  (REQUESTING CLEAR)
        '       |-------------------|-------------------------------------
        'CLEAR  | (REQUESTING STOP) |  (CLEAR)
        '       |-------------------|-------------------------------------   
        'IN-TIME| (IN-TIME)         |  (REQUESTING CLEAR)
        '_________________________________________________________________
        '
        'INDICATIONS ARE VERITICAL
        'CONTROLS ARE HORIZONTAL
        'BRACKETS ARE THE RESULTING (SIGNAL STATUS)

#End Region

#Region "TRAINER/TRAINEE MODE"

        Private Sub SimulateElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)

            Try
                Me.myControlPoint.FlashName()

                If Me.SignalCtlStatus = CtlStatus.RequestedClear Then
                    If Not Me.Ind(0) Is Nothing AndAlso Not Me.Ind(1) Is Nothing Then
                        Me.Ind(0).BitState = True
                        Me.Ind(1).BitState = True
                        Me.LastCtlStatus = CtlStatus.RequestedClear
                    ElseIf Not Me.Ind(0) Is Nothing Then
                        'Me.Ind(0).Status = True
                        Me.Ind(0).BitState = True
                        Me.LastCtlStatus = CtlStatus.RequestedClear
                    End If
                    If Not IsNothing(Me.NearSideSignalIndBit) Then
                        Me.NearSideSignalIndBit.BitState = True
                    End If
                Else
                    If Not Me.Ind(0) Is Nothing AndAlso Not Me.Ind(1) Is Nothing Then
                        Me.Ind(0).BitState = False
                        Me.Ind(1).BitState = False
                        Me.LastCtlStatus = CtlStatus.RequestedStop
                    ElseIf Not Me.Ind(0) Is Nothing Then
                        'Me.Ind(0).Status = False
                        Me.Ind(0).BitState = False
                        Me.LastCtlStatus = CtlStatus.RequestedStop
                    End If
                    If Not IsNothing(Me.InTimeBit) Then
                        If Me.InTimeBit.BitState Then
                            Me.InTimeBit.BitState = False
                            IndSimulator.Interval = 4000
                        Else
                            Me.InTimeBit.BitState = True
                            IndSimulator.Interval = 6000
                        End If
                    End If
                    If Not IsNothing(Me.NearSideSignalIndBit) Then
                        Me.NearSideSignalIndBit.BitState = False
                    End If

                End If
                Me.ApplyIndicationChange()
                Me.UpdateStatus()

                If Not IsNothing(Me.InTimeBit) Then
                    If Me.InTimeBit.BitState Then
                        Me.IndSimulator.Enabled = True
                    Else
                        Me.IndSimulator.Enabled = False
                    End If
                Else
                    Me.IndSimulator.Enabled = False
                End If

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
                Message = Message & "|SG"
                Message = Message & "|" & IndCommand
                Message = Me.TerritoryID.ToString & Chr(12) & Message
                TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overrides Sub SendTraineeStatusUpdate()
            Try
                Dim Message As String = "TEC" & Chr(6) & "COMPSTATUS;"
                Message = Message & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                Message = Message & "|SIG"
                Message = Message & "|CTL=" & Me.SignalCtlStatus.ToString
                Message = Message & "|IND=" & Me.SignalIndStatus.ToString
                Message = Message & "|SS=" & Me.SignalStatus.ToString
                Message = Message & "|DB=" & Me.DeviceBlocked
                Message = Message & "|DC=" & Me.DrillMode
                Message = Message & "|DI=" & Me.DrillIndicatedOn
                Message = Me.TerritoryID.ToString & Chr(12) & Message
                TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "MOVEMENT PLANNING"

        Public Sub ToggleAutoRouting(ByVal AutoOn As Boolean, ByVal OriginOfCommand As String)
            Try
                Dim KeyWord As String = "DISBALED"

                If Me.AllowAutomaticMode Then
                    Me.AutomaticModeOn = AutoOn
                    If AutoOn Then KeyWord = "ENABLED"
                    Dim sigUpdate As String = "UPDATE tblCompSignals SET AutomaticModeOn = " & CInt(AutoOn) & " WHERE UID = " & Me.GUID
                    SystemDatabaseCommandToStatic("SIGNAL:ToggleAutoRouting", sigUpdate)
                    Me.SignalEventLog("AUTO-ROUTING HAS BEEN " & KeyWord & " FROM THE " & OriginOfCommand & " FOR SIGNAL <" & Me.GUID.ToString.PadLeft(6, "0") & "><" & Me.Name & ">")
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
#End Region

#Region "GRAPHICS"
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
        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)


            MyBase.OnPaint(e)
            Try
                If CurrentDrawingForm.Initializing Then Exit Sub

                If Me.MakeInvisible Then Exit Sub
                If Not gDisplaySignals Then Exit Sub
                If IsNothing(Graphical) Then
                    Graphical = Me.CurrentDrawingForm.CreateGraphics
                End If

                SyncLock Graphical
                    SyncLock GraphicPathBulb
                        If FlashComponent Then
                            If FlashOn Then
                                If SelectionMode = ModeSelection.DeviceBlocking Then
                                    e.Graphics.FillPath(ColorStatusBlocking, GraphicPathBulb)
                                ElseIf SelectionMode = ModeSelection.RouteRequesting Then
                                    e.Graphics.FillPath(ColorStatusRouteRequesting, Me.GraphicPathBulb)
                                ElseIf SelectionMode = ModeSelection.SummarySelectionBlocking Then
                                    e.Graphics.FillPath(ColorSummarySelectionBlocking, GraphicPathBulb)
                                ElseIf SelectionMode = ModeSelection.SummarySelectionRouting Then
                                    e.Graphics.FillPath(Me.ColorSummarySelectionRequesting, GraphicPathBulb)
                                Else
                                    e.Graphics.FillPath(CurrentBulbColorStatus, GraphicalPath)
                                End If
                            Else
                                e.Graphics.FillPath(FlashOffColorStatus, GraphicPathBulb)
                            End If
                        Else
                            e.Graphics.FillPath(CurrentBulbColorStatus, GraphicPathBulb)
                        End If
                        If Me.AutomaticSignal Then
                            e.Graphics.FillPath(Brushes.LightGray, GraphicPathBulb)
                        End If
                    End SyncLock
                    SyncLock GraphicPathStem
                        If Me.StoredEntrySignal Then
                            e.Graphics.FillPath(New SolidBrush(Color.Yellow), Me.PathArrowEntry)
                            e.Graphics.FillPath(New SolidBrush(Color.Yellow), Me.gStemPath)
                        ElseIf Me.StoredExitSignal Then
                            GraphicPathStem = Me.gStemPath
                            e.Graphics.DrawPath(New Pen(Color.Yellow, 2), Me.gStemPath)
                        ElseIf Me.SLFAEntrySignal Then
                            e.Graphics.FillPath(New SolidBrush(Color.Aqua), Me.PathArrowEntry)
                            e.Graphics.FillPath(New SolidBrush(Color.Aqua), Me.gStemPath)
                        ElseIf Me.SLFAExitSignal Then
                            GraphicPathStem = Me.gStemPath
                            e.Graphics.DrawPath(New Pen(Color.Aqua, 2), Me.gStemPath)
                        ElseIf Me.DisableStoreRouteFunction Or Me.DisableEntryExitFunction Then
                            e.Graphics.DrawPath(New Pen(Color.AntiqueWhite, 2), Me.gStemPath)
                        ElseIf Me.IsStopTargetApplied Then
                            GraphicPathStem = Me.gStemPath
                            e.Graphics.DrawPath(New Pen(gDeviceBlockColor, 2), Me.gStemPath)
                        ElseIf Me.FleetEnabled Then
                            GraphicPathStem = Me.gStemPath
                            e.Graphics.DrawPath(New Pen(Color.LightGreen, 2), GraphicPathStem)
                        Else
                            If Me.AllowAutomaticMode And Me.AutomaticModeOn And (Not Me.FailState Or optOverrideSimulationMode) Then
                                e.Graphics.FillPath(New SolidBrush(Color.Black), Me.PathArrowEntry)
                                GraphicPathStem = Me.gStemPath
                                e.Graphics.FillPath(New SolidBrush(Color.Gold), GraphicPathStem)
                            Else
                                e.Graphics.FillPath(New SolidBrush(Color.Black), Me.PathArrowEntry)
                                e.Graphics.FillPath(CurrentStemColorStatus, Me.gStemPath)
                            End If
                        End If
                    End SyncLock
                End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub DrawIt()
            Dim g As Graphics
            Try

                If Me.MakeInvisible Then Exit Sub
                If IsNothing(Graphical) Then
                    Graphical = Me.CurrentDrawingForm.CreateGraphics
                End If
                g = Graphical
                SyncLock Graphical
                    SyncLock GraphicPathBulb
                        If FlashComponent Then
                            If FlashOn Then
                                g.FillPath(CurrentBulbColorStatus, GraphicPathBulb)
                            Else
                                g.FillPath(FlashOffColorStatus, GraphicPathBulb)
                            End If
                        Else
                            g.FillPath(CurrentBulbColorStatus, GraphicPathBulb)
                        End If
                    End SyncLock
                    SyncLock GraphicPathStem
                        If Me.StoredEntrySignal Then
                            'GraphicPathStem = Me.PathArrowEntry
                            g.FillPath(New SolidBrush(Color.Yellow), GraphicPathStem)
                            g.FillPath(New SolidBrush(Color.Yellow), Me.gStemPath)

                        ElseIf Me.StoredExitSignal Then
                            GraphicPathStem = Me.gStemPath
                            g.DrawPath(New Pen(Color.Yellow, 2), GraphicPathStem)
                        ElseIf Me.FleetEnabled Then
                            GraphicPathStem = Me.gStemPath
                            g.DrawPath(New Pen(Color.LightGreen, 2), GraphicPathStem)
                        Else
                            'GraphicPathStem = Me.gStemPath
                            g.FillPath(CurrentStemColorStatus, Me.gStemPath)
                        End If
                    End SyncLock
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub RefreshPath()
            Try
                Me.Visible = False
                Path = New GraphicsPath()


                ''** Set graphical path for track without traffic
                Dim pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8 As Point

                'Draw a right stem arrow
                Me.PathArrowEntry = New GraphicsPath()

                ''**********************************************************************************************************
                If Me.ComponentType = "R" Then
                    DefaultTraffic = "RIGHT"

                    Me.Left = LX - 2
                    Me.Top = LY - 1
                    Me.Width = 20
                    Me.Height = 15
                    LX = +2
                    LY = 4

                    'push the base away from the track
                    'Me.LY = Me.LY + 3

                    gStemPath.StartFigure()
                    'Base
                    pt1 = New Point(LX, LY)
                    pt2 = New Point(LX, LY + 10)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX, LY + 10)
                    pt4 = New Point(LX - 2, LY + 10)
                    gStemPath.AddLine(pt3, pt4)

                    pt5 = New Point(LX - 2, LY + 10)
                    pt6 = New Point(LX - 2, LY)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX - 2, LY)
                    pt8 = New Point(LX, LY)
                    gStemPath.AddLine(pt7, pt8)

                    'Stem
                    pt1 = New Point(LX, LY + 4)
                    pt2 = New Point(LX + 5, LY + 4)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX + 5, LY + 4)
                    pt4 = New Point(LX + 5, LY + 6)
                    gStemPath.AddLine(pt3, pt4)

                    pt5 = New Point(LX + 5, LY + 6)
                    pt6 = New Point(LX, LY + 6)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX, LY + 6)
                    pt8 = New Point(LX, LY + 4)
                    gStemPath.AddLine(pt7, pt8)

                    gStemPath.CloseFigure()
                    GraphicPathStem = gStemPath

                    'Draw a right stem arrow for entry
                    Me.PathArrowEntry = New GraphicsPath()
                    PathArrowEntry.StartFigure()
                    'Base
                    pt1 = New Point(LX - 2, LY - 2)
                    pt2 = New Point(LX - 2, LY + 12)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX - 2, LY + 12)
                    pt2 = New Point(LX + 5, LY + 6)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 5, LY + 6)
                    pt2 = New Point(LX + 5, LY + 4)
                    PathArrowEntry.AddLine(pt1, pt2)


                    pt1 = New Point(LX + 5, LY + 4)
                    pt2 = New Point(LX - 2, LY - 2)
                    PathArrowEntry.AddLine(pt1, pt2)
                    PathArrowEntry.CloseFigure()


                    gBulbPath.StartFigure()
                    Bulb = New RectangleF(Me.LX + 5.0, Me.LY - 1, 11.5, 11.5)
                    gBulbPath.AddEllipse(Bulb)
                    gBulbPath.CloseFigure()
                    GraphicalPath = gBulbPath
                    GraphicPathBulb = gBulbPath
                    If Me.JointControlAuthorizationEnabled Then
                        myAuthorizationIcon = New SignalAuthorization(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myAuthorizationIcon)
                        Dim ShowIcon As Boolean = True
                        If Me.myControlPoint.TunnelAutomaticMode Then
                            ShowIcon = Me.myControlPoint.TunnelAutomaticControlled
                        End If
                        myAuthorizationIcon.Visible = ShowIcon
                        myAuthorizationIcon.BringToFront()
                    End If
                    If Me.NearSideSignalEnabled Then
                        myControlArrow = New SignalAuthorizationControlArrow(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myControlArrow)
                        myControlArrow.BringToFront()
                        myControlArrow.mySignal = Me

                        myIndicationArrow = New SignalAuthorizationIndicationArrow(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myIndicationArrow)
                        myIndicationArrow.BringToFront()
                    End If

                ElseIf Me.ComponentType = "L" Then
                    DefaultTraffic = "LEFT"
                    Me.Left = LX - 20
                    Me.Top = LY - 5
                    Me.Width = 20
                    Me.Height = 15
                    LX = 18
                    LY = 11


                    gStemPath.StartFigure()
                    'Base
                    pt1 = New Point(LX, LY)
                    pt2 = New Point(LX, LY - 10)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX, LY - 10)
                    pt4 = New Point(LX + 2, LY - 10)
                    gStemPath.AddLine(pt3, pt4)

                    pt5 = New Point(LX + 2, LY - 10)
                    pt6 = New Point(LX + 2, LY)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX + 2, LY)
                    pt8 = New Point(LX, LY)
                    gStemPath.AddLine(pt7, pt8)

                    'Stem
                    pt1 = New Point(LX, LY - 4)
                    pt2 = New Point(LX - 6, LY - 4)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX - 6, LY - 4)
                    pt4 = New Point(LX - 6, LY - 6)
                    gStemPath.AddLine(pt3, pt4)

                    pt5 = New Point(LX - 6, LY - 6)
                    pt6 = New Point(LX, LY - 6)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX, LY - 6)
                    pt8 = New Point(LX, LY - 4)
                    gStemPath.AddLine(pt7, pt8)

                    gStemPath.CloseFigure()
                    GraphicPathStem = gStemPath


                    'Draw a LEFT stem arrow for entry
                    Me.PathArrowEntry = New GraphicsPath()
                    PathArrowEntry.StartFigure()
                    'Base
                    pt1 = New Point(LX + 2, LY + 1)
                    pt2 = New Point(LX + 2, LY - 12)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 2, LY - 12)
                    pt2 = New Point(LX - 5, LY - 7)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX - 5, LY - 7)
                    pt2 = New Point(LX - 5, LY - 4)
                    PathArrowEntry.AddLine(pt1, pt2)


                    pt1 = New Point(LX - 5, LY - 4)
                    pt2 = New Point(LX + 2, LY + 1)
                    PathArrowEntry.AddLine(pt1, pt2)
                    PathArrowEntry.CloseFigure()


                    gBulbPath.StartFigure()
                    Bulb = New RectangleF(Me.LX - 17.7, Me.LY - 11.25, 11.5, 11.5)
                    gBulbPath.AddEllipse(Bulb)
                    gBulbPath.CloseFigure()
                    GraphicalPath = gBulbPath
                    GraphicPathBulb = gBulbPath

                    If Me.JointControlAuthorizationEnabled Then
                        myAuthorizationIcon = New SignalAuthorization(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myAuthorizationIcon)
                        Dim ShowIcon As Boolean = True
                        If Me.myControlPoint.TunnelAutomaticMode Then
                            ShowIcon = Me.myControlPoint.TunnelAutomaticControlled
                        End If
                        myAuthorizationIcon.Visible = ShowIcon
                    End If

                    If Me.NearSideSignalEnabled Then
                        myControlArrow = New SignalAuthorizationControlArrow(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myControlArrow)
                        myControlArrow.BringToFront()
                        myControlArrow.mySignal = Me

                        myIndicationArrow = New SignalAuthorizationIndicationArrow(Me.Left, Me.Top, Me.ComponentType)
                        Me.CurrentDrawingForm.Controls.Add(Me.myIndicationArrow)
                        myIndicationArrow.BringToFront()
                    End If


                ElseIf Me.ComponentType = "D" Then
                    DefaultTraffic = "RIGHT"
                    'push the base away from the track
                    Me.Left = LX - 2
                    Me.Top = LY - 5
                    Me.Width = 20
                    Me.Height = 25
                    LX = 2
                    LY = 4

                    LX = LX + 2
                    'ly = ly + 2

                    gStemPath.StartFigure()
                    'Base
                    pt1 = New Point(LX, LY)
                    pt2 = New Point(LX, LY + 3)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX + 4, LY + 3)
                    gStemPath.AddLine(pt2, pt3)

                    pt4 = New Point(LX + 4, LY + 9)
                    gStemPath.AddLine(pt3, pt4)


                    pt5 = New Point(LX + 8, LY + 9)
                    gStemPath.AddLine(pt4, pt5)

                    pt6 = New Point(LX + 8, LY + 3)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX + 12, LY + 3)
                    gStemPath.AddLine(pt6, pt7)

                    pt8 = New Point(LX + 12, LY)
                    gStemPath.AddLine(pt7, pt8)

                    gStemPath.AddLine(pt8, pt1)
                    gStemPath.CloseFigure()
                    GraphicPathStem = gStemPath


                    'Draw a DOWN stem arrow for entry
                    Me.PathArrowEntry = New GraphicsPath()
                    PathArrowEntry.StartFigure()
                    'Base
                    pt1 = New Point(LX + 4, LY + 9)
                    pt2 = New Point(LX + 8, LY + 9)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 8, LY + 9)
                    pt2 = New Point(LX + 12, LY)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 12, LY)
                    pt2 = New Point(LX, LY)
                    PathArrowEntry.AddLine(pt1, pt2)


                    pt1 = New Point(LX, LY)
                    pt2 = New Point(LX + 4, LY + 9)
                    PathArrowEntry.AddLine(pt1, pt2)
                    PathArrowEntry.CloseFigure()


                    gBulbPath.StartFigure()
                    Bulb = New RectangleF(Me.LX, Me.LY + 9, 12, 12)
                    gBulbPath.AddEllipse(Bulb)
                    gBulbPath.CloseFigure()

                    GraphicalPath = gBulbPath
                    GraphicPathBulb = gBulbPath

                ElseIf Me.ComponentType = "U" Then
                    DefaultTraffic = "LEFT"

                    Me.Left = LX - 2
                    Me.Top = LY - 2
                    Me.Width = 20
                    Me.Height = 27
                    LX = 2
                    LY = 4

                    'push the base away from the track
                    'lx = lx + 2
                    LY = LY + 2

                    gStemPath.StartFigure()
                    'Base
                    pt1 = New Point(LX, LY + 21)
                    pt2 = New Point(LX, LY + 18)
                    gStemPath.AddLine(pt1, pt2)

                    pt3 = New Point(LX + 4, LY + 18)
                    gStemPath.AddLine(pt2, pt3)

                    pt4 = New Point(LX + 4, LY + 12)
                    gStemPath.AddLine(pt3, pt4)


                    pt5 = New Point(LX + 8, LY + 12)
                    gStemPath.AddLine(pt4, pt5)

                    pt6 = New Point(LX + 8, LY + 18)
                    gStemPath.AddLine(pt5, pt6)

                    pt7 = New Point(LX + 12, LY + 18)
                    gStemPath.AddLine(pt6, pt7)

                    pt8 = New Point(LX + 12, LY + 21)
                    gStemPath.AddLine(pt7, pt8)

                    gStemPath.AddLine(pt8, pt1)
                    gStemPath.CloseFigure()
                    GraphicPathStem = gStemPath

                    'Draw a UP stem arrow for entry
                    Me.PathArrowEntry = New GraphicsPath()
                    PathArrowEntry.StartFigure()
                    'Base
                    pt1 = New Point(LX, LY + 21)
                    pt2 = New Point(LX + 4, LY + 12)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 4, LY + 12)
                    pt2 = New Point(LX + 8, LY + 12)
                    PathArrowEntry.AddLine(pt1, pt2)

                    pt1 = New Point(LX + 8, LY + 12)
                    pt2 = New Point(LX + 12, LY + 21)
                    PathArrowEntry.AddLine(pt1, pt2)


                    pt1 = New Point(LX + 12, LY + 21)
                    pt2 = New Point(LX, LY + 21)
                    PathArrowEntry.AddLine(pt1, pt2)
                    PathArrowEntry.CloseFigure()



                    gBulbPath.StartFigure()
                    Bulb = New RectangleF(Me.LX, Me.LY, 12, 12)
                    gBulbPath.AddEllipse(Bulb)
                    gBulbPath.CloseFigure()
                    GraphicalPath = gBulbPath
                    GraphicPathBulb = gBulbPath

                End If

                Path = New GraphicsPath()
                Path.StartFigure()
                Path.AddPath(gStemPath, True)
                Path.AddPath(gBulbPath, True)
                Path.CloseFigure()
                GraphicalPath = Path

                ' Me.Region = New Region(GraphicalPath)



                'build paths for bitmap imaging
                Me.gBulbPathRelative = Me.gBulbPath.Clone
                Me.gStemPathRelative = Me.gStemPath.Clone
                Me.PathArrowEntryRelative = Me.PathArrowEntry.Clone
                Me.PathArrowExitRelative = Me.gStemPath.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.Left, Me.Top)
                Me.PathArrowEntryRelative.Transform(mat)
                Me.PathArrowExitRelative.Transform(mat)
                Me.gBulbPathRelative.Transform(mat)
                Me.gStemPathRelative.Transform(mat)


                Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub FlashElapsedEvent(ByVal source As Object, ByVal e As EventArgs)
            Try
                If FlashOn Then
                    FlashOn = False
                Else
                    FlashOn = True
                End If
                Flasher.Enabled = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub IndicationSuppressElapsedEvent(ByVal source As Object, ByVal e As EventArgs)
            Try
                ' If Me.SignalSlotting OrElse Me.InTimeType <> SignalComponents.Signal.TypeOfInTime.InTimeBit OrElse Me.DoMonitorMode Then
                Me.ApplyIndicationChange(False)
                Me.UpdateStatus()
                'End If

                'If Me.SignalSlotting Then
                '    Me.ApplyIndicationChange()
                'End If
                SystemEvent("SIGNAL IndicationSuppressElapsedEvent:" & Me.GUID)
                Me.IndicationSuppressTimer.Enabled = False
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                Me.SignalIndicationSupressionStatus = String.Empty
            End Try
        End Sub

        Public Overrides Sub BuildGraphics()
            RefreshPath()
            Refresh()
            Me.Graphical = Me.CreateGraphics
        End Sub

        Overrides Sub InitGraphics()
            If Me.MakeInvisible Then Exit Sub
            If FailState And Not Me.SimulateMode And Not optDisableShowingCodeFailureOnComponents Then
                CurrentBulbColorStatus = gColorFailedState
            ElseIf AuthorityType = "CTC-PSEUDO" Then
                CurrentBulbColorStatus = New SolidBrush(Color.SlateGray)  'ADDED FOR CTC DEVELOPMENT
            ElseIf AuthorityType = "CTC" Then
                CurrentBulbColorStatus = New SolidBrush(Color.Red)
            ElseIf AuthorityType = "NON-TWC" Then
                CurrentBulbColorStatus = New SolidBrush(Color.DarkGray)
            Else
                CurrentBulbColorStatus = gColorFailedState
            End If

            Me.CurrentColorStatus = CurrentBulbColorStatus

        End Sub

        Sub CheckForBrotherSignals()
            Try
                Dim comp As Object
                For Each comp In TMDS.GlobalDeclarations.SignalCollection

                    If comp.Name = Me.Name And comp.StationNumber = Me.StationNumber And comp.GUID <> Me.GUID Then
                        If Not comp.Ind(0) Is Nothing And Not Me.Ind(0) Is Nothing Then
                            'If comp.Ind(0).BitIndex = Me.Ind(0).BitIndex Then
                            If comp.Ind(0).PositionOfBit = Me.Ind(0).PositionOfBit Then
                                Me.BrotherSignals = True
                                Exit Sub
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SignalSharedStopBitCheck()
            Try
                Dim cp As ControlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(Me.StationNumber)
                If cp Is Nothing Then Exit Sub
                If IsNothing(Me.Ctl(2)) Then Exit Sub

                Dim ComponentCollection As ArrayList = cp.AssignedComponenets
                If ComponentCollection Is Nothing Then Exit Sub
                Dim comp As Object
                For Each comp In ComponentCollection
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        If Not IsNothing(sig.Ctl(2)) Then
                            If (sig.CodeStation = Me.CodeStation) And (sig.Ctl(2).PositionOfBit = Me.Ctl(2).PositionOfBit) Then
                                If IsNothing(Me.BrotherSignalSharedStopBitCollection) Then
                                    Me.BrotherSignalSharedStopBitCollection = New ArrayList()
                                    Me.BrotherSignalSharedStopBitCollection.Add(sig)
                                Else
                                    Me.BrotherSignalSharedStopBitCollection.Add(sig)
                                End If
                            End If
                        End If
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub BrotherSignalCheck()
            Try
                Dim cp As ControlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(Me.StationNumber)
                If cp Is Nothing Then Exit Sub
                Dim ComponentCollection As ArrayList = cp.AssignedComponenets
                If ComponentCollection Is Nothing Then Exit Sub
                Dim comp As Object
                For Each comp In ComponentCollection
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then

                        'check for brother signal existence
                        If comp.Name = Me.Name And comp.GUID <> Me.GUID Then
                            If Not comp.Ind(0) Is Nothing And Not Me.Ind(0) Is Nothing Then
                                If comp.Ind(0).PositionOfBit = Me.Ind(0).PositionOfBit Then
                                    Me.BrotherSignals = True
                                    If IsNothing(Me.BrotherSignalCollection) Then
                                        Me.BrotherSignalCollection = New ArrayList()
                                    End If
                                    If Not Me.BrotherSignalCollection.Contains(comp) Then
                                        Me.BrotherSignalCollection.Add(comp)
                                    End If
                                End If
                            End If
                        End If

                        'check for time-in brother signal
                        If Not Me.InTimeBit Is Nothing Then
                            If Not comp.InTimeBit Is Nothing Then
                                If comp.InTimeBit.PositionOfBit = Me.InTimeBit.PositionOfBit And comp.InTimeBit.CodeStation = Me.InTimeBit.CodeStation Then
                                    If Not BrotherSignalIntimeCollection.Contains(comp) Then
                                        Me.BrotherSignalIntimeCollection.Add(comp)
                                    End If
                                ElseIf Not Me.Ind(0) Is Nothing Then
                                    If comp.InTimeBit.PositionOfBit = Me.Ind(0).PositionOfBit And comp.InTimeType = TypeOfInTime.Opposing And Me.InTimeType = TypeOfInTime.Opposing Then
                                        If Not BrotherSignalIntimeCollection.Contains(comp) Then
                                            Me.BrotherSignalIntimeCollection.Add(comp)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub CheckForInTimeBrotherSignals()
            Try
                Dim cp As ControlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(Me.StationNumber)
                If cp Is Nothing Then Exit Sub
                Dim ComponentCollection As ArrayList = cp.AssignedComponenets
                If ComponentCollection Is Nothing Then Exit Sub
                Dim comp As Object
                For Each comp In ComponentCollection
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        If comp.InTimeBit Is Nothing Then Exit For
                        If Me.InTimeBit Is Nothing Then Exit For
                        If comp.InTimeBit.ControlPoint = Me.InTimeBit.ControlPoint And comp.InTimeBit.PositionOfBit = Me.InTimeBit.PositionOfBit And comp.InTimeBit.CodeStation = Me.InTimeBit.CodeStation Then
                            If Not BrotherSignalIntimeCollection.Contains(comp) Then
                                Me.BrotherSignalIntimeCollection.Add(comp)
                            End If
                        ElseIf Not Me.Ind(0) Is Nothing Then
                            If comp.InTimeBit.ControlPoint = Me.StationNumber And comp.InTimeBit.PositionOfBit = Me.Ind(0).PositionOfBit And comp.InTimeType = TypeOfInTime.Opposing And Me.InTimeType = TypeOfInTime.Opposing Then
                                If Not BrotherSignalIntimeCollection.Contains(comp) Then
                                    Me.BrotherSignalIntimeCollection.Add(comp)
                                End If
                            End If
                        End If



                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitHoldingSignalOs()
            Try
                If Me.DefaultTraffic = "LEFT" Then
                    Dim LeftTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                    If Not IsNothing(LeftTrack) Then
                        If Not LeftTrack.OStype Then
                            LeftTrack.HoldingSignalOsLeft = True
                        End If
                    End If

                ElseIf Me.DefaultTraffic = "RIGHT" Then
                    Dim RightTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                    If Not IsNothing(RightTrack) Then
                        If Not RightTrack.OStype Then
                            RightTrack.HoldingSignalOsRight = True
                        End If
                    End If

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function FindControlPointComponents(ByVal cpNum As Int32) As ArrayList
            Try
                Dim cp As ControlPoint
                For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                    If cp.StationNumber = cpNum Then
                        Return cp.AssignedComponenets
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Sub Draw(Optional ByVal g As Graphics = Nothing)
            Try
                If Me.MakeInvisible Then Exit Sub
                If IsNothing(Graphical) Then
                    Graphical = Me.CurrentDrawingForm.CreateGraphics
                End If

                SyncLock Graphical
                    SyncLock GraphicPathBulb
                        If FlashComponent Then
                            If FlashOn Then
                                Graphical.FillPath(CurrentBulbColorStatus, GraphicPathBulb)
                            Else
                                Graphical.FillPath(FlashOffColorStatus, GraphicPathBulb)
                            End If
                        Else
                            Graphical.FillPath(CurrentBulbColorStatus, GraphicPathBulb)
                        End If
                    End SyncLock
                    SyncLock GraphicPathStem
                        If Me.StoredEntrySignal Then
                            GraphicPathStem = Me.PathArrowEntry
                            Graphical.FillPath(New SolidBrush(Color.Yellow), GraphicPathStem)
                        ElseIf Me.StoredExitSignal Then
                            GraphicPathStem = Me.gStemPath
                            Graphical.DrawPath(New Pen(Color.Yellow, 2), GraphicPathStem)
                        ElseIf Me.FleetEnabled Then
                            GraphicPathStem = Me.gStemPath
                            'Graphical.FillPath(New SolidBrush(Color.LightGreen), GraphicPathStem)
                            Graphical.DrawPath(New Pen(Color.LightGreen, 2), GraphicPathStem)
                        Else
                            GraphicPathStem = Me.gStemPath
                            Graphical.FillPath(CurrentStemColorStatus, GraphicPathStem)
                        End If
                    End SyncLock
                End SyncLock


            Catch ex As Exception
                SystemException("Draw-SIGNAL-GUID:" & Me.GUID.ToString & " - " & ex.ToString)
                Graphical = Nothing
            End Try
        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                If Me.MakeInvisible Then Exit Sub
                g = Graphics.FromImage(bp)


                SyncLock GraphicPathBulb
                    If FlashComponent Then
                        If FlashOn Then
                            g.FillPath(CurrentBulbColorStatus, Me.gBulbPathRelative)
                        Else
                            g.FillPath(FlashOffColorStatus, gBulbPathRelative)
                        End If
                    Else
                        g.FillPath(CurrentBulbColorStatus, gBulbPathRelative)
                    End If
                End SyncLock

                If Me.StoredEntrySignal Then
                    'GraphicPathStem = Me.PathArrowEntry
                    'g.FillPath(New SolidBrush(Color.Yellow), GraphicPathStem)
                    g.FillPath(New SolidBrush(Color.Yellow), Me.PathArrowEntryRelative)

                ElseIf Me.StoredExitSignal Then
                    GraphicPathStem = Me.gStemPath
                    g.DrawPath(New Pen(Color.Yellow, 2), Me.PathArrowExitRelative)
                ElseIf Me.FleetEnabled Then
                    GraphicPathStem = Me.gStemPath
                    g.DrawPath(New Pen(Color.LightGreen, 2), gStemPathRelative)
                Else
                    'GraphicPathStem = Me.gStemPath
                    g.FillPath(CurrentStemColorStatus, Me.gStemPathRelative)
                End If
                g.Dispose()
                g = Nothing

            Catch ex As Exception
                g = Nothing
                SystemException("DrawToBitmap-SIGNAL-GUID:" & Me.GUID.ToString & " - " & ex.ToString)
            End Try
        End Sub

        Sub InvalidateSignal()
            Try
                'Me.pCurrentDrawingForm.Invalidate(InvalidateRegion)
                Me.CurrentDrawingForm.InvalidateFormRegionForSignal(Me, InvalidateRegion)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)
            Try
                If Me.AutomaticSignal Then Exit Sub
                If JointControlAuthorizationEnabled Then
                    SignalEventLog($"CHECKING JOIN SIGNAL STATUS={SignalIndStatus}")
                End If
                PreviousSignalStatus = Me.SignalStatus
                Select Case Me.SignalIndStatus
                    Case IndStatus.Stopped
                        Select Case Me.SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                Me.SignalStatus = BulbStatus.RequestingClear
                            Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                                Me.SignalStatus = BulbStatus.StopStatus
                        End Select
                    Case IndStatus.Cleared
                        Select Case Me.SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                Me.SignalStatus = BulbStatus.ClearStatus
                            Case CtlStatus.RequestingStop
                                Me.SignalStatus = BulbStatus.RequestingStop
                            Case CtlStatus.RequestedStop
                                If Me.SlottedByOSTrack AndAlso OptSetSignalToUnsolitatedStateIfSlottedByOSTrack Then
                                    Me.SignalStatus = BulbStatus.Unsolitated

                                ElseIf Not Me.SlottedByOSTrack Then
                                    Me.SignalStatus = BulbStatus.Unsolitated
                                End If
                        End Select
                    Case IndStatus.InTime
                        Select Case Me.SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                Me.SignalStatus = BulbStatus.RequestingClear
                            Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                                Me.SignalStatus = BulbStatus.InTime
                        End Select

                    Case IndStatus.InTimeNoControlledSignal
                        Select Case Me.SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                Me.SignalStatus = BulbStatus.RequestingClear
                            Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                                Me.SignalStatus = BulbStatus.InTimeNoControlledSignal
                        End Select
                End Select
                If Me.AllowSignalBlkInd Then
                    If Not IsNothing(Me.BlockIndBit) Then
                        If Me.BlockIndBit.BitState Then
                            Me.BlockAbsolute = True
                        Else
                            If Not Me.DeviceBlocked Then
                                Me.BlockAbsolute = False
                            End If
                        End If
                    End If
                End If
                Select Case SignalStatus
                    Case BulbStatus.ClearStatus
                        CurrentBulbColorStatus = New SolidBrush(Color.ForestGreen)
                        If Me.DrillMode Then
                            Me.SetToDrillClear()
                        Else
                            Me.SetToClear()
                        End If
                    Case BulbStatus.StopStatus
                        If Me.BlockAbsolute Then
                            CurrentBulbColorStatus = New SolidBrush(Color.MediumSlateBlue)
                        Else
                            CurrentBulbColorStatus = New SolidBrush(Color.Red)
                        End If
                        If (Me.DrilledSignal OrElse Me.DrillMode) AndAlso Me.DrillSignalMaster Then
                            Me.SetToDrillStop()
                            DrilledSignal = False
                        ElseIf Me.DrilledSignal AndAlso optEnableMRLSpecificOptions Then
                            Me.SetToDrillStop()
                            DrilledSignal = False
                        ElseIf Me.DrilledSignal AndAlso Me.PreviousDrilledControlled Then
                            Me.SetToDrillStop()
                            DrilledSignal = False
                        Else
                            Me.SetToStop()
                        End If
                    Case BulbStatus.InTime
                        If Me.PreviousDrilledControlled Then
                            Me.SetToInTimeRouteDrill()
                        Else
                            Me.SetToInTimeRoute()
                        End If
                        If Me.BlockAbsolute Then
                            CurrentBulbColorStatus = New SolidBrush(Color.DarkViolet)
                        Else
                            CurrentBulbColorStatus = New SolidBrush(Color.DarkSalmon)
                        End If
                    Case BulbStatus.InTimeNoControlledSignal
                        If Me.BlockAbsolute Then
                            CurrentBulbColorStatus = New SolidBrush(Color.DarkViolet)
                        Else
                            CurrentBulbColorStatus = New SolidBrush(Color.DarkSalmon)
                        End If
                        Me.SetToInTime()
                    Case BulbStatus.Failed
                        CurrentBulbColorStatus = New SolidBrush(Color.DarkGray)
                    Case BulbStatus.RequestingStop
                        If Me.BlockAbsolute Then
                            CurrentBulbColorStatus = New SolidBrush(Color.DarkViolet)
                        Else
                            CurrentBulbColorStatus = New SolidBrush(Color.Red)
                        End If
                        Me.SetToRequestingStop()
                    Case BulbStatus.RequestingClear
                        CurrentBulbColorStatus = New SolidBrush(Color.Yellow)
                        If Me.DrillMode Then
                            Me.SetToDrillRequestingClear()
                        Else
                            Me.SetToRequestingClear()
                        End If
                    Case BulbStatus.Unsolitated
                        CurrentBulbColorStatus = New SolidBrush(Color.MediumSpringGreen)
                        Me.SetToUnSolicated()
                        If Not Me.SlottedByOSTrack Then
                            If Not optSupressUnSolicitedSignalCheck Then
                                Me.ShowAlarmForUnSolicitedSignal(Me)
                            End If
                        End If
                        If Not optAllowDisableAutomaticSignalSlottingForUnSolicatedSignals Then
                            Me.SendSignalStopAutomatically(True, True)
                        End If
                    Case Else
                        CurrentBulbColorStatus = New SolidBrush(Color.Red)
                End Select
                Me.CurrentColorStatus = CurrentBulbColorStatus
                If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) And Not optDisableShowingCodeFailureOnComponents Then
                    CurrentBulbColorStatus = gColorFailedState
                End If
                If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) Then
                End If
                If PreviousSignalStatus <> Me.SignalStatus Then
                    InvalidateSignal()
                    Dim LogThisEvent As New DoEventLogSignal(AddressOf Me.LogTheSignalEvent)
                    LogThisEvent.BeginInvoke(Signal.EventTypeSignal.STATUS, Nothing, Nothing)
                End If
                If Me.BlockAbsolute <> Me.BlockAbsolutePrevious Then
                    Dim LogThisEvent As New DoEventLogSignal(AddressOf Me.LogTheSignalEvent)
                    LogThisEvent.BeginInvoke(Signal.EventTypeSignal.BLK, Nothing, Nothing)
                End If
                Me.BlockAbsolutePrevious = Me.BlockAbsolute
                'Send Status Update to Xmt Queue to send to attached control server
                If Not Me.FailState Then
                    'SendStatusUpdate()
                End If
                If (Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear) And Me.SignalIndStatus = IndStatus.Cleared And Not Me.DoSimulate Then
                    Me.tmrReControl.Enabled = False
                    Me.ReControlCount = 0
                    ReControlStart = False
                ElseIf (Me.SignalCtlStatus = CtlStatus.RequestedStop Or Me.SignalCtlStatus = CtlStatus.RequestingStop) And Me.SignalIndStatus = IndStatus.Stopped And Not Me.DoSimulate Then
                    Me.tmrReControl.Enabled = False
                    Me.ReControlCount = 0
                    ReControlStart = False
                End If
                If gTraineeMode Then
                    Dim PublishThisEvent As New DoPublishSignalEvent(AddressOf Me.PublishSignalEventUpdate)
                    Select Case Me.SignalCtlStatus
                        Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                            PublishThisEvent.BeginInvoke(Signal.EventTypeSig.REQUESTCLEAR, Nothing, Nothing)
                        Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                            PublishThisEvent.BeginInvoke(Signal.EventTypeSig.REQUESTSTOP, Nothing, Nothing)
                    End Select
                End If
                If Me.myControlPoint.AllowStatusPublishing Then
                    Me.PublishStatus()
                End If
                If Me.NearSideSignalEnabled Then
                    Me.NearSideSignalControlUpdateStatus()
                End If
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If
            Catch ex As Exception
                'ARRC-1567 , collect all frames information for analysis
                SystemException(GetExceptionAllFrameMsg(ex, New StackTrace(True)))
                'SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "METHODS"

        ''' <summary>
        ''' Reset call on
        ''' </summary>
        Public Sub ResetSignalCalledOn()
            Try
                If optEnableCallOnLatching AndAlso SignalCalledOn Then
                    SignalCalledOn = False
                    If CallOnControl IsNot Nothing Then CallOnControl.BitState = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Alert for signals with indicate only
        ''' </summary>
        Private Sub SignalIndicateOnlyeAlert()
            Dim frmMsg As frmMessageBox
            Dim msg As New ArrayList()
            Try
                frmMsg = New frmMessageBox()
                msg.Add("SINAL '" & Me.Name & "' SEM CONTROLE ASSOCIADO!")
                msg.Add("")
                frmMsg.SetMessage(msg, "SINAL", False, Me.CurrentDrawingForm.myLaunchPoint)
                gCritcalPromptExists = True
                frmMsg.ShowDialog()
                frmMsg.Dispose()
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                frmMsg = Nothing
                msg = Nothing
                gCritcalPromptExists = False
            End Try
        End Sub

        Private Delegate Sub DoSetSignalToStopWhileInAssistMode()

        Public Function DoesSignalHaveLicense(ByVal theTrain As TrainID) As Boolean
            Try
                Dim theTrack As Track
                If ComponentType = "L" Then
                    theTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                Else
                    theTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                End If
                If IsNothing(theTrack) Then Return False
                If theTrack.TrackAuthorities.Count > 0 Then
                    Dim auth As TrackAuthority.TrackAuthority = theTrack.TrackAuthorities.Item(0)
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

        ''' <summary>
        ''' Signal event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub SignalEventLog(message As String)
            Try
                SystemEvent($"{NameOf(Signal)} {Name}:{GUID}:{message}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function IsMySlottingTrackIndicated() As Boolean
            Try
                FindOsSlottingTrack()
                If Not IsNothing(Me.SlottingOSTrack) Then
                    If Me.SlottingOSTrack.Ind(0).BitState Then
                        Return True
                    End If
                End If
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub FindOsSlottingTrack()
            Try

                If Me.myControlPoint Is Nothing Then Exit Sub
                Dim LoopControl As Integer
                Dim NextComp As Object

                If Me.ComponentType = "L" Then
                    NextComp = Me.myControlPoint.FindComponent(Me.LeftLink)
                Else
                    NextComp = Me.myControlPoint.FindComponent(Me.RightLink)
                End If

                Do
                    If NextComp Is Nothing Then Exit Sub
                    If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If NextComp.SlottingTrack Then
                            Me.SlottingOSTrack = NextComp
                            Exit Sub
                        End If

                    ElseIf NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        If Me.ComponentType = "L" Then
                            If NextComp.ComponentType = "R" Then
                                Exit Sub
                            End If
                        Else
                            If NextComp.ComponentType = "L" Then
                                Exit Sub
                            End If
                        End If

                    End If
                    If Me.ComponentType = "L" Then
                        NextComp = Me.myControlPoint.FindComponent(NextComp.LeftLink)
                    Else
                        NextComp = Me.myControlPoint.FindComponent(NextComp.RightLink)
                    End If
                    LoopControl = LoopControl + 1
                    If LoopControl > Me.myControlPoint.AssignedComponenets.Count Then Exit Do
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Send Signal Control
        ''' </summary>
        ''' <param name="sendControlUpdateOnly"></param>
        Private Sub SendSignalControl(sendControlUpdateOnly As Boolean)
            Try
                SystemEvent($"{GetCurrentMethod.Name}:Signal Control Start:WhoCallMe = 
                                                        {(New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name}")
                Dim oneControlPoint As ControlPoint = Nothing
                Dim myControlPoint As ControlPoint = Nothing
                For Each oneControlPoint In gControlPointCollection
                    If StationNumber = oneControlPoint.StationNumber Then
                        myControlPoint = oneControlPoint
                        Exit For
                    End If
                Next
                If myControlPoint IsNot Nothing Then
                    FindCodeStationToSend()
                    If sendControlUpdateOnly Then
                        myControlPoint.SendCodeStationControlUpdate()
                    Else
                        myControlPoint.SendCodeStationControls()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SendSignalControlUpdate()
            Try
                Dim cp As ControlPoint
                Dim myControlPoint As ControlPoint = Nothing
                For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                    If Me.StationNumber = cp.StationNumber Then
                        myControlPoint = cp
                        Exit For
                    End If
                Next
                If Not myControlPoint Is Nothing Then
                    myControlPoint.SendCodeStationControlUpdate()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckForSignalFleets()
            Try
                If Me.FleetEnabled Then
                    If Not Me.ControlSignalToClear(True, True, True, False, False) Then
                        Me.DoSetSignalFleeting(False, True)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub FleetRetryElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                If Me.FleetEnabled Then
                    If Not Me.ControlSignalToClear(True, True, True, False, False, False, False, False, True) Then
                        Me.tmrFleetRetry.Enabled = True
                    Else
                        Me.tmrFleetRetry.Enabled = False
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Me.ReControlCount = 0
                ReControlStart = False
            End Try
        End Sub

        Public Sub StopReControlTimer()
            Try
                Me.ReControlCount = 0
                ReControlStart = False
                Me.tmrCheckRoute.Enabled = False
                Me.tmrReControl.Enabled = False
                Me.tmrReControl.Stop()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ReControlElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                If Not Me.myControlPoint.EnableResendControlLogic Then Exit Sub
                If Me.tmrCheckRoute.Enabled Then Exit Sub
                If Me.ReControlCount >= 3 Then
                    ReControlCount = 0
                    Me.tmrCheckRoute.Enabled = False
                    Me.tmrReControl.Enabled = False
                    ReControlStart = False
                    SignalEventLog("SIGNAL CONTROL FAILED:" & Me.Name & "-" & Me.GUID.ToString)
                    Dim LogThisEvent As New DoEventLogSignal(AddressOf Me.LogTheSignalEvent)
                    LogThisEvent.BeginInvoke(Signal.EventTypeSignal.FAIL, Nothing, Nothing)

                Else
                    Me.tmrReControl.Enabled = True
                    Me.tmrReControl.Interval = Me.myControlPoint.ResendControlTime 'optRetryTimeOutValue

                    'Me.tmrCheckRoute.Enabled = True
                    ReControlCount = ReControlCount + 1
                    Me.FindCodeStationToSend()
                    Me.myControlPoint.SendCodeStationControls(optEnableResendControlSpecial)
                    SignalEventLog("SIGNAL CONTROL RESEND:" & Me.Name & "-" & Me.GUID.ToString)

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Me.ReControlCount = 0
                ReControlStart = False
            End Try
        End Sub

        ''' <summary>
        ''' Check Route Elasped Event
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="e"></param>
        Sub CheckRouteElaspedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Dim comp As Object
            Try
                Me.SignalEventLog("CheckRouteElaspedEvent:Start:SignalCtlStatus=" & Me.SignalCtlStatus.ToString)
                If Not Me.SignalRouteCollection Is Nothing Then
                    For Each comp In Me.SignalRouteCollection
                        If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            If comp.ControlledReverse And Not comp.SwitchIndStatus = Switch.SwIndStatus.ReverseInd Then
                                If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                                    Me.tmrCheckRoute.Enabled = True
                                    Me.tmrCheckRoute.Interval = 2000
                                End If
                                Exit Sub
                            ElseIf Not comp.ControlledReverse And Not comp.SwitchIndStatus = Switch.SwIndStatus.NormalInd Then
                                If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                                    Me.tmrCheckRoute.Enabled = True
                                    Me.tmrCheckRoute.Interval = 2000
                                End If
                                Exit Sub
                            End If
                        End If
                    Next
                Else
                    Exit Sub
                End If
                If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                    Dim AllowSignalClear As Boolean = True
                    If Me.SignalCalledOn And CallOnSendSignalClearBit Then
                        AllowSignalClear = False
                    End If
                    If AllowSignalClear Then
                        'NEAR SIDE SIGNAL FUNCTIONALITY ADDED FOR NCTD
                        Dim LocalClearMode As Boolean
                        If Me.NearSideSignalEnabled And Me.NearSideSignalOn Then
                            If Not IsNothing(Me.NearSideSignalCtlBit) Then
                                LocalClearMode = True
                            End If
                        End If
                        'NEAR SIDE SIGNAL FUNCTIONALITY ADDED FOR NCTD
                        If Not LocalClearMode Then
                            If Me.EnableDynamicSignalRouteControlBit Then
                                If Me.SignalCalledOn Then
                                    Me.CallOnControl = Me.GetSignalCallOnBit
                                    If Not Me.CallOnControl Is Nothing Then
                                        Me.CallOnControl.BitState = True
                                    End If
                                Else
                                    Me.Ctl(0) = Me.GetSignalClearRouteBit
                                    If Not Me.Ctl(0) Is Nothing Then
                                        Me.Ctl(0).BitState = True
                                    End If
                                End If
                            Else
                                If Not Me.Ctl(0) Is Nothing Then
                                    Me.Ctl(0).BitState = True
                                End If
                                If Not Me.Ctl(3) Is Nothing Then
                                    Me.Ctl(3).BitState = True
                                End If
                                If Not Me.Ctl(1) Is Nothing Then
                                    Me.Ctl(1).BitState = True
                                End If
                                If Not Me.Ctl(4) Is Nothing Then
                                    Me.Ctl(4).BitState = True
                                End If
                            End If
                        Else
                            NearSideSignalCtlBit.BitState = True
                            Me.NearSideSignalControlUpdateStatus()
                        End If
                    End If
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = False
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If
                    If Me.SignalCalledOn Then
                        If Me.EnableDynamicSignalRouteControlBit Then
                            Me.CallOnControl = Me.GetSignalCallOnBit
                            If Not Me.CallOnControl Is Nothing Then
                                Me.CallOnControl.BitState = True
                            End If
                        Else
                            Me.CallOnControl.BitState = True
                        End If
                    End If
                Else
                    If Me.EnableDynamicSignalRouteControlBit Then
                        Dim bp As BitPosition
                        For Each bp In Me.DynamicSignalRouteControlBitList
                            bp.BitState = False
                        Next
                    End If
                    If Me.NearSideSignalEnabled Then
                        If Not IsNothing(Me.NearSideSignalCtlBit) Then
                            Me.NearSideSignalCtlBit.BitState = False
                            Me.NearSideSignalControlUpdateStatus()
                        End If
                    End If
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If
                    If Not Me.EnablePulseStopControl Then
                        If Not Me.Ctl(2) Is Nothing Then
                            Me.Ctl(2).BitState = True
                        End If
                        If Not Me.Ctl(5) Is Nothing Then
                            Me.Ctl(5).BitState = True
                        End If
                    End If
                    If Me.SignalCalledOn Then
                        Me.CallOnControl.BitState = False
                    End If
                    If Me.JointControlAuthorizationEnabled Then
                        Me.JointControlAuthorizationOff(False)
                    End If
                End If
                If Me.SimulateMode Then
                    Me.DoSimulate = True
                End If
                If Not Me.DoMonitorMode And Not Me.FailState Then
                    If Not gSendCTC Then
                        If Me.SimulateMode Then
                            Me.DoSimulate = False
                            Me.IndSimulator.Enabled = True
                            Me.LastCtlStatus = Me.SignalCtlStatus
                        Else
                            ReControlStart = True
                            Me.ReControlCount = 0
                            Me.FindCodeStationToSend()
                            SignalEventLog("CheckRouteElaspedEvent:SendCodeStationControls")
                            Me.myControlPoint.SendCodeStationControls(False, Me.CodeStation)
                        End If
                    Else
                        Me.ReControlCount = 0
                        ReControlStart = True
                        SignalEventLog("CheckRouteElaspedEvent:AddToSendCollection")
                        Me.AddToSendCollection("CheckRouteElaspedEvent", False)
                    End If
                End If
                Me.tmrCheckRoute.Enabled = False
                Me.SignalEventLog("CheckRouteElaspedEvent:End:SignalCtlStatus=" & Me.SignalCtlStatus.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                comp = Nothing
            End Try
        End Sub

        Private Function GetSignalClearRouteBit() As BitPosition
            Try
                Dim sr As SignalRoute
                For Each sr In gSignalRouteList
                    If sr.EntrySignal.GUID = Me.GUID Then
                        Dim bp As BitPosition
                        For Each bp In Me.DynamicSignalRouteControlBitList
                            If bp.ExitSignalGUID = sr.ExitSignal.GUID AndAlso SignalRouteCollection.Contains(sr.ExitTrack) Then
                                Me.SignalEventLog("GetSignalClearRouteBit:" & bp.ExitSignalGUID & "|" & bp.PositionOfBit & "|" & bp.Mnemonic)
                                Return bp
                            End If
                        Next
                    End If
                Next
                Me.SignalEventLog("GetSignalClearRouteBit-Bit Not Found:" & Me.GUID & "|" & Me.Name)

                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function GetSignalCallOnBit() As BitPosition
            Try
                Dim sr As SignalRoute
                For Each sr In gSignalRouteList
                    If sr.EntrySignal.GUID = Me.GUID Then
                        Dim bp As BitPosition
                        For Each bp In Me.DynamicSignalRouteCallBitList
                            If bp.ExitSignalGUID = sr.ExitSignal.GUID AndAlso SignalRouteCollection.Contains(sr.ExitTrack) Then
                                Me.SignalEventLog("GetSignalCallOnBit:" & bp.ExitSignalGUID & "|" & bp.PositionOfBit & "|" & bp.Mnemonic)
                                Return bp
                            End If
                        Next
                    End If
                Next
                Me.SignalEventLog("GetSignalCallOnBit-Bit Not Found:" & Me.GUID & "|" & Me.Name)

                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function CheckSignalAspects() As Boolean
            Try

                If Not IsNothing(Me.SigHead1Yellow) Then
                    If Me.SigHead1Yellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead1FYellow) Then
                    If Me.SigHead1FYellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead1FRed) Then
                    If Me.SigHead1FRed.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead2Green) Then
                    If Me.SigHead2Green.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead2Yellow) Then
                    If Me.SigHead2Yellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead2FYellow) Then
                    If Me.SigHead2FYellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead2FRed) Then
                    If Me.SigHead2FRed.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead2Lunar) Then
                    If Me.SigHead2Lunar.BitState Then
                        Return True
                    End If
                End If


                If Not IsNothing(Me.SigHead3Green) Then
                    If Me.SigHead3Green.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead3Yellow) Then
                    If Me.SigHead3Yellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead3FYellow) Then
                    If Me.SigHead3FYellow.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead3FRed) Then
                    If Me.SigHead3FRed.BitState Then
                        Return True
                    End If
                End If

                If Not IsNothing(Me.SigHead3Lunar) Then
                    If Me.SigHead3Lunar.BitState Then
                        Return True
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Sub ApplyIndicationChange(Optional ByVal SkipInTimeBit As Boolean = False)
            'Check ind/intime bits and state a temp variable to reflect thier state
            Try
                Dim ExitSignalFound As Boolean
                Dim LoopControl As Integer
                Dim CompToCheck As Object
                Dim InTime As Boolean = False
                Dim LastControlledSignalFound As Boolean = False
                Dim AllBrotherSignalsInitialized As Boolean = False
                Dim LastReferenceGUID As Int32
                Dim FirstNonTransverable As Boolean
                'Try
                '    If gDebug Then Me.DebugEvent("DEBUG: (CURRENT SIGNAL STATUS:" & Me.SignalStatus.ToString & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")(CP:" & TMDS.SystemUtilities.FetchData.FindControlPointName(Me.StationNumber) & ")")
                'Catch ex As Exception
                'End Try

                'check to amke sure this indication can apply to this signal if it is againt a route that is not tranverable

                Dim ControlPointNotInitialized As Boolean
                If gEnableCheckMultipleCodeStationInitialization Then
                    If (Not Me.myControlPoint.AllStationsInitialized And Me.myControlPoint.ContainsMultipleStations) Then
                        ControlPointNotInitialized = True
                    End If
                End If

                If Me.BrotherSignals Then
                    If Me.DefaultTraffic = "LEFT" Then
                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.LeftLink)
                        While Not ExitSignalFound
                            If CompToCheck.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(CompToCheck, Track)
                                If Not FirstNonTransverable Then
                                    If trk.NonTransverable Or ControlPointNotInitialized Then 'And Not trk.FailState Then
                                        'Allow a unsolicated signal through derail
                                        Dim LeftSw As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(trk.LeftLink)
                                        If Not IsNothing(LeftSw) Then
                                            If LeftSw.TypeOfSwitch <> "DERAIL" Then
                                                Me.Initialized = True
                                                Exit Sub
                                            End If
                                        Else
                                            Me.Initialized = True
                                            Exit Sub
                                        End If
                                    End If
                                    FirstNonTransverable = True
                                Else
                                    If trk.NonTransverable Or ControlPointNotInitialized Then 'And Not trk.FailState Then
                                        'Allow a unsolicated signal through derail
                                        Dim LeftSw As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(trk.LeftLink)
                                        If Not IsNothing(LeftSw) Then
                                            If LeftSw.TypeOfSwitch <> "DERAIL" Then
                                                'If LeftSw.RightLink <> trk.GUID Then
                                                Me.Initialized = True
                                                Exit Sub
                                                'End If
                                            End If
                                        Else
                                            Me.Initialized = True
                                            Exit Sub
                                        End If
                                    End If
                                End If

                                LastReferenceGUID = CompToCheck.GUID
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.LeftLink)
                            ElseIf CompToCheck.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                If CompToCheck.ControlledReverse Then
                                    If CompToCheck.LeftAltLink <> 0 Then
                                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.LeftAltLink)
                                    Else
                                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.LeftLink)
                                    End If
                                Else
                                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.LeftLink)
                                End If
                            ElseIf CompToCheck.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                Exit While
                            ElseIf CompToCheck.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.FindExitGUID(LastReferenceGUID))
                            Else
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.LeftLink)
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 50 Then
                                Exit While
                            End If
                        End While
                    Else
                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.RightLink)
                        While Not ExitSignalFound
                            If CompToCheck.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(CompToCheck, Track)
                                If Not FirstNonTransverable Then
                                    If trk.NonTransverable Or ControlPointNotInitialized Then 'And Not trk.FailState Then
                                        'If trk.NonTransverable And Not trk.FailState Then
                                        Me.Initialized = True
                                        Exit Sub
                                    End If
                                    FirstNonTransverable = True
                                Else
                                    If trk.NonTransverable Or ControlPointNotInitialized Then ' Not trk.FailState Then
                                        'If trk.NonTransverable And Not trk.FailState Then
                                        Me.Initialized = True
                                        Exit Sub
                                    End If
                                End If
                                LastReferenceGUID = CompToCheck.GUID
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.RightLink)
                            ElseIf CompToCheck.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                Exit While
                            ElseIf CompToCheck.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                If CompToCheck.ControlledReverse Then
                                    If CompToCheck.RightAltLink <> 0 Then
                                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.RightAltLink)
                                    Else
                                        CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.RightLink)
                                    End If
                                Else
                                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.RightLink)
                                End If
                            ElseIf CompToCheck.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.FindExitGUID(LastReferenceGUID))
                            Else
                                CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CompToCheck.RightLink)
                            End If
                            LoopControl = LoopControl + 1
                            If LoopControl > 50 Then
                                Exit While
                            End If
                        End While
                    End If
                End If

                Dim StopIndBit, InTimeIndBit As Boolean
                If Not IndicateOnlyWithControlSetEnabled Then
                    If Not Me.Ind(0) Is Nothing Then
                        If Me.Ind(0).BitState = True Then
                            StopIndBit = False
                        Else
                            StopIndBit = True
                        End If

                        If Me.CheckSignalAspects Then
                            StopIndBit = False
                        End If

                    Else
                        StopIndBit = True
                    End If
                ElseIf IndicateOnlyWithControlSetEnabled Then
                    If Me.SignalControlled Then
                        If Not IsNothing(Me.Ind(0)) And Not IsNothing(Me.Ind(1)) Then
                            If Me.Ind(0).BitState = True And Me.Ind(1).BitState = True Then
                                StopIndBit = False
                            Else
                                StopIndBit = True
                            End If
                            If Me.CheckSignalAspects Then
                                StopIndBit = False
                            End If

                        Else
                            StopIndBit = True
                        End If
                    Else
                        StopIndBit = True
                    End If
                End If

                'this logic checks for odd ball brother pairing of signals to display the one that is requested clear

                If Not StopIndBit Then
                    If Me.BrotherSignals Then
                        Dim sig As Signal
                        If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                            Me.BrotherSignalFirst = True
                        Else
                            For Each sig In Me.BrotherSignalCollection
                                If sig.SignalCtlStatus = CtlStatus.RequestedClear Or sig.SignalCtlStatus = CtlStatus.RequestingClear Then
                                    sig.BrotherSignalFirst = True
                                    Exit Sub
                                ElseIf sig.BrotherSignalFirst Then
                                    Exit Sub
                                End If
                            Next
                            BrotherSignalFirst = True

                        End If
                    End If
                Else
                    If Me.BrotherSignals Then
                        BrotherSignalFirst = False
                    End If
                End If

                If Not Me.DrillIndOne Is Nothing Then
                    If Me.DrillIndOne.BitState = True Then
                        DrillIndicatedOn = True
                    Else
                        DrillIndicatedOn = False
                    End If
                End If

                If Not Me.DrillIndTwo Is Nothing Then
                    If Me.DrillIndTwo.BitState = True And DrillIndicatedOn Then
                        DrillIndicatedOn = True
                    Else
                        DrillIndicatedOn = False
                    End If
                End If

                If Me.Initialized Then
                    If Not Me.InTimeBit Is Nothing Then
                        If Me.InTimeBit.BitState Then
                            InTimeIndBit = Me.InTimeBit.BitState
                        Else
                            'SlottedByOSTrack = False
                            If Me.SlottedByOSTrack Then
                                If Me.SignalIndStatus <> IndStatus.Cleared Then
                                    SlottedByOSTrack = False
                                End If
                            End If
                        End If
                    End If
                ElseIf Not Me.Initialized Then
                    InTimeIndBit = False
                    SlottedByOSTrack = False
                End If

                If Not SkipInTimeBit Then
                    If Me.InTimeType = TypeOfInTime.LockBit Then
                        If Not Me.SlottingOSTrack Is Nothing Then
                            If Me.SlottingOSTrack.Occuppied Then
                                InTimeIndBit = False
                            End If
                        End If
                    Else
                        If Not Me.InTimeBit Is Nothing Then
                            If Me.InTimeBit.BitState Then
                                InTimeIndBit = Me.InTimeBit.BitState
                            Else
                                'SlottedByOSTrack = False
                                If Me.SlottedByOSTrack Then
                                    If Me.SignalIndStatus <> IndStatus.Cleared Then
                                        SlottedByOSTrack = False
                                    End If
                                End If
                            End If
                        End If
                    End If
                Else
                    If Me.InTimeType = TypeOfInTime.LockBit Then
                        InTimeIndBit = False
                    Else
                        If Not Me.InTimeBit Is Nothing Then
                            If Me.InTimeBit.BitState Then
                                InTimeIndBit = Me.InTimeBit.BitState
                            Else
                                'SlottedByOSTrack = False
                                If Me.SlottedByOSTrack Then
                                    If Me.SignalIndStatus <> IndStatus.Cleared Then
                                        SlottedByOSTrack = False
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

                Dim SignalSetControlledFound As Boolean = False
                If Me.IndicateOnlyWithControlSetEnabled Then
                    Dim sig As Signal
                    For Each sig In Me.BrotherSignalIntimeCollection
                        If sig.SignalControlled Then
                            SignalSetControlledFound = True
                            InTimeIndBit = False
                        End If
                    Next
                End If

                'set the signal indication status from indication bits
                If Me.DrillIndicatedOn Then
                    Me.SignalIndStatus = IndStatus.Cleared
                    If Not Me.DrillPartner Is Nothing Then
                        Me.DrillPartner.SignalIndStatus = IndStatus.Cleared
                    End If
                ElseIf (Not StopIndBit And InTimeIndBit And Me.InTimeType = TypeOfInTime.Opposing And Not Me.IndicateOnlyWithControlSetEnabled) Or (Me.IndicateOnlyWithControlSetEnabled And InTimeIndBit And Me.InTimeType = TypeOfInTime.Opposing And Not SignalSetControlledFound) Then
                    If Me.LastControlSignal Then
                        Me.SignalIndStatus = IndStatus.InTime
                        If Me.DoMonitorMode Then
                            Me.SignalCtlStatus = CtlStatus.RequestedStop
                        End If
                    Else
                        If CheckForLastControlledBrotherInTimeCollection() Then
                            Me.SignalIndStatus = IndStatus.Stopped
                            If Not Me.DrillPartner Is Nothing Then
                                Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                            End If
                        Else
                            Me.SignalIndStatus = IndStatus.InTimeNoControlledSignal
                        End If
                    End If
                ElseIf StopIndBit And InTimeIndBit And Me.InTimeType = TypeOfInTime.InTimeBit Then
                    If Me.LastControlSignal AndAlso Not Me.PreviousDrilledControlled Then
                        'If (Me.SignalCtlStatus = CtlStatus.RequestedStop And Not SlottedByOSTrack) Or Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                        If (Me.SignalCtlStatus = CtlStatus.RequestedStop And Not SlottedByOSTrack) _
                        Or Me.SignalCtlStatus = CtlStatus.RequestingStop _
                        Or Me.SignalCtlStatus = CtlStatus.RequestedStop _
                        Or (Me.myControlPoint.TunnelAutomaticControlled And Me.SignalCtlStatus = CtlStatus.RequestedClear) Then
                            Me.SignalIndStatus = IndStatus.InTime
                            If Me.DoMonitorMode Then
                                Me.SignalCtlStatus = CtlStatus.RequestedStop
                            End If
                        Else
                            Me.SignalIndStatus = IndStatus.Stopped
                            If Not Me.DrillPartner Is Nothing Then
                                Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                            End If

                        End If
                    ElseIf PreviousDrilledControlled Then
                        Me.SignalIndStatus = IndStatus.InTime
                    Else
                        If CheckForLastControlledBrotherInTimeCollection() Then
                            Me.SignalIndStatus = IndStatus.Stopped
                            'If Not Me.DrillPartner Is Nothing Then
                            '    Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                            'End If
                        Else
                            Me.SignalIndStatus = IndStatus.InTimeNoControlledSignal
                        End If
                    End If
                ElseIf StopIndBit And InTimeIndBit And Me.InTimeType = TypeOfInTime.LockBit Then
                    If Me.LastControlSignal Then
                        'If (Me.SignalCtlStatus = CtlStatus.RequestedStop And Not SlottedByOSTrack) Or Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                        If (Me.SignalCtlStatus = CtlStatus.RequestedStop And Not SlottedByOSTrack) _
                        Or Me.SignalCtlStatus = CtlStatus.RequestingStop _
                        Or Me.SignalCtlStatus = CtlStatus.RequestedStop Then
                            Me.SignalIndStatus = IndStatus.InTime
                            If Me.DoMonitorMode Then
                                Me.SignalCtlStatus = CtlStatus.RequestedStop
                            End If
                        Else
                            Me.SignalIndStatus = IndStatus.Stopped
                            If Not Me.DrillPartner Is Nothing Then
                                Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                            End If
                        End If
                    Else
                        If CheckForLastControlledBrotherInTimeCollection() Then
                            Me.SignalIndStatus = IndStatus.Stopped
                            If Not Me.DrillPartner Is Nothing Then
                                Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                            End If
                        Else
                            Me.SignalIndStatus = IndStatus.InTimeNoControlledSignal
                        End If
                    End If

                ElseIf Not StopIndBit And Not InTimeIndBit And Not Me.DrillMode Then
                    Me.SignalIndStatus = IndStatus.Cleared

                ElseIf Not StopIndBit And InTimeIndBit And Me.InTimeType <> TypeOfInTime.Opposing And Not Me.DrillMode Then
                    Me.SignalIndStatus = IndStatus.Cleared

                ElseIf StopIndBit And Not InTimeIndBit Then
                    Me.SignalIndStatus = IndStatus.Stopped
                    If Not Me.DrillPartner Is Nothing And Me.DrillMode Then
                        If Not Me.FailState Then
                            Me.DrillPartner.SignalIndStatus = IndStatus.Stopped
                        End If
                    End If
                End If

                'Upon the failstate of a control point, init the control to the indicated position
                If Me.FailState Or Me.DoMonitorMode Or Me.MatchMode Or Me.NoSignalControlInitialization Or (Me.IndicateOnlyWithControlSetEnabled And Not Me.SignalControlled) Then
                    If Me.DrillIndicatedOn Then
                        Me.SignalIndStatus = IndStatus.Stopped
                        Me.ControlSignalToDrill(False, True)
                        Me.SignalIndStatus = IndStatus.Cleared
                        Me.DrillPartner.SignalIndStatus = IndStatus.Cleared
                        Me.SetToDrillClear()
                        Me.DrilledSignal = True
                    End If

                    Select Case Me.SignalIndStatus
                        Case IndStatus.InTime, IndStatus.Unsolitated
                            Me.SetToInTimeRoute()
                            If Me.DoMonitorMode Then
                                Me.SetSignalControlBitsToStop()
                            End If
                            If Me.NoSignalControlInitialization Then
                                CheckToChangeControlInitialization()
                            End If
                        Case IndStatus.InTimeNoControlledSignal
                            Me.SetToInTime()
                            If Me.NoSignalControlInitialization Then
                                CheckToChangeControlInitialization()
                            End If
                        Case IndStatus.Stopped
                            Me.SetControlBit(False)
                            If Me.NoSignalControlInitialization Then
                                CheckToChangeControlInitialization()
                            End If
                        Case IndStatus.Cleared
                            SetControlBit(True)
                            Me.LastControlSignal = True
                            RemoveControllInitializationFromBrotherSignals()
                    End Select

                    If Me.NearSideSignalEnabled Then
                        If Not IsNothing(Me.NearSideSignalIndBit) Then
                            If Me.NearSideSignalIndBit.BitState Then
                                Me.NearSideSignalOn = True
                                If Not IsNothing(Me.NearSideSignalCtlBit) Then
                                    Me.NearSideSignalCtlBit.BitState = True
                                End If
                            Else
                                Me.NearSideSignalOn = False
                                If Not IsNothing(Me.NearSideSignalCtlBit) Then
                                    Me.NearSideSignalCtlBit.BitState = False
                                End If
                            End If
                            Me.NearSideSignalControlUpdateStatus()
                        End If
                    End If
                ElseIf Me.SignalSlotting Then
                    If Me.PreviousSignalIndStatus = IndStatus.Cleared And Me.SignalIndStatus = IndStatus.Stopped And Not Me.DrillMode Then
                        Me.PreviousSignalIndStatus = Me.SignalIndStatus
                        Me.SlotToStop()
                        SignalEventLog("S5")

                        If Not gAssistMode Then
                            'CheckForSignalFleets()
                        End If
                    End If
                End If

                If Me.SignalIndStatus = IndStatus.Stopped And (Me.SignalCtlStatus = CtlStatus.RequestedStop Or Me.SignalCtlStatus = CtlStatus.RequestingStop) Then
                    If Not Me.myControlPoint.CurrentlySelected Then
                        Me.LastCtlStatus = CtlStatus.RequestedStop
                    End If
                    'added in 1.0.0.398
                    Me.SignalCtlStatus = CtlStatus.RequestedStop
                    'added in 1.0.0.398
                End If

                If Me.JointControlAuthorizationEnabled Then
                    JointControlAuthorizationUpdateStatus()
                End If

                If PreviousSignalIndStatus <> Me.SignalIndStatus Then
                    Dim LogThisEvent As New DoEventLogSignal(AddressOf Me.LogTheSignalEvent)
                    LogThisEvent.BeginInvoke(Signal.EventTypeSignal.IND, Nothing, Nothing)
                    If Me.PublishStatusToPTC Then
                        TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_DeviceStatusBroadcast(Me, Nothing)
                    End If
                End If

                Me.PreviousSignalIndStatus = Me.SignalIndStatus
                Dim EventMessage As String = "SIGNAL INDICATION RECEIVED. (UPDATE STATUS:" & Me.SignalIndStatus.ToString.ToUpper & "|" & Me.SignalCtlStatus.ToString.ToUpper & "|" & Me.SignalStatus.ToString.ToUpper & ")(NAME:" & Me.Name & ")(UID:" & Me.GUID.ToString & ")(CP:" & TMDS.SystemUtilities.FetchData.FindControlPointName(Me.StationNumber) & ")"
                Me.SignalEventLog(EventMessage)
                Me.Initialized = True

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub RemoveControllInitializationFromBrotherSignals()
            Try
                If Not IsNothing(Me.BrotherSignalCollection) Then
                    Dim Sig As Signal
                    For Each Sig In Me.BrotherSignalCollection
                        Sig.NoSignalControlInitialization = False
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function RetreiveSignalRoute() As ArrayList
            Dim EndOfRouteFound As Boolean
            Dim CompToCheck As Object
            Dim SearchRight As Boolean
            Dim FlipFlopFound As Boolean
            Dim DirectionOfRoute As String
            Dim NextReferenceGUID, LastReferenceGUID As Int32
            Dim SignalRoute As New ArrayList()
            Dim LoopControl As Integer

            Try

                'set the direction of the search
                If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                    SearchRight = True
                    DirectionOfRoute = "RIGHT"
                Else
                    DirectionOfRoute = "LEFT"
                End If

                'init the first link to valid
                If SearchRight Then
                    NextReferenceGUID = Me.RightLink
                Else
                    NextReferenceGUID = Me.LeftLink
                End If

                'begin the validation search
                While Not EndOfRouteFound
                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextReferenceGUID) 'FindObjectTypeFromGUID(NextReferenceGUID)
                    If CompToCheck Is Nothing Then Return Nothing
                    Select Case CompToCheck.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch"
                            If FlipFlopFound Then CompToCheck.FlipMe = True

                            'check to see if switch is CTC operations
                            If CompToCheck.AuthorityType <> "CTC" Then
                                Return SignalRoute
                            End If


                            'check request position of switch
                            If CompToCheck.TypeOfSwitch <> "DERAIL" Then

                                If SearchRight And CompToCheck.RightAltLink <> 0 Then
                                    If Not CompToCheck.FlipOperations Then
                                        If Not CompToCheck.ControlledReverse Then
                                            NextReferenceGUID = CompToCheck.RightLink
                                        Else
                                            NextReferenceGUID = CompToCheck.RightAltLink
                                        End If
                                    Else
                                        If CompToCheck.ControlledReverse Then
                                            NextReferenceGUID = CompToCheck.RightLink
                                        Else
                                            NextReferenceGUID = CompToCheck.RightAltLink
                                        End If

                                    End If
                                ElseIf Not SearchRight And CompToCheck.LeftAltLink <> 0 Then
                                    If Not CompToCheck.FlipOperations Then
                                        If Not CompToCheck.ControlledReverse Then
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftAltLink
                                        End If
                                    Else
                                        If CompToCheck.ControlledReverse Then
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftAltLink
                                        End If
                                    End If

                                ElseIf LastReferenceGUID = CompToCheck.LeftLink And SearchRight Then
                                    If Not CompToCheck.FlipOperations Then
                                        If CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.RightLink
                                        End If
                                    Else
                                        If Not CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.RightLink
                                        End If
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.LeftAltLink And SearchRight Then
                                    If Not CompToCheck.FlipOperations Then
                                        If Not CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.RightLink
                                        End If
                                    Else
                                        If CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.RightLink
                                        End If
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.RightLink And Not SearchRight Then
                                    If Not CompToCheck.FlipOperations Then
                                        If CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        End If
                                    Else
                                        If Not CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        End If
                                    End If

                                ElseIf LastReferenceGUID = CompToCheck.RightAltLink And Not SearchRight Then

                                    If Not CompToCheck.FlipOperations Then
                                        If Not CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        End If
                                    Else
                                        If CompToCheck.ControlledReverse Then
                                            Return Nothing
                                        Else
                                            NextReferenceGUID = CompToCheck.LeftLink
                                        End If
                                    End If
                                End If
                            ElseIf CompToCheck.TypeOfSwitch = "DERAIL" Then
                                If SearchRight Then
                                    NextReferenceGUID = CompToCheck.RightLink
                                Else
                                    NextReferenceGUID = CompToCheck.LeftLink
                                End If

                            End If

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRoute.Contains(CompToCheck) Then
                                SignalRoute.Add(CompToCheck)
                            End If
                        Case "TMDS.SignalComponents.Signal"

                            If Not FlipFlopFound Then
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic = Me.DefaultTraffic Then
                                    Return SignalRoute
                                End If
                            Else
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic <> Me.DefaultTraffic Then
                                    Return SignalRoute
                                End If
                            End If
                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If

                            If Not SignalRoute.Contains(CompToCheck) Then
                                SignalRoute.Add(CompToCheck)
                            End If
                            'Track Validation
                        Case "TMDS.TrackComponents.Track"
                            Dim trkToCheck As Track = CType(CompToCheck, Track)
                            'If (trkToCheck.NonTransverable Or trktocheck.FailState) And trktocheck.OStype And Not Me.HoldingSignal Then
                            '    Return Nothing
                            'End If
                            If FlipFlopFound Then CompToCheck.FlipMe = True

                            Dim AuthType As String = CompToCheck.AuthorityType
                            Select Case AuthType
                                Case "TWC", "NON-TWC", "SHELF"
                                    Return SignalRoute
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        Return SignalRoute
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        Return SignalRoute
                                    End If
                            End Select
                            Dim trk As Track = CType(CompToCheck, Track)

                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If
                            If Not SignalRoute.Contains(CompToCheck) Then
                                SignalRoute.Add(CompToCheck)
                            End If

                        Case "TMDS.TrackOtherComponents.RRXING"
                            Dim AuthType As String = CompToCheck.AuthorityType
                            Select Case AuthType
                                Case "TWC", "NON-TWC"
                                    Return SignalRoute
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        Return SignalRoute
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        Return SignalRoute
                                    End If
                            End Select

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRoute.Contains(CompToCheck) Then
                                SignalRoute.Add(CompToCheck)
                            End If

                            'setup the next component to validate

                            'If SearchRight Then
                            NextReferenceGUID = CompToCheck.FindExitGUID(LastReferenceGUID)
                            'Else
                            '    NextReferenceGUID = CompToCheck.LeftLink
                            'End If
                            LastReferenceGUID = CompToCheck.GUID

                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            FlipFlopFound = True
                            Dim AuthType As String = CompToCheck.AuthorityType
                            Select Case AuthType
                                Case "TWC", "NON-TWC"
                                    Return SignalRoute
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        Return SignalRoute
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        Return SignalRoute
                                    End If
                            End Select

                            If Not SearchRight Then
                                SearchRight = True
                            Else
                                SearchRight = False
                            End If

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRoute.Contains(CompToCheck) Then
                                SignalRoute.Add(CompToCheck)
                            End If
                            'setup the next component to validate

                            'If SearchRight Then
                            NextReferenceGUID = CompToCheck.FindExitGUID(LastReferenceGUID)
                            'Else
                            '    NextReferenceGUID = CompToCheck.LeftLink
                            'End If
                            LastReferenceGUID = CompToCheck.GUID
                    End Select
                    LoopControl += 1
                    If LoopControl > 100 Then
                        Me.SignalEventLog("LOOP CONTROL EXCEEDED. EXITING GetClearSignalRouteStep2:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        Return Nothing
                    End If
                End While

                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Sub CheckToChangeControlInitialization()
            Try
                Dim Route As ArrayList = Me.RetreiveSignalRoute
                If Me.ComponentType = "L" Or Me.ComponentType = "U" Then
                    Dim LeftTrk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                    If Not IsNothing(LeftTrk) Then
                        If LeftTrk.BlockAbsolute Then
                            Me.NoSignalControlInitialization = False
                        End If
                    End If
                Else
                    Dim RightTrk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                    If Not IsNothing(RightTrk) Then
                        If RightTrk.BlockAbsolute Then
                            Me.NoSignalControlInitialization = False
                        End If
                    End If
                End If
                Dim comp As Object
                If Not IsNothing(Route) Then

                    For Each comp In Route
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(comp, Track)
                            If trk.BlockAbsolute Then
                                Me.NoSignalControlInitialization = False
                            End If
                        End If
                    Next
                End If
                'If Me.SignalIndStatus = IndStatus.Stopped Then
                '    For Each comp In Route
                '        If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                '            Dim sig As Signal = CType(comp, Signal)
                '            sig.NoSignalControlInitialization = False
                '        End If
                '    Next
                'End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckForLastControlledBrotherInTimeCollection() As Boolean
            Try
                Dim sig As Signal
                For Each sig In Me.BrotherSignalIntimeCollection
                    If sig.LastControlSignal Then
                        Return True
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Sub SetControlBit(ByVal SetToClear As Boolean)
            Try

                If optDisableShowingCodeFailureOnComponents And Not gSimulationMode Then
                    Me.BrotherSignalFirst = False
                    Me.SignalCtlStatus = Signal.CtlStatus.RequestedStop
                    Me.SignalIndStatus = Signal.IndStatus.Stopped
                    Me.SlotToStop()
                    SignalEventLog("S4")

                    If Me.DrillType <> Signal.TypeOfDrill.None Then
                        Me.ControlSignalToDrillStop()
                        Me.SetToDrillStop()
                    End If
                End If

                If optAllowNoSignalControlInitializationOnFailure Then
                    Me.NoSignalControlInitialization = True
                End If


                If Not SetToClear Then
                    Dim BrotherSignalCleared As Boolean
                    Try
                        If Not IsNothing(Me.BrotherSignalIntimeCollection) Then
                            Dim Sig As Signal
                            For Each Sig In Me.BrotherSignalIntimeCollection
                                If Not Sig Is Me Then
                                    If Not IsNothing(Sig.Ctl(0)) Then
                                        If Sig.Ctl(0).BitState Then
                                            If Sig.Ctl(2).PositionOfBit = Me.Ctl(2).PositionOfBit Then
                                                BrotherSignalCleared = True
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Catch
                    End Try

                    Try

                        Dim mySig As Signal
                        If Not IsNothing(Me.BrotherSignalSharedStopBitCollection) Then
                            For Each mySig In Me.BrotherSignalSharedStopBitCollection
                                If mySig.Ctl(0).BitState Then
                                    BrotherSignalCleared = True
                                    Exit For
                                End If
                            Next
                        End If
                    Catch

                    End Try

                    If Not BrotherSignalCleared Then

                        If Not Me.EnablePulseStopControl Then
                            If Not Me.Ctl(2) Is Nothing Then
                                Me.Ctl(2).BitState = True
                            End If

                            If Not Me.Ctl(5) Is Nothing Then
                                Me.Ctl(5).BitState = True
                            End If
                        End If

                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = False
                        End If

                        If Not Me.Ctl(4) Is Nothing Then
                            Me.Ctl(4).BitState = False
                        End If

                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = False
                        End If

                        If Not Me.Ctl(3) Is Nothing Then
                            Me.Ctl(3).BitState = False
                        End If

                        Me.SignalEventLog("SetControlBit-SIGNAL-STOP:" & Me.GUID)

                    Else
                        If Not Me.EnablePulseStopControl Then
                            If Not Me.Ctl(2) Is Nothing Then
                                Me.Ctl(2).BitState = False
                            End If

                            If Not Me.Ctl(5) Is Nothing Then
                                Me.Ctl(5).BitState = False
                            End If
                        End If
                    End If

                    If JointControlAuthorizationEnabled AndAlso Me.myControlPoint.TunnelAutomaticMode AndAlso Not Me.JointControlAuthorizationOfficeMode Then

                        Me.JointControlAuthorizationCtlBit.BitState = False
                        If Not IsNothing(Me.JointControlAuthorizationCancelBit) Then
                            Me.JointControlAuthorizationCancelBit.BitState = True
                        End If
                        JointControlAuthorizationUpdateStatus()
                    End If

                    Me.SignalIndStatus = IndStatus.Stopped

                    If Not (gAssistMode OrElse Me.IsShadowMode) Then

                        If Me.DoMonitorMode Or Me.MatchMode Then
                            Me.ControlSignalToStop(False, True)
                        End If
                        Me.SetToStop()
                    Else
                        Me.SetSignalToStopWhileInAssistMode()
                    End If

                    Me.LastCtlStatus = Me.SignalCtlStatus ' moved from the SetToStop sub

                ElseIf Not Me.DrillIndicatedOn And SetToClear Then
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = False
                    End If

                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If

                    If Me.EnableDynamicSignalRouteControlBit Then
                        'Me.Ctl(0) = Me.GetSignalClearRouteBit
                        'If Not Me.Ctl(0) Is Nothing Then
                        '    Me.Ctl(0).BitState = True
                        'End If
                        If SignalCalledOn Then
                            Me.CallOnControl = Me.GetSignalCallOnBit
                            If Not Me.CallOnControl Is Nothing Then
                                Me.CallOnControl.BitState = True
                            End If
                        Else
                            Me.Ctl(0) = Me.GetSignalClearRouteBit
                            If Not Me.Ctl(0) Is Nothing Then
                                Me.Ctl(0).BitState = True
                            End If
                        End If
                    Else
                        If Not Me.Ctl(1) Is Nothing Then
                            Me.Ctl(1).BitState = True
                        End If

                        If Not Me.Ctl(0) Is Nothing Then
                            Me.Ctl(0).BitState = True
                        End If
                    End If

                    Me.SignalIndStatus = IndStatus.Cleared
                    Me.ControlSignalToClear(False, False, False, False, False)
                    If Not Me.FailState And Me.NoSignalControlInitialization And Not Me.IndicateOnly Then
                        Me.SendSignalStopAutomatically()
                    Else
                        Me.SetToClear()
                        Me.LastCtlStatus = Me.SignalCtlStatus
                        Me.ControlNotYetCommitted = False
                    End If
                End If


                'Me.SendSignalControlUpdate()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SetSignalToStopWhileInAssistMode()
            Try
                Me.ControlSignalToStop(False, True)
                Me.SetToStop()
            Catch
            End Try
        End Sub

        Public Function CheckForInValidRoute(Optional RouteCollection As ArrayList = Nothing) As Boolean
            Try
                If RouteCollection Is Nothing Then
                    CopyArrayCollection(RouteCollection, Me.SignalRouteCollection)
                End If
                Dim comp As Object
                Dim CurrentTrackName As String = String.Empty
                Dim NoValidate As Boolean
                Dim NameList As New ArrayList()
                For Each comp In RouteCollection 'Me.SignalRouteCollection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If CurrentTrackName Is Nothing AndAlso CurrentTrackName.Trim <> "" Then
                            CurrentTrackName = comp.TrackName.Trim.ToUpper
                            'ElseIf CurrentTrackName.Trim = "" Then
                            '    CurrentTrackName = comp.TrackName
                        ElseIf CurrentTrackName.Trim.ToUpper <> comp.TrackName.Trim.ToUpper Then
                            If Not NoValidate Then
                                If CurrentTrackName.Trim <> "" AndAlso Not NameList.Contains(comp.TrackName.Trim.ToUpper) Then
                                    NameList.Add(CurrentTrackName.Trim.ToUpper)
                                End If
                                Dim thisName As String
                                For Each thisName In NameList
                                    If thisName.Trim.ToUpper = comp.TrackName.Trim.ToUpper Then
                                        Return False
                                    End If
                                Next
                                CurrentTrackName = comp.TrackName.Trim.ToUpper
                            End If
                        End If
                    ElseIf comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        Dim sw As Switch = CType(comp, Switch)
                        If sw.NoValidate Then
                            NoValidate = True
                        Else
                            NoValidate = False
                        End If
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Function GetTagsInCallOnTracks(ByVal CallOnTracks As ArrayList, ByVal ShowRoute As Boolean, ByVal DirectionOfRoute As String) As Boolean
            Try
                Dim comp As Object
                Dim PreviousOsTrks As New ArrayList()

                For Each comp In CallOnTracks
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If comp.OStype Then
                            Dim osTrk As Object
                            If Not PreviousOsTrks.Contains(comp.Name) Then
                                PreviousOsTrks.Add(comp.Name)
                                For Each osTrk In comp.OsComponentCollection
                                    If osTrk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                        If osTrk.TrackTagLabels.Count > 0 Then
                                            Dim trkTag As Tag
                                            Dim trk1 As Track = CType(osTrk, Track)
                                            Dim TagList As ArrayList = trk1.TrackTagLabels.Clone

                                            For Each trkTag In TagList 'osTrk.TrackTagLabels
                                                If trkTag.Restriction Then
                                                    If trkTag.Direction = "BOTH" Or trkTag.Direction = DirectionOfRoute Then
                                                        If ShowRoute Then
                                                            PendingRouteShow = True
                                                            Me.tmrShowPendingRoute.Enabled = True
                                                        End If
                                                        Dim TagString() As String
                                                        Dim UserMessage As Integer
                                                        Dim MessageString As String = trkTag.TagType.ToString.ToUpper & " TRACK RESTRICTION: " & vbCrLf & vbCrLf
                                                        TagString = trkTag.DataString.Split("|")
                                                        Select Case trkTag.TagType.ToString.ToUpper
                                                            Case "XING"
                                                                If TagString.Length < 2 Then Exit Select
                                                                MessageString = MessageString & "PROTECT XING AT MP " & TagString(0) & vbCrLf
                                                                MessageString = MessageString & "XING NAME: " & TagString(1) & vbCrLf
                                                            Case "SPEED"
                                                                If TagString.Length < 5 Then Exit Select
                                                                MessageString = MessageString & "SPEED RESTRICTION FOR PASSENGER TRAINS AT " & TagString(0) & " MPH" & vbCrLf
                                                                MessageString = MessageString & "SPEED RESTRICTION FOR FREIGHT TRAINS AT " & TagString(5) & " MPH" & vbCrLf
                                                                MessageString = MessageString & "BETWEEN MP " & TagString(1) & vbCrLf
                                                                MessageString = MessageString & "AND MP " & TagString(2) & vbCrLf
                                                                MessageString = MessageString & "ON " & TagString(3) & " TRACK" & vbCrLf
                                                            Case "INFO"
                                                                If TagString.Length < 1 Then Exit Select
                                                                MessageString = MessageString & "INSTRUCTIONS: " & TagString(0) & vbCrLf
                                                        End Select

                                                        Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(trkTag.TrackGuid)
                                                        UserMessage = MessageBox.Show(TMDS.SystemForms.MySystemForms.frmDisplay, MessageString, "RESTRICTIVE TRACK LABEL", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                                        If UserMessage = 1 Then
                                                            MessageString = "SELECT OK TO TRACK RESTRICTION. (TAG TYPE:" & trkTag.TagType.ToString.ToUpper & ")(TRACK UID:" & trkTag.TrackGuid & ")(TAG DATA:" & trkTag.DataString & ")"
                                                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, MessageString, SystemEventQueue)
                                                            SysEvent = Nothing
                                                            Me.SignalCtlStatus = CtlStatus.RequestingClear
                                                        Else
                                                            Me.SignalCtlStatus = CtlStatus.RequestedStop
                                                            PendingRouteShow = False
                                                            Me.ClearPendingRoute()

                                                            If Me.StoredEntrySignal Then
                                                                Dim sr As StoredRoute = Me.myControlPoint.StoredRoutes.Item(0)
                                                                If Not sr Is Nothing Then
                                                                    sr.RemoveMe("GetTagsInCallOnTracks1", True)
                                                                    'TMDS.SystemForms.MySystemForms.frmSystemSummary.UpdateStoredRouteSummary()
                                                                End If
                                                            End If

                                                            If Me.FleetEnabled Then
                                                                Me.DoSetSignalFleeting(False, True)
                                                            End If
                                                            'added in 1.0.0.409
                                                            'SignalRouteCollection.Clear()
                                                            If Me.SignalIndStatus = IndStatus.Stopped Then
                                                                SignalRouteCollection.Clear()
                                                            End If
                                                            'added in 1.0.0.409
                                                            Return False
                                                        End If

                                                        'If ShowRoute Then
                                                        '    PendingRouteShow = False
                                                        '    Me.ClearPendingRoute()
                                                        'End If
                                                    End If
                                                End If
                                            Next
                                            If ShowRoute Then
                                                PendingRouteShow = False
                                                Me.ClearPendingRoute()
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        Else
                            If comp.TrackTagLabels.Count > 0 Then
                                Dim trkTag As Tag
                                Dim trk2 As Track = CType(comp, Track)
                                Dim TagList As ArrayList = trk2.TrackTagLabels.Clone
                                For Each trkTag In TagList
                                    If trkTag.Restriction Then
                                        If trkTag.Direction = "BOTH" Or trkTag.Direction = DirectionOfRoute Then
                                            If ShowRoute Then
                                                PendingRouteShow = True
                                                Me.tmrShowPendingRoute.Enabled = True
                                            End If
                                            Dim TagString() As String
                                            Dim UserMessage As Integer
                                            Dim MessageString As String = trkTag.TagType.ToString.ToUpper & " TRACK RESTRICTION: " & vbCrLf & vbCrLf
                                            TagString = trkTag.DataString.Split("|")
                                            Select Case trkTag.TagType.ToString.ToUpper
                                                Case "XING"
                                                    If TagString.Length < 2 Then Exit Select
                                                    MessageString = MessageString & "PROTECT XING AT MP " & TagString(0) & vbCrLf
                                                    MessageString = MessageString & "XING NAME: " & TagString(1) & vbCrLf
                                                Case "SPEED"
                                                    'Dim TagData As String = Me.txtMPHP.Text & "|" & Me.txtFromMP.Text & "|"
                                                    'TagData = TagData & Me.txtToMP.Text & "|" & Me.txtTrack.Text & "|" & NoFlags.ToString & "|"
                                                    'TagData = TagData & Me.txtMPHF.Text & "|" & Me.txtFromMPA.Text & "|" & Me.txtToMPA.Text & "|"

                                                    If TagString.Length < 5 Then Exit Select
                                                    MessageString = MessageString & "SPEED RESTRICTION FOR PASSENGER TRAINS AT " & TagString(0) & " MPH" & vbCrLf
                                                    MessageString = MessageString & "SPEED RESTRICTION FOR FREIGHT TRAINS AT " & TagString(5) & " MPH" & vbCrLf
                                                    MessageString = MessageString & "BETWEEN MP " & TagString(1) & vbCrLf
                                                    MessageString = MessageString & "AND MP " & TagString(2) & vbCrLf
                                                    MessageString = MessageString & "ON " & TagString(3) & " TRACK" & vbCrLf
                                                Case "INFO"
                                                    If TagString.Length < 1 Then Exit Select
                                                    MessageString = MessageString & "INSTRUCTIONS: " & TagString(0) & vbCrLf
                                            End Select

                                            Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(trkTag.TrackGuid)
                                            UserMessage = MessageBox.Show(TMDS.SystemForms.MySystemForms.frmDisplay, MessageString, "RESTRICTIVE TRACK LABEL", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                                            If UserMessage = 1 Then
                                                MessageString = "SELECT OK TO TRACK RESTRICTION. (TAG TYPE:" & trkTag.TagType.ToString.ToUpper & ")(TRACK UID:" & trkTag.TrackGuid & ")(TAG DATA:" & trkTag.DataString & ")"
                                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, MessageString, SystemEventQueue)
                                                SysEvent = Nothing
                                                Me.SignalCtlStatus = CtlStatus.RequestingClear
                                            Else
                                                Me.SignalCtlStatus = CtlStatus.RequestedStop
                                                PendingRouteShow = False
                                                Me.ClearPendingRoute()
                                                If Me.StoredEntrySignal Then
                                                    Dim sr As StoredRoute = Me.myControlPoint.StoredRoutes.Item(0)
                                                    If Not sr Is Nothing Then
                                                        sr.RemoveMe("GetTagsInCallOnTracks", True)
                                                        'TMDS.SystemForms.MySystemForms.frmSystemSummary.UpdateStoredRouteSummary()
                                                    End If
                                                End If

                                                If Me.FleetEnabled Then
                                                    Me.DoSetSignalFleeting(False, True)
                                                End If

                                                'added in 1.0.0.409
                                                'SignalRouteCollection.Clear()
                                                If Me.SignalIndStatus = IndStatus.Stopped Then
                                                    SignalRouteCollection.Clear()
                                                End If
                                                'added in 1.0.0.409
                                                Return False
                                            End If

                                            'If ShowRoute Then
                                            '    PendingRouteShow = False
                                            '    Me.ClearPendingRoute()
                                            'End If

                                        End If
                                    End If
                                Next
                                If ShowRoute Then
                                    PendingRouteShow = False
                                    Me.ClearPendingRoute()
                                End If
                            End If
                        End If

                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Function CheckForFailedSignals(ByVal trk As Track) As Boolean
            Try

                Dim LeftSig As Signal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(trk.LeftLink)
                Dim RightSig As Signal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(trk.RightLink)

                If Not IsNothing(LeftSig) Then
                    If LeftSig.FailState Then
                        Return False
                    End If
                End If
                If Not IsNothing(RightSig) Then
                    If RightSig.FailState Then
                        Return False
                    End If
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Function GetClearSignalRoute(Optional ByVal CheckForTags As Boolean = True,
        Optional ByVal ShowRoute As Boolean = False,
        Optional ByVal CallOn As Boolean = False,
        Optional ByVal SignalAlreadyControlClear As Boolean = False,
        Optional ByVal FleetCommand As Boolean = False,
        Optional ByVal CheckForClearingIntoFailedTrack As Boolean = False,
        Optional ByVal IgnoreValidTWCAuthorityOnSignalClear As Boolean = False,
        Optional ByVal isTriggerFromStoredRoute As Boolean = False,
        Optional ByVal pTrain As TrainID = Nothing) As Boolean
            Dim EndOfRouteFound As Boolean
            Dim CompToCheck As Object
            Dim SearchRight As Boolean
            Dim FlipFlopFound As Boolean
            Dim DirectionOfRoute As String
            Dim NextReferenceGUID, LastReferenceGUID As Int32
            Dim CallOnTracks As New ArrayList()
            Dim LoopControl As Integer
            Dim myUID As String = Me.Name & " (" & Me.GUID.ToString & ")"
            Dim CheckPerformedForClearingIntoFailedTrack As Boolean
            Dim CheckPerformedForClearingIntoFailedRemoteTrack As Boolean
            Dim LastTrack As Track = Nothing
            Dim CntrPntInLC As New ArrayList
            Try

                'set the direction of the search
                If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                    SearchRight = True
                    DirectionOfRoute = "RIGHT"
                Else
                    DirectionOfRoute = "LEFT"
                End If

                'init the first link to valid
                If SearchRight Then
                    NextReferenceGUID = Me.RightLink
                Else
                    NextReferenceGUID = Me.LeftLink
                End If

                'begin the validation search
                While Not EndOfRouteFound
                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextReferenceGUID) 'FindObjectTypeFromGUID(NextReferenceGUID)
                    'If CompToCheck Is Nothing Then
                    '    AddMessageToFeedback("ROUTE INCOMPLETE: " & myUID & " <NEXT REFERENCE:" & NextReferenceGUID & ">")
                    '    Return False
                    'End If
                    If CompToCheck Is Nothing Then
                        If Not IsNothing(LastTrack) Then
                            If LastTrack.IsLinkToRemoteTrack And Me.DoMonitorMode Then
                                Return True
                            Else
                                AddMessageToFeedback("ROUTE INCOMPLETE: " & myUID & " <NEXT REFERENCE:" & NextReferenceGUID & ">")
                                Return False
                            End If
                        Else
                            AddMessageToFeedback("ROUTE INCOMPLETE: " & myUID & " <NEXT REFERENCE:" & NextReferenceGUID & ">")
                            Return False
                        End If
                    End If
                    Dim compUID As String = " <" & Mid(CompToCheck.GetType.ToString, 23) & " GUID:" & CompToCheck.GUID
                    compUID = " <" & TypeOfCompString(CompToCheck) & " GUID:" & CompToCheck.GUID
                    Select Case CompToCheck.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(CompToCheck, Switch)
                            If FlipFlopFound Then CompToCheck.FlipMe = True
                            'check for opposing traffic if right search
                            If SearchRight And CompToCheck.TrafficDirection = "LEFT" And Mid(CompToCheck.AuthorityType, 1, 3) = "CTC" Then
                                AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:1: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                Dim OverrideCheck As Boolean = False
                                If optEnableGeoFenceOperations Then
                                    OverrideCheck = Me.DoLicenseCheckForRearOfTrain(Nothing, sw)
                                End If
                                If Not OverrideCheck Then Return False
                            End If
                            'check for opposing traffic if left search
                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") And Mid(CompToCheck.AuthorityType, 1, 3) = "CTC" Then
                                AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:2: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                Dim OverrideCheck As Boolean = False
                                If optEnableGeoFenceOperations Then
                                    OverrideCheck = Me.DoLicenseCheckForRearOfTrain(Nothing, sw)
                                End If
                                If Not OverrideCheck Then Return False
                            End If

                            If sw.Occuppied AndAlso Not sw.IgnoreOS Then
                                If Not Me.DisableTrafficProjection Then
                                    AddMessageToFeedback("SWITCH OCCUPPIED IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                Else
                                    AddMessageToFeedback("SWITCH OCCUPPIED IN ROUTE DISABLED: " & myUID & compUID & ">")
                                End If
                            End If

                            'check to see if switch is CTC operations
                            If CompToCheck.AuthorityType <> "CTC" Then
                                AddMessageToFeedback("AUTHORITY TYPE CONFLICT: " & myUID & compUID & ">")
                                Return False
                            End If

                            'check request position of switch
                            If SearchRight And CompToCheck.RightAltLink <> 0 Then
                                If Not CompToCheck.FlipOperations Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.RightLink
                                    Else
                                        NextReferenceGUID = CompToCheck.RightAltLink
                                    End If
                                Else
                                    If CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.RightLink
                                    Else
                                        NextReferenceGUID = CompToCheck.RightAltLink
                                    End If

                                End If
                            ElseIf Not SearchRight And CompToCheck.LeftAltLink <> 0 Then
                                If Not CompToCheck.FlipOperations Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftAltLink
                                    End If
                                Else
                                    If CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftAltLink
                                    End If
                                End If

                            ElseIf LastReferenceGUID = CompToCheck.LeftLink And SearchRight Then
                                If Not CompToCheck.FlipOperations Then
                                    If CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                Else
                                    If Not CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                End If
                            ElseIf LastReferenceGUID = CompToCheck.LeftAltLink And SearchRight Then
                                If Not CompToCheck.FlipOperations Then
                                    If Not CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                Else
                                    If CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                End If
                            ElseIf LastReferenceGUID = CompToCheck.RightLink And Not SearchRight Then
                                If Not CompToCheck.FlipOperations Then
                                    If CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                Else
                                    If Not CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                End If

                            ElseIf LastReferenceGUID = CompToCheck.RightAltLink And Not SearchRight Then

                                If Not CompToCheck.FlipOperations Then
                                    If Not CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                Else
                                    If CompToCheck.ControlledReverse Then
                                        AddMessageToFeedback("SWITCH ALIGNMENT ERROR: " & myUID & compUID & " NORMAL:" & CompToCheck.ControlledReverse.ToString & ">")
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                End If
                            End If

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) Then
                                SignalRouteCollection.Add(CompToCheck)
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(CompToCheck, Signal)
                            If Not FlipFlopFound Then
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic = Me.DefaultTraffic Then
                                    If optForceTrafficDirectionCheckPassSameSignalDirection Then
                                        Dim TrackInFront As Track
                                        If Me.DefaultTraffic = "RIGHT" Then
                                            TrackInFront = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CompToCheck.RightLink)
                                            If Not IsNothing(TrackInFront) Then
                                                If TrackInFront.OStype And TrackInFront.TrafficDirection = "LEFT" Then
                                                    AddMessageToFeedback("OPPOSING LEFT TRAFFIC IN OS ERROR: " & TrackInFront.GUID & ">")
                                                    Return False
                                                End If
                                            End If
                                        Else
                                            TrackInFront = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CompToCheck.LeftLink)
                                            If Not IsNothing(TrackInFront) Then
                                                If TrackInFront.OStype And TrackInFront.TrafficDirection = "RIGHT" Then
                                                    AddMessageToFeedback("OPPOSING RIGHT TRAFFIC IN OS ERROR: " & TrackInFront.GUID & ">")
                                                    Return False
                                                End If
                                            End If
                                        End If
                                    End If
                                    Dim ContinueTheRoute As Boolean
                                    If sig.AutomaticSignal AndAlso Not sig.DisableTrafficProjection Then
                                        ContinueTheRoute = True
                                    End If
                                    If Not ContinueTheRoute Then
                                        EndOfRouteFound = True
                                        Exit While
                                    Else
                                        ContinueTheRoute = False
                                    End If

                                ElseIf CompToCheck.DefaultTraffic <> Me.DefaultTraffic Then
                                    'if opposing signals found not at stop then exit sub
                                    If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return False
                                    If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return False
                                    If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return False
                                End If
                            Else
                                If Not sig.AutomaticSignal Then
                                    'if same direction signal found, then end of route found
                                    If CompToCheck.DefaultTraffic <> Me.DefaultTraffic Then
                                        EndOfRouteFound = True
                                        Exit While
                                    ElseIf CompToCheck.DefaultTraffic = Me.DefaultTraffic Then
                                        'if opposing signals found not at stop then exit sub
                                        If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return False
                                        If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return False
                                        If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return False
                                    End If
                                End If
                            End If
                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If

                            ' Dim sig As Signal = CType(CompToCheck, Signal)
                            sig.NoSignalControlInitialization = False

                            'Track Validation
                        Case "TMDS.TrackComponents.Track"
                            LastTrack = CompToCheck
                            Dim trkToCheck As Track = CType(CompToCheck, Track)
                            If ((trkToCheck.NonTransverable Or trkToCheck.FailState And trkToCheck.myControlPoint.AllStationsInitialized) And Not (gAssistMode OrElse Me.IsShadowMode)) And trkToCheck.OStype And Not Me.HoldingSignal Then
                                AddMessageToFeedback("TRACK CONDITION CONFLICT NON-TRANSVERABLE: " & myUID & ">")
                                Return False
                            End If

                            If FlipFlopFound Then
                                CompToCheck.FlipMe = True
                            Else
                                CompToCheck.FlipMe = False
                            End If

                            If Not optDisallowSignalRwquestsIntoFailedLocations Then
                                If CheckForClearingIntoFailedTrack And gCheckForClearingIntoFailedTrack Then
                                    Dim ErrorMessage As String = String.Empty
                                    Dim lpoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Width / 2 - 100, 150)
                                    If (trkToCheck.FailState Or Not Me.CheckForFailedSignals(trkToCheck)) And Not CheckPerformedForClearingIntoFailedTrack Then
                                        Dim frmMessageBox1 As frmMessageBox
                                        Dim MessageList1 As New ArrayList()
                                        frmMessageBox1 = New frmMessageBox()
                                        'frmMessageBox1.Owner = gBeginTrackSelect.CurrentDrawingForm
                                        MessageList1.Add("A PORTION OF TRACK WITHIN THE LIMITS OF")
                                        MessageList1.Add("THIS REQUEST IS IN COMMUNICATION FAILURE.")
                                        MessageList1.Add("")
                                        MessageList1.Add("DO YOU WANT TO CONTINUE?")
                                        frmMessageBox1.SetMessage(MessageList1, "COMMUNICATION FAILURE CONFLICT CHECK", True, lpoint)
                                        gCritcalPromptExists = True
                                        frmMessageBox1.ShowDialog()
                                        If frmMessageBox1.FormFeedBack <> 6 Then
                                            ErrorMessage = "DISPATCHER SELECT NO TO CONTINUE WITH SIGNAL CLEAR ACCOUNT FAILED TRACK FOUND IN THE LIMITS OF THE SIGNAL REQUEST."
                                            Me.SignalEventLog(ErrorMessage)
                                            frmMessageBox1.Dispose()
                                            frmMessageBox1 = Nothing
                                            gCritcalPromptExists = False
                                            Return False
                                        End If
                                        gCritcalPromptExists = False
                                        ErrorMessage = "DISPATCHER SELECT YES TO CONTINUE WITH SIGNAL CLEAR ACCOUNT FAILED TRACK FOUND IN THE LIMITS OF THE SIGNAL REQUEST."
                                        Me.SignalEventLog(ErrorMessage)
                                        frmMessageBox1.Dispose()
                                        frmMessageBox1 = Nothing
                                        If trkToCheck.IsRemoteTrack Then
                                            CheckPerformedForClearingIntoFailedRemoteTrack = True
                                        Else
                                            CheckPerformedForClearingIntoFailedTrack = True
                                        End If
                                    End If

                                End If
                            Else
                                If (trkToCheck.FailState Or Not Me.CheckForFailedSignals(trkToCheck)) Then
                                    AddMessageToFeedback("TRACK CONDITION CONFLICT COMMUNICATION FAILURE: " & myUID & ">")
                                    Return False
                                End If
                            End If

                            If Not CallOn Then
                                If Not Me.DisableTrafficProjection Then
                                    If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") And Mid(CompToCheck.AuthorityType, 1, 3) = "CTC" And Not trkToCheck.OverrideRightTrafficCheck Then
                                        AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:3: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                        Dim OverrideCheck As Boolean = False
                                        If optEnableGeoFenceOperations Then
                                            OverrideCheck = Me.DoLicenseCheckForRearOfTrain(CompToCheck, Nothing)
                                        End If
                                        If optPromptTrafficDirectionDivergent Then
                                            Dim frmMsg As New frmMessageBox
                                            Dim message As New ArrayList()
                                            message.Add("BLOCO '" & CompToCheck.Name & "' COM DIREÇAO OPOSTA À ROTA/LICENÇA NO PÁTIO " & CompToCheck.myControlPoint.Name & ".")
                                            message.Add("UTILIZE 'FERRAMENTAS DO SISTEMA->ALTERAR SENTIDO' PARA ALTERAR A DIREÇÃO DO BLOCO!")
                                            frmMsg.SetMessage(message, "BLOCO COM DIREÇÃO OPOSTA", False, Me.CurrentDrawingForm.myLaunchPoint)
                                            frmMsg.ShowDialog()
                                            frmMsg.Dispose()
                                            frmMsg = Nothing
                                            message = Nothing
                                        End If
                                        If Not OverrideCheck Then Return False
                                    End If
                                    'check for opposing traffic if left search
                                    If (SearchRight And CompToCheck.TrafficDirection = "LEFT") And Mid(CompToCheck.AuthorityType, 1, 3) = "CTC" And Not trkToCheck.OverrideLeftTrafficCheck Then
                                        Dim InternalFlipDetect As Boolean
                                        If optEnableFlipFlopDetectionLogic Then
                                            If (CompToCheck.FlipMeLeft And CompToCheck.FlipMeLeft) Or (CompToCheck.FlipMeRight And CompToCheck.FlipMeRight) Then
                                                InternalFlipDetect = True
                                            End If
                                        End If
                                        If Not InternalFlipDetect Then
                                            AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:4: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                            Dim OverrideCheck As Boolean = False
                                            If optEnableGeoFenceOperations Then
                                                OverrideCheck = Me.DoLicenseCheckForRearOfTrain(CompToCheck, Nothing)
                                            End If
                                            If Not OverrideCheck Then Return False
                                        Else
                                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") And Mid(CompToCheck.AuthorityType, 1, 3) = "CTC" And Not trkToCheck.OverrideRightTrafficCheck Then
                                                AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:5: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                                Dim OverrideCheck As Boolean = False
                                                If optEnableGeoFenceOperations Then
                                                    OverrideCheck = Me.DoLicenseCheckForRearOfTrain(CompToCheck, Nothing)
                                                End If
                                                If Not OverrideCheck Then Return False
                                            End If
                                        End If
                                    End If
                                Else
                                    AddMessageToFeedback("DISABLE TRAFFIC PROTECTION ENABLED: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                End If
                                If optForceTrafficDirectionCheckForSignalClearing Then
                                    If trkToCheck.TrafficDirection = "" And trkToCheck.Occuppied Then
                                        AddMessageToFeedback("NO TRAFFIC SET IN ROUTE: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                        Return False
                                    End If
                                End If
                            Else
                                Dim FirstNonOSTrack As Boolean
                                If Not CompToCheck.OStype And CompToCheck.Occuppied Then
                                    FirstNonOSTrack = True
                                    'CompToCheck.CallOn = True
                                ElseIf Not CompToCheck.OStype And Not CompToCheck.Occuppied And Not FirstNonOSTrack Then
                                    If Not CallOnAllowedWithoutOpposingTraffic Then
                                        AddMessageToFeedback("CALL ON CONDITIONS NOT MET: " & myUID & compUID & " TRAFFIC:" & CompToCheck.TrafficDirection & ">")
                                        Return False
                                    End If
                                ElseIf FirstNonOSTrack Then
                                    'CompToCheck.CallOn = True
                                End If
                            End If

                            'CATENARY CHECK FOR ENERGIZED CIRCUIT
                            If optEnableTractionPowerManagementSystem Then
                                If trkToCheck.EnableTractionPowerManagement AndAlso Not IsNothing(trkToCheck.myCatenaryTrack) AndAlso Not trkToCheck.myCatenaryTrack.IsEnergized Then
                                    AddMessageToFeedback(" CATENARY OVER TRACK IS DE-ENERGIZED: " & myUID & compUID & ">")
                                    Return False
                                End If
                            End If
                            'check for occupied OS track types
                            If (CompToCheck.OStype And CompToCheck.Occuppied) And Not optEnableSignalClearIntoOccupancy Then
                                If Not Me.DisableTrafficProjection Then
                                    AddMessageToFeedback("OS TRACK OCCUPPIED IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                Else
                                    AddMessageToFeedback("OS TRACK OCCUPPIED IN ROUTE DISABLED: " & myUID & compUID & ">")
                                End If
                            End If
                            'check for HoldingSignalSlotting  track types
                            If optEnableBRCSpecificChecks AndAlso (CompToCheck.HoldingSignalSlottingTrack And CompToCheck.Occuppied) And Not optEnableSignalClearIntoOccupancy Then
                                Select Case DirectionOfRoute
                                    Case "RIGHT"
                                        If CompToCheck.HoldingSignalOSRightBoundReporting Then
                                        Else
                                            AddMessageToFeedback("OS TRACK OCCUPPIED IN ROUTE: " & myUID & compUID & ">")
                                            Return False
                                        End If
                                    Case Else
                                        If CompToCheck.HoldingSignalOSLeftBoundReporting Then
                                        Else
                                            AddMessageToFeedback("OS TRACK OCCUPPIED IN ROUTE: " & myUID & compUID & ">")
                                            Return False
                                        End If
                                End Select
                            End If
                            If Not CallOn Then
                                'check for Track and Time
                                If CompToCheck.TrackAndTime And Not Me.NoSignalControlInitialization And Not SignalAlreadyControlClear Then
                                    AddMessageToFeedback("AUTHORITY CONFLICT IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If
                                If CompToCheck.SelectionMode = GenericComponent.ModeSelection.DeviceBlocking Then
                                    AddMessageToFeedback("PENDING BLOCK IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If

                                If CompToCheck.BlockAbsolute And Not Me.NoSignalControlInitialization And Not SignalAlreadyControlClear Then
                                    AddMessageToFeedback("BLOCK IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If
                            Else
                                'check for Track and Time
                                If CompToCheck.TrackAndTime Then
                                    AddMessageToFeedback("AUTHORITY CONFLICT IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If
                                If CompToCheck.SelectionMode = GenericComponent.ModeSelection.DeviceBlocking Then
                                    AddMessageToFeedback("PENDING BLOCK IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If

                                If CompToCheck.BlockAbsolute Then
                                    AddMessageToFeedback("BLOCK IN ROUTE: " & myUID & compUID & ">")
                                    Return False
                                End If
                            End If


                            If CompToCheck.RemoteLinkShowDown And Not SignalAlreadyControlClear Then
                                'Me.NoSignalControlInitialization = False
                                If Me.myControlPoint.AllStationsInitialized Then
                                    AddMessageToFeedback("CONTROL POINT NOT INITED: " & myUID & compUID & ">")
                                    Return False
                                ElseIf Not Me.NoSignalControlInitialization Then
                                    AddMessageToFeedback("CONTROL POINT NOT INITED: " & myUID & compUID & ">")
                                    Return False
                                End If
                            End If
                            Dim DoNotAddTrack As Boolean

                            Select Case trkToCheck.AuthorityType
                                Case "TWC", "NON-TWC", "SHELF"
                                    EndOfRouteFound = True
                                    DoNotAddTrack = True
                                    If optEnableMRSSpecificOptions And Not Me.DisableTrafficProjection Then
                                        If Not String.IsNullOrEmpty(trkToCheck.TrafficDirection) AndAlso trkToCheck.TrafficDirection.Trim <> "" AndAlso Me.DefaultTraffic <> trkToCheck.TrafficDirection Then
                                            AddMessageToFeedback("OPPOSING TRAFFIC IN ROUTE:6: " & myUID & compUID & " TRAFFIC:" & trkToCheck.TrafficDirection & ">")
                                            Return False
                                        End If
                                    End If
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select

                            If CompToCheck.IsRemoteTrack Then
                                Dim rt As Track = CType(CompToCheck, Track)
                                If Not CompToCheck.AuthorityType = "SHELF" Then
                                    If Not CheckPerformedForClearingIntoFailedRemoteTrack AndAlso Me.SignalIndStatus <> IndStatus.Cleared Then
                                        Dim CurrentCursor As Cursor = Me.CurrentDrawingForm.Cursor
                                        TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(System.Windows.Forms.Cursors.WaitCursor)
                                        If Not CompToCheck.myRemoteTrack.GetSignalClearConfirmation(DirectionOfRoute) Then
                                            If Me.myControlPoint.AllStationsInitialized And Not SignalAlreadyControlClear Then

                                                If Not (Me.JointControlAuthorizationOfficeModeConfirmed) Then
                                                    Me.SignalEventLog("GetSignalClearConfirmationForRemoteLink:" & Me.GUID.ToString & ":False")
                                                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(CurrentCursor)
                                                    AddMessageToFeedback("CONFLICT ON REMOTE LINK TRACK1: " & myUID & compUID & ">")
                                                    Return False
                                                End If

                                            End If
                                        End If
                                        Me.SignalEventLog("GetSignalClearConfirmationForRemoteLink:" & Me.GUID.ToString & ":True")
                                        TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(CurrentCursor)
                                    End If
                                End If
                            ElseIf optEnableShadowModeRemoteLink AndAlso optApplyConfigurationForKCT AndAlso (CompToCheck.IsShadowRemoteTrack) AndAlso CompToCheck.myRemoteTrack IsNot Nothing Then
                                Dim rt As Track = CType(CompToCheck, Track)
                                If Not CompToCheck.AuthorityType = "SHELF" Then
                                    If Not CheckPerformedForClearingIntoFailedRemoteTrack AndAlso Me.SignalIndStatus <> IndStatus.Cleared Then
                                        Dim CurrentCursor As Cursor = Me.CurrentDrawingForm.Cursor
                                        TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(System.Windows.Forms.Cursors.WaitCursor)
                                        If Not CompToCheck.myRemoteTrack.GetSignalClearConfirmation(DirectionOfRoute) Then
                                            If Me.myControlPoint.AllStationsInitialized And Not SignalAlreadyControlClear Then

                                                If Not (Me.JointControlAuthorizationOfficeModeConfirmed) Then
                                                    Me.SignalEventLog("GetSignalClearConfirmationForRemoteLink:" & Me.GUID.ToString & ":False")
                                                    TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(CurrentCursor)
                                                    AddMessageToFeedback("CONFLICT ON REMOTE LINK TRACK2: " & myUID & compUID & ">")
                                                    Return False
                                                End If

                                            End If
                                        End If
                                        Me.SignalEventLog("GetSignalClearConfirmationForRemoteLink:" & Me.GUID.ToString & ":True")
                                        TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(CurrentCursor)
                                    End If
                                End If
                            End If

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) And Not DoNotAddTrack Then
                                If Not CallOn Then
                                    If Not SignalRouteCollection.Contains(CompToCheck) Then
                                        SignalRouteCollection.Add(CompToCheck)
                                    End If
                                Else
                                    If CompToCheck.OStype OrElse CallOnAllowedWithoutOpposingTraffic Then
                                        If Not SignalRouteCollection.Contains(CompToCheck) Then
                                            SignalRouteCollection.Add(CompToCheck)
                                        End If
                                    Else
                                        If Not CallOnTracks.Contains(CompToCheck) Then
                                            CallOnTracks.Add(CompToCheck)
                                        End If
                                    End If
                                End If
                            End If

                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If
                            If optEnableFlipFlopDetectionLogic Then
                                If (CompToCheck.FlipMeLeft And CompToCheck.FlipMeLeft) Or (CompToCheck.FlipMeRight And CompToCheck.FlipMeRight) Then
                                    FlipFlopFound = True
                                    If Not SearchRight Then
                                        SearchRight = True
                                    Else
                                        SearchRight = False
                                    End If
                                End If
                            End If
                            If gOptDisableSignalControlOnManualOperateMode Then
                                If CompToCheck.myControlPoint.ManualOperateMode Then
                                    If Me.myControlPoint.GUID <> CompToCheck.myControlPoint.guid Then
                                        Dim LCMsg As String = CompToCheck.myControlPoint.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE. " & vbCrLf & " DO YOU WANT TO CONTINUE?"
                                        Dim diag As DialogResult
                                        Dim ShowPrompt As Boolean = True
                                        If CntrPntInLC IsNot Nothing AndAlso CntrPntInLC.Count > 0 Then
                                            If CntrPntInLC.Contains(CompToCheck.myControlPoint.Name) Then
                                                ShowPrompt = False
                                            End If
                                        End If

                                        If ShowPrompt Then
                                            diag = MessageBox.Show(LCMsg, "SYSTEM MESSAGE", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                                            If diag = DialogResult.Yes Then
                                                SignalEventLog(" CONTROL POINT: " & CompToCheck.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE. DO YOU WANT TO CONTINUE?  - DISPATCHER(" & gCurrentUser.Initials & ") SELECTED NO")
                                                If Not CntrPntInLC.Contains(CompToCheck.myControlPoint.Name) Then CntrPntInLC.Add(CompToCheck.myControlPoint.Name)
                                            Else
                                                SignalEventLog(" CONTROL POINT: " & CompToCheck.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE. DO YOU WANT TO CONTINUE?  - DISPATCHER(" & gCurrentUser.Initials & ") SELECTED NO")
                                                Return False
                                            End If
                                        End If
                                        'AddMessageToFeedback("CONTROL POINT MANUAL\LOCAL CONTROL: " & myUID & " CONTROL POINT: " & CompToCheck.myControlPoint.Name & ">")
                                        'Return False

                                    Else
                                        ''Disabling this based on CP-RMT 801 
                                        'Dim LCMsg As String = CompToCheck.myControlPoint.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  OPERATION IS NOT VALID"
                                        'MessageBox.Show(LCMsg, "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        'AddMessageToFeedback("CONTROL POINT MANUAL\LOCAL CONTROL: " & myUID & " CONTROL POINT: " & CompToCheck.myControlPoint.Name & ">")
                                        'Return False
                                    End If
                                End If
                            End If

                            ' FlipFlopFound = False
                        Case "TMDS.TrackOtherComponents.RRXING"
                            Dim rr As RRXING = CType(CompToCheck, RRXING)
                            Dim ErrorMessage As String = ""
                            Select Case rr.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select
                            Dim SignalNotInitialized As Boolean = False
                            If Me.NoSignalControlInitialization And Not SignalAlreadyControlClear Then SignalNotInitialized = True
                            If Not optDisableSignalClearCheckForTNTInRRXing Then
                                If Not CompToCheck.ValidateForClearRoute(Me.ComponentType, LastReferenceGUID, SignalNotInitialized, ErrorMessage, myUID) Then
                                    If ErrorMessage IsNot Nothing AndAlso Not String.IsNullOrEmpty(ErrorMessage) AndAlso ErrorMessage.Trim <> "" Then
                                        AddMessageToFeedback(ErrorMessage)
                                    End If
                                    Return False
                                End If

                            Else
                                If Not CompToCheck.ValidateForClearRouteExcludeTNT(Me.ComponentType, LastReferenceGUID, SignalNotInitialized) Then Return False
                            End If


                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) Then
                                SignalRouteCollection.Add(CompToCheck)
                            End If

                            'setup the next component to validate

                            'If SearchRight Then
                            NextReferenceGUID = CompToCheck.FindExitGUID(LastReferenceGUID)
                            'Else
                            '    NextReferenceGUID = CompToCheck.LeftLink
                            'End If
                            LastReferenceGUID = CompToCheck.GUID

                        Case "TMDS.TrackOtherComponents.FlipFlop" '"TMDS.TrackOtherComponents.FlipFlop"
                            Dim ff As FlipFlop = CType(CompToCheck, FlipFlop)
                            'FlipFlopFound = True
                            If FlipFlopFound Then
                                FlipFlopFound = False
                            Else
                                FlipFlopFound = True
                            End If
                            Select Case ff.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select

                            If Not SearchRight Then
                                SearchRight = True
                            Else
                                SearchRight = False
                            End If

                            'If Not CompToCheck.ValidateForClearRoute Then Return False

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) Then
                                SignalRouteCollection.Add(CompToCheck)
                            End If
                            'setup the next component to validate

                            'If SearchRight Then
                            NextReferenceGUID = CompToCheck.FindExitGUID(LastReferenceGUID)
                            'Else
                            '    NextReferenceGUID = CompToCheck.LeftLink
                            'End If
                            LastReferenceGUID = CompToCheck.GUID
                    End Select

                    LoopControl += 1
                    If LoopControl > 100 Then
                        Me.SignalEventLog("LOOP CONTROL EXCEEDED. EXITING GetClearSignalRouteStep2:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        Return False
                    End If
                End While


                'Reset FLIPFLOP
                If optRailRoadName = "BNSF" Then 'OrElse optEnableMetroLinkSpecificOptions
                    ResetFlipFlopComponents(SignalRouteCollection)
                End If

                If Not Me.CheckForInValidRoute Then
                    AddMessageToFeedback("FAILED VALID ROUTE CHECK: " & myUID)
                    Return False
                End If

                If CheckForTags Then
                    If Not Me.GetTagsInCallOnTracks(CallOnTracks, ShowRoute, DirectionOfRoute) Then
                        AddMessageToFeedback("FAILED TAG ROUTE CHECK: " & myUID)
                        Return False
                    End If
                End If

                'Verison 230
                'Added code to check for restrictive track tags
                Dim comp As Object
                Dim frmMessageBox As frmMessageBox = Nothing
                Dim frmReadRepeat As frmReadRepeat = Nothing
                Dim MessageList As New ArrayList()
                Dim FoundTag As Boolean = False
                Dim deliveryedTag As Boolean = False
                Dim ConfirmedTags As New ArrayList
                Dim PreviousOsTrks As New ArrayList()
                If Not Me.FailState And CheckForTags AndAlso Not (gAssistMode OrElse Me.IsShadowMode) Then
                    For Each comp In SignalRouteCollection
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            If comp.OStype Then
                                Dim osTrk As Object
                                Dim CheckOsTrack As Boolean = True
                                Dim trkToCheck As Track
                                For Each trkToCheck In PreviousOsTrks
                                    If trkToCheck.Name = comp.name And trkToCheck.StationNumber = comp.StationNumber Then
                                        CheckOsTrack = False
                                        Exit For
                                    End If
                                Next
                                If CheckOsTrack Then
                                    PreviousOsTrks.Add(comp)
                                    If comp IsNot Nothing AndAlso comp.OsComponentCollection IsNot Nothing Then
                                        For Each osTrk In comp.OsComponentCollection
                                            If osTrk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                                If osTrk.TrackTagLabels.Count > 0 Then
                                                    Dim trkTag As Tag
                                                    Dim trk1 As Track = CType(osTrk, Track)
                                                    Dim TagList As ArrayList = trk1.TrackTagLabels.Clone
                                                    FoundTag = True
                                                    For Each trkTag In TagList 'osTrk.TrackTagLabels
                                                        If trkTag.Restriction And Not ConfirmedTags.Contains(trkTag) Then
                                                            Dim FlipFound1 As Boolean = FailState
                                                            Dim ForcePrompt1 As Boolean
                                                            If Not IsNothing(trk1.myTerritory) Then
                                                                If trk1.myTerritory.FlipDirection Then FlipFound1 = True
                                                            End If
                                                            If FlipFound1 And trkTag.Direction <> DirectionOfRoute Then
                                                                ForcePrompt1 = True
                                                            End If
                                                            If Not FlipFound1 And trkTag.Direction = DirectionOfRoute Then
                                                                ForcePrompt1 = True
                                                            End If
                                                            If trkTag.Direction = "BOTH" Or ForcePrompt1 Then
                                                                If ShowRoute Then
                                                                    PendingRouteShow = True
                                                                    Me.tmrShowPendingRoute.Enabled = True
                                                                End If
                                                                Dim TagString() As String
                                                                MessageList.Add(optTagString21 & trkTag.TagType.ToString.ToUpper & ":")
                                                                MessageList.Add("")
                                                                TagString = trkTag.DataString.Split("|")
                                                                Select Case trkTag.TagType.ToString.ToUpper
                                                                    Case "XING"
                                                                        If TagString.Length < 2 Then Exit Select
                                                                        MessageList.Add(optTagString17 & TagString(0))
                                                                        MessageList.Add(optTagString18 & TagString(1))
                                                                    Case "SPEED"
                                                                        If TagString.Length < 5 Then Exit Select
                                                                        MessageList.Add(optTagString10 & Space(1) & TagString(0) & Space(1) & optTagString12)
                                                                        MessageList.Add(optTagString11 & Space(1) & TagString(5) & Space(1) & optTagString12)
                                                                        MessageList.Add(optTagString13 & Space(1) & TagString(1))
                                                                        MessageList.Add(optTagString14 & Space(1) & TagString(2))
                                                                        MessageList.Add(optTagString15 & Space(1) & TagString(3) & Space(1) & optTagString16)
                                                                    Case "INFO", optTagType3
                                                                        If TagString.Length < 1 Then Exit Select
                                                                        MessageList.Add(optTagString19 & Space(1) & TagString(0))
                                                                    Case "WEATHER"
                                                                        MessageList.Add("CRITICAL WEATHER ALERT")
                                                                End Select
                                                                MessageList.Add("")
                                                                Dim Feedback As Integer = 1
                                                                If optEnableTagReadRepeat Then
                                                                    'Disable message box for track alarms, speed restriction and work zone
                                                                    If trkTag.TagType = optTagType3 Then
                                                                        frmReadRepeat = New frmReadRepeat
                                                                        frmReadRepeat.SetMessage(MessageList, trkTag.TagType.ToUpper, Me.CurrentDrawingForm.myLaunchPoint, "TAG", False, False)
                                                                        frmReadRepeat.ShowDialog()
                                                                        Feedback = frmReadRepeat.FormFeedback
                                                                    Else
                                                                        Feedback = 6
                                                                    End If
                                                                Else
                                                                    Feedback = 6
                                                                End If
                                                                'Send track tag
                                                                If pTrain IsNot Nothing AndAlso pTrain.IsActivePTC Then
                                                                    TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrackTagRestrictionMessageCreate(trkTag, pTrain, trkTag.PTCMessageUID, False, "GetClearSignalRoute-OSTrack1")
                                                                ElseIf gCreateLicenseTrain IsNot Nothing AndAlso gCreateLicenseTrain.IsActivePTC Then
                                                                    TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrackTagRestrictionMessageCreate(trkTag, gCreateLicenseTrain, trkTag.PTCMessageUID, False, "GetClearSignalRoute-OSTrack2")
                                                                End If
                                                                gCritcalPromptExists = True
                                                                Me.CurrentDrawingForm.FormList.Remove(frmMessageBox)
                                                                If Feedback = 6 And (FleetCommand = Me.FleetEnabled) Then
                                                                    Dim MessageString As String = "SELECT OK TO TRACK RESTRICTION. (TAG TYPE:" & trkTag.TagType.ToString.ToUpper & ")(TRACK UID:" & trkTag.TrackGuid & ")(TAG DATA:" & trkTag.DataString & ")"
                                                                    Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, MessageString, SystemEventQueue)
                                                                    SysEvent = Nothing
                                                                    Me.SignalCtlStatus = CtlStatus.RequestingClear
                                                                    ConfirmedTags.Add(trkTag)
                                                                Else
                                                                    Me.SignalCtlStatus = CtlStatus.RequestedStop
                                                                    PendingRouteShow = False
                                                                    Me.ClearPendingRoute()
                                                                    If Me.StoredEntrySignal Then
                                                                        Dim sr As StoredRoute = Me.myControlPoint.StoredRoutes.Item(0)
                                                                        If Not sr Is Nothing Then
                                                                            sr.RemoveMe("GetClearSignalRoute", True)
                                                                        End If
                                                                    End If
                                                                    If Me.FleetEnabled Then
                                                                        Me.DoSetSignalFleeting(False, True)
                                                                    End If
                                                                    If Me.SignalIndStatus = IndStatus.Stopped Then
                                                                        SignalRouteCollection.Clear()
                                                                    End If
                                                                    If optEnableTagReadRepeat Then
                                                                        If frmReadRepeat IsNot Nothing Then
                                                                            frmReadRepeat.Dispose()
                                                                            frmReadRepeat = Nothing
                                                                        End If
                                                                    Else
                                                                        If frmReadRepeat IsNot Nothing Then
                                                                            frmMessageBox.Dispose()
                                                                            frmMessageBox = Nothing
                                                                        End If
                                                                    End If
                                                                    MessageList.Clear()
                                                                    gCritcalPromptExists = False
                                                                    Return False
                                                                End If
                                                                If optEnableTagReadRepeat Then
                                                                    If frmReadRepeat IsNot Nothing Then
                                                                        frmReadRepeat.Dispose()
                                                                        frmReadRepeat = Nothing
                                                                    End If
                                                                Else
                                                                    If frmReadRepeat IsNot Nothing Then
                                                                        frmMessageBox.Dispose()
                                                                        frmMessageBox = Nothing
                                                                    End If
                                                                End If
                                                                MessageList.Clear()
                                                                gCritcalPromptExists = False
                                                            End If
                                                        End If
                                                    Next
                                                    If ShowRoute Then
                                                        PendingRouteShow = False
                                                        Me.ClearPendingRoute()
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            Else
                                If comp.TrackTagLabels.Count > 0 Then
                                    Dim trkTag As Tag
                                    Dim trk2 As Track = CType(comp, Track)
                                    Dim TagList As ArrayList = trk2.TrackTagLabels.Clone
                                    For Each trkTag In TagList
                                        If trkTag.Restriction And Not ConfirmedTags.Contains(trkTag) Then
                                            Dim FlipFound2 As Boolean = FailState
                                            Dim ForcePrompt2 As Boolean
                                            If Not IsNothing(trk2.myTerritory) Then
                                                If trk2.myTerritory.FlipDirection Then FlipFound2 = True
                                            End If

                                            If FlipFound2 And trkTag.Direction <> DirectionOfRoute Then
                                                ForcePrompt2 = True
                                            End If
                                            If Not FlipFound2 And trkTag.Direction = DirectionOfRoute Then
                                                ForcePrompt2 = True
                                            End If
                                            If trkTag.Direction = "BOTH" Or ForcePrompt2 Then
                                                trk2.TagPrompt = True
                                                If ShowRoute Then
                                                    PendingRouteShow = True
                                                    Me.tmrShowPendingRoute.Enabled = True
                                                End If
                                                Dim TagString() As String
                                                MessageList.Add(optTagString21 & trkTag.TagType.ToString.ToUpper & ":")
                                                MessageList.Add("")
                                                TagString = trkTag.DataString.Split("|")
                                                Select Case trkTag.TagType.ToString.ToUpper
                                                    Case "XING", optTagType2
                                                        If TagString.Length < 2 Then Exit Select
                                                        MessageList.Add(optTagString17 & TagString(0))
                                                        MessageList.Add(optTagString18 & TagString(1))
                                                    Case "SPEED", optTagType1
                                                        If TagString.Length < 5 Then Exit Select
                                                        MessageList.Add(optTagString11 & Space(1) & TagString(5) & Space(1) & optTagString12)
                                                        MessageList.Add(optTagString13 & Space(1) & TagString(1))
                                                        MessageList.Add(optTagString14 & Space(1) & TagString(2))
                                                        MessageList.Add(optTagString15 & Space(1) & optTagString16 & Space(1) & TagString(3))
                                                    Case "INFO", optTagType3
                                                        If TagString.Length < 1 Then Exit Select
                                                        MessageList.Add(optTagString19 & Space(1) & TagString(0))
                                                    Case "WEATHER"
                                                        MessageList.Add("CRITICAL WEATHER ALERT")
                                                        MessageList.Add(optTagString19 & TagString(0))
                                                    Case optTagType6 'TRACK ALARM
                                                        MessageList.Add("TÍTULO:" & trkTag.AdvisorySummary)
                                                        MessageList.Add("BITOLA:" & trkTag.trackAlarmGauge)
                                                        MessageList.Add("INFORMAÇÃO:" & trkTag.DataString)
                                                    Case optTagType7 'WORK ZONE
                                                        If TagString.Length < 5 Then Exit Select
                                                        MessageList.Add(optTagString11 & Space(1) & TagString(5) & Space(1) & optTagString12)
                                                        MessageList.Add(optTagString13 & Space(1) & TagString(1))
                                                        MessageList.Add(optTagString14 & Space(1) & TagString(2))
                                                        MessageList.Add(optTagString15 & Space(1) & optTagString16 & Space(1) & TagString(3))
                                                End Select
                                                MessageList.Add("")
                                                Dim Feedback As Integer = 1
                                                If optEnableTagReadRepeat Then
                                                    'Disable message box for track alarms, speed restriction and work zone
                                                    If trkTag.TagType = optTagType3 Then
                                                        frmReadRepeat = New frmReadRepeat
                                                        frmReadRepeat.SetMessage(MessageList, trkTag.TagType.ToUpper, Me.CurrentDrawingForm.myLaunchPoint, "TAG", False)
                                                        frmReadRepeat.ShowDialog()
                                                        Feedback = frmReadRepeat.FormFeedback
                                                    Else
                                                        Feedback = 6
                                                    End If
                                                Else
                                                    Feedback = 6
                                                End If

                                                'Send track tag
                                                If pTrain IsNot Nothing AndAlso pTrain.IsActivePTC Then
                                                    TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrackTagRestrictionMessageCreate(trkTag, pTrain, trkTag.PTCMessageUID, False, "GetClearSignalRoute-Track1")
                                                ElseIf gCreateLicenseTrain IsNot Nothing AndAlso gCreateLicenseTrain.IsActivePTC Then
                                                    TMDS.NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrackTagRestrictionMessageCreate(trkTag, gCreateLicenseTrain, trkTag.PTCMessageUID, False, "GetClearSignalRoute-Track2")
                                                End If

                                                gCritcalPromptExists = True
                                                Me.CurrentDrawingForm.FormList.Remove(frmMessageBox)
                                                If Feedback = 6 And (FleetCommand = Me.FleetEnabled) Then
                                                    SystemEvent("SELECT OK TO TRACK RESTRICTION. (TAG TYPE:" & trkTag.TagType.ToString.ToUpper & ")(TRACK UID:" & trkTag.TrackGuid & ")(TAG DATA:" & trkTag.DataString & ")")
                                                    Me.SignalCtlStatus = CtlStatus.RequestingClear
                                                    ConfirmedTags.Add(trkTag)
                                                Else
                                                    Me.SignalCtlStatus = CtlStatus.RequestedStop
                                                    PendingRouteShow = False
                                                    Me.ClearPendingRoute()
                                                    If Me.StoredEntrySignal Then
                                                        Dim sr As StoredRoute = Me.myControlPoint.StoredRoutes.Item(0)
                                                        If Not sr Is Nothing Then
                                                        End If
                                                    End If
                                                    If Me.FleetEnabled Then
                                                        Me.DoSetSignalFleeting(False, True)
                                                    End If
                                                    If Me.SignalIndStatus = IndStatus.Stopped Then
                                                        SignalRouteCollection.Clear()
                                                    End If
                                                    trk2.TagPrompt = False
                                                    If optEnableTagReadRepeat Then
                                                        If frmReadRepeat IsNot Nothing Then
                                                            frmReadRepeat.Dispose()
                                                            frmReadRepeat = Nothing
                                                        End If
                                                    End If
                                                    MessageList.Clear()
                                                    gCritcalPromptExists = False
                                                    Return False
                                                End If
                                                trk2.TagPrompt = False
                                                If optEnableTagReadRepeat Then
                                                    If frmReadRepeat IsNot Nothing Then
                                                        frmReadRepeat.Dispose()
                                                        frmReadRepeat = Nothing
                                                    End If
                                                End If
                                                MessageList.Clear()
                                                gCritcalPromptExists = False
                                            End If
                                        End If
                                    Next
                                    If ShowRoute Then
                                        PendingRouteShow = False
                                        Me.ClearPendingRoute()
                                    End If
                                End If
                            End If
                            'check to make sure there are no pending remote blocks that came in while the dispatcher is prompted for tag.
                            If comp.BlockAbsolute Or comp.RemotePendingBlock Then
                                Return False
                            End If

                        End If
                    Next
                End If
                PreviousOsTrks = Nothing
                For Each comp In Me.SignalRouteCollection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If comp.IsLinkToRemoteTrack Then
                            'check for opposing traffic if right search
                            If (Not SearchRight And comp.TrafficDirection = "RIGHT") Then
                                Me.SignalRouteCollection.Clear()
                                Return False
                            End If
                            'check for opposing traffic if left search
                            If (SearchRight And comp.TrafficDirection = "LEFT") Then
                                Me.SignalRouteCollection.Clear()
                                Return False
                            End If
                        End If
                    End If
                Next
                If optPerformSidingLengthCheckInCTC Then
                    If Not CheckSidingLengthAgaistTrainLength() Then
                        AddMessageToFeedback("SIDING LENGTH CHECK FAILED: " & myUID)
                        Return False
                    End If
                End If

                If optEnableCheckForValidTWCAuthorityOnSignalClear AndAlso Me.SignalCtlStatus = CtlStatus.RequestedStop Then
                    If Not CheckSignalClearIntoTWCTrack(IgnoreValidTWCAuthorityOnSignalClear) Then
                        AddMessageToFeedback("SIGNAL CLEAR INTO TWC TRACK CHECK FAILED: " & myUID)
                        Return False
                    End If
                End If

                If optDisableControlPointMenuFunctionsOnManualOperate AndAlso optEnableARRCSpecificOptions Then
                    Dim hasLocalControl As Boolean = False
                    If Me.SignalRouteCollection IsNot Nothing Then
                        For Each comp In Me.SignalRouteCollection
                            If comp IsNot Nothing AndAlso comp.myControlPoint.ManualOperateMode And Not gMaintenanceMode Then
                                If Not ValidateManualOperateMode(comp.myControlPoint) Then
                                    hasLocalControl = True
                                    Exit For
                                End If

                            End If
                        Next
                        If hasLocalControl Then
                            AddMessageToFeedback("SIGNAL COMPONENTS IN LOCAL CONTROL: " & myUID)
                            TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                            ClearSystemVariables()
                            Return False
                        End If
                    End If
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(System.Windows.Forms.Cursors.Default)
                gCritcalPromptExists = False
            Finally
                CntrPntInLC = Nothing
            End Try
        End Function

        Private Function CheckForRemoteBlockValidity(TrackToCheck As Track) As Boolean
            Dim ResetRemoteLink As Boolean = False
            Try
                If TrackToCheck.RemotePendingBlock Then Return False
                If TrackToCheck.DeviceBlocks Is Nothing Then
                    ResetRemoteLink = True
                ElseIf TrackToCheck.DeviceBlocks.Count = 0 Then
                    ResetRemoteLink = True
                End If
                If ResetRemoteLink Then
                    TrackToCheck.DoRemoteTrackLinkReset()
                    Return False
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return True
        End Function

        Private Function CheckSidingLengthAgaistTrainLength() As Boolean
            Try
                Dim trn As TrainID = Nothing
                Dim NextComp As Object
                Dim LoopControl As Integer
                Dim SearchRight As Boolean
                If Me.ComponentType = "L" Then
                    SearchRight = True
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.RightLink)
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.LeftLink)
                End If
                If IsNothing(NextComp) Then Return True

                If SearchRight Then
                    Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.TrainIDs.Count > 0 Then
                                trn = trk.TrainIDs.Item(0)
                                Exit Do
                            End If
                            If Mid(trk.AuthorityType, 1, 3) <> "CTC" Then
                                Exit Do
                            End If
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trk.RightLink)
                        ElseIf NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Exit Do
                        End If

                        LoopControl += 1
                        If LoopControl > 20 Then
                            Exit Do
                        End If
                    Loop
                Else
                    Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.TrainIDs.Count > 0 Then
                                trn = trk.TrainIDs.Item(0)
                                Exit Do
                            End If
                            If Mid(trk.AuthorityType, 1, 3) <> "CTC" Then
                                Exit Do
                            End If

                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trk.LeftLink)
                        ElseIf NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Exit Do
                        End If
                        LoopControl += 1
                        If LoopControl > 20 Then
                            Exit Do
                        End If
                    Loop
                End If

                If IsNothing(trn) Then Return True
                Dim SidingTrackList As New ArrayList
                For Each NextComp In Me.SignalRouteCollection
                    If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        Dim trk As Track = CType(NextComp, Track)
                        If trk.TrackName <> "" Then
                            If optEnableTrackSidingBit Then
                                'NEW SIDING LOGIC CODE
                                Dim IsSidingTrack As Boolean = False
                                If trk.EnableNonControlledSiding Or trk.EnableControlledSiding Then IsSidingTrack = True
                                If trk.TrackName.ToUpper = "ESCAPE" Or IsSidingTrack Then
                                    If trk.TrackLength <> 0 Then
                                        If Not SidingTrackList.Contains(trk) Then
                                            SidingTrackList.Add(trk)
                                        End If
                                    End If
                                End If
                            Else
                                'OLD LEGACY SIDING CODE
                                If trk.TrackName.ToUpper = "ESCAPE" Or trk.TrackName.ToUpper = "SIDING" Then
                                    If trk.TrackLength <> 0 Then
                                        If Not SidingTrackList.Contains(trk) Then
                                            SidingTrackList.Add(trk)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next

                If SidingTrackList.Count = 0 Then Return True
                If trn.OLength = 0 Then Return True
                Dim tk As Track
                Dim LaunchPoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + 300, 300)
                For Each tk In SidingTrackList
                    If tk.TrackLength < trn.OLength Then
                        Dim frmMess As frmMessageBox
                        Dim MessList As New ArrayList()
                        frmMess = New frmMessageBox()
                        'frmMessBox.Owner = gBeginTrackSelect.CurrentDrawingForm
                        MessList.Add("")
                        MessList.Add("LONGITUD DEL TREN (" & trn.OLength & ") SOBREPASA LA DEL ESCAPE (" & tk.TrackLength & ")")
                        MessList.Add("CONTINUE?")
                        MessList.Add("")
                        frmMess.SetMessage(MessList, optSystemMessageString2, True, LaunchPoint)
                        frmMess.ShowDialog()
                        If frmMess.FormFeedBack <> 6 Then
                            frmMess.Dispose()
                            frmMess = Nothing
                            Return False
                        End If
                        frmMess.Dispose()
                        frmMess = Nothing
                    End If
                Next
                Return True


            Catch ex As Exception
                SystemException(ex.ToString)
                Return True
            End Try
        End Function

        Public Function CheckSignalClearIntoTWCTrack(Optional ByVal IgnoreValidTWCAuthorityOnSignalClear As Boolean = False, Optional CompCollection As ArrayList = Nothing, Optional IsPSS As Boolean = False) As Boolean

            Dim LastTrkInSigCollection As Track = Nothing
            Dim ObjAbbutingLastTrkInSigCollection As Object = Nothing
            Dim mySignalClearDirection As String = ""
            Dim LaunchPoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + 300, 300)
            Dim MessageList As New ArrayList
            Dim Auth As Object = Nothing
            Dim HasFlipFlop As Boolean = False
            Dim comp As Object = Nothing
            Dim myCompType As String = ""
            Dim CompCollectionToCheck As New ArrayList

            Try
                If optEnableShadowMode OrElse optEnableTerritoryDeskAssignmentMode Then
                    If Me.myControlPoint.IsControlled Then
                    Else
                        Return True
                    End If
                End If
                If IsPSS Then
                    If Me.SignalRouteCollection IsNot Nothing AndAlso Me.SignalRouteCollection.Count > 0 Then
                        CopyArrayCollection(CompCollectionToCheck, Me.SignalRouteCollection)
                    Else
                        If optEnableARRCSpecificOptions AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                            CopyArrayCollection(CompCollectionToCheck, CompCollection)
                        Else
                            CompCollectionToCheck = TMDS.SystemUtilities.FetchData.FindMySignalRoute(Me)
                        End If

                    End If
                Else
                    If CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                        CopyArrayCollection(CompCollectionToCheck, CompCollection)
                    Else
                        CopyArrayCollection(CompCollectionToCheck, Me.SignalRouteCollection)
                    End If
                End If


                LastTrkInSigCollection = TMDS.SystemUtilities.FetchData.FindLastTrackInList(CompCollectionToCheck)
                For Each comp In CompCollectionToCheck
                    If comp.GetType.ToString = gFlipFlopComponentName Then
                        If HasFlipFlop Then
                            HasFlipFlop = False
                        Else
                            HasFlipFlop = True
                        End If
                    End If
                Next
                Select Case Me.ComponentType.ToUpper.Trim
                    Case "R", "D"
                        myCompType = "R"
                        If HasFlipFlop Then
                            mySignalClearDirection = "LEFT"
                        Else
                            mySignalClearDirection = "RIGHT"
                        End If
                    Case Else
                        myCompType = "L"
                        If HasFlipFlop Then
                            mySignalClearDirection = "RIGHT"
                        Else
                            mySignalClearDirection = "LEFT"
                        End If
                End Select


                Dim InvalidSelection As Boolean = False
                If LastTrkInSigCollection IsNot Nothing Then
                    Select Case mySignalClearDirection.ToUpper.Trim
                        Case "RIGHT"
                            ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(LastTrkInSigCollection.RightLink)
                        Case Else
                            ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(LastTrkInSigCollection.LeftLink)
                    End Select


                    If ObjAbbutingLastTrkInSigCollection IsNot Nothing AndAlso ObjAbbutingLastTrkInSigCollection.GetType.ToString = gSignalComponentName Then
                        Dim TempSigType As String = ""
                        Select Case ObjAbbutingLastTrkInSigCollection.ComponentType.ToUpper.Trim.ToString
                            Case "R", "D"
                                TempSigType = "R"
                            Case Else
                                TempSigType = "L"
                        End Select

                        If HasFlipFlop AndAlso TempSigType = myCompType Then
                            Select Case mySignalClearDirection.ToUpper.Trim
                                Case "RIGHT"
                                    ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(ObjAbbutingLastTrkInSigCollection.RightLink)
                                Case Else
                                    ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(ObjAbbutingLastTrkInSigCollection.LeftLink)
                            End Select
                        ElseIf Not HasFlipFlop AndAlso TempSigType <> myCompType Then
                            Select Case mySignalClearDirection.ToUpper.Trim
                                Case "RIGHT"
                                    ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(ObjAbbutingLastTrkInSigCollection.RightLink)
                                Case Else
                                    ObjAbbutingLastTrkInSigCollection = FindCompFromGUID(ObjAbbutingLastTrkInSigCollection.LeftLink)
                            End Select
                        End If
                    End If



                    If ObjAbbutingLastTrkInSigCollection IsNot Nothing Then
                        If ObjAbbutingLastTrkInSigCollection.GetType.ToString = gTrackComponentName Then
                            If Mid(ObjAbbutingLastTrkInSigCollection.AuthorityType, 1, 3) = "TWC" Then
                                If ObjAbbutingLastTrkInSigCollection.TrackAuthorities Is Nothing OrElse ObjAbbutingLastTrkInSigCollection.TrackAuthorities.Count = 0 Then
                                    InvalidSelection = True
                                ElseIf ObjAbbutingLastTrkInSigCollection.TrackAuthorities IsNot Nothing AndAlso ObjAbbutingLastTrkInSigCollection.TrackAuthorities.Count > 0 Then

                                    InvalidSelection = True
                                    For Each Auth In ObjAbbutingLastTrkInSigCollection.TrackAuthorities
                                        If Auth.GetType.ToString = gSFAComponentName AndAlso Auth.IssuedToTrain Then
                                            If Auth.AuthType1 = gSFAAuthorityTypeItem4 Then
                                                InvalidSelection = False : Exit For
                                            ElseIf Auth.Direction.ToUpper.Trim = mySignalClearDirection.ToUpper.Trim Then
                                                InvalidSelection = False : Exit For
                                            End If
                                        End If

                                    Next
                                End If
                            End If
                        End If
                    End If
                End If
                'cschock: ARRC-1222 - disable message in dispatch assist mode
                If InvalidSelection And Not gAssistMode Then
                    If Not IsPSS Then
                        MessageList.Add("INVALID SIGNAL REQUEST")
                    Else

                        MessageList.Add("INVALID " & gMenuPermissionToPassRedSignalText & " REQUEST")
                    End If

                    MessageList.Add("ADJACENT " & gTWCVariableName & " TRACK DOES NOT HAVE AN AUTHORITY.")


                    If Not IgnoreValidTWCAuthorityOnSignalClear Then
                        If Not IsPSS Then
                            MessageList.Add("SIGNAL REQUEST IS DENIED")
                        Else
                            MessageList.Add(gMenuPermissionToPassRedSignalText & " REQUEST IS DENIED")
                        End If
                        ShowCustomMessageBox(MessageList, "SIGNAL CONFLICT CHECK", False, LaunchPoint)
                        Return False
                    Else

                        If Not IsPSS Then
                            MessageList.Add("CONTINUE WITH SIGNAL REQUEST?")
                        Else
                            MessageList.Add("CONTINUE WITH " & gMenuPermissionToPassRedSignalText & " REQUEST?")
                        End If
                        Dim Result As Integer = ShowCustomMessageBox(MessageList, "SIGNAL CONFLICT CHECK", True, LaunchPoint)
                        If Result <> 6 Then
                            Return False
                        Else
                            Return True
                        End If
                    End If



                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                LaunchPoint = Nothing
                Auth = Nothing
                MessageList = Nothing
                LastTrkInSigCollection = Nothing
                ObjAbbutingLastTrkInSigCollection = Nothing
            End Try
            Return True
        End Function

        Sub SlotToStop(Optional ByVal ForceStatusChangeOnTrack As Boolean = True, Optional ByVal PulseSignalToStop As Boolean = False)
            Try
                'If Me.IsShadowMode Then Exit Sub
                Dim PreviousSignalSignalStatus As BulbStatus = Me.SignalStatus
                Me.SignalStatus = BulbStatus.StopStatus
                Me.SignalCtlStatus = CtlStatus.RequestedStop
                'Me.CurrentColorStatus = (New SolidBrush(Color.Red))
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                Me.RemoveFromFlashCollection(Me)
                Flasher.Enabled = False
                FlashComponent = False
                Dim CompInSelection As Object
                'set the traffic for each switch and track
                Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already


                For Each CompInSelection In SignalRouteCollection
                    Select Case CompInSelection.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim trk1 As Track = CType(CompInSelection, Track)
                            If Not TrafficRouteEstablished Then
                                Dim FirstOSSectionOccupied As Boolean
                                If CompInSelection.OsType Then
                                    Dim OsTrk As Object
                                    For Each OsTrk In CompInSelection.OsComponentCollection
                                        If OsTrk.Occuppied Then
                                            FirstOSSectionOccupied = True
                                        End If
                                    Next
                                ElseIf trk1.HoldingSignalSlottingTrack Then
                                    If trk1.Occuppied Then
                                        FirstOSSectionOccupied = True
                                    End If
                                ElseIf Me.SignalSlotting Then
                                    'Single holding signal check
                                    If Me.DefaultTraffic = "LEFT" Then
                                        If CompInSelection.HoldingSignalOsLeft Then
                                            If Not IsNothing(CompInSelection.Ind(0)) Then
                                                If CompInSelection.Ind(0).BitState = True Then
                                                    FirstOSSectionOccupied = True
                                                End If
                                            End If
                                        End If
                                    ElseIf Me.DefaultTraffic = "RIGHT" Then
                                        If CompInSelection.HoldingSignalOsRight Then
                                            If Not IsNothing(CompInSelection.Ind(0)) Then
                                                If CompInSelection.Ind(0).BitState = True Then
                                                    FirstOSSectionOccupied = True
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                                If FirstOSSectionOccupied And CompInSelection.TrafficDirection <> "" Then
                                    TrafficRouteEstablished = True
                                End If
                            End If
                            If Not TrafficRouteEstablished And ForceStatusChangeOnTrack Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                                CompInSelection.FlipMe = False
                            End If
                        Case "TMDS.SwitchComponents.Switch"
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Switch.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.UpdateStatus()
                            End If
                    End Select
                Next

                If Me.SimulateMode Then
                    If Not Me.Ind(0) Is Nothing Then
                        Ind(0).BitState = False
                        Me.ApplyIndicationChange()
                    End If
                End If
                Me.tmrCheckRoute.Enabled = False
                SignalRouteCollection.Clear()

                'set control bits accordingly
                If Not Me.Ctl(0) Is Nothing Then
                    Me.Ctl(0).BitState = False
                End If
                If Not Me.Ctl(3) Is Nothing Then
                    Me.Ctl(3).BitState = False
                End If

                If Not Me.Ctl(1) Is Nothing Then
                    Me.Ctl(1).BitState = False
                End If
                If Not Me.Ctl(4) Is Nothing Then
                    Me.Ctl(4).BitState = False
                End If

                If Me.EnablePulseStopControl Then
                    If PulseSignalToStop Then
                        If Not Me.Ctl(2) Is Nothing Then
                            Me.Ctl(2).BitState = True
                        End If
                        If Not Me.Ctl(5) Is Nothing Then
                            Me.Ctl(5).BitState = True
                        End If
                    Else
                        If Not Me.Ctl(2) Is Nothing Then
                            Me.Ctl(2).BitState = False
                        End If
                        If Not Me.Ctl(5) Is Nothing Then
                            Me.Ctl(5).BitState = False
                        End If

                    End If
                Else
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                End If

                If Me.NearSideSignalEnabled Then
                    If Not IsNothing(Me.NearSideSignalCtlBit) Then
                        Me.NearSideSignalCtlBit.BitState = False
                        Me.NearSideSignalControlUpdateStatus()
                    End If
                End If

                If Me.JointControlAuthorizationEnabled Then
                    Me.JointControlAuthorizationOff(True)
                End If

                If Not IsNothing(Me.CallOnControl) Then
                    If Not optEnableCallOnLatching Then
                        Me.CallOnControl.BitState = False
                        SignalCalledOn = False
                    End If
                End If

                'put station in queue to send a control to stop signal
                If Me.LastCtlStatus <> Me.SignalCtlStatus Then
                    If Not Me.FailState Then
                        Me.myControlPoint.SendCodeStationControlUpdate()
                        'Me.SendSignalControlUpdate()
                    End If
                End If
                Me.LastCtlStatus = Me.SignalCtlStatus

                If Me.ForceStopControl Then
                    Me.SendSignalStopAutomatically(True)
                    Me.SignalEventLog("SIGNAL SLOTTED TO STOP AND CONTROL SENT TO FIELD:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                Else
                    Me.SignalEventLog("SIGNAL SLOTTED TO STOP AND NO CONTROL SENT TO FIELD:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                End If
                If Me.IndicateOnlyWithControlSetEnabled And Not Me.myControlPoint.FailState Then
                    Me.SignalControlled = False
                    SignalControlUpdateDatabase()
                End If

                If Me.AutomaticModeOn Then
                    If Me.ControlledByTacticalRoute Then
                        Me.CurrentTacticalRoute.UpdateRouteStatus("COMPLETED")
                        'Me.CurrentTacticalRoute.RemoveRoute()
                        'Me.CurrentTacticalRoute = Nothing
                        Me.ControlledByTacticalRoute = False
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub DoSetSignalFleeting(ByVal SetToFleet As Boolean, ByVal SendUpdateStatus As Boolean, Optional ByVal SendAutoControl As Boolean = True)
            Try

                If SetToFleet And Me.myControlPoint.StoredRoutes.Count > 0 Then
                    Dim sr As StoredRoute

                    For Each sr In Me.myControlPoint.StoredRoutes
                        If sr.EntrySignal Is Me Then
                            Exit Sub
                        End If
                        If sr.ExitSignal Is Me Then
                            Exit Sub
                        End If
                        Dim Comp As Object
                        Dim RouteComp As Object
                        For Each Comp In sr.ComponentSelection
                            For Each RouteComp In Me.SignalRouteCollection
                                If Comp Is RouteComp Then
                                    Exit Sub
                                End If
                            Next
                        Next
                    Next
                End If
                If optDisableFleetSignalOnConflictingStoredRoute AndAlso SetToFleet Then
                    If CheckForConflictingStoredRoutes() Then Exit Sub
                End If
                Me.FleetEnabled = SetToFleet
                If Not (gAssistMode OrElse Me.IsShadowMode) Then
                    Dim PublishEvent As New DoPublishSignalEvent(AddressOf Me.PublishSignalEventUpdate)

                    If SetToFleet Then
                        If Not IsNothing(Me.FleetControl) Then
                            Me.FleetControl.BitState = True
                            If Not Me.FailState Then
                                Me.SendSignalControl(False)
                            End If
                            Me.LastCtlStatus = Me.SignalCtlStatus
                        End If


                        SignalEventLog("FLEETING TURNED ON FOR SIGNAL:" & Me.Name & "-" & Me.GUID.ToString)
                        PublishEvent.BeginInvoke(EventTypeSig.FLEETON, Nothing, Nothing)
                    Else
                        If Not IsNothing(Me.FleetControl) Then
                            Me.FleetControl.BitState = False
                            If SendAutoControl Then
                                If Not Me.FailState Then
                                    Me.SendSignalControl(False)
                                End If
                                Me.LastCtlStatus = Me.SignalCtlStatus
                            End If
                        End If

                        SignalEventLog("FLEETING TURNED OFF FOR SIGNAL:" & Me.Name & "-" & Me.GUID.ToString)
                        PublishEvent.BeginInvoke(EventTypeSig.FLEETOFF, Nothing, Nothing)
                        Me.tmrFleetRetry.Enabled = False
                    End If
                End If
                If (SendUpdateStatus And Not Me.FailState) OrElse (gAssistMode OrElse Me.IsShadowMode) Then
                    'Me.UpdateStatus()
                    Me.InvalidateSignal()
                End If
                'Me.InvalidateSignal()
            Catch ex As Exception
                Me.tmrFleetRetry.Enabled = False
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToRequestingClear()
            Try
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))
                Me.AddToFlashCollection(Me)
                FlashComponent = True
                Flasher.Enabled = True
                Dim CompInSelection As Object
                'set the traffic for each switch and track
                Dim EndOfRoute As Boolean
                For Each CompInSelection In SignalRouteCollection
                    If CompInSelection.Occuppied And CompInSelection.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then EndOfRoute = True
                    If Not EndOfRoute Then
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequested
                        CompInSelection.CurrentColorStatus = New SolidBrush(Color.Yellow)
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToRequestingStop()
            Try
                'Me.SignalCtlStatus = CtlStatus.RequestedStop
                Me.SignalCtlStatus = CtlStatus.RequestingStop
                'set signal status
                If Me.BlockAbsolute Then
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkViolet))
                Else
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                End If

                Me.AddToFlashCollection(Me)
                FlashComponent = True
                Flasher.Enabled = True

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToUnSolicated()

            Try

                Me.SignalCtlStatus = CtlStatus.RequestedStop
                Me.SignalStatus = BulbStatus.Unsolitated
                If Me.BlockAbsolute Then
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkViolet))
                Else
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.MediumSpringGreen))
                End If

                Me.AddToFlashCollection(Me)
                Flasher.Enabled = True
                FlashComponent = True
                Dim CompInSelection As Object
                Me.GetSignalRoute()
                'set the traffic for each switch and track
                Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already
                For Each CompInSelection In SignalRouteCollection
                    If CompInSelection.Occuppied Or CompInSelection.TrafficDirection <> "" Then
                        If CompInSelection.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            TrafficRouteEstablished = True
                        End If
                    End If

                    If Not TrafficRouteEstablished Then
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated
                        If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                            CompInSelection.TrafficDirection = "RIGHT"
                        Else
                            CompInSelection.TrafficDirection = "LEFT"
                        End If
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next

                Me.ReSetFlasherCollection()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToClear()
            Try
                'Dim PreviousSignalSignalStatus As BulbStatus = Me.SignalStatus
                Me.SignalStatus = BulbStatus.ClearStatus
                Me.SignalCtlStatus = CtlStatus.RequestedClear
                Me.SignalIndOnlyStatusPrevious = Me.SignalIndStatus
                'Me.CurrentColorStatus = (New SolidBrush(Color.Green))
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.Green))
                Me.RemoveFromFlashCollection(Me)
                Flasher.Enabled = False
                FlashComponent = False
                Dim CompInSelection As Object
                'set the traffic for each switch and track
                For Each CompInSelection In SignalRouteCollection
                    Try
                        If CompInSelection.TrafficDirection = "" Then
                            CompInSelection.TrafficDirection = Me.DefaultTraffic
                        End If
                    Catch ex As Exception
                        SystemException(ex.ToString)
                    End Try
                    'AutoBlock switches functionality - add
                    If CompInSelection.GetType.ToString = gSwitchComponentName Then
                        AddAutoBlock(CompInSelection)
                    End If
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                    CompInSelection.ApplyTraffic()
                    CompInSelection.UpdateStatus()
                    If CompInSelection.GetType.ToString = gTrackComponentName Then CompInSelection.mytmpTrafficDirection = Me.DefaultTraffic
                Next
                If Me.AutomaticModeOn Then
                    If Me.ControlledByTacticalRoute Then
                        TMDS.NetworkMessages.MPARMessages.SendRouteStatusMessage(Me.CurrentTacticalRoute, "UPDATE", "LINED")
                    End If
                End If
                If optEnableGeoFenceOperations Then
                    If Not IsNothing(Me.CurrentTacticalRoute) Then
                        CurrentTacticalRoute.UpdateRouteStatus("CLEARED")
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Add Auto Block
        ''' </summary>
        ''' <param name="pComp"></param>
        Private Sub AddAutoBlock(pComp As Object)
            Dim swAux As Switch
            Try
                If pComp.GetType.ToString <> gSwitchComponentName Then
                    Exit Sub
                End If
                swAux = New Switch
                swAux = CType(pComp, Switch)
                If optEnableAutoBlockSwitchesInSignalRoute AndAlso swAux.AllowBlueFlag AndAlso Me.AllowCallOn AndAlso Not Me.SignalCalledOn AndAlso swAux.DeviceBlocks.Count = 0 Then
                    SignalEventLog("AddAutoBlock:Switch=" & swAux.GUID.ToString & ":" & swAux.Name)
                    swAux.ApplyDeviceBlockFromSwitch(String.Empty, "AUTOBLOCK SWITCHES IN SIGNAL ROUTE")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                swAux = Nothing
            End Try
        End Sub

        ''' <summary>
        ''' Remove Auto Block
        ''' </summary>
        ''' <param name="pComp"></param>
        Private Sub RemoveAutoBlock(pComp As Object)
            Dim swAux As Switch
            Dim deviceToRemove As ArrayList
            Try
                If pComp.GetType.ToString <> gSwitchComponentName Then
                    Exit Sub
                End If
                swAux = New Switch
                swAux = CType(pComp, Switch)
                deviceToRemove = New ArrayList
                If optEnableAutoBlockSwitchesInSignalRoute AndAlso swAux.AllowBlueFlag AndAlso gSelectedSignalUserClickedinStop AndAlso Me.AllowCallOn Then
                    'Check the device blocks
                    For Each deviceBlock As TrackAuthority.DeviceBlock In swAux.DeviceBlocks
                        If deviceBlock.BlockNotes.Contains("AUTOBLOCK SWITCHES IN SIGNAL ROUTE") Then
                            deviceToRemove.Add(deviceBlock)
                        End If
                    Next
                    'Remove the device blocks
                    For Each deviceBlock As TrackAuthority.DeviceBlock In deviceToRemove
                        SignalEventLog("RemoveAutoBlock:Switch=" & swAux.GUID.ToString & ":" & swAux.Name)
                        swAux.RemoveDeviceBlockFromSwitch(deviceBlock)
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                swAux = Nothing
                deviceToRemove = Nothing
            End Try
        End Sub

        Sub SetToStop()
            Try
                If InProgressOfSettingSignalToStop Then
                    Exit Sub
                End If
                InProgressOfSettingSignalToStop = True
                'added this code to lock the route until an indication is received
                If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                    Me.SignalStatus = BulbStatus.OfficeLock
                Else
                    Me.SignalStatus = BulbStatus.StopStatus
                End If
                If Me.BlockAbsolute Then
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.MediumSlateBlue))
                    Me.RemoveFromFlashCollection(Me)
                    Flasher.Enabled = False
                    FlashComponent = False
                Else
                    If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.Orange))
                    Else
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                        Me.RemoveFromFlashCollection(Me)
                        Flasher.Enabled = False
                        FlashComponent = False
                    End If
                End If
                If Me.myControlPoint.TunnelAutomaticControlled And IsMySlottingTrackIndicated() Then 'AndAlso Me.SignalIndStatus <> IndStatus.Stopped Then
                    InProgressOfSettingSignalToStop = False
                    Exit Sub
                End If
                'ARRC-494
                Dim CompInSelection As Object
                Dim FlipFlopFound As Boolean
                'set the traffic for each switch and track
                If SignalCollection.Count = 0 Then
                    Me.GetSignalRoute()
                End If
                Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already
                For Each CompInSelection In SignalRouteCollection
                    'AutoBlock switches functionality - remove
                    If CompInSelection.GetType.ToString = gSwitchComponentName Then
                        RemoveAutoBlock(CompInSelection)
                    End If
                    If CompInSelection.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then
                        FlipFlopFound = True
                    End If
                    If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                    If Not TrafficRouteEstablished Then
                        If optEnableFlipFlopDetectionLogic Then
                            If CompInSelection.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                If (CompInSelection.FlipMeLeft And CompInSelection.FlipMeLeft) Or (CompInSelection.FlipMeRight And CompInSelection.FlipMeRight) Then
                                    FlipFlopFound = True
                                End If
                            End If
                        End If
                        Select Case Me.ComponentType
                            Case "L", "U"
                                If CompInSelection.TrafficDirection = "LEFT" Or CompInSelection.TrafficDirection = "" Then
                                    'added this code to lock the route until an indication is received
                                    If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock
                                    Else
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                        CompInSelection.TrafficDirection = ""
                                        CompInSelection.ApplyTraffic()
                                    End If
                                    CompInSelection.UpdateStatus()
                                ElseIf FlipFlopFound And CompInSelection.TrafficDirection = "RIGHT" Or CompInSelection.TrafficDirection = "" Then
                                    'added this code to lock the route until an indication is received
                                    If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock
                                    Else
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                        CompInSelection.TrafficDirection = ""
                                        CompInSelection.ApplyTraffic()
                                        CompInSelection.FlipMe = False
                                    End If
                                    CompInSelection.UpdateStatus()
                                End If
                            Case "R", "D"
                                If CompInSelection.TrafficDirection = "RIGHT" Or CompInSelection.TrafficDirection = "" Then
                                    'added this code to lock the route until an indication is received
                                    If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock
                                    Else
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                        CompInSelection.TrafficDirection = ""
                                        CompInSelection.ApplyTraffic()
                                    End If
                                    CompInSelection.UpdateStatus()
                                ElseIf FlipFlopFound And CompInSelection.TrafficDirection = "LEFT" Or CompInSelection.TrafficDirection = "" Then
                                    'added this code to lock the route until an indication is received
                                    If Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock
                                    Else
                                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                        CompInSelection.TrafficDirection = ""
                                        CompInSelection.ApplyTraffic()
                                        CompInSelection.FlipMe = False
                                    End If
                                    CompInSelection.UpdateStatus()
                                End If
                        End Select
                    Else
                        'added this code in 332 for item 638
                        If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated Or
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime Then
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            CompInSelection.TrafficDirection = ""
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        End If
                        If optAllowTheRemovalOfTrafficPendingRequestInTheBlock Then
                            If CompInSelection.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(CompInSelection, Track)
                                If Not trk.Occuppied And trk.SignalStatOnMe = Track.SignalStatus.SignalRequested Then
                                    trk.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                    trk.TrafficDirection = ""
                                    trk.ApplyTraffic()
                                    trk.UpdateStatus()
                                    trk.FlipMe = False

                                End If
                            End If
                        End If
                    End If
                Next
                If Not Me.SignalCtlStatus = CtlStatus.RequestingStop Then
                    Dim DoClearCollection As Boolean = True
                    If Not IsNothing(Me.InTimeBit) Then
                        If Me.InTimeBit.BitState Then
                            DoClearCollection = False
                        End If
                    End If
                    If DoClearCollection Then
                        SignalRouteCollection.Clear()
                    End If
                End If
                If Me.EnablePulseStopControl Then
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = False
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If
                    Me.myControlPoint.SendCodeStationControlUpdate()
                ElseIf optEnableMRSSpecificOptions AndAlso (Me.SignalCtlStatus = CtlStatus.RequestedStop AndAlso Me.PreviousSignalStatus = BulbStatus.ClearStatus AndAlso Me.SignalStatus = BulbStatus.StopStatus) Then
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If
                    Me.myControlPoint.SendCodeStationControlUpdate()
                End If
                InProgressOfSettingSignalToStop = False
            Catch ex As Exception
                InProgressOfSettingSignalToStop = False
                SystemException(ex.ToString)
            Finally
                InProgressOfSettingSignalToStop = False
            End Try
        End Sub

        Public Sub ClearSignalRouteForInitialization()
            Try
                Dim CompInSelection As Object
                For Each CompInSelection In SignalRouteCollection
                    If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated Or
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime Then
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                        CompInSelection.TrafficDirection = ""
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Function GetSignalRoute(Optional ByVal DrillMode As Boolean = False, Optional ByVal Initialization As Boolean = False) As Boolean
            Dim EndOfRouteFound As Boolean
            Dim CompToCheck As Object
            Dim SearchRight As Boolean
            Dim FlipFlopFound As Boolean
            Dim NextReferenceGUID, LastReferenceGUID As Int32
            Dim LoopControl As Int32
            Try
                If Me.GUID = 158213 Then
                    '  Beep()
                End If

                'set the direction of the search
                If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                    SearchRight = True
                End If

                'init the first link to valid
                If SearchRight Then
                    NextReferenceGUID = Me.RightLink
                Else
                    NextReferenceGUID = Me.LeftLink
                End If

                'begin the validation search
                While Not EndOfRouteFound
                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextReferenceGUID)
                    If IsNothing(CompToCheck) Then Return False

                    Select Case CompToCheck.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(CompToCheck, Switch)

                            If FlipFlopFound Then CompToCheck.FlipMe = True
                            'check for opposing traffic if right search
                            If SearchRight And CompToCheck.TrafficDirection = "LEFT" Then Return False

                            'check for opposing traffic if left search
                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") Then Return False

                            'check for occupied switch
                            If CompToCheck.Occuppied And Not Initialization Then Return False

                            'check to see if switch is CTC operations
                            If CompToCheck.AuthorityType <> "CTC" Then Return False

                            'check request position of switch
                            If sw.TypeOfSwitch <> "DERAIL" Then
                                If SearchRight And CompToCheck.RightAltLink <> 0 Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.RightLink
                                    Else
                                        NextReferenceGUID = CompToCheck.RightAltLink
                                    End If
                                ElseIf Not SearchRight And CompToCheck.LeftAltLink <> 0 Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftAltLink
                                    End If

                                ElseIf LastReferenceGUID = CompToCheck.LeftLink And SearchRight Then
                                    If CompToCheck.ControlledReverse Then
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.LeftAltLink And SearchRight Then
                                    If Not CompToCheck.ControlledReverse Then
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.RightLink And Not SearchRight Then
                                    If CompToCheck.ControlledReverse Then
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If

                                ElseIf LastReferenceGUID = CompToCheck.RightAltLink And Not SearchRight Then
                                    If Not CompToCheck.ControlledReverse Then
                                        Return False
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                End If
                            ElseIf sw.TypeOfSwitch = "DERAIL" Then
                                If SearchRight Then
                                    NextReferenceGUID = CompToCheck.RightLink
                                Else
                                    NextReferenceGUID = CompToCheck.LeftLink
                                End If

                            End If

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) Then
                                SignalRouteCollection.Add(CompToCheck)
                            End If

                        Case "TMDS.SignalComponents.Signal"

                            If Not FlipFlopFound Then
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic = Me.DefaultTraffic Then
                                    EndOfRouteFound = True
                                ElseIf CompToCheck.DefaultTraffic <> Me.DefaultTraffic Then
                                    'if opposing signals found not at stop then exit sub
                                    If Not CompToCheck.FailState Then
                                        If Not DrillMode Then
                                            If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return False
                                            If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return False
                                            If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return False
                                        End If
                                    End If
                                End If
                            Else
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic <> Me.DefaultTraffic Then
                                    EndOfRouteFound = True
                                ElseIf CompToCheck.DefaultTraffic = Me.DefaultTraffic Then
                                    'if opposing signals found not at stop then exit sub
                                    If Not CompToCheck.FailState Then
                                        If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return False
                                        If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return False
                                        If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return False
                                    End If
                                End If
                            End If
                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If


                            'Track Validation
                        Case "TMDS.TrackComponents.Track"
                            Dim thistrk As Track = CType(CompToCheck, Track)
                            If FlipFlopFound Then CompToCheck.FlipMe = True

                            'check for opposing traffic if right search
                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") Then
                                If DrillMode And Not thistrk.IgnoreTrafficOnDrillRequest Then
                                    Return False
                                Else
                                    Return False
                                End If
                            End If

                            'check for opposing traffic if left search
                            If (SearchRight And CompToCheck.TrafficDirection = "LEFT") Then
                                If DrillMode And Not thistrk.IgnoreTrafficOnDrillRequest Then
                                    Return False
                                Else
                                    Return False
                                End If
                            End If


                            'check for occupied OS track types
                            If (CompToCheck.OStype And CompToCheck.Occuppied) And Not Initialization Then Return False

                            'check for Track and Time
                            If CompToCheck.TrackAndTime Then Return False

                            If CompToCheck.BlockAbsolute Then Return False


                            Select Case thistrk.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(CompToCheck) Then
                                SignalRouteCollection.Add(CompToCheck)
                            End If

                            'setup the next component to validate
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If
                        Case "TMDS.TrackOtherComponents.RRXING"
                            Dim rr As RRXING = CType(CompToCheck, RRXING)
                            Select Case rr.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And rr.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And rr.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select

                            'If Not CompToCheck.ValidateForClearRoute Then Return False
                            If Not rr.ValidateForClearRoute(rr.ComponentType) Then Return False


                            'if component passes the validation test, then add it to the collection
                            'SignalRouteCollection.Add(CompToCheck)

                            'if component passes the validation test, then add it to the collection
                            If Not SignalRouteCollection.Contains(rr) Then
                                SignalRouteCollection.Add(rr)
                            End If

                            'setup the next component to validate

                            'If SearchRight Then
                            NextReferenceGUID = rr.FindExitGUID(LastReferenceGUID)
                            'Else
                            '    NextReferenceGUID = CompToCheck.LeftLink
                            'End If
                            LastReferenceGUID = rr.GUID

                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            Dim ff As FlipFlop = CType(CompToCheck, FlipFlop)
                            FlipFlopFound = True
                            Select Case ff.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And ff.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And ff.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select

                            If Not SearchRight Then
                                SearchRight = True
                            Else
                                SearchRight = False
                            End If
                            NextReferenceGUID = ff.FindExitGUID(LastReferenceGUID)
                            LastReferenceGUID = ff.GUID
                    End Select

                    LoopControl += 1
                    If LoopControl > 100 Then
                        Exit While
                    End If
                End While


                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Sub SetToInTimeRoute()
            Dim FlipTraffic As Boolean
            Try
                Me.SignalStatus = BulbStatus.InTime
                'Me.CurrentColorStatus = (New SolidBrush(Color.DarkRed))
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkSalmon))
                Me.AddToFlashCollection(Me)
                Flasher.Enabled = True
                FlashComponent = True
                Dim RouteList As New ArrayList
                Dim CompInSelection As Object
                Me.GetSignalRoute()

                'set the traffic for each switch and track
                Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already
                For Each CompInSelection In SignalRouteCollection

                    'If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> Nothing Then TrafficRouteEstablished = True
                    If CompInSelection.GetType.ToString = gTrackComponentName Then
                        If CompInSelection.Occuppied And Not CompInSelection.OSType And CompInSelection.TrafficDirection <> Nothing Then TrafficRouteEstablished = True
                    ElseIf CompInSelection.GetType.ToString = gFlipFlopComponentName Then
                        '    FlipTraffic = True

                        If FlipTraffic Then
                            FlipTraffic = False
                        Else
                            FlipTraffic = True
                        End If


                    End If
                    If Not TrafficRouteEstablished Then
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime
                        If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                            If Not FlipTraffic Then
                                CompInSelection.TrafficDirection = "RIGHT"
                            Else
                                CompInSelection.TrafficDirection = "LEFT"
                            End If
                        Else
                            If FlipTraffic Then
                                CompInSelection.TrafficDirection = "RIGHT"
                            Else
                                CompInSelection.TrafficDirection = "LEFT"
                            End If
                        End If

                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If

                Next
                Me.ReSetFlasherCollection()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToInTimeRouteDrill()
            Try
                Me.SignalStatus = BulbStatus.InTime
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkSalmon))
                Me.AddToFlashCollection(Me)
                Flasher.Enabled = True
                FlashComponent = True
                Dim CompInSelection As Object
                Me.GetDrillSignalRoutes()
                For Each CompInSelection In DrillRouteComponents
                    Dim gu As Int32 = CompInSelection.guid
                    CompInSelection.TrafficDirection = "BOTH"
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime
                    CompInSelection.ApplyTraffic()
                    CompInSelection.UpdateStatus()

                Next
                For Each CompInSelection In DrillSharedComponents
                    Dim gu As Int32 = CompInSelection.guid
                    CompInSelection.TrafficDirection = "BOTH"
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime
                    CompInSelection.ApplyTraffic()
                    CompInSelection.UpdateStatus()

                Next


                Me.ReSetFlasherCollection()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToInTime()
            Try
                Me.SignalStatus = BulbStatus.InTimeNoControlledSignal
                'Me.CurrentColorStatus = (New SolidBrush(Color.DarkRed))
                Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkSalmon))
                Me.AddToFlashCollection(Me)
                Flasher.Enabled = True
                FlashComponent = True
                Dim CompInSelection As Object
                'Me.GetClearSignalRoute()
                'set the traffic for each switch and track
                Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already
                For Each CompInSelection In SignalRouteCollection
                    'If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> Nothing Then TrafficRouteEstablished = True
                    If CompInSelection.Occuppied And Not CompInSelection.OSType And CompInSelection.TrafficDirection <> Nothing Then TrafficRouteEstablished = True
                    If Not TrafficRouteEstablished Then
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime
                        If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                            CompInSelection.TrafficDirection = "RIGHT"
                        Else
                            CompInSelection.TrafficDirection = "LEFT"
                        End If
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next

                Me.ReSetFlasherCollection()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function ControlSignalToClear(ByVal SendControl As Boolean,
                                                Optional ByVal CheckForTags As Boolean = True,
                                                Optional ByVal ShowRoute As Boolean = False,
                                                Optional ByVal CallOn As Boolean = False,
                                                Optional ByVal StoredRouteExecution As Boolean = False,
                                                Optional ByVal CancelStopCommand As Boolean = False,
                                                Optional ByVal SignalAlreadyControlledClear As Boolean = False,
                                                Optional ByVal SelectControlPoint As Boolean = False,
                                                Optional ByVal FleetCommand As Boolean = False, Optional ByVal OutOfCorespondenceReset As Boolean = False,
                                                Optional ByVal CheckForClearingIntoFailedTrack As Boolean = False,
                                                Optional ByVal IgnoreValidTWCAuthorityOnSignalClear As Boolean = False,
                                                Optional ByVal pTrain As TrainID = Nothing) As Boolean
            Try
                If AutomaticSignal OrElse
                    IndicateOnly Then Exit Function
                If GetClearSignalRoute(CheckForTags, ShowRoute, CallOn, SignalAlreadyControlledClear,
                                       FleetCommand, CheckForClearingIntoFailedTrack, IgnoreValidTWCAuthorityOnSignalClear,
                                       False, pTrain) OrElse
                                       DoMonitorMode Then
                    If JointControlAuthorizationEnabled Then
                        Dim allTracksLoadedInSameDesk As Boolean = False
                        Dim routeHasRemoteTrack As Boolean = DoesSignalRouteHaveRemoteTrack(SignalRouteCollection, allTracksLoadedInSameDesk)
                        If (Not allTracksLoadedInSameDesk AndAlso
                            routeHasRemoteTrack) OrElse
                            DoesSignalRouteHaveJoinTrack(SignalRouteCollection) Then
                            If JointControlAuthorizationOfficeMode Then
                                If Not JointControlAuthorizationOfficeModeConfirmed OrElse
                                    Not JointControlAuthorizationOfficeModeRequested Then
                                    AddMessageToFeedback($"SIGNAL DOES NOT HAVE JOINT AUTHORIZATION:{Name} ({GUID})")
                                    Exit Function
                                End If
                            Else
                                If Not JointControlAuthorizationIndBit.BitState Then
                                    AddMessageToFeedback($"SIGNAL DOES NOT HAVE JOINT AUTHORIZATION:{Name} ({GUID})")
                                    Exit Function
                                End If
                            End If
                        End If
                    End If
                    If myControlPoint.TunnelAutomaticMode Then
                        If myControlPoint.TunnelAutomaticControlled Then
                            DoTunnelAutomaticMode(True, True)
                            Exit Function
                        End If
                    End If
                    If SelectControlPoint Then
                        Me.myControlPoint.FlasherMode(True)
                    End If
                    Me.NoSignalControlInitialization = False
                    Me.ControlNotYetCommitted = True
                    Me.SignalCtlStatus = CtlStatus.RequestedClear
                    If Me.SignalIndStatus = IndStatus.Stopped Or Me.SignalIndStatus = IndStatus.InTime Then
                        Me.SignalStatus = BulbStatus.RequestingClear
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))
                        Me.AddToFlashCollection(Me)
                        FlashComponent = True
                        Dim CompInSelection As Object
                        Dim FlipFlopFound As Boolean

                        For Each CompInSelection In SignalRouteCollection
                            If CompInSelection.FlipMe Or FlipFlopFound Then
                                If Me.DefaultTraffic = "RIGHT" Then
                                    CompInSelection.TrafficDirection = "LEFT"
                                Else
                                    CompInSelection.TrafficDirection = "RIGHT"
                                End If
                                CompInSelection.FlipMe = False
                            Else
                                CompInSelection.TrafficDirection = Me.DefaultTraffic
                            End If
                            If CompInSelection.SignalStatOnMe <> Track.SignalStatus.SignalCleared Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequested
                                CompInSelection.CurrentColorStatus = New SolidBrush(Color.Yellow)
                            End If
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()

                            If optEnableFlipFlopDetectionLogic Then
                                If CompInSelection.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    If (CompInSelection.FlipMeLeft And CompInSelection.FlipMeLeft) Or (CompInSelection.FlipMeRight And CompInSelection.FlipMeRight) Then
                                        FlipFlopFound = True
                                    End If
                                End If
                            End If
                        Next
                    ElseIf Me.SignalIndStatus = IndStatus.Cleared Then
                        Me.SignalStatus = BulbStatus.ClearStatus
                        Me.RemoveFromFlashCollection(Me)
                        FlashComponent = False
                        Flasher.Enabled = False
                        Dim CompInSelection As Object
                        'set the traffic for each switch and track
                        For Each CompInSelection In SignalRouteCollection
                            If CompInSelection.FlipMe Then
                                If Me.DefaultTraffic = "RIGHT" Then
                                    CompInSelection.TrafficDirection = "LEFT"
                                Else
                                    CompInSelection.TrafficDirection = "RIGHT"
                                End If
                                CompInSelection.FlipMe = False
                            Else
                                CompInSelection.TrafficDirection = Me.DefaultTraffic
                            End If

                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                            CompInSelection.CurrentColorStatus = New SolidBrush(Color.ForestGreen)
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next
                    End If
                    Me.UpdateStatus()
                    'This is to keep track of the last control control in the in-time signal collection
                    Dim sig As Signal
                    For Each sig In Me.BrotherSignalIntimeCollection
                        If sig.SignalIndStatus <> IndStatus.InTime Then
                            sig.LastControlSignal = False
                        End If
                    Next

                    Me.LastControlSignal = True
                    Me.PreviousDrilledControlled = False

                    If CallOn Then
                        Me.SignalCalledOn = True
                    End If


                    'reset the control bits to the proper sequence if the stop control was selected but then the ESC key was pressed
                    If CancelStopCommand And Not OutOfCorespondenceReset Then
                        'NEAR SIDE SIGNAL FUNCTIONALITY ADDED FOR NCTD
                        Dim LocalClearMode As Boolean
                        If Me.NearSideSignalEnabled And Me.NearSideSignalOn Then
                            If Not IsNothing(Me.NearSideSignalCtlBit) Then
                                LocalClearMode = True
                            End If
                        End If
                        'NEAR SIDE SIGNAL FUNCTIONALITY ADDED FOR NCTD

                        If Not LocalClearMode Then

                            If Me.EnableDynamicSignalRouteControlBit Then
                                If SignalCalledOn Then
                                    Me.CallOnControl = Me.GetSignalCallOnBit
                                    If Not Me.CallOnControl Is Nothing Then
                                        Me.CallOnControl.BitState = True
                                    End If
                                Else
                                    Me.Ctl(0) = Me.GetSignalClearRouteBit
                                    If Not Me.Ctl(0) Is Nothing Then
                                        Me.Ctl(0).BitState = True
                                    End If
                                End If
                            Else
                                If Not Me.Ctl(0) Is Nothing Then
                                    Me.Ctl(0).BitState = True
                                End If
                                If Not Me.Ctl(3) Is Nothing Then
                                    Me.Ctl(3).BitState = True
                                End If

                                If Not Me.Ctl(1) Is Nothing Then
                                    Me.Ctl(1).BitState = True
                                End If
                                If Not Me.Ctl(4) Is Nothing Then
                                    Me.Ctl(4).BitState = True
                                End If
                            End If
                        Else
                            Me.NearSideSignalCtlBit.BitState = True
                            Me.NearSideSignalControlUpdateStatus()
                        End If

                        If Not Me.Ctl(2) Is Nothing Then
                            Me.Ctl(2).BitState = False
                        End If
                        If Not Me.Ctl(5) Is Nothing Then
                            Me.Ctl(5).BitState = False
                        End If
                    ElseIf OutOfCorespondenceReset Then
                        Me.tmrCheckRoute.Interval = 250
                        Me.tmrCheckRoute.Enabled = True
                    End If

                    'enable the non-composite route check
                    Dim SwitchOutOfCorespondence As Boolean
                    If SendControl AndAlso Not (gAssistMode OrElse Me.IsShadowMode) Then
                        If Not Me.SignalRouteCollection Is Nothing Then
                            Dim comp As Object
                            For Each comp In Me.SignalRouteCollection
                                If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                    If comp.ControlledReverse And Not comp.SwitchIndStatus = Switch.SwIndStatus.ReverseInd Then
                                        If Not StoredRouteExecution Then
                                            Me.AddToSendCollection("ControlSignalToClear", False)

                                        End If
                                        SwitchOutOfCorespondence = True
                                        Exit For
                                    ElseIf Not comp.ControlledReverse And Not comp.SwitchIndStatus = Switch.SwIndStatus.NormalInd Then
                                        If Not StoredRouteExecution Then
                                            Me.AddToSendCollection("ControlSignalToClear1", False)
                                        End If
                                        SwitchOutOfCorespondence = True
                                        Exit For
                                    End If
                                End If
                            Next
                        Else
                            Return False
                        End If
                        If SwitchOutOfCorespondence And StoredRouteExecution Then
                            Me.FindCodeStationToSend()
                            Me.myControlPoint.SendCodeStationControls()
                        End If
                        Me.tmrCheckRoute.Interval = 250
                        Me.tmrCheckRoute.Enabled = True
                    End If

                    If Me.IndicateOnlyWithControlSetEnabled Then
                        Me.SignalControlled = True
                        SignalControlUpdateDatabase()
                    End If

                    If Me.myTerritory.AllowMovementPlanning Then
                        If Me.myControlPoint.StoredRoutes.Count > 0 Then
                            Dim TrnName As String = Me.FindTrainAssociation
                            If TrnName <> "" Then
                                Dim sr As StoredRoute
                                For Each sr In Me.myControlPoint.StoredRoutes
                                    If sr.AutoRouteGenerated And Not sr.DisplayStoredRoute Then
                                        If sr.myRouteDependentSymbolName = TrnName Then
                                            sr.DisplayStoredRoute = True
                                            sr.UpdateDisplayRoute()
                                            If sr.Priority = 0 Then
                                                sr.UpdateSignalDisplays()
                                            End If
                                        End If
                                    End If
                                Next
                            Else
                                Dim sr As StoredRoute
                                For Each sr In Me.myControlPoint.StoredRoutes
                                    If sr.AutoRouteGenerated And Not sr.DisplayStoredRoute And sr.Priority = 0 Then
                                        sr.DisplayStoredRoute = True
                                        sr.UpdateDisplayRoute()
                                        sr.UpdateSignalDisplays()
                                    End If
                                Next
                            End If
                        End If
                    End If

                    Return True
                Else
                    If Me.SignalIndStatus = IndStatus.Stopped Then
                        SignalRouteCollection.Clear()
                    End If
                    Me.SignalCtlStatus = CtlStatus.RequestedStop
                    Return False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Function FindTrainAssociation() As String
            Try
                Dim NextComp As Object = Nothing
                Dim LoopControl As Integer
                If Me.ComponentType = "L" Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.RightLink)
                    Do
                        If IsNothing(NextComp) Then Exit Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim tk As Track = CType(NextComp, Track)
                            If tk.TrainIDs.Count > 0 Then
                                Dim trn As TrainID = tk.TrainIDs.Item(0)
                                If Not IsNothing(trn) Then
                                    Return trn.Name
                                End If
                            End If
                            If tk.NonTransverable Then Exit Do
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                        ElseIf NextComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            Dim sw As Switch = CType(NextComp, Switch)
                            If sw.RightAltLink <> 0 Then
                                If sw.ControlledReverse Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(sw.RightAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            End If
                        ElseIf NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Dim sig As Signal = CType(NextComp, Signal)
                            If sig.ComponentType = "L" Then
                                If sig.SignalIndStatus = IndStatus.Cleared Or sig.SignalCtlStatus = CtlStatus.RequestedClear Or sig.SignalCtlStatus = CtlStatus.RequestingClear Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                Else
                                    Exit Do
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            End If
                        End If

                        LoopControl += 1
                        If LoopControl > 100 Then
                            Exit Do
                        End If
                    Loop

                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.LeftLink)
                    Do
                        If IsNothing(NextComp) Then Exit Do
                        If NextComp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim tk As Track = CType(NextComp, Track)
                            If tk.TrainIDs.Count > 0 Then
                                Dim trn As TrainID = tk.TrainIDs.Item(0)
                                If Not IsNothing(trn) Then
                                    Return trn.Name
                                End If
                            End If
                            If tk.NonTransverable Then Exit Do
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                        ElseIf NextComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            Dim sw As Switch = CType(NextComp, Switch)
                            If sw.LeftAltLink <> 0 Then
                                If sw.ControlledReverse Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(sw.LeftAltLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If
                        ElseIf NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Dim sig As Signal = CType(NextComp, Signal)
                            If sig.ComponentType = "R" Then
                                If sig.SignalIndStatus = IndStatus.Cleared Or sig.SignalCtlStatus = CtlStatus.RequestedClear Or sig.SignalCtlStatus = CtlStatus.RequestingClear Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                Else
                                    Exit Do
                                End If
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If
                        End If

                        LoopControl += 1
                        If LoopControl > 100 Then
                            Exit Do
                        End If
                    Loop
                End If
                Return String.Empty
            Catch ex As Exception
                SystemException(ex.ToString)
                Return String.Empty
            End Try
        End Function

        Sub ControlSignalToStop(Optional ByVal ShowSignalInTime As Boolean = True, Optional ByVal SetStopBit As Boolean = True)
            Try
                If Me.AutomaticSignal Then Exit Sub
                'If Me.IndicateOnly Then Exit Sub
                Me.SignalCtlStatus = CtlStatus.RequestingStop
                If Me.SignalIndStatus = IndStatus.Stopped Then
                    If ShowSignalInTime Then
                        Me.SignalStatus = BulbStatus.InTime
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkSalmon))
                        Me.myControlPoint.SendRecall()
                    Else
                        Me.SignalCtlStatus = CtlStatus.RequestedStop
                        Me.SignalStatus = BulbStatus.StopStatus
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                        Me.RemoveFromFlashCollection(Me)
                        Flasher.Enabled = False
                        FlashComponent = False

                    End If

                    'Me.SignalStatus = BulbStatus.StopStatus
                    'Me.SignalStatus = BulbStatus.InTime
                    'Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                    'Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkSalmon))


                    Dim CompInSelection As Object
                    Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already

                    'set the traffic for each switch and track
                    For Each CompInSelection In SignalRouteCollection
                        If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                        If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True

                        If Not TrafficRouteEstablished Then
                            If ShowSignalInTime AndAlso Me.SignalIndStatus = IndStatus.Stopped Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalInTime
                            Else
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                            End If
                            CompInSelection.UpdateStatus()
                        Else
                            'if track is occuppied then set the signal status to stop on the track to allow a following track and time (1.0.1.334)
                            If CompInSelection.Occuppied Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            End If
                        End If
                    Next
                ElseIf Me.SignalIndStatus = IndStatus.Cleared Then
                    Me.SignalStatus = BulbStatus.RequestingStop
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                    Me.AddToFlashCollection(Me)
                    FlashComponent = True
                    Dim CompInSelection As Object
                    Dim TrafficRouteEstablished As Boolean ' this is a flag that states the signal route is in use by a train already
                    'set the traffic for each switch and track
                    For Each CompInSelection In SignalRouteCollection

                        If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> Nothing Then TrafficRouteEstablished = True
                        If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                        If Not TrafficRouteEstablished Then
                            CompInSelection.TrafficDirection = ""
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        End If
                    Next
                ElseIf Me.SignalIndStatus = IndStatus.InTime Then
                    Me.SetToInTimeRoute()
                    Me.myControlPoint.SendRecall()
                End If
                If Me.NearSideSignalEnabled Then
                    If Not IsNothing(Me.NearSideSignalCtlBit) Then
                        Me.NearSideSignalCtlBit.BitState = False
                        Me.NearSideSignalControlUpdateStatus()
                    End If
                End If
                If Not Me.Ctl(0) Is Nothing Then
                    Me.Ctl(0).BitState = False
                End If
                If Not Me.Ctl(3) Is Nothing Then
                    Me.Ctl(3).BitState = False
                End If

                If Not Me.Ctl(1) Is Nothing Then
                    Me.Ctl(1).BitState = False
                End If
                If Not Me.Ctl(4) Is Nothing Then
                    Me.Ctl(4).BitState = False
                End If

                If Me.EnableDynamicSignalRouteControlBit Then
                    Dim bp As BitPosition
                    For Each bp In Me.DynamicSignalRouteControlBitList
                        bp.BitState = False
                    Next
                    For Each bp In Me.DynamicSignalRouteCallBitList
                        bp.BitState = False
                    Next
                End If


                If SetStopBit Then
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                End If
                If Me.JointControlAuthorizationEnabled Then
                    Me.JointControlAuthorizationOff(True)
                End If


                If Not IsNothing(Me.CallOnControl) Then
                    Me.CallOnControl.BitState = False
                    SignalCalledOn = False
                End If

                If Me.LastCtlStatus <> Me.SignalCtlStatus AndAlso Not (gAssistMode OrElse Me.IsShadowMode) Then
                    If Me.FailState And Not Me.DoMonitorMode Then
                        'Me.SendSignalControlUpdate()
                    ElseIf Not Me.DoMonitorMode And Not Me.FailState Then
                        Me.AddToSendCollection("ControlSignalToStop")
                    ElseIf Me.DoMonitorMode AndAlso Me.myControlPoint.TunnelAutomaticControlled Then
                        Me.SendSignalControlUpdate()
                    End If
                End If

                If Me.SimulateMode Then
                    Me.DoSimulate = True
                End If
                If Me.tmrCheckRoute.Enabled Then PreviousNonCompositeDetected = True

                Me.tmrCheckRoute.Enabled = False
                Me.tmrCheckRoute.Stop()

                If Initialized And Not Me.FailState Then
                    If Me.FleetEnabled AndAlso Not (gAssistMode OrElse Me.IsShadowMode) Then
                        Me.DoSetSignalFleeting(False, False, False)
                    End If
                    Me.UpdateStatus()
                    Me.InvalidateSignal()
                End If

                If Me.IndicateOnlyWithControlSetEnabled Then
                    Me.SignalControlled = False
                    SignalControlUpdateDatabase()
                End If

                If Me.AutomaticModeOn Then
                    If Me.ControlledByTacticalRoute Then
                        TMDS.NetworkMessages.MPARMessages.SendRouteStatusMessage(Me.CurrentTacticalRoute, "UPDATE", "OVERRIDE")
                        Me.CurrentTacticalRoute.UpdateRouteStatus("DISP-CANCELLED")
                        Me.CurrentTacticalRoute.RemoveRoute()
                        Me.CurrentTacticalRoute.myTrain.ToggleAutoRouting(False, "DISP-CANCELLED")
                        Me.CurrentTacticalRoute = Nothing
                        Me.ControlledByTacticalRoute = False
                    End If
                End If

                Me.SignalEventLog("SIGNAL CONTROLLED TO STOP:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ShowPendingRoute(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Dim FlashColor As SolidBrush
            Dim comp As Object
            Try
                If PendingRouteShow Then
                    If IsNothing(Me.SignalRoutePendingCollection) Then
                        SignalRoutePendingCollection = New ArrayList()
                        SignalRoutePendingCollection = Me.SignalRouteCollection.Clone
                    End If
                    If SelectionPendingRoute Then
                        SelectionPendingRoute = False
                        FlashColor = gBackGroundColor
                    Else
                        SelectionPendingRoute = True
                        FlashColor = gSelectionFlashColor
                    End If

                    For Each comp In Me.SignalRoutePendingCollection

                        Select Case comp.GetType.ToString

                            Case "TMDS.SwitchComponents.Switch", "TMDS.TrackComponents.Track"
                                comp.FlashComponent = True
                                comp.SelectionMode = SwitchComponents.Switch.ModeSelection.SummarySelectionRouting
                                comp.FlashOn = SelectionPendingRoute
                                If comp.InvokeRequired Then
                                    comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                                Else
                                    comp.Refresh()
                                End If
                        End Select
                    Next

                    Me.FlashComponent = True
                    Me.SelectionMode = SwitchComponents.Switch.ModeSelection.SummarySelectionRouting
                    Me.FlashOn = SelectionPendingRoute
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Else
                        Me.Refresh()
                    End If

                    Me.tmrShowPendingRoute.Enabled = True

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
                Try
                    Me.tmrShowPendingRoute.Enabled = False
                    ClearPendingRoute()
                Catch
                End Try
            End Try
        End Sub

        Private Sub ClearPendingRoute()
            Dim comp As Object
            Try
                PendingRouteShow = False
                Do While Me.tmrShowPendingRoute.Enabled
                    Me.tmrShowPendingRoute.Enabled = False
                    Me.tmrShowPendingRoute.Stop()
                Loop
                If Me.SignalRoutePendingCollection Is Nothing Then Exit Sub
                For Each comp In SignalRoutePendingCollection
                    'SyncLock comp
                    Select Case comp.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch", "TMDS.TrackComponents.Track", "TMDS.SignalComponents.Signal"
                            comp.SelectionMode = TrackComponents.Track.ModeSelection.Regular
                            comp.FlashOn = False
                            comp.FlashComponent = False
                            comp.UpdateStatus()
                            If comp.InvokeRequired Then
                                comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                            Else
                                comp.Refresh()
                            End If
                        Case "TMDS.TrackOtherComponents.RRXING"
                        Case Else
                            Try
                                comp.Graphical.FillPath(comp.CurrentColorStatus, comp.GraphicalPath)
                            Catch ex As Exception
                                SystemException(ex.ToString)
                            End Try

                            comp.UpdateStatus()
                            'Me.Invalidate(comp.RegionPath)
                            If comp.InvokeRequired Then
                                comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                            Else
                                comp.Refresh()
                            End If
                    End Select
                    'End SyncLock
                Next
                SignalRoutePendingCollection.Clear()
                SignalRoutePendingCollection = Nothing
                Me.SelectionMode = GenericComponent.ModeSelection.Regular
                Me.FlashComponent = False
                Me.FlashOn = False
                Me.UpdateStatus()
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If

                'Me.SignalEvent("SIGNAL ClearPendingRoute:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SendSignalStopAutomatically(Optional ByVal ForceSignalStopEvent As Boolean = False, Optional ByVal UnSolicatedSignalEvent As Boolean = False)
            Try
                'set control bits accordingly
                If Not Me.Ctl(0) Is Nothing Then
                    Me.Ctl(0).BitState = False
                End If
                If Not Me.Ctl(3) Is Nothing Then
                    Me.Ctl(3).BitState = False
                End If

                If Not Me.Ctl(1) Is Nothing Then
                    Me.Ctl(1).BitState = False
                End If
                If Not Me.Ctl(4) Is Nothing Then
                    Me.Ctl(4).BitState = False
                End If

                If Not Me.EnablePulseStopControl Then
                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                End If

                If Me.JointControlAuthorizationEnabled Then
                    Me.JointControlAuthorizationOff(True)
                End If

                If Me.EnableDynamicSignalRouteControlBit Then
                    Dim bp As BitPosition
                    For Each bp In Me.DynamicSignalRouteControlBitList
                        bp.BitState = False
                    Next
                    For Each bp In Me.DynamicSignalRouteCallBitList
                        bp.BitState = False
                    Next
                End If


                If Not IsNothing(Me.CallOnControl) Then
                    If Not optEnableCallOnLatching Then
                        Me.CallOnControl.BitState = False
                        SignalCalledOn = False
                    End If
                End If

                'put station in queue to send a control to stop signal
                If Not Me.FailState Then
                    If myControlPoint.DisableAutomaticSignalSlotting Then
                        Me.SendSignalControl(myControlPoint.DisableAutomaticSignalSlotting)
                        Me.SignalEventLog("SIGNAL CONTROLLED TO STOP ONLY UPDATED TO CODE SERVER ACCOUNT AUTOMATIC SIGNAL STOP IS DISABLED:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                    Else
                        If ForceSignalStopEvent Then
                            Me.SendSignalControl(False)
                        Else
                            Me.SendSignalControl(True)
                        End If

                        If UnSolicatedSignalEvent Then
                            Me.SignalEventLog("UNSOLICITED SIGNAL CONTROLLED TO STOP:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        ElseIf ForceSignalStopEvent Then
                            Me.SignalEventLog("SIGNAL CONTROLLED TO STOP SENT:" & Me.GetType.ToString & "|" & Me.GUID & "|" & Me.myControlPoint.StationNumber)
                        End If
                    End If

                End If
                Me.LastCtlStatus = Me.SignalCtlStatus

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SignalControlUpdateDatabase()
            Dim dbconn As OleDbConnection = Nothing
            Dim dbcomm As OleDbCommand = Nothing

            Try
                Dim Update As String = "UPDATE tblCompSignals SET SignalControlled = " & CInt(Me.SignalControlled) & " WHERE UID = " & Me.GUID
                dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                dbconn.Open()
                dbcomm = New OleDbCommand(Update, dbconn)
                dbcomm.ExecuteNonQuery()
                dbconn.Close()
                dbconn = Nothing
            Catch ex As Exception
                dbconn = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SignalCallOnUpdateDatabase()
            Dim dbconn As OleDbConnection = Nothing
            Dim dbcomm As OleDbCommand = Nothing

            Try
                Dim Update As String = "UPDATE tblCompSignals SET CallOnControlled = " & CInt(Me.SignalCalledOn) & " WHERE UID = " & Me.GUID
                dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                dbconn.Open()
                dbcomm = New OleDbCommand(Update, dbconn)
                dbcomm.ExecuteNonQuery()
                dbconn.Close()
                dbconn = Nothing
            Catch ex As Exception
                dbconn = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetSignalControlBitsToStop()

            Try
                'set control bits accordingly
                If Not Me.Ctl(0) Is Nothing Then
                    Me.Ctl(0).BitState = False
                End If
                If Not Me.Ctl(3) Is Nothing Then
                    Me.Ctl(3).BitState = False
                End If

                If Not Me.Ctl(1) Is Nothing Then
                    Me.Ctl(1).BitState = False
                End If
                If Not Me.Ctl(4) Is Nothing Then
                    Me.Ctl(4).BitState = False
                End If

                If Not Me.Ctl(2) Is Nothing Then
                    Me.Ctl(2).BitState = True
                End If
                If Not Me.Ctl(5) Is Nothing Then
                    Me.Ctl(5).BitState = True
                End If
                If Me.JointControlAuthorizationEnabled Then
                    Me.JointControlAuthorizationOff(True)
                End If

                If Me.EnableDynamicSignalRouteControlBit Then
                    Dim bp As BitPosition
                    For Each bp In Me.DynamicSignalRouteControlBitList
                        bp.BitState = False
                    Next
                    For Each bp In Me.DynamicSignalRouteCallBitList
                        bp.BitState = False
                    Next
                End If

                If Not IsNothing(Me.CallOnControl) Then
                    If Not optEnableCallOnLatching Then
                        Me.CallOnControl.BitState = False
                        SignalCalledOn = False
                    End If
                End If
                Me.myControlPoint.SendCodeStationControlUpdate()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Function CheckForConflictingStoredRoutes() As Boolean
            Dim AdjacentControlPoint As ControlPoint = Nothing
            Dim Comp As Object = Nothing
            Dim MessageList As New ArrayList
            Dim LaunchPoint As Point
            Dim LastTrack As Object = Nothing
            Dim TempComp As Object = Nothing
            Dim sr As StoredRoute
            Try
                For Each Comp In Me.SignalRouteCollection
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If Not Comp.myControlPoint Is Nothing Then
                            If Not Me.myControlPoint Is Comp.myControlPoint Then
                                AdjacentControlPoint = Comp.myControlPoint
                                Exit For
                            End If
                        End If
                    End If
                Next
                If AdjacentControlPoint Is Nothing Then
                    LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(Me.SignalRouteCollection)
                    If Me.ComponentType = "R" Or Me.ComponentType = "D" Then
                        TempComp = FindCompFromGUID(LastTrack.RightLink)
                    Else
                        TempComp = FindCompFromGUID(LastTrack.LeftLink)
                    End If
                    If TempComp.myControlPoint IsNot Nothing Then
                        If Not Me.myControlPoint Is TempComp.myControlPoint Then
                            AdjacentControlPoint = TempComp.myControlPoint
                        End If
                    End If
                End If
                If Not AdjacentControlPoint Is Nothing Then
                    LaunchPoint = New Point(AdjacentControlPoint.Left + 20, AdjacentControlPoint.Top + 50)
                    If AdjacentControlPoint.CommFail AndAlso optDisableFleetSignalOnCommFail Then
                        MessageList.Add(AdjacentControlPoint.Name & " in Code Fail. This operation is not allowed. Exiting!!!")
                        Me.SignalEventLog("DISPATCHER(" & gCurrentUser.Initials & ") - " & AdjacentControlPoint.Name & "  IN CODE FAIL. THIS OPERATION IS NOT ALLOWED. EXITING!!! - SIGNAL DETAILS: " & Me.Name & "-" & Me.GUID)
                        ShowCustomMessageBox(MessageList, "CONFLICT CHECK FLEET SIGNAL", False, LaunchPoint, True)
                        Return True
                    End If
                    MessageList = New ArrayList
                    If AdjacentControlPoint.ManualOperateMode AndAlso optPromptFleetSignalWhenCPOnManualOperate Then
                        MessageList.Add(AdjacentControlPoint.Name & " in Local Control. Do you wish to proceed?")
                        If ShowCustomMessageBox(MessageList, "CONFLICT CHECK FLEET SIGNAL", True, LaunchPoint, True) <> 6 Then
                            Me.SignalEventLog("DISPATCHER(" & gCurrentUser.Initials & ") CLICKED [NO] TO - " & AdjacentControlPoint.Name & " IN LOCAL CONTROL. DO YOU WISH TO PROCEED? - SIGNAL DETAILS: " & Me.Name & "-" & Me.GUID)
                            Return True
                        Else
                            Me.SignalEventLog("DISPATCHER(" & gCurrentUser.Initials & ") CLICKED [YES] TO - " & AdjacentControlPoint.Name & " IN LOCAL CONTROL. DO YOU WISH TO PROCEED? - SIGNAL DETAILS: " & Me.Name & "-" & Me.GUID)
                        End If
                    End If

                    MessageList = New ArrayList
                    If AdjacentControlPoint.StoredRoutes IsNot Nothing AndAlso AdjacentControlPoint.StoredRoutes.Count > 0 Then
                        Comp = Nothing
                        Dim IsokToStopFleeting As Boolean = False
                        sr = Nothing
                        For Each sr In AdjacentControlPoint.StoredRoutes
                            For Each Comp In sr.ComponentSelection
                                If Me.SignalRouteCollection.Contains(Comp) Then
                                    IsokToStopFleeting = True
                                    Exit For
                                End If
                                If IsokToStopFleeting Then Exit For
                            Next
                        Next
                        If IsokToStopFleeting Then
                            Me.SignalEventLog("DISPATCHER(" & gCurrentUser.Initials & ") - " & AdjacentControlPoint.Name & " Conflicting Stored Route Detected. Exiting!!! - Signal Name: " & Me.Name & "-" & Me.GUID)
                            MessageList.Add("Conflicting Stored Route Detected. Exiting!!!")
                            ShowCustomMessageBox(MessageList, "CONFLICT FLEET SIGNAL", False, LaunchPoint, True)
                            Return True
                        End If
                    End If
                End If
                Return False

            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                Comp = Nothing
                MessageList = Nothing
                AdjacentControlPoint = Nothing
                LaunchPoint = Nothing
                TempComp = Nothing
                LastTrack = Nothing
            End Try
        End Function

        Private Function IsTrainEnabledForLicense() As Boolean
            Try

                If Me.ComponentType = "R" Then
                    Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                    If Not IsNothing(trk) Then
                        If trk.TrainIDs.Count > 0 Then
                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                            If trn.IsActivePTC Then
                                Return True
                            End If
                        End If
                        If trk.TrackAuthorities.Count > 0 Then Return True
                    End If
                Else
                    Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                    If Not IsNothing(trk) Then
                        If trk.TrainIDs.Count > 0 Then
                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                            If trn.IsActivePTC Then
                                Return True
                            End If
                        End If
                        If trk.TrackAuthorities.Count > 0 Then Return True
                    End If
                End If
                Return False
            Catch ex As Exception
                GetErrorDetails(ex)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Get The Signal Route
        ''' </summary>
        ''' <param name="addSignal"></param>
        ''' <returns></returns>
        Private Function GetTheSignalRoute(Optional addSignal As Boolean = False) As ArrayList
            Dim EndOfRouteFound As Boolean
            Dim CompToCheck As Object
            Dim SearchRight As Boolean
            Dim FlipFlopFound As Boolean
            Dim NextReferenceGUID, LastReferenceGUID As Int32
            Dim LoopControl As Int32
            Dim theSignalRouteCollection As New ArrayList
            Try
                'set the direction of the search
                If ComponentType = "R" Or ComponentType = "D" Then
                    SearchRight = True
                End If
                'init the first link to valid
                If SearchRight Then
                    NextReferenceGUID = RightLink
                Else
                    NextReferenceGUID = LeftLink
                End If
                While Not EndOfRouteFound
                    CompToCheck = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextReferenceGUID)
                    If IsNothing(CompToCheck) Then Return Nothing
                    Select Case CompToCheck.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(CompToCheck, Switch)
                            If FlipFlopFound Then CompToCheck.FlipMe = True
                            'check for opposing traffic if right search
                            If SearchRight And CompToCheck.TrafficDirection = "LEFT" Then Return Nothing
                            'check for opposing traffic if left search
                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") Then Return Nothing
                            'check to see if switch is CTC operations
                            If CompToCheck.AuthorityType <> "CTC" Then Return Nothing
                            'check request position of switch
                            If sw.TypeOfSwitch <> "DERAIL" Then
                                If SearchRight And CompToCheck.RightAltLink <> 0 Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.RightLink
                                    Else
                                        NextReferenceGUID = CompToCheck.RightAltLink
                                    End If
                                ElseIf Not SearchRight And CompToCheck.LeftAltLink <> 0 Then
                                    If Not CompToCheck.ControlledReverse Then
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftAltLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.LeftLink And SearchRight Then
                                    If CompToCheck.ControlledReverse Then
                                        Return Nothing
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.LeftAltLink And SearchRight Then
                                    If Not CompToCheck.ControlledReverse Then
                                        Return Nothing
                                    Else
                                        NextReferenceGUID = CompToCheck.RightLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.RightLink And Not SearchRight Then
                                    If CompToCheck.ControlledReverse Then
                                        Return Nothing
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                ElseIf LastReferenceGUID = CompToCheck.RightAltLink And Not SearchRight Then
                                    If Not CompToCheck.ControlledReverse Then
                                        Return Nothing
                                    Else
                                        NextReferenceGUID = CompToCheck.LeftLink
                                    End If
                                End If
                            ElseIf sw.TypeOfSwitch = "DERAIL" Then
                                If SearchRight Then
                                    NextReferenceGUID = CompToCheck.RightLink
                                Else
                                    NextReferenceGUID = CompToCheck.LeftLink
                                End If
                            End If
                            'if component passes the validation test, then add it to the collection
                            If Not theSignalRouteCollection.Contains(CompToCheck) Then
                                theSignalRouteCollection.Add(CompToCheck)
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            If Not FlipFlopFound Then
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic = DefaultTraffic Then
                                    EndOfRouteFound = True
                                ElseIf CompToCheck.DefaultTraffic <> DefaultTraffic Then
                                    'if opposing signals found not at stop then exit sub
                                    If Not CompToCheck.FailState Then
                                        If Not DrillMode Then
                                            If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return Nothing
                                            If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return Nothing
                                            If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return Nothing
                                        End If
                                    End If
                                End If
                            Else
                                'if same direction signal found, then end of route found
                                If CompToCheck.DefaultTraffic <> DefaultTraffic Then
                                    EndOfRouteFound = True
                                ElseIf CompToCheck.DefaultTraffic = DefaultTraffic Then
                                    'if opposing signals found not at stop then exit sub
                                    If Not CompToCheck.FailState Then
                                        If CompToCheck.SignalCtlStatus = CtlStatus.RequestedClear Then Return Nothing
                                        If CompToCheck.SignalIndStatus = IndStatus.Cleared Then Return Nothing
                                        If CompToCheck.SignalIndStatus = IndStatus.InTime Then Return Nothing
                                    End If
                                End If
                            End If
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If
                            If addSignal AndAlso
                                Not theSignalRouteCollection.Contains(CompToCheck) Then
                                theSignalRouteCollection.Add(CompToCheck)
                            End If
                        Case "TMDS.TrackComponents.Track"
                            Dim thistrk As Track = CType(CompToCheck, Track)
                            If FlipFlopFound Then CompToCheck.FlipMe = True
                            'check for opposing traffic if right search
                            If (Not SearchRight And CompToCheck.TrafficDirection = "RIGHT") Then
                                SignalEventLog("GetTheSignalRoute:Opposing traffic found:" & thistrk.Name & "-" & thistrk.GUID.ToString & "-" & thistrk.TrafficDirection)
                                If thistrk.CurrentColorStatus.Color = Color.White Then
                                    thistrk.TrafficDirection = ""
                                    thistrk.UpdateStatus()
                                    SignalEventLog("GetTheSignalRoute:TRACK STATUS UPDATED:" & thistrk.Name & "-" & thistrk.GUID.ToString & "-" & thistrk.TrafficDirection)
                                Else
                                    If DrillMode And Not thistrk.IgnoreTrafficOnDrillRequest Then
                                        Return Nothing
                                    Else
                                        Return Nothing
                                    End If
                                End If
                            End If
                            'check for opposing traffic if left search
                            If (SearchRight And CompToCheck.TrafficDirection = "LEFT") Then
                                SignalEventLog("GetTheSignalRoute:Opposing traffic found:" & thistrk.Name & "-" & thistrk.GUID.ToString & "-" & thistrk.TrafficDirection)
                                If thistrk.CurrentColorStatus.Color = Color.White Then
                                    thistrk.TrafficDirection = ""
                                    thistrk.UpdateStatus()
                                    SignalEventLog("GetTheSignalRoute:TRACK STATUS UPDATED:" & thistrk.Name & "-" & thistrk.GUID.ToString & "-" & thistrk.TrafficDirection)
                                Else
                                    If DrillMode And Not thistrk.IgnoreTrafficOnDrillRequest Then
                                        Return Nothing
                                    Else
                                        Return Nothing
                                    End If
                                End If
                            End If
                            If CompToCheck.TrackAndTime Then Return Nothing
                            If CompToCheck.BlockAbsolute Then Return Nothing
                            Select Case thistrk.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And CompToCheck.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And CompToCheck.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select
                            If Not theSignalRouteCollection.Contains(CompToCheck) Then
                                theSignalRouteCollection.Add(CompToCheck)
                            End If
                            LastReferenceGUID = CompToCheck.GUID
                            If SearchRight Then
                                NextReferenceGUID = CompToCheck.RightLink
                            Else
                                NextReferenceGUID = CompToCheck.LeftLink
                            End If
                        Case "TMDS.TrackOtherComponents.RRXING"
                            Dim rr As RRXING = CType(CompToCheck, RRXING)
                            Select Case rr.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And rr.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And rr.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select
                            If Not rr.ValidateForClearRoute(rr.ComponentType) Then Return Nothing
                            If Not theSignalRouteCollection.Contains(rr) Then
                                theSignalRouteCollection.Add(rr)
                            End If
                            NextReferenceGUID = rr.FindExitGUID(LastReferenceGUID)
                            LastReferenceGUID = rr.GUID
                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            Dim ff As FlipFlop = CType(CompToCheck, FlipFlop)
                            FlipFlopFound = True
                            Select Case ff.AuthorityType
                                Case "TWC", "NON-TWC"
                                    EndOfRouteFound = True
                                Case Else
                                    If SearchRight And ff.RightLink = 0 Then
                                        EndOfRouteFound = True
                                    ElseIf Not SearchRight And ff.LeftLink = 0 Then
                                        EndOfRouteFound = True
                                    End If
                            End Select
                            If Not SearchRight Then
                                SearchRight = True
                            Else
                                SearchRight = False
                            End If
                            NextReferenceGUID = ff.FindExitGUID(LastReferenceGUID)
                            LastReferenceGUID = ff.GUID
                    End Select
                    LoopControl += 1
                    If LoopControl > 100 Then
                        Exit While
                    End If
                End While
                Return theSignalRouteCollection
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function CheckForTrainIDsInSignalRequestClearCollection(ByVal CompCollection As ArrayList) As Boolean
            Dim TempArray As New ArrayList
            Dim Comp As Object = Nothing
            Dim trn As TrainID = Nothing
            Dim mw As MOWID = Nothing
            Dim MessageList As New ArrayList
            Try
                For Each Comp In CompCollection
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                            For Each trn In Comp.TrainIDs
                                If gCreateAuthorityTrain IsNot Nothing AndAlso gCreateAuthorityTrain.Name <> trn.Name AndAlso Not TempArray.Contains(trn.Name) Then
                                    TempArray.Add(trn.Name)
                                    Exit For
                                End If
                            Next
                        ElseIf Comp.MOWIDs IsNot Nothing AndAlso Comp.MOWIDs.Count > 0 Then
                            For Each mw In Comp.MOWIDs
                                If gCreateAuthorityTrain IsNot Nothing AndAlso Not TempArray.Contains(mw.DisplayText) Then
                                    TempArray.Add(mw.DisplayText)
                                    Exit For
                                End If
                            Next

                        End If
                    End If
                Next

                If TempArray IsNot Nothing AndAlso TempArray.Count > 0 Then
                    With MessageList
                        .Add("ONE OR MORE TRAIN/MOW IDS FOUND IN THE AUTHORITY LIMITS")
                        .Add("")
                        .Add("HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT?")

                    End With
                    If ShowCustomMessageBox(MessageList, "CONFLICT CHECK", True, Nothing) <> 6 Then
                        SystemEvent("DISPATCHER CLICKED [NO] TO - ONE OR MORE TRAINS FOUND IN SIGNAL SELECTION! PLEASE BE SURE TO RESOLVE THIS CONFLICT. ")
                        ClearSystemVariables()
                        Return False
                    End If
                End If

                Return True

            Catch ex As Exception
                GetErrorDetails(ex)
                Return False
            Finally
                TempArray = Nothing
                Comp = Nothing
                MessageList = Nothing
                trn = Nothing
            End Try
        End Function

#Region "MRS LICENSE CHECK FOR STORED LICENSE"

        Public Function DoLicenseCheckForRearOfTrain(trk As Track, sw As Switch) As Boolean
            Try
                Dim LicenseOverlapOK As Boolean = False
                If trk IsNot Nothing Then
                    If trk.OStype AndAlso Not trk.Occuppied Then
                        Dim theOpposingLicense As TrackAuthority.TrackAuthority = Nothing ' trk.TrackAuthorities.Item(0)
                        If trk.TrackAuthorities IsNot Nothing AndAlso trk.TrackAuthorities.Count > 0 Then theOpposingLicense = trk.TrackAuthorities.Item(0)
                        If theOpposingLicense IsNot Nothing Then
                            If theOpposingLicense.myTrain IsNot Nothing AndAlso theOpposingLicense.myTrain.myRearEndTrack IsNot Nothing Then
                                If trk.OsComponentCollection.Contains(theOpposingLicense.myTrain.myRearEndTrack) Then
                                    LicenseOverlapOK = True
                                End If
                            End If
                        End If
                    End If
                ElseIf sw IsNot Nothing Then
                    If Not sw.Occuppied Then
                        Dim theOpposingLicense As TrackAuthority.TrackAuthority = Nothing ' sw.TrackAuthorities.Item(0)
                        If sw.TrackAuthorities IsNot Nothing AndAlso sw.TrackAuthorities.Count > 0 Then theOpposingLicense = sw.TrackAuthorities.Item(0)

                        If theOpposingLicense IsNot Nothing Then
                            If theOpposingLicense.myTrain IsNot Nothing AndAlso theOpposingLicense.myTrain.myRearEndTrack IsNot Nothing Then
                                If sw.OsComponentCollection.Contains(theOpposingLicense.myTrain.myRearEndTrack) Then
                                    LicenseOverlapOK = True
                                End If
                            End If
                        End If
                    End If
                End If
                Return LicenseOverlapOK
            Catch ex As Exception
                GetErrorDetails(ex)
                Return False
            End Try
        End Function

#End Region


#End Region

#Region "DRILL"

        Private Function GetDrillSignalRoutes(Optional ByVal Initialization As Boolean = False) As Boolean
            Try
                Dim BrotherDrillFound As Boolean
                Dim LoopControl As Integer
                Dim NextComp As Object
                Dim LastReferenceGUID As Int32


                If Me.DefaultTraffic = "LEFT" Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.LeftLink)

                    Do Until BrotherDrillFound
                        Select Case NextComp.GetType.ToString
                            Case "TMDS.TrackComponents.Track"
                                LastReferenceGUID = NextComp.GUID
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            Case "TMDS.SwitchComponents.Switch"
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                If NextComp.ControlledReverse Then
                                    If NextComp.LeftAltLink <> 0 Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If
                            Case "TMDS.SignalComponents.Signal"
                                If NextComp.DefaultTraffic = "RIGHT" Then
                                    Me.DrillPartner = NextComp
                                    If Me.DrillPartner.DeviceBlocked Then
                                        Me.DrillSharedComponents.Clear()
                                        Me.DrillPartner = Nothing
                                        Return False
                                    End If
                                    NextComp.DrillPartner = Me
                                    Exit Do
                                End If
                            Case "TMDS.TrackOtherComponents.RRXING"
                                If Not NextComp.ValidateForClearRoute(Me.ComponentType, LastReferenceGUID) Then
                                    Return False
                                End If
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)

                            Case Else
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                        End Select
                        LoopControl = LoopControl + 1
                        If LoopControl > 20 Then
                            Return False
                        End If
                    Loop
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(Me.RightLink)

                    Do Until BrotherDrillFound
                        Select Case NextComp.GetType.ToString
                            Case "TMDS.TrackComponents.Track"
                                LastReferenceGUID = NextComp.GUID
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            Case "TMDS.SwitchComponents.Switch"
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                If NextComp.ControlledReverse Then
                                    If NextComp.RightAltLink <> 0 Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                End If
                            Case "TMDS.SignalComponents.Signal"
                                If NextComp.DefaultTraffic = "LEFT" Then
                                    Me.DrillPartner = NextComp
                                    If Me.DrillPartner.DeviceBlocked Then
                                        Me.DrillSharedComponents.Clear()
                                        Me.DrillPartner = Nothing
                                        Return False
                                    End If
                                    NextComp.DrillPartner = Me
                                    Exit Do
                                End If
                            Case "TMDS.TrackOtherComponents.RRXING"
                                If Not NextComp.ValidateForClearRoute(Me.ComponentType, LastReferenceGUID) Then
                                    Return False
                                End If
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            Case Else
                                If Not Me.DrillSharedComponents.Contains(NextComp) Then
                                    Me.DrillSharedComponents.Add(NextComp)
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                        End Select
                        LoopControl = LoopControl + 1
                        If LoopControl > 20 Then Return False
                    Loop

                End If

                If Not Me.GetSignalRoute(True, Initialization) Then
                    Return False
                End If

                If Not Me.DrillPartner.GetSignalRoute(True, Initialization) Then
                    Return False
                End If

                Dim comp As Object
                Me.DrillRouteComponents.Clear()
                For Each comp In Me.SignalRouteCollection
                    If Not Me.DrillSharedComponents.Contains(comp) Then
                        If Not Me.DrillRouteComponents.Contains(comp) Then
                            Me.DrillRouteComponents.Add(comp)
                        End If
                    End If
                Next
                Me.SignalRouteCollection.Clear()
                Me.DrillPartner.DrillRouteComponents.Clear()
                If Not IsNothing(Me.DrillPartner) Then
                    For Each comp In Me.DrillPartner.SignalRouteCollection
                        If Not Me.DrillSharedComponents.Contains(comp) Then
                            Me.DrillPartner.DrillRouteComponents.Add(comp)
                        End If
                    Next
                    Me.DrillPartner.SignalRouteCollection.Clear()

                    For Each comp In Me.DrillSharedComponents
                        If Not Me.DrillPartner.DrillSharedComponents.Contains(comp) Then
                            Me.DrillPartner.DrillSharedComponents.Add(comp)
                        End If
                    Next
                End If

                Dim CompToCheck As Object
                For Each CompToCheck In Me.DrillSharedComponents
                    If CompToCheck.BlockAbsolute Then
                        Return False
                    End If
                Next

                For Each CompToCheck In Me.DrillRouteComponents
                    If CompToCheck.BlockAbsolute Then
                        Return False
                    End If
                Next

                For Each CompToCheck In Me.DrillPartner.DrillRouteComponents
                    If CompToCheck.BlockAbsolute Then
                        Return False
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Sub ControlSignalToDrill(Optional ByVal SendControl As Boolean = True, Optional ByVal Initialization As Boolean = False)
            Try
                If GetDrillSignalRoutes(Initialization) Then
                    Me.NoSignalControlInitialization = False
                    Me.DrillPartner.NoSignalControlInitialization = False
                    DrillSignalMaster = True
                    Me.PreviousDrilledControlled = True

                    Me.SignalCtlStatus = CtlStatus.RequestingClear
                    Me.DrillPartner.SignalCtlStatus = CtlStatus.RequestingClear
                    'set signal status
                    Me.DrillMode = True
                    Me.DrillPartner.DrillMode = True
                    If Me.SignalIndStatus = IndStatus.Stopped Or Me.SignalIndStatus = IndStatus.InTime Then
                        Me.SignalStatus = BulbStatus.RequestingClear
                        Me.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))
                        Me.AddToFlashCollection(Me)
                        Me.FlashComponent = True

                        Me.DrillPartner.SignalStatus = BulbStatus.RequestingClear
                        Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))
                        Me.DrillPartner.AddToFlashCollection(Me.DrillPartner)
                        Me.DrillPartner.FlashComponent = True

                        Dim CompInSelection As Object
                        'set the traffic for each switch and track
                        For Each CompInSelection In Me.DrillRouteComponents
                            CompInSelection.TrafficDirection = Me.DefaultTraffic
                            'If Me.DefaultTraffic = "RIGHT" Then
                            '    CompInSelection.TrafficDirection = "LEFT"
                            'Else
                            '    CompInSelection.TrafficDirection = "RIGHT
                            'End If
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next
                        For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                            CompInSelection.TrafficDirection = Me.DrillPartner.DefaultTraffic
                            'If Me.DefaultTraffic = "RIGHT" Then
                            '    CompInSelection.TrafficDirection = "LEFT"
                            'Else
                            '    CompInSelection.TrafficDirection = "RIGHT"
                            'End If
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next

                        For Each CompInSelection In Me.DrillSharedComponents
                            CompInSelection.TrafficDirection = "BOTH"
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()

                            If CompInSelection.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                If CompInSelection.SlottingTrack Then
                                    Me.myDrillTrack = CompInSelection
                                    Me.DrillPartner.myDrillTrack = CompInSelection
                                End If

                            End If
                        Next

                    ElseIf Me.SignalIndStatus = IndStatus.Cleared Then
                        Me.SignalStatus = BulbStatus.ClearStatus
                        Me.RemoveFromFlashCollection(Me)
                        Me.FlashComponent = False
                        Me.Flasher.Enabled = False

                        Me.DrillPartner.SignalStatus = BulbStatus.ClearStatus
                        Me.DrillPartner.AddToFlashCollection(Me.DrillPartner)
                        Me.DrillPartner.FlashComponent = False

                        Dim CompInSelection As Object
                        'set the traffic for each switch and track
                        For Each CompInSelection In DrillRouteComponents
                            CompInSelection.TrafficDirection = Me.DefaultTraffic
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                            CompInSelection.CurrentColorStatus = New SolidBrush(Color.ForestGreen)
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next
                        For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                            CompInSelection.TrafficDirection = Me.DrillPartner.DefaultTraffic
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                            CompInSelection.CurrentColorStatus = New SolidBrush(Color.ForestGreen)
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next

                        For Each CompInSelection In Me.DrillSharedComponents
                            CompInSelection.TrafficDirection = "BOTH"
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                            CompInSelection.CurrentColorStatus = New SolidBrush(Color.ForestGreen)
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next
                    End If

                    If IsNothing(Me.myDrillTrack) Then
                        Dim comp As Object
                        Dim SlotTrackFound As Boolean
                        For Each comp In Me.DrillSharedComponents
                            If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                If comp.SlottingTrack Then
                                    Me.myDrillTrack = comp
                                    SlotTrackFound = True
                                    Exit For
                                End If
                            End If
                        Next
                        If Not SlotTrackFound Then
                            Dim OsTrack As Track = Me.DrillSharedComponents.Item(0)
                            For Each comp In OsTrack.OsComponentCollection
                                If comp.SlottingTrack Then
                                    Me.myDrillTrack = comp
                                    SlotTrackFound = True
                                    Exit For
                                End If
                            Next
                            If Not SlotTrackFound Then
                                Me.myDrillTrack = Me.DrillSharedComponents.Item(0)
                            End If
                        End If
                    End If

                    Me.myDrillTrack.DrillTrack = True
                    Me.myDrillTrack.myDrillSignal = Me

                    Me.UpdateStatus()

                    'This is to keep track of the last control control in the in-time signal collection
                    Dim sig As Signal
                    For Each sig In Me.BrotherSignalIntimeCollection
                        sig.LastControlSignal = False
                    Next
                    Me.LastControlSignal = True
                    If Not Me.DrillControl Is Nothing Then
                        Me.DrillControl.BitState = True
                        If Not IsNothing(Me.DrillStop) Then
                            Me.DrillStop.BitState = False
                        End If
                        If SendControl Then
                            Me.AddToSendCollection("ControlSignalToDrill", True)
                            gSendCTC = True
                            TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                        End If
                    End If
                Else
                    DrillSignalMaster = False
                    Me.DrillRouteComponents.Clear()
                    If Not IsNothing(Me.DrillPartner) Then
                        If Not IsNothing(Me.DrillPartner.DrillRouteComponents) Then
                            Me.DrillPartner.DrillRouteComponents.Clear()
                        End If
                    End If

                    Me.DrillSharedComponents.Clear()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlSignalToDrillStop()
            Try
                Me.SignalCtlStatus = CtlStatus.RequestingStop
                Me.DrillSignalMaster = False
                If Not IsNothing(Me.DrillPartner) Then
                    Me.DrillPartner.SignalCtlStatus = CtlStatus.RequestingStop
                End If

                If Not IsNothing(Me.myDrillTrack) Then
                    Try
                        Me.myDrillTrack.DrillTrack = False
                        Me.myDrillTrack.myDrillSignal = Nothing
                        Me.myDrillTrack = Nothing
                    Catch
                    End Try
                End If

                If Me.SignalIndStatus = IndStatus.Stopped Then
                    Me.SignalStatus = BulbStatus.StopStatus

                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))

                    Me.RemoveFromFlashCollection(Me)

                    Flasher.Enabled = False
                    FlashComponent = False
                    If Not IsNothing(Me.DrillPartner) Then
                        Me.DrillPartner.SignalStatus = BulbStatus.StopStatus
                        Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                        Me.RemoveFromFlashCollection(Me.DrillPartner)
                        Me.DrillPartner.Flasher.Enabled = False
                        Me.DrillPartner.FlashComponent = False
                    End If


                    Dim CompInSelection As Object
                    'set the traffic for each switch and track
                    If Not IsNothing(Me.DrillRouteComponents) Then
                        For Each CompInSelection In Me.DrillRouteComponents
                            Dim TrafficRouteEstablished As Boolean
                            If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                            If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                            End If
                        Next
                    End If
                    If Not IsNothing(Me.DrillPartner) Then
                        If Not IsNothing(Me.DrillPartner.DrillRouteComponents) Then
                            For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                                Dim TrafficRouteEstablished As Boolean
                                If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                                If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                                If Not TrafficRouteEstablished Then
                                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                    CompInSelection.TrafficDirection = ""
                                    CompInSelection.ApplyTraffic()
                                    CompInSelection.UpdateStatus()
                                End If
                            Next
                        End If
                    End If

                    If Not IsNothing(Me.DrillSharedComponents) Then
                        For Each CompInSelection In Me.DrillSharedComponents
                            Dim TrafficRouteEstablished As Boolean
                            If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                            If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                            End If
                        Next
                    End If
                ElseIf Me.SignalIndStatus = IndStatus.Cleared Then
                    Me.SignalStatus = BulbStatus.RequestingStop
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                    Me.AddToFlashCollection(Me)
                    FlashComponent = True

                    If Not IsNothing(Me.DrillPartner) Then
                        Me.DrillPartner.SignalStatus = BulbStatus.RequestingStop
                        Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                        Me.AddToFlashCollection(Me.DrillPartner)
                        Me.DrillPartner.FlashComponent = True
                    End If

                    'set the traffic for each switch and track
                    Dim CompInSelection As Object

                    If Not IsNothing(Me.DrillRouteComponents) Then
                        For Each CompInSelection In Me.DrillRouteComponents
                            Dim TrafficRouteEstablished As Boolean
                            If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                            If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                            End If
                        Next
                    End If

                    If Not IsNothing(Me.DrillPartner) Then
                        If Not IsNothing(Me.DrillPartner.DrillRouteComponents) Then
                            For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                                Dim TrafficRouteEstablished As Boolean
                                If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                                If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                                If Not TrafficRouteEstablished Then
                                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                    CompInSelection.TrafficDirection = ""
                                    CompInSelection.ApplyTraffic()
                                    CompInSelection.UpdateStatus()
                                End If
                            Next
                        End If
                    End If

                    If Not IsNothing(Me.DrillSharedComponents) Then
                        For Each CompInSelection In Me.DrillSharedComponents
                            Dim TrafficRouteEstablished As Boolean
                            If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                            If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                            End If
                        Next
                    End If
                    'ElseIf Me.SignalIndStatus = IndStatus.InTime Then
                    '    Me.SetToInTimeRoute()
                End If

                If Not Me.Ctl(0) Is Nothing Then
                    Me.Ctl(0).BitState = False
                End If
                If Not Me.Ctl(1) Is Nothing Then
                    Me.Ctl(1).BitState = False
                End If
                If Not Me.Ctl(2) Is Nothing Then
                    Me.Ctl(2).BitState = True
                End If
                If Not Me.DrillControl Is Nothing Then
                    Me.DrillControl.BitState = False
                    If Not IsNothing(Me.DrillStop) Then
                        Me.DrillStop.BitState = True
                    End If
                End If

                If Me.LastCtlStatus <> Me.SignalCtlStatus Then
                    If Me.FailState And Not Me.DoMonitorMode Then
                        'Me.SendSignalControlUpdate()
                    ElseIf Not Me.DoMonitorMode And Not Me.FailState Then
                        Me.AddToSendCollection("ControlSignalToDrillStop")
                        gSendCTC = True
                        TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
                    End If
                End If

                If Me.SimulateMode Then
                    Me.DoSimulate = True
                End If

                Me.UpdateStatus()

                If Not IsNothing(Me.DrillPartner) Then
                    Me.DrillPartner.UpdateStatus()
                End If


                Me.tmrCheckRoute.Enabled = False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToDrillClear()
            Try
                Me.DrillMode = True
                Me.DrillPartner.DrillMode = True

                Me.SignalStatus = BulbStatus.ClearStatus
                Me.DrillPartner.SignalStatus = BulbStatus.ClearStatus

                Me.SignalCtlStatus = CtlStatus.RequestedClear
                Me.DrillPartner.SignalCtlStatus = CtlStatus.RequestedClear

                Me.CurrentBulbColorStatus = (New SolidBrush(Color.Green))
                Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Green))

                Me.RemoveFromFlashCollection(Me)
                Me.RemoveFromFlashCollection(Me.DrillPartner)

                Flasher.Enabled = False
                FlashComponent = False

                Me.DrillPartner.Flasher.Enabled = False
                Me.DrillPartner.FlashComponent = False

                Dim CompInSelection As Object
                'set the traffic for each switch and track
                If Not IsNothing(Me.DrillRouteComponents) Then
                    For Each CompInSelection In Me.DrillRouteComponents
                        CompInSelection.TrafficDirection = Me.DefaultTraffic
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    Next
                End If
                If Not IsNothing(Me.DrillPartner) Then
                    If Not IsNothing(Me.DrillPartner.DrillRouteComponents) Then
                        For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                            CompInSelection.TrafficDirection = Me.DrillPartner.DefaultTraffic
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        Next
                    End If
                End If

                For Each CompInSelection In Me.DrillSharedComponents
                    CompInSelection.TrafficDirection = "BOTH"
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared
                    CompInSelection.ApplyTraffic()
                    CompInSelection.UpdateStatus()
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToDrillRequestingClear()
            Try
                Me.DrillMode = True
                Me.DrillPartner.DrillMode = True

                Me.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))
                Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Yellow))

                Me.AddToFlashCollection(Me)
                Me.AddToFlashCollection(Me.DrillPartner)

                FlashComponent = True
                Flasher.Enabled = True

                Me.DrillPartner.FlashComponent = True
                Me.DrillPartner.Flasher.Enabled = True

                Dim CompInSelection As Object

                'set the traffic for each switch and track
                For Each CompInSelection In Me.DrillRouteComponents
                    Dim EndOfRoute As Boolean
                    If CompInSelection.Occuppied And CompInSelection.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then EndOfRoute = True
                    If Not EndOfRoute Then
                        CompInSelection.TrafficDirection = Me.DefaultTraffic
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                        CompInSelection.CurrentColorStatus = New SolidBrush(Color.Yellow)
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next

                For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                    Dim EndOfRoute As Boolean
                    If CompInSelection.Occuppied And CompInSelection.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then EndOfRoute = True
                    If Not EndOfRoute Then
                        CompInSelection.TrafficDirection = Me.DrillPartner.DefaultTraffic
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                        CompInSelection.CurrentColorStatus = New SolidBrush(Color.Yellow)
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    End If
                Next

                For Each CompInSelection In Me.DrillSharedComponents
                    CompInSelection.TrafficDirection = "BOTH"
                    CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalRequestedDrill
                    CompInSelection.ApplyTraffic()
                    CompInSelection.UpdateStatus()
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToDrillStop()
            Try
                Me.SignalStatus = BulbStatus.StopStatus
                Me.SignalCtlStatus = CtlStatus.RequestedStop
                Me.DrillSignalMaster = False
                Me.PreviousDrilledControlled = False
                If Not IsNothing(Me.DrillPartner) Then
                    Me.DrillPartner.SignalStatus = BulbStatus.StopStatus
                    Me.DrillPartner.SignalCtlStatus = CtlStatus.RequestedStop
                    'Me.DrillPartner.DrillMode = False
                End If

                If Me.BlockAbsolute Then
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.MediumSlateBlue))
                    'Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.MediumSlateBlue))
                Else
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                    'Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                End If

                If Not IsNothing(Me.DrillPartner) Then
                    If Me.BlockAbsolute Then
                        Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.MediumSlateBlue))
                    Else
                        Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Red))
                    End If
                End If

                Me.RemoveFromFlashCollection(Me)
                Flasher.Enabled = False
                FlashComponent = False

                If Not IsNothing(Me.DrillPartner) Then
                    Me.RemoveFromFlashCollection(Me.DrillPartner)
                    Me.DrillPartner.Flasher.Enabled = False
                    Me.DrillPartner.FlashComponent = False
                End If

                Dim CompInSelection As Object
                If Not IsNothing(Me.DrillRouteComponents) Then
                    For Each CompInSelection In Me.DrillRouteComponents
                        Dim TrafficRouteEstablished As Boolean
                        If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                        'If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                        If Not TrafficRouteEstablished Then
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            CompInSelection.TrafficDirection = ""
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        End If
                    Next
                End If

                If Not IsNothing(Me.DrillPartner) Then
                    If Not IsNothing(Me.DrillPartner.DrillRouteComponents) Then
                        For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                            Dim TrafficRouteEstablished As Boolean
                            If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                            'If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                            If Not TrafficRouteEstablished Then
                                CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                                CompInSelection.TrafficDirection = ""
                                CompInSelection.ApplyTraffic()
                                CompInSelection.UpdateStatus()
                            End If
                        Next
                    End If
                End If

                If Not IsNothing(Me.DrillSharedComponents) Then
                    For Each CompInSelection In Me.DrillSharedComponents
                        Dim TrafficRouteEstablished As Boolean
                        If CompInSelection.Occuppied And CompInSelection.TrafficDirection <> "" Then TrafficRouteEstablished = True
                        'If CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalCleared Then TrafficRouteEstablished = True
                        If Not TrafficRouteEstablished Then
                            CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            CompInSelection.TrafficDirection = ""
                            CompInSelection.ApplyTraffic()
                            CompInSelection.UpdateStatus()
                        End If
                    Next
                End If

                'Me.DrillMode = False

                If Not IsNothing(Me.DrillPartner) Then
                    'Me.DrillPartner.DrillMode = False
                    Me.DrillPartner.SignalRouteCollection.Clear()
                    Me.DrillPartner.DrillSharedComponents.Clear()
                End If
                If Not IsNothing(Me.DrillRouteComponents) Then
                    Me.DrillRouteComponents.Clear()
                End If
                'If Not IsNothing(Me.myDrillTrack) Then
                '    Try
                '        Me.myDrillTrack.DrillTrack = False
                '        Me.myDrillTrack.myDrillSignal = Nothing
                '        Me.myDrillTrack = Nothing
                '    Catch
                '    End Try
                'End If


                SignalRouteCollection.Clear()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub SetToDrillUnSolicated()

            Try
                Me.DrillMode = True
                Me.DrillPartner.DrillMode = True

                Me.SignalCtlStatus = CtlStatus.RequestedStop
                Me.SignalStatus = BulbStatus.Unsolitated

                Me.DrillPartner.SignalStatus = BulbStatus.Unsolitated
                Me.DrillPartner.SignalCtlStatus = CtlStatus.RequestedStop
                If Me.BlockAbsolute Then
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.DarkViolet))
                    Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.DarkViolet))
                Else
                    Me.CurrentBulbColorStatus = (New SolidBrush(Color.Cyan))
                    Me.DrillPartner.CurrentBulbColorStatus = (New SolidBrush(Color.Cyan))
                End If

                Me.AddToFlashCollection(Me)
                Flasher.Enabled = True
                FlashComponent = True

                Me.AddToFlashCollection(Me.DrillPartner)
                Me.DrillPartner.Flasher.Enabled = True
                Me.DrillPartner.FlashComponent = True

                If Me.GetDrillSignalRoutes Then
                    Dim CompInSelection As Object
                    'set the traffic for each switch and track
                    For Each CompInSelection In Me.DrillRouteComponents
                        CompInSelection.TrafficDirection = Me.DefaultTraffic
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    Next
                    For Each CompInSelection In Me.DrillPartner.DrillRouteComponents
                        CompInSelection.TrafficDirection = Me.DrillPartner.DefaultTraffic
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    Next

                    For Each CompInSelection In Me.DrillSharedComponents
                        CompInSelection.TrafficDirection = "BOTH"
                        CompInSelection.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated
                        CompInSelection.ApplyTraffic()
                        CompInSelection.UpdateStatus()
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "EVENT LOGGING"

        Public Enum EventTypeSignal
            IND
            CTL
            BLK
            LCK
            STATUS
            FAIL
        End Enum

        Private Delegate Sub DoEventLogSignal(ByVal EventType As EventTypeSignal)

        ''' <summary>
        ''' Log Signal Event
        ''' </summary>
        ''' <param name="eventType">Event Type</param>
        Public Sub LogTheSignalEvent(eventType As EventTypeSignal)
            Try
                Dim curUser As String = $"{TMDS.GlobalDeclarations.gCurrentUser.LastName},{TMDS.GlobalDeclarations.gCurrentUser.FirstName}"
                Dim curWrkstn As Integer = Convert.ToInt32(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim curTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(TerritoryID)
                Dim logString As String = "INSERT INTO tblSystemEvents "
                Dim sqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim sqlValues As String
                Dim c As String = ", "
                Dim q As String = "'"
                sqlValues = $" VALUES ({q}{Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}{q}{c}{TerritoryID}{c}{curWrkstn}{c}{q}{curUser}{q}{c}"
                sqlValues = $"{sqlValues}{q}{curTerr.Dbversion}{q}{c}{q}SIGNAL{q}{c}{q}{GUID.ToString}{q}{c}"
                Dim eventTypeString As String = String.Empty
                Select Case eventType
                    Case EventTypeSignal.CTL
                        Select Case SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                sqlValues = $"{sqlValues}{q}CTL-CLEAR{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-CLEAR"
                            Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                                sqlValues = $"{sqlValues}{q}CTL-STOP{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-STOP"
                            Case Else
                                sqlValues = $"{sqlValues}{q}CTL-UNKNOWN{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-UNKNOWN"
                        End Select
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSignal.IND
                        Select Case SignalIndStatus
                            Case IndStatus.Cleared
                                sqlValues = $"{sqlValues}{q}IND-CLEAR{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-CLEAR"
                            Case IndStatus.Stopped
                                sqlValues = $"{sqlValues}{q}IND-STOP{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-STOP"
                            Case IndStatus.InTime
                                sqlValues = $"{sqlValues}{q}IND-INTIME{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-INTIME"
                            Case IndStatus.InTimeNoControlledSignal
                                sqlValues = $"{sqlValues}{q}IND-INTIMENO{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-INTIMENO"
                            Case IndStatus.Unsolitated
                                sqlValues = $"{sqlValues}{q}IND-UNSOLICITED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-UNSOLICITED"
                            Case Else
                                sqlValues = $"{sqlValues}{q}IND-UNKNOWN{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "IND-UNKNOWN"
                        End Select
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSignal.STATUS
                        Select Case SignalStatus
                            Case BulbStatus.Unsolitated
                                sqlValues = $"{sqlValues}{q}STATUS-UNSOLICITED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-UNSOLICITED"
                            Case BulbStatus.ClearStatus
                                sqlValues = $"{sqlValues}{q}STATUS-CLEAR{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-CLEAR"
                            Case BulbStatus.InTime
                                sqlValues = $"{sqlValues}{q}STATUS-INTIME{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-INTIME"
                            Case BulbStatus.RequestingClear
                                sqlValues = $"{sqlValues}{q}STATUS-REQUESTINGCLEAR{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-REQUESTINGCLEAR"
                            Case BulbStatus.RequestingStop
                                sqlValues = $"{sqlValues}{q}STATUS-REQUESTINGSTOP{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-REQUESTINGSTOP"
                            Case BulbStatus.StopStatus
                                sqlValues = $"{sqlValues}{q}STATUS-STOP{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-STOP"
                            Case Else
                                sqlValues = $"{sqlValues}{q}STATUS-UNKNOWN{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "STATUS-UNKNOWN"
                        End Select
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSignal.BLK
                        If BlockAbsolute Then
                            sqlValues = $"{sqlValues}{q}SIGNAL-BLOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "SIGNAL-BLOCKED"
                        Else
                            sqlValues = $"{sqlValues}{q}SIGNAL-UNBLOCKED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                            eventTypeString = "SIGNAL-UNBLOCKED"
                        End If
                        sqlValues = $"{sqlValues})"
                    Case EventTypeSignal.FAIL
                        Select Case SignalCtlStatus
                            Case CtlStatus.RequestedClear, CtlStatus.RequestingClear
                                sqlValues = $"{sqlValues}{q}CTL-CLEAR-FAILED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-CLEAR-FAILED"
                            Case CtlStatus.RequestedStop, CtlStatus.RequestingStop
                                sqlValues = $"{sqlValues}{q}CTL-STOP-FAILED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-STOP-FAILED"
                            Case Else
                                sqlValues = $"{sqlValues}{q}CTL-UNKNOWN-FAILED{q}{c}{q}{myControlPoint.Name}:{Name}{q}"
                                eventTypeString = "CTL-UNKNOWN-FAILED"
                        End Select
                        sqlValues = $"{sqlValues})"
                    Case Else
                        SystemException($"{eventType}-LogTheSignalEvent")
                End Select
                TMDS.NetworkMessages.CadMessages.SendSignalEventToWabtecOne(Me, eventTypeString)
                SyncLock gSystemEventLogQueue
                    TMDS.GlobalDeclarations.gSystemEventLogQueue.Enqueue($"{logString}{sqlFields}{sqlValues}")
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "STATUS UPDATES"

        Public Enum EventTypeSig
            REQUESTCLEAR
            FLEETON
            FLEETOFF
            REQUESTSTOP
            STACKON
            STACKOFF
        End Enum

        Private Delegate Sub DoPublishSignalEvent(ByVal EventType As EventTypeSig)

        Public Sub PublishSignalEventUpdate(ByVal EventType As EventTypeSig)
            Try
                Dim Message As String = "DC" & Chr(6) & "SIGNAL;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString

                Select Case EventType
                    Case EventTypeSig.REQUESTCLEAR
                        Message = Message & "|" & "RC"
                    Case EventTypeSig.REQUESTSTOP
                        Message = Message & "|" & "RS"
                    Case EventTypeSig.FLEETON
                        Message = Message & "|" & "FON"
                    Case EventTypeSig.FLEETOFF
                        Message = Message & "|" & "FOF"
                    Case EventTypeSig.STACKON
                        Message = Message & "|" & "SON"
                    Case EventTypeSig.STACKOFF
                        Message = Message & "|" & "SOF"

                End Select

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
                    If Me.StatusPreviousSignalStatusInd = Me.SignalIndStatus And Me.StatusPreviousSignalStatusCtl = Me.SignalCtlStatus Then
                        Exit Sub
                    Else
                        Me.StatusPreviousSignalStatusInd = Me.SignalIndStatus
                        Me.StatusPreviousSignalStatusCtl = Me.SignalCtlStatus
                    End If
                End If
                Dim Message As String = "DC" & Chr(6) & "SIGSTAT;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.myControlPoint.GUID.ToString
                Message = Message & "|" & Me.GUID.ToString

                If Me.SignalCtlStatus = CtlStatus.RequestedClear Or Me.SignalCtlStatus = CtlStatus.RequestingClear Then
                    Message = Message & "|CTL=TRUE"
                Else
                    Message = Message & "|CTL=FALSE"
                End If

                If Me.SignalIndStatus = IndStatus.Cleared Or Me.SignalIndStatus = IndStatus.Drilled Then
                    Message = Message & "|IND=TRUE"
                Else
                    Message = Message & "|IND=FALSE"
                End If
                Message = Me.TerritoryID.ToString & Chr(12) & Message

                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                End SyncLock
            Catch EX As Exception
                SystemException(EX.ToString)
            End Try
        End Sub
#End Region

    End Class

#Region "SIGNAL AUTHORIZATION"

    Public Class SignalAuthorization
        Inherits UserControl

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                myBrush = Nothing
                LockBitPath = Nothing
            End If
        End Sub

        Sub New(ByVal myLeft As Int32, ByVal myTop As Int32, ByVal SignalPosition As String)
            Me.Visible = False
            Me.Left = myLeft
            Me.Top = myTop
            Me.Position = SignalPosition
            ForeColor = Color.Yellow
            BuildLockGraphics()
        End Sub
        Public LockBitPath As GraphicsPath
        Public Position As String
        Public myBrush As New SolidBrush(Color.Red)

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'Me.BringToFront()
                e.Graphics.FillPath(myBrush, Me.LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub BuildLockGraphics()
            Try
                LockBitPath = New GraphicsPath()
                If Position = "L" Then
                    Me.Top = Me.Top - 5
                    Me.Left = Me.Left - 12
                    Me.Width = 10
                    Me.Height = 20
                    LockBitPath.AddLine(New Point(20, 20), New Point(20, 0))
                    LockBitPath.AddLine(New Point(20, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(20, 20))
                ElseIf Position = "R" Then
                    Me.Top = Me.Top + 4
                    Me.Left = Me.Left + 22
                    Me.Width = 15
                    Me.Height = 15
                    LockBitPath.AddLine(New Point(0, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(10, 5))
                    LockBitPath.AddLine(New Point(10, 5), New Point(0, 0))
                End If
                Me.Region = New Region(LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "SIGNAL CONTROL ARROW"

    Public Class SignalAuthorizationControlArrow
        Inherits UserControl

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                myBrush = Nothing
                LockBitPath = Nothing
            End If
        End Sub

        Sub New(ByVal myLeft As Int32, ByVal myTop As Int32, ByVal SignalPosition As String)
            Me.Visible = True
            Me.Left = myLeft
            Me.Top = myTop
            Me.Position = SignalPosition
            BuildGraphics()
        End Sub
        Public LockBitPath As GraphicsPath
        Public Position As String
        Public myBrush As New SolidBrush(Color.DarkGray)
        Public mySignal As Signal

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'Me.BringToFront()
                If Me.LockBitPath IsNot Nothing Then e.Graphics.FillPath(myBrush, Me.LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            Try
                If IsNothing(mySignal) Then Exit Sub
                If mySignal.SignalCtlStatus = Signal.CtlStatus.RequestedClear Or mySignal.SignalCtlStatus = Signal.CtlStatus.RequestingClear Or mySignal.SignalIndStatus = Signal.IndStatus.Cleared Then Exit Sub
                If mySignal.NearSideSignalOn Then
                    mySignal.NearSideSignalOn = False
                Else
                    mySignal.NearSideSignalOn = True
                End If
                mySignal.NearSideSignalControlUpdateStatus()

                If Not IsNothing(mySignal.BrotherSignalCollection) Then
                    If mySignal.BrotherSignalCollection.Count > 0 Then
                        Dim s As Signal
                        For Each s In mySignal.BrotherSignalCollection
                            s.NearSideSignalOn = mySignal.NearSideSignalOn
                            s.NearSideSignalControlUpdateStatus()
                        Next
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ToggleControlNearSignalControl(ByVal ToggleOff As Boolean)
            Try
                If IsNothing(mySignal) Then Exit Sub
                If mySignal.SignalCtlStatus = Signal.CtlStatus.RequestedClear Or mySignal.SignalCtlStatus = Signal.CtlStatus.RequestingClear Or mySignal.SignalIndStatus = Signal.IndStatus.Cleared Then Exit Sub

                mySignal.NearSideSignalOn = ToggleOff
                mySignal.NearSideSignalControlUpdateStatus()


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub BuildGraphics()
            Try
                LockBitPath = New GraphicsPath()
                If Position = "L" Then
                    Me.Top = Me.Top - 20 '5
                    Me.Left = Me.Left + 10 '- 15
                    Me.Width = 10
                    Me.Height = 20
                    LockBitPath.AddLine(New Point(20, 20), New Point(20, 0))
                    LockBitPath.AddLine(New Point(20, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(20, 20))
                ElseIf Position = "R" Then
                    Me.Top = Me.Top + 20 '4
                    Me.Left = Me.Left '+ 25
                    Me.Width = 15
                    Me.Height = 15
                    LockBitPath.AddLine(New Point(0, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(10, 5))
                    LockBitPath.AddLine(New Point(10, 5), New Point(0, 0))
                End If
                Me.Region = New Region(LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "SIGNAL INDICATION ARROW"

    Public Class SignalAuthorizationIndicationArrow
        Inherits UserControl

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                myBrush = Nothing
                LockBitPath = Nothing
            End If
        End Sub

        Sub New(ByVal myLeft As Int32, ByVal myTop As Int32, ByVal SignalPosition As String)
            Me.Visible = True
            Me.Left = myLeft
            Me.Top = myTop
            Me.Position = SignalPosition
            BuildGraphics()
        End Sub
        Public LockBitPath As GraphicsPath
        Public Position As String
        Public myBrush As New SolidBrush(Color.DarkGray)

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'Me.BringToFront()
                If Me.LockBitPath IsNot Nothing Then e.Graphics.FillPath(myBrush, Me.LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub BuildGraphics()
            Try
                LockBitPath = New GraphicsPath()
                If Position = "L" Then
                    Me.Top = Me.Top - 20
                    Me.Left = Me.Left - 5
                    Me.Width = 10
                    Me.Height = 20
                    LockBitPath.AddLine(New Point(20, 20), New Point(20, 0))
                    LockBitPath.AddLine(New Point(20, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(20, 20))
                ElseIf Position = "R" Then
                    Me.Top = Me.Top + 20
                    Me.Left = Me.Left + 15
                    Me.Width = 15
                    Me.Height = 15
                    LockBitPath.AddLine(New Point(0, 0), New Point(0, 10))
                    LockBitPath.AddLine(New Point(0, 10), New Point(10, 5))
                    LockBitPath.AddLine(New Point(10, 5), New Point(0, 0))
                End If
                Me.Region = New Region(LockBitPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region


#End Region

#Region "STORED ROUTES"

    Public Class StoredRoute
        Public TerrUID As Int32
        Public EntrySignal As Signal
        Public ExitSignal As Signal
        Public ComponentSelection As New ArrayList()
        Public SwitchExecuteList As New ArrayList()
        Public Priority As Integer
        Public CtrlPoint As ControlPoint
        Public TrafficDirection As String
        Public RouteExecuted As Boolean
        Private ThisStoredRoute As StoredRoute
        Public CreateTime As Date
        Public RouteGUID As String = Guid.NewGuid.ToString.ToUpper
        Public ThisCurrentSelection As ArrayList
        Public myTacticalRoute As TacticalRoute
        Public DisplayStoredRoute As Boolean
        Public myRouteDependentSymbolName As String
        'AUTO-ROUTER
        Public SignalRouteGUID As String
        Public AutoRouteGenerated As Boolean = False
        Public TrainSymbolAssociation As String
        Public TrainGUID As String
        'GEO FENCE
        Public theTrain As TrainID
        Public theSignalRoute As SignalRoute
        Public jointAuthorizationStoredRoute As Boolean
        'SLFA - STORED LICENSE FOR FREE AREA
        Public SLFATrackList As New ArrayList
        'DELEGATE
        Private Delegate Sub DoPublishControlPointEvent(ByVal EventType As TMDS.GraphicComponent.ControlPoint.EventTypeCP, ByVal SR As StoredRoute)
        'GROUPED STORE ROUTE (GSR)
        Public GroupedStoredRoute As Boolean
        Public GroupedStoredRouteParent As Boolean
        Public GroupedStoredRouteOKToExecute As Boolean
        Public GroupedStoredRouteCollection As New ArrayList
        Public GroupedStoredRouteDetails As String = String.Empty

        Sub Dispose(Optional ByVal RemoveMeFromDatabase As Boolean = False)
            Try
                If RemoveMeFromDatabase Then Me.RemoveMe()
                ComponentSelection = Nothing
                SwitchExecuteList = Nothing
                ThisStoredRoute = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function RearrangeCollection(ByVal CurrentSelection As ArrayList) As ArrayList
            Dim sigCollection As New ArrayList
            Dim comp As Object = Nothing
            Dim RightObject As Object = Nothing
            Dim LeftObject As Object = Nothing
            Dim Coll As New ArrayList
            Dim NewColl As New ArrayList

            Try

                For Each comp In CurrentSelection
                    If comp.GetType.ToString = gSignalComponentName Then
                        If Not sigCollection.Contains(comp) Then sigCollection.Add(comp)
                    Else
                        If Not Coll.Contains(comp) Then Coll.Add(comp)
                    End If
                Next
                comp = Nothing
                CopyArrayCollection(NewColl, Coll)
                For Each comp In Coll
                    RightObject = Nothing
                    LeftObject = Nothing
                    If comp.GetType.ToString = gTrackComponentName Then
                        If comp.RightLink > 0 Then RightObject = FindCompFromGUID(comp.RightLink)
                        If comp.LeftLink > 0 Then LeftObject = FindCompFromGUID(comp.LeftLink)
                        If RightObject IsNot Nothing AndAlso RightObject.GetType.ToString = gSignalComponentName Then
                            If sigCollection.Contains(RightObject) AndAlso NewColl.Contains(comp) Then
                                Dim inx As Integer = NewColl.IndexOf(comp)
                                NewColl.Insert(inx, RightObject)
                            End If
                        ElseIf LeftObject IsNot Nothing AndAlso LeftObject.GetType.ToString = gSignalComponentName Then
                            If sigCollection.Contains(LeftObject) AndAlso NewColl.Contains(comp) Then
                                Dim inx As Integer = NewColl.IndexOf(comp)
                                NewColl.Insert(inx, LeftObject)
                            End If
                        End If
                    End If
                Next
                'CopyArrayCollection(CurrentSelection, NewColl)
                Return NewColl
            Catch ex As Exception
                GetErrorDetails(ex)
                Return Nothing
            Finally
                sigCollection = Nothing
                RightObject = Nothing
                LeftObject = Nothing
                Coll = Nothing
                NewColl = Nothing
                comp = Nothing
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' Create stored route
        ''' </summary>
        ''' <param name="pCurrentSelection"></param>
        ''' <param name="pDirection"></param>
        ''' <param name="pIsJointStoredRoute"></param>
        ''' <param name="pWhoCallMe"></param>
        ''' <returns></returns>
        Function Create(ByRef pCurrentSelection As ArrayList, ByVal pDirection As String, Optional ByVal pIsJointStoredRoute As Boolean = False, Optional pWhoCallMe As String = Nothing) As Boolean
            Dim FirstTrack As Track = Nothing
            Dim LastTrack As Track = Nothing
            Dim LastSearchedTrack As Track = Nothing
            Dim OriginalCollection As New ArrayList
            Dim Flipme As Boolean = False
            Dim Comp As Object = Nothing
            Dim sw As SwitchToExecute
            Dim FirstSignalFound As Boolean
            Dim LastComp As Object = Nothing
            Dim SignalFound As Boolean
            Try
                StoredRouteEventLog("Create:Direction=" & pDirection & ":IsJointStoredRoute=" & pIsJointStoredRoute & ":WhoCallMe=" & pWhoCallMe)
                CreateTime = Now
                ThisCurrentSelection = pCurrentSelection
                TrafficDirection = pDirection
                ThisStoredRoute = Me
                'check for a flipflop of direction
                If optEnableSignalRouteForQuickRoute OrElse optEnableGroupedSignalRoutes Then
                    TrafficDirection = gSearchDirection
                    For Each Comp In pCurrentSelection
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName, gSwitchComponentName
                                If Comp.FlipMe Then
                                    Comp.FlipMe = False
                                End If
                            Case gSignalComponentName
                                If Comp.FlipMe Then
                                    Comp.FlipMe = False
                                End If
                        End Select
                    Next
                    For Each Comp In pCurrentSelection
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName, gSwitchComponentName, gSignalComponentName
                                Comp.FlipMe = Flipme
                            Case gFlipFlopComponentName
                                If Flipme Then
                                    Flipme = False
                                Else
                                    Flipme = True
                                End If
                        End Select
                    Next
                Else
                    For Each Comp In pCurrentSelection
                        If Comp.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then
                            If Not SignalFound Then
                                Exit For
                            Else
                                If TrafficDirection = "LEFT" Then
                                    TrafficDirection = "RIGHT"
                                Else
                                    TrafficDirection = "LEFT"
                                End If
                                Exit For
                            End If
                        ElseIf Comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Dim s As Signal = CType(Comp, Signal)
                            SignalFound = True
                            If optEnableCPSpecificOptions Then
                                If Comp.FleetEnabled Then
                                    Return False
                                End If
                            End If
                        End If

                    Next
                End If
                Dim tmpFlipDirection As String = ""
                Dim tmpNonFlipDirection As String = ""
                If TrafficDirection = "LEFT" Then
                    tmpFlipDirection = "RIGHT"
                    tmpNonFlipDirection = "LEFT"
                Else
                    tmpFlipDirection = "LEFT"
                    tmpNonFlipDirection = "RIGHT"
                End If
                For Each Comp In pCurrentSelection
                    If Comp.GetType.ToString = gTrackComponentName Then
                        Dim trk As Track = CType(Comp, Track)
                        Me.StoredRouteEventLog("Stored route:" & trk.Name & ":" & ComponentSelection.Count.ToString)
                        If trk.AuthorityType = "CTC-OTHER" Then
                            Me.StoredRouteEventLog(trk.Name)
                        End If
                    End If
                    If (optEnableSignalRouteForQuickRoute OrElse optEnableGroupedSignalRoutes) AndAlso Comp IsNot Nothing Then
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName, gSwitchComponentName, gSignalComponentName
                                If Comp.FlipMe Then
                                    TrafficDirection = tmpFlipDirection
                                Else
                                    TrafficDirection = tmpNonFlipDirection
                                End If
                        End Select
                    End If
                    If Not FirstSignalFound Then
                        If TrafficDirection = "LEFT" Then
                            If Comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                Dim s As Signal = CType(Comp, Signal)
                                If optCheckForDisableSignalRouteCondition AndAlso s.DisableStoreRouteFunction Then Return False
                                If Comp.ComponentType = "L" Or Comp.ComponentType = "U" Then
                                    ComponentSelection.Add(Comp)
                                    EntrySignal = Comp
                                    FirstSignalFound = True
                                End If
                            End If
                        Else
                            If Comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                Dim s As Signal = CType(Comp, Signal)
                                If optCheckForDisableSignalRouteCondition AndAlso s.DisableStoreRouteFunction Then Return False
                                If Comp.ComponentType = "R" Or Comp.ComponentType = "D" Then
                                    ComponentSelection.Add(Comp)
                                    FirstSignalFound = True
                                    EntrySignal = Comp
                                End If
                            End If
                        End If
                    Else
                        If TypeOf Comp Is Signal Then
                            Dim signal As Signal = DirectCast(Comp, Signal)
                            If Not CheckForOtherExitSignalsInRoute(pCurrentSelection, EntrySignal, signal) Then
                                If TrafficDirection.Equals(optDirectionLeft) AndAlso
                                    (signal.ComponentType = "R" OrElse signal.ComponentType = "D") Then
                                    If Not ComponentSelection.Contains(signal) Then ComponentSelection.Add(signal)
                                    ExitSignal = signal
                                    ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                    StoredRouteEventLog($"{GetCurrentMethod.Name}:1:ExitSignal={signal.Name}")
                                    Exit For
                                ElseIf TrafficDirection.Equals(optDirectionRight) AndAlso
                                    (signal.ComponentType = "L" OrElse signal.ComponentType = "U") Then
                                    If Not ComponentSelection.Contains(signal) Then
                                        ComponentSelection.Add(signal)
                                        ExitSignal = signal
                                        ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                        StoredRouteEventLog($"{GetCurrentMethod.Name}:2:ExitSignal={signal.Name}")
                                        Exit For
                                    End If
                                ElseIf TrafficDirection.Equals(optDirectionRight) AndAlso
                                    EntrySignal.HoldingSignal AndAlso
                                    (signal.ComponentType = "R" OrElse signal.ComponentType = "D") Then
                                    ExitSignal = signal
                                    StoredRouteEventLog($"{GetCurrentMethod.Name}:3:ExitSignal={signal.Name}")
                                    Exit For
                                Else
                                    ExitSignal = signal
                                End If
                                StoredRouteEventLog($"{GetCurrentMethod.Name}:4:ExitSignal={signal.Name}")
                                Exit For
                            ElseIf optEnableMRSSpecificOptions Then
                                If Not ComponentSelection.Contains(Comp) Then
                                    Dim tmpComp As Object = Nothing
                                    Dim tmpExitSigFound As Boolean = False
                                    If TrafficDirection = "LEFT" Then
                                        tmpComp = FindCompFromGUID(Comp.LeftLink)
                                    Else
                                        tmpComp = FindCompFromGUID(Comp.RightLink)
                                    End If
                                    For I As Integer = 0 To 4
                                        If tmpComp.GetType.ToString = gSignalComponentName AndAlso tmpComp.ComponentType = EntrySignal.ComponentType Then
                                            ComponentSelection.Add(Comp)
                                            ExitSignal = Comp
                                            ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                            Me.StoredRouteEventLog("StoredRouteEvent1.2")
                                            tmpExitSigFound = True
                                            Exit For
                                        Else
                                            If TrafficDirection = "LEFT" Then
                                                tmpComp = FindCompFromGUID(tmpComp.LeftLink)
                                            Else
                                                tmpComp = FindCompFromGUID(tmpComp.RightLink)
                                            End If
                                        End If

                                    Next
                                    If tmpExitSigFound Then Exit For
                                End If
                            Else
                                Me.StoredRouteEventLog("StoredRouteEvent:NO EXIT SIGNAL FOUND")
                            End If
                        ElseIf Comp.GetType.ToString = gRRXINGComponentName Then
                            If Not ComponentSelection.Contains(Comp) Then
                                ComponentSelection.Add(Comp)
                            End If
                        ElseIf Comp.GetType.ToString = gFlipFlopComponentName Then
                        Else
                            If Comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                If Comp.LeftAltLink = LastComp.GUID Or Comp.RightAltLink = LastComp.GUID Then
                                    sw = New SwitchToExecute()
                                    sw.switchToControl = Comp
                                    If Not sw.switchToControl.FlipOperations Then
                                        sw.ControlReverse = True
                                    Else
                                        sw.ControlReverse = False
                                    End If
                                    AddSwitchToExecuteList(sw)
                                End If
                            Else
                                If LastComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                    sw = New SwitchToExecute()
                                    sw.switchToControl = LastComp
                                    If Comp.GUID = LastComp.LeftAltLink Or Comp.GUID = LastComp.RightAltLink Then
                                        If Not sw.switchToControl.FlipOperations Then
                                            sw.ControlReverse = True
                                        Else
                                            sw.ControlReverse = False
                                        End If
                                    Else
                                        If Not sw.switchToControl.FlipOperations Then
                                            sw.ControlReverse = False
                                        Else
                                            sw.ControlReverse = True
                                        End If
                                    End If

                                    AddSwitchToExecuteList(sw)
                                End If
                            End If
                            If Not ComponentSelection.Contains(Comp) Then
                                ComponentSelection.Add(Comp)
                            End If
                        End If
                    End If
                    LastComp = Comp
                Next
                If optEnableMRSSpecificOptions Then
                    Dim tmpColl As New ArrayList
                    CopyArrayCollection(tmpColl, ComponentSelection)
                    ComponentSelection = New ArrayList
                    For Each Comp In tmpColl
                        If Comp IsNot Nothing Then
                            Select Case Comp.GetType.ToString
                                Case gTrackComponentName
                                    If Comp.Indicating Or Comp.AuthorityType = "CTC-OTHER" Then ' AndAlso Mid(Comp.AuthorityType, 1, 3) <> "TWC"
                                        If Not ComponentSelection.Contains(Comp) Then
                                            ComponentSelection.Add(Comp)
                                        End If
                                    Else
                                        Me.StoredRouteEventLog("StoredRouteEvent:EndofStoredRoute:TrackNotAllowed:" & Comp.Name)
                                        Exit For
                                    End If
                                Case Else
                                    If Not ComponentSelection.Contains(Comp) Then ComponentSelection.Add(Comp)
                            End Select
                        End If
                    Next
                End If
                If optEnableSignalRouteForQuickRoute OrElse optEnableGroupedSignalRoutes Then
                    For Each Comp In pCurrentSelection
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName, gSwitchComponentName, gSignalComponentName
                                Comp.FlipMe = False
                        End Select
                    Next
                End If
                If Me.EntrySignal Is Nothing Then
                    If optCheckForDisableSignalRouteCondition AndAlso Me.EntrySignal.DisableStoreRouteFunction Then Return False
                    ComponentSelection = Nothing
                    If Not IsNothing(Me.myTacticalRoute) Then
                        SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                        If Not IsNothing(Me.myTacticalRoute.myTrain) Then
                            Me.myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                        End If
                    End If
                    Exit Function
                Else
                    Me.TerrUID = Me.EntrySignal.TerritoryID
                End If

                If (optApplyConfigurationForKCT Or optEnableMRSSpecificOptions) AndAlso Me.ExitSignal Is Nothing AndAlso LastComp IsNot Nothing Then
                    Select Case TrafficDirection
                        Case "LEFT"
                            Comp = FindCompFromGUID(LastComp.LeftLink)
                            If Not IsNothing(Comp) Then
                                If Comp IsNot Nothing AndAlso Comp.GetType.ToString = gSignalComponentName Then
                                    If TrafficDirection = "LEFT" And (Comp.ComponentType = "R" Or Comp.ComponentType = "D") Then
                                        ComponentSelection.Add(Comp)
                                        ExitSignal = Comp
                                        ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                        Me.StoredRouteEventLog("StoredRouteEvent1.1")
                                    End If
                                Else
                                    Comp = FindCompFromGUID(Comp.LeftLink)
                                    If Comp IsNot Nothing AndAlso Comp.GetType.ToString = gSignalComponentName Then
                                        If TrafficDirection = "LEFT" And (Comp.ComponentType = "R" Or Comp.ComponentType = "D") Then
                                            ComponentSelection.Add(Comp)
                                            ExitSignal = Comp
                                            ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                            Me.StoredRouteEventLog("StoredRouteEvent1.1")
                                        End If
                                    End If
                                End If
                            End If
                        Case "RIGHT"
                            If LastComp.RightLink <> 0 Then
                                Comp = FindCompFromGUID(LastComp.RightLink)
                            End If
                            If Comp IsNot Nothing AndAlso Comp.GetType.ToString = gSignalComponentName Then
                                If TrafficDirection = "RIGHT" And (Comp.ComponentType = "L" Or Comp.ComponentType = "U") Then
                                    ComponentSelection.Add(Comp)
                                    ExitSignal = Comp
                                    ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                    Me.StoredRouteEventLog("StoredRouteEvent1.1")
                                End If
                            Else
                                Comp = FindCompFromGUID(Comp.RightLink)
                                If Not IsNothing(Comp) Then
                                    If TrafficDirection = "LEFT" And (Comp.ComponentType = "L" Or Comp.ComponentType = "U") Then
                                        ComponentSelection.Add(Comp)
                                        ExitSignal = Comp
                                        ExtendRouteToNextSignal(ComponentSelection, ExitSignal, TrafficDirection)
                                        Me.StoredRouteEventLog("StoredRouteEvent1.1")
                                    End If
                                End If

                            End If
                    End Select
                End If
                If Me.ExitSignal Is Nothing Then
                    If Not Me.EntrySignal.HoldingSignal AndAlso Not Me.EntrySignal.AutomaticSignal Then
                        ComponentSelection = Nothing
                        If Me.EntrySignal.AutomaticModeOn And Not IsNothing(Me.myTacticalRoute) Then
                            SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                            If Not IsNothing(Me.myTacticalRoute.myTrain) Then
                                Me.myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                            End If
                        End If
                        Exit Function
                    Else
                        Me.ExitSignal = Me.EntrySignal
                    End If
                End If

                If optDisableStoredRouteOnConflictingFleetSignal Then
                    FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(pCurrentSelection)
                    LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(pCurrentSelection)
                    Select Case gSearchDirection
                        Case "LEFT"
                            If CheckforFleetSignal(FirstTrack, "LEFT", False) Then Return False
                            If CheckforFleetSignal(LastTrack, "RIGHT", True) Then Return False
                        Case "RIGHT"
                            If CheckforFleetSignal(FirstTrack, "RIGHT", False) Then Return False
                            If CheckforFleetSignal(LastTrack, "LEFT", True) Then Return False
                    End Select
                End If
                Me.CtrlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(EntrySignal.StationNumber)
                If Me.CtrlPoint Is Nothing Then
                    ComponentSelection = Nothing
                    If Me.EntrySignal.AutomaticModeOn And Not IsNothing(Me.myTacticalRoute) Then
                        SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                        If Not IsNothing(Me.myTacticalRoute.myTrain) Then
                            Me.myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                        End If
                    End If
                    Exit Function
                Else
                    If Me.CtrlPoint.StoredRoutes.Count > 29 Then
                        Me.Dispose()
                        Exit Function
                    End If
                End If
                If theTrain IsNot Nothing AndAlso
                    theTrain.IsActivePTC Then
                    If theTrain.SLFAEnabled AndAlso
                        theTrain.SLFATimer.Enabled AndAlso
                        theTrain.SLFATimer.Interval > 0 Then
                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Stored group for timer enabled")
                        SLFATrackList = theTrain.SLFATrackList.Clone()
                    End If
                    If Not BindStoredRouteToTrain(EntrySignal, ExitSignal) Then
                        Dim entrySignalName As String = "NOTHING"
                        If EntrySignal IsNot Nothing Then entrySignalName = EntrySignal.Name
                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Cannot bind stored route for sinal {entrySignalName}")
                        Return False
                    End If
                    If optEnableStoredLicenseForFreeArea AndAlso
                        theTrain.SLFATrackList.Count > 0 Then
                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Assigned tracks for stored group")
                        SLFATrackList = theTrain.SLFATrackList.Clone()
                    End If
                End If
                If Not gGropupedStoredRoute AndAlso Me.TryCheckForValidity Then

                    If theTrain IsNot Nothing AndAlso
                        (theTrain.SLFATimer IsNot Nothing AndAlso theTrain.SLFATimer.Enabled = False AndAlso theTrain.SLFATimer.Interval = 0) Or
                        (theTrain IsNot Nothing AndAlso theTrain.SLFATimer Is Nothing) Then

                        If Me.TryExecuteRoute Then
                            Me.CheckForRecursiveStoredRoutes()
                            Return True
                        End If

                    End If
                End If

                If optRailRoadName.Contains("RUMO") AndAlso Me.CtrlPoint.DisableAutomaticSignalSlotting Then
                    StoredRouteEventLog("Stored license disabled for control point " & Me.CtrlPoint.Name)
                    Return False
                End If

                Me.Priority = CtrlPoint.StoredRoutes.Count

                If gGropupedStoredRoute AndAlso gGropupedStoredRouteParent IsNot Nothing Then
                    If Me.RouteGUID = gGropupedStoredRouteParent.RouteGUID Then
                        Me.GroupedStoredRouteParent = True
                        Me.GroupedStoredRoute = True
                    Else
                        Me.GroupedStoredRoute = True
                    End If
                    gGropupedStoredRouteParent.GroupedStoredRouteCollection.Add(Me)
                End If

                If Not InsertRouteIntoDatabase() Then
                    ComponentSelection = Nothing
                    If optEnableMovementPlanningOptions Then
                        If Me.EntrySignal.AutomaticModeOn And Not IsNothing(Me.myTacticalRoute) Then
                            SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                            If Not IsNothing(Me.myTacticalRoute.myTrain) Then
                                Me.myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                            End If
                        End If
                    End If
                    Exit Function
                End If

                If Not IsNothing(theTrain) Then
                    theTrain.myStoredRoutes.Add(Me)
                End If

                SyncLock CtrlPoint.StoredRoutes
                    CtrlPoint.StoredRoutes.Insert(Me.Priority, Me)
                End SyncLock

                SyncLock gSummaryRouteQueue.SyncRoot
                    Dim SummItem As New SummaryItem("ROUTE", "ADD", Me)
                    gSummaryRouteQueue.Enqueue(SummItem)
                End SyncLock

                If Me.Priority = 0 Then
                    CheckToDisplayStoredRouteDependency()
                    UpdateSignalDisplays()
                Else
                    CheckToUpdateOtherSignals()
                End If

                If theTrain IsNot Nothing Then
                    Me.StoredRouteEventLog("STORED ROUTE CREATED TO TRAIN " & theTrain.Name & " AT " & Me.CtrlPoint.Name & " FROM ENTRY SIGNAL:" & Me.EntrySignal.Name & " TO EXIT SIGNAL:" & Me.ExitSignal.Name & " <PRIORITY: " & Me.Priority & ">")
                Else
                    Me.StoredRouteEventLog("STORED ROUTE CREATED AT " & Me.CtrlPoint.Name & " FROM ENTRY SIGNAL:" & Me.EntrySignal.Name & " TO EXIT SIGNAL:" & Me.ExitSignal.Name & " <PRIORITY: " & Me.Priority & ">")
                End If

                Dim RouteComp As Object
                Dim CompList As String = "STORE ROUTE COMPONENT LIST:" & vbCrLf
                For Each RouteComp In Me.ComponentSelection
                    CompList = CompList & vbTab & "GUID:" & RouteComp.GUID & " NAME:" & RouteComp.Name & vbCrLf
                Next
                Me.StoredRouteEventLog(CompList)

                Dim PublishEvent As New DoPublishControlPointEvent(AddressOf Me.CtrlPoint.PublishCPEventUpdate)
                PublishEvent.BeginInvoke(ControlPoint.EventTypeCP.SROUTEON, Me, Nothing, Nothing)

                SendRouteExecuteConfirmationMessage("PENDING", "UPDATE")
                CheckForRecursiveStoredRoutes()

                TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(Me, "CREATED")

                Return True

            Catch ex As Exception
                SystemException(ex.ToString)
                ComponentSelection = Nothing
            Finally
                OriginalCollection = Nothing
                FirstTrack = Nothing
                LastTrack = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Bind stored route to train
        ''' </summary>
        ''' <param name="entrySignal">entry Signal</param>
        ''' <param name="exitSignal">exit Signal</param>
        ''' <returns></returns>
        Public Function BindStoredRouteToTrain(ByRef entrySignal As Signal,
                                               ByRef exitSignal As Signal) As Boolean
            Try
                If theTrain Is Nothing Then
                    theTrain = FindTrainFromSymbol(TrainSymbolAssociation)
                    If theTrain Is Nothing Then
                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Train is nothing")
                        Return False
                    End If
                End If
                For Each oneRoute As SignalRoute In gSignalRouteList
                    If oneRoute.EntrySignal IsNot Nothing Then
                        If oneRoute.EntrySignal.GUID = entrySignal.GUID Then
                            If oneRoute.ComponentList IsNot Nothing AndAlso
                                oneRoute.ComponentList.Count > 0 Then
                                If oneRoute.ExitSignal IsNot Nothing Then
                                    If oneRoute.ComponentList.Contains(exitSignal) Or
                                        oneRoute.ExitSignal.GUID = exitSignal.GUID Then
                                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Signal route found 1")
                                        theSignalRoute = oneRoute
                                        Exit For
                                    End If
                                Else
                                    If oneRoute.ComponentList.Contains(exitSignal) Then
                                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Signal route found 2")
                                        theSignalRoute = oneRoute
                                        jointAuthorizationStoredRoute = True
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
                If theSignalRoute Is Nothing Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:Signal route is nothing")
                    Return False
                End If
                StoredRouteEventLog($"{GetCurrentMethod.Name}:Train {theTrain.Name _
                                    } assigned with route {theSignalRoute.EntrySignal.Name}")
                TrainSymbolAssociation = theTrain.Name
                TrainGUID = theTrain.TrainGUID
                SignalRouteGUID = theSignalRoute.SystemUID
                'Create Tactical Route
                Dim oneTacticalRoute As TacticalRoute = New TacticalRoute
                With oneTacticalRoute
                    .mySignalRoute = theSignalRoute
                    .myTrain = theTrain
                    .myGUID = Guid.NewGuid.ToString.ToUpper
                    .EffectiveTime = Now
                    .myStoredRouteUID = Me.RouteGUID
                End With
                myTacticalRoute = oneTacticalRoute
                oneTacticalRoute.CreateRoute(theTrain, theSignalRoute, "STORED ROUTE")
                If theTrain.thisCurrentLicense IsNot Nothing Then
                    theTrain.myNewTacticalRoutes.Add(oneTacticalRoute)
                    theTrain.thisCurrentLicense.UpdateTacticalRouteList()
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub SendRouteExecuteConfirmationMessage(ByVal RouteStatus As String, ByVal MessType As String)
            Try
                If Not Me.AutoRouteGenerated Then Exit Sub
                TMDS.NetworkMessages.MPARMessages.SendRouteStatusMessage(Me.myTacticalRoute, MessType, RouteStatus)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendRouteExecuteErrorMessage(ByVal RouteStatus As String, ByVal MessType As String)
            Try
                If Not Me.AutoRouteGenerated Then Exit Sub
                TMDS.NetworkMessages.MPARMessages.SendRouteErrorMessage(Me.myTacticalRoute, MessType, RouteStatus)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Check For Other Exit Signals In Route
        ''' </summary>
        ''' <param name="components">list of components</param>
        ''' <param name="entrySignal">entry signal</param>
        ''' <param name="exitSignal">exit signal</param>
        ''' <returns>Return <c>True</c>, if exist other signals in route, otherwise, return <c>False</c></returns>
        Private Function CheckForOtherExitSignalsInRoute(components As ArrayList,
                                                         entrySignal As Signal,
                                                         exitSignal As Signal) As Boolean
            Try
                Dim startChecking As Boolean
                For Each signal As Signal In components.OfType(Of Signal)
                    If signal Is exitSignal Then startChecking = True
                    If startChecking AndAlso
                        Not signal.AutomaticSignal AndAlso
                        signal.ComponentType = exitSignal.ComponentType AndAlso
                        signal.myControlPoint.GUID = exitSignal.myControlPoint.GUID AndAlso
                        signal.CodeStation = exitSignal.CodeStation AndAlso
                        Not entrySignal.AutomaticSignal AndAlso
                        Not signal Is exitSignal Then
                        StoredRouteEventLog($"{GetCurrentMethod.Name}:Signal={signal.Name _
                                            }:EntrySignal={entrySignal.Name _
                                            }:ExitSignal={exitSignal.Name}")
                        Return True
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        Public Sub CheckToUpdateOtherSignals()
            Try
                If Me.Priority <> 0 Then
                    Dim sr As StoredRoute
                    'SyncLock Me.CtrlPoint.StoredRoutes
                    For Each sr In Me.CtrlPoint.StoredRoutes
                        If sr.Priority < Me.Priority Then
                            Dim comp As Object
                            For Each comp In Me.ComponentSelection
                                Dim compInRoute As Object
                                For Each compInRoute In sr.ComponentSelection
                                    If compInRoute Is comp Then
                                        Exit Sub
                                    End If
                                Next
                            Next
                        Else
                            Exit For
                        End If
                    Next
                    'End SyncLock
                End If

                UpdateSignalDisplays()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Check for recursive stored routes
        ''' </summary>
        Private Sub CheckForRecursiveStoredRoutes()
            Dim comp As Object
            Try
                Me.StoredRouteEventLog("CheckForRecursiveStoredRoutes:Start")
                For Each comp In Me.ComponentSelection
                    Me.ThisCurrentSelection.Remove(comp)
                Next
                'check for flipflop of direction
                For Each comp In ThisCurrentSelection
                    If comp.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then
                        If TrafficDirection = "LEFT" Then
                            TrafficDirection = "RIGHT"
                        Else
                            TrafficDirection = "LEFT"
                        End If
                        Me.StoredRouteEventLog("CheckForRecursiveStoredRoutes:FlipFlop=" & comp.GUID & ":" & comp.Name & ":NewTrafficDirection=" & TrafficDirection)
                        If Not optEnableSignalRouteForQuickRoute Then Exit For
                    End If
                Next
                Me.StoredRouteEventLog("CheckForRecursiveStoredRoutes:End")
                If Me.ThisCurrentSelection.Count > 1 Then
                    StoredRouteEventLog("CheckForRecursiveStoredRoutes:New stored route:Remaining comps=" & ThisCurrentSelection.Count)
                    Dim NewStoredRoute As New StoredRoute()
                    NewStoredRoute.theTrain = theTrain
                    NewStoredRoute.Create(Me.ThisCurrentSelection, Me.TrafficDirection, False, "CheckForRecursiveStoredRoutes")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                comp = Nothing
            End Try
        End Sub

        Public Sub RemoveMe(Optional CallingMethod As String = Nothing,
                            Optional CancelRoute As Boolean = False,
                            Optional CancelGSR As Boolean = False)
            Try
                StoredRouteEventLog($"{GetCurrentMethod.Name}:{CallingMethod}")
                RouteExecuted = True
                If myTacticalRoute IsNot Nothing AndAlso CancelRoute Then
                    If myTacticalRoute.myTrain IsNot Nothing Then
                        myTacticalRoute.myTrain.RemoveRouteOnward(Me)
                    End If
                    myTacticalRoute.UpdateRouteStatus("STORED ROUTE CANCELLED")
                    myTacticalRoute.RemoveRoute()
                End If
                If GroupedStoredRoute AndAlso Not CancelGSR Then Exit Sub
                UpdateSignalDisplays()
                If Not (gAssistMode OrElse EntrySignal.IsShadowMode) Then
                    Dim PublishEvent As New DoPublishControlPointEvent(AddressOf CtrlPoint.PublishCPEventUpdate)
                    PublishEvent.BeginInvoke(ControlPoint.EventTypeCP.SROUTEOFF, Me, Nothing, Nothing)
                End If
                SyncLock CtrlPoint.StoredRoutes
                    CtrlPoint.StoredRoutes.Remove(Me)
                End SyncLock
                If CtrlPoint.StoredRoutes.Count > 0 Then
                    Dim storedRoute As StoredRoute = CtrlPoint.StoredRoutes(0)
                    storedRoute.UpdateSignalDisplays()
                End If
                If theTrain IsNot Nothing Then
                    theTrain.myStoredRoutes.Remove(Me)
                End If
                If Not (gAssistMode OrElse EntrySignal.IsShadowMode) Then
                    CtrlPoint.ReorderStoredRoutes()
                    If RouteExecuted Then
                        If RemoveRouteFromDatabase() Then
                            Dim LogTheEvent As New DoEventLogStoredRoute(AddressOf LogTheStoredRouteEvent)
                            LogTheEvent.BeginInvoke(EventTypeStoreRoute.CANCEL, Nothing, Nothing)
                            Dispose()
                        End If
                    End If
                    SyncLock gSummaryRouteQueue
                        Dim SummItem As New SummaryItem("ROUTE", "REMOVE", Me)
                        gSummaryRouteQueue.Enqueue(SummItem)
                    End SyncLock
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RemoveMeAndTacticalRoute()
            Try
                Me.StoredRouteEventLog("RemoveMeAndTacticalRoute")
                Me.RouteExecuted = True

                If Not IsNothing(Me.myTacticalRoute) Then
                    Me.myTacticalRoute.UpdateRouteStatus("STORED ROUTE CANCELLED")
                    Me.myTacticalRoute.RemoveRoute()
                End If

                UpdateSignalDisplays()
                If Not (gAssistMode OrElse Me.EntrySignal.IsShadowMode) Then
                    Dim PublishEvent As New DoPublishControlPointEvent(AddressOf Me.CtrlPoint.PublishCPEventUpdate)
                    PublishEvent.BeginInvoke(ControlPoint.EventTypeCP.SROUTEOFF, Me, Nothing, Nothing)
                End If

                SyncLock Me.CtrlPoint.StoredRoutes
                    Me.CtrlPoint.StoredRoutes.Remove(Me)
                End SyncLock

                If Me.CtrlPoint.StoredRoutes.Count > 0 Then
                    Dim sr As StoredRoute = Me.CtrlPoint.StoredRoutes(0)
                    sr.UpdateSignalDisplays()
                End If

                If Not IsNothing(theTrain) Then
                    theTrain.myStoredRoutes.Remove(Me)
                End If


                If Not (gAssistMode OrElse Me.EntrySignal.IsShadowMode) Then
                    Me.CtrlPoint.ReorderStoredRoutes()

                    If Me.RouteExecuted Then
                        If RemoveRouteFromDatabase() Then
                            Dim LogTheEvent As New DoEventLogStoredRoute(AddressOf Me.LogTheStoredRouteEvent)
                            LogTheEvent.BeginInvoke(EventTypeStoreRoute.CANCEL, Nothing, Nothing)
                            Me.Dispose()
                        End If
                    End If
                    SyncLock gSummaryRouteQueue
                        Dim SummItem As New SummaryItem("ROUTE", "REMOVE", Me)
                        gSummaryRouteQueue.Enqueue(SummItem)
                    End SyncLock

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ExtendRouteToNextSignal(ByRef SelectionList As ArrayList, ByVal CompToStart As Signal, ByVal Direction As String)
            Try
                Dim loopControl As Integer
                Dim NextComp As Object = Nothing
                Dim LastComp As Object = Nothing
                Dim FlipDirection As Boolean
                Dim myDirection As String = Direction
                If myDirection = "LEFT" Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(CompToStart.LeftLink)
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(CompToStart.RightLink)
                End If
                Do
                    If NextComp Is Nothing Then Exit Sub
                    If IsNothing(CompToStart) Then Exit Sub

                    If NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        If NextComp.ComponentType <> CompToStart.ComponentType Then
                            If FlipDirection Then
                                If myDirection = "LEFT" Then
                                    myDirection = "RIGHT"
                                Else
                                    myDirection = "LEFT"
                                End If
                            End If
                            Exit Sub
                        End If
                        If Not SelectionList.Contains(NextComp) Then
                            SelectionList.Add(NextComp)
                        End If
                    ElseIf NextComp.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then
                        Dim ff As FlipFlop = CType(NextComp, FlipFlop)
                        NextComp = ff.FindExitComponent(LastComp.GUID)
                        FlipDirection = True
                    Else
                        LastComp = NextComp
                        If Not SelectionList.Contains(NextComp) Then
                            SelectionList.Add(NextComp)
                        End If
                    End If
                    If myDirection = "LEFT" Then
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                    Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                    End If
                    loopControl = loopControl + 1
                    If loopControl = 20 Then
                        If FlipDirection Then
                            If myDirection = "LEFT" Then
                                myDirection = "RIGHT"
                            Else
                                myDirection = "LEFT"
                            End If
                        End If
                        Exit Sub
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub AddSwitchToExecuteList(ByVal swToCheck As SwitchToExecute)
            Try
                Dim AddToList As Boolean = True
                Dim sw As SwitchToExecute
                For Each sw In Me.SwitchExecuteList
                    If swToCheck.switchToControl.GUID = sw.switchToControl.GUID Then
                        AddToList = False
                        Exit For
                    End If
                Next
                If AddToList Then
                    Me.SwitchExecuteList.Add(swToCheck)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Update Route
        ''' </summary>
        ''' <returns></returns>
        Public Function UpdateRoute() As Boolean
            Dim query As String
            Dim q As String = "'"
            Dim s As String = ", "
            Try
                query = "UPDATE tblStoreRoutes "
                query = query & " SET Priority = " & Me.Priority
                query = query & " WHERE RouteGUID = " & q & Me.RouteGUID & q
                SystemDatabaseCommandToDynamic("STOREDROUTE:UpdateRoute", query)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                q = Nothing
                s = Nothing
                query = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Update Group Parent
        ''' </summary>
        ''' <returns></returns>
        Public Function UpdateGroupParent() As Boolean
            Dim query As String
            Dim q As String = "'"
            Dim s As String = ", "
            Dim groupUID As String
            Try
                groupUID = Me.RouteGUID
                For Each sr As StoredRoute In Me.GroupedStoredRouteCollection
                    query = "UPDATE tblStoreRoutes "
                    query = query & " SET GroupedRouteParentGUID = '" & groupUID & "'"
                    query = query & " WHERE RouteGUID = '" & sr.RouteGUID & "'"
                    SystemDatabaseCommandToDynamic("STOREDROUTE:UpdateRoute", query)
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                q = Nothing
                s = Nothing
                query = Nothing
                groupUID = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Insert route into db
        ''' </summary>
        ''' <returns></returns>
        Private Function InsertRouteIntoDatabase() As Boolean
            Dim sqlFields As String
            Dim sqlValues As String
            Dim insertCommand As String
            Dim q As String = "'"
            Dim s As String = ", "
            Dim comp As Object
            Dim compList As String = String.Empty
            Dim swComp As SwitchToExecute
            Dim routeDependentSymbolName As String = String.Empty
            Dim rd As RouteDependency
            Dim logTheEvent As DoEventLogStoredRoute
            Dim slfaTrackList As String = String.Empty
            Try
                sqlFields = "(TerrUID, ControlPoint, CreateTime, Priority, EntrySignal, ExitSignal, ComponentSelection, SwitchList, RouteGUID, "
                sqlFields = sqlFields & "AutoRoute, RouteDependencies, SignalRouteGUID, TrainSymbol, TrainGUID"
                sqlFields = sqlFields & ",GroupedRoute, GroupedRouteParentGUID, GroupedRouteDetails"
                sqlFields = sqlFields & ")"
                sqlValues = " VALUES ("
                sqlValues = sqlValues & Me.TerrUID & s & Me.CtrlPoint.GUID & s & q & Me.CreateTime & q & s & Me.Priority
                sqlValues = sqlValues & s & Me.EntrySignal.GUID & s & Me.ExitSignal.GUID
                If ComponentSelection Is Nothing Then
                    Exit Function
                End If
                For Each comp In Me.ComponentSelection
                    compList = compList & comp.GUID & "|"
                Next
                sqlValues = sqlValues & s & q & compList & q
                compList = ""
                For Each swComp In Me.SwitchExecuteList
                    compList = compList & swComp.switchToControl.GUID & ";" & swComp.ControlReverse.ToString & "|"
                Next
                sqlValues = sqlValues & s & q & compList & q
                compList = ""
                If Not IsNothing(Me.myTacticalRoute) Then
                    For Each rd In Me.myTacticalRoute.myRouteDependencies
                        routeDependentSymbolName = rd.TrnSymbol
                    Next
                End If
                If optEnableARRCSpecificOptions AndAlso gCreateAuthorityTrain IsNot Nothing Then
                    routeDependentSymbolName = gCreateAuthorityTrain.Name
                End If
                myRouteDependentSymbolName = routeDependentSymbolName
                sqlValues = sqlValues & s & q & Me.RouteGUID & q
                sqlValues = sqlValues & s & CInt(Me.AutoRouteGenerated) & s & q & routeDependentSymbolName & q & s & q & Me.SignalRouteGUID & q & s & q & Me.TrainSymbolAssociation & q & s & q & Me.TrainGUID & q
                sqlValues = sqlValues & s & CInt(Me.GroupedStoredRoute)
                If gGropupedStoredRouteParent IsNot Nothing Then
                    sqlValues = sqlValues & s & q & gGropupedStoredRouteParent.RouteGUID & q
                Else
                    sqlValues = sqlValues & s & "''"
                End If
                If Me.SLFATrackList.Count > 0 Then
                    For Each trk As Track In Me.SLFATrackList
                        slfaTrackList += trk.GUID & "|"
                    Next
                    sqlValues = sqlValues & s & q & slfaTrackList & q
                Else
                    sqlValues = sqlValues & s & "''"
                End If
                sqlValues = sqlValues & ")"
                insertCommand = "INSERT Into tblStoreRoutes " & sqlFields & sqlValues
                SystemDatabaseCommandToDynamic("STOREDROUTE:InsertRouteIntoDatabase", insertCommand)
                StoredRouteEventLog("InsertRouteIntoDatabase:Execute")
                logTheEvent = New DoEventLogStoredRoute(AddressOf Me.LogTheStoredRouteEvent)
                logTheEvent.BeginInvoke(EventTypeStoreRoute.CREATE, Nothing, Nothing)
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                swComp = Nothing
                routeDependentSymbolName = Nothing
                rd = Nothing
                logTheEvent = Nothing
                slfaTrackList = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Update display route
        ''' </summary>
        Public Sub UpdateDisplayRoute()
            Dim query As String
            Dim q As String = "'"
            Dim s As String = ", "
            Try
                query = "UPDATE tblStoreRoutes "
                query = query & " SET DisplayRoute = " & CInt(Me.DisplayStoredRoute)
                query = query & " WHERE RouteGUID = " & q & Me.RouteGUID & q
                SystemDatabaseCommandToDynamic("STOREDROUTE:UpdateDisplayRoute", query)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                query = Nothing
                q = Nothing
                s = Nothing
            End Try
        End Sub

        ''' <summary>
        ''' Remove Route From Database
        ''' </summary>
        ''' <returns></returns>
        Private Function RemoveRouteFromDatabase() As Boolean
            Dim q As String = "'"
            Dim s As String = ", "
            Dim query As String
            Try
                query = "DELETE FROM tblStoreRoutes WHERE RouteGUID = " & q & Me.RouteGUID & q
                If gSystemReleaseMode Then
                    ExecuteQuery(query, gConnectionStringDynamic)
                Else
                    SystemDatabaseCommandToDynamic("STOREDROUTE:RemoveRouteFromDatabase", query)
                End If
                StoredRouteEventLog("RemoveRouteFromDatabase:Execute")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                q = Nothing
                s = Nothing
                query = Nothing
            End Try
        End Function

        Public Function TryCheckForValidity() As Boolean

            Dim tmpTrafficDirection As String = ""
            Dim OverrideSigStatus As Boolean = False

            Try
                'VALIDATE THE COMPONENTS TO SEE IF THESE ARE OK TO CLEAR A ROUTE
                If Me.EntrySignal.BlockAbsolute Then Exit Function
                If Me.CtrlPoint.FlashComponent Then Exit Function
                If Me.CtrlPoint.FailState Then Exit Function

                'Check for route dependencies if there is a tactical route associated with storeed route
                If Not IsNothing(Me.myTacticalRoute) Then
                    If Me.myTacticalRoute.myRouteDependencies.Count > 0 Then
                        Dim rd As RouteDependency
                        For Each rd In Me.myTacticalRoute.myRouteDependencies
                            If Not rd.CheckToSatisfyDependency(Me.CtrlPoint) Then
                                Me.StoredRouteEventLog("TryCheckForValidity:NO STORED ROUTE EXECUTION ACCOUNT DEPENDENCY NOT SATISFIED <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "> <" & rd.RouteDepGUID & "> <" & rd.TrnSymbol & "> <" & rd.TrnArrival.ToString & "> <" & rd.Action & ">")
                                Return False
                            End If
                        Next
                    End If
                ElseIf IsNothing(Me.myTacticalRoute) Then
                    If Me.AutoRouteGenerated And Me.myRouteDependentSymbolName <> "" Then
                        Me.StoredRouteEventLog("TryCheckForValidity:NO STORED ROUTE EXECUTION ACCOUNT DEPENDENCY NOT SATISFIED & NO TACTICAL ROUTE ASSOCIATION <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & ">")
                        Return False
                    End If
                End If

                Dim compToCheck As Object
                For Each compToCheck In Me.ComponentSelection
                    Select Case compToCheck.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim trk As Track = CType(compToCheck, Track)
                            tmpTrafficDirection = trk.TrafficDirection
                            Exit For
                    End Select
                Next

                For Each compToCheck In Me.ComponentSelection
                    Select Case compToCheck.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim trk As Track = CType(compToCheck, Track)
                            If Me.EntrySignal.HoldingSignal Then
                                If trk.HoldingSignalSlottingTrack Then
                                    If trk.Occuppied Then
                                        Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK: " & trk.GUID & ":OCCUPIED>")
                                        Return False
                                    End If
                                End If
                            End If

                            If compToCheck.BlockAbsolute Then
                                Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK: " & compToCheck.GUID & ":BLOCKABSOLUTE>")
                                Return False
                            End If
                            If Not trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> "" And compToCheck.TrafficDirection <> Me.TrafficDirection) Then
                                If Not Me.jointAuthorizationStoredRoute Then
                                    Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK: " & compToCheck.GUID & ":SIGNAL STATUS:" & compToCheck.SignalStatOnMe.ToString & "TRACK TRAFFIC DIRECTION:" & compToCheck.TrafficDirection & " SR TRAFFIC DIRECTION:" & Me.TrafficDirection & ">")
                                    Return False
                                End If
                            ElseIf trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop AndAlso Not OverrideSigStatus Then
                                Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK: " & compToCheck.GUID & ":SIGNAL STATUS:" & compToCheck.SignalStatOnMe.ToString & ">")
                                Return False
                            End If

                            If trk.Occuppied AndAlso
                                (Me.TrafficDirection = "RIGHT" AndAlso trk.DisallowRightSignalRequestOnOccupancy OrElse
                                 Me.TrafficDirection = "LEFT" AndAlso trk.DisallowLeftSignalRequestOnOccupancy) Then
                                Dim ConflictDetected As Boolean = True
                                If optEnableLicenseGenerationForPTC AndAlso trk.myTerritory.PTCEnabled Then
                                    If trk.TrackAuthorities.Count > 0 Then
                                        Dim auth As TrackAuthority.TrackAuthority
                                        For Each auth In trk.TrackAuthorities
                                            If auth.TrainSymbol <> Me.TrainSymbolAssociation Then
                                                ConflictDetected = False
                                            End If
                                        Next
                                    End If
                                End If

                                If ConflictDetected Then
                                    Return False
                                End If
                            ElseIf trk.Occuppied AndAlso Me.jointAuthorizationStoredRoute Then
                                Return False
                            End If
                            Dim OverrideDirectionCheck As Boolean
                            If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then OverrideDirectionCheck = True

                            If optEnableTrainDirectionLogicCheckForStoredRoutes AndAlso Not OverrideDirectionCheck Then
                                If trk.TrainIDs.Count > 0 Then
                                    Dim trn As TrainID
                                    For Each trn In trk.TrainIDs
                                        If trn.Name <> Me.TrainSymbolAssociation Then
                                            If trn.Direction <> "NONE" And trn.Direction <> "" Then
                                                Select Case trn.Direction
                                                    Case optDirectionWest, optDirectionSouth
                                                        If Me.TrafficDirection = "RIGHT" Then
                                                            Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRAIN: " & trn.Name & ":DIRECTION:" & trn.Direction & " TRAFFIC DIRECTION:" & Me.TrafficDirection & ">")
                                                            Return False
                                                        End If
                                                    Case optDirectionNorth, optDirectionWest
                                                        If Me.TrafficDirection = "LEFT" Then
                                                            Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRAIN: " & trn.Name & ":DIRECTION:" & trn.Direction & " TRAFFIC DIRECTION:" & Me.TrafficDirection & ">")
                                                            Return False
                                                        End If
                                                End Select
                                            End If
                                        End If
                                    Next
                                End If
                            End If

                            If compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then
                                'Dim trk As Track = CType(compToCheck, Track)
                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                    Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK TRAFFIC DIRECTION DOES NOT MATCH SR DIRCTION:" & compToCheck.GUID.ToString & ">")
                                    Return False
                                End If
                            Else
                                'Dim trk As Track = CType(compToCheck, Track)
                                If Not OverrideDirectionCheck AndAlso trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                    If Not Me.jointAuthorizationStoredRoute Then
                                        Me.StoredRouteEventLog("TryCheckForValidity:STORED ROUTE EXECUTION NOT VALID  <" & Me.EntrySignal.GUID.ToString.PadLeft(6, "0") & "-" & Me.EntrySignal.Name & "><TRACK TRAFFIC DIRECTION DOES NOT MATCH SR DIRCTION:" & compToCheck.GUID.ToString & ">")
                                        Return False
                                    End If
                                End If
                            End If

                            If compToCheck.FailState Or (compToCheck.OStype And compToCheck.Occuppied) Then
                                Return False
                            End If
                            If optEnableLicenseGenerationForPTC Then
                                If trk.TrackAuthorities.Count > 0 Then
                                    Dim AuthCheckFailed As Boolean = True
                                    If Not IsNothing(Me.theTrain) Then
                                        Dim auth As TrackAuthority.TrackAuthority = trk.TrackAuthorities.Item(0)
                                        If auth.IssueTo = theTrain.IdentifingUnit Then
                                            AuthCheckFailed = False
                                        End If
                                    End If
                                    If AuthCheckFailed Then Return False
                                End If
                                If Not IsNothing(theTrain) Then
                                    If Not theTrain.DoCheckForStoreRouteExecutionOrder(Me) Then
                                        Return False
                                    End If
                                End If
                            End If

                            'CATENARY CHECK FOR ENERGIZED CIRCUIT
                            If optEnableTractionPowerManagementSystem Then
                                If trk.EnableTractionPowerManagement AndAlso Not IsNothing(trk.myCatenaryTrack) AndAlso Not trk.myCatenaryTrack.IsEnergized Then
                                    Return False
                                End If
                            End If

                        Case "TMDS.SwitchComponents.Switch"
                            If compToCheck.SignalStatOnMe <> SwitchComponents.Switch.SignalStatus.SignalAtStop _
                            Or compToCheck.FailState _
                            Or compToCheck.Occuppied Then
                                If Not OverrideSigStatus Then
                                    Return False
                                End If
                                'Return False
                            End If
                            If compToCheck.SwitchLockBit Then
                                Return False
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            If (compToCheck.BlockAbsolute _
                            Or compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus _
                            Or compToCheck.LastCtlStatus <> Signal.CtlStatus.RequestedStop _
                            Or compToCheck.FailState) AndAlso Not compToCheck.AutomaticSignal Then
                                If optEnableGeoFenceOperations AndAlso compToCheck.SignalStatus = SignalComponents.Signal.BulbStatus.ClearStatus AndAlso (gCreateLicenseTrain IsNot Nothing AndAlso gCreateLicenseTrain.IsActivePTC) Then
                                    If tmpTrafficDirection = Me.TrafficDirection Then
                                        'SAA NEED TO CHECK WITH MIKE
                                        OverrideSigStatus = True
                                    Else
                                        Return False
                                    End If
                                Else
                                    Return False
                                End If


                            End If

                            Dim sig As Signal = CType(compToCheck, Signal)
                            If sig.JointControlAuthorizationEnabled Then
                                If Not IsNothing(theTrain) Then
                                    'sig.SendJoinAuthorizationRequest(theTrain)
                                End If
                            End If

                        Case "TMDS.TrackOtherComponents.RRXING"
                            If Not compToCheck.ValidateForAllClearRoute Then
                                Return False
                            End If
                    End Select
                Next

                Dim comp As Object
                Dim sr As StoredRoute
                For Each compToCheck In Me.ComponentSelection
                    For Each sr In Me.CtrlPoint.StoredRoutes
                        Dim PerformComponentCheck As Boolean = True
                        If Not IsNothing(sr.myTacticalRoute) Then
                            If sr.myTacticalRoute.myRouteDependencies.Count > 0 Then
                                PerformComponentCheck = False
                            End If
                        End If
                        If Not optEnableAllowStoreRouteExecuteOutOfOrderBasedOnSelection Then
                            If PerformComponentCheck Then
                                For Each comp In sr.ComponentSelection
                                    If comp Is compToCheck Then
                                        Return False
                                    End If
                                Next
                            End If
                        End If
                    Next
                Next
                If theTrain IsNot Nothing AndAlso
                    theTrain.IsActivePTC AndAlso
                    Not IssueLicenseForTrain(True) Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name} Cannot issue license")
                    Return False
                End If
                EntrySignal.ResetSignalCalledOn()
                StoredRouteEventLog($"{GetCurrentMethod.Name} Stored route valid")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Issue License For Train
        ''' </summary>
        ''' <param name="tryExecute">try execute stored route in first run</param>
        ''' <returns><see langword="True"/> License sent for train, <see langword="False"/> otherwise.</returns>
        Private Function IssueLicenseForTrain(Optional tryExecute As Boolean = False) As Boolean
            Try
                If theTrain Is Nothing Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:Train is nothing")
                    Return False
                End If
                If myTacticalRoute Is Nothing Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:Tactical route is nothing")
                    Return False
                End If
                If Not IsStoredRouteValidForTrain() Then Return False
                StoredRouteEventLog($"{GetCurrentMethod.Name}:Execute")
                myTacticalRoute.UpdateRouteStatus("PENDING")
                myTacticalRoute.IsReadyToExecute = True
                If tryExecute Then
                    theTrain.theSignalListForLicense.Add(EntrySignal)
                Else
                    theTrain.PtcGenerateNormalLicense(New ArrayList From {EntrySignal},
                                  False,
                                  False,
                                  GetCurrentMethod.Name)
                    theTrain.SLFAPending = False
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Is Stored Route Valid For Train
        ''' </summary>
        ''' <returns><see langword="True"/> Route valid, <see langword="False"/> otherwise.</returns>
        Private Function IsStoredRouteValidForTrain() As Boolean
            Try
                If theTrain Is Nothing Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:Train is nothing")
                    Return False
                End If
                If (theTrain.thisCurrentLicense IsNot Nothing AndAlso
                    theTrain.thisCurrentLicense.PTCElectronicTransmissionInProgress) OrElse
                    Not theTrain.IsPresenceOnNetwork OrElse
                    theTrain.IsInTrackReleaseMode Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:PTC route pending")
                    Return False
                End If
                If theTrain.StoredRouteCancellationInProgress Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name}:Stored route cancellation in progress")
                    Return False
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Try execute route
        ''' </summary>
        ''' <returns></returns>
        Public Function TryExecuteRoute() As Boolean
            Dim sw As SwitchToExecute
            Try
                If optEnableStoredLicenseForFreeArea AndAlso Me.SLFATrackList.Count > 0 Then
                    StoredRouteEventLog("TryExecuteRoute:SLFA:Exit")
                    Return False
                End If
                If EntrySignal.AutomaticSignal Then
                    StoredRouteEventLog("TryExecuteRoute:Exit due to automatic signal")
                    Return True
                End If
                StoredRouteEventLog("TryExecuteRoute:ATTEMPTING STORED ROUTE EXECUTION")
                For Each sw In Me.SwitchExecuteList
                    If sw.ControlReverse <> sw.switchToControl.ControlledReverse Then
                        If sw.switchToControl.IndicateOnly Then Return False
                        If Not sw.switchToControl.CheckBrotherSwitchesForLock(False) Then
                            StoredRouteEventLog("TryExecuteRoute:ATTEMPTING STORED ROUTE EXECUTION - PREPARING TO LOCK BROTHER SWITCHES FAILED")
                            Return False
                        Else
                            sw.switchToControl.LockBrotherSwitches(True)
                        End If
                        If Not sw.switchToControl.CheckBrotherSwitchesToMove(False) Then 'sw.switchToControl.ControlledReverse) Then
                            Me.StoredRouteEventLog("TryExecuteRoute:ATTEMPTING STORED ROUTE EXECUTION - PREPARING TO MOVE BROTHER SWITCHES FAILED")
                            Me.CtrlPoint.CancelSendCollection()
                            Return False
                        End If
                    Else
                        sw.switchToControl.RouteLock = True
                    End If
                    If sw.switchToControl.BlockAbsolute Then
                        If sw.ControlReverse <> sw.switchToControl.ControlledReverse Then
                            Return False
                        End If
                    End If
                Next
                StoredRouteEventLog("TryExecuteRoute:ATTEMPTING STORED ROUTE EXECUTION - PREPARING TO MOVE APPPLICABLE SWITCHES")
                For Each sw In Me.SwitchExecuteList
                    sw.switchToControl.ControlSwitch(sw.ControlReverse, True)
                    If sw.switchToControl.AllowIndependentControlSwitch Then
                        If sw.switchToControl.CheckBrotherICSSwitchesToMove(False) Then
                            sw.switchToControl.ControlBrotherSwitches(0, sw.ControlReverse, True)
                        End If
                    Else
                        sw.switchToControl.ControlBrotherSwitches(0, sw.ControlReverse, True)
                    End If
                    If sw.switchToControl.SimulateMode Then
                        sw.switchToControl.DoSimulate = False
                        sw.switchToControl.IndSimulator.Enabled = True
                        sw.switchToControl.LastControl = sw.switchToControl.ControlledReverse
                        For Each swbro As Switch In sw.switchToControl.BrotherSwitches
                            swbro.DoSimulate = False
                            swbro.IndSimulator.Enabled = True
                            swbro.LastControl = sw.switchToControl.ControlledReverse
                        Next
                    End If
                Next
                StoredRouteEventLog("TryExecuteRoute:STORED ROUTE EXECUTING AT " & Me.CtrlPoint.Name & " FROM ENTRY SIGNAL:" & Me.EntrySignal.Name & " TO EXIT SIGNAL:" & Me.ExitSignal.Name)
                If Not Me.EntrySignal.ControlSignalToClear(True, True, True, False, True, False, False, False, False, False, True) Then
                    Me.CtrlPoint.CancelSendCollection()
                    For Each sw In Me.SwitchExecuteList
                        sw.switchToControl.LockBrotherSwitches(False)
                        sw.switchToControl.RouteLock = False
                    Next
                    StoredRouteEventLog("TryExecuteRoute:STORED ROUTE CANCELED")
                    If Me.EntrySignal.AutomaticModeOn Then
                        SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                        If Not IsNothing(Me.myTacticalRoute.myTrain) Then
                            Me.myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                        End If
                    End If
                    Return False
                Else
                    For Each sw In Me.SwitchExecuteList
                        sw.switchToControl.LockBrotherSwitches(False)
                        sw.switchToControl.RouteLock = False
                    Next
                End If
                If Me.EntrySignal.AutomaticModeOn Then
                    Me.EntrySignal.CurrentTacticalRoute = Me.myTacticalRoute
                    Me.EntrySignal.ControlledByTacticalRoute = True
                    SendRouteExecuteConfirmationMessage("PENDING", "UPDATE")
                End If
                If Me.TrainSymbolAssociation <> "" Then
                    Me.CheckTpUpdateOtherStoreRoutesWithDependencies()
                End If
                StoredRouteEventLog("TryExecuteRoute:STORED ROUTE EXECUTION COMPLETE:EntrySignal=" & Me.EntrySignal.Name & ":Priority=" & Me.Priority)
                Return True
            Catch ex As Exception
                GetErrorDetails(ex, "TryExecuteRoute")
            Finally
                sw = Nothing
            End Try
        End Function

        ''' <summary>
        ''' Check SLFA track list for execution
        ''' </summary>
        ''' <returns></returns>
        Public Function CheckSLFATrackList() As Boolean
            Try
                If Not IsNothing(Me.theTrain.SLFATimer2) AndAlso Me.theTrain.SLFATimer2.IsRunning = True Then

                    For Each trk As Track In SLFATrackList
                        If Not trk.Occuppied Then
                            Me.EntrySignal.SLFAEntrySignal = True
                            Me.ExitSignal.SLFAExitSignal = True
                            Me.EntrySignal.InvalidateSignal()
                            Me.ExitSignal.InvalidateSignal()
                        End If
                    Next

                    Return True
                ElseIf Not IsNothing(Me.theTrain.SLFAOccupation) AndAlso Me.theTrain.SLFAOccupation = True Then
                    For Each trk As Track In SLFATrackList
                        If trk.Occuppied Then
                            Me.theTrain.SLFAPending = True
                            Exit For
                        End If
                        Me.theTrain.SLFAPending = False
                    Next
                    UpdateSignalDisplays()
                    If Me.theTrain.SLFAPending = True Then
                        Return True
                    End If
                ElseIf Not IsNothing(Me.theTrain.SLFATimer2) AndAlso Me.theTrain.SLFATimer2.IsRunning = False AndAlso Me.theTrain.SLFAExecute = True Then
                    For Each trk As Track In SLFATrackList
                        If trk.Occuppied Then
                            Me.theTrain.SLFAPending = True
                            Exit For
                        End If
                        Me.theTrain.SLFAPending = False
                    Next
                    UpdateSignalDisplays()
                Else
                    For Each trk As Track In SLFATrackList
                        If trk.Occuppied Then
                            Return True
                        End If
                    Next
                End If
                Return False
            Catch ex As Exception
                GetErrorDetails(ex, "CheckSLFATrackList")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Check SR for validity
        ''' </summary>
        ''' <param name="pCheckForTags"></param>
        ''' <returns></returns>
        Public Function CheckForValidity(Optional pCheckForTags As Boolean = True) As Boolean
            Dim tmpTrafficDirection As String = ""
            Dim OverrideSigStatus As Boolean = False
            Dim ThisSRText As String = " < This RouteGUID: " & Me.RouteGUID & " Entry Signal Name: " & Me.EntrySignal.Name & ">"
            Try
                If Me.EntrySignal.BlockAbsolute Then Exit Function
                If Me.CtrlPoint.FlashComponent Then Exit Function
                If Me.CtrlPoint.FailState Then Exit Function
                If optEnableStoredLicenseForFreeArea AndAlso
                    Me.theTrain IsNot Nothing AndAlso
                    CheckSLFATrackList() Then
                    Exit Function
                ElseIf optEnableStoredLicenseForFreeArea AndAlso
                    Me.theTrain IsNot Nothing AndAlso
                    SLFATrackList.Count > 0 AndAlso
                    Not CheckSLFATrackList() Then
                    StoredRouteEventLog("CheckForValidity:SLFA:Execute")
                End If
                Dim compToCheck As Object
                'Check to see if any SR in higher proirity has identical components in route
                'If Me.Priority <> 1 Then
                If Me.Priority <> 0 Then
                    Dim sr As StoredRoute
                    For Each sr In Me.CtrlPoint.StoredRoutes
                        If sr.Priority < Me.Priority Then
                            Dim PerformComponentCheck As Boolean = True
                            If Not IsNothing(sr.myTacticalRoute) Then
                                If sr.myTacticalRoute.myRouteDependencies.Count > 0 Then
                                    PerformComponentCheck = False
                                End If
                            End If
                            If PerformComponentCheck Then
                                Dim comp As Object
                                For Each comp In Me.ComponentSelection
                                    Dim compInRoute As Object
                                    For Each compInRoute In sr.ComponentSelection
                                        If compInRoute Is comp Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_001:" & ThisSRText)
                                            SystemDiagnosticEvent("Another SR has proirity <" & sr.Priority & " - SR GUID: " & sr.RouteGUID & " Entry Signal Name: " & sr.EntrySignal.Name & " >" & ThisSRText)
                                            Return False
                                        End If
                                    Next
                                Next
                            End If
                        Else
                            Exit For
                        End If
                    Next
                End If

                'CHECK TO SEE IF THERE ARE ANY PARALELL BROTHER SIGNALS REQUESTED OR CLEARED. 
                'REFERENCE THE CONFIGURATION AT LOWELL ON SCENIC2 ON THE BNSF WHERE THE SIDING SIGNAL WILL CLEAR OVER THE TOP OF A MAIN LINE SIGNAL.
                If Not IsNothing(Me.EntrySignal.BrotherSignalCollection) Then
                    Dim sig As Signal
                    For Each sig In Me.EntrySignal.BrotherSignalCollection
                        If sig.SignalCtlStatus <> Signal.CtlStatus.RequestedStop Then
                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_002:" & ThisSRText)
                            SystemDiagnosticEvent("Signal Not Stopped <" & sig.Name & " - Sig GUID: " & sig.GUID & " >" & ThisSRText)
                            Return False
                        End If
                    Next
                End If

                If IsNothing(Me.ComponentSelection) Then
                    Me.RemoveMe("CheckForValidity")
                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_003:" & ThisSRText)
                    Exit Function
                End If

                Dim HasFlipFlop As Boolean
                If optEnableARRCSpecificOptions Then
                    For Each compToCheck In Me.ComponentSelection
                        If compToCheck.GetType.ToString = gFlipFlopComponentName Then
                            If HasFlipFlop Then
                                HasFlipFlop = False
                            Else
                                HasFlipFlop = True
                            End If
                        End If
                    Next
                    Dim NextTrack As Object = Nothing
                    If HasFlipFlop Then
                        Select Case Me.TrafficDirection
                            Case "RIGHT"
                                NextTrack = FindCompFromGUID(Me.ExitSignal.LeftLink)
                            Case Else
                                NextTrack = FindCompFromGUID(Me.ExitSignal.RightLink)
                        End Select
                    Else
                        Select Case Me.TrafficDirection
                            Case "RIGHT"
                                NextTrack = FindCompFromGUID(Me.ExitSignal.RightLink)
                            Case Else
                                NextTrack = FindCompFromGUID(Me.ExitSignal.LeftLink)
                        End Select
                    End If

                    If NextTrack IsNot Nothing AndAlso NextTrack.GetType.ToString = gTrackComponentName AndAlso NextTrack.AuthorityType.Contains("TWC") Then
                        If NextTrack.TrackAuthorities IsNot Nothing AndAlso NextTrack.TrackAuthorities.Count > 0 Then
                            Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
                            For Each SFA In NextTrack.TrackAuthorities
                                Select Case SFA.AuthType1
                                    Case gSFAAuthorityTypeItem4, gSFAAuthorityTypeItem4MOW
                                    Case gSFAAuthorityTypeItem2
                                        Dim AuthDirection As String = SFA.Direction.ToUpper.Trim
                                        If HasFlipFlop Then
                                            Select Case AuthDirection
                                                Case "LEFT"
                                                    AuthDirection = "RIGHT"
                                                Case Else
                                                    AuthDirection = "LEFT"
                                            End Select
                                        End If
                                        If AuthDirection.ToUpper.Trim <> Me.TrafficDirection.ToUpper.Trim Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_004:" & ThisSRText)
                                            SystemDiagnosticEvent("Authority Direction and Traffic Direction does not match " & ThisSRText)
                                            Return False
                                        End If
                                End Select
                            Next
                        End If
                    End If
                End If

                For Each compToCheck In Me.ComponentSelection
                    Select Case compToCheck.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim trk As Track = CType(compToCheck, Track)
                            tmpTrafficDirection = trk.TrafficDirection
                            Exit For
                    End Select
                Next

                'AuthDirection

                For Each compToCheck In Me.ComponentSelection
                    Select Case compToCheck.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim OkToProceed As Boolean = True
                            Dim trk As Track = CType(compToCheck, Track)

                            If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.Contains("TWC") Then
                                If trk.TrackAuthorities IsNot Nothing AndAlso trk.TrackAuthorities.Count > 0 Then
                                    Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
                                    For Each SFA In trk.TrackAuthorities
                                        Select Case SFA.AuthType1
                                            Case gSFAAuthorityTypeItem4, gSFAAuthorityTypeItem4MOW
                                                OkToProceed = False
                                            Case gSFAAuthorityTypeItem2
                                                Dim AuthDirection As String = SFA.Direction.ToUpper.Trim
                                                If HasFlipFlop Then
                                                    Select Case AuthDirection
                                                        Case "LEFT"
                                                            AuthDirection = "RIGHT"
                                                        Case Else
                                                            AuthDirection = "LEFT"
                                                    End Select
                                                End If


                                                If AuthDirection.ToUpper.Trim = Me.TrafficDirection.ToUpper.Trim Then
                                                    OkToProceed = False
                                                End If
                                        End Select
                                    Next
                                Else
                                    OkToProceed = False
                                End If

                            End If


                            If OkToProceed Then
                                If Me.EntrySignal.HoldingSignal Then
                                    If trk.HoldingSignalSlottingTrack Then
                                        If trk.Occuppied Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_005:" & ThisSRText)
                                            SystemDiagnosticEvent("Track Occupied <" & trk.Name & " - trk GUID: " & trk.GUID & " >" & ThisSRText)
                                            Return False
                                        End If
                                    End If
                                End If
                                If compToCheck.BlockAbsolute Then
                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_006:" & ThisSRText)
                                    Return False
                                End If
                                If Not compToCheck.IsRemoteTrack Then
                                    If compToCheck.myControlPoint.PendingStatusChange Then
                                        Return False
                                    End If
                                End If

                                Dim FlipDirection As Boolean
                                If Not IsNothing(trk.myTerritory) Then
                                    If trk.myTerritory.FlipDirection Then
                                        FlipDirection = True
                                    Else
                                        FlipDirection = False
                                    End If
                                End If

                                If trk.Occuppied AndAlso
                                (Me.TrafficDirection = "RIGHT" AndAlso trk.DisallowRightSignalRequestOnOccupancy OrElse
                                 Me.TrafficDirection = "LEFT" AndAlso trk.DisallowLeftSignalRequestOnOccupancy) Then
                                    If IsNothing(Me.theTrain) Then
                                        If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_007:" & ThisSRText)
                                        Return False
                                    ElseIf Not CheckToSeeIfTrainInFrontOfStoredRoute(trk) Then
                                        If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_008:" & ThisSRText)
                                        '  If Me.theTrain.MZEnabled orelse  Then
                                        If Me.theTrain.RequestJointAuthForLicnese Then
                                            Return False
                                        Else
                                            Return False
                                        End If

                                    End If
                                End If


                                If optEnableTrainDirectionLogicCheckForStoredRoutes Then
                                    Dim OverrideDirectionCheck As Boolean = False

                                    If Not OverrideDirectionCheck AndAlso trk.TrainIDs.Count > 0 Then
                                        Dim trn As TrainID
                                        For Each trn In trk.TrainIDs
                                            If Not IsNothing(trn) Then
                                                If trn.Direction <> "NONE" And trn.Direction <> "" Then
                                                    Select Case trn.Direction
                                                        Case optDirectionWest, optDirectionSouth
                                                            If FlipDirection Then
                                                                If Me.TrafficDirection = "LEFT" Then
                                                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_009" & ThisSRText)
                                                                    Return False
                                                                End If
                                                            Else
                                                                If Me.TrafficDirection = "RIGHT" Then
                                                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_010" & ThisSRText)
                                                                    Return False
                                                                End If
                                                            End If
                                                        Case optDirectionNorth, optDirectionWest
                                                            If FlipDirection Then
                                                                If Me.TrafficDirection = "RIGHT" Then
                                                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_011" & ThisSRText)
                                                                    Return False
                                                                End If
                                                            Else
                                                                If Me.TrafficDirection = "LEFT" Then
                                                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_012" & ThisSRText)
                                                                    Return False
                                                                End If
                                                            End If
                                                    End Select
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                                If Not trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> "" And compToCheck.TrafficDirection <> Me.TrafficDirection) Then
                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_013:" & ThisSRText)
                                    Return False
                                ElseIf trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop AndAlso Not OverrideSigStatus Then
                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_014:" & ThisSRText)
                                    Return False
                                End If

                                If compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then

                                    If trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                        If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_015:" & ThisSRText)
                                        Return False
                                    End If
                                Else
                                    If trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                        Dim OverRideTrafficCheck As Boolean
                                        If optEnableGeoFenceOperations Then
                                            OverRideTrafficCheck = Me.EntrySignal.DoLicenseCheckForRearOfTrain(trk, Nothing)
                                        End If
                                        If optEnableGeoFenceOperations AndAlso Not OverRideTrafficCheck Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_016:" & ThisSRText)
                                            Return False
                                        End If
                                    End If
                                End If


                                If compToCheck.FailState Or (compToCheck.OStype And compToCheck.Occuppied) Then
                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_017:" & ThisSRText)
                                    Return False
                                End If

                                If optEnableLicenseGenerationForPTC Then
                                    Dim num As Int32 = trk.GUID

                                    If trk.TrackAuthorities.Count > 0 Then
                                        Dim AuthCheckFailed As Boolean = True
                                        Dim OverRideOverlapCheck As Boolean = False

                                        If Not IsNothing(Me.theTrain) Then
                                            Dim auth As TrackAuthority.TrackAuthority = trk.TrackAuthorities.Item(0)
                                            If auth.IssueTo = theTrain.IdentifingUnit Then
                                                AuthCheckFailed = False
                                            End If
                                            If optEnableGeoFenceOperations Then
                                                OverRideOverlapCheck = Me.EntrySignal.DoLicenseCheckForRearOfTrain(trk, Nothing)
                                            End If
                                        End If
                                        If AuthCheckFailed AndAlso Not OverRideOverlapCheck Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_018:" & ThisSRText)
                                            Return False
                                        End If

                                    End If

                                    If Not IsNothing(theTrain) Then
                                        If Not theTrain.DoCheckForStoreRouteExecutionOrder(Me) Then
                                            If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_019:" & ThisSRText)
                                            Return False
                                        End If
                                    End If
                                End If

                                'CATENARY CHECK FOR ENERGIZED CIRCUIT
                                If optEnableTractionPowerManagementSystem Then
                                    If trk.EnableTractionPowerManagement AndAlso Not IsNothing(trk.myCatenaryTrack) AndAlso Not trk.myCatenaryTrack.IsEnergized Then
                                        If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_020:" & ThisSRText)
                                        Return False
                                    End If
                                End If

                            End If
                        Case "TMDS.SwitchComponents.Switch"

                            If compToCheck.TypeOfSwitch = "SPM ONLY" Then
                            Else
                                If compToCheck.SignalStatOnMe <> SwitchComponents.Switch.SignalStatus.SignalAtStop _
                                Or compToCheck.FailState _
                                Or compToCheck.Occuppied Then
                                    If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_021:" & ThisSRText)
                                    If Not OverrideSigStatus Then
                                        Return False
                                    End If
                                    'Return False
                                End If
                                Dim sw As SwitchToExecute
                                For Each sw In Me.SwitchExecuteList
                                    If sw.switchToControl Is compToCheck Then
                                        If sw.switchToControl.BlockAbsolute Or sw.switchToControl.SwitchLockBit Then
                                            If sw.ControlReverse <> sw.switchToControl.ControlledReverse Then
                                                If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_022:" & ThisSRText)

                                                Return False
                                            End If
                                        End If
                                        Exit For
                                    End If
                                Next
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(compToCheck, Signal)
                            If (Me.TrafficDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And compToCheck.BlockAbsolute Then
                                ClearSystemVariables()
                                If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_023:" & ThisSRText)
                                Return False
                            End If
                            If (gSearchDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And compToCheck.BlockAbsolute Then
                                ClearSystemVariables()
                                If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_024:" & ThisSRText)
                                Return False
                            End If

                            If (compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus _
                            Or compToCheck.LastCtlStatus <> Signal.CtlStatus.RequestedStop _
                            Or compToCheck.FailState) AndAlso Not sig.AutomaticSignal Then
                                If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_025:" & ThisSRText)
                                If optEnableGeoFenceOperations AndAlso compToCheck.SignalStatus = SignalComponents.Signal.BulbStatus.ClearStatus AndAlso Me.theTrain IsNot Nothing AndAlso Me.theTrain.IsActivePTC Then
                                    If tmpTrafficDirection = Me.TrafficDirection Then
                                        OverrideSigStatus = True
                                    Else
                                        Return False
                                    End If
                                Else
                                    Return False
                                End If
                                'Return False
                            End If
                        Case "TMDS.TrackOtherComponents.RRXING"
                            If Not compToCheck.ValidateForAllClearRoute Then
                                If optEnableStoredRouteDebug Then SystemDiagnosticEvent("SR_Code_026:" & ThisSRText)
                                Return False
                            End If

                    End Select
                Next
                If theTrain IsNot Nothing AndAlso
                    Not IsStoredRouteValidForTrain() Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name} Stored route pending")
                    Return False
                End If
                If Not ExecuteStoredRoute(pCheckForTags) Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name} Cannot execute stored route")
                    Return False
                End If
                If theTrain IsNot Nothing AndAlso
                    theTrain.IsActivePTC AndAlso
                    Not IssueLicenseForTrain() Then
                    StoredRouteEventLog($"{GetCurrentMethod.Name} Cannot issue license")
                    Return False
                End If
                EntrySignal.ResetSignalCalledOn()
                StoredRouteEventLog($"{GetCurrentMethod.Name} Stored route valid")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function ExecuteStoredRoute(Optional CheckForTags As Boolean = True) As Boolean
            Try
                StoredRouteEventLog($"{GetCurrentMethod.Name}:WhoCallMe:{(New StackTrace).GetFrame(1).GetMethod.Name}:Starting Stored Route Execution")
                If EntrySignal.AutomaticSignal Then
                    If Not IsNothing(theTrain) Then
                        TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(Me, "EXECUTED")
                        If theTrain.RequestJointAuthForLicnese Then
                            If theTrain.WaitingForJointAuthResponse Then
                                theTrain.SignalListForJointAuthRouteBuilding.Add(EntrySignal)
                                RemoveMe()
                                Return False
                            End If
                        End If
                    End If
                    Return True
                End If
                If theTrain IsNot Nothing Then
                    If EntrySignal.JointControlAuthorizationEnabled Then
                        If theTrain.RequestJointAuthForLicnese Then
                            If theTrain.WaitingForJointAuthResponse Then
                                Return False
                            End If
                        End If
                    End If
                End If
                If myTacticalRoute IsNot Nothing Then
                    If myTacticalRoute.myRouteDependencies.Count > 0 Then
                        Dim rd As RouteDependency
                        For Each rd In myTacticalRoute.myRouteDependencies
                            If Not rd.CheckToSatisfyDependency(CtrlPoint) Then
                                StoredRouteEventLog($"NO STORED ROUTE EXECUTION ACCOUNT DEPENDENCY NOT SATISFIED <{EntrySignal.GUID.ToString.PadLeft(6, "0")}-{EntrySignal.Name}> <{rd.RouteDepGUID}> <{rd.TrnSymbol}> <{rd.TrnArrival.ToString}> <{rd.Action}>")
                                Return False
                            End If
                        Next
                    End If
                ElseIf IsNothing(myTacticalRoute) Then
                    If AutoRouteGenerated AndAlso String.IsNullOrWhiteSpace(myRouteDependentSymbolName) Then
                        StoredRouteEventLog($"NO STORED ROUTE EXECUTION ACCOUNT DEPENDENCY NOT SATISFIED & NO TACTICAL ROUTE ASSOCIATION<{EntrySignal.GUID.ToString.PadLeft(6, "0")}-{EntrySignal.Name}>")
                        Return False
                    End If
                End If
                Dim sw As SwitchToExecute
                For Each sw In SwitchExecuteList
                    If Not sw.ControlReverse = sw.switchToControl.ControlledReverse Then
                        If optEnableMetroLinkSpecificOptions AndAlso sw.switchToControl.TypeOfSwitch.Equals("DERAIL") AndAlso sw.switchToControl.IndicateOnly Then
                        Else
                            If sw.switchToControl.IndicateOnly Then Return False
                        End If
                        If Not sw.switchToControl.CheckBrotherSwitchesForLock(False) Then
                            Return False
                        Else
                            sw.switchToControl.LockBrotherSwitches(True)
                        End If
                        If Not sw.switchToControl.CheckBrotherSwitchesToMove(False) Then
                            Me.CtrlPoint.CancelSendCollection()
                            Me.RemoveMe("ExecuteStoredRoute1")
                            Return False
                        End If
                    Else
                        sw.switchToControl.RouteLock = True
                    End If
                    If sw.switchToControl.BlockAbsolute Then
                        If Not sw.ControlReverse = sw.switchToControl.ControlledReverse Then
                            Return False
                        End If
                    End If
                Next
                For Each sw In SwitchExecuteList
                    sw.switchToControl.ControlSwitch(sw.ControlReverse, True)
                    If sw.switchToControl.AllowIndependentControlSwitch Then
                        If sw.switchToControl.CheckBrotherICSSwitchesToMove(False) Then
                            sw.switchToControl.ControlBrotherSwitches(0, sw.ControlReverse, True)
                        End If
                    ElseIf optEnableMetroLinkSpecificOptions AndAlso sw.switchToControl.TypeOfSwitch.Equals("DERAIL") AndAlso sw.switchToControl.IndicateOnly Then
                    Else
                        sw.switchToControl.ControlBrotherSwitches(0, sw.ControlReverse, True)
                    End If
                    If sw.switchToControl.SimulateMode Then
                        sw.switchToControl.DoSimulate = False
                        sw.switchToControl.IndSimulator.Enabled = True
                        sw.switchToControl.LastControl = sw.switchToControl.ControlledReverse
                        Dim swbro As Switch
                        For Each swbro In sw.switchToControl.BrotherSwitches
                            swbro.DoSimulate = False
                            swbro.IndSimulator.Enabled = True
                            swbro.LastControl = sw.switchToControl.ControlledReverse
                        Next
                    End If
                Next
                If Me.jointAuthorizationStoredRoute Then
                    Dim comp As Object
                    For Each comp In Me.ComponentSelection
                        If comp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = CType(comp, Track)
                            If trk.Occuppied Then
                                Return False
                            End If
                        End If
                    Next
                End If
                If theTrain IsNot Nothing Then
                    StoredRouteEventLog($"EXECUTING STORED ROUTE TO TRAIN  {theTrain.Name} AT {CtrlPoint.Name} FROM ENTRY SIGNAL: {EntrySignal.Name} TO EXIT SIGNAL:{ExitSignal.Name}")
                Else
                    StoredRouteEventLog($"EXECUTING STORED ROUTE AT {CtrlPoint.Name} FROM ENTRY SIGNAL: {EntrySignal.Name} TO EXIT SIGNAL: {ExitSignal.Name}")
                End If
                If EntrySignal.JointControlAuthorizationEnabled Then
                    If Not EntrySignal.ControlSignalToClear(True, CheckForTags, True, False, True, False, False, False, False, False, True, False, theTrain) Then
                        StoredRouteEventLog("STORED ROUTE TO JOIN AUTHORIZATION")
                        EntrySignal.SendJoinAuthorizationRequest(myTacticalRoute.myTrain)
                        myTacticalRoute.myTrain.RequestSentForJointViaStoredRoute = True
                        myTacticalRoute.myTrain.WaitingForJointAuthResponse = True
                        If IsNothing(EntrySignal.gJointLicenseForTrainTrain) Then
                            If Not IsNothing(myTacticalRoute.myTrain) Then
                                EntrySignal.gJointLicenseForTrainTrain = myTacticalRoute.myTrain
                                EntrySignal.gJointLicenseForTrainTrain.RequestJointAuthForLicnese = True
                                EntrySignal.SendJoinAuthorizationRequest(myTacticalRoute.myTrain)
                                StoredRouteEventLog("gJointLicenseForTrainTrain:Train=" & myTacticalRoute.myTrain.Name)
                            End If
                        Else
                            EntrySignal.gJointLicenseForTrainTrain.RequestJointAuthForLicnese = True
                        End If
                        Return False
                    End If
                ElseIf Not EntrySignal.ControlSignalToClear(True, CheckForTags, True, False, True, False, False, False, False, False, True, False, theTrain) Then
                    CtrlPoint.CancelSendCollection()
                    For Each sw In SwitchExecuteList
                        sw.switchToControl.LockBrotherSwitches(False)
                        sw.switchToControl.RouteLock = False
                    Next
                    StoredRouteEventLog("STORED ROUTE CANCELED:" & RouteGUID)
                    If EntrySignal.AutomaticModeOn Then
                        SendRouteExecuteErrorMessage("ERROR", "UPDATE")
                        If myTacticalRoute.myTrain IsNot Nothing Then
                            myTacticalRoute.myTrain.ToggleAutoRouting(False, "SYSTEM COMMAND-STORED ROUTE FAILED")
                        End If
                    End If
                    Return False
                Else
                    For Each sw In SwitchExecuteList
                        sw.switchToControl.LockBrotherSwitches(False)
                        sw.switchToControl.RouteLock = False
                    Next
                End If
                StoredRouteEventLog("EXECUTION OF STORED ROUTE PASSED SIGNAL CHECK")
                If Not (gAssistMode OrElse EntrySignal.IsShadowMode) Then
                    Dim PublishEvent As New DoPublishControlPointEvent(AddressOf CtrlPoint.PublishCPEventUpdate)
                    PublishEvent.BeginInvoke(ControlPoint.EventTypeCP.SROUTEOFF, Me, Nothing, Nothing)
                End If
                RouteExecuted = True
                UpdateSignalDisplays()
                StoredRouteEventLog("STORED ROUTE-UpdateSignalDisplays")
                SyncLock CtrlPoint.StoredRoutes
                    CtrlPoint.StoredRoutes.Remove(Me)
                End SyncLock
                If CtrlPoint.StoredRoutes.Count > 0 Then
                    Dim sr As StoredRoute = CtrlPoint.StoredRoutes(0)
                    sr.UpdateSignalDisplays()
                End If
                CtrlPoint.ReorderStoredRoutes()
                If RouteExecuted Then
                    If RemoveRouteFromDatabase() Then
                    End If
                End If
                If Not String.IsNullOrWhiteSpace(TrainSymbolAssociation) Then
                    CheckTpUpdateOtherStoreRoutesWithDependencies()
                End If
                StoredRouteEventLog($"STORED ROUTE EXECUTION COMPLETE:EntrySignal= {EntrySignal.Name}:Priority={Priority}")
                TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(Me, "EXECUTED")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Sub UpdateSignalDisplays()
            Try
                If Me.AutoRouteGenerated Then
                    StoredRouteEventLog("UpdateSignalDisplays")
                End If
                If Me.RouteExecuted Then
                    Me.EntrySignal.StoredEntrySignal = False
                    Me.ExitSignal.StoredExitSignal = False
                    Me.EntrySignal.SLFAEntrySignal = False
                    Me.ExitSignal.SLFAExitSignal = False
                ElseIf Me.AutoRouteGenerated And Not DisplayStoredRoute And Me.myRouteDependentSymbolName <> "" Then
                    Exit Sub
                ElseIf Me.SLFATrackList.Count > 0 AndAlso Me.theTrain.SLFATimer2.IsRunning = False AndAlso Me.theTrain.SLFAPending = True Then
                    If Not IsNothing(Me.theTrain.SLFAOccupation) AndAlso Me.theTrain.SLFAOccupation = True Then
                        Me.EntrySignal.SLFAEntrySignal = True
                        Me.ExitSignal.SLFAExitSignal = True
                        Me.EntrySignal.StoredEntrySignal = False
                        Me.ExitSignal.StoredExitSignal = False
                    Else
                        Me.EntrySignal.SLFAEntrySignal = False
                        Me.ExitSignal.SLFAExitSignal = False
                        Me.EntrySignal.StoredEntrySignal = True
                        Me.ExitSignal.StoredExitSignal = True
                    End If
                ElseIf Me.SLFATrackList.Count > 0 Then
                    Me.EntrySignal.StoredEntrySignal = False
                    Me.ExitSignal.StoredExitSignal = False
                    Me.EntrySignal.SLFAEntrySignal = True
                    Me.ExitSignal.SLFAExitSignal = True
                Else
                    Me.EntrySignal.SLFAEntrySignal = False
                    Me.ExitSignal.SLFAExitSignal = False
                    Me.EntrySignal.StoredEntrySignal = True
                    Me.ExitSignal.StoredExitSignal = True
                End If
                Me.EntrySignal.InvalidateSignal()
                Me.ExitSignal.InvalidateSignal()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckTpUpdateOtherStoreRoutesWithDependencies()
            Try
                Dim sr As StoredRoute
                If Me.CtrlPoint.StoredRoutes.Count > 1 Then
                    For Each sr In Me.CtrlPoint.StoredRoutes
                        If sr.AutoRouteGenerated And Not sr.DisplayStoredRoute Then
                            If sr.myRouteDependentSymbolName = Me.TrainSymbolAssociation Then
                                sr.DisplayStoredRoute = True
                                sr.UpdateDisplayRoute()
                            End If
                        End If
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckToDisplayStoredRouteDependency()
            Try
                Me.StoredRouteEventLog("CheckToDisplayStoredRouteDependency1: <" & Me.RouteGUID & "><" & Me.EntrySignal.myControlPoint.Name & "><" & Me.EntrySignal.Name & "> TO <" & Me.ExitSignal.Name & "><" & Me.myRouteDependentSymbolName & "><" & DisplayStoredRoute.ToString.ToUpper & "><" & AutoRouteGenerated.ToString.ToUpper & ">")

                'If I contain a Tactical route
                If Not IsNothing(Me.myTacticalRoute) Then
                    'If I have dependencies
                    If Me.myTacticalRoute.myRouteDependencies.Count > 0 Then
                        'if my control point has stored routes
                        If Me.CtrlPoint.StoredRoutes.Count > 0 Then
                            'The first stored route = my control points 1st stored route
                            Dim FirstStoredRoute As StoredRoute = Me.CtrlPoint.StoredRoutes(0)
                            'if the FirstStoredRoute has the same GUID as my RouteGUID
                            If FirstStoredRoute.RouteGUID = Me.RouteGUID Then
                                'If Me.CtrlPoint.StoredRoutes.Count = 1 Then
                                'If the train from my tactical route has passed the previous Control Point
                                If Not IsNothing(Me.myTacticalRoute.myTrain.OSPreviousControlPoint) Then
                                    'if the train from my tactical routes previous control point GUID = my stored route control point GUID
                                    If Me.myTacticalRoute.myTrain.OSPreviousControlPoint.GUID = Me.CtrlPoint.GUID Then
                                        'show the stored route
                                        Me.DisplayStoredRoute = True
                                        Me.UpdateDisplayRoute()
                                        Me.StoredRouteEventLog("CheckToDisplayStoredRouteDependency2: <" & Me.RouteGUID & "><" & Me.EntrySignal.myControlPoint.Name & "><" & Me.EntrySignal.Name & "> TO <" & Me.ExitSignal.Name & ">")
                                        Exit Sub
                                    End If
                                End If

                                'ARS-141 - Flag remains when dependency changed if a train is in the OS - This isn't checking the actual symbol (RPBJR)
                                Dim comp As Object
                                For Each comp In Me.ComponentSelection
                                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                        Dim tk As Track = CType(comp, Track)
                                        Dim trainSymbolList As New List(Of String)

                                        'Create a temporary guid list
                                        For Each train As TrainID In tk.TrainIDs

                                            trainSymbolList.Add(train.Name)

                                        Next

                                        'Now checks the train symbols in the component selection - added 'And (trainSymbolList.Contains(Me.myRouteDependentSymbolName))'
                                        If tk.OStype And tk.Occuppied And (trainSymbolList.Contains(Me.myRouteDependentSymbolName)) Then
                                            Me.DisplayStoredRoute = True
                                            Me.UpdateDisplayRoute()
                                            Me.StoredRouteEventLog("CheckToDisplayStoredRouteDependency3: <" & Me.RouteGUID & "><" & Me.EntrySignal.myControlPoint.Name & "><" & Me.EntrySignal.Name & "> TO <" & Me.ExitSignal.Name & ">")
                                            Exit Sub
                                        End If
                                    End If
                                Next
                                'END ARS-141

                                For Each comp In Me.CtrlPoint.AssignedComponenets
                                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                        Dim sig As Signal = CType(comp, Signal)
                                        If (sig.SignalCtlStatus = Signal.CtlStatus.RequestedClear Or sig.SignalCtlStatus = Signal.CtlStatus.RequestingClear Or sig.SignalIndStatus = Signal.IndStatus.Cleared) And Me.Priority = 0 Then
                                            If IsNothing(sig.CurrentTacticalRoute) Then
                                                Me.DisplayStoredRoute = True
                                                Me.UpdateDisplayRoute()
                                                Me.StoredRouteEventLog("CheckToDisplayStoredRouteDependency4: <" & Me.RouteGUID & "><" & Me.EntrySignal.myControlPoint.Name & "><" & Me.EntrySignal.Name & "> TO <" & Me.ExitSignal.Name & ">")
                                            ElseIf Not IsNothing(sig.CurrentTacticalRoute) Then
                                                If Me.myTacticalRoute.DoesItContainTrainDependency(sig.CurrentTacticalRoute.myTrain.Name) Then
                                                    Me.DisplayStoredRoute = True
                                                    Me.UpdateDisplayRoute()
                                                    Me.StoredRouteEventLog("CheckToDisplayStoredRouteDependency5: <" & Me.RouteGUID & "><" & Me.EntrySignal.myControlPoint.Name & "><" & Me.EntrySignal.Name & "> TO <" & Me.ExitSignal.Name & ">")
                                                End If
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ExtendStoreRoute(ByVal Direction As String, ByVal ExitTrack As Track, ByRef SelectionList As ArrayList, Optional ByRef EntryTrack As Track = Nothing)
            Try
                Dim NextComp As Object = ExitTrack
                Dim LastGUID As Int32 = NextComp.GUID
                If Not SelectionList.Contains(ExitTrack) Then
                    SelectionList.Add(ExitTrack)
                End If

                Dim LastTrack As Track = Nothing
                Dim LoopControl As Int32

                Dim CompCheck As Object
                For Each CompCheck In SelectionList
                    If CompCheck.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then
                        If Direction = "LEFT" Then
                            Direction = "RIGHT"
                        Else
                            Direction = "LEFT"
                        End If
                    End If
                Next

                If Direction = "LEFT" Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
                End If


                Do
                    If NextComp Is Nothing Then Exit Sub
                    Select Case NextComp.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            LastGUID = NextComp.GUID
                            LastTrack = NextComp
                            If Not SelectionList.Contains(NextComp) Then
                                SelectionList.Add(NextComp)
                            End If
                            If Direction = "LEFT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                            Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                            End If

                        Case "TMDS.SwitchComponents.Switch"
                            LastGUID = NextComp.GUID

                            If Not SelectionList.Contains(NextComp) Then
                                SelectionList.Add(NextComp)
                            End If

                            If Direction = "LEFT" Then

                                If NextComp.ControlledReverse Then
                                    If NextComp.LeftAltLink <> 0 Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                            Else
                                If NextComp.ControlledReverse Then
                                    If NextComp.RightAltLink <> 0 Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                            End If

                        Case "TMDS.SignalComponents.Signal"
                            LastGUID = NextComp.GUID

                            If (NextComp.ComponentType = "L" Or NextComp.ComponentType = "U") And Direction = "LEFT" Then
                                If Not IsNothing(LastTrack) Then
                                    gExitTrack = LastTrack
                                End If
                                Exit Sub
                            ElseIf (NextComp.ComponentType = "R" Or NextComp.ComponentType = "D") And Direction = "RIGHT" Then
                                If Not IsNothing(LastTrack) Then
                                    gExitTrack = LastTrack
                                End If
                                Exit Sub
                            ElseIf Direction = "LEFT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                If NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                    Exit Sub
                                End If
                            ElseIf Direction = "RIGHT" Then
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                If NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                    Exit Sub
                                End If
                            End If

                            If Not SelectionList.Contains(NextComp) Then
                                SelectionList.Add(NextComp)
                            End If

                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            If Not SelectionList.Contains(NextComp) Then
                                SelectionList.Add(NextComp)
                            End If
                            Dim thisFlip As FlipFlop = CType(NextComp, FlipFlop)
                            If Direction = "LEFT" Then
                                Direction = "RIGHT"
                            Else
                                Direction = "LEFT"
                            End If
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(thisFlip.FindExitGUID(LastGUID))
                        Case "TMDS.TrackOtherComponents.RRXING"

                            NextComp = NextComp.FindExitComponent(LastGUID)

                            'Case Else
                            '    If Not SelectionList.Contains(NextComp) Then
                            '        SelectionList.Add(NextComp)
                            '    End If
                            '    If Direction = "LEFT" Then
                            '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                            '    Else
                            '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                            '    End If

                    End Select

                    LoopControl = LoopControl + 1
                    If LoopControl = 20 Then
                        Exit Sub
                    End If
                Loop

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function ValidateRoute(ByVal CurrentCompSelection As ArrayList) As Boolean
            Try

                Dim compToCheck As Object
                Dim FlipFlopFound As Boolean
                For Each compToCheck In CurrentCompSelection
                    Select Case compToCheck.GetType.ToString

                        Case "TMDS.TrackOtherComponents.RRXING"
                            If Not compToCheck.ValidateForAllClearRoute Then
                                If Not compToCheck.ValidateForAllStoreRoute Then
                                    Exit Function
                                End If
                            End If

                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            FlipFlopFound = True
                        Case "TMDS.TrackComponents.Track"

                            If compToCheck.BlockAbsolute And Not compToCheck Is gFirstEntryTrack Then
                                Exit Function
                            End If
                        Case "TMDS.SwitchComponents.Switch"

                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(compToCheck, Signal)
                            If (gSearchDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                Exit Function
                            End If
                            If (gSearchDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                Exit Function
                            End If

                            'check for fleeted signals at this control point
                            Dim comp As Object
                            For Each comp In sig.myControlPoint.AssignedComponenets
                                If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                    If comp.FleetEnabled And (sig.Name = comp.Name) Then
                                        Exit Function
                                    ElseIf comp.FleetEnabled Then
                                        Dim sigCheck As Signal = CType(comp, Signal)
                                        If Not IsNothing(sigCheck.SignalRouteCollection) Then
                                            Dim compCheck As Object
                                            For Each compCheck In CurrentCompSelection
                                                If sigCheck.SignalRouteCollection.Contains(compCheck) Then
                                                    Exit Function
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            Next
                            If compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus Then
                                If compToCheck.SignalStatus = SignalComponents.Signal.BulbStatus.Unsolitated Then
                                    Exit Function
                                End If
                            End If
                    End Select
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Stored route event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub StoredRouteEventLog(message As String)
            Try
                SystemEvent($"{NameOf(StoredRoute)} {RouteGUID}:{TrainSymbolAssociation}:{message}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckforFleetSignal(ByVal TrackToCheck As Track, ByVal SearchDirection As String, ByVal IsLastTrack As Boolean) As Boolean
            Dim NextComp As Object = Nothing
            Dim CP As ControlPoint = Nothing
            Dim MessageList As New ArrayList
            Dim LaunchPoint As Point
            Dim SigComp As Object = Nothing
            Dim ComptoCheckCollection As New ArrayList
            Try
                If TrackToCheck Is Nothing Then Return False
                'If IsLastTrack and 
                If Not TrackToCheck.myControlPoint Is Nothing Then CP = TrackToCheck.myControlPoint
                If Not CP Is Nothing Then
                    LaunchPoint = New Point(CP.Left + 20, CP.Top + 50)
                    If CP.CommFail AndAlso optDisableStoredRouteOnCommFail Then
                        MessageList.Add(CP.Name & " in Code Fail. This operation is not allowed. Exiting!!!")
                        Me.StoredRouteEventLog("DISPATCHER(" & gCurrentUser.Initials & ") - " & CP.Name & "  IN CODE FAIL. THIS OPERATION IS NOT ALLOWED. EXITING!!!")
                        ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                        Return True
                    End If
                    MessageList = New ArrayList
                    If CP.ManualOperateMode AndAlso optPromptStoredRouteWhenAdjCPOnManualOperate Then
                        MessageList.Add(CP.Name & " in Local Control. This operation is not allowed. Exiting!!!")
                        Me.StoredRouteEventLog("DISPATCHER(" & gCurrentUser.Initials & ") - " & CP.Name & "  IN LOCAL CONTROL. THIS OPERATION IS NOT ALLOWED. EXITING!!!")
                        ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                        Return True

                        'MessageList.Add(CP.Name & " in Local Control. Do you wish to proceed?")
                        'If ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", True, LaunchPoint, True) <> 6 Then
                        '    Me.StoredRouteEvent("DISPATCHER(" & gCurrentUser.Initials & ") CLICKED [NO] TO - " & CP.Name & " IN LOCAL CONTROL. DO YOU WISH TO PROCEED?")
                        '    Return True
                        'Else
                        '    Me.StoredRouteEvent("DISPATCHER(" & gCurrentUser.Initials & ") CLICKED [YES] TO - " & CP.Name & " IN LOCAL CONTROL. DO YOU WISH TO PROCEED?")
                        'End If
                    End If
                End If
                If optEnableCPSpecificOptions Then
                    ComptoCheckCollection = GetComponentsFromSignal(gEntExtTrackSelection)
                End If

                Select Case SearchDirection
                    Case "LEFT"
                        NextComp = FindCompFromGUID(TrackToCheck.RightLink)
                        For i As Integer = 1 To 25
                            Select Case NextComp.GetType.ToString
                                Case gTrackComponentName
                                    NextComp = FindCompFromGUID(NextComp.RightLink)
                                Case gSwitchComponentName
                                    If NextComp.ControlledReverse Then
                                        NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                    Else
                                        NextComp = FindCompFromGUID(NextComp.RightLink)
                                    End If

                                Case gSignalComponentName
                                    'If NextComp.ComponentType = "R" Or NextComp.ComponentType = "D" Then
                                    If NextComp.ComponentType = "L" Or NextComp.ComponentType = "U" Then
                                        If NextComp.FleetEnabled Then
                                            If optEnableCPSpecificOptions Then
                                                SigComp = Nothing
                                                Dim OverrideConflictCheck As Boolean = False
                                                If NextComp.SignalRouteCollection IsNot Nothing AndAlso NextComp.SignalRouteCollection.Count > 0 Then
                                                    For Each SigComp In NextComp.SignalRouteCollection
                                                        If ComptoCheckCollection.Contains(SigComp) Then
                                                            OverrideConflictCheck = True
                                                        End If
                                                    Next
                                                End If
                                                If OverrideConflictCheck Then
                                                    MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                                    LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                                    ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                                    Return True
                                                End If
                                            Else
                                                MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                                LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                                ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                                Return True
                                            End If


                                            'MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                            'LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                            'ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                            'Return True
                                        Else
                                            Return False
                                        End If
                                        'Return NextComp.FleetEnabled
                                    Else
                                        NextComp = FindCompFromGUID(NextComp.RightLink)
                                    End If
                            End Select
                        Next
                    Case "RIGHT"
                        NextComp = FindCompFromGUID(TrackToCheck.LeftLink)
                        For i As Integer = 1 To 25
                            Select Case NextComp.GetType.ToString
                                Case gTrackComponentName
                                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                                Case gSwitchComponentName
                                    If NextComp.ControlledReverse Then
                                        NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                    Else
                                        NextComp = FindCompFromGUID(NextComp.LeftLink)
                                    End If

                                Case gSignalComponentName
                                    If NextComp.ComponentType = "R" Or NextComp.ComponentType = "D" Then
                                        'If NextComp.ComponentType = "L" Or NextComp.ComponentType = "U" Then
                                        'Return NextComp.FleetEnabled
                                        If NextComp.FleetEnabled Then
                                            If optEnableCPSpecificOptions Then
                                                SigComp = Nothing
                                                Dim OverrideConflictCheck As Boolean = False
                                                If NextComp.SignalRouteCollection IsNot Nothing AndAlso NextComp.SignalRouteCollection.Count > 0 Then
                                                    For Each SigComp In NextComp.SignalRouteCollection
                                                        If ComptoCheckCollection.Contains(SigComp) Then
                                                            OverrideConflictCheck = True
                                                        End If
                                                    Next
                                                End If
                                                If OverrideConflictCheck Then
                                                    MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                                    LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                                    ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                                    Return True
                                                End If


                                            Else
                                                MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                                LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                                ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                                Return True
                                            End If
                                            'MessageList.Add("Conflicting Fleet Signal Detected. Exiting!!!")
                                            'LaunchPoint = New Point(NextComp.Left + 20, NextComp.Top + 50)
                                            'ShowCustomMessageBox(MessageList, "CONFLICT CHECK STORED ROUTE", False, LaunchPoint, True)
                                            'Return True
                                        Else
                                            Return False
                                        End If
                                    Else
                                        NextComp = FindCompFromGUID(NextComp.LeftLink)
                                    End If
                            End Select
                        Next

                End Select

            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                NextComp = Nothing
                MessageList = Nothing
                CP = Nothing
                LaunchPoint = Nothing
            End Try
            Return False
        End Function

        Private Function GetComponentsFromSignal(ByVal CompCollection As ArrayList) As ArrayList
            Dim comp As Object = Nothing
            Dim FirstSignalFound As Boolean = False
            Dim ReturnList As New ArrayList
            Try
                For Each comp In CompCollection
                    If comp.GetType.ToString = gSignalComponentName Then FirstSignalFound = True
                    If FirstSignalFound Then
                        If Not ReturnList.Contains(comp) Then ReturnList.Add(comp)
                    End If
                Next
                Return ReturnList

            Catch ex As Exception
                GetErrorDetails(ex)
                Return Nothing
            End Try

        End Function

        Private Function CheckToSeeIfTrainInFrontOfStoredRoute(ByVal trk As Track) As Boolean
            Dim OverrideStoredRoute As Boolean
            Try
                If IsNothing(Me.theTrain) Then Return True

                If Me.TrafficDirection = "LEFT" Then
                    'Dim trkRight As Object = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trk.LeftLink)
                    'If trkRight.GetType.ToString <> gTrackComponentName Then
                    '    trkRight = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trkRight.LeftLink)
                    '    If trkRight.GetType.ToString <> gTrackComponentName Then
                    '        trkRight = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(trkRight.LeftLink)
                    '    End If
                    'End If
                    'If Not IsNothing(trkRight) Then
                    'If trkRight.Name = trk.Name And trkRight.TrainIDs.Count > 0 Then
                    '    OverrideStoredRoute = True
                    If Not IsNothing(theTrain) AndAlso theTrain.GeoFenceLegacyEnabled Then
                        If trk.TrainIDs.Count > 0 Then
                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                            If Not IsNothing(trn) Then
                                If trn.Name = theTrain.Name Then
                                    OverrideStoredRoute = True
                                End If
                            End If
                        End If
                    End If
                    '    End If
                    'End If

                ElseIf Me.TrafficDirection = "RIGHT" Then
                    'Dim trkLeft As Object = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trk.RightLink)
                    'If trkLeft.GetType.ToString <> gTrackComponentName Then
                    '    trkLeft = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(trkLeft.RightLink)
                    '    If trkLeft.GetType.ToString <> gTrackComponentName Then
                    '        trkLeft = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(trkLeft.RightLink)
                    '    End If
                    'End If
                    'If Not IsNothing(trkLeft) Then
                    '    If trkLeft.Name = trk.Name And trkLeft.TrainIDs.Count > 0 Then
                    '        OverrideStoredRoute = True
                    If Not IsNothing(theTrain) AndAlso theTrain.GeoFenceLegacyEnabled Then
                        If trk.TrainIDs.Count > 0 Then
                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                            If Not IsNothing(trn) Then
                                If trn.Name = theTrain.Name Then
                                    OverrideStoredRoute = True
                                End If
                            End If
                        End If
                    End If
                    '    End If
                End If
                Return OverrideStoredRoute
            Catch ex As Exception
                GetErrorDetails(ex)
                Return False
            End Try


        End Function

        ''' <summary>
        ''' Remove Particular Route From Database
        ''' </summary>
        ''' <param name="pRouteGUID"></param>
        ''' <returns></returns>
        Public Function RemoveParticularRouteFromDatabase(ByVal pRouteGUID As String) As Boolean
            Dim q As String = "'"
            Dim s As String = ", "
            Dim query As String
            Try
                query = "DELETE FROM tblStoreRoutes WHERE RouteGUID = " & q & pRouteGUID & q
                SystemDatabaseCommandToDynamic("STOREDROUTE:RemoveRouteFromDatabase", query)
                StoredRouteEventLog("RemoveParticularRouteFromDatabase:Execute")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                q = Nothing
                s = Nothing
                query = Nothing
            End Try
        End Function

        Public Function CheckForValidity_GSR() As Boolean
            Try
                'VALIDATE THE COMPONENTS TO SEE IF THESE ARE OK TO CLEAR A ROUTE
                If Me.EntrySignal.BlockAbsolute Then Exit Function

                If Me.CtrlPoint.FlashComponent Then Exit Function
                If Me.CtrlPoint.FailState Then Exit Function

                Dim compToCheck As Object

                'Check to see if any SR in higher proirity has identical components in route
                'If Me.Priority <> 1 Then
                If Me.Priority <> 0 Then
                    Dim sr As StoredRoute
                    For Each sr In Me.CtrlPoint.StoredRoutes
                        If sr.Priority < Me.Priority Then
                            Dim PerformComponentCheck As Boolean = True
                            If Not IsNothing(sr.myTacticalRoute) Then
                                If sr.myTacticalRoute.myRouteDependencies.Count > 0 Then
                                    PerformComponentCheck = False
                                End If
                            End If
                            If PerformComponentCheck Then
                                Dim comp As Object
                                For Each comp In Me.ComponentSelection
                                    Dim compInRoute As Object
                                    For Each compInRoute In sr.ComponentSelection
                                        If compInRoute Is comp Then
                                            Return False
                                        End If
                                    Next
                                Next
                            End If
                        Else
                            Exit For
                        End If
                    Next
                End If

                'CHECK TO SEE IF THERE ARE ANY PARALELL BROTHER SIGNALS REQUESTED OR CLEARED. 
                'REFERENCE THE CONFIGURATION AT LOWELL ON SCENIC2 ON THE BNSF WHERE THE SIDING SIGNAL WILL CLEAR OVER THE TOP OF A MAIN LINE SIGNAL.
                If Not IsNothing(Me.EntrySignal.BrotherSignalCollection) Then
                    Dim sig As Signal
                    For Each sig In Me.EntrySignal.BrotherSignalCollection
                        If sig.SignalCtlStatus <> Signal.CtlStatus.RequestedStop Then
                            Return False
                        End If
                    Next
                End If

                If IsNothing(Me.ComponentSelection) Then
                    Me.RemoveMe("CheckForValidity")
                    Exit Function
                End If

                For Each compToCheck In Me.ComponentSelection
                    Select Case compToCheck.GetType.ToString
                        Case "TMDS.TrackComponents.Track"
                            Dim trk As Track = CType(compToCheck, Track)
                            If Me.EntrySignal.HoldingSignal Then
                                If trk.HoldingSignalSlottingTrack Then
                                    If trk.Occuppied Then
                                        Return False
                                    End If
                                End If
                            End If
                            If compToCheck.BlockAbsolute Then
                                Return False
                            End If
                            If Not compToCheck.IsRemoteTrack Then
                                If compToCheck.myControlPoint.PendingStatusChange Then
                                    Return False
                                End If
                            End If

                            Dim FlipDirection As Boolean
                            If Not IsNothing(trk.myTerritory) Then
                                If trk.myTerritory.FlipDirection Then
                                    FlipDirection = True
                                Else
                                    FlipDirection = False
                                End If
                            End If

                            If trk.Occuppied AndAlso
                            (Me.TrafficDirection = "RIGHT" AndAlso trk.DisallowRightSignalRequestOnOccupancy OrElse
                             Me.TrafficDirection = "LEFT" AndAlso trk.DisallowLeftSignalRequestOnOccupancy) Then
                                If IsNothing(Me.theTrain) Then
                                    Return False
                                ElseIf Not CheckToSeeIfTrainInFrontOfStoredRoute(trk) Then
                                    Return False
                                End If
                            End If


                            If optEnableTrainDirectionLogicCheckForStoredRoutes Then
                                If trk.TrainIDs.Count > 0 Then
                                    Dim trn As TrainID
                                    For Each trn In trk.TrainIDs
                                        If Not IsNothing(trn) Then
                                            If trn.Direction <> "NONE" And trn.Direction <> "" Then
                                                Select Case trn.Direction
                                                    Case optDirectionWest, optDirectionSouth
                                                        If FlipDirection Then
                                                            If Me.TrafficDirection = "LEFT" Then
                                                                Return False
                                                            End If
                                                        Else
                                                            If Me.TrafficDirection = "RIGHT" Then
                                                                Return False
                                                            End If
                                                        End If
                                                    Case optDirectionNorth, optDirectionWest
                                                        If FlipDirection Then
                                                            If Me.TrafficDirection = "RIGHT" Then
                                                                Return False
                                                            End If
                                                        Else
                                                            If Me.TrafficDirection = "LEFT" Then
                                                                Return False
                                                            End If
                                                        End If
                                                End Select
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                            If Not trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> "" And compToCheck.TrafficDirection <> Me.TrafficDirection) Then
                                Return False
                            ElseIf trk.OStype And compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop Then
                                Return False
                            End If

                            If compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then

                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                    Return False
                                End If
                            Else
                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> Me.TrafficDirection) Then
                                    Dim OverRideTrafficCheck As Boolean
                                    If optEnableGeoFenceOperations Then
                                        OverRideTrafficCheck = Me.EntrySignal.DoLicenseCheckForRearOfTrain(trk, Nothing)
                                    End If
                                    If Not OverRideTrafficCheck Then
                                        Return False
                                    End If
                                End If
                            End If


                            If compToCheck.FailState Or (compToCheck.OStype And compToCheck.Occuppied) Then
                                Return False
                            End If

                            If optEnableLicenseGenerationForPTC Then
                                Dim num As Int32 = trk.GUID

                                If trk.TrackAuthorities.Count > 0 Then
                                    Dim AuthCheckFailed As Boolean = True
                                    Dim OverRideOverlapCheck As Boolean = False

                                    If Not IsNothing(Me.theTrain) Then
                                        Dim auth As TrackAuthority.TrackAuthority = trk.TrackAuthorities.Item(0)
                                        If auth.IssueTo = theTrain.IdentifingUnit Then
                                            AuthCheckFailed = False
                                        End If
                                        If optEnableGeoFenceOperations Then
                                            OverRideOverlapCheck = Me.EntrySignal.DoLicenseCheckForRearOfTrain(trk, Nothing)
                                        End If
                                    End If
                                    If AuthCheckFailed AndAlso Not OverRideOverlapCheck Then Return False
                                End If

                                If Not IsNothing(theTrain) Then
                                    If Not theTrain.DoCheckForStoreRouteExecutionOrder(Me) Then
                                        Return False
                                    End If
                                End If
                            End If

                            'CATENARY CHECK FOR ENERGIZED CIRCUIT
                            If optEnableTractionPowerManagementSystem Then
                                If trk.EnableTractionPowerManagement AndAlso Not IsNothing(trk.myCatenaryTrack) AndAlso Not trk.myCatenaryTrack.IsEnergized Then
                                    Return False
                                End If
                            End If


                        Case "TMDS.SwitchComponents.Switch"

                            If compToCheck.TypeOfSwitch = "SPM ONLY" Then
                            Else
                                If compToCheck.SignalStatOnMe <> SwitchComponents.Switch.SignalStatus.SignalAtStop _
                                Or compToCheck.FailState _
                                Or compToCheck.Occuppied Then
                                    Return False
                                End If
                                Dim sw As SwitchToExecute
                                For Each sw In Me.SwitchExecuteList
                                    If sw.switchToControl Is compToCheck Then
                                        If sw.switchToControl.BlockAbsolute Or sw.switchToControl.SwitchLockBit Then
                                            If sw.ControlReverse <> sw.switchToControl.ControlledReverse Then
                                                Return False
                                            End If
                                        End If
                                        Exit For
                                    End If
                                Next
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(compToCheck, Signal)
                            If (Me.TrafficDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And compToCheck.BlockAbsolute Then
                                ClearSystemVariables()
                                Return False
                            End If
                            If (gSearchDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And compToCheck.BlockAbsolute Then
                                ClearSystemVariables()
                                Return False
                            End If

                            If (compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus _
                            Or compToCheck.LastCtlStatus <> Signal.CtlStatus.RequestedStop _
                            Or compToCheck.FailState) AndAlso Not sig.AutomaticSignal Then
                                Return False
                            End If
                        Case "TMDS.TrackOtherComponents.RRXING"
                            If Not compToCheck.ValidateForAllClearRoute Then
                                Return False
                            End If

                    End Select
                Next

                If optEnableGeoFenceOperations Then
                    If Not IsNothing(Me.theTrain) Then
                        If theTrain.GeoFenceEnabled AndAlso Not theTrain.GeoFenceLegacyEnabled Then
                            If Not IsNothing(theTrain.thisCurrentLicense) Then
                                If theTrain.thisCurrentLicense.PTCElectronicTransmissionInProgress OrElse Not theTrain.IsPresenceOnNetwork Then
                                    Me.StoredRouteEventLog("CBTC:CheckForValidity-PTCElectronicTransmissionInProgress=" & theTrain.thisCurrentLicense.PTCElectronicTransmissionInProgress)
                                    Me.StoredRouteEventLog("CBTC:CheckForValidity-IsPresenceOnNetwork=" & theTrain.IsPresenceOnNetwork)
                                    Return False
                                End If
                            End If
                        End If
                    End If
                End If

                Me.StoredRouteEventLog("STORED ROUTE VALID")
                Return True

            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Friend Function RemoveGSRRoutes() As Boolean
            Try
                Dim GSR As StoredRoute
                Dim TmpGSRCollection As New ArrayList
                CopyArrayCollection(TmpGSRCollection, Me.GroupedStoredRouteCollection)
                For Each GSR In TmpGSRCollection
                    If GSR.RouteGUID <> Me.RouteGUID Then
                        GSR.RemoveMe("mnuRoutes_Click", False, True)
                        If GSR.CtrlPoint IsNot Nothing Then
                            If GSR.CtrlPoint Is Me Then
                            Else
                                GSR.CtrlPoint.StoredRoutes.Remove(GSR)
                            End If
                        End If
                    End If
                Next
                Me.RemoveMe("mnuRoutes_Click", False, True)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Function

#Region "EVENT LOGGING"

        Public Enum EventTypeStoreRoute
            CREATE
            CANCEL
            REORDER
        End Enum

        Private Delegate Sub DoEventLogStoredRoute(ByVal EventType As EventTypeStoreRoute)

        Public Sub LogTheStoredRouteEvent(ByVal EventType As EventTypeStoreRoute)
            Try
                Dim CurUser As String = TMDS.GlobalDeclarations.gCurrentUser.LastName & "," & TMDS.GlobalDeclarations.gCurrentUser.FirstName
                Dim CurWrkstn As Integer = CInt(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim CurTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(Me.TerrUID)

                'Dim LogString As String = "INSERT INTO tblFunctionEvents "
                Dim LogString As String = "INSERT INTO tblSystemEvents "

                'Dim SqlFields As String = "(EventTime, Territory, Wrkstn, SysUser, DbVer, FuncType, UID, Event, SysData)"
                Dim SqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim SqlValues As String
                Dim c As String = ", "
                Dim q As String = "'"
                SqlValues = " VALUES (" & q & Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & q & c & Me.CtrlPoint.TerritoryID & c & CurWrkstn & c & q & CurUser & q & c
                SqlValues = SqlValues & q & CurTerr.Dbversion & q & c & q & "STORED-ROUTE" & q & c & q & Me.RouteGUID & q & c

                Select Case EventType
                    Case EventTypeStoreRoute.CREATE
                        SqlValues = SqlValues & q & "CREATE" & q & c & q
                        'SqlValues = SqlValues & Me.CtrlPoint.Name & "|"
                        'SqlValues = SqlValues & Me.EntrySignal.Name & "|"
                        'SqlValues = SqlValues & Me.EntrySignal.GUID & "|"
                        'SqlValues = SqlValues & Me.ExitSignal.Name & "|"
                        'SqlValues = SqlValues & Me.ExitSignal.GUID & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"

                    Case EventTypeStoreRoute.CANCEL
                        SqlValues = SqlValues & q & "CANCEL" & q & c & q
                        'SqlValues = SqlValues & Me.CtrlPoint.Name & "|"
                        'SqlValues = SqlValues & Me.EntrySignal.Name & "|"
                        'SqlValues = SqlValues & Me.ExitSignal.Name & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"

                    Case EventTypeStoreRoute.REORDER
                        SqlValues = SqlValues & q & "REORDER" & q & c & q
                        'SqlValues = SqlValues & Me.CtrlPoint.Name & "|"
                        'SqlValues = SqlValues & Me.EntrySignal.Name & "|"
                        'SqlValues = SqlValues & Me.ExitSignal.Name & "|"
                        'SqlValues = SqlValues & Me.Priority.ToString & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"
                End Select

                SqlValues = SqlValues & Me.CtrlPoint.GUID & "|"
                SqlValues = SqlValues & Me.EntrySignal.GUID & "|"
                SqlValues = SqlValues & Me.ExitSignal.GUID & "|"
                SqlValues = SqlValues & Me.Priority & "|"

                Dim comp As Object
                Dim CompList As String = String.Empty
                If Not Me.ComponentSelection Is Nothing Then
                    For Each comp In Me.ComponentSelection
                        CompList = CompList & "-" & comp.GUID
                    Next
                End If
                SqlValues = SqlValues & "COMPONENTS" & CompList & "|"
                SqlValues = SqlValues & Me.CtrlPoint.Name & "|"
                SqlValues = SqlValues & Me.EntrySignal.Name & "|"
                SqlValues = SqlValues & Me.ExitSignal.Name & "|"
                SqlValues = SqlValues & q
                SqlValues = SqlValues & ")"

                SyncLock gSystemEventLogQueue
                    TMDS.GlobalDeclarations.gSystemEventLogQueue.Enqueue(LogString & SqlFields & SqlValues)
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

        Public Class SwitchToExecute
            Public ControlReverse As Boolean
            Public switchToControl As Switch

        End Class

    End Class

    Public Class TacticalRoute
        Public myGUID As String
        Public myTrain As TrainID
        Private _status As String
        Public mySignalRoute As SignalRoute
        Public EffectiveTime As String
        Public myStoredRouteUID As String
        Public myRouteDependencies As New ArrayList
        Public RouteRemoved As Boolean
        Public IsReadyToExecute As Boolean
        Private newPropertyValue As String
        Public RouteAlreadyUsed As Boolean

        Public Property myStatus() As String
            Get
                Return _status
            End Get
            Set(ByVal value As String)
                _status = value
                SystemEvent($"TACEVT:{myStatus}:{mySignalRoute.EntrySignal.Name}")
                If myStatus.Equals("FULLFILLED") Then mySignalRoute.EntrySignal.Tag = String.Empty
            End Set
        End Property

        ''' <summary>
        ''' Create Route
        ''' </summary>
        ''' <param name="thetrain">the train</param>
        ''' <param name="theSignalRoute">the Signal Route</param>
        ''' <param name="theStatus">the Status</param>
        Public Sub CreateRoute(thetrain As TrainID,
                               theSignalRoute As SignalRoute,
                               theStatus As String)
            Try
                SystemEvent($"{GetCurrentMethod.Name}:TacticalRoute={theStatus}")
                myTrain = thetrain
                mySignalRoute = theSignalRoute
                myGUID = Guid.NewGuid.ToString.ToUpper
                EffectiveTime = Now
                myStatus = theStatus
                InsertRoute()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function GetRoute(ByVal theTrain As TrainID, ByVal theGUID As String) As Boolean
            Dim dbconn As OleDbConnection = Nothing
            Dim dbcomm As OleDbCommand = Nothing
            Dim dr As OleDbDataReader
            Dim query As New StringBuilder
            Dim RouteExist As Boolean
            Try
                Dim theSignalRouteUID As String = String.Empty
                myTrain = theTrain
                myGUID = theGUID

                query.Append("SELECT SignalRouteGUID, RouteStatus, StoredRouteUID FROM tblPlanningTacticalRoutesActive  WHERE UID = '" & theGUID & "'")

                dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringDynamic)
                dbconn.Open()
                dbcomm = New OleDbCommand(query.ToString, dbconn)
                dr = dbcomm.ExecuteReader
                While dr.Read
                    If Not dr.IsDBNull(0) Then theSignalRouteUID = dr.GetString(0)
                    If Not dr.IsDBNull(1) Then myStatus = dr.GetString(1)
                    If Not dr.IsDBNull(2) Then myStoredRouteUID = dr.GetString(2)
                    RouteExist = True
                End While

                Dim sr As SignalRoute
                For Each sr In gSignalRouteList
                    If sr.SystemUID = theSignalRouteUID Then
                        mySignalRoute = sr
                        Exit For
                    End If
                Next
                If IsNothing(mySignalRoute) Then
                    SystemEvent("GetRoute-FAILED:" & theGUID & ":TRAIN-" & Me.myTrain.Name)
                End If
                dbconn.Close()
                dbconn.Dispose()
                dbconn = Nothing
                Return RouteExist
            Catch ex As Exception
                'SystemException(ex.ToString)
                SystemEvent("GetRoute-FAILED:" & theGUID)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Insert route
        ''' </summary>
        Private Sub InsertRoute()
            Dim update As New System.Text.StringBuilder
            Try
                update.Append("INSERT INTO tblPlanningTacticalRoutesActive ")
                update.Append("(UID, TerrUID, TrainSymbol, TrainGUID, SignalRouteGUID, RouteStatus, ExecutionTime, StoredRouteUID, ControlPointUID, ETA, ETD)")
                update.Append(" VALUES ( ")
                update.Append("'" & myGUID & "', ")
                update.Append(mySignalRoute.EntrySignal.myTerritory.TerritoryID & ", ")
                update.Append("'" & myTrain.Name & "', ")
                update.Append("'" & myTrain.TrainGUID & "', ")
                update.Append("'" & mySignalRoute.SystemUID & "', ")
                update.Append("'" & myStatus & "', ")
                update.Append("'" & Now & "', ")
                update.Append("'" & myStoredRouteUID & "', ")
                update.Append("'" & mySignalRoute.EntrySignal.myControlPoint.GUID & "', ")
                update.Append("'" & Now & "', ")
                update.Append("'" & Now & "'")
                update.Append(")")
                SystemDatabaseCommandToDynamicMovementPlanning("TACTICALROUTE:InsertRoute", update.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                update = Nothing
            End Try
        End Sub

        Public Sub UpdateRouteStatus(ByVal RouteMessage As String)
            Try
                Me.myStatus = RouteMessage
                Dim Update As New System.Text.StringBuilder
                Update.Append("UPDATE tblPlanningTacticalRoutesActive SET RouteStatus = '")
                Update.Append(Me.myStatus) : Update.Append("',")
                Update.Append("TrainGUID = '")
                If Not IsNothing(Me.myTrain) Then
                    If myTrain.GeoFenceLegacyEnabled Then
                        SystemEvent("UpdateRouteStatus-TACTICAL ROUTE STATUS:" & RouteMessage & "SIGNAL:" & Me.mySignalRoute.EntrySignal.Name & "-" & Me.mySignalRoute.EntrySignal.GUID & ":TRAIN-" & Me.myTrain.Name)
                    End If
                    Update.Append(Me.myTrain.TrainGUID)
                End If
                Update.Append("',")
                Update.Append("StoredRouteUID = '") : Update.Append(myStoredRouteUID) : Update.Append("'")
                Update.Append(" WHERE UID = '") : Update.Append(myGUID) : Update.Append("'")
                'SystemDatabaseCommandToDynamic("TACTICALROUTE:UpdateRouteStatus", Update.ToString)
                SystemDatabaseCommandToDynamicMovementPlanning("TACTICALROUTE:UpdateRouteStatus", Update.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RemoveRoute()
            Try
                If RouteRemoved Then Exit Sub
                RouteRemoved = True
                Dim Insert As String = "INSERT INTO tblPlanningTacticalRoutesInActive SELECT * FROM tblPlanningTacticalRoutesActive WHERE UID = '" & myGUID & "'"
                'SystemDatabaseCommandToDynamic("TACTICALROUTE:UpdateRouteStatus", Insert)
                SystemDatabaseCommandToDynamicMovementPlanning("TACTICALROUTE:RemoveRoute", Insert)
                Dim Delete As String = "DELETE FROM tblPlanningTacticalRoutesActive WHERE UID = '" & myGUID & "'"
                'SystemDatabaseCommandToDynamic("TACTICALROUTE:UpdateRouteStatus", Delete)
                SystemDatabaseCommandToDynamicMovementPlanning("TACTICALROUTE:RemoveRoute", Delete)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CreateDependencies(ByVal data As String)
            Try
                Dim deps() As String = data.Split("|")
                Dim dep As String
                For Each dep In deps
                    If dep <> "" Then
                        TMDS.SystemUtilities.Utilities.SystemEvent("TacticalRoute-CREATING ROUTE DEPENDENCY <" & Me.myGUID & "> <" & Me.mySignalRoute.SystemUID & "> :" & dep)
                        Dim rds() As String = dep.Split("-")
                        If rds.Length >= 5 Then
                            Dim RteDep As New RouteDependency
                            RteDep.RouteDepGUID = rds(0)
                            RteDep.TrnArrival = CDate(rds(1))
                            RteDep.TrnSymbol = rds(2)
                            RteDep.Action = rds(3)
                            RteDep.GeoVersion = rds(4)
                            myRouteDependencies.Add(RteDep)
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function DoesItContainTrainDependency(ByVal TrnName As String) As Boolean
            Try
                Dim rd As RouteDependency
                For Each rd In myRouteDependencies
                    If rd.TrnSymbol = TrnName Then
                        Return True
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub SendRouteErrorExecuteMessage(ByVal RouteStatus As String, ByVal MessType As String)
            Try
                TMDS.NetworkMessages.MPARMessages.SendRouteErrorMessage(Me, MessType, RouteStatus)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function PerformLookBehindToNextSignalForAutoRouting() As Boolean
            Try
                Dim SigToTest As Signal = mySignalRoute.EntrySignal
                If IsNothing(SigToTest) Then Return False
                Dim NextComp As Object
                Dim LoopControl As Integer
                If SigToTest.ComponentType = "L" Then
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(SigToTest.RightLink)
                    Do
                        TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting1-NextComp:" & NextComp.guid.ToString)
                        If NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Dim sig As Signal = CType(NextComp, Signal)
                            'If sig.ComponentType = "L" Then
                            If sig.AutomaticModeOn Then
                                Return True
                            Else
                                If IsNothing(myTrain) Then
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-TrainNotFound")
                                    Return False
                                End If

                                If IsNothing(myTrain.OSPreviousControlPoint) Then
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-OSPreviousControlPointNotFound")
                                    Return False
                                End If
                                If myTrain.OSPreviousControlPoint.GUID = sig.myControlPoint.GUID Then
                                    Return True
                                Else
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-OSPreviousControlPointNotMatched")
                                End If
                                Return False
                            End If
                            'End If
                        ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.AuthorityType = "CTC-OTHER" Then
                                Return True
                            End If
                        End If
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                        LoopControl += 1
                        If LoopControl > 20 Then
                            Return False
                        End If
                    Loop
                Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(SigToTest.LeftLink)
                    Do
                        TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting2-NextComp:" & NextComp.guid.ToString)
                        If NextComp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            Dim sig As Signal = CType(NextComp, Signal)
                            'If sig.ComponentType = "R" Then
                            If sig.AutomaticModeOn Then
                                Return True
                            Else
                                If IsNothing(myTrain) Then
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-TrainNotFound")
                                    Return False
                                End If

                                If IsNothing(myTrain.OSPreviousControlPoint) Then
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-OSPreviousControlPointNotFound")
                                    Return False
                                End If
                                If myTrain.OSPreviousControlPoint.GUID = sig.myControlPoint.GUID Then
                                    Return True
                                Else
                                    TMDS.SystemUtilities.Utilities.SystemEvent("PerformLookBehindToNextSignalForAutoRouting-OSPreviousControlPointNotMatched")
                                End If
                                Return False
                            End If
                            'End If 
                        ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = CType(NextComp, Track)
                            If trk.AuthorityType = "CTC-OTHER" Then
                                Return True
                            End If
                        End If
                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                        LoopControl += 1
                        If LoopControl > 20 Then
                            Return False
                        End If
                    Loop
                End If
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function PerfromLookBehindForStoredRoute() As Boolean
            Try
                TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookBehindForStoredRoute")

                Dim tr As TacticalRoute
                For Each tr In Me.myTrain.myTacticalRoutes
                    TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookBehindForStoredRoute-myTacticalRoutes:" & tr.mySignalRoute.SystemUID)
                    If tr.myGUID <> Me.myGUID Then
                        If tr.myStoredRouteUID <> "" Then
                            TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookBehindForStoredRoute-myStoredRouteUID:" & tr.mySignalRoute.SystemUID)
                            Dim sr As StoredRoute
                            For Each sr In tr.mySignalRoute.myControlPoint.StoredRoutes
                                TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookBehindForStoredRoute-myStoredRouteUID1:" & sr.RouteGUID)
                                If sr.RouteGUID = tr.myStoredRouteUID Then
                                    Return False
                                End If
                            Next
                        End If
                    End If
                Next
                TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookBehindForStoredRoute-COMPLETE")

                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function PerformLookForwardForTrainsInRoute(ByVal theTrain As TrainID, ByVal RequestingRoute As TacticalRoute) As Boolean
            Try
                TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookForTrainsInRoute")
                If Me.myTrain.myTacticalRoutes.Count > 0 Then
                    Dim tr As TacticalRoute = Me.myTrain.myTacticalRoutes(Me.myTrain.myTacticalRoutes.Count - 1)
                    If Not IsNothing(tr) Then
                        Dim comp As Object
                        For Each comp In tr.mySignalRoute.ComponentList
                            If comp.ToString = gTrackComponentName Then
                                Dim trk As Track = CType(comp, Track)
                                If trk.TrainIDs.Count > 0 Then
                                    Dim trn As TrainID
                                    For Each trn In trk.TrainIDs
                                        If trn.Name <> theTrain.Name Then
                                            TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookForTrainsInRoute-Train found in route:" & trn.Name)

                                            Return False
                                        End If
                                    Next
                                End If
                            End If
                        Next
                    End If
                Else
                    If Not IsNothing(RequestingRoute) Then
                        Dim comp As Object
                        For Each comp In RequestingRoute.mySignalRoute.ComponentList
                            If comp.ToString = gTrackComponentName Then
                                Dim trk As Track = CType(comp, Track)
                                If trk.TrainIDs.Count > 0 Then
                                    Dim trn As TrainID
                                    For Each trn In trk.TrainIDs
                                        If trn.Name <> theTrain.Name Then
                                            TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookForTrainsInRoute-Train found in route:" & trn.Name)
                                            Return False
                                        End If
                                    Next
                                End If
                            End If
                        Next
                    End If
                    If RequestingRoute.mySignalRoute.ComponentList.Contains(theTrain.myTrack) Then

                    End If
                    Dim theCompsInTrainRoute As ArrayList = FindRouteAheadOfTrain(theTrain)
                    Dim t As Track
                    For Each t In theCompsInTrainRoute
                        TMDS.SystemUtilities.Utilities.SystemEvent("FindRouteAheadOfTrain-Track:" & t.Name & ":" & t.GUID)

                        If t.TrainIDs.Count > 0 Then
                            Dim trn As TrainID
                            For Each trn In t.TrainIDs
                                If trn.Name <> theTrain.Name Then
                                    TMDS.SystemUtilities.Utilities.SystemEvent("FindRouteAheadOfTrain-Train found in route:" & trn.Name)
                                    Return False
                                End If
                            Next
                        End If
                    Next
                End If


                TMDS.SystemUtilities.Utilities.SystemEvent("PerfromLookForTrainsInRoute-COMPLETE")

                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function FindRouteAheadOfTrain(ByVal theTrain As TrainID) As ArrayList
            Dim CompList As New ArrayList

            Dim FirstTrackInBlock As Track = theTrain.myTrack
            Dim NextComp As Object = theTrain.myTrack
            Dim LastComp As Object = Nothing
            Dim LoopControl As Integer
            Try
                Dim theDirection As String = String.Empty
                Select Case theTrain.Direction
                    Case optDirectionNorth, optDirectionEast
                        theDirection = "RIGHT"
                    Case optDirectionSouth, optDirectionWest
                        theDirection = "LEFT"
                End Select

                Do Until LoopControl > 25
                    Select Case theDirection
                        Case "RIGHT"
                            If NextComp.GetType.ToString = gSignalComponentName Then
                                Dim sig As Signal = CType(NextComp, Signal)
                                If sig.ComponentType = "R" And Not sig.AutomaticSignal Then
                                    Exit Do
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                                Dim sw As Switch = CType(NextComp, Switch)
                                If sw.RightAltLink <> 0 Then
                                    If sw.ControlledReverse Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                    End If
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                                End If
                            ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                                Dim trk As Track = CType(NextComp, Track)
                                CompList.Add(NextComp)
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.RightLink)
                            End If

                        Case "LEFT"
                            If NextComp.GetType.ToString = gSignalComponentName Then
                                Dim sig As Signal = CType(NextComp, Signal)
                                If sig.ComponentType = "L" And Not sig.AutomaticSignal Then
                                    Exit Do
                                End If
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            ElseIf NextComp.GetType.ToString = gSwitchComponentName Then
                                Dim sw As Switch = CType(NextComp, Switch)
                                If sw.LeftAltLink <> 0 Then
                                    If sw.ControlledReverse Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftAltLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                    End If
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If
                            ElseIf NextComp.GetType.ToString = gTrackComponentName Then
                                Dim trk As Track = CType(NextComp, Track)
                                CompList.Add(NextComp)
                                NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                            End If


                    End Select
                    LoopControl += 1
                Loop


                Return CompList
            Catch ex As Exception
                GetErrorDetails(ex)
                Return Nothing
            End Try

        End Function

    End Class

    Public Class RouteDependency
        Public RouteDepGUID As String
        Public TrnArrival As Date
        Public Action As String
        Public TrnSymbol As String
        Public GeoVersion As String
        Public RouteDependencySatisfied As Boolean

        Public Function CheckToSatisfyDependency(ByVal thisControlPoint As ControlPoint) As Boolean
            Try
                Dim trn As TrainID
                Dim OS As String
                Dim OsString() As String
                For Each trn In TMDS.GlobalDeclarations.TrainCollection
                    If trn.Name = Me.TrnSymbol Then
                        For Each OS In trn.OSTimes
                            OsString = OS.Split("|")
                            If OsString.Length > 2 Then
                                If Trim(OsString(0)) = Trim(thisControlPoint.Name) Then
                                    RouteDependencySatisfied = True
                                    Return True
                                End If
                            End If
                        Next
                    End If
                Next
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function
    End Class

#End Region

End Namespace

