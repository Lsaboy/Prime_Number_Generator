//Vedant Sharma

using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace CustomExtensions{
    public static class CustomBigIntegerExtensions {
        public static bool IsProbablyPrime(this BigInteger value, int k = 10)
        {
            if(value < 2 || value % 2 == 0)
                return false;
            if(value <= 3)
                return true;
 
            BigInteger d = value - 1;
            int power = 0;
 
            while(d % 2 == 0)
            {
                d /= 2;
                power += 1;
            }
 
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[value.ToByteArray().LongLength];
            BigInteger random_number;
 
            for(int i = 0; i < k; i++)
            {
                do
                {
                    rng.GetBytes(bytes);
                    random_number = new BigInteger(bytes);
                }
                while(random_number < 2 || random_number >= value - 2);
 
                BigInteger a = BigInteger.ModPow(random_number, d, value);
 
                for(int r = 1; r < power; r++)
                {
                    a = BigInteger.ModPow(a, 2, value);
                    if(a == 1)
                        return false;
                    if(a == value - 1)
                        break;
                }
 
                if(a != value - 1)
                    return false;
            }
 
            return true;
        }
    }
}

namespace PrimeGen
{ 
    using CustomExtensions;
    public class PrimeGen
    {
        private static RandomNumberGenerator rng;
        private static object myLock = new object();
        private static int outputCounter, numberOfPrimes;
        private static String usage = "dotnet run <bits> <count=1> \n" +
                                      "\t- bits - the number of bits of the prime number, this must be a multiple of 8, and at least 32 bits.\n"+
                                      "\t- count - the number of prime numbers to generate, defaults to 1";

        static void Main(string[] args)
        {
            var sp = Stopwatch.StartNew();
            rng = RandomNumberGenerator.Create();
            outputCounter = 0;
            numberOfPrimes = 1;
            int bits;
            var tasks = new List<Task>();
            if (args.Length == 0 || !Int32.TryParse(args[0], out bits) || bits < 32 || bits % 8 != 0)
            {
                Console.WriteLine(usage);
                return;
            }

            Console.WriteLine("BitLength: "+ bits+ " bits");
            
            if (args.Length > 1 && !Int32.TryParse(args[1], out numberOfPrimes))
            {
                Console.WriteLine(usage);                
                return;
            }
            
            if (bits < 512)
            {
                for(int i = 0; i < numberOfPrimes; i++)
                    tasks.Add(Task.Run(() => PrimeGenerator(bits)));
            }
            else if(bits < 2048)
            {
                for(int i = 0; i < numberOfPrimes*3; i++)
                    tasks.Add(Task.Run(() => PrimeGenerator(bits)));
            }
            else
            {
                for(int i = 0; i < numberOfPrimes*5; i++)
                    tasks.Add(Task.Run(() => PrimeGenerator(bits)));
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Time to Generate: "+ sp.Elapsed);
            sp.Stop();
            
        }
        
        private static void PrimeGenerator(int bits)
        {
            byte[] bytes = new byte[bits/8];
            BigInteger random_number;
            do
            {
                rng.GetBytes(bytes);
                random_number = new BigInteger(bytes);
            } while (!random_number.IsProbablyPrime() && outputCounter < numberOfPrimes);
            
            if (outputCounter >= numberOfPrimes)
                return;
            
            lock (myLock)
            {
                outputCounter++;
                Console.WriteLine(outputCounter+": "+random_number);
                if (outputCounter < numberOfPrimes)
                    Console.WriteLine("");
            }
        }
    }
}