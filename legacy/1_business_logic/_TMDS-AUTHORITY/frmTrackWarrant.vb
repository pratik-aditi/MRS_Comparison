Imports TMDS.SystemForms.MySystemForms
Imports TMDS.TrackAuthority
Imports TMDS.ManagementClasses
Imports TMDS.GraphicComponent : Imports TMDS.TrainComponents : Imports TMDS.TrackComponents : Imports TMDS.SwitchComponents : Imports TMDS.SignalComponents : Imports TMDS.TrackTagComponents : Imports TMDS.TrackOtherComponents
Imports TMDS.modSystemFuncs
Imports TMDS.PrintForms
Imports System.Data.OleDb
Imports System.ComponentModel
Imports TMDS.SystemUtilities.Utilities
Imports System.Text

Public Class frmTrackWarrantNew
    Inherits System.Windows.Forms.Form

#Region "PROPERTIES"

    Private PrintThread As Threading.Thread
    Private thisSubdivision As Subdivision
    Public ThisCurrentWarrant As Warrant
    Public FirstWarrantFound As Warrant
    Public TrackWarrantInUse As Boolean
    Public DispName As String
    Public RailwayName As String

    Public DirectionOfTheWarrant As String
    Public FirstTypeOfWarrant As String
    Public SecondTypeOfWarrant As String
    Public FirstTrackSelection As ArrayList
    Public SecondTrackSelection As ArrayList
    Public TerritoryIDAssignment As Int32
    Public TerritoryIDAssignment2 As Int32
    Public TerritoryIDAssignment3 As Int32
    Public BoxFrom, BoxTo, BoxTrack As String
    Public BoxTwoFrom, BoxTwoTo, BoxTwoTrack As String
    Public BoxThreeFrom, BoxThreeTo, BoxThreeTrack As String
    Public BoxFourFrom, BoxFourTo, BoxFourTrack As String

    Public ForceBoxOne, ForceBoxTwo, ForceBoxThree, ForceBoxFour, ForceBoxNine As Boolean
    Public ForceBoxSeventeen As Boolean

    'Box Nine elements
    Public BoxNineComponents As New ArrayList()
    Public WarrantsFoundForBoxNine As New ArrayList()


    'Box Eleven track restriction list 
    Public RestrictionList As New ArrayList()
    Public BoxElevenComponents As New ArrayList()
    Public CompleteCompSelection As ArrayList

    Dim gHistoryMessage As String
    Dim SysEvent As SystemEventRecord
    Dim MainForm As frmMainMDI = SystemForms.MySystemForms.frmMainMDI

    Structure JointAuthorityType
        Dim WarrantNum As Integer
        Dim LeftLimits As String

        Dim RightLimits As String
        Dim IssueToName As String
    End Structure


    Dim CurrentPoint As Point
    Dim CurrentTopLocation, CurrentLeftLocation As Integer

    Dim JointAuthRequired As Boolean = False
    Dim JointAuthArray(0) As JointAuthorityType
    Dim JointAuthList As New ArrayList

    Dim ConflictCheckNeeded As Boolean

    Dim IssueToQuery As String
    Dim DirectionOfWarrant As String

    Dim SelectionFlash As Boolean
    Public WithEvents ItemLabel As Label
    Public WithEvents GroupItem As GroupBox
    Public WithEvents CheckItem As CheckBox
    Public WithEvents ToolTipItem As ToolTip
    Public WithEvents TextBoxItem As TextBox
    Public GroupArrayList As New ArrayList()
    Public CheckBoxArrayList As New ArrayList()
    Public ToolTipArrayList As New ArrayList()
    Public LabelArrayList As New ArrayList()
    Public LabelSecondaryArrayList As New ArrayList()
    Public TextBoxArrayList As New ArrayList()
    Public BoxSevenTrains As String
    Public BoxTwentySwitches As New ArrayList()
    Public BoxTwentyOneSwitches As New ArrayList()
    Public BoxSixteenBulletins As String
    Public CurrentSelectedTerritory As Territory
    '  Public CurrentSelectedTerritory As New Territory()
    Public ItemTwoWarrant, ItemFourWarrant, ItemTwenty, ItemTwentyOne As Boolean
    Public VoidedWarrant As Warrant

    Public CurrentTimeZone As String

    Dim TrackSelectionFlash As Boolean

    Private Box21Switch As New ArrayList

    Public RadioBlockLimitFrom As String
    Public RadioBlockLimitTo As String
    Public RadioBlockingEnabled As Boolean
    Friend WithEvents tmrDateCheck As System.Windows.Forms.Timer

    Public IssuedToTrain As Boolean
#End Region

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
            BoxNineComponents = Nothing
            WarrantsFoundForBoxNine = Nothing
            RestrictionList = Nothing
            BoxElevenComponents = Nothing
            CompleteCompSelection = Nothing
            GroupArrayList = Nothing
            CheckBoxArrayList = Nothing
            ToolTipArrayList = Nothing
            LabelArrayList = Nothing
            LabelSecondaryArrayList = Nothing
            TextBoxArrayList = Nothing
            BoxTwentySwitches = Nothing
            BoxTwentyOneSwitches = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents gbHeader As System.Windows.Forms.GroupBox
    Friend WithEvents lblRailroadName As System.Windows.Forms.Label
    Friend WithEvents lblDispName As System.Windows.Forms.Label
    Friend WithEvents lblCreateDate As System.Windows.Forms.Label
    Friend WithEvents lblCreateTime As System.Windows.Forms.Label
    Friend WithEvents gbMain As System.Windows.Forms.GroupBox
    Friend WithEvents lblNumber As System.Windows.Forms.Label
    Friend WithEvents gbInformation As System.Windows.Forms.GroupBox
    Friend WithEvents gbIssue As System.Windows.Forms.GroupBox
    Friend WithEvents lblAt As System.Windows.Forms.Label
    Friend WithEvents lblIssueTo As System.Windows.Forms.Label
    Friend WithEvents cbIssueTo As System.Windows.Forms.ComboBox
    Friend WithEvents cbAT As System.Windows.Forms.ComboBox
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents gbFooter As System.Windows.Forms.GroupBox
    Friend WithEvents lblGO As System.Windows.Forms.Label
    Friend WithEvents lblCTO As System.Windows.Forms.Label
    Friend WithEvents lblOK As System.Windows.Forms.Label
    Friend WithEvents lblDispInt As System.Windows.Forms.Label
    Friend WithEvents lblCopied As System.Windows.Forms.Label
    Friend WithEvents txtDISP As System.Windows.Forms.TextBox
    Friend WithEvents txtCOPIED As System.Windows.Forms.TextBox
    Friend WithEvents txtOKTIME As System.Windows.Forms.TextBox
    Friend WithEvents txtCTO As System.Windows.Forms.TextBox
    Friend WithEvents txtGO As System.Windows.Forms.TextBox
    Friend WithEvents txtAtLocation As System.Windows.Forms.TextBox
    Friend WithEvents txtIssueTo As System.Windows.Forms.TextBox
    Friend WithEvents txtSubdivision As System.Windows.Forms.TextBox
    Friend WithEvents txtDate As System.Windows.Forms.TextBox
    Friend WithEvents txtTWnum As System.Windows.Forms.TextBox
    Friend WithEvents gbButtons As System.Windows.Forms.GroupBox
    Friend WithEvents btnOK As System.Windows.Forms.Button
    Friend WithEvents btnVoid As System.Windows.Forms.Button
    Friend WithEvents btnRepeat As System.Windows.Forms.Button
    Friend WithEvents tmrSelection As System.Windows.Forms.Timer
    Friend WithEvents cmbBox11From As System.Windows.Forms.ComboBox
    Friend WithEvents cmbBox11To As System.Windows.Forms.ComboBox
    Friend WithEvents lblTimeZone As System.Windows.Forms.Label



    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Me.gbHeader = New System.Windows.Forms.GroupBox
        Me.lblTimeZone = New System.Windows.Forms.Label
        Me.lblCreateTime = New System.Windows.Forms.Label
        Me.lblCreateDate = New System.Windows.Forms.Label
        Me.lblDispName = New System.Windows.Forms.Label
        Me.lblRailroadName = New System.Windows.Forms.Label
        Me.gbMain = New System.Windows.Forms.GroupBox
        Me.cmbBox11To = New System.Windows.Forms.ComboBox
        Me.cmbBox11From = New System.Windows.Forms.ComboBox
        Me.gbIssue = New System.Windows.Forms.GroupBox
        Me.lblAt = New System.Windows.Forms.Label
        Me.lblIssueTo = New System.Windows.Forms.Label
        Me.cbIssueTo = New System.Windows.Forms.ComboBox
        Me.cbAT = New System.Windows.Forms.ComboBox
        Me.txtIssueTo = New System.Windows.Forms.TextBox
        Me.txtAtLocation = New System.Windows.Forms.TextBox
        Me.gbInformation = New System.Windows.Forms.GroupBox
        Me.txtTWnum = New System.Windows.Forms.TextBox
        Me.txtDate = New System.Windows.Forms.TextBox
        Me.txtSubdivision = New System.Windows.Forms.TextBox
        Me.lblNumber = New System.Windows.Forms.Label
        Me.CheckBox1 = New System.Windows.Forms.CheckBox
        Me.gbFooter = New System.Windows.Forms.GroupBox
        Me.txtGO = New System.Windows.Forms.TextBox
        Me.txtCTO = New System.Windows.Forms.TextBox
        Me.txtOKTIME = New System.Windows.Forms.TextBox
        Me.txtCOPIED = New System.Windows.Forms.TextBox
        Me.txtDISP = New System.Windows.Forms.TextBox
        Me.lblCopied = New System.Windows.Forms.Label
        Me.lblDispInt = New System.Windows.Forms.Label
        Me.lblOK = New System.Windows.Forms.Label
        Me.lblCTO = New System.Windows.Forms.Label
        Me.lblGO = New System.Windows.Forms.Label
        Me.gbButtons = New System.Windows.Forms.GroupBox
        Me.btnVoid = New System.Windows.Forms.Button
        Me.btnRepeat = New System.Windows.Forms.Button
        Me.btnOK = New System.Windows.Forms.Button
        Me.tmrSelection = New System.Windows.Forms.Timer(Me.components)
        Me.tmrDateCheck = New System.Windows.Forms.Timer(Me.components)
        Me.gbHeader.SuspendLayout()
        Me.gbMain.SuspendLayout()
        Me.gbIssue.SuspendLayout()
        Me.gbInformation.SuspendLayout()
        Me.gbFooter.SuspendLayout()
        Me.gbButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'gbHeader
        '
        Me.gbHeader.Controls.Add(Me.lblTimeZone)
        Me.gbHeader.Controls.Add(Me.lblCreateTime)
        Me.gbHeader.Controls.Add(Me.lblCreateDate)
        Me.gbHeader.Controls.Add(Me.lblDispName)
        Me.gbHeader.Controls.Add(Me.lblRailroadName)
        Me.gbHeader.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbHeader.Location = New System.Drawing.Point(0, 0)
        Me.gbHeader.Name = "gbHeader"
        Me.gbHeader.Size = New System.Drawing.Size(784, 56)
        Me.gbHeader.TabIndex = 0
        Me.gbHeader.TabStop = False
        '
        'lblTimeZone
        '
        Me.lblTimeZone.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTimeZone.Location = New System.Drawing.Point(12, 36)
        Me.lblTimeZone.Name = "lblTimeZone"
        Me.lblTimeZone.Size = New System.Drawing.Size(168, 16)
        Me.lblTimeZone.TabIndex = 4
        Me.lblTimeZone.Text = "TIME ZONE"
        Me.lblTimeZone.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblCreateTime
        '
        Me.lblCreateTime.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCreateTime.Location = New System.Drawing.Point(528, 36)
        Me.lblCreateTime.Name = "lblCreateTime"
        Me.lblCreateTime.Size = New System.Drawing.Size(216, 13)
        Me.lblCreateTime.TabIndex = 3
        Me.lblCreateTime.Text = "CREATE TIME"
        Me.lblCreateTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblCreateDate
        '
        Me.lblCreateDate.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCreateDate.Location = New System.Drawing.Point(536, 12)
        Me.lblCreateDate.Name = "lblCreateDate"
        Me.lblCreateDate.Size = New System.Drawing.Size(208, 13)
        Me.lblCreateDate.TabIndex = 2
        Me.lblCreateDate.Text = "CREATE DATE"
        Me.lblCreateDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblDispName
        '
        Me.lblDispName.Font = New System.Drawing.Font("Arial", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDispName.Location = New System.Drawing.Point(12, 16)
        Me.lblDispName.Name = "lblDispName"
        Me.lblDispName.Size = New System.Drawing.Size(164, 12)
        Me.lblDispName.TabIndex = 1
        Me.lblDispName.Text = "DISPATCHER"
        Me.lblDispName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblRailroadName
        '
        Me.lblRailroadName.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRailroadName.Location = New System.Drawing.Point(224, 8)
        Me.lblRailroadName.Name = "lblRailroadName"
        Me.lblRailroadName.Size = New System.Drawing.Size(304, 40)
        Me.lblRailroadName.TabIndex = 0
        Me.lblRailroadName.Text = "RAILROAD NAME"
        Me.lblRailroadName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'gbMain
        '
        Me.gbMain.Controls.Add(Me.cmbBox11To)
        Me.gbMain.Controls.Add(Me.cmbBox11From)
        Me.gbMain.Controls.Add(Me.gbIssue)
        Me.gbMain.Controls.Add(Me.gbInformation)
        Me.gbMain.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbMain.Location = New System.Drawing.Point(0, 64)
        Me.gbMain.Name = "gbMain"
        Me.gbMain.Size = New System.Drawing.Size(788, 752)
        Me.gbMain.TabIndex = 1
        Me.gbMain.TabStop = False
        '
        'cmbBox11To
        '
        Me.cmbBox11To.AllowDrop = True
        Me.cmbBox11To.Cursor = System.Windows.Forms.Cursors.Default
        Me.cmbBox11To.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBox11To.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBox11To.ForeColor = System.Drawing.Color.DarkRed
        Me.cmbBox11To.Location = New System.Drawing.Point(272, 728)
        Me.cmbBox11To.MaxDropDownItems = 50
        Me.cmbBox11To.Name = "cmbBox11To"
        Me.cmbBox11To.Size = New System.Drawing.Size(210, 23)
        Me.cmbBox11To.TabIndex = 3
        Me.cmbBox11To.Visible = False
        '
        'cmbBox11From
        '
        Me.cmbBox11From.AllowDrop = True
        Me.cmbBox11From.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbBox11From.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbBox11From.ForeColor = System.Drawing.Color.DarkRed
        Me.cmbBox11From.Location = New System.Drawing.Point(8, 724)
        Me.cmbBox11From.MaxDropDownItems = 50
        Me.cmbBox11From.Name = "cmbBox11From"
        Me.cmbBox11From.Size = New System.Drawing.Size(210, 23)
        Me.cmbBox11From.TabIndex = 2
        Me.cmbBox11From.Visible = False
        '
        'gbIssue
        '
        Me.gbIssue.Controls.Add(Me.lblAt)
        Me.gbIssue.Controls.Add(Me.lblIssueTo)
        Me.gbIssue.Controls.Add(Me.cbIssueTo)
        Me.gbIssue.Controls.Add(Me.cbAT)
        Me.gbIssue.Controls.Add(Me.txtIssueTo)
        Me.gbIssue.Controls.Add(Me.txtAtLocation)
        Me.gbIssue.Location = New System.Drawing.Point(5, 48)
        Me.gbIssue.Name = "gbIssue"
        Me.gbIssue.Size = New System.Drawing.Size(775, 48)
        Me.gbIssue.TabIndex = 1
        Me.gbIssue.TabStop = False
        '
        'lblAt
        '
        Me.lblAt.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAt.Location = New System.Drawing.Point(408, 16)
        Me.lblAt.Name = "lblAt"
        Me.lblAt.Size = New System.Drawing.Size(32, 20)
        Me.lblAt.TabIndex = 2
        Me.lblAt.Text = "AT:"
        Me.lblAt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblIssueTo
        '
        Me.lblIssueTo.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblIssueTo.Location = New System.Drawing.Point(8, 16)
        Me.lblIssueTo.Name = "lblIssueTo"
        Me.lblIssueTo.Size = New System.Drawing.Size(32, 16)
        Me.lblIssueTo.TabIndex = 0
        Me.lblIssueTo.Text = "TO:"
        Me.lblIssueTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'cbIssueTo
        '
        Me.cbIssueTo.BackColor = System.Drawing.SystemColors.HighlightText
        Me.cbIssueTo.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbIssueTo.ForeColor = System.Drawing.Color.DarkRed
        Me.cbIssueTo.Location = New System.Drawing.Point(40, 16)
        Me.cbIssueTo.Name = "cbIssueTo"
        Me.cbIssueTo.Size = New System.Drawing.Size(288, 27)
        Me.cbIssueTo.TabIndex = 1
        Me.cbIssueTo.Text = "Issue To"
        '
        'cbAT
        '
        Me.cbAT.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cbAT.ForeColor = System.Drawing.Color.DarkRed
        Me.cbAT.Location = New System.Drawing.Point(440, 12)
        Me.cbAT.Name = "cbAT"
        Me.cbAT.Size = New System.Drawing.Size(288, 27)
        Me.cbAT.TabIndex = 2
        Me.cbAT.Text = "At Location"
        '
        'txtIssueTo
        '
        Me.txtIssueTo.BackColor = System.Drawing.SystemColors.Menu
        Me.txtIssueTo.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtIssueTo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtIssueTo.Font = New System.Drawing.Font("Arial", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtIssueTo.ForeColor = System.Drawing.Color.DarkRed
        Me.txtIssueTo.Location = New System.Drawing.Point(40, 17)
        Me.txtIssueTo.MaxLength = 50
        Me.txtIssueTo.Name = "txtIssueTo"
        Me.txtIssueTo.Size = New System.Drawing.Size(288, 18)
        Me.txtIssueTo.TabIndex = 3
        Me.txtIssueTo.Text = "ISSUE TO"
        Me.txtIssueTo.Visible = False
        '
        'txtAtLocation
        '
        Me.txtAtLocation.BackColor = System.Drawing.SystemColors.Menu
        Me.txtAtLocation.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtAtLocation.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtAtLocation.Font = New System.Drawing.Font("Arial", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtAtLocation.ForeColor = System.Drawing.Color.DarkRed
        Me.txtAtLocation.Location = New System.Drawing.Point(440, 17)
        Me.txtAtLocation.MaxLength = 50
        Me.txtAtLocation.Name = "txtAtLocation"
        Me.txtAtLocation.Size = New System.Drawing.Size(288, 18)
        Me.txtAtLocation.TabIndex = 4
        Me.txtAtLocation.Text = "AT LOCATION"
        Me.txtAtLocation.Visible = False
        '
        'gbInformation
        '
        Me.gbInformation.Controls.Add(Me.txtTWnum)
        Me.gbInformation.Controls.Add(Me.txtDate)
        Me.gbInformation.Controls.Add(Me.txtSubdivision)
        Me.gbInformation.Controls.Add(Me.lblNumber)
        Me.gbInformation.Location = New System.Drawing.Point(5, 10)
        Me.gbInformation.Name = "gbInformation"
        Me.gbInformation.Size = New System.Drawing.Size(775, 38)
        Me.gbInformation.TabIndex = 0
        Me.gbInformation.TabStop = False
        '
        'txtTWnum
        '
        Me.txtTWnum.BackColor = System.Drawing.SystemColors.Menu
        Me.txtTWnum.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtTWnum.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtTWnum.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTWnum.ForeColor = System.Drawing.Color.DarkRed
        Me.txtTWnum.Location = New System.Drawing.Point(40, 16)
        Me.txtTWnum.MaxLength = 10
        Me.txtTWnum.Name = "txtTWnum"
        Me.txtTWnum.Size = New System.Drawing.Size(120, 19)
        Me.txtTWnum.TabIndex = 1
        Me.txtTWnum.Text = "XXXXX"
        Me.txtTWnum.WordWrap = False
        '
        'txtDate
        '
        Me.txtDate.BackColor = System.Drawing.SystemColors.Menu
        Me.txtDate.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtDate.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtDate.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDate.ForeColor = System.Drawing.Color.DarkRed
        Me.txtDate.Location = New System.Drawing.Point(560, 16)
        Me.txtDate.MaxLength = 15
        Me.txtDate.Name = "txtDate"
        Me.txtDate.Size = New System.Drawing.Size(176, 19)
        Me.txtDate.TabIndex = 3
        Me.txtDate.Text = "DATE"
        Me.txtDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'txtSubdivision
        '
        Me.txtSubdivision.BackColor = System.Drawing.SystemColors.Menu
        Me.txtSubdivision.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtSubdivision.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.txtSubdivision.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSubdivision.ForeColor = System.Drawing.Color.DarkRed
        Me.txtSubdivision.Location = New System.Drawing.Point(219, 16)
        Me.txtSubdivision.MaxLength = 25
        Me.txtSubdivision.Name = "txtSubdivision"
        Me.txtSubdivision.Size = New System.Drawing.Size(312, 19)
        Me.txtSubdivision.TabIndex = 2
        Me.txtSubdivision.Text = "SUBDIVISION"
        Me.txtSubdivision.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblNumber
        '
        Me.lblNumber.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNumber.Location = New System.Drawing.Point(8, 16)
        Me.lblNumber.Name = "lblNumber"
        Me.lblNumber.Size = New System.Drawing.Size(32, 20)
        Me.lblNumber.TabIndex = 0
        Me.lblNumber.Text = "No."
        Me.lblNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'CheckBox1
        '
        Me.CheckBox1.Location = New System.Drawing.Point(16, 16)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(104, 24)
        Me.CheckBox1.TabIndex = 0
        Me.CheckBox1.Text = "CheckBox1"
        '
        'gbFooter
        '
        Me.gbFooter.Controls.Add(Me.txtGO)
        Me.gbFooter.Controls.Add(Me.txtCTO)
        Me.gbFooter.Controls.Add(Me.txtOKTIME)
        Me.gbFooter.Controls.Add(Me.txtCOPIED)
        Me.gbFooter.Controls.Add(Me.txtDISP)
        Me.gbFooter.Controls.Add(Me.lblCopied)
        Me.gbFooter.Controls.Add(Me.lblDispInt)
        Me.gbFooter.Controls.Add(Me.lblOK)
        Me.gbFooter.Controls.Add(Me.lblCTO)
        Me.gbFooter.Controls.Add(Me.lblGO)
        Me.gbFooter.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.gbFooter.Location = New System.Drawing.Point(0, 824)
        Me.gbFooter.Name = "gbFooter"
        Me.gbFooter.Size = New System.Drawing.Size(576, 96)
        Me.gbFooter.TabIndex = 2
        Me.gbFooter.TabStop = False
        Me.gbFooter.Visible = False
        '
        'txtGO
        '
        Me.txtGO.BackColor = System.Drawing.SystemColors.ScrollBar
        Me.txtGO.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtGO.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtGO.ForeColor = System.Drawing.Color.Maroon
        Me.txtGO.Location = New System.Drawing.Point(312, 100)
        Me.txtGO.Name = "txtGO"
        Me.txtGO.Size = New System.Drawing.Size(40, 14)
        Me.txtGO.TabIndex = 1
        Me.txtGO.TabStop = False
        Me.txtGO.Text = "XXXX"
        Me.txtGO.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtGO.Visible = False
        '
        'txtCTO
        '
        Me.txtCTO.BackColor = System.Drawing.SystemColors.ScrollBar
        Me.txtCTO.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtCTO.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCTO.ForeColor = System.Drawing.Color.Maroon
        Me.txtCTO.Location = New System.Drawing.Point(308, 120)
        Me.txtCTO.Name = "txtCTO"
        Me.txtCTO.Size = New System.Drawing.Size(52, 14)
        Me.txtCTO.TabIndex = 1
        Me.txtCTO.TabStop = False
        Me.txtCTO.Text = "XXXX"
        Me.txtCTO.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.txtCTO.Visible = False
        '
        'txtOKTIME
        '
        Me.txtOKTIME.BackColor = System.Drawing.SystemColors.ScrollBar
        Me.txtOKTIME.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtOKTIME.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtOKTIME.ForeColor = System.Drawing.Color.DarkRed
        Me.txtOKTIME.Location = New System.Drawing.Point(40, 40)
        Me.txtOKTIME.Name = "txtOKTIME"
        Me.txtOKTIME.Size = New System.Drawing.Size(136, 14)
        Me.txtOKTIME.TabIndex = 2
        Me.txtOKTIME.Text = "HH:MM"
        Me.txtOKTIME.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtCOPIED
        '
        Me.txtCOPIED.BackColor = System.Drawing.SystemColors.ScrollBar
        Me.txtCOPIED.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtCOPIED.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCOPIED.ForeColor = System.Drawing.Color.DarkRed
        Me.txtCOPIED.Location = New System.Drawing.Point(352, 44)
        Me.txtCOPIED.Name = "txtCOPIED"
        Me.txtCOPIED.Size = New System.Drawing.Size(120, 14)
        Me.txtCOPIED.TabIndex = 1
        Me.txtCOPIED.Text = "NAME"
        Me.txtCOPIED.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtDISP
        '
        Me.txtDISP.BackColor = System.Drawing.SystemColors.Menu
        Me.txtDISP.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtDISP.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDISP.ForeColor = System.Drawing.Color.DarkRed
        Me.txtDISP.Location = New System.Drawing.Point(436, 20)
        Me.txtDISP.MaxLength = 6
        Me.txtDISP.Name = "txtDISP"
        Me.txtDISP.Size = New System.Drawing.Size(76, 14)
        Me.txtDISP.TabIndex = 3
        Me.txtDISP.Text = "INITIALS"
        Me.txtDISP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblCopied
        '
        Me.lblCopied.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCopied.Location = New System.Drawing.Point(264, 48)
        Me.lblCopied.Name = "lblCopied"
        Me.lblCopied.Size = New System.Drawing.Size(234, 16)
        Me.lblCopied.TabIndex = 7
        Me.lblCopied.Text = "COPIED BY   __________________________"
        '
        'lblDispInt
        '
        Me.lblDispInt.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDispInt.Location = New System.Drawing.Point(264, 24)
        Me.lblDispInt.Name = "lblDispInt"
        Me.lblDispInt.Size = New System.Drawing.Size(256, 16)
        Me.lblDispInt.TabIndex = 6
        Me.lblDispInt.Text = "OPERATIONS SUPERVISOR _______________________________"
        '
        'lblOK
        '
        Me.lblOK.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOK.Location = New System.Drawing.Point(8, 48)
        Me.lblOK.Name = "lblOK"
        Me.lblOK.Size = New System.Drawing.Size(224, 16)
        Me.lblOK.TabIndex = 5
        Me.lblOK.Text = "OK  ______________________ HRS."
        '
        'lblCTO
        '
        Me.lblCTO.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCTO.Location = New System.Drawing.Point(8, 120)
        Me.lblCTO.Name = "lblCTO"
        Me.lblCTO.Size = New System.Drawing.Size(352, 16)
        Me.lblCTO.TabIndex = 13
        Me.lblCTO.Text = "HIGHEST SUPERINTENDENT NOTICE IN EFFECT  ________"
        Me.lblCTO.Visible = False
        '
        'lblGO
        '
        Me.lblGO.Font = New System.Drawing.Font("Arial", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGO.Location = New System.Drawing.Point(8, 104)
        Me.lblGO.Name = "lblGO"
        Me.lblGO.Size = New System.Drawing.Size(352, 16)
        Me.lblGO.TabIndex = 12
        Me.lblGO.Text = "HIGHEST NUMBER GENERAL ORDER IN EFFECT ________"
        Me.lblGO.Visible = False
        '
        'gbButtons
        '
        Me.gbButtons.Controls.Add(Me.btnVoid)
        Me.gbButtons.Controls.Add(Me.btnRepeat)
        Me.gbButtons.Controls.Add(Me.btnOK)
        Me.gbButtons.Location = New System.Drawing.Point(584, 824)
        Me.gbButtons.Name = "gbButtons"
        Me.gbButtons.Size = New System.Drawing.Size(200, 96)
        Me.gbButtons.TabIndex = 3
        Me.gbButtons.TabStop = False
        '
        'btnVoid
        '
        Me.btnVoid.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnVoid.Location = New System.Drawing.Point(48, 56)
        Me.btnVoid.Name = "btnVoid"
        Me.btnVoid.Size = New System.Drawing.Size(104, 32)
        Me.btnVoid.TabIndex = 3
        Me.btnVoid.TabStop = False
        Me.btnVoid.Text = "VOID"
        '
        'btnRepeat
        '
        Me.btnRepeat.Enabled = False
        Me.btnRepeat.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRepeat.Location = New System.Drawing.Point(48, 16)
        Me.btnRepeat.Name = "btnRepeat"
        Me.btnRepeat.Size = New System.Drawing.Size(104, 32)
        Me.btnRepeat.TabIndex = 1
        Me.btnRepeat.Text = "REPEAT"
        '
        'btnOK
        '
        Me.btnOK.Enabled = False
        Me.btnOK.Font = New System.Drawing.Font("Arial", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnOK.Location = New System.Drawing.Point(48, 16)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(104, 32)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "OK"
        '
        'tmrSelection
        '
        Me.tmrSelection.Interval = 120
        '
        'tmrDateCheck
        '
        Me.tmrDateCheck.Enabled = True
        Me.tmrDateCheck.Interval = 1000
        '
        'frmTrackWarrantNew
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(10, 19)
        Me.ClientSize = New System.Drawing.Size(790, 790)
        Me.ControlBox = False
        Me.Controls.Add(Me.gbButtons)
        Me.Controls.Add(Me.gbFooter)
        Me.Controls.Add(Me.gbMain)
        Me.Controls.Add(Me.gbHeader)
        Me.Font = New System.Drawing.Font("Courier New", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmTrackWarrantNew"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.Text = "Track Warrant"
        Me.TopMost = True
        Me.gbHeader.ResumeLayout(False)
        Me.gbMain.ResumeLayout(False)
        Me.gbIssue.ResumeLayout(False)
        Me.gbIssue.PerformLayout()
        Me.gbInformation.ResumeLayout(False)
        Me.gbInformation.PerformLayout()
        Me.gbFooter.ResumeLayout(False)
        Me.gbFooter.PerformLayout()
        Me.gbButtons.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "BUILD THE TRACK WARRANT"

    Public Function BuildTrackWarrant(ByVal TerritoryID As Integer, ByVal SearchDirection As String, _
                                  ByVal FirstWarrantType As String, ByVal SecondWarrantType As String, _
                                  ByVal ComponentSelection As ArrayList, _
                                  ByVal ComponentSelectionSecondary As ArrayList) As Boolean
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Me.TextBoxArrayList(39).text = " "
            Dim Init, InitSecond As Boolean
            Dim CompToCheck As Object = Nothing
            Dim FirstTrackFound As String = String.Empty
            Dim SecondTrackFound As String = String.Empty
            Dim FirstAuthTypeFound As String = String.Empty
            Dim SecondAuthTypeFound As String = String.Empty
            Dim FirstItemCompCollection As New ArrayList()
            Dim ItemThreeCompCollection As ArrayList = Nothing
            Dim NewLineItemNeeded As Boolean = False

            FirstTrackSelection = FirstItemCompCollection
            SecondTrackSelection = ComponentSelectionSecondary
            DirectionOfTheWarrant = SearchDirection
            FirstTypeOfWarrant = FirstWarrantType
            SecondTypeOfWarrant = SecondWarrantType
            CompleteCompSelection = ComponentSelection

            Me.thisSubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(gBeginTrackSelect.SubdivisionID)

            For Each CompToCheck In ComponentSelection
                If Not Init Then
                    FirstTrackFound = CompToCheck.TrackName
                    FirstAuthTypeFound = CompToCheck.AuthorityType
                    Init = True
                    FirstItemCompCollection.Add(CompToCheck)
                ElseIf Not NewLineItemNeeded Then
                    'first check and see the authority type and track name (i.e. main, main 1, etc) does not change.
                    'if it does then set the boolean variable that a new line is needed
                    If CompToCheck.TrackName = FirstTrackFound And CompToCheck.AuthorityType = FirstAuthTypeFound Then
                        FirstItemCompCollection.Add(CompToCheck)
                    Else
                        NewLineItemNeeded = True
                        If Mid(CompToCheck.AuthorityType, 1, 3) = "TWC" Then
                            SecondTrackFound = CompToCheck.TrackName
                            SecondAuthTypeFound = CompToCheck.AuthorityType
                            InitSecond = True
                            ItemThreeCompCollection = New ArrayList()
                            ForceBoxThree = True
                            ItemThreeCompCollection.Add(CompToCheck)
                        End If
                    End If
                Else
                    'if new line need (e.g. Box Three) then make sure that the authority limits is continuous
                    ' to the end of the selection. If a new line is needed beyond the Box Three item, then EXIT and give some feedback
                    If Not InitSecond And Mid(CompToCheck.AuthorityType, 1, 3) = "TWC" Then
                        SecondTrackFound = CompToCheck.TrackName
                        SecondAuthTypeFound = CompToCheck.AuthorityType
                        InitSecond = True
                        ItemThreeCompCollection = New ArrayList()
                        ForceBoxThree = True
                        ItemThreeCompCollection.Add(CompToCheck)
                    Else
                        If CompToCheck.TrackName = SecondTrackFound And CompToCheck.AuthorityType = SecondAuthTypeFound Then
                            ItemThreeCompCollection.Add(CompToCheck)
                        Else
                            FirstItemCompCollection.Clear()
                            ItemThreeCompCollection.Clear()
                            ErrorMessage = "TOO MANY NON-CONTINOUS TRACK TYPES FOUND IN SELECTION. EXITING!"
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            TrackWarrantEvent(ErrorMessage)
                            ClearSystemVariables()
                            Return False
                        End If
                    End If
                End If
            Next

            Select Case FirstTypeOfWarrant
                Case "ItemTwoToTrain", "ItemFourToTrain", "ItemFourToSameTrain", "ItemTwoToSameTrain", "ItemTwoSevenToTrain"
                    IssuedToTrain = True
            End Select

            'Check each track selection collection for conflict resolution
            'Check the first selection
            If FirstItemCompCollection Is Nothing Then
                ClearSystemVariables()
                Exit Function
            End If

            If Not ConflictResolution(FirstItemCompCollection, FirstWarrantType, SearchDirection) Then
                ClearSystemVariables()
                Exit Function
            Else
                'set the type of track warrant (i.e. item2 or item4)
                If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
                    ForceBoxTwo = True

                Else
                    ForceBoxFour = True
                End If

                'Set the verbiage based on direction and type of warrant (i.e. Item2 or Item4)
                Dim FirstTrack As Track = FirstItemCompCollection.Item(0)
                'Dim LastTrack As Track = FirstItemCompCollection.Item(FirstItemCompCollection.Count - 1)
                Dim LastTrack As Track = FindLastTrackInList(FirstItemCompCollection)

                Me.CurrentTimeZone = FirstTrack.TimeZone.ToUpper

                If SearchDirection = "RIGHT" Then
                    BoxFrom = FirstTrack.LeftLimitName
                    BoxTo = LastTrack.RightLimitName
                    BoxTrack = FirstTrack.TrackName
                ElseIf SearchDirection = "LEFT" Then
                    BoxFrom = FirstTrack.RightLimitName
                    BoxTo = LastTrack.LeftLimitName
                    BoxTrack = FirstTrack.TrackName
                Else
                    ErrorMessage = "Direction of Warrant Not Established!"
                    TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                    ClearSystemVariables()
                    Exit Function
                End If
                If ForceBoxTwo Then
                    BoxTwoFrom = BoxFrom
                    BoxTwoTo = BoxTo
                    BoxTwoTrack = BoxTrack
                Else
                    BoxFourFrom = BoxFrom
                    BoxFourTo = BoxTo
                    BoxFourTrack = BoxTrack
                End If
            End If


            'Check box three track selection if it exists
            If Not ItemThreeCompCollection Is Nothing Then
                If Not ConflictResolution(ItemThreeCompCollection, FirstWarrantType, SearchDirection) Then
                    ClearSystemVariables()
                    Exit Function
                Else
                    'set the type of track warrant (i.e. item3)
                    If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
                        ForceBoxThree = True
                    Else
                        ErrorMessage = "Item Four Warrants Are Not Allow In Across Disjointed Authority Types!"
                        TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return False
                    End If

                    'Set the verbiage based on direction and type of warrant FOR box3)
                    Dim FirstTrack As Track = ItemThreeCompCollection.Item(0)
                    Dim LastTrack As Track = ItemThreeCompCollection.Item(ItemThreeCompCollection.Count - 1)

                    If SearchDirection = "RIGHT" Then
                        BoxThreeFrom = FirstTrack.LeftLimitName
                        BoxThreeTo = LastTrack.RightLimitName
                        BoxThreeTrack = FirstTrack.TrackName
                    ElseIf SearchDirection = "LEFT" Then
                        BoxThreeFrom = FirstTrack.RightLimitName
                        BoxThreeTo = LastTrack.LeftLimitName
                        BoxThreeTrack = FirstTrack.TrackName
                    Else
                        ErrorMessage = "Direction of Warrant Not Established for Box Three Type!"
                        TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                        ClearSystemVariables()
                        Exit Function
                    End If
                End If
            End If

            'Check second track selection if it exists
            If Not ComponentSelectionSecondary Is Nothing Then
                If Not ConflictResolution(ComponentSelectionSecondary, SecondWarrantType, SearchDirection) Then
                    ClearSystemVariables()
                    Exit Function
                Else
                    'set the type of track warrant (i.e. item2 or item4)
                    If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
                        ForceBoxTwo = True
                    Else
                        ForceBoxFour = True
                    End If

                    'Set the verbiage based on direction and type of warrant (i.e. Item2 or Item4)
                    Dim FirstTrack2 As Track = ComponentSelectionSecondary.Item(0)
                    Dim LastTrack2 As Track = ComponentSelectionSecondary.Item(ComponentSelectionSecondary.Count - 1)


                    If SearchDirection = "RIGHT" Then
                        BoxFourFrom = FirstTrack2.LeftLimitName
                        BoxFourTo = LastTrack2.RightLimitName
                        BoxFourTrack = FirstTrack2.TrackName
                    ElseIf SearchDirection = "LEFT" Then
                        BoxFourFrom = FirstTrack2.RightLimitName
                        BoxFourTo = LastTrack2.LeftLimitName
                        BoxFourTrack = FirstTrack2.TrackName
                    Else
                        ErrorMessage = "Direction of Warrant Not Established!"
                        TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                        ClearSystemVariables()
                        Exit Function
                    End If
                End If
            End If


            'Assign territory ID of the first track picked so it can be related to the warrant
            TerritoryIDAssignment = TerritoryID



            'Fill Box Eleven Combo Boxes with limit selections
            If Not FillBoxElevenComboBoxs(ComponentSelection, SearchDirection) Then
                ErrorMessage = "Box Eleven Limits Fill Sub Error!"
                TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                ClearSystemVariables()
                Exit Function
            End If


            'Box Eleven Initial Check for the limits selected in Item 4 type warrants only
            'If Mid(FirstWarrantType, 1, 8) = "ItemFour" Then
            If FirstWarrantType = "ItemFourToTrain" Or FirstWarrantType = "ItemFourToMOW" Or FirstWarrantType = "ItemFourNineToMOW" Then
                If Not Me.BoxElevenInitialCheck(FirstItemCompCollection, FirstWarrantType) Then
                    ErrorMessage = "Box Eleven Restriction Does Not Exist On The Track Selection. EXITING!"
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    TrackWarrantEvent(ErrorMessage)
                    ClearSystemVariables()
                    Return False
                End If
            Else

            End If


            '**Added a form for person copying. DO NOT Prompt if Item 2 or 4 to same MOW
            If FirstWarrantType <> "ItemFourToSameMOW" And FirstWarrantType <> "ItemTwoToSameMOW" Then 'And Not Me.ForceBoxOne Then
                SystemForms.MySystemForms.frmCopyTo = New frmCopyTo()
                If Me.IssuedToTrain Then
                    Dim trn As TrainID = Nothing
                    If gBeginTrackSelect.TrainIDs.Count > 0 Then
                        trn = gBeginTrackSelect.TrainIDs.Item(0)
                    End If

                    If Not IsNothing(trn) Then
                        SystemForms.MySystemForms.frmCopyTo.cboPersonCopy.Text = trn.CrewCurrent.Conductor.NameOfPerson
                    End If
                Else
                    Dim mw As MOWID = Nothing
                    If gBeginTrackSelect.MOWIDs.Count > 0 Then
                        mw = gBeginTrackSelect.MOWIDs.Item(0)
                    End If

                    If Not IsNothing(mw) Then
                        SystemForms.MySystemForms.frmCopyTo.cboPersonCopy.Text = mw.Name
                    End If

                End If
                SystemForms.MySystemForms.frmCopyTo.ShowDialog()
                If SystemForms.MySystemForms.frmCopyTo.WarrantCanceled Then
                    SystemForms.MySystemForms.frmCopyTo.Close()
                    SystemForms.MySystemForms.frmCopyTo = Nothing
                    ErrorMessage = "WARRANT WAS CANCELED AT THE COPY TO PERSON FORM"
                    TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
                    ClearSystemVariables()
                    Exit Function
                End If
            End If


            '*****************Setup the track warrant form*****************************
            'Init Text and Check boxes, Init the header and footer fields on the form
            If Not SetUpTrackWarrantForm(FirstWarrantType, SecondWarrantType) Then
                ClearSystemVariables()
                Exit Function
            End If
            '*******************************End the Setup******************************



            '***************************CHECK FOR PROTECT OPEN SWITCHES*************************

            'Check for protecting open switches in TWC territory for the first component selection
            ProtectOpenSwitchCheck(FirstItemCompCollection, FirstTypeOfWarrant, SearchDirection)

            'Check for protecting open switches in TWC territory for Box Three component selection, if any
            If Not ItemThreeCompCollection Is Nothing Then
                ProtectOpenSwitchCheck(ItemThreeCompCollection, FirstTypeOfWarrant, SearchDirection)
            End If

            'Check for protecting open switches in TWC territory for the second component selection, if any
            If Not ComponentSelectionSecondary Is Nothing Then
                ProtectOpenSwitchCheck(ComponentSelectionSecondary, SecondWarrantType, SearchDirection)
            End If
            '******************************END CHECK FOR PROTECT OPEN SWITCHES***************************



            '********************************CHECK FOR TRACK TAGS*********************************
            'Check for track tags on first component selection
            If Not TrackTagCheck(FirstItemCompCollection, FirstTypeOfWarrant, SearchDirection) Then
                ClearSystemVariables()
                Exit Function
            End If

            If Not ItemThreeCompCollection Is Nothing Then
                If Not TrackTagCheck(ItemThreeCompCollection, FirstTypeOfWarrant, SearchDirection) Then
                    ClearSystemVariables()
                    Exit Function
                End If
            End If

            'Check for track tags on second component selection, if any
            If Not ComponentSelectionSecondary Is Nothing Then
                If Not TrackTagCheck(ComponentSelectionSecondary, SecondWarrantType, SearchDirection) Then
                    ClearSystemVariables()
                    Exit Function
                End If
            End If
            '******************************END CHECK FOR TRACK TAGS*******************************


            '*********************Check to see if Box Ten needs to be check based on switch position
            BoxTenCheck(FirstItemCompCollection)
            If Not ItemThreeCompCollection Is Nothing Then BoxTenCheck(ItemThreeCompCollection)
            '*******************************************************************


            '************************Re-INIT the Check Boxes, Header and footer on the track warrant form
            ReInitCheckBoxesAndForm()
            '************************************************************************************


            '***********************Check for joint authority for Box 11 and Box 12
            JointAuthorityCheck()
            '********************************************************************

            '******************Put the Item 2 and Item3 collection together to add to the track components and database
            If Not ItemThreeCompCollection Is Nothing Then
                For Each comp In ItemThreeCompCollection
                    If Not FirstItemCompCollection.Contains(comp) Then
                        FirstItemCompCollection.Add(comp)
                    End If
                Next
            End If

            If optShowBulletinTabOnWarrantCreation Then
                If Not TMDS.SystemForms.MySystemForms.frmSystemSummary Is Nothing Then
                    TMDS.SystemForms.MySystemForms.frmSystemSummary.TabControl1.SelectedIndex = 3
                End If
            End If
            If Me.IssuedToTrain Then
                If Me.cbIssueTo.Enabled Then
                    Me.cbIssueTo.Text = Me.FindPotentialIssueToTrain
                End If
                PopulateIssueToListFromTrains()

            End If
            SendKeys.Send("{TAB}")
            SendKeys.Send("{TAB}")

            BuildTrackWarrant = True

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Function

    'Public Function BuildTrackWarrant(ByVal TerritoryID As Integer, ByVal SearchDirection As String, _
    '                                  ByVal FirstWarrantType As String, ByVal SecondWarrantType As String, _
    '                                  ByVal ComponentSelection As ArrayList, _
    '                                  ByVal ComponentSelectionSecondary As ArrayList) As Boolean
    '    Try
    '        Dim comp As Object
    '        Dim ErrorMessage As String
    '        Me.TextBoxArrayList(39).text = " "
    '        Dim Init, InitSecond As Boolean
    '        Dim CompToCheck As Object = Nothing
    '        Dim FirstTrackFound As String = String.Empty
    '        Dim SecondTrackFound As String = String.Empty
    '        Dim FirstAuthTypeFound As String = String.Empty
    '        Dim SecondAuthTypeFound As String = String.Empty
    '        Dim FirstItemCompCollection As New ArrayList()
    '        Dim ItemThreeCompCollection As ArrayList = Nothing
    '        Dim NewLineItemNeeded As Boolean = False

    '        FirstTrackSelection = FirstItemCompCollection
    '        SecondTrackSelection = ComponentSelectionSecondary
    '        DirectionOfTheWarrant = SearchDirection
    '        FirstTypeOfWarrant = FirstWarrantType
    '        SecondTypeOfWarrant = SecondWarrantType
    '        CompleteCompSelection = ComponentSelection

    '        Me.thisSubdivision = TMDS.SystemUtilities.FetchData.FindSubdivisionFromUID(gBeginTrackSelect.SubdivisionID)

    '        For Each CompToCheck In ComponentSelection
    '            If Not Init Then
    '                FirstTrackFound = CompToCheck.TrackName
    '                FirstAuthTypeFound = CompToCheck.AuthorityType
    '                Init = True
    '                FirstItemCompCollection.Add(CompToCheck)
    '            ElseIf Not NewLineItemNeeded Then
    '                'first check and see the authority type and track name (i.e. main, main 1, etc) does not change.
    '                'if it does then set the boolean variable that a new line is needed
    '                If CompToCheck.TrackName = FirstTrackFound And CompToCheck.AuthorityType = FirstAuthTypeFound Then
    '                    FirstItemCompCollection.Add(CompToCheck)
    '                Else
    '                    NewLineItemNeeded = True
    '                    If Mid(CompToCheck.AuthorityType, 1, 3) = "TWC" Then
    '                        SecondTrackFound = CompToCheck.TrackName
    '                        SecondAuthTypeFound = CompToCheck.AuthorityType
    '                        InitSecond = True
    '                        ItemThreeCompCollection = New ArrayList()
    '                        ForceBoxThree = True
    '                        ItemThreeCompCollection.Add(CompToCheck)
    '                    End If
    '                End If
    '            Else
    '                'if new line need (e.g. Box Three) then make sure that the authority limits is continuous
    '                ' to the end of the selection. If a new line is needed beyond the Box Three item, then EXIT and give some feedback
    '                If Not InitSecond And Mid(CompToCheck.AuthorityType, 1, 3) = "TWC" Then
    '                    SecondTrackFound = CompToCheck.TrackName
    '                    SecondAuthTypeFound = CompToCheck.AuthorityType
    '                    InitSecond = True
    '                    ItemThreeCompCollection = New ArrayList()
    '                    ForceBoxThree = True
    '                    ItemThreeCompCollection.Add(CompToCheck)
    '                Else
    '                    If CompToCheck.TrackName = SecondTrackFound And CompToCheck.AuthorityType = SecondAuthTypeFound Then
    '                        ItemThreeCompCollection.Add(CompToCheck)
    '                    Else
    '                        FirstItemCompCollection.Clear()
    '                        ItemThreeCompCollection.Clear()
    '                        ErrorMessage = "TOO MANY NON-CONTINOUS TRACK TYPES FOUND IN SELECTION. EXITING!"
    '                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '                        TrackWarrantEvent(ErrorMessage)
    '                        ClearSystemVariables()
    '                        Return False
    '                    End If
    '                End If
    '            End If
    '        Next


    '        'Check each track selection collection for conflict resolution
    '        'Check the first selection
    '        If FirstItemCompCollection Is Nothing Then
    '            ClearSystemVariables()
    '            Exit Function
    '        End If

    '        If Not ConflictResolution(FirstItemCompCollection, FirstWarrantType, SearchDirection) Then
    '            ClearSystemVariables()
    '            Exit Function
    '        Else
    '            'set the type of track warrant (i.e. item2 or item4)
    '            If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
    '                ForceBoxTwo = True

    '            Else
    '                ForceBoxFour = True
    '            End If

    '            'Set the verbiage based on direction and type of warrant (i.e. Item2 or Item4)
    '            Dim FirstTrack As Track = FirstItemCompCollection.Item(0)
    '            'Dim LastTrack As Track = FirstItemCompCollection.Item(FirstItemCompCollection.Count - 1)
    '            Dim LastTrack As Track = FindLastTrackInList(FirstItemCompCollection)

    '            Me.CurrentTimeZone = FirstTrack.TimeZone.ToUpper

    '            If SearchDirection = "RIGHT" Then
    '                BoxFrom = FirstTrack.LeftLimitName
    '                BoxTo = LastTrack.RightLimitName
    '                BoxTrack = FirstTrack.TrackName
    '            ElseIf SearchDirection = "LEFT" Then
    '                BoxFrom = FirstTrack.RightLimitName
    '                BoxTo = LastTrack.LeftLimitName
    '                BoxTrack = FirstTrack.TrackName
    '            Else
    '                ErrorMessage = "Direction of Warrant Not Established!"
    '                TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '                ClearSystemVariables()
    '                Exit Function
    '            End If
    '            If ForceBoxTwo Then
    '                BoxTwoFrom = BoxFrom
    '                BoxTwoTo = BoxTo
    '                BoxTwoTrack = BoxTrack
    '            Else
    '                BoxFourFrom = BoxFrom
    '                BoxFourTo = BoxTo
    '                BoxFourTrack = BoxTrack
    '            End If
    '        End If


    '        'Check box three track selection if it exists
    '        If Not ItemThreeCompCollection Is Nothing Then
    '            If Not ConflictResolution(ItemThreeCompCollection, FirstWarrantType, SearchDirection) Then
    '                ClearSystemVariables()
    '                Exit Function
    '            Else
    '                'set the type of track warrant (i.e. item3)
    '                If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
    '                    ForceBoxThree = True
    '                Else
    '                    ErrorMessage = "Item Four Warrants Are Not Allow In Across Disjointed Authority Types!"
    '                    TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '                    Return False
    '                End If

    '                'Set the verbiage based on direction and type of warrant FOR box3)
    '                Dim FirstTrack As Track = ItemThreeCompCollection.Item(0)
    '                Dim LastTrack As Track = ItemThreeCompCollection.Item(ItemThreeCompCollection.Count - 1)

    '                If SearchDirection = "RIGHT" Then
    '                    BoxThreeFrom = FirstTrack.LeftLimitName
    '                    BoxThreeTo = LastTrack.RightLimitName
    '                    BoxThreeTrack = FirstTrack.TrackName
    '                ElseIf SearchDirection = "LEFT" Then
    '                    BoxThreeFrom = FirstTrack.RightLimitName
    '                    BoxThreeTo = LastTrack.LeftLimitName
    '                    BoxThreeTrack = FirstTrack.TrackName
    '                Else
    '                    ErrorMessage = "Direction of Warrant Not Established for Box Three Type!"
    '                    TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '                    ClearSystemVariables()
    '                    Exit Function
    '                End If
    '            End If
    '        End If

    '        'Check second track selection if it exists
    '        If Not ComponentSelectionSecondary Is Nothing Then
    '            If Not ConflictResolution(ComponentSelectionSecondary, SecondWarrantType, SearchDirection) Then
    '                ClearSystemVariables()
    '                Exit Function
    '            Else
    '                'set the type of track warrant (i.e. item2 or item4)
    '                If Mid(FirstWarrantType, 1, 7) = "ItemTwo" Then
    '                    ForceBoxTwo = True
    '                Else
    '                    ForceBoxFour = True
    '                End If

    '                'Set the verbiage based on direction and type of warrant (i.e. Item2 or Item4)
    '                Dim FirstTrack2 As Track = ComponentSelectionSecondary.Item(0)
    '                Dim LastTrack2 As Track = ComponentSelectionSecondary.Item(ComponentSelectionSecondary.Count - 1)


    '                If SearchDirection = "RIGHT" Then
    '                    BoxFourFrom = FirstTrack2.LeftLimitName
    '                    BoxFourTo = LastTrack2.RightLimitName
    '                    BoxFourTrack = FirstTrack2.TrackName
    '                ElseIf SearchDirection = "LEFT" Then
    '                    BoxFourFrom = FirstTrack2.RightLimitName
    '                    BoxFourTo = LastTrack2.LeftLimitName
    '                    BoxFourTrack = FirstTrack2.TrackName
    '                Else
    '                    ErrorMessage = "Direction of Warrant Not Established!"
    '                    TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '                    ClearSystemVariables()
    '                    Exit Function
    '                End If
    '            End If
    '        End If


    '        'Assign territory ID of the first track picked so it can be related to the warrant
    '        TerritoryIDAssignment = TerritoryID



    '        'Fill Box Eleven Combo Boxes with limit selections
    '        If Not FillBoxElevenComboBoxs(ComponentSelection, SearchDirection) Then
    '            ErrorMessage = "Box Eleven Limits Fill Sub Error!"
    '            TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '            ClearSystemVariables()
    '            Exit Function
    '        End If


    '        'Box Eleven Initial Check for the limits selected in Item 4 type warrants only
    '        'If Mid(FirstWarrantType, 1, 8) = "ItemFour" Then
    '        If FirstWarrantType = "ItemFourToTrain" Or FirstWarrantType = "ItemFourToMOW" Or FirstWarrantType = "ItemFourNineToMOW" Then
    '            If Not Me.BoxElevenInitialCheck(FirstItemCompCollection, FirstWarrantType) Then
    '                ErrorMessage = "Box Eleven Restriction Does Not Exist On The Track Selection. EXITING!"
    '                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    '                TrackWarrantEvent(ErrorMessage)
    '                ClearSystemVariables()
    '                Return False
    '            End If
    '        Else

    '        End If


    '        '**Added a form for person copying. DO NOT Prompt if Item 2 or 4 to same MOW
    '        If FirstWarrantType <> "ItemFourToSameMOW" And FirstWarrantType <> "ItemTwoToSameMOW" Then 'And Not Me.ForceBoxOne Then
    '            Dim trn As TrainID = Nothing
    '            If gBeginTrackSelect.TrainIDs.Count > 0 Then
    '                trn = gBeginTrackSelect.TrainIDs.Item(0)
    '            End If

    '            SystemForms.MySystemForms.frmCopyTo = New frmCopyTo()
    '            If Not IsNothing(trn) Then
    '                SystemForms.MySystemForms.frmCopyTo.cboPersonCopy.Text = trn.CrewCurrent.Conductor.NameOfPerson
    '            End If
    '            SystemForms.MySystemForms.frmCopyTo.ShowDialog()
    '            If SystemForms.MySystemForms.frmCopyTo.WarrantCanceled Then
    '                SystemForms.MySystemForms.frmCopyTo.Close()
    '                SystemForms.MySystemForms.frmCopyTo = Nothing
    '                ErrorMessage = "WARRANT WAS CANCELED AT THE COPY TO PERSON FORM"
    '                TrackWarrantEvent((ErrorMessage & " - EXIT SUB"))
    '                ClearSystemVariables()
    '                Exit Function
    '            End If
    '        End If


    '        '*****************Setup the track warrant form*****************************
    '        'Init Text and Check boxes, Init the header and footer fields on the form
    '        If Not SetUpTrackWarrantForm(FirstWarrantType, SecondWarrantType) Then
    '            ClearSystemVariables()
    '            Exit Function
    '        End If
    '        '*******************************End the Setup******************************



    '        '***************************CHECK FOR PROTECT OPEN SWITCHES*************************

    '        'Check for protecting open switches in TWC territory for the first component selection
    '        ProtectOpenSwitchCheck(FirstItemCompCollection, FirstTypeOfWarrant, SearchDirection)

    '        'Check for protecting open switches in TWC territory for Box Three component selection, if any
    '        If Not ItemThreeCompCollection Is Nothing Then
    '            ProtectOpenSwitchCheck(ItemThreeCompCollection, FirstTypeOfWarrant, SearchDirection)
    '        End If

    '        'Check for protecting open switches in TWC territory for the second component selection, if any
    '        If Not ComponentSelectionSecondary Is Nothing Then
    '            ProtectOpenSwitchCheck(ComponentSelectionSecondary, SecondWarrantType, SearchDirection)
    '        End If
    '        '******************************END CHECK FOR PROTECT OPEN SWITCHES***************************



    '        '********************************CHECK FOR TRACK TAGS*********************************
    '        'Check for track tags on first component selection
    '        If Not TrackTagCheck(FirstItemCompCollection, FirstTypeOfWarrant, SearchDirection) Then
    '            ClearSystemVariables()
    '            Exit Function
    '        End If

    '        If Not ItemThreeCompCollection Is Nothing Then
    '            If Not TrackTagCheck(ItemThreeCompCollection, FirstTypeOfWarrant, SearchDirection) Then
    '                ClearSystemVariables()
    '                Exit Function
    '            End If
    '        End If

    '        'Check for track tags on second component selection, if any
    '        If Not ComponentSelectionSecondary Is Nothing Then
    '            If Not TrackTagCheck(ComponentSelectionSecondary, SecondWarrantType, SearchDirection) Then
    '                ClearSystemVariables()
    '                Exit Function
    '            End If
    '        End If
    '        '******************************END CHECK FOR TRACK TAGS*******************************


    '        '*********************Check to see if Box Ten needs to be check based on switch position
    '        BoxTenCheck(FirstItemCompCollection)
    '        If Not ItemThreeCompCollection Is Nothing Then BoxTenCheck(ItemThreeCompCollection)
    '        '*******************************************************************


    '        '************************Re-INIT the Check Boxes, Header and footer on the track warrant form
    '        ReInitCheckBoxesAndForm()
    '        '************************************************************************************


    '        '***********************Check for joint authority for Box 11 and Box 12
    '        JointAuthorityCheck()
    '        '********************************************************************

    '        '******************Put the Item 2 and Item3 collection together to add to the track components and database
    '        If Not ItemThreeCompCollection Is Nothing Then
    '            For Each comp In ItemThreeCompCollection
    '                If Not FirstItemCompCollection.Contains(comp) Then
    '                    FirstItemCompCollection.Add(comp)
    '                End If
    '            Next
    '        End If

    '        If optShowBulletinTabOnWarrantCreation Then
    '            If Not TMDS.SystemForms.MySystemForms.frmSystemSummary Is Nothing Then
    '                TMDS.SystemForms.MySystemForms.frmSystemSummary.TabControl1.SelectedIndex = 3
    '            End If
    '        End If
    '        If Me.cbIssueTo.Enabled Then
    '                Me.cbIssueTo.Text = Me.FindPotentialIssueToTrain
    '            End If
    '            PopulateIssueToListFromTrains()

    '        BuildTrackWarrant = True

    '    Catch ex As Exception
    '        SystemException(ex.ToString)
    '        ClearSystemVariables()
    '        Me.CloseForm()
    '    End Try
    'End Function

    Private Function FindPotentialIssueToTrain() As String
        Try

            Dim PotentialIssueTo As String = String.Empty
            Dim TrainDirection As String = String.Empty

            If PotentialIssueTo = "" Then
                'If gBeginTrackSelect.Authorities.Count > 1 Then
                If gBeginTrackSelect.TrainIDs.Count > 0 Then
                    Dim trn As TrainID = gBeginTrackSelect.TrainIDs.Item(0)
                    PotentialIssueTo = trn.IdentifingUnit
                End If
                'End If
            End If
            If PotentialIssueTo <> "" Then
                Select Case FirstTypeOfWarrant
                    Case "ItemTwoToTrain", "ItemTwoFiveToTrain"
                        If gSearchDirection = "LEFT" Then
                            If Not IsNothing(thisSubdivision) Then
                                'If thisSubdivision.SubDirection Then
                                '    TrainDirection = "SOUTH"
                                'Else
                                '    TrainDirection = "WEST"
                                'End If
                                If CurrentSelectedTerritory IsNot Nothing AndAlso optEnableWatcoSpecificOptions Then
                                    If CurrentSelectedTerritory.FlipDirection Then
                                        If thisSubdivision.SubDirection Then
                                            TrainDirection = "NORTH"
                                        Else
                                            TrainDirection = "EAST"
                                        End If
                                    Else
                                        If thisSubdivision.SubDirection Then
                                            TrainDirection = "SOUTH"
                                        Else
                                            TrainDirection = "WEST"
                                        End If
                                    End If
                                Else
                                    If thisSubdivision.SubDirection Then
                                        TrainDirection = "NORTH"
                                    Else
                                        TrainDirection = "EAST"
                                    End If
                                End If


                            Else
                                TrainDirection = "WEST"
                            End If

                        Else
                            If Not IsNothing(thisSubdivision) Then
                                'If thisSubdivision.SubDirection Then
                                '    TrainDirection = "NORTH"
                                'Else
                                '    TrainDirection = "EAST"
                                'End If

                                If CurrentSelectedTerritory IsNot Nothing AndAlso optEnableWatcoSpecificOptions Then
                                    If CurrentSelectedTerritory.FlipDirection Then
                                        If thisSubdivision.SubDirection Then
                                            TrainDirection = "SOUTH"
                                        Else
                                            TrainDirection = "WEST"
                                        End If
                                    Else
                                        If thisSubdivision.SubDirection Then
                                            TrainDirection = "NORTH"
                                        Else
                                            TrainDirection = "EAST"
                                        End If
                                    End If
                                Else
                                    If thisSubdivision.SubDirection Then
                                        TrainDirection = "NORTH"
                                    Else
                                        TrainDirection = "EAST"
                                    End If
                                End If


                            Else
                                TrainDirection = "EAST"
                            End If
                        End If
                End Select
                PotentialIssueTo = PotentialIssueTo & " " & TrainDirection
            End If
            Return PotentialIssueTo
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Public Sub PopulateIssueToListFromTrains()
        Try
            Dim trn As TrainID
            For Each trn In TrainCollection
                If Not Me.cbIssueTo.Items.Contains(trn.IdentifingUnit & " " & trn.Direction) Then
                    Me.cbIssueTo.Items.Insert(0, trn.IdentifingUnit & " " & trn.Direction)
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub

    Private Function FindLastTrackInList(ByVal CompList As ArrayList) As Track
        Try
            Dim CompListReverse As ArrayList = CompList.Clone
            CompListReverse.Reverse()
            Dim comp As Object
            For Each comp In CompListReverse
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    Return comp
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

#End Region

#Region "PROTECT OPEN SWITCH"

    Private Sub ProtectOpenSwitchCheck(ByVal ComponentList As ArrayList, ByVal FirstTypeOfWarrant As String, ByVal SearchDirection As String)

        '*******************************PROTECT OPEN SWITCH - PROTECT LOGIC
        Dim CompToInspect As Object
        'Dim Box20Counter As Integer = 1
        Dim SystemMessage As String
        Dim SignalSystemFound As Boolean

        For Each CompToInspect In ComponentList
            If CompToInspect.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                If CompToInspect.AuthorityType <> "TWC" Then
                    SignalSystemFound = True
                End If
                'If CompToInspect.ControlledReverse And CompToInspect.LeaveReversePermission Then
                If CompToInspect.ControlledReverse And CompToInspect.LeaveReversePermission And CompToInspect.ProtectOpenSwitchEnabled Then
                    SystemMessage = CompToInspect.Name & " INDICATES IN THE REVERSE POSITION. " & vbCrLf & vbCrLf & "BOX 20 WILL BE FORCED.!"

                    TrackWarrantEvent(SystemMessage)


                    If optBox20ForcedDialogBox Then
                        MessageBox.Show(MainForm, SystemMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        'MsgBox(CompToInspect.Name & " INDICATES IN THE REVERSE POSITION. " & vbCrLf & vbCrLf & "BOX 20 WILL BE FORCED.")
                    End If

                    ItemTwenty = True
                    gBoxTwelveRequired = True
                    BoxTwentySwitches.Add(CompToInspect.Name)
                    'If Box20Counter > 1 Then
                    'BoxTwentySwitches(Box20Counter) = ",   " & CompToInspect.Name
                    'Else
                    'BoxTwentySwitches(Box20Counter) = CompToInspect.Name
                    'End If
                    'Box20Counter = Box20Counter + 1
                End If
            End If
        Next
        '********************************PROTECT OPEN SWITCH - PROTECTION


        '*******************************PROTECT OPEN SWITCH -PERMISSION
        Dim CompToTest As Object
        Dim UserFeedBack As Integer
        Dim UserSwitchMess As String
        'Dim Box21Counter As Integer = 1
        Dim MainToMainMoveFound As Boolean
        Dim TurnOutPieceToTest As Object
        Dim ErrorMessage As String

        For Each CompToTest In ComponentList
            If CompToTest.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                'If CompToTest.ControlledReverse Then
                If CompToTest.ControlledReverse And CompToTest.ProtectOpenSwitchEnabled Then
                    'Test for a main to main track move. if found then do not prompt with permission to leave switch open
                    If CompToTest.RightAltLink <> 0 Then
                        Dim swToTest As Switch = CType(CompToTest, Switch)
                        MainToMainMoveFound = True

                        If CompToTest.LeftLink = gEndTrackSelect.GUID Then
                            MainToMainMoveFound = False
                        End If

                        For Each TurnOutPieceToTest In swToTest.ReverseRouteCollection
                            If TurnOutPieceToTest.guid = gBeginTrackSelect.GUID Or TurnOutPieceToTest.guid = gEndTrackSelect.GUID Then
                                MainToMainMoveFound = False
                            End If
                            'For Each TurnOutPieceToTest In ComponentList
                            'If TurnOutPieceToTest.GUID = CompToTest.RightLink Then
                            '    MainToMainMoveFound = True
                            'End If

                        Next
                    ElseIf CompToTest.LeftAltLink <> 0 Then
                        If CompToTest.RightLink = gEndTrackSelect.GUID Then
                            MainToMainMoveFound = False
                        End If

                        'For Each TurnOutPieceToTest In ComponentList
                        '    If TurnOutPieceToTest.GUID = CompToTest.LeftLink Then
                        '        MainToMainMoveFound = True
                        '    End If
                        Dim swToTest As Switch = CType(CompToTest, Switch)
                        MainToMainMoveFound = True
                        For Each TurnOutPieceToTest In swToTest.ReverseRouteCollection
                            If TurnOutPieceToTest.guid = gBeginTrackSelect.GUID Or TurnOutPieceToTest.guid = gEndTrackSelect.GUID Then
                                MainToMainMoveFound = False
                            End If
                        Next
                    End If

                    If Not MainToMainMoveFound Then 'this tests of the warrant is from main to main then no need to prompt the dispatcher
                        UserSwitchMess = CompToTest.Name & " INDICATES IN THE REVERSE POSITION. DO YOU WANT TO PROTECT IT?" & vbCrLf
                        TrackWarrantEvent(UserSwitchMess)
                        UserFeedBack = MessageBox.Show(MainForm, UserSwitchMess, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                        If UserFeedBack = 6 Then
                            ItemTwentyOne = True
                            CompToTest.LeaveReversePermission = True
                            If Not CompToTest.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                ClearSystemVariables()
                                CompToTest.LeaveReversePermission = False
                                ErrorMessage = "Protect Open Switch Database Write Error!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Sub
                            End If
                            gBoxThirteenRequired = True
                            BoxTwentyOneSwitches.Add(CompToTest.Name)

                            'If Box21Counter > 1 Then
                            'BoxTwentyOneSwitches(Box21Counter) = ",  " & CompToTest.Name
                            'Else
                            'BoxTwentyOneSwitches(Box21Counter) = CompToTest.Name
                            'End If
                            'Box21Counter = Box21Counter + 1
                        End If
                    End If
                Else
                    Dim TurnOutFound As Boolean
                    Dim SwToTest As Switch = CType(CompToTest, Switch)
                    Dim trk As Track
                    For Each trk In SwToTest.ReverseRouteCollection
                        If trk.GUID = gBeginTrackSelect.GUID Or trk.GUID = gEndTrackSelect.GUID Then
                            TurnOutFound = True
                        End If
                    Next

                    'Dim TestForTurnOut As Object
                    'For Each TestForTurnOut In ComponentList
                    '    If ((TestForTurnOut.GUID = CompToTest.LeftAltLink And CompToTest.LeftAltLink <> 0) Or _
                    '       (TestForTurnOut.GUID = CompToTest.RightAltLink And CompToTest.RightAltLink <> 0)) And CompToTest.ProtectOpenSwitchEnabled Then
                    '        TurnOutFound = True
                    '        Exit For
                    '    End If
                    'Next

                    If TurnOutFound Then
                        UserSwitchMess = CompToTest.Name & " REQUIRES A REVERSE POSITION. DO YOU WANT TO PROTECT IT?" & vbCrLf
                        UserFeedBack = MsgBox(UserSwitchMess, MsgBoxStyle.YesNo, "PERMISSION TO LEAVE SWITCH OPEN")
                        If UserFeedBack = 6 Then
                            Dim sw As Switch = CType(CompToTest, Switch)
                            ItemTwentyOne = True
                            sw.LeaveReversePermission = True
                            If Not sw.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                ErrorMessage = "Protect Open Switch Database Write Error(2)!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Exit Sub
                            End If
                            If Not sw.ControlledReverse Then
                                sw.ControlSwitch()
                            End If
                            gBoxThirteenRequired = True
                            BoxTwentyOneSwitches.Add(CompToTest.Name)
                            Me.Box21Switch.Add(CompToTest)
                        End If
                        TurnOutFound = False
                    End If
                End If
            End If
            'PriorComp = CompToTest
        Next
        '********************************PROTECT OPEN SWITCH


        'This a check for other warrants issued in the selection and where box 21 is being selected
        'to force a box nine for trains that already have authority in these limits.
        If gBoxThirteenRequired And Not gItemFiveRequired Then
            For Each CompToTest In ComponentList
                If CompToTest.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If CompToTest.Authorities.Count > 0 Then
                        Dim warr As Warrant
                        For Each warr In CompToTest.Authorities
                            'Keep track of the warrants found in the track selection
                            If warr.IssueTo <> Me.cbIssueTo.Text Then
                                If Not WarrantsFoundForBoxNine.Contains(warr) Then
                                    WarrantsFoundForBoxNine.Add(warr)
                                End If
                            End If
                        Next
                    End If
                End If
            Next

            If SignalSystemFound Then
                Dim WarrToAddToBoxNine As Warrant
                For Each WarrToAddToBoxNine In WarrantsFoundForBoxNine
                    If WarrToAddToBoxNine.WarrantType = "ItemTwoToTrain" And WarrToAddToBoxNine.Direction = gSearchDirection Then
                        gItemSixRequired = True
                        Me.ForceBoxNine = True
                        Me.CheckBoxArrayList(9).Checked = True
                        If Not Me.BoxNineComponents.Contains(WarrToAddToBoxNine.IssueTo) Then
                            Me.BoxNineComponents.Add(WarrToAddToBoxNine.IssueTo)
                        End If
                    Else
                        WarrantsFoundForBoxNine.Clear()
                        ErrorMessage = "Selection Contains a Warrant Type Not Allowed!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Exit Sub
                    End If
                Next

                If Me.ForceBoxNine Then
                    If Not Me.BoxNineInit() Then
                        Me.CloseForm()
                    End If
                    WarrantsFoundForBoxNine.Clear()
                End If
            End If
        End If

    End Sub

#End Region

#Region "CONFLICT CHECKS"

    Private Function ConflictResolution(ByVal ComponentList As ArrayList, ByVal FirstTypeOfWarrant As String, ByVal SearchDirection As String) As Boolean
        Dim ErrorMessage As String
        Try

            '************DO THE CHECK TO SEE IF A CONFLICT RESOLUTION IS NEEDED
            'Do a check of all the tracks tht are in the selection and see if they have active warrants on them
            Dim comp As Object
            For Each comp In ComponentList
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If comp.DeviceBlocked Or comp.BlockAbsolute Then
                        ErrorMessage = "Track Warrant Limits Selected Has Device Blocking! - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End If
                End If
            Next

            For Each comp In ComponentList
                If comp.ActiveTrackWarrant Or comp.AuthorityCounts > 0 Then
                    ConflictCheckNeeded = True
                End If
            Next



            gHistoryMessage = "ConflictCheckNeeded = " & ConflictCheckNeeded.ToString
            gHistoryMessage = gHistoryMessage & ". FirstTypeOfWarrant=" & FirstTypeOfWarrant
            SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
            SysEvent = Nothing
            If Not ConflictCheckNeeded Then Return True
            '************************************************


            Select Case FirstTypeOfWarrant
                Case "ItemTwoToTrain"
                    Return ConflictCheckItemTwoToTrain(ComponentList, SearchDirection, FirstTypeOfWarrant)
                Case "ItemTwoToMOW"
                    Return ConflictCheckItemTwoToMOW(ComponentList, SearchDirection)
                Case "ItemFourToTrain", "ItemFourToMOW", "ItemFourNineToMOW"
                    Return ConflictCheckItemFour(ComponentList, SearchDirection)
                Case "ItemFourToSameTrain"
                    Return ConflictCheckItemFourSameTrain(ComponentList, SearchDirection)
                Case "ItemFourToSameMOW"
                    Return ConflictCheckItemFourSameMOW(ComponentList, SearchDirection)
                Case "ItemTwoToSameMOW"
                    Return ConflictCheckItemTwoToSameMOW(ComponentList, SearchDirection)
                Case "ItemTwoToSameTrain"
                    Return ConflictCheckItemTwoToSameTrain(ComponentList, SearchDirection)
                Case "ItemTwoNineToMOW"
                    Return ConflictCheckItemTwoNineToMOW(ComponentList, SearchDirection)
                Case "ItemTwoSevenToTrain", "ItemTwoSevenToMOW"
                    Return ConflictCheckItemTwoSeven(ComponentList, SearchDirection)
                Case Else
                    Return False
            End Select
        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Function

    Private Function ConflictCheckItemTwoToTrain(ByVal ComponentList As ArrayList, ByVal SearchDirection As String, ByVal WarrantType As String) As Boolean
        Try
            Dim SignalTerritory As Boolean
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim FirstBoxNineTrain As String = String.Empty
            Dim FirstTrackInList As Track = ComponentList.Item(0)

            If FirstTrackInList.BiDirectionalABS Then SignalTerritory = True
            If FirstTrackInList.SingleABSEast And (SearchDirection = "RIGHT") Then SignalTerritory = True
            If FirstTrackInList.SingleABSWest And (SearchDirection = "LEFT") Then SignalTerritory = True

            For Each comp In ComponentList
                If SignalTerritory Then
                    If (Not comp.SingleABSEast And Not comp.SingleABSWest And Not comp.BiDirectionalABS) Then
                        ErrorMessage = "Track Warrant Limits Are Not Allowed to Have Both Signalled and Non-Signalled Limits! - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End If

                    'ADD THIS IF THERE IS A MOW ITEM TWO ISSUED BEHIND TRAIN IN SIGNAL TWC TERRITORY THEN EXIT SUB
                    If comp.AuthorityCounts > 0 Then
                        Dim warrToCheck As Warrant
                        For Each warrToCheck In comp.Authorities
                            If warrToCheck.WarrantType <> WarrantType Then
                                ErrorMessage = "Conflict Not Resolvable! Warrant Types Conflict (ITEM 2 MOW). - EXITING!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Return False
                            End If
                        Next

                    End If

                Else
                    If (comp.SingleABSEast Or comp.SingleABSWest Or comp.BiDirectionalABS) Then
                        ErrorMessage = "Track Warrant Limits Are Not Allowed to Have Both Signalled and Non-Signalled Limits! - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End If
                End If

                'If comp.BiDirectionalABS <> SignalTerritory Then
                'ErrorMessage = "Track Warrant Limits Are Incompatiable! - EXITING!"
                'TrackWarrantEvent(ErrorMessage)
                'MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                'Return False
                'End If
            Next
            For Each comp In ComponentList
                If SignalTerritory And comp.Authorities.Count >= 4 Then
                    ErrorMessage = "Too Many Track Warrant Issue In ABS. No More Then Four! - EXITING!"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            Next

            'FIRST CHECK - Make sure the overlap starts within an existing authorities
            '              since these type of track selections are normally continance of another authority
            If Not FirstTrackInList.BiDirectionalABS And Not FirstTrackInList.SingleABSEast And Not FirstTrackInList.SingleABSWest Then
                If FirstTrackInList.AuthorityCounts = 0 And Not gItemTwoToTrainWithRadioBlocking Then
                    ErrorMessage = "OverLapping Requests Need To Start In An Existing Warrant! - EXITING!"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False
                ElseIf Not gItemTwoToTrainWithRadioBlocking Then
                    ForceBoxOne = True
                End If
            End If

            'Loop through each component in selection and see if they meet the remainder of the checks per component
            For Each comp In ComponentList
                If comp.AuthorityCounts > 0 Then 'Do these checks on components that have active warrants

                    'SECOND CHECK - Make sure there is only one active warrant on these tracks
                    '               if they are TWC type tracks and not TWC-ABS type opeartion
                    If comp.AuthorityType = "TWC" And (Not comp.BiDirectionalABS And Not FirstTrackInList.SingleABSEast And Not FirstTrackInList.SingleABSWest) Then
                        If comp.AuthorityCounts >= 1 Then
                            If Not gItemTwoToTrainWithRadioBlocking Then
                                'added for item2/9 MOW on same track as itemtwototrain
                                Dim BeginTrackWarrant As Warrant
                                If FirstTrackInList.AuthorityCounts > 0 Then
                                    BeginTrackWarrant = FirstTrackInList.Authorities(0)
                                    Dim CurrentCompWarrant As Warrant = comp.Authorities.Item(0)
                                    If CurrentCompWarrant.WarrantNumber <> BeginTrackWarrant.WarrantNumber Then
                                        ErrorMessage = "Conflict Not Resolvable! Warrant Types Conflict. - EXITING!"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        ClearSystemVariables()
                                        Return False
                                    End If
                                Else 'added for item2/9 MOW on same track as itemtwototrain
                                    ErrorMessage = "Conflict Not Resolvable! Too Many Warrants . - EXITING!"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If 'added for item2/9 MOW on same track as itemtwototrain

                                Dim WarrantOnFirstTrack As Warrant
                                WarrantOnFirstTrack = FirstTrackInList.Authorities.Item(0)
                                warr = comp.Authorities.Item(0)
                                If WarrantOnFirstTrack.WarrantNumber <> warr.WarrantNumber Then
                                    ErrorMessage = "Conflict Not Resolvable! Warrant Mismatch. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                                WarrantOnFirstTrack = Nothing
                            Else 'RADIO BLOCKING
                                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    Dim trk As Track = CType(comp, Track)
                                    For Each warr In trk.Authorities
                                        If FirstBoxNineTrain = "" Then
                                            FirstBoxNineTrain = warr.IssueTo
                                            If warr.WarrantType <> "ItemTwoToTrain" Then
                                                ErrorMessage = "Conflict Not Resolvable! Warrant Not Item 2 To Train Type. - EXITING"
                                                TrackWarrantEvent(ErrorMessage)
                                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                ClearSystemVariables()
                                                Return False
                                            End If
                                        Else
                                            If warr.IssueTo <> FirstBoxNineTrain Then
                                                ErrorMessage = "Conflict Not Resolvable! Radio Blocking Overlaps To Many Authorities. - EXITING"
                                                TrackWarrantEvent(ErrorMessage)
                                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                                ClearSystemVariables()
                                                Return False

                                            End If
                                        End If
                                    Next
                                End If
                            End If

                        End If
                    ElseIf comp.AuthorityType = "TWC" And (comp.BiDirectionalABS Or FirstTrackInList.SingleABSEast Or FirstTrackInList.SingleABSWest) Then
                        If comp.Authorities.Count > 0 Then
                            Dim warrToCheck As Warrant
                            For Each warrToCheck In comp.Authorities
                                If warrToCheck.Direction <> SearchDirection Then
                                    ErrorMessage = "Conflict Not Resolvable! Opposing Warrant Found In Signal " & gTWCVariableName & " Territory. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            Next
                        End If
                    End If

                    'THIRD CHECK - Make sure the authority types match for each track
                    warr = comp.Authorities.Item(0)
                    If FirstTypeOfWarrant <> warr.WarrantType Then
                        ErrorMessage = "Conflict Not Resolvable! Conflicting Warrant Types. - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                    'FOURTH CHECK - Make sure new selection is same direction of existing warrant
                    If SearchDirection <> warr.Direction Then
                        ErrorMessage = "Conflict Not Resolvable! Opposing Direction. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                    warr = Nothing
                End If
            Next comp

            If gItemTwoToTrainWithRadioBlocking Then
                If FirstBoxNineTrain <> "" Then
                    Me.ForceBoxNine = True
                    TextBoxArrayList(13).Text = FirstBoxNineTrain
                    Me.TextBoxArrayList(13).ReadOnly = True

                    gBox17Required = True

                Else
                    ErrorMessage = "Conflict Not Resolvable! Box Nine Train Not Assigned. - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False

                End If
                For Each comp In ComponentList
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        Dim trk As Track = CType(comp, Track)
                        If trk.Authorities.Count > 0 Then
                            Dim auth As Warrant
                            Dim SameBoxNineTrainFound As Boolean
                            For Each auth In trk.Authorities
                                If auth.IssueTo = FirstBoxNineTrain Then
                                    SameBoxNineTrainFound = True
                                    If gSearchDirection = "RIGHT" Then
                                        If Me.RadioBlockLimitFrom = "" Then
                                            Me.RadioBlockLimitFrom = trk.LeftLimitName
                                        End If
                                        Me.RadioBlockLimitTo = trk.RightLimitName
                                    Else
                                        If Me.RadioBlockLimitFrom = "" Then
                                            Me.RadioBlockLimitFrom = trk.RightLimitName
                                        End If
                                        Me.RadioBlockLimitTo = trk.LeftLimitName
                                    End If
                                End If
                            Next
                            If Not SameBoxNineTrainFound Then
                                ErrorMessage = "Conflict Not Resolvable! RADIO BLOCKING TRAIN CANNOT EXTEND BEYOND BOX NINE TRAIN. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False

                            Else
                                SameBoxNineTrainFound = True
                            End If
                        Else
                            If Me.RadioBlockLimitFrom <> "" Then
                                ErrorMessage = "Conflict Not Resolvable! RADIO BLOCKING TRAIN CANNOT EXTEND BEYOND BOX NINE TRAIN. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False

                            End If
                        End If
                    End If
                Next
                TextBoxArrayList(39).Text = "RADIO BLOCKING BEHIND " & FirstBoxNineTrain & " BETWEEN " & RadioBlockLimitFrom & " AND " & RadioBlockLimitTo
                CheckBoxArrayList(9).Checked = True
                CheckBoxArrayList(17).Checked = True
                RadioBlockingEnabled = True
            End If

            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try
    End Function

    Private Function ConflictCheckItemTwoToMOW(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim FirstTrackInList As Track = ComponentList.Item(0)

            'FIRST CHECK - Make sure the overlap starts within an existing authorities
            '              since these type of track selections are normally continance of another authority
            If FirstTrackInList.AuthorityCounts = 0 Then
                ErrorMessage = "OverLapping Requests Need To Start In An Existing Warrant! - EXITING!"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If

            'Loop through each component in selection and see if they meet the remainder of the checks per component
            For Each comp In ComponentList

                If comp.AuthorityCounts > 0 Then 'Do these checks on components that have active warrants

                    'SECOND CHECK - Make sure there is only one active warrant on these tracks
                    '               if they are TWC type tracks and not TWC-ABS type opeartion
                    If comp.AuthorityType = "TWC" Then
                        If comp.AuthorityCounts > 1 Then

                            'added for item2/9 MOW on same track as itemtwototrain
                            Dim BeginTrackWarrant As Warrant
                            If FirstTrackInList.AuthorityCounts > 0 Then
                                BeginTrackWarrant = FirstTrackInList.Authorities(0)
                                Dim CurrentCompWarrant As Warrant = comp.Authorities.Item(0)
                                If CurrentCompWarrant.WarrantNumber <> BeginTrackWarrant.WarrantNumber Then
                                    ErrorMessage = "Conflict Not Resolvable! Warrant Types Conflict. - EXITING!"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            Else 'added for item2/9 MOW on same track as itemtwototrain
                                ErrorMessage = "Conflict Not Resolvable! Too Many Warrants . - EXITING!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False
                            End If 'added for item2/9 MOW on same track as itemtwototrain
                        End If
                    End If

                    'THIRD CHECK - Make sure the authority types match for each track
                    warr = comp.Authorities.Item(0)
                    If FirstTypeOfWarrant <> warr.WarrantType Then
                        ErrorMessage = "Conflict Not Resolvable! Conflicting Warrant Types. - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                    'FOURTH CHECK - Make sure new selection is same direction of existing warrant
                    If SearchDirection <> warr.Direction Then
                        ErrorMessage = "Conflict Not Resolvable! Opposing Direction. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                    Dim WarrantOnFirstTrack As Warrant
                    WarrantOnFirstTrack = FirstTrackInList.Authorities.Item(0)
                    If WarrantOnFirstTrack.WarrantNumber <> warr.WarrantNumber Then
                        ErrorMessage = "Conflict Not Resolvable! Warrant Mismatch. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If
                    WarrantOnFirstTrack = Nothing
                    warr = Nothing
                End If 'comp.AuthorityCounts
            Next comp

            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemFour(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            'Dim warr, FirstWarrantFound As Warrant
            Dim warr As Warrant
            'Dim FirstTrackInList As Track = ComponentList.Item(0)

            'Joint authority stuff for multiple Item 4
            Dim I, EachJointAuth As Integer


            'For each component in the selection collection
            For Each comp In ComponentList

                If comp.AuthorityCounts > 0 Then 'Do these checks on components that have active warrants only

                    'FIRST CHECK - Make sur that box 11 is check on each warrant
                    For Each warr In comp.Authorities
                        If FirstTypeOfWarrant <> "ItemFourNineToMOW" Then
                            If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                If warr.CheckBox(11) <> True Then
                                    ErrorMessage = "Track Warrant Number: " & warr.WarrantNumber & vbCrLf & "DOES NOT HAVE BOX 11 CHECKED!! - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            End If
                        End If
                    Next

                    'SECOND CHECK - Make sure that there are no more then four warrants on each track
                    If comp.AuthorityCounts > 4 Then
                        ErrorMessage = "Too Many Joint Authorities. Warrant Form Only Allows Four Joint Authorities. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                End If
                warr = Nothing
            Next comp

            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)

            'Do these additional checks if this is a Item 4 with a box nine checked over a single item 2 issued to a train
            '**********************************************************************************************
            '************* Conflict resolution for item 4/9 to MOW over an existing Item 2 to Train********
            '**********************************************************************************************

            If FirstTypeOfWarrant = "ItemFourNineToMOW" Then
                'Assign the first Item 2 track warrant found as the one that will be protected with the item 9
                Dim FirstInstanceOfWarrant As Boolean
                Dim FirstItemTwoWarrantNumberFound As Integer
                For Each comp In ComponentList
                    If comp.AuthorityCounts > 0 Then
                        warr = comp.Authorities(0)
                        If warr.WarrantType = "ItemTwoToTrain" Or warr.WarrantType = "ItemTwoToMOW" Then
                            FirstItemTwoWarrantNumberFound = warr.WarrantNumber
                            FirstWarrantFound = warr
                            Exit For
                        End If
                    End If
                Next comp
                warr = Nothing

                'Make sure the item4/9 is in the same direction of item 2 FirstWarrantFound
                If FirstWarrantFound.Direction <> gSearchDirection Then
                    ErrorMessage = "Conflict Not Resolvable!This Warrant Was Selected in the Opposite Direction Of Existing Warrant. - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False
                End If

                'Go through each component and inspect it's waarants for conflicts
                For Each comp In ComponentList

                    'See if this warrant matches the first instance of the item 2 being protected
                    If Not FirstInstanceOfWarrant Then
                        If comp.AuthorityCounts > 0 Then
                            warr = comp.Authorities(0)
                            If FirstWarrantFound.WarrantNumber = warr.WarrantNumber Then
                                FirstInstanceOfWarrant = True
                            End If
                        End If
                    End If

                    'if first instance of the item 2 being protected has been found in track selection AND
                    'we come upon a track that no longer has the instance of warrant assign to it's zero index, THEN
                    'exit the sub because the selection extends beyond the first instance of the item 2 being protected
                    If FirstInstanceOfWarrant Then
                        If comp.AuthorityCounts > 0 Then
                            warr = comp.Authorities(0)
                            If warr.WarrantNumber <> FirstItemTwoWarrantNumberFound Then
                                ErrorMessage = "Conflict Not Resolvable! This Warrant Cannot Extend Beyond the Existing Item 2 Warrant being Protected.- EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False
                            End If
                        ElseIf comp.AuthorityCounts = 0 Then
                            Dim CompOK As Boolean
                            If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                Dim trk As Track = CType(comp, Track)
                                If trk.NonTransverable Then
                                    CompOK = True
                                End If

                            End If
                            If Not CompOK Then
                                ErrorMessage = "Conflict Not Resolvable! This Warrant Cannot Extend Beyond the Existing Item 2 Warrant being Protected.- EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False
                            Else
                                CompOK = False
                            End If
                        End If

                    End If

                    If Not FirstInstanceOfWarrant Then
                        If comp.AuthorityCounts > 0 Then
                            Dim WarrToCheck As Warrant
                            For Each WarrToCheck In comp.Authorities
                                'check for Item 11 checked
                                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                    If Not WarrToCheck.CheckBox(11) Then
                                        ErrorMessage = "Conflict Not Resolvable! Item 11 Not Selected on Warrant:" & WarrToCheck.WarrantNumber & ". - EXITING"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        ClearSystemVariables()
                                        Return False
                                    End If
                                End If
                                'Check to make sure not an item 2 type of warrant
                                If WarrToCheck.WarrantType = "ItemTwoToTrain" Or WarrToCheck.WarrantType = "ItemTwoToMOW" Then
                                    ErrorMessage = "Conflict Not Resolvable! Two Many Warrants Found in The Selelction To Resolve. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            Next

                        End If
                    Else ' the first instance of an item 2 has been found then do these checks
                        If comp.AuthorityCounts > 0 Then
                            Dim WarrToCheck As Warrant
                            For Each WarrToCheck In comp.Authorities
                                If WarrToCheck.WarrantNumber <> FirstItemTwoWarrantNumberFound Then
                                    'check for Item 11 checked
                                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                                        If Not WarrToCheck.CheckBox(11) Then
                                            ErrorMessage = "Conflict Not Resolvable! Item 11 Not Selected on Warrant:" & WarrToCheck.WarrantNumber & ". - EXITING"
                                            TrackWarrantEvent(ErrorMessage)
                                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            ClearSystemVariables()
                                            Return False
                                        End If
                                    End If
                                    'Check to make sure not an item 2 type of warrant
                                    If WarrToCheck.WarrantType = "ItemTwoToTrain" Or WarrToCheck.WarrantType = "ItemTwoToMOW" Then
                                        ErrorMessage = "Conflict Not Resolvable! Two Many Warrants Found in The Selelction To Resolve. - EXITING"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        ClearSystemVariables()
                                        Return False
                                    End If
                                End If
                            Next
                        End If
                    End If 'Not FirstInstanceOfWarrant 
                Next
            End If

            '/////////////////////////////////////////////////////////////////////////////
            '********************************************************************************
            'Begin to build the Joint Authority Array for BOX 12 Joint Authorities,
            'by finding all the different warrant numbers
            '********************************************************************************
            Dim DuplicateWarrantNumber As Boolean = False

            'For each component in the selection collection
            For Each comp In ComponentList

                If comp.AuthorityCounts > 0 Then
                    For Each warr In comp.Authorities
                        If (warr.WarrantType = "ItemTwoToTrain" Or warr.WarrantType = "ItemTwoToMOW") Then
                            'DO NOTHING
                        ElseIf (FirstTypeOfWarrant = "ItemFourToTrain" And warr.WarrantType = "ItemFourToTrain") Then
                            If optJointTrainToTrain Then
                                If UBound(JointAuthArray) = 0 Then
                                    ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                    JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    JointAuthRequired = True
                                    gJointAuthRequired = True
                                    gBox8Required = True
                                Else
                                    'Look through all the warrant numbers to see if there is already one added to joint array
                                    For I = 1 To UBound(JointAuthArray)
                                        If JointAuthArray(I).WarrantNum = warr.WarrantNumber Then
                                            DuplicateWarrantNumber = True
                                            Exit For
                                        Else
                                            DuplicateWarrantNumber = False
                                        End If
                                    Next I
                                    'If no existing warrant number found in JointAuthArray, then add it!
                                    If Not DuplicateWarrantNumber Then
                                        ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                        JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    End If
                                End If
                            End If
                        Else
                            Dim OverrideJointAddition As Boolean
                            If Not OverrideJointAddition Then
                                If UBound(JointAuthArray) = 0 Then
                                    ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                    JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    JointAuthRequired = True
                                    gJointAuthRequired = True
                                    gBox8Required = True
                                Else
                                    'Look through all the warrant numbers to see if there is already one added to joint array
                                    For I = 1 To UBound(JointAuthArray)
                                        If JointAuthArray(I).WarrantNum = warr.WarrantNumber Then
                                            DuplicateWarrantNumber = True
                                            Exit For
                                        Else
                                            DuplicateWarrantNumber = False
                                        End If
                                    Next I
                                    'If no existing warrant number found in JointAuthArray, then add it!
                                    If Not DuplicateWarrantNumber Then
                                        ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                        JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    End If

                                End If
                            Else
                                OverrideJointAddition = False
                            End If
                        End If 'type of warrant

                    Next 'warrant
                    warr = Nothing
                End If 'Authority Counts

            Next 'component
            comp = Nothing

            'check to make sure there is no more then four joint overlapping authorities existing IN THIS SELECTION
            If UBound(JointAuthArray) > 4 Then
                ErrorMessage = "Too Many Joint Authorities. Warrant Form Only Allows Four Joint Authorities. - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If



            '*************************************************************************************
            'Now add the overlapping limits for each authority that this authority laps over.
            '*************************************************************************************
            Dim CompType As String
            For EachJointAuth = 1 To UBound(JointAuthArray)
                'Look at each component in selection
                For Each comp In ComponentList
                    'If track has a warrant existing then go to next step
                    If comp.AuthorityCounts > 0 Then
                        'for each warrant on this track see if it matches the warrant in the joint authority array
                        CompType = comp.GetType.ToString
                        If CompType = "TMDS.TrackComponents.Track" Then
                            For Each warr In comp.Authorities
                                'If it does match the one we are looking for, then go to next step
                                If JointAuthArray(EachJointAuth).WarrantNum = warr.WarrantNumber Then
                                    'Is the search direction left or right, this amkes the difference in setting the first limit name

                                    If gSearchDirection = "RIGHT" Then
                                        If JointAuthArray(EachJointAuth).LeftLimits = "" Then
                                            JointAuthArray(EachJointAuth).LeftLimits = comp.LeftLimitName
                                        End If
                                        JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                    Else
                                        If JointAuthArray(EachJointAuth).RightLimits = "" Then
                                            JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                        End If
                                        JointAuthArray(EachJointAuth).LeftLimits = comp.LeftLimitName
                                        'JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                    End If
                                    JointAuthArray(EachJointAuth).IssueToName = warr.IssueTo
                                End If
                            Next warr
                            warr = Nothing
                        End If
                    End If 'CompType
                Next comp
            Next EachJointAuth
            comp = Nothing
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemFourSameTrain(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim ItemFourWarrantNumToBeVoided As Integer


            '**************************************************************************************************************
            ''** Added to allow the dispatcher to give an item 4 to a train with item 2.
            ''** The item 2 gets voided and item 4 gets a item 11 check box


            For Each comp In ComponentList
                If comp.AuthorityCounts = 1 Then
                    Dim WarrantToCheck As Warrant
                    WarrantToCheck = comp.Authorities(0)
                    If (WarrantToCheck.WarrantType = "ItemFourToMOW") Or (WarrantToCheck.WarrantType = "ItemTwoToMOW") Then
                        ErrorMessage = "INCOMPATIABLE WARRANT FOUND FOR THIS SELECTION! - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        WarrantToCheck = Nothing
                        Return False
                    End If
                    WarrantToCheck = Nothing
                End If
            Next

            'Loop through each component selected and find the first warrant in the selection to assign
            'the track warrant number that will be voided with box one
            For Each comp In ComponentList
                'If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                If comp.AuthorityCounts = 1 Then
                    VoidedWarrant = comp.Authorities(0)
                    ItemFourWarrantNumToBeVoided = VoidedWarrant.WarrantNumber
                    Exit For
                End If
                'End If
            Next
            If VoidedWarrant Is Nothing Then
                'If warr Is Nothing Then
                ErrorMessage = "NO ITEM 4 WARRANT FOUND IN SELECTION FOR THIS TYPE OF SELECTION! - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If

            'For each trach track in the flash array do the nested checkeds within this loop
            For Each comp In ComponentList
                'SECOND CHECK - Make sure there are NO more then one warrant active on the component
                If comp.AuthorityCounts <= 1 Then
                    If comp.AuthorityCounts > 0 Then
                        'THIRD CHECK - Make sure the first component's warrant matches all component's warrant number in each component.
                        warr = comp.Authorities(0)
                        If ItemFourWarrantNumToBeVoided <> warr.WarrantNumber Then
                            ErrorMessage = "Conflict Not Resolvable! Different Authority Number Found in Track Selection. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            VoidedWarrant = Nothing
                            Return False
                        End If
                    End If

                Else
                    ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    VoidedWarrant = Nothing
                    Return False
                End If

            Next comp
            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try
    End Function

    Private Function ConflictCheckItemFourSameMOW(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean

        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim ItemFourWarrantNumToBeVoided As Integer
            Dim I, EachJointAuth As Integer

            For Each comp In ComponentList
                If comp.AuthorityCounts = 1 Then
                    Dim WarrantToCheck As Warrant
                    WarrantToCheck = comp.Authorities(0)
                    If (WarrantToCheck.WarrantType = "ItemFourToTrain" And Not WarrantToCheck.BoxElevenComponents.Contains(comp)) Or (WarrantToCheck.WarrantType = "ItemTwoToTrain") Then
                        ErrorMessage = "INCOMPATIABLE WARRANT FOUND FOR THIS SELECTION! - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        WarrantToCheck = Nothing
                        Return False
                    End If
                    WarrantToCheck = Nothing
                End If
            Next

            'Loop through each component selected and find the first warrant in the selection to assign
            'the track warrant number that will be voided with box one
            'For Each comp In ComponentList
            '    'If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
            '    If comp.AuthorityCounts = 1 Then
            '        VoidedWarrant = comp.Authorities(0)
            '        ItemFourWarrantNumToBeVoided = VoidedWarrant.WarrantNumber
            '        Exit For
            '    End If
            '    'End If
            'Next
            If gBeginTrackSelect.Authorities.Count > 0 Then
                Dim warrOnTrack As Warrant
                For Each warrOnTrack In gBeginTrackSelect.Authorities
                    If warrOnTrack.WarrantType = "ItemFourToMOW" Then
                        VoidedWarrant = warrOnTrack
                        ItemFourWarrantNumToBeVoided = warrOnTrack.WarrantNumber
                        Exit For
                    End If
                Next
            End If
            If VoidedWarrant Is Nothing Then
                'If warr Is Nothing Then
                ErrorMessage = "NO ITEM 4 WARRANT FOUND IN SELECTION FOR THIS TYPE OF SELECTION! - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If

            'For each trach track in the flash array do the nested checkeds within this loop
            For Each comp In ComponentList
                'SECOND CHECK - Make sure there are NO more then one warrant active on the component
                If comp.AuthorityCounts <= 1 Then
                    If comp.AuthorityCounts > 0 Then
                        'THIRD CHECK - Make sure the first component's warrant matches all component's warrant number in each component.
                        warr = comp.Authorities(0)
                        If ItemFourWarrantNumToBeVoided <> warr.WarrantNumber Then
                            If Not warr.BoxElevenComponents.Contains(comp) Then
                                ErrorMessage = "Conflict Not Resolvable! Different Authority Number Found in Track Selection. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                VoidedWarrant = Nothing
                                Return False
                            End If
                        End If
                    End If

                Else
                    ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    VoidedWarrant = Nothing
                    Return False
                End If

            Next comp

            '********************************************************************************
            Dim DuplicateWarrantNumber As Boolean = False

            'For each component in the selection collection
            For Each comp In ComponentList

                If comp.AuthorityCounts > 0 Then
                    For Each warr In comp.Authorities
                        If (warr.WarrantType = "ItemTwoToTrain" Or warr.WarrantType = "ItemTwoToMOW") Then
                            'DO NOTHING
                        ElseIf (warr.WarrantType = "ItemFourToMOW") And VoidedWarrant.WarrantNumber = warr.WarrantNumber Then
                            'DO NOTHING
                        ElseIf (FirstTypeOfWarrant = "ItemFourToTrain" And warr.WarrantType = "ItemFourToTrain") Then
                            If optJointTrainToTrain Then
                                If UBound(JointAuthArray) = 0 Then
                                    ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                    JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    JointAuthRequired = True
                                    gJointAuthRequired = True
                                    gBox8Required = True
                                Else
                                    'Look through all the warrant numbers to see if there is already one added to joint array
                                    For I = 1 To UBound(JointAuthArray)
                                        If JointAuthArray(I).WarrantNum = warr.WarrantNumber Then
                                            DuplicateWarrantNumber = True
                                            Exit For
                                        Else
                                            DuplicateWarrantNumber = False
                                        End If
                                    Next I
                                    'If no existing warrant number found in JointAuthArray, then add it!
                                    If Not DuplicateWarrantNumber Then
                                        ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                        JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    End If
                                End If
                            End If
                        Else
                            Dim OverrideJointAddition As Boolean
                            If Not OverrideJointAddition Then
                                If UBound(JointAuthArray) = 0 Then
                                    ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                    JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    JointAuthRequired = True
                                    gJointAuthRequired = True
                                    gBox8Required = True
                                Else
                                    'Look through all the warrant numbers to see if there is already one added to joint array
                                    For I = 1 To UBound(JointAuthArray)
                                        If JointAuthArray(I).WarrantNum = warr.WarrantNumber Then
                                            DuplicateWarrantNumber = True
                                            Exit For
                                        Else
                                            DuplicateWarrantNumber = False
                                        End If
                                    Next I
                                    'If no existing warrant number found in JointAuthArray, then add it!
                                    If Not DuplicateWarrantNumber Then
                                        ReDim Preserve JointAuthArray(UBound(JointAuthArray) + 1)
                                        JointAuthArray(UBound(JointAuthArray)).WarrantNum = warr.WarrantNumber
                                    End If

                                End If
                            Else
                                OverrideJointAddition = False
                            End If
                        End If 'type of warrant

                    Next 'warrant
                    warr = Nothing
                End If 'Authority Counts

            Next 'component
            comp = Nothing

            'check to make sure there is no more then four joint overlapping authorities existing IN THIS SELECTION
            If UBound(JointAuthArray) > 4 Then
                ErrorMessage = "Too Many Joint Authorities. Warrant Form Only Allows Four Joint Authorities. - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If



            '*************************************************************************************
            'Now add the overlapping limits for each authority that this authority laps over.
            '*************************************************************************************
            Dim CompType As String
            For EachJointAuth = 1 To UBound(JointAuthArray)
                'Look at each component in selection
                For Each comp In ComponentList
                    'If track has a warrant existing then go to next step
                    If comp.AuthorityCounts > 0 Then
                        'for each warrant on this track see if it matches the warrant in the joint authority array
                        CompType = comp.GetType.ToString
                        If CompType = "TMDS.TrackComponents.Track" Then
                            For Each warr In comp.Authorities
                                'If it does match the one we are looking for, then go to next step
                                If JointAuthArray(EachJointAuth).WarrantNum = warr.WarrantNumber Then
                                    'Is the search direction left or right, this amkes the difference in setting the first limit name

                                    If gSearchDirection = "RIGHT" Then
                                        If JointAuthArray(EachJointAuth).LeftLimits = "" Then
                                            JointAuthArray(EachJointAuth).LeftLimits = comp.LeftLimitName
                                        End If
                                        JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                    Else
                                        If JointAuthArray(EachJointAuth).RightLimits = "" Then
                                            JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                        End If
                                        JointAuthArray(EachJointAuth).LeftLimits = comp.LeftLimitName
                                        'JointAuthArray(EachJointAuth).RightLimits = comp.RightLimitName
                                    End If
                                    JointAuthArray(EachJointAuth).IssueToName = warr.IssueTo
                                End If
                            Next warr
                            warr = Nothing
                        End If
                    End If 'CompType
                Next comp
            Next EachJointAuth
            comp = Nothing
            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemTwoToSameMOW(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean

        ''** added to allow the dispatcher to give an item 2 to a train with item 4.
        ''** The item 4 gets voided. The item two is of course sole authority on each track
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim FirstTrackInList As Track = ComponentList.Item(0)
            Dim ItemTwoWarrantNumToBeVoided As Integer

            For Each comp In ComponentList
                If comp.AuthorityCounts = 1 Then
                    Dim WarrantToCheck As Warrant
                    WarrantToCheck = comp.Authorities(0)
                    If (WarrantToCheck.WarrantType = "ItemFourToTrain") Or (WarrantToCheck.WarrantType = "ItemTwoToTrain") Then
                        ErrorMessage = "INCOMPATIABLE WARRANT FOUND FOR THIS SELECTION! - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        WarrantToCheck = Nothing
                        Return False
                    End If
                    WarrantToCheck = Nothing
                End If
            Next


            'FIRST CHECK - Make sure the overlap starts within an existing authorities
            '              since these type of track selections are normally continance of another authority
            If FirstTrackInList.AuthorityCounts = 0 Then
                ErrorMessage = "OverLapping Requests Need To Start In An Existing Item 2 Warrant! - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If

            'Assign the track warrant number that will be replaced
            warr = FirstTrackInList.Authorities(0)
            ItemTwoWarrantNumToBeVoided = warr.WarrantNumber

            'For each trach track in the flash array do the nested checkeds within this loop
            Dim warrToCheck As Warrant
            Dim FirstTrainWarrant As Warrant = Nothing
            Dim FirstTrainWarrantFound As Boolean
            Dim FirstMowWarrantFound As Boolean
            For Each comp In ComponentList
                'SECOND CHECK - Make sure there are NO more then one warrant active on the component
                If comp.AuthorityCounts <= 1 Then
                    If comp.AuthorityCounts > 0 Then
                        'THIRD CHECK - Make sure the first component's warrant matches all component's warrant number in each component.
                        warr = comp.Authorities(0)
                        If ItemTwoWarrantNumToBeVoided <> warr.WarrantNumber Then
                            ErrorMessage = "Conflict Not Resolvable! Different Authority Number Found in Track Selection. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            Return False
                        End If
                    End If

                Else
                    'Check to see if there is a situation to allow to issue a MOW warrant to same person when there is a train (BOX2) in the limits
                    For Each warrToCheck In comp.Authorities
                        If warrToCheck.WarrantType = "ItemTwoToTrain" Then
                            If Not FirstTrainWarrantFound Then
                                FirstTrainWarrantFound = True
                                WarrantsFoundForBoxNine.Add(warrToCheck)
                                FirstTrainWarrant = warrToCheck
                                Me.ForceBoxNine = True
                                gItemSixRequired = True
                            Else
                                If FirstTrainWarrant.WarrantNumber <> warrToCheck.WarrantNumber Then
                                    ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            End If
                        ElseIf warrToCheck.WarrantType = "ItemTwoToMOW" Then
                            If Not FirstMowWarrantFound Then
                                FirstMowWarrantFound = True
                                Me.VoidedWarrant = warrToCheck
                                Me.ForceBoxOne = True
                                gBoxOneRequired = True
                            Else
                                If Me.VoidedWarrant.WarrantNumber <> warrToCheck.WarrantNumber Then
                                    ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            End If
                        Else
                            ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            Return False
                        End If
                    Next
                    'ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                    'TrackWarrantEvent(ErrorMessage)
                    'MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    'ClearSystemVariables()
                    'Return False
                End If

            Next comp

            If Me.ForceBoxNine Then
                Dim WarrToAddToBoxNine As Warrant
                For Each WarrToAddToBoxNine In WarrantsFoundForBoxNine
                    If WarrToAddToBoxNine.WarrantType = "ItemTwoToTrain" Then
                        If Not Me.BoxNineComponents.Contains(WarrToAddToBoxNine.IssueTo) Then
                            Me.BoxNineComponents.Add(WarrToAddToBoxNine.IssueTo)
                        End If
                    End If
                Next

                'Go back through the track selection componentlist and check to make sure that
                'the selection does not extend beyond any other train's authority
                Dim MinuminWarrantCount As Integer
                For Each comp In ComponentList
                    Dim WarrantCheckValid As Boolean
                    If comp.AuthorityCounts > 0 Then

                        'Have a high-water mark for authority counts. 
                        'This helps to ensure this warrant does not extend beyond existing authorities
                        If comp.AuthorityCounts >= MinuminWarrantCount Then
                            MinuminWarrantCount = comp.AuthorityCounts
                        Else
                            'Check to see if existing warrant has Box ten check to allow this one to extend beyond
                            Dim WarrToCheckBoxTen As Warrant
                            For Each WarrToCheckBoxTen In comp.Authorities
                                If Not WarrToCheckBoxTen.CheckBox(10) Then
                                    ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants (NO BOX TEN). - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    WarrantsFoundForBoxNine.Clear()
                                    Return False
                                End If
                            Next
                        End If

                        Dim WarrantToCheck, WarrantToCrossCheck As Warrant
                        For Each WarrantToCheck In comp.Authorities
                            WarrantCheckValid = False
                            For Each WarrantToCrossCheck In WarrantsFoundForBoxNine
                                If WarrantToCheck.WarrantNumber = WarrantToCrossCheck.WarrantNumber _
                                Or ItemTwoWarrantNumToBeVoided = WarrantToCheck.WarrantNumber Then
                                    WarrantCheckValid = True
                                End If
                            Next

                            If Not WarrantCheckValid Then
                                ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                WarrantsFoundForBoxNine.Clear()
                                Return False
                            Else
                                WarrantCheckValid = False
                            End If
                        Next
                    Else

                        'Have a high-water mark for authority counts. 
                        'This helps to ensure this warrant does not extend beyond existing authorities
                        If comp.AuthorityCounts >= MinuminWarrantCount Then
                            MinuminWarrantCount = comp.AuthorityCounts
                        Else
                            'Check to see if existing warrant has Box ten check to allow this one to extend beyond
                            Dim WarrToCheckBoxTen As Warrant
                            For Each WarrToCheckBoxTen In WarrantsFoundForBoxNine
                                If Not WarrToCheckBoxTen.CheckBox(10) Then
                                    ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants (NO BOX TEN). - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    WarrantsFoundForBoxNine.Clear()
                                    Return False
                                End If
                            Next
                        End If


                    End If
                Next
            End If

            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemTwoToSameTrain(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean

        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim FirstTrackInList As Track = ComponentList.Item(0)
            Dim ItemTwoWarrantNumToBeVoided As Integer
            Dim CurrentBoxNineTrain As String = String.Empty

            For Each comp In ComponentList
                If comp.AuthorityCounts = 1 Then
                    Dim WarrantToCheck As Warrant
                    WarrantToCheck = comp.Authorities(0)
                    If (WarrantToCheck.WarrantType = "ItemFourToMOW") Or (WarrantToCheck.WarrantType = "ItemTwoToMOW") Then
                        ErrorMessage = "INCOMPATIABLE WARRANT FOUND FOR THIS SELECTION! - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        WarrantToCheck = Nothing
                        Return False
                    End If
                    WarrantToCheck = Nothing
                End If
            Next


            'FIRST CHECK - Make sure the overlap starts within an existing authorities
            '              since these type of track selections are normally continance of another authority
            If FirstTrackInList.AuthorityCounts = 0 Then
                ErrorMessage = "OverLapping Requests Need To Start In An Existing Item 2 Warrant! - EXITING"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Return False
            End If

            'Assign the track warrant number that will be replaced
            If Not gItemTwoSameTrainWithRadioBlocking Then
                warr = FirstTrackInList.Authorities(0)
                ItemTwoWarrantNumToBeVoided = warr.WarrantNumber

                'For each  track in the flash array do the nested checkeds within this loop
                For Each comp In ComponentList
                    'SECOND CHECK - Make sure there are NO more then one warrant active on the component
                    If comp.AuthorityCounts <= 1 Then
                        If comp.AuthorityCounts > 0 Then
                            'THIRD CHECK - Make sure the first component's warrant matches all component's warrant number in each component.
                            warr = comp.Authorities(0)
                            If ItemTwoWarrantNumToBeVoided <> warr.WarrantNumber Then
                                ErrorMessage = "Conflict Not Resolvable! Different Authority Number Found in Track Selection. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False
                            End If
                        End If

                    Else
                        ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If

                Next comp
            Else
                Dim auth As Warrant
                If gBeginTrackSelect.Authorities.Count = 0 Then
                    ErrorMessage = "OVERLAPPING REQUESTS NEED TO START IN AN EXISTING ITEM 2 AUTHORITY! - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False
                End If

                Dim AuthToVoided As Warrant = Nothing
                For Each auth In gBeginTrackSelect.Authorities
                    If auth.RadioBlockingUsed Then
                        ItemTwoWarrantNumToBeVoided = auth.WarrantNumber
                        CurrentBoxNineTrain = auth.Box9Train
                        AuthToVoided = auth
                        Exit For
                    End If
                Next
                If ItemTwoWarrantNumToBeVoided = 0 Then
                    auth = gBeginTrackSelect.Authorities.Item(0)
                    ItemTwoWarrantNumToBeVoided = auth.WarrantNumber
                    AuthToVoided = auth
                    For Each comp In ComponentList
                        If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                            Dim trk As Track = CType(comp, Track)
                            If trk.Authorities.Count > 0 Then
                                For Each auth In trk.Authorities
                                    If auth.WarrantNumber <> ItemTwoWarrantNumToBeVoided Then
                                        CurrentBoxNineTrain = auth.IssueTo
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If

                For Each comp In ComponentList
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        Dim trk As Track = CType(comp, Track)
                        If trk.Authorities.Count > 0 Then
                            For Each auth In trk.Authorities
                                If auth.WarrantNumber <> ItemTwoWarrantNumToBeVoided And auth.IssueTo <> CurrentBoxNineTrain Then
                                    If auth.Box9Train <> AuthToVoided.IssueTo Then
                                        ErrorMessage = "Conflict Not Resolvable! TOO MANY DIFFERENT AUTHORITIES FOUND IN RADIO BLOKCING LIMITS! - EXITING"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        ClearSystemVariables()
                                        Return False
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
            End If
            If gItemTwoSameTrainWithRadioBlocking Then
                If CurrentBoxNineTrain <> "" Then
                    Me.ForceBoxNine = True
                    TextBoxArrayList(13).Text = CurrentBoxNineTrain
                    Me.TextBoxArrayList(13).ReadOnly = True
                    gBox17Required = True
                Else
                    ErrorMessage = "Conflict Not Resolvable! BOX NINE TRAIN NOT ASSIGNED! - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False
                End If
                For Each comp In ComponentList
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        Dim trk As Track = CType(comp, Track)
                        If trk.Authorities.Count > 0 Then
                            Dim auth As Warrant
                            Dim SameBoxNineTrainFound As Boolean
                            Dim SameAuthToBeVoidedFound As Boolean
                            For Each auth In trk.Authorities
                                If auth.IssueTo = CurrentBoxNineTrain Then
                                    SameBoxNineTrainFound = True
                                    If gSearchDirection = "RIGHT" Then
                                        If Me.RadioBlockLimitFrom = "" Then
                                            Me.RadioBlockLimitFrom = trk.LeftLimitName
                                        End If
                                        Me.RadioBlockLimitTo = trk.RightLimitName
                                    Else
                                        If Me.RadioBlockLimitFrom = "" Then
                                            Me.RadioBlockLimitFrom = trk.RightLimitName
                                        End If
                                        Me.RadioBlockLimitTo = trk.LeftLimitName
                                    End If
                                ElseIf auth.WarrantNumber = ItemTwoWarrantNumToBeVoided Then
                                    SameAuthToBeVoidedFound = True
                                End If
                            Next
                            If Not SameBoxNineTrainFound And Not SameAuthToBeVoidedFound Then
                                ErrorMessage = "Conflict Not Resolvable! RADIO BLOCKING TRAIN CANNOT EXTEND BEYOND BOX NINE TRAIN. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False
                            Else
                                SameAuthToBeVoidedFound = False
                                SameBoxNineTrainFound = False
                            End If
                        Else
                            If Me.RadioBlockLimitFrom <> "" Then
                                ErrorMessage = "Conflict Not Resolvable! RADIO BLOCKING TRAIN CANNOT EXTEND BEYOND BOX NINE TRAIN. - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False

                            End If
                        End If
                    End If
                Next
                TextBoxArrayList(39).Text = "RADIO BLOCKING BEHIND " & CurrentBoxNineTrain & " BETWEEN " & RadioBlockLimitFrom & " AND " & RadioBlockLimitTo
                CheckBoxArrayList(9).Checked = True
                CheckBoxArrayList(17).Checked = True
                RadioBlockingEnabled = True
            End If


            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemTwoNineToMOW(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        '**********************************************************************************************
        '************* Conflict resolution for item 2/9 to MOW over an existing Item 2 to Train********
        '**********************************************************************************************

        Try
            Dim comp As Object
            Dim ErrorMessage As String

            'For each trach track in the track selection do the nested checkeds within this loop
            For Each comp In ComponentList
                If comp.AuthorityCounts > 0 Then
                    Dim WarrToCheck As Warrant
                    For Each WarrToCheck In comp.Authorities

                        'Keep track of the warrants found in the track selection
                        If Not WarrantsFoundForBoxNine.Contains(WarrToCheck) Then
                            WarrantsFoundForBoxNine.Add(WarrToCheck)
                        End If

                        'Check to make sure that there are no Item Four Type Warrants
                        If WarrToCheck.WarrantType = "ItemFourToMOW" Or WarrToCheck.WarrantType = "ItemFourToTrain" Then
                            ErrorMessage = "Conflict Not Resolvable! Conflicting Type Warrants Found. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            WarrantsFoundForBoxNine.Clear()
                            Return False
                        ElseIf WarrToCheck.WarrantType = "ItemTwoToMOW" Then
                            If Me.VoidedWarrant Is Nothing Then
                                Me.VoidedWarrant = WarrToCheck
                                Me.ForceBoxOne = True
                                gBoxOneRequired = True
                            Else
                                If WarrToCheck.WarrantNumber <> Me.VoidedWarrant.WarrantNumber Then
                                    ErrorMessage = "Conflict Not Resolvable! Conflicting Type Warrants Found. - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    WarrantsFoundForBoxNine.Clear()
                                    Return False
                                End If
                            End If
                        End If

                        'Make sure that there are no warrants in the opposing direction
                        If WarrToCheck.Direction <> SearchDirection Then
                            ErrorMessage = "Conflict Not Resolvable! Selection Not In Same Direction of Existing Warrant! - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            WarrantsFoundForBoxNine.Clear()
                            Return False
                        End If

                    Next
                End If
            Next comp

            'Go back through the track selection componentlist and check to make sure that
            'the selection does not extend beyond any other train's authority
            Dim MinuminWarrantCount As Integer
            For Each comp In ComponentList
                Dim WarrantCheckValid As Boolean
                If comp.AuthorityCounts > 0 Then

                    'Have a high-water mark for authority counts. 
                    'This helps to ensure this warrant does not extend beyond existing authorities
                    If comp.AuthorityCounts >= MinuminWarrantCount Then
                        MinuminWarrantCount = comp.AuthorityCounts
                    Else
                        'Check to see if existing warrant has Box ten check to allow this one to extend beyond
                        Dim WarrToCheckBoxTen As Warrant
                        For Each WarrToCheckBoxTen In comp.Authorities
                            If Not WarrToCheckBoxTen.CheckBox(10) Then
                                ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants (NO BOX TEN). - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                WarrantsFoundForBoxNine.Clear()
                                Return False
                            End If
                        Next
                    End If

                    Dim WarrantToCheck, WarrantToCrossCheck As Warrant
                    For Each WarrantToCheck In comp.Authorities
                        WarrantCheckValid = False
                        For Each WarrantToCrossCheck In WarrantsFoundForBoxNine
                            If WarrantToCheck.WarrantNumber = WarrantToCrossCheck.WarrantNumber Then
                                WarrantCheckValid = True
                            End If
                        Next

                        If Not WarrantCheckValid Then
                            ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            WarrantsFoundForBoxNine.Clear()
                            Return False
                        Else
                            WarrantCheckValid = False
                        End If
                    Next
                Else

                    'Have a high-water mark for authority counts. 
                    'This helps to ensure this warrant does not extend beyond existing authorities
                    If comp.AuthorityCounts >= MinuminWarrantCount Then
                        MinuminWarrantCount = comp.AuthorityCounts
                    Else
                        'Check to see if existing warrant has Box ten check to allow this one to extend beyond
                        Dim WarrToCheckBoxTen As Warrant
                        For Each WarrToCheckBoxTen In WarrantsFoundForBoxNine
                            If Not WarrToCheckBoxTen.CheckBox(10) Then
                                ErrorMessage = "Conflict Not Resolvable! This Warrant CANNOT Extend Beyond Existing Warrants (NO BOX TEN). - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                WarrantsFoundForBoxNine.Clear()
                                Return False
                            End If
                        Next
                    End If


                End If
            Next


            Dim WarrToAddToBoxNine As Warrant
            For Each WarrToAddToBoxNine In WarrantsFoundForBoxNine
                If WarrToAddToBoxNine.WarrantType = "ItemTwoToTrain" Then
                    If Not Me.BoxNineComponents.Contains(WarrToAddToBoxNine.IssueTo) Then
                        Me.BoxNineComponents.Add(WarrToAddToBoxNine.IssueTo)
                    End If
                End If
            Next


            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemTwoNineToMOWOld(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        '**********************************************************************************************
        '************* Conflict resolution for item 2/9 to MOW over an existing Item 2 to Train********
        '**********************************************************************************************

        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim warr As Warrant
            Dim ItemTwoWarrantNum As Integer

            'For each trach track in the tracl selection do the nested checkeds within this loop
            For Each comp In ComponentList
                'FIRST CHECK - Make sure there are no more then one warrant on the track
                If comp.AuthorityCounts > 1 Then
                    ErrorMessage = "Conflict Not Resolvable! Too Many Different Authorities in Track Selection For This Type Of Warrant. - EXITING"
                    TrackWarrantEvent(ErrorMessage)
                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    ClearSystemVariables()
                    Return False
                ElseIf comp.AuthorityCounts = 1 Then
                    'SECOND CHECK - Make sure the existing warrant matches the same direction of the selection.
                    warr = comp.Authorities(0)
                    If gSearchDirection <> warr.Direction Then
                        ErrorMessage = "Conflict Not Resolvable! Selection Not In Same Direction of Existing Warrant! - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If
                End If
            Next comp
            warr = Nothing
            comp = Nothing


            'Assign the first Item 2 track warrant found as the one that will be protected with the item 9
            ''Dim FirstWarrantFound As New Warrant()
            Dim FirstInstanceOfWarrant As Boolean
            For Each comp In ComponentList
                If comp.AuthorityCounts > 0 Then
                    warr = comp.Authorities(0)
                    ItemTwoWarrantNum = warr.WarrantNumber
                    FirstWarrantFound = warr
                    Exit For
                End If
            Next comp
            warr = Nothing

            'Go through track selection and find the first instance of each authority of each component and make sure it matches
            'the First Instance of the first warrant found in the first component (FirstWarrantFound)
            For Each comp In ComponentList
                If comp.AuthorityCounts > 0 Then
                    FirstInstanceOfWarrant = True
                    'If first instance of the warrant found does not match what was orginally found - EXIT SUB
                    warr = comp.Authorities(0)
                    If warr.WarrantNumber <> ItemTwoWarrantNum Then
                        ErrorMessage = "Conflict Not Resolvable! Two Many Warrants Found in The Selelction To Resolve. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False
                    End If
                End If
                warr = Nothing

                'After first instance of warrant found, make sure that the track selection does not extend beyond
                'the existing item 2 warrant to the train that this item 9 is protecting
                If FirstInstanceOfWarrant Then
                    If comp.AuthorityCounts = 0 Then
                        ErrorMessage = "Conflict Not Resolvable!This Warrant CANNOT Extend Beyond the Existing Warrant. - EXITING"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSystemVariables()
                        Return False

                    ElseIf comp.AuthorityCounts > 0 Then
                        warr = comp.Authorities(0)
                        If warr.WarrantNumber <> ItemTwoWarrantNum Then
                            'If first instance of the warrant found does not match what was orginally found - EXIT SUB
                            ErrorMessage = "Conflict Not Resolvable! Two Many Warrants Found in The Selelction To Resolve. - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            Return False
                        End If
                    End If
                End If
                warr = Nothing
            Next comp

            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)
            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function

    Private Function ConflictCheckItemTwoSeven(ByVal ComponentList As ArrayList, ByVal SearchDirection As String) As Boolean
        Try
            Dim comp As Object
            Dim ErrorMessage As String
            Dim BoxSevenTrainList As New ArrayList()


            '************NEW CHECK FOR ITEM 7 TYPE TRACK WARRANTS
            '***********************BEGIN**********************
            Dim FirstInstanceOfWarrantFound As Boolean
            Dim FirstInstanceTrackWarrant As Warrant = Nothing
            Dim SignalTerritory As Boolean
            Dim TrackToTest As Object

            Dim FirstTrackInList As Track = ComponentList.Item(0)

            'If the item seven warrant was created in bi-directional signal territory then check to insure all tracks
            'are bi-directional ABS
            If FirstTrackInList.BiDirectionalABS Then SignalTerritory = True
            For Each comp In ComponentList
                If SignalTerritory Then
                    If Not comp.BiDirectionalABS Then
                        ErrorMessage = "Track Warrant Limits Are Not Allowed to Have Both Signalled and Non-Signalled Limits! - EXITING!"
                        TrackWarrantEvent(ErrorMessage)
                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return False
                    End If
                End If
            Next

            For Each TrackToTest In ComponentList
                Dim WarrToTest As Warrant
                If TrackToTest.Authorities.Count > 0 Then
                    If Not FirstInstanceOfWarrantFound Then
                        FirstInstanceOfWarrantFound = True
                        FirstInstanceTrackWarrant = TrackToTest.Authorities(0)
                        BoxSevenTrainList.Add(FirstInstanceTrackWarrant.IssueTo)
                    End If
                    For Each WarrToTest In TrackToTest.Authorities

                        'Check for multiple trains for the Box Seven Train List
                        If Not BoxSevenTrainList.Contains(WarrToTest.IssueTo) Then
                            'if the warrant to check does not conflict with the direction then do not add it.
                            If SearchDirection <> WarrToTest.Direction Then
                                BoxSevenTrainList.Add(WarrToTest.IssueTo)
                            End If
                        End If

                        If Not SignalTerritory Then
                            If WarrToTest.Direction <> gSearchDirection And TrackToTest.Authorities.Count = 1 Then
                                If FirstInstanceTrackWarrant.WarrantNumber <> WarrToTest.WarrantNumber Then
                                    ErrorMessage = "This Type of Track Warrant Can Not Be Resolved (ITEM 7)! - EXITING"
                                    TrackWarrantEvent(ErrorMessage)
                                    MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                    ClearSystemVariables()
                                    Return False
                                End If
                            Else
                                ErrorMessage = "This Type of Track Warrant Can Not Be Resolved (ITEM 7)! - EXITING"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Return False

                            End If
                        Else
                            If WarrToTest.CheckBox(7) Then
                                Dim CheckForItem7Warrant As Warrant
                                For Each CheckForItem7Warrant In TrackToTest.Authorities
                                    Dim IssueToInString As Integer
                                    IssueToInString = InStr(WarrToTest.Box7Arrival, CheckForItem7Warrant.IssueTo)
                                    If IssueToInString <> 0 Then
                                        ErrorMessage = "THERE IS AN ITEM SEVEN WARRANT THAT EXISTS. THIS IS NOT ALLOWED! - EXITING"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                        ClearSystemVariables()
                                        Return False
                                    End If
                                Next
                            End If
                        End If

                        If (WarrToTest.WarrantType = "ItemFourToMOW") Or (WarrToTest.WarrantType = "ItemFourToTrain") Or (WarrToTest.WarrantType = "ItemTwoToMOW") Then
                            ErrorMessage = "This Type of Track Warrant Not Compatiable With An Item Seven Warrant! - EXITING"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSystemVariables()
                            Return False
                        End If
                    Next
                End If

            Next
            TrackWarrantEvent("OVERLAP IS VALIDATED FOR WARRANT TYPE:" & FirstTypeOfWarrant)

            Dim BoxSevenTrain As String
            Dim TrainCount As Integer
            For Each BoxSevenTrain In BoxSevenTrainList
                If TrainCount = 0 Then
                    BoxSevenTrains = BoxSevenTrains & "(" & BoxSevenTrain & ")"
                Else
                    BoxSevenTrains = BoxSevenTrains & "  (" & BoxSevenTrain & ")"
                End If
                Me.cbIssueTo.Items.Remove(BoxSevenTrain)
                TrainCount = TrainCount + 1
            Next


            BoxSevenTrainList = Nothing
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            SysEvent = Nothing
            Return False
        End Try

    End Function
#End Region

#Region "RESTRICTIVE TRACK TAGS CHECK"

    Private Function TrackTagCheck(ByVal ComponentList As ArrayList, ByVal FirstTypeOfWarrant As String, ByVal SearchDirection As String) As Boolean

        Dim ErrorMessage As String
        Try
            '*************************CHECK FOR TAGS ON AUTHORITIES FOR TRAINS ONLY*********************
            If FirstTypeOfWarrant <> "ItemFourToMOW" And FirstTypeOfWarrant <> "ItemTwoToMOW" Then
                'Check for Tags Count on the Track
                Dim SpeedTagCount, XingTagCount As Integer
                Dim CompToCheckTags As Object
                Dim TagToCheck As Tag
                For Each CompToCheckTags In ComponentList
                    If CompToCheckTags.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If CompToCheckTags.TrackTagLabels.Count > 0 Then
                            For Each TagToCheck In CompToCheckTags.TrackTagLabels
                                If TagToCheck.TagType = "SPEED" Then
                                    SpeedTagCount = SpeedTagCount + 1
                                    If SpeedTagCount > 2 Then
                                        ErrorMessage = "Too Many TEMPORARY SPEED TAGS IN THIS TRACK SELECTION."
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        ClearSystemVariables()
                                        Exit Function
                                    End If
                                ElseIf TagToCheck.TagType = "XING" Then
                                    XingTagCount = XingTagCount + 1
                                    If XingTagCount > 1 Then
                                        ErrorMessage = "Too Many XING TAGS IN THIS TRACK SELECTION." & vbCrLf & "RE-SELECT A SMALLER AMOUNT OF TRACKS"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        ClearSystemVariables()
                                        Exit Function
                                    End If
                                End If
                            Next
                        End If
                    End If

                Next
                CompToCheckTags = Nothing
                TagToCheck = Nothing


                'Assign Track Tags on warrant
                Dim DataString() As String
                For Each CompToCheckTags In ComponentList
                    If CompToCheckTags.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If CompToCheckTags.TrackTagLabels.Count > 0 Then
                            For Each TagToCheck In CompToCheckTags.TrackTagLabels
                                If TagToCheck.TagType = "SPEED" And Not gBox13Required Then
                                    gBox13Required = True
                                    DataString = TagToCheck.DataString.Split(Chr(124))
                                    If DataString.GetUpperBound(0) >= 4 Then
                                        Me.TextBoxArrayList(28).text = DataString(0)
                                        Me.TextBoxArrayList(29).text = "MP " & DataString(1)
                                        Me.TextBoxArrayList(30).text = "MP " & DataString(2)
                                        Me.TextBoxArrayList(31).text = DataString(3)
                                        If DataString(4) = "True" Then
                                            Dim NoTrackFlags As String = "NO TRACK FLAGS DISPLAYED FOR BOX 13 SPEED RESTRICTION."
                                            EnableBoxSeventeen()
                                            Me.TextBoxArrayList(39).text = Me.TextBoxArrayList(39).text & NoTrackFlags & "  "
                                        End If

                                        DataString = Nothing
                                        GroupArrayList(13).Enabled = True
                                        GroupArrayList(13).visible = True
                                        CheckBoxArrayList(13).ENABLED = True
                                        CheckBoxArrayList(13).Tag = "VALIDATE"
                                        CheckBoxArrayList(13).Checked = True
                                        CheckBoxArrayList(13).AccessibleDescription = "CHECKED"
                                        TextBoxArrayList(13).ReadOnly = True
                                    Else
                                        ErrorMessage = "ERROR IN TRACK TAG MESSAGE"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        ClearSystemVariables()
                                        Exit Function
                                    End If

                                ElseIf TagToCheck.TagType = "SPEED" And gBox13Required Then
                                    gBox14Required = True
                                    DataString = TagToCheck.DataString.Split(Chr(124))
                                    If DataString.GetUpperBound(0) >= 4 Then
                                        Me.TextBoxArrayList(32).text = DataString(0)
                                        Me.TextBoxArrayList(33).text = "MP " & DataString(1)
                                        Me.TextBoxArrayList(34).text = "MP " & DataString(2)
                                        Me.TextBoxArrayList(35).text = DataString(3)
                                        If DataString(4) = "True" Then
                                            Dim NoTrackFlags As String = "NO TRACK FLAGS DISPLAYED FOR BOX 14 SPEED RESTRICTION."
                                            'EnableBoxSeventeen()
                                            Me.TextBoxArrayList(39).text = Me.TextBoxArrayList(39).text & NoTrackFlags & "  "
                                        End If
                                        DataString = Nothing
                                        GroupArrayList(14).Enabled = True
                                        GroupArrayList(14).visible = True
                                        CheckBoxArrayList(14).ENABLED = True
                                        CheckBoxArrayList(14).Tag = "VALIDATE"
                                        CheckBoxArrayList(14).Checked = True
                                        CheckBoxArrayList(14).AccessibleDescription = "CHECKED"
                                        TextBoxArrayList(14).ReadOnly = True
                                    Else
                                        ErrorMessage = "ERROR IN TRACK TAG MESSAGE"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        ClearSystemVariables()
                                        Exit Function
                                    End If

                                ElseIf TagToCheck.TagType = "XING" Then
                                    DataString = TagToCheck.DataString.Split(Chr(124))
                                    Dim hasPassiveCrossing As Boolean = False
                                    If optEnablePassiveCrossing Then
                                        If DataString.GetUpperBound(0) > 2 Then
                                            Dim XingType As String = DataString(3)
                                            Select Case XingType.Trim
                                                Case "3"
                                                    hasPassiveCrossing = True
                                            End Select
                                        End If
                                    End If


                                    If DataString.GetUpperBound(0) >= 1 Then
                                        If optCrossingNotificationOptionOne Then
                                            gBox15Required = True
                                            Me.TextBoxArrayList(36).text = DataString(0) & DataString(2)
                                            Me.TextBoxArrayList(37).text = DataString(1)
                                            gCrossingWarningIndex = CInt(DataString(3))
                                            hasPassiveCrossing = False
                                        Else
                                            If optEnablePassiveCrossing Then
                                                If DataString.GetUpperBound(0) > 2 Then
                                                    Dim XingType As String = DataString(3)
                                                    Select Case XingType.Trim
                                                        Case "3"
                                                            EnableBoxSeventeen()
                                                            Me.TextBoxArrayList(39).text = GetPassiveCrossingVerbiage(DataString(0), DataString(1))
                                                        Case Else
                                                            gBox15Required = True
                                                            Me.TextBoxArrayList(36).text = DataString(0)
                                                            Me.TextBoxArrayList(37).text = DataString(1)
                                                    End Select
                                                End If

                                            Else
                                                gBox15Required = True
                                                Me.TextBoxArrayList(36).text = DataString(0)
                                                Me.TextBoxArrayList(37).text = DataString(1)
                                            End If
                                        End If

                                        DataString = Nothing
                                        If Not hasPassiveCrossing Then
                                            GroupArrayList(15).Enabled = True
                                            GroupArrayList(15).visible = True
                                            CheckBoxArrayList(15).ENABLED = True
                                            CheckBoxArrayList(15).Tag = "VALIDATE"
                                            CheckBoxArrayList(15).Checked = True
                                            CheckBoxArrayList(15).AccessibleDescription = "CHECKED"
                                            TextBoxArrayList(15).ReadOnly = True
                                        End If
                                    Else
                                        ErrorMessage = "ERROR IN TRACK TAG MESSAGE"
                                        TrackWarrantEvent(ErrorMessage)
                                        MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                        ClearSystemVariables()
                                        Exit Function
                                    End If
                                ElseIf TagToCheck.TagType = "INFO" Then
                                    EnableBoxSeventeen()
                                    Me.TextBoxArrayList(39).text = Me.TextBoxArrayList(39).text & TagToCheck.DataString & ".  "
                                End If
                            Next
                        End If
                    End If
                Next

                '***************END the assigning of Track Tags on warrant
                '******************************************************************************
            End If ' CHECK TAGS ON THE WARRANT TYPES FOR TRAINS ONLY

            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try

    End Function

    Private Sub EnableBoxSeventeen()
        'gBox17Required = True
        Me.TextBoxArrayList(39).ENABLED = True
        GroupArrayList(17).Enabled = True
        GroupArrayList(17).visible = True
        CheckBoxArrayList(17).ENABLED = True
        CheckBoxArrayList(17).Tag = "VALIDATE"
        CheckBoxArrayList(17).Checked = True
        CheckBoxArrayList(17).AccessibleDescription = "CHECKED"
        TextBoxArrayList(39).TextAlign = HorizontalAlignment.Left

    End Sub

    Private Function GetPassiveCrossingVerbiage(ByVal MP As String, ByVal RoadName As String) As String
        Dim rtnStr As String = "CROSSBUCKS DAMAGED OR MISSING AT MP "
        Try
            If MP.Trim = String.Empty Then Return String.Empty
            rtnStr = rtnStr & MP.Trim & ", " & RoadName.Trim & ". DO NOT EXCEED 15 MPH UNTIL MOVEMENT COMPLETELY OCCUPIES CROSSING."
        Catch ex As Exception

        End Try
        'Crossbucks damaged or missing at MP XX.X, ROAD NAME. Do not exceed 15 mph until movement completely occupies crossing."

        Return rtnStr
    End Function

#End Region

#Region "OTHER WARRANT CHECKS"

    Private Sub BoxTenCheck(ByVal ComponentList As ArrayList)

        '*********************Check to see if Item 10 needs check to clear the main track
        Dim SwitchCompToCheck As Object
        Dim RouteToCheck, TrackNextToSwitch As Object
        Dim FindEndOfRoute As Boolean
        Try
            If ComponentList Is Nothing Then Exit Sub
            'Dim FirstTrack As Track = ComponentList.Item(0)
            Dim LastTrack As Track = Me.FindLastTrackInList(ComponentList) '.Item(ComponentList.Count - 1)

            For Each SwitchCompToCheck In ComponentList

                If SwitchCompToCheck.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                    TrackNextToSwitch = FindObjectTypeFromGUID(SwitchCompToCheck.LeftLink)


                    If SwitchCompToCheck.TrackName = LastTrack.TrackName And _
                    SwitchCompToCheck.StationNumber = LastTrack.StationNumber And _
                    LastTrack.Name = TrackNextToSwitch.Name Then
                        If SwitchCompToCheck.ControlledReverse Then
                            If Not gEndTrackSelectionReplacement Is Nothing Then
                                If SwitchCompToCheck.LeftAltLink <> 0 And gEndTrackSelectionReplacement.AuthorityType = "NON-TWC" Then
                                    RouteToCheck = FindObjectTypeFromGUID(SwitchCompToCheck.LeftAltLink)
                                    If gSearchDirection = "LEFT" Then
                                        'NextRouteToCheck = RouteToCheck
                                        While Not FindEndOfRoute
                                            If RouteToCheck.GUID = 0 Then Exit While
                                            'If NextRouteToCheck.GUID = 0 Then Exit While
                                            RouteToCheck = FindObjectTypeFromGUID(RouteToCheck.LeftLink)
                                            If Mid(RouteToCheck.TrackName, 1, 3) <> "TWC" Then
                                                gItem10Required = True
                                                Exit While
                                            ElseIf RouteToCheck.TrackName <> SwitchCompToCheck.TrackName Then
                                                Exit While
                                            End If
                                        End While
                                    End If
                                ElseIf SwitchCompToCheck.RightAltLink <> 0 And gEndTrackSelectionReplacement.AuthorityType = "NON-TWC" Then
                                    RouteToCheck = FindObjectTypeFromGUID(SwitchCompToCheck.RightAltLink)
                                    If gSearchDirection = "RIGHT" Then
                                        While Not FindEndOfRoute
                                            If RouteToCheck.GUID = 0 Then Exit While
                                            RouteToCheck = FindObjectTypeFromGUID(RouteToCheck.RightLink)
                                            If Mid(RouteToCheck.TrackName, 1, 3) <> "TWC" Then
                                                gItem10Required = True
                                                Exit While
                                            ElseIf RouteToCheck.TrackName <> SwitchCompToCheck.TrackName Then
                                                Exit While
                                            End If
                                        End While
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                RouteToCheck = Nothing
                SwitchCompToCheck = Nothing
            Next


            '*********************End the Check to see if Item 10 needs check to clear the main track
            '****************************************************************************************
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub JointAuthorityCheck()
        'If there are joint authorities, then check Box 12 and Box 11 and fill in joint authority information
        Dim InputBoxFactor As Integer
        Dim I, EachJointAuth As Integer

        InputBoxFactor = 0
        If JointAuthRequired Then
            gJointAuthRequired = True
            gBox8Required = True

            For I = 1 To 4
                Me.LabelSecondaryArrayList(I + 1).Visible = False
                Me.TextBoxArrayList(16 + InputBoxFactor).Visible = False
                Me.TextBoxArrayList(17 + InputBoxFactor).Visible = False
                Me.TextBoxArrayList(18 + InputBoxFactor).Visible = False
                InputBoxFactor = I * 3
            Next


            For EachJointAuth = 1 To UBound(JointAuthArray)
                If Not JointAuthArray(EachJointAuth).IssueToName = Me.cbIssueTo.Text Then
                    Me.JointAuthList.Add(JointAuthArray(EachJointAuth))
                End If
            Next

            InputBoxFactor = 0
            EachJointAuth = 1
            Dim joint As JointAuthorityType
            For Each joint In JointAuthList

                Me.LabelSecondaryArrayList(EachJointAuth + 1).Visible = True
                Me.TextBoxArrayList(16 + InputBoxFactor).Text = joint.IssueToName
                If gSearchDirection = "RIGHT" Then
                    Me.TextBoxArrayList(17 + InputBoxFactor).Text = joint.LeftLimits
                    Me.TextBoxArrayList(18 + InputBoxFactor).Text = joint.RightLimits
                Else
                    Me.TextBoxArrayList(17 + InputBoxFactor).Text = joint.RightLimits
                    Me.TextBoxArrayList(18 + InputBoxFactor).Text = joint.LeftLimits
                End If

                Me.TextBoxArrayList(16 + InputBoxFactor).Visible = True
                Me.TextBoxArrayList(17 + InputBoxFactor).Visible = True
                Me.TextBoxArrayList(18 + InputBoxFactor).Visible = True

                Me.TextBoxArrayList(16 + InputBoxFactor).Tag = "VALIDATE"
                Me.TextBoxArrayList(17 + InputBoxFactor).Tag = "VALIDATE"
                Me.TextBoxArrayList(18 + InputBoxFactor).Tag = "VALIDATE"

                InputBoxFactor = EachJointAuth * 3
                EachJointAuth += 1
            Next
        End If 'JointAuthRequired


    End Sub

    Private Function BoxElevenInitialCheck(ByVal ComponentList As ArrayList, ByVal warrantypeToCheck As String) As Boolean
        Try
            Dim Comp As Object
            Dim warr As Warrant = Nothing
            For Each Comp In ComponentList
                'CHECK FOR EXISTING WARRANTS ON EACH TRACK
                If Comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If Comp.Authorities.Count > 0 Then
                        'CHECK TO SEE IF BOX 11 IS APPLIED TO TRACK
                        Dim BoxElevenChecked As Boolean = False
                        If Comp.BoxElevenRestriction Then
                            For Each warr In Comp.Authorities
                                If warr.CheckBox(11) Then
                                    BoxElevenChecked = True
                                    Exit For
                                End If
                            Next

                            'warr = Comp.Authorities.Item(0)
                            'CHECK TO SEE IF BOX 11 IS CHECKED ON THE WARRANT
                            If BoxElevenChecked Then 'warr.CheckBox(11) Then
                                Dim trk As Object
                                Dim trackFound As Boolean
                                'CHECK TO SEE IF THE TRACK REFERENCE IS A PART OF THE WARRANT's BOX 11 ARRAYLIST
                                For Each trk In warr.BoxElevenComponents
                                    If trk Is Comp Then
                                        trackFound = True
                                        Exit For
                                    End If
                                Next
                                'IF THE TRACK IS FOUND AS PART OF THE WARRANT's BOX 11 RESTRICTION LIST THEN
                                'ADD IT TO THE ABSOLUTE RESTRICTIONS LIST THAT WILL BE CHECKED AGAINST
                                'IF BOX 11 IS CHECK FOR THIS CURRENT WARRANT OR IF THE LIMITS
                                'OF THE BOX ELEVEN IS CHANGED.
                                If trackFound Then
                                    gBox8Required = True
                                    trackFound = False
                                    If Not RestrictionList.Contains(Comp) Then
                                        RestrictionList.Add(Comp)
                                    End If
                                Else
                                    Return False
                                End If
                            Else
                                Return False
                            End If
                        Else
                            If Comp.Authorities.Count = 1 Then
                                warr = Comp.Authorities.Item(0)
                                If warr.WarrantType = "ItemTwoToTrain" Then
                                    If warrantypeToCheck = "ItemFourNineToMOW" Then
                                        Return True
                                    End If
                                Else
                                    Return False
                                End If
                            Else
                                Return False
                            End If
                            'warr = Comp.Authorities.Item(0)
                            'If Not Comp.BiDirectionalABS And warr.WarrantType <> "ItemFourToMOW" And warr.WarrantType <> "ItemFourToTrain" Then
                            'Return False
                            'End If
                            Return False
                        End If

                    End If
                End If
            Next
            Return True

        Catch

        End Try
    End Function

    Private Function ResetBoxElevenLimits() As Boolean
        Try
            Dim comp As Object

            If RestrictionList.Count = 0 Then
                If Mid(FirstTypeOfWarrant, 1, 7) = "ItemTwo" Then
                    Me.cmbBox11From.Text = TextBoxArrayList(2).Text
                    Me.cmbBox11To.Text = TextBoxArrayList(3).Text
                Else
                    Me.cmbBox11From.Text = TextBoxArrayList(8).Text
                    Me.cmbBox11To.Text = TextBoxArrayList(9).Text
                End If
                Me.TextBoxArrayList(14).Text = Me.cmbBox11From.Text
                Me.TextBoxArrayList(15).Text = Me.cmbBox11To.Text
                Return True
            End If



            If DirectionOfTheWarrant = "RIGHT" Then
                comp = RestrictionList.Item(0)
                Me.cmbBox11From.Text = comp.LeftLimitName

                comp = RestrictionList.Item(RestrictionList.Count - 1)
                Me.cmbBox11To.Text = comp.RightLimitName
            Else
                comp = RestrictionList.Item(0)
                Me.cmbBox11From.Text = comp.RightLimitName

                comp = RestrictionList.Item(RestrictionList.Count - 1)
                Me.cmbBox11To.Text = comp.LeftLimitName
            End If

            BoxElevenComponents = RestrictionList
            Me.TextBoxArrayList(14).Text = Me.cmbBox11From.Text
            Me.TextBoxArrayList(15).Text = Me.cmbBox11To.Text
            Return True
        Catch
            Return False
        End Try

    End Function

    Private Function CheckBoxElevenChange(ByVal ComponentListToCheck As ArrayList, ByVal DirectionOfWarrant As String) As Boolean

        Dim CurrentBoxElevenSelection As New ArrayList()
        Dim comp As Object
        Dim BoxElevenFromFound As Boolean
        Dim BoxElevenToFound As Boolean

        Try

            'if the FROM and TO limits are the same then reset the box eleven limits
            If Me.cmbBox11From.Text = Me.cmbBox11To.Text Or Me.cmbBox11From.Text.Trim(" ") = "" Or Me.cmbBox11To.Text.Trim(" ") = "" Then
                If Me.ResetBoxElevenLimits Then
                    Return True
                End If
            End If


            CurrentBoxElevenSelection.Clear()

            'Find the current Box Eleven limits based on the From and To Text selections
            If DirectionOfTheWarrant = "RIGHT" Then

                For Each comp In ComponentListToCheck
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not BoxElevenFromFound Then
                            If comp.LeftLimitName = Me.cmbBox11From.Text Then
                                BoxElevenFromFound = True
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        Else
                            If comp.LeftLimitName = Me.cmbBox11From.Text Then
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If

                        If Not BoxElevenToFound And BoxElevenFromFound Then
                            If comp.RightLimitName = Me.cmbBox11To.Text Then
                                BoxElevenToFound = True
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        Else
                            If comp.RightLimitName = Me.cmbBox11To.Text Then
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If

                        If BoxElevenFromFound And BoxElevenToFound Then
                            'added this for OS track sections that have multiple track sections with the same 
                            'right limits name
                            If comp.RightLimitName = Me.cmbBox11To.Text Then
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                            'Exit For
                        Else
                            If BoxElevenFromFound Then
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If
                    End If
                Next
            Else
                For Each comp In ComponentListToCheck
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not BoxElevenFromFound Then
                            If comp.RightLimitName = Me.cmbBox11From.Text Then
                                BoxElevenFromFound = True
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If

                        If Not BoxElevenToFound And BoxElevenFromFound Then
                            If comp.LeftLimitName = Me.cmbBox11To.Text Then
                                BoxElevenToFound = True
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If

                        If BoxElevenFromFound And BoxElevenToFound Then
                            'added this for OS track sections that have multiple track sections with the same 
                            'left limits name
                            If comp.LeftLimitName = Me.cmbBox11To.Text Then
                                BoxElevenToFound = True
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                            'Exit For
                        Else
                            If BoxElevenFromFound Then
                                If Not CurrentBoxElevenSelection.Contains(comp) Then
                                    CurrentBoxElevenSelection.Add(comp)
                                End If
                            End If
                        End If
                    End If
                Next
            End If

            Dim RestrictionComp As Object
            Dim RestrictionCompFound As Boolean
            For Each RestrictionComp In RestrictionList
                For Each comp In CurrentBoxElevenSelection
                    If comp Is RestrictionComp Then
                        RestrictionCompFound = True
                    End If
                Next
                If Not RestrictionCompFound Then
                    If Me.ResetBoxElevenLimits Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    RestrictionCompFound = False
                End If
            Next

            If DirectionOfTheWarrant = "RIGHT" Then
                comp = CurrentBoxElevenSelection.Item(0)
                Me.cmbBox11From.Text = comp.LeftLimitName

                comp = CurrentBoxElevenSelection.Item(CurrentBoxElevenSelection.Count - 1)
                Me.cmbBox11To.Text = comp.RightLimitName
            Else
                comp = CurrentBoxElevenSelection.Item(0)
                Me.cmbBox11From.Text = comp.RightLimitName

                comp = CurrentBoxElevenSelection.Item(CurrentBoxElevenSelection.Count - 1)
                Me.cmbBox11To.Text = comp.LeftLimitName
            End If


            BoxElevenComponents = CurrentBoxElevenSelection
            Me.TextBoxArrayList(14).Text = Me.cmbBox11From.Text
            Me.TextBoxArrayList(15).Text = Me.cmbBox11To.Text
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try

    End Function

    Private Sub CheckForBoxTwentyOneSituation()
        Try
            Dim comp As Track = Me.FindLastTrackInList(CompleteCompSelection) '(CompleteCompSelection.Count - 1)
            Dim compsToTest As ArrayList = comp.GetAllOSComponents(True)
            Dim compToTest As Object
            For Each compToTest In compsToTest
                If compToTest.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                    If Not compToTest.ControlledReverse And compToTest.ProtectOpenSwitchEnabled Then
                        Dim UserMessage As String = "GIVE PERMISSION TO LEAVE " & compToTest.Name & " SWITCH OPEN? PROTECT OPEN SWITCH?"
                        Dim UserResponse As Integer
                        UserResponse = MessageBox.Show(Me, UserMessage, "LEAVE SWITCH OPEN DIALOG", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If UserResponse = 6 Then
                            compToTest.ControlSwitch()
                            If Not compToTest.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                                compToTest.ControlSwitch()
                                compToTest.ControlBrotherSwitches(compToTest.Index, compToTest.ControlledReverse)
                                Dim ErrorMessage As String = "Protect Open Switch Database Write Error! Consider The Switch Not Protected!"
                                MessageBox.Show(Me, ErrorMessage, "PROTECT OPEN SWITCH", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                SystemEvent(ErrorMessage)
                            Else
                                Box21Switch.Add(compToTest)
                                gBoxThirteenRequired = True
                                gBoxTenRequired = True
                                ItemTwentyOne = True
                                CheckBoxArrayList(21).Checked = True
                                CheckBoxArrayList(21).AccessibleDescription = "CHECKED"
                                BoxTwentyOneSwitches.Add(compToTest.Name)
                                ReInitCheckBoxesAndForm()
                                compToTest.UpdateSwitchPositionInDatabase(TMDS.GlobalDeclarations.gDatabaseConnectionStatic)
                            End If
                        End If
                    End If
                End If
            Next
            'This a check for other warrants issued in the selection and where box 21 is being selected
            'to force a box nine for trains that already have authority in these limits.
            If gBoxThirteenRequired Then
                Dim ErrorMessage As String
                For Each compToTest In Me.CompleteCompSelection
                    If compToTest.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If compToTest.Authorities.Count > 0 Then
                            Dim warr As Warrant
                            For Each warr In compToTest.Authorities
                                'Keep track of the warrants found in the track selection
                                If warr.IssueTo <> Me.cbIssueTo.Text Then
                                    If Not WarrantsFoundForBoxNine.Contains(warr) Then
                                        WarrantsFoundForBoxNine.Add(warr)
                                    End If
                                End If
                            Next
                        End If
                    End If
                Next
                Dim WarrToAddToBoxNine As Warrant
                If Not gItemFiveRequired Then
                    For Each WarrToAddToBoxNine In WarrantsFoundForBoxNine
                        If WarrToAddToBoxNine.WarrantType = "ItemTwoToTrain" Then
                            gItemSixRequired = True
                            Me.ForceBoxNine = True
                            Me.CheckBoxArrayList(9).Checked = True
                            If Not Me.BoxNineComponents.Contains(WarrToAddToBoxNine.IssueTo) Then
                                Me.BoxNineComponents.Add(WarrToAddToBoxNine.IssueTo)
                            End If
                        ElseIf WarrToAddToBoxNine.WarrantType = "ItemTwoToMOW" Then
                            'DO NOTHING
                        Else
                            Dim OverrideConflict As Boolean = False
                            If gBoxOneRequired Then
                                If Not IsNothing(VoidedWarrant) Then
                                    If WarrToAddToBoxNine.WarrantNumber = VoidedWarrant.WarrantNumber Then
                                        OverrideConflict = True
                                    End If
                                End If
                            End If
                            If Not OverrideConflict Then
                                WarrantsFoundForBoxNine.Clear()
                                ErrorMessage = "Selection Contains a Warrant Type Not Allowed!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(Me, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                ClearSystemVariables()
                                Me.CloseForm()
                                Exit Sub
                            Else
                                OverrideConflict = False
                            End If
                        End If
                    Next
                End If
                If Me.ForceBoxNine Then
                    If Not Me.BoxNineInit() Then
                        Me.CloseForm()
                    End If
                    WarrantsFoundForBoxNine.Clear()
                    ReInitCheckBoxesAndForm()

                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

    Private Function CheckForBoxOneWarrantInSelectedABSLimits() As Boolean
        Try
            Dim comp As Object
            If CompleteCompSelection Is Nothing Then Exit Function
            For Each comp In CompleteCompSelection
                If comp.Authorities.Count > 0 Then
                    Dim warr As Warrant
                    For Each warr In comp.Authorities
                        If warr.IssueTo = Me.cbIssueTo.Text Then
                            Dim UserMessage As String = "WARRANT NO:" & warr.WarrantNumber.ToString & " FOUND ISSUED TO:" & warr.IssueTo & vbCrLf & vbCrLf
                            UserMessage = UserMessage & "DO YOU WISH TO VOID IT?"
                            Dim UserResponse As Integer = MessageBox.Show(Me, UserMessage, "SYSTEM MESSAGE", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                            If UserResponse = 6 Then
                                gBoxOneRequired = True
                                VoidedWarrant = warr
                                ForceBoxOne = True
                                'Me.cbIssueTo.Text = VoidedWarrant.IssueTo
                                Me.cbIssueTo.Enabled = False
                                Me.CheckBoxArrayList(1).Checked = True
                                Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                                Me.TextBoxArrayList(1).readonly = True
                                Me.btnRepeat.Enabled = True
                                Me.TextBoxArrayList(1).Focus()
                                'Else
                                '    ClearSystemVariables()
                                '    Me.CloseForm()
                                '    Return False
                            End If
                            Return True
                        End If
                    Next
                End If
            Next
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            'ClearSystemVariables()
            'Me.CloseForm()
        End Try
    End Function
#End Region

#Region "FORM BUILD FUNCTIONS"

    Sub ClearFormValues()
        Try



            Dim i As Integer

            Me.cmbBox11To.Items.Clear()
            Me.cmbBox11From.Items.Clear()
            If Not IsNothing(RestrictionList) Then
                RestrictionList.Clear()
            End If

            If Not IsNothing(BoxElevenComponents) Then
                BoxElevenComponents.Clear()
            End If
            If Not IsNothing(BoxNineComponents) Then
                BoxNineComponents.Clear()
            End If
            If Not IsNothing(WarrantsFoundForBoxNine) Then
                WarrantsFoundForBoxNine.Clear()
            End If

            FirstTrackSelection = Nothing
            SecondTrackSelection = Nothing
            DispName = ""
            RailwayName = ""
            DirectionOfTheWarrant = ""
            CurrentSelectedTerritory = Nothing
            VoidedWarrant = Nothing
            ConflictCheckNeeded = False
            IssueToQuery = ""
            DirectionOfWarrant = ""
            ForceBoxOne = False
            ForceBoxTwo = False
            ForceBoxThree = False
            ForceBoxFour = False
            ForceBoxNine = False
            BoxTwoFrom = ""
            BoxTwoTo = ""
            BoxTwoTrack = ""
            BoxThreeFrom = ""
            BoxThreeTo = ""
            BoxThreeTrack = ""
            BoxFourFrom = ""
            BoxFourTo = ""
            BoxFourTrack = ""
            ThisCurrentWarrant = Nothing
            VoidedWarrant = Nothing
            CurrentSelectedTerritory = Nothing
            ItemTwoWarrant = False
            ItemFourWarrant = False
            ItemTwenty = False
            ItemTwentyOne = False
            If Not IsNothing(BoxTwentyOneSwitches) Then
                BoxTwentyOneSwitches.Clear()
            End If

            If Not IsNothing(BoxTwentySwitches) Then
                BoxTwentySwitches.Clear()
            End If

            BoxSevenTrains = ""
            Me.BoxSixteenBulletins = ""
            TerritoryIDAssignment = -1
            FirstTypeOfWarrant = ""
            SecondTypeOfWarrant = ""

            gBoxFourRequired = False
            gBoxTwoRequired = False

            For i = 1 To TextBoxArrayList.Count - 1
                TextBoxArrayList(i).Text = ""
                TextBoxArrayList(i).ForeColor = Color.DarkRed
                TextBoxArrayList(i).Tag = ""
                TextBoxArrayList(i).readonly = False
            Next
            For i = 1 To CheckBoxArrayList.Count - 1
                CheckBoxArrayList(i).Tag = ""
                CheckBoxArrayList(i).ForeColor = Color.Black
                CheckBoxArrayList(i).AccessibleDescription = ""
                CheckBoxArrayList(i).Checked = False
                'CheckBoxArrayList(i).AutoCheck = False
                CheckBoxArrayList(i).CheckState = CheckState.Unchecked
            Next

            For i = 1 To GroupArrayList.Count - 1
                GroupArrayList(i).visible = False
                GroupArrayList(i).enabled = False
            Next

            Me.txtAtLocation.Text = ""
            Me.txtAtLocation.Tag = ""
            Me.txtAtLocation.Visible = False
            Me.txtAtLocation.Enabled = True
            Me.txtAtLocation.ForeColor = Color.DarkRed
            Me.txtAtLocation.ReadOnly = False

            Me.txtCOPIED.Text = ""
            Me.txtCOPIED.Tag = ""
            Me.txtCOPIED.ForeColor = Color.DarkRed

            Me.txtCTO.Text = ""
            Me.txtCTO.Tag = ""
            Me.txtCTO.ForeColor = Color.DarkRed

            Me.txtDate.Text = ""
            Me.txtDate.Tag = ""
            Me.txtDate.ReadOnly = False
            Me.txtDate.ForeColor = Color.DarkRed

            Me.txtDISP.Text = ""
            Me.txtDISP.Tag = ""
            Me.txtDISP.ForeColor = Color.DarkRed

            Me.txtGO.Text = ""
            Me.txtGO.Tag = ""
            Me.txtGO.ForeColor = Color.DarkRed

            Me.txtIssueTo.Text = ""
            Me.txtIssueTo.Tag = ""
            Me.txtIssueTo.ReadOnly = False

            Me.txtIssueTo.ForeColor = Color.DarkRed

            Me.txtOKTIME.Text = ""
            Me.txtOKTIME.Tag = ""
            Me.txtOKTIME.ForeColor = Color.DarkRed

            Me.txtSubdivision.Text = "    "
            Me.txtSubdivision.Tag = ""
            Me.txtSubdivision.ReadOnly = False
            Me.txtSubdivision.ForeColor = Color.DarkRed

            Me.btnOK.Enabled = False
            Me.btnOK.Visible = False
            Me.btnRepeat.Visible = True
            Me.btnRepeat.Enabled = False

            Me.cbAT.Visible = True
            Me.cbAT.Enabled = True
            Me.cbAT.BringToFront()
            Me.cbAT.Items.Clear()

            Me.cbIssueTo.Enabled = True
            Me.cbIssueTo.Visible = True
            Me.cbIssueTo.Text = ""
            Me.cbIssueTo.Items.Clear()

            Me.cbIssueTo.BringToFront()
            Me.gbFooter.Visible = False

            gHistoryMessage = ""
            Me.txtTWnum.Text = "XXXXX"
            Me.txtTWnum.Tag = ""
            Me.txtTWnum.ReadOnly = False
            Me.txtTWnum.ForeColor = Color.DarkRed
            Me.txtTWnum.Enabled = True

            JointAuthRequired = False
            ReDim JointAuthArray(0)


            ClearSystemVariables()

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
            SystemForms.MySystemForms.frmTrackWarrant = Nothing
        End Try

    End Sub

    Private Sub AddTextBox(ByVal IndexNum As Integer, ByVal boxSize As Size, _
                       ByVal boxLocation As Point, ByVal boxName As String, _
                       ByRef objGroupItem As GroupBox, ByVal MultipleLine As Boolean, ByVal TabIndex As Integer)
        Try
            TextBoxItem = New TextBox()
            TextBoxArrayList.Insert(IndexNum, TextBoxItem)
            objGroupItem.Controls.AddRange(New Control() {TextBoxItem})
            TextBoxItem.Font = New Font("Arial", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            TextBoxItem.Location = boxLocation
            TextBoxItem.Name = boxName
            TextBoxItem.Size = boxSize
            TextBoxItem.BackColor = Me.BackColor
            TextBoxItem.ForeColor = Color.DarkRed
            TextBoxItem.TabIndex = 0
            TextBoxItem.Text = "            "
            TextBoxItem.CharacterCasing = CharacterCasing.Upper
            TextBoxItem.BorderStyle = BorderStyle.None
            TextBoxItem.Visible = True
            TextBoxItem.Enabled = False
            'TextBoxItem.Enabled = True
            TextBoxItem.AutoSize = False
            TextBoxItem.TextAlign = HorizontalAlignment.Center
            TextBoxItem.TabIndex = TabIndex
            TextBoxItem.Multiline = MultipleLine
            If MultipleLine Then
                TextBoxItem.Font = New Font("Arial", 8.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            End If

            Select Case IndexNum
                Case 28, 32 'Item 13 & 14 MPH text boxes
                    TextBoxItem.MaxLength = 2
                Case 38
                    TextBoxItem.MaxLength = 50 'bulletins
            End Select
            TextBoxItem.BringToFront()
            AddHandler TextBoxItem.KeyDown, AddressOf TextBoxItem_KeyDown
            AddHandler TextBoxItem.Leave, AddressOf TextBoxItem_Leave

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try

    End Sub

    Private Sub AddLabel(ByVal IndexNum As Integer, ByVal labelSize As Size, ByVal labelLocation As Point, ByVal labelText As String, ByRef objGroupItem As GroupBox)
        Dim ItemLabel As New Label
        Try
            LabelSecondaryArrayList.Insert(IndexNum, ItemLabel)
            objGroupItem.Controls.AddRange(New Control() {ItemLabel})
            ItemLabel.Font = New Font("Arial", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
            ItemLabel.Location = labelLocation
            ItemLabel.Name = "Label1"
            ItemLabel.Size = labelSize
            ItemLabel.TabIndex = 1
            ItemLabel.Text = labelText
            ItemLabel.TextAlign = ContentAlignment.MiddleLeft
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

#End Region

#Region "TRACK WARRANT FORM SETUP"

    Private Function BoxNineInit() As Boolean
        Try
            Dim ErrorMessage As String
            If Me.BoxNineComponents.Count = 0 Then
                ErrorMessage = "Can Not Find Box None Trains...Exiting Warrant!"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Exit Function
            End If

            Dim BoxNineTrains As String = String.Empty
            Dim BoxNineTrain As String = String.Empty
            For Each BoxNineTrain In Me.BoxNineComponents
                BoxNineTrains = BoxNineTrains & BoxNineTrain.ToString & ", "
            Next

            If BoxNineTrains = "" Then
                ErrorMessage = "Can Not Find Box None Trains...Exiting Warrant!"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Exit Function
            End If

            Me.TextBoxArrayList(13).Text = BoxNineTrains
            Me.TextBoxArrayList(13).ReadOnly = True

            If Trim(Me.TextBoxArrayList(13).Text) = "" Then
                ErrorMessage = "Item Nine Verbiage Empty...Exiting Warrant!"
                TrackWarrantEvent(ErrorMessage)
                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ClearSystemVariables()
                Exit Function
            End If
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Function

    Private Function FillBoxElevenComboBoxs(ByVal ComponentSelection As ArrayList, ByVal SearchDirection As String) As Boolean
        Dim comp As Object
        Try
            'Me.cmbBox11From.DroppedDown = True
            'Me.cmbBox11To.DroppedDown = True


            If SearchDirection = "RIGHT" Then
                For Each comp In ComponentSelection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not Me.cmbBox11From.Items.Contains(comp.LeftLimitName) Then
                            Me.cmbBox11From.Items.Add(comp.LeftLimitName)
                        End If
                    End If
                Next
                ComponentSelection.Reverse()
                For Each comp In ComponentSelection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not Me.cmbBox11To.Items.Contains(comp.RightLimitName) Then
                            Me.cmbBox11To.Items.Add(comp.RightLimitName)
                        End If
                    End If
                Next
                ComponentSelection.Reverse()

            Else

                For Each comp In ComponentSelection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not Me.cmbBox11From.Items.Contains(comp.RightLimitName) Then
                            Me.cmbBox11From.Items.Add(comp.RightLimitName)
                        End If
                    End If
                Next
                ComponentSelection.Reverse()
                For Each comp In ComponentSelection
                    If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                        If Not Me.cmbBox11To.Items.Contains(comp.LeftLimitName) Then
                            Me.cmbBox11To.Items.Add(comp.LeftLimitName)
                        End If
                    End If
                Next
                ComponentSelection.Reverse()

            End If

            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try

    End Function

    Private Function SetUpTrackWarrantForm(ByVal FirstWarrantType As String, ByVal SecondWarrantType As String) As Boolean

        Dim ErrorMessage As String
        Try
            'START ADDING THE ATTRIBUTES TO THE FORM
            Me.lblRailroadName.Text = CurrentSelectedTerritory.RailRoadName
            Me.lblDispInt.Text = gDispatcherFormTitle
            Me.txtDISP.Left = gTextBoxDispLocation.X
            Me.RailwayName = CurrentSelectedTerritory.RailRoadName
            Me.DispName = gCurrentUser.Initials
            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    Me.lblTimeZone.Text = "EASTERN TIME ZONE"
                Case "MOUNTAIN"
                    Me.lblTimeZone.Text = "MOUNTAIN TIME ZONE"
                Case "PACIFIC"
                    Me.lblTimeZone.Text = "PACIFIC TIME ZONE"
                Case Else
                    Me.lblTimeZone.Text = "CENTRAL TIME ZONE"
            End Select

            Me.lblDispName.Text = "DISP: " & gCurrentUser.FirstName & " " & gCurrentUser.LastName
            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(1 + gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(1 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")
                Case "PACIFIC"
                    Dim TimeDiff As Integer = (-2 + gThisWorkstationTimeFactor)
                    Me.lblCreateTime.Text = Format(Now.AddHours(TimeDiff), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(-2 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")

                Case "MOUNTAIN"
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(-1 + gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(-1 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")

                Case Else
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.Date, "dddd, MMM d yyyy")
            End Select
            Dim thisSubdivision As Subdivision = FindSubDivision(CurrentSelectedTerritory.Subdivision)
            Me.txtTWnum.Text = CurrentSelectedTerritory.GetNextWarrantNumber(TrackWarrants)
            Me.txtTWnum.ReadOnly = True
            If optEnableWatcoSpecificOptions Then
                If Me.thisSubdivision IsNot Nothing Then
                Else
                    If gBeginTrackSelect.mySubdivision IsNot Nothing Then
                        Me.thisSubdivision = gBeginTrackSelect.mySubdivision
                    End If
                End If
                Me.txtSubdivision.Text = Me.thisSubdivision.Name & " SUBDIVISION"
                thisSubdivision = Me.thisSubdivision
            Else
                Me.txtSubdivision.Text = CurrentSelectedTerritory.Subdivision & " SUBDIVISION"
            End If

            Me.txtSubdivision.ReadOnly = True
            'Me.txtDate.Text = Format(Now.Date, "MMM. d, yyyy")
            Me.txtDate.Text = Format(CDate(Me.lblCreateDate.Text), "MMM. d, yyyy")
            Me.txtDate.ReadOnly = True
            Me.cbIssueTo.Text = "    "
            Me.cbAT.Text = BoxFrom
            Me.txtDISP.Text = gCurrentUser.Initials
            Me.txtCOPIED.Text = gPersonCopying
            Me.txtGO.Text = GetGeneralOrder(CurrentSelectedTerritory.Subdivision)
            Me.txtCTO.Text = GetCTO(CurrentSelectedTerritory.Subdivision)
            gSubdivisionDirection = FindSubDivisionDirection(CurrentSelectedTerritory.Subdivision)


            If optEnableWatcoSpecificOptions Then
                gSubdivisionDirection = FindSubDivisionDirection(thisSubdivision.Name)
                If Not IsNothing(thisSubdivision) Then
                    If thisSubdivision.BranchType Then
                        Me.txtSubdivision.Text = thisSubdivision.Name & " BRANCH"
                    End If
                End If
                Me.txtGO.Text = GetGeneralOrder(thisSubdivision.Name)
                Me.txtCTO.Text = GetCTO(thisSubdivision.Name)
            Else
                If Not IsNothing(thisSubdivision) Then
                    If thisSubdivision.BranchType Then
                        Me.txtSubdivision.Text = CurrentSelectedTerritory.Subdivision & " BRANCH"
                    End If
                End If
            End If

          
            'Load the "ISSUE TO" type this warrant will be issued too (Train (EAST, WEST, WORK) or Man/Equipment)
            Select Case gSearchDirection
                Case "RIGHT"
                    If CurrentSelectedTerritory IsNot Nothing AndAlso optEnableWatcoSpecificOptions Then
                        If CurrentSelectedTerritory.FlipDirection Then
                            If gSubdivisionDirection Then
                                DirectionOfWarrant = "SOUTH"
                            Else
                                DirectionOfWarrant = "WEST"
                            End If
                        Else
                            If gSubdivisionDirection Then
                                DirectionOfWarrant = "NORTH"
                            Else
                                DirectionOfWarrant = "EAST"
                            End If
                        End If
                    Else
                        If gSubdivisionDirection Then
                            DirectionOfWarrant = "NORTH"
                        Else
                            DirectionOfWarrant = "EAST"
                        End If
                    End If
                    'If gSubdivisionDirection Then
                    '    DirectionOfWarrant = "NORTH"
                    'Else
                    '    DirectionOfWarrant = "EAST"
                    'End If
                Case "LEFT"
                    'If gSubdivisionDirection Then
                    '    DirectionOfWarrant = "SOUTH"
                    'Else
                    '    DirectionOfWarrant = "WEST"
                    'End If
                    If CurrentSelectedTerritory IsNot Nothing AndAlso optEnableWatcoSpecificOptions Then
                        If CurrentSelectedTerritory.FlipDirection Then
                            If gSubdivisionDirection Then
                                DirectionOfWarrant = "NORTH"
                            Else
                                DirectionOfWarrant = "EAST"
                            End If
                        Else
                            If gSubdivisionDirection Then
                                DirectionOfWarrant = "SOUTH"
                            Else
                                DirectionOfWarrant = "WEST"
                            End If
                        End If

                    Else
                        If gSubdivisionDirection Then
                            DirectionOfWarrant = "SOUTH"
                        Else
                            DirectionOfWarrant = "WEST"
                        End If
                    End If
                Case Else
                    DirectionOfWarrant = "WORK"
            End Select


            If ConflictCheckNeeded Then
                Dim StringReplacement As String = ""
                Dim NewIssueTo As String = ""
                Dim strPosition, UserMessage As Integer

                'Load the "ISSUE TO" type this warrant will be issued too (Train (EAST, WEST, WORK) or Man/Equipment)
                Select Case FirstTypeOfWarrant
                    Case "ItemTwoToTrain"
                        gWarrantType = "ItemTwoToTrain"
                        If ForceBoxOne And Not gBoxOneRequired Then
                            VoidedWarrant = gBeginTrackSelect.Authorities(0)
                        End If
                        If ForceBoxOne Or gBoxOneRequired Then
                            gBoxOneRequired = True
                            Me.cbIssueTo.Text = VoidedWarrant.IssueTo
                            Me.cbIssueTo.Enabled = False
                            Me.CheckBoxArrayList(1).Checked = True
                            Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                            Me.TextBoxArrayList(1).readonly = True
                            Me.btnRepeat.Enabled = True
                            Me.TextBoxArrayList(1).Focus()

                        Else
                            IssueToQuery = "SELECT DISTINCT Name, Direction FROM " & gtblLocoIDs & " WHERE Direction = " & "'" & DirectionOfWarrant & "'" & " ORDER BY NAME;"
                            LoadEngineIDFromSelectedLimits(CompleteCompSelection)
                            LoadDatabaseField(Me.cbIssueTo, IssueToQuery, True)
                        End If
                    Case "ItemTwoToMOW"
                        gWarrantType = "ItemTwoToMOW"
                        VoidedWarrant = gBeginTrackSelect.Authorities(0)
                        gBoxOneRequired = True
                        Me.cbIssueTo.Text = gPersonCopying & " " & DirectionOfWarrant
                        Me.cbIssueTo.Enabled = False
                        Me.CheckBoxArrayList(1).Checked = True
                        Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                        Me.TextBoxArrayList(1).readonly = True
                        Me.btnRepeat.Enabled = True
                        Me.TextBoxArrayList(1).Focus()
                    Case "ItemFourToTrain"
                        gWarrantType = "ItemFourToTrain"
                        If gItemFourAddWorkToIssue Then
                            IssueToQuery = "SELECT DISTINCT Name, Direction FROM " & gtblLocoIDs & " WHERE Direction = 'WORK' ORDER BY NAME;"
                        Else
                            IssueToQuery = "SELECT DISTINCT Name FROM " & gtblLocoIDs & " ORDER BY NAME;"
                        End If

                        LoadEngineIDFromSelectedLimits(CompleteCompSelection)
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, True)
                    Case "ItemFourToMOW"
                        gWarrantType = "ItemFourToMOW"
                        Me.cbIssueTo.Text = gPersonCopying
                        'If gBoxOneRequired Then
                        '    JointAuthorityCheck()
                        'End If
                        ''LoadDatabaseField(frmWarrant!cmbIssueTo, gTWSupplementalDB, "Foremen", "FIRSTNAME", "", "NONE", "")
                    Case "ItemFourToSameMOW"
                        gWarrantType = "ItemFourToMOW"
                        gBoxOneRequired = True
                        Me.cbIssueTo.Enabled = False
                        Me.CheckBoxArrayList(1).Checked = True
                        Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                        Me.TextBoxArrayList(1).readonly = True
                        Me.btnRepeat.Enabled = True
                        Me.TextBoxArrayList(1).Focus()
                        FirstTypeOfWarrant = "ItemFourToMOW"
                        gWarrantType = "ItemFourToMOW"
                        Me.cbIssueTo.Text = VoidedWarrant.IssueTo
                        strPosition = InStr(VoidedWarrant.IssueTo, "EAST")
                        If strPosition = 0 Then
                            strPosition = InStr(VoidedWarrant.IssueTo, "WEST")
                            If strPosition = 0 Then
                                strPosition = InStr(VoidedWarrant.IssueTo, "NORTH")
                                If strPosition = 0 Then
                                    strPosition = InStr(VoidedWarrant.IssueTo, "SOUTH")
                                End If
                            End If
                        End If


                        If strPosition > 0 Then
                            NewIssueTo = Trim(Mid(VoidedWarrant.IssueTo, 1, strPosition - 1) & StringReplacement)
                        Else
                            NewIssueTo = VoidedWarrant.IssueTo & " " & StringReplacement
                        End If

                        'added this line of code because we skip the copy to dialog box on ItemFourToSameMOW
                        Me.txtCOPIED.Text = NewIssueTo

                        ''PositionWindowOverMain()
                        UserMessage = MessageBox.Show(Me, "IS THIS TO THE CORRECT MOW ID AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "MOW/DIRECTION: " & NewIssueTo, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If UserMessage = 7 Then
                            TrackWarrantEvent("SELECTED - NO TO CORRECT MOW ID AND THE CORRECT DIRECTION! - EXIT SUB")
                            'ClearSystemVariables()
                            Exit Function
                        Else
                            TrackWarrantEvent("SELECTED - YES TO CORRECT MOW ID AND THE CORRECT DIRECTION! - EXIT SUB")
                        End If
                        Me.cbIssueTo.Text = NewIssueTo
                        Me.cbIssueTo.Enabled = False
                    Case "ItemFourToSameTrain"
                        gWarrantType = "ItemFourToMOW"
                        '05-03-2003 remove this line of code and made the reference to VoidedWarrant in the other area of code
                        'VoidedWarrant = TrackComponent(gBeginTrackSelection).Authorities(0)
                        ''** REMOVED THE BOX 11 REQUIRMENT BECAUSE TRAINS CAN MOVE AT TRACK SPEED ON A SOLE ITEM 4
                        ''gBox11Required = True
                        gBoxOneRequired = True
                        ''**frmWarrant!cmbIssueTo.Text = TrackWarrant(FindWarrantIndex(Track(BeginTrackSelection).ActiveWarrants(1))).IssueTo
                        Me.cbIssueTo.Enabled = False
                        Me.CheckBoxArrayList(1).Checked = True
                        Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                        Me.TextBoxArrayList(1).readonly = True
                        Me.btnRepeat.Enabled = True
                        Me.TextBoxArrayList(1).Focus()
                        FirstTypeOfWarrant = "ItemFourToTrain"
                        gWarrantType = "ItemFourToTrain"
                        ''**REMOVED THE LOCK ON THE AT LOCATION BOX PER REQUEST
                        ''**frmWarrant!cmbAtLoc.Locked = True

                        ''**REMOVED THE WORK VERIBAGE FROM THE ENGINE ID
                        ''**StringReplacement = "WORK"
                        ''**
                        strPosition = InStr(VoidedWarrant.IssueTo, "EAST")
                        If strPosition = 0 Then
                            strPosition = InStr(VoidedWarrant.IssueTo, "WEST")
                            If strPosition = 0 Then
                                strPosition = InStr(VoidedWarrant.IssueTo, "NORTH")
                                If strPosition = 0 Then
                                    strPosition = InStr(VoidedWarrant.IssueTo, "SOUTH")
                                End If
                            End If
                        End If


                        If strPosition > 0 Then
                            NewIssueTo = Trim(Mid(VoidedWarrant.IssueTo, 1, strPosition - 1) & gItemFourReplacementString)
                        Else
                            NewIssueTo = VoidedWarrant.IssueTo & " " & gItemFourReplacementString
                        End If
                        ''PositionWindowOverMain()
                        UserMessage = MessageBox.Show(Me, "IS THIS TO THE CORRECT ENGINE AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "ENGINE/DIRECTION: " & NewIssueTo, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If UserMessage = 7 Then
                            TrackWarrantEvent("SELECTED - NO TO CORRECT ENGINE AND THE CORRECT DIRECTION! - EXIT SUB")
                            'ClearSystemVariables()
                            Exit Function
                        Else
                            TrackWarrantEvent("SELECTED - YES TO CORRECT ENGINE AND THE CORRECT DIRECTION! - EXIT SUB")
                        End If
                        Me.cbIssueTo.Text = NewIssueTo
                        Me.cbIssueTo.Enabled = False
                    Case "ItemTwoToSameMOW"
                        VoidedWarrant = gBeginTrackSelect.Authorities(0)
                        gBoxOneRequired = True
                        Me.cbIssueTo.Enabled = False
                        Me.CheckBoxArrayList(1).Checked = True
                        Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                        Me.TextBoxArrayList(1).readonly = True
                        Me.btnRepeat.Enabled = True
                        Me.TextBoxArrayList(1).Focus()
                        FirstTypeOfWarrant = "ItemTwoToMOW"
                        gWarrantType = "ItemTwoToMOW"
                        'added to put direction to MOW
                        If gSearchDirection = "RIGHT" Then
                            If Not gSubdivisionDirection Then
                                StringReplacement = "EAST"
                            Else
                                StringReplacement = "NORTH"
                            End If
                        ElseIf gSearchDirection = "LEFT" Then
                            If Not gSubdivisionDirection Then
                                StringReplacement = "WEST"
                            Else
                                StringReplacement = "SOUTH"
                            End If
                        End If
                        strPosition = InStr(VoidedWarrant.IssueTo, "EAST")
                        If strPosition = 0 Then
                            strPosition = InStr(VoidedWarrant.IssueTo, "WEST")
                            If strPosition = 0 Then
                                strPosition = InStr(VoidedWarrant.IssueTo, "NORTH")
                                If strPosition = 0 Then
                                    strPosition = InStr(VoidedWarrant.IssueTo, "SOUTH")
                                End If
                            End If
                        End If

                        If strPosition > 0 Then
                            NewIssueTo = Trim(Mid(VoidedWarrant.IssueTo, 1, strPosition - 1) & StringReplacement)

                            'added this line of code because we skip the copy to dialog box on ItemTwoToSameMOW
                            Me.txtCOPIED.Text = Trim(Mid(VoidedWarrant.IssueTo, 1, strPosition - 1))

                        Else
                            NewIssueTo = VoidedWarrant.IssueTo & " " & StringReplacement
                            Me.txtCOPIED.Text = NewIssueTo
                        End If

                        If gItemSixRequired Or Me.ForceBoxNine Then
                            Me.CheckBoxArrayList(9).Checked = True
                            If Me.BoxNineComponents.Count = 0 Then
                                ErrorMessage = "Can Not Find Box None Trains...Exiting Warrant!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Function
                            End If

                            Dim BoxNineTrains As String = String.Empty
                            Dim BoxNineTrain As String = String.Empty
                            For Each BoxNineTrain In Me.BoxNineComponents
                                BoxNineTrains = BoxNineTrains & BoxNineTrain.ToString & ", "
                            Next

                            If BoxNineTrains = "" Then
                                ErrorMessage = "Can Not Find Box None Trains...Exiting Warrant!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Function
                            End If

                            Me.TextBoxArrayList(13).Text = BoxNineTrains
                            Me.TextBoxArrayList(13).ReadOnly = True

                            If Trim(Me.TextBoxArrayList(13).Text) = "" Then
                                ErrorMessage = "Item Nine Verbiage Empty...Exiting Warrant!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                Exit Function
                            End If
                        End If
                        ''PositionWindowOverMain()
                        UserMessage = MessageBox.Show(Me, "IS THIS TO THE CORRECT MOW ID AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "MOW/DIRECTION: " & NewIssueTo, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        'UserMessage = MsgBox("IS THIS TO THE CORRECT ENGINE AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "ENGINE/DIRECTION: " & NewIssueTo, vbYesNo)
                        If UserMessage = 7 Then
                            TrackWarrantEvent("SELECTED - NO TO CORRECT MOW AND THE CORRECT DIRECTION! - EXIT SUB")
                            'ClearSystemVariables()
                            Exit Function
                        Else
                            TrackWarrantEvent("SELECTED - YES TO CORRECT MOW AND THE CORRECT DIRECTION! - EXIT SUB")
                        End If
                        Me.cbIssueTo.Text = NewIssueTo
                        ''Me.cbIssueTo.Text = VoidedWarrant.IssueTo
                    Case "ItemTwoToSameTrain"

                        If gItemTwoSameTrainWithRadioBlocking Then
                            Dim auth As Warrant
                            For Each auth In gBeginTrackSelect.Authorities
                                If auth.RadioBlockingUsed Then
                                    VoidedWarrant = auth
                                    Exit For
                                End If
                            Next
                            If IsNothing(VoidedWarrant) Then
                                VoidedWarrant = gBeginTrackSelect.Authorities(0)
                            End If
                        Else
                            VoidedWarrant = gBeginTrackSelect.Authorities(0)
                        End If

                        gBoxOneRequired = True
                        Me.cbIssueTo.Enabled = False
                        Me.CheckBoxArrayList(1).Checked = True
                        Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                        Me.TextBoxArrayList(1).readonly = True
                        Me.btnRepeat.Enabled = True
                        Me.TextBoxArrayList(1).Focus()
                        FirstTypeOfWarrant = "ItemTwoToTrain"
                        gWarrantType = "ItemTwoToTrain"
                        ''**REMOVED THE LOCK ON THE AT LOCATION BOX PER REQUEST
                        ''**frmWarrant!cmbAtLoc.Locked = True

                        If gSearchDirection = "RIGHT" Then
                            If Not gSubdivisionDirection Then
                                StringReplacement = "EAST"
                            Else
                                StringReplacement = "NORTH"
                            End If
                        ElseIf gSearchDirection = "LEFT" Then
                            If Not gSubdivisionDirection Then
                                StringReplacement = "WEST"
                            Else
                                StringReplacement = "SOUTH"
                            End If
                        End If
                        If InStr(VoidedWarrant.IssueTo, StringReplacement) = 0 Then
                            strPosition = InStr(VoidedWarrant.IssueTo, "WORK")
                            If strPosition > 0 Then
                                NewIssueTo = Mid(VoidedWarrant.IssueTo, 1, strPosition - 1) & StringReplacement
                            Else
                                NewIssueTo = VoidedWarrant.IssueTo & " " & StringReplacement
                            End If
                        Else
                            NewIssueTo = VoidedWarrant.IssueTo
                        End If
                        ''PositionWindowOverMain()
                        UserMessage = MessageBox.Show(Me, "IS THIS TO THE CORRECT ENGINE AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "ENGINE/DIRECTION: " & NewIssueTo, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        'UserMessage = MsgBox("IS THIS TO THE CORRECT ENGINE AND THE CORRECT DIRECTION?" & vbCrLf & vbCrLf & "ENGINE/DIRECTION: " & NewIssueTo, vbYesNo)
                        If UserMessage = 7 Then
                            TrackWarrantEvent("SELECTED - NO TO CORRECT ENGINE AND THE CORRECT DIRECTION! - EXIT SUB")
                            'ClearSystemVariables()
                            Exit Function
                        Else
                            TrackWarrantEvent("SELECTED - YES TO CORRECT ENGINE AND THE CORRECT DIRECTION! - EXIT SUB")
                        End If
                        Me.cbIssueTo.Text = NewIssueTo


                    Case "ItemTwoNineToMOW"

                        FirstTypeOfWarrant = "ItemTwoToMOW"
                        gWarrantType = "ItemTwoToMOW"
                        gItemSixRequired = True
                        Me.cbIssueTo.Text = gPersonCopying
                        Me.cbIssueTo.Focus()
                        Me.CheckBoxArrayList(9).Checked = True
                        Me.BoxNineInit()

                        If gBoxOneRequired Or Me.ForceBoxOne Then
                            'VoidedWarrant = TrackComponent(gBeginTrackSelection).Authorities(0)
                            'gBoxOneRequired = True
                            If VoidedWarrant Is Nothing Then
                                ErrorMessage = "Could Not Get The Voided Warrant!"
                                TrackWarrantEvent(ErrorMessage)
                                MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                'TrackWarrantEvent("Box Nine Verbiage Empty(2)! - EXIT SUB")
                                'MsgBox("Item Nine Verbiage Empty...Exiting Warrant!", vbCritical)
                                'ClearSystemVariables()
                                Exit Function
                            End If
                            Me.cbIssueTo.Text = VoidedWarrant.IssueTo
                            Me.cbIssueTo.Enabled = False
                            Me.CheckBoxArrayList(1).Checked = True
                            Me.TextBoxArrayList(1).Text = VoidedWarrant.WarrantNumber
                            Me.TextBoxArrayList(1).readonly = True
                            Me.btnRepeat.Enabled = True
                            Me.TextBoxArrayList(1).Focus()
                            FirstTypeOfWarrant = "ItemTwoToMOW"
                            gWarrantType = "ItemTwoToMOW"

                        End If


                    Case "ItemFourNineToMOW"
                        Me.cbIssueTo.Text = gPersonCopying
                        Me.cbIssueTo.Focus()
                        FirstTypeOfWarrant = "ItemFourToMOW"
                        gWarrantType = "ItemFourToMOW"
                        gItemSixRequired = True
                        Me.CheckBoxArrayList(9).Checked = True
                        If FirstWarrantFound Is Nothing Then
                            ErrorMessage = "Can Not Get Next Track Warrant Index...Exiting Warrant!"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            'TrackWarrantEvent("Warrant Index was Zero! - EXIT SUB")
                            'MsgBox("Can Not Get Next Track Warrant Index...Exiting Warrant!", vbCritical)
                            'ClearSystemVariables()
                            Exit Function
                        End If

                        Me.TextBoxArrayList(13).Text = FirstWarrantFound.IssueTo
                        Me.TextBoxArrayList(13).ReadOnly = True
                        If Trim(Me.TextBoxArrayList(13).Text) = "" Then
                            ErrorMessage = "Item Nine Verbiage Empty...Exiting Warrant!"
                            TrackWarrantEvent(ErrorMessage)
                            MessageBox.Show(MainForm, ErrorMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            'TrackWarrantEvent("Box Nine Verbiage Empty(2)! - EXIT SUB")
                            'MsgBox("Item Nine Verbiage Empty...Exiting Warrant!", vbCritical)
                            'ClearSystemVariables()
                            Exit Function
                        End If
                    Case "ItemTwoSevenToTrain"
                        IssueToQuery = "SELECT DISTINCT Name, Direction FROM " & gtblLocoIDs & " WHERE Direction = " & "'" & DirectionOfWarrant & "'" & " ORDER BY NAME;"
                        LoadEngineIDFromSelectedLimits(CompleteCompSelection)
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, True)
                        gItemFiveRequired = True
                        FirstTypeOfWarrant = "ItemTwoToTrain"
                        gWarrantType = "ItemTwoToTrain"
                        Me.CheckBoxArrayList(7).Checked = True
                        Me.TextBoxArrayList(11).Text = BoxSevenTrains
                        Me.TextBoxArrayList(11).readonly = True
                        Me.TextBoxArrayList(12).Text = BoxFrom
                        Me.TextBoxArrayList(12).readonly = True

                    Case "ItemTwoSevenToMOW"
                        Me.cbIssueTo.Text = gPersonCopying
                        Me.cbIssueTo.Focus()
                        gItemFiveRequired = True
                        FirstTypeOfWarrant = "ItemTwoToMOW"
                        gWarrantType = "ItemTwoToMOW"
                        Me.CheckBoxArrayList(7).Checked = True
                        Me.TextBoxArrayList(11).Text = BoxSevenTrains
                        Me.TextBoxArrayList(11).readonly = True
                        Me.TextBoxArrayList(12).Text = BoxFrom
                        Me.TextBoxArrayList(12).readonly = True

                End Select
            Else

                'Load the "ISSUE TO" type this warrant will be issued too (Train (EAST, WEST, WORK) or Man/Equipment)
                Select Case FirstTypeOfWarrant
                    Case "ItemTwoToTrain"
                        gWarrantType = "ItemTwoToTrain"
                        IssueToQuery = "SELECT DISTINCT Name, Direction FROM " & gtblLocoIDs & " WHERE Direction = " & "'" & DirectionOfWarrant & "'" & " ORDER BY NAME;"
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, True)
                    Case "ItemFourToTrain"
                        gWarrantType = "ItemFourToTrain"
                        If gItemFourAddWorkToIssue Then
                            IssueToQuery = "SELECT DISTINCT Name, Direction FROM " & gtblLocoIDs & " WHERE Direction = 'WORK' ORDER BY NAME;"
                        Else
                            IssueToQuery = "SELECT DISTINCT Name FROM " & gtblLocoIDs & " ORDER BY NAME;"
                        End If
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, True)
                    Case "ItemFourToMOW"
                        gWarrantType = "ItemFourToMOW"
                        Me.cbIssueTo.Text = gPersonCopying
                        IssueToQuery = "SELECT * FROM " & gtblForemen
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, False)
                    Case "ItemTwoToMOW"
                        gWarrantType = "ItemTwoToMOW"
                        Me.cbIssueTo.Text = gPersonCopying & " " & DirectionOfWarrant
                        IssueToQuery = "SELECT * FROM " & gtblForemen
                        LoadDatabaseField(Me.cbIssueTo, IssueToQuery, False)
                End Select
            End If


            ''Set the warrant type boolean variable to draw the proper check item boxes in the init process below
            Select Case gWarrantType
                'Select Case FirstWarrantType
                Case "ItemTwoToTrain", "ItemTwoToMOW"
                    gBoxTwoRequired = True
                    gBoxFourRequired = False
                    ItemFourWarrant = False
                    ItemTwoWarrant = True
                    If ForceBoxThree Then gBoxThreeRequired = True
                Case "ItemFourToTrain", "ItemFourToMOW"
                    gBoxTwoRequired = False
                    gBoxFourRequired = True
                    ItemFourWarrant = True
                    ItemTwoWarrant = False
                Case Else
                    gBoxTwoRequired = True
            End Select


            Select Case SecondWarrantType
                Case "ItemTwoToTrain", "ItemTwoToMOW"
                    gBoxTwoRequired = True
                    ItemTwoWarrant = True
                Case "ItemFourToTrain", "ItemFourToMOW"
                    gBoxFourRequired = True
                    ItemFourWarrant = True
                Case Else
                    gBoxTwoRequired = True
            End Select


            Return True

        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try

    End Function

    Private Sub LoadEngineIDFromSelectedLimits(ByVal SelectionCollection As ArrayList)
        Try
            Dim comp As Object
            Me.cbIssueTo.Items.Clear()
            For Each comp In SelectionCollection
                If comp.GetType.ToString = "TMDS.TrackComponents.Track" Then
                    If comp.Authorities.Count > 0 Then
                        Dim warr As Warrant
                        For Each warr In comp.Authorities
                            If Not Me.cbIssueTo.Items.Contains(warr.IssueTo) Then
                                Me.cbIssueTo.Items.Add(warr.IssueTo)
                            End If
                        Next
                    End If
                End If
            Next

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub ReInitCheckBoxesAndForm()


        ''**********************************************************************************************************************
        ''*** Begin initing The check boxes and placing them according to the check items

        Dim BoxIndex As Integer

        CurrentTopLocation = Me.gbIssue.Top + Me.gbIssue.Height - 2
        CurrentLeftLocation = 5
        CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)

        For BoxIndex = 1 To 22

            GroupArrayList(BoxIndex).Tag = "CheckBox" & BoxIndex
            GroupArrayList(BoxIndex).Location = CurrentPoint


            Select Case BoxIndex

                Case 1
                    GroupArrayList(BoxIndex).Visible = True
                    GroupArrayList(BoxIndex).enabled = True
                    TextBoxArrayList(1).enabled = False
                    TextBoxArrayList(1).ReadOnly = True
                    If gBoxOneRequired Then
                        TextBoxArrayList(1).Enabled = True
                    End If
                Case 4
                    If ItemFourWarrant Then

                        GroupArrayList(BoxIndex).Enabled = True
                        GroupArrayList(BoxIndex).visible = True
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        GroupArrayList(BoxIndex).Enabled = True
                        TextBoxArrayList(8).text = BoxFourFrom
                        TextBoxArrayList(9).Text = BoxFourTo
                        TextBoxArrayList(10).Text = BoxFourTrack
                        TextBoxArrayList(8).Enabled = True
                        TextBoxArrayList(9).Enabled = True
                        TextBoxArrayList(10).Enabled = True
                    Else
                        GroupArrayList(BoxIndex).Enabled = False
                        GroupArrayList(BoxIndex).visible = False
                    End If

                Case 2
                    If ItemTwoWarrant Then

                        GroupArrayList(BoxIndex).Enabled = True
                        GroupArrayList(BoxIndex).visible = True
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        GroupArrayList(BoxIndex).Enabled = True
                        TextBoxArrayList(2).text = BoxTwoFrom
                        TextBoxArrayList(3).Text = BoxTwoTo
                        TextBoxArrayList(4).Text = BoxTwoTrack
                        TextBoxArrayList(2).Enabled = True
                        TextBoxArrayList(3).Enabled = True
                        TextBoxArrayList(4).Enabled = True
                    Else
                        GroupArrayList(BoxIndex).Visible = False
                        GroupArrayList(BoxIndex).Enabled = False
                    End If

                Case 3
                    If ForceBoxThree Then

                        GroupArrayList(BoxIndex).Enabled = True
                        GroupArrayList(BoxIndex).visible = True
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        GroupArrayList(BoxIndex).Enabled = True
                        TextBoxArrayList(5).text = BoxThreeFrom
                        TextBoxArrayList(6).Text = BoxThreeTo
                        TextBoxArrayList(7).Text = BoxThreeTrack
                        TextBoxArrayList(5).Enabled = True
                        TextBoxArrayList(6).Enabled = True
                        TextBoxArrayList(7).Enabled = True
                    Else
                        GroupArrayList(BoxIndex).Visible = False
                        GroupArrayList(BoxIndex).Enabled = False
                    End If

                Case 10  ',8
                    If ItemFourWarrant Then
                        If BoxIndex = 8 Or BoxIndex = 10 Then
                            GroupArrayList(BoxIndex).Visible = False
                            GroupArrayList(BoxIndex).enabled = False
                        End If
                    Else
                        If gItem10Required Or gBoxTenRequired Then
                            CheckBoxArrayList(BoxIndex).ENABLED = True
                            CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                            CheckBoxArrayList(BoxIndex).Checked = True
                            CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        End If
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                    End If
                Case 7, 9, 13, 14, 15, 16, 22
                    If BoxIndex = 16 Then
                        If optForceBox16 Then
                            CheckBoxArrayList(BoxIndex).checked = True
                        End If
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                        TextBoxArrayList(38).TextAlign = HorizontalAlignment.Left
                    ElseIf BoxIndex = 22 Then
                        CheckBoxArrayList(BoxIndex).Visible = False
                        CheckBoxArrayList(BoxIndex).enabled = False
                        CheckBoxArrayList(BoxIndex).checked = False
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                        TextBoxArrayList(43).TextAlign = HorizontalAlignment.Left
                        TextBoxArrayList(42).Enabled = True
                        TextBoxArrayList(43).Enabled = True
                        ''** Added this to disable the Item 8 on track warrants per request
                        'ElseIf BoxIndex = 8 Then
                        '    GroupArrayList(BoxIndex).Visible = False
                        '    GroupArrayList(BoxIndex).enabled = False
                    ElseIf BoxIndex = 7 Then
                        If gItemFiveRequired Then
                            GroupArrayList(BoxIndex).Visible = True
                            GroupArrayList(BoxIndex).enabled = True
                        Else
                            GroupArrayList(BoxIndex).Visible = False
                            GroupArrayList(BoxIndex).enabled = False
                        End If
                    ElseIf BoxIndex = 9 Then
                        If gItemSixRequired Or gItemTwoSameTrainWithRadioBlocking Or gItemTwoToTrainWithRadioBlocking Then
                            GroupArrayList(BoxIndex).Visible = True
                            GroupArrayList(BoxIndex).enabled = True
                        Else
                            GroupArrayList(BoxIndex).Visible = False
                            GroupArrayList(BoxIndex).enabled = False
                        End If


                    ElseIf BoxIndex = 15 Then
                        If optCrossingNotificationOptionOne Then
                            If gBox15Required Then
                                GroupArrayList(BoxIndex).Visible = True
                                GroupArrayList(BoxIndex).enabled = True
                                CheckBoxArrayList(BoxIndex).Text = "CROSSING WARNING SYSTEM AT:"
                                Dim CrossingWarning As String = String.Empty
                                Select Case gCrossingWarningIndex
                                    Case 5
                                        CrossingWarning = "Are disabled, stop and protect movement over crossing even if devices are seen to be working."
                                    Case 6
                                        CrossingWarning = "Are malfunctioning, protect per GCOR rule 6.32.2. "
                                    Case 7
                                        CrossingWarning = "Has an activation failure. Stop and protect movement over crossing even if devices are seen to be working."
                                    Case 8
                                        CrossingWarning = "Crossbucks damaged or missing protect per GCOR rule 6.32.2."
                                    Case 9
                                        CrossingWarning = "Damaged or Malfunctioning pedestrian crossing warning protect per GCOR rule 6.32.2."
                                End Select
                                LabelSecondaryArrayList(7).Text = CrossingWarning.ToUpper
                                LabelSecondaryArrayList(6).Text = "MP__________,  _________________________________________ (TOWN @ CROSSING NAME)"
                                'TextBoxArrayList(36).text = gCrossingWarningIndex
                                'TextBoxArrayList(37).Text = gCrossingWarningIndex
                            Else
                                GroupArrayList(BoxIndex).Visible = False
                                GroupArrayList(BoxIndex).enabled = False
                            End If
                        Else
                            GroupArrayList(BoxIndex).Visible = True
                            GroupArrayList(BoxIndex).enabled = True
                        End If
                    Else
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                    End If
                Case 11
                    If gJointAuthRequired Or gBox8Required Then
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        Me.CheckBoxElevenChange(Me.CompleteCompSelection, Me.DirectionOfTheWarrant)
                    Else
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).enabled = True
                    End If
                Case 12
                    If ItemFourWarrant And JointAuthRequired Then
                        GroupArrayList(BoxIndex).Visible = True
                        GroupArrayList(BoxIndex).Enabled = True
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                    Else
                        GroupArrayList(BoxIndex).Visible = False
                        GroupArrayList(BoxIndex).Enabled = False
                    End If
                Case 17
                    GroupArrayList(BoxIndex).Visible = True
                    GroupArrayList(BoxIndex).enabled = True
                    TextBoxArrayList(39).TextAlign = HorizontalAlignment.Left
                Case 20
                    If ItemTwenty Then
                        GroupArrayList(BoxIndex).Enabled = True
                        GroupArrayList(BoxIndex).visible = True
                        GroupArrayList(BoxIndex).TabIndex = 20
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        GroupArrayList(BoxIndex).Enabled = True
                        Dim BoxTwentySwitch As String
                        TextBoxArrayList(40).text = ""
                        Dim Count As Integer
                        For Each BoxTwentySwitch In BoxTwentySwitches
                            If Count = 0 Then
                                TextBoxArrayList(40).text = TextBoxArrayList(40).text & BoxTwentySwitch
                            Else
                                TextBoxArrayList(40).text = TextBoxArrayList(40).text & ",    " & BoxTwentySwitch
                            End If
                            Count = Count + 1
                        Next
                        TextBoxArrayList(40).TextAlign = HorizontalAlignment.Left
                        TextBoxArrayList(40).readonly = True
                        TextBoxArrayList(40).Enabled = True
                    End If
                Case 21
                    If ItemTwentyOne Then
                        GroupArrayList(BoxIndex).Enabled = True
                        GroupArrayList(BoxIndex).visible = True
                        GroupArrayList(BoxIndex).TabIndex = 21
                        CheckBoxArrayList(BoxIndex).ENABLED = True
                        CheckBoxArrayList(BoxIndex).Tag = "VALIDATE"
                        CheckBoxArrayList(BoxIndex).Checked = True
                        CheckBoxArrayList(BoxIndex).AccessibleDescription = "CHECKED"
                        GroupArrayList(BoxIndex).Enabled = True
                        TextBoxArrayList(41).TextAlign = HorizontalAlignment.Left

                        Dim BoxTwentyOneSwitch As String
                        TextBoxArrayList(41).text = ""

                        Dim Count As Integer
                        For Each BoxTwentyOneSwitch In BoxTwentyOneSwitches
                            If Count = 0 Then
                                TextBoxArrayList(41).text = TextBoxArrayList(41).text & BoxTwentyOneSwitch
                            Else
                                TextBoxArrayList(41).text = TextBoxArrayList(41).text & ",    " & BoxTwentyOneSwitch
                            End If
                            Count = Count + 1
                        Next
                        TextBoxArrayList(41).readonly = True
                        TextBoxArrayList(41).Enabled = True
                    End If
                Case Else
                    GroupArrayList(BoxIndex).Enabled = False
                    GroupArrayList(BoxIndex).Visible = False
            End Select

            If GroupArrayList(BoxIndex).Enabled = True Then
                CurrentTopLocation = CurrentTopLocation + GroupArrayList(BoxIndex).Height '- 2 '+ 1
                CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)
            End If

        Next BoxIndex

        gbMain.Height = CurrentTopLocation + 5
        gbFooter.Top = gbMain.Top + gbMain.Height + 1
        gbButtons.Top = gbFooter.Top
        Me.Height = gbFooter.Top + gbFooter.Height + 30


    End Sub

#End Region

#Region "Group Box"

    Private Sub Group_Item_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles GroupItem.Leave
        Dim Index As Integer
        Try
            Index = CInt(sender.NAME)
            If Not gRepeatMode Then
                Select Case Index
                    Case 1
                        If TextBoxArrayList(1).Enabled Then
                            If Trim(TextBoxArrayList(1).Text) = "" Then TextBoxArrayList(1).Focus() : Exit Sub
                        End If
                    Case 2
                        If TextBoxArrayList(2).Enabled Then
                            If Trim(TextBoxArrayList(2).Text) = "" Then TextBoxArrayList(2).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(3).Text) = "" Then TextBoxArrayList(3).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(4).Text) = "" Then TextBoxArrayList(4).Focus() : Exit Sub
                        End If
                    Case 3
                        If TextBoxArrayList(5).Enabled Then
                            If Trim(TextBoxArrayList(5).Text) = "" Then TextBoxArrayList(5).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(6).Text) = "" Then TextBoxArrayList(6).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(7).Text) = "" Then TextBoxArrayList(7).Focus() : Exit Sub
                        End If
                    Case 4
                        If TextBoxArrayList(8).Enabled Then
                            If Trim(TextBoxArrayList(8).Text) = "" Then TextBoxArrayList(8).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(9).Text) = "" Then TextBoxArrayList(9).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(10).Text) = "" Then TextBoxArrayList(10).Focus() : Exit Sub
                        End If
                    Case 7
                        If TextBoxArrayList(11).Enabled Then
                            If Trim(TextBoxArrayList(11).Text) = "" Then TextBoxArrayList(11).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(12).Text) = "" Then TextBoxArrayList(12).Focus() : Exit Sub
                        End If
                    Case 9
                        If TextBoxArrayList(13).Enabled Then
                            If Trim(TextBoxArrayList(13).Text) = "" Then TextBoxArrayList(13).Focus() : Exit Sub
                        End If
                    Case 11
                        If TextBoxArrayList(14).Enabled Then
                            If Trim(TextBoxArrayList(14).Text) = "" Then TextBoxArrayList(14).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(15).Text) = "" Then TextBoxArrayList(15).Focus() : Exit Sub
                        End If
                    Case 13
                        If TextBoxArrayList(28).Enabled Then
                            If Trim(TextBoxArrayList(28).Text) = "" Then TextBoxArrayList(28).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(29).Text) = "" Then TextBoxArrayList(29).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(30).Text) = "" Then TextBoxArrayList(30).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(31).Text) = "" Then TextBoxArrayList(31).Focus() : Exit Sub
                        End If
                    Case 14
                        If TextBoxArrayList(32).Enabled Then
                            If Trim(TextBoxArrayList(32).Text) = "" Then TextBoxArrayList(32).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(33).Text) = "" Then TextBoxArrayList(33).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(34).Text) = "" Then TextBoxArrayList(34).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(35).Text) = "" Then TextBoxArrayList(35).Focus() : Exit Sub
                        End If
                    Case 15
                        If TextBoxArrayList(36).Enabled Then
                            If Trim(TextBoxArrayList(36).Text) = "" Then TextBoxArrayList(36).Focus() : Exit Sub
                            If Trim(TextBoxArrayList(37).Text) = "" Then TextBoxArrayList(37).Focus() : Exit Sub
                        End If
                    Case 16
                        If TextBoxArrayList(38).Enabled Then
                            If Trim(TextBoxArrayList(38).Text) = "" Then TextBoxArrayList(38).Focus() : Exit Sub
                        End If
                    Case 17
                        If TextBoxArrayList(39).Enabled Then
                            If Trim(TextBoxArrayList(39).Text) = "" Then TextBoxArrayList(39).Focus() : Exit Sub
                        End If
                End Select

            End If
            CheckToEnableRepeatButton()
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try

    End Sub

#End Region

#Region "CHECK BOXES"

    Public Sub DrawCheckBoxControls()
        Dim ItemNum As Integer
        Dim CurrentPoint As Point
        Dim CurrentTopLocation, CurrentLeftLocation As Integer
        Try

            GroupArrayList.Insert(0, "null")
            CheckBoxArrayList.Insert(0, "null")
            ToolTipArrayList.Insert(0, "null")
            LabelArrayList.Insert(0, "null")
            LabelSecondaryArrayList.Insert(0, "null")
            TextBoxArrayList.Insert(0, "null")

            CurrentTopLocation = Me.gbIssue.Top + Me.gbIssue.Height - 2
            CurrentLeftLocation = 5
            CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)
            For ItemNum = 1 To 22
                '
                'Group Box
                '
                GroupItem = New GroupBox()
                GroupArrayList.Insert(ItemNum, GroupItem)
                gbMain.Controls.AddRange(New System.Windows.Forms.Control() {GroupItem})
                GroupItem.Font = New Font("Arial", 11.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                GroupItem.Location = CurrentPoint
                GroupItem.Name = ItemNum.ToString
                GroupItem.Size = New Size(775, 37)
                GroupItem.TabIndex = ItemNum '2
                GroupItem.TabStop = False
                AddHandler GroupItem.Leave, AddressOf Group_Item_Leave
                '
                'CheckBox1
                '
                CheckItem = New CheckBox()
                ToolTipItem = New ToolTip()
                ToolTipArrayList.Insert(ItemNum, ToolTipItem)
                CheckBoxArrayList.Insert(ItemNum, CheckItem)
                CheckItem.Font = New Font("Arial", 9.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                CheckItem.Location = New Point(30, 10)
                CheckItem.Name = ItemNum.ToString
                CheckItem.Size = New Size(730, 25)
                CheckItem.TabIndex = 0
                CheckItem.Text = "CHECKBOX " & ItemNum.ToString
                AddHandler CheckItem.CheckedChanged, AddressOf CheckItem_CheckChange
                AddHandler CheckItem.KeyDown, AddressOf CheckItem_KeyDown
                ToolTipItem.SetToolTip(CheckItem, CheckItem.Text)
                ToolTipItem.Active = True
                '
                'Label1
                '
                ItemLabel = New Label()
                LabelArrayList.Insert(ItemNum, ItemLabel)
                ItemLabel.Font = New Font("Arial", 10.0!, FontStyle.Bold, GraphicsUnit.Point, CType(0, Byte))
                ItemLabel.Location = New Point(5, 9)
                ItemLabel.Name = "Label1"
                ItemLabel.Size = New Size(25, 25)
                ItemLabel.TabIndex = 1
                ItemLabel.Text = ItemNum.ToString + "."
                ItemLabel.TextAlign = ContentAlignment.MiddleRight
                'Add Check and Label to Group Box 
                GroupItem.Controls.AddRange(New Control() {ItemLabel, CheckItem})
                GroupArrayList(ItemNum).visible = False
                GroupArrayList(ItemNum).enabled = False

                Select Case ItemNum
                    Case 1
                        CheckItem.Text = "TRACK WARRANT NO:  ____________________________________   IS VOID."
                        CheckItem.TabIndex = 1
                        AddTextBox(1, New Size(170, 15), New Point(180, 13), "1", GroupItem, False, 2)

                    Case 2
                        CheckItem.Text = "PROCEED FROM __________________________ TO __________________________ ON __________________ TRACK."
                        CheckItem.TabIndex = 3
                        AddTextBox(2, New Size(175, 15), New Point(143, 13), "2", GroupItem, False, 4)
                        AddTextBox(3, New Size(175, 15), New Point(343, 13), "3", GroupItem, False, 5)
                        AddTextBox(4, New Size(120, 15), New Point(551, 13), "4", GroupItem, False, 6)
                    Case 3
                        CheckItem.Text = "PROCEED FROM __________________________ TO __________________________ ON __________________ TRACK."
                        CheckItem.TabIndex = 7
                        AddTextBox(5, New Size(175, 15), New Point(143, 13), "5", GroupItem, False, 8)
                        AddTextBox(6, New Size(175, 15), New Point(343, 13), "6", GroupItem, False, 9)
                        AddTextBox(7, New Size(120, 15), New Point(551, 13), "7", GroupItem, False, 10)
                        'GroupArrayList(ItemNum).visible = False
                        'GroupArrayList(ItemNum).enabled = False
                    Case 4
                        CheckItem.Text = "WORK BETWEEN ________________________ AND __________________________ ON __________________ TRACK."
                        CheckItem.TabIndex = 11
                        AddTextBox(8, New Size(173, 15), New Point(146, 13), "8", GroupItem, False, 12)
                        AddTextBox(9, New Size(173, 15), New Point(347, 13), "9", GroupItem, False, 13)
                        AddTextBox(10, New Size(120, 15), New Point(555, 13), "10", GroupItem, False, 14)
                    Case 5
                        CheckItem.Text = ""
                        GroupArrayList(ItemNum).visible = False
                        GroupArrayList(ItemNum).enabled = False
                    Case 6
                        CheckItem.Text = ""
                        GroupArrayList(ItemNum).visible = False
                        GroupArrayList(ItemNum).enabled = False
                    Case 7
                        GroupItem.Size = New Size(775, 60)
                        CheckItem.Size = New Size(740, 25)
                        CheckItem.Text = "NOT IN EFFECT UNTIL AFTER ARRIVAL OF _________________________________________________________________"
                        CheckItem.TabIndex = 15
                        AddTextBox(11, New Size(460, 15), New Point(285, 13), "11", GroupItem, False, 16)
                        AddLabel(1, New Size(600, 25), New Point(50, 30), "AT__________________________________", GroupItem)
                        AddTextBox(12, New Size(180, 15), New Point(70, 33), "12", GroupItem, False, 17)
                    Case 8
                        CheckItem.Text = ""
                        GroupArrayList(ItemNum).visible = False
                        GroupArrayList(ItemNum).enabled = False
                        'CheckItem.TabIndex = 16
                        'CheckItem.Text = "HOLD MAIN TRACK AT LAST NAMED POINT."
                    Case 9
                        CheckItem.Text = "DO NOT FOUL LIMITS AHEAD OF ________________________________________________"
                        CheckItem.TabIndex = 17
                        AddTextBox(13, New Size(340, 15), New Point(230, 13), "13", GroupItem, False, 18)
                    Case 10
                        CheckItem.Text = "CLEAR MAIN TRACK AT LAST NAMED POINT."
                        CheckItem.TabIndex = 19
                    Case 11
                        GroupItem.Size = New Size(775, 65)
                        CheckItem.Size = New Size(730, 35)
                        ItemLabel.Location = New Point(5, 24)
                        CheckItem.Location = New Point(30, 22)
                        CheckItem.Text = "BETWEEN _______________________________  AND  _______________________________ MAKE " & _
                                                  "ALL MOVEMENTS AT RESTRICTED SPEED. LIMITS OCCUPIED BY TRAIN, MEN, OR EQUIPMENT."
                        CheckItem.TabIndex = 20
                        AddTextBox(14, New Size(200, 15), New Point(110, 22), "14", GroupItem, False, 21)
                        AddTextBox(15, New Size(200, 15), New Point(360, 22), "15", GroupItem, False, 22)
                        GroupItem.Controls.AddRange(New Control() {Me.cmbBox11From, Me.cmbBox11To})
                        Me.cmbBox11From.Location = New Point(110, 12)
                        Me.cmbBox11To.Location = New Point(365, 12)
                        'Me.cmbBox11From.Visible = True
                        'Me.cmbBox11To.Visible = True
                        'Me.cmbBox11From.BringToFront()
                        'Me.cmbBox11To.BringToFront()
                        TextBoxArrayList(14).Visible = False
                        TextBoxArrayList(15).Visible = False

                    Case 12
                        GroupItem.Size = New Size(775, 120)
                        CheckItem.Text = "THIS AUTHORITY IS JOINT WITH THE FOLLOWING:"
                        CheckItem.TabIndex = 23
                        AddLabel(2, New Size(700, 25), New Point(50, 30), "_______________________ BETWEEN ___________________________ AND _____________________________", GroupItem)
                        AddTextBox(16, New Size(150, 15), New Point(50, 33), "16", GroupItem, False, 24)
                        AddTextBox(17, New Size(180, 15), New Point(280, 33), "17", GroupItem, False, 25)
                        AddTextBox(18, New Size(180, 15), New Point(495, 33), "18", GroupItem, False, 26)

                        AddLabel(3, New Size(700, 25), New Point(50, 50), "_______________________ BETWEEN ___________________________ AND _____________________________", GroupItem)
                        AddTextBox(19, New Size(150, 15), New Point(50, 53), "19", GroupItem, False, 27)
                        AddTextBox(20, New Size(180, 15), New Point(280, 53), "20", GroupItem, False, 28)
                        AddTextBox(21, New Size(180, 15), New Point(495, 53), "21", GroupItem, False, 29)

                        AddLabel(4, New Size(700, 25), New Point(50, 70), "_______________________ BETWEEN ___________________________ AND _____________________________", GroupItem)
                        AddTextBox(22, New Size(150, 15), New Point(50, 73), "22", GroupItem, False, 30)
                        AddTextBox(23, New Size(180, 15), New Point(280, 73), "23", GroupItem, False, 31)
                        AddTextBox(24, New Size(180, 15), New Point(495, 73), "24", GroupItem, False, 32)

                        AddLabel(5, New Size(700, 25), New Point(50, 90), "_______________________ BETWEEN ___________________________ AND _____________________________", GroupItem)
                        AddTextBox(25, New Size(150, 15), New Point(50, 93), "25", GroupItem, False, 33)
                        AddTextBox(26, New Size(180, 15), New Point(280, 93), "26", GroupItem, False, 34)
                        AddTextBox(27, New Size(180, 15), New Point(495, 93), "27", GroupItem, False, 35)
                    Case 13
                        CheckItem.Text = "DO NOT EXCEED _____ MPH BETWEEN__________________ AND __________________  ON _____________ TRACK."
                        CheckItem.TabIndex = 36
                        AddTextBox(28, New Size(30, 15), New Point(146, 13), "28", GroupItem, False, 37)
                        AddTextBox(29, New Size(120, 15), New Point(272, 13), "29", GroupItem, False, 38)
                        AddTextBox(30, New Size(120, 15), New Point(430, 13), "30", GroupItem, False, 39)
                        AddTextBox(31, New Size(90, 15), New Point(580, 13), "31", GroupItem, False, 40)
                    Case 14
                        CheckItem.Text = "DO NOT EXCEED _____ MPH BETWEEN__________________ AND __________________  ON _____________ TRACK."
                        CheckItem.TabIndex = 41
                        AddTextBox(32, New Size(30, 15), New Point(146, 13), "32", GroupItem, False, 42)
                        AddTextBox(33, New Size(120, 15), New Point(272, 13), "33", GroupItem, False, 43)
                        AddTextBox(34, New Size(120, 15), New Point(430, 13), "34", GroupItem, False, 44)
                        AddTextBox(35, New Size(90, 15), New Point(580, 13), "35", GroupItem, False, 45)
                    Case 15
                        CheckItem.Text = optBox15StringOne
                        'CheckItem.Text = "CROSSING WARNING SYSTEM AT:"
                        If optCrossingNotificationOptionOne Then
                            GroupItem.Size = New Size(775, 100)
                        Else
                            GroupItem.Size = New Size(775, 80)
                        End If

                        CheckItem.Size = New Size(730, 25)
                        CheckItem.TabIndex = 46
                        AddLabel(6, New Size(600, 25), New Point(50, 30), "MP__________,  _________________________________________ (STREET, HWY, OR ROAD NAME)", GroupItem)
                        If optCrossingNotificationOptionOne Then
                            AddLabel(7, New Size(600, 35), New Point(50, 50), "STOP AND PROTECT THE CROSSING PER GCOR 6.32.2.", GroupItem)                    'Add Check and Label to Group Box 'Add Check and Label to Group Box 
                        Else
                            AddLabel(7, New Size(600, 25), New Point(50, 50), optBox15StringTwo, GroupItem)                    'Add Check and Label to Group Box 'Add Check and Label to Group Box 
                        End If

                        AddTextBox(36, New Size(50, 15), New Point(75, 33), "36", GroupItem, False, 47)
                        AddTextBox(37, New Size(280, 15), New Point(150, 33), "37", GroupItem, False, 48)
                    Case 16
                        'CheckItem.Text = ""
                        'GroupArrayList(ItemNum).visible = False
                        'GroupArrayList(ItemNum).enabled = False
                        GroupItem.Size = New Size(785, 47)
                        CheckItem.TabIndex = 49
                        CheckItem.Text = "BULLETINS IN EFFECT:"
                        AddTextBox(38, New Size(530, 30), New Point(185, 14), "38", GroupItem, False, 50)
                    Case 17
                        CheckItem.Text = "OTHER SPECIFIC INSTRUCTIONS:"
                        GroupItem.Size = New Size(775, 60)
                        CheckItem.Size = New Size(200, 30)
                        ItemLabel.Location = New Point(5, 12)
                        CheckItem.TabIndex = 51
                        AddTextBox(39, New Size(560, 40), New Point(142, 10), "39", GroupItem, True, 52)
                    Case 18
                        CheckItem.Text = ""
                        GroupArrayList(ItemNum).visible = False
                        GroupArrayList(ItemNum).enabled = False
                    Case 19
                        CheckItem.Text = ""
                        GroupArrayList(ItemNum).visible = False
                        GroupArrayList(ItemNum).enabled = False
                    Case 20
                        CheckItem.Text = "BE PREPARED TO STOP AT THE FOLLOWING SWITCH(ES) UNTIL KNOWN TO BE IN THE NORMAL POSITION: "
                        GroupItem.Size = New Size(775, 50)
                        CheckItem.Size = New Size(730, 15)
                        CheckItem.Location = New Point(30, 14)
                        CheckItem.TabIndex = 53
                        AddTextBox(40, New Size(550, 15), New Point(15, 28), "40", GroupItem, False, 54)
                    Case 21
                        CheckItem.Text = "PERMISSION TO LEAVE THE FOLLOWING SWITCH(ES) IN THE REVERSE POSITION: "
                        GroupItem.Size = New Size(775, 50)
                        CheckItem.Size = New Size(730, 15)
                        CheckItem.Location = New Point(30, 14)
                        CheckItem.TabIndex = 55
                        AddTextBox(41, New Size(550, 15), New Point(15, 28), "41", GroupItem, False, 56)
                    Case 22
                        GroupItem.Size = New Size(775, 50)
                        CheckItem.Text = "" '"A TOTAL OF ____ BOXES ARE CHECKED:"
                        CheckItem.TabIndex = 57
                        If optDisplayBox22 Then
                            CheckItem.Enabled = True
                            CheckItem.Visible = True
                            ItemLabel.Visible = True
                        Else
                            CheckItem.Enabled = False
                            CheckItem.Visible = False
                            ItemLabel.Visible = False
                        End If
                        AddLabel(8, New Size(600, 25), New Point(50, 15), "A TOTAL OF ____ BOXES ARE MARKED:", GroupItem)
                        AddTextBox(42, New Size(25, 15), New Point(129, 18), "42", GroupItem, False, 58)
                        AddTextBox(43, New Size(400, 15), New Point(295, 20), "43", GroupItem, False, 59)
                End Select
                If GroupItem.Enabled = True Then
                    CurrentTopLocation = CurrentTopLocation + GroupItem.Height - 2 '+ 1
                    CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)
                End If

            Next
            CurrentPoint = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Public Sub CheckItem_CheckChange(ByVal sender As Object, ByVal e As EventArgs)
        Dim Index As Integer
        Try
            Index = CInt(sender.NAME)
            If Not gRepeatMode Then
                Select Case Index
                    Case 1
                        If gBoxOneRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            TextBoxArrayList(1).ReadOnly = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(1).Enabled = True
                            TextBoxArrayList(1).Focus()
                            TextBoxArrayList(1).Tag = "VALIDATE"

                            If optUseReleaseAuthorityVerbiageDME Then
                                'If Not Me.PerformSwitchPointConfirmationDME Then
                                '    gBoxOneRequired = False
                                '    CheckBoxArrayList(Index).CHECKSTATE = CheckState.Unchecked
                                '    Me.CloseForm()
                                '    Exit Sub
                                'End If
                            Else
                                If Not Me.PerformSwitchPointConfirmation Then
                                    gBoxOneRequired = False
                                    CheckBoxArrayList(Index).CHECKSTATE = CheckState.Unchecked
                                    Me.CloseForm()
                                    Exit Sub
                                End If
                            End If

                        Else
                            TextBoxArrayList(1).Tag = ""
                            TextBoxArrayList(1).Text = ""
                            TextBoxArrayList(1).Enabled = False
                        End If

                    Case 2
                        If gBoxTwoRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            CheckBoxArrayList(Index).tag = "VALIDATE"
                            TextBoxArrayList(2).ReadOnly = True
                            TextBoxArrayList(3).ReadOnly = True
                            TextBoxArrayList(4).ReadOnly = True
                            TextBoxArrayList(2).Tag = "VALIDATE"
                            TextBoxArrayList(3).Tag = "VALIDATE"
                            TextBoxArrayList(4).Tag = "VALIDATE"
                        Else

                        End If
                    Case 3
                        If gBoxThreeRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            CheckBoxArrayList(Index).tag = "VALIDATE"
                            TextBoxArrayList(5).ReadOnly = True
                            TextBoxArrayList(6).ReadOnly = True
                            TextBoxArrayList(7).ReadOnly = True
                            TextBoxArrayList(5).Tag = "VALIDATE"
                            TextBoxArrayList(6).Tag = "VALIDATE"
                            TextBoxArrayList(7).Tag = "VALIDATE"
                        End If
                    Case 4
                        If gBoxFourRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            CheckBoxArrayList(Index).tag = "VALIDATE"
                            TextBoxArrayList(8).ReadOnly = True
                            TextBoxArrayList(9).ReadOnly = True
                            TextBoxArrayList(10).ReadOnly = True
                            TextBoxArrayList(8).Tag = "VALIDATE"
                            TextBoxArrayList(9).Tag = "VALIDATE"
                            TextBoxArrayList(10).Tag = "VALIDATE"
                        End If
                    Case 7
                        If gItemFiveRequired Then
                            CheckBoxArrayList(7).Checked = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(11).Enabled = True
                            TextBoxArrayList(12).Enabled = True
                            TextBoxArrayList(11).Tag = "VALIDATE"
                            TextBoxArrayList(12).Tag = "VALIDATE"
                            TextBoxArrayList(11).Focus()
                        Else
                            TextBoxArrayList(11).Enabled = False
                            TextBoxArrayList(11).Text = ""
                            TextBoxArrayList(12).Enabled = False
                            TextBoxArrayList(12).Text = ""
                            TextBoxArrayList(11).Tag = ""
                            TextBoxArrayList(12).Tag = ""
                        End If

                        'Case 8
                        '    If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                        '        CheckBoxArrayList(10).Enabled = False
                        '    Else
                        '        CheckBoxArrayList(10).Enabled = True
                        '    End If
                    Case 9
                        If gItemSixRequired Or gItemTwoToTrainWithRadioBlocking Or gItemTwoSameTrainWithRadioBlocking Then
                            CheckBoxArrayList(Index).Checked = True
                            TextBoxArrayList(13).ReadOnly = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(13).Enabled = True
                            TextBoxArrayList(13).Tag = "VALIDATE"
                            TextBoxArrayList(13).Focus()
                        Else
                            TextBoxArrayList(13).Enabled = False
                            TextBoxArrayList(13).Text = ""
                            TextBoxArrayList(13).Tag = ""
                        End If
                    Case 10
                        If gItem10Required Or gBoxTenRequired Then
                            CheckBoxArrayList(Index).Checked = True
                        End If
                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            'If Box 10 checked, disable Box 8
                            CheckBoxArrayList(8).Enabled = False
                            CheckForBoxTwentyOneSituation()
                        Else
                            CheckBoxArrayList(8).Enabled = True
                        End If
                    Case 11
                        'Dim FromLimits, ToLimits As String
                        'If CheckBoxArrayList(4).ENABLED = True Then
                        'FromLimits = TextBoxArrayList(8).Text
                        'ToLimits = TextBoxArrayList(9).Text
                        'ElseIf CheckBoxArrayList(2).ENABLED = True Then
                        '    FromLimits = TextBoxArrayList(2).Text
                        '    ToLimits = TextBoxArrayList(3).Text
                        'End If

                        'If global joint required flag is true, then alreadys check box 11
                        If gJointAuthRequired Or gBox8Required Then
                            CheckBoxArrayList(Index).Checked = True
                            'TextBoxArrayList(14).Text = FromLimits
                            'TextBoxArrayList(15).Text = ToLimits
                            'TextBoxArrayList(14).ReadOnly = True
                            'TextBoxArrayList(15).ReadOnly = True
                            Me.CheckBoxElevenChange(Me.CompleteCompSelection, Me.DirectionOfTheWarrant)
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(14).Tag = "VALIDATE"
                            TextBoxArrayList(15).Tag = "VALIDATE"
                            'TextBoxArrayList(14).Text = FromLimits
                            'TextBoxArrayList(15).Text = ToLimits
                            TextBoxArrayList(14).Enabled = True
                            TextBoxArrayList(15).Enabled = True
                            TextBoxArrayList(14).ReadOnly = True
                            TextBoxArrayList(15).ReadOnly = True
                            Me.TextBoxArrayList(14).Text = Me.cmbBox11From.Text
                            Me.TextBoxArrayList(15).Text = Me.cmbBox11To.Text
                            Me.cmbBox11From.Visible = True
                            Me.cmbBox11To.Visible = True
                            Me.cmbBox11From.BringToFront()
                            Me.cmbBox11To.BringToFront()
                            Me.CheckBoxElevenChange(Me.CompleteCompSelection, Me.DirectionOfTheWarrant)
                        Else
                            TextBoxArrayList(14).Enabled = False
                            TextBoxArrayList(15).Enabled = False
                            TextBoxArrayList(14).Text = ""
                            TextBoxArrayList(15).Text = ""
                            TextBoxArrayList(14).Tag = ""
                            TextBoxArrayList(15).Tag = ""
                            TextBoxArrayList(14).ReadOnly = False
                            TextBoxArrayList(15).ReadOnly = False
                            Me.cmbBox11From.Visible = False
                            Me.cmbBox11To.Visible = False
                            TextBoxArrayList(14).BringToFront()
                            TextBoxArrayList(15).BringToFront()

                        End If
                    Case 12
                        If gJointAuthRequired Then
                            CheckBoxArrayList(12).Checked = True
                        End If
                        Dim I As Integer
                        For I = 16 To 27
                            TextBoxArrayList(I).Enabled = True
                            TextBoxArrayList(I).READONLY = True
                        Next

                    Case 13
                        If gBox13Required Then
                            'CheckBoxArrayList(Index).Checked = True
                            TextBoxArrayList(28).READONLY = True
                            TextBoxArrayList(29).READONLY = True
                            TextBoxArrayList(30).READONLY = True
                            TextBoxArrayList(31).READONLY = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(28).Enabled = True
                            TextBoxArrayList(29).Enabled = True
                            TextBoxArrayList(30).Enabled = True
                            TextBoxArrayList(31).Enabled = True
                            TextBoxArrayList(28).Focus()
                            TextBoxArrayList(28).Tag = "VALIDATE"
                            TextBoxArrayList(29).Tag = "VALIDATE"
                            TextBoxArrayList(30).Tag = "VALIDATE"
                            TextBoxArrayList(31).Tag = "VALIDATE"
                        Else
                            TextBoxArrayList(28).Enabled = False
                            TextBoxArrayList(29).Enabled = False
                            TextBoxArrayList(30).Enabled = False
                            TextBoxArrayList(31).Enabled = False
                            'TextBoxArrayList(28).Text = ""
                            'TextBoxArrayList(29).Text = ""
                            'TextBoxArrayList(30).Text = ""
                            'TextBoxArrayList(31).Text = ""
                            TextBoxArrayList(28).Tag = ""
                            TextBoxArrayList(29).Tag = ""
                            TextBoxArrayList(30).Tag = ""
                            TextBoxArrayList(31).Tag = ""
                        End If
                    Case 14
                        If gBox14Required Then
                            'CheckBoxArrayList(Index).Checked = True
                            TextBoxArrayList(32).READONLY = True
                            TextBoxArrayList(33).READONLY = True
                            TextBoxArrayList(34).READONLY = True
                            TextBoxArrayList(35).READONLY = True
                        End If
                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(32).Enabled = True
                            TextBoxArrayList(33).Enabled = True
                            TextBoxArrayList(34).Enabled = True
                            TextBoxArrayList(35).Enabled = True
                            TextBoxArrayList(32).Focus()
                            TextBoxArrayList(32).Tag = "VALIDATE"
                            TextBoxArrayList(33).Tag = "VALIDATE"
                            TextBoxArrayList(34).Tag = "VALIDATE"
                            TextBoxArrayList(35).Tag = "VALIDATE"
                        Else
                            TextBoxArrayList(32).Enabled = False
                            TextBoxArrayList(33).Enabled = False
                            TextBoxArrayList(34).Enabled = False
                            TextBoxArrayList(35).Enabled = False
                            'TextBoxArrayList(32).Text = ""
                            'TextBoxArrayList(33).Text = ""
                            'TextBoxArrayList(34).Text = ""
                            'TextBoxArrayList(35).Text = ""
                            TextBoxArrayList(32).Tag = ""
                            TextBoxArrayList(33).Tag = ""
                            TextBoxArrayList(34).Tag = ""
                            TextBoxArrayList(35).Tag = ""
                        End If
                    Case 15
                        If gBox15Required Then
                            'CheckBoxArrayList(Index).Checked = True
                            TextBoxArrayList(36).READONLY = True
                            TextBoxArrayList(37).READONLY = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(36).Enabled = True
                            TextBoxArrayList(37).Enabled = True
                            TextBoxArrayList(36).Focus()
                            TextBoxArrayList(36).Tag = "VALIDATE"
                            TextBoxArrayList(37).Tag = "VALIDATE"
                        Else
                            TextBoxArrayList(36).Enabled = False
                            'TextBoxArrayList(36).Text = ""
                            TextBoxArrayList(36).Tag = ""
                            TextBoxArrayList(37).Enabled = False
                            'TextBoxArrayList(37).Text = ""
                            TextBoxArrayList(37).Tag = ""

                        End If
                    Case 16
                        If optForceBox16 Then
                            CheckBoxArrayList(Index).Checked = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(38).Enabled = True
                            TextBoxArrayList(38).Focus()
                            TextBoxArrayList(38).Tag = "VALIDATE"
                        Else
                            TextBoxArrayList(38).Enabled = False
                            TextBoxArrayList(38).Text = ""
                            TextBoxArrayList(38).Tag = ""
                        End If
                    Case 17
                        If gBox17Required Then
                            CheckBoxArrayList(Index).Checked = True
                        End If

                        If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                            TextBoxArrayList(39).Enabled = True
                            TextBoxArrayList(39).Focus()
                            TextBoxArrayList(39).Tag = "VALIDATE"
                            If gItemTwoSameTrainWithRadioBlocking Or gItemTwoToTrainWithRadioBlocking Then
                                TextBoxArrayList(39).ReadOnly = True
                            End If
                        Else
                            TextBoxArrayList(39).Enabled = False
                            'TextBoxArrayList(39).Text = ""
                            TextBoxArrayList(39).Tag = ""
                        End If
                    Case 20
                        If gBoxTwelveRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            CheckBoxArrayList(Index).tag = "VALIDATE"
                            TextBoxArrayList(40).ReadOnly = True
                            TextBoxArrayList(40).Tag = "VALIDATE"
                        Else

                        End If
                    Case 21
                        If gBoxThirteenRequired Then
                            CheckBoxArrayList(Index).Checked = True
                            CheckBoxArrayList(Index).tag = "VALIDATE"
                            TextBoxArrayList(41).ReadOnly = True
                            TextBoxArrayList(41).Tag = "VALIDATE"
                            TextBoxArrayList(41).Enabled = True
                            TextBoxArrayList(41).Focus()
                        End If

                End Select

                If CheckBoxArrayList(Index).CHECKSTATE = CheckState.Checked Then
                    CheckBoxArrayList(Index).AccessibleDescription = "CHECKED"
                    CheckBoxArrayList(Index).tag = "VALIDATE"
                Else
                    CheckBoxArrayList(Index).AccessibleDescription = ""
                    CheckBoxArrayList(Index).tag = ""

                End If

                AddCheckBoxesToItem22()

            Else
                'if the check box marked then keep it marked after the REPEAT button was selected
                If CheckBoxArrayList(Index).AccessibleDescription = "CHECKED" Then
                    CheckBoxArrayList(Index).Checked = True
                End If
            End If

            CheckToEnableRepeatButton()

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Private Sub AddCheckBoxesToItem22()



        Try
            Dim I As Integer
            Dim CheckBox22 As String = String.Empty
            Dim NumOfBoxesChecked As Integer
            Dim NumberOfBoxesToCheck As Integer

            If optDisplayBox22 Then
                NumberOfBoxesToCheck = 22
            Else
                NumberOfBoxesToCheck = 21
            End If

            For I = 1 To NumberOfBoxesToCheck
                If CheckBoxArrayList(I).CHECKSTATE = CheckState.Checked Then
                    CheckBoxArrayList(22).CHECKSTATE = CheckState.Checked
                    CheckBoxArrayList(22).AccessibleDescription = "CHECKED"
                    CheckBox22 = CheckBox22 & I & "  "
                    NumOfBoxesChecked = NumOfBoxesChecked + 1
                End If

            Next

            TextBoxArrayList(43).Text = CheckBox22
            TextBoxArrayList(43).Tag = "VALIDATE"

            TextBoxArrayList(42).Tag = "VALIDATE"
            TextBoxArrayList(42).Text = NumOfBoxesChecked
            TextBoxArrayList(42).ReadOnly = True
            TextBoxArrayList(43).ReadOnly = True
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try

    End Sub

    Private Sub CheckItem_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles CheckItem.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    sender.ForeColor = Color.DarkGreen
                    sender.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            Else
                If e.KeyCode = Keys.Enter Then
                    If sender.checked Then
                        sender.checked = False
                    Else
                        sender.checked = True
                    End If
                End If

            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

#End Region

#Region "TEXT BOXES"
    Private Sub TextBoxItem_Focus(ByVal sender As Object, ByVal e As EventArgs) Handles TextBoxItem.Enter
        SendKeys.Send("{Home}+{End}")
    End Sub

    Private Sub TextBoxItem_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles TextBoxItem.Leave
        Try
            Dim sTextBox As String = String.Empty
            If Not IsNothing(sender) Then
                sTextBox = sender.Name
            End If
            Select Case sTextBox
                Case "14", "15", "28", "29", "30", "31", "32", "33", "34", "35", "38", "39", "36", "37", 52
                    sender.Text = FormatProperString(sender.Text)
            End Select
        Catch

        End Try
    End Sub

    Private Sub TextBoxItem_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles TextBoxItem.KeyDown

        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    sender.ForeColor = Color.DarkGreen
                    sender.Tag = "VALIDATED"
                    SendKeys.Send("{TAB}")
                    ValidateInputBoxes()
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub
#End Region

#Region "Objects In Header"

    Private Sub cbIssueTo_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbIssueTo.TextChanged
        txtIssueTo.Text = cbIssueTo.Text
        If Not ForceBoxOne Then
            CheckForBoxOneWarrantInSelectedABSLimits()
        End If
    End Sub

    Private Sub cbAT_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbAT.TextChanged
        txtAtLocation.Text = cbAT.Text
    End Sub

    Private Sub txtIssueTo_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtIssueTo.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    txtIssueTo.ForeColor = Color.DarkGreen
                    txtIssueTo.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Private Sub txtAtLocation_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtAtLocation.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    txtAtLocation.ForeColor = Color.DarkGreen
                    txtAtLocation.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Private Sub txtSubdivision_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtSubdivision.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    txtSubdivision.ForeColor = Color.DarkGreen
                    txtSubdivision.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try

    End Sub

    Private Sub txtDate_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtDate.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    txtDate.ForeColor = Color.DarkGreen
                    txtDate.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Private Sub txtTWnum_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtTWnum.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    txtTWnum.ForeColor = Color.DarkGreen
                    txtTWnum.Tag = "VALIDATED"
                    ValidateInputBoxes()
                    SendKeys.Send("{TAB}")
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

#End Region

#Region "BUTTON COMMANDS"

    Private Sub btnRepeat_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRepeat.Click
        Try
            gHistoryMessage = "REPEAT BUTTON SELECTED FOR TRACK WARRANT:" & Me.txtTWnum.Text
            SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
            SysEvent = Nothing
            Dim I As Integer
            Dim CurrentPoint As Point
            Dim CurrentTopLocation, CurrentLeftLocation As Integer

            CurrentTopLocation = Me.gbIssue.Top + Me.gbIssue.Height - 2
            CurrentLeftLocation = 5
            CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)


            gRepeatMode = True
            btnRepeat.Visible = False
            btnOK.Visible = True

            'Set The OK time
            'txtOKTIME.Text = Format(Now, "HH:mm")

            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    txtOKTIME.Text = Format(Now.AddHours(1 + gThisWorkstationTimeFactor), "HH:mm")
                Case "PACIFIC"
                    txtOKTIME.Text = Format(Now.AddHours(-2 + gThisWorkstationTimeFactor), "HH:mm")
                Case "MOUNTAIN"
                    txtOKTIME.Text = Format(Now.AddHours(-1 + gThisWorkstationTimeFactor), "HH:mm")
                Case Else
                    txtOKTIME.Text = Format(Now, "HH:mm")
            End Select


            Me.ResumeLayout(False)
            'Check all the check boxes' status. Disable boxes that are not checked and set VAILDATE status for the ones that are
            For I = 1 To 21
                GroupArrayList(I).SuspendLayout()
                'Disable all check boxes, frames and labels that are not checked
                If CheckBoxArrayList(I).CHECKSTATE = CheckState.Unchecked Then
                    'GroupArrayList(I).Enabled = False
                    GroupArrayList(I).Visible = False
                End If
                If CheckBoxArrayList(I).CHECKSTATE = CheckState.Checked Then
                    CheckBoxArrayList(I).AutoCheck = True
                    GroupArrayList(I).Location = CurrentPoint
                    CurrentTopLocation = CurrentTopLocation + GroupArrayList(I).Height - 2 '+ 1
                    CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)
                End If
            Next I
            For I = 1 To 21
                GroupArrayList(I).ResumeLayout()
            Next

            GroupArrayList(22).Location = CurrentPoint
            CurrentTopLocation = CurrentTopLocation + GroupItem.Height + 5 '+ 1
            CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)

            gbMain.Height = CurrentTopLocation + 5
            gbFooter.Top = gbMain.Top + gbMain.Height + 1
            gbButtons.Top = gbFooter.Top
            Me.Height = gbFooter.Top + gbFooter.Height + 30


            'gbMain.Height = CurrentTopLocation
            'CurrentTopLocation = CurrentTopLocation + gbMain.Height + 2 '+ 1
            'CurrentPoint = New Point(CurrentLeftLocation, CurrentTopLocation)
            'gbFooter.Top = CurrentTopLocation - 120
            'gbButtons.Top = CurrentTopLocation - 120
            'Me.Height = CurrentTopLocation + 30

            'Lock all text boxes and enable them
            For I = 1 To 41     'changed from 18 boxes to 38 boxes 07-23-02
                TextBoxArrayList(I).ReadOnly = True
            Next I

            'Lock the Number, subdivision, and data field and make it be validated

            txtTWnum.Tag = "VALIDATE"
            txtTWnum.ReadOnly = True
            txtSubdivision.Tag = "VALIDATE"
            txtDate.Tag = "VALIDATE"

            'Lock the ISSUE TO field and make it be validated
            cbIssueTo.Enabled = False
            cbIssueTo.Visible = False

            txtIssueTo.ReadOnly = True
            txtIssueTo.Visible = True
            txtIssueTo.Tag = "VALIDATE"


            'Lock the AT LOCATION field and make it be validated
            cbAT.Enabled = False
            cbAT.Visible = False

            'ADDED THIS ON 05-12-2003
            txtAtLocation.Text = cbAT.Text

            txtAtLocation.ReadOnly = True
            txtAtLocation.Visible = True
            txtAtLocation.Tag = "VALIDATE"


            'Make the box eleven combo boxes invisible
            Me.cmbBox11From.Visible = False
            Me.cmbBox11To.Visible = False
            Me.TextBoxArrayList(14).Visible = True
            Me.TextBoxArrayList(15).Visible = True

            'Set the focus to the track warrant number field to start validation
            txtTWnum.Focus()

            '****************************************************************************************************
            ' This code moved from the OK button Click event, so the warrant is applied to the system before it is OKed
            '****************************************************************************************************
            Dim TrackWarrant As Warrant
            TrackWarrant = New Warrant()
            For I = 1 To 3
                Select Case I
                    Case 1
                        With Me

                            If TrackWarrant.AddTrackWarrantNewToSystem(Me, Me.VoidedWarrant) Then
                                'write info to event database
                            Else
                                gHistoryMessage = "System Error! Please Consider Track Warrant " & Me.txtTWnum.Text & " NOT in Effect!"
                                SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                                TrackWarrantEvent(gHistoryMessage)
                                MessageBox.Show(Me, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                'MsgBox(gHistoryMessage, vbCritical)
                                ClearSystemVariables()
                                Me.CloseForm()
                                Exit Sub
                            End If
                        End With

                    Case 2 'Check to see if track warrant data is written to database OK
                        If TrackWarrant.AddTrackWarrantToDB() Then
                            'write info to event database
                        Else
                            ClearSystemVariables()
                            gHistoryMessage = "System Database Error! Please Consider Track Warrant " & Me.txtTWnum.Text & " NOT in Effect!"
                            SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                            TrackWarrantEvent(gHistoryMessage)
                            MessageBox.Show(Me, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            'MsgBox(gHistoryMessage, vbCritical)
                            ClearSystemVariables()
                            Me.CloseForm()
                            Exit Sub
                        End If

                    Case 3
                        SystemForms.MySystemForms.frmMainMDI.UpdateAuthoritySummary()
                        ThisCurrentWarrant = TrackWarrant
                End Select
            Next

            txtTWnum.Focus()
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

    Private Sub btnVoid_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVoid.Click
        Dim sw As Switch
        Try
            If Not gRepeatMode Then

                'return any switches to normal position if Item 21 was selected on them
                ' before the repeat button is selected.
                'Dim Comp As Object
                'For Each Comp In CompleteCompSelection
                '    If Comp.GetType.ToString = "TMDS.SwitchComponents.Switch" Then
                '        If Comp.AuthorityCounts = 0 Then
                '            If Not Comp.ControlledReverse And Comp.LeaveReversePermission Then
                '                Comp.LeaveReversePermission = False
                '                'Comp.ControlSwitch(False)
                '            End If
                '        End If
                '    End If
                'Next
                For Each sw In Me.Box21Switch
                    If sw.Authorities.Count = 0 Then
                        If sw.ControlledReverse Then
                            sw.ControlSwitch()
                            sw.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority)
                        End If
                    End If
                Next
                'If Not IsNothing(Me.Box21Switch) Then
                '    Me.Box21Switch.ControlSwitch()
                '    Me.Box21Switch.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority)
                'End If

                ClearSystemVariables()
                gHistoryMessage = "VOID BUTTON SELECTED FOR TRACK WARRANT:" & Me.txtTWnum.Text
                SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                Me.CloseForm()
            Else
                'launch the report clear form
                'If VoidTrackWarrant(ThisCurrentWarrant, False, False) Then
                'ClearSystemVariables()
                'gHistoryMessage = "VOID BUTTON SELECTED FOR TRACK WARRANT:" & Me.txtTWnum.Text
                'SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                'Me.CloseForm()
                'Else
                '    'ClearSystemVariables()
                'End If
                Dim UserMessage As String = "VOID TRACK WARRANT NO. " & Me.ThisCurrentWarrant.WarrantNumber.ToString
                Dim UserResponse As Integer
                UserResponse = MessageBox.Show(Me, UserMessage, "VOID TRACK AUTHORITY", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If UserResponse = 6 Then
                    Me.TrackWarrantEvent("DISPATCHER SELECTED YES TO VOID TRACK AUTHORITY NO. " & Me.ThisCurrentWarrant.WarrantNumber.ToString)
                    If ThisCurrentWarrant.RemoveActiveTrackWarrantFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        If ThisCurrentWarrant.RemoveActiveTrackWarrantFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                            TMDS.GlobalDeclarations.TrackWarrants.Remove(ThisCurrentWarrant)
                            ThisCurrentWarrant = Nothing
                            TMDS.SystemForms.MySystemForms.frmMainMDI.UpdateAuthoritySummary()
                        Else
                            Me.TrackWarrantEvent("AUTHORITY NO. " & Me.ThisCurrentWarrant.WarrantNumber.ToString & " NOT REMOVED FROM DATABASE!")
                        End If
                        For Each sw In Me.Box21Switch
                            If sw.Authorities.Count = 0 Then
                                If sw.ControlledReverse Then
                                    sw.ControlSwitch()
                                    sw.UpdateProtectOpenSwitch(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority)
                                End If
                            End If
                        Next
                    Else
                        Me.TrackWarrantEvent("AUTHORITY NO. " & Me.ThisCurrentWarrant.WarrantNumber.ToString & " NOT REMOVED FROM SYSTEM!")
                    End If
                    Me.CloseForm()
                Else
                    Me.TrackWarrantEvent("DISPATCHER SELECTED NO TO VOID TRACK AUTHORITY NO. " & Me.ThisCurrentWarrant.WarrantNumber.ToString)
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try

    End Sub

    Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOK.Click
        Try
            Dim trn As TrainID = TrainExistsForWarrant(ThisCurrentWarrant)

            gHistoryMessage = "SELECTED OK FOR TRACK WARRANT:" & Me.txtTWnum.Text
            SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    Me.ThisCurrentWarrant.OKTime = Now.AddHours(1 + gThisWorkstationTimeFactor)
                Case "PACIFIC"
                    Me.ThisCurrentWarrant.OKTime = Now.AddHours(-2 + gThisWorkstationTimeFactor)
                Case "MOUNTAIN"
                    Me.ThisCurrentWarrant.OKTime = Now.AddHours(-1 + gThisWorkstationTimeFactor)
                Case Else
                    Me.ThisCurrentWarrant.OKTime = Now.AddHours(gThisWorkstationTimeFactor)

            End Select

            Me.ThisCurrentWarrant.UpdateOKTime()
            If gBoxOneRequired And Not VoidedWarrant Is Nothing Then
                Me.Hide()
                'Disable the track selection flash timer
                TMDS.SystemForms.MySystemForms.frmMainMDI.tmrSelectionFlasher.Enabled = False
                TMDS.SystemForms.MySystemForms.frmMainMDI.SelectionClear()

                'Version 237
                If optVoidWarrantConfirmationBox Then
                    If VoidTrackWarrant(VoidedWarrant, True, False) Then
                        gHistoryMessage = "VOID FOR TRACK WARRANT:" & Me.txtTWnum.Text
                        SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                    End If
                Else
                    If VoidTrackWarrant(VoidedWarrant, True, True) Then
                        gHistoryMessage = "VOID FOR TRACK WARRANT:" & Me.txtTWnum.Text
                        SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                    End If
                End If
            ElseIf Not gBoxOneRequired Then
                If ThisCurrentWarrant.WarrantType = "ItemTwoToTrain" Or ThisCurrentWarrant.WarrantType = "ItemFourToTrain" Then
                    If Not trn Is Nothing Then
                        Dim UserFeedBack As Integer
                        Dim UserMessage As String = "EXISTING TRAIN FOUND FOR TRACK WARRANT: " & trn.Name & vbCrLf
                        UserMessage = UserMessage & "MOVE TRAIN SYMBOL TO BEGINNING OF AUTHORITY?" & vbCrLf
                        UserFeedBack = MessageBox.Show(Me, UserMessage, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If UserFeedBack = 6 Then
                            Dim MoveFromTrack As Track = FindTrackItemFromGUID(trn.TrackGUID)
                            Dim MoveToTrack As Track = FindTrackItemFromGUID(gBeginTrackSelect.GUID)
                            MoveToTrack.MoveTrainIDPlace(trn, MoveFromTrack, True, TMDS.GlobalDeclarations.gDatabaseConnectionTrain)
                        End If
                    End If
                End If
            End If
            VoidedWarrant = Nothing
            Dim EntityUID As String
            If IsNothing(trn) Then
                EntityUID = ThisCurrentWarrant.IssueTo
            Else
                EntityUID = trn.Name
            End If
            If gPrintWarrantMode Then
                Dim PerformPrint As New DoPrintWarrant(AddressOf Me.PrintWarrant)
                PerformPrint.BeginInvoke(EntityUID, ThisCurrentWarrant.TerritoryAssignment, gBeginTrackSelect.SubdivisionID, Nothing, Nothing)
            ElseIf optPerformDocumentArchiving Then
                Dim docWarrant As New WarrantDocument()
                Dim FileName As String = docWarrant.BuildWarrantDocument(ThisCurrentWarrant)
                If System.IO.File.Exists(FileName) Then
                    TMDS.PrintForms.DocumentArchive.ArchiveTheDocument(FileName, "WARRANT", EntityUID, ThisCurrentWarrant.TerritoryAssignment, gBeginTrackSelect.SubdivisionID)
                End If
            End If

            ClearSystemVariables()
            Me.CloseForm()
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

#End Region
    'Private Function GetLimits(ByVal warr As Warrant) As String
    '    Dim Limits As String = String.Empty
    '    Try
    '        If warr.WarrantType = "ItemTwoToTrain" Or warr.WarrantType = "ItemTwoToMOW" Then
    '            Limits = "FROM " & warr.Box2From & vbCrLf & "TO " & warr.Box2To & vbCrLf & "ON " & warr.Box2Track & vbCrLf & vbCrLf
    '            If warr.CheckBox(3) Then
    '                Limits = Limits & "FROM " & warr.Box3From & vbCrLf & "TO " & warr.Box3To & vbCrLf & "ON " & warr.Box3Track
    '            End If
    '        Else
    '            If warr.Direction = "RIGHT" Then
    '                Limits = "BETWEEN " & warr.Box4From & vbCrLf & "AND " & warr.Box4To & vbCrLf & "ON " & warr.Box4Track
    '            Else
    '                Limits = "BETWEEN " & warr.Box4From & vbCrLf & "AND " & warr.Box4To & vbCrLf & "ON " & warr.Box4Track
    '            End If
    '        End If
    '        Return Limits
    '    Catch ex As Exception
    '        SystemException(ex.ToString)
    '        Return String.Empty
    '    End Try
    'End Function
    Private Function GetLimits(ByVal warr As Warrant) As String
        Dim Limits As String = String.Empty
        Try
            If warr.WarrantType = "ItemTwoToTrain" Or warr.WarrantType = "ItemTwoToMOW" Then
                Limits = "FROM " & warr.Box2From & vbCrLf & "TO " & warr.Box2To & vbCrLf & "ON " & warr.Box2Track & vbCrLf & vbCrLf
                If warr.CheckBox(3) Then
                    Limits = Limits & "FROM " & warr.Box3From & vbCrLf & "TO " & warr.Box3To & vbCrLf & "ON " & warr.Box3Track
                End If
            Else
                If warr.Direction = "RIGHT" Then
                    Limits = "BETWEEN " & warr.Box4From & vbCrLf & "AND " & warr.Box4To & vbCrLf & "ON " & warr.Box4Track
                Else
                    Limits = "BETWEEN " & warr.Box4From & vbCrLf & "AND " & warr.Box4To & vbCrLf & "ON " & warr.Box4Track
                End If
            End If
            Return Limits
        Catch ex As Exception
            SystemException(ex.ToString)
            Return String.Empty
        End Try

    End Function
    Private Function PerformSwitchPointConfirmationDME() As Boolean
        Dim FormPosition As New Point(Me.Left + 200, Me.Top + 400)
        Dim CurrentWarrantItem As Warrant = Me.VoidedWarrant
        Try
            If IsNothing(CurrentWarrantItem) Then
                Me.CloseForm()
                Exit Function
            End If

            Dim Box20Switches As String = String.Empty
            Dim str As String
            For Each str In CurrentWarrantItem.Box21
                Box20Switches = Box20Switches & " " & str
            Next

            Dim frmMessageBox1 As frmMessageBox
            Dim MessageList1 As New ArrayList()

            MessageList1.Add("Operations Supervisor will confirm with employee reporting clear of track warrant:".ToUpper)
            MessageList1.Add(txtIssueTo.Text)
            MessageList1.Add(" ")
            MessageList1.Add("SWITCH|lined, locked and checked for NORMAL position'CHECKBOX-WITH-TEXTBOX'".ToUpper)
            MessageList1.Add(" ")
            If CurrentWarrantItem.Box21.Count > 0 Then
                MessageList1.Add("SWITCH|lined, locked and checked for REVERSE position'CHECKBOX-WITH-TEXTBOX'".ToUpper & Box20Switches.ToUpper)
                MessageList1.Add(" ")
            End If
            MessageList1.Add("Clear OF Track Warrant: ".ToUpper & CurrentWarrantItem.WarrantNumber)
            MessageList1.Add(GetLimits(CurrentWarrantItem).Replace(vbCrLf, " "))
            MessageList1.Add("AT " & Format(Now, "HH:mm") & " " & gCurrentUser.Initials & " IS THAT CORRECT?")
            MessageList1.Add(" ")
            MessageList1.Add(" ")
            MessageList1.Add("Employee will confirm with Operations Supervisor:".ToUpper)
            MessageList1.Add(" ")
            MessageList1.Add("That is correct, CLEAR of Track Warrant ".ToUpper & CurrentWarrantItem.WarrantNumber & " AT " & Format(Now, "HH:mm") & " BY 'TEXTBOX'Occupation/Name".ToUpper)
            MessageList1.Add("")
            MessageList1.Add("Select YES to confirm that required Track Warrant Report Clear process has taken place.".ToUpper)

            frmMessageBox1 = New frmMessageBox()
            frmMessageBox1.Owner = Me
            frmMessageBox1.SetMessage(MessageList1, "REPORT CLEAR CONFIRMATION " & CurrentWarrantItem.WarrantNumber, True, FormPosition)
            frmMessageBox1.ShowDialog()
            If frmMessageBox1.FormFeedBack <> 6 Then
                frmMessageBox1.Close()
                frmMessageBox1.Dispose()
                Return False
            End If
            frmMessageBox1.Close()
            frmMessageBox1.Dispose()
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
            Return False
        End Try

    End Function

    Private Function PerformSwitchPointConfirmation() As Boolean
        Try
            Dim SwitchMessage As String = "HAVE ALL MAIN TRACK SWITCHES THAT YOU HAVE OPERATED BEEN RESTORED TO THE NORMAL POSITION?"
            Dim UserResponseSwitch As Integer = MessageBox.Show(Me, SwitchMessage, "SWITCH POSITION CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3)
            If UserResponseSwitch = 6 Then
                gHistoryMessage = "SELECTED - YES TO " & SwitchMessage '& ":" & WarrantToVoid.WarrantNumber.ToString
                TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
            Else
                gHistoryMessage = "SELECTED - NO TO  " & SwitchMessage '& ":" & WarrantToVoid.WarrantNumber.ToString
                TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                Return False
            End If

            If Not optDisablePromptSPAF Then
                Dim SwitchFormMessage As String = "HAVE ALL EMPLOYEES COMPLETED THE SPAF AS REQUIRED?"
                Dim UserResponseSwitchForm As Integer = MessageBox.Show(Me, SwitchFormMessage, "SWITCH POSITION FORM CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3)
                If UserResponseSwitchForm = 6 Then
                    gHistoryMessage = "SELECTED - YES TO " & SwitchFormMessage '& ":" & WarrantToVoid.WarrantNumber.ToString
                    TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                Else
                    gHistoryMessage = "SELECTED - NO TO  " & SwitchFormMessage '& ":" & WarrantToVoid.WarrantNumber.ToString
                    TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                    Return False
                End If
            End If
            Return True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Function

    Private Sub ValidateInputBoxes()
        Dim i As Integer
        Try
            If gRepeatMode Then

                'Check the text box inputs to insure validation complete

                If txtTWnum.Tag = "VALIDATE" Then Exit Sub
                If txtSubdivision.Tag = "VALIDATE" Then Exit Sub
                If txtDate.Tag = "VALIDATE" Then Exit Sub
                If txtIssueTo.Tag = "VALIDATE" Then Exit Sub
                If txtAtLocation.Tag = "VALIDATE" Then Exit Sub

                'Check the text box inputs to insure validation complete
                For i = 1 To 43
                    If TextBoxArrayList(i).Tag = "VALIDATE" Then Exit Sub
                Next i


                'Check the check box inputs to insure validation complete
                For i = 1 To 21
                    If CheckBoxArrayList(i).Tag = "VALIDATE" Then
                        Exit Sub
                    End If
                Next i

                'Make the Footer visible and fill in the blanks and make the fields be validated
                gbFooter.Enabled = True
                gbFooter.Visible = True
                txtOKTIME.Text = Format(Now, "HH:mm")
                txtOKTIME.ReadOnly = True
                txtOKTIME.Tag = "VALIDATE"
                txtDISP.Tag = "VALIDATE"

            End If

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try
    End Sub

    Private Sub ValidateOKTIME()
        Dim UserResponse As Integer, VerifyMessage As String
        Try
            If txtOKTIME.Tag = "VALIDATE" Then Exit Sub
            If txtDISP.Tag = "VALIDATE" Then Exit Sub


            If gItemSixRequired Then
                If optMOWEnterBehindTrainDialogBox Then
                    VerifyMessage = "VERIFIED THAT " & cbIssueTo.Text & " HAS OR WILL SET ON TRACK BEHIND " & TextBoxArrayList(13).Text
                    UserResponse = MessageBox.Show(Me, VerifyMessage, "SET ON TRACK BEHIND VERIFICATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                    'UserResponse = MsgBox(VerifyMessage, vbYesNo, "SET ON TRACK BEHIND VERIFICATION")
                    If UserResponse = 7 Then
                        gHistoryMessage = "SELECTED NO TO > " & VerifyMessage
                        SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                    Else
                        gHistoryMessage = "SELECTED YES TO > " & VerifyMessage
                        SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                    End If
                End If
                btnOK.Enabled = True
                btnOK.Focus()
            Else

                btnOK.Enabled = True
                btnOK.Focus()
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.Dispose()
        End Try

    End Sub

    Private Sub txtOkTime_Enter(ByVal sender As Object, ByVal e As EventArgs) Handles txtOKTIME.Enter
        Try
            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    txtOKTIME.Text = Format(Now.AddHours(1 + gThisWorkstationTimeFactor), "HH:mm")
                Case "PACIFIC"
                    txtOKTIME.Text = Format(Now.AddHours(-2 + gThisWorkstationTimeFactor), "HH:mm")
                Case "MOUNTAIN"
                    txtOKTIME.Text = Format(Now.AddHours(-1 + gThisWorkstationTimeFactor), "HH:mm")
                Case Else
                    txtOKTIME.Text = Format(Now, "HH:mm")
            End Select
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub txtOkTime_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtOKTIME.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    sender.ForeColor = Color.DarkGreen
                    sender.Tag = "VALIDATED"
                    ValidateOKTIME()
                    SendKeys.Send("{TAB}")

                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

    Private Sub txtDISP_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles txtDISP.KeyDown
        Try
            If gRepeatMode Then
                If e.KeyValue = Keys.Enter Then
                    sender.ForeColor = Color.DarkGreen
                    sender.Tag = "VALIDATED"
                    ValidateOKTIME()
                End If
            End If
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try

    End Sub

    Private Delegate Sub DoPrintWarrant(ByVal EntityUID As String, ByVal TerrUID As Int32, ByVal SubUID As Int32)

    Private Sub PrintWarrant(ByVal EntityUID As String, ByVal TerrUID As Int32, ByVal SubUID As Int32)
        Try
            Dim PrintWarrant As New Warrant()
            PrintWarrant = ThisCurrentWarrant
            Dim docWarrant As New WarrantDocument()
            Dim FileName As String = docWarrant.BuildWarrantDocument(PrintWarrant)
            If System.IO.File.Exists(FileName) Then
                Dim startInfo As New ProcessStartInfo(FileName)
                startInfo.UseShellExecute = True
                Process.Start(startInfo)
                If optPerformDocumentArchiving Then
                    TMDS.PrintForms.DocumentArchive.ArchiveTheDocument(FileName, "WARRANT", EntityUID, TerrUID, SubUID)
                End If

            End If
            docWarrant = Nothing
            PrintThread = Nothing
            PrintWarrant = Nothing

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Public Function FindTrackItemFromGUID(ByVal UniqueID As Int32) As Track
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

    Private Function TrainExistsForWarrant(ByVal warr As Warrant) As TrainID
        Dim trn As TrainID
        Dim IDEngine As String
        Try
            For Each trn In TrainCollection
                IDEngine = trn.IdentifingUnit & " " & trn.Direction
                If IDEngine = warr.IssueTo Then
                    Return trn
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try

    End Function

    Private Function GetGeneralOrder(ByVal SubdivisionName As String) As String
        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim query As StringBuilder = Nothing
        Dim GeneralOrder As String = String.Empty
        Try
            query = New StringBuilder("SELECT GeneralOrder FROM " & gtblSubdivisions & " WHERE LongName = " & "'" & SubdivisionName & "'")
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)

            dr = dbcomm.ExecuteReader()
            While dr.Read()
                GeneralOrder = dr.GetString(0)
            End While
            dr.Close()
            dbconn.Close()
            dbconn = Nothing
            Return GeneralOrder
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return String.Empty
        End Try

    End Function

    Private Function GetCTO(ByVal SubdivisionName As String) As String

        Dim dbconn As OleDbConnection = Nothing
        Dim dbcomm As OleDbCommand = Nothing
        Dim dr As OleDbDataReader = Nothing
        Dim query As StringBuilder = Nothing
        Dim CTO As String = String.Empty
        Try
            query = New StringBuilder("SELECT CTO FROM " & gtblSubdivisions & " WHERE LongName = " & "'" & SubdivisionName & "'")
            dbconn = New OleDbConnection(gConnectionStringStatic)
            dbconn.Open()
            dbcomm = New OleDbCommand(query.ToString, dbconn)

            dr = dbcomm.ExecuteReader()
            While dr.Read()
                CTO = dr.GetString(0)
            End While
            dr.Close()
            dbconn.Close()
            dbconn = Nothing
            Return CTO
        Catch ex As Exception
            SystemException(ex.ToString)
            dbconn = Nothing
            Return String.Empty
        End Try

    End Function

    Private Overloads Sub LoadDatabaseField(ByVal LoadObject As Object, ByVal SQL As String, ByVal Direction As Boolean)
        Dim ResultString As String = String.Empty
        Try
            Dim DatabaseCommand As OleDb.OleDbCommand
            Dim DataReader As OleDb.OleDbDataReader
            DatabaseCommand = New OleDbCommand(SQL, TMDS.GlobalDeclarations.gDatabaseConnectionDynamic)
            If TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.State <> ConnectionState.Open Then
                TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Open()
            End If
            DataReader = DatabaseCommand.ExecuteReader()

            While DataReader.Read()
                If Direction Then
                    If Not DataReader.IsDBNull(0) And Not DataReader.IsDBNull(1) Then
                        ResultString = DataReader.GetString(0) & " " & DataReader.GetString(1)
                    End If
                Else
                    ResultString = DataReader.GetString(1)
                End If
                If Not LoadObject.Items.contains(ResultString) Then
                    LoadObject.Items.Add(ResultString)
                End If
            End While
            DataReader.Close()
            TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Close()
            DatabaseCommand.Dispose()
            DatabaseCommand = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Close()
            Me.CloseForm()
        End Try
    End Sub

    Private Overloads Sub LoadDatabaseField(ByVal LoadObject As Object, ByVal SQL As String)
        Dim ResultString As String
        Try
            Dim DatabaseCommand As OleDb.OleDbCommand
            Dim DataReader As OleDb.OleDbDataReader
            DatabaseCommand = New OleDbCommand(SQL, TMDS.GlobalDeclarations.gDatabaseConnectionDynamic)
            TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Open()
            DataReader = DatabaseCommand.ExecuteReader()

            While DataReader.Read()
                ResultString = DataReader.GetString(0)
                If Not LoadObject.Items.contains(ResultString) Then
                    LoadObject.Items.Add(ResultString)
                End If
            End While
            DataReader.Close()
            TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Close()
            DatabaseCommand.Dispose()
            DatabaseCommand = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            'ClearSystemVariables()
            'TMDS.GlobalDeclarations.gDatabaseConnectionDynamic.Close()
            'Me.CloseForm()
        End Try
    End Sub

    Public Sub FindCurrentTerritory()

        Dim CurrentTerritoryID As Integer = gBeginTrackSelect.TerritoryID
        Dim terr As Territory
        Try
            For Each terr In ControlledTerritories
                If terr.TerritoryID = CurrentTerritoryID Then
                    CurrentSelectedTerritory = terr
                    Exit For
                End If
            Next
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

    Private Function VoidTrackWarrant(ByVal WarrantToVoid As Warrant, ByVal BoxOneVoid As Boolean, ByVal ForceWarrantDeletion As Boolean) As Boolean
        Dim LongMessage(7), Limits As String, UserMessage As Integer
        Dim I As Integer
        Dim gHistoryMessage As String
        Try
            If Not ForceWarrantDeletion Then 'Version 237
                tmrSelection.Enabled = True

                If WarrantToVoid.WarrantType = "ItemTwoToTrain" Or WarrantToVoid.WarrantType = "ItemTwoToMOW" Then
                    Limits = "FROM " & WarrantToVoid.Box2From & vbCrLf & "TO " & WarrantToVoid.Box2To & vbCrLf & "ON " & WarrantToVoid.Box2Track
                Else
                    Limits = "BETWEEN " & WarrantToVoid.Box4From & vbCrLf & "AND " & WarrantToVoid.Box4To & vbCrLf & "ON " & WarrantToVoid.Box4Track
                End If

                If Not BoxOneVoid Then
                    LongMessage(1) = "VOID TRACK WARRANT NUMBER: " & WarrantToVoid.WarrantNumber.ToString
                    LongMessage(2) = "LIMITS: " & Limits
                    LongMessage(3) = "ISSUED TO: " & WarrantToVoid.IssueTo
                    LongMessage(4) = "Report Clear Time: " & Format(Now, "MM-dd-yyy HH:mm")
                    LongMessage(5) = "BY: " & WarrantToVoid.CopiedBy & vbCrLf
                    LongMessage(6) = "ARE YOU SURE?" & vbCrLf
                Else
                    LongMessage(1) = "ARE YOU SURE?" & vbCrLf & vbCrLf
                    LongMessage(2) = "RELEASE TRACK WARRANT NUMBER: " & WarrantToVoid.WarrantNumber.ToString & vbCrLf
                    LongMessage(3) = "LIMITS: " & Limits & vbCrLf
                    LongMessage(4) = "ISSUED TO: " & WarrantToVoid.IssueTo
                End If

                For I = 1 To 6
                    LongMessage(7) = LongMessage(7) & LongMessage(I) & vbCrLf
                    gHistoryMessage = LongMessage(I)
                Next I

                UserMessage = MessageBox.Show(Me, LongMessage(7), "REPORT LIMITS CLEAR CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)


                If UserMessage = 6 Then
                    tmrSelection.Enabled = False

                    gHistoryMessage = "SELECTED - YES ON THE REPORT LIMITS CLEAR ON TRACK WARRANT:" & WarrantToVoid.WarrantNumber.ToString
                    TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                    WarrantToVoid.ReportClearTime = Format(Now, "g")
                    WarrantToVoid.ReportedClearBy = WarrantToVoid.CopiedBy

                    'added this code to move the train id to new track warrant that replaces the item one warrant
                    Dim trn As TrainID = TrainExistsForWarrant(WarrantToVoid)
                    If Not trn Is Nothing Then
                        Dim UserFeedBack As Integer
                        Dim UserMessageOne As String = "EXISTING TRAIN FOUND FOR TRACK WARRANT: " & trn.Name & vbCrLf
                        UserMessageOne = UserMessageOne & "MOVE TRAIN SYMBOL TO BEGINNING OF AUTHORITY?" & vbCrLf
                        UserFeedBack = MessageBox.Show(Me, UserMessageOne, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If UserFeedBack = 6 Then
                            Dim MoveFromTrack As Track = FindTrackItemFromGUID(trn.TrackGUID)
                            Dim MoveToTrack As Track = FindTrackItemFromGUID(gBeginTrackSelect.GUID)
                            MoveToTrack.MoveTrainIDPlace(trn, MoveFromTrack, True, TMDS.GlobalDeclarations.gDatabaseConnectionTrain)
                        End If
                    End If

                    If WarrantToVoid.RemoveActiveTrackWarrantFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        If WarrantToVoid.RemoveActiveTrackWarrantFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                            TrackWarrants.Remove(WarrantToVoid)
                            TMDS.SystemForms.MySystemForms.frmMainMDI.UpdateAuthoritySummary()
                        Else
                            gHistoryMessage = "Track Warrant Number " & WarrantToVoid.WarrantNumber.ToString & " NOT DELETE FROM THE SYSTEM."
                            TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                            'SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                            MessageBox.Show(MainForm, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            ClearSelection()
                        End If
                    Else
                        gHistoryMessage = "Track Warrant Number " & WarrantToVoid.WarrantNumber.ToString & " NOT DELETED FROM THE DATABASE."
                        TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                        'SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                        MessageBox.Show(MainForm, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                    ClearSelection()
                    Return True
                Else
                    gHistoryMessage = "SELECTED - NO TO REPORT LIMITS CLEAR ON TRACK WARRANT:" & WarrantToVoid.WarrantNumber.ToString
                    TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                    ClearSelection()
                    Return False
                    'ClearSelection()
                    'gHistoryMessage = "SELECTED - NO ON THE REPORT LIMITS CLEAR"
                    ''DisplaygHistoryMessage(vbBlack, True)
                End If
                'Version 237
            Else

                'added this code to move the train id to new track warrant that replaces the item one warrant
                Dim trn As TrainID = TrainExistsForWarrant(WarrantToVoid)
                If Not trn Is Nothing Then
                    Dim UserFeedBack As Integer
                    Dim UserMessageOne As String = "EXISTING TRAIN FOUND FOR TRACK WARRANT: " & trn.Name & vbCrLf
                    UserMessageOne = UserMessageOne & "MOVE TRAIN SYMBOL TO BEGINNING OF AUTHORITY?" & vbCrLf
                    UserFeedBack = MessageBox.Show(Me, UserMessageOne, "System Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    If UserFeedBack = 6 Then
                        Dim MoveFromTrack As Track = FindTrackItemFromGUID(trn.TrackGUID)
                        Dim MoveToTrack As Track = FindTrackItemFromGUID(gBeginTrackSelect.GUID)
                        MoveToTrack.MoveTrainIDPlace(trn, MoveFromTrack, True, TMDS.GlobalDeclarations.gDatabaseConnectionTrain)
                    End If
                End If

                If WarrantToVoid.RemoveActiveTrackWarrantFromDB(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                    If WarrantToVoid.RemoveActiveTrackWarrantFromComponents(TMDS.GlobalDeclarations.gDatabaseConnectionAuthority) Then
                        TrackWarrants.Remove(WarrantToVoid)
                        TMDS.SystemForms.MySystemForms.frmMainMDI.UpdateAuthoritySummary()
                    Else
                        gHistoryMessage = "Track Warrant Number " & WarrantToVoid.WarrantNumber.ToString & " NOT DELETE FROM THE SYSTEM."
                        TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                        'SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                        MessageBox.Show(MainForm, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        ClearSelection()
                    End If
                Else
                    gHistoryMessage = "Track Warrant Number " & WarrantToVoid.WarrantNumber.ToString & " NOT DELETED FROM THE DATABASE."
                    TMDS.SystemUtilities.Utilities.SystemEvent(gHistoryMessage)
                    'SysEvent = New SystemEventRecord(Now, SystemEventRecord.EventEnum.DispatcherEvent, gHistoryMessage, SystemEventQueue)
                    MessageBox.Show(MainForm, gHistoryMessage, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
                VoidTrackWarrant = True
            End If
            'Version 237
        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSelection()
            VoidedWarrant = Nothing
            ClearSystemVariables()
            Me.CloseForm()
        Finally
            VoidedWarrant = Nothing
        End Try
    End Function

    Private Sub tmrSelection_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrSelection.Tick
        Dim comp As Object

        Try
            If TrackSelectionFlash Then
                TrackSelectionFlash = False
            Else
                TrackSelectionFlash = True
            End If

            For Each comp In VoidedWarrant.SelectionComponents
                SyncLock comp
                    Select Case comp.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch", "TMDS.TrackComponents.Track"
                            comp.SelectionMode = GraphicComponent.GenericComponent.ModeSelection.SummarySelectionBlocking
                            comp.FlashComponent = True
                            comp.FlashOn = TrackSelectionFlash
                            comp.Refresh()
                    End Select

                End SyncLock
            Next
            tmrSelection.Enabled = True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub ClearSelection()
        Dim comp As Object
        Try
            tmrSelection.Enabled = False

            For Each comp In VoidedWarrant.SelectionComponents
                SyncLock comp
                    Select Case comp.GetType.ToString
                        Case "TMDS.SwitchComponents.Switch", "TMDS.TrackComponents.Track"
                            comp.SelectionMode = GraphicComponent.GenericComponent.ModeSelection.Regular
                            comp.FlashComponent = False
                            comp.FlashOn = False
                            comp.Refresh()
                    End Select
                End SyncLock
            Next
            VoidedWarrant = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Sub cbIssueTo_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbIssueTo.Leave
        Try
            cbIssueTo.Text = FormatProperString(cbIssueTo.Text)
            CheckToEnableRepeatButton()
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub txtSub_Focus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSubdivision.Enter
        SendKeys.Send("{Home}+{End}")
    End Sub

    Private Sub txtTWnum_Focus(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTWnum.Enter
        Try
            If Not gRepeatMode Then
                If cbIssueTo.Enabled = False Then
                    Me.btnRepeat.Enabled = True
                End If
            End If
            SendKeys.Send("{Home}+{End}")

        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Function FindObjectTypeFromGUID(ByVal UniqueID As Int32) As Object
        'This function finds the current trackcomponent index from the global unique ID
        Dim I As Integer
        For I = 1 To UBound(TrackComponent)
            If TrackComponent(I).GUID = UniqueID Then
                Return TrackComponent(I)
            End If
        Next I

        For I = 1 To UBound(SwitchComponent)
            If SwitchComponent(I).GUID = UniqueID Then
                Return SwitchComponent(I)
            End If
        Next I
        Return Nothing
    End Function

    Private Sub FrmTrackWarrant_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Location = FrmTrackWarrantProp.FormLocation
    End Sub

    Public Sub CloseForm()
        Try
            ClearFormValues()
            TrackWarrantInUse = False
            TMDS.ManagementClasses.FormPropertyUpdate.SetFormPosition(Me, "FrmTrackWarrantProp", FrmTrackWarrantProp)
            'Me.Hide()
            Me.Close()
            Me.Dispose(True)
            TMDS.SystemForms.MySystemForms.frmTrackWarrant = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Me.Dispose()
            SystemForms.MySystemForms.frmTrackWarrant = Nothing
        End Try

    End Sub

    Private Sub FrmTrackWarrant_Closed(ByVal sender As Object, ByVal e As CancelEventArgs) Handles MyBase.Closing
        Try
            TMDS.ManagementClasses.FormPropertyUpdate.SetFormPosition(Me, "FrmTrackWarrantProp", FrmTrackWarrantProp)
            'ClearFormValues()
            'Me.Hide()
        Catch ex As Exception
            SystemException(ex.ToString)
            Me.Dispose()
            SystemForms.MySystemForms.frmTrackWarrant = Nothing
        End Try

    End Sub

    Private Sub FrmTrackWarrant_Activated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        SendKeys.Send("{TAB}")
    End Sub

    Private Sub TrackWarrantEvent(ByVal EventNote As String)

        Try
            SystemEvent(EventNote)
            'ClickEvent = Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try

    End Sub

    Private Sub CheckToEnableRepeatButton()
        Dim I As Integer
        Try
            If Trim(cbIssueTo.Text) = "" Then cbIssueTo.Focus() : Me.btnRepeat.Enabled = False : Exit Sub
            For I = 1 To 40
                Select Case I
                    Case 1, 2, 3, 4, 8, 9, 10, 11, 12, 13, 14, 15, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39
                        If TextBoxArrayList(I).Enabled Then
                            If Trim(TextBoxArrayList(I).Text) = "" Then Me.btnRepeat.Enabled = False : Exit Sub
                        End If
                End Select
            Next I
            Me.btnRepeat.Enabled = True
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

    Private Function FindSubDivisionDirection(ByVal SubDivisionName As String) As Boolean
        Try
            Dim subdvn As Subdivision
            For Each subdvn In ControlledSubdivisions
                If subdvn.LongName = SubDivisionName Then
                    FindSubDivisionDirection = subdvn.SubDirection
                    Exit Function
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try

    End Function

    Private Function FindSubDivision(ByVal SubDivisionName As String) As Subdivision
        Try
            Dim subdvn As Subdivision
            For Each subdvn In ControlledSubdivisions
                If subdvn.LongName = SubDivisionName Then
                    Return subdvn
                End If
            Next
            Return Nothing
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FormatProperString(ByVal strToFormat As String) As String
        Dim i As Integer
        Dim ch As String
        Dim sFormatProperString As String = String.Empty
        Try
            For i = 1 To strToFormat.Length
                ch = Mid(strToFormat, i, 1)
                Select Case ch
                    Case "'", "=", "-", "*", "#", "@", "!", "_", "+", ")", "(", "^", "%", "$", "~", "`", Chr(34), ";", ":", "/", "?", "\", "|", ">", "<", "[", "]", "{", "}"
                        sFormatProperString = sFormatProperString & " "
                    Case "&"
                        sFormatProperString = sFormatProperString & "AND"
                    Case Else
                        sFormatProperString = sFormatProperString & ch
                End Select
            Next
            Return sFormatProperString
        Catch ex As Exception
            SystemException(ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Sub cbAT_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbAT.Leave
        Try
            cbAT.Text = FormatProperString(cbAT.Text)
        Catch
        End Try
    End Sub

    Private Sub cmbBox11From_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBox11From.SelectedIndexChanged
        Try
            If Not Me.CheckBoxElevenChange(Me.CompleteCompSelection, Me.DirectionOfTheWarrant) Then
                ClearSystemVariables()
                Me.CloseForm()
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try
    End Sub

    Private Sub cmbBox11To_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBox11To.SelectedIndexChanged
        Try
            If Not Me.CheckBoxElevenChange(Me.CompleteCompSelection, Me.DirectionOfTheWarrant) Then
                ClearSystemVariables()
                Me.CloseForm()
            End If

        Catch ex As Exception
            SystemException(ex.ToString)
            ClearSystemVariables()
            Me.CloseForm()
        End Try

    End Sub

    Private Sub tmrDateCheck_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrDateCheck.Tick
        Try
            Select Case Me.CurrentTimeZone
                Case "EASTERN"
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(1 + gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(1 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")
                Case "PACIFIC"
                    Dim TimeDiff As Integer = (-2 + gThisWorkstationTimeFactor)
                    Me.lblCreateTime.Text = Format(Now.AddHours(TimeDiff), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(-2 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")

                Case "MOUNTAIN"
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(-1 + gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.AddHours(CDbl(-1 + gThisWorkstationTimeFactor)), "dddd, MMM d yyyy")

                Case Else
                    Me.lblCreateTime.Text = Format(Now.AddHours(CDbl(gThisWorkstationTimeFactor)), "HH:mm")
                    Me.lblCreateDate.Text = Format(Now.Date, "dddd, MMM d yyyy")
            End Select
            Me.txtDate.Text = Format(CDate(Me.lblCreateDate.Text), "MMM. d, yyyy")
            tmrDateCheck.Enabled = True

        Catch ex As Exception
            SystemException(ex.ToString)
            tmrDateCheck.Enabled = True
        End Try
    End Sub
End Class
