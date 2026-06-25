using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PROG6221_FINALPOE

{//start of namespace

    public partial class MainWindow : Window
    {//start of class


        //creating an instance for the class Array
        ArrayList reply = new ArrayList();
        ArrayList ignore = new ArrayList();
        user_name check_name = new user_name();

        TaskAssistant taskManager = new TaskAssistant();

        bool waitingForReminder = false;

        string pendingTaskTitle = "";

        string pendingTaskDescription = "";

        // variables
        string username = string.Empty;
        string pre_question = string.Empty;
        int counting = 0;

        public MainWindow()
        {
            InitializeComponent();

            new respond(reply, ignore) { };

            //creating an instance for the class voice_greeting 
            //with an object name greet
            voice_greeting greet = new voice_greeting();

            //call the voice method
            greet.greet();

            //Method to start background animation
            AnimateBackground();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            ColorAnimation glow = new ColorAnimation();
            glow.From = Colors.DeepSkyBlue;
            glow.To = Colors.Cyan;
            glow.Duration = TimeSpan.FromSeconds(1.5);
            glow.AutoReverse = true;
            glow.RepeatBehavior = RepeatBehavior.Forever;

            SolidColorBrush brush = new SolidColorBrush(Colors.DeepSkyBlue);
            cyberbot_title.Foreground = brush;

            brush.BeginAnimation(SolidColorBrush.ColorProperty, glow);
        }
        //proceed  event handler
        private void proceed(object sender, RoutedEventArgs e)
        {
            //Hide home page grid and set Username grid visible
            MainGrid.Visibility = Visibility.Hidden;
            username_grid.Visibility = Visibility.Visible;
        }

        //submit name  event handler
        private void submit_name(object sender, RoutedEventArgs e)

        {
            username = check_name.submit_name(usernames_input, chats);

            if (!string.IsNullOrWhiteSpace(username))
            {
                username_grid.Visibility = Visibility.Hidden;
                chat_grid.Visibility = Visibility.Visible;
            }
        }


        //send event handler
        private void send(object sender, RoutedEventArgs e)
        {
            // Get the question from the design and sanitize it
            string rawQuestion = question.Text.ToString().Trim();

            if (string.IsNullOrWhiteSpace(rawQuestion))
            {
                error_method("ChatBot", "Please enter a question.");
                return;
            }

            // Remove special characters and clean the question
            string questions = RemoveSpecialCharacters(rawQuestion);

            // Show what the user typed 
            error_method(username, rawQuestion);

            //save chat history
            File.AppendAllText(
          "chat_history.txt",
          username + ": " + rawQuestion + Environment.NewLine
      );
            if (questions.ToLower().StartsWith("add task"))
            {
                AddTaskFromChat(questions);
                return;
            }

            if (waitingForReminder)
            {
                HandleReminder(questions);
                return;
            }
            //ai chats and auto_show_interest
            auto_show_interest();
            ai_check(questions);
        }

        //end for the username submit

        //start of ai_check method
        private void ai_check(string questions)
        {


            // Check if user entered anything meaningful
            if (string.IsNullOrWhiteSpace(questions))
            {
                error_method("ChatBot", "Please enter a valid question.");
                question.Clear();
                return;
            }

            // Check if the question contains only special characters or empty after cleaning
            if (questions.Length == 0 || string.IsNullOrWhiteSpace(questions))
            {
                error_method("ChatBot", "I couldn't understand that.");
                question.Clear();
                return;
            }

            // Variables for processing
            string[] words = questions.ToLower().Split(new char[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
            bool found = false;
            string message = string.Empty;
            Random indexer = new Random();
            List<string> per_word = new List<string>();
            List<string> answers_found = new List<string>();

            // Process each word
            foreach (string word in words)
            {
                // Skip very short words or ignored words
                if (word.Length < 3 || ignore.Contains(word.ToLower()))
                    continue;

                per_word.Clear();

                //start of interests

                if (word.Contains("interested"))
                {
                    string store_interests = string.Empty;
                    bool found_interest = false;

                    HashSet<string> currentInterests = new HashSet<string>();

                    foreach (string interest in words)
                    {
                        // CLEAN INPUT
                        string clean = interest.ToLower().Trim();
                        clean = Regex.Replace(clean, @"[^a-zA-Z0-9\s]", "");

                        // FILTER NOISE WORDS
                        if (!ignore.Contains(clean) && clean != "interested" && clean != "and" && clean != "in" && clean.Length >= 3)
                        {
                            found_interest = true;
                            currentInterests.Add(clean);
                        }
                    }


                    // prepare interests
                    store_interests = string.Join(", ", currentInterests);

                    if (found_interest && !string.IsNullOrWhiteSpace(store_interests))
                    {
                        string filename = "interested_topic.txt";
                        bool userFound = false;

                        if (File.Exists(filename))
                        {
                            string[] lines = File.ReadAllLines(filename);

                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].StartsWith(username))
                                {
                                    userFound = true;

                                    //get all the interests
                                    string existing = lines[i].Replace(username + " interested in:", "").ToLower();

                                    HashSet<string> existingSet = new HashSet<string>(existing.Split(',').Select(x => x.Trim()).Where(x => x != ""));

                                    // remove dumplicates
                                    foreach (string item in currentInterests)
                                    {
                                        existingSet.Add(item);
                                    }

                                    string finalList = string.Join(", ", existingSet);

                                    lines[i] = username + " interested in: " + finalList;
                                    File.WriteAllLines(filename, lines);

                                    message += "great, i added " + store_interests + " to your interests and ";
                                    break;
                                }
                            }
                        }

                        if (!userFound)
                        {
                            File.AppendAllText(
                                filename,
                                username + " interested in: " + store_interests + "\n"
                            );

                            message += "great, i will remember that you are interested in " + store_interests + " and ";
                        }
                    }
                    else
                    {
                        message += "Please specify what you're interested in (e.g., 'I am interested in cybersecurity')";
                    }
                }

                //end of interests

                // Search for matching answers
                bool wordFound = false;
                foreach (string answer in reply)
                {
                    if (answer.ToLower().Contains(word))
                    {
                        wordFound = true;
                        per_word.Add(answer);
                    }
                }

                if (wordFound && per_word.Count > 0)
                {
                    found = true;
                    int indexing = indexer.Next(0, per_word.Count);
                    answers_found.Add(per_word[indexing]);
                }
            }

            // Show responses or error message
            if (found && answers_found.Count > 0)
            {
                // Remove duplicate answers
                answers_found = answers_found.Distinct().ToList();

                foreach (string per_answer in answers_found)
                {
                    message += per_answer + "\n";
                }

                error_method("ChatBot", message.TrimEnd('\n'));


                chats.ScrollIntoView(chats.Items[chats.Items.Count - 1]);
            }
            else
            {
                // when nothing is found
                string[] fallbackMessages = {
            "I'm sorry, I don't understand that. Could you rephrase your question?",
            "I didn't quite get that. Try asking about cyber security topics.",
            "Hmm, I'm not sure how to respond to that. Can you ask something else?",
            "I couldn't find an answer for that. Please ask about programming, security, or technology.",
            "My apologies, I don't have information on that topic yet."
        };

                Random random = new Random();
                string fallbackMessage = fallbackMessages[random.Next(fallbackMessages.Length)];
                error_method("ChatBot", fallbackMessage);
            }

            // Clear the input box
            question.Clear();


        }

        //end of ai_chat method


        //method to remove special characters
        private string RemoveSpecialCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            StringBuilder sanitized = new StringBuilder();

            foreach (char c in input)
            {
                // Keep letters, numbers, spaces, and basic punctuation
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-')
                {
                    sanitized.Append(c);
                }
                else
                {
                    // Replace other special characters with space
                    sanitized.Append(' ');
                }

            }


            // Clean up extra spaces and trim
            string result = sanitized.ToString();
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }


        //end of method to remove special characters

        private void Timer_Tick(object sender, EventArgs e)
        {
            clock_text.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        //method count to show interests randomly
        private void auto_show_interest()
        {
            //check if three times
            if (counting == 3)
            {
                //read the user's interests from file
                string filename = "interested_topic.txt";

                if (File.Exists(filename))
                {
                    string[] lines = File.ReadAllLines(filename);

                    //find the user's line
                    foreach (string line in lines)
                    {
                        if (line.StartsWith(username))
                        {
                            //get the interests part
                            int colonIndex = line.IndexOf("interested in:");
                            if (colonIndex >= 0)
                            {
                                string interests = line.Substring(colonIndex + 14).Trim();

                                //show reminder of interests
                                error_method("ChatBot", "Just a reminder, you are interested in " + interests + " and ");
                                ai_check(interests);
                                break;
                            }
                        }
                    }
                }

                //reset counting
                counting = 0;
            }
            else
            {
                //incrementing
                counting += 1;
            }
        }
        //end of count interest method

        // Updated error method with better formatting
        private void error_method(string name, string message)
        {
            // Create a border for chats
            Border messageBorder = new Border
            {
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(5, 3, 5, 3),
                CornerRadius = new CornerRadius(5)
            };

            // Set different background for user vs bot
            if (name.ToLower().Contains("chatbot") || name.ToLower().Contains("chat"))
            {// Light blue
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
                messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            }
            else
            {    // Light gray
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(211, 211, 211));
            }
            messageBorder.BorderThickness = new Thickness(1);

            TextBlock messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2)
            };

            // Set color based on sender
            Brush nameColor = (name.ToLower().Contains("chatbot") || name.ToLower().Contains("chat")) ?
                              Brushes.PaleGreen : Brushes.PaleTurquoise;

            Brush messageColor = Brushes.Black;

            messageText.Inlines.Add(new Run
            {
                Text = name + ": ",
                Foreground = nameColor,
                FontWeight = FontWeights.Bold
            });

            messageText.Inlines.Add(new Run
            {
                Text = message,
                Foreground = messageColor
            });

            messageBorder.Child = messageText;
            chats.Items.Add(messageBorder);

            chats.ScrollIntoView(chats.Items[chats.Items.Count - 1]);
        }//end of error method

        private void AnimateBackground()
        {
            //Create a new colour animation object
            ColorAnimation colorAnimation = new ColorAnimation();

            //starting color of the animation
            colorAnimation.From = Colors.AliceBlue;

            //ending color of the animation
            colorAnimation.To = Colors.Magenta;

            //setting the animation duration
            colorAnimation.Duration = TimeSpan.FromSeconds(4);

            //reserve back to the original color
            colorAnimation.AutoReverse = true;

            //repeat the animation forever
            colorAnimation.RepeatBehavior = RepeatBehavior.Forever;

            //create a solidBrushColor with the starting colour
            SolidColorBrush brush = new SolidColorBrush(Colors.AliceBlue);

            //apply the brush to the grid background
            MainGrid.Background = brush;

            //start the colour animation on the brush
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void show_history(object sender, RoutedEventArgs e)
        {
            //check if file exists
            if (File.Exists("chat_history.txt"))
            {
                //read all history
                string history = File.ReadAllText("chat_history.txt");

                //display history
                error_method("History", history);
            }
            else
            {
                error_method("ChatBot", "No search history found.");
            }
        }

        private void AddTaskFromChat(string input)
        {
            string title =
                input.Replace("add task", "").Trim();

            if (title == "")
            {
                error_method("ChatBot",
                    "Please enter a task title.");
                return;
            }

            pendingTaskTitle = title;

            pendingTaskDescription =
                "Review " + title +
                " to improve cybersecurity.";

            waitingForReminder = true;

            error_method("ChatBot",
                "Task added with description \"" +
                pendingTaskDescription +
                "\". Would you like a reminder?");
        }
        private void HandleReminder(string input)
        {
            int days = 0;

            foreach (string word in input.Split(' '))
            {
                if (int.TryParse(word, out days))
                {
                    break;
                }
            }

            DateTime reminder =
                DateTime.Now.AddDays(days);

            taskManager.AddTask(
                pendingTaskTitle,
                pendingTaskDescription,
                reminder);

            error_method(
                "ChatBot",
                "Got it! I'll remind you in "
                + days +
                " days.");

            waitingForReminder = false;

            pendingTaskTitle = "";
            pendingTaskDescription = "";

            LoadTasks();
        }

        private void LoadTasks()
        {
            taskList.Items.Clear();

            foreach (var task in taskManager.GetTasks())
            {
                taskList.Items.Add(
                    task.Title +
                    " | " +
                    task.Description +
                    " | " +
                    (task.Completed ?
                    "Completed" :
                    "Pending"));
            }
        }
        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }
        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (taskList.SelectedIndex >= 0)
            {
                taskManager.CompleteTask(
                    taskList.SelectedIndex);

                LoadTasks();
            }
        }
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (taskList.SelectedIndex >= 0)
            {
                taskManager.DeleteTask(
                    taskList.SelectedIndex);

                LoadTasks();
            }
        }
        private string DetectIntent(string input)
        {
            input = input.ToLower();

            // TASKS
            if (input.Contains("add task") ||
                input.Contains("create task") ||
                input.Contains("new task"))
            {
                return "add_task";
            }

            // REMINDERS
            if (input.Contains("remind me") ||
                input.Contains("set reminder") ||
                input.Contains("remember to"))
            {
                return "reminder";
            }

            // QUIZ
            if (input.Contains("quiz") ||
                input.Contains("game") ||
                input.Contains("test me"))
            {
                return "quiz";
            }

            // ACTIVITY LOG
            if (input.Contains("what have you done") ||
                input.Contains("activity log") ||
                input.Contains("recent actions"))
            {
                return "log";
            }

            return "normal";
        }
    }//end of class
}//end of namespace

