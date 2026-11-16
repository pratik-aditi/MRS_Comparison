Imports System.Data.OleDb
Imports System.Text
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrainComponents

Namespace FastRoutes

    Public Class GroupedRoutes

        Public Sub New()

        End Sub

#Region " GROUPED SIGNAL ROUTES "

        Public Function SearchForGroupedSignalRoutes(ByRef EntryTrack As Track, ByRef ExitTrack As Track) As Boolean
            Dim EntrySignal As Signal = Nothing
            Dim ExitSignal As Signal = Nothing
            Dim GPSR As GroupedSignalRoutes = Nothing
            Dim CurrentCompSelection As New ArrayList
            Dim StartDirection As String = ""
            Dim SearchStartDirection As String = ""
            Dim IgnoreEntryClear As Boolean = False
            Dim FirstRouteSearch As Boolean = False
            Try
                If gSelectedSignalRouteList Is Nothing Then gSelectedSignalRouteList = New ArrayList
                If gSelectedSignalRouteList IsNot Nothing AndAlso gSelectedSignalRouteList.Count > 0 Then
                    GetEntryTrackAndSignalFromPreviousRoute(EntryTrack, EntrySignal, SearchStartDirection)
                    If EntrySignal IsNot Nothing Then
                        IgnoreEntryClear = True
                    End If
                End If
                If EntryTrack Is Nothing Then Return False
                If ExitTrack Is Nothing Then Return False

                If gSearchDirection IsNot Nothing AndAlso gSearchDirection.Trim <> "" Then
                Else
                    SearchStartDirection = "RIGHT"
                    FirstRouteSearch = True
                End If


                'Do Right Search
                If SearchStartDirection = "RIGHT" Then
                    GetEntryExitSignal(EntryTrack, ExitTrack, True, EntrySignal, ExitSignal)
                    If EntrySignal IsNot Nothing Then 'AndAlso ExitSignal IsNot Nothing
                        If ExitSignal Is Nothing Then
                            ExitSignal = Nothing
                            GetEntryExitSignal(EntryTrack, ExitTrack, True, EntrySignal, ExitSignal, True)
                        End If

                        Dim OkToProceedRight As Boolean = True
                        If FirstRouteSearch AndAlso EntrySignal Is Nothing Then
                            OkToProceedRight = False
                            SearchStartDirection = "LEFT"
                        End If
                        If OkToProceedRight Then
                            GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                            If GPSR Is Nothing Then
                                ExitSignal = Nothing
                                GetEntryExitSignal(EntryTrack, ExitTrack, True, EntrySignal, ExitSignal, True)
                                If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                                    GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                                End If
                            End If
                        End If

                        If GPSR IsNot Nothing Then
                            'gSearchDirection = "RIGHT"
                        Else
                            If Not IgnoreEntryClear Then EntrySignal = Nothing
                            SearchStartDirection = "LEFT"
                        End If
                    ElseIf FirstRouteSearch AndAlso EntrySignal Is Nothing Then
                        'OkToProceedRight = False
                        If EntryTrack IsNot Nothing Then
                            Me.SystemEvent("SearchForGroupedSignalRoutes: RIGHT ENTRY SIGNAL NOT FOUND FOR TRACK" & EntryTrack.GUID & " CHANGING SEARCH DIRECTION TO LEFT")
                        Else
                            Me.SystemEvent("SearchForGroupedSignalRoutes: RIGHT ENTRY SIGNAL NOT FOUND CHANGING SEARCH DIRECTION TO LEFT")
                        End If

                        SearchStartDirection = "LEFT"
                    End If
                End If


                'Do Left Search
                If SearchStartDirection = "LEFT" Then
                    If GPSR Is Nothing Then
                        'EntrySignal = Nothing
                        ExitSignal = Nothing
                        GetEntryExitSignal(EntryTrack, ExitTrack, False, EntrySignal, ExitSignal)
                        If EntrySignal IsNot Nothing Then
                            If ExitSignal Is Nothing Then
                                ExitSignal = Nothing
                                GetEntryExitSignal(EntryTrack, ExitTrack, False, EntrySignal, ExitSignal, True)
                            End If
                            GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                            If GPSR Is Nothing Then
                                ExitSignal = Nothing
                                GetEntryExitSignal(EntryTrack, ExitTrack, False, EntrySignal, ExitSignal, True)
                                If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                                    GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                                End If
                            End If
                        End If


                        'If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                        '    GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                        '    If GPSR Is Nothing Then
                        '        ExitSignal = Nothing
                        '        GetEntryExitSignal(EntryTrack, ExitTrack, False, EntrySignal, ExitSignal, True)
                        '        If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                        '            GPSR = GetGroupedRoutes(EntrySignal.GUID, ExitSignal.GUID)
                        '        End If
                        '    End If
                        'End If
                        'If GPSR IsNot Nothing Then gSearchDirection = "LEFT"
                    End If
                End If
                If GPSR Is Nothing Then Return False
                If GPSR IsNot Nothing Then
                    StartDirection = GetSignalDirection(GPSR.myEntrySignal)
                    Dim SetgSearchDirection As Boolean = False
                    If gSearchDirection Is Nothing Then
                        SetgSearchDirection = True
                    ElseIf gSearchDirection IsNot Nothing AndAlso gSearchDirection.Trim = "" Then
                        SetgSearchDirection = True
                    End If
                    If SetgSearchDirection Then gSearchDirection = StartDirection

                    Dim EndDirection As String = StartDirection
                    GPSR.myStartDirection = StartDirection
                    GPSR.myEntryTrack = EntryTrack
                    GPSR.myExittrack = ExitTrack
                    Dim Sr As SignalRoute
                    Dim Comp As Object = Nothing
                    For Each Sr In GPSR.myRouteCollection
                        Comp = Nothing
                        If Sr.EntrySignal IsNot Nothing AndAlso Not CurrentCompSelection.Contains(Sr.EntrySignal) Then CurrentCompSelection.Add(Sr.EntrySignal)
                        For Each Comp In Sr.ComponentList
                            If Comp IsNot Nothing AndAlso Not CurrentCompSelection.Contains(Comp) Then CurrentCompSelection.Add(Comp)
                            If Comp.GetType.ToString = gFlipFlopComponentName Then
                                If EndDirection = "RIGHT" Then
                                    EndDirection = "LEFT"
                                ElseIf EndDirection = "LEFT" Then
                                    EndDirection = "RIGHT"
                                End If
                            End If
                        Next
                        If Sr.ExitSignal IsNot Nothing Then
                            If GetSignalDirection(Sr.ExitSignal) = EndDirection Then
                            Else
                                If Not CurrentCompSelection.Contains(Sr.ExitSignal) Then CurrentCompSelection.Add(Sr.ExitSignal)
                            End If
                        End If

                        Comp = Nothing
                        If Sr.myExitTracks IsNot Nothing AndAlso Sr.myExitTracks.Count > 0 Then
                            For Each Comp In Sr.myExitTracks
                                ' If Comp IsNot Nothing AndAlso Not CurrentCompSelection.Contains(Comp) Then CurrentCompSelection.Add(Comp)
                            Next
                        End If
                    Next
                    GPSR.myEndDirection = EndDirection
                    If gSelectedSignalRouteList Is Nothing Then gSelectedSignalRouteList = New ArrayList
                    If gSelectedSignalRouteList IsNot Nothing AndAlso gSelectedSignalRouteList.Count > 0 Then
                        GPSR.myRouteOrder = gSelectedSignalRouteList.Count + 1
                    End If
                    If Not gSelectedSignalRouteList.Contains(GPSR) Then gSelectedSignalRouteList.Add(GPSR)
                End If
                If gSelectedSignalRouteList Is Nothing Then Return False
                If gSelectedSignalRouteList.Count = 0 Then Return False

                'TMDS.SystemForms.MySystemForms.frmMainMDI.ExtendStoreRoute(gSearchDirection, ExitTrack, CurrentCompSelection, EntryTrack)
                If Not ValidateRouteGroupedSignalRoute() Then
                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                    ClearSystemVariables()
                    Return False
                End If
                If Not ValidateRoute(CurrentCompSelection, EntryTrack, ExitTrack) Then
                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                    ClearSystemVariables()
                    Return False
                End If


                Return True


            Catch ex As Exception
                GetErrorDetails(ex)
            End Try


        End Function

        Private Sub GetEntryExitSignal(EntryTrack As Track, ExitTrack As Track, IsRightSearch As Boolean, ByRef EntrySignal As Signal, ByRef ExitSignal As Signal, Optional DoFlipSearch As Boolean = False)
            Dim I As Integer = 0
            Dim NextComp As Object = Nothing
            Try
                If EntrySignal Is Nothing Then
                    NextComp = EntryTrack
                    For I = 0 To 10
                        If NextComp IsNot Nothing Then
                            Select Case NextComp.GetType.ToString
                                Case gSignalComponentName
                                    Dim Dir As String = ""
                                    Dir = GetSignalDirection(NextComp)
                                    If Dir = "RIGHT" AndAlso IsRightSearch Then
                                        EntrySignal = NextComp
                                        Exit For
                                    ElseIf Dir = "LEFT" AndAlso Not IsRightSearch Then
                                        EntrySignal = NextComp
                                        Exit For
                                    End If
                            End Select
                        End If
                        Select Case IsRightSearch
                            Case True
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End Select
                    Next
                End If
                If EntrySignal IsNot Nothing Then
                    Dim sr As SignalRoute = Nothing
                    For Each sr In gSignalRouteList
                        If sr.EntrySignal IsNot Nothing AndAlso sr.EntrySignal Is EntrySignal AndAlso sr.ExitSignal IsNot Nothing Then
                            Dim LeftTrack As Object = Nothing
                            Dim RightTrack As Object = Nothing
                            NextComp = Nothing
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sr.ExitSignal.RightLink)
                            For I = 0 To 3
                                If NextComp IsNot Nothing Then
                                    Select Case NextComp.GetType.ToString
                                        Case gTrackComponentName
                                            If ExitTrack Is NextComp Then
                                                ExitSignal = sr.ExitSignal
                                                Exit Sub
                                            End If
                                    End Select
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                End If
                            Next
                            NextComp = Nothing
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(sr.ExitSignal.LeftLink)
                            For I = 0 To 3
                                If NextComp IsNot Nothing Then
                                    Select Case NextComp.GetType.ToString
                                        Case gTrackComponentName
                                            If ExitTrack Is NextComp Then
                                                ExitSignal = sr.ExitSignal
                                                Exit Sub
                                            End If
                                    End Select
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                End If
                            Next
                        End If
                    Next

                    Dim EntrySignalDir As String = ""
                    EntrySignalDir = GetSignalDirection(EntrySignal)
                    If DoFlipSearch Then
                        Select Case EntrySignalDir
                            Case "RIGHT"
                                EntrySignalDir = "LEFT"
                            Case "LEFT"
                                EntrySignalDir = "RIGHT"
                        End Select
                    End If
                    NextComp = Nothing
                    Dim RightComp As Object = Nothing
                    Dim LeftComp As Object = Nothing
                    Select Case EntrySignalDir
                        Case "RIGHT"
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
                        Case "LEFT"
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
                    End Select
                    RightComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
                    LeftComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
                    If RightComp IsNot Nothing AndAlso RightComp.GetType.ToString = gSignalComponentName Then
                        If IsValidSignalCombination(EntrySignal, RightComp) Then
                            ExitSignal = RightComp
                            Exit Sub
                        End If
                    End If
                    If LeftComp IsNot Nothing AndAlso LeftComp.GetType.ToString = gSignalComponentName Then
                        If IsValidSignalCombination(EntrySignal, LeftComp) Then
                            ExitSignal = LeftComp
                            Exit Sub
                        End If
                    End If

                    'If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName Then
                    '    If IsValidSignalCombination(EntrySignal, NextComp) Then
                    '        ExitSignal = NextComp
                    '        Exit Sub
                    '    End If
                    'End If
                    NextComp = Nothing
                    NextComp = ExitTrack
                    For I = 0 To 10
                        If NextComp IsNot Nothing Then
                            Select Case NextComp.GetType.ToString
                                Case gSignalComponentName
                                    Dim Dir As String = ""
                                    Dir = GetSignalDirection(NextComp)
                                    If Not DoFlipSearch Then
                                        If EntrySignalDir = "LEFT" AndAlso Dir = "RIGHT" AndAlso Not IsRightSearch Then
                                            ExitSignal = NextComp
                                            Exit For
                                        ElseIf EntrySignalDir = "RIGHT" AndAlso Dir = "LEFT" AndAlso IsRightSearch Then
                                            ExitSignal = NextComp
                                            Exit For
                                        End If
                                    Else
                                        If EntrySignalDir = "LEFT" Then
                                            ExitSignal = NextComp
                                            Exit For
                                        ElseIf EntrySignalDir = "RIGHT" Then
                                            ExitSignal = NextComp
                                            Exit For
                                        End If
                                    End If

                            End Select
                        End If
                        Select Case EntrySignalDir
                            Case "RIGHT"
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                            Case "LEFT"
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End Select
                    Next
                    'Select Case EntrySignalDir
                    '    Case "RIGHT"
                    '    Case "LEFT"
                    'End Select
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Function GetGroupedRoutes(EntrySignalGUID As Integer, ExitSignalGUID As Integer) As GroupedSignalRoutes
            Dim tEntrySignal As Signal = Nothing
            Dim tExitSignal As Signal = Nothing
            Dim tmpSgCollection As New ArrayList
            Dim dr As OleDbDataReader = Nothing
            Dim dbcomm As OleDbCommand = Nothing
            Try

                Dim SelectSQL As New StringBuilder("SELECT * FROM [tblSystemGroupedRoutes] WHERE [Validated]=1 AND  [EntrySignal]= " & EntrySignalGUID & " AND [ExitSignal]= " & ExitSignalGUID & " ORDER BY [Priority]")


                Using dbConn As New OleDbConnection(gConnectionStringStatic)
                    dbConn.Open()
                    dbcomm = New OleDbCommand(SelectSQL.ToString, dbConn)
                    dr = dbcomm.ExecuteReader
                    If dr.HasRows Then
                        While dr.Read
                            Dim tmpGPSR As New GroupedSignalRoutes
                            If Not dr.IsDBNull(0) AndAlso Not dr.IsDBNull(1) AndAlso Not dr.IsDBNull(2) AndAlso Not dr.IsDBNull(4) Then
                                Dim OkToProcess As Boolean = True
                                tmpSgCollection = New ArrayList
                                tEntrySignal = Nothing
                                tExitSignal = Nothing

                                Dim iEntrySignal As Integer = 0
                                Dim iExitSignal As Integer = 0
                                Dim sRouteCollection As String = ""

                                Integer.TryParse(dr.GetValue(1), iEntrySignal)
                                Integer.TryParse(dr.GetValue(2), iExitSignal)
                                sRouteCollection = dr.GetValue(4)
                                tEntrySignal = TryCast(TMDS.SystemUtilities.FetchData.FindComponentFromGUID(iEntrySignal), Signal)
                                tExitSignal = TryCast(TMDS.SystemUtilities.FetchData.FindComponentFromGUID(iExitSignal), Signal)
                                If sRouteCollection IsNot Nothing AndAlso Not String.IsNullOrEmpty(sRouteCollection) AndAlso sRouteCollection.Trim <> "" Then
                                    Dim sRouteCollectionData() As String = sRouteCollection.Split("|")
                                    Dim s As String
                                    For Each s In sRouteCollectionData
                                        If s.Trim <> "" Then
                                            Dim SR As SignalRoute = GetValidSr(s) 'TMDS.SystemUtilities.FetchData.FindSignalRouteByGUID(s)
                                            If SR IsNot Nothing AndAlso SR.ComponentList IsNot Nothing AndAlso SR.ComponentList.Count > 0 Then
                                                If Not tmpSgCollection.Contains(SR) Then tmpSgCollection.Add(SR)

                                            Else
                                                SystemEvent("UNABLE TO LOAD SIGNAL ROUTE [GUID] " & s)
                                                'Return Nothing
                                                OkToProcess = False
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                                If OkToProcess Then
                                    If tEntrySignal IsNot Nothing AndAlso tExitSignal IsNot Nothing AndAlso tmpSgCollection IsNot Nothing AndAlso tmpSgCollection.Count > 0 Then
                                        With tmpGPSR
                                            .myGUID = dr.GetValue(0)
                                            .myEntrySignal = tEntrySignal
                                            .myExitSignal = tExitSignal
                                            .myRouteCollectionString = sRouteCollection
                                            .myRouteCollection = tmpSgCollection
                                            If Not dr.IsDBNull(3) Then .myPriority = dr.GetValue(3)
                                            If Not dr.IsDBNull(6) Then .myTerritory = dr.GetValue(6)
                                            Dim CPName As String = ""
                                            Dim CP As ControlPoint = Nothing
                                            If tEntrySignal IsNot Nothing Then
                                                CP = tEntrySignal.myControlPoint
                                                If CP IsNot Nothing Then
                                                    .myControlPointName = CP.Name
                                                    .myControlPointNumber = CP.GUID
                                                End If
                                            End If
                                        End With
                                        Return tmpGPSR
                                    End If
                                End If
                            End If


                        End While
                    End If
                    dr.Close()
                    dbConn.Close()
                End Using

            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                If dbcomm IsNot Nothing Then dbcomm.Dispose()
                dbcomm = Nothing
                dr = Nothing
            End Try
            Return Nothing
        End Function

        Private Sub GetEntryTrackAndSignalFromPreviousRoute(ByRef EntryTrack As Track, ByRef EntrySignal As Signal, ByRef StartSearchDirection As String)
            Dim GPSR As GroupedSignalRoutes = Nothing
            Dim Lastcomp As Object = Nothing
            Dim Nextcomp As Object = Nothing
            Dim SearchDirection As String = ""
            Dim tmpEntrySignal As Signal = Nothing
            Dim tmpEntrytrack As Track = Nothing
            Try
                GPSR = gSelectedSignalRouteList(gSelectedSignalRouteList.Count - 1)
                SearchDirection = GPSR.myEndDirection
                'Select Case SearchDirection
                '    Case "RIGHT"
                '        Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(GPSR.myExitSignal.RightLink)
                '    Case "LEFT"
                '        Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(GPSR.myExitSignal.LeftLink)
                'End Select

                Select Case SearchDirection
                    Case "RIGHT"
                        Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(GPSR.myExitSignal.LeftLink)
                    Case "LEFT"
                        Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(GPSR.myExitSignal.RightLink)
                End Select


                For i As Integer = 0 To 20
                    If Nextcomp Is Nothing Then Exit Sub
                    Lastcomp = Nextcomp
                    Select Case Nextcomp.GetType.ToString
                        Case gSignalComponentName
                            Dim SigDirection As String = GetSignalDirection(Nextcomp)
                            If SearchDirection = SigDirection Then
                                tmpEntrySignal = Nextcomp
                                Exit For
                            End If
                            Select Case SearchDirection
                                Case "RIGHT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.RightLink)
                                Case "LEFT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.LeftLink)
                            End Select
                        Case gFlipFlopComponentName
                            Select Case SearchDirection
                                Case "RIGHT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.RightLink)
                                    SearchDirection = "LEFT"
                                Case "LEFT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.LeftLink)
                                    SearchDirection = "RIGHT"
                            End Select
                        Case gRRXINGComponentName
                            Dim RR As RRXING = TryCast(Nextcomp, RRXING)
                            Dim ExitGUID As Integer
                            If RR IsNot Nothing AndAlso Lastcomp IsNot Nothing Then
                                ExitGUID = RR.FindExitGUID(Lastcomp.GUID)
                            End If
                            Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitGUID)
                        Case Else
                            Select Case SearchDirection
                                Case "RIGHT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.RightLink)
                                Case "LEFT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.LeftLink)
                            End Select
                    End Select
                Next

                If tmpEntrySignal IsNot Nothing Then
                    Dim SigDirection As String = GetSignalDirection(tmpEntrySignal)
                    Nextcomp = Nothing
                    StartSearchDirection = GPSR.myEndDirection
                    Select Case SigDirection
                        Case "LEFT"
                            Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(tmpEntrySignal.RightLink)
                        Case "RIGHT"
                            Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(tmpEntrySignal.LeftLink)
                    End Select

                    For i As Integer = 0 To 4
                        If Nextcomp Is Nothing Then Exit For
                        If Nextcomp.GetType.ToString = gTrackComponentName Then
                            tmpEntrytrack = Nextcomp
                            Exit For
                        Else
                            Select Case SigDirection
                                Case "LEFT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.RightLink)
                                Case "RIGHT"
                                    Nextcomp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(Nextcomp.LeftLink)
                            End Select
                        End If
                    Next

                    EntrySignal = tmpEntrySignal
                    If tmpEntrytrack IsNot Nothing Then EntryTrack = tmpEntrytrack
                End If



            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Sub

        Private Function IsValidSignalCombination(EntrySignal As Signal, ExitSignal As Signal) As Boolean
            Dim dr As OleDbDataReader = Nothing
            Dim dbcomm As OleDbCommand = Nothing
            Try
                Dim SelectSQL As New StringBuilder("SELECT [GroupRouteUID] FROM [tblSystemGroupedRoutes] WHERE [Validated]=1 AND [EntrySignal]= " & EntrySignal.GUID & " AND [ExitSignal]= " & ExitSignal.GUID)
                Using dbConn As New OleDbConnection(gConnectionStringStatic)
                    dbConn.Open()
                    dbcomm = New OleDbCommand(SelectSQL.ToString, dbConn)
                    dr = dbcomm.ExecuteReader
                    If dr.HasRows Then
                        Return True
                    End If
                    dr.Close()
                    dbConn.Close()
                End Using
            Catch ex As Exception
                GetErrorDetails(ex)
            Finally
                If dbcomm IsNot Nothing Then dbcomm.Dispose()
                dbcomm = Nothing
                dr = Nothing
            End Try
        End Function

        Private Function DoSRConflictCheck(CompList As ArrayList) As Boolean
            Dim Comp As Object = Nothing
            Try
                If CompList IsNot Nothing AndAlso CompList.Count > 0 Then
                    For Each Comp In CompList
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                If Comp.DeviceBlocked Then
                                    Return False
                                ElseIf Comp.TrackAndTime Then
                                    Return False
                                ElseIf Comp.TrackAuthorities IsNot Nothing AndAlso Comp.TrackAuthorities.Count > 0 Then
                                    Return False
                                ElseIf Comp.OtherSplAuthorities IsNot Nothing AndAlso Comp.OtherSplAuthorities.Count > 0 Then
                                    Return False
                                End If
                            Case gSwitchComponentName
                                If Comp.DeviceBlocked Then Return False
                        End Select
                    Next
                End If
                Return True

            Catch ex As Exception
                GetErrorDetails(ex)
            End Try

        End Function

        Private Function GetValidSr(SRSystemUID As String) As SignalRoute
            Dim tmpSR As SignalRoute = Nothing
            Dim SR As SignalRoute = Nothing
            Dim SRCollection As New List(Of SignalRoute)
            Try
                tmpSR = TMDS.SystemUtilities.FetchData.FindSignalRouteByGUID(SRSystemUID)
                If tmpSR IsNot Nothing Then
                    If IsValidSR(tmpSR) AndAlso tmpSR.ComponentList IsNot Nothing AndAlso tmpSR.ComponentList.Count > 0 Then
                        If Not SRCollection.Contains(tmpSR) Then SRCollection.Add(tmpSR)
                        For Each SR In gSignalRouteList
                            If IsValidSR(SR) AndAlso SR.ComponentList IsNot Nothing AndAlso SR.ComponentList.Count > 0 Then
                                If SR.EntrySignal.GUID = tmpSR.EntrySignal.GUID AndAlso SR.ExitSignal.GUID = tmpSR.ExitSignal.GUID Then
                                    If Not SRCollection.Contains(SR) Then SRCollection.Add(SR)
                                End If
                            End If
                        Next
                        If SRCollection IsNot Nothing AndAlso SRCollection.Count > 0 Then
                            SRCollection.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                            For Each SR In SRCollection
                                Dim SRConflictCheckOK As Boolean = True
                                If DoSRConflictCheck(SR.ComponentList) Then
                                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                                        If Not DoSRConflictCheck(SR.myExitTracks) Then
                                            SRConflictCheckOK = False
                                        End If
                                    End If
                                    If SRConflictCheckOK Then Return SR
                                End If
                            Next
                        End If
                    End If
                End If
                Return Nothing


            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
            Return Nothing
        End Function

        Private Function IsValidSR(SRToCheck As SignalRoute) As Boolean
            Try
                If SRToCheck IsNot Nothing AndAlso SRToCheck.EntrySignal IsNot Nothing AndAlso SRToCheck.ExitSignal IsNot Nothing Then
                    Return True
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
            End Try
        End Function

        Private Function ValidateRouteGroupedSignalRoute() As Boolean '(ByRef CurrentCompSelection As ArrayList, ByRef enterTrack As Track, ByRef exitTrack As Track) As Boolean
            Dim TmpGroupedRoutes As New ArrayList
            Dim GSR As GroupedSignalRoutes = Nothing
            Dim CurrentCompSelection As New ArrayList
            Dim tComp As Object = Nothing
            Try
                'gSelectedSignalRouteList
                For Each GSR In gSelectedSignalRouteList
                    If GSR IsNot Nothing AndAlso Not GSR.tmpProcessed Then
                        If Not TmpGroupedRoutes.Contains(GSR) Then TmpGroupedRoutes.Add(GSR)
                    End If
                Next
                If TmpGroupedRoutes.Count = 0 Then Return False

                For Each GSR In TmpGroupedRoutes
                    If GSR.myRouteCollection IsNot Nothing AndAlso GSR.myRouteCollection.Count > 0 Then
                        Dim sr As SignalRoute = Nothing
                        Dim tmpgDoStoredRoute As Boolean = False
                        For Each sr In GSR.myRouteCollection
                            If sr IsNot Nothing Then


                                'VALIDATE THE COMPONENTS TO SEE IF THESE ARE OK TO CLEAR A ROUTE
                                Dim compToCheck As Object
                                Dim OverrideStoredRoute As Boolean
                                Dim LastComp As Object = Nothing

                                Dim FlipFlopFound As Boolean = False
                                Dim TempList As New ArrayList
                                CopyArrayCollection(TempList, sr.ComponentList)
                                'TMDS.SystemForms.MySystemForms.frmMainMDI.ExtendStoreRoute(sr.myDirection, sr.ExitTrack, TempList, sr.ExitTrack)
                                ExtendStoreRoute(sr.myDirection, sr.ExitTrack, TempList, sr.ExitTrack)
                                Dim SearchDirection As String = gSearchDirection
                                For Each compToCheck In TempList
                                    Select Case compToCheck.GetType.ToString

                                        Case "TMDS.TrackOtherComponents.RRXING"
                                            If Not compToCheck.ValidateForAllClearRoute Then
                                                If compToCheck.ValidateForAllStoreRoute Then
                                                    Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-ValidateForAllStoreRoute")
                                                    tmpgDoStoredRoute = True
                                                Else
                                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                    ClearSystemVariables()
                                                    Return False
                                                End If
                                            End If

                                        Case gFlipFlopComponentName
                                            'If gSearchDirection = "LEFT" Then
                                            '    gSearchDirection = "RIGHT"
                                            'Else
                                            '    gSearchDirection = "LEFT"
                                            'End If
                                            If SearchDirection = "LEFT" Then
                                                SearchDirection = "RIGHT"
                                            Else
                                                SearchDirection = "LEFT"
                                            End If
                                            If FlipFlopFound Then
                                                FlipFlopFound = False
                                            Else
                                                FlipFlopFound = True
                                            End If

                                        Case gTrackComponentName
                                            Dim trk As Track = CType(compToCheck, Track)
                                            If compToCheck.BlockAbsolute And Not compToCheck Is gFirstEntryTrack Then
                                                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                ClearSystemVariables()
                                                Return False
                                            ElseIf compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> SearchDirection) _
                                            Or compToCheck.OStype And compToCheck.Occuppied Then

                                                'THIS IS FOR ALLOWING LICENSES OVER OCCUPPIED OS TRACKS for MRS
                                                If optEnableGeoFenceOperations Then
                                                    OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(TempList, trk)
                                                    If OverrideStoredRoute AndAlso trk.SlottingTrack Then
                                                        TMDS.SystemForms.MySystemForms.frmMainMDI.DoTagSignalToIgnoreForTrainLicense(TempList, trk)
                                                    End If
                                                End If
                                                'THIS IS FOR ALLOWING LICENSES OVER OCCUPPIED OS TRACKS for MRS

                                                If Not OverrideStoredRoute Then
                                                    tmpgDoStoredRoute = True
                                                End If
                                                OverrideStoredRoute = False

                                                Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                    gEntExtTrackSelection.Add(compToCheck)
                                                End If
                                            ElseIf compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then
                                                'Dim trk As Track = CType(compToCheck, Track)
                                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> SearchDirection) Then
                                                    Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Track1:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " SearchDirection:" & SearchDirection.ToString)
                                                    tmpgDoStoredRoute = True
                                                End If
                                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                    gEntExtTrackSelection.Add(compToCheck)
                                                End If
                                            Else
                                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> SearchDirection) Then
                                                    If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then
                                                    Else
                                                        Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Track2:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & SearchDirection.ToString)
                                                        tmpgDoStoredRoute = True
                                                    End If
                                                End If


                                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                    gEntExtTrackSelection.Add(compToCheck)
                                                End If

                                                If optEnableGeoFenceOperations Then
                                                    If trk.TrackAuthorities.Count > 0 Then
                                                        If IsNothing(gCreateLicenseTrain) Then tmpgDoStoredRoute = True
                                                        If Not IsNothing(gCreateLicenseTrain) Then
                                                            Dim a As TrackAuthority.TrackAuthority
                                                            For Each a In trk.TrackAuthorities
                                                                If a.TrainSymbol <> gCreateLicenseTrain.Name Then
                                                                    tmpgDoStoredRoute = True
                                                                End If
                                                            Next
                                                        End If
                                                    End If
                                                End If
                                            End If

                                            If trk.Occuppied Then
                                                If (trk.DisallowRightSignalRequestOnOccupancy AndAlso SearchDirection = "RIGHT" OrElse trk.DisallowLeftSignalRequestOnOccupancy AndAlso SearchDirection = "LEFT") Then
                                                    If trk.GUID = sr.EntryTrack.GUID Then
                                                        If trk.TrainIDs.Count > 0 Then
                                                            'Dim trn As TrainID = enterTrack.TrainIDs(0)
                                                            'If trn.Name = gCreateLicenseTrain.Name Then
                                                            Me.SystemEvent("SearchForRoute-OverrideStoredRoute1-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                            OverrideStoredRoute = True
                                                            'End If
                                                        ElseIf trk.IsLinkToRemoteTrack Then
                                                            If trk.Name = trk.myRemoteTrack.thisTrack.Name AndAlso trk.myRemoteTrack.thisTrack.TrainIDs.Count > 0 Then
                                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute2-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                                OverrideStoredRoute = True
                                                            End If
                                                        Else
                                                            If optEnableGeoFenceOperations Then
                                                                OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(TempList, compToCheck)
                                                            End If
                                                        End If
                                                    ElseIf trk.Name = sr.EntryTrack.Name Then
                                                        If SearchDirection = "LEFT" Then
                                                            If trk.RightLink = sr.EntryTrack.GUID Then
                                                                OverrideStoredRoute = True
                                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute5-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                            End If
                                                        ElseIf SearchDirection = "LEFT" Then
                                                            If trk.LeftLink = sr.EntryTrack.GUID Then
                                                                OverrideStoredRoute = True
                                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute6-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                            End If
                                                        End If

                                                    ElseIf Not IsNothing(gCreateLicenseTrain) AndAlso gCreateLicenseTrain.GeoFenceLegacyEnabled Then
                                                        If trk.TrainIDs.Count > 0 Then
                                                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                                                            If Not IsNothing(trn) Then
                                                                If trn.Name = gCreateLicenseTrain.Name Then
                                                                    OverrideStoredRoute = True
                                                                End If
                                                            End If
                                                        End If
                                                    End If

                                                    If optEnableGeoFenceOperations AndAlso Not IsNothing(gCreateLicenseTrain) Then
                                                        OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(TempList, compToCheck)
                                                    End If

                                                    If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then
                                                        OverrideStoredRoute = True
                                                    End If

                                                    If Not OverrideStoredRoute Then
                                                        Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Track5:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & SearchDirection.ToString)
                                                        tmpgDoStoredRoute = True
                                                    Else
                                                        OverrideStoredRoute = False
                                                    End If
                                                End If
                                            End If

                                            'CATENARY CHECK FOR ENERGIZED CIRCUIT
                                            If optEnableTractionPowerManagementSystem Then
                                                If trk.EnableTractionPowerManagement AndAlso Not IsNothing(trk.myCatenaryTrack) AndAlso Not trk.myCatenaryTrack.IsEnergized Then
                                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                    ClearSystemVariables()
                                                    Return False
                                                    'tmpgDoStoredRoute = True
                                                End If
                                            End If


                                        Case "TMDS.SwitchComponents.Switch"
                                            Dim sw As Switch = CType(compToCheck, Switch)
                                            Dim SwitchUnmoveable As Boolean
                                            Dim NeedBrotherSwitchCheck As Boolean

                                            If Not compToCheck.CheckBrotherSwitchesForLock(False, False) Then
                                                NeedBrotherSwitchCheck = True
                                            End If

                                            'check to see if switch is lined for the route being requested
                                            If sw.BlockAbsolute Or sw.SwitchLockBit Or NeedBrotherSwitchCheck Or sw.IndicateOnly Then
                                                Dim RightLinkTrk As Track
                                                Dim LeftLinkTrk As Track
                                                If compToCheck.ControlledReverse Then
                                                    If compToCheck.RightAltLink <> 0 Then
                                                        RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightAltLink)
                                                        LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftLink)
                                                    Else
                                                        RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightLink)
                                                        LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftAltLink)
                                                    End If
                                                Else
                                                    RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightLink)
                                                    LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftLink)
                                                End If
                                                If Not TempList.Contains(LeftLinkTrk) Then
                                                    SwitchUnmoveable = True
                                                End If
                                                If Not TempList.Contains(RightLinkTrk) Then
                                                    SwitchUnmoveable = True
                                                End If

                                            End If


                                            If SwitchUnmoveable Or _
                                               compToCheck.SignalStatOnMe <> SwitchComponents.Switch.SignalStatus.SignalAtStop Then
                                                ' Dim sw As Switch = CType(compToCheck, Switch)
                                                Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Switch:" & sw.GUID & " SignalStatOnMe:" & sw.SignalStatOnMe)
                                                Dim OverrideStoreRoute As Boolean
                                                If Not IsNothing(gCreateLicenseTrain) Then
                                                    If Not compToCheck.DoesSwitchHaveLicense(gCreateLicenseTrain) Then
                                                        OverrideStoreRoute = True
                                                    End If
                                                End If
                                                If Not OverrideStoreRoute Then
                                                    tmpgDoStoredRoute = True
                                                Else
                                                    OverrideStoreRoute = False
                                                End If


                                                If Not IsNothing(gCreateLicenseTrain) Then
                                                    If optEnableClearSignalWithSignalIntime AndAlso sw.SignalStatOnMe = Switch.SignalStatus.SignalInTime AndAlso sw.TrafficDirection = SearchDirection Then
                                                        tmpgDoStoredRoute = False
                                                    End If
                                                End If
                                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                    gEntExtTrackSelection.Add(compToCheck)
                                                    compToCheck.SelectionMode = SwitchComponents.Switch.ModeSelection.RouteRequesting
                                                    compToCheck.FlashComponent = True
                                                End If

                                            Else

                                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                    gEntExtTrackSelection.Add(compToCheck)
                                                    compToCheck.SelectionMode = SwitchComponents.Switch.ModeSelection.RouteRequesting
                                                    compToCheck.FlashComponent = True
                                                End If
                                            End If
                                        Case "TMDS.SignalComponents.Signal"
                                            Dim sig As Signal = CType(compToCheck, Signal)

                                            Dim AllowSignalSelection As Boolean = True
                                            If sig.AutomaticSignal And Not optEnableLicenseGenerationForPTC Then
                                                AllowSignalSelection = False
                                            ElseIf sig.AutomaticSignal And optEnableLicenseGenerationForPTC Then
                                                If IsNothing(gCreateLicenseTrain) Then AllowSignalSelection = False
                                                'ElseIf Not FlipFlopFound AndAlso ((gSearchDirection = "LEFT" AndAlso sig.ComponentType = "R") OrElse (gSearchDirection = "RIGHT" AndAlso sig.ComponentType = "L")) Then
                                                '    AllowSignalSelection = False
                                            End If

                                            If AllowSignalSelection Then
                                                If (SearchDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                    ClearSystemVariables()
                                                    Exit Function
                                                End If
                                                If (SearchDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                    ClearSystemVariables()
                                                    Exit Function
                                                End If

                                                'check for fleeted signals at this control point
                                                Dim comp As Object
                                                For Each comp In sig.myControlPoint.AssignedComponenets
                                                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                                        If comp.FleetEnabled And (sig.Name = comp.Name) Then
                                                            TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                            ClearSystemVariables()
                                                            tmpgDoStoredRoute = False
                                                            Exit Function

                                                        ElseIf comp.FleetEnabled Then
                                                            Dim sigCheck As Signal = CType(comp, Signal)
                                                            If Not IsNothing(sigCheck.SignalRouteCollection) Then
                                                                Dim compCheck As Object
                                                                For Each compCheck In TempList
                                                                    If sigCheck.SignalRouteCollection.Contains(compCheck) Then
                                                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                                        ClearSystemVariables()
                                                                        tmpgDoStoredRoute = False
                                                                        Exit Function
                                                                    End If
                                                                Next
                                                            End If
                                                        End If
                                                    End If
                                                Next

                                                If compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus Then
                                                    If compToCheck.SignalStatus = SignalComponents.Signal.BulbStatus.Unsolitated Then
                                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                        ClearSystemVariables()
                                                        Exit Function
                                                    End If
                                                    Dim sig1 As Signal = CType(compToCheck, Signal)
                                                    Me.SystemEvent("SearchForRoute-tmpgDoStoredRoute-Check-Signal:" & sig1.GUID & "SignalStatus:" & sig1.SignalStatus)
                                                    Dim OverrideStoreRoute As Boolean
                                                    If Not IsNothing(gCreateLicenseTrain) Then
                                                        If Not sig1.DoesSignalHaveLicense(gCreateLicenseTrain) Then
                                                            OverrideStoreRoute = True
                                                        End If
                                                    End If
                                                    If Not OverrideStoreRoute Then
                                                        tmpgDoStoredRoute = True
                                                    Else
                                                        OverrideStoreRoute = False
                                                    End If


                                                    If optEnableClearSignalWithSignalIntime AndAlso sig1.SignalStatus = Signal.BulbStatus.InTime Then
                                                        tmpgDoStoredRoute = False
                                                    End If

                                                    If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                                    Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

                                                        If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                            gEntExtTrackSelection.Add(compToCheck)
                                                        End If
                                                    End If

                                                Else
                                                    If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                                    Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

                                                        If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                                            gEntExtTrackSelection.Add(compToCheck)
                                                        End If
                                                    End If
                                                End If
                                            Else
                                                TempList.Remove(compToCheck)
                                            End If
                                    End Select
                                Next
                                sr.tmpProcessAsStoredRoute = tmpgDoStoredRoute

                                ''''''''''
                                For Each tComp In TempList
                                    If Not CurrentCompSelection.Contains(tComp) Then CurrentCompSelection.Add(tComp)
                                Next
                            End If
                        Next
                    End If
                Next

                For Each GSR In TmpGroupedRoutes
                    Dim gsr2 As GroupedSignalRoutes
                    For Each gsr2 In gSelectedSignalRouteList
                        If gsr2.myGUID = GSR.myGUID Then
                            gsr2.tmpProcessed = True
                            Exit For
                        End If
                    Next
                Next

                TMDS.SystemForms.MySystemForms.frmMainMDI.tmrTrackSelectionEntExt.Enabled = True

                Me.SystemEvent("SearchForRoute-Step3")

                'If tmpgDoStoredRoute OrElse gStackRoute OrElse gGropupedStoredRoute Then
                '    If gStoredRouteList Is Nothing Then
                '        gStoredRouteList = New ArrayList()
                '    End If
                '    If optEnableLicenseGenerationForPTC Then
                '        TMDS.SystemForms.MySystemForms.frmMainMDI.FilterAutomaticSignalsForStoredRoute(CurrentCompSelection, gStoredRouteList)
                '    Else
                '        Dim comp As Object
                '        For Each comp In CurrentCompSelection
                '            gStoredRouteList.Add(comp)
                '        Next
                '    End If
                'End If

                'LastComp = enterTrack

                ''If FlipFlopFound Then
                ''    If gSearchDirection = "LEFT" Then
                ''        gSearchDirection = "RIGHT"
                ''    Else
                ''        gSearchDirection = "LEFT"
                ''    End If
                ''End If
                'If FlipFlopFound Then
                '    If SearchDirection = "LEFT" Then
                '        SearchDirection = "RIGHT"
                '    Else
                '        SearchDirection = "LEFT"
                '    End If
                'End If

                ''Add all the switches and signals in the route to the gEntExtTrackSelection arraylist
                'For Each compToCheck In CurrentCompSelection
                '    Select Case compToCheck.GetType.ToString

                '        Case "TMDS.TrackComponents.Track"
                '            If Not LastComp Is Nothing Then
                '                If LastComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                '                    If LastComp.RightAltLink = compToCheck.GUID Then
                '                        If Not LastComp.FlipOperations Then
                '                            If Not LastComp.ControlledReverse Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        Else
                '                            If LastComp.ControlledReverse Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        End If
                '                    ElseIf LastComp.RightLink = compToCheck.GUID Then
                '                        If Not LastComp.FlipOperations Then
                '                            If LastComp.ControlledReverse And SearchDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        Else
                '                            If Not LastComp.ControlledReverse And SearchDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        End If
                '                    End If

                '                    If LastComp.LeftAltLink = compToCheck.GUID Then
                '                        If Not LastComp.FlipOperations Then
                '                            If Not LastComp.ControlledReverse Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        Else
                '                            If LastComp.ControlledReverse Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        End If
                '                    ElseIf LastComp.LeftLink = compToCheck.GUID Then
                '                        If Not LastComp.FlipOperations Then
                '                            If LastComp.ControlledReverse And SearchDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If
                '                        Else
                '                            If Not LastComp.ControlledReverse And SearchDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
                '                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                '                            End If

                '                        End If

                '                    End If
                '                End If
                '            End If
                '            LastComp = compToCheck
                '        Case "TMDS.SwitchComponents.Switch"
                '            Dim sw As Switch = CType(compToCheck, Switch)

                '            'Traveling Left through a switch from a right Alt Link turn out. 
                '            'Check switch for reverve control, if not then add to list
                '            If sw.TypeOfSwitch = "DERAIL" Then
                '                If compToCheck.ControlledReverse Then
                '                    AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                End If
                '            Else
                '                If compToCheck.RightAltLink = LastComp.GUID Then
                '                    If Not compToCheck.FlipOperations Then
                '                        If Not compToCheck.ControlledReverse Then
                '                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                        End If
                '                    Else
                '                        If compToCheck.ControlledReverse Then
                '                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                        End If
                '                    End If
                '                    'Traveling Left through a switch from a right Link straight track. 
                '                    'Check switch for normal control, if not then add to list.
                '                ElseIf compToCheck.RightLink = LastComp.GUID And compToCheck.RightAltLink <> 0 Then
                '                    If Not compToCheck.FlipOperations Then
                '                        If compToCheck.ControlledReverse And SearchDirection = "LEFT" Then
                '                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                        End If
                '                    Else
                '                        If Not compToCheck.ControlledReverse And SearchDirection = "LEFT" Then
                '                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                        End If
                '                    End If
                '                End If
                '            End If

                '            'Traveling Right through a switch from a Left Alt Link turn out. 
                '            'Check switch for reverve control, if not then add to list
                '            If compToCheck.LeftAltLink = LastComp.GUID Then
                '                If Not compToCheck.FlipOperations Then
                '                    If Not compToCheck.ControlledReverse Then
                '                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                    End If
                '                Else
                '                    If compToCheck.ControlledReverse Then
                '                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                    End If
                '                End If
                '            End If

                '            'Traveling Right through a switch from a Left Link straight track
                '            'Check switch for normal control, if not then add to list
                '            If compToCheck.LeftLink = LastComp.GUID And compToCheck.LeftAltLink <> 0 Then
                '                If Not compToCheck.FlipOperations Then
                '                    If compToCheck.ControlledReverse And SearchDirection = "RIGHT" Then
                '                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                    End If
                '                Else
                '                    If Not compToCheck.ControlledReverse And SearchDirection = "RIGHT" Then
                '                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                '                    End If
                '                End If
                '            End If
                '            LastComp = compToCheck

                '        Case "TMDS.SignalComponents.Signal"

                '            Dim sig As Signal = CType(compToCheck, Signal)
                '            Dim AllowSignalSelection As Boolean = True
                '            If sig.AutomaticSignal And Not optEnableLicenseGenerationForPTC Then
                '                AllowSignalSelection = False
                '            ElseIf sig.AutomaticSignal And optEnableLicenseGenerationForPTC Then
                '                If IsNothing(gCreateLicenseTrain) Then AllowSignalSelection = False
                '            End If

                '            If AllowSignalSelection Then
                '                If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                '                Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then
                '                    If Not gEntExtExecutionSelection.Contains(compToCheck) Then
                '                        gEntExtExecutionSelection.Add(compToCheck)
                '                        If gGenerateRouteForTrainMode AndAlso compToCheck IsNot Nothing Then
                '                            If Not gManualRouteSelectionList.Contains(compToCheck) Then gManualRouteSelectionList.Add(compToCheck)
                '                        End If
                '                    End If
                '                Else
                '                    If gGenerateRouteForTrainMode Then
                '                        If Not sig.HoldingSignal Then
                '                            If Not gManualRouteSelectionList.Contains(compToCheck) Then gManualRouteSelectionList.Add(compToCheck)
                '                        End If
                '                    End If
                '                End If
                '            End If

                '        Case "TMDS.TrackOtherComponents.FlipFlop"
                '            'If gSearchDirection = "LEFT" Then
                '            '    gSearchDirection = "RIGHT"
                '            'Else
                '            '    gSearchDirection = "LEFT"
                '            'End If
                '            If SearchDirection = "LEFT" Then
                '                SearchDirection = "RIGHT"
                '            Else
                '                SearchDirection = "LEFT"
                '            End If
                '    End Select
                'Next
                'TMDS.SystemForms.MySystemForms.frmMainMDI.EntExtFlasherReset()

                'Me.SystemEvent("SearchForRoute-Step4")

                'If optEnableMisrouteProtection AndAlso gMisrouteProtectionEnabled Then
                '    If Not IsNothing(gCreateAuthorityTrain) Then
                '        If Not TMDS.SystemForms.MySystemForms.frmMainMDI.DoCheckForMisroute(CurrentCompSelection) Then
                '            If Not TMDS.SystemForms.MySystemForms.frmMainMDI.GetMisRouteDialogInput() Then
                '                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                '                ClearSystemVariables()
                '                Exit Function
                '            End If
                '        End If
                '    End If
                'End If


                'If (gStackRoute OrElse gGropupedStoredRoute) AndAlso optEnableBulletinRouteCheck Then
                '    If Not TMDS.SystemForms.MySystemForms.frmMainMDI.PerformRouteStackCheckForUndeliveredBulletins(CurrentCompSelection) Then
                '        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                '        ClearSystemVariables()
                '        Exit Function
                '    End If
                'End If
                'Me.SystemEvent("SearchForRoute-Step5")

                Return True
            Catch ex As Exception

            End Try
        End Function

        Public Function ValidateRoute(ByRef CurrentCompSelection As ArrayList, ByRef enterTrack As Track, ByRef exitTrack As Track) As Boolean
            Try
                'VALIDATE THE COMPONENTS TO SEE IF THESE ARE OK TO CLEAR A ROUTE
                Dim compToCheck As Object
                Dim OverrideStoredRoute As Boolean
                Dim LastComp As Object

                Dim FlipFlopFound As Boolean
                Dim TempList As ArrayList = CurrentCompSelection.Clone
                Dim SearchDirection As String = gSearchDirection
                For Each compToCheck In TempList
                    Select Case compToCheck.GetType.ToString

                        Case "TMDS.TrackOtherComponents.RRXING"
                            If Not compToCheck.ValidateForAllClearRoute Then
                                If compToCheck.ValidateForAllStoreRoute Then
                                    Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-ValidateForAllStoreRoute")
                                    gDoStoredRoute = True
                                Else
                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                    ClearSystemVariables()
                                    Exit Function
                                End If
                            End If

                        Case gFlipFlopComponentName
                            'If gSearchDirection = "LEFT" Then
                            '    gSearchDirection = "RIGHT"
                            'Else
                            '    gSearchDirection = "LEFT"
                            'End If
                            If SearchDirection = "LEFT" Then
                                SearchDirection = "RIGHT"
                            Else
                                SearchDirection = "LEFT"
                            End If
                            If FlipFlopFound Then
                                FlipFlopFound = False
                            Else
                                FlipFlopFound = True
                            End If

                        Case gTrackComponentName
                            Dim trk As Track = CType(compToCheck, Track)
                            If compToCheck.BlockAbsolute And Not compToCheck Is gFirstEntryTrack Then
                                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                ClearSystemVariables()
                                Exit Function
                            ElseIf compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> SearchDirection) _
                            Or compToCheck.OStype And compToCheck.Occuppied Then

                                'THIS IS FOR ALLOWING LICENSES OVER OCCUPPIED OS TRACKS for MRS
                                If optEnableGeoFenceOperations Then
                                    OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(CurrentCompSelection, trk)
                                    If OverrideStoredRoute AndAlso trk.SlottingTrack Then
                                        TMDS.SystemForms.MySystemForms.frmMainMDI.DoTagSignalToIgnoreForTrainLicense(CurrentCompSelection, trk)
                                    End If
                                End If
                                'THIS IS FOR ALLOWING LICENSES OVER OCCUPPIED OS TRACKS for MRS

                                If Not OverrideStoredRoute Then
                                    gDoStoredRoute = True
                                End If
                                OverrideStoredRoute = False

                                Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track:GR:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                    gEntExtTrackSelection.Add(compToCheck)
                                End If
                            ElseIf compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then
                                'Dim trk As Track = CType(compToCheck, Track)
                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> SearchDirection) Then
                                    Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track1:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " SearchDirection:" & SearchDirection.ToString)
                                    gDoStoredRoute = True
                                End If
                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                    gEntExtTrackSelection.Add(compToCheck)
                                End If
                            Else
                                If trk.TrafficDirection <> "" And (trk.TrafficDirection <> SearchDirection) Then
                                    If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then
                                    Else
                                        Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track2:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & SearchDirection.ToString)
                                        gDoStoredRoute = True
                                    End If
                                End If


                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                    gEntExtTrackSelection.Add(compToCheck)
                                End If

                                If optEnableGeoFenceOperations Then
                                    If trk.TrackAuthorities.Count > 0 Then
                                        If IsNothing(gCreateLicenseTrain) Then gDoStoredRoute = True
                                        If Not IsNothing(gCreateLicenseTrain) Then
                                            Dim a As TrackAuthority.TrackAuthority
                                            For Each a In trk.TrackAuthorities
                                                If a.TrainSymbol <> gCreateLicenseTrain.Name Then
                                                    gDoStoredRoute = True
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            End If

                            If trk.Occuppied Then
                                If (trk.DisallowRightSignalRequestOnOccupancy AndAlso SearchDirection = "RIGHT" OrElse trk.DisallowLeftSignalRequestOnOccupancy AndAlso SearchDirection = "LEFT") Then
                                    If trk.GUID = enterTrack.GUID Then
                                        If trk.TrainIDs.Count > 0 Then
                                            'Dim trn As TrainID = enterTrack.TrainIDs(0)
                                            'If trn.Name = gCreateLicenseTrain.Name Then
                                            Me.SystemEvent("SearchForRoute-OverrideStoredRoute1-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                            OverrideStoredRoute = True
                                            'End If
                                        ElseIf trk.IsLinkToRemoteTrack Then
                                            If trk.Name = trk.myRemoteTrack.thisTrack.Name AndAlso trk.myRemoteTrack.thisTrack.TrainIDs.Count > 0 Then
                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute2-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                                OverrideStoredRoute = True
                                            End If
                                        Else
                                            If optEnableGeoFenceOperations Then
                                                OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(CurrentCompSelection, compToCheck)
                                            End If
                                        End If
                                    ElseIf trk.Name = enterTrack.Name Then
                                        If SearchDirection = "LEFT" Then
                                            If trk.RightLink = enterTrack.GUID Then
                                                OverrideStoredRoute = True
                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute5-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                            End If
                                        ElseIf SearchDirection = "LEFT" Then
                                            If trk.LeftLink = enterTrack.GUID Then
                                                OverrideStoredRoute = True
                                                Me.SystemEvent("SearchForRoute-OverrideStoredRoute6-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                            End If
                                        End If

                                    ElseIf Not IsNothing(gCreateLicenseTrain) AndAlso gCreateLicenseTrain.GeoFenceLegacyEnabled Then
                                        If trk.TrainIDs.Count > 0 Then
                                            Dim trn As TrainID = trk.TrainIDs.Item(0)
                                            If Not IsNothing(trn) Then
                                                If trn.Name = gCreateLicenseTrain.Name Then
                                                    OverrideStoredRoute = True
                                                End If
                                            End If
                                        End If
                                    End If

                                    If optEnableGeoFenceOperations AndAlso Not IsNothing(gCreateLicenseTrain) Then
                                        OverrideStoredRoute = TMDS.SystemForms.MySystemForms.frmMainMDI.IsRouteForTrainLicense(CurrentCompSelection, compToCheck)
                                    End If

                                    If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then
                                        OverrideStoredRoute = True
                                    End If

                                    If Not OverrideStoredRoute Then
                                        Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track5:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & SearchDirection.ToString)
                                        gDoStoredRoute = True
                                    Else
                                        OverrideStoredRoute = False
                                    End If
                                End If
                            End If

                            'CATENARY CHECK FOR ENERGIZED CIRCUIT
                            If optEnableTractionPowerManagementSystem Then
                                If trk.EnableTractionPowerManagement AndAlso Not IsNothing(trk.myCatenaryTrack) AndAlso Not trk.myCatenaryTrack.IsEnergized Then
                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                    ClearSystemVariables()
                                    Exit Function
                                    'gDoStoredRoute = True
                                End If
                            End If


                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(compToCheck, Switch)
                            Dim SwitchUnmoveable As Boolean
                            Dim NeedBrotherSwitchCheck As Boolean

                            If Not compToCheck.CheckBrotherSwitchesForLock(False, False) Then
                                NeedBrotherSwitchCheck = True
                            End If

                            'check to see if switch is lined for the route being requested
                            If sw.BlockAbsolute Or sw.SwitchLockBit Or NeedBrotherSwitchCheck Or sw.IndicateOnly Then
                                Dim RightLinkTrk As Track
                                Dim LeftLinkTrk As Track
                                If compToCheck.ControlledReverse Then
                                    If compToCheck.RightAltLink <> 0 Then
                                        RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightAltLink)
                                        LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftLink)
                                    Else
                                        RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightLink)
                                        LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftAltLink)
                                    End If
                                Else
                                    RightLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.RightLink)
                                    LeftLinkTrk = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(compToCheck.LeftLink)
                                End If
                                If Not CurrentCompSelection.Contains(LeftLinkTrk) Then
                                    SwitchUnmoveable = True
                                End If
                                If Not CurrentCompSelection.Contains(RightLinkTrk) Then
                                    SwitchUnmoveable = True
                                End If

                            End If


                            If SwitchUnmoveable Or _
                               compToCheck.SignalStatOnMe <> SwitchComponents.Switch.SignalStatus.SignalAtStop Then
                                ' Dim sw As Switch = CType(compToCheck, Switch)
                                Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Switch:" & sw.GUID & " SignalStatOnMe:" & sw.SignalStatOnMe)
                                Dim OverrideStoreRoute As Boolean
                                If Not IsNothing(gCreateLicenseTrain) Then
                                    If Not compToCheck.DoesSwitchHaveLicense(gCreateLicenseTrain) Then
                                        OverrideStoreRoute = True
                                    End If
                                End If
                                If Not OverrideStoreRoute Then
                                    gDoStoredRoute = True
                                Else
                                    OverrideStoreRoute = False
                                End If


                                If Not IsNothing(gCreateLicenseTrain) Then
                                    If optEnableClearSignalWithSignalIntime AndAlso sw.SignalStatOnMe = Switch.SignalStatus.SignalInTime AndAlso sw.TrafficDirection = SearchDirection Then
                                        gDoStoredRoute = False
                                    End If
                                End If
                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                    gEntExtTrackSelection.Add(compToCheck)
                                    compToCheck.SelectionMode = SwitchComponents.Switch.ModeSelection.RouteRequesting
                                    compToCheck.FlashComponent = True
                                End If

                            Else

                                If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                    gEntExtTrackSelection.Add(compToCheck)
                                    compToCheck.SelectionMode = SwitchComponents.Switch.ModeSelection.RouteRequesting
                                    compToCheck.FlashComponent = True
                                End If
                            End If
                        Case "TMDS.SignalComponents.Signal"
                            Dim sig As Signal = CType(compToCheck, Signal)

                            Dim AllowSignalSelection As Boolean = True
                            If sig.AutomaticSignal And Not optEnableLicenseGenerationForPTC Then
                                AllowSignalSelection = False
                            ElseIf sig.AutomaticSignal And optEnableLicenseGenerationForPTC Then
                                If IsNothing(gCreateLicenseTrain) Then AllowSignalSelection = False
                                'ElseIf Not FlipFlopFound AndAlso ((gSearchDirection = "LEFT" AndAlso sig.ComponentType = "R") OrElse (gSearchDirection = "RIGHT" AndAlso sig.ComponentType = "L")) Then
                                '    AllowSignalSelection = False
                            End If

                            If AllowSignalSelection Then
                                If (SearchDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                    ClearSystemVariables()
                                    Exit Function
                                End If
                                If (SearchDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                    TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                    ClearSystemVariables()
                                    Exit Function
                                End If

                                'check for fleeted signals at this control point
                                Dim comp As Object
                                For Each comp In sig.myControlPoint.AssignedComponenets
                                    If comp.GetType.ToString = "TMDS.SignalComponents.Signal" Then
                                        If comp.FleetEnabled And (sig.Name = comp.Name) Then
                                            TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                            ClearSystemVariables()
                                            gDoStoredRoute = False
                                            Exit Function

                                        ElseIf comp.FleetEnabled Then
                                            Dim sigCheck As Signal = CType(comp, Signal)
                                            If Not IsNothing(sigCheck.SignalRouteCollection) Then
                                                Dim compCheck As Object
                                                For Each compCheck In CurrentCompSelection
                                                    If sigCheck.SignalRouteCollection.Contains(compCheck) Then
                                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                                        ClearSystemVariables()
                                                        gDoStoredRoute = False
                                                        Exit Function
                                                    End If
                                                Next
                                            End If
                                        End If
                                    End If
                                Next

                                If compToCheck.SignalStatus <> SignalComponents.Signal.BulbStatus.StopStatus Then
                                    If compToCheck.SignalStatus = SignalComponents.Signal.BulbStatus.Unsolitated Then
                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                        ClearSystemVariables()
                                        Exit Function
                                    End If
                                    Dim sig1 As Signal = CType(compToCheck, Signal)
                                    Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Signal:" & sig1.GUID & "SignalStatus:" & sig1.SignalStatus)
                                    Dim OverrideStoreRoute As Boolean
                                    If Not IsNothing(gCreateLicenseTrain) Then
                                        If Not sig1.DoesSignalHaveLicense(gCreateLicenseTrain) Then
                                            OverrideStoreRoute = True
                                        End If
                                    End If
                                    If Not OverrideStoreRoute Then
                                        gDoStoredRoute = True
                                    Else
                                        OverrideStoreRoute = False
                                    End If


                                    If optEnableClearSignalWithSignalIntime AndAlso sig1.SignalStatus = Signal.BulbStatus.InTime Then
                                        gDoStoredRoute = False
                                    End If

                                    If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                    Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

                                        If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                            gEntExtTrackSelection.Add(compToCheck)
                                        End If
                                    End If

                                Else
                                    If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                    Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

                                        If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                            gEntExtTrackSelection.Add(compToCheck)
                                        End If
                                    End If
                                End If
                            Else
                                CurrentCompSelection.Remove(compToCheck)
                            End If
                    End Select
                Next
                TMDS.SystemForms.MySystemForms.frmMainMDI.tmrTrackSelectionEntExt.Enabled = True

                Me.SystemEvent("SearchForRoute-Step3")

                If gDoStoredRoute OrElse gStackRoute OrElse gGropupedStoredRoute Then
                    If gStoredRouteList Is Nothing Then
                        gStoredRouteList = New ArrayList()
                    End If
                    If optEnableLicenseGenerationForPTC Then
                        TMDS.SystemForms.MySystemForms.frmMainMDI.FilterAutomaticSignalsForStoredRoute(CurrentCompSelection, gStoredRouteList)
                    Else
                        Dim comp As Object
                        For Each comp In CurrentCompSelection
                            gStoredRouteList.Add(comp)
                        Next
                    End If
                End If

                LastComp = enterTrack

                'If FlipFlopFound Then
                '    If gSearchDirection = "LEFT" Then
                '        gSearchDirection = "RIGHT"
                '    Else
                '        gSearchDirection = "LEFT"
                '    End If
                'End If
                If FlipFlopFound Then
                    If SearchDirection = "LEFT" Then
                        SearchDirection = "RIGHT"
                    Else
                        SearchDirection = "LEFT"
                    End If
                End If

                'Add all the switches and signals in the route to the gEntExtTrackSelection arraylist
                For Each compToCheck In CurrentCompSelection
                    Select Case compToCheck.GetType.ToString

                        Case "TMDS.TrackComponents.Track"
                            If Not LastComp Is Nothing Then
                                If LastComp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                                    If LastComp.RightAltLink = compToCheck.GUID Then
                                        If Not LastComp.FlipOperations Then
                                            If Not LastComp.ControlledReverse Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        Else
                                            If LastComp.ControlledReverse Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        End If
                                    ElseIf LastComp.RightLink = compToCheck.GUID Then
                                        If Not LastComp.FlipOperations Then
                                            If LastComp.ControlledReverse And SearchDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        Else
                                            If Not LastComp.ControlledReverse And SearchDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        End If
                                    End If

                                    If LastComp.LeftAltLink = compToCheck.GUID Then
                                        If Not LastComp.FlipOperations Then
                                            If Not LastComp.ControlledReverse Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        Else
                                            If LastComp.ControlledReverse Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        End If
                                    ElseIf LastComp.LeftLink = compToCheck.GUID Then
                                        If Not LastComp.FlipOperations Then
                                            If LastComp.ControlledReverse And SearchDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If
                                        Else
                                            If Not LastComp.ControlledReverse And SearchDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
                                                AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                            End If

                                        End If

                                    End If
                                End If
                            End If
                            LastComp = compToCheck
                        Case "TMDS.SwitchComponents.Switch"
                            Dim sw As Switch = CType(compToCheck, Switch)

                            'Traveling Left through a switch from a right Alt Link turn out. 
                            'Check switch for reverve control, if not then add to list
                            If sw.TypeOfSwitch = "DERAIL" Then
                                If compToCheck.ControlledReverse Then
                                    AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                End If
                            Else
                                If compToCheck.RightAltLink = LastComp.GUID Then
                                    If Not compToCheck.FlipOperations Then
                                        If Not compToCheck.ControlledReverse Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                        End If
                                    Else
                                        If compToCheck.ControlledReverse Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                        End If
                                    End If
                                    'Traveling Left through a switch from a right Link straight track. 
                                    'Check switch for normal control, if not then add to list.
                                ElseIf compToCheck.RightLink = LastComp.GUID And compToCheck.RightAltLink <> 0 Then
                                    If Not compToCheck.FlipOperations Then
                                        If compToCheck.ControlledReverse And SearchDirection = "LEFT" Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                        End If
                                    Else
                                        If Not compToCheck.ControlledReverse And SearchDirection = "LEFT" Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                        End If
                                    End If
                                End If
                            End If

                            'Traveling Right through a switch from a Left Alt Link turn out. 
                            'Check switch for reverve control, if not then add to list
                            If compToCheck.LeftAltLink = LastComp.GUID Then
                                If Not compToCheck.FlipOperations Then
                                    If Not compToCheck.ControlledReverse Then
                                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                    End If
                                Else
                                    If compToCheck.ControlledReverse Then
                                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                    End If
                                End If
                            End If

                            'Traveling Right through a switch from a Left Link straight track
                            'Check switch for normal control, if not then add to list
                            If compToCheck.LeftLink = LastComp.GUID And compToCheck.LeftAltLink <> 0 Then
                                If Not compToCheck.FlipOperations Then
                                    If compToCheck.ControlledReverse And SearchDirection = "RIGHT" Then
                                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                    End If
                                Else
                                    If Not compToCheck.ControlledReverse And SearchDirection = "RIGHT" Then
                                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                    End If
                                End If
                            End If
                            LastComp = compToCheck

                        Case "TMDS.SignalComponents.Signal"

                            Dim sig As Signal = CType(compToCheck, Signal)
                            Dim AllowSignalSelection As Boolean = True
                            If sig.AutomaticSignal And Not optEnableLicenseGenerationForPTC Then
                                AllowSignalSelection = False
                            ElseIf sig.AutomaticSignal And optEnableLicenseGenerationForPTC Then
                                If IsNothing(gCreateLicenseTrain) Then AllowSignalSelection = False
                            End If

                            If AllowSignalSelection Then
                                If ((SearchDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                Or ((SearchDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then
                                    If Not gEntExtExecutionSelection.Contains(compToCheck) Then
                                        gEntExtExecutionSelection.Add(compToCheck)
                                        If gGenerateRouteForTrainMode AndAlso compToCheck IsNot Nothing Then
                                            If Not gManualRouteSelectionList.Contains(compToCheck) Then gManualRouteSelectionList.Add(compToCheck)
                                        End If
                                    End If
                                Else
                                    If gGenerateRouteForTrainMode Then
                                        If Not sig.HoldingSignal Then
                                            If Not gManualRouteSelectionList.Contains(compToCheck) Then gManualRouteSelectionList.Add(compToCheck)
                                        End If
                                    End If
                                End If
                            End If

                        Case "TMDS.TrackOtherComponents.FlipFlop"
                            'If gSearchDirection = "LEFT" Then
                            '    gSearchDirection = "RIGHT"
                            'Else
                            '    gSearchDirection = "LEFT"
                            'End If
                            If SearchDirection = "LEFT" Then
                                SearchDirection = "RIGHT"
                            Else
                                SearchDirection = "LEFT"
                            End If
                    End Select
                Next
                TMDS.SystemForms.MySystemForms.frmMainMDI.EntExtFlasherReset()

                Me.SystemEvent("SearchForRoute-Step4")

                If optEnableMisrouteProtection AndAlso gMisrouteProtectionEnabled Then
                    If Not IsNothing(gCreateAuthorityTrain) Then
                        If Not TMDS.SystemForms.MySystemForms.frmMainMDI.DoCheckForMisroute(CurrentCompSelection) Then
                            If Not TMDS.SystemForms.MySystemForms.frmMainMDI.GetMisRouteDialogInput() Then
                                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                ClearSystemVariables()
                                Exit Function
                            End If
                        End If
                    End If
                End If


                If (gStackRoute OrElse gGropupedStoredRoute) AndAlso optEnableBulletinRouteCheck Then
                    If Not TMDS.SystemForms.MySystemForms.frmMainMDI.PerformRouteStackCheckForUndeliveredBulletins(CurrentCompSelection) Then
                        TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                        ClearSystemVariables()
                        Exit Function
                    End If
                End If
                Me.SystemEvent("SearchForRoute-Step5")

                Return True
            Catch ex As Exception

            End Try
        End Function

        Private Sub AddSwitchToExecutionCollection(ByRef ExecuteCollection As ArrayList, ByVal swToCheck As Switch)
            Try

                'The last check is to insure that both ends of a cross-over type switch are not in the execution colleciton
                'When the collection is executed the routine will check for brother-type switches and control them accordingly
                If ExecuteCollection.Contains(swToCheck) Then Exit Sub

                If ExecuteCollection.Count = 0 Then ExecuteCollection.Add(swToCheck) : Exit Sub

                Dim comp As Object
                For Each comp In ExecuteCollection
                    If comp.GetType.ToString = "TMDS.SwitchComponents.Switch" And _
                        (comp.StationNumber = swToCheck.StationNumber) And _
                        (comp.Name = swToCheck.Name) Then
                        Exit Sub
                    End If
                Next
                ExecuteCollection.Add(swToCheck)
                If gGenerateRouteForTrainMode Then
                    gManualRouteSelectionList.Add(swToCheck)
                End If


            Catch ex As Exception
                SystemException(ex.ToString)
                ClearSystemVariables()
            End Try

        End Sub

#End Region

#Region " EVENT LOG HANDLING "

        Private Sub SystemEvent(ByVal EventNote As String)

            Try
                TMDS.SystemUtilities.Utilities.SystemEvent(EventNote)
                'ClickEvent = Nothing
            Catch ex As Exception
                ExceptionEvent(ex.ToString)
            End Try

        End Sub

        Private Sub DebugEvent(ByVal EventNote As String)

            Try
                If gDebug Then
                    SystemEvent(EventNote)
                    'ClickEvent = Nothing
                End If
            Catch ex As Exception
                ExceptionEvent(ex.ToString)
            End Try

        End Sub

        Private Sub ExceptionEvent(ByVal EventNote As String)

            Try
                SystemException(EventNote)

            Catch ex As Exception
                ExceptionEvent(ex.ToString)
            End Try

        End Sub

#End Region

    End Class

End Namespace