'use client'

import { AlertColor, Box, Button, TextField, styled } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import {
  ManageAdminAction,
  HardenedContract,
  ReadMethod,
  IFeeStructure,
  IInfusionMintObject,
  IInfusionUpdateObject,
} from '@/utils/neo/contracts/hardened'
import React, { ChangeEvent, useEffect, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN, IMAGE_URL_PATTERN, NUMBER_PATTERN } from '../constant'

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

const InputTextField = styled(TextField)`
  width: 600px;
  margin-top: 25px;
  margin-left: 25px;
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

export default function AdminPage() {
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
      component: ManageBlueprintImage(),
    },
    {
      label: 'FeeUpdate',
      component: ManageFeeUpdate(),
    },
    {
      label: 'InfusionMint',
      component: ManageInfusionMint(),
    },
    {
      label: 'InfusionUpdate',
      component: ManageInfusionUpdate(),
    },
  ]

  function ManageAdmin(manageAdminAction: ManageAdminAction) {
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

    const [loading, setLoading] = useState(true)
    const [adminList, setAdminList] = useState<string[]>([])
    const getAdmin = async () => {
      setLoading(true)
      try {
        const result = await new HardenedContract(network).Read(
          ReadMethod.GET_ADMIN
        )
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
      if (manageAdminAction == ManageAdminAction.GET) getAdmin() // Only get for one action.
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
            <InvokeButton isDisable={isDisable()} invoke={invoke} />
            <NotificationBox />
          </>
        )}
      </div>
    )
  }

  function ManageBlueprintImage() {
    const [loading, setLoading] = useState(true)
    const [blueprintImageUrl, setBlueprintImageUrl] = useState<string>('')
    const getBlueprintImageUrl = async () => {
      setLoading(true)
      try {
        const result = await new HardenedContract(network).Read(
          ReadMethod.GET_BLUEPRINT_IMAGE_URL
        )
        setBlueprintImageUrl(result)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.error(e)
      }

      setLoading(false)
    }

    const [inputImageUrl, setInputImageUrl] = useState('')
    const [isValidImageUrl, setIsValidImageUrl] = useState(true)
    const isDisable = () => {
      return !connectedWallet || !isValidImageUrl || inputImageUrl.length == 0
    }

    const handleImageUrlChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputImageUrl(value)
      if (value.length > 0) {
        setIsValidImageUrl(IMAGE_URL_PATTERN.test(value))
      } else {
        setIsValidImageUrl(true)
      }
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new HardenedContract(network).SetBlueprintImageUrl(
            connectedWallet,
            inputImageUrl
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

    useEffect(() => {
      getBlueprintImageUrl()
    }, [])

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <>
          {loading && <MessagePanel message="Loading" />}
          {!loading && (
            <Div
              style={{
                width: '450px',
                marginTop: '25px',
                marginLeft: '25px',
              }}
            >
              Current Blueprint Image Url:{' '}
              <span style={{ fontSize: '1.5em' }}>{blueprintImageUrl}</span>
            </Div>
          )}
        </>
        <>
          <TextField
            required
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Blueprint Image Url (Required)"
            helperText={
              isValidImageUrl
                ? 'Please input full path Url starting from https, only accept png or jpg'
                : 'Invalid url'
            }
            defaultValue=""
            value={inputImageUrl}
            onChange={handleImageUrlChange}
            error={!isValidImageUrl}
          />
          <InvokeButton isDisable={isDisable()} invoke={invoke} />
          <NotificationBox />
        </>
      </div>
    )
  }

  function ManageFeeUpdate() {
    const [inputWalletPoolHash, setInputWalletPoolHash] = useState('')
    const [isValidHash, setIsValidHash] = useState(true)

    const INPUT_B_TOKEN_MINT_COST_ID = 'input-b-token-mint-cost'
    const [inputBTokenMintCost, setInputBTokenMintCost] = useState('')
    const [isValidBTokenMintCost, setIsValidBTokenMintCost] = useState(true)

    const INPUT_B_TOKEN_UPDATE_COST_ID = 'input-b-token-update-cost'
    const [inputBTokenUpdateCost, setInputBTokenUpdateCost] = useState('')
    const [isValidBTokenUpdateCost, setIsValidBTokenUpdateCost] = useState(true)

    const INPUT_GAS_MINT_COST_ID = 'input-gas-mint-cost'
    const [inputGasMintCost, setInputGasMintCost] = useState('')
    const [isValidGasMintCost, setIsValidGasMintCost] = useState(true)

    const INPUT_GAS_UPDATE_COST_ID = 'input-gas-update-cost'
    const [inputGasUpdateCost, setInputGasUpdateCost] = useState('')
    const [isValidGasUpdateCost, setIsValidGasUpdateCost] = useState(true)

    const isDisable = () => {
      return (
        !connectedWallet ||
        !isValidHash ||
        inputWalletPoolHash.length == 0 ||
        !isValidBTokenMintCost ||
        inputBTokenMintCost.length == 0 ||
        !isValidBTokenUpdateCost ||
        inputBTokenUpdateCost.length == 0 ||
        !isValidGasMintCost ||
        inputGasMintCost.length == 0 ||
        !isValidGasUpdateCost ||
        inputGasUpdateCost.length == 0
      )
    }

    const handleWalletHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputWalletPoolHash(value)
      if (value.length > 0) {
        setIsValidHash(HASH160_PATTERN.test(value))
      } else {
        setIsValidHash(true)
      }
    }

    const handleNumberChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      switch (event.target.id) {
        case INPUT_B_TOKEN_MINT_COST_ID:
          setInputBTokenMintCost(value)
          if (value.length > 0) {
            setIsValidBTokenMintCost(NUMBER_PATTERN.test(value))
          } else {
            setIsValidBTokenMintCost(true)
          }
          break
        case INPUT_B_TOKEN_UPDATE_COST_ID:
          setInputBTokenUpdateCost(value)
          if (value.length > 0) {
            setIsValidBTokenUpdateCost(NUMBER_PATTERN.test(value))
          } else {
            setIsValidBTokenUpdateCost(true)
          }
          break
        case INPUT_GAS_MINT_COST_ID:
          setInputGasMintCost(value)
          if (value.length > 0) {
            setIsValidGasMintCost(NUMBER_PATTERN.test(value))
          } else {
            setIsValidGasMintCost(true)
          }
          break
        case INPUT_GAS_UPDATE_COST_ID:
          setInputGasUpdateCost(value)
          if (value.length > 0) {
            setIsValidGasUpdateCost(NUMBER_PATTERN.test(value))
          } else {
            setIsValidGasUpdateCost(true)
          }
          break
      }
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const feeStructure: IFeeStructure = {
            bTokenMintCost: Number(inputBTokenMintCost),
            bTokenUpdateCost: Number(inputBTokenUpdateCost),
            gasMintCost: Number(inputGasMintCost),
            gasUpdateCost: Number(inputGasUpdateCost),
            walletPoolHash: inputWalletPoolHash,
          }
          const txid = await new HardenedContract(network).FeeUpdate(
            connectedWallet,
            feeStructure
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

    const costHelperText =
      'Cost with BigInteger format. If the token use 8 decimal, then 1 token must input as 100000000'
    const costErrorText = 'Must be number only'

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <>
          <InputTextField
            id={INPUT_B_TOKEN_MINT_COST_ID}
            required
            label="B token mint cost (Required)"
            helperText={isValidBTokenMintCost ? costHelperText : costErrorText}
            defaultValue=""
            value={inputBTokenMintCost}
            onChange={handleNumberChange}
            error={!isValidBTokenMintCost}
          />
          <InputTextField
            id={INPUT_B_TOKEN_UPDATE_COST_ID}
            required
            label="B token update cost (Required)"
            helperText={
              isValidBTokenUpdateCost ? costHelperText : costErrorText
            }
            defaultValue=""
            value={inputBTokenUpdateCost}
            onChange={handleNumberChange}
            error={!isValidBTokenUpdateCost}
          />
          <InputTextField
            id={INPUT_GAS_MINT_COST_ID}
            required
            label="GAS mint cost (Required)"
            helperText={isValidGasMintCost ? costHelperText : costErrorText}
            defaultValue=""
            value={inputGasMintCost}
            onChange={handleNumberChange}
            error={!isValidGasMintCost}
          />
          <InputTextField
            id={INPUT_GAS_UPDATE_COST_ID}
            required
            label="Gas update cost (Required)"
            helperText={isValidGasUpdateCost ? costHelperText : costErrorText}
            defaultValue=""
            value={inputGasUpdateCost}
            onChange={handleNumberChange}
            error={!isValidGasUpdateCost}
          />
          <TextField
            required
            style={{
              width: '450px',
              marginTop: '25px',
              marginLeft: '25px',
            }}
            label="Wallet Pool Hash (Required)"
            helperText={
              isValidHash
                ? 'Pool wallet in Hash160 format start in 0x'
                : 'Invalid hash'
            }
            defaultValue=""
            value={inputWalletPoolHash}
            onChange={handleWalletHashChange}
            error={!isValidHash}
            inputProps={{ maxLength: 42 }}
          />
          <InvokeButton isDisable={isDisable()} invoke={invoke} />
          <NotificationBox />
        </>
      </div>
    )
  }

  function ManageInfusionMint() {
    const [inputClientPubKey, setInputClientPubKey] = useState('')
    const INPUT_CLIENT_PUBKEY_ID = 'input-client-pubkey-id'
    const [inputContractPubKey, setInputContractPubKey] = useState('')
    const INPUT_CONTRACT_PUBKEY_ID = 'input-contract-pubkey-id'

    const INPUT_CONTRACT_HASH = 'input-contract-hash'
    const [inputContractHash, setInputContractHash] = useState('')
    const [isValidContractHash, setIsValidContractHash] = useState(true)

    const INPUT_PAY_TOKEN_HASH = 'input-pay-token-hash'
    const [inputPayTokenHash, setInputPayTokenHash] = useState('')
    const [isValidPayTokenHash, setIsValidPayTokenHash] = useState(true)

    const [inputPayTokenAmount, setInputPayTokenAmount] = useState('')
    const [isValidPayTokenAmount, setIsValidPayTokenAmount] = useState(true)

    const [inputBase58Properties, setInputBase58Properties] = useState('')
    const INPUT_BASE58_PROPERTIES_ID = 'input-base58-properties-id'

    const isDisable = () => {
      return (
        !connectedWallet ||
        inputClientPubKey.length == 0 ||
        inputContractPubKey.length == 0 ||
        inputBase58Properties.length == 0 ||
        !isValidPayTokenHash ||
        inputPayTokenHash.length == 0 ||
        !isValidContractHash ||
        inputContractHash.length == 0 ||
        !isValidPayTokenAmount ||
        inputPayTokenAmount.length == 0
      )
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_CLIENT_PUBKEY_ID) {
        setInputClientPubKey(value)
      } else if (event.target.id == INPUT_CONTRACT_PUBKEY_ID) {
        setInputContractPubKey(value)
      } else if (event.target.id == INPUT_BASE58_PROPERTIES_ID) {
        setInputBase58Properties(value)
      }
    }

    const handleHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_PAY_TOKEN_HASH) {
        setInputPayTokenHash(value)
        if (value.length > 0) {
          setIsValidPayTokenHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidPayTokenHash(true)
        }
      } else if (event.target.id == INPUT_CONTRACT_HASH) {
        setInputContractHash(value)
        if (value.length > 0) {
          setIsValidContractHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidContractHash(true)
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

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const infusionMintObject: IInfusionMintObject = {
            clientPubKey: inputClientPubKey,
            contractPubKey: inputContractPubKey,
            contractHash: inputContractHash,
            payTokenHash: inputPayTokenHash,
            payTokenAmount: Number(inputPayTokenAmount),
            base58Properties: inputBase58Properties,
          }
          const txid = await new HardenedContract(network).InfusionMint(
            connectedWallet,
            infusionMintObject
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
        <TextField
          required
          id={INPUT_CONTRACT_HASH}
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="Contract Hash (Required for transfer PayToken to)"
          helperText={
            isValidPayTokenHash
              ? 'Contract in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          value={inputContractHash}
          onChange={handleHashChange}
          error={!isValidContractHash}
          inputProps={{ maxLength: 42 }}
        />
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
            isValidPayTokenHash
              ? 'Pay token in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          value={inputPayTokenHash}
          onChange={handleHashChange}
          error={!isValidPayTokenHash}
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
        <InputTextField
          multiline={true}
          id={INPUT_BASE58_PROPERTIES_ID}
          required
          label="Base58 Properties"
          value={inputBase58Properties}
          onChange={handleTextChange}
        />
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
      </div>
    )
  }

  function ManageInfusionUpdate() {
    const [inputClientPubKey, setInputClientPubKey] = useState('')
    const INPUT_CLIENT_PUBKEY_ID = 'input-client-pubkey-id'
    const [inputContractPubKey, setInputContractPubKey] = useState('')
    const INPUT_CONTRACT_PUBKEY_ID = 'input-contract-pubkey-id'

    const INPUT_USER_WALLET_HASH = 'input-user-wallet-hash'
    const [inputUserWalletHash, setInputUserWalletHash] = useState('')
    const [isValidUserWalletHash, setIsValidUserWalletHash] = useState(true)

    const INPUT_PAY_TOKEN_HASH = 'input-pay-token-hash'
    const [inputPayTokenHash, setInputPayTokenHash] = useState('')
    const [isValidPayTokenHash, setIsValidPayTokenHash] = useState(true)

    const [inputPayTokenAmount, setInputPayTokenAmount] = useState('')
    const [isValidPayTokenAmount, setIsValidPayTokenAmount] = useState(true)

    const [inputBase58Properties, setInputBase58Properties] = useState('')
    const INPUT_BASE58_PROPERTIES_ID = 'input-base58-properties-id'

    const isDisable = () => {
      return (
        !connectedWallet ||
        inputClientPubKey.length == 0 ||
        inputContractPubKey.length == 0 ||
        inputBase58Properties.length == 0 ||
        !isValidPayTokenHash ||
        inputPayTokenHash.length == 0 ||
        !isValidUserWalletHash ||
        inputUserWalletHash.length == 0 ||
        !isValidPayTokenAmount ||
        inputPayTokenAmount.length == 0
      )
    }

    const handleTextChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_CLIENT_PUBKEY_ID) {
        setInputClientPubKey(value)
      } else if (event.target.id == INPUT_CONTRACT_PUBKEY_ID) {
        setInputContractPubKey(value)
      } else if (event.target.id == INPUT_BASE58_PROPERTIES_ID) {
        setInputBase58Properties(value)
      }
    }

    const handleHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value

      if (event.target.id == INPUT_PAY_TOKEN_HASH) {
        setInputPayTokenHash(value)
        if (value.length > 0) {
          setIsValidPayTokenHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidPayTokenHash(true)
        }
      } else if (event.target.id == INPUT_USER_WALLET_HASH) {
        setInputUserWalletHash(value)
        if (value.length > 0) {
          setIsValidUserWalletHash(HASH160_PATTERN.test(value))
        } else {
          setIsValidUserWalletHash(true)
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

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const infusionUpdateObject: IInfusionUpdateObject = {
            clientPubKey: inputClientPubKey,
            contractPubKey: inputContractPubKey,
            userWalletHash: inputUserWalletHash,
            payTokenHash: inputPayTokenHash,
            payTokenAmount: Number(inputPayTokenAmount),
            base58Properties: inputBase58Properties,
          }
          const txid = await new HardenedContract(network).InfusionUpdate(
            connectedWallet,
            infusionUpdateObject
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
        <TextField
          required
          id={INPUT_USER_WALLET_HASH}
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="User Wallet Hash (Required for check with contract that the wallet is the same)"
          helperText={
            isValidPayTokenHash
              ? 'User Wallet in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          value={inputUserWalletHash}
          onChange={handleHashChange}
          error={!isValidUserWalletHash}
          inputProps={{ maxLength: 42 }}
        />
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
            isValidPayTokenHash
              ? 'Pay token in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          value={inputPayTokenHash}
          onChange={handleHashChange}
          error={!isValidPayTokenHash}
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
        <InputTextField
          multiline={true}
          id={INPUT_BASE58_PROPERTIES_ID}
          required
          label="Base58 Properties"
          value={inputBase58Properties}
          onChange={handleTextChange}
        />
        <InvokeButton isDisable={isDisable()} invoke={invoke} />
      </div>
    )
  }
  return <TabPanel pages={pages} />
}
