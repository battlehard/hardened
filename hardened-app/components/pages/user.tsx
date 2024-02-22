'use client'

import {
  AlertColor,
  Box,
  Button,
  Input,
  Modal,
  TextField,
  Typography,
  styled,
} from '@mui/material'
import {
  IPendingProperties,
  HardenedContract,
} from '@/utils/neo/contracts/hardened'
import React, { ChangeEvent, useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'
import Notification from '../notification'
import { HASH160_PATTERN, NUMBER_PATTERN } from '../constant'
import { getDecimalForm } from '@/utils/neo/helpers'
import { AddressShorten } from '@/utils/app-helpers'

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

export default function UserPage() {
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

  const { connectedWallet, network } = useWallet()
  const [loading, setLoading] = useState(true)
  const [tradeList, setTradeList] = useState<IPendingProperties[]>([])
  const [openModal, setOpenModal] = useState(false)
  const handleModalOpen = () => {
    setOpenModal(true)
  }
  const handleModalClose = () => {
    setOpenModal(false)
  }

  const isDisable = () => {
    return (
      !connectedWallet ||
      !isValidOfferTokenHash ||
      inputOfferTokenHash.length == 0 ||
      !isValidPurchaseTokenHash ||
      inputPurchaseTokenHash.length == 0 ||
      !isValidOfferTokenAmount ||
      inputOfferTokenAmount.length == 0 ||
      !isValidOfferPackages ||
      inputOfferPackages.length == 0 ||
      !isValidPurchasePrice ||
      inputPurchasePrice.length == 0
    )
  }

  const INPUT_OFFER_TOKEN_HASH_ID = 'input-offer-token-hash'
  const [isValidOfferTokenHash, setIsValidOfferTokenHash] = useState(true)
  const [inputOfferTokenHash, setInputOfferTokenHash] = useState('')

  const INPUT_PURCHASE_TOKEN_HASH_ID = 'input-purchase-token-hash'
  const [isValidPurchaseTokenHash, setIsValidPurchaseTokenHash] = useState(true)
  const [inputPurchaseTokenHash, setInputPurchaseTokenHash] = useState('')

  const INPUT_OFFER_TOKEN_AMOUNT_ID = 'input-offer-token-amount'
  const [isValidOfferTokenAmount, setIsValidOfferTokenAmount] = useState(true)
  const [inputOfferTokenAmount, setInputOfferTokenAmount] = useState('')

  const INPUT_OFFER_PACKAGES_ID = 'input-offer-packages'
  const [isValidOfferPackages, setIsValidOfferPackages] = useState(true)
  const [inputOfferPackages, setInputOfferPackages] = useState('')

  const INPUT_PURCHASE_PRICE_ID = 'input-purchase-price'
  const [isValidPurchasePrice, setIsValidPurchasePrice] = useState(true)
  const [inputPurchasePrice, setInputPurchasePrice] = useState('')

  const INPUT_PURCHASE_PACKAGES_ID = 'input-purchase-packages'
  const [isValidPurchasePackages, setIsValidPurchasePackages] = useState<
    boolean[]
  >([true])
  const [inputPurchasePackages, setInputPurchasePackages] = useState<string[]>([
    '',
  ])

  const handleTokenHashChange = (event: ChangeEvent<HTMLInputElement>) => {
    const id = event.target.id
    const value = event.target.value
    switch (id) {
      case INPUT_OFFER_TOKEN_HASH_ID:
        setInputOfferTokenHash(value)
        if (value.length > 0) {
          setIsValidOfferTokenHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidOfferTokenHash(true)
        }
        break
      case INPUT_PURCHASE_TOKEN_HASH_ID:
        setInputPurchaseTokenHash(value)
        if (value.length > 0) {
          setIsValidPurchaseTokenHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidPurchaseTokenHash(true)
        }
        break
    }
  }

  const handleNumberChange = (event: ChangeEvent<HTMLInputElement>) => {
    const id = event.target.id.startsWith(INPUT_PURCHASE_PACKAGES_ID)
      ? INPUT_PURCHASE_PACKAGES_ID
      : event.target.id
    const index = event.target.id.startsWith(INPUT_PURCHASE_PACKAGES_ID)
      ? Number(event.target.id.split(INPUT_PURCHASE_PACKAGES_ID)[1])
      : 0
    const value = event.target.value
    switch (id) {
      case INPUT_OFFER_TOKEN_AMOUNT_ID:
        setInputOfferTokenAmount(value)
        if (value.length > 0) {
          setIsValidOfferTokenAmount(NUMBER_PATTERN.test(value))
        } else {
          setIsValidOfferTokenAmount(true)
        }
        break
      case INPUT_OFFER_PACKAGES_ID:
        setInputOfferPackages(value)
        if (value.length > 0) {
          setIsValidOfferPackages(NUMBER_PATTERN.test(value))
        } else {
          setIsValidOfferPackages(true)
        }
        break
      case INPUT_PURCHASE_PRICE_ID:
        setInputPurchasePrice(value)
        if (value.length > 0) {
          setIsValidPurchasePrice(NUMBER_PATTERN.test(value))
        } else {
          setIsValidPurchasePrice(true)
        }
        break
      case INPUT_PURCHASE_PACKAGES_ID:
        const updatedInputPurchasePackages = [...inputPurchasePackages]
        updatedInputPurchasePackages[index] = value
        setInputPurchasePackages(updatedInputPurchasePackages)
        const isValid = NUMBER_PATTERN.test(value)
        const updatedIsValidPurchasePackages = [...isValidPurchasePackages]
        updatedIsValidPurchasePackages[index] =
          value.length > 0 ? isValid : true
        setIsValidPurchasePackages(updatedIsValidPurchasePackages)
        break
    }
  }

  // const fetchListTrade = async () => {
  //   setLoading(true)
  //   try {
  //     const result = await new HardenedContract(network).ListTrade()
  //     setTradeList(result.pendingList)
  //     // Initialize values for each row
  //     setIsValidPurchasePackages(
  //       new Array(result.pendingList.length).fill(true)
  //     )
  //     setInputPurchasePackages(new Array(result.pendingList.length).fill(''))
  //   } catch (e: any) {
  //     if (e.type !== undefined) {
  //       showErrorPopup(`Error: ${e.type} ${e.description}`)
  //     }
  //     console.error(e)
  //   }

  //   setLoading(false)
  // }

  // useEffect(() => {
  //   fetchListTrade()
  // }, [])

  // const handleCreateTrade = async () => {
  //   if (connectedWallet) {
  //     try {
  //       const txid = await new HardenedContract(network).CreateTrade(
  //         connectedWallet,
  //         inputOfferTokenHash,
  //         Number(inputOfferTokenAmount),
  //         Number(inputOfferPackages),
  //         inputPurchaseTokenHash,
  //         Number(inputPurchasePrice)
  //       )
  //       showSuccessPopup(txid)
  //       handleModalClose()
  //     } catch (e: any) {
  //       if (e.type !== undefined) {
  //         showErrorPopup(`Error: ${e.type} ${e.description}`)
  //       }
  //       console.log(e)
  //     }
  //   }
  // }

  // const handleExecuteTrade = async (
  //   tradeId: number,
  //   purchaseTokenHash: string,
  //   purchasePackages: number
  // ) => {
  //   if (connectedWallet) {
  //     try {
  //       const txid = await new HardenedContract(network).ExecuteTrade(
  //         connectedWallet,
  //         tradeId,
  //         purchaseTokenHash,
  //         purchasePackages
  //       )
  //       showSuccessPopup(txid)
  //     } catch (e: any) {
  //       if (e.type !== undefined) {
  //         showErrorPopup(`Error: ${e.type} ${e.description}`)
  //       }
  //       console.log(e)
  //     }
  //   }
  // }

  return (
    <Box sx={{ width: '100%' }}>
      <Container>
        <Button
          disabled={!connectedWallet}
          variant="outlined"
          onClick={handleModalOpen}
        >
          Create Trade
        </Button>
      </Container>
      {loading && <MessagePanel message="Loading" />}
      {!loading && tradeList.length == 0 && (
        <MessagePanel message="No Trade in the pool" />
      )}
      <Modal
        open={openModal}
        onClose={handleModalClose}
        aria-labelledby="modal-modal-title"
      >
        <Box sx={modalStyle}>
          <Typography id="modal-modal-title" variant="h6" component="h2">
            Trade Creation
          </Typography>
          <Container>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
              <InputTextField
                id={INPUT_OFFER_TOKEN_HASH_ID}
                required
                label="Offer token Hash (Required)"
                helperText={
                  isValidOfferTokenHash
                    ? 'Token contract in Hash160 format.'
                    : 'Invalid hash'
                }
                defaultValue=""
                value={inputOfferTokenHash}
                onChange={handleTokenHashChange}
                error={!isValidOfferTokenHash}
              />
              <InputTextField
                id={INPUT_OFFER_TOKEN_AMOUNT_ID}
                required
                label="Offer token Amount (Required)"
                helperText={
                  isValidOfferTokenAmount
                    ? 'Total amount for sell with BigInteger format. If the token use 8 decimal, then 1 token must input as 100000000'
                    : 'Must be number only'
                }
                defaultValue=""
                value={inputOfferTokenAmount}
                onChange={handleNumberChange}
                error={!isValidOfferTokenAmount}
              />
              <InputTextField
                id={INPUT_OFFER_PACKAGES_ID}
                required
                label="Offer packages (Required)"
                helperText={
                  isValidOfferPackages
                    ? 'Number of packages for sell, must be evenly divisible'
                    : 'Must be number only'
                }
                defaultValue=""
                value={inputOfferPackages}
                onChange={handleNumberChange}
                error={!isValidOfferPackages}
              />
              <InputTextField
                id={INPUT_PURCHASE_TOKEN_HASH_ID}
                required
                label="Purchase token Hash (Required)"
                helperText={
                  isValidPurchaseTokenHash
                    ? 'Token contract in Hash160 format.'
                    : 'Invalid hash'
                }
                defaultValue=""
                value={inputPurchaseTokenHash}
                onChange={handleTokenHashChange}
                error={!isValidPurchaseTokenHash}
              />
              <InputTextField
                id={INPUT_PURCHASE_PRICE_ID}
                required
                label="Purchase price (Required)"
                helperText={
                  isValidPurchasePrice
                    ? 'Price per package with BigInteger format'
                    : 'Must be number only'
                }
                defaultValue=""
                value={inputPurchasePrice}
                onChange={handleNumberChange}
                error={!isValidPurchasePrice}
              />
            </div>
            <Button
              disabled={isDisable()}
              style={{ marginTop: '25px', marginLeft: '25px' }}
              variant="outlined"
            >
              Create
            </Button>
          </Container>
        </Box>
      </Modal>
      <Notification
        open={open}
        handleClose={handleClose}
        severity={severity}
        message={msg}
      />
    </Box>
  )
}
