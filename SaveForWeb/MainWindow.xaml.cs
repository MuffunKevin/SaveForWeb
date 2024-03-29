﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;

//TODO:
// Convert all process into a class
// Use resources for error message
// Addd some parallel processing

//Note: Some code come from: http://msdn.microsoft.com/en-us/library/bb882583.aspx

namespace SaveForWeb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<char, char> _invalidCaraters = new Dictionary<char, char>();
        private Dictionary<string, string> _invalidString = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            _invalidCaraters.Add('â', 'a');
            _invalidCaraters.Add('à', 'a');
            _invalidCaraters.Add('ä', 'a');
            _invalidCaraters.Add('é', 'e');
            _invalidCaraters.Add('è', 'e');
            _invalidCaraters.Add('ë', 'e');
            _invalidCaraters.Add('ê', 'e');
            _invalidCaraters.Add('ç', 'c');
            _invalidCaraters.Add('ï', 'i');
            _invalidCaraters.Add('î', 'i');
            _invalidCaraters.Add('ü', 'u');
            _invalidCaraters.Add('û', 'u');

            _invalidCaraters.Add('Â', 'A');
            _invalidCaraters.Add('À', 'A');
            _invalidCaraters.Add('Ä', 'A');
            _invalidCaraters.Add('É', 'E');
            _invalidCaraters.Add('È', 'E');
            _invalidCaraters.Add('Ë', 'E');
            _invalidCaraters.Add('Ê', 'E');
            _invalidCaraters.Add('Ç', 'C');
            _invalidCaraters.Add('Ï', 'I');
            _invalidCaraters.Add('Î', 'I');
            _invalidCaraters.Add('Ü', 'U');
            _invalidCaraters.Add('Û', 'U');

            _invalidString.Add(" ", "-");
            _invalidString.Add("?", "-");
            _invalidString.Add("'", string.Empty);

        }

        #region Events
        private void btnSaveForWeb_Click(object sender, RoutedEventArgs e)
        {
            bool canProcess = true;
            int maxHeight = 0;
            int maxWidth = 0;
            long quality = 0;

            canProcess = int.TryParse(_txtMaxHeight.Text, out maxHeight);

            if (canProcess)
            {
                canProcess = int.TryParse(_txtMaxWidth.Text, out maxWidth);

                if (!canProcess)
                {
                    System.Windows.MessageBox.Show("Max width must be a valid number.");
                    _txtMaxWidth.Focus();
                }
                else
                {
                    canProcess = long.TryParse(_txtQuality.Text, out quality);

                    if (!canProcess)
                    {
                        MessageBox.Show("Quality must be a valid number");
                    }
                }
            }
            else
            {
                MessageBox.Show("Max height must be a valid number.");
                _txtMaxHeight.Focus();
            }

            if (canProcess)
            {
                foreach (var file in _txtFiles.Text.Split(new[] { ';' }))
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            //Processt the file
                            ProcessTheFile(file, maxWidth, maxHeight, quality);
                        }
                        else if (Directory.Exists(file)) //It may be a folder
                        {
                            ProcessFolder(file, maxWidth, maxHeight, quality);
                        }
                        else
                        {
                            Log(string.Format("Noting found at {0}", file), true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("There was an error while executing the process on {0}", file), true);
                        MessageBox.Show(ex.Message);
                    }
                }
                MessageBox.Show("All files were process");
                _txtFiles.Text = string.Empty;
            }
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Multiselect = true;
            dlg.Filter = "Fichiers image (*.jpg, *.jepg,*.JPG,*.JEPG)|*.jpg;*.jepg;*.JPG;*.JEPG|Tous les fichiers (*.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Open document
                _txtFiles.Text = string.Join(";", dlg.FileNames);
            }
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _txtFiles.Text = dialog.SelectedPath;

            }
        }
        #endregion

        #region Private functions

        private void ProcessFolder(string file, int maxWidth, int maxHeight, long quality)
        {
            Log(string.Format("Folder found at {0}", file), false);
            
            foreach (var item in Directory.GetDirectories(file))
            {
                ProcessFolder(file, maxWidth, maxHeight, quality);
            }

            foreach (var innerFile in Directory.GetFiles(file).Select(f => f.ToLowerInvariant()).Where(f => f.EndsWith(".jpg") || f.EndsWith(".jepg")))
            {
                //process the file
                ProcessTheFile(innerFile, maxWidth, maxHeight, quality);
            }
        }

        private void Log(string message, bool error)
        {
            //_logsCollection.Add(new Log(message, error));

            _txtLogs.Text += message + Environment.NewLine;
        }

        private void ProcessTheFile(string fileName, int maxWidth, int maxHeight, long quality)
        {
            Log(string.Format("File found at {0}", fileName), false);

            var fullsizeImage = System.Drawing.Image.FromFile(fileName);

            double newWidth = maxWidth;
            double newHeight = maxHeight;

            if (fullsizeImage.Width <= maxWidth)
            {
                newWidth = fullsizeImage.Width;
            }

            newHeight = fullsizeImage.Height * newWidth / fullsizeImage.Width;
            if (newHeight > maxHeight)
            {
                // Resize with height instead
                newWidth = fullsizeImage.Width * MaxHeight / fullsizeImage.Height;
                newHeight = maxHeight;
            }

            Bitmap bmp1 = new Bitmap((int)newWidth, (int)newHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)bmp1);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(fullsizeImage, 0, 0, (int)newWidth, (int)newHeight);
            g.Dispose();

            // Clear handle to original file so that we can overwrite it if necessary
            fullsizeImage.Dispose();

            File.Delete(fileName);

            //Change quality

            // Get a bitmap.
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;

            foreach (var invalidCararter in _invalidCaraters)
            {
                fileName = fileName.Replace(invalidCararter.Key, invalidCararter.Value);
            }

            foreach (var invalidCararter in _invalidString)
            {
                fileName = fileName.Replace(invalidCararter.Key, invalidCararter.Value);
            }

            bmp1.Save(fileName, jgpEncoder, myEncoderParameters);

            bmp1.Dispose();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        #endregion
    }
}
