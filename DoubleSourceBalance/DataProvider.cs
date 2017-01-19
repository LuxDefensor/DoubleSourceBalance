using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace DoubleSourceBalance
{
    static class DataProvider
    {
        #region Piramida2000
        private static double PiramidaConsumptionInterval(Source source,
            string channelID, DateTime dtStart, DateTime dtEnd)
        {
            object result;
            using (SqlConnection cn = new SqlConnection(ConnectionString(source)))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка подключения к БД " + Environment.NewLine +
                        "Connection string: " + cn.ConnectionString, ex);
                }
                SqlCommand cmd = cn.CreateCommand();
                StringBuilder sql = new StringBuilder();
                SqlDataReader dr;
                string objectCode = "-1";
                string itemCode = "-1";
                sql.Append("select DEVICES.CODE,SENSORS.CODE ");
                sql.Append("FROM DEVICES inner join SENSORS on SENSORS.STATIONID = DEVICES.id ");
                sql.Append("WHERE SENSORS.ID = " + channelID);
                cmd.CommandText = sql.ToString();
                try
                {
                    dr = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    if (dr.Read())
                    {
                        if (dr[0] == null || Convert.IsDBNull(dr[0]))
                            throw new Exception("Ошибка при выполнении запроса " + cmd.CommandText);
                        if (dr[1] == null || Convert.IsDBNull(dr[1]))
                            throw new Exception("Ошибка при выполнении запроса " + cmd.CommandText);
                        objectCode = dr[0].ToString();
                        itemCode = dr[1].ToString();
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + cmd.CommandText + Environment.NewLine +
                        ex.Message);
                }
                sql.Clear();
                sql.Append("select sum(value0)/2 consumption from DATA ");
                sql.AppendFormat("WHERE PARNUMBER = 12 AND OBJECT = {0} AND ITEM = {1} ",
                    objectCode, itemCode);
                sql.AppendFormat("AND DATA_DATE Between '{0}' AND '{1}'",
                    dtStart.Date.AddMinutes(30).ToString("yyyyMMdd HH:mm"),
                    dtEnd.Date.ToString("yyyyMMdd"));
                cmd.CommandText = sql.ToString();
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + cmd.CommandText +
                        Environment.NewLine + ex.Message);
                }
                if (result == null || Convert.IsDBNull(result))
                    return 0;
                else
                    return Convert.ToDouble(result);
            }
        }

        private static double PiramidaConsumptionIntegral(Source source,
            string channelID, DateTime dtStart, DateTime dtEnd)
        {
            object result;
            using (SqlConnection cn = new SqlConnection(ConnectionString(source)))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка подключения к БД " + Environment.NewLine +
                        "Connection string: " + cn.ConnectionString, ex);
                }
                SqlCommand cmd = cn.CreateCommand();
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("declare @objCode int");
                sql.AppendLine("declare @itemCode int");
                sql.AppendLine("declare @ktr float");
                sql.AppendLine("declare @first float");
                sql.AppendLine("declare @last float");
                sql.AppendLine();
                sql.AppendLine("select @objCode = DEVICES.CODE, @itemCode = SENSORS.CODE");
                sql.AppendLine("FROM DEVICES inner join SENSORS on SENSORS.STATIONID = DEVICES.id");
                sql.AppendFormat("WHERE SENSORS.ID = {0}", channelID);
                sql.AppendLine();
                sql.AppendFormat("select @ktr = koef from KTR WHERE SensorID = {0}", channelID);
                sql.AppendLine();
                sql.AppendLine();
                sql.AppendLine("select @first = VALUE0 FROM DATA");
                sql.AppendLine("WHERE PARNUMBER = 101 AND OBJECT = @objCode AND ITEM = @itemCode");
                sql.AppendFormat("AND DATA_DATE = '{0}'", dtStart.ToString("yyyyMMdd"));
                sql.AppendLine();
                sql.AppendLine();
                sql.AppendLine("select @last = VALUE0 from DATA");
                sql.AppendLine("WHERE PARNUMBER = 101 AND OBJECT = @objCode AND ITEM = @itemCode");
                sql.AppendFormat("AND DATA_DATE = '{0}'", dtEnd.ToString("yyyyMMdd"));
                sql.AppendLine();
                sql.AppendLine();
                sql.Append("select(@last - @first) * @ktr consumption");
                cmd.CommandText = sql.ToString().Replace("\r\n", Environment.NewLine);
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + Environment.NewLine +
                        cmd.CommandText, ex);
                }
                if (result == null || Convert.IsDBNull(result))
                    return 0;
                else
                    return Convert.ToDouble(result);
            }
        }
        #endregion

        #region Energosphere

        private static double EnergosphereConsumptionIntegral(Source source,
           string channelID, DateTime dtStart, DateTime dtEnd)
        {
            using (SqlConnection cn = new SqlConnection(ConnectionString(source)))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка подключения к БД " + Environment.NewLine +
                        "Connection string: " + cn.ConnectionString, ex);
                }
                SqlCommand cmd = cn.CreateCommand();
                StringBuilder sql = new StringBuilder();

                // Find meter's channel and coefficient
                sql.Append("select id_ref,coeff from SchemaContents ");
                sql.AppendFormat("where RefIsPoint = 2 and ID_PP = {0} and '{1}' between DT1 and DT2",
                    channelID, dtStart);
                cmd.CommandText = sql.ToString();
                SqlDataReader dr;
                try
                {
                    dr = cmd.ExecuteReader(CommandBehavior.SingleRow);
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось получить информацию о канале " + channelID + Environment.NewLine +
                        "Source: " + source + Environment.NewLine + cmd.CommandText, ex);
                }
                int meterChannel;
                double ktr;
                if (dr.Read())
                {
                    if (dr[0] == null || Convert.IsDBNull(dr[0]))
                        throw new Exception("Запрос вернул пустые значения",
                                            new Exception(cmd.CommandText));
                    if (dr[1] == null || Convert.IsDBNull(dr[1]))
                        throw new Exception("Запрос вернул пустые значения",
                                            new Exception(cmd.CommandText));
                    meterChannel = Convert.ToInt32(dr[0]);
                    ktr = Convert.ToDouble(dr[1]);
                }
                else
                {
                    throw new Exception("Запрос вернул пустой набор строк",
                                        new Exception(cmd.CommandText));
                }
                if (!dr.IsClosed)
                    dr.Close();
                // Find starting fixed value
                sql.Clear();
                sql.AppendFormat("select sum(Val) consumption from PointNIs_On_Main_Stack where ID_PP={0} and DT ='{1}'",
                                 meterChannel, dtStart.ToString("yyyyMMdd"));
                cmd.CommandText = sql.ToString();
                object objValue;
                double firstValue, lastValue;
                try
                {
                    objValue = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + Environment.NewLine + cmd.CommandText, ex);
                }
                if (objValue == null || Convert.IsDBNull(objValue))
                    return 0;
                firstValue = Convert.ToDouble(objValue);

                // Find ending fixed value
                sql.Clear();
                sql.AppendFormat("select sum(Val) consumption from PointNIs_On_Main_Stack where ID_PP={0} and DT ='{1}'",
                                 meterChannel, dtEnd.ToString("yyyyMMdd"));
                cmd.CommandText = sql.ToString();
                try
                {
                    objValue = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + Environment.NewLine + cmd.CommandText, ex);
                }
                if (objValue == null || Convert.IsDBNull(objValue))
                    return 0;
                lastValue = Convert.ToDouble(objValue);
                return (lastValue - firstValue) * ktr;
            }
        }

        private static double EnergosphereConsumptionInterval(Source source,
          string channelID, DateTime dtStart, DateTime dtEnd)
        {
            object result;
            using (SqlConnection cn = new SqlConnection(ConnectionString(source)))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка подключения к БД " + Environment.NewLine +
                        "Connection string: " + cn.ConnectionString, ex);
                }
                SqlCommand cmd = cn.CreateCommand();
                StringBuilder sql = new StringBuilder();               
                sql.Append("select sum(m.Val) consumption from PointMains m ");
                sql.AppendFormat("where m.ID_PP={0} ", channelID);
                sql.AppendFormat("and dt between '{0}' and '{1}'",
                    dtStart.ToString("yyyyMMdd"), dtEnd.AddMinutes(-30).ToString("yyyyMMdd HH:mm"));
                cmd.CommandText = sql.ToString();
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка выполнения запроса " + Environment.NewLine + cmd.CommandText, ex);
                }
                if (result == null || Convert.IsDBNull(result))
                    throw new Exception("Запрос вернул пустое значение" +
                        Environment.NewLine + cmd.CommandText);
                return Convert.ToDouble(result);
            }
        }

        #endregion

        #region Both
        private static string ConnectionString(Source source)
        {
            SqlConnectionStringBuilder cs = new SqlConnectionStringBuilder();
            cs.DataSource = source.Server;
            cs.InitialCatalog = source.Database;
            cs.UserID = source.User;
            cs.Password = source.Password;
            cs.ConnectTimeout = 300;
            return cs.ConnectionString;
        }

        public static string ChannelName(Source source, string channelID)
        {
            object result;
            using (SqlConnection cn = new SqlConnection(ConnectionString(source)))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    formErrorMessage dlg = new formErrorMessage("Операция: получение имени канала",
                            new Tuple<string, string>("Не удалось подключиться к базе данных:",
                            ex.Message + Environment.NewLine + "Connection string = " +
                            cn.ConnectionString));
                    dlg.ShowDialog();
                    return "";
                }
                SqlCommand cmd = cn.CreateCommand();
                cmd.CommandText = source.SQL.Replace("{x}", channelID);
                try
                {
                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    formErrorMessage dlg = new formErrorMessage("Операция: получение имени канала",
                        new Tuple<string, string>("Ошибка при выполнении запроса к БД",
                        ex.Message + Environment.NewLine + Environment.NewLine + cmd.CommandText));
                    dlg.ShowDialog();
                    return "";
                }
            }
            if (result == null || Convert.IsDBNull(result))
            {
                formErrorMessage dlg = new formErrorMessage("Операция: получение имени канала",
                    new Tuple<string, string>("Запрос вернул пустое значение",
                    source.SQL.Replace("{x}", channelID)));
                dlg.ShowDialog();
                return "";
            }
            return result.ToString();
        }

        public static double GetConsumption(Source source, BalanceComponent channel,
            DateTime dtStart,DateTime dtEnd)
        {
            double result;
            switch (source.Id)
            {
                case "1":
                    switch (channel.Method)
                    {
                        case CalculateMethods.integral:
                            result = PiramidaConsumptionIntegral(source, channel.Channel,
                                dtStart, dtEnd);
                            break;
                        case CalculateMethods.interval:
                            result = PiramidaConsumptionInterval(source, channel.Channel,
                                dtStart, dtEnd);
                            break;
                        default:
                            throw new Exception("Должны быть либо получасовки, либо показания");
                            // It will never reach this
                    }
                    break;
                case "2":
                    switch (channel.Method)
                    {                        
                        case CalculateMethods.integral:
                            result = EnergosphereConsumptionIntegral(source, channel.Channel,
                                dtStart, dtEnd);
                            break;
                        case CalculateMethods.interval:
                            result = EnergosphereConsumptionInterval(source, channel.Channel,
                                dtStart, dtEnd);
                            break;
                        default:
                            throw new Exception("Должны быть либо получасовки, либо показания");
                            // It will never reach this
                    }
                    break;
                default:
                    throw new Exception("Невозможно использовать источник данных: " + source.Id);
            }
            return channel.Sign == "-1" ? result * (-1) : result;
        }
        #endregion
    }
}
