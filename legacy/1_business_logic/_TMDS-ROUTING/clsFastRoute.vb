Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrainComponents

Public Class clsFastRoute

    Private HasSameDirectionExitSignal As Boolean = False
#Region " FAST Route Collection "

    Public Function SearchForRoute(ByRef enterTrack As Track, ByRef exitTrack As Track) As Boolean
        Try
            Dim CurrentCompSelection As ArrayList
            Dim SearchRightFirst As Boolean
            Dim SearchLeftFirst As Boolean
            If enterTrack.CurrentDrawingForm Is exitTrack.CurrentDrawingForm Then

                If enterTrack.LX <= exitTrack.LX And enterTrack.LY >= enterTrack.LY - 50 Then
                    SearchRightFirst = True
                ElseIf enterTrack.LX <= exitTrack.LX And enterTrack.LY <= exitTrack.LY Then
                    SearchRightFirst = True
                ElseIf (enterTrack.LX <= exitTrack.LX) AndAlso (enterTrack.LY = exitTrack.LY) Then
                    SearchRightFirst = True
                ElseIf enterTrack.LX > exitTrack.LX And exitTrack.LY > enterTrack.LY + 50 Then
                    SearchRightFirst = True
                ElseIf enterTrack.LX > exitTrack.LX AndAlso exitTrack.LY = enterTrack.LY Then
                    SearchRightFirst = True
                ElseIf enterTrack.LX < exitTrack.LX And exitTrack.LY = enterTrack.LY Then
                    SearchRightFirst = True
                ElseIf enterTrack.LX > exitTrack.LX And exitTrack.LY = enterTrack.LY Then
                    SearchLeftFirst = True
                End If
            Else
                If CInt(enterTrack.CurrentDrawingForm.Tag) < CInt(exitTrack.CurrentDrawingForm.Tag) Then
                    gSearchDirection = "RIGHT"
                Else
                    gSearchDirection = "LEFT"
                End If
            End If

            If gSearchDirection = "" Then
                If SearchRightFirst Then
                    CurrentCompSelection = SearchRightForRouteNew(enterTrack, exitTrack)
                    If CurrentCompSelection Is Nothing Then
                        CurrentCompSelection = SearchLeftForRouteNew(enterTrack, exitTrack)

                        gSearchDirection = "LEFT"
                    Else
                        If gSearchDirection = "" Then
                            gSearchDirection = "RIGHT"
                        End If
                    End If
                Else
                    Dim LinkDirectionReversed As Boolean
                    If enterTrack.myTerritory.LinkDirectionReversed Then
                        LinkDirectionReversed = True
                    End If
                    If LinkDirectionReversed Then
                        If SearchLeftFirst Then
                            CurrentCompSelection = SearchLeftForRouteNew(enterTrack, exitTrack)
                            If CurrentCompSelection Is Nothing Then
                                CurrentCompSelection = SearchRightForRouteNew(enterTrack, exitTrack)
                                gSearchDirection = "RIGHT"
                            Else
                                gSearchDirection = "LEFT"
                            End If

                        Else
                            CurrentCompSelection = SearchRightForRouteNew(enterTrack, exitTrack)
                            If CurrentCompSelection Is Nothing Then
                                CurrentCompSelection = SearchLeftForRouteNew(enterTrack, exitTrack)
                                gSearchDirection = "LEFT"
                            Else
                                gSearchDirection = "RIGHT"
                            End If

                        End If
                    Else
                        CurrentCompSelection = SearchLeftForRouteNew(enterTrack, exitTrack)
                        If CurrentCompSelection Is Nothing Then
                            CurrentCompSelection = SearchRightForRouteNew(enterTrack, exitTrack)
                            gSearchDirection = "RIGHT"
                        Else
                            gSearchDirection = "LEFT"
                        End If
                    End If
                End If
            ElseIf gSearchDirection = "LEFT" Then
                CurrentCompSelection = SearchLeftForRouteNew(enterTrack, exitTrack)

            Else
                CurrentCompSelection = SearchRightForRouteNew(enterTrack, exitTrack)
            End If
            If CurrentCompSelection Is Nothing Then

                If Not IsNothing(enterTrack) Then
                    Me.SystemEvent("SearchForRoute-EntryTrack:" & enterTrack.GUID)
                End If
                If Not IsNothing(exitTrack) Then
                    Me.SystemEvent("SearchForRoute-ExitTrack:" & exitTrack.GUID)
                End If

                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                ClearSystemVariables()
                Exit Function
            ElseIf CurrentCompSelection IsNot Nothing AndAlso optEnableARRCSpecificOptions Then
                If optEnableARRCSpecificOptions Then TMDS.SystemForms.MySystemForms.frmMainMDI.RemoveTWCComponentsFromENTEXitSelection(CurrentCompSelection, enterTrack)
            End If


            If optEnableLicenseGenerationForPTC AndAlso Not IsNothing(gCreateLicenseTrain) Then
                If gCreateLicenseTrain.myStoredRoutes.Count > 0 Then
                    gDoStoredRoute = True
                End If
            End If
            'TMDS.SystemForms.MySystemForms.frmMainMDI.ExtendStoreRoute(gSearchDirection, exitTrack, CurrentCompSelection, enterTrack)
            ExtendStoreRoute(gSearchDirection, exitTrack, CurrentCompSelection, enterTrack)
            If Not ValidateRoute(CurrentCompSelection, enterTrack, exitTrack) Then
                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                ClearSystemVariables()
                Return False
            End If
            Return True
        Catch ex As Exception
            GetErrorDetails(ex)
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
            Dim TempDirection As String = gSearchDirection
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
                        If TempDirection = "LEFT" Then
                            TempDirection = "RIGHT"
                        Else
                            TempDirection = "LEFT"
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
                        ElseIf compToCheck.SignalStatOnMe <> TrackComponents.Track.SignalStatus.SignalAtStop And (compToCheck.TrafficDirection <> TempDirection) _
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

                            Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track:FR:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                            If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                gEntExtTrackSelection.Add(compToCheck)
                            End If
                        ElseIf compToCheck.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop And compToCheck.Occuppied Then
                            'Dim trk As Track = CType(compToCheck, Track)
                            If trk.TrafficDirection <> "" And (trk.TrafficDirection <> TempDirection) Then
                                Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track1:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & TempDirection.ToString)
                                gDoStoredRoute = True
                            End If
                            If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                gEntExtTrackSelection.Add(compToCheck)
                            End If
                        Else
                            If trk.TrafficDirection <> "" And (trk.TrafficDirection <> TempDirection) Then
                                If optEnableARRCSpecificOptions AndAlso trk.AuthorityType.ToUpper.Trim.Contains("TWC") Then
                                Else
                                    Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track2:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & TempDirection.ToString)
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
                            If (trk.DisallowRightSignalRequestOnOccupancy AndAlso TempDirection = "RIGHT" OrElse trk.DisallowLeftSignalRequestOnOccupancy AndAlso TempDirection = "LEFT") Then
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
                                    If TempDirection = "RIGHT" Then '  If TempDirection = "LEFT" Then
                                        If trk.RightLink = enterTrack.GUID Then
                                            OverrideStoredRoute = True
                                            Me.SystemEvent("SearchForRoute-OverrideStoredRoute5-Check-Track:" & trk.GUID & " SignalStatOnMe:" & trk.SignalStatOnMe & " Occuppied:" & trk.Occuppied.ToString)
                                        End If
                                    ElseIf TempDirection = "LEFT" Then
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
                                    Me.SystemEvent("SearchForRoute-gDoStoredRoute-Check-Track5:" & trk.GUID & " TrafficDirection:" & trk.TrafficDirection & " gSearchDirection:" & TempDirection.ToString)
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
                                If optEnableClearSignalWithSignalIntime AndAlso sw.SignalStatOnMe = Switch.SignalStatus.SignalInTime AndAlso sw.TrafficDirection = TempDirection Then
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
                            If (TempDirection = "LEFT" And (compToCheck.ComponentType = "L" Or compToCheck.ComponentType = "D")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
                                TMDS.SystemForms.MySystemForms.frmMainMDI.ClearEntExtSelection()
                                ClearSystemVariables()
                                Exit Function
                            End If
                            If (TempDirection = "RIGHT" And (compToCheck.ComponentType = "R" Or compToCheck.ComponentType = "U")) And (compToCheck.BlockAbsolute Or compToCheck.IndicateOnly) Then
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

                                If ((TempDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                Or ((TempDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

                                    If Not gEntExtTrackSelection.Contains(compToCheck) Then
                                        gEntExtTrackSelection.Add(compToCheck)
                                    End If
                                End If

                            Else
                                If ((TempDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                                Or ((TempDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then

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


            If FlipFlopFound Then
                If TempDirection = "LEFT" Then
                    TempDirection = "RIGHT"
                Else
                    TempDirection = "LEFT"
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
                                        If LastComp.ControlledReverse And TempDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                        End If
                                    Else
                                        If Not LastComp.ControlledReverse And TempDirection = "RIGHT" And LastComp.RightAltLink <> 0 Then
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
                                        If LastComp.ControlledReverse And TempDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
                                            AddSwitchToExecutionCollection(gEntExtExecutionSelection, LastComp)
                                        End If
                                    Else
                                        If Not LastComp.ControlledReverse And TempDirection = "LEFT" And LastComp.LeftAltLink <> 0 Then
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
                                    If compToCheck.ControlledReverse And TempDirection = "LEFT" Then
                                        AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                    End If
                                Else
                                    If Not compToCheck.ControlledReverse And TempDirection = "LEFT" Then
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
                                If compToCheck.ControlledReverse And TempDirection = "RIGHT" Then
                                    AddSwitchToExecutionCollection(gEntExtExecutionSelection, compToCheck)
                                End If
                            Else
                                If Not compToCheck.ControlledReverse And TempDirection = "RIGHT" Then
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
                            If ((TempDirection = "LEFT") And (Mid(compToCheck.ComponentType, 1, 1) = "L" Or Mid(compToCheck.ComponentType, 1, 1) = "U")) _
                            Or ((TempDirection = "RIGHT") And (Mid(compToCheck.ComponentType, 1, 1) = "R" Or Mid(compToCheck.ComponentType, 1, 1) = "D")) Then
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
                        If TempDirection = "LEFT" Then
                            TempDirection = "RIGHT"
                        Else
                            TempDirection = "LEFT"
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

    Public Function SearchRightForRouteNew(ByRef entry_track As Track, ByVal exit_track As Track) As ArrayList
        Dim Robject As Object = Nothing
        Dim Lobject As Object = Nothing
        Try
            Me.SystemEvent("SearchRightForRouteNew1")
            If gSelectedSignalRouteList IsNot Nothing AndAlso gSelectedSignalRouteList.Count > 0 Then
                Dim tmpETrack As Track = Nothing
                Dim tmpCompList As New ArrayList
                Dim tmpComp As Object
                Dim tmpMRSC As New MySRRouteCollection
                For Each tmpMRSC In gSelectedSignalRouteList
                    If tmpMRSC IsNot Nothing AndAlso tmpMRSC.Components IsNot Nothing AndAlso tmpMRSC.Components.Count > 0 Then
                        For Each tmpComp In tmpMRSC.Components
                            If Not tmpCompList.Contains(tmpComp) Then tmpCompList.Add(tmpComp)
                        Next
                    End If
                Next
                If optEnableBRCSpecificChecks Then
                    tmpETrack = GetEntryTrackFromExistingCollection_BRC(tmpCompList)
                Else
                    tmpETrack = GetEntryTrackFromExistingCollection(tmpCompList)
                End If

                If tmpETrack IsNot Nothing Then
                    entry_track = tmpETrack
                End If
            End If

            'Used to iterate through tmdsObjects in the search            
            Dim nextComponent As Object = Nothing
            Dim lastComponent As Object = Nothing
            Dim currentComponentCollection As New ArrayList()

            'Used to validate that we have reached the end of our search for components


            'TEST FOR EXISTENCE OF TRACK ROUTE

            Dim entrySignal As Signal = Me.FindSignalEntryForRightRoute(entry_track)

            Dim exitSignal As Signal = Nothing ' Me.FindSignalExitForRightRoute(exit_track, entrySignal)
            'If optApplyConfigurationForKCT Then
            '    exitSignal = Me.FindSignalExitForRightRoute_KCT(exit_track, entrySignal, True)
            'Else
            exitSignal = Me.FindSignalExitForRightRoute(exit_track, entrySignal, True)
            'End If
            'Check to see if we found an entry and exit signal
            If Not IsNothing(exitSignal) AndAlso Not IsNothing(entrySignal) Then 'Not optEnableBRCSpecificChecks AndAlso 
                'Check to see if the entry signal has track routes associated with it
                If Not IsNothing(entrySignal.myControlPoint.TrackRoutes) Then
                    'This signal has track routes, so lets go through and see if we can find a route with both the entry signal and the exit signal
                    For Each trackRouteComponent As TrackRoute In entrySignal.myControlPoint.TrackRoutes

                        If trackRouteComponent.EntrySignal Is entrySignal And trackRouteComponent.ExitSignal Is exitSignal Then

                            'A route exists for these signals so just go through and add all of the components in the route
                            If Not currentComponentCollection.Contains(entrySignal) Then currentComponentCollection.Add(entrySignal)

                            For Each tmdsComponent As Object In trackRouteComponent.myComponentList
                                If Not currentComponentCollection.Contains(tmdsComponent) Then currentComponentCollection.Add(tmdsComponent)
                            Next

                            If Not currentComponentCollection.Contains(exitSignal) Then currentComponentCollection.Add(exitSignal)

                            'We found all of the components we needed and passed Conflict Check, so go ahead and return them
                            If trackRouteComponent.DoConflictChecks(currentComponentCollection) Then
                                ''Set the global search direction based on the entry signal
                                If trackRouteComponent.EntrySignal.ComponentType = "L" Or trackRouteComponent.EntrySignal.ComponentType = "U" Then
                                    TMDS.GlobalDeclarations.gSearchDirection = "LEFT"
                                Else
                                    TMDS.GlobalDeclarations.gSearchDirection = "RIGHT"
                                End If
                                If optEnableBRCSpecificChecks Then gSelectedSignalRouteList = New ArrayList
                                'Dim MSRC As New MySRRouteCollection
                                'CopyArrayCollection(MSRC.Components, currentComponentCollection)
                                'If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                                'If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                                'If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                                'If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                                '    Dim Comp As Object
                                '    For Each Comp In SR.myExitTracks
                                '        If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                                '    Next
                                'End If
                                'MSRC.MySR = SR
                                'MSRC.MySRDirection = EntryDir
                                'MSRC.HasExitSignal = True
                                'MSRC.CPCollection.Add(EntryCP)
                                'MSRC.ExitCPFound = True
                                'myMSRCCollection.Add(MSRC)





                                Return currentComponentCollection
                            Else
                                currentComponentCollection = New ArrayList
                            End If
                            ''Set the global search direction based on the entry signal
                            'If trackRouteComponent.EntrySignal.ComponentType = "L" Or trackRouteComponent.EntrySignal.ComponentType = "U" Then
                            '    TMDS.GlobalDeclarations.gSearchDirection = "LEFT"
                            'Else
                            '    TMDS.GlobalDeclarations.gSearchDirection = "RIGHT"
                            'End If

                            ''We found all of the components we needed, so go ahead and return them
                            'Return currentComponentCollection

                        End If
                    Next
                End If
            End If
            If optEnableSignalRouteForQuickRoute Then
                Dim EntrySignalHasFlipFlop As Boolean = False
                EntrySignalHasFlipFlop = DoesEntrySignalHasFlip(entrySignal)
                If optApplyConfigurationForKCT Then
                    exitSignal = Me.FindSignalExitForRightRoute_KCT(exit_track, entrySignal)
                Else
                    exitSignal = Me.FindSignalExitForRightRoute(exit_track, entrySignal, True)
                End If

                Dim tempCompCollection As New ArrayList
                tempCompCollection = GetSignalRouteComponentsForQuickRoute(entrySignal, exitSignal, entry_track, exit_track)
                If tempCompCollection Is Nothing Then
                    exitSignal = Me.FindSignalExitForRightRoute(exit_track, entrySignal, False)
                    tempCompCollection = GetSignalRouteComponentsForQuickRoute(entrySignal, exitSignal, entry_track, exit_track)
                End If
                If tempCompCollection IsNot Nothing AndAlso tempCompCollection.Count > 1 Then
                    gSearchDirection = "RIGHT"
                    CopyArrayCollection(currentComponentCollection, tempCompCollection)
                    Return currentComponentCollection
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Return Nothing
        Finally
            Robject = Nothing
            Lobject = Nothing
        End Try
        Return Nothing
    End Function

    Private Function SearchLeftForRouteNew(ByRef enterTrack As Track, ByVal exitTrack As Track) As ArrayList

        Try
            Me.SystemEvent("SearchLeftForRouteNew")

            Dim LastComp As Object = Nothing
            Dim CurrentCompSelection As New ArrayList()
            If gSelectedSignalRouteList IsNot Nothing AndAlso gSelectedSignalRouteList.Count > 0 Then
                Dim tmpETrack As Track = Nothing
                Dim tmpCompList As New ArrayList
                Dim tmpComp As Object
                Dim tmpMRSC As New MySRRouteCollection
                For Each tmpMRSC In gSelectedSignalRouteList
                    If tmpMRSC IsNot Nothing AndAlso tmpMRSC.Components IsNot Nothing AndAlso tmpMRSC.Components.Count > 0 Then
                        For Each tmpComp In tmpMRSC.Components
                            If Not tmpCompList.Contains(tmpComp) Then tmpCompList.Add(tmpComp)
                        Next
                    End If
                Next
                'tmpETrack = GetEntryTrackFromExistingCollection(tmpCompList)
                If optEnableBRCSpecificChecks Then
                    tmpETrack = GetEntryTrackFromExistingCollection_BRC(tmpCompList)
                Else
                    tmpETrack = GetEntryTrackFromExistingCollection(tmpCompList)
                End If
                If tmpETrack IsNot Nothing Then
                    enterTrack = tmpETrack
                End If
            End If

            'TEST FOR EXISTENCE OF TRACK ROUTE

            Dim EntrySignal As Signal = Me.FindSignalEntryForLeftRoute(enterTrack)
            Dim ExitSignal As Signal = Nothing

            'If optApplyConfigurationForKCT Then
            '    ExitSignal = Me.FindSignalExitForLeftRoute_KCT(exitTrack, EntrySignal)
            'Else
            ExitSignal = Me.FindSignalExitForLeftRoute(exitTrack, EntrySignal)
            'End If

            If Not IsNothing(ExitSignal) And Not IsNothing(EntrySignal) Then
                Dim tr As TrackRoute
                If Not IsNothing(EntrySignal.myControlPoint.TrackRoutes) Then
                    For Each tr In EntrySignal.myControlPoint.TrackRoutes
                        If tr.EntrySignal Is EntrySignal And tr.ExitSignal Is ExitSignal Then
                            CurrentCompSelection.Add(EntrySignal)
                            Dim comp As Object
                            For Each comp In tr.myComponentList
                                If Not CurrentCompSelection.Contains(comp) Then CurrentCompSelection.Add(comp)
                            Next
                            If Not CurrentCompSelection.Contains(ExitSignal) Then CurrentCompSelection.Add(ExitSignal)

                            'We found all of the components we needed and passed Conflict Check, so go ahead and return them
                            If tr.DoConflictChecks(CurrentCompSelection) Then
                                If tr.EntrySignal.ComponentType = "L" Or tr.EntrySignal.ComponentType = "U" Then
                                    TMDS.GlobalDeclarations.gSearchDirection = "LEFT"
                                Else
                                    TMDS.GlobalDeclarations.gSearchDirection = "RIGHT"
                                End If
                                If optEnableBRCSpecificChecks Then gSelectedSignalRouteList = New ArrayList
                                Return CurrentCompSelection
                            Else
                                CurrentCompSelection = New ArrayList
                            End If


                            'If tr.EntrySignal.ComponentType = "L" Or tr.EntrySignal.ComponentType = "U" Then
                            '    TMDS.GlobalDeclarations.gSearchDirection = "LEFT"
                            'Else
                            '    TMDS.GlobalDeclarations.gSearchDirection = "RIGHT"
                            'End If
                            'Return CurrentCompSelection
                        End If
                    Next
                End If
            End If

            If optEnableSignalRouteForQuickRoute Then
                Dim EntrySignalHasFlipFlop As Boolean = False
                EntrySignalHasFlipFlop = DoesEntrySignalHasFlip(EntrySignal)
                If optApplyConfigurationForKCT Then
                    ExitSignal = Me.FindSignalExitForLeftRoute_KCT(exitTrack, EntrySignal)
                ElseIf ExitSignal Is Nothing Then
                    ExitSignal = Me.FindSignalExitForLeftRoute(exitTrack, EntrySignal, True)
                End If


                Dim tempCompCollection As New ArrayList
                tempCompCollection = GetSignalRouteComponentsForQuickRoute(EntrySignal, ExitSignal, enterTrack, exitTrack)

                If tempCompCollection Is Nothing Then
                    ExitSignal = Me.FindSignalExitForLeftRoute(exitTrack, EntrySignal, False)
                    tempCompCollection = GetSignalRouteComponentsForQuickRoute(EntrySignal, ExitSignal, enterTrack, exitTrack)
                End If

                If tempCompCollection IsNot Nothing AndAlso tempCompCollection.Count > 1 Then
                    CopyArrayCollection(CurrentCompSelection, tempCompCollection)
                    Return CurrentCompSelection
                End If
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Return Nothing
        Finally

        End Try
        Return Nothing
    End Function

#Region " FIND ENTRY SIGNAL "

    Private Function FindSignalEntryForRightRoute(ByVal entry_track As Track) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Try

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(entry_track.RightLink)


            'If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName AndAlso GetSignalDirection(NextComp) = "RIGHT" Then
            'Else
            '    Dim Comp As Object = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(entry_track.LeftLink)
            '    If Comp IsNot Nothing AndAlso Comp.GetType.ToString = gSignalComponentName AndAlso GetSignalDirection(Comp) = "RIGHT" Then
            '        Return Comp
            '    End If
            'End If


            For i As Integer = 0 To 4
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        If Not NextComp.AutomaticSignal Then
                            Select Case GetSignalDirection(NextComp)
                                Case "RIGHT"
                                    Return NextComp
                                Case Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                            End Select
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End If

                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                End Select
            Next

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FindSignalEntryForLeftRoute(ByVal entry_track As Track) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Try

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(entry_track.LeftLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        If Not NextComp.AutomaticSignal Then
                            Select Case GetSignalDirection(NextComp)
                                Case "LEFT"
                                    Return NextComp
                                Case Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                            End Select
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End If

                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                End Select
            Next

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

#End Region

#Region " FIND EXIT SIGNAL "

    Private Function FindSignalExitForRightRoute_KCT(ByVal ExitTrack As Track, ByVal EntrySignal As Signal, Optional IgnoreFlip As Boolean = False) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Dim EntrySigDirection As String = ""
        Try
            If EntrySignal Is Nothing Then Return Nothing
            If ExitTrack Is Nothing Then Return Nothing
            EntrySigDirection = GetSignalDirection(EntrySignal)
            HasSameDirectionExitSignal = False
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)
                Dim sr As SignalRoute = Nothing
                For Each sr In gSignalRouteList
                    Try
                        If ExitTrack.GUID = sr.ExitTrack.GUID AndAlso sr.HasSameDirectionExitSignal Then
                            HasSameDirectionExitSignal = True
                            Return sr.ExitSignal
                        ElseIf sr.HasSameDirectionExitSignal AndAlso sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                            HasSameDirectionExitSignal = True
                            Return sr.ExitSignal
                        End If
                    Catch ex As Exception

                    End Try
                Next




                For Each sr In gSignalRouteList
                    Try
                        If sr.ExitTrack IsNot Nothing AndAlso ExitTrack.GUID = sr.ExitTrack.GUID Then
                            Return sr.ExitSignal
                        ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                            Return sr.ExitSignal
                            'ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                            ' Return sr.ExitSignal
                        End If
                        'End If
                    Catch ex As Exception
                        GetErrorDetails(ex, "SR FindSignalExitForRightRoute_KCT:" & sr.SystemUID)
                    End Try

                Next
            End If
            returnSignal = GetSRExitSignal(ExitTrack, EntrySignal, IgnoreFlip)
            If returnSignal IsNot Nothing Then Return returnSignal

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        If EntrySigDirection <> GetSignalDirection(NextComp) Then
                            Return NextComp
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End If
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                End Select
            Next



            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FindSignalExitForLeftRoute_KCT(ByVal ExitTrack As Track, ByVal EntrySignal As Signal, Optional IgnoreFlip As Boolean = False) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Dim EntrySigDirection As String = ""
        Try
            If EntrySignal Is Nothing Then Return Nothing
            If ExitTrack Is Nothing Then Return Nothing
            EntrySigDirection = GetSignalDirection(EntrySignal)
            HasSameDirectionExitSignal = False
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)
                Dim sr As SignalRoute = Nothing
                For Each sr In gSignalRouteList
                    Try
                        If ExitTrack.GUID = sr.ExitTrack.GUID AndAlso sr.HasSameDirectionExitSignal Then
                            HasSameDirectionExitSignal = True
                            Return sr.ExitSignal
                        ElseIf sr.HasSameDirectionExitSignal AndAlso sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                            HasSameDirectionExitSignal = True
                            Return sr.ExitSignal
                        End If
                    Catch ex As Exception

                    End Try
                Next

                For Each sr In gSignalRouteList
                    Try
                        If sr.ExitTrack IsNot Nothing AndAlso ExitTrack.GUID = sr.ExitTrack.GUID Then
                            Return sr.ExitSignal
                        ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                            Return sr.ExitSignal
                        ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                            Return sr.ExitSignal
                            'ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                            ' Return sr.ExitSignal
                        End If
                        'End If
                    Catch ex As Exception
                        GetErrorDetails(ex)
                    End Try

                Next
            End If
            returnSignal = GetSRExitSignal(ExitTrack, EntrySignal, IgnoreFlip)
            If returnSignal IsNot Nothing Then Return returnSignal

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        If EntrySigDirection = GetSignalDirection(NextComp) Then
                            Return NextComp
                        Else
                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End If
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                End Select
            Next



            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FindSignalExitForRightRoute(ByVal ExitTrack As Track, ByVal EntrySignal As Signal, Optional IgnoreFlip As Boolean = False) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Dim sr As SignalRoute = Nothing
        Try
            'Let's Do ExitSignal Search from FLipFlopComponents First
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)
                If Not IgnoreFlip Then
                    For Each sr In gSignalRouteList
                        If sr.HasFlipFlop AndAlso sr.ExitSignal IsNot EntrySignal Then
                            If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                Return sr.ExitSignal
                            ElseIf sr.ComponentList.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            End If
                        End If
                    Next
                Else
                    For Each sr In gSignalRouteList
                        If sr.ExitSignal IsNot Nothing Then
                            If sr.thisTerritory.TerritoryID = ExitTrack.myTerritory.TerritoryID AndAlso sr.myDirection = SignalDirection AndAlso sr.ExitSignal.myControlPoint.GUID = ExitTrack.myControlPoint.GUID Then
                                If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                    Return sr.ExitSignal
                                ElseIf sr.ExitTrack.GUID = ExitTrack.GUID Then
                                    Return sr.ExitSignal
                                ElseIf sr.ComponentList.Contains(ExitTrack) Then
                                    Return sr.ExitSignal
                                End If
                            End If
                        End If

                    Next
                End If
            End If

            returnSignal = GetSRExitSignal(ExitTrack, EntrySignal, IgnoreFlip)
            If returnSignal IsNot Nothing Then Return returnSignal



            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "LEFT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                End Select
            Next

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "LEFT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                End Select
            Next
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)

                For Each sr In gSignalRouteList
                    'If sr.myDirection = SignalDirection AndAlso sr.HasFlipFlop AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                    If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                        Return sr.ExitSignal
                    ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                        Return sr.ExitSignal
                    End If
                    'End If
                Next
            End If

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FindSignalExitForLeftRoute(ByVal ExitTrack As Track, ByVal EntrySignal As Signal, Optional IgnoreFlip As Boolean = False) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Dim sr As SignalRoute = Nothing
        Try
            'Let's Do ExitSignal Search from FLipFlopComponents First
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)
                If Not IgnoreFlip Then
                    For Each sr In gSignalRouteList
                        If sr.HasFlipFlop AndAlso sr.ExitSignal IsNot EntrySignal Then
                            If optEnableSpecificOptionsForFECR Then
                                If GetSignalDirection(sr.ExitSignal) = GetSignalDirection(EntrySignal) Then
                                    If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                        Return sr.ExitSignal
                                    ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                        Return sr.ExitSignal
                                    ElseIf sr.ComponentList.Contains(ExitTrack) Then
                                        Return sr.ExitSignal
                                    End If
                                End If
                            Else
                                If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                    Return sr.ExitSignal
                                ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                    Return sr.ExitSignal
                                ElseIf sr.ComponentList.Contains(ExitTrack) Then
                                    Return sr.ExitSignal
                                End If
                            End If


                        End If
                    Next
                    'For Each sr In gSignalRouteList
                    '    If sr.HasFlipFlop Then
                    '        If sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                    '            If EntrySignal IsNot sr.ExitSignal Then
                    '                Return sr.ExitSignal
                    '            End If

                    '        End If
                    '    End If
                    'Next
                Else
                    'For Each sr In gSignalRouteList
                    '    If sr.thisTerritory IsNot Nothing AndAlso sr.ExitSignal IsNot Nothing AndAlso sr.ExitSignal.myControlPoint IsNot Nothing Then
                    '        If sr.thisTerritory.TerritoryID = ExitTrack.myTerritory.TerritoryID AndAlso sr.myDirection = SignalDirection AndAlso sr.ExitSignal.myControlPoint.GUID = ExitTrack.myControlPoint.GUID Then
                    '            If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                    '                Return sr.ExitSignal
                    '            ElseIf sr.ExitTrack.GUID = ExitTrack.GUID Then
                    '                Return sr.ExitSignal
                    '            ElseIf sr.ComponentList.Contains(ExitTrack) Then
                    '                Return sr.ExitSignal
                    '            End If
                    '        End If
                    '    End If
                    'Next
                End If
            End If
            'If EntrySignal IsNot Nothing Then
            '    Dim SignalDirection As String = GetSignalDirection(EntrySignal)

            '    For Each sr In gSignalRouteList
            '        If sr.HasFlipFlop Then
            '            If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
            '                Return sr.ExitSignal
            '            ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
            '                Return sr.ExitSignal
            '            End If
            '        End If
            '    Next
            'End If

            returnSignal = GetSRExitSignal(ExitTrack, EntrySignal, IgnoreFlip)
            If returnSignal IsNot Nothing Then Return returnSignal


            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "RIGHT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                End Select
            Next

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
            For i As Integer = 0 To 3
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "RIGHT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                End Select
            Next
            If EntrySignal IsNot Nothing Then
                Dim SignalDirection As String = GetSignalDirection(EntrySignal)

                For Each sr In gSignalRouteList
                    'If sr.myDirection = SignalDirection AndAlso sr.HasFlipFlop AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                    If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                        Return sr.ExitSignal
                    ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                        Return sr.ExitSignal
                    End If
                    'End If
                Next
            End If

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function GetSRExitSignal(ExitTrack As Track, EntrySignal As Signal, Optional IgnoreFlip As Boolean = False) As Signal
        Dim sr As SignalRoute = Nothing
        Try
            If ExitTrack Is Nothing Then Return Nothing
            If EntrySignal Is Nothing Then Return Nothing
            Dim SignalDirection As String = GetSignalDirection(EntrySignal)

            If ExitTrack IsNot Nothing AndAlso EntrySignal IsNot Nothing Then
                If Not IgnoreFlip Then
                    If SignalDirection = "RIGHT" Then
                        SignalDirection = "LEFT"
                    Else
                        SignalDirection = "RIGHT"
                    End If
                End If
                Select Case SignalDirection
                    Case "RIGHT"
                        ExitTrack.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each sr In ExitTrack.myControlPoint.myRightSignalRoutes
                            If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                Return sr.ExitSignal
                            ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            End If
                        Next
                        EntrySignal.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each sr In EntrySignal.myControlPoint.myRightSignalRoutes
                            If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                Return sr.ExitSignal
                            ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            End If
                        Next
                        If optEnableSpecificOptionsForFECR Then
                            Dim tmptrack As Object = Nothing
                            Dim tmpExitTrack As Track = Nothing
                            tmptrack = FindCompFromGUID(ExitTrack.LeftLink)
                            For i As Integer = 0 To 3
                                If tmptrack IsNot Nothing AndAlso tmptrack.GetType.ToString = gTrackComponentName Then
                                    tmpExitTrack = tmptrack
                                    Exit For
                                Else
                                    tmptrack = FindCompFromGUID(tmptrack.LeftLink)
                                End If
                            Next
                            If tmpExitTrack IsNot Nothing Then
                                tmpExitTrack.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                                For Each sr In tmpExitTrack.myControlPoint.myRightSignalRoutes
                                    If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                        Return sr.ExitSignal
                                    ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                        Return sr.ExitSignal
                                    ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                                        Return sr.ExitSignal
                                    End If
                                Next
                            End If
                        End If
                    Case Else
                        ExitTrack.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each sr In ExitTrack.myControlPoint.myLeftSignalRoutes
                            'If sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                            '    Return sr.ExitSignal
                            'End If
                            If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                Return sr.ExitSignal
                            ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            End If
                        Next
                        EntrySignal.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each sr In EntrySignal.myControlPoint.myLeftSignalRoutes
                            If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = ExitTrack.GUID Then
                                Return sr.ExitSignal
                            ElseIf sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
                                Return sr.ExitSignal
                            End If
                        Next

                        If optEnableMetroLinkSpecificOptions AndAlso ExitTrack IsNot Nothing AndAlso Not ExitTrack.OSReporting Then
                            Dim tmpTrkCollection As New ArrayList
                            Dim comp As Object = Nothing
                            Dim trk As Track = Nothing
                            Try
                                tmpTrkCollection.Add(ExitTrack)
                                ExitTrack.ExpandOutForComponents(tmpTrkCollection)
                                If tmpTrkCollection IsNot Nothing AndAlso tmpTrkCollection.Count > 1 Then
                                    For Each comp In tmpTrkCollection
                                        trk = Nothing
                                        If comp IsNot Nothing AndAlso comp.GetType.ToString = gTrackComponentName AndAlso comp.GUID <> ExitTrack.GUID Then
                                            trk = TryCast(comp, Track)
                                            trk.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                                            For Each sr In trk.myControlPoint.myLeftSignalRoutes
                                                If sr.ExitTrack IsNot Nothing AndAlso sr.ExitTrack.GUID = comp.GUID Then
                                                    Return sr.ExitSignal
                                                End If
                                            Next
                                        End If

                                    Next
                                End If
                            Catch ex As Exception
                                GetErrorDetails(ex)
                            End Try


                        End If


                End Select

            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing

    End Function

    Private Function DoesEntrySignalHasFlip(EntrySignal As Signal) As Boolean
        If EntrySignal Is Nothing Then Return False
        Try
            Dim Sr As SignalRoute = Nothing
            Dim SigDirection As String = GetSignalDirection(EntrySignal)
            If EntrySignal IsNot Nothing AndAlso EntrySignal.myControlPoint IsNot Nothing Then
                Select Case SigDirection
                    Case "RIGHT"
                        If EntrySignal.myControlPoint.myRightSignalRoutes IsNot Nothing Then
                            For Each Sr In EntrySignal.myControlPoint.myRightSignalRoutes
                                If Sr.EntrySignal Is EntrySignal AndAlso Sr.HasFlipFlop Then
                                    Return True
                                End If
                            Next
                        End If
                    Case Else
                        If EntrySignal.myControlPoint.myLeftSignalRoutes IsNot Nothing Then
                            For Each Sr In EntrySignal.myControlPoint.myLeftSignalRoutes
                                If Sr.EntrySignal Is EntrySignal AndAlso Sr.HasFlipFlop Then
                                    Return True
                                End If
                            Next
                        End If
                End Select
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return False
    End Function

#End Region

#Region " PTC SECTION "

    Public Function SearchForPTCRoute(CompList As ArrayList) As ArrayList
        Dim EntryTrack As Track
        Dim ExitTrack As Track
        Dim entrySignal As Signal = Nothing
        Dim exitSignal As Signal = Nothing
        Dim currentComponentCollection As New ArrayList()
        Try
            Me.SystemEvent("SearchRightForPTCRoute")
            If gBeginTrackSelect IsNot Nothing Then
                EntryTrack = gBeginTrackSelect
            Else
                EntryTrack = TMDS.SystemUtilities.FetchData.FindFirstTrackInList(CompList)
            End If
            If gEndTrackSelect IsNot Nothing Then
                ExitTrack = gEndTrackSelect
            Else
                ExitTrack = TMDS.SystemUtilities.FetchData.FindLastTrackInList(CompList)
            End If


            Select Case gSearchDirection
                Case "RIGHT"
                    entrySignal = Me.FindPTCSignalEntryForRightRoute(EntryTrack)
                    exitSignal = Me.FindSignalExitForRightRoute(ExitTrack, entrySignal)
                Case "LEFT"
                    entrySignal = Me.FindSignalEntryForLeftRoute(EntryTrack)
                    exitSignal = Me.FindSignalExitForLeftRoute(ExitTrack, entrySignal)
            End Select
            Dim tempCompCollection As New ArrayList
            tempCompCollection = GetSignalRouteComponentsForQuickRoute(entrySignal, exitSignal, EntryTrack, ExitTrack)
            If tempCompCollection IsNot Nothing AndAlso tempCompCollection.Count > 1 Then
                CopyArrayCollection(currentComponentCollection, tempCompCollection)
                Return currentComponentCollection
            End If
            Return Nothing
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally

        End Try
    End Function

    Public Function SearchForPTCRoute(EntryTrack As Track, ExitTrack As Track, ByVal Direction As String, Optional ByVal EntrySig As Signal = Nothing) As ArrayList

        Dim entrySignal As Signal = Nothing
        Dim exitSignal As Signal = Nothing
        Dim currentComponentCollection As New ArrayList()
        Dim SR As SignalRoute = Nothing
        Try
            Me.SystemEvent("SearchRightForPTCRoute")

            Select Case Direction
                Case "RIGHT"
                    If EntrySig IsNot Nothing Then
                        entrySignal = EntrySig
                    Else
                        entrySignal = Me.FindPTCSignalEntryForRightRoute(EntryTrack)
                    End If

                    If entrySignal IsNot Nothing Then
                        entrySignal.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each SR In entrySignal.myControlPoint.myRightSignalRoutes
                            If SR.EntrySignal IsNot Nothing AndAlso SR.EntrySignal.GUID = EntrySig.GUID Then
                                If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Contains(ExitTrack) Then
                                     Return SR.ComponentList
                                ElseIf SR.ExitTrack IsNot Nothing AndAlso SR.ExitTrack.GUID = ExitTrack.GUID Then
                                    Return SR.ComponentList
                                End If
                            End If
                        Next
                    End If

                    exitSignal = Me.FindSignalExitForRightRoute(ExitTrack, entrySignal, True)
                    If exitSignal Is Nothing Then
                        exitSignal = Me.FindSignalExitForRightRoute(ExitTrack, entrySignal)
                    End If
                Case "LEFT"
                    If EntrySig IsNot Nothing Then
                        entrySignal = EntrySig
                    Else
                        entrySignal = Me.FindSignalEntryForLeftRoute(EntryTrack)
                    End If
                    If entrySignal IsNot Nothing Then
                        entrySignal.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                        For Each SR In entrySignal.myControlPoint.myLeftSignalRoutes
                            If SR.EntrySignal IsNot Nothing AndAlso SR.EntrySignal.GUID = EntrySig.GUID Then
                                If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Contains(ExitTrack) Then
                                    Return SR.ComponentList
                                ElseIf SR.ExitTrack IsNot Nothing AndAlso SR.ExitTrack.GUID = ExitTrack.GUID Then
                                    Return SR.ComponentList
                                End If
                            End If
                        Next
                    End If
                    exitSignal = Me.FindSignalExitForLeftRoute(ExitTrack, entrySignal, True)
                    If exitSignal Is Nothing Then
                        exitSignal = Me.FindSignalExitForLeftRoute(ExitTrack, entrySignal)
                    End If
            End Select
            Dim tempCompCollection As New ArrayList
            tempCompCollection = GetSignalRouteComponentsForQuickRoute(entrySignal, exitSignal, EntryTrack, ExitTrack)
            If tempCompCollection IsNot Nothing AndAlso tempCompCollection.Count > 1 Then
                CopyArrayCollection(currentComponentCollection, tempCompCollection)
                Return currentComponentCollection
            End If
            Return Nothing
        Catch ex As Exception
            GetErrorDetails(ex)
            Return Nothing
        Finally

        End Try
    End Function

    Private Function FindPTCSignalEntryForRightRoute(ByVal entry_track As Track) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Try

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(entry_track.LeftLink)




            For i As Integer = 0 To 10
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "RIGHT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
                End Select
            Next

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FindPTCSignalEntryForLeftRoute(ByVal entry_track As Track) As Signal
        Dim NextComp As Object = Nothing
        Dim returnSignal As Signal = Nothing
        Try

            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(entry_track.RightLink)
            For i As Integer = 0 To 10
                If NextComp Is Nothing Then Exit For
                Select Case NextComp.GetType.ToString
                    Case gSignalComponentName
                        Select Case GetSignalDirection(NextComp)
                            Case "LEFT"
                                Return NextComp
                            Case Else
                                NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                        End Select
                    Case Else
                        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
                End Select
            Next

            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    'Private Function FindSignalExitForRightRoute(ByVal ExitTrack As Track, ByVal EntrySignal As Signal) As Signal
    '    Dim NextComp As Object = Nothing
    '    Dim returnSignal As Signal = Nothing
    '    Try
    '        'Let's Do ExitSignal Search from FLipFlopComponents First
    '        If EntrySignal IsNot Nothing Then
    '            Dim SignalDirection As String = GetSignalDirection(EntrySignal)
    '            Dim sr As SignalRoute = Nothing
    '            For Each sr In gSignalRouteList
    '                If sr.HasFlipFlop Then
    '                    If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                        Return sr.ExitSignal
    '                    ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
    '                        Return sr.ExitSignal
    '                    End If
    '                End If
    '            Next
    '        End If

    '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.LeftLink)
    '        For i As Integer = 0 To 3
    '            If NextComp Is Nothing Then Exit For
    '            Select Case NextComp.GetType.ToString
    '                Case gSignalComponentName
    '                    Select Case GetSignalDirection(NextComp)
    '                        Case "LEFT"
    '                            Return NextComp
    '                        Case Else
    '                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
    '                    End Select
    '                Case Else
    '                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
    '            End Select
    '        Next

    '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
    '        For i As Integer = 0 To 3
    '            If NextComp Is Nothing Then Exit For
    '            Select Case NextComp.GetType.ToString
    '                Case gSignalComponentName
    '                    Select Case GetSignalDirection(NextComp)
    '                        Case "LEFT"
    '                            Return NextComp
    '                        Case Else
    '                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
    '                    End Select
    '                Case Else
    '                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
    '            End Select
    '        Next
    '        If EntrySignal IsNot Nothing Then
    '            Dim SignalDirection As String = GetSignalDirection(EntrySignal)
    '            Dim sr As SignalRoute = Nothing
    '            For Each sr In gSignalRouteList
    '                'If sr.myDirection = SignalDirection AndAlso sr.HasFlipFlop AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                    Return sr.ExitSignal
    '                ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
    '                    Return sr.ExitSignal
    '                End If
    '                'End If
    '            Next
    '        End If

    '        Return Nothing
    '    Catch ex As Exception
    '        SystemException(ex.ToString)
    '        Return Nothing
    '    End Try
    'End Function

    'Private Function FindSignalExitForLeftRoute(ByVal ExitTrack As Track, ByVal EntrySignal As Signal) As Signal
    '    Dim NextComp As Object = Nothing
    '    Dim returnSignal As Signal = Nothing
    '    Try
    '        'Let's Do ExitSignal Search from FLipFlopComponents First
    '        If EntrySignal IsNot Nothing Then
    '            Dim SignalDirection As String = GetSignalDirection(EntrySignal)
    '            Dim sr As SignalRoute = Nothing
    '            For Each sr In gSignalRouteList
    '                If sr.HasFlipFlop Then
    '                    If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                        Return sr.ExitSignal
    '                    ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
    '                        Return sr.ExitSignal
    '                    End If
    '                End If
    '            Next
    '        End If

    '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
    '        For i As Integer = 0 To 3
    '            If NextComp Is Nothing Then Exit For
    '            Select Case NextComp.GetType.ToString
    '                Case gSignalComponentName
    '                    Select Case GetSignalDirection(NextComp)
    '                        Case "RIGHT"
    '                            Return NextComp
    '                        Case Else
    '                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
    '                    End Select
    '                Case Else
    '                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.RightLink)
    '            End Select
    '        Next

    '        NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(ExitTrack.RightLink)
    '        For i As Integer = 0 To 3
    '            If NextComp Is Nothing Then Exit For
    '            Select Case NextComp.GetType.ToString
    '                Case gSignalComponentName
    '                    Select Case GetSignalDirection(NextComp)
    '                        Case "RIGHT"
    '                            Return NextComp
    '                        Case Else
    '                            NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
    '                    End Select
    '                Case Else
    '                    NextComp = TMDS.SystemUtilities.FetchData.FindComponentFromGUID(NextComp.LeftLink)
    '            End Select
    '        Next
    '        If EntrySignal IsNot Nothing Then
    '            Dim SignalDirection As String = GetSignalDirection(EntrySignal)
    '            Dim sr As SignalRoute = Nothing
    '            For Each sr In gSignalRouteList
    '                'If sr.myDirection = SignalDirection AndAlso sr.HasFlipFlop AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                If sr.myExitTracks IsNot Nothing AndAlso sr.myExitTracks.Count > 0 AndAlso sr.myExitTracks.Contains(ExitTrack) Then
    '                    Return sr.ExitSignal
    '                ElseIf sr.ComponentList IsNot Nothing AndAlso sr.ComponentList.Count > 0 AndAlso sr.ComponentList.Contains(ExitTrack) Then
    '                    Return sr.ExitSignal
    '                End If
    '                'End If
    '            Next
    '        End If

    '        Return Nothing
    '    Catch ex As Exception
    '        SystemException(ex.ToString)
    '        Return Nothing
    '    End Try
    'End Function

#End Region

    Private Function GetEntryTrackFromExistingCollection(CompList As ArrayList) As Track
        Dim Comp As Object = Nothing
        Dim LastTrack As Track = Nothing
        Dim FlipIt As Boolean = False
        Dim Direction As String = gSearchDirection
        Dim LastTrackToReturn As Track = Nothing
        Try
            If CompList Is Nothing Then Return Nothing
            If CompList.Count = 0 Then Return Nothing
            For Each Comp In CompList
                If Comp IsNot Nothing Then
                    Select Case Comp.GetType.ToString
                        Case gTrackComponentName
                            LastTrack = Comp
                        Case gFlipFlopComponentName
                            If FlipIt Then
                                FlipIt = False
                            Else
                                FlipIt = True
                            End If
                            If Direction = "RIGHT" Then
                                Direction = "LEFT"
                            Else
                                Direction = "RIGHT"
                            End If
                    End Select
                End If
            Next
            Comp = Nothing
            Comp = LastTrack
            LastTrack = Nothing
            For I As Integer = 0 To 10
                Select Case gSearchDirection
                    Case "RIGHT"
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                LastTrack = Comp
                                Comp = FindCompFromGUID(Comp.RightLink)
                            Case gSignalComponentName
                                If GetSignalDirection(Comp, FlipIt) = Direction Then
                                    Return LastTrack
                                End If
                                Comp = FindCompFromGUID(Comp.RightLink)
                        End Select
                    Case Else
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                LastTrack = Comp
                                Comp = FindCompFromGUID(Comp.LeftLink)
                            Case gSignalComponentName
                                If GetSignalDirection(Comp, FlipIt) = Direction Then
                                    Return LastTrack
                                End If
                                Comp = FindCompFromGUID(Comp.LeftLink)
                        End Select
                End Select
            Next

        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Private Function GetEntryTrackFromExistingCollection_BRC(CompList As ArrayList) As Track
        Dim Comp As Object = Nothing
        Dim LastTrack As Track = Nothing
        Dim FlipIt As Boolean = False
        Dim Direction As String = gSearchDirection
        Dim LastTrackToReturn As Track = Nothing
        Try
            If CompList Is Nothing Then Return Nothing
            If CompList.Count = 0 Then Return Nothing
            For Each Comp In CompList
                If Comp IsNot Nothing Then
                    Select Case Comp.GetType.ToString
                        Case gTrackComponentName
                            LastTrack = Comp
                        Case gFlipFlopComponentName
                            If FlipIt Then
                                FlipIt = False
                            Else
                                FlipIt = True
                            End If
                            If Direction = "RIGHT" Then
                                Direction = "LEFT"
                            Else
                                Direction = "RIGHT"
                            End If
                    End Select
                End If
            Next
            Comp = Nothing
            Comp = LastTrack
            LastTrack = Nothing
            For I As Integer = 0 To CompList.Count - 1 '10
                Select Case gSearchDirection
                    Case "RIGHT"
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                LastTrack = Comp
                                Comp = FindCompFromGUID(Comp.RightLink)
                            Case gSignalComponentName
                                If GetSignalDirection(Comp, FlipIt) = Direction Then
                                    If LastTrack IsNot Nothing Then
                                        LastTrackToReturn = LastTrack
                                    End If
                                End If
                                Comp = FindCompFromGUID(Comp.RightLink)
                        End Select
                    Case Else
                        Select Case Comp.GetType.ToString
                            Case gTrackComponentName
                                LastTrack = Comp
                                Comp = FindCompFromGUID(Comp.LeftLink)
                            Case gSignalComponentName
                                If GetSignalDirection(Comp, FlipIt) = Direction Then
                                    'Return LastTrack
                                    If LastTrack IsNot Nothing Then
                                        LastTrackToReturn = LastTrack
                                    End If
                                End If
                                Comp = FindCompFromGUID(Comp.LeftLink)
                        End Select
                End Select
            Next
            If LastTrackToReturn IsNot Nothing Then Return LastTrackToReturn
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

#End Region

    Private Function GetSignalRouteComponentsForQuickRoute(ByRef EntrySignal As Signal, ByRef ExitSignal As Signal, ByVal enterTrack As Track, ByVal exitTrack As Track) As ArrayList
        Dim TempEntrySignal As Signal = Nothing
        Dim TempEntrytrack As Track = Nothing
        Dim tmpmyEntrySignalRoutueCollection As New ArrayList
        Dim tmpMyValidRoutueCollection As New ArrayList
        Try
            TempEntrySignal = EntrySignal
            Dim HasRouteCollection As Boolean = False

            'If gSelectedSignalRouteList IsNot Nothing AndAlso gSelectedSignalRouteList.Count > 0 Then
            '    tmpmyEntrySignalRoutueCollection = SearchFromSelectedRoute(ExitSignal)
            '    Return tmpmyEntrySignalRoutueCollection
            'End If
            tmpmyEntrySignalRoutueCollection = New ArrayList

            If Not IsNothing(EntrySignal) Then Dim EntrySignalInfo As String = "GetSignalRouteComponentsForQuickRoute-ENTRY SIGNAL:" & EntrySignal.GUID & "-" & EntrySignal.Name : SystemEvent(EntrySignalInfo)
            If Not IsNothing(ExitSignal) Then Dim ExitSignalInfo As String = "GetSignalRouteComponentsForQuickRoute-EXIT SIGNAL:" & ExitSignal.GUID & "-" & ExitSignal.Name : SystemEvent(ExitSignalInfo)
            Dim EntryTrackInfo As String = "GetSignalRouteComponentsForQuickRoute-ENTRY TRACK:" & enterTrack.GUID & "-" & enterTrack.Name : SystemEvent(EntryTrackInfo)
            Dim ExitTrackInfo As String = "GetSignalRouteComponentsForQuickRoute-EXIT TRACK:" & exitTrack.GUID & "-" & exitTrack.Name : SystemEvent(ExitTrackInfo)


            Dim EntrySignalDirection As String = ""
            Dim ConfirmedFirstRoute As SignalRoute = Nothing
            If TempEntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                Dim temLis As New ArrayList
                Dim MSRC As MySRRouteCollection
                temLis = FindSignalRoute(EntrySignal, ExitSignal, False, 100, exitTrack)
                If temLis IsNot Nothing AndAlso temLis.Count > 0 Then
                    SystemEvent("GetSignalRouteComponentsForQuickRoute-NumberOfRoutesFound:" & temLis.Count)
                    For Each MSRC In temLis
                        If MSRC.HasExitSignal Then
                            CleanComps(MSRC.Components, EntrySignal, ExitSignal, MSRC.MySRDirection)
                            If gSelectedSignalRouteList Is Nothing Then gSelectedSignalRouteList = New ArrayList
                            If Not gSelectedSignalRouteList.Contains(MSRC) Then gSelectedSignalRouteList.Add(MSRC)
                            Return MSRC.Components
                        End If
                    Next
                    Dim tmpMSRC As MySRRouteCollection = Nothing
                    For Each MSRC In temLis
                        tmpMSRC = FindRouteThrouSignals(MSRC, ExitSignal)

                        If tmpMSRC IsNot Nothing Then
                            CleanComps(tmpMSRC.Components, MSRC.MySR.EntrySignal, ExitSignal, MSRC.MySRDirection)
                            CopyArrayCollection(MSRC.Components, tmpMSRC.Components)
                            CopyArrayCollection(MSRC.EntrySignalCollection, tmpMSRC.EntrySignalCollection)
                            CopyArrayCollection(MSRC.ExitSignalCollection, tmpMSRC.ExitSignalCollection)
                            MSRC.HasExitSignal = True
                            If gSelectedSignalRouteList Is Nothing Then gSelectedSignalRouteList = New ArrayList
                            If Not gSelectedSignalRouteList.Contains(MSRC) Then gSelectedSignalRouteList.Add(MSRC)
                            Return tmpMSRC.Components
                        End If
                    Next
                End If
            End If

        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            TempEntrytrack = Nothing
            tmpmyEntrySignalRoutueCollection = Nothing
            tmpMyValidRoutueCollection = Nothing
        End Try
        Return Nothing
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

    Private Function FindRouteThrouSignals(MSRC As MySRRouteCollection, ExitSignal As Signal) As MySRRouteCollection
        Dim Sg As Signal = Nothing
        Dim SR As SignalRoute = Nothing
        Dim tmpMSRC As MySRRouteCollection = Nothing
        Dim Comp As Object = Nothing
        Dim SortList As New Generic.List(Of TMDS.ManagementClasses.SignalRoute)
        Dim TempSigCollection As New ArrayList
        Dim tmpCompList As New ArrayList
        Dim SearchDirection As String = ""
        Try

            CopyArrayCollection(TempSigCollection, MSRC.EntrySignalCollection)
            TempSigCollection.Reverse()

            'TempSigCollection.Clear()
            'TempSigCollection.Add(MSRC.MySR.ExitSignal)
            SearchDirection = MSRC.MySRDirection
            If MSRC.MySR IsNot Nothing Then
                Select Case MSRC.MySRDirection
                    Case "RIGHT"
                        If MSRC.MySR.HasFlipFlop Then
                            SearchDirection = "LEFT"
                        End If
                    Case "LEFT"
                        If MSRC.MySR.HasFlipFlop Then SearchDirection = "RIGHT"
                End Select
            End If

            For Each Sg In TempSigCollection ' MSRC.EntrySignalCollection
                Dim TempSRCollection As New ArrayList
                If Not Sg.AutomaticSignal AndAlso Sg.myControlPoint IsNot Nothing Then
                    Select Case SearchDirection 'MSRC.MySRDirection
                        Case "RIGHT"
                            Sg.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                            SortList.Clear()
                            For Each SR In Sg.myControlPoint.myRightSignalRoutes
                                If SR.EntrySignal.GUID = Sg.GUID AndAlso DoSRConflictCheck(SR.ComponentList) Then
                                    If Not SortList.Contains(SR) Then SortList.Add(SR)
                                End If
                            Next
                            SortList.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                            For Each SR In SortList
                                If Not TempSRCollection.Contains(SR) Then TempSRCollection.Add(SR)
                            Next



                            For Each SR In TempSRCollection
                                Dim TmpSRList As New ArrayList
                                TmpSRList = FindSignalRoute(SR.EntrySignal, ExitSignal, True, MSRC.Components.Count + 50)
                                If TmpSRList IsNot Nothing AndAlso TmpSRList.Count > 0 Then
                                    For Each tmpMSRC In TmpSRList
                                        If tmpMSRC.HasExitSignal Then
                                            tmpCompList = New ArrayList
                                            For Each Comp In MSRC.Components
                                                If Comp IsNot Nothing AndAlso Comp Is Sg Then Exit For
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            For Each Comp In tmpMSRC.Components
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            CopyArrayCollection(tmpMSRC.Components, tmpCompList)

                                            If MSRC.EntrySignalCollection Is Nothing Then MSRC.EntrySignalCollection = New ArrayList
                                            If tmpMSRC.EntrySignalCollection Is Nothing Then tmpMSRC.EntrySignalCollection = New ArrayList
                                            tmpCompList = New ArrayList
                                            For Each Comp In MSRC.EntrySignalCollection
                                                If Comp IsNot Nothing AndAlso Comp Is Sg Then Exit For
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            For Each Comp In tmpMSRC.EntrySignalCollection
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            CopyArrayCollection(tmpMSRC.EntrySignalCollection, tmpCompList)

                                            If MSRC.ExitSignalCollection Is Nothing Then MSRC.ExitSignalCollection = New ArrayList
                                            If tmpMSRC.ExitSignalCollection Is Nothing Then tmpMSRC.ExitSignalCollection = New ArrayList
                                            tmpCompList = New ArrayList
                                            If (MSRC.ExitSignalCollection IsNot Nothing AndAlso MSRC.ExitSignalCollection.Count > 0) AndAlso (tmpMSRC.ExitSignalCollection IsNot Nothing AndAlso tmpMSRC.ExitSignalCollection.Count > 0) Then
                                                For Each Comp In MSRC.ExitSignalCollection
                                                    If Comp IsNot Nothing AndAlso tmpMSRC.ExitSignalCollection.Count > 0 AndAlso Comp Is tmpMSRC.ExitSignalCollection(0) Then Exit For
                                                    If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                                Next
                                                For Each Comp In tmpMSRC.ExitSignalCollection
                                                    If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                                Next
                                                CopyArrayCollection(tmpMSRC.ExitSignalCollection, tmpCompList)
                                            End If



                                            Return tmpMSRC
                                        End If
                                    Next
                                End If
                            Next
                        Case "LEFT"
                            Sg.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                            'For Each SR In Sg.myControlPoint.myLeftSignalRoutes
                            '    If SR.EntrySignal.GUID = Sg.GUID Then
                            '        If Not TempSRCollection.Contains(SR) Then TempSRCollection.Add(SR)
                            '    End If
                            'Next
                            SortList.Clear()
                            For Each SR In Sg.myControlPoint.myLeftSignalRoutes
                                If SR.EntrySignal.GUID = Sg.GUID AndAlso DoSRConflictCheck(SR.ComponentList) Then
                                    If Not SortList.Contains(SR) Then SortList.Add(SR)
                                End If
                            Next
                            SortList.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                            For Each SR In SortList
                                If Not TempSRCollection.Contains(SR) Then TempSRCollection.Add(SR)
                            Next
                            For Each SR In TempSRCollection
                                Dim TmpSRList As New ArrayList
                                TmpSRList = FindSignalRoute(SR.EntrySignal, ExitSignal, True, MSRC.Components.Count + 50)
                                If TmpSRList IsNot Nothing AndAlso TmpSRList.Count > 0 Then
                                    For Each tmpMSRC In TmpSRList
                                        If tmpMSRC.HasExitSignal Then
                                            tmpCompList = New ArrayList
                                            For Each Comp In MSRC.Components
                                                If Comp IsNot Nothing AndAlso Comp Is Sg Then Exit For
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            For Each Comp In tmpMSRC.Components
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            CopyArrayCollection(tmpMSRC.Components, tmpCompList)

                                            If MSRC.EntrySignalCollection Is Nothing Then MSRC.EntrySignalCollection = New ArrayList
                                            If tmpMSRC.EntrySignalCollection Is Nothing Then tmpMSRC.EntrySignalCollection = New ArrayList
                                            tmpCompList = New ArrayList
                                            For Each Comp In MSRC.EntrySignalCollection
                                                If Comp IsNot Nothing AndAlso Comp Is Sg Then Exit For
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            For Each Comp In tmpMSRC.EntrySignalCollection
                                                If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                            Next
                                            CopyArrayCollection(tmpMSRC.EntrySignalCollection, tmpCompList)

                                            If MSRC.ExitSignalCollection Is Nothing Then MSRC.ExitSignalCollection = New ArrayList
                                            If tmpMSRC.ExitSignalCollection Is Nothing Then tmpMSRC.ExitSignalCollection = New ArrayList
                                            tmpCompList = New ArrayList
                                            If (MSRC.ExitSignalCollection IsNot Nothing AndAlso MSRC.ExitSignalCollection.Count > 0) AndAlso (tmpMSRC.ExitSignalCollection IsNot Nothing AndAlso tmpMSRC.ExitSignalCollection.Count > 0) Then
                                                For Each Comp In MSRC.ExitSignalCollection
                                                    If Comp IsNot Nothing AndAlso tmpMSRC.ExitSignalCollection.Count > 0 AndAlso Comp Is tmpMSRC.ExitSignalCollection(0) Then Exit For
                                                    If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                                Next
                                                For Each Comp In tmpMSRC.ExitSignalCollection
                                                    If Not tmpCompList.Contains(Comp) Then tmpCompList.Add(Comp)
                                                Next
                                                CopyArrayCollection(tmpMSRC.ExitSignalCollection, tmpCompList)
                                            End If
                                            Return tmpMSRC
                                        End If
                                    Next
                                End If
                            Next
                    End Select
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        Finally
            TempSigCollection = Nothing
            SortList = Nothing
        End Try
        Return Nothing
    End Function

    Private Function FindSignalRoute(ByVal EntrySignal As Signal, ByVal ExitSignal As Signal, Optional ReturnOnlyExitSignalFound As Boolean = False, Optional EntryControlPointCount As Integer = 200, Optional ExitTrack As Track = Nothing) As ArrayList ', EntryTrack As Track, ExitTrack As Track,
        Dim CPList As New ArrayList
        Dim EntryCP As ControlPoint = Nothing
        Dim ExitCP As ControlPoint = Nothing
        Dim EntryDir As String = ""
        Dim ExitDir As String = ""

        Dim SR As SignalRoute = Nothing
        Dim NextComp As Object = Nothing
        Dim TmpSRList As New ArrayList
        Dim myMSRCCollection As New ArrayList
        Try

            If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                EntryDir = GetSignalDirection(EntrySignal)
                ExitDir = GetSignalDirection(ExitSignal)
                EntryCP = EntrySignal.myControlPoint
                ExitCP = ExitSignal.myControlPoint
                'EntryControlPointCount =  FindEntryControlPointCount(EntryCP)
                Select Case EntryDir
                    Case "RIGHT"
                        DoRightSignalRouteSearch(myMSRCCollection, EntryCP, ExitCP, EntrySignal, ExitSignal, EntryDir, EntryControlPointCount, ExitTrack)
                    Case Else
                        'DO LEFT SEARCH
                        DoLeftSignalRouteSearch(myMSRCCollection, EntryCP, ExitCP, EntrySignal, ExitSignal, EntryDir, EntryControlPointCount, ExitTrack)
                End Select
            End If
            If myMSRCCollection IsNot Nothing AndAlso myMSRCCollection.Count > 0 Then
                TmpSRList = New ArrayList
                Dim MSRC As MySRRouteCollection
                For Each MSRC In myMSRCCollection
                    If Not ReturnOnlyExitSignalFound Then
                        If MSRC.ExitCPFound Then
                            TmpSRList.Add(MSRC)
                        End If
                    Else
                        If MSRC.HasExitSignal Then
                            TmpSRList.Add(MSRC)
                        End If
                    End If
                Next
                Return TmpSRList
            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Private Sub DoRightSignalRouteSearch(ByRef myMSRCCollection As ArrayList, EntryCP As ControlPoint, ExitCP As ControlPoint, EntrySignal As Signal, ExitSignal As Signal, EntryDir As String, EntryControlPointCount As Integer, Optional ExitTrack As Track = Nothing)
        Dim SR As SignalRoute = Nothing
        Dim NextComp As Object = Nothing
        Dim TmpSRList As New ArrayList
        Dim SortList As New Generic.List(Of TMDS.ManagementClasses.SignalRoute)
        Dim tmpDirection As String = "RIGHT"
        Dim tmpLastTrack As Track = Nothing
        Try
            TmpSRList = New ArrayList
            'EntryCP.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) String.Compare(x.myPriority, y.myPriority))
            EntryCP.myControlPoint.myRightSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
            For Each SR In EntryCP.myControlPoint.myRightSignalRoutes
                If SR.EntrySignal.GUID = EntrySignal.GUID Then 'AndAlso SR.SystemUID = "2F86D3FD93"
                    If DoSRConflictCheck(SR.ComponentList) Then
                        If Not SortList.Contains(SR) Then SortList.Add(SR)
                    End If
                End If
            Next
            If SortList Is Nothing Then Exit Sub
            If SortList.Count = 0 Then Exit Sub
            For Each SR In SortList
                If SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.ExitSignal.GUID = ExitSignal.GUID Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                End If
            Next
            SortList.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
            For Each SR In SortList
                If SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.ExitSignal.GUID = ExitSignal.GUID Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                ElseIf Not HasSameDirectionExitSignal AndAlso ExitTrack IsNot Nothing AndAlso SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.myExitTracks.Contains(ExitTrack) Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                ElseIf SR.EntrySignal.GUID = EntrySignal.GUID Then 'AndAlso SR.SystemUID = "2F86D3FD93"
                    If Not TmpSRList.Contains(SR) Then TmpSRList.Add(SR)
                End If
            Next



            myMSRCCollection = New ArrayList
            For Each SR In TmpSRList
                Dim MSRC As New MySRRouteCollection
                CopyArrayCollection(MSRC.Components, SR.ComponentList)
                If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                MSRC.MySR = SR
                MSRC.MySRDirection = EntryDir
                NextComp = FindCompFromGUID(SR.ExitSignal.LeftLink)
                'If EntryCP Is ExitCP Then MSRC.ExitCPFound = True
                If SR.ExitSignal.myControlPoint Is ExitCP Then MSRC.ExitCPFound = True
                Dim CompList As New ArrayList
                If NextComp IsNot Nothing Then
                    If NextComp.myControlPoint Is EntryCP Then
                    Else
                        If Not MSRC.CPCollection.Contains(NextComp.myControlPoint) Then MSRC.CPCollection.Add(NextComp.myControlPoint)
                    End If
                    NextComp = FindCompFromGUID(NextComp.RightLink)
                    If Not MSRC.Components.Contains(NextComp) Then MSRC.Components.Add(NextComp)

                    If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName AndAlso Not NextComp.AutomaticSignal Then
                        If EntryDir = GetSignalDirection(NextComp, SR.HasFlipFlop) Then
                            If Not MSRC.EntrySignalCollection.Contains(NextComp) Then MSRC.EntrySignalCollection.Add(NextComp)
                        Else
                            If Not MSRC.ExitSignalCollection.Contains(NextComp) Then MSRC.ExitSignalCollection.Add(NextComp)
                            If NextComp Is ExitSignal Then
                                MSRC.HasExitSignal = True
                            End If
                        End If
                    End If
                End If
                Dim FlipIt As Boolean = False
                Dim tmpComp As Object = Nothing
                For Each tmpComp In SR.ComponentList
                    If tmpComp.GetType.ToString = gFlipFlopComponentName Then
                        FlipIt = True
                    End If
                Next


                For i As Integer = 0 To EntryControlPointCount

                    If NextComp Is Nothing Then Exit For

                    If Not MSRC.Components.Contains(NextComp) Then MSRC.Components.Add(NextComp)
                    Select Case NextComp.GetType.ToString
                        Case gRRXINGComponentName
                            Dim ExitGUID As Integer
                            Dim RR As RRXING = TryCast(NextComp, RRXING)
                            ExitGUID = RR.FindExitGUID(tmpLastTrack.GUID)
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(ExitGUID)
                            'If UseRRALTLink(NextComp, MSRC.Components) Then
                            '    If Not FlipIt Then
                            '        NextComp = FindCompFromGUID(NextComp.LeftAltLink)
                            '    Else
                            '        NextComp = FindCompFromGUID(NextComp.RightAltLink)
                            '    End If
                            'Else
                            '    If FlipIt Then
                            '        NextComp = FindCompFromGUID(NextComp.LeftLink)
                            '    Else
                            '        NextComp = FindCompFromGUID(NextComp.RightLink)
                            '    End If
                            'End If

                        Case gFlipFlopComponentName
                            'If FlipIt Then
                            '    FlipIt = False
                            '    NextComp = FindCompFromGUID(NextComp.LeftLink)
                            'Else
                            '    FlipIt = True
                            '    NextComp = FindCompFromGUID(NextComp.RightLink)
                            'End If
                            Dim ExitGUID As Integer
                            TMDS.SystemUtilities.FetchData.DoFlipSearch(tmpLastTrack, tmpDirection, NextComp, ExitGUID)
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(ExitGUID)
                            If FlipIt Then
                                FlipIt = False
                            Else
                                FlipIt = True
                            End If
                        Case gTrackComponentName
                            tmpLastTrack = NextComp
                            If NextComp.myControlPoint.GUID = ExitCP.GUID Then
                                MSRC.ExitCPFound = True
                            ElseIf NextComp.myControlPoint.GUID <> EntryCP.GUID Then
                                If MSRC.ExitCPFound Then Exit For
                                If NextComp.myControlPoint.GUID = ExitCP.GUID Then MSRC.ExitCPFound = True
                                If Not MSRC.CPCollection.Contains(NextComp.myControlPoint) Then MSRC.CPCollection.Add(NextComp.myControlPoint)
                            End If
                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            End If
                        Case gSignalComponentName
                            If Not NextComp.AutomaticSignal Then
                                If EntryDir = GetSignalDirection(NextComp, SR.HasFlipFlop) Then
                                    If Not MSRC.EntrySignalCollection.Contains(NextComp) Then MSRC.EntrySignalCollection.Add(NextComp)
                                Else
                                    If Not MSRC.ExitSignalCollection.Contains(NextComp) Then MSRC.ExitSignalCollection.Add(NextComp)
                                    If NextComp Is ExitSignal Then
                                        MSRC.HasExitSignal = True
                                        Exit For
                                    End If
                                End If
                            End If

                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            End If
                        Case Else

                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            End If

                    End Select
                Next
                myMSRCCollection.Add(MSRC)
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Private Sub DoLeftSignalRouteSearch(ByRef myMSRCCollection As ArrayList, EntryCP As ControlPoint, ExitCP As ControlPoint, EntrySignal As Signal, ExitSignal As Signal, EntryDir As String, EntryControlPointCount As Integer, Optional ExitTrack As Track = Nothing)
        Dim SR As SignalRoute = Nothing
        Dim NextComp As Object = Nothing
        Dim TmpSRList As New ArrayList
        Dim SortList As New Generic.List(Of TMDS.ManagementClasses.SignalRoute)
        Dim tmpDirection As String = "LEFT"
        Dim tmpLastTrack As Track = Nothing
        Try
            myMSRCCollection = New ArrayList
            TmpSRList = New ArrayList

            EntryCP.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
            For Each SR In EntryCP.myControlPoint.myLeftSignalRoutes
                If SR.EntrySignal.GUID = EntrySignal.GUID Then 'AndAlso SR.SystemUID = "2F86D3FD93"
                    If DoSRConflictCheck(SR.ComponentList) Then
                        If Not SortList.Contains(SR) Then SortList.Add(SR)
                    End If
                End If
            Next
            SortList.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
            If SortList Is Nothing Then Exit Sub
            If SortList.Count = 0 Then Exit Sub
            For Each SR In SortList
                If SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.ExitSignal.GUID = ExitSignal.GUID Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                End If
            Next
            For Each SR In SortList
                If SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.ExitSignal.GUID = ExitSignal.GUID Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                ElseIf Not HasSameDirectionExitSignal AndAlso ExitTrack IsNot Nothing AndAlso SR.EntrySignal.GUID = EntrySignal.GUID AndAlso SR.myExitTracks.Contains(ExitTrack) Then
                    Dim MSRC As New MySRRouteCollection
                    CopyArrayCollection(MSRC.Components, SR.ComponentList)
                    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
                        Dim Comp As Object
                        For Each Comp In SR.myExitTracks
                            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
                        Next
                    End If
                    MSRC.MySR = SR
                    MSRC.MySRDirection = EntryDir
                    MSRC.HasExitSignal = True
                    MSRC.CPCollection.Add(EntryCP)
                    MSRC.ExitCPFound = True
                    myMSRCCollection.Add(MSRC)
                    Exit Sub
                ElseIf SR.EntrySignal.GUID = EntrySignal.GUID Then
                    If Not TmpSRList.Contains(SR) Then TmpSRList.Add(SR)
                End If
            Next


            myMSRCCollection = New ArrayList
            For Each SR In TmpSRList
                Dim MSRC As New MySRRouteCollection
                CopyArrayCollection(MSRC.Components, SR.ComponentList)
                If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
                If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
                If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
                If Not MSRC.EntrySignalCollection.Contains(SR.EntrySignal) Then MSRC.EntrySignalCollection.Add(SR.EntrySignal)
                MSRC.MySR = SR
                MSRC.MySRDirection = EntryDir
                NextComp = FindCompFromGUID(SR.ExitSignal.RightLink)
                'If NextComp IsNot Nothing Then
                '    If Not NextComp.GetType.ToString = gSignalComponentName Then
                '        NextComp = FindCompFromGUID(SR.ExitSignal.LeftLink)
                '    End If
                'End If

                'If EntryCP Is ExitCP Then MSRC.ExitCPFound = True
                If SR.ExitSignal.myControlPoint Is ExitCP Then MSRC.ExitCPFound = True
                If SR.HasFlipFlop Then

                End If
                Dim CompList As New ArrayList
                If NextComp IsNot Nothing Then
                    If NextComp.myControlPoint Is EntryCP Then
                    Else
                        If Not MSRC.CPCollection.Contains(NextComp.myControlPoint) Then MSRC.CPCollection.Add(NextComp.myControlPoint)
                    End If
                    NextComp = FindCompFromGUID(NextComp.LeftLink)
                    If Not MSRC.Components.Contains(NextComp) Then MSRC.Components.Add(NextComp)

                    If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName AndAlso Not NextComp.AutomaticSignal Then
                        If EntryDir = GetSignalDirection(NextComp, SR.HasFlipFlop) Then
                            If Not MSRC.EntrySignalCollection.Contains(NextComp) Then MSRC.EntrySignalCollection.Add(NextComp)
                        Else
                            If Not MSRC.ExitSignalCollection.Contains(NextComp) Then MSRC.ExitSignalCollection.Add(NextComp)
                            If NextComp Is ExitSignal Then
                                MSRC.HasExitSignal = True
                            End If
                        End If
                    End If
                End If
                Dim FlipIt As Boolean = False
                Dim tmpComp As Object = Nothing
                For Each tmpComp In SR.ComponentList
                    If tmpComp.GetType.ToString = gFlipFlopComponentName Then
                        FlipIt = True
                    End If
                Next



                For i As Integer = 0 To EntryControlPointCount

                    If NextComp Is Nothing Then Exit For
                    If Not MSRC.Components.Contains(NextComp) Then MSRC.Components.Add(NextComp)
                    Select Case NextComp.GetType.ToString
                        Case gRRXINGComponentName
                            Dim ExitGUID As Integer
                            Dim RR As RRXING = TryCast(NextComp, RRXING)
                            ExitGUID = RR.FindExitGUID(tmpLastTrack.GUID)
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(ExitGUID)


                        Case gFlipFlopComponentName

                            Dim ExitGUID As Integer
                            TMDS.SystemUtilities.FetchData.DoFlipSearch(tmpLastTrack, tmpDirection, NextComp, ExitGUID)
                            NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(ExitGUID)
                            If FlipIt Then
                                FlipIt = False
                            Else
                                FlipIt = True
                            End If
                        Case gTrackComponentName
                            tmpLastTrack = NextComp
                            If NextComp.myControlPoint.GUID = ExitCP.GUID Then
                                MSRC.ExitCPFound = True
                            ElseIf NextComp.myControlPoint.GUID <> EntryCP.GUID Then
                                If MSRC.ExitCPFound Then Exit For
                                If NextComp.myControlPoint.GUID = ExitCP.GUID Then MSRC.ExitCPFound = True
                                If Not MSRC.CPCollection.Contains(NextComp.myControlPoint) Then MSRC.CPCollection.Add(NextComp.myControlPoint)
                            End If
                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            End If
                        Case gSignalComponentName
                            If Not NextComp.AutomaticSignal Then
                                ' If EntryDir = GetSignalDirection(NextComp, MSRC.MySR.HasFlipFlop) Then
                                If EntryDir = GetSignalDirection(NextComp, SR.HasFlipFlop) Then
                                    If Not MSRC.EntrySignalCollection.Contains(NextComp) Then MSRC.EntrySignalCollection.Add(NextComp)
                                    If optEnableSpecificOptionsForFECR Then
                                        Dim tmpSR As SignalRoute
                                        Dim tmpSrList1 As New ArrayList
                                        'NextComp.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))
                                        For Each tmpSR In NextComp.myControlPoint.myLeftSignalRoutes
                                            If tmpSR.EntrySignal.GUID = NextComp.GUID Then
                                                Dim SRFound As Boolean = False
                                                If tmpSR.ExitTrack IsNot Nothing AndAlso ExitTrack IsNot Nothing AndAlso tmpSR.ExitTrack.GUID = ExitTrack.GUID Then
                                                    SRFound = True
                                                ElseIf tmpSR.myExitTracks IsNot Nothing AndAlso ExitTrack IsNot Nothing AndAlso tmpSR.myExitTracks.Count > 0 AndAlso tmpSR.myExitTracks.Contains(ExitTrack) Then
                                                    SRFound = True
                                                End If
                                                If SRFound Then
                                                    If DoSRConflictCheck(SR.ComponentList) Then
                                                        Dim tcmp As Object = Nothing
                                                        MSRC.ExitCPFound = True
                                                        If Not MSRC.EntrySignalCollection.Contains(tmpSR.EntrySignal) Then MSRC.EntrySignalCollection.Add(tmpSR.EntrySignal)
                                                        If Not MSRC.Components.Contains(tmpSR.EntrySignal) Then MSRC.Components.Add(tmpSR.EntrySignal)
                                                        For Each tcmp In tmpSR.ComponentList
                                                            If Not MSRC.Components.Contains(tcmp) Then MSRC.Components.Add(tcmp)
                                                        Next
                                                        If Not MSRC.Components.Contains(tmpSR.ExitSignal) Then MSRC.Components.Add(tmpSR.ExitSignal)
                                                        MSRC.HasExitSignal = True
                                                        myMSRCCollection.Add(MSRC)
                                                        Exit Sub
                                                    End If

                                                End If
                                            End If
                                        Next
                                    End If
                                    ' EntryCP.myControlPoint.myLeftSignalRoutes.Sort(Function(x, y) x.myPriority.CompareTo(y.myPriority))


                                Else
                                    If Not MSRC.ExitSignalCollection.Contains(NextComp) Then MSRC.ExitSignalCollection.Add(NextComp)
                                    If NextComp Is ExitSignal Then
                                        MSRC.HasExitSignal = True
                                        Exit For
                                    End If
                                End If
                            End If

                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            End If
                        Case Else

                            If FlipIt Then
                                NextComp = FindCompFromGUID(NextComp.RightLink)
                            Else
                                NextComp = FindCompFromGUID(NextComp.LeftLink)
                            End If

                    End Select
                Next
                myMSRCCollection.Add(MSRC)
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Sub

    Private Function SearchFromSelectedRoute(ExitSignal As Signal) As ArrayList
        Dim Sg As Signal = Nothing
        Dim MasterMSRC As MySRRouteCollection = Nothing
        Try
            MasterMSRC = TryCast(gSelectedSignalRouteList(0), MySRRouteCollection)
            If MasterMSRC IsNot Nothing Then
                Dim EntrySignal As Signal = FindTheLastSignalInCollection(MasterMSRC.Components, MasterMSRC.MySRDirection)
                If EntrySignal Is Nothing Then
                    EntrySignal = FindTheLastSignalInCollection(MasterMSRC.Components, MasterMSRC.MySRDirection)
                End If
                Dim EntrySignalDirection As String = ""
                Dim ConfirmedFirstRoute As SignalRoute = Nothing
                If EntrySignal IsNot Nothing AndAlso ExitSignal IsNot Nothing Then
                    Dim temLis As New ArrayList
                    Dim MSRC As MySRRouteCollection
                    temLis = FindSignalRoute(EntrySignal, ExitSignal) ', enterTrack, exitTrack
                    If temLis IsNot Nothing AndAlso temLis.Count > 0 Then
                        For Each MSRC In temLis
                            If MSRC.HasExitSignal Then
                                If gSelectedSignalRouteList Is Nothing Then gSelectedSignalRouteList = New ArrayList
                                If gSelectedSignalRouteList.Contains(MSRC) Then gSelectedSignalRouteList.Add(MSRC)
                                Return MSRC.Components
                            End If
                        Next
                        Dim tmpMSRC As MySRRouteCollection = Nothing
                        For Each MSRC In temLis
                            tmpMSRC = FindRouteThrouSignals(MSRC, ExitSignal)
                            If tmpMSRC IsNot Nothing Then
                                CopyArrayCollection(MasterMSRC.Components, tmpMSRC.Components)
                                CopyArrayCollection(MasterMSRC.EntrySignalCollection, tmpMSRC.EntrySignalCollection)
                                CopyArrayCollection(MasterMSRC.ExitSignalCollection, tmpMSRC.ExitSignalCollection)
                                MSRC.HasExitSignal = True
                                gSelectedSignalRouteList = New ArrayList
                                If gSelectedSignalRouteList.Contains(MSRC) Then gSelectedSignalRouteList.Add(MSRC)
                                Return tmpMSRC.Components
                            End If
                        Next
                    End If
                End If

            End If
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Private Function FindEntryControlPointCount(ByVal EntryCP As ControlPoint) As Integer
        Dim CP As ControlPoint
        Dim Count As Integer = 0
        Try
            Dim TerritoryID As Integer = EntryCP.myTerritory.TerritoryID
            For Each CP In TMDS.GlobalDeclarations.gControlPointCollection
                If CP.myTerritory.TerritoryID = TerritoryID Then
                    Count = Count + 1
                End If
            Next
        Catch ex As Exception

        End Try
        Return Count
    End Function

    Private Function FindTheLastSignalInCollection(CompList As ArrayList, Direction As String) As Signal
        Dim Comp As Object = Nothing
        Try
            CompList.Reverse()
            For Each Comp In CompList
                If Comp.GetType.ToString = gSignalComponentName AndAlso GetSignalDirection(Comp) = Direction Then
                    Return Comp
                End If
            Next
        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
        Return Nothing
    End Function

    Private Function CleanComps(ByRef CompList As ArrayList, ByVal EntrySignal As Signal, ExitSignal As Signal, ByVal Direction As String) As Boolean
        Dim tmpLastComp As Object = Nothing
        Dim NextComp As Object = Nothing
        Dim FLipIt As Boolean = False
        Dim Dir As String = ""
        Dim tmpList As New ArrayList
        Dim tmpCompList As New ArrayList
        Dim CompToRearrage As Object = Nothing
        Dim RightComp As Object = Nothing
        Dim EntrySignalCollection As New ArrayList
        Try

            'Dir = GetSignalDirection(EntrySignal)
            Dir = Direction



            For Each NextComp In CompList
                Select Case NextComp.GetType.ToString
                    Case gFlipFlopComponentName
                        If FLipIt Then
                            FLipIt = False
                        Else
                            FLipIt = True
                        End If

                        If Dir = "RIGHT" Then
                            Dir = "LEFT"
                        Else
                            Dir = "RIGHT"
                        End If
                    Case gSignalComponentName
                        If NextComp IsNot EntrySignal Then
                            If GetSignalDirection(EntrySignal) = GetSignalDirection(NextComp, FLipIt) Then
                                If Not EntrySignalCollection.Contains(NextComp) Then EntrySignalCollection.Add(NextComp)
                            End If
                        End If
                End Select
            Next



            'If CompList(CompList.Count - 1).GetType.ToString = gSignalComponentName Then
            '    Sig = CompList(CompList.Count - 1)
            '    If GetSignalDirection(EntrySignal) = GetSignalDirection(Sig, FLipIt) Then
            '        CompList.Remove(Sig)
            '    End If

            '    'If CompList(CompList.Count - 2).GetType.ToString = gSignalComponentName Then
            '    '    Sig = CompList(CompList.Count - 2)
            '    '    If GetSignalDirection(EntrySignal) = GetSignalDirection(Sig, FLipIt) Then
            '    '        CompList.Remove(Sig)
            '    '    End If
            '    'End If
            'End If



            tmpList = New ArrayList
            Dim ExitSigFound As Boolean = False
            Select Case Dir 'Direction
                Case "RIGHT"
                    NextComp = FindCompFromGUID(ExitSignal.LeftLink)
                    For i As Integer = 0 To 10
                        If NextComp IsNot Nothing Then
                            Select Case NextComp.GetType.ToString
                                Case gSignalComponentName
                                    If NextComp Is ExitSignal Then
                                        ExitSigFound = True
                                    ElseIf GetSignalDirection(EntrySignal) = Dir Then
                                        'Exit For
                                    End If
                                Case gTrackComponentName
                                    If ExitSigFound AndAlso CompList.Contains(NextComp) Then
                                        tmpList.Add(NextComp)
                                    End If
                            End Select
                            NextComp = FindCompFromGUID(NextComp.RightLink)
                        Else
                            Exit For
                        End If
                    Next
                Case Else
                    NextComp = FindCompFromGUID(ExitSignal.RightLink)
                    For i As Integer = 0 To 10
                        If NextComp IsNot Nothing Then
                            Select Case NextComp.GetType.ToString
                                Case gSignalComponentName
                                    If NextComp Is ExitSignal Then
                                        ExitSigFound = True
                                    ElseIf GetSignalDirection(EntrySignal) = Dir Then
                                        'Exit For
                                    End If
                                Case gTrackComponentName
                                    If ExitSigFound AndAlso CompList.Contains(NextComp) Then
                                        tmpList.Add(NextComp)
                                    End If

                            End Select
                            NextComp = FindCompFromGUID(NextComp.leftLink)
                        End If
                    Next
            End Select

            If tmpList IsNot Nothing AndAlso tmpList.Count > 0 Then
                For Each NextComp In tmpList
                    If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName Then
                        If Dir = GetSignalDirection(NextComp, FLipIt) Then
                            CompList.Remove(NextComp)
                            'Exit For
                        End If
                    Else
                        CompList.Remove(NextComp)
                    End If
                Next
            End If

            Dim Sig As Signal = Nothing
            For Each Sig In EntrySignalCollection
                Select Case GetSignalDirection(Sig)
                    Case "RIGHT"
                        tmpLastComp = FindCompFromGUID(Sig.RightLink)
                        If tmpLastComp IsNot Nothing AndAlso tmpLastComp.GetType.ToString = gSignalComponentName Then
                            tmpLastComp = FindCompFromGUID(tmpLastComp.RightLink)
                        End If
                    Case "LEFT"
                        tmpLastComp = FindCompFromGUID(Sig.LeftLink)
                        If tmpLastComp IsNot Nothing AndAlso tmpLastComp.GetType.ToString = gSignalComponentName Then
                            tmpLastComp = FindCompFromGUID(tmpLastComp.LeftLink)
                        End If
                End Select
                If tmpLastComp IsNot Nothing AndAlso Not CompList.Contains(tmpLastComp) Then
                    CompList.Remove(Sig)
                End If
            Next

            tmpList = New ArrayList
            For Each NextComp In CompList
                If NextComp IsNot Nothing AndAlso NextComp.GetType.ToString = gSignalComponentName AndAlso NextComp IsNot EntrySignal Then
                    If Dir = GetSignalDirection(NextComp, FLipIt) Then
                        If Not tmpList.Contains(NextComp) Then tmpList.Add(NextComp)
                    End If
                End If
            Next



            'CopyArrayCollection(tmpCompList, CompList)
            For Each NextComp In tmpList
                If NextComp IsNot Nothing Then
                    CompToRearrage = Nothing
                    RightComp = Nothing
                    Select Case GetSignalDirection(NextComp)
                        Case "RIGHT"
                            CompToRearrage = FindCompFromGUID(NextComp.RightLink)
                        Case "LEFT"
                            CompToRearrage = FindCompFromGUID(NextComp.LeftLink)
                    End Select
                    If CompList.Contains(CompToRearrage) Then
                        CompList.Remove(CompToRearrage)
                        Dim Ind As Integer = CompList.IndexOf(NextComp)
                        CompList.Insert(Ind + 1, CompToRearrage)
                    End If
                End If
            Next
            For Each NextComp In CompList
                If NextComp IsNot Nothing Then
                    SystemEvent("CLEANUP FAST ROUTE FINAL COLLECTION>>>> " & NextComp.GetType.ToString & " GUID: " & NextComp.GUID)
                End If
            Next




        Catch ex As Exception
            GetErrorDetails(ex)
        End Try
    End Function

    Private Class MySRRouteCollection
        Public MySR As SignalRoute
        Public CPCollection As New ArrayList
        Public ExitCPFound As Boolean
        Public HasExitSignal As Boolean
        Public Components As New ArrayList
        Public EntrySignalCollection As New ArrayList
        Public ExitSignalCollection As New ArrayList
        Public MySRDirection As String = ""
    End Class


    Private Function CreateTmpSRFromCompCollection(CompList As ArrayList, EntrySignal As Signal, ExitSignal As Signal) As SignalRoute
        'Dim SR As New SignalRoute(
        'Try
        '    Dim MSRC As New MySRRouteCollection
        '    CopyArrayCollection(MSRC.Components, CompList)

        '    If Not MSRC.Components.Contains(SR.EntryTrack) Then MSRC.Components.Insert(0, SR.EntryTrack)
        '    If Not MSRC.Components.Contains(SR.ExitSignal) Then MSRC.Components.Add(SR.ExitSignal)
        '    If Not MSRC.Components.Contains(SR.ExitTrack) Then MSRC.Components.Add(SR.ExitTrack)
        '    If SR.myExitTracks IsNot Nothing AndAlso SR.myExitTracks.Count > 0 Then
        '        Dim Comp As Object
        '        For Each Comp In SR.myExitTracks
        '            If Not MSRC.Components.Contains(Comp) Then MSRC.Components.Add(Comp)
        '        Next
        '    End If
        '    MSRC.MySR = SR
        '    MSRC.MySRDirection = EntryDir
        '    MSRC.HasExitSignal = True
        '    MSRC.CPCollection.Add(EntryCP)
        '    MSRC.ExitCPFound = True
        '    myMSRCCollection.Add(MSRC)
        'Catch ex As Exception

        'End Try
        Return Nothing
    End Function


End Class
