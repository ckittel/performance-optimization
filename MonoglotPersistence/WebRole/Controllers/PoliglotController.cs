﻿using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebRole.Controllers
{
    public class PoliglotController : ApiController
    {
        string sqlServerConnectionString = ConfigurationManager.ConnectionStrings["sqlServerConnectionString"].ConnectionString;

        // GET: api/Poliglot/5
        public string Get(int id)
        {
            string result = "";
            try
            {
                PoliglotEventSource.Log.Startup();
                PoliglotEventSource.Log.PageStart(id, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString = "SELECT Description FROM Production.ProductDescription WHERE ProductDescriptionID=@inputId";
                using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, cn))
                    {
                        cmd.Parameters.AddWithValue("@inputId", id);
                        PoliglotEventSource.Log.ReadDataStart();
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        cn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            result = String.Format("Description = {0}", reader[0]);
                        }
                        reader.Close();
                        watch.Stop();
                        PoliglotEventSource.Log.ReadDataFinish(watch.ElapsedMilliseconds);
                    }
                }
                PoliglotEventSource.Log.PageEnd();
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                PersistenceErrorEventSource.Log.Failure(ex.Message);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
            return result;
        }
        public void Post([FromBody]string value)
        {
            try
            {
                PoliglotEventSource.Log.Startup();
                PoliglotEventSource.Log.PageStart(1, this.Url.Request.RequestUri.AbsoluteUri.ToString());
                string queryString =
                    "INSERT INTO Purchasing.PurchaseOrderHeader(" +
                    " RevisionNumber, Status, EmployeeID, VendorID, ShipMethodID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, ModifiedDate)" +
                    " VALUES(" +
                    "@RevisionNumber,@Status,@EmployeeID,@VendorID,@ShipMethodID,@OrderDate,@ShipDate,@SubTotal,@TaxAmt,@Freight,@ModifiedDate)";
                var dt = DateTime.Now;
                using (SqlConnection cn = new SqlConnection(sqlServerConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, cn))
                    {
                        PoliglotEventSource.Log.WriteDataStart();

                        cmd.Parameters.Add("@RevisionNumber", SqlDbType.TinyInt).Value = 1;
                        cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = 4;
                        cmd.Parameters.Add("@EmployeeID", SqlDbType.Int).Value = 258;
                        cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = 1580;
                        cmd.Parameters.Add("@ShipMethodID", SqlDbType.Int).Value = 3;
                        cmd.Parameters.Add("@OrderDate", SqlDbType.DateTime).Value = dt;
                        cmd.Parameters.Add("@ShipDate", SqlDbType.DateTime).Value = dt;
                        cmd.Parameters.Add("@SubTotal", SqlDbType.Money).Value = 123.40;
                        cmd.Parameters.Add("@TaxAmt", SqlDbType.Money).Value = 12.34;
                        cmd.Parameters.Add("@Freight", SqlDbType.Money).Value = 5.76;
                        cmd.Parameters.Add("@ModifiedDate", SqlDbType.DateTime).Value = dt;
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        watch.Stop();
                        PoliglotEventSource.Log.WriteDataFinish(watch.ElapsedMilliseconds);
                    }
                }

                PoliglotEventSource.Log.PageEnd();
            }
            catch (Exception ex)
            {
                //SQL Server Store is probably not available, log to table storage
                PersistenceErrorEventSource.Log.Failure(ex.Message);
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
    }
}
