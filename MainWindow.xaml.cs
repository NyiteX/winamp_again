using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Windows.Ink;

namespace Winamp_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> playlist_list = new List<string>();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();
        uint pause_time = 0;
        string tmpSource = "";
        public MainWindow()
        {
            InitializeComponent();
            Player.MediaEnded += media_MediaEnded;
            
            timer.Tick += new EventHandler(Update);
            timer.Start();
            stereo_label.Foreground = new SolidColorBrush(Color.FromArgb(255,77,253,0));

            timer2.Interval = TimeSpan.FromMilliseconds(250);
            timer2.Tick += new EventHandler(RunningName);
            Main.WindowStyle = WindowStyle.ToolWindow;
            balance_label.Visibility = Visibility.Hidden;
            volume_label.Visibility = Visibility.Hidden;
        }
        //
        //My Funcs
        public void Total_seconds()
        {
            if (List.SelectedIndex != -1)
            {
                try
                {
                    using (var shell = ShellObject.FromParsingName(playlist_list[List.SelectedIndex]))
                    {
                        IShellProperty prop = shell.Properties.System.Media.Duration;
                        var t = (ulong)prop.ValueAsObject;
                        progress_player.Maximum = TimeSpan.FromTicks((long)t).TotalSeconds;
                        songName_label.Content = shell.Name + "   ";
                        kbps_tb.Text = (shell.Properties.System.Audio.EncodingBitrate.Value / 1000).ToString();
                        khz_tb.Text = shell.Properties.System.Audio.SampleSize.Value.ToString();
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        public void Prev_song()
        {
            if (List.Items.Count > 0)
            {
                if (List.SelectedIndex - 1 >= 0)
                {
                    Player.Source = new Uri(playlist_list[List.SelectedIndex - 1]);
                    List.SelectedIndex = List.SelectedIndex - 1;
                }
                else
                {
                    Player.Source = new Uri(playlist_list[List.Items.Count - 1]);
                    List.SelectedIndex = List.Items.Count - 1;
                }
            }
            Total_seconds();
        }
        public void Next_song()
        {
            if (List.Items.Count > 0)
            {
                if (List.SelectedIndex + 1 < List.Items.Count)
                {
                    Player.Source = new Uri(playlist_list[List.SelectedIndex + 1]);
                    List.SelectedIndex = List.SelectedIndex + 1;
                }
                else
                {
                    Player.Source = new Uri(playlist_list[0]);
                    List.SelectedIndex = 0;
                }
            }
            Total_seconds();
        }
        public void Rand_song()
        {
            Random r = new Random();           
            int rand = r.Next(0, List.Items.Count);
            while (rand == List.SelectedIndex)
            {
                rand = r.Next(0, List.Items.Count);
            }    
            if (List.Items.Count > 0)
            {
                Player.Source = new Uri(playlist_list[rand]);
                List.SelectedIndex = rand;
            }
            Total_seconds();
        }
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            Next_song();           
        }
        //
        //Timers
        public void Update(object sender, EventArgs e)
        {
            //time_file_label.Content = Player.Position.ToString("mm") + ':' + Player.Position.ToString("ss");
            time_file_label.Content = Player.Position.ToString(@"hh\:mm\:ss");

            balance_label.Content = balance_menu.Value.ToString();
            volume_label.Content = volume_menu.Value.ToString();
            progress_player.Value = Player.Position.TotalSeconds;    
        }
        public void RunningName(object sender, EventArgs e)
        {
            if (songName_label.Content.ToString().Count() > 0)
            {
                songName_label.Content = songName_label.Content.ToString() + songName_label.Content.ToString()[0];
                songName_label.Content = songName_label.Content.ToString().Remove(0, 1);
            }
        }
        //
        //Buttons            
        private void X_Button_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void play_click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedIndex != -1)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex]);
                Total_seconds();
                timer2.Start();
            }
        }
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (List.SelectedIndex != -1)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex]);
                Total_seconds();
                timer2.Start();
            }
        }
        private void Pause_Button(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Player.Source != null)
                {
                    pause_time = (uint)Player.Position.TotalSeconds;
                    tmpSource = Player.Source.ToString();
                    Player.Source = null;
                }
                else
                {
                    if (tmpSource != "" && pause_time != 0)
                    {
                        Player.Source = new Uri(tmpSource);
                        Player.Position = new TimeSpan(0, 0, 0, (int)pause_time, 0);

                        pause_time = 0;
                        tmpSource = "";
                    }
                }
            }catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void Stop_Button(object sender, RoutedEventArgs e)
        {
            Player.Source = null;
            timer2.Stop();
            pause_time = 0;
            tmpSource = "";
        }
        private void Next_Button(object sender, RoutedEventArgs e)
        {
            if (rand_checkbox.IsChecked == true)
                Rand_song();
            else
                Next_song();
        }
        private void Prev_Button(object sender, RoutedEventArgs e)
        {
            if (rand_checkbox.IsChecked == true)
                Rand_song();
            else
                Prev_song();
        }
        private void add_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "mp3, flac, wave files|*.mp3; *.flac; *.wave";
            openFileDialog1.ShowDialog();

            for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
            {
                playlist_list.Add(openFileDialog1.FileNames[i]);
                List.Items.Add("[" + playlist_list.Count + "]  " + openFileDialog1.SafeFileNames[i]);
            }
        }
        private void rem_click(object sender, RoutedEventArgs e)
        {
            if (List.Items.Count > 0 && playlist_list.Count > 0)
            {
                playlist_list.RemoveAt(List.SelectedIndex);
                List.Items.Remove(List.SelectedItem);
                if(List.Items.Count == 0 && playlist_list.Count == 0)
                    Player.Source = null;
            }
        }
        private void clear_pl_Click(object sender, RoutedEventArgs e)
        {
            if (List.Items.Count > 0 && playlist_list.Count > 0)
            {
                playlist_list.Clear();
                List.Items.Clear();
                Player.Source = null;
            }
        }
        private void Rewind_Button(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null)
            {
                double n = Player.Position.TotalSeconds;
                if (n - 10 > 0)
                    Player.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(n) - 10, 0);
            }
        }
        private void Forward_Button(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null)
            {
                double n = Player.Position.TotalSeconds;
                if (n + 10 < progress_player.Maximum)
                    Player.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(n) + 10, 0);
            }
        }
        //
        //Sliders
        private void volume_menu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            volume_menu.Value = (int)Math.Round(e.NewValue);
            Player.Volume = (double)e.NewValue/100;
            volume_menu.SelectionEnd = Convert.ToInt32(e.NewValue);
        }
        private void balance_menu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            balance_menu.Value = (int)Math.Round(e.NewValue);
            Player.Balance = balance_menu.Value/10;
        }
        private void balance_menu_GotMouseCapture(object sender, MouseEventArgs e)
        {
            balance_label.Visibility = Visibility.Visible;            
        }
        private void balance_menu_LostMouseCapture(object sender, MouseEventArgs e)
        {
            balance_label.Visibility = Visibility.Hidden;
        }
        private void volume_menu_GotMouseCapture(object sender, MouseEventArgs e)
        {
            volume_label.Visibility = Visibility.Visible;
        }
        private void volume_menu_LostMouseCapture(object sender, MouseEventArgs e)
        {
            volume_label.Visibility = Visibility.Hidden;
        }
        private void progress_player_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int p = (int)((e.GetPosition(progress_player).X / progress_player.ActualWidth)*progress_player.Maximum);
            progress_player.Value = p;
            Player.Position = new TimeSpan(0, 0, 0, p, 0);
        }
    }
}