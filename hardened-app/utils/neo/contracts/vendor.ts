import { GAS_SCRIPT_HASH, MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { WalletAPI } from '../wallet'
import { stackJsonToObject } from '../helpers'
import { Network } from '../network'
import { wallet as NeonWallet, tx } from '@cityofzion/neon-core'
import { MAX_PAGE_LIMIT } from '@/components/constant'

export enum WhiteListAction {
  ADD = 'Add',
  REMOVE = 'Remove',
}

export const VENDOR_SCRIPT_HASH = {
  [TESTNET]: '0x2792f6c37e165bbe7ca2a210689dc73136446e2c',
  [MAINNET]: '',
}

export interface ITradeProperties {
  id: number
  owner: string
  offerTokenHash: string
  offerTokenSymbol: string
  offerTokenImageUrl: string
  offerTokenAmount: number
  offerPackages: number
  amountPerPackage: number
  purchaseTokenHash: string
  purchasePrice: number
  soldPackages: number
}

export interface ITradeListPagination {
  totalPages: number
  totalTrades: number
  tradeList: ITradeProperties[]
}

export class VendorContract {
  network: INetworkType
  contractHash: string

  constructor(networkType: INetworkType) {
    this.network = networkType
    this.contractHash = VENDOR_SCRIPT_HASH[networkType]
  }

  ModifyAdminWhiteList = async (
    connectedWallet: IConnectedWallet,
    adminWalletHash: string,
    action: WhiteListAction
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: `${action.toLowerCase()}AdminWhiteList`,
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: adminWalletHash,
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  ModifyOfferTokenWhiteList = async (
    connectedWallet: IConnectedWallet,
    tokenContractHash: string,
    symbol: string,
    imageUrl: string,
    action: WhiteListAction
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: `${action.toLowerCase()}OfferTokenWhiteList`,
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: tokenContractHash,
        },
      ],
    }

    if (action == WhiteListAction.ADD) {
      invokeScript.args?.push(
        ...[
          {
            type: 'String',
            value: symbol,
          },
          {
            type: 'String',
            value: imageUrl,
          },
        ]
      )
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  AdminCancelTrade = async (
    connectedWallet: IConnectedWallet,
    tradeId: number
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'adminCancelTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Integer',
          value: tradeId,
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  CreateTrade = async (
    connectedWallet: IConnectedWallet,
    offerTokenHash: string,
    offerTokenAmount: number,
    offerPackages: number,
    purchaseTokenHash: string,
    purchasePrice: number
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'createTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: offerTokenHash,
        },
        {
          type: 'Integer',
          value: offerTokenAmount,
        },
        {
          type: 'Integer',
          value: offerPackages,
        },
        {
          type: 'Hash160',
          value: purchaseTokenHash,
        },
        {
          type: 'Integer',
          value: purchasePrice,
        },
      ],
      signers: [
        {
          account: NeonWallet.getScriptHashFromAddress(
            connectedWallet.account.address
          ),
          scopes: tx.WitnessScope.CustomContracts,
          // This scope allow the contract transfer tokens out from user's wallet
          allowedContracts: [
            GAS_SCRIPT_HASH, // Give permission to transfer fee from user's wallet to contract
            offerTokenHash, // Give permission to asset that need to transfer from user's wallet to contract
          ],
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  ListTrade = async (
    currentPage: number = 1,
    pageItemsLimit: number = MAX_PAGE_LIMIT
  ): Promise<ITradeListPagination> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'listTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Integer',
          value: currentPage,
        },
        {
          type: 'Integer',
          value: pageItemsLimit,
        },
      ],
    }

    const res = await Network.read(this.network, [invokeScript])
    return stackJsonToObject(res.stack[0])
  }

  ExecuteTrade = async (
    connectedWallet: IConnectedWallet,
    tradeId: number,
    purchaseTokenHash: string,
    purchasePackages: number
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'executeTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Integer',
          value: tradeId,
        },
        {
          type: 'Integer',
          value: purchasePackages,
        },
      ],
      signers: [
        {
          account: NeonWallet.getScriptHashFromAddress(
            connectedWallet.account.address
          ),
          scopes: tx.WitnessScope.CustomContracts,
          // This scope allow the contract transfer tokens out from user's wallet
          allowedContracts: [
            purchaseTokenHash, // Give permission to asset that need to transfer from user's wallet to contract
          ],
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }
}
