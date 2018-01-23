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
using System.Linq;

namespace titan {
    public static class crud{

        /*
            Dataase optional...
        */
        public static int create(string connection_string,string table,Hashtable data) {
            List<string> columns=new List<string>();
            List<string> values =new List<string>();
            List<SqlParameter> param=new List<SqlParameter>();

            if (String.IsNullOrWhiteSpace(table)) {
                db.Log(String.Format("CRUD: Cannot Create sql statement. No Table {0},{1}",connection_string,table));
                return -1;
            }

            if (data.Keys.Count==0) {
                db.Log(String.Format("CRUD: Cannot Create sql statement. No Data {0},{1}",connection_string,table));
                return -1;
            }
            

            //foreach (KeyValuePair<string,object> v in data){
            
            foreach (DictionaryEntry v in data){
                if(null==v.Value || String.IsNullOrWhiteSpace(v.Key.ToString())) {                         //dont add null columns or values.
                    continue;
                }
                columns.Add("["+v.Key+"]");
                values.Add("@"+v.Key);
                param.Add(new SqlParameter("@"+v.Key,v.Value.ToString().Trim()));
            }

            if(columns.Count()==0 || values.Count()==0) {                                       //exit if nothing to process
                db.Log(String.Format("CRUD: Cannot Create sql statement. {0},{1}",connection_string,table));
                return -1;
            }

            string query=String.Format("INSERT INTO {0} ({1}) VALUES ({2}); SELECT SCOPE_IDENTITY()",   table,
                                                                               String.Join(",",columns.ToArray()),
                                                                               String.Join(",",values.ToArray()));
            string res=db.execute_scalar(connection_string,query,param.ToArray());
            int id=0;
            Int32.TryParse(res,out id);
            return id;
        }
        public static Hashtable remove_param_data(Hashtable data,string[,] parameters) {
            for(int index=0;index<parameters.GetLength(0);index++){
                string column=parameters[index,0];
                if(column[0]=='@') column=column.Substring(1);
                if (data.ContainsKey(column)) {
                    data.Remove(column);
                }//end if data contains
            }//end for
            return data;
        }
        public static int update(string connection_string,string table,Hashtable data,string[,] parameters) {
            List<string>        calc_where=new List<string>();
            List<string>        calc_data =new List<string>();
            List<SqlParameter>  param     =new List<SqlParameter>();

            data =remove_param_data(data,parameters);
            if (null==parameters || parameters.GetLength(0)==0) {
                db.Log(String.Format("CRUD: Canot UPDATE. No Parameters -> {0},{1}",connection_string,table));
                return -1;
            }
            for (int index=0;index<parameters.GetLength(0);index++){
                calc_where.Add(String.Format("[{0}]=@{0}",parameters[index,0]));
                param.Add(new SqlParameter(parameters[index,0],parameters[index,1].Trim()));
            }

            foreach (DictionaryEntry v in data){
                calc_data.Add(String.Format("[{0}]=@{0}",v.Key));
                param.Add(new SqlParameter("@"+v.Key,v.Value.ToString()));
            }

            string query=String.Format("UPDATE {0} SET {1} WHERE {2}", table,
                                                                       String.Join(","  ,calc_data.ToArray()),
                                                                       String.Join("AND",calc_where.ToArray()));
            db.execute_scalar(connection_string,query,param.ToArray());
            return 1;
        }


        /*public static bool fetch(string connection_string,string table,Hashtable data,string[,] parameters) {
            List<SqlParameter>  param     =new List<SqlParameter>();
            //db.fetch(connection_string,query
            return false;
        }

        public static bool delete(string connection_string,string table,Hashtable data,string[,] parameters) {
            return false;
        }*/
    }
}