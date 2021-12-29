﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PasteIntoFile.Properties;
using WK.Libraries.BetterFolderBrowserNS;
using WK.Libraries.SharpClipboardNS;

namespace PasteIntoFile
{
    public partial class Dialog : MasterForm
    {
        private string text;
        private Image image;
        private readonly SharpClipboard clipboard = new SharpClipboard();
        private bool continuousMode = false;
        private int saveCount = 0;
        private DateTime clipboardTimestamp;

        public Dialog(string location, bool forceShowDialog = false)
        {
            // always show GUI if shift pressed during start
            forceShowDialog |= ModifierKeys == Keys.Shift;
            
            // Setup GUI
            InitializeComponent();
            
            foreach (Control element in GetAllChild(this))
            {
                // ReSharper disable once UnusedVariable (to convince IDE that these resource strings are actually used)
                string[] usedResourceStrings = { Resources.str_filename, Resources.str_extension, Resources.str_location, Resources.str_clear_clipboard, Resources.str_save, Resources.str_preview, Resources.str_main_info, Resources.str_autosave_checkbox, Resources.str_contextentry_checkbox, Resources.str_continuous_mode };
                element.Text = Resources.ResourceManager.GetString(element.Text) ?? element.Text;
            }
            
            Icon = Resources.icon;
            Text = Resources.str_main_window_title;
            infoLabel.Text = string.Format(Resources.str_version, ProductVersion);
            

            // Dark theme
            if (RegistryUtil.IsDarkMode())
            {
                foreach (Control element in GetAllChild(this))
                {
                    element.ForeColor = Color.White;
                    element.BackColor = Color.FromArgb(53, 53, 53);
                }
                
                BackColor = Color.FromArgb(24, 24, 24);

            }

            // read clipboard and populate GUI

            if (!readClipboard())
                Environment.Exit(1);
            
            updateFilename();
            txtCurrentLocation.Text = Path.GetFullPath(location);
            chkClrClipboard.Checked = Settings.Default.clrClipboard;
            chkContinuousMode.Checked = continuousMode;
            updateSavebutton(); 
            chkAutoSave.Checked = Settings.Default.autoSave;
            chkContextEntry.Checked = RegistryUtil.IsAppRegistered();
            

            txtFilename.Select();

            // show dialog or autosave option
            if (forceShowDialog)
            {
                // Make sure to bring window to foreground (holding shift will open window in background)
                WindowState = FormWindowState.Minimized;
                Show();
                BringToFront();
                WindowState = FormWindowState.Normal;
            }
            // otherwise perform autosave if enabled
            else if (Settings.Default.autoSave)
            {
                var file = save();
                if (file != null)
                {
                    ExplorerUtil.RequestFilenameEdit(file);
                    
                    var message = string.Format(Resources.str_autosave_balloontext, file);
                    Program.ShowBalloon(Resources.str_autosave_balloontitle, message, 10_000);

                    Environment.Exit(0);
                }
            }
            
            // register clipboard monitor
            clipboard.ClipboardChanged += ClipboardChanged;
            
        }

        public string formatFilenameTemplate(string template)
        {
            return String.Format(template, clipboardTimestamp, saveCount);
        }
        public void updateFilename()
        {
            try
            {
                txtFilename.Text = formatFilenameTemplate(Settings.Default.filenameTemplate);
            }
            catch (FormatException)
            {
                txtFilename.Text = "filename_template_invalid";
            }
        }
        
        private bool readClipboard()
        {
            clipboardTimestamp = DateTime.Now;
                
            // reset GUI elements
            text = null;
            txtContent.Hide();
            image = null;
            imgContent.Hide();
            comExt.Items.Clear();

            if (Clipboard.ContainsText())
            {
                text = Clipboard.GetText();
                txtContent.Text = text;
                txtContent.Show();
                box.Text = string.Format(Resources.str_preview_text, text.Length, text.Split('\n').Length);
                comExt.Items.AddRange(new object[] {
                    "bat", "java", "js", "json", "cpp", "cs", "css", "csv", "html", "md", "php", "ps1", "py", "txt", "url"
                });
                comExt.DropDownStyle = ComboBoxStyle.DropDown;
                comExt.Text = Settings.Default.extensionText == null ? "txt" : Settings.Default.extensionText;
                return true;
            }
            
            if (Clipboard.ContainsImage())
            {
                image = Clipboard.GetImage();
                imgContent.BackgroundImage = image;
                imgContent.Show();
                box.Text = string.Format(Resources.str_preview_image, image.Width, image.Height);
                comExt.Items.AddRange(new object[] {
                    "bpm", "emf", "gif", "ico", "jpg", "png", "tif", "wmf"
                });
                comExt.DropDownStyle = ComboBoxStyle.DropDownList; // prevent custom formats
                comExt.SelectedItem = comExt.Items.Contains(Settings.Default.extensionImage) ? Settings.Default.extensionImage : "png";
                return true;
            }
            
            MessageBox.Show(Resources.str_noclip_text, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
            
        }
        
        private void ClipboardChanged(Object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            var previousClipboardTimestamp = clipboardTimestamp;
            readClipboard();

            // ignore duplicate clipboard content updates within 100ms
            if (continuousMode && (clipboardTimestamp - previousClipboardTimestamp).TotalMilliseconds > 100)
            {
                updateFilename();
                save();
                updateSavebutton();
            }
        }

        private void updateSavebutton()
        {
            btnSave.Enabled = txtFilename.Enabled = !continuousMode;
            btnSave.Text = continuousMode ? string.Format(Resources.str_n_saved, saveCount) : Resources.str_save;
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (save() != null)
            {
                Environment.Exit(0);
            }
        }
        
        string save()
        {
            try {
                string dirname = Path.GetFullPath(txtCurrentLocation.Text);
                string ext = comExt.Text.ToLowerInvariant();
                string filename = txtFilename.Text;
                if (!string.IsNullOrWhiteSpace(ext) && !filename.EndsWith("." + ext))
                    filename += "." + ext;
                string file = Path.Combine(dirname, filename);
                
                // check if file exists
                if (File.Exists(file))
                {
                    var result = MessageBox.Show(string.Format(Resources.str_file_exists, file), Resources.str_main_window_title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        return null;
                    }
                } else if (Directory.Exists(file))
                {
                    MessageBox.Show(string.Format(Resources.str_file_exists_directory, file), Resources.str_main_window_title,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            
            
                // create folders if required
                Directory.CreateDirectory(dirname);

                if (text != null)
                {
                    switch (ext)
                    {
                        case "url":
                            if (!Uri.IsWellFormedUriString(text.Trim(), UriKind.RelativeOrAbsolute))
                            {
                                MessageBox.Show(Resources.str_text_is_no_uri, Resources.str_main_window_title,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return null;
                            }
                            
                            File.WriteAllLines(file, new[] {
                                @"[InternetShortcut]",
                                @"URL=" + text.Trim()
                            }, Encoding.UTF8);
                            break;
                        
                        default:
                            File.WriteAllText(file, text, Encoding.UTF8);
                            break;
                    }

                    
                }
                else if (image != null)
                {
                    ImageFormat format;
                    switch (ext)
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

                    image.Save(file, format);
                }
                else
                {
                    return null;
                }
                
                if (Settings.Default.clrClipboard)
                {
                    Clipboard.Clear();
                }

                saveCount++;
                return file;

            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message + "\n" + Resources.str_message_run_as_admin, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Program.RestartAppElevated(txtCurrentLocation.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return null;
        }

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            BetterFolderBrowser betterFolderBrowser = new BetterFolderBrowser();

            betterFolderBrowser.Title = Resources.str_select_folder;
            betterFolderBrowser.RootFolder = txtCurrentLocation.Text ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Allow multi-selection of folders.
            betterFolderBrowser.Multiselect = false;

            if (betterFolderBrowser.ShowDialog(this) == DialogResult.OK)
            {
                txtCurrentLocation.Text = betterFolderBrowser.SelectedFolder;
            }
        }

        private void Main_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Escape)
            {
                Environment.Exit(1);
            }
        }

        private void ChkClrClipboard_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.clrClipboard = chkClrClipboard.Checked;
            Settings.Default.Save();
        }

        private void chkContinuousMode_CheckedChanged(object sender, EventArgs e)
        {
            if (chkContinuousMode.Checked)
            {
                var saveNow = MessageBox.Show(Resources.str_continuous_mode_enabled_ask_savenow, Resources.str_continuous_mode, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (saveNow == DialogResult.Yes) // save current clipboard now
                {
                    updateFilename();
                    save();
                }
                else if (saveNow != DialogResult.No)
                    chkContinuousMode.Checked = false;
            } 
            
            continuousMode = chkContinuousMode.Checked;
            updateSavebutton();
                
        }

        private void ChkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoSave.Checked && !Settings.Default.autoSave)
            {
                MessageBox.Show(Resources.str_autosave_infotext, Resources.str_autosave_checkbox, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            Settings.Default.autoSave = chkAutoSave.Checked;
            Settings.Default.Save();

        }

        private void ChkContextEntry_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkContextEntry.Checked && !RegistryUtil.IsAppRegistered())
                {
                    RegistryUtil.RegisterApp();
                    MessageBox.Show(Resources.str_message_register_context_menu_success, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!chkContextEntry.Checked && RegistryUtil.IsAppRegistered())
                {
                    RegistryUtil.UnRegisterApp();
                    MessageBox.Show(Resources.str_message_unregister_context_menu_success, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + Resources.str_message_run_as_admin, Resources.str_main_window_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void infoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Resources.str_main_info_url);
        }

        private void comExt_Update(object sender, EventArgs e)
        {
            if (text != null)
            {
                Settings.Default.extensionText = comExt.Text;
            }
            else if (image != null)
            {
                Settings.Default.extensionImage = comExt.Text;
            }
            Settings.Default.Save();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var dialog = new TemplateEdit();
            dialog.FormClosed += DialogOnFormClosed;
            dialog.ShowDialog(this);
        }

        private void DialogOnFormClosed(object sender, FormClosedEventArgs e)
        {
            updateFilename();
        }
    }
}