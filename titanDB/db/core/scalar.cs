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
using System.Collections.Generic;
using System.Data.SqlClient;

namespace titan {
    public static partial class db {
            public static void execute_scalar(string src,string query,Dictionary<string,string> parameters) {
                execute_scalar(src,query,dictonary2Parameter(parameters));
            }
            public static string execute_scalar(string src,string query,SqlParameter[] parameters) {
                string result="";
                try{
                    string connection="";
                    update_connection_and_destination(ref query,src,ref connection);
                    log_query(connection,query,parameters);
                    using (SqlConnection conn = new SqlConnection(connection)) {
                        conn.Open();
                        if(conn.State!=System.Data.ConnectionState.Open) return null;                           //no connection DIE
                        using (SqlCommand comm = new SqlCommand("SET ARITHABORT ON", conn)) {
                                comm.ExecuteNonQuery();
                        }
                        SqlCommand command = new SqlCommand(query, conn);
                        if(null!=parameters) {
                            command.Parameters.AddRange(parameters);
                        }
                        result=command.ExecuteScalar().ToString();
                        conn.Close();                                   //close it out
                        conn.Dispose();                                 //clear it (using does this...)
                    }//end using
                }catch (Exception e){
                    //log.error("DB::query", e.ToString());
                }
                return result;
            }//end function
    }//end class
}//end namespace