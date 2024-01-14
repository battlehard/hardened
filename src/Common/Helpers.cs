using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;

namespace Hardened
{
  public class Helpers
  {
    private const string ADDRESS_INVALID = "The address is invalid";
    private const string UUID_INVALID = "The UUID is invalid";

    public static void ValidateScriptHash(UInt160 scriptHash)
    {
      Assert(scriptHash is not null && scriptHash.IsValid, ADDRESS_INVALID);
    }

    public static void ValidateUuid(string uuid)
    {
      Assert(IsUuid(uuid), UUID_INVALID);
    }

    public static void Assert(bool condition, string errorMessage)
    {
      if (!condition)
      {
        throw new Exception(errorMessage);
      }
    }

    public static UInt160 ToScriptHash(string address)
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

    public static string GenerateIdBase64(int bytesLength)
    {
      Assert(bytesLength > 0 && bytesLength <= 16, "Error: bytesLength must be 1 to 16");
      return StdLib.Base64Encode((ByteString)Runtime.GetRandom().ToByteArray().Range(0, bytesLength));
    }

    public static string GenerateUuidV4()
    {
      // Populate the byte array with random values
      byte[] guidBytes = Runtime.GetRandom().ToByteArray().Range(0, 16);

      // Set the version (4) and variant bits (RFC 4122)
      guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
      guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

      // Convert the byte array to a string representation
      return BytesToHexString(guidBytes);
    }

    static string BytesToHexString(byte[] byteArray)
    {
      string resultBuilder = "";
      for (int i = 0; i < byteArray.Length; i++)
      {
        int value = byteArray[i];
        // Convert byte to hexstring by preserve upper 4 bits and lower 4 bits then convert to char separately
        resultBuilder += $"{GetHexChar(value >> 4)}{GetHexChar(value & 0xF)}";
      }
      return resultBuilder;
    }

    static string GetHexChar(int value)
    {
      // Get hexstring represent of value, if less than 10 then output as-is
      // If 10 or larger the Itoa will return with prefix 0 like this 0a, so need to get only last index.
      string result = value < 10 ? StdLib.Itoa(value) : StdLib.Itoa(value, 16).Substring(1);
      return result;
    }

    public static bool IsUuid(string input)
    {
      // Ensure the input is exactly 32 characters long (no hyphens)
      if (input.Length != 32)
      {
        return false;
      }

      // Check if all characters are valid hexadecimal characters
      foreach (char c in input)
      {
        if (!IsHexChar(c))
        {
          return false;
        }
      }

      return true;
    }

    static bool IsHexChar(char c)
    {
      return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }

    public static bool IsEmpty(string input)
    {
      if (input == null) return true;
      else if (input.Length == 0) return true;
      else return false;
    }
  }
}