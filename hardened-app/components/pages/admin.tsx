'use client'

import { AlertColor, Button, TextField, styled } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import { VendorContract, WhiteListAction } from '@/utils/neo/contracts/vendor'
import React, { ChangeEvent, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN, NUMBER_PATTERN } from '../constant'

const InputTextField = styled(TextField)`
  width: 450px;
  margin-top: 25px;
  margin-left: 25px;
`

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

  const pages: ITabPage[] = [
    {
      label: 'Add offer token white list',
      component: OfferTokenWhiteList(WhiteListAction.ADD),
    },
    {
      label: 'Remove offer token list',
      component: OfferTokenWhiteList(WhiteListAction.REMOVE),
    },
    {
      label: 'Admin Cancel Trade',
      component: AdminSubPanel(),
    },
  ]

  function OfferTokenWhiteList(offerTokenWhiteListAction: WhiteListAction) {
    const { connectedWallet, network } = useWallet()
    const [inputOfferTokenHash, setInputOfferTokenHash] = useState('')
    const [isValidHash, setIsValidHash] = useState(true)
    const [inputSymbol, setInputSymbol] = useState('')
    const [inputImageUrl, setInputImageUrl] = useState('')
    const [isValidUrl, setIsValidUrl] = useState(true)

    const isDisable = (action: WhiteListAction) => {
      if (action == WhiteListAction.ADD) {
        return (
          !connectedWallet ||
          !isValidHash ||
          inputOfferTokenHash.length == 0 ||
          inputSymbol.length == 0 ||
          !isValidUrl ||
          inputImageUrl.length == 0
        )
      } else {
        return (
          !connectedWallet || !isValidHash || inputOfferTokenHash.length == 0
        )
      }
    }

    const handleOfferTokenHashChange = (
      event: ChangeEvent<HTMLInputElement>
    ) => {
      const value = event.target.value
      setInputOfferTokenHash(value)
      if (value.length > 0) {
        setIsValidHash(HASH160_PATTERN.test(value))
      } else {
        setIsValidHash(true)
      }
    }

    const handleSymbolChange = (event: ChangeEvent<HTMLInputElement>) => {
      setInputSymbol(event.target.value)
    }

    const handleImageUrlChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputImageUrl(value)
      if (value.length > 0) {
        setIsValidUrl(ValidateURL(value))
      } else {
        setIsValidUrl(true)
      }
    }

    const ValidateURL = (url: string): boolean => {
      try {
        new URL(url)
        return true
      } catch (_) {
        return false
      }
    }

    const showPopup = (severity: AlertColor, message: string) => {
      setOpen(true)
      setSeverity(severity)
      setMsg(message)
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new VendorContract(
            network
          ).ModifyOfferTokenWhiteList(
            connectedWallet,
            inputOfferTokenHash,
            inputSymbol,
            inputImageUrl,
            offerTokenWhiteListAction
          )
          showPopup('success', `Transaction submitted: txid = ${txid}`)
        } catch (e: any) {
          if (e.type !== undefined) {
            showPopup('error', `Error: ${e.type} ${e.description}`)
          }
          console.log(e)
        }
      }
    }

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <TextField
          required
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="Offer Token Contract Hash (Required)"
          helperText={
            isValidHash
              ? 'Offer token contract hash in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          defaultValue=""
          value={inputOfferTokenHash}
          onChange={handleOfferTokenHashChange}
          error={!isValidHash}
          inputProps={{ maxLength: 42 }}
        />
        {offerTokenWhiteListAction == WhiteListAction.ADD && (
          <InputTextField
            required
            label="Symbol (Required)"
            helperText="Symbol no more than 10 characters"
            defaultValue=""
            value={inputSymbol}
            onChange={handleSymbolChange}
            inputProps={{ maxLength: 10 }}
          />
        )}
        {offerTokenWhiteListAction == WhiteListAction.ADD && (
          <InputTextField
            required
            label="Image URL (Required)"
            defaultValue=""
            value={inputImageUrl}
            onChange={handleImageUrlChange}
            error={!isValidUrl}
          />
        )}
        <Button
          disabled={isDisable(offerTokenWhiteListAction)}
          onClick={invoke}
          style={{ marginTop: '25px', marginLeft: '25px', alignSelf: 'start' }}
        >
          Invoke
        </Button>
        <Notification
          open={open}
          handleClose={handleClose}
          severity={severity}
          message={msg}
        />
      </div>
    )
  }

  function AdminSubPanel() {
    const { connectedWallet, network } = useWallet()
    const [inputTradeId, setInputTradeId] = useState('')
    const [isValidTradeId, setIsValidTradeId] = useState(true)
    const handleTradeIdChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputTradeId(value)
      if (value.length > 0) {
        setIsValidTradeId(NUMBER_PATTERN.test(value))
      } else {
        setIsValidTradeId(false)
      }
    }

    const isInvokeDisable = (): boolean => {
      return !connectedWallet || !isValidTradeId || inputTradeId.length == 0
    }

    const invokeAdminCancelTrade = async () => {
      if (connectedWallet) {
        try {
          const txid = await new VendorContract(network).AdminCancelTrade(
            connectedWallet,
            Number(inputTradeId)
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

    return (
      <div>
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <InputTextField
            required
            label="TradeId (Required)"
            defaultValue=""
            value={inputTradeId}
            onChange={handleTradeIdChange}
            error={!isValidTradeId}
          />
        </div>
        <Button
          disabled={isInvokeDisable()}
          onClick={invokeAdminCancelTrade}
          style={{ marginTop: '25px', marginLeft: '25px', alignSelf: 'start' }}
        >
          Invoke
        </Button>
        <Notification
          open={open}
          handleClose={handleClose}
          severity={severity}
          message={msg}
        />
      </div>
    )
  }

  return <TabPanel pages={pages} />
}
