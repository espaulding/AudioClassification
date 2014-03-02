using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.Audio.Filters;
using Accord.Math;
using Accord.Audio.Windows;

namespace AudioClassification {
    public class Sound {
        public static string[] featureNames = new string[] {"Zerocrossing","Low Band Energy","High Band Energy","Brightness"};
        public string filename;
        public string fullname;
        public bool isMusic;
        public List<double> features = new List<double>();

        //Constructor
        public Sound(string filename, string fullname) {
            this.filename = filename;
            this.fullname = fullname;
        }

        public Sound(string filename, string fullname, bool isMusic) {
            this.filename = filename;
            this.fullname = fullname;
            this.isMusic = isMusic;
        }

        public override string ToString() {
            string output = "";
            if (isMusic) {
                output += "music";
            } else {
                output += "speech";
            }
            output += " - " + filename;
            return output;
        }


        //see http://en.wikipedia.org/wiki/IEEE_floating_point for further information
        //signals contain a series of floating point samples (the wav format tested with this softwware is in 32bit ieee floats)
        //Name  	Common      name 	    Base 	Digits  E min 	E max 	Decimal Decimal
        //                                                              digits 	E max
        //binary16 	Half        precision 	2 	    10+1    −14 	+15 	3.31 	4.51
        //binary32 	Single      precision 	2 	    23+1 	−126 	+127 	7.22 	38.23
        //binary64 	Double      precision 	2 	    52+1 	−1022 	+1023 	15.95 	307.95
        //binary128 Quadruple   precision   2 	    112+1 	−16382 	+16383 	34.02 	4931.77

        //Decimal digits is digits × log10 base, this gives an approximate precision in decimal.
        //Decimal E max is Emax × log10 base, this gives the maximum exponent in decimal.
        #region feature extraction

        //analyze the sound file that is being referenced by the sound object and collect structural information about the sound file
        //the files were recorded at wildly inconsistent volume
        //TODO: is there some way to normalize the volume?
        public void ExtractFeatures() {
            features.Clear(); //dump any old data

            //decode the wave file into a signal
            WaveDecoder decoder = new WaveDecoder();
            decoder.Open(fullname);
            Signal decodedSignal = decoder.Decode();
            //decodedSignal = CopySignal(decodedSignal, (int)Math.Pow(2, 14)); //must cut signal to a power of 2

            //***************Get the time domain of the signal********************
            float[] time = null, amplitude = null;
            GetTimeDomain(ref decodedSignal, ref time, ref amplitude);
            float[] decibel = AmplitudeToDecibel(amplitude);
            float maxdb = decibel.Max();
            float mindb = decibel.Where(e => !float.IsNaN(e) && !float.IsNegativeInfinity(e)).Min();

            //TODO: confirm that this matches min's average energy algorithm
            //confirmed that either it doesn't or I'm using the wrong amplitudes
            double energyOriginal = decodedSignal.GetEnergy(); //indicates the loudness of the signal

            double energyAmp = CalculateAverageEnergy(amplitude);
            float maxAmp = amplitude.Max();
            float minAmp = amplitude.Min();

            double zerocrossings = CalculateZeroCrossings(amplitude);

            //speech is typically below 7 kHz
            double cutoff = 7000; //CalculateBandwidth(frequency); //always returns 8000 which is samplerate/2
            double RC = 1d / (2d * Math.PI * cutoff);
            double RC8 = 240000 * 82 * Math.Pow(10, -12); //matches up with cutoff of 8087 Hz
            double dt = 1d / decodedSignal.SampleRate;
            //double dt = (double)decodedSignal.Duration / 1000 / decodedSignal.Samples; //dt = duration(ms) / number of samples.
            float alpha = (float)(dt / (dt + RC));

            LowPassFilter low = new LowPassFilter(alpha);
            Signal lowSignal = low.Apply(decodedSignal);
            //double energyLow = lowSignal.GetEnergy();
            //float[] l = lowSignal.ToFloat();
            double energyLow = CalculateAverageEnergy(lowSignal.ToFloat());

            HighPassFilter high = new HighPassFilter(alpha);
            Signal highSignal = high.Apply(decodedSignal);
            //double energyHigh = highSignal.GetEnergy();
            //float[] h = highSignal.ToFloat();
            double energyHigh = CalculateAverageEnergy(highSignal.ToFloat());

            //***************get the frequency domain of the signal*****************
            //TODO: make sure this has vectors with the same length as time
            double[] frequency = null, magnitude = null, phase = null, power1 = null, power2 = null;
            double[] energyf = GetFrequencyDomain(decodedSignal, ref frequency, ref magnitude, ref phase, ref power1, ref power2);

            double power1max = power1.Max();
            double power1min = power1.Min();
            double brightness = CalculateSpectralCentroid(frequency, magnitude);

            features.Add(zerocrossings); //expect speech to have more zero crossings
            features.Add(energyLow);     //expect speech to have more energy below 7 kHz
            features.Add(energyHigh);    //expect music to have more energy above 7 kHz
            features.Add(brightness);    //expect speech to be lower and music to be higher
        }

        #region signal domains

        //***time domain (time-amplitude representation)***
        //Amplitude has an audible range of 0 to 100 or 120dB(decibels) for humans
        //In a wav file the values are normalized on the range [-1,1]
        //Negative amplitudes do exist, but are not audible to humans
        //typically visualized with time on x-axis and amplitude on y-axis 
        private void GetTimeDomain(ref Signal signal, ref float[] time, ref float[] amplitude) {
            //Signal.ToFloat: Converts a signal into a array of single precision samples.
            amplitude = signal.ToFloat();

            time = new float[amplitude.Length];
            float deltaTime = (float)signal.Duration / signal.Samples; //dt = duration(ms) / number of samples.
            for (int c = 0; c < signal.Samples; c++) { time[c] = (c + 1) * deltaTime; }
        }

        //***frequency domain (frequency-magnitude representation)***
        //Frequency has an audible range of 20 to 20,000 Hz for humans
        //typically visualized with frequency(Hz) on x-axis and magnitude(dB) on y-axis
        private double[] GetFrequencyDomain(Signal signal, ref double[] frequency, ref double[] magnitude, ref double[] phase, ref double[] power1, ref double[] power2) {
            //TODO: use Hamming window here to get a better ComplexSignal object that is the same length as the signal

            //Create Hamming window allowing the window to be processed in chunks that are sized by powers of 2    
            //RaisedCosineWindow window = RaisedCosineWindow.Hamming((int)Math.Pow(2,9));

            //signal = window.Apply(signal, 0);
            //signal.ForwardFourierTransform();
            //AForge.Math.Complex[] ch = s.GetChannel(0); //get signal FFT elements

            //Note that this will result in overlapped windows.
            //Signal[] windows = signal.Split(window, 256); //Split requires signal to already be a ComplexSignal which kinda defeats the point here
            //ComplexSignal[] complex = windows.Apply(ComplexSignal.FromSignal,0);
            //complex.ForwardFourierTransform();

            //TODO: delete OLD CODE when Hamming window is working
            RaisedCosineWindow window = RaisedCosineWindow.Hamming((int)Math.Pow(2, 14));
            Signal signalSlice = CopySignal(signal, (int)Math.Pow(2, 14)); //must cut signal to a power of 2
            ComplexSignal fftSignal = signalSlice.ToComplex();             //convert to complex signal for FFT
            fftSignal = window.Apply(fftSignal, 0);

            fftSignal.ForwardFourierTransform();
            AForge.Math.Complex[] channel = fftSignal.GetChannel(0); //get signal FFT elements
 
            frequency = Accord.Audio.Tools.GetFrequencyVector(fftSignal.Length, fftSignal.SampleRate);
            magnitude = Accord.Audio.Tools.GetMagnitudeSpectrum(channel);
            phase     = Accord.Audio.Tools.GetPhaseSpectrum(channel);
            power1    = Accord.Audio.Tools.GetPowerCepstrum(channel); //squared magnitude of the inverse Fourier transform of the logarithm of the squared magnitude of the Fourier transform of a signal
            power2    = Accord.Audio.Tools.GetPowerSpectrum(channel);

            //do i need to scale here? if so how do i do it.
            //magnitude = magnitude.Select(e => e*scale).ToArray();
            double maxMag = magnitude.Max();
            double minMag = magnitude.Min();

            double[] result = new double[] { signalSlice.GetEnergy(), fftSignal.GetEnergy() };
            return result;
        }

        #endregion //signal domains

        #region feature algorithms

        //the energy of a sample is the value squared, so the average
        //energy is the sum of the squared values divided by the number of values
        private double CalculateAverageEnergy(float[] amplitude) {
            //short[] amp = new short[amplitude.Length];
            //SampleConverter.Convert(amplitude, amp);

            int count = amplitude.Length;
            amplitude = AmplitudeToDecibel(amplitude.Where(a => a>0).ToArray());

            float sum = 0;
            for (int c = 0; c < amplitude.Length; c++) {
                if (!float.IsNaN(amplitude[c]) && !float.IsNegativeInfinity(amplitude[c])) {
                    sum += (float)Math.Pow((double)amplitude[c]+1,2);
                }
            }
            return sum / count;
        }

        //also called brightness, derived from energy distribution
        private double CalculateSpectralCentroid(double[] frequency, double[] magnitude) {
            double numerator = 0, denominator = 0;
            for (int c = 0; c < frequency.Length; c++) {
                numerator += frequency[c] * magnitude[c];
                denominator += magnitude[c];
            }
            return numerator/denominator;
        }

        //find the magnitude of the frequency range
        private double CalculateBandwidth(double[] frequency) {
            return frequency.Max() - frequency.Min();
        }

        //count the number of times that the zmplitude crosses the x-axis. 
        //i.e. switches from positive to negative or negative to positive
        private double CalculateZeroCrossings(float[] amplitude) {
            int[] signs = amplitude.Select(e => Math.Sign(e)).Where(e => e!=0).ToArray();
            double numerator = 0;

            for (int c = 1; c < signs.Length; c++) {
                numerator += Math.Abs(signs[c] - signs[c-1]);
            }
            return numerator / (2 * signs.Length);
        }

        #endregion //feature algorithms

        #region helper functions

        //private float AmplitudeToDecibel(short amplitude) {
        //    if (amplitude > 0) {
        //        return 20f * (float)Math.Log10(amplitude);
        //    }
        //    return -340; //floor our decibels at .00000000000000001
        //}

        //private float[] AmplitudeToDecibel(short[] amplitude) {
        //    return amplitude.Select(e => 20f * (float)Math.Log10(e)).ToArray();
        //}

        private float AmplitudeToDecibel(float amplitude) {
            //short amp = 0;
            //SampleConverter.Convert(amplitude, out amp);
            //return 20f * (float)Math.Log10(amp);
            return 20f * (float)Math.Log10(amplitude);
        }

        private float[] AmplitudeToDecibel(float[] amplitude) {
            //short[] amp = new short[amplitude.Length];
            //SampleConverter.Convert(amplitude, amp); //can convert from 32-bit float to 16-bit int
            //return amp.Select(e => 20f * (float)Math.Log10(e)).ToArray();
            return amplitude.Select(e => 20f * (float)Math.Log10(e)).ToArray();
        }

        //returns a new Signal object of size lengthToCopy
        //signal       : the signal to copy from
        //lengthToCopy : how much of signal to use when copying
        //               if lengthToCopy is greater than the signal's length
        //               the function will return null
        private Signal CopySignal(Signal signal, int lengthToCopy) {
            if (lengthToCopy > signal.Length) { return null; }
            Signal output;

            switch (signal.SampleFormat) {
                case SampleFormat.Format32BitIeeeFloat: {
                        float[] sigArray = new float[signal.Length], sigSlice = new float[lengthToCopy];
                        signal.CopyTo(sigArray);
                        for (int i = 0; i < lengthToCopy; i++) { sigSlice[i] = sigArray[i]; }
                        output = Signal.FromArray(sigSlice, signal.Channels, signal.SampleRate, signal.SampleFormat);
                        break;
                    }

                case SampleFormat.Format16Bit: {
                        short[] sigArray = new short[signal.Length], sigSlice = new short[lengthToCopy];
                        signal.CopyTo(sigArray);
                        for (int i = 0; i < lengthToCopy; i++) { sigSlice[i] = sigArray[i]; }
                        output = Signal.FromArray(sigSlice, signal.Channels, signal.SampleRate, signal.SampleFormat);
                        break;
                    }

                default:
                        output = null;
                        break;
            }

            return output;
        }

        #endregion //helper functions

        #endregion //feature extraction
    }
}
