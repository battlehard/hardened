import { u, wallet } from '@cityofzion/neon-core'

export const stackJsonToObject = (item: any) => {
  let obj: any = {}

  if (item.value != null) {
    if (item.type && item.type === 'Array') {
      let value = item.value

      value = value.map((item: any) => stackJsonToObject(item))

      return value
    } else if (item.type && item.type === 'Map') {
      let value = item.value

      value = value.map((item: any) => stackJsonToObject(item))
      value = Object.assign({}, ...value)

      return value
    } else {
      let key = u.base642utf8(item.key.value)
      let value = item.value.value
      let valueType = item.value.type

      if (key === 'owner') {
        value = base64ToAddress(value)
      } else if (key === 'offerTokenHash' || key === 'purchaseTokenHash') {
        value = '0x' + base64ToHash160(value)
      } else {
        switch (valueType) {
          case 'ByteString':
            value = base64ToString(value)
            break
          case 'Integer':
            value = +value
            break
          case 'Array':
            value = value.map((item: any) => stackJsonToObject(item))
            break
          case 'Map':
            value = value.map((item: any) => stackJsonToObject(item))
            value = Object.assign({}, ...value)
            break
          default:
            break
        }
      }
      obj[key] = value
    }
    return obj
  }
}

export const base64ToAddress = (str: string) =>
  wallet.getAddressFromScriptHash(base64ToHash160(str))

export const base64ToHash160 = (str: string) => u.reverseHex(u.base642hex(str))

export const base64ToString = (str: string) =>
  u.HexString.fromBase64(str).toAscii().toString()

export const getBigIntegerForm = (amount: number, decimal: number) => {
  return u.BigInteger.fromDecimal(amount, decimal)
}

export const getDecimalForm = (amount: number, decimal: number) => {
  return amount / (1 * Math.pow(10, decimal))
}
