using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfAppWindowAPPMutex
{
    public class NumberThreadWorker
    {
        public struct Report
        {
            public int countNumber;
            public long sizeFile;
            public string fileContent;
        }

        private static List<string> fileNames;
        Random random;
        Mutex[] mutexes;
        public event Action<string> WorkBegin;
        public event Action<string> WorkEnd;
        public NumberThreadWorker()
        {
            fileNames = new List<string>()
            {
            "ThreadGenerationNumbers.txt",
            "ThreadSearchPrimeNumbers.txt",
            "ThreadLastNumberSeven.txt"
            };
            random = new Random();
            mutexes = new Mutex[fileNames.Count];
            for (int i = 0;i < mutexes.Length; i++) 
            {
                mutexes[i] = new Mutex();
            }
        }

        public void Start(object numberOperation)
        {
            switch ((int)numberOperation) 
            {
                case 0:
                    {
                        mutexes[0].WaitOne();
                        GenerationNumbers();
                        mutexes[0].ReleaseMutex();
                        break;
                    }
                case 1:
                    {
                        mutexes[1].WaitOne();
                        SearchPrimeNumbers();
                        mutexes[1].ReleaseMutex();
                        break;
                    }
                case 2:
                    {
                        mutexes[2].WaitOne();
                        SearchlastNumberSeven();
                        mutexes[2].ReleaseMutex();
                        break;
                    }
            }
        }

        private void SearchlastNumberSeven()
        {
            WorkBegin(Thread.CurrentThread.Name + "begin work");
            Thread.Sleep(3000);
            using (StreamWriter writer = new StreamWriter(fileNames[2], false))
            {
                using (StreamReader reader = new StreamReader(fileNames[1]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.EndsWith("7"))
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            WorkEnd(Thread.CurrentThread.Name + "end work");
            WorkEnd("End operation");
        }

        private void SearchPrimeNumbers()
        {
            WorkBegin(Thread.CurrentThread.Name + "begin work");
            Thread.Sleep(3000);
            using (StreamWriter writer = new StreamWriter(fileNames[1], false))
            {
                using (StreamReader reader = new StreamReader(fileNames[0]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if(ulong.TryParse(line, out ulong number) && CheckPrimeNumber(number))
                        {
                            writer.WriteLine(number.ToString());
                        }
                    }
                }
            }
            WorkEnd(Thread.CurrentThread.Name + "end work");
        }

        private bool CheckPrimeNumber(ulong number)
        {
            if (number < 2) return false;
            for(ulong i = 2; i < number; i++)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        private void GenerationNumbers()
        {
            WorkBegin(Thread.CurrentThread.Name + "begin work");
            Thread.Sleep(3000);
            using (StreamWriter writer = new StreamWriter(fileNames[0], false))
            {
                for (int i = 0; i < int.MaxValue; i++)
                {
                    int num = random.Next(10000);
                    writer.WriteLine(num.ToString());
                }
            }
            WorkEnd(Thread.CurrentThread.Name + "end work");
        }
    }
}
