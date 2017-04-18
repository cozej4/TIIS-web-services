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
	public partial class HealthFacilityStockStatus
	{

		#region Properties

		public string antigen { get; set; }
		public Int32 received { get; set; }
		public Int32 adjusted { get; set; }
		public Int32 consumed { get; set; }
		public Int32 balance { get; set; }
		


		#endregion

		public static List<HealthFacilityStockStatus> GetHealthFacilityStockStatus(string hfid, DateTime fromDate, DateTime toDate)
		{
			try
			{
				string query = "SELECT * FROM   crosstab($$ SELECT t1.status, t1.\"GENDER\", t1.count FROM " +
							   "(select \"MOTHER_HIV_STATUS\" as status, \"GENDER\", count(\"GENDER\") as count from \"CHILD\" where \"HEALTHCENTER_ID\" = "+hfid+ " AND (\"BIRTHDATE\" >='"+fromDate + "' and \"BIRTHDATE\"<='" +toDate + "')  GROUP BY \"MOTHER_HIV_STATUS\", \"GENDER\" order by \"MOTHER_HIV_STATUS\")AS t1  $$)" +
							   "as final_result(\"status\" text, \"female\" bigint,\"male\" bigint) ";
				
				List<NpgsqlParameter> parameters = new List<NpgsqlParameter>()
					{
					new NpgsqlParameter("@hfid", DbType.Int32) { Value = hfid },
					new NpgsqlParameter("@fromDate", DbType.DateTime) { Value = fromDate },
					new NpgsqlParameter("@toDate", DbType.DateTime) { Value = toDate }
					};
				DataTable dt = DBManager.ExecuteReaderCommand(query, CommandType.Text, parameters);


				return GetHealthFacilityStockStatusAsList(dt);
			}
			catch (Exception ex)
			{
				Log.InsertEntity("Child", "GetAllChildrenPMTCTstatus", 4, ex.StackTrace.Replace("'", ""), ex.Message.Replace("'", ""));
				throw ex;
			}
		}






		#region Helper Methods       

		public static List<HealthFacilityStockStatus> GetHealthFacilityStockStatusAsList(DataTable dt)
		{
			List<HealthFacilityStockStatus> oList = new List<HealthFacilityStockStatus>();
			foreach (DataRow row in dt.Rows)
			{
				try
				{
					HealthFacilityStockStatus o = new HealthFacilityStockStatus();
					if (!row["status"].ToString().Equals(""))
					{
						o.status = row["status"].ToString();
					}
					else {
						o.status = "not assigned";
					}					
					o.female = Helper.ConvertToInt(row["female"]);
					o.male = Helper.ConvertToInt(row["male"]);

					oList.Add(o);
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

