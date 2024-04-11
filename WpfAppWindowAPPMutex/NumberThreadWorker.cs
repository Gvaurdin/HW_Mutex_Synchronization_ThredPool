using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WpfAppWindowAPPMutex
{
    public class NumberThreadWorker
    {
        public struct Report
        {
            public string Name;
            public int countNumber;
            public long sizeFile;
            public List<string> fileContent;
        }

        private static List<string> fileNames;
        Random random;
        Mutex mutex;
        Mutex mutex2;
        Mutex mutex3;
        public event Action<string> WorkBegin;
        public event Action<string> WorkEnd;
        public static event Action<List<Report>> CreateReport;
        static List<Report> reportList;

        public NumberThreadWorker()
        {
            fileNames = new List<string>()
            {
            "ThreadGenerationNumbers.txt",
            "ThreadSearchPrimeNumbers.txt",
            "ThreadLastNumberSeven.txt"
            };
            random = new Random();
            mutex = new Mutex();
            reportList = new List<Report>();
        }

        public void Start(object numberOperation)
        {
            switch ((int)numberOperation)
            {
                case 0:
                    {
                        mutex.WaitOne();
                        GenerationNumbers();
                        mutex.ReleaseMutex();
                        break;
                    }
                case 1:
                    {
                        mutex.WaitOne();
                        SearchPrimeNumbers();
                        mutex.ReleaseMutex();
                        break;
                    }
                case 2:
                    {
                        mutex.WaitOne();
                        SearchlastNumberSeven();
                        mutex.ReleaseMutex();
                        break;
                    }
            }
        }

        private void SearchlastNumberSeven()
        {
            //mutex.WaitOne();
            WorkBegin(Thread.CurrentThread.Name + " begin work");
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
            WorkEnd(Thread.CurrentThread.Name + " end work");
            reportList.Add(CreateReportAboutFile(fileNames[2]));
            WorkEnd("Report is created");
            WorkEnd("End operation");
            CreateReport(reportList);
            //mutex.ReleaseMutex();
        }

        private void SearchPrimeNumbers()
        {
           // mutex.WaitOne();
            WorkBegin(Thread.CurrentThread.Name + " begin work");
            Thread.Sleep(3000);
            using (StreamWriter writer = new StreamWriter(fileNames[1], false))
            {
                using (StreamReader reader = new StreamReader(fileNames[0]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if(ulong.TryParse(line, out ulong number) & CheckPrimeNumber(number))
                        {
                            writer.WriteLine(number.ToString());
                        }
                    }
                }
            }
            WorkEnd(Thread.CurrentThread.Name + " end work");
            reportList.Add(CreateReportAboutFile(fileNames[1]));
            WorkEnd("Report is created");
            //mutex.ReleaseMutex();
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

        private Report CreateReportAboutFile(string file)
        {
            Report report = new Report();
            FileInfo fileInfo = new FileInfo(file);
            if(fileInfo.Exists)
            {
                report.Name = file.Substring(0,file.Length - 4);
                report.sizeFile = fileInfo.Length;
                report.countNumber = CountNumbersInFile(file);
                report.fileContent = GetFileContent(file);
            }
            return report;
        }

        private int CountNumbersInFile(string file)
        {
            int count = 0;
            using(StreamReader reader = new StreamReader(file))
            {
                while(reader.ReadLine() != null)
                {
                    count++;
                }
            }
            return count;
        }

        private List<string> GetFileContent(string file)
        {
            List<string> numbers = new List<string>();
            using(StreamReader reader = new StreamReader(file))
            {
                string line= "";
                int count = 0;
                while(reader.ReadLine() != null) 
                {
                    line += reader.ReadLine() + " , ";
                    count++;
                    if(count == 5)
                    {
                        numbers.Add(line);
                        count = 0;
                        line = "";
                    }
                }
            }
            return numbers;
        }



        private void GenerationNumbers()
        {
            //mutex.WaitOne();
            WorkBegin(Thread.CurrentThread.Name + " begin work");
            Thread.Sleep(3000);
            List<int> ints = new List<int>();
            for (int i = 0; i < 10000; i++)
            {
                int num = random.Next(1000);
                ints.Add(num);
            }
            using (StreamWriter writer = new StreamWriter(fileNames[0], false))
            {
                for (int i = 0; i < 10000; i++)
                {
                    writer.WriteLine(ints[i].ToString());
                }
            }
            WorkEnd(Thread.CurrentThread.Name + " end work");
            reportList.Add(CreateReportAboutFile(fileNames[0]));
            WorkEnd("Report is created");

            //mutex.ReleaseMutex();
        }
    }
}
