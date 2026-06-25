using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PROG6221_FINALPOE
{
    /// <summary>
    /// Interaction logic for TaskManagement.xaml
    /// </summary>
    public partial class TaskManagement : Window
    {

        private TaskRepository repository;
        private string username;

        public TaskManagement(string username)
        {
            InitializeComponent();
            this.username = username;
            repository = new TaskRepository();
            LoadTasks();
        }

        private void LoadTasks()
        {
            DataTable tasks = repository.GetTasks(username);
            TasksListView.ItemsSource = tasks.DefaultView;
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleBox.Text.Trim();
            string description = TaskDescriptionBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? reminderDate = null;

            if (ReminderTimeBox.SelectedItem != null)
            {
                string reminder = ((ComboBoxItem)ReminderTimeBox.SelectedItem).Content.ToString();
                if (reminder != "None")
                {
                    reminderDate = DateTime.Now;
                    switch (reminder)
                    {
                        case "Today":
                            reminderDate = DateTime.Now.Date;
                            break;
                        case "Tomorrow":
                            reminderDate = DateTime.Now.Date.AddDays(1);
                            break;
                        case "3 days":
                            reminderDate = DateTime.Now.Date.AddDays(3);
                            break;
                        case "1 week":
                            reminderDate = DateTime.Now.Date.AddDays(7);
                            break;
                    }
                }
            }

            if (ReminderDatePicker.SelectedDate.HasValue && reminderDate == null)
            {
                reminderDate = ReminderDatePicker.SelectedDate.Value;
            }

            repository.AddTask(username, title, description, reminderDate);
            LoadTasks();

            TaskTitleBox.Clear();
            TaskDescriptionBox.Clear();
            ReminderDatePicker.SelectedDate = null;
            ReminderTimeBox.SelectedIndex = 0;

            MessageBox.Show("Task added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TaskCompleted_Click(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var row = checkbox.DataContext as DataRowView;
            if (row != null)
            {
                int taskId = Convert.ToInt32(row["id"]);
                repository.MarkTaskCompleted(taskId);
                LoadTasks();
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int taskId = Convert.ToInt32(button.Tag);
                if (MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    repository.DeleteTask(taskId);
                    LoadTasks();
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}