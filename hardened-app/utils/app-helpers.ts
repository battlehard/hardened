export const AddressShorten = (address: string) => {
  return address.substring(0, 6) + '...' + address.slice(-6)
}
