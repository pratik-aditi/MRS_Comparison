Imports System.Data.OleDb
Imports System.IO
Imports System.Text
Imports TMDS.GeneralBulletins
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrainComponents
Imports System.Reflection.MethodInfo

Module modSystemFuncs
    Public Delegate Sub DoForceTGBORefresh(ByVal CallingMethod As String)

#Region "Garabge Collection and Dup Process Check"
    ' Public Declare Function DeleteObject Lib "gdi32" Alias "DeleteObject" (ByVal hObject As Long) As Long

    Sub ReleaseDBCommand(ByVal dbcomm As OleDbCommand)
        Try
            If Not dbcomm Is Nothing Then
                dbcomm.Dispose()
                dbcomm = Nothing
            End If
        Catch ex As Exception
            SystemException("ERROR OCCURED WHILE RELEASING DB COMMAND OBJECT :" & ex.ToString)
        End Try
    End Sub

    Sub ReleaseDBConnection(ByVal dbconn As OleDbConnection)
        Try
            If Not dbconn Is Nothing Then
                If dbconn.State = ConnectionState.Open Then dbconn.Close()
                dbconn.Dispose()
                dbconn = Nothing
            End If
        Catch ex As Exception
            SystemException("ERROR OCCURED WHILE RELEASING DB CONNECTION OBJECT :" & ex.ToString)
        End Try
    End Sub

    'Release/disposes the windows handles
    Public Sub ReleaseWH(ByVal objfrm As System.Windows.Forms.Form)
        Try
            If Not IsNothing(objfrm) Then
                objfrm.Close()
                objfrm.Dispose()
                objfrm = Nothing
            End If
        Catch ex As Exception
            SystemException("ERROR OCCURED WHILE RELEASING WINDOWS HANBLE OBJECT : " & ex.ToString)
        End Try
    End Sub

    'Releases/disposes the GDI objects
    Public Sub ReleaseGDI(ByVal objgdi As Object)
        Try
            If Not IsNothing(objgdi) Then
                objgdi.Dispose()
                objgdi = Nothing
                'DeleteObject(objgdi.GetHashCode)
            End If
        Catch ex As Exception
            SystemException("ERROR OCCURED WHILE RELEASING GDI OBJECTS : " & ex.ToString)
        End Try
    End Sub

    Sub ForceGarbageCollection()
        Try
            'Dim MemoryUsage As String = "ForceGarbageCollection Event! WM:"
            'Dim CurrentProcess As Process = Process.GetCurrentProcess
            GC.Collect(2)
        Catch
        End Try
    End Sub

#End Region

#Region "BUILD TRAIN ABBREVIATIONS"

    Sub BuildTrainAbbreviations()
        Try
            Dim i As Integer
            For i = 1 To 75
                Dim TrainIDN As New TrainIDAbbreviation()
                TrainIDN.AbbrID = "N" & i.ToString
                TrainIDN.TrainDirection = "NORTH"
                gTrainAbbreviation.Add(TrainIDN)
            Next
            For i = 1 To 75
                Dim TrainIDS As New TrainIDAbbreviation()
                TrainIDS.AbbrID = "S" & i.ToString
                TrainIDS.TrainDirection = "SOUTH"
                gTrainAbbreviation.Add(TrainIDS)
            Next
            For i = 1 To 75
                Dim TrainIDE As New TrainIDAbbreviation()
                TrainIDE.AbbrID = "E" & i.ToString
                TrainIDE.TrainDirection = "EAST"
                gTrainAbbreviation.Add(TrainIDE)
            Next
            For i = 1 To 75
                Dim TrainIDW As New TrainIDAbbreviation()
                TrainIDW.AbbrID = "W" & i.ToString
                TrainIDW.TrainDirection = "WEST"
                gTrainAbbreviation.Add(TrainIDW)

            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Check if exist another TMDS running
    ''' </summary>
    Sub CheckForDupProcess()
        Dim p As Process
        Dim proc As New Process()
        Dim currentProcess As Process = Process.GetCurrentProcess
        Try
            For Each p In Process.GetProcesses
                If p.Id <> currentProcess.Id Then
                    If p.ProcessName = currentProcess.ProcessName Then
                        MessageBox.Show("TMDS already running!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Stop)
                        currentProcess.Kill()
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            proc.Dispose()
            proc = Nothing
            p = Nothing
            currentProcess = Nothing
        End Try
    End Sub

#End Region

#Region "LOAD FORMS PROPERTIES"

    Sub LoadFormProperties()
        Dim dr As OleDbDataReader
        Dim I As Integer
        Dim TopLeft() As String


        'Dim ConnString As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=c:\TMDS-WORKSTATION-SETTINGS.mdb"
        Dim dbConn As New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
        Dim FormQuery As String = "SELECT * FROM " & gtblWrkStnSettings & " WHERE WrkStnName = " & "'" & gWorkStationName & "'"
        Try
            Dim dbCommand As New OleDbCommand(FormQuery, dbConn)
            dbConn.Open()
            dr = dbCommand.ExecuteReader()
            While dr.Read()
                For I = 1 To 17
                    TopLeft = dr.GetString(I).Split("|")
                    If Not TopLeft Is Nothing Then
                        Select Case dr.GetName(I)
                            Case "FrmMainMDIProp"
                                If TopLeft.Length > 2 Then FrmMainMDIProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmLoginProp"
                                If TopLeft.Length > 2 Then FrmLoginProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmCopyToProp"
                                If TopLeft.Length > 2 Then FrmCopyToProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmAuthSumProp"
                                If TopLeft.Length > 2 Then FrmAuthSumProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmReportClearProp"
                                If TopLeft.Length > 2 Then FrmReportClearProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTrackWarrantProp"
                                If TopLeft.Length > 2 Then FrmTrackWarrantProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTerritoryListProp"
                                If TopLeft.Length > 2 Then FrmTerritoryListProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTrnSumProp"
                                If TopLeft.Length > 2 Then FrmTrnSumProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmBullSumProp"
                                If TopLeft.Length > 2 Then FrmBullSumProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmBulletinProp"
                                If TopLeft.Length > 2 Then FrmBulletinProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmGTBProp"
                                If TopLeft.Length > 2 Then FrmGTBProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmSysSumProp"
                                If TopLeft.Length > 2 Then FrmSysSumProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTrackTimeProp"
                                If TopLeft.Length > 2 Then FrmTrackTimeProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTrackAuthIssueToProp"
                                If TopLeft.Length > 2 Then FrmTrackAuthIssueToProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmDeviceBlkRemoveProp"
                                If TopLeft.Length > 2 Then FrmDeviceBlkRemoveProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmTagProp"
                                If TopLeft.Length > 2 Then FrmTagProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))
                            Case "FrmDeviceBlkProp"
                                If TopLeft.Length > 2 Then FrmDeviceBlkProp.FormLocation = New Point(CInt(TopLeft(0)), CInt(TopLeft(1)))

                        End Select
                    End If
                    TopLeft = Nothing
                Next
            End While
            dbCommand = Nothing
            dbConn.Close()
            dbConn = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            dbConn = Nothing
        End Try

    End Sub

#End Region

#Region "CRITICAL SYSTEM ROUTINES"

    Public Function CheckForValidSystemDatabasePath(ByVal PathToCheck As String, ByVal FileToCheck As String) As Boolean
        Try
            Dim SecondarySystemDatabaseFile As String = "C:\SystemDatabase.mdb"

            If Not Directory.Exists(PathToCheck) Then
                Directory.CreateDirectory(PathToCheck)
                If File.Exists(SecondarySystemDatabaseFile) Then
                    File.Copy(SecondarySystemDatabaseFile, FileToCheck)
                    Return True
                Else
                    Return False
                End If
            Else
                If Not File.Exists(FileToCheck) Then
                    If File.Exists(SecondarySystemDatabaseFile) Then
                        File.Copy(SecondarySystemDatabaseFile, FileToCheck)
                        Return True
                    Else
                        Return False
                    End If
                Else
                    If File.GetLastWriteTime(SecondarySystemDatabaseFile) > File.GetLastWriteTime(FileToCheck) Then
                        File.Copy(SecondarySystemDatabaseFile, FileToCheck, True)
                    End If
                    Return True
                End If
            End If


        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Public Sub LoadCursors()
        Dim MessageList As New ArrayList
        Try
            CursorPath = Application.StartupPath & "\Library\SystemCursors"
            If Not Directory.Exists(CursorPath) Then
                With MessageList
                    .Add("CURSOR FOLDER NOT FOUND")
                    SystemEvent("LoadCursors:CURSOR FOLDER NOT FOUND")
                End With
                'ShowCustomMessageBox(MessageList, "TMDS LOAD VALIDATION", False, Nothing, True, True, TMDS.SystemForms.MySystemForms.frmMainMDI)
                Exit Sub
            End If
            If CheckCursorInPlace(CursorPath, "TW-CREATE.cur") Then curCreateTW = New Cursor(CursorPath & "\TW-CREATE.cur")
            If CheckCursorInPlace(CursorPath, "curRollup.cur") Then curRollUpTW = New Cursor(CursorPath & "\curRollup.cur")
            'curDefault = New Cursor(CursorPath & "\normal_c.cur")
            curDefault = System.Windows.Forms.Cursors.Default
            If CheckCursorInPlace(CursorPath, "cursorPOSID.cur") Then curPositionID = New Cursor(CursorPath & "\cursorPOSID.cur")
            If CheckCursorInPlace(CursorPath, "cursorMove.cur") Then curMove = New Cursor(CursorPath & "\cursorMove.cur")
            If CheckCursorInPlace(CursorPath, "curNote.cur") Then curNote = New Cursor(CursorPath & "\curNote.cur")
            If CheckCursorInPlace(CursorPath, "curOccupy.cur") Then curOccupyTrack = New Cursor(CursorPath & "\curOccupy.cur")
            If CheckCursorInPlace(CursorPath, "curOSTrain.cur") Then curOSTrain = New Cursor(CursorPath & "\curOSTrain.cur")
            If CheckCursorInPlace(CursorPath, "curSelectID.cur") Then curSelectID = New Cursor(CursorPath & "\curSelectID.cur")
            If CheckCursorInPlace(CursorPath, "curTag1.cur") Then curPlaceTag = New Cursor(CursorPath & "\curTag1.cur")
            If CheckCursorInPlace(CursorPath, "needssen.cur") Then curSend = New Cursor(CursorPath & "\needssen.cur")
            If CheckCursorInPlace(CursorPath, "simulate.cur") Then curSimulateID = New Cursor(CursorPath & "\simulate.cur")
            If CheckCursorInPlace(CursorPath, "TT_CUR.cur") Then curCreateTT = New Cursor(CursorPath & "\TT_CUR.cur")
            If CheckCursorInPlace(CursorPath, "FT_CUR.cur") Then curCreateFT = New Cursor(CursorPath & "\FT_CUR.cur")
            If CheckCursorInPlace(CursorPath, "TP_CUR.cur") Then curCreateTP = New Cursor(CursorPath & "\TP_CUR.cur")
            If CheckCursorInPlace(CursorPath, "DB_cur.cur") Then curCreateDB = New Cursor(CursorPath & "\DB_cur.cur")
            If CheckCursorInPlace(CursorPath, "curDevice.cur") Then curDevice = New Cursor(CursorPath & "\curDevice.cur")
            If CheckCursorInPlace(CursorPath, "curEntExt.cur") Then curEntExit = New Cursor(CursorPath & "\curEntExt.cur")
            If CheckCursorInPlace(CursorPath, "curQuickRoute.cur") Then curQuickRoute = New Cursor(CursorPath & "\curQuickRoute.cur")
            If CheckCursorInPlace(CursorPath, "curPlaceNote.cur") Then curPlaceNote = New Cursor(CursorPath & "\curPlaceNote.cur")
            If CheckCursorInPlace(CursorPath, "curMoveNote.cur") Then curMoveNote = New Cursor(CursorPath & "\curMoveNote.cur")
            If CheckCursorInPlace(CursorPath, "curRecall.cur") Then curReCall = New Cursor(CursorPath & "\curRecall.cur")
            If CheckCursorInPlace(CursorPath, "curReSend.cur") Then curReSend = New Cursor(CursorPath & "\curReSend.cur")
            If CheckCursorInPlace(CursorPath, "curChangeOcc.cur") Then curChangeOcc = New Cursor(CursorPath & "\curChangeOcc.cur")
            If CheckCursorInPlace(CursorPath, "curChangeTraffic.cur") Then curChangeTraffic = New Cursor(CursorPath & "\curChangeTraffic.cur")
            If CheckCursorInPlace(CursorPath, "cursorLINKID.cur") Then curLinkTrainID = New Cursor(CursorPath & "\cursorLINKID.cur")
            If CheckCursorInPlace(CursorPath, "cursorStack.cur") Then curStackRoute = New Cursor(CursorPath & "\cursorStack.cur")
            If CheckCursorInPlace(CursorPath, "cursorGSR.cur") Then curGSR = New Cursor(CursorPath & "\cursorGSR.cur")
            If optEnableDTC Then
                If CheckCursorInPlace(CursorPath, "DTC-CREATE.cur") Then curCreateDTC = New Cursor(CursorPath & "\DTC-CREATE.cur")
                If CheckCursorInPlace(CursorPath, "curRollup.cur") Then curRollUpDTC = New Cursor(CursorPath & "\curRollup.cur")
            End If
            If CheckCursorInPlace(CursorPath, "TNTC_CUR.cur") Then curCreateTTContinue = New Cursor(CursorPath & "\TNTC_CUR.cur")
            If CheckCursorInPlace(CursorPath, "TNTR_CUR.cur") Then curTNTRollUp = New Cursor(CursorPath & "\TNTR_CUR.cur")
            If CheckCursorInPlace(CursorPath, "curTagCopy.cur") Then curCopyTag = New Cursor(CursorPath & "\curTagCopy.cur")
            If CheckCursorInPlace(CursorPath, "curContingentSelection.cur") Then curContingentSelection = New Cursor(CursorPath & "\curContingentSelection.cur")
            If CheckCursorInPlace(CursorPath, "GP_CUR.cur") Then curCreatePlan = New Cursor(CursorPath & "\GP_CUR.cur")
            If CheckCursorInPlace(CursorPath, "TR_CUR.cur") Then curCreateTrackReseveration = New Cursor(CursorPath & "\TR_CUR.cur")
            If CheckCursorInPlace(CursorPath, "MP_CUR.cur") Then curCreateManualRoute = New Cursor(CursorPath & "\MP_CUR.cur")
            If CheckCursorInPlace(CursorPath, "block_cu.cur") Then curWait = New Cursor(CursorPath & "\block_cu.cur")
            Try
                If CheckCursorInPlace(CursorPath, "curItemSixSelection.cur") Then curItemSixSelection = New Cursor(CursorPath & "\curItemSixSelection.cur")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Try
                If optLoadCursorsForCP Then
                    If CheckCursorInPlace(CursorPath, "TOP-ISSUE.cur") Then curCreateTOP = New Cursor(CursorPath & "\TOP-ISSUE.cur")
                    If CheckCursorInPlace(CursorPath, "564-ISSUE.cur") Then curCreate564 = New Cursor(CursorPath & "\564-ISSUE.cur")
                    If CheckCursorInPlace(CursorPath, "ClearanceProceesIssue.cur") Then curCreateClearanceProceed = New Cursor(CursorPath & "\ClearanceProceesIssue.cur")
                    'curClearTW = New Cursor(CursorPath & "\CLEAR.cur")
                    If CheckCursorInPlace(CursorPath, "curCompleteBy.cur") Then curCompleteBy = New Cursor(CursorPath & "\curCompleteBy.cur")
                    'curCompleteBy.cur
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Try
                If CheckCursorInPlace(CursorPath, "curEMT.cur") Then curEnterMainTrack = New Cursor(CursorPath & "\curEMT.cur")
                If CheckCursorInPlace(CursorPath, "curPRS.cur") Then curPassRedSignal = New Cursor(CursorPath & "\curPRS.cur")
                If CheckCursorInPlace(CursorPath, "curSwitch.cur") Then curPassRedSignalSW = New Cursor(CursorPath & "\curSwitch.cur")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Try
                If CheckCursorInPlace(CursorPath, "TRACKAUTHORITY_CUR.cur") Then curTrackAuthority = New Cursor(CursorPath & "\TRACKAUTHORITY_CUR.cur")
                If gTWCVariableName = "TA" Then
                    curCreateTW = curTrackAuthority
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            If CheckCursorInPlace(CursorPath, "cancel_c.cur") Then curCancelLicense = New Cursor(CursorPath & "\cancel_c.cur")
            If CheckCursorInPlace(CursorPath, "curBulletin.cur") Then curBulletin = New Cursor(CursorPath & "\curBulletin.cur")
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            MessageList = Nothing
        End Try
    End Sub
    Private Function CheckCursorInPlace(ByVal Cursorpath As String, ByVal CursorName As String) As Boolean
        Try
            Dim FullPath As String = Cursorpath & "\" & CursorName
            If File.Exists(FullPath) Then Return True
        Catch ex As Exception
            GetErrorDetails(ex)
            Return True
        End Try
    End Function

    Public Sub LoadSounds()
        Try
            gWavePath = Application.StartupPath & "\Library\SystemSounds"
            If Not Directory.Exists(gWavePath) Then Exit Sub
            WaveDefault = gWavePath & "\Default.wav"
            WaveStop = gWavePath & "\Stop.wav"
            WaveDing = gWavePath & "\Ding.wav"
            WaveHorn = gWavePath & "\Horn.wav"
            'WaveChord = gWavePath & "\Chord.wav"
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Clear system variables
    ''' </summary>
    ''' <param name="escKeyPressed">Esc Key Pressed</param>
    Public Sub ClearSystemVariables(Optional escKeyPressed As Boolean = False)
        Try
            SystemEvent($"{GetCurrentMethod.Name}:ESCKeyPressed={escKeyPressed _
                        }:CallingMethod={(New StackTrace).GetFrame(1).GetMethod().Name}")
            gPermissionToRemoveStopTargetSignal = Nothing
            gPermissionToRemoveStopTarget = False
            gSelectedSignalRouteList = New ArrayList
            gPerformingReverseSearch = False
            gTrackLinkedReverse = False
            gTrackLinkedReverseDirection = ""
            If TMDS.SystemForms.MySystemForms.frmMainMDI.InvokeRequired Then
                TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.StopControlPointFlash))
                TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.BringToFront))
            Else
                TMDS.SystemForms.MySystemForms.frmMainMDI.StopControlPointFlash()
                TMDS.SystemForms.MySystemForms.frmMainMDI.BringToFront()
            End If
            'Set Cursur back to default
            If gGropupedStoredRoute Then
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curDefault)
                gGropupedStoredRoute = False
            ElseIf Not gQuickRouteMode And Not gStackMode Then
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curDefault)
                gStackRoute = False
            ElseIf gStackMode Then
                gStackRoute = True
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curStackRoute)
            ElseIf gQuickRouteMode Then
                gStackRoute = False
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curQuickRoute)
            Else
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(TMDS.GlobalDeclarations.curDefault)
            End If
            If Not TMDS.SystemForms.MySystemForms.frmTrainStack Is Nothing Then
                TMDS.SystemForms.MySystemForms.frmTrainStack.ResetCursor()
            End If
            gGropupedStoredRouteParent = Nothing
            gDoStoredRoute = False
            gGropupedStoredRoute = False
            If Not gStoredRouteList Is Nothing Then
                gStoredRouteList.Clear()
            End If
            If gCreateTrainPassRed564 Then
                Dim comp As Object
                For Each comp In gEntExtTrackSelection
                    comp.UpdateStatus()
                Next
            End If
            gCurrentAuthorityFlash = False
            gCurrentAuthorityFlashShow = False
            TMDS.SystemUtilities.Utilities.ClearCurrentAuthorityListSelection()
            Dim CompToTest As Object
            For Each CompToTest In gTrackSelectionCollection
                If CompToTest.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If optApplyConfigurationForKCT AndAlso (CompToTest.IsShadowRemoteTrack) AndAlso CompToTest.myRemoteTrack IsNot Nothing Then
                        CompToTest.myRemoteTrack.SendBlockPendingStatus(False, False)
                    End If
                    If CompToTest.IsRemoteTrack Then
                        CompToTest.myRemoteTrack.SendBlockPendingStatus(False, False)
                    End If
                    If CompToTest.IsLinkToRemoteTrack Then
                        'CompToTest.RemotePendingBlock = False
                    End If

                End If
            Next
            'Disable the track selection flash timer
            TMDS.SystemForms.MySystemForms.frmMainMDI.tmrSelectionFlasher.Enabled = False
            SystemForms.MySystemForms.frmMainMDI.tmrTrackSelectionTWC.Enabled = False
            TMDS.SystemForms.MySystemForms.frmMainMDI.SelectionClear()
            'Clear the global variable for track selection
            gTrainSetHeadEndToTrack = False
            gTrainSetRearEndToTrack = False
            gTunnelMaintenanceMode = False
            gTNTComplete = False
            gTNTContinue = False
            gDTCComplete = False
            gBeginTrackSelect = Nothing
            gEndTrackSelect = Nothing
            gEndTrackSelectionReplacement = Nothing
            gSearchDirection = ""
            gTypeOfPersonCopying = ""
            gPersonCopying = ""
            gSubdivisionDirection = False
            gChangeTraffic = False
            gChangeOccupancy = False
            gSearchAgain = False
            gRemoveLicense = False
            gRemoveLicenseTrainSelectedID = Nothing
            gPermissionToRemoveStopTarget = False
            gMultipleSelectModeForTNT = False
            gTrackSelectionCollection.Clear()
            'Clear the global variables for Track Warrant
            gIssueToSelected = ""
            gLaunchCreateItemFourToSameTrain = False
            gLaunchCreateItemTwoToTrain = False
            gCreateTrackWarrant = False
            gRollUpTrackWarrant = False
            gOverLapCheckNeeded = False
            gSubdivisionDirection = False
            gOverLapValid = False
            gSearchDirection = ""
            gSearchDirectionLocked = ""
            gCurrentTrackWarrantIndex = 0
            gWarrantType = ""
            gItemSixRequired = False
            gItemSevenRequired = False
            gBoxTwoRequired = False
            gBoxThreeRequired = False
            gBoxFourRequired = False
            gBoxOneRequired = False
            gBoxTenRequired = False
            gItemFiveRequired = False
            gRepeatMode = False
            gOverLapValid = False
            gBox8Required = False
            gJointAuthRequired = False
            gTrackOccupancy = False
            gBoxThirteenRequired = False
            gBoxFourteenRequired = False
            gBoxTwelveRequired = False
            gItem10Required = False
            gBox13Required = False
            gBox14Required = False
            gBox15Required = False
            gBox17Required = False
            gBox11Required = False
            gItemTwoSameTrainWithRadioBlocking = False
            gItemTwoToTrainWithRadioBlocking = False
            gItemTwoFiveVoidedAuthorityCandidate = Nothing
            gTrackSelectOrginatingOutOfSiding = False
            gTrackSelectEndingIntoSiding = False
            If gContingentMode Or gDualAuthorityMode Then
                SystemForms.MySystemForms.frmMainMDI.tmrTrackSelectionContingent.Enabled = False
                If TMDS.SystemForms.MySystemForms.frmMainMDI.InvokeRequired Then
                    TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.TWCCSelectionClear))
                Else
                    TMDS.SystemForms.MySystemForms.frmMainMDI.TWCCSelectionClear()
                End If
            End If
            gContingentTrackList.Clear()
            gContingentMode = False
            gBeginTrackSelectContingent = Nothing
            gEndTrackSelectContingent = Nothing
            gTrainMenuMiscSelectedTrainID = Nothing
            gTrainMenuMiscSelectedTrack = Nothing
            gDualAuthorityMode = False
            If gCreateLicenseTrain IsNot Nothing Then
                gCreateLicenseTrain.UserActionType = UserActionTypeEnum.None
                gCreateLicenseTrain.SLFAEnabled = False
                gCreateLicenseTrain.SLFATrackList.Clear()
            End If
            gCreateLicenseDTC = False
            gCreateLicenseTrain = Nothing
            gCancellingStartSignalForLicense = Nothing
            If gSelectedTrainIDTrack IsNot Nothing AndAlso
                gSelectedTrainIDTrack.TrainIDs.Count > 0 Then
                Dim train As TrainID = gSelectedTrainIDTrack.TrainIDs.Item(0)
                If escKeyPressed AndAlso
                    train.ClickedOnTrainForLicense Then
                    train.ClickedOnTrainForLicense = False
                    If train.thisCurrentLicense Is Nothing AndAlso
                            train.IsBIDirectional Then
                        train.IsBIDirectional = False
                    End If
                End If
                If Not train.SLFACreateTrain AndAlso
                    train.SLFATimer IsNot Nothing AndAlso
                    train.SLFATimer.Enabled Then
                    train.CancelSLFA(train)
                End If
            End If
            'ARRC
            gCreateAuthorityTrain = Nothing
            gCreateAuthorityTypeTrain = String.Empty
            gCreateTrackAuthorityTrain = False
            gCreateAuthorityMOW = Nothing
            gCurrentMOWTrack = Nothing
            gCreateAuthorityTypeMOW = String.Empty
            gCreateTrackAuthorityMOW = False
            'DTC VARIABLES
            gCreateDTCWarrant = False
            gRollUpDTCWarrant = False
            gCreateDTCWarrantToMOW = False
            gCreateDTCWarrantToTrain = False
            'Clear Train ID Variables
            gRouteCheckTrainSelectedID = Nothing
            gDeviceSelectMode = False
            gSimulateTrain = False
            gSimulateTrainStop = False
            gPositionTrainID = False
            gMoveTrainSelected = False
            gModifyTrainID = False
            gCreateTrain = False
            If gMoveTrainID Or gOSTrain Then
                If Not gMoveTrainSelectedTrack Is Nothing Then
                    gMoveTrainSelectedTrack.InitTrainDisplay()
                    If TMDS.SystemForms.MySystemForms.frmMainMDI.InvokeRequired Then
                        TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.ActiveMdiChild.Refresh))
                    Else
                        SystemForms.MySystemForms.frmMainMDI.ActiveMdiChild.Refresh()
                    End If
                End If
                If gMoveTrainID Then
                    If TMDS.SystemForms.MySystemForms.frmSystemSummary.InvokeRequired Then
                        TMDS.SystemForms.MySystemForms.frmSystemSummary.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmSystemSummary.ClearTrackSelection))
                    Else
                        TMDS.SystemForms.MySystemForms.frmSystemSummary.ClearTrackSelection()
                    End If
                End If
            End If
            gCancelLicenseToTrain = False
            gOSTrain = False
            gMoveTrainID = False
            gMoveTrainSelectedTrack = Nothing
            gMoveTrainFromTrainStackForm = False
            gMoveTrainSelectedID = Nothing
            gPlaceOSTrain = False
            gAddDelayTrain = False
            gFinalTrain = False
            gPermissionPassRedSignal = False
            gPermissionEnterMainTrack = False
            gPermissionPassRedSignalSwitchSelectionMode = False
            gPermissionPassRedSignalSwitches = New ArrayList
            gPermissionPassRedSignalControlPoint = Nothing
            gPermissionPassRedSignalSelectionCompleted = False
            gAllowPSSSwitchSelection = True
            If gHelperTrainSelected Then
                If Not gLinkTrainSelectedTrack Is Nothing Then
                    gLinkTrainSelectedTrack.InitTrainDisplay()
                    If SystemForms.MySystemForms.frmMainMDI.InvokeRequired Then
                        TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.ActiveMdiChild.Refresh))
                    Else
                        SystemForms.MySystemForms.frmMainMDI.ActiveMdiChild.Refresh()
                    End If
                End If
            End If
            gHelperTrainSelected = False
            gLinkTrainSelectedTrack = Nothing
            gHelperTrainSelectedID = Nothing
            gLinkTrainSelectedID = Nothing
            'MOW ID
            gCreateMOWID = False
            gDeleteMOWID = False
            gMoveMOWID = False
            gMoveMOWSelected = False
            gMoveMOWSelectedTrack = Nothing
            gMoveMOWSelectedID = Nothing
            gMowMenuMiscSelectedID = Nothing
            gMowMenuMiscSelectedTrack = Nothing
            gForemanMenuMiscSelectedID = Nothing
            gCreateTOP = False
            gCreateCPTNT = False
            gCreateCPFT = False
            'Other Type of Authority Variables
            gCreateTrackTime = False
            gCreateTrackTimeIssuedToTrain = False
            gCreateFoulTime = False
            gCreateTrackPermit = False
            gCreateDeviceBlock = False
            gCreateBlueFlagBlock = False
            gBlockOSDevices = False
            gBlockAllDevices = False
            gBlockTrackDevices = False
            gBlockSwitchDevices = False
            gBlockSignalDevices = False
            gBlockMiscDevices = False
            gCreateTrackPermit = False
            gSwitchListForAutoRevereseForTNT.Clear()
            If gDeviceBlockSelection.Count > 0 Then
                If TMDS.SystemForms.MySystemForms.frmMainMDI.InvokeRequired Then
                    TMDS.SystemForms.MySystemForms.frmMainMDI.Invoke(New MethodInvoker(AddressOf TMDS.SystemForms.MySystemForms.frmMainMDI.ClearDeviceSelection))
                Else
                    SystemForms.MySystemForms.frmMainMDI.ClearDeviceSelection()
                End If
            End If
            gDeviceBlockSelection.Clear()
            'Clear Other System Variables
            gSelectedNote = Nothing
            gCreateNote = False
            gCreateNonTag = False
            gCreateDelayTag = False
            gPlaceNote = False
            'signal drill stuff
            gSelectedSignal = Nothing
            gSelectedSignalUserClickedinStop = False
            'Clear Tag Variables
            gCreateTag = False
            gCreateTagType = ""
            gCreateTagDataList = Nothing
            gTagToCopy = Nothing
            gCopyTag = False
            SystemForms.MySystemForms.frmMainMDI.tmrFlasherTrackTag.Enabled = False
            SystemForms.MySystemForms.frmMainMDI.ClearTrackTagSelection()
            gTagTrackSelectionList.Clear()
            gTagSelectionDirection = ""
            gTagLeftExitTrack = 0
            gTagRightExitTrack = 0
            gCreateBulletin = False
            gCreateBulletinType = ""
            'Simulator
            gSimPositionReport = False
            gSimReleaseEot = False
            'CTC Variables
            gSendCTC = False
            gEntExit = False
            gReCall = False
            gReSend = False
            'MRS 587
            SystemForms.MySystemForms.frmMainMDI.tmrTrackSelectionEntExt.Enabled = False
            If gEntExtTrackSelection.Count > 0 Then
                SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
            End If
            gEntExtTrackSelection.Clear()
            gEntExtTrackSelection = New ArrayList
            'MRS 587
            gEntryTrack = Nothing
            gExitTrack = Nothing
            gFirstEntryTrack = Nothing
            gTagAuthorityCheckDone = False
            'Put the track type menu selection back to the default
            TMDS.SystemUtilities.UtilitiesDisplayForm.ResetMenuItems()
            SystemForms.MySystemForms.frmMainMDI.slblLimitsLeft.Text = "LL:"
            SystemForms.MySystemForms.frmMainMDI.slblLimitsRight.Text = "RL:"
            If gMoveNote Then
                If Not gMoveNoteSelectedNote Is Nothing Then
                    Dim frm As Form = SystemForms.MySystemForms.frmMainMDI.ActiveMdiChild
                    gMoveNoteSelectedNote.Graphical = frm.CreateGraphics
                    gMoveNoteSelectedNote.HighLightNote(False)
                    gMoveNoteSelectedNote.BuildGraphics()
                    gMoveNoteSelectedNote.InvalidateNote(frm)
                End If
            End If
            gMoveNote = False
            gMoveNoteSelectedNote = Nothing
            'planning
            gGenerateRouteForTrainMode = False
            gCurrentTrainSelectedForManualRoute = Nothing
            gManualRouteSelectionList.Clear()
            gManualRouteSelectionList = New ArrayList
            gCreateTrackreservationMode = False
            gManualRouteList.Clear()
            '*********************  START PSS Variables *******************************
            If optEnableMenuPermissionToPassRedSignal Then
                ForcedSwitchCollection = New ArrayList
                PSSOriginalCompCollection = New ArrayList
                ExemptTrackCollection = New ArrayList
                ExemptSignalCollection = New ArrayList
                ForcedSwitch = Nothing
                gFirstOSTrack = Nothing
                ExemptTrack = Nothing
                ExemptSignal = Nothing
                ValidatedSignal = Nothing
                PSSEntrySignal = Nothing
                PSSSameMain = True
                PSSSelectedSwitchCollection = New ArrayList
                PSSNonSelectedSwitchCollection = New ArrayList
                PSSReversedSwitchCollection = New ArrayList
                PSSVerticalTrack = False
                'Clear the current end signals
                PSSEndSignals = New ArrayList
            End If
            '*********************  END PSS Variables *******************************
            'Clear CP Variables
            If optRailRoadName = "CP" Then ClearCPSpecificVariables()
            If Not IsNothing(TMDS.SystemForms.MySystemForms.frmSystemSummary) Then
                If TMDS.SystemForms.MySystemForms.frmSystemSummary.InvokeRequired Then
                Else
                    TMDS.SystemForms.MySystemForms.frmSystemSummary.Cursor = Cursors.Default
                End If
            End If
            If optEnableTerritoryDeskAssignmentMode AndAlso gInUseComponentCollection IsNot Nothing AndAlso gInUseComponentCollection.Count > 0 Then
                ReleaseJointControlComponents()
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            MsgBox("SYSTEM ERROR! CONTACT ADMINISTRATOR", MsgBoxStyle.Critical)
        Finally
            optMandatoryDirectivesInUse = False
        End Try
    End Sub

    Private Sub ClearCPSpecificVariables()
        Try
            '*********************  START CP Variables *******************************'
            gSearchDirectionLocked = ""
            Dim SFA As TMDS.TrackAuthority.SFAuthority
            Dim Auth As Object = Nothing
            Try
                If gCPReportClearSFACollection IsNot Nothing AndAlso gCPReportClearSFACollection.Count > 0 Then
                    For Each SFA In gCPReportClearSFACollection
                        If Not SFA.PTCTypeAuthority Then
                            SFA.FlasherMode(False)
                        ElseIf SFA.PTCTypeAuthority AndAlso SFA.PTCAuthorityACKPending.Trim = "" Then
                            SFA.FlasherMode(False)
                        End If
                    Next
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                SFA = Nothing
            End Try

            Try
                For Each Auth In OtherTrackAuthorities
                    If Auth.GetType.ToString = gTTComponentName Then
                        If Not Auth.PTCTypeAuthority Then
                            Auth.FlasherMode(False)
                        ElseIf Auth.PTCTypeAuthority AndAlso Auth.PTCAuthorityACKPending.Trim = "" Then
                            Auth.FlasherMode(False)
                        End If
                    ElseIf Auth.GetType.ToString = gSFAComponentName Then
                        If Not Auth.PTCTypeAuthority Then
                            Auth.FlasherMode(False)
                        ElseIf Auth.PTCTypeAuthority AndAlso Auth.PTCAuthorityACKPending.Trim = "" Then
                            Auth.FlasherMode(False)
                        End If
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                Auth = Nothing
            End Try


            gTWCEntrySwitches = New ArrayList
            gTWCEntryTracks = New ArrayList
            gCPReportClearMissingSFAToTrainCollection = New ArrayList
            gCPReportClearSFACollection = New ArrayList
            gCPErrorCollectionList = New ArrayList
            gCreateCPEMT = False
            gCreateTrainPassRed564 = False
            gCreateIssueClearanceProceed = False
            gCreateIssueTrackWarrantProceedForTrain = False
            gCreateIssueTrackWarrantWorkForTrain = False
            gCreateIssueTrackWarrantWorkForMOW = False
            gCreateIssueTrackWarrantRollUpForTrain = False
            gCreateIssueTrackWarrantRollUpForMow = False
            gCPRollupComps = New ArrayList
            gCPRollUpSFA = Nothing
            gCPRollupTrack = Nothing
            gEndCPTATSelection = False
            gTNTISSUEToTrain = False
            gPermissionPassRedEntrySignal = Nothing
            gCPTNTRollUpForTrain = False
            gCPTNTRollUpForMow = False
            gCPRollUpTNT = Nothing
            gCPTWCTrainReportClearMode = False
            ReinitFlipDirection()
            RemoveHighLightMWsAuthority()
            gCPAuthDirection = ""
            '*********************  END CP Variables *******************************
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Friend Sub ClearTempQuickRouteVariable()
        Try
            'Clear StackRoute Temp Variables
            If Not optEnablePreseveQuickRouteCollectionOnInvalidClick Then Exit Sub
            gTmpRouteCollection.Clear()
            gTmpRouteSignalCollection.Clear()
            gTmpRouteCollection = New ArrayList
            gTmpRouteSignalCollection = New ArrayList
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

#End Region

#Region "LOAD GLOBAL Language References "

    Public Sub LoadGlobalSpanishLanguageReferences()
        Dim rm As ResourceManager
        Try
            Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo(gLocale)
            rm = ResourceManager.CreateFileBasedResourceManager(gSpanishResoureFileName, ".", Nothing)

            optSystemMessageString1 = rm.GetString("optSystemMessageString1")
            optSystemMessageString2 = rm.GetString("optSystemMessageString2")
            optSystemMessageString3 = rm.GetString("optSystemMessageString3")
            optSystemMessageString4 = rm.GetString("optSystemMessageString4")
            optSystemMessageString5 = rm.GetString("optSystemMessageString5")
            optSystemMessageString6 = rm.GetString("optSystemMessageString6")
            optSystemMessageString7 = rm.GetString("optSystemMessageString7")
            optSystemMessageString8 = rm.GetString("optSystemMessageString8")
            optSystemMessageString9 = rm.GetString("optSystemMessageString9")
            optSystemMessageString10 = rm.GetString("optSystemMessageString10")
            optSystemMessageString11 = rm.GetString("optSystemMessageString11")
            optSystemMessageString12 = rm.GetString("optSystemMessageString12")
            optSystemMessageString13 = rm.GetString("optSystemMessageString13")
            optSystemMessageString14 = rm.GetString("optSystemMessageString14")


            optSystemDateTimeString = rm.GetString("optSystemDateTimeString")

            optSystemValidationString1 = rm.GetString("optSystemValidationString1")
            optSystemValidationString2 = rm.GetString("optSystemValidationString2")


            optTrainReportString1 = rm.GetString("optTrainReportString1")
            optTrainReportString2 = rm.GetString("optTrainReportString2")
            optTrainReportString3 = rm.GetString("optTrainReportString3")
            optTrainReportString4 = rm.GetString("optTrainReportString4")
            optTrainReportString5 = rm.GetString("optTrainReportString5")
            optTrainReportString6 = rm.GetString("optTrainReportString6")
            optTrainReportString7 = rm.GetString("optTrainReportString7")
            optTrainReportString8 = rm.GetString("optTrainReportString8")
            optTrainReportString9 = rm.GetString("optTrainReportString9")
            optTrainReportString10 = rm.GetString("optTrainReportString10")
            optTrainReportString11 = rm.GetString("optTrainReportString11")
            optTrainReportString12 = rm.GetString("optTrainReportString12")
            optTrainReportString13 = rm.GetString("optTrainReportString13")
            optTrainReportString14 = rm.GetString("optTrainReportString14")
            optTrainReportString15 = rm.GetString("optTrainReportString15")
            optTrainReportString16 = rm.GetString("optTrainReportString16")
            optTrainReportString17 = rm.GetString("optTrainReportString17")
            optTrainReportString18 = rm.GetString("optTrainReportString18")
            optTrainReportString19 = rm.GetString("optTrainReportString19")
            optTrainReportString20 = rm.GetString("optTrainReportString20")
            optTrainReportString21 = rm.GetString("optTrainReportString21")
            optTrainReportString22 = rm.GetString("optTrainReportString22")
            optTrainReportString23 = rm.GetString("optTrainReportString23")
            optTrainReportString24 = rm.GetString("optTrainReportString24")
            optTrainReportString25 = rm.GetString("optTrainReportString25")
            optTrainReportString26 = rm.GetString("optTrainReportString26")
            optTrainReportString27 = rm.GetString("optTrainReportString27")
            optTrainReportString28 = rm.GetString("optTrainReportString28")
            optTrainReportString29 = rm.GetString("optTrainReportString29")
            optTrainReportString30 = rm.GetString("optTrainReportString30")
            optTrainReportString31 = rm.GetString("optTrainReportString31")
            optTrainReportString32 = rm.GetString("optTrainReportString32")
            optTrainReportString33 = rm.GetString("optTrainReportString33")
            optTrainReportString34 = rm.GetString("optTrainReportString34")
            optTrainReportString35 = rm.GetString("optTrainReportString35")
            optTrainReportString36 = rm.GetString("optTrainReportString36")
            optTrainReportString37 = rm.GetString("optTrainReportString37")
            optTrainReportString38 = rm.GetString("optTrainReportString38")
            optTrainReportString39 = rm.GetString("optTrainReportString39")
            optTrainReportString40 = rm.GetString("optTrainReportString40")
            optTrainReportString41 = rm.GetString("optTrainReportString41")


            optAuthTypeJoint = rm.GetString("optAuthTypeJoint")
            optAuthTypeSole = rm.GetString("optAuthTypeSole")
            optAuthTypeNone = rm.GetString("optAuthTypeNone")
            optAuthType1 = rm.GetString("optAuthType1") '"ITEM 6 TO TRAIN"
            optAuthType2 = rm.GetString("optAuthType2") '"ITEM 6 TO MOW"
            optAuthType3 = rm.GetString("optAuthType3") '"ITEM 2/4 TO TRAIN"
            optAuthType4 = rm.GetString("optAuthType4") '"ITEM 2 TO TRAIN"
            optAuthType5 = rm.GetString("optAuthType5") '"ITEM 4 ONLY TO TRAIN"

            optAuthCheckString1 = rm.GetString("optAuthCheckString1")
            optAuthCheckString2 = rm.GetString("optAuthCheckString2")
            optAuthCheckString3 = rm.GetString("optAuthCheckString3")
            optAuthCheckStringItem7Str1 = rm.GetString("optAuthCheckStringItem7Str1")
            optAuthCheckStringItem7Str2 = rm.GetString("optAuthCheckStringItem7Str2")
            optAuthCheckString5 = rm.GetString("optAuthCheckString5")
            optAuthCheckString6 = rm.GetString("optAuthCheckString6")
            optAuthReadAuthorityString = rm.GetString("optAuthReadAuthorityString")
            optAuthTrackTimeNoString = rm.GetString("mstr_frmAuth_lblAuthNum").ToUpper
            optAuthTrackTimeString = rm.GetString("frmAuthority_Name").ToUpper

            optPDFEndTT = rm.GetString("optPDFEndTT")
            optPDFCopiedBy = rm.GetString("optPDFCopiedBy")
            optPDFRelayedTo = rm.GetString("optPDFRelayedTo")

            optTrainString1 = rm.GetString("optTrainString1")
            optTrainString2 = rm.GetString("optTrainString2")
            optTrainString3 = rm.GetString("optTrainString3")
            optTrainString4 = rm.GetString("optTrainString4")
            optTrainString5 = rm.GetString("optTrainString5")
            optTrainString6 = rm.GetString("optTrainString6")
            optTrainString7 = rm.GetString("optTrainString7")
            optTrainString8 = rm.GetString("optTrainString8")
            optTrainString9 = rm.GetString("optTrainString9")
            optTrainString10 = rm.GetString("optTrainString10")
            optTrainString11 = rm.GetString("optTrainString11")
            optTrainString12 = rm.GetString("optTrainString12")
            optTrainString13 = rm.GetString("optTrainString13")
            optTrainString14 = rm.GetString("optTrainString14")
            optTrainString15 = rm.GetString("optTrainString15")
            optTrainString16 = rm.GetString("optTrainString16")
            optTrainString17 = rm.GetString("optTrainString17")
            optTrainString18 = rm.GetString("optTrainString18")
            optTrainString19 = rm.GetString("optTrainString19")
            optTrainString20 = rm.GetString("optTrainString20")
            optTrainString21 = rm.GetString("optTrainString21")


            optChangeDirectionString1 = rm.GetString("optChangeDirectionString1").ToUpper
            optChangeDirectionString2 = rm.GetString("optChangeDirectionString2").ToUpper
            optChangeDirectionString3 = rm.GetString("optChangeDirectionString3").ToUpper
            optChangeDirectionString4 = rm.GetString("optChangeDirectionString4").ToUpper
            optChangeDirectionString5 = rm.GetString("optChangeDirectionString5").ToUpper
            optChangeDirectionString6 = rm.GetString("optChangeDirectionString6").ToUpper
            optChangeDirectionString7 = rm.GetString("optChangeDirectionString7").ToUpper
            optChangeDirectionString8 = rm.GetString("optChangeDirectionString8").ToUpper
            optChangeDirectionString9 = rm.GetString("optChangeDirectionString9").ToUpper
            optChangeDirectionMessageHeader = rm.GetString("optChangeDirectionMessageHeader").ToUpper
            optChangeDirectionString11 = rm.GetString("optChangeDirectionString11").ToUpper
            optChangeDirectionString12 = rm.GetString("optChangeDirectionString12").ToUpper
            optChangeDirectionString13 = rm.GetString("optChangeDirectionString13").ToUpper
            optChangeDirectionString14 = rm.GetString("optChangeDirectionString14").ToUpper

            optSystemSummaryName = rm.GetString("optSystemSummaryName").ToUpper
            optSystemSummaryAuthPreviewFormName = rm.GetString("optSystemSummaryAuthPreviewFormName").ToUpper

            optLanguageDisplayFormName = rm.GetString("OptLanguageDisplayFormName").ToUpper
            optLanguageMessageBoxCaptionDefault1 = rm.GetString("OptLanguageMessageBoxCaptionDefault1").ToUpper

            optGraphicalCompContextMenuPositionTrain = rm.GetString("optGraphicalCompContextMenuPositionTrain").ToUpper
            optGraphicalCompContextMenuTNTIssue = rm.GetString("optGraphicalCompContextMenuTNTIssue").ToUpper
            optGraphicalCompContextMenuTNTClear = rm.GetString("optGraphicalCompContextMenuTNTClear").ToUpper
            optGraphicalCompContextMenuTNTModify = rm.GetString("optGraphicalCompContextMenuTNTModify").ToUpper

            optGraphicalCompContextMenuTNTRollUp = rm.GetString("optGraphicalCompContextMenuTNTRollUp").ToUpper
            optGraphicalCompContextMenuTWCIssue = rm.GetString("optGraphicalCompContextMenuTWCIssue").ToUpper
            optGraphicalCompContextMenuTWCClear = rm.GetString("optGraphicalCompContextMenuTWCClear").ToUpper
            optGraphicalCompContextMenuTWCRollUp = rm.GetString("optGraphicalCompContextMenuTWCRollUp").ToUpper
            optGraphicalCompContextMenuTWCVOID = rm.GetString("optGraphicalCompContextMenuTWCVOID").ToUpper

            optGraphicalCompContextMenuTagCreate = rm.GetString("optGraphicalCompContextMenuTagCreate").ToUpper
            optGraphicalCompContextMenuTagSpeed = rm.GetString("optGraphicalCompContextMenuTagSpeed").ToUpper
            optGraphicalCompContextMenuTagXing = rm.GetString("optGraphicalCompContextMenuTagXing").ToUpper
            optGraphicalCompContextMenuTagInfo = rm.GetString("optGraphicalCompContextMenuTagInfo").ToUpper
            optGraphicalCompContextMenuTagRemove = rm.GetString("optGraphicalCompContextMenuTagRemove").ToUpper
            optGraphicalCompContextMenuTagModify = rm.GetString("optGraphicalCompContextMenuTagModify").ToUpper
            optGraphicalCompContextMenuTagCopy = rm.GetString("optGraphicalCompContextMenuTagCopy").ToUpper

            optGraphicalCompContextMenuBlockCreate = rm.GetString("optGraphicalCompContextMenuBlockCreate").ToUpper
            optGraphicalCompContextMenuBlockRemove = rm.GetString("optGraphicalCompContextMenuBlockRemove").ToUpper
            optGraphicalCompContextMenuBlockOS = rm.GetString("optGraphicalCompContextMenuBlockOS").ToUpper
            optGraphicalCompContextMenuTrackReset = rm.GetString("optGraphicalCompContextMenuTrackReset").ToUpper

            optGraphicalCompContextMenuSwBlockCreate = rm.GetString("optGraphicalCompContextMenuSwBlockCreate").ToUpper
            optGraphicalCompContextMenuSwBlockRemove = rm.GetString("optGraphicalCompContextMenuSwBlockRemove").ToUpper
            optGraphicalCompContextMenuSwBlockOS = rm.GetString("optGraphicalCompContextMenuSwBlockOS").ToUpper
            optGraphicalCompContextMenu_mnuRECALL = rm.GetString("optGraphicalCompContextMenu_mnuRECALL").ToUpper
            optGraphicalCompContextMenu_mnuRESEND = rm.GetString("optGraphicalCompContextMenu_mnuRESEND").ToUpper
            optGraphicalCompContextMenu_mnuRoutes = rm.GetString("optGraphicalCompContextMenu_mnuRoutes").ToUpper
            optGraphicalCompContextMenu_mnuReleaseControl = rm.GetString("optGraphicalCompContextMenu_mnuReleaseControl").ToUpper
            optGraphicalCompContextMenu_mnuMCZ = rm.GetString("optGraphicalCompContextMenu_mnuMCZ").ToUpper


            optGraphicalCompProtectSwitch1 = rm.GetString("optGraphicalCompProtectSwitch1").ToUpper
            optGraphicalCompProtectSwitch2 = rm.GetString("optGraphicalCompProtectSwitch2").ToUpper
            optGraphicalCompProtectSwitch3 = rm.GetString("optGraphicalCompProtectSwitch3").ToUpper
            optGraphicalCompProtectSwitchMsgHeader = rm.GetString("optGraphicalCompProtectSwitchMsgHeader").ToUpper
            optRollUpAuthorityString1 = rm.GetString("optRollUpAuthorityString1").ToUpper
            optRollUpAuthorityString2 = rm.GetString("optRollUpAuthorityString2").ToUpper
            optRollUpAuthorityString3 = rm.GetString("optRollUpAuthorityString3").ToUpper
            optRollUpAuthorityString4 = rm.GetString("optRollUpAuthorityString4").ToUpper
            optRollUpAuthorityString5 = rm.GetString("optRollUpAuthorityString5").ToUpper
            optRollUpAuthorityString6 = rm.GetString("optRollUpAuthorityString6").ToUpper
            optRollUpAuthorityString7 = rm.GetString("optRollUpAuthorityString7").ToUpper
            optRollUpAuthorityString8 = rm.GetString("optRollUpAuthorityString8").ToUpper
            optRollUpAuthorityString9 = rm.GetString("optRollUpAuthorityString9").ToUpper
            optRollUpAuthorityString10 = rm.GetString("optRollUpAuthorityString10").ToUpper
            optRollUpAuthorityString11 = rm.GetString("optRollUpAuthorityString11").ToUpper
            optRollUpAuthorityString12 = rm.GetString("optRollUpAuthorityString12").ToUpper
            optRollUpAuthorityString13 = rm.GetString("optRollUpAuthorityString13").ToUpper
            optRollUpAuthorityString14 = rm.GetString("optRollUpAuthorityString14").ToUpper
            optRollUpAuthorityString15 = rm.GetString("optRollUpAuthorityString15").ToUpper
            optRollUpAuthorityString16 = rm.GetString("optRollUpAuthorityString16").ToUpper
            optRollUpAuthorityString17 = rm.GetString("optRollUpAuthorityString17").ToUpper
            optRollUpAuthorityString18 = rm.GetString("optRollUpAuthorityString18").ToUpper
            optRollUpAuthorityString19 = rm.GetString("optRollUpAuthorityString19").ToUpper
            optRollUpAuthorityString20 = rm.GetString("optRollUpAuthorityString20").ToUpper
            optRollUpAuthorityString21 = rm.GetString("optRollUpAuthorityString21").ToUpper
            optRollUpAuthorityString22 = rm.GetString("optRollUpAuthorityString22").ToUpper

            'optRollUpAuthority_TitleOne_WN = rm.GetString("optRollUpAuthority_TitleOne_WN").ToUpper
            'optRollUpAuthority_TitleTWO_ES = rm.GetString("optRollUpAuthority_TitleTWO_ES").ToUpper
            'optRollUpAuthority_ButtonStringOne_WN = rm.GetString("optRollUpAuthority_ButtonStringOne_WN").ToUpper
            'optRollUpAuthority_ButtonStringTwo_ES = rm.GetString("optRollUpAuthority_ButtonStringTwo_ES").ToUpper
            optRollUpAuthority_TitleOne_WS = rm.GetString("optRollUpAuthority_TitleOne_WS").ToUpper
            optRollUpAuthority_TitleOne_EN = rm.GetString("optRollUpAuthority_TitleOne_EN").ToUpper
            'optRollUpAuthority_ButtonStringOne_WS = rm.GetString("optRollUpAuthority_ButtonStringOne_WS").ToUpper
            'optRollUpAuthority_ButtonStringTwo_EN = rm.GetString("optRollUpAuthority_ButtonStringTwo_EN").ToUpper
            optAuthString1 = rm.GetString("optAuthString1").ToUpper '"PROCEED FROM "
            optAuthString2 = rm.GetString("optAuthString2").ToUpper '" TO "
            optAuthString3 = rm.GetString("optAuthString3").ToUpper '" ON "
            optAuthString4 = rm.GetString("optAuthString4").ToUpper '"WORK BETWEEN "
            optAuthString5 = rm.GetString("optAuthString5").ToUpper '" AND "
            optAuthString6 = rm.GetString("optAuthString6").ToUpper '" ON "
            optAuthString7 = rm.GetString("optAuthString7").ToUpper '" ON "
            optAuthString8 = rm.GetString("optAuthString8").ToUpper
            optAuthString9 = rm.GetString("optAuthString9").ToUpper
            optAuthString10 = rm.GetString("optAuthString10").ToUpper
            optAuthString11 = rm.GetString("optAuthString11").ToUpper
            optAuthString12 = rm.GetString("optAuthString12").ToUpper
            optAuthString13 = rm.GetString("optAuthString13").ToUpper
            optAuthString14 = rm.GetString("optAuthString14").ToUpper
            optAuthString15 = rm.GetString("optAuthString15").ToUpper
            optAuthString16 = rm.GetString("optAuthString16").ToUpper
            optAuthString17 = rm.GetString("optAuthString17").ToUpper
            optAuthString18 = rm.GetString("optAuthString18").ToUpper
            optAuthString19 = rm.GetString("optAuthString19").ToUpper
            optAuthString20 = rm.GetString("optAuthString20").ToUpper
            optAuthString21 = rm.GetString("optAuthString21").ToUpper
            optAuthString22 = rm.GetString("optAuthString22").ToUpper
            optAuthString23 = rm.GetString("optAuthString23").ToUpper
            optAuthString24 = rm.GetString("optAuthString24").ToUpper
            optAuthString25 = rm.GetString("optAuthString25").ToUpper
            optAuthString26 = rm.GetString("optAuthString26").ToUpper
            optAuthString27 = rm.GetString("optAuthString27").ToUpper
            optAuthString28 = rm.GetString("optAuthString28").ToUpper
            optAuthString29 = rm.GetString("optAuthString29").ToUpper
            optAuthString30 = rm.GetString("optAuthString30").ToUpper
            optAuthString31 = rm.GetString("optAuthString31").ToUpper
            optAuthString32 = rm.GetString("optAuthString32").ToUpper
            optAuthString33 = rm.GetString("optAuthString33").ToUpper
            optAuthString34 = rm.GetString("optAuthString34").ToUpper
            optAuthString35 = rm.GetString("optAuthString35").ToUpper
            optAuthString36 = rm.GetString("optAuthString36").ToUpper
            optAuthString37 = rm.GetString("optAuthString37").ToUpper
            optAuthString38 = rm.GetString("optAuthString38").ToUpper
            optAuthString39 = rm.GetString("optAuthString39").ToUpper
            optAuthString40 = "" 'rm.GetString("optAuthString40").ToUpper
            optAuthString41 = rm.GetString("optAuthString41").ToUpper
            optAuthString42 = rm.GetString("optAuthString42").ToUpper
            optAuthString43 = rm.GetString("optAuthString43").ToUpper
            optAuthString44 = rm.GetString("optAuthString44").ToUpper
            optAuthString45 = rm.GetString("optAuthString45").ToUpper
            optAuthString46 = rm.GetString("optAuthString46").ToUpper
            optAuthString47 = rm.GetString("optAuthString47").ToUpper
            optAuthString48 = rm.GetString("optAuthString48").ToUpper
            optAuthString49 = rm.GetString("optAuthString49").ToUpper
            optAuthString50 = rm.GetString("optAuthString50").ToUpper
            optAuthString51 = rm.GetString("optAuthString51").ToUpper
            optAuthString52 = rm.GetString("optAuthString52").ToUpper
            optAuthString53 = rm.GetString("optAuthString53").ToUpper
            optAuthString54 = rm.GetString("optAuthString54").ToUpper
            optAuthString55 = rm.GetString("optAuthString55").ToUpper

            optAuthTrackAlert = rm.GetString("optAuthTrackAlert").ToUpper
            optAuthTrackAlertRouteEstablished = rm.GetString("optAuthTrackAlertRouteEstablished").ToUpper
            optAuthTrackAlertDispatcherResolved = rm.GetString("optAuthTrackAlertDispatcherResolved").ToUpper
            optAuthTrackAlertIsSafe = rm.GetString("optAuthTrackAlertIsSafe").ToUpper

            optAuthBehindTrainAlert = rm.GetString("optAuthBehindTrainAlert").ToUpper
            optAuthBehindTrainMessage = rm.GetString("optAuthBehindTrainMessage").ToUpper

            optAuthJointMoreThan7Message = rm.GetString("optAuthJointMoreThan7Message").ToUpper

            optAuthStrOccupancy = rm.GetString("optAuthStrOccupancy").ToUpper
            optAuthStrHasDispResolved = rm.GetString("optAuthStrHasDispResolved").ToUpper
            optAuthStrTrkAuthAlert = rm.GetString("optAuthStrTrkAuthAlert").ToUpper

            optAuthDeviceBlockString1 = rm.GetString("optAuthDeviceBlockString1").ToUpper

            optAuthConflictString1 = rm.GetString("optAuthConflictString1").ToUpper
            optAuthErrorAdding = rm.GetString("optAuthErrorAdding").ToUpper
            optAuthErrorAddingMsgBox = rm.GetString("optAuthErrorAddingMsgBox").ToUpper

            optTrainPositionString1 = rm.GetString("optTrainPositionString1").ToUpper
            optTrainPositionString2 = rm.GetString("optTrainPositionString2").ToUpper
            optTrainPositionString3 = rm.GetString("optTrainPositionString3").ToUpper
            optTrainPositionString4 = rm.GetString("optTrainPositionString4").ToUpper
            optTrainPositionString5 = rm.GetString("optTrainPositionString5").ToUpper
            optTrainPositionString6 = rm.GetString("optTrainPositionString6").ToUpper
            optTrainPositionString7 = rm.GetString("optTrainPositionString7").ToUpper
            optTrainPositionString8 = rm.GetString("optTrainPositionString8").ToUpper
            optTrainPositionString9 = rm.GetString("optTrainPositionString9").ToUpper
            optTrainPositionString10 = rm.GetString("optTrainPositionString10").ToUpper


            optDirectionWest = rm.GetString("optDirectionWest").ToUpper
            optDirectionEast = rm.GetString("optDirectionEast").ToUpper
            optDirectionNorth = rm.GetString("optDirectionNorth").ToUpper
            optDirectionSouth = rm.GetString("optDirectionSouth").ToUpper
            optDirectionNone = rm.GetString("optDirectionNONE").ToUpper

            optTrainReleasePrompt = rm.GetString("optTrainReleasePrompt").ToUpper
            optTrainReleaseHeader = rm.GetString("optTrainReleaseHeader").ToUpper

            optTrainFinalPrompt1 = rm.GetString("optTrainFinalPrompt1").ToUpper
            optTrainFinalPrompt2 = rm.GetString("optTrainFinalPrompt2").ToUpper

            optTrainFinalHeader = rm.GetString("optTrainFinalHeader").ToUpper

            optMOWReleaseString1 = rm.GetString("optMOWReleaseString1").ToUpper
            optMOWReleaseString2 = rm.GetString("optMOWReleaseString2").ToUpper

            optBlockReleaseString1 = rm.GetString("optBlockReleaseString1").ToUpper
            optBlockReleaseString2 = rm.GetString("optBlockReleaseString2").ToUpper
            optBlockReleaseString3 = rm.GetString("optBlockReleaseString3").ToUpper
            optBlockString1 = rm.GetString("optBlockString1").ToUpper

            optMilePostAbbreviation = rm.GetString("optMilePostAbbreviation").ToUpper

            optMOWIssueToAbbreviation = rm.GetString("optMOWIssueToAbbreviation").ToUpper
            optMOWIDString1 = rm.GetString("optMOWIDString1").ToUpper

            optTagString1 = rm.GetString("optTagString1").ToUpper
            optTagString2 = rm.GetString("optTagString2").ToUpper
            optTagString3 = rm.GetString("optTagString3").ToUpper
            optTagString4 = rm.GetString("optTagString4").ToUpper
            optTagString5 = rm.GetString("optTagString5").ToUpper
            optTagString6 = rm.GetString("optTagString6").ToUpper
            optTagString7 = rm.GetString("optTagString7").ToUpper
            optTagString8 = rm.GetString("optTagString8").ToUpper
            optTagString9 = rm.GetString("optTagString9").ToUpper
            optTagString10 = rm.GetString("optTagString10").ToUpper
            optTagString11 = rm.GetString("optTagString11").ToUpper
            optTagString12 = rm.GetString("optTagString12").ToUpper
            optTagString13 = rm.GetString("optTagString13").ToUpper
            optTagString14 = rm.GetString("optTagString14").ToUpper
            optTagString15 = rm.GetString("optTagString15").ToUpper
            optTagString16 = rm.GetString("optTagString16").ToUpper
            optTagString17 = rm.GetString("optTagString17").ToUpper
            optTagString18 = rm.GetString("optTagString18").ToUpper
            optTagString19 = rm.GetString("optTagString19").ToUpper
            optTagString20 = rm.GetString("optTagString20").ToUpper
            optTagString21 = rm.GetString("optTagString21").ToUpper
            optTagString22 = rm.GetString("optTagString22").ToUpper
            optTagString23 = rm.GetString("optTagString23").ToUpper
            optTagString24 = rm.GetString("optTagString24").ToUpper
            optTagString25 = rm.GetString("optTagString25").ToUpper

            optTagType1 = rm.GetString("optTagType1").ToUpper
            optTagType2 = rm.GetString("optTagType2").ToUpper
            optTagType3 = rm.GetString("optTagType3").ToUpper
            optTagStateTrue = rm.GetString("optTagStateTrue").ToUpper
            optTagStateFalse = rm.GetString("optTagStateFalse").ToUpper
            optMilePostDisplay = rm.GetString("optMilePostDisplay").ToUpper
            optGraphicalCompContextMenuRemoveMOW = rm.GetString("optGraphicalCompContextMenuRemoveMOW").ToUpper
            optGraphicalCompContextMenuPositionMOW = rm.GetString("optGraphicalCompContextMenuPositionMOW").ToUpper
            optGraphicalCompContextMenuMoveMOW = rm.GetString("optGraphicalCompContextMenuMoveMOW").ToUpper

            optXferReportString1 = rm.GetString("optXferReportString1").ToUpper
            optXferReportString2 = rm.GetString("optXferReportString2").ToUpper
            optXferReportString3 = rm.GetString("optXferReportString3").ToUpper
            optXferReportString4 = rm.GetString("optXferReportString4").ToUpper
            optXferReportString5 = rm.GetString("optXferReportString5").ToUpper
            optXferReportString6 = rm.GetString("optXferReportString6").ToUpper
            optXferReportString7 = rm.GetString("optXferReportString7").ToUpper
            optXferReportString8 = rm.GetString("optXferReportString8").ToUpper
            optXferReportString9 = rm.GetString("optXferReportString9").ToUpper
            optXferReportString10 = rm.GetString("optXferReportString10").ToUpper
            optXferReportString11 = rm.GetString("optXferReportString11").ToUpper
            optXferReportString12 = rm.GetString("optXferReportString12").ToUpper
            optXferReportString13 = rm.GetString("optXferReportString13").ToUpper
            optXferReportString14 = rm.GetString("optXferReportString14").ToUpper
            optXferReportString15 = rm.GetString("optXferReportString15").ToUpper
            optXferReportString16 = rm.GetString("optXferReportString16").ToUpper
            optXferReportString17 = rm.GetString("optXferReportString17").ToUpper
            optXferReportString18 = rm.GetString("optXferReportString18").ToUpper
            optXferReportString19 = rm.GetString("optXferReportString19").ToUpper
            optXferReportString20 = rm.GetString("optXferReportString20").ToUpper
            optXferReportString21 = rm.GetString("optXferReportString21").ToUpper
            optXferReportString22 = rm.GetString("optXferReportString22").ToUpper
            optXferReportString23 = rm.GetString("optXferReportString23").ToUpper

            mstrmnuSignalDrillON = rm.GetString("frmDisplay_mstrmnuSignalDrillON")
            mstrmnuSignalDrillOFF = rm.GetString("frmDisplay_mstrmnuSignalDrillOFF")
            mstrmnuSignalCallOn = rm.GetString("frmDisplay_mstrmnuSignalCallOn")
            mstrmnuFleetOn = rm.GetString("frmDisplay_mstrmnuFleetOn")
            mstrmnuFleetOFF = rm.GetString("frmDisplay_mstrmnuFleetOFF")

            optDeviceBlockString1 = rm.GetString("optDeviceBlockString1")
            optDeviceBlockString2 = rm.GetString("optDeviceBlockString2")
            optDeviceBlockString3 = rm.GetString("optDeviceBlockString3")
            optAuthTypeNone = ""

            If optRailRoadName = "FERROMEX" Then
                optRollUpAuthorityString7 = ""
            End If

            optSystemAlertMessage1 = rm.GetString("optSystemAlertMessage1")
            optSystemAlertMessage2 = rm.GetString("optSystemAlertMessage2")
            optSystemAlertMessage3 = rm.GetString("optSystemAlertMessage3")
            optSystemAlertMessage4 = rm.GetString("optSystemAlertMessage4")

            optTrainCancelPrompt = rm.GetString("optTrainCancelPrompt")
            optTrainCancelHeader = rm.GetString("optTrainCancelHeader")

            mstrRelConfHeader = rm.GetString("frmReportClearTrackNTime_mstrRelConfHeader")
            mstrTNT = rm.GetString("frmReportClearTrackNTime_mstrTNT")
            mstrReleaseAT = rm.GetString("frmReportClearTrackNTime_mstReleaseAT")
            mstrISThatCorrect = rm.GetString("frmReportClearTrackNTime_mstrISThatCorrect")
            optWeatherMessage1 = rm.GetString("optWeatherMessage1").ToUpper
            optWeatherMessage2 = rm.GetString("optWeatherMessage2").ToUpper
            optWeatherMessage3 = rm.GetString("optWeatherMessage3").ToUpper
            optWeatherMessage4 = rm.GetString("optWeatherMessage4").ToUpper
            optWeatherMessage5 = rm.GetString("optWeatherMessage5").ToUpper
            optWeatherMessage6 = rm.GetString("optWeatherMessage6").ToUpper
            optWeatherMessage7 = rm.GetString("optWeatherMessage7").ToUpper
            optWeatherMessage8 = rm.GetString("optWeatherMessage8").ToUpper
            optWeatherMessage9 = rm.GetString("optWeatherMessage9").ToUpper
            optWeatherMessage10 = rm.GetString("optWeatherMessage10").ToUpper
            optWeatherMessage11 = rm.GetString("optWeatherMessage11").ToUpper
            optWeatherMessage12 = rm.GetString("optWeatherMessage12").ToUpper
            optWeatherMessage13 = rm.GetString("optWeatherMessage13").ToUpper
            optWeatherMessage14 = rm.GetString("optWeatherMessage14").ToUpper

            'Report Clear Track Authority
            optReportClear_TrackTimeAuthNumber = rm.GetString("optReportClear_TrackTimeAuthNumber").ToUpper
            optReportClear_RptClearOnAuthNumber = rm.GetString("optReportClear_RptClearOnAuthNumber").ToUpper
            optReportClear_IsReleasedFor = rm.GetString("optReportClear_IsReleasedFor").ToUpper
            optReportClear_Movement = rm.GetString("optReportClear_Movement").ToUpper
            optReportClear_At = rm.GetString("optReportClear_At").ToUpper
            optReportClear_IsReleasedAT = rm.GetString("optReportClear_IsReleasedAT").ToUpper
            optReportClear_IsReleasedFor = rm.GetString("optReportClear_IsReleasedFor").ToUpper
            optReportClear_For = rm.GetString("optReportClear_For").ToUpper
            optReportClear_IsThatCorrect = rm.GetString("optReportClear_IsThatCorrect").ToUpper
            optReportClear_RptClearConfirmation = rm.GetString("optReportClear_RptClearConfirmation").ToUpper
            optReportClear_WarningElectronicAuth = rm.GetString("optReportClear_WarningElectronicAuth").ToUpper
            optReportClear_PerformingManualRelease = rm.GetString("optReportClear_PerformingManualRelease").ToUpper
            optReportClear_RptClearPersonDoesNotMatch = rm.GetString("optReportClear_RptClearPersonDoesNotMatch").ToUpper
            optReportClear_IssueToPerson = rm.GetString("optReportClear_IssueToPerson").ToUpper
            optReportClear_RptClearPerson = rm.GetString("optReportClear_RptClearPerson").ToUpper

            optEASTWARD = rm.GetString("frmReleaseMovement_rbEast")
            optWESTWARD = rm.GetString("frmReleaseMovement_rbWest")
            optBoxesMarked = rm.GetString("optBoxesMarked")

            mstrSOLE = rm.GetString("frmSystemSummary_mstrSOLE")
            mstrJoint = rm.GetString("frmSystemSummary_mstrJoint")


            optReleaseDirectionSouth = rm.GetString("optReleaseDirectionSouth").ToUpper
            optReleaseDirectionNorth = rm.GetString("optReleaseDirectionNorth").ToUpper
            optReleaseDirectionEast = rm.GetString("optReleaseDirectionEast").ToUpper
            optReleaseDirectionWest = rm.GetString("optReleaseDirectionWest").ToUpper
            optReleaseDirectionNone = rm.GetString("optReleaseDirectionNone").ToUpper

            optBehindTrain = rm.GetString("optBehindTrain").ToUpper
            optAuthorityJointWith = rm.GetString("optAuthorityJointWith").ToUpper

            'DEVICE
            optDevPOKName = rm.GetString("optDevPOKName").ToUpper
            optDevDOKName = rm.GetString("optDevDOKName").ToUpper

            'TRACK
            optGraphicalCompTrkLinkFailureMsgHeader = rm.GetString("optGraphicalCompTrkLinkFailureMsgHeader").ToUpper
            optGraphicalCompTrkLinkFailureMsg = rm.GetString("optGraphicalCompTrkLinkFailureMsg").ToUpper

            'SYSTEM MESSAGES
            optConflictResolutionMsg = rm.GetString("optConflictResolutionMsg").ToUpper
            optSystemMessage = rm.GetString("optSystemMessage").ToUpper
            optTrkAuthSignalCheckMsg = rm.GetString("optTrkAuthSignalCheckMsg").ToUpper


            optCodeFailureDeviceName = rm.GetString("optCodeFailureDeviceName").ToUpper

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub LoadGlobalPortugueseLanguageReferences()
        Dim rm As ResourceManager
        Try
            Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo(gLocale)
            rm = ResourceManager.CreateFileBasedResourceManager("PortugueseLanguageResource", ".", Nothing)
            optTrainPositionString1 = rm.GetString("optTrainPositionString1").ToUpper
            optTrainPositionString2 = rm.GetString("optTrainPositionString2").ToUpper
            optTrainPositionString3 = rm.GetString("optTrainPositionString3").ToUpper
            optTrainPositionString4 = rm.GetString("optTrainPositionString4").ToUpper
            optTrainPositionString5 = rm.GetString("optTrainPositionString5").ToUpper
            optTrainPositionString6 = rm.GetString("optTrainPositionString6").ToUpper
            optTrainPositionString7 = rm.GetString("optTrainPositionString7").ToUpper
            optTrainPositionString8 = rm.GetString("optTrainPositionString8").ToUpper
            optTrainPositionString9 = rm.GetString("optTrainPositionString9").ToUpper
            optTrainPositionString10 = rm.GetString("optTrainPositionString10").ToUpper
            optTrainPositionString11 = rm.GetString("optTrainPositionString11").ToUpper
            optChangeDirectionString1 = rm.GetString("optChangeDirectionString1").ToUpper
            optChangeDirectionString2 = rm.GetString("optChangeDirectionString2").ToUpper
            optChangeDirectionString3 = rm.GetString("optChangeDirectionString3").ToUpper
            optChangeDirectionString4 = rm.GetString("optChangeDirectionString4").ToUpper
            optChangeDirectionString5 = rm.GetString("optChangeDirectionString5").ToUpper
            optChangeDirectionString6 = rm.GetString("optChangeDirectionString6").ToUpper
            optChangeDirectionString7 = rm.GetString("optChangeDirectionString7").ToUpper
            optChangeDirectionString8 = rm.GetString("optChangeDirectionString8").ToUpper
            optChangeDirectionString9 = rm.GetString("optChangeDirectionString9").ToUpper
            optChangeDirectionString11 = rm.GetString("optChangeDirectionString11").ToUpper
            optChangeDirectionString12 = rm.GetString("optChangeDirectionString12").ToUpper
            optChangeDirectionString13 = rm.GetString("optChangeDirectionString13").ToUpper
            optChangeDirectionString14 = rm.GetString("optChangeDirectionString14").ToUpper

            optChangeDirectionMessageHeader = rm.GetString("optChangeDirectionMessageHeader").ToUpper

            If optRailRoadName.Contains("RUMO") Then
                optDirectionWest = rm.GetString("optDirectionEast").ToUpper
                optDirectionEast = rm.GetString("optDirectionWest").ToUpper
                optDirectionNorth = rm.GetString("optDirectionSouth").ToUpper
                optDirectionSouth = rm.GetString("optDirectionNorth").ToUpper
            Else
                optDirectionWest = rm.GetString("optDirectionWest").ToUpper
                optDirectionEast = rm.GetString("optDirectionEast").ToUpper
                optDirectionNorth = rm.GetString("optDirectionNorth").ToUpper
                optDirectionSouth = rm.GetString("optDirectionSouth").ToUpper
            End If
            optDirectionDescending = rm.GetString("optDirectionDescending").ToUpper
            optDirectionAscending = rm.GetString("optDirectionAscending").ToUpper
            optDirectionNone = rm.GetString("optDirectionNONE").ToUpper
            optTagString22 = rm.GetString("optTagString22").ToUpper
            optTrainPositionString1 = rm.GetString("optTrainPositionString1").ToUpper
            optTrainPositionString2 = rm.GetString("optTrainPositionString2").ToUpper
            optTrainPositionString3 = rm.GetString("optTrainPositionString3").ToUpper
            optTrainPositionString4 = rm.GetString("optTrainPositionString4").ToUpper
            optTrainPositionString5 = rm.GetString("optTrainPositionString5").ToUpper
            optTrainPositionString6 = rm.GetString("optTrainPositionString6").ToUpper
            optTrainPositionString7 = rm.GetString("optTrainPositionString7").ToUpper
            optTrainPositionString8 = rm.GetString("optTrainPositionString8").ToUpper
            optTrainPositionString9 = rm.GetString("optTrainPositionString9").ToUpper
            optTrainPositionString10 = rm.GetString("optTrainPositionString10").ToUpper
            optTrainPositionString11 = rm.GetString("optTrainPositionString11").ToUpper
            optTrainString19 = rm.GetString("optTrainString19").ToUpper
            optTrainLinkString1 = rm.GetString("optTrainLinkString1").ToUpper
            optTrainLinkString2 = rm.GetString("optTrainLinkString2").ToUpper
            optTrainLinkString3 = rm.GetString("optTrainLinkString3").ToUpper
            optLanguageMessageBoxCaptionDefault1 = rm.GetString("OptLanguageMessageBoxCaptionDefault1").ToUpper
            optControlPointManualOperate1 = rm.GetString("optControlPointManualOperate1").ToUpper
            optControlPointManualOperate2 = rm.GetString("optControlPointManualOperate2").ToUpper
            optTrainReleasePrompt = rm.GetString("optTrainReleasePrompt").ToUpper
            optTrainReleaseHeader = rm.GetString("optTrainReleaseHeader").ToUpper
            optTrainFinalPrompt1 = rm.GetString("optTrainFinalPrompt1").ToUpper
            optTrainFinalPrompt2 = rm.GetString("optTrainFinalPrompt2").ToUpper
            optTrainFinalHeader = rm.GetString("optTrainFinalHeader").ToUpper
            optGraphicalCompContextMenuPositionTrain = rm.GetString("optGraphicalCompContextMenuPositionTrain").ToUpper
            optGraphicalCompContextMenuPositionTrainManeuverLoad = rm.GetString("optGraphicalCompContextMenuPositionTrainManeuverLoad").ToUpper
            optGraphicalCompContextMenuPositionTrainManeuverEmpty = rm.GetString("optGraphicalCompContextMenuPositionTrainManeuverEmpty").ToUpper
            optGraphicalCompContextMenuPositionPlannedTrains = rm.GetString("optGraphicalCompContextMenuPositionPlannedTrains").ToUpper
            optGraphicalCompContextMenuPositionUpdatePlannedTrains = rm.GetString("optGraphicalCompContextMenuPositionUpdatePlannedTrains").ToUpper
            optGraphicalCompContextMenuTNTIssue = rm.GetString("optGraphicalCompContextMenuTNTIssue").ToUpper
            optGraphicalCompContextMenuTNTClear = rm.GetString("optGraphicalCompContextMenuTNTClear").ToUpper
            optGraphicalCompContextMenuTWCRollUp = rm.GetString("optGraphicalCompContextMenuTWCRollUp").ToUpper
            optGraphicalCompContextMenuExtendLicenseinSlitDeskCreate = rm.GetString("optGraphicalCompContextMenuExtendLicenseinSlitDeskCreate").ToUpper
            optGraphicalCompContextMenuExtendLicenseinSlitDeskCancel = rm.GetString("optGraphicalCompContextMenuExtendLicenseinSlitDeskCancel").ToUpper
            optGraphicalCompContextMenuExtendLicenseinSlitDeskCancelStoredLicense = rm.GetString("optGraphicalCompContextMenuExtendLicenseinSlitDeskCancelStoredLicense").ToUpper
            optGraphicalCompContextMenuTagCreate = rm.GetString("optGraphicalCompContextMenuTagCreate").ToUpper
            optGraphicalCompContextMenuTagSpeed = rm.GetString("optGraphicalCompContextMenuTagSpeed").ToUpper
            optGraphicalCompContextMenuTagInfo = rm.GetString("optGraphicalCompContextMenuTagInfo").ToUpper
            optGraphicalCompContextMenuTagRemove = rm.GetString("optGraphicalCompContextMenuTagRemove").ToUpper
            optGraphicalCompContextMenuTagModify = rm.GetString("optGraphicalCompContextMenuTagModify").ToUpper
            optGraphicalCompContextMenuTagCopy = rm.GetString("optGraphicalCompContextMenuTagCopy").ToUpper
            optGraphicalCompContextMenuTagTrackAlarm = rm.GetString("optGraphicalCompContextMenuTagTrackAlarm").ToUpper
            optGraphicalCompContextMenuTagWorkZone = rm.GetString("optGraphicalCompContextMenuTagWorkZone").ToUpper
            optGraphicalCompContextMenuBlockCreate = rm.GetString("optGraphicalCompContextMenuBlockCreate").ToUpper
            optGraphicalCompContextMenuBlockRemove = rm.GetString("optGraphicalCompContextMenuBlockRemove").ToUpper
            optGraphicalCompContextMenuBlockOS = rm.GetString("optGraphicalCompContextMenuBlockOS").ToUpper
            optGraphicalCompContextMenuTrackReset = rm.GetString("optGraphicalCompContextMenuTrackReset").ToUpper
            optGraphicalCompContextMenuSwBlockCreate = rm.GetString("optGraphicalCompContextMenuSwBlockCreate").ToUpper
            optGraphicalCompContextMenuSwBlockRemove = rm.GetString("optGraphicalCompContextMenuSwBlockRemove").ToUpper
            optGraphicalCompContextMenuSwBlockOS = rm.GetString("optGraphicalCompContextMenuSwBlockOS").ToUpper
            optGraphicalCompContextMenu_mnuRECALL = rm.GetString("optGraphicalCompContextMenu_mnuRECALL").ToUpper
            optGraphicalCompContextMenu_mnuRESEND = rm.GetString("optGraphicalCompContextMenu_mnuRESEND").ToUpper
            optGraphicalCompContextMenu_mnuRoutes = rm.GetString("optGraphicalCompContextMenu_mnuRoutes").ToUpper
            optGraphicalCompContextMenu_mnuReleaseControl = rm.GetString("optGraphicalCompContextMenu_mnuReleaseControl").ToUpper
            optGraphicalCompContextMenu_mnuMCZ = rm.GetString("optGraphicalCompContextMenu_mnuMCZ").ToUpper
            mstrmnuFleetOn = rm.GetString("optGraphicalCompContextMenu_mstrmnuFleetOn")
            mstrmnuFleetOFF = rm.GetString("optGraphicalCompContextMenu_mstrmnuFleetOFF")
            optGraphicalCompProtectSwitch1 = rm.GetString("optGraphicalCompProtectSwitch1").ToUpper
            optGraphicalCompProtectSwitch2 = rm.GetString("optGraphicalCompProtectSwitch2").ToUpper
            optGraphicalCompProtectSwitch3 = rm.GetString("optGraphicalCompProtectSwitch3").ToUpper
            optGraphicalCompProtectSwitch4 = rm.GetString("optGraphicalCompProtectSwitch4").ToUpper
            'TOOL TIP ON TRACK
            optTagType1 = rm.GetString("optTagType1").ToUpper
            optTagType2 = rm.GetString("optTagType2").ToUpper
            optTagType3 = rm.GetString("optTagType3").ToUpper
            optTagType6 = rm.GetString("optTagType6").ToUpper
            optTagType7 = rm.GetString("optTagType7").ToUpper
            optBlockReleaseString1 = rm.GetString("optBlockReleaseString1").ToUpper
            optBlockReleaseString2 = rm.GetString("optBlockReleaseString2").ToUpper
            optBlockReleaseString3 = rm.GetString("optBlockReleaseString3").ToUpper
            optBlockReleaseString4 = rm.GetString("optBlockReleaseString4").ToUpper
            optBlockString1 = rm.GetString("optBlockString1").ToUpper
            optMilePostAbbreviation = rm.GetString("optMilePostAbbreviation").ToUpper
            optMilePostAbbreviation1 = rm.GetString("optMilePostAbbreviation1").ToUpper
            optTrackNameSidingDefault = rm.GetString("optTrackNameSidingDefault").ToUpper
            optTrackLengthLeftInSidingString = rm.GetString("optTrackLengthLeftInSidingString").ToUpper
            optMilePostDisplay = rm.GetString("optMilePostDisplay").ToUpper
            optSystemAlertMessage1 = rm.GetString("optSystemAlertMessage1")
            optSystemAlertMessage2 = rm.GetString("optSystemAlertMessage2")
            optSystemAlertMessage3 = rm.GetString("optSystemAlertMessage3")
            optSystemAlertMessage4 = rm.GetString("optSystemAlertMessage4")
            optSystemAlertMessage7 = rm.GetString("optSystemAlertMessage7")
            optSystemAlertMessage8 = rm.GetString("optSystemAlertMessage8")
            optSystemAlertMessage9 = rm.GetString("optSystemAlertMessage9")
            optSystemAlertMessage10 = rm.GetString("optSystemAlertMessage10")
            optSystemAlertMessage11 = rm.GetString("optSystemAlertMessage11")
            optSystemAlertMessage12 = rm.GetString("optSystemAlertMessage12")
            optSystemAlertMessage13 = rm.GetString("optSystemAlertMessage13")
            optSystemAlertMessage14 = rm.GetString("optSystemAlertMessage14")
            optSystemAlertMessage15 = rm.GetString("optSystemAlertMessage15")
            optSystemAlertMessage16 = rm.GetString("optSystemAlertMessage16")
            optSystemAlertMessage17 = rm.GetString("optSystemAlertMessage17")
            optSystemAlertMessage18 = rm.GetString("optSystemAlertMessage18")
            optTagString1 = rm.GetString("optTagString1").ToUpper
            optTagString2 = rm.GetString("optTagString2").ToUpper
            optTagString3 = rm.GetString("optTagString3").ToUpper
            optTagString5 = rm.GetString("optTagString5").ToUpper
            optTagString6 = rm.GetString("optTagString6").ToUpper
            optTagString7 = rm.GetString("optTagString7").ToUpper
            optTagString8 = rm.GetString("optTagString8").ToUpper
            optTagString9 = rm.GetString("optTagString9").ToUpper
            optTagString10 = rm.GetString("optTagString10").ToUpper
            optTagString11 = rm.GetString("optTagString11").ToUpper
            optTagString12 = rm.GetString("optTagString12").ToUpper
            optTagString13 = rm.GetString("optTagString13").ToUpper
            optTagString14 = rm.GetString("optTagString14").ToUpper
            optTagString15 = rm.GetString("optTagString15").ToUpper
            optTagString16 = rm.GetString("optTagString16").ToUpper
            optTagString19 = rm.GetString("optTagString19").ToUpper
            optTagString20 = rm.GetString("optTagString20").ToUpper
            optTagString26 = rm.GetString("optTagString26").ToUpper
            optTagString27 = rm.GetString("optTagString27").ToUpper
            optTagString28 = rm.GetString("optTagString28").ToUpper
            optTagStateTrue = rm.GetString("optTagStateTrue").ToUpper
            optTagStateFalse = rm.GetString("optTagStateFalse").ToUpper
            optAuthString24 = rm.GetString("optAuthString24").ToUpper
            optAuthString25 = rm.GetString("optAuthString25").ToUpper
            optAuthString56 = rm.GetString("optAuthString56").ToUpper
            optAuthString57 = rm.GetString("optAuthString57").ToUpper
            optAuthString62 = rm.GetString("optAuthString62").ToUpper
            optAuthString63 = rm.GetString("optAuthString63").ToUpper
            optAuthString65 = rm.GetString("optAuthString65").ToUpper
            optAuthString66 = rm.GetString("optAuthString66").ToUpper
            optAuthString67 = rm.GetString("optAuthString67").ToUpper
            optAuthString68 = rm.GetString("optAuthString68").ToUpper
            optAuthString69 = rm.GetString("optAuthString69").ToUpper
            optAuthString70 = rm.GetString("optAuthString70").ToUpper
            optAuthString71 = rm.GetString("optAuthString71").ToUpper
            optAuthString72 = rm.GetString("optAuthString72").ToUpper
            optAuthString73 = rm.GetString("optAuthString73").ToUpper
            optAuthString74 = rm.GetString("optAuthString74").ToUpper
            optAuthString75 = rm.GetString("optAuthString75").ToUpper
            optAuthString76 = rm.GetString("optAuthString76").ToUpper
            optAuthString77 = rm.GetString("optAuthString77").ToUpper
            optAuthString78 = rm.GetString("optAuthString78").ToUpper
            optAuthString79 = rm.GetString("optAuthString79").ToUpper
            optAuthString80 = rm.GetString("optAuthString80").ToUpper
            optAuthString81 = rm.GetString("optAuthString81").ToUpper
            optAuthString82 = rm.GetString("optAuthString82").ToUpper
            optAuthString83 = rm.GetString("optAuthString83").ToUpper
            optAuthString84 = rm.GetString("optAuthString84").ToUpper
            optAuthString85 = rm.GetString("optAuthString85").ToUpper
            'PSS
            optAuthString58 = rm.GetString("optAuthString58").ToUpper
            optAuthString59 = rm.GetString("optAuthString59").ToUpper
            optAuthString60 = rm.GetString("optAuthString60").ToUpper
            optAuthString65 = rm.GetString("optAuthString65").ToUpper
            optAuthString66 = rm.GetString("optAuthString66").ToUpper
            optAuthString67 = rm.GetString("optAuthString67").ToUpper
            optAuthString68 = rm.GetString("optAuthString68").ToUpper
            optAuthString69 = rm.GetString("optAuthString69").ToUpper
            optGraphicalCompContextMenuTWCVOID = rm.GetString("optGraphicalCompContextMenuTWCVOID").ToUpper
            optSystemSummaryName = rm.GetString("optSystemSummaryName").ToUpper
            optLanguageDisplayFormName = rm.GetString("optLanguageDisplayFormName").ToUpper & Space(1)
            optSystemMessageDbBlkTimerString1 = rm.GetString("optSystemMessageDbBlkTimerString1").ToUpper
            optSystemMessageDbBlkTimerString2 = rm.GetString("optSystemMessageDbBlkTimerString2").ToUpper
            optSystemMessageTAGTimerString1 = rm.GetString("optSystemMessageTAGTimerString1").ToUpper
            optSystemMessageTAGTimerString2 = rm.GetString("optSystemMessageTAGTimerString2").ToUpper
            mstrfrmDeviceBlock_EnterComments = rm.GetString("frmDeviceBlock_EnterComments").Trim.ToUpper
            mstrfrmDeviceBlock_CommentsForTrainProtect = rm.GetString("frmDeviceBlock_CommentsForTrainProtect").Trim.ToUpper
            optDeviceBlockString1 = rm.GetString("optDeviceBlockString1")
            optDeviceBlockString2 = rm.GetString("optDeviceBlockString2")
            optDeviceBlockString3 = rm.GetString("optDeviceBlockString3")
            optSystemMessageString9 = rm.GetString("optSystemMessageString9").ToUpper
            optSystemMessageString10 = rm.GetString("optSystemMessageString10").ToUpper
            optSystemMessageString11 = rm.GetString("optSystemMessageString11").ToUpper
            optSystemMessageString12 = rm.GetString("optSystemMessageString12").ToUpper
            optSystemMessageString13 = rm.GetString("optSystemMessageString13").ToUpper
            optSystemMessageString14 = rm.GetString("optSystemMessageString14").ToUpper
            optSystemMessageString15 = rm.GetString("optSystemMessageString15").ToUpper
            optSystemMessageString16 = rm.GetString("optSystemMessageString16").ToUpper
            optSystemMessageString19 = rm.GetString("optSystemMessageString19").ToUpper
            optSystemMessageString20 = rm.GetString("optSystemMessageString20").ToUpper
            optSystemMessageString21 = rm.GetString("optSystemMessageString21").ToUpper
            optRollUpAuthorityString12 = rm.GetString("optRollUpAuthorityString12").ToUpper
            mstrmnuRequestAuthorization = rm.GetString("mstrmnuRequestAuthorization").ToUpper
            mstrmnuCancelRequestAuthorization = rm.GetString("mstrmnuCancelRequestAuthorization").ToUpper
            'JOIN AUTHORIZATION
            optSignalAuth01 = rm.GetString("optSignalAuth01").ToUpper
            optSignalAuth02 = rm.GetString("optSignalAuth02").ToUpper
            optSignalAuth03 = rm.GetString("optSignalAuth03").ToUpper
            optSignalAuth04 = rm.GetString("optSignalAuth04").ToUpper
            optSignalAuth05 = rm.GetString("optSignalAuth05").ToUpper
            optSignalAuth06 = rm.GetString("optSignalAuth06").ToUpper
            optSignalAuth07 = rm.GetString("optSignalAuth07").ToUpper
            optSignalAuth08 = rm.GetString("optSignalAuth08").ToUpper
            optSignalAuth09 = rm.GetString("optSignalAuth09").ToUpper
            optSignalAuth10 = rm.GetString("optSignalAuth10").ToUpper
            optSignalAuth11 = rm.GetString("optSignalAuth11").ToUpper
            optSignalAuth12 = rm.GetString("optSignalAuth12").ToUpper
            optSignalAuth13 = rm.GetString("optSignalAuth13").ToUpper
            optSignalAuth14 = rm.GetString("optSignalAuth14").ToUpper
            optSignalAuth15 = rm.GetString("optSignalAuth15").ToUpper
            optSignalAuth16 = rm.GetString("optSignalAuth16").ToUpper
            optSignalAuth17 = rm.GetString("optSignalAuth17").ToUpper
            optSignalAuth18 = rm.GetString("optSignalAuth18").ToUpper
            optSignalAuth19 = rm.GetString("optSignalAuth19").ToUpper
            optSignalAuth20 = rm.GetString("optSignalAuth20").ToUpper
            optSignalAuth21 = rm.GetString("optSignalAuth21").ToUpper
            optSignalAuth22 = rm.GetString("optSignalAuth22").ToUpper
            optSignalAuth23 = rm.GetString("optSignalAuth23").ToUpper
            optSignalAuth24 = rm.GetString("optSignalAuth24").ToUpper
            optSignalAuth25 = rm.GetString("optSignalAuth25").ToUpper
            optSignalAuth26 = rm.GetString("optSignalAuth26").ToUpper
            optSignalAuth27 = rm.GetString("optSignalAuth27").ToUpper
            optSignalAuth28 = rm.GetString("optSignalAuth28").ToUpper
            optSignalAuth29 = rm.GetString("optSignalAuth29").ToUpper
            optSignalAuth30 = rm.GetString("optSignalAuth30").ToUpper
            optSignalAuth31 = rm.GetString("optSignalAuth31").ToUpper
            optSignalAuth32 = rm.GetString("optSignalAuth32").ToUpper
            optSignalAuth33 = rm.GetString("optSignalAuth33").ToUpper
            optSignalAuth34 = rm.GetString("optSignalAuth34").ToUpper
            optSignalAuth35 = rm.GetString("optSignalAuth35").ToUpper
            optSignalAuth36 = rm.GetString("optSignalAuth36").ToUpper
            optSignalAuth37 = rm.GetString("optSignalAuth37").ToUpper
            optSignalAuth38 = rm.GetString("optSignalAuth38").ToUpper
            optSignalAuth39 = rm.GetString("optSignalAuth39").ToUpper
            optSignalAuth40 = rm.GetString("optSignalAuth40").ToUpper
            optTrainReleaseLicenseExitsPrompt = rm.GetString("optTrainReleaseLicenseExitsPrompt").ToUpper
            optTrainReleaseLicenseExitsPromptConfirm = rm.GetString("optTrainReleaseLicenseExitsPromptConfirm").ToUpper
            'COMMON WORDS
            optCommonWordNone = rm.GetString("optCommonWordNone").ToUpper
            optCommonWordTO = rm.GetString("optCommonWordTO").ToUpper
            optTrainString21 = optCommonWordNone
            mstrmnuSignalDrillON = rm.GetString("frmDisplay_mstrmnuSignalDrillON").ToUpper
            mstrmnuSignalDrillOFF = rm.GetString("frmDisplay_mstrmnuSignalDrillOFF").ToUpper
            mstrmnuSignalCallOn = rm.GetString("frmDisplay_mstrmnuSignalCallOn").ToUpper
            mstrmnuFleetOn = rm.GetString("frmDisplay_mstrmnuFleetOn").ToUpper
            mstrmnuFleetOFF = rm.GetString("frmDisplay_mstrmnuFleetOFF").ToUpper
            mstrmnuSignalClear = rm.GetString("frmDisplay_mnuSignalClear").ToUpper
            'SYSTEM MESSAGES
            optDerailmentDetectorString1 = rm.GetString("optDerailmentDetectorString1").ToUpper
            optDerailmentDetectorString2 = rm.GetString("optDerailmentDetectorString2").ToUpper
            optDerailmentDetectorString3 = rm.GetString("optDerailmentDetectorString3").ToUpper
            optDerailmentDetectorString4 = rm.GetString("optDerailmentDetectorString4").ToUpper
            optSignalActivityInLimitsContinueMessage = rm.GetString("optSignalActivityInLimitsContinueMessage").ToUpper
            optOccupationInvalidSelectionMessage = rm.GetString("optOccupationInvalidSelectionMessage").ToUpper
            optConflictResolutionMsg = rm.GetString("optConflictResolutionMsg").ToUpper
            optPssMessageTWELine1 = rm.GetString("optPssMessageTWELine1").ToUpper
            optPssMessageTWELine2 = rm.GetString("optPssMessageTWELine2").ToUpper
            optConflictAuthFound = rm.GetString("optConflictAuthFound").ToUpper
            optConflictPSS = rm.GetString("optConflictPSS").ToUpper
            optConflictEnterMainTrack = rm.GetString("optConflictEnterMainTrack").ToUpper
            optConflictSignalActivity = rm.GetString("optConflictSignalActivity").ToUpper
            optConflictTrackWarrant = rm.GetString("optConflictTrackWarrant").ToUpper
            optConflictNotValid = rm.GetString("optConflictNotValid").ToUpper
            optReportClear_RptClearConfirmation = rm.GetString("optReportClear_RptClearConfirmation").ToUpper
            optReleaseLicense = rm.GetString("optReleaseLicense").ToUpper
            'RESET GEO FENCE
            optResetGeoFenceHeader = rm.GetString("optResetGeoFenceHeader").ToUpper
            optResetGeoFenceLine1 = rm.GetString("optResetGeoFenceLine1").ToUpper
            optResetGeoFenceLine2 = rm.GetString("optResetGeoFenceLine2").ToUpper
            optResetGeoFenceLine3 = rm.GetString("optResetGeoFenceLine3").ToUpper
            'STOP TRAIN
            optStopTrainHeader = rm.GetString("optStopTrainHeader").ToUpper
            optStopTrainLine1 = rm.GetString("optStopTrainLine1").ToUpper
            optStopTrainLine2 = rm.GetString("optStopTrainLine2").ToUpper
            optStopTrainLine3 = rm.GetString("optStopTrainLine3").ToUpper
            'DISABLE GEOFENCE
            optDisableGeoFenceHeader = rm.GetString("optDisableGeoFenceHeader").ToUpper
            optDisableGeoFenceLine1 = rm.GetString("optDisableGeoFenceLine1").ToUpper
            optDisableGeoFenceLine2 = rm.GetString("optDisableGeoFenceLine2").ToUpper
            optDisableGeoFenceLine3 = rm.GetString("optDisableGeoFenceLine3").ToUpper
            'DISABLE CBTC
            optDisableCBTCHeader = rm.GetString("optDisableCBTCHeader").ToUpper
            optDisableCBTCLine1 = rm.GetString("optDisableCBTCLine1").ToUpper
            optDisableCBTCLine2 = rm.GetString("optDisableCBTCLine2").ToUpper
            optDisableCBTCLine3 = rm.GetString("optDisableCBTCLine3").ToUpper
            'CANCEL LICENCE
            optCancelLicenseHeader = rm.GetString("optCancelLicenseHeader").ToUpper
            optCancelLicenseLine1 = rm.GetString("optCancelLicenseLine1").ToUpper
            optCancelLicenseLine2 = rm.GetString("optCancelLicenseLine2").ToUpper
            'CUTOUT DIALOG BOX
            optPTCString004 = rm.GetString("optPTCString004").ToUpper
            optPTCString005 = rm.GetString("optPTCString005").ToUpper
            'PSS
            mstrPSSCancelTitle = rm.GetString("mstrPSSCancelTitle").ToUpper
            mstrPSSCancelLine1 = rm.GetString("mstrPSSCancelLine1").ToUpper
            mstrPSSCancelLine2 = rm.GetString("mstrPSSCancelLine2").ToUpper
            mstrPSSChoiceSwitch = rm.GetString("mstrPSSChoiceSwitch").ToUpper
            optPSSDeviceBlockTitle = rm.GetString("optPSSDeviceBlockTitle").ToUpper
            optPSSDeviceWarningBegin = rm.GetString("optPSSDeviceWarningBegin").ToUpper
            optPSSDeviceWarningLimits = rm.GetString("optPSSDeviceWarningLimits").ToUpper
            optPSSDeviceWarningContinue = rm.GetString("optPSSDeviceWarningContinue").ToUpper
            'FSAUTHORITY
            optRollUpAuthorityString29 = rm.GetString("optRollUpAuthorityString29").ToUpper
            optRollUpAuthorityString30 = rm.GetString("optRollUpAuthorityString30").ToUpper
            optRollUpAuthorityString31 = rm.GetString("optRollUpAuthorityString31").ToUpper
            optRollUpAuthorityString32 = rm.GetString("optRollUpAuthorityString32").ToUpper
            'INTERJOURNEY
            optInterjourneySendError1 = rm.GetString("optInterjourneySendError1").ToUpper
            optInterjourneySendError2 = rm.GetString("optInterjourneySendError2").ToUpper
            optInterjourneySendError3 = rm.GetString("optInterjourneySendError3").ToUpper
            optInterjourneyMessage1 = rm.GetString("optInterjourneyMessage1").ToUpper
            Try
                'PTC/CBTC
                optPTCString001 = rm.GetString("optPTCString001").ToUpper
                optPTCString002 = rm.GetString("optPTCString002").ToUpper
                optPTCString003 = rm.GetString("optPTCString003").ToUpper
                optPTCString004 = rm.GetString("optPTCString004").ToUpper
                optPTCString006 = rm.GetString("optPTCString006").ToUpper
                optPTCString008 = rm.GetString("optPTCString008").ToUpper
                optPTCString009 = rm.GetString("optPTCString009").ToUpper
                optPTCString010 = rm.GetString("optPTCString010").ToUpper
                optPTCString011 = rm.GetString("optPTCString011").ToUpper
                optPTCString012 = rm.GetString("optPTCString012").ToUpper
                optPTCString013 = rm.GetString("optPTCString013").ToUpper
                optPTCString014 = rm.GetString("optPTCString014").ToUpper
                optPTCString015 = rm.GetString("optPTCString015").ToUpper
                optPTCString016 = rm.GetString("optPTCString016").ToUpper
                optPTCString017 = rm.GetString("optPTCString017").ToUpper
                optPTCString018 = rm.GetString("optPTCString018").ToUpper
                optPTCString019 = rm.GetString("optPTCString019").ToUpper
                optPTCString020 = rm.GetString("optPTCString020").ToUpper
                optPTCString021 = rm.GetString("optPTCString021").ToUpper
                optPTCString022 = rm.GetString("optPTCString022").ToUpper
                optPTCString023 = rm.GetString("optPTCString023").ToUpper
                optPTCString024 = rm.GetString("optPTCString024").ToUpper
                optPTCString025 = rm.GetString("optPTCString025").ToUpper
                optPTCString029 = rm.GetString("optPTCString029").ToUpper
                optPTCString030 = rm.GetString("optPTCString030").ToUpper
                optPTCString031 = rm.GetString("optPTCString031").ToUpper
                optPTCString032 = rm.GetString("optPTCString032").ToUpper
                optPTCDefaultRestrictionType = rm.GetString("optPTCDefaultRestrictionType").ToUpper
                optSystemMessageString1 = rm.GetString("optSystemMessageString1")
                optSystemMessageString2 = rm.GetString("optSystemMessageString2")
                optSystemMessageString3 = rm.GetString("optSystemMessageString3")
                optSystemMessageString6 = rm.GetString("optSystemMessageString6")
                optTrainAlertString01 = rm.GetString("optTrainAlertString01")
                optTrainAlertString02 = rm.GetString("optTrainAlertString02")
                optTrainAlertString03 = rm.GetString("optTrainAlertString03")
                optTrainAlertString04 = rm.GetString("optTrainAlertString04")
                optTrainAlertString05 = rm.GetString("optTrainAlertString05").ToUpper
                optTrainAlertString06 = rm.GetString("optTrainAlertString06")
                strLicense_TWCExistsForAnotherTrain = rm.GetString("strLicense_TWCExistsForAnotherTrain").ToUpper
                mstrLeadUnitCheck1 = rm.GetString("frmDisplay_mstrLeadUnitCheck1")
                mstrLeadUnitCheck2 = rm.GetString("frmDisplay_mstrLeadUnitCheck2")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Sub ReinitFlipDirection()
        Try
            Dim trk As Track
            For Each trk In TrackCollection
                trk.FlipMe = False
            Next

            Dim sw As Switch
            For Each sw In SwitchCollection
                sw.FlipMe = False
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

#End Region

#Region "FIND TYPE ROUTINES"

    Public Function FindComponentType(ByVal UniqueID As Int32) As String
        Dim I As Integer
        Try
            'Search through the tracks
            For I = 1 To UBound(TrackComponent)
                If TrackComponent(I).GUID = UniqueID Then
                    Return TrackComponent(I).ComponentType
                End If
            Next
            'Search through the signals
            'Search through the switches
            For I = 1 To UBound(SwitchComponent)
                If SwitchComponent(I).GUID = UniqueID Then
                    Return SwitchComponent(I).ComponentType
                End If
            Next
            'Search through the switches
            For I = 1 To UBound(SignalComponent)
                If SignalComponent(I).GUID = UniqueID Then
                    Return SignalComponent(I).ComponentType
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function FindCompFromGUID(ByVal UniqueID As Int32) As Object
        'This function finds the current trackcomponent index from the global unique ID
        Try
            Dim comp As Object
            For Each comp In TMDS.GlobalDeclarations.gComponentsCollection
                If comp.GUID > 0 AndAlso comp.GUID = UniqueID Then
                    Return comp
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function FindCompIndexFromGUID(ByVal UniqueID As Int32) As Int32
        'This function finds the current trackcomponent index from the global unique ID
        Try
            Dim comp As Object
            For Each comp In TMDS.GlobalDeclarations.gComponentsCollection
                If comp.GUID = UniqueID Then
                    Return comp.GUID
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Public Function FindTrackIndexFromGUID(ByVal UniqueID As Int32) As Integer
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        Try
            For I = 1 To UBound(TrackComponent)
                If TrackComponent(I).GUID = UniqueID Then
                    FindTrackIndexFromGUID = I
                    Exit Function
                End If
            Next I
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Public Function FindTrackFromIndex(ByVal Index As Int32) As Track
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        Try
            For I = 1 To UBound(TrackComponent)
                If I = Index Then
                    Return TrackComponent(I)
                End If
            Next I
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function

    Public Function FindTrackFromGUID(ByVal UniqueID As Int32) As Track
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        Try
            For I = 1 To UBound(TrackComponent)
                If TrackComponent(I).GUID = UniqueID Then
                    Return TrackComponent(I)
                End If
            Next I
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function FindSwitchIndexFromGUID(ByVal UniqueID As Int32) As Integer
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        Try
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

    Public Function FindSwitchFromName(ByVal Name As String) As Switch
        'This function finds the Switch Component by Name
        Dim sw As Switch = Nothing
        Try
            For Each sw In SwitchComponent
                If Not sw Is Nothing Then
                    If sw.Name.Trim.ToUpper = Name.Trim.ToUpper Then
                        Return sw
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        Finally
            sw = Nothing
        End Try
        Return Nothing
    End Function


    Public Function FindSignalIndexFromGUID(ByVal UniqueID As Int32) As Integer
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        Try
            For I = 1 To UBound(SignalComponent)
                If SignalComponent(I).GUID = UniqueID Then
                    FindSignalIndexFromGUID = I
                    Exit Function
                End If
            Next I
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Public Function FindSignalFromGUID(ByVal UniqueID As Int32) As Signal
        Dim I As Integer
        Try
            For I = 1 To UBound(SignalComponent)
                If SignalComponent(I).GUID = UniqueID Then
                    Return SignalComponent(I)
                End If
            Next I
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function FindPSSESTAuthority(ByVal DBlkGUID As String) As Object
        'This function finds the PSS/EST authority associating it with Deviceblock GUID.
        Dim comp As Object
        Try

            For Each comp In TMDS.GlobalDeclarations.gOtherSpecialAuthorityCollection
                If comp.GetType.ToString = gOtherSplAuthorityName Then
                    If comp.DBlkGUID = DBlkGUID Then
                        Return comp
                    End If
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        Finally
            comp = Nothing
        End Try
    End Function

    Public Function FindDeviceBlockByPSS_EST(ByVal DBlkGUID As String) As Object
        'This function finds the Device Block associated with the Authority.
        Dim comp As Object
        Try
            For Each comp In TMDS.GlobalDeclarations.gDeviceBlocks
                If comp.BlockGUID = DBlkGUID Then
                    Return comp
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        Finally
            comp = Nothing
        End Try
    End Function

    ''' <summary>
    ''' Find Device Block By Authority Uid
    ''' </summary>
    ''' <param name="authorityUid">Authority Uid</param>
    ''' <returns>Return device block object</returns>
    Public Function FindDeviceBlockByAuthorityUid(authorityUid As String) As TrackAuthority.DeviceBlock
        Try
            For Each oneDeviceBlock As TrackAuthority.DeviceBlock In gDeviceBlocks.OfType(Of TrackAuthority.DeviceBlock)
                If oneDeviceBlock.AuthorityUID.Equals(authorityUid) Then Return oneDeviceBlock
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Find Train By Uid
    ''' </summary>
    ''' <param name="trainUid"></param>
    ''' <returns></returns>
    Public Function FindTrainByUid(trainUid As String) As TrainID
        Try
            For Each oneTrain As TrainID In TrainCollection
                If oneTrain.TrainGUID.Equals(trainUid) Then
                    Return oneTrain
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function FindTrainBySymbol(ByVal Symbol As String) As TrainID
        Try
            Dim trnID As TrainID
            For Each trnID In TrainCollection
                If trnID.Name = Symbol Then
                    Return trnID
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function FindTrainEngineID(ByVal EngineID As String) As TrainID
        Try
            Dim trnID As TrainID
            For Each trnID In TrainCollection
                If trnID.IdentifingUnit.Trim.ToUpper = EngineID.Trim.ToUpper Then
                    Return trnID
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function FindForeman(ByVal Name As String) As Foreman
        Dim fmn As Foreman = Nothing
        Try
            For Each fmn In gMowForemen
                If fmn.Name.Trim.ToUpper = Name.Trim.ToUpper Then
                    Return fmn
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            fmn = Nothing
        End Try
        Return Nothing
    End Function

#End Region

#Region "RAILROAD SPECIFIC SETTINGS"

    Public Sub ExecuteRailRoadSettings(ByVal pRailRoadName As String)
        Try
            If pRailRoadName Is Nothing Then Exit Sub
            Select Case pRailRoadName.ToUpper
                Case "MRS"
                    optRailRoadEnumName = GlobalDeclarations.RailRoadName.MRS
                    ApplyOptionsForMRS()
                Case "RUMO"
                    optRailRoadEnumName = GlobalDeclarations.RailRoadName.RUMO
                    ApplyOptionsForRUMO()
            End Select
            If optEnableTNTCheckSum Then LoadTNTCheckSumVariableArray()
            If optEnableValidationMandatoryDirectivesCheckSumOnLoad Then
                optEnableValidationSFACheckSumOnLoad = True
                optEnableValidationTNTCheckSumOnLoad = True
                optEnableValidationSPLCheckSumOnLoad = True
                optEnableValidationDeviceBLKCheckSumOnLoad = True
                optEnableValidationTGBOCheckSumOnLoad = True
                optEnableValidationTrackBulletinCheckSumOnLoad = True
                optEnableValidationTAGCheckSumOnLoad = True
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub LoadSoftwareOptionFromDB()
        'ScheduleDelayTolerance
        Dim dbComm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim SQLQuery As String = "SELECT * FROM tblSystemSoftwareOptions"
        Dim OptionName As String = ""
        Dim OptionValue As String = ""
        Dim OptionIntValue As Integer = 0



        Try
            Using DbConn As New OleDbConnection(gConnectionStringStatic)
                DbConn.Open()
                dbComm = New OleDbCommand(SQLQuery, DbConn)
                dr = dbComm.ExecuteReader
                While dr.Read
                    OptionName = ""
                    OptionValue = ""
                    OptionIntValue = 0
                    If Not dr.IsDBNull(1) Then OptionName = dr.GetString(1)
                    If Not dr.IsDBNull(2) Then OptionValue = dr.GetString(2)
                    If Not String.IsNullOrEmpty(OptionName) AndAlso Not String.IsNullOrEmpty(OptionValue) Then
                        Select Case OptionName.ToUpper.Trim
                            Case "SCHEDULEDELAYTOLERANCE"
                                Integer.TryParse(OptionValue, OptionIntValue)
                                If OptionIntValue > 0 Then
                                    gDelayTolerance = OptionIntValue
                                End If
                            Case "ENABLECOPIEDBYONREPEAT"
                                Integer.TryParse(OptionValue, OptionIntValue)
                                If OptionIntValue = 1 Then
                                    OptEnableCopiedByOnRepeat = True
                                End If
                        End Select
                    End If
                End While
                dr.Close()
                DbConn.Close()
            End Using
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            If dbComm IsNot Nothing Then dbComm.Dispose()
            dbComm = Nothing
            If dr IsNot Nothing Then dr = Nothing
        End Try
    End Sub

    Private Sub ApplyOptionsForMRS()
        Try
            'Alerts
            optEnableInsertAllAlertsIntoDB = True
            optenableDirectionChangeBasedOnTracking = True
            optEnableSuppressFailureModePromptsForPTC = True
            optEnableAllowSwitchAlignmentWithPassStopSignalInCodeFailure = True
            optEnableAllowSwitchControlMovementWhileInCodeFailure = True
            optEnableShowStatusUnderNonTransverableTrack = True
            optDisableDateRangeFunctionForAuthorityNumber = True
            OptEnableSignalClearByMenu = True
            optAllowQuickRouteWithSignalClearbyMenu = True
            optOverrideRemoteCheckForJointAuthorization = True
            optEnableTestTrainForPTC = True
            optEnableAutomaticPositionReportRequesting = True
            optEnableAutomaticCutoutFromActiveToDisengagedState = True
            optLocomotiveReportKeepAliveIntervalCheck = 120
            optSystemDateAndTimeString = "dd/MM/yyyy HH:mm"
            optSystemDateTimeString = "dd/MM/yyyy"
            optEnableDeviceBlkExpirationCheck = True
            optUseDeviceBlockTimeExpried = True
            optEnableLostOccupancyAlert = True
            optUseResourceCultureFile = True
            optEnableAutoGenerateClearance = True
            optGetTrainUnitsFromLocomotiveTable = True
            optEnableAxlesBrakesCounts = True
            optEnableTagModifyMenu = False
            gEnablePTCVETMSTrackingOnCTC = True
            '*** CODE BROWN OPTIONS
            optEnableToPassRedSignalOnCodeBrown = True
            optEnableTrainMenuCancelStoredRoutes = True
            optAllowTrafficChangeOnTrainDirectionChange = True
            optEnableOverrideSIGValidationForPRS = True
            ' optEnableApplyDeviceBlockFirstPRS_EMT = True
            optEnableOverrideDeviceBlockForPRS = True
            optEnableOverrideJointTNTForPRS = True
            optEnableOverrideTNTForPRS = True
            optTrainIDColorChangeForCBTCEnabled = True
            optUseResourceCultureFile = True

            optDisableFlashingTrackForPTC = True
            gUseTrackAuthorityTypeInPlaceOfTrackAndTime = True
            optEnableMRSSpecificOptions = True
            optEnablePTCBOSVETMSMode = True
            optEnablePTCOperations = True
            optEnableDisallowDuplicateEngineIDsToTrains = True
            optEnableClearSignalWithSignalIntime = False
            optEnableTrainStopOnPowerOffManualOperateCheck = True
            'optEnableTrainStopOnCommunicationFailureCheck = True
            optEnableTrainStopOnDerailmentDetectorCheck = True
            optEnableTrainStopOnOccupancyCheck = True
            optEnableTrainStopCommand = True
            optEnableTrainConsistRequest = True
            optEnableTrainMenuInstantMessage = True
            optEnableDirectionBindNorthToEastAndSouthToWest = True
            optEnableTrainSimulatorMenu = True
            optEnableBindingEntryExitTracksToStationReporting = True
            optEnableOnlyShowFailureOnCodeStationComponents = False
            optEnableMRSSpecificPTCOperations = True
            optEnableLicenseGenerationForPTC = True
            optAllowDisableAutomaticSignalSlottingForUnSolicatedSignals = True
            optAllowDisableAutomaticSignalSlotting = True
            optAllowSecondsInOSReportLogging = True
            optEnableExtendLicenseinSplitDesk = True
            optEnableStoredLicenseForFreeArea = True
            optEnableSplitLicensesBetweenTerritories = True
            optTimeWaitTimeForAuthorityRelease = 2
            optUseLicensingMethodForAuthority = True
            optManualOperateText = "CL"
            optTWCOSEventType = "OS"
            optEnableAllowStoreRouteExecuteOutOfOrderBasedOnSelection = True
            optDisablePassengarSpeedOnTags = True
            optAllowDeviceBlockOnSameNamedTrackOnly = True
            optEnableCheckLicenseExistenceOnTrainMove = True
            optEnableConflictChecksForTWCOSTracksMRS = True
            optAllowSwitchLegSelectionForSwitchControl = True
            optEnableGroupTerritoryLoading = True
            'loading MPH with KPH as MRS uses Kilometers.
            optMPHDisplay = "KPH"
            optbPassengerLoad = False
            optEnableCheckForAuthoritiesBeforeFinalTrain = True
            optEnableUsingTooltipField = True
            optDisableTrafficArrowsSPM = True
            'Disable Read Repeat Process
            optDisableReadRepeatProcess = True
            gReadColor = Color.LightBlue
            gRepeatColor = Color.LightGreen
            'Disable POS Switch Dialog
            optDisablePOSSwitchDialog = True
            optAutoReleaseLicenseOnTrainMove = True
            optEnableShowSignalNameOnToolTip = True
            optUseDurationTimeForTags = True
            optUseDurationTimeForDeviceBlocks = True
            optEnableTagCopyMenu = True
            optEnableTrackResetContextMenus = False
            optEnableTrackContextMenuTrainRequest = True
            optEnableTNTContextMenu = False
            optUseSingleFormAuthorityVersionMRS = True
            optEnableTrainPositionReportingForMRS = True
            optEnableNeedOccupancyToPositionMoveTrain = False
            optEnableNeedOccupancyToPositionMoveTrainElseDeviceBlock = True
            optDeviceBlockCommentsRequired = True
            optApplyTagConfigurationForMRS = True
            optDisableTagDirection = True
            optDisableTagDelayInfo = True
            optEnableDirectionalSymbolsOnTrainSymbol = True
            optEnableGenerateAlarmNonIdentifiedOccuppancy = True
            optEnableJournal = True
            optEnableAutomaticRemovalOfPRSEMT = True
            optEnableTagReadRepeat = True
            optEnableTrainConsistAbbreviated = True
            optTrainIDDisplayCrewDetails = False
            optEnableForceAllTrackInfoMouseHover = True
            optEnableSidingCalculationMouseHover = True
            optEnableTrackMileageInfoMouseHover = True
            optDisableTrackInfoMouseHover = False
            optTrainIDDisplayTrainMiscDetails = False
            optTrainSymbolQuickRouting = True
            optEnableDbAuthorityRemoveTimer = True
            optEnableDbBlkDeleteTimer = True
            optEnableTAGDeleteTimer = True
            optDisableTagXing = True
            optEnableAuthorityIssuanceMRS = True
            optEnableTrainConsistChangeMenu = False
            optEnableGenerateRoutePlan = False
            optAllow1920x1200ResolutionUse = True
            optApplyTrainSheetConfigurationForMRS = True
            optEnableCTCFunctionalityMenus = False
            optEnablePortugueseResourceLanguageFile = True
            optUseResourceCultureFile = True
            optDisableMenuItemsForMRS = True
            optUseSystemUserCredentails = True
            optDisableTNTModify = True
            optEnableDeviceBlockMenuOnMiscDevices = True
            optEnableFlashTrainForHighWide = True
            optEnableFlashTrainForKey = True
            optEnableTrainColorsForKeyHazHighWide = True
            optEnableCNFLIPFLOPLimits = True
            optEnableFlashOSTracks = True
            optEnablePSSEMTAuthorityType = True
            optEnableMenuPermissionToEnterMainTrack = False
            optEnableMenuPermissionToPassRedSignal = True
            gSendDenyMessageOnIMClose = True
            optEnableBackChannelCommunications = True
            optEnableReportingToMobileApplication = False
            optEnableSystemManagementServices = False
            optEnablePromptForMultipleTagRelease = True
            gEnableAutoClearingMode = False
            optHPTStringFormat = "0.00"
            optEnableAllowForceOperationsToTWC = True
            optEnableFlipFlopDetectionLogic = True
            optSystemTimeToLiveTasksEnabled = True
            optKeyTrainSymbolPrefix = "#"
            optShowHighWideKeyTrainSymbolTogether = True
            optEnableTerritoryRequest = True
            optEnableMultipleTrackTagSelectionForWeather = True
            optEnableWeatherTags = False
            optEnableAlertContentView = True
            optCheckForTerritoryFlipDirectionEnabled = True
            optAllowNonTWC = True
            optAllowTagCopyFunctionOnCodeFailureTrack = True
            optUseClearMainTrackColorOption = False
            optAllowNoSignalControlInitializationOnFailure = True
            optLaunchTagFormOnTrackLineDisplay = True
            optAutoRefreshViewOnlyTrains = True
            optAllowTheRemovalOfTrafficPendingRequestInTheBlock = True
            optCheckForChangeDirectionEnabled = True
            optEnableRouteBlocking = True
            optOverrideAuthorityGapCheck = True
            optExcludeIndicationLikeOsTracksWhenNonTransversable = True
            optDisallowAuthorityAfterItemTwoJoint = True
            optShowTrainSymbolColorScheme1 = False
            optShowTrainSymbolColorScheme2 = True
            optShowJointLimitsColor = True
            optEnableRadioBlockingRules = True
            optDisableUnSolicatedSignalAlert = True
            optOverrideXingTagCount = True
            optOverrideSpeedTagCount = True
            optFormatTrainSymbolRuleMRS = True
            optDisplayScreenName = True
            optPerformDocumentArchiving = True
            optEnableCalculateHorsePerTon = True
            optShowItemEightFullOverlapVerbiage = True
            optAllowItemTwoMenuWithinLimitsOfItem6and8ToTrain = True
            optFlipNorthSouthDirection = True
            optDirectionNorth = "SOUTH"
            optDirectionSouth = "NORTH"
            optAllowItem2With8ThroughItem6With8 = True
            optUseSpecificSwitchNotificationList = False
            optFillInCopyByField = True
            optEnableOnlyOneItemFourTrain = True
            optDisableSubdivisionNameOnAuthorityBuildForm = True
            optCheckForEIC = False
            optShowMobileMessages = True
            optEnableMultipleSelectModeForTNT = True
            optEnableMOWContextMenu = False
            optDisableGeneralMOWMenu = True
            optForceCopyBy = True
            optTrainIDLeftMouse = False
            optOsTimesReverseOrder = True
            optKeepAliveDatabaseCheck = False
            optDoEventLogging = True
            optDoCommCheck = True
            optDoControlServerCheck = True
            optDisableTWC = False
            optMOWEnterBehindTrainDialogBox = True
            optFormatTrainSymbolRuleOne = False
            optWeatherReminderEnabled = False
            optAllowPOSCheck = False
            optShowMobileMessages = True
            optSingleFormAuthority = True
            gSFAMode = True
            optVoidWarrantConfirmationBox = False
            optJointTrainToTrain = True
            optShowRollupMenuItem = False
            optFontAlphaTextSmall = False
            optFontAlphaTextMedium = True
            optFontControlPointSmall = False
            optFontControlPointLarge = False
            optFontControlPointMedium = True
            optDisableItemTwoToItemFourDirectionPrompt = True
            optDisallowTrainIdPlacementOnTrackWithAuthorityOfOtherTrain = True
            optPostionAndOSTrainOnAuthorityReleaseWithSFAItemThreeIntoSiding = True
            optUseWorkstationAuthorityNumbering = True
            optEnableTNTMultipleTrackSelection = True
            optEnableTNTRollUp = True
            optEnableStandAloneTrainSummary = False
            optEnableStandAloneAuthoritySummary = False
            optPassWordProtectTerritoryInsert = True
            optVerifyTrainMovementOnMainTrackBeforeSymbolMove = True
            optUseSingleFormAuthorityVersionCN = False
            optDisableTrainMovePromptOnAuthorityRelease = True
            optEnableSubdivisionFilterForTrains = False
            optEnableSystemTrainSummaryUpdating = False
            optEnableVehicleAssociationFunctionality = False
            optReportClearSimpleRelease = True
            optAllowNonTWCSelection = True
            optDeviceBlockingSimpleRelease = True
            optDisableVehicleAssociation = True
            optDisplayPseudoTrainAsMainIdentifer = False
            optDisableTrainSymbolTruncate = True
            optEnableScreenShotCapturing = True
            optEnableOverrideNonTransverableTrackCheck = True
            optEnableOverrideDerailPositionCheck = True
            optEnableOsReportingFormatKCS = True
            optEnableOverrideForceItemOne = True
            optDisableSimulateMenu = True
            optEnableVoidAuthorityMenuItem = True
            optEnableFontStringsForDevices = True
            optEnableLogOSOnAllTWCMoves = False
            optEnableTrackLockModeDisablingAuthorityIssuance = True
            optEnableTimeTableDatabaseTable = True
            optEnableForceAuthorityPrintingOnAtStartUp = True
            optEnablePrintTrainContextMenu = False
            optDisableAlertBanner = False
            optEnableTimeTableInformationPrintOut = True
            optLanguageShowFinalTrainMenuVerbiage = "FINAL TRAIN "
            optEnableTrainMiscContextMenu = False
            optDisablePassRedSignalAlert = False
            optPassRedSignalOnRequestedSignalDisabled = True
            optTrainPassRedSignalWithTrainSymbolOnly = True
            optEnableSecondaryTrainPlacement = False
            optShowTrainTimeSummary = False
            optUseSubdivisionStationTable = True
            optEnableSubdivisionGeoReferencing = True
            optEnableAutoTimeIncrementingForCrewHOS = False
            optEnableClearAllAlertsMenu = True
            optEnableResendControlSpecial = True
            optEnableMOWQuerySelection = True
            optEnableFullScreenSnapShot = False
            optEnableWeatherAlerts = True
            optPerformSidingLengthCheck = True
            optEnableSystemTrainAutoRefresh = True
            optEnableMoveUnits = False
            optEnableCTCTAGDispalyonCodeBrown = True
            optEnableSystemSummaryColor = True
            optEnableHOSCheck = True
            optEnableConsitsPower = True
            optEnableHOSAlertGeneration = False
            optEnableTrainTAG = True
            optEnableConvertTrainTAGwDelayToDelayEvent = True
            optEnableTrainUnitColor = True
            optEnableLoadTrainUnitInformation = True
            optEnableReportIssueScrShot = True
            optLocomotiveDefaultLength = 23
            optLocomotiveDefaultWeight = 215
            optLocomotiveDefaultHorsePowerEffective = 4400
            optLocomotiveDefaultHorsePowerTotal = 4400
            gPTCTimeOutTransmissionAuthorityRollup = 120000
            gPTCTimeOutTransmissionAuthorityIssuance = 120000
            gPTCTimeOutTransmissionAuthorityRelease = 900000
            gPTCTimeOutTransmissionBulletinIssuance = 120000
            gPTCTimeOutTransmissionBulletinCancel = 120000
            optEnableTWCSpecialLicense = True
            optScreenShotCaptureDuration = 1
            optEnableStoredRouteClearOnRelease = True
            optEnableTagTrackAlarm = True
            optOverrideReadRepeatForRestrictionDelivery = True
            optEnableTagWorkZone = True
            optEnableDisengagedColorStatus = True
            optEnableTrainRestrictionDelivery = True
            optPromptTrafficDirectionDivergent = False
            gEnableTrainTagTimerRefresh = True
            gEnableTrainTimerRefresh = False
            optEnableScaleAlarm = True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Sub ApplyOptionsForRUMO()
        Try
            'Alerts
            optEnableInsertAllAlertsIntoDB = True
            optenableDirectionChangeBasedOnTracking = True
            optEnableSuppressFailureModePromptsForPTC = True
            optEnableAllowSwitchAlignmentWithPassStopSignalInCodeFailure = True
            optEnableAllowSwitchControlMovementWhileInCodeFailure = True
            optEnableShowStatusUnderNonTransverableTrack = True
            optDisableDateRangeFunctionForAuthorityNumber = True
            OptEnableSignalClearByMenu = True
            optAllowQuickRouteWithSignalClearbyMenu = True
            optOverrideRemoteCheckForJointAuthorization = True
            optEnableTestTrainForPTC = True
            optEnableAutomaticPositionReportRequesting = True
            optEnableAutomaticCutoutFromActiveToDisengagedState = True
            optLocomotiveReportKeepAliveIntervalCheck = 120
            optSystemDateAndTimeString = "dd/MM/yyyy HH:mm"
            optSystemDateTimeString = "dd/MM/yyyy"
            optEnableDeviceBlkExpirationCheck = True
            optUseDeviceBlockTimeExpried = True
            optEnableLostOccupancyAlert = True
            optEnableLostOccupancyAlertWithTrackBlock = True
            optUseResourceCultureFile = True
            optEnableAutoGenerateClearance = True
            optGetTrainUnitsFromLocomotiveTable = True
            optEnableAxlesBrakesCounts = True
            optEnableTagModifyMenu = False
            gEnablePTCVETMSTrackingOnCTC = True
            '*** CODE BROWN OPTIONS
            optEnableToPassRedSignalOnCodeBrown = True
            optEnableTrainMenuCancelStoredRoutes = True
            optAllowTrafficChangeOnTrainDirectionChange = True
            optEnableOverrideSIGValidationForPRS = True
            optEnableOverrideDeviceBlockForPRS = True
            optEnableOverrideJointTNTForPRS = True
            optEnableOverrideTNTForPRS = True
            optTrainIDColorChangeForCBTCEnabled = True
            optUseResourceCultureFile = True
            optOverrideReadRepeatForRestrictionDelivery = True
            optEnableTrainRestrictionDelivery = True
            optDisableFlashingTrackForPTC = True
            gUseTrackAuthorityTypeInPlaceOfTrackAndTime = True
            optEnableMRSSpecificOptions = True
            optEnablePTCBOSVETMSMode = True
            optEnablePTCOperations = True
            optEnableDisallowDuplicateEngineIDsToTrains = True
            optEnableClearSignalWithSignalIntime = False
            optEnableTrainStopOnPowerOffManualOperateCheck = True
            optEnableTrainStopOnDerailmentDetectorCheck = True
            optEnableTrainStopOnOccupancyCheck = True
            optEnableTrainStopCommand = True
            optEnableTrainConsistRequest = True
            optEnableTrainMenuInstantMessage = True
            optEnableDirectionBindNorthToEastAndSouthToWest = True
            optEnableTrainSimulatorMenu = True
            optEnableBindingEntryExitTracksToStationReporting = True
            optEnableOnlyShowFailureOnCodeStationComponents = False
            optEnableMRSSpecificPTCOperations = True
            optEnableLicenseGenerationForPTC = True
            optAllowDisableAutomaticSignalSlottingForUnSolicatedSignals = True
            optAllowDisableAutomaticSignalSlotting = True
            optAllowSecondsInOSReportLogging = True
            optTimeWaitTimeForAuthorityRelease = 2
            optUseLicensingMethodForAuthority = True
            optManualOperateText = "CL"
            optTWCOSEventType = "OS"
            optEnableAllowStoreRouteExecuteOutOfOrderBasedOnSelection = True
            optDisablePassengarSpeedOnTags = True
            optAllowDeviceBlockOnSameNamedTrackOnly = True
            optEnableCheckLicenseExistenceOnTrainMove = True
            optEnableConflictChecksForTWCOSTracksMRS = True
            optAllowSwitchLegSelectionForSwitchControl = True
            optEnableGroupTerritoryLoading = True
            'loading MPH with KPH as MRS uses Kilometers.
            optMPHDisplay = "KPH"
            optbPassengerLoad = False
            optEnableCheckForAuthoritiesBeforeFinalTrain = True
            optEnableUsingTooltipField = True
            optDisableTrafficArrowsSPM = True
            'Disable Read Repeat Process
            optDisableReadRepeatProcess = True
            gReadColor = Color.LightBlue
            gRepeatColor = Color.LightGreen
            'Disable POS Switch Dialog
            optDisablePOSSwitchDialog = True
            optAutoReleaseLicenseOnTrainMove = True
            optEnableShowSignalNameOnToolTip = True
            optUseDurationTimeForTags = True
            optUseDurationTimeForDeviceBlocks = True
            optEnableTagCopyMenu = True
            optEnableTrackResetContextMenus = False
            optEnableTrackContextMenuTrainRequest = True
            optEnableTNTContextMenu = False
            optUseSingleFormAuthorityVersionMRS = True
            optEnableTrainPositionReportingForMRS = True
            optEnableNeedOccupancyToPositionMoveTrain = False
            optEnableNeedOccupancyToPositionMoveTrainElseDeviceBlock = True
            optDeviceBlockCommentsRequired = True
            optApplyTagConfigurationForMRS = True
            optDisableTagDirection = True
            optDisableTagDelayInfo = True
            optEnableDirectionalSymbolsOnTrainSymbol = True
            optEnableGenerateAlarmNonIdentifiedOccuppancy = True
            optEnableJournal = True
            optEnableAutomaticRemovalOfPRSEMT = True
            optEnableTagReadRepeat = True
            optEnableTrainConsistAbbreviated = True
            optTrainIDDisplayCrewDetails = False
            optEnableForceAllTrackInfoMouseHover = True
            optEnableSidingCalculationMouseHover = True
            optEnableTrackMileageInfoMouseHover = True
            optDisableTrackInfoMouseHover = False
            optTrainIDDisplayTrainMiscDetails = False
            optTrainSymbolQuickRouting = True
            optEnableDbAuthorityRemoveTimer = True
            optEnableDbBlkDeleteTimer = True
            optEnableTAGDeleteTimer = True
            optDisableTagXing = True
            optEnableAuthorityIssuanceMRS = True
            optEnableTrainConsistChangeMenu = False
            optEnableGenerateRoutePlan = False
            optAllow1920x1200ResolutionUse = True
            optApplyTrainSheetConfigurationForRUMO = True
            optEnableCTCFunctionalityMenus = False
            optEnablePortugueseResourceLanguageFile = True
            optUseResourceCultureFile = True
            optDisableMenuItemsForMRS = True
            optUseSystemUserCredentails = True
            optDisableTNTModify = True
            optEnableDeviceBlockMenuOnMiscDevices = True
            optEnableFlashTrainForHighWide = True
            optEnableFlashTrainForKey = True
            optEnableTrainColorsForKeyHazHighWide = True
            optEnableCNFLIPFLOPLimits = True
            optEnableFlashOSTracks = True
            optEnablePSSEMTAuthorityType = True
            optEnableMenuPermissionToEnterMainTrack = False
            optEnableMenuPermissionToPassRedSignal = True
            gSendDenyMessageOnIMClose = True
            optEnableBackChannelCommunications = True
            optEnableReportingToMobileApplication = False
            optEnableSystemManagementServices = False
            optEnablePromptForMultipleTagRelease = True
            gEnableAutoClearingMode = False
            optHPTStringFormat = "0.00"
            optEnableAllowForceOperationsToTWC = True
            optEnableFlipFlopDetectionLogic = True
            optSystemTimeToLiveTasksEnabled = True
            optKeyTrainSymbolPrefix = "#"
            optShowHighWideKeyTrainSymbolTogether = True
            optEnableTerritoryRequest = True
            optEnableMultipleTrackTagSelectionForWeather = True
            optEnableWeatherTags = False
            optEnableAlertContentView = True
            optCheckForTerritoryFlipDirectionEnabled = True
            optAllowNonTWC = True
            optAllowTagCopyFunctionOnCodeFailureTrack = True
            optUseClearMainTrackColorOption = False
            optAllowNoSignalControlInitializationOnFailure = True
            optLaunchTagFormOnTrackLineDisplay = True
            optAutoRefreshViewOnlyTrains = True
            optAllowTheRemovalOfTrafficPendingRequestInTheBlock = True
            optCheckForChangeDirectionEnabled = True
            optEnableRouteBlocking = True
            optOverrideAuthorityGapCheck = True
            optExcludeIndicationLikeOsTracksWhenNonTransversable = True
            optDisallowAuthorityAfterItemTwoJoint = True
            optShowTrainSymbolColorScheme1 = False
            optShowTrainSymbolColorScheme2 = True
            optShowJointLimitsColor = True
            optEnableRadioBlockingRules = True
            optDisableUnSolicatedSignalAlert = True
            optOverrideXingTagCount = True
            optOverrideSpeedTagCount = True
            optFormatTrainSymbolRuleMRS = True
            optDisplayScreenName = True
            optPerformDocumentArchiving = True
            optEnableCalculateHorsePerTon = True
            optShowItemEightFullOverlapVerbiage = True
            optAllowItemTwoMenuWithinLimitsOfItem6and8ToTrain = True
            optFlipNorthSouthDirection = True
            optDirectionNorth = "SOUTH"
            optDirectionSouth = "NORTH"
            optAllowItem2With8ThroughItem6With8 = True
            optUseSpecificSwitchNotificationList = False
            optFillInCopyByField = True
            optEnableOnlyOneItemFourTrain = True
            optDisableSubdivisionNameOnAuthorityBuildForm = True
            optCheckForEIC = False
            optShowMobileMessages = True
            optEnableMultipleSelectModeForTNT = True
            optEnableMOWContextMenu = False
            optDisableGeneralMOWMenu = True
            optForceCopyBy = True
            optTrainIDLeftMouse = False
            optOsTimesReverseOrder = True
            optKeepAliveDatabaseCheck = False
            optDoEventLogging = True
            optDoCommCheck = True
            optDoControlServerCheck = True
            optDisableTWC = False
            optMOWEnterBehindTrainDialogBox = True
            optFormatTrainSymbolRuleOne = False
            optWeatherReminderEnabled = False
            optAllowPOSCheck = False
            optShowMobileMessages = True
            optSingleFormAuthority = True
            gSFAMode = True
            optVoidWarrantConfirmationBox = False
            optJointTrainToTrain = True
            optShowRollupMenuItem = False
            optFontAlphaTextSmall = False
            optFontAlphaTextMedium = True
            optFontControlPointSmall = False
            optFontControlPointLarge = False
            optFontControlPointMedium = True
            optDisableItemTwoToItemFourDirectionPrompt = True
            optDisallowTrainIdPlacementOnTrackWithAuthorityOfOtherTrain = True
            optPostionAndOSTrainOnAuthorityReleaseWithSFAItemThreeIntoSiding = True
            optUseWorkstationAuthorityNumbering = True
            optEnableTNTMultipleTrackSelection = True
            optEnableTNTRollUp = True
            optEnableStandAloneTrainSummary = False
            optEnableStandAloneAuthoritySummary = False
            optPassWordProtectTerritoryInsert = True
            optVerifyTrainMovementOnMainTrackBeforeSymbolMove = True
            optUseSingleFormAuthorityVersionCN = False
            optDisableTrainMovePromptOnAuthorityRelease = True
            optEnableSubdivisionFilterForTrains = False
            optEnableSystemTrainSummaryUpdating = False
            optEnableVehicleAssociationFunctionality = False
            optReportClearSimpleRelease = True
            optAllowNonTWCSelection = True
            optDeviceBlockingSimpleRelease = True
            optDisableVehicleAssociation = True
            optDisplayPseudoTrainAsMainIdentifer = False
            optDisableTrainSymbolTruncate = True
            optEnableScreenShotCapturing = True
            optEnableOverrideNonTransverableTrackCheck = True
            optEnableOverrideDerailPositionCheck = True
            optEnableOsReportingFormatKCS = True
            optEnableOverrideForceItemOne = True
            optDisableSimulateMenu = True
            optEnableVoidAuthorityMenuItem = True
            optEnableFontStringsForDevices = True
            optEnableLogOSOnAllTWCMoves = False
            optEnableTrackLockModeDisablingAuthorityIssuance = True
            optEnableTimeTableDatabaseTable = True
            optEnableForceAuthorityPrintingOnAtStartUp = True
            optEnablePrintTrainContextMenu = False
            optDisableAlertBanner = False
            optEnableTimeTableInformationPrintOut = True
            optLanguageShowFinalTrainMenuVerbiage = "FINAL TRAIN "
            optEnableTrainMiscContextMenu = False
            optDisablePassRedSignalAlert = False
            optPassRedSignalOnRequestedSignalDisabled = True
            optTrainPassRedSignalWithTrainSymbolOnly = True
            optEnableSecondaryTrainPlacement = False
            optShowTrainTimeSummary = False
            optUseSubdivisionStationTable = True
            optEnableSubdivisionGeoReferencing = True
            optEnableAutoTimeIncrementingForCrewHOS = False
            optEnableClearAllAlertsMenu = True
            optEnableResendControlSpecial = True
            optEnableMOWQuerySelection = True
            optEnableFullScreenSnapShot = False
            optEnableWeatherAlerts = True
            optPerformSidingLengthCheck = True
            optEnableSystemTrainAutoRefresh = True
            optEnableMoveUnits = False
            optEnableCTCTAGDispalyonCodeBrown = True
            optEnableSystemSummaryColor = True
            optEnableHOSCheck = True
            optEnableConsitsPower = True
            optEnableHOSAlertGeneration = False
            optEnableTrainTAG = True
            optEnableConvertTrainTAGwDelayToDelayEvent = True
            optEnableTrainUnitColor = True
            optEnableLoadTrainUnitInformation = True
            optEnableReportIssueScrShot = True
            optLocomotiveDefaultLength = 23
            optLocomotiveDefaultWeight = 215
            optLocomotiveDefaultHorsePowerEffective = 4400
            optLocomotiveDefaultHorsePowerTotal = 4400
            gPTCTimeOutTransmissionAuthorityRollup = 120000
            gPTCTimeOutTransmissionAuthorityIssuance = 120000
            gPTCTimeOutTransmissionAuthorityRelease = 120000
            gPTCTimeOutTransmissionBulletinIssuance = 120000
            gPTCTimeOutTransmissionBulletinCancel = 120000
            optEnableTWCSpecialLicense = False
            optScreenShotCaptureDuration = 1
            optEnableStoredRouteClearOnRelease = True
            optEnableTagTrackAlarm = False
            optEnableTagWorkZone = True
            optEnableDisengagedColorStatus = True
            optEnableTrainRestrictionDelivery = True
            optPromptTrafficDirectionDivergent = False
            gEnableTrainTagTimerRefresh = True
            gEnableTrainTimerRefresh = False
            optEnableAutoBlockSwitchesInSignalRoute = True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

#End Region

#Region "CHECKSUM ARRAYS "

    Private Sub LoadTNTCheckSumVariableArray()
        Try
            gTNTCheckSumVariables = New ArrayList
            With gTNTCheckSumVariables
                .Add("AuthGUID")
                .Add("AuthorityNumber")
                .Add("RailRoadName")
                .Add("DispatcherName")
                .Add("CreateDate")
                .Add("Direction")
                .Add("AuthorityDate")
                .Add("AuthorityType")
                .Add("IssueTo")
                .Add("AuthorityLimits")
                .Add("OKTime")
                .Add("UntilTime")
                .Add("SoleAuthority")
                .Add("TerritoryAssignment")
                .Add("TerritoryAssignmentSecondary")
                .Add("TerrIDTeritary")
                .Add("TerrIDQuadtary")
                .Add("AuthorityUID")
                .Add("JointAuthority")
                .Add("JointLimits")
                .Add("IssuedToMobileClient")
                .Add("CurrentTimeZone")
                .Add("CopiedBy")
                .Add("RelayedTo")
                .Add("TrainSymbol")
                .Add("IssuedToTrain")
                .Add("JointAbsolute")
                .Add("LimitsInfo")
                .Add("BehindTrainsInfo")
                .Add("BehindTrains")
                .Add("PTCAuthorityText")
                .Add("PTCAuthoritySegments")
                .Add("PTCCompositionCRC")
                .Add("PTCContingencySegments")
                .Add("AllowTrainMovementThroughLimits")
                .Add("CurrentRollUpEvent")
                .Add("AuthorityStatus")
                .Add("AuthVoidList")
                .Add("JointVerbiage")
                .Add("UntilTimeCount")
                .Add("Box1List")
                .Add("LicenseTypeAuthority")
            End With
        Catch ex As Exception
            GetErrorDetails(ex)
            optEnableTNTCheckSum = False
        End Try
    End Sub

    Private Sub LoadSFACheckSumVariableArray()
        Try
            gSFACheckSumVariables = New ArrayList
            With gSFACheckSumVariables
                .Add("RailRoadName")
                .Add("Subdivision")
                .Add("TimeZone")
                .Add("DispName")
                .Add("Direction")
                .Add("AuthDate")
                .Add("TerrID1")
                .Add("TerrID2")
                .Add("TerrID3")
                .Add("AuthType1")
                .Add("AuthType2")
                .Add("AuthType3")
                .Add("AuthorityNumber")
                .Add("AuthorityUID")
                .Add("AuthGUID")
                .Add("IssuedToTrain")
                .Add("IssueTo")
                .Add("AtLocation")
                .Add("Box2From1")
                .Add("Box2To1")
                .Add("Box2Track1")
                .Add("Box2From2")
                .Add("Box2To2")
                .Add("Box2Track2")
                .Add("Box2From3")
                .Add("Box2To3")
                .Add("Box2Track3")
                .Add("Box3Clear")
                .Add("Box4From1")
                .Add("Box4To1")
                .Add("Box4Track1")
                .Add("Box4From2")
                .Add("Box4To2")
                .Add("Box4Track2")
                .Add("Box4From3")
                .Add("Box4To3")
                .Add("Box4Track3")
                .Add("Box5At")
                .Add("Box8From1")
                .Add("Box8To1")
                .Add("Box8Track1")
                .Add("Box10ClearBy")
                .Add("Box11MPH1")
                .Add("Box11From1")
                .Add("Box11To1")
                .Add("Box11Track1")
                .Add("Box11MPH2")
                .Add("Box11From2")
                .Add("Box11To2")
                .Add("Box11Track2")
                .Add("Box14Instructions")
                .Add("BoxItems")
                .Add("NumOfBoxesChecked")
                .Add("OKTime")
                .Add("CopiedBy")
                .Add("RelayedTo")
                .Add("JointAuthority")
                .Add("IssuedToMobileClient")
                .Add("AuthorityVerbiage")
                .Add("CurrentTimeZone")
                .Add("ReducedLimitsVerbiage")
                .Add("ContinegentLimitsVerbiage")
                .Add("CurrentRollUpEvent")
                .Add("TrainSymbol")
                .Add("ClearMainLocationOne")
                .Add("ClearMainLocationTwo")
                .Add("Box11TrainMeetLocation")
                .Add("ShowItemFourComponents")
                .Add("BoxFourTrainSatifiedOne")
                .Add("BoxFourTrainSatifiedTwo")
                .Add("BoxFourTrainSatifiedThree")
                .Add("PTCAuthorityText")
                .Add("PTCAuthoritySegments")
                .Add("PTCCompositionCRC")
                .Add("PTCContingencySegments")
                .Add("RadioBlockingUsed")
                .Add("JointAbsolute")
                .Add("AuthorityStatus")
                .Add("TagInfo")
                .Add("JointWithTrain")
                .Add("JointWithMOW")
                .Add("OptionalJoint")
                .Add("Box4NormalSwitchRptOnReleasePTC")
            End With
        Catch ex As Exception
            GetErrorDetails(ex)
            optEnableSFACheckSum = False
        End Try
    End Sub
#End Region

#Region "DATABASE ROUTINES"

    Private Function LoadElectricalSwitches(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING ELECTRICAL SWITCH...")
            gSplashScreenString = "LOADING ELECTRICAL SWITCHES FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompElectricalSwitches  WHERE TerritoryUID = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            While dr.Read
                Dim es As New ElectricalSwitch
                es.GUID = dr.GetInt32(0)
                es.StationNumber = dr.GetInt32(1)
                es.Name = dr.GetString(2)
                es.LX = dr.GetInt16(4)
                es.LY = dr.GetInt16(5)
                es.RX = dr.GetInt16(6)
                es.RY = dr.GetInt16(7)
                es.LeftLink = dr.GetInt32(8)
                es.RightLink = dr.GetInt32(9)
                es.LeftAltLink = dr.GetInt32(10)
                es.RightAltLink = dr.GetInt32(11)
                es.AltLinkOne = dr.GetInt32(12)
                es.AltLinkTwo = dr.GetInt32(13)
                es.CodelineNumber = dr.GetInt32(14)
                es.TerritoryID = dr.GetInt32(21)
                es.Subdivision = dr.GetInt32(22)
                es.IndicateOnly = dr.GetBoolean(23)
                es.BlockAble = dr.GetBoolean(24)
                es.LockBit = dr.GetString(25)
                es.SwitchBlockInd = dr.GetBoolean(26)
                es.SwitchBlockBit = dr.GetString(27)
                es.TypeOfSwitch = dr.GetString(29)
                es.MatchMode = dr.GetBoolean(30)

                'close ind
                es.TempCodeStationBitBuild(dr.GetString(15), es.Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                Dim CtlWord1() As String
                Dim CtlWord2() As String
                CtlWord1 = dr.GetString(16).Split("|")
                CtlWord2 = dr.GetString(18).Split("|")

                'close control
                es.TempCodeStationBitBuild(CtlWord1(0), es.Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                'open control
                es.TempCodeStationBitBuild(CtlWord2(0), es.Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)


                'INHIBIT
                If Not dr.IsDBNull(39) Then es.TempCodeStationBitBuild(dr.GetString(39), es.IndInhibit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                'LOCAL CONTROL
                If Not dr.IsDBNull(40) Then es.TempCodeStationBitBuild(dr.GetString(40), es.IndLocalControl, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                'LOCK OUT
                If Not dr.IsDBNull(41) Then es.TempCodeStationBitBuild(dr.GetString(41), es.IndLockOut, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                'RACK OUT
                If Not dr.IsDBNull(42) Then es.TempCodeStationBitBuild(dr.GetString(42), es.IndRackOut, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                If Not dr.IsDBNull(31) Then
                    es.GraphicalGroupID = dr.GetValue(31)
                End If
                If Not dr.IsDBNull(32) Then
                    es.ScreenRegionID = dr.GetValue(32)
                End If
                If Not dr.IsDBNull(33) Then
                    es.FlipOperations = dr.GetValue(33)
                End If
                If Not dr.IsDBNull(34) Then
                    es.MakeInvisible = dr.GetValue(34)
                End If
                If Not dr.IsDBNull(35) Then
                    es.EnableTrackLineDisplayMode = dr.GetValue(35)
                End If

                If Not dr.IsDBNull(38) Then
                    es.SwitchControlledClosed = dr.GetValue(38)
                End If


                If Not dr.IsDBNull(43) Then
                    es.SwitchIndicatedClosed = dr.GetValue(43)
                End If

                If Not dr.IsDBNull(46) Then
                    es.PowerFromLeftDetectedFixed = dr.GetValue(46)
                End If

                If Not dr.IsDBNull(47) Then
                    es.PowerFromRightDetectedFixed = dr.GetValue(47)
                End If


                AddHandler es.MouseHover, AddressOf es.Switch_MouseHover
                AddHandler es.MouseEnter, AddressOf es.Switch_MouseEnter
                AddHandler es.MouseLeave, AddressOf es.Switch_MouseLeave

                es.Visible = True
                gComponentsCollection.Add(es)
                gTerritoryComponentsCollection.Add(es)
                ElectricalSwitchCollection.Add(es)
            End While
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadElectricalLines(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING ELECTRICAL LINES...")
            gSplashScreenString = "LOADING ELECTRICAL LINES FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompElectricalLines  WHERE TerritoryUID = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            While dr.Read
                Dim el As New ElectricalLine
                el.GUID = dr.GetInt32(0)
                el.StationNumber = dr.GetInt32(1)
                el.Name = dr.GetString(2)
                el.LX = dr.GetInt16(4)
                el.LY = dr.GetInt16(5)
                el.RX = dr.GetInt16(6)
                el.RY = dr.GetInt16(7)
                el.LeftLink = dr.GetInt32(8)
                el.RightLink = dr.GetInt32(9)
                el.LeftAltLink = dr.GetInt32(10)
                el.RightAltLink = dr.GetInt32(11)
                el.AltLinkOne = dr.GetInt32(12)
                el.AltLinkTwo = dr.GetInt32(13)
                el.CodelineNumber = dr.GetInt32(14)
                el.LeftLimitRange = dr.GetInt32(21)
                el.RightLimitRange = dr.GetInt32(22)
                el.LeftLimitName = dr.GetString(23)
                el.RightLimitName = dr.GetString(24)

                el.TerritoryID = dr.GetInt32(25)
                el.Subdivision = dr.GetInt32(26)

                If Not dr.IsDBNull(27) Then
                    el.GraphicalGroupID = dr.GetValue(27)
                End If
                If Not dr.IsDBNull(28) Then
                    el.ScreenRegionID = dr.GetValue(28)
                End If

                If Not dr.IsDBNull(30) Then
                    el.TypeOfLine = dr.GetValue(30)
                End If

                If Not dr.IsDBNull(31) Then
                    el.FlipOperations = dr.GetValue(31)
                End If
                If Not dr.IsDBNull(32) Then
                    el.MakeInvisible = dr.GetValue(32)
                End If
                el.Visible = True

                gComponentsCollection.Add(el)
                gTerritoryComponentsCollection.Add(el)
                ElectricalLineCollection.Add(el)

            End While
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadElectricalConnections(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING ELECTRICAL CONNECTIONS...")
            gSplashScreenString = "LOADING ELECTRICAL CONNECTIONS FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompElectricalConnections  WHERE TerritoryUID = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            While dr.Read
                Dim ec As New ElectricalConnection
                ec.GUID = dr.GetInt32(0)
                ec.StationNumber = dr.GetInt32(1)
                ec.Name = dr.GetString(2)
                ' ec.type = dr.GetString(3)
                ec.LX = dr.GetInt16(4)
                ec.LY = dr.GetInt16(5)
                ec.RX = dr.GetInt16(6)
                ec.RY = dr.GetInt16(7)
                ec.LeftLink = dr.GetInt32(8)
                ec.RightLink = dr.GetInt32(9)
                ec.LeftAltLink = dr.GetInt32(10)
                ec.RightAltLink = dr.GetInt32(11)
                ec.AltLinkOne = dr.GetInt32(12)
                ec.AltLinkTwo = dr.GetInt32(13)
                ec.CodelineNumber = dr.GetInt32(14)

                'ec.Ind1 = dr.GetString(15)
                'ec.Ctl1 = dr.GetString(16)

                'ec.Ind2 = dr.GetString(17)
                'ec.Ctl2 = dr.GetString(18)

                'ec.Ind3 = dr.GetString(19)
                'ec.Ctl3 = dr.GetString(20)

                ec.TerritoryID = dr.GetInt32(21)
                ec.Subdivision = dr.GetInt32(22)

                If Not dr.IsDBNull(23) Then
                    ec.GraphicalGroupID = dr.GetValue(23)
                End If
                If Not dr.IsDBNull(24) Then
                    ec.ScreenRegionID = dr.GetValue(24)
                End If

                If Not dr.IsDBNull(26) Then
                    ec.MakeInvisible = dr.GetValue(26)
                End If
                ec.Visible = True

                gComponentsCollection.Add(ec)
                gTerritoryComponentsCollection.Add(ec)
                ElectricalConnectionCollection.Add(ec)

            End While
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadSlideZone(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING SLIDE ZONE...")
            gSplashScreenString = "LOADING SLIDE ZONE FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompSlideZones  WHERE TerritoryID = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            While dr.Read
                Dim sz As New SlideZone
                sz = New SlideZone
                sz.GUID = dr.GetInt32(0)
                sz.TerritoryID = TerritoryID
                sz.StationNumber = dr.GetInt32(1)
                sz.Name = dr.GetString(2)
                sz.ComponentType = dr.GetString(3)
                sz.LX = dr.GetInt16(4)
                sz.LY = dr.GetInt16(5)
                sz.RX = dr.GetInt16(6)
                sz.RY = dr.GetInt16(7)
                sz.TerritoryID = TerritoryID
                sz.LeftLimitMPRange = dr.GetDouble(9)
                sz.RightLimitMPRange = dr.GetDouble(10) : sz.myMilePost = dr.GetDouble(10)
                sz.MilePostPrefix = dr.GetString(11) : sz.myMilePostPrefix = dr.GetString(11)
                sz.MilePostSuffix = dr.GetString(12) : sz.myMilePostSuffix = dr.GetString(12)
                sz.CodelineNumber = dr.GetInt32(13)
                sz.ZoneUID = dr.GetInt32(20)
                sz.RailroadUID = dr.GetString(21)
                sz.LongName = dr.GetString(22)
                sz.TrackReferenceList = dr.GetString(23)
                sz.ZoneTypeList = dr.GetString(24)
                sz.SubdivisionID = dr.GetInt32(27)

                sz.Visible = False
                If Not dr.IsDBNull(25) Then
                    sz.GraphicalGroupID = dr.GetValue(25)
                End If
                If Not dr.IsDBNull(26) Then
                    sz.ScreenRegionID = dr.GetValue(26)
                End If

                AddHandler sz.MouseHover, AddressOf sz.SlideZone_MouseHover

                gComponentsCollection.Add(sz)
                gTerritoryComponentsCollection.Add(sz)
                SlideZoneCollection.Add(sz)

            End While
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadWeatherStation(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING WEATHER STATION...")
            gSplashScreenString = "LOADING WEATHER STATION FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompWeatherStations  WHERE TerritoryAssignment = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            If dr IsNot Nothing AndAlso dr.HasRows Then
                While dr.Read
                    Dim ws As New WeatherStation
                    ws = New WeatherStation
                    ws.GUID = dr.GetInt32(0)
                    ws.StationNumber = dr.GetInt32(1)
                    ws.Name = dr.GetString(2)
                    ws.ComponentType = dr.GetString(3)
                    ws.LX = dr.GetInt16(4)
                    ws.LY = dr.GetInt16(5)
                    ws.RX = dr.GetInt16(6)
                    ws.RY = dr.GetInt16(7)
                    ws.TerritoryID = dr.GetInt32(14)

                    If Not dr.IsDBNull(17) Then
                        ws.GraphicalGroupID = dr.GetValue(17)
                    End If

                    If Not dr.IsDBNull(18) Then
                        ws.ScreenRegionID = dr.GetValue(18)
                    End If

                    If Not dr.IsDBNull(15) Then
                        ws.Subdivision = dr.GetValue(15)
                    End If

                    If Not dr.IsDBNull(16) Then
                        ws.TrackGUID = dr.GetInt32(16)
                    End If

                    If Not dr.IsDBNull(19) Then
                        ws.MilePost = dr.GetDouble(19)
                    End If

                    If Not dr.IsDBNull(20) Then
                        ws.MilePostSuffix = dr.GetString(20)
                    End If

                    If Not dr.IsDBNull(21) Then
                        ws.MilePostPrefix = dr.GetString(21)
                    End If

                    If Not dr.IsDBNull(22) Then
                        ws.Notes = dr.GetString(22)
                    End If

                    If Not dr.IsDBNull(23) Then
                        ws.TypeOfStation = dr.GetString(23)
                    End If

                    If Not dr.IsDBNull(24) Then
                        ws.UniqueName = dr.GetString(24)
                    End If

                    ws.Visible = True

                    AddHandler ws.MouseHover, AddressOf ws.WeatherStation_MouseHover

                    gComponentsCollection.Add(ws)
                    gTerritoryComponentsCollection.Add(ws)
                    WeatherStationCollection.Add(ws)

                End While
            End If
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadGradeCrossing(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing

        Try
            SystemEvent("LOADING GRADE CROSSING...")
            gSplashScreenString = "LOADING GRADE CROSSINGS FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblSystemInformationHighwayCrossings  WHERE TerritoryID = " & TerritoryID)
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)
            dr = dbcomm.ExecuteReader
            Dim count As Integer = 1
            While dr.Read
                Dim Hgc As New GradeCrossing
                Hgc.GUID = count
                count = count + 1
                If Not dr.IsDBNull(0) Then
                    Hgc.myGUID = dr.GetString(0)
                    If IsNumeric(Hgc.myGUID) Then
                        Hgc.GUID = CInt(Hgc.myGUID)
                    End If
                End If
                If Not dr.IsDBNull(1) Then Hgc.DOT = dr.GetString(1)
                If Not dr.IsDBNull(2) Then Hgc.StreetName = dr.GetString(2)
                If Not dr.IsDBNull(3) Then Hgc.City = dr.GetString(3)
                If Not dr.IsDBNull(4) Then Hgc.State = dr.GetString(4)
                If Not dr.IsDBNull(5) Then Hgc.ZipCode = dr.GetString(5)
                If Not dr.IsDBNull(6) Then Hgc.CrossingType = dr.GetString(6)
                If Not dr.IsDBNull(7) Then Hgc.MilePost = dr.GetString(7)
                If Not dr.IsDBNull(10) Then Hgc.mySubdivisionUID = dr.GetInt32(10)
                If Not dr.IsDBNull(24) Then Hgc.sTrackList = dr.GetString(24)
                If Not dr.IsDBNull(20) Then Hgc.MilePostPrefix = dr.GetString(20)
                If Not dr.IsDBNull(19) Then Hgc.MilePostSuffix = dr.GetString(19)
                If Not dr.IsDBNull(27) Then Hgc.LX = dr.GetInt16(27)
                If Not dr.IsDBNull(28) Then Hgc.LY = dr.GetInt16(28)
                If Not dr.IsDBNull(29) Then Hgc.RX = dr.GetInt16(29)
                If Not dr.IsDBNull(30) Then Hgc.RY = dr.GetInt16(30)

                If dr.FieldCount >= 32 Then
                    If Not dr.IsDBNull(31) Then Hgc.StationNumber = dr.GetInt32(31)
                End If

                If dr.FieldCount >= 33 Then
                    If Not dr.IsDBNull(32) Then
                        Hgc.GraphicalGroupID = dr.GetValue(32)
                    End If
                End If
                If dr.FieldCount >= 36 Then
                    If Not dr.IsDBNull(33) Then Hgc.ScreenRegionID = dr.GetValue(33)
                    If Not dr.IsDBNull(34) Then Hgc.sCrossingEdges = dr.GetValue(34)
                    If Not dr.IsDBNull(35) Then Hgc.MakeInvisible = dr.GetBoolean(35)
                End If
                If dr.FieldCount >= 41 Then
                    If Not dr.IsDBNull(36) Then Boolean.TryParse(dr.GetValue(36), Hgc.IsNonControlledTrack)
                    If Not dr.IsDBNull(37) Then Hgc.MilePostAlternate = dr.GetValue(37)
                    If Not dr.IsDBNull(38) Then Hgc.MilePostAlternateSuffix = dr.GetValue(38)
                    If Not dr.IsDBNull(39) Then Hgc.MilePostAlternatePrefix = dr.GetValue(39)
                    If Not dr.IsDBNull(40) Then Hgc.AlternateSubName = dr.GetValue(40)
                End If
                If Hgc.SubdivisionID = 0 AndAlso Hgc.mySubdivisionUID > 0 Then
                    Hgc.SubdivisionID = Hgc.mySubdivisionUID
                End If

                ParseCrossingEdges(Hgc)
                Hgc.Visible = False
                Hgc.TerritoryID = TerritoryID
                Hgc.ReferenceToTracks()
                AddHandler Hgc.MouseHover, AddressOf Hgc.GradeCrossing_MouseHover
                gComponentsCollection.Add(Hgc)
                gTerritoryComponentsCollection.Add(Hgc)
                GradeCrossingCollection.Add(Hgc)
            End While
            dbconn.Close()
            dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return False
        End Try
    End Function

    Private Function LoadSignalComponents(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        'Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim I As Integer
        Try
            SystemEvent("LOADING SIGNALS...")
            gSplashScreenString = "LOADING SIGNALS FOR " & TerrName
            'dbconn = New OleDbConnection(gConnectionStringStatic)
            ''dbconn.Open()

            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic Is Nothing Then
                TMDS.GlobalDeclarations.gDatabaseConnectionStatic = New OleDbConnection(gConnectionStringStatic)
            End If
            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic.State <> ConnectionState.Open Then TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Open()


            dbcomm = New OleDbCommand(gSignalSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            dr = dbcomm.ExecuteReader
            I = SignalComponent.GetUpperBound(0)

            While dr.Read()
                Application.DoEvents()
                I = I + 1
                ReDim Preserve SignalComponent(I)
                SignalComponent(I) = New Signal()
                SignalComponent(I).GUID = dr.GetInt32(0)
                SignalComponent(I).StationNumber = dr.GetInt32(1)
                SignalComponent(I).Name = dr.GetString(2)
                SignalComponent(I).ComponentType = dr.GetString(3)
                SignalComponent(I).LX = dr.GetInt16(4)
                SignalComponent(I).LY = dr.GetInt16(5)
                SignalComponent(I).RX = dr.GetInt16(6)
                SignalComponent(I).RY = dr.GetInt16(7)
                SignalComponent(I).LeftLink = dr.GetInt32(8)
                SignalComponent(I).RightLink = dr.GetInt32(9)
                SignalComponent(I).LeftAltLink = dr.GetInt32(10)
                SignalComponent(I).RightAltLink = dr.GetInt32(11)
                SignalComponent(I).AltLinkOne = dr.GetInt32(12)
                SignalComponent(I).AltLinkTwo = dr.GetInt32(13)
                SignalComponent(I).CodelineNumber = dr.GetInt32(14)
                SignalComponent(I).AuthorityType = dr.GetString(21)

                'simulation mode
                SignalComponent(I).SimulateMode = gSimulationMode

                If SignalComponent(I).AuthorityType = "CTC" Then
                    Dim CtlWord1() As String
                    gCTCTerritoryFound = True
                    Dim index As Integer
                    If Mid(dr.GetString(15), 1, 1) <> "0" Then
                        SignalComponent(I).TempCodeStationBitBuild(dr.GetString(15), SignalComponent(I).Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    End If

                    If Mid(dr.GetString(16 + index), 1, 1) <> "0" Then
                        CtlWord1 = dr.GetString(16).Split("|")
                        SignalComponent(I).TempCodeStationBitBuild(CtlWord1(0), SignalComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord1.Length > 1 Then
                            If CtlWord1(1) <> "" Then
                                SignalComponent(I).TempCodeStationBitBuild(CtlWord1(1), SignalComponent(I).Ctl(3), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SignalComponent(I).TempCodeStationBitBuild(dr.GetString(16), SignalComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                    End If

                    If Mid(dr.GetString(17), 1, 1) <> "0" Then
                        SignalComponent(I).TempCodeStationBitBuild(dr.GetString(17), SignalComponent(I).Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    End If

                    If Mid(dr.GetString(18), 1, 1) <> "0" Then
                        CtlWord1 = dr.GetString(18).Split("|")
                        SignalComponent(I).TempCodeStationBitBuild(CtlWord1(0), SignalComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord1.Length > 1 Then
                            If CtlWord1(1) <> "" Then
                                SignalComponent(I).TempCodeStationBitBuild(CtlWord1(1), SignalComponent(I).Ctl(4), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SignalComponent(I).TempCodeStationBitBuild(dr.GetString(18), SignalComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                    End If

                    If Mid(dr.GetString(19), 1, 1) <> "0" Then
                        SignalComponent(I).TempCodeStationBitBuild(dr.GetString(19), SignalComponent(I).Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    End If

                    If Mid(dr.GetString(20), 1, 1) <> "0" Then
                        CtlWord1 = dr.GetString(20).Split("|")
                        SignalComponent(I).TempCodeStationBitBuild(CtlWord1(0), SignalComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord1.Length > 1 Then
                            If CtlWord1(1) <> "" Then
                                SignalComponent(I).TempCodeStationBitBuild(CtlWord1(1), SignalComponent(I).Ctl(5), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SignalComponent(I).TempCodeStationBitBuild(dr.GetString(20), SignalComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                    End If
                    'Version 227
                    'Set fail state to true on the initial load until a code server status is received
                    If Not SignalComponent(I).SimulateMode Then SignalComponent(I).FailState = True
                    SignalComponent(I).Indicating = True

                End If



                SignalComponent(I).SubdivisionID = dr.GetInt32(23)
                SignalComponent(I).IndicateOnly = dr.GetBoolean(24)
                If SignalComponent(I).IndicateOnly Then
                    SignalComponent(I).DoMonitorMode = True
                End If
                SignalComponent(I).AllowFleeting = dr.GetBoolean(25)
                SignalComponent(I).AllowStacking = dr.GetBoolean(26)
                SignalComponent(I).AllowBlocking = dr.GetBoolean(27)
                SignalComponent(I).AllowCallOn = dr.GetBoolean(28)

                Select Case dr.GetString(29)
                    Case "OPPOSING"
                        SignalComponent(I).InTimeType = SignalComponents.Signal.TypeOfInTime.Opposing
                    Case "LOCKBIT"
                        SignalComponent(I).InTimeType = SignalComponents.Signal.TypeOfInTime.LockBit
                    Case "INTIMEBIT"
                        SignalComponent(I).InTimeType = SignalComponents.Signal.TypeOfInTime.InTimeBit
                    Case Else
                        SignalComponent(I).InTimeType = SignalComponents.Signal.TypeOfInTime.Opposing
                End Select

                SignalComponent(I).TempCodeStationBitBuild(dr.GetString(30), SignalComponent(I).InTimeBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                SignalComponent(I).AllowDrilling = dr.GetBoolean(31)
                If SignalComponent(I).AllowDrilling Then
                    SignalComponent(I).DrillRouteComponents = New ArrayList()
                    SignalComponent(I).DrillSharedComponents = New ArrayList()
                    Select Case dr.GetString(32)
                        Case "CONTROLLED"
                            SignalComponent(I).DrillType = SignalComponents.Signal.TypeOfDrill.Controlled
                            SignalComponent(I).TempCodeStationBitBuild(dr.GetString(33), SignalComponent(I).DrillControl, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            SignalComponent(I).TempCodeStationBitBuild(dr.GetString(34), SignalComponent(I).DrillStop, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            SignalComponent(I).TempCodeStationBitBuild(dr.GetString(35), SignalComponent(I).DrillIndOne, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(dr.GetString(36), SignalComponent(I).DrillIndTwo, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        Case "NONCONTROLLED"
                            SignalComponent(I).DrillType = SignalComponents.Signal.TypeOfDrill.NonControlled
                    End Select
                End If
                If SignalComponent(I).AllowCallOn Then
                    SignalComponent(I).TempCodeStationBitBuild(dr.GetString(41), SignalComponent(I).CallOnControl, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                End If
                SignalComponent(I).AllowSignalBlkInd = dr.GetBoolean(37)
                If SignalComponent(I).AllowSignalBlkInd Then
                    SignalComponent(I).TempCodeStationBitBuild(dr.GetString(38), SignalComponent(I).BlockIndBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                End If

                If dr.FieldCount >= 44 Then
                    SignalComponent(I).SignalSlotting = dr.GetBoolean(43)
                End If

                If dr.FieldCount >= 46 Then
                    If Not dr.IsDBNull(44) Then
                        SignalComponent(I).GraphicalGroupID = dr.GetByte(44)
                    End If
                    If Not dr.IsDBNull(45) Then
                        SignalComponent(I).ScreenRegionID = dr.GetByte(45)
                    End If
                End If
                If dr.FieldCount >= 47 Then
                    If Not dr.IsDBNull(46) Then
                        SignalComponent(I).MatchMode = dr.GetBoolean(46)
                    End If
                End If

                If dr.FieldCount >= 48 Then
                    If Not dr.IsDBNull(47) Then
                        SignalComponent(I).NoAlarmOnPending = dr.GetBoolean(47)
                    End If
                End If

                If dr.FieldCount >= 49 Then
                    If Not dr.IsDBNull(48) Then
                        SignalComponent(I).MakeInvisible = dr.GetBoolean(48)
                    End If
                End If

                If dr.FieldCount >= 50 Then
                    If Not dr.IsDBNull(49) Then
                        SignalComponent(I).ForceStopControl = dr.GetBoolean(49)
                        If optEnableForceSendStopControlToField Then
                            SignalComponent(I).ForceStopControl = True
                        End If
                    End If
                End If

                If dr.FieldCount >= 51 Then
                    If Not dr.IsDBNull(50) Then
                        Dim Aspects() As String = dr.GetString(50).Split("|")
                        If Aspects.Length >= 9 Then
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(0), SignalComponent(I).SigHead1Yellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(1), SignalComponent(I).SigHead1FYellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(2), SignalComponent(I).SigHead1FRed, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                            SignalComponent(I).TempCodeStationBitBuild(Aspects(3), SignalComponent(I).SigHead2Yellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(4), SignalComponent(I).SigHead2FYellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(5), SignalComponent(I).SigHead2FRed, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                            SignalComponent(I).TempCodeStationBitBuild(Aspects(6), SignalComponent(I).SigHead3Yellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(7), SignalComponent(I).SigHead3FYellow, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SignalComponent(I).TempCodeStationBitBuild(Aspects(8), SignalComponent(I).SigHead3FRed, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            If Aspects.Length >= 21 Then
                                SignalComponent(I).TempCodeStationBitBuild(Aspects(13), SignalComponent(I).SigHead2Green, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                                SignalComponent(I).TempCodeStationBitBuild(Aspects(17), SignalComponent(I).SigHead3Green, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                                SignalComponent(I).TempCodeStationBitBuild(Aspects(14), SignalComponent(I).SigHead2Lunar, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                                SignalComponent(I).TempCodeStationBitBuild(Aspects(18), SignalComponent(I).SigHead3Lunar, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            End If
                        End If
                    End If
                End If

                If dr.FieldCount >= 52 Then
                    If Not dr.IsDBNull(51) Then
                        SignalComponent(I).TempCodeStationBitBuild(dr.GetString(51), SignalComponent(I).FleetControl, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                    End If
                End If
                If dr.FieldCount >= 53 AndAlso
                    Not dr.IsDBNull(52) Then
                    SignalComponent(I).Notes = dr.GetString(52)
                End If
                If dr.FieldCount >= 54 Then
                    If Not dr.IsDBNull(53) Then
                        SignalComponent(I).HoldingSignal = dr.GetBoolean(53)
                    End If
                End If

                If dr.FieldCount >= 59 Then
                    If Not dr.IsDBNull(54) Then
                        SignalComponent(I).SignalClearAheadCount = dr.GetByte(54)
                    End If
                    If Not dr.IsDBNull(55) Then
                        SignalComponent(I).AllowAutomaticMode = dr.GetBoolean(55)
                    End If
                    If Not dr.IsDBNull(56) Then
                        SignalComponent(I).AutomaticModeOn = dr.GetBoolean(56)
                    End If
                    If Not dr.IsDBNull(57) Then
                        SignalComponent(I).IndicateOnlyWithControlSetEnabled = dr.GetBoolean(57)
                    End If
                    If Not dr.IsDBNull(58) Then
                        SignalComponent(I).SignalControlled = dr.GetBoolean(58)
                    End If
                End If


                If dr.FieldCount >= 61 Then
                    If Not dr.IsDBNull(59) Then
                        SignalComponent(I).CallOnAllowedWithoutOpposingTraffic = dr.GetBoolean(59)
                    End If
                    If Not dr.IsDBNull(60) Then
                        SignalComponent(I).CallOnSendSignalClearBit = dr.GetBoolean(60)
                    End If
                End If

                If dr.FieldCount >= 63 Then
                    If Not dr.IsDBNull(62) Then
                        SignalComponent(I).myToolTip = dr.GetString(62)
                    End If
                End If
                If dr.FieldCount >= 82 Then
                    If Not dr.IsDBNull(79) Then
                        SignalComponent(I).JointControlAuthorizationOfficeMode = dr.GetBoolean(79)
                    End If
                End If
                If dr.FieldCount >= 66 Then
                    If Not dr.IsDBNull(63) Then
                        SignalComponent(I).JointControlAuthorizationEnabled = dr.GetBoolean(63)
                        If SignalComponent(I).JointControlAuthorizationEnabled AndAlso Not SignalComponent(I).JointControlAuthorizationOfficeMode Then
                            If Not dr.IsDBNull(64) Then
                                SignalComponent(I).TempCodeStationBitBuild(dr.GetString(64), SignalComponent(I).JointControlAuthorizationIndBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            End If
                            If Not dr.IsDBNull(65) Then
                                Dim bits() As String = dr.GetString(65).Split("|")
                                If bits.Length >= 2 Then
                                    SignalComponent(I).TempCodeStationBitBuild(bits(0), SignalComponent(I).JointControlAuthorizationCtlBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                    If Trim(bits(1)) <> "" Then
                                        SignalComponent(I).TempCodeStationBitBuild(bits(1), SignalComponent(I).JointControlAuthorizationCancelBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                        If Not IsNothing(SignalComponent(I).JointControlAuthorizationCancelBit) Then
                                            SignalComponent(I).JointControlAuthorizationCancelBit.BitState = True
                                        End If
                                    End If
                                Else
                                    SignalComponent(I).TempCodeStationBitBuild(bits(0), SignalComponent(I).JointControlAuthorizationCtlBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                End If

                            End If
                        End If
                    End If
                End If
                If dr.FieldCount >= 70 Then
                    If Not dr.IsDBNull(66) Then
                        SignalComponent(I).NearSideSignalEnabled = dr.GetBoolean(66)
                        If SignalComponent(I).NearSideSignalEnabled Then
                            If Not dr.IsDBNull(67) Then
                                SignalComponent(I).NearSideSignalOn = dr.GetBoolean(67)
                            End If
                            If Not dr.IsDBNull(68) Then
                                SignalComponent(I).TempCodeStationBitBuild(dr.GetString(68), SignalComponent(I).NearSideSignalCtlBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                            If Not dr.IsDBNull(69) Then
                                SignalComponent(I).TempCodeStationBitBuild(dr.GetString(69), SignalComponent(I).NearSideSignalIndBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            End If
                        End If
                    End If
                End If
                If dr.FieldCount >= 74 Then
                    If Not dr.IsDBNull(70) Then
                        SignalComponent(I).myMilePost = dr.GetDouble(70)
                    End If
                    If Not dr.IsDBNull(71) Then
                        SignalComponent(I).myMilePostSuffix = dr.GetString(71)
                    End If
                    If Not dr.IsDBNull(72) Then
                        SignalComponent(I).myMilePostPrefix = dr.GetString(72)
                    End If
                    If Not dr.IsDBNull(73) Then
                        'AdditionalReferenceUID
                    End If
                End If

                If dr.FieldCount >= 75 Then
                    If Not dr.IsDBNull(74) Then
                        If optEnableCallOnLatching Then
                            SignalComponent(I).CallOnControlled = dr.GetBoolean(74)
                            If SignalComponent(I).CallOnControlled Then
                                SignalComponent(I).SignalCalledOn = True
                                SignalComponent(I).CallOnControl.BitState = True
                            End If
                        End If
                    End If
                End If

                If dr.FieldCount >= 79 Then
                    If Not dr.IsDBNull(75) Then
                        SignalComponent(I).PublishStatusToPTC = dr.GetBoolean(75)
                    End If
                    If Not dr.IsDBNull(76) Then
                        SignalComponent(I).DevicePTCUID = dr.GetString(76)
                    End If
                    If Not dr.IsDBNull(77) Then
                        SignalComponent(I).AutomaticSignal = dr.GetBoolean(77)
                    End If
                    If Not dr.IsDBNull(78) Then
                        SignalComponent(I).DisableTrafficProjection = dr.GetBoolean(78)
                    End If
                End If

                If optEnableGeoFenceOperations Then
                    If Not dr.IsDBNull(52) Then
                        SignalComponent(I).theGeoFenceInformation = dr.GetString(52)
                    End If
                End If

                If dr.FieldCount >= 85 Then
                    If Not dr.IsDBNull(82) Then
                        SignalComponent(I).EnableDynamicSignalRouteControlBit = dr.GetBoolean(82)
                    End If
                    If SignalComponent(I).EnableDynamicSignalRouteControlBit Then
                        SignalComponent(I).DynamicSignalRouteControlBitList = New ArrayList
                        If Not dr.IsDBNull(83) Then
                            SignalComponent(I).DynamicSignalRouteControlBitString = dr.GetString(83)
                            Dim Bitmaps() As String = SignalComponent(I).DynamicSignalRouteControlBitString.Split("|")
                            Dim bitmap As String
                            For Each bitmap In Bitmaps
                                If Trim(bitmap) <> "" Then
                                    Dim bm As New TMDS.CodeServerClient.BitPosition
                                    SignalComponent(I).TempCodeStationBitBuildSpecial(bitmap, bm, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                    SignalComponent(I).DynamicSignalRouteControlBitList.Add(bm)
                                End If
                            Next
                        End If
                    End If
                    If Not dr.IsDBNull(84) Then
                        SignalComponent(I).EnablePulseStopControl = dr.GetBoolean(84)
                    End If

                End If
                If dr.FieldCount >= 86 Then
                    If SignalComponent(I).EnableDynamicSignalRouteControlBit Then
                        SignalComponent(I).DynamicSignalRouteCallBitList = New ArrayList
                        If Not dr.IsDBNull(85) Then
                            SignalComponent(I).DynamicSignalRouteCallOnBitString = dr.GetString(85)
                            Dim Bitmaps() As String = SignalComponent(I).DynamicSignalRouteCallOnBitString.Split("|")
                            Dim bitmap As String
                            For Each bitmap In Bitmaps
                                If Trim(bitmap) <> "" Then
                                    Dim bm As New TMDS.CodeServerClient.BitPosition
                                    SignalComponent(I).TempCodeStationBitBuildSpecial(bitmap, bm, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                    SignalComponent(I).DynamicSignalRouteCallBitList.Add(bm)
                                End If
                            Next
                        End If
                    End If
                End If


                If dr.FieldCount >= 92 Then
                    If Not dr.IsDBNull(87) Then
                        SignalComponent(I).PTC_DeviceName = dr.GetString(87)
                    End If

                    If Not dr.IsDBNull(88) Then
                        SignalComponent(I).PTC_ProperName = dr.GetString(88)
                    End If

                    If Not dr.IsDBNull(89) Then
                        SignalComponent(I).PTC_WIUStatusIndex = dr.GetString(89)
                    End If

                    If Not dr.IsDBNull(90) Then
                        SignalComponent(I).PTC_DeviceBlockID = dr.GetString(90)
                    End If

                    If Not dr.IsDBNull(91) Then
                        SignalComponent(I).PTC_Enable_PSS = dr.GetBoolean(91)

                    End If
                End If

                If dr.FieldCount >= 93 Then
                    If Not dr.IsDBNull(92) Then
                        SignalComponent(I).SupressSignalStopWithOutOS = dr.GetBoolean(92)
                        If SignalComponent(I).SupressSignalStopWithOutOS Then
                            SignalComponent(I).IndicationSuppressTimer = New System.Timers.Timer
                            SignalComponent(I).IndicationSuppressTimer.Interval = 20000
                            AddHandler SignalComponent(I).IndicationSuppressTimer.Elapsed, AddressOf SignalComponent(I).IndicationSuppressElapsedEvent
                        End If
                    End If
                End If

                If dr.FieldCount >= 95 Then
                    If Not dr.IsDBNull(93) Then SignalComponent(I).DisableEntryExitFunction = dr.GetBoolean(93)
                    If Not dr.IsDBNull(94) Then SignalComponent(I).DisableStoreRouteFunction = dr.GetBoolean(94)
                End If



                If optEnableHideAutomaticSignalsOnLoad Then
                    If SignalComponent(I).AutomaticSignal Then
                        gSignalAutomaticCollection.Add(SignalComponent(I))
                    End If
                End If
                '

                AddHandler SignalComponent(I).MouseHover, AddressOf SignalComponent(I).Signal_MouseHover

                SignalComponent(I).Index = I
                SignalComponent(I).TerritoryID = TerritoryID
                SignalComponent(I).SignalStatus = SignalComponents.Signal.BulbStatus.StopStatus
                'Add the object reference to the Components Collection
                gComponentsCollection.Add(SignalComponent(I))
                gTerritoryComponentsCollection.Add(SignalComponent(I))
                SignalCollection.Add(SignalComponent(I))

            End While
            dr.Close()
            'dbconn.Close()
            'dbconn = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try
    End Function

    Private Function LoadControlPoints(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try

            '//Loading Station Components
            SystemEvent("LOADING CONTROL POINTS...")
            gSplashScreenString = "LOADING CONTROL POINTS FOR " & TerrName
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(gStationSelectQuery, dbconn)
            dr = dbcomm.ExecuteReader()
            Dim cp As ControlPoint
            While dr.Read()
                Application.DoEvents()
                cp = New ControlPoint()

                cp.GUID = dr.GetInt32(0)
                cp.RUID = dr.GetInt32(35)

                cp.StationNumber = dr.GetInt32(1)
                cp.Name = dr.GetString(2)
                cp.ComponentType = dr.GetString(3)
                cp.LX = dr.GetInt16(4)
                cp.LY = dr.GetInt16(5)
                cp.RX = dr.GetInt16(6)
                cp.RY = dr.GetInt16(7)
                cp.LeftLink = dr.GetInt32(8)
                cp.RightLink = dr.GetInt32(9)
                cp.LeftAltLink = dr.GetInt32(10)
                cp.RightAltLink = dr.GetInt32(11)
                cp.CodelineNumber = dr.GetInt32(14)
                cp.AuthorityType = dr.GetString(32)
                cp.SubdivisionID = dr.GetInt32(34)

                If Not dr.IsDBNull(35) Then
                    cp.UniqueID = dr.GetInt32(35)
                End If
                If Not dr.IsDBNull(37) Then
                    Dim USTR As String = dr.GetValue(37)
                    Select Case USTR
                        Case "NULL", "NOTHING"
                            cp.UniqueName = String.Empty
                        Case Else
                            cp.UniqueName = dr.GetString(37)
                    End Select

                End If

                'MOK
                cp.TempCodeStationBitBuild(dr.GetString(15), cp.Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                ''POK
                cp.TempCodeStationBitBuild(dr.GetString(16), cp.Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                cp.TempCodeStationBitBuild(dr.GetString(21), cp.Ind(6), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                ''LOK
                cp.TempCodeStationBitBuild(dr.GetString(17), cp.Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                If Not IsNothing(cp.Ind(2)) Then
                    cp.Ind(2).IsAlarmType = True
                    cp.Ind(2).LoadAlarmInformation()
                End If

                ''DOK
                cp.TempCodeStationBitBuild(dr.GetString(18), cp.Ind(3), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                'GEN
                cp.TempCodeStationBitBuild(dr.GetString(20), cp.Ind(5), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                'SMZ SMK
                cp.TempCodeStationBitBuild(dr.GetString(24), cp.Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                cp.TempCodeStationBitBuild(dr.GetString(19), cp.Ind(4), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                cp.TempCodeStationBitBuild(dr.GetString(22), cp.Ind(7), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                cp.TempCodeStationBitBuild(dr.GetString(28), cp.Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                If dr.FieldCount >= 65 Then
                    'TEMPORAL MODE
                    If Not dr.IsDBNull(23) Then cp.TempCodeStationBitBuild(dr.GetString(23), cp.Ind(8), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    If Not dr.IsDBNull(25) Then cp.TempCodeStationBitBuild(dr.GetString(25), cp.Ind(9), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    If Not dr.IsDBNull(30) Then cp.TempCodeStationBitBuild(dr.GetString(30), cp.Ctl(3), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                    If Not dr.IsDBNull(59) Then cp.TempCodeStationBitBuild(dr.GetString(59), cp.Ctl(4), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                End If

                'MCZ
                cp.TempCodeStationBitBuild(dr.GetString(26), cp.Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                'cp.Graphical = g
                cp.SimulateMode = gSimulationMode
                cp.Initialized = gSimulationMode
                cp.AllStationsInitialized = gSimulationMode

                If cp.AuthorityType = "CTC" Then
                    'Version 227
                    'Set fail state to true on the initial load until a code server status is received
                    If Not cp.SimulateMode Then cp.FailState = True
                    cp.CodeServerList = TMDS.GlobalDeclarations.CodeServers
                    cp.CodeStationList = TMDS.GlobalDeclarations.CodeStations
                    cp.CodeBitList = TMDS.GlobalDeclarations.CtlIndBits
                End If


                If dr.FieldCount >= 41 Then
                    If Not dr.IsDBNull(39) Then
                        cp.GraphicalGroupID = dr.GetByte(39)
                    End If
                    If Not dr.IsDBNull(40) Then
                        cp.ScreenRegionID = dr.GetByte(40)
                    End If
                End If

                Try
                    If dr.FieldCount >= 48 Then
                        If Not dr.IsDBNull(47) Then
                            cp.SlotSMControlOnIndication = dr.GetBoolean(47)
                        End If
                    End If
                Catch
                End Try

                Try
                    If dr.FieldCount >= 49 Then
                        If Not dr.IsDBNull(48) Then
                            cp.AllowStatusPublishing = dr.GetBoolean(48)
                        End If
                    End If
                Catch
                End Try

                If dr.FieldCount >= 51 Then
                    If Not dr.IsDBNull(49) Then
                        cp.EnableResendControlLogic = dr.GetBoolean(49)
                    End If
                    If Not dr.IsDBNull(50) Then
                        If IsNumeric(dr.GetByte(50)) Then
                            cp.ResendControlTime = dr.GetByte(50) * 1000
                            If cp.ResendControlTime = 0 Then
                                cp.ResendControlTime = 60000
                            End If
                        End If
                    End If
                End If

                If dr.FieldCount >= 53 Then
                    If Not dr.IsDBNull(51) Then
                        cp.AllowAutomaticMode = dr.GetBoolean(51)
                    End If
                    If Not dr.IsDBNull(52) Then
                        cp.AutomaticModeOn = dr.GetBoolean(52)
                    End If
                End If
                If dr.FieldCount >= 55 Then
                    If Not dr.IsDBNull(53) And Not dr.IsDBNull(54) Then
                        cp.AllowSnowMelterTimer = dr.GetBoolean(53)
                        cp.SnowMelterTime = dr.GetInt32(54)
                        If optEnableCustomSnowMelterTimer Then
                            cp.AttachSnMZTimer()
                        Else
                            If cp.AllowSnowMelterTimer Then
                                cp.AttachSnMZTimer()
                            End If

                        End If
                    End If
                End If


                If optEnableAllowForceOperationsToTWC Then
                    If dr.FieldCount >= 56 Then
                        If Not dr.IsDBNull(55) Then
                            cp.ForceOperationsToTWC = dr.GetBoolean(55)
                        End If
                    End If
                End If

                If optEnableAllowControlPointCorrelation Then
                    If dr.FieldCount >= 58 Then
                        If Not dr.IsDBNull(56) Then
                            cp.AllowControlPointCorrelation = dr.GetBoolean(56)
                        End If
                        If Not dr.IsDBNull(57) Then
                            cp.CorrelatingControlPointReferences = dr.GetString(57)
                            cp.CorrelatingControlPoints = New ArrayList
                        End If
                    End If
                End If

                If optAllowTemporalMode Then
                    If dr.FieldCount >= 59 Then
                        If Not dr.IsDBNull(58) Then
                            cp.AllowTemporalModeOperations = dr.GetBoolean(58)
                            If cp.AllowTemporalModeOperations Then
                                If Not IsNothing(gTemporalControlPointList) Then
                                    gTemporalControlPointList.Add(cp)
                                End If
                            End If
                        End If
                    End If
                    If dr.FieldCount >= 66 Then
                        If Not dr.IsDBNull(65) Then
                            cp.AllowPointOfConflictOperations = dr.GetBoolean(65)
                            If cp.AllowPointOfConflictOperations Then
                                If Not IsNothing(gTemporalPointOfConflictList) Then
                                    gTemporalPointOfConflictList.Add(cp)
                                End If

                            End If
                        End If
                    End If
                End If

                If dr.FieldCount >= 70 Then
                    If Not dr.IsDBNull(66) Then
                        cp.myMilePost = dr.GetDouble(66)
                    End If
                    If Not dr.IsDBNull(67) Then
                        cp.myMilePostSuffix = dr.GetString(67)
                    End If
                    If Not dr.IsDBNull(68) Then
                        cp.myMilePostPrefix = dr.GetString(68)
                    End If
                    If Not dr.IsDBNull(69) Then
                        'AdditionalReferenceUID
                    End If
                End If
                If dr.FieldCount >= 71 Then
                    If Not dr.IsDBNull(70) Then
                        cp.MakeInvisible = dr.GetBoolean(70)
                    End If
                End If

                If optAllowDisableAutomaticSignalSlotting Then
                    If dr.FieldCount >= 72 Then
                        If Not dr.IsDBNull(71) Then
                            cp.DisableAutomaticSignalSlotting = dr.GetBoolean(71)
                        End If
                    End If
                End If

                If optEnableCustomSnowMelterTimer Then
                    If dr.FieldCount >= 73 Then
                        If Not dr.IsDBNull(72) Then
                            Dim SNStopTime As Date = dr.GetDateTime(72)
                            cp.CustomSnowMelterEndTime = SNStopTime
                        End If
                    End If
                End If
                If optEnableTunnelAutomaticOperations Then
                    If dr.FieldCount >= 75 Then
                        If Not dr.IsDBNull(73) Then
                            cp.TunnelAutomaticMode = dr.GetBoolean(73)
                            cp.TempCodeStationBitBuild(dr.GetString(60), cp.Ctl(5), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            cp.TempCodeStationBitBuild(dr.GetString(61), cp.Ctl(6), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        End If
                        If Not dr.IsDBNull(74) Then
                            cp.TunnelAutomaticControlled = dr.GetBoolean(74)
                        End If
                    End If
                End If

                If optEnableTractionPowerManagementSystem Then
                    If dr.FieldCount >= 79 Then
                        If Not dr.IsDBNull(75) Then
                            cp.ScadaTractionPowerControlPointMode = dr.GetBoolean(75)
                        End If
                        If Not dr.IsDBNull(76) Then
                            cp.ScadaCommunicationControlPointMode = dr.GetBoolean(76)
                        End If
                        If Not dr.IsDBNull(77) Then
                            cp.ScadaNonReportingModeControlled = dr.GetBoolean(77)
                        End If
                        If Not dr.IsDBNull(78) Then
                            cp.ScadaAlarmReportingIndicationBit = dr.GetString(78)
                        End If
                        If cp.ScadaTractionPowerControlPointMode Or cp.ScadaCommunicationControlPointMode Then
                            cp.Visible = False
                        End If
                    End If
                End If

                If dr.FieldCount >= 80 Then
                    If Not dr.IsDBNull(79) Then
                        cp.DevicePTCUID = dr.GetString(79)
                    End If
                End If

                If dr.FieldCount >= 82 Then
                    If Not dr.IsDBNull(80) Then
                        cp.ControllingWorkstation = dr.GetString(80)
                    End If
                    If optEnableShadowMode Then
                        If Not dr.IsDBNull(81) Then
                            cp.ControllingWorkStationIPString = dr.GetString(81)
                        End If
                        cp.IsShadowMode = True
                    End If
                End If

                If dr.FieldCount >= 88 Then
                    If Not dr.IsDBNull(82) Then
                        cp.PTC_SiteName = dr.GetString(82)

                    End If

                    If Not dr.IsDBNull(83) Then
                        cp.PTC_SiteAddress = dr.GetString(83)

                    End If
                    If Not dr.IsDBNull(84) Then
                        cp.PTC_SecurityKey = dr.GetString(84)
                    End If

                    If Not dr.IsDBNull(85) Then
                        cp.PTC_WIUID = dr.GetString(85)

                    End If
                    If Not dr.IsDBNull(86) Then
                        cp.PTC_RailroadSCAC = dr.GetString(86)

                    End If
                    If Not dr.IsDBNull(87) Then
                        cp.PTC_UID_KEY = dr.GetString(87)
                    End If
                End If


                cp.TerritoryID = TerritoryID

                gTerritoryComponentsCollection.Add(cp)
                gComponentsCollection.Add(cp)
                gControlPointCollection.Add(cp)

                AddHandler cp.MouseDown, AddressOf cp.ControlPoint_MouseDown
                AddHandler cp.MouseEnter, AddressOf cp.ControlPoint_MouseEnter
                AddHandler cp.MouseLeave, AddressOf cp.ControlPoint_MouseLeave
                AddHandler cp.MouseHover, AddressOf cp.ControlPoint_MouseHover


                'reference all the componenets to each control point
                cp.AssignComponentsToControlPoint(gTerritoryComponentsCollection)
                cp.BuildContextMenu()

                cp.myTerritory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(TerritoryID)

                If cp.AllowAutomaticMode Then
                    cp.InitializeAutoRouting()
                End If
                If cp.ForceOperationsToTWC Then
                    cp.InitializeForceOperationsChangeToTWC()
                End If

                If optEnableTunnelAutomaticOperations And cp.TunnelAutomaticMode Then
                    cp.ApplyTunnelAutomaticMode()
                End If

                If cp.ControllingWorkstation.Contains("MAINTENANCE MODE") Then
                    cp.MaintenanceMode = True
                    cp.ApplyMonitorMode()
                    cp.UpdateStatus()
                    For Each comp As Object In cp.AssignedComponenets
                        If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            SystemEvent("LoadControlPoints:Switch=" & comp.Name & ":DoMonitorMode=" & comp.DoMonitorMode)
                        End If
                        If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                            SystemEvent("LoadControlPoints:Signal=" & comp.Name & ":DoMonitorMode=" & comp.DoMonitorMode)
                        End If
                    Next
                End If
            End While

            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        Finally
            If Not dbconn Is Nothing Then
                If dbconn.State = ConnectionState.Open Then dbconn.Close()
                dbconn.Dispose()
                dbconn = Nothing
            End If
            If Not dr Is Nothing Then
                dr.Close()
                dr = Nothing
            End If
            If Not dbcomm Is Nothing Then
                dbcomm.Dispose()
                dbcomm = Nothing
            End If
        End Try
    End Function

    Public Sub LoadComponents(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE")
        Dim StartTime As Date = Now   ' Starting date/time.
        Dim DatabaseCommand As OleDb.OleDbCommand = Nothing
        Dim DataReader As OleDb.OleDbDataReader = Nothing
        Dim I As Integer = TrackComponent.GetUpperBound(0)
        Dim RunLength As System.TimeSpan = Now.Subtract(StartTime)
        Dim Millisecs As Integer = RunLength.Milliseconds

        Try
            ''//Load Track Components
            SystemEvent("LOADING TRACKS...")
            gSplashScreenString = "LOADING TRACKS FOR " & TerrName
            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic Is Nothing Then
                TMDS.GlobalDeclarations.gDatabaseConnectionStatic = New OleDbConnection(gConnectionStringStatic)
            End If


            DatabaseCommand = New OleDbCommand(gTrackSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic.State <> ConnectionState.Open Then TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Open()
            DataReader = DatabaseCommand.ExecuteReader()
            While DataReader.Read()
                Application.DoEvents()
                I = I + 1
                ReDim Preserve TrackComponent(I)
                TrackComponent(I) = New Track()

                If optEnableAllowForceOperationsToTWC Then
                    If DataReader.FieldCount >= 89 Then
                        If Not DataReader.IsDBNull(88) Then
                            TrackComponent(I).ForceOperationsToTWC = DataReader.GetBoolean(88)
                        End If
                    End If
                End If

                TrackComponent(I).TUID = DataReader.GetInt32(44)

                TrackComponent(I).GUID = DataReader.GetInt32(0)

                If TrackComponent(I).GUID = "111228" Then
                    Beep()
                End If


                TrackComponent(I).StationNumber = DataReader.GetInt32(1)
                TrackComponent(I).Name = DataReader.GetString(2)
                TrackComponent(I).ComponentType = DataReader.GetString(3)
                TrackComponent(I).LX = DataReader.GetInt16(4)
                TrackComponent(I).LY = DataReader.GetInt16(5)
                TrackComponent(I).RX = DataReader.GetInt16(6)
                TrackComponent(I).RY = DataReader.GetInt16(7)
                TrackComponent(I).LeftLink = DataReader.GetInt32(8)
                TrackComponent(I).RightLink = DataReader.GetInt32(9)
                TrackComponent(I).LeftAltLink = DataReader.GetInt32(10)
                TrackComponent(I).RightAltLink = DataReader.GetInt32(11)
                TrackComponent(I).AltLinkOne = DataReader.GetInt32(12)
                TrackComponent(I).AltLinkTwo = DataReader.GetInt32(13)

                If TrackComponent(I).AltLinkOne <> 0 Or TrackComponent(I).AltLinkTwo <> 0 Then
                    TMDS.GlobalDeclarations.gTracksNeedingLinks.Add(TrackComponent(I))
                    TrackComponent(I).NeedsLinking = True
                End If
                TrackComponent(I).CodelineNumber = DataReader.GetInt32(14)

                TrackComponent(I).LeftLimitMilePost = DataReader.GetDouble(23)
                TrackComponent(I).RightLimitMilePost = DataReader.GetDouble(24)

                Try
                    If TrackComponent(I).LeftLimitMilePost > TrackComponent(I).RightLimitMilePost Then
                        TrackComponent(I).MilePostHigh = TrackComponent(I).LeftLimitMilePost
                        TrackComponent(I).MilePostLow = TrackComponent(I).RightLimitMilePost
                    Else
                        TrackComponent(I).MilePostLow = TrackComponent(I).LeftLimitMilePost
                        TrackComponent(I).MilePostHigh = TrackComponent(I).RightLimitMilePost
                    End If
                Catch ex As Exception
                End Try


                If TrackComponent(I).ForceOperationsToTWC Then
                    TrackComponent(I).AuthorityType = "TWC"
                Else
                    TrackComponent(I).AuthorityType = DataReader.GetString(25)
                End If

                'added to allow 
                Select Case Mid(TrackComponent(I).ComponentType, 2)
                    Case "R"
                        TrackComponent(I).ItemSevenAllowedOnRightLimits = True
                        TrackComponent(I).ComponentType = "T"
                    Case "L"
                        TrackComponent(I).ItemSevenAllowedOnLeftLimits = True
                        TrackComponent(I).ComponentType = "T"
                    Case "&"
                        TrackComponent(I).TrackIndicationANDED = True
                        TrackComponent(I).TrackIndicationORED = False
                    Case "+"
                        TrackComponent(I).TrackIndicationANDED = False
                        TrackComponent(I).TrackIndicationORED = True
                    Case "*"
                        TrackComponent(I).TrackIndicationANDED = False
                        TrackComponent(I).TrackIndicationORED = False
                        TrackComponent(I).TrackIndicationANDEDORED = False
                        TrackComponent(I).TrackIndicationOREDANDED = True
                    Case "#"
                        TrackComponent(I).TrackIndicationANDED = False
                        TrackComponent(I).TrackIndicationORED = False
                        TrackComponent(I).TrackIndicationANDEDORED = True
                        TrackComponent(I).TrackIndicationOREDANDED = False
                End Select

                'simulation mode
                TrackComponent(I).SimulateMode = gSimulationMode

                TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(15), TrackComponent(I).Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                If Not TrackComponent(I).ForceOperationsToTWC Then
                    If Mid(TrackComponent(I).AuthorityType, 1, 3) = "CTC" Or TrackComponent(I).AuthorityType = "OTHER" Or Mid(TrackComponent(I).AuthorityType, 1, 3) = "TWC" Then
                        gCTCTerritoryFound = True
                        'TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(15), TrackComponent(I).Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(16), TrackComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(17), TrackComponent(I).Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(18), TrackComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(19), TrackComponent(I).Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(20), TrackComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        'Version 227
                        'Set fail state to true on the initial load until a code server status is received
                    End If
                End If

                If TrackComponent(I).AuthorityType = "TWC-BABS" Then
                    TrackComponent(I).AuthorityType = Mid(TrackComponent(I).AuthorityType, 1, 3)
                    TrackComponent(I).BiDirectionalABS = True
                ElseIf TrackComponent(I).AuthorityType = "TWC-ABSW" Then
                    TrackComponent(I).AuthorityType = Mid(TrackComponent(I).AuthorityType, 1, 3)
                    TrackComponent(I).SingleABSWest = True
                ElseIf TrackComponent(I).AuthorityType = "TWC-ABSE" Then
                    TrackComponent(I).AuthorityType = Mid(TrackComponent(I).AuthorityType, 1, 3)
                    TrackComponent(I).SingleABSEast = True
                End If
                If Not TrackComponent(I).ForceOperationsToTWC Then
                    If Not TrackComponent(I).Ind(0) Is Nothing Then
                        TrackComponent(I).Indicating = True
                    Else
                        TrackComponent(I).FailState = False
                    End If

                    If Not TrackComponent(I).SimulateMode And TrackComponent(I).Indicating Then
                        TrackComponent(I).FailState = True
                    End If
                Else
                    TrackComponent(I).FailState = False
                End If

                TrackComponent(I).TrackName = DataReader.GetString(26)
                If TrackComponent(I).ForceOperationsToTWC Then
                    If InStr(TrackComponent(I).TrackName, "MAIN") = 0 Then
                        TrackComponent(I).AuthorityType = "NON-TWC"
                    End If
                End If

                TrackComponent(I).LeftLimitName = DataReader.GetString(27)
                TrackComponent(I).RightLimitName = DataReader.GetString(28)
                TrackComponent(I).SubdivisionID = DataReader.GetInt32(30)
                TrackComponent(I).SlottingTrack = DataReader.GetBoolean(31)
                TrackComponent(I).TrackLength = DataReader.GetInt32(32)
                TrackComponent(I).AllowBlocking = DataReader.GetBoolean(33)

                If Not TrackComponent(I).ForceOperationsToTWC Then
                    TrackComponent(I).AllowFieldBlocking = DataReader.GetBoolean(34)
                    TrackComponent(I).AllowFoulTime = DataReader.GetBoolean(35)
                    TrackComponent(I).AllowTrackPermit = DataReader.GetBoolean(36)
                    TrackComponent(I).AllowTrackTime = DataReader.GetBoolean(37)
                    TrackComponent(I).AllowTraffic = DataReader.GetBoolean(38)

                    If TrackComponent(I).AllowFieldBlocking Then
                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(40), TrackComponent(I).FBK, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        Dim BlockControl1() As String = DataReader.GetString(39).Split("|")
                        If BlockControl1.Length > 1 Then
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl1(0), TrackComponent(I).FBZ, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl1(1), TrackComponent(I).FBOFFZ, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        Else
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl1(0), TrackComponent(I).FBZ, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        End If

                        Dim BlockControl2() As String = DataReader.GetString(49).Split("|")
                        If BlockControl2.Length > 1 Then
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl2(0), TrackComponent(I).FBZ2, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl2(1), TrackComponent(I).FBOFFZ2, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        Else
                            TrackComponent(I).TempCodeStationBitBuild(BlockControl2(0), TrackComponent(I).FBZ2, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        End If

                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(50), TrackComponent(I).FBK2, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        TrackComponent(I).FieldBlockingControlStatus = TrackComponents.Track.FieldBlockStatus.BlockingOff
                        TrackComponent(I).FieldBlockingIndicationStatus = TrackComponents.Track.FieldBlockStatus.BlockingOff
                        TrackComponent(I).BlockingFlasher = New Timers.Timer()
                        TrackComponent(I).BlockingFlasher.Interval = 400
                        TrackComponent(I).BlockingFlasher.AutoReset = False
                        AddHandler TrackComponent(I).BlockingFlasher.Elapsed, AddressOf TrackComponent(I).FlashFieldBlockEvent
                    End If

                    If Not DataReader.IsDBNull(42) Then
                        TrackComponent(I).AllowTrackLock = DataReader.GetBoolean(42)
                    End If

                    If Not DataReader.IsDBNull(43) Then
                        TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(43), TrackComponent(I).TrackLockBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    End If


                End If

                TrackComponent(I).TimeZone = DataReader.GetString(41)
                If DataReader.FieldCount >= 46 Then
                    TrackComponent(I).OSReporting = DataReader.GetBoolean(45)
                    TrackComponent(I).OriginalOSReporting_APB = DataReader.GetBoolean(45)
                End If

                If DataReader.FieldCount >= 48 Then
                    If Not DataReader.IsDBNull(46) Then
                        TrackComponent(I).GraphicalGroupID = DataReader.GetValue(46)
                    End If
                    If Not DataReader.IsDBNull(47) Then
                        TrackComponent(I).ScreenRegionID = DataReader.GetValue(47)
                    End If
                End If

                If DataReader.FieldCount >= 49 Then
                    If Not DataReader.IsDBNull(48) Then
                        TrackComponent(I).LossOfShunt = DataReader.GetValue(48)
                        If CInt(TrackComponent(I).LossOfShunt) <> 0 Then
                            TrackComponent(I).LossOfShuntTimer.Interval = CInt(TrackComponent(I).LossOfShunt) * 1000
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 52 Then
                    If Not DataReader.IsDBNull(51) Then
                        TrackComponent(I).ReleaseTrainIDTrack = DataReader.GetBoolean(51)
                    End If
                End If

                If DataReader.FieldCount >= 53 Then
                    If Not DataReader.IsDBNull(52) Then
                        TrackComponent(I).DisallowTNTOnRegionSplit = DataReader.GetBoolean(52)
                    End If
                End If

                If DataReader.FieldCount >= 54 Then
                    If Not DataReader.IsDBNull(53) Then
                        TrackComponent(I).AllowBlueFlag = DataReader.GetBoolean(53)
                    End If
                End If

                'If DataReader.FieldCount >= 55 Then
                '    If Not DataReader.IsDBNull(54) Then
                '        TrackComponent(I).AllowOCS = DataReader.GetBoolean(54)
                '    End If
                'End If

                If optInitializeTrackSpeeds Then
                    If DataReader.FieldCount >= 56 Then
                        If Not DataReader.IsDBNull(55) Then
                            TrackComponent(I).TrackSpeedString = DataReader.GetString(55)
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 57 Then
                    If Not DataReader.IsDBNull(56) Then
                        TrackComponent(I).PassRedSignalAlertOff = DataReader.GetBoolean(56)
                    End If
                End If

                If DataReader.FieldCount >= 59 Then
                    If Not DataReader.IsDBNull(58) Then
                        TrackComponent(I).SlotFieldTraffic = DataReader.GetBoolean(58)
                    End If
                End If

                If DataReader.FieldCount >= 60 Then
                    If Not DataReader.IsDBNull(59) Then
                        TrackComponent(I).PTCBlockID = DataReader.GetInt32(59)
                    End If
                End If

                If DataReader.FieldCount >= 61 Then
                    If Not DataReader.IsDBNull(60) Then
                        TrackComponent(I).RefuseOpposingAuthoritiesFromLeft = DataReader.GetBoolean(60)
                    End If
                End If

                If DataReader.FieldCount >= 62 Then
                    If Not DataReader.IsDBNull(61) Then
                        TrackComponent(I).RefuseOpposingAuthoritiesFromRight = DataReader.GetBoolean(61)
                    End If
                End If

                If DataReader.FieldCount >= 63 Then
                    If Not DataReader.IsDBNull(62) Then
                        TrackComponent(I).AllowOpposingAuthorityCreationToLeft = DataReader.GetBoolean(62)
                    End If
                End If

                If DataReader.FieldCount >= 64 Then
                    If Not DataReader.IsDBNull(63) Then
                        TrackComponent(I).AllowOpposingAuthorityCreationToRight = DataReader.GetBoolean(63)
                    End If
                End If

                If DataReader.FieldCount >= 65 Then
                    If Not DataReader.IsDBNull(64) Then
                        TrackComponent(I).HoldingSignalSlottingTrack = DataReader.GetBoolean(64)
                    End If
                End If

                If DataReader.FieldCount >= 66 Then
                    If Not DataReader.IsDBNull(65) Then
                        TrackComponent(I).HoldingSignalOSLeftBoundReporting = DataReader.GetBoolean(65)
                    End If
                End If

                If DataReader.FieldCount >= 67 Then
                    If Not DataReader.IsDBNull(66) Then
                        TrackComponent(I).HoldingSignalOSRightBoundReporting = DataReader.GetBoolean(66)
                    End If
                End If

                If DataReader.FieldCount >= 69 Then

                    If TrackComponent(I).GUID = "201181" Then
                        Beep()
                    End If

                    If Not DataReader.IsDBNull(67) Then
                        TrackComponent(I).OverrideLeftTrafficCheck = DataReader.GetBoolean(67)
                    End If
                    If Not DataReader.IsDBNull(68) Then
                        TrackComponent(I).OverrideRightTrafficCheck = DataReader.GetBoolean(68)
                    End If
                End If

                If DataReader.FieldCount >= 70 Then
                    If Not DataReader.IsDBNull(69) Then
                        Dim refs() As String = DataReader.GetString(69).Split("|")
                        Dim ref As String
                        For Each ref In refs
                            If IsNumeric(ref) Then
                                If IsNothing(TrackComponent(I).TrackBlockingReferences) Then
                                    TrackComponent(I).TrackBlockingReferences = New ArrayList()
                                End If
                                TrackComponent(I).TrackBlockingReferences.Add(CInt(ref))
                            End If
                        Next
                    End If

                    If DataReader.FieldCount >= 71 Then
                        If Not DataReader.IsDBNull(70) Then
                            TrackComponent(I).MatchModeFieldBlocking = DataReader.GetBoolean(70)
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 72 Then
                    If Not DataReader.IsDBNull(71) Then
                        TrackComponent(I).MilePostSuffix = DataReader.GetString(71)
                        TrackComponent(I).myMilePostSuffix = DataReader.GetString(71)
                        TrackComponent(I).LeftLimitMilePostSuffix = DataReader.GetString(71)
                        TrackComponent(I).RightLimitMilePostSuffix = DataReader.GetString(71)
                    End If
                End If

                If DataReader.FieldCount >= 73 Then
                    If Not DataReader.IsDBNull(72) Then
                        TrackComponent(I).TurnOutType = DataReader.GetBoolean(72)
                    End If
                End If

                If DataReader.FieldCount >= 75 Then
                    If Not DataReader.IsDBNull(73) Then
                        TrackComponent(I).AltLinkThree = DataReader.GetInt32(73)
                    End If
                    If Not DataReader.IsDBNull(74) Then
                        TrackComponent(I).AltLinkFour = DataReader.GetInt32(74)
                    End If
                End If


                If DataReader.FieldCount >= 76 Then
                    If Not DataReader.IsDBNull(75) Then
                        TrackComponent(I).HAZMATAlarming = DataReader.GetBoolean(75)
                    End If
                End If

                If DataReader.FieldCount >= 77 Then
                    If Not DataReader.IsDBNull(76) Then
                        TrackComponent(I).FoulingTrackSPMS = DataReader.GetBoolean(76)
                    End If
                End If

                If DataReader.FieldCount >= 78 Then
                    If Not DataReader.IsDBNull(77) Then
                        TrackComponent(I).RouteName = DataReader.GetString(77)
                    End If
                End If


                If DataReader.FieldCount >= 79 Then
                    If Not DataReader.IsDBNull(78) Then
                        If optEnableRouteBlocking Then
                            TrackComponent(I).RouteBlockingEnabled = DataReader.GetBoolean(78)
                            If TrackComponent(I).RouteBlockingEnabled Then
                                TrackComponent(I).RouteList = New ArrayList
                            End If
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 82 Then
                    If Not DataReader.IsDBNull(79) Then
                        TrackComponent(I).TrackZoneIntegrityMonitoringEnabled = DataReader.GetBoolean(79)
                        If TrackComponent(I).TrackZoneIntegrityMonitoringEnabled Then
                            gTrackIntegrityMonitoringExists = True
                            If Not DataReader.IsDBNull(80) And Not DataReader.IsDBNull(81) Then
                                CheckToCreateTrackZone(DataReader.GetString(81), DataReader.GetInt32(80), TrackComponent(I))
                            End If
                        End If
                    End If
                End If
                If DataReader.FieldCount >= 83 Then
                    If Not DataReader.IsDBNull(82) Then
                        TrackComponent(I).TrackZoneReportTimeOut = DataReader.GetInt32(82)
                    End If
                End If


                If DataReader.FieldCount >= 84 Then
                    If Not DataReader.IsDBNull(83) Then
                        TrackComponent(I).TrackAndTimeConfirmation = DataReader.GetBoolean(83)
                    End If
                End If

                If DataReader.FieldCount >= 85 Then
                    If Not DataReader.IsDBNull(84) Then
                        'TrackComponent(I).sAlternateTrackAbbreviation = DataReader.GetString(84)
                        TrackComponent(I).sAlternateTrackNames = DataReader.GetString(84)
                    End If
                End If

                If optUseTrackAlternateNameForMovementReporting Then
                    If TrackComponent(I).sAlternateTrackNames <> "" Then
                        Dim names() As String = TrackComponent(I).sAlternateTrackNames.Split("|")
                        If names.Length > 0 Then
                            If Trim(names(0)) <> "" Then
                                TrackComponent(I).TrackNameAlternateReportingName = names(0)
                            End If
                        End If
                    End If
                    If TrackComponent(I).TrackNameAlternateReportingName = "" Then
                        TrackComponent(I).TrackNameAlternateReportingName = TrackComponent(I).Name
                    End If
                End If



                If DataReader.FieldCount >= 86 Then
                    If Not DataReader.IsDBNull(85) Then
                        'TrackComponent(I).sAlternateTrackNames = DataReader.GetString(85)
                        TrackComponent(I).sAlternateTrackAbbreviation = DataReader.GetString(85)
                    End If
                End If
                'optEnableEntryExitTWCPoints
                If optEnableMenuPermissionToEnterMainTrack Or optEnableCPPermissionToEnterMainTrack Then
                    If DataReader.FieldCount >= 87 Then
                        If Not DataReader.IsDBNull(86) Then
                            Select Case TrackComponent(I).AuthorityType.Trim.ToUpper
                                Case "SHELF"
                                    If optEnableEntryExitTWCPoints Then
                                        TrackComponent(I).AllowMainTrackEntry = DataReader.GetBoolean(86)
                                    End If
                                Case Else
                                    TrackComponent(I).AllowMainTrackEntry = DataReader.GetBoolean(86)
                            End Select
                            'TrackComponent(I).AllowMainTrackEntry = DataReader.GetBoolean(86)
                        End If
                    End If
                    If TrackComponent(I).AllowMainTrackEntry Then
                        If DataReader.FieldCount >= 88 Then
                            If Not DataReader.IsDBNull(87) Then
                                TrackComponent(I).sMainTrackEntrancePoints = DataReader.GetString(87)
                            End If
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 90 Then
                    If Not DataReader.IsDBNull(89) Then
                        TrackComponent(I).EnableAbsolutePermissiveBlock = DataReader.GetBoolean(89)
                    End If
                End If

                If DataReader.FieldCount >= 91 Then
                    If Not DataReader.IsDBNull(90) Then
                        TrackComponent(I).AbsolutePermissiveBlockPTCUID = DataReader.GetInt32(90)
                    End If
                End If
                If DataReader.FieldCount >= 92 Then
                    If Not DataReader.IsDBNull(91) Then
                        TrackComponent(I).LineSegmentGroupName = DataReader.GetString(91)
                    End If
                End If


                If DataReader.FieldCount >= 96 Then
                    If Not DataReader.IsDBNull(92) Then
                        TrackComponent(I).DerailmentDetectorEnabled = DataReader.GetBoolean(92)
                        If TrackComponent(I).DerailmentDetectorEnabled Then
                            If Not DataReader.IsDBNull(93) Then
                                TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(93), TrackComponent(I).DerailmentDetectorIndBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            End If
                            If Not DataReader.IsDBNull(94) Then
                                TrackComponent(I).TempCodeStationBitBuild(DataReader.GetString(94), TrackComponent(I).DerailmentDetectorCtlBit, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                            If Not DataReader.IsDBNull(95) Then
                                TrackComponent(I).DerailmentDetectorInhibited = DataReader.GetBoolean(95)
                            End If
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 99 Then
                    If Not DataReader.IsDBNull(98) Then
                        TrackComponent(I).IsPositionReportingTrack = DataReader.GetBoolean(98)
                    End If
                End If

                If DataReader.FieldCount >= 102 Then
                    If Not DataReader.IsDBNull(96) Then
                        TrackComponent(I).MilePostPrefix = DataReader.GetString(96)
                        TrackComponent(I).myMilePostPrefix = DataReader.GetString(96)
                        TrackComponent(I).LeftLimitMilePostPrefix = TrackComponent(I).MilePostPrefix
                        TrackComponent(I).RightLimitMilePostPrefix = TrackComponent(I).MilePostPrefix
                    End If
                    If Not DataReader.IsDBNull(99) Then
                        Dim TrackNameAlias As String = DataReader.GetString(99)
                        Select Case Trim(TrackNameAlias)
                            Case "NOTHING", "NONE", ""
                                TrackNameAlias = TrackComponent(I).TrackName
                        End Select
                        TrackComponent(I).TrackNameAlias = TrackNameAlias
                    End If

                    If Not DataReader.IsDBNull(100) Then
                        TrackComponent(I).EnableNonControlledSiding = DataReader.GetBoolean(100)
                    End If
                    If Not DataReader.IsDBNull(101) Then
                        TrackComponent(I).EnableControlledSiding = DataReader.GetBoolean(101)
                    End If

                End If
                If DataReader.FieldCount >= 104 Then
                    If Not DataReader.IsDBNull(102) Then
                        TrackComponent(I).DisallowLeftSignalRequestOnOccupancy = DataReader.GetBoolean(102)
                    End If
                    If Not DataReader.IsDBNull(103) Then
                        TrackComponent(I).DisallowRightSignalRequestOnOccupancy = DataReader.GetBoolean(103)
                    End If
                End If

                If DataReader.FieldCount >= 111 AndAlso optEnableTrackSpeedInformation Then
                    If Not DataReader.IsDBNull(104) Then
                        TrackComponent(I).TrackSpeedAverage = DataReader.GetInt32(104)
                    End If
                    If Not DataReader.IsDBNull(105) Then
                        TrackComponent(I).TrackSpeedMaximum = DataReader.GetInt32(105)
                    End If
                    If Not DataReader.IsDBNull(106) Then
                        TrackComponent(I).TrackElevationAverage = DataReader.GetDouble(106)
                    End If
                    If Not DataReader.IsDBNull(107) Then
                        TrackComponent(I).TrackElevationMaximum = DataReader.GetDouble(107)
                    End If
                    If Not DataReader.IsDBNull(108) Then
                        TrackComponent(I).TrackElevationAscending = DataReader.GetBoolean(108)
                    End If
                    If Not DataReader.IsDBNull(109) Then
                        TrackComponent(I).TrackSpeedMaximumFreight = DataReader.GetInt32(109)
                    End If
                    If Not DataReader.IsDBNull(110) Then
                        TrackComponent(I).TrackSpeedMaximumPassengar = DataReader.GetInt32(110)
                    End If
                End If

                If DataReader.FieldCount >= 114 Then
                    If Not DataReader.IsDBNull(113) Then
                        TrackComponent(I).TypeOfPTC = DataReader.GetString(113)
                    End If
                End If

                If optEnableJointControlMode Then
                    If DataReader.FieldCount >= 115 Then
                        If Not DataReader.IsDBNull(114) Then
                            TrackComponent(I).JointControl = DataReader.GetBoolean(114)
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 123 Then
                    If Not DataReader.IsDBNull(115) Then
                        TrackComponent(I).EnableTractionPowerManagement = DataReader.GetBoolean(115)
                    End If
                    If Not DataReader.IsDBNull(116) Then
                        TrackComponent(I).myCatenaryTrackCircuitName = DataReader.GetString(116)
                    End If

                End If
                If DataReader.FieldCount >= 128 Then '
                    If Not DataReader.IsDBNull(127) Then
                        TrackComponent(I).ForceIgnoreTrainDirectionLogicChange = DataReader.GetBoolean(127)
                    End If
                End If

                If DataReader.FieldCount >= 129 Then '
                    If Not DataReader.IsDBNull(128) Then
                        TrackComponent(I).SuppressAuthorityRelease = DataReader.GetBoolean(128)
                    End If
                End If

                If DataReader.FieldCount >= 130 Then '
                    If Not DataReader.IsDBNull(129) Then
                        TrackComponent(I).IsTrackWarrantEnforcementEnabled = DataReader.GetBoolean(129)
                    End If
                End If
                If DataReader.FieldCount >= 131 Then '
                    If Not DataReader.IsDBNull(130) Then
                        TrackComponent(I).RailRoadUniqueName = DataReader.GetValue(130)
                        If Not gTrackRRName.Contains(TrackComponent(I).RailRoadUniqueName.Trim.ToUpper) Then gTrackRRName.Add(TrackComponent(I).RailRoadUniqueName.Trim.ToUpper)
                    End If
                End If

                TrackComponent(I).Index = I
                TrackComponent(I).TerritoryID = TerritoryID

                If optEnableBulletinSubTypes And optEnableDisplayBulletinsOnTracks Then
                    TrackComponent(I).Bulletins = New ArrayList
                End If


                AddHandler TrackComponent(I).MouseEnter, AddressOf TrackComponent(I).Track_MouseEnter
                AddHandler TrackComponent(I).MouseLeave, AddressOf TrackComponent(I).Track_MouseLeave
                AddHandler TrackComponent(I).MouseHover, AddressOf TrackComponent(I).Track_MouseHover

                'Add the object reference to the Components Collection
                gComponentsCollection.Add(TrackComponent(I))
                gTerritoryComponentsCollection.Add(TrackComponent(I))
                TrackCollection.Add(TrackComponent(I))
            End While
            DataReader.Close()
            'DatabaseCommand.Dispose()
            'DatabaseCommand = Nothing
            RunLength = Now.Subtract(StartTime)
            Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            SystemEvent("TRACKS LOADED...(" & Millisecs.ToString & ")")
            '//Finished Loading Tracks



            '//Loading Switch Components
            SystemEvent("LOADING SWITCHES...")
            gSplashScreenString = "LOADING SWITCHES FOR " & TerrName

            DatabaseCommand = New OleDbCommand(gSwitchSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            DataReader = DatabaseCommand.ExecuteReader()
            I = SwitchComponent.GetUpperBound(0)

            While DataReader.Read()
                Application.DoEvents()
                I = I + 1
                ReDim Preserve SwitchComponent(I)
                SwitchComponent(I) = New Switch()
                SwitchComponent(I).Visible = False

                SwitchComponent(I).GUID = DataReader.GetInt32(0)
                SwitchComponent(I).StationNumber = DataReader.GetInt32(1)
                SwitchComponent(I).Name = DataReader.GetString(2)
                SwitchComponent(I).ComponentType = DataReader.GetString(3)
                SwitchComponent(I).LX = DataReader.GetInt16(4)
                SwitchComponent(I).LY = DataReader.GetInt16(5)
                SwitchComponent(I).RX = DataReader.GetInt16(6)
                SwitchComponent(I).RY = DataReader.GetInt16(7)
                SwitchComponent(I).LeftLink = DataReader.GetInt32(8)
                SwitchComponent(I).RightLink = DataReader.GetInt32(9)
                SwitchComponent(I).LeftAltLink = DataReader.GetInt32(10)
                SwitchComponent(I).RightAltLink = DataReader.GetInt32(11)
                SwitchComponent(I).AltLinkOne = DataReader.GetInt32(12)
                SwitchComponent(I).AltLinkTwo = DataReader.GetInt32(13)
                SwitchComponent(I).CodelineNumber = DataReader.GetInt32(14)
                SwitchComponent(I).AuthorityType = DataReader.GetString(21)

                If optEnableAllowForceOperationsToTWC Then
                    If DataReader.FieldCount >= 56 Then
                        If Not DataReader.IsDBNull(55) Then
                            SwitchComponent(I).ForceOperationsToTWC = DataReader.GetBoolean(55)
                        End If
                    End If
                End If


                'simulation mode
                SwitchComponent(I).SimulateMode = gSimulationMode
                If Not SwitchComponent(I).ForceOperationsToTWC Then
                    If SwitchComponent(I).AuthorityType = "CTC" Or SwitchComponent(I).AuthorityType = "OTHER" Then
                        Dim CtlWord() As String
                        gCTCTerritoryFound = True
                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(15), SwitchComponent(I).Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                        CtlWord = DataReader.GetString(16).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(3), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(16), SwitchComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(17), SwitchComponent(I).Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        CtlWord = DataReader.GetString(18).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(4), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(18), SwitchComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(19), SwitchComponent(I).Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        CtlWord = DataReader.GetString(20).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(5), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(20), SwitchComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        'Version 227
                        'Set fail state to true on the initial load until a code server status is received
                        If Not SwitchComponent(I).SimulateMode Then SwitchComponent(I).FailState = True
                        SwitchComponent(I).Indicating = True

                        If Not SwitchComponent(I).Ctl(0) Is Nothing Then
                            SwitchComponent(I).Ctl(0).BitState = True
                        End If
                        If Not SwitchComponent(I).Ctl(1) Is Nothing Then
                            SwitchComponent(I).Ctl(1).BitState = False
                        End If

                        If Not SwitchComponent(I).Ctl(3) Is Nothing Then
                            SwitchComponent(I).Ctl(3).BitState = True
                        End If
                        If Not SwitchComponent(I).Ctl(4) Is Nothing Then
                            SwitchComponent(I).Ctl(4).BitState = False
                        End If
                    Else
                        Dim CtlWord() As String
                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(15), SwitchComponent(I).Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                        CtlWord = DataReader.GetString(16).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(3), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(16), SwitchComponent(I).Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(17), SwitchComponent(I).Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        CtlWord = DataReader.GetString(18).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(4), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                        'SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(18), SwitchComponent(I).Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(19), SwitchComponent(I).Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        CtlWord = DataReader.GetString(20).Split("|")
                        SwitchComponent(I).TempCodeStationBitBuild(CtlWord(0), SwitchComponent(I).Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        If CtlWord.Length > 1 Then
                            If CtlWord(1) <> "" Then
                                SwitchComponent(I).TempCodeStationBitBuild(CtlWord(1), SwitchComponent(I).Ctl(5), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            End If
                        End If
                    End If


                    'Version 227
                    'Set fail state to true on the initial load until a code server status is received
                    SwitchComponent(I).Indicating = True

                    If Not SwitchComponent(I).Ctl(0) Is Nothing Then
                        SwitchComponent(I).Ctl(0).BitState = True
                    End If
                    If Not SwitchComponent(I).Ctl(1) Is Nothing Then
                        SwitchComponent(I).Ctl(1).BitState = False
                    End If

                    If Not SwitchComponent(I).Ctl(3) Is Nothing Then
                        SwitchComponent(I).Ctl(3).BitState = True
                    End If
                    If Not SwitchComponent(I).Ctl(4) Is Nothing Then
                        SwitchComponent(I).Ctl(4).BitState = False
                    End If

                    If Not SwitchComponent(I).Ctl(0) Is Nothing Then
                        If Not SwitchComponent(I).Ctl(1) Is Nothing Then
                            SwitchComponent(I).PowerSwitchInTWC = True
                        End If
                    End If
                End If

                If SwitchComponent(I).ForceOperationsToTWC Then
                    If SwitchComponent(I).AuthorityType = "CTC" Then
                        SwitchComponent(I).AuthorityType = "TWC"
                    Else
                        SwitchComponent(I).AuthorityType = "NON-TWC"
                    End If
                End If
                If SwitchComponent(I).AuthorityType = "TWC-BABS" Then
                    SwitchComponent(I).AuthorityType = Mid(SwitchComponent(I).AuthorityType, 1, 3)
                    SwitchComponent(I).BiDirectionalABS = True
                    If Not optAllowPOSWithInABS Then
                        SwitchComponent(I).ProtectOpenSwitchEnabled = False
                    End If
                ElseIf SwitchComponent(I).AuthorityType = "TWC-ABSW" Then
                    SwitchComponent(I).AuthorityType = Mid(SwitchComponent(I).AuthorityType, 1, 3)
                    SwitchComponent(I).SingleABSWest = True
                ElseIf SwitchComponent(I).AuthorityType = "TWC-ABSE" Then
                    SwitchComponent(I).AuthorityType = Mid(SwitchComponent(I).AuthorityType, 1, 3)
                    SwitchComponent(I).SingleABSEast = True
                ElseIf SwitchComponent(I).AuthorityType = "TWC" Then
                    If IsNothing(SwitchComponent(I).Ind(0)) Then
                        SwitchComponent(I).FailState = False
                        SwitchComponent(I).NoIndicationAssigned = True
                    End If
                End If

                SwitchComponent(I).TrackName = DataReader.GetString(22)
                SwitchComponent(I).SubdivisionID = DataReader.GetInt32(24)

                If Not SwitchComponent(I).ForceOperationsToTWC Then
                    SwitchComponent(I).IndicateOnly = DataReader.GetBoolean(25)
                    If SwitchComponent(I).IndicateOnly Then
                        SwitchComponent(I).DoMonitorMode = True
                    End If
                    SwitchComponent(I).AllowBlocking = DataReader.GetBoolean(26)
                    SwitchComponent(I).AllowSwitchIndBlock = DataReader.GetBoolean(28)
                    If SwitchComponent(I).AllowSwitchIndBlock Then
                        SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(29), SwitchComponent(I).SwitchBlockInd, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                    End If
                    SwitchComponent(I).TempCodeStationBitBuild(DataReader.GetString(27), SwitchComponent(I).SwitchLockInd, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                End If
                SwitchComponent(I).NormalCorespondence = True
                SwitchComponent(I).Index = I
                SwitchComponent(I).TerritoryID = TerritoryID

                'This is to force box 20 on switches in TWC where they are left reverse in the database
                If Mid(SwitchComponent(I).AuthorityType, 1, 3) = "TWC" And SwitchComponent(I).ControlledReverse Then
                    SwitchComponent(I).LeaveReversePermission = True
                End If
                SwitchComponent(I).ProtectOpenSwitchEnabled = DataReader.GetBoolean(31)
                SwitchComponent(I).OSLocking = DataReader.GetBoolean(32)
                SwitchComponent(I).NoValidate = DataReader.GetBoolean(33)
                SwitchComponent(I).TypeOfSwitch = DataReader.GetString(34)
                If SwitchComponent(I).TypeOfSwitch = "SPRING" Then
                    SwitchComponent(I).SpringSwitch = True
                End If
                If DataReader.FieldCount >= 38 Then
                    If Not DataReader.IsDBNull(36) Then
                        SwitchComponent(I).GraphicalGroupID = DataReader.GetByte(36)
                    End If
                    If Not DataReader.IsDBNull(37) Then
                        SwitchComponent(I).ScreenRegionID = DataReader.GetByte(37)
                    End If
                End If

                If Not DataReader.IsDBNull(35) Then
                    SwitchComponent(I).MatchMode = DataReader.GetBoolean(35)
                End If

                If Not DataReader.IsDBNull(38) Then
                    SwitchComponent(I).FlipOperations = DataReader.GetBoolean(38)
                End If

                If Not DataReader.IsDBNull(39) Then
                    SwitchComponent(I).IgnoreOS = DataReader.GetBoolean(39)
                End If

                If Not DataReader.IsDBNull(40) Then
                    SwitchComponent(I).AllowBlueFlag = DataReader.GetBoolean(40)
                End If

                If Not DataReader.IsDBNull(42) Then
                    SwitchComponent(I).AllowIndependentControlSwitch = DataReader.GetBoolean(42)
                End If

                If DataReader.FieldCount >= 45 Then
                    If Not DataReader.IsDBNull(44) Then
                        SwitchComponent(I).IgnoreBothIndicationReceipt = DataReader.GetBoolean(44)
                    End If
                End If

                If DataReader.FieldCount >= 46 Then
                    If Not DataReader.IsDBNull(45) Then
                        SwitchComponent(I).RCPS = DataReader.GetBoolean(45)
                    End If
                End If
                If SwitchComponent(I).RCPS Then
                    SwitchComponent(I).FailState = True
                    SwitchComponent(I).ForceProtectionOnRCPS = True
                    If Not DataReader.IsDBNull(46) Then
                        Dim ControlBitString() As String = DataReader.GetString(46).Split("|")
                        If ControlBitString.Length >= 4 Then
                            SwitchComponent(I).TempCodeStationBitBuild(ControlBitString(0), SwitchComponent(I).LockControlRightDirection, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            SwitchComponent(I).TempCodeStationBitBuild(ControlBitString(1), SwitchComponent(I).LockControlLeftDirection, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            SwitchComponent(I).TempCodeStationBitBuild(ControlBitString(2), SwitchComponent(I).LockControlBiDirectionNoRestriction, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                            SwitchComponent(I).TempCodeStationBitBuild(ControlBitString(3), SwitchComponent(I).LockControlBiDirectionRestricting, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                        End If
                    End If

                    If Not DataReader.IsDBNull(47) Then
                        Dim IndicationBitString() As String = DataReader.GetString(47).Split("|")
                        If IndicationBitString.Length >= 5 Then
                            SwitchComponent(I).TempCodeStationBitBuild(IndicationBitString(0), SwitchComponent(I).LockIndRightDirection, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SwitchComponent(I).TempCodeStationBitBuild(IndicationBitString(1), SwitchComponent(I).LockIndLeftDirection, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SwitchComponent(I).TempCodeStationBitBuild(IndicationBitString(2), SwitchComponent(I).LockIndBiDirectionNoRestriction, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SwitchComponent(I).TempCodeStationBitBuild(IndicationBitString(3), SwitchComponent(I).LockIndBiDirectionRestricting, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                            SwitchComponent(I).TempCodeStationBitBuild(IndicationBitString(4), SwitchComponent(I).TimeLockIndication, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                        End If
                    End If
                End If
                If DataReader.FieldCount >= 49 Then
                    If Not DataReader.IsDBNull(48) Then
                        SwitchComponent(I).SwitchPointerMonitorMode = DataReader.GetBoolean(48)
                        If SwitchComponent(I).SwitchPointerMonitorMode Then
                            gSwitchPointMonitoringExists = True
                            If Not IsNothing(SwitchComponent(I).Ind(0)) Then
                                SwitchComponent(I).FailState = True
                                SwitchComponent(I).CodeStation = SwitchComponent(I).Ind(0).CodeStation
                            Else
                                SwitchComponent(I).FailState = False
                                SwitchComponent(I).NoIndicationAssigned = True

                            End If

                        End If
                        If SwitchComponent(I).TypeOfSwitch = "SPM ONLY" Then
                            SwitchComponent(I).NonOperationSwitchPointMonitor = True
                        End If
                    End If
                End If
                If DataReader.FieldCount >= 50 Then
                    If Not DataReader.IsDBNull(49) Then
                        SwitchComponent(I).SwitchLockNoControl = DataReader.GetBoolean(49)
                    End If
                End If
                If DataReader.FieldCount >= 51 Then
                    If Not DataReader.IsDBNull(50) Then
                        If DataReader.GetInt32(50) <> 0 Then
                            SwitchComponent(I).PositionReportTimeOut = DataReader.GetInt32(50)
                        End If
                    End If
                End If
                If DataReader.FieldCount >= 52 Then
                    If Not DataReader.IsDBNull(51) Then
                        If DataReader.GetInt32(51) <> 0 Then
                            SwitchComponent(I).mySwitchPositionIndicatorCoordinate = DataReader.GetInt32(51)
                        End If
                    End If
                End If
                If DataReader.FieldCount >= 53 Then
                    If Not DataReader.IsDBNull(52) Then
                        SwitchComponent(I).AllowSwitchRestoralOnAuthRelease = DataReader.GetBoolean(52)
                    End If
                End If
                If DataReader.FieldCount >= 54 Then
                    If Not DataReader.IsDBNull(53) Then
                        SwitchComponent(I).UsedForEntryToMain = DataReader.GetBoolean(53)
                    End If
                End If

                If DataReader.FieldCount >= 55 Then
                    If Not DataReader.IsDBNull(54) Then
                        SwitchComponent(I).PTCUID = DataReader.GetInt32(54)
                    End If
                End If

                If DataReader.FieldCount >= 58 Then
                    If Not DataReader.IsDBNull(56) Then
                        SwitchComponent(I).MilePost = DataReader.GetDouble(56)
                        SwitchComponent(I).myMilePost = DataReader.GetDouble(56)

                    End If
                    If Not DataReader.IsDBNull(57) Then
                        SwitchComponent(I).MilePostSuffix = DataReader.GetString(57)
                        SwitchComponent(I).myMilePostSuffix = DataReader.GetString(57)
                    End If
                    If DataReader.FieldCount >= 62 Then
                        If Not DataReader.IsDBNull(61) Then
                            SwitchComponent(I).myMilePostPrefix = DataReader.GetString(61)
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 59 Then
                    If Not DataReader.IsDBNull(58) Then
                        SwitchComponent(I).myToolTip = DataReader.GetString(58)
                    End If
                End If


                If DataReader.FieldCount >= 60 Then
                    If Not DataReader.IsDBNull(59) Then
                        Dim BlockControls() As String = DataReader.GetString(59).Split("|")
                        If BlockControls.Length = 2 Then
                            If SwitchComponent(I).AllowSwitchIndBlock Then
                                SwitchComponent(I).TempCodeStationBitBuild(BlockControls(0), SwitchComponent(I).SwitchBlockControlOn, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                SwitchComponent(I).TempCodeStationBitBuild(BlockControls(1), SwitchComponent(I).SwitchBlockControlOff, CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                                SwitchComponent(I).AllowFieldBlocking = True
                            End If
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 65 Then
                    If Not DataReader.IsDBNull(63) Then
                        SwitchComponent(I).PublishStatusToPTC = DataReader.GetBoolean(63)
                    End If
                    If Not DataReader.IsDBNull(64) Then
                        SwitchComponent(I).DevicePTCUID = DataReader.GetString(64)
                    End If
                End If

                If DataReader.FieldCount >= 72 Then

                    If Not DataReader.IsDBNull(66) Then
                        SwitchComponent(I).PTC_UID_KEY = DataReader.GetString(66)
                    End If

                    If Not DataReader.IsDBNull(67) Then
                        SwitchComponent(I).PTC_DeviceName = DataReader.GetString(67)
                    End If

                    If Not DataReader.IsDBNull(68) Then
                        SwitchComponent(I).PTC_ProperName = DataReader.GetString(68)
                    End If

                    If Not DataReader.IsDBNull(69) Then
                        SwitchComponent(I).PTC_WIUStatusIndex = DataReader.GetString(69)
                    End If

                    If Not DataReader.IsDBNull(70) Then
                        SwitchComponent(I).PTC_DeviceBlockID = DataReader.GetString(70)
                    End If

                    If Not DataReader.IsDBNull(71) Then
                        SwitchComponent(I).PTC_Enable_EMT = DataReader.GetBoolean(71)
                    End If
                End If




                'Add the object reference to the Components Collection
                SwitchCollection.Add(SwitchComponent(I))
                If optEnableSPMSwitchCollection Then
                    Try
                        If SwitchComponent(I).TypeOfSwitch = "SPM ONLY" Then
                            If Not gSPMSwitchCollection.Contains(SwitchComponent(I)) Then gSPMSwitchCollection.Add(SwitchComponent(I))
                        End If
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    End Try
                End If

                gComponentsCollection.Add(SwitchComponent(I))
                gTerritoryComponentsCollection.Add(SwitchComponent(I))

                If optEnableBulletinSubTypes And optEnableDisplayBulletinsOnTracks Then
                    SwitchComponent(I).Bulletins = New ArrayList
                End If

                If Not DataReader.IsDBNull(30) Then SwitchComponent(I).ControlledReverse = DataReader.GetBoolean(30)


                AddHandler SwitchComponent(I).MouseEnter, AddressOf SwitchComponent(I).Switch_MouseEnter
                AddHandler SwitchComponent(I).MouseLeave, AddressOf SwitchComponent(I).Switch_MouseLeave
                AddHandler SwitchComponent(I).MouseHover, AddressOf SwitchComponent(I).Switch_MouseHover

                SwitchComponent(I).BringToFront()

            End While
            DataReader.Close()
            RunLength = Now.Subtract(StartTime)
            Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            SystemEvent("SWITCHES LOADED...(" & Millisecs.ToString & ")")
            '//Finished Loading Switches


            If Not LoadSignalComponents(TerritoryID, TerrName) Then
                MessageBox.Show("SIGNAL COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                End
            End If


            If optEnableLoadGradeCrossingRecords Then
                If Not LoadGradeCrossing(TerritoryID, TerrName) Then
                    MessageBox.Show("GRADE CROSSING COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    'End
                End If
            End If
            If optEnableLoadSlideZoneRecords Then
                If Not LoadSlideZone(TerritoryID, TerrName) Then
                    MessageBox.Show("SLIDE ZONE COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    End
                End If
            End If

            If optEnableTractionPowerManagementSystem Then
                If Not LoadElectricalSwitches(TerritoryID, TerrName) Then
                    MessageBox.Show("ELECTRICAL SWITCH COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    End
                End If
                If Not LoadElectricalLines(TerritoryID, TerrName) Then
                    MessageBox.Show("ELECTRICAL LINE COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    End
                End If
                If Not LoadElectricalConnections(TerritoryID, TerrName) Then
                    MessageBox.Show("ELECTRICAL CONNECTIONS COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    End
                End If

            End If


            If optEnableWeatherComponentRecords Then
                LoadWeatherStation(TerritoryID, TerrName)
            End If

            If optEnablePassengerStation Then
                If Not LoadPassengerStation(TerritoryID, TerrName) Then
                    MessageBox.Show("PASSENGER STATION COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                    End
                End If
            End If

            '//Loading Alphatext Components
            SystemEvent("LOADING ALPHA TEXT...")
            gSplashScreenString = "LOADING ALPHA TEXT FOR " & TerrName

            DatabaseCommand = New OleDbCommand(gAlphaTextSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            DataReader = DatabaseCommand.ExecuteReader()

            While DataReader.Read()
                Application.DoEvents()
                I = I + 1
                ReDim Preserve AlphaComponent(I)
                AlphaComponent(I) = New AlphaText()
                AlphaComponent(I).GUID = DataReader.GetInt32(0)
                AlphaComponent(I).StationNumber = DataReader.GetInt32(1)
                AlphaComponent(I).Name = DataReader.GetString(2)
                AlphaComponent(I).ComponentType = DataReader.GetString(3)
                AlphaComponent(I).LX = DataReader.GetInt16(4)
                AlphaComponent(I).LY = DataReader.GetInt16(5)
                AlphaComponent(I).SubdivisionID = DataReader.GetInt32(15)

                If Not DataReader.IsDBNull(16) Then
                    AlphaComponent(I).TextColor = DataReader.GetString(16)
                End If

                If DataReader.FieldCount >= 19 Then
                    If Not DataReader.IsDBNull(17) Then
                        AlphaComponent(I).GraphicalGroupID = DataReader.GetByte(17)
                    End If
                    If Not DataReader.IsDBNull(18) Then
                        AlphaComponent(I).ScreenRegionID = DataReader.GetByte(18)
                    End If
                End If

                If DataReader.FieldCount >= 20 Then
                    If Not DataReader.IsDBNull(19) Then
                        AlphaComponent(I).FontString = DataReader.GetString(19)
                        AlphaComponent(I).InitializeFontString()
                    End If
                End If

                AlphaComponent(I).TerritoryID = TerritoryID
                'Add the object reference to the Components Collection
                gComponentsCollection.Add(AlphaComponent(I))
                gTerritoryComponentsCollection.Add(AlphaComponent(I))
            End While
            DataReader.Close()
            'DatabaseCommand.Dispose()
            'DatabaseCommand = Nothing
            RunLength = Now.Subtract(StartTime)
            Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            SystemEvent("ALPHA TEXT LOADED...(" & Millisecs.ToString & ")")


            '//Loading Authority Type Points Components
            'SystemEvent("LOADING AUTHORITY POINTS...")

            DatabaseCommand = New OleDbCommand(gAuthPointsSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            ''gDatabaseConnection.Open()
            DataReader = DatabaseCommand.ExecuteReader()

            While DataReader.Read()
                Dim AuthPt As New AuthorityPoint
                AuthPt.GUID = DataReader.GetInt32(0)
                AuthPt.StationNumber = DataReader.GetInt32(1)
                AuthPt.Name = DataReader.GetString(2)
                AuthPt.ComponentType = DataReader.GetString(3)
                AuthPt.LX = DataReader.GetInt16(4)
                AuthPt.LY = DataReader.GetInt16(5)
                AuthPt.SubdivisionID = DataReader.GetInt32(15)

                If Not DataReader.IsDBNull(16) Then
                    AuthPt.TextColor = DataReader.GetString(16)
                End If

                If DataReader.FieldCount >= 19 Then
                    If Not DataReader.IsDBNull(17) Then
                        AuthPt.GraphicalGroupID = DataReader.GetByte(17)
                    End If
                    If Not DataReader.IsDBNull(18) Then
                        AuthPt.ScreenRegionID = DataReader.GetByte(18)
                    End If
                End If

                If Not DataReader.IsDBNull(25) Then
                    AuthPt.MilePost = DataReader.GetDouble(25)
                End If
                If optEnableHideAuthorityPointObjects Then
                    AuthPt.Visible = False
                End If
                AuthPt.TerritoryID = TerritoryID
                'Add the object reference to the Components Collection
                gComponentsCollection.Add(AuthPt)
                gTerritoryComponentsCollection.Add(AuthPt)
            End While
            DataReader.Close()
            DatabaseCommand.Dispose()
            DatabaseCommand = Nothing
            RunLength = Now.Subtract(StartTime)
            Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            SystemEvent("AUTHORITY POINTS LOADED...(" & Millisecs.ToString & ")")
            '//Finished Loading ALPHA COMPONENTS

            '//Loading RRXING Components
            SystemEvent("LOADING RAILROAD XINGs...")
            gSplashScreenString = "LOADING RAILROAD XINGS FOR " & TerrName

            DatabaseCommand = New OleDbCommand(gRRXINGSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            DataReader = DatabaseCommand.ExecuteReader()

            Dim rr As RRXING

            While DataReader.Read()
                rr = New RRXING()
                rr.GUID = DataReader.GetInt32(0)
                rr.StationNumber = DataReader.GetInt32(1)
                rr.Name = DataReader.GetString(2)
                rr.ComponentType = DataReader.GetString(3)
                rr.LX = DataReader.GetInt16(4)
                rr.LY = DataReader.GetInt16(5)
                rr.RX = DataReader.GetInt16(6)
                rr.RY = DataReader.GetInt16(7)
                rr.LeftLink = DataReader.GetInt32(8)
                rr.RightLink = DataReader.GetInt32(9)
                rr.LeftAltLink = DataReader.GetInt32(10)
                rr.RightAltLink = DataReader.GetInt32(11)
                rr.CodelineNumber = DataReader.GetInt32(14)
                rr.TrackName = DataReader.GetString(26)

                'rr.AuthorityType = DataReader.GetString(32)
                'rr.SubdivisionID = DataReader.GetInt32(34)
                If DataReader.FieldCount >= 43 Then
                    If Not DataReader.IsDBNull(41) Then
                        rr.GraphicalGroupID = DataReader.GetByte(41)
                    End If
                    If Not DataReader.IsDBNull(42) Then
                        rr.ScreenRegionID = DataReader.GetByte(42)
                    End If
                End If
                rr.SimulateMode = gSimulationMode
                rr.TerritoryID = TerritoryID

                gComponentsCollection.Add(rr)
                gTerritoryComponentsCollection.Add(rr)
            End While
            DataReader.Close()
            'DatabaseCommand.Dispose()
            'DatabaseCommand = Nothing
            'RunLength = Now.Subtract(StartTime)
            'Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            'SystemEvent("RAILROAD XINGs LOADED...(" & Millisecs.ToString & ")")

            '//Finished Loading RRXING

            '//Loading FlipFlop Components
            SystemEvent("LOADING FLIP-FLOPS...")
            gSplashScreenString = "LOADING FLIP-FLOPS FOR " & TerrName

            DatabaseCommand = New OleDbCommand(gFlipFlopSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            DataReader = DatabaseCommand.ExecuteReader()

            Dim ff As FlipFlop

            While DataReader.Read()
                ff = New FlipFlop()
                ff.GUID = DataReader.GetInt32(0)
                ff.StationNumber = DataReader.GetInt32(1)
                ff.Name = DataReader.GetString(2)
                ff.ComponentType = DataReader.GetString(3)
                ff.LX = DataReader.GetInt16(4)
                ff.LY = DataReader.GetInt16(5)
                ff.RX = DataReader.GetInt16(6)
                ff.RY = DataReader.GetInt16(7)
                ff.LeftLink = DataReader.GetInt32(8)
                ff.RightLink = DataReader.GetInt32(9)
                ff.LeftAltLink = DataReader.GetInt32(10)
                ff.RightAltLink = DataReader.GetInt32(11)
                ff.CodelineNumber = DataReader.GetInt32(14)
                ff.TrackName = DataReader.GetString(26)

                'ff.AuthorityType = DataReader.GetString(32)
                'ff.SubdivisionID = DataReader.GetInt32(34)
                If DataReader.FieldCount >= 43 Then
                    If Not DataReader.IsDBNull(41) Then
                        ff.GraphicalGroupID = DataReader.GetByte(41)
                    End If
                    If Not DataReader.IsDBNull(42) Then
                        ff.ScreenRegionID = DataReader.GetByte(42)
                    End If
                End If

                ff.SimulateMode = gSimulationMode
                ff.TerritoryID = TerritoryID

                gFlipFlopCollection.Add(ff)

                gComponentsCollection.Add(ff)
                gTerritoryComponentsCollection.Add(ff)
            End While
            DataReader.Close()
            'DatabaseCommand.Dispose()
            'DatabaseCommand = Nothing
            'RunLength = Now.Subtract(StartTime)
            'Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            'SystemEvent("FLIFLOPs LOADED...(" & Millisecs.ToString & ")")

            '//Finished Loading FlipFlop


            'Load Miscellanous Devices
            'Load Misc Devices
            Dim dv As Device
            SystemEvent("LOADING DEVICES...")
            gSplashScreenString = "LOADING DEVICES FOR " & TerrName

            DatabaseCommand = New OleDbCommand(gDeviceSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
            DataReader = DatabaseCommand.ExecuteReader()
            While DataReader.Read()
                Application.DoEvents()
                dv = New Device()
                dv.GUID = DataReader.GetInt32(0)
                dv.StationNumber = DataReader.GetInt32(1)
                dv.Name = DataReader.GetString(2)
                dv.PrimaryText = dv.Name
                dv.ComponentType = DataReader.GetString(3)
                dv.LX = DataReader.GetInt16(4)
                dv.LY = DataReader.GetInt16(5)
                dv.RX = DataReader.GetInt16(6)
                dv.RY = DataReader.GetInt16(7)
                dv.LeftLink = DataReader.GetInt32(8)
                dv.RightLink = DataReader.GetInt32(9)
                dv.LeftAltLink = DataReader.GetInt32(10)
                dv.RightAltLink = DataReader.GetInt32(11)
                dv.AltLinkOne = DataReader.GetInt32(12)
                dv.AltLinkTwo = DataReader.GetInt32(13)
                dv.CodelineNumber = DataReader.GetInt32(14)
                Select Case Mid(dv.ComponentType, 2)
                    Case "&"
                        dv.IndicationANDED = True
                        dv.IndicationORED = False
                    Case "+"
                        dv.IndicationANDED = False
                        dv.IndicationORED = True
                End Select
                If DataReader.FieldCount >= 46 Then
                    If Not DataReader.IsDBNull(45) Then dv.ScadaMode = DataReader.GetBoolean(45)
                End If

                dv.TempCodeStationBitBuild(DataReader.GetString(15), dv.Ind(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                dv.TempCodeStationBitBuild(DataReader.GetString(16), dv.Ind(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)
                dv.TempCodeStationBitBuild(DataReader.GetString(17), dv.Ind(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.INDICATION)

                If dv.ScadaMode AndAlso Not IsNothing(dv.Ind(0)) Then
                    dv.Ind(0).IsAlarmType = True
                    dv.Ind(0).LoadAlarmInformation()
                End If

                dv.TempCodeStationBitBuild(DataReader.GetString(18), dv.Ctl(0), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                dv.TempCodeStationBitBuild(DataReader.GetString(19), dv.Ctl(1), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)
                dv.TempCodeStationBitBuild(DataReader.GetString(20), dv.Ctl(2), CtlIndBits, gCtlIndBitMapNumber, CodeServerClient.BitPosition.BitType.CONTROL)

                dv.TerritoryID = DataReader.GetInt32(21)
                dv.MatchMode = DataReader.GetBoolean(22)
                dv.IndicateOnly = DataReader.GetBoolean(23)
                dv.ControlOnly = DataReader.GetBoolean(24)
                dv.FlashIndicate = DataReader.GetBoolean(25)
                dv.FlashControl = DataReader.GetBoolean(26)
                dv.TextColor = DataReader.GetString(27)
                dv.TypeOfDevice = DataReader.GetString(28)
                If IsNumeric(dv.TextColor) Then
                    dv.ForeColor = Color.FromArgb(CInt(dv.TextColor))

                    dv.RefreshPath()
                Else
                    dv.ForeColor = Color.RosyBrown
                End If
                dv.PrimaryColor = dv.ForeColor

                If DataReader.FieldCount >= 30 Then
                    dv.Subdivision = DataReader.GetInt32(29)
                End If

                If DataReader.FieldCount >= 32 Then
                    If Not DataReader.IsDBNull(30) Then
                        dv.GraphicalGroupID = DataReader.GetByte(30)
                    End If
                    If Not DataReader.IsDBNull(31) Then
                        dv.ScreenRegionID = DataReader.GetByte(31)
                    End If
                End If

                If Not DataReader.IsDBNull(32) Then
                    dv.TrackGUID = DataReader.GetString(32)
                End If

                If Not DataReader.IsDBNull(33) Then
                    dv.FontString = DataReader.GetString(33)
                End If

                If Not DataReader.IsDBNull(34) Then
                    Dim data() As String = Trim(DataReader.GetString(34)).Split(";")
                    Try
                        dv.SecondaryText = data(0)
                        dv.SecondaryTextColor = data(1)
                        If dv.SecondaryText <> "NONE" Then
                            dv.SecondaryIndFound = True
                        End If
                        If IsNumeric(dv.SecondaryTextColor) Then
                            dv.SecondaryColor = Color.FromArgb(CInt(dv.SecondaryTextColor))
                        End If
                    Catch
                        dv.SecondaryText = "NONE"
                        dv.SecondaryTextColor = Color.White.ToArgb.ToString
                    End Try
                Else
                    dv.SecondaryText = "NONE"
                    dv.SecondaryTextColor = Color.White.ToArgb.ToString
                End If


                If Not DataReader.IsDBNull(35) Then
                    dv.DeviceControlString = DataReader.GetString(35)
                End If
                Try
                    If Not DataReader.IsDBNull(36) Then
                        dv.DeviceControlled = DataReader.GetBoolean(36)
                    End If
                Catch
                End Try

                If DataReader.FieldCount >= 40 Then
                    If Not DataReader.IsDBNull(38) Then
                        dv.AlertOnIndication = Trim(DataReader.GetBoolean(38))
                    End If

                    If Not DataReader.IsDBNull(39) Then
                        dv.AlertVerbiage = Trim(DataReader.GetString(39))
                    End If
                End If

                If DataReader.FieldCount >= 42 Then
                    If Not DataReader.IsDBNull(41) Then
                        dv.InvertIndication = DataReader.GetBoolean(41)
                    End If
                End If

                If DataReader.FieldCount >= 43 Then
                    If Not DataReader.IsDBNull(42) Then
                        dv.ManualOperateMode = DataReader.GetBoolean(42)
                    End If
                End If

                If DataReader.FieldCount >= 45 Then
                    If Not DataReader.IsDBNull(43) And Not DataReader.IsDBNull(44) Then
                        dv.AllowSnowMelterTimer = DataReader.GetBoolean(43)
                        dv.SnowMelterTime = DataReader.GetInt32(44)
                        If dv.AllowSnowMelterTimer Then
                            dv.AttachSnMZTimer()
                        End If
                        'Future Use
                        'If OptEnableCustomSnowMelterTimer Then
                        '    If dv.myControlPoint IsNot Nothing Then dv.SnowMelterTime = dv.myControlPoint.SNMZTime
                        '    dv.AttachSnMZTimer()
                        'Else
                        '    If dv.AllowSnowMelterTimer Then
                        '        dv.AttachSnMZTimer()
                        '    ElseIf OptEnableCustomSnowMelterTimer Then

                        '    End If
                        'End If
                    End If

                End If

                If DataReader.FieldCount >= 47 Then
                    If Not DataReader.IsDBNull(46) Then
                        If optEnableMiscDeviceTrackInterlockMode Then
                            dv.IsInterlockedWithTrack = DataReader.GetBoolean(46)
                            If dv.IsInterlockedWithTrack Then
                                dv.InitializeTrackInterlockMode()
                            End If
                        End If
                    End If
                End If

                If DataReader.FieldCount >= 48 Then
                    If Not DataReader.IsDBNull(47) Then dv.DisableSlotting = DataReader.GetBoolean(47)
                End If
                If DataReader.FieldCount >= 50 Then
                    If Not DataReader.IsDBNull(49) Then dv.ScadaSuppressTime = DataReader.GetValue(49)
                    If dv.ScadaSuppressTime > 0 Then
                        dv.AttachSCADATimer()
                    End If
                End If
                If Not dv.SecondaryIndFound Then
                    dv.Visible = False
                End If
                dv.SimulateMode = gSimulationMode
                dv.TerritoryID = TerritoryID
                DeviceCollection.Add(dv)
                gComponentsCollection.Add(dv)
                gTerritoryComponentsCollection.Add(dv)
                ' Connect the shape to all its event handlers.
                AddHandler dv.MouseDown, AddressOf dv.Device_MouseDown
                AddHandler dv.MouseEnter, AddressOf dv.Device_MouseEnter
                AddHandler dv.MouseLeave, AddressOf dv.Device_MouseLeave

                dv.Mode = GraphicComponent.GenericComponent.ComponentMode.Regular
            End While
            DataReader.Close()
            'DatabaseCommand.Dispose()
            'DatabaseCommand = Nothing
            'RunLength = Now.Subtract(StartTime)
            'Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
            'SystemEvent("DEVICES LOADED...(" & Millisecs.ToString & ")")


            Try
                SystemEvent("LOADING BITMAPS...")
                gSplashScreenString = "LOADING BITMAPS FOR " & TerrName

                DatabaseCommand = New OleDbCommand(gBitMapSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                DataReader = DatabaseCommand.ExecuteReader()
                If Not Directory.Exists(Application.StartupPath & "\Bitmaps") Then
                    Directory.CreateDirectory(Application.StartupPath & "\Bitmaps")
                End If

                Dim bp As BitMapPicture

                While DataReader.Read()
                    bp = New BitMapPicture()
                    bp.GUID = DataReader.GetInt32(0)
                    bp.Name = DataReader.GetString(1)
                    bp.StationNumber = DataReader.GetInt32(2)
                    bp.TerritoryID = DataReader.GetInt32(3)
                    bp.LX = DataReader.GetInt16(4)
                    bp.LY = DataReader.GetInt16(5)
                    bp.RX = DataReader.GetInt16(6)
                    bp.RY = DataReader.GetInt16(7)

                    If Not DataReader.IsDBNull(10) Then
                        bp.GraphicalGroupID = DataReader.GetByte(10)
                    End If
                    If Not DataReader.IsDBNull(11) Then
                        bp.ScreenRegionID = DataReader.GetByte(11)
                    End If
                    bp.TerritoryID = TerritoryID

                    Dim fs As FileStream
                    Dim bw As BinaryWriter
                    Dim bufferSize As Integer = DataReader.GetInt32(9)
                    Dim outbyte(DataReader.GetInt32(9)) As Byte
                    Dim retval As Long
                    Dim TempBitmapFile As String = Application.StartupPath & "\Bitmaps\" & bp.GUID.ToString & ".bmp"
                    If Not File.Exists(TempBitmapFile) Then
                        fs = New FileStream(TempBitmapFile, FileMode.OpenOrCreate, FileAccess.ReadWrite)
                        bw = New BinaryWriter(fs)
                        retval = DataReader.GetBytes(8, 0, outbyte, 0, bufferSize)
                        bw.Write(outbyte)
                        bw.Flush()
                        bw.Close()
                        fs.Close()
                    End If
                    If DataReader.FieldCount >= 13 Then
                        If Not DataReader.IsDBNull(12) Then
                            bp.myToolTip = DataReader.GetValue(12)
                        End If
                    End If
                    Dim curImage As Bitmap = Image.FromFile(TempBitmapFile)
                    bp.Picture = New PictureBox()
                    bp.Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
                    bp.Picture.Location = New Point(bp.LX, bp.LY)
                    'bp.Picture.Image.FromFile(TempBitmapFile)
                    bp.Picture.Image = CType(curImage, Bitmap) ' New Bitmap(bp.PicturePath)
                    'bp.Picture.Size = New System.Drawing.Size(275, 200)

                    'File.Delete(TempBitmapFile)
                    AddHandler bp.MouseHover, AddressOf bp.BMP_MouseHover
                    BitmapPictureCollection.Add(bp)
                    gComponentsCollection.Add(bp)

                    gTerritoryComponentsCollection.Add(bp)

                End While
            Catch ex As Exception
                TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
            End Try
            DataReader.Close()


            Try
                SystemEvent("LOADING FEDS...")
                gSplashScreenString = "LOADING FEDS FOR " & TerrName

                DatabaseCommand = New OleDbCommand(gFEDSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                DataReader = DatabaseCommand.ExecuteReader()

                Dim fd As FED

                While DataReader.Read()
                    fd = New FED()
                    fd.GUID = DataReader.GetInt32(0)
                    fd.StationNumber = DataReader.GetInt32(1)
                    fd.Name = DataReader.GetString(2)
                    fd.ComponentType = DataReader.GetString(3)
                    fd.LX = DataReader.GetInt16(4)
                    fd.LY = DataReader.GetInt16(5)
                    fd.RX = DataReader.GetInt16(6)
                    fd.RY = DataReader.GetInt16(7)
                    fd.TerritoryID = DataReader.GetInt32(14)
                    fd.SubdivisionID = DataReader.GetInt32(15)
                    fd.TrackGUID = DataReader.GetInt32(16)
                    fd.GraphicalGroupID = DataReader.GetByte(17)
                    fd.ScreenRegionID = DataReader.GetByte(18)
                    If Not DataReader.IsDBNull(19) Then
                        fd.MilePost = DataReader.GetString(19)
                        fd.myMilePost = DataReader.GetString(19)
                    End If
                    fd.FEDUID = DataReader.GetString(23)
                    If DataReader.FieldCount >= 27 Then
                        If Not DataReader.IsDBNull(25) Then
                            fd.TypeOfDetector = DataReader.GetString(25)
                        End If
                        If Not DataReader.IsDBNull(26) Then
                            fd.UniqueIdentifier = DataReader.GetString(26)
                        End If
                    End If
                    If DataReader.FieldCount >= 29 Then
                        If Not DataReader.IsDBNull(27) Then
                            fd.myMilePostSuffix = DataReader.GetString(27)
                        End If
                        If Not DataReader.IsDBNull(28) Then
                            fd.myMilePostPrefix = DataReader.GetString(28)
                        End If
                    End If

                    If DataReader.FieldCount >= 25 Then
                        If Not DataReader.IsDBNull(24) Then
                            fd.notes = Trim(DataReader.GetString(24))
                        End If
                    End If

                    Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(fd.TrackGUID)
                    fd.myTrack = trk
                    If Not IsNothing(trk) Then
                        If IsNothing(trk.FEDList) Then
                            trk.FEDList = New ArrayList()
                        End If
                        trk.FEDList.Add(fd)
                    End If
                    fd.Visible = False
                    AddHandler fd.MouseHover, AddressOf fd.FED_MouseHover

                    DetectorCollection.Add(fd)
                    gComponentsCollection.Add(fd)
                    gTerritoryComponentsCollection.Add(fd)
                End While

            Catch ex As Exception
                TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
            End Try
            DataReader.Close()


            Try
                SystemEvent("LOADING FAX-PRINT DEVICES...")
                gSplashScreenString = "LOADING FAX-PRINT DEVICES FOR " & TerrName

                DatabaseCommand = New OleDbCommand(gPFDSelectQuery, TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                DataReader = DatabaseCommand.ExecuteReader()

                Dim pfd As PrintFaxDevice

                While DataReader.Read()
                    Application.DoEvents()
                    pfd = New PrintFaxDevice()
                    pfd.GUID = DataReader.GetInt32(0)
                    pfd.StationNumber = DataReader.GetInt32(1)
                    pfd.Name = DataReader.GetString(2)
                    pfd.LX = DataReader.GetInt16(4)
                    pfd.LY = DataReader.GetInt16(5)
                    pfd.RX = DataReader.GetInt16(6)
                    pfd.RY = DataReader.GetInt16(7)
                    pfd.TerritoryID = DataReader.GetInt32(14)
                    pfd.SubdivisionID = DataReader.GetInt32(15)

                    If Not DataReader.IsDBNull(19) Then
                        pfd.Email = DataReader.GetString(19)
                    End If

                    If Not DataReader.IsDBNull(20) Then
                        pfd.PhoneNumber = DataReader.GetString(20)
                    End If

                    If Not DataReader.IsDBNull(21) Then
                        pfd.IPAdd = DataReader.GetString(21)
                    End If

                    If Not DataReader.IsDBNull(22) Then
                        pfd.UniqueName = DataReader.GetString(22)
                    End If
                    If Not DataReader.IsDBNull(23) Then
                        pfd.TypeOfDevice = DataReader.GetString(23)
                    End If
                    If Not DataReader.IsDBNull(24) Then
                        If IsNumeric(DataReader.GetString(24)) Then
                            pfd.NumberOfCopies = CInt(DataReader.GetString(24))
                        End If
                    End If
                    'If Not gPrintFaxDeviceList.Contains(pfd) Then gPrintFaxDeviceList.Add(pfd)
                    If optEnablePFDUniqueNameCheck Then
                        If Not CheckPrinterExists(pfd) Then gPrintFaxDeviceList.Add(pfd)
                    Else
                        If Not gPrintFaxDeviceList.Contains(pfd) Then gPrintFaxDeviceList.Add(pfd)
                    End If

                End While

            Catch ex As Exception
                TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
            End Try
            DataReader.Close()

            If Not LoadControlPoints(TerritoryID, TerrName) Then
                MessageBox.Show("CONTROL POINT COMPONENTS DID NOT LOAD PROPERLY....SHUTTING APPLICATION DOWN!")
                End
            Else
                RunLength = Now.Subtract(StartTime)
                Millisecs = ((RunLength.Seconds * 1000) + RunLength.Milliseconds)
                SystemEvent("CONTROL POINTS LOADED...(" & Millisecs.ToString & ")")
            End If


            'TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Close()
            gDataLoaded = True



        Catch ex As Exception
            'SystemException(ex.ToString)
            GetErrorDetails(ex)
            'TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Close()
            'TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Close()
        Finally
            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic.State = ConnectionState.Open Then TMDS.GlobalDeclarations.gDatabaseConnectionStatic.Close()
            If Not DataReader Is Nothing Then
                DataReader.Close()
                DataReader = Nothing
            End If
            If Not DatabaseCommand Is Nothing Then
                DatabaseCommand.Dispose()
                DatabaseCommand = Nothing
            End If
        End Try

    End Sub

    Private Function CheckPrinterExists(ByVal Pfd As PrintFaxDevice) As Boolean
        Try
            Dim prn As PrintFaxDevice
            For Each prn In gPrintFaxDeviceList
                If optEnableMRLSpecificOptions Then
                    If prn.UniqueName = Pfd.UniqueName And prn.IPAdd = Pfd.IPAdd AndAlso prn.SubdivisionID = Pfd.SubdivisionID Then
                        Return True
                    End If
                Else
                    If prn.UniqueName = Pfd.UniqueName And prn.IPAdd = Pfd.IPAdd Then
                        Return True
                    End If
                End If

            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return False
    End Function

    Private Sub ParseCrossingEdges(ByRef GC As GradeCrossing)
        Dim lstHighMile As New ArrayList
        Dim lstLowMile As New ArrayList
        Dim CrossingEdgeArr As New ArrayList
        Dim s As String = ""
        Dim FirstCrossingEdgeString As String = ""
        Try
            If GC Is Nothing Then Exit Sub
            If GC.sCrossingEdges IsNot Nothing AndAlso Not String.IsNullOrEmpty(GC.sCrossingEdges) AndAlso GC.sCrossingEdges.Trim <> "" Then
                Dim sEdge() As String = GC.sCrossingEdges.Split("|")
                s = ""
                For Each s In sEdge
                    If Not String.IsNullOrEmpty(s) AndAlso s.Trim <> "" Then
                        CrossingEdgeArr.Add(s)
                    End If
                Next
                If CrossingEdgeArr IsNot Nothing AndAlso CrossingEdgeArr.Count > 0 Then
                    For Each s In CrossingEdgeArr
                        If s.Trim <> "" Then
                            Dim Prefix As String = ""
                            Dim Suffix As String = ""
                            Dim LowMile As Double = 0
                            Dim HighMile As Double = 0
                            Dim TrkName As String = ""
                            Dim data() As String = s.Split(";")
                            If data IsNot Nothing AndAlso data.Length > 3 Then
                                Prefix = data(0)
                                Suffix = data(1)
                                TrkName = data(4)
                                'LowMile = data(2)
                                'HighMile = data(3)
                                If Double.TryParse(data(2), LowMile) Then
                                    If LowMile > 0 Then lstLowMile.Add(LowMile)
                                End If
                                If Double.TryParse(data(3), HighMile) Then
                                    If HighMile > 0 Then lstHighMile.Add(HighMile)
                                End If
                                Dim CE As New GradeCrossing.clsCrossingEdge
                                With CE
                                    .HighMile = HighMile
                                    .LowMile = LowMile
                                    .Prefix = Prefix
                                    .Suffix = Suffix
                                    .TrackName = TrkName
                                End With
                                If CE IsNot Nothing Then
                                    GC.CrossingEdgesCollection.Add(CE)
                                End If
                            End If
                        End If
                    Next
                    If lstLowMile IsNot Nothing AndAlso lstLowMile.Count > 0 Then
                        lstLowMile.Sort()
                        GC.CrossingEdgeLowMile = lstLowMile(0)
                    End If
                    If lstHighMile IsNot Nothing AndAlso lstHighMile.Count > 0 Then
                        lstHighMile.Sort()
                        GC.CrossingEdgeHighMile = lstHighMile(lstHighMile.Count - 1)
                    End If
                End If
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Private Function LoadPassengerStation(ByVal TerritoryID As Integer, Optional ByVal TerrName As String = "NONE") As Boolean
        'Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("LOADING PLATFORM STATIONS...")
            gSplashScreenString = "LOADING PLATFORM STATIONS FOR " & TerrName
            Dim query As New StringBuilder("SELECT * FROM tblCompPSGRPlatforms WHERE [TerritoryAssignment]  = " & TerritoryID)
            'dbconn = New OleDbConnection(gConnectionStringStatic)
            Using dbconn As New OleDbConnection(gConnectionStringStatic)
                dbconn.Open()
                dbcomm = New OleDbCommand(query.ToString, dbconn)
                dr = dbcomm.ExecuteReader
                While dr.Read
                    Dim PS As New PassengerStation
                    PS.GUID = dr.GetInt32(0)
                    PS.StationNumber = dr.GetInt32(1)
                    PS.Name = dr.GetString(2)
                    PS.ComponentType = dr.GetString(3)
                    'PS.Characteristics = dr.GetString(3)
                    PS.LX = dr.GetInt16(4)
                    PS.LY = dr.GetInt16(5)
                    PS.RX = dr.GetInt16(6)
                    PS.RY = dr.GetInt16(7)
                    PS.TerritoryID = dr.GetInt32(14)
                    PS.Subdivision = dr.GetInt32(15)
                    PS.GraphicalGroupID = dr.GetByte(16)
                    PS.ScreenRegionID = dr.GetByte(17)
                    PS.LocIndex = dr.GetInt32(18)
                    PS.OrderIndex = dr.GetInt32(19)

                    If Not dr.IsDBNull(20) Then
                        PS.MilePost = dr.GetString(20)
                    End If
                    If Not dr.IsDBNull(21) Then
                        PS.MilePostSuffix = dr.GetString(21)
                    End If
                    If Not dr.IsDBNull(22) Then
                        PS.MileageNext = dr.GetString(22)
                    End If
                    If Not dr.IsDBNull(23) Then
                        PS.SidingLen = dr.GetString(23)
                    End If
                    If Not dr.IsDBNull(24) Then
                        PS.TypeOp = dr.GetString(24)
                    End If
                    If Not dr.IsDBNull(25) Then
                        PS.TrackType = dr.GetString(25)
                    End If
                    'If Not dr.IsDBNull(32) Then
                    '    PS.Notes = Trim(dr.GetString(32))
                    'End If
                    If dr.FieldCount >= 31 Then
                        If Not dr.IsDBNull(30) Then
                            PS.MilePostChangeEvent = dr.GetBoolean(30)
                        End If
                    End If

                    If Not dr.IsDBNull(33) Then
                        PS.MilePostPrefix = dr.GetString(33)
                    End If

                    If dr.FieldCount >= 43 Then
                        Dim S As String = ""
                        Dim TrkGUID As Integer = 0
                        If Not dr.IsDBNull(38) Then
                            PS.ReportingUID = dr.GetString(38)
                        End If
                        If Not dr.IsDBNull(39) Then
                            PS.EntryReportingLeftBoundTracks = dr.GetString(39)
                            If PS.EntryReportingLeftBoundTracks.Trim <> "" Then
                                Dim Data() As String = PS.EntryReportingLeftBoundTracks.Split("|")
                                For Each S In Data
                                    If S.Trim <> "" Then
                                        TrkGUID = 0
                                        If Integer.TryParse(S, TrkGUID) Then
                                            If TrkGUID > 0 Then
                                                If Not PS.ReportingEntryTracksForLeftBoundMovements.Contains(TrkGUID) Then PS.ReportingEntryTracksForLeftBoundMovements.Add(TrkGUID)
                                            End If
                                        End If
                                    End If

                                Next
                            End If
                        End If
                        If Not dr.IsDBNull(40) Then
                            PS.EntryReportingRightBoundTracks = dr.GetString(40)
                            If PS.EntryReportingRightBoundTracks.Trim <> "" Then
                                Dim Data() As String = PS.EntryReportingRightBoundTracks.Split("|")
                                For Each S In Data
                                    If S.Trim <> "" Then
                                        TrkGUID = 0
                                        If Integer.TryParse(S, TrkGUID) Then
                                            If TrkGUID > 0 Then
                                                If Not PS.ReportingEntryTracksForRightBoundMovements.Contains(TrkGUID) Then PS.ReportingEntryTracksForRightBoundMovements.Add(TrkGUID)
                                            End If
                                        End If
                                    End If

                                Next
                            End If
                        End If
                        If Not dr.IsDBNull(41) Then
                            PS.ExitReportingLeftBoundTracks = dr.GetString(41)
                            If PS.ExitReportingLeftBoundTracks.Trim <> "" Then
                                Dim Data() As String = PS.ExitReportingLeftBoundTracks.Split("|")
                                For Each S In Data
                                    If S.Trim <> "" Then
                                        TrkGUID = 0
                                        If Integer.TryParse(S, TrkGUID) Then
                                            If TrkGUID > 0 Then
                                                If Not PS.ReportingExitTracksForLeftBoundMovements.Contains(TrkGUID) Then PS.ReportingExitTracksForLeftBoundMovements.Add(TrkGUID)
                                            End If
                                        End If
                                    End If

                                Next
                            End If
                        End If
                        If Not dr.IsDBNull(42) Then
                            PS.ExitReportingRightBoundTracks = dr.GetString(42)
                            If PS.ExitReportingRightBoundTracks.Trim <> "" Then
                                Dim Data() As String = PS.ExitReportingRightBoundTracks.Split("|")
                                For Each S In Data
                                    If S.Trim <> "" Then
                                        TrkGUID = 0
                                        If Integer.TryParse(S, TrkGUID) Then
                                            If TrkGUID > 0 Then
                                                If Not PS.ReportingExitTracksForRightBoundMovements.Contains(TrkGUID) Then PS.ReportingExitTracksForRightBoundMovements.Add(TrkGUID)
                                            End If
                                        End If
                                    End If

                                Next
                            End If
                        End If
                    End If
                    If dr.FieldCount >= 44 Then
                        If Not dr.IsDBNull(43) Then
                            PS.StandardPointLocationCode = dr.GetString(43)
                        End If
                        Dim Ditp As Boolean = False
                        Dim TTp As Boolean = False
                        Dim LeftScanner As String = ""
                        Dim RightScanner As String = ""
                        Dim LData() As String
                        Dim RData() As String
                        Dim tmpStr As String = ""
                        If Not dr.IsDBNull(44) Then Boolean.TryParse(dr.GetValue(44), Ditp)
                        If Not dr.IsDBNull(45) Then Boolean.TryParse(dr.GetValue(45), TTp)
                        With PS
                            .DITP = Ditp
                            .TTP = TTp
                            If Not dr.IsDBNull(46) Then .DistanceToLeftStation = dr.GetValue(46)
                            If Not dr.IsDBNull(47) Then .DistanceToRightStation = dr.GetValue(47)
                            If Not dr.IsDBNull(48) Then .RunningTimeToLeftStation = dr.GetValue(48)
                            If Not dr.IsDBNull(49) Then .RunningTimeToRightStation = dr.GetValue(49)
                            If Not dr.IsDBNull(50) Then .DesignatedArrivalShelfTrackUID = dr.GetValue(50)
                            If Not dr.IsDBNull(51) Then .AutoPostionLeftTrackUID = dr.GetValue(51)
                            If Not dr.IsDBNull(52) Then .AutoPostionRightTrackUID = dr.GetValue(52)
                            If Not dr.IsDBNull(53) Then LeftScanner = dr.GetValue(53)
                            If Not dr.IsDBNull(54) Then RightScanner = dr.GetValue(54)

                            If LeftScanner IsNot Nothing AndAlso Not String.IsNullOrEmpty(LeftScanner) AndAlso LeftScanner.Trim <> "" Then
                                LData = LeftScanner.Split("|")
                                If LData IsNot Nothing AndAlso LData.Length > 0 Then
                                    For Each tmpStr In LData
                                        If tmpStr.Trim.ToUpper <> "NOTHING" Then
                                            If Not .LeftScannerList.Contains(tmpStr) Then .LeftScannerList.Add(tmpStr)
                                        End If
                                    Next
                                End If
                            End If
                            If RightScanner IsNot Nothing AndAlso Not String.IsNullOrEmpty(RightScanner) AndAlso RightScanner.Trim <> "" Then
                                RData = RightScanner.Split("|")
                                If RData IsNot Nothing AndAlso RData.Length > 0 Then
                                    For Each tmpStr In RData
                                        If tmpStr.Trim.ToUpper <> "NOTHING" Then
                                            If Not .RightScannerList.Contains(tmpStr) Then .RightScannerList.Add(tmpStr)
                                        End If
                                    Next
                                End If
                            End If

                            Dim txtColor As Integer = 0
                            If Not dr.IsDBNull(55) Then Integer.TryParse(dr.GetValue(55), txtColor)
                            If txtColor <> 0 Then
                                .myPSColor = Color.FromArgb(CInt(txtColor))
                                .RefreshPath()
                            End If


                        End With


                    End If



                    AddHandler PS.MouseHover, AddressOf PS.PassengerStation_MouseHover
                    PS.BuildContextMenu()
                    gComponentsCollection.Add(PS)
                    gTerritoryComponentsCollection.Add(PS)
                    If Not gPassengerStationCollection.Contains(PS) Then gPassengerStationCollection.Add(PS)

                End While
                dr.Close()
                dbconn.Close()
            End Using


            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        Finally
            If dbcomm IsNot Nothing Then dbcomm.Dispose()
            dbcomm = Nothing
            dr = Nothing
        End Try
    End Function

#End Region

#Region "INITIALIZATION"

    Private Sub CheckToCreateTrackZone(ByVal ZoneName As String, ByVal ZoneUID As Int32, ByVal trk As Track)
        Try
            Dim tz As TrackZone
            Dim ZoneExists As Boolean
            Dim CurrentZone As TrackZone = Nothing
            For Each tz In gTrackZones
                If tz.ZoneName = ZoneName And tz.ZoneUID = ZoneUID Then
                    ZoneExists = True
                    CurrentZone = tz
                    Exit For
                End If
            Next

            If ZoneExists Then
                If Not CurrentZone.TrackList.Contains(trk) Then
                    CurrentZone.TrackList.Add(trk)
                End If
                trk.TrackZoneReference = CurrentZone
                trk.TrackZoneMasterTrack = True
            Else
                Dim NewTZ As New TrackZone
                NewTZ.TrackList.Add(trk)
                If trk.FoulingTrackSPMS Then
                    Dim sw As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(trk)
                    Dim ZoneName1 As String = trk.Name & "-FOULING"
                    If Not IsNothing(sw) Then
                        ZoneName1 = sw.Name
                    End If
                    NewTZ.ZoneName = ZoneName1
                    NewTZ.ZoneUID = trk.GUID
                Else
                    NewTZ.ZoneName = ZoneName
                    NewTZ.ZoneUID = ZoneUID
                End If
                gTrackZones.Add(NewTZ)
                trk.TrackZoneMasterTrack = True
                trk.TrackZoneReference = NewTZ
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitDevices()
        Dim dv As Device
        Try
            For Each dv In TMDS.GlobalDeclarations.DeviceCollection
                If dv.DeviceControlString <> "" Then
                    dv.DeviceControlOn = New ArrayList()
                    dv.DeviceControlOff = New ArrayList()
                    dv.DeviceControlOnOff = New ArrayList()
                    dv.DeviceControlOffOff = New ArrayList()

                    Dim DataType() As String = Trim(dv.DeviceControlString).Split("-")
                    If DataType.Length > 0 Then
                        Dim data() As String = Trim(DataType(0)).Split(";")
                        Dim dataStr As String
                        Dim j As Integer = 0
                        For Each dataStr In data
                            Dim TempArrayList As ArrayList

                            If j = 0 Then
                                TempArrayList = dv.DeviceControlOff
                                j = 1
                            Else
                                TempArrayList = dv.DeviceControlOn
                            End If
                            Dim Devices() As String = dataStr.Split("|")
                            Dim dev As String
                            For Each dev In Devices
                                If IsNumeric(dev) Then
                                    Try
                                        TempArrayList.Add(TMDS.SystemUtilities.FetchData.FindDeviceItemFromGUID(CInt(dev)))
                                    Catch
                                    End Try
                                End If
                            Next
                        Next
                    End If
                    If DataType.Length > 1 Then
                        Dim data() As String = Trim(DataType(1)).Split(";")
                        Dim dataStr As String
                        Dim j As Integer = 0
                        For Each dataStr In data
                            Dim TempArrayList As ArrayList

                            If j = 0 Then
                                TempArrayList = dv.DeviceControlOffOff
                                j = 1
                            Else
                                TempArrayList = dv.DeviceControlOnOff
                            End If
                            Dim Devices() As String = dataStr.Split("|")
                            Dim dev As String
                            For Each dev In Devices
                                If IsNumeric(dev) Then
                                    Try
                                        TempArrayList.Add(TMDS.SystemUtilities.FetchData.FindDeviceItemFromGUID(CInt(dev)))
                                    Catch
                                    End Try
                                End If
                            Next
                        Next
                    End If
                    dv.InitControl(dv.DeviceControlled)
                End If
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Public Sub InitSwitchTWC()
        Try
            Dim sw As Switch
            For Each sw In TMDS.GlobalDeclarations.SwitchCollection
                If Mid(sw.AuthorityType, 1, 3) = "TWC" And Not sw.TypeOfSwitch = "SPM ONLY" Then
                    Dim trk As Track = Nothing
                    If sw.LeftAltLink <> 0 Then
                        trk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.RightLink)
                    Else
                        trk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.LeftLink)
                    End If

                    If Not IsNothing(trk) Then
                        trk.OStype = True
                        trk.GetOsTypeTWCComponents()
                        sw.JointControl = trk.JointControl
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitRPCSwitchTWC()
        Try
            Dim sw As Switch
            For Each sw In TMDS.GlobalDeclarations.SwitchCollection
                Dim LockRight As Boolean = False
                Dim LockLeft As Boolean = False
                Dim LockRestricting As Boolean = False
                If Mid(sw.AuthorityType, 1, 3) = "TWC" Then
                    If sw.TrackAuthorities.Count > 0 And sw.RCPS Then
                        Dim sfa As TrackAuthority.SFAuthority = sw.TrackAuthorities.Item(0)
                        Select Case sfa.AuthType1
                            Case "ItemTwoToTrain"
                                Select Case sfa.Direction
                                    Case "RIGHT"
                                        LockRight = True
                                    Case "LEFT"
                                        LockLeft = True
                                    Case Else
                                        LockRight = True
                                End Select
                            Case "ItemFourToTrain"
                                LockRestricting = True
                        End Select
                        If sw.RCPS Then
                            If LockRight Then
                                sw.LockControlRightDirection.BitState = True
                            ElseIf LockLeft Then
                                sw.LockControlLeftDirection.BitState = True
                            ElseIf LockRestricting Then
                                sw.LockControlBiDirectionRestricting.BitState = True
                            End If
                            sw.myControlPoint.SendCodeStationControlSpecial(True, True)
                        End If
                    End If
                End If
            Next

        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitSwitchPointMonitorsInTWC()

        Try
            Dim sw As Switch
            For Each sw In TMDS.GlobalDeclarations.SwitchCollection
                If sw.SwitchPointerMonitorMode Then
                    sw.InitSwitchPoint()
                End If
            Next

        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitTrackZoneMonitorsInTWC()

        Try
            Dim tz As TrackZone
            For Each tz In TMDS.GlobalDeclarations.gTrackZones
                Dim trk As Track
                For Each trk In tz.TrackList
                    trk.InitTrackZone()
                Next
                tz.ReorderTrackList()
            Next

        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitTrackSubdivisionCorrelation()
        Dim trk As Track
        Try
            For Each trk In TrackCollection
                trk.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(trk.SubdivisionID)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub
    Public Sub InitSignalSubdivisionCorrelation()
        Dim sig As Signal
        Try
            For Each sig In SignalCollection
                sig.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(sig.SubdivisionID)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub
    Public Sub InitSwitchSubdivisionCorrelation()
        Dim Sw As Switch
        Try
            For Each Sw In SwitchCollection
                Sw.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(Sw.SubdivisionID)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitSignalRoutes()
        Dim sr As SignalRoute = Nothing
        Dim BadSignalRoutes As New ArrayList
        Try
            For Each sr In gSignalRouteList
                'SystemEvent(">>>>>>>>>>>>>>>>SR GUID: " & sr.SystemUID & "<<<<<<<<<<<<<<<<<<")
                If optEnableSignalRouteForQuickRoute OrElse optEnableRestrictedSignalRouteForQuickRoute OrElse optEnableGroupedStoreRouteOnly OrElse OptEnableEnhancedPSS Then
                    sr.CreateComponentListNew()
                    If sr.ValidSignalRoute Then
                        UpdateCPwithSR(sr)
                        If sr.myControlPoint IsNot Nothing Then
                            Select Case sr.myDirection
                                Case "RIGHT"
                                    If Not sr.myControlPoint.myRightSignalRoutes.Contains(sr) Then sr.myControlPoint.myRightSignalRoutes.Add(sr)
                                Case Else
                                    If Not sr.myControlPoint.myLeftSignalRoutes.Contains(sr) Then sr.myControlPoint.myLeftSignalRoutes.Add(sr)
                            End Select
                        End If
                    Else
                        If Not BadSignalRoutes.Contains(sr) Then BadSignalRoutes.Add(sr)
                    End If
                Else
                    sr.CreateComponentList()
                End If
            Next

            If BadSignalRoutes IsNot Nothing AndAlso BadSignalRoutes.Count > 0 Then
                Dim Errmsg As New ArrayList
                Dim str As String = ""
                'SystemEvent(">>>>>>>>>>>>>>>>Total Loaded SignalRoutes Count: " & gSignalRouteList.Count & "<<<<<<<<<<<<<<<<<<")
                Errmsg.Add(">>>>>>>>>>>>>>>>Total Loaded SignalRoutes Count: " & gSignalRouteList.Count & "<<<<<<<<<<<<<<<<<<")
                For Each sr In BadSignalRoutes
                    Errmsg.Add(">>>>>>>>>>>>>>>>Removed SR GUID: " & sr.SystemUID & "<<<<<<<<<<<<<<<<<<")
                    If gSignalRouteList.Contains(sr) Then gSignalRouteList.Remove(sr)
                Next
                Errmsg.Add(">>>>>>>>>>>>>>>>Total after removing bad Routes SignalRoutes Count: " & gSignalRouteList.Count & "<<<<<<<<<<<<<<<<<<")
                If Errmsg IsNot Nothing AndAlso Errmsg.Count > 0 Then
                    For Each str In Errmsg
                        SystemEvent(str)
                    Next
                End If
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            sr = Nothing
        End Try
    End Sub

    Public Sub InitGradeCrossingSubdivisionCorrelation()
        Dim c As GradeCrossing
        Try
            For Each c In GradeCrossingCollection
                If c.mySubdivision Is Nothing Then c.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(c.SubdivisionID)
                If Not IsNothing(c.mySubdivision) Then
                    c.Subdivision = c.mySubdivision.Name
                End If
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitSlideZoneSubdivisionCorrelation()
        Dim sz As SlideZone
        Try
            For Each sz In SlideZoneCollection
                sz.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(sz.SubdivisionID)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitFEDSubdivisionCorrelation()
        Dim d As GraphicComponent.FED
        Try
            For Each d In DetectorCollection
                d.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(d.SubdivisionID)
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitOsTracks()

        'This checks all the track to see if they are OS Type Tracks and sets the flag according
        Dim trk As Track
        Dim OsTrk As Object
        Try
            'SET THE TRACKS THAT ARE OS TYPE CANIDATES
            'AND ASSIGN THE OS TRACK COLLECTION

            For Each trk In TrackCollection
                If trk.SlottingTrack Or trk.OSReporting Then
                    trk.OStype = True
                    trk.GetOsTypeComponents()
                End If

                If trk.TrackSpeedMaximum <> 0 Then
                    trk.myTrackSpeedMax = trk.TrackSpeedMaximum
                End If
            Next

            For Each trk In TrackCollection
                If Not gTrackNameList.Contains(trk.TrackName) Then
                    gTrackNameList.Add(trk.TrackName)
                End If
                gTrackNameList.Sort()

                If trk.OStype Then
                    'check to see if this track in the OS colelction is a candidate for placing a train id on it.
                    Dim LeftComp As Object = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(trk.LeftLink)
                    Dim RightComp As Object = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(trk.RightLink)

                    If Not LeftComp Is Nothing Then
                        If LeftComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            Dim sw As Switch = CType(LeftComp, Switch)
                            If sw.RightAltLink = 0 Then
                                trk.TrainIDPlace = True
                                trk.OSReleaseTrackForRollup = True
                                sw.NormalTrackTrailingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.LeftLink)
                                sw.NormalTrackFacingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.RightLink)
                                sw.ReverseTrackTrailingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.LeftAltLink)
                            End If
                            trk.IsLinkedToSwitchLeft = True
                            trk.SwitchLeft = sw
                        End If
                    End If
                    If Not RightComp Is Nothing Then
                        If RightComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                            Dim sw As Switch = CType(RightComp, Switch)
                            If sw.LeftAltLink = 0 Then
                                trk.TrainIDPlace = True
                                trk.OSReleaseTrackForRollup = True
                                sw.NormalTrackTrailingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.RightLink)
                                sw.NormalTrackFacingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.LeftLink)
                                sw.ReverseTrackTrailingPortion = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(sw.RightAltLink)

                            End If
                            trk.isLinkedToSwitchRight = True
                            trk.SwitchRight = sw
                        End If
                    End If

                    If RightComp Is Nothing And LeftComp Is Nothing Then
                        Dim comp As Object
                        Dim switchFound As Boolean
                        If Not IsNothing(trk.OsComponentCollection) Then
                            For Each comp In trk.OsComponentCollection
                                If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                    switchFound = True
                                    Exit For
                                End If
                            Next
                        End If
                        If Not switchFound Then
                            If trk.LY = trk.RY Then
                                trk.TrainIDPlace = True
                            ElseIf trk.LY <> trk.RY Then
                                RightComp = TMDS.SystemUtilities.FetchData.FindCompRRXINGFromGUID(trk.RightLink)
                                If Not IsNothing(RightComp) Then
                                    If RightComp.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then
                                        trk.TrainIDPlace = True
                                        trk.OSReleaseTrackForRollup = True
                                    End If
                                End If
                            End If
                        Else
                            switchFound = False
                        End If
                        '
                    End If
                End If
            Next


            For Each trk In TrackCollection
                If trk.OStype Then
                    If (trk.SlottingTrack Or trk.OSReporting) And trk.TrainIDPlace Then
                        'trk.OStype = True
                        'trk.GetOsTypeComponents()
                        Dim OSShuntTime As Byte = 0
                        For Each OsTrk In trk.OsComponentCollection
                            If OsTrk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                OsTrk.OsComponentCollection = trk.OsComponentCollection
                                If OsTrk.LossOfShunt > OSShuntTime Then
                                    OSShuntTime = OsTrk.LossOfShunt
                                End If
                                'If Not OsTrk Is trk Then
                                If Not OsTrk.TrainIDPlace Then
                                    OsTrk.OSReporting = False
                                    OsTrk.SlottingTrack = False
                                End If
                                If OsTrk.NonTransverable Then
                                    OsTrk.OSReporting = False
                                    OsTrk.SlottingTrack = False
                                End If
                                'Else
                                '    If OsTrk.SlottingTrack Then
                                '        OsTrk.OSReporting = True
                                '    End If
                                'End If
                            ElseIf OsTrk.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                'Dim sw As Switch = CType(OsTrk, Switch)

                                OsTrk.OsComponentCollection = trk.OsComponentCollection

                            End If
                        Next

                        'set shunt time for each track in os collection
                        If OSShuntTime > 0 Then
                            For Each OsTrk In trk.OsComponentCollection
                                If OsTrk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    'OsTrk.LossOfShunt = OSShuntTime
                                    OsTrk.LossOfShuntTimer.Interval = CInt(OSShuntTime) * 1000
                                End If
                            Next
                        End If
                    End If


                    'Find the signals that need slotted as a result of how the signals are
                    'connected to the OS tracks in this collection
                    If trk.TrainIDPlace Then
                        trk.OStype = True
                        trk.SignalSlotCollection = New ArrayList()
                        Dim SignalFound As Signal
                        If Not trk.OsComponentCollection Is Nothing Then
                            For Each OsTrk In trk.OsComponentCollection
                                If OsTrk.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    OsTrk.SlottingTrack = False
                                    'ADDED THIS IN 386
                                    'OsTrk.OSReporting = False
                                    'ADDED
                                    SignalFound = TMDS.SystemUtilities.FetchData.FindSignalLeftFromGUID(OsTrk.RightLink)
                                    If Not SignalFound Is Nothing Then
                                        If Not trk.SignalSlotCollection.Contains(SignalFound) Then
                                            trk.SignalSlotCollection.Add(SignalFound)
                                        End If
                                    End If
                                    SignalFound = TMDS.SystemUtilities.FetchData.FindSignalRightFromGUID(OsTrk.LeftLink)

                                    If Not SignalFound Is Nothing Then
                                        If Not trk.SignalSlotCollection.Contains(SignalFound) Then
                                            trk.SignalSlotCollection.Add(SignalFound)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                        trk.SlottingTrack = True
                    End If

                End If
            Next

            'ADDED ONE MORE CHECK TO ENSURE AN OS COLLECTION ALWAYS HAS ASLOTTING TRACK
            For Each trk In TrackCollection
                If trk.OStype Then
                    Dim HasSlottingTrack As Boolean = False
                    If IsNothing(trk.OsComponentCollection) Then
                        If Not trk.SlottingTrack Then
                            SystemException("NO SLOTTING TRACK DETECTED:" & trk.GUID)
                            trk.SlottingTrack = True
                        End If
                    Else
                        Dim comp As Object
                        For Each comp In trk.OsComponentCollection
                            If comp.GetType.ToString = gTrackComponentName Then
                                Dim t As Track = CType(comp, Track)
                                If t.SlottingTrack Then
                                    HasSlottingTrack = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If

                    If Not HasSlottingTrack Then
                        SystemException("NO SLOTTING TRACK DETECTED:" & trk.GUID)
                        trk.SlottingTrack = True
                    Else
                        HasSlottingTrack = False
                    End If
                End If
            Next

            If optEnablePTCOperations Then
                For Each trk In TrackCollection
                    If trk.OStype Then
                        If trk.OSReleaseTrackForRollup Then
                            Dim comp As Object
                            For Each comp In trk.OsComponentCollection
                                If comp.GetType.ToString = gTrackComponentName Then
                                    Dim t As Track = CType(comp, Track)
                                    If t.GUID <> trk.GUID Then
                                        t.OSReleaseTrackForRollup = False
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Initialize flip flop objects
    ''' </summary>
    Public Sub InitFlipFlops()
        Dim nextComp As Track
        Dim ff As FlipFlop
        Dim loopControlLeft As Integer = 0
        Dim loopControlRight As Integer = 0
        Try
            For Each ff In gFlipFlopCollection
                ff.myTrackLeft = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(ff.LeftLink)
                ff.myTrackRight = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(ff.RightLink)
                If Not IsNothing(ff.myTrackRight) Then
                    ff.myTrackRight.IsLinkToFlipFlopToLeft = True
                    ff.myTrackRight.FlipFlopToLeft = ff
                    nextComp = ff.myTrackRight
                    Do
                        nextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(nextComp.RightLink)
                        If Not IsNothing(nextComp) Then
                            nextComp.IsLinkToFlipFlopToLeft = True
                            nextComp.FlipFlopToLeft = ff
                        Else
                            Exit Do
                        End If
                        If loopControlRight > 1000 Then
                            SystemEvent("InitFlipFlops:1:BAD SETTING FOUND IN THE FLIPFLOP COMPONENT = " & ff.GUID.ToString)
                            Exit Do
                        End If
                        loopControlRight += 1
                    Loop
                End If
                If Not IsNothing(ff.myTrackLeft) Then
                    ff.myTrackLeft.IsLinkToFlipFlopToRight = True
                    ff.myTrackLeft.FlipFlopToRight = ff
                    nextComp = ff.myTrackLeft
                    Do
                        nextComp = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(nextComp.LeftLink)
                        If Not IsNothing(nextComp) Then
                            nextComp.IsLinkToFlipFlopToRight = True
                            nextComp.FlipFlopToRight = ff
                        Else
                            Exit Do
                        End If
                        If loopControlLeft > 1000 Then
                            SystemEvent("InitFlipFlops:2:BAD SETTING FOUND IN THE FLIPFLOP COMPONENT = " & ff.GUID.ToString)
                            Exit Do
                        End If
                        loopControlLeft += 1
                    Loop
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            nextComp = Nothing
            ff = Nothing
            loopControlLeft = Nothing
            loopControlRight = Nothing
        End Try
    End Sub

    Public Sub InitElectricalCircuits()
        Try
            Dim CircuitList As New ArrayList
            Dim el As ElectricalLine
            For Each el In ElectricalLineCollection
                If Not CircuitList.Contains(el.Name) Then
                    CircuitList.Add(el.Name)
                End If
            Next

            Dim ec As ElectricalConnection
            For Each ec In ElectricalConnectionCollection
                If Not CircuitList.Contains(ec.Name) Then
                    CircuitList.Add(ec.Name)
                End If
            Next

            Dim t As Track
            For Each t In TrackCollection
                If Not IsNothing(t.myCatenaryTrack) Then
                    If Not CircuitList.Contains(t.myCatenaryTrackCircuitName) Then
                        CircuitList.Add(t.myCatenaryTrackCircuitName)
                    End If
                End If
            Next

            Dim c As String
            For Each c In CircuitList
                Dim circuit As New ElectricalCircuit
                circuit.myCircuitName = c
                For Each el In ElectricalLineCollection
                    If c = el.Name Then
                        circuit.myCircuitList.Add(el)
                    End If
                Next
                For Each ec In ElectricalConnectionCollection
                    If c = ec.Name Then
                        circuit.myCircuitList.Add(ec)
                    End If
                Next

                For Each t In TrackCollection
                    If t.EnableTractionPowerManagement AndAlso Not IsNothing(t.myCatenaryTrack) Then
                        If t.myCatenaryTrackCircuitName = c Then
                            circuit.myCantenaryList.Add(t.myCatenaryTrack)
                        End If
                    End If
                Next

                ElectricalCircuitCollection.Add(circuit)
            Next

            Dim cr As ElectricalCircuit
            For Each cr In ElectricalCircuitCollection
                Dim comp As Object
                For Each comp In cr.myCircuitList
                    If comp.RightLink <> 0 Then
                        Dim sw As ElectricalSwitch = TMDS.SystemUtilities.FetchData.FindElectricalSwitch(comp.RightLink)
                        If Not IsNothing(sw) Then
                            If Not cr.mySwitchListToRight.Contains(sw) Then
                                cr.mySwitchListToRight.Add(sw)
                                sw.myCircuitToLeft = cr
                            End If
                        End If
                    End If
                    If comp.LeftLink <> 0 Then
                        Dim sw As ElectricalSwitch = TMDS.SystemUtilities.FetchData.FindElectricalSwitch(comp.LeftLink)
                        If Not IsNothing(sw) Then
                            If Not cr.mySwitchListToLeft.Contains(sw) Then
                                cr.mySwitchListToLeft.Add(sw)
                                sw.myCircuitToRight = cr
                            End If
                        End If
                    End If
                Next
                For Each comp In cr.myCantenaryList
                    Dim theTrack As Track = CType(comp.myTrack, Track)
                    If theTrack.AltLinkFour <> 0 Then
                        Dim sw As ElectricalSwitch = TMDS.SystemUtilities.FetchData.FindElectricalSwitch(theTrack.AltLinkFour)
                        If Not IsNothing(sw) Then
                            If Not cr.mySwitchListToRight.Contains(sw) Then
                                cr.mySwitchListToRight.Add(sw)
                                sw.myCircuitToLeft = cr
                            End If
                        End If
                    End If
                    If theTrack.AltLinkThree <> 0 Then
                        Dim sw As ElectricalSwitch = TMDS.SystemUtilities.FetchData.FindElectricalSwitch(theTrack.AltLinkThree)
                        If Not IsNothing(sw) Then
                            If Not cr.mySwitchListToLeft.Contains(sw) Then
                                cr.mySwitchListToLeft.Add(sw)
                                sw.myCircuitToRight = cr
                            End If
                        End If
                    End If

                Next
            Next


        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitHighwayGradeTrackReference()
        Try

            Dim HwyGc As GradeCrossing
            For Each HwyGc In GradeCrossingCollection
                HwyGc.ReferenceToTracks()
                If Integer.TryParse(HwyGc.mySubdivisionUID, Nothing) AndAlso HwyGc.mySubdivisionUID > 0 Then
                    HwyGc.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(HwyGc.mySubdivisionUID)
                End If

            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub CheckForLinkedTrains()
        Try
            Dim trn As TrainID
            For Each trn In TMDS.GlobalDeclarations.TrainCollection
                If trn.TrainLinkedToMe <> "" Then
                    trn.TrainIsLinked = True
                    trn.myTrainIDLinkedToMe = trn.FindTrainID(trn.TrainLinkedToMe)
                    If IsNothing(trn.myTrainIDLinkedToMe) Then
                        trn.TrainIsLinked = False
                        trn.TrainLinkedToMe = ""
                    Else
                        trn.myTrainIDLinkedToMe.thisHelperIsLinkedToThisTrainID = trn
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitRouteBlockingTracks()

        Dim trk As Track
        Dim ListOfRoutes As New ArrayList
        Try
            For Each trk In TrackCollection
                If trk.RouteBlockingEnabled Then
                    If Not ListOfRoutes.Contains(trk.RouteName) Then
                        ListOfRoutes.Add(trk.RouteName)
                    End If
                End If
            Next

            Dim rte As String
            For Each rte In ListOfRoutes
                Dim TrkList As New ArrayList
                For Each trk In TrackCollection
                    If trk.RouteName = rte Then
                        TrkList.Add(trk)
                    End If
                Next

                For Each trk In TrackCollection
                    If trk.RouteName = rte Then
                        trk.RouteList = TrkList.Clone
                    End If
                Next
                TrkList = Nothing
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitMainTrackEntrancePoints()
        If Not (optEnableMenuPermissionToEnterMainTrack Or optEnableCPPermissionToEnterMainTrack) Then Exit Sub
        Try
            Dim trk As Track
            For Each trk In TrackCollection
                If trk.sMainTrackEntrancePoints <> "" Then
                    trk.MainTrackEntrancePoints = New ArrayList
                    Dim switches() As String = trk.sMainTrackEntrancePoints.Split("|")
                    Dim s As String
                    For Each s In switches
                        If IsNumeric(s) Then
                            Dim sw As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(CInt(s))
                            If Not IsNothing(sw) Then
                                If Not trk.MainTrackEntrancePoints.Contains(sw) Then
                                    trk.MainTrackEntrancePoints.Add(sw)
                                End If
                            End If
                        End If
                    Next
                End If
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitEntryExitTrackBindingToSubdivisionStations()
        Try
            Dim subdvn As Subdivision
            For Each subdvn In ControlledSubdivisions
                Dim stat As SubdivisionStation
                For Each stat In subdvn.Stations
                    If Trim(stat.EntryReportingLeftBoundTracks) <> "" Then
                        Dim trks() As String = stat.EntryReportingLeftBoundTracks.Split("|")
                        Dim t As String
                        For Each t In trks
                            If IsNumeric(t) Then
                                If IsNothing(stat.ReportingEntryTracksForLeftBoundMovements) Then stat.ReportingEntryTracksForLeftBoundMovements = New ArrayList
                                Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(t))
                                If Not IsNothing(trk) Then
                                    Dim os As Object
                                    Dim theReportingTrack As Track = Nothing
                                    If Not IsNothing(trk.OsComponentCollection) Then
                                        For Each os In trk.OsComponentCollection
                                            If os.GetType.ToString = gTrackComponentName Then
                                                Dim OsTrk As Track = CType(os, Track)
                                                If OsTrk.OSReporting And OsTrk.TrainIDPlace Then
                                                    theReportingTrack = OsTrk
                                                    If OsTrk.GUID = trk.GUID Then
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                    If trk.IsPositionReportingTrack Then
                                        theReportingTrack = trk
                                    End If
                                    If Not IsNothing(theReportingTrack) Then
                                        stat.ReportingEntryTracksForLeftBoundMovements.Add(theReportingTrack)
                                        theReportingTrack.IsReporingStationEntryLeftBoundMovements = True
                                        theReportingTrack.myReportingStation = stat
                                    End If
                                Else
                                    SystemEvent("InitEntryExitTrackBindingToSubdivisionStations:TRACK NOT FOUND:" & t)
                                End If
                            End If
                        Next
                    End If
                    If Trim(stat.ExitReportingLeftBoundTracks) <> "" Then
                        Dim trks() As String = stat.ExitReportingLeftBoundTracks.Split("|")
                        Dim t As String
                        For Each t In trks
                            If IsNumeric(t) Then
                                If IsNothing(stat.ReportingExitTracksForLeftBoundMovements) Then stat.ReportingExitTracksForLeftBoundMovements = New ArrayList
                                Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(t))
                                If Not IsNothing(trk) Then
                                    Dim os As Object
                                    Dim theReportingTrack As Track = Nothing
                                    If Not IsNothing(trk.OsComponentCollection) Then
                                        For Each os In trk.OsComponentCollection
                                            If os.GetType.ToString = gTrackComponentName Then
                                                Dim OsTrk As Track = CType(os, Track)
                                                If OsTrk.OSReporting And OsTrk.TrainIDPlace Then
                                                    theReportingTrack = OsTrk
                                                    If OsTrk.GUID = trk.GUID Then
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                    If trk.IsPositionReportingTrack Then
                                        theReportingTrack = trk
                                    End If
                                    If Not IsNothing(theReportingTrack) Then
                                        stat.ReportingExitTracksForLeftBoundMovements.Add(theReportingTrack)
                                        theReportingTrack.IsReporingStationExitLeftBoundMovements = True
                                        theReportingTrack.myReportingStation = stat
                                    End If
                                End If
                            End If
                        Next
                    End If

                    If Trim(stat.EntryReportingRightBoundTracks) <> "" Then
                        Dim trks() As String = stat.EntryReportingRightBoundTracks.Split("|")
                        Dim t As String
                        For Each t In trks
                            If IsNumeric(t) Then
                                If IsNothing(stat.ReportingEntryTracksForRightBoundMovements) Then stat.ReportingEntryTracksForRightBoundMovements = New ArrayList
                                Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(t))
                                If Not IsNothing(trk) Then
                                    Dim os As Object
                                    Dim theReportingTrack As Track = Nothing
                                    If Not IsNothing(trk.OsComponentCollection) Then
                                        For Each os In trk.OsComponentCollection
                                            If os.GetType.ToString = gTrackComponentName Then
                                                Dim OsTrk As Track = CType(os, Track)
                                                If OsTrk.OSReporting And OsTrk.TrainIDPlace Then
                                                    theReportingTrack = OsTrk
                                                    If OsTrk.GUID = trk.GUID Then
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                    If trk.IsPositionReportingTrack Then
                                        theReportingTrack = trk
                                    End If
                                    If Not IsNothing(theReportingTrack) Then
                                        stat.ReportingEntryTracksForRightBoundMovements.Add(theReportingTrack)
                                        theReportingTrack.IsReporingStationEntryRightBoundMovements = True
                                        theReportingTrack.myReportingStation = stat
                                    End If
                                End If
                            End If

                        Next
                    End If

                    If Trim(stat.ExitReportingRightBoundTracks) <> "" Then
                        Dim trks() As String = stat.ExitReportingRightBoundTracks.Split("|")
                        Dim t As String
                        For Each t In trks
                            If IsNumeric(t) Then
                                If IsNothing(stat.ReportingExitTracksForRightBoundMovements) Then stat.ReportingExitTracksForRightBoundMovements = New ArrayList
                                Dim trk As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(CInt(t))

                                If Not IsNothing(trk) Then
                                    Dim os As Object
                                    Dim theReportingTrack As Track = Nothing
                                    If Not IsNothing(trk.OsComponentCollection) Then
                                        For Each os In trk.OsComponentCollection
                                            If os.GetType.ToString = gTrackComponentName Then
                                                Dim OsTrk As Track = CType(os, Track)
                                                If OsTrk.OSReporting And OsTrk.TrainIDPlace Then
                                                    theReportingTrack = OsTrk
                                                    If OsTrk.GUID = trk.GUID Then
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                    If trk.IsPositionReportingTrack Then
                                        theReportingTrack = trk
                                    End If
                                    If Not IsNothing(theReportingTrack) Then
                                        stat.ReportingExitTracksForRightBoundMovements.Add(theReportingTrack)
                                        theReportingTrack.IsReporingStationExitRightBoundMovements = True
                                        theReportingTrack.myReportingStation = stat
                                    End If
                                End If
                            End If
                        Next
                    End If
                    Dim trck As Track

                    If Not IsNothing(stat.ReportingEntryTracksForLeftBoundMovements) Then
                        For Each trck In stat.ReportingEntryTracksForLeftBoundMovements
                            If stat.ReportingEntryTracksForLeftBoundMovements.Count > 1 Then
                                trck.HasMultipleExitPoints = True
                            Else
                                CheckForMultipleExitPoints(trck, stat.ReportingExitTracksForLeftBoundMovements)
                            End If
                        Next
                    End If

                    If Not IsNothing(stat.ReportingEntryTracksForRightBoundMovements) Then
                        For Each trck In stat.ReportingEntryTracksForRightBoundMovements
                            If stat.ReportingEntryTracksForRightBoundMovements.Count > 1 Then
                                trck.HasMultipleExitPoints = True
                            Else
                                CheckForMultipleExitPoints(trck, stat.ReportingExitTracksForRightBoundMovements)
                            End If
                        Next
                    End If
                Next
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Sub CheckForMultipleExitPoints(ByVal theTrack As Track, ByVal ExitList As ArrayList)
        Try
            If ExitList Is Nothing Then Exit Sub
            Dim trk As Track
            For Each trk In ExitList
                If trk.TrackName <> theTrack.TrackName Then
                    theTrack.HasMultipleExitPoints = True
                    Exit For
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitTracksForTractionPowerManagement()
        Try
            Dim t As Track
            For Each t In TrackCollection
                If t.EnableTractionPowerManagement Then
                    t.InitCatenaryTrack()
                End If
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitPassegerStationSubdivisionCorrelation()
        Dim PS As PassengerStation
        Try
            For Each PS In gPassengerStationCollection
                PS.mySubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(PS.SubdivisionID)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Sub UpdateCPwithSR(SR As SignalRoute)
        Dim tmpCP As ControlPoint = Nothing
        Dim CP As ControlPoint = Nothing
        Try
            If SR Is Nothing Then Exit Sub
            If SR.EntrySignal IsNot Nothing AndAlso SR.EntrySignal.myControlPoint IsNot Nothing Then
                CP = SR.EntrySignal.myControlPoint
            Else
                Exit Sub
            End If
            If CP Is Nothing Then Exit Sub
            If gControlPointCollection IsNot Nothing Then
                For Each tmpCP In gControlPointCollection
                    If CP.GUID = tmpCP.GUID Then
                        Select Case SR.myDirection
                            Case "RIGHT"
                                If tmpCP.myRightSignalRoutes Is Nothing Then tmpCP.myRightSignalRoutes = New List(Of SignalRoute)
                                If Not tmpCP.myRightSignalRoutes.Contains(SR) Then tmpCP.myRightSignalRoutes.Add(SR)
                            Case Else
                                If tmpCP.myLeftSignalRoutes Is Nothing Then tmpCP.myLeftSignalRoutes = New List(Of SignalRoute)
                                If Not tmpCP.myLeftSignalRoutes.Contains(SR) Then tmpCP.myLeftSignalRoutes.Add(SR)
                        End Select
                    End If
                Next
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub
#End Region

#Region "LOCAL-REMOTE TRACK LINKING"

    Public Delegate Sub DoCreateRemoteLink(ByVal GUID As Int32, ByVal LeftLink As Int32, ByVal RightLink As Int32, ByVal FlipFlopper As Int32)

    Public Sub InitTrackLinkageShadowMode()
        Try
            If optEnableShadowModeRemoteLink Then
                InitRemoteTrackLinkageShadowMode()
                Exit Sub
            End If

            Dim trk, trkToFind As Track
            For Each trk In TMDS.GlobalDeclarations.gTracksNeedingLinks
                If trk.NeedsLinking Then
                    If trk.AltLinkOne <> 0 Then
                        For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection
                            If trkToFind.GUID = trk.AltLinkOne Then
                                trk.LeftLink = trkToFind.GUID
                                trk.NeedsLinking = False
                                trkToFind.RightLink = trk.GUID
                                trkToFind.NeedsLinking = False
                                'trkToFind.FlipMeLeft = True
                                'trkToFind.FlipMe = True
                                Exit For
                            End If
                        Next
                    Else
                        If trk.AltLinkTwo <> 0 Then
                            For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection
                                If trkToFind.GUID = trk.AltLinkTwo Then
                                    If trkToFind.AltLinkTwo = 0 Then
                                        trk.RightLink = trkToFind.GUID
                                        trk.NeedsLinking = False
                                        trkToFind.LeftLink = trk.GUID
                                        trkToFind.NeedsLinking = False
                                        trk.AltLinkTwo = 0
                                    Else
                                        trk.RightLink = trkToFind.GUID
                                        trk.NeedsLinking = False
                                        trk.AltLinkOne = 0
                                        trkToFind.RightLink = trk.GUID
                                        trkToFind.NeedsLinking = False
                                        trkToFind.FlipMeRight = True
                                        trk.FlipMeRight = True
                                    End If

                                End If
                            Next
                        End If
                    End If

                    If trk.NeedsLinking AndAlso Not (gAssistMode) Then 'OrElse trk.IsShadowMode
                        Dim LinkUID, LeftLink, RightLink As Int32
                        Dim FlipFlopper As Int32
                        If trk.AltLinkOne <> 0 Then
                            LinkUID = trk.AltLinkOne
                            RightLink = trk.GUID
                            trk.LeftLink = LinkUID
                            LeftLink = 0
                            FlipFlopper = trk.LeftAltLink
                        ElseIf trk.AltLinkTwo <> 0 Then
                            LinkUID = trk.AltLinkTwo
                            LeftLink = trk.GUID
                            trk.RightLink = LinkUID
                            RightLink = 0
                            FlipFlopper = trk.RightAltLink
                        End If
                        trk.IsLinkToRemoteTrack = True
                        CreateRemoteTrackLink(LinkUID, LeftLink, RightLink, FlipFlopper, trk)
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitTrackLinkage()
        Try


            Dim trk, trkToFind As Track
            For Each trk In TMDS.GlobalDeclarations.gTracksNeedingLinks
                If trk.NeedsLinking Then
                    If trk.AltLinkOne <> 0 Then
                        For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection
                            If trkToFind.GUID = trk.AltLinkOne Then
                                trk.LeftLink = trkToFind.GUID
                                trk.NeedsLinking = False
                                trkToFind.RightLink = trk.GUID
                                trkToFind.NeedsLinking = False
                                Exit For
                            End If
                        Next
                    Else
                        If trk.AltLinkTwo <> 0 Then
                            For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection

                                If trkToFind.GUID = trk.AltLinkTwo Then

                                    If trkToFind.AltLinkTwo = 0 Then
                                        trk.RightLink = trkToFind.GUID
                                        trk.NeedsLinking = False
                                        trkToFind.LeftLink = trk.GUID
                                        trkToFind.NeedsLinking = False
                                    Else
                                        trk.RightLink = trkToFind.GUID
                                        trk.NeedsLinking = False
                                        trkToFind.RightLink = trk.GUID
                                        trkToFind.NeedsLinking = False
                                        trkToFind.FlipMeRight = True
                                        trk.FlipMeRight = True
                                    End If

                                End If
                            Next
                        End If
                    End If

                    If trk.NeedsLinking And Not (gAssistMode) OrElse trk.IsShadowMode Then
                        Dim LinkUID, LeftLink, RightLink As Int32
                        Dim FlipFlopper As Int32
                        If trk.AltLinkOne <> 0 Then
                            LinkUID = trk.AltLinkOne
                            RightLink = trk.GUID
                            trk.LeftLink = LinkUID
                            LeftLink = 0
                            FlipFlopper = trk.LeftAltLink
                        Else
                            LinkUID = trk.AltLinkTwo
                            LeftLink = trk.GUID
                            trk.RightLink = LinkUID
                            RightLink = 0
                            FlipFlopper = trk.RightAltLink
                        End If
                        trk.IsLinkToRemoteTrack = True
                        'Dim CreateRemoteLink As New DoCreateRemoteLink(AddressOf CreateRemoteTrackLink)
                        'CreateRemoteLink.BeginInvoke(LinkUID, LeftLink, RightLink, FlipFlopper, Nothing, Nothing)
                        CreateRemoteTrackLink(LinkUID, LeftLink, RightLink, FlipFlopper, trk)
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Create Remote Track Link
    ''' </summary>
    ''' <param name="uidLink">uid link</param>
    ''' <param name="leftLink">left link</param>
    ''' <param name="rightLink">right link</param>
    ''' <param name="flipFlopLink">flip flop link</param>
    ''' <param name="theTrack">track object</param>
    Private Sub CreateRemoteTrackLink(uidLink As Integer,
                                      leftLink As Integer,
                                      rightLink As Integer,
                                      flipFlopLink As Integer,
                                      theTrack As Track)
        Try
            Dim remoteTrack As New RemoteTrack(uidLink, leftLink, rightLink, flipFlopLink, theTrack)
            If remoteTrack IsNot Nothing Then gRemoteLinkTracks.Add(remoteTrack)
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub InitRemoteTrackLinkageShadowMode()
        Dim tmpArray As New ArrayList
        Dim trk, trkToFind As Track
        Try
            CopyArrayCollection(tmpArray, TMDS.GlobalDeclarations.gTracksNeedingLinks)
            For Each trk In tmpArray
                If trk.IsShadowMode Then
                    TMDS.GlobalDeclarations.gTracksNeedingLinks.Remove(trk)
                End If
            Next


            For Each trk In TMDS.GlobalDeclarations.gTracksNeedingLinks
                If trk.NeedsLinking Then
                    If trk.AltLinkOne <> 0 Then
                        For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection
                            If trkToFind.GUID = trk.AltLinkOne Then
                                If (trkToFind.IsShadowMode AndAlso trk.IsShadowMode) Or (Not trkToFind.IsShadowMode AndAlso Not trk.IsShadowMode) Then
                                    trk.LeftLink = trkToFind.GUID
                                    trk.NeedsLinking = False
                                    trkToFind.RightLink = trk.GUID
                                    trkToFind.NeedsLinking = False
                                    'trkToFind.FlipMeLeft = True
                                    'trkToFind.FlipMe = True
                                    Exit For
                                End If
                            End If
                        Next
                    Else
                        If trk.AltLinkTwo <> 0 Then
                            For Each trkToFind In TMDS.GlobalDeclarations.TrackCollection
                                If trkToFind.GUID = trk.AltLinkTwo Then

                                    If trkToFind.AltLinkTwo = 0 Then
                                        If (trkToFind.IsShadowMode AndAlso trk.IsShadowMode) Or (Not trkToFind.IsShadowMode AndAlso Not trk.IsShadowMode) Then
                                            trk.RightLink = trkToFind.GUID
                                            trk.NeedsLinking = False
                                            trkToFind.LeftLink = trk.GUID
                                            trkToFind.NeedsLinking = False
                                            trk.AltLinkTwo = 0
                                        End If
                                    Else
                                        If (trkToFind.IsShadowMode AndAlso trk.IsShadowMode) Or (Not trkToFind.IsShadowMode AndAlso Not trk.IsShadowMode) Then
                                            trk.RightLink = trkToFind.GUID
                                            trk.NeedsLinking = False
                                            trk.AltLinkOne = 0
                                            trkToFind.RightLink = trk.GUID
                                            trkToFind.NeedsLinking = False
                                            trkToFind.FlipMeRight = True
                                            trk.FlipMeRight = True
                                        End If
                                    End If

                                End If
                            Next
                        End If
                    End If

                    If trk.NeedsLinking AndAlso Not (gAssistMode) Then 'OrElse trk.IsShadowMode
                        Dim LinkUID, LeftLink, RightLink As Int32
                        Dim FlipFlopper As Int32
                        If trk.AltLinkOne <> 0 Then
                            LinkUID = trk.AltLinkOne
                            RightLink = trk.GUID
                            trk.LeftLink = LinkUID
                            LeftLink = 0
                            FlipFlopper = trk.LeftAltLink
                        ElseIf trk.AltLinkTwo <> 0 Then
                            LinkUID = trk.AltLinkTwo
                            LeftLink = trk.GUID
                            trk.RightLink = LinkUID
                            RightLink = 0
                            FlipFlopper = trk.RightAltLink
                        End If
                        trk.IsLinkToRemoteTrack = True
                        'Dim CreateRemoteLink As New DoCreateRemoteLink(AddressOf CreateRemoteTrackLink)
                        'CreateRemoteLink.BeginInvoke(LinkUID, LeftLink, RightLink, FlipFlopper, Nothing, Nothing)
                        CreateRemoteTrackLink(LinkUID, LeftLink, RightLink, FlipFlopper, trk)
                    End If
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

#End Region

#Region "GRAPHIC ROUTINES"

    Delegate Sub BuildFormGraphics()

    Sub BuildGraphicsAll()
        Dim comp As Object
        Dim note As Annotation = Nothing
        Dim StartTime As Date = Now
        Dim TotalTimeProcessing, TotalDrawing As Int32
        Dim RunLength As System.TimeSpan = Now.Subtract(StartTime)
        Dim displayform As frmDisplay
        Try

            For Each displayform In TMDS.GlobalDeclarations.gDisplayFormCollection
                displayform.Initializing = True
                displayform.Cursor = System.Windows.Forms.Cursors.WaitCursor
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

        Try
            Dim ScreenName As String = String.Empty
            For Each comp In gComponentsCollection
                Application.DoEvents()

                If Not comp.CurrentDrawingForm Is Nothing Then
                    If ScreenName <> comp.CurrentDrawingForm.Text Then
                        ScreenName = comp.CurrentDrawingForm.Text
                        gSplashScreenString = "BUILDING GRAPHICS FOR - " & ScreenName
                    End If
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myTracksAndSwitches.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myTracksAndSwitches.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.ControlPoint" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myControlPoints.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.Device" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myOtherComponents.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.BitMapPicture" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.mySignals.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.GradeCrossing" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myGradeCrossings.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.SlideZone" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.mySlideZones.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.ElectricalSwitch" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myElectricalSwitches.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.ElectricalLine" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myElectricalLines.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.ElectricalConnection" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myElectricalConnections.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.WeatherStation" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myWeatherStations.Add(comp)
                    ElseIf comp.GetType.ToString = "TMDS.GraphicComponent.FED" Then
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myFEDs.Add(comp)
                    Else
                        comp.CurrentDrawingForm.Controls.Add(comp)
                        comp.CurrentDrawingForm.myTextDisplay.Add(comp)
                    End If

                    If Not comp.CurrentDrawingForm.TerritoryList.Contains(comp.TerritoryID) Then
                        comp.CurrentDrawingForm.TerritoryList.Add(comp.TerritoryID)
                    End If
                End If

                If gMonitorMode Then
                    comp.DoMonitorMode = gMonitorMode
                End If
                If Not comp.CurrentDrawingForm Is Nothing Then
                    StartTime = Now
                    comp.CurrentDrawingForm.myComponents.Add(comp)
                    comp.Graphical = comp.CurrentDrawingForm.CreateGraphics
                    comp.BuildGraphics()
                    comp.InitGraphics()
                    comp.GlobalFlasherCollectionReference = FlashCollection
                End If
                If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                    StartTime = Now
                    Dim sig1 As Signal = CType(comp, Signal)
                    sig1.BrotherSignalCheck()
                    sig1.SignalSharedStopBitCheck()
                End If
                If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                    StartTime = Now
                    comp.CheckForBrotherSwitches()
                End If
                comp.myTerritory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(comp.TerritoryID)
            Next

            gSplashScreenString = "PERFORMING FINAL GRAPHICS PROCESSING..."

            SystemEvent("TOTAL PROCESSING:" & TotalTimeProcessing.ToString)
            SystemEvent("TOTAL DRAWING:" & TotalDrawing.ToString)
            For Each comp In Notes
                note.BuildGraphics()
            Next

            Dim trk As Track
            For Each trk In TMDS.GlobalDeclarations.TrackCollection
                trk.Visible = True
                If trk.AuthorityType = "NON-TWC" Then
                    trk.SendToBack()
                End If
                If optEnableCPMenus Then
                    trk.BuildContextMenuCP()
                Else
                    trk.BuildContextMenu()
                End If
                trk.CheckForRemoteTrackLinkFlippedSituation()
                trk.Refresh()
            Next

            Dim sw As Switch
            For Each sw In TMDS.GlobalDeclarations.SwitchCollection
                sw.BuildContextMenu()
                sw.Visible = True
                If optEnableHideHandSwitchesOnLoad Then
                    If gSPMSwitchCollection.Contains(sw) Then
                        sw.Visible = False
                    End If
                End If
                sw.Refresh()
            Next

            Dim sig As Signal
            For Each sig In TMDS.GlobalDeclarations.SignalCollection
                If sig.SignalSlotting Then
                    sig.InitHoldingSignalOs()
                End If
                sig.BuildContextMenu()
                sig.Visible = True
                If optEnableHideAutomaticSignalsOnLoad AndAlso sig.AutomaticSignal Then
                    sig.Visible = False
                End If
                sig.Refresh()
            Next

            Dim cp As ControlPoint
            For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                cp.InitializeTrackAssignment()
                If optEnableTractionPowerManagementSystem AndAlso (cp.ScadaTractionPowerControlPointMode Or cp.ScadaCommunicationControlPointMode) Then
                    cp.Visible = False
                Else
                    cp.Visible = True
                End If
                cp.Refresh()
            Next

            Dim dv As Device
            For Each dv In DeviceCollection
                If (dv.ControlIndicate Or dv.ControlOnly) And optEnableDeviceBlockMenuOnMiscDevices Then
                    dv.BuildContextMenu()
                End If
            Next

            Dim es As ElectricalSwitch
            For Each es In ElectricalSwitchCollection
                es.BuildContextMenu()
                ' es.InitTrackCircuitAssociation()
            Next

            Dim sz As SlideZone
            For Each sz In SlideZoneCollection
                sz.BuildContextMenu()
            Next

            Dim fd As FED
            For Each fd In DetectorCollection
                fd.BuildContextMenu()
            Next

            Dim hc As GradeCrossing
            For Each hc In GradeCrossingCollection
                hc.BuildContextMenu()
            Next

            Dim ws As WeatherStation
            If WeatherStationCollection IsNot Nothing AndAlso WeatherStationCollection.Count > 0 Then
                If optEnableMetroLinkSpecificOptions Then optWeatherReminderEnabled = True
                If optEnableWeatherStationMenu Then
                    For Each ws In WeatherStationCollection
                        ws.BuildContextMenu()
                    Next
                End If

            End If

            For Each displayform In TMDS.GlobalDeclarations.gDisplayFormCollection
                displayform.SetControlsToToolTip()
                displayform.Cursor = System.Windows.Forms.Cursors.Default
                If optDisplayScreenName Then
                    displayform.RenameFormText()
                ElseIf optDisplayScreenNameWithSubdivision Then
                    displayform.RenameFormTextWithSubdivisionname()
                End If
                displayform.Initializing = False
                displayform.Refresh()
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub HideAllComponents()
        Try

            Dim trk As Track
            For Each trk In TMDS.GlobalDeclarations.TrackCollection
                trk.Visible = False
                trk.Refresh()
            Next

            Dim sw As Switch
            For Each sw In TMDS.GlobalDeclarations.SwitchCollection
                sw.Visible = False
                sw.Refresh()
                If Not IsNothing(sw.mySwitchLock) Then
                    sw.mySwitchLock.Visible = False
                End If
            Next

            Dim sig As Signal
            For Each sig In TMDS.GlobalDeclarations.SignalCollection
                sig.Visible = False
                sig.Refresh()
            Next

            Dim cp As ControlPoint
            For Each cp In TMDS.GlobalDeclarations.gControlPointCollection
                cp.Visible = False
                cp.Refresh()
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

#End Region

#Region "GLOBAL MISC FUNCTIONS "
    Public Function GetExceptionAllFrameMsg(ByRef ex As Exception, ByRef st As StackTrace) As String
        'ARRC 1567 -- use this function to collect all frame information for exceptio log
        Try
            If optEnableARRCSpecificOptions Then
            ElseIf optEnableBRCSpecificChecks AndAlso gAssistMode Then
            Else
                Return ex.ToString
            End If
            ' If Not optEnableARRCSpecificOptions Then Return ex.ToString
            Dim k As Integer
            Dim s As StackFrame
            Dim StackMsg As String = ex.ToString
            Dim allFrame As StackFrame() = st.GetFrames()
            For k = 0 To allFrame.Count - 1
                s = allFrame(k)
                Dim filename As String = s.GetFileName
                If filename Is Nothing Then Exit For
                StackMsg = StackMsg & vbCrLf & s.ToString
            Next
            Return StackMsg
        Catch exNow As Exception
            Return ex.ToString
        End Try

    End Function

    ''' <summary>
    ''' Log event log
    ''' </summary>
    ''' <param name="message">message to write in log</param>
    Public Sub LogEvent(message As String)
        SystemEvent(message)
    End Sub

    Public Function CheckAuthTerrExists(ByVal TerrID1 As Integer) As Boolean
        Dim terr As Territory = Nothing
        Try
            For Each terr In ControlledTerritories
                If terr.TerritoryID = TerrID1 Then
                    Return True
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            terr = Nothing
        End Try
        Return False
    End Function

    Public Function ChkStrDBNull(ByVal val As Object) As Object
        Try
            If IsDBNull(val) Then
                Return String.Empty
            Else
                Return val
            End If
        Catch ex As Exception
        End Try
        Return String.Empty
    End Function

    Public Function IsValidDataset(ByVal ds As DataSet) As Boolean
        Dim ReturnValue As Boolean = False
        Try
            If Not IsNothing(ds) Then
                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count > 0 Then
                        ReturnValue = True
                    End If
                End If
            End If
        Catch ex As Exception
        End Try

        Return ReturnValue
    End Function

    Public Function GetProperStringFormatForDatabase(ByVal strToFormat As String) As String
        Dim i As Integer
        Dim ch As String
        Dim sFormatProperStringForDatabase As String = String.Empty
        Try
            For i = 1 To strToFormat.Length
                ch = Mid(strToFormat, i, 1)
                Select Case ch
                    'removed "-", ":", ";", "/", "\", "|", ">", "<", "[", "]", "{", "}""=", "*", "#", "@", "&", "!", "_", "+", ")", "(", "^", "%", "$", "~", 
                    Case "'", "`", Chr(34), "?"
                        sFormatProperStringForDatabase = sFormatProperStringForDatabase & " "
                    Case Else
                        sFormatProperStringForDatabase = sFormatProperStringForDatabase & ch
                End Select
            Next
            Return sFormatProperStringForDatabase
        Catch ex As Exception
            SystemException(ex.ToString)
            Return String.Empty
        End Try
    End Function

    Public Sub LoadRestrictedColorList()
        ColorList.Add("RED")
        ColorList.Add("YELLOW")
        ColorList.Add("YELLOW")
    End Sub

    Public Function GetUnitColorDefinition(ByVal FailCode As String) As Color
        Select Case FailCode.Trim.ToUpper
            Case "N"
                Return Color.Black
            Case "F"
                Return Color.Red
            Case "P"
                Return Color.Blue
            Case "D"
                Return Color.Goldenrod
        End Select
        Return Color.Black
    End Function

    Public Function GetInstalledPrograms() As Boolean
        Try
            Dim localmachine As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine
            Dim Uninstall As Microsoft.Win32.RegistryKey = localmachine.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Uninstall")
            For Each key As String In Uninstall.GetSubKeyNames()
                Dim activeKey As Microsoft.Win32.RegistryKey = Uninstall.OpenSubKey(key)
                Dim displayName As String = DirectCast(activeKey.GetValue("DisplayName"), String)
                If Not [String].IsNullOrEmpty(displayName) Then
                    If displayName.Trim.ToUpper = "WINDOWS MEDIA ENCODER 9 SERIES" Then
                        Return True
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Public Function CheckFileExists(ByVal Filename As String) As Boolean
        If File.Exists(Filename) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function checkAlphaNumeric(ByVal strInputText As String) As Boolean
        Dim intCounter As Integer
        Dim strCompare As String
        Dim strInput As String
        Dim InputAlpha As Integer
        Dim InputNum As Integer
        Try
            For intCounter = 1 To Len(strInputText)
                strCompare = Mid$(strInputText, intCounter, 1)
                strInput = Mid$(strInputText, intCounter + 1, Len(strInputText))
                If strCompare Like ("[A-Z]") Or strCompare Like ("[a-z]") Then
                    InputAlpha = 1
                ElseIf strCompare Like ("#") Then
                    InputNum = 1
                End If
                If InputNum = 1 And InputAlpha = 1 Then
                    Return True
                End If
            Next intCounter
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try
    End Function

    Public Function Truncate(ByVal NumberToTruncate As Decimal, Optional ByVal NumberofDecimals As Integer = 2) As Decimal
        Try
            Dim tp As Decimal = NumberToTruncate
            Dim first As Int32 = 0
            Dim second As String = ""
            Dim s() As String = tp.ToString.Split(".")
            If Not String.IsNullOrEmpty(s(0)) Then
                first = Fix(s(0))
            End If
            If s.GetUpperBound(0) > 0 Then
                If Not String.IsNullOrEmpty(s(1)) Then
                    If s(1).Length >= NumberofDecimals Then
                        second = s(1).Substring(0, NumberofDecimals)
                    Else
                        second = s(1)
                    End If
                End If
            End If
            Dim TPs As String = ""
            If second.Trim = "" Then
                Select Case NumberofDecimals
                    Case 0
                        TPs = first
                    Case 1
                        second = "0"
                        TPs = first & "." & second
                    Case 2
                        second = "00"
                        TPs = first & "." & second
                    Case 3
                        second = "000"
                        TPs = first & "." & second
                    Case Else
                        second = "00"
                        TPs = first & "." & second
                End Select
            Else
                TPs = first & "." & second
            End If
            Return CDec(TPs)
        Catch ex As Exception
            GetErrorDetails(ex)
            Return NumberToTruncate
        End Try

    End Function

    Public Function SplitBySize(ByVal strInput As String, ByVal iSize As Integer) As Integer

        Dim j As Integer = 0, i As Integer
        Try
            Dim strA() As String
            Dim iLength As Integer = strInput.Length()
            Dim iWords As Integer = iLength / iSize + IIf((iLength Mod iSize <> 0), 1, 0)
            ReDim strA(iWords)

            For i = 0 To iLength Step iSize
                strA(j) = Mid(strInput, i + 1, iSize)
                'MsgBox(strA(j))
                j = j + 1
            Next i
            Return j
        Catch ex As Exception
            SystemException(ex.ToString)
            Return -1
        End Try

    End Function

    Public Function WrapText(ByVal str As String, ByVal WrapLength As Integer, Optional ByVal ShowLineBreaks As Boolean = True) As ArrayList

        Dim LineBuilder As New StringBuilder(WrapLength)
        Dim wrd As String = ""
        Dim ReturnstrArray As New ArrayList
        Try
            If ShowLineBreaks Then
                'Dim Lines() As String = str.Split(CChar(Environment.NewLine))
                Dim Lines() As String = str.Split(New [Char]() {Chr(10), CChar(Environment.NewLine)})
                Dim EachLine As String = ""
                For Each EachLine In Lines
                    wrd = ""
                    Dim Linewords() As String = EachLine.Split(" ")

                    For Each wrd In Linewords
                        If wrd.Length >= WrapLength - 1 Then
                            Dim CharCount As Integer = 0
                            Dim w As String
                            Dim LnArray As New ArrayList
                            Dim line As New StringBuilder
                            For Each w In wrd
                                w = w.Replace(vbCrLf, " ")
                                If CharCount + w.Length < WrapLength - 1 Then
                                    line.Append(w.Trim)
                                    CharCount = CharCount + w.Length
                                Else

                                    LnArray.Add(line.ToString.Trim)
                                    line = New StringBuilder
                                    line.Append(w.Trim)
                                    CharCount = w.Length


                                End If
                            Next
                            LnArray.Add(line.ToString.Trim)

                            Dim FirstLoop As Boolean = True
                            For Each w In LnArray
                                If w.Trim <> "" Then
                                    If Not FirstLoop Then
                                        ReturnstrArray.Add("- " & w.Trim)
                                    Else
                                        ReturnstrArray.Add(w.Trim)
                                        FirstLoop = False
                                    End If
                                End If

                            Next
                        ElseIf ((LineBuilder.Length + wrd.Length) > WrapLength Or wrd.StartsWith(Environment.NewLine)) Then
                            ReturnstrArray.Add(LineBuilder.ToString)
                            LineBuilder.Remove(0, LineBuilder.Length)
                            wrd = wrd.Replace(Environment.NewLine, " ")
                            LineBuilder.Append(wrd.Trim)
                            LineBuilder.Append(" ")
                        Else

                            LineBuilder.Append(wrd.Trim)
                            LineBuilder.Append(" ")
                        End If
                    Next
                    If optEnableCNSpecificOptions Then
                        If LineBuilder.ToString.Contains("SWITCH TENDER") Then
                            LineBuilder.Replace("SWITCH TENDER", "EIC")
                        ElseIf LineBuilder.ToString.Contains("SWITCHTENDER") Then
                            LineBuilder.Replace("SWITCHTENDER", "EIC")
                        End If
                    End If
                    ReturnstrArray.Add(LineBuilder.ToString)
                    'ReturnstrArray.Add("")
                    LineBuilder = New StringBuilder(WrapLength)
                Next

            Else
                Dim words() As String = str.Split(New [Char]() {" "c, CChar(Environment.NewLine)})
                For Each wrd In words
                    If ((LineBuilder.Length + wrd.Length) > WrapLength Or wrd.StartsWith(Environment.NewLine)) Then
                        ReturnstrArray.Add(LineBuilder.ToString)
                        LineBuilder.Remove(0, LineBuilder.Length)
                        wrd = wrd.Replace(Environment.NewLine, " ")
                        LineBuilder.Append(wrd)
                        LineBuilder.Append(" ")
                    Else
                        LineBuilder.Append(wrd)
                        LineBuilder.Append(" ")
                    End If
                Next
                If optEnableCNSpecificOptions Then
                    If LineBuilder.ToString.Contains("SWITCH TENDER") Then
                        LineBuilder.Replace("SWITCH TENDER", "EIC")
                    ElseIf LineBuilder.ToString.Contains("SWITCHTENDER") Then
                        LineBuilder.Replace("SWITCHTENDER", "EIC")
                    End If
                End If
                ReturnstrArray.Add(LineBuilder.ToString)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally
            LineBuilder = New StringBuilder
        End Try
        Return ReturnstrArray
    End Function

    Public Function WrapTextIgnoreNewLine(ByVal str As String, ByVal WrapLength As Integer, Optional ByVal ShowLineBreaks As Boolean = True) As ArrayList

        Dim LineBuilder As New StringBuilder(WrapLength)
        Dim wrd As String = ""
        Dim ReturnstrArray As New ArrayList
        Try
            If ShowLineBreaks Then
                Dim Lines() As String = str.Split(CChar(Environment.NewLine))
                'Dim Lines() As String = str.Split(New [Char]() {Chr(10), CChar(Environment.NewLine)})
                Dim EachLine As String = ""
                For Each EachLine In Lines
                    wrd = ""
                    Dim Linewords() As String = EachLine.Split(" ")
                    For Each wrd In Linewords
                        If ((LineBuilder.Length + wrd.Length) > WrapLength Or wrd.StartsWith(Environment.NewLine)) Then
                            ReturnstrArray.Add(LineBuilder.ToString)
                            LineBuilder.Remove(0, LineBuilder.Length)
                            wrd = wrd.Replace(Environment.NewLine, " ")
                            LineBuilder.Append(wrd.Trim)
                            LineBuilder.Append(" ")
                        Else
                            LineBuilder.Append(wrd.Trim)
                            LineBuilder.Append(" ")
                        End If
                    Next

                    ReturnstrArray.Add(LineBuilder.ToString)
                    'ReturnstrArray.Add("")
                    LineBuilder = New StringBuilder(WrapLength)
                Next

            Else
                Dim words() As String = str.Split(New [Char]() {" "c, CChar(Environment.NewLine)})
                For Each wrd In words
                    If ((LineBuilder.Length + wrd.Length) > WrapLength Or wrd.StartsWith(Environment.NewLine)) Then
                        ReturnstrArray.Add(LineBuilder.ToString)
                        LineBuilder.Remove(0, LineBuilder.Length)
                        wrd = wrd.Replace(Environment.NewLine, " ")
                        LineBuilder.Append(wrd)
                        LineBuilder.Append(" ")
                    Else
                        LineBuilder.Append(wrd)
                        LineBuilder.Append(" ")
                    End If
                Next
                ReturnstrArray.Add(LineBuilder.ToString)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally
            LineBuilder = New StringBuilder
        End Try
        Return ReturnstrArray
    End Function

    Public Function GetDateFromUTC(ByVal DateSring As String) As Date
        Try
            Dim Y As String = Mid(DateSring, 1, 4)
            Dim M As String = Mid(DateSring, 5, 2)
            Dim D As String = Mid(DateSring, 7, 2)
            Dim H As String = Mid(DateSring, 9, 2)
            Dim n As String = Mid(DateSring, 11, 2)
            Dim s As String = Mid(DateSring, 13, 2)
            Dim theDate As Date = CDate(Y & "-" & M & "-" & D & " " & H & ":" & n & ":" & s)
            Return theDate

        Catch ex As Exception
            Return Now
        End Try
    End Function

    Public Function GetDataset(ByVal SQLQuery As String, ByVal ConnectionString As String) As DataSet
        Dim dsResult As New DataSet
        Dim dbconn As OleDbConnection = Nothing
        Dim oleAdp As OleDbDataAdapter = Nothing
        Try
            dbconn = New OleDb.OleDbConnection(ConnectionString)
            If dbconn.State = ConnectionState.Closed Then
                dbconn.Open()
            End If

            oleAdp = New OleDbDataAdapter(SQLQuery, dbconn)
            oleAdp.Fill(dsResult)

        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        Finally
            If dbconn.State <> ConnectionState.Closed Then
                dbconn.Close()
            End If
            dbconn = Nothing
            oleAdp = Nothing
        End Try

        Return dsResult
    End Function

    Public Sub ResetFlipFlopComponents(ByRef ComponentsColletion As ArrayList)
        Dim comp As Object = Nothing
        Dim CompColl As New ArrayList
        Dim FlipFlopFound As Boolean = False
        Dim OrgCompColl As New ArrayList
        Try
            For Each comp In ComponentsColletion
                OrgCompColl.Add(comp)
                Select Case comp.GetType.ToString
                    Case gTrackComponentName, gSwitchComponentName, gRRXINGComponentName
                        comp.FlipMe = False
                End Select

                CompColl.Add(comp)
            Next
            ComponentsColletion.Clear()
            comp = Nothing

            For Each comp In CompColl
                If FlipFlopFound Then
                    Select Case comp.GetType.ToString
                        Case gTrackComponentName, gSwitchComponentName, gRRXINGComponentName
                            comp.FlipMe = True
                    End Select
                End If
                If comp.GetType.ToString = gFlipFlopComponentName Then
                    FlipFlopFound = True
                End If
                ComponentsColletion.Add(comp)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
            ComponentsColletion.Clear()
            For Each comp In OrgCompColl
                ComponentsColletion.Add(comp)
            Next
        Finally
            CompColl = Nothing
            comp = Nothing
            OrgCompColl = Nothing
        End Try

    End Sub

    Public Sub GetErrorDetails(ByVal exp As Exception, Optional ByVal Comment As String = "", Optional ByVal strStackTrace As String = "")
        Dim Returnstring As String = ""
        Dim MethodName As String = ""
        Dim LineNumber As String = ""
        Dim Column As String = ""
        Dim FileName As String = ""
        Dim AppVersion As String = ""
        Try
            If Not String.IsNullOrEmpty(Application.ProductVersion) Then
                AppVersion = Application.ProductVersion
            End If
            Dim Trace As System.Diagnostics.StackTrace = New System.Diagnostics.StackTrace(exp, True)
            For i As Integer = 0 To Trace.FrameCount - 1
                If Trace.GetFrame(i).GetFileColumnNumber() <> 0 Then
                    MethodName = Trace.GetFrame(i).GetMethod().Name
                    LineNumber = Trace.GetFrame(i).GetFileLineNumber()
                    Column = Trace.GetFrame(i).GetFileColumnNumber()
                    FileName = Trace.GetFrame(i).GetFileName()
                    Exit For
                End If
            Next
            If LineNumber.Trim <> String.Empty Then
                Returnstring = "Version: " & AppVersion & vbTab & "FileName: " & FileName & vbTab & "MethodName: " & MethodName & vbTab & "LineNumber: " & LineNumber & vbCrLf
                Returnstring = Returnstring & exp.ToString
                If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
                If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
                SystemException(Returnstring)
            Else
                Returnstring = "Version: " & AppVersion & vbCrLf
                Returnstring = Returnstring & exp.ToString
                If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
                If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
                SystemException(Returnstring)
            End If
        Catch ex As Exception
            'SystemException(exp.ToString)
            Returnstring = exp.ToString
            If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
            If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
            SystemException(Returnstring)
            SystemException("GetErrorDetails Error:" & ex.ToString & vbCrLf & System.Environment.StackTrace.ToString)
        End Try

    End Sub

    Public Sub GetNetworkErrorDetails(ByVal exp As Exception, Optional ByVal Comment As String = "", Optional ByVal strStackTrace As String = "")
        Dim Returnstring As String = ""
        Dim MethodName As String = ""
        Dim LineNumber As String = ""
        Dim Column As String = ""
        Dim FileName As String = ""
        Dim AppVersion As String = ""
        Try
            If Not String.IsNullOrEmpty(Application.ProductVersion) Then
                AppVersion = Application.ProductVersion
            End If
            Dim Trace As System.Diagnostics.StackTrace = New System.Diagnostics.StackTrace(exp, True)
            For i As Integer = 0 To Trace.FrameCount - 1
                If Trace.GetFrame(i).GetFileColumnNumber() <> 0 Then
                    MethodName = Trace.GetFrame(i).GetMethod().Name
                    LineNumber = Trace.GetFrame(i).GetFileLineNumber()
                    Column = Trace.GetFrame(i).GetFileColumnNumber()
                    FileName = Trace.GetFrame(i).GetFileName()
                    Exit For
                End If
            Next
            If LineNumber.Trim <> String.Empty Then
                Returnstring = "Version: " & AppVersion & vbTab & "FileName: " & FileName & vbTab & "MethodName: " & MethodName & vbTab & "LineNumber: " & LineNumber & vbCrLf
                Returnstring = Returnstring & exp.ToString
                If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
                If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
                SystemNetworkException(Returnstring)
            Else
                Returnstring = "Version: " & AppVersion & vbCrLf
                Returnstring = Returnstring & exp.ToString
                If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
                If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
                SystemNetworkException(Returnstring)
            End If
        Catch ex As Exception
            'SystemException(exp.ToString)
            Returnstring = exp.ToString
            If Comment.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & Comment.Trim
            If gDeveloperSystem AndAlso strStackTrace.Trim <> String.Empty Then Returnstring = Returnstring & vbCrLf & "TRACE: " & strStackTrace.Trim
            SystemNetworkException(Returnstring)
            SystemNetworkException("GetErrorDetails Error:" & ex.ToString & vbCrLf & System.Environment.StackTrace.ToString)
        End Try

    End Sub

    Public Function CheckArrayContainsComp(ByVal ArrayToCheck As ArrayList, ByVal CompToCheck As Object) As Boolean
        Dim Comp As Object
        Try
            For Each Comp In ArrayToCheck
                If Comp.GUID = CompToCheck.GUID Then
                    Return True
                End If
            Next


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return False
    End Function

    Public Function GetResultDS(ByVal SQLQuery As String, ByVal UseStaticConnection As Boolean) As DataSet
        Dim ds As New DataSet
        Dim oleAdp As System.Data.OleDb.OleDbDataAdapter = Nothing
        Try
            Select Case UseStaticConnection
                Case True
                    Using DbConn As New OleDbConnection(gConnectionStringStatic)
                        DbConn.Open()
                        oleAdp = New System.Data.OleDb.OleDbDataAdapter(SQLQuery, DbConn)
                        oleAdp.Fill(ds)
                        If IsValidDataset(ds) Then
                            Return ds
                        End If
                        DbConn.Close()
                    End Using
                Case False
                    Using DbConn As New OleDbConnection(gConnectionStringDynamic)
                        DbConn.Open()
                        oleAdp = New System.Data.OleDb.OleDbDataAdapter(SQLQuery, DbConn)
                        oleAdp.Fill(ds)
                        If IsValidDataset(ds) Then
                            Return ds
                        End If
                        DbConn.Close()
                    End Using
            End Select

        Catch ex As Exception
            GetErrorDetails(ex, "SQL QUERY:: " & SQLQuery)
        Finally
            If oleAdp IsNot Nothing Then oleAdp.Dispose()
            oleAdp = Nothing
        End Try
        Return Nothing
    End Function

    Public Sub CheckForEICUsers()
        Dim DatabaseCommand As OleDb.OleDbCommand = Nothing
        Dim dr As OleDb.OleDbDataReader = Nothing
        Dim dbconn As OleDb.OleDbConnection = Nothing

        Try
            Dim EICQuery As String = "SELECT  PassWord, EICEnabled FROM " & gtblUsers & " WHERE"
            Dim UserEIC As String
            dbconn = New OleDb.OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringStatic)
            dbconn.Open()
            If dbconn.State <> ConnectionState.Open Then
                dbconn = Nothing
                Exit Sub
            End If
            Dim fmn As Foreman
            For Each fmn In TMDS.GlobalDeclarations.gMowForemen
                UserEIC = ""
                If optUseSystemUserCredentails Then
                    UserEIC = " FirstName = '" & fmn.FirstName & "' AND LastName = '" & fmn.LastName & "' AND PIN = '" & fmn.PIN & "' AND (UseType = 'EICUSER' OR (UseType='SYSTEMUSER' AND AllowRoleSmartMobileClient=1))"
                Else
                    UserEIC = " FirstName = '" & fmn.FirstName & "' AND LastName = '" & fmn.LastName & "' AND PIN = '" & fmn.PIN & "' AND (UseType = 'EICUSER' OR UseType='SYSTEMUSER')"
                End If

                UserEIC = EICQuery & UserEIC
                DatabaseCommand = New OleDbCommand(UserEIC, dbconn)
                dr = DatabaseCommand.ExecuteReader()
                While dr.Read()
                    If Not dr.IsDBNull(0) Then
                        fmn.Password = dr.GetString(0)
                    End If
                    If Not dr.IsDBNull(1) Then
                        fmn.EICEnabled = dr.GetBoolean(1)
                    End If

                    Exit While
                End While
                dr.Close()
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            dbconn.Close()
            dbconn = Nothing
            DatabaseCommand.Dispose()
            DatabaseCommand = Nothing
            dr = Nothing
        End Try
    End Sub

    Public Function GenerateTime(ByVal Subdvn As Subdivision) As Date
        Dim CurrentTimeZone As String = ""
        Dim OKDateTime As Date = Now

        Try
            If Subdvn Is Nothing Then
                If Format(OKDateTime, "HH:mm") = "00:00" Then OKDateTime = OKDateTime.AddMinutes(1)
                Return OKDateTime
            End If

            If Subdvn.TimeZone IsNot Nothing Then
                CurrentTimeZone = Subdvn.TimeZone
            Else
                CurrentTimeZone = "C"
            End If

            If Not optEnableTimeZoneSettings Then
                Select Case Mid(CurrentTimeZone, 1, 1)
                    Case "E"
                        OKDateTime = Now.AddHours(1 + gThisWorkstationTimeFactor)

                    Case "M"
                        OKDateTime = Now.AddHours(-1 + gThisWorkstationTimeFactor)

                    Case "P"
                        OKDateTime = Now.AddHours(-2 + gThisWorkstationTimeFactor)
                    Case "A"
                        OKDateTime = Now.AddHours(-3 + gThisWorkstationTimeFactor)

                    Case Else
                        OKDateTime = Now.AddHours(gThisWorkstationTimeFactor)
                End Select
            Else
                Dim TimeZnInfo As TimeZoneInfo
                Select Case Mid(CurrentTimeZone.ToUpper, 1, 1) 'Me.CurrentTimeZone
                    Case "E"
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(ESTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                    Case "M"
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(MSTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                    Case "P"
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(PSTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                    Case "S"
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(SaskatchewanSTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                    Case "A"
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(AlaskaSTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                    Case Else
                        TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(CSTimeZone)
                        OKDateTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                End Select
            End If
            If Format(OKDateTime, "HH:mm") = "00:00" Then OKDateTime = OKDateTime.AddMinutes(1)
            Return OKDateTime

        Catch ex As Exception
            GetErrorDetails(ex, "", ex.StackTrace.ToString)
            Return OKDateTime
        End Try
        'Return Nothing
    End Function

    Public Function GetAdjustedDateTimeByTimeZone(ByVal Subdvn As Subdivision) As Date
        Dim CurrentTimeZone As String = ""
        Dim CurrentTime As Date = Now
        Try
            If Subdvn Is Nothing Then
                Return CurrentTime
            End If

            CurrentTimeZone = Subdvn.TimeZone
            Dim TimeZnInfo As TimeZoneInfo
            Select Case Mid(CurrentTimeZone.ToUpper, 1, 1) 'Me.CurrentTimeZone
                Case "E"
                    TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(ESTimeZone)
                    CurrentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                Case "M"
                    TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(MSTimeZone)
                    CurrentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                Case "P"
                    TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(PSTimeZone)
                    CurrentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                Case "S"
                    TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(SaskatchewanSTimeZone)
                    CurrentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
                Case Else
                    TimeZnInfo = TimeZoneInfo.FindSystemTimeZoneById(CSTimeZone)
                    CurrentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZnInfo)
            End Select
            Return CurrentTime

        Catch ex As Exception
            GetErrorDetails(ex)
            Return CurrentTime
        End Try
        'Return Nothing
    End Function

    Public Function FormatMilepostForBulletinKCS(ByVal MilePost As String, ByVal MilePostSuffix As String, ByVal IsFlag As Boolean, Optional ByVal ShowHundredthDecimal As Boolean = False) As String
        Dim MP As Double = 0.0
        Try
            If IsNumeric(MilePost) Then
                Double.TryParse(MilePost, MP)
                If MP > 0 Then
                    'Return MilePostSuffix & " " & FormatMilePostToTenths(MilePost)
                    If optAllowOnlyTenthsinBulletin AndAlso Not ShowHundredthDecimal Then
                        Return MilePostSuffix & "" & Format(CDec(MilePost), "0.0")
                    Else
                        Return MilePostSuffix & "" & Format(CDec(MilePost), "0.00")
                    End If
                    Return MilePostSuffix & "" & (MilePost)
                Else
                    If IsFlag Then
                        Return String.Empty
                    Else
                        Return MilePostSuffix & "" & "0.0"
                    End If
                    '
                End If
            Else
                'Return MilePostSuffix & " " & FormatMilePostToTenths(MilePost)
                Return MilePostSuffix & " " & (MilePost)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return MilePostSuffix & " " & FormatMilePostToTenths(MilePost)
    End Function

    Private Function FormatMilePostToTenths(ByVal sMilePost As String) As String
        Dim DecimalPosition As Integer = InStr(sMilePost, ".")
        If DecimalPosition > 0 Then
            sMilePost = Mid(sMilePost, 1, DecimalPosition + 1)
        End If
        Return sMilePost
    End Function

    Public Function FindRemoteTrack(ByVal trackToSearch As Track, ByVal RemoteTerritory As Integer, ByRef RemoteLinkDirection As String, ByRef TrackHasRemoteLink As Track) As Track
        Dim RemoteTrack As Track = Nothing
        'Dim LeftRemoteTracks As New ArrayList
        'Dim RightRemoteTracks As New ArrayList
        Dim NextComp As Object = Nothing
        Dim SearchDirection As String = "L"
        Dim lastComp As Object = Nothing
        Try
            'Do LeftSearch
            If trackToSearch.LeftLink > 0 Then
                NextComp = FindCompFromGUID(trackToSearch.LeftLink)
                If NextComp IsNot Nothing Then
                    If NextComp.TerritoryID = RemoteTerritory Then
                        TrackHasRemoteLink = trackToSearch
                        RemoteTrack = NextComp
                        RemoteLinkDirection = "LEFT"
                        NextComp = Nothing
                    End If
                End If
            Else
                If trackToSearch.LeftAltLink > 0 Then
                    NextComp = FindCompFromGUID(trackToSearch.LeftAltLink)
                    If NextComp IsNot Nothing Then
                        If NextComp.TerritoryID <> RemoteTerritory Then
                            NextComp = Nothing
                        Else
                            If trackToSearch.LeftAltLink > 0 Then
                                NextComp = FindCompFromGUID(trackToSearch.LeftAltLink)
                                If NextComp IsNot Nothing Then
                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                        NextComp = Nothing
                                    Else
                                        TrackHasRemoteLink = trackToSearch
                                        RemoteTrack = NextComp
                                        RemoteLinkDirection = "LEFT"
                                    End If
                                End If
                            ElseIf trackToSearch.RightAltLink > 0 Then
                                NextComp = FindCompFromGUID(trackToSearch.RightAltLink)
                                If NextComp IsNot Nothing Then
                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                        NextComp = Nothing
                                    Else
                                        TrackHasRemoteLink = trackToSearch
                                        RemoteTrack = NextComp
                                        RemoteLinkDirection = "RIGHT"
                                    End If
                                End If
                            End If
                            NextComp = Nothing
                        End If

                    End If
                End If
            End If

            If NextComp IsNot Nothing Then
                For I As Integer = 0 To TrackCollection.Count - 1
                    If NextComp IsNot Nothing Then
                        Select Case NextComp.GetType.ToString
                            Case gTrackComponentName
                                Select Case SearchDirection
                                    Case "L"
                                        If NextComp.LeftLink > 0 Then
                                            lastComp = NextComp
                                            NextComp = FindCompFromGUID(NextComp.LeftLink)
                                            If NextComp.TerritoryID = RemoteTerritory Then
                                                TrackHasRemoteLink = lastComp
                                                RemoteTrack = NextComp
                                                RemoteLinkDirection = "RIGHT"
                                                NextComp = Nothing
                                            End If
                                        Else
                                            If NextComp.LeftAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                If NextComp IsNot Nothing Then
                                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                                        NextComp = Nothing
                                                        Exit For
                                                    Else
                                                        TrackHasRemoteLink = lastComp
                                                        RemoteTrack = NextComp
                                                        RemoteLinkDirection = "LEFT"
                                                        Exit For
                                                    End If
                                                End If
                                            ElseIf NextComp.RightAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                If NextComp IsNot Nothing Then
                                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                                        NextComp = Nothing
                                                        Exit For
                                                    Else
                                                        TrackHasRemoteLink = lastComp
                                                        RemoteTrack = NextComp
                                                        RemoteLinkDirection = "RIGHT"
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Case "R"
                                        If NextComp.RightLink > 0 Then
                                            lastComp = NextComp
                                            NextComp = FindCompFromGUID(NextComp.RightLink)
                                            If NextComp.TerritoryID = RemoteTerritory Then
                                                TrackHasRemoteLink = lastComp
                                                RemoteTrack = NextComp
                                                RemoteLinkDirection = "RIGHT"
                                                NextComp = Nothing
                                            End If
                                        Else
                                            If NextComp.LeftAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                If NextComp IsNot Nothing Then
                                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                                        NextComp = Nothing
                                                        Exit For
                                                    Else
                                                        TrackHasRemoteLink = lastComp
                                                        RemoteTrack = NextComp
                                                        RemoteLinkDirection = "LEFT"
                                                        Exit For
                                                    End If
                                                End If
                                            ElseIf NextComp.RightAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                If NextComp IsNot Nothing Then
                                                    If NextComp.TerritoryID <> RemoteTerritory Then
                                                        NextComp = Nothing
                                                        Exit For
                                                    Else
                                                        TrackHasRemoteLink = lastComp
                                                        RemoteTrack = NextComp
                                                        RemoteLinkDirection = "RIGHT"
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        End If
                                End Select



                            Case gSwitchComponentName
                                Dim sw As Switch = NextComp
                                Select Case SearchDirection
                                    Case "L"
                                        If sw.ControlledReverse Then
                                            If sw.RightAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                            ElseIf sw.LeftAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                            End If
                                        Else
                                            NextComp = FindCompFromGUID(sw.LeftLink)
                                        End If
                                    Case "R"
                                        If sw.ControlledReverse Then
                                            If sw.RightAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                            ElseIf sw.LeftAltLink > 0 Then
                                                NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                            End If
                                        Else
                                            NextComp = FindCompFromGUID(sw.RightLink)
                                        End If
                                End Select

                            Case gFlipFlopComponentName
                                If SearchDirection = "L" Then NextComp = FindCompFromGUID(NextComp.LeftLink) Else NextComp = FindCompFromGUID(NextComp.RightLink)
                                SearchDirection = "R"

                            Case Else
                                If SearchDirection = "L" Then NextComp = FindCompFromGUID(NextComp.LeftLink) Else NextComp = FindCompFromGUID(NextComp.RightLink)
                        End Select
                    End If
                Next
            End If



            'Do RightSearch
            lastComp = Nothing
            NextComp = Nothing
            If RemoteTrack Is Nothing Then
                SearchDirection = "R"
                NextComp = Nothing
                If trackToSearch.RightLink > 0 Then
                    NextComp = FindCompFromGUID(trackToSearch.RightLink)
                    If NextComp IsNot Nothing Then
                        If NextComp.TerritoryID = RemoteTerritory Then
                            TrackHasRemoteLink = trackToSearch
                            RemoteTrack = NextComp
                            RemoteLinkDirection = "RIGHT"
                            NextComp = Nothing
                        End If
                    End If
                Else
                    If trackToSearch.LeftAltLink > 0 Then
                        NextComp = FindCompFromGUID(trackToSearch.LeftAltLink)
                        If NextComp IsNot Nothing Then
                            If NextComp.TerritoryID <> RemoteTerritory Then
                                NextComp = Nothing
                            Else
                                TrackHasRemoteLink = trackToSearch
                                RemoteTrack = NextComp
                                RemoteLinkDirection = "LEFT"
                            End If
                        End If
                    ElseIf trackToSearch.RightAltLink > 0 Then
                        NextComp = FindCompFromGUID(trackToSearch.RightAltLink)
                        If NextComp IsNot Nothing Then
                            If NextComp.TerritoryID <> RemoteTerritory Then
                                NextComp = Nothing
                            Else
                                TrackHasRemoteLink = trackToSearch
                                RemoteTrack = NextComp
                                RemoteLinkDirection = "RIGHT"
                            End If
                        End If
                    End If
                    NextComp = Nothing
                End If


                If NextComp IsNot Nothing Then
                    For I As Integer = 0 To TrackCollection.Count - 1
                        If NextComp IsNot Nothing Then
                            Select Case NextComp.GetType.ToString
                                Case gTrackComponentName
                                    Select Case SearchDirection
                                        Case "L"
                                            If NextComp.LeftLink > 0 Then
                                                lastComp = NextComp
                                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                                                If NextComp.TerritoryID = RemoteTerritory Then
                                                    TrackHasRemoteLink = lastComp
                                                    RemoteTrack = NextComp
                                                    RemoteLinkDirection = "RIGHT"
                                                    NextComp = Nothing
                                                End If
                                            Else
                                                If NextComp.LeftAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                    If NextComp IsNot Nothing Then
                                                        If NextComp.TerritoryID <> RemoteTerritory Then
                                                            NextComp = Nothing
                                                            Exit For
                                                        Else
                                                            TrackHasRemoteLink = lastComp
                                                            RemoteTrack = NextComp
                                                            RemoteLinkDirection = "LEFT"
                                                            Exit For
                                                        End If
                                                    End If
                                                ElseIf NextComp.RightAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                    If NextComp IsNot Nothing Then
                                                        If NextComp.TerritoryID <> RemoteTerritory Then
                                                            NextComp = Nothing
                                                            Exit For
                                                        Else
                                                            TrackHasRemoteLink = lastComp
                                                            RemoteTrack = NextComp
                                                            RemoteLinkDirection = "RIGHT"
                                                            Exit For
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Case "R"
                                            If NextComp.RightLink > 0 Then
                                                lastComp = NextComp
                                                NextComp = FindCompFromGUID(NextComp.RightLink)
                                                If NextComp.TerritoryID = RemoteTerritory Then
                                                    TrackHasRemoteLink = lastComp
                                                    RemoteTrack = NextComp
                                                    RemoteLinkDirection = "RIGHT"
                                                    NextComp = Nothing
                                                End If
                                            Else
                                                If NextComp.LeftAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                    If NextComp IsNot Nothing Then
                                                        If NextComp.TerritoryID <> RemoteTerritory Then
                                                            NextComp = Nothing
                                                            Exit For
                                                        Else
                                                            TrackHasRemoteLink = lastComp
                                                            RemoteTrack = NextComp
                                                            RemoteLinkDirection = "LEFT"
                                                            Exit For
                                                        End If
                                                    End If
                                                ElseIf NextComp.RightAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                    If NextComp IsNot Nothing Then
                                                        If NextComp.TerritoryID <> RemoteTerritory Then
                                                            NextComp = Nothing
                                                            Exit For
                                                        Else
                                                            TrackHasRemoteLink = lastComp
                                                            RemoteTrack = NextComp
                                                            RemoteLinkDirection = "RIGHT"
                                                            Exit For
                                                        End If
                                                    End If
                                                End If
                                            End If
                                    End Select



                                Case gSwitchComponentName
                                    Dim sw As Switch = NextComp
                                    Select Case SearchDirection
                                        Case "L"
                                            If sw.ControlledReverse Then
                                                If sw.RightAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                ElseIf sw.LeftAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(sw.LeftLink)
                                            End If
                                        Case "R"
                                            If sw.ControlledReverse Then
                                                If sw.RightAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.RightAltLink)
                                                ElseIf sw.LeftAltLink > 0 Then
                                                    NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(sw.RightLink)
                                            End If
                                    End Select

                                Case gFlipFlopComponentName
                                    If SearchDirection = "L" Then NextComp = FindCompFromGUID(NextComp.LeftLink) Else NextComp = FindCompFromGUID(NextComp.RightLink)
                                    SearchDirection = "R"

                                Case Else
                                    If SearchDirection = "L" Then NextComp = FindCompFromGUID(NextComp.LeftLink) Else NextComp = FindCompFromGUID(NextComp.RightLink)
                            End Select
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            NextComp = Nothing
            lastComp = Nothing
        End Try
        Return RemoteTrack
    End Function

    Friend Function ReturnReportTime(CompCollection As ArrayList, CompToCheck As Object) As Date
        Dim ReportTime As Date = Now
        Dim Comp As Object = Nothing
        Dim CurrentTimeZone As String = ""
        Try
            'If CompCollection Is Nothing AndAlso CompToCheck Is Nothing Then Return ReportTime
            'If CompCollection IsNot Nothing AndAlso CompCollection.Count = 0 AndAlso CompToCheck Is Nothing Then Return ReportTime
            If CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                For Each Comp In CompCollection
                    If Comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Comp.TimeZone <> "" Then
                            CurrentTimeZone = Mid(Comp.TimeZone, 1, 1)
                            CompToCheck = Comp
                            Exit For
                        End If
                    End If
                Next
            ElseIf CompToCheck IsNot Nothing AndAlso CompToCheck.TimeZone IsNot Nothing Then
                If CompToCheck.TimeZone <> "" Then
                    CurrentTimeZone = Mid(CompToCheck.TimeZone, 1, 1)
                    'CompToCheck = Comp
                End If
            End If

            If String.IsNullOrEmpty(CurrentTimeZone) Then Return ReportTime
            If CurrentTimeZone.Trim = "" Then Return ReportTime

            Select Case CurrentTimeZone
                Case "E"
                    ReportTime = Now.AddHours(1 + gThisWorkstationTimeFactor)
                    'PTCBOSReportTime = Format(Now.AddHours(1 + gThisWorkstationTimeFactor).ToUniversalTime, "yyyyMMddHHmmss")
                Case "M"
                    ReportTime = Now.AddHours(-1 + gThisWorkstationTimeFactor)
                    'PTCBOSReportTime = Format(Now.AddHours(-1 + gThisWorkstationTimeFactor).ToUniversalTime, "yyyyMMddHHmmss")
                Case "P"
                    ReportTime = Now.AddHours(-2 + gThisWorkstationTimeFactor)
                Case "A"
                    ReportTime = Now.AddHours(-3 + gThisWorkstationTimeFactor)
                    'PTCBOSReportTime = Format(Now.AddHours(-2 + gThisWorkstationTimeFactor).ToUniversalTime, "yyyyMMddHHmmss")
                Case Else
                    ReportTime = Now.AddHours(gThisWorkstationTimeFactor)
                    'PTCBOSReportTime = Format(Now.AddHours(gThisWorkstationTimeFactor).ToUniversalTime, "yyyyMMddHHmmss")
            End Select

            If optEnableTimeZoneSettings AndAlso CompToCheck IsNot Nothing AndAlso CompToCheck.mySubdivision IsNot Nothing Then
                Dim NewReportTime As Date = GenerateTime(CompToCheck.mySubdivision)
                ReportTime = NewReportTime ' Format(NewReportTime, "HH:mm")
            End If





        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

        Return ReportTime
    End Function

#Region " Memory Release "

    Public Sub SetWorkingSet(ByVal lnMaxSize As Integer, ByVal lnMinSize As Integer)

        Try
            Dim loProcess As System.Diagnostics.Process = System.Diagnostics.Process.GetCurrentProcess()
            loProcess.MaxWorkingSet = lnMaxSize
            loProcess.MinWorkingSet = lnMinSize
        Catch ex As Exception
        End Try

    End Sub

#End Region

    Public Sub CheckGlobalDynamicConnection()
        Try
            If TMDS.GlobalDeclarations.gDatabaseConnectionDynamic Is Nothing Then
                TMDS.GlobalDeclarations.gDatabaseConnectionDynamic = New OleDbConnection(gConnectionStringDynamic)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub CheckGlobalStaticConnection()
        Try
            If TMDS.GlobalDeclarations.gDatabaseConnectionStatic Is Nothing Then
                TMDS.GlobalDeclarations.gDatabaseConnectionStatic = New OleDbConnection(gConnectionStringStatic)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub CopyArrayCollection(ByRef ArrayCopyTo As ArrayList, ByVal ArrayCopyFrom As ArrayList)
        Dim obj As Object = Nothing
        Try
            obj = Nothing
            ArrayCopyTo = New ArrayList
            If ArrayCopyFrom IsNot Nothing AndAlso ArrayCopyFrom.Count > 0 Then
                For Each obj In ArrayCopyFrom
                    If Not obj Is Nothing Then
                        ArrayCopyTo.Add(obj)
                    End If
                Next
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub RemoveCPPSS(ByVal Auth As TMDS.TrackAuthority.OtherSpecialAuthorityType)
        Dim comp As Object = Nothing
        Dim sig As Signal = Nothing
        Try
            For Each comp In Auth.ComponentsInSelection
                comp.OtherSplAuthorities.remove(Auth)
            Next

            For Each comp In Auth.ComponentsInSelection
                If comp.GetType.ToString = gSignalComponentName Then
                    sig = comp

                    If comp.DeviceBlocks IsNot Nothing AndAlso comp.DeviceBlocks.Count > 0 Then
                    Else
                        comp.DeviceBlocked = False
                        comp.BlockAbsolute = False
                    End If
                    comp.InvalidateSignal()
                    comp.updateStatus()
                    sig.CurrentDrawingForm.Invalidate()
                Else
                    comp.DeviceBlocked = False
                    comp.BlockAbsolute = False
                End If
                comp.SplAuthorityBlock = False
                If optShwoPSSEMTAuthorityNoDeviceBlkDirection Then comp.TrafficDirection = ""
                comp.updateStatus()

                ' gMoveNoteSelectedNote.CurrentDisplayForm.Invalidate()
            Next
            ResetCompToCodeBrown(Auth.ComponentsInSelection)
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            comp = Nothing
            sig = Nothing
        End Try
    End Sub

    Public Sub SetlvItemBackColor(ByRef lv As ListViewItem, ByVal colorName As Color)
        Try
            Dim i As Integer
            For i = 0 To lv.SubItems.Count - 1
                lv.SubItems(i).BackColor = colorName
            Next
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub SetlvSubItemForeColor(ByRef lv As ListViewItem, ByVal SubItemindex As Integer, ByVal colorName As Color)
        Try
            lv.UseItemStyleForSubItems = False
            Dim i As Integer
            For i = 0 To lv.SubItems.Count - 1
                If i = SubItemindex Then
                    lv.SubItems(i).ForeColor = colorName
                End If
            Next
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

#Region " DATABASE COMMANDS "

    Public Function ExecuteQueryCollection(ByVal SQLQueryCollection As ArrayList, ByVal pConnectionString As String) As Boolean

        Dim Cmd As OleDbCommand = Nothing
        Dim DBTransaction As OleDbTransaction = Nothing
        Dim SaveTime As String = Now.ToString
        Dim s As String = ""
        Try

            If Not SQLQueryCollection Is Nothing AndAlso SQLQueryCollection.Count = 0 Then
                Return False
            End If
            Using DbConn As New OleDbConnection(pConnectionString)
                DbConn.Open()
                DBTransaction = DbConn.BeginTransaction()
                Try
                    For Each s In SQLQueryCollection
                        If gDebug Then SystemEvent("SQL QUERY: " & s)
                        Cmd = New OleDbCommand(s, DbConn, DBTransaction)
                        Cmd.ExecuteNonQuery()
                    Next
                    DBTransaction.Commit()
                Catch ex As Exception
                    If Not DBTransaction Is Nothing Then DBTransaction.Rollback()
                    GetErrorDetails(ex, "SQL Query Detail:: " & s)
                    Return False
                End Try
                DbConn.Close()
            End Using

            Return True

        Catch ex As Exception
            GetErrorDetails(ex, "SQL Query Detail:: " & s)
            Return False
        Finally
            If Cmd IsNot Nothing Then Cmd.Dispose()
            Cmd = Nothing
            If DBTransaction IsNot Nothing Then DBTransaction.Dispose()
            DBTransaction = Nothing
        End Try

    End Function

    Public Function ExecuteQuery(ByVal SQLQuery As String, ByVal pConnectionString As String) As Boolean

        Dim Cmd As OleDbCommand = Nothing
        Dim DBTransaction As OleDbTransaction = Nothing
        Dim SaveTime As String = Now.ToString
        Dim s As String = ""
        Try

            If SQLQuery IsNot Nothing AndAlso Not String.IsNullOrEmpty(SQLQuery) AndAlso SQLQuery.Trim <> "" Then
            Else
                Return False
            End If
            Using DbConn As New OleDbConnection(pConnectionString)
                DbConn.Open()
                DBTransaction = DbConn.BeginTransaction()
                Try
                    If gDebug Then SystemEvent("SQL QUERY: " & SQLQuery)
                    Cmd = New OleDbCommand(SQLQuery, DbConn, DBTransaction)
                    Cmd.ExecuteNonQuery()

                    DBTransaction.Commit()
                Catch ex As Exception
                    If Not DBTransaction Is Nothing Then DBTransaction.Rollback()
                    GetErrorDetails(ex, "SQL Query Detail:: " & SQLQuery)
                    Return False
                End Try
                DbConn.Close()
            End Using
            Return True
        Catch ex As Exception
            GetErrorDetails(ex, "SQL Query Detail:: " & SQLQuery)
            If DBTransaction IsNot Nothing Then DBTransaction.Rollback()
            Return False
        Finally
            If Cmd IsNot Nothing Then Cmd.Dispose()
            Cmd = Nothing
            DBTransaction.Dispose()
            DBTransaction = Nothing
        End Try

    End Function

#End Region

    Public Function CheckAlphaNumericEngineFormatting(ByVal pEngineId As String) As Boolean
        Try
            If pEngineId <> String.Empty Then
                pEngineId = pEngineId.Replace(" ", "")
                Dim c As String
                Dim NumericFound As Boolean = False
                Dim AlphaFirstFound As Boolean = False
                Dim AlphaPart As String = String.Empty
                Dim NumericPart As String = String.Empty
                Dim i As Integer = 0

                For Each c In pEngineId
                    i = i + 1
                    If i = 1 Then
                        If Not IsAlpha(c) Then
                            Return False
                        Else
                            AlphaFirstFound = True
                        End If
                    End If

                    If IsNumeric(c) Then
                        NumericFound = True
                        NumericPart = NumericPart & c
                    Else
                        AlphaPart = AlphaPart & c
                    End If
                Next
                If AlphaFirstFound And NumericFound Then
                    Return True
                Else
                    Return False
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return False
    End Function

    Public Function GetStandardIdentifyEngineFormatting(ByVal pEngineId As String) As String
        Try
            Dim NumericAllowed As Integer
            Select Case optRailRoadName
                Case "CP"
                    NumericAllowed = 5
                Case Else
                    NumericAllowed = 4
            End Select

            If optAllowIdentifyEngineNumericLengthToSix Then
                NumericAllowed = 6
            End If
            Dim c As String
            Dim NumericFound As Boolean
            Dim UnitID As String
            Dim AlphaPart As String = String.Empty
            Dim NumericPart As String = String.Empty
            pEngineId = pEngineId.Replace(" ", "")
            For Each c In pEngineId
                If IsNumeric(c) Then
                    NumericFound = True
                    If NumericPart.Length < NumericAllowed Then
                        NumericPart = NumericPart & c
                    End If
                Else
                    If Not NumericFound Then
                        If AlphaPart.Length < 4 Then
                            AlphaPart = AlphaPart & c
                        End If
                    End If
                End If
            Next

            Dim SpaceNeeded As Integer = 10 - (AlphaPart.Length + NumericPart.Length)

            UnitID = AlphaPart.ToUpper & Space(SpaceNeeded) & NumericPart

            pEngineId = Trim(UnitID.ToUpper)

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return pEngineId
    End Function

    Public Function GetTrainWithLeadLocomotive(ByVal LeadEngine As String, Optional ByRef SubDivisionUID As String = "", Optional ByVal TrainSymbol As String = "", Optional ByRef TrainIsNotActive As Boolean = False) As String
        Dim DataFetch As String = String.Empty
        Dim dr As OleDbDataReader
        Dim DatabaseCommand As OleDbCommand
        Dim dbconn As OleDbConnection
        Dim ReturnData As String = String.Empty
        Try
            DataFetch = "SELECT Symbol,EngineID,Subdivision,TrainIsNotActive FROM " & gtblTrainsActive & " WHERE EngineID = '" & LeadEngine & "' OR EngineIDp= '" & LeadEngine & "'"
            If TrainSymbol.Trim <> "" Then
                DataFetch = "SELECT Symbol,EngineID,Subdivision,TrainIsNotActive  FROM " & gtblTrainsActive & " WHERE (EngineID = '" & LeadEngine & "' OR EngineIDp= '" & LeadEngine & "') AND Symbol <>'" & TrainSymbol & "'"
            End If

            dbconn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringDynamic)
            DatabaseCommand = New OleDbCommand(DataFetch, dbconn)
            dbconn.Open()
            dr = DatabaseCommand.ExecuteReader()
            While dr.Read()
                ReturnData = dr.GetString(0)
                SubDivisionUID = dr.GetString(2)
                If Not dr.IsDBNull(3) Then
                    'Dim IsNotActive As Boolean
                    'IsNotActive = dr.GetBoolean(3)
                    'If IsNotActive Then
                    '    TrainIsNotActive = True
                    'End If
                    TrainIsNotActive = dr.GetBoolean(3)
                    If Not TrainIsNotActive Then Return ReturnData
                End If

            End While
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return ReturnData
    End Function

    Public Function IsAlpha(ByVal Str As String) As Boolean
        Try
            Dim AlphaString As String = "ABCDEFGHIJKLMNOPQNRSTUVWXYZ"
            Dim ch As String
            For Each ch In AlphaString
                If Str.ToUpper = ch Or Str = " " Then
                    Return True
                End If
            Next
            Return False
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try
    End Function

    Public Function CheckIfAuthExistsForTrain(ByVal trnsymbol As String) As Boolean
        Dim Auth As Object = Nothing
        Dim authcount As Integer = 0
        Dim ExstAuth As New ArrayList()
        Try
            For Each Auth In OtherTrackAuthorities
                If Not Auth Is Nothing Then
                    Select Case Auth.GetType.ToString
                        Case gSFAComponentName
                            If Auth.TrainSymbol = trnsymbol.Trim Then
                                ExstAuth.Add(Auth)
                            End If
                        Case gTTComponentName
                            If Auth.TrainSymbol = trnsymbol.Trim Then
                                ExstAuth.Add(Auth)
                            End If
                    End Select
                End If
            Next
            Auth = Nothing
            For Each Auth In TMDS.GlobalDeclarations.gOtherSpecialAuthorityCollection
                If Not Auth Is Nothing AndAlso Auth.GetType.ToString = gOtherSplAuthorityName Then
                    If Auth.TrainSymbol = trnsymbol.Trim Then
                        ExstAuth.Add(Auth)
                    End If
                End If
            Next

            If ExstAuth.Count > 0 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
            Return True
        End Try
    End Function

    Public Function CheckIfThisTrainHasAuthorityOnThisTrack(ByVal TrackToCheck As Track, ByVal TrainToCheck As TrainID, Optional ByVal UseEngineID As Boolean = True) As Boolean
        Dim TrackType As String = ""
        Dim Auth As Object = Nothing
        Try
            If TrackToCheck.TrackAuthorities IsNot Nothing AndAlso TrackToCheck.TrackAuthorities.Count > 0 Then
            Else
                Return False
            End If
            If TrackToCheck.AuthorityType.Contains("TWC") Then
                TrackType = "TWC"
            Else
                TrackType = "CTC"
            End If
            For Each Auth In TrackToCheck.TrackAuthorities
                Select Case TrackType
                    Case "TWC"
                        If Auth.GetType.ToString = gSFAComponentName Then
                            If UseEngineID AndAlso Auth.IssuedToTrain Then
                                If Auth.IssueTo.Contains(TrainToCheck.IdentifingUnit) Then Return True
                            ElseIf Not UseEngineID AndAlso Auth.IssuedToTrain Then
                                If Auth.TrainSymbol = TrainToCheck.Name Then Return True
                            End If
                        End If
                    Case "CTC"
                        If Auth.GetType.ToString = gTTComponentName Then
                            If UseEngineID AndAlso Auth.IssuedToTrain Then
                                If Auth.IssueTo.Contains(TrainToCheck.IdentifingUnit) Then Return True
                            ElseIf Not UseEngineID AndAlso Auth.IssuedToTrain Then
                                If Auth.TrainSymbol = TrainToCheck.Name Then Return True
                            End If
                        End If
                End Select
            Next

            Return False
        Catch ex As Exception

        End Try
    End Function

    ''' <summary>
    ''' Show Custom Message Box
    ''' </summary>
    ''' <param name="message">message to show</param>
    ''' <param name="formPoint">form point from object</param>
    Public Sub ShowCustomMessageBox(message As String, formPoint As Point)
        Try
            ShowCustomMessageBox(New ArrayList From {message}, My.Resources.GenericSystemMessage, False, formPoint)
        Catch ex As Exception
            SystemException(ex.Message)
        End Try
    End Sub


    ''' <summary>
    ''' Displays a confirmation dialog to allow or deny a territory override for a different user or workstation.
    ''' </summary>
    ''' <param name="ownerForm">Parent form used as the dialog owner and for screen positioning.</param>
    ''' <param name="workStationName">
    ''' Workstation name currently associated with the territory; used to build the prompt message when provided.
    ''' </param>
    ''' <param name="userLoged">
    ''' User name currently associated with the territory; used when <paramref name="workStationName"/> is empty.
    ''' </param>
    ''' <param name="optLoginUserMessage1">Message prefix used when a workstation name is supplied.</param>
    ''' <param name="optLoginUserMessage2">Message suffix appended to the constructed prompt.</param>
    ''' <param name="optLoginUserMessage3">Message prefix used when a logged-in user name is supplied.</param>
    ''' <param name="optLoginUserMessage4">Dialog caption displayed in the message box.</param>
    ''' <returns>
    ''' <see cref="Boolean"/>: <see langword="True"/> indicates the override was approved or suppressed by simulation mode;
    ''' <see langword="False"/> indicates the override was declined by the user.
    ''' </returns>
    ''' <exception cref="Exception">
    ''' Handled internally via <see cref="SystemException(String)"/>; not rethrown.
    ''' </exception>
    Public Function ShowMessageOverrideUserTerritory(ownerForm As Form,
                                                     workStationName As String,
                                                     userLoged As String,
                                                     optLoginUserMessage1 As String,
                                                     optLoginUserMessage2 As String,
                                                     optLoginUserMessage3 As String,
                                                     optLoginUserMessage4 As String) As Boolean
        Try
            If gSimulationMode Then Return True
            Using frmMessageBox As New frmMessageBox()
                Dim messageList As New ArrayList()
                frmMessageBox.Owner = ownerForm
                If Not String.IsNullOrWhiteSpace(workStationName) Then
                    messageList.Add($"{optLoginUserMessage1} {workStationName _
                                    } {optLoginUserMessage2}")
                ElseIf Not String.IsNullOrWhiteSpace(userLoged) Then
                    messageList.Add($"{optLoginUserMessage3} {userLoged _
                                        } {optLoginUserMessage2}")
                End If
                If messageList.Count > 0 Then
                    frmMessageBox.SetMessage(messageList,
                         optLoginUserMessage4,
                         True,
                         New Point(ownerForm.Location.X, ownerForm.Location.Y))
                    frmMessageBox.ShowDialog()
                    If frmMessageBox.FormFeedBack = 6 Then
                        If Not ShowAdminPassword(ownerForm,
                                             Nothing,
                                             True) Then
                            Return False
                        End If
                    Else
                        Return False
                    End If
                End If
            End Using
            Return True
        Catch ex As Exception
            SystemException(ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Show Custom Message Box
    ''' </summary>
    ''' <param name="messages">list of messages to show</param>
    ''' <param name="header">text to show in header</param>
    ''' <param name="yesNoButton">yes or no buttons</param>
    ''' <param name="formPoint">form point from object</param>
    ''' <param name="ignoreRed">ignore Red</param>
    ''' <param name="showTopMost">show Top Most</param>
    ''' <param name="myParent">my Parent</param>
    ''' <param name="focusButton">focus Button</param>
    ''' <param name="focusButtonName">focus Button Name</param>
    ''' <param name="buttonNames">button Names</param>
    ''' <param name="centerOnParentForm">center On Parent Form</param>
    ''' <returns>Return user feedback</returns>
    Public Function ShowCustomMessageBox(messages As ArrayList,
                                         header As String,
                                         yesNoButton As Boolean,
                                         Optional formPoint As Object = Nothing,
                                         Optional ignoreRed As Boolean = False,
                                         Optional showTopMost As Boolean = True,
                                         Optional myParent As Form = Nothing,
                                         Optional focusButton As Boolean = False,
                                         Optional focusButtonName As String = "",
                                         Optional buttonNames As String = Nothing,
                                         Optional centerOnParentForm As Boolean = False) As Integer
        Try
            If messages Is Nothing Then Return -1
            Dim launchPoint As Point
            If formPoint Is Nothing Then
                launchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + 20,
                                        TMDS.SystemForms.MySystemForms.frmMainMDI.Top + 50)
            ElseIf formPoint.GetType.ToString <> "System.Drawing.Point" Then
                launchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + 20,
                                        TMDS.SystemForms.MySystemForms.frmMainMDI.Top + 50)
            Else
                launchPoint = formPoint
            End If
            messages.Add(String.Empty)
            Dim disableFocusOnButtons As Boolean = True
            If focusButton Then disableFocusOnButtons = False
            Using frmMessageBox As New frmMessageBox
                With frmMessageBox
                    If myParent IsNot Nothing Then
                        .Owner = myParent
                    Else
                        .Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                    End If
                    If centerOnParentForm Then
                        .StartPosition = FormStartPosition.CenterParent
                    End If
                    If showTopMost Then
                        .TopMost = True
                        .BringToFront()
                    End If
                    .SetMessage(messages, header, yesNoButton, launchPoint, buttonNames, False, False, Nothing,
                                Nothing, False, Nothing, ignoreRed, False, False, False, False, False, False,
                                False, disableFocusOnButtons, focusButtonName)
                    .ShowDialog()
                    .TopMost = False
                End With
                Return frmMessageBox.FormFeedBack
            End Using
        Catch ex As Exception
            SystemException(ex.Message)
        End Try
    End Function

    Public Function ShowAdminPassword(ByVal myParent As Form, Optional ByVal Lpoint As Object = Nothing, Optional ByVal ShowTopMost As Boolean = True, Optional UsedFor As String = "", Optional TRK As Track = Nothing) As Boolean
        'Dim frmAdminLogin As frmAdminLogin = Nothing
        Try
            Dim LaunchPoint As Point
            If Lpoint Is Nothing Then
                LaunchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + 20, TMDS.SystemForms.MySystemForms.frmMainMDI.Top + 50)
            ElseIf Lpoint.GetType.ToString <> "System.Drawing.Point" Then
                LaunchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + 20, TMDS.SystemForms.MySystemForms.frmMainMDI.Top + 50)
            Else
                LaunchPoint = Lpoint
            End If


            Using frmAdmin As New frmAdminLogin
                With frmAdmin
                    .UsedFor = UsedFor
                    .SetUpProperties(TRK)
                    If myParent IsNot Nothing Then
                        .Owner = myParent
                    End If
                    .Location = LaunchPoint
                    .TopMost = True
                    .ShowDialog()
                    If Not .PassWordValid Then
                        Return False
                    End If
                End With
            End Using


            'frmAdminLogin = New frmAdminLogin()
            'frmAdminLogin.SetUpProperties(Nothing)
            'If Not IsNothing(myParent) Then
            '    frmAdminLogin.Owner = myParent
            'End If
            'If ShowTopMost Then
            '    frmAdminLogin.TopMost = True
            '    frmAdminLogin.BringToFront()
            'End If
            'frmAdminLogin.Location = LaunchPoint
            'frmAdminLogin.ShowDialog()
            'If Not frmAdminLogin.PassWordValid Then
            '    Return False
            'End If

            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        Finally
            'frmAdminLogin.Close()
            'If Not frmAdminLogin.IsDisposed Then frmAdminLogin.Dispose()
            'frmMessageBox = Nothing
        End Try
    End Function

    Public Sub PerformSRandFleetCancelOnBlocking(ByVal ComponentsCollection As ArrayList, Optional ByVal CancelStoredRouteOnSignalStopRequested As Boolean = False, Optional ByVal FromAuthority As Boolean = False)
        If optDisableExistingFleetSignalOnAbsoluteBlocking Or optDisableExistingStoredRouteOnAbsoluteBlocking Or optCancelStoredRouteWhenSignalStopRequested Then
            Dim comp As Object = Nothing
            Dim Comp2 As Object = Nothing
            Dim Collcomp As Object = Nothing
            Dim CPList As New ArrayList
            Dim CP As ControlPoint = Nothing
            Dim LeftAdjacentCP As ControlPoint = Nothing
            Dim RightAdjacentCP As ControlPoint = Nothing
            Dim LeftObject As Object = Nothing
            Dim RightObject As Object = Nothing
            Dim sr As StoredRoute
            Dim RearrangedCollection As New ArrayList
            Dim SignalCollection As New ArrayList
            If ComponentsCollection Is Nothing Then Exit Sub
            If ComponentsCollection.Count = 0 Then Exit Sub
            Dim FirstTrack As Object = Nothing
            Dim LastTrack As Object = Nothing
            Dim TrackToVerify As Track = Nothing
            Dim AdjacentSignal As Signal = Nothing
            Dim MyTrafficDirection As String = ""
            Dim SignalToCheck As Signal = Nothing
            Try
                For Each comp In ComponentsCollection
                    Select Case comp.GetType.ToString
                        Case gTrackComponentName, gSignalComponentName, gSwitchComponentName
                            If comp.myControlPoint IsNot Nothing Then
                                If Not CPList.Contains(comp.myControlPoint) Then CPList.Add(comp.myControlPoint)
                            End If
                    End Select
                    SystemEvent(comp.GetType.ToString & " " & comp.GUID & "  " & comp.myControlPoint.Name)
                Next
                If optEnableCPSpecificOptions AndAlso FromAuthority Then
                    comp = Nothing
                    For Each comp In ComponentsCollection
                        Select Case comp.GetType.ToString
                            Case gTrackComponentName
                                If comp.TrafficDirection <> "" Then
                                    If comp.SignalStatOnMe = Track.SignalStatus.SignalCleared Then
                                        'SignalCollection = FindAllClearedToMySelection(comp, ComponentsCollection)
                                        'Exit for
                                        AdjacentSignal = TMDS.SystemUtilities.FetchData.FindMyControllingSignal(comp, comp.TrafficDirection)
                                        If Not SignalCollection.Contains(AdjacentSignal) Then SignalCollection.Add(AdjacentSignal)
                                    End If
                                End If
                        End Select
                    Next
                    AdjacentSignal = Nothing
                    For Each AdjacentSignal In SignalCollection
                        If Not CPList.Contains(AdjacentSignal.myControlPoint) Then
                            CPList.Add(AdjacentSignal.myControlPoint)
                        End If
                    Next

                    For Each AdjacentSignal In SignalCollection
                        If Not CPList.Contains(AdjacentSignal.myControlPoint) Then
                            For i As Integer = 1 To 10
                                For Each comp In AdjacentSignal.SignalRouteCollection
                                    If comp.GetType.ToString = gTrackComponentName Then
                                        If Not ComponentsCollection.Contains(AdjacentSignal) Then
                                            ComponentsCollection.Add(comp)
                                            CPList.Add(AdjacentSignal.myControlPoint)
                                            Exit For
                                        End If

                                    End If
                                Next
                            Next
                        End If
                    Next
                ElseIf optEnableCPSpecificOptions And Not FromAuthority Then
                    RightObject = Nothing
                    LeftObject = Nothing
                    comp = Nothing
                    For Each comp In ComponentsCollection
                        Select Case comp.GetType.ToString
                            Case gTrackComponentName
                                If comp.TrafficDirection <> "" Then
                                    If comp.SignalStatOnMe = Track.SignalStatus.SignalCleared Then
                                        LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ComponentsCollection)
                                        If LastTrack.GetType.ToString = gTrackComponentName Then
                                            Select Case LastTrack.TrafficDirection.ToString.Trim.ToUpper
                                                Case "RIGHT"
                                                    If LastTrack.RightLink > 0 Then
                                                        RightObject = FindCompFromGUID(LastTrack.RightLink)
                                                        If Not RightObject Is Nothing Then
                                                            If Not CPList.Contains(RightObject.myControlPoint) Then CPList.Add(RightObject.myControlPoint)
                                                        End If
                                                    End If
                                                Case "LEFT"
                                                    If LastTrack.LeftLink > 0 Then
                                                        RightObject = FindCompFromGUID(LastTrack.LeftLink)
                                                        If Not LeftObject Is Nothing Then
                                                            If Not CPList.Contains(LeftObject.myControlPoint) Then CPList.Add(LeftObject.myControlPoint)
                                                        End If
                                                    End If
                                            End Select
                                        End If
                                    End If
                                End If
                        End Select
                    Next

                End If
                RightObject = Nothing
                LeftObject = Nothing
                comp = Nothing
                If ComponentsCollection.Count = 1 Then
                    comp = Nothing
                    comp = FindCompFromGUID(ComponentsCollection(0).GUID)
                    If Not comp Is Nothing Then
                        If comp.LeftLink > 0 Then LeftObject = FindCompFromGUID(comp.LeftLink)
                        If comp.RightLink > 0 Then RightObject = FindCompFromGUID(comp.RightLink)
                        If LeftObject IsNot Nothing Then If LeftObject.myControlPoint IsNot Nothing Then If Not CPList.Contains(LeftObject.myControlPoint) Then CPList.Add(LeftObject.myControlPoint)
                        If RightObject IsNot Nothing Then If RightObject.myControlPoint IsNot Nothing Then If Not CPList.Contains(RightObject.myControlPoint) Then CPList.Add(RightObject.myControlPoint)
                    End If
                Else
                    comp = Nothing
                    comp = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ComponentsCollection)
                    If comp.LeftLink > 0 Then LeftObject = FindCompFromGUID(comp.RightLink)
                    comp = Nothing
                    comp = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ComponentsCollection)
                    If comp.LeftLink > 0 Then RightObject = FindCompFromGUID(comp.LeftLink)
                    If LeftObject IsNot Nothing Then If LeftObject.myControlPoint IsNot Nothing Then If Not CPList.Contains(LeftObject.myControlPoint) Then CPList.Add(LeftObject.myControlPoint)
                    If RightObject IsNot Nothing Then If RightObject.myControlPoint IsNot Nothing Then If Not CPList.Contains(RightObject.myControlPoint) Then CPList.Add(RightObject.myControlPoint)
                End If




                If CPList IsNot Nothing AndAlso CPList.Count > 0 Then
                    For Each CP In CPList
                        comp = Nothing
                        If CancelStoredRouteOnSignalStopRequested Then
                            comp = Nothing
                            Dim IsokToStopFleeting As Boolean = False
                            sr = Nothing
                            If CP.StoredRoutes IsNot Nothing AndAlso CP.StoredRoutes.Count > 0 Then
                                For Each sr In CP.StoredRoutes
                                    For Each comp In sr.ComponentSelection
                                        If ComponentsCollection.Contains(comp) Then
                                            IsokToStopFleeting = True
                                            Exit For
                                        End If
                                        If IsokToStopFleeting Then Exit For
                                    Next
                                Next
                                If IsokToStopFleeting Then CP.CancelStoredRoutes()
                            End If
                        ElseIf optDisableExistingStoredRouteOnAbsoluteBlocking Then
                            If optEnableCPSpecificOptions Then
                                CP.CancelStoredRoutes(ComponentsCollection)
                            Else
                                CP.CancelStoredRoutes()
                            End If
                        End If
                        If optDisableExistingFleetSignalOnAbsoluteBlocking And Not CancelStoredRouteOnSignalStopRequested Then
                            For Each comp In CP.AssignedComponenets
                                If comp.GetType.ToString = gSignalComponentName Then
                                    If comp.FleetEnabled Then
                                        If optEnableCPSpecificOptions Then
                                            For Each Collcomp In ComponentsCollection
                                                If comp.SignalRouteCollection.Contains(Collcomp) Then
                                                    comp.DoSetSignalFleeting(False, True)
                                                    Exit For
                                                End If
                                                If comp.SignalRouteCollection IsNot Nothing AndAlso comp.SignalRouteCollection.Count = 0 AndAlso FromAuthority Then
                                                    Comp2 = Nothing
                                                    Dim FltDisabled As Boolean = False
                                                    For Each Comp2 In ComponentsCollection
                                                        If Comp2.GetType.ToString = gTrackComponentName Then
                                                            If Comp2.TrafficDirection <> "" Then
                                                                SignalToCheck = TMDS.SystemUtilities.FetchData.FindMyControllingSignal(Comp2, Comp2.TrafficDirection)
                                                                If SignalToCheck IsNot Nothing AndAlso SignalToCheck.FleetEnabled AndAlso SignalToCheck.GUID = comp.guid Then
                                                                    comp.DoSetSignalFleeting(False, True)
                                                                    FltDisabled = True
                                                                    Exit For
                                                                End If
                                                            End If
                                                        End If
                                                    Next
                                                    If FltDisabled Then Exit For
                                                End If
                                            Next
                                        Else
                                            comp.DoSetSignalFleeting(False, True)
                                        End If
                                    End If

                                End If
                            Next
                        End If
                    Next
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                comp = Nothing
                CPList = Nothing
                CP = Nothing
                LeftObject = Nothing
                RightObject = Nothing

                Comp2 = Nothing
                Collcomp = Nothing
                LeftAdjacentCP = Nothing
                RightAdjacentCP = Nothing
                sr = Nothing
                RearrangedCollection = Nothing

                SignalCollection = Nothing
                FirstTrack = Nothing
                LastTrack = Nothing
                TrackToVerify = Nothing
                AdjacentSignal = Nothing
                SignalToCheck = Nothing

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Checks if the Selected Entry track is valid to start SFA issuance process
    ''' </summary>
    ''' <param name="EntryTrack"></param>
    ''' <param name="LastSFA"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsEntryTrackValidForTWCIssue(ByVal EntryTrack As Track, ByVal LastSFA As TMDS.TrackAuthority.SFAuthority) As Boolean
        Dim comp As Object = Nothing
        Dim sw As Switch = Nothing
        Dim LastTrack As Track = Nothing
        Dim EntryTrackFirstEntryPoint As Track = Nothing
        Dim LastSFAExitPoint As Track = Nothing
        Dim EntryTrackSwitch As Switch = Nothing
        Dim AltTrack As Track = Nothing
        Dim D As String = ""
        Dim AuthorityDirection As String = ""
        Dim EntryObject As Object = Nothing
        Try
            If LastSFA Is Nothing Then Return True
            AuthorityDirection = LastSFA.Direction
            If AuthorityDirection.Trim = "" Then Return False
            LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(LastSFA.Components)
            If LastTrack Is Nothing Then Return True
            If EntryTrack Is Nothing Then Return False

            If LastSFA.Components.Contains(EntryTrack) Then Return True

            If LastTrack.RightLink = 0 Or LastTrack.LeftLink = 0 Then Return True

            If EntryTrack.OsComponentCollection IsNot Nothing AndAlso EntryTrack.OsComponentCollection.Count > 0 Then
                comp = Nothing
                For Each comp In EntryTrack.OsComponentCollection
                    If comp.GetType.ToString = gSwitchComponentName Then
                        EntryTrackSwitch = comp
                        Exit For
                    End If
                Next
                AltTrack = Nothing
                If EntryTrackSwitch.ControlledReverse Then
                    If EntryTrackSwitch.RightAltLink > 0 Then
                        AltTrack = FindCompFromGUID(EntryTrackSwitch.RightAltLink)
                        D = "R"
                    ElseIf EntryTrackSwitch.LeftAltLink > 0 Then
                        AltTrack = FindCompFromGUID(EntryTrackSwitch.LeftAltLink)
                        D = "L"
                    End If
                    If Not AltTrack Is Nothing Then
                        For i As Integer = 0 To EntryTrack.OsComponentCollection.Count
                            Select Case D
                                Case "R"
                                    comp = FindCompFromGUID(AltTrack.RightLink)
                                    If Not comp Is Nothing Then
                                        If EntryTrackSwitch.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        Else
                                            EntryTrackFirstEntryPoint = comp
                                            Exit For
                                        End If
                                    End If
                                Case "L"
                                    comp = FindCompFromGUID(AltTrack.LeftLink)
                                    If Not comp Is Nothing Then
                                        If EntryTrackSwitch.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        Else
                                            EntryTrackFirstEntryPoint = comp
                                            Exit For
                                        End If
                                    End If
                            End Select
                        Next
                    End If
                Else
                    Select Case AuthorityDirection
                        Case "RIGHT"
                            comp = Nothing
                            AltTrack = Nothing
                            comp = FindCompFromGUID(EntryTrackSwitch.LeftLink)
                            If Not comp Is Nothing Then
                                For i As Integer = 0 To EntryTrack.OsComponentCollection.Count
                                    If Not comp Is Nothing Then
                                        If EntryTrackSwitch.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        Else
                                            EntryTrackFirstEntryPoint = comp
                                            Exit For
                                        End If
                                    End If
                                    comp = FindCompFromGUID(AltTrack.LeftLink)
                                Next
                            End If
                        Case "LEFT"
                            comp = Nothing
                            AltTrack = Nothing
                            comp = FindCompFromGUID(EntryTrackSwitch.RightLink)
                            If Not comp Is Nothing Then
                                For i As Integer = 0 To EntryTrack.OsComponentCollection.Count
                                    If Not comp Is Nothing Then
                                        If EntryTrackSwitch.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        Else
                                            EntryTrackFirstEntryPoint = comp
                                            Exit For
                                        End If
                                    End If
                                    comp = FindCompFromGUID(AltTrack.RightLink)
                                Next
                            End If
                    End Select
                End If
            Else
                ' EntryTrackFirstEntryPoint = EntryTrack
                Select Case AuthorityDirection
                    Case "RIGHT"
                        EntryObject = FindCompFromGUID(EntryTrack.LeftLink)
                        For i As Integer = 0 To 5
                            If EntryObject.GetType.ToString = gTrackComponentName Then
                                EntryTrackFirstEntryPoint = EntryObject
                                Exit For
                            Else
                                EntryObject = FindCompFromGUID(EntryObject.LeftLink)
                            End If
                        Next
                        ' EntryTrackFirstEntryPoint = FindCompFromGUID(EntryTrack.LeftLink)
                    Case Else
                        EntryObject = FindCompFromGUID(EntryTrack.RightLink)
                        For i As Integer = 0 To 5
                            If EntryObject.GetType.ToString = gTrackComponentName Then
                                EntryTrackFirstEntryPoint = EntryObject
                                Exit For
                            Else
                                EntryObject = FindCompFromGUID(EntryObject.RightLink)
                            End If
                        Next
                        ' EntryTrackFirstEntryPoint = FindCompFromGUID(EntryTrack.RightLink)

                End Select
            End If

            If LastSFA.Components.Contains(EntryTrackFirstEntryPoint) Then Return True
            Return False
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            D = ""
            sw = Nothing
            comp = Nothing
            AltTrack = Nothing
            EntryTrackFirstEntryPoint = Nothing
            LastSFAExitPoint = Nothing
            LastTrack = Nothing
            EntryTrackSwitch = Nothing
            EntryObject = Nothing
        End Try

        Return False
    End Function

    Public Function ValidateDate(ByVal [Date] As String) As String
        Try
            If CDate([Date]) > "1/2/2000" Then
                Return [Date]
            Else
                Return String.Empty
            End If
        Catch ex As Exception
            Return String.Empty
        End Try


    End Function

    Public Function IsValidDate(ByVal DateToCheck As String) As Boolean
        Dim DtToCheck As Date
        Try
            If Date.TryParse(DateToCheck, DtToCheck) Then
                If DtToCheck.Year > 1998 Then
                    Return True

                Else
                    If DtToCheck.Minute > 0 AndAlso DtToCheck.Hour > 0 Then
                        Return True
                    Else
                        Return False
                    End If
                End If
            End If
        Catch ex As Exception
            Return False
        End Try


    End Function

    Public Sub DoLocalPrint(ByVal FileName As String, Optional ByVal showPDFDocument As Boolean = False)
        Dim PrinterName As String = ""
        Dim Timeout As Integer = 3
        Try
            'If PrinterName.Trim.Length = 0 Then
            PrinterName = (New System.Drawing.Printing.PrinterSettings).PrinterName
            'End If

            Dim Proc As New System.Diagnostics.Process

            Proc.EnableRaisingEvents = True
            Proc.StartInfo.FileName = FileName
            Proc.StartInfo.Arguments = Chr(34) + PrinterName + Chr(34)
            Proc.StartInfo.Verb = "PrintTo"
            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized
            Proc.StartInfo.CreateNoWindow = True
            Proc.Start()

            Do While Timeout > 0 AndAlso Not Proc.HasExited
                System.Threading.Thread.Sleep(1000)
                Timeout -= 1
            Loop

            If Not Proc.HasExited Then
                SystemEvent("Killing PDF process FileName: " & FileName)
                Proc.Kill()
            End If
            SystemEvent("Closing PDF process FileName: " & FileName)
            Proc.Close()
            If showPDFDocument Then ViewPDFDocument(FileName)
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

    End Sub

    Friend Sub ViewPDFDocument(ByVal FileName As String)
        Try
            If System.IO.File.Exists(FileName) Then
                Dim startInfo As New ProcessStartInfo(FileName)
                startInfo.UseShellExecute = True
                Process.Start(startInfo)
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub KillPDFProcesses()
        Try
            Dim p As Process
            Dim proc As New Process()
            Dim CurrentProcess As Process = Process.GetCurrentProcess
            Dim ProcList As New ArrayList()

            For Each p In Process.GetProcesses
                If Mid(p.ProcessName, 1, 4) = "Acro" Or p.ProcessName = "AcroRD32" Or p.ProcessName = "Acrobat" Then
                    p.Kill()
                End If
            Next
            proc.Dispose()
            proc = Nothing
        Catch
        End Try

    End Sub

    Public Sub PlaySoundFile(ByVal SoundPath As String)
        Try
            If SoundPath.Trim = "" Then Exit Sub
            'Dim PlayAudio As New DoPlayAudio(AddressOf Sound.PlayWaveFile)
            'PlayAudio.BeginInvoke(SoundPath, Nothing, Nothing)
            Sound.PlayWaveFile(SoundPath)
        Catch ex As Exception
            Beep()
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Function LimitNumberOfDecimals(ByVal tb As TextBox, ByVal NumberOfDecimalValues As Integer) As Boolean
        Try
            If tb.Text.Trim <> "0.0" AndAlso tb.Text.IndexOf(".") >= 0 AndAlso Len(Mid$(tb.Text, InStr(tb.Text, ".") + 1)) >= NumberOfDecimalValues Then
                Return True
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return False
    End Function

    Public Function ConvertPoundToTons(ByVal WeightInPounds As Double, Optional ByVal LongTon As Boolean = True) As Double
        Dim WeightInTonns As Double = 0
        Try
            Select Case LongTon
                Case True
                    WeightInTonns = WeightInPounds / 2240
                Case Else
                    WeightInTonns = WeightInPounds / 2000
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return WeightInTonns
    End Function

    Friend Function ConvertMilesToKilometers(ByVal miles As Double) As Double
        Return miles * 1.609344
    End Function

    Friend Function ConvertKilometersToMiles(ByVal kilometers As Double) As Double
        Return kilometers * 0.621371192
    End Function

    Friend Function ConvertMetersToKilometers(ByVal Meters As Double) As Double
        Return Meters * 0.001
    End Function

    Friend Function GetTrackList(ByVal BeginMilePost As String, ByVal BeginMilePostSuffix As String, ByVal EndMilePost As String, ByVal EndMilePostSuffix As String, Optional ByVal TrackName As String = "ALL") As ArrayList
        Dim TrackList As New ArrayList
        Dim NewTrackList As New ArrayList
        Dim MilePostTravelDirection As String = ""
        Dim trk As Track = Nothing ' Object
        Dim IsBullFFSortedInLocation As Boolean = False

        Dim TrackNameList As New ArrayList
        Try
            Dim MileBegin As String = BeginMilePost
            Dim MileEnd As String = EndMilePost

            Dim MileHigh As Decimal
            Dim MileLow As Decimal
            Dim CuspMileLow As Decimal = MileLow + 0.00001
            If CDec(MileBegin) > CDec(MileEnd) Then
                MileHigh = CDec(MileBegin)
                MileLow = CDec(MileEnd)
            Else
                MileLow = CDec(MileBegin)
                MileHigh = CDec(MileEnd)
            End If

            Dim AllTracks As Boolean
            If InStr(TrackName.ToUpper, "ALL") <> 0 Then
                AllTracks = True
            End If


            TrackList = TMDS.SystemUtilities.FetchData.FindAllTracksFromMilePost(MileLow, MileHigh, "", "", "", "")

            NewTrackList = New ArrayList
            If MileLow <> MileHigh Then
                If TrackList IsNot Nothing AndAlso TrackList.Count > 0 Then
                    trk = TrackList(0)
                    If trk IsNot Nothing Then
                        If trk.LeftLimitMilePost > trk.RightLimitMilePost Then
                            MilePostTravelDirection = "LEFT"
                        Else
                            MilePostTravelDirection = "RIGHT"
                        End If
                    End If
                End If
                If MilePostTravelDirection.Trim <> "" Then
                    NewTrackList = GetFilteredTracks(TrackList, MileLow, MileHigh, MilePostTravelDirection)
                    If NewTrackList IsNot Nothing AndAlso NewTrackList.Count > 0 Then
                        CopyArrayCollection(TrackList, NewTrackList)
                    End If
                End If
            End If

            Return TrackList
        Catch ex As Exception
            GetErrorDetails(ex)
            Return TrackList
        Finally
            trk = Nothing
            NewTrackList = Nothing
        End Try
    End Function

    <System.Runtime.CompilerServices.Extension()> Public Sub Invoke(ByVal control As Control, ByVal action As Action)
        If control.InvokeRequired Then
            'control.Invoke(New MethodInvoker(action), Nothing)
            control.Invoke(DirectCast(action, [Delegate]))
        Else
            action.Invoke()
        End If
    End Sub

    Private Function ValidateTrainActiveStatus(ByVal CurrentTrain As TrainID) As Boolean
        Dim LongMessage As New ArrayList
        'Dim LaunchPoint As New Point(Me.Location.X + 100, Me.Location.Y + 300)
        Dim Feedback As Integer = 0
        Dim trn As TrainID = Nothing
        Dim ExistingTrain As TrainID = Nothing
        Try
            For Each trn In TrainCollection
                If trn.IdentifingUnit.Trim = CurrentTrain.IdentifingUnit.Trim AndAlso trn.Name.ToUpper.Trim <> CurrentTrain.Name.ToUpper.Trim Then
                    ExistingTrain = trn
                    Exit For
                End If
            Next
            If ExistingTrain Is Nothing Then Return False
            If CheckIfAuthExistsForTrain(ExistingTrain.Name) Then Return True







        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            LongMessage = Nothing
            'LaunchPoint = Nothing
        End Try
    End Function

    Public Function CheckForDuplicateEngineID(ByVal CurrentTrain As TrainID, ByRef MenuText As String) As Boolean
        Dim LongMessage As New ArrayList
        'Dim LaunchPoint As New Point(Me.Location.X + 100, Me.Location.Y + 300)
        Dim Feedback As Integer = 0
        Dim trn As TrainID = Nothing
        Dim ExistingTrain As TrainID = Nothing
        Try
            MenuText = ActivateTrainMenuText & Space(1) & CurrentTrain.IdentifingUnit
            For Each trn In TrainCollection
                If trn.IdentifingUnit.Trim = CurrentTrain.IdentifingUnit.Trim AndAlso trn.Name.ToUpper.Trim <> CurrentTrain.Name.ToUpper.Trim Then
                    ExistingTrain = trn
                    Exit For
                End If
            Next
            If ExistingTrain Is Nothing Then Return False
            If Not CheckIfAuthExistsForTrain(ExistingTrain.Name) AndAlso Not CheckIfAuthExistsForTrain(CurrentTrain.Name) Then
                If ExistingTrain.TrainInActive AndAlso Not CurrentTrain.TrainInActive Then
                    MenuText = DeActivateTrainMenuText & Space(1) & CurrentTrain.IdentifingUnit
                    Return True
                ElseIf Not ExistingTrain.TrainInActive AndAlso CurrentTrain.TrainInActive Then
                    MenuText = ActivateTrainMenuText & Space(1) & CurrentTrain.IdentifingUnit
                    Return True
                ElseIf ExistingTrain.TrainInActive AndAlso CurrentTrain.TrainInActive Then
                    MenuText = ActivateTrainMenuText & Space(1) & CurrentTrain.IdentifingUnit
                    Return True
                End If
            End If


        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            LongMessage = Nothing
            'LaunchPoint = Nothing
        End Try

    End Function

    Public Function GetDuplicateTrain(ByVal CurrentTrain As TrainID) As ArrayList
        Dim trn As TrainID = Nothing
        Dim ExistingTrainCollection As New ArrayList
        Try
            For Each trn In TrainCollection
                If trn.IdentifingUnit.Trim = CurrentTrain.IdentifingUnit.Trim AndAlso trn.Name.ToUpper.Trim <> CurrentTrain.Name.ToUpper.Trim Then
                    If Not ExistingTrainCollection.Contains(trn) Then ExistingTrainCollection.Add(trn)
                End If
            Next
            If ExistingTrainCollection IsNot Nothing AndAlso ExistingTrainCollection.Count > 0 Then Return ExistingTrainCollection
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Sub ActivateDeActivateEngine(ByVal CurrentTrain As TrainID, ByRef RedrawTrain As Boolean, ByVal ForcedDeActivate As Boolean)

        Try
            If ForcedDeActivate Then
                CurrentTrain.TrainInActive = True
                CurrentTrain.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A15(TRUE)")
                RedrawTrain = True
            ElseIf Not CheckForDuplicateEngineID(CurrentTrain, String.Empty) Then
                Dim ExistingTrnCollection As New ArrayList
                ExistingTrnCollection = GetDuplicateTrain(CurrentTrain)
                Dim trnother As TrainID = Nothing
                Dim trnotherHasAuth As TrainID = Nothing
                If ExistingTrnCollection IsNot Nothing Then
                    RedrawTrain = True
                    For Each trnother In ExistingTrnCollection
                        If CheckIfAuthExistsForTrain(trnother.Name) Then
                            trnotherHasAuth = trnother
                            Exit For
                        End If
                    Next
                    If trnotherHasAuth Is Nothing Then
                        For Each trnother In ExistingTrnCollection
                            trnother.TrainInActive = True
                            trnother.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A16(TRUE)")
                            trnother.myTrack.InvalidateTrainID()
                            trnother.myTrack.InitTrainDisplay()
                            trnother.myTrack.InvalidateTrainIDMessageDisplay()
                        Next
                        CurrentTrain.TrainInActive = False
                        CurrentTrain.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A17(FALSE)")
                    Else
                        For Each trnother In ExistingTrnCollection
                            If trnother IsNot trnotherHasAuth Then
                                trnother.TrainInActive = True
                                trnother.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A18(TRUE)")
                                trnother.myTrack.InvalidateTrainID()
                                trnother.myTrack.InitTrainDisplay()
                                trnother.myTrack.InvalidateTrainIDMessageDisplay()
                            End If
                        Next
                        CurrentTrain.TrainInActive = True
                        CurrentTrain.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A19(TRUE)")
                        trnotherHasAuth.TrainInActive = False
                        trnotherHasAuth.UpdateTrainActiveStatusToDB("CHANGE ACTIVE STATUS-ActivateDeActivateEngine-modfunc TrainID-A20(FALSE)")
                        trnotherHasAuth.myTrack.InvalidateTrainID()
                        trnotherHasAuth.myTrack.InitTrainDisplay()
                        trnotherHasAuth.myTrack.InvalidateTrainIDMessageDisplay()
                    End If
                End If
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Friend Function GetValidCrewName(ByVal CrewName As String) As String

        Try
            Select Case CrewName.ToUpper.Trim
                Case "NONE", "---", "", "----"
                    Return String.Empty
                Case Else
                    Return CrewName.ToUpper.Trim
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

    ''' <summary>
    ''' 
    ''' 
    ''' </summary>
    ''' <param name="SQLQuery"></param>
    ''' <param name="ConnStr"></param>
    ''' <param name="CallingMethod"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function CheckIfRecordExistInDB(SQLQuery As String, ConnStr As String, CallingMethod As String) As Boolean
        Dim dbComm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Try
            SystemEvent("CheckIfRecordExistInDB CALLED BY: " & CallingMethod)
            Using DbConn As New OleDbConnection(ConnStr)
                Try
                    DbConn.Open()
                    dbComm = New OleDbCommand(SQLQuery, DbConn)
                    dr = dbComm.ExecuteReader
                    If dr.HasRows Then Return True

                Catch ex As Exception
                    GetErrorDetails(ex, "SQL QUERY::" & SQLQuery)
                Finally
                    DbConn.Close()
                End Try
            End Using
        Catch ex As Exception
            GetErrorDetails(ex, "SQL QUERY::" & SQLQuery)
        Finally
            If dbComm IsNot Nothing Then dbComm.Dispose()
            dbComm = Nothing
            If dr IsNot Nothing Then dr = Nothing
        End Try
    End Function


    Friend Sub ReprintTransferReport()
        Dim dbAdapter As OleDbDataAdapter = Nothing
        Try
            Dim dsPDF As New DataSet
            Dim SQLstr As String = "Select top 1 * from tblArchiveDocuments where EventType='TRANSFER-REPORT' AND WkstnUID ='" & gWorkStationName & "' order by EventTimeStamp desc"
            Using dbConn As New OleDb.OleDbConnection(gConnectionStringDynamic)
                dbAdapter = New OleDbDataAdapter(SQLstr, dbConn)
                dbAdapter.Fill(dsPDF)
                Dim bFileData() As Byte = dsPDF.Tables(0).Rows(0)("binaryImage")
                Dim fn As String = dsPDF.Tables(0).Rows(0)("DocumentName")
                Dim FileSize As String = dsPDF.Tables(0).Rows(0)("binaryImageSize")
                Dim FolderPath As String = Application.StartupPath & "\REPORTS\"
                File.WriteAllBytes(FolderPath & fn, bFileData)
                If System.IO.File.Exists(FolderPath & fn) Then
                    Dim startInfo As New ProcessStartInfo(FolderPath & fn)
                    startInfo.UseShellExecute = True
                    Process.Start(startInfo)
                End If
                dbAdapter.Dispose()
                dbConn.Close()
            End Using
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Friend Sub LoadTrainCarSecurementForm(myDisplayForm As Windows.Forms.Form, myTrain As TrainID)
        Try
            If TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced IsNot Nothing Then
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            End If
            Using frmTRE As New frmTroubleReportEnhanced
                With frmTRE
                    .Owner = myDisplayForm
                    .thisTrain = myTrain
                    .CreateReport(Nothing, "TCS")
                    .ShowDialog()
                End With
            End Using
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Friend Sub LoadTrainIncidentReportForm(myDisplayForm As Windows.Forms.Form, myTrain As TrainID, ReportType As String)
        Try
            If TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced IsNot Nothing Then
                TMDS.SystemForms.MySystemForms.frmTroubleReportEnhanced = Nothing
            End If
            Using frmTRE As New frmTroubleReportEnhanced
                With frmTRE
                    .Owner = myDisplayForm
                    .thisTrain = myTrain
                    .CreateReport(Nothing, ReportType)
                    .ShowDialog()
                End With
            End Using
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Function RearrangeCompCollection_New(ByVal CollectionToBeReaaranged As ArrayList, ByVal IsPSSType As Boolean, SearchDiretion As String, Optional SortAscending As Boolean = False, Optional IgnoreNonTraverableTracks As Boolean = False) As ArrayList
        Dim LeftTrack As Track = Nothing
        Dim RightTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Dim TempCompCollection As New ArrayList
        Try
            Dim tempArray As New Collection
            For Each Comp In CollectionToBeReaaranged
                Dim compSort As New clsCompSort
                Select Case Comp.GetType.ToString
                    Case gTrackComponentName
                        'Dim trk As Track
                        If Comp.NonTransverable AndAlso IgnoreNonTraverableTracks Then
                        Else
                            With compSort
                                .myUID = Comp.GUID
                                Select Case SearchDiretion.Trim.ToUpper
                                    Case "LEFT"
                                        .MilePost = Comp.LeftLimitMilePost
                                    Case Else
                                        .MilePost = Comp.RightLimitMilePost
                                End Select

                                .myType = gTrackComponentName
                                tempArray.Add(compSort)
                            End With
                        End If

                    Case gSwitchComponentName
                        With compSort
                            .myUID = Comp.GUID
                            .MilePost = Comp.MilePost
                            .myType = gSwitchComponentName
                            tempArray.Add(compSort)
                        End With
                End Select
            Next
            SortCollection(tempArray, "MilePost", SortAscending)
            Dim compSort1 As clsCompSort
            For Each compSort1 In tempArray
                For Each Comp In CollectionToBeReaaranged
                    If Comp.GUID = compSort1.myUID Then
                        TempCompCollection.Add(Comp)
                        Exit For
                    End If
                Next
            Next
            If gDebug Then
                Comp = Nothing
                For Each Comp In TempCompCollection
                    SystemEvent(Comp.GetType.ToString & Space(2) & Comp.Guid)
                Next
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return TempCompCollection


    End Function

    Friend Function SplitArrayListToLines(ByVal strContent As ArrayList, ByVal itmcount As Integer) As ArrayList
        Try
            Dim Counter As Integer = 1
            Dim conString As String = ""
            Dim RetList As New ArrayList
            Dim itm As String = ""
            For Each itm In strContent
                If Not itm Is Nothing Then
                    If conString.Trim = "" Then
                        conString = itm
                    Else
                        conString = conString & ", " & itm
                    End If
                    Counter = Counter + 1
                    If Counter > itmcount Then
                        RetList.Add(conString)
                        Counter = 1
                        conString = ""
                    End If
                End If
            Next
            If conString <> String.Empty Then
                RetList.Add(conString)
            End If

            Return RetList
        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function

    Friend Function GetLOGOPath() As String
        Dim FilePathForIcon As String = ""

        Try
            Select Case optRailRoadName.ToUpper
                Case "FECR"
                    FilePathForIcon = "\Library\SystemBitmaps\FECR.jpg"
                Case "KANSAS CITY TERMINAL"
                    FilePathForIcon = "\Library\SystemBitmaps\KCS-LOGO.jpg"
                Case "METROLINK"
                    FilePathForIcon = "\Library\SystemBitmaps\METROLINK.jpg"
                Case "NCTD", "NCTD-PTC"
                    FilePathForIcon = "\Library\SystemBitmaps\NCTD.jpg"
                Case "DENVER"
                    FilePathForIcon = "\Library\SystemBitmaps\DTP-logo.jpg"
                Case "ARRC"
                    FilePathForIcon = "\Library\SystemBitmaps\ARRC-logo.bmp"
                Case "MRL"
                    FilePathForIcon = "\Library\SystemBitmaps\MRL-ICON.bmp"
                Case "CP"

                Case "CN"

                Case "IRAQ REPUBLIC"

                Case "DME-ICE"

                Case "TRE"

                Case "INDIANA"

                Case "WATCO"

                Case "BNSF"

                Case "ACTA"

                Case "NMRX"

                Case "KANSAS CITY SOUTHERN"
                    FilePathForIcon = "\Library\SystemBitmaps\KCS-LOGO.jpg"
                Case "KANSAS CITY SOUTHERN MEXICO"

                Case "FERROCARRILES MEXICANOS", "FERROMEX"

                Case "CP-POC"

                Case "BRC"
                    FilePathForIcon = "\Library\SystemBitmaps\BRC.jpg"
                Case "MRS", "RUMO", "TLSA"

                Case Else

            End Select

            If File.Exists(Application.StartupPath & FilePathForIcon) Then
                Return FilePathForIcon
            End If
        Catch ex As Exception
            GetErrorDetails(ex, "GetLOGOPath")
        End Try
        Return String.Empty
    End Function

    Friend Function GetSignalDirection(Sig As Signal, Optional FlipMe As Boolean = False) As String
        Dim Direction As String = ""
        Try
            If FlipMe Then
                Select Case Sig.ComponentType
                    Case "L", "U"
                        Direction = "RIGHT"
                    Case "R", "D"
                        Direction = "LEFT"
                End Select
            Else
                Select Case Sig.ComponentType
                    Case "L", "U"
                        Direction = "LEFT"
                    Case "R", "D"
                        Direction = "RIGHT"
                End Select
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
            Return String.Empty
        End Try
        Return Direction
    End Function

    Public Function IsTrainMoveAllowedOnIndication(trn As TrainID, ByVal CallingMethod As String) As Boolean
        Try
            If Not gMoveTrainID Then Return True
            If gMoveTrainSelectedID Is Nothing Then Return True
            If trn Is Nothing Then Return True
            If gMoveTrainSelectedID.TrainGUID = trn.TrainGUID Then
                SystemEvent(CallingMethod & " - IGNORING INDICATION TRAIN MOVE. SINCE DISPATCHER INITIATED MANUAL TRAIN MOVE FOR TRAIN NAME: " & gMoveTrainSelectedID.Name)
                Return False
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return True
    End Function

    Public Function IsValidCrewName(ByVal cmb As ComboBox) As Boolean
        Try
            Select Case cmb.Text.Trim.ToUpper
                Case "", "----", "NONE", "---", "--", "-"
                    Return False
                Case Else
                    Return True
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try

    End Function
    Public Function IsValidCrewName(ByVal CrewName As String) As Boolean
        Try
            Select Case CrewName.Trim.ToUpper
                Case "", "----", "NONE", "---", "--", "-"
                    Return False
                Case Else
                    Return True
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try

    End Function

    Public Function GetSearchableTrainDirection(ByVal Direction As String) As String

        Try
            Select Case Direction.Trim.ToUpper
                Case optDirectionEast, optDirectionSouth, "RIGHT"
                    Return "RIGHT"
                Case optDirectionNorth, optDirectionWest, "LEFT"
                    Return "LEFT"
                Case Else
                    Return Direction
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

    Public Function GetSearchableTrainDirectionBasedOnSubdivision(ByVal Direction As String, ByVal theSub As Subdivision) As String

        Try
            If theSub Is Nothing Then
                'SystemEvent("GetSearchableTrainDirectionBasedOnSubdivision")
                Return GetSearchableTrainDirection(Direction)
            End If


            Select Case Direction.Trim.ToUpper
                Case optDirectionEast, "SOUTH", "RIGHT"
                    If theSub.SubDirection Then
                        Return "LEFT"
                    Else
                        Return "RIGHT"
                    End If
                Case "NORTH", optDirectionWest, "LEFT"
                    If theSub.SubDirection Then
                        Return "RIGHT"
                    Else
                        Return "LEFT"
                    End If
                Case Else
                    Return Direction
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

    Public Function GetSearchableDirectionBasedOnSubdivision(ByVal theSub As Subdivision) As String

        Try
            If theSub.SubDirection Then
                Return "RIGHT"
            Else
                Return "LEFT"
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

    Public Function ReArrangeCompCollectionWithinCollection(BeginTrack As Track, EndTrack As Track, Direction As String, CompCollection As ArrayList) As ArrayList
        Dim Comp As Object = Nothing
        Dim LastComp As Object = Nothing
        Dim NextComp As Object = Nothing
        Dim FirstTrack As Track = Nothing
        Dim LastTrack As Track = Nothing
        Dim FlipIt As Boolean = False
        Dim SW As Switch = Nothing
        Dim TempArrayList As New ArrayList
        Dim LoopCount As Integer = 0
        Try
            If CompCollection Is Nothing Then Return Nothing
            If CompCollection.Count = 0 Then Return Nothing
            If Direction Is Nothing Then Return Nothing
            If String.IsNullOrEmpty(Direction) Or Direction.Trim = "" Then Return Nothing
            If BeginTrack Is Nothing Then Return Nothing
            If EndTrack Is Nothing Then Return Nothing

            'FirstTrack = GetFirstOrLastTrack(BeginTrack, Direction, CompCollection)
            Select Case Direction.Trim.ToUpper
                Case "RIGHT"
                    FirstTrack = GetFirstOrLastTrack(BeginTrack, "LEFT", CompCollection)
                Case Else
                    FirstTrack = GetFirstOrLastTrack(BeginTrack, "RIGHT", CompCollection)
            End Select
            If FirstTrack Is Nothing Then
                FirstTrack = BeginTrack
            End If




            LastTrack = GetFirstOrLastTrack(EndTrack, Direction, CompCollection)
            If LastTrack Is Nothing Then
                LastTrack = EndTrack
            End If



            TempArrayList = New ArrayList
            LoopCount = (CompCollection.Count + 10)
            TempArrayList.Add(FirstTrack)
            LastComp = FirstTrack
            Select Case Direction.Trim.ToUpper
                Case "RIGHT"
                    FlipIt = False
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(FirstTrack.RightLink)
                    For i As Integer = 0 To LoopCount
                        If NextComp Is Nothing Then Exit For
                        Select Case NextComp.GetType.ToString
                            Case gTrackComponentName
                                LastComp = NextComp
                                'If NextComp Is LastTrack Then Exit For
                                If Not CompCollection.Contains(NextComp) Then Exit For
                                If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If


                            Case gSwitchComponentName
                                SW = Nothing
                                SW = TryCast(NextComp, Switch)
                                Dim ALTLinkUID As Integer = 0
                                If SW IsNot Nothing Then
                                    If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                    If SW.RightAltLink > 0 Then ALTLinkUID = SW.RightAltLink
                                    If SW.LeftAltLink > 0 Then ALTLinkUID = SW.LeftAltLink



                                    If SW.ControlledReverse Then
                                        If LastComp IsNot Nothing AndAlso LastComp.GUID = ALTLinkUID Then
                                            If FlipIt Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            End If
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ALTLinkUID)
                                        End If
                                    Else
                                        If FlipIt Then
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If
                                'Case gSignalComponentName
                                '    If FlipIt Then
                                '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                '    Else
                                '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                '    End If
                            Case gFlipFlopComponentName

                                If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                Dim IsFlipFlopLinkedWrong As Boolean = False
                                If LastComp IsNot Nothing AndAlso LastComp.GUID = NextComp.RightLink Then
                                    IsFlipFlopLinkedWrong = True
                                End If
                                If IsFlipFlopLinkedWrong Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                                If FlipIt Then
                                    FlipIt = False
                                Else
                                    FlipIt = True
                                End If
                            Case Else
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                        End Select
                    Next
                Case Else
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(FirstTrack.LeftLink)
                    FlipIt = False
                    For i As Integer = 0 To LoopCount

                        If NextComp Is Nothing Then Exit For

                        Select Case NextComp.GetType.ToString
                            Case gTrackComponentName
                                LastComp = NextComp
                                'If NextComp Is LastTrack Then Exit For
                                If Not CompCollection.Contains(NextComp) Then Exit For
                                If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If


                            Case gSwitchComponentName
                                SW = Nothing
                                SW = TryCast(NextComp, Switch)
                                Dim ALTLinkUID As Integer = 0
                                If SW IsNot Nothing Then
                                    If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                    If SW.RightAltLink > 0 Then ALTLinkUID = SW.RightAltLink
                                    If SW.LeftAltLink > 0 Then ALTLinkUID = SW.LeftAltLink



                                    If SW.ControlledReverse Then
                                        If LastComp IsNot Nothing AndAlso LastComp.GUID = ALTLinkUID Then
                                            If FlipIt Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            End If
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ALTLinkUID)
                                        End If
                                    Else
                                        If FlipIt Then
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If
                            Case gRRXINGComponentName
                                Dim RR As RRXING = Nothing
                                RR = TryCast(NextComp, RRXING)
                                If RR IsNot Nothing Then
                                    If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                    If LastComp IsNot Nothing Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(RR.FindExitGUID(LastComp.GUID))
                                    End If
                                Else
                                    Exit For
                                End If

                            Case gFlipFlopComponentName
                                'If CompCollection.Contains(NextComp) Then If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                If Not TempArrayList.Contains(NextComp) Then TempArrayList.Add(NextComp)
                                Dim IsFlipFlopLinkedWrong As Boolean = False
                                If LastComp IsNot Nothing AndAlso LastComp.GUID = NextComp.Leftlink Then
                                    IsFlipFlopLinkedWrong = True
                                End If
                                If IsFlipFlopLinkedWrong Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                                If FlipIt Then
                                    FlipIt = False
                                Else
                                    FlipIt = True
                                End If
                                'If FlipIt Then
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                '    FlipIt = False
                                'Else
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                '    FlipIt = True
                                'End If
                            Case Else
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                        End Select
                    Next
            End Select
            'If Not TempArrayList.Contains(LastTrack) Then TempArrayList.Add(LastTrack)
            Dim str As New StringBuilder
            If TempArrayList IsNot Nothing AndAlso TempArrayList.Count > 0 Then
                For i As Integer = 0 To TempArrayList.Count - 1
                    With str
                        .Append(">>>>>>" & TempArrayList(i).GetType.ToString & "  " & TempArrayList(i).GUID.ToString)
                        .Append(vbCrLf)
                    End With
                Next
                SystemEvent(str.ToString)
                Return TempArrayList
            End If


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function


    Public Function GetFirstOrLastTrack(TrackToValidate As Track, Direction As String, CompCollection As ArrayList) As Track

        Dim NextComp As Object = Nothing
        Dim TrackFound As Track = Nothing

        Dim FlipIt As Boolean = False
        Dim SW As Switch = Nothing
        Try
            TrackFound = TrackToValidate
            Select Case Direction.Trim.ToUpper
                Case "LEFT"
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(TrackToValidate.LeftLink)
                    For i As Integer = 0 To 20
                        If NextComp Is Nothing Then Exit For
                        Select Case NextComp.GetType.ToString
                            Case gTrackComponentName
                                If CompCollection.Contains(NextComp) Then
                                    TrackFound = NextComp
                                    If FlipIt Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    Exit For
                                End If
                            Case gSwitchComponentName
                                SW = Nothing
                                SW = TryCast(NextComp, Switch)
                                If SW IsNot Nothing Then
                                    If FlipIt Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    End If
                                Else
                                    Exit For
                                End If
                            Case gFlipFlopComponentName
                                Dim IsFlipFlopLinkedWrong As Boolean = False
                                If TrackFound IsNot Nothing AndAlso TrackFound.GUID = NextComp.RightLink Then
                                    IsFlipFlopLinkedWrong = True
                                End If
                                If IsFlipFlopLinkedWrong Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                                If FlipIt Then
                                    FlipIt = False
                                Else
                                    FlipIt = True
                                End If
                                'If FlipIt Then
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                '    FlipIt = False
                                'Else
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                '    FlipIt = True
                                'End If
                            Case Else
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                        End Select
                    Next

                Case Else
                    FlipIt = False
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(TrackToValidate.RightLink)
                    For i As Integer = 0 To 20
                        If NextComp Is Nothing Then Exit For
                        Select Case NextComp.GetType.ToString
                            Case gTrackComponentName
                                If CompCollection.Contains(NextComp) Then
                                    TrackFound = NextComp
                                    If FlipIt Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    End If
                                Else
                                    Exit For
                                End If
                            Case gSwitchComponentName
                                SW = Nothing
                                SW = TryCast(NextComp, Switch)
                                If SW IsNot Nothing Then
                                    If FlipIt Then
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    Else
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    End If
                                Else
                                    Exit For
                                End If
                            Case gFlipFlopComponentName
                                Dim IsFlipFlopLinkedWrong As Boolean = False
                                If TrackFound IsNot Nothing AndAlso TrackFound.GUID = NextComp.RightLink Then
                                    IsFlipFlopLinkedWrong = True
                                End If
                                If IsFlipFlopLinkedWrong Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                                If FlipIt Then
                                    FlipIt = False
                                Else
                                    FlipIt = True
                                End If
                                'If FlipIt Then
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                '    FlipIt = False
                                'Else
                                '    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                '    FlipIt = True
                                'End If
                            Case Else
                                If FlipIt Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                        End Select
                    Next
            End Select
            Return TrackFound
        Catch ex As Exception

        End Try
        Return Nothing
    End Function

    Public Function IsValidString(StringToCheck As String) As Boolean
        Try
            If StringToCheck Is Nothing Then Return False
            If String.IsNullOrEmpty(StringToCheck) Then Return False
            If StringToCheck.Trim = "" Then Return False
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

#End Region

#Region " Stretchy Tag Check Function "

    Public Function OkToIssueTagWithAuthority() As Boolean
        Dim trk As Object = Nothing 'Track = gTagTrackSelectionList.Item(0)
        Try
            If Not gTagTrackSelectionList Is Nothing Then
                If gTagTrackSelectionList.Count > 0 Then
                    For Each trk In gTagTrackSelectionList
                        If trk.GetType.ToString = gTrackComponentName Then
                            If trk.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop Or trk.Occuppied Or trk.ActiveTrackWarrant Then
                                gTagAuthorityCheckDone = True
                                Dim frmMessageBox As frmMessageBox
                                Dim MessageList As New ArrayList()
                                If Mid(trk.AuthorityType, 1, 3) = "TWC" Then
                                    MessageList.Add(optSystemMessageString11)
                                Else
                                    MessageList.Add(optSystemMessageString12)
                                End If
                                MessageList.Add(optSystemMessageString14)
                                MessageList.Add("")
                                frmMessageBox = New frmMessageBox()
                                frmMessageBox.Owner = trk.CurrentDrawingForm
                                frmMessageBox.StartPosition = FormStartPosition.Manual
                                frmMessageBox.SetMessage(MessageList, optSystemMessageString13, True, trk.CurrentDrawingForm.myLaunchPoint)
                                frmMessageBox.ShowDialog()
                                If frmMessageBox.FormFeedBack <> 6 Then
                                    frmMessageBox.Dispose()
                                    frmMessageBox = Nothing
                                    TMDS.modSystemFuncs.ClearSystemVariables()
                                    Return False
                                    Exit Function
                                End If
                                frmMessageBox.Dispose()
                                frmMessageBox = Nothing
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            trk = Nothing
        End Try
    End Function

#End Region

#Region " GLOBAL TGBO/GBO FUNCTIONS "

    Public Function GetMileageRangeFromUID(ByVal UID As String) As MileageRange
        Dim MR As MileageRange
        Try
            For Each MR In TMDS.GlobalDeclarations.gMileageRangeList
                If MR.myUID = UID Then
                    Return MR
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function GetMileageRangeFromMP(ByVal MilePost As Decimal, ByVal Suffix As String) As MileageRange
        Dim MR As MileageRange
        Try
            For Each MR In TMDS.GlobalDeclarations.gMileageRangeList
                If MilePost >= MR.LeftMileage And MilePost <= MR.RightMileage And (MR.LeftMileageSuffix.Trim = Suffix.Trim) Then
                    Return MR
                ElseIf MilePost >= MR.RightMileage And MilePost <= MR.LeftMileage And (MR.RightMileageSuffix.Trim = Suffix.Trim) Then
                    Return MR
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function GetBulletinCellFromUID(ByVal UID As Integer) As BulletinCell
        Dim BC As BulletinCell
        Try
            For Each BC In TMDS.GlobalDeclarations.gBulletinCellRouteList
                If BC.myUID = UID Then
                    Return BC
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function GetBulletinCellFromName(ByVal pName As String) As BulletinCell
        Dim BC As BulletinCell
        Try
            For Each BC In TMDS.GlobalDeclarations.gBulletinCellRouteList
                If BC.myName = pName Then
                    Return BC
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function CreateGTBAlert(ByVal AlertData As String, ByVal GTBguid As String, ByVal GTBAlertType As Alert.AlertType) As Boolean
        Dim ExistingAlert As Alert = Nothing
        Try

            For Each ExistingAlert In gSystemAlertList
                If ExistingAlert.AlertObjectUID = GTBguid Then
                    Exit Function
                End If
                'If ExistingAlert.AlertObjectUID = GTBguid And ExistingAlert.TypeOfAlert = Alert.AlertType.GTBEXCEPTIONRedAlert Then
                '    Exit Sub
                'ElseIf ExistingAlert.AlertObjectUID = GTBguid And ExistingAlert.TypeOfAlert = Alert.AlertType.GTBEXCEPTIONYellowAlert Then
                '    Exit Sub
                'End If
            Next

            Dim al As Alert = New Alert(GTBAlertType, AlertData, Nothing, Nothing)
            If Not IsNothing(al) Then
                al.AlertObjectUID = GTBguid
                al.AlertTime = Now
                SyncLock TMDS.GlobalDeclarations.gSystemAlertQueue.SyncRoot
                    TMDS.GlobalDeclarations.gSystemAlertQueue.Enqueue(al)
                End SyncLock
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            ExistingAlert = Nothing
        End Try
        Return False
    End Function

    Public Function LoadGTBAlert(ByVal AlertData As String, ByVal GTBguid As String, ByVal GTBAlertType As Alert.AlertType) As Boolean
        Dim ExistingAlert As Alert = Nothing
        Try

            For Each ExistingAlert In gSystemAlertList
                If ExistingAlert.AlertObjectUID = GTBguid Then
                    Exit Function
                End If
            Next

            Dim al As Alert = New Alert(GTBAlertType, AlertData, Nothing, Nothing)
            If Not IsNothing(al) Then
                al.AlertObjectUID = GTBguid
                al.AlertTime = Now
                SyncLock TMDS.GlobalDeclarations.gSystemAlertQueue.SyncRoot
                    TMDS.GlobalDeclarations.gSystemAlertQueue.Enqueue(al)
                End SyncLock
            End If

            'SyncLock TMDS.GlobalDeclarations.gSystemAlertList.SyncRoot
            '    TMDS.GlobalDeclarations.gSystemAlertList.Remove(al)
            'End SyncLock

            'RemoveGTBExceptionFromDB
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            ExistingAlert = Nothing
        End Try
        Return False
    End Function

    Public Function FindBulletinCellByUIDFromDB(ByVal UID As Integer) As BulletinCell
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim Query As String = "SELECT * FROM tblBulletinCells WHERE UID=" & UID

        Try

            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(Query, dbconn)
            dr = dbcomm.ExecuteReader

            While dr.Read
                Dim BC As New BulletinCell
                With BC
                    If Not dr.IsDBNull(0) Then .myUID = dr.GetInt32(0)
                    If Not dr.IsDBNull(1) Then .myName = dr.GetString(1)
                    If Not dr.IsDBNull(2) Then .CellType = dr.GetString(2)
                    If Not dr.IsDBNull(3) Then .MileageRangeDefinitions = dr.GetString(3)
                    If Not dr.IsDBNull(4) Then .StartSubdivisionUID = dr.GetInt32(4)
                    If Not dr.IsDBNull(5) Then .EndSubdivisionUID = dr.GetInt32(5)
                End With
                BC.CreateMileageRanges()
                Return BC
            End While

        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            dbconn.Close()
            dbconn = Nothing
            dbcomm = Nothing
            dr = Nothing
        End Try
        Return Nothing
    End Function

    Public Function CreateBCExceptionList(ByVal sExceptions As String, ByVal b As SubBulletin) As ArrayList
        Dim ExcepList As New ArrayList
        Try

            If Trim(sExceptions) = "" Then Return ExcepList
            Dim Exceps() As String = sExceptions.Split("|")
            Dim e As String
            For Each e In Exceps
                Dim data() As String = e.Split(";")
                If data.Length >= 5 Then
                    Dim ex As New TabularGeneralBulletinException
                    With ex
                        ex.SystemUID = data(0)
                        ex.theDesignation = data(1)
                        ex.theTBGOSytemUID = data(2)
                        ex.theTGBORouteCodeName = data(3)
                        ex.theTBGOUID = data(4)
                        ex.theGBOUID = b.myGUID
                        ex.theGBO = b
                        ex.theSubdivision = b.thisSubdivision
                        ex.theSubUID = b.thisSubdivision.GUID

                    End With
                    ExcepList.Add(ex)
                End If
            Next
            Return ExcepList
        Catch exp As Exception
            GetErrorDetails(exp)
            Return Nothing
        End Try
    End Function

    Public Function CheckIfTrainHasValidTGBOForAuthority(ByVal CompList As ArrayList, ByVal TrainName As String, Optional ByRef MyTGBO As TabularGeneralBulletin = Nothing, Optional ByVal DoLimtCheckWithTGBO As Boolean = True) As Boolean
        Dim trn As TrainID = Nothing ' TMDS.SystemUtilities.FetchData.FindTrainFromIssueToAuthority(Me.cbIssueTo.Text)
        Dim theTGBO As TabularGeneralBulletin = Nothing
        Dim mrc As MileageRange = Nothing ' MileageRangeBulletinContainer
        Dim myTrackSubdivision As New ArrayList
        Dim comp As Object = Nothing
        Dim myTGBOMileageRanges As New ArrayList
        Dim S As String = ""
        Dim StrSub As String = ""
        Dim SubDivisionTracks As New ArrayList
        Dim myValidatedComp As New ArrayList
        Dim CompToValidate As New ArrayList
        Dim MySubMileageRange As MileageRange = Nothing
        Try
            '************** MileageRangeListDefinitions
            '1002;CENTRAL-01;2;CENTRAL;102;;197;|1009;CENTRAL-02;2;CENTRAL;0;;0.11;|1010;CENTRAL-03;2;CENTRAL;0;;0.1;|1000;KENAI-01;1;KENAI;3.42;;102;|1008;KENAI-02;1;KENAI;2.55;;12.45;
            'mr.UID & ";" & mr.Name & ";" & mr.SubdivisionUID & ";" & mr.SubdivisionName & ";" & mr.LeftMileage & ";" & mr.LeftMileageSuffix & ";" & mr.RightMileage & ";" & mr.RightMileageSuffix & ";" & mr.LeftMileagePrefix.Trim & ";" & mr.RightMileagePrefix.Trim
            '***********************************************************************************
            If TrainName.Trim = "" Then Return False
            trn = TMDS.SystemUtilities.FetchData.FindTrainFromIssueToAuthority(TrainName)
            If trn Is Nothing Then Return True
            If trn IsNot Nothing Then theTGBO = TMDS.SystemUtilities.FetchData.FindTGBOByTrainSymbol(trn.Name, True)
            If theTGBO Is Nothing Then Return False
            If theTGBO.Status.ToUpper.Trim <> "ACTIVE" Then Return False
            MyTGBO = theTGBO
            If optEnableTGBOCheckOnAuthority AndAlso DoLimtCheckWithTGBO AndAlso CompList IsNot Nothing AndAlso CompList.Count > 0 Then
                For Each comp In CompList
                    If comp.GetType.ToString = gTrackComponentName Then
                        Dim trk As Track = CType(comp, Track)

                        If comp.mySubdivision IsNot Nothing AndAlso trk.AuthorityType <> "CTC-OTHER" Then
                            If Not myTrackSubdivision.Contains(comp.mySubdivision.Name.ToUpper.Trim) Then myTrackSubdivision.Add(comp.mySubdivision.Name.ToUpper.Trim)
                            If Not CompToValidate.Contains(comp) Then CompToValidate.Add(comp)
                        End If
                    End If
                Next
            Else
                Return True
            End If

            If myTrackSubdivision IsNot Nothing AndAlso myTrackSubdivision.Count > 0 Then
                For Each StrSub In myTrackSubdivision
                    Dim HasValidSubdivision As Boolean = False
                    Dim Data() As String = theTGBO.MileageRangeListDefinitions.Split("|")

                    For Each S In Data
                        MySubMileageRange = Nothing
                        If S.Trim <> "" Then
                            Dim LeftMP As Double = 0
                            Dim LeftMPSuffix As String = String.Empty
                            Dim LeftMPPrefix As String = String.Empty
                            Dim RightMP As Double = 0
                            Dim RightMPSuffix As String = String.Empty
                            Dim RightMPPrefix As String = String.Empty

                            Dim MRstr() As String = S.Split(";")
                            Dim mrUID As Integer = 0
                            Integer.TryParse(MRstr(0), mrUID)
                            If mrUID > 0 Then
                                Dim Mr As MileageRange = Nothing
                                Mr = GetMileageRangeFromUID(mrUID)
                                If Mr IsNot Nothing Then
                                    MySubMileageRange = New MileageRange
                                    With MySubMileageRange
                                        .myUID = Mr.myUID
                                        .mySubdivisionUID = Mr.mySubdivisionUID
                                        .mySubdivisionName = Mr.mySubdivisionName
                                        .mySubdivision = Mr.mySubdivision
                                        .myName = Mr.myName
                                        .myDescription = Mr.myDescription
                                    End With
                                    Double.TryParse(MRstr(4), LeftMP)
                                    If Not String.IsNullOrEmpty(MRstr(5)) Then LeftMPSuffix = MRstr(5).ToUpper.Trim
                                    Double.TryParse(MRstr(6), RightMP)
                                    If Not String.IsNullOrEmpty(MRstr(7)) Then RightMPSuffix = MRstr(7).ToUpper.Trim
                                    If MRstr.GetUpperBound(0) > 7 Then
                                        If Not String.IsNullOrEmpty(MRstr(8)) Then LeftMPPrefix = MRstr(8).ToUpper.Trim
                                        If Not String.IsNullOrEmpty(MRstr(9)) Then RightMPPrefix = MRstr(9).ToUpper.Trim
                                    Else
                                        LeftMPPrefix = Mr.LeftMileagePrefix.ToUpper.Trim
                                        RightMPPrefix = Mr.RightMileagePrefix.ToUpper.Trim
                                    End If
                                    With MySubMileageRange
                                        .LeftMileage = LeftMP
                                        .LeftMileageSuffix = LeftMPSuffix
                                        .LeftMileagePrefix = LeftMPPrefix
                                        .RightMileage = RightMP
                                        If MRstr.GetUpperBound(0) > 7 Then
                                            .RightMileageSuffix = RightMPSuffix
                                            .RightMileagePrefix = RightMPPrefix
                                        End If
                                    End With
                                    If Not myTGBOMileageRanges.Contains(MySubMileageRange) Then myTGBOMileageRanges.Add(MySubMileageRange)
                                End If
                            End If
                        End If
                    Next
                    For Each mrc In myTGBOMileageRanges
                        If myTrackSubdivision.Contains(mrc.mySubdivisionName.ToUpper.Trim) Then HasValidSubdivision = True : MySubMileageRange = mrc : Exit For
                    Next
                    If Not HasValidSubdivision Then
                        Return False
                    End If
                Next
                MySubMileageRange = Nothing
                For Each StrSub In myTrackSubdivision
                    SubDivisionTracks = New ArrayList

                    'Check For AuthorityLimits
                    For Each comp In CompToValidate
                        If comp.GetType.ToString = gTrackComponentName Then
                            If comp.mySubdivision IsNot Nothing Then
                                If comp.mySubdivision.Name.ToUpper.Trim = StrSub.ToUpper.Trim Then
                                    If Not SubDivisionTracks.Contains(comp) Then SubDivisionTracks.Add(comp)
                                End If
                            End If
                        End If
                    Next

                    If SubDivisionTracks IsNot Nothing AndAlso SubDivisionTracks.Count > 0 Then
                        comp = Nothing
                        Dim HasValidRouteCode As Boolean = False
                        For Each mrc In myTGBOMileageRanges
                            If mrc.mySubdivisionName.ToUpper.Trim = StrSub.ToUpper.Trim Then
                                For Each comp In SubDivisionTracks
                                    If Not myValidatedComp.Contains(comp) AndAlso
                                    TMDS.SystemUtilities.FetchData.BetweenNumbers(mrc.LeftMileage, mrc.RightMileage, comp.LeftLimitMilePost, comp.RightLimitMilePost) AndAlso
                                    ValidateSuffix_Prefix(mrc, comp) Then
                                        If Not myValidatedComp.Contains(comp) Then myValidatedComp.Add(comp)
                                    End If
                                Next
                            End If

                        Next
                    End If
                Next
                If myValidatedComp.Count <> CompToValidate.Count Then
                    Return False
                End If
            Else
                Return True
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            trn = Nothing
            'MyTGBO = Nothing
            mrc = Nothing
            'theTGBO = Nothing
            myTrackSubdivision = Nothing
            myTGBOMileageRanges = Nothing
            SubDivisionTracks = Nothing
            myValidatedComp = Nothing
            CompToValidate = Nothing
            comp = Nothing
            mrc = Nothing
            MySubMileageRange = Nothing
        End Try
    End Function

    Private Function ValidateSuffix_Prefix(ByVal MR As MileageRange, ByVal trk As Track) As Boolean
        Try
            If (MR.LeftMileageSuffix.ToUpper.Trim = trk.LeftLimitMilePostSuffix.ToUpper.Trim AndAlso MR.LeftMileagePrefix.ToUpper.Trim = trk.LeftLimitMilePostPrefix.ToUpper.Trim) AndAlso (MR.RightMileageSuffix.ToUpper.Trim = trk.RightLimitMilePostSuffix.ToUpper.Trim AndAlso MR.RightMileagePrefix.ToUpper.Trim = trk.RightLimitMilePostPrefix.ToUpper.Trim) Then
                Return True
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Private Function ValidateSuffix_Prefix(ByVal LeftSuffix As String, ByVal LeftPrefix As String, ByVal RightSuffix As String, ByVal RightPrefix As String, ByVal LeftSuffixToCheck As String, ByVal LeftPrefixToCheck As String, ByVal RightSuffixToCheck As String, ByVal RightPrefixToCheck As String) As Boolean

        Try
            If (LeftSuffix.ToUpper.Trim = LeftSuffixToCheck.ToUpper.Trim AndAlso LeftPrefix.ToUpper.Trim = LeftPrefixToCheck.ToUpper.Trim) AndAlso (RightSuffix.ToUpper.Trim = RightSuffixToCheck.ToUpper.Trim AndAlso RightPrefix.ToUpper.Trim = RightPrefixToCheck.ToUpper.Trim) Then
                Return True
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Friend Function GetBulletinType(ByVal theBulletin As SubBulletin) As String
        Dim theBullType As String = "BULL"
        Try
            If IsNothing(theBulletin) Then Return "BULLETIN"
            Select Case theBulletin.mysTypeForm
                Case "FORM A"
                    theBullType = sBulletinTypeSR
                Case "FORM B"
                    theBullType = sBulletinTypeWZ
                Case "FORM C"
                    theBullType = sBulletinTypeDim
                Case "FORM F"
                    theBullType = sBulletinTypeInstructional
                Case "FORM O"
                    theBullType = sBulletinTypeOOS
                Case "FORM S"
                    theBullType = sBulletinTypeSlide
                Case "FORM X"
                    theBullType = sBulletinTypeXing
                Case "FORMTC"
                    theBullType = sBulletinTypeTrackCondition
                Case Else
                    theBullType = theBulletin.mysTypeForm
            End Select
            Return theBullType
        Catch ex As Exception
            GetErrorDetails(ex)
            Return theBulletin.mysTypeForm
        End Try
    End Function

    Public Sub DoTGBOSummaryRefresh(CallingMethodName As String)
        If Not gInitialized Then Exit Sub
        Dim UseQueue As Boolean = True
        Try
            If Not UseQueue Then
                RefreshGBO(CallingMethodName) 'CN-1203
                RefreshTGBO(CallingMethodName)
            Else
                Dim SummItem As New SummaryItem("TGBO", "REFRESH", Nothing, "DoTGBOSummaryRefresh_" & CallingMethodName)
                SyncLock gSubBulletinsMessageSummaryQueue
                    gSubBulletinsMessageSummaryQueue.Enqueue(SummItem)
                End SyncLock
            End If
            If CallingMethodName IsNot Nothing AndAlso CallingMethodName.Trim <> "" Then SystemEvent("<REFRESH EVENT>DoTGBOSummaryRefresh called by " & CallingMethodName.Trim)
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub DoGBOSummaryRefresh(CallingMethodName As String)
        If Not gInitialized Then Exit Sub
        Dim UseQueue As Boolean = True
        Try
            If Not UseQueue Then
                RefreshGBO(CallingMethodName)
            Else
                Dim SummItem1 As New SummaryItem("SUB-BULLETIN", "REFRESH", Nothing, "DoGBOSummaryRefresh_" & CallingMethodName)
                SyncLock gSubBulletinsMessageSummaryQueue
                    gSubBulletinsMessageSummaryQueue.Enqueue(SummItem1)
                End SyncLock
            End If
            If CallingMethodName IsNot Nothing AndAlso CallingMethodName.Trim <> "" Then SystemEvent("<REFRESH EVENT>DoGBOSummaryRefresh called by " & CallingMethodName.Trim)
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub


    Private Function RefreshTGBO(CallingMethodName As String) As Boolean
        Try
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function
    Private Function RefreshGBO(CallingMethodName As String) As Boolean
        Try
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

#End Region

#Region " HIGHLIGHT OR HATCH TRACKS "

    Public Sub HighLightTATAuthority(ByVal IssueTo As String, Optional ByVal TrainSymbol As String = "", Optional ByVal IgnoreAuthUID As String = "")
        If Not optEnableHighLightTrack Then Exit Sub
        Dim objAuth As Object = Nothing
        Dim comp As Object = Nothing
        'Dim mw As MOWID = gMowMenuMiscSelectedID
        Dim SysMsg As String = IssueTo & " HAS OTHER TRACK AUTHORITY(S) UNACKNOWLEDGED PLEASE ACKNOWLEDGED TO PROCEED. EXITING!"
        gHighlightedTracks = New ArrayList
        Try
            Dim tat As TrackAuthority.TrackAuthority = Nothing
            For Each objAuth In OtherTrackAuthorities
                If objAuth.GetType.ToString = gTTComponentName Then
                    If objAuth.IssueTo = IssueTo Or (TrainSymbol.Trim <> "" And objAuth.TrainSymbol = TrainSymbol) Then
                        If IgnoreAuthUID.Trim <> objAuth.AuthorityUID Then
                            For Each comp In objAuth.ComponentsInSelection
                                If comp.GetType.ToString = gTrackComponentName Then
                                    If Not gHighlightedTracks.Contains(comp) Then gHighlightedTracks.Add(comp)
                                    comp.SelectionMode = GenericComponent.ModeSelection.HighLight
                                    comp.Refresh()
                                End If
                            Next
                        End If
                    End If
                ElseIf objAuth.GetType.ToString = gSFAComponentName Then
                    If objAuth.IssueTo = IssueTo Or (TrainSymbol.Trim <> "" And objAuth.TrainSymbol = TrainSymbol) Then
                        'Dim sfa As TMDS.TrackAuthority.SFAuthority
                        For Each comp In objAuth.Components
                            If comp.GetType.ToString = gTrackComponentName Then
                                If Not gHighlightedTracks.Contains(comp) Then gHighlightedTracks.Add(comp)
                                comp.SelectionMode = GenericComponent.ModeSelection.HighLight
                                comp.Refresh()
                            End If
                        Next
                    End If
                End If
            Next
            Dim splAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType = Nothing
            For Each splAuth In gOtherSpecialAuthorityCollection
                If (splAuth.IssueTo = IssueTo) Or (TrainSymbol.Trim <> "" And splAuth.TrainSymbol = TrainSymbol) Then
                    For Each comp In splAuth.ComponentsInSelection
                        If comp.GetType.ToString = gTrackComponentName Then
                            If Not gHighlightedTracks.Contains(comp) Then gHighlightedTracks.Add(comp)
                            comp.SelectionMode = GenericComponent.ModeSelection.HighLight
                            comp.Refresh()
                        End If
                    Next
                End If
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            objAuth = Nothing
            comp = Nothing
        End Try
    End Sub

    Public Sub RemoveHighLightMWsAuthority()
        If Not optEnableHighLightTrack Then Exit Sub
        Dim objAuth As Object = Nothing
        Dim comp As Object = Nothing


        Try
            If Not gHighlightedTracks Is Nothing AndAlso gHighlightedTracks.Count > 0 Then
                For Each comp In gHighlightedTracks
                    If comp.GetType.ToString = gTrackComponentName Then
                        comp.SelectionMode = GenericComponent.ModeSelection.Regular
                        comp.Refresh()
                    End If
                Next
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            objAuth = Nothing
            comp = Nothing
            gHighlightedTracks = New ArrayList
        End Try
    End Sub

#End Region

#Region " TIWS "

    'Public Function GetAdjacentZones(ByVal ZoneTrack As Track, ByVal Direction As String, ByRef TrackedZone As Track) As String
    '    Dim CurrentTrackZoneID As Integer
    '    Dim Comp As Object = Nothing
    '    Try
    '        If ZoneTrack.TrackZoneIntegrityMonitoringEnabled Then
    '            CurrentTrackZoneID = ZoneTrack.TrackZoneReference.ZoneUID
    '            Select Case Direction.Trim.ToUpper
    '                Case "RIGHT"
    '                    If gDebug Then SystemEvent("GetAdjacentZones RIGHT LINK:" & ZoneTrack.RightLink)
    '                    If ZoneTrack.RightLink > 0 Then
    '                        Comp = FindCompFromGUID(ZoneTrack.RightLink)
    '                        If Comp Is Nothing Then Return String.Empty
    '                        For i As Integer = 1 To 40
    '                            If Comp Is Nothing Then Return String.Empty
    '                            Select Case Comp.GetType.ToString
    '                                Case gTrackComponentName
    '                                    If Comp.TrackZoneIntegrityMonitoringEnabled Then
    '                                        If CurrentTrackZoneID = Comp.TrackZoneReference.ZoneUID Then
    '                                            Comp = FindCompFromGUID(Comp.RightLink)
    '                                        Else
    '                                            TrackedZone = Comp
    '                                            Return Comp.TrackZoneReference.ZoneUID.ToString
    '                                        End If
    '                                    Else
    '                                        Return String.Empty
    '                                    End If
    '                                Case Else 'gSwitchComponentName
    '                                    Comp = FindCompFromGUID(Comp.RightLink)
    '                            End Select
    '                        Next
    '                    Else
    '                        Return ZoneTrack.TrackZoneReference.ZoneUID
    '                    End If

    '                Case "LEFT"
    '                    If gDebug Then SystemEvent("GetAdjacentZones RIGHT LINK:" & ZoneTrack.LeftLink)
    '                    If ZoneTrack.LeftLink > 0 Then
    '                        Comp = FindCompFromGUID(ZoneTrack.LeftLink)
    '                        If Comp Is Nothing Then Return String.Empty
    '                        For i As Integer = 1 To 40
    '                            If Comp Is Nothing Then Return String.Empty
    '                            Select Case Comp.GetType.ToString
    '                                Case gTrackComponentName
    '                                    If Comp.TrackZoneIntegrityMonitoringEnabled Then
    '                                        If CurrentTrackZoneID = Comp.TrackZoneReference.ZoneUID Then
    '                                            Comp = FindCompFromGUID(Comp.LeftLink)
    '                                        Else
    '                                            TrackedZone = Comp
    '                                            Return Comp.TrackZoneReference.ZoneUID.ToString
    '                                        End If
    '                                    Else
    '                                        Return String.Empty
    '                                    End If
    '                                Case Else 'gSwitchComponentName
    '                                    Comp = FindCompFromGUID(Comp.LeftLink)
    '                            End Select
    '                        Next
    '                    Else
    '                        Return ZoneTrack.TrackZoneReference.ZoneUID
    '                    End If

    '                Case Else
    '                    Return String.Empty
    '            End Select
    '        End If
    '        'ZoneTrack.trackz()
    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    Finally
    '        Comp = Nothing
    '    End Try
    '    Return String.Empty
    'End Function

    Public Function GetAllContinuouslyOccuipiedTrackZones(ByVal TrackZoneToCheck As TrackZone) As ArrayList
        Dim LeftZoneCollection As New ArrayList
        Dim RightZoneCollection As New ArrayList
        Dim ReturnZoneCollection As New ArrayList
        Dim TrackedZonetrack As Track = Nothing
        Dim TrackToCheck As Track
        Dim tz As TrackZone = Nothing
        Try

            TrackToCheck = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(TrackZoneToCheck.TrackList)
            If Not TrackToCheck.TrackZoneIntegrityMonitoringEnabled Then
                ReturnZoneCollection.Add(TrackZoneToCheck)
                Return ReturnZoneCollection
            ElseIf Not TrackToCheck.Occuppied Then
                ReturnZoneCollection.Add(TrackZoneToCheck)
                Return ReturnZoneCollection
            End If


            For i As Integer = 0 To TMDS.GlobalDeclarations.gTrackZones.Count + 1
                If Not TrackToCheck Is Nothing Then
                    GetAdjacentZones(TrackToCheck, "LEFT", TrackedZonetrack)
                    If TrackedZonetrack IsNot Nothing AndAlso TrackedZonetrack.TrackZoneIntegrityMonitoringEnabled AndAlso TrackedZonetrack.Occuppied Then
                        LeftZoneCollection.Add(TrackedZonetrack.TrackZoneReference)
                        TrackToCheck = TrackedZonetrack
                        TrackedZonetrack = Nothing
                    Else
                        Exit For
                    End If
                End If
            Next
            If LeftZoneCollection IsNot Nothing AndAlso LeftZoneCollection.Count > 0 Then
                LeftZoneCollection.Reverse()
                For Each tz In LeftZoneCollection
                    If Not ReturnZoneCollection.Contains(tz) Then ReturnZoneCollection.Add(tz)
                Next
            End If

            If Not ReturnZoneCollection.Contains(TrackZoneToCheck) Then ReturnZoneCollection.Add(TrackZoneToCheck)

            TrackedZonetrack = Nothing
            TrackToCheck = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(TrackZoneToCheck.TrackList)

            For i As Integer = 0 To TMDS.GlobalDeclarations.gTrackZones.Count + 1
                If Not TrackToCheck Is Nothing Then
                    GetAdjacentZones(TrackToCheck, "RIGHT", TrackedZonetrack)
                    If TrackedZonetrack IsNot Nothing AndAlso TrackedZonetrack.TrackZoneIntegrityMonitoringEnabled AndAlso TrackedZonetrack.Occuppied Then
                        RightZoneCollection.Add(TrackedZonetrack.TrackZoneReference)
                        TrackToCheck = TrackedZonetrack
                        TrackedZonetrack = Nothing
                    Else
                        Exit For
                    End If
                End If
            Next
            If RightZoneCollection IsNot Nothing AndAlso RightZoneCollection.Count > 0 Then
                For Each tz In RightZoneCollection
                    If Not ReturnZoneCollection.Contains(tz) Then ReturnZoneCollection.Add(tz)
                Next
            End If
            Return ReturnZoneCollection
        Catch ex As Exception
            GetErrorDetails(ex)
            ReturnZoneCollection.Add(TrackZoneToCheck)
            Return ReturnZoneCollection
        Finally
            LeftZoneCollection = Nothing
            RightZoneCollection = Nothing
            TrackedZonetrack = Nothing
            TrackToCheck = Nothing
            tz = Nothing
        End Try


    End Function

    Public Class TIWSZoneStatus
        Public ZoneName As String = ""
        Public OccupancyStatus As Boolean
        Public ZoneHasTrain As Boolean
        Public ZoneTrainName As String = ""
        Public ZoneOrder As Integer = 0
    End Class
    Private Sub AddTIWSZoneStatus(ByRef TmpZone As ArrayList, trk As Track, ByVal Auth As TMDS.TrackAuthority.SFAuthority)
        Dim tiwsZS As TIWSZoneStatus = Nothing
        Dim tmpTIWSZS As New TIWSZoneStatus
        Dim TempZoneList As New ArrayList
        Dim UpdateDone As Boolean = False
        Try
            If trk Is Nothing Then Exit Sub
            If trk.TrackZoneReference Is Nothing Then Exit Sub
            If TmpZone Is Nothing Then TmpZone = New ArrayList
            If TmpZone.Count = 0 Then TmpZone = New ArrayList
            If TmpZone.Count > 0 Then
                CopyArrayCollection(TempZoneList, TmpZone)
            End If
            If trk.TrackZoneReference IsNot Nothing Then
                With tmpTIWSZS
                    .ZoneName = trk.TrackZoneReference.ZoneName.ToUpper
                    If Auth.LastTrainZone IsNot Nothing AndAlso Auth.LastTrainZone.ZoneName.ToUpper.Trim = trk.TrackZoneReference.ZoneName.ToUpper.Trim Then
                        .ZoneHasTrain = True
                    End If
                    Dim tmpTrack As Track = Nothing
                    If trk.TrackZoneReference.TrackList IsNot Nothing AndAlso trk.TrackZoneReference.TrackList.Count > 0 Then
                        For Each tmpTrack In trk.TrackZoneReference.TrackList
                            If tmpTrack IsNot Nothing AndAlso tmpTrack.GetType.ToString = gTrackComponentName Then
                                If tmpTrack.Occuppied Then .OccupancyStatus = True
                            End If
                        Next
                    End If
                    .ZoneOrder = TmpZone.Count + 1
                    '.OccupancyStatus = trk.Occuppied
                    'If trk.TrainIDs IsNot Nothing AndAlso trk.TrainIDs.Count > 0 AndAlso trk.TrainIDs.Contains(trn) Then
                    '    .ZoneHasTrain = True
                    '    .ZoneTrainName = trn.Name
                    'End If
                End With
            End If
            For Each tiwsZS In TmpZone
                If tiwsZS.ZoneName.Trim.ToUpper = tmpTIWSZS.ZoneName.Trim.ToUpper Then
                    If Not tmpTIWSZS.OccupancyStatus Then tiwsZS.OccupancyStatus = tmpTIWSZS.OccupancyStatus
                    'If tmpTIWSZS.ZoneHasTrain AndAlso tmpTIWSZS.OccupancyStatus Then
                    '    tiwsZS.ZoneHasTrain = tmpTIWSZS.ZoneHasTrain
                    '    tiwsZS.ZoneTrainName = tmpTIWSZS.ZoneTrainName
                    'End If

                    UpdateDone = True
                    Exit For
                End If
            Next
            If Not UpdateDone Then
                TmpZone.Add(tmpTIWSZS)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

    End Sub


    Public Function GetAdjacentZones(ByVal ZoneTrack As Track, ByVal Direction As String, ByRef TrackedZone As Track) As String
        Dim CurrentTrackZoneID As Integer
        Dim Comp As Object = Nothing
        Try
            If ZoneTrack.TrackZoneIntegrityMonitoringEnabled Then
                CurrentTrackZoneID = ZoneTrack.TrackZoneReference.ZoneUID
                Select Case Direction.Trim.ToUpper
                    Case "RIGHT"
                        If ZoneTrack.RightLink > 0 Then
                            Comp = FindCompFromGUID(ZoneTrack.RightLink)
                            If Comp Is Nothing Then Return String.Empty
                            For i As Integer = 1 To 40
                                If Comp Is Nothing Then Return String.Empty
                                Select Case Comp.GetType.ToString
                                    Case gTrackComponentName
                                        If Comp.TrackZoneIntegrityMonitoringEnabled Then
                                            If CurrentTrackZoneID = Comp.TrackZoneReference.ZoneUID Then
                                                Comp = FindCompFromGUID(Comp.RightLink)
                                            Else
                                                TrackedZone = Comp
                                                Return Comp.TrackZoneReference.ZoneUID.ToString
                                            End If
                                        Else
                                            Return String.Empty
                                        End If
                                    Case gSwitchComponentName
                                        Dim sw As Switch
                                        sw = DirectCast(Comp, Switch)
                                        If sw.ControlledReverse AndAlso sw.RightAltLink > 0 Then
                                            Comp = FindCompFromGUID(sw.RightAltLink)
                                        Else
                                            Comp = FindCompFromGUID(sw.RightLink)
                                        End If
                                    Case Else
                                        Comp = FindCompFromGUID(Comp.RightLink)
                                End Select
                            Next
                        Else
                            Return ZoneTrack.TrackZoneReference.ZoneUID
                        End If
                    Case "LEFT"
                        If ZoneTrack.LeftLink > 0 Then
                            Comp = FindCompFromGUID(ZoneTrack.LeftLink)
                            If Comp Is Nothing Then Return String.Empty
                            For i As Integer = 1 To 40
                                If Comp Is Nothing Then Return String.Empty
                                Select Case Comp.GetType.ToString
                                    Case gTrackComponentName
                                        If Comp.TrackZoneIntegrityMonitoringEnabled Then
                                            If CurrentTrackZoneID = Comp.TrackZoneReference.ZoneUID Then
                                                Comp = FindCompFromGUID(Comp.LeftLink)
                                            Else
                                                TrackedZone = Comp
                                                Return Comp.TrackZoneReference.ZoneUID.ToString
                                            End If
                                        Else
                                            Return String.Empty
                                        End If
                                    Case gSwitchComponentName
                                        Dim sw As Switch
                                        sw = DirectCast(Comp, Switch)
                                        If sw.ControlledReverse AndAlso sw.LeftAltLink > 0 Then
                                            Comp = FindCompFromGUID(sw.LeftAltLink)
                                        Else
                                            Comp = FindCompFromGUID(sw.LeftLink)
                                        End If
                                    Case Else
                                        Comp = FindCompFromGUID(Comp.LeftLink)
                                End Select
                            Next
                        Else
                            Return ZoneTrack.TrackZoneReference.ZoneUID
                        End If
                    Case Else
                        Return String.Empty
                End Select
            End If
            'ZoneTrack.trackz()
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            Comp = Nothing
        End Try
        Return String.Empty
    End Function

    Public Function GetAllContinuouslyOccupiedTrackZones(ByVal TrackZoneToCheck As TrackZone, Optional ByVal SelectedZoneList As ArrayList = Nothing,
                                           Optional ByVal SelectedFoulingZoneList As ArrayList = Nothing) As ArrayList
        Dim LeftZoneCollection As New ArrayList
        Dim RightZoneCollection As New ArrayList
        Dim ReturnZoneCollection As New ArrayList
        Dim TrackedZonetrack As Track = Nothing
        Dim TrackToCheck As Track
        Dim tz As TrackZone = Nothing
        Try

            TrackToCheck = TMDS.SystemUtilities.FetchData.FindFirstTransverableTrackInList(TrackZoneToCheck.TrackList)
            If Not TrackToCheck.TrackZoneIntegrityMonitoringEnabled Then
                ReturnZoneCollection.Add(TrackZoneToCheck)
                Return ReturnZoneCollection
            ElseIf Not TrackToCheck.Occuppied Then
                ReturnZoneCollection.Add(TrackZoneToCheck)
                Return ReturnZoneCollection
            End If

            For i As Integer = 0 To TMDS.GlobalDeclarations.gTrackZones.Count + 1
                If Not TrackToCheck Is Nothing Then
                    GetAdjacentZones(TrackToCheck, "LEFT", TrackedZonetrack)
                    If TrackedZonetrack IsNot Nothing AndAlso TrackedZonetrack.TrackZoneIntegrityMonitoringEnabled Then
                        If TrackedZonetrack.Occuppied Or
                        (Not IsNothing(SelectedZoneList) AndAlso SelectedZoneList.Contains(TrackedZonetrack.TrackZoneReference)) Or
                        (Not IsNothing(SelectedFoulingZoneList) AndAlso SelectedFoulingZoneList.Contains(TrackedZonetrack.TrackZoneReference)) Then
                            LeftZoneCollection.Add(TrackedZonetrack.TrackZoneReference)
                            TrackToCheck = TrackedZonetrack
                            TrackedZonetrack = Nothing
                        End If
                    Else
                        Exit For
                    End If
                End If
            Next
            If LeftZoneCollection IsNot Nothing AndAlso LeftZoneCollection.Count > 0 Then
                LeftZoneCollection.Reverse()
                For Each tz In LeftZoneCollection
                    If Not ReturnZoneCollection.Contains(tz) Then ReturnZoneCollection.Add(tz)
                Next
            End If

            If Not ReturnZoneCollection.Contains(TrackZoneToCheck) Then ReturnZoneCollection.Add(TrackZoneToCheck)

            TrackedZonetrack = Nothing
            TrackToCheck = TMDS.SystemUtilities.FetchData.FindFirstTransverableTrackInList(TrackZoneToCheck.TrackList)

            For i As Integer = 0 To TMDS.GlobalDeclarations.gTrackZones.Count + 1
                If Not TrackToCheck Is Nothing Then
                    GetAdjacentZones(TrackToCheck, "RIGHT", TrackedZonetrack)
                    If TrackedZonetrack IsNot Nothing AndAlso TrackedZonetrack.TrackZoneIntegrityMonitoringEnabled Then
                        If TrackedZonetrack.Occuppied Or
                        (Not IsNothing(SelectedZoneList) AndAlso SelectedZoneList.Contains(TrackedZonetrack.TrackZoneReference)) Or
                        (Not IsNothing(SelectedFoulingZoneList) AndAlso SelectedFoulingZoneList.Contains(TrackedZonetrack.TrackZoneReference)) Then
                            RightZoneCollection.Add(TrackedZonetrack.TrackZoneReference)
                            TrackToCheck = TrackedZonetrack
                            TrackedZonetrack = Nothing
                        End If
                    Else
                        Exit For
                    End If
                End If
            Next
            If RightZoneCollection IsNot Nothing AndAlso RightZoneCollection.Count > 0 Then
                For Each tz In RightZoneCollection
                    If Not ReturnZoneCollection.Contains(tz) Then ReturnZoneCollection.Add(tz)
                Next
            End If
            Return ReturnZoneCollection
        Catch ex As Exception
            GetErrorDetails(ex)
            ReturnZoneCollection.Add(TrackZoneToCheck)
            Return ReturnZoneCollection
        Finally
            LeftZoneCollection = Nothing
            RightZoneCollection = Nothing
            TrackedZonetrack = Nothing
            TrackToCheck = Nothing
            tz = Nothing
        End Try
    End Function

    Public Function GetSPMSSwitchListBehindTrainInAuthority(ByVal AuthorityItem As Object, Optional ByRef ZoneTrainOn As TrackZone = Nothing) As ArrayList
        Dim SwitchBehindMyTrain As New ArrayList
        Dim CompList As New ArrayList
        Dim HasTrainID As Boolean = False
        Try
            If AuthorityItem.GetType.ToString = gSFAComponentName Then
                CompList = AuthorityItem.components
            ElseIf AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority" Then
                CompList = AuthorityItem.CompleteCompSelection
            End If
            If (AuthorityItem.GetType.ToString = gSFAComponentName Or AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority") AndAlso AuthorityItem.issuedtotrain Then
                Dim comp As Object = Nothing
                Dim trn As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(Mid(AuthorityItem.IssueTo, 1, 10))
                If Not trn Is Nothing Then
                    For Each comp In CompList
                        If comp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = comp
                            If trn.myTrack Is trk Then
                                HasTrainID = True
                                Exit For
                            End If
                        ElseIf comp.GetType.ToString = gSwitchComponentName Then
                            Dim sw As Switch = comp
                            If sw.SwitchPointerMonitorMode Then
                                If Not SwitchBehindMyTrain.Contains(sw) Then
                                    SwitchBehindMyTrain.Add(sw)
                                End If
                            End If
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        If HasTrainID Then
            Return SwitchBehindMyTrain
        Else
            Return Nothing
        End If
    End Function

    Public Function GetZonesBehindTrainInAuthority(ByVal AuthorityItem As Object, Optional ByRef ZoneTrainOn As TrackZone = Nothing) As ArrayList
        Dim TrackZoneBehindMyTrain As New ArrayList
        Dim HasTrainID As Boolean = False
        Dim IsSFA As Boolean = False
        Dim CompList As New ArrayList
        Dim TempZoneList As New ArrayList
        Try
            If AuthorityItem.GetType.ToString = gSFAComponentName Then
                CompList = AuthorityItem.components
                IsSFA = True
            ElseIf AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority" Then
                CompList = AuthorityItem.CompleteCompSelection
                IsSFA = True
            End If

            If AuthorityItem.GetType.ToString = gSFAComponentName Or AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority" AndAlso AuthorityItem.issuedtotrain Then
                Dim comp As Object = Nothing
                Dim trn As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(Mid(AuthorityItem.IssueTo, 1, 10))
                If Not trn Is Nothing Then
                    comp = Nothing
                    For Each comp In CompList
                        If comp.GetType.ToString = gTrackComponentName Then
                            Dim trk As Track = comp
                            If trn.myTrack Is trk Then
                                ZoneTrainOn = trn.myTrack.TrackZoneReference
                                HasTrainID = True
                                Exit For
                            End If
                            If trk.TrackZoneIntegrityMonitoringEnabled AndAlso Not trk.TrackZoneReference Is Nothing Then
                                If Not TrackZoneBehindMyTrain.Contains(trk.TrackZoneReference.ZoneName.ToUpper) Then
                                    TrackZoneBehindMyTrain.Add(trk.TrackZoneReference.ZoneName.ToUpper)
                                End If
                            End If
                        End If
                    Next

                    Dim tzn As String
                    Dim tznlst As New ArrayList
                    tznlst = TrackZoneBehindMyTrain.Clone()
                    If ZoneTrainOn IsNot Nothing Then
                        For Each tzn In tznlst
                            If tzn = ZoneTrainOn.ZoneName.ToUpper Then
                                TrackZoneBehindMyTrain.Remove(tzn)
                            End If
                        Next
                    End If

                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        If HasTrainID Then
            Return TrackZoneBehindMyTrain
        Else
            Return Nothing
        End If

    End Function

    Public Function GetZonesBehindTrainInAuthority_New(ByVal AuthorityItem As Object) As ArrayList
        Dim TrackZoneBehindMyTrain As New ArrayList
        Dim HasTrainID As Boolean = False
        Dim IsSFA As Boolean = False
        Dim CompList As New ArrayList
        Dim TempZoneList As New ArrayList
        Dim TmpZoneListOccupancyCollection As New ArrayList
        Try
            If AuthorityItem.GetType.ToString = gSFAComponentName Then
                CompList = AuthorityItem.components
                IsSFA = True
            ElseIf AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority" Then
                CompList = AuthorityItem.CompleteCompSelection
                IsSFA = True
            End If

            If AuthorityItem.GetType.ToString = gSFAComponentName Or AuthorityItem.GetType.ToString = "TMDS.frmSingleFormAuthority" AndAlso AuthorityItem.issuedtotrain Then
                Dim comp As Object = Nothing
                Dim trn As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(Mid(AuthorityItem.IssueTo, 1, 10))
                If Not trn Is Nothing Then
                    For Each comp In CompList
                        If comp.GetType.ToString = gTrackComponentName Then
                            If comp.TrackZoneReference IsNot Nothing AndAlso comp.TrackZoneIntegrityMonitoringEnabled Then
                                AddTIWSZoneStatus(TempZoneList, comp, AuthorityItem)
                            End If
                        End If
                    Next
                    Dim tmpTIWSZS As TIWSZoneStatus
                    Dim tmpTIWSTrainZS As TIWSZoneStatus
                    Dim tmpTIWSTrainZIndex As Integer
                    Dim Index As Integer = 0
                    Dim TrainOnZoneName As String = ""
                    For Each tmpTIWSZS In TempZoneList
                        If tmpTIWSZS.ZoneHasTrain Then
                            tmpTIWSTrainZS = tmpTIWSZS
                            tmpTIWSTrainZIndex = Index
                            TrainOnZoneName = tmpTIWSZS.ZoneName
                            HasTrainID = True
                            Exit For
                        End If
                        Index = Index + 1
                    Next
                    Dim tznlst As New ArrayList
                    For i As Integer = 0 To tmpTIWSTrainZIndex
                        Dim tmpZC As TIWSZoneStatus = Nothing
                        tmpZC = TryCast(TempZoneList(i), TIWSZoneStatus)
                        If tmpZC IsNot Nothing AndAlso tmpZC.OccupancyStatus Then
                            If Not tznlst.Contains(tmpZC) Then tznlst.Add(tmpZC)
                        End If
                    Next
                    tznlst.Reverse()
                    'TmpZoneListOccupancyCollection
                    For Each tmpTIWSZS In tznlst
                        If tmpTIWSZS.OccupancyStatus Then
                            If Not TmpZoneListOccupancyCollection.Contains(tmpTIWSZS) Then TmpZoneListOccupancyCollection.Add(tmpTIWSZS)
                        Else
                            Exit For
                        End If
                    Next
                    For Each tmpTIWSZS In TmpZoneListOccupancyCollection
                        If Not TrackZoneBehindMyTrain.Contains(tmpTIWSZS.ZoneName.ToUpper.Trim) Then
                            TrackZoneBehindMyTrain.Add(tmpTIWSZS.ZoneName.ToUpper.Trim)
                        End If
                    Next
                    If Not String.IsNullOrEmpty(TrainOnZoneName) AndAlso TrainOnZoneName.Trim <> "" Then
                        TrackZoneBehindMyTrain.Remove(TrainOnZoneName)
                    End If

                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        If HasTrainID Then
            Return TrackZoneBehindMyTrain
        Else
            Return Nothing
        End If

    End Function

    Friend Sub PopulateAuthZones(ByRef auth As Object, Optional ForcePopulate As Boolean = False)
        Dim Comp As Track = Nothing
        Try
            If auth Is Nothing Then Exit Sub
            If auth.GetType.ToString = gSFAComponentName AndAlso auth.issuedtotrain AndAlso auth.AuthType1 = "ItemTwoToTrain" Then
                Dim sfa As TMDS.TrackAuthority.SFAuthority = Nothing
                Dim Populate As Boolean = False
                If auth.tmpMyTrackZoneList Is Nothing Then
                    Populate = True
                ElseIf auth.tmpMyTrackZoneList.Count = 0 Then
                    Populate = True
                End If
                If ForcePopulate Then Populate = True

                For Each Comp In sfa.Components
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If auth.tmpMyTrackZoneList.Contains(Comp.TrackZoneReference) Then auth.tmpMyTrackZoneList.Add(Comp.TrackZoneReference)
                    End If
                Next

            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Function GetZonesBehindTrainInAuthority_New2(ByVal AuthorityItem As Object) As ArrayList
        Dim TrackZoneBehindMyTrain As New ArrayList
        Dim HasTrainID As Boolean = False
        Dim IsSFA As Boolean = False
        Dim CompList As New ArrayList
        Dim TempZoneList As New ArrayList
        Try
            If AuthorityItem.GetType.ToString = gSFAComponentName AndAlso AuthorityItem.issuedtotrain AndAlso AuthorityItem.AuthType1 = "ItemTwoToTrain" Then
                'Dim sfa As TMDS.TrackAuthority.SFAuthority
                If AuthorityItem.LastTrainZone Is Nothing Then Return New ArrayList
                If AuthorityItem.tmpMyTrackZoneList Is Nothing Then AuthorityItem.PopulateAuthZones()
                If AuthorityItem.tmpMyTrackZoneList.Count = 0 Then AuthorityItem.PopulateAuthZones()
                Dim tzn As TrackZone = Nothing
                For Each tzn In AuthorityItem.tmpMyTrackZoneList
                    If tzn.ZoneName = AuthorityItem.LastTrainZone.ZoneName Then
                        Exit For
                    Else
                        If Not TrackZoneBehindMyTrain.Contains(tzn) Then TrackZoneBehindMyTrain.Add(tzn)
                    End If
                Next
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return TrackZoneBehindMyTrain
    End Function

    Public Function PreviousZoneHaveAuthority(ByVal trk As Track) As Boolean
        Try
            If trk.TrackZoneIntegrityMonitoringEnabled Then
                Dim NextComp As Object = trk
                Dim trkcnt As Integer
                For trkcnt = 0 To 50
                    If IsNothing(NextComp) Then
                        Return False
                    End If
                    If trk.TrafficDirection = "RIGHT" Then
                        NextComp = FindCompFromGUID(NextComp.LeftLink)
                    ElseIf trk.TrafficDirection = "LEFT" Then
                        NextComp = FindCompFromGUID(NextComp.RightLink)
                    End If
                    If Not IsNothing(NextComp) AndAlso NextComp.GetType.ToString = gTrackComponentName Then
                        Dim tcomp As Track = DirectCast(NextComp, Track)
                        If tcomp.TrackZoneIntegrityMonitoringEnabled AndAlso
                            trk.TrackZoneReference.ZoneName.Trim.ToUpper <> tcomp.TrackZoneReference.ZoneName.Trim.ToUpper Then
                            If tcomp.TrackAuthorities.Count > 0 Then
                                Return True
                            Else
                                Return False
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return False
    End Function

    Public Function CheckBehindForAuthContinuity(ByVal trk As Track, ByVal trkAuth As TMDS.TrackAuthority.SFAuthority) As Boolean
        Try
            '' this will look behind the zone that was indicated occupied to see if the other zones previous to it are occupied and have authority issues to same train 
            '' and are sequential from the zone that the train is sitting on.
            If trk.TrackZoneIntegrityMonitoringEnabled Then
                Dim trkcnt As Integer
                Dim NextComp As Object = trk
                Dim trn As TrainID = TMDS.SystemUtilities.FetchData.FindTrainFromIssueToAuthority(Mid(trkAuth.IssueTo, 1, 10))
                For trkcnt = 0 To 50
                    If IsNothing(NextComp) Then
                        Return False
                    End If
                    If trk.TrafficDirection = "RIGHT" Then
                        NextComp = FindCompFromGUID(NextComp.LeftLink)
                    ElseIf trk.TrafficDirection = "LEFT" Then
                        NextComp = FindCompFromGUID(NextComp.RightLink)
                    End If
                    If Not IsNothing(NextComp) AndAlso NextComp.GetType.ToString = gTrackComponentName Then
                        Dim tcomp As Track = DirectCast(NextComp, Track)
                        If tcomp.TrackZoneIntegrityMonitoringEnabled AndAlso trk.TrackZoneReference.ZoneName.Trim.ToUpper <> NextComp.TrackZoneReference.ZoneName.trim.toupper _
                        AndAlso tcomp.TrackAuthorities.Count > 0 Then
                            If tcomp.Occuppied Then
                                Dim auth As TMDS.TrackAuthority.SFAuthority
                                For Each auth In tcomp.TrackAuthorities
                                    If Mid(auth.IssueTo, 1, 10) = Mid(trkAuth.IssueTo, 1, 10) AndAlso auth.AuthorityUID <> trkAuth.AuthorityUID Then
                                        Return True
                                    End If
                                Next
                            Else
                                Dim auth As TMDS.TrackAuthority.SFAuthority
                                For Each auth In tcomp.TrackAuthorities
                                    If Mid(auth.IssueTo, 1, 10) = Mid(trkAuth.IssueTo, 1, 10) Then
                                        If trn.TrackGUID = tcomp.GUID Then
                                            Return True
                                        ElseIf auth.AuthorityUID <> trkAuth.AuthorityUID AndAlso tcomp.TrackAuthorities.Count = 1 Then
                                            Return True
                                        Else
                                            Return False
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End If
                Next
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return False
    End Function

    Public Function IsFoulingTrackInZoneOccupied(ByVal TrkZn As TrackZone, ByVal SelectedFoulingZoneList As ArrayList, ByRef fltrk As Track) As Boolean
        Try
            Dim zonefound As Boolean = False
            Dim comp, OSComp As Object
            For Each comp In TrkZn.TrackList
                If comp.OsComponentCollection IsNot Nothing AndAlso comp.OsComponentCollection.Count > 0 Then
                    For Each OSComp In comp.OsComponentCollection
                        If OSComp.GetType.ToString = gTrackComponentName AndAlso OSComp.FoulingTrackSPMS AndAlso OSComp.Occuppied Then
                            If Not IsNothing(SelectedFoulingZoneList) AndAlso SelectedFoulingZoneList.Count > 0 AndAlso SelectedFoulingZoneList.Contains(OSComp.TrackZoneReference) Then
                            Else
                                fltrk = OSComp
                                zonefound = True
                            End If
                        End If
                    Next
                End If
            Next
            Return zonefound
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return False
    End Function

    Public Function GetSwitchFoulingZone(sw As Switch) As TrackZone
        Dim AltTrack As Track = Nothing
        Try
            If sw.RightAltLink > 0 Then AltTrack = FindCompFromGUID(sw.RightAltLink)
            If sw.LeftAltLink > 0 Then AltTrack = FindCompFromGUID(sw.LeftAltLink)
            If AltTrack IsNot Nothing AndAlso AltTrack.TrackZoneIntegrityMonitoringEnabled Then
                Return AltTrack.TrackZoneReference
            End If
        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function

    Public Function GetSwitchParentZone(sw As Switch) As TrackZone
        Dim RightTrack As Track = Nothing
        Dim LeftTrack As Track = Nothing
        Dim AltTrack As Track = Nothing
        Try
            If sw.RightLink > 0 Then RightTrack = FindCompFromGUID(sw.RightLink)
            If sw.LeftLink > 0 Then LeftTrack = FindCompFromGUID(sw.LeftLink)
            If sw.RightAltLink > 0 Then AltTrack = FindCompFromGUID(sw.RightAltLink)
            If sw.LeftAltLink > 0 Then AltTrack = FindCompFromGUID(sw.LeftAltLink)

            If RightTrack IsNot Nothing AndAlso RightTrack.TrackZoneIntegrityMonitoringEnabled Then
                Return RightTrack.TrackZoneReference
            End If
            If LeftTrack IsNot Nothing AndAlso LeftTrack.TrackZoneIntegrityMonitoringEnabled Then
                Return LeftTrack.TrackZoneReference
            End If

        Catch ex As Exception
            TMDS.SystemUtilities.Utilities.SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function

    Public Function GetFoulingTrkParentZone(ByVal FoulingTrack As Track, Optional ByRef ConnectingTrack As Track = Nothing,
                                              Optional ByRef ConnectingSwSPMS As Switch = Nothing, Optional ByRef FoulingTrackSwtich As Switch = Nothing) As TrackZone

        Dim Tz As TrackZone = Nothing
        Dim Comp As Object = Nothing
        Dim RightTrack As Track = Nothing
        Dim LeftTrack As Track = Nothing
        Dim AltTrack As Track = Nothing
        Dim RightTrackZoneName As String = ""
        Dim LeftTrackZoneName As String = ""
        Dim AltTrackZoneName As String = ""
        Dim OSSW As Switch = Nothing
        Try
            If FoulingTrack Is Nothing Then Return Nothing
            If FoulingTrack.TrackZoneIntegrityMonitoringEnabled AndAlso FoulingTrack.Occuppied AndAlso FoulingTrack.FoulingTrackSPMS _
            AndAlso FoulingTrack.OsComponentCollection IsNot Nothing AndAlso FoulingTrack.OsComponentCollection.Count > 0 Then

                Dim tmpSwitchList As New ArrayList
                tmpSwitchList = TMDS.SystemUtilities.FetchData.FindSwitchesFromOSList(FoulingTrack)
                If tmpSwitchList IsNot Nothing AndAlso tmpSwitchList.Count > 1 Then
                    Dim SW As Switch
                    For Each SW In tmpSwitchList
                        If SW IsNot Nothing Then
                            Dim SwIdentified As Boolean = False
                            If SW.LeftLink = FoulingTrack.GUID Then
                                SwIdentified = True
                            ElseIf SW.LeftAltLink = FoulingTrack.GUID Then
                                SwIdentified = True
                            ElseIf SW.RightLink = FoulingTrack.GUID Then
                                SwIdentified = True
                            ElseIf SW.RightAltLink = FoulingTrack.GUID Then
                                SwIdentified = True
                            End If
                            If SwIdentified Then
                                FoulingTrackSwtich = SW
                                OSSW = SW
                                Exit For
                            End If
                        End If
                    Next
                End If
                If OSSW Is Nothing Then
                    OSSW = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(FoulingTrack)
                End If


                If Not OSSW Is Nothing Then
                    ConnectingSwSPMS = OSSW
                    If OSSW.RightLink > 0 Then RightTrack = FindCompFromGUID(OSSW.RightLink)
                    If OSSW.LeftLink > 0 Then LeftTrack = FindCompFromGUID(OSSW.LeftLink)
                    If OSSW.RightAltLink > 0 Then AltTrack = FindCompFromGUID(OSSW.RightAltLink)
                    If OSSW.LeftAltLink > 0 Then AltTrack = FindCompFromGUID(OSSW.LeftAltLink)
                    If RightTrack IsNot Nothing Then
                        ConnectingTrack = RightTrack
                        Return RightTrack.TrackZoneReference
                    End If
                    If LeftTrack IsNot Nothing Then
                        ConnectingTrack = LeftTrack
                        Return LeftTrack.TrackZoneReference
                    End If
                End If
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            RightTrack = Nothing
            LeftTrack = Nothing
            AltTrack = Nothing
            OSSW = Nothing
            Comp = Nothing
        End Try
        Return Nothing
    End Function

    Public Function GetFoulingZoneRightTrack(ByVal FoulingZone As TrackZone) As Track
        Dim Trk As Track = Nothing
        Try
            Dim connsw As Switch = Nothing
            If FoulingZone.TrackList(0).leftlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).leftlink)
            End If
            If IsNothing(connsw) AndAlso FoulingZone.TrackList(0).rightlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).rightlink)
            End If
            If Not IsNothing(connsw) Then
                Dim RightTrack As Object = Nothing
                If connsw.RightLink > 0 Then RightTrack = FindCompFromGUID(connsw.RightLink)
                If RightTrack IsNot Nothing AndAlso RightTrack.GetType.ToString = gTrackComponentName AndAlso RightTrack.TrackZoneIntegrityMonitoringEnabled Then
                    Trk = RightTrack
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return Trk
    End Function

    Public Function GetFoulingZoneLeftTrack(ByVal FoulingZone As TrackZone) As Track
        Dim Trk As Track = Nothing
        Try
            Dim connsw As Switch = Nothing
            If FoulingZone.TrackList(0).leftlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).leftlink)
            End If
            If IsNothing(connsw) AndAlso FoulingZone.TrackList(0).rightlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).rightlink)
            End If
            If Not IsNothing(connsw) Then
                Dim LeftTrack As Object = Nothing
                If connsw.LeftLink > 0 Then LeftTrack = FindCompFromGUID(connsw.LeftLink)
                If LeftTrack IsNot Nothing AndAlso LeftTrack.GetType.ToString = gTrackComponentName AndAlso LeftTrack.TrackZoneIntegrityMonitoringEnabled Then
                    Trk = LeftTrack
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return Trk
    End Function

    Public Function GetFoulingZoneConnectedSwitch(ByVal FoulingZone As TrackZone) As Switch
        Dim connsw As Switch = Nothing
        Try
            If FoulingZone.TrackList(0).leftlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).leftlink)
            End If
            If IsNothing(connsw) AndAlso FoulingZone.TrackList(0).rightlink > 0 Then
                connsw = FindCompFromGUID(FoulingZone.TrackList(0).rightlink)
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return connsw
    End Function

    Public Function GetFoulingZoneConnectedSwitches(ByVal FoulingZone As TrackZone) As ArrayList
        Dim connsw As Switch = Nothing
        Dim SwitchList As New ArrayList
        Dim FoulingTrack As Track = Nothing
        Try
            If FoulingZone.TrackList(0) IsNot Nothing Then
                FoulingTrack = FoulingZone.TrackList(0)
            End If
            If FoulingTrack IsNot Nothing Then
                SwitchList = TMDS.SystemUtilities.FetchData.FindSwitchesFromOSList(FoulingTrack)
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return SwitchList
    End Function

    Public Function GetFoulingZoneConnectedTracks(ByVal FoulingZone As TrackZone, Optional FoulingTrackSwitch As Switch = Nothing) As ArrayList
        Dim CompList As New ArrayList
        Try
            Dim connsw As Switch = Nothing
            Dim comp As Object = Nothing
            If FoulingZone.TrackList(0).leftlink > 0 Then
                comp = FindCompFromGUID(FoulingZone.TrackList(0).leftlink)
                If comp.GetType.ToString = gSwitchComponentName Then
                    connsw = comp
                End If
            End If
            If IsNothing(connsw) AndAlso FoulingZone.TrackList(0).rightlink > 0 Then
                comp = FindCompFromGUID(FoulingZone.TrackList(0).rightlink)
                If comp.GetType.ToString = gSwitchComponentName Then
                    connsw = comp
                End If
            End If
            If FoulingTrackSwitch IsNot Nothing Then connsw = FoulingTrackSwitch

            'added Based on BNSF-341 [BNSF-341 with 690.59.02-Shorten with only 1 fouling down]
            If connsw.OsComponentCollection IsNot Nothing AndAlso connsw.OsComponentCollection.Count > 0 Then
                CopyArrayCollection(CompList, connsw.OsComponentCollection)
            End If
            'Commented Based on BNSF-341 [BNSF-341 with 690.59.02-Shorten with only 1 fouling down]
            'If Not IsNothing(connsw) Then
            '    CompList.Add(connsw)
            '    Dim RightTrack As Object = Nothing
            '    Dim LeftTrack As Object = Nothing
            '    If connsw.RightLink > 0 Then RightTrack = FindCompFromGUID(connsw.RightLink)
            '    If connsw.LeftLink > 0 Then LeftTrack = FindCompFromGUID(connsw.LeftLink)
            '    If RightTrack IsNot Nothing AndAlso RightTrack.GetType.ToString = gTrackComponentName AndAlso RightTrack.TrackZoneIntegrityMonitoringEnabled Then
            '        CompList.Add(RightTrack)
            '    End If
            '    If LeftTrack IsNot Nothing AndAlso LeftTrack.GetType.ToString = gTrackComponentName AndAlso LeftTrack.TrackZoneIntegrityMonitoringEnabled Then
            '        CompList.Add(LeftTrack)
            '    End If
            If CompList.Count > 0 Then
                Return CompList
            End If
            'End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return Nothing
    End Function


#End Region

#Region "PTC"

    Public Function GetVETMSMilePostDirection(ByVal FirstTrack As Track, ByVal AuthDirection As String) As String
        Dim MilepostDirection As String = String.Empty
        Try

            Select Case AuthDirection.ToUpper.Trim
                Case "RIGHT"
                    If FirstTrack.LeftLimitMilePost > FirstTrack.RightLimitMilePost Then
                        MilepostDirection = "D"
                    Else
                        MilepostDirection = "A"
                    End If
                Case Else
                    If FirstTrack.LeftLimitMilePost > FirstTrack.RightLimitMilePost Then
                        MilepostDirection = "A"
                    Else
                        MilepostDirection = "D"
                    End If
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return MilepostDirection
    End Function


    Public Sub ProcessNAcknowledgeCutOutTrainAlerts(ByVal TrnSymbol As String, ByVal LocoID As String)
        Try
            Dim RemoveList As New ArrayList
            Dim al As Alert
            Dim AlToRemove As Alert = Nothing
            Dim Symbol As String = String.Empty
            Dim data() As String
            Dim trn As TrainID = Nothing
            If optEnablePTCBOSAPIMode Then
                SyncLock TMDS.GlobalDeclarations.gSystemAlertList
                    For Each al In TMDS.GlobalDeclarations.gSystemAlertList
                        Select Case al.TypeOfAlert
                            Case ManagementClasses.Alert.AlertType.PTC7004Authority
                                data = al.AlertData.Split("|")
                                'Dim AuthLoco As MobileTracking.Locomotive = Nothing
                                'Dim auth As Object = Nothing
                                'Dim authuid As String = data(8)
                                'authuid = Mid(authuid, 1, 3) & " - " & CInt(Mid(authuid, 4, 3))
                                'Select Case data(1)
                                '    Case "TWC"
                                '        auth = SystemUtilities.FetchData.FindAuthority(authuid)
                                '        auth = CType(auth, TrackAuthority.TrackAuthority)
                                '        If Not IsNothing(auth) Then
                                '            trn = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(Mid(auth.IssueTo, 1, 10))
                                '            AuthLoco = auth.FindLocomotive
                                '        End If
                                '    Case "SFA"
                                '        auth = SystemUtilities.FetchData.FindSFA(authuid)
                                '        auth = CType(auth, TrackAuthority.SFAuthority)
                                '        If Not IsNothing(auth) Then
                                '            If auth.IssuedToTrain Then
                                '                trn = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(Mid(auth.IssueTo, 1, 10))
                                '                AuthLoco = auth.FindLocomotive
                                '            End If
                                '        End If
                                '    Case "SPL"
                                '        auth = SystemUtilities.FetchData.FindSpecialAuthority(authuid, data(10))
                                '        auth = CType(auth, TrackAuthority.OtherSpecialAuthorityType)
                                '        If Not IsNothing(auth) Then
                                '            trn = TMDS.SystemUtilities.FetchData.FindTrainFromSymbol(auth.TrainSymbol)
                                '        End If
                                'End Select
                                'If Not IsNothing(trn) Then
                                '    Symbol = trn.Name
                                'End If
                                Symbol = data(10)
                                If Symbol.Trim = TrnSymbol.Trim Then
                                    AlToRemove = al
                                End If
                            Case ManagementClasses.Alert.AlertType.PTC7004FormAB
                                data = al.AlertData.Split("|")
                                Symbol = data(4)
                                If Symbol = TrnSymbol Then
                                    AlToRemove = al
                                End If
                            Case ManagementClasses.Alert.AlertType.PTC7004Tag
                                data = al.AlertData.Split("|")
                                Symbol = data(4)
                                If Symbol = TrnSymbol Then
                                    AlToRemove = al
                                End If
                        End Select
                        If Not IsNothing(AlToRemove) Then
                            Dim SummItem As New SummaryItem("ALERT", "ACKNOWLEDGE", al)
                            SyncLock TMDS.GlobalDeclarations.gSummaryAlertQueue.SyncRoot
                                TMDS.GlobalDeclarations.gSummaryAlertQueue.Enqueue(SummItem)
                            End SyncLock
                            gSystemAlertExists = False
                            RemoveList.Add(al)
                        End If
                    Next

                    For Each al In RemoveList
                        TMDS.GlobalDeclarations.gSystemAlertList.Remove(al)
                    Next
                End SyncLock
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

#End Region

#Region "COMPONENT SORT CLASS"

    Public Class clsCompSort
        Public myUID As Integer
        Private pMilepost As Double
        Public myType As String = String.Empty
        Public Prefix As String = String.Empty
        Public Suffix As String = String.Empty
        Public Property MilePost() As Double
            Get
                Return pMilepost
            End Get
            Set(ByVal value As Double)
                pMilepost = value
            End Set
        End Property


    End Class
#End Region

#Region " MIXED MODE GLOBAL FUNCTIONS "

    Public Sub MoveTrain_MOWOnRollup(ByVal CompList As ArrayList, ByVal IssuedToTrain As Boolean, ByVal IssueToName As String, ByVal TrackSelected As Track)
        Dim Comp As Object
        Try
            Dim trk As Track
            If IssuedToTrain Then
                For Each Comp In CompList
                    If Comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        trk = Comp
                        If trk.TrainIDs.Count > 0 Then
                            Dim movetrn As TrainID
                            movetrn = trk.TrainIDs(0)
                            If movetrn.Name = IssueToName Then
                                TrackSelected.MoveTrainIDPlace(movetrn, trk, True, TMDS.GlobalDeclarations.gDatabaseConnectionTrain)
                                Exit For
                            End If
                        End If

                    End If
                Next
            Else
                For Each Comp In CompList
                    If Comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        trk = Comp
                        If trk.MOWIDs.Count > 0 Then
                            Dim MOW As MOWID
                            MOW = trk.MOWIDs.Item(0)
                            If MOW.Name = IssueToName Then
                                TrackSelected.MoveMOWIDPlace(MOW, trk, False)
                            End If
                        End If
                    End If
                Next
            End If
            trk = Nothing
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region " TRAIN COMMENTS CLASS "
    Friend Class clsTrainComments
        Public myGUID As String = ""
        Public IsOnRelease As Boolean
        Public IdentifingPointName As String = String.Empty
        Public IdentifingPointUID As String = String.Empty
        Public RRUID As String = String.Empty
        Public TrainUnit As String = String.Empty
        Public TypeOfComment As String = String.Empty
        Public Comments As String = String.Empty
        Public Status As String = String.Empty
        Public PreviousDispatcher As String = String.Empty

        Public CreateTime As Date
        Private pmyTrain As TrainID = Nothing
        Private pDataItems As String = ""
        'gPreviousDispatcher

        Public Sub New()
            myGUID = Guid.NewGuid.ToString.ToUpper
            CreateTime = Now
        End Sub

        Public Property myTrain() As TrainID
            Get
                Return pmyTrain
            End Get
            Set(ByVal Value As TrainID)
                pmyTrain = Value
                If pmyTrain IsNot Nothing Then UpdateIdentifingPointDetails()
            End Set
        End Property

        Public Property DataItems() As String
            Get
                Return pDataItems
            End Get
            Set(ByVal value As String)
                pDataItems = value
            End Set
        End Property

        Private Sub UpdateIdentifingPointDetails()
            Try
                If optEnableTrainFunctionalityFXE Then
                    If myTrain.Direction = "LEFT" Then
                        IdentifingPointName = myTrain.myTrack.RightLimitName
                    Else
                        IdentifingPointName = myTrain.myTrack.LeftLimitName
                    End If
                    IdentifingPointUID = myTrain.myTrack.myControlPoint.GUID
                    RRUID = myTrain.myTrack.myControlPoint.UniqueName
                Else
                    IdentifingPointName = myTrain.myTrack.myControlPoint.Name
                    IdentifingPointUID = myTrain.myTrack.myControlPoint.GUID
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try


        End Sub

        Friend Function GetAddCommentToDatabaseSQL() As String
            Dim q As String = "'"
            Dim s As String = ","
            If myTrain Is Nothing Then Return Nothing
            Try
                Dim Insert As New StringBuilder
                Insert.Append("INSERT INTO tblTrainEventActiveComments")
                Insert.Append(" (")
                Insert.Append(" CommentUID, TrainSheetUID, EventLocationUID, EventLocation, EventType, EventTimeStamp,")
                Insert.Append(" EventTimeZone, EventComments,RailroadUID, EntityUID,ReportData,EventCreator")
                Insert.Append(" )")
                Insert.Append(" VALUES (")
                Insert.Append(q & Guid.NewGuid.ToString.ToUpper & q & s)
                Insert.Append(q & myTrain.ActiveTrainSheetGUID & q & s)
                Insert.Append(q & IdentifingPointUID & q & s)
                Insert.Append(q & IdentifingPointName & q & s)
                Insert.Append(q & Me.TypeOfComment & q & s)
                Insert.Append(q & Now.ToString & q & s)
                Insert.Append(q & Mid(myTrain.myTrack.TimeZone, 1, 1) & q & s)
                Insert.Append(q & Me.Comments & q & s)
                Insert.Append(q & RRUID & q & s)

                Select Case TypeOfComment
                    Case "UNIT"
                        Insert.Append(q & TrainUnit & q & s)
                    Case "UNITFAIL"
                        Insert.Append(q & TrainUnit & q & s)
                    Case Else
                        Insert.Append(q & myTrain.Name & q & s)
                End Select

                Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & q)

                Select Case TypeOfComment
                    Case "UNITFAIL"
                        Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & "|FailCode-" & Status & q)
                    Case Else
                        Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & q)
                End Select

                Insert.Append(s & q & gPreviousDispatcher & q)
                Insert.Append(" )")
                Return Insert.ToString

                '    SystemDatabaseCommandToDynamicTrain("AddCommentToDatabase", Insert.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return String.Empty
        End Function

        Friend Sub AddCommentToDatabase()
            Dim q As String = "'"
            Dim s As String = ","
            If myTrain Is Nothing Then Exit Sub
            Try
                Dim Insert As New StringBuilder
                Insert.Append("INSERT INTO tblTrainEventActiveComments")
                Insert.Append(" (")
                Insert.Append(" CommentUID, TrainSheetUID, EventLocationUID, EventLocation, EventType, EventTimeStamp,")
                Insert.Append(" EventTimeZone, EventComments,RailroadUID, EntityUID,ReportData,EventCreator")
                Insert.Append(" )")
                Insert.Append(" VALUES (")
                Insert.Append(q & Guid.NewGuid.ToString.ToUpper & q & s)
                Insert.Append(q & myTrain.ActiveTrainSheetGUID & q & s)
                Insert.Append(q & IdentifingPointUID & q & s)
                Insert.Append(q & IdentifingPointName & q & s)
                Insert.Append(q & Me.TypeOfComment & q & s)
                Insert.Append(q & Now.ToString & q & s)
                Insert.Append(q & Mid(myTrain.myTrack.TimeZone, 1, 1) & q & s)
                Insert.Append(q & Me.Comments & q & s)
                Insert.Append(q & RRUID & q & s)

                Select Case TypeOfComment
                    Case "UNIT"
                        Insert.Append(q & TrainUnit & q & s)
                    Case "UNITFAIL"
                        Insert.Append(q & TrainUnit & q & s)
                    Case Else
                        Insert.Append(q & myTrain.Name & q & s)
                End Select

                Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & q)

                Select Case TypeOfComment
                    Case "UNITFAIL"
                        Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & "|FailCode-" & Status & q)
                    Case Else
                        Insert.Append(q & myTrain.GetTrainReportDataString("NONE", 0) & q)
                End Select

                Insert.Append(s & q & gPreviousDispatcher & q)
                Insert.Append(" )")
                SystemDatabaseCommandToDynamicTrain("AddCommentToDatabase", Insert.ToString)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

    End Class

#End Region

#Region " JOINT CONTROL SECTION "

    Friend Enum JointControlLockOptions
        ADD
        RELEASE
    End Enum

    Friend Function IsValidJointControlComponents(ByVal CompCollection As ArrayList) As Boolean
        If Not optEnableTerritoryDeskAssignmentMode Then Return True
        Dim Comp As Object = Nothing
        Try
            'TRK.InUse=False
            Dim trk As Track = Nothing
            'trk.JointControl
            If CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                For Each Comp In CompCollection
                    If Comp IsNot Nothing Then
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                If Comp.JointControl AndAlso Mid(Comp.AuthorityType.ToUpper.Trim, 1, 3) = "TWC" AndAlso Not gInUseComponentCollection.Contains(Comp) AndAlso Comp.InUse Then Return False
                        End Select
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Friend Function IsValidJointControlComponents(ByVal Comp As Object) As Boolean
        If Not optEnableTerritoryDeskAssignmentMode Then Return True

        Try
            Dim trk As Track = Nothing
            If Comp IsNot Nothing Then
                Select Case Comp.GetType.ToString
                    Case gTrackComponentName
                        If Comp.JointControl AndAlso Mid(Comp.AuthorityType.ToUpper.Trim, 1, 3) = "TWC" AndAlso Not gInUseComponentCollection.Contains(Comp) AndAlso Comp.InUse Then Return False
                End Select
            End If

            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Friend Function LockJointControlComponent(ByVal CompCollection As ArrayList) As Boolean
        If Not optEnableTerritoryDeskAssignmentMode Then Return True
        Dim Comp As Object = Nothing
        Try
            If CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                For Each Comp In CompCollection
                    If Comp IsNot Nothing Then
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                Dim TRK As Track = Nothing
                                TRK = TryCast(Comp, Track)
                                If TRK.JointControl AndAlso Mid(TRK.AuthorityType.ToUpper.Trim, 1, 3) = "TWC" AndAlso Not gInUseComponentCollection.Contains(TRK) Then
                                    PublishCompinUseStatus(TRK, JointControlLockOptions.ADD)
                                    gInUseComponentCollection.Add(TRK)
                                End If
                                'If Comp.JointControl AndAlso Comp.InUse Then Return False
                        End Select
                    End If
                Next
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function
    Friend Function LockJointControlComponent(ByVal Comp As Object) As Boolean
        If Not optEnableTerritoryDeskAssignmentMode Then Return True
        Try

            If Comp IsNot Nothing Then
                Select Case Comp.GetType.ToString
                    Case gTrackComponentName
                        Dim TRK As Track = Nothing
                        TRK = TryCast(Comp, Track)
                        If TRK.JointControl AndAlso Mid(TRK.AuthorityType.ToUpper.Trim, 1, 3) = "TWC" AndAlso Not gInUseComponentCollection.Contains(TRK) Then
                            PublishCompinUseStatus(TRK, JointControlLockOptions.ADD)
                            gInUseComponentCollection.Add(TRK)
                        End If
                        'If Comp.JointControl AndAlso Comp.InUse Then Return False
                End Select
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function


    Friend Sub PublishCompinUseStatus(Comp As Object, EventType As JointControlLockOptions)
        Try
            If Not optEnableTerritoryDeskAssignmentMode Then Exit Sub
            'If Not gInUseComponentCollection.Contains(Comp) Then
            Dim TRK As Track = Nothing
            TRK = TryCast(Comp, Track)
            If TRK IsNot Nothing Then
                'gInUseComponentCollection.Add(Comp)
                Dim MessageNew As String = "DC" & Chr(6) & "LOCK;"
                MessageNew = MessageNew & "|" & TRK.myTerritory.TerritoryID.ToString
                MessageNew = MessageNew & "|" & EventType.ToString
                MessageNew = MessageNew & "|" & TRK.GUID
                MessageNew = TRK.myTerritory.TerritoryID.ToString & Chr(12) & MessageNew
                SyncLock TMDS.GlobalDeclarations.gControlServerXmtQueue
                    TMDS.GlobalDeclarations.gControlServerXmtQueue.Enqueue(MessageNew)
                End SyncLock
            End If
            'End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

    End Sub

    Private Sub ReleaseJointControlComponents()
        Try
            Dim Comp As Object
            For Each Comp In gInUseComponentCollection
                PublishCompinUseStatus(Comp, JointControlLockOptions.RELEASE)
            Next
            gInUseComponentCollection.Clear()
            gInUseComponentCollection = ArrayList.Synchronized(New ArrayList())
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

#End Region

#Region " SEND CONTROL "
    Friend Sub SendControl_FullUpdate(ByVal TrackSelection As ArrayList)
        Try

            Dim CPLIST As New ArrayList()
            Dim cp As ControlPoint
            Dim comp As Object
            For Each comp In TrackSelection
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    Dim trk As Track = CType(comp, Track)
                    Dim leftSig As Signal = TMDS.SystemUtilities.FetchData.FindSignalLeftFromGUID(trk.LeftLink)
                    Dim rightSig As Signal = TMDS.SystemUtilities.FetchData.FindSignalRightFromGUID(trk.RightLink)

                    If Not CPLIST.Contains(trk.myControlPoint) Then
                        CPLIST.Add(trk.myControlPoint)
                    End If

                    If Not IsNothing(leftSig) Then
                        If Not CPLIST.Contains(leftSig.myControlPoint) Then
                            CPLIST.Add(leftSig.myControlPoint)
                        End If
                    End If
                    If Not IsNothing(rightSig) Then
                        If Not CPLIST.Contains(rightSig.myControlPoint) Then
                            CPLIST.Add(rightSig.myControlPoint)
                        End If
                    End If
                End If
            Next

            For Each cp In CPLIST
                cp.FindCodeStationToSend(cp)
                cp.SendCodeStationControlSpecial(True)
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub
#End Region

    Friend Sub RecursiveTraversal(ByRef comp As Object, ByRef OrderedtrackCollection As ArrayList, ByVal EndGuid As Integer, ByVal direction As String, ByRef gTrackSelectionCollection As ArrayList)
        Try
            ' Should stop at Yard Authority. Raj needs to check 
            If (Mid(comp.AuthorityType, 1, 3) = "YL") Then
                Exit Sub
            End If


            If (TypeOf (comp) Is Track) Then
                'OrderedtrackCollection.Add(comp)
                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, comp)
                Dim presenttrack As Track = TryCast(comp, Track)

                If (presenttrack.GUID = EndGuid) Then
                    'end of recursion and will backtrack
                Else
                    'carry on furthur with recursion
                    If (direction = "RIGHT") Then


                        Dim nextcomp As Object = FindCompFromGUID(presenttrack.RightLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    Else
                        Dim nextcomp As Object = FindCompFromGUID(presenttrack.LeftLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    End If

                End If
            ElseIf (TypeOf (comp) Is Switch) Then
                'OrderedtrackCollection.Add(comp)
                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, comp)
                Dim presentSwitch As Switch = TryCast(comp, Switch)

                If (presentSwitch.GUID = EndGuid) Then
                    'end of recursion
                Else
                    'get the altlink for switch
                    Dim AltLinkTrack As Object = Nothing
                    If (presentSwitch.RightAltLink <> 0) Then
                        AltLinkTrack = TryCast(FindCompFromGUID(presentSwitch.RightAltLink), Track)
                    ElseIf (presentSwitch.LeftAltLink <> 0) Then
                        AltLinkTrack = TryCast(FindCompFromGUID(presentSwitch.LeftAltLink), Track)
                    End If
                    'AltLinkTrack = TryCast(FindCompFromGUID(presentSwitch.RightAltLink), Track)
                    If (AltLinkTrack.NonTransverable = True) Then
                        'meaning this switch is normal.
                        'So add this alttrack and proceed with Right Link
                        'OrderedtrackCollection.Add(AltLinkTrack)
                        AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, AltLinkTrack)
                        If (direction = "RIGHT") Then
                            Dim nextcomp As Object = FindCompFromGUID(presentSwitch.RightLink)
                            RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                        Else
                            Dim nextcomp As Object = FindCompFromGUID(presentSwitch.LeftLink)
                            RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                        End If
                    Else
                        'meaning this switch is reverse
                        'so proceed with recursion with AltLink as next comp
                        If (presentSwitch.RightAltLink <> 0) Then

                            If (direction = "RIGHT") Then

                                'first add Right link track component
                                'OrderedtrackCollection.Add(FindCompFromGUID(presentSwitch.RightLink))
                                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, FindCompFromGUID(presentSwitch.RightLink))
                                Dim nextcomp As Object = FindCompFromGUID(presentSwitch.RightAltLink)
                                RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                            Else
                                'first add Left link track component
                                'OrderedtrackCollection.Add(FindCompFromGUID(presentSwitch.RightLink))
                                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, FindCompFromGUID(presentSwitch.RightLink))
                                Dim nextcomp As Object = FindCompFromGUID(presentSwitch.LeftLink)
                                RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                            End If
                        Else
                            If (direction = "RIGHT") Then


                                'First Add the Left link track component
                                'OrderedtrackCollection.Add(FindCompFromGUID(presentSwitch.LeftLink))
                                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, FindCompFromGUID(presentSwitch.LeftLink))
                                Dim nextcomp As Object = FindCompFromGUID(presentSwitch.RightLink)
                                RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                            Else

                                'First Add the Left link track component
                                'OrderedtrackCollection.Add(FindCompFromGUID(presentSwitch.LeftLink))
                                AddCompInOrderedCollection(OrderedtrackCollection, gTrackSelectionCollection, FindCompFromGUID(presentSwitch.LeftLink))
                                Dim nextcomp As Object = FindCompFromGUID(presentSwitch.LeftAltLink)
                                RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                            End If
                        End If

                    End If

                End If
            ElseIf (TypeOf (comp) Is RRXING) Then
                'we skip the RRXING . So not adding the comp to the OrderedCollection
                If (direction = "RIGHT") Then

                    Dim leftCompGuid As Integer = TryCast(OrderedtrackCollection(OrderedtrackCollection.Count - 1), Track).GUID
                    Dim presentRRXING As RRXING = TryCast(comp, RRXING)

                    If (leftCompGuid = presentRRXING.LeftLink) Then
                        Dim nextcomp As Object = FindCompFromGUID(presentRRXING.RightLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    ElseIf (leftCompGuid = presentRRXING.LeftAltLink) Then
                        Dim nextcomp As Object = FindCompFromGUID(presentRRXING.RightAltLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    End If

                Else

                    Dim RightCompGuid As Integer = TryCast(OrderedtrackCollection(OrderedtrackCollection.Count - 1), Track).GUID
                    Dim presentRRXING As RRXING = TryCast(comp, RRXING)
                    If (RightCompGuid = presentRRXING.RightLink) Then
                        Dim nextcomp As Object = FindCompFromGUID(presentRRXING.LeftLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    ElseIf (RightCompGuid = presentRRXING.RightAltLink) Then
                        Dim nextcomp As Object = FindCompFromGUID(presentRRXING.LeftAltLink)
                        RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                    End If

                End If
            ElseIf (TypeOf (comp) Is Signal) Then
                'we skip signal, so we no add the comp
                Dim presentSignal As Signal = TryCast(comp, Signal)
                If (direction = "RIGHT") Then

                    Dim nextcomp As Object = FindCompFromGUID(presentSignal.RightLink)
                    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                Else
                    Dim nextcomp As Object = FindCompFromGUID(presentSignal.LeftLink)
                    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, direction, gTrackSelectionCollection)
                End If

            ElseIf (TypeOf (comp) Is FlipFlop) Then
                'we add Flip FLips in the Traversed list of comp? Ask Raj. Here Im not adding the Flip FLop in the traversal list but Im traversing further after inverting the direction
                Dim FlipFlopNode As FlipFlop = TryCast(comp, FlipFlop)
                OrderedtrackCollection.Add(comp)
                If (direction = "RIGHT") Then
                    Dim nextcomp As Object = FindCompFromGUID(FlipFlopNode.RightLink)
                    OrderedtrackCollection.Add(nextcomp)
                    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, "LEFT", gTrackSelectionCollection)
                Else
                    Dim nextcomp As Object = FindCompFromGUID(FlipFlopNode.LeftLink)
                    OrderedtrackCollection.Add(nextcomp)
                    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, "RIGHT", gTrackSelectionCollection)
                End If

                'If (direction = "RIGHT") Then
                '    Dim nextcomp As Object = FindCompFromGUID(FlipFlopNode.LeftLink)
                '    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, "LEFT", gTrackSelectionCollection)
                'Else
                '    Dim nextcomp As Object = FindCompFromGUID(FlipFlopNode.RightLink)
                '    RecursiveTraversal(nextcomp, OrderedtrackCollection, EndGuid, "RIGHT", gTrackSelectionCollection)
                'End If

            End If


        Catch ex As Exception

        End Try
    End Sub

    Private Sub AddCompInOrderedCollection(ByRef OrderedTrackCollection As ArrayList, ByRef gTrackSelectionCollection As ArrayList, ByRef Node As Object)
        If (gTrackSelectionCollection.Contains(Node)) Then
            'adding in the ordered list only if the Comp existed in TrackSelectionCollection
            OrderedTrackCollection.Add(Node)
        End If
    End Sub

    'Public Sub GetTrainScheduleInfo(trn As TrainID, Optional ByRef strToolTip As String = "", Optional ByRef strDEV As String = "", Optional ByRef trnColor As Object = Nothing)
    '    Dim dispColor As Color = Color.LightSeaGreen
    '    Dim ToolTipText As String = "NONE"
    '    Dim DispText As String = "NONE"
    '    Dim StationName As String = ""
    '    'Dim MystationCollection As New ArrayList
    '    'Dim SubDvn As Subdivision
    '    'Dim subStat As SubdivisionStation = Nothing
    '    Try

    '        Dim TdAmt As Integer = trn.myScheduleDeviation
    '        If TdAmt > 0 Then
    '            ToolTipText = "SCH INFO:  -" & TdAmt.ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
    '            DispText = "-" & TdAmt.ToString
    '        Else
    '            ToolTipText = "SCH INFO: +" & Math.Abs(TdAmt).ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
    '            DispText = "+" & Math.Abs(TdAmt).ToString
    '        End If
    '        'DispText = TdAmt.ToString
    '        If TdAmt > 5 AndAlso TdAmt < 10 Then
    '            dispColor = Color.Yellow
    '        ElseIf TdAmt > 9 Then
    '            dispColor = Color.PaleVioletRed
    '        End If

    '        strToolTip = ToolTipText
    '        strDEV = DispText
    '        trnColor = dispColor





    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Sub
    Public Sub GetTrainScheduleInfo(trn As TrainID, Optional ByRef strToolTip As String = "", Optional ByRef strDEV As String = "", Optional ByRef trnColor As Object = Nothing, Optional ByRef strDEVSign As String = "")

        Dim dispColor As Color = Color.LightSeaGreen
        Dim ToolTipText As String = "NONE"
        Dim DispText As String = "NONE"
        Dim StationName As String = ""
        'Dim MystationCollection As New ArrayList
        'Dim SubDvn As Subdivision
        'Dim subStat As SubdivisionStation = Nothing
        If Not gInitialized Then Exit Sub
        Try
            Dim TdAmt As Double = trn.myScheduleDeviation

            ' DTP-180, DTP-201, DTP-202, DTP-210 - CCP
            If optEnableDenverSpecificOptions Then
                strDEVSign = If(TdAmt > 0, "+", "-")
                If TdAmt < 0 Then
                    dispColor = Color.Red
                ElseIf TdAmt >= 0 AndAlso TdAmt < 5 Then
                    dispColor = Color.Green
                ElseIf TdAmt >= 5 AndAlso TdAmt < 15 Then
                    ' CCP - added "=" 
                    dispColor = Color.Yellow
                ElseIf TdAmt > 15 Then
                    dispColor = Color.Red
                End If
            Else
                strDEVSign = If(TdAmt > 0, "-", "+")
                If TdAmt > 5 AndAlso TdAmt < 10 Then
                    dispColor = Color.Yellow
                ElseIf TdAmt > 9 Then
                    dispColor = Color.PaleVioletRed
                End If
            End If

            ToolTipText = String.Format("SCH INFO:  {0}{1} TO {2}", strDEVSign, If(TdAmt > 0, TdAmt.ToString, Math.Abs(TdAmt).ToString), trn.myScheduleDeviationStation.Trim.ToUpper)
            DispText = strDEVSign & If(TdAmt > 0, TdAmt.ToString, Math.Abs(TdAmt).ToString)

            strToolTip = ToolTipText
            strDEV = DispText
            trnColor = dispColor


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Remove accent
    ''' </summary>
    ''' <param name="message"></param>
    ''' <returns></returns>
    Public Function RemoveAccent(message As String) As String
        Try
            Dim WithAccent As String = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÒÓÔÕÖÙÚÛÜàáâãäåçèéêëìíîïòóôõöùúûü"
            Dim WithoutAccent As String = "AAAAAACEEEEIIIIOOOOOUUUUaaaaaaceeeeiiiiooooouuuu"
            For i As Integer = 1 To Len(message)
                Dim Position As Byte = InStr(1, WithAccent, Mid(message, i, 1))
                If Position > 0 Then
                    Mid(message, i, 1) = Mid(WithoutAccent, Position, 1)
                End If
            Next
            Return message
        Catch ex As Exception
            SystemException(ex.ToString)
            Return message
        End Try
    End Function

    ''' <summary>
    ''' Set device
    ''' </summary>
    ''' <param name="theDevice"></param>
    ''' <param name="theStatus"></param>
    Public Sub SetDevice(theDevice As Device,
                         theStatus As Boolean)
        Try
            SystemEvent($"{GetCurrentMethod.Name}:Status={theStatus}:TrackGuid={theDevice.GUID}")
            SetVisibleArrow(theDevice, theStatus)
            theDevice.JoinAuthAlert = theStatus
            theDevice.ApplyIndicationChange()
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Set Visible Arrow
    ''' </summary>
    ''' <param name="theDevice">the device</param>
    ''' <param name="theValue">the value</param>
    Private Sub SetVisibleArrow(theDevice As Device,
                                theValue As Boolean)
        Try
            If theDevice.InvokeRequired Then
                theDevice.BeginInvoke(New Action(Of Control, Boolean)(AddressOf SetVisibleArrow), theDevice, theValue)
            Else
                theDevice.Visible = theValue
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Remove Alert
    ''' </summary>
    ''' <param name="thealert"></param>
    ''' <returns></returns>
    Public Function RemoveAlert(thealert As Alert) As Boolean
        Try
            Dim summItem As New SummaryItem("ALERT", "REMOVE", thealert)
            SyncLock gSummaryAlertQueue.SyncRoot
                gSummaryAlertQueue.Enqueue(summItem)
            End SyncLock
            SyncLock gSystemAlertList.SyncRoot
                gSystemAlertList.Remove(thealert)
                Return True
            End SyncLock
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Flip direction
    ''' </summary>
    ''' <param name="direction">direction to change</param>
    ''' <returns>Return LEFT is direction is RIGHT, otherwise, return RIGHT</returns>
    Public Function FlipDirection(direction As String) As String
        Return If(direction.Equals(optDirectionLeft), optDirectionRight, optDirectionLeft)
    End Function

    ''' <summary>
    ''' Interrupt Route
    ''' </summary>
    Public Sub InterruptRoute()
        SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
        ClearSystemVariables()
    End Sub

End Module
