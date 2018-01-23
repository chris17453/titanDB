/******************************************************************** 
        ████████╗██╗████████╗ █████╗ ███╗   ██╗██████╗ ██████╗ 
        ╚══██╔══╝██║╚══██╔══╝██╔══██╗████╗  ██║██╔══██╗██╔══██╗
           ██║   ██║   ██║   ███████║██╔██╗ ██║██║  ██║██████╔╝
           ██║   ██║   ██║   ██╔══██║██║╚██╗██║██║  ██║██╔══██╗
           ██║   ██║   ██║   ██║  ██║██║ ╚████║██████╔╝██████╔╝
           ╚═╝   ╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═════╝ 
*********************************************************************
   Created  : 01-23-2017
   Author   : Charles Watkins
   Email    : chris17453@gmail.com
   GitHub   : http://github.com/chris17453
   LinkedIn : http://linkedin.com/chris17453
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace titan {
    public static partial class db{

        public static Hashtable fetch(string src,string query){

            List<Hashtable> ht=fetchAll(src,query,(SqlParameter[])null,true);
            if(ht!=null && ht.Count>0) {
                return ht[0];
            }
            return null;
        }
        
        public static Hashtable fetch(string src,string query,string[,] param){

            List<Hashtable> ht=fetchAll(src,query,param,true);
            if(ht!=null && ht.Count>0) {
                return ht[0];
            }
            return null;
        }

        public static Hashtable fetch(string src,string query,Dictionary<string,string> parameters=null){
            List<Hashtable> ht=fetchAll(src,query,parameters,true);
            if(ht!=null && ht.Count>0) {
                return ht[0];
            }
            return null;
        }
        public static Hashtable fetch(string src,string query,SqlParameter[] parameters=null){
            List<Hashtable> ht=fetchAll(src,query,parameters,true);
            if(ht!=null && ht.Count>0) {
                return ht[0];
            }
            return null;
        }

        public static List<Hashtable> fetchAll(string src,string query){
            return fetchAll(src,query,(SqlParameter [])null,false);
        }
        public static List<Hashtable> fetchAll(string src,string query,string [,] param=null,bool single=false){

            return fetchAll(src,query,string2Param(param),single);
        }

        public static List<Hashtable> fetchAll(string src,string query,Dictionary<string,string> parameters=null,bool single=false){
                return fetchAll(src,query,dictonary2Parameter(parameters),single);
        }

        public static List<Hashtable> fetchAll(string src,string query,SqlParameter[] parameters=null,bool single=false){
            List <Hashtable> results=new List <Hashtable>();
            try{
                string connection="";
                update_connection_and_destination(ref query,src,ref connection);
                log_query(connection,query,parameters);
                
                using (SqlConnection conn = new SqlConnection(connection)) {
                    conn.Open();
                    if(conn.State!=System.Data.ConnectionState.Open) return null;                               //no connection DIE
                    using (SqlCommand comm = new SqlCommand("SET ARITHABORT ON", conn)) {
                            comm.ExecuteNonQuery();
                    }
                    SqlDataReader reader = null;
                    SqlCommand command = new SqlCommand(query, conn);
                
                    if(null!=parameters) {
                        command.Parameters.AddRange(parameters);                                                //parameters from wherever...
                    }

                    reader = command.ExecuteReader();
                    if (reader.HasRows) {
                        Log(String.Format("\r\nRows Returned"));

                        while (reader.Read()) {
                            Hashtable result = new Hashtable();
                            for (int i = 0; i < reader.FieldCount; i++) {
                                object data="";
                                if(null!=reader[i] && !reader.IsDBNull(i)) data=reader[i];//.ToString().Trim();
                                string column=reader.GetName(i);
                                Type t=reader[i].GetType();
                                if(Type.GetTypeCode(t)==TypeCode.String) {
                                    
                                    result[column] =((string)data).Trim();                                      //so much whitespace
                                } else {
                                    result[column] =data;
                                }
                            }//end field loop
                            results.Add(result);
                            if(single) break;                       //if we only want 1 row....
                        }//end while
                    }//end if reader
                    else {
                        Log(String.Format("\r\nNO Rows Returned"));
                    }
                    reader.Close();                                 //close this out as well..
                    conn.Close();                                   //close it out
                    conn.Dispose();                                 //clear it (using does this...)
                }//end using
            }catch (Exception e){
                Log("DB::query Error: "+ e.ToString());
            }
            return results;
        }
    }
}