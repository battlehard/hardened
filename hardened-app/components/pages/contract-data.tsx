'use client'

import { AlertColor, Box, TextField, styled } from '@mui/material'
import {
  HardenedContract,
  IPendingInfusionProperties,
} from '@/utils/neo/contracts/hardened'
import React, { useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'
import Notification from '../notification'
import TabPanel, { ITabPage } from '../tab-panel'

const Container = styled(Box)`
  max-width: 900px;
  margin: 25px auto 0px;
  display: flex;
  flex-direction: column;
  border-top: 1px solid #ccc;
`

const ContainerRowForPool = styled(Box)`
  display: grid;
  grid-template-columns: repeat(9, 1fr);
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

const modalStyle = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 'auto',
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
}

const InputTextField = styled(TextField)`
  width: 600px;
  margin-top: 25px;
  margin-left: 25px;
`

const Image = styled('img')`
  width: 32px;
  height: 32px;
`

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

export default function ContractDataPage() {
  // Wallet
  const { connectedWallet, network } = useWallet()
  // Notification
  const [open, setOpen] = useState(false)
  const [severity, setSeverity] = useState<AlertColor>('success')
  const [msg, setMsg] = useState('')
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
  const handleClose = (
    event?: React.SyntheticEvent | Event,
    reason?: string
  ) => {
    if (reason === 'clickaway') {
      return
    }

    setOpen(false)
  }

  const NotificationBox = () => {
    return (
      <Notification
        open={open}
        handleClose={handleClose}
        severity={severity}
        message={msg}
      />
    )
  }

  const pages: ITabPage[] = [
    {
      label: 'PendingInfusion',
      component: ManagePendingInfusion(),
    },
  ]

  function ManagePendingInfusion() {
    // Loading
    const [loading, setLoading] = useState(true)
    const [pendingList, setPendingList] = useState<
      IPendingInfusionProperties[]
    >([])

    const fetchPendingInfusion = async () => {
      setLoading(true)
      try {
        const result = await new HardenedContract(network).PendingInfusion()
        setPendingList(result.pendingList)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.error(e)
      }

      setLoading(false)
    }

    useEffect(() => {
      fetchPendingInfusion()
    }, [])

    return (
      <Box sx={{ width: '100%' }}>
        {loading && <MessagePanel message="Loading" />}
        {!loading && pendingList.length == 0 && (
          <MessagePanel message="No PendingInfusion" />
        )}
        {!loading && pendingList.length > 0 && (
          <Container>
            <ContainerRowForPool>
              <Div>Client Pub Key</Div>
              <Div>Contract Pub Key</Div>
              <Div>UserWalletAddress</Div>
              <Div>BH NFT ID</Div>
              <Div>Slot NFT Hashes</Div>
              <Div>Slot NFT IDs</Div>
            </ContainerRowForPool>
            {pendingList.map((pending, index) => {
              return (
                <ContainerRowForPool key={index}>
                  <Div>{pending.clientPubKey}</Div>
                  <Div>{pending.contractPubKey}</Div>
                  <Div>{pending.userWalletAddress}</Div>
                  <Div>{pending.bhNftId}</Div>
                  <Div>{pending.slotNftHashes}</Div>
                  <Div>{pending.slotNftIds}</Div>
                </ContainerRowForPool>
              )
            })}
          </Container>
        )}
        <Notification
          open={open}
          handleClose={handleClose}
          severity={severity}
          message={msg}
        />
      </Box>
    )
  }

  return <TabPanel pages={pages} />
}
