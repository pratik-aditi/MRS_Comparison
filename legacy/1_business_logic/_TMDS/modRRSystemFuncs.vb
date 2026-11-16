Imports TMDS.SystemForms
Imports TMDS.GraphicComponent : Imports TMDS.TrainComponents : Imports TMDS.TrackComponents : Imports TMDS.SwitchComponents : Imports TMDS.SignalComponents : Imports TMDS.TrackTagComponents : Imports TMDS.TrackOtherComponents
Imports TMDS.ManagementClasses
Imports TMDS.GlobalDeclarations
Imports TMDS.SystemUtilities.Utilities
Imports System.IO
Imports System.Data.OleDb
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports TMDS.GeneralBulletins
Imports System.Text
Imports TMDS.SystemUtilities
Imports System.Xml
Module modRRSystemFuncs

#Region " CP SPECIFIC FUNCTIONS "

    Public Sub UnderLineTextbox(ByVal tb As RichTextBox, ByVal Underline As Boolean)
        Try
            Dim strSearch As String = tb.Text
            Dim index As Integer = tb.Find(strSearch, 0, RichTextBoxFinds.WholeWord)
            If index <> -1 Then
                tb.SelectionStart = index
                tb.SelectionLength = strSearch.Length
                If Underline Then
                    tb.SelectionFont = New Font(tb.Font, FontStyle.Underline + FontStyle.Bold)
                Else
                    tb.SelectionFont = New Font(tb.Font, FontStyle.Bold)
                End If
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub HighLightTextBox(ByVal tb As RichTextBox, ByVal HighlightBox As Boolean)
        Try
            If HighlightBox Then
                tb.BackColor = gControlHighLightColor
                tb.ForeColor = gReadBackColor
            Else
                tb.BackColor = gReadBackColor
                tb.ForeColor = gReadSelectionColor
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub HighLightComboBox(ByVal tb As ComboBox, ByVal HighlightBox As Boolean)
        Try
            If HighlightBox Then
                tb.BackColor = gControlHighLightColor
                tb.ForeColor = gReadBackColor
            Else
                tb.BackColor = gReadBackColor
                tb.ForeColor = gReadSelectionColor
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Sub GetAllUnselectedTWCAuthorityForTrainReportClear(ByVal Symbol As String)
        Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim Authobj As Object = Nothing
        For Each Authobj In OtherTrackAuthorities
            If Authobj.GetType.ToString = gSFAComponentName Then
                If Authobj.IssuedToTrain And Authobj.TrainSymbol = Symbol Then
                    If Not gCPReportClearSFACollection.Contains(Authobj) Then
                        If Not gCPReportClearMissingSFAToTrainCollection.Contains(Authobj) Then gCPReportClearMissingSFAToTrainCollection.Add(Authobj)
                    End If
                End If
            End If
        Next
    End Sub

    Public Function GetFirstTWCAuthority(ByVal Symbol As String, ByVal EngineID As String, ByVal Direction As String, ByVal GetLastTWC As Boolean, ByVal FirstSFA As TMDS.TrackAuthority.SFAuthority) As TMDS.TrackAuthority.SFAuthority
        Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
        If Symbol.Trim = "" Then Return Nothing
        Dim Authobj As Object = Nothing
        Dim TrackToSearch As Track = Nothing
        Dim LinkedTrack As Object = Nothing
        Dim FirstAuthFound As Boolean = False
        Try
            For Each Authobj In OtherTrackAuthorities
                If Authobj.GetType.ToString = gSFAComponentName Then
                    If Authobj.IssuedToTrain And (Mid(Authobj.IssueTo, 1, 10) = EngineID And Authobj.TrainSymbol = Symbol) Then
                        TrackToSearch = Nothing
                        LinkedTrack = Nothing
                        If Direction.Trim = "" Then Direction = Authobj.Direction
                        If Not GetLastTWC Then
                            TrackToSearch = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(Authobj.Components)
                        Else
                            TrackToSearch = TMDS.SystemUtilities.FetchData.FindLastTrackInList(Authobj.Components)
                        End If
                        TrackToSearch = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(Authobj.Components)
                        Select Case Direction.Trim.ToUpper
                            Case "RIGHT"
                                If Not GetLastTWC Then
                                    If TrackToSearch IsNot Nothing And TrackToSearch.LeftLink > 0 Then
                                        LinkedTrack = FindCompFromGUID(TrackToSearch.LeftLink)
                                        If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                            LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                        End If
                                        If Authobj.Components.Contains(LinkedTrack) Then
                                            For i As Integer = 0 To 10
                                                LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                                If Not Authobj.Components.Contains(LinkedTrack) Then
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                        If LinkedTrack Is Nothing Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                            Dim MyAuthCount As Integer = 0
                                            Dim authToCheck As Object = Nothing
                                            For Each authToCheck In LinkedTrack.TrackAuthorities
                                                If authToCheck.GetType.ToString = gSFAComponentName Then
                                                    If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                        MyAuthCount = MyAuthCount + 1
                                                    End If
                                                End If
                                            Next
                                            If MyAuthCount = 0 Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                            End If
                                        Else
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        End If
                                    End If
                                Else
                                    If TrackToSearch IsNot Nothing And TrackToSearch.RightLink > 0 Then
                                        LinkedTrack = FindCompFromGUID(TrackToSearch.RightLink)
                                        If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                            LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                        End If
                                        If Authobj.Components.Contains(LinkedTrack) Then
                                            For i As Integer = 0 To 10
                                                LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                                If Not Authobj.Components.Contains(LinkedTrack) Then
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                        If LinkedTrack Is Nothing Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                            Dim MyAuthCount As Integer = 0
                                            Dim authToCheck As Object = Nothing
                                            For Each authToCheck In LinkedTrack.TrackAuthorities
                                                If authToCheck.GetType.ToString = gSFAComponentName Then
                                                    If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                        MyAuthCount = MyAuthCount + 1
                                                    End If
                                                End If
                                            Next
                                            If MyAuthCount = 0 Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                            End If
                                        Else
                                            If Not FirstSFA Is Nothing AndAlso Not FirstSFA Is Authobj Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                                'Else
                                                '    FirstAuthFound = True
                                                '    SFA = Authobj
                                                '    Exit For
                                            End If

                                        End If
                                    End If
                                End If
                            Case "LEFT"
                                If Not GetLastTWC Then
                                    If TrackToSearch IsNot Nothing And TrackToSearch.RightLink > 0 Then
                                        LinkedTrack = FindCompFromGUID(TrackToSearch.RightLink)
                                        If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                            LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                        End If
                                        If Authobj.Components.Contains(LinkedTrack) Then
                                            For i As Integer = 0 To 10
                                                LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                                If Not Authobj.Components.Contains(LinkedTrack) Then
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                        If LinkedTrack Is Nothing Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                            Dim MyAuthCount As Integer = 0
                                            Dim authToCheck As Object = Nothing
                                            For Each authToCheck In LinkedTrack.TrackAuthorities
                                                If authToCheck.GetType.ToString = gSFAComponentName Then
                                                    If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                        MyAuthCount = MyAuthCount + 1
                                                    End If
                                                End If
                                            Next
                                            If MyAuthCount = 0 Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                            End If
                                        Else
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        End If
                                    End If
                                Else
                                    If TrackToSearch IsNot Nothing And TrackToSearch.LeftLink > 0 Then
                                        LinkedTrack = FindCompFromGUID(TrackToSearch.LeftLink)
                                        If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                            LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                        End If
                                        If Authobj.Components.Contains(LinkedTrack) Then
                                            For i As Integer = 0 To 10
                                                LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                                If Not Authobj.Components.Contains(LinkedTrack) Then
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                        If LinkedTrack Is Nothing Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                            FirstAuthFound = True
                                            SFA = Authobj
                                            Exit For
                                        ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                            Dim MyAuthCount As Integer = 0
                                            Dim authToCheck As Object = Nothing
                                            For Each authToCheck In LinkedTrack.TrackAuthorities
                                                If authToCheck.GetType.ToString = gSFAComponentName Then
                                                    If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                        MyAuthCount = MyAuthCount + 1
                                                    End If
                                                End If
                                            Next
                                            If MyAuthCount = 0 Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                            End If
                                        Else
                                            'FirstAuthFound = True
                                            'SFA = Authobj
                                            'Exit For
                                            If Not FirstSFA Is Nothing AndAlso Not FirstSFA Is Authobj Then
                                                FirstAuthFound = True
                                                SFA = Authobj
                                                Exit For
                                                'Else
                                                '    FirstAuthFound = True
                                                '    SFA = Authobj
                                                '    Exit For
                                            End If
                                        End If
                                    End If
                                End If
                        End Select
                    End If
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally

        End Try

        Return SFA

    End Function

    Public Function GetNextTWCAuthority(ByVal CurrentSFA As TMDS.TrackAuthority.SFAuthority, ByVal Symbol As String, ByVal EngineID As String, ByVal Direction As String, ByVal SearchFromLastComponent As Boolean) As TMDS.TrackAuthority.SFAuthority
        Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
        If Symbol.Trim = "" Then Return Nothing
        Dim Authobj As Object = CurrentSFA
        Dim TrackToSearch As Track = Nothing
        Dim LinkedTrack As Object = Nothing
        Dim boxArr As New ArrayList
        Dim LikeTracksArray As New ArrayList
        Dim Comp As Object = Nothing
        Dim OSSwitch As Switch = Nothing
        Dim NextComp As Object = Nothing
        Try


            If Authobj.IssuedToTrain And (Mid(Authobj.IssueTo, 1, 10) = EngineID And Authobj.TrainSymbol = Symbol) Then
                TrackToSearch = Nothing
                LinkedTrack = Nothing
                If Direction.Trim = "" Then Direction = Authobj.Direction
                If Not SearchFromLastComponent Then
                    TrackToSearch = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(Authobj.Components)
                Else
                    TrackToSearch = TMDS.SystemUtilities.FetchData.FindLastTrackInList(Authobj.Components)

                    boxArr = GetCPBoxesChecked(CurrentSFA)
                    If boxArr.Contains(10) Then
                        TrackToSearch = GetNextValidTrackForBox10Auth(CurrentSFA)
                    Else
                        If TrackToSearch IsNot Nothing AndAlso TrackToSearch.OsComponentCollection IsNot Nothing AndAlso TrackToSearch.OsComponentCollection.Count > 0 Then
                            OSSwitch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(TrackToSearch)
                            If OSSwitch IsNot Nothing Then
                                Select Case Direction.Trim.ToUpper
                                    Case "RIGHT"
                                        TrackToSearch = FindCompFromGUID(OSSwitch.RightLink)
                                    Case "LEFT"
                                        TrackToSearch = FindCompFromGUID(OSSwitch.LeftLink)
                                End Select
                            End If
                        End If
                    End If
                End If
                'If Not TrackToSearch Is Nothing Then TrackToSearch.FindLikeTracks(LikeTracksArray, TMDS.GlobalDeclarations.TrackCollection)
                'If LikeTracksArray.Count > 1 Then
                '    Dim LastTrack As Object = Nothing
                '    Dim LastObject As Object = Nothing
                '    Select Case Direction.Trim.ToUpper
                '        Case "RIGHT"
                '            For Each Comp In LikeTracksArray
                '                LastObject = FindCompFromGUID(Comp.RightLink)
                '                For i As Integer = 0 To 20
                '                    If LastObject IsNot Nothing Then
                '                        If LastObject.GetType.ToString = gTrackComponentName Then
                '                            If Authobj.Components.contains(LastObject) Then
                '                                LastObject = FindCompFromGUID(LastObject.RightLink)
                '                            Else
                '                                LastTrack = LastObject
                '                                Exit For
                '                            End If
                '                        Else
                '                            LastObject = FindCompFromGUID(LastObject.RightLink)
                '                        End If
                '                    End If
                '                Next
                '                If Not LastTrack Is Nothing Then
                '                    TrackToSearch = LastTrack
                '                    Exit For
                '                End If
                '            Next
                '        Case "LEFT"
                '    End Select
                'End If

                Select Case Direction.Trim.ToUpper
                    Case "RIGHT"
                        If Not SearchFromLastComponent Then
                            If TrackToSearch IsNot Nothing And TrackToSearch.LeftLink > 0 Then
                                LinkedTrack = FindCompFromGUID(TrackToSearch.LeftLink)

                                If LinkedTrack Is Nothing Then Return Nothing
                                Select Case LinkedTrack.GetType.ToString
                                    Case gSignalComponentName
                                        NextComp = FindCompFromGUID(LinkedTrack.LeftLink)
                                        For I As Integer = 0 To TrackCollection.Count
                                            If NextComp Is Nothing Then Return Nothing
                                            If NextComp.GetType.ToString = gTrackComponentName Then
                                                If NextComp.AuthorityType.ToString.Trim.ToUpper.Contains("TWC") Then
                                                    LinkedTrack = NextComp
                                                    Exit For
                                                Else
                                                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                                            End If
                                        Next
                                End Select

                                If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                    LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                End If
                                If Authobj.Components.Contains(LinkedTrack) Then
                                    For i As Integer = 0 To 10
                                        LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                        If Not Authobj.Components.Contains(LinkedTrack) Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                If LinkedTrack Is Nothing Then
                                ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                    Dim MyAuthCount As Integer = 0
                                    Dim authToCheck As Object = Nothing
                                    For Each authToCheck In LinkedTrack.TrackAuthorities
                                        If authToCheck.GetType.ToString = gSFAComponentName Then
                                            If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                SFA = authToCheck
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Else
                            If TrackToSearch IsNot Nothing And TrackToSearch.RightLink > 0 Then
                                LinkedTrack = FindCompFromGUID(TrackToSearch.RightLink)

                                If LinkedTrack Is Nothing Then Return Nothing
                                Select Case LinkedTrack.GetType.ToString
                                    Case gSignalComponentName
                                        NextComp = FindCompFromGUID(LinkedTrack.RightLink)
                                        For I As Integer = 0 To TrackCollection.Count
                                            If NextComp Is Nothing Then Return Nothing
                                            If NextComp.GetType.ToString = gTrackComponentName Then
                                                If NextComp.AuthorityType.ToString.Trim.ToUpper.Contains("TWC") Then
                                                    LinkedTrack = NextComp
                                                    Exit For
                                                Else
                                                    NextComp = FindCompFromGUID(NextComp.RightLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(NextComp.RightLink)
                                            End If
                                        Next
                                End Select

                                If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                    LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                End If
                                If Authobj.Components.Contains(LinkedTrack) Then
                                    For i As Integer = 0 To 10
                                        LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                        If Not Authobj.Components.Contains(LinkedTrack) Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                If LinkedTrack Is Nothing Then
                                ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                    Dim MyAuthCount As Integer = 0
                                    Dim authToCheck As Object = Nothing
                                    For Each authToCheck In LinkedTrack.TrackAuthorities
                                        If authToCheck.GetType.ToString = gSFAComponentName Then
                                            If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                SFA = authToCheck
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Case "LEFT"
                        If Not SearchFromLastComponent Then
                            If TrackToSearch IsNot Nothing And TrackToSearch.RightLink > 0 Then
                                LinkedTrack = FindCompFromGUID(TrackToSearch.RightLink)

                                If LinkedTrack Is Nothing Then Return Nothing
                                Select Case LinkedTrack.GetType.ToString
                                    Case gSignalComponentName
                                        NextComp = FindCompFromGUID(LinkedTrack.RightLink)
                                        For I As Integer = 0 To TrackCollection.Count
                                            If NextComp Is Nothing Then Return Nothing
                                            If NextComp.GetType.ToString = gTrackComponentName Then
                                                If NextComp.AuthorityType.ToString.Trim.ToUpper.Contains("TWC") Then
                                                    LinkedTrack = NextComp
                                                    Exit For
                                                Else
                                                    NextComp = FindCompFromGUID(NextComp.RightLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(NextComp.RightLink)
                                            End If
                                        Next
                                End Select

                                If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                    LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                End If
                                If Authobj.Components.Contains(LinkedTrack) Then
                                    For i As Integer = 0 To 10
                                        LinkedTrack = FindCompFromGUID(LinkedTrack.RightLink)
                                        If Not Authobj.Components.Contains(LinkedTrack) Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                If LinkedTrack Is Nothing Then
                                ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                    Dim MyAuthCount As Integer = 0
                                    Dim authToCheck As Object = Nothing
                                    For Each authToCheck In LinkedTrack.TrackAuthorities
                                        If authToCheck.GetType.ToString = gSFAComponentName Then
                                            If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                SFA = authToCheck
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Else
                            If TrackToSearch IsNot Nothing And TrackToSearch.LeftLink > 0 Then
                                LinkedTrack = FindCompFromGUID(TrackToSearch.LeftLink)

                                If LinkedTrack Is Nothing Then Return Nothing
                                Select Case LinkedTrack.GetType.ToString
                                    Case gSignalComponentName
                                        NextComp = FindCompFromGUID(LinkedTrack.LeftLink)
                                        For I As Integer = 0 To TrackCollection.Count
                                            If NextComp Is Nothing Then Return Nothing
                                            If NextComp.GetType.ToString = gTrackComponentName Then
                                                If NextComp.AuthorityType.ToString.Trim.ToUpper.Contains("TWC") Then
                                                    LinkedTrack = NextComp
                                                    Exit For
                                                Else
                                                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                                                End If
                                            Else
                                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                                            End If
                                        Next
                                End Select

                                If LinkedTrack.GetType.ToString = gSwitchComponentName Then
                                    LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                End If
                                If Authobj.Components.Contains(LinkedTrack) Then
                                    For i As Integer = 0 To 10
                                        LinkedTrack = FindCompFromGUID(LinkedTrack.LeftLink)
                                        If Not Authobj.Components.Contains(LinkedTrack) Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                If LinkedTrack Is Nothing Then
                                ElseIf Mid(LinkedTrack.AuthorityType, 1, 3) <> "TWC" Then
                                ElseIf LinkedTrack.TrackAuthorities IsNot Nothing AndAlso LinkedTrack.TrackAuthorities.Count > 0 Then
                                    Dim MyAuthCount As Integer = 0
                                    Dim authToCheck As Object = Nothing
                                    For Each authToCheck In LinkedTrack.TrackAuthorities
                                        If authToCheck.GetType.ToString = gSFAComponentName Then
                                            If authToCheck.IssuedToTrain And (authToCheck.TrainSymbol = Symbol) Then
                                                SFA = authToCheck
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                End Select
            End If


        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally

        End Try

        Return SFA

    End Function

    Public Sub PopulatedForemanListDropDown(ByRef ForemanDropDown As ComboBox, Optional ByVal IssueTo As String = "", Optional ByVal ShowEmployeeUID As Boolean = False)
        Dim fmn As Foreman
        Dim cmbTxtStr As String = ""
        Try
            ForemanDropDown.Items.Clear()
            For Each fmn In gMowForemen 'MOWCollection
                If Not ShowEmployeeUID Then
                    If fmn.Name.Trim <> "" Then
                        ForemanDropDown.Items.Add(fmn.Name)
                    End If
                Else

                    If Not String.IsNullOrEmpty(fmn.EmployeeUID) Then
                        ForemanDropDown.Items.Add(fmn.EmployeeUID)
                    End If
                End If
            Next
            If Not ForemanDropDown.Items.Contains(IssueTo) AndAlso IssueTo.Trim <> "" Then
                ForemanDropDown.Items.Insert(0, IssueTo)
                'If ForemanDropDown.DropDownStyle = ComboBoxStyle.DropDown Then
                '    ForemanDropDown.Text = IssueTo
                'End If
            ElseIf ForemanDropDown.Items.Contains(IssueTo) AndAlso IssueTo.Trim <> "" Then
                ForemanDropDown.Text = IssueTo
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub PopulatedTrainCrewDropDown(ByRef TrainCrewNameDropDown As ComboBox, ByVal dvTrainCrewName As DataView, ByVal CraftName As String)
        Dim dv As New DataView
        Dim FirstName As String = ""
        Dim dt As New DataTable
        Try
            TrainCrewNameDropDown.Items.Clear()
            If dvTrainCrewName IsNot Nothing AndAlso dvTrainCrewName.Count > 0 Then
                dt = dvTrainCrewName.Table
                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    dv = New DataView(dt)
                    dv.RowFilter = ""
                    dv.RowFilter = "Craft ='" & CraftName.Trim.ToUpper & "'"
                    dv.Sort = "Name"
                    If dv.Count > 0 Then
                        For i As Integer = 0 To dv.Count - 1
                            If FirstName.Trim = String.Empty Then FirstName = dv(i)("Name")
                            TrainCrewNameDropDown.Items.Add(dv(i)("Name"))
                        Next
                    End If
                    TrainCrewNameDropDown.Text = FirstName.Trim.ToUpper
                End If
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub PopulatedTrainCraftDropDown(ByRef TrainCrewCraftDropDown As ComboBox, Optional ByVal DefaultCraftName As String = "")
        Dim s As String = ""
        Try
            TrainCrewCraftDropDown.Items.Clear()
            If gTrainCrewCrafts IsNot Nothing AndAlso gTrainCrewCrafts.Count > 0 Then
                For Each s In gTrainCrewCrafts
                    TrainCrewCraftDropDown.Items.Add(s.Trim.ToUpper)
                Next
            Else
                With TrainCrewCraftDropDown.Items
                    .Add("ENGINEER")
                    .Add("CONDUCTOR")
                    .Add("ENGR")
                    .Add("ASST ENGR")
                    .Add("2ND ENGR")
                    .Add("ENGR TRNEE")
                    .Add("CONDR")
                    .Add("CONDR TRNEE")
                    .Add("TRNM")
                    .Add("TRNM TRNEE")
                End With
            End If
            If DefaultCraftName.Trim <> "" Then
                TrainCrewCraftDropDown.Text = DefaultCraftName.Trim.ToUpper
            Else
                TrainCrewCraftDropDown.Text = gCPDefaultCraft.Trim.ToUpper
            End If


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Function GetCPBoxesChecked(ByVal sfa As TMDS.TrackAuthority.SFAuthority) As ArrayList
        Dim BoxesChecked As New ArrayList
        Dim s As String = ""
        Try
            Dim boxStr As String = sfa.BoxItems.Replace("[", "")
            boxStr = boxStr.Replace("]", "")
            s = ""
            Dim boxItms() As String
            boxItms = boxStr.Split(",")
            For Each s In boxItms
                If s.Trim <> "" And Integer.TryParse(s.Trim, Nothing) Then
                    BoxesChecked.Add(CInt(s.Trim))
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return BoxesChecked
    End Function

    Public Sub ResetCompToCodeBrown(ByVal ComponentsInSelection As ArrayList)
        Dim comp As Object = Nothing
        Try
            If optEnableCTCTAGDispalyonCodeBrown Then
                For Each comp In ComponentsInSelection
                    Dim OKtoReset As Boolean = True
                    If comp.FailState Then
                        Select Case comp.GetType.ToString
                            Case gSignalComponentName
                                If OKtoReset Then
                                    comp.OverRideCodeBrown = False
                                    comp.UpdateStatus()
                                End If
                            Case gTrackComponentName, gSwitchComponentName
                                If comp.OtherSplAuthorities IsNot Nothing AndAlso comp.OtherSplAuthorities.Count > 0 Then
                                    OKtoReset = False
                                ElseIf comp.TrackAuthorities IsNot Nothing AndAlso comp.TrackAuthorities.Count > 0 Then
                                    OKtoReset = False
                                ElseIf comp.BlockAbsolute = True Or comp.DeviceBlocked = True Then
                                    OKtoReset = False
                                End If
                                If OKtoReset Then
                                    comp.OverRideCodeBrown = False
                                    comp.UpdateStatus()
                                End If
                        End Select
                    End If
                Next
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            comp = Nothing
        End Try

    End Sub

#Region "CP READ REPEAT CLASS "

    Public Class LineItemsControlCollection

        Public LineNumber As Integer
        Public Read As Boolean = False
        Public ControllingObjectName As String = ""
        Public ControlsCollection As New ArrayList

        Public Sub CleanMe()
            Try
                Me.ControlsCollection.Clear()
                Me.ControlsCollection = New ArrayList
            Catch ex As Exception
            End Try
        End Sub

    End Class

#End Region

    Public Function LoadStationsWithinTGBOLimits(ByVal TGBOLimitName As String) As ArrayList
        Try
            Dim selTGBOLimit As BulletinCell = Nothing
            Dim distincStationList As New ArrayList
            Dim bc As BulletinCell
            Dim subdiv As Subdivision = Nothing
            Dim stat As SubdivisionStation

            For Each bc In gBulletinCellRouteList
                If bc.myName = TGBOLimitName Then
                    selTGBOLimit = bc
                    Exit For
                End If
            Next

            If Not IsNothing(selTGBOLimit) Then
                Dim mr As MileageRange
                For Each mr In selTGBOLimit.myMileageRangeList
                    If gDebug Then SystemEvent(" My TGBO Name: " & selTGBOLimit.myName & " MileageRange Name: " & mr.myName & " MR RightMileage: " & mr.RightMileage & " MR LeftMileage: " & mr.LeftMileage)
                    subdiv = SystemUtilities.FetchData.FindSubdivisionFromUID(mr.mySubdivisionUID)
                    If Not IsNothing(subdiv) Then
                        For Each stat In subdiv.Stations
                            If IsNumeric(stat.MilePost) Then
                                Dim statMP As Double = CDbl(stat.MilePost)
                                If (statMP >= mr.LeftMileage And statMP <= mr.RightMileage) Or (statMP >= mr.RightMileage And statMP <= mr.LeftMileage) Then
                                    If Not distincStationList.Contains(stat) Then
                                        If Not stat.MilePostMarker Then
                                            distincStationList.Add(stat)
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next
            End If
            Return distincStationList
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function LoadStationsByControlledSub() As ArrayList
        Dim Subdvn As Subdivision
        Dim stat As SubdivisionStation
        Dim StList As New ArrayList
        Try
            For Each Subdvn In ControlledSubdivisions
                For Each stat In Subdvn.Stations
                    If Not stat.MilePostMarker Then
                        StList.Add(stat)
                    End If
                Next
            Next
            StList.Sort()
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
        Return StList
    End Function

    Public Function LoadSubdivisionWithinTGBOLimits(ByVal TGBOLimitName As String) As ArrayList
        Try
            Dim selTGBOLimit As BulletinCell = Nothing
            Dim distincSubList As New ArrayList
            Dim bc As BulletinCell
            Dim subdiv As Subdivision = Nothing

            For Each bc In gBulletinCellRouteList
                If bc.myName = TGBOLimitName Then
                    selTGBOLimit = bc
                    Exit For
                End If
            Next

            If Not IsNothing(selTGBOLimit) Then
                Dim mr As MileageRange
                For Each mr In selTGBOLimit.myMileageRangeList
                    subdiv = SystemUtilities.FetchData.FindSubdivisionFromUID(mr.mySubdivisionUID)
                    If Not IsNothing(subdiv) Then
                        If Not distincSubList.Contains(subdiv) Then
                            distincSubList.Add(subdiv)
                        End If
                    End If
                Next
            End If
            Return distincSubList
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Function UpdateCPCrewFromAuthority(ByVal trn As TrainID, ByVal CraftName As String, ByVal Name As String) As Boolean
        Dim cwprsn As CrewPerson = Nothing
        Dim CrewPersonExists As Boolean = False
        Try
            If trn.CrewCurrent.CrewList.Count > 0 Then
                Dim cw As CrewPerson
                For Each cw In trn.CrewCurrent.CrewList
                    If cw.NameOfPerson.Trim.ToUpper = Name.Trim.ToUpper Then
                        CrewPersonExists = True
                        Exit For
                    End If
                Next
            End If
            If Not CrewPersonExists Then
                cwprsn = New CrewPerson(CraftName)
                cwprsn.NameOfPerson = Name.Trim.ToUpper
                trn.CreateCrewEdits(cwprsn)
                Dim AlarmMessage As String = "CREW MISMATCH WITH TRAIN: " & trn.Name & " NEW CREW INFO: " & CraftName.Trim.ToUpper & Space(1) & Name.Trim.ToUpper
                TMDS.ManagementClasses.AlertUtilities.CreateAlert(ManagementClasses.Alert.AlertType.TrainCrewChange, AlarmMessage)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Public Function IsOKTOReleaseTrain(ByVal trn As TrainID) As Boolean
        Dim OkToRelease As Boolean = True
        Dim auth As Object = Nothing
        Try

            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            Return False
                        End If
                    End If
                Next
            End If
            auth = Nothing
            If gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            Return False
                        End If
                    End If
                Next
            End If

            If optEnableCPTrainsheet Then
                If trn.TGBONumber <> String.Empty Then
                    Select Case trn.TGBOStatus.Trim.ToUpper
                        Case "", "CANCELLED", "RELEASED"
                        Case Else
                            Return False
                    End Select
                End If
            End If

            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If auth IsNot Nothing AndAlso auth.GetType.ToString = gTTComponentName Then
                        If auth.AuthorityType = "TT" Then
                            If Not auth.BehindTrainsInfo Is Nothing AndAlso auth.BehindTrainsInfo.Trim <> "" Then
                                If auth.BehindTrainsInfo.Contains(trn.IdentifingUnit) Then
                                    Return False
                                End If
                            ElseIf auth.JointAuthority AndAlso auth.JointVerbiage.Trim <> "" Then
                                If auth.JointVerbiage.Trim.ToUpper.Contains(trn.IdentifingUnit) Then
                                    Return False
                                End If
                            End If
                        End If
                    End If
                Next
            End If

            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If auth IsNot Nothing AndAlso auth.GetType.ToString = gSFAComponentName Then
                        If auth.Box5Arrival IsNot Nothing AndAlso auth.Box5Arrival.Count > 0 Then
                            Dim s As String = ""
                            For Each s In auth.Box5Arrival
                                If s.Trim.ToUpper.Contains(trn.IdentifingUnit.Trim.ToUpper) Then Return False
                            Next
                        End If
                    End If
                Next
            End If



        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
        Return OkToRelease
    End Function

    Public Function GetFirstTWCAuthority(ByVal Symbol As String) As TMDS.TrackAuthority.SFAuthority
        Dim FirstSfa As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim Auth As Object = Nothing
        Dim ThisTrainsAuthorities As New ArrayList
        Try
            For Each Auth In OtherTrackAuthorities
                If Auth.GetType.ToString = gSFAComponentName Then
                    If Auth.TrainSymbol.Trim.ToUpper = Symbol.Trim.ToUpper Then
                        If Not ThisTrainsAuthorities.Contains(Auth) Then ThisTrainsAuthorities.Add(Auth)
                    End If
                End If
            Next
            If ThisTrainsAuthorities Is Nothing Then Return Nothing
            If ThisTrainsAuthorities.Count = 0 Then Return Nothing
            If ThisTrainsAuthorities.Count = 1 Then Return ThisTrainsAuthorities(0)
            For i As Integer = 0 To ThisTrainsAuthorities.Count + 1
                For Each SFA In ThisTrainsAuthorities
                    If FirstSfa Is Nothing Then
                        FirstSfa = SFA
                    Else
                        If FirstSfa.AuthDate > SFA.AuthDate Then
                            FirstSfa = SFA
                        End If
                    End If
                Next
            Next
            Return FirstSfa
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally
            SFA = Nothing
            Auth = Nothing
            ThisTrainsAuthorities = Nothing
        End Try
        Return Nothing
    End Function

    Public Function IsCPEntryTrackValidForTWCIssue(ByVal EntryTrack As Track, ByVal LastSFA As TMDS.TrackAuthority.SFAuthority) As Boolean
        Dim comp As Object = Nothing
        Dim sw As Switch = Nothing
        Dim LastTrack As Track = Nothing
        'Dim CurrentNewestAuthority As TMDS.TrackAuthority.SFAuthority = Nothing
        'Dim auth As Object = Nothing
        Dim BoxArr As New ArrayList
        Dim AltTrack As Track = Nothing
        Dim NextComp As Object = Nothing
        Dim D As String = ""
        Try
            LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(LastSFA.Components)
            If LastTrack Is Nothing Then Return False
            If EntryTrack.OsComponentCollection Is Nothing Then Return False
            If EntryTrack.OsComponentCollection.Count = 0 Then Return False

            If LastTrack.OsComponentCollection IsNot Nothing AndAlso LastTrack.OsComponentCollection.Count > 0 Then
                For Each comp In LastTrack.OsComponentCollection
                    If comp.GetType.ToString = gSwitchComponentName Then
                        sw = comp
                        Exit For
                    End If
                Next
                BoxArr = GetCPBoxesChecked(LastSFA)
                If BoxArr.Contains(10) Then
                    If sw.RightAltLink > 0 Then
                        AltTrack = FindCompFromGUID(sw.RightAltLink)
                        D = "R"
                    ElseIf sw.LeftAltLink > 0 Then
                        AltTrack = FindCompFromGUID(sw.LeftAltLink)
                        D = "L"
                    End If
                    If Not AltTrack Is Nothing Then
                        For i As Integer = 0 To 5
                            Select Case D
                                Case "R"
                                    comp = FindCompFromGUID(AltTrack.RightLink)
                                    If Not comp Is Nothing Then
                                        If sw.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        ElseIf EntryTrack.OsComponentCollection.Contains(comp) Then
                                            Return True
                                        Else
                                            AltTrack = comp
                                        End If
                                    End If
                                Case "L"
                                    comp = FindCompFromGUID(AltTrack.LeftLink)
                                    If Not comp Is Nothing Then
                                        If sw.OsComponentCollection.Contains(comp) Then
                                            AltTrack = comp
                                        ElseIf EntryTrack.OsComponentCollection.Contains(comp) Then
                                            Return True
                                        Else
                                            AltTrack = comp
                                        End If
                                    End If
                            End Select
                        Next
                    End If
                    'Else
                    '    If sw.RightAltLink > 0 Then
                    '        AltTrack = FindCompFromGUID(sw.RightLink)
                    '        D = "R"
                    '    ElseIf sw.LeftAltLink > 0 Then
                    '        AltTrack = FindCompFromGUID(sw.LeftLink)
                    '        D = "L"
                    '    End If
                    '    If Not AltTrack Is Nothing Then
                    '        For i As Integer = 0 To 5
                    '            Select Case D
                    '                Case "R"
                    '                    comp = FindCompFromGUID(AltTrack.RightLink)
                    '                    If Not comp Is Nothing Then
                    '                        If sw.OsComponentCollection.Contains(comp) Then
                    '                            AltTrack = comp
                    '                        Else
                    '                            LastTrack = AltTrack
                    '                            Exit For
                    '                        End If
                    '                    End If
                    '                Case "L"
                    '                    comp = FindCompFromGUID(AltTrack.LeftLink)
                    '                    If Not comp Is Nothing Then
                    '                        If sw.OsComponentCollection.Contains(comp) Then
                    '                            AltTrack = comp
                    '                        Else
                    '                            LastTrack = AltTrack
                    '                            Exit For
                    '                        End If
                    '                    End If
                    '            End Select
                    '        Next
                    '    End If
                End If
            End If

            'If EntryTrack.OsComponentCollection.Contains(LastTrack) Then Return True
            Return False
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

        Return False
    End Function

    Public Function GetNextValidTrackForBox10Auth(ByVal LastSFA As TMDS.TrackAuthority.SFAuthority) As Track
        Dim comp As Object = Nothing
        Dim sw As Switch = Nothing
        Dim LastTrack As Track = Nothing
        'Dim CurrentNewestAuthority As TMDS.TrackAuthority.SFAuthority = Nothing
        'Dim auth As Object = Nothing
        Dim BoxArr As New ArrayList
        Dim AltTrack As Track = Nothing
        Dim NextComp As Object = Nothing
        Dim D As String = ""
        Dim SidingDetected As Boolean = False
        Try
            LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(LastSFA.Components)
            If LastTrack Is Nothing Then Return Nothing
            If LastTrack.OsComponentCollection IsNot Nothing AndAlso LastTrack.OsComponentCollection.Count > 0 Then
                For Each comp In LastTrack.OsComponentCollection
                    If comp.GetType.ToString = gSwitchComponentName Then
                        sw = comp
                        Exit For
                    End If
                Next
                BoxArr = GetCPBoxesChecked(LastSFA)
                If BoxArr.Contains(10) Then
                    If sw.RightAltLink > 0 Then
                        AltTrack = FindCompFromGUID(sw.RightAltLink)
                        D = "R"
                    ElseIf sw.LeftAltLink > 0 Then
                        AltTrack = FindCompFromGUID(sw.LeftAltLink)
                        D = "L"
                    End If
                    If Not AltTrack Is Nothing Then
                        For i As Integer = 0 To 5
                            Select Case D
                                Case "R"
                                    comp = FindCompFromGUID(AltTrack.RightLink)
                                    If Not comp Is Nothing Then
                                        If comp.GetType.ToString = gTrackComponentName Then
                                            If optEnableTrackSidingBit Then
                                                'NEW SIDING LOGIC CODE
                                                Dim IsSidingTrack As Boolean = False
                                                If comp.EnableNonControlledSiding Or comp.EnableControlledSiding Then IsSidingTrack = True
                                                If sw.OsComponentCollection.Contains(comp) Then
                                                    AltTrack = comp
                                                ElseIf comp.AuthorityType = "NON-TWC" Or IsSidingTrack Then
                                                    SidingDetected = True
                                                    AltTrack = comp
                                                ElseIf SidingDetected AndAlso Mid(comp.AuthorityType, 1, 3) = "TWC" Then
                                                    Return comp
                                                Else
                                                    AltTrack = comp
                                                End If
                                            Else
                                                'OLD LEGACY SIDING CODE
                                                If sw.OsComponentCollection.Contains(comp) Then
                                                    AltTrack = comp
                                                ElseIf comp.AuthorityType = "NON-TWC" Or comp.TrackName = "SIDING" Then
                                                    SidingDetected = True
                                                    AltTrack = comp
                                                ElseIf SidingDetected AndAlso Mid(comp.AuthorityType, 1, 3) = "TWC" Then
                                                    Return comp
                                                Else
                                                    AltTrack = comp
                                                End If
                                            End If


                                        Else
                                            AltTrack = comp
                                        End If
                                    End If
                                Case "L"
                                    comp = FindCompFromGUID(AltTrack.LeftLink)
                                    If Not comp Is Nothing Then
                                        If comp.GetType.ToString = gTrackComponentName Then
                                            If optEnableTrackSidingBit Then
                                                'NEW SIDING LOGIC CODE
                                                Dim IsSidingTrack As Boolean = False
                                                If comp.EnableNonControlledSiding Or comp.EnableControlledSiding Then IsSidingTrack = True
                                                If sw.OsComponentCollection.Contains(comp) Then
                                                    AltTrack = comp
                                                ElseIf comp.AuthorityType = "NON-TWC" Or IsSidingTrack Then
                                                    SidingDetected = True
                                                    AltTrack = comp
                                                ElseIf SidingDetected AndAlso Mid(comp.AuthorityType, 1, 3) = "TWC" Then
                                                    Return comp
                                                Else
                                                    AltTrack = comp
                                                End If

                                            Else
                                                'OLD LEGACY SIDING CODE
                                                If sw.OsComponentCollection.Contains(comp) Then
                                                    AltTrack = comp
                                                ElseIf comp.AuthorityType = "NON-TWC" Or comp.TrackName = "SIDING" Then
                                                    SidingDetected = True
                                                    AltTrack = comp
                                                ElseIf SidingDetected AndAlso Mid(comp.AuthorityType, 1, 3) = "TWC" Then
                                                    Return comp
                                                Else
                                                    AltTrack = comp
                                                End If
                                            End If

                                        Else
                                            AltTrack = comp
                                        End If
                                    End If
                            End Select
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

    Public Function GetAllLimitsFromTheCollection(ByVal ComponentsCollection As ArrayList) As ArrayList
        Dim LimitsCollection As New ArrayList
        Dim Comp As Object = Nothing
        Try
            If Not ComponentsCollection Is Nothing AndAlso ComponentsCollection.Count > 0 Then
                For Each Comp In ComponentsCollection
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If Not LimitsCollection.Contains(Comp.RightLimitName.Trim.ToUpper) Then LimitsCollection.Add(Comp.RightLimitName.Trim.ToUpper)
                        If Not LimitsCollection.Contains(Comp.LeftLimitName.Trim.ToUpper) Then LimitsCollection.Add(Comp.LeftLimitName.Trim.ToUpper)
                    End If
                Next
            End If
            Return LimitsCollection
        Catch ex As Exception
        Finally
            Comp = Nothing
            LimitsCollection = Nothing
        End Try
        Return Nothing
    End Function

    Public Function ValidateTGBOLimits(ByVal trn As TrainID, ByVal Complist As ArrayList) As Boolean
        Dim TrackList As New ArrayList
        Dim AD As AuthorityDesignation = Nothing
        Dim BCCollection As New ArrayList
        Dim BC As BulletinCell = Nothing
        Dim Comp As Object = Nothing
        Try
            For Each AD In trn.AuthDesignations
                If AD.EngineID = trn.IdentifingUnit Then
                    BC = Nothing
                    Dim BCUID As Integer = 0
                    Integer.TryParse(AD.BulletinCellID, BCUID)
                    If BCUID > 0 Then
                        BC = TMDS.SystemUtilities.FetchData.FindBulletinCellByID(AD.BulletinCellID)
                        If BC IsNot Nothing Then
                            If Not BCCollection.Contains(BC) Then BCCollection.Add(BC)
                        End If
                    End If
                End If
            Next
            BC = Nothing
            For Each BC In BCCollection
                Dim mr As MileageRange
                For Each mr In BC.myMileageRangeList
                    TMDS.SystemUtilities.FetchData.FindAllTracksFromMilePost(mr.LeftMileage, mr.LeftMileageSuffix, mr.RightMileage, mr.RightMileageSuffix, TrackList)
                Next
            Next

            For Each Comp In Complist
                If Comp IsNot Nothing Then
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If Mid(Comp.AuthorityType, 1, 3) = "TWC" Then
                            If Not TrackList.Contains(Comp) Then
                                If gDebug Then SystemEvent("Track outside TGBO Limit: " & Comp.guid.ToString)
                                Return False
                            End If
                        End If
                    End If
                End If
            Next
            Return True

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            TrackList = Nothing
            BCCollection = Nothing
        End Try


    End Function

    Public Function CheckIfAllTrackIsBABSEnabled(ByVal CompCollection As ArrayList) As Boolean
        Dim OverrideCheck As Boolean = True
        Dim comp As Object = Nothing
        Try
            For Each comp In CompCollection
                If comp.GetType.ToString = gTrackComponentName Then
                    Dim trk As Track = comp
                    If Not trk.BiDirectionalABS Then Return False
                End If
            Next
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            comp = Nothing
        End Try

    End Function

    'Public Function CheckIfOccupancyBelongsToTrain(ByVal TrackToCheck As Track) As Boolean
    '    Dim LeftSideTrainFound As Boolean = False
    '    Dim RightSideTrainFound As Boolean = False
    '    Dim NextComp As Object = Nothing
    '    Dim sw As Switch
    '    Dim SidingCollection As New ArrayList
    '    Dim TempComp As Object = Nothing
    '    Dim PreviousComp As Object = Nothing
    '    Try
    '        If TrackToCheck.OsComponentCollection IsNot Nothing AndAlso TrackToCheck.OsComponentCollection.Count > 0 Then
    '            For Each NextComp In TrackToCheck.OsComponentCollection
    '                If NextComp.GetType.ToString = gSwitchComponentName Then
    '                    sw = NextComp
    '                    If sw.RightAltLink > 0 Then
    '                        TempComp = FindCompFromGUID(sw.RightAltLink)
    '                        For i As Integer = 1 To 5
    '                            If TrackToCheck.OsComponentCollection.Contains(TempComp) Then
    '                                If Not SidingCollection.Contains(TempComp) Then SidingCollection.Add(TempComp)
    '                                TempComp = FindCompFromGUID(TempComp.RightLink)
    '                            Else
    '                                Exit For
    '                            End If
    '                        Next
    '                    ElseIf sw.LeftAltLink > 0 Then
    '                        TempComp = FindCompFromGUID(sw.LeftAltLink)
    '                        For i As Integer = 1 To 5
    '                            If TrackToCheck.OsComponentCollection.Contains(TempComp) Then
    '                                If Not SidingCollection.Contains(TempComp) Then SidingCollection.Add(TempComp)
    '                                TempComp = FindCompFromGUID(TempComp.LeftLink)
    '                            Else
    '                                Exit For
    '                            End If
    '                        Next
    '                    End If
    '                    If Not SidingCollection Is Nothing AndAlso SidingCollection.Count > 0 Then
    '                        If Not sw.ControlledReverse Then
    '                            If SidingCollection.Contains(TrackToCheck) Then Return True
    '                            Exit For
    '                        End If
    '                    Else
    '                        Exit For
    '                    End If

    '                End If
    '            Next

    '        End If


    '        PreviousComp = TrackToCheck
    '        NextComp = FindCompFromGUID(TrackToCheck.RightLink)
    '        For i As Integer = 1 To 10
    '            Select Case NextComp.GetType.ToString
    '                Case gTrackComponentName
    '                    If NextComp.Occuppied Then
    '                        If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
    '                            RightSideTrainFound = True
    '                            Exit For
    '                        Else
    '                            PreviousComp = NextComp
    '                            NextComp = FindCompFromGUID(NextComp.RightLink)
    '                        End If
    '                    Else
    '                        RightSideTrainFound = False
    '                        Exit For
    '                    End If
    '                Case gSignalComponentName
    '                    PreviousComp = NextComp
    '                    NextComp = FindCompFromGUID(NextComp.RightLink)
    '                Case gSwitchComponentName
    '                    sw = Nothing
    '                    sw = NextComp
    '                    If sw.TypeOfSwitch = "SPM ONLY" Then
    '                        PreviousComp = NextComp
    '                        NextComp = FindCompFromGUID(NextComp.RightLink)
    '                    Else
    '                        If sw.ControlledReverse Then
    '                            If sw.RightAltLink > 0 Then
    '                                'PreviousComp = NextComp
    '                                NextComp = FindCompFromGUID(sw.RightAltLink)
    '                            ElseIf sw.LeftAltLink > 0 Then
    '                                'PreviousComp = NextComp
    '                                NextComp = FindCompFromGUID(sw.LeftAltLink)
    '                                If PreviousComp.GUID = sw.LeftAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.RightLink)
    '                                End If
    '                            End If
    '                        Else
    '                            PreviousComp = NextComp
    '                            NextComp = FindCompFromGUID(NextComp.RightLink)
    '                        End If
    '                    End If
    '            End Select
    '        Next



    '        PreviousComp = TrackToCheck
    '        NextComp = Nothing
    '        NextComp = FindCompFromGUID(TrackToCheck.LeftLink)
    '        For i As Integer = 1 To 10
    '            Select Case NextComp.GetType.ToString
    '                Case gTrackComponentName
    '                    If NextComp.Occuppied Then
    '                        If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
    '                            LeftSideTrainFound = True
    '                            Exit For
    '                        Else
    '                            PreviousComp = NextComp
    '                            NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                        End If
    '                    Else
    '                        LeftSideTrainFound = False
    '                        Exit For
    '                    End If
    '                Case gSignalComponentName
    '                    PreviousComp = NextComp
    '                    NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                Case gSwitchComponentName
    '                    sw = Nothing
    '                    sw = NextComp
    '                    If sw.TypeOfSwitch = "SPM ONLY" Then
    '                        PreviousComp = NextComp
    '                        NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                    Else
    '                        If sw.ControlledReverse Then
    '                            If sw.RightAltLink > 0 Then
    '                                'PreviousComp = NextComp
    '                                NextComp = FindCompFromGUID(sw.RightAltLink)
    '                                If PreviousComp.GUID = sw.RightAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.LeftLink)
    '                                End If
    '                            ElseIf sw.LeftAltLink > 0 Then
    '                                'PreviousComp = NextComp
    '                                NextComp = FindCompFromGUID(sw.LeftAltLink)
    '                                If PreviousComp.GUID = sw.LeftAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.LeftLink)
    '                                End If
    '                            End If
    '                        Else
    '                            NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                        End If
    '                    End If
    '            End Select
    '        Next

    '        If LeftSideTrainFound Or RightSideTrainFound Then
    '            Return True
    '        End If
    '        Return False

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '        Return False
    '    End Try
    'End Function

    'Public Function GetTrailingOccupancyTrain(ByVal TrackToCheck As Track) As TrainID
    '    Dim LeftSideTrainFound As Boolean = False
    '    Dim RightSideTrainFound As Boolean = False
    '    Dim NextComp As Object = Nothing
    '    Dim sw As Switch
    '    'Dim SidingCollection As New ArrayList
    '    Dim TempComp As Object = Nothing
    '    Dim PreviousComp As Object = Nothing
    '    Try
    '        PreviousComp = TrackToCheck
    '        NextComp = FindCompFromGUID(TrackToCheck.RightLink)
    '        For i As Integer = 1 To 50
    '            Select Case NextComp.GetType.ToString
    '                Case gTrackComponentName
    '                    If NextComp.Occuppied Then
    '                        If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
    '                            Return NextComp.TrainIDs(0)
    '                        Else
    '                            PreviousComp = NextComp
    '                            NextComp = FindCompFromGUID(NextComp.RightLink)
    '                        End If
    '                    Else
    '                        RightSideTrainFound = False
    '                        Exit For
    '                    End If
    '                Case gSignalComponentName
    '                    PreviousComp = NextComp
    '                    NextComp = FindCompFromGUID(NextComp.RightLink)
    '                Case gSwitchComponentName
    '                    sw = Nothing
    '                    sw = NextComp
    '                    If sw.TypeOfSwitch = "SPM ONLY" Then
    '                        PreviousComp = NextComp
    '                        NextComp = FindCompFromGUID(NextComp.RightLink)
    '                    Else
    '                        If sw.ControlledReverse Then
    '                            If sw.RightAltLink > 0 Then
    '                                NextComp = FindCompFromGUID(sw.RightAltLink)
    '                                If PreviousComp.GUID = sw.RightAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.RightLink)
    '                                End If
    '                            ElseIf sw.LeftAltLink > 0 Then
    '                                NextComp = FindCompFromGUID(sw.LeftAltLink)
    '                                If PreviousComp.GUID = sw.LeftAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.RightLink)
    '                                End If
    '                            End If
    '                        Else
    '                            NextComp = FindCompFromGUID(NextComp.RightLink)
    '                        End If
    '                    End If
    '            End Select
    '        Next
    '        NextComp = Nothing
    '        PreviousComp = TrackToCheck
    '        NextComp = FindCompFromGUID(TrackToCheck.LeftLink)
    '        For i As Integer = 1 To 50
    '            Select Case NextComp.GetType.ToString
    '                Case gTrackComponentName
    '                    If NextComp.Occuppied Then
    '                        If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
    '                            Return NextComp.TrainIDs(0)
    '                        Else
    '                            PreviousComp = NextComp
    '                            NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                        End If
    '                    Else
    '                        LeftSideTrainFound = False
    '                        Exit For
    '                    End If
    '                Case gSignalComponentName
    '                    PreviousComp = NextComp
    '                    NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                Case gSwitchComponentName
    '                    sw = Nothing
    '                    sw = NextComp
    '                    If sw.TypeOfSwitch = "SPM ONLY" Then
    '                        NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                    Else
    '                        If sw.ControlledReverse Then
    '                            If sw.RightAltLink > 0 Then
    '                                NextComp = FindCompFromGUID(sw.RightAltLink)
    '                                If PreviousComp.GUID = sw.RightAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.LeftLink)
    '                                End If
    '                            ElseIf sw.LeftAltLink > 0 Then
    '                                NextComp = FindCompFromGUID(sw.LeftAltLink)
    '                                If PreviousComp.GUID = sw.LeftAltLink Then
    '                                    NextComp = FindCompFromGUID(sw.LeftLink)
    '                                End If
    '                            End If
    '                        Else
    '                            NextComp = FindCompFromGUID(NextComp.LeftLink)
    '                        End If
    '                    End If
    '            End Select
    '        Next
    '        Return Nothing

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '        Return Nothing
    '    End Try
    'End Function

    Public Function CheckIfOccupancyBelongsToTrain(ByVal TrackToCheck As Track, Optional ByVal CrossoverCheck As Boolean = False, Optional ByRef TrainsCollection As ArrayList = Nothing) As Boolean
        Dim NextComp As Object = Nothing
        Dim sw As Switch
        Dim SwitchRightTrack As Track = Nothing
        Dim SwitchLeftTrack As Track = Nothing
        Dim SwitchAltTrack As Track = Nothing

        Dim RightRoute As New ArrayList
        Dim LeftRoute As New ArrayList
        Dim RightObject As Object = Nothing
        Dim LeftObject As Object = Nothing
        Dim RunLen As System.TimeSpan
        Dim msecs As Integer
        Dim MyStartTime As Date = Now
        Dim LastTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Try
            TrainsCollection = New ArrayList
            'Added Based on CP RMT 1115. 
            If TrackToCheck IsNot Nothing AndAlso TrackToCheck.Occuppied Then
                If TrackToCheck.OsComponentCollection IsNot Nothing AndAlso TrackToCheck.OsComponentCollection.Count > 0 Then
                    For Each Comp In TrackToCheck.OsComponentCollection
                        If Comp.GetType.ToString = gTrackComponentName Then
                            If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                Return True
                            End If
                        End If
                    Next
                End If
            End If

            RightObject = FindCompFromGUID(TrackToCheck.RightLink)
            LeftObject = FindCompFromGUID(TrackToCheck.LeftLink)
            If gDebug Then
                RunLen = Now.Subtract(MyStartTime)
                gStartTime = Now
                msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                SystemEvent(" CheckIfOccupancyBelongsToTrain RIGHT Start (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
            End If
            LastTrack = TrackToCheck
            For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                If RightObject Is Nothing Then Exit For
                Select Case RightObject.GetType.ToString
                    Case gTrackComponentName
                        If Not RightObject.NonTransverable AndAlso RightObject.Occuppied AndAlso Mid(RightObject.AuthorityType, 1, 3) = "CTC" Then
                            If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                            If RightObject.TrainIDs IsNot Nothing AndAlso RightObject.TrainIDs.Count > 0 Then
                                CopyArrayCollection(TrainsCollection, RightObject.TrainIDs)
                                Return True
                            End If
                            LastTrack = RightObject
                            RightObject = FindCompFromGUID(RightObject.RightLink)

                        Else
                            Exit For
                        End If

                    Case gSwitchComponentName
                        If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject) Else Exit For
                        sw = Nothing
                        sw = RightObject
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            RightObject = FindCompFromGUID(RightObject.RightLink)
                        Else
                            If sw.ControlledReverse AndAlso Not CrossoverCheck Then
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchAltTrack = Nothing

                                Dim TravellingTowardsSwitch As Boolean = False
                                SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                If sw.RightAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                End If
                                If RightRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True
                                If TravellingTowardsSwitch Then
                                    RightObject = FindCompFromGUID(RightObject.RightLink)
                                Else
                                    If LastTrack Is SwitchAltTrack Then
                                        RightObject = FindCompFromGUID(RightObject.RightLink)
                                    Else
                                        RightObject = FindCompFromGUID(SwitchAltTrack.RightLink)
                                    End If

                                End If
                            Else
                                RightObject = FindCompFromGUID(RightObject.RightLink)
                            End If
                        End If

                    Case Else
                        If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                        RightObject = FindCompFromGUID(RightObject.RightLink)
                End Select
            Next
            If gDebug Then
                RunLen = Now.Subtract(MyStartTime)
                gStartTime = Now
                msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                SystemEvent(" CheckIfOccupancyBelongsToTrain RIGHT End (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
            End If
            If gDebug Then
                RunLen = Now.Subtract(MyStartTime)
                gStartTime = Now
                msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                SystemEvent(" CheckIfOccupancyBelongsToTrain LEFT Start (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
            End If
            LastTrack = TrackToCheck
            For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                If LeftObject Is Nothing Then Exit For
                Select Case LeftObject.GetType.ToString
                    Case gTrackComponentName
                        If Not LeftObject.NonTransverable AndAlso LeftObject.Occuppied AndAlso Mid(LeftObject.AuthorityType, 1, 3) = "CTC" Then
                            If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                            If LeftObject.TrainIDs IsNot Nothing AndAlso LeftObject.TrainIDs.Count > 0 Then
                                CopyArrayCollection(TrainsCollection, LeftObject.TrainIDs)
                                Return True
                            End If
                            LastTrack = LeftObject
                            LeftObject = FindCompFromGUID(LeftObject.LeftLink)

                        Else
                            Exit For
                        End If

                    Case gSwitchComponentName
                        If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject) Else Exit For
                        sw = Nothing
                        sw = LeftObject
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                        Else
                            If sw.ControlledReverse AndAlso Not CrossoverCheck Then
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchAltTrack = Nothing

                                Dim TravellingTowardsSwitch As Boolean = False
                                SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                If sw.RightAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                End If
                                If LeftRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True

                                If TravellingTowardsSwitch Then
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                Else
                                    If SwitchAltTrack Is LastTrack Then
                                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                    Else
                                        LeftObject = FindCompFromGUID(SwitchAltTrack.LeftLink)
                                    End If

                                End If
                            Else
                                LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                            End If
                        End If

                    Case Else
                        If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                End Select
            Next
            If gDebug Then
                RunLen = Now.Subtract(MyStartTime)
                gStartTime = Now
                msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                SystemEvent(" CheckIfOccupancyBelongsToTrain LEFT END (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
            End If
            For Each NextComp In RightRoute
                If NextComp.GetType.ToString = gTrackComponentName Then
                    If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                        CopyArrayCollection(TrainsCollection, NextComp.TrainIDs)
                        Return True
                    ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                        For Each Comp In NextComp.OsComponentCollection
                            If Comp.GetType.ToString = gTrackComponentName Then
                                If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                    CopyArrayCollection(TrainsCollection, Comp.TrainIDs)
                                    Return True
                                End If
                            End If
                        Next
                    End If
                End If
            Next
            For Each NextComp In LeftRoute
                If NextComp.GetType.ToString = gTrackComponentName Then
                    If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                        CopyArrayCollection(TrainsCollection, NextComp.TrainIDs)
                        Return True
                    ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                        For Each Comp In NextComp.OsComponentCollection
                            If Comp.GetType.ToString = gTrackComponentName Then
                                If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                    CopyArrayCollection(TrainsCollection, Comp.TrainIDs)
                                    Return True
                                End If
                            End If
                        Next
                    End If
                End If
            Next


            Return False

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        Finally
            NextComp = Nothing
            RightRoute = Nothing
            LeftRoute = Nothing
            LeftObject = Nothing
            RightObject = Nothing

            sw = Nothing
            SwitchRightTrack = Nothing
            SwitchLeftTrack = Nothing
            SwitchAltTrack = Nothing
            Comp = Nothing
        End Try
    End Function

    Public Function CheckIfOccupancyBelongsToTrainByDirection(ByVal TrackToCheck As Track, ByVal DirectionToCheck As String) As Boolean
        Dim NextComp As Object = Nothing
        Dim sw As Switch
        Dim SwitchRightTrack As Track = Nothing
        Dim SwitchLeftTrack As Track = Nothing
        Dim SwitchAltTrack As Track = Nothing

        Dim RightRoute As New ArrayList
        Dim LeftRoute As New ArrayList
        Dim RightObject As Object = Nothing
        Dim LeftObject As Object = Nothing
        Dim RunLen As System.TimeSpan
        Dim msecs As Integer
        Dim MyStartTime As Date = Now
        Dim LastTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Try
            'Added Based on CP RMT 1115. 
            If TrackToCheck IsNot Nothing AndAlso TrackToCheck.Occuppied Then
                If TrackToCheck.OsComponentCollection IsNot Nothing AndAlso TrackToCheck.OsComponentCollection.Count > 0 Then
                    For Each Comp In TrackToCheck.OsComponentCollection
                        If Comp.GetType.ToString = gTrackComponentName Then
                            If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                Return True
                            End If
                        End If
                    Next
                End If
            End If

            RightObject = FindCompFromGUID(TrackToCheck.RightLink)
            LeftObject = FindCompFromGUID(TrackToCheck.LeftLink)


            If gDebug Then
                RunLen = Now.Subtract(MyStartTime)
                gStartTime = Now
                msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                SystemEvent(" CheckIfOccupancyBelongsToTrain RIGHT Start (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
            End If
            Select Case DirectionToCheck.Trim.ToUpper
                Case "RIGHT"
                    LastTrack = TrackToCheck
                    For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                        If RightObject Is Nothing Then Exit For
                        Select Case RightObject.GetType.ToString
                            Case gTrackComponentName
                                If Not RightObject.NonTransverable AndAlso RightObject.Occuppied AndAlso Mid(RightObject.AuthorityType, 1, 3) = "CTC" Then
                                    If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                                    If RightObject.TrainIDs IsNot Nothing AndAlso RightObject.TrainIDs.Count > 0 Then
                                        Return True
                                    End If
                                    LastTrack = RightObject
                                    RightObject = FindCompFromGUID(RightObject.RightLink)

                                Else
                                    Exit For
                                End If

                            Case gSwitchComponentName
                                If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject) Else Exit For
                                sw = Nothing
                                sw = RightObject
                                If sw.TypeOfSwitch = "SPM ONLY" Then
                                    RightObject = FindCompFromGUID(RightObject.RightLink)
                                Else
                                    If sw.ControlledReverse Then
                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchAltTrack = Nothing

                                        Dim TravellingTowardsSwitch As Boolean = False
                                        SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                        If sw.RightAltLink > 0 Then
                                            SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                        ElseIf sw.LeftAltLink > 0 Then
                                            SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                        End If
                                        If RightRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True
                                        If TravellingTowardsSwitch Then
                                            RightObject = FindCompFromGUID(RightObject.RightLink)
                                        Else
                                            If LastTrack Is SwitchAltTrack Then
                                                RightObject = FindCompFromGUID(RightObject.RightLink)
                                            Else
                                                RightObject = FindCompFromGUID(SwitchAltTrack.RightLink)
                                            End If

                                        End If
                                    Else
                                        RightObject = FindCompFromGUID(RightObject.RightLink)
                                    End If
                                End If

                            Case Else
                                If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                                RightObject = FindCompFromGUID(RightObject.RightLink)
                        End Select
                    Next
                    For Each NextComp In RightRoute
                        If NextComp.GetType.ToString = gTrackComponentName Then
                            If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                                Return True
                            ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                                For Each Comp In NextComp.OsComponentCollection
                                    If Comp.GetType.ToString = gTrackComponentName Then
                                        If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                            Return True
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                    If gDebug Then
                        RunLen = Now.Subtract(MyStartTime)
                        gStartTime = Now
                        msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                        SystemEvent(" CheckIfOccupancyBelongsToTrain RIGHT End (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
                    End If
                Case "LEFT"
                    LastTrack = TrackToCheck
                    For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                        If LeftObject Is Nothing Then Exit For
                        Select Case LeftObject.GetType.ToString
                            Case gTrackComponentName
                                If Not LeftObject.NonTransverable AndAlso LeftObject.Occuppied AndAlso Mid(LeftObject.AuthorityType, 1, 3) = "CTC" Then
                                    If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                                    If LeftObject.TrainIDs IsNot Nothing AndAlso LeftObject.TrainIDs.Count > 0 Then
                                        Return True
                                    End If
                                    LastTrack = LeftObject
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)

                                Else
                                    Exit For
                                End If

                            Case gSwitchComponentName
                                If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject) Else Exit For
                                sw = Nothing
                                sw = LeftObject
                                If sw.TypeOfSwitch = "SPM ONLY" Then
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                Else
                                    If sw.ControlledReverse Then
                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchAltTrack = Nothing

                                        Dim TravellingTowardsSwitch As Boolean = False
                                        SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                        If sw.RightAltLink > 0 Then
                                            SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                        ElseIf sw.LeftAltLink > 0 Then
                                            SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                        End If
                                        If LeftRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True

                                        If TravellingTowardsSwitch Then
                                            LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                        Else
                                            If SwitchAltTrack Is LastTrack Then
                                                LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                            Else
                                                LeftObject = FindCompFromGUID(SwitchAltTrack.LeftLink)
                                            End If

                                        End If
                                    Else
                                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                    End If
                                End If

                            Case Else
                                If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                                LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                        End Select
                    Next
                    For Each NextComp In LeftRoute
                        If NextComp.GetType.ToString = gTrackComponentName Then
                            If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                                Return True
                            ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                                For Each Comp In NextComp.OsComponentCollection
                                    If Comp.GetType.ToString = gTrackComponentName Then
                                        If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                            Return True
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                    If gDebug Then
                        RunLen = Now.Subtract(MyStartTime)
                        gStartTime = Now
                        msecs = ((RunLen.Seconds * 1000) + RunLen.Milliseconds)
                        SystemEvent(" CheckIfOccupancyBelongsToTrain LEFT END (Track GUID:[" & TrackToCheck.GUID & "]) - Execution Time in Milliseconds: " & msecs.ToString)
                    End If
            End Select






            Return False

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        Finally
            NextComp = Nothing
            RightRoute = Nothing
            LeftRoute = Nothing
            LeftObject = Nothing
            RightObject = Nothing

            sw = Nothing
            SwitchRightTrack = Nothing
            SwitchLeftTrack = Nothing
            SwitchAltTrack = Nothing
            Comp = Nothing
        End Try
    End Function

    Public Function GetTrailingOccupancyTrains(ByVal TrackToCheck As Track, Optional ByVal CrossoverCheck As Boolean = False) As ArrayList

        Dim NextComp As Object = Nothing
        Dim sw As Switch
        Dim SwitchRightTrack As Track = Nothing
        Dim SwitchLeftTrack As Track = Nothing
        Dim SwitchAltTrack As Track = Nothing

        Dim RightRoute As New ArrayList
        Dim LeftRoute As New ArrayList
        Dim RightObject As Object = Nothing
        Dim LeftObject As Object = Nothing
        Dim LastTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Dim OktoProceedRightSearch As Boolean = True
        Dim OktoProceedLeftSearch As Boolean = True
        Dim Dir As String = ""
        Try
            If CrossoverCheck AndAlso TrackToCheck IsNot Nothing Then
                If TrackToCheck.Occuppied AndAlso TrackToCheck.OsComponentCollection IsNot Nothing AndAlso TrackToCheck.OsComponentCollection.Count > 0 Then
                    For Each Comp In TrackToCheck.OsComponentCollection
                        If Comp.GetType.ToString = gTrackComponentName Then
                            If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                Return Comp.TrainIDs
                            End If
                        End If
                    Next
                End If
            End If

            If Not String.IsNullOrEmpty(TrackToCheck.TrafficDirection) Then
                Dir = TrackToCheck.TrafficDirection.ToUpper.Trim
                Select Case Dir
                    Case "RIGHT"
                        OktoProceedLeftSearch = False
                    Case "LEFT"
                        OktoProceedRightSearch = False
                End Select
            End If

            If OktoProceedRightSearch Then RightObject = FindCompFromGUID(TrackToCheck.RightLink)
            If OktoProceedLeftSearch Then LeftObject = FindCompFromGUID(TrackToCheck.LeftLink)
            LastTrack = TrackToCheck
            For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                If RightObject Is Nothing Then Exit For
                Select Case RightObject.GetType.ToString
                    Case gTrackComponentName
                        If RightObject.Occuppied AndAlso Not RightObject.NonTransverable Then
                            LastTrack = RightObject
                            If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                            RightObject = FindCompFromGUID(RightObject.RightLink)
                        Else
                            Exit For
                        End If

                    Case gSwitchComponentName
                        If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                        sw = Nothing
                        sw = RightObject
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            RightObject = FindCompFromGUID(RightObject.RightLink)
                        Else
                            If sw.ControlledReverse AndAlso Not CrossoverCheck Then
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchAltTrack = Nothing

                                Dim TravellingTowardsSwitch As Boolean = False
                                SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                If sw.RightAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                End If
                                If RightRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True
                                If TravellingTowardsSwitch Then
                                    RightObject = FindCompFromGUID(RightObject.RightLink)
                                Else
                                    If LastTrack Is SwitchAltTrack Then
                                        RightObject = FindCompFromGUID(RightObject.RightLink)
                                    Else
                                        RightObject = FindCompFromGUID(SwitchAltTrack.RightLink)
                                    End If

                                End If
                            Else
                                RightObject = FindCompFromGUID(RightObject.RightLink)
                            End If
                        End If

                    Case Else
                        If Not RightRoute.Contains(RightObject) Then RightRoute.Add(RightObject)
                        RightObject = FindCompFromGUID(RightObject.RightLink)
                End Select
            Next

            LastTrack = TrackToCheck
            For i As Integer = 0 To TMDS.GlobalDeclarations.TrackCollection.Count
                If LeftObject Is Nothing Then Exit For
                Select Case LeftObject.GetType.ToString
                    Case gTrackComponentName
                        If LeftObject.Occuppied AndAlso Not LeftObject.NonTransverable Then
                            LastTrack = LeftObject
                            If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                            LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                        Else
                            Exit For
                        End If

                    Case gSwitchComponentName
                        If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                        sw = Nothing
                        sw = LeftObject
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                        Else
                            If sw.ControlledReverse AndAlso Not CrossoverCheck Then
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchAltTrack = Nothing

                                Dim TravellingTowardsSwitch As Boolean = False
                                SwitchRightTrack = FindCompFromGUID(sw.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(sw.LeftLink)
                                If sw.RightAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    SwitchAltTrack = FindCompFromGUID(sw.LeftAltLink)
                                End If
                                If LeftRoute.Contains(SwitchAltTrack) Then TravellingTowardsSwitch = True

                                If TravellingTowardsSwitch Then
                                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                Else
                                    If LastTrack Is SwitchAltTrack Then
                                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                                    Else
                                        LeftObject = FindCompFromGUID(SwitchAltTrack.LeftLink)
                                    End If
                                End If
                            Else
                                LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                            End If
                        End If

                    Case Else
                        If Not LeftRoute.Contains(LeftObject) Then LeftRoute.Add(LeftObject)
                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                End Select
            Next

            For Each NextComp In RightRoute
                If NextComp.GetType.ToString = gTrackComponentName Then
                    If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                        Return NextComp.TrainIDs
                    ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                        For Each Comp In NextComp.OsComponentCollection
                            If Comp.GetType.ToString = gTrackComponentName Then
                                If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                    Return Comp.TrainIDs
                                End If
                            End If
                        Next
                    End If
                End If
            Next
            For Each NextComp In LeftRoute
                If NextComp.GetType.ToString = gTrackComponentName Then
                    If NextComp.TrainIDs IsNot Nothing AndAlso NextComp.TrainIDs.Count > 0 Then
                        Return NextComp.TrainIDs
                    ElseIf NextComp.OsComponentCollection IsNot Nothing AndAlso NextComp.OsComponentCollection.Count > 0 Then
                        For Each Comp In NextComp.OsComponentCollection
                            If Comp.GetType.ToString = gTrackComponentName Then
                                If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
                                    Return Comp.TrainIDs
                                End If
                            End If
                        Next
                    End If
                End If
            Next


        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally
            NextComp = Nothing
            RightRoute = Nothing
            LeftRoute = Nothing
            LeftObject = Nothing
            RightObject = Nothing

            sw = Nothing
            SwitchRightTrack = Nothing
            SwitchLeftTrack = Nothing
            SwitchAltTrack = Nothing
            Comp = Nothing
        End Try
        Return Nothing
    End Function

    Public Sub GetAuthDesignation(ByRef HasEast As Boolean, ByRef HasWest As Boolean, ByRef HasWork As Boolean, ByVal trn As TrainID)
        '140;OTTUMWA;raja 1234;EAST;1410|140;OTTUMWA;raja 1234;WEST;1410|140;OTTUMWA;raja 1234;WORK;1410| - OLD
        '140;OTTUMWA;CP    1244;NORTH;5;TWC;True|140;OTTUMWA;CP    1245;NORTH;5;TWC;False|  - NEW
        Try
            If trn Is Nothing Then Exit Sub
            If trn.AuthDesignations IsNot Nothing AndAlso trn.AuthDesignations.Count > 0 Then
                Dim AuthDesg() As String = Nothing
                Dim s As TMDS.TrainComponents.AuthorityDesignation = Nothing

                Dim TerrID As Integer = 0
                Dim SubName As String = ""
                Dim IssueTo As String = ""
                Dim AuthDirection As String = ""
                Dim BCID As Integer = 0

                For Each s In trn.AuthDesignations
                    AuthDesg = Nothing
                    If ((s.EngineID.Trim.ToUpper = trn.IdentifingUnit.Trim.ToUpper) Or (s.EngineID.Trim.ToUpper = trn.PendingLeadEngine.Trim.ToUpper)) AndAlso s.DesignationType = "TWC" AndAlso Not s.InActive Then
                        Select Case s.Direction.Trim.ToUpper
                            Case "EAST", "NORTH"
                                HasEast = True
                            Case "WEST", "SOUTH"
                                HasWest = True
                            Case "WORK"
                                HasWork = True
                        End Select
                    End If
                Next
            End If




        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub



    Public Function GetTrainDirectionFromLeftRight(ByVal Direction As String, ByVal Trk As Track) As String
        Dim subdvn As Subdivision = Nothing
        Try
            If Trk Is Nothing Then Return String.Empty
            If Trk.mySubdivision Is Nothing Then Return String.Empty

            subdvn = Trk.mySubdivision
            Select Case Direction.Trim.ToUpper
                Case "LEFT"
                    If subdvn.SubDirection Then
                        If Trk.myTerritory.FlipDirection Then
                            Return optDirectionNorth
                        Else
                            Return optDirectionSouth
                        End If
                    Else
                        If Trk.myTerritory.FlipDirection Then
                            Return optDirectionWest
                        Else
                            Return optDirectionEast
                        End If

                    End If
                    'Return optDirectionSouth
                Case "RIGHT"
                    'Return optDirectionNorth
                    If subdvn.SubDirection Then
                        If Trk.myTerritory.FlipDirection Then
                            Return optDirectionSouth
                        Else
                            Return optDirectionNorth
                        End If

                        'Return optDirectionNorth
                    Else
                        'Return optDirectionWest
                        If Trk.myTerritory.FlipDirection Then
                            Return optDirectionEast
                        Else
                            Return optDirectionWest
                        End If
                    End If

                Case Else
                    Return optDirectionNorth
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        'Try
        '    Select Case Direction.Trim.ToUpper
        '        Case "LEFT"
        '            If Direction = "WEST" Then
        '                Return optDirectionEast
        '            Else
        '                Return optDirectionSouth
        '            End If
        '        Case "RIGHT"
        '            If Direction = "EAST" Then
        '                Return optDirectionWest
        '            Else
        '                Return optDirectionNorth
        '            End If
        '            'Case Else
        '            '    Return optDirectionNorth
        '    End Select
        'Catch ex As Exception
        '    GetErrorDetails(ex)
        'End Try
        Return String.Empty
    End Function
    Public Function GetTrainDirectionMRS(ByVal Direction As String) As String

        Try
            Select Case Direction.Trim.ToUpper
                Case optDirectionSouth
                    Return "LEFT"
                Case optDirectionNorth
                    Return "RIGHT"
            End Select
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

    Private Delegate Sub UpdateTrainDirection()
    Public Function PerformTrainDirectionCheckCN(ByVal TrackingDirection As String, trn As TrainID, trk As Track) As String
        Try
            Dim FlipDirection As Boolean
            If optCheckForTerritoryFlipDirectionEnabled Then
                FlipDirection = trk.myTerritory.FlipDirection
            End If



            If Not IsNothing(TrackingDirection) Then
                Dim OldDirection As String = String.Empty
                Dim AssumedDirection As String = String.Empty
                Dim DirectionBasedOnSubDivision As String = GetSearchableTrainDirectionBasedOnSubdivision(trn.Direction, trn.myTrack.mySubdivision)
                Select Case trn.Direction
                    Case optDirectionEast, optDirectionNorth
                        If FlipDirection Then
                            AssumedDirection = "LEFT"
                        Else
                            AssumedDirection = "RIGHT"
                        End If

                    Case optDirectionWest, optDirectionSouth
                        If FlipDirection Then
                            AssumedDirection = "RIGHT"
                        Else
                            AssumedDirection = "LEFT"
                        End If
                End Select

                If (optEnableCPSpecificOptions) AndAlso Not String.IsNullOrEmpty(DirectionBasedOnSubDivision) Then
                    AssumedDirection = DirectionBasedOnSubDivision

                End If

                Dim DirectionChanged As Boolean
                If AssumedDirection <> "" Then
                    OldDirection = trn.Direction
                    If AssumedDirection <> TrackingDirection Then

                        Select Case trn.Direction
                            Case optDirectionEast
                                trn.Direction = optDirectionWest
                                DirectionChanged = True
                            Case optDirectionNorth
                                trn.Direction = optDirectionSouth
                                DirectionChanged = True
                            Case optDirectionWest
                                trn.Direction = optDirectionEast
                                DirectionChanged = True
                            Case optDirectionSouth
                                trn.Direction = optDirectionNorth
                                DirectionChanged = True
                        End Select

                    End If

                    Dim isEastWestDirection As Boolean = True
                    Dim ChangeToSubDirection As Boolean = False
                    If trn.myTrack.mySubdivision.SubDirection Then
                        isEastWestDirection = False
                    End If
                    Select Case trn.Direction
                        Case optDirectionEast, optDirectionWest
                            If Not isEastWestDirection Then ChangeToSubDirection = True
                        Case optDirectionNorth, optDirectionSouth
                            If isEastWestDirection Then ChangeToSubDirection = True
                    End Select
                    If ChangeToSubDirection Then
                        If isEastWestDirection Then
                            Select Case trn.Direction
                                Case optDirectionNorth
                                    trn.Direction = optDirectionEast
                                    DirectionChanged = True
                                Case optDirectionSouth
                                    trn.Direction = optDirectionWest
                                    DirectionChanged = True
                            End Select
                        Else
                            Select Case trn.Direction
                                Case optDirectionWest
                                    trn.Direction = optDirectionNorth
                                    DirectionChanged = True
                                Case optDirectionEast
                                    trn.Direction = optDirectionSouth
                                    DirectionChanged = True
                            End Select
                        End If
                    End If
                End If
                If DirectionChanged Then
                    Dim DoUpdateToDirection As New UpdateTrainDirection(AddressOf trn.UpdateTrainDirection)
                    DoUpdateToDirection.BeginInvoke(Nothing, Nothing)
                    trn.TrainSummaryDirectionRefresh()
                    TMDS.SystemUtilities.Utilities.SystemEvent("AUTOMATIC TRAIN Direction CHANGE FROM <" & OldDirection & "> TO <" & trn.Direction & ">")
                    trk.InitTrainDisplay()

                End If
            End If
        Catch ex As Exception

        End Try
        'Try
        '    Select Case Direction.Trim.ToUpper
        '        Case optDirectionSouth, optDirectionEast
        '            Return "LEFT"
        '        Case optDirectionNorth, optDirectionWest
        '            Return "RIGHT"
        '    End Select
        'Catch ex As Exception
        '    GetErrorDetails(ex)
        'End Try
        Return String.Empty
    End Function

    Public Function FindAllCTCTracks(ByVal TrackToSearch As Track, ByVal SearchDirection As String) As ArrayList
        Dim CTCTracksCollection As New ArrayList
        Dim comp As Object = Nothing
        Dim ControlPointsCollection As New ArrayList
        Dim sig As Signal = Nothing
        Dim CP As ControlPoint = Nothing
        Try
            If TrackToSearch Is Nothing Then Return Nothing
            Select Case SearchDirection
                Case "RIGHT"
                    comp = FindCompFromGUID(TrackToSearch.RightLink)
                    If comp IsNot Nothing Then
                        For I As Integer = 0 To gControlPointCollection.Count - 1
                            If Not ControlPointsCollection.Contains(comp.myControlPoint) Then ControlPointsCollection.Add(comp.myControlPoint)
                            comp = FindCompFromGUID(comp.RightLink)
                            If comp.GetType.ToString = gTrackComponentName Then
                                If Mid(comp.AuthorityType.Trim.ToUpper, 1, 3) = "TWC" Then
                                    Exit For
                                End If
                            End If
                        Next

                    End If
                Case "LEFT"
                    comp = FindCompFromGUID(TrackToSearch.LeftLink)
                    If comp IsNot Nothing Then
                        For I As Integer = 0 To gControlPointCollection.Count - 1
                            If Not ControlPointsCollection.Contains(comp.myControlPoint) Then ControlPointsCollection.Add(comp.myControlPoint)
                            comp = FindCompFromGUID(comp.RightLink)
                            If comp.GetType.ToString = gTrackComponentName Then
                                If Mid(comp.AuthorityType.Trim.ToUpper, 1, 3) = "TWC" Then
                                    Exit For
                                End If
                            End If
                        Next
                    End If
            End Select
            For Each CP In ControlPointsCollection
                If Not CP.AssignedTracks Is Nothing Then
                    For Each comp In CP.AssignedTracks
                        If Not CTCTracksCollection.Contains(comp) Then CTCTracksCollection.Add(comp)
                    Next
                End If
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally

        End Try

        Return CTCTracksCollection
    End Function

    Public Function RearrangeCompCollection_ver1(ByVal CollectionToBeReaaranged As ArrayList) As ArrayList
        'Dim FirstTrack As Track = Nothing
        'Dim LastTrack As Track = Nothing
        Dim RightTrack As Track = Nothing
        Dim LeftTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Dim SearchDirection As String = ""
        Dim StartTrack As Object = Nothing
        Dim ArrangedCollection As New ArrayList
        Dim RightObject As Object = Nothing
        Dim LeftObject As Object = Nothing

        Dim SwitchRightTrack As Track = Nothing
        Dim SwitchLeftTrack As Track = Nothing
        Dim SwitchLeftAltTrack As Track = Nothing
        Dim SwitchRightAltTrack As Track = Nothing
        Dim SwitchComps As Object = Nothing
        Try
            For Each Comp In CollectionToBeReaaranged
                RightObject = Nothing
                LeftObject = Nothing
                If Comp.RightLink > 0 Then
                    RightObject = FindCompFromGUID(Comp.RightLink)
                End If
                If Comp.LeftLink > 0 Then
                    LeftObject = FindCompFromGUID(Comp.LeftLink)
                End If

                If RightObject Is Nothing Then
                    SearchDirection = "RIGHT"
                    StartTrack = Comp
                ElseIf LeftObject Is Nothing Then
                    SearchDirection = "LEFT"
                    StartTrack = Comp
                End If
                If StartTrack IsNot Nothing Then Exit For
                If RightObject.GetType.ToString = gSignalComponentName Then
                    If Not CollectionToBeReaaranged.Contains(RightObject) Then
                        RightObject = FindCompFromGUID(RightObject.RightLink)
                    End If
                End If
                If LeftObject.GetType.ToString = gSignalComponentName Then
                    If Not CollectionToBeReaaranged.Contains(LeftObject) Then
                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                    End If
                End If

                If Not CollectionToBeReaaranged.Contains(RightObject) Then
                    SearchDirection = "LEFT"
                    StartTrack = Comp
                ElseIf Not CollectionToBeReaaranged.Contains(LeftObject) Then
                    SearchDirection = "RIGHT"
                    StartTrack = Comp
                End If
                If StartTrack IsNot Nothing Then Exit For
            Next

            Comp = Nothing
            If SearchDirection.Trim = "" AndAlso StartTrack Is Nothing Then
            Else
                Select Case SearchDirection
                    Case "RIGHT"
                        If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
                        Comp = FindCompFromGUID(StartTrack.RightLink)
                        For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
                            If CollectionToBeReaaranged.Contains(Comp) Then
                                If Comp.GetType.ToString = gSwitchComponentName Then
                                    Dim SW As Switch = Comp
                                    If SW.TypeOfSwitch = "SPM ONLY" Then
                                        If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                        Comp = FindCompFromGUID(Comp.RightLink)
                                    Else
                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchRightAltTrack = Nothing
                                        SwitchLeftAltTrack = Nothing
                                        SwitchComps = Nothing
                                        SwitchRightTrack = FindCompFromGUID(SW.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
                                        If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)
                                        If SwitchRightAltTrack IsNot Nothing AndAlso SwitchRightAltTrack.NonTransverable Then
                                            If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
                                            For Each SwitchComps In SW.OsComponentCollection
                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                    If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps.NonTransverable Then
                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                    End If
                                                End If
                                            Next
                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                            Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                        ElseIf SwitchRightAltTrack IsNot Nothing AndAlso Not SwitchRightAltTrack.NonTransverable Then
                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                            For Each SwitchComps In SW.OsComponentCollection
                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                    If SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps.NonTransverable Then
                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                    End If
                                                End If
                                            Next
                                            If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
                                            Comp = FindCompFromGUID(SwitchRightAltTrack.RightLink)
                                        ElseIf SwitchLeftAltTrack IsNot Nothing Then
                                            If SwitchLeftAltTrack.NonTransverable Then
                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchRightTrack Then
                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next
                                                If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                            Else

                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next
                                                If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)

                                            End If
                                        End If
                                    End If
                                Else
                                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                    Comp = FindCompFromGUID(Comp.RightLink)
                                End If
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        End If
                                    End If
                                End If
                            Else
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        Else
                                            Exit For
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If

                            End If
                        Next
                    Case "LEFT"
                        If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
                        Comp = FindCompFromGUID(StartTrack.LeftLink)
                        For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
                            If CollectionToBeReaaranged.Contains(Comp) Then
                                If Comp.GetType.ToString = gSwitchComponentName Then
                                    Dim SW As Switch = Comp
                                    If SW.TypeOfSwitch = "SPM ONLY" Then
                                        If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                        Comp = FindCompFromGUID(Comp.LeftLink)
                                    Else
                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchRightAltTrack = Nothing
                                        SwitchLeftAltTrack = Nothing
                                        SwitchComps = Nothing
                                        SwitchRightTrack = FindCompFromGUID(SW.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
                                        If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)
                                        If SwitchLeftAltTrack IsNot Nothing AndAlso SwitchLeftAltTrack.NonTransverable Then
                                            If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                            For Each SwitchComps In SW.OsComponentCollection
                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                    If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps.NonTransverable Then
                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                    End If
                                                End If
                                            Next
                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                            Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
                                        ElseIf SwitchLeftAltTrack IsNot Nothing AndAlso Not SwitchLeftAltTrack.NonTransverable Then
                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                            For Each SwitchComps In SW.OsComponentCollection
                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                    If SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps.NonTransverable Then
                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                    End If
                                                End If
                                            Next
                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                            Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
                                        ElseIf SwitchRightAltTrack IsNot Nothing Then

                                            If SwitchRightAltTrack.NonTransverable Then
                                                If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchLeftTrack Then
                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next

                                                If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)
                                            Else

                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchLeftTrack AndAlso SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next
                                                If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)

                                            End If







                                        End If
                                    End If
                                Else
                                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                    Comp = FindCompFromGUID(Comp.LeftLink)
                                End If
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        LeftObject = Nothing
                                        LeftObject = FindCompFromGUID(Comp.LeftLink)
                                        If CollectionToBeReaaranged.Contains(LeftObject) Then
                                            Comp = LeftObject
                                        End If
                                    End If
                                End If
                            Else
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        Else
                                            Exit For
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If

                                'Exit For
                            End If
                        Next
                End Select
            End If

            If gDebug Then
                Comp = Nothing
                For Each Comp In ArrangedCollection
                    SystemEvent(Comp.GetType.ToString & Space(2) & Comp.Guid)
                Next
            End If

            Return ArrangedCollection

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            LeftObject = Nothing
            RightObject = Nothing
            LeftTrack = Nothing
            RightTrack = Nothing
            Comp = Nothing
            SwitchRightTrack = Nothing
            SwitchLeftTrack = Nothing
            SwitchRightAltTrack = Nothing
            SwitchLeftAltTrack = Nothing
            SwitchComps = Nothing
        End Try
        Return Nothing
    End Function

    Public Sub SortCollection(ByRef col As Collection, ByVal psSortPropertyName As String, ByVal pbAscending As Boolean, Optional ByVal psKeyPropertyName As String = "")

        Dim obj As Object
        Dim i As Integer
        Dim j As Integer
        Dim iMinMaxIndex As Integer
        Dim vMinMax As Object
        Dim vValue As Object
        Dim bSortCondition As Boolean
        Dim bUseKey As Boolean
        Dim sKey As String

        bUseKey = (psKeyPropertyName <> "")

        For i = 1 To col.Count - 1
            obj = col(i)
            vMinMax = CallByName(obj, psSortPropertyName, vbGet)
            iMinMaxIndex = i

            For j = i + 1 To col.Count
                obj = col(j)
                vValue = CallByName(obj, psSortPropertyName, vbGet)

                If (pbAscending) Then
                    bSortCondition = (vValue < vMinMax)
                Else
                    bSortCondition = (vValue > vMinMax)
                End If

                If (bSortCondition) Then
                    vMinMax = vValue
                    iMinMaxIndex = j
                End If

                obj = Nothing
            Next j

            If (iMinMaxIndex <> i) Then
                obj = col(iMinMaxIndex)

                col.Remove(iMinMaxIndex)
                If (bUseKey) Then
                    sKey = CStr(CallByName(obj, psKeyPropertyName, vbGet))
                    col.Add(obj, sKey, i)
                Else
                    col.Add(obj, , i)
                End If

                obj = Nothing
            End If

            obj = Nothing
        Next i

    End Sub

    Public Function RearrangeCompCollection(ByVal CollectionToBeReaaranged As ArrayList, ByVal IsPSSType As Boolean) As ArrayList
        Dim FirstTrack As Track = Nothing
        'Dim LastTrack As Track = Nothing
        Dim RightTrack As Track = Nothing
        Dim LeftTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Dim SearchDirection As String = ""
        Dim StartTrack As Object = Nothing
        Dim ArrangedCollection As New ArrayList
        Dim RightObject As Object = Nothing
        Dim LeftObject As Object = Nothing

        Dim SwitchRightTrack As Track = Nothing
        Dim SwitchLeftTrack As Track = Nothing
        Dim SwitchLeftAltTrack As Track = Nothing
        Dim SwitchRightAltTrack As Track = Nothing
        Dim SwitchComps As Object = Nothing
        Dim OsComp As Object = Nothing

        Dim EndOSSwitchCollection As New ArrayList
        Dim EntrySignal As Signal = Nothing
        Dim LastCompChecked As Object = Nothing
        Dim IgnoreSwitchReverse As Boolean = False

        Dim NonTraversablesRightTrack As Track = Nothing
        Dim NonTraversablesLeftTrack As Track = Nothing
        Dim NonTraversablesTrack As Track = Nothing

        Try
            If IsPSSType Then
                EntrySignal = gPermissionPassRedEntrySignal
            Else
                'This Condition was added to statisfy CP-RMT 836. 
                For Each Comp In CollectionToBeReaaranged
                    If Comp.GetType.ToString = gTrackComponentName Then
                        If Comp.NonTransverable Then
                            NonTraversablesTrack = Comp
                            Exit For
                        End If
                    End If
                Next
                Comp = Nothing
                If NonTraversablesTrack IsNot Nothing Then
                    Comp = FindCompFromGUID(NonTraversablesTrack.RightLink)
                    For i As Integer = 0 To 5
                        If Comp Is Nothing Then
                            Exit For
                        End If

                        If Comp.GetType.ToString = gTrackComponentName Then
                            NonTraversablesRightTrack = Comp
                        Else
                            Comp = FindCompFromGUID(Comp.RightLink)
                        End If
                    Next
                    Comp = Nothing
                    Comp = FindCompFromGUID(NonTraversablesTrack.LeftLink)
                    For i As Integer = 0 To 5
                        If Comp Is Nothing Then Exit For
                        If Comp.GetType.ToString = gTrackComponentName Then
                            NonTraversablesLeftTrack = Comp
                        Else
                            Comp = FindCompFromGUID(Comp.LeftLink)
                        End If
                    Next
                    If NonTraversablesLeftTrack IsNot Nothing AndAlso NonTraversablesLeftTrack IsNot Nothing Then
                        If CollectionToBeReaaranged.Contains(NonTraversablesLeftTrack) AndAlso CollectionToBeReaaranged.Contains(NonTraversablesRightTrack) Then
                            IgnoreSwitchReverse = True
                        End If
                    End If
                End If
            End If

            FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(CollectionToBeReaaranged)
            If FirstTrack.OsComponentCollection IsNot Nothing AndAlso FirstTrack.OsComponentCollection.Count > 0 Then
                Comp = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(FirstTrack)
                If Comp IsNot Nothing Then If Not EndOSSwitchCollection.Contains(Comp) Then EndOSSwitchCollection.Add(Comp)
                Comp = Nothing
                FirstTrack = Nothing
            Else
                FirstTrack = Nothing
            End If
            FirstTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(CollectionToBeReaaranged)
            If FirstTrack.OsComponentCollection IsNot Nothing AndAlso FirstTrack.OsComponentCollection.Count > 0 Then
                Comp = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(FirstTrack)
                If Comp IsNot Nothing Then If Not EndOSSwitchCollection.Contains(Comp) Then EndOSSwitchCollection.Add(Comp)
                Comp = Nothing
                FirstTrack = Nothing
            Else
                FirstTrack = Nothing
            End If

            If Not IsPSSType Then

            End If

            Dim tempArray As New Collection
            For Each Comp In CollectionToBeReaaranged
                If Comp.GetType.ToString = gTrackComponentName Then
                    tempArray.Add(Comp)
                End If
            Next

            SortCollection(tempArray, "LeftLimitMilePost", False)
            For Each Comp In tempArray
                SystemEvent("TRACING COLLECTIONTOBEREAARANGED :" & Comp.GetType.ToString & Space(2) & Comp.Guid)
            Next



            'Exit Function

            For Each Comp In CollectionToBeReaaranged
                SystemEvent("TRACING COLLECTIONTOBEREAARANGED :" & Comp.GetType.ToString & Space(2) & Comp.Guid)
                RightObject = Nothing
                LeftObject = Nothing
                If Comp.RightLink > 0 Then
                    RightObject = FindCompFromGUID(Comp.RightLink)
                End If
                If Comp.LeftLink > 0 Then
                    LeftObject = FindCompFromGUID(Comp.LeftLink)
                End If

                If RightObject Is Nothing Then
                    SearchDirection = "RIGHT"
                    StartTrack = Comp
                ElseIf LeftObject Is Nothing Then
                    SearchDirection = "LEFT"
                    StartTrack = Comp
                End If
                If StartTrack IsNot Nothing Then
                    Exit For
                End If


                If RightObject.GetType.ToString = gSignalComponentName Then
                    If Not CollectionToBeReaaranged.Contains(RightObject) Then
                        RightObject = FindCompFromGUID(RightObject.RightLink)
                    End If
                End If
                If LeftObject.GetType.ToString = gSignalComponentName Then
                    If Not CollectionToBeReaaranged.Contains(LeftObject) Then
                        LeftObject = FindCompFromGUID(LeftObject.LeftLink)
                    End If
                End If

                If Not CollectionToBeReaaranged.Contains(RightObject) Then
                    SearchDirection = "LEFT"
                    Dim overrideReversePos As Boolean = False
                    If Comp.GetType.ToString = gSignalComponentName Then
                        LastCompChecked = Comp
                        If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                        Comp = FindCompFromGUID(Comp.LeftLink)
                        If IsPSSType AndAlso LastCompChecked Is EntrySignal Then
                            If Comp.NonTransverable Then overrideReversePos = True : IgnoreSwitchReverse = True
                        End If
                    End If
                    RightTrack = Comp
                    If Comp.NonTransverable AndAlso Not overrideReversePos Then
                        If Comp.OsComponentCollection IsNot Nothing AndAlso Comp.OsComponentCollection.Count > 0 Then
                            Dim OSSwitch As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(Comp)

                            If Not OSSwitch Is Nothing Then
                                SwitchRightAltTrack = Nothing
                                SwitchLeftAltTrack = Nothing
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchRightTrack = FindCompFromGUID(OSSwitch.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(OSSwitch.LeftLink)
                                If OSSwitch.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.RightAltLink)
                                If OSSwitch.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.LeftAltLink)
                                If EndOSSwitchCollection.Contains(OSSwitch) Then EndOSSwitchCollection.Remove(OSSwitch)
                                For Each OsComp In RightTrack.OsComponentCollection
                                    If OsComp.GetType.ToString = gTrackComponentName Then
                                        If OsComp IsNot SwitchLeftTrack AndAlso OsComp IsNot SwitchRightTrack AndAlso OsComp IsNot SwitchRightAltTrack Then
                                            FirstTrack = OsComp
                                        End If
                                    End If
                                Next
                                If FirstTrack Is Nothing Then FirstTrack = SwitchRightAltTrack
                                If FirstTrack Is Nothing Then FirstTrack = Comp
                                StartTrack = FirstTrack
                                OSSwitch = Nothing
                                SwitchRightAltTrack = Nothing
                                SwitchLeftAltTrack = Nothing
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing

                            Else
                                StartTrack = Comp
                            End If

                        End If
                    Else
                        StartTrack = Comp
                    End If
                ElseIf Not CollectionToBeReaaranged.Contains(LeftObject) Then
                    SearchDirection = "RIGHT"
                    Dim overrideReversePos As Boolean = False
                    If Comp.GetType.ToString = gSignalComponentName Then
                        LastCompChecked = Comp
                        If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                        Comp = FindCompFromGUID(Comp.RightLink)
                        If IsPSSType AndAlso LastCompChecked Is EntrySignal Then
                            If Comp.NonTransverable Then overrideReversePos = True : IgnoreSwitchReverse = True
                        End If
                    End If
                    RightTrack = Comp



                    If Comp.NonTransverable AndAlso Not overrideReversePos Then
                        If Comp.OsComponentCollection IsNot Nothing AndAlso Comp.OsComponentCollection.Count > 0 Then
                            Dim OSSwitch As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(Comp)

                            If Not OSSwitch Is Nothing Then
                                SwitchRightAltTrack = Nothing
                                SwitchLeftAltTrack = Nothing
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing
                                SwitchRightTrack = FindCompFromGUID(OSSwitch.RightLink)
                                SwitchLeftTrack = FindCompFromGUID(OSSwitch.LeftLink)
                                If OSSwitch.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.RightAltLink)
                                If OSSwitch.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.LeftAltLink)
                                If EndOSSwitchCollection.Contains(OSSwitch) Then EndOSSwitchCollection.Remove(OSSwitch)
                                For Each OsComp In RightTrack.OsComponentCollection
                                    If OsComp.GetType.ToString = gTrackComponentName Then
                                        If OsComp IsNot SwitchLeftTrack AndAlso OsComp IsNot SwitchRightTrack AndAlso OsComp IsNot SwitchRightAltTrack Then
                                            FirstTrack = OsComp
                                        End If
                                    End If
                                Next
                                If FirstTrack Is Nothing Then FirstTrack = SwitchRightAltTrack
                                If FirstTrack Is Nothing Then FirstTrack = Comp
                                StartTrack = FirstTrack
                                OSSwitch = Nothing
                                SwitchRightAltTrack = Nothing
                                SwitchLeftAltTrack = Nothing
                                SwitchRightTrack = Nothing
                                SwitchLeftTrack = Nothing

                            Else
                                StartTrack = Comp
                            End If

                        End If
                    Else
                        StartTrack = Comp
                    End If


                End If
                If StartTrack IsNot Nothing Then Exit For
            Next
            SearchDirection = "RIGHT"
            StartTrack = tempArray(1)
            Comp = Nothing
            If SearchDirection.Trim = "" AndAlso StartTrack Is Nothing Then
            Else
                Select Case SearchDirection
                    Case "RIGHT"
                        If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
                        Comp = FindCompFromGUID(StartTrack.RightLink)
                        For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
                            If CollectionToBeReaaranged.Contains(Comp) Then
                                If Comp.GetType.ToString = gSwitchComponentName Then
                                    Dim EOFCollection As Boolean = False
                                    Dim IsSwitchReversed As Boolean = False
                                    Dim LastComp As Object = Nothing
                                    Dim SW As Switch = Comp
                                    If SW.TypeOfSwitch = "SPM ONLY" OrElse SW.TypeOfSwitch = "DERAIL" Then
                                        If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                        Comp = FindCompFromGUID(Comp.RightLink)
                                    Else
                                        If EndOSSwitchCollection.Contains(SW) Then EOFCollection = True

                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchRightAltTrack = Nothing
                                        SwitchLeftAltTrack = Nothing
                                        SwitchComps = Nothing
                                        SwitchRightTrack = FindCompFromGUID(SW.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
                                        'If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        'If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        If SW.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)

                                        If Not EOFCollection Then
                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then
                                                If SwitchLeftTrack IsNot Nothing Then ArrangedCollection.Add(SwitchLeftTrack)
                                                If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                            Else
                                                If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If SwitchComps IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next
                                                If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                            End If


                                        Else
                                            'IsSwitchReversed
                                            If SwitchRightTrack.NonTransverable Or SwitchLeftTrack.NonTransverable Then IsSwitchReversed = True
                                            If IgnoreSwitchReverse Then IsSwitchReversed = False
                                            If Not IsSwitchReversed Then
                                                If Not ArrangedCollection.Contains(SwitchLeftTrack) Then
                                                    If SwitchLeftTrack IsNot Nothing Then ArrangedCollection.Add(SwitchLeftTrack)
                                                    If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                    Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                                Else
                                                    If SwitchRightAltTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
                                                    For Each SwitchComps In SW.OsComponentCollection
                                                        If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                            If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                                If SwitchComps IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                            End If
                                                        End If
                                                    Next
                                                    If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                    Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                                End If
                                            Else
                                                If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                If SwitchLeftTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                If SwitchRightAltTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If SwitchComps IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchComps) Then
                                                                ArrangedCollection.Add(SwitchComps)
                                                                LastComp = SwitchComps
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                                If LastComp IsNot Nothing Then
                                                    Comp = FindCompFromGUID(LastComp.RightLink)
                                                Else
                                                    Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
                                                End If

                                            End If

                                        End If



                                    End If
                                Else
                                    If Comp IsNot Nothing AndAlso Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                    Comp = FindCompFromGUID(Comp.RightLink)
                                End If
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        Else
                                            Exit For
                                        End If
                                    End If
                                End If
                            Else
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        Else
                                            Exit For
                                        End If
                                    End If
                                Else
                                    Exit For
                                End If

                            End If
                        Next
                    Case "LEFT"
                        If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
                        Comp = FindCompFromGUID(StartTrack.LeftLink)
                        For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
                            If CollectionToBeReaaranged.Contains(Comp) Then
                                If Comp.GetType.ToString = gSwitchComponentName Then
                                    Dim EOFCollection As Boolean = False
                                    Dim IsSwitchReversed As Boolean = False
                                    Dim LastComp As Object = Nothing
                                    Dim SW As Switch = Comp
                                    If SW.TypeOfSwitch = "SPM ONLY" Then
                                        If Comp IsNot Nothing AndAlso Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                        Comp = FindCompFromGUID(Comp.LeftLink)
                                    Else
                                        If EndOSSwitchCollection.Contains(SW) Then EOFCollection = True
                                        SwitchRightTrack = Nothing
                                        SwitchLeftTrack = Nothing
                                        SwitchRightAltTrack = Nothing
                                        SwitchLeftAltTrack = Nothing
                                        SwitchComps = Nothing
                                        SwitchRightTrack = FindCompFromGUID(SW.RightLink)
                                        SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
                                        'If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        'If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If SW.RightAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.RightAltLink)
                                        If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
                                        If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)

                                        If Not EOFCollection Then
                                            If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then
                                                ArrangedCollection.Add(SwitchRightTrack)
                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)
                                            Else

                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If SwitchComps IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                        End If
                                                    End If
                                                Next
                                                If SwitchLeftAltTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                If SwitchLeftTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
                                            End If


                                        Else
                                            If SwitchRightTrack.NonTransverable Or SwitchLeftTrack.NonTransverable Then IsSwitchReversed = True
                                            If IgnoreSwitchReverse Then IsSwitchReversed = False
                                            If Not IsSwitchReversed Then
                                                If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then
                                                    ArrangedCollection.Add(SwitchRightTrack)
                                                    If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                    Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)
                                                Else

                                                    For Each SwitchComps In SW.OsComponentCollection
                                                        If SwitchComps.GetType.ToString = gTrackComponentName Then
                                                            If SwitchComps IsNot Nothing AndAlso SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                                If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
                                                            End If
                                                        End If
                                                    Next
                                                    If SwitchLeftAltTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                    If SwitchLeftTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                    Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
                                                End If
                                            Else

                                                If SwitchRightTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
                                                If SwitchLeftTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
                                                If SwitchLeftAltTrack IsNot Nothing AndAlso Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
                                                For Each SwitchComps In SW.OsComponentCollection
                                                    If SwitchComps IsNot Nothing AndAlso SwitchComps.GetType.ToString = gTrackComponentName Then
                                                        If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
                                                            If Not ArrangedCollection.Contains(SwitchComps) Then
                                                                ArrangedCollection.Add(SwitchComps)
                                                                LastComp = SwitchComps
                                                            End If
                                                        End If
                                                    End If
                                                Next
                                                If LastComp IsNot Nothing Then
                                                    Comp = FindCompFromGUID(LastComp.LeftLink)
                                                Else
                                                    Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
                                                End If

                                            End If




                                        End If





                                    End If
                                Else
                                    If Comp IsNot Nothing AndAlso Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
                                    Comp = FindCompFromGUID(Comp.LeftLink)
                                End If
                                If Comp Is Nothing Then Exit For
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        LeftObject = Nothing
                                        LeftObject = FindCompFromGUID(Comp.LeftLink)
                                        If CollectionToBeReaaranged.Contains(LeftObject) Then
                                            Comp = LeftObject
                                        Else
                                            Exit For
                                        End If
                                    Else
                                        Exit For
                                    End If
                                End If
                            Else
                                If Comp.GetType.ToString = gSignalComponentName Then
                                    If Not CollectionToBeReaaranged.Contains(Comp) Then
                                        RightObject = Nothing
                                        RightObject = FindCompFromGUID(Comp.RightLink)
                                        If CollectionToBeReaaranged.Contains(RightObject) Then
                                            Comp = RightObject
                                        Else
                                            Exit For
                                        End If
                                    Else
                                        Exit For
                                    End If
                                Else
                                    Exit For
                                End If

                                'Exit For
                            End If
                        Next
                End Select
            End If

            If gDebug Then
                Comp = Nothing
                For Each Comp In ArrangedCollection
                    If Comp IsNot Nothing Then SystemEvent(Comp.GetType.ToString & Space(2) & Comp.Guid)
                Next
            End If

            Return ArrangedCollection

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            LeftObject = Nothing
            RightObject = Nothing
            LeftTrack = Nothing
            RightTrack = Nothing
            Comp = Nothing
            SwitchRightTrack = Nothing
            SwitchLeftTrack = Nothing
            SwitchRightAltTrack = Nothing
            SwitchLeftAltTrack = Nothing
            SwitchComps = Nothing

            NonTraversablesRightTrack = Nothing
            NonTraversablesLeftTrack = Nothing
            NonTraversablesTrack = Nothing
        End Try
        Return Nothing
    End Function

    'Public Function RearrangeCompCollection(ByVal CollectionToBeReaaranged As ArrayList, ByVal IsPSSType As Boolean) As ArrayList
    '    Dim FirstTrack As Track = Nothing
    '    'Dim LastTrack As Track = Nothing
    '    Dim RightTrack As Track = Nothing
    '    Dim LeftTrack As Track = Nothing
    '    Dim Comp As Object = Nothing
    '    Dim SearchDirection As String = ""
    '    Dim StartTrack As Object = Nothing
    '    Dim ArrangedCollection As New ArrayList
    '    Dim RightObject As Object = Nothing
    '    Dim LeftObject As Object = Nothing

    '    Dim SwitchRightTrack As Track = Nothing
    '    Dim SwitchLeftTrack As Track = Nothing
    '    Dim SwitchLeftAltTrack As Track = Nothing
    '    Dim SwitchRightAltTrack As Track = Nothing
    '    Dim SwitchComps As Object = Nothing
    '    Dim OsComp As Object = Nothing

    '    Dim EndOSSwitchCollection As New ArrayList
    '    Dim EntrySignal As Signal = Nothing
    '    Dim LastCompChecked As Object = Nothing
    '    Dim IgnoreSwitchReverse As Boolean = False

    '    Dim NonTraversablesRightTrack As Track = Nothing
    '    Dim NonTraversablesLeftTrack As Track = Nothing
    '    Dim NonTraversablesTrack As Track = Nothing

    '    Try
    '        If IsPSSType Then
    '            EntrySignal = gPermissionPassRedEntrySignal
    '        Else
    '            'This Condition was added to statisfy CP-RMT 836. 
    '            For Each Comp In CollectionToBeReaaranged
    '                If Comp.GetType.ToString = gTrackComponentName Then
    '                    If Comp.NonTransverable Then
    '                        NonTraversablesTrack = Comp
    '                        Exit For
    '                    End If
    '                End If
    '            Next
    '            Comp = Nothing
    '            If NonTraversablesTrack IsNot Nothing Then
    '                Comp = FindCompFromGUID(NonTraversablesTrack.RightLink)
    '                For i As Integer = 0 To 5
    '                    If Comp Is Nothing Then
    '                        Exit For
    '                    End If

    '                    If Comp.GetType.ToString = gTrackComponentName Then
    '                        NonTraversablesRightTrack = Comp
    '                    Else
    '                        Comp = FindCompFromGUID(Comp.RightLink)
    '                    End If
    '                Next
    '                Comp = Nothing
    '                Comp = FindCompFromGUID(NonTraversablesTrack.LeftLink)
    '                For i As Integer = 0 To 5
    '                    If Comp Is Nothing Then Exit For
    '                    If Comp.GetType.ToString = gTrackComponentName Then
    '                        NonTraversablesLeftTrack = Comp
    '                    Else
    '                        Comp = FindCompFromGUID(Comp.LeftLink)
    '                    End If
    '                Next
    '                If NonTraversablesLeftTrack IsNot Nothing AndAlso NonTraversablesLeftTrack IsNot Nothing Then
    '                    If CollectionToBeReaaranged.Contains(NonTraversablesLeftTrack) AndAlso CollectionToBeReaaranged.Contains(NonTraversablesRightTrack) Then
    '                        IgnoreSwitchReverse = True
    '                    End If
    '                End If
    '            End If
    '        End If

    '        FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(CollectionToBeReaaranged)
    '        If FirstTrack.OsComponentCollection IsNot Nothing AndAlso FirstTrack.OsComponentCollection.Count > 0 Then
    '            Dim SWArr As New ArrayList
    '            SWArr = TMDS.SystemUtilities.FetchData.GetMyOSSwitches(FirstTrack)
    '            If SWArr.Count > 1 Then
    '                Dim FromMP As Double = 0
    '                Dim ToMP As Double = 0
    '                Dim SWSortAscending As Boolean = True
    '                Select Case gSearchDirection
    '                    Case "RIGHT"
    '                        FromMP = FirstTrack.RightLimitMilePost
    '                        ToMP = FirstTrack.LeftLimitMilePost
    '                    Case Else
    '                        FromMP = FirstTrack.LeftLimitMilePost
    '                        ToMP = FirstTrack.RightLimitMilePost
    '                End Select
    '                If FromMP > ToMP Then
    '                    SWSortAscending = False
    '                Else
    '                    SWSortAscending = True
    '                End If
    '                TMDS.SystemUtilities.FetchData.SortSwitchesByMilePost(SWArr, SWSortAscending)
    '                Comp = SWArr(0)
    '                If Comp IsNot Nothing Then If Not EndOSSwitchCollection.Contains(Comp) Then EndOSSwitchCollection.Add(Comp)
    '                Comp = Nothing
    '                FirstTrack = Nothing
    '            Else
    '                Comp = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(FirstTrack)
    '                If Comp IsNot Nothing Then If Not EndOSSwitchCollection.Contains(Comp) Then EndOSSwitchCollection.Add(Comp)
    '                Comp = Nothing
    '                FirstTrack = Nothing
    '            End If
    '        Else
    '            FirstTrack = Nothing
    '        End If
    '        FirstTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(CollectionToBeReaaranged)
    '        If FirstTrack.OsComponentCollection IsNot Nothing AndAlso FirstTrack.OsComponentCollection.Count > 0 Then
    '            Comp = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(FirstTrack)
    '            If Comp IsNot Nothing Then If Not EndOSSwitchCollection.Contains(Comp) Then EndOSSwitchCollection.Add(Comp)
    '            Comp = Nothing
    '            FirstTrack = Nothing
    '        Else
    '            FirstTrack = Nothing
    '        End If


    '        Dim tempArray As New Collection
    '        For Each Comp In CollectionToBeReaaranged
    '            Dim compSort As New clsCompSort
    '            Select Case Comp.GetType.ToString
    '                Case gTrackComponentName
    '                    With compSort
    '                        .myUID = Comp.GUID
    '                        .MilePost = Comp.LeftLimitMilePost
    '                        .myType = gTrackComponentName
    '                        tempArray.Add(compSort)
    '                    End With
    '                Case gSwitchComponentName
    '                    With compSort
    '                        .myUID = Comp.GUID
    '                        .MilePost = Comp.MilePost
    '                        .myType = gSwitchComponentName
    '                        tempArray.Add(compSort)
    '                    End With
    '            End Select
    '        Next

    '        SortCollection(tempArray, "MilePost", False)
    '        'For Each Comp In tempArray
    '        '    SystemEvent("TRACING COLLECTIONTOBEREAARANGED :" & Comp.GetType.ToString & Space(2) & Comp.Guid)
    '        'Next
    '        Dim TempCompCollection As New ArrayList
    '        Dim compSort1 As clsCompSort
    '        For Each compSort1 In tempArray
    '            For Each Comp In CollectionToBeReaaranged
    '                If Comp.GUID = compSort1.myUID Then
    '                    TempCompCollection.Add(Comp)
    '                    Exit For
    '                End If
    '            Next
    '        Next
    '        For Each Comp In TempCompCollection
    '            SystemEvent("TRACING COLLECTIONTOBEREAARANGED :" & Comp.GetType.ToString & Space(2) & Comp.Guid)
    '        Next

    '        For Each Comp In CollectionToBeReaaranged
    '            SystemEvent("TRACING COLLECTIONTOBEREAARANGED :" & Comp.GetType.ToString & Space(2) & Comp.Guid)
    '            RightObject = Nothing
    '            LeftObject = Nothing
    '            If Comp.RightLink > 0 Then
    '                RightObject = FindCompFromGUID(Comp.RightLink)
    '            End If
    '            If Comp.LeftLink > 0 Then
    '                LeftObject = FindCompFromGUID(Comp.LeftLink)
    '            End If

    '            If RightObject Is Nothing Then
    '                SearchDirection = "RIGHT"
    '                StartTrack = Comp
    '            ElseIf LeftObject Is Nothing Then
    '                SearchDirection = "LEFT"
    '                StartTrack = Comp
    '            End If
    '            If StartTrack IsNot Nothing Then
    '                Exit For
    '            End If


    '            If RightObject.GetType.ToString = gSignalComponentName Then
    '                If Not CollectionToBeReaaranged.Contains(RightObject) Then
    '                    RightObject = FindCompFromGUID(RightObject.RightLink)
    '                End If
    '            End If
    '            If LeftObject.GetType.ToString = gSignalComponentName Then
    '                If Not CollectionToBeReaaranged.Contains(LeftObject) Then
    '                    LeftObject = FindCompFromGUID(LeftObject.LeftLink)
    '                End If
    '            End If

    '            If Not CollectionToBeReaaranged.Contains(RightObject) Then
    '                SearchDirection = "LEFT"
    '                Dim overrideReversePos As Boolean = False
    '                If Comp.GetType.ToString = gSignalComponentName Then
    '                    LastCompChecked = Comp
    '                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                    Comp = FindCompFromGUID(Comp.LeftLink)
    '                    If IsPSSType AndAlso LastCompChecked Is EntrySignal Then
    '                        If Comp.NonTransverable Then overrideReversePos = True : IgnoreSwitchReverse = True
    '                    End If
    '                End If
    '                RightTrack = Comp
    '                If Comp.NonTransverable AndAlso Not overrideReversePos Then
    '                    If Comp.OsComponentCollection IsNot Nothing AndAlso Comp.OsComponentCollection.Count > 0 Then
    '                        Dim OSSwitch As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(Comp)

    '                        If Not OSSwitch Is Nothing Then
    '                            SwitchRightAltTrack = Nothing
    '                            SwitchLeftAltTrack = Nothing
    '                            SwitchRightTrack = Nothing
    '                            SwitchLeftTrack = Nothing
    '                            SwitchRightTrack = FindCompFromGUID(OSSwitch.RightLink)
    '                            SwitchLeftTrack = FindCompFromGUID(OSSwitch.LeftLink)
    '                            If OSSwitch.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.RightAltLink)
    '                            If OSSwitch.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.LeftAltLink)
    '                            If EndOSSwitchCollection.Contains(OSSwitch) Then EndOSSwitchCollection.Remove(OSSwitch)
    '                            For Each OsComp In RightTrack.OsComponentCollection
    '                                If OsComp.GetType.ToString = gTrackComponentName Then
    '                                    If OsComp IsNot SwitchLeftTrack AndAlso OsComp IsNot SwitchRightTrack AndAlso OsComp IsNot SwitchRightAltTrack Then
    '                                        FirstTrack = OsComp
    '                                    End If
    '                                End If
    '                            Next
    '                            If FirstTrack Is Nothing Then FirstTrack = SwitchRightAltTrack
    '                            If FirstTrack Is Nothing Then FirstTrack = Comp
    '                            StartTrack = FirstTrack
    '                            OSSwitch = Nothing
    '                            SwitchRightAltTrack = Nothing
    '                            SwitchLeftAltTrack = Nothing
    '                            SwitchRightTrack = Nothing
    '                            SwitchLeftTrack = Nothing

    '                        Else
    '                            StartTrack = Comp
    '                        End If

    '                    End If
    '                Else
    '                    StartTrack = Comp
    '                End If
    '            ElseIf Not CollectionToBeReaaranged.Contains(LeftObject) Then
    '                SearchDirection = "RIGHT"
    '                Dim overrideReversePos As Boolean = False
    '                If Comp.GetType.ToString = gSignalComponentName Then
    '                    LastCompChecked = Comp
    '                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                    Comp = FindCompFromGUID(Comp.RightLink)
    '                    If IsPSSType AndAlso LastCompChecked Is EntrySignal Then
    '                        If Comp.NonTransverable Then overrideReversePos = True : IgnoreSwitchReverse = True
    '                    End If
    '                End If
    '                RightTrack = Comp



    '                If Comp.NonTransverable AndAlso Not overrideReversePos Then
    '                    If Comp.OsComponentCollection IsNot Nothing AndAlso Comp.OsComponentCollection.Count > 0 Then
    '                        Dim OSSwitch As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromOSList(Comp)

    '                        If Not OSSwitch Is Nothing Then
    '                            SwitchRightAltTrack = Nothing
    '                            SwitchLeftAltTrack = Nothing
    '                            SwitchRightTrack = Nothing
    '                            SwitchLeftTrack = Nothing
    '                            SwitchRightTrack = FindCompFromGUID(OSSwitch.RightLink)
    '                            SwitchLeftTrack = FindCompFromGUID(OSSwitch.LeftLink)
    '                            If OSSwitch.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.RightAltLink)
    '                            If OSSwitch.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(OSSwitch.LeftAltLink)
    '                            If EndOSSwitchCollection.Contains(OSSwitch) Then EndOSSwitchCollection.Remove(OSSwitch)
    '                            For Each OsComp In RightTrack.OsComponentCollection
    '                                If OsComp.GetType.ToString = gTrackComponentName Then
    '                                    If OsComp IsNot SwitchLeftTrack AndAlso OsComp IsNot SwitchRightTrack AndAlso OsComp IsNot SwitchRightAltTrack Then
    '                                        FirstTrack = OsComp
    '                                    End If
    '                                End If
    '                            Next
    '                            If FirstTrack Is Nothing Then FirstTrack = SwitchRightAltTrack
    '                            If FirstTrack Is Nothing Then FirstTrack = Comp
    '                            StartTrack = FirstTrack
    '                            OSSwitch = Nothing
    '                            SwitchRightAltTrack = Nothing
    '                            SwitchLeftAltTrack = Nothing
    '                            SwitchRightTrack = Nothing
    '                            SwitchLeftTrack = Nothing

    '                        Else
    '                            StartTrack = Comp
    '                        End If

    '                    End If
    '                Else
    '                    StartTrack = Comp
    '                End If


    '            End If
    '            If StartTrack IsNot Nothing Then Exit For
    '        Next
    '        SearchDirection = "RIGHT"
    '        'StartTrack = tempArray(1)
    '        StartTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(TempCompCollection)
    '        Comp = Nothing
    '        If SearchDirection.Trim = "" AndAlso StartTrack Is Nothing Then
    '        Else
    '            Select Case SearchDirection
    '                Case "RIGHT"
    '                    If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
    '                    Comp = FindCompFromGUID(StartTrack.RightLink)
    '                    For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
    '                        If CollectionToBeReaaranged.Contains(Comp) Then
    '                            If Comp.GetType.ToString = gSwitchComponentName Then
    '                                Dim EOFCollection As Boolean = False
    '                                Dim IsSwitchReversed As Boolean = False
    '                                Dim LastComp As Object = Nothing
    '                                Dim SW As Switch = Comp
    '                                If SW.TypeOfSwitch = "SPM ONLY" Then
    '                                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                                    Comp = FindCompFromGUID(Comp.RightLink)
    '                                Else
    '                                    If EndOSSwitchCollection.Contains(SW) Then EOFCollection = True

    '                                    SwitchRightTrack = Nothing
    '                                    SwitchLeftTrack = Nothing
    '                                    SwitchRightAltTrack = Nothing
    '                                    SwitchLeftAltTrack = Nothing
    '                                    SwitchComps = Nothing
    '                                    SwitchRightTrack = FindCompFromGUID(SW.RightLink)
    '                                    SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
    '                                    'If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
    '                                    'If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
    '                                    If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
    '                                    If SW.LeftAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.LeftAltLink)
    '                                    If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)

    '                                    If Not EOFCollection Then
    '                                        If Not ArrangedCollection.Contains(SwitchLeftTrack) Then
    '                                            ArrangedCollection.Add(SwitchLeftTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                            Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
    '                                        Else
    '                                            If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
    '                                            For Each SwitchComps In SW.OsComponentCollection
    '                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                    If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
    '                                                    End If
    '                                                End If
    '                                            Next
    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                            Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
    '                                        End If


    '                                    Else
    '                                        'IsSwitchReversed
    '                                        If SwitchRightTrack.NonTransverable Or SwitchLeftTrack.NonTransverable Then IsSwitchReversed = True
    '                                        If IgnoreSwitchReverse Then IsSwitchReversed = False
    '                                        If Not IsSwitchReversed Then
    '                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then
    '                                                ArrangedCollection.Add(SwitchLeftTrack)
    '                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
    '                                            Else
    '                                                If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
    '                                                For Each SwitchComps In SW.OsComponentCollection
    '                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                        If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
    '                                                        End If
    '                                                    End If
    '                                                Next
    '                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
    '                                            End If
    '                                        Else
    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchRightAltTrack) Then ArrangedCollection.Add(SwitchRightAltTrack)
    '                                            For Each SwitchComps In SW.OsComponentCollection
    '                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                    If SwitchComps IsNot SwitchRightAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                        If Not ArrangedCollection.Contains(SwitchComps) Then
    '                                                            ArrangedCollection.Add(SwitchComps)
    '                                                            LastComp = SwitchComps
    '                                                        End If
    '                                                    End If
    '                                                End If
    '                                            Next
    '                                            If LastComp IsNot Nothing Then
    '                                                Comp = FindCompFromGUID(LastComp.RightLink)
    '                                            Else
    '                                                Comp = FindCompFromGUID(SwitchRightTrack.RightLink)
    '                                            End If

    '                                        End If

    '                                    End If



    '                                End If
    '                            Else
    '                                If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                                Comp = FindCompFromGUID(Comp.RightLink)
    '                            End If
    '                            If Comp.GetType.ToString = gSignalComponentName Then
    '                                If Not CollectionToBeReaaranged.Contains(Comp) Then
    '                                    RightObject = Nothing
    '                                    RightObject = FindCompFromGUID(Comp.RightLink)
    '                                    If CollectionToBeReaaranged.Contains(RightObject) Then
    '                                        Comp = RightObject
    '                                    Else
    '                                        Exit For
    '                                    End If
    '                                End If
    '                            End If
    '                        Else
    '                            If Comp.GetType.ToString = gSignalComponentName Then
    '                                If Not CollectionToBeReaaranged.Contains(Comp) Then
    '                                    RightObject = Nothing
    '                                    RightObject = FindCompFromGUID(Comp.RightLink)
    '                                    If CollectionToBeReaaranged.Contains(RightObject) Then
    '                                        Comp = RightObject
    '                                    Else
    '                                        Exit For
    '                                    End If
    '                                End If
    '                            Else
    '                                Exit For
    '                            End If

    '                        End If
    '                    Next
    '                Case "LEFT"
    '                    If Not ArrangedCollection.Contains(StartTrack) Then ArrangedCollection.Add(StartTrack)
    '                    Comp = FindCompFromGUID(StartTrack.LeftLink)
    '                    For I As Integer = 0 To CollectionToBeReaaranged.Count + 5
    '                        If CollectionToBeReaaranged.Contains(Comp) Then
    '                            If Comp.GetType.ToString = gSwitchComponentName Then
    '                                Dim EOFCollection As Boolean = False
    '                                Dim IsSwitchReversed As Boolean = False
    '                                Dim LastComp As Object = Nothing
    '                                Dim SW As Switch = Comp
    '                                If SW.TypeOfSwitch = "SPM ONLY" Then
    '                                    If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                                    Comp = FindCompFromGUID(Comp.LeftLink)
    '                                Else
    '                                    If EndOSSwitchCollection.Contains(SW) Then EOFCollection = True
    '                                    SwitchRightTrack = Nothing
    '                                    SwitchLeftTrack = Nothing
    '                                    SwitchRightAltTrack = Nothing
    '                                    SwitchLeftAltTrack = Nothing
    '                                    SwitchComps = Nothing
    '                                    SwitchRightTrack = FindCompFromGUID(SW.RightLink)
    '                                    SwitchLeftTrack = FindCompFromGUID(SW.LeftLink)
    '                                    'If SW.RightAltLink > 0 Then SwitchRightAltTrack = FindCompFromGUID(SW.RightAltLink)
    '                                    'If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
    '                                    If SW.RightAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.RightAltLink)
    '                                    If SW.LeftAltLink > 0 Then SwitchLeftAltTrack = FindCompFromGUID(SW.LeftAltLink)
    '                                    If Not ArrangedCollection.Contains(SW) Then ArrangedCollection.Add(SW)

    '                                    If Not EOFCollection Then
    '                                        If Not ArrangedCollection.Contains(SwitchRightTrack) Then
    '                                            ArrangedCollection.Add(SwitchRightTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                            Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)
    '                                        Else

    '                                            For Each SwitchComps In SW.OsComponentCollection
    '                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                    If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                        If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
    '                                                    End If
    '                                                End If
    '                                            Next
    '                                            If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
    '                                            Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
    '                                        End If


    '                                    Else
    '                                        If SwitchRightTrack.NonTransverable Or SwitchLeftTrack.NonTransverable Then IsSwitchReversed = True
    '                                        If IgnoreSwitchReverse Then IsSwitchReversed = False
    '                                        If Not IsSwitchReversed Then
    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then
    '                                                ArrangedCollection.Add(SwitchRightTrack)
    '                                                If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                                Comp = FindCompFromGUID(SwitchRightTrack.LeftLink)
    '                                            Else

    '                                                For Each SwitchComps In SW.OsComponentCollection
    '                                                    If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                        If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                            If Not ArrangedCollection.Contains(SwitchComps) Then ArrangedCollection.Add(SwitchComps)
    '                                                        End If
    '                                                    End If
    '                                                Next
    '                                                If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
    '                                                If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
    '                                                Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
    '                                            End If
    '                                        Else

    '                                            If Not ArrangedCollection.Contains(SwitchRightTrack) Then ArrangedCollection.Add(SwitchRightTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchLeftTrack) Then ArrangedCollection.Add(SwitchLeftTrack)
    '                                            If Not ArrangedCollection.Contains(SwitchLeftAltTrack) Then ArrangedCollection.Add(SwitchLeftAltTrack)
    '                                            For Each SwitchComps In SW.OsComponentCollection
    '                                                If SwitchComps.GetType.ToString = gTrackComponentName Then
    '                                                    If SwitchComps IsNot SwitchLeftAltTrack AndAlso SwitchComps IsNot SwitchRightTrack AndAlso SwitchComps IsNot SwitchLeftTrack Then
    '                                                        If Not ArrangedCollection.Contains(SwitchComps) Then
    '                                                            ArrangedCollection.Add(SwitchComps)
    '                                                            LastComp = SwitchComps
    '                                                        End If
    '                                                    End If
    '                                                End If
    '                                            Next
    '                                            If LastComp IsNot Nothing Then
    '                                                Comp = FindCompFromGUID(LastComp.LeftLink)
    '                                            Else
    '                                                Comp = FindCompFromGUID(SwitchLeftTrack.LeftLink)
    '                                            End If

    '                                        End If




    '                                    End If





    '                                End If
    '                            Else
    '                                If Not ArrangedCollection.Contains(Comp) Then ArrangedCollection.Add(Comp)
    '                                Comp = FindCompFromGUID(Comp.LeftLink)
    '                            End If
    '                            If Comp Is Nothing Then Exit For
    '                            If Comp.GetType.ToString = gSignalComponentName Then
    '                                If Not CollectionToBeReaaranged.Contains(Comp) Then
    '                                    LeftObject = Nothing
    '                                    LeftObject = FindCompFromGUID(Comp.LeftLink)
    '                                    If CollectionToBeReaaranged.Contains(LeftObject) Then
    '                                        Comp = LeftObject
    '                                    Else
    '                                        Exit For
    '                                    End If
    '                                Else
    '                                    Exit For
    '                                End If
    '                            End If
    '                        Else
    '                            If Comp.GetType.ToString = gSignalComponentName Then
    '                                If Not CollectionToBeReaaranged.Contains(Comp) Then
    '                                    RightObject = Nothing
    '                                    RightObject = FindCompFromGUID(Comp.RightLink)
    '                                    If CollectionToBeReaaranged.Contains(RightObject) Then
    '                                        Comp = RightObject
    '                                    Else
    '                                        Exit For
    '                                    End If
    '                                Else
    '                                    Exit For
    '                                End If
    '                            Else
    '                                Exit For
    '                            End If

    '                            'Exit For
    '                        End If
    '                    Next
    '            End Select
    '        End If

    '        If gDebug Then
    '            Comp = Nothing
    '            For Each Comp In ArrangedCollection
    '                SystemEvent(Comp.GetType.ToString & Space(2) & Comp.Guid)
    '            Next
    '        End If

    '        Return ArrangedCollection

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    Finally
    '        LeftObject = Nothing
    '        RightObject = Nothing
    '        LeftTrack = Nothing
    '        RightTrack = Nothing
    '        Comp = Nothing
    '        SwitchRightTrack = Nothing
    '        SwitchLeftTrack = Nothing
    '        SwitchRightAltTrack = Nothing
    '        SwitchLeftAltTrack = Nothing
    '        SwitchComps = Nothing

    '        NonTraversablesRightTrack = Nothing
    '        NonTraversablesLeftTrack = Nothing
    '        NonTraversablesTrack = Nothing
    '    End Try
    '    Return Nothing
    'End Function

    Public Function FindAdjacentTrackToMyCollection(ByVal TrackToSearch As Track, ByVal Compcollection As ArrayList) As Track
        Dim ReturnTrack As Track = Nothing
        Dim comp As Object = Nothing
        Dim DirectionToSearch As String = ""
        Dim I As Integer = 0
        Try
            If TrackToSearch.LeftLink = 0 Or TrackToSearch.RightLink = 0 Then Return TrackToSearch

            If TrackToSearch.LeftLink = 0 Then
                comp = FindCompFromGUID(TrackToSearch.RightLink)
                I = 0
                For I = 0 To 10
                    Select Case comp.GetType.ToString
                        Case gTrackComponentName
                            If Compcollection.Contains(comp) Then
                                Return TrackToSearch
                            Else
                                Return comp
                            End If
                        Case gSignalComponentName
                            comp = FindCompFromGUID(comp.RightLink)
                        Case gSwitchComponentName
                            Dim sw As Switch = comp
                            If sw.TypeOfSwitch = "SPM ONLY" Then
                                comp = FindCompFromGUID(TrackToSearch.RightLink)
                            Else
                                If sw.ControlledReverse Then
                                    If sw.RightAltLink > 0 Then
                                        comp = FindCompFromGUID(sw.RightAltLink)
                                    ElseIf sw.LeftAltLink > 0 Then
                                        comp = FindCompFromGUID(sw.LeftAltLink)
                                    End If
                                Else
                                    comp = FindCompFromGUID(sw.RightLink)
                                End If
                                For J As Integer = 0 To 6
                                    If sw.OsComponentCollection.Contains(comp) Then
                                        comp = FindCompFromGUID(TrackToSearch.RightLink)
                                    End If
                                Next
                            End If
                    End Select
                Next
            End If
            If TrackToSearch.RightLink = 0 Then
                comp = FindCompFromGUID(TrackToSearch.LeftLink)
                I = 0
                For I = 0 To 10
                    Select Case comp.GetType.ToString
                        Case gTrackComponentName
                            If Compcollection.Contains(comp) Then
                                Return TrackToSearch
                            Else
                                Return comp
                            End If
                        Case gSignalComponentName
                            comp = FindCompFromGUID(comp.LeftLink)
                        Case gSwitchComponentName
                            Dim sw As Switch = comp
                            If sw.TypeOfSwitch = "SPM ONLY" Then
                                comp = FindCompFromGUID(TrackToSearch.LeftLink)
                            Else
                                If sw.ControlledReverse Then
                                    If sw.RightAltLink > 0 Then
                                        comp = FindCompFromGUID(sw.RightAltLink)
                                    ElseIf sw.LeftAltLink > 0 Then
                                        comp = FindCompFromGUID(sw.LeftAltLink)
                                    End If
                                Else
                                    comp = FindCompFromGUID(sw.LeftLink)
                                End If
                                For J As Integer = 0 To 6
                                    If sw.OsComponentCollection.Contains(comp) Then
                                        comp = FindCompFromGUID(TrackToSearch.LeftLink)
                                    End If
                                Next
                            End If
                    End Select
                Next
            End If

            comp = FindCompFromGUID(TrackToSearch.LeftLink)
            I = 0
            For I = 0 To 10
                Select Case comp.GetType.ToString
                    Case gTrackComponentName
                        If Compcollection.Contains(comp) Then
                            Return TrackToSearch
                        Else
                            Return comp
                        End If
                    Case gSignalComponentName
                        comp = FindCompFromGUID(comp.LeftLink)
                    Case gSwitchComponentName
                        Dim sw As Switch = comp
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            comp = FindCompFromGUID(TrackToSearch.LeftLink)
                        Else
                            If sw.ControlledReverse Then
                                If sw.RightAltLink > 0 Then
                                    comp = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    comp = FindCompFromGUID(sw.LeftAltLink)
                                End If
                            Else
                                comp = FindCompFromGUID(sw.LeftLink)
                            End If
                            For J As Integer = 0 To 6
                                If sw.OsComponentCollection.Contains(comp) Then
                                    comp = FindCompFromGUID(TrackToSearch.LeftLink)
                                End If
                            Next
                        End If
                End Select
            Next
            comp = FindCompFromGUID(TrackToSearch.RightLink)
            I = 0
            For I = 0 To 10
                Select Case comp.GetType.ToString
                    Case gTrackComponentName
                        If Compcollection.Contains(comp) Then
                            Return TrackToSearch
                        Else
                            Return comp
                        End If
                    Case gSignalComponentName
                        comp = FindCompFromGUID(comp.RightLink)
                    Case gSwitchComponentName
                        Dim sw As Switch = comp
                        If sw.TypeOfSwitch = "SPM ONLY" Then
                            comp = FindCompFromGUID(TrackToSearch.RightLink)
                        Else
                            If sw.ControlledReverse Then
                                If sw.RightAltLink > 0 Then
                                    comp = FindCompFromGUID(sw.RightAltLink)
                                ElseIf sw.LeftAltLink > 0 Then
                                    comp = FindCompFromGUID(sw.LeftAltLink)
                                End If
                            Else
                                comp = FindCompFromGUID(sw.RightLink)
                            End If
                            For J As Integer = 0 To 6
                                If sw.OsComponentCollection.Contains(comp) Then
                                    comp = FindCompFromGUID(TrackToSearch.RightLink)
                                End If
                            Next
                        End If
                End Select
            Next


        Catch ex As Exception
            GetErrorDetails(ex)
            Return TrackToSearch
        End Try
        Return TrackToSearch

    End Function

    Public Function FindAllClearedToMySelection(ByVal TrackToSearch As Track, ByVal ComponentsCollection As ArrayList) As ArrayList
        'Dim NextTrack As Track
        Dim NextSignal As Signal = Nothing
        Dim SigCollection As New ArrayList
        Dim MyTrafficDirection As String = ""
        Dim LeftObject As Object = Nothing
        Dim RightObject As Object = Nothing
        Try
            If TrackToSearch.SignalStatOnMe <> Track.SignalStatus.SignalCleared Then Return Nothing
            MyTrafficDirection = TrackToSearch.TrafficDirection
            If MyTrafficDirection.Trim = "" Then Return Nothing
            NextSignal = TMDS.SystemUtilities.FetchData.FindMyControllingSignal(TrackToSearch, TrackToSearch.TrafficDirection)


            For i As Integer = 0 To 100
                Select Case MyTrafficDirection.Trim.ToUpper
                    Case "LEFT"
                        LeftObject = FindCompFromGUID(NextSignal.RightLink)
                        If Not ComponentsCollection.Contains(LeftObject) Then
                            If LeftObject.GetType.ToString = gTrackComponentName Then
                                If LeftObject.TrafficDirection <> "" AndAlso MyTrafficDirection = LeftObject.TrafficDirection Then
                                    NextSignal = TMDS.SystemUtilities.FetchData.FindMyControllingSignal(LeftObject, LeftObject.TrafficDirection)
                                    If NextSignal IsNot Nothing AndAlso Not SigCollection.Contains(NextSignal) Then
                                        SigCollection.Add(NextSignal)
                                    End If
                                Else
                                    Exit For
                                End If
                            End If
                        End If
                    Case "RIGHT"
                        RightObject = FindCompFromGUID(NextSignal.LeftLink)
                        If Not ComponentsCollection.Contains(RightObject) Then
                            If RightObject.GetType.ToString = gTrackComponentName Then
                                If RightObject.TrafficDirection <> "" AndAlso MyTrafficDirection = RightObject.TrafficDirection Then
                                    NextSignal = TMDS.SystemUtilities.FetchData.FindMyControllingSignal(RightObject, RightObject.TrafficDirection)
                                    If NextSignal IsNot Nothing AndAlso Not SigCollection.Contains(NextSignal) Then
                                        SigCollection.Add(NextSignal)
                                    End If
                                Else
                                    Exit For
                                End If
                            End If
                        End If

                End Select
            Next
            Return SigCollection
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

        Return Nothing
    End Function

    'Public Sub GetRightLeftTrackFromCollection(ByRef RightTrack As Track, ByRef LeftTrack As Track, ByVal CompCollection As ArrayList)
    '    Dim Comp As Object = Nothing
    '    Dim NextComp As Object = Nothing
    '    Dim FirstTrack As Track = Nothing
    '    Dim LastTrack As Track = Nothing
    '    If CompCollection Is Nothing Then Exit Sub
    '    If CompCollection.Count = 0 Then Exit Sub
    '    RightTrack = Nothing
    '    LeftTrack = Nothing
    '    Dim ReArrangedCollection As New ArrayList
    '    Try
    '        CopyArrayCollection(ReArrangedCollection, CompCollection)
    '        If ReArrangedCollection.Count > 1 Then
    '            FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
    '            Comp = FindCompFromGUID(FirstTrack.RightLink)
    '            If Not ReArrangedCollection.Contains(Comp) Then
    '                Dim ReverseCollection As Boolean = True
    '                If Comp.GetType.ToString = gSignalComponentName Then
    '                    NextComp = Nothing
    '                    NextComp = FindCompFromGUID(Comp.RightLink)
    '                    If ReArrangedCollection.Contains(NextComp) Then ReverseCollection = False
    '                End If
    '                If ReverseCollection Then
    '                    ReArrangedCollection.Reverse()
    '                    FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
    '                    LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
    '                    'RightTrack = LastTrack
    '                    'LeftTrack = FirstTrack
    '                Else
    '                    LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
    '                    'RightTrack = FirstTrack
    '                    'LeftTrack = LastTrack
    '                End If
    '                RightTrack = LastTrack
    '                LeftTrack = FirstTrack
    '            Else
    '                LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
    '                RightTrack = LastTrack
    '                LeftTrack = FirstTrack
    '            End If

    '        Else
    '            FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
    '            RightTrack = FirstTrack
    '            LeftTrack = FirstTrack
    '        End If
    '        If gDebug Then
    '            Comp = Nothing
    '            If RightTrack IsNot Nothing Then SystemEvent("RightTrack " & RightTrack.GetType.ToString & Space(2) & RightTrack.GUID)
    '            If LeftTrack IsNot Nothing Then SystemEvent("LeftTrack " & LeftTrack.GetType.ToString & Space(2) & LeftTrack.GUID)
    '        End If

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    Finally
    '        FirstTrack = Nothing
    '        LastTrack = Nothing
    '        ReArrangedCollection = Nothing
    '        Comp = Nothing
    '    End Try



    'End Sub

    Public Sub GetRightLeftTrackFromCollection(ByRef RightTrack As Track, ByRef LeftTrack As Track, ByVal CompCollection As ArrayList)
        Dim Comp As Object = Nothing
        Dim NextComp As Object = Nothing
        Dim FirstTrack As Track = Nothing
        Dim LastTrack As Track = Nothing
        If CompCollection Is Nothing Then Exit Sub
        If CompCollection.Count = 0 Then Exit Sub
        RightTrack = Nothing
        LeftTrack = Nothing
        Dim ReArrangedCollection As New ArrayList
        Try
            CopyArrayCollection(ReArrangedCollection, CompCollection)

            If ReArrangedCollection.Count > 1 Then
                FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)



                'Do RightSearch
                Comp = FindCompFromGUID(FirstTrack.RightLink)
                NextComp = FirstTrack
                For i As Integer = 1 To 50
                    If Comp IsNot Nothing Then
                        If Comp.GetType.ToString = gTrackComponentName Then
                            If Not ReArrangedCollection.Contains(Comp) Then
                                RightTrack = NextComp
                                Exit For
                            Else
                                NextComp = Comp
                                Comp = FindCompFromGUID(Comp.RightLink)
                            End If
                        Else
                            Comp = FindCompFromGUID(Comp.RightLink)
                        End If
                    End If
                Next
                NextComp = Nothing
                'Do LeftSearch
                Comp = FindCompFromGUID(FirstTrack.LeftLink)
                NextComp = FirstTrack
                For i As Integer = 1 To 50
                    If Comp IsNot Nothing Then
                        If Comp.GetType.ToString = gTrackComponentName Then
                            If Not ReArrangedCollection.Contains(Comp) Then
                                LeftTrack = NextComp
                                Exit For
                            Else
                                NextComp = Comp
                                Comp = FindCompFromGUID(Comp.LeftLink)
                            End If
                        Else
                            Comp = FindCompFromGUID(Comp.LeftLink)
                        End If
                    End If
                Next
            ElseIf ReArrangedCollection.Count = 1 Then
                RightTrack = ReArrangedCollection(0)
                LeftTrack = ReArrangedCollection(0)
            End If





            'If ReArrangedCollection.Count > 1 Then
            '    FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
            '    Comp = FindCompFromGUID(FirstTrack.RightLink)
            '    If Not ReArrangedCollection.Contains(Comp) Then
            '        Dim ReverseCollection As Boolean = True
            '        If Comp.GetType.ToString = gSignalComponentName Then
            '            NextComp = Nothing
            '            NextComp = FindCompFromGUID(Comp.RightLink)
            '            If ReArrangedCollection.Contains(NextComp) Then ReverseCollection = False
            '        End If
            '        If ReverseCollection Then
            '            ReArrangedCollection.Reverse()
            '            FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
            '            LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
            '            'RightTrack = LastTrack
            '            'LeftTrack = FirstTrack
            '        Else
            '            LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
            '            'RightTrack = FirstTrack
            '            'LeftTrack = LastTrack
            '        End If
            '        RightTrack = LastTrack
            '        LeftTrack = FirstTrack
            '    Else
            '        LastTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(ReArrangedCollection)
            '        RightTrack = LastTrack
            '        LeftTrack = FirstTrack
            '    End If

            'Else
            '    FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(ReArrangedCollection)
            '    RightTrack = FirstTrack
            '    LeftTrack = FirstTrack
            'End If
            If gDebug Then
                Comp = Nothing
                If RightTrack IsNot Nothing Then SystemEvent("RightTrack " & RightTrack.GetType.ToString & Space(2) & RightTrack.GUID)
                If LeftTrack IsNot Nothing Then SystemEvent("LeftTrack " & LeftTrack.GetType.ToString & Space(2) & LeftTrack.GUID)
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            FirstTrack = Nothing
            LastTrack = Nothing
            ReArrangedCollection = Nothing
            Comp = Nothing
        End Try



    End Sub

    Public Function ShowAuthorityUnAckPrompt(ByVal IssueTO As String, ByVal Header As String, ByVal LaunchPoint As Point) As Integer
        Dim FeedBack As Integer = 6
        Dim MessageList As New ArrayList()
        Try
            MessageList.Add(IssueTO.Trim.ToUpper)
            MessageList.Add("HAS OTHER TRACK AUTHORITY(S) UNACKNOWLEDGED.")
            MessageList.Add("DO YOU WANT TO ACKNOWLEDGE OTHER UNACKNOWLEDGED AUTHORITY ?")
            MessageList.Add("")
            ClearSystemVariables()
            FeedBack = ShowCustomMessageBox(MessageList, Header, True, LaunchPoint)
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            MessageList = Nothing
        End Try
        Return FeedBack
    End Function
    Public Sub ShowAuthorityPTCTransmissionInProgress(ByVal IssueTO As String, ByVal Header As String, ByVal LaunchPoint As Point)
        Dim MessageList As New ArrayList()
        Try
            MessageList.Add(IssueTO.Trim.ToUpper)
            MessageList.Add("HAS OTHER TRACK AUTHORITY(S) PTC TRANSMISSION IN PROGRESS.")
            MessageList.Add("CURRENT OPERATION IS NOT VALID.")
            MessageList.Add("")
            ClearSystemVariables()
            ShowCustomMessageBox(MessageList, Header, False, LaunchPoint)
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            MessageList = Nothing
        End Try

    End Sub

    Public Sub PopulateCPCrewCraftList()
        'Dim Crewlist As New ArrayList
        Try
            gTrainCrewCrafts = New ArrayList
            With gTrainCrewCrafts
                .Add("ENGR")
                .Add("ASST ENGR")
                .Add("2ND ENGR")
                .Add("ENGR TRNEE")
                .Add("CONDR")
                .Add("CONDR TRNEE")
                .Add("TRNM")
                .Add("TRNM TRNEE")
            End With
            'Crewlist.Add("ENGR")
            'Crewlist.Add("ASST ENGR")
            'Crewlist.Add("2ND ENGR")
            'Crewlist.Add("ENGR TRNEE")
            'Crewlist.Add("CONDR")
            'Crewlist.Add("CONDR TRNEE")
            'Crewlist.Add("TRNM")
            'Crewlist.Add("TRNM TRNEE")
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        'Return Crewlist
    End Sub

#Region " TB PROTECTION COLOR "

    Private Function GetTrackListForTheSB(ByVal theBulletin As SubBulletin) As ArrayList
        Dim TrackList As New ArrayList
        Dim NewTrackList As New ArrayList
        Dim MilePostTravelDirection As String = ""
        Dim trk As Track = Nothing ' Object

        Dim TrackNameList As New ArrayList
        Try
            Dim MileBegin As String = theBulletin.MilePostBegin
            Dim MileEnd As String = theBulletin.MilePostEnd
            Dim SubUID As Int32 = CInt(theBulletin.SubUID)
            Dim TrackName As String = theBulletin.TrackName
            If theBulletin.TrackNameList IsNot Nothing AndAlso theBulletin.TrackNameList.Trim <> "" Then
                Dim TrackNameSplt() As String
                Dim s As String = ""
                TrackNameSplt = theBulletin.TrackNameList.Split("|")
                For Each s In TrackNameSplt
                    If s.Trim <> "" Then
                        If Not TrackNameList.Contains(s.Trim.ToUpper) Then TrackNameList.Add(s.Trim.ToUpper)
                    End If
                Next
            End If

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

            NewTrackList = TMDS.SystemUtilities.FetchData.FindAllTracksFromMilePost(MileLow, MileHigh, "", "", "", "")

            If NewTrackList IsNot Nothing AndAlso NewTrackList.Count > 0 Then
                For Each trk In NewTrackList
                    If trk.GetType.ToString = gTrackComponentName Then
                        Dim TrkName As String = Trim(FormatProperWord(trk.TrackName.ToUpper.Replace("TRACK", "")))
                        If trk.sAlternateTrackNames.Trim <> "" Then
                            Dim names() As String = trk.sAlternateTrackNames.Split("|")
                            If names.GetUpperBound(0) > 0 Then TrkName = names(0)
                        End If
                        If TrackNameList.Contains(TrkName.Trim.ToUpper) Then
                            If Not TrackList.Contains(trk) Then TrackList.Add(trk)
                        End If
                    End If
                Next
            End If

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

    Public Function GetFilteredTracks(ByVal TrackList As ArrayList, ByVal MPlow As Decimal, ByVal MPhigh As Decimal, ByVal MPDirection As String) As ArrayList
        Dim dt As New DataTable
        Dim Col1 As DataColumn
        Dim NewTrackList As New ArrayList
        Dim trk As Track = Nothing
        Dim CuspMileLow As Decimal = MPlow + 0.00001
        Dim Dither As Decimal = 0.00001
        Dim dv As DataView
        Try
            Col1 = New DataColumn("TrackGUID")
            Col1.DataType = System.Type.GetType("System.Int32")
            dt.Columns.Add(Col1)

            Col1 = New DataColumn("MileLow")
            Col1.DataType = System.Type.GetType("System.Decimal")
            dt.Columns.Add(Col1)

            Col1 = New DataColumn("MileHigh")
            Col1.DataType = System.Type.GetType("System.Decimal")
            dt.Columns.Add(Col1)
            trk = Nothing
            For Each trk In TrackList
                Dim drow As DataRow = dt.NewRow
                drow("TrackGUID") = trk.GUID
                Select Case MPDirection.Trim.ToUpper
                    Case "RIGHT"
                        drow("MileLow") = trk.LeftLimitMilePost
                        drow("MileHigh") = trk.RightLimitMilePost
                    Case Else
                        drow("MileLow") = trk.RightLimitMilePost
                        drow("MileHigh") = trk.LeftLimitMilePost
                End Select
                dt.Rows.Add(drow)
                dt.AcceptChanges()
            Next
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then

                dv = New DataView(dt)
                dv.RowFilter = ""
                dv.RowFilter = "MileLow>=" & MPlow & " and MileHigh<=" & MPhigh
                If dv.Count > 0 Then
                    For i As Integer = 0 To dv.Count - 1
                        trk = Nothing
                        For Each trk In TrackList
                            If trk.GUID = dv(i)("TrackGUID") Then
                                If Not NewTrackList.Contains(trk) Then NewTrackList.Add(trk)
                            End If
                        Next
                    Next
                End If

                trk = Nothing
                For Each trk In TrackList
                    Dim FallsWithinRange As Boolean = False
                    If (CuspMileLow >= trk.RightLimitMilePost AndAlso CuspMileLow <= trk.LeftLimitMilePost) Then
                        FallsWithinRange = True
                    ElseIf (MPhigh > trk.RightLimitMilePost + Dither AndAlso MPhigh < trk.LeftLimitMilePost - Dither) Then
                        FallsWithinRange = True
                    End If
                    If FallsWithinRange Then
                        If Not NewTrackList.Contains(trk) Then NewTrackList.Add(trk)
                    End If
                Next

            End If
            Return NewTrackList


        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            dt = Nothing
            Col1 = Nothing
            dv = Nothing
        End Try
        Return Nothing
    End Function

    Public Function ShowRedOnTBProtection(ByVal theBulletin As SubBulletin) As Boolean
        Dim ShowRed As Boolean = True
        Dim auth As Object = Nothing
        Dim TrackCollection As New ArrayList
        Dim Comp As Object = Nothing
        Dim ExpDeliveredToTrain As New ArrayList
        Dim exp As TabularGeneralBulletinException
        Try
            TrackCollection = GetTrackListForTheSB(theBulletin)
            If TrackCollection Is Nothing Then Return False
            If TrackCollection.Count = 0 Then Return False

            If theBulletin.myExceptionList IsNot Nothing AndAlso theBulletin.myExceptionList.Count > 0 Then
                For Each exp In theBulletin.myExceptionList
                    If exp.ProtectionType = "DELIVERED" Then
                        If Not ExpDeliveredToTrain.Contains(exp.theDesignation) Then ExpDeliveredToTrain.Add(exp.theDesignation)
                    End If
                Next
            End If

            For Each Comp In TrackCollection
                If Comp.GetType.ToString = gTrackComponentName Then
                    If Comp.OtherSplAuthorities IsNot Nothing AndAlso Comp.OtherSplAuthorities.Count > 0 Then
                        For Each auth In Comp.OtherSplAuthorities
                            If ExpDeliveredToTrain IsNot Nothing AndAlso ExpDeliveredToTrain.Count > 0 Then
                                If Not ExpDeliveredToTrain.Contains(auth.TrainSymbol) Then Return True
                            Else
                                Return True
                            End If
                        Next
                    End If
                    If Comp.TrackAuthorities IsNot Nothing AndAlso Comp.TrackAuthorities.Count > 0 Then
                        For Each auth In Comp.TrackAuthorities
                            Select Case auth.GetType.ToString
                                Case gSFAComponentName
                                    If auth.IssuedToTrain Then
                                        If ExpDeliveredToTrain IsNot Nothing AndAlso ExpDeliveredToTrain.Count > 0 Then
                                            If Not ExpDeliveredToTrain.Contains(auth.TrainSymbol) Then Return True
                                        Else
                                            Return True
                                        End If
                                    End If
                                Case gTTComponentName
                                    If auth.AuthorityType = "TT" AndAlso auth.IssuedToTrain Then
                                        If ExpDeliveredToTrain IsNot Nothing AndAlso ExpDeliveredToTrain.Count > 0 Then
                                            If Not ExpDeliveredToTrain.Contains(auth.TrainSymbol) Then Return True
                                        Else
                                            Return True
                                        End If
                                    End If
                            End Select
                        Next
                    End If
                    'If Comp.AuthorityType.Trim.ToUpper.Contains("CTC") Then
                    '    If Comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then
                    '        Return True
                    '    End If
                    'End If

                End If
            Next


        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        Finally
            TrackCollection = Nothing
        End Try
        Return False
    End Function

    Public Function FormatProperWord(ByVal theWord As String) As String
        Try
            Dim words() As String = theWord.Split(" ")
            Dim theResult As String = String.Empty
            Dim w As String
            For Each w In words
                Dim thisWord As String = String.Empty
                thisWord = Mid(w.ToUpper, 1, 1) & Mid(w.ToLower, 2)
                If theResult = "" Then
                    theResult = thisWord
                Else
                    theResult = theResult & " " & thisWord
                End If
            Next
            Return theResult
        Catch ex As Exception
            GetErrorDetails(ex)
            Return theWord
        End Try
    End Function


    Public Function CheckIfTBIsDelivered(ByVal sb As SubBulletin) As Boolean
        Dim excp As TabularGeneralBulletinException
        Try
            Dim TBDelivered As Boolean = True
            For Each excp In sb.myExceptionList
                If (excp.ProtectionType <> "DELIVERED") Then
                    Return False
                ElseIf (excp.ProtectionType = "DELIVERED" AndAlso excp.HandlingType = "UNACKNOWLEDGED") Then
                    Return False
                End If
            Next
            'If DisableDeliver AndAlso Not AtleastOneDeliverPending AndAlso Not sb.CancelledBulletin Then
            'End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

#End Region


#Region " CP PTC "

#Region " SFA PTC "

    Public Function PerformPTCMultipleAuthorityRelease(ByVal AuthorityCollection As ArrayList, ByVal PTCTrainID As TrainID) As Boolean
        If AuthorityCollection Is Nothing Then Return False
        If AuthorityCollection.Count = 0 Then Return False
        Dim PTCSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim LastPTCSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim FailedPTCSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim HasMultipleAuthority As Boolean = False
        Dim AuthorityFailedCollection As New ArrayList
        Try
            If AuthorityCollection.Count > 1 Then HasMultipleAuthority = True
            LastPTCSFA = AuthorityCollection(AuthorityCollection.Count - 1)
            'PTCTrain = FindTrainEngineID(Mid(LastPTCSFA.IssueTo, 1, 10))
            If PTCTrainID Is Nothing Then Return False
            For Each PTCSFA In AuthorityCollection
                PTCSFA.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
                PTCSFA.PTCAuthorityComponents = PTCSFA.Components.Clone
                PTCSFA.UpdateDatabaseWithPTCComponents()
            Next
            For Each PTCSFA In AuthorityCollection
                If optDisableFlashingTrackForPTC Then PTCSFA.FlasherMode(False, True)
                'If IsRollup Then
                '    PTCSFA.PTCMarkedToBeRolledUpElectronically = True
                'Else
                PTCSFA.PTCMarkedToBeVoidedElectronically = True
                'End If

                PTCSFA.VETMS_PerformRelease(PTCTrainID)
                Do
                    Select Case PTCSFA.PTCStatus
                        Case "RELEASE-SUCCESSFUL"
                            SystemEvent("PTC RELEASE - Authority Number " & PTCSFA.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL ")
                            Exit Do
                        Case "RELEASE-FAILED"
                            SystemEvent("PTC RELEASE - Authority Number " & PTCSFA.AuthorityUID & " AUTHORITY RELEASE FAILED ")
                            FailedPTCSFA = PTCSFA
                            Exit For
                    End Select
                Loop
            Next
            If FailedPTCSFA IsNot Nothing Then
                PTCSFA = Nothing
                Dim StartFailedCollection As Boolean = False
                For Each PTCSFA In AuthorityCollection
                    If PTCSFA.AuthGUID = FailedPTCSFA.AuthGUID Then
                        StartFailedCollection = True
                    End If
                    If StartFailedCollection Then
                        If Not AuthorityFailedCollection.Contains(PTCSFA) Then AuthorityFailedCollection.Add(PTCSFA)
                    End If
                Next
                PTCSFA = Nothing
                For Each PTCSFA In AuthorityFailedCollection
                    PTCSFA.FlasherMode(False)
                    PTCSFA.ReportClearTime = Nothing
                    PTCSFA.ReportClearBy = ""
                    PTCSFA.CopiedBy = ""
                    PTCSFA.PTCStatus = "NONE"

                    PTCSFA.PTCAuthorityACKPending = ""
                    PTCSFA.PTCAuthorityComponents = New ArrayList
                    PTCSFA.UpdateDatabaseWhenPTCAuthorityTransmissionFailed()

                    Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", PTCSFA)
                    SyncLock gSummaryAuthQueue.SyncRoot
                        gSummaryAuthQueue.Enqueue(SummItem)
                    End SyncLock

                    Dim SummItem1 As New SummaryItem("AUTHORITY", "ADD", PTCSFA)
                    SyncLock gSummaryAuthQueue.SyncRoot
                        gSummaryAuthQueue.Enqueue(SummItem1)
                    End SyncLock

                Next
                Return False
            End If
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        Finally
            PTCSFA = Nothing
            LastPTCSFA = Nothing
            FailedPTCSFA = Nothing
            AuthorityFailedCollection = Nothing
        End Try

    End Function


    Public Function PerformPTCAuthorityRelease(ByVal SFA As TMDS.TrackAuthority.SFAuthority, ByVal PTCTrainID As TrainID) As Boolean
        Dim ReleaseSuccess As Boolean = False
        Try
            If PTCTrainID Is Nothing Then Return False
            SyncLock SFA
                SFA.PTCMarkedToBeVoidedElectronically = True
                SFA.VETMS_PerformRelease(PTCTrainID)
                'SFA.PTCReleaseStartTime = Now
            End SyncLock

            Do
                Select Case SFA.PTCStatus
                    Case "RELEASE-SUCCESSFUL"
                        ReleaseSuccess = True
                        Exit Do
                    Case "RELEASE-FAILED"
                        ReleaseSuccess = False
                        Exit Do
                End Select
                'If IsValidDate(SFA.PTCReleaseStartTime) AndAlso SFA.PTCReleaseStartTime.AddMinutes(3) > Now Then
                '    SFA.PTCReleaseStartTime = CDate("1/1/1900")
                '    ReleaseSuccess = False
                '    Exit Do
                'End If
            Loop

            If Not ReleaseSuccess Then
                ClearSFAPTCVariables(SFA)
            End If

            Return ReleaseSuccess
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        Finally

        End Try

    End Function

    Public Function PerformPTCMultipleAuthorityReleaseRollUp(ByVal RollUpAuthority As TMDS.TrackAuthority.SFAuthority, ByVal RollupAuthorityReleaseCollection As ArrayList, ByVal PTCTrain As TrainID) As Boolean
        Dim HasReleaseAuthority As Boolean = False
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim comp As Object = Nothing
        Dim SFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim AuthorityFailedCollection As New ArrayList
        Dim TempRollupReleaseAuth As New ArrayList
        Dim RollUpSuccess As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim ToBeReleaseUIDs As String = ""
        Dim CurrentRollup() As String
        Dim PTCRollupEvent As String = "" 'Space(5) & " ROLLUP EVENT FROM: "
        Try
            If RollUpAuthority Is Nothing Then Return False
            CurrentRollup = RollUpAuthority.CurrentRollUpEvent.Split("|")
            If CurrentRollup.GetUpperBound(0) >= 2 Then
                PTCRollupEvent = Space(5) & " ROLLUP EVENT FROM: "
                PTCRollupEvent = PTCRollupEvent & CurrentRollup(0)
                PTCRollupEvent = PTCRollupEvent & " TO " & CurrentRollup(1)
            End If
            If RollupAuthorityReleaseCollection IsNot Nothing AndAlso RollupAuthorityReleaseCollection.Count > 0 Then HasReleaseAuthority = True
            If HasReleaseAuthority Then
                CopyArrayCollection(TempRollupReleaseAuth, RollupAuthorityReleaseCollection)
                SystemEvent("PTC AUTHORITIES TO BE RELEASED BY THIS COMPLETE BY(" & RollUpAuthority.AuthorityUID & "): " & ToBeReleaseUIDs.Trim)
                For Each SFA In TempRollupReleaseAuth
                    If PerformPTCAuthorityRelease(SFA, PTCTrain) Then
                        gHistoryMessage = "PTC COMPLETE BY RELEASE - Authority Number " & SFA.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL - Compelte By Authority Number:" & RollUpAuthority.AuthorityUID
                        SystemEvent(gHistoryMessage)
                        RollupAuthorityReleaseCollection.Remove(SFA)
                    Else
                        gHistoryMessage = "PTC COMPLETE BY RELEASE - Authority Number " & SFA.AuthorityUID & " AUTHORITY RELEASE FAILED - Compelte By Authority Number: " & RollUpAuthority.AuthorityUID
                        SystemEvent(gHistoryMessage)
                        HasReleaseAuthorityFailed = True
                        Exit For
                    End If
                Next
                If Not HasReleaseAuthorityFailed Then
                    Threading.Thread.Sleep(200)
                    Application.DoEvents()
                Else
                    For Each SFA In RollupAuthorityReleaseCollection
                        ClearSFAPTCVariables(SFA)
                    Next
                End If
            End If
            If HasReleaseAuthorityFailed Then
                SystemEvent("PTC REQUEST TO RELEASE TRACK FAILED. ABORTING ROLLUP:" & RollUpAuthority.AuthorityUID)
                For Each comp In RollUpAuthority.PTCRollupCompCollection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        comp.InvalidateTrainID()
                        comp.InvalidateMOWID()
                    End If
                Next
                RollUpAuthority.FlasherMode(False)
                RollUpAuthority.PTCMarkedToBeRolledUpElectronically = False
                RollUpAuthority.PTCRollupTimeExpired = False
                RollUpAuthority.PTCRollupSuccessful = False
                RollUpAuthority.PTCAuthorityACKPending = ""
                RollUpAuthority.PTCAuthorityComponents = New ArrayList
                RollUpAuthority.UpdateDatabaseWhenPTCAuthorityTransmissionFailed()

                Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", RollUpAuthority)
                SyncLock gSummaryAuthQueue.SyncRoot
                    gSummaryAuthQueue.Enqueue(SummItem)
                End SyncLock

                Dim SummItem1 As New SummaryItem("AUTHORITY", "ADD", RollUpAuthority)
                SyncLock gSummaryAuthQueue.SyncRoot
                    gSummaryAuthQueue.Enqueue(SummItem1)
                End SyncLock

            Else
                If optDisableFlashingTrackForPTC Then RollUpAuthority.FlasherMode(False, True)
                If Not String.IsNullOrEmpty(RollUpAuthority.PTCReducedRollEventText) Then
                    PTCRollupEvent = RollUpAuthority.PTCReducedRollEventText
                Else
                    PTCRollupEvent = GetRollupText(RollUpAuthority)
                End If
                RollUpAuthority.PTCMarkedToBeRolledUpElectronically = True
                RollUpAuthority.VETMS_PerformRollup(PTCTrain, PTCRollupEvent, RollUpAuthority.PTCRollupCompCollection)
                Do
                    Select Case RollUpAuthority.PTCStatus
                        Case "RELEASE-SUCCESSFUL"
                            RollUpSuccess = True
                            Exit Do
                        Case "RELEASE-FAILED"
                            RollUpSuccess = False
                            Exit Do
                    End Select
                Loop


                'Do Until RollUpAuthority.PTCRollupSuccessful Or RollUpAuthority.PTCRollupTimeExpired
                '    Threading.Thread.Sleep(100)
                '    Application.DoEvents()
                'Loop
                'RollUpSuccess = RollUpAuthority.PTCRollupSuccessful
                RollUpAuthority.FlasherMode(False)
                RollUpAuthority.PTCMarkedToBeRolledUpElectronically = False
                RollUpAuthority.PTCRollupTimeExpired = False

                RollUpAuthority.PTCAuthorityACKPending = ""
                RollUpAuthority.PTCAuthorityComponents = New ArrayList
                'If Not RollUpAuthority.PTCRollupSuccessful Then
                If Not RollUpSuccess Then
                    RollUpAuthority.ClearSelection()
                    RollUpAuthority.VETMS_TerminatePTCTimerRollup()
                    RollUpAuthority.PTCMarkedToBeRolledUpElectronically = False
                    RollUpAuthority.PTCRollupTimeExpired = False
                    RollUpAuthority.PTCRollupSuccessful = False
                    RollUpAuthority.UpdateDatabaseWhenPTCAuthorityTransmissionFailed()
                    gHistoryMessage = "PTC COMPLETE BY  - AUTHORITY NUMBER " & RollUpAuthority.AuthorityUID & " FAILED "
                    SystemEvent(gHistoryMessage)
                Else
                    RollUpAuthority.VETMS_TerminatePTCTimerRollup()
                    RollUpAuthority.PTCMarkedToBeRolledUpElectronically = False
                    RollUpAuthority.PTCRollupTimeExpired = False
                    RollUpAuthority.PTCRollupSuccessful = False
                    RollUpAuthority.PerformCPTrainRollup()
                    gHistoryMessage = "PTC COMPLETE BY  - AUTHORITY NUMBER " & RollUpAuthority.AuthorityUID & " SUCCESSFUL "
                    SystemEvent(gHistoryMessage)
                End If
                RollUpAuthority.PTCRollupSuccessful = False
                Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", RollUpAuthority)
                SyncLock gSummaryAuthQueue.SyncRoot
                    gSummaryAuthQueue.Enqueue(SummItem)
                End SyncLock

                Dim SummItem1 As New SummaryItem("AUTHORITY", "ADD", RollUpAuthority)
                SyncLock gSummaryAuthQueue.SyncRoot
                    gSummaryAuthQueue.Enqueue(SummItem1)
                End SyncLock
            End If



        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            AuthorityFailedCollection = Nothing
            TempRollupReleaseAuth = Nothing
            SFA = Nothing
            comp = Nothing
        End Try

    End Function

    Public Sub ClearSFAPTCVariables(ByVal SFA As TMDS.TrackAuthority.SFAuthority)
        Try
            If SFA Is Nothing Then Exit Sub
            With SFA
                .FlasherMode(False)
                .ReportClearTime = Nothing
                .ReportClearBy = ""
                .CopiedBy = ""
                .PTCStatus = "NONE"
                .PTCAuthorityACKPending = ""
                .PTCAuthorityComponents = New ArrayList
                .Box4NormalSwitchRptOnReleasePTC = ""
                .UpdateDatabaseWithPTCAuthorityStatus(SFA.PTCStatus)
                .UpdateDatabaseWithPTCComponents()
                .UpdateDatabaseWhenPTCAuthorityTransmissionFailed()
            End With

            Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", SFA)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem)
            End SyncLock

            Dim SummItem1 As New SummaryItem("AUTHORITY", "ADD", SFA)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem1)
            End SyncLock

            Dim SummItem2 As New SummaryItem("SUB-BULLETIN", "REFRESH", Nothing, "ClearSFAPTCVariables")
            SyncLock gSubBulletinsMessageSummaryQueue.SyncRoot
                gSubBulletinsMessageSummaryQueue.Enqueue(SummItem2)
            End SyncLock

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    'Public Function PerformPTCSFAIssue(ByRef SFA As TMDS.TrackAuthority.SFAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
    '    Dim VoidedSFA As TMDS.TrackAuthority.SFAuthority = Nothing
    '    Dim VoidedSFA2 As TMDS.TrackAuthority.SFAuthority = Nothing
    '    Dim HasReleaseAuthorityFailed As Boolean = False
    '    Dim gHistoryMessage As String = ""
    '    Dim TempVOIDAuthCollection As New ArrayList
    '    Dim HasVoidedList As Boolean = False
    '    Dim AuthorityIssueSucess As Boolean = True
    '    Dim IsNewAuthority As Boolean = True
    '    Dim MySFA As TMDS.TrackAuthority.SFAuthority = SFA
    '    Try
    '        If Not String.IsNullOrEmpty(SFA.PTCStatus.Trim) Then
    '            If SFA.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
    '                IsNewAuthority = False
    '                AuthorityIssueSucess = True
    '            End If
    '        End If
    '        If IsNewAuthority AndAlso Not SendVoidOnly Then
    '            If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
    '                Dim AuthDesg As AuthorityDesignation = Nothing
    '                Dim ActiveAuthDesgination As New ArrayList
    '                Try
    '                    For Each AuthDesg In PTCTrainID.AuthDesignations
    '                        If AuthDesg.InActive Then
    '                            If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
    '                        End If
    '                    Next
    '                    For Each AuthDesg In ActiveAuthDesgination
    '                        PTCTrainID.AuthDesignations.Remove(AuthDesg)
    '                    Next
    '                    PTCTrainID.UpdateAuthorityDesignationToDB()
    '                    PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
    '                    PTCTrainID.PendingLeadEngine = ""
    '                    PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
    '                    PTCTrainID.myTrack.InvalidateTrainID()
    '                    PTCTrainID.myTrack.InitTrainDisplay()
    '                Catch ex As Exception
    '                    GetErrorDetails(ex)
    '                Finally
    '                    ActiveAuthDesgination = Nothing
    '                    AuthDesg = Nothing
    '                End Try
    '            End If

    '            'Me.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
    '            If Not gEnableIncludeVoidAuthorityIn1051 Then
    '                If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '                    For Each VoidedSFA In VoidList
    '                        VoidedSFA.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
    '                        VoidedSFA.PTCAuthorityComponents = VoidedSFA.Components.Clone
    '                        VoidedSFA.UpdateDatabaseWithPTCComponents()
    '                    Next
    '                End If
    '                VoidedSFA = Nothing
    '                MySFA.VETMS_PerformIssuance(PTCTrainID)
    '                Do
    '                    Select Case MySFA.PTCStatus.Trim.ToUpper
    '                        Case "AUTHORITY-ACTIVATED"
    '                            AuthorityIssueSucess = True
    '                            Exit Do
    '                        Case "AUTHORITY-FAILED"
    '                            AuthorityIssueSucess = False
    '                            Exit Do
    '                    End Select
    '                    Threading.Thread.Sleep(250)
    '                Loop
    '            End If
    '        End If

    '        If Not AuthorityIssueSucess Then
    '            SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & SFA.AuthorityUID)
    '            SFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            SFA.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & SFA.AuthorityUID)
    '            Return False
    '        Else
    '            If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '                SFA.PTCAuthorityACKPending = "NONE"
    '                SFA.UpdateDatabaseWithPTCComponents()
    '            End If
    '        End If

    '        If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '            CopyArrayCollection(TempVOIDAuthCollection, VoidList)
    '            HasVoidedList = True
    '            'HasReleaseAuthorityFailed = True
    '            For Each VoidedSFA In VoidList
    '                If PerformPTCAuthorityRelease(VoidedSFA, PTCTrainID) Then
    '                    TempVOIDAuthCollection.Remove(VoidedSFA)
    '                    gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedSFA.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL - VOIDED By Authority Number:" & VoidedSFA.AuthorityUID
    '                    SystemEvent(gHistoryMessage)
    '                Else
    '                    gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedSFA.AuthorityUID & " AUTHORITY RELEASE FAILED - VOIDED By Authority Number: " & VoidedSFA.AuthorityUID
    '                    SystemEvent(gHistoryMessage)
    '                    HasReleaseAuthorityFailed = True
    '                    Exit For
    '                End If

    '                If Not HasReleaseAuthorityFailed Then
    '                    Threading.Thread.Sleep(200)
    '                    Application.DoEvents()
    '                Else
    '                    If TempVOIDAuthCollection IsNot Nothing AndAlso TempVOIDAuthCollection.Count > 0 Then
    '                        For Each VoidedSFA2 In TempVOIDAuthCollection
    '                            ClearSFAPTCVariables(VoidedSFA2)
    '                        Next
    '                    End If
    '                End If
    '            Next
    '        End If

    '        If HasReleaseAuthorityFailed Then
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. ABORTING ISSUE:" & SFA.AuthorityUID)
    '            SFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            SFA.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & SFA.AuthorityUID)
    '            Return False
    '        Else
    '            SFA.UpdateTrackAuthorityStatus()
    '            SFA.AuthorityStatus = TNTAuthStatus.ACKNOWLEDGED.ToString
    '            SFA.UpdateTrackAuthorityStatus()
    '            UpdateLeadEngine(HasLeadEngineChange, PTCTrainID)
    '            If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '                SFA.PTCAuthorityACKPending = ""
    '                SFA.UpdateDatabaseWithPTCComponents()
    '            End If
    '        End If

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Function

    Public Function PerformPTCSFAIssueResend(ByVal SFA As TMDS.TrackAuthority.SFAuthority, ByVal PTCTrainID As TrainID, ByVal HasLeadEngineChange As Boolean) As Boolean
        Dim VoidedSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim VoidedSFA2 As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim TempVOIDAuthCollection As New ArrayList
        Dim HasVoidedList As Boolean = False
        Dim AuthorityIssueSucess As Boolean = True
        Dim IsNewAuthority As Boolean = True
        Dim MySFA As TMDS.TrackAuthority.SFAuthority = SFA
        Try
            If Not String.IsNullOrEmpty(SFA.PTCStatus.Trim) Then
                If SFA.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
                    IsNewAuthority = False
                    AuthorityIssueSucess = True
                End If
            End If
            If IsNewAuthority Then
                If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                    Dim AuthDesg As AuthorityDesignation = Nothing
                    Dim ActiveAuthDesgination As New ArrayList
                    Try
                        For Each AuthDesg In PTCTrainID.AuthDesignations
                            If AuthDesg.InActive Then
                                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                            End If
                        Next
                        For Each AuthDesg In ActiveAuthDesgination
                            PTCTrainID.AuthDesignations.Remove(AuthDesg)
                        Next
                        PTCTrainID.UpdateAuthorityDesignationToDB()
                        PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                        PTCTrainID.PendingLeadEngine = ""
                        PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                        PTCTrainID.myTrack.InvalidateTrainID()
                        PTCTrainID.myTrack.InitTrainDisplay()
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    Finally
                        ActiveAuthDesgination = Nothing
                        AuthDesg = Nothing
                    End Try
                End If
                MySFA.VETMS_PerformIssuance(PTCTrainID)
                Do
                    Select Case MySFA.PTCStatus.Trim.ToUpper
                        Case "AUTHORITY-ACTIVATED"
                            AuthorityIssueSucess = True
                            Exit Do
                        Case "AUTHORITY-FAILED"
                            AuthorityIssueSucess = False
                            Exit Do
                    End Select
                    Threading.Thread.Sleep(250)
                Loop
            End If

            If Not AuthorityIssueSucess Then
                SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & SFA.AuthorityUID)
                SFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                SFA.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & SFA.AuthorityUID)
                Return False
            End If



        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Private Function GetRollupText(ByVal RollUpAuthority As TMDS.TrackAuthority.SFAuthority) As String
        Dim trk1 As Object = Nothing
        Dim trk2 As Object = Nothing
        Dim RollUpEventMessage As String = String.Empty
        Dim RightLimits As String = String.Empty
        Dim LeftLimits As String = String.Empty
        Try
            If RollUpAuthority.PTCRollupCompCollection Is Nothing Then Return String.Empty
            If RollUpAuthority.PTCRollupCompCollection.Count = 0 Then Return String.Empty

            trk1 = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(RollUpAuthority.PTCRollupCompCollection)
            trk2 = TMDS.SystemUtilities.FetchData.FindLastTrackInList(RollUpAuthority.PTCRollupCompCollection)
            If RollUpAuthority.PTCRollupReleaseDirection = "RIGHT" Then
                RightLimits = trk1.RightLimitName
                LeftLimits = trk2.LeftLimitName
            Else
                RightLimits = trk1.LeftLimitName
                LeftLimits = trk2.RightLimitName
            End If

            If InStr(RightLimits, ",") > 0 Then
                Dim Delimiter As Integer = InStr(RightLimits, ",")
                RightLimits = Mid(RightLimits, 1, Delimiter - 1)
            End If
            If InStr(LeftLimits, ",") > 0 Then
                Dim Delimiter As Integer = InStr(LeftLimits, ",")
                LeftLimits = Mid(LeftLimits, 1, Delimiter - 1)
            End If
            RollUpEventMessage = Space(5) & " ROLLUP EVENT FROM: "
            RollUpEventMessage = RollUpEventMessage & LeftLimits & " TO " & RightLimits

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return RollUpEventMessage
    End Function

    Public Function PerformPTCSFAIssueWithVoid(ByRef MySFA As TMDS.TrackAuthority.SFAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
        Dim VoidedSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim HasVoidedList As Boolean = False
        Dim AuthorityIssueSucess As Boolean = True
        Dim IsNewAuthority As Boolean = True
        Dim RpClearTIme As String = Format(Now, "HHmm")
        Dim RptClearDateTime As String = Format(Now, "g")
        Dim FirstTrack As Object = Nothing

        Try
            If MySFA Is Nothing Then Return False
            If Not String.IsNullOrEmpty(MySFA.PTCStatus.Trim) Then
                If MySFA.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
                    IsNewAuthority = False
                    AuthorityIssueSucess = True
                End If
            End If
            FirstTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(MySFA.Components)
            If optEnableTimeZoneSettings AndAlso FirstTrack IsNot Nothing AndAlso FirstTrack.GetType.ToString = gTrackComponentName AndAlso FirstTrack.mySubdivision IsNot Nothing Then
                Dim SubTime As Date = GenerateTime(FirstTrack.mySubdivision)
                RptClearDateTime = SubTime
                RpClearTIme = Format(SubTime, "HHmm")
            End If


            If IsNewAuthority Then
                'If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                '    Dim AuthDesg As AuthorityDesignation = Nothing
                '    Dim ActiveAuthDesgination As New ArrayList
                '    Try
                '        For Each AuthDesg In PTCTrainID.AuthDesignations
                '            If AuthDesg.InActive Then
                '                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                '            End If
                '        Next
                '        For Each AuthDesg In ActiveAuthDesgination
                '            PTCTrainID.AuthDesignations.Remove(AuthDesg)
                '        Next
                '        PTCTrainID.UpdateAuthorityDesignationToDB()
                '        PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                '        PTCTrainID.PendingLeadEngine = ""
                '        PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                '        PTCTrainID.myTrack.InvalidateTrainID()
                '        PTCTrainID.myTrack.InitTrainDisplay()
                '    Catch ex As Exception
                '        GetErrorDetails(ex)
                '    Finally
                '        ActiveAuthDesgination = Nothing
                '        AuthDesg = Nothing
                '    End Try
                'End If

                If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                    For Each VoidedSFA In VoidList
                        VoidedSFA.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
                        VoidedSFA.PTCAuthorityComponents = VoidedSFA.Components.Clone
                        VoidedSFA.UpdateDatabaseWithPTCComponents()
                    Next
                End If
                VoidedSFA = Nothing
                MySFA.VETMS_PerformIssuance(PTCTrainID)
                Do
                    Select Case MySFA.PTCStatus.Trim.ToUpper
                        Case "AUTHORITY-ACTIVATED"
                            AuthorityIssueSucess = True
                            Exit Do
                        Case "AUTHORITY-FAILED"
                            AuthorityIssueSucess = False
                            Exit Do
                    End Select
                    Threading.Thread.Sleep(250)
                Loop

            End If

            If Not AuthorityIssueSucess Then
                SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & MySFA.AuthorityUID)
                MySFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                MySFA.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & MySFA.AuthorityUID)
                Return False
            End If

            If AuthorityIssueSucess AndAlso VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then

                For Each VoidedSFA In VoidList
                    'VoidedSFA.ReportClearTime = RptClearDateTime
                    'VoidedSFA.ReportClearBy = gCurrentUser.Initials
                    If VoidedSFA.RemoveSFAVIIIAuthorityFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        If VoidedSFA.RemoveAuthorityFromComponents() Then
                            TMDS.GlobalDeclarations.OtherTrackAuthorities.Remove(VoidedSFA)
                            Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", VoidedSFA)
                            SyncLock gSummaryAuthQueue.SyncRoot
                                gSummaryAuthQueue.Enqueue(SummItem)
                            End SyncLock
                        Else
                            gHistoryMessage = "Authority Number " & VoidedSFA.AuthorityUID & " NOT Deleted from SYSTEM. CONTACT THE SIGNAL DESK."
                            MessageBox.Show(TMDS.SystemForms.MySystemForms.frmMainMDI, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            SystemEvent(gHistoryMessage)
                            ClearSystemVariables()
                            HasReleaseAuthorityFailed = True
                        End If
                    Else
                        gHistoryMessage = "Auhtority Number " & VoidedSFA.AuthorityUID & " NOT Deleted from DATABASE. CONTACT THE SIGNAL DESK."
                        MessageBox.Show(TMDS.SystemForms.MySystemForms.frmMainMDI, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        SystemEvent(gHistoryMessage)
                        ClearSystemVariables()
                        HasReleaseAuthorityFailed = True
                    End If
                Next
            End If

            If HasReleaseAuthorityFailed Then
                SystemEvent("PTC REQUEST TO ISSUE FAILED. ABORTING ISSUE:" & MySFA.AuthorityUID)
                MySFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                MySFA.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & MySFA.AuthorityUID)
                Return False
            Else
                MySFA.UpdateTrackAuthorityStatus()
                MySFA.AuthorityStatus = TNTAuthStatus.ACKNOWLEDGED.ToString
                MySFA.UpdateTrackAuthorityStatus()
                UpdateLeadEngine(HasLeadEngineChange, PTCTrainID)
                If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                    MySFA.PTCAuthorityACKPending = ""
                    MySFA.UpdateDatabaseWithPTCComponents()
                End If
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function


#End Region

#Region " TNT PTC "


    Public Function PerformPTCTNTRelease(ByVal TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID) As Boolean
        Dim ReleaseSuccess As Boolean = False
        Try
            If optDisableFlashingTrackForPTC Then TNT.FlasherMode(False)
            If PTCTrainID Is Nothing Then Return False
            'TNT.PTCMarkedToBeVoidedElectronically = True
            If optEnablePTCBOSVETMSMode Then
                TNT.VETMS_PerformRelease(PTCTrainID)
                Return True
            End If
            Return False
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
    End Function

    Private Function PerformPTCTNTVoidAuthRelease(ByVal TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID) As Boolean
        Dim ReleaseSuccess As Boolean = False
        Try
            If PTCTrainID Is Nothing Then Return False
            'TNT.PTCMarkedToBeVoidedElectronically = True
            If optEnablePTCBOSVETMSMode Then
                TNT.VETMS_PerformRelease(PTCTrainID)
                Do
                    Select Case TNT.PTCStatus
                        Case "RELEASE-SUCCESSFUL"
                            ReleaseSuccess = True
                            Exit Do
                        Case "RELEASE-FAILED"
                            ReleaseSuccess = False
                            Exit Do
                    End Select
                Loop

                If Not ReleaseSuccess Then
                    ClearTNTPTCVariables(TNT)
                End If
            End If
            Return ReleaseSuccess
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
    End Function

    Public Function PerformPTCTNTIssueVer1(ByRef TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean) As Boolean
        Dim VoidedTNT As TMDS.TrackAuthority.TrackAuthority = Nothing
        Dim VoidedTNT2 As TMDS.TrackAuthority.TrackAuthority = Nothing
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim TempVOIDAuthCollection As New ArrayList
        Dim HasVoidedList As Boolean = False
        Try
            TNT.FlasherMode(False)
            If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                CopyArrayCollection(TempVOIDAuthCollection, VoidList)
                HasVoidedList = True
                'HasReleaseAuthorityFailed = True
                For Each VoidedTNT In VoidList
                    'VoidedTNT.PTCMarkedToBeVoidedElectronically = True
                    If PerformPTCTNTVoidAuthRelease(VoidedTNT, PTCTrainID) Then
                        TempVOIDAuthCollection.Remove(VoidedTNT)
                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL - VOIDED By Authority Number:" & TNT.AuthorityUID
                        SystemEvent(gHistoryMessage)
                    Else
                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE FAILED - VOIDED By Authority Number: " & TNT.AuthorityUID
                        SystemEvent(gHistoryMessage)
                        HasReleaseAuthorityFailed = True
                        Exit For
                    End If

                    If Not HasReleaseAuthorityFailed Then
                        Threading.Thread.Sleep(200)
                        Application.DoEvents()
                    Else
                        If TempVOIDAuthCollection IsNot Nothing AndAlso TempVOIDAuthCollection.Count > 0 Then
                            For Each VoidedTNT2 In TempVOIDAuthCollection
                                ClearTNTPTCVariables(VoidedTNT2)
                            Next
                        End If
                    End If
                Next
            End If

            If HasReleaseAuthorityFailed Then
                SystemEvent("PTC REQUEST TO ISSUE FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
                TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                TNT.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
                Return False
            Else
                If HasVoidedList Then
                    TNT.AuthVoidList = ""
                    TNT.UpdateTrackAuthorityVOIDList()
                End If
                'TNT.FlasherMode(False)
                TNT.UpdateTrackAuthority()
                TNT.AuthorityStatus = TNTAuthStatus.ACKNOWLEDGED.ToString
                TNT.UpdateTrackAuthorityStatus()
                If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                    Dim AuthDesg As AuthorityDesignation = Nothing
                    Dim ActiveAuthDesgination As New ArrayList
                    Try
                        For Each AuthDesg In PTCTrainID.AuthDesignations
                            If AuthDesg.InActive Then
                                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                            End If
                        Next
                        For Each AuthDesg In ActiveAuthDesgination
                            PTCTrainID.AuthDesignations.Remove(AuthDesg)
                        Next
                        PTCTrainID.UpdateAuthorityDesignationToDB()
                        PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                        PTCTrainID.PendingLeadEngine = ""
                        'myTrain.UpdateIdentifingUnit(True)
                        PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                        PTCTrainID.myTrack.InvalidateTrainID()
                        PTCTrainID.myTrack.InitTrainDisplay()
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    Finally
                        ActiveAuthDesgination = Nothing
                        AuthDesg = Nothing
                    End Try
                End If
                'TNT.PTCStatus = "AUTHORITY-SENT"
                TNT.VETMS_PerformIssuance(PTCTrainID, Nothing, Nothing)
                'Dim SummItem2 As New SummaryItem("AUTHORITY", "UPDATE-PTC", TNT)
                'SyncLock gSummaryAuthQueue.SyncRoot
                '    gSummaryAuthQueue.Enqueue(SummItem2)
                'End SyncLock
                'Return True
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    'Public Function PerformPTCTNTIssue(ByRef TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
    '    Dim VoidedTNT As TMDS.TrackAuthority.TrackAuthority = Nothing
    '    Dim VoidedTNT2 As TMDS.TrackAuthority.TrackAuthority = Nothing
    '    Dim HasReleaseAuthorityFailed As Boolean = False
    '    Dim gHistoryMessage As String = ""
    '    Dim TempVOIDAuthCollection As New ArrayList
    '    Dim HasVoidedList As Boolean = False
    '    Dim AuthorityIssueSucess As Boolean = True
    '    Dim IsNewAuthority As Boolean = True
    '    Try
    '        TNT.FlasherMode(False)
    '        If Not String.IsNullOrEmpty(TNT.PTCStatus.Trim) Then
    '            If TNT.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
    '                IsNewAuthority = False
    '                AuthorityIssueSucess = True
    '            End If
    '        ElseIf SendVoidOnly Then
    '            AuthorityIssueSucess = True
    '        End If
    '        If IsNewAuthority AndAlso Not SendVoidOnly Then
    '            If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
    '                Dim AuthDesg As AuthorityDesignation = Nothing
    '                Dim ActiveAuthDesgination As New ArrayList
    '                Try
    '                    For Each AuthDesg In PTCTrainID.AuthDesignations
    '                        If AuthDesg.InActive Then
    '                            If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
    '                        End If
    '                    Next
    '                    For Each AuthDesg In ActiveAuthDesgination
    '                        PTCTrainID.AuthDesignations.Remove(AuthDesg)
    '                    Next
    '                    PTCTrainID.UpdateAuthorityDesignationToDB()
    '                    PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
    '                    PTCTrainID.PendingLeadEngine = ""
    '                    'myTrain.UpdateIdentifingUnit(True)
    '                    PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
    '                    PTCTrainID.myTrack.InvalidateTrainID()
    '                    PTCTrainID.myTrack.InitTrainDisplay()
    '                Catch ex As Exception
    '                    GetErrorDetails(ex)
    '                Finally
    '                    ActiveAuthDesgination = Nothing
    '                    AuthDesg = Nothing
    '                End Try
    '            End If
    '            TNT.VETMS_PerformIssuance(PTCTrainID, Nothing, Nothing)
    '            Do
    '                Select Case TNT.PTCStatus.Trim.ToUpper
    '                    Case "AUTHORITY-ACTIVATED"
    '                        AuthorityIssueSucess = True
    '                        Exit Do
    '                    Case "AUTHORITY-FAILED"
    '                        AuthorityIssueSucess = False
    '                        Exit Do
    '                End Select
    '                Threading.Thread.Sleep(250)
    '            Loop
    '        End If

    '        If Not AuthorityIssueSucess Then
    '            SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
    '            'TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            'TNT.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
    '            Return False
    '        Else
    '            TNT.PTCAuthorityACKPending = "NONE"
    '            TNT.UpdateDatabaseWithPTCComponents()
    '        End If
    '        ' 
    '        If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '            CopyArrayCollection(TempVOIDAuthCollection, VoidList)
    '            HasVoidedList = True
    '            'HasReleaseAuthorityFailed = True
    '            If Not gEnableIncludeVoidAuthorityIn1051 Then
    '                For Each VoidedTNT In VoidList
    '                    'VoidedTNT.PTCMarkedToBeVoidedElectronically = True
    '                    If PerformPTCTNTVoidAuthRelease(VoidedTNT, PTCTrainID) Then
    '                        TempVOIDAuthCollection.Remove(VoidedTNT)
    '                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL - VOIDED By Authority Number:" & TNT.AuthorityUID
    '                        SystemEvent(gHistoryMessage)
    '                    Else
    '                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE FAILED - VOIDED By Authority Number: " & TNT.AuthorityUID
    '                        SystemEvent(gHistoryMessage)
    '                        HasReleaseAuthorityFailed = True
    '                        Exit For
    '                    End If

    '                    If Not HasReleaseAuthorityFailed Then
    '                        Threading.Thread.Sleep(200)
    '                        Application.DoEvents()
    '                    Else
    '                        If TempVOIDAuthCollection IsNot Nothing AndAlso TempVOIDAuthCollection.Count > 0 Then
    '                            For Each VoidedTNT2 In TempVOIDAuthCollection
    '                                ClearTNTPTCVariables(VoidedTNT2)
    '                            Next
    '                        End If
    '                    End If
    '                Next
    '            End If
    '        Else

    '        End If

    '        If HasReleaseAuthorityFailed Then
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
    '            TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            TNT.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
    '            Return False
    '        Else
    '            If gEnableIncludeVoidAuthorityIn1051 Then
    '                ReleaseVoidedAuthority(VoidList, TNT.CopiedBy.ToUpper.Trim)
    '            End If
    '            If HasVoidedList Then
    '                TNT.AuthVoidList = ""
    '                TNT.UpdateTrackAuthorityVOIDList()
    '            End If
    '            TNT.PTCAuthorityACKPending = ""
    '            TNT.UpdateDatabaseWithPTCComponents()
    '            TNT.UpdateTrackAuthority()
    '            TNT.AuthorityStatus = TNTAuthStatus.ACKNOWLEDGED.ToString
    '            TNT.UpdateTrackAuthorityStatus()
    '        End If

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Function

    'Public Function PerformPTCTNTIssueWithVoid(ByRef TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
    '    Dim VoidedTNT As TMDS.TrackAuthority.TrackAuthority = Nothing
    '    Dim VoidedTNT2 As TMDS.TrackAuthority.TrackAuthority = Nothing
    '    Dim HasReleaseAuthorityFailed As Boolean = False
    '    Dim gHistoryMessage As String = ""
    '    Dim TempVOIDAuthCollection As New ArrayList
    '    Dim HasVoidedList As Boolean = False
    '    Dim AuthorityIssueSucess As Boolean = True
    '    Dim IsNewAuthority As Boolean = True
    '    Try
    '        TNT.FlasherMode(False)
    '        If Not String.IsNullOrEmpty(TNT.PTCStatus.Trim) Then
    '            If TNT.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
    '                IsNewAuthority = False
    '                AuthorityIssueSucess = True
    '            End If
    '        ElseIf SendVoidOnly Then
    '            AuthorityIssueSucess = True
    '        End If
    '        If IsNewAuthority AndAlso Not SendVoidOnly Then
    '            If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
    '                Dim AuthDesg As AuthorityDesignation = Nothing
    '                Dim ActiveAuthDesgination As New ArrayList
    '                Try
    '                    For Each AuthDesg In PTCTrainID.AuthDesignations
    '                        If AuthDesg.InActive Then
    '                            If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
    '                        End If
    '                    Next
    '                    For Each AuthDesg In ActiveAuthDesgination
    '                        PTCTrainID.AuthDesignations.Remove(AuthDesg)
    '                    Next
    '                    PTCTrainID.UpdateAuthorityDesignationToDB()
    '                    PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
    '                    PTCTrainID.PendingLeadEngine = ""
    '                    'myTrain.UpdateIdentifingUnit(True)
    '                    PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
    '                    PTCTrainID.myTrack.InvalidateTrainID()
    '                    PTCTrainID.myTrack.InitTrainDisplay()
    '                Catch ex As Exception
    '                    GetErrorDetails(ex)
    '                Finally
    '                    ActiveAuthDesgination = Nothing
    '                    AuthDesg = Nothing
    '                End Try
    '            End If
    '            TNT.VETMS_PerformIssuance(PTCTrainID, Nothing, Nothing)
    '            Do
    '                Select Case TNT.PTCStatus.Trim.ToUpper
    '                    Case "AUTHORITY-ACTIVATED"
    '                        AuthorityIssueSucess = True
    '                        Exit Do
    '                    Case "AUTHORITY-FAILED"
    '                        AuthorityIssueSucess = False
    '                        Exit Do
    '                End Select
    '                Threading.Thread.Sleep(250)
    '            Loop
    '        End If

    '        If Not AuthorityIssueSucess Then
    '            SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
    '            TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            TNT.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
    '            Return False
    '        Else
    '            TNT.PTCAuthorityACKPending = "NONE"
    '            TNT.UpdateDatabaseWithPTCComponents()
    '        End If
    '        ' 
    '        If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
    '            CopyArrayCollection(TempVOIDAuthCollection, VoidList)
    '            HasVoidedList = True
    '            'HasReleaseAuthorityFailed = True
    '            If Not gEnableIncludeVoidAuthorityIn1051 Then
    '                For Each VoidedTNT In VoidList
    '                    'VoidedTNT.PTCMarkedToBeVoidedElectronically = True
    '                    If PerformPTCTNTVoidAuthRelease(VoidedTNT, PTCTrainID) Then
    '                        TempVOIDAuthCollection.Remove(VoidedTNT)
    '                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE SUCCESSFUL - VOIDED By Authority Number:" & TNT.AuthorityUID
    '                        SystemEvent(gHistoryMessage)
    '                    Else
    '                        gHistoryMessage = "PTC VOID RELEASE - Authority Number " & VoidedTNT.AuthorityUID & " AUTHORITY RELEASE FAILED - VOIDED By Authority Number: " & TNT.AuthorityUID
    '                        SystemEvent(gHistoryMessage)
    '                        HasReleaseAuthorityFailed = True
    '                        Exit For
    '                    End If

    '                    If Not HasReleaseAuthorityFailed Then
    '                        Threading.Thread.Sleep(200)
    '                        Application.DoEvents()
    '                    Else
    '                        If TempVOIDAuthCollection IsNot Nothing AndAlso TempVOIDAuthCollection.Count > 0 Then
    '                            For Each VoidedTNT2 In TempVOIDAuthCollection
    '                                ClearTNTPTCVariables(VoidedTNT2)
    '                            Next
    '                        End If
    '                    End If
    '                Next
    '            End If
    '        Else

    '        End If

    '        If HasReleaseAuthorityFailed Then
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
    '            TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
    '            TNT.UpdateTrackAuthorityStatus()
    '            SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
    '            Return False
    '        Else
    '            If gEnableIncludeVoidAuthorityIn1051 Then
    '                ReleaseVoidedAuthority(VoidList, TNT.CopiedBy.ToUpper.Trim)
    '            End If
    '            If HasVoidedList Then
    '                TNT.AuthVoidList = ""
    '                TNT.UpdateTrackAuthorityVOIDList()
    '            End If
    '            TNT.PTCAuthorityACKPending = ""
    '            TNT.UpdateDatabaseWithPTCComponents()
    '            TNT.UpdateTrackAuthority()
    '            TNT.AuthorityStatus = TNTAuthStatus.ACKNOWLEDGED.ToString
    '            TNT.UpdateTrackAuthorityStatus()
    '        End If

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Function

    Private Sub ReleaseVoidedAuthority(ByVal VoidList As ArrayList, ByVal Desgination As String)
        Dim TempVOIDAuthCollection As New ArrayList
        Dim TAT As TrackAuthority.TrackAuthority = Nothing
        Dim VoidedTAT As TrackAuthority.TrackAuthority = Nothing
        If VoidList Is Nothing Then Exit Sub
        If VoidList.Count = 0 Then Exit Sub
        Try

            CopyArrayCollection(TempVOIDAuthCollection, VoidList)
            For Each VoidedTAT In TempVOIDAuthCollection
                If VoidedTAT IsNot Nothing Then
                    TAT = VoidedTAT
                    If Not TAT Is Nothing Then
                        PerformTNTAuthorityRemove(TAT, Now, Desgination)
                    End If
                End If
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try

    End Sub


    Public Sub PerformPTCTNTRollUp(ByVal RollUpAuthority As TMDS.TrackAuthority.TrackAuthority, ByVal RollupAuthorityReleaseCollection As ArrayList, ByVal PTCTrain As TrainID, ByVal RollupList As ArrayList, ByVal RollUpEventText As String)
        Dim ReducedTxt As String = "ROLLUP EVENT FROM: "
        Dim RollUpEventTextData() As String
        Try
            If optDisableFlashingTrackForPTC Then RollUpAuthority.FlasherMode(False) : RollUpAuthority.FlasherMode(False, True)
            RollUpAuthority.PTCRollEventText = RollUpEventText
            RollUpEventTextData = RollUpEventText.Split("|")
            If RollUpEventTextData.GetUpperBound(0) > 2 Then
                ReducedTxt = Space(5) & ReducedTxt & RollUpEventTextData(0) & " TO " & RollUpEventTextData(1)
                RollUpAuthority.PTCReducedRollEventText = ReducedTxt
            End If
            RollUpAuthority.PtcPerformRollup(PTCTrain, ReducedTxt, RollupList)
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Function PerformPTCTNTExtendTime(ByVal TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID) As Boolean
        Dim ReleaseSuccess As Boolean = False
        Try
            TNT.FlasherMode(False)
            If PTCTrainID Is Nothing Then Return False
            If optEnablePTCBOSVETMSMode Then
                TNT.VETMS_PerformModify(PTCTrainID)
                Return True
            End If
            Return False
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
    End Function

    Private Sub PerformTNTAuthorityRemove(ByVal AuthorityToRemove As TrackAuthority.TrackAuthority, ByVal RClearTime As Date, ByVal Desgination As String)
        Dim UpdateCrew As Boolean = False
        Try
            Dim gHistoryMessage As String = ""
            AuthorityToRemove.ReportClear = RClearTime 'Me.ReportClear
            AuthorityToRemove.CopiedBy = Desgination.ToUpper.Trim

            If AuthorityToRemove.UpdateTrackAuthorityReportClearTime(True) Then
                If AuthorityToRemove.RemoveActiveAuthorityFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                    If AuthorityToRemove.RemoveActiveAuthorityFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        AuthorityToRemove.FlasherMode(False)
                        TMDS.GlobalDeclarations.OtherTrackAuthorities.Remove(AuthorityToRemove)
                        Dim SummItem2 As New SummaryItem("AUTHORITY", "REMOVE", AuthorityToRemove)
                        SyncLock gSummaryAuthQueue.SyncRoot
                            gSummaryAuthQueue.Enqueue(SummItem2)
                        End SyncLock
                        SystemEvent("AUTHORITY REMOVED MESSAGE RECEIPT: " & AuthorityToRemove.AuthorityUID)
                    End If
                Else
                    gHistoryMessage = "SYSTEM DATABASE ERROR! PLEASE CONSIDER TRACK AUTHORITY (" & AuthorityToRemove.AuthorityUID & ") NOT REMOVED! CONTACT SYSTEM ADMINISTRATOR IMMEDIATELY!"
                    SystemEvent(gHistoryMessage & "UNABLE TO DELETE AUTHORITY FROM DB")
                    MessageBox.Show(TMDS.SystemForms.MySystemForms.frmMainMDI, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Exit Sub
                End If
            Else
                gHistoryMessage = "SYSTEM DATABASE ERROR! PLEASE CONSIDER TRACK AUTHORITY (" & AuthorityToRemove.AuthorityUID & ") NOT REMOVED! CONTACT SYSTEM ADMINISTRATOR IMMEDIATELY!"
                SystemEvent(gHistoryMessage & "UNABLE TO UPDATE DB WITH REPORT CLEAR TIME")
                MessageBox.Show(TMDS.SystemForms.MySystemForms.frmMainMDI, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Exit Sub
            End If


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub ClearTNTPTCVariables(ByVal TNT As TMDS.TrackAuthority.TrackAuthority)
        Dim SummItem As SummaryItem = Nothing
        Try
            If TNT Is Nothing Then Exit Sub
            With TNT
                .FlasherMode(False)
                .ReportClear = Nothing
                .CopiedBy = ""
                .PTCStatus = "NONE"
                .PTCAuthorityACKPending = ""
                .PTCAuthorityComponents = New ArrayList
                .UpdateDatabaseWithPTCAuthorityStatus(TNT.PTCStatus)
                .UpdateDatabaseWithPTCComponents()
                .UpdateDatabaseWhenPTCAuthorityTransmissionFailed()
            End With

            'Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", TNT)
            SummItem = Nothing
            SummItem = New SummaryItem("AUTHORITY", "REMOVE", TNT)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem)
            End SyncLock

            'Dim SummItem1 As New SummaryItem("AUTHORITY", "ADD", TNT)
            SummItem = Nothing
            SummItem = New SummaryItem("AUTHORITY", "ADD", TNT)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem)
            End SyncLock

            'Dim SummItem2 As New SummaryItem("SUB-BULLETIN", "REFRESH", Nothing)
            'SyncLock gSubBulletinsMessageSummaryQueue.SyncRoot
            '    gSubBulletinsMessageSummaryQueue.Enqueue(SummItem2)
            'End SyncLock

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            SummItem = Nothing
        End Try
    End Sub

#End Region

#Region " CP PSS-EMT PTC "

    'Public Function PerformPTCSplAuthIssue(ByRef SPLAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType, ByVal HasLeadEngineChange As Boolean) As Boolean
    '    Try
    '        If SPLAuth Is Nothing Then Return False
    '        'If PTCTrainID Is Nothing Then Return False
    '        If optDisableFlashingTrackForPTC Then SPLAuth.FlasherMode(False)
    '        SPLAuth.VETMS_PerformIssuance()
    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Function

    'Public Function PerformPTCSPLAuthCancel(ByRef SplAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType, ByVal PTCTrainID As TrainID, ByVal HasLeadEngineChange As Boolean) As Boolean
    '    Dim ReleaseSuccess As Boolean = False
    '    Try
    '        If PTCTrainID Is Nothing Then Return False
    '        If Not optEnablePTCBOSVETMSMode Then Return False
    '        If SplAuth Is Nothing Then Return False
    '        If optDisableFlashingTrackForPTC Then SplAuth.FlasherMode(False)
    '        'If optEnablePTCBOSVETMSMode Then
    '        '    SplAuth.VETMS_PerformRelease(PTCTrainID)
    '        '    Return True
    '        'End If
    '        'Return False


    '        SplAuth.VETMS_PerformRelease(PTCTrainID)
    '        Dim WaitTime As Date = Now
    '        Do
    '            Select Case SplAuth.PTCStatus
    '                Case "RELEASE-SUCCESSFUL"
    '                    ReleaseSuccess = True
    '                    Exit Do
    '                Case "RELEASE-FAILED"
    '                    ReleaseSuccess = False
    '                    Exit Do
    '            End Select
    '            Threading.Thread.Sleep(1000)
    '            If WaitTime < Now.AddMinutes(-2) Then ReleaseSuccess = False : Exit Do
    '        Loop

    '        If Not ReleaseSuccess Then
    '            ClearSplAuthVariables(SplAuth)
    '        Else
    '            PerformSPLAuthorityRemove(SplAuth, PTCTrainID, HasLeadEngineChange)
    '        End If

    '        Return ReleaseSuccess




    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '        Return False
    '    End Try
    'End Function

    'Public Function PerformPTCSPLAuthFullFill(ByRef SplAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType, ByVal PTCTrainID As TrainID, ByVal HasLeadEngineChange As Boolean) As Boolean
    '    Dim ReleaseSuccess As Boolean = False
    '    Try
    '        If PTCTrainID Is Nothing Then Return False
    '        If Not optEnablePTCBOSVETMSMode Then Return False
    '        If SplAuth Is Nothing Then Return False
    '        If optDisableFlashingTrackForPTC Then SplAuth.FlasherMode(False)
    '        'If optEnablePTCBOSVETMSMode Then
    '        '    SplAuth.VETMS_PerformRelease(PTCTrainID)
    '        '    Return True
    '        'End If
    '        'Return False
    '        SplAuth.PTCMarkedToBeVoidedElectronically = True
    '        SplAuth.PTCTypeAuthority = True
    '        SplAuth.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
    '        SplAuth.PTCAuthorityComponents = SplAuth.ComponentsInSelection
    '        SplAuth.SetPTCAuthorityFlasherStatus()
    '        If Not optDisableFlashingTrackForPTC Then SplAuth.FlasherMode(True, True)
    '        SplAuth.VETMS_PerformRelease(PTCTrainID)

    '        Dim WaitTime As Date = Now
    '        Do
    '            Select Case SplAuth.PTCStatus
    '                Case "RELEASE-SUCCESSFUL"
    '                    ReleaseSuccess = True
    '                    Exit Do
    '                Case "RELEASE-FAILED"
    '                    ReleaseSuccess = False
    '                    Exit Do
    '            End Select
    '            Threading.Thread.Sleep(1000)
    '            If WaitTime < Now.AddMinutes(-2) Then ReleaseSuccess = False : Exit Do
    '        Loop

    '        If Not ReleaseSuccess Then
    '            ClearSplAuthVariables(SplAuth)
    '        End If

    '        Return ReleaseSuccess

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '        Return False
    '    End Try
    'End Function

    'Private Sub PerformSPLAuthorityRemove(ByVal mySplAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType, ByVal myTrain As TrainID, ByVal HasNewLeadEngine As Boolean)
    '    Try
    '        If Not mySplAuth Is Nothing Then

    '            mySplAuth.RemoveCPSplAuthorityFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionDynamic)
    '            RemoveCPPSS(mySplAuth)
    '            gOtherSpecialAuthorityCollection.Remove(mySplAuth)
    '            If HasNewLeadEngine AndAlso myTrain IsNot Nothing Then
    '                Dim AuthDesg As AuthorityDesignation = Nothing
    '                Dim ActiveAuthDesgination As New ArrayList
    '                Dim CPLeadEngRC As New clsLeadEngineChange(myTrain)
    '                Try
    '                    If Not CPLeadEngRC.CheckIfTrainHasAuthorities Then
    '                        For Each AuthDesg In myTrain.AuthDesignations
    '                            If AuthDesg.InActive Then
    '                                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
    '                            End If
    '                        Next
    '                        For Each AuthDesg In ActiveAuthDesgination
    '                            myTrain.AuthDesignations.Remove(AuthDesg)
    '                        Next
    '                        myTrain.UpdateAuthorityDesignationToDB()
    '                        myTrain.IdentifingUnit = myTrain.PendingLeadEngine
    '                        myTrain.PendingLeadEngine = ""
    '                        myTrain.DoUpdateTrainIDIntoDatabase(myTrain.myTrack, True)
    '                        myTrain.myTrack.InvalidateTrainID()
    '                        myTrain.myTrack.InitTrainDisplay()
    '                        Dim SummItem1 As New SummaryItem("TRAIN", "FORCE-REFRESH", myTrain)
    '                        SyncLock TMDS.GlobalDeclarations.gSummaryTrainQueue.SyncRoot
    '                            TMDS.GlobalDeclarations.gSummaryTrainQueue.Enqueue(SummItem1)
    '                        End SyncLock
    '                    End If
    '                Catch ex As Exception
    '                    GetErrorDetails(ex)
    '                Finally
    '                    ActiveAuthDesgination = Nothing
    '                    AuthDesg = Nothing
    '                End Try
    '            End If
    '            mySplAuth.FlasherMode(False)
    '            If Not optEnablePTCBOSVETMSMode Then mySplAuth.FlasherMode(False, True)
    '            Dim SummItem As New SummaryItem("SPLAUTHORITY", "REMOVE", mySplAuth)
    '            SyncLock gSummaryAuthQueue
    '                gSummaryAuthQueue.Enqueue(SummItem)
    '            End SyncLock
    '        End If
    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '    End Try
    'End Sub
#End Region

    Private Sub UpdateLeadEngine(ByVal HasLeadEngineChange As Boolean, ByVal PTCTrainID As TrainID)
        Try
            If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                Dim AuthDesg As AuthorityDesignation = Nothing
                Dim ActiveAuthDesgination As New ArrayList
                Try
                    For Each AuthDesg In PTCTrainID.AuthDesignations
                        If AuthDesg.InActive Then
                            If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                        End If
                    Next
                    For Each AuthDesg In ActiveAuthDesgination
                        PTCTrainID.AuthDesignations.Remove(AuthDesg)
                    Next
                    PTCTrainID.UpdateAuthorityDesignationToDB()
                    PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                    PTCTrainID.PendingLeadEngine = ""
                    PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                    PTCTrainID.myTrack.InvalidateTrainID()
                    PTCTrainID.myTrack.InitTrainDisplay()
                Catch ex As Exception
                    GetErrorDetails(ex)
                Finally
                    ActiveAuthDesgination = Nothing
                    AuthDesg = Nothing
                End Try
            End If

        Catch ex As Exception

        End Try
    End Sub

    Public Sub ClearSplAuthVariables(ByVal SplAuth As TMDS.TrackAuthority.OtherSpecialAuthorityType)
        Dim SummItem As SummaryItem = Nothing
        Try
            If SplAuth Is Nothing Then Exit Sub
            With SplAuth
                .FlasherMode(False)
                .PTCStatus = "NONE"
                .PTCAuthorityACKPending = ""
                .PTCAuthorityComponents = New ArrayList
                .UpdateDatabaseWithPTCAuthorityStatus(SplAuth.PTCStatus)
                .UpdateDatabaseWithPTCComponents()
                '.UpdateDatabaseWhenPTCAuthorityTransmissionFailed()
            End With


            SummItem = Nothing
            SummItem = New SummaryItem("SPLAUTHORITY", "REMOVE", SplAuth)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem)
            End SyncLock


            SummItem = Nothing
            SummItem = New SummaryItem("SPLAUTHORITY", "ADD", SplAuth)
            SyncLock gSummaryAuthQueue.SyncRoot
                gSummaryAuthQueue.Enqueue(SummItem)
            End SyncLock

            'Dim SummItem2 As New SummaryItem("SUB-BULLETIN", "REFRESH", Nothing)
            'SyncLock gSubBulletinsMessageSummaryQueue.SyncRoot
            '    gSubBulletinsMessageSummaryQueue.Enqueue(SummItem2)
            'End SyncLock

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            SummItem = Nothing
        End Try
    End Sub





#End Region

    Public Function RearrangeCompCollection2(ByVal CollectionToBeReaaranged As ArrayList, ByVal IsPSSType As Boolean, Optional SortDEsc As Boolean = False) As ArrayList
        Dim LeftTrack As Track = Nothing
        Dim RightTrack As Track = Nothing
        Dim Comp As Object = Nothing
        Dim TempCompCollection As New ArrayList
        Try
            GetRightLeftTrackFromCollection(RightTrack, LeftTrack, CollectionToBeReaaranged)
            Dim tempArray As New Collection
            For Each Comp In CollectionToBeReaaranged
                Dim compSort As New clsCompSort
                Select Case Comp.GetType.ToString
                    Case gTrackComponentName
                        With compSort
                            .myUID = Comp.GUID
                            If Comp Is LeftTrack Then
                                .MilePost = Comp.RightLimitMilePost
                            Else
                                .MilePost = Comp.LeftLimitMilePost
                            End If
                            .myType = gTrackComponentName
                            tempArray.Add(compSort)
                        End With
                    Case gSwitchComponentName
                        With compSort
                            .myUID = Comp.GUID
                            .MilePost = Comp.MilePost
                            .myType = gSwitchComponentName
                            tempArray.Add(compSort)
                        End With
                End Select
            Next
            SortCollection(tempArray, "MilePost", SortDEsc)
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

#End Region

#Region " ARRC SPECIFIC FUNCTIONS "

    Public Function IsOKTOFinalTrainARRC(ByVal trn As TrainID, Optional ByVal LPoint As Object = Nothing) As Boolean
        Dim OkToRelease As Boolean = True
        Dim auth As Object = Nothing
        Dim MessageList As New ArrayList()
        Dim TmpWorkTask As New ArrayList
        Dim CheckForEnrouteValidation As Boolean = False
        Dim EnrouteValidationCompleted As Boolean = False
        Dim TrainHadTGBOEvent As Boolean = False
        Try
            '   MessageList.Add(Chr(149) & Space(3) & "Authority " & SFA.AuthorityUID & " Has Item 9 checked")
            If gDeveloperSystem Then Return True
            If Not String.IsNullOrEmpty(trn.TGBONumber) AndAlso trn.TGBONumber <> String.Empty Then
                Select Case trn.TGBOStatus.Trim.ToUpper
                    Case "CANCELLED", "RELEASED"
                        TrainHadTGBOEvent = True
                End Select
            End If
            If Not TrainHadTGBOEvent Then
                Dim TGBOEvent As New ArrayList
                TGBOEvent = TMDS.SystemUtilities.TrainFunction.GetTrainEvents(trn, "TGBO EVENT")
                If TGBOEvent IsNot Nothing AndAlso TGBOEvent.Count > 0 Then
                    TrainHadTGBOEvent = True
                End If
            End If
            If Not TrainHadTGBOEvent AndAlso CheckIfTrainHasActiveEnrouteReport(trn.Name) Then
                MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE ENROUTE FAILURE EVENT")
                OkToRelease = False
                EnrouteValidationCompleted = True
            ElseIf Not TrainHadTGBOEvent Then
                Return True
            End If

            'If trn.TGBONumber <> String.Empty Then
            '    Select Case trn.TGBOStatus.Trim.ToUpper
            '        Case "", "CANCELLED", "RELEASED"
            '            CheckForEnrouteValidation = True
            '            'Return True
            '        Case Else
            '            'OkToRelease = False
            '            'MessageList.Add(Chr(149) & Space(3) & "ACTVIE TGBO")
            '    End Select
            'Else
            '    Dim TGBOEvent As New ArrayList
            '    TGBOEvent = TMDS.SystemUtilities.TrainFunction.GetTrainEvents(trn, "TGBO EVENT")
            '    If TGBOEvent IsNot Nothing AndAlso TGBOEvent.Count > 0 Then
            '    Else
            '        CheckForEnrouteValidation = True
            '    End If
            'End If


            'If CheckForEnrouteValidation AndAlso CheckIfTrainHasActiveEnrouteReport(trn.Name) Then
            '    MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE ENROUTE FAILURE EVENT")
            '    OkToRelease = False
            '    EnrouteValidationCompleted = True
            'ElseIf CheckForEnrouteValidation Then
            '    Return True
            'End If

            If trn.IdentifingUnit.Trim = "" Then MessageList.Add(Chr(149) & Space(3) & "MISSING LEAD ENGINE") : OkToRelease = False
            If trn.OSTimes Is Nothing Then MessageList.Add(Chr(149) & Space(3) & "MISSING OS TIME(s)") : OkToRelease = False
            If trn.OSTimes IsNot Nothing AndAlso trn.OSTimes.Count = 0 Then MessageList.Add(Chr(149) & Space(3) & "MISSING OS TIME(s)") : OkToRelease = False
            If trn.CrewCurrent IsNot Nothing Then
                Dim HasAleastOneValidCrew As Boolean = True
                With trn.CrewCurrent
                    If CheckValidTrainCrewName(.Engineer.NameOfPerson) OrElse CheckValidTrainCrewName(.Conductor.NameOfPerson) OrElse CheckValidTrainCrewName(.ConductorTwo.NameOfPerson) _
                     OrElse CheckValidTrainCrewName(.BrakeMan.NameOfPerson) OrElse CheckValidTrainCrewName(.BrakeManTwo.NameOfPerson) OrElse CheckValidTrainCrewName(.EngineerTrainee.NameOfPerson) _
                     OrElse CheckValidTrainCrewName(.ConductorTrainee.NameOfPerson) Then
                    Else
                        HasAleastOneValidCrew = False
                    End If
                End With
                If Not HasAleastOneValidCrew Then
                    OkToRelease = False
                    MessageList.Add(Chr(149) & Space(3) & "MISSING CREW")
                End If
            Else
                MessageList.Add(Chr(149) & Space(3) & "MISSING CREW")
                OkToRelease = False
            End If


            Dim ArrTime As Date
            Dim DepTime As Date
            If Date.TryParse(trn.ArrTime, ArrTime) Then
                If Not IsValidDate(ArrTime.ToShortDateString) Then
                    MessageList.Add(Chr(149) & Space(3) & "MISSING ARRIVAL TIME")
                    OkToRelease = False
                Else
                    If ArrTime.Hour > 0 OrElse ArrTime.Minute > 0 Then
                    Else
                        MessageList.Add(Chr(149) & Space(3) & "MISSING ARRIVAL TIME")
                        OkToRelease = False
                    End If
                End If
            Else
                MessageList.Add(Chr(149) & Space(3) & "MISSING ARRIVAL TIME")
                OkToRelease = False
            End If
            If Date.TryParse(trn.DepTime, DepTime) Then
                If Not IsValidDate(DepTime.ToShortDateString) Then
                    MessageList.Add(Chr(149) & Space(3) & "MISSING DEPARTURE TIME")
                    OkToRelease = False
                Else
                    If DepTime.Hour > 0 OrElse DepTime.Minute > 0 Then
                    Else
                        MessageList.Add(Chr(149) & Space(3) & "MISSING DEPARTURE TIME")
                        OkToRelease = False
                    End If
                End If

            Else
                MessageList.Add(Chr(149) & Space(3) & "MISSING DEPARTURE TIME")
                OkToRelease = False
            End If

            Dim WeatherEvent As New ArrayList
            WeatherEvent = TMDS.SystemUtilities.TrainFunction.GetTrainEvents(trn, "WEATHER EVENT")
            If WeatherEvent Is Nothing OrElse WeatherEvent.Count = 0 Then
                MessageList.Add(Chr(149) & Space(3) & "MISSING WEATHER EVENT")
                OkToRelease = False
            End If

            Dim TrainHasAuthority As Boolean = False
            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE WARRANTS")
                            TrainHasAuthority = True
                            OkToRelease = False
                        End If
                    End If
                Next
            End If
            auth = Nothing
            If Not TrainHasAuthority AndAlso gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE WARRANTS")
                            OkToRelease = False
                        End If
                    End If
                Next
            End If

            If Not EnrouteValidationCompleted AndAlso CheckIfTrainHasActiveEnrouteReport(trn.Name) Then
                MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE ENROUTE FAILURE EVENT")
                OkToRelease = False
            End If


            CopyArrayCollection(TmpWorkTask, gWorkTasks)
            Dim wt As WorkTask
            For Each wt In TmpWorkTask
                If wt.EntityGUID = trn.TrainGUID Then
                    MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE WORK TASK")
                    OkToRelease = False
                    Exit For
                End If
            Next

            If Not OkToRelease Then
                MessageList.Insert(0, "TRAIN CANNOT BE FINALED BECAUSE:")
                MessageList.Insert(1, "")
                MessageList.Add("")
                ShowCustomMessageBox(MessageList, "TRAIN DELETE CHECK", False, LPoint)
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        Finally
            TmpWorkTask = Nothing
        End Try
        Return OkToRelease
    End Function

    Private Function CheckValidTrainCrewName(ByVal CrewString As String) As Boolean
        Try
            If String.IsNullOrEmpty(CrewString) Then
                Return False
            Else
                Select Case CrewString.ToUpper.Trim
                    Case "NONE", "----", ""
                        Return False
                End Select
            End If
            If CrewString.ToUpper.Trim = "NONE" Or CrewString.ToUpper.Trim = "----" Or CrewString.Trim = "" Then
                Return False
            End If
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Function

    Public Function RemoveTGBOOnTrainDelete(ByVal trn As TrainID) As Boolean
        Dim MyTgboReleasedSucessfully As Boolean = False
        Dim MyTgbo As TabularGeneralBulletin = Nothing
        Dim LPoint As Point
        Dim MessageList As New ArrayList()
        Try
            If String.IsNullOrEmpty(trn.TGBONumber) Then Return True
            MyTgbo = TMDS.SystemUtilities.FetchData.FindTGBOByTrainSymbol(trn.Name)
            If MyTgbo IsNot Nothing Then
                Dim frmBulletinHMI As New frmBulletinHMI
                Try
                    frmBulletinHMI.theTGBO = MyTgbo
                    frmBulletinHMI.LoadForm(MyTgbo.TypeOfBulletin, "RELEASE", Nothing, Nothing, MyTgbo)
                    LPoint = New Point(trn.myTrack.CurrentDrawingForm.Left + 100, trn.myTrack.CurrentDrawingForm.Top + 150)
                    frmBulletinHMI.Owner = trn.myTrack.CurrentDrawingForm
                    frmBulletinHMI.Location = LPoint
                    frmBulletinHMI.ShowDialog()
                    If frmBulletinHMI.GBOFormFeedBack = 6 Then MyTgboReleasedSucessfully = True
                Catch ex As Exception
                    GetErrorDetails(ex)
                    Return False
                Finally
                    frmBulletinHMI.Dispose()
                    frmBulletinHMI = Nothing
                    MyTgbo = Nothing
                End Try
                If Not MyTgboReleasedSucessfully Then
                    MessageList.Insert(0, "TRAIN CANNOT BE FINALED BECAUSE:")
                    MessageList.Insert(1, "")
                    MessageList.Add(Chr(149) & Space(3) & "TGBO RELEASE FAILED")
                    ShowCustomMessageBox(MessageList, "TRAIN DELETE CHECK", False, LPoint)
                    Return False
                End If
            End If

            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try

    End Function

    Public Function IsOKTOReleaseTrainARRC(ByVal trn As TrainID, ByRef ErrorMessage As ArrayList, Optional ByVal LPoint As Object = Nothing, Optional myParent As Object = Nothing) As Boolean
        Dim OkToRelease As Boolean = True
        Dim auth As Object = Nothing
        'Dim MessageList As New ArrayList()
        Dim MyTGBOCollection As New ArrayList
        Dim TGBO As TabularGeneralBulletin = Nothing
        Dim FirstHeader As String = ""
        Try
            '   MessageList.Add(Chr(149) & Space(3) & "Authority " & SFA.AuthorityUID & " Has Item 9 checked")
            If trn Is Nothing Then Return False
            ErrorMessage = New ArrayList

            If trn.CheckMyPTCStatus Then
                With ErrorMessage
                    .Add("THIS TRAIN[ " & trn.Name & " ] IS PTC ACTIVE. ")
                    .Add("TRAIN HAS TO BE IN CUTOUT STATE TO BE RELEASED")
                    .Add("REQUEST DENIED!")
                End With
                ShowCustomMessageBox(ErrorMessage, "RELEASE VALIDATION", False, LPoint)
                ErrorMessage = Nothing
                Return False
            End If
            ErrorMessage = New ArrayList
            FirstHeader = "THIS TRAIN [ " & trn.Name & " ] HAS "

            ErrorMessage.Add("THIS TRAIN [ " & trn.Name & " ] HAS ")

            MyTGBOCollection = TMDS.SystemUtilities.FetchData.FindAllTGBOsByTrainSymbol(trn.Name)
            If MyTGBOCollection IsNot Nothing AndAlso MyTGBOCollection.Count > 0 Then
                For Each TGBO In MyTGBOCollection
                    Select Case TGBO.Status
                        Case "ACTIVE"
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ACTIVE TGBO [" & TGBO.UID & "]")
                        Case "PENDING"
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "PENDING TGBO [" & TGBO.UID & "]")
                    End Select
                Next
                If ShowCustomMessageBox(ErrorMessage, "RELEASE VALIDATION", True, LPoint, False, True, myParent, False, "", "CONTINUE|CANCEL", True) <> 6 Then
                    ErrorMessage = Nothing
                    Return False
                Else
                    ErrorMessage = New ArrayList
                    ErrorMessage.Add("THIS TRAIN [ " & trn.Name & " ] HAS ")
                End If
            End If

            If CheckIfTrainHasActiveEnrouteReport(trn.Name) Then
                ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE ENROUTE FAILURE EVENT")
                OkToRelease = False

            End If


            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE AUTHORITIES.")
                            OkToRelease = False
                            Exit For
                        End If
                    End If
                Next
            End If
            auth = Nothing
            If gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE AUTHORITIES.")
                            OkToRelease = False
                            Exit For
                        End If
                    End If
                Next
            End If
            If OkToRelease Then ErrorMessage = New ArrayList
        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
        Return OkToRelease
    End Function

    Public Sub GetTrainReleaseErrorMessage(ByRef ErrorMessage As ArrayList, TrainName As String)
        Try
            ErrorMessage = New ArrayList
            ErrorMessage.Add("THIS TRAIN [ " & TrainName & " ] HAS ")
            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE WARRANTS.")
            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ACTIVE OR PENDING TGBO. ")
            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE ENROUTE FAILURE.")
            ErrorMessage.Add("RELEASE REQUEST IS NOT VALID.")
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Function CheckIfTrainHasTGBO(ByVal trn As TrainID) As Boolean
        Dim MyTGBOCollection As New ArrayList
        Dim TGBO As TabularGeneralBulletin = Nothing
        Try

        Catch ex As Exception

        End Try
    End Function

    Public Function CheckIfEngineIsActiveOnDifferentTrain(TrainToCheck As TrainID, UnitTocheck As String, ByVal pForm As Object, LaunchPoint As Point) As Boolean
        Dim dbComm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim ExitingSymbol As String = ""
        Dim ExistingTrnActive As Boolean = False
        Dim IgnoreChange As Boolean = False
        Dim ErrorMsg As New ArrayList
        Try
            'Dim ExistingTrnActive As Boolean = False
            'Dim ExistTrnSymbol As String = GetTrainWithLeadLocomotive(UnitTocheck, "", TrainToCheck.Name, ExistingTrnActive)
            Dim SQLquery As String = "SELECT Symbol,EngineID,Subdivision,TrainIsNotActive FROM " & gtblTrainsActive & " WHERE TrainIsNotActive=0 AND EngineID = '" & UnitTocheck & "' and symbol <> '" & TrainToCheck.Name & "'"
            Using dbConn As New OleDbConnection(gConnectionStringTrain)
                dbConn.Open()
                dbComm = New OleDbCommand(SQLquery, dbConn)
                dr = dbComm.ExecuteReader
                If dr.HasRows Then
                    While dr.Read
                        ExitingSymbol = dr.GetString(0)
                        ExistingTrnActive = dr.GetValue(3)
                        If Not ExistingTrnActive Then
                            IgnoreChange = True
                        End If
                    End While
                End If
                dr.Close()
                dbConn.Close()
            End Using
            If IgnoreChange Then
                With ErrorMsg
                    .Add(ExitingSymbol & " IS IN ACTIVE STATE ")
                    .Add("PLEASE DE-ACTIVATE IN ORDER TO ACTIVATE THIS ENGINE")
                    .Add("REQUEST DENIED ")
                End With
                ShowCustomMessageBox(ErrorMsg, "Engine Validation", False, LaunchPoint, True, True, pForm)
                Return True
            End If
            Return False

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    'Private Function CheckForTrainIDOrOccupancyInClearedRoute(ByVal LaunchPoint As Object, ByVal TrackToCheck As Track) As Boolean
    '    Dim TempArray As New ArrayList
    '    'Dim Comp As Object = Nothing
    '    Dim trn As TrainID = Nothing
    '    Dim mw As MOWID = Nothing
    '    Dim MessageList As New ArrayList
    '    Dim HasTrain As Boolean = False
    '    Try
    '        If Not TrackToCheck.GetType.ToString = gTrackComponentName Then Return True
    '        If TrackToCheck.TrainIDs IsNot Nothing AndAlso TrackToCheck.TrainIDs.Count > 0 Then
    '            For Each trn In TrackToCheck.TrainIDs
    '                If Me.Name <> trn.Name AndAlso Not TempArray.Contains(trn.Name) Then
    '                    TempArray.Add(trn.Name)
    '                    Exit For
    '                End If
    '            Next
    '        End If

    '        For Each Comp In CompCollection
    '            If Comp.GetType.ToString = gTrackComponentName Then
    '                If Comp.TrainIDs IsNot Nothing AndAlso Comp.TrainIDs.Count > 0 Then
    '                    For Each trn In Comp.TrainIDs
    '                        If Me.Name <> trn.Name AndAlso Not TempArray.Contains(trn.Name) Then
    '                            TempArray.Add(trn.Name)
    '                            Exit For
    '                        End If
    '                    Next
    '                End If
    '                If Comp.MOWIDs IsNot Nothing AndAlso Comp.MOWIDs.Count > 0 Then
    '                    For Each mw In Comp.MOWIDs
    '                        If Not TempArray.Contains(mw.DisplayText) Then
    '                            TempArray.Add(mw.DisplayText)
    '                            Exit For
    '                        End If
    '                    Next

    '                End If
    '            End If
    '        Next

    '        If TempArray IsNot Nothing AndAlso TempArray.Count > 0 Then
    '            With MessageList
    '                .Add("ONE OR MORE TRAIN/MOW IDS FOUND IN THE AUTHORITY LIMITS")
    '                .Add("")
    '                .Add("HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT?")

    '            End With
    '            If ShowCustomMessageBox(MessageList, "CONFLICT CHECK", True, LaunchPoint) <> 6 Then
    '                SystemEvent("DISPATCHER CLICKED [NO] TO - ONE OR MORE TRAINS FOUND IN PSS/EMT SELECTION! PLEASE BE SURE TO RESOLVE THIS CONFLICT. ")
    '                ClearSystemVariables()
    '                Return False
    '            End If
    '        End If

    '        Return True

    '    Catch ex As Exception
    '        GetErrorDetails(ex)
    '        Return False
    '    Finally
    '        TempArray = Nothing
    '        Comp = Nothing
    '        MessageList = Nothing
    '        trn = Nothing
    '    End Try
    'End Function

    Public Function ValidateManualOperateMode(thisControlPoint As ControlPoint) As Boolean
        Try
            'If gMaintenanceMode Then Return False
            If optEnableARRCSpecificOptions AndAlso thisControlPoint.TunnelAutomaticMode AndAlso thisControlPoint.ManualOperateMode AndAlso Not thisControlPoint.devMOK.DeviceState Then
                Return True
            ElseIf optEnableARRCSpecificOptions AndAlso thisControlPoint.TunnelAutomaticMode AndAlso Not thisControlPoint.ManualOperateMode AndAlso thisControlPoint.devMOK.DeviceState Then
                Return False
            ElseIf thisControlPoint.ManualOperateMode Then
                Return False
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Public Sub ReinitializeExecpListAllBulletin()
        Dim dr As OleDbDataReader = Nothing
        Dim dbComm As OleDbCommand = Nothing
        Dim SQLQuery As String = ""
        Dim sb As SubBulletin = Nothing
        Try
            Using dbConn As New OleDbConnection(gConnectionStringBulletin)
                dbConn.Open()
                For Each sb In BulletinCollection
                    SQLQuery = "SELECT [ExceptionList], [Status] ,[ConfirmationData] FROM [tblSystemBulletins] WHERE [SystemUID]='" & sb.myGUID & "'"
                    dbComm = New OleDbCommand(SQLQuery, dbConn)
                    dr = dbComm.ExecuteReader
                    If dr.HasRows Then
                        While dr.Read
                            Dim ExceptionList As String = String.Empty
                            If Not dr.IsDBNull(0) Then ExceptionList = Trim(dr.GetValue(0))
                            sb.myExceptionList = CreateExceptionListARRC(ExceptionList, sb)
                            If Not dr.IsDBNull(1) Then sb.myStatus = dr.GetValue(1).ToString.Trim
                            If Not dr.IsDBNull(2) Then sb.ConfirmationData = dr.GetValue(2).ToString.Trim
                        End While
                    End If
                    dr.Close()
                Next


                dbConn.Close()
            End Using

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            If dbComm IsNot Nothing Then dbComm.Dispose()
            dbComm = Nothing
            dr = Nothing
        End Try
    End Sub

    Public Function CreateExceptionListARRC(ByVal sExceptions As String, ByVal b As SubBulletin) As ArrayList
        Dim ExcepList As New ArrayList
        Try

            If Trim(sExceptions) = "" Then Return ExcepList
            Dim Exceps() As String = sExceptions.Split("|")
            Dim e As String
            For Each e In Exceps
                Dim data() As String = e.Split(";")
                If data.Length >= 5 Then
                    Dim ex As New TabularGeneralBulletinException
                    If Not optEnableTGBOOperationsForCP Then
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
                    Else
                        ex = GetInactiveSubBulletinExceptionByGUID(data(0))
                        If ex IsNot Nothing Then
                            ex.theTBGOUID = data(4)
                        End If
                    End If
                    If ex IsNot Nothing Then
                        If Not ExcepList.Contains(ex) Then ExcepList.Add(ex)
                    End If

                End If
            Next
            Return ExcepList
        Catch exp As Exception
            GetErrorDetails(exp)
            Return Nothing
        End Try
    End Function

    Private Function GetInactiveSubBulletinExceptionByGUID(ByVal ExpGUID As String) As TabularGeneralBulletinException
        Dim TBExp As New TabularGeneralBulletinException
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing

        Dim Query As String = "SELECT *  FROM tblSystemTabularGeneralBulletinExceptionInactive WHERE SystemUID='" & ExpGUID & "'"
        Try
            Using DbConn As New OleDbConnection(gConnectionStringDynamic)
                DbConn.Open()
                dbcomm = New OleDbCommand(Query, DbConn)
                dr = dbcomm.ExecuteReader
                If dr.HasRows Then
                    While dr.Read()
                        With TBExp
                            If Not dr.IsDBNull(0) Then .SystemUID = dr.GetString(0)
                            If Not dr.IsDBNull(1) Then .theTBGOSytemUID = dr.GetString(1)
                            If Not dr.IsDBNull(2) Then .theGBOUID = dr.GetString(2)
                            If Not dr.IsDBNull(3) Then .theDesignation = dr.GetString(3)
                            If Not dr.IsDBNull(4) Then .theSubUID = dr.GetInt32(4)
                            If Not dr.IsDBNull(5) Then .theTGBORouteCodeName = dr.GetString(5)
                            If Not dr.IsDBNull(6) Then .Status = dr.GetString(6)
                            If Not dr.IsDBNull(7) Then .Status = dr.GetString(7)
                            If Not dr.IsDBNull(8) Then .ProtectionType = dr.GetString(8)
                            If Not dr.IsDBNull(9) Then .HandlingType = dr.GetString(9)
                            If Not dr.IsDBNull(10) Then .InitialsOfMitigator = dr.GetString(10)
                            If Not dr.IsDBNull(11) Then .SubDesignation = dr.GetString(11)
                            If Not dr.IsDBNull(12) Then .BulletinReadData = dr.GetString(12)
                            If Not dr.IsDBNull(13) Then .theBulletinType = dr.GetString(13)
                            If dr.FieldCount > 13 Then If Not dr.IsDBNull(14) Then .OKTime = Trim(dr.GetDateTime(14))
                            If dr.FieldCount > 14 Then If Not dr.IsDBNull(15) Then .theCancelledBy = dr.GetString(15)
                            If dr.FieldCount > 15 Then If Not dr.IsDBNull(16) Then .theCancelDate = Trim(dr.GetDateTime(16))
                            If dr.FieldCount > 16 Then If Not dr.IsDBNull(17) Then .CancelledBulletinGUID = Trim(dr.GetValue(17))
                        End With
                    End While
                    dr.Close()
                Else
                    If Not dr.IsClosed Then dr.Close()
                    dr = Nothing
                    Query = "SELECT *  FROM tblSystemTabularGeneralBulletinExceptionActive WHERE SystemUID='" & ExpGUID & "'"
                    dbcomm = New OleDbCommand(Query, DbConn)
                    dr = dbcomm.ExecuteReader
                    If dr.HasRows Then
                        While dr.Read()
                            With TBExp
                                If Not dr.IsDBNull(0) Then .SystemUID = dr.GetString(0)
                                If Not dr.IsDBNull(1) Then .theTBGOSytemUID = dr.GetString(1)
                                If Not dr.IsDBNull(2) Then .theGBOUID = dr.GetString(2)
                                If Not dr.IsDBNull(3) Then .theDesignation = dr.GetString(3)
                                If Not dr.IsDBNull(4) Then .theSubUID = dr.GetInt32(4)
                                If Not dr.IsDBNull(5) Then .theTGBORouteCodeName = dr.GetString(5)
                                If Not dr.IsDBNull(6) Then .Status = dr.GetString(6)
                                If Not dr.IsDBNull(7) Then .Status = dr.GetString(7)
                                If Not dr.IsDBNull(8) Then .ProtectionType = dr.GetString(8)
                                If Not dr.IsDBNull(9) Then .HandlingType = dr.GetString(9)
                                If Not dr.IsDBNull(10) Then .InitialsOfMitigator = dr.GetString(10)
                                If Not dr.IsDBNull(11) Then .SubDesignation = dr.GetString(11)
                                If Not dr.IsDBNull(12) Then .BulletinReadData = dr.GetString(12)
                                If Not dr.IsDBNull(13) Then .theBulletinType = dr.GetString(13)
                                If dr.FieldCount > 13 Then If Not dr.IsDBNull(14) Then .OKTime = Trim(dr.GetDateTime(14))
                                If dr.FieldCount > 14 Then If Not dr.IsDBNull(15) Then .theCancelledBy = dr.GetString(15)
                                If dr.FieldCount > 15 Then If Not dr.IsDBNull(16) Then .theCancelDate = Trim(dr.GetDateTime(16))
                                If dr.FieldCount > 16 Then If Not dr.IsDBNull(17) Then .CancelledBulletinGUID = Trim(dr.GetValue(17))
                            End With
                        End While
                        dr.Close()
                    End If
                End If

                DbConn.Close()
            End Using
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try


        Return TBExp
    End Function

#End Region

#Region " DENVER SPECIFIC FUNCTIONS "

    Public Function IsOKTOReleaseTrainDenver(ByVal trn As TrainID, ByRef ErrorMessage As ArrayList, Optional ByVal LPoint As Object = Nothing, Optional myParent As Object = Nothing) As Boolean
        Dim OkToRelease As Boolean = True
        Dim td As TrainDelay = Nothing
        Dim auth As Object = Nothing
        Try
            If trn Is Nothing Then Return False
            ErrorMessage = New ArrayList
            ErrorMessage.Add("THIS TRIP [ " & trn.Name & " ] HAS ")

            If trn.myDelayList IsNot Nothing AndAlso trn.myDelayList.Count > 0 Then
                For Each td In trn.myDelayList
                    If td.myDelayExcused = "NONE" Then
                        ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE UNEXCUSED DELAY(s).")
                        OkToRelease = False
                        Exit For
                    ElseIf td.EventDelayCode.Trim = "" Then
                        ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE DELAY(s) DO NOT HAVE DELAY CODE ASSIGNED")
                        OkToRelease = False
                        Exit For
                    End If
                Next
            End If
            If gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE AUTHORITIES.")
                            OkToRelease = False
                            Exit For
                        End If
                    End If
                Next
            End If
            If OkToRelease Then ErrorMessage = New ArrayList

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
        Return OkToRelease
    End Function

    Public Function IsOKTOFinalTrainDenver(ByVal trn As TrainID, Optional ByVal LPoint As Object = Nothing) As Boolean
        Dim OkToFinal As Boolean = True
        Dim td As TrainDelay = Nothing
        Dim ErrorMessage As New ArrayList()
        Dim auth As Object = Nothing
        Try
            If trn Is Nothing Then Return False
            ErrorMessage = New ArrayList
            ErrorMessage.Add("THIS TRAIN [ " & trn.Name & " ] HAS ")

            If trn.myDelayList IsNot Nothing AndAlso trn.myDelayList.Count > 0 Then
                For Each td In trn.myDelayList
                    If td.myDelayExcused = "NONE" Then
                        ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE UNEXCUSED DELAY(s).")
                        OkToFinal = False
                        Exit For
                    ElseIf td.EventDelayCode.Trim = "" Then
                        ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE DELAY(s) DO NOT HAVE DELAY CODE ASSIGNED")
                        OkToFinal = False
                        Exit For
                    End If
                Next
            End If
            If gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            ErrorMessage.Add(Space(3) & Chr(149) & Space(3) & "ONE OR MORE ACTIVE AUTHORITIES.")
                            OkToFinal = False
                            Exit For
                        End If
                    End If
                Next
            End If
            If OkToFinal Then ErrorMessage = New ArrayList

            If Not OkToFinal Then
                ErrorMessage.Insert(0, "TRIP CANNOT BE FINALED BECAUSE:")
                ErrorMessage.Insert(1, "")
                ErrorMessage.Add("")
                ShowCustomMessageBox(ErrorMessage, "TRIP DELETE CHECK", False, LPoint)
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
        Return OkToFinal
    End Function

    Public Sub GetTrainScheduleColor(TrainSchedule As TrainSchedule, Optional ByRef ScheduleColor As Object = Nothing, Optional ByRef strDEVSign As String = "")

        Dim dispColor As Color = Color.LightSeaGreen
        Dim ToolTipText As String = "NONE"
        Dim DispText As String = "NONE"
        Dim StationName As String = ""
        'Dim MystationCollection As New ArrayList
        'Dim SubDvn As Subdivision
        'Dim subStat As SubdivisionStation = Nothing

        Try
            Dim TdAmt As Double = TrainSchedule.myScheduleMinutesLate

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

            ScheduleColor = dispColor

            'If TdAmt > 0 Then
            '    ToolTipText = "SCH INFO:  -" & TdAmt.ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
            '    DispText = "-" & TdAmt.ToString
            '    strDEVSign = "-"
            '    If optEnableDenverSpecificOptions Then
            '        ToolTipText = "SCH INFO:  +" & TdAmt.ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
            '        DispText = "+" & TdAmt.ToString
            '        strDEVSign = "+"
            '    End If
            'Else
            '    ToolTipText = "SCH INFO: +" & Math.Abs(TdAmt).ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
            '    DispText = "+" & Math.Abs(TdAmt).ToString
            '    strDEVSign = "+"

            '    If optEnableDenverSpecificOptions Then
            '        ToolTipText = "SCH INFO: -" & Math.Abs(TdAmt).ToString & " TO " & trn.myScheduleDeviationStation.Trim.ToUpper
            '        DispText = "-" & Math.Abs(TdAmt).ToString
            '        strDEVSign = "-"
            '    End If
            'End If
            'DispText = TdAmt.ToString
            'If TdAmt > 0 Then
            '    strDEVSign = "-"
            '    If optEnableDenverSpecificOptions Then strDEVSign = "+"
            'Else
            '    strDEVSign = "+"
            '    If optEnableDenverSpecificOptions Then strDEVSign = "-"
            'End If

            'If optEnableDenverSpecificOptions Then
            '    If TdAmt < 0 Then
            '        dispColor = Color.Red
            '    ElseIf TdAmt >= 0 AndAlso TdAmt < 5 Then
            '        dispColor = Color.Green
            '    ElseIf TdAmt > 5 AndAlso TdAmt < 15 Then
            '        dispColor = Color.Yellow
            '    ElseIf TdAmt > 15 Then
            '        dispColor = Color.Red
            '    End If
            'Else
            '    If TdAmt > 5 AndAlso TdAmt < 10 Then
            '        dispColor = Color.Yellow
            '    ElseIf TdAmt > 9 Then
            '        dispColor = Color.PaleVioletRed
            '    End If
            'End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub GetTrainScheduleColorByDate(ScheduledDate As Date, ActualOSDate As Date, Optional ByRef ScheduleColor As Object = Nothing, Optional ByRef strDEVSign As String = "", Optional RefreshTrain As Boolean = False)

        Dim dispColor As Color = Color.LightSeaGreen
        Dim ToolTipText As String = "NONE"
        Dim DispText As String = "NONE"
        Dim StationName As String = ""


        Try
            Dim tmspan As TimeSpan
            tmspan = ActualOSDate.Subtract(ScheduledDate)
            Dim TdAmt As Double = tmspan.TotalMinutes

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

            ScheduleColor = dispColor

            If RefreshTrain Then

            End If


        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

#End Region

#Region " MRL SPECIFIC FUNCTIONS "

    Public Function IsOKToFinalTrainMRL(ByVal trn As TrainID, Optional ByVal LPoint As Object = Nothing) As Boolean
        Dim OkToFinal As Boolean = True
        Dim td As TrainDelay = Nothing
        Dim ErrorMessage As New ArrayList()
        Dim auth As Object = Nothing
        Dim MessageList As New ArrayList()
        Try
            If trn Is Nothing Then Return False
            MessageList = New ArrayList


            Dim TrainHasAuthority As Boolean = False
            If OtherTrackAuthorities IsNot Nothing AndAlso OtherTrackAuthorities.Count > 0 Then
                For Each auth In OtherTrackAuthorities
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE WARRANTS")
                            TrainHasAuthority = True
                            OkToFinal = False
                        End If
                    End If
                Next
            End If
            auth = Nothing
            If OkToFinal AndAlso Not TrainHasAuthority AndAlso gOtherSpecialAuthorityCollection IsNot Nothing AndAlso gOtherSpecialAuthorityCollection.Count > 0 Then
                For Each auth In gOtherSpecialAuthorityCollection
                    If Not auth Is Nothing Then
                        If auth.TrainSymbol = trn.Name Then
                            MessageList.Add(Chr(149) & Space(3) & "TRAIN HAS ONE OR MORE ACTIVE WARRANTS")
                            OkToFinal = False
                        End If
                    End If
                Next
            End If
            If OkToFinal Then ErrorMessage = New ArrayList

            If Not OkToFinal Then
                MessageList.Insert(0, "TRAIN CANNOT BE FINALED BECAUSE:")
                MessageList.Insert(1, "")
                MessageList.Add("")
                ShowCustomMessageBox(MessageList, "TRAIN FINAL CHECK", False, LPoint)
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
            Return False
        End Try
        Return OkToFinal
    End Function
#End Region

#Region " KCT SPECIFIC FUNCTIONS "
  
#End Region

#Region " VETMS FUNCTIONS "
    Private Delegate Sub DoProcessSFAAuthorityRelease(ByVal PTCTrain As TrainID)
    Private Delegate Sub DoProcessTNTAuthorityRelease(ByVal PTCTrain As TrainID)

    Public Function PerformPTCSFAIssue(ByRef SFA As TMDS.TrackAuthority.SFAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
        Dim VoidedSFA As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim VoidedSFA2 As TMDS.TrackAuthority.SFAuthority = Nothing
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim TempVOIDAuthCollection As New ArrayList
        Dim HasVoidedList As Boolean = False
        Dim AuthorityIssueSucess As Boolean = True
        Dim IsNewAuthority As Boolean = True
        Dim MySFA As TMDS.TrackAuthority.SFAuthority = SFA
        Try
            If Not String.IsNullOrEmpty(SFA.PTCStatus.Trim) Then
                If SFA.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
                    IsNewAuthority = False
                    AuthorityIssueSucess = True
                End If
            End If
            If IsNewAuthority AndAlso Not SendVoidOnly Then
                If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                    Dim AuthDesg As AuthorityDesignation = Nothing
                    Dim ActiveAuthDesgination As New ArrayList
                    Try
                        For Each AuthDesg In PTCTrainID.AuthDesignations
                            If AuthDesg.InActive Then
                                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                            End If
                        Next
                        For Each AuthDesg In ActiveAuthDesgination
                            PTCTrainID.AuthDesignations.Remove(AuthDesg)
                        Next
                        PTCTrainID.UpdateAuthorityDesignationToDB()
                        PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                        PTCTrainID.PendingLeadEngine = ""
                        PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                        PTCTrainID.myTrack.InvalidateTrainID()
                        PTCTrainID.myTrack.InitTrainDisplay()
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    Finally
                        ActiveAuthDesgination = Nothing
                        AuthDesg = Nothing
                    End Try
                End If

                'Me.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
                If Not gEnableIncludeVoidAuthorityIn1051 Then
                    If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                        For Each VoidedSFA In VoidList
                            VoidedSFA.PTCAuthorityACKPending = PTCAuthorityACKPendingStatus.RELEASE.ToString
                            VoidedSFA.PTCAuthorityComponents = VoidedSFA.Components.Clone
                            VoidedSFA.UpdateDatabaseWithPTCComponents()
                        Next
                    End If
                    VoidedSFA = Nothing
                    MySFA.VETMS_PerformIssuance(PTCTrainID)
                    Do
                        Select Case MySFA.PTCStatus.Trim.ToUpper
                            Case "AUTHORITY-ACTIVATED"
                                AuthorityIssueSucess = True
                                Exit Do
                            Case "AUTHORITY-FAILED"
                                AuthorityIssueSucess = False
                                Exit Do
                        End Select
                        Threading.Thread.Sleep(250)
                    Loop
                End If
            End If

            If Not AuthorityIssueSucess Then
                SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & SFA.AuthorityUID)
                'SFA.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                'SFA.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & SFA.AuthorityUID)
                Return False
            Else
                If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                    SFA.PTCAuthorityACKPending = "NONE"
                    SFA.UpdateDatabaseWithPTCComponents()
                End If
            End If


            If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                CopyArrayCollection(TempVOIDAuthCollection, VoidList)
                HasVoidedList = True
                For Each VoidedSFA In VoidList
                    If PTCTrainID IsNot Nothing Then
                        If VoidedSFA.IssuedToTrain AndAlso SFA.IssuedToTrain AndAlso VoidedSFA.IssueTo.Trim <> SFA.IssueTo.Trim AndAlso VoidedSFA.TrainSymbol.Trim = SFA.TrainSymbol.Trim AndAlso CheckIfUnitExistsInTrain(Mid(VoidedSFA.IssueTo, 1, 10), PTCTrainID) Then
                            SyncLock SFA
                                VoidedSFA.PTCMarkedToBeVoidedElectronically = True
                                Dim ProcessAuthSFARelease As New DoProcessSFAAuthorityRelease(AddressOf VoidedSFA.VETMS_PerformRelease)
                                ProcessAuthSFARelease.BeginInvoke(PTCTrainID, Nothing, Nothing)
                            End SyncLock
                            'If VoidedSFA.RemoveSFAVIIIAuthorityFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                            '    If VoidedSFA.RemoveAuthorityFromComponents() Then
                            '        VoidedSFA.PTCStatus = "RELEASE-SUCCESSFUL"
                            '        TMDS.GlobalDeclarations.OtherTrackAuthorities.Remove(VoidedSFA)
                            '        Dim SummItem2 As New SummaryItem("AUTHORITY", "REMOVE", VoidedSFA)
                            '        SyncLock gSummaryAuthQueue.SyncRoot
                            '            gSummaryAuthQueue.Enqueue(SummItem2)
                            '        End SyncLock
                            '        SystemEvent("AUTOMATIC RELEASE DUE TO FLIP CONSIST-TRAIN SYMBOL: " & PTCTrainID.Name & "_VOIDED AUTHUID:" & VoidedSFA.AuthorityUID & "_ISSUED TO:" & VoidedSFA.IssueTo & "_CANCELLED BY AUTHUID:" & SFA.AuthorityUID & "_ISSUED TO:" & SFA.IssueTo)
                            '        VoidedSFA.SetPTCAuthorityFlasherStatus()
                            '        VoidedSFA.FlasherMode(False, True)
                            '        VoidedSFA.PTCAuthorityACKPending = ""
                            '        VoidedSFA.PTCAuthorityComponents = New ArrayList
                            '        VoidedSFA.UpdateDatabaseWithPTCComponents()
                            '    End If
                            'End If
                        Else
                            SyncLock SFA
                                VoidedSFA.PTCMarkedToBeVoidedElectronically = True
                                'VoidedSFA.VETMS_PerformRelease(PTCTrainID)
                                Dim ProcessAuthSFARelease As New DoProcessSFAAuthorityRelease(AddressOf VoidedSFA.VETMS_PerformRelease)
                                ProcessAuthSFARelease.BeginInvoke(PTCTrainID, Nothing, Nothing)
                            End SyncLock
                        End If


                      
                    End If
                Next
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Public Function PerformPTCTNTIssue(ByRef TNT As TMDS.TrackAuthority.TrackAuthority, ByVal PTCTrainID As TrainID, ByVal VoidList As ArrayList, ByVal HasLeadEngineChange As Boolean, ByVal SendVoidOnly As Boolean) As Boolean
        Dim VoidedTNT As TMDS.TrackAuthority.TrackAuthority = Nothing
        Dim HasReleaseAuthorityFailed As Boolean = False
        Dim gHistoryMessage As String = ""
        Dim AuthorityIssueSucess As Boolean = True
        Dim IsNewAuthority As Boolean = True
        Try
            TNT.FlasherMode(False)
            If Not String.IsNullOrEmpty(TNT.PTCStatus.Trim) Then
                If TNT.PTCStatus.Trim.ToUpper = "AUTHORITY-ACTIVATED" Then
                    IsNewAuthority = False
                    AuthorityIssueSucess = True
                End If
            ElseIf SendVoidOnly Then
                AuthorityIssueSucess = True
            End If
            If IsNewAuthority AndAlso Not SendVoidOnly Then
                If HasLeadEngineChange AndAlso PTCTrainID IsNot Nothing Then
                    Dim AuthDesg As AuthorityDesignation = Nothing
                    Dim ActiveAuthDesgination As New ArrayList
                    Try
                        For Each AuthDesg In PTCTrainID.AuthDesignations
                            If AuthDesg.InActive Then
                                If Not ActiveAuthDesgination.Contains(AuthDesg) Then ActiveAuthDesgination.Add(AuthDesg)
                            End If
                        Next
                        For Each AuthDesg In ActiveAuthDesgination
                            PTCTrainID.AuthDesignations.Remove(AuthDesg)
                        Next
                        PTCTrainID.UpdateAuthorityDesignationToDB()
                        PTCTrainID.IdentifingUnit = PTCTrainID.PendingLeadEngine
                        PTCTrainID.PendingLeadEngine = ""
                        'myTrain.UpdateIdentifingUnit(True)
                        PTCTrainID.DoUpdateTrainIDIntoDatabase(PTCTrainID.myTrack, True)
                        PTCTrainID.myTrack.InvalidateTrainID()
                        PTCTrainID.myTrack.InitTrainDisplay()
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    Finally
                        ActiveAuthDesgination = Nothing
                        AuthDesg = Nothing
                    End Try
                End If
                TNT.VETMS_PerformIssuance(PTCTrainID, Nothing, Nothing)
                Do
                    Select Case TNT.PTCStatus.Trim.ToUpper
                        Case "AUTHORITY-ACTIVATED"
                            AuthorityIssueSucess = True
                            Exit Do
                        Case "AUTHORITY-FAILED"
                            AuthorityIssueSucess = False
                            Exit Do
                    End Select
                    Threading.Thread.Sleep(250)
                Loop
            End If

            If Not AuthorityIssueSucess Then
                SystemEvent("PTC REQUEST TO ISSUE AUTHORITY FAILED. ABORTING ISSUE:" & TNT.AuthorityUID)
                'TNT.AuthorityStatus = TNTAuthStatus.UNACKNOWLEDGED.ToString
                'TNT.UpdateTrackAuthorityStatus()
                SystemEvent("PTC REQUEST TO ISSUE FAILED. UPDATING AUTHORTY WITH UNACKNOWLEDGED STATUS:" & TNT.AuthorityUID)
                Return False
            Else
                TNT.PTCAuthorityACKPending = "NONE"
                TNT.UpdateDatabaseWithPTCComponents()
            End If
            ' 
            If VoidList IsNot Nothing AndAlso VoidList.Count > 0 Then
                If Not gEnableIncludeVoidAuthorityIn1051 Then
                    For Each VoidedTNT In VoidList
                        If PTCTrainID IsNot Nothing Then
                            If VoidedTNT.IssuedToTrain AndAlso TNT.IssuedToTrain AndAlso VoidedTNT.IssueTo.Trim <> TNT.IssueTo.Trim AndAlso VoidedTNT.TrainSymbol.Trim = TNT.TrainSymbol.Trim AndAlso CheckIfUnitExistsInTrain(Mid(VoidedTNT.IssueTo, 1, 10), PTCTrainID) Then
                                SyncLock VoidedTNT
                                    VoidedTNT.PTCMarkedToBeVoidedElectronically = True
                                    Dim ProcessAuthTNTRelease As New DoProcessTNTAuthorityRelease(AddressOf VoidedTNT.VETMS_PerformRelease)
                                    ProcessAuthTNTRelease.BeginInvoke(PTCTrainID, Nothing, Nothing)
                                End SyncLock
                                If VoidedTNT.RemoveActiveAuthorityFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                    If VoidedTNT.RemoveActiveAuthorityFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                        TMDS.GlobalDeclarations.OtherTrackAuthorities.Remove(VoidedTNT)
                                        Dim SummItem2 As New SummaryItem("AUTHORITY", "REMOVE", VoidedTNT)
                                        SyncLock gSummaryAuthQueue.SyncRoot
                                            gSummaryAuthQueue.Enqueue(SummItem2)
                                        End SyncLock

                                        VoidedTNT.PTCStatus = "RELEASE-SUCCESSFUL"
                                        VoidedTNT.SetPTCAuthorityFlasherStatus("ProcessMA_1053_TNT_ReleaseMessageResponse-3")
                                        VoidedTNT.FlasherMode(False)

                                        If VoidedTNT.UpdateDatabaseWithPTCAuthorityStatus(VoidedTNT.PTCStatus) Then

                                        End If
                                        SystemEvent("AUTOMATIC RELEASE DUE TO FLIP CONSIST-TRAIN SYMBOL: " & PTCTrainID.Name & "_VOIDED AUTHUID:" & VoidedTNT.AuthorityUID & "_ISSUED TO:" & VoidedTNT.IssueTo & "_CANCELLED BY AUTHUID:" & TNT.AuthorityUID & "_ISSUED TO:" & TNT.IssueTo)
                                    End If
                                End If
                            Else
                                SyncLock VoidedTNT
                                    VoidedTNT.PTCMarkedToBeVoidedElectronically = True
                                    Dim ProcessAuthTNTRelease As New DoProcessTNTAuthorityRelease(AddressOf VoidedTNT.VETMS_PerformRelease)
                                    ProcessAuthTNTRelease.BeginInvoke(PTCTrainID, Nothing, Nothing)
                                End SyncLock
                            End If

                            'SyncLock VoidedTNT
                            '    VoidedTNT.PTCMarkedToBeVoidedElectronically = True
                            '    Dim ProcessAuthTNTRelease As New DoProcessTNTAuthorityRelease(AddressOf VoidedTNT.VETMS_PerformRelease)
                            '    ProcessAuthTNTRelease.BeginInvoke(PTCTrainID, Nothing, Nothing)
                            'End SyncLock
                        End If
                    Next
                End If
            End If



        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Private Function CheckIfUnitExistsInTrain(IssueTo As String, trn As TrainID) As Boolean
        If trn Is Nothing Then Return False
        If IssueTo Is Nothing Then Return False
        If IssueTo.Trim = "" Then Return False
        Dim tu As TrainUnit = Nothing
        Try
            If trn.UnitsInTrain Is Nothing Then Return False
            If trn.UnitsInTrain.Count > 1 Then
            Else
                Return False
            End If
            For Each tu In trn.UnitsInTrain
                If tu.UnitNumber.Trim.ToUpper = IssueTo.Trim.ToUpper Then Return True
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

#End Region

#Region " GENERAL RAILROAD FUNCTIONS "

    Public Function IsVaildToIssueTATtoTrain(LeadEngine As String, LaunchPoint As Object, myForm As Object) As Boolean
        Dim trn As TrainID = Nothing

        Try
            trn = TMDS.SystemUtilities.FetchData.FindTrainFromIDUnit(LeadEngine)
            If trn IsNot Nothing Then
                Dim DisableIssuance As Boolean = False
                If trn.IsActivePTC Then
                    DisableIssuance = True
                ElseIf trn.CBTCEquipped Then
                    DisableIssuance = True
                End If
                If DisableIssuance Then
                    Dim msgList As New ArrayList
                    With msgList
                        .Add("AUTHORITY CANNOT BE ISSUED TO AN ACTIVE PTC TRAIN OR TO A PTC-EQUIPPED TRAIN")
                        .Add("REQUEST DENIED!!!")
                        .Add("")
                    End With
                    ShowCustomMessageBox(msgList, "PTC TRAIN VALIDATION", False, LaunchPoint, True, True, myForm)
                    Return False
                End If
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Public Function IsOkToReleasePTCTrainTGBO(LaunchPoint As Point, trn As TrainID) As Boolean
        Dim MessageList As New ArrayList
        Try
            If Not optCheckPTCStatusOnTGBORelease Then Return True
            If trn Is Nothing Then Return True
            If trn.IsActivePTC Then
                With MessageList
                    .Add("THIS TRAIN[ " & trn.Name & " ] IS PTC ACTIVE. ")
                    .Add("TRAIN HAS TO BE IN CUTOUT STATE TO ACTIVATE/RELEASE THIS TGBO")
                    .Add("REQUEST DENIED!")
                End With
                ShowCustomMessageBox(MessageList, "SYSTEM MESSAGE", False, LaunchPoint)
                Return False
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Public Function FindRoute(StartingTrack As Track, SearchDirection As String, StartingTracksCollection As ArrayList, AuthorityCompCollection As ArrayList, ByVal CompleteCompCollection As ArrayList) As ArrayList
        Dim TmpComplist As New ArrayList
        Dim TrackSelectedOsCompList As New ArrayList
        Dim NextComp As Object
        Dim FlipIt As Boolean
        Dim LastTrackComp As Object = Nothing
        Try
            If optApplyConfigurationForKCT Then
                TrackSelectedOsCompList.Add(StartingTrack)
                StartingTrack.ExpandOutForComponents(TrackSelectedOsCompList)
                'TrackSelected.ExtendOSComponents(TrackSelectedOsCompList)
            ElseIf optEnableARRCSpecificOptions Then
                TrackSelectedOsCompList.Add(StartingTrack)
                StartingTrack.AddOSComponents(TrackSelectedOsCompList)
                StartingTrack.ExtendOSComponents(TrackSelectedOsCompList)
                StartingTrack.ExpandOutForComponents(TrackSelectedOsCompList)
                IncludeLikeTracks(TrackSelectedOsCompList, CompleteCompCollection)
                FindLikeComponents(TrackSelectedOsCompList, TrackSelectedOsCompList)
                Dim FirstTrack As Track = Nothing
                Select Case SearchDirection.Trim.ToUpper
                    Case "RIGHT"
                        FirstTrack = GetFirstOrLastTrack(StartingTrack, "LEFT", TrackSelectedOsCompList)
                    Case Else
                        FirstTrack = GetFirstOrLastTrack(StartingTrack, "RIGHT", TrackSelectedOsCompList)
                End Select
                If FirstTrack IsNot Nothing Then
                    StartingTrack = FirstTrack
                    LastTrackComp = StartingTrack
                End If
            Else
                TrackSelectedOsCompList.Add(StartingTrack)
                StartingTrack.AddOSComponents(TrackSelectedOsCompList)
                StartingTrack.ExtendOSComponents(TrackSelectedOsCompList)
            End If


            Select Case SearchDirection.ToUpper.Trim
                Case "LEFT"
                    TmpComplist.Add(StartingTrack)
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(StartingTrack.LeftLink)
                    For i As Integer = 0 To 300
                        If NextComp Is Nothing Then Exit For
                        If Not TmpComplist.Contains(NextComp) Then


                            If Not FlipIt Then
                                Select Case NextComp.GetType.ToString
                                    Case gTrackComponentName
                                        LastTrackComp = NextComp
                                        'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                        If AuthorityCompCollection.Contains(NextComp) Then
                                            NextComp.Flipme = False
                                            TmpComplist.Add(NextComp)
                                        ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                        ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                        Else
                                            Exit For
                                        End If
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    Case gSwitchComponentName
                                        'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                        If AuthorityCompCollection.Contains(NextComp) Then
                                            TmpComplist.Add(NextComp)
                                        ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                        ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                        Else
                                            Exit For
                                        End If
                                        If NextComp.ControlledReverse Then
                                            If NextComp.BrotherSwitches IsNot Nothing AndAlso NextComp.BrotherSwitches.Count > 0 AndAlso TmpComplist.Contains(NextComp.BrotherSwitches(0)) Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            ElseIf NextComp.LeftAltLink <> 0 Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                            ElseIf NextComp.LeftAltLink = 0 AndAlso NextComp.RightAltLink <> 0 Then
                                                Dim tmpComp As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                                If tmpComp IsNot Nothing AndAlso TmpComplist.Contains(tmpComp) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                                ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                                End If
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                            End If
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        End If
                                    Case gSignalComponentName
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                    Case gFlipFlopComponentName
                                        Dim FF As FlipFlop = NextComp
                                        NextComp = FF.FindExitComponent(LastTrackComp.guid)
                                        'NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        If Not FlipIt Then FlipIt = True Else FlipIt = False
                                    Case gRRXINGComponentName
                                        Dim RL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        Dim LL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        Dim RAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                        Dim LAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                        If CompleteCompCollection.Contains(RAL) AndAlso CompleteCompCollection.Contains(LAL) Then
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        End If

                                End Select
                            Else
                                TmpComplist.Add(StartingTrack)
                                Select Case NextComp.GetType.ToString
                                    Case gTrackComponentName
                                        LastTrackComp = NextComp
                                        'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                        If AuthorityCompCollection.Contains(NextComp) Then
                                            NextComp.Flipme = True
                                            TmpComplist.Add(NextComp)
                                        ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                        ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                        Else
                                            Exit For
                                        End If
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    Case gSwitchComponentName
                                        'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                        If AuthorityCompCollection.Contains(NextComp) Then
                                            TmpComplist.Add(NextComp)
                                        ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                        ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                        Else
                                            Exit For
                                        End If
                                        If NextComp.ControlledReverse Then
                                            If NextComp.BrotherSwitches IsNot Nothing AndAlso NextComp.BrotherSwitches.Count > 0 AndAlso TmpComplist.Contains(NextComp.BrotherSwitches(0)) Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            ElseIf NextComp.RightAltLink <> 0 Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                            ElseIf NextComp.RightAltLink = 0 AndAlso NextComp.LeftAltLink <> 0 Then
                                                Dim tmpComp As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                                If tmpComp IsNot Nothing AndAlso TmpComplist.Contains(tmpComp) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                                ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                                End If
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                            End If
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        End If
                                    Case gSignalComponentName
                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                    Case gFlipFlopComponentName
                                        Dim FF As FlipFlop = NextComp
                                        NextComp = FF.FindExitComponent(LastTrackComp.GUID)
                                        'NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        If Not FlipIt Then FlipIt = True Else FlipIt = False
                                    Case gRRXINGComponentName
                                        Dim RL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        Dim LL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        Dim RAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                        Dim LAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                        If CompleteCompCollection.Contains(RAL) AndAlso CompleteCompCollection.Contains(LAL) Then
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                        Else
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        End If
                                    Case Else
                                End Select


                            End If
                        Else
                            Exit For
                        End If
                    Next


                Case Else
                    TmpComplist.Add(StartingTrack)
                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(StartingTrack.RightLink)
                    For i As Integer = 0 To 300
                        If NextComp Is Nothing Then Exit For
                        If Not TmpComplist.Contains(NextComp) Then
                            If Not TmpComplist.Contains(NextComp) Then
                                If Not FlipIt Then
                                    Select Case NextComp.GetType.ToString
                                        Case gTrackComponentName
                                            LastTrackComp = NextComp
                                            If AuthorityCompCollection.Contains(NextComp) Then
                                                NextComp.Flipme = False
                                                TmpComplist.Add(NextComp)
                                            ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                            ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                            Else
                                                Exit For
                                            End If
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        Case gSwitchComponentName
                                            'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                            If AuthorityCompCollection.Contains(NextComp) Then
                                                TmpComplist.Add(NextComp)
                                            ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                            ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                            Else
                                                Exit For
                                            End If
                                            If NextComp.ControlledReverse Then
                                                If NextComp.BrotherSwitches IsNot Nothing AndAlso NextComp.BrotherSwitches.Count > 0 AndAlso TmpComplist.Contains(NextComp.BrotherSwitches(0)) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                                ElseIf NextComp.RightAltLink <> 0 Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                                ElseIf NextComp.RightAltLink = 0 AndAlso NextComp.LeftAltLink <> 0 Then
                                                    Dim tmpComp As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                                    If tmpComp IsNot Nothing AndAlso TmpComplist.Contains(tmpComp) Then
                                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                                    ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                                    End If
                                                Else
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                                End If
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            End If
                                        Case gSignalComponentName
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                        Case gFlipFlopComponentName
                                            Dim FF As FlipFlop = NextComp
                                            NextComp = FF.FindExitComponent(LastTrackComp.guid)
                                            'NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            If Not FlipIt Then FlipIt = True Else FlipIt = False
                                        Case gRRXINGComponentName
                                            Dim RL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            Dim LL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            Dim RAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                            Dim LAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                            If CompleteCompCollection.Contains(RAL) AndAlso CompleteCompCollection.Contains(LAL) Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            End If
                                        Case Else
                                    End Select
                                Else
                                    Select Case NextComp.GetType.ToString
                                        Case gTrackComponentName
                                            LastTrackComp = NextComp
                                            'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                            If AuthorityCompCollection.Contains(NextComp) Then
                                                NextComp.Flipme = True
                                                TmpComplist.Add(NextComp)
                                            ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                            ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                            Else
                                                Exit For
                                            End If
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        Case gSwitchComponentName
                                            'If AuthorityCompCollection.Contains(NextComp) Then TmpComplist.Add(NextComp) Else Exit For
                                            If AuthorityCompCollection.Contains(NextComp) Then
                                                TmpComplist.Add(NextComp)
                                            ElseIf StartingTrack.OsComponentCollection IsNot Nothing AndAlso StartingTrack.OsComponentCollection.Count > 0 AndAlso StartingTrack.OsComponentCollection.Contains(NextComp) Then
                                            ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                            Else
                                                Exit For
                                            End If
                                            If NextComp.ControlledReverse Then
                                                If NextComp.BrotherSwitches IsNot Nothing AndAlso NextComp.BrotherSwitches.Count > 0 AndAlso TmpComplist.Contains(NextComp.BrotherSwitches(0)) Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                                ElseIf NextComp.LeftAltLink <> 0 Then
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                                ElseIf NextComp.LeftAltLink = 0 AndAlso NextComp.RightAltLink <> 0 Then
                                                    Dim tmpComp As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                                    If tmpComp IsNot Nothing AndAlso TmpComplist.Contains(tmpComp) Then
                                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                                    ElseIf TrackSelectedOsCompList IsNot Nothing AndAlso TrackSelectedOsCompList.Count > 0 AndAlso TrackSelectedOsCompList.Contains(NextComp) Then
                                                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                                    End If
                                                Else
                                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)

                                                End If
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            End If
                                        Case gSignalComponentName
                                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                        Case gFlipFlopComponentName
                                            'NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            Dim FF As FlipFlop = NextComp
                                            NextComp = FF.FindExitComponent(LastTrackComp.GUID)
                                            If Not FlipIt Then FlipIt = True Else FlipIt = False
                                        Case gRRXINGComponentName
                                            Dim RL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                                            Dim LL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            Dim RAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightAltLink)
                                            Dim LAL As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                            If CompleteCompCollection.Contains(RAL) AndAlso CompleteCompCollection.Contains(LAL) Then
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftAltLink)
                                            Else
                                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                                            End If
                                        Case Else
                                    End Select
                                End If
                            End If
                        Else
                            Exit For
                        End If
                    Next



            End Select

            For Each NextComp In TmpComplist
                If NextComp.GetType.ToString = gTrackComponentName Then SystemEvent(NextComp.GUID & ":" & NextComp.LeftLimitMilePost & "  " & NextComp.AUTHORITYTYPE)
            Next

            Return TmpComplist
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Private Sub IncludeLikeTracks(ByRef RollUpList As ArrayList, ByVal CompleteCompCollection As ArrayList)
        Try
            Dim comp As Object
            Dim CompInList As Object
            Dim RollUpListTemp As ArrayList = RollUpList.Clone
            For Each comp In CompleteCompCollection
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    For Each CompInList In RollUpListTemp
                        If CompInList.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            If CompInList.LeftLimitName = comp.LeftLimitName And CompInList.RightLimitName = comp.RightLimitName And CompInList.TrackName <> comp.TrackName Then
                                If Not RollUpList.Contains(comp) Then
                                    RollUpList.Insert(RollUpList.IndexOf(CompInList), comp)
                                    Dim swLeft As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(comp.LeftLink)
                                    Dim swRight As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(comp.RightLink)
                                    If Not IsNothing(swLeft) Then
                                        If Not RollUpList.Contains(swLeft) Then RollUpList.Insert(RollUpList.IndexOf(CompInList), swLeft)
                                    End If
                                    If Not IsNothing(swRight) Then
                                        If Not RollUpList.Contains(swRight) Then RollUpList.Insert(RollUpList.IndexOf(CompInList), swRight)
                                    End If
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

    Private Function FindLikeComponents(ByRef TrackSelection As ArrayList, ByVal OSCompList As ArrayList) As Boolean
        Dim myTrackOSCollection As New ArrayList
        Try
            Dim comp, CompToCompare As Object
            Dim CompList As New ArrayList()
            Dim AddComp As Boolean
            CopyArrayCollection(myTrackOSCollection, OSCompList)
            For Each comp In OSCompList 'Me.EntireTrackSelection 
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If comp.OStype And Not comp.RouteBlockingEnabled Then
                        AddComp = True
                        For Each CompToCompare In CompList
                            'If Not CompList.Contains(comp) Then
                            If comp.StationNumber = CompToCompare.StationNumber _
                            And comp.TrackName = CompToCompare.TrackName _
                            And comp.Name = CompToCompare.Name Then
                                AddComp = False
                                Exit For
                            End If
                            'Else
                            'AddComp = False
                            'Exit For
                            'End If
                        Next
                        If AddComp Then
                            If Not CompList.Contains(comp) Then
                                CompList.Add(comp)
                            End If
                        End If
                    End If
                End If
            Next

            Dim cpList As New ArrayList()
            For Each comp In CompList
                Dim cp As ControlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(comp.StationNumber)
                If Not cp Is Nothing Then
                    If Not cpList.Contains(cp) Then
                        cpList.Add(cp)
                    End If
                End If
            Next

            For Each comp In CompList
                Dim cpTocheck As ControlPoint
                For Each cpTocheck In cpList
                    Dim compTocheck As Object
                    For Each compTocheck In cpTocheck.AssignedComponenets
                        If compTocheck.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            If compTocheck.OStype And Not compTocheck.RouteBlockingEnabled Then
                                If Not myTrackOSCollection.Contains(compTocheck) Then
                                    If (compTocheck.TrackName = comp.TrackName And compTocheck.TrackName <> "") _
                                    And (compTocheck.StationNumber = comp.StationNumber) _
                                    And (compTocheck.AllowTrackTime Or compTocheck.AllowTrackPermit) And (compTocheck.OStype And comp.OStype) And _
                                        (compTocheck.RightLimitName = comp.RightLimitName And compTocheck.LeftLimitName = comp.LeftLimitName) Then
                                        If Not OSCompList.Contains(compTocheck) Then
                                            If Not TrackSelection.Contains(compTocheck) Then TrackSelection.Insert(TrackSelection.IndexOf(comp), compTocheck)
                                        End If

                                        Dim swRight As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(compTocheck.RightLink)
                                        Dim swLeft As Switch = TMDS.SystemUtilities.FetchData.FindSwitchFromGUID(compTocheck.LeftLink)
                                        If Not swRight Is Nothing Then
                                            If Not TrackSelection.Contains(swRight) Then
                                                TrackSelection.Add(swRight)
                                            End If
                                        End If
                                        If Not swLeft Is Nothing Then
                                            If Not TrackSelection.Contains(swLeft) Then
                                                TrackSelection.Add(swLeft)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                Next
            Next


            For Each comp In CompList
                If Not TrackSelection.Contains(comp) Then
                    TrackSelection.Add(comp)
                End If
            Next

            CompList.Clear()
            CompList = Nothing
            cpList.Clear()
            cpList = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Public Function GetDelayCodeByCode(DelayCode As String) As DelayCode
        Dim DC As DelayCode = Nothing
        Try
            For Each DC In TMDS.GlobalDeclarations.gDelayCodeList
                If DC.Code.Trim.ToUpper = DelayCode.Trim.ToUpper Then
                    Return DC
                End If
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Public Function GetPassengerStation(IsEntry As Boolean, TrackToCheck As Track, Direction As String) As PassengerStation
        Dim PS As PassengerStation = Nothing
        Dim TrackGUID As Integer = 0
        Try
            For Each PS In gPassengerStationCollection
                If PS IsNot Nothing Then
                    If IsEntry Then
                        For Each TrackGUID In PS.ReportingEntryTracksForLeftBoundMovements
                            If TrackGUID = TrackToCheck.GUID Then
                                Return PS
                            End If
                        Next
                        For Each TrackGUID In PS.ReportingEntryTracksForRightBoundMovements
                            If TrackGUID = TrackToCheck.GUID Then
                                Return PS
                            End If
                        Next
                    Else
                        For Each TrackGUID In PS.ReportingExitTracksForLeftBoundMovements
                            If TrackGUID = TrackToCheck.GUID Then
                                Return PS
                            End If
                        Next
                        For Each TrackGUID In PS.ReportingExitTracksForRightBoundMovements
                            If TrackGUID = TrackToCheck.GUID Then
                                Return PS
                            End If
                        Next
                    End If
                End If
            Next
        Catch ex As Exception

        End Try
        Return Nothing
    End Function

    Dim DeactivatingTrainInProgress As Boolean = False
    Public Sub DeActivateTrainPTCStatus()
        Dim trn As TrainID = Nothing
        Try
            If DeactivatingTrainInProgress Then Exit Sub
            DeactivatingTrainInProgress = True
            For Each trn In TMDS.GlobalDeclarations.TrainCollection
                Dim ProcessTrain As Boolean = True
                If optEnableShadowMode AndAlso trn IsNot Nothing AndAlso trn.myTrack IsNot Nothing Then
                    If trn.myTrack.myControlPoint.IsShadowMode Then ProcessTrain = False
                End If

                If ProcessTrain AndAlso trn IsNot Nothing AndAlso trn.IsActivePTC Then
                    trn.IsActivePTC = False
                    trn.RefreshTrainIDDisplay()
                    Dim Loco As MobileTracking.Locomotive = FindLocomotive(trn.IdentifingUnit, trn.Name)
                    If Loco IsNot Nothing Then
                        With Loco.myTrainID
                            If Loco.LocomotiveID = .IdentifingUnit Then
                                .TrainIDColor = Loco.LocomotiveIDColor
                            End If
                            Loco.OutOfComm = True

                            If Loco.PTCCapable And Loco.OutOfComm Then
                                .ShowFailure = True
                            Else
                                .ShowFailure = False
                            End If
                            .Speed = 0

                        End With
                        Loco.LocoStatusID = "UNKNOWN"
                        Dim SummItem2 As New SummaryItem("TRAIN", "UPDATE", Loco.myTrainID)
                        SyncLock TMDS.GlobalDeclarations.gSummaryTrainQueue
                            TMDS.GlobalDeclarations.gSummaryTrainQueue.Enqueue(SummItem2)
                        End SyncLock

                        Dim SummItem As New SummaryItem("LOCOMOTIVE", "LOCO-STATUS", Loco)
                        SyncLock gLocomotiveSummaryQueue.SyncRoot
                            gLocomotiveSummaryQueue.Enqueue(SummItem)
                        End SyncLock
                    End If
                End If


            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            DeactivatingTrainInProgress = False
        End Try
    End Sub

    Private Function FindLocomotive(ByVal LocoUID As String, ByVal TrainSymbol As String) As MobileTracking.Locomotive
        Dim LocFound As MobileTracking.Locomotive = Nothing
        Try
            Dim loco As MobileTracking.Locomotive = Nothing
            Dim LocExists As Boolean

            If Not IsNothing(TMDS.GlobalDeclarations.gLocomotiveCollection) Then
                If TMDS.GlobalDeclarations.gLocomotiveCollection.Count > 0 Then
                    For Each loco In TMDS.GlobalDeclarations.gLocomotiveCollection
                        If (loco.LocomotiveID = LocoUID) And (loco.Symbol.Trim = TrainSymbol.Trim) Then
                            LocFound = loco
                            LocExists = True
                            Exit For
                        End If
                    Next
                End If
            End If


            Return LocFound

        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try

    End Function

    Public Function TypeOfCompString(CompToCheck As Object) As String
        Try
            If CompToCheck Is Nothing Then Return String.Empty
            Select Case CompToCheck.GetType.ToString
                Case gTrackComponentName
                    Return "Track"
                Case gGradeCrossingComponentName
                    Return "Crossing"
                Case gSignalComponentName
                    Return "Signal"
                Case gSwitchComponentName
                    Return "Switch"
                Case gDeviceComponentName
                    Return "Device"
                Case gFlipFlopComponentName
                    Return "FlipFlop"
                Case gControlPointComponentName
                    Return "ControlPoint"
                Case gRRXINGComponentName
                    Return "RRXING"
                Case gSlideZoneComponentName
                    Return "SlideZone"
                Case gDefectDetectorComponentName
                    Return "FED"
            End Select
        Catch ex As Exception

        End Try
        Return String.Empty
    End Function

    Public Function GetEventTime(ByVal Comp As Object, Optional ByVal NotCurrentDrawing As Boolean = False) As String
        Dim ScriptExcuteTime As String = ""
        ''Dim frm As System.Windows.Forms.Form
        'Try

        '    'frm = Comp.CurrentDrawingForm
        '    If gSimulationRecordMode And gIsAfterStartupScript Then
        '        Dim trnPlan As New frmTimeSpanConverter ' frmScriptEventTime
        '        If (NotCurrentDrawing) Then
        '            trnPlan.Owner = Comp
        '            Comp.SendToBack()
        '        Else
        '            trnPlan.Owner = Comp.CurrentDrawingForm
        '        End If
        '        'trnPlan.Location = New Point(frmMainMDI.Left + frm.Left + frm.Width / 2 - trnPlan.Width / 2, 500)
        '        trnPlan.IsScriptPlan = True
        '        trnPlan.BringToFront()
        '        trnPlan.ShowDialog()
        '        ScriptExcuteTime = trnPlan.ScriptExcuteTime
        '        trnPlan.Dispose()
        '        trnPlan = Nothing
        '    Else 'Bob Varghese 12/11 When loading the script the time for load should be saved with the event
        '        ScriptExcuteTime = EvtTime '-----------------------------------------------------------------------
        '    End If

        'Catch ex As Exception
        '    MsgBox(ex.ToString)
        'Finally
        '    'frm = Nothing
        'End Try

        Return ScriptExcuteTime
    End Function

    Public Sub GenerateScripts(objAuth As Object, Optional Action As SimulatorScriptAction = SimulatorScriptAction.ISSUED)
        Dim clsScript As New TMDS.clsScriptGenrator
        Try
            If Not gEnableXMLRecording Then Exit Sub
            clsScript.CreateXMLString(objAuth, Action)
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Public Sub ExtendStoreRoute(ByVal Direction As String, ByVal ExitTrack As Track, ByRef SelectionList As ArrayList, Optional ByRef EntryTrack As Track = Nothing)
        Dim Flipme As Boolean = False

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
                Select Case CompCheck.GetType.ToString
                    Case gTrackComponentName, gSwitchComponentName, gSignalComponentName
                        CompCheck.FlipMe = Flipme
                    Case gFlipFlopComponentName
                        If Flipme Then
                            Flipme = False
                        Else
                            Flipme = True
                        End If
                End Select
            Next

            If Direction = "LEFT" Then
                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
            Else
                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
            End If


            Do
                Dim tmpFlipDirection As String = ""
                Dim tmpNonFlipDirection As String = ""
                If Direction = "LEFT" Then
                    tmpFlipDirection = "RIGHT"
                    tmpNonFlipDirection = "LEFT"
                Else
                    tmpFlipDirection = "LEFT"
                    tmpNonFlipDirection = "RIGHT"
                End If


                If NextComp Is Nothing Then Exit Sub
                Select Case NextComp.GetType.ToString
                    Case "TMDS.TrackComponents.Track"
                        Dim trk As Track = CType(NextComp, Track)
                        If Mid(trk.AuthorityType, 1, 3) <> "CTC" Then
                            Exit Sub
                        End If
                        If trk.FlipMe Then
                            Direction = tmpFlipDirection
                        Else
                            Direction = tmpNonFlipDirection
                        End If

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
                        If NextComp.FlipMe Then
                            Direction = tmpFlipDirection
                        Else
                            Direction = tmpNonFlipDirection
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


                        Dim sigComp As Signal = CType(NextComp, Signal)
                        If sigComp.FlipMe Then
                            Direction = tmpFlipDirection
                        Else
                            Direction = tmpNonFlipDirection
                        End If
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

                        If Not SelectionList.Contains(sigComp) Then
                            SelectionList.Add(sigComp)
                            If NextComp.GetType.ToString = gTrackComponentName Then
                                If Mid(NextComp.AuthorityType, 1, 3) <> "CTC" Then
                                    Exit Sub
                                End If
                            End If
                        End If

                    Case "TMDS.TrackOtherComponents.FlipFlop"
                        'If Not SelectionList.Contains(NextComp) Then
                        '    SelectionList.Add(NextComp)
                        'End If
                        Dim thisFlip As FlipFlop = CType(NextComp, FlipFlop)
                        'If Direction = "LEFT" Then
                        '    Direction = "RIGHT"
                        'Else
                        '    Direction = "LEFT"
                        'End If
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(thisFlip.FindExitGUID(LastGUID))
                    Case "TMDS.TrackOtherComponents.RRXING"

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
                    'Exit Sub
                    Exit Do
                End If
            Loop

            For Each CompCheck In SelectionList
                Select Case CompCheck.GetType.ToString
                    Case gTrackComponentName, gSwitchComponentName, gSignalComponentName
                        CompCheck.FlipMe = False
                End Select
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Public Function GetPTCTrainStatusColor(TrainPTCStaus As String) As Color
        Try
            '     0 - not used 
            '1 - self test - Red *
            '2 - initializing - Yellow *
            '3 - disengaged - Orange *
            '4 - cut out - Red *
            '5 - active - Lime *
            '6 - switching - Lime *
            '7 - failed - Red *
            If TrainPTCStaus Is Nothing Then Return Color.White
            If TrainPTCStaus.Trim = "" Then Return Color.White
            Select Case TrainPTCStaus.Trim.ToUpper
                Case "ACTIVE", "SWITCHING"
                    Return Color.Lime
                Case "CUTOUT", "SELFTEST"
                    Return Color.Red
                Case "DISENGAGED", "FAILED"
                    Return Color.Orange
                Case "INITIALIZING"
                    Return Color.Yellow
                Case Else
                    Return Color.White
            End Select



        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Color.White
    End Function

    Public Function ArchivePDF(PDFFileToArchive As String, DocName As String, EntityUID As String, TerrUID As Integer, SubUID As Integer) As Boolean
        Try
            If PDFFileToArchive Is Nothing Then Return False
            If PDFFileToArchive.Trim = "" Then Return False

            If DocName Is Nothing Then Return False
            If DocName.Trim = "" Then Return False

            If EntityUID Is Nothing Then Return False
            If EntityUID.Trim = "" Then Return False

            If optPerformDocumentArchiving Then
                TMDS.PrintForms.DocumentArchive.ArchiveTheDocument(PDFFileToArchive, DocName, EntityUID, TerrUID, SubUID)
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

#End Region

#Region " BRC "

    Public Sub ClearTempTraffic(ByVal Sig As Signal)
        Dim tmpSigRouteCollection As New ArrayList
        Dim Comp As Object = Nothing
        Try
            If Sig Is Nothing Then Exit Sub
            If Sig.SignalRouteCollection IsNot Nothing AndAlso Sig.SignalRouteCollection.Count > 0 Then
                CopyArrayCollection(tmpSigRouteCollection, Sig.SignalRouteCollection)
            Else
                tmpSigRouteCollection = TMDS.SystemUtilities.FetchData.FindMySignalRoute(Sig)
            End If


            If tmpSigRouteCollection IsNot Nothing AndAlso tmpSigRouteCollection.Count > 0 Then
                For Each Comp In tmpSigRouteCollection
                    If Comp.GetType.ToString = gTrackComponentName Then
                        Comp.mytmpTrafficDirection = ""
                    End If
                Next
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub
#End Region

#Region " DB FUNCTIONS "

    Public Sub ProcessBullFlagDataRead(dr As OleDbDataReader, ByRef sb As SubBulletin, CallingMethod As String)
        Try
            If Not dr.IsClosed Then
                '********************************************************************************
                Dim Flag1BeginMP As Double = 0
                Dim Flag1EndMP As Double = 0
                Dim Flag2BeginMP As Double = 0
                Dim Flag2EndMP As Double = 0
                Dim Flag3BeginMP As Double = 0
                Dim Flag3EndMP As Double = 0

                Dim Flag1BeginSubUID As Integer = 0
                Dim Flag1EndSubUID As Integer = 0
                Dim Flag2BeginSubUID As Integer = 0
                Dim Flag2EndSubUID As Integer = 0
                Dim Flag3BeginSubUID As Integer = 0
                Dim Flag3EndSubUID As Integer = 0


                If Not dr.IsDBNull(35) Then Integer.TryParse(dr.GetValue(35), Flag1BeginSubUID)
                If Not dr.IsDBNull(36) Then Double.TryParse(dr.GetValue(36), Flag1BeginMP)
                sb.FlagMilePostBegin1SubUID = Flag1BeginSubUID
                sb.FlagMilePostBegin1 = Flag1BeginMP
                If Not dr.IsDBNull(37) Then sb.FlagMilePostBeginSuffix1 = Trim(dr.GetString(37))

                If Not dr.IsDBNull(38) Then Integer.TryParse(dr.GetValue(38), Flag1EndSubUID)
                If Not dr.IsDBNull(39) Then Double.TryParse(dr.GetValue(39), Flag1EndMP)
                sb.FlagMilePostEnd1SubUID = Flag1EndSubUID
                sb.FlagMilePostEnd1 = Flag1EndMP
                If Not dr.IsDBNull(40) Then sb.FlagMilePostEndSuffix1 = Trim(dr.GetString(40))




                If Not dr.IsDBNull(41) Then Integer.TryParse(dr.GetValue(41), Flag2BeginSubUID)
                If Not dr.IsDBNull(42) Then Double.TryParse(dr.GetValue(42), Flag2BeginMP)
                sb.FlagMilePostBegin2SubUID = Flag2BeginSubUID
                sb.FlagMilePostBegin2 = Flag2BeginMP
                If Not dr.IsDBNull(43) Then sb.FlagMilePostBeginSuffix2 = Trim(dr.GetString(43))

                If Not dr.IsDBNull(44) Then Integer.TryParse(dr.GetValue(44), Flag2EndSubUID)
                If Not dr.IsDBNull(45) Then Double.TryParse(dr.GetValue(45), Flag2EndMP)
                sb.FlagMilePostEnd2SubUID = Flag2EndSubUID
                sb.FlagMilePostEnd2 = Flag2EndMP
                If Not dr.IsDBNull(46) Then sb.FlagMilePostEndSuffix2 = Trim(dr.GetString(46))


                If Not dr.IsDBNull(47) Then Integer.TryParse(dr.GetValue(47), Flag3BeginSubUID)
                If Not dr.IsDBNull(48) Then Double.TryParse(dr.GetValue(48), Flag3BeginMP)
                sb.FlagMilePostBegin3SubUID = Flag3BeginSubUID
                sb.FlagMilePostBegin3 = Flag3BeginMP
                If Not dr.IsDBNull(49) Then sb.FlagMilePostBeginSuffix3 = Trim(dr.GetString(49))

                If Not dr.IsDBNull(50) Then Integer.TryParse(dr.GetValue(50), Flag3BeginSubUID)
                If Not dr.IsDBNull(51) Then Double.TryParse(dr.GetValue(51), Flag3EndMP)
                sb.FlagMilePostEnd3SubUID = Flag3BeginSubUID
                sb.FlagMilePostEnd3 = Flag3EndMP
                If Not dr.IsDBNull(52) Then sb.FlagMilePostEndSuffix3 = Trim(dr.GetString(52))

                '********************************************************************************
            End If
        Catch ex As Exception
            GetErrorDetails(ex, "ProcessBullFlagDataRead --> " & CallingMethod)
        End Try

    End Sub

#End Region

#Region " TGBO "

    Public Function GenerateTGBOPDF(tgb As TabularGeneralBulletin) As String
        Dim TGBODocument As New PrintForms.GeneralBulletinOrders
        Dim TGBOFile As String = ""
        Try
            Select Case optRailRoadName.ToUpper.Trim
                Case "ARRC"
                    TGBOFile = TGBODocument.PrintTGBOForARRC(tgb, False, True)
                Case "DENVER"
                    TGBOFile = TGBODocument.PrintTGBOForDenver(tgb)
                Case "METROLINK", "NCTD"
                    TGBOFile = TGBODocument.PrintTGBOForML(tgb)
                    'Case "CN-TEST"
                Case "CN"  'CN-1188
                    TGBOFile = TGBODocument.PrintTGBO(tgb)
                Case Else
                    TGBOFile = TGBODocument.PrintTGBOForML(tgb)
            End Select
            Return TGBOFile
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return String.Empty
    End Function

#End Region

End Module
