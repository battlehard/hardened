'use client'

import { AlertColor, Box, Button, TextField, styled } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import {
  ManageAdminAction,
  HardenedContract,
} from '@/utils/neo/contracts/hardened'
import React, { ChangeEvent, useEffect, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN } from '../constant'

const Container = styled(Box)`
  max-width: 900px;
  margin: 25px auto 0px;
  display: flex;
  flex-direction: column;
  border-top: 1px solid #ccc;
`

const ContainerRowForPool = styled(Box)`
  display: grid;
  grid-template-columns: 1fr;
  justify-items: center;
  align-items: center;
  text-align: center;
  margin-bottom: 10px;
  overflow-wrap: anywhere;
  border-bottom: 1px solid #ccc;
`

const Div = styled('div')(({ theme }) => ({
  ...theme.typography.button,
  padding: theme.spacing(1),
  textTransform: 'none',
}))

interface MessagePanelProps {
  message: string
}
const MessagePanel = ({ message }: MessagePanelProps) => {
  return (
    <Container>
      <Div style={{ textAlign: 'center' }}>{message}</Div>
    </Container>
  )
}

export default function AdminPage() {
  // Notification
  const [open, setOpen] = useState(false)
  const [severity, setSeverity] = useState<AlertColor>('success')
  const [msg, setMsg] = useState('')
  const handleClose = (
    event?: React.SyntheticEvent | Event,
    reason?: string
  ) => {
    if (reason === 'clickaway') {
      return
    }

    setOpen(false)
  }

  // TODO: Remaining Commands
  const pages: ITabPage[] = [
    {
      label: 'GetAdmin',
      component: ManageAdmin(ManageAdminAction.GET),
    },
    {
      label: 'SetAdmin',
      component: ManageAdmin(ManageAdminAction.SET),
    },
    {
      label: 'DeleteAdmin',
      component: ManageAdmin(ManageAdminAction.DELETE),
    },
    {
      label: 'SetBlueprintImageUrl',
      component: <></>,
    },
    {
      label: 'FeeUpdate',
      component: <></>,
    },
    {
      label: 'InfusionMint',
      component: <></>,
    },
    {
      label: 'InfusionUpdate',
      component: <></>,
    },
  ]

  function ManageAdmin(manageAdminAction: ManageAdminAction) {
    const { connectedWallet, network } = useWallet()
    const [inputWalletHash, setInputWalletHash] = useState('')
    const [isValidHash, setIsValidHash] = useState(true)
    const isDisable = () => {
      return !connectedWallet || !isValidHash || inputWalletHash.length == 0
    }

    const handleWalletHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputWalletHash(value)
      if (value.length > 0) {
        setIsValidHash(HASH160_PATTERN.test(value))
      } else {
        setIsValidHash(true)
      }
    }

    const showPopup = (severity: AlertColor, message: string) => {
      setOpen(true)
      setSeverity(severity)
      setMsg(message)
    }
    const showSuccessPopup = (txid: string) => {
      showPopup('success', `Transaction submitted: txid = ${txid}`)
    }
    const showErrorPopup = (message: string) => {
      showPopup('error', message)
    }

    const [loading, setLoading] = useState(true)
    const [adminList, setAdminList] = useState<string[]>([])
    const getAdmin = async () => {
      setLoading(true)
      try {
        const result = await new HardenedContract(network).GetAdmin()
        setAdminList(result)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.error(e)
      }

      setLoading(false)
    }

    useEffect(() => {
      getAdmin()
    }, [])

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new HardenedContract(network).ManageAdmin(
            connectedWallet,
            inputWalletHash,
            manageAdminAction
          )
          showSuccessPopup(txid)
        } catch (e: any) {
          if (e.type !== undefined) {
            showErrorPopup(`Error: ${e.type} ${e.description}`)
          }
          console.log(e)
        }
      }
    }

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {manageAdminAction == ManageAdminAction.GET && (
          <div>
            {loading && <MessagePanel message="Loading" />}
            {!loading && adminList.length == 0 && (
              <MessagePanel message="No Admin in the list" />
            )}
            {!loading && adminList.length > 0 && (
              <Container>
                {adminList.map((admin, index) => {
                  return (
                    <ContainerRowForPool key={index}>
                      <Div>{admin}</Div>
                    </ContainerRowForPool>
                  )
                })}
              </Container>
            )}
          </div>
        )}
        {manageAdminAction != ManageAdminAction.GET && (
          <>
            <TextField
              required
              style={{
                width: '450px',
                marginTop: '25px',
                marginLeft: '25px',
              }}
              label="Wallet Hash (Required)"
              helperText={
                isValidHash
                  ? 'Admin wallet in Hash160 format start in 0x'
                  : 'Invalid hash'
              }
              defaultValue=""
              value={inputWalletHash}
              onChange={handleWalletHashChange}
              error={!isValidHash}
              inputProps={{ maxLength: 42 }}
            />
            <Button
              disabled={isDisable()}
              onClick={invoke}
              style={{
                marginTop: '25px',
                marginLeft: '25px',
                alignSelf: 'start',
              }}
            >
              Invoke
            </Button>
            <Notification
              open={open}
              handleClose={handleClose}
              severity={severity}
              message={msg}
            />
          </>
        )}
      </div>
    )
  }
  return <TabPanel pages={pages} />
}
