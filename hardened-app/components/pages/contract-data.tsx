'use client'

import { AlertColor, Box, Button, TextField, styled } from '@mui/material'
import {
  HardenedContract,
  IPendingInfusionProperties,
} from '@/utils/neo/contracts/hardened'
import React, { ChangeEvent, useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'
import Notification from '../notification'
import TabPanel, { ITabPage } from '../tab-panel'
import { N3_ADDRESS_PATTERN } from '../constant'
import { getScriptHashFromAddress } from '@cityofzion/neon-core/lib/wallet'

const Container = styled(Box)`
  max-width: 1280px;
  margin: 25px auto 0px;
  display: flex;
  flex-direction: column;
  border-top: 1px solid #ccc;
`

const ContainerRowForPool = styled(Box)`
  display: grid;
  grid-template-columns: repeat(6, 1fr);
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

export default function ContractDataPage() {
  // Wallet
  const { network } = useWallet()
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

  const pages: ITabPage[] = [
    {
      label: 'PendingInfusion',
      component: ManagePendingInfusion(),
    },
  ]

  function ManagePendingInfusion() {
    // Filter
    const [inputFilter, setInputFilter] = useState('')
    const [isValidFilter, setIsValidFilter] = useState(true)
    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputFilter(value)

      let isValid = true
      if (value.length > 0) {
        value.split(',').forEach((address) => {
          if (N3_ADDRESS_PATTERN.test(address) == false) {
            isValid = false
          }
        })
        setIsValidFilter(isValid)
      } else {
        setIsValidFilter(isValid)
      }
    }
    // Loading
    const [loading, setLoading] = useState(true)
    const [pendingList, setPendingList] = useState<
      IPendingInfusionProperties[]
    >([])

    const fetchPendingInfusion = async (filterList: string[] = []) => {
      setLoading(true)
      try {
        const result = await new HardenedContract(network).PendingInfusion(
          filterList
        )
        setPendingList(result.pendingList)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.error(e)
      }

      setLoading(false)
    }

    const CreateArrayFromFilterText = (inputFilter: string) => {
      if (inputFilter.length == 0) return []
      else {
        return inputFilter
          .split(',')
          .map((address) => getScriptHashFromAddress(address))
      }
    }

    useEffect(() => {
      fetchPendingInfusion(CreateArrayFromFilterText(inputFilter))
    }, [])

    const filter = async () => {
      fetchPendingInfusion(CreateArrayFromFilterText(inputFilter))
    }

    return (
      <Box sx={{ width: '100%' }}>
        <Container>
          <TextField
            style={{
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Filter Wallet List (Optional)"
            value={inputFilter}
            onChange={handleTextChange}
            helperText={
              isValidFilter
                ? 'Address List separate with comma. E.g. NZR5RJpeRqjP3aHFGoiKMDkCLfgxRuMLzh,NTsSEKhpngsRsLZDcrJpMUT7523fcdM9qF'
                : 'Must be Address separate with comma (,)'
            }
            error={!isValidFilter}
          />
          <Button
            style={{ width: '100px', marginLeft: '25px' }}
            variant="outlined"
            onClick={filter}
          >
            Filter
          </Button>
        </Container>
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
                  <Div>
                    {pending.slotNftHashes.map((hash, index) => {
                      return (
                        <Div style={{ borderBottom: '1px solid' }} key={index}>
                          {hash}
                        </Div>
                      )
                    })}
                  </Div>
                  <Div>
                    {pending.slotNftIds.map((nftId, index) => {
                      return (
                        <Div style={{ borderBottom: '1px solid' }} key={index}>
                          {nftId}
                        </Div>
                      )
                    })}
                  </Div>
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
