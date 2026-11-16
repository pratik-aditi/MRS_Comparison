Option Explicit On
Option Strict On

Imports System.Reflection.MethodBase
Imports System.Text
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities.FetchData
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackComponents
Imports TMDS.TrackOtherComponents
Imports TMDS.TrackTagComponents

Namespace TrainComponents

    Partial Public Class TrainID

#Region "ISSUE LICENSE"

        ''' <summary>
        ''' Generate normal license over track
        ''' </summary>
        ''' <param name="theTrack">track assigned</param>
        ''' <param name="componentList">component List</param>
        Public Sub PtcGenerateNormalLicenseOverTrack(theTrack As Track, componentList As ArrayList)
            Try
                TrainEventLog($"{GetCurrentMethod.Name}:Track={theTrack.GUID}:{theTrack.Name}")
                Dim foundDifferentNames As Boolean = False
                For Each oneTrack As Track In componentList
                    If Not oneTrack.Name.Equals(theTrack.Name) Then
                        foundDifferentNames = True
                        Exit For
                    End If
                Next
                If foundDifferentNames Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Found double left click to end track")
                    Dim messageList As New ArrayList From {
                        $"{optSystemAlertMessage12} {Name}",
                        optSystemAlertMessage13
                    }
                    ShowCustomMessageBox(messageList, optSystemMessageString3,
                                         False, myTrack.CurrentDrawingForm.myLaunchPoint,
                                         False, True, myTrack.CurrentDrawingForm)
                    Exit Sub
                End If
                If optEnableStoredLicenseForFreeArea AndAlso
                    SLFAEnabled Then
                    TrainEventLog($"{GetCurrentMethod.Name}:SLFA:Enter and exit must be different")
                    Dim messageList As New ArrayList From {
                        $"{optSystemAlertMessage14} {Name}",
                        optSystemAlertMessage15
                    }
                    ShowCustomMessageBox(messageList, optSystemAlertMessage18,
                                         False, myTrack.CurrentDrawingForm.myLaunchPoint,
                                         False, True, myTrack.CurrentDrawingForm)
                    Exit Sub
                End If
                If theTrack.TypeOfPTC.Contains("ENTRY") Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Enter and exit must be different to entry track")
                    Dim messageList As New ArrayList From {
                        $"{optSystemAlertMessage14} {Name} {optSystemAlertMessage16}",
                        optSystemAlertMessage17
                    }
                    ShowCustomMessageBox(messageList, optSystemMessageString3,
                                         False, myTrack.CurrentDrawingForm.myLaunchPoint,
                                         False, True, myTrack.CurrentDrawingForm)
                    Exit Sub
                End If
                PtcGenerateNormalLicense(Nothing, False, False, GetCurrentMethod.Name)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Remove Pending Route Behind Train
        ''' Note: Workaround for pending rollup issue
        ''' </summary>
        Private Sub PtcRemovePendingRouteBehindTrain()
            Try
                'Just for modify license and routes bigger than 2
                If thisCurrentLicense IsNot Nothing AndAlso
                    myTacticalRoutes.Count > 1 Then
                    Dim newList As List(Of TacticalRoute) = myTacticalRoutes.OfType(Of TacticalRoute).ToList
                    If newList(0).myStatus.Equals("PENDING") AndAlso
                        newList(1).myStatus.Equals("FULLFILLED") Then
                        TrainEventLog($"{GetCurrentMethod.Name _
                                      }:RemovePendingRoute=:{newList(0).mySignalRoute.EntrySignal.Name _
                                      }:{newList(0).myStatus}")
                        myTacticalRoutes.Remove(newList(0))
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Generate license for PTC
        ''' </summary>
        ''' <param name="signalList">signal list</param>
        ''' <param name="removeJointSigFromList">remove Joint Sig From List</param>
        ''' <param name="requestJointAuthSignal">request Joint Auth Signal</param>
        ''' <param name="whoCallMe">who Call Me</param>
        Public Sub PtcGenerateNormalLicense(signalList As ArrayList,
                                         Optional removeJointSigFromList As Boolean = False,
                                         Optional requestJointAuthSignal As Boolean = False,
                                         Optional whoCallMe As String = Nothing)
            Try
                TrainEventLog($"{GetCurrentMethod.Name _
                              }:Start:RemoveJointSigFromList={removeJointSigFromList _
                              }:RequestJointAuthSignal={requestJointAuthSignal _
                              }:Direction={Direction _
                              }:WhoCallMe={whoCallMe}")
                If optEnableStoredLicenseForFreeArea AndAlso
                    SLFAEnabled Then
                    TrainEventLog($"{GetCurrentMethod.Name}:SLFAEnabled")
                    Exit Sub
                End If
                If Not IsActivePTC Then
                    TrainEventLog($"{GetCurrentMethod.Name}:IsActivePTC")
                    Exit Sub
                End If
                If IsInTrackReleaseMode Then
                    TrainEventLog($"{GetCurrentMethod.Name}:InTrackReleasMode")
                    Exit Sub
                End If
                If signalList IsNot Nothing AndAlso
                    signalList.Count = 0 Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Signal list is empty")
                    Exit Sub
                End If
                IsBIDirectional = (UserActionType = UserActionTypeEnum.SpecialLicenseOption6 OrElse
                    UserActionType = UserActionTypeEnum.SpecialLicenseOption7)
                ClickedOnTrainForLicense = True
                For Each oneTacticalRoute As TacticalRoute In myTacticalRoutesFullfilled
                    TrainEventLog($"{GetCurrentMethod.Name _
                                  }:TacticalRoute={oneTacticalRoute.mySignalRoute.EntrySignal.GUID _
                                  }:{oneTacticalRoute.mySignalRoute.EntrySignal.Name _
                                  }:{oneTacticalRoute.myStatus}")
                    If signalList IsNot Nothing AndAlso
                        signalList.Contains(oneTacticalRoute.mySignalRoute.EntrySignal) Then
                        signalList.Remove(oneTacticalRoute.mySignalRoute.EntrySignal)
                        TrainEventLog($"{GetCurrentMethod.Name}:Remove from signal list")
                    End If
                    If myTacticalRoutes.Contains(oneTacticalRoute) Then
                        myTacticalRoutes.Remove(oneTacticalRoute)
                        TrainEventLog($"{GetCurrentMethod.Name}:Remove from tactical routes")
                    End If
                Next
                For Each oneComponent As Object In SignalListForRemoval
                    If signalList.Contains(oneComponent) Then
                        signalList.Remove(oneComponent)
                    End If
                Next
                Dim isOnlyThisTrackForLicense As Boolean
                Dim joinSignal As Signal = Nothing
                Dim authLimitList As ArrayList
                Dim signalRouteFrontOfTrain As SignalRoute
                Dim routeList As New ArrayList
                If signalList IsNot Nothing AndAlso
                    signalList.Count > 0 Then
                    'Remove pending routes behind fullfill routes in modify licenses
                    PtcRemovePendingRouteBehindTrain()
                    'Check tactical routes
                    Dim fullfilledTacticalRoute As New ArrayList
                    For Each oneTacticalRoute As TacticalRoute In myTacticalRoutes
                        TrainEventLog($"{GetCurrentMethod.Name _
                                      }:TacticalRoutes={oneTacticalRoute.mySignalRoute.EntrySignal.GUID _
                                      }:{oneTacticalRoute.mySignalRoute.EntrySignal.Name _
                                      }:{oneTacticalRoute.myStatus}")
                        'Add routes pending or cleared in route to send in license
                        If oneTacticalRoute.myStatus <> "FULLFILLED" AndAlso
                                oneTacticalRoute.myStatus <> "STORED ROUTE" AndAlso
                                oneTacticalRoute.myStatus <> "CANCELLING" Then
                            TrainEventLog($"{GetCurrentMethod.Name}:Added in route list")
                            routeList.Add(oneTacticalRoute.mySignalRoute)
                        Else 'Ignore routes in license
                            If oneTacticalRoute.myStatus = "FULLFILLED" OrElse
                                    oneTacticalRoute.myStatus = "CANCELLING" Then
                                TrainEventLog($"{GetCurrentMethod.Name}:Added in full filled list")
                                fullfilledTacticalRoute.Add(oneTacticalRoute)
                            End If
                            If Not myTacticalRoutesFullfilled.Contains(oneTacticalRoute) Then
                                TrainEventLog($"{GetCurrentMethod.Name}:Added in my full filled list")
                                myTacticalRoutesFullfilled.Add(oneTacticalRoute)
                            End If
                        End If
                    Next
                    'Update routes to train object
                    For Each oneTacticalRoute As TacticalRoute In fullfilledTacticalRoute
                        If myTacticalRoutes.Contains(oneTacticalRoute) Then
                            TrainEventLog($"{GetCurrentMethod.Name _
                                      }:RemoveTacticalRoutes={oneTacticalRoute.mySignalRoute.EntrySignal.GUID _
                                      }:{oneTacticalRoute.mySignalRoute.EntrySignal.Name _
                                      }:{oneTacticalRoute.myStatus}")
                            myTacticalRoutes.Remove(oneTacticalRoute)
                            If Not myTacticalRoutesFullfilled.Contains(oneTacticalRoute) Then
                                TrainEventLog($"{GetCurrentMethod.Name}:Added in my full filled list")
                                myTacticalRoutesFullfilled.Add(oneTacticalRoute)
                            End If
                            SignalListForRemoval.Add(oneTacticalRoute.mySignalRoute.EntrySignal)
                        End If
                    Next
                    For Each oneComponent As Object In SignalListForRemoval
                        If signalList.Contains(oneComponent) Then
                            TrainEventLog($"{GetCurrentMethod.Name}:Remove from signal list")
                            signalList.Remove(oneComponent)
                        End If
                    Next
                    For Each oneComponent As Object In signalList
                        Select Case oneComponent.GetType.ToString
                            Case gSignalComponentName
                                Dim signal As Signal = CType(oneComponent, Signal)
                                'Need to avoid the license issues in join auth area
                                If signal.gJointLicenseForTrainTrain IsNot Nothing Then
                                    If Not signal.gJointLicenseForTrainTrain.Name.Equals(Name) Then
                                        TrainEventLog($"{GetCurrentMethod.Name _
                                                      }:TrainIDinJoinAuth={signal.gJointLicenseForTrainTrain.Name}")
                                        If myTrack.myControlPoint.UniqueName <> signal.gJointLicenseForTrainTrain.myTrack.myControlPoint.UniqueName Then
                                            TrainEventLog($"{GetCurrentMethod.Name _
                                                          }:TrainIDControlPoint={myTrack.myControlPoint.UniqueName _
                                                          }:TrainIDinJoinAuthControlPoint={signal.gJointLicenseForTrainTrain.myTrack.myControlPoint.UniqueName}")
                                            Continue For
                                        End If
                                    End If
                                End If
                                If requestJointAuthSignal Then
                                    If signal.JointControlAuthorizationEnabled AndAlso
                                        Not RequestJointAuthForLicnese AndAlso
                                        Not signal.RequestJointAuthForLicenseClick Then
                                        joinSignal = signal
                                        signal.SendJoinAuthorizationRequest(Me)
                                        signal.gJointLicenseForTrainTrain = Me
                                        RequestJointAuthForLicnese = True
                                        WaitingForJointAuthResponse = True
                                    ElseIf signal.RequestJointAuthForLicenseClick Then
                                        signal.gJointLicenseForTrainTrain = Me
                                    End If
                                End If
                                Dim theSignalRoute As SignalRoute = GetSignalRoute(signal)
                                If theSignalRoute IsNot Nothing AndAlso
                                    (theSignalRoute.IsControlledForPTC OrElse
                                    GeoFenceLegacyEnabled) Then
                                    If Not routeList.Contains(theSignalRoute) Then
                                        routeList.Add(theSignalRoute)
                                        TrainEventLog($"{GetCurrentMethod.Name _
                                                      }:Signal route added for signal {signal.Name}")
                                    Else
                                        TrainEventLog($"{GetCurrentMethod.Name _
                                                      }:Signal route not added for signal {signal.Name}")
                                    End If
                                ElseIf routeList.Count = 0 AndAlso
                                    CBTCEquipped Then
                                    TrainEventLog($"{GetCurrentMethod.Name _
                                                  }:Signal route not added for signal {signal.Name}")
                                    Exit Sub
                                End If
                        End Select
                    Next
                    authLimitList = GetLicenseVerbiageAbbreviated(routeList)
                Else
                    TrainEventLog($"{GetCurrentMethod.Name}:IsOnlyThisTrackForLicense")
                    isOnlyThisTrackForLicense = True
                    signalRouteFrontOfTrain = GetSignalRouteInFrontOfTrack(myTrack)
                    If signalRouteFrontOfTrain Is Nothing Then Exit Sub
                    routeList.Add(signalRouteFrontOfTrain)
                    authLimitList = GetLicenseVerbiageAbbreviated(Nothing)
                End If
                Dim firstRoute As SignalRoute = DirectCast(routeList.Item(0), SignalRoute)
                Dim compList As New ArrayList
                If thisCurrentLicense Is Nothing Then 'New license
                    TrainEventLog($"{GetCurrentMethod.Name}:Add component under train")
                    AddCompTrainPositionFirstRoute(firstRoute, compList)
                Else 'Modify license
                    For Each oneComponent As GenericComponent In thisCurrentLicense.ComponentsInSelection
                        TrainEventLog($"{GetCurrentMethod.Name}:Add previus components:Name={oneComponent.Name}")
                        compList.Add(oneComponent)
                    Next
                    If IsSpecialLicense AndAlso
                        thisRestrictiveLicense IsNot Nothing Then
                        TrainEventLog($"{GetCurrentMethod.Name}:Add component under train")
                        AddCompTrainPositionFirstRoute(firstRoute, compList)
                    End If
                End If
                Dim allRoutesStored As Boolean
                For Each oneTacticalRoute As TacticalRoute In myTacticalRoutes
                    If oneTacticalRoute IsNot Nothing Then
                        If oneTacticalRoute.myStatus <> "STORED ROUTE" Then
                            allRoutesStored = False
                            Exit For
                        Else
                            allRoutesStored = True
                        End If
                    End If
                Next
                If removeJointSigFromList Then
                    If signalList.Contains(joinSignal) Then
                        signalList.Remove(joinSignal)
                    End If
                End If
                Dim foundEntryTrackForMarking As Boolean
                If Not isOnlyThisTrackForLicense Then
                    For Each oneRoute As SignalRoute In routeList
                        If oneRoute.IsControlledForPTC OrElse
                            GeoFenceLegacyEnabled Then
                            For Each oneComponent As GenericComponent In oneRoute.ComponentList
                                If Not compList.Contains(oneComponent) Then
                                    If TypeOf oneComponent Is Track Then
                                        Dim oneTrack As Track = CType(oneComponent, Track)
                                        If oneTrack.IsControlledForPTC OrElse
                                            GeoFenceLegacyEnabled OrElse
                                            (IsTestCBTC AndAlso
                                            (oneTrack.IsTestForPTC OrElse
                                            gSimulationMode OrElse
                                            oneTrack.IsControlledForPTC)) Then
                                            If RemoveListForRollUps IsNot Nothing Then
                                                'Avoiding add tracks behind the train
                                                If Not RemoveListForRollUps.Contains(oneComponent) Then
                                                    TrainEventLog($"{GetCurrentMethod.Name _
                                                      }:AddTrack1={oneTrack.GUID}:{oneTrack.Name}")
                                                    compList.Add(oneComponent)
                                                End If
                                            Else
                                                TrainEventLog($"{GetCurrentMethod.Name _
                                                      }:AddTrack2={oneTrack.GUID}:{oneTrack.Name}")
                                                compList.Add(oneComponent)
                                            End If
                                        ElseIf oneTrack.IsEntryForPTC Then
                                            foundEntryTrackForMarking = True
                                        End If
                                    ElseIf TypeOf oneComponent Is Switch Then
                                        Dim oneSwitch As Switch = CType(oneComponent, Switch)
                                        If Not compList.Contains(oneSwitch.NormalTrackTrailingPortion) Then
                                            If oneSwitch.NormalTrackTrailingPortion.IsControlledForPTC OrElse
                                                oneSwitch.NormalTrackTrailingPortion.IsTestForPTC Then
                                                TrainEventLog($"{GetCurrentMethod.Name _
                                                          }:AddSwitch={oneSwitch.GUID}:{oneSwitch.Name}")
                                                compList.Add(oneSwitch)
                                                compList.Add(oneSwitch.NormalTrackTrailingPortion)
                                            End If
                                        End If
                                    ElseIf TypeOf oneComponent Is FlipFlop Then
                                        TrainEventLog($"{GetCurrentMethod.Name _
                                                          }:AddFliFlop={oneComponent.GUID}:{oneComponent.Name}")
                                        compList.Add(oneComponent)
                                    End If
                                End If
                            Next
                            ExtendRouteToNextSignal(oneRoute, compList)
                        End If
                    Next
                End If
                If foundEntryTrackForMarking Then
                    FoundEntryTrackForMRS = True
                Else
                    FoundEntryTrackForMRS = False
                End If
                Application.DoEvents()
                'If doenst exist remote track is need to change the status to Local
                If optEnableExtendLicenseinSplitDesk AndAlso
                    thisCurrentLicense IsNot Nothing AndAlso
                    TrainRegionStatus = TrainRegionEnum.InTrasition AndAlso
                    Not thisCurrentLicense.CheckIfExistRemoteTrack(thisCurrentLicense.ComponentsInSelection) Then
                    TrainRegionStatus = TrainRegionEnum.Local
                    TrainEventLog($"{GetCurrentMethod.Name}:TrainRegionStatus=Local")
                End If
                'Local segments
                Dim trackSegmentsListCurrentDesk As New ArrayList
                Dim strTrackSegmentsCurrentDesk As New StringBuilder
                GetTrackSegmentsForCAD(firstRoute, compList,
                                       trackSegmentsListCurrentDesk,
                                       False, GetCurrentMethod.Name)
                For Each trackSeg As String In trackSegmentsListCurrentDesk
                    TrainEventLog($"{GetCurrentMethod.Name}:SegmentCurrentDesk={trackSeg}")
                    If strTrackSegmentsCurrentDesk.Length = 0 Then
                        strTrackSegmentsCurrentDesk.Append(trackSeg)
                    Else
                        strTrackSegmentsCurrentDesk.Append("|")
                        strTrackSegmentsCurrentDesk.Append(trackSeg)
                    End If
                Next
                DoLogTrainPositionReportFromGPS()
                Dim beginTrack As Track = firstRoute.EntryTrack
                If thisCurrentLicense IsNot Nothing AndAlso
                    Not CheckIfTNTExistsInDB(thisCurrentLicense.AuthGUID) Then
                    thisCurrentLicense = Nothing
                End If
                Dim authorityExists As Boolean
                Dim theAuthority As TrackAuthority.TrackAuthority
                Dim foundFlipFlop As Boolean
                'CREATE
                If thisCurrentLicense Is Nothing Then
                    TrainEventLog($"{GetCurrentMethod.Name}:New license")
                    theAuthority = New TrackAuthority.TrackAuthority
                    With theAuthority
                        .IssuedToTrain = True
                        .Direction = firstRoute.myDirection
                        .myTrain = Me
                        .TrainSymbol = Name
                        .LicenseTypeAuthority = True
                    End With
                    If IsBIDirectional Then theAuthority.BiDirectional = True
                    If ClickedOnSpecialLicense3 Then theAuthority.UntilTime = TNTExpireDate
                Else 'MODIFY
                    TrainEventLog($"{GetCurrentMethod.Name}:License being modified")
                    theAuthority = thisCurrentLicense
                    For Each oneComponent As Object In theAuthority.ComponentsInSelection
                        If Not compList.Contains(oneComponent) Then
                            compList.Insert(theAuthority.ComponentsInSelection.IndexOf(oneComponent), oneComponent)
                            If oneComponent.GetType.ToString = gTrackComponentName Then
                                Dim oneTrack As Track = CType(oneComponent, Track)
                                If oneTrack.TrafficDirection = "" Then
                                    oneTrack.TrafficDirection = firstRoute.myDirection
                                    TrainEventLog($"{GetCurrentMethod.Name _
                                                  }:AddTrack={oneTrack.Name}:{oneTrack.TrafficDirection}")
                                End If
                            End If
                        Else
                            Exit For
                        End If
                    Next
                    For Each oneComponent As Object In compList
                        If oneComponent.GetType.ToString = gTrackComponentName Then
                            Dim oneTrack As Track = CType(oneComponent, Track)
                            If oneTrack.IsLinkToFlipFlopToLeft OrElse
                                oneTrack.IsLinkToFlipFlopToRight Then
                                If Not TrainInitAtFlipFlop AndAlso
                                    Direction = optDirectionSouth Then
                                    If firstRoute.myDirection = "RIGHT" Then
                                        TrainEventLog($"{GetCurrentMethod.Name}:Flipflop left")
                                        theAuthority.Direction = "LEFT"
                                        foundFlipFlop = True
                                        TrainInitAtFlipFlop = False
                                    End If
                                ElseIf Not TrainInitAtFlipFlop AndAlso
                                    Direction = optDirectionNorth Then
                                    If firstRoute.myDirection = "LEFT" Then
                                        TrainEventLog($"{GetCurrentMethod.Name}:Flipflop right")
                                        theAuthority.Direction = "RIGHT"
                                        foundFlipFlop = True
                                        TrainInitAtFlipFlop = False
                                    End If
                                End If
                            End If
                        ElseIf oneComponent.GetType.ToString = gFlipFlopComponentName Then
                            foundFlipFlop = True
                        End If
                    Next
                    If Not foundFlipFlop AndAlso
                        Not TrainInitAtFlipFlop Then
                        TrainEventLog($"{GetCurrentMethod.Name}:Flipflop direction {firstRoute.myDirection}")
                        If IsBIDirectional Then theAuthority.BiDirectional = True
                        theAuthority.Direction = firstRoute.myDirection
                    End If
                    If TrainInitAtFlipFlop Then TrainInitAtFlipFlop = False
                    If ClickedOnSpecialLicense3 Then theAuthority.UntilTime = TNTExpireDate
                    authorityExists = True
                End If
                Application.DoEvents()
                If theAuthority.AddLicenseToComponents(compList, beginTrack, Me, Nothing, routeList, authorityExists) Then
                    If theAuthority.AddTrackAuthorityToDB(Nothing, authorityExists) Then
                        For Each oneSignalRoute As SignalRoute In routeList
                            If Not DoesTacticalRouteExist(oneSignalRoute) AndAlso
                                Not isOnlyThisTrackForLicense Then
                                Dim theTacticalRoute As New TacticalRoute
                                Dim routeStatus As String = "PENDING"
                                If oneSignalRoute.EntrySignal.Tag IsNot Nothing AndAlso
                                    oneSignalRoute.EntrySignal.Tag.ToString = $"IGNORE FOR LICENSE:{Name}" Then
                                    TrainEventLog($"{GetCurrentMethod.Name _
                                                  }:Ignore signal due to tag:Signal={oneSignalRoute.EntrySignal.Name}")
                                    routeStatus = "FULLFILLED"
                                    oneSignalRoute.EntrySignal.Tag = String.Empty
                                ElseIf oneSignalRoute.EntrySignal.SlottingOSTrack IsNot Nothing AndAlso
                                    Not oneSignalRoute.EntrySignal.AutomaticSignal AndAlso
                                    oneSignalRoute.EntrySignal.SlottingOSTrack.Occuppied Then
                                    TrainEventLog($"{GetCurrentMethod.Name _
                                                  }:Ignore signal due to OS track occupied:Signal={oneSignalRoute.EntrySignal.Name}")
                                    routeStatus = "FULLFILLED"
                                End If
                                theTacticalRoute.CreateRoute(Me, oneSignalRoute, routeStatus)
                                myTacticalRoutes.Add(theTacticalRoute)
                                If authorityExists Then
                                    myNewTacticalRoutes.Add(theTacticalRoute)
                                    CheckToRemoveFromTacticalRemoveList(theTacticalRoute)
                                    If theTacticalRoute.mySignalRoute.EntrySignal.CurrentTacticalRoute Is Nothing Then
                                        theTacticalRoute.mySignalRoute.EntrySignal.CurrentTacticalRoute = theTacticalRoute
                                    End If
                                End If
                            Else
                                Dim oneTacticalRoute As TacticalRoute = FindTacticalRouteToExecute(oneSignalRoute)
                                If oneTacticalRoute IsNot Nothing AndAlso
                                    authorityExists Then
                                    myNewTacticalRoutes.Add(oneTacticalRoute)
                                    oneTacticalRoute.IsReadyToExecute = False
                                End If
                            End If
                        Next
                        If allRoutesStored Then
                            authorityExists = False
                        End If
                        PtcSendBulletins(compList)
                        theAuthority.UpdateTacticalRouteList()
                        theAuthority.PTCAuthorityBeingModified = authorityExists
                        theAuthority.VETMS_PerformIssuance(Me, strTrackSegmentsCurrentDesk.ToString, authLimitList, False)
                        theAuthority.NeedsElectronicTransmission = True
                        Me.thisCurrentLicense = theAuthority
                        theAuthority.ApplyTrackAuthToRemoteLinks()
                        If theAuthority.TrackAuthorityDuplicateCheck Then
                            OtherTrackAuthorities.Add(theAuthority)
                            TrainEventLog($"{GetCurrentMethod.Name}:Added Authority {theAuthority.AuthorityNumber}")
                        Else
                            TrainEventLog($"{GetCurrentMethod.Name}:Authority found {theAuthority.AuthorityNumber}")
                        End If
                    End If
                End If
                TrainEventLog($"{GetCurrentMethod.Name}:End")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


#End Region

#Region "CANCEL LICENSE"

        ''' <summary>
        ''' Cancel Normal License
        ''' </summary>
        ''' <param name="theSignal">signal to cancel</param>
        ''' <param name="overridePrompt">override prompt</param>
        Public Sub PtcCancelNormalLicense(theSignal As Signal,
                                         Optional overridePrompt As Boolean = False)
            Try
                TrainEventLog($"{GetCurrentMethod.Name _
                              }:Start:WhoCallMe={(New StackTrace).GetFrame(1).GetMethod().Name}")
                If Not overridePrompt Then
                    Dim messageList As New ArrayList()
                    With messageList
                        .Add($"{optCancelLicenseLine1} {Name}")
                        .Add(optCancelLicenseLine2)
                        .Add(String.Empty)
                    End With
                    Using frmMessageBox As New frmMessageBox()
                        frmMessageBox.Owner = SystemForms.MySystemForms.frmMainMDI
                        frmMessageBox.SetMessage(messageList, optCancelLicenseHeader,
                                                 True, myTrack.CurrentDrawingForm.myLaunchPoint)
                        frmMessageBox.ShowDialog()
                        If frmMessageBox.FormFeedBack <> 6 Then
                            TrainEventLog($"{GetCurrentMethod.Name}:User clicked in NO")
                            Exit Sub
                        End If
                    End Using
                End If
                If theSignal IsNot Nothing Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Signal={theSignal.Name}")
                    Dim routeFound As Boolean
                    For Each oneTactical As TacticalRoute In myTacticalRoutes
                        TrainEventLog($"{GetCurrentMethod.Name _
                                      }:TacticalRoute={oneTactical.mySignalRoute.EntrySignal.Name}")
                        If oneTactical.mySignalRoute.EntrySignal.GUID = theSignal.GUID Then
                            routeFound = True
                            If Not myTacticalRoutesToRemove.Contains(oneTactical) Then
                                myTacticalRoutesToRemove.Add(oneTactical)
                            End If
                        ElseIf routeFound Then
                            If Not myTacticalRoutesToRemove.Contains(oneTactical) Then
                                myTacticalRoutesToRemove.Add(oneTactical)
                            End If
                        End If
                    Next
                End If
                Dim routeList As New ArrayList
                Dim removeStoredRoutesDetected As Boolean
                For Each oneTactical As TacticalRoute In myTacticalRoutesToRemove
                    If oneTactical.myStatus.Equals("PENDING") OrElse
                        oneTactical.myStatus.Equals("CLEARED") OrElse
                        oneTactical.myStatus.Equals("FULLFILLED") Then
                        TrainEventLog($"{GetCurrentMethod.Name _
                                      }:AddTacticalRoute={oneTactical.mySignalRoute.EntrySignal.Name}")
                        routeList.Add(oneTactical.mySignalRoute)
                        oneTactical.myStatus = "CANCELLING"
                        oneTactical.UpdateRouteStatus("CANCELLING")
                        removeStoredRoutesDetected = True
                    End If
                Next
                Dim componentList As New ArrayList
                For Each oneSignal As SignalRoute In routeList
                    For Each oneComponent As Object In oneSignal.ComponentList
                        If Not componentList.Contains(oneComponent) Then
                            If oneComponent.GetType.ToString = gTrackComponentName Then
                                If Not componentList.Contains(oneComponent) Then
                                    componentList.Add(oneComponent)
                                End If
                            ElseIf oneComponent.GetType.ToString = gSwitchComponentName Then
                                If Not componentList.Contains(oneComponent) Then
                                    componentList.Add(oneComponent)
                                End If
                                Dim oneSwitch As Switch = CType(oneComponent, Switch)
                                If Not componentList.Contains(oneSwitch.NormalTrackTrailingPortion) Then
                                    componentList.Add(oneSwitch.NormalTrackTrailingPortion)
                                End If
                            ElseIf oneComponent.GetType.ToString = gFlipFlopComponentName Then
                                If Not componentList.Contains(oneComponent) Then
                                    componentList.Add(oneComponent)
                                End If
                            End If
                        End If
                    Next
                Next
                Dim removeEntireAuthority As Boolean
                Dim theAuthority As TrackAuthority.TrackAuthority = thisCurrentLicense
                theAuthority.overridePrompt = overridePrompt
                If myTacticalRoutesToRemove.Count = myTacticalRoutes.Count OrElse
                    theSignal Is Nothing Then
                    For Each oneTactical As TacticalRoute In myTacticalRoutes
                        Dim tactRouteFound As Boolean
                        For Each oneTacticalRemove As TacticalRoute In Me.myTacticalRoutesToRemove
                            If oneTactical.myGUID = oneTacticalRemove.myGUID Then
                                tactRouteFound = True
                                Exit For
                            End If
                        Next
                        If Not tactRouteFound Then
                            If Not optEnableMRSSpecificPTCOperations Then
                                removeEntireAuthority = True
                            End If
                        End If
                    Next
                End If
                If myTacticalRoutesToRemove.Count > 0 Then
                    Dim LastTactRoute As TacticalRoute =
                        DirectCast(myTacticalRoutesToRemove.Item(myTacticalRoutesToRemove.Count - 1), TacticalRoute)
                    Dim LastRoute As SignalRoute = LastTactRoute.mySignalRoute
                    ExtendRouteToNextSignal(LastRoute, componentList)
                End If
                Dim sendTheControl As Boolean
                Dim newTactList As ArrayList = DirectCast(myTacticalRoutes.Clone, ArrayList)
                For Each oneTactical As TacticalRoute In myTacticalRoutesToRemove
                    If Not oneTactical.mySignalRoute.EntrySignal.AutomaticSignal AndAlso
                        oneTactical.myStatus = "CANCELLING" Then
                        oneTactical.mySignalRoute.EntrySignal.myControlPoint.FlasherMode(True)
                        oneTactical.mySignalRoute.EntrySignal.ControlSignalToStop(False, True)
                        oneTactical.mySignalRoute.EntrySignal.CurrentTacticalRoute = Nothing
                        sendTheControl = True
                    End If
                    newTactList.Remove(oneTactical)
                Next
                Dim newRouteList As New ArrayList
                For Each oneTactical As TacticalRoute In newTactList
                    newRouteList.Add(oneTactical.mySignalRoute)
                Next
                Dim authLimitList As ArrayList = GetLicenseVerbiageAbbreviated(newRouteList)
                If sendTheControl Then
                    Dim doSendControls As New SendControls(AddressOf SystemForms.MySystemForms.frmMainMDI.SendControls)
                    doSendControls.BeginInvoke(False, Nothing, Nothing)
                End If
                'This sent a void to train when doenst exist tracks controlled in license
                Dim foundControlledTrack As Boolean = False
                For Each oneComponent As Object In theAuthority.ComponentsInSelection
                    If Not componentList.Contains(oneComponent) Then
                        If oneComponent.GetType.ToString = gTrackComponentName Then
                            Dim oneTrack As Track = DirectCast(oneComponent, Track)
                            If oneTrack.TypeOfPTC.Contains("CONTROLLED") Then
                                foundControlledTrack = True
                                Exit For
                            End If
                        End If
                    End If
                Next
                If removeEntireAuthority OrElse
                    Not foundControlledTrack Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Cancel all license for train")
                    CancelAllLicenseForPTC(False, False)
                Else
                    TrainEventLog($"{GetCurrentMethod.Name}:Partial cancel for train")
                    theAuthority.PtcPerformRollup(Me, "NONE", componentList,
                                                         True, True, authLimitList, True)
                    For Each oneTactical As TacticalRoute In myTacticalRoutesToRemove
                        If oneTactical.myStatus.Equals("STORED ROUTE") Then
                            oneTactical.RemoveRoute()
                            myTacticalRoutes.Remove(oneTactical)
                            For Each oneSignalRoute As StoredRoute In myStoredRoutes
                                If oneTactical.mySignalRoute.EntrySignal.GUID = oneSignalRoute.EntrySignal.GUID Then
                                    oneSignalRoute.RemoveMe(GetCurrentMethod.Name)
                                    myStoredRoutes.Remove(oneSignalRoute)
                                    Exit For
                                End If
                            Next
                        End If
                    Next
                    If removeStoredRoutesDetected Then RemoveAllStoredRoutes()
                End If
                If RequestJointAuthForLicnese Then RequestJointAuthForLicnese = False
                If SignalListForJointAuthRouteBuilding.Count > 0 Then
                    SignalListForJointAuthRouteBuilding.Clear()
                End If
                CheckTrackTagToMarkAsUnRead(componentList)
                ClearSystemVariables()
                ClickedOnTrainForLicense = False
                TrainEventLog($"{GetCurrentMethod.Name}:End")
            Catch ex As Exception
                SystemException(ex.ToString)
                ClearSystemVariables()
            End Try
        End Sub

#End Region

#Region "LICENSE VALIDATION"

        ''' <summary>
        ''' No Begin Track Within Authority
        ''' </summary>
        ''' <param name="enterTrack">enter track</param>
        ''' <param name="components">components to check</param>
        ''' <returns><see langword="True"/>No begin track found, <see langword="False"/> otherwise.</returns>
        Public Function NoBeginTrackWithinAuthority(enterTrack As Track,
                                                    components As ArrayList) As Boolean
            Try
                Dim blockLicense As Boolean = False
                If thisCurrentLicense Is Nothing Then
                    If Not components.Contains(myTrack) Then
                        blockLicense = True
                        TrainEventLog($"{GetCurrentMethod.Name}:1:Block license")
                    End If
                Else
                    If Not thisCurrentLicense.ComponentsInSelection.Contains(myTrack) Then
                        TrainEventLog($"{GetCurrentMethod.Name}:2:Block license")
                    End If
                    If RemoveListForRollUps.Count > 0 AndAlso
                        components.Contains(RemoveListForRollUps(RemoveListForRollUps.Count - 1)) AndAlso
                        Not components.Contains(myTrack) Then
                        blockLicense = True
                        TrainEventLog($"{GetCurrentMethod.Name}:3:Block license")
                    End If
                    If thisCurrentLicense.ComponentsInSelection.Count > 0 AndAlso
                        thisCurrentLicense.ComponentsInSelection(0) Is enterTrack AndAlso
                        Not components.Contains(myTrack) Then
                        blockLicense = True
                        TrainEventLog($"{GetCurrentMethod.Name}:4:Block license")
                    End If
                End If
                For Each track As Track In components.OfType(Of Track)
                    If track.IsLinkToFlipFlopToLeft OrElse
                        track.IsLinkToFlipFlopToRight Then
                        blockLicense = False
                        Exit For
                    End If
                Next
                Return blockLicense
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Is join integration with signal clear?
        ''' </summary>
        ''' <param name="components">list of components to check</param>
        ''' <returns><see langword="True"/>Join integration with signal clear, <see langword="False"/> otherwise.</returns>
        Public Function IsJoinIntegrationWithSignalClear(components As ArrayList) As Boolean
            Try
                For Each signal As Signal In components.OfType(Of Signal)
                    If signal.JointControlAuthorizationOfficeMode AndAlso
                        Not myTrack?.IsMarkedToJoinIntregation AndAlso
                        signal.DoesSignalRouteHaveJoinTrack(components) AndAlso
                        signal.SignalStatus <> Signal.BulbStatus.ClearStatus Then
                        TrainEventLog($"{GetCurrentMethod.Name} Join integration is not clear for signal {signal.Name}.")
                        InfoMessage($"{My.Resources.UserMessage4} {signal.Name}")
                        Return False
                    End If
                Next
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

#End Region

#Region "ROLLUP"

        ''' <summary>
        ''' Perform Partial Release
        ''' </summary>
        ''' <param name="track">track to check</param>
        Private Sub PtcPerformPartialRelease(track As Track)
            Try
                Dim removeEntireLicense As Boolean
                If thisCurrentLicense Is Nothing Then
                    BindLicenseToTrain()
                    If thisCurrentLicense Is Nothing Then
                        TrainEventLog($"{GetCurrentMethod.Name}:License not found")
                        Exit Sub
                    End If
                End If
                Dim componentsToRemove As New ArrayList
                If thisCurrentLicense.ComponentsInSelection.Contains(track) Then
                    For Each component As GenericComponent In thisCurrentLicense.ComponentsInSelection
                        If (track.OStype AndAlso
                            track.OsComponentCollection?.Contains(component)) OrElse
                            component.Name.Equals(track.Name) Then
                            TrainEventLog($"{GetCurrentMethod.Name}:ExitComp={component.GUID}:{component.Name}")
                            Exit For
                        End If
                        If Not componentsToRemove.Contains(component) Then
                            TrainEventLog($"{GetCurrentMethod.Name}:RemoveComp={component.GUID}:{component.Name}")
                            componentsToRemove.Add(component)
                        End If
                    Next
                End If
                DoLogTrainPositionReportFromGPS()
                If removeEntireLicense Then
                    PtcCancelNormalLicense(Nothing, True)
                ElseIf componentsToRemove.Count > 0 Then
                    CheckForRearOfTrain(componentsToRemove)
                    thisCurrentLicense.PtcPerformRollup(Me, "NONE", componentsToRemove, True, True)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Do Release License Behind Train
        ''' </summary>
        ''' <param name="track">track to release</param>
        Public Sub PtcDoReleaseLicenseBehind(track As Track)
            Try
                If track.SuppressAuthorityRelease Then Exit Sub
                TrainEventLog($"{GetCurrentMethod.Name}:Track={track.GUID}:{track.Name}")
                DoLogTrainPositionReportFromGPS()
                If thisCurrentLicense IsNot Nothing Then
                    If thisCurrentLicense.PTCElectronicTransmissionInProgress Then
                        TrainEventLog($"{GetCurrentMethod.Name}:PTCElectronicTransmissionInProgress")
                        Exit Sub
                    ElseIf Not thisCurrentLicense.ComponentsInSelection.Contains(track) Then
                        If track.IsEntryForPTC AndAlso
                            track.IsMarkedToDisableAutoCutout Then
                            CancelAllLicenseForPTC(False)
                        End If
                        TrainEventLog($"{GetCurrentMethod.Name}:TrackDoesNotExistInLicense")
                        Exit Sub
                    ElseIf Not CheckForOsTrackOccuppancyBeforeRelease(track) Then
                        TrainEventLog($"{GetCurrentMethod.Name}:CheckForOsTrackOccuppancyBeforeRelease")
                        Exit Sub
                    ElseIf MZEnabled Then
                        TrainEventLog($"{GetCurrentMethod.Name}:MZEnabled")
                        Exit Sub
                    End If
                End If
                Dim currentRoute As SignalRoute = Nothing
                Dim tempRouteListToRemove As New ArrayList
                For Each oneTacticalRoute As TacticalRoute In myTacticalRoutes
                    TrainEventLog($"{GetCurrentMethod.Name _
                                  }:AddTacticalRoute={oneTacticalRoute.mySignalRoute.EntrySignal.Name _
                                  }:{oneTacticalRoute.myStatus}")
                    If oneTacticalRoute.mySignalRoute.ComponentList.Contains(track) Then
                        currentRoute = oneTacticalRoute.mySignalRoute
                        tempRouteListToRemove.Add(oneTacticalRoute)
                        Exit For
                    Else
                        tempRouteListToRemove.Add(oneTacticalRoute)
                    End If
                Next
                If currentRoute Is Nothing Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Current route not found")
                    tempRouteListToRemove = Nothing
                    PtcPerformPartialRelease(track)
                    RemoveListForRollUps.Add(track)
                    Exit Sub
                End If
                myTacticalRoutesToRemove = DirectCast(tempRouteListToRemove.Clone, ArrayList)
                Dim removeList As New ArrayList
                For Each component As GenericComponent In thisCurrentLicense.ComponentsInSelection
                    TrainEventLog($"{GetCurrentMethod.Name _
                                  }:RemoveComponent={component.GUID}:{component.Name}")
                    If (track.OStype AndAlso
                        track.OsComponentCollection IsNot Nothing AndAlso
                        track.OsComponentCollection.Contains(component)) OrElse
                        (component.GUID = track.GUID) Then
                        RemoveListForRollUps.Add(component)
                        Exit For
                    End If
                    removeList.Add(component)
                    RemoveListForRollUps.Add(component)
                Next
                If removeList.Count = 0 Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Remove list empty")
                    Exit Sub
                End If
                Dim newTactList As ArrayList = DirectCast(myTacticalRoutes.Clone, ArrayList)
                For Each oneTacticalRoute As TacticalRoute In myTacticalRoutesToRemove
                    TrainEventLog($"{GetCurrentMethod.Name _
                                  }:UpdateTacticalRoute={oneTacticalRoute.mySignalRoute.EntrySignal.Name}")
                    oneTacticalRoute.myStatus = "FULLFILLED"
                    oneTacticalRoute.UpdateRouteStatus("FULLFILLED")
                    newTactList.Remove(oneTacticalRoute)
                    myTacticalRoutesFullfilled.Add(oneTacticalRoute)
                    SignalListForRemoval.Add(oneTacticalRoute.mySignalRoute.EntrySignal)
                Next
                Dim newRouteList As New ArrayList
                For Each oneTacticalRoute As TacticalRoute In newTactList
                    newRouteList.Add(oneTacticalRoute.mySignalRoute)
                Next
                Dim authLimitList As ArrayList = GetLicenseVerbiageAbbreviated(newRouteList)
                If thisCurrentLicense IsNot Nothing Then
                    TrainEventLog($"{GetCurrentMethod.Name}:Perform rollup")
                    CheckForRearOfTrain(removeList)
                    thisCurrentLicense.PtcPerformRollup(Me,
                                                        "NONE",
                                                        removeList,
                                                        True,
                                                        True,
                                                        authLimitList)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "BULLETINS"

        ''' <summary>
        ''' Send bulletins for PTC
        ''' </summary>
        ''' <param name="components">components list</param>
        Public Sub PtcSendBulletins(components As ArrayList)
            Try
                For Each track As Track In GetFullTrackList(components)
                    If track.TrackTagLabels.Count > 0 Then
                        Dim tags As List(Of Tag) = track.TrackTagLabels.OfType(Of Tag).ToList
                        For Each tag As Tag In tags
                            NetworkMessages.PTCBOSVETMSMessaging.Send_VETMS_PTC_TrackTagRestrictionMessageCreate(tag,
                                                                                                                 Me,
                                                                                                                 tag.PTCMessageUID,
                                                                                                                 False,
                                                                                                                 GetCurrentMethod.Name)
                        Next
                    End If
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Get Full Track List
        ''' This method can be removed when MRS CAD to add all tracks over train.
        ''' </summary>
        ''' <param name="components">list of components</param>
        ''' <returns>Return full list of tracks</returns>
        Private Function GetFullTrackList(components As ArrayList) As List(Of Track)
            Dim currentTracks As List(Of Track) = components.OfType(Of Track).ToList
            Try
                Dim newTracks As New List(Of Track)
                For Each currentTrack As Track In currentTracks
                    newTracks.Add(currentTrack)
                    If Not currentTrack.OStype Then
                        Dim tracks As List(Of Track) = currentTrack.GetTracksWithSameName(False)
                        For Each track As Track In tracks
                            If Not currentTracks.Contains(track) Then
                                TrainEventLog($"{GetCurrentMethod.Name} AddNewTrack={track.GUID}:{track.Name}")
                                newTracks.Add(track)
                            End If
                        Next
                    End If
                Next
                Return newTracks
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return currentTracks
        End Function

#End Region

    End Class

End Namespace
