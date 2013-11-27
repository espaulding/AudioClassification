using System;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;

//NuGet: install into visual studio
//tools -> Library Package Manager

namespace AudioClassification {
    public partial class frmMain : Form {
        //Constants
        private string[] ALLOWED_TYPES = new string[] { "WAV Files (*.wav)|*.wav", "MP3 Files (*.mp3)|*.mp3" };

        //Class variables
        private SoundPlayer spAudio = new SoundPlayer();

        public frmMain() {
            InitializeComponent();
        }

        #region Form and Control Events

        #region playlist

        //Load all the files in an entire folder
        //TODO: restrict the files loaded to only those defined by ALLOWED_TYPES
        //TODO: find a better way to identify music or speech labels from the user
        private void btnLoadFolder_Click(object sender, EventArgs e) {
            string hardcoded_path = "";
            bool isMusic = false;
            ToolStripMenuItem tsmiSender = (ToolStripMenuItem)sender;
            if (tsmiSender.Name.Equals("loadAudioFolderToolStripMenuItem")) { isMusic = true; }
            //fbdAudio.ShowDialog();

            //if (fbdAudio.SelectedPath.Length > 0) {
            if (isMusic) {
                hardcoded_path = @"C:\Users\murikumo\Dropbox\Fall2013\CSCI 578 - Multimedia Systems\Project4\audio\music";
            } else {
                hardcoded_path = @"C:\Users\murikumo\Dropbox\Fall2013\CSCI 578 - Multimedia Systems\Project4\audio\speech";
            }
                DirectoryInfo dir = new DirectoryInfo(hardcoded_path);
                //DirectoryInfo dir = new DirectoryInfo(fbdAudio.SelectedPath);
                
                foreach (var file in dir.GetFiles()) {
                    lbFiles.Items.Add(new Sound(file.Name, file.FullName,isMusic));
                }
            //}
        }

        //Load one single file
        //Only file types defined in allowed types will be loaded
        //TODO: find a better way to identify music or speech labels from the user
        private void btnLoadFile_Click(object sender, EventArgs e) {
            bool isMusic = false;
            ToolStripMenuItem tsmiSender = (ToolStripMenuItem)sender;
            if (tsmiSender.Name.Equals("loadAudioFileToolStripMenuItem")) { isMusic = true; }
            ofdAudio.Filter = string.Join("|", ALLOWED_TYPES);
            ofdAudio.FilterIndex = 1;
            ofdAudio.FileName = "";
            ofdAudio.ShowDialog();

            if (ofdAudio.FileName != "") {
                string file = ofdAudio.FileName.Split('\\').Last();
                lbFiles.Items.Add(new Sound(file, ofdAudio.FileName,isMusic));
            }
        }

        private void btnClear_Click(object sender, EventArgs e) {
            lbFiles.Items.Clear();
        }

        //Loop over the entire playlist and generate the audio features for each sound object
        private void btnExtractFeatures_Click(object sender, EventArgs e) {
            for (int c = 0; c < lbFiles.Items.Count; c++) {
                ((Sound)lbFiles.Items[c]).ExtractFeatures();
            }
        }

        #endregion //playlist

        #region player

        //Play the file but also check whether the user wants to loop it or not
        private void btnPlay_Click(object sender, EventArgs e) {
            if (lbFiles.Items.Count != 0) {
                if (cbLooping.Checked) {
                    spAudio.PlayLooping();
                } else {
                    spAudio.Play();
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e) {
            spAudio.Stop();
        }

        //Point the audio player at the currently selected file
        private void lbFiles_SelectedIndexChanged(object sender, EventArgs e) {
            spAudio.SoundLocation = ((Sound)lbFiles.Items[lbFiles.SelectedIndex]).fullname;
            spAudio.Load();
        }

        //Allow user to play a file by double clicking its name in the playlist
        private void lbFiles_DoubleClick(object sender, EventArgs e) {
            btnPlay_Click(sender, e);
        }

        #endregion //player

        #endregion //Form and Control Events

        #region data mining

        #endregion //data mining
    }
}
