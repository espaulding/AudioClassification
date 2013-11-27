namespace AudioClassification {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lbFiles = new System.Windows.Forms.ListBox();
            this.fbdAudio = new System.Windows.Forms.FolderBrowserDialog();
            this.ofdAudio = new System.Windows.Forms.OpenFileDialog();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cbLooping = new System.Windows.Forms.CheckBox();
            this.btnExtractFeatures = new System.Windows.Forms.Button();
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAudioFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAudioFileToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAudioFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadAudioFolderToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnClassify = new System.Windows.Forms.Button();
            this.msMainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbFiles
            // 
            this.lbFiles.FormattingEnabled = true;
            this.lbFiles.Location = new System.Drawing.Point(12, 50);
            this.lbFiles.Name = "lbFiles";
            this.lbFiles.Size = new System.Drawing.Size(252, 342);
            this.lbFiles.TabIndex = 2;
            this.lbFiles.SelectedIndexChanged += new System.EventHandler(this.lbFiles_SelectedIndexChanged);
            this.lbFiles.DoubleClick += new System.EventHandler(this.lbFiles_DoubleClick);
            // 
            // ofdAudio
            // 
            this.ofdAudio.FileName = "openFileDialog1";
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 21);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(80, 23);
            this.btnPlay.TabIndex = 3;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(98, 21);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(293, 16);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 28);
            this.btnClear.TabIndex = 6;
            this.btnClear.Text = "Clear Playlist";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // cbLooping
            // 
            this.cbLooping.AutoSize = true;
            this.cbLooping.Location = new System.Drawing.Point(184, 25);
            this.cbLooping.Name = "cbLooping";
            this.cbLooping.Size = new System.Drawing.Size(80, 17);
            this.cbLooping.TabIndex = 7;
            this.cbLooping.Text = "Loop Audio";
            this.cbLooping.UseVisualStyleBackColor = true;
            // 
            // btnExtractFeatures
            // 
            this.btnExtractFeatures.Location = new System.Drawing.Point(379, 16);
            this.btnExtractFeatures.Name = "btnExtractFeatures";
            this.btnExtractFeatures.Size = new System.Drawing.Size(106, 28);
            this.btnExtractFeatures.TabIndex = 10;
            this.btnExtractFeatures.Text = "Extract Features";
            this.btnExtractFeatures.UseVisualStyleBackColor = true;
            this.btnExtractFeatures.Click += new System.EventHandler(this.btnExtractFeatures_Click);
            // 
            // msMainMenu
            // 
            this.msMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.msMainMenu.Location = new System.Drawing.Point(0, 0);
            this.msMainMenu.Name = "msMainMenu";
            this.msMainMenu.Size = new System.Drawing.Size(788, 24);
            this.msMainMenu.TabIndex = 11;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAudioFileToolStripMenuItem,
            this.loadAudioFileToolStripMenuItem2,
            this.loadAudioFolderToolStripMenuItem,
            this.loadAudioFolderToolStripMenuItem2});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadAudioFileToolStripMenuItem
            // 
            this.loadAudioFileToolStripMenuItem.Name = "loadAudioFileToolStripMenuItem";
            this.loadAudioFileToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.loadAudioFileToolStripMenuItem.Text = "Load Audio File (music)";
            this.loadAudioFileToolStripMenuItem.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // loadAudioFileToolStripMenuItem2
            // 
            this.loadAudioFileToolStripMenuItem2.Name = "loadAudioFileToolStripMenuItem2";
            this.loadAudioFileToolStripMenuItem2.Size = new System.Drawing.Size(219, 22);
            this.loadAudioFileToolStripMenuItem2.Text = "Load Audio File (speech)";
            this.loadAudioFileToolStripMenuItem2.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // loadAudioFolderToolStripMenuItem
            // 
            this.loadAudioFolderToolStripMenuItem.Name = "loadAudioFolderToolStripMenuItem";
            this.loadAudioFolderToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.loadAudioFolderToolStripMenuItem.Text = "Load Audio Folder (music)";
            this.loadAudioFolderToolStripMenuItem.Click += new System.EventHandler(this.btnLoadFolder_Click);
            // 
            // loadAudioFolderToolStripMenuItem2
            // 
            this.loadAudioFolderToolStripMenuItem2.Name = "loadAudioFolderToolStripMenuItem2";
            this.loadAudioFolderToolStripMenuItem2.Size = new System.Drawing.Size(219, 22);
            this.loadAudioFolderToolStripMenuItem2.Text = "Load Audio Folder (speech)";
            this.loadAudioFolderToolStripMenuItem2.Click += new System.EventHandler(this.btnLoadFolder_Click);
            // 
            // txtResults
            // 
            this.txtResults.Enabled = false;
            this.txtResults.Location = new System.Drawing.Point(293, 50);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.Size = new System.Drawing.Size(483, 342);
            this.txtResults.TabIndex = 12;
            // 
            // btnClassify
            // 
            this.btnClassify.Location = new System.Drawing.Point(491, 16);
            this.btnClassify.Name = "btnClassify";
            this.btnClassify.Size = new System.Drawing.Size(106, 28);
            this.btnClassify.TabIndex = 13;
            this.btnClassify.Text = "Classify Playlist";
            this.btnClassify.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 409);
            this.Controls.Add(this.btnClassify);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.btnExtractFeatures);
            this.Controls.Add(this.cbLooping);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.lbFiles);
            this.Controls.Add(this.msMainMenu);
            this.MainMenuStrip = this.msMainMenu;
            this.Name = "frmMain";
            this.Text = "Audio Classifier";
            this.msMainMenu.ResumeLayout(false);
            this.msMainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbFiles;
        private System.Windows.Forms.FolderBrowserDialog fbdAudio;
        private System.Windows.Forms.OpenFileDialog ofdAudio;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.CheckBox cbLooping;
        private System.Windows.Forms.Button btnExtractFeatures;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAudioFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAudioFolderToolStripMenuItem;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button btnClassify;
        private System.Windows.Forms.ToolStripMenuItem loadAudioFileToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem loadAudioFolderToolStripMenuItem2;
    }
}

