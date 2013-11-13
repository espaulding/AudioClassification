using System;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Linq;

namespace AudioClassification {
    public partial class frmMain : Form {
        private string[] ALLOWED_TYPES = new string[] { "WAV Files (*.wav)|*.wav", "MP3 Files (*.mp3)|*.mp3" };
        //private MediaLibrary mlAudio = new MediaLibrary();
        //private List<WAVFile>
        private SoundPlayer spAudio = new SoundPlayer();

        public frmMain() {
            InitializeComponent();
        }

        private void btnLoadFolder_Click(object sender, EventArgs e) {
            fbdAudio.ShowDialog();

            if (fbdAudio.SelectedPath.Length > 0) {
                DirectoryInfo dir = new DirectoryInfo(fbdAudio.SelectedPath);
                foreach (var file in dir.GetFiles()) {
                    lbFiles.Items.Add(new Sound(file.Name, file.FullName));
                }
            }
        }

        private void btnLoadFile_Click(object sender, EventArgs e) {
            ofdAudio.Filter = string.Join("|", ALLOWED_TYPES);
            ofdAudio.FilterIndex = 1;
            ofdAudio.FileName = "";
            ofdAudio.ShowDialog();

            if (ofdAudio.FileName != "") {
                string file = ofdAudio.FileName.Split('\\').Last();
                lbFiles.Items.Add(new Sound(file, ofdAudio.FileName));
            }
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            spAudio.Play();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            
        }

        private void btnStop_Click(object sender, EventArgs e) {
            spAudio.Stop();
        }

        private void lbFiles_SelectedIndexChanged(object sender, EventArgs e) {
            spAudio.SoundLocation = ((Sound)lbFiles.Items[lbFiles.SelectedIndex]).fullname;
            spAudio.Load();
        }

        private void lbFiles_DoubleClick(object sender, EventArgs e) {
            btnPlay_Click(sender, e);
        }
    }
}
