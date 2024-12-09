using Course.Classes;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Course
{
    public partial class MainWindow : Window
    {
        //Main part - creating notes list, accounts list, settings variable etc
        List<Note> notes = new();
        List<Account> account = new();
        SettingsA settings = new();

        string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotesByBroniev", "data.xml");
        string path_acc = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotesByBroniev", "accounts.xml");
        XmlSerializer formatter = new(typeof(List<Note>)), formatter_acc = new(typeof(List<Account>));
        GoogleApi gc = new();
        private int p = -1, currentUserID = 0;
        private static Mutex appMutex = new(true, "NotesByBroniev", out createdNew);
        private static bool createdNew;
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool ShouldSystemUseDarkMode();

        public MainWindow()
        {
            if (!createdNew)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("The application is already running.", "Warning", MessageBoxButton.OK);
                Close();
                return;
            }

            notes = new List<Note>();
            account = new List<Account>();

            if (File.Exists(path_acc))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(path_acc, FileMode.Open))
                    {
                        account = (List<Account>)formatter_acc.Deserialize(fileStream);
                    }

                    if (account is null || account.Count == 0) return;
                    for (int i = 0; i < account.Count; i++)
                    {
                        if (account[i].IsUsedLast == true)
                        {
                            currentUserID = i;
                            settings = account[i].SettingsOfAccount;
                            break;
                        }
                    }
                }
                catch (Exception) {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Maybe file with accounts is damaged?\nI creating new basic account", "Error", MessageBoxButton.OK);
                    BasicAccountCreating();
                }
            }
            else if (account is null || account.Count == 0)
            {
                BasicAccountCreating();
            }

            if (File.Exists(path))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        notes = (List<Note>)formatter.Deserialize(fileStream);
                    }
                }
                catch (Exception) { Xceed.Wpf.Toolkit.MessageBox.Show("Maybe the file with notes is damaged?", "Error"); }
            }

            InitializeComponent();
            Settings_ThemeSw.IsChecked = settings.Theme;
            ChangeView.IsChecked = settings.View;

            this.Resources["CurrentTextColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#F2FAF5" : "#272643"));
            this.Resources["CurrentMainColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#082831" : "#FFFFFF"));
            this.Resources["CurrentNotesColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#072B24" : "#E3F6F5"));
            this.Resources["CurrentAccentColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#66A182" : "#BAE8E8"));

            UpdateComboBox();
            MakeAllNotesVisible();
            ChangeViewF();
            UpdateNotes(true);
            ColorChanged();

            AccountBorder.Visibility = Visibility.Visible;
            Accounts_cmb.SelectedIndex = currentUserID;
            AboutBorder.Visibility = Visibility.Hidden;
            NoteWindow.Visibility = Visibility.Visible;
        }
        private void BasicAccountCreating()
        {
            account.Add(new AccountBuilder().WithName(Environment.UserName).Build());
            account[currentUserID].SettingsOfAccount.Theme = ShouldSystemUseDarkMode();
            settings = account[currentUserID].SettingsOfAccount;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();
            ToastNotifications.PopToast("Bye bye, " + account[currentUserID].Name + "!", "Have a nice day!");
            Application.Current.Shutdown();
        }
        private async void SaveData()
        {
            if (currentUserID < 0 || currentUserID >= account.Count) return;
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotesByBroniev"))) Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NotesByBroniev"));

            foreach (Account a in account)
            {
                a.IsUsedLast = false;
            }

            account[currentUserID].IsUsedLast = true;

            if (!string.IsNullOrEmpty(account[currentUserID].Email))
            {
                account[currentUserID].Email = string.Empty;
                await gc.LeaveAccountAsync();
            }

            account[currentUserID].SettingsOfAccount = settings;
            try
            {
                using (FileStream fileStream = new(path, FileMode.Create))
                {
                    formatter.Serialize(fileStream, notes);
                }

                using (FileStream fileStream = new(path_acc, FileMode.Create))
                {
                    formatter_acc.Serialize(fileStream, account);
                }
            }
            catch (Exception) { }
        }
        private void MWindow_Closed(object sender, EventArgs e)
        {
            if (createdNew)
            {
                appMutex.ReleaseMutex();
            }

            Thread.Sleep(6000);
            ToastNotificationManagerCompat.History.Clear();
            Close();
        }
        //^
        // UI System (add, remove etc)

        ///Themes
        private void Settings_ThemeSw_Click(object sender, RoutedEventArgs e)
        {

            if (Settings_ThemeSw.IsChecked == true) settings.Theme = true;
            else settings.Theme = false;

            this.Resources["CurrentTextColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#F2FAF5" : "#272643"));
            this.Resources["CurrentMainColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#082831" : "#FFFFFF"));
            this.Resources["CurrentNotesColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#072B24" : "#E3F6F5"));
            this.Resources["CurrentAccentColor"] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.Theme ? "#66A182" : "#BAE8E8"));

            ColorChanged();
        }
        public void ColorChanged()
        {
            MainBorder.Background = (SolidColorBrush)this.Resources["CurrentMainColor"];
            EditCanvas.Background = (SolidColorBrush)this.Resources["CurrentMainColor"];
            SettingsStackPanel.Background = (SolidColorBrush)this.Resources["CurrentMainColor"];
            AccountStackPanel.Background = (SolidColorBrush)this.Resources["CurrentMainColor"];
            NoteCanvas.Background = (SolidColorBrush)this.Resources["CurrentMainColor"];
        }
        ///^
        private void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (p == -1) return;
            notes[p].ArchiveNote();
            MakeAllNotesVisible();
            UpdateNotes(true);
            UpdateItemList(true);
            ButtonClose_Click(sender, e);
        }
        private void Name_Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (p == -1) return;
            try
            {
                notes[p].Name = name_Notes.Text;
                UpdateNotes(true);
                UpdateItemList(true);
            }
            catch (Exception) { }
        }
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            UpdateNotes(true);
            UpdateItemList(true);
            EditWindow.Visibility = Visibility.Hidden;
            NoteWindow.Visibility = Visibility.Visible;
        }
        private void ComboSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)e.AddedItems[0];
            string selectedOption = (string)selectedItem.Tag;
            SortList(selectedOption);
        }
        private void SortList(string sortOption)
        {
            switch (sortOption)
            {
                case "Name A-Z":
                    SortNotes(notes, "Name", true);
                    break;
                case "Name Z-A":
                    SortNotes(notes, "Name", false);
                    break;
                case "Date (Oldest first)":
                    SortNotes(notes, "Date", true);
                    break;
                case "Date (Newest first)":
                    SortNotes(notes, "Date", false);
                    break;
                case "Author A-Z":
                    SortNotes(notes, "Author", true);
                    break;
                case "Author Z-A":
                    SortNotes(notes, "Text", false);
                    break;
                case "Text A-Z":
                    SortNotes(notes, "Text", true);
                    break;
                case "Text Z-A":
                    SortNotes(notes, "Date", false);
                    break;
                default:
                    break;
            }
            UpdateNotes(true);
            UpdateItemList(true);
            NoteList.Items.Refresh();
        }
        public void OpenEditWindow(int ind)
        {
            if (ind == -1) return;
            AccountWindow.Visibility = Visibility.Hidden;
            NoteWindow.Visibility = Visibility.Hidden;
            SettingsWindow.Visibility = Visibility.Hidden;
            EditWindow.Visibility = Visibility.Visible;
            if (notes[ind].IsPrivate == true)
            {
                PrivateCheck.IsChecked = true;
            }
            else
            {
                PrivateCheck.IsChecked = false;
            }

            if (notes[ind].IsEventCr == true)
            {
                MakeEvent.IsChecked = true;
            }
            else MakeEvent.IsChecked = false;

            if (notes[ind].IsEventCr && notes[ind].timeOfNot.HasValue)
            {
                DatePickEv.Minimum = notes[ind].timeOfNot.Value.AddSeconds(-10);
                DatePickEv.Value = notes[ind].timeOfNot;
            }
            else
            {
                DatePickEv.Minimum = DateTime.Today;
                DatePickEv.Value = DateTime.Now.AddMinutes(20);
            }
            name_Notes.Text = notes[ind].Name;
            text_Notes.Text = notes[ind].RTF;
            text_Notes.Foreground = (SolidColorBrush)Resources["CurrentTextColor"];
            string tagsString = string.Join(", ", notes[ind].Tags);
            txtTags.Text = tagsString;
            time_Notes.Content = ("Created: " + notes[ind].time.ToString());
            p = ind;
        }
        public int SearchNotesIndex(int indexInPanel, bool searchArchived)
        {
            int indexInNotes = -1;
            int adjustedIndex = 0;

            for (int currentIndex = 0; currentIndex < notes.Count; currentIndex++)
            {
                Note n = notes[currentIndex];

                if (n.IsArchived != searchArchived
                                  || n.IsHidden
                                  || (!n.AccountN.Equals(account[currentUserID]) && n.IsPrivate))
                {
                    continue;
                }

                if (adjustedIndex == indexInPanel)
                {
                    indexInNotes = currentIndex;
                    break;
                }
                adjustedIndex++;
            }
            return indexInNotes;
        }

        private void PrivateCheck_Click(object sender, RoutedEventArgs e)
        {
            if (p == -1)
            {
                PrivateCheck.IsChecked = false;
                return;
            }
            if (account[currentUserID].Name != notes[p].AccountN.Name)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("You can't make it private because it wasn't created by you.", "Error");
                PrivateCheck.IsChecked = false;
                return;
            }

            if (PrivateCheck.IsChecked != true)
            {
                notes[p].MakeNotePublic();
            }
            else notes[p].MakeNotePrivate();
        }
        public void WelcomeTextChange()
        {
            string content = "Hi!";
            DateTime d = DateTime.Now;
            string userName = account[currentUserID].Name;

            if (d.Hour < 12)
            {
                content = $"Good morning, {userName}! Have a nice work!";
            }
            else if (d.Hour < 16)
            {
                content = $"Good afternoon, {userName}! Good luck on your business!";
            }
            else
            {
                content = $"Good evening, {userName}! Have a nice work!";
            }

            Label_NameOfNotes.Content = content;
        }
        private void UpdateQuote()
        {
            Random rnd = new();
            string[] quotes = {
                "\"Don't watch the clock; do what it does. Keep going.\" - Sam Levenson",
                "\"Believe and act as if it were impossible to fail.\" - Charles Kettering",
                "\"You don't have to be great to start, but you have to start to be great.\" - Zig Ziglar",
                "\"The only person you are destined to become is the person you decide to be.\" - Ralph Waldo Emerson",
                "\"The best way to predict the future is to create it.\" - Peter Drucker",
                "\"It's not about ideas. It's about making ideas happen.\" - Scott Belsky",
                "\"Value what you know and start charging for it.\" - Kim Garst",
                "\"The future belongs to those who believe in the beauty of their dreams.\" - Eleanor Roosevelt",
                "\"It's not what you look at that matters, it's what you see.\" - Henry David Thoreau",
                "\"Believe in yourself and all that you are.\" - Christian D. Larson",
                "\"You miss 100% of the shots you don't take.\" - Wayne Gretzky",
                "\"Be the change you wish to see in the world.\" - Mahatma Gandhi",
                "\"Don't count the days, make the days count.\" - Muhammad Ali",
                "\"The best way to predict the future is to create it.\" - Abraham Lincoln",
                "\"If you want to go fast, go alone. If you want to go far, go together.\" - African Proverb",
                "\"Be yourself; everyone else is already taken.\" - Oscar Wilde",
                "\"The only thing we have to fear is fear itself.\" - Franklin D. Roosevelt",
                "\"It always seems impossible until it's done.\" - Nelson Mandela",
                "\"The greatest glory in living lies not in never falling, but in rising every time we fall.\" - Nelson Mandela",
                "\"Happiness is not something ready made. It comes from your own actions.\" - Dalai Lama",
                "\"Act as if what you do makes a difference.It does.\" - William James",
                "\"Strive not to be a success, but rather to be of value.\" - Albert Einstein",
                "\"Believe you can and you're halfway there.\" - Theodore Roosevelt",
                "\"Happiness is not by chance, but by choice.\" - Jim Rohn",
                "\"Don't wait. The time will never be just right.\" - Napoleon Hill",
                "\"You have to learn the rules of the game.And then you have to play better than anyone else.\" - Albert Einstein",
                "\"The best revenge is massive success.\" - Frank Sinatra",
                "\"The only way to do great work is to love what you do.\" - Steve Jobs",
                "\"I have not failed. I've just found 10,000 ways that won't work.\" - Thomas Edison",
                "\"Success is not the key to happiness. Happiness is the key to success.\" - Albert Schweitzer",
                "\"The way to get started is to quit talking and begin doing.\" - Walt Disney",
                "\"You are never too old to set another goal or to dream a new dream.\" - C.S. Lewis",
                "\"Don't let yesterday take up too much of today.\" - Will Rogers",
                "\"Your time is limited, don't waste it living someone else's life.\" - Steve Jobs",
                "\"Success is not final, failure is not fatal: it is the courage to continue that counts.\" - Winston Churchill",
                "\"The only limit to our realization of tomorrow will be our doubts of today.\" - Franklin D. Roosevelt",
                "\"The only true wisdom is in knowing you know nothing.\" - Socrates",
                "\"If you can dream it, you can do it.\" - Walt Disney",
                "\"Success is walking from failure to failure with no loss of enthusiasm.\" - Winston Churchill",
                "\"Your life does not get better by chance, it gets better by change.\" - Jim Rohn",
                "\"The harder you work for something, the greater you'll feel when you achieve it.\" - Unknown",
                "\"Life is 10% what happens to us and 90% how we react to it.\" - Charles R. Swindoll",
                "\"In the middle of every difficulty lies opportunity.\" - Albert Einstein",
                "\"The only way to achieve the impossible is to believe it is possible.\" - Charles Kingsleigh",
                "\"Do what you can, with what you have, where you are.\" - Theodore Roosevelt",
                "\"If opportunity doesn't knock, build a door.\" - Milton Berle",
                "\"The secret of getting ahead is getting started.\" - Mark Twain",
                "\"It does not matter how slowly you go as long as you do not stop.\" - Confucius",
                "\"What lies behind us and what lies before us are tiny matters compared to what lies within us.\" - Ralph Waldo Emerson",
                "\"The only way to do great work is to love what you do.\" - Steve Jobs",
                "\"The secret of change is to focus all your energy not on fighting the old, but on building the new.\" - Socrates",
                "\"Success is not about how much money you make, it's about the difference you make in people's lives.\" - Michelle Obama",
                "\"The biggest risk is not taking any risk. In a world that's changing quickly, the only strategy that is guaranteed to fail is not taking risks.\" - Mark Zuckerberg",
                "\"Difficult roads often lead to beautiful destinations.\" - Zig Ziglar",
                "\"The only limit to our realization of tomorrow will be our doubts of today.\" - Franklin D. Roosevelt",
                "\"To be yourself in a world that is constantly trying to make you something else is the greatest accomplishment.\" - Ralph Waldo Emerson",
                "\"If you're going through hell, keep going.\" - Winston Churchill",
                "\"Success is not final; failure is not fatal: It is the courage to continue that counts.\" - Winston S. Churchill",
                "\"In the end, it's not the years in your life that count. It's the life in your years.\" - Abraham Lincoln",
                "\"The best way to find yourself is to lose yourself in the service of others.\" - Mahatma Gandhi",
                "\"The only thing standing between you and your goal is the story you keep telling yourself as to why you can't achieve it.\" - Jordan Belfort",
                "\"You can't cross the sea merely by standing and staring at the water.\" - Rabindranath Tagore",
                "\"The best time to plant a tree was 20 years ago. The second best time is now.\" - Chinese Proverb",
                "\"The biggest adventure you can take is to live the life of your dreams.\" - Oprah Winfrey",
                "\"The journey of a thousand miles begins with one step.\" - Lao Tzu",
                "\"I can't change the direction of the wind, but I can adjust my sails to always reach my destination.\" - Jimmy Dean",
                "\"When everything seems to be going against you, remember that the airplane takes off against the wind, not with it.\" - Henry Ford"
                };
            int quoteNum = rnd.Next(0, quotes.Length);
            string quote = quotes[quoteNum];
            Label_Quote.Content = quote;
        }
        private void Text_Note_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (p == -1) return;
                notes[p].RTF = text_Notes.Text;
                UpdateNotes(false);
                UpdateItemList(false);
            }
            catch (Exception) { }
        }
        public void UpdateNotes(bool mode)
        {
            Notes.Children.Clear();
            foreach (Note n in notes)
            {
                if (n.IsHidden && mode) continue;
                if (n.IsArchived == !mode)
                {
                    if ((account[currentUserID].Equals(n.AccountN) && n.IsPrivate) || !n.IsPrivate)
                    {
                        TextBox t = new()
                        {
                            Style = (Style)FindResource("NoteBox"),
                            Tag = n.Name,
                            Text = n.PlainText + "\nCreated: " + n.time + "\n",
                            TextWrapping = TextWrapping.Wrap,
                            BorderBrush = Brushes.Transparent,
                            BorderThickness = new Thickness(3),
                            IsReadOnly = true,
                            ToolTip = "Click to open/restore note"
                        };

                        Notes.Children.Add(t);
                    }
                }
            }
        }
        private void UpdateItemList(bool mode)
        {

            NoteList.Items.Clear();
            foreach (Note n in notes)
            {
                if (n.IsHidden && mode) continue;
                if (n.IsArchived == !mode)
                {
                    if ((account.Count > currentUserID && account[currentUserID].Equals(n.AccountN) && n.IsPrivate) || !n.IsPrivate)
                    {
                        NoteList.Items.Add(n.Name + "\n" + n.PlainText + "\n" + n.time + " | " + n.AccountN.Name + "\n>-------------------------------");
                    }
                }
            }
        }
        private void NoteList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ChangeView.IsChecked == true)
            {
                int index = NoteList.SelectedIndex;
                if (index == -1) return;

                bool isArchived = ArchiveNotesMenuItem.IsChecked == true;
                int noteIndex = SearchNotesIndex(index, isArchived);

                if (noteIndex != -1)
                {
                    HandleNoteAction(noteIndex, isArchived);
                }
            }
        }
        private void Notes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var myTextBox = e.Source as TextBox;
                if (myTextBox == null) return;

                int index = Notes.Children.IndexOf(myTextBox);
                if (index == -1) return;

                bool isArchived = ArchiveNotesMenuItem.IsChecked == true;
                int noteIndex = SearchNotesIndex(index, isArchived);

                if (noteIndex != -1)
                {
                   HandleNoteAction(noteIndex, isArchived);
                }
            }
        }
        private void HandleNoteAction(int noteIndex, bool isArchived)
        {
            if (!isArchived)
            {
                p = noteIndex;
                OpenEditWindow(p);
            }
            else
            {
                p = noteIndex;
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Do you want delete this note?\n(No - save this note)", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) notes.RemoveAt(noteIndex);
                else if (result == MessageBoxResult.No) notes[noteIndex].IsArchived = false;
                UpdateNotes(false);
                UpdateItemList(false);
            }
        }
        public List<Note> SortNotes(List<Note> notes, string sortBy, bool ascending = true)
        {
            NotesSorting notesSorting = new(notes);
            switch (sortBy)
            {
                case "Author":
                    return notesSorting.SortNotesByAuthor(ascending);
                case "Date":
                    return notesSorting.SortNotesByDate(ascending);
                case "Name":
                    return notesSorting.SortNotesByName(ascending);
                case "Text":
                    return notesSorting.SortNotesByText(ascending);
                default:
                    return notes;
            }
        }
        private void txtTags_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string tagsText = txtTags.Text;
                char[] separators = { ',', ';' };
                string[] tagsToAdd = tagsText.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                List<string> existingTags = notes[p].Tags;

                existingTags.Clear();

                foreach (string tag in tagsToAdd)
                {
                    string trimmedTag = tag.Trim();
                    existingTags.Add(trimmedTag);
                }

                txtTags.Text = string.Join(", ", existingTags);
            }
        }
        private void name_NotesSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            HomeMenuItem.IsChecked = true;
            HomeMenuItem_Checked(sender, e);
            if (string.IsNullOrWhiteSpace(name_NotesSearch.Text))
            {
                MakeAllNotesVisible();
                UpdateItemList(true);
                UpdateNotes(true);
            }
            else
            {
                NoteList.Items.Clear();
                foreach (Note n in notes)
                {
                    if (string.IsNullOrEmpty(n.PlainText)) continue;
                    string searchedtxt = name_NotesSearch.Text.Trim().ToLower(), plainText = n.PlainText.Trim().ToLower();

                    if (n.Name.ToLower().Trim().StartsWith(searchedtxt, StringComparison.OrdinalIgnoreCase) || (plainText != null && plainText.StartsWith(searchedtxt, StringComparison.OrdinalIgnoreCase)) || (plainText != null && plainText.ToLower().Contains(searchedtxt)) || n.Tags.Exists(tag => tag.ToLower().Trim().Equals(searchedtxt, StringComparison.OrdinalIgnoreCase) || tag.ToLower().Trim().StartsWith(searchedtxt, StringComparison.OrdinalIgnoreCase)))
                    {
                        if ((account[currentUserID].Equals(n.AccountN) && n.IsPrivate == true) || n.IsPrivate == false)
                        {
                            NoteList.Items.Add(n.Name + "\n" + n.PlainText + "\n" + n.time + " | " + n.AccountN.Name + "\n>-------------------------------");
                            n.IsHidden = false;
                            UpdateNotes(true);
                        }
                    }
                    else { n.IsHidden = true; }
                }
            }
        }
        private void MakeAllNotesVisible()
        {
            foreach (Note note in notes)
            {
                note.IsHidden = false;
            }
        }
        private void AllTagsDel_Click(object sender, RoutedEventArgs e)
        {
            var confirmResult = Xceed.Wpf.Toolkit.MessageBox.Show($"The ({notes[p].Tags.Count}) tags will removed. Are you sure?", "Confirmation", MessageBoxButton.YesNo);
            if (confirmResult == MessageBoxResult.Yes)
            {
                notes[p].Tags.Clear();
            }
            string tagsString = string.Join(", ", notes[p].Tags);
            txtTags.Text = tagsString;
        }
        private void EditWindow__IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditWindow.Visibility == Visibility.Visible && p != -1)
            {
                UpdateNotes(true);
                UpdateItemList(true);
            }
            else { }
        }

        //MENU SYSTEM
        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            About_Btt_Click(sender, e);
        }
        private void HomeMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            WelcomeTextChange();
            EditWindow.Visibility = Visibility.Hidden;
            SettingsWindow.Visibility = Visibility.Hidden;
            AccountWindow.Visibility = Visibility.Hidden;
            ComboSorting.Visibility = Visibility.Visible;
            ButtonAddNote.Visibility = Visibility.Visible;
            NoteWindow.Visibility = Visibility.Visible;
            Label_Quote.Visibility = Visibility.Visible;
            UpdateNotes(true);
            UpdateItemList(true);
        }
        private void AccountMenuItem_Checked(object sender, RoutedEventArgs e)
        {
                NoteWindow.Visibility = Visibility.Hidden;
                AccountWindow.Visibility = Visibility.Visible;
                EditWindow.Visibility = Visibility.Hidden;
                SettingsWindow.Visibility = Visibility.Hidden;
                NameAcc_Label.Content = account[currentUserID].Name;
                Label_Email.Content = account[currentUserID].Email;
        }
        private void ArchiveNotesMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            EditWindow.Visibility = Visibility.Hidden;
            SettingsWindow.Visibility = Visibility.Hidden;
            AccountWindow.Visibility = Visibility.Hidden;
            Label_NameOfNotes.Visibility = Visibility.Visible;
            Label_Quote.Visibility = Visibility.Hidden;
            ComboSorting.Visibility = Visibility.Hidden;
            ButtonAddNote.Visibility = Visibility.Hidden;
            NoteWindow.Visibility = Visibility.Visible;
            Label_NameOfNotes.Content = "Archived notes";
            UpdateNotes(false);
            UpdateItemList(false);
        }
        private void SettingsMenuItem_Checked(object sender, RoutedEventArgs e)
        {
                NoteWindow.Visibility = Visibility.Hidden;
                EditWindow.Visibility = Visibility.Hidden;
                AccountWindow.Visibility = Visibility.Hidden;
                SettingsWindow.Visibility = Visibility.Visible;
        }
        private void EditingPosition_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void ButtonClosing_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ButtonAddNote_Click(object sender, RoutedEventArgs e)
        {
            p = notes.Count;
            text_Notes.Text = "Highlight that text and enter new note's text";
            notes.Add(new NoteBuilder()
                            .WithName(account[currentUserID].Name + "'s new note")
                            .WithRTF(text_Notes.Text)
                            .WithTime(DateTime.Now)
                            .WithAccount(account[currentUserID])
                            .Build());

            OpenEditWindow(notes.Count - 1);
            UpdateNotes(true);
            UpdateItemList(true);
        }
        private void ChangeView_Click(object sender, RoutedEventArgs e)
        {
            ChangeViewF();
        }
        private void ChangeViewF()
        {
            if (ChangeView.IsChecked == true)
            {
                ScrV.Visibility = Visibility.Hidden;
                NoteList.Visibility = Visibility.Visible;
                settings.View = true;
                UpdateItemList(true);
            }
            else
            {
                ScrV.Visibility = Visibility.Visible;
                NoteList.Visibility = Visibility.Hidden;
                settings.View = false;
                UpdateNotes(true);
            }
        }
        //^

        //Calendar & Notifications system
        private async void MakeEvent_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(account[currentUserID].Email))
            {

                    if (MakeEvent.IsChecked == true)
                    {
                        DateTime selectedDate = (DatePickEv.Value != null) ? DatePickEv.Value.Value : DateTime.Now.AddMinutes(10);

                        if (p < notes.Count)
                        {
                            await gc.AddEvent(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), selectedDate, notes[p]);
                            ToastNotificationManagerCompat.OnActivated += ToastNotificationURL;
                            ToastNotifications.PopToast("Added", "Reminder successfully added", "Open event in calendar", notes[p].URL);
                            notes[p].IsEventCr = true;
                            notes[p].timeOfNot = selectedDate;
                        }
                    }
                    else
                    {
                        if (p < notes.Count && notes[p].EvId != null)
                        {
                            await gc.DeleteEvent(notes[p].EvId);
                            notes[p].NoteClearEvent();
                        }
                    }
                    SaveData();

            }
            else
            {
                MakeEvent.IsChecked = false;
                ShowMessageBox("Log in to your Google account and try again.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        private void DatePickEv_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue is DateTime?)) return;

            DateTime? newValue = e.NewValue as DateTime?;
            if (newValue < DatePickEv.Minimum)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show($"Selected date {newValue} is smaller than minimum value {DatePickEv.Minimum}.", "Date Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private void ToastNotificationURL(ToastNotificationActivatedEventArgsCompat args)
        {
            if (p != -1)
            {
                if (!string.IsNullOrEmpty(notes[p].URL))
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {notes[p].URL}"));
                }
            }
        }
        //^

        ///Account window
        private void AccountChangeBtt_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(account[currentUserID].Email))
            {
                account[currentUserID].Email = string.Empty;
                Label_Email.Content = account[currentUserID].Email;
            }
            account[currentUserID].IsUsedLast = false;
            AccountBorder.Visibility = Visibility.Visible;
        }
        private void Account_DeleteBtt_Click(object sender, RoutedEventArgs e)
        {
            if (account.Count > 1 && currentUserID != -1)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this account?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    account.RemoveAt(currentUserID);
                    currentUserID--;
                    UpdateComboBox();
                    Accounts_cmb.SelectedIndex = currentUserID;
                    AccountBorder.Visibility = Visibility.Visible;
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("It's the last account, you can't delete it.");
            }
        }
        private async void Account_Google_EE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AccGoogle_B_TextBlock.Text.ToString() == "Connect Google")
                {

                    await gc.LogToAccount();
                    account[currentUserID].Email = gc.GetCurrentEmail();
                    Label_Email.Content = account[currentUserID].Email;
                    AccGoogle_B_TextBlock.Text = "Unconnect Google";
                    ToastNotifications.PopToast("Google connected", "Successfully\nConnected email: " + account[currentUserID].Email);
                }
                else
                {
                    await gc.LeaveAccountAsync();
                    AccGoogle_B_TextBlock.Text = "Connect Google";
                    account[currentUserID].Email = String.Empty;
                    Label_Email.Content = account[currentUserID].Email;
                    ToastNotifications.PopToast("Google unconnected", "Successfully unconnected Google");

                }
            }
            catch (Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Could not connect to Google.\nPlease check your internet connection and try again.\n" + ex.Message);
            }
        }
       
        //Login system
        private void UpdateComboBox()
        {
            Accounts_cmb.Items.Clear();
            foreach (Account acc in account)
            {
                Accounts_cmb.Items.Add(acc.Name);
            }
            Accounts_cmb.Items.Add("Create new account");
            Accounts_cmb.SelectedIndex = 0;
        }
        private void Accounts_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indx = Accounts_cmb.SelectedIndex;
            bool isCreateNewAccount = indx == Accounts_cmb.Items.Count - 1;

            if (isCreateNewAccount)
            {
                Account_Name_F.Text = string.Empty;
                EnterAccountButton.Content = "Create account";
                Account_Password_F.Visibility = Visibility.Visible;
                Account_Name_F.IsEnabled = true;
            }
            else
            {
                Account_Name_F.Text = account[indx].Name;
                EnterAccountButton.Content = "Enter account";
                Account_Password_F.Visibility = string.IsNullOrEmpty(account[indx].Password)
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Account_Name_F.IsEnabled = false;
            }

            Account_Password_F.Password = string.Empty;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Accounts_cmb.SelectedIndex != Accounts_cmb.Items.Count - 1)
            {
                var selectedAccount = account[Accounts_cmb.SelectedIndex];
                if (Account_Password_F.Visibility == Visibility.Hidden || selectedAccount.VerifyPassword(Account_Password_F.Password))
                {
                    currentUserID = Accounts_cmb.SelectedIndex;
                    settings = selectedAccount.SettingsOfAccount;
                    ApplySettings(sender, e);
                    UpdateUIElements();
                }
                else
                {
                    ShowMessageBox("Wrong password", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Account_Password_F.Password = string.Empty;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Account_Name_F.Text.Trim()) && IsNameFree(Account_Name_F.Text.Trim()))
                {
                    bool mode = !string.IsNullOrWhiteSpace(Account_Password_F.Text);
                    AddNewAccount(mode);
                    UpdateComboBox();
                    SaveData();
                    Accounts_cmb.SelectedIndex = account.Count - 1;
                    Account_Password_F.Password = string.Empty;
                }
                else
                {
                    ShowMessageBox("Enter a valid username (no spaces!) or choose a different nickname.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }
        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            Settings_ThemeSw.IsChecked = settings.Theme;
            ChangeView.IsChecked = settings.View;
            Settings_ThemeSw_Click(sender, e);
            ChangeViewF();
        }
        private void UpdateUIElements()
        {
            WelcomeTextChange();
            UpdateQuote();
            HomeMenuItem.IsChecked = true;
            HomeMenuItem_Checked(null, null);
            ToastNotifications.PopToast($"Welcome back, {account[currentUserID].Name}", "Have a nice work!");

            NameAcc_Label.Content = account[currentUserID].Name;
            AccGoogle_B_TextBlock.Text = string.IsNullOrEmpty(account[currentUserID].Email) ? "Connect Google" : "Unconnect Google";
            ComboSorting.SelectedIndex = 4;
            AccountBorder.Visibility = Visibility.Hidden;
        }
        private void AddNewAccount(bool mode)
        {
            Account newAccount = new AccountBuilder()
                                        .WithName(Account_Name_F.Text.Trim())
                                        .WithSettings(new SettingsA())
                                        .Build();
            if (mode) newAccount.SetPassword(Account_Password_F.Password);
            account.Add(newAccount);
            currentUserID = account.Count;
        }
        private static void ShowMessageBox(string message, MessageBoxButton button, MessageBoxImage image)
        {
            Xceed.Wpf.Toolkit.MessageBox.Show(message, "Error", button, image);
        }
        public bool IsNameFree(string name)
        {
            foreach (var acc in account)
            {
                if (acc.Name == name)
                {
                    return false;
                }
            }
            return true;
        }
        ///^

        // Export to PDF
        private void ExportToPdf()
        {
            MakeAllNotesVisible();
            var dialog = new SaveFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf";
            dialog.FileName = "MyNotes.pdf";
            if (dialog.ShowDialog() == true)
            {
                var writer = new PdfWriter(dialog.FileName);
                var pdfDoc = new PdfDocument(writer);
                var doc = new iText.Layout.Document(pdfDoc);

                PdfFont titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont bodyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont timestampFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

                iText.Kernel.Colors.Color titleColor = new DeviceRgb(34, 34, 34);
                iText.Kernel.Colors.Color timestampColor = new DeviceRgb(128, 128, 128);


                foreach (Note n in notes)
                {
                    if (n.IsArchived) continue;
                    if ((account[currentUserID].Equals(n.AccountN) && n.IsPrivate == true) || n.IsPrivate == false)
                    {
                        iText.Layout.Element.Paragraph header = new iText.Layout.Element.Paragraph(n.Name)
                            .SetFont(titleFont)
                            .SetFontSize(18)
                            .SetMarginBottom(10)
                            .SetFixedLeading(20)
                            .SetFontColor(titleColor);

                        iText.Layout.Element.Paragraph body = new iText.Layout.Element.Paragraph(n.PlainText)
                            .SetFont(bodyFont)
                            .SetFontSize(12)
                            .SetMarginBottom(10)
                            .SetFixedLeading(18);

                        iText.Layout.Element.Paragraph footer = new iText.Layout.Element.Paragraph(n.time.ToString("yyyy-MM-dd HH:mm"))
                            .SetFont(timestampFont)
                            .SetFontSize(10)
                            .SetMarginBottom(20)
                            .SetFontColor(timestampColor);

                        doc.Add(header);
                        doc.Add(body);
                        doc.Add(footer);
                    }
                }
                iText.Layout.Element.Paragraph last = new iText.Layout.Element.Paragraph("\n> Created at the request of user " + account[currentUserID].Name + " at " + DateTime.Now.ToString())
                    .SetFont(timestampFont)
                    .SetFontSize(10)
                    .SetMarginBottom(10);
                doc.Add(last);
                doc.Close();

                ToastNotifications.PopToast("Successfully", "Notes exported to PDF successfully.\nPath to PDF: " + dialog.FileName);
            }
        }
        private void ConvertToPDF_Button_Click(object sender, RoutedEventArgs e)
        {
            if (notes.Count > 0)
            {
                ExportToPdf();
            }
            else Xceed.Wpf.Toolkit.MessageBox.Show("I cannot convert nothing to PDF.\nYou need to create some notes first!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        ///About window system
        private void About_Btt_Click(object sender, RoutedEventArgs e)
        {
            AboutBorder.Visibility = Visibility.Visible;
        }
        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            AboutBorder.Visibility = Visibility.Hidden;
        }
        ///^
    }
}