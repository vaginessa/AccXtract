﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AccXtract
{
    public partial class Form1 : Form
    {
        GroupBox lastGroup;
        string AccXtractFolder;
        public Form1()
        {
            InitializeComponent();
            string home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            lastGroup = localGroup;
            LoadLocalChrome(home);
        }

        private void LoadLocalChrome(string home)
        {
            string chromeDir = home +@"\AppData\Local\Google\Chrome\";
            if (Directory.Exists(chromeDir))
            {
                string line;

                System.IO.StreamReader file = new StreamReader(chromeDir + @"User Data\Local State");
                int currentNumberOfProfiles = 0;
                Button currentButton = (Button)chromeLocalPanel.Controls[chromeLocalPanel.Controls.Count - 1];
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("\"shortcut_name\""))
                    {

                        string[] values = line.Split('"');
                        Console.WriteLine(values[3]);

                        if (currentNumberOfProfiles != 0)
                        {
                            currentButton = addButtonToPanel(values[3], chromeLocalPanel);
                        }

                        else
                        {
                            if (values[3] != "") currentButton.Text = values[3];
                        }

                        //this will be changed later
                        currentButton.Enabled = false;

                        currentNumberOfProfiles++;
                    }
                }

                file.Close();

                //Resize for the scroll bar
                if (chromeLocalPanel.HorizontalScroll.Visible) chromeLocalPanel.Size = new Size(chromeLocalPanel.Size.Width, chromeLocalPanel.Size.Height + 17);
            }

            else
            {
                chromeLocalPanel.Enabled = false;
            }
        }

        private void loadFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AccXtractFolder = diag.SelectedPath;
                string[] computers = Directory.GetDirectories(AccXtractFolder);
                foreach (string computer in computers)
                {
                    string[] components = computer.Split('\\');
                    string computerName = components[components.Length - 1];

                    
                    lastGroup = addNewComputer(computer);
                    //LoadChrome(computer, false);
                }
            }
        }

        private void addChromeProfile(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string profileName = button.Text;


            string localStatePath = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + @"\AppData\Local\Google\Chrome\User Data\";
            StreamReader localState = new StreamReader(localStatePath + "\\Local State");
            StreamWriter output = new StreamWriter(localStatePath + "\\Local State.out");

            string line = "";
            bool foundProfiles = false;

            while (!foundProfiles && line != null)
            {
                line = localState.ReadLine();
                output.WriteLine(line);
                if (line.Contains(@"""profile"": {"))
                {
                    foundProfiles = true;
                }
            }

            bool wroteNewProfile = false;
            int highestProfileNumber = 0;
            
            while (!wroteNewProfile && line != null)
            {
                line = localState.ReadLine();

                if (line.Contains(@"""Profile "))
                {
                    string number = line[line.IndexOf('e') + 2].ToString();
                    highestProfileNumber = Convert.ToInt32(number);
                }

                if (line.Contains(@"}"))
                {
                    if (!line.Contains(@","))
                    {
                        line += ",";
                        output.WriteLine(line);
                        highestProfileNumber++;
                        
                        //WARNING
                        //do ***NOT*** modify spacing of the quoted parted in ANY WAY if you want it to keep working
                        //Chrome is very picky with this thing
                        output.Write(@"         ""Profile " + highestProfileNumber + @""": {
            ""avatar_icon"": ""chrome://theme/IDR_PROFILE_AVATAR_12"",
            ""background_apps"": false,
            ""managed_user_id"": """",
            ""name"": """ + profileName + @""",
            ""shortcut_name"": """ + profileName + @""",
            ""user_name"": """"
         }
");
                        wroteNewProfile = true;
                    }

                    else output.WriteLine(line);
                }

                else output.WriteLine(line);
            }

            while (line != null)
            {
                line = localState.ReadLine();
                output.WriteLine(line);
            }

            localState.Close();
            output.Close();

            GroupBox group = (GroupBox)button.Parent.Parent;
            string computerName = group.Text;

            string newUserProfile = localStatePath + "Profile " + highestProfileNumber.ToString();
            Directory.CreateDirectory(newUserProfile);
            File.Copy(AccXtractFolder + "\\" + computerName + "\\Chrome\\" + button.Text + "\\cookies", newUserProfile + "\\cookies");
            File.Copy(localStatePath + "\\Local State.out", localStatePath + "\\Local State", true);
            File.Delete(localStatePath + "Local State.out");

            button.Enabled = false;
        }

        #region New Creation Functions
        private GroupBox addNewComputer(string path)
        {
            GroupBox newGroup = new GroupBox();

            string[] components = path.Split('\\');
            string computerName = components[components.Length - 1];

            newGroup.Text = computerName;
            newGroup.AutoSize = true;
            newGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            newGroup.Tag = computerName + "Group";

            Point location = new Point(lastGroup.Location.X, lastGroup.Location.Y + lastGroup.Height + 4);
            newGroup.Location = location;

            this.Controls.Add(newGroup);

            Panel lastPanel = null;

            if (Directory.Exists(path + "\\Chrome"))
            {
                lastPanel = addPanel(@"Google Chrome (Click to add to Chrome)", lastPanel, newGroup);
                string[] profiles = Directory.GetDirectories(path + "\\Chrome");

                foreach (string profile in profiles)
                {
                    string[] components2 = profile.Split('\\');
                    string profileName = components2[components2.Length - 1];
                    Button newButton = addButtonToPanel(profileName, lastPanel);
                    newButton.Click += addChromeProfile;
                }
            }

            return newGroup;
        }

        private Panel addPanel(string title, Panel lastPanel, GroupBox group)
        {
            Panel newPanel = new Panel();
            Label newLabel = new Label();

            if (lastPanel != null)
            {
                newLabel.Location = new Point(7, lastPanel.Location.Y + lastPanel.Height + 12);
                newPanel.Location = new Point(10, newLabel.Location.Y + newLabel.Height + 3);
            }

            else
            {
                newLabel.Location = new Point(7, 20);
                newPanel.Location = new Point(10, 37);
            }

            newLabel.Text = title;
            newLabel.AutoSize = true;
            
            newPanel.AutoScroll = true;
            newPanel.Size = chromeLocalPanel.Size;
            newPanel.Tag = title + "Panel";

            group.Controls.Add(newLabel);
            group.Controls.Add(newPanel);

            return newPanel;
        }

        private Button addButtonToPanel(string title, Panel panel)
        {
            Button newButton = new Button();

            newButton.Text = title;
            newButton.AutoSize = true;
            newButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            if (panel.Controls.Count != 0)
            {
                Button oldButton = (Button)panel.Controls[panel.Controls.Count - 1];
                newButton.Location = new Point(oldButton.Location.X + oldButton.Width + 3, 5);
            }

            else
            {
                newButton.Location = new Point(4, 5);
            }

            panel.Controls.Add(newButton);

            return newButton;
        }

        #endregion
    }
}