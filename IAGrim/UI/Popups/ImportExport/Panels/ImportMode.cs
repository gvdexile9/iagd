﻿using IAGrim.Backup.FileWriter;
using IAGrim.Database.Interfaces;
using IAGrim.Parsers.Arz;
using IAGrim.UI.Controller;
using IAGrim.Utilities.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAGrim.Services;
using IAGrim.Utilities;

namespace IAGrim.UI.Popups.ImportExport.Panels {
    partial class ImportMode : Form {
        private readonly GDTransferFile[] _modSelection;
        private readonly IPlayerItemDao _playerItemDao;
        private string _filename;
        private readonly TransferStashService _sm;

        public ImportMode(GDTransferFile[] modSelection, IPlayerItemDao playerItemDao, TransferStashService sm) {
            InitializeComponent();
            this._modSelection = modSelection;
            this._playerItemDao = playerItemDao;
            this._sm = sm;
        }

        private void ImportMode_Load(object sender, EventArgs e) {
            this.Dock = DockStyle.Fill;
            this.buttonImport.Enabled = false;
            cbItemSelection.Visible = false;
            cbItemSelection.Enabled = false;

            cbItemSelection.Items.AddRange(_modSelection);
            LocalizationLoader.ApplyLanguage(Controls, RuntimeSettings.Language);
        }

        private void radioIAStash_CheckedChanged(object sender, EventArgs e) {
            cbItemSelection.Visible = !(sender as Control).Enabled;
        }

        private void radioGDStash_CheckedChanged(object sender, EventArgs e) {
            cbItemSelection.Visible = (sender as Control).Enabled;
        }
        private void radioGameStash_CheckedChanged(object sender, EventArgs e) {
            cbItemSelection.Visible = !(sender as Control).Enabled;
        }

        private void buttonBrowse_Click(object sender, EventArgs e) {
            if (buttonBrowse.Enabled) {
                OpenFileDialog diag = new OpenFileDialog {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Grim Dawn", "Save"),
                    Multiselect = false,
                    Title = "Select the file to import"
                };

                if (radioGDStash.Checked) {
                    diag.DefaultExt = "gds";
                    diag.Filter = "GD Stash exports (*.gds)|*.gds";
                }
                else if (radioIAStash.Checked) {
                    diag.DefaultExt = "ias";
                    diag.Filter = "IA Stash exports (*.ias)|*.ias";
                }
                else {
                    diag.DefaultExt = "gst";
                    diag.Filter = "Grim Dawn Stash files (*.gst)|*.gst|HD Hardcore Stash files (*.gsh)|*.gsh";
                    diag.FileName = "transfer.gst";
                }

                if (diag.ShowDialog() == DialogResult.OK) {
                    radioGDStash.Enabled = false;
                    radioIAStash.Enabled = false;
                    buttonImport.Enabled = true;
                    cbItemSelection.Enabled = true;
                    this._filename = diag.FileName;
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e) {
            if (buttonImport.Enabled) {
                FileExporter io;

                if (radioIAStash.Checked) {
                    io = new IAFileExporter(_filename);
                }
                else if (radioGDStash.Checked) {
                    GDTransferFile settings = cbItemSelection.SelectedItem as GDTransferFile;
                    if (settings == null) {
                        io = new GDFileExporter(_filename, false, string.Empty);
                    }
                    else {
                        io = new GDFileExporter(_filename, settings.IsExpansion1, settings.Mod);
                    }
                }
                else {
                    _playerItemDao.Save(_sm.EmptyStash(_filename));
                    MessageBox.Show("Items imported", "Items imported!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var items = io.Read();
                _playerItemDao.Import(items);

                MessageBox.Show("Items imported\nIf you already had items, you may have gotten duplicates.", "Items imported!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void helpRestoreBackup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            HelpService.ShowHelp(HelpService.HelpType.RestoreBackup);
        }
    }
}
