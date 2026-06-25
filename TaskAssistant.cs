using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROG6221_FINALPOE
{
    public class TaskAssistant
    {
        public List<TaskItem> Tasks = new List<TaskItem>();

        public void AddTask(string title,
                            string description,
                            DateTime? reminder)
        {
            Tasks.Add(new TaskItem()
            {
                Title = title,
                Description = description,
                ReminderDate = reminder,
                Completed = false
            });
        }

        public List<TaskItem> GetTasks()
        {
            return Tasks;
        }

        public void CompleteTask(int index)
        {
            if (index >= 0 && index < Tasks.Count)
            {
                Tasks[index].Completed = true;
            }
        }

        public void DeleteTask(int index)
        {
            if (index >= 0 && index < Tasks.Count)
            {
                Tasks.RemoveAt(index);
            }
        }
    }
}

