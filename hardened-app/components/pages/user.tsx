'use client'

import { AlertColor, Box, Button, TextField, styled } from '@mui/material'
import {
  HardenedContract,
  IPreInfusionObject,
} from '@/utils/neo/contracts/hardened'
import React, { ChangeEvent, useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'
import Notification from '../notification'
import { HASH160_PATTERN, NUMBER_PATTERN } from '../constant'
import TabPanel, { ITabPage } from '../tab-panel'

const Container = styled(Box)`
  max-width: 900px;
  margin: 25px auto 0px;
  display: flex;
  flex-direction: column;
  border-top: 1px solid #ccc;
`

const ContainerRowForSlot = styled(Box)`
  display: grid;
  grid-template-columns: repeat(2, 475px);
  justify-items: left;
  align-items: left;
  text-align: left;
  margin-bottom: 10px;
  overflow-wrap: anywhere;
  // border-bottom: 1px solid #ccc;
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

  interface InvokeButtonProps {
    isDisable: boolean
    invoke: () => Promise<void>
  }
  const InvokeButton = ({ isDisable, invoke }: InvokeButtonProps) => {
    return (
      <Button
        disabled={isDisable}
        onClick={invoke}
        style={{
          marginTop: '25px',
          marginLeft: '25px',
          alignSelf: 'start',
        }}
      >
        Invoke
      </Button>
    )
  }

  // TODO: Remaining tabs
  const pages: ITabPage[] = [
    {
      label: 'PreInfusion',
      component: ManagePreInfusion(),
    },
    {
      label: 'CancelInfusion',
      component: ManageCancelInfusion(),
    },
    {
      label: 'Unfuse',
      component: ManageUnfuse(),
    },
    {
      label: 'BurnInfusion',
      component: ManageBurnInfusion(),
    },
  ]

  function ManagePreInfusion() {
    const [inputClientPubKey, setInputClientPubKey] = useState('')

    const INPUT_PAY_TOKEN_HASH = 'input-pay-token-hash'
    const [inputPayTokenHash, setInputPayTokenHash] = useState('')
    const [isValidHash, setIsValidHash] = useState(true)

    const [inputPayTokenAmount, setInputPayTokenAmount] = useState('')
    const [isValidPayTokenAmount, setIsValidPayTokenAmount] = useState(true)

    const INPUT_BH_NFT_ID_ID = 'input-bh-nft-id'
    const [inputBhNftId, setInputBhNftId] = useState('')

    const INPUT_SLOT_NFT_HASH_PREFIX = 'input-slot-nft-hash-'
    const [inputSlotNftHash, setInputSlotNftHash] = useState<string[]>([
      '',
      '',
      '',
      '',
    ])
    const [isValidSlotNftHash, setIsValidSlotNftHash] = useState<boolean[]>([
      true,
      true,
      true,
      true,
    ])
    const INPUT_SLOT_NFT_ID_PREFIX = 'input-slot-nft-id-'
    const [inputSlotNftId, setInputSlotNftId] = useState<string[]>([
      '',
      '',
      '',
      '',
    ])
    const [isAtLeastOneNftFill, setIsAtLeastOneNftFill] = useState(false)

    const isDisable = () => {
      return (
        !connectedWallet ||
        !isValidHash ||
        inputPayTokenHash.length == 0 ||
        !isValidPayTokenAmount ||
        inputPayTokenAmount.length == 0 ||
        !isAtLeastOneNftFill
      )
    }

    const checkAtLeastOneSlotFilled = (
      inputSlotNftHash: string[],
      inputSlotNftId: string[]
    ): void => {
      if (inputSlotNftHash.length != inputSlotNftId.length)
        throw new Error('slot nft hash and id list must be the same length')
      for (let i = 0; i < inputSlotNftId.length; i++) {
        if (inputSlotNftHash[i].length > 0 && inputSlotNftId[i].length) {
          setIsAtLeastOneNftFill(true)
          return
        }
      }
      setIsAtLeastOneNftFill(false)
    }

    const handleHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_PAY_TOKEN_HASH) {
        setInputPayTokenHash(value)
        if (value.length > 0) {
          setIsValidHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidHash(true)
        }
      } else if (event.target.id.match(INPUT_SLOT_NFT_HASH_PREFIX)) {
        const index = Number(
          event.target.id.split(INPUT_SLOT_NFT_HASH_PREFIX)[1]
        )
        setInputSlotNftHash((prevInputSlotNftHash) => {
          const updatedInputSlotNftHash = [...prevInputSlotNftHash]
          updatedInputSlotNftHash[index] = value
          checkAtLeastOneSlotFilled(updatedInputSlotNftHash, inputSlotNftId)
          return updatedInputSlotNftHash
        })
        if (value.length > 0) {
          setIsValidSlotNftHash((prevIsValidSlotNftHash) => {
            const updateIsValidSlotNftHash = [...prevIsValidSlotNftHash]
            updateIsValidSlotNftHash[index] = HASH160_PATTERN.test(value)
            return updateIsValidSlotNftHash
          })
        } else {
          setIsValidSlotNftHash((prevIsValidSlotNftHash) => {
            const updateIsValidSlotNftHash = [...prevIsValidSlotNftHash]
            updateIsValidSlotNftHash[index] = true
            return updateIsValidSlotNftHash
          })
        }
      }
    }

    const handleNumberChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputPayTokenAmount(value)
      if (value.length > 0) {
        setIsValidPayTokenAmount(NUMBER_PATTERN.test(value))
      } else {
        setIsValidPayTokenAmount(true)
      }
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_BH_NFT_ID_ID) {
        setInputBhNftId(value)
      } else if (event.target.id.match(INPUT_SLOT_NFT_ID_PREFIX)) {
        const index = Number(event.target.id.split(INPUT_SLOT_NFT_ID_PREFIX)[1])
        setInputSlotNftId((prevInputSlotNftId) => {
          const updatedInputSlotNftId = [...prevInputSlotNftId]
          updatedInputSlotNftId[index] = value
          checkAtLeastOneSlotFilled(inputSlotNftHash, updatedInputSlotNftId)
          return updatedInputSlotNftId
        })
      }
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const preInfusionObj: IPreInfusionObject = {
            clientPubKey: inputClientPubKey,
            payTokenHash: inputPayTokenHash,
            payTokenAmount: Number(inputPayTokenAmount),
            bhNftId: inputBhNftId,
            slotNftHashes: inputSlotNftHash.filter((hash) => hash !== ''),
            slotNftIds: inputSlotNftId.filter((id) => id !== ''),
          }

          const txid = await new HardenedContract(network).PreInfusion(
            connectedWallet,
            preInfusionObj
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

    const genClientPubKey = (bytesLength: number): void => {
      if (!connectedWallet) return
      // Generate a random byte array of specified length
      const randomBytes = new Uint8Array(bytesLength)
      for (let i = 0; i < bytesLength; i++) {
        randomBytes[i] = Math.floor(Math.random() * 256) // Generate random byte (0-255)
      }

      let clientPubKey = Buffer.from(randomBytes).toString('base64')

      clientPubKey =
        clientPubKey.substring(0, clientPubKey.length - 4) +
        connectedWallet.account.address.substring(
          connectedWallet.account.address.length - 4
        )

      setInputClientPubKey(clientPubKey)
    }

    useEffect(() => {
      if (connectedWallet) {
        genClientPubKey(16)
      }
    }, [connectedWallet])

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {connectedWallet && (
          <InputTextField
            disabled
            required
            label="ClientPubKey"
            value={inputClientPubKey}
          />
        )}
        <TextField
          required
          id={INPUT_PAY_TOKEN_HASH}
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="Pay Token Hash (Required)"
          helperText={
            isValidHash
              ? 'Pay token in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          value={inputPayTokenHash}
          onChange={handleHashChange}
          error={!isValidHash}
          inputProps={{ maxLength: 42 }}
        />
        <InputTextField
          required
          label="Pay Token Amount (Required)"
          helperText={
            isValidPayTokenAmount
              ? 'Amount with BigInteger format. If the token use 8 decimal, then 1 token must input as 100000000'
              : 'Must be number only'
          }
          value={inputPayTokenAmount}
          onChange={handleNumberChange}
          error={!isValidPayTokenAmount}
        />
        <TextField
          id={INPUT_BH_NFT_ID_ID}
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="BH NFT ID (Optional)"
          value={inputBhNftId}
          onChange={handleTextChange}
        />
        <ContainerRowForSlot>
          <TextField
            id={INPUT_SLOT_NFT_HASH_PREFIX + '0'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 1 NFT Hash (Optional at least 1 slot must have value)"
            value={inputSlotNftHash[0]}
            onChange={handleHashChange}
            helperText={
              isValidSlotNftHash[0]
                ? 'Hash160 format start in 0x'
                : 'Invalid hash'
            }
            error={!isValidSlotNftHash[0]}
            inputProps={{ maxLength: 42 }}
          />
          <TextField
            id={INPUT_SLOT_NFT_ID_PREFIX + '0'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 1 NFT ID (Optional at least 1 slot must have value)"
            value={inputSlotNftId[0]}
            onChange={handleTextChange}
          />
        </ContainerRowForSlot>
        <ContainerRowForSlot>
          <TextField
            id={INPUT_SLOT_NFT_HASH_PREFIX + '1'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 2 NFT Hash (Optional at least 1 slot must have value)"
            value={inputSlotNftHash[1]}
            onChange={handleHashChange}
            helperText={
              isValidSlotNftHash[1]
                ? 'Hash160 format start in 0x'
                : 'Invalid hash'
            }
            error={!isValidSlotNftHash[1]}
            inputProps={{ maxLength: 42 }}
          />
          <TextField
            id={INPUT_SLOT_NFT_ID_PREFIX + '1'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 2 NFT ID (Optional at least 1 slot must have value)"
            value={inputSlotNftId[1]}
            onChange={handleTextChange}
          />
        </ContainerRowForSlot>
        <ContainerRowForSlot>
          <TextField
            id={INPUT_SLOT_NFT_HASH_PREFIX + '2'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 3 NFT Hash (Optional at least 1 slot must have value)"
            value={inputSlotNftHash[2]}
            onChange={handleHashChange}
            helperText={
              isValidSlotNftHash[2]
                ? 'Hash160 format start in 0x'
                : 'Invalid hash'
            }
            error={!isValidSlotNftHash[2]}
            inputProps={{ maxLength: 42 }}
          />
          <TextField
            id={INPUT_SLOT_NFT_ID_PREFIX + '2'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 3 NFT ID (Optional at least 1 slot must have value)"
            value={inputSlotNftId[2]}
            onChange={handleTextChange}
          />
        </ContainerRowForSlot>
        <ContainerRowForSlot>
          <TextField
            id={INPUT_SLOT_NFT_HASH_PREFIX + '3'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 4 NFT Hash (Optional at least 1 slot must have value)"
            value={inputSlotNftHash[3]}
            onChange={handleHashChange}
            helperText={
              isValidSlotNftHash[3]
                ? 'Hash160 format start in 0x'
                : 'Invalid hash'
            }
            error={!isValidSlotNftHash[3]}
            inputProps={{ maxLength: 42 }}
          />
          <TextField
            id={INPUT_SLOT_NFT_ID_PREFIX + '3'}
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Slot 4 NFT ID (Optional at least 1 slot must have value)"
            value={inputSlotNftId[3]}
            onChange={handleTextChange}
          />
        </ContainerRowForSlot>
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
        <NotificationBox />
      </div>
    )
  }

  function ManageCancelInfusion() {
    const [inputClientPubKey, setInputClientPubKey] = useState('')
    const INPUT_CLIENT_PUBKEY_ID = 'input-client-pubkey-id'
    const [inputContractPubKey, setInputContractPubKey] = useState('')
    const INPUT_CONTRACT_PUBKEY_ID = 'input-contract-pubkey-id'

    const isDisable = () => {
      return (
        !connectedWallet ||
        inputClientPubKey.length == 0 ||
        inputContractPubKey.length == 0
      )
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_CLIENT_PUBKEY_ID) {
        setInputClientPubKey(value)
      } else if (event.target.id == INPUT_CONTRACT_PUBKEY_ID) {
        setInputContractPubKey(value)
      }
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new HardenedContract(network).CancelInfusion(
            connectedWallet,
            inputClientPubKey,
            inputContractPubKey
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
        <InputTextField
          id={INPUT_CLIENT_PUBKEY_ID}
          required
          label="ClientPubKey"
          value={inputClientPubKey}
          onChange={handleTextChange}
        />
        <InputTextField
          id={INPUT_CONTRACT_PUBKEY_ID}
          required
          label="ContractPubKey"
          value={inputContractPubKey}
          onChange={handleTextChange}
        />
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
      </div>
    )
  }

  function ManageUnfuse() {
    const [inputBhNftId, setInputBhNftId] = useState('')

    const isDisable = () => {
      return !connectedWallet || inputBhNftId.length == 0
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputBhNftId(value)
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new HardenedContract(network).Unfuse(
            connectedWallet,
            inputBhNftId
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
        <InputTextField
          required
          label="BH NFT ID"
          value={inputBhNftId}
          onChange={handleTextChange}
        />
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
      </div>
    )
  }

  function ManageBurnInfusion() {
    const [inputBhNftId, setInputBhNftId] = useState('')

    const isDisable = () => {
      return !connectedWallet || inputBhNftId.length == 0
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputBhNftId(value)
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new HardenedContract(network).BurnInfusion(
            connectedWallet,
            inputBhNftId
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
        <InputTextField
          required
          label="BH NFT ID"
          value={inputBhNftId}
          onChange={handleTextChange}
        />
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
      </div>
    )
  }

  return <TabPanel pages={pages} />
}
