Imports System.Data.OleDb
Imports System.Drawing.Drawing2D
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.NetworkClasses
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.TrackAuthority
Imports TMDS.TrackComponents
Imports TMDS.TrackTagComponents
Imports TMDS.TrainComponents

Public Module GlobalDeclarations

#Region "ASSIST MODE"

    Public gResetEvent As Boolean
    Public optTrackTrainIdOnAssistMode As Boolean = False
#End Region

#Region "SHADOW MODE"

    Public optEnableShadowMode As Boolean
    Public optEnableTerritoryDeskAssignmentMode As Boolean
    Public gMyControllingControlPtGUID As New ArrayList
    Public ForceLoadMyControllingControlPoints As Boolean
#End Region

#Region "TROUBLE REPORT"

    Public gTroubleReports As New ArrayList()
    Public gSummaryTroubleReportQueue As Queue = Queue.Synchronized(New Queue())
    Public gTroubleReportCodes As New ArrayList()
    Public optTroubleReportName As String = "TROUBLE REPORT"
    Public gPublishTroubleReportStatus As Boolean = True
    Public gTroubleReportIncidentTypes As New ArrayList
    Public gTroubleReportIncidentSubTypes As New List(Of IncidentReportTypes)
    Public gTroubleReportIncidentNotification As New List(Of IncidentReportNotification)
    Public Enum TROUBLEREPORTSTATUS
        CREATE
        UPDATE
        CLOSE
        DELETE

    End Enum

    Public Class IncidentReportTypes
        Public UID As String = ""
        Public IRTypeName As String = ""
        Public IRSubType As String = ""
        Public DefaultExcludedEvent As Boolean = False
        Public DefaultDelayCode As String = ""
        Public RemedyMeasurementType As String = ""
        Public RemedyMeasurementValue As Integer = 0
        Public SendIRRequest As Boolean = False
        Public SendSRRequest As Boolean = False

        Public Function GetmySubTypes(IRType As String) As ArrayList
            Dim IRT As IncidentReportTypes = Nothing
            Dim SubTypesList As New ArrayList
            Try
                For Each IRT In gTroubleReportIncidentSubTypes
                    If IRT.IRTypeName.Trim.ToUpper = IRType.Trim.ToUpper Then
                        If Not SubTypesList.Contains(IRT.IRSubType.Trim.ToUpper) Then SubTypesList.Add(IRT.IRSubType.Trim.ToUpper)
                    End If
                Next
                Return SubTypesList
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return Nothing
        End Function
        Public Function GetIRT(IRType As String, IRSubType As String) As IncidentReportTypes
            Dim IRT As IncidentReportTypes = Nothing
            Dim SubTypesList As New ArrayList
            Try
                For Each IRT In gTroubleReportIncidentSubTypes
                    If IRT.IRTypeName.Trim.ToUpper = IRType.Trim.ToUpper AndAlso IRT.IRSubType.Trim.ToUpper = IRSubType.Trim.ToUpper Then
                        Return IRT
                    End If
                Next

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return Nothing
        End Function
        Public Function GetMyNotificationEntity(IRType As String) As ArrayList
            Dim IRTN As IncidentReportNotification = Nothing
            Dim SubTypesList As New ArrayList
            Try
                For Each IRTN In gTroubleReportIncidentNotification
                    If IRTN.NIRType.Trim.ToUpper = IRType.Trim.ToUpper Then
                        If Not SubTypesList.Contains(IRTN.NEntityID.Trim.ToUpper) Then SubTypesList.Add(IRTN.NEntityID.Trim.ToUpper)
                    End If
                Next
                Return SubTypesList
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return Nothing
        End Function
        Public Function GetMyNotification(EntityID As String) As IncidentReportNotification
            Dim IRTN As IncidentReportNotification = Nothing
            Try
                For Each IRTN In gTroubleReportIncidentNotification
                    If IRTN.NEntityID.Trim.ToUpper = EntityID.Trim.ToUpper Then
                        Return IRTN
                    End If
                Next
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return Nothing
        End Function
    End Class

    Public Class IncidentReportNotification
        Public NEntityID As String = ""
        Public NEntityName As String = ""
        Public NIRType As String = ""
    End Class

#End Region

#Region "DATABASE CONNECTION"

    Public Property gRepositories As Repositories
    Public gDatabaseServerStatic As SystemServer
    Public gDatabaseServerDynamic As SystemServer
    Public gDatabaseServerTest As SystemServer
    Public gDatabaseServerBackUp As SystemServer
    Public gDatabaseServerAuthority As SystemServer
    Public gDatabaseServerBulletin As SystemServer
    Public gDatabaseServerTrain As SystemServer
    Public gDatabaseServerRemote As SystemServer
    Public gDatabaseServerMovementPlanning As SystemServer
    Public gConnectionStringStatic As String
    Public gConnectionStringDynamic As String
    Public gConnectionStringAuthority As String
    Public gConnectionStringBulletin As String
    Public gConnectionStringTrain As String
    Public gConnectionStringMovementPlanning As String
    Public gConnectionStringRemoteDatabase As String
    Public gConnectionStringRemoteDynamic As String
    Public gDatabaseConnectionStatic As OleDbConnection
    Public gDatabaseConnectionDynamic As OleDbConnection
    Public gDatabaseConnectionAuthority As OleDbConnection
    Public gDatabaseConnectionBulletin As OleDbConnection
    Public gDatabaseConnectionTrain As OleDbConnection
    Public gDatabaseConnectionMovementPlanning As OleDbConnection
    Public gQueryTimeDelimiter As String
    Public gSystemDatabasePath As String = Application.StartupPath & "\LocalDatabase"
    Public gSystemDatabaseStaticFile As String = gSystemDatabasePath & "\SystemDatabaseStatic.mdb"
    Public gSystemDatabaseDynamicFile As String = gSystemDatabasePath & "\SystemDatabaseDynamic.mdb"
    Public gdbLogin As String = "tmds"
    Public gdbPassword As String = "tmds"
    Public gDatabaseDynamicTrainQueue As Queue = Queue.Synchronized(New Queue())
    Public gDatabaseDynamicQueue As Queue = Queue.Synchronized(New Queue())
    Public gDatabaseStaticQueue As Queue = Queue.Synchronized(New Queue())
    Public gDatabaseDynamicMovementPlanningQueue As Queue = Queue.Synchronized(New Queue())
    Public gDatabaseDynamicTrainConn As OleDbConnection
    Public gDatabaseDynamicConn As OleDbConnection
    Public gDatabaseStaticConn As OleDbConnection
    Public gDatabaseDynamicMovementPlanningConn As OleDbConnection
    Public gDatabaseAdminEventQueue As Queue = Queue.Synchronized(New Queue())
    Public optEnableAdminEventLogging As Boolean
    Public gSLFATimer As String()
    Public Enum DatabaseConnectionType
        EVENTLOGDB
        STATICDB
        DYMANICDB
        MPARDB
        ALL
    End Enum

#End Region

#Region " GLOBAL COMPONENTNAME VARIABLE "
    Public ReadOnly gElectricalSwitchComponentName As String = "TMDS.GraphicComponent.ElectricalSwitch"
    Public ReadOnly gGradeCrossingComponentName As String = "TMDS.GraphicComponent.GradeCrossing"
    Public ReadOnly gSignalComponentName As String = "TMDS.SignalComponents.Signal"
    Public ReadOnly gTrackComponentName As String = "TMDS.TrackComponents.Track"
    Public ReadOnly gSwitchComponentName As String = "TMDS.SwitchComponents.Switch"
    Public ReadOnly gDeviceComponentName As String = "TMDS.GraphicComponent.Device"
    Public ReadOnly gFlipFlopComponentName As String = "TMDS.TrackOtherComponents.FlipFlop"
    Public ReadOnly gControlPointComponentName As String = "TMDS.GraphicComponent.ControlPoint"
    Public ReadOnly gRRXINGComponentName As String = "TMDS.TrackOtherComponents.RRXING"
    Public ReadOnly gTagComponentName As String = "TMDS.TrackTagComponents.Tag"
    Public ReadOnly gTTComponentName As String = "TMDS.TrackAuthority.TrackAuthority"
    Public ReadOnly gDBLKComponentName As String = "TMDS.TrackAuthority.DeviceBlock"
    Public ReadOnly gSFAComponentName As String = "TMDS.TrackAuthority.SFAuthority"
    Public ReadOnly gOtherSplAuthorityName As String = "TMDS.TrackAuthority.OtherSpecialAuthorityType"
    Public ReadOnly gSlideZoneComponentName As String = "TMDS.GraphicComponent.SlideZone"
    Public ReadOnly gDefectDetectorComponentName As String = "TMDS.GraphicComponent.FED"
    Public ReadOnly gTabularGeneralBulletinException As String = "TabularGeneralBulletinException"
    Public ReadOnly gTrainComponentName As String = "TMDS.TrainComponents.TrainID"

    Public ReadOnly gSFAAuthorityTypeItem2 As String = "ItemTwoToTrain"
    Public ReadOnly gSFAAuthorityTypeItem4 As String = "ItemFourToTrain"
    Public ReadOnly gSFAAuthorityTypeItem4MOW As String = "ItemFourToMOW"

    '"TMDS.TrackOtherComponents.RRXING"

    'Bulletin
    Public ReadOnly gSubBulletinName As String = "TMDS.GeneralBulletins.SubBulletin"
    Public ReadOnly gTGBOExceptionName As String = "TMDS.GeneralBulletins.TabularGeneralBulletinException"
    Public ReadOnly gWeatherStationTypeName As String = "TMDS.GraphicComponent.WeatherStation"
    Public ReadOnly gPassengerStationComponentName As String = "TMDS.GraphicComponent.PassengerStation"


    Public Enum TMDSAuthorityType
        TNT
        SFA
        SPL
        NONE
    End Enum

#End Region

#Region " GLOBAL TIMEZONE VARIABLE "
    'Mountain Standard Time
    'Eastern Standard Time
    'Pacific Standard Time
    'Central Standard Time
    'Canada Central Standard Time -> SASKATCHEWAN
    Public ReadOnly ESTimeZone As String = "Eastern Standard Time"
    Public ReadOnly CSTimeZone As String = "Central Standard Time"
    Public ReadOnly MSTimeZone As String = "Mountain Standard Time"
    Public ReadOnly PSTimeZone As String = "Pacific Standard Time"
    Public ReadOnly SaskatchewanSTimeZone As String = "Canada Central Standard Time"
    Public ReadOnly AlaskaSTimeZone As String = "Alaskan Standard Time"

    Public optEnableTimeZoneSettings As Boolean
#End Region

#Region "EVENT LOGGER"

    Public gSystemEventLogQueue As New Queue()
    Public gSystemEventLogThread As Threading.Thread
    Public gEventLoggerServer As New SystemServer("127.0.0.1")
    Public gConnectionStringEventLog As String
    Public gTestConnectionStringEventLog As String = ""
    Public gEventLogDatabaseName As String = "tmdsEventLogDatabase_" & Format(Now, "MMddyyyy")
    Public gEventLogDatabaseConnectErrorCount As Integer
    Public gEventLogServerLoaded As Boolean
    Public gEnableEventQueueValidation As Boolean = False
#End Region

#Region "Database  Queries"

    Public gTrackSelectQuery As String = "SELECT * FROM tblComponents WHERE (Type = 'T');"
    Public gSignalSelectQuery As String = "SELECT * FROM tblComponents WHERE (Type = 'L') OR (Type = 'R') OR (Type = 'U') OR (Type = 'D');"
    Public gDeviceSelectQuery As String = "SELECT * FROM tblCompMiscDevices WHERE (Type = 'D');"
    Public gSwitchSelectQuery As String = "SELECT * FROM tblComponents WHERE (Type = LIKE 'S%');"
    Public gRRXINGSelectQuery As String = "SELECT * FROM tblCompRRXings WHERE (Type = '+');"
    Public gFlipFlopSelectQuery As String = "SELECT * FROM tblCompFlipFlops WHERE (Type = 'F');"
    Public gDispatcherSelectQuery As String = "SELECT * FROM " & gtblUsers & " ORDER BY LastName"
    Public gTerritorySelectQuery As String = "SELECT * FROM tblTerritoryList WHERE Name <> 'ALL SUBDIVISIONS' ORDER BY UID;"
    Public gStationSelectQuery As String = "SELECT * FROM tblComponents WHERE (Type = 'N');"
    Public gAlphaTextSelectQuery As String = "SELECT * FROM tblComponents WHERE ((Type = 'X') or (Type = 'A'));"
    Public gAuthPointsSelectQuery As String = "SELECT * FROM tblComponents WHERE ((Type = 'X'));"
    Public gBitMapSelectQuery As String
    Public gFEDSelectQuery As String
    Public gSPMSelectQuery As String
    Public gPFDSelectQuery As String
    Public gPlatformSelectQuery As String = ""
#End Region

#Region "Database Table Definitions"

    'Dynamic Database Tables
    Public gtblAuthActive As String = "tblAuthoritiesActive"
    Public gtblAuthInActive As String = "tblAuthoritiesInActive"

    Public gtblAuthActiveDTC As String = "tblAuthoritiesActiveDTC"
    Public gtblAuthInActiveDTC As String = "tblAuthoritiesInActiveDTC"

    Public gtblSFAActive As String = "tblAuthoritiesSingleFormActive"
    Public gtblSFAInActive As String = "tblAuthoritiesSingleFormInActive"

    Public gtblBullActive As String = "tblBulletinsActive"
    Public gtblBullInActive As String = "tblBulletinsInActive"
    Public gtblBullInGTB As String = "tblBulletinsInGTB"

    Public gtblBlkActive As String = "tblDeviceBlocksActive"
    Public gtblBlkInActive As String = "tblDeviceBlocksInActive"

    Public gtblGTBActive As String = "tblGeneralBulletinsActive"
    Public gtblGTBInActive As String = "tblGeneralBulletinsInActive"

    Public gtblMOWActive As String = "tblMowIDsActive"
    Public gtblMOWInActive As String = "tblMowIDsInActive"

    Public gtblNotesActive As String = "tblNotesActive"
    Public gtblNotesInActive As String = "tblNotesInActive"

    Public gtblTagsActive As String = "tblTrackTagsActive"
    Public gtblTagsInActive As String = "tblTrackTagsInActive"

    Public gtblTrainsActive As String = "tblTrainsActive"
    Public gtblTrainsInActive As String = "tblTrainsInActive"

    Public gtblTrainSheetActive As String = "tblTrainsActiveSubTrainSheets"
    Public gtblTrainSheetInActive As String = "tblTrainsInActiveSubTrainSheets"

    Public gtblWarrantsActive As String = "tblWarrantsActive"
    Public gtblWarrantsInActive As String = "tblWarrantsInActive"
    Public gtblItem20Switches As String = "tblWarrantItem20Switches"

    Public gtblDispEvents As String = "tblDispatcherLog"
    Public gtblSubEvents As String = "tblSubdivisionEvents"

    Public gtblEngineers As String = "tblSystemEngineers"
    Public gtblConductors As String = "tblSystemConductors"
    Public gtblLocoIDs As String = "tblSystemEngineIDs"
    Public gtblForemen As String = "tblSystemForemen"

    Public gtblWeatherAlertsActive As String = "tblWeatherAlertsActive"
    Public gtblWeatherAlertsInActive As String = "tblWeatherAlertsInActive"

    Public gtblJournalActive As String = "tblJournalEntriesActive"
    Public gtblJournalInActive As String = "tblJournalEntriesInActive"

    'static tables
    Public gtblWrkStnSettings As String = "tblSystemWrkStnSettings"
    Public gtblWrkStn As String = "tblSystemWorkStations"
    Public gtblUsers As String = "tblSystemUsers"
    Public gtblStations As String = "tblSystemTimeTableStations"
    Public gtblSubdivisions As String = "tblSystemSubdivisions"
    Public gtblServers As String = "tblSystemServers"
    Public gtblDelayCode As String = "tblDelayCodes"

    'Special Authority Types
    Public gtblOtherSpecialAuthActive As String = "tblAuthoritiesOtherActive"
    Public gtblOtherSpecialInActive As String = "tblAuthoritiesOtherInActive"


    'GTB EXCEPTION TABLE
    Public gtblGTBExceptionActive As String = "tblSystemBulletinExceptions"
    Public gtblGTBExceptionInActive As String = "tblSystemBulletinExceptionsInActive"

    Public gtblAlertsActive As String = "tblAlertsActive"
    Public gtblAlertsInActive As String = "tblAlertsInActive"

#End Region

    Public gThreadsCollection As New ArrayList()

#Region "System Component Classes"
    Public gRailroadInformation As RailroadInformation

    Public gDataLoaded As Boolean
    Public gInitialized As Boolean
    Public gInitializedCodeServers As Boolean
    Public gInitializedControlServers As Boolean
    Public gComponentsCollection As New ArrayList()
    Public gTerritoryComponentsCollection As New ArrayList()
    Public gFlipFlopCollection As New ArrayList
    Public gTrackRRName As New ArrayList()

    Public TrackComponent(0) As Track
    Public TrackCollection As New ArrayList()
    Public SignalComponent(0) As Signal
    Public SignalCollection As New ArrayList()
    Public gSignalAutomaticCollection As New ArrayList()

    Public gPassengerStationCollection As New ArrayList()
    Public GradeCrossingCollection As New ArrayList()
    Public SlideZoneCollection As New ArrayList()
    Public ElectricalLineCollection As New ArrayList()
    Public ElectricalSwitchCollection As New ArrayList()
    Public WeatherStationCollection As New ArrayList()
    Public ElectricalConnectionCollection As New ArrayList()
    Public ElectricalCircuitCollection As New ArrayList()

    Public DeviceComponent(0) As Device
    Public DeviceCollection As New ArrayList()

    Public SwitchComponent(0) As Switch
    Public SwitchCollection As New ArrayList()

    Public DetectorCollection As New ArrayList()

    Public gControlPointCollection As New ArrayList()

    Public BitmapPictureCollection As New ArrayList()
    Public AlphaComponent(0) As AlphaText
    Public TrackWarrants As New ArrayList()
    Public OtherTrackAuthorities As New ArrayList()
    Public gDeviceBlocks As New ArrayList()
    Public MOWCollection As New ArrayList()
    Public Notes As New ArrayList()
    Public gJournals As New ArrayList()

    Public ControlledTerritories As New ArrayList()
    Public TerritoryControl As Territory()
    Public ControlledSubdivisions As New ArrayList()
    Public AvailableFaxLocations As New ArrayList()

    Public gCurrentUser As New UserLogin()
    Public gCurrentDeskConfiguration As String
    Public gPreviousDispatcherEvent As String
    Public gPreviousDispatcher As String
    Public gPreviousDispatcherEventTime As Date
    Public gPreviousDispatcherInitial As String = ""
    Public gPreviousCPDispatcher As String = ""

    Public gWorkStationName As String

    Structure ScreenInformation
        Dim Name As String
        Dim Bounds As Rectangle
        Dim WorkingBounds As Rectangle
        Dim Type As String
        Dim PrimaryScreen As Boolean
    End Structure
    Public SystemScreens() As ScreenInformation

    Public Snippets(0) As ScreenDrawingRegion
    Public TempSnippets As New ArrayList()
    'Public TempSnippets(0) As ScreenDrawingRegion
    Public gFurtherestX As Int32
    Public gFurtherestY As Int32

    Public FlashCollection As ArrayList = ArrayList.Synchronized(New ArrayList()) 'As New ArrayList()
    Public gFlashSelection As Boolean
    Public gSystemReDrawing As Boolean

    Public gScreenResolution As Size

    Public gTrainMenuMiscSelectedTrack As Track
    Public gTrainMenuMiscSelectedTrainID As TrainID

    Public gMowMenuMiscSelectedTrack As Track
    Public gMowMenuMiscSelectedID As MOWID

    Public ColorList As New ArrayList
    Public gSystemSumForeColor As Color = Nothing
    Public gSystemSumBackColor As Color = Nothing

    Public gPreferencesUserUID As String = ""
    Public IsOktoCloseSystemSummary As Boolean = False
    Public gForemanMenuMiscSelectedID As Foreman = Nothing

    Public gSPMSwitchCollection As New List(Of Switch)

    Public gInUseComponentCollection As ArrayList = ArrayList.Synchronized(New ArrayList())

#End Region

#Region "Train ID Variables"
    ' Public gTrainMoveOfficeLock As Integer = -3
    Public optIdentifyEngineNumericLength As Integer = 4
    Public gEnableTrainTimerRefresh As Boolean
    Public gEnableTrainTagTimerRefresh As Boolean
    Public gCancelLicenseToTrain As Boolean
    Public gRemoveLicenseTrainSelectedID As TrainID
    Public gOSTrain As Boolean
    Public gPlaceOSTrain As Boolean
    Public gAddDelayTrain As Boolean
    Public gPositionTrainID As Boolean
    Public gModifyTrainID As Boolean
    Public gMoveTrainID As Boolean
    Public gMoveMOWID As Boolean
    Public gMoveTrainSelected As Boolean
    Public gMoveMOWSelected As Boolean
    Public gMoveTrainSelectedTrack As Track
    Public gMoveTrainFromTrainStackForm As Boolean
    Public gMoveMOWSelectedTrack As Track
    Public gMoveMOWSelectedID As MOWID
    Public gMoveTrainSelectedID As TrainID
    Public gRouteCheckTrainSelectedID As TrainID
    Public gSendOSMessageFromReportClearPositionTWC As Boolean
    Public gFinalTrain As Boolean
    Public gCreateTrain As Boolean
    Public gViewTrain As Boolean
    Public gDisplayPseudo As Boolean
    Public gDisplaySymbol As Boolean
    Public gDisplayEngineID As Boolean
    Public gDisplayAbbrev As Boolean
    Public gCreateMOWID As Boolean
    Public gDeleteMOWID As Boolean
    Public gTrainAbbreviation As New ArrayList()
    Public gSimulateTrain As Boolean
    Public gSimulateTrainPassRed As Boolean
    Public gSimulateTrainStop As Boolean
    Public gSelectedTrainIDTrack As Track
    Public gSummaryTrainQueue As Queue = Queue.Synchronized(New Queue())
    Public TrainCollection As New ArrayList() 'As ArrayList = ArrayList.Synchronized(New ArrayList())
    Public TrainCollectionForSplitDeskMRS As New ArrayList()
    Public TrainCollectionDummy As New ArrayList()
    Public gTrainSetRearEndToTrack As Boolean
    Public gTrainSetHeadEndToTrack As Boolean
    Public gStandAloneTrainSummaryQueue As Queue = Queue.Synchronized(New Queue())
    Public gStandAloneAuthoritySummaryQueue As Queue = Queue.Synchronized(New Queue())
    'Helper Train Variables
    Public gHelperTrainSelected As Boolean
    Public gLinkTrainSelectedTrack As Track
    Public gHelperTrainSelectedID As TrainID
    Public gLinkTrainSelectedID As TrainID
    'OTHER AUTHORITY PERMISSIONS
    Public gPermissionEnterMainTrack As Boolean
    Public gPermissionPassRedSignal As Boolean
    Public gPermissionPassRedSignalSwitchSelectionMode As Boolean
    Public gPermissionPassRedSignalSwitches As New ArrayList
    Public gPermissionPassRedSignalControlPoint As ControlPoint
    Public gPermissionPassRedSignalIncludeSiding As Boolean
    Public gTrainNotFound As String = "SYMBOL NOT FOUND - START OVER"
    Public gPermissionPassRedSignalSelectionCompleted As Boolean = False
    Public gAllowPSSSwitchSelection As Boolean = True
    Public gPermissionPassRedEntrySignal As Signal = Nothing
    'Delay
    Public OptEnableTrainDelayModify As Boolean = False
    Public optEnableSlowOrderTrainDelayGeneration As Boolean
    Public optEnableTrainDelayReportingLegacy As Boolean = True
    Public optEnableTrainDelayReporting As Boolean
    Public DeActivateTrainMenuText As String = "DE-ACTIVATE ENGINE"
    Public ActivateTrainMenuText As String = "ACTIVATE ENGINE"
    'SHADOW MODE OPTIONS
    Public optEnableShadowModeFilterForTrains As Boolean
    Public optSaveTrainEventsToEventLog As Boolean
    Public optDisableUnitDPStatusOnFinal As Boolean = False
    Public optDisableUnitPTCStatusOnFinal As Boolean = False
    Public optenableDirectionChangeBasedOnTracking As Boolean = False
    Public optEnableArchiveTrainSheetSummaryOnFinal As Boolean = False
    'SIMULATOR
    Public gSimPositionReport As Boolean = False
    Public gSimReleaseEot As Boolean = False
#End Region

#Region "RUMO"
    Public gManeuverLoadCount As Integer = 0
    Public gManeuverEmptyCount As Integer = 1
#End Region

#Region "Tag Variables"
    Public gCreateTag As Boolean
    Public gCreateTagType As String
    Public gCreateTagDataList As ArrayList
    Public gTagToCopy As Tag
    Public gCopyTag As Boolean
    Public TrackTags As New ArrayList()
    Public gDelayCodeList As New ArrayList()
    Public optLockRestrictiveTags As Boolean
    Public gSummaryTagQueue As Queue = Queue.Synchronized(New Queue())
    Public gTagTrackSelectionList As New ArrayList()
    Public gTagSelectionDirection As String = ""
    Public gTagLeftExitTrack As Integer
    Public gTagRightExitTrack As Integer
    Public optLockTagDirection As Boolean
#End Region

#Region "BULLETIN VARIABLES"
    Public gCreateBulletin As Boolean
    Public gCreateBulletinType As String
#End Region

#Region "DTC Warrant Variables"
    Public gCreateDTCWarrant As Boolean
    Public gCreateDTCWarrantToTrain As Boolean
    Public gCreateDTCWarrantToMOW As Boolean
    Public gRollUpDTCWarrant As Boolean
    Public gDTCComplete As Boolean
#End Region

#Region "MRS LICENSE Variables"
    Public gCreateLicenseDTC As Boolean
    Private pCreateLicenseTrain As TrainID
    Public Property gCreateLicenseTrain() As TrainID
        Get
            Return pCreateLicenseTrain
        End Get
        Set(ByVal value As TrainID)
            pCreateLicenseTrain = value
        End Set
    End Property
    Public gCancellingStartSignalForLicense As Signal
    Public optEnableTestTrainForPTC As Boolean
#End Region

#Region "ARRC TRAIN SELECTION"
    Public gCreateAuthorityTrain As TrainID
    Public gCreateAuthorityTypeTrain As String
    Public gCreateTrackAuthorityTrain As Boolean

    Public gCreateAuthorityMOW As MOWID
    Public gCurrentMOWTrack As Track
    Public gCreateAuthorityTypeMOW As String
    Public gCreateTrackAuthorityMOW As Boolean

#End Region

#Region "Global Track Warrant variables"
    Public gIssueToSelected As String
    Public gCreateTrackWarrant As Boolean
    Public gRollUpTrackWarrant As Boolean
    Public gSubdivisionDirection As Boolean
    Public gOverLapCheckNeeded As Boolean
    Public gOverLapValid As Boolean
    Public gWarrantType As String
    Public gCurrentTrackWarrantIndex As Integer
    Public gBoxTwoRequired As Boolean
    Public gBoxThreeRequired As Boolean
    Public gBoxFourRequired As Boolean
    Public gBoxSixRequired As Boolean
    Public gLaunchCreateItemTwoToTrain As Boolean
    Public gLaunchCreateItemFourToSameTrain As Boolean

    Public gItemSixRequired As Boolean
    Public gItemSevenRequired As Boolean

    Public gBoxTenRequired As Boolean
    Public gBoxOneRequired As Boolean
    Public gItemFiveRequired As Boolean
    Public gItemFourRequired As Boolean

    Public gItem10Required As Boolean
    Public gBox13Required As Boolean
    Public gBox14Required As Boolean
    Public gBox15Required As Boolean
    Public gBox17Required As Boolean
    Public gBox11Required As Boolean
    Public gBoxTwelveRequired As Boolean
    Public gBoxThirteenRequired As Boolean
    Public gBoxFourteenRequired As Boolean

    Public gRepeatMode As Boolean

    Private pJointAuthRequired As Boolean
    Public Property gJointAuthRequired() As Boolean
        Get
            Return pJointAuthRequired
        End Get
        Set(ByVal Value As Boolean)
            pJointAuthRequired = Value
        End Set
    End Property

    Public gBox8Required As Boolean
    Public gTypeOfPersonCopying As String
    Public gPersonCopying As String
    Public gTrackOccupancy As Boolean
    Public gDisplayTrackAuthorityNumbers As Boolean = True
    'Public gPrintWarrant As Boolean = True
    Public gItemFourReplacementString As String
    Public gDispatcherFormTitle As String
    Public gTextBoxDispLocation As Point
    Public gItemFourAddWorkToIssue As Boolean
    Public optLoadConductorSwitchDialog As Boolean
    Public gCrossingWarningIndex As Integer
    Public gCrossingMilepost As String
    Public gCrossingLocation As String
    Public gFirstTrackSelectionMousePoint As Integer

    Public gOverviewTWC As SummaryOverviewTWC

    Public gContingentTrackList As New ArrayList()
    Public gContingentMode As Boolean
    Public gBeginTrackSelectContingent As Track
    Public gEndTrackSelectContingent As Track

    Public gCurrentAuthorityFlash As Boolean
    Public gCurrentAuthorityList As ArrayList = ArrayList.Synchronized(New ArrayList())

    Public gCurrentAuthorityFlashShow As Boolean

    Public gItemTwoFiveVoidedAuthorityCandidate As SFAuthority

    Public gSwitchPointMonitoringExists As Boolean

    Public gItemTwoSameTrainWithRadioBlocking As Boolean
    Public gItemTwoToTrainWithRadioBlocking As Boolean

    Public gTrackSelectOrginatingOutOfSiding As Boolean
    Public gTrackSelectEndingIntoSiding As Boolean

    'MRL ITEM 2/6 MODE
    Public gDualAuthorityMode As Boolean

    'Stretchy Speed Tag Authority Check Variable
    Public gTagAuthorityCheckDone As Boolean

    Public optAllowAuthorityRemovalAlertFromWorkstation As Boolean = True
#Region "CP"

    Public gCreateTOP As Boolean
    Public gCreateCPTNT As Boolean
    Public gCreateCPFT As Boolean
    Public gEndCPTATSelection As Boolean = False
    Public gTNTISSUEToTrain As Boolean = False


#End Region

#End Region

#Region "TRACK ZONE VARIABLES"

    Public gTrackIntegrityMonitoringExists As Boolean
    Public gTrackZones As New ArrayList
    Public optShowTIWSShortenedLimits As Boolean = False

#End Region

#Region "Print Variables"

    Public optPerformDocumentArchiving As Boolean
    Public gPrintBulletinMode As Boolean = True
    Public gPrintGTBMode As Boolean
    Public gPrintWarrantMode As Boolean
    Public gBulletinFooterPage As String
    Public gBulletinFooterLastPage As String
    Public gBulletinHeaderPage As String
    Public gDispatcherTitle As String
    Public gPrintFaxDeviceList As New ArrayList()
    Public optEnablePrintTrainDelay As Boolean
    Public optEnablePrintTrainComment As Boolean
    Public optEnablePrintTrainUnitComment As Boolean
    Public optEnableAutoPrintTrainRelease As Boolean
    Public optEnableNoPrintSafetySpotLightRule As Boolean
    Public optEnableTNTVoidPrint As Boolean
#End Region

#Region "System variables"
    Public gSystemReleaseMode As Boolean = False
    Public optEnableMutipleTimeTable As Boolean = False
    Public optEnableNewAdminPasswordWindow As Boolean = False
    Public optApplyNonOccuppiedTrackIndicationChangeFirst As Boolean = False
    Public optAllow2560x1600ResolutionUse As Boolean
    Public OptSetSignalToUnsolitatedStateIfSlottedByOSTrack As Boolean = False
    Public OptOnSimulationModeClearOSEventonTrnMove As Boolean = False
    Public gCritcalPromptExists As Boolean
    Public gCreateNote As Boolean
    Public gSelectedNote As Annotation
    Public gCreateNonTag As Boolean
    Public gCreateDelayTag As Boolean
    Public gMoveNote As Boolean
    Public gPlaceNote As Boolean
    Public gMoveNoteSelectedNote As Annotation
    Public gDatabaseServerDown As Boolean
    Public gSystemLoggedIn As Boolean
    Public SystemEventQueue As Queue = Queue.Synchronized(New Queue())
    Public gDemoMode As Boolean = False
    Public gSimulationMode As Boolean
    Public gSimulationModeARS As Boolean
    Public gSimulationRecordMode As Boolean
    Public gSimulationModeRealTimeControl As Boolean
    Public gEnableScriptFeedToRTSCM As Boolean
    Public gMonitorMode As Boolean
    'Assit Mode
    Public gAssistMode As Boolean = False
    Public gOptEnableAssistModeAutoRefresh As Boolean = False
    Public gAssistModeAutoRefreshTimeinMinutes As Integer = -30
    Public gAssitModeRefreshInProgress As Boolean = False
    Public gLastAssistRefreshTime As Date = Now
    Public gMaintenanceMode As Boolean
    Public gDeviceSelectMode As Boolean
    Public gTestMode As Boolean
    Public gBackUpDatabaseServer As Boolean
    Public gStandAlone As Boolean
    Public gApplyChange As Boolean
    Public gShowEventLogWindow As Boolean
    Public gOverrideETMSValidation As Boolean
    Public gPTCBOSAPIMode As Boolean = False
    Public gOverrideTrainSheetDisable As Boolean = False
    'This will snuff my extra PSS switch finding (RPBJR)
    Public gForceMPAR As Boolean = False
    Public gThisWorkstation As TMDS.ManagementClasses.WorkStation
    Public gThisWorkstationTimeFactor As Integer
    Public gDebugPrint As Boolean = True
    Public gDebug As Boolean = False
    Public gStartTime As Date
    Public gDisplayFormCollection As New ArrayList()
    'This will snuff my extra indication debug messages (RPBJR)
    ' Public gDebugShowIndications As Boolean = False
    'This will snuff my extra remote track messages (RPBJR)
    'Public gDebugShowRemoteTrackInfo As Boolean = False
    'This will snuff my extra PSS switch finding (RPBJR)
    Public gDebugShowPSSInfo As Boolean = False
    Public gSystemFlasher As Threading.Thread
    Public gSplashScreenString As String = "LOADING SYSTEM RECORDS...."
    Public gSplashScreenProgressCount As Integer
    Public gDoDiagnosticCollection As Boolean
    Public gDoBackLogFiles As Boolean
    Public gPerformedDiagnosticCollection As Boolean
    Public gPerformedBackLogFiles As Boolean
    Public gBackUpFiles As Boolean
    Public gInitEventQueue As Boolean
    Public gEventLogSizeCheckedDate As Date = Now
    Public goptEnableCheckLogSize As Boolean = False
    Public gSystemSubdivisions As New ArrayList()
    Public gSearchAgain As Boolean
    Public gMouseButton As Integer
    Public PgSendCTC As Boolean

    Public Property gSendCTC() As Boolean
        Get
            Return PgSendCTC
        End Get
        Set(ByVal Value As Boolean)
            PgSendCTC = Value
        End Set
    End Property

    Public gRemoteCPFailGUIDS As New ArrayList
    Public gPTCLinkDown As Boolean = False
    Public gDisableTerritoryControlPublishingToDatabase As Boolean
    Public gEnableCPAuthorityView As Boolean = False
    Public gRollUpComponentCount As Integer = 500
    'SQL Injection boolean
    Public gProtectFromInjection As Boolean = False
    'Developer Settings
    Public gDeveloperSystem As Boolean = False
    Public optEnableResizeOfDisplayScreen As Boolean = False
    'ShortCut Keys
    Public gEnableCtrlShiftF As Boolean = False
    'Snow Melter Timer
    Public optEnableCustomSnowMelterTimer As Boolean = False
    Public optEnableStartStopAllSnowMelter As Boolean = False
    'SPM SWITCH COLLECTION
    Public optEnableSPMSwitchCollection As Boolean = False
    Public gDisplayHighwayCrossing As Boolean = False
    Public gDisplaySlideZone As Boolean = False
    Public gDisplayTPMScomponents As Boolean = False
    Public gDisplayAuxSwitches As Boolean = False
    Public gDisplayFED As Boolean = False
    'SIGNAL
    Public gDisplaySignals As Boolean = True
    Public gDisplayAutomaticSignals As Boolean
    'TRAIN
    Public gOptSortOSTimesDesc As Boolean = False
    Public OptEnableTrainCommentSaveOnPostion As Boolean = False
    Public optEnableTrainDelayOnTrackTag As Boolean
    'Simulate Train Movement
    Public OptEnableSimulateTrainMovement As Boolean
    Public OptEnableSimulateTrainPassRedSignal As Boolean
    'TAGs
    'Device BLOCK
    Public OptDisableSwitchBlkOnTWCSwitches As Boolean = False
    'MOW
    'TRACK LIST
    Public gTrackNameList As New ArrayList
    'COMM CONNECTION DOWN MESSAGE
    Public optEnableCADCommConnectionDownMessage As Boolean
    Public optEnableGPSCommConnectionDownMessage As Boolean
    Public optEnableMISCommConnectionDownMessage As Boolean
    'CONTROL SERVER CONNECTION DOWN MESSAGE
    Public optEnableControlServerConnectionDownMessage As Boolean
    Public ControlSvrConnectionFailedList As New ArrayList
    'Alerts
    Public optEnableInsertAllAlertsIntoDB As Boolean = False
#End Region

#Region "DISPACTHER ASSITS SETTINGS "
    Public OptAllowAssistTrainPositionOnNonMainTracksOnly As Boolean = False
#End Region

#Region "TRAINER/TRAINEE MODE"

    Public gTrainerMode As Boolean
    Public gTraineeMode As Boolean

#End Region

#Region "Track Selection Global Variables"
    'Public gBeginTrackSelection As Integer
    'Public gEndTrackSelection As Integer

    Public gBeginTrackSelect As Track
    Public gEndTrackSelect As Track

    Public gEndTrackSelectionReplacement As Track
    Public gSearchDirection As String = ""
    Public gSearchDirectionLocked As String = ""
    Public gChangeTraffic As Boolean
    Public gChangeOccupancy As Boolean
    Public gRemoveLicense As Boolean

    'replaced this collection with home made collection
    'Public gTrackSelectionCollection As New ComponentSelectionCollection()
    Public gTrackSelectionCollection As New ArrayList()
    Public gTrackSelectionFlash As Boolean

    Public gTrackEntExtSelectionFlash As Boolean
    Public gTrackAuthSelectionFlash As Boolean
    Public gTrackAuthSelectionFlashOn As Boolean

    Public PSSHandeOperationRequiredNOOption As Integer = 0
    Public gFirstOSTrack As Track

    Public gTracksDown As New List(Of Integer)
    Public gPerformingReverseSearch As Boolean = False
    Public gTrackLinkedReverse As Boolean = False
    Public gTrackLinkedReverseDirection As String = ""

    Public gOptDisableTrackSelectionOnTurnOut As Boolean
    Public gOptDisableTrackSelectionOnNonTraversable As Boolean

#End Region

#Region "Cursor Declarations"

    Public CursorPath As String = Application.StartupPath & "\Library\SystemCursors"
    Public curCreateTW As Cursor
    Public curRollUpTW As Cursor
    Public curDefault As Cursor
    Public curPositionID As Cursor
    Public curMove As Cursor
    Public curPlaceNote As Cursor
    Public curMoveNote As Cursor
    Public curNote As Cursor
    Public curOccupyTrack As Cursor
    Public curOSTrain As Cursor
    Public curSelectID As Cursor
    Public curPlaceTag As Cursor
    Public curSend As Cursor
    Public curSimulateID As Cursor
    Public curCreateTT As Cursor
    Public curCreateFT As Cursor
    Public curCreateTP As Cursor
    Public curCreateDB As Cursor
    Public curDevice As Cursor
    Public curEntExit As Cursor
    Public curQuickRoute As Cursor
    Public curReCall As Cursor
    Public curReSend As Cursor
    Public curChangeTraffic As Cursor
    Public curChangeOcc As Cursor
    Public curLinkTrainID As Cursor
    Public curStackRoute As Cursor
    Public curGSR As Cursor

    Public curCreateDTC As Cursor
    Public curRollUpDTC As Cursor
    Public curCreateTTContinue As Cursor
    Public curTNTRollUp As Cursor
    Public curCopyTag As Cursor
    Public curWait As Cursor
    Public curEnterMainTrack As Cursor
    Public curPassRedSignal As Cursor
    Public curPassRedSignalSW As Cursor
    Public curCancelLicense As Cursor
    Public curTrackAuthority As Cursor

    Public curContingentSelection As Cursor
    Public curCreatePlan As Cursor
    Public curCreateTrackReseveration As Cursor
    Public curCreateManualRoute As Cursor

    Public curItemSixSelection As Cursor


    Public curCreateTOP As Cursor
    Public curCreate564 As Cursor
    Public curCreateClearanceProceed As Cursor
    Public curClearTW As Cursor

    Public curCompleteBy As Cursor

    Public curBulletin As Cursor

#End Region

#Region "Sound Declarations"
    Public gWavePath As String = Application.StartupPath & "\Library\SystemSounds"
    Public WaveDefault As String = ""
    Public WaveStop As String = ""
    Public WaveDing As String = ""
    Public WaveHorn As String = ""
#End Region

#Region "Query Declarations"
    Public gPersonCopyingQuery As String
#End Region

#Region "Form Property Variables"

    Public FrmMainMDIProp As New FormProperties()
    Public FrmPrintProp As New FormProperties()
    Public FrmLoginProp As New FormProperties()
    Public FrmCopyToProp As New FormProperties()
    Public FrmAuthSumProp As New FormProperties()
    Public FrmReportClearProp As New FormProperties()
    Public FrmTrackWarrantProp As New FormProperties()
    Public FrmTerritoryListProp As New FormProperties()
    Public FrmTrnSumProp As New FormProperties()
    Public FrmBullSumProp As New FormProperties()
    Public FrmBulletinProp As New FormProperties()
    Public FrmGTBProp As New FormProperties()
    Public FrmSysSumProp As New FormProperties()
    Public FrmTrackTimeProp As New FormProperties()
    Public FrmTrackAuthIssueToProp As New FormProperties()
    Public FrmDeviceBlkRemoveProp As New FormProperties()
    Public FrmDeviceBlkProp As New FormProperties()
    Public FrmTagProp As New FormProperties()
    Public FrmMessageBoxProp As New FormProperties()
    Public FrmTrnSchSumProp As New FormProperties()

#End Region

#Region "File and Paths"

    Public gEventLogFile As String
    Public gWorkStationSettingsFile As String


#End Region

#Region "TGBO Sub-Bulletin Variables "
    Public OptLogTGBO_GBORefresh As Boolean = False
    Public TGBORefreshStarted As Boolean = False
    Public GBORefreshStarted As Boolean = False

    Public BulletinCollection As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public GTBCollection As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gFormatMilePostToTenths As Boolean = True
    Public gShowGeneralBulletinTypeVerbiage As Boolean
    Public gPrintBulletins As Boolean = True
    Public gShowAllSubdivisions As Boolean
    Public gDisplayBulletinsOnTrack As Boolean = True
    Public gPublishBulletinStatus As Boolean = True
    Public optSlideZoneBulletinSpeedFreight As Integer = 15
    Public optSlideZoneBulletinSpeedPassenger As Integer = 15
    Public optSlideZoneBulletinSpeedExpedite As Integer = 15
    Public optPromptFormBOnSwitchReverse As Boolean
    Public optLogBulletinEventOnTrain As Boolean
    Public optEnableAllSubdivisionsGTB As Boolean
    Public optUseDefinedTrackNameForBulletins As Boolean = False
    Public optUseMOWIDAndName As Boolean = False
    Public optTBAutoConfirmFormBandFormO As Boolean = False
    Public optTBAutoExpireFormB As Boolean = False
    Public optTBAutoExpireOfBulletins As Boolean = False
    Public DisableVBCRLFReplaceOnTB As Boolean = False
    Public optTBAutoExpireCheckInMins As Integer = -60
    Public optEnableNonControlledTrackCrossing As Boolean = False
    Public optEnableBulletinDirection As Boolean = False

    Public gmstrFormOTrackBlockedWithEquipment As String = "Track Blocked With Equipment"
    Public gmstrFormOTrackOutOfService As String = "Track Out Of Service"

    Public Enum BULLETINSTATUS
        CREATE
        DELETE
        CANCEL
        UPDATE
        REFRESH
        EXCEPTION
    End Enum

#End Region

#Region "TGBO Variable "
    Public optUseTGBOSummaryForm As Boolean = False
    Public optEnableTGBO As Boolean = False
    Public MOWTGBOCreateTime As String = "02:00"
    Public optEnableTrainMenuTGBO As Boolean
    Public optEnableTrainMenuTGBORelease As Boolean = True
    Public optEnableActivatePendingTGBOToReleaseActiveTGBO As Boolean = True
    Public OptDisableMowTGBOTimerDelete As Boolean = False
    Public gTGBOCommandText As String = "TGBO COMMANDS"
    Public OptDisableMowTGBOTimerCreate As Boolean = False
    Public gTGBOAddressedToRailRoadName As String = ""
    Public optEnableCreateMultipleTrainAssociationGTB As Boolean = False
    Public optEnableBulletinSortForOnboard As Boolean = False
    Public optCheckPTCStatusOnTGBORelease As Boolean = False
    Public optEnableBulletinMileageRangeCheckForTGBO As Boolean = False
    Public optEnableTGBOChangeDesignationTransmission As Boolean = False
    Public optEnableTGBOUpdateOnException As Boolean = False
    Public optEnableNewTGBODOB As Boolean = False

    Public gTGOBTypeTrain As String = "TGBO-TRAIN"
    Public gTGOBTypeConductor As String = "TGBO-CONDUCTOR"
    Public gTGOBTypeDOB As String = "TGBO-DOB"
    Public gTGOBTypeMOW As String = "TGBO-MOW"
    Public gTGOBTypeForemen As String = "TGBO-FOREMAN"
    Public gTGOBTypeSubDivision As String = "TGBO-SUBDIVISION"
    Public gTGOBTypeOffice As String = "TGBO-OFFICE"

#End Region

#Region "CTC Variables"
    Public gDisplayFeildBlockFailure As Boolean = False
    Public gDisplayTNTAuthorityNumbers As Boolean = False
    Public gCTCTerritoryFound As Boolean
    Public gEntExit As Boolean
    Public gAutoMaticControl As Boolean
    Public gCallOn As Boolean
    Public gFleet As Boolean
    Public gReCall As Boolean
    Public gReSend As Boolean
    Public gSelectedSignal As Signal
    Public gSelectedSignalUserClickedinStop As Boolean
    Public gSwitchListForAutoRevereseForTNT As New ArrayList

    Public gTmpRouteCollection As New ArrayList
    Public gTmpRouteSignalCollection As New ArrayList
    Public gEntExtTrackSelection As New ArrayList()
    Public gEntExtExecutionSelection As New ArrayList()
    Public gEntryTrack As Track
    Public gFirstEntryTrack As Track
    Public gExitTrack As Track
    Public gQuickRouteMode As Boolean

    Public CtlIndBits As New ArrayList()
    Public CodeServers As New ArrayList()
    Public ControlServers As New ArrayList()
    Public Codelines As New ArrayList()
    Public CodeStations As New ArrayList()

    'These are to send controls from selected components
    Public gSendControlCollection As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gReCallCollection As New ArrayList()
    Public gSendControlCollectionThread As Threading.Thread
    Public gSendRecallCollectionThread As Threading.Thread

    'These are to build GUID references from the component ctl/ind positions
    'to the literal code bit assignment. These get built and assigned on the territroy load event
    Public gCtlIndBitMapNumber As Int32



#End Region

#Region "PASS STOP SIGNAL VARIABLES "

    Public ForcedSwitch As Object = Nothing
    Public ForcedSwitchCollection As New ArrayList
    Public ExemptTrack As Object = Nothing
    Public ExemptSignal As Object = Nothing
    Public PSSOriginalCompCollection As New ArrayList
    Public ExemptTrackCollection As New ArrayList
    Public ExemptSignalCollection As New ArrayList
    Public ValidatedSignal As Object = Nothing
    Public PSSEntrySignal As Object = Nothing
    Public PSSSelectedSwitchCollection As New ArrayList
    Public PSSNonSelectedSwitchCollection As New ArrayList
    Public PSSReversedSwitchCollection As New ArrayList
    Public PSSOriginalEntryTrack As Track = Nothing
    Public PSSOriginalExitTrack As Track = Nothing
    'Added so that I can search for missing signals (BNSF) (RPBJR)
    Public PSSEndSignals As New ArrayList
    Public PSSSameMain As Boolean = True
    Public PSSVerticalTrack As Boolean = False
    Public PSSDirection As String = String.Empty

    Public gOtherSpecialAuthorityCollection As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public OptEnableMutiplePSSToATrain As Boolean = False
    Public OptEnableEnhancedPSS As Boolean = False

    ''' <summary>
    ''' Structure to manage component directions for PSS Blocking (BNSF) (RPBJR)
    ''' </summary>
    ''' <remarks></remarks>
    Public Structure ComponentDirection

        Private m_CurrentDirection As TMDSComponentDirection
        Private m_VerticalDirection As TMDSComponentDirection

        Public Property CurrentDirection() As TMDSComponentDirection
            Get
                Return m_CurrentDirection
            End Get
            Set(ByVal value As TMDSComponentDirection)
                Select Case value

                    Case TMDSComponentDirection.LEFT
                        m_VerticalDirection = TMDSComponentDirection.UP
                    Case TMDSComponentDirection.DOWN
                        m_VerticalDirection = TMDSComponentDirection.DOWN
                    Case Else
                        m_VerticalDirection = value

                End Select
            End Set
        End Property

        Public ReadOnly Property VerticalDirection() As TMDSComponentDirection
            Get
                Return m_VerticalDirection
            End Get
        End Property


        ''' <summary>
        ''' Gets the basic direction from Alt directions. (RPBJR)
        ''' </summary>
        ''' <param name="inbound_direction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetSimpleDirection(ByVal inbound_direction As TMDSComponentDirection) As TMDSComponentDirection

            Dim newDirection As TMDSComponentDirection

            Select Case inbound_direction

                Case TMDSComponentDirection.LEFTALT
                    newDirection = TMDSComponentDirection.LEFT
                Case TMDSComponentDirection.LEFT
                    newDirection = TMDSComponentDirection.LEFT
                Case TMDSComponentDirection.RIGHTALT
                    newDirection = TMDSComponentDirection.RIGHT
                Case TMDSComponentDirection.RIGHT
                    newDirection = TMDSComponentDirection.RIGHT
                Case Else
                    newDirection = inbound_direction

            End Select

            Return newDirection

        End Function

        ''' <summary>
        ''' Flips the inbound direction. (RPBJR)
        ''' </summary>
        ''' <param name="current_direction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetOppositeDirection(ByVal current_direction As TMDSComponentDirection) As TMDSComponentDirection

            Dim newDirection As TMDSComponentDirection = TMDSComponentDirection.LEFT

            Select Case current_direction

                Case TMDSComponentDirection.DOWN
                    newDirection = TMDSComponentDirection.UP
                Case TMDSComponentDirection.UP
                    newDirection = TMDSComponentDirection.DOWN
                Case TMDSComponentDirection.LEFT
                    newDirection = TMDSComponentDirection.RIGHT
                Case TMDSComponentDirection.RIGHT
                    newDirection = TMDSComponentDirection.LEFT
                Case TMDSComponentDirection.LEFTALT
                    newDirection = TMDSComponentDirection.RIGHTALT
                Case TMDSComponentDirection.RIGHTALT
                    newDirection = TMDSComponentDirection.LEFTALT

            End Select

            Return newDirection

        End Function

        ''' <summary>
        ''' Gets the vertical direction based off a horizontal direction (or sometimes already vertical) (RPBJR)
        ''' </summary>
        ''' <param name="horizontal_direction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetVerticalDirection(ByVal horizontal_direction As TMDSComponentDirection) As TMDSComponentDirection

            Dim verticalDirection As TMDSComponentDirection = TMDSComponentDirection.DOWN

            If horizontal_direction = TMDSComponentDirection.LEFT Then
                verticalDirection = TMDSComponentDirection.UP
            ElseIf horizontal_direction = TMDSComponentDirection.RIGHT Then
                verticalDirection = TMDSComponentDirection.DOWN
            ElseIf horizontal_direction = TMDSComponentDirection.UP Then
                verticalDirection = TMDSComponentDirection.UP
            Else
                verticalDirection = TMDSComponentDirection.DOWN
            End If

            Return verticalDirection

        End Function

        ''' <summary>
        ''' Changes a direction string to an enum value (L = LEFT) (RPBJR)
        ''' </summary>
        ''' <param name="direction_string"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetDirectionEnumFromString(ByVal direction_string As String) As TMDSComponentDirection

            Dim returnDirection As TMDSComponentDirection = TMDSComponentDirection.LEFT

            Try
                Select Case direction_string.Trim

                    Case "L"
                        returnDirection = TMDSComponentDirection.LEFT
                    Case "R"
                        returnDirection = TMDSComponentDirection.RIGHT
                    Case "U"
                        returnDirection = TMDSComponentDirection.UP
                    Case "D"
                        returnDirection = TMDSComponentDirection.DOWN
                    Case "RIGHT"
                        returnDirection = TMDSComponentDirection.RIGHT
                    Case "LEFT"
                        returnDirection = TMDSComponentDirection.LEFT
                    Case Else
                        returnDirection = TMDSComponentDirection.LEFT

                End Select

            Catch ex As Exception
                LogEvent(ex.ToString)
            End Try

            Return returnDirection

        End Function

        ''' <summary>
        ''' Takes in a direction (LEFT/RIGHT/LEFTALT/RIGHTALT) and returns its alternate. (RPBJR)
        ''' </summary>
        ''' <param name="simple_direction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetAlternateDirection(ByVal simple_direction As TMDSComponentDirection) As TMDSComponentDirection

            Dim alternateDirection As TMDSComponentDirection = TMDSComponentDirection.LEFTALT

            Select Case simple_direction

                Case TMDSComponentDirection.LEFT
                    alternateDirection = TMDSComponentDirection.LEFTALT
                Case TMDSComponentDirection.RIGHT
                    alternateDirection = TMDSComponentDirection.RIGHTALT
                Case TMDSComponentDirection.LEFTALT
                    alternateDirection = TMDSComponentDirection.LEFT
                Case TMDSComponentDirection.RIGHTALT
                    alternateDirection = TMDSComponentDirection.RIGHT
                Case Else
                    alternateDirection = simple_direction

            End Select

            Return alternateDirection

        End Function

        ''' <summary>
        ''' Checks to see if this is either LEFTALT or RIGHTALT. (RPBJR)
        ''' </summary>
        ''' <param name="current_direction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function IsAlternateDirection(ByVal current_direction As TMDSComponentDirection) As Boolean

            Dim isAlternate As Boolean = False

            If current_direction = TMDSComponentDirection.LEFTALT Or current_direction = TMDSComponentDirection.RIGHTALT Then
                isAlternate = True
            End If

            Return isAlternate

        End Function


    End Structure

    ''' <summary>
    ''' Directional Enumeration for components. (RPBJR)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum TMDSComponentDirection
        LEFT
        LEFTALT
        RIGHT
        RIGHTALT
        UP
        DOWN
    End Enum

    ''' <summary>
    ''' Enumeration for handling PSS errors. (RPBJR)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum StatusMessagePSS
        TNT
        PRS
        EMT
        SIG
        TWC
        FAILED
        FLEET_ENABLED_1
        FLEET_ENABLED_2
        NONE
    End Enum

    ''' <summary>
    ''' Enumeration for Rail Road Names... :) WOW
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum RailRoadName
        BNSF
        MRS
        RUMO
    End Enum


#End Region

#Region "Other Authority Type Variables"

    Public gCreateTrackTime As Boolean
    Public gCreateFoulTime As Boolean
    Public gCreateTrackPermit As Boolean
    Public gCreateDeviceBlock As Boolean
    Public gBlockAllDevices As Boolean
    Public gBlockOSDevices As Boolean
    Public gBlockTrackDevices As Boolean
    Public gBlockSwitchDevices As Boolean
    Public gBlockSignalDevices As Boolean
    Public gBlockMiscDevices As Boolean
    Public gDeviceBlockSelection As New ArrayList()
    Public gDeviceSelectionFlash As Boolean
    'Public gIssueToList As New ArrayList()

    Public gSummaryBlockQueue As Queue = Queue.Synchronized(New Queue())
    Public gSummaryAuthQueue As Queue = Queue.Synchronized(New Queue())

    Public gCreateBlueFlagBlock As Boolean
    Public gBlueFlagDevices As Boolean

    Public gBlueFlagBlockSelection As New ArrayList()

    Public gMultipleSelectModeForTNT As Boolean
    Public optEnableMultipleSelectModeForTNT As Boolean
#End Region

#Region "Communication Server Information"

    Public Clients As New ClientConnections()

#Region "LEGACY CAD COMMUNICATION SERVERS"
    Public gCadPrimaryServer As SystemServer
    Public gCadSecondaryServer As SystemServer
    Public gCadPrimaryServerConnection As CommunicationServerConnection
    Public gCadSecondaryServerConnection As CommunicationServerConnection
    Public gCadReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gCadTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gCadRcvQueueCheckThread As Threading.Thread
    Public gCadXmtQueueCheckThread As Threading.Thread
    Public gCadCommServerExists As Boolean
    Public gCadServerExists As Boolean
#End Region

#Region "MOBILE GPS COMMUNICATION SERVERS"
    Public gMGPSPrimaryServer As SystemServer
    Public gMGPSSecondaryServer As SystemServer
    Public gMGPSPrimaryServerConnection As CommunicationServerConnection
    Public gMGPSSecondaryServerConnection As CommunicationServerConnection
    Public gMGPSReceiveQueue As Queue = Queue.Synchronized(New Queue())
    '    Public gCadTransmitQueue As New Queue()
    Public gMGPSTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gMGPSRcvQueueCheckThread As Threading.Thread
    Public gMGPSXmtQueueCheckThread As Threading.Thread
    Public gMobileServerExists As Boolean

#End Region

#Region "BUSINESS SYSTEMS COMMUNICATION SERVERS"
    Public gBSCSPrimaryServer As SystemServer
    Public gBSCSSecondaryServer As SystemServer
    Public gBSCSPrimaryServerConnection As CommunicationServerConnection
    Public gBSCSSecondaryServerConnection As CommunicationServerConnection
    Public gBSCSReceiveQueue As Queue = Queue.Synchronized(New Queue())
    '    Public gCadTransmitQueue As New Queue()
    Public gBSCSTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gBSCSRcvQueueCheckThread As Threading.Thread
    Public gBSCSXmtQueueCheckThread As Threading.Thread
    Public gBusinessGatewayServerExists As Boolean


#End Region

#Region "MOBILE SMART CLIENT COMMUNICATION SERVERS"
    Public gMSCPrimaryServer As SystemServer
    Public gMSCSecondaryServer As SystemServer
    Public gMSCPrimaryServerConnection As CommunicationServerConnection
    Public gMSCSecondaryServerConnection As CommunicationServerConnection
    Public gMSCReceiveQueue As Queue = Queue.Synchronized(New Queue())
    '    Public gCadTransmitQueue As New Queue()
    Public gMSCTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gMSCRcvQueueCheckThread As Threading.Thread
    Public gMSCXmtQueueCheckThread As Threading.Thread
    Public gMobileSmartClientServerExists As Boolean

#End Region

#Region "MULTI-PURPOSE COMMUNICATION SERVERS"

    Public gMultiPurposePrimaryServer As SystemServer
    Public gMultiPurposeSecondaryServer As SystemServer
    Public gMultiPurposePrimaryServerConnection As CommunicationServerConnection
    Public gMultiPurposeSecondaryServerConnection As CommunicationServerConnection
    Public gMultiPurposeReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gMultiPurposeTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gMultiPurposeRcvQueueCheckThread As Threading.Thread
    Public gMultiPurposeXmtQueueCheckThread As Threading.Thread
    Public gMultiPurposeClientServerExists As Boolean

#End Region

#Region "VOICE COMMUNICATION INTERFACE SERVER"

    Public gVCIPrimaryServer As SystemServer
    Public gVCISecondaryServer As SystemServer
    Public gVCIPrimaryServerConnection As CommunicationServerConnection
    Public gVCISecondaryServerConnection As CommunicationServerConnection
    Public gVCIReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gVCITransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gVCIRcvQueueCheckThread As Threading.Thread
    Public gVCIXmtQueueCheckThread As Threading.Thread
    Public gVCIClientServerExists As Boolean

#End Region

#Region "SMS COMMUNICATION INTERFACE SERVER FOR REAL-TIME CONTROL COMMANDS"

    Public gSMSPrimaryServer As SystemServer
    Public gSMSSecondaryServer As SystemServer
    Public gSMSPrimaryServerConnection As CommunicationServerConnection
    Public gSMSSecondaryServerConnection As CommunicationServerConnection
    Public gSMSReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gSMSTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gSMSRcvQueueCheckThread As Threading.Thread
    Public gSMSXmtQueueCheckThread As Threading.Thread
    Public gSMSClientServerExists As Boolean

#End Region

#Region "IVS MESSAGING SERVICE"

    Public gIVSPrimaryServer As SystemServer
    Public gIVSSecondaryServer As SystemServer
    Public gIVSPrimaryServerConnection As CommunicationServerConnection
    Public gIVSSecondaryServerConnection As CommunicationServerConnection
    Public gIVSReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gIVSTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gIVSRcvQueueCheckThread As Threading.Thread
    Public gIVSXmtQueueCheckThread As Threading.Thread
    Public gIVSClientServerExists As Boolean

#End Region

#Region "CONTROL SERVER "

    Public gControlServerXmtQueue As Queue = Queue.Synchronized(New Queue())
    Public gControlServerRcvQueue As Queue = Queue.Synchronized(New Queue())
    Public gStartControlServerThread As Threading.Thread
    Public gConfirmationQueue As Queue = Queue.Synchronized(New Queue())
    Public gStartCommandQueueCheckThread As Threading.Thread

#End Region

#Region "BACK CHANNEL MAINTENANCE COMMUNICATION SERVERS"
    Public gBCMPrimaryServer As SystemServer
    Public gBCMSecondaryServer As SystemServer
    Public gBCMPrimaryServerConnection As CommunicationServerConnection
    Public gBCMSecondaryServerConnection As CommunicationServerConnection
    Public gBCMReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gBCMTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gBCMRcvQueueCheckThread As Threading.Thread
    Public gBCMXmtQueueCheckThread As Threading.Thread
    Public gBCMServerExists As Boolean
#End Region

#Region "API BOS COMMUNICATION SERVERS"

    Public gBOSPrimaryServer As SystemServer
    Public gBOSSecondaryServer As SystemServer
    Public gBOSPrimaryServerConnection As CommunicationServerConnection
    Public gBOSSecondaryServerConnection As CommunicationServerConnection
    Public gBOSReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gBOSTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gBOSRcvQueueCheckThread As Threading.Thread
    Public gBOSXmtQueueCheckThread As Threading.Thread
    Public gBOSClientServerExists As Boolean

#End Region

#Region " MIS SERVER "
    Public gMISPrimaryServer As SystemServer
    Public gMISSecondaryServer As SystemServer
    Public gMISPrimaryServerConnection As CommunicationServerConnection
    Public gMISSecondaryServerConnection As CommunicationServerConnection
    Public gMISReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gMISTransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gMISRcvQueueCheckThread As Threading.Thread
    Public gMISXmtQueueCheckThread As Threading.Thread
    Public gMISCommServerExists As Boolean
    Public gMISServerExists As Boolean = False
#End Region

#Region " IC3 SERVER "
    Public gIC3PrimaryServer As SystemServer
    Public gIC3SecondaryServer As SystemServer
    Public gIC3PrimaryServerConnection As CommunicationServerConnection
    Public gIC3SecondaryServerConnection As CommunicationServerConnection
    Public gIC3ReceiveQueue As Queue = Queue.Synchronized(New Queue())
    Public gIC3TransmitQueue As Queue = Queue.Synchronized(New Queue())
    Public gIC3RcvQueueCheckThread As Threading.Thread
    Public gIC3XmtQueueCheckThread As Threading.Thread
    Public gIC3CommServerExists As Boolean
    Public gIC3ServerExists As Boolean = False
    Public optEnableIC3Interface As Boolean
#End Region

#End Region

#Region "SYSTEM COLOR DEFINITIONS"
    'Verison 227
    'Added color definitions
    Public gSystemColorRed As New SolidBrush(Color.Red)
    Public gSystemColorWhite As New SolidBrush(Color.White)
    Public gColorFailedState As New SolidBrush(Color.DarkGoldenrod)
    Public gColorRemoteLinkDown As New SolidBrush(Color.SaddleBrown)
    'Public gColorRemoteLinkDown As New SolidBrush(Color.SaddleBrown)
    Public gColorNonTraverable As New SolidBrush(Color.FromArgb(128, 128, 128))
    Public gColorJointControlled As New SolidBrush(Color.Gold)


    Public gColorTrackDefault As New SolidBrush(Color.White)
    Public gColorTrackNonIndicated As New SolidBrush(Color.Gray)
    Public gBackGroundColor As New SolidBrush(Color.Black)
    Public gSelectionFlashColor As New SolidBrush(Color.YellowGreen)
    Public gBlockSelectionFlashColor As New SolidBrush(Color.SteelBlue)
    Public gSummarySelectionColor As New SolidBrush(Color.LightSteelBlue)
    Public gColorTrackNotUsable As New SolidBrush(Color.SlateGray)
    Public gSystemStationColor As New SolidBrush(Color.Wheat)
    Public gHiglightColor As New SolidBrush(Color.White)
    Public gColorSwitchMonitorMute As New SolidBrush(Color.Gray)

    Public gTNTColorOverDeviceBlock As Color = Color.MediumSlateBlue
    Public gDeviceBlockColor As Color = Color.MediumSlateBlue
    Public gOverRideFeildBlockingDeviceBlockColor As Color = Color.DodgerBlue
    Public gTNTTrackColor As Color = Color.DodgerBlue

    Public gShowFeildBlockingFaliureColor As Color = Color.Gold

#End Region

#Region "SFA"

    Public gSFAMode As Boolean
    Public optMatchBox8CompsOnRollup As Boolean = False
    Public optMatchSubdirectionToSFADirection As Boolean = True

    Public optEnableSFARollupEventDate As Boolean = False
    Public optEnableSFARollupEventDisplay As Boolean = False
    Public optEnableSFARollupReportClearPDF As Boolean = False
    Public gTWCVariableName As String = "TWC"
    Public gTrackWarrantVariableName As String = "TRACK WARRANT"
#End Region

#Region " TNT OPTIONS "
    Public gTNTComplete As Boolean
    Public gTNTContinue As Boolean
    Public optEnableTNTTrainContextMenu As Boolean = False
    Public optEnableTNTContextMenu As Boolean = True
    Public optAllowTNTSearchThroughSwitch As Boolean = False
    Public optIgnoreSwitchPostionOnTNTIssue As Boolean = False
    Public gCreateTrackTimeIssuedToTrain As Boolean

    Public optDisableTNTModify As Boolean = False
    Public optEnableTNTMultipleTrackSelection As Boolean
    Public optEnableTNTRollUp As Boolean
    Public optEnableTNTModifyJointSoleOnly As Boolean = False
    Public optDisableTNTIssueOnNonTransverableTrack As Boolean = False
    Public optDisableTNTStartingAndEndingAtRRXing As Boolean = False
    Public optEnableTTRollupEventDate As Boolean = False
    Public optEnableTTRollupEventDisplay As Boolean = False
    Public optEnableTTRollupReportClearPDF As Boolean = False
    Public optDisableTTReportClearOnIsLinkToRemoteTrack As Boolean = False

    Public optEnableMOWMoveOnTNTRollup As Boolean
    Public optDisableTATForPTCActiveTrains As Boolean = False
    Public optIncludeFlipFlopInTNTSearch As Boolean = False
#End Region

#Region "PROGRAMMABLE OPTIONS"

#Region "SYSTEM OPTIONS"
    'Passenger Station
    Public optEnablePassengerStation As Boolean = False
    Public gOptUsePassengerStationForOS As Boolean = False
    Public gOptUsePassengerStationForSchedule As Boolean = False
    Public optEnableDoubleFlipFlopLogic As Boolean
    Public optEnableStoredRouteDebug As Boolean
    Public optStoredRouteExecuteLoopSleepTime As Integer = 2
    Public optDisAllowIMonMandatoryDirectivesInUse As Boolean
    Public optMandatoryDirectivesInUse As Boolean

    'CTC
    Public optEnableTWCWarrantSelectionIntoSignalRouteSelection As Boolean
    Public optEnableGroupedSignalRoutes As Boolean
    Public optEnableSignalRouteForQuickRoute As Boolean
    Public optEnablePreseveQuickRouteCollectionOnInvalidClick As Boolean
    Public optEnableRestrictedSignalRouteForQuickRoute As Boolean
    Public optBox20ForcedDialogBox As Boolean = True
    Public optWeatherReminderEnabled As Boolean = True
    Public optShowWeatherReminderForWeatherStationsOnly As Boolean = False
    Public optShowWeatherReminderForAllSubdivisionOnly As Boolean = False
    Public optMOWEnterBehindTrainDialogBox As Boolean = True
    Public optVoidWarrantConfirmationBox As Boolean = True
    Public optRailRoadName As String = ""
    Public optRailRoadLongName As String = ""
    Public optRailRoadEnumName As RailRoadName
    Public optGeneralOrderNotice As String = "MOST CURRENT GENERAL SUPERINTENDENT NOTICE IN EFFECT: "
    Public optDefaultCursorQuickRoute As Boolean = True
    Public optCrossingNotificationOptionOne As Boolean
    Public optJointTrainToTrain As Boolean = False
    Public optWarrantBoxCheckVerbiage As String = ""
    Public optDisplayScreenNameOption As String = ""
    Public optDisplayScreenName As Boolean = False
    Public optAllowPOSCheck As Boolean = True
    Public optBox15StringOne As String = "AUTOMATIC CROSSING DEVICE IS NOT WORKING PROPERLY AT:"
    Public optBox15StringTwo As String = "STOP AND PROTECT THE CROSSING PER GCOR 6.32.2."
    Public optSystemDateTimeString As String = "MM-dd-yyyy"
    Public optSystemDateAndTimeString As String = "MM-dd-yyyy HH:mm"
    Public optSystemOSDateAndTimeString As String = "MM-dd-yyyy HH:mm"
    Public optSystemTimeFormat As String = "HH:mm"
    Public optEnableRecording As Boolean = False
    Public optEnableReportIssueScrShot As Boolean = False
    Public optUseSystemUserCredentails As Boolean = False
    Public optUseNewCheckEICUserMethod As Boolean = False
    Public optEnablePFDUniqueNameCheck As Boolean = False
    Public optEnableCPUSAOptions As Boolean = False
    Public optShowOccupancyOnAuthorirySelection As Boolean = False
    Public optProtectOpenSwitchForCP As Boolean = False
    Public optEnableDateControlForOnDutyDate As Boolean = False
    Public optEnableRemoteTrackResetContextMenuOnly As Boolean = False
    Public optEnableTrackResetContextMenus As Boolean = True
    Public optEnableTrackContextMenuTrainRequest As Boolean
    Public optTrackLengthOnMouseHover As Boolean = True
    Public optEnablePromptToCancelRequestingClear As Boolean = False

    'MANUAL OPERATE
    Public optManualOperateText As String = "MO"
    Public optDisableControlPointMenuFunctionsOnManualOperate As Boolean = False
    Public optEnableReSendOnMO_OFF As Boolean = False
    '
    'MOW DISPLAY
    Public optEnableMOWIDDisplay As Boolean = True
    Public OptShowMOWDisplayText As Boolean = False
    Public optOverrideZoneList As Boolean = True

    'Custom Software Options
    Public gLoadSofwareOptionFromDB As Boolean = False

    'TRAIN EVENTS
    Public optEnableTrainEventFunctions As Boolean = True
    Public optEnableLoadTrainUnitInformation As Boolean

    'MILEPOST PREFIX & SUFFIX
    Public optEnableMilePostPrefix As Boolean = False
    Public optEnableMilePostPrefixAndSuffix As Boolean = False

    'ALLOWED CHARCTERS
    Public ReadOnly gAllowedChars As String = "0123456789.QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm# "
    Public ReadOnly gAllowedNumericChars As String = "0123456789."

    'UserName
    Public optUseFirstLastNameFormat As Boolean = False
    Public optEnableUpdateDataFromViewTrainModify As Boolean = False
    Public optEnableScreenShotCapturing As Boolean
    Public optUseEngineIDScreenShotCapturing As Boolean
    Public optUseTrainPseudoNameForScreenShotCapturing As Boolean
    Public optUseTrainNameScreenShotCapturing As Boolean = False
    Public gWeatherAlertFirstQuarter As String = "06:00:00"
    Public gWeatherAlertSecondQuarter As String = "12:00:00"
    Public gWeatherAlertThirdQuarter As String = "18:00:00"
    Public gWeatherAlertFourthQuarter As String = "23:59:00"

    'SFA & TAT Common Settings
    Public OptEnableCopiedByOnRepeat As Boolean = False
    Public optEnableDirectionalRollup As Boolean = False

    'For all alternative Signal routes (PSS)
    Public optEnableAlternativeRoutes As Boolean = False

    'Train Runtime Data Capture
    Public optEnableCaptureTRTData As Boolean = False

#Region " TRAIN SETTINGS "
    Public optHelperTrainSymbolPrefix As String = ""
    Public optHelperTrainSymbolPrefixDisplay As Boolean = False
    Public optHazMatTrainSymbolPrefix As String = ""
    Public optHazMatTrainSymbolPrefixDisplay As Boolean = False
    Public optDisableModifyTrain As Boolean = False
    Public optDisableCreateTrain As Boolean = False
    Public optEnableTrainSheetCrewSequentialTabOrder As Boolean = False
    Public optEnablePromptForCrewChange As Boolean = True
    Public optKeyTrainSymbolPrefix As String = "K"
    Public optEnableTrainColorsForKeyHazHighWide As Boolean
    Public optEnableFlashTrainForHighWide As Boolean
    Public optEnableFlashTrainForKey As Boolean
    Public optEnableRoadCode As Boolean
    Public optEnableLoadRoadCodeFormDB As Boolean = False
    Public optEnableSendViewTrainOnPosition As Boolean = False
    Public optShowTrainSymbolColorScheme1 As Boolean
    Public optShowTrainSymbolColorScheme2 As Boolean
    Public optEnableConsitsPower As Boolean = False
    Public optShowHighWideKeyTrainSymbolTogether As Boolean = False
    Public optShowHaZmatKeyTrainSymbolTogether As Boolean = False
    Public optEnableTrainUnitColor As Boolean = False
    Public optEnableTrainConsistChangeMenu As Boolean
    Public optEnableCalculateHorsePerTon As Boolean
    Public optHPTStringFormat As String = "00.0"
    Public optScaleTrainSymbolsFormatting As Boolean = True
    Public optShowEngineIDWithSymbol As Boolean
    Public optTrainIDLeftMouse As Boolean = True
    Public optShowTrainScheduleFunctionality As Boolean
    Public optCreateTrainTimes As Boolean
    Public optTrainIDDisplayCrewDetails As Boolean = True
    Public optTrainIDDisplayTrainMiscDetails As Boolean = True
    Public optTrainIDToolTipTrainScheduleInfo As Boolean = False
    Public optShowTrainScheduleColor As Boolean = False
    Public optEnableTrainDelayOnTimeCheck As Boolean = False
    Public optEnableCalculateTrainDeviation As Boolean = False
    'HOS
    Public optEnableHOSCheck As Boolean = False
    Public optHOSDefaultTime As Integer = 12
    Public optHOSRedAlertTime As Double = 0.0
    Public optHOSBrownAlertMaxTime As Double = 1.0
    Public optHOSBrownAlertMinTime As Double = 0.0
    Public optHOSYellowAlertMaxTime As Double = 2.0
    Public optHOSYellowAlertMinTime As Double = 1.0
    Public optEnableTrainAutoRefreshSystemSummary As Boolean
    Public gTrainAutoRefreshSystemSummaryTimeinMinutes As Integer = -10
    Public gLastTrainAutoRefreshSystemSummaryTime As Date = Now
    Public optEnableDisengagedColorStatus As Boolean = False
#End Region

#Region "SCALE ALARM"
    Public optEnableScaleAlarm As Boolean = False
#End Region


#Region " DEVICE BLOCK SETTINGS "
    Public optDeviceBlkPromptWhenAdjacentCPinCodeFail As Boolean = False
    Public optEnableDeviceBlkOnCodeFail As Boolean = False
    Public optEnableBlueFlagDeviceBlk As Boolean = False
    Public optDeviceBlkExpirationTime As Integer = 1
    Public optEnableDeviceBlkExpirationCheck As Boolean = False
    Public optUseDeviceBlockTimeExpried As Boolean = False
    Public optIgnoreLikeCompOnDeviceBlock As Boolean = False
    Public optSingleComponentSelectOnDeviceBlock As Boolean = False

#End Region

#Region " TAG SETTINGS "
    'Tags
    Public optOverrideXingTagCount As Boolean
    Public optOverrideSpeedTagCount As Boolean
    Public optForceSpeedTagSpeed As Boolean = False
    Public optDisablePassengarSpeedOnTags As Boolean
    Public optShowInfoTagHeadingOnToolTip As Boolean
    Public OptEnableInfoTAGOnly As Boolean = False
    Public optEnableTagTrackAlarm As Boolean = False
    Public optEnableTagWorkZone As Boolean = False
    Public optEnableStretchyTag As Boolean
    Public optEnableCopyTagsWithin100Limit As Boolean
    Public optDisableTagMilepostValidation As Boolean
    Public optDisableTagMilepostSuffixValidation As Boolean
    Public optEnableMultipleTrackTagSelectionForSpeedRestrictions As Boolean
    Public optEnableMultipleTrackTagSelectionForInfoRestrictions As Boolean
    Public optEnableMultipleTrackTagSelectionForWeather As Boolean
    Public optEnableWeatherTags As Boolean
    Public gTrackTagLimit As Integer = 9
    Public optEnableTagCopyMenu As Boolean = True
    Public optEnableTagModifyMenu As Boolean = True
    Public optUseOneSpeedDisplay As Boolean
    Public optSupressMilePostCheckForTags As Boolean
    Public optEnableTrainTAG As Boolean = False
    Public optEnableConvertTrainTAGwDelayToDelayEvent As Boolean = False
    Public optAllowZeroSpeedTag As Boolean
    Public optForcePerformMilepostCheckOnTags As Boolean
    Public optEnablePromptForMultipleTagRelease As Boolean
    Public optOverrideMPHCheckForSpeedTags As Boolean
    Public optEnableSpeedRestrictionFlags As Boolean
    Public optEnableSpeedRestrictionToTSS As Boolean
    Public optDisableTagMilePostAutoFill As Boolean
    Public optClearTagTrackList As Boolean
    Public optEnableTrackTagMultipleSameTrackNameFunction As Boolean
    Public optEnableTagPsgrSpeedGreaterThanFreight As Boolean
#End Region

#Region " BULLETIN SETTINGS"
    Public optEnableBulletinTrackContextMenus As Boolean
    Public optDisableBulletinCreationTrackLineMenuSpeed As Boolean
    Public optDisableBulletinCreationTrackLineMenuWork As Boolean


#End Region

#Region " SIGNAL SETTINGS "
    'SIGNAL
    Public optEnableShowSignalNameOnToolTip As Boolean
    Public gOptDisableSignalControlOnManualOperateMode As Boolean = False
    Public optEnableCheckForValidTWCAuthorityOnSignalClear As Boolean = False
    Public OptEnableSignalClearByMenu As Boolean = False
    Public optAllowQuickRouteWithSignalClearbyMenu As Boolean
    Public OptEnableSignalClearByMenuCheckControlPointStatus As Boolean
    Public OptEnableSignalFleetMenu As Boolean = True
    Public optSupressUnSolicitedSignalCheck As Boolean
    Public optEnableSignalClearIntoOccupancy As Boolean = False
    Public optDisableSignalClearCheckForTNTInRRXing As Boolean = False
    Public OptEnableSignalBlockMenu As Boolean = False
    'FLEET SIGNAL
    Public optDisableFleetSignalOnConflictingStoredRoute As Boolean = False
    Public optDisableFleetSignalOnCommFail As Boolean = False
    Public optDisableExistingFleetSignalOnManualOperate As Boolean = False
    Public optPromptFleetSignalWhenCPOnManualOperate As Boolean = False
    Public optDisableExistingFleetSignalOnAbsoluteBlocking As Boolean = False
    'STORED ROUTE
    Public optDisableStoredRouteOnConflictingFleetSignal As Boolean = False
    Public optDisableStoredRouteOnCommFail As Boolean = False
    Public optDisableExistingStoredRouteOnManualOperate As Boolean = False
    Public optPromptStoredRouteWhenAdjCPOnManualOperate As Boolean = False
    Public optDisableExistingStoredRouteOnAbsoluteBlocking As Boolean = False
    Public optCancelStoredRouteWhenSignalStopRequested As Boolean = False
    Public optEnableGroupedStoreRoute As Boolean = False
    Public optEnableGroupedStoreRouteOnly As Boolean = False
    Public optCheckForDisableSignalRouteCondition As Boolean = False
    Public optEnableStoredRouteClearOnRelease As Boolean = False
    'MESSAGES
    Public optPromptTrafficDirectionDivergent As Boolean = False
#End Region

#Region " TRACK SETTINGS "
    Public optEnableTrackSidingBit As Boolean = False
    Public gFieldBlockTimerWaitSecs As Integer = 30
    Public optEnableFieldBlockTimerWait As Boolean = False
    Public optEnableReverseMovementIntoOSIfSignalIndicatesClear As Boolean = False
    Public optEnableFlashOSTracks As Boolean
#End Region

#Region " OS SETTINGS "
    Public optEnableLogOSOnAllTWCMoves As Boolean
    Public OptUseSwitchNameAsOSPoint As Boolean
    Public OptCheckLastOSForDuplicateOS As Boolean
#End Region

#Region " TUNNEL AUTOMATIC MODE SETTINGS "
    Public optEnableTunnelAutomaticModeString As String = "ENABLE TUNNEL AUTOMATIC MODE"
    Public optDisableTunnelAutomaticModeString As String = "DISABLE TUNNEL AUTOMATIC MODE"
    Public gTunnelMaintenanceMode As Boolean
#End Region

#Region " CODE BROWN SETTINGS "
    Public optEnableCTCTAGDispalyonCodeBrown As Boolean = False
    Public optEnableCTCNonTraversableDispalyonCodeBrown As Boolean = False
    Public optEnableToPassRedSignalOnCodeBrown As Boolean
    Public optDisbaleContextMenuOnCodeBrownDisplay As Boolean = False
#End Region

#Region " SPM SETTINGS "
    'SPM
    Public optEnableTrainOnAllLikeTracks As Boolean
    Public optDisableTrafficArrowsSPM As Boolean
    Public optShowOneAuthorityNumberPerSPMSwitch As Boolean = False
    Public optHideTurnOutTracksOnSPMSwitch As Boolean = False
    Public optHideSPMSwitchLock As Boolean = False
    Public optHideSPMSwitchLockOnNoLink As Boolean = False
#End Region

#Region " TWC SETTINGS "
    Public optShowSwitchPositionOnTWCSelect As Boolean = False
    Public optValidateTrackSelectionForRollup As Boolean = False
    Public optAllowNonTWC As Boolean = True
    Public optAllowNonTWCSelection As Boolean
    Public optSingleFormAuthority As Boolean
#End Region

#Region " SFA PTC COLORS SETTINGS "
    Public optUseSFAColorStatusForPTC As Boolean = False
    Public SFAColorStatusPTCIssuancePending As New SolidBrush(Color.YellowGreen)
    Public SFAColorStatusPTCIssuanceImminent As New SolidBrush(Color.LawnGreen)
    Public SFAColorStatusPTCReleasePending As New SolidBrush(Color.LightCoral)
    Public SFAColorStatusPTCReleaseImminent As New SolidBrush(Color.DarkRed)
#End Region

#Region " TNT SETTINGS "

#End Region

#Region " PSS & EMT SETTINGS "
    Public optDisableSwitchesInHandForPSS As Boolean = False
    Public optEnableIncludeTrackUnderTrainForPSS As Boolean = False
    Public optEnableCheckForValidTWCAuthorityOnPSS As Boolean = False
    Public optEnablePSSEMTAuthorityType As Boolean = False
    Public gMenuPermissionToEnterMainTrackText As String = "ENTER MAIN TRACK"
    Public gMenuPermissionToPassRedSignalText As String = "PASS RED SIGNAL"
    Public optEnablePSSEMTAuthorityNoDeviceBlk As Boolean = False
    Public optEnablePSSEMTAuthorityApplyBlocking As Boolean = True
    Public optShwoPSSEMTAuthorityNoDeviceBlkDirection As Boolean = False
    Public optEnableMenuPermissionToEnterMainTrack As Boolean
    Public optEnableMenuPermissionToPassRedSignal As Boolean
    Public optEnableMenuPermissionToRemoveStopTarget As Boolean
    Public gPermissionToRemoveStopTarget As Boolean
    Public gPermissionToRemoveStopTargetSignal As Signal = Nothing
    Public gPermissionToRemoveStopTargetText As String = "REMOVE STOP TARGET"
    Public gMenuCancelPassRedSignalText As String = "CANCEL PASS RED SIGNAL"

    Public gOverridePSSPermissionForCTCIsland As Boolean = False
    Public optUseEngineIDOnPSSEMTCancel As Boolean = False
    Public optUseReadRepeatOnPSSEMT As Boolean = False
    Public optEnableOverrideTNTForPRS As Boolean
    Public optEnableOverrideJointTNTForPRS As Boolean
    Public optEnableOverrideDeviceBlockForPRS As Boolean
    Public optEnableApplyDeviceBlockFirstPRS_EMT As Boolean
    Public optEnableOverrideSIGValidationForPRS As Boolean = False
    Public optEnableAutomaticRemovalOfPRSEMT As Boolean = False
    Public optEnablePSSEMTShowDeleteMenuOnNoTrain As Boolean = False
    Public optAllowPSSOverOccupancy As Boolean = False
    Public optEnablePSSAutoSelectSwitchesOnCodeFail As Boolean = False
    Public optEnableOverrideSIGValidationForPRSCheckSigStatus As Boolean = False

    Public optProcessPTCPSSEMT_FulfillmentMessage As Boolean = False
    Public optEnableAutomaticRemovalOfStopTarget As Boolean = False

    Public optEnableLogTrainDelayFromPSS As Boolean = False
    Public optEnablePSSDelayUseCode As String = String.Empty

    Public optEnableSystemSummaryPreviewForPRS_EMT As Boolean


#End Region

#Region " TGBO SETTINGS "
    Public optEnableTGBOCheckOnAuthority As Boolean = False
    Public optReleaseTGBOOnTrainFinal As Boolean
    Public optEnableLoadSlideZoneRecords As Boolean = False
    Public optEnableWeatherComponentRecords As Boolean = False
    Public optEnableTGBOMOWBulletinFilter As Boolean
    Public optEnableMultipeTrainListingsInGTB As Boolean
    Public optEnableBulletinTabWithTGBO As Boolean
    Public optEnableTGBOWeb As Boolean
    Public optEnableBulletinFunctionsGCOR As Boolean
    Public gTGBOHeaderString As String = "TGBO"
    Public optEnableTGBO_GBOModify As Boolean
    Public optEnableConductorTGBO As Boolean = False
#End Region

#Region " BLUEFLAG MENU TEXT SETTINGS "
    Public optBFlagMnuIssueString As String = "BLUE FLAG-ISSUE"
    Public optBFlagMnuReleaseString As String = "BLUE FLAG-REMOVE"
    Public optBFlagString As String = "BLUE FLAG"
#End Region

#Region "EVENTLOG DATABASE SETTINGS "
    Public optEnableEventlogDBAlert As Boolean = True
    Public optLastEventlogDBAlertShown As Date
    Public optShowEventlogDBAlert As Boolean = True
    Public optEnableDirectMoveLog As Boolean = False
#End Region

#Region " ALERTS SETTINGS "
    Public optPublishComponentAlert As Boolean = False
    Public gDisbleIndicationAlertOnAssistMode As Boolean = False
    Public OptShowRedAlertFirst As Boolean
    Public OptShowTrainPassRedSignalAlertDetails As Boolean = False
    Public optTrainPassRedSignalWithTrainSymbolOnly As Boolean
    Public optUseAbbreviatedTrainPassRedVerbiage As Boolean
    Public optEnableHOSAlertGeneration As Boolean
    Public optEnableWeatherAlerts As Boolean = False
    Public optEnableAlertContentView As Boolean
    Public optEnableEnhancedAlertTab As Boolean = False
    Public optEnableFEDAlerts As Boolean = False
    Public optSupressPassRedSignalCheck As Boolean
#End Region

#Region " TRAIN SCHEDULER SETTINGS "
    Public optEnableTrainScheduleFunctions As Boolean = False
    Public optEnableLoadTrainSchedule As Boolean
    Public gDelayTolerance As Integer = 6
    Public optEnableTrainScheduleDelayProcessingBasedOnSchedules As Boolean
#End Region

#Region " CUSTOM RELEASE TIMER SETTINGS "

    Public optEnableTAGDeleteTimer As Boolean = False
    Public optEnableDbBlkDeleteTimer As Boolean = False
    Public optEnableDbAuthorityRemoveTimer As Boolean = False
    Public optReportClearSingleReleaseWithTimer As Boolean = False
    Public optEnableConfigurableReleaseTimer As Boolean = False
    Public optReportRollupWithTimer As Boolean = False

#End Region

#Region " TRANSFER REPORT SETTINGS "
    Public optPrintTransferReport As Boolean
    Public optUseTerritoryForTransferReport As Boolean
    Public optEnableReprintTransferReport As Boolean = False
    Public optEnableTransferReportAcknowledgement As Boolean
    Public optEnableTransferReportAddDeviceBlocks As Boolean
    Public optEnableTransferReportAddTrackTags As Boolean
    Public optEnableTransferReportAddBulletins As Boolean
    Public optEnableTransferReportAddBulletinsExecptions As Boolean
    Public optEnableTransferReportAddAnnotations As Boolean
#End Region

#Region " SYSTEM TRAINS "

    Public optEnableQueryAllSystemTrains As Boolean
    Public optEnableSystemTrainAutoRefresh As Boolean
    Public optLockTrainPropertiesForViewOnlyTrains As Boolean = True
    Public optOverrideSystemTrainAutomaticDisabling As Boolean
    Public optEnableSystemTrainSummaryUpdating As Boolean
    Public optEnableQueryAllSystemTrainsNotinUseOnly As Boolean
#End Region

#Region " INCIDENT REPORT SETTINGS "
    Public optUseTroubleReportingEnhanced As Boolean
    Public optEnableTrainCarSecurement As Boolean
    Public optUseIncidentTypeTables As Boolean = False

#End Region

#Region " GPS SETTING "
    Public optEnableTrainTrackingGPS As Boolean
    Public optDisableTrainTrackingViaGPS As Boolean
    Public optCheckForGPSEnabledUnit As Boolean
    Public optEnableGPSTrackingOnCTC As Boolean
#End Region

#Region " Weather station "
    Public optEnableWeatherStationMenu As Boolean
    Public optPublishWeatherEvent As Boolean
#End Region

#End Region

#Region "GTM OPTIONS "
    Public optForceGTMEventsForTWCRollupAndReportClear As Boolean
    Public optEnableGTMToBeDouble As Boolean = True
    Public optIncludeEmailOptionInGTBIssuanceProcess As Boolean = False
    Public optEnableNetworkGTBPrint As Boolean = False
#End Region

#Region "BULLETIN OPTIONS "

    Public gListAllSubdivisions As Boolean
    Public optAllowGTBCreationWithNoBulletins As Boolean = False
    Public optEnableBCSearchSpanningBulletinCell As Boolean = False
    Public optEnableBCGTBExecptionValidation As Boolean
    Public optEnableBulletinCellFunctionality As Boolean
    Public optEnableBCFlagSearch As Boolean = True
    Public optForceEmptyFlagMilepost As Boolean
    Public optLoadBulletinsBySubdivision As Boolean
    Public optEnableWaitTimeForBulletinDelete As Boolean = False
    Public optTimeWaitTimeForBulletinDelete As Integer = 5
    Public optEnableSystemBulletinFunctionality As Boolean
    Public optDisableGTBFunctions As Boolean
    Public optBulletinCellGTBRailRoadHeader As String = ""
    Public optEnableBulletinCellMilepostValidation As Boolean = False
    Public optEnableBulletinCellMilepostFlipLowToHigh As Boolean = False

    Public optBulletinConfiguredForMRL As Boolean
    Public optApplyKCSBulletinConfiguration As Boolean
    Public optApplyNCTDBulletinConfiguration As Boolean
    Public optAllowZeroSpeedOnFormA As Boolean
    Public optDontForceSameSpeedOnFormA As Boolean
    Public optEnableDisplayBulletinsOnTracks As Boolean
    Public optUseCompBulletinsTracksListOnReLoad As Boolean = False


    Public LastSpeed1TBEvent As Date
    Public LastSpeed2TBEvent As Date
    Public LastWork1TBEvent As Date
    Public LastWork2TBEvent As Date
    Public LastXingTBEvent As Date
    Public LastInfo1TBEvent As Date
    Public LastInfo2TBEvent As Date

    Public optLoadGTBBulletinsByGUID As Boolean = False
    Public gFlagMileslimit As Integer = 2
    Public gShortFlagMileslimit As Integer = 1
    Public optEnableBulletinSystemEnhanced As Boolean
    Public optEnableLegacyBulletinMenuOnTrkLnDisplay As Boolean = True

    Public optEnableCancelFormOTrainSpecific As Boolean = False
    Public optEnableSetDeliveredTextOnDelivery As Boolean = False

#End Region

#Region "TRACK COLOR OPTIONS "

    Public optEnableHighLightTrack As Boolean = False
    Public optEnableHatchTrack As Boolean = False
    Public gHighlightedTracks As New ArrayList

#End Region

#Region "SYSTEM MANAGEMENT SERVICES"

    Public optEnableSystemManagementServices As Boolean
    Public gSystemManagementService As SystemManagementServices
    Public gSystemManagementServiceMessageQueue As Queue = Queue.Synchronized(New Queue)

#End Region

#Region "CP OPTIONS"

    Public optEnableCPSpecificOptions As Boolean = False
    Public gCPDefaultCraft As String = "ENGR"
    Public optEnableCPSpecificPTCOperations As Boolean
    Public optEnableNewTWCWorkLogic As Boolean = False
    Public optEnableNewTrackSelectionVersion3ForCP As Boolean = False

    Public optEnableTagAlertOnsuccessfulGBOException As Boolean = True
    Public optDisableGBOTAGOnFreeFormWithMP As Boolean = False
    Public optSortHighwayCrossingByMilePost As Boolean = False
    Public optEnableGBOMilePostFlipLowToHigh As Boolean = False
    Public optEnableSwitchMuteColor As Boolean
    Public optEnableTGBOOKTime As Boolean = False
    '
    Public optCPTrackToolTipFormat As Boolean = False
    Public optEnableHideAuthorityPointObjects As Boolean
    Public optEnableCPCheckOnTrainMove As Boolean = False
    Public optEnableOperations_CP_GCOR As Boolean
    Public optEnableTGBOCancelExceptions As Boolean
    Public optEnableNotificationsForTGBO As Boolean
    Public optEnableTGBOOperationsForCP As Boolean

    Public optDisableTrainMoveOnTWCReportClear As Boolean = False
    Public optLoadForemanBasedOnSubDivision As Boolean = False
    Public optEnableTWCForCP As Boolean
    Public optEnableTNTForCP As Boolean
    Public optLoadCursorsForCP As Boolean
    Public optEnableTrainMiscMenuCP As Boolean
    Public optUseTwelvePointFontOnAuthority As Boolean
    Public OptDisableTWCMenus As Boolean
    Public optEnableMOWMiscMenuCP As Boolean
    Public optEnableTrainSheetSummaryConfigurationForCPR As Boolean
    Public optDisableTNTReleaseUNACKAuthoritiesForCP As Boolean = False
    Public optExtendUntilTimeCount As Integer = -1
    Public optUseSingleFormAuthorityVersionIII As Boolean

    Public gReadColor As Color = Color.Yellow
    Public gRepeatColor As Color = Color.LimeGreen
    Public gReadSelectionColor As Color = Color.Turquoise
    Public gRepeatSelectionColor As Color = Color.WhiteSmoke
    Public gReadRepeatHighlightColor As Color = Color.DarkViolet
    Public gReadBackColor As Color = Color.Black
    Public gRepeatBackColor As Color = Color.Black
    Public gControlHighLightColor As Color = Color.WhiteSmoke
    Public gRepeatFont As Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Public gCPErrorCollectionList As New ArrayList
    Public optOnLoginPrintCPTransferReport As Boolean = False
    Public optEnableEntryExitTWCPoints As Boolean = False
    Public optEnableAuthorityView As Boolean = False
    Public optEnableReleaseReport As Boolean = False
    'Public optByandFor As String = "by and for"
    'Public optForMovement As String = "For movement"
    Public optByandFor As String = ""
    Public optForMovement As String = ""
    Public optEnableABSCP As Boolean = False
    Public optDisableTWCAtLocation As Boolean = False


    Public optEnableCPMenus As Boolean = False
    Public optEnableCPPermissionToEnterMainTrack As Boolean = False
    Public gCreateCPEMT As Boolean = False
    Public gCreateTrainPassRed564 As Boolean = False
    Public gCreateIssueClearanceProceed As Boolean
    Public gCreateIssueTrackWarrantProceedForTrain As Boolean
    Public gCreateIssueTrackWarrantWorkForTrain As Boolean
    Public gCreateIssueTrackWarrantWorkForMOW As Boolean
    Public gCreateIssueTrackWarrantRollUpForMow As Boolean
    Public gCreateIssueTrackWarrantRollUpForTrain As Boolean
    Public gCPReportClearSFACollection As New ArrayList
    Public gCPReportClearMissingSFAToTrainCollection As New ArrayList
    Public gCPRollUpSFA As TMDS.TrackAuthority.SFAuthority = Nothing
    Public gCPRollupComps As New ArrayList
    Public gCPRollupTrack As Track = Nothing
    Public gCPTWCTrainReportClearMode As Boolean = False
    Public gCPTWCDisableSwitchMove As Boolean = False
    Public gCPTWCEnablePOSSwitchMove As Boolean = False
    Public gTWCEntryTracks As New ArrayList
    Public gTWCEntrySwitches As New ArrayList
    Public gTrainCrewCrafts As New ArrayList
    Public gCPRollUpTNT As TMDS.TrackAuthority.TrackAuthority = Nothing
    Public gCPTNTRollUpForMow As Boolean = False
    Public gCPTNTRollUpForTrain As Boolean = False
    Public ReadColor As Color = Color.Yellow
    Public RepeatedColor As Color = Color.LightGreen
    Public optEnableCPTrainsheet As Boolean = False

    Public gCPAuthDirection As String = ""
    Public gCPTBNumberingColumn As String = "BullNumbrINF1"
    Public gCPTBNumberRangeColumn As String = "BullRangeINF1"
    Public gCPTBNumberFactorColumn As String = "BullFactorINF1"
    Public gCPTBNumberingTable As String = "tblSystemSubDivisions"

    'TO DO - Remove these var after master schema deployment
    Public gPTCAuthoritySegmentsCurrentDesk As String = "PTCAuthoritySegmentsCurrentDesk"
    Public gAuthorityLimitsCurrentDesk As String = "AuthorityLimitsCurrentDesk"
    Public gIsRestrictionTypesExistInDb As String = False

#End Region

#Region "ARRC OPTIONS"
    Public optEnableJointControlMode As Boolean

    Public optDisableRadioBlocking As Boolean
    Public optDisableAfterArrival As Boolean = True

    Public optAllowZeroSpeedForSpeeRestriction As Boolean
    Public optTrainSymbolMnemonicForPTC As String = "^"
    Public optEnableARRCSpecificPTCOperations As Boolean


    Public optEnableTrainMenuTrackAuthority As Boolean
    Public optEnableMOWMenuTrackAuthority As Boolean

    Public optEnableTrainMenuRouteStack As Boolean

    Public optEnableSubvisionAuthorityNumbering As Boolean
    Public optEnableLoadRailroadInformation As Boolean
    Public optEnableTrackSpeedInformation As Boolean


    Public optEnableTrainRouteSelectionMode As Boolean
    Public optDisableTagSpeed As Boolean
    Public optDisableTagXing As Boolean
    Public optDisableModifyMenuTNT As Boolean
    Public optEnableBulletinRouteCheck As Boolean

    Public optEnableARRCSpecificOptions As Boolean = False
    Public optEnableTunnelAutomaticOperations As Boolean
    Public optUseSingleFormAuthorityVersionARRC As Boolean
    Public optUseSPAFComboBoxSelection As Boolean


    Public optEnableWorkTaskFunctions As Boolean

    Public optEnableTrainFunctionalityARRC As Boolean
    Public optEnableSendOSMessageARRC As Boolean
    Public optApplyTrainSheetConfigurationForARRC As Boolean
    Public optEnableWorkOrderFunctions As Boolean
    Public optEnableTrainRestrictionFunctions As Boolean
    Public optGetTrainUnitsFromLocomotiveTable As Boolean
    Public optEnableAxlesBrakesCounts As Boolean
    Public optEnableTrainCarFunctions As Boolean
    Public optEnableRemoveScheduleOnTrainFinal As Boolean

    Public optEnableTrackMileageInfoMouseHover As Boolean
    Public optEnableTrackMileageInfoMouseHoverRoundToHundereds As Boolean
    Public optEnableDisplayAuxSwitches As Boolean = False

    Public optEnableDisplaySignal As Boolean = True
    Public optDisableMainMaintenanceMenu As Boolean
    Public optDisableMainDisplayOptionsMenu As Boolean
    'cschock: ARRC-1160
    Public optEnableCadMessageAlerts As Boolean = False

#End Region

#Region "KCT"
    Public optTrackToolTipFormatForKCT As Boolean
    Public optTrackToolTipFormatWithControlPointMilepost As Boolean = False
    Public optEnableSegmentBlockingForTNT As Boolean
    Public optEnableShowStatusUnderNonTransverableTrack As Boolean
    Public optApplyConfigurationForKCT As Boolean
    Public optEnableReportClearTimer As Boolean
    Public optEnableTagVerbiageForKCT As Boolean
    Public optUSeSignalNameInOsReport As Boolean
#End Region

#Region "BRC"
    Public optUseTrackAlternateNameForMovementReporting As Boolean
    Public optEnableTrainMovementReporting As Boolean
    Public optEnableBRCSpecificChecks As Boolean = False
    Public optDisableTrackNameVerbiageOnTNT As Boolean
    Public optEnableAutoFillFreightSpeedInTrackTag As Boolean
    Public optEnableSwitchPositionRouting As Boolean
    Public optDisableExpandOutLogicOnOsTracks As Boolean ' = True
    Public optAllow1920x1200ResolutionUse As Boolean
    Public optApplyTrainSheetConfigurationForBRC As Boolean
    Public optApplySystemSummaryConfigurationForBRC As Boolean
    Public optEnableHoursLoggedAlert As Boolean = False
    Public optWaitHoursToShowLogAlert As Double = 8.0
    Public gHoursLoggedAlertShown As Boolean = False
    Public gTimeLogged As String = ""
    Public optUseUntilCalledOnTAT As Boolean = False
    Public optShowLimitsonTATRelease As Boolean = False
    Public optEnableStoreRouteToolTipForBRC As Boolean
    Public optEnableDeleteNoteConfirmation As Boolean
    Public optEnableThirdPromptForReportClear As Boolean = False
    Public optOverrideTabOrder As Boolean = False
    Public optEnableTroubleTicketOptionsForBRC As Boolean
#End Region

#Region "METROLINK"
    Public optEnableWorkZoneForcedToAllTracks As Boolean
    Public optEnableTrainCrewRecursiveUpdateToLinkedTrains As Boolean
    Public optEnableTrainConsistRecursiveUpdateToLinkedTrains As Boolean

    Public optEnableTripLinkLegacy As Boolean
    Public optEnableHatchForTrackConditionMessageTypeBulletins As Boolean = True
    Public optEnableTrackSymbolTrackingOnVerticalTracks As Boolean = True

    Public optEnableMessagingEDI As Boolean
    Public optEnableLoadSystemPrintFaxDevices As Boolean
    Public optEnableMiscDeviceTrackInterlockMode As Boolean
    Public optDisableCrossingBulletinMalfunctionTypes As Boolean
    Public optEnableElectronicAuthoritySummaryDisplay As Boolean
    Public optEnableAuthorityJointCountCheck As Boolean
    Public optAuthorityJointCount As Integer = 2
    Public optUseEmployeeIdentiferForAuthorityIssuance As Boolean
    Public optEnableSendControlsToFieldOnBlocking As Boolean
    Public optEnableJointAuthorizationConflictChecking As Boolean
    Public optTrackAuthorityExpirationTimeInMinutes As Integer
    Public optEnableNumberPools As Boolean
    Public optFormatTrainSymbolRuleML As Boolean
    Public optEnableAutoTrainPositionFromScheduleLink As Boolean

    Public optEnableAutoReleaseGTB As Boolean

    Public optEnableMetroLinkSpecificOptions As Boolean = False
    Public optApplyTrainSheetConfigurationForMetrolink As Boolean
    Public optEnableSpecificOptionsForMetrolink As Boolean = False
    Public optEnableSpecificPTCOperationsForMetrolink As Boolean

#End Region

#Region "FECR"
    Public optApplyTrainSheetConfigurationForFECR As Boolean
    Public optEnableSpecificOptionsForFECR As Boolean = False
    Public optEnableCommunicationFailureOverride As Boolean
    Public optEnableSendOSMessageFECR As Boolean
    Public optEnableDelayCodeSortOrderByCode As Boolean
    Public optEnableAuthorityCompleteVerbiage As Boolean
#End Region

#Region "NMRX "
    Public optEnableAutoDetectRailCode As Boolean
    Public optEnableLegacyBulletinFunctionality As Boolean = True
    Public optForceFormBShortFlagsToNothing As Boolean
    Public optPerformTrainTonsCheck As Boolean
    Public optLoadConductorsToMOWIssueToList As Boolean
    Public optLoadEngineersToMOWIssueToList As Boolean
    Public optApplyTrainSheetConfigurationForNMRX As Boolean
    Public optUseSystemWideGeneralOrder As Boolean
    Public optDisableControlsInMonitorMode As Boolean
    Public optDefaultSelectAllSubdivisions As Boolean
    Public optEnableSubdivisonSelectionCheckInGTB As Boolean
    Public optAllowOnlyTenthsinBulletin As Boolean
    Public optEnableLoadTrainEng_Cond As Boolean

#End Region

#Region " MRL OPTIONS "

    Public optEnableMRLSpecificOptions As Boolean = False
    Public optAllowAutoSwitchReverseIntoSidingForTNT As Boolean
    Public optCheckToSetDirectionForNoneDirection As Boolean
    Public optCheckAlphaNumericPsesudoTrnName As Boolean
    Public optUseUntilCallFormattingForUntilTime As Boolean
    Public optEnableTroubleTicketOptionsForMRL As Boolean

    Public optDisableDoubleClickDeleteOtherUnits As Boolean
    Public optUseSingleFormAuthorityVersionMRL As Boolean
    Public optEnableSendOSMessageMRL As Boolean
    Public optApplyTrainSheetConfigurationForMRL As Boolean
    Public optDisableTrackInfoMouseHover As Boolean
    Public optDisableShowItemEightFullOverlapVerbiageForTNTonly As Boolean
    Public optLogGrossTonMiles As Boolean = True
    Public optShowTRCrossingArrival As Boolean = True
    Public optEnableTRClose As Boolean = True
    Public optTimeWaitTimeForAuthorityRelease As Integer = 5
#End Region

#Region "CN/IC OPTIONS "
    Public optEnableSpecificOptionsForCN As Boolean
    Public optLoadBulletinZones As Boolean
    Public optEnableForceSendStopControlToField As Boolean
    Public optEnableSignalRouteGenerationForPTC As Boolean
    Public optEnableCNSpecificPTCOperations As Boolean
    Public optEnableCNSpecificOptions As Boolean = False
    Public optEnableOptimalBulletinParseFunction As Boolean
    Public optEnableBulletinTemplates As Boolean
    Public optEnableTrainSheetCrewCNTabOrder As Boolean = False
    Public optDisallowNonTWCTracksInMiddleOfSelection As Boolean
    Public optEnableTGBOOperationsForCN As Boolean

    Public optEnableNetworkPortLogging As Boolean


    Public optEnableCNFLIPFLOPLimits As Boolean

    Public gSendDenyMessageOnIMClose As Boolean
    Public gEnableAutoClearingMode As Boolean
    Public optEnableAllowForceOperationsToTWC As Boolean
    Public optEnableFlipFlopDetectionLogic As Boolean ' THIS IS USED FOR TRACKS THAT CONNECT IN THE SAME DIRECTION BUT ON DIFFERENT TERRITORIES
    Public optSystemTimeToLiveTasksEnabled As Boolean
    Public optElectronicAuthorityDeliveryConfirmationEnabled As Boolean

    Public optEnableTerritoryRequest As Boolean
    Public optCheckForTerritoryFlipDirectionEnabled As Boolean
    Public optAllowTagCopyFunctionOnCodeFailureTrack As Boolean
    Public optUseClearMainTrackColorOption As Boolean
    Public optLaunchTagFormOnTrackLineDisplay As Boolean
    Public optAutoRefreshViewOnlyTrains As Boolean
    Public optAllowTheRemovalOfTrafficPendingRequestInTheBlock As Boolean
    Public optCheckForChangeDirectionEnabled As Boolean = True
    Public optScreenShotCaptureDuration As Integer = 5
    Public optExcludeIndicationLikeOsTracksWhenNonTransversable As Boolean

    Public optShowJointLimitsColor As Boolean
    Public optUseSingleFormAuthorityVersionCN As Boolean
    Public optApplyTrainSheetConfigurationForCN As Boolean
    Public optFlipNorthSouthDirection As Boolean
    Public optEnableSendOSMessageCN As Boolean

    Public optFormatTrainSymbolRuleCN As Boolean

    Public optOverrideAuthorityGapCheck As Boolean
    Public optEnableRouteBlocking As Boolean

    Public optEnableJournal As Boolean = False
    Public optEnableMoveUnits As Boolean = False
    Public optEnableSystemSummaryColor As Boolean = False





    Public optEnableDeviceBlockMenuOnMiscDevices As Boolean

    Public optEnableTabularGeneralBulletinOrders As Boolean
    Public optEnableTabularGeneralBulletinOrdersOnSystemSummary As Boolean

    Public optAllowPOSWithInABS As Boolean

#End Region

#Region " WATCO OPTIONS "
    Public optEnableWatcoSpecificOptions As Boolean
    Public optEnableBulletinFormR As Boolean
    Public optUseFirstTerritoryWhenTrainSymbolNotFound As Boolean = True
    Public optDisplayGeneralNotice As Boolean = True
    Public optDisplaySuptNotice As Boolean = True
    Public optDisplayBox22 As Boolean
    Public optForceBox16 As Boolean
    Public optShowBulletinTabOnWarrantCreation As Boolean
    Public optFontControlPointMedium As Boolean
    Public optFontAlphaTextMedium As Boolean
    Public optFontAlphaTextLarge As Boolean
    Public optFontControlPointSmall As Boolean
    Public optFontControlPointSmaller As Boolean
    Public optFontControlPointLarge As Boolean
    Public optFontAlphaTextSmall As Boolean
    Public optFontAlphaTextUseDatabaseValue As Boolean
    Public optGeneralBulletinHeaderAdditions As Boolean
    Public optForceBox17Verbiage As Boolean

    Public optShowFinalTrainButtonOnSummary As Boolean
    Public optShowReleaseTrainButtonOnSummary As Boolean
    Public optPrintWarrantOnVoidEvent As Boolean
    Public optPromptToFinalTrainOnReportClear As Boolean
    Public optDisplayGeneralBulletinHeader As Boolean = True
    Public optRailroadDescriptorWithSubdivision As Boolean
    Public optDeFaultToNow As Boolean
    Public optShowCopiedByFieldForBulletins As Boolean
    Public optShowVoidForWarrants As Boolean
    Public optShowTroubleReport As Boolean
    Public optShowAllMaintainers As Boolean
    Public optEnableBulletinMenus As Boolean
    Public optDisablePromptSPAF As Boolean
    Public optPromptMOWReportClearDialog As Boolean
#End Region

#Region " BNSF OPTIONS "
    Public optEnableBNSFSpecificOptions As Boolean
    Public optEnableMovementPlanningOptions As Boolean
    Public optForceTagDeliveryToMovementPlanning As Boolean = False
    Public optDeleteStoredRouteDependenciesOnLoading As Boolean
    Public optDisableTIWSOccupancyCheckOnAdjacentZoneForVoidAuthority As Boolean = False
    Public optEnableZeroCheckOnMilePostForTags As Boolean = False
    Public optEnableAutomaticSlottedOffUponBlocking As Boolean
    Public optDisableRouteExecutionAlerts As Boolean = True
    Public optEnablePTCBOSAPIMode As Boolean



    Public optEnableAutomaticSignalSlottingWithTNT As Boolean
    Public gEnableCheckMultipleCodeStationInitialization As Boolean

    Public optLockJointStatusOnEICTNT As Boolean
    Public optEnableSendTrackTagToBusInfoCommunicationServer As Boolean

    Public optEnablePTCOperations As Boolean
    Public optDelimitTrackTimeAuthorityComponentsWithZero As Boolean
    Public optUseDigitalSignatureMessaging As Boolean = True

    Public optSendOsMessageWhenSymbolMoves As Boolean = True
    Public optShowReleaseVerbiageForBNSFMOW As Boolean
    Public gCheckForClearingIntoFailedTrack As Boolean
    Public optEnableTrainMiscMenuBNSF As Boolean

    Public optCommunicationTimeOutValueForETMS As Integer = 90
    Public optShowToolTipForPlanningPriority As Boolean
    Public optShowExceptedReleaseTimes As Boolean
    Public optOverrideSimulationMode As Boolean
    Public optPerformedDiagnosticCollection As Boolean = True
    Public optSendDiagnosticsToBusinessCommunicationServer As Boolean
    Public optShowSwitchLockWithSwitchPointMonitors As Boolean
    Public optSwitchPointMonitoringForBNSF As Boolean


    Public optOsTimesReverseOrder As Boolean
    Public optKeepAliveDatabaseCheck As Boolean = True
    Public optDoEventLogging As Boolean
    Public optDoCommCheck As Boolean
    Public optDoControlServerCheck As Boolean
    Public optDisableTWC As Boolean
    Public optFormatTrainSymbolRuleOne As Boolean
    Public optCheckForEIC As Boolean
    Public optTrackRoutesEnabled As Boolean = True
    Public optShowTrainTimeSummary As Boolean = True
    Public optShowMobileMessages As Boolean
    Public optTrackTagMilePostRounding As Boolean

    Public optShowRollupMenuItem As Boolean = True
    Public optDeviceBlockingSimpleRelease As Boolean
    Public optDisableDynamicBoxEightLimits As Boolean
    Public optUseNormalSwitchNotificationList As Boolean
    Public optUseSwitchNormalConfirmationVerbiage As Boolean
    Public optReportTrackWarrantEventByFED As Boolean

    Public optAllowInfiniteNumberOfAuthoritiesInABS As Boolean
    Public optEnableThreeLineAuthorityLimits As Boolean
    Public optEnablePrintFaxFunctionality As Boolean
    Public optCheckForBoxOneWarrantInSelectedABSLimits As Boolean
    Public optEnableOverviewForTWC As Boolean
    Public optUseSubdivisionStationTable As Boolean
    Public optEnableVehicleAssociationFunctionality As Boolean = True
    Public optFlipSubdivisionStationsOnOverview As Boolean
    Public optShowTrainSymbolRequestMenu As Boolean
    Public optSizeMainFormBasedOnPosition As Boolean
    Public optEnableSpacingForTWCContextMenu As Boolean
    Public optEnableTrainMiscContextMenu As Boolean
    Public optEnableTrainMiscContextMenuIssuanceTWC As Boolean

    Public optEnableSecondaryTrainPlacement As Boolean
    Public optEnableGeneralBulletinBNSFStyle As Boolean
    Public optOverLapAuthorityCount As Integer = 3
    Public optUseSpecificSwitchNotificationList As Boolean = True
    Public optOverrideDisableTrainMiscContextMenu As Boolean
    Public optRetryTimeOutValue As Int32 = 60000
    Public optEnableCommaDelimitingFormatingOnMOW As Boolean
    Public optEnableBoxEightNote As Boolean
    Public optEnableChkOsReportingTrackTWC As Boolean = True





    Public optEnableCheckTrnPTCStatusBeforeRollup As Boolean = False
    Public optDisableCheckTrnPTCStatusTATReportClear As Boolean = False
    Public optForceVehicleAssociationOnMOWAuthority As Boolean = False
    Public optEnableBNSFXingFormat As Boolean = False
    Public optEnableCheckRemoteLinkStatusOnApplyAction As Boolean = False
#End Region

#Region " DME OPTIONS "
    Public optDefaultWarrantToMOWID As Boolean
    Public optShowSecondaryAuthoritySummary As Boolean



    Public optPassWordProtectTerritoryInsert As Boolean
    Public optDisableCheckForReversePermission As Boolean
    Public optEnableDMEICESwitchPositionForm As Boolean
    Public optValidateReliefCrewAttributes As Boolean

    Public optEnableRadioBlockingRules As Boolean
    Public optApplyTrainSheetConfigurationForDME As Boolean
    Public optUseReleaseAuthorityVerbiageDME As Boolean
#End Region

#Region " IRR OPTIONS "
    Public optEnableDTC As Boolean
    Public optDisableDeviceBlockContextMenu As Boolean
    Public optDisableTagInfo As Boolean
    Public optTrainTypeRequired As Boolean
    Public optInitializeTrackSpeeds As Boolean
    Public optMPHDisplay As String = "MPH"
    Public optMilePostDisplay As String = "MP"
    Public optCBTCEnabled As Boolean
    Public gDTCMode As Boolean
    Public gRoundKilometerDecimalPointToOne As Boolean
    Public gIssueToIsNumericOnlyForTrains As Boolean
    Public optTrainIDColorChangeForCBTCEnabled As Boolean
    Public optLocomotivePositionTimeOut As Int32 = 120000
    Public optShowInstantMessages As Boolean
    Public optSendManualPositionReportToCBTC As Boolean
    Public optbPassengerLoad As Boolean = True
#End Region

#Region " TRE Options "
    Public optUseRailroadNameToFindSubdivision As Boolean
    Public optAllowTemporalMode As Boolean
    Public optEnableTrainDirectionLogicCheckForStoredRoutes As Boolean = True
    Public optShowTripLinkFunctionality As Boolean
    Public optFlopPassFrghtSpeedInFormA As Boolean
    Public optShowPassFrghtSpeedInFormAPrintOut As Boolean
    Public optUseSubdivisionDescriptor As Boolean = True
    Public optAllowGTBModify As Boolean
    Public optPerformDesendingOnBulletinList As Boolean
    Public optEnablePassengarSpeedOnFormA As Boolean
    Public optShowBulletinsInTrainSheet As Boolean
    Public optShowGetBulletinMenuItem As Boolean
    Public optFlopEngineerNameWhenLoading As Boolean
    Public optFlopConductorNameWhenLoading As Boolean
    Public optPassRedSignalOnRequestedSignalDisabled As Boolean
    Public optAllowNoSignalControlInitializationOnFailure As Boolean
    Public optAllowTrainTrackingThroughDisjointedOSTracks As Boolean 'NORTH JCT EXAMPLE WHERE 2TK IS SPLIT BETWEEN 1TK WHERE 2&1 SWITCH ARE REVERSE
    Public optDisplayJointNamesInPDF As Boolean
    Public optEnableSortDepartTimeButton As Boolean
#End Region

#Region " INDR Options "
    Public optLoadTrackRestrictionCodes As Boolean
    Public optEnableSafetySpotLight As Boolean
    Public optEnableSafetyRuleOfWeek As Boolean
    Public optEnableSecurityAlert As Boolean
    Public optEnableINDRSwitchPositionForm As Boolean
    Public optEnableAuthorityNumbersDefaultOn As Boolean = True
    Public optEnablePrintingCauseCodesForSpeedRestrictions As Boolean
    Public optEnablePassiveCrossing As Boolean
    Public optEnabaleINDRSpecficOptions As Boolean = False
#End Region

#Region " KCS Options "
    Public optEnableKCSSpecificOptions As Boolean
    Public optUseReleaseVerbiageForKCS As Boolean
    Public optUseReadRepeatMessageBoxOnAuthorityRelease As Boolean
    Public optLoadSubdivisinBulletinNumbering As Boolean
    Public optAllowIdentifyEngineNumericLengthToSix As Boolean
    Public optAllowOverrideItemSeven As Boolean
    Public optDisableItemTwoToItemFourDirectionPrompt As Boolean
    Public optDisallowTrainIdPlacementOnTrackWithAuthorityOfOtherTrain As Boolean
    Public optPostionAndOSTrainOnAuthorityReleaseWithSFAItemThreeIntoSiding As Boolean
    Public optUseWorkstationAuthorityNumbering As Boolean

    Public optEnableStandAloneTrainSummary As Boolean
    Public optEnableStandAloneAuthoritySummary As Boolean
    Public optVerifyTrainMovementOnMainTrackBeforeSymbolMove As Boolean
    Public optUseSingleFormAuthorityVersionII As Boolean
    Public optDisableTrainMovePromptOnAuthorityRelease As Boolean
    Public optEnableSubdivisionFilterForTrains As Boolean

    Public optReportClearSimpleRelease As Boolean
    Public optApplyTrainSheetConfigurationForKCS As Boolean
    Public optDisableVehicleAssociation As Boolean
    Public optDisplayPseudoTrainAsMainIdentifer As Boolean
    Public optDisableTrainSymbolTruncate As Boolean

    Public optEnableOverrideNonTransverableTrackCheck As Boolean
    Public optEnableOverrideDerailPositionCheck As Boolean
    Public optEnableOsReportingFormatKCS As Boolean
    Public optEnableOverrideForceItemOne As Boolean
    Public optDisableSimulateMenu As Boolean
    Public optEnableVoidAuthorityMenuItem As Boolean
    Public optEnableFontStringsForDevices As Boolean

    Public optEnableTrackLockModeDisablingAuthorityIssuance As Boolean
    Public optEnableGroupTerritoryLoading As Boolean
    Public optEnableTimeTableDatabaseTable As Boolean
    Public optEnableForceAuthorityPrintingOnAtStartUp As Boolean
    Public optEnablePrintTrainContextMenu As Boolean
    Public optDisableAlertBanner As Boolean
    Public optEnableTimeTableInformationPrintOut As Boolean
    Public optDisablePassRedSignalAlert As Boolean
    Public optDisableUnSolicatedSignalAlert As Boolean
    Public optEnableSubdivisionGeoReferencing As Boolean
    Public optEnableAutoTimeIncrementingForCrewHOS As Boolean = True
    Public optEnableClearAllAlertsMenu As Boolean
    Public optEnableFullScreenSnapShot As Boolean
    Public optEnableSingleScreenSnapShot As Boolean = True

    Public OptEnableRowAlternateColor As Boolean = False
#End Region

#Region " KCSM Options "
    Public optEnableChangeTrainDirectionMenu As Boolean = True
    Public optUseKCSMAuthorityForm As Boolean
    Public optEnableResendControlSpecial As Boolean
    Public optShowItemEightFullOverlapVerbiage As Boolean
    Public optDisableChangeTrafficMenuOption As Boolean
    Public optEnableItemTwoSevenToTrain As Boolean
    Public optEnableViewSubTrainSheets As Boolean
    Public optEnableMOWContextMenu As Boolean
    Public optEnableMOWQuerySelection As Boolean
    Public optEnableNoSendSwitchBitOnLockedSwitch As Boolean
    Public optUseMOWFormattingRules As Boolean = True
    Public optApplyTrainSheetConfigurationForKCSM As Boolean

#End Region

#Region " ACTA "
    Public optEnableBulletinFormTC As Boolean = True
    Public optIncludeAllLoadedSubdivisions As Boolean
    Public optOverrideNoBulletinCheck As Boolean
    Public optForceSubdivisionSelectForGTB As Boolean

    Public optEnableTrainReportingACTA As Boolean
    Public optEnableBulletinMilepostSuffix As Boolean
    Public optOverrideGeneralBulletinLocationRequired As Boolean
    Public optOverrideGeneralBulletinEngineIDRequired As Boolean
    Public optAddHomeRoadCodeForATCA As Boolean
    Public optShowNumberOfLinesInFormCBulletin As Boolean = True
    Public optShowSubdivisionDescriptionDetails As Boolean = True
    Public optAddAllSelectedSubdivisionsToGTBCoverPage As Boolean

    Public optEnableCheckPrinterFaxSelectionToContinue As Boolean
#End Region

#Region "FERROMEX "

    Public optEnableFXESpecificOptions As Boolean
    Public optEnableLocomotiveReportingSchemaForFerromex As Boolean
    Public optLocomotiveReportKeepAliveIntervalCheck As Integer = 2 ' in seconds
    Public optLogOsOnTWCAuthorityRelease As Boolean
    Public optEnableDirectionLoggingWithOsReports As Boolean
    Public optEnableOsTimeCheck As Boolean
    Public optEnableAllowControlPointCorrelation As Boolean
    Public optDisplayBulletinFerromex As Boolean
    Public optFilterAuthoritiesOnSplitTracksNotOwned As Boolean
    Public optPerformSidingLengthCheckInCTC As Boolean
    Public optEnableIssueToWithTrnAndMowPresent As Boolean
    Public optDisableCheckForBoxOneWarrantInSelectedTNTLimits As Boolean
    Public optMakeReportClearTimeReadonlyOnTWC As Boolean
    Public optAutoPopulateMOWInTTLimits As Boolean
    Public optOnlyAllowReleaseTTOnControlledTerritories As Boolean
    Public optForceTrafficDirectionCheckPassSameSignalDirection As Boolean
    Public optOnlyAllowTrafficChangeOnNonOccuppiedTrack As Boolean
    Public optForceTrafficDirectionCheckForSignalClearing As Boolean
    Public optDisallowSignalRwquestsIntoFailedLocations As Boolean
    Public optEnableLoadingOfCurrentMOWIDs As Boolean

    Public optUseFerromexAuthorityVerbiage As Boolean
    Public optSubBulletinFunctionalityEnabledForFXE As Boolean
    Public optEnableBulletinSubTypes As Boolean
    'Public optEnableDisplayBulletinsOnTracks As Boolean
    Public optEnableExpandOutForTWCComponents As Boolean = True
    Public optDisableTimeTableMenuGeneralOrders As Boolean
    Public optDisableTimeTableMenuSuptNotice As Boolean
    Public optUseSubdivisionAbbrAuthorityNumbering As Boolean
    Public optEnableTrainFunctionalityFXE As Boolean
    Public optEnableBulletinRequestMenu As Boolean
    Public optApplyTrainSheetConfigurationForFXE As Boolean
    Public optAddBulletinsToAuthority As Boolean
    Public optFormatEngineIDRuleFXE As Boolean
    Public optDisableSubdivisionNameOnAuthorityBuildForm As Boolean
    Public optDisplayAuthorityNumberOnlyOnAuthorityBuildForm As Boolean
    Public optForceIssueToComboToDropDownList As Boolean
    Public optForceBoxNine As Boolean = True
    Public optForceBoxEight As Boolean
    Public optDisableSummaryMenu As Boolean
    Public optDisallowTrackSelectionThroughBlockedSwitch As Boolean
    Public optPrintSpanishSingleFormAuthority As Boolean
    Public optForwardTrainToNextTerritoryOnRelease As Boolean
    Public optForceFocusOnReadInAuthorityBuildScreen As Boolean
    Public optDisableMaintenanceMainMenu As Boolean
    Public optDisableDisplayOptionsMainMenu As Boolean

    Public optDisallowItem8WithItem2or4 As Boolean
    Public optEnableOnlyOneItemFourTrain As Boolean
    Public optAllowItemFourWithConflictingAuthorityAndLocation As Boolean
    Public optDisplayScreenNameWithSubdivision As Boolean
    Public optDisallowAuthorityAfterItemTwoJoint As Boolean
    Public optEnableMOWQuerySelectionForFXE As Boolean
    Public optAllowItem3Or5OnlyOnExitPointAtSwitch As Boolean
    Public optEnableSendOSMessageFXE As Boolean
    Public optOverrideJobBriefingOnItemOne As Boolean
    Public optMoveTraintoNextAuthorityUponAuthorityRelease As Boolean
    Public optAllowItem2With8ThroughItem6With8 As Boolean
    Public optAllowTrainUnitDuplication As Boolean = True
    Public optCreatePseudoSymbol As Boolean
    Public optDisallowSpecialCharactersInTrainSymbol As Boolean
    Public optForceItem8WithItem6and7 As Boolean
    Public optPerformSidingLengthCheck As Boolean
    Public optPerformSidingLengthCheckTT As Boolean
    Public optDisableDestinationGroupBoxOnTrainSummary As Boolean
    Public optDisableGeneralAuthoritiesMenu As Boolean
    Public optDecreaseReadBackFormFont As Boolean
    Public optMilePostRoundFactor As Integer = 4
    Public optMilePostRoundFactorXing As Integer = 4
    Public optPerformSecondPromptForAuthorityRelease As Boolean
    Public optAllowItemTwoMenuWithinLimitsOfItem6and8ToTrain As Boolean
    Public optCheckForUserLogin As Boolean
    Public optDisableGeneralMOWMenu As Boolean
    Public optRemoveTrackDesignationFromReadBack As Boolean
    Public optEnableDisallowDuplicateEngineIDsToTrains As Boolean
    Public optUseFXETextFilePrinting As Boolean
    Public optDisableUseOfHighWaterAuthorityMark As Boolean
    Public optAllowUniqueTrainReportFXE As Boolean
    Public optUseTerritoryUniquenessForAuthorityNumbers As Boolean
    Public optUseChangableAtLocationBox As Boolean
    Public optForceCopyBy As Boolean
    Public optForceAtLocationField As Boolean

    Public optOnlyUseMOWFromList As Boolean
    Public optFillInCopyByField As Boolean
    Public optDisableSetAuthorityNumberToFurtherestNumber As Boolean = True
    Public optEnableDirectionalItemSevenAuthorities As Boolean
    Public optFilterTrainsForItemFourAuthoritiesInTheIssueTo As Boolean
    Public optEnbaleEngineIDTrackLineFormatting As Boolean
    Public optEnbaleSwitchMoveOnTNT As Boolean = True
    Public optDisableSFAonSwitchBlock As Boolean = False
    Public optDisableBox9OnTNT As Boolean = False
    Public optFXEFormat As Boolean = False
    Public optEnableAutoRouting As Boolean
    Public optEnableGenerateRoutePlan As Boolean = True
    Public optShowUNTILTIMEBox9NotChecked As Boolean = True
    Public optShowAuthoritySoleBox8NotChecked As Boolean = True
    Public optShowBehindTrainsBox7NotChecked As Boolean = True
    Public optBlockTNTonDeviceBlock As Boolean = False

    Public optEnableNorthSouthRollupOnly As Boolean = False

    Public optEnableTrackTimeDisplayonCodeBrown As Boolean = False
    Public optEnableTrackTimeIssueOnCodeBrown As Boolean = False
    Public optEnableBeepOnTrainOS As Boolean = False

    Public optEnbaleSwitchMoveOnNonTranversable As Boolean = True
    Public optEnableCodeFailureMiscDevice As Boolean
    Public optDisableShowingCodeFailureOnComponents As Boolean = False
    Public optEnableAuthorityReleaseTimer As Boolean
    Public optEnableSpanishResourceLanguageFile As Boolean
#End Region

#Region "MRS OPTIONS"
    Public optEnableSuppressFailureModePromptsForPTC As Boolean
    Public optEnableAllowSwitchAlignmentWithPassStopSignalInCodeFailure As Boolean
    Public optEnableAllowSwitchControlMovementWhileInCodeFailure As Boolean
    Public optDisableDateRangeFunctionForAuthorityNumber As Boolean
    Public optOverrideRemoteCheckForJointAuthorization As Boolean
    Public optEnableLostOccupancyAlert As Boolean
    Public optEnableLostOccupancyAlertWithTrackBlock As Boolean
    Public optEnableAutoGenerateClearance As Boolean
    Public optEnableTrainMenuCancelStoredRoutes As Boolean
    Public optAllowTrafficChangeOnTrainDirectionChange As Boolean
    Public optOverrideReadRepeatForRestrictionDelivery As Boolean
    Public optColorStatusPTCIssuancePending As Color = Color.PaleGreen
    Public optColorStatusPTCIssuanceImminent As Color = Color.YellowGreen
    Public optColorStatusPTCReleasePending As Color = Color.LightPink
    Public optColorStatusPTCReleaseImminent As Color = Color.Crimson
    Public optEnableMRSSpecificOptions As Boolean = False
    Public optEnableClearSignalWithSignalIntime As Boolean
    Public optEnableTrainStopOnPowerOffManualOperateCheck As Boolean
    Public optEnableTrainStopOnOccupancyCheck As Boolean
    Public optEnableTrainStopOnDerailmentDetectorCheck As Boolean
    Public optEnableTrainStopOnCommunicationFailureCheck As Boolean
    Public optEnableTrainMenuInstantMessage As Boolean
    Public optEnableTrainStopCommand As Boolean
    Public optEnableTrainConsistRequest As Boolean
    Public optEnableGeoFenceOperations As Boolean
    Public optEnableDirectionBindNorthToEastAndSouthToWest As Boolean
    Public optEnableTrainSimulatorMenu As Boolean
    Public optEnableBindingEntryExitTracksToStationReporting As Boolean
    Public optEnableOnlyShowFailureOnCodeStationComponents As Boolean
    Public optEnableMRSSpecificPTCOperations As Boolean
    Public optEnableExtendLicenseinSplitDesk As Boolean
    Public optEnableSplitLicensesBetweenTerritories As Boolean
    Public optEnableStoredLicenseForFreeArea As Boolean
    Public optEnableAutoBlockSwitchesInSignalRoute As Boolean
    Public optEnableLicenseGenerationForPTC As Boolean
    Public optAllowDisableAutomaticSignalSlottingForUnSolicatedSignals As Boolean
    Public optAllowDisableAutomaticSignalSlotting As Boolean
    Public optAllowSecondsInOSReportLogging As Boolean
    Public optEnableCallOnLatching As Boolean
    Public optUseLicensingMethodForAuthority As Boolean
    Public optEnableAllowStoreRouteExecuteOutOfOrderBasedOnSelection As Boolean
    Public optAllowDeviceBlockOnSameNamedTrackOnly As Boolean
    Public optEnableCheckLicenseExistenceOnTrainMove As Boolean
    Public optEnableConflictChecksForTWCOSTracksMRS As Boolean
    Public optAllowSwitchLegSelectionForSwitchControl As Boolean
    Public optEnableCheckForAuthoritiesBeforeFinalTrain As Boolean
    Public optEnableUsingTooltipField As Boolean
    Public optDisableReadRepeatProcess As Boolean = False
    Public optDisablePOSSwitchDialog As Boolean = False
    Public optAutoReleaseLicenseOnTrainMove As Boolean
    Public optUseDurationTimeForTags As Boolean
    Public optUseDurationTimeForDeviceBlocks As Boolean
    Public optUseSingleFormAuthorityVersionMRS As Boolean
    Public optEnableTrainPositionReportingForMRS As Boolean
    Public optEnableNeedOccupancyToPositionMoveTrain As Boolean
    Public optEnableNeedOccupancyToPositionMoveTrainElseDeviceBlock As Boolean
    Public optDeviceBlockCommentsRequired As Boolean
    Public optDisableTagDelayInfo As Boolean
    Public optDisableTagDirection As Boolean
    Public optApplyTagConfigurationForMRS As Boolean
    Public optEnableDirectionalSymbolsOnTrainSymbol As Boolean
    Public optEnableGenerateAlarmNonIdentifiedOccuppancy As Boolean
    Public optEnableSidingCalculationMouseHover As Boolean
    Public optEnableForceAllTrackInfoMouseHover As Boolean
    Public optEnableAuthorityIssuanceMRS As Boolean
    Public optFormatTrainSymbolRuleMRS As Boolean
    Public optDisableMenuItemsForMRS As Boolean
    Public optEnablePortugueseResourceLanguageFile As Boolean
    Public optEnableCTCFunctionalityMenus As Boolean = True
    Public optApplyTrainSheetConfigurationForMRS As Boolean
    Public optTrainSymbolQuickRouting As Boolean
    Public optEnableTrainConsistAbbreviated As Boolean
    Public optEnableTagReadRepeat As Boolean = False
    Public optTWCOSEventType As String = String.Empty
    Public optEnableAutomaticPositionReportRequesting As Boolean
    Public optEnableAutomaticCutoutFromActiveToDisengagedState As Boolean
    Public optEnableTWCSpecialLicense As Boolean
#End Region

#Region "RUMO OPTIONS"

    Public optApplyTrainSheetConfigurationForRUMO As Boolean

#End Region

#Region "KCT OPTIONS"
    Public optApplyTrainSheetConfigurationForKCT As Boolean

#End Region

#Region "NCTD OPTIONS"
    Public optEnableHideAutomaticSignalsOnLoad As Boolean
    Public optEnableHideHandSwitchesOnLoad As Boolean

    Public optUseSingleFormAuthorityVersionNCTD As Boolean
    Public optShowAutoRoutingEnableOptionFromReleaseTNT As Boolean = True
    Public optEnableMisrouteProtection As Boolean
    Public gMisrouteProtectionEnabled As Boolean
    Public optEnableNCTDSpecificOptions As Boolean = False
    Public optDisableRequestedSignalCheck As Boolean
    Public optEnableNearSideSignalControl As Boolean
    Public optEnableNCTDAdditionalRestrictionForm As Boolean
    Public optEnableVerbiageSpecialParseOnTNT As Boolean
    Public optEnableAuthorityExpirationAlert As Boolean
    Public optEnableLoadGradeCrossingRecords As Boolean
    Public optEnableTrainRestrictionDelivery As Boolean
    Public optApplyTrainSheetConfigurationForNCTD As Boolean

#End Region

#Region "DENVER OPTIONS"
    Public optEnableTrainFunctionalityDenver As Boolean
    Public optApplyTrainSheetConfigurationForDenver As Boolean
    Public optEnableSpecificFunctionalityForDenver As Boolean
    Public optEnableTractionPowerManagementSystem As Boolean
    Public optEnableDenverSpecificOptions As Boolean = False
    Public optEnableSendOSMessageDenver As Boolean
#End Region

#Region "iTMDS OPTIONS"

    Public optEnableReportingToMobileApplication As Boolean
    Public optEnableBackChannelCommunications As Boolean

#End Region

#Region "PTC VETMS OPTIONS"
    Public optForceDisablePTCEquippedBox As Boolean
    Public optDisableFlashingTrackForPTC As Boolean
    Public optEnableGlobalPTCFunctions As Boolean = False
    Public gBOSInComm As Boolean = False
    Public optEnablePTCBOSVETMSMode As Boolean
    Public gPTCTimeOutTransmissionAuthorityRollup As Integer = 60000
    Public gPTCTimeOutTransmissionAuthorityIssuance As Integer = 180000
    Public gPTCTimeOutTransmissionAuthorityRelease As Integer = 60000
    Public gPTCTimeOutTransmissionBulletinIssuance As Integer = 60000
    Public gPTCTimeOutTransmissionBulletinCancel As Integer = 60000
    Public gEnablePTCVETMSTrackingOnCTC As Boolean = False
    Public gUseTrackAuthorityTypeInPlaceOfTrackAndTime As Boolean = False
    Public gEnablePTCVETMSTrackSegmentSplit As Boolean = False
    Public gEnablePTCVETMSTrackSegmentSplitMiles As Integer = 10
    Public gEnablePTCVETMSRebuildTrackSegmentOnRollup As Boolean = False
    Public gEnableIncludeVoidAuthorityIn1051 As Boolean = False
    Public gEnablePTCVETMSTrackingOnYardLimits As Boolean = False
    Public optLocomotiveDefaultLength As Integer = 75
    Public optLocomotiveDefaultWeight As Integer = 215
    Public optLocomotiveDefaultHorsePowerEffective As Integer = 4400
    Public optLocomotiveDefaultHorsePowerTotal As Integer = 4400
    Public optLocomotiveDefaultDirection As String = "FORWARD"
    Public optLocomotiveDefaultStatus As String = "R"
    Public optLocomotiveDefaultNumOfAxles As Integer = 4
    Public gEnableIgnoreCrewACKForMandatoryDirectives As Boolean = False
    Public gEnableIgnoreCrewACKForBOX1Void As Boolean = False
    Public optUseCrossingEdgeForXingBulletin As Boolean = False
    Public optUseNewComponetLoadPTCInfo As Boolean = False
    Public gEnablePTCVETMSTrackSegmentMultipleSubs As Boolean = False
    Friend Class clsPTCVETMSTrackSegmentSplit
        Public CreateDate As Date
        Public BeginMP As Decimal
        Public EndMP As Decimal
        Public TrackCollection As New ArrayList
    End Class
    Public gEnableManeuverZone As Boolean = False
#End Region

#Region "BC GTB OPTIONS "
    Public optIncludePrinterFaxOptionInGTBIssuanceProcess As Boolean
    Public optIncludePrinterEmailOptionInGTBIssuanceProcess As Boolean
#End Region

#Region " OBJECT CHECKSUM(INTERGRITY) OPTIONS "

    Public optEnableSFACheckSum As Boolean = False
    Public optEnableTNTCheckSum As Boolean = False
    Public optEnableSPLAuthorityCheckSum As Boolean = False
    Public optEnableTAGCheckSum As Boolean = False
    Public optEnableDeviceBlkCheckSum As Boolean = False
    Public optEnableTGBOCheckSum As Boolean = False
    Public optEnableTrackBulletinCheckSum As Boolean = False
    Public optEnablePasswordCheckSum As Boolean = False

    Public gSFACheckSumVariables As New ArrayList
    Public gTNTCheckSumVariables As New ArrayList
    Public gSPLAuthorityCheckSumVariables As New ArrayList
    Public gTAGCheckSumVariables As New ArrayList
    Public gDeviceBlkCheckSumVariables As New ArrayList
    Public gTGBOCheckSumVariables As New ArrayList
    Public gTrackBulletinCheckSumVariables As New ArrayList

    Public optEnableValidationSFACheckSumOnLoad As Boolean = False
    Public optEnableValidationTNTCheckSumOnLoad As Boolean = False
    Public optEnableValidationSPLCheckSumOnLoad As Boolean = False
    Public optEnableValidationTAGCheckSumOnLoad As Boolean = False
    Public optEnableValidationDeviceBLKCheckSumOnLoad As Boolean = False
    Public optEnableValidationTGBOCheckSumOnLoad As Boolean = False
    Public optEnableValidationTrackBulletinCheckSumOnLoad As Boolean = False
    Public optEnableValidationMandatoryDirectivesCheckSumOnLoad As Boolean = False



#End Region

#Region " RESOURCE FILE SETTINGS "

    Public optUseResourceCultureFile As Boolean
    Public gSpanishResoureFileName As String = "SpanishLanguageResource"

#End Region

#End Region

#Region "MOBILE TRACKING"
    Public gVehicleCollection As New ArrayList()
    Public gDebugMobileTracking As Boolean
    Public gSummaryQueue As Queue = Queue.Synchronized(New Queue())

    Public gLocomotiveCollection As New ArrayList()
    Public gLocomotiveSummaryQueue As Queue = Queue.Synchronized(New Queue())


#End Region

#Region "GENERAL BULLETINS"
    Public gGroupBoxGeneralBulletinsName As String = "GENERAL BULLETINS - ISSUED"
    Public gfrmGeneralBulletinName As String = "GENERAL BULLETINS"
#End Region

#Region "MOW FOREMEN"

    'Public gMowForemen As New ArrayList()
    Public gMowForemen As ArrayList = ArrayList.Synchronized(New ArrayList())
    'CMV SCRRA-250
    Public optEnableMetrolinkSpecificCallSignRule As Boolean = False
#End Region

#Region "STORED ROUTES"
    Public gStartStoredRouteCheckThread As Threading.Thread
    Private pgDoStoredRoute As Boolean
    Public gStoredRouteList As ArrayList
    Public gStoreRoutesLoad As Boolean = True
    Public gSummaryRouteQueue As Queue = Queue.Synchronized(New Queue())
    Public gStackRoute As Boolean
    Public gStackMode As Boolean
    Public gGropupedStoredRoute As Boolean
    Public gGropupedStoredRouteParent As StoredRoute = Nothing

    Public Property gDoStoredRoute() As Boolean
        Get
            Return pgDoStoredRoute
        End Get
        Set(ByVal Value As Boolean)
            pgDoStoredRoute = Value
        End Set

    End Property

#End Region

#Region "REMOTE LINKS"

    Public gRemoteLinkPortListener As TMDS.NetworkListener.PortListener
    Public gRemoteLinkHashTable As New Hashtable()

    Public gTracksNeedingLinks As New ArrayList()
    Public gRemoteLinkTracks As New ArrayList()
    Public RemoteLinkTimeToLive As Integer = -15
    Public optEnableShadowModeRemoteLink As Boolean = False
#End Region

#Region "ALERTS"

    Public gSystemAlertList As ArrayList = ArrayList.Synchronized(New ArrayList())
    Public gSystemAlertFile As String = Application.StartupPath & "\Alerts\AlertLog.txt"
    Public gSystemAlertQueue As Queue = Queue.Synchronized(New Queue())
    Public gSystemAlertExists As Boolean
    Public gSystemMobileMessageExists As Boolean

    Public gSystemAlertCritical As Boolean
    Public gSystemAlertMedium As Boolean
    Public gSystemAlertPTC As Boolean
    Public gSystemAlertLow As Boolean
    Public gSystemAlertFlashOn As Boolean
    Public gSystemAlertVerbiage As String
    Public gSystemAlertOff As Boolean
    Public gSystemAlertType As Object

    Public gAlertTabSelectionTime As Date = Now
    Public gSummaryAlertQueue As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "SMART MOBILE CLIENT"

    Public gMobileClientMessages As ArrayList = ArrayList.Synchronized(New ArrayList())
    Public gMobileClientMessageSummaryQueue As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "INSTANT MESSAGING"

    Public gInstantMessages As ArrayList = ArrayList.Synchronized(New ArrayList())
    Public gInstantMessageSummaryQueue As Queue = Queue.Synchronized(New Queue())
    Public gInstantMessengerList As New ArrayList()
    Public gInstantMessengerMessages As New ArrayList()
    Public gInstantMessengerQueue As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "SYSTEM MESSAGES"

    Public gSystemMessageSafetyRuleOfTheWeek As String
    Public gSystemMessageSafetySpotLight As String
    Public gSystemMessageSecurityAlert As String

#End Region

#Region "SCREEN SHOT CAPTURING"

    Public gScreenShotCaptureThread As Threading.Thread
    Public gScreenShotCaptureGUID As String

#End Region

#Region "FORM MESSAGE QUEUES"

    Public gMessageQueuefrmSingleFormAuthority As Queue = Queue.Synchronized(New Queue())
    Public gMessageQueuefrmReportClear As Queue = Queue.Synchronized(New Queue())
    Public gMessageQueuefrmReportClearTNT As Queue = Queue.Synchronized(New Queue())
    Public gMessageQueuefrmTrackAuthority As Queue = Queue.Synchronized(New Queue())
    Public gMessageQueuefrmMessageBox As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "CULTURE INFO"

    Public gLocale As String
    Public NotInheritable Class TMDSLocale
        Public Shared English As String = "en-US"
        Public Shared Spanish As String = "es-ES"
        Public Shared Portuguese As String = "pt-BR"
    End Class
    Public optSystemMessageString1 As String = "NETWORK IS DOWN OR DATABASE SERVER IS DOWN"
    Public optSystemMessageString2 As String = "SYSTEM ALERT"
    Public optSystemMessageString3 As String = "SYSTEM MESSAGE"
    Public optSystemMessageString4 As String = "SYSTEM DATABASE ERROR! PLEASE CONSIDER TRACK AUTHORITY ("
    Public optSystemMessageString5 As String = ") NOT IN EFFECT!"
    Public optSystemMessageString6 As String = "CONTACT SYSTEM ADMINISTRATOR IMMEDIATELY!"
    Public optSystemMessageString7 As String = "SYSTEM ERROR! PLEASE CONSIDER TRACK AUTHORITY ("
    Public optSystemMessageString8 As String = ") NOT IN EFFECT! CONTACT SYSTEM ADMINISTRATOR IMMEDIATELY!"
    Public optSystemMessageString9 As String = "PLEASE REVIEW THE AUTHORITY RELEASE - ( "
    Public optSystemMessageString10 As String = " SECONDS)"
    Public optSystemMessageString11 As String = "AUTHORITY ACTIVITY IN TAG LIMITS."
    Public optSystemMessageString12 As String = "SIGNAL ACTIVITY IN TAG LIMITS."
    Public optSystemMessageString13 As String = "TRACK TAG CHECK"
    Public optSystemMessageString14 As String = "CONTINUE WITH TRACK TAG?"
    Public optSystemMessageString15 As String = "RELEASING LICENSE BEHIND"
    Public optSystemMessageString16 As String = "CONTINUE TO RELEASE THIS PORTION OF THE LICENSE?"
    Public optSystemMessageString17 As String = "RELEASE CONFIRMATION"
    Public optSystemMessageString18 As String = "SYSTEM DATABASE ERROR! PLEASE CONSIDER TGBO FOR ("
    Public optSystemMessageString19 As String = "DOES TRAIN MOVE"
    Public optSystemMessageString20 As String = "TO TRACK"
    Public optSystemMessageString21 As String = "ON THE STATION"

    Public optSystemAlertMessagePTC As String
    Public optSystemAlertTitlePTC As String = "MANDATORY DIRECTIVE ALERT"

    Public optSystemAlertMessage1 As String = "PLEASE ACKNOWLEDGE TRAIN PASSED RED SIGNAL ALERT"
    Public optSystemAlertMessage2 As String = "TRAIN PASSED RED SIGNAL"
    Public optSystemAlertMessage3 As String = "PASSED RED SIGNAL AT"
    Public optSystemAlertMessage4 As String = "PASSED RED SIGNAL ALERT"
    Public optSystemAlertMessage5 As String = "MAINTEANCE CONTROL ALERT"
    Public optSystemAlertMessage6 As String = "GTB EXCEPTION ALERT"
    Public optSystemAlertMessage7 As String = "PLEASE ACKNOWLEDGE UN-IDENTIFIED TRACK OCCUPANCY"
    Public optSystemAlertMessage8 As String = "UN-IDENTIFIED TRACK OCCUPANCY"
    Public optSystemAlertMessage9 As String = "UN-IDENTIFIED TRACK OCCUPANCY AT"
    Public optSystemAlertMessage10 As String = "UN-IDENTIFIED TRACK OCCUPANCY ALERT"
    Public optSystemAlertMessage11 As String = "POSITION A TRAIN?"
    Public optSystemAlertMessage12 As String = "WRONG PROCEDURE FOR TRAIN"
    Public optSystemAlertMessage13 As String = "CANT DOUBLE CLICK TO END TRACK!"
    Public optSystemAlertMessage14 As String = "TRACK BEGIN MUST BE DIFFERENT FROM END TRACK"
    Public optSystemAlertMessage15 As String = "PLEASE, ISSUE STORED LICENSE AGAIN!"
    Public optSystemAlertMessage16 As String = "ON THE CONTROLLED AREA"
    Public optSystemAlertMessage17 As String = "PLEASE, ISSUE LICENSE AGAIN!"
    Public optSystemAlertMessage18 As String = "STORED LICENSE FOR FREE AREA"

    Public optSystemValidationString1 As String = "COPIED BY FIELD CANNOT BE BLANK."
    Public optSystemValidationString2 As String = "FORM VALIDATION CHECK"
    Public optSystemValidationString3 As String = "AT LOCATION FIELD CANNOT BE BLANK."


    Public optDeviceBlockString1 As String = "AUTHORITY ACTIVITY IN BLOCKING LIMITS. CONTINUE WITH DEVICE BLOCK?"
    Public optDeviceBlockString2 As String = "OCCUPANCY ACTIVITY IN BLOCKING LIMITS. CONTINUE WITH DEVICE BLOCK?"
    Public optDeviceBlockString3 As String = "DEVICE BLOCK CHECK"

    Public mstrmnuSignalDrillON As String = "DRILL ON"
    Public mstrmnuSignalDrillOFF As String = "DRILL OFF"
    Public mstrmnuSignalCallOn As String = "CALL ON"
    Public mstrmnuSignalClear As String = "CLEAR"
    Public mstrmnuFleetOn As String = "FLEET ON"
    Public mstrmnuFleetOFF As String = "FLEET OFF"
    Public optGraphicalCompContextMenuSigBlockCreate As String = "BLOCK-CREATE"
    Public optGraphicalCompContextMenuSigBlockRemove As String = "BLOCK-REMOVE"

    Public mstrmnuRequestAuthorization As String = "REQUEST AUTHORIZATION"
    Public mstrmnuCancelRequestAuthorization As String = "CANCEL AUTHORIZATION REQUEST"
    Public optSignalAuth01 As String = "SIGNAL AUTHORIZATION REQUEST"
    Public optSignalAuth02 As String = "SIGNAL AUTHORIZATION FROM"
    Public optSignalAuth03 As String = "FOR SIGNAL"
    Public optSignalAuth04 As String = "AUTHORIZATION REQUEST"
    Public optSignalAuth05 As String = "SIGNAL"
    Public optSignalAuth06 As String = "COMM SERVER NOT FOUND!"
    Public optSignalAuth07 As String = "PLEASE, SEND STOP SIGNAL!"
    Public optSignalAuth08 As String = "TRAINS NOT FOUND IN THE DESK"
    Public optSignalAuth09 As String = "EXIST A JOIN AUTHORIZATION PENDING"
    Public optSignalAuth10 As String = "JOIN AUTHORIZATION CANCELLATION"
    Public optSignalAuth11 As String = "INFORM THE REASON FOR CANCELLATION"
    Public optSignalAuth12 As String = "DENY JOIN AUTHORIZATION"
    Public optSignalAuth13 As String = "REQUEST JOIN AUTHORIZATION IN"
    Public optSignalAuth14 As String = "SELECT A SYMBOL"
    Public optSignalAuth15 As String = "THERE AREN´T SYMBOL IN THE DESK"
    Public optSignalAuth16 As String = "PLEASE, SELECT A SYMBOL!"
    Public optSignalAuth17 As String = "PLEASE, FILL THE REASON!"
    Public optSignalAuth18 As String = "CONFIRM JOIN AUTHORIZATION?"
    Public optSignalAuth19 As String = "CONFIRMED"
    Public optSignalAuth20 As String = "DENY"
    Public optSignalAuth21 As String = "AUTHORIZATION NOT FOUND FOR TRAIN"
    Public optSignalAuth22 As String = "AND LOCO ID"
    Public optSignalAuth23 As String = "AUTHORIZATION CONFIRMED FOR TRAIN"
    Public optSignalAuth24 As String = "AUTHORIZATION DENY FOR TRAIN"
    Public optSignalAuth25 As String = "REASON"
    Public optSignalAuth26 As String = "JOIN AUTORIZATION REQUESTED FOR TRAIN"
    Public optSignalAuth27 As String = "AT LINE"
    Public optSignalAuth28 As String = "OF THE"
    Public optSignalAuth29 As String = "CANCELLATION TO JOIN AUTORIZATION REQUESTED FOR TRAIN"
    Public optSignalAuth30 As String = "LINE OR STATION END NOT FOUND FOR TRAIN"
    Public optSignalAuth31 As String = "EXIST A JOIN AUTHORIZATION FOR SIGNAL"
    Public optSignalAuth32 As String = "LINE"
    Public optSignalAuth33 As String = "INFORMED IS DIFFERENT THAN SENT"
    Public optSignalAuth34 As String = "NAME LINE REQUESTED FROM"
    Public optSignalAuth35 As String = "NAME LINE RESPONSE"
    Public optSignalAuth36 As String = "REQUEST NAME LINE"
    Public optSignalAuth37 As String = "AUTHORIZED ON LINE: "
    Public optSignalAuth38 As String = ", PLEASE REQUEST JOINT AUTHORIZATION"
    Public optSignalAuth39 As String = "LINE REQUEST NOT AUTHORIZED, PLEASE TRY REQUESTING AGAIN."
    Public optSignalAuth40 As String = "NAME LINE REQUEST"

    Public optLanguageDisplayFormName As String = "TRACK LINE DISPLAY - "
    Public optLanguageMessageBoxCaptionDefault1 As String = "SYSTEM MESSAGE"
    Public optControlPointManualOperate1 As String = "CONTROL POINT ("
    Public optControlPointManualOperate2 As String = ") CURRENTLY IN MONITOR MODE? RE-SEND CONTROL, ARE YOU SURE?"
    Public optLanguageShowFinalTrainMenuVerbiage As String = "DELETE TRAIN "
    Public optGraphicalCompContextMenuPositionTrain As String = "POSITION TRAIN"
    Public optGraphicalCompContextMenuPositionTrainManeuverLoad As String = "POSITION TRAIN MANEUVER LOAD"
    Public optGraphicalCompContextMenuPositionTrainManeuverEmpty As String = "POSITION TRAIN MANEUVER EMPTY"
    Public optGraphicalCompContextMenuPositionPlannedTrains As String = "PLANNED TRAINS"
    Public optGraphicalCompContextMenuPositionUpdatePlannedTrains As String = "UPDATE PLANNED TRAINS"
    Public optGraphicalCompContextMenuPositionMOW As String = "POSITION MOW ID"
    Public optGraphicalCompContextMenuRemoveMOW As String = "REMOVE MOW ID"
    Public optGraphicalCompContextMenuMoveMOW As String = "MOVE MOW ID"
    Public optGraphicalCompContextMenuForemanList As String = "FOREMAN LIST"
    Public optGraphicalCompContextMenuCreateGhostTrain As String = "CREATE GHOST TRAIN"

    Public optGraphicalCompContextMenuExtendLicenseinSlitDeskCreate As String = "EXTEND LICENSE"
    Public optGraphicalCompContextMenuExtendLicenseinSlitDeskCancel As String = "CANCEL LICENSE"
    Public optGraphicalCompContextMenuExtendLicenseinSlitDeskCancelStoredLicense As String = "CANCEL STORED LICENSE"

    Public optGraphicalCompContextMenuTNTIssue As String = "T&&T-ISSUE"
    Public optGraphicalCompContextMenuTNTIssueTrain As String = "T&&T-ISSUE-TRAIN"

    Public optGraphicalCompContextMenuTNTClear As String = "T&&T-REPORT CLEAR"
    Public optGraphicalCompContextMenuTNTModify As String = "T&&T-MODIFY"
    Public optGraphicalCompContextMenuTNTRollUp As String = "T&&T-ROLLUP"
    Public optGraphicalCompContextMenuTWCIssue As String = gTWCVariableName & "-ISSUE" '"TWC-ISSUE"
    Public optGraphicalCompContextMenuTWCClear As String = gTWCVariableName & "-REPORT CLEAR" '"TWC-REPORT CLEAR"
    Public optGraphicalCompContextMenuTWCRollUp As String = gTWCVariableName & "-ROLLUP" '"TWC-ROLLUP"
    Public optGraphicalCompContextMenuTWCVOID As String = gTWCVariableName & "-VOID" '"TWC-VOID"

    Public optGraphicalCompContextMenuTagCreate As String = "TAG-CREATE"
    Public optGraphicalCompContextMenuTagSpeed As String = "SPEED"
    Public optGraphicalCompContextMenuTagXing As String = "XING"
    Public optGraphicalCompContextMenuTagInfo As String = "INFO"
    Public optGraphicalCompContextMenuTagTrackAlarm As String = "TRACK ALARM"
    Public optGraphicalCompContextMenuTagWorkZone As String = "WORK ZONE"
    Public optGraphicalCompContextMenuTagWeather As String = "WEATHER"
    Public optGraphicalCompContextMenuTagRemove As String = "TAG-REMOVE"
    Public optGraphicalCompContextMenuTagModify As String = "TAG-MODIFY"
    Public optGraphicalCompContextMenuTagCopy As String = "TAG-COPY"

    Public optGraphicalCompContextMenuBulletinCreate As String = "BULLETIN-CREATE"
    Public optGraphicalCompContextMenuBulletinRemove As String = "BULLETIN-REMOVE"
    Public optGraphicalCompContextMenuBulletinSpeed As String = "SPEED RESTRICTION"
    Public optGraphicalCompContextMenuBulletinWork As String = "WORK ZONE"
    Public optGraphicalCompContextMenuBulletinOutOfService As String = "OUT OF SERVICE"
    Public optGraphicalCompContextMenuBulletinAdvisory As String = "ADVISORY"
    Public optGraphicalCompContextMenuBulletinCrossing As String = "CROSSING PROTECTION"
    Public optGraphicalCompContextMenuBulletinTrackCondition As String = "TRACK CONDITION"

    Public sBulletinTypeSR As String = "FORM A"
    Public sBulletinTypeWZ As String = "FORM B"
    Public sBulletinTypeDim As String = "FORM C"
    Public sBulletinTypeInstructional As String = "FORM F"
    Public sBulletinTypeOOS As String = "FORM O"
    Public sBulletinTypeXing As String = "FORM X"
    Public sBulletinTypeSlide As String = "FORM S"
    Public sBulletinTypeTrackCondition As String = "FORMTC"

    Public optGraphicalCompContextMenuBlockCreate As String = "BLOCK-CREATE"
    Public optGraphicalCompContextMenuBlockRemove As String = "BLOCK-REMOVE"
    Public optGraphicalCompContextMenuBlockOS As String = "BLOCK-OS"
    Public optGraphicalCompContextMenuTrackReset As String = "RESET-TRACK"
    Public optGraphicalCompContextMenuSwBlockCreate As String = "BLOCK-CREATE"
    Public optGraphicalCompContextMenuSwBlockRemove As String = "BLOCK-REMOVE"
    Public optGraphicalCompContextMenuSwBlockOS As String = "BLOCK-OS"


    Public optGraphicalCompContextMenu_mnuRECALL As String = "RECALL"
    Public optGraphicalCompContextMenu_mnuRESEND As String = "RESEND"
    Public optGraphicalCompContextMenu_mnuRoutes As String = "CANCEL STORED ROUTES"
    Public optGraphicalCompContextMenu_mnuReleaseControl As String = "RELEASE CONTROL"
    Public optGraphicalCompContextMenu_mnuRequestControl As String = "REQUEST CONTROL"


    Public optGraphicalCompContextMenu_mnuMCZ As String = "MAINTAINER CALL"
    Public optGraphicalCompContextMenu_mnuSMZ As String = "SNOW MELTER"
    Public optGraphicalCompContextMenu_mnuSMZStop As String = "STOP SNOW MELTER"
    Public optGraphicalCompContextMenu_mnuMaintenanceModeRequest As String = "REQUEST CONTROL"
    Public optGraphicalCompContextMenu_mnuMaintenanceModeRelease As String = "RELEASE CONTROL"

    Public optGraphicalCompProtectSwitch1 As String = "A SWITCH @ "
    Public optGraphicalCompProtectSwitch2 As String = " HAS TRACK AUTHORITY ISSUED OVER IT."
    Public optGraphicalCompProtectSwitch3 As String = "ARE YOU SURE IT IS SAFE TO PROCEED?"
    Public optGraphicalCompProtectSwitch4 As String = " HAS A TRACK BLOCK OVER IT."
    Public optGraphicalCompProtectSwitchMsgHeader As String = "PROTECT SWITCH VERIFICATION"
    Public optRollUpAuthorityString1 As String = "RELEASING THAT PORTION Of TRACK WARRANT NUMBER:"
    Public optRollUpAuthorityString2 As String = "BETWEEN "
    Public optRollUpAuthorityString3 As String = " AND "
    Public optRollUpAuthorityString4 As String = "AT "
    Public optRollUpAuthorityString5 As String = "SWITCHES NORMAL TO THE REAR EXCEPT SWITCH(ES) AT"
    Public optRollUpAuthorityString6 As String = "SWITCHES NORMAL EXCEPT SWITCH(ES) AT"
    Public optRollUpAuthorityString7 As String = "LEFT REVERSE POSITION AND KCS FORM 4751 COMPLETED."
    Public optRollUpAuthorityString8 As String = "SWITCHES NORMAL TO THE REAR."
    Public optRollUpAuthorityString9 As String = "SWITCHES NORMAL."
    Public optRollUpAuthorityString10 As String = "IS THAT CORRECT?"
    Public optRollUpAuthorityString11 As String = "SELECT YES TO CONFIRM REQUIRED BRIEFING HAS OCCURRED."
    Public optRollUpAuthorityString12 As String = "RELEASE CONFIRMATION"
    Public optRollUpAuthorityString13 As String = "THIS AUTHORITY HAS AN ITEM 4 CONTINGENT AUTHORITY."
    Public optRollUpAuthorityString14 As String = "THIS IS THE LAST TRACK IN THE ITEM 2 LIMITS."
    Public optRollUpAuthorityString15 As String = "SELECT YES IF TO RELEASE THE ENTIRE ITEM 2 LIMITS?"

    Public optRollUpAuthorityString16 As String = "SELECT 'S' TO ROLL UP SOUTH"
    Public optRollUpAuthorityString17 As String = "SELECT 'N' TO ROLL UP NORTH"
    Public optRollUpAuthorityString18 As String = "SELECT 'W' TO ROLL UP WEST"
    Public optRollUpAuthorityString19 As String = "SELECT 'E' TO ROLL UP EAST"
    Public optRollUpAuthorityString20 As String = "ROLL UP DIRECTION CONFIRMATION - AUTHORITY"
    Public optRollUpAuthorityString21 As String = "ROLL UP CONFIRMATION"

    Public optRollUpAuthorityString22 As String = "PROTECT OPEN SWITCH FROM RELEASE OF "
    Public optRollUpAuthorityString23 As String = "RELEASING THAT PORTION OF TRACK AUTHORITY NUMBER:"
    Public optRollUpAuthorityString24 As String = "REPORTING CLEAR OF TRACK AUTHORITY NUMBER:"
    Public optRollUpAuthorityString25 As String = " ROLLED-UP LIMITS TO"
    Public optRollUpAuthorityString26 As String = "RELEASING THAT PORTION OF AUTHORITY NUMBER:"
    Public optRollUpAuthorityString27 As String = "REPORTING CLEAR OF AUTHORITY NUMBER:"
    Public optRollUpAuthorityString28 As String = "VOID AUTHORITY NUMBER:"

    Public optRollUpAuthorityString29 As String = "THE AUTHORITY "
    Public optRollUpAuthorityString30 As String = "MUST BE REMOVED THROUGH THE SUMMARY"
    Public optRollUpAuthorityString31 As String = "OR THROUGH THE CANCEL LICENSE MENU"
    Public optRollUpAuthorityString32 As String = "BECAUSE THERE IS NO TRAIN LINKED TO IT ON CAD"

    'Public optRollUpAuthority_TitleOne_WN As String = "SELECT 'W/N' TO ROLL UP WEST/NORTH"
    'Public optRollUpAuthority_TitleTWO_ES As String = "SELECT 'E/S' TO ROLL UP EAST/SOUTH"
    'Public optRollUpAuthority_ButtonStringOne_WN As String = "W/N"
    'Public optRollUpAuthority_ButtonStringTwo_ES As String = "E/S"
    Public optRollUpAuthority_TitleOne_WS As String = "SELECT 'W/S' TO ROLL UP WEST/SOUTH"
    Public optRollUpAuthority_TitleOne_EN As String = "SELECT 'E/N' TO ROLL UP EAST/NORTH"
    'Public optRollUpAuthority_ButtonStringOne_WS As String = "W/S"

    Public optAuthString1 As String = "PROCEED FROM "
    Public optAuthString2 As String = " TO "
    Public optAuthString3 As String = " ON "
    Public optAuthString4 As String = "WORK BETWEEN "
    Public optAuthString5 As String = " AND "
    Public optAuthString6 As String = " ON "
    Public optAuthString7 As String = "RELEASING THAT PORTION OF TRACK & TIME AUTHORITY NUMBER:"
    Public optAuthString8 As String = "BETWEEN "
    Public optAuthString9 As String = " AND "
    Public optAuthString10 As String = "AT "
    Public optAuthString11 As String = "IS THAT CORRECT?"
    Public optAuthString12 As String = "RELEASE CONFIRMATION "
    Public optAuthString13 As String = "ON THE "
    Public optAuthString14 As String = " TRACK"
    Public optAuthString15 As String = " AND THE "
    Public optAuthString16 As String = " TRACK"
    Public optAuthString17 As String = "OTHER SOLE AUTHORITY(s) FOUND! WILL THIS AUTHORITY BE ISSUED TO THE SAME PERSON?"
    Public optAuthString18 As String = "Please Modify the Sole Authority First and Re-Issue This Authority"
    Public optAuthString19 As String = "A Conflict Condition Is Present That Disallows the Issuance of this Track Authority."
    Public optAuthString20 As String = "ARE THESE TRAIN(s) BY THE LOCATION WHERE "
    Public optAuthString21 As String = " WILL ENTER THE MAIN TRACK?"
    Public optAuthString22 As String = " AND HAVE YOU NOTIFIED THE PRECEDING TRAIN(S)?"
    Public optAuthString23 As String = "TRAIN BEHIND CONFIRMATION"
    Public optAuthString24 As String = "VOID TRACK WARRANT NO. "
    Public optAuthString25 As String = "VOID TRACK AUTHORITY"
    Public optAuthString26 As String = "SYSTEM BLOCK CHECK"
    Public optAuthString27 As String = "Track Warrant Limits Selected Has Device Blocking! - EXITING!"
    Public optAuthString28 As String = "TRACK AUTHORITY SIGNAL CHECK"
    Public optAuthString29 As String = "THE PRESENT SIGNAL CONDITION"
    Public optAuthString30 As String = "DISALLOWS TRACK AUTHORITY ISSUANCE!"
    Public optAuthString31 As String = "THE CURRENT TRACK SELECTION IS PAST A BLOCKED REVERSE SWITCH."
    Public optAuthString32 As String = "PLEASE STOP THE TRACK SELECTION AT THE BLOCKED SWITCH."
    Public optAuthString33 As String = "THIS AUTHORITY IS NOT CONSECUTIVE"
    Public optAuthString34 As String = "AND WILL RESULT IN A GAP OF AUTHORITY."
    Public optAuthString35 As String = "DO YOU WANT TO CONTINUE?"
    Public optAuthString36 As String = "AUTHORITY GAP CHECK"
    Public optAuthString37 As String = "ROLL UP EVENT FROM "
    Public optAuthString38 As String = "NONE"
    Public optAuthString39 As String = "NOT IN EFFECT UNTIL AFTER ARRIVAL OF"
    Public optAuthString40 As String = "NO TIME"
    Public optAuthString41 As String = "WARNING! THE CURRENT TRAIN LENGTH WILL NOT FIT INTO SIDING."
    Public optAuthString42 As String = "TRAIN LENGTH = "
    Public optAuthString43 As String = " AND SIDING LENGTH = "
    Public optAuthString44 As String = "CONTINUE THE AUTHORITY?"
    Public optAuthString45 As String = "MOW ID ALREADY EXISTS! ENTER A DIFFERENT ID."
    Public optAuthString46 As String = "ENTER ID"
    Public optAuthString47 As String = "FOREMAN NOT FOUND"
    Public optAuthString48 As String = "THE ISSUE TO DOES NOT MATCH THE CONFLICTING"
    Public optAuthString49 As String = "AUTHORITY'S AFTER ARRIVAL LIST."
    Public optAuthString50 As String = "HAS "
    Public optAuthString51 As String = " ROLLED-UP LIMITS TO"
    Public optAuthString52 As String = "THE BEGINNING OF THIS NEW AUTHORITY?"
    Public optAuthString53 As String = "SELECT YES TO CONFIRM."
    Public optAuthString54 As String = "ROLL UP CONFIRMATION"
    Public optAuthString55 As String = "TRACK & TIME"
    Public optAuthString56 As String = "AUTHORITY TO PASS RED SIGNAL"
    Public optAuthString57 As String = "AUTHORITY TO ENTER CTC LIMITS"
    Public optAuthString58 As String = "ARE THERE ANY SWITCHES THAT WILL BE"
    Public optAuthString59 As String = "INSTRUCTED TO PLACE IN HAND OPERATION?"
    Public optAuthString60 As String = "SWITCH CONFIRMATION"
    Public optAuthString61 As String = "PASS RED SIGNAL"

    Public optAuthString62 As String = "HAS AUTHORITY TO ENTER"
    Public optAuthString63 As String = "AND PROCEED"
    Public optAuthString64 As String = ""
    Public optAuthString65 As String = "AFTER STOPPING,"
    Public optAuthString66 As String = "AT"
    Public optAuthString67 As String = "HAS AUTHORITY TO PASS SIGNAL DISPLAYING STOP INDICATION"
    Public optAuthString68 As String = "AND IS INSTRUCTED TO OPERATE (SWITCH(ES))"
    Public optAuthString69 As String = "BY HAND FOR YOUR ROUTE"
    Public optAuthString70 As String = "FROM"
    Public optAuthString71 As String = "TO"
    Public optAuthString72 As String = "TRAINS DIRECTION WILL BE CHANGED BASED ON LICENSE DIRECTION"
    Public optAuthString73 As String = "PROCEED FROM SIGNAL"
    Public optAuthString74 As String = "ON TRACK"
    Public optAuthString75 As String = "TRACK AUTHORITY"
    Public optAuthString76 As String = "AUTHORITY NO:"
    Public optAuthString77 As String = "DATE"
    Public optAuthString78 As String = "TO SIGNAL"
    Public optAuthString79 As String = "TO"
    Public optAuthString80 As String = "IN SPEED RESTRICTION"
    Public optAuthString81 As String = "REASON"
    Public optAuthString82 As String = "TO"
    Public optAuthString83 As String = "AUTHORITY ALLOWED ONLY TO TRAIN TRACK"
    Public optAuthString84 As String = ""
    Public optAuthString85 As String = "DO NOT PASS SIGNAL THE FRONT"

    Public optAuthTrackAlert As String = "TRACK AUTHORITY ALERT"
    Public optAuthTrackAlertRouteEstablished As String = "A ROUTE APPEARS TO BE ESTABLISHED INTO THE REQUESTED LIMITS!"
    Public optAuthTrackAlertDispatcherResolved As String = "HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT"
    Public optAuthTrackAlertIsSafe As String = "AND IS IT SAFE TO CONTINUE THE AUTHORITY ISSUANCE?"


    Public optAuthBehindTrainAlert As String = "BEHIND TRAIN CHECK"
    Public optAuthBehindTrainMessage As String = "TO MANY BEHIND TRAINS FOR BOX SEVEN."

    Public optAuthJointMoreThan7Message As String = "Too Many Joint Authorities. Authority Form Only Allows Six Joint Authorities. - EXITING"

    Public optAuthStrOccupancy As String = "AN OCCUPANCY WAS FOUND WITH NO TRAIN ID ASSOCIATED!"
    Public optAuthStrHasDispResolved As String = "HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT?"
    Public optAuthStrTrkAuthAlert As String = "TRACK AUTHORITY ALERT"

    Public optAuthConflictString1 As String = "THE AUTHORITY CONFLICT COULD NOT BE RESOLVED. ERROR NUMBER:"
    Public optAuthDeviceBlockString1 As String = "TRACK AUTHORITY LIMITS SELECTED HAS DEVICE BLOCKING! - EXITING!"

    Public optAuthErrorAdding As String = "Error Adding Track Authority To System. CALL SYSTEM ADMINISTRATOR!"
    Public optAuthErrorAddingMsgBox As String = "SYSTEM ALERT MESSAGE"

    Public optAuthTypeSole As String = "TNT-SOLE"
    Public optAuthTypeJoint As String = "TNT-JOINT"
    Public optAuthTypeTPSole As String = "TP-SOLE"
    Public optAuthTypeTPJoint As String = "TP-JOINT"
    Public optAuthTypeNone As String = "NONE"
    Public optAuthType1 As String = "ITEM 6 TO TRAIN"
    Public optAuthType2 As String = "ITEM 6 TO MOW"
    Public optAuthType3 As String = "ITEM 2/4 TO TRAIN"
    Public optAuthType4 As String = "ITEM 2 TO TRAIN"
    Public optAuthType5 As String = "ITEM 4 TO TRAIN"

    Public optPDFEndTT As String = "END OF TRACK AUTHORITY NO."
    Public optPDFCopiedBy As String = "COPIED BY"
    Public optPDFRelayedTo As String = "RELAYED TO"

    Public optAuthCheckString1 As String = "BOX ONE IS CHECK"
    Public optAuthCheckString2 As String = "BUT NO AUTHORITIES SELECTED TO VOID!"
    Public optAuthCheckString3 As String = "AUTHORITY CHECK MESSAGE"
    Public optAuthCheckStringItem7Str1 As String = "BOX 7 IS CHECK BUT NO TRAIN IS SELECTED."
    Public optAuthCheckStringItem7Str2 As String = ""

    Public optAuthCheckString5 As String = "INVALID SELECTION"
    Public optAuthCheckString6 As String = "Current Track Selection Not Valid - Signal Traffic Type Conflict Found In Selection!"
    Public optAuthReadAuthorityString As String = "READ AUTHORITY"

    Public optAuthTrackTimeNoString As String = "TRACK AND TIME NO. "
    Public optAuthTrackTimeString As String = "TRACK AND TIME"
    Public optAuthTrackTimePrintString As String = "TRACK AND TIME AUTHORITY NO:"

    Public optTrainString1 As String = "TRAIN ARRIVED "
    Public optTrainString2 As String = "TRAIN DEPARTURED "
    Public optTrainString3 As String = " AT "
    Public optTrainString4 As String = "LOADS CHANGED FROM "
    Public optTrainString5 As String = "EMPTIES CHANGED FROM "
    Public optTrainString6 As String = "LENGTH CHANGED FROM "
    Public optTrainString7 As String = "TONS CHANGED FROM "
    Public optTrainString8 As String = " TO "
    Public optTrainString9 As String = "CREW REPLACED WITH RELIEF CREW"
    Public optTrainString10 As String = "ENG="
    Public optTrainString11 As String = "COND="
    Public optTrainString12 As String = "ON DUTY TIME"
    Public optTrainString13 As String = "HOS"
    Public optTrainString14 As String = "OFF DUTY TIME"
    Public optTrainString15 As String = "ENGINEER"
    Public optTrainString16 As String = "BRAKEMAN"
    Public optTrainString17 As String = "ON DUTY TIME HAS CHANGED. UPDATE THE HOURS OF SERVICE?"
    Public optTrainString18 As String = "ON DUTY TIME"
    Public optTrainString19 As String = "ENTER SYMBOL"
    Public optTrainString20 As String = "NO ENGINE"
    Public optTrainString21 As String = "NONE"
    Public optTrainString22 As String = "SELECT SYMBOL"
    Public optTrainString23 As String = "DIMSL CHANGED FROM "
    Public optTrainString24 As String = "TIH "
    Public optTrainString25 As String = "KEY "
    Public optTrainString26 As String = "ALERT "
    Public optTrainString27 As String = "SPEED CHANGED FROM "
    Public optTrainString28 As String = "DC CHANGED FROM "
    Public optTrainString29 As String = "AHP/TON CHANGED FROM "
    Public optTrainString30 As String = "CONSIST PROMOTED "
    Public optTrainString31 As String = "CONSIST EDITED "
    Public optTrainString32 As String = "MAX HOURS EXPIRES"
    Public optTrainString33 As String = "NUMBER OF AXLES CHANGED FROM"
    Public optTrainString34 As String = "NUMBER OF OPERATING BRAKES CHANGED FROM"
    Public optTrainLinkString1 As String = "TRAIN LINK FAILED:"
    Public optTrainLinkString2 As String = "CANNOT LINK TO YOURSELF"
    Public optTrainLinkString3 As String = "TRAIN LINK MESSAGE"
    Public optDirectionWest As String = "WEST"
    Public optDirectionEast As String = "EAST"
    Public optDirectionSouth As String = "SOUTH"
    Public optDirectionNorth As String = "NORTH"
    Public optDirectionNone As String = "NONE"
    Public optDirectionDescending As String = "LEFT"
    Public optDirectionAscending As String = "RIGHT"
    Public Const optDirectionLeft As String = "LEFT"
    Public Const optDirectionRight As String = "RIGHT"
    Public optRegularCaseDirectionWest As String = "West"
    Public optRegularCaseDirectionEast As String = "East"
    Public optRegularCaseDirectionSouth As String = "South"
    Public optRegularCaseDirectionNorth As String = "North"
    Public optRegularCaseDirectionNone As String = "None"
    Public optChangeDirectionString1 As String = "CHANGE DIRECTION OF "
    Public optChangeDirectionString2 As String = "FROM "
    Public optChangeDirectionString3 As String = "TO   "
    Public optChangeDirectionString4 As String = "ARE YOU SURE?"
    Public optChangeDirectionString5 As String = "DOES TRAIN"
    Public optChangeDirectionString6 As String = "DOES HAVE LICENSE NUMBER"
    Public optChangeDirectionString7 As String = "DOES HAVE SPECIAL LICENSE NUMBER"
    Public optChangeDirectionString8 As String = "IS IT NEED SEND AN IMMEDIATE STOP BEFORE."
    Public optChangeDirectionString9 As String = "ARE YOU SURE?"

    Public optChangeDirectionMessageHeader As String = "CHANGE TRAIN DIRECTION"
    Public optChangeDirectionString11 As String = "CHANGE DIRECTION OF LINKED TRAIN"
    Public optChangeDirectionString12 As String = "CHANGE DIRECTION TO"
    Public optChangeDirectionString13 As String = "CHANGE DIRECTION TO"
    Public optChangeDirectionString14 As String = "NO DIRECTION SET"

    Public optTrainReleasePrompt As String = "RELEASE TRAIN "
    Public optTrainReleaseHeader As String = "RELEASE TRAIN "
    Public optTrainFinalPrompt1 As String = "ARE YOU SURE YOU WANT TO DELETE THIS TRAIN:"
    Public optTrainFinalPrompt2 As String = "YOU WILL NOT BE ABLE TO USE THIS TRAIN AGAIN!"
    Public optTrainFinalHeader As String = "DELETE TRAIN DIALOG"

    Public optTrainCancelPrompt As String = "ARE YOU SURE YOU WANT TO CANCEL THIS TRAIN:"
    Public optTrainCancelHeader As String = "CANCEL TRAIN DIALOG"

    Public optTrainPositionString1 As String = "TRAIN CANNOT BE POSITIONED ON A TRACK"
    Public optTrainPositionString2 As String = "THAT HAS AN AUTHORITY FOR ANOTHER TRAIN."
    Public optTrainPositionString3 As String = "EXISTING TRAIN FOUND ON CTC FOR THIS TRACK AUTHORITY:"
    Public optTrainPositionString4 As String = "EXISTING TRAIN FOUND ON NON-" & gTWCVariableName & "FOR THIS TRACK AUTHORITY:"
    Public optTrainPositionString5 As String = "MOVE TRAIN SYMBOL TO BEGINNING OF AUTHORITY?"
    Public optTrainPositionString6 As String = "MOVE TRAIN ID CONFIRMATION"
    Public optTrainPositionString7 As String = "TRAIN CANNOT BE POSITIONED ON A TRACK"
    Public optTrainPositionString8 As String = "THAT HAS AN AUTHORITY FOR ANOTHER TRAIN."
    Public optTrainPositionString9 As String = "TRAIN SYMBOL EXCEPTION MESSAGE!"
    Public optTrainPositionString10 As String = "ENGINE ID USED ON ANOTHER TRAIN AND CANNOT BE DUPLICATED!"
    Public optTrainPositionString11 As String = "UNIT ID CANNOT BE LESS THAN 5 CHARACTERS"

    Public optTrainReleaseLicenseExitsPrompt As String = "LICENSES STILL EXIST FOR TRAIN: "
    Public optTrainReleaseLicenseExitsPromptConfirm As String = "PLEASE RELEASE LICENSES TO FINAL THE TRAIN."

    Public optTrainReleaseTGBOExitsPrompt As String = "OUTSTANDING TGBO STILL EXISTS FOR TRAIN."
    Public optTrainReleaseTGBOExitsPromptConfirm As String = "CANNOT DELETE TRAIN."

    Public optMOWReleaseString1 As String = "RELEASE MOW ID:"
    Public optMOWReleaseString2 As String = "RELEASE ID CONFIRMATION"
    Public optMOWReleaseString3 As String

    Public optBlockReleaseString1 As String = "REMOVE DEVICE BLOCK NAME: "
    Public optBlockReleaseString2 As String = "IS THAT CORRECT?"
    Public optBlockReleaseString3 As String = "REMOVE BLOCK CONFIRMATION"
    Public optBlockReleaseString4 As String = "CANT REMOVE DEVICE BLOCK. REASON=DEVICE BLOCK REMOTE"

    Public optBlockString1 As String = "DEVICE BLK:"

    Public optMilePostAbbreviation As String = "MP:"
    Public optMilePostAbbreviation1 As String = "TO MP:"
    Public optTrackNameSidingDefault As String = "SIDING"
    Public optTrackLengthLeftInSidingString As String = "FEET LEFT IN SIDING" 'metros livres neste bloco"


    Public optMOWIssueToAbbreviation As String = "MW"
    Public optMOWIDString1 As String = "MW ID"

    Public optTagString1 As String = "DELETE"
    Public optTagString2 As String = "SPEED RESTRICTION FROM"
    Public optTagString3 As String = "TO"
    Public optTagString4 As String = "GRADE CROSSING "
    Public optTagString5 As String = "AT MILEPOST"
    Public optTagString6 As String = "INFO:"
    Public optTagString7 As String = "TRACK TAG"
    Public optTagString8 As String = "TAG DELETE CONFIRMATION"
    Public optTagString9 As String = "RESTRICTIVE TRACK LABEL"
    Public optTagString10 As String = "SPEED RESTRICTION FOR PASSENGER TRAINS AT "
    Public optTagString11 As String = "SPEED RESTRICTION FOR FREIGHT TRAINS AT "
    Public optTagString12 As String = "MPH"
    Public optTagString13 As String = "BETWEEN MP"
    Public optTagString14 As String = "AND MP"
    Public optTagString15 As String = "ON"
    Public optTagString16 As String = " TRACK"
    Public optTagString17 As String = "PROTECT XING AT MP "
    Public optTagString18 As String = "XING NAME: "
    Public optTagString19 As String = "INSTRUCTIONS:"
    Public optTagString20 As String = "TRACK RESTRICTION:"
    Public optTagString21 As String = ""
    Public optTagString22 As String = "BOTH"
    Public optTagString23 As String = "NO DELAY"
    Public optTagString24 As String = "N/E ONLY"
    Public optTagString25 As String = "S/W ONLY"
    Public optTagString26 As String = "ENTER INFORMATION..."
    Public optTagString27 As String = "TRACK TAG?"
    Public optTagString28 As String = "CAN'T REMOVE TRACK TAG. REASON: TRACK TAG CREATED REMOTELY."

    Public optTagString_SignalActivity As String = "SIGNAL ACTIVITY IN TAG LIMITS. CONTINUE WITH TRACK TAG?"
    Public optTagStringNoFlags As String = "NO FLAGS DISPLAYED"
    Public optTagStringOneSpeed As String = "SPEED RESTRICTION OF "
    '

    Public optTagType1 As String = "SPEED"
    Public optTagType2 As String = "XING"
    Public optTagType3 As String = "INFO"
    Public optTagType4 As String = "WEATHER"
    Public optTagType5 As String = "TRACK BULLETIN"
    Public optTagType6 As String = "TRACK ALARM"
    Public optTagType7 As String = "WORK ZONE"

    Public optTagStateTrue As String = "TRUE"
    Public optTagStateFalse As String = "FALSE"

    Public optXferReportString1 As String = "TRANSFER REPORT"
    Public optXferReportString2 As String = "END OF TRANSFER REPORT"
    Public optXferReportString3 As String = "CURRENT TRAINS:"
    Public optXferReportString4 As String = "TRAIN SYMBOL:"
    Public optXferReportString5 As String = "ENGINE ID:"
    Public optXferReportString6 As String = "DIRECTION:"
    Public optXferReportString7 As String = "LAST OS:"
    Public optXferReportString8 As String = "NO:" '"AUTHORITY NO:"
    Public optXferReportString9 As String = "AUTHORITY TYPE:"
    Public optXferReportString10 As String = "ISSUE TO:"
    Public optXferReportString23 As String = "BOXES"

    Public optXferReportString11 As String = "OK TIME:"
    Public optXferReportString12 As String = "CURRENT AUTHORITIES"
    Public optXferReportString13 As String = "BOX TWO TO TRAIN"
    Public optXferReportString14 As String = "BOX SIX TO TRAIN"
    Public optXferReportString15 As String = "BOX SIX TO MOW"
    Public optXferReportString16 As String = "SUBDIVISION:"
    Public optXferReportString17 As String = "PAGE"
    Public optXferReportString18 As String = "OF"
    Public optXferReportString19 As String = "TRAIN"
    Public optXferReportString20 As String = "LIMITS"
    Public optXferReportString21 As String = "ROLL UP EVENTS:"
    Public optXferReportString22 As String = "LIMITS RELEASED"

    Public optSystemSummaryName As String = "SYSTEM SUMMARY"
    Public optSystemSummaryAuthPreviewFormName As String = "TRACK WARRANT PREVIEW"

    Public optTrainReportString1 As String = "END OF TRAIN REPORT"
    Public optTrainReportString2 As String = "TRAIN REPORT"
    Public optTrainReportString3 As String = "TRAIN REPORT FOR "
    Public optTrainReportString4 As String = " ON "
    Public optTrainReportString5 As String = "TRAIN SYMBOL: "
    Public optTrainReportString6 As String = "IDENTIFYING UNIT: "
    Public optTrainReportString7 As String = "DIRECTION: "
    Public optTrainReportString8 As String = "UNITS: "
    Public optTrainReportString9 As String = "EOTD: "
    Public optTrainReportString10 As String = "HOTD: "
    Public optTrainReportString11 As String = "MAX SPEED: "
    Public optTrainReportString12 As String = "HAZMAT: "
    'Code Added (optTrainReportString67) by Saba Sharif on May 28, 2013 for RMT Ref#-BRC-205
    'Public optTrainReportString67 As String = "RSSM: "
    Public optTrainReportString13 As String = "HIGH/WIDE: "
    Public optTrainReportString14 As String = "ORIGIN: "
    Public optTrainReportString15 As String = "DEPART TIME: "
    Public optTrainReportString16 As String = "LOADS: "
    Public optTrainReportString17 As String = "EMPTIES: "
    Public optTrainReportString18 As String = "TONS: "
    Public optTrainReportString19 As String = "LENGTH: "
    Public optTrainReportString20 As String = "DESTINATION: "
    Public optTrainReportString21 As String = "ARRIVAL  TIME: "
    Public optTrainReportString22 As String = "LOADS: "
    Public optTrainReportString23 As String = "EMPTIES: "
    Public optTrainReportString24 As String = "TONS: "
    Public optTrainReportString25 As String = "LENGTH: "
    Public optTrainReportString26 As String = "ENGINEER: "
    Public optTrainReportString27 As String = "ON DUTY: "
    Public optTrainReportString28 As String = "HOS: "
    Public optTrainReportString29 As String = "CONDUCTOR: "
    Public optTrainReportString30 As String = "ON DUTY: "
    Public optTrainReportString31 As String = "HOS: "
    Public optTrainReportString32 As String = "OSTIMES: "
    Public optTrainReportString33 As String = "TIME: "
    Public optTrainReportString34 As String = " SUB:"
    Public optTrainReportString35 As String = " LOCATION:"
    Public optTrainReportString36 As String = "CREW CHANGE EVENTS:"
    Public optTrainReportString37 As String = "CONSIST CHANGE EVENTS:"
    Public optTrainReportString38 As String = "ARRIVAL AND DEPARTURE EVENTS:"
    Public optTrainReportString39 As String = "NO"
    Public optTrainReportString40 As String = "SI"
    'Code Added (optTrainReportString66) by Saba Sharif on May 28, 2013 for RMT Ref#-BRC-205
    Public optTrainReportString66 As String = "YES"
    Public optTrainReportString41 As String = "NO"
    Public optTrainReportString42 As String = "TRAIN DELAY: "
    Public optTrainReportString43 As String = "TRAIN COMMENT: "
    Public optTrainReportString44 As String = "TRAIN UNIT COMMENT: "
    Public optTrainReportString45 As String = "EVENT TYPE: "
    Public optTrainReportString46 As String = "TYPE: "
    Public optTrainReportString47 As String = "TGBO: "
    Public optTrainReportString48 As String = "KEY: "
    Public optTrainReportString49 As String = "HELPER: "
    Public optTrainReportString50 As String = "MIS UID: "
    Public optTrainReportString51 As String = "HPT: "
    Public optTrainReportString52 As String = "TBOP: "
    Public optTrainReportString53 As String = "TOTAL HP: "
    Public optTrainReportString54 As String = "GROSS LEN: "
    Public optTrainReportString55 As String = "GROSS TONS: "
    Public optTrainReportString56 As String = "NUM OF AXLES: "
    Public optTrainReportString57 As String = "NUM OF BRAKES: "
    Public optTrainReportString58 As String = "RESTRICTIONS: "
    Public optTrainReportString59 As String = "FORM C: "
    Public optTrainReportString60 As String = "WORKS ORDERS: "
    Public optTrainReportString61 As String = "CURRENT AUTHORITES: "
    Public optTrainReportString62 As String = "CURRENT BULLETIN EXECPTIONS: "
    Public optTrainReportString63 As String = "SCHEDULE: "
    Public optTrainReportString64 As String = "CAR CONSIST: "
    Public optTrainReportString65 As String = "DELIVERED BULLETIN EXECPTIONS: "
    Public optTrainReportString67 As String = "BULLETIN EVENT(s): "

    Public optSystemDateTimeFormat1 As String = "MM-dd-yyyy HH:mm:ss" '"dd-MM-yyyy HH:mm:ss"
    Public optSystemDateTimeFormat2 As String = "MM-dd-yy HH:mm:ss"
    Public optSystemDateTimeFormat3 As String = "MM-dd-yy HH:mm"
    Public optSystemDateTimeFormat4 As String = "dd-MM-yy HH:mm"

    Public optBulletinString1 As String = "BULLETIN"
    Public optBulletinSystemDateTimeFormat As String = "dd-MM-yyyy HH:mm" '"dd-MM-yyyy HH:mm:ss"
    Public optBulletinString2 As String = " TO "
    Public optBulletinContainerName As String = "TGBO"
    Public optBulletinContainerLongName As String = "TABULAR GENERAL BULLETIN ORDER"
    Public optBulletinProperName As String = "GENERAL BULLETIN ORDER"
    Public optBulletinAbbrName As String = "GBO"

    Public mstrRelConfHeader As String = "REPORT CLEAR CONFIRMATION - "
    Public mstrTNT As String = "TRACK & TIME AUTHORITY NUMBER: "
    Public mstrReleaseAT As String = "IS RELEASED AT "
    Public mstrISThatCorrect As String = "IS THAT CORRECT?"

    Public optWeatherMessage1 As String = "WEATHER"
    Public optWeatherMessage2 As String = "ALERT: "
    Public optWeatherMessage3 As String = "WEATHER DATA ALERT MESSAGE RECIEVED."
    Public optWeatherMessage4 As String = "PLEASE ACKNOWLEDGE WEATHER ALERT"
    Public optWeatherMessage5 As String = "ACKNOWLEDGE WEATHER ALERT"
    Public optWeatherMessage6 As String = "CRITICAL WEATHER ALERT"
    Public optWeatherMessage7 As String = "SUBDIVISIONS - "
    Public optWeatherMessage8 As String = "BEGIN TIME - "
    Public optWeatherMessage9 As String = "EXPIRE TIME - "
    Public optWeatherMessage10 As String = "ALERT STATUS - "
    Public optWeatherMessage11 As String = "ALERT LEVEL - "
    Public optWeatherMessage12 As String = "ALERT TYPE - "
    Public optWeatherMessage13 As String = "BROADCAST WEATHER ALERT"
    Public optWeatherMessage14 As String = "ALERT DETAILS"

    Public optReportClear_TrackTimeAuthNumber As String = "TRACK & TIME AUTHORITY NUMBER"
    Public optReportClear_RptClearOnAuthNumber As String = "REPORTING CLEAR ON AUTHORITY NUMBER"
    Public optReportClear_IsReleasedFor As String = "IS RELEASED FOR"
    Public optReportClear_Movement As String = "MOVEMENT"
    Public optReportClear_At As String = "AT"
    Public optReportClear_IsReleasedAT As String = "IS RELEASED AT"
    Public optReportClear_For As String = "FOR "
    Public optReportClear_IsThatCorrect As String = "IS THAT CORRECT?"
    Public optReportClear_RptClearConfirmation As String = "REPORT CLEAR CONFIRMATION"
    Public optReleaseLicense As String = "RELEASE LICENSE? "
    Public optReportClear_RptClearAcknowledgement As String = "REPORT CLEAR ACKNOWLEDGEMENT"

    Public optReportClear_WarningElectronicAuth As String = "WARNING! THIS IS AN ELECTRONICALLY ISSUED AUTHORITY."
    Public optReportClear_PerformingManualRelease As String = "YOU ARE PERFORMING A MANUAL RELEASE. ARE YOU SURE?"
    Public optReportClear_RptClearPersonDoesNotMatch As String = "THE PERSON REPORTING AUTHORITY CLEAR IS NOT THE SAME PERSON IT WAS ISSUED TO!"
    Public optReportClear_IssueToPerson As String = "ISSUE TO PERSON:"
    Public optReportClear_RptClearPerson As String = "REPORT CLEAR PERSON:"

    Public optEASTWARD As String = "EASTWARD"
    Public optWESTWARD As String = "WESTWARD"
    Public optBoxesMarked As String = "BOX(ES) MARKED"

    Public mstrJoint As String = "-JOINT"
    Public mstrSOLE As String = "-SOLE"

    Public optReleaseDirectionSouth As String = "SOUTHWARD"
    Public optReleaseDirectionNorth As String = "NORTHWARD"
    Public optReleaseDirectionEast As String = "EASTWARD"
    Public optReleaseDirectionWest As String = "WESTWARD"
    Public optReleaseDirectionNone As String = "NO DIRECTION"
    Public optBehindTrain As String = "BEHIND TRAIN(s) "
    Public optAuthorityJointWith As String = "AUTHORITY IS JOINT WITH"
    'Public optReleaseDirectionWest As String = "WESTWARD"

    'Device Block
    Public optDevPOKName As String = "PO"
    Public optDevDOKName As String = "DO"

    'System Messages
    Public optSystemMessage As String = "SYSTEM MESSAGE"
    Public optConflictResolutionMsg As String = "CONFLICT RESOLUTION MESSAGE"
    Public optTrkAuthSignalCheckMsg As String = "TRACK AUTHORITY SIGNAL CHECK"
    Public optConflictAuthFound As String = "Current Track Selection Not Valid - Track and Time Authority Found In Selection!"
    Public optConflictPSS As String = "Current Track Selection Not Valid - Exsiting PASS RED SIGNAL Block Found In Selection!"
    Public optConflictEnterMainTrack As String = "Current Track Selection Not Valid - Exsiting ENTER MAIN TRACK Block Found In Selection!"
    Public optConflictSignalActivity As String = "Current Track Selection Not Valid - SIGNAL ACTIVITY Found In Selection!"
    Public optConflictTrackWarrant As String = "Current Track Selection Not Valid - Track Warrant Found In Selection!"
    Public optConflictNotValid As String = "Current Track Selection Not Valid!"

    'TRACK
    Public optGraphicalCompTrkLinkFailureMsgHeader As String = "AUTHORITY LIMITS CHECK"
    Public optGraphicalCompTrkLinkFailureMsg As String = "CONDITION EXISTS ON REMOTE LINK PREVENTING AN ISSUANCE OF AN AUTHORITY"

    'MISC DEVICES
    Public optCodeFailureDeviceName As String = "C F"

    'BULLETIN CELL
    Public optSystemMessageBCString1 As String = "PLEASE REVIEW THE BULLETIN DELETE - ( "
    Public optSystemMessageBCString2 As String = " SECONDS)"

    'TAG TIMER
    Public optSystemMessageTAGTimerString1 As String = "PLEASE REVIEW THE TAG DELETE - ("
    Public optSystemMessageTAGTimerString2 As String = "SECONDS)"

    'DEVICE BLOCK TIMER
    Public optSystemMessageDbBlkTimerString1 As String = "PLEASE REVIEW THE DEVICE BLOCK DELETE - ("
    Public optSystemMessageDbBlkTimerString2 As String = " SECONDS)"
    Public mstrfrmDeviceBlock_EnterComments As String = "ENTER COMMENTS..."
    Public mstrfrmDeviceBlock_CommentsForTrainProtect As String = "NO OCCUPANCY FOR TRAIN"

    'DERAILMENT DETECTOR
    Public optDerailmentDetectorString1 As String = "INHIBIT Derailment Detector".ToUpper 'Inibir detetor de descarrilamento
    Public optDerailmentDetectorString2 As String = "ENABLE Derailment Detector".ToUpper 'Habilitar detetor de descarrilamento
    Public optDerailmentDetectorString3 As String = "Derailment Detector Alert ON TRACK ".ToUpper ''Alarme de detetor de descarrilamento na linha
    Public optDerailmentDetectorString4 As String = "DERAILMENT ALARM" 'Alarme de descarrilamento

    'COMMON NAMES
    Public optCommonWordNone As String = "NONE"
    Public optCommonWordTO As String = "TO"

    'System Warning Messages - these were new that were added to Portuguese but are still absent from Spanish
    Public optSignalActivityInLimitsContinueMessage As String = "SIGNAL ACTIVITY IN BLOCKING LIMITS. CONTINUE WITH DEVICE BLOCK?"
    Public optOccupationInvalidSelectionMessage As String = "SELECTION CONTAINS ONE OR MORE TRACKS THAT HAS BEEN OCCUPIED BY OTHER TRAIN(s). INVALID SELECTION. EXITING !!!"
    '    Request Signal Authority
    'Requisitar sinal


    'Stop Signal Authority Request
    'Parar requisição de sinal

    'Generic Authority Errormessage
    Public optSystemAuthErrorMessageString1 As String = "SYSTEM DATABASE ERROR! PLEASE CONSIDER AUTHORITY ("
    Public optSystemAuthErrorMessageString2 As String = ") NOT IN EFFECT!"

    Public optCrewPositionEngineer As String = "ENGINEER"
    Public optCrewPositionConductor As String = "CONDUCTOR"


    'RESET GEO FENCE
    Public optResetGeoFenceHeader As String = "RESET GEOFENCE CONFIRMATION"
    Public optResetGeoFenceLine1 As String = "RESET GEOFENCE FOR TRAIN - "
    Public optResetGeoFenceLine2 As String = "RESET GEO FENCE WILL REMOVE ALL LICENSES!"
    Public optResetGeoFenceLine3 As String = "ARE YOU SURE?"

    'STOP TRAIN
    Public optStopTrainHeader As String = "STOP TRAIN CONFIRMATION"
    Public optStopTrainLine1 As String = "STOP THE TRAIN -"
    Public optStopTrainLine2 As String = ""
    Public optStopTrainLine3 As String = "ARE YOU SURE?"

    'DISABLE GEOFENCE
    Public optDisableGeoFenceHeader As String = "DISABLE GEOFENCE CONFIRMATION"
    Public optDisableGeoFenceLine1 As String = "DISABLE GEOFENCE FOR TRAIN -"
    Public optDisableGeoFenceLine2 As String = "DISABLING GEO FENCE WILL CAUSE THE TRAIN TO STOP!"
    Public optDisableGeoFenceLine3 As String = "ARE YOU SURE?"

    'DISABLE CBTC
    Public optDisableCBTCHeader As String = "DISABLE CBTC CONFIRMATION"
    Public optDisableCBTCLine1 As String = "DISABLE CBTC FOR TRAIN -"
    Public optDisableCBTCLine2 As String = "DISABLING CBTC WILL CAUSE THE TRAIN TO STOP!"
    Public optDisableCBTCLine3 As String = "ARE YOU SURE?"


    'CANCEL LICENCE
    Public optCancelLicenseHeader As String = "CANCEL LICENSE CONFIRMATION"
    Public optCancelLicenseLine1 As String = "CANCEL A PART OF THE LICENSE FOR TRAIN -"
    Public optCancelLicenseLine2 As String = "ARE YOU SURE?"
    Public optUntilCalled As String = "UNTIL CALLED"

    'PSS CANCEL
    Public mstrPSSCancelTitle As String = "CANCEL PASS RED SIGNAL"
    Public mstrPSSCancelLine1 As String = "CANCEL PASS RED SIGNAL AUTHORITY FOR TRAIN -"
    Public mstrPSSCancelLine2 As String = "ARE YOU SURE?"

    Public mstrPSSCancelLine3 As String = "CANCEL PASS RED SIGNAL AUTHORITY ["
    Public mstrPSSCancelLine4 As String = "] FOR TRAIN -"

    Public mstrPSSChoiceSwitch As String = "PLEASE SELECT A SWITCH FOR CONTINUE."

    Public optPSSDeviceBlockTitle As String = "DEVICE BLOCK CONFLICT CHECK"
    Public optPSSDeviceWarningBegin As String = "A DEVICE BLOCK(s) EXISTS WITHIN"
    Public optPSSDeviceWarningLimits As String = "THE SELECTED LIMITS!"
    Public optPSSDeviceWarningContinue As String = "DO YOU WANT TO CONTINUE?"

    Public optPssMessageTWELine1 As String = "A CONFLICTING ROUTE MAY EXIST."
    Public optPssMessageTWELine2 As String = "HAS THE DISPATCHER RESOLVED THIS CONFLICT?"


    'CBTC
    Public optPTCString001 As String = "HAS TRAIN CREW CUTOUT ETMS"
    Public optPTCString002 As String = "ARE YOU SURE?"
    Public optPTCString003 As String = "PTC CUTOUT DIALOG"
    Public optPTCString004 As String = "RESEND"
    Public optPTCString005 As String = "CUTOUT"
    Public optPTCString006 As String = "FALHA DE COMUNICAÇÃO CBTC"
    Public optPTCString007 As String = "LINK TO PTC IS DOWN"
    Public optPTCString008 As String = "REAL TIME FEEDBACK..."
    Public optPTCString009 As String = "TRAIN HAS BEEN ACTIVATED FOR PTC OPERATION"
    Public optPTCString010 As String = " IDENTIFYING UNIT "
    Public optPTCString011 As String = "TRAIN HAS BEEN DE-ACTIVATED FOR PTC OPERATION"
    Public optPTCString012 As String = "BRAKING ENFORCEMENT ON TRAIN "
    Public optPTCString013 As String = "BOS CONNECTION LOST"
    Public optPTCString014 As String = "UNABLE TO CONNECT TO BOS FOR PTC OPERATIONS. PLEASE CONTACT SYSTEM ADMINISTRATOR"
    Public optPTCString015 As String = "PTC EVENT NOTIFICATION ALERT"
    Public optPTCString016 As String = "PTC TRANSMISSION FAILURE ALERT"
    Public optPTCString017 As String = "CUTOUT CBTC"
    Public optPTCString018 As String = "ELECTRONIC MODIFY OF AUTHORITY"
    Public optPTCString019 As String = "TO TRAIN"
    Public optPTCString020 As String = "ELECTRONIC ROLLUP OF AUTHORITY"
    Public optPTCString021 As String = "ELECTRONIC ISSUANCE OF AUTHORITY"
    Public optPTCString022 As String = "ELECTRONIC RELEASE OF AUTHORITY"
    Public optPTCString023 As String = "FAILED. | PLEASE RE-TRY THE REQUEST!"
    Public optPTCString024 As String = "BOS CONNECTION ALERT"
    Public optPTCString025 As String = "TRAIN CONSIST ALERT"
    Public optPTCString026 As String = "TRAIN CREW REQUEST ALERT"
    Public optPTCString027 As String = "REJECTED. | PLEASE RE-TRY THE REQUEST!"
    Public optPTCString028 As String = "NEW CONSIST REQUEST FROM TRAIN CREW FOR TRAIN "
    Public optPTCString029 As String = "HOTBOX ALERT"
    Public optPTCString030 As String = "SCALE ALERT"
    Public optPTCString031 As String = "CREW ALERT"
    Public optPTCString032 As String = "PLANNED TRAINS"
    Public optPTCDefaultRestrictionType As String = "COMMON TRAIN"

    Public optFEDMessage As String = "PLEASE ACKNOWLEDGE FAILED EQUIPMENT DETECTOR ALERT"
    Public optFEDMessage1 As String = "FAILED EQUIPMENT DETECTOR"
    Public optFEDMessage2 As String = "ALERT: "
    Public optFEDMessage3 As String = "FED DATA ALERT MESSAGE RECIEVED."

    Public optFEDMessage4 As String = "OFFICE - "
    Public optFEDMessage5 As String = "NAME - "
    Public optFEDMessage6 As String = "PRIORITY - "
    Public optFEDMessage7 As String = "SUBNAME - "
    Public optFEDMessage8 As String = "TYPE - "
    Public optFEDMessage9 As String = "MILE FROM - "
    Public optFEDMessage10 As String = "FED MESSAGE - "
    Public optFEDMessage11 As String = "CRITICAL FED ALERT"
    Public optFEDMessage12 As String = "MILE TO - "

    'COMMUNICATION SERVER DOWN MESSAGE
    Public optstrCADCommDown As String = "UNABLE TO CONNECT TO CAD COMMUNICATION. PLEASE CONTACT SYSTEM ADMINISTRATOR"
    Public optstrGPSCommDown As String = "UNABLE TO CONNECT TO GPS COMMUNICATION. PLEASE CONTACT SYSTEM ADMINISTRATOR"
    Public optstrMISCommDown As String = "UNABLE TO CONNECT TO MIS COMMUNICATION. PLEASE CONTACT SYSTEM ADMINISTRATOR"
    Public optstrCADCommUp As String = "CAD COMMUNICATION CONNECTED FOR NORMAL OPERATIONS."
    Public optstrGPSCommUp As String = "GPS COMMUNICATION CONNECTED FOR NORMAL OPERATIONS."
    Public optstrMISCommUp As String = "MIS COMMUNICATION CONNECTED FOR NORMAL OPERATIONS."

    Public optstrControlSvrDown As String = "PLEASE CONTACT SYSTEM ADMINISTRATOR. UNABLE TO CONNECT TO CONTROL SERVER - "
    Public optstrControlSvrUP As String = " - CONTROL SERVER CONNECTED FOR NORMAL OPERATIONS."
    Public IsControlServerDisConnected As Boolean

    Public optTrainAlertString01 As String = "TRAIN ("
    Public optTrainAlertString02 As String = ") HAS LOST OCCUPANCY ALERT ON TRACK CIRCUIT "
    Public optTrainAlertString03 As String = " AT CONTROL POINT "
    Public optTrainAlertString04 As String = "TRAIN WARNING NOTIFICATION"
    Public optTrainAlertString05 As String = "TEXT MESSAGE EVENT NOTIFICATION"
    Public optTrainAlertString06 As String = "TEXT MESSAGE FROM TRAIN"
    Public optGTBConflictString As String = "THE GTB CONFLICT COULD NOT BE RESOLVED. ERROR NUMBER:"
    Public strLicense_TWCExistsForAnotherTrain As String = "ISSUING AUTHORITY ON EXISTING AUTHORITY THAT IS OTHER TRAIN"
    Public optInterjourneySendError1 As String = "TEXT MESSAGE TO TRAIN"
    Public optInterjourneySendError2 As String = "WASN'T SENT TO ONBOARD"
    Public optInterjourneySendError3 As String = "FAILURE ON TEXT MESSAGE DELIVERY"
    Public optInterjourneyMessage1 As String = "INTERJOURNEY"
    Public mstrLeadUnitCheck1 As String = "LEAD UNIT NEEDS ASSIGNED BEFORE"
    Public mstrLeadUnitCheck2 As String = "LICENSE CAN BE ISSUED TO TRAIN:"

#End Region

#Region "PTC OPTIONS"

    Public gPTCType As String = "PTC"
    'Public gPTCPrimaryServer As SystemServer
    'Public gPTCSecondaryServer As SystemServer
    'Public gPTCPrimaryServerConnection As CommunicationServerConnection
    'Public gPTCSecondaryServerConnection As CommunicationServerConnection
    'Public gPTCReceiveQueue As Queue = Queue.Synchronized(New Queue())
    'Public gPTCTransmitQueue As Queue = Queue.Synchronized(New Queue())
    'Public gPTCRcvQueueCheckThread As Threading.Thread
    'Public gPTCXmtQueueCheckThread As Threading.Thread
    'Public gPTCServerExists As Boolean

    Public gPTCTerritoryExists As Boolean
    Public gPTCCheckHeadNTailEndOnReportClear As Boolean = False
    Public gPTCBackOfficeServerList As New ArrayList

#End Region

#Region "TWC OverView "

    Public Enum ArrowHead
        EAST = 1
        WEST = 2
        MOW = 3
    End Enum

    Public WmoveLimit As New Point
    Public EmoveLimit As New Point
    Public dtArrow As New DataTable
    Public bkcolor As System.Drawing.Color
    Public FntColor As System.Drawing.Color
    Public MaintainWestTrainPnlPosition As New SortedList

    Public MaintainWestPnlMOWPosition As New SortedList

    Public WTrainCount As Integer = 0

    Public WMOWCount As Integer = 0

    Public MaintainEastPnlPosition As New SortedList

    Public ETrainCount As Integer = 0

    Public EMOWCount As Integer = 0
    Public dtSFAGrpBx As New DataTable

    Public MaintainWestPnlPosition As New SortedList
    Public SFAtrn As TrainID = Nothing

    Public Sub DrawArrow(ByVal sender As Object, ByVal ArrorwType As Integer, ByVal Color As System.Drawing.Color, ByVal Width As Integer, ByVal StartPostion As Integer, ByVal EndPostion As Integer)
        Dim gr As Graphics
        Dim myPen As Pen
        ' sender.image = Nothing
        Dim ObjWdth As Integer = (sender.width / 2) - 1
        gr = CreatePermanentGraphics(sender) ' Graphics.FromHwnd(hwnd:=sender.Handle)
        'myPen = New Pen(Color:=Color.Blue, Width:=6)
        myPen = New Pen(Color, Width)
        Select Case ArrorwType
            Case 1, 3
                myPen.StartCap = LineCap.ArrowAnchor
        End Select

        Select Case ArrorwType
            Case 2, 3
                myPen.EndCap = Drawing.Drawing2D.LineCap.ArrowAnchor
        End Select
        'Dim X1, Y1, X2, Y2 As Integer


        gr.DrawLine(pen:=myPen, x1:=ObjWdth, y1:=StartPostion, x2:=ObjWdth, y2:=EndPostion)
        ' gr.DrawLine(pen:=myPen, x1:=6, y1:=1, x2:=6, y2:=150)

    End Sub

    Function CreatePermanentGraphics(ByVal cajaImagen As PictureBox) As Graphics
        Dim BMPImage As New Bitmap(cajaImagen.Width, cajaImagen.Height)
        If cajaImagen.Image Is Nothing Then
            cajaImagen.Image = BMPImage
        Else
            Graphics.FromImage(BMPImage).DrawImage(cajaImagen.Image, 0, 0)
            cajaImagen.Image = BMPImage
        End If
        Return Graphics.FromImage(BMPImage)
    End Function

    Function CreatePermanentGraphics(ByVal cajaImagen As Label) As Graphics
        Dim BMPImage As New Bitmap(cajaImagen.Width, cajaImagen.Height)
        If cajaImagen.Image Is Nothing Then
            cajaImagen.Image = BMPImage
        Else
            Graphics.FromImage(BMPImage).DrawImage(cajaImagen.Image, 0, 0)
            cajaImagen.Image = BMPImage
        End If
        Return Graphics.FromImage(BMPImage)
    End Function

#End Region

#Region "AUTO-ROUTER"
    Public gSignalRouteList As ArrayList = ArrayList.Synchronized(New ArrayList()) ' As New ArrayList
    Public gMovementPlanningTerritoryExists As Boolean
    Public gGenerateRouteForTrainMode As Boolean
    Public gCurrentTrainSelectedForManualRoute As TrainID
    Public gManualRouteSelectionList As New ArrayList
    Public gManualRouteExecuteTime As Date
    Public gManualRouteDwellTime As Date
    Public gManualRouteList As New ArrayList
    Public gManualRouteDependencyList As New ArrayList
    Public gSelectedSignalRouteList As New ArrayList
#End Region

#Region "TRACK RESERVATION"

    Public gTrackReservations As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gTrackReservationsSummaryQueue As Queue = Queue.Synchronized(New Queue())
    Public gShowTrackReservationMessages As Boolean
    Public gCreateTrackreservationMode As Boolean

#End Region

#Region "SUB BULLETINS"
    Public gSubBulletinsMessageSummaryQueue As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "REALTIME FEEDBACK"

    Public gSystemFeedbackQueue As Queue = Queue.Synchronized(New Queue())
    Public gSystemFeedbackTime As Date
    Public gRTCDSA As String = ""
#End Region

#Region "SIMULATOR SCRIPT "

    Public Enum SimulatorScriptMode
        IDLE
        STARTUP
        AFTERSTARTUP
        EDITSTARTUP
        EDITAFTERSTARTUP
    End Enum

    Public Enum SimulatorScriptType
        SIMULATIONSYSTEMSTARTUP
        SIMULATIONSYSTEMSHUTDOWN
        SIMULATIONEVENTSBEFORESTARTUP
        SIMULATIONEVENTSAFTERSTARTUP
        SIMULATIONMASTERGROUP
    End Enum

    Public Enum SimulatorScriptAction
        ISSUED
        REMOVED
        ROLLUP
        MODIFIED
        VOID
        REPORTCLEAR
        SFAREPORTCLEAR
        SFAROLLUP
        VOIDAUTHORITY
    End Enum

    'Public gGenerateSimulatorScript As Boolean
    'Public gGenerateScriptOnStartUpMode As Boolean
    Public gEnableXMLRecording As Boolean
    Public gIsScriptLoading As Boolean

    Public gSimulatorScriptMode As Integer = 0
    Public gGenerateSimulatorLastEventTime As Integer = 0

    Public gGenerateSimulatorSystemStartCreated As Boolean
    Public gIsAfterStartupScript As Boolean

    Public gGenerateSimulatorCompCollection As New ArrayList
    Public gGenerateSimulatorCompSequence As New ArrayList
    Public gGenerateSimulatorStartupScriptCollection As New ArrayList

    Public dsStartupScript As New DataSet
    Public dsAfterStartupScript As New DataSet

    'Public dsSystemStart As New DataSet
    'Public dsSystemShutDown As DataSet

    'Public dsDispatchers As New DataSet
    'Public dsWrkStn As New DataSet
    'Public dsSysServer As New DataSet

    'Public dtSystemStart As New DataTable
    'Public dtSystemShutDown As New DataTable

    Public Delegate Sub RTCMSendControls(ByVal WaitTimeEnabled As Boolean)

#Region " Script Details "

    Public gStartupScriptCreator As String = ""
    Public gStartupScriptCreateDate As String = ""
    Public gStartupScriptVersion As String = ""
    Public gStartupScriptName As String = ""
    Public gStartupScriptComments As String = ""

    Public gAfterStartupScriptCreator As String = ""
    Public gAfterStartupScriptCreateDate As String = ""
    Public gAfterStartupScriptVersion As String = ""
    Public gAfterStartupScriptName As String = ""
    Public gAfterStartupScriptComments As String = ""

#End Region

#Region " Datatable Def "

    'Public Sub DefineSystemStartDT()

    '    dtSystemStart = New DataTable("SYSTEMSTART")
    '    Dim ID As DataColumn = New DataColumn("Sno")
    '    ID.DataType = System.Type.GetType("System.Int32")
    '    'System.Int64()
    '    ID.AutoIncrement = True
    '    ID.AutoIncrementSeed = 1
    '    ID.AutoIncrementStep = 1
    '    ID.Unique = True
    '    dtSystemStart.Columns.Add(ID)

    '    'Login
    '    Dim colLoginGUID As DataColumn = New DataColumn("GUID")
    '    colLoginGUID.DataType = System.Type.GetType("System.String")
    '    colLoginGUID.DefaultValue = ""
    '    dtSystemStart.Columns.Add(colLoginGUID)

    '    Dim colLoginName As DataColumn = New DataColumn("DispName")
    '    colLoginName.DataType = System.Type.GetType("System.String")
    '    colLoginName.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginName)

    '    Dim colLoginFName As DataColumn = New DataColumn("FirstName")
    '    colLoginFName.DataType = System.Type.GetType("System.String")
    '    colLoginFName.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginFName)

    '    Dim colLoginLName As DataColumn = New DataColumn("LastName")
    '    colLoginLName.DataType = System.Type.GetType("System.String")
    '    colLoginLName.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginLName)

    '    Dim colLoginInitials As DataColumn = New DataColumn("Initials")
    '    colLoginInitials.DataType = System.Type.GetType("System.String")
    '    colLoginInitials.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginInitials)

    '    Dim colLoginPWD As DataColumn = New DataColumn("PWD")
    '    colLoginPWD.DataType = System.Type.GetType("System.String")
    '    colLoginPWD.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginPWD)

    '    Dim colLoginPIN As DataColumn = New DataColumn("PIN")
    '    colLoginPIN.DataType = System.Type.GetType("System.String")
    '    colLoginPIN.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginPIN)

    '    Dim colLoginTerr As DataColumn = New DataColumn("TerrList")
    '    colLoginTerr.DataType = System.Type.GetType("System.String")
    '    colLoginTerr.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colLoginTerr)

    '    'WrkStn
    '    Dim colWrkStnGUID As DataColumn = New DataColumn("WrkStnGUID")
    '    colWrkStnGUID.DataType = System.Type.GetType("System.String")
    '    colWrkStnGUID.DefaultValue = ""
    '    dtSystemStart.Columns.Add(colWrkStnGUID)

    '    Dim colWrkStnName As DataColumn = New DataColumn("WrkStnName")
    '    colWrkStnName.DataType = System.Type.GetType("System.String")
    '    colWrkStnName.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colWrkStnName)

    '    Dim colWrkStnIP As DataColumn = New DataColumn("IPAddress")
    '    colWrkStnIP.DataType = System.Type.GetType("System.String")
    '    colWrkStnIP.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colWrkStnIP)

    '    Dim colWrkStnNo As DataColumn = New DataColumn("WrkStnNo")
    '    colWrkStnNo.DataType = System.Type.GetType("System.String")
    '    colWrkStnNo.DefaultValue = String.Empty
    '    dtSystemStart.Columns.Add(colWrkStnNo)




    'End Sub

    'Public Sub DefineShutDownDT()

    '    dtSystemShutDown = New DataTable("SYSTEMSHUTDOWN")
    '    Dim ID As DataColumn = New DataColumn("Sno")
    '    ID.DataType = System.Type.GetType("System.Int32")
    '    'System.Int64()
    '    ID.AutoIncrement = True
    '    ID.AutoIncrementSeed = 1
    '    ID.AutoIncrementStep = 1
    '    ID.Unique = True
    '    dtSystemShutDown.Columns.Add(ID)


    '    'Login
    '    Dim colLoginGUID As DataColumn = New DataColumn("GUID")
    '    colLoginGUID.DataType = System.Type.GetType("System.String")
    '    colLoginGUID.DefaultValue = ""
    '    dtSystemShutDown.Columns.Add(colLoginGUID)

    '    Dim colTrain As DataColumn = New DataColumn("TRAIN")
    '    colTrain.DataType = System.Type.GetType("System.String")
    '    colTrain.DefaultValue = String.Empty
    '    dtSystemShutDown.Columns.Add(colTrain)

    '    Dim colWrkStn As DataColumn = New DataColumn("WrkStn")
    '    colWrkStn.DataType = System.Type.GetType("System.String")
    '    colWrkStn.DefaultValue = String.Empty
    '    dtSystemShutDown.Columns.Add(colWrkStn)

    'End Sub

#End Region

#End Region

#Region "SYSTEM TIME TO LIVE TASKS"
    Public gSystemTimeToLiveTasksList As ArrayList = ArrayList.Synchronized(New ArrayList())
#End Region

#Region "WEATHER MESSAGES"

    Public gWeatherMessageList As ArrayList = ArrayList.Synchronized(New ArrayList)

#End Region

#Region "FED MESSAGES"

    Public gFEDMessageList As ArrayList = ArrayList.Synchronized(New ArrayList)

#End Region

#Region "GLOBAL HOTKEY FUNCTIONS "

    ' Windows API functions and constants
    Public Declare Function RegisterHotKey Lib "user32" (ByVal hwnd As IntPtr, ByVal id As Integer,
       ByVal fsModifiers As Integer, ByVal vk As Integer) As Integer
    Public Declare Function UnregisterHotKey Lib "user32" (ByVal hwnd As IntPtr, ByVal id As Integer) _
       As Integer
    Public Declare Function GlobalAddAtom Lib "kernel32" Alias "GlobalAddAtomA" (ByVal lpString As _
       String) As Short
    Public Declare Function GlobalDeleteAtom Lib "kernel32" (ByVal nAtom As Short) As Short

    Public Const MOD_ALT As Integer = 1
    Public Const MOD_CONTROL As Integer = 2
    Public Const MOD_SHIFT As Integer = 4
    Public Const MOD_WIN As Integer = 8
    Public HotkeyID As Short

#End Region

#Region "PTC VARIABLES"
    Public Enum PTCAuthorityACKPendingStatus
        ROLLUP
        RELEASE
        VOID
        ISSUE
        PENDINGRELEASE
        MODIFY
    End Enum

    Public gTestPTC As Boolean

#End Region

#Region "MILEAGE RANGES & BULLETIN CELLS"

    Public gMileageRangeList As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gBulletinCellRouteList As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gBulletinZoneList As ArrayList = ArrayList.Synchronized(New ArrayList)

#End Region

#Region "NETWORK PORT LOGGING"
    Public gNetworkPortLoggingClient As NetworkEventLogging
    Public gNetworkPortLoggingServer As SystemServer
    Public gNetworkPortLogAllEvents As Boolean

    Public Sub GetNetworkPortLoggingServerInformation()
        Dim dr As OleDbDataReader
        Dim dbConn As New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
        Dim Query As String = "SELECT ServerName, ClientSettings FROM " & gtblServers & " WHERE ServerType = 'NETWORKLOGGINGSERVER-WRKSTN'"
        Dim dbComm As OleDbCommand
        Try
            gNetworkPortLoggingServer = New SystemServer

            dbComm = New OleDbCommand(Query, dbConn)
            dbConn.Open()
            dr = dbComm.ExecuteReader()
            While dr.Read
                If Not dr.IsDBNull(0) And Not dr.IsDBNull(1) Then
                    gNetworkPortLoggingServer.Name = dr.GetString(0)
                    Dim connection() As String = dr.GetString(1).Split(";")
                    If connection.Length >= 3 Then
                        gNetworkPortLoggingServer.IPstring = connection(1)
                        gNetworkPortLoggingServer.PortNumber = (CInt(connection(2)))
                    End If
                End If
            End While
        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
    End Sub
#End Region

#Region "TABULAR GENERAL BULLETIN ORDERS"
    Public gTabularGeneralBulletins As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gTabularGeneralBulletinExceptions As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gBulletinsTemplates As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gBulletinsTemplateDefaults As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gTabularGeneralBulletinSuspensions As ArrayList = ArrayList.Synchronized(New ArrayList)

#End Region

#Region "TRACK AUTHORITY ENUM "

    Public Enum TNTAuthStatus
        PENDING
        UNACKNOWLEDGED
        ACKNOWLEDGED
        VOID
    End Enum

#End Region

#Region "TEMPORAL MODE"

    Public gTemporalControlPointList As ArrayList
    Public gTemporalModeControlled As Boolean
    Public gTemporalModeFreight As Boolean
    Public gTemporalModePassenger As Boolean
    Public gTemporalModeInitialized As Boolean
    Public gTemporalOperationsStartTime As Date

    Public gTemporalModePassengerOperationsAlarmStartTime As Date
    Public gTemporalAlarmAcknowledged As Boolean
    Public gTemporalAlertGenerated As Boolean
    Public gTemporalAlarmOverride As Boolean

    Public gTemporalPointOfConflictList As ArrayList


#End Region

#Region "HIGHWAY CROSSING INFO"
    Public gHighWayCrossingList As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public optLoadHighwayCrossingInfo As Boolean
#End Region

#Region "SPEED RESTRICTION REASON CODES"

    Public gSpeedRestrictionCodeList As New ArrayList
    Public optLoadSpeedRestrictionCode As Boolean

#End Region

#Region "WORK TASKS"

    Public gWorkTasks As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gWorkTasksSummaryQueue As Queue = Queue.Synchronized(New Queue())

#End Region

#Region "TRAIN SHEET MESSAGE QUEUE"
    Public gTrainSheetMessageQueue As Queue = Queue.Synchronized(New Queue)

#End Region

#Region " COMPARE CONTROL ORDER "

    ''' <summary>
    ''' This Class is needed to perform a Sort on items in an ArrayList.
    ''' When using a Generic List, this functionality is built into the Sort function.
    ''' </summary>
    ''' <remarks>WGROH - Nov 7, 2012 - Added for BRC Tab order request.</remarks>
    Public Class CompareControlIndexes
        Implements IComparer

        ' Default method for IComparer, called implicitly when the class is referenced in a Sort function.
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
            Dim c1 As Control = DirectCast(x, Control)
            Dim c2 As Control = DirectCast(y, Control)
            Return c1.TabIndex.CompareTo(c2.TabIndex)
        End Function

    End Class

#End Region

#Region " COMBOBOX ITEMS "
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ComboboxItems
        Private pDisplay As String = String.Empty
        Private pValue As String = String.Empty
        Private m_foreColor As Color = Color.Black
        Public Sub New(ByVal Text As String, ByVal AssignedValue As String)
            DisplayText = Text
            Value = AssignedValue
        End Sub
        Public Overrides Function ToString() As String
            ' Generates the text shown in the combo box
            Return DisplayText
        End Function

        Public Property DisplayText() As String
            Get
                Return pDisplay
            End Get
            Set(ByVal value As String)
                pDisplay = value
            End Set
        End Property
        Public Property Value() As String
            Get
                Return pValue
            End Get
            Set(ByVal value As String)
                pValue = value
            End Set
        End Property

        Public Property ForeColor() As Color
            Get
                Return m_foreColor
            End Get
            Set(value As Color)
                m_foreColor = value
            End Set
        End Property

    End Class


#End Region

#Region "RAILROAD SPECIFIC TEST VARIABLES"
    Public optUseNewSRControlServerLogic As Boolean = False
#End Region

#Region "APPLICATION EXCEPTION TRACKING"

    Public gApplicationExceptionList As ArrayList = ArrayList.Synchronized(New ArrayList)
    Public gApplicationExceptionThreshold As Integer = 20
#End Region

#Region " PUBLIC SUB CLASSES"

    Public Class GroupedSignalRoutes
        Public myGUID As String = Guid.NewGuid.ToString.ToUpper
        Public myEntrySignal As Signal = Nothing
        Public myExitSignal As Signal = Nothing
        Public myRouteCollection As New ArrayList
        Public myCompCollection As New ArrayList
        Public myPriority As Integer = 1
        Public myTerritory As Integer = 0
        Public myRouteCollectionString As String = String.Empty
        Public myControlPointName As String = String.Empty
        Public myControlPointNumber As Integer = 0
        Public myEntryTrack As Track = Nothing
        Public myExittrack As Track = Nothing
        Public myStartDirection As String = String.Empty
        Public myEndDirection As String = String.Empty
        Public myRouteOrder As Integer = 1
        Public tmpProcessed As Boolean = False


        Public Sub New()

        End Sub
    End Class


#End Region

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
            '   SystemException(ex.ToString)
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
            '  SystemException(ex.ToString)
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
            '  SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function
End Module
