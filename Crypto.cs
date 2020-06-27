using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.CodeDom;
using System.Net;
using System.ComponentModel;
using System.Security.Policy;

namespace CryptoSystem
{
    class Crypto
    {
        private BigInt PrivateKey;
        private BigInt PublicKey;
        private static int g = 3;
        private static int g1 = 7;
        public static ulong AreaPrime = 14295028729954775939UL;

        public Crypto(string data)
        {
            GenerateKeys(out this.PrivateKey, out this.PublicKey, data);
        }

        public Crypto(BigInt PublicKey)
        {
            this.PrivateKey = null;
            this.PublicKey = PublicKey;
        }

        public Crypto(BigInt PublicKey, BigInt PrivateKey)
        {
            this.PrivateKey = PrivateKey;
            this.PublicKey = PublicKey;
        }

        public BigInt[] GetKeys()
        {
            BigInt[] keys = { this.PrivateKey, this.PublicKey };
            return keys;
        }

        public static void GenerateKeys(out BigInt PrivateKey, out BigInt PublicKey, string info)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(info);
            byte[] hash = SHA256Managed.Create().ComputeHash(bytes);
            uint seed = ((((uint)hash[0] * 256) + (uint)hash[1]) * 256 + (uint)hash[2]) * 256 + (uint)hash[3];
            Random rd = new Random((int)seed);
            ulong pvk = (ulong)Math.Floor(rd.NextDouble() * AreaPrime);
            PrivateKey = new BigInt(pvk);
            PublicKey = g ^ PrivateKey;
            Console.WriteLine(PrivateKey.ToString());
            Console.WriteLine(PublicKey.ToString());
        }

        public string[] EncodeData(string data)
        {
            BigInt bi = BigInt.HexCode(data);
            Random rd = new Random();
            BigInt[] C = new BigInt[2];
            ulong k = (ulong)Math.Floor(rd.NextDouble() * AreaPrime);
            BigInt K = new BigInt(k);
            C[0] = g ^ K;
            C[1] = bi * (this.PublicKey ^ K);
            string[] s = new string[2];
            s[0] = C[0].ToString();
            s[1] = C[1].ToString();
            return s;
        }

        public BigInt[] Encode(string data)
        {
            BigInt[] BI = BigInt.StringToBigint(data);
            BigInt[] C = new BigInt[BI.Length * 2];
            for(int i = 0; i < BI.Length; i++)
            {
                Random rd = new Random();
                ulong k = (ulong)Math.Floor(rd.NextDouble() * AreaPrime);
                BigInt K = new BigInt(k);
                C[2 * i] = g ^ K;
                C[2 * i + 1] = BI[i] * (this.PublicKey ^ K);
            }
            return C;
        }

        public string EncodeFile(string Filename, string OutputFilename)
        {
            UTF8Encoding utf8 = new UTF8Encoding(false);
            try
            {
                using (StreamReader sr = new StreamReader(Filename, utf8))
                {
                    string data = sr.ReadToEnd();
                    byte[] output = BigInt.ToUTF8(Encode(data));
                    int len = output.Length;
                    using (BinaryWriter sw = new BinaryWriter(new FileStream(OutputFilename,FileMode.Create)))
                    {
                        sw.Write(len);
                        sw.Write(output);
                    }
                    string s = "";
                    string v = "";
                    BigInt[] o = BigInt.StringToBigint(data);
                    BigInt[] p = Encode(data);
                    foreach (BigInt bi in o)
                    {
                        s += " " + bi.ToString();
                    }
                    foreach (BigInt bi in p)
                    {
                        v += " " + bi.ToString();
                    }
                    return "密文消息：\n" + Encoding.UTF8.GetString(output);
                }
            }
            catch(Exception e)
            {
                return "发生异常："+e.Message;
            }
        }

        public string DecodeData(string[] s)
        {
            BigInt[] C = new BigInt[2];
            C[0] = BigInt.HexCode(s[0]);
            C[1] = BigInt.HexCode(s[1]);
            BigInt reverse = new BigInt(AreaPrime - 2);
            string m = (C[1] * ((C[0] ^ this.PrivateKey) ^ reverse)).ToString();
            return m.Trim();
        }

        public string Decode(BigInt[] C)
        {
            string m = "";
            BigInt reverse = new BigInt(AreaPrime - 2);
            for(int i = 0; i < C.Length; i += 2)
            {
                if (C.Length <= (i + 1)) break;
                m += (C[i+1] * ((C[i] ^ this.PrivateKey) ^ reverse)).ToUTF8();
            }
            return m.Trim();
        }

        public string DecodeFile(string Filename, string OutputFilename)
        {
            UTF8Encoding utf8 = new UTF8Encoding(false);
            try
            {
                using (BinaryReader sr = new BinaryReader(new FileStream(Filename, FileMode.Open)))
                {
                    int len = sr.ReadInt32();
                    byte[] data = sr.ReadBytes(len);
                    BigInt[] input = BigInt.fromUTF8(data);
                    string output = Decode(input);
                    using (StreamWriter sw = new StreamWriter(OutputFilename, false, utf8))
                    {
                        sw.Write(output);
                    }
                    return "消息明文：\n" + output;
                }
            }
            catch (Exception e)
            {
                return "发生异常：" + e.Message + e.StackTrace;
            }
        }

        public static ulong Hash(string data)
        {
            ulong hashedValue = AreaPrime;
            for (int i = 0; i < data.Length; i++)
            {
                hashedValue += data[i];
                hashedValue *= AreaPrime;
            }
            return hashedValue;
        }
    }

    public class BigInt
    {
        private ulong number;
        
        public BigInt()
        {
            this.number = 0;
        }

        public BigInt(byte[] number)
        {
            this.number = 0;
            for(int i = 0; i < 6; i++)
            {
                this.number *= 256;
                this.number += number[i];
            }
        }

        public BigInt(int number)
        {
            this.number = (ulong)number;
        }

        public BigInt(uint number)
        {
            this.number = (ulong)number;
        }

        public BigInt(ulong number)
        {
            this.number = number;
        }
        public BigInt(BigInteger number)
        {
            this.number = (ulong)number;
        }

        public BigInt(string number)
        {
            this.number = Convert.ToUInt64(number);
        }

        public static BigInt Random()
        {
            Random rd = new Random();
            ulong k = (ulong)Math.Floor(rd.NextDouble() * Crypto.AreaPrime);
            BigInt K = new BigInt(k);
            return K;
        }

        public static BigInt[] StringToBigint(string data)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(data);
            if (bytes.Length % 6 != 0)
            {
                int l = bytes.Length / 6;
                byte[] buffer = new byte[(l + 1) * 6];
                bytes.CopyTo(buffer, 0);
                bytes = buffer;
            }
            BigInt[] BI = new BigInt[bytes.Length / 6];
            for(int i = 0; i < bytes.Length / 6; i++)
            {
                byte[] bit = { bytes[i * 6], bytes[i * 6 + 1], bytes[i * 6 + 2], bytes[i * 6 + 3], bytes[i * 6 + 4], bytes[i * 6 + 5] };
                BI[i] = new BigInt(bit);
            }
            return BI;
        }
        public static BigInt operator +(BigInt a, BigInt b)
        {
            BigInteger A = new BigInteger(a.number);
            BigInteger B = new BigInteger(b.number);
            BigInteger C = A + B;
            if (C >= Crypto.AreaPrime) C -= Crypto.AreaPrime;
            BigInt c = new BigInt(C);
            return c;
        }

        public static BigInt operator *(BigInt a, BigInt b)
        {
            BigInteger A = new BigInteger(a.number);
            BigInteger B = new BigInteger(b.number);
            BigInteger C = A * B;
            if (C >= Crypto.AreaPrime) C %= Crypto.AreaPrime;
            BigInt c = new BigInt(C);
            return c;
        }

        public static BigInt operator ^(int a, BigInt b)
        {
            BigInt A = new BigInt(a);
            BigInt C = new BigInt(BigInteger.ModPow(A.number, b.number,new BigInteger(Crypto.AreaPrime)));
            return C;
        }
        public static BigInt operator ^(BigInt a, BigInt b)
        {
            BigInt C = new BigInt(BigInteger.ModPow(a.number, b.number, new BigInteger(Crypto.AreaPrime)));
            return C;
        }

        public override string ToString()
        {
            string hex = this.number.ToString("X");
            string head = "0x";
            for(int i = hex.Length; i<16; i++)
            {
                head += '0';
            }
            return head + hex;
        }

        public string ToUTF8()
        {
            byte[] bytes = new byte[6];
            ulong number = this.number;
            for(int i = 5; i >= 0; i--)
            {
                bytes[i] = (byte)(number % 256);
                number /= 256;
            }
            string str = new UTF8Encoding().GetString(bytes);
            return str;
        }

        public static byte[] ToUTF8(BigInt[] BI)
        {
            byte[] bytes = new byte[BI.Length * 8];
            int count = 0;
            foreach (BigInt bi in BI)
            {
                ulong bv = bi.number;
                for (int i = 7; i >= 0; i--)
                {
                    bytes[count * 8 + i] = (byte)(bv % 256);
                    bv /= 256;
                }
                count++;
            }
            return bytes;
        }

        public static BigInt[] fromUTF8(byte[] bytes)
        {
            BigInt[] BI = new BigInt[bytes.Length / 8];
            for (int i = 0;i < BI.Length;i++)
            {
                ulong bv = 0;
                for(int j = 0; j < 8; j++)
                {
                    bv *= 256;
                    bv += bytes[i * 8 + j];
                }
                BI[i] = new BigInt(bv);
            }
            return BI;
        }

        public static BigInt HexCode(string data)
        {
            ulong number = (ulong)Convert.ToInt64(data.Split('x')[1], 16);
            BigInt output = new BigInt(number);
            return output;
        }
    } 
}
