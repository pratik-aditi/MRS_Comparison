Option Strict On

Imports System.Reflection.MethodBase
Imports System.Timers
Imports TMDS.SignalComponents
Imports TMDS.SystemUtilities.Utilities
Imports TMDS.TrainComponents
Imports Wabtec.Base

''' <summary>
''' This class handles the validation and verifications of routes that will be issued as an authority for each train.
''' </summary>
Public Class TrainRouteScan
    Implements IDisposable

    Private _isScanning As Boolean

    ''' <summary>
    ''' Defines the interval (in seconds) for the <see cref="ScanningTimer"/> elapsed events. 
    ''' </summary>
    Private ReadOnly _interval As Integer = 5

    ''' <summary>
    ''' Get the default instance of <see cref="TrainRouteScan"/>
    ''' </summary>
    Public Shared ReadOnly Property Instance As New TrainRouteScan

    ''' <summary>
    ''' Get or set the value of <see cref="_isScanning"/> matching <see cref="ScanningTimer"/>'s state.
    ''' </summary>
    Public Property IsScanning As Boolean
        Get
            Return _isScanning
        End Get
        Private Set(value As Boolean)
            _isScanning = value
        End Set
    End Property

    ''' <summary>
    ''' Get the <see cref="ScanningTimer"/> interval value.
    ''' </summary>
    Public ReadOnly Property Interval As Integer
        Get
            Return _interval
        End Get
    End Property

    Private DisposedValue As Boolean

    ''' <summary>
    ''' A <see cref="Timer"/> used to periodically trigger route scanning operations at a configured interval.
    ''' </summary>
    Private ReadOnly ScanningTimer As Timer

    ''' <summary>
    ''' Constructor which initializes a new instance of the class. 
    ''' </summary>
    ''' <remarks>It configures the <see cref="ScanningTimer"/> with the specified interval (converted to milliseconds) 
    ''' and associates the <see cref = "OnTimerElapsed" /> method With the timer's Elapsed event.</remarks>
    Public Sub New()
        ScanningTimer = New Timer(Interval * 1000) With {.AutoReset = False}
        AddHandler ScanningTimer.Elapsed, AddressOf OnTimerElapsed
        IsScanning = False
    End Sub

    ''' <summary>
    ''' This method initiates the route scanning process if it is not already active. 
    ''' </summary>
    Public Sub StartScanning()
        Try
            If Not IsScanning Then
                SystemEvent($"{GetCurrentMethod.Name} Starting route scanning with an interval of {Interval} seconds.")
                IsScanning = True
                ScanningTimer.Start()
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' This method halts the route scanning process if it is currently active. 
    ''' </summary>
    Public Sub StopScanning()
        Try
            If IsScanning Then
                SystemEvent($"{GetCurrentMethod.Name} Stopping route scanning.")
                IsScanning = False
                ScanningTimer.Stop()
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' This method iterates over all the trains in the <see cref="TrainCollection"/> and selects which trains are fit to
    ''' invoke the <see cref="DoRouteScanning(TrainID)"/> method. 
    ''' </summary>
    ''' <param name="sender">The source (<see cref="ScanningTimer"/>) of the event.</param>
    ''' <param name="e">Object that contains the event data.</param>
    ''' <remarks>Ensuring that route scanning is periodically performed for all trains in the collection based on 
    ''' the imposed conditions and <see cref="ScanningTimer"/> configured interval.</remarks>
    Private Sub OnTimerElapsed(sender As Object, e As ElapsedEventArgs)
        Try
            TrainCollection.OfType(Of TrainID).
                            Where(Function(oneTrain)
                                      With oneTrain
                                          Return .IsActivePTC AndAlso
                                                 .thisCurrentLicense IsNot Nothing
                                      End With
                                  End Function).
                            ToList.
                            ForEach(AddressOf DoRouteScanning)
            OtherTrackAuthorities.OfType(Of TrackAuthority.TrackAuthority).
                Select(Function(x) x.myTrain).
                ForEach(AddressOf DoRouteScanning)
        Catch ex As Exception
            SystemException(ex.ToString)
        Finally
            ScanningTimer.Start()
        End Try
    End Sub

    ''' <summary>
    ''' This method performs a detailed check on the state of a train's current authority. 
    ''' </summary>
    ''' <param name="train">The train whose route will be assessed to determine eligibility for authority issuance.</param>    
    ''' <remarks>This method evaluates the positions of switches within the authority's component selection. 
    ''' If any switches are found to be out of position, the authority will not be issued.</remarks>
    Private Sub DoRouteScanning(train As TrainID)
        Try
            If train Is Nothing Then
                SystemEvent($"{GetCurrentMethod.Name} Train is null")
                Exit Sub
            End If
            With train.thisCurrentLicense
                If .ComponentsInSelection.Count = 0 Then
                    SystemEvent($"{GetCurrentMethod.Name} Components is empty")
                    Exit Sub
                End If
                If .NeedsElectronicTransmission AndAlso
                    train.IsPresenceOnNetwork AndAlso
                    Not .PTCElectronicTransmissionInProgress Then
                    Dim sendAuthority As Boolean = True
                    For Each route As TacticalRoute In train.myTacticalRoutes
                        If Not route.myStatus.Equals("FULLFILLED") AndAlso
                            Not route.myStatus.Equals("STORED ROUTE") Then
                            With route.mySignalRoute.EntrySignal
                                If .SignalIndStatus <> Signal.IndStatus.Cleared AndAlso
                                    Not .AutomaticSignal AndAlso
                                    Not .HoldingSignal Then
                                    sendAuthority = False
                                    train.TrainEventLog($"{GetCurrentMethod.Name _
                                                }:Signal not clear={ .Name _
                                                }:SignalIndStatus={ .SignalIndStatus _
                                                }:myStatus={route.myStatus}")
                                    Exit For
                                ElseIf Not route.myStatus.Equals("STORED ROUTE") Then
                                    .CurrentTacticalRoute = route
                                End If
                            End With
                        End If
                    Next
                    If sendAuthority Then
                        train.TrainEventLog($"{GetCurrentMethod.Name}:Send authority { .AuthorityNumber}")
                        .NeedsElectronicTransmission = False
                        If Not .MarkedForDelete Then
                            .VETMS_PerformReIssuance(train, .PTCAuthorityBeingModified)
                        End If
                        If gTestPTC Then
                            train.TrainEventLog($"{GetCurrentMethod.Name}:Confirm license for Test PTC")
                            .PtcConfirmLicense()
                        End If
                    End If
                Else
                    train.CheckAutoSignalCancellation()
                End If
            End With
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' This method performs resource cleanup.
    ''' </summary>
    ''' <param name="disposing">Specifies whether to release both managed and unmanaged resources.
    ''' <see langword="True"/> indicates that managed resources should also be cleaned up. 
    ''' <see langword="False"/> indicates that only unmanaged resources should be released.</param>
    ''' <remarks>It utilizes the <see cref="DisposedValue"/> flag to ensure resources will be released only once.</remarks>
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not DisposedValue Then
            If disposing Then
                RemoveHandler ScanningTimer.Elapsed, AddressOf OnTimerElapsed
                ScanningTimer.Dispose()
            End If
            DisposedValue = True
        End If
    End Sub

    ''' <summary>
    ''' This method is the interface implementation for IDisposable.
    ''' </summary>
    ''' <remarks>Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method.
    ''' Calls <see cref="Dispose(Boolean)"/> with parameter set to <see langword="True"/> to ensure that managed 
    ''' resources will be cleaned up.</remarks>
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
