//*******************************************************************************
//Copyright 2015 TIIS - Tanzania Immunization Information System
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using Npgsql;

namespace GIIS.DataLayer
{
	public partial class HealthFacilityPMTCTstatus
	{

		#region Properties

		public string status { get; set; }
		public Int32 female { get; set; }
		public Int32 male { get; set; }



		#endregion

		public static List<HealthFacilityPMTCTstatus> GetAllChildrenPMTCTstatus(string hfid, DateTime fromDate, DateTime toDate)
		{
			if (hfid.Equals(""))
			{
				return null;
			}
			try
			{
				string query = "SELECT * FROM   crosstab($$ SELECT t1.status, t1.\"GENDER\", t1.count FROM " +
							   "(select \"MOTHER_HIV_STATUS\" as status, \"GENDER\", count(\"GENDER\") as count from \"CHILD\"" +
					"inner join \"HEALTH_FACILITY\" on \"CHILD\".\"HEALTHCENTER_ID\" = \"HEALTH_FACILITY\".\"ID\" where  (\"CHILD\".\"HEALTHCENTER_ID\" IN (SELECT DISTINCT A.\"ID\" FROM \"HEALTH_FACILITY\" AS A\nLEFT JOIN \"HEALTH_FACILITY\" AS B ON A.\"ID\" = B.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS C ON B.\"ID\" = C.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS D ON C.\"ID\" = D.\"PARENT_ID\"\nWHERE \nA.\"ID\" = "+hfid+"\nUNION\nSELECT DISTINCT B.\"ID\" FROM \"HEALTH_FACILITY\" AS A\nLEFT JOIN \"HEALTH_FACILITY\" AS B ON A.\"ID\" = B.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS C ON B.\"ID\" = C.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS D ON C.\"ID\" = D.\"PARENT_ID\"\nWHERE \nA.\"ID\" = "+hfid+"\nUNION\nSELECT DISTINCT C.\"ID\" FROM \"HEALTH_FACILITY\" AS A\nLEFT JOIN \"HEALTH_FACILITY\" AS B ON A.\"ID\" = B.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS C ON B.\"ID\" = C.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS D ON C.\"ID\" = D.\"PARENT_ID\"\nWHERE \nA.\"ID\" = "+hfid+"\nUNION\nSELECT DISTINCT D.\"ID\" FROM \"HEALTH_FACILITY\" AS A\nLEFT JOIN \"HEALTH_FACILITY\" AS B ON A.\"ID\" = B.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS C ON B.\"ID\" = C.\"PARENT_ID\"\nLEFT JOIN \"HEALTH_FACILITY\" AS D ON C.\"ID\" = D.\"PARENT_ID\"\nWHERE \nA.\"ID\" = "+hfid+"))  AND (\"CHILD\".\"BIRTHDATE\" >='" + fromDate + "' and \"CHILD\".\"BIRTHDATE\"<='" + toDate + "')  GROUP BY \"MOTHER_HIV_STATUS\", \"GENDER\" order by \"MOTHER_HIV_STATUS\")AS t1  $$)" +
							   "as final_result(\"status\" text, \"female\" bigint,\"male\" bigint) ";
				
				List<NpgsqlParameter> parameters = new List<NpgsqlParameter>()
					{
					new NpgsqlParameter("@hfid", DbType.Int32) { Value = hfid },
					new NpgsqlParameter("@fromDate", DbType.DateTime) { Value = fromDate },
					new NpgsqlParameter("@toDate", DbType.DateTime) { Value = toDate }
					};
				DataTable dt = DBManager.ExecuteReaderCommand(query, CommandType.Text, parameters);


				return GetAllChildrenPMTCTstatusAsList(dt);
			}
			catch (Exception ex)
			{
				Log.InsertEntity("Child", "GetAllChildrenPMTCTstatus", 4, ex.StackTrace.Replace("'", ""), ex.Message.Replace("'", ""));
				throw ex;
			}
		}






		#region Helper Methods       

		public static List<HealthFacilityPMTCTstatus> GetAllChildrenPMTCTstatusAsList(DataTable dt)
		{
			List<HealthFacilityPMTCTstatus> oList = new List<HealthFacilityPMTCTstatus>();
			foreach (DataRow row in dt.Rows)
			{
				try
				{
					HealthFacilityPMTCTstatus o = new HealthFacilityPMTCTstatus();
					if (!row["status"].ToString().Equals(""))
					{
						o.status = row["status"].ToString();
						o.female = Helper.ConvertToInt(row["female"]);
						o.male = Helper.ConvertToInt(row["male"]);

						oList.Add(o);
					}					

				}
				catch (Exception ex)
				{
					Log.InsertEntity("HealthFacilityDefaulters", "HealthFacilityDefaultersAsList", 1, ex.StackTrace.Replace("'", ""), ex.Message.Replace("'", ""));
					throw ex;
				}
			}


			foreach (DataRow row in dt.Rows)
			{
				try
				{
					if (row["status"].ToString().Equals(""))
					{
						var healthFacilityPMTCTstatus = oList.Where(p => p.status == "U");
						foreach (var h in healthFacilityPMTCTstatus)
						{
							h.female += Helper.ConvertToInt(row["female"]);
							h.male += Helper.ConvertToInt(row["male"]);
						}
					}

				}
				catch (Exception ex)
				{
					Log.InsertEntity("HealthFacilityDefaulters", "HealthFacilityDefaultersAsList", 1, ex.StackTrace.Replace("'", ""), ex.Message.Replace("'", ""));
					throw ex;
				}
			}



			return oList;
		}








		#endregion

	}
}

