using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROG6221_FINALPOE
{
    // Stores all chatbot activities
    public class ActivityLogger
    {
        // List containing activity history
        public List<string> Logs { get; set; }


        public ActivityLogger()
        {
            Logs = new List<string>();
        }

        // Add activity with timestamp
        public void AddLog(string action)
        {
            string logEntry =
                "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] "
                + action;

            Logs.Add(logEntry);
        }

        // Return all logs as a single string
        public string ShowLogs()
        {
            if (Logs.Count == 0)
            {
                return "No activity recorded yet.";
            }

            return string.Join(Environment.NewLine, Logs);
        }

        // Clear logs
        public void ClearLogs()
        {
            Logs.Clear();
        }

        // Total logs count
        public int CountLogs()
        {
            return Logs.Count;
        }
    }

}