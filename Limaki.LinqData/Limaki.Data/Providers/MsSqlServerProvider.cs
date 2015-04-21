/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace Limaki.Data {

    public class MsSqlServerProvider : IDbProvider {

        public string Name {
            get { return "MsSqlServer"; }
        }

        public DbConnection GetConnection (Iori iori) {
            return new SqlConnection (ConnectionString(iori));
        }

        public int Timeout = 5;

        public void Check (Iori iori) {
            if (string.IsNullOrEmpty (iori.Extension)) {
                iori.Extension = "mdf";
            }
            if (string.IsNullOrEmpty (iori.Path)) {
                iori.Path = Directory.GetCurrentDirectory ();
            }
        }

        public string ConnectionString (Iori iori) {
            var dbName = iori.Name??"";
            Check (iori);
            var server = iori.Server;
            var localFile = iori.Server == "file";
            if (localFile) {
                server = @".\SQLExpress";
                dbName = Iori.ToFileName (iori);
            }
            var user = "user id=sa";
            var pw = string.Format ("password={0};", iori.Password);
            if (string.IsNullOrEmpty (iori.User)) {
                user = "Integrated Security=true;User Instance=false;";
                pw = "";
            } else {
                user = string.Format ("user id={0};", iori.User);
            }
            var db = string.Format ("database={0};", iori.Name);
            if(localFile)
                db += string.Format ("AttachDbFilename={0};", dbName);
            if (iori.Name == null)
                db = "";
            return string.Format (
                "server={0}; {1} {2} {3} connection timeout={4}",
                server, db, user, pw, this.Timeout);
        }

        public bool CreateDatabase (Iori iori) {
            var localFile = iori.Server == "file";
            Check (iori);
            var name = iori.Name;
            iori.Name = null;
            try {
                using (var con = GetConnection (iori) as SqlConnection) {
                    Trace.WriteLine ("Create Database on " + con.ConnectionString);
                    con.Open ();
                    iori.Name = name;
                    var command = con.CreateCommand ();
                    command.CommandText =
                        "CREATE DATABASE "+name;
                    if (localFile) {
                        var filename = Iori.ToFileName (iori);
                        command.CommandText += string.Format (" on (name={0}, filename ='{1}')", name, filename);
                        command.CommandText += string.Format (" log on (name={0}_log, filename ='{1}')", name, 
                            Path.GetDirectoryName(filename)+Path.DirectorySeparatorChar+Path.GetFileNameWithoutExtension(filename)+".ldf");
                    }
                    Trace.WriteLine ("Create Database with: " + command.CommandText);
                    command.ExecuteNonQuery ();
                }
                return true;
            } catch (SqlException ex) {
                Trace.WriteLine (string.Format ("Create Database failed: {0}", ex.Message));
                return false;
            } finally {
                iori.Name = name;
            }

        }

        public bool DropDatabase (Iori iori) {
            var localFile = iori.Server == "file";
            Check (iori);
            var name = iori.Name;
            iori.Name = null;
            try {
                using (var con = GetConnection (iori) as SqlConnection) {
                    Trace.WriteLine ("Drop Database on " + con.ConnectionString);
                    con.Open ();
                    iori.Name = name;
                    var command = con.CreateCommand ();
                    command.CommandText = "DROP DATABASE " + name;
                    Trace.WriteLine ("Drop Database with: " + command.CommandText);
                    command.ExecuteNonQuery ();
                }
                return true;
            } catch (SqlException ex) {
                Trace.WriteLine (string.Format ("Drop Database failed: {0}", ex.Message));
                return false;
            } finally {
                iori.Name = name;
            }
        }

        public bool DataBaseExists (Iori iori) {
            var timeout = this.Timeout;
            using (var connection = GetConnection (iori) as SqlConnection)
                try {
                    this.Timeout = 5;
                    connection.Open ();
                    return true;
                } catch (SqlException) {
                    return false;
                } finally {
                    this.Timeout = timeout;
                }
        }

        public bool CloseEverything () {
          
            throw new NotImplementedException ();
          
        }
    }
}