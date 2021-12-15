﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using PasteIntoFile.Properties;
using WK.Libraries.BetterFolderBrowserNS;

namespace PasteIntoFile
{
    public partial class frmMain : Form
    {
        public const string DefaultFilenameFormat = "yyyy-MM-dd HH-mm-ss";
        public string CurrentLocation { get; set; }
        public bool IsText { get; set; }
        
        public frmMain(string location = null)
        {
            CurrentLocation = location;
            
            // Setup GUI
            InitializeComponent();
            
            foreach (Control element in GetAllChild(this))
            {
                element.Text = Resources.ResourceManager.GetString(element.Text) ?? element.Text;
            }
            
            Icon = Resources.icon;
            Text = Resources.str_main_window_title;
            infoLabel.Text = string.Format(Resources.str_version, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            

            // Dark theme
            if (Settings.Default.darkTheme)
            {
                foreach (Control element in GetAllChild(this))
                {
                    element.ForeColor = Color.White;
                    element.BackColor = Color.FromArgb(53, 53, 53);
                }
                
                BackColor = Color.FromArgb(24, 24, 24);
                linkRegister.ForeColor = Color.LightBlue;
                linkUnregister.ForeColor = Color.LightBlue;

            }

            //

            var key = @"HKEY_CURRENT_USER\Software\Classes\Directory\shell\" + Program.RegistrySubKey + @"\filename";
            var filenameFormat = (string) Registry.GetValue(key, "", null) ?? DefaultFilenameFormat;
            txtFilename.Text = DateTime.Now.ToString(filenameFormat);
            txtCurrentLocation.Text = CurrentLocation ?? @Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            chkClrClipboard.Checked = Settings.Default.clrClipboard;
            chkAutoSave.Checked = Settings.Default.autoSave;
            
            
            if (Clipboard.ContainsText())
            {
                IsText = true;
                txtContent.Text = Clipboard.GetText();
                txtContent.Show();
                comExt.Items.AddRange(new object[] {
                    "txt", "html", "js", "css", "csv", "json", "cs", "cpp", "java", "php", "py"
                });
                // heuristic to suggest filetype
                if (Regex.IsMatch(txtContent.Text, "<html.*>(.|\n)*</html>")) {
                    comExt.SelectedItem = "html";
                } else if (Regex.IsMatch(txtContent.Text, "^\\{(.|\n)*:(.|\n)*\\}$")) {
                    comExt.SelectedItem = "json";
                } else {
                    comExt.SelectedItem = "txt";
                }
            }
            else if (Clipboard.ContainsImage())
            {
                comExt.Items.AddRange(new object[] {
                    "bpm", "emf", "gif", "ico", "jpg", "png", "tif", "wmf"
                });
                comExt.DropDownStyle = ComboBoxStyle.DropDownList; // prevent custom formats
                comExt.SelectedItem = "png";
                imgContent.Show();
                imgContent.BackgroundImage = Clipboard.GetImage();

            }

            // Pressed shift key resets autosave option
            if (ModifierKeys == Keys.Shift)
            {
                // Make sure to bring window to foreground
                WindowState = FormWindowState.Minimized;
                Show();
                BringToFront();
                WindowState = FormWindowState.Normal;
            }
            // otherwise perform autosave if enabled
            else if (chkAutoSave.Checked)
            {
                save();

                var message = string.Format(Resources.str_autosave_balloontext, txtCurrentLocation.Text + @"\" + txtFilename.Text + "." + comExt.Text);
                Program.ShowBalloon(Resources.str_autosave_balloontitle, message, 10_000);

                Environment.Exit(0);
            }
        }
        
        public IEnumerable<Control> GetAllChild(Control control, Type type = null)
        {
            var controls = control.Controls.Cast<Control>();
            var enumerable = controls as Control[] ?? controls.ToArray();
            return enumerable.SelectMany(ctrl => GetAllChild(ctrl, type))
                .Concat(enumerable)
                .Where(c => type == null || type == c.GetType());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            save();
            Environment.Exit(0);
        }
        
        void save()
        {

            string location = txtCurrentLocation.Text;
            string file = location + (location.EndsWith("\\") ? "" : "\\");
            file += txtFilename.Text + (txtFilename.Text.EndsWith("." + comExt.Text) ? "" : "." + comExt.Text);
            try
            {
                if (IsText)
                {
                    File.WriteAllText(file, txtContent.Text, Encoding.UTF8);
                }
                else
                {
                    ImageFormat format;
                    switch (comExt.Text)
                    {
                        case "bpm": format = ImageFormat.Bmp; break;
                        case "emf": format = ImageFormat.Emf; break;
                        case "gif": format = ImageFormat.Gif; break;
                        case "ico": format = ImageFormat.Icon; break;
                        case "jpg": format = ImageFormat.Jpeg; break;
                        case "tif": format = ImageFormat.Tiff; break;
                        case "wmf": format = ImageFormat.Wmf; break;
                        default: format = ImageFormat.Png; break;
                    }

                    imgContent.BackgroundImage.Save(file, format);
                }

                if (chkClrClipboard.Checked)
                {
                    Clipboard.Clear();
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message + "\n" + Resources.str_message_run_as_admin, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.RestartAppElevated(location);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            
        }

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            BetterFolderBrowser betterFolderBrowser = new BetterFolderBrowser();

            betterFolderBrowser.Title = Resources.str_select_folder;
            betterFolderBrowser.RootFolder = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Allow multi-selection of folders.
            betterFolderBrowser.Multiselect = false;

            if (betterFolderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtCurrentLocation.Text = betterFolderBrowser.SelectedFolder;
            }
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Environment.Exit(0);
            }
        }

        private void ChkClrClipboard_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.clrClipboard = chkClrClipboard.Checked;
            Settings.Default.Save();
        }

        private void ChkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoSave.Checked && !Settings.Default.autoSave)
            {
                MessageBox.Show(Resources.str_autosave_infotext, Resources.str_autosave, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            Settings.Default.autoSave = chkAutoSave.Checked;
            Settings.Default.Save();

        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Program.RegisterApp();
        }

        private void linkUnregister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Program.UnRegisterApp();
        }

        private void infoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Resources.str_main_info_url);
        }
    }
}