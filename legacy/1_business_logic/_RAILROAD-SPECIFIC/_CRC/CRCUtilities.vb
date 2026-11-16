
Public Class CRCUtilities
    Public Shared usCRC(255) As Long
    Public Shared crctbl(255) As Long

    Function Crc16(ByVal cp() As Byte, Optional ByVal Size As Long = -1) As Long
        Dim i As Long
        Dim fcs As Long
        Static fcstab(255) As Long

        Const pppinitfcs16 As Long = &HFF& 'Initial FCS value

        If Size < 0 Then Size = UBound(cp) - LBound(cp) + 1

        If fcstab(1) = 0 Then
            ' Initialize array once and for all
            fcstab(0) = &H0&
            fcstab(1) = &H1189&
            fcstab(2) = &H2312&
            fcstab(3) = &H329B&
            fcstab(4) = &H4624&
            fcstab(5) = &H57AD&
            fcstab(6) = &H6536&
            fcstab(7) = &H74BF&
            fcstab(8) = &H8C48&
            fcstab(9) = &H9DC1&
            fcstab(10) = &HAF5A&
            fcstab(11) = &HBED3&
            fcstab(12) = &HCA6C&
            fcstab(13) = &HDBE5&
            fcstab(14) = &HE97E&
            fcstab(15) = &HF8F7&
            fcstab(16) = &H1081&
            fcstab(17) = &H108&
            fcstab(18) = &H3393&
            fcstab(19) = &H221A&
            fcstab(20) = &H56A5&
            fcstab(21) = &H472C&
            fcstab(22) = &H75B7&
            fcstab(23) = &H643E&
            fcstab(24) = &H9CC9&
            fcstab(25) = &H8D40&
            fcstab(26) = &HBFDB&
            fcstab(27) = &HAE52&
            fcstab(28) = &HDAED&
            fcstab(29) = &HCB64&
            fcstab(30) = &HF9FF&
            fcstab(31) = &HE876&
            fcstab(32) = &H2102&
            fcstab(33) = &H308B&
            fcstab(34) = &H210&
            fcstab(35) = &H1399&
            fcstab(36) = &H6726&
            fcstab(37) = &H76AF&
            fcstab(38) = &H4434&
            fcstab(39) = &H55BD&
            fcstab(40) = &HAD4A&
            fcstab(41) = &HBCC3&
            fcstab(42) = &H8E58&
            fcstab(43) = &H9FD1&
            fcstab(44) = &HEB6E&
            fcstab(45) = &HFAE7&
            fcstab(46) = &HC87C&
            fcstab(47) = &HD9F5&
            fcstab(48) = &H3183&
            fcstab(49) = &H200A&
            fcstab(50) = &H1291&
            fcstab(51) = &H318&
            fcstab(52) = &H77A7&
            fcstab(53) = &H662E&
            fcstab(54) = &H54B5&
            fcstab(55) = &H453C&
            fcstab(56) = &HBDCB&
            fcstab(57) = &HAC42&
            fcstab(58) = &H9ED9&
            fcstab(59) = &H8F50&
            fcstab(60) = &HFBEF&
            fcstab(61) = &HEA66&
            fcstab(62) = &HD8FD&
            fcstab(63) = &HC974&
            fcstab(64) = &H4204&
            fcstab(65) = &H538D&
            fcstab(66) = &H6116&
            fcstab(67) = &H709F&
            fcstab(68) = &H420&
            fcstab(69) = &H15A9&
            fcstab(70) = &H2732&
            fcstab(71) = &H36BB&
            fcstab(72) = &HCE4C&
            fcstab(73) = &HDFC5&
            fcstab(74) = &HED5E&
            fcstab(75) = &HFCD7&
            fcstab(76) = &H8868&
            fcstab(77) = &H99E1&
            fcstab(78) = &HAB7A&
            fcstab(79) = &HBAF3&
            fcstab(80) = &H5285&
            fcstab(81) = &H430C&
            fcstab(82) = &H7197&
            fcstab(83) = &H601E&
            fcstab(84) = &H14A1&
            fcstab(85) = &H528&
            fcstab(86) = &H37B3&
            fcstab(87) = &H263A&
            fcstab(88) = &HDECD&
            fcstab(89) = &HCF44&
            fcstab(90) = &HFDDF&
            fcstab(91) = &HEC56&
            fcstab(92) = &H98E9&
            fcstab(93) = &H8960&
            fcstab(94) = &HBBFB&
            fcstab(95) = &HAA72&
            fcstab(96) = &H6306&
            fcstab(97) = &H728F&
            fcstab(98) = &H4014&
            fcstab(99) = &H519D&
            fcstab(100) = &H2522&
            fcstab(101) = &H34AB&
            fcstab(102) = &H630&
            fcstab(103) = &H17B9&
            fcstab(104) = &HEF4E&
            fcstab(105) = &HFEC7&
            fcstab(106) = &HCC5C&
            fcstab(107) = &HDDD5&
            fcstab(108) = &HA96A&
            fcstab(109) = &HB8E3&
            fcstab(110) = &H8A78&
            fcstab(111) = &H9BF1&
            fcstab(112) = &H7387&
            fcstab(113) = &H620E&
            fcstab(114) = &H5095&
            fcstab(115) = &H411C&
            fcstab(116) = &H35A3&
            fcstab(117) = &H242A&
            fcstab(118) = &H16B1&
            fcstab(119) = &H738&
            fcstab(120) = &HFFCF&
            fcstab(121) = &HEE46&
            fcstab(122) = &HDCDD&
            fcstab(123) = &HCD54&
            fcstab(124) = &HB9EB&
            fcstab(125) = &HA862&
            fcstab(126) = &H9AF9&
            fcstab(127) = &H8B70&
            fcstab(128) = &H8408&
            fcstab(129) = &H9581&
            fcstab(130) = &HA71A&
            fcstab(131) = &HB693&
            fcstab(132) = &HC22C&
            fcstab(133) = &HD3A5&
            fcstab(134) = &HE13E&
            fcstab(135) = &HF0B7&
            fcstab(136) = &H840&
            fcstab(137) = &H19C9&
            fcstab(138) = &H2B52&
            fcstab(139) = &H3ADB&
            fcstab(140) = &H4E64&
            fcstab(141) = &H5FED&
            fcstab(142) = &H6D76&
            fcstab(143) = &H7CFF&
            fcstab(144) = &H9489&
            fcstab(145) = &H8500&
            fcstab(146) = &HB79B&
            fcstab(147) = &HA612&
            fcstab(148) = &HD2AD&
            fcstab(149) = &HC324&
            fcstab(150) = &HF1BF&
            fcstab(151) = &HE036&
            fcstab(152) = &H18C1&
            fcstab(153) = &H948&
            fcstab(154) = &H3BD3&
            fcstab(155) = &H2A5A&
            fcstab(156) = &H5EE5&
            fcstab(157) = &H4F6C&
            fcstab(158) = &H7DF7&
            fcstab(159) = &H6C7E&
            fcstab(160) = &HA50A&
            fcstab(161) = &HB483&
            fcstab(162) = &H8618&
            fcstab(163) = &H9791&
            fcstab(164) = &HE32E&
            fcstab(165) = &HF2A7&
            fcstab(166) = &HC03C&
            fcstab(167) = &HD1B5&
            fcstab(168) = &H2942&
            fcstab(169) = &H38CB&
            fcstab(170) = &HA50&
            fcstab(171) = &H1BD9&
            fcstab(172) = &H6F66&
            fcstab(173) = &H7EEF&
            fcstab(174) = &H4C74&
            fcstab(175) = &H5DFD&
            fcstab(176) = &HB58B&
            fcstab(177) = &HA402&
            fcstab(178) = &H9699&
            fcstab(179) = &H8710&
            fcstab(180) = &HF3AF&
            fcstab(181) = &HE226&
            fcstab(182) = &HD0BD&
            fcstab(183) = &HC134&
            fcstab(184) = &H39C3&
            fcstab(185) = &H284A&
            fcstab(186) = &H1AD1&
            fcstab(187) = &HB58&
            fcstab(188) = &H7FE7&
            fcstab(189) = &H6E6E&
            fcstab(190) = &H5CF5&
            fcstab(191) = &H4D7C&
            fcstab(192) = &HC60C&
            fcstab(193) = &HD785&
            fcstab(194) = &HE51E&
            fcstab(195) = &HF497&
            fcstab(196) = &H8028&
            fcstab(197) = &H91A1&
            fcstab(198) = &HA33A&
            fcstab(199) = &HB2B3&
            fcstab(200) = &H4A44&
            fcstab(201) = &H5BCD&
            fcstab(202) = &H6956&
            fcstab(203) = &H78DF&
            fcstab(204) = &HC60&
            fcstab(205) = &H1DE9&
            fcstab(206) = &H2F72&
            fcstab(207) = &H3EFB&
            fcstab(208) = &HD68D&
            fcstab(209) = &HC704&
            fcstab(210) = &HF59F&
            fcstab(211) = &HE416&
            fcstab(212) = &H90A9&
            fcstab(213) = &H8120&
            fcstab(214) = &HB3BB&
            fcstab(215) = &HA232&
            fcstab(216) = &H5AC5&
            fcstab(217) = &H4B4C&
            fcstab(218) = &H79D7&
            fcstab(219) = &H685E&
            fcstab(220) = &H1CE1&
            fcstab(221) = &HD68&
            fcstab(222) = &H3FF3&
            fcstab(223) = &H2E7A&
            fcstab(224) = &HE70E&
            fcstab(225) = &HF687&
            fcstab(226) = &HC41C&
            fcstab(227) = &HD595&
            fcstab(228) = &HA12A&
            fcstab(229) = &HB0A3&
            fcstab(230) = &H8238&
            fcstab(231) = &H93B1&
            fcstab(232) = &H6B46&
            fcstab(233) = &H7ACF&
            fcstab(234) = &H4854&
            fcstab(235) = &H59DD&
            fcstab(236) = &H2D62&
            fcstab(237) = &H3CEB&
            fcstab(238) = &HE70&
            fcstab(239) = &H1FF9&
            fcstab(240) = &HF78F&
            fcstab(241) = &HE606&
            fcstab(242) = &HD49D&
            fcstab(243) = &HC514&
            fcstab(244) = &HB1AB&
            fcstab(245) = &HA022&
            fcstab(246) = &H92B9&
            fcstab(247) = &H8330&
            fcstab(248) = &H7BC7&
            fcstab(249) = &H6A4E&
            fcstab(250) = &H58D5&
            fcstab(251) = &H495C&
            fcstab(252) = &H3DE3&
            fcstab(253) = &H2C6A&
            fcstab(254) = &H1EF1&
            fcstab(255) = &HF78&
        End If

        ' The initial FCS value
        fcs = pppinitfcs16

        ' evaluate the FCS
        For i = LBound(cp) To LBound(cp) + Size - 1
            fcs = (fcs \ &H100&) Xor fcstab((fcs Xor cp(i)) And &HFF&)
        Next i

        ' return the result
        Crc16 = fcs
    End Function

    Public Overloads Shared Function mbus_CalcCRC16(ByVal nBuf() As Byte) As Long
        Try
            Dim x As Long
            Dim crc As Long
            Dim i As Integer

            If (usCRC(1) <> &HC0C1) Then
                mbus_FillCRCTable()
            End If

            crc = 65535                     ' start with all 1's for a reverse CRC
            For i = 0 To nBuf.Length - 1              ' process each character in the message */
                x = nBuf(i)                 ' get next char
                x = (crc Xor x) And 255     '
                x = usCRC(x) And 65535      '
                crc = (crc \ 256) Xor x
            Next i
            Return crc

        Catch ex As Exception
        End Try
    End Function

    Public Overloads Shared Function mbus_CalcCRC16(ByVal byteList As ArrayList) As Long
        Try
            Dim x As Long
            Dim crc As Long
            Dim bt As Byte

            If (usCRC(1) <> &HC0C1) Then
                mbus_FillCRCTable()
            End If

            crc = 65535                     ' start with all 1's for a reverse CRC
            For Each bt In byteList         ' process each character in the message */
                x = bt                      ' get next char
                x = (crc Xor x) And 255     '
                x = usCRC(x) And 65535      '
                crc = (crc \ 256) Xor x
            Next
            Return crc
        Catch ex As Exception
        End Try
    End Function

    ' given a Byte, Calc a modbus style CRC-16 by look-up table
    'Function mbus_CalcOneCRC(ByVal nByte As Integer, ByVal lCrc As Long) As Long
    '    Dim n As Long
    '    If (usCRC(1) <> &HC0C1) Then
    '        mbus_FillCRCTable()
    '    End If

    '    n = nByte
    '    n = (crc Xor n) And 255  '
    '    n = usCRC(n) And 65535   '
    '    mbus_CalcOneCRC = (lCrc \ 256) Xor n
    'End Function

    Shared Sub mbus_FillCRCTable()
        ' This is a very silly function, but I cannot think
        ' of a better way to init this array (other than .DLL)
        ' VB doesn't have the DATA/READ statement used by earlier BASIC



        usCRC(0) = &H0 : usCRC(1) = &HC0C1 : usCRC(2) = &HC181 : usCRC(3) = &H140
        usCRC(4) = &HC301 : usCRC(5) = &H3C0 : usCRC(6) = &H280 : usCRC(7) = &HC241
        usCRC(8) = &HC601 : usCRC(9) = &H6C0 : usCRC(10) = &H780 : usCRC(11) = &HC741
        usCRC(12) = &H500 : usCRC(13) = &HC5C1 : usCRC(14) = &HC481 : usCRC(15) = &H440
        usCRC(16) = &HCC01 : usCRC(17) = &HCC0 : usCRC(18) = &HD80 : usCRC(19) = &HCD41
        usCRC(20) = &HF00 : usCRC(21) = &HCFC1 : usCRC(22) = &HCE81 : usCRC(23) = &HE40
        usCRC(24) = &HA00 : usCRC(25) = &HCAC1 : usCRC(26) = &HCB81 : usCRC(27) = &HB40
        usCRC(28) = &HC901 : usCRC(29) = &H9C0 : usCRC(30) = &H880 : usCRC(31) = &HC841
        usCRC(32) = &HD801 : usCRC(33) = &H18C0 : usCRC(34) = &H1980 : usCRC(35) = &HD941
        usCRC(36) = &H1B00 : usCRC(37) = &HDBC1 : usCRC(38) = &HDA81 : usCRC(39) = &H1A40
        usCRC(40) = &H1E00 : usCRC(41) = &HDEC1 : usCRC(42) = &HDF81 : usCRC(43) = &H1F40
        usCRC(44) = &HDD01 : usCRC(45) = &H1DC0 : usCRC(46) = &H1C80 : usCRC(47) = &HDC41
        usCRC(48) = &H1400 : usCRC(49) = &HD4C1 : usCRC(50) = &HD581 : usCRC(51) = &H1540
        usCRC(52) = &HD701 : usCRC(53) = &H17C0 : usCRC(54) = &H1680 : usCRC(55) = &HD641
        usCRC(56) = &HD201 : usCRC(57) = &H12C0 : usCRC(58) = &H1380 : usCRC(59) = &HD341
        usCRC(60) = &H1100 : usCRC(61) = &HD1C1 : usCRC(62) = &HD081 : usCRC(63) = &H1040
        usCRC(64) = &HF001 : usCRC(65) = &H30C0 : usCRC(66) = &H3180 : usCRC(67) = &HF141
        usCRC(68) = &H3300 : usCRC(69) = &HF3C1 : usCRC(70) = &HF281 : usCRC(71) = &H3240
        usCRC(72) = &H3600 : usCRC(73) = &HF6C1 : usCRC(74) = &HF781 : usCRC(75) = &H3740
        usCRC(76) = &HF501 : usCRC(77) = &H35C0 : usCRC(78) = &H3480 : usCRC(79) = &HF441
        usCRC(80) = &H3C00 : usCRC(81) = &HFCC1 : usCRC(82) = &HFD81 : usCRC(83) = &H3D40
        usCRC(84) = &HFF01 : usCRC(85) = &H3FC0 : usCRC(86) = &H3E80 : usCRC(87) = &HFE41
        usCRC(88) = &HFA01 : usCRC(89) = &H3AC0 : usCRC(90) = &H3B80 : usCRC(91) = &HFB41
        usCRC(92) = &H3900 : usCRC(93) = &HF9C1 : usCRC(94) = &HF881 : usCRC(95) = &H3840
        usCRC(96) = &H2800 : usCRC(97) = &HE8C1 : usCRC(98) = &HE981 : usCRC(99) = &H2940
        usCRC(100) = &HEB01 : usCRC(101) = &H2BC0 : usCRC(102) = &H2A80 : usCRC(103) = &HEA41
        usCRC(104) = &HEE01 : usCRC(105) = &H2EC0 : usCRC(106) = &H2F80 : usCRC(107) = &HEF41
        usCRC(108) = &H2D00 : usCRC(109) = &HEDC1 : usCRC(110) = &HEC81 : usCRC(111) = &H2C40
        usCRC(112) = &HE401 : usCRC(113) = &H24C0 : usCRC(114) = &H2580 : usCRC(115) = &HE541
        usCRC(116) = &H2700 : usCRC(117) = &HE7C1 : usCRC(118) = &HE681 : usCRC(119) = &H2640
        usCRC(120) = &H2200 : usCRC(121) = &HE2C1 : usCRC(122) = &HE381 : usCRC(123) = &H2340
        usCRC(124) = &HE101 : usCRC(125) = &H21C0 : usCRC(126) = &H2080 : usCRC(127) = &HE041
        usCRC(128) = &HA001 : usCRC(129) = &H60C0 : usCRC(130) = &H6180 : usCRC(131) = &HA141
        usCRC(132) = &H6300 : usCRC(133) = &HA3C1 : usCRC(134) = &HA281 : usCRC(135) = &H6240
        usCRC(136) = &H6600 : usCRC(137) = &HA6C1 : usCRC(138) = &HA781 : usCRC(139) = &H6740
        usCRC(140) = &HA501 : usCRC(141) = &H65C0 : usCRC(142) = &H6480 : usCRC(143) = &HA441
        usCRC(144) = &H6C00 : usCRC(145) = &HACC1 : usCRC(146) = &HAD81 : usCRC(147) = &H6D40
        usCRC(148) = &HAF01 : usCRC(149) = &H6FC0 : usCRC(150) = &H6E80 : usCRC(151) = &HAE41
        usCRC(152) = &HAA01 : usCRC(153) = &H6AC0 : usCRC(154) = &H6B80 : usCRC(155) = &HAB41
        usCRC(156) = &H6900 : usCRC(157) = &HA9C1 : usCRC(158) = &HA881 : usCRC(159) = &H6840
        usCRC(160) = &H7800 : usCRC(161) = &HB8C1 : usCRC(162) = &HB981 : usCRC(163) = &H7940
        usCRC(164) = &HBB01 : usCRC(165) = &H7BC0 : usCRC(166) = &H7A80 : usCRC(167) = &HBA41
        usCRC(168) = &HBE01 : usCRC(169) = &H7EC0 : usCRC(170) = &H7F80 : usCRC(171) = &HBF41
        usCRC(172) = &H7D00 : usCRC(173) = &HBDC1 : usCRC(174) = &HBC81 : usCRC(175) = &H7C40
        usCRC(176) = &HB401 : usCRC(177) = &H74C0 : usCRC(178) = &H7580 : usCRC(179) = &HB541
        usCRC(180) = &H7700 : usCRC(181) = &HB7C1 : usCRC(182) = &HB681 : usCRC(183) = &H7640
        usCRC(184) = &H7200 : usCRC(185) = &HB2C1 : usCRC(186) = &HB381 : usCRC(187) = &H7340
        usCRC(188) = &HB101 : usCRC(189) = &H71C0 : usCRC(190) = &H7080 : usCRC(191) = &HB041
        usCRC(192) = &H5000 : usCRC(193) = &H90C1 : usCRC(194) = &H9181 : usCRC(195) = &H5140
        usCRC(196) = &H9301 : usCRC(197) = &H53C0 : usCRC(198) = &H5280 : usCRC(199) = &H9241
        usCRC(200) = &H9601 : usCRC(201) = &H56C0 : usCRC(202) = &H5780 : usCRC(203) = &H9741
        usCRC(204) = &H5500 : usCRC(205) = &H95C1 : usCRC(206) = &H9481 : usCRC(207) = &H5440
        usCRC(208) = &H9C01 : usCRC(209) = &H5CC0 : usCRC(210) = &H5D80 : usCRC(211) = &H9D41
        usCRC(212) = &H5F00 : usCRC(213) = &H9FC1 : usCRC(214) = &H9E81 : usCRC(215) = &H5E40
        usCRC(216) = &H5A00 : usCRC(217) = &H9AC1 : usCRC(218) = &H9B81 : usCRC(219) = &H5B40
        usCRC(220) = &H9901 : usCRC(221) = &H59C0 : usCRC(222) = &H5880 : usCRC(223) = &H9841
        usCRC(224) = &H8801 : usCRC(225) = &H48C0 : usCRC(226) = &H4980 : usCRC(227) = &H8941
        usCRC(228) = &H4B00 : usCRC(229) = &H8BC1 : usCRC(230) = &H8A81 : usCRC(231) = &H4A40
        usCRC(232) = &H4E00 : usCRC(233) = &H8EC1 : usCRC(234) = &H8F81 : usCRC(235) = &H4F40
        usCRC(236) = &H8D01 : usCRC(237) = &H4DC0 : usCRC(238) = &H4C80 : usCRC(239) = &H8C41
        usCRC(240) = &H4400 : usCRC(241) = &H84C1 : usCRC(242) = &H8581 : usCRC(243) = &H4540
        usCRC(244) = &H8701 : usCRC(245) = &H47C0 : usCRC(246) = &H4680 : usCRC(247) = &H8641
        usCRC(248) = &H8201 : usCRC(249) = &H42C0 : usCRC(250) = &H4380 : usCRC(251) = &H8341
        usCRC(252) = &H4100 : usCRC(253) = &H81C1 : usCRC(254) = &H8081 : usCRC(255) = &H4040
    End Sub

    Public Shared Function ApplyCRC31(ByVal ByteList() As Byte) As ArrayList
        Try
            'Dim message(ByteList.Count - 1) As Byte
            'Dim bt As Byte
            'Dim i As Integer
            'For Each bt In ByteList
            '    message(i) = bt
            '    i += 1
            'Next
            Dim crc31 As New ATCSCRC.CRC31()
            Dim crc() As UInt64 = crc31.calculateCRCShift(ByteList, ByteList.Length)
            Dim lg As UInt64
            Dim CRCList As New ArrayList
            For Each lg In crc
                CRCList.Add(CByte(lg.ToString))
            Next
            Return CRCList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function ApplyCRC31ToByteList(ByVal ByteList() As Byte) As ArrayList
        Try
            'Dim message(ByteList.Count - 1) As Byte
            'Dim bt As Byte
            'Dim i As Integer
            'For Each bt In ByteList
            '    message(i) = bt
            '    i += 1
            'Next
            Dim crc31 As New ATCSCRC.CRC31()
            Dim crc() As UInt64 = crc31.calculateCRCShift(ByteList, ByteList.Length)
            Dim lg As UInt64
            Dim CRCList As New ArrayList
            For Each lg In crc
                CRCList.Add(CByte(lg.ToString))
            Next
            Return CRCList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    Public Shared Sub ApplyCRC32Extended(ByVal ByteList As ArrayList)
        Try
            Dim message(ByteList.Count - 1) As Byte
            Dim bt As Byte
            Dim i As Integer
            For Each bt In ByteList
                message(i) = bt
                i += 1
            Next
            Dim crc31 As New ATCSCRC.CRC32Extended()
            Dim crc() As UInt64 = crc31.calculateCRCShift(message, message.Length)
            Dim lg As UInt64
            Array.Reverse(crc)
            For Each lg In crc
                ByteList.Add(CByte(lg.ToString))
            Next

        Catch ex As Exception
            'AddEventToLog(EventEnum.Exception, ex.ToString, gEventQueue)
        End Try
    End Sub

    Public Shared Sub InitializeCRCTable()
        crctbl(0) = 0 : crctbl(1) = 1802443554 : crctbl(2) = 1469023759 : crctbl(3) = 1021353261
        crctbl(4) = 776911957 : crctbl(5) = 1159833463 : crctbl(6) = 2042706522 : crctbl(7) = 313401720
        crctbl(8) = 1553823914 : crctbl(9) = 938632072 : crctbl(10) = 185794213 : crctbl(11) = 1618863495
        crctbl(12) = 1926481151 : crctbl(13) = 431808477 : crctbl(14) = 626803440 : crctbl(15) = 1311988178
        crctbl(16) = 946553119 : crctbl(17) = 1392794173 : crctbl(18) = 1877264144 : crctbl(19) = 76274738
        crctbl(20) = 371588426 : crctbl(21) = 2102060648 : crctbl(22) = 1101691717 : crctbl(23) = 717578343
        crctbl(24) = 1693858229 : crctbl(25) = 261694103 : crctbl(26) = 863616954 : crctbl(27) = 1477878936
        crctbl(28) = 1253606880 : crctbl(29) = 567779010 : crctbl(30) = 490144751 : crctbl(31) = 1985485005
        crctbl(32) = 1893106238 : crctbl(33) = 465148188 : crctbl(34) = 660145201 : crctbl(35) = 1278615315
        crctbl(36) = 1587035755 : crctbl(37) = 905389385 : crctbl(38) = 152549476 : crctbl(39) = 1652073286
        crctbl(40) = 743176852 : crctbl(41) = 1193599414 : crctbl(42) = 2076474523 : crctbl(43) = 279668665
        crctbl(44) = 33900225 : crctbl(45) = 1768578531 : crctbl(46) = 1435156686 : crctbl(47) = 1055251436
        crctbl(48) = 1220395809 : crctbl(49) = 601020419 : crctbl(50) = 523388206 : crctbl(51) = 1952275980
        crctbl(52) = 1727233908 : crctbl(53) = 228353110 : crctbl(54) = 830273915 : crctbl(55) = 1511252569
        crctbl(56) = 337689483 : crctbl(57) = 2135924905 : crctbl(58) = 1135558020 : crctbl(59) = 683681446
        crctbl(60) = 980289502 : crctbl(61) = 1359027452 : crctbl(62) = 1843495377 : crctbl(63) = 110009075
        crctbl(64) = 1627173943 : crctbl(65) = 194227989 : crctbl(66) = 930296376 : crctbl(67) = 1545349402
        crctbl(68) = 1320290402 : crctbl(69) = 635245376 : crctbl(70) = 423464557 : crctbl(71) = 1918014799
        crctbl(72) = 1013042333 : crctbl(73) = 1460590527 : crctbl(74) = 1810778770 : crctbl(75) = 8475056
        crctbl(76) = 305098952 : crctbl(77) = 2034265066 : crctbl(78) = 1168176839 : crctbl(79) = 785378789
        crctbl(80) = 1486353704 : crctbl(81) = 871951882 : crctbl(82) = 253260583 : crctbl(83) = 1685547013
        crctbl(84) = 1993951613 : crctbl(85) = 498487903 : crctbl(86) = 559337330 : crctbl(87) = 1245303888
        crctbl(88) = 67800450 : crctbl(89) = 1868928672 : crctbl(90) = 1401228173 : crctbl(91) = 954863791
        crctbl(92) = 709112279 : crctbl(93) = 1093348085 : crctbl(94) = 2110502872 : crctbl(95) = 379890938
        crctbl(96) = 271201801 : crctbl(97) = 2068131115 : crctbl(98) = 1202040838 : crctbl(99) = 751479588
        crctbl(100) = 1046776412 : crctbl(101) = 1426821502 : crctbl(102) = 1777011795 : crctbl(103) = 42211185
        crctbl(104) = 1287081635 : crctbl(105) = 668489089 : crctbl(106) = 456706220 : crctbl(107) = 1884803982
        crctbl(108) = 1660547830 : crctbl(109) = 160885204 : crctbl(110) = 896955641 : crctbl(111) = 1578725339
        crctbl(112) = 675378966 : crctbl(113) = 1127115828 : crctbl(114) = 2144268569 : crctbl(115) = 346155579
        crctbl(116) = 101698371 : crctbl(117) = 1835061345 : crctbl(118) = 1367362892 : crctbl(119) = 988763758
        crctbl(120) = 1960579004 : crctbl(121) = 531829918 : crctbl(122) = 592677299 : crctbl(123) = 1211929233
        crctbl(124) = 1519563753 : crctbl(125) = 838707403 : crctbl(126) = 220018150 : crctbl(127) = 1718759108
        crctbl(128) = 1084807205 : crctbl(129) = 734511879 : crctbl(130) = 388455978 : crctbl(131) = 2085111048
        crctbl(132) = 1860592752 : crctbl(133) = 92897106 : crctbl(134) = 963240575 : crctbl(135) = 1376188765
        crctbl(136) = 473276559 : crctbl(137) = 2002434989 : crctbl(138) = 1270490752 : crctbl(139) = 550845858
        crctbl(140) = 846929114 : crctbl(141) = 1494484984 : crctbl(142) = 1710529237 : crctbl(143) = 245072375
        crctbl(144) = 2026084666 : crctbl(145) = 330072600 : crctbl(146) = 793517877 : crctbl(147) = 1143145495
        crctbl(148) = 1452090735 : crctbl(149) = 1038237261 : crctbl(150) = 16950112 : crctbl(151) = 1785575490
        crctbl(152) = 610197904 : crctbl(153) = 1328675506 : crctbl(154) = 1943103391 : crctbl(155) = 415136957
        crctbl(156) = 168844741 : crctbl(157) = 1635731175 : crctbl(158) = 1570757578 : crctbl(159) = 921747688
        crctbl(160) = 813586971 : crctbl(161) = 1527857465 : crctbl(162) = 1743903764 : crctbl(163) = 211732278
        crctbl(164) = 506521166 : crctbl(165) = 1969225068 : crctbl(166) = 1237278785 : crctbl(167) = 584088419
        crctbl(168) = 1826824881 : crctbl(169) = 126630291 : crctbl(170) = 996975806 : crctbl(171) = 1342422940
        crctbl(172) = 1118674660 : crctbl(173) = 700614086 : crctbl(174) = 354556139 : crctbl(175) = 2118976457
        crctbl(176) = 135600900 : crctbl(177) = 1668939814 : crctbl(178) = 1603968267 : crctbl(179) = 888505897
        crctbl(180) = 643540817 : crctbl(181) = 1295301747 : crctbl(182) = 1909727582 : crctbl(183) = 448477820
        crctbl(184) = 1418224558 : crctbl(185) = 1072134284 : crctbl(186) = 50849185 : crctbl(187) = 1751711363
        crctbl(188) = 2059853819 : crctbl(189) = 296338649 : crctbl(190) = 759781876 : crctbl(191) = 1176912598
        crctbl(192) = 542403602 : crctbl(193) = 1262188336 : crctbl(194) = 2010901021 : crctbl(195) = 481620287
        crctbl(196) = 236638279 : crctbl(197) = 1702218597 : crctbl(198) = 1502959176 : crctbl(199) = 855264618
        crctbl(200) = 2093552824 : crctbl(201) = 396758938 : crctbl(202) = 726045367 : crctbl(203) = 1076464021
        crctbl(204) = 1384622317 : crctbl(205) = 971551695 : crctbl(206) = 84422370 : crctbl(207) = 1852257728
        crctbl(208) = 406793485 : crctbl(209) = 1934636591 : crctbl(210) = 1336978178 : crctbl(211) = 618639392
        crctbl(212) = 913412440 : crctbl(213) = 1562282618 : crctbl(214) = 1644042071 : crctbl(215) = 177278069
        crctbl(216) = 1151489447 : crctbl(217) = 801984133 : crctbl(218) = 321770408 : crctbl(219) = 2017642634
        crctbl(220) = 1793911282 : crctbl(221) = 25424592 : crctbl(222) = 1029926909 : crctbl(223) = 1443656927
        crctbl(224) = 1350757932 : crctbl(225) = 1005450510 : crctbl(226) = 118319139 : crctbl(227) = 1818391297
        crctbl(228) = 2127319673 : crctbl(229) = 363022683 : crctbl(230) = 692311158 : crctbl(231) = 1110232916
        crctbl(232) = 203396742 : crctbl(233) = 1735429540 : crctbl(234) = 1536168073 : crctbl(235) = 822021035
        crctbl(236) = 575744723 : crctbl(237) = 1228812785 : crctbl(238) = 1977527516 : crctbl(239) = 514963454
        crctbl(240) = 1760145203 : crctbl(241) = 59159569 : crctbl(242) = 1063659836 : crctbl(243) = 1409888798
        crctbl(244) = 1185354598 : crctbl(245) = 768084036 : crctbl(246) = 287872361 : crctbl(247) = 2051509835
        crctbl(248) = 880072601 : crctbl(249) = 1595657403 : crctbl(250) = 1677414806 : crctbl(251) = 143936180
        crctbl(252) = 440036300 : crctbl(253) = 1901424878 : crctbl(254) = 1303768515 : crctbl(255) = 651884257
    End Sub

    Public Shared Function CRC31(ByVal buffer() As Byte, ByVal init As Long) As Long
        Dim accum As Long
        Dim tmp As Long
        accum = init   'initialize CRC accumulator
        For tmp = 0 To buffer.Length - 1
            accum = crctbl((accum Xor buffer(tmp)) And &HFF) Xor (ShiftBitsRight(accum, 8))
        Next tmp
        Return accum
    End Function

    Public Shared Function CRC32Extended(ByVal buffer() As Byte, ByVal init As Long) As Long
        Dim accum As Long
        Dim tmp As Long
        accum = init   'initialize CRC accumulator
        For tmp = 0 To buffer.Length - 1
            accum = crctbl((accum Xor buffer(tmp)) And &HFF) Xor (ShiftBitsRight(accum, 8))
        Next tmp
        Return accum
    End Function

    Public Shared Function ShiftBitsRight(ByVal dwValue As Long, _
                           ByVal dwBitsRight As Long) As Long

        'bitwise right-shift equivalent to
        'C++ >> operator
        ShiftBitsRight = CLng(dwValue / (2 ^ dwBitsRight))

    End Function

End Class
