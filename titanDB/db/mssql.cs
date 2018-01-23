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
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace titan {
    public static partial class db{
            public static List<string> queries=new List<string>();

            public static SqlParameter[] dictonary2Parameter(Dictionary<string,string> parameters) {
                List<SqlParameter> parameterCollection=new List<SqlParameter>();

                foreach (KeyValuePair<string, string> parameter in parameters) {
                    if(parameter.Key[0]!='@') {
                        parameterCollection.Add(new SqlParameter("@" + parameter.Key, parameter.Value));
                    } else {
                        parameterCollection.Add(new SqlParameter(parameter.Key, parameter.Value));
                    }
                }
                return parameterCollection.ToArray();
            }
            public static SqlParameter[] string2Param(string[,] parameters) {
                List<SqlParameter> parameterCollection=new List<SqlParameter>();
                for(int i=0;i<parameters.GetLength(0);i++) {
                    if(parameters.GetLength(1)!=2) return null;
                    if(parameters[i,0][0]!='@') {
                        parameterCollection.Add(new SqlParameter("@"+parameters[i,0],parameters[i,1]));
                    } else {
                        parameterCollection.Add(new SqlParameter(parameters[i,0],parameters[i,1]));
                    }
                }
                return parameterCollection.ToArray();
            }
            public static void Log(string data) {
                try {
                    queries.Add(data);
                    //mango.Utils.Log.Write(Enroute.Utils.LogSeverity.Verbose,data);
                    System.Diagnostics.Debug.WriteLine(data);
                }catch(Exception ex) {
                    
                }
            }
            public static SqlParameter param_compare (SqlParameter x,SqlParameter y){
                if(x.ParameterName.Length>y.ParameterName.Length) return x;
                return y;
            }
            public static string extract_query(string connection_string,string query,SqlParameter[] parameters) {
                StringBuilder o=new StringBuilder();
                if(null!=parameters) {
                    if(parameters.Length>1) {
                        Array.Sort<SqlParameter>(parameters,(x,y) =>{
                            return y.ParameterName.Length.CompareTo(x.ParameterName.Length); } );
                    
                    }
                    foreach (SqlParameter p in parameters) {
                        string name=p.ParameterName;
                        string value="";
                        if(null!=p &&null!= p.Value) value=p.Value.ToString().Replace("'","''");
                        if(name[0]!='@') {
                        
                        
                        //    query=query.Replace("@"+name,"'"+value+"'");
                        } else {
                        //    query=query.Replace(name,"'"+value+"'");
                        }
                        int n;
                        bool isNumeric = int.TryParse(value, out n);

                        if (value=="True") {
                            o.Append(String.Format("DECLARE {0,-30}     bit=1;       --TRUE\r\n",name));
                        } else
                        if(value=="False") {
                            o.Append(String.Format("DECLARE {0,-30}     bit=0;       --FALSE\r\n",name));
                        } else
                        if(isNumeric==true) {
                            o.Append(String.Format("DECLARE {0,-30}     int={1};\r\n",name,value));
                        }
                        else { 
                            if(value.Length>=4000) {
                                o.Append(String.Format("DECLARE {0,-30}     varchar({2})='{1}';\r\n",name,value,value.Length+10));
                            } else {
                                o.Append(String.Format("DECLARE {0,-30}     nchar({2})='{1}';\r\n",name,value,value.Length+10));
                            }   
                        }
                    }
                }
                o.Append("\r\n--"+connection_string+"\r\n");
                o.Append(query);
                return o.ToString();
            }
            public static void log_query(string connection_string,string query,SqlParameter[] parameters) {
                //Log(extract_query(connection_string,query,parameters));
            }

            /*This function updates the targeet DB/connection based on settings. A core switch for titan queries.*/
            public static void update_connection_and_destination(ref string query,string src,ref string connection) {
                string titan_connection_string="";
                string titan_database="";
                if(src.ToLower()=="titan") {
                    titan_database         =ConfigurationManager.AppSettings["titan_db"];
                    titan_connection_string=ConfigurationManager.AppSettings["titan_connection"];
                    if (!String.IsNullOrWhiteSpace(titan_database)) {
                        query=String.Format("Use {0}; {1} ",titan_database,query);
                    }
                } 

                if(string.IsNullOrWhiteSpace(src) ) {                                       //no connection string present and not overriden, use local
                    connection=ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;
                } else {
                    if(src=="titan") {                                                      //it is overriden by titan
                      if(!String.IsNullOrWhiteSpace(titan_connection_string)) {             //its a titan query, with a specific titan database
                            connection=ConfigurationManager.ConnectionStrings[titan_connection_string].ConnectionString;
                        } else {                                                            //if its a titan query but no titan connection is specified use local
                            connection=ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString;       
                        }
                    } else {
                        if(!string.IsNullOrWhiteSpace(src) && src!="titan"){                //if its overridden and not a titan query...
                            connection=src;
                        }
                    }//end last else
                }//end first if string null
            }//end function
        }//end class
}//end namespace
