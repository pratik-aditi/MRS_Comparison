Option Explicit On
Option Strict On
Imports System.Data.OleDb
Imports System.Reflection.MethodBase
Imports System.Text
Imports TMDS.GraphicComponent
Imports TMDS.JoinAuthorization
Imports TMDS.ManagementClasses
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackComponents
Imports TMDS.TrainComponents

Namespace SignalComponents

    Partial Public Class Signal

        Public RequestJointAuthForPSS As Boolean
        Public RequestJointAuthForLicense As Boolean
        Public RequestJointAuthForLicenseClick As Boolean
        Public JoinControlSignalRouteCollection As New ArrayList
        Public JoinControlLastAuthorizationSent As JoinAuthorization

        ''' <summary>
        ''' Checks if is needed to request popup for signal
        ''' </summary>
        ''' <returns>Return <c>True</c>, if signal is marked as REQUESTPOPUP, otherwise, return <c>False</c></returns>
        Public ReadOnly Property IsRequestPopup() As Boolean
            Get
                Return Notes.Equals("REQUESTPOPUP", StringComparison.InvariantCultureIgnoreCase)
            End Get
        End Property

#Region "OFFICE MODE VIA JOIN INTEGRATION"

        ''' <summary>
        ''' Simple info message for user
        ''' </summary>
        ''' <param name="theMessage">text message for show</param>

        Public Sub InfoMessage(theMessage As String)
            Try
                Dim messageList As New ArrayList From {
                    theMessage, $"SINAL:{Name}"
                    }
                Using frmMessageBox As New frmMessageBox()
                    frmMessageBox.Owner = CurrentDrawingForm
                    frmMessageBox.SetMessage(messageList, optSystemMessageString3, False, CurrentDrawingForm.myLaunchPoint)
                    frmMessageBox.ShowDialog()
                End Using
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Send Join Integration Office Mode
        ''' </summary>
        Private Sub SendJoinIntegrationOfficeMode(componentList As ArrayList)
            Try
                SignalEventLog($"{GetCurrentMethod.Name}")
                If SignalIndStatus = Signal.IndStatus.Cleared Then
                    SignalEventLog($"{GetCurrentMethod.Name}:Signal is clear")
                    InfoMessage(optSignalAuth07)
                    Exit Sub
                End If
                Dim endTrack As Track = componentList.OfType(Of Track).Last
                If endTrack Is Nothing Then
                    SignalEventLog($"{GetCurrentMethod.Name}:End track not found")
                    Exit Sub
                End If
                If JointControlAuthorizationOfficeModeRequested Then
                    If gJointLicenseForTrainTrain IsNot Nothing Then
                        Using join As New JoinAuthorization
                            If JointControlAuthorizationOfficeModeConfirmed Then 'Just show form if join already authorized
                                Using joinForm As New frmJoinAuthorization
                                    joinForm.Owner = CurrentDrawingForm
                                    joinForm.SetCancelAuthorization(gJointLicenseForTrainTrain,
                                                                    Me, CurrentDrawingForm.myLaunchPoint)
                                    joinForm.ShowDialog()
                                    If Not joinForm.FormFeedBack Then
                                        SignalEventLog($"{GetCurrentMethod.Name}:Dispatcher clicked in CANCEL")
                                        Exit Sub
                                    End If
                                    join.Reason = joinForm.Reason
                                End Using
                            End If
                            join.LoadData(Me, gJointLicenseForTrainTrain, endTrack)
                            If String.IsNullOrWhiteSpace(join.Reason) Then join.Reason = optSignalAuth10
                            join.CancelAuthorizationGoingToRumo()
                        End Using
                        'Set arrow for red
                        myAuthorizationIcon.myBrush = New SolidBrush(Color.Red)
                        myAuthorizationIcon.Invoke(New MethodInvoker(AddressOf myAuthorizationIcon.Refresh))
                        gJointLicenseForTrainTrain = Nothing
                    End If
                    JointControlAuthorizationOfficeModeRequested = False
                    JointControlAuthorizationOfficeModeConfirmed = False
                    JoinControlSignalRouteCollection.Clear()
                    JoinControlLastAuthorizationSent = Nothing
                Else
                    If TrainCollection.Count = 0 Then
                        SignalEventLog($"{GetCurrentMethod.Name}:Trains not found")
                        InfoMessage(optSignalAuth08)
                        Exit Sub
                    End If
                    Using join As New JoinAuthorization
                        Dim theDevice As GraphicComponent.Device = join.FindJoinDevice(endTrack.GUID)
                        If theDevice IsNot Nothing AndAlso
                            theDevice.JoinAuthAlert Then
                            SignalEventLog($"{GetCurrentMethod.Name}:Found a join authorization opposite")
                            InfoMessage(optSignalAuth09)
                            Exit Sub
                        End If
                        Using joinForm As New frmJoinAuthorization
                            joinForm.Owner = CurrentDrawingForm
                            joinForm.SetRequestAuthorization(Me, endTrack)
                            joinForm.ShowDialog()
                            If Not joinForm.FormFeedBack Then
                                SystemEvent($"{GetCurrentMethod.Name}:Dispatcher clicked in CANCEL")
                                Exit Sub
                            End If
                            join.Reason = joinForm.Reason
                            join.LoadData(Me, joinForm.SymbolSelected, endTrack)
                        End Using
                        join.RequestAuthorizationGoingToRumo()
                        gJointLicenseForTrainTrain = join.MyTrain
                        JoinControlLastAuthorizationSent = join
                    End Using
                    JointControlAuthorizationOfficeModeRequested = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Does Signal Route Have Join Track
        ''' </summary>
        ''' <param name="components">components to check</param>
        ''' <returns><see langword="True"/>Join integration track found in list, <see langword="False"/> otherwise.</returns>
        Public Function DoesSignalRouteHaveJoinTrack(components As ArrayList) As Boolean
            Try
                For Each track As Track In components.OfType(Of Track)
                    If track.IsMarkedToJoinIntregation Then Return True
                Next
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return False
        End Function

        ''' <summary>
        ''' Request Popup
        ''' </summary>
        ''' <returns></returns>
        Public Function RequestPopup() As Boolean
            Try
                Dim theSignalRouteCollection As ArrayList = GetTheSignalRoute(True)
                If theSignalRouteCollection Is Nothing Then Return False
                For Each oneSignal As Signal In theSignalRouteCollection.OfType(Of Signal)
                    If oneSignal.JointControlAuthorizationEnabled AndAlso
                        (oneSignal.JointControlAuthorizationOfficeModeConfirmed OrElse
                        oneSignal.JointControlAuthorizationOfficeModeRequested) Then
                        SystemEvent($"{GetCurrentMethod.Name}:Signal {oneSignal.Name} with join authorization pending")
                        Return False
                    End If
                Next
                Using signalForm As New frmSignalDialog()
                    signalForm.SetUpProperties(Me, True)
                    signalForm.ShowDialog()
                    If Not signalForm.UserResponse Then
                        SystemEvent($"{GetCurrentMethod.Name}:Dispatcher clicked in NO")
                        Return False
                    End If
                End Using
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

#End Region

#Region "CTC MODE"

        ''' <summary>
        ''' Send Join Authorization Ctc Mode
        ''' </summary>
        Private Sub SendJoinAuthorizationCtcMode()
            Try
                If Not JointControlAuthorizationCtlBit.BitState Then
                    JointControlAuthorizationCtlBit.BitState = True
                    If JointControlAuthorizationCancelBit IsNot Nothing Then
                        JointControlAuthorizationCancelBit.BitState = False
                    End If
                Else
                    JointControlAuthorizationCtlBit.BitState = False
                    If JointControlAuthorizationCancelBit IsNot Nothing Then
                        JointControlAuthorizationCancelBit.BitState = True
                    End If
                End If
                myControlPoint.FlasherMode(True)
                AddToSendCollection(GetCurrentMethod.Name(),
                                    True,
                                    JointControlAuthorizationCancelBit)
                gSendCTC = True
                If myAuthorizationIcon IsNot Nothing Then
                    If Not JointControlAuthorizationIndBit.BitState Then
                        JointControlAuthorizationUpdateStatus()
                    End If
                End If
                TMDS.SystemUtilities.UtilitiesDisplayForm.SetCursor(curSend)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Does Signal Route Have Remote Track
        ''' </summary>
        ''' <param name="theSignalRouteCollection"></param>
        ''' <param name="allTracksLoadedInSameDesk"></param>
        ''' <returns></returns>
        Private Function DoesSignalRouteHaveRemoteTrack(theSignalRouteCollection As ArrayList,
                                                        Optional ByRef allTracksLoadedInSameDesk As Boolean = False) As Boolean
            Try
                For Each oneTrack As Track In theSignalRouteCollection.OfType(Of Track)
                    If oneTrack.IsLinkToRemoteTrack OrElse
                        oneTrack.IsRemoteTrack Then
                        Return True
                    End If
                    If oneTrack.GUID = 121693 OrElse
                        oneTrack.GUID = 121692 OrElse
                        oneTrack.GUID = 96949 OrElse
                        oneTrack.GUID = 96950 OrElse
                        oneTrack.GUID = 96258 OrElse
                        oneTrack.GUID = 96259 Then
                        Return True
                    End If
                Next
                allTracksLoadedInSameDesk = True
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

#End Region

        ''' <summary>
        ''' Send Join Authorization Request
        ''' </summary>
        ''' <param name="theTrain"></param>
        ''' <param name="isSpecialLicense"></param>
        Public Sub SendJoinAuthorizationRequest(theTrain As TrainID,
                                                Optional isSpecialLicense As Boolean = False)
            Try
                If JointControlAuthorizationOfficeMode Then
                    Dim theSignalRouteCollection As ArrayList = GetTheSignalRoute()
                    If theSignalRouteCollection Is Nothing Then Exit Sub
                    Dim RouteHasRemoteTrack As Boolean = DoesSignalRouteHaveRemoteTrack(theSignalRouteCollection)
                    If Not RouteHasRemoteTrack Then Exit Sub
                    If myAuthorizationIcon IsNot Nothing Then
                        If JointControlAuthorizationOfficeModeRequested Then
                            If Not RequestJointAuthForLicense Then
                                JointControlAuthorizationOfficeModeRequested = False
                                JointControlAuthorizationOfficeModeConfirmed = False
                            Else
                                RequestJointAuthForLicense = False
                            End If
                        Else
                            JointControlAuthorizationOfficeModeRequested = True
                        End If
                        If theTrain IsNot Nothing AndAlso
                            isSpecialLicense Then
                            JointControlAuthorizationRequest(Me, theTrain.Name, True)
                        ElseIf theTrain IsNot Nothing Then
                            JointControlAuthorizationRequest(Me, theTrain.Name)
                        Else
                            JointControlAuthorizationRequest(Me)
                        End If
                        JointControlAuthorizationUpdateStatus()
                    End If
                Else
                    SendJoinAuthorizationCtcMode()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Update Database Joint Authorization
        ''' </summary>
        Private Sub UpdateDatabaseJointAuthorization()
            Try
                Dim query As String = $"UPDATE tblCompSignals SET 
                                        JointControlAuthorizationOfficeModeRequested = {Convert.ToInt16(JointControlAuthorizationOfficeModeRequested)}, 
                                        JointControlAuthorizationOfficeModeConfirmed = {Convert.ToInt16(JointControlAuthorizationOfficeModeConfirmed)} 
                                        WHERE UID = {GUID}"
                Using dbConn As New OleDbConnection(gConnectionStringStatic)
                    dbConn.Open()
                    Using dbComm As New OleDbCommand(query, dbConn)
                        dbComm.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub


#Region "REQUEST TRACK NAME"

        ''' <summary>
        ''' Request Name Line
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="e"></param>
        ''' <remarks>Handles Request Name Line Menu Click</remarks>
        Private Sub mnuRequestTrackName_Click(sender As Object, e As EventArgs)
            Try
                SignalEventLog($"{GetCurrentMethod.Name}:CLICKED - REQUEST NAME LINE")
                TrackNameAuthorizationRequest(Me)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Toggle Name Line Option
        ''' </summary>
        ''' <param name="isJointAuthorizationAvailable"></param>
        ''' <param name="theSignal"></param>
        ''' <remarks>Toggles Between Request Name Line and Joint Auth For Joint Authorization Signals</remarks>
        Public Sub ToggleTrackNameOption(isJointAuthorizationAvailable As Boolean,
                                        theSignal As Signal)
            Try
                For Each oneSignal As Signal In SignalCollection
                    If JointControlAuthorizationOfficeMode AndAlso
                        Notes.Contains("ENABLETRACKNAMEREQUEST") AndAlso
                        oneSignal.ComponentType = theSignal.ComponentType Then
                        If isJointAuthorizationAvailable Then
                            oneSignal.RequestTrackNameEnabled = False
                        Else
                            oneSignal.RequestTrackNameEnabled = True
                        End If
                    End If
                Next
                RefreshContextMenu()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Set Requests Options Signal Menu
        ''' </summary>
        ''' <remarks>Sets The Correct Menu Items For Joint Authorization Signals</remarks>
        Private Sub SetRequestsOptionsSignalMenu()
            Try
                If Not Notes.Contains("ENABLETRACKNAMEREQUEST") Then Exit Sub
                If RequestTrackNameEnabled Then
                    mnuRequestTrackName.Enabled = True
                    mnuRequestTrackName.Visible = True
                    mnuJointAuthorization.Enabled = False
                    mnuJointAuthorization.Visible = False
                Else
                    mnuRequestTrackName.Enabled = False
                    mnuRequestTrackName.Visible = False
                    mnuJointAuthorization.Enabled = True
                    mnuJointAuthorization.Visible = True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Name Line Authorization Request
        ''' </summary>
        ''' <param name="theSignal">Signal Used to Request Name Line</param>
        ''' <param name="theSymbol">Train Symbol</param>
        ''' <param name="IsPSS">Is Special License</param>
        ''' <remarks>Requests Name Line To Another Desk</remarks>
        Private Sub TrackNameAuthorizationRequest(theSignal As Signal,
                                                 Optional theSymbol As String = "",
                                                 Optional IsPSS As Boolean = False)
            Try
                Dim myRoute As SignalRoute = GetSignalRoute(theSignal)
                If myRoute Is Nothing Then
                    SignalEventLog($"{GetCurrentMethod.Name}:Signal Route is not found!")
                    Exit Sub
                End If
                Dim theComponent As Object = FindExitTrackForRequests(myRoute)
                If theComponent Is Nothing Then
                    SignalEventLog($"{GetCurrentMethod.Name}:Remote track NOT FOUND")
                    Exit Sub
                End If
                SendAuthorizationRequests(theComponent,
                                          theSignal,
                                          theSymbol,
                                          IsPSS)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Build Signal Menu Choose Request
        ''' </summary>
        ''' <remarks>Chooses Which Menu Options For Joint Authorization Signals </remarks>
        Private Sub BuildSignalMenuChooseRequest()
            If JointControlAuthorizationEnabled AndAlso
                   Not myControlPoint.TunnelAutomaticMode AndAlso
                   Notes.Contains("ENABLETRACKNAMEREQUEST") Then
                Dim mnuSplit1 As New MenuItem("-")
                ContextMenuControl.MenuItems.Add(mnuSplit1)
                mnuRequestTrackName = New MenuItem(optSignalAuth36)
                AddHandler mnuRequestTrackName.Click, AddressOf mnuRequestTrackName_Click
                ContextMenuControl.MenuItems.Add(mnuRequestTrackName)
                mnuRequestTrackName.Enabled = True
                mnuRequestTrackName.Visible = True
                mnuJointAuthorization = New MenuItem("SEND AUTHORIZATION REQUEST")
                AddHandler mnuJointAuthorization.Click, AddressOf Me.mnuJointAuthorization_Click
                ContextMenuControl.MenuItems.Add(mnuJointAuthorization)
                mnuJointAuthorization.Enabled = False
                mnuJointAuthorization.Visible = False
            ElseIf JointControlAuthorizationEnabled AndAlso
                Not myControlPoint.TunnelAutomaticMode Then
                Dim mnuSplit1 As New MenuItem("-")
                ContextMenuControl.MenuItems.Add(mnuSplit1)
                mnuJointAuthorization = New MenuItem("SEND AUTHORIZATION REQUEST")
                AddHandler mnuJointAuthorization.Click, AddressOf Me.mnuJointAuthorization_Click
                ContextMenuControl.MenuItems.Add(mnuJointAuthorization)
            End If
        End Sub
#End Region


#Region "JOINT AUTH / TRACK NAME REQUEST UTILITIES"
        ''' <summary>
        ''' Find Exit Track For Requests
        ''' </summary>
        ''' <param name="theRoute">Signal Route Based On Selected Signal</param>
        ''' <returns>Returns Exits Track For Name Line/Joint Authorizatin Requests</returns>
        Private Function FindExitTrackForRequests(theRoute As SignalRoute) As Object
            Try
                Dim theComponent As Object
                If ComponentType.Equals("R", StringComparison.InvariantCultureIgnoreCase) Then
                    theComponent = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(theRoute.ExitTrack.RightLink)
                    If theComponent Is Nothing AndAlso
                        theRoute.ExitTrack.RightLink = 0 AndAlso
                        theRoute.ExitTrack.IsRemoteTrack Then Return theRoute.ExitTrack
                Else
                    theComponent = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(theRoute.ExitTrack.LeftLink)
                    If theComponent Is Nothing AndAlso
                        theRoute.ExitTrack.LeftLink = 0 AndAlso
                        theRoute.ExitTrack.IsRemoteTrack Then Return theRoute.ExitTrack
                End If
                Return theComponent
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' Send Authorization Requests
        ''' </summary>
        ''' <param name="theComponent">Exit Track On Route</param>
        ''' <param name="theSignal">Selected Signal</param>
        ''' <param name="theSymbol">Train Symbol</param>
        ''' <param name="isPSS">is Special License</param>
        ''' <remarks>Sends Authorization  For Name Line/Joint Authorizatin Requests</remarks>
        Private Sub SendAuthorizationRequests(theComponent As Object,
                                              theSignal As Signal,
                                              theSymbol As String,
                                              isPSS As Boolean)
            Try
                Dim whoCalled As String = (New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name
                Dim loopControl As Integer = 0
                Do
                    If theComponent.GetType.ToString = gTrackComponentName Then
                        Dim theTrack As Track = DirectCast(theComponent, Track)
                        Dim trackName As String = theTrack.GUID.ToString
                        If theTrack.IsRemoteTrack Then
                            ChooseRequestToSend(theSymbol, isPSS, theTrack, whoCalled)
                            Exit Do
                        End If
                    ElseIf theComponent.GetType.ToString = gSignalComponentName Then
                        Dim oneSignal As Signal = DirectCast(theComponent, Signal)
                        If Not oneSignal.AutomaticSignal AndAlso
                            (oneSignal.ComponentType = theSignal.ComponentType) Then
                            Exit Do
                        End If
                    End If
                    theComponent = FindNextObject(theComponent)
                    loopControl += 1
                    If loopControl > 15 Then
                        Exit Sub
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Choose Request to Send
        ''' </summary>
        ''' <param name="theSymbol">Train Symbol</param>
        ''' <param name="isPSS">Is Special License</param>
        ''' <param name="theTrack">Exit Track</param>
        ''' <param name="whoCalled">Who Called | Name Line Or Joint Authorization</param>
        ''' <remarks>Chooses Between Name Line and Joint Authorization Requests</remarks>
        Private Sub ChooseRequestToSend(theSymbol As String,
                                        isPSS As Boolean,
                                        theTrack As Track,
                                        whoCalled As String)
            Try
                Select Case whoCalled
                    Case "TrackNameAuthorizationRequest"
                        If Not String.IsNullOrWhiteSpace(theSymbol) AndAlso
                           Not isPSS Then
                            theTrack.myRemoteTrack.RequestTrackName(Me, theSymbol, False, theTrack)
                        ElseIf Not String.IsNullOrWhiteSpace(theSymbol) AndAlso
                               isPSS Then
                            theTrack.myRemoteTrack.RequestTrackName(Me, theSymbol, True, theTrack)
                        Else
                            If theTrack.myRemoteTrack.theNetworkConnecton IsNot Nothing Then
                                theTrack.myRemoteTrack.RequestTrackName(Me, String.Empty, False, theTrack)
                            End If
                        End If
                    Case "JointControlAuthorizationRequest"
                        If Not String.IsNullOrWhiteSpace(theSymbol) AndAlso
                           Not isPSS Then
                            theTrack.myRemoteTrack.RequestSignalAuthorization(Me, theSymbol, False, theTrack)
                        ElseIf Not String.IsNullOrWhiteSpace(theSymbol) AndAlso
                               isPSS Then
                            theTrack.myRemoteTrack.RequestSignalAuthorization(Me, theSymbol, True, theTrack)
                        Else
                            If theTrack.myRemoteTrack.theNetworkConnecton IsNot Nothing Then
                                theTrack.myRemoteTrack.RequestSignalAuthorization(Me, String.Empty, False, theTrack)
                            End If
                        End If
                End Select
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Find Next Object
        ''' </summary>
        ''' <param name="theComponent">Initial Component To Search</param>
        ''' <returns>Find Next Object For Name Line/Joint Authorizatin Requests</returns>
        Private Function FindNextObject(theComponent As Object) As Object
            Try
                If ComponentType.Equals("R", StringComparison.InvariantCultureIgnoreCase) Then
                    Return SystemUtilities.FetchData.FindCompItemFromGUID(DirectCast(theComponent, GenericComponent).RightLink)
                Else
                    Return SystemUtilities.FetchData.FindCompItemFromGUID(DirectCast(theComponent, GenericComponent).LeftLink)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return Nothing
        End Function
#End Region

    End Class

End Namespace

