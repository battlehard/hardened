using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using static Hardened.Helpers;
using System;

namespace Hardened
{
  public static class Extensions
  {
    public static UInt160 AddressToScriptHash(this string address)
    {
      if (address.ToByteArray()[0] == 0x4e) // N3 address start with 'N'
      {
        var decoded = (byte[])StdLib.Base58CheckDecode(address);
        var scriptHash = (UInt160)decoded.Last(20);
        ValidateScriptHash(scriptHash);
        return scriptHash;
      }
      else
      {
        throw new Exception(ADDRESS_INVALID);
      }
    }
    public static UInt160 HexStringToUInt160(this string hexString)
    {
      // remove prefix 0x
      if (hexString[0] == '0' && hexString[1] == 'x')
      {
        hexString = hexString.Substring(2);
      }

      // Check if the input string has an odd length
      if (hexString.Length % 2 != 0)
      {
        throw new Exception("Hexadecimal string must have an even number of characters.");
      }

      // Create a byte array to hold the result
      byte[] byteArray = new byte[hexString.Length / 2];

      // Convert each pair of hexadecimal characters to a byte
      for (int i = 0; i < hexString.Length; i += 2)
      {
        byte upperNibble = GetHexValue(hexString[i]);
        byte lowerNibble = GetHexValue(hexString[i + 1]);
        byteArray[i / 2] = (byte)((upperNibble << 4) | lowerNibble);
      }
      // Reverse sequence to construct UInt160
      byte[] hashByteArray = new byte[byteArray.Length];
      for (int i = 0; i < byteArray.Length; i++)
      {
        hashByteArray[i] = byteArray[byteArray.Length - i - 1];
      }
      UInt160 hash = (UInt160)hashByteArray;
      Assert(hash.IsValid, "Invalid input");
      return hash;
    }
    private static byte GetHexValue(char hexChar)
    {
      if (hexChar >= '0' && hexChar <= '9')
      {
        return (byte)(hexChar - '0');
      }
      else if (hexChar >= 'A' && hexChar <= 'F')
      {
        return (byte)(hexChar - 'A' + 10);
      }
      else if (hexChar >= 'a' && hexChar <= 'f')
      {
        return (byte)(hexChar - 'a' + 10);
      }
      else
      {
        throw new Exception("Invalid hexadecimal character.");
      }
    }
  }
}