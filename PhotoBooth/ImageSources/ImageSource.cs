﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using SpiritLab.CustomTypes;

namespace SpiritLab
{
    public interface IImageSource
    {
        void Initialize();

        void Dispose();

        List<string> GetImageSourceNames();

        void SetActiveSource(string name);

        Task<Bitmap> TakeStillImage(ImagePurpose purpose);

        void SaveToPositiveResults();

        void DeleteComparison();

        void StartLiveView(Action<Bitmap> OnLiveViewReceived);

        void StopLiveView();

        void Close();
    }
}
