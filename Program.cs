using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;

namespace RockPaper
{
    internal class Program
    {
        private static bool CheckRepeatingItems(string[] handsigns)
        {
            var n = (string[]) handsigns.Clone();
            for (var i = 1; i < n.Length; i++)
                if (n[i] == n[i - 1])
                {
                    Console.WriteLine(n[i]);
                    return false;
                }

            return true;
        }

        private static bool WinOrLose(IEnumerable<int> winList, int computerTurn)
        {
            foreach (var handSign in winList)
                if (handSign == computerTurn - 1)
                    return true;

            return false;
        }

        private static byte[] GetRandomKey()
        {
            var rngCrypt = new RNGCryptoServiceProvider();
            var tokenBuffer = new byte[128];
            rngCrypt.GetBytes(tokenBuffer);
            return tokenBuffer;
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        
        private static byte[] HexDecode(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }
        private static byte[] HashHMAC(byte[] key, string message)
        {   
            var ba = Encoding.Default.GetBytes(message);
            var hexString = BitConverter.ToString(ba);
            hexString = hexString.Replace("-", "");
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(HexDecode(hexString));
        }

        public static void Main(string[] args)
        {
            var handsigns = args;
            while (true)
            {
                string strArgs;

                if (handsigns.Length == 0 || handsigns.Length == 1)
                {
                    Console.WriteLine("Enter handsigns (e.g., Rock Paper Scissors)");
                    strArgs = Console.ReadLine();
                    handsigns = strArgs.Split(' ');
                    continue;
                }

                if (handsigns.Length % 2 == 0)
                {
                    Console.WriteLine("Enter an odd number of handsigns (e.g., Rock Paper Scissors )");
                    strArgs = Console.ReadLine();
                    handsigns = strArgs.Split(' ');
                    continue;
                }

                if (CheckRepeatingItems(handsigns) == false)
                {
                    Console.WriteLine(
                        "Oops, it looks like some handsigns are duplicated. Please enter correct information.");
                    strArgs = Console.ReadLine();
                    handsigns = strArgs.Split(' ');
                    continue;
                }

                break;
            }

            var secureKey = GetRandomKey();
            var rnd = new Random(BitConverter.ToInt32(secureKey, 0));
            var computerTurn = rnd.Next(1, handsigns.Length + 1);
            var hmac = HashEncode(HashHMAC(secureKey, handsigns[computerTurn - 1]));

            Console.WriteLine("HMAC:{0}", hmac);
            var hmacKey = HashEncode(secureKey);

            Console.WriteLine("Available moves:");
            for (var i = 1; i <= handsigns.Length; i++)
            {
                Console.WriteLine("{0} - {1}", i, handsigns[i - 1]);
            }

            Console.WriteLine("0 - Exit\nEnter your move: ");
            int userMove;
            while (true)
            {
                var enterTurn = Console.ReadLine();
                if (int.TryParse(enterTurn, out userMove))
                {
                    if (userMove == 0)
                    {
                        return;
                    }

                    if (userMove > 0 && userMove <= handsigns.Length)
                    {
                        break;
                    }
                }
                else Console.WriteLine("Invalid Enter");
            }

            var winList = new List<int>();
            for (int i = userMove - 2, k = 0; k < handsigns.Length / 2; --i, k++)
            {
                if (i < 0)
                {
                    winList.Add(handsigns.Length + i);
                }
                else
                {
                    winList.Add(i);
                }
            }

            Console.WriteLine(
                "Your move: {0}\nComputer move: {1}",
                handsigns[userMove - 1],
                handsigns[computerTurn - 1]
            );
            if (userMove == computerTurn) Console.WriteLine("Draw");
            else if (WinOrLose(winList, computerTurn)) Console.WriteLine("You Win!");
            else Console.WriteLine("You Defeat!");
            Console.WriteLine("HMAC key: {0}", hmacKey);
        }
    }
}
