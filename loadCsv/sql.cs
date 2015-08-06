﻿/*  NEW TABLES
  Scripts creation solaSW !
 
drop table inverters
;
CREATE TABLE dbo.inverters(
	serialNo varchar(50) not null primary key,

	-- company
	company varchar(50) not NULL,

	-- site specific information
	site varchar(50) NOT NULL,
	longitude decimal(9, 6) default 0,
	latitude decimal(9, 6) default 0,
	city varchar(50) not null,
	country varchar(50) not null,

	-- Inverter specific information
	name varchar(50) NOT NULL,
	power bigint not null,
	model varchar(50) not null,
	type tinyint not NULL,
	nbMeasures int not null,
	nbStrings int not null,
	sensorSN varchar(50) null
);

 
/* OLD SCRIPT .....
CREATE TABLE [dbo].[misc_northHillFarm](
	[dateMeasure] [dateTime] NOT NULL,
	[inverter] [varchar](50) NULL,
	[feedInTime] [decimal](18, 2) NULL,
	[operatingTime] [decimal](18, 2) NULL,
	[evtDescription] [varchar](255) NULL,
	[evtNo] [varchar](255) NULL,
	[evtNoShort] [varchar](4096) NULL,
	[evtMsg] [varchar](255) NULL,
	[evtPrior] [varchar](255) NULL,
	[isolationLeakRis] [decimal](18, 2) NULL,
	[gridSwitchCount] [decimal](18, 2) NULL,
	[health] [varchar](255) NULL
) ON [PRIMARY]
GO
select * from misc_northHillFarm where health != 'Ok'

insert into misc_northHillFarm
SELECT
       convert(DateTime, convert(varchar(10), dateMeasure) + ' ' + convert(varchar(5), timeMeasure) )
	  ,[inverter]
 	  ,[time_FeedIn]
	  ,[time_Operating]
      ,[evt_Description]
      ,[evt_No]
      ,[evt_NoShort]
      ,[evt_Msg]
      ,[evt_Prior]
      ,[isol_LeakRis]
      ,[gridSwitchCount]
      ,[health]
  FROM [solarPlants].[dbo].[m_northHillFarm]
  where stringNo is null

GO



CREATE TABLE [dbo].[mDc_northHillFarm](
	[dateMeasure] [dateTime] NOT NULL,
	[invString] [varchar](50) NULL,
	[current] [decimal](18, 4) NULL,
	[voltage] [decimal](18, 2) NULL,
	[power] [decimal](18, 2) NULL,
	[temp] [decimal](18, 2) NULL,
	[insolation] [decimal](18, 2) NULL,
	[windSpeed] [decimal](18, 2) NULL
) ON [PRIMARY]

GO
USE [solarPlants]
GO

select * from mDc_northHillFarm
delete from mDc_northHillFarm

insert into mDc_northHillFarm
SELECT
       convert(DateTime, convert(varchar(10), dateMeasure) + ' ' + convert(varchar(5), timeMeasure) )
      ,[inverter]+[stringNo]
      ,[dc_Current]
      ,[dc_Voltage]
      ,[dc_Power]
      ,[env_Temp]
      ,[env_Insolation]
      ,[env_WindSpeed]
  FROM [solarPlants].[dbo].[m_northHillFarm]
  where stringNo is not null and stringNo != 'C'


CREATE TABLE [dbo].[mAc_northHillFarm](
	[dateMeasure] [dateTime] NOT NULL,
	[invPhase] [varchar](50) NULL,
	[current] [decimal](18, 2) NULL,
	[frequency] [decimal](18, 2) NULL,
	[voltage] [decimal](18, 2) NULL,
	[power] [decimal](18, 2) NULL,
	[energyDaily] [decimal](18, 2) NULL,
	[energyPrevious] [decimal](18, 2) NULL,
	[isolationFlt] [decimal](18, 2) NULL,
    generated as energyDaily - energyPrevious,
) ON [PRIMARY]

GO
USE [solarPlants]
GO

insert into mAc_northHillFarm
SELECT
       convert(DateTime, convert(varchar(10), dateMeasure) + ' ' + convert(varchar(5), timeMeasure) )
      ,[inverter]+case when stringNo = 'A' then 'Phase 1' when stringNo = 'B' then 'Phase 2' when stringNo = 'C' then 'Phase 3' when stringNo is null then '' else null END
      ,[out_Current]
      ,[out_Frequency]
      ,[out_Voltage]
      ,[out_ActivePower]
      ,[Out_EnergyDaily]
	  ,0
      ,[isol_Flt]
  FROM [solarPlants].[dbo].[m_northHillFarm]
  where stringNo is null or
  (stringNo = 'A' or stringNo = 'B' or stringNo = 'C')


update mAc_northHillFarm set energyPrevious =  
	(	select max(b.energyDaily) from mAc_northHillFarm b
		where  mAc_northHillFarm.invPhase = b.invPhase
		  and b.dateMeasure < mAc_northHillFarm.dateMeasure
		  and datediff(day, mAc_northHillFarm.dateMeasure, b.dateMeasure) <=1
		  and b.energyDaily <= mAc_northHillFarm.energyDaily
		  and mAc_northHillFarm.energyDaily - b.EnergyDaily < 10000
	)
where 
	invPhase not like '%Phase%' and
	energyPrevious is null



GO
CREATE TABLE [dbo].[dateLine](
	[dateTime] [dateTime] NOT NULL,
	[day]  AS (datepart(day,[dateTime])),
	[month]  AS (datepart(month,[dateTime])),
	[year]  AS (datepart(year,[dateTime])),
	[yearMonth]  AS (CONVERT([varchar](5),datepart(year,[dateTime]))+right('0'+CONVERT([varchar](2),datepart(month,[dateTime])),(2))),
	[semester]  AS (CONVERT([int],datepart(month,[dateTime])/(6.0)+(0.9))),
	[quarter]  AS (CONVERT([int],datepart(month,[dateTime])/(3.0)+(0.99))),
	[hour] as (datepart(hour, [dateTime])),
    YMD as convert(date, dateTime),
PRIMARY KEY CLUSTERED 
(
	[dateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
insert dateLine
select distinct convert(DateTime, convert(varchar(10), dateMeasure) + ' ' + convert(varchar(5), timeMeasure) ) from [solarPlants].[dbo].[m_northHillFarm]

  
 * 

drop table inverters
CREATE TABLE [dbo].[inverters](
	[company] [varchar](50) NULL,
	[site] [varchar](50) NOT NULL,
	[longitude] [decimal](9, 6) NULL,
	[latitude] [decimal](9, 6) NULL,
	city varchar(50) not null,
	country varchar(50) not null,
	power bigint not null,
	[mainPanel] [varchar](50) NULL,
	[distributionBoard] [varchar](50) NULL,
	[inverter] [varchar](50) NOT NULL,
	[invString] varchar(50) not null,
	stringNo varchar(50) null,
	[type] [tinyint] NULL,
    
PRIMARY KEY CLUSTERED 
(
	[invString] ASC, stringNo ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

 * 
insert inverters
select distinct  
	'SolarSW', 'northHillFarm', 55, -2, 'LONDON', 'UK', 15000, 
	'Panel 1', 'Board 1', inverter, inverter+case(stringNo) when 'C' then 'Phase 3' else stringNo END,
	case(stringNo) when 'C' then 'Phase 3' else stringNo END, 0
	from [solarPlants].[dbo].[m_northHillFarm]

insert inverters
select distinct  
	'SolarSW', 'northHillFarm', 55, -2, 'LONDON', 'UK', 15000, 
	'Panel 1', 'Board 1', inverter, inverter + 'Phase 1',
	'Phase 1', 0
	from [solarPlants].[dbo].[m_northHillFarm]
	where stringNo = 'C'

insert inverters
select distinct  
	'SolarSW', 'northHillFarm', 55, -2, 'LONDON', 'UK', 15000, 
	'Panel 1', 'Board 1', inverter, inverter + 'Phase 2',
	'Phase 2', 0
	from [solarPlants].[dbo].[m_northHillFarm]
	where stringNo = 'C'

 * 
 * get errors
select dateMeasure, inverter, evtNo = convert(int, LEFT(evtNo, CHARINDEX('.', evtNo)-1)), evtNoShort = convert(int, LEFT(evtNoShort, CHARINDEX('.', evtNoShort)-1)), evtMsg, evtPrior, health 
from misc_northHillFarm where 
	health != 'Ok' or 
	(evtNo is not null ) or 
	(evtMsg is not null and evtMsg != 'None') or 
	(evtNoShort is not null and evtNoShort != '0.00') or 
	(evtPrior is not null and evtPrior != 'NonePrio')
 order by dateMeasure DESC
  
 */


/*
drop table inverters;
select distinct inverter from m_northHillFarm;
create table inverters
(company varchar(50),
 site varchar(50) not null,
 longitude decimal(9, 6),
 latitude decimal (9, 6),
 mainPanel varchar(50),
 distributionBoard varchar(50),
 inverter varchar(50) primary key,
 isItWebBox tinyint)

insert into inverters
select distinct 'SolarSW', 'northHillFarm',  50.1234, -2.5678, 'Panel1', 'Board 1', inverter, 0 from m_northHillFarm
;
update inverters set isItWebBox=1 where inverter ='155025205'

 * 
 * 
 * drop table p_northHillFarm
;
create table p_northHillFarm
(datePrevi date not null,
 inverter varchar(50),
 isIsWebBox tinyint,
 out_ActivePower decimal(18,2)
 )

 insert into p_northHillFarm
 select dateMeasure, inverter, 0, sum(out_ActivePower) * (rand()+0.5) from m_northHillFarm
   group by  dateMeasure, inverter

   select * from p_northHillFarm order by datePrevi

   update p_northHillFarm set isIsWebBox = 1 where inverter = '155025205'

 alter table m_northHillFarm
add constraint FK_NorthHillFarmInverter foreign key(inverter)
  references inverters(inverter);

 * alter table p_northHillFarm
add constraint FK_NorthHillFarmInverterPrevi foreign key(inverter)
  references inverters(inverter);

 * alter table measures
add constraint FK_AllInverter foreign key(inverter)
  references inverters(inverter);

 * 
 * Idx:

create index idx_northHillFarm_date on m_northHillFarm(dateMeasure)
create index idx_measure_date on m_northHillFarm(inverter)
create index idx_measure_date on measures(dateMeasure)
create index idx_measure_inv on measures(inverter)
 
 
-------------------------
   load date/hour Line
-------------------------
drop table timeLine;

create table timeLine (
date date not null primary key,
day  as DAY(date),
month as month(date),
year as year(date),
yearMonth as cast( year(date) as varchar(5)) + right('0' + convert(varchar(2), month(date)),2),
semester as cast (month(date)/6.0 + 0.9 as int),
quarter as cast (month(date)/3.0 + 0.99 as int)
)
;
insert into timeline
select distinct dateMeasure from measures

select * from timeLine

drop table hourLine
create table hourLine(
	time time not null primary key,
	hour as cast( cast(time as varchar(2)) as integer),
	minute as cast( right(cast(time as varchar(5)), 2) as integer)
)

insert into hourLine
select distinct timeMeasure from measures

select * from hourLine


 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace loadCsv
{
    class dbserver
    {
        String strConnect = "Data Source=.; Initial Catalog = solarSW ; Integrated Security = true ; Connection Timeout=10 ; Min Pool Size=2 ; Max Pool Size=20;";
        SqlConnection con;
        public dbserver(String siteName)
        {
            con = new SqlConnection(strConnect);
            con.Open();

            try
            {
                createTables(siteName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

//	[site] [varchar](50) not null,
         void createTables(String siteName)
        {

         }
        object getValueOrNull(String strVal)
         {
             try
             {
                 return String.IsNullOrWhiteSpace(strVal) ? (object)DBNull.Value : (object)Double.Parse(strVal.Replace(".",","));
             }
            catch(Exception ex)
             {
                Console.WriteLine("exception sur: " + strVal+ ", " + ex.Message);
                return (object) DBNull.Value;
            }
         }

        public Dictionary<string, inverter> loadInverters(String siteName)
        {
            Dictionary<String, inverter> allInvs = new Dictionary<string, inverter>();
            inverter oneInv;
            String serialNo, company, name, model, sensorSN;
            long power;
            int type, nbMeasures, nbStrings;
            invType invTp;

            using(SqlCommand cde = new SqlCommand("select serialNo, company, name, power, model, type, nbMeasures, nbStrings, sensorSN from inverters;", con))
            {
                SqlDataReader myRdr = cde.ExecuteReader();
                while (myRdr.Read()) {
                    serialNo = myRdr.GetString(0);
                    company = myRdr.GetString(1);
                    name = myRdr.GetString(2);
                    power = myRdr.GetInt64(3);
                    model = myRdr.GetString(4);
                    type = myRdr.GetInt32(5);
                    nbMeasures = myRdr.GetInt32(6);
                    nbStrings = myRdr.GetInt32(7);
                    sensorSN = myRdr.GetString(8);
                    switch(type){
                        case 1:
                        invTp = invType.WEBBOX;
                        break;
                        case 2:
                        invTp = invType.SENSOR;
                        break;
                        default:
                        invTp = invType.INVERTER;
                        break;
                    }

                    oneInv = new inverter(company, name, model, serialNo, power, invTp, nbMeasures, nbStrings, sensorSN);
                    allInvs.Add(oneInv.serialNo, oneInv);
                }
                myRdr.Close();
            }
            return allInvs;
        }
        public void createOneInverter(String siteName)
        {
/*
            foreach(String key in invInAllM.Keys){
                if (key == null || key.Length == 0)
                    continue;
                int isItWebBox = (invInAllM[key].StartsWith("WebBox")) ? 1 : 0;
                using (SqlCommand cde = new SqlCommand(
                    "INSERT INTO [dbo].[inverters] ([company], [site], [longitude],[latitude],[mainPanel],[distributionBoard],[inverter], [isItWebBox]) VALUES ("+
                    "'???', '" + siteName +"', 0, 0, '???', '???', '" +  key + "', " + isItWebBox + ");", con))
                        cde.ExecuteNonQuery();
            }
  */      }

        /*
        public void loadData(String [][]measuresFromOneFile, Boolean[] bFlush, String siteName){

            Boolean bSomethingToWrite = false;
            Boolean bIsAWebBox = false;

            DataTable measuresDT = new DataTable("m_" + siteName);
            DataTable measuresDTWebBox = new DataTable("measures");

            initializeColumns(measuresDT, null);
            initializeColumns(measuresDTWebBox, siteName);


            foreach (String[] oneMeasurePerTimeInverterString in measuresFromOneFile)
            {
                bIsAWebBox = oneMeasurePerTimeInverterString[4].StartsWith("WebBox");

                int monTime = int.Parse(oneMeasurePerTimeInverterString[1].ToString().Substring(0, 2));

                // skip Measures before 7am or after 9pm, if no events
                if ((monTime >= 21 || monTime < 7) &&
                    (oneMeasurePerTimeInverterString[19] == null || 
                     oneMeasurePerTimeInverterString[19] == "None"))
                    continue;

                Boolean bToWrite = false;
                int iCol = -1;
                for (int jj = 0; jj < oneMeasurePerTimeInverterString.Length; jj++)
                {
                    String strCell = oneMeasurePerTimeInverterString[jj];
                    if (strCell != null && strCell.Length != 0)
                    {
                        if (bFlush[jj])
                            bToWrite = true;
                    }
                }
                if (bToWrite)
                {
                    DataRow dtRowMeasures = measuresDT.NewRow();
                    DataRow dtRowWebBox = measuresDTWebBox.NewRow();
                    loadRowValues(dtRowMeasures, oneMeasurePerTimeInverterString, null);
                    measuresDT.Rows.Add(dtRowMeasures);
                    if(bIsAWebBox){
                        loadRowValues(dtRowWebBox, oneMeasurePerTimeInverterString, siteName);
                        measuresDTWebBox.Rows.Add(dtRowWebBox);
                    }
                    bSomethingToWrite = true;
                }
            }
            if (bSomethingToWrite)
            {
                using (SqlConnection dbConnection = new SqlConnection(strConnect))
                {
                    dbConnection.Open();
                    using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                    {
                        s.DestinationTableName = measuresDT.TableName;

                        foreach (var column in measuresDT.Columns)
                            s.ColumnMappings.Add(column.ToString(), column.ToString());

                        s.WriteToServer(measuresDT);
                    }
                    if(bIsAWebBox)
                    {
                        using (SqlBulkCopy s = new SqlBulkCopy(dbConnection))
                        {
                            s.DestinationTableName = measuresDTWebBox.TableName;

                            foreach (var column in measuresDTWebBox.Columns)
                                s.ColumnMappings.Add(column.ToString(), column.ToString());

                            s.WriteToServer(measuresDTWebBox);
                        }
                    }
                }
            }
        }
        void initializeColumns(DataTable dtT, String siteName)
        {
            if (siteName != null)
            {
                DataColumn siteCol = new DataColumn("site", Type.GetType("System.String"));
                dtT.Columns.Add(siteCol);
            }
            DataColumn dateCol = new DataColumn("dateMeasure", Type.GetType("System.String"));
            dtT.Columns.Add(dateCol);
            DataColumn timeColCol = new DataColumn("timeMeasure", Type.GetType("System.String"));
            dtT.Columns.Add(timeColCol);
            DataColumn inverterCol = new DataColumn("inverter", Type.GetType("System.String"));
            dtT.Columns.Add(inverterCol);
            DataColumn stringNoCol = new DataColumn("stringNo", Type.GetType("System.String"));
            dtT.Columns.Add(stringNoCol);

            DataColumn dc_CurrentCol = new DataColumn("dc_Current", Type.GetType("System.Double"));
            dtT.Columns.Add(dc_CurrentCol);
            DataColumn dc_VoltageCol = new DataColumn("dc_Voltage", Type.GetType("System.Double"));
            dtT.Columns.Add(dc_VoltageCol);
            DataColumn dc_PowerCol = new DataColumn("dc_Power", Type.GetType("System.Double"));
            dtT.Columns.Add(dc_PowerCol);
            DataColumn env_TempCol = new DataColumn("env_Temp", Type.GetType("System.Double"));
            dtT.Columns.Add(env_TempCol);
            DataColumn env_InsolationCol = new DataColumn("env_Insolation", Type.GetType("System.Double"));
            dtT.Columns.Add(env_InsolationCol);
            DataColumn env_WindSpeedCol = new DataColumn("env_WindSpeed", Type.GetType("System.Double"));
            dtT.Columns.Add(env_WindSpeedCol);
            DataColumn currentCol = new DataColumn("out_Current", Type.GetType("System.Double"));
            dtT.Columns.Add(currentCol);
            DataColumn frequencyCol = new DataColumn("out_Frequency", Type.GetType("System.Double"));
            dtT.Columns.Add(frequencyCol);
            DataColumn out_VoltageCol = new DataColumn("out_Voltage", Type.GetType("System.Double"));
            dtT.Columns.Add(out_VoltageCol);
            DataColumn out_ActivePowerCol = new DataColumn("out_ActivePower", Type.GetType("System.Double"));
            dtT.Columns.Add(out_ActivePowerCol);
            DataColumn mod_TempCol = new DataColumn("mod_Temp", Type.GetType("System.Double"));
            dtT.Columns.Add(mod_TempCol);
            DataColumn time_FeedInCol = new DataColumn("time_FeedIn", Type.GetType("System.Double"));
            dtT.Columns.Add(time_FeedInCol);
            DataColumn time_OperatingCol = new DataColumn("time_Operating", Type.GetType("System.Double"));
            dtT.Columns.Add(time_OperatingCol);
            DataColumn Out_EnergyDailyCol = new DataColumn("Out_EnergyDaily", Type.GetType("System.Double"));
            dtT.Columns.Add(Out_EnergyDailyCol);
            DataColumn evt_DescriptionCol = new DataColumn("evt_Description", Type.GetType("System.String"));
            dtT.Columns.Add(evt_DescriptionCol);
            DataColumn evt_NoCol = new DataColumn("evt_No", Type.GetType("System.String"));
            dtT.Columns.Add(evt_NoCol);
            DataColumn evt_NoShortCol = new DataColumn("evt_NoShort", Type.GetType("System.String"));
            dtT.Columns.Add(evt_NoShortCol);
            DataColumn evt_MsgCol = new DataColumn("evt_Msg", Type.GetType("System.String"));
            dtT.Columns.Add(evt_MsgCol);
            DataColumn evt_PriorCol = new DataColumn("evt_Prior", Type.GetType("System.String"));
            dtT.Columns.Add(evt_PriorCol);
            DataColumn isol_FltCol = new DataColumn("isol_Flt", Type.GetType("System.Double"));
            dtT.Columns.Add(isol_FltCol);
            DataColumn isol_LeakRisCol = new DataColumn("isol_LeakRis", Type.GetType("System.Double"));
            dtT.Columns.Add(isol_LeakRisCol);
            DataColumn gridSwitchCountCol = new DataColumn("gridSwitchCount", Type.GetType("System.Double"));
            dtT.Columns.Add(gridSwitchCountCol);
            DataColumn healthCol = new DataColumn("health", Type.GetType("System.String"));
            dtT.Columns.Add(healthCol);
        }
        void loadRowValues(DataRow dtR, String[] oneMeasurePerTimeInverterString, String siteName)
        {
            if (siteName != null)
            {
                dtR["site"] = siteName;
            }
            dtR["dateMeasure"] = oneMeasurePerTimeInverterString[0];
            dtR["TimeMeasure"] = oneMeasurePerTimeInverterString[1];
            dtR["inverter"] = oneMeasurePerTimeInverterString[2];
            dtR["stringNo"] = oneMeasurePerTimeInverterString[3];
            dtR["dc_Current"] = getValueOrNull(oneMeasurePerTimeInverterString[5]);
            dtR["dc_Voltage"] = getValueOrNull(oneMeasurePerTimeInverterString[6]);
            dtR["dc_Power"] = getValueOrNull(oneMeasurePerTimeInverterString[7]);
            dtR["env_Temp"] = getValueOrNull(oneMeasurePerTimeInverterString[8]);
            dtR["env_Insolation"] = getValueOrNull(oneMeasurePerTimeInverterString[9]);
            dtR["env_WindSpeed"] = getValueOrNull(oneMeasurePerTimeInverterString[10]);
            dtR["out_Current"] = getValueOrNull(oneMeasurePerTimeInverterString[11]);
            dtR["out_Frequency"] = getValueOrNull(oneMeasurePerTimeInverterString[12]);
            dtR["out_Voltage"] = getValueOrNull(oneMeasurePerTimeInverterString[13]);
            dtR["out_ActivePower"] = getValueOrNull(oneMeasurePerTimeInverterString[14]);
            dtR["mod_Temp"] = getValueOrNull(oneMeasurePerTimeInverterString[15]);
            dtR["time_FeedIn"] = getValueOrNull(oneMeasurePerTimeInverterString[16]);
            dtR["time_Operating"] = getValueOrNull(oneMeasurePerTimeInverterString[17]);
            dtR["Out_EnergyDaily"] = getValueOrNull(oneMeasurePerTimeInverterString[18]);
            dtR["evt_Description"] = oneMeasurePerTimeInverterString[19];
            dtR["evt_No"] = oneMeasurePerTimeInverterString[20];
            dtR["evt_NoShort"] = oneMeasurePerTimeInverterString[21];
            dtR["evt_Msg"] = oneMeasurePerTimeInverterString[22];
            dtR["evt_Prior"] = oneMeasurePerTimeInverterString[23];
            dtR["isol_Flt"] = getValueOrNull(oneMeasurePerTimeInverterString[24]);
            dtR["isol_LeakRis"] = getValueOrNull(oneMeasurePerTimeInverterString[25]);
            dtR["gridSwitchCount"] = getValueOrNull(oneMeasurePerTimeInverterString[26]);
            dtR["health"] = oneMeasurePerTimeInverterString[27];
        }
         * */
    }
}
