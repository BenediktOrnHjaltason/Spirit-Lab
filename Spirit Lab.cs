﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spirit_Studio.CustomTypes;
using System.Diagnostics;
using Spirit_Studio.Utilities;
using System.IO;
using System.Drawing.Imaging;

namespace Spirit_Studio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            btnStartPhotoshoot.Enabled = false;

            lblRefImageNotifier.Visible =
            lblRefImageCountdown.Visible =
            lblDiffPercentage.Visible =
            lblSavedToFile.Visible = false;

            UpdateTrackBarThresholdLabel();
        }

        private PhotoShoot photoShoot = new PhotoShoot();

        #region Photo shoot

        private void btnGetCameras_Click(object sender, EventArgs e)
        {
            foreach (var cameraName in photoShoot.GetCameras())
                cboCamera.Items.Add(cameraName);

            btnStartPhotoshoot.Enabled = cboCamera.Items.Count > 0;

            cboCamera.SelectedIndex = 0;
        }

        private bool photoShootRunning = false;
        private void OnClick_Start(object sender, MouseEventArgs e)
        {
            if (photoShootRunning == false)
            {
                btnStartPhotoshoot.Text = "Stop after\nnext";

                photoShootRunning = true;

                lblRefImageNotifier.Visible = true;
                lblRefImageCountdown.Visible = true;

                photoShoot.Initialize(cboCamera.SelectedIndex);
                RunPhotoShoot();
            }

            else
            {
                photoShootRunning = false;
                btnStartPhotoshoot.Text = "Start";
                lblRefImageNotifier.Text = "Saving reference image in";
                lblRefImageNotifier.Visible = false;
                lblRefImageCountdown.Text = "5";
                lblRefImageCountdown.Visible = false;
                lblDiffPercentage.Visible = false;
            }
        }

        private async void RunPhotoShoot()
        {
            await CountDown(5);

            picReference.Image =  Utils.ResizeImage(photoShoot.TakeReferenceImage(), new Size(picReference.Width, picReference.Height));

            lblRefImageNotifier.Text = "Next image in";

            while (photoShootRunning)
            {
                await CountDown(5);

                PhotoshootResult result = photoShoot.TakeSpiritImage();

                lblDiffPercentage.Visible = true;
                lblDiffPercentage.Text = $"Difference:\n{result.DifferencePercentage.ToString("0.000")}%";

                picNewImage.Image = Utils.ResizeImage(result.NewImage, new Size(picNewImage.Width, picNewImage.Height));
                picCamera.Image = Utils.ResizeImage(result.ProcessedImage, new Size(picCamera.Width, picCamera.Height));

                if (result.DifferencePercentage > trackBarSaveFileThreshold.Value)
                {
                    lblSavedToFile.Visible = true;
                    result.NewImage.Save(Path.Combine("C:/ProgramData/Spirit Lab/PhotoShoot", $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.bmp"), ImageFormat.Bmp);
                }
                else lblSavedToFile.Visible = false;
            }

            picCamera.Image = null;
            picReference.Image = null;
            picNewImage.Image = null;
        }

        private async Task CountDown(int duration)
        {
            int countDownIterator = duration;

            while (countDownIterator > 0)
            {
                lblRefImageCountdown.Text = countDownIterator.ToString();

                await Task.Delay(1000);

                countDownIterator--;
            }
        }

        private void trackBarSaveFileThreshold_Scroll(object sender, EventArgs e)
        {
            UpdateTrackBarThresholdLabel();
        }

        private void UpdateTrackBarThresholdLabel()
        {
            lblTrackBarFileSave.Text = $"Change percentage required to save new image: {trackBarSaveFileThreshold.Value}%";
        }

        #endregion

        #region RandomNumbers

        private bool randomNumbersRunning = false;
        private Random random = new Random();
        private int lastGeneratedValue;
        private int sumGeneratedZeros = 0;
        private int sumGeneratedOnes = 0;

        private List<int> generatedValuesQueue = new List<int>();

        private void btnRandomStartStop_Click(object sender, EventArgs e)
        {
            randomNumbersRunning = !randomNumbersRunning;

            if (randomNumbersRunning)
            {
                btnRandomStartStop.Text = "Stop";
                GenerateRandomNumbers();
            }
            else
            {
                btnRandomStartStop.Text = "Start";
                labelValue.Text = "#";

                sumGeneratedOnes = sumGeneratedZeros = 0;
                labelOnesPercentage.Text = labelZerosPercentages.Text = labelOnesTotal.Text = labelZerosTotal.Text = "0";
            }
        }

        private async void GenerateRandomNumbers()
        {
            while(randomNumbersRunning)
            {
                lastGeneratedValue = random.Next(0, 2);

                labelValue.Text = lastGeneratedValue.ToString();

                if (lastGeneratedValue == 0)
                    sumGeneratedZeros++;
                else if (lastGeneratedValue == 1)
                    sumGeneratedOnes++;

                labelOnesTotal.Text = sumGeneratedOnes.ToString();
                labelZerosTotal.Text = sumGeneratedZeros.ToString();

                labelZerosPercentages.Text = (((float)sumGeneratedZeros / ((float)sumGeneratedZeros + (float)sumGeneratedOnes)) * 100.0f).ToString("0.0");
                labelOnesPercentage.Text = (100.0f - float.Parse(labelZerosPercentages.Text)).ToString();

                await Task.Delay(50);
            }
        }

        #endregion
    }
}
