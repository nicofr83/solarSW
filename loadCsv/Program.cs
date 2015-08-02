using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadWriteCsv;
using System.IO;

namespace loadCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: loadCsv site_name");
                return;
            }
            loadData ld = new loadData();
            ld.run(args[0]);
        }
    }
    class loadData
    {
        Dictionary<String, String> transcodName = new Dictionary<string, string>()
        {
        //               {"4530432", "7"},
        //               {"4609792", "8"},
        //               {"4529920", "9"},
            // from site nortHillFarm
                {"6627584", "-1"},//1
                {"4529920","-1"},//2
                {"2432512","-1"},//3
                {"4609792","-1"},
                {"2506496","-1"},
                {"4608512","-1"},
                {"4608256","-1"},
                {"4608000","-1"},
                {"4606464","-1"},
                {"4606208","-1"},
                {"4605952","-1"},
                {"4530432","-1"}
        };

        List<String[]> paramDef = new List<String[]> {
            new string [] {"dc_Current", "DcMs", "Amp", "", "1", "A", "1"},
            new string [] {"dc_Voltage","DcMs", "Vol", "", "2", "V", "1"},
            new string [] {"dc_Power", "DcMs", "Watt", "", "3", "W", "1"},
            new string [] {"environmentTemp", "Env", "TmpVal", "", "4", "C", "1"},
            new string [] {"envirnomentTotalInsolation", "Env", "TotInsol", "", "5", "w/m2", "1"},
            new string [] {"envirnomentWindspeed", "Env", "HorWSpd", "", "6", "m/s", "1"},
            new string [] {"current", "GridMs", "A", "", "7", "A", "1"},
            new string [] {"frequency", "GridMs", "Hz", "", "8", "Hz", "1"},
            new string [] {"voltage", "GridMs", "PhV", "", "9", "V", "1"},
            new string [] {"activePower", "GridMs", "TotW", "", "10", "W", "1"},
            new string [] {"mod_Temp", "Mdul", "TmpVal", "", "11", "C", "0"},
            new string [] {"time_FeedIn", "Metering", "TotFeedTms", "", "12", "?", "0"},
            new string [] {"time_Operating", "Metering", "TotOpTms", "", "13", "h", "0"},
            new string [] {"Out_EnergyDaily", "Metering", "TotWhOut", "", "14", "W", "1"},
            new string [] {"evt_Description", "Operation", "Evt", "Dsc", "15", "String", "0"},
            new string [] {"evt_No", "Operation", "Evt", "EvtNo", "16", "String", "1"},
            new string [] {"evt_NoShort", "Operation", "Evt", "EvtNoShrt", "17", "String","0"},
            new string [] {"evt_Msg", "Operation", "Evt", "Msg", "18", "String", "1"},
            new string [] {"evt_Prior", "Operation", "Evt", "Prio", "19", "String", "0"},
            new string [] {"isol_Flt", "Isolation", "Flt", "", "20", "String", "0"},
            new string [] {"isol_LeakRis", "Isolation", "LeakRis", "", "21", "String", "0"},
            new string [] {"gridSwitchCount", "Operation", "GriSwCnt", "", "22", "String", "0"},
            new string [] {"Health", "Operation", "Health", "", "23", "String", "0"} };


        private allMeasures allM = new allMeasures();
        private int MAX_VALUES_PER_LINE = 150;
        private int MAX_VALUES;
        private const int MAX_STRING = 10;
        dbserver dbs;
        private Boolean [] bFlushMaster;

        public void run(String siteName)
        {
            MAX_VALUES = paramDef.Count + 4;        // 4 for date, time, inverterSN, String
            Directory.SetCurrentDirectory(siteName);
            dbs = new dbserver(siteName);
            String[] allFileNames = Directory.GetFiles(".", "*.csv", SearchOption.AllDirectories);
            Boolean BOnlyOne = false;
            bFlushMaster = new Boolean[paramDef.Count+5];
            bFlushMaster[0] = false;
            bFlushMaster[1] = false;
            bFlushMaster[2] = false;
            bFlushMaster[3] = false;
            bFlushMaster[4] = false;
            for (int i = 0; i < paramDef.Count + 5; i++)
            {
                if (i < 5)
                    bFlushMaster[i] = false;
                else
                    bFlushMaster[i] = paramDef[i-5][6] == "1" ? true : false;
            }
            foreach (String fileName in allFileNames)
            {
                oneMeasure oneM = loadACsv(fileName);
                if (oneM.values.Count > 0)
                    allM.Add(oneM);
                if (BOnlyOne)
                    break;
            }
            Directory.SetCurrentDirectory("..");

            dbs.checkInverters(siteName, allM);
            dbs.checkDateTimeLine(allM);

            uploadData(siteName);
        }

        oneMeasure loadACsv(String fileName)
        {
            int LineNo = 0;
            Console.WriteLine("Loading: " + fileName);
            oneMeasure oneM = new oneMeasure(MAX_VALUES_PER_LINE);

            using (CsvFileReader reader = new CsvFileReader(fileName))
            {
                CsvRow row = new CsvRow();
                while (reader.ReadRow(row) && LineNo++ < 10000)
                {
                    switch (LineNo)
                    {
                        case 1:
                        case 2:
                        case 3:
                            break;
                        case 4:
                            oneM.inverter = row.ToArray();
                            break;
                        case 5:
                            oneM.inverterType = row.ToArray();
                            break;
                        case 6:
                            oneM.inverterSN = row.ToArray();
                            break;
                        case 7:
                            oneM.measureName = row.ToArray();
                            break;
                        case 8:
                            oneM.measureType = row.ToArray();
                            break;
                        case 9:
                            oneM.measureUnit = row.ToArray();
                            break;
                        default:
                            oneM.addMeasure(row);

                            break;
                    }
                    //foreach (string s in row)
                    // {
                    //Console.Write(s);
                    //Console.Write(" ");
                    //}
                    //Console.WriteLine();
                }
            }
            return oneM;
        }

        void uploadMe(String[][] dt, String siteName)
        {
            dbs.loadData(dt, bFlushMaster, siteName);
        }
        // 0 : root name
        // 1 : suffix 1
        // 2 : suffix 2
        // 3 : String
        // 4 : Original Name
        string[] decodeName(String mName)
        {
            String[] strName = new String[5];
            String[] tmpStr = mName.Split(new char[] { '.' });

            strName[4] = mName;
            strName[0] = tmpStr[0];
            if (tmpStr.Count() > 3)
                throw new Exception(mName + " has more than 2 dots !!!");
            if (tmpStr.Count() > 1)
                strName[1] = tmpStr[1];
            if (tmpStr.Count() > 2)
                strName[2] = tmpStr[2];
            int indToCheck = 1;
            if (strName[1] == null)
                indToCheck = 0;
            if (strName[indToCheck].IndexOf('[') > 0)
            {
                strName[3] = strName[indToCheck].Substring(strName[indToCheck].IndexOf('[') + 1, strName[indToCheck].IndexOf(']') - strName[indToCheck].IndexOf('[') - 1);
                strName[indToCheck] = strName[indToCheck].Substring(0, strName[indToCheck].IndexOf('['));
            }
            else
                strName[3] = "";
            if (strName[1] != null)
            {
                if (strName[1].ToLower().StartsWith("flt"))
                {
                    strName[3] = strName[1].Substring(3, 1);
                    strName[1] = strName[1].Substring(0, 3);
                }
            }
            if (strName[2] != null)
            {
                switch (strName[2].ToLower())
                {
                    case "phsa":
                        strName[3] = "A";
                        break;
                    case "phsb":
                        strName[3] = "B";
                        break;
                    case "phsc":
                        strName[3] = "C";
                        break;
                    default:
                        if (strName[2].ToLower().StartsWith("phs"))
                            throw new Exception(mName + " has a phs suffice not reconized !!!");
                        break;
                }
            }
            return strName;
        }
        void uploadData(String siteName)
        {
            String currentInv = "????";
            // array per Date
            //   then per String
            String[][] valueUpload = null;

            for (int iOneMIndx = 0; iOneMIndx < allM.Count(); iOneMIndx++)
            {
                oneMeasure myM = allM[iOneMIndx];

                MAX_VALUES_PER_LINE = paramDef.Count + 5;
                currentInv = "????";
                Console.WriteLine("upLoading: " + myM.values[0][0]);

                for (int iLine = 0; iLine < myM.values.Count; iLine++)
                {
                    for (int iCol = 0; iCol < myM.values[iLine].Length; iCol++)
                    {
                        if (currentInv != myM.inverterSN[iCol])
                        {
                            // Skip the first col, and the empty columns
                            if (myM.inverterSN[iCol] == null || myM.inverterSN[iCol].Length == 0)
                            {
                                currentInv = "????";
                                continue;
                            }
                            // go to our server
                            if (valueUpload != null)
                                uploadMe(valueUpload, siteName);

                            valueUpload = new String[MAX_STRING][];
                            currentInv = myM.inverterSN[iCol];

                            DateTime dtMeasure = DateTime.Parse(myM.values[iLine][0]);
                            for (int i = 0; i < MAX_STRING; i++)
                            {
                                valueUpload[i] = new String[MAX_VALUES_PER_LINE];

                                valueUpload[i][0] = dtMeasure.ToShortDateString();
                                valueUpload[i][1] = dtMeasure.ToShortTimeString();
                                valueUpload[i][2] = myM.inverterSN[iCol];
                                valueUpload[i][3] = null;
                                valueUpload[i][4] = myM.inverterType[iCol];
                            }
                        }

                        //Decode our Measure (should be cached...)
                        String[] mName = decodeName(myM.measureName[iCol]);

                        // get our String in the array
                        int stringNo = 0;
                        if (mName[3] != "" && mName.Length != 0)
                        {
                            for (int i = 1; i < MAX_STRING; i++)
                            {
                                if (valueUpload[i][3] == mName[3])
                                {
                                    stringNo = i;
                                    break;
                                }
                                if (valueUpload[i][3] == null)
                                {
                                    valueUpload[i][3] = mName[3];
                                    stringNo = i;
                                    break;
                                }
                            }
                            //throw new Exception("Impossible to store the string for Measure: " + myM.measureName[iCol]);
                        }

                        // find measures def
                        bool bMeasureFound = false;
                        if (transcodName.ContainsKey(mName[0]))
                        {
                            if (transcodName[mName[0]] == "-1")
                            {
                                bMeasureFound = true;
                            }
                            else
                            {
                                foreach (String[] oneDef in paramDef)
                                {
                                    if (oneDef[4] == transcodName[mName[0]])
                                    {
                                        if (int.Parse(oneDef[4]) >= 0)
                                        {
                                            if (myM.values[iLine][iCol] != null && myM.values[iLine][iCol].Length != 0)
                                            {
                                                valueUpload[stringNo][int.Parse(oneDef[4]) + 4] = myM.values[iLine][iCol];
                                            }
                                        }
                                        bMeasureFound = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (String[] oneDef in paramDef)
                            {
                                if (oneDef[1].ToLower() == mName[0].ToLower())
                                {
                                    if (mName[1] == null || (oneDef[2].ToLower() == mName[1].ToLower()))
                                    {
                                        if ((oneDef[3] == "" || mName[2] == "") ||
                                            (oneDef[3].ToLower() == mName[2].ToLower()))
                                        {
                                            if (int.Parse(oneDef[4]) >= 0)
                                            {
                                                if (myM.values[iLine][iCol] != null && myM.values[iLine][iCol].Length != 0)
                                                {
                                                    valueUpload[stringNo][int.Parse(oneDef[4]) + 4] = myM.values[iLine][iCol];
                                                }
                                            }
                                            bMeasureFound = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!bMeasureFound)
                        {
                            continue; 

                            throw new Exception("measure not used !!!");
                        }

                    }       // next ICol
                }
                if (valueUpload != null)
                {
                    uploadMe(valueUpload, siteName);
                    valueUpload = null;
                }

            }
        }
    }
    public class allMeasures : List<oneMeasure>
    {
        //        List<oneMeasure> allM;
        public allMeasures()
        {
            //            allM = new List<oneMeasure>();
        }
    }
    public class oneMeasure
    {
        private int max_values = 200;
        public String siteName;
        public String[] inverter;
        public String[] inverterSN;
        public String[] inverterType;
        public String[] measureName;
        public String[] measureType;
        public String[] measureUnit;        // can get less element if the last are null...

        public List<String[]> values;

        public oneMeasure(int mv = 200)
        {
            max_values = mv;
            values = new List<string[]>();
        }
        public void addMeasure(CsvRow csvRow)
        {
            values.Add(csvRow.ToArray<String>());
        }
        public String[] getData(string inv, string measure)
        {
            int indx = 0;
            Boolean bOK = false;
            foreach (String mName in inverter)
            {
                if (mName == inv)
                {
                    bOK = true;
                    break;
                }
                indx++;
            }
            if (!bOK)
                throw new Exception("inverter " + inv + " not found");

            String[] data = new String[max_values];

            data[0] = inverter[indx];
            data[1] = measureName[indx];
            int indexValue = 2;
            foreach (string[] vals in values)
            {
                data[indexValue] = values[indexValue - 2][indx];
                indexValue++;
            }
            return data;
        }
    }
}

        /*
        */
