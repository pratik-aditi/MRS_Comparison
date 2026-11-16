Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Data.OleDb
Imports System.Reflection
Imports System.Drawing
Imports System.Timers
Imports System.Text
Imports System.Net
Imports System.IO
Imports TMDS.SystemForms
Imports TMDS.TrackAuthority
Imports TMDS.NetworkClasses
Imports TMDS.CodeServerClient
Imports TMDS.ManagementClasses
Imports TMDS.GlobalDeclarations
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.ManagementClasses.SystemEventRecord
Imports TMDS.SystemUtilities.UtilitiesDisplayForm
Imports TMDS.TrainComponents
Imports TMDS.TrackComponents
Imports TMDS.SwitchComponents
Imports TMDS.SignalComponents
Imports TMDS.TrackTagComponents
Imports TMDS.TrackOtherComponents
Imports System.Reflection.MethodBase

Namespace GraphicComponent

#Region "COMPONENT"

    Public Class GenericComponent
        Inherits System.Windows.Forms.UserControl
        Implements IDisposable

#Region " Windows Form Designer generated code "


        Public Sub New()
            MyBase.New()

            'This call is required by the Windows Form Designer.
            InitializeComponent()

            'Add any initialization after the InitializeComponent() call
            Me.threadSafePropertySet = New ThreadSafePropertySetter(Me)
        End Sub


        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                Try

                    Me.pCurrentColorStatus = Nothing
                    Me.GraphicalPath = Nothing
                    Me.Graphical = Nothing
                    Me.myGraphic = Nothing
                    Me.pGraphic = Nothing
                    Me.pGraphicLocal = Nothing
                    Me.pGraphicPath = Nothing
                    Me.pGraphicPathRelative = Nothing
                    Me.GraphicalPathRelative = Nothing

                Catch
                End Try

            End If
            MyBase.Dispose(disposing)
        End Sub

        ''' <summary>
        ''' Overriding the Equals function to allow for comparisons. (RPBJR)
        ''' </summary>
        ''' <param name="object_to_compare"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Overloads Function Equals(ByVal object_to_compare As Object) As Boolean

            Dim isEqual As Boolean = False

            Try

                'Verify that the passed object is not empty and it is the same type
                If object_to_compare Is Nothing Or Not Me.GetType.Equals(object_to_compare.GetType) Then
                    Exit Function
                End If

                'Check UIDs
                If Me.GUID = object_to_compare.GUID Then
                    isEqual = True
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
                isEqual = False
            End Try

            Return isEqual

        End Function

        ''' <summary>
        ''' Overloads the = operator (RPBJR)
        ''' </summary>
        ''' <param name="first_component"></param>
        ''' <param name="second_component"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator =(ByVal first_component As GenericComponent, ByVal second_component As GenericComponent) As Boolean

            Return Object.Equals(first_component, second_component)

        End Operator

        ''' <summary>
        ''' Overloads the NOT EQUALS operator (RPBJR)
        ''' </summary>
        ''' <param name="first_component"></param>
        ''' <param name="second_component"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator <>(ByVal first_component As GenericComponent, ByVal second_component As GenericComponent) As Boolean

            Return Not (first_component = second_component)

        End Operator

        'Public Overloads Sub Dispose()

        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(True)
        '    GC.SuppressFinalize(Me)
        'End Sub

        ''UserControl overrides dispose to clean up the component list.
        'Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        '    If disposing Then
        '        If Not (components Is Nothing) Then
        '            components.Dispose()
        '        End If
        '    End If
        '    MyBase.Dispose(disposing)
        'End Sub


        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            '
            'GenericComponent
            '
            Me.BackColor = System.Drawing.Color.Black
            Me.Name = "GenericComponent"
            Me.Size = New System.Drawing.Size(4, 4)
            Me.SetStyle(ControlStyles.UserPaint, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        End Sub

#End Region

#Region "ENUMS"

        Enum ComponentMode
            Regular
            Selected
        End Enum

        Public Enum ModeSelection
            Regular
            DeviceBlocking
            RouteRequesting
            SummarySelectionBlocking
            SummarySelectionRouting
            TagBlocking
            HighLight
            OccupancySelectionBlocking
            PTCIssuancePending
            PTCIssuanceImminent
            PTCReleasePending
            PTCReleaseImminent
        End Enum

        Public ColorStatusPTCIssuancePending As New SolidBrush(optColorStatusPTCIssuancePending)
        Public ColorStatusPTCIssuanceImminent As New SolidBrush(optColorStatusPTCIssuanceImminent)
        Public ColorStatusPTCReleasePending As New SolidBrush(optColorStatusPTCReleasePending)
        Public ColorStatusPTCReleaseImminent As New SolidBrush(optColorStatusPTCReleaseImminent)

#End Region

#Region "FIELDS"
        Public threadSafePropertySet As ThreadSafePropertySetter
        Public myTerritory As Territory
        Public mySubdivision As Subdivision
        Public SelectionMode As ModeSelection
        Private pGUID As Int32
        Private pName As String
        Private pStatNum As Int32
        Private pTerritoryID As Integer
        Private pSubdivision As String
        Private pSubdivisionID As Integer
        Private pAuthorityType As String
        Private pCompType As String
        Private pLXpos As Int16
        Private pLYpos As Int16
        Private pRXpos As Int16
        Private pRYpos As Int16
        Private pLeftLnk As Int32
        Private pRightLnk As Int32
        Private pLeftAltLnk As Int32
        Private pRightAltLnk As Int32
        Private pAltLnkOne As Int32
        Private pAltLnkTwo As Int32
        Private pAltLnkThree As Int32
        Private pAltLnkFour As Int32

        Private pGraphic As Graphics
        Private pGraphicLocal As Graphics

        Private pGraphicPath As GraphicsPath
        Private pGraphicPathRelative As GraphicsPath
        Public DefaultPathRelative As GraphicsPath


        Public pDefaultPath As GraphicsPath
        Private pRegion As Region
        Private pSnippetRegion As Integer

        Private pEnabled As Boolean
        Private pControllable As Boolean
        Private pIndicated As Boolean
        Private pIndicatedAlt As Boolean
        Private pIndicatedAlt1 As Boolean
        Private pFailState As Boolean
        Private pLocked As Boolean
        Private pIndex As Integer
        Private pBlockRestrictive As Boolean
        Private pBlockNonRestrictive As Boolean
        Private pCurrentColorStatus As SolidBrush = New SolidBrush(Color.White)

        Public pCurrentDrawingForm As frmDisplay
        Private pGlobalFlasherCollectionReference As ArrayList
        'Public FlashComponent As Boolean
        Private pFlashComponent As Boolean
        Public FlashOn As Boolean
        Public Flasher As System.Windows.Forms.Timer

        'Simulation
        Public IndSimulator As Timers.Timer
        Public SimulateMode As Boolean
        Public DoSimulate As Boolean

        'Monitoring Mode
        Private pDoMonitorMode As Boolean

        'CTC stuff
        Public CodelineNumber As Integer
        Public CodeStation As Integer
        Public Ind(3) As BitPosition
        Public Ctl(6) As BitPosition
        Public Indicating As Boolean
        Public IndicateOnly As Boolean

        'Blocking Parameters
        Private pBlockAbsolute As Boolean
        Public BlockAbsolutePrevious As Boolean
        Public TrackAuthorities As New ArrayList()
        Public DeviceBlocks As New ArrayList()
        Public DeviceBlocked As Boolean

        Public Mode As ComponentMode
        Public Path As GraphicsPath

        Public ContextMenuControl As ContextMenu
        Public PreviousColorStatus As SolidBrush
        Public PendingStatusChange As Boolean

        'Graphical Grouping
        Public GraphicalGroupID As Integer
        Public ScreenRegionID As Integer
        Public ScreenRegionMapID As Integer
        Public MakeInvisible As Boolean


        'Control Point Stuff
        Public myControlPoint As ControlPoint

        'Reset Flasher
        Public LockReset As Boolean
        Public LockRemove As Boolean
        Public LockAdd As Boolean

        'Status Reporting
        Public StatusMessage As String

        'Override Code Brown
        Public pOverRideCodeBrown As Boolean = False

        Public ForceOperationsToTWC As Boolean

        'Snow Melter Timer
        Public tmrSnowMelter As System.Windows.Forms.Timer
        Public IsSnowMelterTmrRunning As Boolean = False
        Public CustomSnowMelterEndTime As Date = Now.AddMinutes(-1)

        Public OtherSplAuthorities As New ArrayList()
        Public SplAuthorityBlock As Boolean = False
        Public SpecialAuthorityBlocking As Boolean = False
        Public myToolTip As String

        Public mnuTroubleReport As MenuItem
        Public mnuTroubleReportTrack As MenuItem
        Public mnuTroubleReportMisc As MenuItem
        Public mnuTroubleReportPsgrStation As MenuItem
        Public mnuTroubleReportCommunications As MenuItem
        Public mnuTroubleReportWayside As MenuItem
        Public mnuTroubleReportOverhead As MenuItem
        Public mnuTroubleReportTractionPower As MenuItem

        Public myMilePost As String
        Public myMilePostSuffix As String
        Public myMilePostPrefix As String

        Public IsControlled As Boolean
        Public IsShadowMode As Boolean
        Public IsPrimaryController As Boolean

        Public InUse As Boolean

        'PTC
        Public PTC_SiteName As String
        Public PTC_SiteAddress As String
        Public PTC_SecurityKey As String
        Public PTC_WIUID As String
        Public PTC_RailroadSCAC As String
        Public PTC_UID_KEY As String

        'Weather Station 
        Public mnuWeatherReport As MenuItem
        'Public tmrWeatherAlert As System.Windows.Forms.Timer

        'SCADA TIMER
        Public tmrSCADASupperssion As System.Timers.Timer

#End Region

        Dim instance As CreateParams
        Dim value As Integer

#Region "PROPERTIES"

        Public Property DoMonitorMode() As Boolean
            Get
                Return pDoMonitorMode
            End Get
            Set(ByVal Value As Boolean)
                pDoMonitorMode = Value
            End Set

        End Property


        Public Property FlashComponent() As Boolean
            Get
                Return pFlashComponent
            End Get
            Set(ByVal Value As Boolean)
                pFlashComponent = Value
            End Set

        End Property

        Public Property OverRideCodeBrown() As Boolean
            Get
                Return pOverRideCodeBrown
            End Get
            Set(ByVal Value As Boolean)
                pOverRideCodeBrown = Value
            End Set

        End Property

        Public Property BlockAbsolute() As Boolean
            Get
                Return pBlockAbsolute
            End Get
            Set(ByVal Value As Boolean)
                pBlockAbsolute = Value
            End Set

        End Property


        Public Property CurrentColorStatus() As SolidBrush
            Get
                Return pCurrentColorStatus
            End Get
            Set(ByVal Value As SolidBrush)
                pCurrentColorStatus = Value
            End Set
        End Property

        Public Property CurrentDrawingForm() As frmDisplay
            Get
                Return pCurrentDrawingForm
            End Get
            Set(ByVal Value As frmDisplay)
                pCurrentDrawingForm = Value
            End Set
        End Property

        Public Property GlobalFlasherCollectionReference() As ArrayList
            Get
                Return pGlobalFlasherCollectionReference
            End Get
            'Set(ByVal Value As ArrayList)
            '    pGlobalFlasherCollectionReference = Value
            'End Set
            Set(ByVal value As ArrayList)

            End Set
        End Property

        Public Property Index() As Integer
            Get
                Return pIndex
            End Get
            Set(ByVal Value As Integer)
                pIndex = Value
            End Set
        End Property

        Public Property GUID() As Int32
            Get
                Return pGUID
            End Get
            Set(ByVal Value As Int32)
                pGUID = Value
            End Set
        End Property
        Public Shadows Property Name() As String
            Get
                Return pName
            End Get
            Set(ByVal Value As String)
                pName = Value
            End Set
        End Property
        Public Property StationNumber() As Int32
            Get
                Return pStatNum
            End Get
            Set(ByVal Value As Int32)
                pStatNum = Value
            End Set
        End Property
        Public Property Subdivision() As String
            Get
                Return pSubdivision
            End Get
            Set(ByVal Value As String)
                pSubdivision = Value
            End Set
        End Property
        Public Property TerritoryID() As Integer
            Get
                Return pTerritoryID
            End Get
            Set(ByVal Value As Integer)
                pTerritoryID = Value
            End Set
        End Property
        Public Property SubdivisionID() As Integer
            Get
                Return pSubdivisionID
            End Get
            Set(ByVal Value As Integer)
                pSubdivisionID = Value
            End Set
        End Property

        Public Property ComponentType() As String
            Get
                Return pCompType
            End Get
            Set(ByVal Value As String)
                pCompType = Value
            End Set
        End Property
        Public Property AuthorityType() As String
            Get
                Return pAuthorityType
            End Get
            Set(ByVal Value As String)
                pAuthorityType = Value
            End Set
        End Property
        Public Property LX() As Int16
            Get
                Return pLXpos
            End Get
            Set(ByVal Value As Int16)
                pLXpos = Value
            End Set
        End Property
        Public Property LY() As Int16
            Get
                Return pLYpos
            End Get
            Set(ByVal Value As Int16)
                pLYpos = Value
            End Set
        End Property
        Public Property RX() As Int16
            Get
                Return pRXpos
            End Get
            Set(ByVal Value As Int16)
                pRXpos = Value
            End Set
        End Property
        Public Property RY() As Int16
            Get
                Return pRYpos
            End Get
            Set(ByVal Value As Int16)
                pRYpos = Value
            End Set
        End Property
        Public Property Graphical() As Graphics
            Get
                Return pGraphic
            End Get
            Set(ByVal Value As Graphics)
                pGraphic = Value
            End Set
        End Property
        Public Property myGraphic() As Graphics
            Get
                Return pGraphicLocal
            End Get
            Set(ByVal Value As Graphics)
                pGraphicLocal = Value
            End Set
        End Property
        Public Property GraphicalPath() As GraphicsPath
            Get
                Return pGraphicPath
            End Get
            Set(ByVal Value As GraphicsPath)
                pGraphicPath = Value
            End Set

        End Property
        Public Property GraphicalPathRelative() As GraphicsPath
            Get
                Return pGraphicPathRelative
            End Get
            Set(ByVal Value As GraphicsPath)
                pGraphicPathRelative = Value
            End Set

        End Property

        Public Property RegionPath() As Region
            Get
                Return pRegion
            End Get
            Set(ByVal Value As Region)
                pRegion = Value
            End Set

        End Property
        Public Property SnippetRegion() As Integer
            Get
                Return pSnippetRegion
            End Get
            Set(ByVal Value As Integer)
                pSnippetRegion = Value
            End Set

        End Property

        Public Property LeftLink() As Int32
            Get
                Return pLeftLnk
            End Get
            Set(ByVal Value As Int32)
                pLeftLnk = Value
            End Set
        End Property
        Public Property RightLink() As Int32
            Get
                Return pRightLnk
            End Get
            Set(ByVal Value As Int32)
                pRightLnk = Value
            End Set
        End Property
        Public Property LeftAltLink() As Int32
            Get
                Return pLeftAltLnk
            End Get
            Set(ByVal Value As Int32)
                pLeftAltLnk = Value
            End Set
        End Property
        Public Property RightAltLink() As Int32
            Get
                Return pRightAltLnk
            End Get
            Set(ByVal Value As Int32)
                pRightAltLnk = Value
            End Set
        End Property
        Public Property AltLinkOne() As Int32
            Get
                Return pAltLnkOne
            End Get
            Set(ByVal Value As Int32)
                pAltLnkOne = Value
            End Set
        End Property
        Public Property AltLinkTwo() As Int32
            Get
                Return pAltLnkTwo
            End Get
            Set(ByVal Value As Int32)
                pAltLnkTwo = Value
            End Set
        End Property
        Public Property Indicated() As Boolean
            Get
                Return pIndicated
            End Get
            Set(ByVal Value As Boolean)
                pIndicated = Value
            End Set

        End Property
        Public Property BlockRestrictive() As Boolean
            Get
                Return pBlockRestrictive
            End Get
            Set(ByVal Value As Boolean)
                pBlockRestrictive = Value
            End Set
        End Property
        Public Property NonBlockRestrictive() As Boolean
            Get
                Return pBlockNonRestrictive
            End Get
            Set(ByVal Value As Boolean)
                pBlockNonRestrictive = Value
            End Set

        End Property
        Public Property FailState() As Boolean
            Get
                Return pFailState
            End Get
            Set(ByVal Value As Boolean)
                pFailState = Value
            End Set

        End Property

        Public Property AltLinkThree() As Int32
            Get
                Return pAltLnkThree
            End Get
            Set(ByVal Value As Int32)
                pAltLnkThree = Value
            End Set
        End Property

        Public Property AltLinkFour() As Int32
            Get
                Return pAltLnkFour
            End Get
            Set(ByVal Value As Int32)
                pAltLnkFour = Value
            End Set
        End Property

        Private pClickedSendLicense As Boolean
        Public Property ClickedSendLicense() As Boolean
            Get
                Return pClickedSendLicense
            End Get
            Set(ByVal value As Boolean)
                pClickedSendLicense = value
            End Set
        End Property

#End Region

#Region "SUBS"
        Public Function GetCompType() As String
            Try
                Dim d() As String = [GetType].ToString.Split(".")
                If d.Count = 3 Then
                    Return d(2)
                End If
                Return Me.GetType.ToString

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Me.GetType.ToString
            End Try
        End Function

        Public Sub AddTroubleReportMenuItem(Optional ByVal ShowMiscMenu As Boolean = False)
            Try
                If ContextMenuControl IsNot Nothing AndAlso ContextMenuControl.MenuItems.Count > 0 Then
                    Dim mnuSplit3 As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit3)
                End If

                'mnuTroubleReport = New MenuItem
                'mnuTroubleReport = New MenuItem("CREATE INCIDENT REPORT")
                ''AddHandler mnuTroubleReport.Click, AddressOf Me.mnuTroubleReport_Click
                'ContextMenuControl.MenuItems.Add(mnuTroubleReport)
                'mnuTroubleReportTrack = New MenuItem("TRACK")
                'AddHandler mnuTroubleReportTrack.Click, AddressOf Me.mnuTroubleReport_Click
                'mnuTroubleReport.MenuItems.Add(mnuTroubleReportTrack)

                'mnuTroubleReportTrack = New MenuItem("TRACK")
                'AddHandler mnuTroubleReportTrack.Click, AddressOf Me.mnuTroubleReport_Click
                'mnuTroubleReport.MenuItems.Add(mnuTroubleReportTrack)
                If ShowMiscMenu Then
                    mnuTroubleReport = New MenuItem
                    mnuTroubleReport = New MenuItem("CREATE INCIDENT REPORT")
                    ContextMenuControl.MenuItems.Add(mnuTroubleReport)

                    mnuTroubleReportTrack = New MenuItem("TRACK")
                    AddHandler mnuTroubleReportTrack.Click, AddressOf Me.mnuTroubleReport_Click
                    mnuTroubleReport.MenuItems.Add(mnuTroubleReportTrack)

                    'Bob Varghese DTP-190 Misc was removed for Denver.
                    If optEnableSpecificFunctionalityForDenver AndAlso Me.GetType.ToString = gTrackComponentName Then
                    Else
                        mnuTroubleReportMisc = New MenuItem("MISC")
                        AddHandler mnuTroubleReportMisc.Click, AddressOf Me.mnuTroubleReportMisc_Click
                        mnuTroubleReport.MenuItems.Add(mnuTroubleReportMisc)
                    End If



                Else
                    mnuTroubleReport = New MenuItem
                    mnuTroubleReport = New MenuItem("CREATE INCIDENT REPORT")
                    AddHandler mnuTroubleReport.Click, AddressOf Me.mnuTroubleReport_Click
                    ContextMenuControl.MenuItems.Add(mnuTroubleReport)

                    If optEnableSpecificFunctionalityForDenver Then 'CMV DTP-86
                        Select Case Me.GetType.ToString
                            Case gSignalComponentName, gSwitchComponentName
                            Case Else
                                mnuTroubleReportCommunications = New MenuItem("COMMUNICATIONS")
                                mnuTroubleReportCommunications.Tag = "COMMUNICATIONS"
                                AddHandler mnuTroubleReportCommunications.Click, AddressOf Me.mnuTroubleReport_Click
                                mnuTroubleReport.MenuItems.Add(mnuTroubleReportCommunications)

                                mnuTroubleReportOverhead = New MenuItem("OVERHEAD (OCS)")
                                mnuTroubleReportOverhead.Tag = "OVERHEAD (OCS)"
                                AddHandler mnuTroubleReportOverhead.Click, AddressOf Me.mnuTroubleReport_Click
                                mnuTroubleReport.MenuItems.Add(mnuTroubleReportOverhead)

                                mnuTroubleReportTractionPower = New MenuItem("TRACTION POWER (TPSS)")
                                mnuTroubleReportTractionPower.Tag = "TRACTION POWER (TPSS)"
                                AddHandler mnuTroubleReportTractionPower.Click, AddressOf Me.mnuTroubleReport_Click
                                mnuTroubleReport.MenuItems.Add(mnuTroubleReportTractionPower) 'CMV DTP-86
                        End Select
                        mnuTroubleReportWayside = New MenuItem("WAYSIDE EQUIPMENT")
                        mnuTroubleReportWayside.Tag = "WAYSIDE EQUIPMENT"
                        AddHandler mnuTroubleReportWayside.Click, AddressOf Me.mnuTroubleReport_Click
                        mnuTroubleReport.MenuItems.Add(mnuTroubleReportWayside)
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Overridable Sub mnuTroubleReport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me)
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
        Public Overridable Sub mnuTroubleReportMisc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me, "MISC")
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overridable Sub mnuTroubleReportPSGR_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim ReportType As String = ""
            Try
                If sender.Tag IsNot Nothing AndAlso sender.Tag.ToString.Trim <> "" Then
                    ReportType = sender.Tag.ToString.Trim
                End If
                If ReportType.Trim = "" Then Exit Sub
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me, ReportType)
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overridable Sub SendStatusUpdate()

        End Sub

        Public Overridable Sub SendTrainerCommand()

        End Sub

        Public Overridable Sub SendTraineeStatusUpdate()

        End Sub

        Public Overridable Sub FlasherMode(ByVal EnableFlasher As Boolean, Optional ByVal LogEvent As Boolean = False)
            Try

                Me.FlashComponent = EnableFlasher
                Me.FlashOn = EnableFlasher
                Me.Flasher.Enabled = EnableFlasher
                Me.Flasher.Interval = 250
                If EnableFlasher Then
                    Me.AddToFlashCollection(Me)
                    Me.ReSetFlasherCollection()
                Else
                    If FlashCollection.Contains(Me) Then
                        Me.RemoveFromFlashCollection(Me)
                    End If
                    Do Until Not Me.Flasher.Enabled
                        Me.Flasher.Enabled = False
                        Exit Do
                    Loop
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)

        End Sub

        Overridable Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)

        End Sub

        Overridable Sub InitializeGraphicsFromScreenCoordinates()


        End Sub

        Overridable Sub RefreshPath()
            Try
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub AddToFlashCollection(ByVal obj As Object)
            Try

                'SyncLock FlashCollection.SyncRoot
                If Not FlashCollection.Contains(obj) Then
                    If obj.GetType.ToString <> "TMDS.GraphicComponent.ControlPoint" Then
                        FlashCollection.Add(obj)
                    End If
                End If
                'End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Sub RemoveFromFlashCollection(ByVal obj As Object)
            Try
                'SyncLock FlashCollection.SyncRoot
                FlashCollection.Remove(obj)
                obj.FlashComponent = False
                'End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub FindCodeStationToSend(Optional theCP As ControlPoint = Nothing)
            Try
                Dim I As Integer
                Dim thisControlPoint As ControlPoint = Me.myControlPoint
                If Not IsNothing(theCP) Then
                    thisControlPoint = theCP
                End If
                For I = 0 To 6
                    If Not IsNothing(Me.Ctl(I)) Then
                        If Not thisControlPoint.CodeStationsToSend.Contains(Me.Ctl(I).CodeStation) Then
                            thisControlPoint.CodeStationsToSend.Add(Me.Ctl(I).CodeStation)
                            SystemEvent("FindCodeStationToSend:CodeStationsToSend:Number:" & Me.Name & ":" & Me.Ctl(I).CodeStation)
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub AddToSendCollection(CallingMethod As String, Optional ByVal ShowControlPointSelected As Boolean = True, Optional theBit As BitPosition = Nothing)
            Try

                SystemEvent("AddToSendCollection:Start:CallingMethod:" & CallingMethod)

                Dim cp As ControlPoint
                Dim cpToCompare As ControlPoint = Nothing
                Dim CPGUID As Int32 = Me.StationNumber
                Dim theOtherControlPoint As ControlPoint = Nothing
                If Not IsNothing(theBit) Then
                    If theBit.ControlPoint <> CPGUID Then
                        CPGUID = theBit.ControlPoint
                        theOtherControlPoint = TMDS.SystemUtilities.FetchData.FindControlPoint(CPGUID)
                    End If
                End If

                FindCodeStationToSend(theOtherControlPoint)

                For Each cp In gControlPointCollection
                    If cp.StationNumber = CPGUID Then
                        cpToCompare = cp
                        Exit For
                    End If
                Next
                If IsNothing(cpToCompare) Then Exit Sub

                If Not gSendControlCollection.Contains(cpToCompare) Then
                    SystemEvent("AddToSendCollection:AddSendControlCollection:" & cpToCompare.Name)
                    gSendControlCollection.Add(cpToCompare)
                End If

                'Dim AddTheStation As Boolean = True
                'SyncLock gSendControlCollection
                '    For Each cp In gSendControlCollection
                '        If cp.StationNumber = cpToCompare.StationNumber Then
                '            AddTheStation = False
                '        End If
                '    Next
                '    If AddTheStation AndAlso Not gSendControlCollection.Contains(cpToCompare) Then
                '        gSendControlCollection.Add(cpToCompare)
                '    End If
                'End SyncLock

                If ShowControlPointSelected Then
                    cpToCompare.FlasherMode(True)
                End If

                SystemEvent("AddToSendCollection:End:CallingMethod:" & CallingMethod)


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ReSetFlasherCollection()
            Try
                Dim comp As Object
                'SyncLock FlashCollection
                Dim myFlashCollection As ArrayList = ArrayList.Synchronized(FlashCollection)
                For Each comp In myFlashCollection
                    If Not comp Is Nothing Then
                        If Not comp.flasher Is Nothing Then
                            comp.flashon = False
                        Else
                            comp.flashon = False
                        End If
                    End If
                Next
                'End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub TempCodeStationBitBuild(ByVal BitMappingString As String, ByRef CtlIndRef As BitPosition, ByVal BitCollection As ArrayList, ByRef GUIDCounter As Int32, ByRef TypeOfBit As CodeServerClient.BitPosition.BitType)
            Try
                Dim BitToProcess As BitPosition
                If BitMappingString = "" Or BitMappingString = "NONE" Or BitMappingString = "*****" Then Exit Sub
                Dim TempString() As String = BitMappingString.Split(";")
                BitToProcess = New BitPosition()
                BitToProcess.TypeOfBit = TypeOfBit

                If TempString.Length >= 1 Then
                    If IsNumeric(TempString(0)) Then
                        BitToProcess.PositionOfBit = CInt(TempString(0))
                        If BitToProcess.PositionOfBit = 0 Then BitToProcess = Nothing : Exit Sub
                    End If
                End If

                If TempString.Length >= 2 Then
                    If IsNumeric(TempString(1)) Then
                        BitToProcess.CodeStation = CInt(TempString(1))
                        If BitToProcess.CodeStation = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.CodeStation = Me.CodeStation
                    End If
                Else
                    BitToProcess.CodeStation = Me.CodeStation
                End If

                If TempString.Length >= 3 Then
                    If IsNumeric(TempString(2)) Then
                        BitToProcess.ControlPoint = CInt(TempString(2))
                        If BitToProcess.ControlPoint = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.ControlPoint = Me.StationNumber
                    End If
                Else
                    BitToProcess.ControlPoint = Me.StationNumber
                End If

                If TempString.Length >= 4 Then
                    If IsNumeric(TempString(3)) Then
                        BitToProcess.Codeline = CInt(TempString(3))
                        If BitToProcess.Codeline = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.Codeline = Me.CodelineNumber
                    End If
                Else
                    BitToProcess.Codeline = Me.CodelineNumber
                End If

                'Check to see if the bitposition already exists in the global collection
                Dim BitExists As Boolean
                Dim bp As BitPosition
                For Each bp In BitCollection
                    If (bp.ControlPoint = BitToProcess.ControlPoint) _
                    And bp.Codeline = BitToProcess.Codeline _
                    And bp.CodeStation = BitToProcess.CodeStation _
                    And bp.PositionOfBit = BitToProcess.PositionOfBit And bp.TypeOfBit = BitToProcess.TypeOfBit Then
                        BitExists = True
                        bp.ComponentRefCollection.Add(Me)
                        CtlIndRef = bp
                        Me.CodeStation = BitToProcess.CodeStation

                        Exit For
                    End If
                Next

                'if bit does not exist then ad it to the collection
                If Not BitExists Then
                    BitToProcess.TypeOfBit = TypeOfBit
                    GUIDCounter = GUIDCounter + 10
                    BitToProcess.GUID = GUIDCounter
                    CtlIndRef = BitToProcess
                    CtlIndRef.GUID = BitToProcess.GUID
                    BitCollection.Add(BitToProcess)

                    'make a reference to this component from the bit
                    Select Case Me.GetType.ToString
                        Case "TMDS.GraphicComponent.SwitchPointMonitor"
                            'DO NOTHING
                        Case Else
                            BitToProcess.ComponentRefCollection.Add(Me)
                            Me.CodeStation = BitToProcess.CodeStation

                    End Select
                End If

                'added to set a flag on tracks that has an indication assigned to it
                If Me.GetType.ToString = gTrackComponentName And Not Me.Ind(0) Is Nothing Then
                    Me.Indicating = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub TempCodeStationBitBuildSpecial(ByVal BitMappingString As String, ByRef CtlIndRef As BitPosition, ByVal BitCollection As ArrayList, ByRef GUIDCounter As Int32, ByRef TypeOfBit As CodeServerClient.BitPosition.BitType)
            Try
                Dim BitToProcess As BitPosition
                If BitMappingString = "" Or BitMappingString = "NONE" Or BitMappingString = "*****" Then Exit Sub
                Dim TempString() As String = BitMappingString.Split(";")
                BitToProcess = New BitPosition()
                BitToProcess.TypeOfBit = TypeOfBit

                If TempString.Length >= 1 Then
                    If IsNumeric(TempString(0)) Then
                        BitToProcess.ExitSignalGUID = CInt(TempString(0))
                        If BitToProcess.ExitSignalGUID = 0 Then BitToProcess = Nothing : Exit Sub
                    End If
                End If


                If TempString.Length >= 2 Then
                    If IsNumeric(TempString(1)) Then
                        BitToProcess.PositionOfBit = CInt(TempString(1))
                        If BitToProcess.PositionOfBit = 0 Then BitToProcess = Nothing : Exit Sub
                    End If
                End If

                If TempString.Length >= 3 Then
                    If IsNumeric(TempString(2)) Then
                        BitToProcess.CodeStation = CInt(TempString(2))
                        If BitToProcess.CodeStation = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.CodeStation = Me.CodeStation
                    End If
                Else
                    BitToProcess.CodeStation = Me.CodeStation
                End If

                If TempString.Length >= 4 Then
                    If IsNumeric(TempString(3)) Then
                        BitToProcess.ControlPoint = CInt(TempString(3))
                        If BitToProcess.ControlPoint = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.ControlPoint = Me.StationNumber
                    End If
                Else
                    BitToProcess.ControlPoint = Me.StationNumber
                End If

                If TempString.Length >= 5 Then
                    If IsNumeric(TempString(4)) Then
                        BitToProcess.Codeline = CInt(TempString(4))
                        If BitToProcess.Codeline = 0 Then BitToProcess = Nothing : Exit Sub
                    Else
                        BitToProcess.Codeline = Me.CodelineNumber
                    End If
                Else
                    BitToProcess.Codeline = Me.CodelineNumber
                End If

                'Check to see if the bitposition already exists in the global collection
                Dim BitExists As Boolean
                Dim bp As BitPosition
                For Each bp In BitCollection
                    If (bp.ControlPoint = BitToProcess.ControlPoint) _
                    And bp.Codeline = BitToProcess.Codeline _
                    And bp.CodeStation = BitToProcess.CodeStation _
                    And bp.PositionOfBit = BitToProcess.PositionOfBit And bp.TypeOfBit = BitToProcess.TypeOfBit Then
                        BitExists = True
                        bp.ComponentRefCollection.Add(Me)
                        CtlIndRef = bp
                        Me.CodeStation = BitToProcess.CodeStation

                        Exit For
                    End If
                Next

                'if bit does not exist then ad it to the collection
                If Not BitExists Then
                    BitToProcess.TypeOfBit = TypeOfBit
                    GUIDCounter = GUIDCounter + 10
                    BitToProcess.GUID = GUIDCounter
                    CtlIndRef = BitToProcess
                    CtlIndRef.GUID = BitToProcess.GUID
                    BitCollection.Add(BitToProcess)

                    'make a reference to this component from the bit
                    Select Case Me.GetType.ToString
                        Case "TMDS.GraphicComponent.SwitchPointMonitor"
                            'DO NOTHING
                        Case Else
                            BitToProcess.ComponentRefCollection.Add(Me)
                            Me.CodeStation = BitToProcess.CodeStation

                    End Select
                End If

                'added to set a flag on tracks that has an indication assigned to it
                If Me.GetType.ToString = gTrackComponentName And Not Me.Ind(0) Is Nothing Then
                    Me.Indicating = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub AssignDrawingForm(ByRef DrawingForm As System.Windows.Forms.Form)
            pCurrentDrawingForm = DrawingForm
        End Sub

        Sub SetSnippetPosition(ByRef SnippetRegion As ArrayList)
            'This sub calls the setbounds for a snippet of screen and set it as used if the points are found in it.
            Try
                Dim snip As ScreenDrawingRegion
                If Not SnippetRegion Is Nothing Then
                    For Each snip In SnippetRegion
                        If snip.IsInBounds(New Point(LX, LY)) Then
                            pSnippetRegion = snip.RegionNumber
                            Exit Sub
                        End If

                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ReAdjustSnippetRegion(ByRef SnippetRegion() As ScreenDrawingRegion, ByRef TempSnippetRegion() As ScreenDrawingRegion)
            Dim TraceMessage As String = String.Empty
            Try

                If pSnippetRegion <> 0 Then
                    Dim NewRegion As Integer = FindAvailableRegion(pSnippetRegion, SnippetRegion)
                    'TraceMessage = TraceMessage & "NewRegion:" & NewRegion & "; pSnippetRegion:" & pSnippetRegion
                    'TraceMessage = TraceMessage & "; SnippetRegion(NewRegion):"
                    'TraceMessage = TraceMessage & SnippetRegion(NewRegion).MapFromRegion
                    'TraceMessage = TraceMessage & "; TempSnippetRegion(pSnippetRegion):"
                    'TraceMessage = TraceMessage & TempSnippetRegion(pSnippetRegion).MapFromRegion & vbCrLf
                    Dim AdjustedLX, AdjustedLY, AdjustedRX, AdjustedRY As Int16
                    AdjustedLX = pLXpos - TempSnippetRegion(pSnippetRegion).Bounds.X
                    AdjustedLY = pLYpos - TempSnippetRegion(pSnippetRegion).Bounds.Y
                    AdjustedRX = pRXpos - TempSnippetRegion(pSnippetRegion).Bounds.X
                    AdjustedRY = pRYpos - TempSnippetRegion(pSnippetRegion).Bounds.Y

                    pLXpos = SnippetRegion(NewRegion).Bounds.X + AdjustedLX
                    pLYpos = SnippetRegion(NewRegion).Bounds.Y + AdjustedLY
                    pRXpos = SnippetRegion(NewRegion).Bounds.X + AdjustedRX
                    pRYpos = SnippetRegion(NewRegion).Bounds.Y + AdjustedRY

                    pSnippetRegion = NewRegion
                    'TraceMessage = TraceMessage & "ALX,ALY,ARX,ARY:" & AdjustedLX & "," & AdjustedLY & "," & AdjustedRX & "," & AdjustedRY & vbCrLf
                    'TraceMessage = TraceMessage & SnippetRegion(NewRegion).Bounds.X & "," & SnippetRegion(NewRegion).Bounds.Y & vbCrLf
                    'TraceMessage = TraceMessage & "LX,LY,RX,RY:" & pLXpos & "," & pLYpos & "," & pRXpos & "," & pRYpos
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function FindAvailableRegion(ByVal CurrentRegion As Integer, ByRef Snippet() As ScreenDrawingRegion) As Integer
            Dim I As Integer
            Try
                For I = 1 To Snippet.GetUpperBound(0)
                    If Not Snippet(I).BoundsInUse Then
                        If Snippet(I).MapFromRegion = CurrentRegion Then
                            FindAvailableRegion = I
                            Exit Function
                        End If
                    End If

                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Function

        Overridable Sub BuildGraphics()

        End Sub

        Overridable Sub InitGraphics()

        End Sub

        Overridable Sub InitAuthorityNumbers()

        End Sub

        Overridable Sub DrawAuthNumbers(ByRef g As Graphics)

        End Sub

        Public Function DeviceBlockDuplicateCheck(ByVal blockToCheck As DeviceBlock) As Boolean
            Try
                Dim dv As DeviceBlock
                For Each dv In Me.DeviceBlocks
                    If Trim(dv.BlockGUID) = Trim(blockToCheck.BlockGUID) Then
                        Return False
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return True
            End Try
        End Function

        Public Sub DeviceBlockRemoveFromMe(ByVal blockToCheck As DeviceBlock)
            Try
                Dim auth As TrackAuthority.DeviceBlock
                Dim DeleteList As New ArrayList()
                Dim CloneList As ArrayList = Me.DeviceBlocks.Clone
                For Each auth In CloneList
                    If Trim(auth.BlockGUID) = Trim(blockToCheck.BlockGUID) Then
                        DeleteList.Add(auth)
                    End If
                Next
                For Each auth In DeleteList
                    Me.DeviceBlocks.Remove(auth)
                Next

                DeleteList.Clear()
                DeleteList = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function ShallowCopy() As GenericComponent
            Try
                Return Me.MemberwiseClone()
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try

        End Function

#Region "Weather station Menu "

        Public Sub AddWeatherStationMenuItem()
            Try
                If ContextMenuControl.MenuItems.Count > 0 Then
                    Dim mnuSplit3 As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit3)
                End If
                mnuWeatherReport = New MenuItem
                mnuWeatherReport = New MenuItem("CREATE WEATHER REPORT")
                AddHandler mnuWeatherReport.Click, AddressOf Me.mnuWeatherReport_Click
                ContextMenuControl.MenuItems.Add(mnuWeatherReport)


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Overridable Sub mnuWeatherReport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Using frmWeather As New TMDS.frmWeatherReport
                    With frmWeather
                        .Owner = Me.CurrentDrawingForm
                        .gWeatherStation = Me
                        .ShowDialog()
                    End With
                End Using


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region
#End Region

#Region "COMPONENT ALERTS"

        Public Sub ShowAlarmForUnSolicitedSignal(ByVal sig As Signal)
            Try
                If sig Is Nothing Then Exit Sub
                If sig.myControlPoint.ManualOperateMode Then Exit Sub
                Dim AlertMessage As String = sig.Name & "|" & sig.GUID.ToString & "|" & sig.TerritoryID & "|" & sig.myControlPoint.Name
                If gDisbleIndicationAlertOnAssistMode AndAlso gAssistMode OrElse sig.IsShadowMode Then
                    TMDS.SystemUtilities.Utilities.SystemEvent("UnSolicited Signal:" & AlertMessage)
                ElseIf Not optDisableUnSolicatedSignalAlert Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.UnSolicitedSignal, AlertMessage, Nothing, Nothing, sig, Nothing)
                Else
                    TMDS.SystemUtilities.Utilities.SystemEvent("UnSolicited Signal:" & AlertMessage)
                End If

                If optPublishComponentAlert AndAlso Not (gAssistMode OrElse sig.IsShadowMode) Then
                    Dim PublistAlertMessage As String = ManagementClasses.Alert.AlertType.UnSolicitedSignal.ToString & ";" & "UNSOLICITED SIGNAL" & ";" & sig.myControlPoint.Name.ToUpper & ";" & Now.ToString & ";" & "UNSOLICITED SIGNAL" & ";" & sig.Name
                    Dim Message As String = "DC" & Chr(6) & "CADALERTBROADCAST;"
                    Message = Message & "|" & sig.TerritoryID.ToString
                    Message = Message & "|" & sig.GUID.ToString
                    Message = Message & "|" & PublistAlertMessage
                    Message = sig.TerritoryID.ToString & Chr(12) & Message
                    SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                        TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                    End SyncLock
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ShowAlarmForOutOfCorrespondenceRCPS(ByVal sw As Switch, ByVal auth As SFAuthority)
            Try
                If sw Is Nothing Then Exit Sub
                If sw.myControlPoint.ManualOperateMode Then Exit Sub
                Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo

                'check for existing alert
                Dim al As Alert
                Dim AlertExists As Boolean
                For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                If Not AlertExists Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.OutOfCorrespondenceRCPS, AlertMessage, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, sw)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ShowAlarmForCommFailureRCPS(ByVal sw As Switch, ByVal auth As SFAuthority)
            Try
                If sw Is Nothing Then Exit Sub
                Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo

                'check for existing alert
                Dim al As Alert
                Dim AlertExists As Boolean
                For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                If Not AlertExists Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.CommFailureRCPS, AlertMessage, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, sw)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForTrainMoveByRedSignal(ByVal trk As Track, ByVal trn As TrainID)
            Try
                If trk.myControlPoint.ManualOperateMode Then Exit Sub

                If trk.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated Then
                    'do nothing because there is a check for unsolicited signals
                ElseIf trk.SignalStatOnMe <> Track.SignalStatus.SignalCleared And (Not trk.BlockAbsolute) Then

                    If optPassRedSignalOnRequestedSignalDisabled And trk.SignalStatOnMe = Track.SignalStatus.SignalRequested Then
                        Exit Sub
                    End If

                    Dim trackToCheck As Track = Nothing
                    Dim SlotTrack As Track = Nothing
                    For Each trackToCheck In trk.OsComponentCollection
                        If trackToCheck.SlottingTrack Then
                            SlotTrack = trackToCheck
                            Exit For
                        End If
                    Next
                    If Not IsNothing(SlotTrack) Then
                        Dim sig As Signal
                        For Each sig In SlotTrack.SignalSlotCollection
                            If sig.NoAlarmOnPending Then
                                If trk.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Or trk.SignalStatOnMe = Track.SignalStatus.SignalRequested Then
                                    Exit Sub
                                End If
                            ElseIf (sig.IndicateOnly Or sig.MatchMode) And sig.SignalIndOnlyStatusPrevious <> Signal.IndStatus.Stopped Then
                                sig.SignalIndOnlyStatusPrevious = Signal.IndStatus.Stopped
                                Exit Sub
                            ElseIf sig.SlottedByOSTrack And sig.PreviousSignalStatus = Signal.BulbStatus.ClearStatus Then
                                Exit Sub
                            End If
                        Next
                    End If

                    If optEnableShadowMode OrElse optEnableTerritoryDeskAssignmentMode Then
                        If Not trk.IsValidTrackForOperation Then Exit Sub
                    End If
                    If optEnableCNSpecificOptions AndAlso Not IsTrackValidForPRSAlert(SlotTrack, trn) Then
                        Exit Sub
                    End If

                    If trn Is Nothing Then
                        If optTrainPassRedSignalWithTrainSymbolOnly Then Exit Sub
                        trn = New TrainID()
                        trn.Name = "UNKNOWN TRAIN"
                        trn.IdentifingUnit = "UNKNOWN UNIT ID"

                    End If

                    Dim AlertMessage As String = trk.TrackName & "|" & trk.GUID.ToString & "|" & trk.TerritoryID & "|" & trk.myControlPoint.Name & "|" & trn.Name & "|" & trn.IdentifingUnit
                    If optUseAbbreviatedTrainPassRedVerbiage Then
                        AlertMessage = trk.myControlPoint.Name & "|" & trn.Name & "|" & trn.IdentifingUnit
                    End If

                    Dim MessageAlert As String = "TRAIN PASS RED:" & trk.TrackName & "|" & trk.GUID.ToString & "|" & trk.TerritoryID & "|" & trk.myControlPoint.Name & "|" & trn.Name & "|" & trn.IdentifingUnit

                    If gDisbleIndicationAlertOnAssistMode AndAlso (gAssistMode OrElse trk.IsShadowMode) Then
                        TMDS.SystemUtilities.Utilities.SystemEvent(MessageAlert)
                    ElseIf Not optDisablePassRedSignalAlert Then
                        If Not DoesAlarmExist(AlertMessage) Then
                            TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.PassRedSignal, AlertMessage, Nothing, Nothing, Nothing, trk)
                        End If
                    Else
                        TMDS.SystemUtilities.Utilities.SystemEvent(MessageAlert)
                    End If

                    If optPublishComponentAlert AndAlso Not (gAssistMode OrElse trk.IsShadowMode) Then
                        Dim PublistAlertMessage As String = ManagementClasses.Alert.AlertType.PassRedSignal.ToString & ";" & optSystemAlertMessage2 & ";" & trk.myControlPoint.Name.ToUpper & ";" & Now.ToString & ";" & optSystemAlertMessage2 & ";" & trn.Name

                        Dim Message As String = "DC" & Chr(6) & "CADALERTBROADCAST;"
                        Message = Message & "|" & trk.TerritoryID.ToString
                        Message = Message & "|" & trk.GUID.ToString
                        Message = Message & "|" & PublistAlertMessage
                        Message = trk.TerritoryID.ToString & Chr(12) & Message
                        SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                            TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                        End SyncLock
                    End If

                    trn = Nothing
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub
        Private Function DoesAlarmExist(AlertMessage As String) As Boolean
            Dim al As Alert

            Try
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.PassRedSignal, ManagementClasses.Alert.AlertType.OSOccupancy
                                If AlertMessage = al.AlertData Then
                                    Return True
                                End If
                        End Select
                    Next
                End SyncLock
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub ShowAlarmForOpenSwitch(ByVal sw As Switch, ByVal auth As SFAuthority, ByVal LastStation As String)
            Try
                If sw Is Nothing Then Exit Sub
                Dim AlertMessage As String = sw.Name & "|" & sw.GUID.ToString & "|" & sw.TerritoryID & "|" & sw.myControlPoint.Name & "|" & auth.IssueTo & "|" & auth.AuthorityUID & "|" & LastStation

                'check for existing alert
                Dim al As Alert
                Dim AlertExists As Boolean
                For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                If Not AlertExists Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.SwitchPointOpen, AlertMessage, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, sw)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ShowAlarmForTrackZone(ByVal tz As TrackZone, ByVal auth As SFAuthority)
            Try
                If tz Is Nothing Then Exit Sub
                Dim BeginLimit As String = String.Empty
                Dim EndLimit As String = String.Empty

                If auth.Direction = "RIGHT" Then
                    'For Each trk In tz.TrackList
                    '    If auth.Components.Contains(trk) Then
                    '        If BeginLimit = "" Then
                    '            BeginLimit = trk.LeftLimitName
                    '        End If
                    '        EndLimit = trk.RightLimitName
                    '    End If
                    'Next
                    Dim BeginTrack As Track = tz.TrackList.Item(0)
                    BeginLimit = BeginTrack.LeftLimitName

                    Dim EndTrack As Track = tz.TrackList.Item(tz.TrackList.Count - 1)
                    EndLimit = EndTrack.RightLimitName

                Else
                    'For Each trk In tz.TrackList
                    '    If auth.Components.Contains(trk) Then
                    '        If BeginLimit = "" Then
                    '            BeginLimit = trk.RightLimitName
                    '        End If
                    '        EndLimit = trk.LeftLimitName
                    '    End If
                    'Next
                    Dim BeginTrack As Track = tz.TrackList.Item(0)
                    EndLimit = BeginTrack.LeftLimitName

                    Dim EndTrack As Track = tz.TrackList.Item(tz.TrackList.Count - 1)
                    BeginLimit = EndTrack.RightLimitName

                End If

                Dim AlertMessage As String = tz.ZoneName & "|" & tz.ZoneUID.ToString & "|" & BeginLimit & " TO " & EndLimit & "|" & auth.IssueTo & "|" & auth.AuthorityUID
                'check for existing alert
                Dim al As Alert
                Dim AlertExists As Boolean
                For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                For Each al In gSystemAlertQueue
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                If Not AlertExists Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.TrackZone, AlertMessage, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, tz)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ShowAlarmForFoulingTrack(ByVal thisTrack As Track, ByVal auth As SFAuthority)
            Try
                Dim sw As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(thisTrack)

                Dim SwitchName As String = thisTrack.Name

                If Not IsNothing(sw) Then
                    SwitchName = sw.Name
                End If


                Dim AlertMessage As String = SwitchName & "|" & thisTrack.Name & "|FOULING POINT INDICATION|" & auth.IssueTo & "|" & auth.AuthorityUID
                'check for existing alert
                Dim al As Alert
                Dim AlertExists As Boolean
                For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                For Each al In gSystemAlertQueue

                    If al.AlertData = AlertMessage And Not al.AlertAcknowledged Then
                        AlertExists = True
                        Exit For
                    End If
                Next
                If Not AlertExists Then
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.FoulingPoint, AlertMessage, Nothing, Nothing, Nothing, thisTrack)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ShowAlarmForNonIdentifiedOccupancy(ByVal trk As Track)
            Dim al As Alert
            Try
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.NonIdentifiedTrackOccupancy
                                Dim trk1 As Track = CType(al.AlertObject, Track)
                                If Not IsNothing(trk1) Then
                                    If trk1.GUID = trk.GUID Then
                                        Exit Sub
                                    End If
                                End If
                        End Select
                    Next
                End SyncLock
                Dim AlertMessage As String = trk.TrackName & "|" & trk.GUID.ToString & "|" & trk.TerritoryID & "|" & trk.myControlPoint.Name
                If gDisbleIndicationAlertOnAssistMode AndAlso (gAssistMode OrElse trk.IsShadowMode) Then
                    TMDS.SystemUtilities.Utilities.SystemEvent("UN-IDENTIFIED TRACK OCCUPANCY:" & AlertMessage)
                Else
                    TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.NonIdentifiedTrackOccupancy, AlertMessage, Nothing, Nothing, Nothing, trk)
                    TMDS.SystemUtilities.Utilities.SystemEvent("UN-IDENTIFIED TRACK OCCUPANCY:" & AlertMessage)
                End If

                If optPublishComponentAlert AndAlso Not (gAssistMode OrElse trk.IsShadowMode) Then
                    Dim PublistAlertMessage As String = ManagementClasses.Alert.AlertType.UnSolicitedSignal.ToString & ";" & optSystemAlertMessage8 & ";" & trk.myControlPoint.Name.ToUpper & ";" & Now.ToString & ";" & optSystemAlertMessage10

                    Dim Message As String = "DC" & Chr(6) & "CADALERTBROADCAST;"
                    Message = Message & "|" & trk.TerritoryID.ToString
                    Message = Message & "|" & trk.GUID.ToString
                    Message = Message & "|" & PublistAlertMessage
                    Message = trk.TerritoryID.ToString & Chr(12) & Message
                    SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                        TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                    End SyncLock
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Function IsTrackValidForPRSAlert(ByVal TrackToCheck As Track, ByVal trn As TrainID) As Boolean
            Dim Comp As Object = Nothing
            Dim ApproachTrack As Object = Nothing
            Dim LeftObject As Object = Nothing
            Dim RightObject As Object = Nothing
            Try
                If Not TrackToCheck.OStype Then Return False
                If TrackToCheck.PassRedSignalAlertOff Then Return False
                If TrackToCheck.SignalStatOnMe = Track.SignalStatus.SignalCleared Then Return False
                If TrackToCheck.TrackAuthorities IsNot Nothing AndAlso TrackToCheck.TrackAuthorities.Count > 0 Then Return False
                LeftObject = FindCompFromGUID(TrackToCheck.LeftLink)
                RightObject = FindCompFromGUID(TrackToCheck.RightLink)
                'Do a Left Search
                If LeftObject IsNot Nothing Then
                    For i As Integer = 0 To 10
                        If LeftObject Is Nothing Then Exit For
                        Select Case LeftObject.GetType.ToString
                            Case gSignalComponentName
                                LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                If LeftObject.GetType.ToString = gTrackComponentName AndAlso LeftObject.Occuppied Then
                                    ApproachTrack = LeftObject
                                End If
                                Exit For
                            Case gSwitchComponentName
                                If TrackToCheck.OsComponentCollection.Contains(LeftObject) Then
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                Else
                                    Exit For
                                End If
                            Case gTrackComponentName
                                If TrackToCheck.OsComponentCollection.Contains(LeftObject) Then
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                Else
                                    Exit For
                                End If
                        End Select
                    Next
                End If
                If RightObject IsNot Nothing AndAlso ApproachTrack Is Nothing Then
                    For i As Integer = 0 To 10
                        If RightObject Is Nothing Then Exit For
                        Select Case RightObject.GetType.ToString
                            Case gSignalComponentName
                                RightObject = FindCompFromGUID(RightObject.RightLink)
                                If RightObject.GetType.ToString = gTrackComponentName AndAlso RightObject.Occuppied Then
                                    ApproachTrack = RightObject
                                End If

                                Exit For
                            Case gSwitchComponentName
                                If TrackToCheck.OsComponentCollection.Contains(RightObject) Then
                                    RightObject = FindCompFromGUID(RightObject.RightLink)
                                Else
                                    Exit For
                                End If
                            Case gTrackComponentName
                                If TrackToCheck.OsComponentCollection.Contains(RightObject) Then
                                    RightObject = FindCompFromGUID(RightObject.RightLink)
                                Else
                                    Exit For
                                End If
                        End Select
                    Next
                End If

                If ApproachTrack Is Nothing Then
                    ShowOccupancyAlert(TrackToCheck, trn)
                    Return False
                ElseIf ApproachTrack IsNot Nothing AndAlso ApproachTrack.GetType.ToString = gTrackComponentName Then
                    If Not ApproachTrack.Occuppied Then
                        ShowOccupancyAlert(TrackToCheck, trn)
                        Return False
                    End If
                End If

                Return True
            Catch ex As Exception
                GetErrorDetails(ex)
                Return True
            End Try
        End Function

        Private Sub ShowOccupancyAlert(TrackToCheck As Track, trn As TrainID)
            Try
                'OS OCCUPIED NO TRAIN ID ASSOCIATED ALERT
                Dim AlertMessage As String = "OS OCCUPIED NO TRAIN ID ASSOCIATED ALERT|" & TrackToCheck.TrackName & "|" & TrackToCheck.GUID.ToString & "|" & TrackToCheck.TerritoryID & "|" & TrackToCheck.myControlPoint.Name
                If trn IsNot Nothing Then
                    AlertMessage = "OS OCCUPIED ALERT|" & TrackToCheck.TrackName & "|" & TrackToCheck.GUID.ToString & "|" & TrackToCheck.TerritoryID & "|" & TrackToCheck.myControlPoint.Name & "|" & trn.Name & "|" & trn.IdentifingUnit
                End If


                TMDS.SystemUtilities.Utilities.SystemEvent(AlertMessage)

                If gDisbleIndicationAlertOnAssistMode AndAlso (gAssistMode OrElse TrackToCheck.IsShadowMode) Then
                Else
                    If Not DoesAlarmExist(AlertMessage) Then
                        TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.OSOccupancy, AlertMessage, Nothing, Nothing, Nothing, TrackToCheck)
                    End If

                End If

                If optPublishComponentAlert AndAlso Not (gAssistMode OrElse TrackToCheck.IsShadowMode) Then
                    Dim PublistAlertMessage As String = ManagementClasses.Alert.AlertType.OSOccupancy.ToString & ";OS OCCUPIED NO TRAIN ID ASSOCIATED ALERT;" & TrackToCheck.myControlPoint.Name.ToUpper & ";" & Now.ToString & ";"
                    If trn IsNot Nothing Then
                        PublistAlertMessage = ManagementClasses.Alert.AlertType.OSOccupancy.ToString & ";OS OCCUPIED ALERT;" & TrackToCheck.myControlPoint.Name.ToUpper & ";" & Now.ToString & ";"
                    End If
                    Dim Message As String = "DC" & Chr(6) & "CADALERTBROADCAST;"
                    Message = Message & "|" & TrackToCheck.TerritoryID.ToString
                    Message = Message & "|" & TrackToCheck.GUID.ToString
                    Message = Message & "|" & PublistAlertMessage
                    Message = TrackToCheck.TerritoryID.ToString & Chr(12) & Message
                    SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                        TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                    End SyncLock
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

    End Class

    Public Class CompStatusBit
        Public GUID As Int32
        Public Status As Boolean
        Public BitIndex As Integer
        Public Codeline As Integer
        Public ControlPoint As Int32
        Public CodeStation As Integer

        Public Sub New()

        End Sub

    End Class

#End Region

#Region "CONTROL POINT"

    Public Class ControlPoint
        Inherits GenericComponent

        Private Delegate Sub DoPublishSwitchEvent(ByVal EventType As TMDS.SwitchComponents.Switch.EventTypeSw)
        Private Delegate Sub DoPublishSignalEvent(ByVal EventType As TMDS.SignalComponents.Signal.EventTypeSig)
        Private Delegate Sub DoEventLogSwitch(ByVal EventType As TMDS.SwitchComponents.Switch.EventTypeSwitch)
        Private Delegate Sub DoEventLogSignal(ByVal EventType As TMDS.SignalComponents.Signal.EventTypeSignal)
        Private Delegate Sub DoEventLogDevice(ByVal EventType As TMDS.GraphicComponent.Device.EventTypeDevice)


#Region "FIELDS"

        'Private pStationFont As Font = New Font("Courier", 13.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
        Private pStationFont As Font = New Font("Arial", 13.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
        'Private pStationFont As Font = New Font("ARIAL", 14.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
        'Private pStationFont As Font = New Font("Microsoft Sans Serif", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private pStationFontBold As Font = New Font("Courier", 10.5!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
        Private pStationBox As RectangleF
        Private pStationLocation As Point
        Private pStationBoxSize As SizeF
        Private pStationColor As Color = Color.PaleGreen
        Private pStationColorOff As Color = Color.Black
        Private pStationColorOn As Color = Color.WhiteSmoke
        Private pStationBrush As SolidBrush = New SolidBrush(pStationColor)
        Private pStationBrushOff As SolidBrush = New SolidBrush(pStationColorOff)
        Private pStationBrushOn As SolidBrush = New SolidBrush(pStationColorOn)
        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()
        Private pStringSize As SizeF
        Private DeviceCollection As New ArrayList()
        Private pStationBrushAutoMode As SolidBrush = New SolidBrush(Color.Gold)
        Private pStationBrushMaintenanceMode As SolidBrush = New SolidBrush(Color.Blue)

        Public CodeStations As New ArrayList()
        Public AssignedComponenets As New ArrayList()
        Public AssignedTracks As New ArrayList()

        Public StationRect As New Rectangle()
        Public flashNameTimer As New Timers.Timer()
        Public flashDeviceTimer As New Timers.Timer()
        Public devMOK As Device
        Public devPOK As Device
        Public devLOK As Device
        Public devDOK As Device
        Public devSMK As Device
        Public devSMKINIT As Boolean
        Public devGEN As Device
        Public devMCK As Device
        Public devCFK As Device 'CODE FAILURE INDICATION
        Public pFlashNameStartTime As New DateTime
        Public ManualOperateMode As Boolean = False
        Private FlashOffDevBrush As New SolidBrush(Color.Black)
        Private FlashOnDevBrush As New SolidBrush(Color.PaleGreen)
        Private MouseEntered As Boolean
        Private MouseLeft As Boolean
        Private mnuRECALL As MenuItem
        Private mnuRESEND As MenuItem
        Private mnuReleaseControl As MenuItem
        Private mnuRequestControl As MenuItem
        Public mnuSMZ As MenuItem
        Public mnuSMZStop As MenuItem
        Private mnuMCZ As MenuItem
        Private mnuRoutes As MenuItem
        Private mnuMaintenanceModeRequest As MenuItem
        Private mnuMaintenanceModeRelease As MenuItem

        Private FlashDevices As Boolean
        Private FlashOnDevices As Boolean


        Public CodeServerList As ArrayList
        Public CodeStationList As ArrayList
        Public CodeBitList As ArrayList
        Public CodeStationsToSend As New ArrayList()

        Public RUID As Int32
        Public UniqueName As String = ""
        Public StoredRoutes As ArrayList = ArrayList.Synchronized(New ArrayList())
        Public ResendTime As Date
        Public RecallTime As Date
        Public LockStoredRouteList As Boolean

        Public SlotSMControlOnIndication As Boolean
        Public TrackRoutes As ArrayList

        Public AllStationsInitialized As Boolean
        Public ContainsMultipleStations As Boolean
        Private pInitialized As Boolean

        'TRAINER MODE MENUS
        Private mnuTrainerPowerOFF As MenuItem
        Private mnuTrainerPowerON As MenuItem
        Private mnuTrainerFailStationOn As MenuItem
        Private mnuTrainerFailStationOff As MenuItem
        Private mnuTrainerMOKOFF As MenuItem
        Private mnuTrainerMOKON As MenuItem

        Public SubdvnGeoReference As Integer

        Public AllowStatusPublishing As Boolean

        Public StatusPreviousState As Boolean

        Public CurrentlySelected As Boolean
        Public EnableResendControlLogic As Boolean = True
        Public ResendControlTime As Int32 = 60000

        'AUTOROUTING MODE
        Public AllowAutomaticMode As Boolean
        Public AutomaticModeOn As Boolean
        Private mnuAutoRouting As MenuItem

        'SNOW MELTOR CONTROL
        Public AllowSnowMelterTimer As Boolean
        Public SnowMelterTime As Int32
        Public SNMZTime As Integer = 0
        Public SnowMelterStopClickedTime As Date
        Public SnowMelterStopTime As Date = Now.AddMinutes(-1)
        Public SnowMelterMinimumHour As Integer = 3
        'MAINTENANCE MODE
        Public MaintenanceMode As Boolean
        Public MaintenanceModeRemoteUID As String

        Private pCommFailState As Boolean
        Private pPowerOff As Boolean
        Private IndicatedOnce As Boolean

        Public AllowControlPointCorrelation As Boolean
        Public CorrelatingControlPointReferences As String
        Public CorrelatingControlPoints As ArrayList

        Private pTemporalModePassenger As Boolean
        Private pTemporalModeFreight As Boolean
        Public devTMP As Device 'Temporal Mode Passenger
        Public devTMF As Device 'Temporal Mode Freight

        Public AllowTemporalModeOperations As Boolean
        Public TemporalAlarmGenerated As Boolean
        Public TemporalAlarmAcknowledged As Boolean
        Public TemporalAlarmAcknowledgedTime As Date
        Public AllowPointOfConflictOperations As Boolean
        Public UniqueID As String

        Public DisableAutomaticSignalSlotting As Boolean



        'ARRC TUNNEL OPERATIONS
        Public TunnelAutomaticMode As Boolean
        Private pTunnelAutomaticControlled As Boolean
        Private mnuTunnelAutomaticMode As MenuItem
        Private mnuTunnelMaintenaceMode As MenuItem

        'ELECTRICAL LINE VARIABLES
        Public ScadaAlarmReportingIndicationBit As String
        Public ScadaNonReportingModeControlled As Boolean

        Public ScadaTractionPowerControlPointMode As Boolean
        Public ScadaCommunicationControlPointMode As Boolean

        Public TractionCPCommPath As GraphicsPath
        Public TractionCPStatePath As New ArrayList

        Public Enum ControlPointType
            Normal
            TractionControl
            ScadaCommunication
        End Enum
        Public DevicePTCUID As String = String.Empty

        'SHADOW MODE VARIABLES
        Public ControllingWorkstation As String = ""
        Public ControllingWorkStationIPString As String = ""

        Public mySubdivisionStation As SubdivisionStation

        'Signal Route
        Public myRightSignalRoutes As New List(Of SignalRoute)
        Public myLeftSignalRoutes As New List(Of SignalRoute)

#End Region

#Region "PROPERTIES"

        Public CtlSM As BitPosition = Ctl(0)
        Public CtlSMOFF As BitPosition = Ctl(2)
        Public CtlMC As BitPosition = Ctl(1)

        Public Property StationFont() As Font
            Get
                Return pStationFont
            End Get
            Set(ByVal Value As Font)
                pStationFont = Value
            End Set
        End Property
        Public Property StationLocation() As Point
            Get
                Return pStationLocation
            End Get
            Set(ByVal Value As Point)
                pStationLocation = Value
            End Set
        End Property
        Public Property StationBoxSize() As SizeF
            Get
                Return pStationBoxSize
            End Get
            Set(ByVal Value As SizeF)
                pStationBoxSize = Value
            End Set
        End Property
        Public Property StationColor() As Color
            Get
                Return pStationColor
            End Get
            Set(ByVal Value As Color)
                pStationColor = Value
            End Set
        End Property
        Public Property CommFail() As Boolean
            Get
                Return pCommFailState
            End Get
            Set(ByVal Value As Boolean)
                If Value <> pCommFailState And IndicatedOnce Then
                    Dim LogThisEvent As New DoEventLogControlPoint(AddressOf Me.LogTheControlPointEvent)
                    LogThisEvent.BeginInvoke(EventTypeControlPoint.COMMUNICATIONFAILURE, Nothing, Nothing, Nothing)
                End If
                pCommFailState = Value

            End Set

        End Property
        Public Property PowerOff() As Boolean
            Get
                Return pPowerOff
            End Get
            Set(ByVal Value As Boolean)

                If pPowerOff <> Value And IndicatedOnce Then
                    Dim LogThisEvent As New DoEventLogControlPoint(AddressOf Me.LogTheControlPointEvent)
                    LogThisEvent.BeginInvoke(EventTypeControlPoint.POWEROFF, Nothing, Nothing, Nothing)
                End If
                pPowerOff = Value
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
        Public Property TemporalModePassenger() As Boolean
            Get
                Return pTemporalModePassenger
            End Get
            Set(ByVal Value As Boolean)
                pTemporalModePassenger = Value
            End Set
        End Property
        Public Property TemporalModeFreight() As Boolean
            Get
                Return pTemporalModeFreight
            End Get
            Set(ByVal Value As Boolean)
                pTemporalModeFreight = Value
            End Set
        End Property

        Public Property TunnelAutomaticControlled() As Boolean

            Get
                Return pTunnelAutomaticControlled
            End Get

            Set(ByVal Value As Boolean)
                pTunnelAutomaticControlled = Value
                If pTunnelAutomaticControlled Then
                    If Not Me.Ctl(6) Is Nothing Then
                        Me.Ctl(6).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If
                Else
                    If Not Me.Ctl(6) Is Nothing Then
                        Me.Ctl(6).BitState = False
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                End If

                If Me.Initialized Then
                    Me.SendCodeStationControlSpecial(True)
                Else
                    ' Me.SendCodeStationControlUpdate()
                End If
            End Set
        End Property

#End Region

#Region "MAIN METHODS"

        Sub New()
            'Build the flasher timer for the signal bulb
            MouseLeft = True
            Me.Visible = False
            Flasher = New Windows.Forms.Timer() 'New Timers.Timer()
            Flasher.Interval = 250
            AddHandler Flasher.Tick, AddressOf FlashElapsedEvent

            Me.flashDeviceTimer.Interval = 500
            Me.flashDeviceTimer.AutoReset = False
            Me.flashNameTimer.AutoReset = True
            AddHandler flashNameTimer.Elapsed, AddressOf Me.FlashNameEvent
            AddHandler flashDeviceTimer.Elapsed, AddressOf Me.FlashControlPointDevices
            ReDim Me.Ind(9)
            ReDim Me.Ctl(9)
        End Sub

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then

                Try
                    RemoveHandler Me.MouseDown, AddressOf Me.ControlPoint_MouseDown
                    RemoveHandler Me.MouseEnter, AddressOf Me.ControlPoint_MouseEnter
                    RemoveHandler Me.MouseLeave, AddressOf Me.ControlPoint_MouseLeave
                    RemoveHandler Me.MouseHover, AddressOf Me.ControlPoint_MouseHover
                Catch
                End Try
                Try

                    RemoveHandler Flasher.Tick, AddressOf FlashElapsedEvent
                    Flasher.Dispose()
                    Flasher = Nothing
                Catch
                End Try
                Try

                    RemoveHandler flashNameTimer.Elapsed, AddressOf Me.FlashNameEvent
                    flashNameTimer.Dispose()
                    flashNameTimer = Nothing
                Catch
                End Try
                Try
                    RemoveHandler flashDeviceTimer.Elapsed, AddressOf Me.FlashControlPointDevices
                    flashDeviceTimer.Dispose()
                    flashDeviceTimer = Nothing
                Catch
                End Try
                Try
                    Me.pStationBrushOn = Nothing
                    Me.FlashOnDevBrush = Nothing
                    Me.pStationFontBold = Nothing
                    Me.pStringFormat = Nothing
                    Me.pStationBrushOff = Nothing
                    Me.FlashOffDevBrush = Nothing
                    Me.pStationBrush = Nothing
                    Me.pStationFont = Nothing
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

        Public Sub ReorderStoredRoutes()
            Try
                Dim sr As StoredRoute
                Dim count As Integer
                SyncLock StoredRoutes
                    For Each sr In Me.StoredRoutes
                        sr.Priority = count
                        sr.UpdateRoute()
                        count = count + 1
                    Next
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region

#Region "CONTEXT MENU"

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                'If Me.IsShadowMode Then
                '    Dim m As MenuItem
                '    For Each m In Me.ContextMenu.MenuItems
                '        Select Case m.Text
                '            Case "RELEASE CONTROL", "REQUEST CONTROL"
                '                mnuRequestControl.Enabled = True
                '                mnuRequestControl.Visible = True
                '            Case Else
                '                m.Visible = False

                '        End Select
                '    Next
                'End If
                If Me.IsShadowMode Then
                    Dim m As MenuItem
                    For Each m In Me.ContextMenu.MenuItems
                        Select Case m.Text
                            Case "REQUEST CONTROL" '"RELEASE CONTROL",
                                If IsControlPointValidForTransfer() Then
                                    mnuRequestControl.Enabled = True
                                Else
                                    mnuRequestControl.Enabled = False
                                End If
                                mnuRequestControl.Visible = True

                            Case Else
                                m.Visible = False
                        End Select
                    Next
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

                    Me.mnuTrainerFailStationOff = New MenuItem("STATION-INDICATE")
                    AddHandler mnuTrainerFailStationOff.Click, AddressOf Me.mnuTrainerFailStationOff_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerFailStationOff)

                    Me.mnuTrainerFailStationOn = New MenuItem("STATION-FAIL")
                    AddHandler mnuTrainerFailStationOn.Click, AddressOf Me.mnuTrainerFailStationOn_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerFailStationOn)

                    Me.mnuTrainerPowerOFF = New MenuItem("POWER-OFF")
                    AddHandler mnuTrainerPowerOFF.Click, AddressOf Me.mnuTrainerPowerOFF_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerPowerOFF)

                    Me.mnuTrainerPowerON = New MenuItem("POWER-ON")
                    AddHandler mnuTrainerPowerON.Click, AddressOf Me.mnuTrainerPowerON_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerPowerON)

                    Me.mnuTrainerMOKON = New MenuItem("MANUAL-OPERATE-ON")
                    AddHandler mnuTrainerMOKON.Click, AddressOf Me.mnuTrainerMOKON_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerMOKON)

                    Me.mnuTrainerMOKOFF = New MenuItem("MANUAL-OPERATE-OFF")
                    AddHandler mnuTrainerMOKOFF.Click, AddressOf Me.mnuTrainerMOKOFF_Click
                    ContextMenuControl.MenuItems.Add(mnuTrainerMOKOFF)
                    Me.ContextMenu = Me.ContextMenuControl
                    Exit Sub
                ElseIf gAssistMode And Not gMaintenanceMode Then
                    If optUseTroubleReportingEnhanced And optEnableSpecificFunctionalityForDenver Then 'CMV DTP-115
                        Me.ContextMenuControl = New ContextMenu()
                        AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp
                        AddTroubleReportMenuItem()
                        Me.ContextMenu = Me.ContextMenuControl
                    End If
                    Exit Sub
                End If

                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                If optEnableShadowMode Then
                    mnuReleaseControl = New MenuItem(optGraphicalCompContextMenu_mnuReleaseControl)
                    AddHandler mnuReleaseControl.Click, AddressOf Me.mnuRelease_Click
                    ContextMenuControl.MenuItems.Add(mnuReleaseControl)
                    mnuReleaseControl.Enabled = False 'Me.IsControlled
                    mnuReleaseControl.Visible = False

                    mnuRequestControl = New MenuItem(optGraphicalCompContextMenu_mnuRequestControl)
                    AddHandler mnuRequestControl.Click, AddressOf Me.mnuRequest_Click
                    ContextMenuControl.MenuItems.Add(mnuRequestControl)
                    mnuRequestControl.Enabled = False

                End If

                If Me.AuthorityType <> "CTC" Then
                    If optEnableShadowMode Then
                        Me.ContextMenu = Me.ContextMenuControl
                    End If
                    Exit Sub
                End If

                If optEnableShadowMode Then
                    Dim mnuSplitControl As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplitControl)
                End If

                mnuRECALL = New MenuItem(optGraphicalCompContextMenu_mnuRECALL)
                AddHandler mnuRECALL.Click, AddressOf Me.mnuRecall_Click
                ContextMenuControl.MenuItems.Add(mnuRECALL)

                mnuRESEND = New MenuItem(optGraphicalCompContextMenu_mnuRESEND)
                AddHandler mnuRESEND.Click, AddressOf Me.mnuResend_Click
                ContextMenuControl.MenuItems.Add(mnuRESEND)
                mnuRESEND.Enabled = False

                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem()
                End If


                If gMaintenanceMode Then
                    mnuRESEND.Enabled = False
                    Dim mnuSplit As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit)
                    mnuMaintenanceModeRequest = New MenuItem(optGraphicalCompContextMenu_mnuMaintenanceModeRequest)
                    AddHandler mnuMaintenanceModeRequest.Click, AddressOf Me.mnuRequestControl_Click
                    ContextMenuControl.MenuItems.Add(mnuMaintenanceModeRequest)

                    mnuMaintenanceModeRelease = New MenuItem(optGraphicalCompContextMenu_mnuReleaseControl)
                    AddHandler mnuMaintenanceModeRelease.Click, AddressOf Me.mnuReleaseControl_Click
                    ContextMenuControl.MenuItems.Add(mnuMaintenanceModeRelease)
                    mnuMaintenanceModeRelease.Enabled = False
                    Me.ContextMenu = Me.ContextMenuControl
                    Exit Sub
                End If

                Me.CtlSM = Ctl(0)
                Me.CtlSMOFF = Ctl(2)
                If Not Me.CtlSM Is Nothing Then
                    If optEnableCustomSnowMelterTimer Then
                        mnuSMZ = New MenuItem(optGraphicalCompContextMenu_mnuSMZ)
                        'AddHandler mnuSMZ.Click, AddressOf Me.mnuSMZ_Click
                        For I As Integer = SnowMelterMinimumHour To 10
                            Dim Mi As New MenuItem
                            With Mi
                                .Text = I & " Hour"
                                .Tag = I
                                .Name = "mnuHour" & I
                                .RadioCheck = True
                                AddHandler Mi.Click, AddressOf Me.mnuSMZTimer_Click
                            End With
                            mnuSMZ.MenuItems.Add(Mi)
                        Next
                        Dim MiEx As New MenuItem
                        With MiEx
                            .Text = "-"
                            .Name = "mnuHourSep"
                        End With
                        mnuSMZ.MenuItems.Add(MiEx)
                        MiEx = New MenuItem
                        '
                        mnuSMZStop = New MenuItem(optGraphicalCompContextMenu_mnuSMZStop)
                        With mnuSMZStop
                            .Tag = "Stop"
                            .Enabled = False
                            AddHandler mnuSMZStop.Click, AddressOf Me.mnuSMZTimer_Click
                        End With
                        mnuSMZ.MenuItems.Add(mnuSMZStop)
                        ContextMenuControl.MenuItems.Add(mnuSMZ)
                    Else
                        mnuSMZ = New MenuItem(optGraphicalCompContextMenu_mnuSMZ)
                        AddHandler mnuSMZ.Click, AddressOf Me.mnuSMZ_Click
                        ContextMenuControl.MenuItems.Add(mnuSMZ)

                    End If
                End If

                Me.CtlMC = Ctl(1)
                If Not Me.CtlMC Is Nothing Then
                    mnuMCZ = New MenuItem(optGraphicalCompContextMenu_mnuMCZ)
                    AddHandler mnuMCZ.Click, AddressOf Me.mnuMCZ_Click
                    ContextMenuControl.MenuItems.Add(mnuMCZ)
                    Me.devMCK = New Device()
                    Me.devMCK.TypeOfDevice = "IndicateOnly"
                    Me.devMCK.Name = "MC"
                    Me.DeviceCollection.Add(Me.devMCK)
                End If

                mnuRoutes = New MenuItem(optGraphicalCompContextMenu_mnuRoutes)
                AddHandler mnuRoutes.Click, AddressOf Me.mnuRoutes_Click
                ContextMenuControl.MenuItems.Add(mnuRoutes)
                mnuRoutes.Enabled = True


                If Me.AuthorityType = "CTC" Then
                    Me.ContextMenu = Me.ContextMenuControl
                End If


                If Me.AllowAutomaticMode Then
                    Dim mnuSplit As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit)

                    mnuAutoRouting = New MenuItem("ENABLE AUTO-ROUTING")
                    AddHandler mnuAutoRouting.Click, AddressOf Me.mnuAutoRouting_Click
                    ContextMenuControl.MenuItems.Add(mnuAutoRouting)
                End If

                EnableContextMenu(True)
                If optEnableCustomSnowMelterTimer Then InitializeSnowMelterTimer()

                If TunnelAutomaticMode Then
                    Dim mnuSplit As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit)
                    Dim MenuText As String = optEnableTunnelAutomaticModeString ' "ENABLE TUNNEL AUTOMATIC MODE"
                    If Me.TunnelAutomaticControlled Then
                        MenuText = optDisableTunnelAutomaticModeString ' "DISABLE TUNNEL AUTOMATIC MODE"
                    End If
                    mnuTunnelAutomaticMode = New MenuItem(MenuText)
                    AddHandler mnuTunnelAutomaticMode.Click, AddressOf Me.mnuTunnelAutomaticMode_Click

                    ContextMenuControl.MenuItems.Add(mnuTunnelAutomaticMode)

                    mnuTunnelMaintenaceMode = New MenuItem("MAINTAINER MODE")
                    AddHandler mnuTunnelMaintenaceMode.Click, AddressOf Me.mnuTunnelMaintenanceMode_Click
                    ContextMenuControl.MenuItems.Add(mnuTunnelMaintenaceMode)

                    If Not TunnelAutomaticControlled Then
                        mnuTunnelMaintenaceMode.Visible = False
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub EnableContextMenu(ByVal EnableMode As Boolean)
            Try

                If Me.AuthorityType <> "CTC" Then Exit Sub
                mnuRECALL.Enabled = EnableMode
                mnuRESEND.Enabled = EnableMode
                mnuRoutes.Enabled = EnableMode
                If mnuReleaseControl IsNot Nothing Then mnuReleaseControl.Enabled = False
                If Not Me.CtlSM Is Nothing Then
                    Me.mnuSMZ.Enabled = EnableMode
                End If

                If Not Me.CtlMC Is Nothing Then
                    Me.mnuMCZ.Enabled = EnableMode
                End If

                If Me.AllowAutomaticMode Then
                    If Me.AutomaticModeOn Then
                        mnuAutoRouting.Text = "DISABLE AUTO-ROUTING"
                    Else
                        mnuAutoRouting.Text = "ENABLE AUTO-ROUTING"
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuRecall_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SystemEvent("Clicked in recall control:ControlPoint=" & Me.GUID & ":" & Me.Name)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If

                If Me.RecallTime < Now.AddSeconds(-30) Then
                    TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL RECALL SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                    Me.SendRecall()
                    RecallTime = Now
                Else
                    TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL RECALL SELECTED BUT NOT SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                End If
                Me.FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuRequestControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SystemEvent("Clicked in request control:ControlPoint=" & Me.GUID & ":" & Me.Name)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If
                TMDS.SystemUtilities.Utilities.SystemEvent("REQUEST CONTROL SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                SendRequestForMaintenanceControl()
                Me.FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuReleaseControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SystemEvent("Clicked in release control:ControlPoint=" & Me.GUID & ":" & Me.Name)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If
                TMDS.SystemUtilities.Utilities.SystemEvent("RELEASE CONTROL SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                SendRequestForMaintenanceRelease()
                Me.FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        ''' <summary>
        ''' Click in cancel stored routes in control point menu
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        Public Sub mnuRoutes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim removedSR As New ArrayList
            Dim storedRoute As StoredRoute
            Dim theRoutes As ArrayList
            Dim groupedStoredRoute As StoredRoute
            Dim parentRoute As StoredRoute
            Try
                SystemEvent("Clicked in cancel stored routes:ControlPoint=" & Me.GUID & ":" & Me.Name)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If
                If Me.StoredRoutes.Count <> 0 Then
                    Try
                        If optEnableMetroLinkSpecificOptions Then
                            Dim MessageList As New ArrayList()
                            MessageList.Add("DO YOU WANT TO CANCEL ALL STORED ROUTES ")
                            MessageList.Add("ASSIGNED TO THIS CONTROL POINT [ " & Me.Name & " ]?")
                            MessageList.Add("")
                            If ShowCustomMessageBox(MessageList, optGraphicalCompContextMenu_mnuRoutes, True, Nothing, , False, Me.CurrentDrawingForm, False, "", "", True) <> 6 Then
                                FlasherMode(False)
                                Exit Sub
                            End If
                        End If
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    End Try
                    theRoutes = StoredRoutes.Clone
                    For index As Integer = 0 To theRoutes.Count - 1
                        TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(theRoutes(index), "CANCELLED")
                    Next
                    For Each storedRoute In theRoutes
                        If optEnableStoredLicenseForFreeArea AndAlso storedRoute.SLFATrackList.Count > 0 Then
                            SystemEvent("mnuRoutes_Click:SLFA:No cancelled:SRUID=" & storedRoute.RouteGUID)
                            Continue For
                        End If
                        If storedRoute.GroupedStoredRoute Then 'AndAlso sr.GroupedStoredRouteParent Then
                            Dim tmpGSRCollection As New ArrayList
                            parentRoute = FindGroupRouteParent(storedRoute)
                            If storedRoute.GroupedStoredRouteParent Then parentRoute = storedRoute
                            If Not IsNothing(parentRoute) Then
                                CopyArrayCollection(tmpGSRCollection, parentRoute.GroupedStoredRouteCollection)
                                For Each groupedStoredRoute In tmpGSRCollection
                                    groupedStoredRoute.RemoveMe("mnuRoutes_Click", False, True)
                                    If groupedStoredRoute.CtrlPoint IsNot Nothing Then
                                        If groupedStoredRoute.CtrlPoint Is Me Then
                                            If Not removedSR.Contains(groupedStoredRoute) Then removedSR.Add(groupedStoredRoute)
                                        Else
                                            groupedStoredRoute.CtrlPoint.StoredRoutes.Remove(groupedStoredRoute)
                                        End If
                                    End If
                                Next
                                storedRoute.RemoveMe("mnuRoutes_Click", False, True)
                                If Not removedSR.Contains(storedRoute) Then removedSR.Add(storedRoute)
                            End If
                        ElseIf optEnableGeoFenceOperations Then
                            storedRoute.RemoveMe("mnuRoutes_Click", True)
                            If Not removedSR.Contains(storedRoute) Then removedSR.Add(storedRoute)
                        Else
                            storedRoute.RemoveMe("mnuRoutes_Click")
                            If Not removedSR.Contains(storedRoute) Then removedSR.Add(storedRoute)
                        End If
                    Next
                End If
                For Each storedRoute In removedSR
                    StoredRoutes.Remove(storedRoute)
                Next
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuResend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                SystemEvent("Clicked in resend control:ControlPoint=" & Me.GUID & ":" & Me.Name)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If

                If Me.MaintenanceMode And Not gMaintenanceMode Then
                    AddMessageToFeedback("CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MAINTENANCE MODE")
                    Exit Sub
                End If

                If Me.DoMonitorMode Then
                    Dim UserMessage As String = optControlPointManualOperate1 & Me.Name & optControlPointManualOperate2
                    TMDS.SystemUtilities.Utilities.SystemEvent(UserMessage)
                    Dim UserResponse As Integer = MessageBox.Show(Me.CurrentDrawingForm, UserMessage, optLanguageMessageBoxCaptionDefault1, MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                    If UserResponse = 6 Then
                        Me.SendCodeStationControlSpecial(True, False, optEnableResendControlSpecial)
                        TMDS.SystemUtilities.Utilities.SystemEvent("SELECTED YES TO RESEND CONTROL TO " & Me.Name)
                    Else
                        TMDS.SystemUtilities.Utilities.SystemEvent("SELECTED NO TO RESEND CONTROL TO " & Me.Name)
                    End If
                Else
                    If Me.ResendTime < Now.AddSeconds(-30) Then
                        TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL RESEND SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                        Me.SendCodeStationControlSpecial(True, False, optEnableResendControlSpecial)
                        ResendTime = Now
                    Else
                        TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL RESEND SELECTED BUT NOT SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                    End If
                End If
                Me.FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuRelease_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuRequest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                TMDS.NetworkMessages.CadMessages.SendControlRequestForControlPoint(Me, gCadTransmitQueue)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuMCZ_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If
                If Me.mnuMCZ.Checked Then
                    Me.mnuMCZ.Checked = False
                    Me.CtlMC.BitState = False
                    Me.devMCK.DeviceControlled = False
                Else
                    Me.mnuMCZ.Checked = True
                    Me.CtlMC.BitState = True
                    Me.devMCK.DeviceControlled = True
                End If
                Me.SendCodeStationControlSpecial(True)
                Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.devMCK.LogTheDeviceEvent)
                LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceCTL, Nothing, Nothing)

                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub mnuSMZ_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If
                If Me.mnuSMZ.Checked Then
                    Me.mnuSMZ.Checked = False
                    Me.CtlSM.BitState = False
                    StopSnowMelterTimer()
                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = True
                    End If
                    Me.devSMK.DeviceControlled = False
                Else
                    Me.mnuSMZ.Checked = True
                    Me.CtlSM.BitState = True
                    StartSnowMelterTimer()
                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = False
                    End If
                    Me.devSMK.DeviceControlled = True
                End If
                Me.SendCodeStationControlSpecial(True)
                Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.devSMK.LogTheDeviceEvent)
                LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceCTL, Nothing, Nothing)

                FlasherMode(False)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function StopSnowMelter() As Boolean
            Try
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    'MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE. FORCE STOP SNOWMELTER OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Function
                End If
                If Me.mnuSMZ.Checked Then
                    Me.mnuSMZ.Checked = False
                    Me.CtlSM.BitState = False
                    StopSnowMelterTimer()
                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = True
                    End If
                    Me.devSMK.DeviceControlled = False
                    Me.SendCodeStationControlSpecial(True)
                    Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.devSMK.LogTheDeviceEvent)
                    LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceCTL, Nothing, Nothing)
                Else
                    'Me.mnuSMZ.Checked = True
                    'Me.CtlSM.BitState = True
                    'StartSnowMelterTimer()
                    'If Not IsNothing(Me.CtlSMOFF) Then
                    '    Me.CtlSMOFF.BitState = False
                    'End If
                    'Me.devSMK.DeviceControlled = True
                End If


                FlasherMode(False)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function
        Public Function StartSnowMelter() As Boolean
            Try
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    'MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE. FORCE START SNOWMELTER OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Function
                End If
                If Me.mnuSMZ.Checked Then
                    'Me.mnuSMZ.Checked = False
                    'Me.CtlSM.BitState = False
                    'StopSnowMelterTimer()
                    'If Not IsNothing(Me.CtlSMOFF) Then
                    '    Me.CtlSMOFF.BitState = True
                    'End If
                    'Me.devSMK.DeviceControlled = False
                Else
                    Me.mnuSMZ.Checked = True
                    Me.CtlSM.BitState = True
                    StartSnowMelterTimer()
                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = False
                    End If
                    Me.devSMK.DeviceControlled = True
                    Me.SendCodeStationControlSpecial(True)
                    Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.devSMK.LogTheDeviceEvent)
                    LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceCTL, Nothing, Nothing)
                End If


                FlasherMode(False)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Private Sub SelectStation()
            Try
                FlasherMode(True)
                Threading.Thread.Sleep(5000)
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub StartSnowMelterTimer()
            Try
                If Me.AllowSnowMelterTimer Then
                    SNMZTime = Me.SnowMelterTime * 60
                    tmrSnowMelter.Enabled = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub StopSnowMelterTimer()
            Try
                If Me.AllowSnowMelterTimer Then
                    tmrSnowMelter.Enabled = False
                    SNMZTime = Me.SnowMelterTime * 60
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub AttachSnMZTimer()
            Try
                If Not optEnableCustomSnowMelterTimer Then
                    If Me.AllowSnowMelterTimer Then
                        Me.tmrSnowMelter = New Windows.Forms.Timer()
                        tmrSnowMelter.Interval = 1000
                        AddHandler tmrSnowMelter.Tick, AddressOf SnowMelterCountDown
                        SNMZTime = Me.SnowMelterTime * 60
                    End If
                Else
                    Me.tmrSnowMelter = New Windows.Forms.Timer()
                    tmrSnowMelter.Interval = 1000
                    AddHandler tmrSnowMelter.Tick, AddressOf SnowMelterCountDown
                    SNMZTime = Me.SnowMelterTime * 60
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function FindGroupRouteParent(theRoute As StoredRoute) As StoredRoute
            Try
                Dim cp As ControlPoint
                Dim sr As StoredRoute
                For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                    For Each sr In cp.StoredRoutes
                        If Not IsNothing(sr.GroupedStoredRouteCollection) Then
                            If sr.GroupedStoredRouteCollection.Contains(theRoute) Then
                                Return sr
                            End If
                        End If
                    Next
                Next
                Return Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

#Region "TRAINER MODE MENU"

        Public Sub mnuTrainerFailStationOff_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("FAIL-STATION-OFF")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuTrainerFailStationOn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("FAIL-STATION-ON")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuTrainerPowerON_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("POWER-ON")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuTrainerPowerOFF_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("POWER-OFF")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuTrainerMOKOFF_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("MOK-OFF")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub mnuTrainerMOKON_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Me.SendTrainerCommand("MOK-ON")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

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

#Region "TUNNEL AUTOMATIC MODE"

        Public Sub mnuTunnelMaintenanceMode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                gTunnelMaintenanceMode = True
                gCreateTrackTime = True
                SetCursor(curCreateTT)
                FlasherMode(False)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuTunnelAutomaticMode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim MSGLIST As New ArrayList
            Dim LaunchPoint As New Point(Me.Location.X, Me.Location.Y + 10)
            Try
                If mnuTunnelAutomaticMode.Text = optEnableTunnelAutomaticModeString Then
                    MSGLIST.Add("DO YOU WANT TO SWITCH TO: " & optEnableTunnelAutomaticModeString)
                Else
                    MSGLIST.Add("DO YOU WANT TO SWITCH TO: " & optDisableTunnelAutomaticModeString)
                End If

                If ShowCustomMessageBox(MSGLIST, "SYSTEM MESSAGE", True, LaunchPoint) <> 6 Then
                    FlasherMode(False)
                    Exit Sub
                End If



                If mnuTunnelAutomaticMode.Text = optEnableTunnelAutomaticModeString Then '"ENABLE TUNNEL AUTOMATIC MODE" Then
                    ToggleAutoMode(True)
                    mnuTunnelMaintenaceMode.Visible = True
                Else
                    ToggleAutoMode(False)
                    mnuTunnelMaintenaceMode.Visible = False
                End If
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                MSGLIST = Nothing
                LaunchPoint = Nothing
            End Try
        End Sub

        Private Sub ToggleAutoMode(ByVal ModeOn As Boolean)
            Try
                SystemEvent("CONTROL POINT:" & Me.Name & " TUNNEL AUTOMATIC MODE STATUS:" & ModeOn.ToString.ToUpper)
                TunnelAutomaticControlled = ModeOn
                ApplyTunnelAutomaticMode()
                UpdateTunnelAutomaticModeInDatabase()
                PublishCPAutomaticTunnelStatus()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ApplyTunnelAutomaticMode()
            Try
                Dim comp As Object
                If TunnelAutomaticControlled Then
                    ManualOperateMode = True
                    mnuTunnelAutomaticMode.Text = optDisableTunnelAutomaticModeString ' "DISABLE TUNNEL AUTOMATIC MODE"

                    'FOR ARRC WIERD TUNNEL OPERATIONS
                    If Me.myControlPoint.TunnelAutomaticMode AndAlso Me.myControlPoint.TunnelAutomaticControlled Then
                        For Each comp In Me.AssignedComponenets
                            If comp.GetType.ToString = gSwitchComponentName Then
                                Dim sw As Switch = CType(comp, Switch)
                                'If Not sw.Ctl(1) Is Nothing Then
                                '    sw.Ctl(1).BitState = False
                                'End If
                                If Not sw.Ctl(0) Is Nothing Then
                                    sw.Ctl(0).BitState = False
                                End If
                                'If Not sw.Ctl(3) Is Nothing Then
                                '    sw.Ctl(3).BitState = False
                                'End If
                                'If Not sw.Ctl(4) Is Nothing Then
                                '    sw.Ctl(4).BitState = False
                                'End If
                            End If
                        Next
                    End If
                Else
                    mnuTunnelAutomaticMode.Text = optEnableTunnelAutomaticModeString ' "ENABLE TUNNEL AUTOMATIC MODE"
                    ManualOperateMode = False
                    SendCodeStationControlUpdate()
                    For Each comp In Me.AssignedComponenets
                        If comp.GetType.ToString = gSwitchComponentName Then
                            Dim sw As Switch = CType(comp, Switch)
                            sw.InitSwitchControl(sw.ControlledReverse)
                        End If
                    Next
                End If
                ApplyMonitorMode()
                UpdateSignalControlStatus()
                SendCodeStationControlUpdate()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub UpdateSignalControlStatus()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = gSignalComponentName Then
                        Dim sig As Signal = CType(comp, Signal)
                        If Not IsNothing(sig.myAuthorizationIcon) Then
                            If TunnelAutomaticControlled Then
                                sig.myAuthorizationIcon.Visible = True
                            Else
                                sig.myAuthorizationIcon.Visible = False
                                sig.JointControlAuthorizationCtlBit.BitState = False
                                If Not IsNothing(sig.JointControlAuthorizationCancelBit) Then
                                    sig.JointControlAuthorizationCancelBit.BitState = True
                                End If
                                sig.JointControlAuthorizationUpdateStatus()
                            End If
                        End If
                    End If
                    Select Case comp.GetType.ToString
                        Case gSwitchComponentName, gDeviceComponentName
                            If TunnelAutomaticControlled Then
                                comp.Enabled = False
                            Else
                                comp.Enabled = True
                            End If
                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub UpdateTunnelAutomaticModeInDatabase()
            Dim dbconn As OleDbConnection = Nothing
            Dim dbComm As OleDbCommand = Nothing
            Dim update As New StringBuilder("UPDATE tblCompControlPoints SET TunnelAutomaticControlled = " & CInt(TunnelAutomaticControlled) & " WHERE (UID = " & Me.GUID & ")")
            Try
                dbconn = New OleDbConnection(gConnectionStringStatic)
                dbconn.Open()
                dbComm = New OleDbCommand(update.ToString, dbconn)
                dbComm.ExecuteNonQuery()
                dbconn.Close()
                dbconn = Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
                dbconn = Nothing

            End Try
        End Sub

#End Region

#Region " CUSTOM SNOW MELTER TIMER "


        Private Sub mnuSMZTimer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim MiSm As MenuItem
            Try
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    MessageBox.Show(Me.Name & vbCrLf & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & vbCrLf & sender.text.ToString & " OPERATION IS NOT VALID", "SYSTEM MESSAGE", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    TMDS.SystemUtilities.Utilities.SystemEvent(Me.Name & " IS CURRENTLY IN MANUAL/LOCAL OPERATE MODE.  " & sender.text.ToString & " OPERATION IS NOT VALID " & Me.GUID.ToString)
                    Me.FlasherMode(False)
                    Exit Sub
                End If

                Dim mi As MenuItem = DirectCast(sender, MenuItem)

                If mi IsNot Nothing AndAlso mi.Text.ToUpper.Trim = "STOP SNOW MELTER" Then
                    If Me.SnowMelterStopClickedTime < Now.AddSeconds(-30) Then
                        SnowMelterStopClickedTime = Now
                    Else
                        Me.FlasherMode(False)
                        Exit Sub
                    End If
                Else
                    Me.SnowMelterStopClickedTime = Now.AddSeconds(-40)
                End If



                Dim timerHour As String = mi.Tag
                If timerHour.Trim.ToUpper = "STOP" Then
                    Me.CtlSM.BitState = False
                    mi.Enabled = False
                    For Each MiSm In mnuSMZ.MenuItems
                        If MiSm IsNot mi Then
                            MiSm.Checked = False
                        End If
                    Next
                    StopCustomSnowMelterTimer()
                    mnuSMZStop.Enabled = True

                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = True
                    End If
                    Me.devSMK.DeviceControlled = False
                Else
                    mi.Checked = True
                    For Each MiSm In mnuSMZ.MenuItems
                        If MiSm IsNot mi Then
                            MiSm.Checked = False
                        End If
                    Next
                    mnuSMZStop.Enabled = True
                    Dim Endhour As Double = 0.5
                    Double.TryParse(timerHour, Endhour)
                    StartCustomSnowMelterTimer(Endhour)

                    Me.CtlSM.BitState = True
                    If Not IsNothing(Me.CtlSMOFF) Then
                        Me.CtlSMOFF.BitState = False
                    End If
                    Me.devSMK.DeviceControlled = True

                End If
                Me.SendCodeStationControlSpecial(True)
                Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.devSMK.LogTheDeviceEvent)
                LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceCTL, Nothing, Nothing)


                FlasherMode(False)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub InitializeSnowMelterTimer()
            Dim MiSm As MenuItem
            Try
                If Date.TryParse(Me.CustomSnowMelterEndTime, Nothing) Then
                    Dim tmSpan As TimeSpan
                    tmSpan = CustomSnowMelterEndTime.Subtract(Now)
                    If tmSpan.TotalMinutes > 0 Then
                        SNMZTime = tmSpan.TotalMinutes * 60
                        tmrSnowMelter.Enabled = True
                        mnuSMZStop.Enabled = True

                        Select Case tmSpan.Hours
                            Case 0, 1
                                For Each MiSm In mnuSMZ.MenuItems
                                    If MiSm.Tag = "1" Then
                                        MiSm.Checked = True
                                        Exit For
                                    End If
                                Next
                            Case Is <= SnowMelterMinimumHour
                                For Each MiSm In mnuSMZ.MenuItems
                                    If MiSm.Tag = SnowMelterMinimumHour Then
                                        MiSm.Checked = True
                                        Exit For
                                    End If
                                Next
                            Case Else
                                For Each MiSm In mnuSMZ.MenuItems
                                    If MiSm.Tag = tmSpan.Hours Then
                                        MiSm.Checked = True
                                        Exit For
                                    End If
                                Next
                        End Select
                    End If
                Else
                    CustomSnowMelterEndTime = Now.AddMinutes(-1)
                    SNMZTime = 0
                    mnuSMZStop.Enabled = True


                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Sub StartCustomSnowMelterTimer(ByVal NoOfHours As Double)
            Dim SnMStartTime As Date = Now
            Dim SnMEndTime As Date = Now
            Try
                Me.tmrSnowMelter.Stop()
                Me.tmrSnowMelter.Enabled = False
                SnMEndTime = SnMStartTime.AddHours(NoOfHours)
                'SnMEndTime = SnMStartTime.AddMinutes(2)
                Dim dbUpdate As String = "UPDATE  tblCompControlPoints SET SnowMelterTimeStampOn = '" & SnMEndTime.ToString & "' WHERE UID = " & Me.GUID
                SystemDatabaseCommandToStatic("CONTROLPOINT:CustomSnowMelter", dbUpdate)
                TMDS.SystemUtilities.Utilities.SystemEvent("SNOW MELTER HAS BEEN START TIME @ " & SnMStartTime.ToString & " END TIME " & SnMEndTime.ToString & " FOR CONTROL POINT <" & Me.GUID.ToString.PadLeft(6, "0") & "><" & Me.Name & ">")
                Dim TmSpan As TimeSpan
                TmSpan = SnMEndTime.Subtract(SnMStartTime)

                CustomSnowMelterEndTime = SnMEndTime
                SNMZTime = TmSpan.TotalMinutes * 60
                tmrSnowMelter.Enabled = True
                IsSnowMelterTmrRunning = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub StopCustomSnowMelterTimer()
            Dim SnMEndTime As Date = Now.AddMinutes(-1)
            Try
                Me.tmrSnowMelter.Stop()
                Me.tmrSnowMelter.Enabled = False
                Dim dbUpdate As String = "UPDATE  tblCompControlPoints SET SnowMelterTimeStampOn = '" & SnMEndTime.ToString & "' WHERE UID = " & Me.GUID
                SystemDatabaseCommandToStatic("CONTROLPOINT:CustomSnowMelter", dbUpdate)
                TMDS.SystemUtilities.Utilities.SystemEvent("SNOW MELTER HAS BEEN STOPED BY DISPATCHER(" & gCurrentUser.LastName & "," & gCurrentUser.FirstName & "{" & gCurrentUser.Initials & "} )  END TIME " & CustomSnowMelterEndTime.ToString & " FOR CONTROL POINT <" & Me.GUID.ToString.PadLeft(6, "0") & "><" & Me.Name & ">")

                If IsSnowMelterTmrRunning Then
                    tmrSnowMelter.Stop()
                    tmrSnowMelter.Enabled = False
                Else
                    tmrSnowMelter.Stop()
                    tmrSnowMelter.Enabled = False
                    SNMZTime = Me.SnowMelterTime * 60
                End If
                Me.CtlSM.BitState = False
                If Not IsNothing(Me.CtlSMOFF) Then
                    Me.CtlSMOFF.BitState = True
                End If
                Me.devSMK.DeviceControlled = False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


#End Region

#End Region

#Region "SHADOW MODE"

        ''' <summary>
        ''' Update control point status
        ''' </summary>
        ''' <param name="pSetControl"></param>
        Public Sub UpdateControlPointStatus(pSetControl As Boolean)
            Dim strUpdate As String
            Try
                strUpdate = "UPDATE tblCompControlPoints SET ControllingWorkstation = '" & Me.ControllingWorkstation & "', ControllingWorkStationIPString = '" & Me.ControllingWorkStationIPString & "' WHERE UID = " & Me.myControlPoint.GUID
                SystemDatabaseCommandToStatic("UpdateControlPointStatus", strUpdate)
                SetPrimaryController(pSetControl)
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                strUpdate = Nothing
            End Try
        End Sub

        Public Sub SetShadowModeForComponents()
            Try

                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    comp.IsShadowMode = Me.IsShadowMode
                    comp.IsControlled = Me.IsControlled
                    Select Case comp.GetType.ToString
                        Case gTrackComponentName, gSwitchComponentName, gSignalComponentName, gElectricalSwitchComponentName
                            comp.BuildContextMenu()

                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Friend Sub SetPrimaryController(SetControl As Boolean)
            Dim trk As Track = Nothing
            Try
                If gAssistMode Then Exit Sub
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = gTrackComponentName Then
                        If comp.JointControl Then
                            comp.IsPrimaryController = SetControl
                        Else
                            comp.IsControlled = SetControl
                        End If
                    End If
                Next
                Me.IsPrimaryController = SetControl
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub DoReleaseControl(message As String)
            Try
                Dim data() As String = message.Split("|")
                'Dim Message As String = UserName & "|REQUESTING CONTROL OF|" & CPName & "|FROM WORKSTATION|" & Workstation
                'Me.ControllingWorkstation = String.Empty
                Me.ControllingWorkStationIPString = "NONE"
                Me.IsControlled = False
                Me.IsShadowMode = True

                Me.UpdateControlPointStatus(False)
                SetShadowModeForComponents()

                TMDS.NetworkMessages.CadMessages.SendControlAllowedForControlPoint(Me, data(4), gCadTransmitQueue)
                Me.UpdateStatus()
                Me.BuildContextMenu()
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub DoAllowControl()
            Try
                Me.IsShadowMode = False
                Me.IsControlled = True
                'Me.ControllingWorkstation = gWorkStationName
                Me.ControllingWorkStationIPString = gThisWorkstation.IpAddress
                Me.UpdateControlPointStatus(True)
                SetShadowModeForComponents()
                Me.UpdateStatus()
                Me.BuildContextMenu()
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Function IsControlPointValidForTransfer() As Boolean
            Dim Comp As Object = Nothing
            Try
                If Not optEnableDenverSpecificOptions Then Return True
                For Each Comp In Me.AssignedComponenets
                    If Comp IsNot Nothing Then
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                If Comp.TrackAuthorities IsNot Nothing AndAlso Comp.TrackAuthorities.Count > 0 Then
                                    Return False
                                ElseIf Comp.OtherSplAuthorities IsNot Nothing AndAlso Comp.OtherSplAuthorities.Count > 0 Then
                                    Return False
                                ElseIf Comp.DeviceBlocks IsNot Nothing AndAlso Comp.DeviceBlocks.Count > 0 Then
                                    Return False
                                ElseIf Comp.BlockAbsolute Then
                                    Return False
                                ElseIf Comp.TrackAndTime Then
                                    Return False
                                End If
                            Case gSwitchComponentName
                                If Comp.BlockAbsolute Then
                                    Return False
                                ElseIf Comp.DeviceBlocked Then
                                    Return False
                                End If
                            Case gSignalComponentName
                                If Comp.BlockAbsolute Then
                                    Return False
                                ElseIf Comp.DeviceBlocked Then
                                    Return False
                                End If
                        End Select
                    End If
                Next
                Return True
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Function

#End Region

#Region "MAINTENANCE MODE"

        Public Sub ProcessReleaseControlRequest() ' this is processing the command from maintenance terminal to return control to dispatcher
            Try
                MaintenanceMode = False
                Me.ApplyMonitorMode()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ProcessControlPermission() 'this is processing command from dispatcher to take control at maintenance terminal
            Try

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Remove block all track comps
        ''' </summary>
        Public Sub RemoveBlockAllTrackComponents()
            Dim deviceBlk As TrackAuthority.DeviceBlock = Nothing
            Dim trackNames As String = Nothing
            Dim firtStationName As String = Nothing
            Dim summItem As SummaryItem
            Try
                For Each trackblock As TrackAuthority.DeviceBlock In gDeviceBlocks
                    If trackblock.BlockGUID = Me.MaintenanceModeRemoteUID Then
                        deviceBlk = trackblock
                        Exit For
                    End If
                Next
                If deviceBlk Is Nothing Then
                    SystemEvent("AddBlockAllTrackComponents:Can't add device block. ControlPointName=" & Me.Name)
                    Exit Sub
                End If
                If deviceBlk.RemoveDeviceBlockFromDB() Then
                    deviceBlk.RemoveDeviceBlockFromRemoteLinks()
                    If deviceBlk.RemoveDeviceBlockFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        gDeviceBlocks.Remove(deviceBlk)
                        summItem = New SummaryItem("BLOCK", "REMOVE", deviceBlk)
                        SyncLock gSummaryBlockQueue.SyncRoot
                            gSummaryBlockQueue.Enqueue(summItem)
                        End SyncLock
                        TMDS.NetworkMessages.CadMessages.SendTrackBlockEventForWabtecOne(deviceBlk, "DELETED")
                    End If
                Else
                    SystemEvent("AddBlockAllTrackComponents:Can´t add device block. ControlPointName=" & Me.Name)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                deviceBlk = Nothing
                trackNames = Nothing
                firtStationName = Nothing
                summItem = Nothing
            End Try
        End Sub

        ''' <summary>
        ''' Add Block in All track Components
        ''' </summary>
        Public Sub AddBlockAllTrackComponents()
            Dim trk As Track
            Dim deviceBlk As New TrackAuthority.DeviceBlock()
            Dim territory As Territory
            Dim summItem As SummaryItem
            Try
                deviceBlk.AuthorityLimits = "MAINTENANCE MODE DEVICE BLOCK"
                deviceBlk.isRemoteDeviceBlock = False
                deviceBlk.isMaintenanceMode = True
                deviceBlk.BlockNotes = "MODO MANUTENCAO"
                deviceBlk.OKTime = Now
                deviceBlk.IssueTo = "MODO MANUTENCAO"
                deviceBlk.RailRoadName = "MRS"
                deviceBlk.DispatcherName = gCurrentUser.UserFullName
                deviceBlk.AuthorityDate = Now
                territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(Me.myTerritory.TerritoryID)
                deviceBlk.AuthorityNumber = territory.GetNextAuthorityNumber(False, True)
                deviceBlk.Direction = "NONE"
                deviceBlk.AuthorityType = "DEVICE BLOCK"
                deviceBlk.EnableTimeExpired = False
                deviceBlk.OverrideFieldBlocking = False
                deviceBlk.CreateDate = Date.Now
                deviceBlk.AuthorityUID = Me.myTerritory.TerritoryID.ToString & " - " & deviceBlk.AuthorityNumber
                deviceBlk.TerritoryAssignment = Me.myTerritory.TerritoryID
                deviceBlk.BlockGUID = Me.MaintenanceModeRemoteUID

                For Each comp As Object In Me.AssignedComponenets
                    If comp.GetType.ToString = gTrackComponentName Then
                        deviceBlk.ComponentsInSelection.Add(comp)
                    End If
                Next

                For Each comp As Object In Me.AssignedComponenets
                    If comp.GetType.ToString = gTrackComponentName Then
                        trk = CType(comp, Track)
                        trk.DeviceBlocks.Add(deviceBlk)
                        trk.BlockAbsolute = True
                        trk.DeviceBlocked = True
                        trk.UpdateStatus(deviceBlk)
                    End If
                Next

                If Not deviceBlk.AddDeviceBlockToDB() Then
                    SystemEvent("AddBlockAllTrackComponents:Can´t add device block. ControlPointName=" & Me.Name)
                    Exit Sub
                End If

                If Not gDeviceBlocks.Contains(deviceBlk) Then
                    gDeviceBlocks.Add(deviceBlk)
                End If

                summItem = New SummaryItem("BLOCK", "ADD", deviceBlk)
                SyncLock gSummaryBlockQueue.SyncRoot
                    gSummaryBlockQueue.Enqueue(summItem)
                End SyncLock

                SystemEvent("AddBlockAllTrackComponents:ControlPointName=" & Me.Name)

            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                trk = Nothing
                deviceBlk = Nothing
                territory = Nothing
                summItem = Nothing
            End Try
        End Sub

        ''' <summary>
        ''' Send Control Permission From Request
        ''' </summary>
        ''' <param name="pStatus"></param>
        Public Sub SendControlPermissionFromRequest(ByVal pStatus As String, Optional pExpiredTime As String = Nothing)
            Try
                SystemEvent("SendControlPermissionFromRequest:Status=" & pStatus)
                TMDS.NetworkMessages.MaintenanceModeMessages.SendControlPermissionFromRequest(Me, pStatus)
                If pStatus.Contains("NOK") Then
                    Exit Sub
                End If
                MaintenanceMode = True
                Me.ApplyMonitorMode()
                Me.UpdateStatus()
                Me.ControllingWorkstation = "MAINTENANCE MODE" & "|" & DateTime.Now.ToString("MMddyy HHmmss") & "|" & CDate(pExpiredTime).ToString("MMddyy HHmmss")
                Me.UpdateControlPointStatus(False)
                Me.AddBlockAllTrackComponents()
                For Each comp As Object In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        SystemEvent("SendControlPermissionFromRequest:Switch=" & comp.Name & ":DoMonitorMode=" & comp.DoMonitorMode)
                    End If
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        SystemEvent("SendControlPermissionFromRequest:Signal=" & comp.Name & ":DoMonitorMode=" & comp.DoMonitorMode)
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendRequestForMaintenanceControl()
            TMDS.NetworkMessages.MaintenanceModeMessages.SendRequestForMaintenanceControl(Me)
        End Sub

        Public Sub SendRequestForMaintenanceRelease()
            Try
                TMDS.NetworkMessages.MaintenanceModeMessages.SendRequestForMaintenanceRelease(Me)

                Me.mnuMaintenanceModeRequest.Enabled = True
                Me.mnuMaintenanceModeRelease.Enabled = False
            Catch EX As Exception
                SystemException(EX.ToString)
            End Try
        End Sub

#End Region

#Region "MOUSE EVENTS"

        Public Sub RefreshContextMenu()
            Try
                If Not IsNothing(mnuAutoRouting) Then
                    If Me.AllowAutomaticMode Then
                        If Me.AutomaticModeOn Then
                            mnuAutoRouting.Text = "DISABLE AUTO-ROUTING"
                        Else
                            mnuAutoRouting.Text = "ENABLE AUTO-ROUTING"
                        End If
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlPoint_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
            Try
                If gPermissionPassRedSignalSwitchSelectionMode Then
                    Exit Sub
                End If
                If gTrainerMode Then
                    Exit Sub
                ElseIf gAssistMode OrElse Me.IsShadowMode Then
                    Exit Sub
                End If
                If e.Button = Windows.Forms.MouseButtons.Right And Me.AuthorityType = "CTC" Then  'DTP-236
                    FlasherMode(True)
                End If
                RefreshContextMenu()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlPoint_MouseEnter(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = True
            MouseLeft = False
            Refresh()
        End Sub

        Public Sub ControlPoint_MouseLeave(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = False
            MouseLeft = True
            Refresh()
        End Sub

        Public Sub ControlPoint_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Dim RightTrack As Track = Nothing
            Dim LeftTrack As Track = Nothing
            Try
                If Me.StoredRoutes.Count > 0 Then
                    Dim sr As StoredRoute
                    For Each sr In Me.StoredRoutes
                        RightTrack = Nothing
                        LeftTrack = Nothing
                        Select Case sr.EntrySignal.ComponentType
                            Case "L", "U"
                                If optEnableStoreRouteToolTipForBRC Then
                                    ToolTipMessage = ToolTipMessage & optDirectionNorth
                                Else
                                    If InStr(Mid(sr.EntrySignal.Name, 1, 2), "S") <> 0 Then
                                        ToolTipMessage = ToolTipMessage & optDirectionSouth
                                    ElseIf InStr(Mid(sr.EntrySignal.Name, 1, 2), "W") <> 0 Then
                                        ToolTipMessage = ToolTipMessage & optDirectionWest
                                    ElseIf InStr(Mid(sr.EntrySignal.Name, 1, 3), "N") <> 0 And optFlipNorthSouthDirection Then
                                        ToolTipMessage = ToolTipMessage & optDirectionSouth
                                    Else
                                        Exit Sub
                                    End If
                                End If
                                RightTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.EntrySignal.RightLink)
                                LeftTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.ExitSignal.LeftLink)
                                If Not IsNothing(RightTrack) Then
                                    ToolTipMessage = ToolTipMessage & " (" & RightTrack.TrackName
                                Else
                                    RightTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.EntrySignal.LeftLink)
                                    If Not IsNothing(RightTrack) Then
                                        ToolTipMessage = ToolTipMessage & " (" & RightTrack.TrackName
                                    Else
                                        Exit Sub
                                    End If

                                End If

                                If Not IsNothing(LeftTrack) Then
                                    ToolTipMessage = ToolTipMessage & " TO " & LeftTrack.TrackName & ")"
                                Else
                                    LeftTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.ExitSignal.RightLink)

                                    If Not IsNothing(LeftTrack) Then
                                        ToolTipMessage = ToolTipMessage & " TO " & LeftTrack.TrackName & ")"
                                    Else
                                        Exit Sub
                                    End If
                                End If

                            Case "R", "D"
                                If optEnableStoreRouteToolTipForBRC Then
                                    ToolTipMessage = ToolTipMessage & optDirectionSouth
                                Else
                                    If InStr(Mid(sr.EntrySignal.Name, 1, 2), "N") <> 0 Then
                                        ToolTipMessage = ToolTipMessage & optDirectionNorth
                                    ElseIf InStr(Mid(sr.EntrySignal.Name, 1, 2), "E") <> 0 Then
                                        ToolTipMessage = ToolTipMessage & optDirectionEast
                                    ElseIf InStr(Mid(sr.EntrySignal.Name, 1, 3), "S") <> 0 And optFlipNorthSouthDirection Then
                                        ToolTipMessage = ToolTipMessage & optDirectionNorth
                                    Else
                                        Exit Sub
                                    End If
                                End If

                                LeftTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.EntrySignal.LeftLink)
                                RightTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.ExitSignal.RightLink)
                                If Not IsNothing(LeftTrack) Then
                                    ToolTipMessage = ToolTipMessage & " (" & LeftTrack.TrackName
                                Else
                                    LeftTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.EntrySignal.RightLink)
                                    If Not IsNothing(LeftTrack) Then
                                        ToolTipMessage = ToolTipMessage & " (" & LeftTrack.TrackName
                                    Else
                                        Exit Sub
                                    End If
                                End If

                                If Not IsNothing(RightTrack) Then
                                    ToolTipMessage = ToolTipMessage & " TO " & RightTrack.TrackName & ")"
                                Else
                                    RightTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sr.ExitSignal.LeftLink)
                                    If Not IsNothing(RightTrack) Then
                                        ToolTipMessage = ToolTipMessage & " TO " & RightTrack.TrackName & ")"
                                    Else
                                        Exit Sub
                                    End If
                                End If

                        End Select


                        'ToolTipMessage = ToolTipMessage & "(" & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & ") "
                        ToolTipMessage = ToolTipMessage & vbCrLf
                    Next
                End If

                If ToolTipMessage = "" Then
                    ToolTipMessage = ToolTipMessage & Me.Name.ToUpper
                End If
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "TRAINER/TRAINEE MODE"

        Public Overloads Sub SendTrainerCommand(ByVal Command As String)
            Try
                Dim Message As String = "TRC" & Chr(6) & "TRAINERCOMMAND;"
                Message = Message & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                Message = Message & "|CP"
                Message = Message & "|" & Command
                Message = Me.TerritoryID.ToString & Chr(12) & Message
                TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendSimulatedIndication()
            Try
                Dim myCodeStations As New ArrayList()
                Dim stat As CodeStation

                For Each stat In Me.CodeStationList
                    If stat.ControlPointNumber = Me.StationNumber Then
                        myCodeStations.Add(stat)
                    End If
                Next

                For Each stat In myCodeStations
                    Dim Message As String = "TEC" & Chr(6) & "IND;"
                    Message = Message & Me.TerritoryID.ToString
                    Message = Message & "|" & Me.AssembleIndicationMessage(stat)
                    Message = Me.TerritoryID.ToString & Chr(12) & Message
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendSimulatedFailStation()
            Try
                Dim stat As CodeStation
                Dim myCodeStations As New ArrayList()

                For Each stat In Me.CodeStationList
                    If stat.ControlPointNumber = Me.StationNumber Then
                        myCodeStations.Add(stat)
                    End If
                Next
                For Each stat In myCodeStations
                    Dim Message As String = "TEC" & Chr(6) & "IND;"
                    Message = Message & Me.TerritoryID.ToString
                    Message = Message & "|" & Me.AssembleFailureMessage(stat)
                    Message = Me.TerritoryID.ToString & Chr(12) & Message
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub UpdateSimulatedComponentStatus()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    comp.FailState = False
                    comp.UpdateStatus()
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function AssembleFailureMessage(ByVal cs As CodeStation) As String
            Try

                Dim FailHeader As String = String.Empty
                Dim IndMess As String = String.Empty
                Dim FailMessage As String = String.Empty

                FailHeader = "FAILSTATION=" & Me.CodelineNumber.ToString
                FailHeader = FailHeader & ":" & Me.GUID.ToString
                FailHeader = FailHeader & ":" & cs.Number.ToString.PadLeft(2, "0")
                FailMessage = FailHeader
                Return FailMessage
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

#End Region

#Region "GRAPHICS"

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                If Me.MakeInvisible Then Exit Sub
                g = Graphics.FromImage(bp)
                Dim AlphaFont As Font = New Font("Courier", 13.5!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))

                Dim thisStationBox As RectangleF = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height / 2), pStationBoxSize)
                g.DrawString(Name, AlphaFont, pStationBrush, thisStationBox, pStringFormat)
                g.Dispose()
                g = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                g = Nothing
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                If Me.MakeInvisible Then Exit Sub
                'If CurrentDrawingForm.Initializing Then Exit Sub
                If optEnableTractionPowerManagementSystem Then
                    Try
                        If ScadaTractionPowerControlPointMode Then
                            e.Graphics.DrawPath(New Pen(Color.Gray, 1), Path)
                            e.Graphics.DrawPath(New Pen(Color.Gray, 1), TractionCPCommPath)
                            e.Graphics.DrawPath(New Pen(Color.Gray, 1), TractionCPStatePath(1))
                            e.Graphics.DrawString(Name, StationFont, New SolidBrush(Color.SteelBlue), 35, 12, pStringFormat)
                            Exit Sub
                        ElseIf ScadaCommunicationControlPointMode Then
                            e.Graphics.DrawPath(New Pen(Color.Gray, 1), Path)
                            e.Graphics.DrawString(Name, StationFont, New SolidBrush(Color.SteelBlue), 35, 12, pStringFormat)
                            Exit Sub
                        End If
                    Catch ex As Exception
                        SystemException(ex.ToString)
                    End Try
                End If

                Try
                    If FlashComponent Then
                        If FlashOn Then
                            e.Graphics.DrawString(Name, pStationFont, pStationBrushOff, pStationBox, pStringFormat)
                        ElseIf MouseEntered Then
                            e.Graphics.DrawPath(New Pen(pStationBrush, 1), Path)
                            e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                        ElseIf MouseLeft Then
                            If Me.Indicating Then
                                e.Graphics.DrawString(Name, pStationFont, New SolidBrush(Color.Red), pStationBox, pStringFormat)
                            Else
                                If Me.AllowAutomaticMode And Me.AutomaticModeOn And Not Me.FailState Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushAutoMode, pStationBox, pStringFormat)
                                ElseIf MaintenanceMode Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushMaintenanceMode, pStationBox, pStringFormat)
                                Else
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                                End If
                            End If
                        Else
                            e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                        End If
                    Else

                        If MouseEntered Then
                            e.Graphics.DrawPath(New Pen(pStationBrush, 1), Path)
                            e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                        ElseIf MouseLeft Then
                            If Me.Indicating Then
                                e.Graphics.DrawString(Name, pStationFont, New SolidBrush(Color.Red), pStationBox, pStringFormat)
                            Else
                                If Me.AllowAutomaticMode And Me.AutomaticModeOn And Not Me.FailState Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushAutoMode, pStationBox, pStringFormat)
                                ElseIf MaintenanceMode Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushMaintenanceMode, pStationBox, pStringFormat)
                                Else
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                                End If
                            End If
                        Else
                            If Me.Indicating Then
                                e.Graphics.DrawString(Name, pStationFont, New SolidBrush(Color.Red), pStationBox, pStringFormat)
                            Else
                                If Me.AllowAutomaticMode And Me.AutomaticModeOn And Not Me.FailState Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushAutoMode, pStationBox, pStringFormat)
                                ElseIf MaintenanceMode Then
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrushMaintenanceMode, pStationBox, pStringFormat)
                                Else
                                    e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)
                                End If
                            End If
                        End If

                    End If
                Catch ex As Exception
                    SystemException(ex.ToString)
                End Try
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Overloads Sub UpdateStatus(ByVal RefreshMe As Boolean)

            Select Case Mode
                Case ComponentMode.Regular
                    Me.ForeColor = Color.Khaki
                Case ComponentMode.Selected
                    Me.ForeColor = Color.WhiteSmoke
            End Select

            If Me.Visible Then
                Me.Refresh()
            End If

        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()

            Me.Left = LX - pStringSize.Width / 2
            Me.Top = LY - pStringSize.Height / 2

        End Sub

        Overrides Sub RefreshPath()

            If optEnableTractionPowerManagementSystem Then
                Try
                    If ScadaTractionPowerControlPointMode Or ScadaCommunicationControlPointMode Then
                        RefreshPathTPMS()
                        Exit Sub
                    End If
                Catch ex As Exception
                    SystemException(ex.ToString)
                End Try
            End If

            Path = New GraphicsPath()
            Dim FontLength As Integer

            Try

                If TMDS.GlobalDeclarations.optFontControlPointMedium Then
                    pStationFont = New Font("ARIAL", 11.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    FontLength = 150
                    FontLength = 200
                ElseIf TMDS.GlobalDeclarations.optFontControlPointSmall Then
                    pStationFont = New Font("ARIAL", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    FontLength = 130
                ElseIf TMDS.GlobalDeclarations.optFontControlPointLarge Then
                    pStationFont = New Font("ARIAL", 12.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    FontLength = 160
                Else
                    pStationFont = New Font("ARIAL", 13.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    FontLength = 175
                End If

                Dim g As Graphics = Me.CreateGraphics
                pStringFormat = New StringFormat()
                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Center
                'pStringSize = g.MeasureString(Name, StationFont, 100)
                pStringSize = g.MeasureString(Name, StationFont, FontLength)

                pStationBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                pStationBox = New RectangleF(New PointF(0.0, 0.0), pStationBoxSize)
                Path.AddRectangle(pStationBox)
                'Path.AddString(Name pStationFont, pStationBrushOff, pStationBox, pStringFormat)
                Me.Left = LX - pStringSize.Width / 2
                Me.Top = LY - pStringSize.Height / 2
                Me.Width = pStringSize.Width + 2
                Me.Height = pStringSize.Height + 2

                'build paths for bitmap imaging
                Me.GraphicalPathRelative = Path.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.LX, Me.LY)
                GraphicalPathRelative.Transform(mat)


                Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RefreshPathTPMS()
            Path = New GraphicsPath()
            Try
                If ScadaTractionPowerControlPointMode Then

                    TractionCPCommPath = New GraphicsPath
                    TractionCPStatePath = New ArrayList

                    Dim pt1 As Point
                    Dim pt2 As Point
                    Dim pt3 As Point
                    Dim pt4 As Point

                    'RECTANGLE GRAPHICS 
                    Path.StartFigure()
                    pt1 = New Point(0, 0)
                    pt2 = New Point(75, 0)
                    pt3 = New Point(75, 59)
                    pt4 = New Point(0, 59)
                    Path.AddLine(pt1, pt2)
                    Path.AddLine(pt2, pt3)
                    Path.AddLine(pt3, pt4)
                    Path.AddLine(pt4, pt1)
                    Path.CloseFigure()

                    'LIGHTNING GRAPHICS PART1
                    TractionCPCommPath.StartFigure()
                    pt1 = New Point(35, 36)
                    pt2 = New Point(33, 40)
                    pt3 = New Point(48, 44)
                    TractionCPCommPath.AddLine(pt1, pt2)
                    TractionCPCommPath.AddLine(pt2, pt3)
                    TractionCPCommPath.AddLine(pt3, pt1)
                    TractionCPCommPath.CloseFigure()

                    'LIGHTNING GRAPHICS PART2
                    TractionCPCommPath.StartFigure()
                    pt1 = New Point(36, 36)
                    pt2 = New Point(39, 33)
                    pt3 = New Point(24, 28)
                    TractionCPCommPath.AddLine(pt1, pt2)
                    TractionCPCommPath.AddLine(pt2, pt3)
                    TractionCPCommPath.AddLine(pt3, pt1)
                    TractionCPCommPath.CloseFigure()

                    'CIRCLE GRAPHICS1
                    Dim CrossedCircle As New GraphicsPath
                    CrossedCircle.StartFigure()
                    Dim rect2 As New Rectangle(20, 23, 30, 28)
                    CrossedCircle.AddArc(rect2, -30, 180)
                    CrossedCircle.AddArc(rect2, -30, -180)
                    CrossedCircle.CloseFigure()
                    TractionCPStatePath.Add(CrossedCircle)

                    'CIRCLE GRAPHICS2
                    Dim ClosedCircle As New GraphicsPath
                    ClosedCircle.StartFigure()
                    Dim rect3 As New Rectangle(20, 23, 30, 28)
                    ClosedCircle.AddArc(rect2, -30, 360)
                    ClosedCircle.CloseFigure()
                    TractionCPStatePath.Add(ClosedCircle)

                    Dim SmallSquare As New GraphicsPath
                    SmallSquare.StartFigure()
                    pt1 = New Point(17, 21)
                    pt2 = New Point(53, 21)
                    pt3 = New Point(53, 53)
                    pt4 = New Point(17, 53)
                    SmallSquare.AddLine(pt1, pt2)
                    SmallSquare.AddLine(pt2, pt3)
                    SmallSquare.AddLine(pt3, pt4)
                    SmallSquare.AddLine(pt4, pt1)
                    SmallSquare.CloseFigure()
                    TractionCPStatePath.Add(SmallSquare)


                    'TEXT GRAPHICS
                    Dim g As Graphics = Me.CreateGraphics
                    pStringFormat = New StringFormat()
                    StationFont = New Font("Arial", 11.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    pStringFormat.Alignment = StringAlignment.Center
                    pStringFormat.LineAlignment = StringAlignment.Center

                    Me.Width = 77
                    Me.Height = 60
                ElseIf ScadaCommunicationControlPointMode Then

                    TractionCPCommPath = New GraphicsPath
                    TractionCPStatePath = New ArrayList

                    Dim pt1 As Point
                    Dim pt2 As Point
                    Dim pt3 As Point
                    Dim pt4 As Point

                    'RECTANGLE GRAPHICS 
                    Path.StartFigure()
                    pt1 = New Point(0, 0)
                    pt2 = New Point(75, 0)
                    pt3 = New Point(75, 22)
                    pt4 = New Point(0, 22)
                    Path.AddLine(pt1, pt2)
                    Path.AddLine(pt2, pt3)
                    Path.AddLine(pt3, pt4)
                    Path.AddLine(pt4, pt1)
                    Path.CloseFigure()

                    'TEXT GRAPHICS
                    Dim g As Graphics = Me.CreateGraphics
                    pStringFormat = New StringFormat()
                    StationFont = New Font("Arial", 11.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    pStringFormat.Alignment = StringAlignment.Center
                    pStringFormat.LineAlignment = StringAlignment.Center


                    Me.Width = 77
                    Me.Height = 24
                Else
                    Dim g As Graphics = Me.CreateGraphics
                    pStringFormat = New StringFormat()
                    StationFont = New Font("Arial", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    'StationFont = New Font("COURIER", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
                    pStringFormat.Alignment = StringAlignment.Center
                    pStringFormat.LineAlignment = StringAlignment.Center
                    pStringSize = g.MeasureString(Name, StationFont, 130)
                    pStationBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                    pStationBox = New RectangleF(New PointF(0.0, 0.0), pStationBoxSize)
                    Path.AddRectangle(pStationBox)
                    Me.Left = LX - pStringSize.Width / 2
                    Me.Top = LY - pStringSize.Height / 2
                    Me.Width = pStringSize.Width + 2
                    Me.Height = pStringSize.Height + 2
                    'Me.Region = New Region(Path)
                    Me.BringToFront()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub FlashElapsedEvent(ByVal source As Object, ByVal e As EventArgs)

            If FlashOn Then
                FlashOn = False
            Else
                FlashOn = True
            End If
            Flasher.Enabled = True
            Refresh()
        End Sub

        Private Sub SnowMelterCountDown(ByVal source As Object, ByVal e As EventArgs)
            SNMZTime = SNMZTime - 1
            If SNMZTime = 0 Then
                If optEnableCustomSnowMelterTimer Then
                    Me.mnuSMZStop.PerformClick()
                Else
                    Me.mnuSMZ.PerformClick()
                End If

            End If

        End Sub

        Public Overrides Sub FlasherMode(ByVal EnableFlasher As Boolean, Optional ByVal Fake As Boolean = False)
            Try
                If EnableFlasher Then
                    CurrentlySelected = True
                Else
                    CurrentlySelected = False
                End If
                Me.FlashComponent = EnableFlasher
                Me.FlashOn = EnableFlasher
                Me.Flasher.Enabled = EnableFlasher
                Me.PendingStatusChange = EnableFlasher
                Me.CurrentDrawingForm.ReSetComponentFlash()
                Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            Try
                If Me.MakeInvisible Then Exit Sub
                Me.UpdateStatus()
                Refresh()
            Catch ex As Exception

            End Try

        End Sub

        Overrides Sub BuildGraphics()
            Try
                InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                CreateDeviceComponents()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub UpdateStatus()
            Try
                If optEnableShadowMode Then
                    If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) Then
                        Me.pStationBrush = gColorFailedState
                    Else
                        If Me.IsControlled Then
                            Me.pStationBrush = New SolidBrush(Color.LightGreen)
                            If mnuRESEND IsNot Nothing Then mnuRESEND.Enabled = True
                        Else
                            Me.pStationBrush = New SolidBrush(Color.LightSalmon)
                            If mnuRESEND IsNot Nothing Then mnuRESEND.Enabled = False
                        End If
                    End If
                Else
                    If Me.AuthorityType = "CTC" AndAlso Not gAssistMode Then
                        If Me.FailState And (Not Me.SimulateMode Or gTraineeMode) Then
                            Me.pStationBrush = gColorFailedState
                        Else
                            Me.pStationBrush = gSystemStationColor
                            If mnuRESEND IsNot Nothing Then mnuRESEND.Enabled = True
                        End If
                    Else
                        Me.pStationBrush = gSystemStationColor
                    End If
                End If


                If optEnableCodeFailureMiscDevice Then
                    If Not Me.FailState Then
                        Me.devCFK.DeviceState = False
                        Me.devCFK.UpdateStatus()
                    End If
                End If
                If gInitialized Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "FORCE TWC OPERATIONS CHANGE"

        Public Sub InitializeForceOperationsChangeToTWC()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        sig.MakeInvisible = True
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub
#End Region

#Region "AUTOROUTING"

        Public Sub InitializeAutoRouting()
            Try
                If (Me.myTerritory) Is Nothing Then
                    ToggleAutoRouting(False, "INIT")
                    Exit Sub
                End If

                If Me.myTerritory.AllowMovementPlanning And gThisWorkstation.AllowMovementPlanning Then
                    ToggleAutoRouting(Me.AutomaticModeOn, "INIT")
                Else
                    ToggleAutoRouting(False, "INIT")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ToggleAutoRouting(ByVal AutoOn As Boolean, ByVal OriginOfCommand As String, Optional ByVal ForceStoredRouteRemoval As Boolean = False)
            Try
                AutomaticModeOn = AutoOn
                If Not AutomaticModeOn Then
                    If Me.StoredRoutes.Count > 0 And Not ForceStoredRouteRemoval Then
                        Dim LaunchPoint As New Point(Me.Location.X, Me.Location.Y + 10)
                        Dim frmMessageBox As New frmMessageBox()
                        Dim MessageList As New ArrayList()
                        Dim MessageString As String = "Do you wish to remove all stored routes for this control point?"
                        MessageList.Add(MessageString.ToUpper)
                        MessageList.Add("")
                        frmMessageBox.SetMessage(MessageList, "STORED ROUTE CHECK", True, LaunchPoint)
                        frmMessageBox.ShowDialog()
                        If frmMessageBox.FormFeedBack = 6 Then
                            CancelStoredRoutes()
                        End If
                        frmMessageBox.Dispose()
                        frmMessageBox = Nothing
                    ElseIf Me.StoredRoutes.Count > 0 And ForceStoredRouteRemoval Then
                        CancelStoredRoutes()
                    End If
                End If

                Dim Keyword As String = "DISABLE"
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        sig.ToggleAutoRouting(AutoOn, OriginOfCommand)
                    End If
                Next
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If
                Dim dbUpdate As String = "UPDATE  tblCompControlPoints SET AutomaticModeOn = " & CInt(AutoOn) & " WHERE UID = " & Me.GUID
                SystemDatabaseCommandToStatic("CONTROLPOINT:ToggleAutoRouting", dbUpdate)
                TMDS.SystemUtilities.Utilities.SystemEvent("AUTO-ROUTING HAS BEEN " & Keyword & " FROM THE " & OriginOfCommand & " FOR CONTROL POINT <" & Me.GUID.ToString.PadLeft(6, "0") & "><" & Me.Name & ">")

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "TEMPORAL MODE OPERATIONS"

        Public Sub TemporalControlMode(ByVal ControlModeOn As Boolean)
            Try
                If ControlModeOn Then
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = True
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If
                Else
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = True
                    End If
                End If
                Me.SendCodeStationControlSpecial(True)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

        Private Delegate Sub DelayInitialization()

        Public Sub IniitializeTunnnelMode()
            Try

                If TunnelAutomaticControlled Then
                    If Not Me.Ctl(6) Is Nothing Then
                        Me.Ctl(6).BitState = True
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = False
                    End If
                Else
                    If Not Me.Ctl(6) Is Nothing Then
                        Me.Ctl(6).BitState = False
                    End If
                    If Not Me.Ctl(5) Is Nothing Then
                        Me.Ctl(5).BitState = True
                    End If
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub DoInitialization()
            Try
                If Me.StoredRoutes.Count > 0 Then
                    Dim Initialize As New DelayInitialization(AddressOf PerformInitialization)
                    Initialize.BeginInvoke(Nothing, Nothing)
                Else
                    Initialized = True
                    If Not gSendCTC Then SendCodeStationControlUpdate()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub PerformInitialization()
            Try
                Threading.Thread.Sleep(15000)
                Initialized = True
                SendCodeStationControlUpdate()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ApplyMonitorModeFromDevice(ByVal dv As Device, ByVal DeviceIndicated As Boolean)
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If Not comp.IndicateOnly Then
                        If Not IsNothing(comp.Ind(0)) Then
                            If comp.Ind(0).CodeStation = dv.Ind(0).CodeStation Then
                                comp.DoMonitorMode = DeviceIndicated
                            End If
                        End If
                    End If
                Next
                If DeviceIndicated Then
                    If optEnableTrainStopOnPowerOffManualOperateCheck Then
                        CheckToStopTrainOnOManualOperate(dv)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Apply monitor mode
        ''' </summary>
        Public Sub ApplyMonitorMode()
            Dim comp As Object
            Try
                If TunnelAutomaticControlled Then
                    ManualOperateMode = True
                End If

                If gMonitorMode Then
                    ManualOperateMode = True
                End If

                If Not MaintenanceMode Then
                    For Each comp In Me.AssignedComponenets
                        If Not comp.IndicateOnly Then
                            comp.DoMonitorMode = ManualOperateMode
                        End If
                    Next
                ElseIf MaintenanceMode Then
                    For Each comp In Me.AssignedComponenets
                        If Not comp.IndicateOnly Then
                            comp.DoMonitorMode = MaintenanceMode
                        End If
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                comp = Nothing
            End Try
        End Sub

        Public Sub InitializeSignalRoutesOverFailedLink()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        If sig.SignalIndStatus = Signal.IndStatus.Cleared And Not sig.DrillMode Then
                            sig.ControlSignalToClear(False, False, False, False, False, False, True)
                            sig.ControlNotYetCommitted = False
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub CheckToShowOutOfFailure(ByVal StationNumber As Integer)
            Try
                Dim cdstat As CodeStation

                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Then
                        If cdstat.Number = StationNumber Then
                            cdstat.FailState = False
                        End If
                    End If
                Next
                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Then
                        If cdstat.FailState Then
                            Exit Sub
                        End If
                    End If
                Next
                Me.FailState = False
                Me.CommFail = False
                PublishStatusesForAllComponents()
            Catch ex As Exception
                SystemException(ex.ToString)

            End Try
        End Sub

        Public Sub PublishStatusesForAllComponents()
            Try
                Dim comp As Object
                If Not Me.AllowStatusPublishing Then Exit Sub

                For Each comp In Me.AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.TrackComponents.Track", "TMDS.SwitchComponents.Switch", "TMDS.SignalComponents.Signal"
                            comp.PublishStatus(True)
                    End Select
                Next
                Me.PublishStatus(True)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForAllStationInitialization()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString <> "TMDS.GraphicComponent.ControlPoint" Then
                        If comp.FailState Then
                            If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(comp, Track)
                                If Not IsNothing(trk.Ind(0)) Then
                                    If trk.Ind(0).ControlPoint = Me.GUID Then
                                        Exit Sub
                                    End If
                                End If
                                If Not IsNothing(trk.Ind(1)) Then
                                    If trk.Ind(1).ControlPoint = Me.GUID Then
                                        Exit Sub
                                    End If
                                End If
                            Else
                                Exit Sub
                            End If
                        End If
                    End If
                Next

                If Me.ContainsMultipleStations Then
                    Me.SendRecall()
                Else
                    Me.Initialized = True
                End If
                Me.AllStationsInitialized = True

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitializeSignalRoutes()
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        If sig.SignalIndStatus = Signal.IndStatus.Cleared And Not sig.DrillMode Then
                            'sig.ApplyIndicationChange()
                            'sig.ClearSignalRouteForInitialization()
                            sig.NoSignalControlInitialization = True
                            sig.ControlSignalToClear(False, False, False, False, False, False)
                            sig.ControlNotYetCommitted = False
                            sig.NoSignalControlInitialization = False
                        Else
                            If sig.IndicateOnlyWithControlSetEnabled And Not sig.FailState Then
                                If sig.SignalControlled Then
                                    sig.SignalControlled = False
                                    sig.SignalControlUpdateDatabase()
                                End If
                            End If
                        End If
                    End If

                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub IniitializeTemporalMode()
            Try

                If TemporalModePassenger Then
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = True
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = False
                    End If

                ElseIf TemporalModeFreight Then
                    If Not Me.Ctl(4) Is Nothing Then
                        Me.Ctl(4).BitState = False
                    End If
                    If Not Me.Ctl(3) Is Nothing Then
                        Me.Ctl(3).BitState = True
                    End If
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CancelStoredRoutes()
            Try
                If Me.StoredRoutes.Count <> 0 Then
                    Dim storedRoute As StoredRoute
                    Dim theRoutes As ArrayList = StoredRoutes.Clone
                    For index As Integer = 0 To theRoutes.Count - 1
                        TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(theRoutes(index), "CANCELLED")
                    Next
                    For Each storedRoute In theRoutes
                        storedRoute.EntrySignal.StoredEntrySignal = False
                        storedRoute.ExitSignal.StoredExitSignal = False
                        storedRoute.RemoveMe("CancelStoredRoutes1", True)
                    Next
                End If
                StoredRoutes.Clear()
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CancelStoredRoutes(ByVal ComponentsCollection As ArrayList)
            Dim theComp As Object = Nothing
            Try
                If StoredRoutes.Count <> 0 Then
                    Dim storedRoute As StoredRoute
                    Dim theRoutes As ArrayList = Me.StoredRoutes.Clone
                    For index As Integer = 0 To theRoutes.Count - 1
                        TMDS.NetworkMessages.CadMessages.SendStoredRouteEventForWabtecOne(theRoutes(index), "CANCELLED")
                    Next
                    For Each storedRoute In theRoutes
                        For Each theComp In ComponentsCollection
                            If storedRoute.ComponentSelection.Contains(theComp) Then
                                storedRoute.EntrySignal.StoredEntrySignal = False
                                storedRoute.ExitSignal.StoredExitSignal = False
                                storedRoute.RemoveMe("CancelStoredRoutes", True)
                                Exit For
                            End If
                        Next
                    Next
                End If
                Me.FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CreateDeviceComponents()
            Try


                If optEnableCodeFailureMiscDevice Then
                    Me.devCFK = New Device()
                    Me.devCFK.TypeOfDevice = "IndicateOnly"
                    Me.devCFK.AlphaColor = Color.Red
                    Me.devCFK.myFontForceToBold = True
                    Me.devCFK.InitializeGraphicsFromScreenCoordinates()
                    Me.devCFK.Name = optCodeFailureDeviceName
                    Me.DeviceCollection.Add(Me.devCFK)
                    Me.devCFK.myControlPoint = Me

                End If

                Dim TopPosition As Int32 = Me.Top + Me.Height + 6
                Dim I As Integer
                For I = 0 To 9
                    If Not Me.Ind(I) Is Nothing Then
                        Select Case I
                            Case 0
                                Me.devMOK = New Device()
                                Me.devMOK.TypeOfDevice = "IndicateOnly"
                                Me.devMOK.Name = optManualOperateText
                                Me.devMOK.Tag = optManualOperateText
                                'If Not optEnableCPSpecificOptions Then
                                '    Me.devMOK.Name = "MO"
                                '    Me.devMOK.Tag = "MO"
                                'Else
                                '    Me.devMOK.Name = "LC"
                                '    Me.devMOK.Tag = "LC"
                                'End If
                                Me.DeviceCollection.Add(Me.devMOK)
                            Case 1
                                Me.devPOK = New Device()
                                Me.devPOK.TypeOfDevice = "IndicateOnly"
                                Me.devPOK.Name = optDevPOKName ' "PO"
                                Me.devPOK.Tag = "PO"
                                Me.DeviceCollection.Add(Me.devPOK)
                            Case 4
                                Me.devSMK = New Device()
                                Me.devSMK.TypeOfDevice = "IndicateOnly"
                                Me.devSMK.Name = "SM"
                                Me.devSMK.Tag = "SM"
                                Me.DeviceCollection.Add(Me.devSMK)
                            Case 2
                                Me.devLOK = New Device()
                                Me.devLOK.TypeOfDevice = "IndicateOnly"
                                Me.devLOK.Name = "LO"
                                Me.devLOK.Tag = "LO"
                                Me.DeviceCollection.Add(Me.devLOK)
                            Case 3
                                Me.devDOK = New Device()
                                Me.devDOK.TypeOfDevice = "IndicateOnly"
                                Me.devDOK.Name = optDevDOKName '"DO"
                                Me.devDOK.Tag = "DO"
                                Me.DeviceCollection.Add(Me.devDOK)
                            Case 5
                                Me.devGEN = New Device()
                                Me.devGEN.TypeOfDevice = "IndicateOnly"
                                Me.devGEN.Name = "GN"
                                Me.DeviceCollection.Add(Me.devGEN)
                            Case 8
                                Me.devTMF = New Device()
                                Me.devTMF.TypeOfDevice = "IndicateOnly"
                                Me.devTMF.Name = "TMF"
                                Me.DeviceCollection.Add(Me.devTMF)
                                Me.devTMF.TemporalType = True
                            Case 9
                                Me.devTMP = New Device()
                                Me.devTMP.TypeOfDevice = "IndicateOnly"
                                Me.devTMP.Name = "TMP"
                                Me.DeviceCollection.Add(Me.devTMP)
                                Me.devTMP.Left = Me.devPOK.Left
                                Me.devTMP.Top = Me.devMOK.Top + Me.devMOK.Height + 5
                                Me.devTMP.TemporalType = True

                        End Select
                    End If
                Next

                Dim DeviceSpacer As Integer = 5
                Dim dev As Device
                For Each dev In Me.DeviceCollection
                    dev.GUID = Me.GUID
                    dev.CreatedWithControlPoint = True
                    dev.TerritoryID = Me.TerritoryID
                    dev.myControlPoint = Me
                    dev.LX = Me.Left + DeviceSpacer
                    dev.LY = TopPosition
                    dev.BuildGraphics()
                    dev.InitGraphics()

                    Me.AssignedComponenets.Add(dev)
                    Me.CurrentDrawingForm = Me.myControlPoint.CurrentDrawingForm
                    Me.CurrentDrawingForm.Controls.Add(dev)

                    Select Case dev.Name
                        Case "TMP"
                            dev.Left = Me.devPOK.Left
                            dev.Top = Me.devPOK.Top + Me.devMOK.Height + 1
                            dev.ForeColor = Color.Lime
                        Case "TMF"
                            dev.Left = Me.devMOK.Left
                            dev.Top = Me.devMOK.Top + Me.devMOK.Height + 1
                            dev.ForeColor = Color.Red

                        Case Else
                            DeviceSpacer = DeviceSpacer + 25
                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub FlashControlPointDevices(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                Dim FlashBrush As SolidBrush
                If Me.FlashOnDevices Then
                    FlashOnDevices = False
                    FlashBrush = Me.FlashOnDevBrush
                Else
                    FlashOnDevices = True
                    FlashBrush = Me.FlashOffDevBrush
                End If
                Dim dev As Device
                For Each dev In Me.DeviceCollection
                    If dev.DeviceState = True Then
                        'dev.Draw(FlashBrush)
                        dev.Visible = FlashOnDevices
                        dev.Refresh()
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ApplyIndicationChange()
            'Check ind/intime bits and state a temp variable to reflect thier state
            Dim comp As Object = Nothing
            Dim DoForceCPReSend As Boolean = False
            Try

                FlashDevices = False


                If optEnableCodeFailureMiscDevice AndAlso Not Me.FailState Then
                    If Not IsNothing(Me.devCFK) Then
                        Me.devCFK.DeviceState = False
                        Me.devCFK.UpdateStatus()
                    End If
                End If

                If Not Me.Ind(0) Is Nothing Then
                    If Ind(0).BitState = True Then
                        Me.devMOK.DeviceState = True
                        FlashDevices = True
                        Me.ManualOperateMode = True
                        Try
                            If optDisableExistingStoredRouteOnManualOperate Then CancelStoredRoutes()
                            If optDisableExistingFleetSignalOnManualOperate Then
                                For Each comp In AssignedComponenets
                                    If comp.GetType.ToString = gSignalComponentName Then
                                        If comp.FleetEnabled Then comp.DoSetSignalFleeting(False, True)
                                    End If
                                Next
                            End If
                        Catch exCancelSRandFleet As Exception
                            GetErrorDetails(exCancelSRandFleet)
                        End Try
                        If optEnableTrainStopOnPowerOffManualOperateCheck Then
                            CheckToStopTrainOnOManualOperate(Me.devMOK)
                        End If
                    Else
                        If optEnableReSendOnMO_OFF AndAlso Me.ManualOperateMode Then DoForceCPReSend = True
                        Me.devMOK.DeviceState = False
                        Me.ManualOperateMode = False


                    End If
                    Me.devMOK.Indicated = Me.devMOK.DeviceState
                    Me.devMOK.UpdateStatus()
                    ApplyMonitorMode()
                End If
                Dim SinglePowerOff As Boolean
                If Me.Ind(6) Is Nothing Then
                    SinglePowerOff = True
                End If
                Dim PowerOffIndicated As Boolean

                If Not Me.Ind(1) Is Nothing Then
                    If Me.Ind(1).BitState = True Then
                        If optEnableTrainStopOnPowerOffManualOperateCheck Then
                            CheckToStopTrainOnPowerOff(Me.Ind(1).CodeStation, SinglePowerOff)
                        End If
                        PowerOffIndicated = True
                    End If
                Else
                    PowerOffIndicated = False
                End If

                If Not Me.Ind(6) Is Nothing Then
                    If Me.Ind(6).BitState = True Then

                        If optEnableTrainStopOnPowerOffManualOperateCheck Then
                            CheckToStopTrainOnPowerOff(Me.Ind(6).CodeStation, SinglePowerOff)
                        End If
                        PowerOffIndicated = True

                    ElseIf Not PowerOffIndicated Then
                        PowerOffIndicated = False
                    End If
                End If

                If Not IsNothing(Me.devPOK) Then
                    If PowerOffIndicated Then
                        Me.devPOK.DeviceState = True
                        Me.PowerOff = True
                        FlashDevices = True
                        Me.devPOK.UpdateStatus()
                    Else

                        Me.PowerOff = False
                        Me.devPOK.DeviceState = False

                        Me.devPOK.Indicated = Me.devPOK.DeviceState
                        Me.devPOK.UpdateStatus()
                    End If
                End If




                If Not Me.Ind(2) Is Nothing Then
                    If Me.Ind(2).BitState = True Then
                        Me.devLOK.DeviceState = True
                        FlashDevices = True
                        If optEnableTractionPowerManagementSystem AndAlso Ind(2).IsAlarmType Then
                            GenerateAlarmAlert(Me.Ind(2))
                        End If
                    Else
                        Me.devLOK.DeviceState = False
                    End If
                    Me.devLOK.Indicated = Me.devLOK.DeviceState

                    Me.devLOK.UpdateStatus()
                End If

                If Not Me.Ind(3) Is Nothing Then
                    If Me.Ind(3).BitState = True Then
                        Me.devDOK.DeviceState = True
                        FlashDevices = True
                    Else
                        Me.devDOK.DeviceState = False
                    End If
                    Me.devDOK.Indicated = Me.devDOK.DeviceState
                    Me.devDOK.UpdateStatus()
                End If

                If Not Me.Ind(4) Is Nothing Then
                    If Me.Ind(4).BitState = True Then
                        If Not Me.Ind(7) Is Nothing Then
                            If Me.Ind(7).BitState = True Then
                                Me.devSMK.DeviceState = True
                                FlashDevices = True
                                If SlotSMControlOnIndication Then
                                    If Not IsNothing(Me.CtlSM) Then
                                        If Me.CtlSM.BitState Then
                                            'Me.CtlSM.BitState = False
                                            Me.mnuSMZ_Click(Nothing, Nothing)
                                            Me.SendCodeStationControlSpecial(True)
                                        End If
                                    End If
                                End If
                            Else
                                Me.devSMK.DeviceState = False
                            End If

                        Else
                            Me.devSMK.DeviceState = True
                            FlashDevices = True
                            If Not devSMKINIT Then
                                If Not optEnableCustomSnowMelterTimer Then Me.mnuSMZ.Checked = True
                                Me.CtlSM.BitState = True
                                If optEnableCustomSnowMelterTimer AndAlso Me.CtlSM IsNot Nothing AndAlso Me.CtlSM.BitState AndAlso mnuSMZStop IsNot Nothing Then mnuSMZStop.Enabled = True
                            End If
                            If Not IsNothing(Me.CtlSMOFF) Then
                                Me.CtlSMOFF.BitState = False
                            End If
                        End If
                    Else
                        Me.devSMK.DeviceState = False
                    End If
                    devSMKINIT = True
                    Me.devSMK.Indicated = Me.devSMK.DeviceState
                    Me.devSMK.UpdateStatus()
                End If

                If Not Me.Ind(5) Is Nothing Then
                    If Me.Ind(5).BitState = True Then
                        Me.devGEN.DeviceState = True
                        FlashDevices = True
                    Else
                        Me.devGEN.DeviceState = False
                    End If
                    Me.devGEN.Indicated = Me.devGEN.DeviceState
                    Me.devGEN.UpdateStatus()

                End If

                If Not Me.Ind(8) Is Nothing Then
                    If Me.Ind(8).BitState = True Then
                        Me.devTMF.DeviceState = True
                        FlashDevices = True
                        Me.TemporalModeFreight = True
                    Else
                        Me.devTMF.DeviceState = False
                        Me.TemporalModeFreight = False
                    End If
                    Me.devTMF.Indicated = Me.devTMF.DeviceState
                    Me.devTMF.UpdateStatus()
                End If

                If Not Me.Ind(9) Is Nothing Then
                    If Me.Ind(9).BitState = True Then
                        Me.devTMP.DeviceState = True
                        Me.TemporalModePassenger = True
                        FlashDevices = True
                    Else
                        Me.devTMP.DeviceState = False
                        Me.TemporalModePassenger = False
                    End If
                    Me.devTMP.Indicated = Me.devTMP.DeviceState
                    Me.devTMP.UpdateStatus()
                End If



                If Me.myControlPoint.AllowStatusPublishing Then
                    Me.PublishStatus()
                End If
                IndicatedOnce = True

                If DoForceCPReSend AndAlso Me.myControlPoint.Initialized Then
                    If Me.ResendTime < Now.AddSeconds(-10) Then
                        TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL(MO Released) RE-SEND SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                        Me.DoForceReSend()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                comp = Nothing
            End Try
        End Sub

        Private Sub GenerateAlarmAlert(bp As BitPosition)
            Try
                Dim MessageAlert As String = "SCADA ALARM DETECTED"
                Dim Alarm As New AlarmAlert
                Alarm.myControlPoint = Me
                Select Case bp.AlarmLevel
                    Case "CRITICAL"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.CRITICAL
                    Case "HIGH"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.HIGH
                    Case "MEDIUM"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.MEDIUM
                    Case "LOW"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.LOW

                End Select
                Alarm.myAleryEquipmentUID = bp.AlarmEquipmentUID
                Alarm.myAlertDescription = bp.AlarmDescription

                TMDS.ManagementClasses.AlertUtilities.CreateAlarmAlert(ManagementClasses.Alert.AlertType.AlarmAlert, MessageAlert, Alarm)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Function IsControlPoint(ByVal e As System.Windows.Forms.MouseEventArgs) As Boolean
            Try

                If Me.GraphicalPath.IsVisible(e.X, e.Y) Then
                    Me.AddToFlashCollection(Me)
                    Me.ReSetFlasherCollection()
                    FlashComponent = True
                    Flasher.Enabled = True
                    Return True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        ''' <summary>
        ''' Cancel Send Collection
        ''' </summary>
        Public Sub CancelSendCollection()
            Try
                For Each oneComponent As Object In AssignedComponenets
                    Select Case oneComponent.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim theSignal As Signal = DirectCast(oneComponent, Signal)
                            If theSignal.LastCtlStatus <> theSignal.SignalCtlStatus Then
                                Select Case theSignal.LastCtlStatus
                                    Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                        If oneComponent.DrillMode Then
                                            theSignal.ControlSignalToDrill(False)
                                            theSignal.DrilledSignal = True
                                            If theSignal.DrillPartner IsNot Nothing Then
                                                theSignal.DrillPartner.DrilledSignal = True
                                            End If
                                        Else
                                            theSignal.ControlSignalToClear(False, False, False, False, False, True, True, False, False, theSignal.PreviousNonCompositeDetected)
                                            theSignal.ControlNotYetCommitted = False
                                        End If
                                    Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                                        If theSignal.DrillMode Then
                                            theSignal.ControlSignalToDrillStop()
                                            theSignal.DrillMode = False
                                            If theSignal.DrillPartner IsNot Nothing Then
                                                theSignal.DrillPartner.DrillMode = False
                                            End If
                                        Else
                                            SystemEvent($"{GetCurrentMethod.Name}:1:Signal={theSignal.Name}:{theSignal.ControlNotYetCommitted}")
                                            theSignal.ControlSignalToStop(False)
                                        End If
                                End Select
                                oneComponent.RemoveFromFlashCollection(oneComponent)
                            ElseIf oneComponent.tmrCheckRoute.Enabled AndAlso
                                theSignal.ControlNotYetCommitted Then
                                SystemEvent($"{GetCurrentMethod.Name}:2:Signal={theSignal.Name}:{theSignal.ControlNotYetCommitted}")
                                oneComponent.ControlSignalToStop(False)
                            ElseIf theSignal.ControlNotYetCommitted Then
                                SystemEvent($"{GetCurrentMethod.Name}:3:Signal={theSignal.Name}:{theSignal.ControlNotYetCommitted}")
                                oneComponent.ControlSignalToStop(False)
                            End If
                            oneComponent.LastCtlStatus = oneComponent.SignalCtlStatus
                            oneComponent.UpdateStatus()
                        Case "TMDS.SwitchComponents.Switch"
                            Dim theSwitch As Switch = DirectCast(oneComponent, Switch)
                            If theSwitch.LastControl <> theSwitch.ControlledReverse Then
                                theSwitch.RouteLock = False
                                If theSwitch.TypeOfSwitch <> "DERAIL" AndAlso
                                    Not theSwitch.IndicateOnly Then
                                    theSwitch.SwitchIndStatus = theSwitch.SwitchIndStatusPrevious
                                    theSwitch.ControlSwitch()
                                ElseIf theSwitch.TypeOfSwitch = "DERAIL" AndAlso
                                    theSwitch.IndicateOnly Then
                                    theSwitch.ControlMatchIndicateOnlyDerail(theSwitch.ControlledReverse)
                                End If
                            End If
                        Case "TMDS.GraphicComponent.Device"
                            Dim theDevice As Device = DirectCast(oneComponent, Device)
                            If theDevice.LastControl <> theDevice.DeviceControlled Then
                                theDevice.Device_MouseDown(Nothing, Nothing)
                            End If
                    End Select
                Next
                FlasherMode(False)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub SendRecall()
            Dim cdstat As CodeStation
            'Dim cdsvr As CodeServerClient.CodeServerConnection
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()
            Try
                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Or (cdstat.PartialControlDelivery And cdstat.ControlPointNumberSecondaryReference = Me.StationNumber) Then
                        If Not Me.CodeStationsToSend.Contains(cdstat) Then
                            myCodeStations.Add(cdstat)
                        End If
                    End If
                Next
                If myCodeStations.Count = 0 Then Exit Sub

                For Each cdstat In myCodeStations

                    cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber, CodeServers)
                    Dim RecallMessage As String = "RECALL;" & cdstat.CodeLineNumber.ToString
                    RecallMessage = RecallMessage & ":" & cdstat.ControlPointNumber.ToString
                    RecallMessage = RecallMessage & ":" & cdstat.Number.ToString.PadLeft(2, "0")
                    RecallMessage = RecallMessage & ":FALSE"
                    If RecallMessage Is Nothing Then Exit Sub
                    If Not cdsNormal Is Nothing Then
                        cdsNormal.SendData(RecallMessage)
                        Dim EventMessage As String = "RECALL MESSAGE SENT TO NORMAL CODE SERVER:" & cdsNormal.myCodeServerName & " (" & RecallMessage & ")"
                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                        SysEvent = Nothing
                    End If

                    cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber, CodeServers)
                    If RecallMessage Is Nothing Then Exit Sub
                    If Not cdsStandby Is Nothing Then
                        cdsStandby.SendData(RecallMessage)
                        Dim EventMessage As String = "RECALL MESSAGE SENT TO STANDBY CODE SERVER:" & cdsStandby.myCodeServerName & " (" & RecallMessage & ")"
                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                        SysEvent = Nothing
                    End If

                Next
                myCodeStations.Clear()
                myCodeStations = Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Overloads Sub SendRecall(ByVal codeservers As ArrayList, ByVal codelines As ArrayList, ByVal codestations As ArrayList)
            Dim cdstat As CodeStation
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()
            Try
                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Or (cdstat.PartialControlDelivery And cdstat.ControlPointNumberSecondaryReference = Me.StationNumber) Then
                        If Not Me.CodeStationsToSend.Contains(cdstat) Then
                            myCodeStations.Add(cdstat)
                        End If
                    End If
                Next

                If myCodeStations.Count = 0 Then Exit Sub

                For Each cdstat In myCodeStations

                    cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber, codeservers)
                    Dim RecallMessage As String = "RECALL;" & cdstat.CodeLineNumber.ToString
                    RecallMessage = RecallMessage & ":" & cdstat.ControlPointNumber.ToString
                    RecallMessage = RecallMessage & ":" & cdstat.Number.ToString.PadLeft(2, "0")
                    RecallMessage = RecallMessage & ":FALSE"
                    If RecallMessage Is Nothing Then Exit Sub
                    If Not cdsNormal Is Nothing Then
                        cdsNormal.SendData(RecallMessage)
                        Dim EventMessage As String = "RECALL MESSAGE SENT TO NORMAL CODE SERVER:" & cdsNormal.myCodeServerName & " (" & RecallMessage & ")"
                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                        SysEvent = Nothing
                    End If

                    cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber, codeservers)
                    If RecallMessage Is Nothing Then Exit Sub
                    If Not cdsStandby Is Nothing Then
                        cdsStandby.SendData(RecallMessage)
                        Dim EventMessage As String = "RECALL MESSAGE SENT TO STANDBY CODE SERVER:" & cdsStandby.myCodeServerName & " (" & RecallMessage & ")"
                        Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                        SysEvent = Nothing
                    End If
                Next

                myCodeStations.Clear()
                myCodeStations = Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        ''' <summary>
        ''' SEND CODESTATION CONTROL SPECIAL
        ''' </summary>
        ''' <param name="overRideMonitorMode"></param>
        ''' <param name="overRideFailureMode"></param>
        ''' <param name="resendType"></param>
        Public Sub SendCodeStationControlSpecial(Optional overRideMonitorMode As Boolean = False,
                                                 Optional overRideFailureMode As Boolean = True,
                                                 Optional resendType As Boolean = False)
            Dim cdStat As CodeStation
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()
            SystemEvent($"{GetCurrentMethod.Name}:
                        Send To Control Special Start:WhoCallMe={(New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name}:
                        OverRideMonitorMode:{overRideMonitorMode.ToString}:")
            Try
                If optDisableControlsInMonitorMode OrElse IsShadowMode Then Exit Sub
                If IsNothing(CodeStationList) Then Exit Sub
                For Each cdStat In CodeStationList
                    If cdStat.ControlPointNumber = StationNumber Or
                        (cdStat.PartialControlDelivery And cdStat.ControlPointNumberSecondaryReference = StationNumber) Then
                        If Not CodeStationsToSend.Contains(cdStat) Then
                            If Not cdStat.FailState Or overRideFailureMode Then
                                myCodeStations.Add(cdStat)
                            End If
                        End If
                    End If
                Next
                CodeStationsToSend.Clear()
                If myCodeStations.Count = 0 Then Exit Sub
                ResendTime = Now
                If overRideMonitorMode Then
                    For Each cdStat In myCodeStations
                        If cdStat.PartialControlDelivery Then
                            cdsNormal = FindCodeServerConnection(cdStat.CodeLineNumber,
                                                                 CodeServers)
                            Dim ControlMessage As String = AssemblePartialControlMessage(cdStat,
                                                                                         CtlIndBits,
                                                                                         False,
                                                                                         resendType)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:1:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsNormal IsNot Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = $"PARTIAL CONTROL MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                            cdsStandby = FindCodeServerStandbyConnection(cdStat.CodeLineNumber,
                                                                         CodeServers)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:2:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsStandby IsNot Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = $"PARTIAL CONTROL MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                        Else
                            cdsNormal = FindCodeServerConnection(cdStat.CodeLineNumber,
                                                                 CodeServers)
                            Dim ControlMessage As String = AssembleControlMessage(cdStat,
                                                                                  CtlIndBits,
                                                                                  False,
                                                                                  resendType)

                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:3:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsNormal IsNot Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = $"CONTROL MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                            cdsStandby = FindCodeServerStandbyConnection(cdStat.CodeLineNumber,
                                                                         CodeServers)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:4:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsStandby IsNot Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = $"CONTROL MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                        End If
                    Next
                End If
                myCodeStations.Clear()
                myCodeStations = Nothing
                LogControlEvents()
                Dim comp As Object
                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(comp, Signal)
                            sig.ControlNotYetCommitted = False
                            sig.NoSignalControlInitialization = False
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            Select Case sig.SignalCtlStatus
                                Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                                    If sig.DrillMode Then
                                        sig.DrillMode = False
                                        If sig.DrillPartner IsNot Nothing Then
                                            sig.DrillPartner.DrillMode = False
                                        End If
                                        If sig.myDrillTrack IsNot Nothing Then
                                            Try
                                                sig.myDrillTrack.DrillTrack = False
                                                sig.myDrillTrack.myDrillSignal = Nothing
                                                sig.myDrillTrack = Nothing
                                            Catch
                                            End Try
                                        End If
                                    End If
                                    sig.PreviousNonCompositeDetected = False
                                Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                    If sig.DrillMode Then
                                        sig.DrilledSignal = True
                                        If sig.DrillPartner IsNot Nothing Then
                                            sig.DrillPartner.DrilledSignal = True
                                        End If
                                    End If
                            End Select
                            comp.LastCtlStatus = comp.SignalCtlStatus
                        Case "TMDS.SwitchComponents.Switch"
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            comp.LastControl = comp.ControlledReverse
                        Case "TMDS.GraphicComponent.Device"
                            comp.LastControl = comp.DeviceControlled
                    End Select
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' SEND CODE STATION CONTROLS
        ''' </summary>
        ''' <param name="resendType"></param>
        ''' <param name="codeStationAssigned"></param>
        Public Overloads Sub SendCodeStationControls(Optional resendType As Boolean = False,
                                                     Optional codeStationAssigned As Integer = 0)
            Dim cdStat As CodeStation
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()
            SystemEvent($"{GetCurrentMethod.Name}
                        :Send To Control Start:WhoCallMe={(New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name}
                        :ResendType:{resendType.ToString}
                        -NumberCSToSend:{CodeStationsToSend.Count.ToString}
                        -CSAssigned:{codeStationAssigned.ToString}")
            Try
                If optDisableControlsInMonitorMode OrElse IsShadowMode Then Exit Sub
                For Each cdStat In CodeStationList
                    If cdStat.ControlPointNumber = StationNumber Or
                       (cdStat.PartialControlDelivery And cdStat.ControlPointNumberSecondaryReference = StationNumber) Then
                        If Not cdStat.FailState Then
                            If CodeStationsToSend.Contains(cdStat.Number) Then
                                SystemEvent($"{GetCurrentMethod.Name}:CSToSend:{cdStat.Name}-CSAssigned:{codeStationAssigned.ToString}")
                                myCodeStations.Add(cdStat)
                            End If
                        End If
                    End If
                Next
                If codeStationAssigned <> 0 Then
                    Dim CodeStationFound As Boolean = False
                    For Each cdStat In myCodeStations
                        If cdStat.Number = codeStationAssigned Then
                            CodeStationFound = True
                        End If
                    Next
                    If CodeStationFound = False Then
                        SystemEvent($"{GetCurrentMethod.Name}:CSAssignedNotFound:{codeStationAssigned.ToString}")
                        For Each cdStat In CodeStationList
                            If cdStat.ControlPointNumber = StationNumber Or
                                (cdStat.PartialControlDelivery And cdStat.ControlPointNumberSecondaryReference = StationNumber) Then
                                If Not cdStat.FailState Then
                                    If cdStat.Number = codeStationAssigned Then
                                        SystemEvent($"{GetCurrentMethod.Name}:CSToSend:{cdStat.Name}-CSAssigned:{codeStationAssigned.ToString}")
                                        myCodeStations.Add(cdStat)
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
                CodeStationsToSend.Clear()
                If myCodeStations.Count = 0 Then
                    SystemEvent($"{GetCurrentMethod.Name}:{myCodeStations.Count = 0}:CSAssigned:{codeStationAssigned.ToString}")
                    Exit Sub
                End If
                ResendTime = Now
                If Not DoMonitorMode Then
                    For Each cdStat In myCodeStations
                        If cdStat.PartialControlDelivery Then
                            cdsNormal = FindCodeServerConnection(cdStat.CodeLineNumber,
                                                                 CodeServers)
                            Dim ControlMessage As String = AssemblePartialControlMessage(cdStat,
                                                                                        CtlIndBits,
                                                                                        False,
                                                                                        resendType)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:1:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsNormal IsNot Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = $"PARTIAL CONTROL MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                            cdsStandby = FindCodeServerStandbyConnection(cdStat.CodeLineNumber,
                                                                         CodeServers)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:2:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsStandby IsNot Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = $"PARTIAL CONTROL MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                        Else
                            cdsNormal = FindCodeServerConnection(cdStat.CodeLineNumber,
                                                                 CodeServers)
                            Dim ControlMessage As String = AssembleControlMessage(cdStat,
                                                                                  CtlIndBits,
                                                                                  False,
                                                                                  resendType)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:3:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsNormal IsNot Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = $"CONTROL MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                            cdsStandby = FindCodeServerStandbyConnection(cdStat.CodeLineNumber,
                                                                         CodeServers)
                            If ControlMessage Is Nothing Then
                                SystemEvent($"{GetCurrentMethod.Name}:4:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If cdsStandby IsNot Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = $"CONTROL MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})"
                                Dim SysEvent As New SystemEventRecord(Now,
                                                                      SystemEventRecord.EventEnum.SystemEvent,
                                                                      EventMessage,
                                                                      SystemEventQueue)
                            Else
                                SystemEvent($"CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:{cdStat.CodeLineNumber}:{ControlMessage}")
                            End If
                        End If
                    Next
                End If
                myCodeStations.Clear()
                myCodeStations = Nothing
                LogControlEvents()
                Dim comp As Object
                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(comp, Signal)
                            sig.ControlNotYetCommitted = False
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            Select Case sig.SignalCtlStatus
                                Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                                    If sig.DrillMode Then
                                        sig.DrillMode = False
                                        If sig.DrillPartner IsNot Nothing Then
                                            sig.DrillPartner.DrillMode = False
                                        End If
                                        If sig.myDrillTrack IsNot Nothing Then
                                            Try
                                                sig.myDrillTrack.DrillTrack = False
                                                sig.myDrillTrack.myDrillSignal = Nothing
                                                sig.myDrillTrack = Nothing
                                            Catch
                                            End Try
                                        End If
                                    End If
                                    sig.PreviousNonCompositeDetected = False
                                Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                    If sig.DrillMode Then
                                        sig.DrilledSignal = True
                                        If sig.DrillPartner IsNot Nothing Then
                                            sig.DrillPartner.DrilledSignal = True
                                        End If
                                    End If
                            End Select
                            comp.LastCtlStatus = comp.SignalCtlStatus
                        Case "TMDS.SwitchComponents.Switch"
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            comp.LastControl = comp.ControlledReverse
                        Case "TMDS.GraphicComponent.Device"
                            comp.LastControl = comp.DeviceControlled
                    End Select
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' SEND CODE STATION CONTROL UPDATE
        ''' </summary>
        Public Overloads Sub SendCodeStationControlUpdate()
            Dim cdstat As CodeStation
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()
            SystemEvent($"{GetCurrentMethod.Name}:Send Control Update Start:WhoCallMe ={(New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name}")
            Try
                If optDisableControlsInMonitorMode OrElse IsShadowMode Then Exit Sub
                If gAssistMode Then Exit Sub
                For Each cdstat In CodeStationList
                    If cdstat.ControlPointNumber = StationNumber Or
                        (cdstat.PartialControlDelivery And cdstat.ControlPointNumberSecondaryReference = StationNumber) Then
                        If Not CodeStationsToSend.Contains(cdstat) Then
                            If Not cdstat.FailState Then
                                myCodeStations.Add(cdstat)
                            End If
                        End If
                    End If
                Next
                CodeStationsToSend.Clear()
                If myCodeStations.Count = 0 Then Exit Sub
                For Each cdstat In myCodeStations
                    If cdstat.PartialControlDelivery Then
                        cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber,
                                                             CodeServers)
                        Dim ControlMessage As String = AssemblePartialControlMessage(cdstat,
                                                                                     CtlIndBits,
                                                                                     True)
                        If ControlMessage Is Nothing Then
                            SystemEvent($"{GetCurrentMethod.Name}:1:ControlMessage is nothing")
                            Exit Sub
                        End If
                        If cdsNormal IsNot Nothing Then
                            cdsNormal.SendData(ControlMessage)
                            SystemEvent($"PARITAL CONTROL UPDATE ONLY MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})")
                        End If
                        cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber,
                                                                     CodeServers)
                        If ControlMessage Is Nothing Then
                            SystemEvent($"{GetCurrentMethod.Name}:2:ControlMessage is nothing")
                            Exit Sub
                        End If
                        If cdsStandby IsNot Nothing Then
                            cdsStandby.SendData(ControlMessage)
                            SystemEvent($"PARITAL CONTROL UPDATE ONLY MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})")
                        End If
                    Else
                        cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber,
                                                             CodeServers)
                        Dim ControlMessage As String = AssembleControlMessage(cdstat,
                                                                              CtlIndBits,
                                                                              True)
                        If ControlMessage Is Nothing Then
                            SystemEvent($"{GetCurrentMethod.Name}:3:ControlMessage is nothing")
                            Exit Sub
                        End If
                        If cdsNormal IsNot Nothing Then
                            cdsNormal.SendData(ControlMessage)
                            SystemEvent($"CONTROL UPDATE ONLY MESSAGE SENT TO NORMAL CODE SERVER:{cdsNormal.myCodeServerName}:({ControlMessage})")
                        End If
                        cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber,
                                                                     CodeServers)
                        If ControlMessage Is Nothing Then
                            SystemEvent($"{GetCurrentMethod.Name}:4:ControlMessage is nothing")
                            Exit Sub
                        End If
                        If cdsStandby IsNot Nothing Then
                            cdsStandby.SendData(ControlMessage)
                            SystemEvent($"CONTROL UPDATE ONLY MESSAGE SENT TO STANDBY CODE SERVER:{cdsStandby.myCodeServerName}:({ControlMessage})")
                        End If
                    End If
                Next
                myCodeStations.Clear()
                myCodeStations = Nothing
                Dim comp As Object
                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            comp.LastCtlStatus = comp.SignalCtlStatus
                        Case "TMDS.SwitchComponents.Switch"
                            comp.LastControl = comp.ControlledReverse
                        Case "TMDS.GraphicComponent.Device"
                            comp.LastControl = comp.DeviceControlled
                    End Select
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overloads Sub SendCodeStationControls(ByVal codeservers As ArrayList, ByVal codelines As ArrayList, ByVal codestations As ArrayList, ByVal ctlindbits As ArrayList)

            Dim cdstat As CodeStation
            Dim cdsNormal As CodeServerClient.CodeServerConnection
            Dim cdsStandby As CodeServerClient.CodeServerConnection
            Dim myCodeStations As New ArrayList()

            TMDS.SystemUtilities.Utilities.SystemEvent("SendCodeStationControls:codeservers " & codeservers.Count.ToString)

            Try
                If optDisableControlsInMonitorMode OrElse Me.IsShadowMode Then Exit Sub
                If gAssistMode Then Exit Sub
                'If Me.FailState Then Exit Sub
                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Or (cdstat.PartialControlDelivery And cdstat.ControlPointNumberSecondaryReference = Me.StationNumber) Then
                        If Not cdstat.FailState Then
                            If Me.CodeStationsToSend.Contains(cdstat.Number) Then
                                myCodeStations.Add(cdstat)
                            End If
                        End If
                    End If
                Next

                CodeStationsToSend.Clear()

                If myCodeStations.Count = 0 Then Exit Sub
                ResendTime = Now
                If Not Me.DoMonitorMode Or MaintenanceMode Then
                    For Each cdstat In myCodeStations
                        If cdstat.PartialControlDelivery Then
                            cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber, codeservers)
                            Dim ControlMessage As String = Me.AssemblePartialControlMessage(cdstat, ctlindbits, False)
                            If ControlMessage Is Nothing Then
                                SystemEvent("SendCodeStationControls:5:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If Not cdsNormal Is Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = "PARTIAL CONTROL MESSAGE SENT TO NORMAL CODE SERVER:" & cdsNormal.myCodeServerName & " (" & ControlMessage & ")"
                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                                SysEvent = Nothing
                            Else
                                TMDS.SystemUtilities.Utilities.SystemEvent("CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:" & cdstat.CodeLineNumber & ":" & ControlMessage)
                            End If

                            cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber, codeservers)
                            If ControlMessage Is Nothing Then
                                SystemEvent("SendCodeStationControls:6:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If Not cdsStandby Is Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = "PARTIAL CONTROL MESSAGE SENT TO STANDBY CODE SERVER:" & cdsStandby.myCodeServerName & " (" & ControlMessage & ")"
                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                                SysEvent = Nothing
                            Else
                                TMDS.SystemUtilities.Utilities.SystemEvent("CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:" & cdstat.CodeLineNumber & ":" & ControlMessage)
                            End If

                        Else
                            cdsNormal = FindCodeServerConnection(cdstat.CodeLineNumber, codeservers)
                            Dim ControlMessage As String = AssembleControlMessage(cdstat, ctlindbits, False)

                            If ControlMessage Is Nothing Then
                                SystemEvent("SendCodeStationControls:7:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If Not cdsNormal Is Nothing Then
                                cdsNormal.SendData(ControlMessage)
                                Dim EventMessage As String = "CONTROL MESSAGE SENT TO NORMAL CODE SERVER:" & cdsNormal.myCodeServerName & " (" & ControlMessage & ")"
                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                                SysEvent = Nothing
                            Else
                                TMDS.SystemUtilities.Utilities.SystemEvent("CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:" & cdstat.CodeLineNumber & ":" & ControlMessage)
                            End If

                            cdsStandby = FindCodeServerStandbyConnection(cdstat.CodeLineNumber, codeservers)
                            If ControlMessage Is Nothing Then
                                SystemEvent("SendCodeStationControls:8:ControlMessage is nothing")
                                Exit Sub
                            End If
                            If Not cdsStandby Is Nothing Then
                                cdsStandby.SendData(ControlMessage)
                                Dim EventMessage As String = "CONTROL MESSAGE SENT TO STANDBY CODE SERVER:" & cdsStandby.myCodeServerName & " (" & ControlMessage & ")"
                                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventMessage, SystemEventQueue)
                                SysEvent = Nothing
                            Else
                                TMDS.SystemUtilities.Utilities.SystemEvent("CONTROL NOT SENT. CODESERVER NOT FOUND FOR CODE STATION:" & cdstat.CodeLineNumber & ":" & ControlMessage)
                            End If
                        End If
                    Next
                End If
                myCodeStations.Clear()
                myCodeStations = Nothing

                LogControlEvents()

                Dim comp As Object

                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(comp, Signal)
                            sig.ControlNotYetCommitted = False
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            Select Case sig.SignalCtlStatus
                                Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                                    If sig.DrillMode Then
                                        sig.DrillMode = False
                                        If Not IsNothing(sig.DrillPartner) Then
                                            sig.DrillPartner.DrillMode = False
                                        End If
                                        If Not IsNothing(sig.myDrillTrack) Then
                                            Try
                                                sig.myDrillTrack.DrillTrack = False
                                                sig.myDrillTrack.myDrillSignal = Nothing
                                                sig.myDrillTrack = Nothing
                                            Catch
                                            End Try
                                        End If
                                    End If
                                    sig.PreviousNonCompositeDetected = False
                                Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                    If sig.DrillMode Then
                                        sig.DrilledSignal = True
                                        If Not IsNothing(sig.DrillPartner) Then
                                            sig.DrillPartner.DrilledSignal = True
                                        End If
                                    End If
                            End Select
                            comp.LastCtlStatus = comp.SignalCtlStatus

                        Case "TMDS.SwitchComponents.Switch"
                            If comp.ReControlStart And comp.ReControlCount = 0 Then
                                comp.tmrReControl.Start()
                            End If
                            comp.LastControl = comp.ControlledReverse
                        Case "TMDS.GraphicComponent.Device"
                            comp.LastControl = comp.DeviceControlled

                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Function FindCodeServerConnection(ByVal cdlnNumber As Integer, ByVal codeservers As ArrayList) As CodeServerClient.CodeServerConnection
            Try
                Dim cdsvr As CodeServerClient.CodeServerClient
                Dim cdln As Codeline
                For Each cdsvr In codeservers
                    For Each cdln In cdsvr.AssignedCodeLines
                        If cdln.CodeLineNumber = cdlnNumber Then
                            Return cdsvr.NormalCodeServerConnection
                        End If
                    Next
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try

        End Function

        Private Function FindCodeServerStandbyConnection(ByVal cdlnNumber As Integer, ByVal codeservers As ArrayList) As CodeServerClient.CodeServerConnection
            Try
                Dim cdsvr As CodeServerClient.CodeServerClient
                Dim cdln As Codeline
                For Each cdsvr In codeservers
                    For Each cdln In cdsvr.AssignedCodeLines
                        If cdln.CodeLineNumber = cdlnNumber Then
                            Return cdsvr.StandbyCodeServerConnection
                        End If
                    Next
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function AssembleControlMessage(ByVal cdstat As CodeStation, ByVal CtlIndBits As ArrayList, ByVal UpdateControlImageOnly As Boolean, Optional ByVal Resend As Boolean = False) As String
            Dim CtlHeader As String
            Dim CtlMess As String = String.Empty
            Dim Control As String
            Dim posbit As BitPosition
            Dim ControlBits(cdstat.NumOfControls) As String
            Dim myControlBits As New ArrayList()
            Dim bitIndex As Integer
            Try

                If CtlIndBits Is Nothing OrElse CtlIndBits.Count = 0 Then SystemEvent("AssembleControlMessage:CtlIndBits is nothing or empty")

                For Each posbit In CtlIndBits
                    If posbit.TypeOfBit = CodeServerClient.BitPosition.BitType.CONTROL Then
                        If posbit.Codeline = cdstat.CodeLineNumber And posbit.ControlPoint = cdstat.ControlPointNumber And posbit.CodeStation = cdstat.Number Then
                            myControlBits.Add(posbit)
                        End If
                    End If
                Next

                If myControlBits Is Nothing OrElse myControlBits.Count = 0 Then SystemEvent("AssembleControlMessage:myControlBits is nothing or empty")

                If ControlBits Is Nothing Then SystemEvent("AssembleControlMessage:ControlBits is nothing")

                For Each posbit In myControlBits
                    For bitIndex = 1 To ControlBits.Length - 1
                        If bitIndex = posbit.PositionOfBit Then
                            If posbit.BitState Then
                                If Not optEnableNoSendSwitchBitOnLockedSwitch Then
                                    ControlBits(bitIndex) = "1"
                                Else
                                    Dim SetToZero As Boolean
                                    Dim comp As Object
                                    For Each comp In posbit.ComponentRefCollection
                                        If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                            Dim sw As Switch = CType(comp, Switch)
                                            If sw.SwitchLockNoControl And (sw.SwitchLockBit Or sw.SignalStatOnMe <> Switch.SignalStatus.SignalAtStop) Then
                                                SetToZero = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    If SetToZero Then
                                        ControlBits(bitIndex) = "0"
                                        SetToZero = False
                                    Else
                                        ControlBits(bitIndex) = "1"
                                    End If
                                End If

                            Else
                                ControlBits(bitIndex) = "0"
                            End If
                        End If
                    Next
                Next

                For bitIndex = 1 To ControlBits.Length - 1
                    If ControlBits(bitIndex) = Nothing Then
                        ControlBits(bitIndex) = "0"
                    End If
                    CtlMess = CtlMess & ControlBits(bitIndex)
                Next
                If UpdateControlImageOnly Then
                    CtlHeader = "CONTROL UPDATE;" & cdstat.CodeLineNumber.ToString
                Else
                    If Resend Then
                        CtlHeader = "RESENDCONTROL;" & cdstat.CodeLineNumber.ToString
                    Else
                        CtlHeader = "CONTROL;" & cdstat.CodeLineNumber.ToString
                    End If
                End If

                CtlHeader = CtlHeader & ":" & cdstat.ControlPointNumber.ToString
                CtlHeader = CtlHeader & ":" & cdstat.Number.ToString.PadLeft(2, "0")
                Control = CtlHeader & ":" & CtlMess

                If Control Is Nothing Then SystemEvent("AssembleControlMessage:Control is nothing")

                myControlBits.Clear()
                myControlBits = Nothing
                Return Control

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function AssemblePartialControlMessage(ByVal cdstat As CodeStation, ByVal CtlIndBits As ArrayList, ByVal UpdateControlImageOnly As Boolean, Optional ByVal Resend As Boolean = False) As String
            Dim CtlHeader As String
            Dim CtlMess As String = String.Empty
            Dim Control As String
            Dim posbit As BitPosition
            Dim ControlBits(cdstat.NumOfControls) As String
            Dim myControlBits As New ArrayList()
            Dim bitIndex As Integer
            Try

                For Each posbit In CtlIndBits
                    If posbit.TypeOfBit = CodeServerClient.BitPosition.BitType.CONTROL Then
                        If posbit.Codeline = cdstat.CodeLineNumber And posbit.ControlPoint = cdstat.ControlPointNumber And posbit.CodeStation = cdstat.Number Then
                            myControlBits.Add(posbit)
                        End If
                    End If
                Next


                For Each posbit In myControlBits
                    For bitIndex = 1 To ControlBits.Length - 1
                        If bitIndex = posbit.PositionOfBit Then
                            If posbit.BitState Then
                                If Not optEnableNoSendSwitchBitOnLockedSwitch Then
                                    ControlBits(bitIndex) = "1"
                                Else
                                    Dim SetToZero As Boolean
                                    Dim comp As Object
                                    For Each comp In posbit.ComponentRefCollection
                                        If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                            Dim sw As Switch = CType(comp, Switch)
                                            If sw.SwitchLockNoControl And (sw.SwitchLockBit Or sw.SignalStatOnMe <> Switch.SignalStatus.SignalAtStop) Then
                                                SetToZero = True
                                                Exit For
                                            End If
                                        ElseIf comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                            Dim sig As Signal = CType(comp, Signal)
                                            If sig.tmrCheckRoute.Enabled Then
                                                SetToZero = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                    If SetToZero Then
                                        ControlBits(bitIndex) = "0"
                                        SetToZero = False
                                    Else
                                        ControlBits(bitIndex) = "1"
                                    End If
                                End If

                            Else
                                ControlBits(bitIndex) = "0"
                            End If
                        End If
                    Next
                Next

                Dim index As Integer
                For bitIndex = 1 To ControlBits.Length - 1
                    If Not IsNothing(ControlBits(bitIndex)) Then
                        If index = 0 Then
                            CtlMess = bitIndex.ToString & "=" & ControlBits(bitIndex)
                            index = 1
                        Else
                            CtlMess = CtlMess & "|" & bitIndex.ToString & "=" & ControlBits(bitIndex)
                        End If

                    End If
                Next

                If UpdateControlImageOnly Then
                    CtlHeader = "CONTROL UPDATE-PARTIAL;" & cdstat.CodeLineNumber.ToString
                Else
                    If Resend Then
                        CtlHeader = "RESENDCONTROL-PARTIAL;" & cdstat.CodeLineNumber.ToString
                    Else
                        CtlHeader = "CONTROL-PARTIAL;" & cdstat.CodeLineNumber.ToString
                    End If
                End If

                CtlHeader = CtlHeader & ":" & cdstat.ControlPointNumber.ToString
                CtlHeader = CtlHeader & ":" & cdstat.Number.ToString.PadLeft(2, "0")
                Control = CtlHeader & ":" & CtlMess
                myControlBits.Clear()
                myControlBits = Nothing
                Return Control

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function AssembleIndicationMessage(ByVal cdstat As CodeStation) As String
            Dim INDHeader As String
            Dim INDMess As String = String.Empty
            Dim INDICATION As String
            Dim posbit As BitPosition
            Dim IndBits(cdstat.NumOfControls) As String
            Dim myIndBits As New ArrayList()
            Dim bitIndex As Integer
            Try

                For Each posbit In CtlIndBits
                    If posbit.TypeOfBit = CodeServerClient.BitPosition.BitType.INDICATION Then
                        If posbit.Codeline = cdstat.CodeLineNumber And posbit.ControlPoint = cdstat.ControlPointNumber And posbit.CodeStation = cdstat.Number Then
                            myIndBits.Add(posbit)
                        End If
                    End If
                Next


                For Each posbit In myIndBits
                    For bitIndex = 1 To IndBits.Length - 1
                        If bitIndex = posbit.PositionOfBit Then
                            If posbit.BitState Then
                                IndBits(bitIndex) = "1"
                            Else
                                IndBits(bitIndex) = "0"
                            End If
                        End If
                    Next
                Next

                For bitIndex = 1 To IndBits.Length - 1
                    If IndBits(bitIndex) = Nothing Then
                        IndBits(bitIndex) = "0"
                    End If
                    INDMess = INDMess & IndBits(bitIndex)
                Next
                INDHeader = "INDICATION=" & cdstat.CodeLineNumber.ToString

                INDHeader = INDHeader & ":" & cdstat.ControlPointNumber.ToString
                INDHeader = INDHeader & ":" & cdstat.Number.ToString.PadLeft(2, "0")
                INDICATION = INDHeader & ":" & INDMess
                myIndBits.Clear()
                myIndBits = Nothing
                Return INDICATION

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Sub AssignComponentsToControlPoint(ByRef GlobalComponentCollection As ArrayList)
            Dim comp As Object
            Try
                For Each comp In GlobalComponentCollection
                    If comp.StationNumber = Me.StationNumber Then 'And comp.GetType.ToString <> "TMDS.GraphicComponent.ControlPoint" Then
                        If Not AssignedComponenets.Contains(comp) Then
                            AssignedComponenets.Add(comp)
                        End If
                    End If
                Next

                For Each comp In Me.AssignedComponenets
                    comp.myControlPoint = Me
                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        Dim sig As Signal = CType(comp, Signal)
                        sig.tmrReControl.Interval = Me.ResendControlTime
                        comp.FindOsSlottingTrack()
                    ElseIf comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not Me.AssignedTracks.Contains(comp) Then
                            Me.AssignedTracks.Add(comp)
                        End If
                    ElseIf comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        Dim sw As Switch = CType(comp, Switch)
                        sw.tmrReControl.Interval = Me.ResendControlTime
                    End If
                    GlobalComponentCollection.Remove(comp)
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitializeTrackAssignment()
            Try
                Dim TempTracks As ArrayList
                TempTracks = Me.AssignedTracks.Clone
                Dim TrackList As New ArrayList()
                Dim trk As Track
                Dim CurrentFarTrack As Track = Nothing
                Dim FarLeft As Int32
                Dim FarBottom As Int32
                Dim LoopControl As Integer
                While Me.AssignedTracks.Count > 0
                    For Each trk In TempTracks
                        If trk.Left > FarLeft And trk.Top > FarBottom Then
                            CurrentFarTrack = trk
                            FarLeft = trk.Left
                            FarBottom = trk.Top
                        End If
                    Next
                    FarLeft = 0
                    FarBottom = 0
                    TrackList.Add(CurrentFarTrack)
                    Me.AssignedTracks.Remove(CurrentFarTrack)
                    TempTracks.Remove(CurrentFarTrack)
                    LoopControl += 1
                    If LoopControl > 500 Then
                        Exit While
                    End If
                End While
                TrackList.Reverse()
                Me.AssignedTracks.Clear()
                Me.AssignedTracks = TrackList.Clone
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub CheckForMultipleStationAssignments()
            Try
                'Dim cdstat As CodeStation
                'Dim StationCount As Integer
                'For Each cdstat In Me.CodeStationList
                '    If cdstat.ControlPointNumber = Me.StationNumber Then
                '        StationCount += 1
                '    End If
                'Next
                'If StationCount > 1 Then
                '    Me.ContainsMultipleStations = True
                'End If
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        Dim sw As Switch = CType(comp, Switch)
                        If sw.AllowIndependentControlSwitch Then
                            Me.ContainsMultipleStations = True
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub FailStation(Optional ByVal StationNumber As Integer = 0)
            If gSimulationModeARS Then Exit Sub
            Dim comp As Object = Nothing
            Try
                Dim cdstat As CodeStation

                For Each cdstat In Me.CodeStationList
                    If cdstat.ControlPointNumber = Me.StationNumber Then
                        If optEnableOnlyShowFailureOnCodeStationComponents Then
                            If cdstat.Number = StationNumber Then
                                cdstat.FailState = True
                            End If
                        Else
                            cdstat.FailState = True
                        End If
                    End If
                Next

                Me.ManualOperateMode = False
                Me.ApplyMonitorMode()

                If Me.myControlPoint.AllowStatusPublishing Then
                    Me.PublishStatus()
                End If
                If Not gSimulationMode Then
                    AllStationsInitialized = False
                    FailState = True
                    CommFail = True
                    Initialized = False
                End If
                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(comp, Signal)

                            Dim DoFailSignal As Boolean = True
                            If optEnableOnlyShowFailureOnCodeStationComponents Then
                                If sig.CodeStation <> StationNumber And StationNumber <> 0 Then
                                    DoFailSignal = False
                                End If
                            End If
                            If gSimulationMode Then DoFailSignal = False

                            If DoFailSignal Then
                                sig.FailState = True
                                If Not optDisableShowingCodeFailureOnComponents Then
                                    sig.BrotherSignalFirst = False
                                    sig.SignalCtlStatus = Signal.CtlStatus.RequestedStop
                                    sig.SignalIndStatus = Signal.IndStatus.Stopped
                                    sig.SlotToStop()
                                    If sig.DrillType <> Signal.TypeOfDrill.None Then
                                        sig.ControlSignalToDrillStop()
                                        sig.SetToDrillStop()
                                    End If
                                    comp.Initialized = False
                                    If optAllowNoSignalControlInitializationOnFailure Then
                                        sig.NoSignalControlInitialization = True
                                    End If
                                End If
                                Try
                                    If optDisableFleetSignalOnCommFail Then
                                        If sig.FleetEnabled Then sig.DoSetSignalFleeting(False, True)
                                    End If
                                Catch ex As Exception
                                    GetErrorDetails(ex)
                                End Try
                            End If
                        Case gSwitchComponentName
                            Dim oneSwitch As Switch = DirectCast(comp, Switch)
                            Dim doFailSwitch As Boolean = True
                            If optEnableOnlyShowFailureOnCodeStationComponents AndAlso
                                oneSwitch.CodeStation <> StationNumber AndAlso
                                StationNumber <> 0 Then
                                doFailSwitch = False
                            End If
                            If gSimulationMode Then doFailSwitch = False
                            If doFailSwitch Then
                                oneSwitch.SignalStatOnMe = Switch.SignalStatus.SignalAtStop
                                If oneSwitch.Ind(0) Is Nothing Then
                                    oneSwitch.FailState = False
                                End If
                                If oneSwitch.PowerSwitchInTWC AndAlso
                                    Not oneSwitch.RCPS Then
                                    oneSwitch.FailState = False
                                ElseIf oneSwitch.RCPS Then
                                    If Not oneSwitch.FailState Then
                                        For Each oneAuthority As SFAuthority In oneSwitch.TrackAuthorities
                                            If oneAuthority.IssuedToTrain Then
                                                ShowAlarmForCommFailureRCPS(oneSwitch, oneAuthority)
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End If
                                If Not oneSwitch.SwitchPointerMonitorMode Then
                                    If oneSwitch.AuthorityType.Equals("TWC") AndAlso
                                        oneSwitch.NoIndicationAssigned Then
                                        If Not oneSwitch.Initialized Then
                                            oneSwitch.PreviouSIndStatus = oneSwitch.SwitchIndStatus
                                        End If
                                        oneSwitch.Initialized = True
                                        oneSwitch.FailState = False
                                    Else
                                        oneSwitch.Initialized = False
                                        oneSwitch.FailState = True
                                    End If
                                Else
                                    If oneSwitch.CodeStation = StationNumber Then
                                        oneSwitch.Initialized = False
                                        oneSwitch.FailState = True
                                        If oneSwitch.SwitchPointerMonitorMode Then
                                            If oneSwitch.ManualSwitchPointVerification Then
                                                oneSwitch.FailState = False
                                            End If
                                        End If
                                        If oneSwitch.mySwitchPositionIndicator IsNot Nothing Then
                                            oneSwitch.mySwitchPositionIndicator.SetPosition()
                                            oneSwitch.CheckForExistingAuthorities()
                                        End If
                                    End If
                                End If
                            End If
                        Case "TMDS.TrackComponents.Track"
                            Dim trk As Track = CType(comp, Track)

                            Dim DoFailTrack As Boolean = True
                            If optEnableOnlyShowFailureOnCodeStationComponents Then
                                If trk.CodeStation <> StationNumber And StationNumber <> 0 Then
                                    DoFailTrack = False
                                End If
                            End If
                            If gSimulationMode Then DoFailTrack = False

                            If DoFailTrack Then
                                trk.FailState = True
                                If trk.Indicating = False Then
                                    trk.FailState = False
                                End If
                                If Mid(trk.AuthorityType, 1, 3) = "TWC" Then
                                    trk.FailState = False
                                Else
                                    trk.Initialized = False
                                    trk.FailStatePrevious = True
                                End If
                            End If
                    End Select
                    comp.UpdateStatus()
                Next

                If optEnableCodeFailureMiscDevice Then
                    If Me.FailState Then
                        Me.devCFK.DeviceState = True
                        FlashDevices = True
                    Else
                        Me.devCFK.DeviceState = False
                    End If
                    Me.devCFK.UpdateStatus()
                End If

                If gTraineeMode Then
                    Me.SendSimulatedFailStation()
                End If

                devSMKINIT = False
                CancelStoredRoutes()

                If optEnableTrainStopOnCommunicationFailureCheck Then
                    CheckToStopTrainOnOCommunicationFailure()
                End If

                Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))

            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                comp = Nothing
            End Try
        End Sub

        Public Sub ControlAcknowledgement()
            Dim comp As Object
            Try
                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(comp, Signal)
                            If sig.tmrReControl.Enabled Then
                                sig.StopReControlTimer()
                            End If
                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub FlashName()
            Try
                If Me.Indicating = True And flashNameTimer.Enabled = True Then
                    Me.Indicating = False
                    Me.Invalidate()
                    flashNameTimer.Enabled = False
                    flashNameTimer.EndInit()
                    flashNameTimer.Stop()
                    flashNameTimer.Close()
                    Exit Sub
                End If
                flashNameTimer.Interval = 300
                Me.Indicating = True
                flashNameTimer.Enabled = True
                pFlashNameStartTime = DateTime.Now()
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Refresh()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub FlashNameEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                Me.Indicating = False
                Me.Invalidate()
                flashNameTimer.Enabled = False
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function FindComponent(ByVal UID As Int32) As Object
            Try
                Dim comp As Object
                For Each comp In Me.AssignedComponenets
                    If comp.GUID = UID Then
                        Return comp
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Sub LogControlEvents()
            Try

                Dim comp As Object

                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(comp, Switch)
                            If sw.LastControl <> sw.ControlledReverse Then
                                Dim PublishThisEvent As New DoPublishSwitchEvent(AddressOf sw.PublishSwitchEventUpdate)
                                If sw.ControlledReverse Then
                                    PublishThisEvent.BeginInvoke(Switch.EventTypeSw.CONTROLREVERSE, Nothing, Nothing)
                                Else
                                    PublishThisEvent.BeginInvoke(Switch.EventTypeSw.CONTROLNORMAL, Nothing, Nothing)
                                End If
                            End If
                    End Select
                Next


                For Each comp In AssignedComponenets
                    Select Case comp.GetType.ToString
                        Case "TMDS.SignalComponents.Signal"
                            If comp.LastCtlStatus <> comp.SignalCtlStatus Then
                                Dim sig As Signal = CType(comp, Signal)
                                Dim LogThisEvent As New DoEventLogSignal(AddressOf sig.LogTheSignalEvent)
                                LogThisEvent.BeginInvoke(Signal.EventTypeSignal.CTL, Nothing, Nothing)

                                Dim PublishThisEvent As New DoPublishSignalEvent(AddressOf sig.PublishSignalEventUpdate)
                                Select Case sig.SignalCtlStatus
                                    Case Signal.CtlStatus.RequestedClear, Signal.CtlStatus.RequestingClear
                                        PublishThisEvent.BeginInvoke(Signal.EventTypeSig.REQUESTCLEAR, Nothing, Nothing)
                                    Case Signal.CtlStatus.RequestedStop, Signal.CtlStatus.RequestingStop
                                        PublishThisEvent.BeginInvoke(Signal.EventTypeSig.REQUESTSTOP, Nothing, Nothing)
                                End Select

                            End If
                            comp.LastCtlStatus = comp.SignalCtlStatus
                        Case "TMDS.SwitchComponents.Switch"
                            If comp.LastControl <> comp.ControlledReverse Then
                                Dim sw As Switch = CType(comp, Switch)
                                Dim LogThisEvent As New DoEventLogSwitch(AddressOf sw.LogTheSwitchEvent)
                                LogThisEvent.BeginInvoke(Switch.EventTypeSwitch.CTL, Nothing, Nothing)
                            End If
                            comp.LastControl = comp.ControlledReverse
                        Case "TMDS.GraphicComponent.Device"
                            If comp.LastControl <> comp.DeviceControlled Then
                                Dim dv As Device = CType(comp, Device)
                                Dim LogThisEvent As New DoEventLogDevice(AddressOf dv.LogTheDeviceEvent)
                                LogThisEvent.BeginInvoke(Device.EventTypeDevice.CTL, Nothing, Nothing)
                            End If
                            comp.LastControl = comp.DeviceControlled
                    End Select
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DebugEvent(ByVal EventNote As String)

            Try
                If gDebug Then
                    Dim DebugEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventNote, SystemEventQueue)
                    DebugEvent = Nothing
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Public Sub PublishAllRoutes()
            Try
                Dim sr As StoredRoute
                For Each sr In StoredRoutes
                    Dim PublishEvent As New DoPublishControlPointEvent(AddressOf PublishCPEventUpdate)
                    PublishEvent.BeginInvoke(ControlPoint.EventTypeCP.SROUTEON, sr, Nothing, Nothing)
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Sub DoForceReSend()
            Try
                Me.FlasherMode(True)
                If optDisableControlPointMenuFunctionsOnManualOperate And Me.ManualOperateMode Then
                    Me.FlasherMode(False)
                    Exit Sub
                End If

                If Me.MaintenanceMode And Not gMaintenanceMode Then
                    AddMessageToFeedback("CONTROL Not OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " Is CURRENTLY In MAINTENANCE MODE")
                    Exit Sub
                End If

                If Me.DoMonitorMode Then
                    'Dim UserMessage As String = "CONTROL POINT (" & Me.Name & ") CURRENTLY In MONITOR MODE? RE-SEND CONTROL, ARE YOU SURE?"
                    'TMDS.SystemUtilities.Utilities.SystemEvent(UserMessage)
                    'Dim UserResponse As Integer = MessageBox.Show(Me.CurrentDrawingForm, UserMessage, "SYSTEM MESSAGE", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                    'If UserResponse = 6 Then
                    '    Me.SendCodeStationControlSpecial(True, False, optEnableResendControlSpecial)
                    '    TMDS.SystemUtilities.Utilities.SystemEvent("SELECTED YES To RESEND CONTROL To " & Me.Name)
                    'Else
                    '    TMDS.SystemUtilities.Utilities.SystemEvent("SELECTED NO To RESEND CONTROL To " & Me.Name)
                    'End If
                    TMDS.SystemUtilities.Utilities.SystemEvent("CP In MONITOR MODE FORCE RESEND Not SENT For CP:" & Me.Name & ":" & Me.GUID.ToString)
                Else
                    If Me.ResendTime < Now.AddSeconds(-30) Then
                        TMDS.SystemUtilities.Utilities.SystemEvent("MANUAL RESEND SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                        Me.SendCodeStationControlSpecial(True, False, optEnableResendControlSpecial)
                        ResendTime = Now
                    Else
                        TMDS.SystemUtilities.Utilities.SystemEvent("RESEND OFFICE LOCK FORCE RESEND NOT SENT FOR CP:" & Me.Name & ":" & Me.GUID.ToString)
                    End If
                End If



            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                Me.FlasherMode(False)
            End Try
        End Sub

#Region "MRS METHODS"
        Private Delegate Sub PerformCheckToStopTrainOnPowerOff(ByVal CodeStnNumber As Integer, ByVal AllSignals As Boolean)
        Private Delegate Sub PerformCheckToStopTrainOnManualOperate(ByVal dv As Device)
        Private Delegate Sub PerformCheckToStopTrainOnCommunciationFailure()

        Private Sub CheckToStopTrainOnOManualOperate(ByVal dv As Device)
            Try
                Dim DoTheCheck As New PerformCheckToStopTrainOnManualOperate(AddressOf DoCheckToStopTrainOnManualOperate)
                DoTheCheck.BeginInvoke(dv, Nothing, Nothing)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckToStopTrainOnOCommunicationFailure()
            Try
                Dim DoTheCheck As New PerformCheckToStopTrainOnCommunciationFailure(AddressOf DoCheckToStopTrainOnCommuncationFailure)
                DoTheCheck.BeginInvoke(Nothing, Nothing)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckToStopTrainOnPowerOff(ByVal CodeStnNumber As Integer, ByVal AllSignals As Boolean)
            Try
                Dim DoTheCheck As New PerformCheckToStopTrainOnPowerOff(AddressOf DoCheckToStopTrainOnPowerOff)
                DoTheCheck.BeginInvoke(CodeStnNumber, AllSignals, Nothing, Nothing)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DoCheckToStopTrainOnManualOperate(ByVal dv As Device) 'release license behind the train
            Try
                SystemEvent("CBTC:DoCheckToStopTrainOnManualOperate:" & Me.Name)
                Dim AuthList As New ArrayList
                Dim SegList As New ArrayList
                Dim theTrack As Track
                For Each theTrack In Me.AssignedTracks
                    If theTrack.OStype AndAlso theTrack.CodeStation = dv.CodeStation Then
                        Dim sig As Signal
                        If Not IsNothing(theTrack.SignalSlotCollection) Then
                            For Each sig In theTrack.SignalSlotCollection
                                If sig.SignalCtlStatus = Signal.CtlStatus.RequestedClear Or sig.SignalCtlStatus = Signal.CtlStatus.RequestingClear Then
                                    AuthList.Add(theTrack.TrackAuthorities.Item(0))
                                    Dim newseg As New LicenseSegment(theTrack, theTrack.TrackAuthorities.Item(0))
                                    SegList.Add(newseg)
                                End If
                            Next
                        End If
                    End If
                Next

                Dim ls As LicenseSegment
                For Each ls In SegList
                    Dim theTrain As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(ls.theAuth.IssueTo)
                    If Not IsNothing(theTrain) Then
                        Dim tr As TacticalRoute
                        For Each tr In theTrain.myTacticalRoutes
                            If tr.mySignalRoute.ComponentList.Contains(ls.theTrack) AndAlso theTrain.GeoFenceLegacyEnabled Then
                                theTrain.PtcCancelNormalLicense(tr.mySignalRoute.EntrySignal, True)
                                Exit For
                            End If
                        Next
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub DoCheckToStopTrainOnCommuncationFailure()
            Try
                SystemEvent("CBTC:DoCheckToStopTrainOnCommuncationFailure:" & Me.Name)
                Dim AuthList As New ArrayList
                Dim SegList As New ArrayList
                Dim theTrack As Track
                For Each theTrack In Me.AssignedTracks
                    If theTrack.IsTrackWarrantEnforcementEnabled AndAlso theTrack.OStype AndAlso theTrack.TrackAuthorities.Count > 0 AndAlso theTrack.FailState Then
                        AuthList.Add(theTrack.TrackAuthorities.Item(0))
                        Dim newseg As New LicenseSegment(theTrack, theTrack.TrackAuthorities.Item(0))
                        SegList.Add(newseg)
                        Exit For
                    End If
                Next

                Dim ls As LicenseSegment
                For Each ls In SegList
                    Dim theTrain As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(ls.theAuth.IssueTo)
                    If Not IsNothing(theTrain) Then
                        Dim tr As TacticalRoute
                        For Each tr In theTrain.myTacticalRoutes
                            If tr.mySignalRoute.ComponentList.Contains(ls.theTrack) AndAlso Not theTrain.GeoFenceLegacyEnabled Then
                                theTrain.PtcCancelNormalLicense(tr.mySignalRoute.EntrySignal, True)
                                Exit For
                            End If
                        Next
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


        Private Sub DoCheckToStopTrainOnPowerOff(ByVal CodeStationNum As Integer, ByVal AllSignals As Boolean)
            Try
                SystemEvent("CBTC:DoCheckToStopTrainOnPowerOffManualOperate:" & Me.Name)
                Dim AuthList As New ArrayList
                Dim SegList As New ArrayList
                Dim theTrack As Track
                For Each theTrack In Me.AssignedTracks
                    If theTrack.OStype AndAlso (theTrack.CodeStation = CodeStationNum Or AllSignals) Then
                        Dim sig As Signal
                        If Not IsNothing(theTrack.SignalSlotCollection) Then
                            For Each sig In theTrack.SignalSlotCollection
                                If sig.SignalCtlStatus = Signal.CtlStatus.RequestedClear Or sig.SignalCtlStatus = Signal.CtlStatus.RequestingClear Then
                                    AuthList.Add(theTrack.TrackAuthorities.Item(0))
                                    Dim newseg As New LicenseSegment(theTrack, theTrack.TrackAuthorities.Item(0))
                                    SegList.Add(newseg)
                                End If
                            Next
                        End If
                    End If
                Next

                Dim ls As LicenseSegment
                For Each ls In SegList
                    Dim theTrain As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(ls.theAuth.IssueTo)
                    If Not IsNothing(theTrain) Then
                        Dim tr As TacticalRoute
                        For Each tr In theTrain.myTacticalRoutes
                            If tr.mySignalRoute.ComponentList.Contains(ls.theTrack) AndAlso theTrain.GeoFenceLegacyEnabled Then
                                theTrain.PtcCancelNormalLicense(tr.mySignalRoute.EntrySignal, True)
                                Exit For
                            End If
                        Next
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Class LicenseSegment

            Public theTrack As Track
            Public theAuth As TrackAuthority.TrackAuthority

            Sub New(ByRef myTrack As Track, ByVal myauth As TrackAuthority.TrackAuthority)
                theAuth = myauth
                theTrack = myTrack
            End Sub
        End Class

#End Region

#Region "AUTO ROUTER COMMANDS "

        'Public Function ReshuffleStoredRoutes() As Boolean
        '    Try
        '        Dim AddToTheBottomOfList As Boolean
        '        Dim RouteOrderChanged As Boolean
        '        Dim ChangesMade As Boolean
        '        Dim NewList As New ArrayList
        '        If Me.StoredRoutes.Count = 0 Then Exit Function
        '        Dim sr As StoredRoute
        '        Dim LastTimeStamp As Date = Now.AddDays(1)
        '        Dim CurrentIndex As Integer
        '        Dim CurrentIndexOfNonDependentRoute As Integer
        '        Dim LastIndexOfNewestDependentStoredRoute As Integer
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1")
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoreRoute Count:" & Me.StoredRoutes.Count)

        '        For Each sr In Me.StoredRoutes
        '            TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteToShuffle:" & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
        '            If sr.myRouteDependentSymbolName <> "" Then
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteTrainSymbol:" & sr.myRouteDependentSymbolName)
        '                If Not IsNothing(sr.myTacticalRoute) Then
        '                    If sr.myTacticalRoute.myRouteDependencies.Count <> 0 Then
        '                        Dim rd As RouteDependency = sr.myTacticalRoute.myRouteDependencies(0)
        '                        If IsNothing(rd) Then Exit For
        '                        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteDepTime:" & rd.TrnArrival.ToString)
        '                        If rd.TrnArrival < LastTimeStamp Then
        '                            LastTimeStamp = rd.TrnArrival
        '                            If CurrentIndex > 0 Then
        '                                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Index1:" & LastIndexOfNewestDependentStoredRoute)
        '                                NewList.Insert(LastIndexOfNewestDependentStoredRoute, sr)
        '                                RouteOrderChanged = True
        '                                LastIndexOfNewestDependentStoredRoute = NewList.IndexOf(sr)
        '                                ChangesMade = True
        '                            Else
        '                                LastIndexOfNewestDependentStoredRoute = CurrentIndex
        '                                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Index2:" & LastIndexOfNewestDependentStoredRoute)
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '                If Not RouteOrderChanged Then
        '                    AddToTheBottomOfList = True
        '                    TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1.1")
        '                End If

        '                If AddToTheBottomOfList Then
        '                    NewList.Add(sr)
        '                    AddToTheBottomOfList = False
        '                    TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1.2")
        '                End If
        '            Else
        '                NewList.Insert(CurrentIndexOfNonDependentRoute, sr)
        '                CurrentIndexOfNonDependentRoute += 1
        '                LastIndexOfNewestDependentStoredRoute += 1
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1.2")
        '                If NewList.Count > 1 Then
        '                    ChangesMade = True
        '                End If
        '            End If
        '            RouteOrderChanged = False
        '            CurrentIndex += 1

        '        Next

        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2")
        '        If Not ChangesMade Then
        '            TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.1")
        '            Return False
        '        End If
        '        SyncLock Me.StoredRoutes
        '            Dim Count As Integer = Me.StoredRoutes.Count
        '            Dim i As Integer
        '            For i = 1 To Count
        '                Dim srRemove As StoredRoute = Me.StoredRoutes.Item(0)
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Remove:" & srRemove.RouteGUID)
        '                Me.StoredRoutes.RemoveAt(0)
        '            Next
        '            TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.2")
        '        End SyncLock
        '        SyncLock Me.StoredRoutes
        '            'Me.StoredRoutes.Clear()
        '            Dim Count As Integer
        '            For Each sr In NewList
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Add: " & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
        '                Me.StoredRoutes.Add(sr)
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.3")
        '                sr.Priority = Count
        '                sr.EntrySignal.StoredEntrySignal = False
        '                sr.ExitSignal.StoredExitSignal = False
        '                'TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.4")
        '                'sr.EntrySignal.InvalidateSignal()
        '                'sr.ExitSignal.InvalidateSignal()
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.5")
        '                Count = Count + 1
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2.6")
        '            Next
        '        End SyncLock
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step3")

        '        Dim strte As StoredRoute = Me.StoredRoutes(0)
        '        If strte.Priority = 0 Then
        '            strte.CheckToDisplayStoredRouteDependency()
        '            strte.UpdateSignalDisplays()
        '        Else
        '            strte.CheckToUpdateOtherSignals()
        '        End If

        '        '
        '        Me.ReorderStoredRoutes()

        '        SyncLock gSummaryRouteQueue
        '            Dim SummItem As New SummaryItem("ROUTE", "REMOVE", Me)
        '            gSummaryRouteQueue.Enqueue(SummItem)
        '        End SyncLock

        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step4")

        '        Return True
        '    Catch ex As Exception
        '        SystemException(ex.ToString)
        '    End Try
        'End Function

        Public Function ReshuffleStoredRoutes() As Boolean
            Try
                Dim AddToTheBottomOfList As Boolean
                Dim RouteOrderChanged As Boolean
                Dim NewList As New ArrayList
                If Me.StoredRoutes.Count = 0 Then Exit Function
                Dim sr As StoredRoute
                Dim LastTimeStamp As Date = Now.AddDays(1)
                Dim CurrentIndex As Integer
                Dim LastIndexOfNewestDependentStoredRoute As Integer
                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1")
                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoreRoute Count:" & Me.StoredRoutes.Count)

                For Each sr In Me.StoredRoutes
                    TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteToShuffle:" & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
                    'if my symbol name is not blank
                    If sr.myRouteDependentSymbolName <> "" Then
                        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteTrainSymbol:" & sr.myRouteDependentSymbolName)
                        'if i have a tactical route
                        If Not IsNothing(sr.myTacticalRoute) Then
                            'if my tactical route has dependencies
                            If sr.myTacticalRoute.myRouteDependencies.Count <> 0 Then
                                'create a reference dependency to my first route dependency
                                Dim rd As RouteDependency = sr.myTacticalRoute.myRouteDependencies(0)
                                'if my route temporary dependency is nothing then GTFO
                                If IsNothing(rd) Then Exit For
                                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteDepTime:" & rd.TrnArrival.ToString)
                                'if my tac routes first dependency train arrival time is less than the last time stamp (First run is Today + 1)
                                If rd.TrnArrival < LastTimeStamp Then
                                    'last time stamp = tac route first dependency train arrival time
                                    LastTimeStamp = rd.TrnArrival
                                    'if this route isn't already the first route
                                    If CurrentIndex > 0 Then
                                        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Index:" & LastIndexOfNewestDependentStoredRoute)
                                        'move this route to next in line of a new temporary list
                                        NewList.Insert(LastIndexOfNewestDependentStoredRoute, sr)
                                        'route order has obviously changed
                                        RouteOrderChanged = True
                                        'this line could also be solved with LastIndexOfNewestDependentStoredRoute = NewList.Count - 1
                                        LastIndexOfNewestDependentStoredRoute = NewList.IndexOf(sr)
                                    Else
                                        'this route dependency is already first so don't do anything
                                        LastIndexOfNewestDependentStoredRoute = CurrentIndex
                                    End If
                                End If
                            End If
                        End If
                    End If

                    'if the route order has not changed, then this stored route is already priority 0
                    If Not RouteOrderChanged Then
                        AddToTheBottomOfList = True
                    End If

                    If AddToTheBottomOfList Then
                        NewList.Add(sr)
                        AddToTheBottomOfList = False
                    End If
                    CurrentIndex += 1
                Next
                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2")
                'if the route order didn't change at all, then GTFO
                If Not RouteOrderChanged Then Exit Function
                SyncLock Me.StoredRoutes
                    Me.StoredRoutes.Clear()
                    Dim Count As Integer
                    'Go through and recreate the stored route list 
                    For Each sr In NewList
                        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Add: " & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
                        Me.StoredRoutes.Add(sr)
                        sr.Priority = Count
                        sr.EntrySignal.StoredEntrySignal = False
                        sr.ExitSignal.StoredExitSignal = False
                        sr.EntrySignal.InvalidateSignal()
                        sr.ExitSignal.InvalidateSignal()
                        Count = Count + 1
                    Next
                End SyncLock
                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step3")

                'create a reference to my first stored route
                Dim strte As StoredRoute = Me.StoredRoutes(0)
                'if the priority of this route is 0, then it is the first in the list so lets check to see
                'if we should display it
                If strte.Priority = 0 Then
                    strte.CheckToDisplayStoredRouteDependency()
                    strte.UpdateSignalDisplays()
                Else
                    strte.CheckToUpdateOtherSignals()
                End If

                '
                Me.ReorderStoredRoutes()

                SyncLock gSummaryRouteQueue.SyncRoot
                    Dim SummItem As New SummaryItem("ROUTE", "REMOVE", Me)
                    gSummaryRouteQueue.Enqueue(SummItem)
                End SyncLock

                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step4")
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function


        'Public Sub ReshuffleStoredRoutes()
        '    Try
        '        Dim AddToTheBottomOfList As Boolean
        '        Dim RouteOrderChanged As Boolean
        '        Dim NewList As New ArrayList
        '        If Me.StoredRoutes.Count = 0 Then Exit Sub
        '        Dim sr As StoredRoute
        '        Dim LastTimeStamp As Date = Now.AddDays(1)
        '        Dim CurrentIndex As Integer
        '        Dim LastIndexOfNewestDependentStoredRoute As Integer
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step1")
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoreRoute Count:" & Me.StoredRoutes.Count)

        '        For Each sr In Me.StoredRoutes
        '            TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteToShuffle:" & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
        '            If sr.myRouteDependentSymbolName <> "" Then
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteTrainSymbol:" & sr.myRouteDependentSymbolName)
        '                If Not IsNothing(sr.myTacticalRoute) Then
        '                    If sr.myTacticalRoute.myRouteDependencies.Count <> 0 Then
        '                        Dim rd As RouteDependency = sr.myTacticalRoute.myRouteDependencies(0)
        '                        If IsNothing(rd) Then Exit For
        '                        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-StoredRouteDepTime:" & rd.TrnArrival.ToString)
        '                        If rd.TrnArrival < LastTimeStamp Then
        '                            LastTimeStamp = rd.TrnArrival
        '                            If CurrentIndex > 0 Then
        '                                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Index:" & LastIndexOfNewestDependentStoredRoute)
        '                                NewList.Insert(LastIndexOfNewestDependentStoredRoute, sr)
        '                                RouteOrderChanged = True
        '                                LastIndexOfNewestDependentStoredRoute = NewList.IndexOf(sr)
        '                            Else
        '                                LastIndexOfNewestDependentStoredRoute = CurrentIndex
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If

        '            If Not RouteOrderChanged Then
        '                AddToTheBottomOfList = True
        '            End If

        '            If AddToTheBottomOfList Then
        '                NewList.Add(sr)
        '                AddToTheBottomOfList = False
        '            End If
        '            CurrentIndex += 1
        '        Next
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step2")
        '        If Not RouteOrderChanged Then Exit Sub
        '        SyncLock Me.StoredRoutes
        '            Me.StoredRoutes.Clear()
        '            Dim Count As Integer
        '            For Each sr In NewList
        '                TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Add: " & sr.EntrySignal.Name & " TO " & sr.ExitSignal.Name & " (" & sr.RouteGUID & ")")
        '                Me.StoredRoutes.Add(sr)
        '                sr.Priority = Count
        '                sr.EntrySignal.StoredEntrySignal = False
        '                sr.ExitSignal.StoredExitSignal = False
        '                sr.EntrySignal.InvalidateSignal()
        '                sr.ExitSignal.InvalidateSignal()
        '                Count = Count + 1
        '            Next
        '        End SyncLock
        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step3")

        '        Dim strte As StoredRoute = Me.StoredRoutes(0)
        '        If strte.Priority = 0 Then
        '            strte.CheckToDisplayStoredRouteDependency()
        '            strte.UpdateSignalDisplays()
        '        Else
        '            strte.CheckToUpdateOtherSignals()
        '        End If

        '        '
        '        Me.ReorderStoredRoutes()

        '        SyncLock gSummaryRouteQueue
        '            Dim SummItem As New SummaryItem("ROUTE", "REMOVE", Me)
        '            gSummaryRouteQueue.Enqueue(SummItem)
        '        End SyncLock

        '        TMDS.SystemUtilities.Utilities.SystemEvent("ReshuffleStoredRoutes-Step4")

        '    Catch ex As Exception
        '        SystemException(ex.ToString)
        '    End Try
        'End Sub


#End Region

#Region "STATUS UPDATES"

        Public Enum EventTypeCP
            SROUTEON
            SROUTEOFF
            SRDELETEALL
        End Enum

        Private Delegate Sub DoPublishControlPointEvent(ByVal EventType As EventTypeCP, ByVal SR As StoredRoute)

        Public Sub PublishCPEventUpdate(ByVal EventType As EventTypeCP, ByVal sr As StoredRoute)
            Try
                Dim Message As String = "DC" & Chr(6) & "CNTRLPT;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                Dim DeleteAllRoutes As Boolean

                Select Case EventType
                    Case EventTypeCP.SROUTEOFF
                        Message = Message & "|" & "SROFF"
                    Case EventTypeCP.SROUTEON
                        Message = Message & "|" & "SRON"
                    Case EventTypeCP.SRDELETEALL
                        Message = Message & "|" & "SRDELETEALL"
                        DeleteAllRoutes = True
                End Select
                If Not DeleteAllRoutes Then
                    Message = Message & "|" & Trim(sr.RouteGUID)
                    Message = Message & "|" & sr.EntrySignal.GUID.ToString
                    Message = Message & "|" & sr.ExitSignal.GUID.ToString
                    Message = Message & "|" & sr.Priority.ToString
                End If
                Message = Me.TerritoryID.ToString & Chr(12) & Message

                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                End SyncLock
                If DeleteAllRoutes Then
                    Threading.Thread.Sleep(1000)
                    Me.PublishAllRoutes()
                End If
            Catch EX As Exception
                SystemException(EX.ToString)
            End Try
        End Sub

        Public Sub PublishStatus(Optional ByVal ForceUpdate As Boolean = False)
            Try
                If Not ForceUpdate Then
                    If Me.FailState = StatusPreviousState Then
                        Exit Sub
                    Else
                        StatusPreviousState = Me.FailState
                    End If
                End If
                Dim Message As String = "DC" & Chr(6) & "CPSTAT;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString

                If Not Me.FailState Then
                    Message = Message & "|INIT=TRUE"
                Else
                    Message = Message & "|INIT=FALSE"
                End If

                Message = Me.TerritoryID.ToString & Chr(12) & Message

                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(Message)
                End SyncLock

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub PublishCPAutomaticTunnelStatus()
            Try
                Dim Message As String = "DC" & Chr(6) & "CNTRLPT;"
                Message = Message & "|" & Me.TerritoryID.ToString
                Message = Message & "|" & Me.GUID.ToString
                If Me.myControlPoint.TunnelAutomaticControlled Then
                    Message = Message & "|" & "ATON"
                Else
                    Message = Message & "|" & "ATOFF"
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

#Region "LOG EVENTS"

        Public Enum EventTypeControlPoint
            POWEROFF
            COMMUNICATIONFAILURE
            MICSDEVICE

        End Enum

        Private Delegate Sub DoEventLogControlPoint(ByVal EventType As EventTypeControlPoint, ByVal EventData As String)

        Public Sub LogTheControlPointEvent(ByVal EventType As EventTypeControlPoint, ByVal EventData As String)
            Try
                Dim CurUser As String = TMDS.GlobalDeclarations.gCurrentUser.LastName & "," & TMDS.GlobalDeclarations.gCurrentUser.FirstName
                Dim CurWrkstn As Integer = CInt(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim CurTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(Me.TerritoryID)
                Dim LogString As String = "INSERT INTO tblSystemEvents "
                'Dim SqlFields As String = "(EventTime, Territory, Wrkstn, SysUser, DbVer, FuncType, UID, Event, SysData)"
                Dim SqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim SqlValues As String = String.Empty
                Dim c As String = ", "
                Dim q As String = "'"
                Dim CPNAME As String = String.Empty
                Dim OtherTypeEvent As String = EventType.ToString.ToUpper
                Select Case OtherTypeEvent
                    Case "POWEROFF", "COMMUNICATIONFAILURE"
                        SqlValues = " VALUES (" & q & Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & q & c & Me.TerritoryID & c & CurWrkstn & c & q & CurUser & q & c
                        Dim myGUID As String = Me.myControlPoint.GUID.ToString

                        Dim myDBVer As String
                        If Not CurTerr Is Nothing Then
                            myDBVer = CurTerr.Dbversion
                        Else
                            myDBVer = "UNKNOWN"
                        End If
                        SqlValues = SqlValues & q & myDBVer & q & c & q & "CONTROL POINT" & q & c & q & Me.GUID & q & c

                        If Not Me.myControlPoint Is Nothing Then
                            CPNAME = Me.myControlPoint.Name
                        Else
                            CPNAME = "NONE"
                        End If

                End Select

                Select Case EventType
                    Case EventTypeControlPoint.COMMUNICATIONFAILURE
                        If Me.CommFail Then
                            SqlValues = SqlValues & q & "COMMUNICATION-FAILED" & q & c & q & CPNAME & ":" & Me.GUID & q
                        Else
                            SqlValues = SqlValues & q & "COMMUNICATION-UNFAILED" & q & c & q & CPNAME & ":" & Me.GUID & q
                        End If
                        SqlValues = SqlValues & ")"

                    Case EventTypeControlPoint.POWEROFF
                        If Me.PowerOff Then
                            SqlValues = SqlValues & q & "POWER-OFF" & q & c & q & CPNAME & ":" & Me.GUID & q
                        Else
                            SqlValues = SqlValues & q & "POWER-ON" & q & c & q & CPNAME & ":" & Me.GUID & q
                        End If
                        SqlValues = SqlValues & ")"
                End Select
                SyncLock gSystemEventLogQueue
                    TMDS.GlobalDeclarations.gSystemEventLogQueue.Enqueue(LogString & SqlFields & SqlValues)
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region
    End Class

#End Region

#Region "DEVICES"

    Public Class Device
        Inherits GenericComponent

        Sub New()
            Flasher = New Windows.Forms.Timer() 'New Timers.Timer()
            Flasher.Interval = 250
            AddHandler Flasher.Tick, AddressOf FlashElapsedEvent

        End Sub

        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then

                Try
                    RemoveHandler Me.MouseEnter, AddressOf Me.Device_MouseEnter
                    RemoveHandler Me.MouseLeave, AddressOf Me.Device_MouseLeave
                    RemoveHandler Me.MouseDown, AddressOf Me.Device_MouseDown
                Catch ex As Exception
                End Try
                Try
                    RemoveHandler Flasher.Tick, AddressOf FlashElapsedEvent
                    Flasher.Dispose()
                    Flasher = Nothing
                Catch ex As Exception
                End Try

                Me.pAlphaBrush = Nothing
                Me.ColorStatusBlocking = Nothing
                Me.pAlphaFont = Nothing
                Me.ColorSummarySelectionBlocking = Nothing
                Me.pStringFormat = Nothing
                Me.ColorStatusRouteRequesting = Nothing
                Me.pAlphaBrushOff = Nothing

            End If
            MyBase.Dispose(disposing)
        End Sub

        Public Overloads Sub Dispose()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#Region "Fields"

        Private pAlphaFont As Font = New Font("Microsoft Sans Serif", 11.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private pAlphaBox As RectangleF
        Private pAlphaLocation As Point
        Private pAlphaBoxSize As SizeF
        Private pAlphaColor As Color = Color.Yellow
        Private pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)
        Private pAlphaBrushOff As SolidBrush = New SolidBrush(Color.Black)
        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()
        Private pStringSize As SizeF
        Public pDeviceState As Boolean

        Public DeviceControlled As Boolean
        Public LastControl As Boolean
        Public MouseEntered As Boolean
        Public MouseLefted As Boolean
        Public MatchMode As Boolean
        Public ControlOnly As Boolean
        Public ControlIndicate As Boolean
        Public FlashIndicate As Boolean
        Public FlashControl As Boolean
        Public TextColor As String
        Public TypeOfDevice As String
        Public StringTypeDevice As Boolean
        Public CurrentColor As Color
        Public DefaultColor As Color
        Public IndicatedPreviously As Boolean
        Public IndicationORED As Boolean
        Public IndicationANDED As Boolean
        Public TrackReference As Track
        Public TrackReferences As ArrayList
        Public myTrackList As ArrayList

        Public TrackGUID As String
        Public Initialized As Boolean

        Public FontString As String
        Public myFontSize As Single
        Public myFontStyle As FontStyle
        Public myFontName As String
        Public myFontForceToBold As Boolean

        Public PrimaryText As String = " "
        Public PrimaryColor As Color

        Public SecondaryText As String = "NONE"
        Public SecondaryTextColor As String = Color.White.ToArgb.ToString
        Public SecondaryColor As Color
        Public DeviceControlOn As ArrayList
        Public DeviceControlOff As ArrayList
        Public DeviceControlOnOff As ArrayList
        Public DeviceControlOffOff As ArrayList

        Public DeviceControlString As String
        Public DeviceControlSlottedOn As Boolean
        Public DeviceControlSlottedOff As Boolean
        Public SecondaryIndFound As Boolean

        Public DeviceControlSlottedOnOff As Boolean
        Public DeviceControlSlottedOffOff As Boolean

        Public AlertOnIndication As Boolean
        Public AlertVerbiage As String

        Public SlotControlOnIndication As Boolean
        Public InvertIndication As Boolean

        Public ManualOperateMode As Boolean

        Public CreatedWithControlPoint As Boolean
        'SNOW MELTOR CONTROL
        Public AllowSnowMelterTimer As Boolean
        Public SnowMelterTime As Int32
        Private dSNMZTime As Integer

        Public mnuDBCreate As MenuItem
        Public mnuDBRemove As MenuItem

        Public TemporalType As Boolean

        Public IsInterlockedWithTrack As Boolean
        Public myInterlockTrackToLeft As Track
        Public myInterlockTrackToRight As Track
        Public DisableSlotting As Boolean

        Public ScadaMode As Boolean
        Public ScadaSuppressTime As Integer = 0
        Public ScadaSuppressEndDateTime As Date = CDate("1/1/1900")
        Private ScadaAlarm As New AlarmAlert
        Public ScadaAlertMode As Boolean
        Private ScadaMessage As String = ""

        'SCADA TIMER Settings
        Public IsSCADASupperssionTmrRunning As Boolean = False
        'Public CustomSCADASupperssionEndTime As Date = Now.AddMinutes(-1)
        Public dSCADACountDownTime As Integer

        Public DisposeInProgress As Boolean = False

        Public JoinAuthAlert As Boolean
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
                pAlphaBrush = New SolidBrush(pAlphaColor)
            End Set
        End Property
        Public Property DeviceState() As Boolean
            Get
                Return pDeviceState
            End Get
            Set(ByVal Value As Boolean)
                If Not IsNothing(Me.myControlPoint) Then
                    If Me.myControlPoint.Initialized And Me.CreatedWithControlPoint Then
                        If Value <> pDeviceState Then
                            Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.LogTheDeviceEvent)
                            LogThisEvent.BeginInvoke(Device.EventTypeDevice.CPDeviceIND, Nothing, Nothing)
                        End If
                    End If
                End If
                pDeviceState = Value
            End Set

        End Property

#End Region

#Region "CONTEXT MENU"
        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)
            MouseEntered = True
            MouseLefted = False
            Me.Refresh()

        End Sub

        Private Sub RefreshContextMenu()
            Try
                If Me.ControlOnly Or Me.ControlIndicate Then
                    BuildBlockMenus()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


        Public Sub ContextMenuEnabled(ByVal EnableMenuItem As Boolean)
            Try
                Dim mi As MenuItem
                For Each mi In Me.ContextMenu.MenuItems
                    mi.Enabled = EnableMenuItem
                Next
            Catch
            End Try
        End Sub

        Public Sub BuildContextMenu()
            Try
                If Not optDisableDeviceBlockContextMenu Then
                    Me.ContextMenuControl = New ContextMenu()
                    AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                    mnuDBCreate = New MenuItem(optGraphicalCompContextMenuBlockCreate)
                    AddHandler mnuDBCreate.Click, AddressOf Me.mnuDBCreate_Click
                    ContextMenuControl.MenuItems.Add(mnuDBCreate)

                    mnuDBRemove = New MenuItem(optGraphicalCompContextMenuBlockRemove)
                    AddHandler mnuDBRemove.Click, AddressOf Me.mnuDBRemove_Click
                    ContextMenuControl.MenuItems.Add(mnuDBRemove)
                    Me.ContextMenu = Me.ContextMenuControl

                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Private Function FindDeviceBlock(ByVal BlockTime As String) As TrackAuthority.DeviceBlock
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

        Private Sub mnuDBCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                DeviceEvent("Clicked - Create Device Block from CM")
                gCreateDeviceBlock = True
                gBlockAllDevices = True
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
                Dim blk As TrackAuthority.DeviceBlock = FindDeviceBlock(BlkData(0))
                If blk Is Nothing Then Exit Sub
                DeviceEvent("Clicked - Remove Device Block from CM:" & blk.AuthorityUID)
                SystemForms.MySystemForms.frmReportClearDeviceBlock = New frmReportClearDeviceBlock()
                SystemForms.MySystemForms.frmReportClearDeviceBlock.Location = Me.CurrentDrawingForm.myLaunchPoint
                SystemForms.MySystemForms.frmReportClearDeviceBlock.LoadForm()
                If optDeviceBlockingSimpleRelease And Not blk.AuthorityType = "BLUE FLAG" Then
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.SimpleReportClearEvent(blk)
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.Dispose()
                    SystemForms.MySystemForms.frmReportClearDeviceBlock = Nothing
                Else
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.SingleReportClearEvent(blk)
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.ShowDialog()
                    SystemForms.MySystemForms.frmReportClearDeviceBlock.Dispose()
                    SystemForms.MySystemForms.frmReportClearDeviceBlock = Nothing
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub BuildBlockMenus()
            Dim dblkCollection As New ArrayList
            Try
                Dim miRemove As MenuItem = ContextMenuControl.MenuItems.Item(ContextMenuControl.MenuItems.IndexOf(mnuDBRemove))
                miRemove.MenuItems.Clear()

                If Me.DeviceBlocks.Count > 0 Then
                    Dim auth As TrackAuthority.DeviceBlock
                    miRemove.Visible = True
                    For Each auth In Me.DeviceBlocks
                        If auth.AuthorityType = "DEVICE BLOCK" Then
                            If Not dblkCollection.Contains(auth.BlockGUID) Then
                                Me.mnuDBRemove.MenuItems.Add(New MenuItem(Format(auth.AuthorityDate, "HH:mm:ss") & ", (" & auth.IssueTo & ")", AddressOf mnuDBRemove_Click))
                                dblkCollection.Add(auth.BlockGUID)
                            End If
                        End If

                    Next
                Else
                    miRemove.Visible = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                dblkCollection = New ArrayList
            End Try

        End Sub

#End Region

#Region "SELECTION MODE"

        Public ColorStatusBlocking As New SolidBrush(Color.SteelBlue)
        Public ColorStatusRouteRequesting As New SolidBrush(Color.YellowGreen)
        Public ColorSummarySelectionBlocking As New SolidBrush(Color.LightSteelBlue)
#End Region

#Region "MOUSE EVENTS"

        Public Sub Device_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
            Try

                Me.DeviceEvent("MOUSE-DOWN-DEVICE:" & Me.GUID.ToString)

                If gAssistMode OrElse Me.IsShadowMode Then Exit Sub

                If Me.myControlPoint.MaintenanceMode And Not gMaintenanceMode Then
                    AddMessageToFeedback("MISC DEVICE CONTROL NOT OPERATONAL BECAUSE THE CONTROL POINT " & Me.myControlPoint.Name & " IS CURRENTLY IN MAINTENANCE MODE")
                    Exit Sub
                End If
                Dim LeftMouseButton As Boolean = False
                If Not IsNothing(e) Then
                    If e.Button = Windows.Forms.MouseButtons.Left Then
                        LeftMouseButton = True
                    End If
                End If

                If optEnableMiscDeviceTrackInterlockMode AndAlso Me.IsInterlockedWithTrack Then
                    'If LeftMouseButton AndAlso Not IsNothing(Me.myInterlockTrackToLeft) Then
                    If Me.myInterlockTrackToLeft IsNot Nothing Then
                        If LeftMouseButton AndAlso gInitialized AndAlso TMDS.SystemUtilities.Utilities.CheckForTrackTags(Me.myInterlockTrackToLeft, Nothing, "LEFT") Then
                            ClearSystemVariables()
                            Exit Sub
                        End If
                        If Me.myInterlockTrackToLeft.BlockAbsolute Then
                            AddMessageToFeedback("MISC DEVICE CONTROL NOT OPERATONAL BECAUSE THE TRACK IS BLOCKED AT " & Me.myInterlockTrackToLeft.TrackName)
                            Exit Sub
                        ElseIf Not Me.CheckForOpposinfTraffic Then
                            AddMessageToFeedback("MISC DEVICE CONTROL NOT OPERATONAL BECAUSE THE TRACK IS IN USED " & Me.myInterlockTrackToLeft.TrackName)
                            Exit Sub
                        End If
                    End If
                    If Not IsNothing(Me.myInterlockTrackToRight) Then
                        If LeftMouseButton AndAlso gInitialized AndAlso TMDS.SystemUtilities.Utilities.CheckForTrackTags(Me.myInterlockTrackToRight, Nothing, "RIGHT") Then
                            ClearSystemVariables()
                            Exit Sub
                        End If
                        If Me.myInterlockTrackToRight.BlockAbsolute Then
                            AddMessageToFeedback("MISC DEVICE CONTROL NOT OPERATONAL BECAUSE THE TRACK IS BLOCKED AT " & Me.myInterlockTrackToRight.TrackName)
                            Exit Sub
                        ElseIf Not Me.CheckForOpposinfTraffic Then
                            AddMessageToFeedback("MISC DEVICE CONTROL NOT OPERATONAL BECAUSE THE TRACK IS IN USED " & Me.myInterlockTrackToRight.TrackName)
                            Exit Sub
                        End If
                    End If
                End If

                Dim RightMouseButton As Boolean
                If Not IsNothing(e) Then
                    If e.Button = Windows.Forms.MouseButtons.Right Then
                        RightMouseButton = True
                    End If

                End If

                If RightMouseButton And optEnableDeviceBlockMenuOnMiscDevices Then
                    ContextMenuEnabled(True)
                    RefreshContextMenu()
                Else
                    If gBlockAllDevices Then
                        Me.DoDeviceBlock()
                    Else
                        If Me.ControlOnly Then
                            If Me.DeviceBlocked Then Exit Sub
                            If Not Me.DeviceControlled Then
                                Me.DeviceControlled = True
                                Me.ForeColor = Color.Green
                            Else
                                Me.DeviceControlled = False
                                Me.ForeColor = DefaultColor
                            End If

                        ElseIf Me.ControlIndicate Then
                            If Me.DeviceBlocked Then Exit Sub
                            If Not Me.DeviceControlled Then
                                Me.DeviceControlled = True
                                Me.ForeColor = Color.Green
                                If Not Me.Indicated Then
                                    Me.FlasherMode(True)
                                Else
                                    Me.FlasherMode(False)
                                End If
                            Else
                                Me.DeviceControlled = False
                                Me.ForeColor = DefaultColor
                                If Not Me.Indicated Then
                                    Me.FlasherMode(False)
                                Else
                                    Me.FlasherMode(True)
                                End If
                            End If
                        Else
                            Exit Sub
                        End If

                        Me.ControlDevice(False)
                    End If
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub Device_MouseEnter(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = True
            MouseLefted = False
            Me.Invalidate()
            Me.Update()
        End Sub

        Public Sub Device_MouseLeave(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = False
            MouseLefted = True
            Me.Invalidate()
            Me.Update()
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

#Region "GRAPHICS"

        Public Sub DrawIt()
            Dim g As Graphics
            Try
                If optEnableBRCSpecificChecks AndAlso Me.IsDisposed Then Exit Sub
                If IsNothing(Me.Graphical) Then
                    Me.Graphical = Me.CreateGraphics
                End If

                g = Me.Graphical
                If Not FlashOn Then
                    If Me.FlashComponent Then
                        If StringTypeDevice Then
                            g.DrawString(Name, pAlphaFont, New SolidBrush(Color.Black), pAlphaBox, pStringFormat)
                        Else
                            If Me.DeviceBlocked Then
                                g.FillPath(New SolidBrush(Color.DodgerBlue), Path)
                            Else
                                g.FillPath(New SolidBrush(Color.WhiteSmoke), Path)
                            End If
                            'g.DrawPath(New Pen(Color.Yellow, 2), Path)
                        End If
                    ElseIf MouseEntered Then
                        If StringTypeDevice Then
                            g.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                            g.DrawPath(New Pen(Color.Yellow, 1), Path)
                        Else
                            g.FillPath(New SolidBrush(Me.ForeColor), Path)
                            g.DrawPath(New Pen(Color.Yellow, 2), Path)
                        End If
                    Else
                        'If Me.FailState Then
                        '    Me.ForeColor = Color.DarkGoldenrod
                        'End If

                        If StringTypeDevice Then
                            g.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                        Else
                            g.DrawPath(New Pen(Me.ForeColor, 2), Path)
                            g.FillPath(New SolidBrush(Me.ForeColor), Path)
                            Me.BringToFront()
                        End If
                    End If
                Else
                    'If Me.FailState Then
                    '    Me.ForeColor = Color.DarkGoldenrod
                    'End If
                    If StringTypeDevice Then
                        g.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                    Else
                        g.DrawPath(New Pen(Me.ForeColor, 2), Path)
                        g.FillPath(New SolidBrush(Me.ForeColor), Path)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "TRACK INTERLOCK MODE"

        Private Function CheckForOpposinfTraffic() As Boolean
            Try
                If Not IsNothing(Me.myInterlockTrackToRight) Then
                    If Me.myInterlockTrackToRight.TrafficDirection = "LEFT" Then Return False
                    If Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalInTime Then Return False
                    If Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock Then Return False
                    If Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalRequested Then Return False
                    If Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated Then Return False
                End If

                If Not IsNothing(Me.myInterlockTrackToLeft) Then
                    If Me.myInterlockTrackToLeft.TrafficDirection = "RIGHT" Then Return False
                    If Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalInTime Then Return False
                    If Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalOfficeLock Then Return False
                    If Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalRequested Then Return False
                    If Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalUnSolicated Then Return False
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Sub InitializeTrackInterlockMode()
            Try
                If Not Me.IsInterlockedWithTrack Then Exit Sub
                If Me.RightLink <> 0 Then
                    Me.myInterlockTrackToRight = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                    If Not IsNothing(Me.myInterlockTrackToRight) Then
                        Me.myInterlockTrackToRight.IsInterlockWithMiscellanousDevice = True
                        Me.myInterlockTrackToRight.myInterlockedMiscellanousDevice = Me
                    End If
                End If
                If Me.LeftLink <> 0 Then
                    Me.myInterlockTrackToLeft = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                    If Not IsNothing(Me.myInterlockTrackToLeft) Then
                        Me.myInterlockTrackToLeft.IsInterlockWithMiscellanousDevice = True
                        Me.myInterlockTrackToLeft.myInterlockedMiscellanousDevice = Me
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ProjectTrafficOnTrack()
            Try

                If Not IsNothing(Me.myInterlockTrackToRight) Then
                    If Me.DeviceControlled Then
                        Me.myInterlockTrackToRight.TrafficDirection = "RIGHT"
                        Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalCleared
                    Else
                        Me.myInterlockTrackToRight.TrafficDirection = ""
                        Me.myInterlockTrackToRight.SignalStatOnMe = Track.SignalStatus.SignalAtStop

                    End If
                    Me.myInterlockTrackToRight.ApplyTraffic()
                    Me.myInterlockTrackToRight.UpdateStatus()
                End If
                If Not IsNothing(Me.myInterlockTrackToLeft) Then
                    If Me.DeviceControlled Then
                        Me.myInterlockTrackToLeft.TrafficDirection = "LEFT"
                        Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalCleared
                    Else
                        Me.myInterlockTrackToLeft.TrafficDirection = ""
                        Me.myInterlockTrackToLeft.SignalStatOnMe = Track.SignalStatus.SignalAtStop

                    End If
                    Me.myInterlockTrackToLeft.ApplyTraffic()
                    Me.myInterlockTrackToLeft.UpdateStatus()

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

#End Region

        Private Sub DeviceEvent(ByVal EventNote As String)
            Try
                Dim DeviceEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.SystemEvent, EventNote, SystemEventQueue)
                DeviceEvent = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub ControlManually(ByVal ControlDevice As Boolean, Optional ByVal ForceControlSend As Boolean = False)
            Try
                If Me.ControlOnly Then
                    If ControlDevice Then
                        Me.DeviceControlled = ControlDevice
                        If Me.DeviceBlocked Then
                            Me.ForeColor = Color.DodgerBlue
                        Else
                            Me.ForeColor = Color.Green
                        End If

                    Else
                        Me.DeviceControlled = ControlDevice
                        Me.ForeColor = DefaultColor
                    End If
                ElseIf Me.ControlIndicate Then
                    If ControlDevice Then
                        Me.DeviceControlled = ControlDevice
                        If Me.DeviceBlocked Then
                            Me.ForeColor = Color.DodgerBlue
                        Else
                            Me.ForeColor = Color.Green
                        End If
                        If Not Me.Indicated Then
                            Me.FlasherMode(True)
                        Else
                            Me.FlasherMode(False)
                        End If
                    Else
                        Me.DeviceControlled = ControlDevice
                        Me.ForeColor = DefaultColor
                        If Not Me.Indicated Then
                            Me.FlasherMode(False)
                        Else
                            Me.FlasherMode(True)
                        End If
                    End If
                End If
                If Me.myControlPoint.CurrentlySelected Then
                    Me.ControlDevice(False)
                Else
                    Me.ControlDevice(ForceControlSend)
                End If
                LastControl = Me.DeviceControlled

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Overrides Sub FlasherMode(ByVal EnableFlasher As Boolean, Optional ByVal Fake As Boolean = False)
            Try
                Me.FlashComponent = EnableFlasher
                Me.FlashOn = EnableFlasher
                'Me.Flasher.Enabled = EnableFlasher
                'Me.Flasher.Interval = 250
                If EnableFlasher Then
                    Me.AddToFlashCollection(Me)
                    'FlashCollection.Add(Me)
                    Me.ReSetFlasherCollection()
                Else
                    If FlashCollection.Contains(Me) Then
                        'SyncLock FlashCollection
                        Me.RemoveFromFlashCollection(Me)
                        'FlashCollection.Remove(Me)
                        'End SyncLock
                    End If

                End If
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Me.Invoke(New MethodInvoker(AddressOf Me.Validate))

                Else
                    Me.Validate()
                    Me.Refresh()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub ApplyIndicationChange()
            'Check ind/intime bits and state a temp variable to reflect thier state
            Try
                Dim DeviceIndicated As Boolean

                If Me.IndicationORED Then
                    If Not IsNothing(Me.Ind(0)) And Not IsNothing(Me.Ind(1)) Then
                        If Not IsNothing(Me.Ind(2)) Then
                            If Ind(0).BitState Or Ind(1).BitState Or Ind(2).BitState Then 'CTC Track Indication Bit
                                DeviceIndicated = True
                            Else
                                DeviceIndicated = False
                            End If
                        Else
                            If Ind(0).BitState Or Ind(1).BitState Then 'CTC Track Indication Bit
                                DeviceIndicated = True
                            Else
                                DeviceIndicated = False
                            End If
                        End If
                    End If
                ElseIf Me.IndicationANDED Then
                    If Not Me.Ind(0) Is Nothing And Not Me.Ind(1) Is Nothing Then
                        If Not IsNothing(Me.Ind(2)) Then
                            If Ind(0).BitState And Ind(1).BitState And Ind(2).BitState Then 'CTC Track Indication Bit
                                DeviceIndicated = True
                            Else
                                DeviceIndicated = False
                            End If
                        End If
                    Else
                        If Ind(0).BitState And Ind(1).BitState Then 'CTC Track Indication Bit
                            DeviceIndicated = True
                        Else
                            DeviceIndicated = False
                        End If
                    End If
                Else
                    'If Not Me.Ind(2) Is Nothing Then
                    '    If Me.Ind(2).BitState = True Then
                    '        Me.devLOK.DeviceState = True
                    '        FlashDevices = True
                    '        If optEnableTractionPowerManagementSystem AndAlso Ind(2).IsAlarmType Then
                    '            GenerateAlarmAlert(Me.Ind(2))
                    '        End If
                    '    Else
                    '        Me.devLOK.DeviceState = False
                    '    End If
                    '    Me.devLOK.Indicated = Me.devLOK.DeviceState

                    '    Me.devLOK.UpdateStatus()
                    'End If

                    If Me.ScadaMode Then
                        If Not Me.Ind(0) Is Nothing Then
                            If Ind(0).BitState Then 'SCADA Indication Bit
                                DeviceIndicated = True
                                If optEnableTractionPowerManagementSystem AndAlso Ind(0).IsAlarmType Then
                                    Me.ScadaAlertMode = True
                                    GenerateAlarmAlert(Me.Ind(0), Me.PrimaryText)
                                End If
                            Else
                                If optEnableTractionPowerManagementSystem AndAlso Ind(0).IsAlarmType Then
                                    Me.ScadaAlertMode = False
                                    If Me.IsSCADASupperssionTmrRunning Then StopSCADATimer()
                                End If
                                DeviceIndicated = False
                            End If
                        End If
                    ElseIf Not Me.InvertIndication Then
                        If Not Me.Ind(0) Is Nothing Then
                            If Ind(0).BitState Then 'CTC Track Indication Bit
                                DeviceIndicated = True
                            Else
                                DeviceIndicated = False
                            End If
                        End If
                    Else
                        If Not Me.Ind(0) Is Nothing Then
                            If Ind(0).BitState Then 'CTC Track Indication Bit
                                DeviceIndicated = False
                            Else
                                DeviceIndicated = True
                            End If
                        End If
                    End If
                End If

                'Emulate a arrow when join auth for request
                If JoinAuthAlert Then
                    DeviceIndicated = True
                End If

                If DeviceIndicated Then
                    Me.DeviceState = True
                    'Me.Visible = True
                    Me.Indicated = True
                    If Me.FlashIndicate Then
                        'Me.FlashComponent = True
                        Me.FlasherMode(True)
                    ElseIf Me.ControlIndicate Then
                        If Me.Initialized Then
                            If Not Me.DeviceControlled And Not gMonitorMode Then
                                Me.FlasherMode(True)
                            Else
                                Me.FlasherMode(False)
                            End If
                        End If
                    End If

                    If Me.SecondaryText <> "NONE" Then
                        Me.Name = Me.SecondaryText
                        Me.ForeColor = Me.SecondaryColor
                    End If


                    If Not DeviceControlSlottedOff Then
                        Me.SlotControlOff(True)
                        DeviceControlSlottedOff = True
                    End If

                    If Not DeviceControlSlottedOn Then
                        Me.SlotControlOn(True)
                        DeviceControlSlottedOn = True
                    End If

                    DeviceControlSlottedOnOff = False
                    DeviceControlSlottedOffOff = False


                Else
                    DeviceControlSlottedOn = False
                    DeviceControlSlottedOff = False


                    Me.DeviceState = False
                    If Me.IndicateOnly And Not SecondaryIndFound Then
                        'Me.Visible = False
                    End If
                    Me.Indicated = False
                    If Me.ControlIndicate Then
                        If Me.DeviceControlled And Not gMonitorMode Then
                            Me.FlasherMode(True)
                        Else
                            Me.FlasherMode(False)
                        End If
                    Else
                        Me.FlasherMode(False)
                    End If

                    If Not DeviceControlSlottedOffOff Then
                        Me.SlotControlOffOff(True)
                        DeviceControlSlottedOffOff = True
                    End If

                    If Not DeviceControlSlottedOnOff Then
                        Me.SlotControlOnOff(True)
                        DeviceControlSlottedOnOff = True
                    End If

                    Me.Name = Me.PrimaryText
                    Me.ForeColor = Me.PrimaryColor
                End If


                If Me.IndicatedPreviously <> Me.Indicated Then
                    Dim LogThisEvent As New DoEventLogDevice(AddressOf Me.LogTheDeviceEvent)
                    LogThisEvent.BeginInvoke(Device.EventTypeDevice.IND, Nothing, Nothing)
                End If
                Me.IndicatedPreviously = Me.Indicated



                If Not Initialized Or TMDS.GlobalDeclarations.gMonitorMode Or Me.MatchMode Then
                    If Me.ControlOnly Or Me.ControlIndicate Then
                        If Me.Indicated Then
                            InitControl(True)
                        Else
                            InitControl(False)
                        End If
                    End If

                    Initialized = True
                End If

                If DeviceIndicated Then
                    If Me.IndicateOnly Or Me.ControlIndicate Then
                        If Me.AlertOnIndication Then
                            If Me.AlertVerbiage <> "" Then
                                TMDS.ManagementClasses.AlertUtilities.CreateAlert(Alert.AlertType.MiscDevice, AlertVerbiage, Nothing, Nothing, Nothing, Nothing, Me)
                            End If
                        End If
                    End If
                End If

                If Me.ManualOperateMode Then
                    Me.myControlPoint.ApplyMonitorModeFromDevice(Me, DeviceIndicated)
                End If

                'If Me.Name = "MO" Then
                '    Me.myControlPoint.ManualOperateMode = DeviceIndicated
                'End If

                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Refresh()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SlotControlOn(ByVal SlotFromInd As Boolean)
            Try
                If IsNothing(DeviceControlOn) Then Exit Sub
                If Me.DeviceControlOn.Count > 0 Then
                    Dim dv As Device
                    For Each dv In Me.DeviceControlOn
                        If SlotFromInd Then
                            dv.ControlManually(True, True)
                        Else
                            dv.ControlManually(True, False)
                        End If
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SlotControlOff(ByVal SlotFromInd As Boolean)
            Try
                If IsNothing(DeviceControlOff) Then Exit Sub
                If Me.DeviceControlOff.Count > 0 Then
                    Dim dv As Device
                    For Each dv In Me.DeviceControlOff
                        If SlotFromInd Then
                            dv.ControlManually(False, True)
                        Else
                            dv.ControlManually(False, False)
                        End If
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SlotControlOnOff(ByVal SlotFromInd As Boolean)
            Try
                If IsNothing(DeviceControlOn) Then Exit Sub
                If Me.DeviceControlOn.Count > 0 Then
                    Dim dv As Device
                    For Each dv In Me.DeviceControlOn
                        If SlotFromInd Then
                            dv.ControlManually(False, True)
                        Else
                            dv.ControlManually(False, False)
                        End If
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SlotControlOffOff(ByVal SlotFromInd As Boolean)
            Try
                If IsNothing(DeviceControlOffOff) Then Exit Sub
                If Me.DeviceControlOffOff.Count > 0 Then
                    Dim dv As Device
                    For Each dv In Me.DeviceControlOffOff
                        If SlotFromInd Then
                            dv.ControlManually(False, True)
                        Else
                            dv.ControlManually(False, False)
                        End If
                    Next
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitControl(ByVal ControlDevice As Boolean)
            Try
                If ControlDevice Then
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = True
                    Else
                        Exit Sub
                    End If
                Else
                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    Else
                        Exit Sub
                    End If
                End If

                If ControlDevice Then
                    Me.DeviceControlled = ControlDevice
                    Me.ForeColor = Color.Green
                Else
                    Me.DeviceControlled = ControlDevice
                    Me.ForeColor = DefaultColor
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

        Public Sub ControlDevice(Optional ByVal ForceControlSend As Boolean = False, Optional ByVal ResetControl As Boolean = False)
            Try
                If Me.DeviceControlled Then
                    If Me.SecondaryText <> "NONE" Then
                        Me.Name = Me.SecondaryText
                        Me.ForeColor = Me.SecondaryColor
                        If Me.InvokeRequired Then
                            Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                        Else
                            Me.Validate()
                        End If

                    End If

                    If Not DeviceControlSlottedOn Then
                        Me.SlotControlOn(False)
                        DeviceControlSlottedOn = True
                    End If

                    If Not DeviceControlSlottedOff Then
                        Me.SlotControlOff(ForceControlSend)
                        DeviceControlSlottedOff = True
                    End If

                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = True
                    Else
                        Exit Sub
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = True
                    End If

                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = True
                    End If

                    If Me.AllowSnowMelterTimer Then
                        StartSnowMelterTimer()
                        ForceControlSend = True
                    End If


                    DeviceControlSlottedOnOff = False
                    DeviceControlSlottedOffOff = False

                Else
                    DeviceControlSlottedOn = False
                    DeviceControlSlottedOff = False

                    Me.Name = Me.PrimaryText
                    Me.ForeColor = Me.PrimaryColor
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Else
                        Me.Refresh()
                    End If


                    If Not DeviceControlSlottedOnOff Then
                        Me.SlotControlOnOff(False)
                        DeviceControlSlottedOnOff = True
                    End If

                    If Not DeviceControlSlottedOffOff Then
                        Me.SlotControlOffOff(False)
                        DeviceControlSlottedOffOff = True
                    End If

                    If Not Me.Ctl(0) Is Nothing Then
                        Me.Ctl(0).BitState = False
                    Else
                        Exit Sub
                    End If

                    If Not Me.Ctl(1) Is Nothing Then
                        Me.Ctl(1).BitState = False
                    End If

                    If Not Me.Ctl(2) Is Nothing Then
                        Me.Ctl(2).BitState = False
                    End If

                    If Me.AllowSnowMelterTimer Then
                        StopSnowMelterTimer()
                    End If

                    Me.Name = Me.PrimaryText
                    Me.ForeColor = Me.PrimaryColor
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Else
                        Me.Refresh()
                    End If

                End If

                UpdateDeviceStatusToDatabase()

                If ResetControl Then
                    Exit Sub
                End If
                If Not ForceControlSend Then
                    If LastControl <> Me.DeviceControlled Then
                        Me.AddToSendCollection("ControlDevice")
                    End If
                    gSendCTC = True
                    SetCursor(curSend)
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Validate))
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Else
                        Me.Validate()
                    End If

                ElseIf Not gSendCTC Then
                    Me.DeviceEvent("DEVICE SLOTTED AND CONTROL SENT:" & Me.GUID.ToString)
                    Me.FindCodeStationToSend()
                    Me.myControlPoint.SendCodeStationControls()
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Validate))
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))

                    Else
                        Me.Validate()
                        Me.Refresh()
                    End If
                Else
                    If LastControl <> Me.DeviceControlled Then
                        Me.AddToSendCollection("ControlDevice1")
                    End If
                    gSendCTC = True
                    SetCursor(curSend)
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Validate))
                        Me.Validate()
                    Else
                        Me.Validate()
                    End If
                End If
                Me.UpdateStatus()


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'If CurrentDrawingForm.Initializing Then Exit Sub

                If Not FlashOn Then
                    If Me.FlashComponent Then
                        If StringTypeDevice Then
                            If Me.DeviceBlocked Then
                                'Dim AlphaFont As Font = New Font("Microsoft Sans Serif", 10.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Color.DodgerBlue), pAlphaBox, pStringFormat)
                            Else
                                'e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Color.Black), pAlphaBox, pStringFormat)
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                            End If
                        Else
                            If Me.DeviceBlocked Then
                                'e.Graphics.DrawPath(New Pen(Color.DodgerBlue, 2), Path)
                                e.Graphics.FillPath(New SolidBrush(Color.DodgerBlue), Path)
                            Else

                                e.Graphics.FillPath(New SolidBrush(Color.WhiteSmoke), Path)
                                'e.Graphics.DrawPath(New Pen(Color.Yellow, 2), Path)
                            End If
                        End If
                    ElseIf MouseEntered Then
                        If StringTypeDevice Then
                            e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                            e.Graphics.DrawPath(New Pen(Color.Yellow, 1), Path)
                        Else
                            If Me.DeviceBlocked Then
                                e.Graphics.DrawPath(New Pen(Color.DodgerBlue, 2), Path)
                                e.Graphics.FillPath(New SolidBrush(Color.DodgerBlue), Path)
                            Else
                                e.Graphics.FillPath(New SolidBrush(Me.ForeColor), Path)
                                e.Graphics.DrawPath(New Pen(Color.Yellow, 2), Path)
                            End If
                        End If
                    Else
                        'If Me.FailState Then
                        '    Me.ForeColor = Color.DarkGoldenrod
                        'End If

                        If StringTypeDevice Then
                            If Me.DeviceBlocked Then
                                'Dim AlphaFont As Font = New Font("Microsoft Sans Serif", 10.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Color.DodgerBlue), pAlphaBox, pStringFormat)
                            Else
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                            End If

                        Else
                            If Me.DeviceBlocked Then
                                e.Graphics.DrawPath(New Pen(Color.DodgerBlue, 2), Path)
                                e.Graphics.FillPath(New SolidBrush(Color.DodgerBlue), Path)
                            Else
                                e.Graphics.DrawPath(New Pen(Me.ForeColor, 2), Path)
                                e.Graphics.FillPath(New SolidBrush(Me.ForeColor), Path)
                            End If
                        End If
                    End If
                Else
                    'If Me.FailState Then
                    '    Me.ForeColor = Color.DarkGoldenrod
                    'End If
                    If StringTypeDevice Then
                        If Me.DeviceBlocked Then
                            'Dim AlphaFont As Font = New Font("Microsoft Sans Serif", 10.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
                            e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Color.DodgerBlue), pAlphaBox, pStringFormat)
                        Else
                            If Me.SelectionMode = ModeSelection.DeviceBlocking Then
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Color.DodgerBlue), pAlphaBox, pStringFormat)
                            Else
                                e.Graphics.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), pAlphaBox, pStringFormat)
                            End If
                        End If
                    Else
                        e.Graphics.DrawPath(New Pen(Me.ForeColor, 2), Path)
                        e.Graphics.FillPath(New SolidBrush(Me.ForeColor), Path)
                    End If
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub Draw(ByRef g As Graphics)
            Me.Invalidate()
            Me.Update()
        End Sub

        Private Sub FlashElapsedEvent(ByVal source As Object, ByVal e As EventArgs)
            Flasher.Enabled = False
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            Try
                Me.Visible = False
                'Set the position of the Control Drawing Area
                'Me.Left = LX
                'Me.Top = LY
                If Me.StringTypeDevice Then
                    Me.Left = LX - pStringSize.Width / 2
                    Me.Top = LY - pStringSize.Height / 2
                Else
                    Me.Left = LX
                    Me.Top = LY
                    Me.Width = 22
                    Me.Height = 22
                End If

                Select Case Me.TypeOfDevice
                    Case "IndicateOnly"
                        IndicateOnly = True
                    Case "ControlOnly"
                        Me.ControlOnly = True
                    Case "ControlIndicate"
                        Me.ControlIndicate = True

                    Case "ArrowRightTrafficFR", "ArrowLeftTrafficFR"
                        Me.ControlOnly = True
                        Me.TrackReference = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(Me.TrackGUID))

                        If Not Me.TrackReference Is Nothing Then
                            Me.TrackReference.FieldTrafficOn = True
                            If Me.TypeOfDevice = "ArrowRightTrafficFR" Then
                                Me.TrackReference.TrafficRightList = New ArrayList()
                                Me.TrackReference.TrafficRightList.Add(Me)
                            ElseIf Me.TypeOfDevice = "ArrowLeftTrafficFR" Then
                                Me.TrackReference.TrafficLeftList = New ArrayList()
                                Me.TrackReference.TrafficLeftList.Add(Me)
                            End If
                        End If

                    Case "ArrowRightTrafficFD", "ArrowLeftTrafficFD"
                        Me.ControlOnly = True
                        Me.TrackReferences = New ArrayList()
                        Me.myTrackList = New ArrayList()

                        Dim tracks() As String = Me.TrackGUID.Split(";")
                        Dim trk As String
                        For Each trk In tracks
                            If IsNumeric(trk) Then
                                Dim thisTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(trk))
                                If Not IsNothing(thisTrack) Then
                                    Me.myTrackList.Add(thisTrack)
                                End If
                            End If
                        Next
                        Dim trak As Track

                        For Each trak In Me.myTrackList
                            trak.FieldDurmantTrafficOn = True
                            If Me.TypeOfDevice = "ArrowRightTrafficFD" Then
                                trak.TrafficRightList = New ArrayList()
                                trak.TrafficRightList.Add(Me)
                                trak.TrafficRightTrackList = Me.myTrackList

                            ElseIf Me.TypeOfDevice = "ArrowLeftTrafficFD" Then
                                trak.TrafficLeftList = New ArrayList()
                                trak.TrafficLeftTrackList = Me.myTrackList
                                trak.TrafficLeftList.Add(Me)
                            End If
                            TrackReferences.Add(trak)
                        Next

                    Case Else
                        If Not Me.IndicateOnly And Not Me.ControlOnly Then
                            Me.ControlIndicate = True
                        End If
                End Select


                If Me.ControlOnly Or Me.ControlIndicate Then
                    Me.Visible = True
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub RefreshPath()

            Path = New GraphicsPath()
            Dim g As Graphics = Me.CreateGraphics
            Dim pt1 As Point
            Dim pt2 As Point
            Dim pt3 As Point
            Dim pt4 As Point
            Dim pt5 As Point
            Dim pt6 As Point
            Dim pt7 As Point

            Try
                Select Case Me.TypeOfDevice
                    Case "ControlOnly", "IndicateOnly", "ControlIndicate"
                        pStringFormat = New StringFormat()
                        If optEnableFontStringsForDevices And Me.FontString <> "" Then
                            Dim FontStr() As String = Me.FontString.Split(";")
                            Dim Fstyle As FontStyle
                            If FontStr.Length = 3 Then
                                Select Case FontStr(1)
                                    Case "Regular"
                                        Fstyle = FontStyle.Regular
                                    Case "Bold"
                                        Fstyle = FontStyle.Bold
                                    Case Else
                                        Fstyle = FontStyle.Regular
                                End Select
                                If CSng(FontStr(2)) >= 11 Then
                                    Me.pAlphaFont = New Font(FontStr(0), CSng(FontStr(2)), Fstyle, GraphicsUnit.Pixel, CType(0, Byte))
                                End If
                            End If
                            'Microsoft Sans Serif;Regular;8.25         
                        Else
                            If myFontForceToBold Then
                                Me.pAlphaFont = New Font("Microsoft Sans Serif", 11.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
                            End If
                        End If
                        pStringFormat.Alignment = StringAlignment.Center
                        pStringFormat.LineAlignment = StringAlignment.Center
                        If Me.SecondaryText <> "" And Me.SecondaryText <> "NONE" Then
                            If Me.PrimaryText.Length > Me.SecondaryText.Length Then
                                pStringSize = g.MeasureString(Name, AlphaFont, 500)
                            Else
                                pStringSize = g.MeasureString(Me.SecondaryText, AlphaFont, 500)
                            End If
                        Else
                            pStringSize = g.MeasureString(Name, AlphaFont, 500)
                        End If

                        pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                        pAlphaBox = New RectangleF(New PointF(0.0, 0.0), pAlphaBoxSize)
                        Path.AddRectangle(pAlphaBox)
                        Me.Left = LX - pStringSize.Width / 2
                        Me.Top = LY - pStringSize.Height / 2
                        Me.Width = pStringSize.Width + 2
                        Me.Height = pStringSize.Height + 2
                        'Me.Region = New Region(Path)
                        StringTypeDevice = True
                    Case "ArrowRight", "ArrowRightTrafficFR", "ArrowRightTrafficFD"
                        pt1 = New Point(0, 5)
                        pt2 = New Point(7, 5)
                        pt3 = New Point(7, 0)
                        pt4 = New Point(18, 6)
                        pt5 = New Point(7, 12)
                        pt6 = New Point(7, 8)
                        pt7 = New Point(0, 8)
                        Path.StartFigure()
                        Path.AddLine(pt1, pt2)
                        Path.AddLine(pt2, pt3)
                        Path.AddLine(pt3, pt4)
                        Path.AddLine(pt4, pt5)
                        Path.AddLine(pt5, pt6)
                        Path.AddLine(pt6, pt7)
                        Path.AddLine(pt7, pt1)
                        Path.CloseFigure()
                        Me.Region = New Region(Path)
                    Case "ArrowLeft", "ArrowLeftTrafficFR", "ArrowLeftTrafficFD"
                        pt1 = New Point(18, 5)
                        pt2 = New Point(11, 5)
                        pt3 = New Point(11, 0)
                        pt4 = New Point(0, 6)
                        pt5 = New Point(11, 12)
                        pt6 = New Point(11, 8)
                        pt7 = New Point(18, 8)
                        Path.StartFigure()
                        Path.AddLine(pt1, pt2)
                        Path.AddLine(pt2, pt3)
                        Path.AddLine(pt3, pt4)
                        Path.AddLine(pt4, pt5)
                        Path.AddLine(pt5, pt6)
                        Path.AddLine(pt6, pt7)
                        Path.AddLine(pt7, pt1)
                        Path.CloseFigure()
                        Me.Region = New Region(Path)

                    Case "ArrowUp"
                        pt1 = New Point(5, 18)
                        pt2 = New Point(5, 11)
                        pt3 = New Point(0, 11)
                        pt4 = New Point(6, 0)
                        pt5 = New Point(12, 11)
                        pt6 = New Point(8, 11)
                        pt7 = New Point(8, 18)
                        Path.StartFigure()
                        Path.AddLine(pt1, pt2)
                        Path.AddLine(pt2, pt3)
                        Path.AddLine(pt3, pt4)
                        Path.AddLine(pt4, pt5)
                        Path.AddLine(pt5, pt6)
                        Path.AddLine(pt6, pt7)
                        Path.AddLine(pt7, pt1)
                        Path.CloseFigure()
                        Me.Region = New Region(Path)

                    Case "ArrowDown"
                        pt1 = New Point(5, 0)
                        pt2 = New Point(5, 7)
                        pt3 = New Point(0, 7)
                        pt4 = New Point(6, 18)
                        pt5 = New Point(12, 7)
                        pt6 = New Point(8, 7)
                        pt7 = New Point(8, 0)
                        Path.StartFigure()
                        Path.AddLine(pt1, pt2)
                        Path.AddLine(pt2, pt3)
                        Path.AddLine(pt3, pt4)
                        Path.AddLine(pt4, pt5)
                        Path.AddLine(pt5, pt6)
                        Path.AddLine(pt6, pt7)
                        Path.AddLine(pt7, pt1)
                        Path.CloseFigure()
                        Me.Region = New Region(Path)
                End Select

                ' Me.BringToFront()
                If IsNumeric(Me.TextColor) Then
                    Me.ForeColor = Color.FromArgb(CInt(Me.TextColor))
                    DefaultColor = Color.FromArgb(CInt(Me.TextColor))
                Else
                    Me.ForeColor = Me.pAlphaColor
                    DefaultColor = Me.pAlphaColor
                    'Me.TextColor = Me.ForeColor.ToArgb.ToString
                End If
                If Me.SecondaryIndFound Then
                    Me.Visible = True
                    If Me.InvokeRequired Then
                        Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                    Else
                        Me.Refresh()
                    End If

                End If
                'Me.Visible = True
                Me.BringToFront()

                'build paths for bitmap imaging
                Me.GraphicalPathRelative = Path.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.LX, Me.LY)
                GraphicalPathRelative.Transform(mat)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            If Me.InvokeRequired Then
                Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
            Else
                Me.Refresh()
            End If

        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)
            Try
                If InStr(Me.TypeOfDevice.ToUpper, "ARROW") > 0 And (Me.IndicateOnly) Then
                    Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "Visible", DeviceState)
                    'do nothing
                ElseIf Me.ControlIndicate Or Me.ControlOnly Then
                    Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "Visible", True)

                ElseIf Me.DeviceState <> Me.Visible And Not Me.ControlIndicate Then
                    Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "Visible", DeviceState)
                    If Not Me.SecondaryIndFound Then
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "Visible", DeviceState)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "Visible", True)
                    End If
                End If
                If Me.InvokeRequired Then
                    Me.Invoke(New MethodInvoker(AddressOf Me.Invalidate))
                    Me.Invoke(New MethodInvoker(AddressOf Me.Refresh))
                Else
                    Me.Invalidate()
                End If

                If optEnableMiscDeviceTrackInterlockMode AndAlso Me.IsInterlockedWithTrack Then
                    ProjectTrafficOnTrack()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub BuildGraphics()
            Try
                InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Me.Graphical = Me.CreateGraphics
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub MeasureStringSize()
            Try
                'SyncLock Graphical
                pStringSize = Graphical.MeasureString(Name, AlphaFont, 100)
                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                'End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Sub DrawRectAroundName()
            Try
                'SyncLock Graphical
                Graphical.DrawRectangle(New Pen(Color.Red, 1), 0.0F, 0.0F, pStringSize.Width, pStringSize.Height)
                'End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Overloads Sub Draw(ByVal CurrentBrush As SolidBrush)
            Try
                'SyncLock Graphical
                Graphical.DrawString(Name, pAlphaFont, CurrentBrush, pAlphaBox, pStringFormat)
                'End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim gr As Graphics
            Try
                If Not Me.Visible Then Exit Sub
                gr = Graphics.FromImage(bp)
                Dim thisAlphaBox As RectangleF = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height / 2), pAlphaBoxSize)
                If StringTypeDevice Then
                    gr.DrawString(Name, pAlphaFont, New SolidBrush(Me.ForeColor), thisAlphaBox, pStringFormat)
                Else
                    gr.DrawPath(New Pen(Me.ForeColor, 2), Me.GraphicalPathRelative)
                    gr.FillPath(New SolidBrush(Me.ForeColor), Me.GraphicalPathRelative)
                End If
                gr.Dispose()
                gr = Nothing
            Catch ex As Exception
                gr = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub UpdateDeviceStatusToDatabase()
            Try
                Dim DeviceUpdateSQL As String
                DeviceUpdateSQL = "UPDATE tblCompMiscDevices"
                DeviceUpdateSQL = DeviceUpdateSQL & " SET ControlStatus = " & CInt(Me.DeviceControlled)
                DeviceUpdateSQL = DeviceUpdateSQL & " WHERE UID = '" & GUID & "'"
                SystemDatabaseCommandToStatic("Device:UpdateDeviceStatusToDatabase", DeviceUpdateSQL)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#Region " SNOW MELTOR Timer "


        Private Sub StartSnowMelterTimer()
            Try
                If Me.AllowSnowMelterTimer Or optEnableCustomSnowMelterTimer Then
                    dSNMZTime = Me.SnowMelterTime * 60
                    tmrSnowMelter.Enabled = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub StopSnowMelterTimer()
            Try
                If Me.AllowSnowMelterTimer Or optEnableCustomSnowMelterTimer Then
                    tmrSnowMelter.Enabled = False
                    dSNMZTime = Me.SnowMelterTime * 60
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub AttachSnMZTimer()
            Try
                If Me.AllowSnowMelterTimer Or optEnableCustomSnowMelterTimer Then
                    Me.tmrSnowMelter = New Windows.Forms.Timer()
                    tmrSnowMelter.Interval = 1000
                    AddHandler tmrSnowMelter.Tick, AddressOf dSnowMelterCountDown
                    dSNMZTime = Me.SnowMelterTime * 60
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub dSnowMelterCountDown(ByVal source As Object, ByVal e As EventArgs)
            dSNMZTime = dSNMZTime - 1
            If dSNMZTime <= 0 Then
                Me.DeviceControlled = False
                Me.ControlDevice(True)
                Me.FlasherMode(False)
            End If

        End Sub

#End Region

#Region "EVENT LOGGING"

        Public Enum EventTypeDevice
            IND
            CTL
            CPDeviceIND
            CPDeviceCTL
        End Enum

        Private Delegate Sub DoEventLogDevice(ByVal EventType As EventTypeDevice)

        Public Sub LogTheDeviceEvent(ByVal EventType As EventTypeDevice)
            Try
                Dim CurUser As String = TMDS.GlobalDeclarations.gCurrentUser.LastName & "," & TMDS.GlobalDeclarations.gCurrentUser.FirstName
                Dim CurWrkstn As Integer = CInt(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim CurTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(Me.TerritoryID)
                'Dim LogString As String = "INSERT INTO tblComponentEvents "
                Dim LogString As String = "INSERT INTO tblSystemEvents "
                'Dim SqlFields As String = "(EventTime, Territory, Wrkstn, SysUser, DbVer, FuncType, UID, Event, SysData)"
                Dim SqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim SqlValues As String
                Dim c As String = ", "
                Dim q As String = "'"
                SqlValues = " VALUES (" & q & Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & q & c & Me.TerritoryID & c & CurWrkstn & c & q & CurUser & q & c
                SqlValues = SqlValues & q & CurTerr.Dbversion & q & c & q & "MISC-DEVICE" & q & c & q & Me.GUID.ToString & q & c

                Select Case EventType
                    Case EventTypeDevice.CTL
                        If Me.DeviceControlled Then
                            SqlValues = SqlValues & q & "CTL-ON" & q & c & q & Me.myControlPoint.Name & ":" & Me.Name & q
                        Else
                            SqlValues = SqlValues & q & "CTL-OFF" & q & c & q & Me.myControlPoint.Name & ":" & Me.Name & q
                        End If
                        SqlValues = SqlValues & ")"
                    Case EventTypeDevice.IND
                        If Me.Indicated Then
                            SqlValues = SqlValues & q & "IND-ON" & q & c & q & Me.myControlPoint.Name & ":" & Me.Name & q
                        Else
                            SqlValues = SqlValues & q & "IND-OFF" & q & c & q & Me.myControlPoint.Name & ":" & Me.Name & q
                        End If
                        SqlValues = SqlValues & ")"
                    Case EventTypeDevice.CPDeviceIND
                        If Me.Indicated Then
                            SqlValues = SqlValues & q & "CPDIND-ON" & q & c & q & Me.Tag & q
                        Else
                            SqlValues = SqlValues & q & "CPDIND-OFF" & q & c & q & Me.Tag & q
                        End If
                        SqlValues = SqlValues & ")"
                    Case EventTypeDevice.CPDeviceCTL
                        If Me.DeviceControlled Then
                            SqlValues = SqlValues & q & "CPDCTL-ON" & q & c & q & Me.Tag & q
                        Else
                            SqlValues = SqlValues & q & "CPDCTL-OFF" & q & c & q & Me.Tag & q
                        End If
                        SqlValues = SqlValues & ")"
                End Select
                TMDS.GlobalDeclarations.gSystemEventLogQueue.Enqueue(LogString & SqlFields & SqlValues)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

        'Private Sub GenerateAlarmAlert(bp As BitPosition)
        '    Try
        '        Dim MessageAlert As String = "SCADA ALARM DETECTED"
        '        Dim Alarm As New AlarmAlert
        '        Alarm.myControlPoint = Me.myControlPoint
        '        Select Case bp.AlarmLevel
        '            Case "CRITICAL"
        '                Alarm.myAlertLevel = AlarmAlert.AlertLevel.CRITICAL
        '            Case "HIGH"
        '                Alarm.myAlertLevel = AlarmAlert.AlertLevel.HIGH
        '            Case "MEDIUM"
        '                Alarm.myAlertLevel = AlarmAlert.AlertLevel.MEDIUM
        '            Case "LOW"
        '                Alarm.myAlertLevel = AlarmAlert.AlertLevel.LOW

        '        End Select
        '        Alarm.myAleryEquipmentUID = bp.AlarmEquipmentUID
        '        Alarm.myAlertDescription = bp.AlarmDescription

        '        TMDS.ManagementClasses.AlertUtilities.CreateAlarmAlert(ManagementClasses.Alert.AlertType.AlarmAlert, MessageAlert, Alarm)

        '    Catch ex As Exception
        '        SystemException(ex.ToString)
        '    End Try

        'End Sub
        Private Sub GenerateAlarmAlert(bp As BitPosition, DeviceText As String)
            Try
                Dim MessageAlert As String = "SCADA ALARM DETECTED"
                Dim Alarm As New AlarmAlert
                Alarm.myControlPoint = Me.myControlPoint
                Select Case bp.AlarmLevel
                    Case "CRITICAL"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.CRITICAL
                    Case "HIGH"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.HIGH
                    Case "MEDIUM"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.MEDIUM
                    Case "LOW"
                        Alarm.myAlertLevel = AlarmAlert.AlertLevel.LOW
                End Select
                Alarm.myAleryEquipmentUID = bp.AlarmEquipmentUID
                Alarm.myAlertDescription = bp.AlarmDescription
                MessageAlert = MessageAlert & "-" & DeviceText.Trim.ToUpper
                If Me.ScadaSuppressTime > 0 Then
                    If Not Me.IsSCADASupperssionTmrRunning Then
                        Me.ScadaAlarm = Alarm
                        Me.ScadaMessage = MessageAlert
                        StartSCADATimer()
                    End If
                Else
                    Me.ScadaAlarm = New AlarmAlert
                    Me.ScadaMessage = ""
                    TMDS.ManagementClasses.AlertUtilities.CreateAlarmAlert(ManagementClasses.Alert.AlertType.AlarmAlert, MessageAlert, Alarm)
                End If




            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

#Region " SCADA SUPPRESSION TIMER "
        Private Sub StartSCADATimer()
            Try
                If Me.ScadaSuppressTime > 0 AndAlso Not Me.IsSCADASupperssionTmrRunning Then
                    Me.IsSCADASupperssionTmrRunning = True
                    dSCADACountDownTime = Me.ScadaSuppressTime * 60
                    tmrSCADASupperssion.Enabled = True
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Sub StopSCADATimer()
            Try
                tmrSCADASupperssion.AutoReset = True
                tmrSCADASupperssion.Enabled = False
                dSCADACountDownTime = Me.ScadaSuppressTime * 60
                IsSCADASupperssionTmrRunning = False
                Me.ScadaMessage = ""
                Me.ScadaAlarm = New AlarmAlert
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub AttachSCADATimer()
            Try
                If Me.ScadaSuppressTime > 0 Then
                    tmrSCADASupperssion = New System.Timers.Timer ' New Windows.Forms.Timer()
                    tmrSCADASupperssion.Interval = 1000
                    AddHandler tmrSCADASupperssion.Elapsed, AddressOf Me.dSCADACountDown
                    dSCADACountDownTime = Me.ScadaSuppressTime * 60
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Sub dSCADACountDown(ByVal source As Object, ByVal e As EventArgs)
            Try
                dSCADACountDownTime = dSCADACountDownTime - 1
                If dSCADACountDownTime <= 0 Then
                    If Me.ScadaAlertMode Then
                        TMDS.ManagementClasses.AlertUtilities.CreateAlarmAlert(ManagementClasses.Alert.AlertType.AlarmAlert, Me.ScadaMessage, Me.ScadaAlarm)
                    End If
                    StopSCADATimer()
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

    End Class
#End Region

#Region "SCREEN REGION"

    Public Class ScreenDrawingRegion

        Private pBounds As Rectangle
        Private pGraphicPath As GraphicsPath
        Private pBoundsInUse As Boolean
        Private pMapFromRegion As Integer
        Public TerrID As Int32
        Public RegionNumber As Integer
        'Public MapFromRegion As Integer

        Public Property Bounds() As Rectangle
            Get
                Return pBounds
            End Get
            Set(ByVal Value As Rectangle)
                pBounds = Value
            End Set

        End Property
        Public Property BoundsInUse() As Boolean
            Get
                Return pBoundsInUse
            End Get
            Set(ByVal Value As Boolean)
                pBoundsInUse = Value
            End Set

        End Property
        Public Property GraphicPath() As GraphicsPath
            Get
                Return pGraphicPath
            End Get
            Set(ByVal Value As GraphicsPath)
                pGraphicPath = Value
            End Set

        End Property
        Public Property MapFromRegion() As Integer
            Get
                Return pMapFromRegion
            End Get
            Set(ByVal Value As Integer)
                pMapFromRegion = Value
            End Set

        End Property

        Sub SetBounds(ByVal LeftUpwardsPoint As Point, ByVal RectSize As Size)
            pBounds = New Rectangle(LeftUpwardsPoint, RectSize)
            Dim gPath As New GraphicsPath()
            gPath.AddRectangle(pBounds)
            pGraphicPath = gPath
            ''MsgBox(gPath.GetBounds.ToString())
        End Sub

        Function SetBoundsUsage(ByVal PointToCheck As Point) As Boolean
            If Not pBoundsInUse Then
                If pGraphicPath.IsVisible(PointToCheck) Then
                    pBoundsInUse = True
                    SetBoundsUsage = True
                End If
            End If

        End Function

        Function IsInBounds(ByVal PointToCheck As Point) As Boolean
            If pGraphicPath.IsVisible(PointToCheck) Then
                pBoundsInUse = True
                IsInBounds = True
            End If
        End Function

    End Class
#End Region

#Region "ALPHA TEXT"

    Public Class AlphaText
        Inherits GenericComponent

#Region "Fields"

        Private pAlphaFont As Font = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
        Private pAlphaBox As RectangleF
        Private pAlphaLocation As Point
        Private pAlphaBoxSize As SizeF
        Private pAlphaColor As Color = Color.White
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

        Overrides Sub InitializeGraphicsFromScreenCoordinates()

            Me.Left = LX - pStringSize.Width / 2
            Me.Top = LY - pStringSize.Height / 2

        End Sub

        Overrides Sub RefreshPath()
            Path = New GraphicsPath()
            Try
                If TMDS.GlobalDeclarations.optFontAlphaTextMedium Then
                    pAlphaFont = New Font("ARIAL", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 170
                ElseIf optFontControlPointSmaller Then
                    pAlphaFont = New Font("ARIAL", 6.75!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 150

                ElseIf optFontControlPointSmall Then
                    pAlphaFont = New Font("ARIAL", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 150
                ElseIf optFontControlPointLarge Then
                    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 150
                ElseIf optFontAlphaTextUseDatabaseValue Then
                    If IsNothing(Me.pAlphaFont) Then
                        pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                        BoxSize = 100
                    Else
                        BoxSize = 250
                    End If

                Else
                    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 250
                End If
                'End If
                If TextColor <> "" Then
                    If IsNumeric(TextColor) Then
                        Me.pAlphaColor = Color.FromArgb(CInt(TextColor))
                        pAlphaBrush = New SolidBrush(pAlphaColor)
                    End If
                End If

                Dim g As Graphics = Me.CreateGraphics
                pStringFormat = New StringFormat()
                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Near
                'pStringSize = g.MeasureString(Name, StationFont, 100)
                pStringSize = g.MeasureString(Name, pAlphaFont, BoxSize)

                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                pAlphaBox = New RectangleF(New PointF(0.0, 0.0), pAlphaBoxSize)

                Path.AddRectangle(pAlphaBox)
                Me.Left = LX - pStringSize.Width / 2
                Me.Top = LY - pStringSize.Height - 1 '+ 5
                Me.Width = pStringSize.Width + 2
                Me.Height = pStringSize.Height - 1
                Me.BackColor = Color.Black
                'build paths for bitmap imaging
                Me.GraphicalPathRelative = Path.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.LX, Me.LY)
                GraphicalPathRelative.Transform(mat)

                Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            'Draw(Graphical)
            Refresh()
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'If CurrentDrawingForm.Initializing Then Exit Sub

                e.Graphics.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitializeFontString()
            Try
                Dim FontSize As Single = 12.5!
                If Trim(FontString) Is Nothing Then
                    Exit Sub
                End If
                Dim DataString() As String = Me.FontString.Split(";")

                If DataString.Length >= 3 Then
                    Dim myFontStyle As FontStyle

                    Select Case DataString(1).Trim
                        Case "Regular"
                            myFontStyle = FontStyle.Regular
                        Case "Bold"
                            myFontStyle = FontStyle.Bold
                        Case "Italic"
                            myFontStyle = FontStyle.Italic
                        Case "Underline"
                            myFontStyle = FontStyle.Underline
                        Case Else
                            myFontStyle = FontStyle.Regular
                    End Select
                    Single.TryParse(DataString(2).Trim, FontSize)
                    AlphaFont = New Font(DataString(0).Trim, FontSize, myFontStyle, GraphicsUnit.Point, CType(0, Byte))
                    'AlphaFont = New Font(DataString(0).Trim, CSng(DataString(2).Trim), myFontStyle, GraphicsUnit.Point, CType(0, Byte))

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
                pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)
            'nothing, just a dummy
        End Sub

        Overrides Sub BuildGraphics()

            InitializeGraphicsFromScreenCoordinates()
            Me.RefreshPath()
            'If TMDS.GlobalDeclarations.optFontAlphaTextMedium Then
            '    pAlphaFont = New Font("ARIAL", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 170
            'ElseIf optFontControlPointSmall Then
            '    pAlphaFont = New Font("ARIAL", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 150
            'ElseIf optFontControlPointLarge Then
            '    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 150
            'ElseIf optFontAlphaTextUseDatabaseValue Then
            '    If IsNothing(Me.pAlphaFont) Then
            '        pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '        BoxSize = 100
            '    Else
            '        BoxSize = 250
            '    End If

            'Else
            '    pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 100
            'End If
            ''End If
            'If TextColor <> "" Then
            '    If IsNumeric(TextColor) Then
            '        Me.pAlphaColor = Color.FromArgb(CInt(TextColor))
            '        pAlphaBrush = New SolidBrush(pAlphaColor)
            '    End If
            'End If

            'pStringFormat.Alignment = StringAlignment.Center
            'pStringFormat.LineAlignment = StringAlignment.Center
            'MeasureStringSize()
            'pAlphaBox = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height), pAlphaBoxSize)

        End Sub

        Sub InvalidateText()
            Try
                Dim rgn As New Region(pAlphaBox)
                Me.CurrentDrawingForm.Invalidate(rgn)
            Catch
            End Try
        End Sub

        Sub MeasureStringSize()
            SyncLock Graphical
                pStringSize = Graphical.MeasureString(Name, AlphaFont, BoxSize)
                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            End SyncLock
        End Sub

        Sub DrawRectAroundName()
            SyncLock Graphical
                Graphical.DrawRectangle(New Pen(Color.Red, 1), 0.0F, 0.0F, pStringSize.Width, pStringSize.Height)
            End SyncLock

        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                Dim AlphaFont As Font = New Font("Courier", 12.5!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))

                Dim thisStationBox As RectangleF = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height / 2 - 6), pAlphaBoxSize)
                g.SmoothingMode = SmoothingMode.HighQuality
                'g.TextContrast = 2
                ' g.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixel

                g.DrawString(Name, AlphaFont, pAlphaBrush, thisStationBox, pStringFormat)
                g.Dispose()
                g = Nothing
                ' g.DrawString(Name, pAlphaFont, Brushes.Orchid, pAlphaBox, pStringFormat)

                'Dim g As Graphics = Graphics.FromImage(bp)
                'g.FillRectangle(New SolidBrush(Color.Black), pAlphaBox)
                'g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                'g.Dispose()
            Catch ex As Exception
                g = Nothing
            End Try
        End Sub

        Overloads Sub Draw(ByRef g As Graphics)
            Try
                SyncLock Graphical
                    'InvalidateText()
                    Graphical.FillRectangle(New SolidBrush(Color.Black), pAlphaBox)
                    Graphical.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub Draw()
            Dim g As Graphics
            Try
                If Not Me.CurrentDrawingForm Is Nothing Then

                    If IsNothing(Me.Graphical) Then
                        Me.Graphical = Me.CreateGraphics
                    End If

                    g = Me.Graphical
                    g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                    g.Dispose()
                    g = Nothing
                End If
            Catch ex As Exception
                g = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "AUTHORITY POINT"

    Public Class AuthorityPoint
        Inherits GenericComponent

#Region "Fields"

        Private pAlphaFont As Font = New Font("ARIAL", 8.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
        Private pAlphaBox As RectangleF
        Private pAlphaLocation As Point
        Private pAlphaBoxSize As SizeF
        Private pAlphaColor As Color = Color.White
        Private pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)

        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()
        Private pStringSize As SizeF
        Public TextColor As String
        Public BoxSize As Int32 = 150
        Public FontString As String
        Public MilePost As Decimal

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


        Overrides Sub InitializeGraphicsFromScreenCoordinates()

            Me.Left = LX - pStringSize.Width / 2
            Me.Top = LY - pStringSize.Height / 2

        End Sub

        Overrides Sub RefreshPath()
            Path = New GraphicsPath()
            Try
                If TMDS.GlobalDeclarations.optFontAlphaTextMedium Then
                    pAlphaFont = New Font("ARIAL", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 170
                ElseIf optFontControlPointSmall Then
                    pAlphaFont = New Font("ARIAL", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 150
                ElseIf optFontControlPointLarge Then
                    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 150
                ElseIf optFontAlphaTextUseDatabaseValue Then
                    If IsNothing(Me.pAlphaFont) Then
                        pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                        BoxSize = 100
                    Else
                        BoxSize = 250
                    End If

                Else
                    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                    BoxSize = 250
                End If
                'End If
                If TextColor <> "" Then
                    If IsNumeric(TextColor) Then
                        Me.pAlphaColor = Color.FromArgb(CInt(TextColor))
                        pAlphaBrush = New SolidBrush(pAlphaColor)
                    End If
                End If

                Dim g As Graphics = Me.CreateGraphics
                pStringFormat = New StringFormat()
                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Near
                'pStringSize = g.MeasureString(Name, StationFont, 100)
                pStringSize = g.MeasureString(Name, pAlphaFont, BoxSize)

                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
                pAlphaBox = New RectangleF(New PointF(0.0, 0.0), pAlphaBoxSize)

                Path.AddRectangle(pAlphaBox)
                Me.Left = LX - pStringSize.Width / 2
                Me.Top = LY - pStringSize.Height - 1 '+ 5
                Me.Width = pStringSize.Width + 2
                Me.Height = pStringSize.Height - 1
                Me.BackColor = Color.Black
                'build paths for bitmap imaging
                Me.GraphicalPathRelative = Path.Clone
                Dim mat As New Matrix()
                mat.Translate(Me.LX, Me.LY)
                GraphicalPathRelative.Transform(mat)



                Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            'Draw(Graphical)
            Refresh()
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                e.Graphics.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub InitializeFontString()
            Try
                If Trim(FontString) Is Nothing Then
                    Exit Sub
                End If
                Dim DataString() As String = Me.FontString.Split(";")

                If DataString.Length >= 3 Then
                    Dim myFontStyle As FontStyle

                    Select Case DataString(1)
                        Case "Regular"
                            myFontStyle = FontStyle.Regular
                        Case "Bold"
                            myFontStyle = FontStyle.Bold
                        Case "Italic"
                            myFontStyle = FontStyle.Italic
                        Case "Underline"
                            myFontStyle = FontStyle.Underline
                        Case Else
                            myFontStyle = FontStyle.Regular
                    End Select
                    AlphaFont = New Font(DataString(0), CSng(DataString(2)), myFontStyle, GraphicsUnit.Point, CType(0, Byte))

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
                pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)
            'nothing, just a dummy
        End Sub

        Overrides Sub BuildGraphics()

            InitializeGraphicsFromScreenCoordinates()
            Me.RefreshPath()
            'If TMDS.GlobalDeclarations.optFontAlphaTextMedium Then
            '    pAlphaFont = New Font("ARIAL", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 170
            'ElseIf optFontControlPointSmall Then
            '    pAlphaFont = New Font("ARIAL", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 150
            'ElseIf optFontControlPointLarge Then
            '    pAlphaFont = New Font("ARIAL", 11.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 150
            'ElseIf optFontAlphaTextUseDatabaseValue Then
            '    If IsNothing(Me.pAlphaFont) Then
            '        pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '        BoxSize = 100
            '    Else
            '        BoxSize = 250
            '    End If

            'Else
            '    pAlphaFont = New Font("ARIAL", 12.5!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            '    BoxSize = 100
            'End If
            ''End If
            'If TextColor <> "" Then
            '    If IsNumeric(TextColor) Then
            '        Me.pAlphaColor = Color.FromArgb(CInt(TextColor))
            '        pAlphaBrush = New SolidBrush(pAlphaColor)
            '    End If
            'End If

            'pStringFormat.Alignment = StringAlignment.Center
            'pStringFormat.LineAlignment = StringAlignment.Center
            'MeasureStringSize()
            'pAlphaBox = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height), pAlphaBoxSize)

        End Sub

        Sub InvalidateText()
            Try
                Dim rgn As New Region(pAlphaBox)
                Me.CurrentDrawingForm.Invalidate(rgn)
            Catch
            End Try
        End Sub

        Sub MeasureStringSize()
            SyncLock Graphical
                pStringSize = Graphical.MeasureString(Name, AlphaFont, BoxSize)
                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            End SyncLock
        End Sub

        Sub DrawRectAroundName()
            SyncLock Graphical
                Graphical.DrawRectangle(New Pen(Color.Red, 1), 0.0F, 0.0F, pStringSize.Width, pStringSize.Height)
            End SyncLock

        End Sub

        Overloads Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                Dim AlphaFont As Font = New Font("Courier", 12.5!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))

                Dim thisStationBox As RectangleF = New RectangleF(New PointF(LX - pStringSize.Width / 2, LY - pStringSize.Height / 2 - 6), pAlphaBoxSize)
                g.SmoothingMode = SmoothingMode.HighQuality
                'g.TextContrast = 2
                ' g.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixel

                g.DrawString(Name, AlphaFont, pAlphaBrush, thisStationBox, pStringFormat)
                g.Dispose()
                g = Nothing
                ' g.DrawString(Name, pAlphaFont, Brushes.Orchid, pAlphaBox, pStringFormat)

                'Dim g As Graphics = Graphics.FromImage(bp)
                'g.FillRectangle(New SolidBrush(Color.Black), pAlphaBox)
                'g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                'g.Dispose()
            Catch ex As Exception
                g = Nothing
            End Try
        End Sub

        Overloads Sub Draw(ByRef g As Graphics)
            Try
                SyncLock Graphical
                    'InvalidateText()
                    Graphical.FillRectangle(New SolidBrush(Color.Black), pAlphaBox)
                    Graphical.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overloads Sub Draw()
            Dim g As Graphics
            Try
                If Not Me.CurrentDrawingForm Is Nothing Then

                    If IsNothing(Me.Graphical) Then
                        Me.Graphical = Me.CreateGraphics
                    End If

                    g = Me.Graphical
                    g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                    g.Dispose()
                    g = Nothing
                End If
            Catch ex As Exception
                g = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "AUTHORITY NUMBER"

    Public Class AuthNumberText
        Implements IDisposable

        'Private pAlphaFont As Font = New Font("Courier", 9.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private pAlphaFont As Font = New Font("Courier", 10.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Public pAlphaBox As RectangleF
        Private pAlphaLocation As Point
        Private pAlphaBoxSize As SizeF
        Private pAlphaColor As Color = Color.Yellow
        Private pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)

        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()
        Private pStringSize As SizeF

        Public Graphical As Graphics
        Public Name As String
        Public Left As Short
        Public Top As Short

        Protected Overloads Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                Me.pAlphaBrush.Dispose()
                Me.pStringFormat.Dispose()
                pAlphaFont.Dispose()
            End If
        End Sub

        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

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

        Sub InitGraphics()
            Draw(Graphical)
        End Sub

        Sub BuildGraphics()
            If optUseTwelvePointFontOnAuthority Then
                pAlphaFont = New Font("Courier", 12.0!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
            End If

            pStringFormat.Alignment = StringAlignment.Center
            pStringFormat.LineAlignment = StringAlignment.Center
            MeasureStringSize()
            pAlphaBox = New RectangleF(New PointF(Left, Top), pAlphaBoxSize)

        End Sub

        Sub MeasureStringSize()
            SyncLock Graphical
                pStringSize = Graphical.MeasureString(Name, AlphaFont, 80)
                pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            End SyncLock
        End Sub

        Sub DrawRectAroundName()
            SyncLock Graphical
                Graphical.DrawRectangle(New Pen(Color.Red, 1), 0.0F, 0.0F, pStringSize.Width, pStringSize.Height)
            End SyncLock

        End Sub

        Sub Draw(ByRef g As Graphics)
            SyncLock g
                g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
            End SyncLock

        End Sub

        Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                g.Dispose()
                g = Nothing
            Catch
                g = Nothing
            End Try
        End Sub

    End Class

    Public Class AuthNumberTextCollection

        ' The first thing to do when building a CollectionBase class is
        ' to inherit from System.Collections.CollectionBase
        Inherits System.Collections.CollectionBase

        Public Overloads Function Add(ByVal Value As AuthNumberText) As AuthNumberText

            ' After you inherit the CollectionBase class, 
            ' you can access an intrinsic object
            ' called InnerList that represents your 
            ' collection. InnerList is of type ArrayList.
            Me.InnerList.Add(Value)
            Return Value

        End Function

        Public Overloads Function Item(ByVal Index As Integer) As AuthNumberText

            ' To retrieve an item from the InnerList, pass
            ' the index of that item to the .Item property.

            Return CType(Me.InnerList.Item(Index), AuthNumberText)
        End Function

        Public Overloads Function Item(ByVal TW As AuthNumberText) As AuthNumberText

            ' To retrieve an item from the InnerList, pass
            ' the index of that item to the .Item property.

            Dim myIndex As Integer

            myIndex = Me.InnerList.IndexOf(TW)
            Return CType(Me.InnerList.Item(myIndex), AuthNumberText)

        End Function

        Public Overloads Sub Remove(ByVal TW As AuthNumberText)

            ' To remove an item from the collection, you must
            ' pass in a reference to that item (in this case, the
            ' Customer object we want to remove).

            ' However, it is often more convenient to create a
            ' Remove method that allows the calling code to pass in 
            ' only the index of the item instead of an object reference.
            ' So we've overloaded the Remove method to use either one.
            Me.InnerList.Remove(TW)
        End Sub

        Public Overloads Sub Remove(ByVal Index As Integer)

            ' This is the second Remove method. Instead of passing
            ' in an object reference, this Remove expects an index.

            ' The calling code can decide which one to call.

            ' If the calling code passes an index, you can simply
            ' look up that item by using the InnerList.Item method,
            ' then remove the item.

            Dim TW As AuthNumberText

            TW = CType(Me.InnerList.Item(Index), AuthNumberText)
            If Not TW Is Nothing Then
                Me.InnerList.Remove(TW)
            End If
        End Sub

    End Class
#End Region

#Region "ANNOTATIONS"

    Public Class Annotation
        Implements IDisposable

        Public pAlphaFont As Font = New Font("ARIAL", 10.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
        Public pAlphaBox As RectangleF
        Public pAlphaLocation As Point
        Public pAlphaBoxSize As SizeF
        Public pAlphaColor As Color = Color.Aqua
        Public pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)
        Public pAlphaRegion As Region
        Public pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Public pStringFormat As StringFormat = New StringFormat()
        Public pStringSize As SizeF

        Public Graphical As Graphics
        Public GraphicalPath As New GraphicsPath()
        Public GraphicalRegion As Region
        Public Name As String
        Public Left As Short
        Public Top As Short

        Public xFactor As Short
        Public yFactor As Short
        Public TrackGUID As Int32

        Public TerritoryID As Integer
        Public PreviousTerrID As Integer
        Public Subdivision As String
        Public NoteText As String
        Public CreateTime As Date
        Public CurrentDisplayForm As frmDisplay
        Public NoteGUID As String
        Public UserUID As String

        Sub New()
            NoteGUID = System.Guid.NewGuid.ToString.ToUpper
        End Sub

#Region "IDisposable"

        Private disposed As Boolean


        Private Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    GraphicalPath.Dispose()
                End If

                ' TODO: put code to free unmanaged resources here
            End If
            Me.disposed = True
        End Sub


        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region

        Sub AssignPointProperties(ByVal NotePoint As Point, ByRef ctrls As System.Windows.Forms.Form.ControlCollection)
            Try
                Dim xSum, ySum As Integer
                Dim xSumCurrent As Integer = 120000
                Dim ySumCurrent As Integer = 120000

                Dim xcurrent As Integer
                Dim ycurrent As Integer

                Dim trk As Object
                Dim CurrentTrk As Track = Nothing
                For Each trk In ctrls
                    If trk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        xSum = trk.LX - NotePoint.X
                        ySum = trk.LY - NotePoint.Y

                        If xSum < 0 Then xSum = xSum * -1
                        If ySum < 0 Then ySum = ySum * -1

                        If xSum <= xSumCurrent And ySum <= ySumCurrent Then
                            CurrentTrk = trk
                            xSumCurrent = xSum
                            ySumCurrent = ySum
                            xcurrent = trk.LX
                            ycurrent = trk.LY
                        End If
                    End If
                Next
                xFactor = CurrentTrk.LX - NotePoint.X
                yFactor = CurrentTrk.LY - NotePoint.Y

                Left = NotePoint.X
                Top = NotePoint.Y

                TrackGUID = CurrentTrk.GUID
                TerritoryID = CurrentTrk.TerritoryID
                Subdivision = CurrentTrk.Subdivision
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub HighLightNote(ByVal HighLight As Boolean)
            Try
                If HighLight Then
                    pAlphaFont = New Font("ARIAL", 10.5!, FontStyle.Italic, GraphicsUnit.Point, CType(0, Byte))
                    'DrawRectAroundName()
                Else
                    pAlphaFont = New Font("ARIAL", 10.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        Sub InvalidateNote(ByVal DisplayForm As frmDisplay)
            Try
                Dim rgn As New Region(pAlphaBox)
                DisplayForm.Invalidate(rgn)
                rgn = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub InitGraphics()
            Draw(Graphical)
        End Sub

        Sub BuildGraphics()
            Try
                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Center
                MeasureStringSize()
                pAlphaBox = New RectangleF(New PointF(Left, Top), pAlphaBoxSize)
                GraphicalPath.AddRectangle(pAlphaBox)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub MeasureStringSize()
            pStringSize = Graphical.MeasureString(Name, pAlphaFont, 150)
            pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            Graphical = Nothing
        End Sub

        Sub DrawRectAroundName()
            Try
                Graphical.DrawRectangle(New Pen(Color.Red, 1), Left + 1, Top - 1, pStringSize.Width, pStringSize.Height)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub Draw(ByRef g As Graphics)
            g.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
        End Sub

        Sub DrawToBitmap(ByRef bp As Bitmap)
            Dim g As Graphics
            Try
                g = Graphics.FromImage(bp)
                Dim AlphaFont As Font = New Font("Courier", 10.5!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))

                g.DrawString(Name, AlphaFont, pAlphaBrush, pAlphaBox, pStringFormat)
                g.Dispose()
                g = Nothing
            Catch ex As Exception
                g = Nothing
            End Try
        End Sub

        Public Function IsNote(ByVal e As System.Windows.Forms.MouseEventArgs) As Boolean
            Try
                If GraphicalPath.IsVisible(e.X, e.Y) Then
                    IsNote = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

#Region "EVENT LOGGING"

        Public Enum EventTypeNote
            CREATE
            DELETE
            MODIFY
        End Enum

        Private Delegate Sub DoEventLogNote(ByVal EventType As EventTypeNote)

        Public Sub LogTheNoteEvent(ByVal EventType As EventTypeNote)
            Try
                Dim CurUser As String = TMDS.GlobalDeclarations.gCurrentUser.LastName & "," & TMDS.GlobalDeclarations.gCurrentUser.FirstName
                Dim CurWrkstn As Integer = CInt(TMDS.GlobalDeclarations.gThisWorkstation.WrkStnNumber)
                Dim CurTrack As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.TrackGUID)
                Dim CurTerr As Territory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(Me.TerritoryID)
                'Dim LogString As String = "INSERT INTO tblFunctionEvents "
                Dim LogString As String = "INSERT INTO tblSystemEvents "
                'Dim SqlFields As String = "(EventTime, Territory, Wrkstn, SysUser, DbVer, FuncType, UID, Event, SysData
                Dim SqlFields As String = "([EventTime], [Territory], [Wrkstn], [SysUser], [DbVer], [FuncType], [UID], [Event], [SysData])"
                Dim SqlValues As String
                Dim c As String = ", "
                Dim q As String = "'"
                SqlValues = " VALUES (" & q & Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & q & c & Me.TerritoryID & c & CurWrkstn & c & q & CurUser & q & c
                SqlValues = SqlValues & q & CurTerr.Dbversion & q & c & q & "NOTE" & q & c & q & Me.NoteGUID & q & c

                Select Case EventType
                    Case EventTypeNote.CREATE
                        SqlValues = SqlValues & q & "CREATE" & q & c & q
                        'SqlValues = SqlValues & Me.TrackGUID & "|"
                        'SqlValues = SqlValues & CurTrack.Name & "|"
                        'SqlValues = SqlValues & Me.NoteText & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"

                    Case EventTypeNote.DELETE
                        SqlValues = SqlValues & q & "DELETE" & q & c & q
                        'SqlValues = SqlValues & Me.TrackGUID & "|"
                        'SqlValues = SqlValues & CurTrack.Name & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"

                    Case EventTypeNote.MODIFY
                        SqlValues = SqlValues & q & "MODIFY" & q & c & q
                        'SqlValues = SqlValues & Me.TrackGUID & "|"
                        'SqlValues = SqlValues & CurTrack.Name & "|"
                        'SqlValues = SqlValues & Me.NoteText & "|"
                        'SqlValues = SqlValues & q
                        'SqlValues = SqlValues & ")"
                End Select

                SqlValues = SqlValues & "|" & Me.CreateTime
                SqlValues = SqlValues & "|" & Me.NoteGUID
                SqlValues = SqlValues & "|" & Me.Name
                SqlValues = SqlValues & "|" & Me.NoteText
                SqlValues = SqlValues & "|" & Me.TrackGUID
                SqlValues = SqlValues & "|" & Me.xFactor
                SqlValues = SqlValues & "|" & Me.yFactor
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

#Region "STATUS UPDATES"

        Private Delegate Sub DoPublishNoteEvent(ByVal EventType As EventTypeNote)

        Public Sub PublishNoteEventUpdate(ByVal EventType As EventTypeNote)
            Try
                Threading.Thread.Sleep(5000)
                Dim Message As String = "DC" & Chr(6) & "NOTE;"
                Message = Message & "|" & Me.TerritoryID.ToString

                Select Case EventType
                    Case EventTypeNote.CREATE
                        Message = Message & "|" & "CREATE"
                    Case EventTypeNote.DELETE
                        Message = Message & "|" & "DELETE"
                    Case EventTypeNote.MODIFY
                        Message = Message & "|" & "UPDATE"
                End Select

                Message = Message & "|" & Me.CreateTime
                Message = Message & "|" & Me.NoteGUID
                Message = Message & "|" & Me.Name
                Message = Message & "|" & Me.NoteText
                Message = Message & "|" & Me.TrackGUID
                Message = Message & "|" & Me.xFactor
                Message = Message & "|" & Me.yFactor
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

#End Region

#Region "BITMAP PICTURE"


    Public Class BitMapPicture
        Inherits GenericComponent

#Region "FIELDS"

        Public Picture As New PictureBox()

#End Region

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                Me.BackColor = Color.Black
                e.Graphics.FillRectangle(New TextureBrush(Me.Picture.Image), New Rectangle(0, 0, Me.Picture.Size.Width, Me.Picture.Size.Height))

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()

            Me.Picture.Left = LX
            Me.Picture.Top = LY

        End Sub

        Overloads Sub BuildGraphics()
            Try
                InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()

            'Set the position of the Control Drawing Area
            Me.Location = Me.Picture.Location
            Me.Size = Me.Picture.Size
            Me.Top = Me.Picture.Top
            Me.Width = Me.Picture.Width
            Me.Visible = True
            'Me.Controls.Add(Me.Picture)

        End Sub

        Overrides Sub RefreshPath()
            Try
                Path = New GraphicsPath()
                Path.StartFigure()
                Path.AddRectangle(New Rectangle(Me.Picture.Location, Me.Picture.Size))
                Path.CloseFigure()

                'Path.AddPolygon(New Point() {pt1, pt2, pt3, pt4, pt1})

                Me.Region = New Region(New Rectangle(0, 0, Me.Picture.Size.Width, Me.Picture.Size.Height))
                'Me.Region = New Region(Path)


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub BMP_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = "House Information: "
            Try
                If Me.myToolTip IsNot Nothing AndAlso Not String.IsNullOrEmpty(Me.myToolTip) AndAlso Me.myToolTip.Trim <> "" Then
                    ToolTipMessage = ToolTipMessage & Me.myToolTip.Trim
                    Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
                End If

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

    End Class



#End Region

#Region "FED"

    Public Class FED
        Inherits GenericComponent

#Region "PROPERITIES"

        Dim pt1 As New Point()
        Dim pt2 As New Point()
        Dim pt3 As New Point()
        Dim pt4 As New Point()
        Dim pt5 As New Point()
        Dim pt6 As New Point()

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32
        Dim rectF As RectangleF

        Private StationFont As Font = New Font("COURIER", 10.5!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private StringFormat As StringFormat

        Public FEDUID As Int32
        Public TrackGUID As Int32
        Public MilePost As String = String.Empty
        Public myTrack As Track
        Public TypeOfDetector As String = String.Empty
        Public UniqueIdentifier As String = String.Empty
        Public notes As String = String.Empty
        Public EnableDisplay As Boolean
        Public IsAvalancheDetector As Boolean
#End Region

#Region "CONTEXT MENU"

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub

        Public Sub BuildContextMenu()
            Try

                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem()
                End If


                Me.ContextMenu = Me.ContextMenuControl

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Sub

#End Region

#Region "FED GRAPHICS METHODS"

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Set the position of the Control Drawing Area
            Me.Left = LX - 6
            Me.Top = LY - 2

            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            rLy = 0
            rRx = 30
            rRy = 15

            Me.Width = rRx
            Me.Height = rRy
        End Sub

        Overrides Sub RefreshPath()
            Path = New GraphicsPath()
            Try
                StringFormat = New StringFormat()
                StringFormat.Alignment = StringAlignment.Center
                StringFormat.LineAlignment = StringAlignment.Center

                rectF = New RectangleF(rLx, rLy, 30, 15)
                pt1 = New Point(0, 0)
                pt3 = New Point(20, 13)
                pt2 = New Point(20, 0)
                pt4 = New Point(-2, 13)
                Path.AddRectangle(rectF)
                Me.Region = New Region(Path)
                If InStr("AVALANCHE", Me.TypeOfDetector) <> 0 Then
                    Me.ForeColor = Color.Yellow
                    IsAvalancheDetector = True
                Else
                    Me.ForeColor = Color.PaleVioletRed
                End If
                Me.BringToFront()
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                e.Graphics.DrawPath(New Pen(Me.ForeColor, 3), Path)
                e.Graphics.DrawString(Me.ComponentType, StationFont, New SolidBrush(Me.ForeColor), rectF, StringFormat)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub FED_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                ToolTipMessage = ToolTipMessage & "(MP " & Me.MilePost & ") ( TYPE : " & Me.TypeOfDetector & ") ( ID : " & Me.UniqueIdentifier & ") ( NAME : " & Me.Name & ")"
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

    End Class
#End Region

#Region "PRINT-FAX DEVICE"

    Public Class PrintFaxDevice
        Inherits GenericComponent

#Region "Properities"

        Public Email As String = ""
        Public PhoneNumber As String
        Public IPAdd As String
        Public UniqueName As String
        Public TypeOfDevice As String
        Public NumberOfCopies As Integer

#End Region

        Sub UpdateCopyCountInDatabase()
            Try
                Dim SwitchUpdateSQL As String
                'Dim s As String = ","
                'Dim dbconn As OleDb.OleDbConnection
                'Dim CommandInsert As OleDbCommand
                SwitchUpdateSQL = "UPDATE tblSystemPrintFaxLocations"
                SwitchUpdateSQL = SwitchUpdateSQL & " SET NumOfCopies = " & CInt(Me.NumberOfCopies)
                SwitchUpdateSQL = SwitchUpdateSQL & " WHERE UID = " & GUID
                SystemDatabaseCommandToStatic("PRINTFAXDEVICE:UpdateCopyCountInDatabase", SwitchUpdateSQL)

                'dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                'CommandInsert = New OleDbCommand(SwitchUpdateSQL, dbconn)
                'CommandInsert.Connection.Open()
                'CommandInsert.ExecuteNonQuery()
                'dbconn.Close()
                'CommandInsert.Dispose()
                'CommandInsert = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "GRADE CROSSING"
    Public Class GradeCrossing
        Inherits GenericComponent

#Region "PROPERTIES"
        Public myGUID As String
        Public DOT As String = String.Empty
        Public StreetName As String = String.Empty
        Public City As String = String.Empty
        Public State As String = String.Empty
        Public ZipCode As String = String.Empty
        Public CrossingType As String = String.Empty
        Public MilePost As String = String.Empty
        Public County As String = String.Empty
        Public MilePostPrefix As String = String.Empty
        Public MilePostSuffix As String = String.Empty
        Public sTrackList As String = String.Empty
        Public myTrackList As New ArrayList
        Public pmySubdivisionUID As Integer = 0
        Public mnuCreateCrossingMalfunction As MenuItem
        Public mnuCreateCrossingWayside As MenuItem 'CMV DTP-87
        Public mnuCreateCrossingEvent As MenuItem
        Public sCrossingEdges As String
        Public CrossingEdgeHighMile As Double = 0
        Public CrossingEdgeLowMile As Double = 0
        Public CrossingEdgesCollection As New List(Of clsCrossingEdge)
        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32

        Public IsNonControlledTrack As Boolean = False
        Public MilePostAlternate As String = String.Empty
        Public MilePostAlternateSuffix As String = String.Empty
        Public MilePostAlternatePrefix As String = String.Empty
        Public AlternateSubName As String = String.Empty

        Public Property mySubdivisionUID() As Integer
            Get
                Return pmySubdivisionUID
            End Get
            Set(ByVal Value As Integer)
                pmySubdivisionUID = Value
            End Set
        End Property

        Public Sub ReferenceToTracks()
            Try
                Dim tracks() As String = sTrackList.Split("|")
                Dim t As String
                For Each t In tracks
                    If IsNumeric(t) Then
                        Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(t))
                        If Not IsNothing(trk) Then
                            myTrackList.Add(trk)
                        End If
                    End If
                Next
                Dim tk As Track
                If myTrackList IsNot Nothing AndAlso myTrackList.Count > 0 Then
                    For Each tk In myTrackList

                        If Not tk.myGradeCrossings.Contains(Me) Then tk.myGradeCrossings.Add(Me)
                        Me.SubdivisionID = tk.SubdivisionID
                    Next

                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Class clsCrossingEdge
            Public myGuid As String = ""
            Public TrackName As String = String.Empty
            Public HighMile As Double = 0
            Public LowMile As Double = 0
            Public Prefix As String = String.Empty
            Public Suffix As String = String.Empty


            Public Sub New()
                myGuid = System.Guid.NewGuid.ToString.ToUpper
            End Sub

        End Class

#End Region

#Region " GRAPHICS METHODS "

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
                'mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(Me.SubdivisionID)
                'If Not IsNothing(mySubdivision) Then
                '    Subdivision = mySubdivision.Name
                'End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Set the position of the Control Drawing Area
            Me.Left = LX
            Me.Top = LY
            Me.Width = 60
            Me.Height = (RY - LY) + 10

            rLx = 0
            rLy = 0

            rRx = 0
            rRy = RY - LY
        End Sub

        Overrides Sub RefreshPath()
            Try
                Dim crossingwidth As Integer = 20
                Dim crossingHeight As Integer = rRy

                Path = New GraphicsPath()
                Dim myPath As New System.Drawing.Drawing2D.GraphicsPath()

                Dim g As Graphics = Me.CreateGraphics
                Path.StartFigure()

                Path.AddLine(New Point(0, 0), New Point(3, 0))
                Path.AddLine(New Point(3, 0), New Point(5, 3))
                Path.AddLine(New Point(5, 3), New Point(8, 0))
                Path.AddLine(New Point(8, 0), New Point(10, 0))
                Path.AddLine(New Point(10, 0), New Point(7, 4))

                Path.AddLine(New Point(7, 4), New Point(7, crossingHeight + 4))

                Path.AddLine(New Point(7, crossingHeight + 4), New Point(10, crossingHeight + 8))
                Path.AddLine(New Point(10, crossingHeight + 8), New Point(7, crossingHeight + 8))
                Path.AddLine(New Point(7, crossingHeight + 8), New Point(5, crossingHeight + 5))
                Path.AddLine(New Point(5, crossingHeight + 5), New Point(3, crossingHeight + 8))
                Path.AddLine(New Point(3, crossingHeight + 8), New Point(0, crossingHeight + 8))
                Path.AddLine(New Point(0, crossingHeight + 8), New Point(4, crossingHeight + 4))
                Path.AddLine(New Point(4, crossingHeight + 4), New Point(4, 4))
                Path.AddLine(New Point(10, 10), New Point(0, 0))

                Path.CloseFigure()
                pDefaultPath = Path
                GraphicalPath = pDefaultPath
                Me.GraphicalPath = Path
                Me.Region = New Region(Path)

                Me.BringToFront()
                Me.ForeColor = Color.Yellow

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                If CurrentDrawingForm.Initializing Then Exit Sub

                e.Graphics.DrawPath(New Pen(Color.Yellow, 10), Path)
                e.Graphics.FillPath(New SolidBrush(Color.Yellow), Path)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub GradeCrossing_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                ToolTipMessage = ToolTipMessage & "( NAME : " & Me.StreetName.ToString & ") ( MP : " & Me.MilePost.ToString & ") ( DOT : " & DOT & ") ( CROSSING TYPE : " & Me.CrossingType.ToString & ")"
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "CONTEXT MENU"

        Public Sub mnuCrossingMalfunction_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced IsNot Nothing Then
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
                End If
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me, "CROSSING MALFUNCTION")
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuCrossingWayside_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) 'CMV DTP-87
            Try
                If TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced IsNot Nothing Then
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
                End If
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me, "WAYSIDE EQUIPMENT")
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuCrossingEvent_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced IsNot Nothing Then
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                    TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
                End If
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = New frmTroubleReportEnhanced()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Owner = Me.CurrentDrawingForm
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.CreateReport(Me, "CROSSING EVENT")
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.ShowDialog()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced.Dispose()
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub

        Public Sub BuildContextMenu()
            Try

                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp


                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem(True)
                End If

                Me.ContextMenu = Me.ContextMenuControl

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Sub

        Public Overloads Sub AddTroubleReportMenuItem(ByVal CrossingTypes As Boolean)
            Try
                If ContextMenuControl.MenuItems.Count > 0 Then
                    Dim mnuSplit3 As New MenuItem("-")
                    ContextMenuControl.MenuItems.Add(mnuSplit3)
                End If

                mnuTroubleReport = New MenuItem
                mnuTroubleReport = New MenuItem("CREATE INCIDENT REPORT")
                ContextMenuControl.MenuItems.Add(mnuTroubleReport)

                If optEnableSpecificFunctionalityForDenver Then 'Bob Varghese DTP-191 This condition was added to remove these menus for Denver
                    'CMV DTP-87
                    mnuCreateCrossingWayside = New MenuItem
                    mnuCreateCrossingWayside = New MenuItem("WAYSIDE EQUIPMENT")
                    mnuTroubleReport.MenuItems.Add(mnuCreateCrossingWayside)
                    AddHandler mnuCreateCrossingWayside.Click, AddressOf Me.mnuCrossingWayside_Click
                Else
                    mnuCreateCrossingEvent = New MenuItem
                    mnuCreateCrossingEvent = New MenuItem("CROSSING EVENT REPORT")
                    mnuTroubleReport.MenuItems.Add(mnuCreateCrossingEvent)
                    AddHandler mnuCreateCrossingEvent.Click, AddressOf Me.mnuCrossingEvent_Click

                    mnuCreateCrossingMalfunction = New MenuItem
                    mnuCreateCrossingMalfunction = New MenuItem("CROSSING MALFUNCTION REPORT")
                    mnuTroubleReport.MenuItems.Add(mnuCreateCrossingMalfunction)
                    AddHandler mnuCreateCrossingMalfunction.Click, AddressOf Me.mnuCrossingMalfunction_Click
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub
#End Region

    End Class

#End Region

#Region "SLIDE ZONE"

    Public Class SlideZone
        Inherits GenericComponent

#Region "FIELDS"

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32

        Public LeftLimitMPRange As Double
        Public RightLimitMPRange As Double
        Public MilePostSuffix As String
        Public MilePostPrefix As String
        Public TrackReferenceList As String
        Public ZoneTypeList As String
        Public ZoneUID As String
        Public RailroadUID As String
        Public LongName As String

        Const HORIZ_BOTTOM_LINE_Y As Integer = 20
        Public TextColor As String = Me.ForeColor.ToArgb.ToString
        Private pStationFont As Font = New Font("Calibri", 18.0!, FontStyle.Bold, GraphicsUnit.Pixel, CType(0, Byte))
        Private pStringSize As SizeF

        Public FontString As String
        Public myFontSize As Single
        Public myFontStyle As FontStyle
        Public myFontName As String

        Private pStationBox As RectangleF
        Private pStationLocation As Point
        Private pStationBoxSize As SizeF
        Private pStationColor As Color = Color.Black
        Private pStationBrush As SolidBrush = New SolidBrush(pStationColor)
        Private pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Private pStringFormat As StringFormat = New StringFormat()

        Private mnuAddBulletin As MenuItem

        Private pEnableDisplay As Boolean
#End Region

#Region "Properties"

        Public Property EnableDisplay() As Boolean
            Get
                Return pEnableDisplay
            End Get
            Set(ByVal Value As Boolean)
                pEnableDisplay = Value
            End Set
        End Property

        Public Property StationFont() As Font
            Get
                Return pStationFont
            End Get
            Set(ByVal Value As Font)
                pStationFont = Value
            End Set
        End Property
#End Region

#Region " GRAPHICS METHODS "

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Set the position of the Control Drawing Area
            Me.Left = LX
            Me.Top = LY

            Me.Width = (RX - LX) + 5 '10
            Me.Height = 60 '45

            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            rLy = 0
            rRx = (RX - LX)
            rRy = 0

        End Sub

        Overrides Sub RefreshPath()
            Try
                Path = New GraphicsPath()

                Dim pt1 As Point
                Dim pt2 As Point
                Dim pt3 As Point
                Dim pt4 As Point
                Dim pt5 As Point
                Dim pt6 As Point
                Dim pt7 As Point

                Dim pt8 As Point
                Dim pt9 As Point
                Dim pt10 As Point

                Dim pt11 As Point
                Dim pt12 As Point
                Dim pt13 As Point
                Dim pt14 As Point
                Dim pt15 As Point
                Dim pt16 As Point
                Dim pt17 As Point



                Dim ZoneWidth As Integer = rRx

                'First Arrow
                pt1 = New Point(5, HORIZ_BOTTOM_LINE_Y - 2)
                pt2 = New Point(5, 11)
                pt3 = New Point(0, 11)
                pt4 = New Point(6, 0)
                pt5 = New Point(12, 11)
                pt6 = New Point(8, 11)
                pt7 = New Point(8, HORIZ_BOTTOM_LINE_Y - 2)


                'Line Inbetween

                pt8 = New Point(5, HORIZ_BOTTOM_LINE_Y)
                pt9 = New Point(60, HORIZ_BOTTOM_LINE_Y)
                pt10 = New Point(60, HORIZ_BOTTOM_LINE_Y - 2)



                'Second Arrow

                pt11 = New Point(57, HORIZ_BOTTOM_LINE_Y - 2)
                pt12 = New Point(57, 11)
                pt13 = New Point(52, 11)
                pt14 = New Point(58, 0)
                pt15 = New Point(64, 11)
                pt16 = New Point(60, 11)
                pt17 = New Point(60, HORIZ_BOTTOM_LINE_Y - 2)


                pt9 = New Point(ZoneWidth, 20)
                pt10 = New Point(ZoneWidth, 18)
                pt11 = New Point(ZoneWidth - 3, 18)
                pt12 = New Point(ZoneWidth - 3, 11)
                pt13 = New Point(ZoneWidth - 8, 11)
                pt14 = New Point(ZoneWidth - 2, 0)
                pt15 = New Point(ZoneWidth + 4, 11)
                pt16 = New Point(ZoneWidth, 11)
                pt17 = New Point(ZoneWidth, 18)

                Path.StartFigure()

                Path.AddLine(pt1, pt2)
                Path.AddLine(pt2, pt3)
                Path.AddLine(pt3, pt4)
                Path.AddLine(pt4, pt5)
                Path.AddLine(pt5, pt6)
                Path.AddLine(pt6, pt7)
                Path.AddLine(pt7, pt1)

                Path.AddLine(pt1, pt8)
                Path.AddLine(pt8, pt9)
                Path.AddLine(pt9, pt10)
                Path.AddLine(pt10, pt7)

                Path.AddLine(pt11, pt12)
                Path.AddLine(pt12, pt13)
                Path.AddLine(pt13, pt14)
                Path.AddLine(pt14, pt15)
                Path.AddLine(pt15, pt16)
                Path.AddLine(pt16, pt17)
                Path.AddLine(pt17, pt11)


                Dim g As Graphics = Me.CreateGraphics
                pStringFormat = New StringFormat()
                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Center
                pStringSize = g.MeasureString(Name, StationFont, Me.Width)

                pStationBoxSize = New SizeF(pStringSize.Width, pStringSize.Height - 5)

                pStationBox = New RectangleF(New PointF(Me.Width / 2 - pStationBoxSize.Width / 2, 22), pStationBoxSize)
                Path.AddRectangle(pStationBox)

                Path.CloseFigure()

                pDefaultPath = Path
                GraphicalPath = pDefaultPath
                Me.GraphicalPath = Path
                Me.Region = New Region(Path)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                'e.Graphics.DrawPath(New Pen(Color.Red, 20), Path)
                If EnableDisplay Then
                    e.Graphics.FillPath(New SolidBrush(Color.Red), Path)
                Else
                    e.Graphics.FillPath(New SolidBrush(Color.Yellow), Path)
                End If
                e.Graphics.DrawString(Name, pStationFont, pStationBrush, pStationBox, pStringFormat)

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub SlideZone_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                Dim strZoneTypeList As String = Me.ZoneTypeList.Replace("|", " , ")

                ToolTipMessage = ToolTipMessage & "(MP " & Me.MilePostPrefix & Me.LeftLimitMPRange.ToString & " to MP " & Me.MilePostPrefix & Me.RightLimitMPRange.ToString & ") ( ZONE TYPES : " & strZoneTypeList & ")"
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

#Region "CONTEXT MENU"

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub

        Public Sub BuildContextMenu()
            Try

                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                mnuAddBulletin = New MenuItem("ADD FORM S BULLETIN")
                AddHandler mnuAddBulletin.Click, AddressOf Me.mnuAddBulletin_Click
                ContextMenuControl.MenuItems.Add(mnuAddBulletin)

                If optUseTroubleReportingEnhanced Then
                    AddTroubleReportMenuItem()
                End If


                Me.ContextMenu = Me.ContextMenuControl

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Sub

        Private Sub mnuAddBulletin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                TMDS.SystemUtilities.Utilities.SystemEvent("CLICKED ADD BULLETIN:" & Me.Name & ":" & Me.GUID.ToString)
                LaunchBulletinForm()
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

#Region "GENERAL METHODS"



#End Region

#Region "BULLETIN METHODS"

        Private Sub LaunchBulletinForm()
            Try
                Dim frmBulletinHMI As New frmBulletinHMI
                frmBulletinHMI.LoadFormSlideZone(Me)
                SetFormPropertiesGBO(frmBulletinHMI)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Sub SetFormPropertiesGBO(ByVal frmBulletinHMI As frmBulletinHMI)
            Try
                frmBulletinHMI.Owner = Me.CurrentDrawingForm
                Dim LaunchPoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + Me.CurrentDrawingForm.Width / 2, Me.CurrentDrawingForm.Top + Me.CurrentDrawingForm.Height / 2)
                frmBulletinHMI.Location = LaunchPoint
                frmBulletinHMI.ShowDialog()
                frmBulletinHMI.Dispose()
                frmBulletinHMI = Nothing
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

    End Class

#End Region

#Region "ELECTRICAL SWITCH"

    Public Class ElectricalSwitch
        Inherits GenericComponent

#Region "FIELDS"

        'Drwing points for the switch
        Dim pt1 As New Point()
        Dim pt2 As New Point()
        Dim pt3 As New Point()
        Dim pt4 As New Point()
        Dim pt5 As New Point()
        Dim pt6 As New Point()

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32
        Dim rectF As RectangleF

        Private StationFont As Font = New Font("COURIER", 10.5!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private StringFormat As StringFormat
        Private StringType As String

        Public BlockAble As Boolean = True
        Public LockIndication As Boolean = False
        Public LockBit As String
        Public SwitchBlockInd As Boolean
        Public SwitchBlockBit As String
        Public TypeOfSwitch As String
        Public FlipOperations As Boolean
        Public MatchMode As Boolean

        Public gSwitchIndicationPath As New ArrayList
        Public gSwitchStatePath As New ArrayList

        Public EnableTrackLineDisplayMode As Boolean

        Private pStringFormat As StringFormat
        Private pStringSize As SizeF

        Public MouseEntered As Boolean
        Public MouseLefted As Boolean

        Public mnuControlOpen As MenuItem
        Public mnuControlClose As MenuItem
        Public mnuInhibitControl As MenuItem

        Public mnuSimulateIndication As MenuItem
        Public mnuSimulateIndInhibit As MenuItem
        Public mnuSimulateIndLocalControl As MenuItem
        Public mnuSimulateIndLockOut As MenuItem
        Public mnuSimulateIndRackOut As MenuItem

        Public SwitchControlledClosed As Boolean

        Public myCircuitTracksToRight As New ArrayList
        Public myCircuitTracksToLeft As New ArrayList

        Public PowerFromLeftDetected As Boolean
        Public PowerFromRightDetected As Boolean

        Public PowerFromLeftDetectedFixed As Boolean
        Public PowerFromRightDetectedFixed As Boolean

        Public myCircuitToLeft As ElectricalCircuit
        Public myCircuitToRight As ElectricalCircuit

        Public DisableControl As Boolean
        Public InhibitControlFromDispatcher As Boolean

        Public pSwitchIndicatedClosed As Boolean
        Public pSwitchIndicatedLocalControl As Boolean
        Public IndInhibit As BitPosition
        Public IndLocalControl As BitPosition
        Public IndLockOut As BitPosition
        Public IndRackOut As BitPosition

        Public pSwitchIndicatedLockedOut As Boolean
        Public pSwitchIndicatedRackedOut As Boolean
        Public pSwitchIndicatedProhibited As Boolean

        Dim SwitchIndState As Object = BreakerIndStatus.None
        Dim SwitchCtrlState As Object = BreakerCtrlStatus.Open

        Private SwitchName As String = "SWITCH"

        Public myText As New TextualTag

        Public Property SwitchIndicatedClosed() As Boolean
            Get
                Return pSwitchIndicatedClosed
            End Get
            Set(ByVal Value As Boolean)
                pSwitchIndicatedClosed = Value
                DoCircuitEnergizeCheck()

                Refresh()
            End Set
        End Property

        Public Property SwitchIndicatedLocalControl() As Boolean
            Get
                Return pSwitchIndicatedLocalControl
            End Get
            Set(ByVal Value As Boolean)
                pSwitchIndicatedLocalControl = Value
                Refresh()
            End Set
        End Property

        Public Property SwitchIndicatedLockedOut() As Boolean
            Get
                Return pSwitchIndicatedLockedOut
            End Get
            Set(ByVal Value As Boolean)
                pSwitchIndicatedLockedOut = Value
                Refresh()
            End Set
        End Property

        Public Property SwitchIndicatedRackedOut() As Boolean
            Get
                Return pSwitchIndicatedRackedOut
            End Get
            Set(ByVal Value As Boolean)
                pSwitchIndicatedRackedOut = Value
                Refresh()
            End Set
        End Property

        Public Property SwitchIndicatedProhibited() As Boolean
            Get
                Return pSwitchIndicatedProhibited
            End Get
            Set(ByVal Value As Boolean)
                pSwitchIndicatedProhibited = Value
                Refresh()
            End Set
        End Property

        Enum BreakerIndStatus
            ControlInhibit
            InAccessible
            LocalControl
            LockOut
            NonReporting
            Prohibited
            RackedOut
            None
        End Enum

        Enum BreakerCtrlStatus
            Open
            Close
            RequestingOpen
            RequestingClose
            None
        End Enum

#End Region

#Region "CONTEXT MENU"

        Public Sub BuildContextMenu()
            Try
                If Me.IsShadowMode Then Exit Sub

                If TypeOfSwitch = "BREAKER" Then
                    SwitchName = "BREAKER"
                End If
                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                mnuControlClose = New MenuItem("CLOSE " & SwitchName)
                AddHandler mnuControlClose.Click, AddressOf Me.mnuControlClose_Click
                ContextMenuControl.MenuItems.Add(mnuControlClose)

                mnuControlOpen = New MenuItem("OPEN " & SwitchName)
                AddHandler mnuControlOpen.Click, AddressOf Me.mnuControlOpen_Click
                ContextMenuControl.MenuItems.Add(mnuControlOpen)

                mnuInhibitControl = New MenuItem("INHIBIT CONTROL " & SwitchName)
                AddHandler mnuInhibitControl.Click, AddressOf Me.mnuInhibitControlLocal_Click
                ContextMenuControl.MenuItems.Add(mnuInhibitControl)

                Dim sep As New MenuItem("-")
                ContextMenuControl.MenuItems.Add(sep)


                mnuSimulateIndication = New MenuItem("SIM INDICATION")
                AddHandler mnuSimulateIndication.Click, AddressOf Me.mnuSimulateIndication_Click
                ContextMenuControl.MenuItems.Add(mnuSimulateIndication)

                mnuSimulateIndInhibit = New MenuItem("SIM INHIBIT")
                AddHandler mnuSimulateIndInhibit.Click, AddressOf Me.mnuSimulateIndInhibit_Click
                ContextMenuControl.MenuItems.Add(mnuSimulateIndInhibit)

                mnuSimulateIndLocalControl = New MenuItem("SIM LOCAL CONTROL")
                AddHandler mnuSimulateIndLocalControl.Click, AddressOf Me.mnuSimulateIndLocalControl_Click
                ContextMenuControl.MenuItems.Add(mnuSimulateIndLocalControl)

                mnuSimulateIndLockOut = New MenuItem("SIM LOCK OUT")
                AddHandler mnuSimulateIndLockOut.Click, AddressOf Me.mnuSimulateIndLockOut_Click
                ContextMenuControl.MenuItems.Add(mnuSimulateIndLockOut)

                mnuSimulateIndRackOut = New MenuItem("SIM RACK OUT")
                AddHandler mnuSimulateIndRackOut.Click, AddressOf Me.mnuSimulateIndRackOut_Click
                ContextMenuControl.MenuItems.Add(mnuSimulateIndRackOut)

                Me.ContextMenu = Me.ContextMenuControl
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                mnuControlClose.Enabled = True
                mnuControlOpen.Enabled = True
                If Not DisableControl AndAlso Not InhibitControlFromDispatcher Then
                    If SwitchControlledClosed Then
                        mnuControlClose.Enabled = False
                    Else
                        mnuControlOpen.Enabled = False
                    End If
                Else
                    mnuControlClose.Enabled = False
                    mnuControlOpen.Enabled = False
                End If

                If InhibitControlFromDispatcher Then
                    mnuInhibitControl.Text = "ENABLE CONTROL " & SwitchName
                Else
                    mnuInhibitControl.Text = "INHIBIT CONTROL " & SwitchName
                End If

                If Not gSimulationMode Then
                    mnuSimulateIndication.Visible = False
                    mnuSimulateIndInhibit.Visible = False
                    mnuSimulateIndLocalControl.Visible = False
                    mnuSimulateIndLockOut.Visible = False
                    mnuSimulateIndRackOut.Visible = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuControlClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Dim ConfirmMessageList As New ArrayList
                With ConfirmMessageList
                    .Add("CLOSE THE " & SwitchName & " - (" & Me.Name & ")")
                    .Add("ARE YOU SURE?")
                End With
                If Not DoDialogForConfirmation(ConfirmMessageList) Then Exit Sub


                If Ctl(0) IsNot Nothing Then '
                    Ctl(0).BitState = True
                End If
                If Ctl(1) IsNot Nothing Then
                    Ctl(1).BitState = False
                End If

                SwitchControlledClosed = True
                RefreshPath()
                UpdateControlStateToDatabase()
                Me.FindCodeStationToSend()
                Me.myControlPoint.SendCodeStationControls()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuControlOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                Dim ConfirmMessageList As New ArrayList
                With ConfirmMessageList
                    .Add("OPEN THE " & SwitchName & " - (" & Me.Name & ")")
                    .Add("ARE YOU SURE?")
                End With
                If Not DoDialogForConfirmation(ConfirmMessageList) Then Exit Sub

                If CheckForTrackOccunancy() Then
                    ConfirmMessageList.Clear()
                    With ConfirmMessageList
                        .Add("TRACK OCCUPANCIES FOUND IN CIRCUIT FOR " & SwitchName & " - (" & Me.Name & ")")
                        .Add("CONTINUE TO OPEN SWTCH?")
                    End With
                    If Not DoDialogForConfirmation(ConfirmMessageList) Then Exit Sub
                End If
                If Ctl(0) IsNot Nothing Then '
                    Ctl(0).BitState = False
                End If
                If Ctl(1) IsNot Nothing Then
                    Ctl(1).BitState = True
                End If
                SwitchControlledClosed = False
                RefreshPath()
                UpdateControlStateToDatabase()
                Me.FindCodeStationToSend()
                Me.myControlPoint.SendCodeStationControls()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuInhibitControlLocal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try

                Dim ConfirmMessageList As New ArrayList
                With ConfirmMessageList
                    If Not InhibitControlFromDispatcher Then
                        .Add("INHIBIT THE " & SwitchName & " - (" & Me.Name & ")")
                    Else
                        .Add("ENABLE THE " & SwitchName & " - (" & Me.Name & ")")
                    End If

                    .Add("ARE YOU SURE?")
                End With
                If Not DoDialogForConfirmation(ConfirmMessageList) Then Exit Sub
                If Not InhibitControlFromDispatcher Then
                    InhibitControlFromDispatcher = True
                Else
                    InhibitControlFromDispatcher = False
                End If

                RefreshPath()
                ' UpdateControlStateToDatabase()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuSimulateIndication_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Not Me.Ind(0) Is Nothing Then
                    If Ind(0).BitState Then
                        Ind(0).BitState = False
                    Else
                        Ind(0).BitState = True
                    End If
                End If
                ApplyIndicationChange()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuSimulateIndInhibit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Not Me.IndInhibit Is Nothing Then
                    If IndInhibit.BitState Then
                        IndInhibit.BitState = False
                    Else
                        IndInhibit.BitState = True
                    End If
                End If
                ApplyIndicationChange()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuSimulateIndLocalControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Not Me.IndLocalControl Is Nothing Then
                    If IndLocalControl.BitState Then
                        IndLocalControl.BitState = False
                    Else
                        IndLocalControl.BitState = True
                    End If
                End If
                ApplyIndicationChange()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuSimulateIndLockOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Not Me.IndLockOut Is Nothing Then
                    If IndLockOut.BitState Then
                        IndLockOut.BitState = False
                    Else
                        IndLockOut.BitState = True
                    End If
                End If
                ApplyIndicationChange()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub mnuSimulateIndRackOut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If Not Me.IndRackOut Is Nothing Then
                    If IndRackOut.BitState Then
                        IndRackOut.BitState = False
                    Else
                        IndRackOut.BitState = True
                    End If
                End If
                ApplyIndicationChange()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub UpdateControlStateToDatabase()
            Dim dbconn As OleDbConnection = Nothing
            Dim dbcomm As OleDbCommand = Nothing
            Dim Update As New StringBuilder
            Try
                Update.Append("UPDATE tblCompElectricalSwitches ")
                Update.Append("SET ControlClosedByOffice = " & CInt(SwitchControlledClosed))
                Update.Append(" WHERE UID = " & Me.GUID)

                dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
                dbconn.Open()
                dbcomm = New OleDbCommand(Update.ToString, dbconn)
                dbcomm.ExecuteNonQuery()
                dbconn.Close()
                dbconn = Nothing
            Catch ex As Exception
                dbconn = Nothing
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function DoDialogForConfirmation(MessageList As ArrayList) As Boolean
            Dim frmMessage As New frmMessageBox
            Dim LaunchPoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + Me.CurrentDrawingForm.Left + (Me.CurrentDrawingForm.Width / 2) - 100, 250)
            Dim Confirmed As Boolean
            Try
                frmMessage.Owner = Me.CurrentDrawingForm
                frmMessage.SetMessage(MessageList, "CONFIRMATION", True, LaunchPoint)
                frmMessage.ShowDialog()
                If frmMessage.FormFeedBack <> 6 Then
                    SystemEvent("CLICKED NO TO CONFIRMATION PROMPT")
                Else
                    SystemEvent("CLICKED YES TO CONFIRMATION PROMPT")
                    Confirmed = True
                End If
                Return Confirmed
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try

        End Function

        Private Function CheckForTrackOccunancy() As Boolean
            Try
                Dim ct As CatenaryTrack
                For Each ct In Me.myCircuitToLeft.myCantenaryList
                    If ct.myTrack.Occuppied Then
                        Return True
                    End If
                Next
                For Each ct In Me.myCircuitToRight.myCantenaryList
                    If ct.myTrack.Occuppied Then
                        Return True
                    End If
                Next

                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return True
            End Try
        End Function
#End Region

#Region "MOUSE EVENTS"

        Public Sub Switch_MouseEnter(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = True
            MouseLefted = False
            Me.Invalidate()
            Me.Update()
        End Sub

        Public Sub Switch_MouseLeave(ByVal sender As Object, ByVal e As EventArgs)
            MouseEntered = False
            MouseLefted = True
            Me.Invalidate()
            Me.Update()
        End Sub

        Public Sub Switch_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim tt As New StringBuilder
            Try

                tt.Append("(" & Me.Name & ")")
                If InhibitControlFromDispatcher Then tt.Append(" (OFFICE INHIBITED)")
                If SwitchIndicatedLocalControl Then tt.Append(" (LOCAL CONTROL)")
                If SwitchIndicatedLockedOut Then tt.Append(" (LOCKED OUT)")
                If SwitchIndicatedProhibited Then tt.Append(" (FIELD INHIBITED)")
                If pSwitchIndicatedRackedOut Then tt.Append(" (RACKED OUT)")

                Dim sTooltip As String = tt.ToString

                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, sTooltip)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "GRAPHICS METHODS"

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                Dim thePenSize As Integer = 2
                Dim theColor As Color = Color.Red
                BackColor = Color.Black

                If Not SwitchIndicatedClosed Then
                    theColor = Color.Green
                End If
                If InhibitControlFromDispatcher Then theColor = Color.Red : thePenSize = 2
                If SwitchIndicatedLocalControl Then BackColor = Color.Gray : theColor = Color.Red : thePenSize = 2
                If SwitchIndicatedLockedOut Then BackColor = Color.Yellow : theColor = Color.Red : thePenSize = 2
                If SwitchIndicatedRackedOut Then BackColor = Color.MediumPurple : theColor = Color.Red : thePenSize = 2
                If SwitchIndicatedProhibited Then BackColor = Color.SteelBlue : theColor = Color.Red : thePenSize = 2

                Dim thePen As New Pen(theColor, thePenSize)

                If gSwitchIndicationPath.Count > 0 Then e.Graphics.DrawPath(thePen, gSwitchIndicationPath(0))
                If gSwitchStatePath.Count > 0 Then e.Graphics.DrawPath(thePen, gSwitchStatePath(0))

                If Me.MouseEntered Then
                    Dim theMousePen As New Pen(Color.WhiteSmoke, thePenSize)
                    If SwitchIndicatedLocalControl OrElse SwitchIndicatedLockedOut OrElse SwitchIndicatedRackedOut OrElse SwitchIndicatedProhibited Then theMousePen = New Pen(Color.Black, thePenSize)
                    If gSwitchIndicationPath.Count > 0 Then e.Graphics.DrawPath(theMousePen, gSwitchIndicationPath(0))
                    If gSwitchStatePath.Count > 0 Then e.Graphics.DrawPath(theMousePen, gSwitchStatePath(0))
                ElseIf Me.MouseLefted Then
                    Me.MouseLefted = False
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
                Me.Visible = True
                If EnableTrackLineDisplayMode Then
                    'myText = New TextualTag
                    myText.Name = Me.Name
                    myText.Left = Me.Left
                    myText.Top = Me.Top + Me.Height + 2
                    Me.CurrentDrawingForm.Controls.Add(myText)

                    myText.BuildGraphics()
                    myText.Refresh()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            Me.Left = LX
            Me.Top = LY
            Select Case TypeOfSwitch
                Case "BREAKER"
                    Me.Width = 27
                    Me.Height = 27
                    RX = LX + 27
                Case "MOTORIZED"
                    If EnableTrackLineDisplayMode Then
                        Me.Width = 15
                        Me.Height = 15
                        RX = LX + 15
                        Me.Left = LX + 3
                        Me.Top = LY + 2
                    Else
                        Me.Width = 27
                        Me.Height = 27
                        RX = LX + 27
                    End If
                Case "MANUAL"
                    Me.Width = 31
                    Me.Height = 16
                    RX = LX + 31
            End Select
            RY = LY

            ''Set the position of the Control Drawing Area
            'Me.Left = LX - 2
            'Me.Top = LY
            'Select Case TypeOfSwitch
            '    Case "BREAKER"
            '        Me.Width = 31
            '        Me.Height = 31
            '        RX = LX + 28
            '    Case "MOTORIZED"
            '        If EnableTrackLineDisplayMode Then
            '            Me.Width = 15
            '            Me.Height = 15
            '            RX = LX + 15
            '            Me.Left = LX + 3
            '            Me.Top = LY + 2
            '        Else
            '            Me.Width = 27
            '            Me.Height = 27
            '            RX = LX + 27
            '            Left = LX + 1
            '            Me.Top = LY
            '        End If
            '    Case "MANUAL"
            '        Me.Width = 31
            '        Me.Height = 16
            '        RX = LX + 31
            'End Select
            'RY = LY
        End Sub

        Overrides Sub RefreshPath()
            Dim gSwitchGraphicsPath As New GraphicsPath
            Try
                Select Case TypeOfSwitch
                    Case "BREAKER"
                        Dim swRect As RectangleF
                        swRect = New RectangleF(0, 0, 27, 27)
                        gSwitchGraphicsPath.AddRectangle(swRect)
                        SetBreakerGraphics()
                    Case "MOTORIZED"
                        Dim swRect As RectangleF
                        If EnableTrackLineDisplayMode Then
                            swRect = New RectangleF(0, 0, 15, 15)
                            gSwitchGraphicsPath.AddRectangle(swRect)
                            SetMotorizedGraphicsForTrackLine()
                        Else
                            swRect = New RectangleF(0, 0, 27, 27)
                            gSwitchGraphicsPath.AddRectangle(swRect)
                            SetMotorizedGraphics()
                        End If
                    Case "MANUAL"
                        Dim swRect As RectangleF
                        swRect = New RectangleF(0, 0, 31, 16)
                        gSwitchGraphicsPath.AddRectangle(swRect)
                        'SetManualGraphics()
                End Select
                Me.Region = New Region(gSwitchGraphicsPath)
                Me.BringToFront()

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        'Private Sub SetManualGraphics()
        '    Try
        '        Dim SwitchControlPath As New GraphicsPath
        '        Select Case SwitchCtrlState
        '            Case SwCtlStatus.Open

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(0, 7)
        '                pt2 = New Point(10, 7)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(10, 0)
        '                pt2 = New Point(10, 15)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(15, 0)
        '                pt2 = New Point(15, 15)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(20, 0)
        '                pt2 = New Point(20, 15)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(20, 7)
        '                pt2 = New Point(30, 7)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '            Case SwCtlStatus.Close

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(0, 7)
        '                pt2 = New Point(10, 7)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(10, 0)
        '                pt2 = New Point(10, 15)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(20, 0)
        '                pt2 = New Point(20, 15)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '                SwitchControlPath.StartFigure()
        '                pt1 = New Point(20, 7)
        '                pt2 = New Point(30, 7)
        '                SwitchControlPath.AddLine(pt1, pt2)
        '                SwitchControlPath.CloseFigure()

        '            Case SwCtlStatus.None

        '        End Select
        '        gSwitchIndicationPath.Add(SwitchControlPath)
        '    Catch ex As Exception
        '        SystemException(ex.ToString)
        '    End Try
        'End Sub

        Private Sub SetMotorizedGraphics()
            Dim tx As Integer = 0
            Dim ty As Integer = 0
            Dim tsize As Integer = 24
            Dim SwitchPath As New GraphicsPath

            Try
                gSwitchIndicationPath.Clear()
                gSwitchStatePath.Clear()




                If InhibitControlFromDispatcher Then
                    SwitchPath.StartFigure()

                    Dim rect As New Rectangle(tx, ty, tsize + 1, tsize + 1)
                    SwitchPath.AddArc(rect, -30, 180)
                    SwitchPath.AddArc(rect, -30, -180)
                    SwitchPath.CloseFigure()
                    SwitchPath.CloseFigure()
                End If
                If InhibitControlFromDispatcher Then
                    tx = 5
                    ty = 5
                    tsize = 16
                End If
                If SwitchIndicatedLocalControl OrElse SwitchIndicatedLockedOut OrElse SwitchIndicatedProhibited OrElse SwitchIndicatedRackedOut Then
                    tx = 3
                    ty = 3
                    tsize = 20
                End If

                SwitchPath.StartFigure()
                Dim rect2 As New Rectangle(tx, ty, tsize, tsize)

                If SwitchIndicatedClosed Then
                    If Me.FlipOperations Then
                        SwitchPath.AddArc(rect2, 90, 180)
                        SwitchPath.AddArc(rect2, 90, -180)
                    Else
                        SwitchPath.AddArc(rect2, 0, 180)
                        SwitchPath.AddArc(rect2, 0, -180)
                    End If
                    SwitchPath.CloseFigure()
                    'gSwitchPath.Add(SwitchPath)
                Else
                    If Me.FlipOperations Then
                        SwitchPath.AddArc(rect2, 0, 180)
                        SwitchPath.AddArc(rect2, 0, -180)
                    Else
                        SwitchPath.AddArc(rect2, 90, 180)
                        SwitchPath.AddArc(rect2, 90, -180)
                    End If
                    SwitchPath.CloseFigure()
                    'gSwitchPath.Add(SwitchPath)
                End If

                gSwitchIndicationPath.Add(SwitchPath)

                'Select Case SwitchIndState
                '    Case SwIndStatus.None

                '        tx = 0
                '        ty = 0
                '        tsize = 22

                '    Case SwIndStatus.ControlInhibit

                '        tx = 4
                '        ty = 4
                '        tsize = 14

                '        Dim SwitchStatePath As New GraphicsPath
                '        SwitchStatePath.StartFigure()
                '        Dim rect2 As New Rectangle(0, 0, 24, 24)
                '        SwitchStatePath.AddArc(rect2, -30, 180)
                '        SwitchStatePath.AddArc(rect2, -30, -180)
                '        SwitchStatePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStatePath)

                '    Case Else
                '        tx = 4
                '        ty = 4
                '        tsize = 14
                '        Dim SwitchStatePath As New GraphicsPath
                '        SwitchStatePath.StartFigure()
                '        pt1 = New Point(0, 0)
                '        pt2 = New Point(24, 0)
                '        pt3 = New Point(24, 24)
                '        pt4 = New Point(0, 24)
                '        SwitchStatePath.AddLine(pt1, pt2)
                '        SwitchStatePath.AddLine(pt2, pt3)
                '        SwitchStatePath.AddLine(pt3, pt4)
                '        SwitchStatePath.AddLine(pt4, pt1)
                '        SwitchStatePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStatePath)

                'End Select

                'Dim SwitchControlPath As New GraphicsPath
                'Select Case SwitchCtrlState
                '    Case SwCtlStatus.Open
                '        SwitchControlPath.StartFigure()
                '        Dim rect0 As New Rectangle(tx + 1, tx + 1, tsize, tsize)
                '        SwitchControlPath.AddArc(rect0, -90, 180)
                '        SwitchControlPath.AddArc(rect0, -90, -180)
                '        SwitchControlPath.CloseFigure()
                '    Case SwCtlStatus.Close
                '        SwitchControlPath.StartFigure()
                '        Dim rect1 As New Rectangle(tx + 1, tx + 1, tsize, tsize)
                '        SwitchControlPath.AddArc(rect1, 0, 180)
                '        SwitchControlPath.AddArc(rect1, 0, -180)
                '        SwitchControlPath.CloseFigure()
                '    Case SwCtlStatus.None
                '        SwitchControlPath.StartFigure()
                '        Dim rect1 As New Rectangle(tx + 1, tx + 1, tsize, tsize)
                '        SwitchControlPath.AddArc(rect1, 0, 360)
                '        SwitchControlPath.AddArc(rect1, 0, -360)
                '        SwitchControlPath.CloseFigure()
                'End Select
                'gSwitchIndicationPath.Add(SwitchControlPath)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SetMotorizedGraphicsForTrackLine()
            Try

            Catch ex As Exception

            End Try
            Dim tx As Integer = 0
            Dim ty As Integer = 0
            Dim tsize As Integer = 13
            Dim SwitchPath As New GraphicsPath

            Try
                gSwitchIndicationPath.Clear()
                gSwitchStatePath.Clear()




                If InhibitControlFromDispatcher Then
                    SwitchPath.StartFigure()

                    Dim rect As New Rectangle(tx, ty, tsize + 1, tsize + 1)
                    SwitchPath.AddArc(rect, -30, 180)
                    SwitchPath.AddArc(rect, -30, -180)
                    SwitchPath.CloseFigure()
                    SwitchPath.CloseFigure()
                End If
                If InhibitControlFromDispatcher Then
                    'tx = 5
                    'ty = 5
                    'tsize = 8
                End If
                If SwitchIndicatedLocalControl OrElse SwitchIndicatedLockedOut OrElse SwitchIndicatedProhibited OrElse SwitchIndicatedRackedOut Then
                    'tx = 3
                    'ty = 3
                    'tsize = 10
                End If

                SwitchPath.StartFigure()
                Dim rect2 As New Rectangle(tx, ty, tsize, tsize)

                If SwitchIndicatedClosed Then
                    If Me.FlipOperations Then
                        SwitchPath.AddArc(rect2, 90, 180)
                        SwitchPath.AddArc(rect2, 90, -180)
                    Else
                        SwitchPath.AddArc(rect2, 0, 180)
                        SwitchPath.AddArc(rect2, 0, -180)
                    End If
                    SwitchPath.CloseFigure()
                    'gSwitchPath.Add(SwitchPath)
                Else
                    If Me.FlipOperations Then
                        SwitchPath.AddArc(rect2, 0, 180)
                        SwitchPath.AddArc(rect2, 0, -180)
                    Else
                        SwitchPath.AddArc(rect2, 90, 180)
                        SwitchPath.AddArc(rect2, 90, -180)
                    End If
                    SwitchPath.CloseFigure()
                    'gSwitchPath.Add(SwitchPath)
                End If

                gSwitchIndicationPath.Add(SwitchPath)
                'Dim tx As Integer = 0
                'Dim ty As Integer = 0
                'Dim tsize As Integer = 0
                'Try
                '    gSwitchIndicationPath.Clear()
                '    Dim SwitchStatePath As New GraphicsPath
                '    SwitchStatePath.StartFigure()
                '    Dim rect2 As New Rectangle(0, 0, 13, 13)

                '    If SwitchIndicatedClosed Then
                '        SwitchStatePath.AddArc(rect2, 0, 180)
                '        SwitchStatePath.AddArc(rect2, 0, -180)
                '        SwitchStatePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStatePath)
                '    Else
                '        SwitchStatePath.AddArc(rect2, 90, 180)
                '        SwitchStatePath.AddArc(rect2, 90, -180)
                '        SwitchStatePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStatePath)
                '    End If

                '    gSwitchIndicationPath.Add(SwitchStatePath)

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub SetBreakerGraphics()
            Try
                Dim SwitchPath As New GraphicsPath
                Dim tx As Integer = 1
                Dim ty As Integer = 1
                Dim tsize As Integer = 24
                gSwitchIndicationPath.Clear()



                'Select Case SwitchIndState
                '    Case BreakerIndStatus.None

                '        tx = 1
                '        ty = 1
                '        tsize = 24

                '    Case BreakerIndStatus.ControlInhibit

                '        tx = 7
                '        ty = 7
                '        tsize = 12

                '        Dim SwitchStateCirclePath As New GraphicsPath
                '        SwitchStateCirclePath.StartFigure()
                '        Dim rect As New Rectangle(tx - 5, ty - 5, 22, 22)
                '        SwitchStateCirclePath.AddArc(rect, -30, 180)
                '        SwitchStateCirclePath.AddArc(rect, -30, -180)
                '        SwitchStateCirclePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStateCirclePath)

                '    Case Else

                '        tx = 7
                '        ty = 7
                '        tsize = 12

                '        Dim SwitchStateSquarePath As New GraphicsPath
                '        SwitchStateSquarePath.StartFigure()
                '        pt1 = New Point(tx - 7, ty - 7)
                '        pt2 = New Point(tx + 19, ty - 7)
                '        pt3 = New Point(tx + 19, ty + 19)
                '        pt4 = New Point(tx - 7, ty + 19)
                '        SwitchStateSquarePath.AddLine(pt1, pt2)
                '        SwitchStateSquarePath.AddLine(pt2, pt3)
                '        SwitchStateSquarePath.AddLine(pt3, pt4)
                '        SwitchStateSquarePath.AddLine(pt4, pt1)
                '        SwitchStateSquarePath.CloseFigure()
                '        gSwitchStatePath.Add(SwitchStateSquarePath)

                'End Select


                If InhibitControlFromDispatcher Then
                    SwitchPath.StartFigure()

                    Dim rect As New Rectangle(tx - 1, ty - 1, tsize + 1, tsize + 1)
                    SwitchPath.AddArc(rect, -30, 180)
                    SwitchPath.AddArc(rect, -30, -180)
                    SwitchPath.CloseFigure()
                    SwitchPath.CloseFigure()
                End If
                If InhibitControlFromDispatcher Then
                    tx = 6
                    ty = 6
                    tsize = 14
                End If
                If SwitchIndicatedLocalControl OrElse SwitchIndicatedLockedOut OrElse SwitchIndicatedProhibited OrElse SwitchIndicatedRackedOut Then
                    tx = 5
                    ty = 5
                    tsize = 18
                End If

                'SWITCH INDICATION STATE
                SwitchPath.StartFigure()
                pt1 = New Point(tx, ty)
                pt2 = New Point(tx + tsize, ty)
                pt3 = New Point(tx + tsize, ty + tsize)
                pt4 = New Point(tx, ty + tsize)
                SwitchPath.AddLine(pt1, pt2)
                SwitchPath.AddLine(pt2, pt3)
                SwitchPath.AddLine(pt3, pt4)
                SwitchPath.AddLine(pt4, pt1)
                SwitchPath.CloseFigure()

                If SwitchIndicatedClosed Then
                    SwitchPath.StartFigure()
                    If Me.FlipOperations Then
                        pt1 = New Point(tx, ty + (tsize / 2))
                        pt2 = New Point(tx + tsize, ty + (tsize / 2))
                    Else
                        pt1 = New Point(tx + (tsize / 2), ty)
                        pt2 = New Point(tx + (tsize / 2), ty + tsize)
                    End If
                    SwitchPath.AddLine(pt1, pt2)
                    SwitchPath.CloseFigure()
                Else
                    SwitchPath.StartFigure()
                    If Me.FlipOperations Then
                        pt1 = New Point(tx + (tsize / 2), ty)
                        pt2 = New Point(tx + (tsize / 2), ty + tsize)
                    Else
                        pt1 = New Point(tx, ty + (tsize / 2))
                        pt2 = New Point(tx + tsize, ty + (tsize / 2))
                    End If
                    SwitchPath.AddLine(pt1, pt2)
                    SwitchPath.CloseFigure()
                End If


                gSwitchIndicationPath.Add(SwitchPath)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        'Private Sub SetBreakerGraphics()
        '    Try
        '        Dim tx As Integer = 0
        '        Dim ty As Integer = 0
        '        Dim tsize As Integer = 0


        '        gSwitchIndicationPath.Clear()
        '        Dim SwitchStatePath As New GraphicsPath
        '        SwitchStatePath.StartFigure()
        '        Dim rect2 As New Rectangle(0, 0, 24, 24)

        '        If SwitchIndicatedClosed Then
        '            SwitchStatePath.AddArc(rect2, 0, 180)
        '            SwitchStatePath.AddArc(rect2, 0, -180)
        '            SwitchStatePath.CloseFigure()
        '            gSwitchStatePath.Add(SwitchStatePath)
        '        Else
        '            tx = 7
        '            ty = 7
        '            tsize = 12

        '            'Dim SwitchStateSquarePath As New GraphicsPath
        '            'SwitchStateSquarePath.StartFigure()
        '            pt1 = New Point(tx - 7, ty - 7)
        '            pt2 = New Point(tx + 19, ty - 7)
        '            pt3 = New Point(tx + 19, ty + 19)
        '            pt4 = New Point(tx - 7, ty + 19)
        '            Dim pt5 As New Point(0, 12)
        '            Dim pt6 As New Point(26, 12)
        '            SwitchStatePath.AddLine(pt1, pt2)
        '            SwitchStatePath.AddLine(pt2, pt3)
        '            SwitchStatePath.AddLine(pt3, pt4)
        '            SwitchStatePath.AddLine(pt4, pt1)
        '            'SwitchStatePath.AddLine(pt5, pt6)

        '            SwitchStatePath.CloseFigure()
        '            gSwitchStatePath.Add(SwitchStatePath)

        '            'SwitchStateSquarePath.AddLine(pt1, pt2)
        '            'SwitchStateSquarePath.AddLine(pt2, pt3)
        '            'SwitchStateSquarePath.AddLine(pt3, pt4)
        '            'SwitchStateSquarePath.AddLine(pt4, pt1)
        '            'SwitchStateSquarePath.CloseFigure()
        '            'gSwitchStatePath.Add(SwitchStateSquarePath)
        '        End If

        '        gSwitchIndicationPath.Add(SwitchStatePath)


        '        'Select Case SwitchIndState
        '        '    Case SwIndStatus.None

        '        '        tx = 0
        '        '        ty = 0
        '        '        tsize = 24

        '        '    Case SwIndStatus.ControlInhibit

        '        '        tx = 7
        '        '        ty = 7
        '        '        tsize = 12

        '        '        Dim SwitchStateCirclePath As New GraphicsPath
        '        '        SwitchStateCirclePath.StartFigure()
        '        '        Dim rect As New Rectangle(tx - 5, ty - 5, 22, 22)
        '        '        SwitchStateCirclePath.AddArc(rect, -30, 180)
        '        '        SwitchStateCirclePath.AddArc(rect, -30, -180)
        '        '        SwitchStateCirclePath.CloseFigure()
        '        '        gSwitchStatePath.Add(SwitchStateCirclePath)

        '        '    Case Else

        '        '        tx = 7
        '        '        ty = 7
        '        '        tsize = 12

        '        '        Dim SwitchStateSquarePath As New GraphicsPath
        '        '        SwitchStateSquarePath.StartFigure()
        '        '        pt1 = New Point(tx - 7, ty - 7)
        '        '        pt2 = New Point(tx + 19, ty - 7)
        '        '        pt3 = New Point(tx + 19, ty + 19)
        '        '        pt4 = New Point(tx - 7, ty + 19)
        '        '        SwitchStateSquarePath.AddLine(pt1, pt2)
        '        '        SwitchStateSquarePath.AddLine(pt2, pt3)
        '        '        SwitchStateSquarePath.AddLine(pt3, pt4)
        '        '        SwitchStateSquarePath.AddLine(pt4, pt1)
        '        '        SwitchStateSquarePath.CloseFigure()
        '        '        gSwitchStatePath.Add(SwitchStateSquarePath)

        '        'End Select

        '        ''SWITCH CONTROL STATE
        '        'Dim SwitchControlPath As New GraphicsPath
        '        'SwitchControlPath.StartFigure()
        '        'pt1 = New Point(tx, ty)
        '        'pt2 = New Point(tx + tsize, ty)
        '        'pt3 = New Point(tx + tsize, ty + tsize)
        '        'pt4 = New Point(tx, ty + tsize)
        '        'SwitchControlPath.AddLine(pt1, pt2)
        '        'SwitchControlPath.AddLine(pt2, pt3)
        '        'SwitchControlPath.AddLine(pt3, pt4)
        '        'SwitchControlPath.AddLine(pt4, pt1)
        '        'SwitchControlPath.CloseFigure()

        '        'Select Case SwitchCtrlState
        '        '    Case SwCtlStatus.Open
        '        '        SwitchControlPath.StartFigure()
        '        '        pt1 = New Point(tx + (tsize / 2), ty)
        '        '        pt2 = New Point(tx + (tsize / 2), ty + tsize)
        '        '        SwitchControlPath.AddLine(pt1, pt2)
        '        '        SwitchControlPath.CloseFigure()
        '        '    Case SwCtlStatus.Close
        '        '        SwitchControlPath.StartFigure()
        '        '        pt1 = New Point(tx, ty + (tsize / 2))
        '        '        pt2 = New Point(tx + tsize, ty + (tsize / 2))
        '        '        SwitchControlPath.AddLine(pt1, pt2)
        '        '        SwitchControlPath.CloseFigure()
        '        'End Select

        '        'gSwitchIndicationPath.Add(SwitchControlPath)
        '    Catch ex As Exception
        '        SystemException(ex.ToString)
        '    End Try
        'End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)
            Me.Refresh()
        End Sub

#End Region

#Region "CIRCUIT METHODS"

#Region "POWER MANAGEMENT DISPLAY"

        Public Sub DoCircuitEnergizeCheck()
            Try
                Dim theSwitchList As New ArrayList
                theSwitchList.Add(Me)
                If Me.SwitchIndicatedClosed Then
                    'energize circuits linked to the right
                    If (Me.PowerFromLeftDetectedFixed OrElse Me.PowerFromLeftDetected) AndAlso Not IsNothing(Me.myCircuitToRight) Then
                        Me.myCircuitToRight.DoEnergizeCircuit(Me, theSwitchList)
                    End If
                    If (Me.PowerFromRightDetectedFixed OrElse Me.PowerFromRightDetected) AndAlso Not IsNothing(Me.myCircuitToLeft) Then
                        Me.myCircuitToLeft.DoEnergizeCircuit(Me, theSwitchList)
                    End If
                Else
                    If Not IsNothing(Me.myCircuitToRight) Then
                        Me.myCircuitToRight.DoDenergizeCircuit(Me, theSwitchList)
                    End If
                    If Not IsNothing(Me.myCircuitToLeft) Then
                        Me.myCircuitToLeft.DoDenergizeCircuit(Me, theSwitchList)
                    End If

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#End Region

#Region "GENERAL METHODS"

        Sub ApplyIndicationChange()
            Try
                If Not Me.Ind(0) Is Nothing Then
                    If Ind(0).BitState Then
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedClosed", True)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedClosed", False)
                    End If
                End If
                Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "DisableControl", False)

                If Not Me.IndInhibit Is Nothing Then
                    If IndInhibit.BitState Then
                        DisableControl = True
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "DisableControl", True)
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedProhibited", True)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedProhibited", False)
                    End If
                End If

                If Not Me.IndLocalControl Is Nothing Then
                    If IndLocalControl.BitState Then
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "DisableControl", True)
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedLocalControl", True)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedLocalControl", False)
                    End If
                End If
                If Not Me.IndLockOut Is Nothing Then
                    If IndLockOut.BitState Then
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "DisableControl", True)
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedLockedOut", True)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedLockedOut", False)
                    End If
                End If
                If Not Me.IndRackOut Is Nothing Then
                    If IndRackOut.BitState Then
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "DisableControl", True)
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedRackedOut", True)
                    Else
                        Me.threadSafePropertySet.SetCtrlProperty(Of Boolean)(Me, "SwitchIndicatedRackedOut", False)
                    End If
                End If

                RefreshPath()

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

    End Class

#End Region

#Region "ELECTRCIAL LINE"

    Public Class ElectricalLine
        Inherits GenericComponent

#Region "Fields"

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32

        Public TypeOfLine As String
        Public LeftLimitRange As Integer
        Public RightLimitRange As Integer
        Public LeftLimitName As String
        Public RightLimitName As String
        Public FlipOperations As Boolean
        Public IsEnergized As Boolean
        Public myCircuit As ElectricalCircuit


#End Region

#Region "GRAPHICS METHODS"

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Dim pen As Pen = Nothing
            Try
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality
                If IsEnergized Then
                    e.Graphics.FillPath(New SolidBrush(Color.Red), Path)
                Else
                    e.Graphics.FillPath(New SolidBrush(Color.Green), Path)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
                Me.Visible = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)

            Me.Refresh()

        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            Dim TrackWidth As Integer = 9

            'Set the position of the Control Drawing Area
            Me.Left = LX
            If RY < LY Then
                Me.Top = RY
            Else
                Me.Top = LY
            End If

            Me.Width = Math.Abs(RX - LX)
            Me.Height = Math.Abs(RY - LY) + TrackWidth
            'CHECK TO SEE IF THIS IS A VERTICAL TRACK
            If Me.Width = 0 Then
                Me.Width = TrackWidth
            End If


            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            If RY < LY Then
                rLy = Math.Abs(LY - RY)
            Else
                rLy = 0
            End If

            rRx = Math.Abs(RX - LX)
            If RY < LY Then
                rRy = 0
            Else
                rRy = Math.Abs(RY - LY)
            End If

        End Sub

        Overrides Sub RefreshPath()
            Try
                Path = New GraphicsPath()

                Dim TrackWidth As Integer = 3
                Dim TrackGap As Integer = 0
                Dim pt1, pt2, pt3, pt4 As Point

                'This draws a horizontal line
                If Math.Abs((rRy - rLy) / (rRx - rLx)) <= 1 Then
                    pt1 = New Point(rLx, rLy)
                    pt2 = New Point(rRx - TrackGap, rRy)
                    pt3 = New Point(rRx - TrackGap, rRy + TrackWidth)
                    pt4 = New Point(rLx, rLy + TrackWidth)

                Else
                    pt1 = New Point(rLx, rLy)
                    pt2 = New Point(rRx, rRy - TrackGap)
                    pt3 = New Point(rRx + TrackWidth, rRy - TrackGap)
                    pt4 = New Point(rLx + TrackWidth, rLy)

                End If

                Path.StartFigure()
                Path.AddLine(pt1, pt2)
                Path.AddLine(pt2, pt3)
                Path.AddLine(pt3, pt4)
                Path.AddLine(pt4, pt1)
                Path.CloseFigure()

                'Path.AddPolygon(New Point() {pt1, pt2, pt3, pt4, pt1})

                Me.Region = New Region(Path)


                If rRy < rLy Then
                    'Adjust the screen cooridnates
                    RY = CShort(Top)
                    LY = CShort(Top + rLy)
                Else
                    LY = CShort(Top)
                    RY = CShort(Top + rRy)
                End If
                LX = CShort(rLx + Left)
                RX = CShort(rRx + LX)
                ' Me.BringToFront()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

    End Class

#End Region

#Region "ELECTRCIAL CONNECTION"

    Public Class ElectricalConnection
        Inherits GenericComponent

#Region "Fields"

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32
        Public IsEnergized As Boolean

#End Region

#Region " GRAPHICS METHODS "

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Dim pen As Pen = Nothing
            Try
                If IsEnergized Then
                    e.Graphics.FillPath(New SolidBrush(Color.Red), Path)
                Else
                    e.Graphics.FillPath(New SolidBrush(Color.Green), Path)
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
                Me.Visible = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)

            Me.Refresh()

        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Dim TrackWidth As Integer = 9

            ''Set the position of the Control Drawing Area
            'Me.Left = LX
            'If RY < LY Then
            '    Me.Top = RY
            'Else
            '    Me.Top = LY
            'End If

            'Me.Width = Math.Abs(RX - LX)
            'Me.Height = Math.Abs(RY - LY) + TrackWidth
            ''CHECK TO SEE IF THIS IS A VERTICAL TRACK
            'If Me.Width = 0 Then
            '    Me.Width = TrackWidth
            'End If


            ''Set the Relative coordinates in the control from the screen coords
            'rLx = 0
            'If RY < LY Then
            '    rLy = Math.Abs(LY - RY)
            'Else
            '    rLy = 0
            'End If

            'rRx = Math.Abs(RX - LX)
            'If RY < LY Then
            '    rRy = 0
            'Else
            '    rRy = Math.Abs(RY - LY)
            'End If

            'Set the position of the Control Drawing Area
            Me.Left = LX - 6
            Me.Top = LY - 2

            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            rLy = 0
            rRx = 12
            rRy = 12
            Me.Width = rRx
            Me.Height = rRy

        End Sub

        Overrides Sub RefreshPath()
            Try
                Dim pt1, pt2, pt3, pt4 As Point
                Path = New GraphicsPath()
                Dim rectF As RectangleF = New RectangleF(rLx, rLy, 10, 10)
                pt1 = New Point(0, 0)
                pt3 = New Point(11, 11)
                pt2 = New Point(11, 0)
                pt4 = New Point(-2, 11)
                Path.AddEllipse(rectF)
                Me.Region = New Region(Path)

                Me.BringToFront()

                'Path = New GraphicsPath()

                'Dim TrackWidth As Integer = 3
                'Dim TrackGap As Integer = 0
                'Dim pt1, pt2, pt3, pt4 As Point

                ''This draws a horizontal line
                'If Math.Abs((rRy - rLy) / (rRx - rLx)) <= 1 Then
                '    pt1 = New Point(rLx, rLy)
                '    pt2 = New Point(rRx - TrackGap, rRy)
                '    pt3 = New Point(rRx - TrackGap, rRy + TrackWidth)
                '    pt4 = New Point(rLx, rLy + TrackWidth)

                'Else
                '    pt1 = New Point(rLx, rLy)
                '    pt2 = New Point(rRx, rRy - TrackGap)
                '    pt3 = New Point(rRx + TrackWidth, rRy - TrackGap)
                '    pt4 = New Point(rLx + TrackWidth, rLy)

                'End If

                'Path.StartFigure()
                'Path.AddLine(pt1, pt2)
                'Path.AddLine(pt2, pt3)
                'Path.AddLine(pt3, pt4)
                'Path.AddLine(pt4, pt1)
                'Path.CloseFigure()

                ''Path.AddPolygon(New Point() {pt1, pt2, pt3, pt4, pt1})

                'Me.Region = New Region(Path)


                'If rRy < rLy Then
                '    'Adjust the screen cooridnates
                '    RY = CShort(Top)
                '    LY = CShort(Top + rLy)
                'Else
                '    LY = CShort(Top)
                '    RY = CShort(Top + rRy)
                'End If
                'LX = CShort(rLx + Left)
                'RX = CShort(rRx + LX)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

    End Class

#End Region

#Region "WEATHER STATION"

    Public Class WeatherStation
        Inherits GenericComponent

#Region "Fields"
        'Drwing points for the switch
        Dim pt1 As New Point()
        Dim pt2 As New Point()
        Dim pt3 As New Point()
        Dim pt4 As New Point()
        Dim pt5 As New Point()
        Dim pt6 As New Point()

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32
        Dim rectF As RectangleF

        Private StationFont As Font = New Font("COURIER", 10.5!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private StringFormat As StringFormat
        Private StringType As String = "WS"

        Public MilePost As Double
        Public MilePostSuffix As String = String.Empty
        Public MilePostPrefix As String = String.Empty

        Public TypeOfStation As String = String.Empty
        Public UniqueName As String = String.Empty
        Public TrackGUID As Integer
        Public Notes As String
        Public LastAlertAt As Date = Now
#End Region

#Region " GRAPHICS METHODS "

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Set the position of the Control Drawing Area
            Me.Left = LX
            Me.Top = LY

            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            rLy = 0
            rRx = 25
            rRy = 25

            Me.Width = rRx
            Me.Height = rRy
        End Sub

        Overrides Sub RefreshPath()
            Try
                Path = New GraphicsPath()

                StringFormat = New StringFormat()
                StringFormat.Alignment = StringAlignment.Center
                StringFormat.LineAlignment = StringAlignment.Center

                rectF = New RectangleF(rLx, rLy, 25, 25)
                pt1 = New Point(0, 0)
                pt3 = New Point(20, 13)
                pt2 = New Point(20, 0)
                pt4 = New Point(-2, 23)
                Path.AddRectangle(rectF)
                Me.Region = New Region(Path)
                Path.CloseFigure()

                pDefaultPath = Path
                GraphicalPath = pDefaultPath
                Me.GraphicalPath = Path
                Me.Region = New Region(Path)
                Me.BringToFront()
                Me.ForeColor = Color.Firebrick
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                e.Graphics.DrawPath(New Pen(Me.ForeColor, 3), Path)
                e.Graphics.DrawString(StringType, StationFont, New SolidBrush(Me.ForeColor), rectF, StringFormat)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub WeatherStation_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                ToolTipMessage = ToolTipMessage & "(" & Me.UniqueName & " ; " & Me.TypeOfStation & ")"
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)

            Me.Refresh()

        End Sub

#End Region

#Region "CONTEXT MENU"

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub

        Public Sub BuildContextMenu()
            Try

                Me.ContextMenuControl = New ContextMenu()
                AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp

                If optEnableWeatherStationMenu Then
                    AddWeatherStationMenuItem()
                End If


                Me.ContextMenu = Me.ContextMenuControl

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Sub

#End Region

    End Class

#End Region

#Region "ELECTRICAL CIRCUIT"

    Public Class ElectricalCircuit
        Public myCircuitName As String
        Public myCircuitList As New ArrayList
        Public myCantenaryList As New ArrayList

        Public mySwitchListToRight As New ArrayList
        Public mySwitchListToLeft As New ArrayList
        Private pIsEnergized As Boolean

        Public Property IsEnergized() As Boolean
            Get
                Return pIsEnergized
            End Get
            Set(ByVal Value As Boolean)
                pIsEnergized = Value
            End Set

        End Property

        Public Sub DoDenergizeCircuit(theSw As ElectricalSwitch, ByRef theSwitchList As ArrayList)
            Try
                IsEnergized = False
                Dim comp As Object
                Dim es As ElectricalSwitch

                For Each es In mySwitchListToRight
                    If es.SwitchIndicatedClosed AndAlso (es.PowerFromRightDetected OrElse es.PowerFromRightDetectedFixed) Then
                        IsEnergized = True
                    End If
                Next

                For Each es In mySwitchListToLeft
                    If es.SwitchIndicatedClosed AndAlso (es.PowerFromLeftDetectedFixed OrElse es.PowerFromLeftDetected) Then
                        IsEnergized = True
                    End If
                Next

                If Not IsEnergized Then
                    For Each comp In myCircuitList
                        comp.IsEnergized = IsEnergized
                        SystemEvent("DoEnergizeCircuit:" & myCircuitName & ":" & comp.Name & ":" & comp.guid & ":" & IsEnergized)
                        If comp.InvokeRequired Then
                            comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                        Else
                            comp.Refresh()
                        End If
                    Next
                    For Each comp In myCantenaryList
                        comp.IsEnergized = IsEnergized
                        If comp.InvokeRequired Then
                            comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                        Else
                            comp.Refresh()
                        End If
                    Next

                    For Each es In mySwitchListToRight
                        es.PowerFromLeftDetected = False
                        If es.SwitchIndicatedClosed AndAlso Not es.PowerFromRightDetected Then
                            If Not theSwitchList.Contains(es) Then
                                theSwitchList.Add(es)
                                If Not IsNothing(es.myCircuitToRight) Then
                                    es.myCircuitToRight.DoDenergizeCircuit(es, theSwitchList)
                                End If
                            End If
                        End If
                    Next
                    For Each es In mySwitchListToLeft
                        es.PowerFromRightDetected = False
                        If es.SwitchIndicatedClosed AndAlso Not es.PowerFromLeftDetected Then
                            If Not theSwitchList.Contains(es) Then
                                theSwitchList.Add(es)
                                If Not IsNothing(es.myCircuitToLeft) Then
                                    es.myCircuitToLeft.DoDenergizeCircuit(es, theSwitchList)
                                End If
                            End If
                        End If
                    Next



                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub DoEnergizeCircuit(theSw As ElectricalSwitch, ByRef theSwitchList As ArrayList)
            Try
                IsEnergized = True
                Dim comp As Object
                Dim es As ElectricalSwitch

                For Each comp In myCircuitList
                    comp.IsEnergized = IsEnergized
                    SystemEvent("DoEnergizeCircuit:" & myCircuitName & ":" & comp.Name & ":" & comp.guid & ":" & IsEnergized)
                    If comp.InvokeRequired Then
                        comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                    Else
                        comp.Refresh()
                    End If
                Next
                For Each comp In myCantenaryList
                    comp.IsEnergized = IsEnergized
                    If comp.InvokeRequired Then
                        comp.Invoke(New MethodInvoker(AddressOf comp.Refresh))
                    Else
                        comp.Refresh()
                    End If
                Next

                For Each es In mySwitchListToRight
                    If Not theSwitchList.Contains(es) Then
                        es.PowerFromLeftDetected = True
                        If es.SwitchIndicatedClosed Then
                            theSwitchList.Add(es)
                            If Not IsNothing(es.myCircuitToRight) Then
                                es.myCircuitToRight.DoEnergizeCircuit(es, theSwitchList)
                            End If
                        End If
                    End If
                Next

                For Each es In mySwitchListToLeft
                    If Not theSwitchList.Contains(es) Then
                        es.PowerFromRightDetected = True
                        If es.SwitchIndicatedClosed Then
                            theSwitchList.Add(es)
                            If Not IsNothing(es.myCircuitToLeft) Then
                                es.myCircuitToLeft.DoEnergizeCircuit(es, theSwitchList)
                            End If
                        End If
                    End If
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "TEXTUAL TAG"

    Public Class TextualTag
        'Inherits GenericComponent
        Inherits System.Windows.Forms.UserControl
        Implements IDisposable

        Public pAlphaFont As Font = New Font("ARIAL", 8.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
        Public pAlphaBox As RectangleF
        Public pAlphaLocation As Point
        Public pAlphaBoxSize As SizeF
        Public pAlphaColor As Color = Color.White
        Public pAlphaBrush As SolidBrush = New SolidBrush(pAlphaColor)
        Public pAlphaRegion As Region
        Public pStringAlign As StringAlignment = New StringAlignment() = StringAlignment.Center
        Public pStringFormat As StringFormat = New StringFormat()
        Public pStringSize As SizeF

        Public Graphical As Graphics
        Public GraphicalPath As New GraphicsPath()
        Public GraphicalRegion As Region

        Public xFactor As Short
        Public yFactor As Short

        Sub BuildGraphics()
            Try
                Me.Graphical = Me.CreateGraphics

                pStringFormat.Alignment = StringAlignment.Center
                pStringFormat.LineAlignment = StringAlignment.Center
                MeasureStringSize()
                pAlphaBox = New RectangleF(New PointF(0, 0), pAlphaBoxSize)
                GraphicalPath.AddRectangle(pAlphaBox)
                Me.Width = pAlphaBox.Width
                Me.Height = pAlphaBox.Height
                Me.Visible = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Sub MeasureStringSize()
            pStringSize = Graphical.MeasureString(Name, pAlphaFont, 150)
            pAlphaBoxSize = New SizeF(pStringSize.Width, pStringSize.Height)
            Graphical = Nothing
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Dim pen As Pen = Nothing
            Try
                Me.BringToFront()
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality

                ' e.Graphics.FillPath(New SolidBrush(Color.Green), GraphicalPath)
                e.Graphics.DrawString(Name, pAlphaFont, pAlphaBrush, pAlphaBox)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

    End Class
#End Region

#Region "PASSENGER STATION"

    Public Class PassengerStation
        Inherits GenericComponent

#Region "PROPERITIES"

        Dim pt1 As New Point()
        Dim pt2 As New Point()
        Dim pt3 As New Point()
        Dim pt4 As New Point()
        Dim pt5 As New Point()
        Dim pt6 As New Point()

        Dim rLx As Int32
        Dim rLy As Int32
        Dim rRx As Int32
        Dim rRy As Int32
        Dim rectF As RectangleF

        Private StationFont As Font = New Font("COURIER", 10.5!, FontStyle.Regular, GraphicsUnit.Pixel, CType(0, Byte))
        Private StringFormat As StringFormat

        Public LocIndex As String
        Public OrderIndex As Integer
        Public MilePost As String
        Public MileageNext As String
        Public SidingLen As String
        Public Rule43 As String
        Public TypeOp As String
        Public SubID As Int32
        Public ControlPointUID As Int32
        Public TrackType As String
        Public MilePostSuffix As String = String.Empty
        Public MilePostPrefix As String = String.Empty
        Public MilePostMarker As Boolean
        Public AscendsionChange As Boolean
        Public MilePostChangeEvent As Boolean

        Public ReportingUID As String = String.Empty
        Public EntryReportingLeftBoundTracks As String = String.Empty
        Public EntryReportingRightBoundTracks As String = String.Empty
        Public ExitReportingLeftBoundTracks As String = String.Empty
        Public ExitReportingRightBoundTracks As String = String.Empty
        Public ReportingEntryTracksForRightBoundMovements As New ArrayList
        Public ReportingEntryTracksForLeftBoundMovements As New ArrayList
        Public ReportingExitTracksForRightBoundMovements As New ArrayList
        Public ReportingExitTracksForLeftBoundMovements As New ArrayList

        Public StandardPointLocationCode As String

        Public DITP As Boolean = False
        Public TTP As Boolean = False
        Public DistanceToLeftStation As Integer = 0
        Public DistanceToRightStation As Integer = 0
        Public RunningTimeToLeftStation As String = String.Empty
        Public RunningTimeToRightStation As String = String.Empty
        Public DesignatedArrivalShelfTrackUID As Integer = 0
        Public AutoPostionLeftTrackUID As Integer = 0
        Public AutoPostionRightTrackUID As Integer = 0
        Public LeftScannerList As New ArrayList
        Public RightScannerList As New ArrayList
        Public myPSColor As Color = Color.Yellow

#End Region

#Region "CONTEXT MENU"

        Private Sub ContextMenu_PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        End Sub

        Public Sub BuildContextMenu()
            Try
                If optUseTroubleReportingEnhanced Then


                    Me.ContextMenuControl = New ContextMenu()
                    AddHandler ContextMenuControl.Popup, AddressOf ContextMenu_PopUp
                    'Public mnuTroubleReportCommunications As MenuItem
                    'Public mnuTroubleReportWayside As MenuItem
                    'Public mnuTroubleReportOverhead As MenuItem
                    'Public mnuTroubleReportTractionPower As MenuItem
                    mnuTroubleReport = New MenuItem
                    mnuTroubleReport = New MenuItem("CREATE INCIDENT REPORT")
                    ContextMenuControl.MenuItems.Add(mnuTroubleReport)
                    '
                    'mnuTroubleReportPsgrStation
                    mnuTroubleReportPsgrStation = New MenuItem("STATION/FACILITY")
                    mnuTroubleReportPsgrStation.Tag = "STATION/FACILITY"
                    AddHandler mnuTroubleReportPsgrStation.Click, AddressOf Me.mnuTroubleReportPSGR_Click
                    mnuTroubleReport.MenuItems.Add(mnuTroubleReportPsgrStation)

                    mnuTroubleReportCommunications = New MenuItem("COMMUNICATIONS")
                    mnuTroubleReportCommunications.Tag = "COMMUNICATIONS"
                    AddHandler mnuTroubleReportCommunications.Click, AddressOf Me.mnuTroubleReportPSGR_Click
                    mnuTroubleReport.MenuItems.Add(mnuTroubleReportCommunications)

                    mnuTroubleReportWayside = New MenuItem("WAYSIDE EQUIPMENT")
                    mnuTroubleReportWayside.Tag = "WAYSIDE EQUIPMENT"
                    AddHandler mnuTroubleReportWayside.Click, AddressOf Me.mnuTroubleReportPSGR_Click
                    mnuTroubleReport.MenuItems.Add(mnuTroubleReportWayside)

                    mnuTroubleReportOverhead = New MenuItem("OVERHEAD (OCS)")
                    mnuTroubleReportOverhead.Tag = "OVERHEAD (OCS)"
                    AddHandler mnuTroubleReportOverhead.Click, AddressOf Me.mnuTroubleReportPSGR_Click
                    mnuTroubleReport.MenuItems.Add(mnuTroubleReportOverhead)

                    mnuTroubleReportTractionPower = New MenuItem("TRACTION POWER (TPSS)")
                    mnuTroubleReportTractionPower.Tag = "TRACTION POWER (TPSS)"
                    AddHandler mnuTroubleReportTractionPower.Click, AddressOf Me.mnuTroubleReportPSGR_Click
                    If optEnableSpecificFunctionalityForDenver Then
                        mnuTroubleReport.MenuItems.Add(mnuTroubleReportTractionPower) 'CMV DTP-86
                    Else
                        mnuTroubleReport.MenuItems.Add(mnuTroubleReportOverhead)
                    End If
                    'mnuTroubleReport.MenuItems.Add(mnuTroubleReportOverhead) 'CMV DTP-86
                    Me.ContextMenu = Me.ContextMenuControl
                End If


            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Sub

#End Region

#Region "PASSENGER STATION GRAPHICS METHODS"

        Overrides Sub InitGraphics()
            Refresh()
        End Sub

        Overloads Sub BuildGraphics()
            Try
                Me.InitializeGraphicsFromScreenCoordinates()
                RefreshPath()
                Refresh()
                Me.Graphical = Me.CreateGraphics
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Overrides Sub InitializeGraphicsFromScreenCoordinates()
            'Set the position of the Control Drawing Area
            Me.Left = LX - 6
            Me.Top = LY - 2

            'Set the Relative coordinates in the control from the screen coords
            rLx = 0
            rLy = 0
            rRx = 30
            rRy = 15

            Me.Width = rRx
            Me.Height = rRy
        End Sub

        Overrides Sub RefreshPath()
            Path = New GraphicsPath()
            Try
                StringFormat = New StringFormat()
                StringFormat.Alignment = StringAlignment.Center
                StringFormat.LineAlignment = StringAlignment.Center

                rectF = New RectangleF(rLx, rLy, 30, 15)
                pt1 = New Point(0, 0)
                pt3 = New Point(20, 13)
                pt2 = New Point(20, 0)
                pt4 = New Point(-2, 13)
                Path.AddRectangle(rectF)
                Me.Region = New Region(Path)
                Me.ForeColor = myPSColor
                Me.BringToFront()
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            MyBase.OnPaint(e)
            Try
                e.Graphics.DrawPath(New Pen(Me.ForeColor, 3), Path)
                e.Graphics.DrawString(Me.ComponentType, StationFont, New SolidBrush(Me.ForeColor), rectF, StringFormat)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Public Sub PassengerStation_MouseHover(ByVal sender As Object, ByVal e As EventArgs)
            Dim ToolTipMessage As String = String.Empty
            Try
                ToolTipMessage = ToolTipMessage & "(MP " & Me.MilePost & ") ( NAME : " & Me.Name & ")"
                Me.CurrentDrawingForm.TrackToolTip.SetToolTip(Me, ToolTipMessage)
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

#End Region

    End Class

#End Region

End Namespace

