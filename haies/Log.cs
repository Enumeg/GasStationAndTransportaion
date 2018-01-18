using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Source;
namespace haies
{
    public class Log
    {
        public Log()
        {
            Columns = new List<Column>();
        }
        public List<Column> Columns { get; set; }
        public void CreateLog(string entity, bool isInsert)
        {

            var database = new DB("Users_log");
            database.AddColumn("log_user_id", App.User.Id);
            database.AddColumn("log_entity", entity);
            database.AddColumn("log_action", isInsert ? Actions.Insert : Actions.Update);
            database.AddColumn("log_details", GetDetails());

            database.Insert();
        }
        public void CreateLog(string entity)
        {
            var database = new DB("Users_log");
            database.AddColumn("log_user_id", App.User.Id);
            database.AddColumn("log_entity", entity);
            database.AddColumn("log_action", Actions.Delete);
            database.AddColumn("log_details", GetDetails());

            database.Insert();
        }

        private string GetDetails()
        {
            string details = "";
            Columns.ForEach(prop =>
            {
                details += prop.Name + " : " + prop.Value + " | ";
            });
            if (details != "")
                details = details.Remove(details.Length - 3, 3);
            return details;
        }

    }
}
