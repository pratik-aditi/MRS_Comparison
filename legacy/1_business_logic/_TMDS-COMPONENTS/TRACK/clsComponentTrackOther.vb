Imports System.Data.OleDb
Imports System.Net
Imports System.Text
Imports System.Timers
Imports TMDS.CodeServerClient
Imports TMDS.GraphicComponent
Imports TMDS.ManagementClasses
Imports TMDS.ManagementClasses.SystemEventRecord
Imports TMDS.SignalComponents
Imports TMDS.SwitchComponents
Imports TMDS.SystemUtilities
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrackAuthority
Imports TMDS.TrackComponents
Imports TMDS.TrackTagComponents
Imports TMDS.TrainComponents
Imports System.Reflection.MethodBase
Imports System.Net.Mime.MediaTypeNames

Namespace TrackOtherComponents

#Region "RRXING"

    Public Class RRXING
        Inherits GenericComponent
        Public TrafficDirection As String
        Public Occuppied As Boolean
        Public SignalStatOnMe As SignalStatus
        Public FlipMe As Boolean
        Public TrackName As String
        Public OSType As Boolean = False

        Enum SignalStatus
            SignalAtStop
            SignalRequested
            SignalCleared
            SignalInTime
            SignalUnSolicated
            PendingStateChange
            SignalRequestedDrill
            SignalClearedDrill
        End Enum

        Sub ApplyTraffic()
            'nothing here to do, just a dummy sub to match up to the track class
        End Sub

        Overridable Sub Draw(ByRef g As Graphics)

        End Sub

        Public Function FindExitComponent(ByVal EntryGUID As Int32) As Object
            Try
                Dim GUIDtoFind As Int32
                If EntryGUID = Me.LeftLink Then
                    GUIDtoFind = Me.RightLink
                ElseIf EntryGUID = Me.LeftAltLink Then
                    GUIDtoFind = Me.RightAltLink
                ElseIf EntryGUID = Me.RightLink Then
                    GUIDtoFind = Me.LeftLink
                ElseIf EntryGUID = Me.RightAltLink Then
                    GUIDtoFind = Me.LeftAltLink
                Else
                    Return Nothing
                End If
                Return Me.FindCompFromGUID(GUIDtoFind)

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindExitGUID(ByVal EntryGUID As Int32) As Object
            Try
                Dim GUIDtoFind As Int32
                If EntryGUID = Me.LeftLink Then
                    GUIDtoFind = Me.RightLink
                ElseIf EntryGUID = Me.LeftAltLink Then
                    GUIDtoFind = Me.RightAltLink
                ElseIf EntryGUID = Me.RightLink Then
                    GUIDtoFind = Me.LeftLink
                ElseIf EntryGUID = Me.RightAltLink Then
                    GUIDtoFind = Me.LeftAltLink
                Else
                    Return Nothing
                End If
                Return GUIDtoFind

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindCompFromGUID(ByVal UniqueID As Int32) As Object
            Try
                Dim comp As Object
                For Each comp In TMDS.GlobalDeclarations.gComponentsCollection
                    If UniqueID = comp.GUID Then
                        Return comp
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function ValidateForClearRoute(ByVal SignalType As String, Optional ByVal MyLinkTrack As Int32 = 0, Optional ByVal SignalNotInitialized As Boolean = False, Optional ByRef ErrorMessage As String = "", Optional myUID As String = "") As Boolean
            Try
                ErrorMessage = ""
                Dim CompArray As New ArrayList()
                Dim comp As Track
                'Select Case SignalType
                '    Case "L", "R"
                '        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                '        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                '    Case "D", "U"
                '        CompArray.Add(FindCompFromGUID(Me.RightLink))
                '        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                'End Select

                If MyLinkTrack <> 0 Then
                    If MyLinkTrack = Me.RightAltLink Or MyLinkTrack = Me.LeftAltLink Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                    End If
                End If
                Dim compUID As String = ""
                For Each comp In CompArray
                    If comp IsNot Nothing Then
                        If comp.GetType.ToString = gTrackComponentName Then
                            compUID = String.Empty
                            compUID = " <Track GUID:" & comp.GUID
                            If comp.TrafficDirection <> "" Then Return False
                            If comp.Occuppied Then ErrorMessage = "TRACK OCCUPPIED IN ROUTE: " & myUID & compUID & ">" : Return False
                            If comp.TrackAndTime AndAlso Not SignalNotInitialized Then ErrorMessage = "AUTHORITY CONFLICT IN ROUTE: " & myUID & compUID & ">" : Return False ' 
                            If comp.BlockAbsolute And Not SignalNotInitialized Then ErrorMessage = "BLOCK IN ROUTE: " & myUID & compUID & ">" : Return False
                            If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                        End If
                    Else
                        Return False
                    End If
                Next
                CompArray = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function ValidateForClearRouteExcludeTNT(ByVal SignalType As String, Optional ByVal MyLinkTrack As Int32 = 0, Optional ByVal SignalNotInitialized As Boolean = False) As Boolean
            Dim CompArray As New ArrayList()
            Dim comp As Track
            Try
                If MyLinkTrack <> 0 Then
                    If MyLinkTrack = Me.RightAltLink Or MyLinkTrack = Me.LeftAltLink Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                    End If
                End If
                For Each comp In CompArray
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If comp.TrafficDirection <> "" Then Return False
                        If comp.Occuppied AndAlso Not comp.NonTransverable Then Return False
                        'If Not optDisableSignalClearCheckForTNTInRRXing AndAlso comp.TrackAndTime AndAlso Not SignalNotInitialized Then Return False ' 
                        'If comp.BlockAbsolute And Not SignalNotInitialized Then Return False
                        If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                    End If
                Next
                If optDisableSignalClearCheckForTNTInRRXing AndAlso Not SignalNotInitialized Then
                    Dim BothTrackHasTAT As Boolean = True
                    Dim BothTrackHasBlk As Boolean = True

                    For Each comp In CompArray
                        If comp.TrackAuthorities IsNot Nothing AndAlso comp.TrackAuthorities.Count > 0 Then
                        Else
                            BothTrackHasTAT = False
                        End If
                        If comp.DeviceBlocks IsNot Nothing AndAlso comp.DeviceBlocks.Count > 0 Then
                        Else
                            BothTrackHasBlk = False
                        End If
                    Next
                    If BothTrackHasBlk Then
                        Return False
                    ElseIf BothTrackHasTAT Then
                        Dim AuthIDs As New ArrayList
                        Dim Comp1 As Track = Nothing
                        Dim Comp2 As Object = Nothing
                        Dim TAT As Object = Nothing
                        Try
                            If CompArray IsNot Nothing AndAlso CompArray.Count > 1 Then
                                Comp1 = CompArray(0)
                                Comp2 = CompArray(1)
                                If Comp1 IsNot Nothing AndAlso Comp1.TrackAuthorities IsNot Nothing AndAlso Comp1.TrackAuthorities.Count > 0 Then
                                    For Each TAT In Comp1.TrackAuthorities
                                        If TAT.GetType.ToString = gTTComponentName Then
                                            If Not AuthIDs.Contains(TAT.AuthorityNumber) Then AuthIDs.Add(TAT.AuthorityNumber)
                                        End If
                                    Next
                                End If
                                If (Comp2 IsNot Nothing AndAlso Comp2.TrackAuthorities IsNot Nothing AndAlso Comp2.TrackAuthorities.Count > 0) AndAlso (AuthIDs IsNot Nothing AndAlso AuthIDs.Count > 0) Then
                                    Dim HasMatchingTAT As Boolean = False
                                    For Each TAT In Comp2.TrackAuthorities
                                        If TAT.GetType.ToString = gTTComponentName Then
                                            If AuthIDs.Contains(TAT.AuthorityNumber) Then HasMatchingTAT = True : Exit For
                                        End If
                                    Next
                                    If HasMatchingTAT Then
                                        Return False
                                    Else
                                        Return True
                                    End If

                                End If
                            End If
                        Catch ex As Exception
                            GetErrorDetails(ex)
                            Return False
                        Finally
                            AuthIDs = Nothing
                            Comp1 = Nothing
                            Comp2 = Nothing
                            TAT = Nothing
                        End Try
                        Return False
                    End If
                End If

                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            Finally
                CompArray = Nothing
                comp = Nothing
            End Try
        End Function

        Public Function ValidateForAuthorityRoute(Optional ByVal TrackGuid As Int32 = 0, Optional ByVal CompCollection As ArrayList = Nothing) As Boolean
            Dim tmpCompArray As New ArrayList
            Dim PassesThroughXXing As Boolean = False
            Dim CompArray As New ArrayList()
            Dim comp As Track
            Try


                If TrackGuid = 0 Then
                    CompArray.Add(FindCompFromGUID(Me.RightLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                Else
                    If Me.RightLink = TrackGuid Or Me.LeftLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    ElseIf Me.RightAltLink = TrackGuid Or Me.LeftAltLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))

                    End If
                    If optDisableSignalClearCheckForTNTInRRXing Then
                        With tmpCompArray
                            .Add(FindCompFromGUID(Me.RightLink))
                            .Add(FindCompFromGUID(Me.LeftLink))
                            .Add(FindCompFromGUID(Me.RightAltLink))
                            .Add(FindCompFromGUID(Me.RightAltLink))
                        End With
                    End If

                End If
                If optDisableSignalClearCheckForTNTInRRXing AndAlso tmpCompArray IsNot Nothing AndAlso tmpCompArray.Count > 0 AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                    Dim Counter As Integer = 0
                    For Each comp In tmpCompArray
                        If CompCollection.Contains(comp) Then
                            Counter = Counter + 1
                        End If
                    Next
                    If Counter > 1 Then PassesThroughXXing = True
                End If

                For Each comp In CompArray
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If optDisableSignalClearCheckForTNTInRRXing AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                            If PassesThroughXXing Then
                                If comp.TrafficDirection <> "" Then Return False
                                If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                            End If
                        Else
                            If comp.TrafficDirection <> "" Then Return False
                            If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                        End If
                        'If comp.TrafficDirection <> "" Then Return False
                        ''If comp.Occuppied Then Return False
                        'If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                    End If
                Next
                CompArray = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            Finally
                comp = Nothing
                CompArray = Nothing
                tmpCompArray = Nothing

            End Try
        End Function

        Public Function ValidateForAuthorityAgainstOccuppancy(Optional ByVal TrackGuid As Int32 = 0) As Boolean
            Try
                Dim CompArray As New ArrayList()
                Dim comp As Track
                If TrackGuid = 0 Then
                    CompArray.Add(FindCompFromGUID(Me.RightLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                Else
                    If Me.RightLink = TrackGuid Or Me.LeftLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    ElseIf Me.RightAltLink = TrackGuid Or Me.LeftAltLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    End If
                End If
                For Each comp In CompArray
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If comp.Occuppied Then Return True
                    End If
                Next
                CompArray = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function ValidateForAuthorityAgainstSoleAuthorities(ByVal TrackGuid As Int32, ByRef AuthorityReference As TrackAuthority.TrackAuthority, Optional CompCollection As ArrayList = Nothing) As Boolean
            Try
                Dim CompArray As New ArrayList()
                Dim comp As Track
                If TrackGuid = 0 Then
                    CompArray.Add(FindCompFromGUID(Me.RightLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                Else
                    If Me.RightLink = TrackGuid Or Me.LeftLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    ElseIf Me.RightAltLink = TrackGuid Or Me.LeftAltLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    End If
                End If
                If optDisableSignalClearCheckForTNTInRRXing AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                    Dim OKtoProceed As Boolean = True
                    Dim BothTrackHasTAT As Boolean = True
                    For Each comp In CompArray
                        If comp.TrackAuthorities IsNot Nothing AndAlso comp.TrackAuthorities.Count > 0 Then
                        Else
                            BothTrackHasTAT = False
                        End If
                    Next
                    If BothTrackHasTAT Then
                        If DoesTNTCrossesRRXing(CompArray, CompCollection) Then OKtoProceed = False
                    End If


                    'For Each comp In CompArray
                    '    If CompCollection.Contains(comp) Then
                    '        OKtoProceed = True
                    '        Exit For
                    '    End If
                    'Next
                    If Not OKtoProceed Then
                        For Each comp In CompArray
                            If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(comp, Track)
                                If trk.TrackAuthorities.Count > 0 Then
                                    Dim auth As TrackAuthority.TrackAuthority
                                    For Each auth In trk.TrackAuthorities
                                        If Not auth.JointAuthority Then
                                            AuthorityReference = auth
                                            Return False
                                        End If
                                    Next
                                End If

                            End If
                        Next
                    End If
                Else
                    For Each comp In CompArray
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(comp, Track)
                            If trk.TrackAuthorities.Count > 0 Then
                                Dim auth As TrackAuthority.TrackAuthority
                                For Each auth In trk.TrackAuthorities
                                    If Not auth.JointAuthority Then
                                        AuthorityReference = auth
                                        Return False
                                    End If
                                Next
                            End If

                        End If
                    Next
                End If

                CompArray = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function ValidateForAuthorityAgainstJointAuthorities(Optional ByVal TrackGuid As Int32 = 0, Optional ByVal CompCollection As ArrayList = Nothing) As Boolean
            Dim OKtoProceed As Boolean = True
            Try
                Dim CompArray As New ArrayList()
                Dim comp As Track
                If TrackGuid = 0 Then
                    CompArray.Add(FindCompFromGUID(Me.RightLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                Else
                    If Me.RightLink = TrackGuid Or Me.LeftLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    ElseIf Me.RightAltLink = TrackGuid Or Me.LeftAltLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    End If
                End If
                If optDisableSignalClearCheckForTNTInRRXing AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                    Dim BothTrackHasTAT As Boolean = True
                    For Each comp In CompArray
                        If comp.TrackAuthorities IsNot Nothing AndAlso comp.TrackAuthorities.Count > 0 Then
                        Else
                            BothTrackHasTAT = False
                        End If
                    Next
                    If BothTrackHasTAT Then
                        If Not DoesTNTCrossesRRXing(CompArray, CompCollection) Then OKtoProceed = False
                    Else
                        OKtoProceed = False
                    End If
                    If OKtoProceed Then

                    End If
                    'OKtoProceed = False
                    'For Each comp In CompArray
                    '    If CompCollection.Contains(comp) Then
                    '        OKtoProceed = True
                    '        Exit For
                    '    End If
                    'Next
                End If
                If OKtoProceed Then
                    For Each comp In CompArray
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(comp, Track)
                            If trk.TrackAuthorities.Count > 0 Then
                                Dim auth As TrackAuthority.TrackAuthority
                                For Each auth In trk.TrackAuthorities
                                    Return True
                                Next
                            End If

                        End If
                    Next
                End If
                CompArray = Nothing
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        Public Function ValidateForAuthorityAgainstJointAuthorities(ByVal TrackGuid As Int32, ByVal IssueTo As String, Optional ByVal CompCollection As ArrayList = Nothing) As ArrayList
            Dim OKtoProceed As Boolean = True
            Try
                Dim CompArray As New ArrayList()
                Dim AuthorityJointLimits As New ArrayList()
                Dim comp As Track
                If TrackGuid = 0 Then
                    CompArray.Add(FindCompFromGUID(Me.RightLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                    CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                Else
                    If Me.RightLink = TrackGuid Or Me.LeftLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    ElseIf Me.RightAltLink = TrackGuid Or Me.LeftAltLink = TrackGuid Then
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                    Else
                        CompArray.Add(FindCompFromGUID(Me.RightLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftLink))
                        CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                        CompArray.Add(FindCompFromGUID(Me.LeftAltLink))

                    End If
                End If
                If optDisableSignalClearCheckForTNTInRRXing AndAlso CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                    'OKtoProceed = False
                    'For Each comp In CompArray
                    '    If CompCollection.Contains(comp) Then
                    '        OKtoProceed = True
                    '        Exit For
                    '    End If
                    'Next
                    Dim BothTrackHasTAT As Boolean = True
                    For Each comp In CompArray
                        If comp.TrackAuthorities IsNot Nothing AndAlso comp.TrackAuthorities.Count > 0 Then
                        Else
                            BothTrackHasTAT = False
                        End If
                    Next
                    If BothTrackHasTAT Then
                        If Not DoesTNTCrossesRRXing(CompArray, CompCollection) Then OKtoProceed = False
                    Else
                        OKtoProceed = False
                    End If


                End If
                If OKtoProceed Then
                    For Each comp In CompArray
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim auth As TrackAuthority.TrackAuthority
                            If comp.TrackAuthorities.Count > 0 Then
                                For Each auth In comp.TrackAuthorities
                                    If Not AuthorityJointLimits.Contains(auth.IssueTo) Then
                                        If auth.IssueTo.ToUpper <> IssueTo Then
                                            AuthorityJointLimits.Add(auth)
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
                CompArray = Nothing
                Return AuthorityJointLimits
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Validates for all clear route.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ValidateForAllClearRoute(Optional Trk As Track = Nothing) As Boolean
            Try
                Dim CompArray As New ArrayList()
                Dim comp As Track
                CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                CompArray.Add(FindCompFromGUID(Me.RightLink))
                CompArray.Add(FindCompFromGUID(Me.LeftLink))
                For Each comp In CompArray
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If comp.TrafficDirection <> "" Then Return False
                        If comp.Occuppied Then Return False
                        If Not optDisableSignalClearCheckForTNTInRRXing AndAlso comp.TrackAndTime Then Return False 'If comp.TrackAndTime Then Return False
                        If Not optDisableSignalClearCheckForTNTInRRXing AndAlso comp.BlockAbsolute Then Return False 'If comp.BlockAbsolute Then Return False
                        If comp.SignalStatOnMe <> Track.SignalStatus.SignalAtStop Then Return False
                    End If
                Next
                If optDisableSignalClearCheckForTNTInRRXing Then
                    Dim HasMatchingTAT As Boolean = False
                    HasMatchingTAT = DoesTNTCrossesRRXing()
                    If HasMatchingTAT Then
                        Return False
                    Else
                        Return True
                    End If
                    Return False
                End If


                CompArray = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        ''' <summary>
        ''' Validates for all store route.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ValidateForAllStoreRoute() As Boolean
            Try
                Dim CompArray As New ArrayList()
                Dim comp As Track
                CompArray.Add(FindCompFromGUID(Me.RightAltLink))
                CompArray.Add(FindCompFromGUID(Me.LeftAltLink))
                CompArray.Add(FindCompFromGUID(Me.RightLink))
                CompArray.Add(FindCompFromGUID(Me.LeftLink))
                For Each comp In CompArray
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not optDisableSignalClearCheckForTNTInRRXing AndAlso comp.TrackAndTime Then Return False 'If comp.TrackAndTime Then Return False
                        If Not optDisableSignalClearCheckForTNTInRRXing AndAlso comp.BlockAbsolute Then Return False 'If comp.BlockAbsolute Then Return False
                    End If
                Next
                If optDisableSignalClearCheckForTNTInRRXing Then
                    Dim HasMatchingTAT As Boolean = False
                    HasMatchingTAT = DoesTNTCrossesRRXing()
                    If HasMatchingTAT Then
                        Return False
                    Else
                        Return True
                    End If
                    Return False
                End If


                CompArray = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Function FindAllTracksConnected() As ArrayList
            Try
                Dim TrackList As New ArrayList()
                Dim trkLeft As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftLink)
                trkLeft.FindLikeTracks(TrackList, TMDS.GlobalDeclarations.TrackCollection)

                Dim trkLeftAlt As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.LeftAltLink)
                trkLeftAlt.FindLikeTracks(TrackList, TMDS.GlobalDeclarations.TrackCollection)

                Dim trkRightAlt As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightAltLink)
                trkRightAlt.FindLikeTracks(TrackList, TMDS.GlobalDeclarations.TrackCollection)

                Dim trkRight As Track = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(Me.RightLink)
                trkRight.FindLikeTracks(TrackList, TMDS.GlobalDeclarations.TrackCollection)

                Return TrackList
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function DoesTNTCrossesRRXing(CompArray As ArrayList, ByVal CompCollection As ArrayList) As Boolean
            Dim TAT As Object = Nothing
            Dim AuthIDs As New ArrayList
            Dim RightComp As Object = Nothing
            Dim RightAltComp As Object = Nothing
            Dim LeftComp As Object = Nothing
            Dim LeftAltComp As Object = Nothing

            Dim myComps As New ArrayList
            Dim Comp1 As Object = Nothing
            Dim Comp2 As Object = Nothing
            Dim Comp As Object = Nothing
            Dim HasMatchingTAT As Boolean = False
            Try
                RightComp = FindCompFromGUID(Me.RightLink)
                If RightComp IsNot Nothing AndAlso Not myComps.Contains(RightComp) Then myComps.Add(RightComp)
                RightAltComp = FindCompFromGUID(Me.RightAltLink)
                If RightAltComp IsNot Nothing AndAlso Not myComps.Contains(RightAltComp) Then myComps.Add(RightAltComp)
                LeftComp = FindCompFromGUID(Me.LeftLink)
                If LeftComp IsNot Nothing AndAlso Not myComps.Contains(LeftComp) Then myComps.Add(LeftComp)
                LeftAltComp = FindCompFromGUID(Me.LeftAltLink)
                If LeftAltComp IsNot Nothing AndAlso Not myComps.Contains(LeftAltComp) Then myComps.Add(LeftAltComp)


                If CompCollection IsNot Nothing AndAlso CompCollection.Count > 0 Then
                    Dim Counter As Integer = 0
                    For Each Comp In myComps
                        If CompCollection.Contains(Comp) Then
                            Counter = Counter + 1
                        End If
                    Next
                    If Counter = 1 Then Return False
                End If
                If CompArray IsNot Nothing AndAlso CompArray.Count > 1 Then
                    Comp1 = CompArray(0)
                    Comp2 = CompArray(1)
                    If DoesTNTCrossesRRXingStep2(Comp1, Comp2) Then Return True
                    If Comp1 IsNot Nothing AndAlso Comp1.TrackAuthorities IsNot Nothing AndAlso Comp1.TrackAuthorities.Count > 0 Then
                        For Each TAT In Comp1.TrackAuthorities
                            If TAT.GetType.ToString = gTTComponentName Then
                                If Not AuthIDs.Contains(TAT.AuthorityNumber) Then AuthIDs.Add(TAT.AuthorityNumber)
                            End If
                        Next
                    End If
                    If (Comp2 IsNot Nothing AndAlso Comp2.TrackAuthorities IsNot Nothing AndAlso Comp2.TrackAuthorities.Count > 0) AndAlso (AuthIDs IsNot Nothing AndAlso AuthIDs.Count > 0) Then
                        For Each TAT In Comp2.TrackAuthorities
                            If TAT.GetType.ToString = gTTComponentName Then
                                If AuthIDs.Contains(TAT.AuthorityNumber) Then HasMatchingTAT = True : Exit For
                            End If
                        Next
                        Return HasMatchingTAT
                    End If
                Else
                    Return False
                End If
            Catch ex As Exception
                GetErrorDetails(ex)
                Return True
            Finally
                RightComp = Nothing
                LeftComp = Nothing
                RightAltComp = Nothing
                LeftAltComp = Nothing
                Comp1 = Nothing
                Comp2 = Nothing
            End Try
            Return True
        End Function
        Private Function DoesTNTCrossesRRXing() As Boolean
            Dim RightComp As Object = Nothing
            Dim RightAltComp As Object = Nothing
            Dim LeftComp As Object = Nothing
            Dim LeftAltComp As Object = Nothing

            Dim myComps As New ArrayList
            Dim Comp1 As Object = Nothing
            Dim Comp2 As Object = Nothing
            Dim Comp As Object = Nothing
            Dim HasMatchingTAT As Boolean = False
            Try
                RightComp = FindCompFromGUID(Me.RightLink)
                If RightComp IsNot Nothing AndAlso Not myComps.Contains(RightComp) Then myComps.Add(RightComp)
                RightAltComp = FindCompFromGUID(Me.RightAltLink)
                If RightAltComp IsNot Nothing AndAlso Not myComps.Contains(RightAltComp) Then myComps.Add(RightAltComp)
                LeftComp = FindCompFromGUID(Me.LeftLink)
                If LeftComp IsNot Nothing AndAlso Not myComps.Contains(LeftComp) Then myComps.Add(LeftComp)
                LeftAltComp = FindCompFromGUID(Me.LeftAltLink)
                If LeftAltComp IsNot Nothing AndAlso Not myComps.Contains(LeftAltComp) Then myComps.Add(LeftAltComp)

                If RightComp IsNot Nothing AndAlso LeftComp IsNot Nothing Then
                    If (RightComp.DeviceBlocks IsNot Nothing AndAlso RightComp.DeviceBlocks.Count > 0) AndAlso (LeftComp.DeviceBlocks IsNot Nothing AndAlso LeftComp.DeviceBlocks.Count > 0) Then
                        Return True
                    End If
                    Comp1 = RightComp
                    Comp2 = LeftComp
                    If DoesTNTCrossesRRXingStep2(Comp1, Comp2) Then Return True
                End If
                If RightAltComp IsNot Nothing AndAlso LeftAltComp IsNot Nothing Then
                    If (RightAltComp.DeviceBlocks IsNot Nothing AndAlso RightAltComp.DeviceBlocks.Count > 0) AndAlso (LeftAltComp.DeviceBlocks IsNot Nothing AndAlso LeftAltComp.DeviceBlocks.Count > 0) Then
                        Return True
                    End If
                    Comp1 = RightAltComp
                    Comp2 = LeftAltComp
                    If DoesTNTCrossesRRXingStep2(Comp1, Comp2) Then Return True
                End If


            Catch ex As Exception

            End Try
        End Function

        Private Function DoesTNTCrossesRRXingStep2(Comp1 As Object, comp2 As Object) As Boolean
            Dim TAT As Object = Nothing
            Dim AuthIDs As New ArrayList
            Dim HasMatchingTAT As Boolean = False
            Try
                If Comp1 IsNot Nothing AndAlso Comp1.TrackAuthorities IsNot Nothing AndAlso Comp1.TrackAuthorities.Count > 0 Then
                    For Each TAT In Comp1.TrackAuthorities
                        If TAT.GetType.ToString = gTTComponentName Then
                            If Not AuthIDs.Contains(TAT.AuthorityNumber) Then AuthIDs.Add(TAT.AuthorityNumber)
                        End If
                    Next
                End If
                If (comp2 IsNot Nothing AndAlso comp2.TrackAuthorities IsNot Nothing AndAlso comp2.TrackAuthorities.Count > 0) AndAlso (AuthIDs IsNot Nothing AndAlso AuthIDs.Count > 0) Then
                    For Each TAT In comp2.TrackAuthorities
                        If TAT.GetType.ToString = gTTComponentName Then
                            If AuthIDs.Contains(TAT.AuthorityNumber) Then HasMatchingTAT = True : Exit For
                        End If
                    Next

                End If
            Catch ex As Exception
                GetErrorDetails(ex)
                Return True
            Finally
                TAT = Nothing
                AuthIDs = Nothing

            End Try
            Return HasMatchingTAT
        End Function


    End Class



#End Region

#Region "FLIP FLOP"

    Public Class FlipFlop
        Inherits GenericComponent

        Enum SignalStatus
            SignalAtStop
            SignalRequested
            SignalCleared
            SignalInTime
            SignalUnSolicated
            PendingStateChange
            SignalRequestedDrill
            SignalClearedDrill
        End Enum

        Public TrafficDirection As String
        Public FlipMe As Boolean
        Public Occuppied As Boolean
        Public SignalStatOnMe As SignalStatus
        Public TrackName As String
        Public TrackAuthority As Boolean

        Public myTrackLeft As Track
        Public myTrackRight As Track

        Sub ApplyTraffic()

        End Sub

        Overrides Sub UpdateStatus(Optional ByVal objAuthority As Object = Nothing)

        End Sub

        Overridable Sub Draw(ByRef g As Graphics)

        End Sub

        Public Function FindExitComponent(ByVal EntryGUID As Int32) As Object
            Try
                Dim GUIDtoFind As Int32
                If EntryGUID = Me.LeftLink Then
                    GUIDtoFind = Me.RightLink
                ElseIf EntryGUID = Me.LeftAltLink Then
                    GUIDtoFind = Me.RightAltLink
                ElseIf EntryGUID = Me.RightLink Then
                    GUIDtoFind = Me.LeftLink
                ElseIf EntryGUID = Me.RightAltLink Then
                    GUIDtoFind = Me.LeftAltLink
                Else
                    Return Nothing
                End If
                Return Me.FindCompFromGUID(GUIDtoFind)

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Function FindExitGUID(ByVal EntryGUID As Int32) As Object
            Try
                Dim GUIDtoFind As Int32
                If EntryGUID = Me.LeftLink Then
                    GUIDtoFind = Me.RightLink
                ElseIf EntryGUID = Me.LeftAltLink Then
                    GUIDtoFind = Me.RightAltLink
                ElseIf EntryGUID = Me.RightLink Then
                    GUIDtoFind = Me.LeftLink
                ElseIf EntryGUID = Me.RightAltLink Then
                    GUIDtoFind = Me.LeftAltLink
                Else
                    Return Nothing
                End If
                Return GUIDtoFind

            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Private Function FindCompFromGUID(ByVal UniqueID As Int32) As Object
            Try
                Dim comp As Object
                For Each comp In TMDS.GlobalDeclarations.gComponentsCollection
                    If UniqueID = comp.GUID Then
                        Return comp
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Search For End Of Track
        ''' </summary>
        ''' <param name="pSearchRight"></param>
        ''' <param name="pEndTrack"></param>
        ''' <param name="pBeginTrack"></param>
        ''' <returns></returns>
        Public Function SearchForEndOfTrack(ByVal pSearchRight As Boolean, ByVal pEndTrack As Track, ByVal pBeginTrack As Track) As ArrayList
            Dim SelectionArrayList As New ArrayList()
            Dim CompToCheck As Object = Nothing
            Dim NextCompToCheck As Object = Nothing
            Dim LastCompToCheck As Object = Nothing
            Dim LoopControl As Integer
            Dim tk As Track
            Dim trk As Track = Nothing
            Dim TrackToCheck As Track = Nothing
            Dim SearchRightCompleted As Boolean
            Dim SearchLeftCompleted As Boolean
            Dim LikeTracks As New ArrayList()
            Dim TracksThatConnectToSwitch As ArrayList
            Try
                SystemEvent("SearchForEndOfTrack:pSearchRight=" & pSearchRight & ":pEndTrack=" & pEndTrack.Name & "-" & pEndTrack.GUID & ":pBeginTrack=" & pBeginTrack.Name & "-" & pBeginTrack.GUID)
                CompToCheck = FindExitComponent(pBeginTrack.GUID)
                If pSearchRight Then
                    Do Until SearchRightCompleted

                        If CompToCheck Is Nothing Then
                            SelectionArrayList = Nothing
                            Return SelectionArrayList
                        End If

                        SystemEvent("SearchForEndOfTrack:Comp=" & CompToCheck.GetType.ToString & "-" & CompToCheck.GUID & "-" & CompToCheck.Name)

                        If CompToCheck.GetType.ToString = "TMDS.TrackComponents.Track" Then

                            tk = CType(CompToCheck, Track)
                            If tk.NonTransverable Then
                                Return Nothing
                            End If
                            If CompToCheck.AuthorityType <> "CTC" And CompToCheck.AuthorityType <> "TWC" Then
                                SystemEvent("SearchForEndOfTrack:AuthorityType not allowed. AuthorityType=" & CompToCheck.AuthorityType & ":GUID=" & CompToCheck.GUID)
                                SelectionArrayList = Nothing
                                Return SelectionArrayList
                            End If
                            If Not SelectionArrayList.Contains(CompToCheck) Then
                                SelectionArrayList.Add(CompToCheck)
                            End If
                            NextCompToCheck = Me.FindCompFromGUID(CompToCheck.RightLink)
                            LastCompToCheck = CompToCheck

                        ElseIf CompToCheck.GetType.ToString = "TMDS.SwitchComponents.Switch" Then

                            If Not SelectionArrayList.Contains(CompToCheck) Then SelectionArrayList.Add(CompToCheck)
                            'Get the like track that connect to this switch
                            TracksThatConnectToSwitch = CompToCheck.FindAllTracksThatConnect
                            If TracksThatConnectToSwitch Is Nothing Then
                                SelectionArrayList = Nothing
                                Return SelectionArrayList
                            End If
                            trk = Nothing
                            TrackToCheck = Nothing

                            For Each trk In TracksThatConnectToSwitch
                                If optEnableRouteBlocking Then
                                    If trk.RouteBlockingEnabled Then
                                        If trk.RightLimitName = gEndTrackSelect.RightLimitName And trk.LeftLimitName = gEndTrackSelect.LeftLimitName Then
                                            If Not SelectionArrayList.Contains(trk) Then
                                                SelectionArrayList.Add(trk)
                                            End If
                                            TrackToCheck = trk
                                        End If
                                    Else
                                        If Not SelectionArrayList.Contains(trk) Then
                                            SelectionArrayList.Add(trk)
                                        End If
                                        TrackToCheck = trk
                                    End If
                                Else
                                    If Not SelectionArrayList.Contains(trk) Then
                                        SelectionArrayList.Add(trk)
                                    End If
                                    TrackToCheck = trk
                                End If
                            Next

                            LikeTracks = New ArrayList()
                            If Not IsNothing(TrackToCheck) Then
                                TrackToCheck.FindLikeTracks(LikeTracks, TrackCollection)
                                TrackToCheck.FindTouchingSwitchesFromSelectedTracks(LikeTracks, SelectionArrayList)
                            End If
                            For Each comp As Object In LikeTracks
                                If Not SelectionArrayList.Contains(comp) Then
                                    SelectionArrayList.Add(comp)
                                End If
                            Next
                            LikeTracks = Nothing
                            NextCompToCheck = CompToCheck.FindRouteThroughSwitch("RIGHT")

                        ElseIf CompToCheck.GetType.ToString = "TMDS.SignalComponents.Signal" Then

                            NextCompToCheck = Me.FindCompFromGUID(CompToCheck.RightLink)

                        ElseIf CompToCheck.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then

                            If LastCompToCheck Is Nothing Then
                                SelectionArrayList = Nothing
                                Return SelectionArrayList
                            End If
                            NextCompToCheck = CompToCheck.FindExitComponent(LastCompToCheck.GUID)

                        ElseIf CompToCheck.GetType.ToString = "TMDS.TrackOtherComponents.FlipFlop" Then

                        Else
                            Exit Do
                        End If

                        If CompToCheck.GUID = pEndTrack.GUID Then
                            Return SelectionArrayList
                        End If

                        If LoopControl > 1000 Then
                            SelectionArrayList = Nothing
                            Return SelectionArrayList
                        End If
                        LoopControl = LoopControl + 1

                        CompToCheck = NextCompToCheck
                        NextCompToCheck = Nothing
                    Loop
                Else
                    Do Until SearchLeftCompleted

                        If CompToCheck Is Nothing Then
                            SelectionArrayList = Nothing
                            Return SelectionArrayList
                        End If

                        SystemEvent("SearchForEndOfTrack:Comp=" & CompToCheck.GetType.ToString & "-" & CompToCheck.GUID & "-" & CompToCheck.Name)

                        If CompToCheck.GetType.ToString = "TMDS.TrackComponents.Track" Then

                            tk = CType(CompToCheck, Track)
                            If tk.NonTransverable Then
                                Return Nothing
                            End If
                            If Not SelectionArrayList.Contains(CompToCheck) Then
                                SelectionArrayList.Add(CompToCheck)
                            End If
                            NextCompToCheck = Me.FindCompFromGUID(CompToCheck.LeftLink)

                        ElseIf CompToCheck.GetType.ToString = "TMDS.SwitchComponents.Switch" Then

                            If Not SelectionArrayList.Contains(CompToCheck) Then SelectionArrayList.Add(CompToCheck)
                            'Get the like track that connect to this switch
                            TracksThatConnectToSwitch = CompToCheck.FindAllTracksThatConnect
                            If TracksThatConnectToSwitch Is Nothing Then
                                SelectionArrayList = Nothing
                                Return SelectionArrayList
                            End If
                            trk = Nothing
                            TrackToCheck = Nothing
                            For Each trk In TracksThatConnectToSwitch
                                If optEnableRouteBlocking Then
                                    If trk.RouteBlockingEnabled Then
                                        If trk.RightLimitName = gEndTrackSelect.RightLimitName And trk.LeftLimitName = gEndTrackSelect.LeftLimitName Then
                                            If Not SelectionArrayList.Contains(trk) Then
                                                SelectionArrayList.Add(trk)
                                            End If
                                            TrackToCheck = trk
                                        End If
                                    Else
                                        If Not SelectionArrayList.Contains(trk) Then
                                            SelectionArrayList.Add(trk)
                                        End If
                                        TrackToCheck = trk
                                    End If
                                Else
                                    If Not SelectionArrayList.Contains(trk) Then
                                        SelectionArrayList.Add(trk)
                                    End If
                                    TrackToCheck = trk
                                End If
                            Next

                            LikeTracks = New ArrayList()
                            If Not IsNothing(TrackToCheck) Then
                                TrackToCheck.FindLikeTracks(LikeTracks, TrackCollection)
                                TrackToCheck.FindTouchingSwitchesFromSelectedTracks(LikeTracks, SelectionArrayList)
                            End If
                            For Each comp As Object In LikeTracks
                                If Not SelectionArrayList.Contains(comp) Then
                                    SelectionArrayList.Add(comp)
                                End If
                            Next
                            LikeTracks = Nothing
                            NextCompToCheck = CompToCheck.FindRouteThroughSwitch("LEFT")

                        ElseIf CompToCheck.GetType.ToString = "TMDS.SignalComponents.Signal" Then

                            NextCompToCheck = Me.FindCompFromGUID(CompToCheck.LeftLink)

                        ElseIf CompToCheck.GetType.ToString = "TMDS.TrackOtherComponents.RRXING" Then

                            If LastCompToCheck Is Nothing Then
                                SelectionArrayList = Nothing
                                Return SelectionArrayList
                            End If
                            NextCompToCheck = CompToCheck.FindExitComponent(LastCompToCheck.GUID)
                        Else
                            SelectionArrayList = Nothing
                            Return SelectionArrayList
                        End If

                        If CompToCheck.GUID = gEndTrackSelect.GUID Then
                            Return SelectionArrayList
                        End If

                        If LoopControl > 1000 Then
                            SelectionArrayList = Nothing
                            Return SelectionArrayList
                        End If

                        LoopControl = LoopControl + 1
                        CompToCheck = NextCompToCheck
                        NextCompToCheck = Nothing
                    Loop
                End If

                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            Finally
                SelectionArrayList = Nothing
                CompToCheck = Nothing
                NextCompToCheck = Nothing
                LastCompToCheck = Nothing
                LoopControl = Nothing
                tk = Nothing
                trk = Nothing
                TrackToCheck = Nothing
                SearchRightCompleted = Nothing
                SearchLeftCompleted = Nothing
                LikeTracks = Nothing
                TracksThatConnectToSwitch = Nothing
            End Try
        End Function

    End Class



#End Region

#Region "REMOTE TRACK"

    Public Class RemoteTrack
        Implements IDisposable

#Region "FIELDS"

        Public thisTrack As Track
        Public thePort As Int32 = 7250
        Public theWrkStnIPString As String = "127.0.0.1"
        Public theWrkStnIP As IPAddress
        Public theRcvQueue As Queue
        Public theXmtQueue As Queue
        Public theNetworkConnecton As NetworkClient
        Public MonitorConnectionThread As Threading.Thread
        Public StopConnectionMonitoring As Boolean
        Public myLeftTrack As Track
        Public myRightTrack As Track
        Public myLastAckTime As Date = Now
        Private AcknowledgementCheck As Timers.Timer
        Private MessageProcessor As Timers.Timer
        Private ConnectedTrackGUID As Integer
        Public RouteRequestOK As Boolean
        Private RouteRequestGUID As Guid
        Private RouteRequestEvent As Boolean
        Public PromptForCodeFailureConfirmation As Boolean
        Public TagList As String = String.Empty
        Public BlockRequestOK As Boolean
        Private BlockRequestGUID As Guid
        Private BlockRequestEvent As Boolean
        Private BlockRequestConfirmationNeeded As Boolean
        Public BlockResolutionConfirmed As Boolean
        Public TrainIDRequestOK As Boolean
        Private TrainID As String
        Private TrainRearPosition As String
        Private TrainRearTrackNameAlias As String
        Private TrainRearTrackGUID As String
        Private TrainDirection As String
        Private TrainIDRequestEvent As Boolean
        Private TrainRearPositionRequestEvent As Boolean
        Public TrainsRequestOK As Boolean
        Private TrainsRequested As String
        Private TrainsRequestEvent As Boolean
        Private TrackTagRefreshLock As String = "LOCK"
        Public FlipTraffic As Boolean
        Public ResettingTrackBlocks As Boolean
        Public MessageQueue As Queue = Queue.Synchronized(New Queue)
        Public IsFalseRemoteSignalRequestEvent As Boolean
        Public IsFalseRemoteSignalRequestOK As Boolean
        Private PSSRequestGUID As Guid
        Private SetStatusMessage As String = String.Empty
        Private SetStatusTimeStamp As Date = Now

#End Region

#Region "PROPERTY"

        Private _ConnectedTrack As Track

        ''' <summary>
        ''' Connected track with remote track
        ''' </summary>
        ''' <returns>Value of <see cref="_ConnectedTrack"/></returns>
        Public Property ConnectedTrack As Track
            Get
                Return _ConnectedTrack
            End Get
            Set(ByVal value As Track)
                _ConnectedTrack = value
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="uidLink">uid link</param>
        ''' <param name="leftLink">left link</param>
        ''' <param name="rightLink">right link</param>
        ''' <param name="flipFlopLink">flip flopper link</param>
        ''' <param name="theTrack">the track that will connect with remote track</param>
        Sub New(uidLink As Integer,
                leftLink As Integer,
                rightLink As Integer,
                flipFlopLink As Integer,
                theTrack As Track)
            Try
                ConnectedTrackGUID = If(leftLink = 0, rightLink, leftLink)
                ConnectedTrack = theTrack
                thisTrack = GetRemoteTrackDataFromDb(uidLink, leftLink, rightLink)
                If thisTrack Is Nothing Then Exit Sub
                RemoteEventLog($"NEW REMOTE TRACK CREATED FOR TRACK {thisTrack.GUID}:{thisTrack.Name _
                               } WITH CONNECTED TRACK {ConnectedTrack.GUID}:{ConnectedTrack.Name} ")
                thisTrack.myControlPoint = New ControlPoint()
                If flipFlopLink <> 0 Then FlipTraffic = True
                GetTrackLocationInfo()
                AcknowledgementCheck = New Timer()
                AcknowledgementCheck.Interval = 1000
                AddHandler AcknowledgementCheck.Elapsed, AddressOf AckElapsedEvent
                MessageProcessor = New Timer()
                MessageProcessor.Interval = 100
                AddHandler MessageProcessor.Elapsed, AddressOf MessageProcessElapsedEvent
                MonitorConnectionThread = New Thread(AddressOf Me.MonitorConnection)
                MonitorConnectionThread.Priority = ThreadPriority.Normal
                MonitorConnectionThread.Name = "RemoteTrack-MonitorConnectionThread"
                MonitorConnectionThread.Start()
                AcknowledgementCheck.Start()
                MessageProcessor.Start()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#Region "IDisposable"
        Private disposed As Boolean

        Private Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    'If Not IsNothing(theNetworkConnecton) Then
                    '    theNetworkConnecton.Dispose()
                    'End If
                End If

                ' TODO: put code to free unmanaged resources here
            End If
            Me.disposed = True
            Try
                StopConnectionMonitoring = True
                StopConnectionMonitor()
                Me.SendLinkDownNotification()

                Try
                    AcknowledgementCheck.Enabled = False
                    AcknowledgementCheck.Stop()
                    AcknowledgementCheck.Dispose()
                    AcknowledgementCheck = Nothing
                Catch
                End Try
                Try
                    MessageProcessor.Enabled = False
                    MessageProcessor.Stop()
                    MessageProcessor.Dispose()
                    MessageProcessor = Nothing
                Catch ex As Exception

                End Try
                If Not Me.theNetworkConnecton Is Nothing Then
                    If Me.theNetworkConnecton.thisClient IsNot Nothing Then
                        Me.theNetworkConnecton.thisClient.Close()
                    End If
                    Me.theNetworkConnecton.CloseClientConnection()
                    theNetworkConnecton.Dispose()
                End If
                Me.theNetworkConnecton = Nothing
                Me.thisTrack = Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
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

        Private Sub AckElapsedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Try
                If Not Me.theNetworkConnecton Is Nothing Then
                    If myLastAckTime < Now.AddSeconds(-15) Then
                        Me.DebugEventLog("ACK NOT RCVD FROM REMOTE TRACK:" & myLastAckTime.ToLongTimeString & "<" & Now.AddSeconds(-15) & ":" & Me.thisTrack.GUID.ToString)
                        Me.ForceLinkDown()
                        Me.theNetworkConnecton.CloseClientConnection()
                        Me.theNetworkConnecton = Nothing
                    End If
                End If

                AcknowledgementCheck.Enabled = True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub MessageProcessElapsedEvent(ByVal source As Object, ByVal e As ElapsedEventArgs)
            Dim NewQueue As Queue = Nothing
            Try
                SyncLock Me.MessageQueue
                    If Me.MessageQueue.Count > 0 Then
                        NewQueue = Me.MessageQueue.Clone
                        Me.MessageQueue.Clear()
                    End If
                End SyncLock

                If Not IsNothing(NewQueue) Then
                    While NewQueue.Count > 0
                        Dim mess As NetworkMessageRemoteTrack = NewQueue.Dequeue
                        ProcessMessageReceived(mess.theSender, mess.theMessage)
                    End While
                End If
                NewQueue = Nothing
                Me.MessageProcessor.Enabled = True
            Catch ex As Exception
                SystemException(ex.ToString)
                If Me.MessageProcessor IsNot Nothing Then Me.MessageProcessor.Enabled = True
            End Try
        End Sub

        Private Sub StopConnectionMonitor()
            Try
                Try
                    Threading.Thread.Sleep(1000)
                    MonitorConnectionThread.Abort()
                Catch
                End Try
                MonitorConnectionThread = Nothing

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Get Remote Track Data From Db
        ''' </summary>
        ''' <param name="trackUid"></param>
        ''' <param name="leftLink"></param>
        ''' <param name="rightLink"></param>
        ''' <returns>Return remote track found</returns>
        Private Function GetRemoteTrackDataFromDb(trackUid As Integer,
                                                  leftLink As Integer,
                                                  rightLink As Integer) As Track
            Try
                Dim connectionString As String = gConnectionStringStatic
                If ConnectedTrack?.IsRemoteDatabaseLink Then
                    RemoteEventLog($"{GetCurrentMethod.Name}:Get data from remote static db to track {trackUid}")
                    connectionString = gConnectionStringRemoteDatabase
                End If
                'TO DO - MOVE IT TO REPOSITORY
                Using dbConn As New OleDbConnection(connectionString)
                    Dim query As String = $"SELECT * FROM tblCompTracks WHERE UID = {trackUid}"
                    Using dbCommand As New OleDbCommand(query, dbConn)
                        dbConn.Open()
                        Using dr As OleDbDataReader = dbCommand.ExecuteReader()
                            If Not dr.HasRows Then Return Nothing
                            dr.Read()
                            Dim RemoteTrack As New Track()
                            RemoteTrack.myRemoteTrack = Me
                            RemoteTrack.IsRemoteTrack = True
                            RemoteTrack.GUID = dr.GetInt32(0)
                            RemoteTrack.StationNumber = dr.GetInt32(1)
                            RemoteTrack.Name = dr.GetString(2)
                            RemoteTrack.ComponentType = dr.GetString(3)
                            RemoteTrack.LX = dr.GetInt16(4)
                            RemoteTrack.LY = dr.GetInt16(5)
                            RemoteTrack.RX = dr.GetInt16(6)
                            RemoteTrack.RY = dr.GetInt16(7)
                            RemoteTrack.LeftLink = leftLink
                            RemoteTrack.RightLink = rightLink
                            myLeftTrack = FetchData.FindTrackItemFromGUID(leftLink)
                            myRightTrack = FetchData.FindTrackItemFromGUID(rightLink)
                            If myLeftTrack IsNot Nothing Then myLeftTrack.RemoteLinkShowDown = True
                            If myRightTrack IsNot Nothing Then myRightTrack.RemoteLinkShowDown = True
                            RemoteTrack.LeftAltLink = dr.GetInt32(10)
                            RemoteTrack.RightAltLink = dr.GetInt32(11)
                            RemoteTrack.AltLinkOne = dr.GetInt32(12)
                            RemoteTrack.AltLinkTwo = dr.GetInt32(13)
                            RemoteTrack.CodelineNumber = dr.GetInt32(14)
                            RemoteTrack.LeftLimitMilePost = dr.GetDouble(23)
                            RemoteTrack.RightLimitMilePost = dr.GetDouble(24)
                            RemoteTrack.AuthorityType = dr.GetString(25)
                            Select Case Mid(RemoteTrack.ComponentType, 2)
                                Case "R"
                                    RemoteTrack.ItemSevenAllowedOnRightLimits = True
                                    RemoteTrack.ComponentType = "T"
                                Case "L"
                                    RemoteTrack.ItemSevenAllowedOnLeftLimits = True
                                    RemoteTrack.ComponentType = "T"
                                Case "&"
                                    RemoteTrack.TrackIndicationANDED = True
                                    RemoteTrack.TrackIndicationORED = False
                                Case "+"
                                    RemoteTrack.TrackIndicationANDED = False
                                    RemoteTrack.TrackIndicationORED = True
                            End Select
                            RemoteTrack.SimulateMode = gSimulationMode
                            If Mid(RemoteTrack.AuthorityType, 1, 3) = "CTC" Then
                                gCTCTerritoryFound = True
                            ElseIf RemoteTrack.AuthorityType = "TWC-BABS" Then
                                RemoteTrack.AuthorityType = Mid(RemoteTrack.AuthorityType, 1, 3)
                                RemoteTrack.BiDirectionalABS = True
                            ElseIf RemoteTrack.AuthorityType = "TWC-ABSW" Then
                                RemoteTrack.AuthorityType = Mid(RemoteTrack.AuthorityType, 1, 3)
                                RemoteTrack.SingleABSWest = True
                            ElseIf RemoteTrack.AuthorityType = "TWC-ABSE" Then
                                RemoteTrack.AuthorityType = Mid(RemoteTrack.AuthorityType, 1, 3)
                                RemoteTrack.SingleABSEast = True
                            ElseIf RemoteTrack.AuthorityType = "SHELF" Then
                                RemoteTrack.IsShelfTrack = True
                            End If
                            If Not RemoteTrack.Ind(0) Is Nothing Then
                                RemoteTrack.Indicating = True
                            ElseIf Mid(RemoteTrack.AuthorityType, 1, 3) = "CTC" Then
                                RemoteTrack.Indicating = True
                                RemoteTrack.Ind(0) = New BitPosition()
                                RemoteTrack.Ind(1) = New BitPosition()
                            Else
                                RemoteTrack.FailState = False
                            End If
                            If Not RemoteTrack.SimulateMode And RemoteTrack.Indicating Then
                                RemoteTrack.FailState = True
                            End If
                            RemoteTrack.TrackName = dr.GetString(26)
                            RemoteTrack.LeftLimitName = dr.GetString(27)
                            RemoteTrack.RightLimitName = dr.GetString(28)
                            RemoteTrack.TerritoryID = dr.GetInt32(29)
                            RemoteTrack.SubdivisionID = dr.GetInt32(30)
                            RemoteTrack.SlottingTrack = dr.GetBoolean(31)
                            RemoteTrack.TrackLength = dr.GetInt32(32)
                            RemoteTrack.AllowBlocking = dr.GetBoolean(33)
                            RemoteTrack.AllowFieldBlocking = dr.GetBoolean(34)
                            RemoteTrack.AllowFoulTime = dr.GetBoolean(35)
                            RemoteTrack.AllowTrackPermit = dr.GetBoolean(36)
                            RemoteTrack.AllowTrackTime = dr.GetBoolean(37)
                            RemoteTrack.AllowTraffic = dr.GetBoolean(38)
                            RemoteTrack.TimeZone = dr.GetString(41)
                            RemoteTrack.OSReporting = dr.GetBoolean(45)
                            If Not dr.IsDBNull(46) Then RemoteTrack.GraphicalGroupID = dr.GetByte(46)
                            If Not dr.IsDBNull(47) Then RemoteTrack.ScreenRegionID = dr.GetByte(47)
                            If dr.FieldCount >= 100 Then
                                If Not dr.IsDBNull(99) Then
                                    Dim TrackNameAlias As String = dr.GetString(99)
                                    Select Case Trim(TrackNameAlias)
                                        Case "NOTHING", "NONE", ""
                                            TrackNameAlias = RemoteTrack.TrackName
                                    End Select
                                    RemoteTrack.TrackNameAlias = TrackNameAlias
                                End If
                            End If
                            If dr.FieldCount >= 114 AndAlso
                            Not dr.IsDBNull(113) Then
                                RemoteTrack.TypeOfPTC = dr.GetString(113)
                            End If
                            gComponentsCollection.Add(RemoteTrack)
                            TrackCollection.Add(RemoteTrack)
                            RemoteEventLog($"{GetCurrentMethod.Name}:REMOTE TRACK INFO RETREIVED:{RemoteTrack.Name _
                                       }:{RemoteTrack.GUID}:{RemoteTrack.StationNumber}")
                            Return RemoteTrack
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
            Return Nothing
        End Function

        ''' <summary>
        ''' Get Track Location Info
        ''' </summary>
        Private Sub GetTrackLocationInfo()
            Try
                Dim connectionString As String = gConnectionStringStatic
                If ConnectedTrack?.IsRemoteDatabaseLink Then connectionString = gConnectionStringRemoteDatabase
                Dim query As String = $"SELECT WorkStationIPString 
                                            FROM tblTerritoryList WHERE UID = {thisTrack.TerritoryID}"
                If optEnableShadowMode Then
                    query = $"Select ControllingWorkStationIPString 
                                    from tblCompControlPoints where uid={thisTrack.StationNumber}"
                End If
                'TO DO - MOVE IT TO REPOSITORY
                Using dbConn As New OleDbConnection(connectionString)
                    Using dbCommand As New OleDbCommand(query, dbConn)
                        dbConn.Open()
                        Using dr As OleDbDataReader = dbCommand.ExecuteReader()
                            If Not dr.HasRows Then Exit Sub
                            dr.Read()
                            If Not dr.IsDBNull(0) Then theWrkStnIPString = dr.GetString(0)
                            SetIpAddressWorkstation()
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Function CheckForAuthorityExists(ByVal AuthGUID As String) As Boolean
            Dim dr As OleDbDataReader
            Dim dbCommand As OleDbCommand
            Dim dbConn As OleDbConnection
            Dim FetchWrkstn As String = "SELECT AuthGUID FROM tblAuthoritiesActive WHERE AuthGUID = '" & AuthGUID & "'"
            Dim AuthExists As Boolean
            Try
                dbConn = New OleDbConnection(TMDS.GlobalDeclarations.gConnectionStringDynamic)
                dbCommand = New OleDbCommand(FetchWrkstn, dbConn)
                dbConn.Open()
                dr = dbCommand.ExecuteReader()
                While dr.Read
                    If Not dr.IsDBNull(0) Then
                        If AuthGUID = dr.GetString(0) Then
                            AuthExists = True
                        End If
                    End If
                End While
                dbConn.Close()
                dr = Nothing
                dbConn = Nothing
                dbCommand = Nothing
                Return AuthExists
            Catch ex As Exception
                SystemException(ex.ToString)
                Return True
            End Try
        End Function

        ''' <summary>
        ''' Set Ip Address Workstation
        ''' </summary>
        Private Sub SetIpAddressWorkstation()
            Try
                If Not String.IsNullOrEmpty(theWrkStnIPString) AndAlso
                    Not theWrkStnIPString.Equals("NONE") Then
                    RemoteEventLog($"{GetCurrentMethod.Name}:REMOTE TRACK LOCATION IP ADDRESS:{theWrkStnIPString}")
                    theWrkStnIP = IPAddress.Parse(theWrkStnIPString)
                Else
                    theWrkStnIP = IPAddress.Parse("127.0.0.1")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub MonitorConnection()
            Try
                Do Until StopConnectionMonitoring
                    CheckConnection()
                    UpdateRemoteLinkStatus()
                    Threading.Thread.Sleep(2000)
                Loop
                StopConnectionMonitoring = False
            Catch ex As Exception
                'SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub CheckConnection()
            Try
                If Me.theNetworkConnecton Is Nothing Then
                    CreateConnectionToListener()
                    Exit Sub
                End If

                If Not Me.theNetworkConnecton.SendPoll("REMOTELINK;POLL" & Chr(4)) Then
                    Me.theNetworkConnecton.ConnectionVerify = False
                    CreateConnectionToListener()
                Else
                    'Me.SendTrackStatus()
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        'This is for Build 684.0
        Public Sub UpdateRemoteLinkStatus()

            'BUILD 684.0 P1-5074
            Try
                'First check to see if we have connection to the remote TMDS workstation
                Dim linkIsDown As Boolean = False
                If Not IsNothing(theNetworkConnecton) Then
                    If Me.theNetworkConnecton.ConnectionVerify Then

                        'we are connected to the remote track, so make sure this GUID is not in the LinkDown list
                        For Each i As Integer In gTracksDown

                            If i = Me.ConnectedTrackGUID Then

                                gTracksDown.Remove(i)
                                Exit For
                            End If

                        Next

                    Else

                        linkIsDown = True

                    End If
                Else

                    linkIsDown = True

                End If

                'Check if the link was down...
                If linkIsDown Then

                    'we are NOT connected to the remote track, so make sure this GUID is IN the LinkDown List
                    Dim remoteLinkFound As Boolean = False
                    For Each i As Integer In gTracksDown

                        If i = Me.ConnectedTrackGUID Then

                            remoteLinkFound = True
                            Exit For
                        End If

                    Next

                    'If it was not found, we need to add it to the list
                    If Not remoteLinkFound Then
                        gTracksDown.Add(Me.ConnectedTrackGUID)
                    End If

                End If

            Catch ex As Exception
                SystemException(ex.Message & " - " & ex.StackTrace)

            End Try
            'END 684.0 P1-5074

        End Sub

        Public Function CreateConnectionToListener() As Boolean
            Try
                GetTrackLocationInfo()
                If Me.theWrkStnIPString <> "" And Me.theWrkStnIPString <> "NONE" And Me.theWrkStnIPString <> "127.0.0.1" Then

                    If Not Me.theNetworkConnecton Is Nothing Then
                        Me.theNetworkConnecton.CloseClientConnection()
                        If Me.theNetworkConnecton IsNot Nothing Then Me.theNetworkConnecton.Dispose()
                    End If
                    Me.RemoteEventLog("CREATING NETWORK CONNECTION FOR TRACK:" & Me.thisTrack.GUID & " @ " & theWrkStnIP.ToString & ":" & thePort.ToString)
                    Me.theNetworkConnecton = New NetworkClient(thePort, theWrkStnIP.ToString)
                    ' AddHandler theNetworkConnecton.LineReceived, AddressOf Me.ProcessMessageReceived
                    AddHandler theNetworkConnecton.LinkChange, AddressOf Me.LinkStatusChange
                    Me.theNetworkConnecton.thisTrack = Me
                    Me.theNetworkConnecton.UseForRemoteTrack = True
                End If

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;CONNECT;" & gThisWorkstation.Name & ";" & gThisWorkstation.WrkStnNumber & ";" & Me.thisTrack.GUID.ToString & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.RemoteEventLog(Message)
                    Return True
                End If
            Catch ex As Exception
                SystemException(ex.ToString)

                Me.theNetworkConnecton = Nothing
                ForceLinkDown()
            End Try
        End Function

        Public Sub ForceLinkDown()
            Try
                Me.RemoteEventLog("REMOTE TRACK LINK FORCED DOWN:" & Me.thisTrack.GUID.ToString)

                If Not Me.myLeftTrack Is Nothing Then
                    If Me.myLeftTrack.RemoteLinkShowDown <> True Then
                        Me.myLeftTrack.RemoteLinkShowDown = True
                        Me.myLeftTrack.UpdateStatus()
                    End If
                End If
                If Not Me.myRightTrack Is Nothing Then
                    If Me.myRightTrack.RemoteLinkShowDown <> True Then
                        Me.myRightTrack.RemoteLinkShowDown = True
                        Me.myRightTrack.UpdateStatus()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub LinkStatusChange(ByVal sender As NetworkClient, ByVal LinkConnected As Boolean, ByVal LastAckTime As Date)
            Try
                myLastAckTime = Now 'LastAckTime
                'Me.DebugEvent("STATUS RCV: LinkConnected=" & LinkConnected & " LastAckTime:" & myLastAckTime.ToLongTimeString & " TRACK:" & Me.thisTrack.GUID.ToString)
                If Me.thisTrack.RemoteLinkConnected <> LinkConnected Then
                    Me.thisTrack.RemoteLinkConnected = LinkConnected
                    Me.RefreshMe()
                    Me.RefreshMyLinkTracks()
                    Me.thisTrack.UpdateStatus()
                    Me.RemoteEventLog("REMOTE TRACK LINK STATUS CHANGE: LinkConnected=" & LinkConnected & " LastAckTime:" & LastAckTime.ToLongTimeString)
                End If

                Dim LinkDown As Boolean
                If Not LinkConnected Then
                    LinkDown = True
                Else
                    LinkDown = False
                End If
                If Not Me.myLeftTrack Is Nothing Then
                    If Me.myLeftTrack.RemoteLinkShowDown <> LinkDown Then
                        Me.RemoteEventLog("REMOTE TRACK LINK STATUS:" & Me.myLeftTrack.RemoteLinkShowDown & " ME:" & Me.thisTrack.GUID & "myLeftTrack:" & myLeftTrack.GUID)
                        Me.myLeftTrack.RemoteLinkShowDown = LinkDown
                        If Not LinkDown Then
                            Threading.Thread.Sleep(1000)
                            Me.myLeftTrack.myControlPoint.InitializeSignalRoutesOverFailedLink()
                        Else
                            Me.myLeftTrack.TrafficDirection = ""
                            Me.myLeftTrack.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                        End If
                        Me.myLeftTrack.UpdateStatus()
                    End If
                End If
                If Not Me.myRightTrack Is Nothing Then
                    If Me.myRightTrack.RemoteLinkShowDown <> LinkDown Then
                        Me.RemoteEventLog("REMOTE TRACK LINK STATUS:" & Me.myRightTrack.RemoteLinkShowDown & " ME:" & Me.thisTrack.GUID & "myRightTrack:" & myRightTrack.GUID)
                        Me.myRightTrack.RemoteLinkShowDown = LinkDown
                        Me.myRightTrack.UpdateStatus()

                        If Not LinkDown Then
                            Threading.Thread.Sleep(1000)
                            Me.myRightTrack.myControlPoint.InitializeSignalRoutesOverFailedLink()
                        Else
                            Me.myRightTrack.TrafficDirection = ""
                            Me.myRightTrack.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                        End If
                        Me.myRightTrack.UpdateStatus()
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Delegate Sub DoProcessStatusMessage(ByVal datastring As String)

        Public Sub ProcessMessageReceived(ByVal sender As NetworkClient, ByVal data As String)

            RemoteEventLog($"RCV REMOTE TRACK DATA:{data}")
            Dim MessageRcvd() As String = data.Split(Chr(4))
            Dim mess As String
            Try
                For Each mess In MessageRcvd
                    If mess = "" Then Exit Sub
                    Dim dataArray() As String
                    ' Message parts are divided by "|"  Break the string into an array accordingly.
                    dataArray = mess.Split(";")
                    myLastAckTime = Now
                    'Me.TrackEvent(mess)
                    Select Case dataArray(0)
                        Case "REMOTELINK"
                            Me.RemoteEventLog("REMOTE TRACK DATA RECEIVED:" & Me.thisTrack.GUID.ToString & " (" & mess & ")")

                            Select Case dataArray(1)
                                Case "ROUTEREQUEST"
                                    Me.RouteRequestOK = False
                                    Me.PromptForCodeFailureConfirmation = False
                                    Me.TagList = String.Empty
                                    If dataArray(2) = Me.RouteRequestGUID.ToString.ToUpper Then
                                        If dataArray(3) = "TRUE" Then
                                            Me.RouteRequestOK = True
                                        End If
                                        If dataArray.Length >= 5 Then
                                            Select Case dataArray(4)
                                                Case "PROMPTFORCODEFAILURE"
                                                    PromptForCodeFailureConfirmation = True
                                            End Select
                                        End If
                                        If dataArray.Length >= 6 Then
                                            Me.TagList = dataArray(5)
                                        End If
                                    End If
                                    RouteRequestEvent = True
                                Case "BLOCKREQUEST"
                                    Me.BlockRequestOK = False
                                    If dataArray.Length >= 6 Then
                                        Me.BlockRequestConfirmationNeeded = CBool(dataArray(5))
                                    End If
                                    If dataArray(2) = Me.BlockRequestGUID.ToString.ToUpper Then
                                        'If dataArray(3) = "TRUE" Then
                                        Me.BlockRequestOK = True
                                        ''Me.BlockRequestNonControllingRemoteTracks = ""
                                        ''If dataArray.Length >= 7 Then
                                        ''    Me.BlockRequestNonControllingRemoteTracks = dataArray(6)
                                        ''End If
                                        'End If
                                    Else
                                        Me.BlockRequestOK = False
                                    End If
                                    BlockRequestEvent = True
                                Case "TRAINIDREQUEST"
                                    TrainID = dataArray(2)
                                    If TrainID.Trim <> "" Then
                                        TrainIDRequestEvent = True
                                        TrainDirection = dataArray(3)
                                        Me.RemoteEventLog("TRAINIDREQUEST: TRAIN RECEIVED FROM REMOTE DESK:" & TrainID.Trim)
                                    End If
                                Case "TRAINIDREQUESTED"
                                    TrainID = dataArray(2)
                                    If TrainID.Trim <> "" Then
                                        TrainIDRequestEvent = True
                                        TrainDirection = dataArray(3)
                                        Me.RemoteEventLog("TRAINIDREQUESTED: TRAIN RECEIVED FROM REMOTE DESK:" & TrainID.Trim)
                                    End If
                                Case "TRAINREARPOSITION"
                                    TrainRearPosition = ""
                                    TrainRearPosition = dataArray(2)
                                    TrainRearPositionRequestEvent = True
                                Case "TRAINSREQUESTED"
                                    TrainsRequested = ""
                                    TrainsRequested = dataArray(2)
                                    If TrainsRequested <> "" Then
                                        TrainsRequestEvent = True
                                    End If
                                Case "REQSTATUS"

                                Case "TAGRESET"
                                    Me.ResetTrackTags()
                                Case "SETSTATUS"
                                    Me.ReceiveRemoteLinkTrackStatus(dataArray(2))
                                Case "PSSRESPONSE"
                                    If dataArray(2) = Me.PSSRequestGUID.ToString.ToUpper Then
                                        IsFalseRemoteSignalRequestOK = CBool(dataArray(3))
                                        Me.IsFalseRemoteSignalRequestEvent = True
                                    End If
                                Case "SIGNALAUTHORIZATIONRESPONSE"
                                    SignalAuthorizationResponse(mess)
                                Case "TRACKNAMERESPONSE"
                                    TrackNameResponse(mess)
                                Case "DISCONNECT"
                                    Me.theNetworkConnecton.CloseClientConnection()
                            End Select
                        Case "ACK"
                            sender.ConnectionVerify = True
                            LinkStatusChange(sender, True, Now)
                        Case Else
                            Me.RemoteEventLog("UNKNOWN REMOTE TRACK DATA RECIEVED:" & data)
                    End Select
                Next

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Name Line Response
        ''' </summary>
        ''' <param name="theMessage">Received Message</param>
        ''' <remarks>Creates Alert and Toggles Menu Options After Name Line Response is Received</remarks>
        Public Sub TrackNameResponse(theMessage As String)
            Try
                Dim theData() As String = theMessage.Split(";")
                Dim theSignal As Signal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(CInt(theData(2)))
                Dim theResponse As Boolean = CBool(theData(3))
                Dim chosenLine As String = theData(4)
                Dim responseMessage As String
                If theSignal IsNot Nothing AndAlso theResponse Then
                    responseMessage = $"{optSignalAuth37}{chosenLine}{optSignalAuth38}"
                    AlertUtilities.CreateAlert(Alert.AlertType.JoinTrackNameAcceptedResponse, responseMessage)
                    theSignal.ToggleTrackNameOption(theResponse, theSignal)
                Else
                    responseMessage = optSignalAuth39
                    AlertUtilities.CreateAlert(Alert.AlertType.JoinTrackNameDeniedResponse, responseMessage)
                    theSignal.ToggleTrackNameOption(theResponse, theSignal)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Signal auth response
        ''' </summary>
        ''' <param name="pMessage"></param>
        Public Sub SignalAuthorizationResponse(ByVal pMessage As String)
            Dim data() As String
            Dim theSignal As TMDS.SignalComponents.Signal
            Dim theResponse As Boolean
            Dim sr As New ArrayList
            Dim filterListForDuplicates As New ArrayList
            Dim trn As TrainID
            Dim theSignalRoute2 As SignalRoute = Nothing
            Try
                data = pMessage.Split(";")
                theSignal = TMDS.SystemUtilities.FetchData.FindSignalFromGUID(CInt(data(2)))
                theResponse = CBool(data(3))
                If Not IsNothing(theSignal) Then
                    If theResponse Then
                        Me.RemoteEventLog("SignalAuthorizationResponse:Authorized")
                        If Not IsNothing(theSignal.gJointLicenseForTrainTrain) Then
                            Me.RemoteEventLog("SignalAuthorizationResponse:gJointLicenseForTrainTrain:" & theSignal.gJointLicenseForTrainTrain.Name)
                        End If
                        If Not IsNothing(gCreateLicenseTrain) Then
                            Me.RemoteEventLog("SignalAuthorizationResponse:gCreateLicenseTrain:" & gCreateLicenseTrain.Name)
                            ClearSystemVariables()
                        End If
                        If Not IsNothing(theSignal.gJointLicenseForTrainTrain) AndAlso Not theSignal.RequestJointAuthForLicenseClick Then
                            If theSignal.RequestJointAuthForPSS Then
                                trn = theSignal.gJointLicenseForTrainTrain
                                theSignal.JointControlAuthorizationOfficeModeConfirmed = True
                                theSignal.JointControlAuthorizationUpdateStatus()
                                theSignal.gJointLicenseForTrainTrain.WaitingForJointAuthResponse = False
                                trn.ShowDialogToPassRedSignal_MRS(theSignal, trn.RequestJointAuthForPSSEntryTrack, trn.RequestJointAuthForPSSExitTrack, False)
                                Me.RemoteEventLog("SignalAuthorizationResponse:JointAuthorization:PSS")
                            ElseIf theSignal.gJointLicenseForTrainTrain.RequestJointAuthForLicnese Then
                                theSignal.JointControlAuthorizationOfficeModeConfirmed = True
                                theSignal.JointControlAuthorizationUpdateStatus()
                                gEntExtExecutionSelection.Add(theSignal)
                                theSignal.gJointLicenseForTrainTrain.WaitingForJointAuthResponse = False
                                Me.RemoteEventLog("SignalAuthorizationResponse:AddgEntExtExecutionSelection:" & theSignal.Name)
                                If theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Count = 0 Then
                                    If Not theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Contains(theSignal) Then
                                        theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Add(theSignal)
                                        Me.RemoteEventLog("SignalAuthorizationResponse:1")
                                    End If
                                ElseIf theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Count >= 1 Then
                                    If Not theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Contains(theSignal) Then
                                        theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Insert(0, theSignal)
                                        theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Add(theSignal)
                                        Me.RemoteEventLog("SignalAuthorizationResponse:2")
                                    End If
                                    For Each comp As Object In theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding
                                        If Not filterListForDuplicates.Contains(comp) Then
                                            filterListForDuplicates.Add(comp)
                                        End If
                                    Next
                                    CopyArrayCollection(theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding, filterListForDuplicates)
                                End If
                                If theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Count >= 1 Then
                                    Me.RemoteEventLog("SignalAuthorizationResponse:3")
                                    For Each sig As Signal In theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding
                                        If gEntExtExecutionSelection.Count > 0 Then
                                            If Not gEntExtExecutionSelection.Contains(sig) Then
                                                gEntExtExecutionSelection.Add(sig)
                                                Me.RemoteEventLog("SignalAuthorizationResponse:sig1:" & sig.Name)
                                            End If
                                        Else
                                            gEntExtExecutionSelection.Add(sig)
                                            Me.RemoteEventLog("SignalAuthorizationResponse:sig2:" & sig.Name)
                                        End If
                                        If Not IsNothing(gStoredRouteList) Then
                                            If gStoredRouteList.Count > 0 Then
                                                Dim comp2 As Object
                                                For Each comp2 In gStoredRouteList
                                                    If comp2.GetType.ToString = gSignalComponentName Then
                                                        Dim checksig As Signal = CType(comp2, Signal)
                                                        If checksig.StoredEntrySignal Or checksig.StoredExitSignal Then
                                                            Exit Sub
                                                        End If
                                                    End If
                                                Next
                                            End If
                                        End If
                                        theSignalRoute2 = GetSignalRoute(sig)
                                        Me.RemoteEventLog("SignalAuthorizationResponse:sig3:" & sig.Name)
                                        If Not IsNothing(gStoredRouteList) Then
                                            If gStoredRouteList.Count = 0 Then
                                                CopyArrayCollection(gStoredRouteList, theSignalRoute2.ComponentList)
                                            End If
                                        Else
                                            CopyArrayCollection(gStoredRouteList, theSignalRoute2.ComponentList)
                                        End If
                                        gEntryTrack = theSignalRoute2.EntryTrack
                                        gExitTrack = theSignalRoute2.ExitTrack
                                        For Each comp As Object In theSignalRoute2.ComponentList
                                            If Not gStoredRouteList.Contains(comp) Then
                                                gStoredRouteList.Add(comp)
                                                Me.RemoteEventLog("SignalAuthorizationResponse:4")
                                            End If
                                        Next
                                    Next
                                    theSignal.gJointLicenseForTrainTrain.SignalListForJointAuthRouteBuilding.Clear()
                                Else
                                    gEntExtExecutionSelection.Clear()
                                End If
                                TMDS.SystemForms.MySystemForms.frmMainMDI.PtcIssueLicense(gEntExtTrackSelection,
                                                                                          gEntExtExecutionSelection,
                                                                                          gSearchDirection,
                                                                                          gDoStoredRoute,
                                                                                          gStoredRouteList,
                                                                                          True,
                                                                                          theSignal.gJointLicenseForTrainTrain,
                                                                                          False,
                                                                                          theSignalRoute2.myDirection, True)
                            Else
                                theSignal.JointControlAuthorizationOfficeModeConfirmed = True
                                theSignal.JointControlAuthorizationUpdateStatus()
                            End If
                        Else
                            theSignal.JointControlAuthorizationOfficeModeConfirmed = True
                            theSignal.JointControlAuthorizationUpdateStatus()
                        End If
                    Else
                        Me.RemoteEventLog("SignalAuthorizationResponse:Denied")
                        theSignal.JointControlAuthorizationOff(True)
                    End If
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                data = Nothing
                theSignal = Nothing
                theResponse = Nothing
                sr = Nothing
                filterListForDuplicates = Nothing
                trn = Nothing
                theSignalRoute2 = Nothing
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
                    If theExitSignal = Nothing Then
                        theExitSignal = theSignal
                    End If
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
                                If NextComp.LeftLink = 0 Then
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftAltLink)
                                    If NextComp = Nothing Then
                                        Return Nothing
                                    End If
                                Else
                                    NextComp = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(NextComp.LeftLink)
                                End If

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

        Private Sub RefreshMe()
            Me.ResetTrackTags()
            ResetTrackBlocks()
        End Sub

        Public Function SendRequestGetRearOfTrain(Optional ByVal Direction As String = "") As String
            Try

                If Not Me.theNetworkConnecton Is Nothing Then
                    Me.DebugEventLog("SendRequestGetRearOfTrain:" & Me.thisTrack.GUID.ToString)

                    Dim Message As String = "REMOTELINK;GETREARPOSITIONOFTRAIN;" & Me.thisTrack.GUID.ToString & "|" & Direction & "|" & thisTrack.TrackNameAlias
                    Me.theNetworkConnecton.SendData(Message, True)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)

                    TrainRearPosition = ""
                    Dim WaitTime As Date = Now.AddSeconds(-3)
                    Do While WaitTime > Now.AddSeconds(-10)
                        'Application.DoEvents()
                        If TrainRearPositionRequestEvent Then
                            TrainRearPositionRequestEvent = False
                            Dim TrainPosition As String = TrainRearPosition
                            'Dim TrackNameAlias As String = TrainRearTrackNameAlias
                            Me.DebugEventLog("TRAIN POSITION REQUEST RETURNED:" & TrainPosition)
                            ' Return TrainPosition & ";" & TrackNameAlias & ";" & TrainRearTrackGUID
                            Return TrainPosition
                        End If
                    Loop

                Else
                    Me.DebugEventLog("SendRequestGetRearOfTrain: Network Connetion is nothing exiting Function")
                    Return Nothing
                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                TrainIDRequestEvent = False
                Return Nothing
            End Try

        End Function

        Public Sub ResetTrackBlocks()
            If ResettingTrackBlocks Then Exit Sub
            Try
                ResettingTrackBlocks = True
                Dim Rmttrk As Track = Nothing
                Me.RemoteEventLog("ResetTrackBlocks:" & Me.thisTrack.GUID.ToString)

                Dim BlockList As New ArrayList()
                Dim AuthList As New ArrayList()

                Dim dv As DeviceBlock = Nothing
                Dim BlockArray As ArrayList = Nothing

                If Not IsNothing(Me.myLeftTrack) Then
                    Rmttrk = Me.myLeftTrack
                    BlockArray = Me.myLeftTrack.DeviceBlocks
                ElseIf Not IsNothing(Me.myRightTrack) Then
                    Rmttrk = Me.myRightTrack
                    BlockArray = Me.myRightTrack.DeviceBlocks
                End If
                If Not IsNothing(BlockArray) Then
                    For Each dv In BlockArray
                        BlockList.Add(dv.AuthorityUID)
                        gDeviceBlocks.Remove(dv)
                        Dim SummItem As New SummaryItem("BLOCK", "REMOVE", dv)
                        SyncLock gSummaryBlockQueue.SyncRoot
                            gSummaryBlockQueue.Enqueue(SummItem)
                        End SyncLock
                    Next
                End If

                'For Each dv In Me.thisTrack.DeviceBlocks
                '    BlockList.Add(dv.AuthorityUID)
                '    TMDS.GlobalDeclarations.gDeviceBlocks.Remove(dv)
                '    Dim SummItem As New SummaryItem("BLOCK", "REMOVE", dv)
                '    SyncLock gSummaryBlockQueue.SyncRoot
                '        gSummaryBlockQueue.Enqueue(SummItem)
                '    End SyncLock
                'Next

                Dim auth As Object = Nothing 'TrackAuthority.TrackAuthority
                Dim AuthArray As ArrayList = Nothing
                Dim AuthDeleteArray As ArrayList = Nothing

                If Not IsNothing(Me.myLeftTrack) Then
                    AuthArray = Me.myLeftTrack.TrackAuthorities
                ElseIf Not IsNothing(Me.myRightTrack) Then
                    AuthArray = Me.myRightTrack.TrackAuthorities
                End If

                If Not IsNothing(AuthArray) Then
                    For Each auth In AuthArray
                        If auth.GetType.ToString = "TMDS.TrackAuthority.TrackAuthority" Then
                            Dim AuthExists As Boolean = CheckForAuthorityExists(auth.AuthGUID)
                            If AuthExists Then
                                AuthList.Add(auth.AuthorityUID)
                                If auth.ComponentsInSelection.Count = 1 Then
                                    OtherTrackAuthorities.Remove(auth)
                                    Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", auth)
                                    SyncLock gSummaryAuthQueue.SyncRoot
                                        gSummaryAuthQueue.Enqueue(SummItem)
                                    End SyncLock
                                End If
                            Else
                                If Not IsNothing(Me.myLeftTrack) Then
                                    Me.myLeftTrack.RemoveTrackAuthority(auth.AuthorityUID, False, False, Nothing)
                                ElseIf Not IsNothing(Me.myRightTrack) Then
                                    Me.myRightTrack.RemoveTrackAuthority(auth.AuthorityUID, False, False, Nothing)
                                End If
                                'OtherTrackAuthorities.Remove(auth)
                                'Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", auth)
                                'SyncLock gSummaryAuthQueue.SyncRoot
                                '    gSummaryAuthQueue.Enqueue(SummItem)
                                'End SyncLock
                            End If
                        Else
                            AuthList.Add(auth.AuthorityUID)
                            Select Case auth.GetType.ToString
                                Case gSFAComponentName
                                    If auth.Components.Count = 1 Then
                                        OtherTrackAuthorities.Remove(auth)
                                        Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", auth)
                                        SyncLock gSummaryAuthQueue.SyncRoot
                                            gSummaryAuthQueue.Enqueue(SummItem)
                                        End SyncLock
                                    End If
                                Case gTTComponentName
                                    If auth.ComponentsInSelection.Count = 1 Then
                                        OtherTrackAuthorities.Remove(auth)
                                        Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", auth)
                                        SyncLock gSummaryAuthQueue.SyncRoot
                                            gSummaryAuthQueue.Enqueue(SummItem)
                                        End SyncLock
                                    End If
                                Case Else
                                    If auth.ComponentsInSelection.Count = 1 Then
                                        OtherTrackAuthorities.Remove(auth)
                                        Dim SummItem As New SummaryItem("AUTHORITY", "REMOVE", auth)
                                        SyncLock gSummaryAuthQueue.SyncRoot
                                            gSummaryAuthQueue.Enqueue(SummItem)
                                        End SyncLock
                                    End If
                            End Select


                        End If
                    Next
                End If

                If Not Me.myLeftTrack Is Nothing Then
                    Me.myLeftTrack.TrackAuthorities.Clear()
                    Me.myLeftTrack.DeviceBlocks.Clear()
                End If
                If Not Me.myRightTrack Is Nothing Then
                    Me.myRightTrack.TrackAuthorities.Clear()
                    Me.myRightTrack.DeviceBlocks.Clear()
                End If

                'Me.thisTrack.TrackAuthorities.Clear()
                'Me.thisTrack.DeviceBlocks.Clear()

                If optApplyConfigurationForKCT Then
                    If BlockList IsNot Nothing AndAlso BlockList.Count = 0 Then
                        For Each dv In gDeviceBlocks
                            If dv IsNot Nothing AndAlso dv.ComponentsInSelection.Contains(Rmttrk) Then
                                If Not BlockList.Contains(dv.AuthorityUID) Then BlockList.Add(dv.AuthorityUID)
                            End If
                        Next
                    End If
                    If AuthList IsNot Nothing AndAlso AuthList.Count = 0 Then
                        For Each auth In OtherTrackAuthorities
                            If auth IsNot Nothing AndAlso auth.GetType.ToString = gTTComponentName Then
                                If auth.ComponentsInSelection.Contains(Rmttrk) Then
                                    If Not AuthList.Contains(auth.AuthorityUID) Then AuthList.Add(auth.AuthorityUID)
                                End If
                            End If
                        Next
                    End If
                End If

                Dim UID As String
                For Each UID In BlockList
                    If Not Me.myLeftTrack Is Nothing Then
                        Me.myLeftTrack.ApplyDeviceBlock(UID)
                    End If
                    If Not Me.myRightTrack Is Nothing Then
                        Me.myRightTrack.ApplyDeviceBlock(UID)
                    End If
                Next
                For Each UID In AuthList
                    If Not Me.myLeftTrack Is Nothing Then
                        Me.myLeftTrack.ApplyTrackAuthority(UID)
                    End If
                    If Not Me.myRightTrack Is Nothing Then
                        Me.myRightTrack.ApplyTrackAuthority(UID)
                    End If
                Next
                ResettingTrackBlocks = False
            Catch ex As Exception
                SystemException(ex.ToString)
                ResettingTrackBlocks = False
            End Try
        End Sub

        Public Sub ResetTrackTags()
            Try
                SyncLock TrackTagRefreshLock
                    DebugEventLog($"ResetTrackTags:{thisTrack.GUID}")
                    thisTrack.RemoveTags()
                    Thread.Sleep(1000)
                    thisTrack.RetrieveTags()
                End SyncLock
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RefreshMyLinkTracks()
            Try
                Me.DebugEventLog("RefreshMyLinkTracks:" & Me.thisTrack.GUID.ToString)

                If Not Me.myLeftTrack Is Nothing Then
                    Me.myLeftTrack.TrafficDirection = Me.thisTrack.TrafficDirection
                    Me.myLeftTrack.SignalStatOnMe = Me.thisTrack.SignalStatOnMe
                    If Me.thisTrack.BlockAbsolute Then
                        If Not Me.myLeftTrack.BlockAbsolute AndAlso Not Me.myLeftTrack.IsShelfTrack Then
                            Me.myLeftTrack.RemotePendingBlock = True
                        End If
                    End If
                    Me.myLeftTrack.UpdateStatus()
                End If

                If Not Me.myRightTrack Is Nothing Then
                    Me.myRightTrack.TrafficDirection = Me.thisTrack.TrafficDirection
                    Me.myRightTrack.SignalStatOnMe = Me.thisTrack.SignalStatOnMe
                    If Me.thisTrack.BlockAbsolute Then
                        If Not Me.myRightTrack.BlockAbsolute AndAlso Not Me.myRightTrack.IsShelfTrack Then
                            Me.myRightTrack.RemotePendingBlock = True
                        End If
                    End If
                    Me.myRightTrack.UpdateStatus()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub RequestTrackStatus()
            Try
                Me.DebugEventLog("RequestTrackStatus:" & Me.thisTrack.GUID.ToString)

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;REQSTATUS;" & Me.thisTrack.GUID.ToString & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
                Else
                    Me.thisTrack.RemoteLinkConnected = False
                    Me.thisTrack.UpdateStatus()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Request Train ID
        ''' </summary>
        ''' <param name="theDirection">remote link direction</param>
        ''' <returns>Return symbol in remote track</returns>
        Public Function RequestTrainID(Optional theDirection As String = "") As String
            Try
                If theNetworkConnecton Is Nothing Then Return String.Empty
                Dim theMessage As String = $"REMOTELINK;TRAINIDREQUEST;{theDirection};{Chr(4)}"
                RemoteEventLog($"{GetCurrentMethod.Name}:XMT REMOTE TRACK DATA={theMessage}")
                theNetworkConnecton.SendData(theMessage)
                Thread.Sleep(1000)
                Dim waitTime As Date = Now
                Dim loopCounter As Integer = 0 'control variable to check for stuck loop
                Do While waitTime > Now.AddSeconds(-5)
                    If TrainIDRequestEvent AndAlso
                            Not String.IsNullOrWhiteSpace(TrainID) Then
                        TrainIDRequestEvent = False
                        RemoteEventLog($"{GetCurrentMethod.Name}:Train {TrainID} found")
                        Return TrainID
                    Else
                        RemoteEventLog($"{GetCurrentMethod.Name}:Waiting train")
                    End If
                    loopCounter += 1
                    Thread.Sleep(100)
                Loop
                RemoteEventLog($"{GetCurrentMethod.Name}:Train not found")
            Catch ex As Exception
                SystemException(ex.ToString)
                TrainIDRequestEvent = False
            End Try
            Return String.Empty
        End Function

        ''' <summary>
        ''' Release Train ID
        ''' </summary>
        ''' <param name="symbol">symbol assigned with train</param>
        Public Sub ReleaseTrainID(symbol As String)
            Try
                If theNetworkConnecton Is Nothing Then Exit Sub
                Dim message As String = $"REMOTELINK;TRAINIDRELEASE;{symbol}{Chr(4)}"
                RemoteEventLog($"{GetCurrentMethod.Name}:XMT REMOTE TRACK DATA:{message}")
                theNetworkConnecton.SendData($"REMOTELINK;TRAINIDRELEASE;{symbol}{Chr(4)}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub PlaceHelperTrainID(ByVal TrainID As String)
            Try
                Me.DebugEventLog("PlaceHelperTrainID:" & Me.thisTrack.GUID.ToString)

                If Not Me.myLeftTrack Is Nothing Then
                    Me.myLeftTrack.TrainIDMoveFromRemoteTrack(TrainID)
                ElseIf Not Me.myRightTrack Is Nothing Then
                    Me.myRightTrack.TrainIDMoveFromRemoteTrack(TrainID)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Place Train ID
        ''' </summary>
        ''' <param name="symbol">symbol assigned with train</param>
        Public Sub PlaceTrainID(symbol As String)
            Try
                RemoteEventLog($"{GetCurrentMethod.Name}:Train={symbol}")
                If Not myLeftTrack Is Nothing Then
                    myLeftTrack.TrainIDMoveFromRemoteTrack(symbol)
                ElseIf Not myRightTrack Is Nothing Then
                    myRightTrack.TrainIDMoveFromRemoteTrack(symbol)
                End If
                Dim train As TrainID = FindTrainBySymbol(symbol)
                If train IsNot Nothing AndAlso
                    Not String.IsNullOrEmpty(train.TrainLinkedToMe) Then
                    RemoteEventLog($"{GetCurrentMethod.Name}:TrainLinkedToMe:Train={symbol}")
                    ReleaseTrainID(train.TrainLinkedToMe)
                    PlaceHelperTrainID(train.TrainLinkedToMe)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function SendTrackStatus() As Boolean
            Try
                Me.DebugEventLog("SendTrackStatus:" & Me.thisTrack.GUID.ToString)

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;SETSTATUS;" & Me.thisTrack.GUID.ToString & ";" & Me.SetRemoteLinkTrackStatus() & Chr(4)

                    'SAFETY PRECAUTIONS TO AVOID NETWORK LOOPS
                    If SetStatusMessage = Message AndAlso SetStatusTimeStamp < Now.AddSeconds(-15) Then
                        Me.RemoteEventLog("SendTrackStatus-DUP-MESSAGE:" & Me.thisTrack.GUID.ToString)
                        Return True
                    End If
                    'SAFETY PRECAUTIONS TO AVOID NETWORK LOOPS

                    SetStatusMessage = Message
                    SetStatusTimeStamp = Now
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
                Else
                    Me.thisTrack.RemoteLinkConnected = False
                    Return False
                    ' Me.thisTrack.UpdateStatus()
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Sub RequestSignalAuthorization(ByVal theSignal As Signal, Optional ByVal symbol As String = "", Optional ByVal isPSS As Boolean = False, Optional ByVal ExitTrack As Track = Nothing)
            Try
                Me.DebugEventLog("RequestSignalAuthorization:" & theSignal.GUID.ToString)
                Dim theMessageRequest As String
                Dim ExitTrackUID As String = String.Empty

                If ExitTrack IsNot Nothing Then
                    ExitTrackUID = ExitTrack.GUID.ToString
                End If

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String
                    If symbol <> "" AndAlso Not isPSS Then
                        theMessageRequest = theSignal.myControlPoint.Name & "|" & theSignal.Name & "|" & theSignal.GUID & "|" & symbol & "|" & "FALSE" & "|" & ExitTrackUID
                    ElseIf symbol <> "" AndAlso isPSS Then
                        theMessageRequest = theSignal.myControlPoint.Name & "|" & theSignal.Name & "|" & theSignal.GUID & "|" & symbol & "|" & "TRUE" & "|" & ExitTrackUID
                    Else
                        theMessageRequest = theSignal.myControlPoint.Name & "|" & theSignal.Name & "|" & theSignal.GUID & "|" & symbol & "|" & "FALSE" & "|" & ExitTrackUID
                    End If

                    Message = "REMOTELINK;SIGNALAUTHORIZATIONREQUEST;" & theSignal.GUID & ";" & theSignal.JointControlAuthorizationOfficeModeRequested.ToString & ";" & theMessageRequest & ";" & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Request Name Line
        ''' </summary>
        ''' <param name="theSignal"></param>
        ''' <param name="theSymbol"></param>
        ''' <param name="isPSS"></param>
        ''' <param name="exitTrack"></param>
        ''' <remarks>Sends Message To Another TMDS Requesting Name Line</remarks>
        Public Sub RequestTrackName(theSignal As Signal,
                                   Optional theSymbol As String = "",
                                   Optional isPSS As Boolean = False,
                                   Optional exitTrack As Track = Nothing)
            Try
                DebugEventLog($"{GetCurrentMethod.Name}:{theSignal.GUID.ToString}")
                Dim theMessageRequest As String
                Dim exitTrackUID As String = String.Empty
                If exitTrack IsNot Nothing Then exitTrackUID = exitTrack.GUID.ToString
                If theNetworkConnecton IsNot Nothing Then

                    Dim theMessage As String
                    If Not String.IsNullOrWhiteSpace(theSymbol) AndAlso
                       isPSS Then
                        theMessageRequest = $"{theSignal.myControlPoint.Name}|{theSignal.Name}|{theSignal.GUID}|{theSymbol}|TRUE|{exitTrackUID}"
                    Else
                        theMessageRequest = $"{theSignal.myControlPoint.Name}|{theSignal.Name}|{theSignal.GUID}|{theSymbol}|FALSE|{exitTrackUID}"
                    End If

                    theMessage = $"REMOTELINK;TRACKNAMEREQUEST;{theSignal.GUID};{theSignal.JointControlAuthorizationOfficeModeRequested.ToString};{theMessageRequest};{Chr(4)}"
                    theNetworkConnecton.SendData(theMessage)
                    DebugEventLog($"XMT REMOTE TRACK DATA: {theMessage}")
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub



#Region " PSS REMOTE SIGNAL REQUEST "

        Public Function SendPSSRequestOK(ByVal Direction As String) As Boolean
            Try

                Me.RemoteEventLog("SendPSSRequestOK:" & Me.thisTrack.GUID.ToString & " Direction:" & Direction)
                IsFalseRemoteSignalRequestEvent = False
                PSSRequestGUID = System.Guid.NewGuid
                Dim Message As String = "REMOTELINK;PSSREQUEST;" & Me.thisTrack.GUID.ToString & ";" & Direction & ";" & PSSRequestGUID.ToString.ToUpper & Chr(4)


                If Not Me.theNetworkConnecton Is Nothing Then
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
                Else
                    Return True
                End If
                Dim WaitTime As Date = Now
                Me.RemoteEventLog("PSS STARTING WAIT TIME FOR REMOTE SIGNAL RESPONSE:" & WaitTime.ToLongTimeString)
                Do While WaitTime > Now.AddSeconds(-3)
                    If IsFalseRemoteSignalRequestEvent Then
                        IsFalseRemoteSignalRequestEvent = False
                        If IsFalseRemoteSignalRequestOK Then
                            IsFalseRemoteSignalRequestOK = False
                            Return True
                        End If
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            Finally
                IsFalseRemoteSignalRequestEvent = False
                IsFalseRemoteSignalRequestOK = False
            End Try
            Return False
        End Function

#End Region

        Public Function TrackTrainID(ByVal TrainName As String) As Boolean
            Try
                Me.DebugEventLog("Track Train To:" & Me.thisTrack.GUID.ToString & " TRAIN:" & TrainName)

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;TRACKTRAIN;" & TrainName & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
                Else
                    Return False
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Function SendDisconnect() As Boolean
            Try
                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;DISCONNECT;" & gThisWorkstation.Name & ";" & gThisWorkstation.WrkStnNumber & ";" & Me.thisTrack.GUID.ToString & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)
                End If
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Function

        Public Sub SendLinkDownNotification()
            Try
                Me.DebugEventLog("SendLinkDownNotification:" & Me.thisTrack.GUID.ToString)

                If theNetworkConnecton Is Nothing Then Exit Sub
                Me.theNetworkConnecton.SendData("REMOTELINK;FORCELINKDOWN" & Chr(4))
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & "REMOTELINK;FORCELINKDOWN")

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function SendBlockPendingStatus(ByVal SetPendingBlock As Boolean, Optional ByVal BlockPrompt As Boolean = False, Optional ByVal DeviceBlock As Boolean = False, Optional ByRef RemoteTrackGUID As String = "") As Boolean
            Try
                Me.RemoteEventLog("SendBlockPendingStatus:" & Me.thisTrack.GUID.ToString)
                BlockRequestOK = False
                BlockRequestGUID = System.Guid.NewGuid
                Dim Message As String = "REMOTELINK;PENDINGBLOCK;" & SetPendingBlock.ToString.ToUpper & ";" & BlockRequestGUID.ToString.ToUpper & ";" & DeviceBlock.ToString & Chr(4)
                If Me.theNetworkConnecton Is Nothing Then
                    Return True
                End If
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)

                If SetPendingBlock = False Then
                    BlockResolutionConfirmed = False
                    Me.RemoteEventLog("PENDINGBLOCK TURNED OFF: " & Me.thisTrack.GUID.ToString)
                    Exit Function
                End If
                BlockRequestEvent = False
                Dim WaitTime As Date = Now
                Me.RemoteEventLog("STARTING WAIT TIME FOR REMOTE TRACK BLOCK RESPONSE:" & WaitTime.ToLongTimeString)
                Do While WaitTime > Now.AddSeconds(-3)
                    If BlockRequestEvent Then
                        BlockRequestEvent = False
                        If BlockRequestOK Then
                            BlockRequestOK = False
                            Me.RemoteEventLog("BLOCK REQUEST RETURNED TRUE")
                            If BlockRequestConfirmationNeeded And Not BlockResolutionConfirmed And Not DeviceBlock Then
                                BlockRequestConfirmationNeeded = False
                                If Not LaunchRouteResolutionDialog(BlockPrompt) Then
                                    Return False
                                Else
                                    BlockResolutionConfirmed = True
                                End If
                            End If
                            Return True
                        Else
                            Me.RemoteEventLog("BLOCK REQUEST RETURNED FALSE DUE TO REMOTE TRACK RESPONSE WAS NEGATIVE")
                            Return False
                        End If
                    End If
                Loop
                Me.RemoteEventLog("BLOCK REQUEST RETURNED FALSE BECAUSE OF TIMEOUT:" & Now.ToLongTimeString)
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                BlockRequestEvent = False
                BlockRequestOK = False
                BlockRequestConfirmationNeeded = False
                BlockResolutionConfirmed = False
                Return False
            Finally
            End Try
        End Function

        Public Function SendBlockRemotePendingStatus(ByVal pSetPendingBlock As Boolean, Optional ByVal pBlockPrompt As Boolean = False, Optional ByVal pDeviceBlock As Boolean = False, Optional ByRef pRemoteTrackGUID As String = "") As Boolean
            Dim Message As String
            Try
                Me.RemoteEventLog("SendBlockRemotePendingStatus:Start:WhoCallMe=" & (New System.Diagnostics.StackTrace).GetFrame(1).GetMethod.Name)

                Me.RemoteEventLog("SendBlockRemotePendingStatus:" & Me.thisTrack.GUID.ToString)
                BlockRequestOK = False
                BlockRequestGUID = System.Guid.NewGuid
                Message = "REMOTELINK;PENDINGBLOCK;" & pSetPendingBlock.ToString.ToUpper & ";" & BlockRequestGUID.ToString.ToUpper & ";" & pDeviceBlock.ToString & Chr(4)
                If Me.theNetworkConnecton Is Nothing Then
                    Return True
                End If
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)

                If pSetPendingBlock = False Then
                    BlockResolutionConfirmed = False
                    Me.RemoteEventLog("PENDINGBLOCKREMOTE TURNED OFF: " & Me.thisTrack.GUID.ToString)
                    Exit Function
                End If
                BlockRequestEvent = False

                Return True

            Catch ex As Exception
                SystemException(ex.ToString)
                BlockRequestEvent = False
                BlockRequestOK = False
                BlockRequestConfirmationNeeded = False
                BlockResolutionConfirmed = False
                Return False
            Finally
                Message = Nothing
            End Try
        End Function

        Private Function LaunchRouteResolutionDialog(ByVal BlockPrompt As Boolean) As Boolean
            Try
                Dim LaunchPoint As Point
                Try
                    If Not IsNothing(gBeginTrackSelect) Then
                        LaunchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + gBeginTrackSelect.CurrentDrawingForm.Width / 2 - 100, 150)
                    Else
                        Dim trk As Track
                        If IsNothing(Me.myLeftTrack) Then
                            trk = Me.myRightTrack
                        Else
                            trk = Me.myLeftTrack
                        End If
                        LaunchPoint = New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + trk.CurrentDrawingForm.Width / 2 - 100, 150)

                    End If

                Catch ex As Exception

                End Try

                Dim frmMessageBox As frmMessageBox
                Dim MessageList As New ArrayList()
                If BlockPrompt Then
                    MessageList.Add("A ROUTE APPEARS TO BE ESTABLISHED INTO THE REQUESTED LIMITS ON THE ADJACENT DESK!")
                    MessageList.Add("")
                    MessageList.Add("HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT")
                    MessageList.Add("AND IS IT SAFE TO CONTINUE THE BLOCKING ISSUANCE?")
                    MessageList.Add("")
                    frmMessageBox = New frmMessageBox()
                    frmMessageBox.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                    frmMessageBox.SetMessage(MessageList, "REMOTE LINK ALERT", True, LaunchPoint)

                Else
                    MessageList.Add("THE PRESENT SIGNAL CONDITION ON THE ADJACENT DESK")
                    MessageList.Add("DISALLOWS TRACK AUTHORITY ISSUANCE.")
                    MessageList.Add("")
                    'MessageList.Add("HAS THE TRAIN DISPATCHER RESOLVED THIS CONFLICT")
                    'MessageList.Add("AND IS IT SAFE TO CONTINUE THE BLOCKING ISSUANCE?")
                    'MessageList.Add("")

                    frmMessageBox = New frmMessageBox()
                    frmMessageBox.Owner = TMDS.SystemForms.MySystemForms.frmMainMDI
                    frmMessageBox.SetMessage(MessageList, "REMOTE LINK ALERT", False, LaunchPoint)
                End If

                frmMessageBox.ShowDialog()
                If frmMessageBox.FormFeedBack <> 6 Then
                    frmMessageBox.Dispose()
                    frmMessageBox = Nothing
                    Return False
                End If
                frmMessageBox.Dispose()
                frmMessageBox = Nothing
                Return True
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try

        End Function

        Public Sub SendGetDeviceBlock(ByVal BlockUID As String)
            Try
                Me.DebugEventLog("SendGetDeviceBlock:" & Me.thisTrack.GUID.ToString)

                Dim Message As String = "REMOTELINK;GETDEVICEBLOCK;" & BlockUID & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)

                'Me.DebugEvent(Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendRemoveDeviceBlock(ByVal BlockUID As String)

            Try
                Me.DebugEventLog("SendRemoveDeviceBlock:" & Me.thisTrack.GUID.ToString)

                Dim Message As String = "REMOTELINK;REMOVEDEVICEBLOCK;" & BlockUID & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendGetTrackAuthority(ByVal AuthUID As String, Optional ByVal FlipFlopFound As Boolean = False)
            Try
                Me.RemoteEventLog("SendGetTrackAuthority-AuthID:" & Me.thisTrack.GUID.ToString & "-FlipFlopFound:" & FlipFlopFound.ToString & "-RemoteTrackDirection:" & Me.thisTrack.TrafficDirection)

                Dim Message As String = "REMOTELINK;GETTRACKAUTHORITY;" & AuthUID & ";" & FlipFlopFound & ";" & Me.thisTrack.TrafficDirection & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendRemoveTrackAuthority(ByVal pAuthUID As String, Optional ByVal pCancelType As String = Nothing)
            Try
                Me.RemoteEventLog("SendRemoveTrackAuthority:Track=" & Me.thisTrack.GUID.ToString & ":Auth=" & pAuthUID & ":CancelType=" & pCancelType)
                Dim Message As String = "REMOTELINK;REMOVETRACKAUTHORITY;" & pAuthUID & ";" & pCancelType & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendSignalAuthorizationResponse(ByVal theReponse As String)
            Try
                Me.DebugEventLog("SendSignalAuthorizationResponse:" & Me.thisTrack.GUID.ToString)

                Dim Message As String = "REMOTELINK;SIGNALAUTHORIZATIONRESPONSE;" & theReponse & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendRemoveTrackAuthorityViaEIC(ByVal AuthUID As String)

            Try
                Me.DebugEventLog("SendRemoveTrackAuthorityViaEIC:" & Me.thisTrack.GUID.ToString)

                Dim Message As String = "REMOTELINK;REMOVETRACKAUTHORITYEIC;" & AuthUID & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Sub SendUpdateTrackAuthority(ByVal AuthUID As String)

            Try
                Me.DebugEventLog("SendUpdateTrackAuthority:" & Me.thisTrack.GUID.ToString)

                Dim Message As String = "REMOTELINK;UPDATETRACKAUTHORITY;" & AuthUID & Chr(4)
                Me.theNetworkConnecton.SendData(Message)
                Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function SetRemoteLinkTrackStatus() As String
            Try
                Me.DebugEventLog("SetRemoteLinkTrackStatus:" & Me.thisTrack.GUID.ToString)

                If Not Me.thisTrack Is Nothing AndAlso Not Me.thisTrack.IsShelfTrack Then
                    Dim Message As String = String.Empty
                    Message = Message & "BLOCK:" & thisTrack.BlockAbsolute & "|"
                    'Message = Message & "SIGNALSTAT:" & thisTrack.SignalStatOnMe & "|"
                    If thisTrack.TrainIDs.Count > 0 Then
                        Dim TRN As TrainID = thisTrack.TrainIDs.Item(0)
                        Message = Message & "TRAINID:" & TRN.Name & "|"
                    Else
                        Message = Message & "TRAINID:NONE|"
                    End If
                    Me.DebugEventLog(Message)
                    Return (Message)

                End If
                Return Nothing
            Catch ex As Exception
                SystemException(ex.ToString)
                Return Nothing
            End Try
        End Function

        Public Sub ReceiveRemoteLinkTrackStatus(ByVal StatusMessage As String)
            Try
                Me.DebugEventLog("ReceiveRemoteLinkTrackStatus:" & Me.thisTrack.GUID.ToString & "-MESSAGE:" & StatusMessage)
                TMDS.SystemUtilities.Utilities.SystemEvent("ReceiveRemoteLinkTrackStatus:Thistrack:" & Me.ConnectedTrackGUID & "-RemoteTrack:" & Me.thisTrack.GUID)
                If Not Me.thisTrack Is Nothing Then
                    Dim dataarray() As String = StatusMessage.Split("|")
                    Dim message As String
                    For Each message In dataarray
                        Dim Status() As String = message.Split(":")
                        'TrackEvent("StatusMessage (" & StatusMessage & ") Status[0]: " & Status(0))
                        Select Case Status(0)
                            Case "TRAFFIC"
                                ' Me.DebugEvent("XMT REMOTE FLIP ME:" & Me.FlipTraffic.ToString)
                                If Me.FlipTraffic Then
                                    If Status(1) = "LEFT" Then
                                        Me.thisTrack.TrafficDirection = "RIGHT"
                                        '  Me.DebugEvent("XMT REMOTE FLIP ME 1")
                                    ElseIf Status(1) = "RIGHT" Then
                                        Me.thisTrack.TrafficDirection = "LEFT"
                                        '  Me.DebugEvent("XMT REMOTE FLIP ME 2")
                                    Else
                                        Me.thisTrack.TrafficDirection = Status(1)
                                        '  Me.DebugEvent("XMT REMOTE FLIP ME 3")
                                    End If
                                Else
                                    Me.thisTrack.TrafficDirection = Status(1)
                                    ' Me.DebugEvent("XMT REMOTE FLIP ME 4")
                                End If
                                'If Me.thisTrack.GUID = "151197" Then
                                '    Dim trk As Track = TMDS.SystemUtilities.FetchData.tr
                                '    If Me.thisTrack.TrafficDirection = "RIGHT" Then
                                '        Me.ConnectedTrackGUID.thisTrack.TrafficDirection = "LEFT"
                                '    End If
                                'End If
                                'If Not IsNothing(Me.myRightTrack) Then
                                '    If Me.myRightTrack.TrafficDirection = "LEFT" Then
                                '        Me.thisTrack.TrafficDirection = "LEFT"
                                '    End If
                                'End If
                            Case "BLOCK"
                                Me.thisTrack.BlockAbsolute = CBool(Status(1))
                            Case "SIGNALSTAT"
                                Select Case CInt(Status(1))
                                    Case 0
                                        Me.thisTrack.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalAtStop
                                    Case 1
                                        Me.thisTrack.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalRequested
                                    Case 2
                                        Me.thisTrack.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalCleared
                                    Case 3
                                        Me.thisTrack.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalInTime
                                    Case 4
                                        Me.thisTrack.SignalStatOnMe = TrackComponents.Track.SignalStatus.SignalUnSolicated
                                End Select
                            Case "OCCUPIED"
                                Me.thisTrack.Occuppied = CBool(Status(1))
                                If Not IsNothing(Me.thisTrack.Ind(0)) Then
                                    Me.thisTrack.Ind(0).BitState = CBool(Status(1))
                                End If
                                If Me.thisTrack.TrackIndicationORED Then
                                    If Not IsNothing(Me.thisTrack.Ind(1)) Then
                                        Me.thisTrack.Ind(1).BitState = CBool(Status(1))
                                    End If
                                End If
                                'Me.thisTrack.ApplyIndicationChange()
                            Case "FAILED"
                                '684.0 P1-5087 - Update remote track with failed state if local link is down or vise versa...Added option to have different code for other railroads (RPBJR)
                                'First get a reference to the local track
                                'Dim trk As Track = FindTrackFromGUID(ConnectedTrackGUID)
                                Me.thisTrack.FailState = CBool(Status(1))
                                'If Me.thisTrack.GUID = "151197" Then
                                '    If Me.thisTrack.TrafficDirection = "RIGHT" OrElse Me.thisTrack.TrafficDirection = "" Then
                                '        Me.thisTrack.TrafficDirection = "LEFT"
                                '    End If
                                'End If
                                'If optRailRoadName = "BNSF" Then
                                '    If Me.thisTrack.FailState And Me.thisTrack.Initialized Then
                                '        Me.thisTrack.FailStatePrevious = True

                                '        If gDebug And gDebugShowRemoteTrackInfo Then SystemUtilities.Utilities.SystemEvent("REMOTE TRACK IS IN FAILSTATE, SETTING LOCAL TRACK (" & trk.GUID & ") TO FAILED")
                                '        trk.FailStatePrevious = trk.FailState
                                '        trk.FailState = Me.thisTrack.FailState
                                '        trk.UpdateStatus()
                                '    ElseIf Not Me.thisTrack.FailState And Me.thisTrack.FailStatePrevious Then
                                '        Me.thisTrack.FailStatePrevious = False
                                '        Me.thisTrack.CheckToRemoveOfficeTrafficRoute()
                                '        Me.thisTrack.CheckForPendingRoutesBehindMe()
                                '        Me.thisTrack.UpdateStatus()
                                '        If gDebug And gDebugShowRemoteTrackInfo Then SystemUtilities.Utilities.SystemEvent("REMOTE TRACK IS <<NOT>> IN FAILSTATE, SETTING LOCAL TRACK (" & trk.GUID & ") TO <<NOT>> FAILED")
                                '        trk.FailStatePrevious = False
                                '        trk.CheckToRemoveOfficeTrafficRoute()
                                '        trk.CheckForPendingRoutesBehindMe()
                                '        trk.UpdateStatus()
                                '    End If
                                'Else
                                'THIS IS THE ORIGINAL CODE PRIOR TO MY (RPBJR) P1-5087 UPDATE
                                If Me.thisTrack.FailState Then
                                    Me.thisTrack.FailStatePrevious = True
                                ElseIf Not Me.thisTrack.FailState And Me.thisTrack.FailStatePrevious Then
                                    Me.thisTrack.FailStatePrevious = False
                                    Me.thisTrack.CheckToRemoveOfficeTrafficRoute()
                                    Me.thisTrack.CheckForPendingRoutesBehindMe()
                                    Me.thisTrack.UpdateStatus()
                                End If
                                'End If
                                'END 684.0 P1-5087
                        End Select
                    Next
                    ' ApplyTrackStatus()
                    If Mid(Me.thisTrack.AuthorityType, 1, 3) <> "TWC" Then ApplyTrackStatus()
                    Me.thisTrack.UpdateStatusFromRemoteMessage()
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ApplyTrackStatus()
            Try
                If Not IsNothing(Me.myRightTrack) Then
                    If Me.thisTrack.TrafficDirection = "RIGHT" Then
                        ApplyRightTraffic()
                    Else
                        Me.CheckToRemoveTraffic(Me.thisTrack.TrafficDirection)
                    End If

                End If

                If Not IsNothing(Me.myLeftTrack) Then
                    If Me.thisTrack.TrafficDirection = "LEFT" Then
                        ApplyLeftTraffic()
                    Else
                        Me.CheckToRemoveTraffic(Me.thisTrack.TrafficDirection)
                    End If

                End If

            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function GetTrainConfirmation() As ArrayList
            Try
                Me.DebugEventLog("GetTrainConfirmation:" & Me.thisTrack.GUID.ToString)

                If Not Me.theNetworkConnecton Is Nothing Then
                    Dim Message As String = "REMOTELINK;REQUESTINGTRAINS;" & Chr(4)
                    Me.theNetworkConnecton.SendData(Message)
                    Me.DebugEventLog("XMT REMOTE TRACK DATA:" & Message)

                    TrainsRequested = ""
                    Dim WaitTime As Date = Now
                    Dim TrainList As New ArrayList()
                    Do While WaitTime > Now.AddSeconds(-3)
                        If TrainsRequestEvent Then
                            TrainsRequestEvent = False

                            Dim Trains() As String = TrainsRequested.Split("|")

                            Dim TrnID As String
                            For Each TrnID In Trains
                                Dim TrainDetails() As String = TrnID.Split(":")
                                If TrainDetails.Length >= 3 Then
                                    TrainList.Add(TrainDetails(1) & " " & TrainDetails(2))
                                End If
                            Next
                            Return TrainList
                        End If
                    Loop
                    Return TrainList
                    'Me.DebugEvent("TRAIND REQUEST RETURNED:" & TrainID)
                Else
                    Return Nothing
                End If

            Catch ex As Exception
                SystemException(ex.ToString)
                TrainsRequestEvent = False
                Return Nothing
            End Try

        End Function

        Public Sub Shutdown()
            Try
                Me.DebugEventLog("Shutdown:" & Me.thisTrack.GUID.ToString)

                Dim count As Integer
                Me.theNetworkConnecton.SendData("REMOTELINK;DISCONNECT;" & gThisWorkstation.Name & ";" & gThisWorkstation.WrkStnNumber & ";" & Me.thisTrack.GUID.ToString & Chr(4))
                While theXmtQueue.Count <> 0
                    count = count + 1
                    Threading.Thread.Sleep(1000)
                    If count > 4 Then
                        Exit While
                    End If
                End While
                Me.theNetworkConnecton.CloseClientConnection()
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try

        End Sub

        ''' <summary>
        ''' Remote event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub RemoteEventLog(message As String)
            Try
                SystemEvent($"{NameOf(RemoteTrack)} {theWrkStnIPString _
                            }:{If(thisTrack IsNot Nothing,
                            $"{thisTrack.Name}:{thisTrack.GUID}", String.Empty)}:{message}")
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        ''' <summary>
        ''' Debug event log
        ''' </summary>
        ''' <param name="message">message to write in log</param>
        Private Sub DebugEventLog(message As String)
            Try
                If gDebug Then RemoteEventLog(message)
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#Region "NEW CODE UPDATES FOR ROUTE CHECK ACROSS MANY TRACKS"

        Private Sub CheckToRemoveTraffic(ByVal Direction As String)
            Try
                Dim LoopControl As Integer
                If Direction = "RIGHT" Then
                    Dim NextTrack As Track = Me.myRightTrack
                    Do
                        If IsNothing(NextTrack) Then
                            Exit Sub
                        End If

                        NextTrack.CheckToRemoveOfficeTrafficRoute()
                        NextTrack.CheckForPendingRoutesBehindMe()
                        'NextTrack = TMDS.SystemUtilities.FetchData.FindRemoteTrackItemFromGUID(NextTrack.RightLink)
                        NextTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextTrack.RightLink)
                        LoopControl = LoopControl + 1
                        If LoopControl >= 10 Then
                            Exit Sub
                        End If
                    Loop
                ElseIf Direction = "LEFT" Then
                    Dim NextTrack As Track = Me.myLeftTrack
                    Do
                        If IsNothing(NextTrack) Then
                            Exit Sub
                        End If

                        NextTrack.CheckToRemoveOfficeTrafficRoute()
                        NextTrack.CheckForPendingRoutesBehindMe()
                        'NextTrack = TMDS.SystemUtilities.FetchData.FindRemoteTrackItemFromGUID(NextTrack.LeftLink)
                        NextTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextTrack.LeftLink)
                        LoopControl = LoopControl + 1
                        If LoopControl >= 10 Then
                            Exit Sub
                        End If
                    Loop
                Else

                    Dim NextLeftTrack As Track = Me.myLeftTrack
                    Do
                        If IsNothing(NextLeftTrack) Then
                            Exit Do
                        End If
                        If NextLeftTrack.Occuppied Then
                            Exit Do
                        ElseIf NextLeftTrack.TrafficDirection <> "LEFT" Then
                            Exit Do
                        Else
                            NextLeftTrack.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            NextLeftTrack.TrafficDirection = ""
                            NextLeftTrack.UpdateStatusFromRemoteMessage()
                        End If
                        'NextLeftTrack = TMDS.SystemUtilities.FetchData.FindRemoteTrackItemFromGUID(NextLeftTrack.LeftLink)
                        NextLeftTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextLeftTrack.LeftLink)
                        LoopControl = LoopControl + 1
                        If LoopControl >= 10 Then
                            Exit Do
                        End If
                    Loop

                    Dim NextRightTrack As Track = Me.myRightTrack
                    Do
                        If IsNothing(NextRightTrack) Then
                            Exit Do
                        End If
                        If NextRightTrack.Occuppied Then
                            Exit Do
                        ElseIf NextRightTrack.TrafficDirection <> "RIGHT" Then
                            Exit Do
                        Else
                            NextRightTrack.SignalStatOnMe = Track.SignalStatus.SignalAtStop
                            NextRightTrack.TrafficDirection = ""
                            NextRightTrack.UpdateStatusFromRemoteMessage()
                        End If
                        'NextRightTrack = TMDS.SystemUtilities.FetchData.FindRemoteTrackItemFromGUID(NextRightTrack.RightLink)
                        NextRightTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextRightTrack.RightLink)
                        LoopControl = LoopControl + 1
                        If LoopControl >= 10 Then
                            Exit Do
                        End If
                    Loop


                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ApplyRightTraffic()
            Try
                Dim LoopControl As Integer
                Dim NextTrack As Track = Me.myRightTrack
                Do
                    If NextTrack.TrafficDirection <> Me.thisTrack.TrafficDirection OrElse NextTrack.SignalStatOnMe <> Me.thisTrack.SignalStatOnMe Then
                        NextTrack.TrafficDirection = Me.thisTrack.TrafficDirection
                        NextTrack.SignalStatOnMe = Me.thisTrack.SignalStatOnMe
                        NextTrack.UpdateStatus()
                    End If
                    NextTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextTrack.RightLink)

                    If IsNothing(NextTrack) Then
                        Exit Sub
                    End If
                    LoopControl = LoopControl + 1
                    If LoopControl >= 10 Then
                        Exit Sub
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Private Sub ApplyLeftTraffic()
            Try
                Dim LoopControl As Integer
                Dim NextTrack As Track = Me.myLeftTrack
                Do
                    If NextTrack.TrafficDirection <> Me.thisTrack.TrafficDirection OrElse NextTrack.SignalStatOnMe <> Me.thisTrack.SignalStatOnMe Then
                        NextTrack.TrafficDirection = Me.thisTrack.TrafficDirection
                        NextTrack.SignalStatOnMe = Me.thisTrack.SignalStatOnMe
                        NextTrack.UpdateStatus()
                        RemoteEventLog("ApplyLeftTraffic:TrakName:" & NextTrack.Name & "-GUID:" & NextTrack.GUID & "-TrafficDirection:" & NextTrack.TrafficDirection)
                    End If

                    NextTrack = TMDS.SystemUtilities.FetchData.FindTrackItemFromGUID(NextTrack.LeftLink)

                    If IsNothing(NextTrack) Then
                        Exit Sub
                    End If
                    LoopControl = LoopControl + 1
                    If LoopControl >= 10 Then
                        Exit Sub
                    End If
                Loop
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

#End Region

#Region "NEW CODE  FOR TRACK TAG CHECK ACROSS MANY TRACKS"
        Public Function GetSignalClearConfirmation(ByVal Direction As String) As Boolean
            Try
                Me.RemoteEventLog("GETTING-SignalClearConfirmationOnRemoteTrack:" & Me.thisTrack.GUID.ToString)

                RouteRequestOK = False
                RouteRequestEvent = False
                RouteRequestGUID = System.Guid.NewGuid
                Dim Message As String = "REMOTELINK;ROUTEREQUEST;" & Direction & ";" & RouteRequestGUID.ToString.ToUpper & Chr(4)
                Me.DebugEventLog(Message)
                If Not IsNothing(Me.theNetworkConnecton) Then
                    Me.theNetworkConnecton.SendData(Message)
                    Me.RemoteEventLog("XMT REMOTE TRACK DATA:" & Message)

                    Dim WaitTime As Date = Now
                    Do While WaitTime > Now.AddSeconds(-3)
                        If RouteRequestEvent Then
                            RouteRequestEvent = False
                            If RouteRequestOK Then
                                RouteRequestOK = False
                                If Not gRemoteCPFailGUIDS.Contains(Me.thisTrack.GUID.ToString) Then
                                    If PromptForCodeFailureConfirmation Then
                                        gRemoteCPFailGUIDS.Add(Me.thisTrack.GUID.ToString)
                                        If Not DoCheckForCodeFailureConfirmation() Then
                                            Return False
                                        End If
                                    End If
                                End If
                                If Me.TagList.Length > 0 Then
                                    If Not DoCheckForTagConfirmation(TagList) Then
                                        Return False
                                    End If
                                End If
                                PromptForCodeFailureConfirmation = False
                                Me.RemoteEventLog("GetSignalClearConfirmationOnRemoteTrack:TRUE:" & Me.thisTrack.GUID.ToString)
                                Return True
                            Else
                                Me.RemoteEventLog("GetSignalClearConfirmationOnRemoteTrack:FALSE:" & Me.thisTrack.GUID.ToString)
                                PromptForCodeFailureConfirmation = False
                                Return False
                            End If
                        End If
                        Threading.Thread.Sleep(50)
                    Loop
                    RouteRequestEvent = False
                    RouteRequestOK = False
                    PromptForCodeFailureConfirmation = False
                    Me.RemoteEventLog("GetSignalClearConfirmationOnRemoteTrack:FALSE:" & Me.thisTrack.GUID.ToString)
                End If
                Return False
            Catch ex As Exception
                SystemException(ex.ToString)
                RouteRequestEvent = False
                RouteRequestOK = False
                PromptForCodeFailureConfirmation = False
                Return False
            End Try

        End Function

        Private Function DoCheckForTagConfirmation(tagList As String) As Boolean
            Try
                Dim tags() As String = tagList.Split("|")
                Dim theList As ArrayList = RetrieveTags(tags)
                If IsNothing(theList) Then Return False

                Dim t As Tag
                For Each t In theList
                    If Not DoConfirmTag(t) Then
                        Return False
                    End If
                Next

                Return True
            Catch ex As Exception
                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, "LogClickEvent Error. Exception:" & ex.ToString, SystemEventQueue)
                Return False
            End Try
        End Function

        Private Function DoCheckForCodeFailureConfirmation() As Boolean

            Try
                Dim ErrorMessage As String = String.Empty
                Dim myTrack As Track = Nothing
                If Not IsNothing(Me.myLeftTrack) Then
                    myTrack = myLeftTrack
                Else
                    myTrack = myRightTrack
                End If
                Dim lpoint As New Point(TMDS.SystemForms.MySystemForms.frmMainMDI.Left + myTrack.CurrentDrawingForm.Width / 2 - 100, 150)
                Dim frmMessageBox1 As frmMessageBox
                Dim MessageList1 As New ArrayList()
                frmMessageBox1 = New frmMessageBox()
                'frmMessageBox1.Owner = gBeginTrackSelect.CurrentDrawingForm
                MessageList1.Add("A PORTION OF TRACK WITHIN THE LIMITS OF")
                MessageList1.Add("THIS REQUEST ON THE ADJANENT DESK IS IN COMMUNICATION FAILURE.")
                MessageList1.Add("")
                MessageList1.Add("DO YOU WANT TO CONTINUE?")
                frmMessageBox1.SetMessage(MessageList1, "COMMUNICATION FAILURE CONFLICT CHECK", True, lpoint)
                gCritcalPromptExists = True
                frmMessageBox1.ShowDialog()
                If frmMessageBox1.FormFeedBack <> 6 Then
                    ErrorMessage = "DISPATCHER SELECT NO TO CONTINUE WITH SIGNAL CLEAR ACCOUNT FAILED TRACK FOUND IN THE LIMITS OF THE SIGNAL REQUEST."
                    Me.RemoteEventLog(ErrorMessage)
                    frmMessageBox1.Dispose()
                    frmMessageBox1 = Nothing
                    gCritcalPromptExists = False
                    Return False
                End If
                gCritcalPromptExists = False
                ErrorMessage = "DISPATCHER SELECT YES TO CONTINUE WITH SIGNAL CLEAR ACCOUNT FAILED TRACK FOUND IN THE LIMITS OF THE SIGNAL REQUEST."
                Me.RemoteEventLog(ErrorMessage)
                frmMessageBox1.Dispose()
                frmMessageBox1 = Nothing
                Return True
            Catch ex As Exception
                Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.ExceptionEvent, "LogClickEvent Error. Exception:" & ex.ToString, SystemEventQueue)
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Do confirm tag
        ''' </summary>
        ''' <param name="pTag"></param>
        ''' <returns></returns>
        Private Function DoConfirmTag(pTag As Tag) As Boolean
            Dim tagConfirmed As Boolean
            Dim confirmedTags As New ArrayList
            Dim messageList As New ArrayList
            Dim frmMessageBox As frmMessageBox = Nothing
            Dim trk As Track = Me.myLeftTrack
            Try
                If Not IsNothing(Me.myRightTrack) Then
                    trk = Me.myRightTrack
                End If
                If pTag.Restriction And Not confirmedTags.Contains(pTag) Then
                    If pTag.Direction = "BOTH" Then
                        Dim TagString() As String
                        messageList.Add(optTagString21 & pTag.TagType.ToString.ToUpper & Space(1) & optTagString20)
                        messageList.Add("")
                        TagString = pTag.DataString.Split("|")
                        Select Case pTag.TagType.ToString.ToUpper
                            Case "XING"
                                If TagString.Length < 2 Then Exit Select
                                messageList.Add(optTagString17 & TagString(0))
                                messageList.Add(optTagString18 & TagString(1))
                            Case "SPEED"
                                If TagString.Length < 5 Then Exit Select
                                messageList.Add(optTagString10 & Space(1) & TagString(0) & Space(1) & optTagString12)
                                messageList.Add(optTagString11 & Space(1) & TagString(5) & Space(1) & optTagString12)
                                messageList.Add(optTagString13 & Space(1) & TagString(1))
                                messageList.Add(optTagString14 & Space(1) & TagString(2))
                                messageList.Add(optTagString15 & Space(1) & TagString(3) & Space(1) & optTagString16)
                            Case "INFO"
                                If TagString.Length < 1 Then Exit Select
                                messageList.Add(optTagString19 & Space(1) & TagString(0))
                            Case optTagType6 'TRACK ALARM
                                If TagString.Length < 1 Then Exit Select
                                messageList.Add(TagString(0))
                            Case "WEATHER"
                                messageList.Add("CRITICAL WEATHER ALERT")
                        End Select
                        messageList.Add("")
                        Dim Feedback As Integer = 1
                        If optEnableTagReadRepeat Then
                            If optApplyConfigurationForKCT Then
                                Using frmReadRepeat1 As New frmReadRepeat
                                    frmReadRepeat1.SetMessage(pTag.GetKCTTagVerbiage, optTagString9, trk.CurrentDrawingForm.myLaunchPoint, "TAG", False, False)
                                    frmReadRepeat1.ShowDialog()
                                    Feedback = frmReadRepeat1.FormFeedback
                                End Using
                            ElseIf Not optApplyTagConfigurationForMRS Then
                                Using frmReadRepeat As New frmReadRepeat
                                    frmReadRepeat.SetMessage(messageList, optTagString9, trk.CurrentDrawingForm.myLaunchPoint, "TAG", False, False)
                                    frmReadRepeat.ShowDialog()
                                    Feedback = frmReadRepeat.FormFeedback
                                End Using
                            End If
                        Else
                            frmMessageBox = New frmMessageBox()
                            frmMessageBox.SetMessage(messageList, optTagString9, True, trk.CurrentDrawingForm.myLaunchPoint, "OK|CANCEL")
                            If Not trk.CurrentDrawingForm.InvokeRequired Then
                                frmMessageBox.Owner = trk.CurrentDrawingForm
                                frmMessageBox.ShowDialog()
                            Else
                                ' This is a dirty way to take care of cross thread operation. Need to revisit to takecare of this issue. 
                                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False
                                frmMessageBox.Owner = trk.CurrentDrawingForm
                                frmMessageBox.ShowDialog()
                                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = True
                            End If
                            Feedback = frmMessageBox.FormFeedBack
                        End If
                        gCritcalPromptExists = True
                        If optEnableTagReadRepeat Then
                            trk.CurrentDrawingForm.FormList.Remove(frmReadRepeat)
                        Else
                            trk.CurrentDrawingForm.FormList.Remove(frmMessageBox)
                        End If
                        If Feedback = 6 Then
                            Dim MessageString As String = "SELECT OK TO TRACK RESTRICTION. (TAG TYPE:" & pTag.TagType.ToString.ToUpper & ")(TRACK UID:" & pTag.TrackGuid & ")(TAG DATA:" & pTag.DataString & ")"
                            Dim SysEvent As New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, MessageString, SystemEventQueue)
                            SysEvent = Nothing
                            confirmedTags.Add(pTag)
                            tagConfirmed = True
                        Else
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
                            messageList.Clear()
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
                        messageList.Clear()
                        gCritcalPromptExists = False
                    End If
                End If
                Return tagConfirmed
            Catch ex As Exception
                SystemException(ex.ToString)
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Retrieve Tags
        ''' </summary>
        ''' <param name="tagList"></param>
        ''' <returns></returns>
        Private Function RetrieveTags(tagList() As String) As ArrayList
            Dim theList As New ArrayList
            Try
                Using dbConn As New OleDbConnection(gConnectionStringDynamic)
                    dbConn.Open()
                    Dim query As String = $"SELECT TrackGUID, Subdivision, TerritoryID, Creator, CreateTime, TagType, 
                                            DataString, Restrictive, RestrictionFlag, DelayData, Delay, TagUID, 
                                            Direction, ExpectedReleaseTime, CompList, RefNumPTC, PTCData, 
                                            NameRequestingTagPlacement, SpeedTagFlagInfo, SpeedTagReasonCode,
                                            CreateSpeedRestriction, SpeedRestrictionCreated, DurationTime, BulletinTag, 
                                            BulletinReferenceGUID, BulletinForProtectionList, BulletinForCancellationList, 
                                            PTCComponents, PTCTrackSegments, PTCBulletinText, MilePostBeginPrefix, 
                                            MilePostEndPrefix, DeliveryOfRestriction, AdvisorySummary, MilePostBeginSuffix, 
                                            MilePostEndSuffix, MilePostBegin, MilePostEnd, PTCCompositionCRC, 
                                            ObjectCRC, CriticalAlertType, HeadEndRestrictionOnly 
                                        FROM tblTrackTagsActive WHERE (TagUID = "
                    Dim tagString As New StringBuilder
                    For Each oneTag As String In tagList
                        If Trim(oneTag) <> "" Then
                            If tagString.Length = 0 Then
                                tagString.Append($"'{oneTag}')")
                            Else
                                tagString.Append($" OR (TagUID = '{oneTag}')")
                            End If
                        End If
                    Next
                    query &= tagString.ToString
                    Using dbComm As New OleDbCommand(query, dbConn)
                        Using dr As OleDbDataReader = dbComm.ExecuteReader()
                            While dr.Read()
                                Dim TrackTag As New Tag()
                                With TrackTag
                                    .TrackGuid = dr.GetInt32(0)
                                    .Subdivision = dr.GetString(1)
                                    .TerritoryID = dr.GetInt32(2)
                                    .Creator = dr.GetString(3)
                                    .CreateTime = dr.GetString(4)
                                    .TagType = dr.GetString(5)
                                    .DataString = dr.GetString(6)
                                    .Restrictive = dr.GetInt32(7)
                                    .Restriction = dr.GetBoolean(8)
                                    .DelayData = dr.GetString(9)
                                    .Delay = dr.GetBoolean(10)
                                    If Not dr.IsDBNull(11) Then .TagGuid = dr.GetString(11).Trim
                                    If .Restrictive = 2 Then .ShowOccupancy = True
                                    If Not dr.IsDBNull(12) Then .Direction = dr.GetString(12).Trim
                                    If .Restriction Then theList.Add(TrackTag)
                                End With
                            End While
                        End Using
                    End Using
                End Using
                Return theList
            Catch ex As Exception
                SystemException(ex.ToString)
                Return theList
            End Try
        End Function

#End Region

    End Class

#End Region

#Region "TRACK ROUTES"

    Public Class TrackRoute
        Public GUID As String
        Public Priority As Integer
        Public myName As String
        Public EntrySignal As Signal
        Public ExitSignal As Signal
        Public myControlPoint As ControlPoint
        Public myTerritory As Territory
        Public myComponentList As New ArrayList()

        Sub New(ByVal CPNUM As Int32, ByVal TERRID As Int32, ByVal CPLIST As String)
            Try
                Me.myTerritory = TMDS.SystemUtilities.FetchData.FindTerritoryFromID(TERRID)
                Me.myControlPoint = TMDS.SystemUtilities.FetchData.FindCPFromGUID(CPNUM)

                Dim list() As String = CPLIST.Split("|")
                Dim comp As String
                For Each comp In list
                    If IsNumeric(comp) Then
                        Dim compInList As Object = TMDS.SystemUtilities.FetchData.FindCompItemFromGUID(CInt(comp))
                        If Not myComponentList.Contains(compInList) Then
                            myComponentList.Add(compInList)
                        End If
                    End If
                Next
                If Not IsNothing(Me.myControlPoint) Then
                    If IsNothing(Me.myControlPoint.TrackRoutes) Then
                        Me.myControlPoint.TrackRoutes = New ArrayList()
                    End If
                    Me.myControlPoint.TrackRoutes.Add(Me)
                End If
            Catch ex As Exception
                SystemException(ex.ToString)
            End Try
        End Sub

        Public Function DoConflictChecks(CompList As ArrayList) As Boolean
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
    End Class

#End Region

End Namespace


