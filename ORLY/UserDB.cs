using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrlyBot
{
    class UserDB
    {

        //This class allows to query and push dynamic json objects to use a folder as a whole database
        //There is automatic caching of the objects for reading and writing so that it doesn't query the install drive
        //when reading is only needed.

        internal Dictionary<string, dynamic> dbCache = new Dictionary<string, dynamic>();

        static UserDB _db = null;
        public static UserDB db
        {
            get
            {
                if (_db == null)
                    _db = new UserDB();

                return _db;
            }
        }

        internal dynamic GetDatabaseObject(string dbPath)
        {

            try
            {
                if(dbCache.TryGetValue(dbPath, out dynamic val))
                {
                    return val;
                }


                if (System.IO.File.Exists(dbPath))
                {
                    var db = System.IO.File.ReadAllText(dbPath);

                    var converter = new ExpandoObjectConverter();
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(db, converter);

                    dbCache[dbPath] = obj;

                    return obj;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogMessage msg = new LogMessage(LogSeverity.Error, "UserDB", "Could not load database file", ex);
                Globals.logger.OnLogAsync(msg).Start();
                return null;
            }
        }
        internal void SetDatabaseObject(string dbPath, dynamic db)
        {
            dbCache[dbPath] = db;

            string json = "";

            try
            {
                var directory = Path.GetDirectoryName(dbPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                json = Newtonsoft.Json.JsonConvert.SerializeObject(db);
                File.WriteAllText(dbPath, json);


            }
            catch(Exception ex)
            {
                LogMessage msg = new LogMessage(LogSeverity.Error, "UserDB", $"Could not save database for user. JSON Contained: {json}", ex);
                Globals.logger.OnLogAsync(msg).Start();
            }
        }
        internal dynamic GetUser(string server_id, string user_id)
        {
            return GetDatabaseObject(Path.Combine(Globals.baseDir.FullName,server_id,$"USER_{user_id}.json"));
        }
        internal dynamic SetUser(string server_id, string user_id, List<string> roles)
        {
            dynamic dbUser = new ExpandoObject();
            dbUser.user_id = user_id;
            dbUser.roles = roles;

            SetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"USER_{user_id}.json"), dbUser);

            return dbUser;
        }

        internal async Task<dynamic> UpdateUserRoles(string server_id, string user_id, List<string> roles)
        {
            dynamic dbUser = GetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"USER_{user_id}.json"));
            dbUser.roles = roles;
            SetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"USER_{user_id}.json"), dbUser);

            return dbUser;
        }
        internal async Task<dynamic> UpdateUserTimestamps(string server_id, string user_id, List<msgTimestamp> timestamps)
        {
            dynamic dbUser = GetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"USER_{user_id}.json"));
            dbUser.timestamps = timestamps;
            SetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"USER_{user_id}.json"), dbUser);

            return dbUser;
        }

        internal dynamic GetDB(string server_id, string database_id)
        {
            return GetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"DB_{database_id}.json"));
        }
        internal void SetDB(string server_id, string database_id, dynamic db)
        {
            SetDatabaseObject(Path.Combine(Globals.baseDir.FullName, server_id, $"DB_{database_id}.json"), db);
        }

    }
}
