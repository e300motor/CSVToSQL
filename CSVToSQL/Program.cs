using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace CSVToSQL
{
    interface IEnvironment
    {
        bool ReadAllFiles { get; set; }
        string StoredDirectory { get; set; }

        void setCSVDirectory();
        void chooseAllCSVorNewCreatedFiles();
    }

    class Environment : IEnvironment
    {
        private static int VERSION = 1;
        private static int SUBVERSION = 0;
        private bool _isReadAllFiles;
        public bool ReadAllFiles
        {
            get { return _isReadAllFiles; }
            set { _isReadAllFiles = value; }
        }

        public string StoredDirectory
        {
            get { return Properties.Settings.Default.DirName; }
            set
            {
                Properties.Settings.Default.DirName = value;
                Properties.Settings.Default.Save();
            }
        }

        //return (Yes == false) to break loop.
        private bool askYesNo(string msg, string strAns)
        {
            Console.WriteLine("\n {0} => \"{1}\" : ", msg, strAns);
            Console.WriteLine("\n\t(Y)Yes: OK!");
            Console.WriteLine("\t(N)No: Choose again!");

            Console.Write("\n Type (N)No Or \"Enter\" To Default (Y)Yes: ");

            string answer = Console.ReadLine();
            if (string.IsNullOrEmpty(answer))
            {
                return false;
            }

            string ANS = answer.ToUpper();

            if (ANS == "Y" || ANS == "YES")
            {
                return false;
            }
            else if (ANS == "N" || ANS == "NO")
            {
                return true;
            }
            else
            {
                Console.WriteLine("ERROR! Please Retype Again!...");
                return true;
            }
        }

        private void showCopyrightAndVersion()
        {
            Console.WriteLine("\t2017/03 (C) Software CSVToSQL, Copyright Owner ITRI E300");
            Console.WriteLine("\t\tCSVToSQL VERSION:{0}.{1}", VERSION, SUBVERSION);
        }

        //Set the Directory of CSV-files.
        public void setCSVDirectory()
        {
            bool loopForNo = true;
            showCopyrightAndVersion();
            do
            {
  
                Console.Write("\nSelect New Directory Or Press \"Enter\" To Default ({0}):", StoredDirectory);

                string line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    line = StoredDirectory;
                }

                loopForNo = askYesNo("Your CSV-Directory: ", line);

                loopForNo = loopForNo || !Directory.Exists(line);
                if (!loopForNo) //Yes == false No == true ! Yes to Break the loop.
                {
                    StoredDirectory = line;
                    break;
                }
                Console.WriteLine("\n\n\t#Warning! Select Directory Again!");
            } while (loopForNo);
        }

        private void choosePreMsg()
        {
            Console.WriteLine("\nReading-Type Setting:");
            Console.WriteLine("\tRead the \"New-Created\" Subdirectory CSV-Files is (N)New.");
            Console.WriteLine("\tRead the \"All\" Subdirectory CSV-Files         is (A)All.");
            Console.Write("\n Type (A)All or Press \"Enter\" To Default (N)New: ");
        }

        public void chooseAllCSVorNewCreatedFiles()
        {
            bool loopForNo = true;
            do
            {
                choosePreMsg();
                string line = Console.ReadLine();
                string ANS = line.ToUpper();
                bool isAllFiles;

                if (string.IsNullOrEmpty(line) || ANS == "N" || ANS == "NEW")
                {
                    isAllFiles = false;
                    line = " New-Created";
                }
                else if (ANS == "A" || ANS == "ALL")
                {
                    isAllFiles = true;
                    line = "All";
                }
                else
                {
                    continue;
                }

                loopForNo = askYesNo("You choose: ", line);
                if (!loopForNo) //Yes!Got the answer. Break the loop.
                {
                    _isReadAllFiles = isAllFiles;
                    break;
                }
                Console.WriteLine("\n\n\t#Warning! Choose Yes Or No Again!");
            } while (loopForNo);
        }
    }

    enum DATABASE_FIELD
    {
        DATE = 1,
        SECTION = 4,
        RPM = 6,
        CURRENT = 8,
        TEMP = 10
    }

    enum SECTION_NO
    {
        SECTION_0,
        SECTION_1,
        SECTION_2,
        SECTION_3,
        SECTION_4,
        SECTION_5,
        SECTION_6,
        SECTION_7,
        SECTION_8
    }

    struct LiftSensorRaw
    {
        public long date;
        public int sec;
        public int rpm;
        public int current;
        public int temp;
    }

    interface IMapSensorData
    {
        int mapToSection(int input);
        int mapToRPM(int input);
        double mapToCurrent(int input);
        double mapToTemp(int input);
    }

    class MapSensorData : IMapSensorData
    {
        const int LEVEL_MAX = 32767;
        private double preMap(int value)
        {
            return ((value - LEVEL_MAX) * 10.0) / LEVEL_MAX;
        }

        public int mapToSection(int input)
        {
            double value = preMap(input);
            if (0.3 < value && value < 0.6)
            {
                return (int)SECTION_NO.SECTION_1;
            }
            else if (0.8 < value && value < 1.2)
            {
                return (int)SECTION_NO.SECTION_2;
            }
            else if (1.3 < value && value < 1.6)
            {
                return (int)SECTION_NO.SECTION_3;
            }
            else if (2.7 < value && value < 3.1)
            {
                return (int)SECTION_NO.SECTION_6;
            }
            else if (3.2 < value && value < 3.6)
            {
                return (int)SECTION_NO.SECTION_7;
            }
            else if (3.6 < value && value < 4.2)
            {
                return (int)SECTION_NO.SECTION_8;
            }

            return (int)SECTION_NO.SECTION_0;
        }
        public int mapToRPM(int input)
        {
            double value = preMap(input);
            return (int)(value * 1500);
        }
        public double mapToCurrent(int input)
        {
            double value = preMap(input);
            return value * 5;
        }
        public double mapToTemp(int input)
        {
            double value = preMap(input);
            return (value / 0.00833) - 100;
        }
    }

    class Program
    {

        private static CSVModel csvModel;

        static void Main(string[] args)
        {
            IEnvironment env = new Environment();
            csvModel = new CSVModel();

            #region Environment
            env.setCSVDirectory();
            env.chooseAllCSVorNewCreatedFiles();

            StringBuilder pool = new StringBuilder();
            pool.Append("Your CSV-Directory: ");
            pool.Append(env.StoredDirectory);
            pool.Append(" ; Reading-type: ");
            pool.Append(env.ReadAllFiles ? "All" : "New-Created");
            Console.WriteLine(pool.ToString());
            #endregion

            if (env.ReadAllFiles)
            {
                readAllSubDirFiles(env.StoredDirectory);
            }
            else
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                //Watch StoreDirectory.
                watcher.Path = @env.StoredDirectory;
                watcher.IncludeSubdirectories = true;

                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                    | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                //public delegate void FileSystemEventHandler(object sender,FileSystemEventArgs e)
                watcher.Created += new FileSystemEventHandler((s, e) => OnChanged(s, e, env));
                watcher.EnableRaisingEvents = true;
            }

            Console.WriteLine("Press Any Key to Exit...");
            Console.ReadKey();
        }
        //===============================================================


        private static LiftSensorRaw transformSensorRaw(string[] fieldData)
        {
            LiftSensorRaw raw;
            raw.date = Convert.ToInt64(fieldData[(int)DATABASE_FIELD.DATE]);
            raw.sec = Convert.ToInt32(fieldData[(int)DATABASE_FIELD.SECTION]);
            raw.rpm = Convert.ToInt32(fieldData[(int)DATABASE_FIELD.RPM]);
            raw.current = Convert.ToInt32(fieldData[(int)DATABASE_FIELD.CURRENT]);
            raw.temp = Convert.ToInt32(fieldData[(int)DATABASE_FIELD.TEMP]);
            return raw;
        }

        private static void transformRawToDB(LiftSensorRaw structRaw)
        {
            IMapSensorData mapSensorData = new MapSensorData();

            LiftChairs liftChairs = new LiftChairs
            {
                Date = (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(structRaw.date),
                Section = mapSensorData.mapToSection(structRaw.sec),
                RPM = mapSensorData.mapToRPM(structRaw.rpm),
                Current = mapSensorData.mapToCurrent(structRaw.current),
                Temp = mapSensorData.mapToTemp(structRaw.temp)
            };
            csvModel.LiftChairs.Add(liftChairs);
            csvModel.SaveChanges();
        }


        private static bool loopReading(string fullName)
        {
            FileInfo fileInfo = new FileInfo(fullName);
            if (fileInfo.IsReadOnly)
            {
                return false;
            }

            using (TextFieldParser csvReader = new TextFieldParser(fullName))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;

                //Give up Fields' name.
                string[] colFields = csvReader.ReadFields();

                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    try
                    {
                        LiftSensorRaw raw = transformSensorRaw(fieldData);
                        transformRawToDB(raw);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                fileInfo.IsReadOnly = true;
            }
            return true;
        }

        private static void importToDB(DirectoryInfo dirInfo)
        {
            loopReading(dirInfo.FullName);
        }

        private static void importToDB(string fullName)
        {
            if (loopReading(fullName))
            {
                Console.WriteLine("\tReads {0} End...", fullName);
            }
        }

        private static void readAllSubDirFiles(string dir)
        {
            string[] files = Directory.GetFiles(@dir, "*.csv", System.IO.SearchOption.AllDirectories);


            Console.WriteLine("\n\tReading All Subdirectory Files...Waiting!");
            foreach (var x in files)
            {
                importToDB(x);
            }

        }

        //  This method is called when a file is created, changed, or deleted.
        private static void OnChanged(object source, FileSystemEventArgs e, IEnvironment env)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Reset();
            //sw.Start();
            //  Show that a file has been created, changed, or deleted.
            WatcherChangeTypes wct = e.ChangeType;
            Console.WriteLine("Reading File {0} {1}", e.FullPath, wct.ToString());

            DirectoryInfo dirInfo = new DirectoryInfo(e.FullPath.ToString());

            importToDB(dirInfo);

            Console.WriteLine("{0} Finish!" ,e.Name );

            //sw.Stop();
            //string result1 = sw.Elapsed.TotalMilliseconds.ToString();
            //Console.WriteLine(result1);
        }
    }
}
