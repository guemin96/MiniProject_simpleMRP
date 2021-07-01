using MRPApp.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRPApp.Logic
{
    public class DataAccess
    {
        //셋팅 테이블에서 데이터 가져오기
        public static List<Settings> GetSettings()
        {
            List<Model.Settings> list;
            using (var ctx = new MRPEntities()) // ctx에 MRP데이터를 넣는 행동 , new MRPEntities를 사용함으로써 커넥션 커맨드 파라미터 역할을 해줌
            {
                list = ctx.Settings.ToList();
            }// using을 사용함으로써 데이터베이스를 열고 알아서 닫아주는 역할을 함
            return list;

        }

        public static int SetSettings(Settings item)
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Settings.AddOrUpdate(item);
                return ctx.SaveChanges(); //COMMIT
            }
        }

        public static int DelSetting(Settings item)
        {
            using (var ctx = new MRPEntities())
            {
                var obj = ctx.Settings.Find(item.BasicCode);//DB안에서 데이터를 찾아야 삭제할 수 있음(BasicCode를 통해=>PK이기 때문에)
                ctx.Settings.Remove(obj); //DB데이터를 삭제하는 과정
                return ctx.SaveChanges();
            }
        }

        public static List<Schedules> GetSchedules()
        {
            List<Model.Schedules> list;
            using (var ctx = new MRPEntities())
            {
                list = ctx.Schedules.ToList();
            }
            return list;
        }

        internal static int SetSchedule(Schedules item)
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Schedules.AddOrUpdate(item);
                return ctx.SaveChanges(); //COMMIT
            }
        }

        internal static List<Process> GetProcesses()
        {
            List<Model.Process> list;
            using (var ctx = new MRPEntities())
                list = ctx.Process.ToList();
            return list;
        }
        internal static int SetProcess(Process item)
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Process.AddOrUpdate(item);
                return ctx.SaveChanges();
            }
        }

        internal static List<Report> GetReportDatas(string startDate, string endDate, string plantcode)
        {
            var connString = ConfigurationManager.ConnectionStrings["MRPConnString"].ToString();//기존의 데이터베이스 연결은 app.config에 잇음[MRPEntities]로 연결
            var list = new List<Model.Report>();

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var sqlQuery = $@"SELECT sch.SchIdx,sch.PlantCode, sch.SchAmount, prc.PrcDate,
		                                prc.PrcOKAmount,prc.PrcFailAmount
                                    FROM Schedules AS sch
                                    INNER JOIN (
		                                        SELECT smr.SchIdx,smr.PrcDate,
			                                        Sum(smr.PrcOK) as PrcOKAmount,Sum(smr.PrcFail) as PrcFailAmount
		                                        FROM (
			                                        SELECT p.SchIdx,p.PrcDate,
				                                      CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,
				                                      CASE p.prcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail
				                                      FROM Process AS p
			                                        ) AS smr
		                                        GROUP BY smr.SchIdx,smr.PrcDate
                                            )AS prc
                                                ON sch.SchIdx = prc.SchIdx
                                        where sch.PlantCode = '{plantcode}'
                                          AND prc.PrcDate BETWEEN '{startDate}' AND '{endDate}' ";

                SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var tmp = new Report
                    {
                        SchIdx = (int)reader["SchIdx"],
                        PlantCode = reader["PlantCode"].ToString(),
                        SchAmount = (int)reader["SchAmount"],
                        PrcDate = DateTime.Parse(reader["PrcDate"].ToString()),
                        PrcOKAmount = (int)reader["PrcOKAmount"],
                        PrcFailAmount = (int)reader["PrcFailAmount"]
                    };
                    list.Add(tmp);
                }
                return list;
            }
        }
    }
}
