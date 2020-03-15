﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Silkier.Extensions
{
    public static class DataExtension
    {
        public static IDbCommand CreateCommand(this IDbConnection db, string commandText)
        {
            var cmd = db.CreateCommand();
            cmd.CommandText = commandText;
            return cmd;
        }

        public static IDbCommand SetCommandTimeout(this IDbCommand command, TimeSpan span)
        {
            command.CommandTimeout = (int)span.TotalSeconds;
            return command;
        }
        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static List<T> ExecuteList<T>(this IDbCommand command) where T : class
        {
            return command.ExecuteReader().ToList<T>();
        }
        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static List<T> ExecuteTupleLis<T>(this IDbCommand command)
        {
            return command.ExecuteReader().ToTupleList<T>();
        }
        public static string ToISO8601(this System.DateTime time) => time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:sssZ");

        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static T ToJson<T>(this IDataReader dataReader) where T : class
        {
            return dataReader.ToJson().ToObject<T>();
        }



        public static JArray ToJson(this IDataReader dataReader)
        {
            JArray jArray = new JArray();
            try
            {
                while (dataReader.Read())
                {
                    JObject jObject = new JObject();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        try
                        {
                            string strKey = dataReader.GetName(i);
                            if (dataReader[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dataReader[i], dataReader.GetFieldType(i));
                                jObject.Add(strKey, JToken.FromObject(obj));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                dataReader.Close();
            }
            return jArray;
        }



        public static JArray ToJson(this DataTable dt)
        {
            JArray jArray = new JArray();
            try
            {
                for (int il = 0; il < dt.Rows.Count; il++)
                {
                    JObject jObject = new JObject();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        try
                        {
                            string strKey = dt.Columns[i].ColumnName;
                            if (dt.Rows[il].ItemArray[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dt.Rows[il].ItemArray[i], dt.Columns[i].DataType);
                                jObject.Add(strKey, JToken.FromObject(obj));
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return jArray;
        }
        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static List<T> ToList<T>(this DataTable dt) where T : class
        {
            List<T> jArray = new List<T>();
            var prs = typeof(T).GetProperties();
            try
            {
                for (int il = 0; il < dt.Rows.Count; il++)
                {
                    T jObject = Activator.CreateInstance<T>();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        try
                        {
                            string strKey = dt.Columns[i].ColumnName;
                            if (dt.Rows[il].ItemArray[i] != DBNull.Value)
                            {
                                object obj = Convert.ChangeType(dt.Rows[il].ItemArray[i], dt.Columns[i].DataType);

                                var p = prs.FirstOrDefault(px => px.Name.ToLower() == strKey.ToLower());
                                if (p != null)
                                {
                                    SetValue(jObject, dt.Columns[i].DataType, obj, p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return jArray;
        }

        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static List<T> ToList<T>(this IDataReader dataReader) where T : class
        {
            List<T> jArray = new List<T>();
            var prs = typeof(T).GetProperties();
            try
            {
                while (dataReader.Read())
                {
                    T jObject = Activator.CreateInstance<T>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        try
                        {
                            string strKey = dataReader.GetName(i);

                            if (dataReader[i] != DBNull.Value)
                            {
                                var ft = dataReader.GetFieldType(i);
                                var _v = dataReader[i];
                                object obj = Convert.ChangeType(_v, ft);
                                var p = prs.FirstOrDefault(px => px.Name.ToLower() == strKey.ToLower());
                                if (p != null)
                                {
                                    SetValue(jObject, ft, obj, p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                dataReader.Close();
            }
            return jArray;
        }
        [Obsolete("请使用Silkier.EFCore.DbReaderExtensions中的方法")]
        public static List<T> ToTupleList<T>(this IDataReader dataReader)
        {
            List<T> jArray = new List<T>();
            var prs = typeof(T).GetFields();
            try
            {
                while (dataReader.Read())
                {
                    T jObject = Activator.CreateInstance<T>();
                    object jObjectEx = jObject;
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        try
                        {
                            string strKey = dataReader.GetName(i);

                            if (dataReader[i] != DBNull.Value)
                            {
                                var ft = dataReader.GetFieldType(i);
                                var _v = dataReader[i];
                                object obj = Convert.ChangeType(_v, ft);
                                var p = prs[i];
                                if (p != null)
                                {
                                    SetValue(jObjectEx, ft, obj, p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    jObject = (T)Convert.ChangeType(jObjectEx, typeof(T));
                    jArray.Add(jObject);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                dataReader.Close();
            }
            return jArray;
        }

        private static void SetValue<T>(T jObject, Type ft, object obj, System.Reflection.FieldInfo p)
        {
            if (p.FieldType == ft)
            {
                p.SetValue(jObject, obj);
            }
            else if (p.FieldType == typeof(DateTime) && ft == typeof(string))
            {
                if (DateTime.TryParse((string)obj, out DateTime dt))
                {
                    p.SetValue(jObject, dt);
                }
            }
            else
            {
                p.SetValue(jObject, Convert.ChangeType(obj, p.FieldType));
            }
        }
        private static void SetValue<T>(T jObject, Type ft, object obj, System.Reflection.PropertyInfo p) where T : class
        {
            if (p.PropertyType == ft)
            {
                p.SetValue(jObject, obj);
            }
            else if (p.PropertyType == typeof(DateTime) && ft == typeof(string))
            {
                if (DateTime.TryParse((string)obj, out DateTime dt))
                {
                    p.SetValue(jObject, dt);
                }
            }
            else
            {
                p.SetValue(jObject, Convert.ChangeType(obj, p.PropertyType));
            }
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

    }
}
