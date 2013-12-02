using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.MachineLearning;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Formats;
using Accord.Statistics.Kernels;
using ZedGraph;

//NuGet: install into visual studio
//tools -> Library Package Manager

namespace AudioClassification {
    public partial class frmMain : Form {
        //Constants
        private string[] ALLOWED_TYPES = new string[] { "WAV Files (*.wav)|*.wav", "MP3 Files (*.mp3)|*.mp3" };

        //Class variables
        private SoundPlayer spAudio = new SoundPlayer(); //used to playback the audio files as sound through a speaker

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
            DirectoryInfo dir;
            bool isMusic = false;
            ToolStripMenuItem tsmiSender = (ToolStripMenuItem)sender;
            if (tsmiSender.Name.Equals("loadAudioFolderToolStripMenuItem")) { isMusic = true; }
            fbdAudio.ShowDialog();

            if (fbdAudio.SelectedPath.Length > 0) {
                //if (isMusic) {
                //    hardcoded_path = @"C:\Users\murikumo\Dropbox\Fall2013\CSCI 578 - Multimedia Systems\Project4\audio\music";
                //}
                //else {
                //    hardcoded_path = @"C:\Users\murikumo\Dropbox\Fall2013\CSCI 578 - Multimedia Systems\Project4\audio\speech";
                //}
                //dir = new DirectoryInfo(hardcoded_path);
                dir = new DirectoryInfo(fbdAudio.SelectedPath);

                foreach (var file in dir.GetFiles()) {
                    lbFiles.Items.Add(new Sound(file.Name, file.FullName, isMusic));
                }
            }

            PlayListChanged();
            btnExtractFeatures.Enabled = (lbFiles.Items.Count > 0);
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
                lbFiles.Items.Add(new Sound(file, ofdAudio.FileName, isMusic));
            }

            PlayListChanged();
            btnExtractFeatures.Enabled = (lbFiles.Items.Count > 0);
        }

        //clear out the playlist and any accompanying user display
        private void btnClear_Click(object sender, EventArgs e) {
            lbFiles.Items.Clear();
            PlayListChanged();
        }

        //reset the analysis state when playlist is changed
        private void PlayListChanged() {
            btnExtractFeatures.Enabled = false;
            btnClassify.Enabled = false;
            dgvFeatures.DataSource = null;
            dgvStats.DataSource = null;
            dgvClassification.DataSource = null;
            dgvPerformance.DataSource = null;
        }

        //Collect audio features, statistics, and show the user
        private void btnExtractFeatures_Click(object sender, EventArgs e) {
            if (lbFiles.Items.Count == 0) { return; }

            bool display = true, addLabels = false, rawdata = false;
            DataTable features = DisplayFeatures(display, addLabels, rawdata);
            int numFeatures = ((Sound)lbFiles.Items[0]).features.Count;
            DisplayStats(numFeatures, features);

            btnClassify.Enabled = true;
        }

        //classify the playlist
        private void btnClassify_Click(object sender, EventArgs e) {
            //get the audio features
            bool display = false, addLabels = true, rawdata = true;
            DataTable features = DisplayFeatures(display, addLabels, rawdata);

            // Get the ground truth label outputs in integer form
            int[] labels = lbFiles.Items.OfType<Sound>().Select(s => MapBool(s.isMusic)).ToArray();
            string[] filenames = lbFiles.Items.OfType<Sound>().Select(s => s.filename).ToArray();

            //Get a random training set from 66% of the data
            int[] test = null, train = null, labelsTrain = null, labelsTest = null;
            GetRandomTrainingSet(.66, features, labels, ref train, ref labelsTrain, ref test, ref labelsTest);

            //use the decision tree to find the answeres to the remaining 34% of the data
            int[] answers = ExecuteDecisionTree(features, train, labelsTrain, test);       

            DisplayClassification(filenames, test, answers); //show the user the test set's #, model output, ground truth label
            DisplayClassificationStats(answers, labelsTest); //show the user stats about the classification
        }

        #endregion //playlist

        #region player

        //Play the file but also check whether the user wants to loop it or not
        private void btnPlay_Click(object sender, EventArgs e) {
            if (lbFiles.Items.Count != 0) {
                if (cbLooping.Checked) {
                    spAudio.PlayLooping();
                }
                else {
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

        #region form display

        //build a dataset out of the audio features
        //each row is an audio sample
        //each column is an audio feature
        private DataTable DisplayFeatures(bool display, bool addLabels, bool rawdata) {
            BindingSource bsFeatures = new BindingSource();
            DataSet dsFeatures = new DataSet();
            DataTable tableFeatures = dsFeatures.Tables.Add();

            //Loop over the entire playlist and generate the audio features for each sound object
            for (int c = 0; c < lbFiles.Items.Count; c++) {
                ((Sound)lbFiles.Items[c]).ExtractFeatures();
            }

            int numFeatures = ((Sound)lbFiles.Items[0]).features.Count;

            //columns headers
            if (!rawdata) {
                if (addLabels) { tableFeatures.Columns.Add("IsMusic", typeof(string)); }
                else { tableFeatures.Columns.Add("File Name", typeof(string)); }
            }
            for (int i = 0; i < numFeatures; i++) {
                tableFeatures.Columns.Add(Sound.featureNames[i], typeof(double));
            }
            
            //row data
            for (int r = 0; r < lbFiles.Items.Count; r++) {
                tableFeatures.Rows.Add();
                int shift = 0;

                //toggle either ground truth labels or filenames in the first column
                if (!rawdata) {
                    if (addLabels) {
                        string label = "no";
                        if (((Sound)lbFiles.Items[r]).isMusic) { label = "yes"; }
                        tableFeatures.Rows[r][0] = label;
                    }
                    else {
                        tableFeatures.Rows[r][0] = ((Sound)lbFiles.Items[r]).filename;
                    }
                    shift = 1;
                }

                for (int c = shift; c < numFeatures + shift; c++) {
                    tableFeatures.Rows[r][c] = ((Sound)lbFiles.Items[r]).features[c - shift];
                }
            }

            //bind the data to the gridview on the form
            if (display) {
                bsFeatures.DataSource = tableFeatures;
                dgvFeatures.DataSource = bsFeatures;
                dgvFeatures.AutoGenerateColumns = true;
            }

            return tableFeatures;
        }

        //Calculate and display the mean, standard deviation, max, and min of
        //each audio feature previously extracted from the audio files.
        //assume that all data columns will be the rightmost columns in features
        private void DisplayStats(int numFeatures, DataTable features) {
            BindingSource bsStats = new BindingSource();
            DataSet dsStats = new DataSet();
            DataTable tableStats = dsStats.Tables.Add();

            //stat columns headers
            tableStats.Columns.Add("Statistics", typeof(string));
            for (int i = 0; i < numFeatures; i++) {
                tableStats.Columns.Add(Sound.featureNames[i], typeof(double));
            }

            //stat row headers
            string[] rowHeaders = new string[] { "Mean", "Stdev", "Max", "Min" };

            int offset = 0; //assume that all data columns will be the rightmost columns in features
            offset = features.Columns.Count - numFeatures;

            //generate some statistics on the features
            double[] featureAvg = new double[numFeatures];
            double[] featureMax = new double[numFeatures];
            double[] featureMin = new double[numFeatures];
            double[] featureStdev = new double[numFeatures];

            for (int c = 0; c < numFeatures; c++) {
                if (features.Columns[c + offset].DataType == typeof(double)) {
                    double[] col = features.Columns[c + offset].ToArray();
                    featureAvg[c] = col.Average();
                    featureMax[c] = col.Max();
                    featureMin[c] = col.Min();
                    double sum = col.Sum(f => Math.Pow(f - featureAvg[c], 2));
                    featureStdev[c] = Math.Sqrt(sum / (col.Length - 1));
                }
            }

            //load the features into a dataset
            for (int r = 0; r < rowHeaders.Length; r++) {
                tableStats.Rows.Add("");
                tableStats.Rows[r][0] = rowHeaders[r];
            }
            for (int c = 0; c < numFeatures; c++) { tableStats.Rows[0][c + 1] = featureAvg[c]; }
            for (int c = 0; c < numFeatures; c++) { tableStats.Rows[1][c + 1] = featureStdev[c]; }
            for (int c = 0; c < numFeatures; c++) { tableStats.Rows[2][c + 1] = featureMax[c]; }
            for (int c = 0; c < numFeatures; c++) { tableStats.Rows[3][c + 1] = featureMin[c]; }

            //bind the data to the gridview on the form
            bsStats.DataSource = tableStats;
            dgvStats.DataSource = bsStats;
            dgvStats.AutoGenerateColumns = true;
        }

        //display the classification results
        private void DisplayClassification(string[] filenames, int[] test, int[] answers) {
            BindingSource bsClassify = new BindingSource();
            DataSet dsClassify = new DataSet();
            DataTable tableClassify = dsClassify.Tables.Add();

            string[] classifyHeaders = new string[] { "File", "#", "Model output", "Ground truth" };
            Type[] colTypes = new Type[] { typeof(string), typeof(int), typeof(string), typeof(string) };
            for (int i = 0; i < classifyHeaders.Length; i++) {
                tableClassify.Columns.Add(classifyHeaders[i], colTypes[i]);
            }

            //row data
            int testCounter = 0;
            for (int r = 0; r < filenames.Length; r++) {
                if (test.Contains(r)) {
                    tableClassify.Rows.Add();

                    //filename of the test set item
                    tableClassify.Rows[testCounter][0] = filenames[r];

                    tableClassify.Rows[testCounter][1] = r+1;

                    //the model output for the test set item
                    if (MapBool(answers[testCounter])) {
                        tableClassify.Rows[testCounter][2] = "Music";
                    } else {
                        tableClassify.Rows[testCounter][2] = "Speech";
                    }

                    //the ground truth of the test set item
                    if (((Sound)lbFiles.Items[r]).isMusic) {
                        tableClassify.Rows[testCounter][3] = "Music";
                    } else {
                        tableClassify.Rows[testCounter][3] = "Speech";
                    }
                    testCounter++;
                }
            }

            //bind the data to the gridview on the form
            bsClassify.DataSource = tableClassify;
            dgvClassification.DataSource = bsClassify;
            dgvClassification.AutoGenerateColumns = true;
            dgvClassification_Sorted(new object(), new EventArgs());
        }

        //make sure mismatches are given a redish background when columns are resorted
        private void dgvClassification_Sorted(object sender, EventArgs e) {
            for (int r = 0; r < dgvClassification.Rows.Count; r++) {
                //if the classifier got it wrong color this row's backcolor red
                if (!dgvClassification.Rows[r].Cells[2].Value.Equals(dgvClassification.Rows[r].Cells[3].Value)) {
                    dgvClassification.Rows[r].DefaultCellStyle.BackColor = Color.PaleVioletRed;
                }
            }
        }

        // Use confusion matrix to compute some statistics on the classification.
        // for precision and recall the computations treat positives as relevant results
        // currently music is considered positive and speech is considered negative
        // Precision : (# correct positives) over (# positives guessed) 
        // Recall    : (# correct positives) over (# positives possible)

        // maximum precision (no false positives) and maximum recall (no false negatives).

        //In simple terms, high recall means that an algorithm returned most of the relevant results, 
        //while high precision means that an algorithm returned substantially more relevant results than irrelevant.
        private void DisplayClassificationStats(int[] answers, int[] labelsTest) {
            BindingSource bsPerformance = new BindingSource();
            DataSet dsPerformance = new DataSet();
            DataTable tablePerformance = dsPerformance.Tables.Add();
            
            //assume that music is postive and speech is negative
            ConfusionMatrix stats = new ConfusionMatrix(answers, labelsTest, MapBool(true), MapBool(false));

            string[] colHeaders = new string[] { "NumSamples", "Accuracy", "Precision", "Recall" };
            for (int i = 0; i < colHeaders.Length; i++) { tablePerformance.Columns.Add(colHeaders[i], typeof(double)); }

            tablePerformance.Rows.Add();

            tablePerformance.Rows[0][0] = (double)stats.Samples;
            tablePerformance.Rows[0][1] = (double)stats.Accuracy;
            tablePerformance.Rows[0][2] = (double)stats.Precision;
            tablePerformance.Rows[0][3] = (double)stats.Recall;

            //display the confusion matrix on the form
            //dgvPerformance.DataSource = new List<ConfusionMatrix> { confusionMatrix };
            //using new List<ConfusionMatrix> { confusionMatrix }; gets too much information and somehow seems to have the statistics wrong

            //bind the data to the gridview on the form
            //TODO: somehow the confusionmatrix is polluting the data grid view, but I have no idea how the hell it's happening
            //TODO: compute these statistics myself becaues I can't figure out how or why the confusion matrix is getting what it is.
            dgvPerformance.AutoGenerateColumns = true;
            bsPerformance.DataSource = tablePerformance;
            dgvPerformance.DataSource = bsPerformance;
            dgvPerformance.Columns[9].DefaultCellStyle.Format = "N0"; //numsamples
            dgvPerformance.Columns[11].DefaultCellStyle.Format = "N4"; //recall
            dgvPerformance.Columns[12].DefaultCellStyle.Format = "N4"; //precision
            dgvPerformance.Columns[10].Visible = true; //accuracy
            //bsPerformance.ResetBindings(true);
        }

        #endregion //form display

        #region classification

        private void GetRandomTrainingSet(double percent, DataTable data, int[] labels,
                                          ref int[] train, ref int[] labelsTrain, ref int[] test, ref int[] labelsTest) {
            if ((percent <= 0d) || (percent >= 1d)) { return; }

            //randomly pick % of the rows to train with, the rest will be used to classify
            int cutoff = (int)(data.Rows.Count * percent);
            if (cutoff == 0) { return; } //stop because there is no training set
            if (cutoff == data.Rows.Count) { return; } //stop because there is no test set

            var random = new Random();
            int[] sample = Enumerable.Range(0, data.Rows.Count).OrderBy(n => random.Next()).ToArray();
            train = new int[cutoff]; labelsTrain = new int[cutoff];
            test = new int[data.Rows.Count - cutoff];  labelsTest = new int[data.Rows.Count - cutoff];
            for (int c = 0; c < data.Rows.Count; c++) {
                if (c < cutoff) {
                    train[c] = sample[c];
                    labelsTrain[c] = labels[sample[c]];
                } else {
                    test[c - cutoff] = sample[c];
                    labelsTest[c - cutoff] = labels[sample[c]];
                }
            }
        }

        private int[] ExecuteDecisionTree(DataTable data, int[] train, int[] labelsTrain, int[] test) {
            //*** Train the decision tree****
            string[] sourceColumns;
            double[,] sourceMatrix = (data).ToMatrix(out sourceColumns);

            //separate the data into the trainingSet and testSet
            double[][] trainingSet = sourceMatrix.Submatrix(train, 0, data.Columns.Count - 1).ToArray();
            double[][] testSet = sourceMatrix.Submatrix(test, 0, data.Columns.Count - 1).ToArray();

            //make a decision variable for each dimension of the data
            DecisionVariable[] attributes = new DecisionVariable[data.Columns.Count];
            for(int i=0;i<data.Columns.Count;i++){
                attributes[i] = new DecisionVariable(sourceColumns[i], DecisionVariableKind.Continuous);
            }

            DecisionTree tree = new DecisionTree(attributes, sourceColumns.Length); // Create the Decision tree
            C45Learning c45 = new C45Learning(tree); // Creates a new instance of the C4.5 learning algorithm
            double error = c45.Run(trainingSet, labelsTrain); // Learn the decision tree

            //*** Use the decision tree to predict against the test set ***
            int[] answers = new int[test.Length];
            for (int i = 0; i < test.Length; i++) { answers[i] = tree.Compute(testSet[i]); }
            return answers;
        }

        //Link boolean values to integer values in a standarized way
        //music is true and will map to 1
        //speech is false and will map to 0
        private int MapBool(bool b) {
            if (b) { return 1; }
            return 0;
        }

        //Link boolean values to integer values in a standarized way
        private bool MapBool(int b) {
            if (b == 1) { return true; }
            return false;
        }

        #endregion //classification
    }
}
