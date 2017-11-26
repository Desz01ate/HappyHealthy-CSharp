﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite.Net;
using SQLite.Net.Platform.XamarinAndroid;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace HappyHealthyCSharp
{
    abstract class DatabaseHelper
    {
        public abstract List<string> Column { get; }
        virtual public List<T> Select<T>(string query) where T : class
        {
            var conn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
            var result = conn.Query<T>(query);
            conn.Close();
            return result;
        }
        virtual public bool Delete<T>(int id)
        {
            try
            {
                var conn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
                var result = conn.Delete<T>(id);
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        virtual public JavaList<IDictionary<string, object>> GetJavaList<T>(string queryCustomized, List<string> columnTag) where T : class
        {
            #region MySQL
            /*
            var sqlconn = new MySqlConnection(GlobalFunction.remoteAccess);
            sqlconn.Open();
            var fbsList = new JavaList<IDictionary<string, object>>();
            var query = queryCustomized;
            var tickets = new DataSet();
            var adapter = new MySqlDataAdapter(query, sqlconn);
            adapter.Fill(tickets, "FBS");
            foreach(DataRow x in tickets.Tables["FBS"].Rows)
            {
                var fbs = new JavaDictionary<string, object>();
                fbs.Add("fbs_id", GlobalFunction.StringValidation(x[0].ToString()));
                fbs.Add("fbs_time", GlobalFunction.StringValidation(x[1].ToString()));
                Console.WriteLine(GlobalFunction.StringValidation(((DateTime)x[1]).ToThaiLocale().ToString()));
                fbs.Add("fbs_fbs", GlobalFunction.StringValidation(x[2].ToString()));
                fbs.Add("fbs_fbs_lvl", GlobalFunction.StringValidation(x[3].ToString()));
                fbs.Add("ud_id", GlobalFunction.StringValidation(x[4].ToString()));
                fbsList.Add(fbs);
            }
            sqlconn.Close();
            */
            #endregion
            var dataList = new JavaList<IDictionary<string, object>>();
            var conn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
            var queryResult = conn.Query<T>(queryCustomized);
            queryResult.ForEach(dataRow =>
            {
                var data = new JavaDictionary<string, object>();
                columnTag.ForEach(attribute =>
                {
                    data.Add(attribute, dataRow.GetType().GetProperty(attribute).PropertyType == typeof(DateTime) ? ((DateTime)dataRow.GetType().GetProperty(attribute).GetValue(dataRow)).ToLocalTime() : dataRow.GetType().GetProperty(attribute).GetValue(dataRow));
                });
                dataList.Add(data);
            }); //a little obfuscate code, try solve it for a little challenge :P
            conn.Close();
            return dataList;
        }
    }
    static class DatabaseHelperExtension
    {
        public static bool Update(this DatabaseHelper data)
        {
            try
            {
                var conn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
                var result = conn.Update(data);
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool Insert(this DatabaseHelper data)
        {
            try
            {
                var conn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
                var result = conn.Insert(data);
                conn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public static bool CreateSQLiteTableIfNotExists()
        {

            try
            {
                var sqliteConn = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
                sqliteConn.CreateTable<DiabetesTABLE>();
                sqliteConn.CreateTable<DoctorTABLE>();
                sqliteConn.CreateTable<FoodTABLE>();
                sqliteConn.CreateTable<KidneyTABLE>();
                sqliteConn.CreateTable<MedicineTABLE>();
                sqliteConn.CreateTable<PressureTABLE>();
                sqliteConn.CreateTable<SummaryDiabetesTABLE>();
                sqliteConn.CreateTable<UserTABLE>();
                CreateTriggers(sqliteConn);
                sqliteConn.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"DEBUG LOG : {e.ToString()}");
                return false;
            }
        }
        private static void CreateTriggers(SQLiteConnection c)
        {
            try
            {
                c.CreateTable<TEMP_DiabetesTABLE>();
                c.Execute($@"CREATE TRIGGER DiabetesTABLE_After_Insert_Trigger AFTER INSERT ON DiabetesTABLE 
                           BEGIN 
                                INSERT INTO TEMP_DiabetesTABLE(fbs_id_pointer, fbs_time_new, fbs_fbs_new, fbs_fbs_lvl_new, MODE) 
                                VALUES(NEW.fbs_id, NEW.fbs_time, NEW.fbs_fbs, NEW.fbs_fbs_lvl, 'I'); 
                                UPDATE DiabetesTABLE 
                                SET 
                                    fbs_time_string = DATETIME('now','7 hours')
                                WHERE 
                                    fbs_id = NEW.fbs_id; 
                                END;");
                c.Execute($@"CREATE TRIGGER DiabetesTABLE_After_Update_Trigger AFTER UPDATE ON DiabetesTABLE 
                             BEGIN
                             	INSERT INTO TEMP_DiabetesTABLE(fbs_id_pointer, fbs_time_old, fbs_fbs_old, fbs_fbs_lvl_old, fbs_time_new, fbs_fbs_new, fbs_fbs_lvl_new,fbs_time_string_new, MODE)
                             	VALUES(OLD.fbs_id, OLD.fbs_time, OLD.fbs_fbs, OLD.fbs_fbs_lvl, NEW.fbs_time, NEW.fbs_fbs, NEW.fbs_fbs_lvl,NEW.fbs_time_string, 'U');
                             END");
                c.Execute($@"CREATE TRIGGER DiabetesTABLE_After_Delete_Trigger AFTER DELETE ON DiabetesTABLE 
                            BEGIN 
                                INSERT INTO TEMP_DiabetesTABLE(fbs_id_pointer, fbs_time_old, fbs_fbs_old, fbs_fbs_lvl_old, MODE) 
                                VALUES(OLD.fbs_id, OLD.fbs_time, OLD.fbs_fbs, OLD.fbs_fbs_lvl, 'D'); 
                            END");

                c.CreateTable<TEMP_PressureTABLE>();
                c.Execute($@"CREATE TRIGGER PressureTABLE_After_Insert_Trigger AFTER INSERT ON PressureTABLE 
                            BEGIN 
                                INSERT INTO TEMP_PressureTABLE(bp_id_pointer, bp_time_new ,bp_up_new ,bp_lo_new ,bp_hr_new ,bp_up_lvl_new ,bp_lo_lvl_new ,bp_hr_lvl_new ,MODE) 
                                VALUES(NEW.bp_id, NEW.bp_time ,NEW.bp_up ,NEW.bp_lo ,NEW.bp_hr ,NEW.bp_up_lvl ,NEW.bp_lo_lvl ,NEW.bp_hr_lvl ,'I'); 
                            UPDATE PressureTABLE
                            SET
                                bp_time_string = DATETIME('now','7 hours')
                            WHERE
                                bp_id = NEW.bp_id;
                            END");
                c.Execute($@"CREATE TRIGGER PressureTABLE_After_UPDATE_Trigger AFTER UPDATE ON PressureTABLE 
                            BEGIN 
                                INSERT INTO TEMP_PressureTABLE(bp_id_pointer, bp_time_old ,bp_up_old ,bp_lo_old ,bp_hr_old ,bp_up_lvl_old ,bp_lo_lvl_old ,bp_hr_lvl_old ,bp_time_new ,bp_up_new ,bp_lo_new ,bp_hr_new ,bp_up_lvl_new ,bp_lo_lvl_new ,bp_hr_lvl_new,bp_time_string_new ,MODE) 
                                VALUES(OLD.bp_id, OLD.bp_time ,OLD.bp_up ,OLD.bp_lo ,OLD.bp_hr ,OLD.bp_up_lvl ,OLD.bp_lo_lvl ,OLD.bp_hr_lvl ,NEW.bp_time ,NEW.bp_up ,NEW.bp_lo ,NEW.bp_hr ,NEW.bp_up_lvl ,NEW.bp_lo_lvl ,NEW.bp_hr_lvl,NEW.bp_time_string ,'U'); 
                            END");
                c.Execute($@"CREATE TRIGGER PressureTABLE_After_DELETE_Trigger AFTER DELETE ON PressureTABLE BEGIN Insert into TEMP_PressureTABLE(bp_id_pointer, bp_time_old ,bp_up_old ,bp_lo_old ,bp_hr_old ,bp_up_lvl_old ,bp_lo_lvl_old ,bp_hr_lvl_old ,MODE) values(OLD.bp_id, OLD.bp_time ,OLD.bp_up ,OLD.bp_lo ,OLD.bp_hr ,OLD.bp_up_lvl ,OLD.bp_lo_lvl ,OLD.bp_hr_lvl ,'D'); END");

                c.CreateTable<TEMP_KidneyTABLE>();
                c.Execute($@"CREATE TRIGGER KidneyTABLE_After_Insert_Trigger AFTER INSERT ON KidneyTABLE 
                            BEGIN 
                                INSERT INTO `TEMP_KidneyTABLE`(`ckd_id_pointer`, `ckd_time_new` , `ckd_gfr_new` , `ckd_gfr_level_new` , `ckd_creatinine_new` , `ckd_bun_new` , `ckd_sodium_new` , `ckd_potassium_new` , `ckd_albumin_blood_new` , `ckd_albumin_urine_new` , `ckd_phosphorus_blood_new` , `MODE`) 
                                VALUES(NEW.ckd_id, NEW.ckd_time , NEW.ckd_gfr , NEW.ckd_gfr_level , NEW.ckd_creatinine , NEW.ckd_bun , NEW.ckd_sodium , NEW.ckd_potassium , NEW.ckd_albumin_blood , NEW.ckd_albumin_urine , NEW.ckd_phosphorus_blood , 'I' ); 
                                UPDATE KidneyTABLE
                                SET 
                                    ckd_time_string = DATETIME('now','7 hours')
                                WHERE 
                                    ckd_id = NEW.ckd_id; 
                            END");
                c.Execute($@"CREATE TRIGGER KidneyTABLE_After_UPDATE_Trigger AFTER UPDATE ON KidneyTABLE 
                            BEGIN 
                                INSERT INTO `TEMP_KidneyTABLE`(`ckd_id_pointer`, `ckd_time_new` , `ckd_time_old` , `ckd_gfr_new` , `ckd_gfr_old` , `ckd_gfr_level_new` , `ckd_gfr_level_old` , `ckd_creatinine_new` , `ckd_creatinine_old` , `ckd_bun_new` , `ckd_bun_old` , `ckd_sodium_new` , `ckd_sodium_old` , `ckd_potassium_new` , `ckd_potassium_old` , `ckd_albumin_blood_new` , `ckd_albumin_blood_old` , `ckd_albumin_urine_new` , `ckd_albumin_urine_old` , `ckd_phosphorus_blood_new` , `ckd_phosphorus_blood_old` ,`ckd_time_string_new`, `MODE`) 
                                VALUES(OLD.ckd_id, NEW.ckd_time , OLD.ckd_time , NEW.ckd_gfr , OLD.ckd_gfr , NEW.ckd_gfr_level , OLD.ckd_gfr_level , NEW.ckd_creatinine , OLD.ckd_creatinine , NEW.ckd_bun , OLD.ckd_bun , NEW.ckd_sodium , OLD.ckd_sodium , NEW.ckd_potassium , OLD.ckd_potassium , NEW.ckd_albumin_blood , OLD.ckd_albumin_blood , NEW.ckd_albumin_urine , OLD.ckd_albumin_urine , NEW.ckd_phosphorus_blood , OLD.ckd_phosphorus_blood ,NEW.ckd_time_string, 'U' ); 
                            END");
                c.Execute($@"CREATE TRIGGER KidneyTABLE_After_DELETE_Trigger AFTER DELETE ON KidneyTABLE 
                            BEGIN 
                                INSERT INTO `TEMP_KidneyTABLE`(`ckd_id_pointer`, `ckd_time_old` , `ckd_gfr_old` , `ckd_gfr_level_old` , `ckd_creatinine_old` , `ckd_bun_old` , `ckd_sodium_old` , `ckd_potassium_old` , `ckd_albumin_blood_old` , `ckd_albumin_urine_old` , `ckd_phosphorus_blood_old`, `MODE`) 
                                VALUES(OLD.ckd_id, OLD.ckd_time , OLD.ckd_gfr , OLD.ckd_gfr_level , OLD.ckd_creatinine , OLD.ckd_bun , OLD.ckd_sodium , OLD.ckd_potassium , OLD.ckd_albumin_blood , OLD.ckd_albumin_urine , OLD.ckd_phosphorus_blood , 'D' ); 
                            END");
                //c.Execute("CREATE TABLE `TEMP_KidneyTABLE` (`ckd_id_pointer` int, `ckd_time_new` bigint, `ckd_time_old` bigint, `ckd_gfr_new` float, `ckd_gfr_old` float, `ckd_gfr_level_new` INTEGER, `ckd_gfr_level_old` INTEGER, `ckd_creatinine_new` float, `ckd_creatinine_old` float, `ckd_bun_new` float, `ckd_bun_old` float, `ckd_sodium_new` float, `ckd_sodium_old` float, `ckd_potassium_new` float, `ckd_potassium_old` float, `ckd_albumin_blood_new` float, `ckd_albumin_blood_old` float, `ckd_albumin_urine_new` float, `ckd_albumin_urine_old` float, `ckd_phosphorus_blood_new` Float, `ckd_phosphorus_blood_old` Float, `MODE` varchar(255) )");
                c.Execute("CREATE TABLE `TEMP_SummaryDiabetesTABLE` (`sfbs_id_pointer` int, `sfbs_time_new` bigint, `sfbs_time_old` bigint, `sfbs_sfbs_new` float, `sfbs_sfbs_old` float, `sfbs_sfbs_lvl_new` INT, `sfbs_sfbs_lvl_old` INT, `MODE` varchar(255) )");
                c.Execute("CREATE TRIGGER SummaryDiabetesTABLE_After_Delete_Trigger AFTER DELETE ON SummaryDiabetesTABLE BEGIN insert into TEMP_SummaryDiabetesTABLE(sfbs_id_pointer, sfbs_time_old, sfbs_sfbs_old, sfbs_sfbs_lvl_old, MODE) values(OLD.bp_id, OLD.sfbs_time, OLD.sfbs_sfbs, OLD.sfbs_sfbs_lvl, 'D'); END");
                c.Execute("CREATE TRIGGER SummaryDiabetesTABLE_After_Insert_Trigger AFTER INSERT ON SummaryDiabetesTABLE BEGIN insert into TEMP_SummaryDiabetesTABLE(sfbs_id_pointer, sfbs_time_new, sfbs_sfbs_new, sfbs_sfbs_lvl_new, MODE) values(NEW.bp_id, NEW.sfbs_time, NEW.sfbs_sfbs, NEW.sfbs_sfbs_lvl, 'I'); END");
                c.Execute("CREATE TRIGGER SummaryDiabetesTABLE_After_Update_Trigger AFTER UPDATE ON SummaryDiabetesTABLE BEGIN insert into TEMP_SummaryDiabetesTABLE(sfbs_id_pointer, sfbs_time_old, sfbs_sfbs_old, sfbs_sfbs_lvl_old, sfbs_time_new, sfbs_sfbs_new, sfbs_sfbs_lvl_new, MODE) values(OLD.bp_id, OLD.sfbs_time, OLD.sfbs_sfbs, OLD.sfbs_sfbs_lvl, NEW.sfbs_time, NEW.sfbs_sfbs, NEW.sfbs_sfbs_lvl, 'U'); END");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }
            //c.Execute("CREATE TABLE `TEMP_DiabetesTABLE` (`fbs_id_pointer` int, `fbs_time_new` bigint, `fbs_time_old` bigint, `fbs_fbs_new` float, `fbs_fbs_old` float, `fbs_fbs_lvl_new` integer, `fbs_fbs_lvl_old` integer, `MODE` varchar(255) )");
        }
    }
    static class MySQLDatabaseHelper
    {
        public static bool TestConnection(string url)
        {
            try
            {
                var MySQLConn = new MySqlConnection(url);
                MySQLConn.Open();
                MySQLConn.Close();
                return true;

            }
            catch
            {
                return false;
            }
        }
        public static bool GetDataFromMySQLToSQLite(string email, string password)
        {
            try
            {
                var Service = new HHCSService.HHCSService();
                var userData = Service.GetData("UserTABLE", email, password);
                var diabetesData = Service.GetData("DiabetesTABLE", email, password);
                var kidneyData = Service.GetData("KidneyTABLE", email, password);
                var pressureData = Service.GetData("PressureTABLE", email, password);
                if (userData!=null)
                {
                    foreach (DataRow row in ((DataSet)userData).Tables["UserTABLE"].Rows)
                    {
                        var tempUser = new UserTABLE();
                        tempUser.ud_id = Convert.ToInt32(row[0].ToString());
                        tempUser.ud_birthdate = DateTime.Now;
                        tempUser.ud_gender = "n/a";
                        tempUser.ud_name = row[1].ToString();
                        tempUser.ud_email = row[1].ToString();
                        tempUser.ud_pass = row[2].ToString();
                        tempUser.Insert();
                    }
                    foreach (DataRow row in ((DataSet)diabetesData).Tables["DiabetesTABLE"].Rows)
                    {
                        var tempDiabetes = new DiabetesTABLE();
                        tempDiabetes.fbs_id = Convert.ToInt32(row[0].ToString());
                        tempDiabetes.fbs_time = ((DateTime)row[1]).ToThaiLocale();
                        tempDiabetes.fbs_time_string = row[1].ToString();
                        tempDiabetes.fbs_fbs = Convert.ToDecimal(row[2].ToString());
                        tempDiabetes.fbs_fbs_lvl = Convert.ToInt32(row[3].ToString());
                        tempDiabetes.ud_id = Convert.ToInt32(row[4].ToString());
                        tempDiabetes.Insert();
                    }
                    foreach(DataRow row in ((DataSet)kidneyData).Tables["KidneyTABLE"].Rows)
                    {
                        var tempKidney = new KidneyTABLE();
                        tempKidney.ckd_id = Convert.ToInt32(row[0].ToString());
                        tempKidney.ckd_time = ((DateTime)row[1]).ToThaiLocale();
                        tempKidney.ckd_time_string = row[1].ToString();
                        tempKidney.ckd_gfr = Convert.ToDecimal(row[2].ToString());
                        tempKidney.ckd_gfr_level = Convert.ToInt32(row[3].ToString());
                        tempKidney.ckd_creatinine = Convert.ToDecimal(row[4].ToString());
                        tempKidney.ckd_bun = Convert.ToDecimal(row[5].ToString());
                        tempKidney.ckd_sodium = Convert.ToDecimal(row[6].ToString());
                        tempKidney.ckd_potassium = Convert.ToDecimal(row[7].ToString());
                        tempKidney.ckd_albumin_blood = Convert.ToDecimal(row[8].ToString());
                        tempKidney.ckd_albumin_urine = Convert.ToDecimal(row[9].ToString());
                        tempKidney.ckd_phosphorus_blood = Convert.ToDecimal(row[10].ToString());
                        tempKidney.ud_id = Convert.ToInt32(row[11].ToString());
                        tempKidney.Insert();
                    }
                    foreach (DataRow row in ((DataSet)pressureData).Tables["PressureTABLE"].Rows)
                    {
                        var tempPressure = new PressureTABLE();
                        tempPressure.bp_id = Convert.ToInt32(row[0].ToString());
                        tempPressure.bp_time = ((DateTime)row[1]).ToThaiLocale();
                        tempPressure.bp_time_string = row[1].ToString();
                        tempPressure.bp_up = Convert.ToDecimal(row[2].ToString());
                        tempPressure.bp_lo = Convert.ToDecimal(row[3].ToString());
                        tempPressure.bp_hr = Convert.ToInt32(row[4].ToString());
                        tempPressure.bp_up_lvl = Convert.ToInt32(row[5].ToString());
                        tempPressure.bp_lo_lvl = Convert.ToInt32(row[6].ToString());
                        tempPressure.bp_hr_lvl = Convert.ToInt32(row[7].ToString());
                        tempPressure.ud_id = Convert.ToInt32(row[8].ToString());
                        tempPressure.Insert();
                    }
                    var sqliteInstance = new SQLiteConnection(new SQLitePlatformAndroid(), Extension.sqliteDBPath);
                    sqliteInstance.DeleteAll<TEMP_DiabetesTABLE>();
                    sqliteInstance.DeleteAll<TEMP_KidneyTABLE>();
                    sqliteInstance.DeleteAll<TEMP_PressureTABLE>();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}