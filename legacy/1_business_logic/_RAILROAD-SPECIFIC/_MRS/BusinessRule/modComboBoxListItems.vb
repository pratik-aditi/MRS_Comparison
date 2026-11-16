Imports TMDS.SystemUtilities.Utilities

Module modComboBoxListItems

    ''' <summary>
    ''' Load Items PSS
    ''' </summary>
    ''' <param name="pCmbList"></param>
    ''' <param name="pLoadType"></param>
    Public Sub LoadItemsPSS(ByRef pCmbList As List(Of clsPSSSendReasons), ByVal pLoadType As Integer)
        Try
            SystemEvent("LoadItemsPSS:LoadType=" & pLoadType.ToString)
            Select Case pLoadType
                Case 0 'PSS
                    pCmbList.Add(New clsPSSSendReasons() With
                              {.Text = "SINAL INOPERANTE",
                               .ID = 0
                              })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "SINAL EM manutenção".ToUpper,
                                 .ID = 1
                                })
                Case 1 'SWITCH
                    pCmbList.Add(New clsPSSSendReasons() With
                              {.Text = "CHAVE INOPERANTE",
                               .ID = 0
                              })
                Case 2 'TRACK BLOCK
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "TRECHO INTERDITADO",
                                 .ID = 0
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "QUEDA DE TALUDE ",
                                 .ID = 1
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "INTERVALO DE VIA ",
                                 .ID = 2
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                             {.Text = "CHAVE SEM indicação".ToUpper,
                              .ID = 3
                             })
                    pCmbList.Add(New clsPSSSendReasons() With
                            {.Text = "HOMENS TRABALHANDO",
                             .ID = 4
                            })
                Case 3 'FALSE OCCUPANCY
                    pCmbList.Add(New clsPSSSendReasons() With
                              {.Text = "FALSA ocupação".ToUpper,
                               .ID = 0
                              })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "TRILHO PARTIDO",
                                 .ID = 1
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                             {.Text = "manutenção".ToUpper,
                              .ID = 2
                             })
                Case 4 'TRAIN AHEAD
                    pCmbList.Add(New clsPSSSendReasons() With
                                 {.Text = "MANOBRA".ToUpper,
                                  .ID = 0
                                 })
                    pCmbList.Add(New clsPSSSendReasons() With
                                 {.Text = "Trem à frente".ToUpper,
                                  .ID = 1
                                 })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "Veículo DE VIA A FRENTE".ToUpper,
                                 .ID = 2
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                                 {.Text = "manutenção".ToUpper,
                                  .ID = 3
                                 })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "TRILHO PARTIDO".ToUpper,
                                 .ID = 4
                                })
                Case 5 'CONTROL POINT NO COMMS
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "CTC ISOLADO".ToUpper,
                                .ID = 0
                                })
                Case 6 'BIDIRECTIONAL LICENSE
                    pCmbList.Add(New clsPSSSendReasons() With
                              {.Text = "ABASTECIMENTO".ToUpper,
                               .ID = 0
                              })
                    pCmbList.Add(New clsPSSSendReasons() With
                                {.Text = "manutenção DE VIA".ToUpper,
                                 .ID = 1
                                })
                    pCmbList.Add(New clsPSSSendReasons() With
                             {.Text = "INTERVALO DE VIA".ToUpper,
                              .ID = 2
                             })
                Case 7 'TNT
                    pCmbList.Add(New clsPSSSendReasons() With
                              {.Text = "INTERVALO DE VIA".ToUpper,
                               .ID = 0
                              })
            End Select
        Catch ex As Exception
            SystemException(ex.ToString)
        End Try
    End Sub

End Module
