using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Pingu;
using Pingu.Chunks;
using Pingu.Colors;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Graphics = UnityEngine.Graphics;

namespace Shaders
{
    public class PreparePng : MonoBehaviour
    {
        private static readonly bool CanUseBitmap;

        static PreparePng()
        {
            try
            {
                using (new Bitmap(2,2))
                {
                    CanUseBitmap = true;
                }
            }
            catch (Exception)
            {
                CanUseBitmap = false;
            }
            Debug.LogFormat("Can use bitmap: {0}", CanUseBitmap);
        }

        private class RenderWorker
        {
            private bool _enabled = true;

            private ImageData _data;
            readonly AutoResetEvent _start = new AutoResetEvent(false);
            readonly AutoResetEvent _done = new AutoResetEvent(false);

            public RenderWorker()
            {
                var thread = new Thread(Worker) { IsBackground = true };
                thread.Start();
            }

            public void Set(ImageData data)
            {
                _start.WaitOne();
                if (!_enabled)
                {
                    return;
                }
                _data = data;
                _done.Set();
            }

            private void Worker()
            {
                while (_enabled)
                {
                    _start.Set();
                    _done.WaitOne();
                    if (!_enabled)
                    {
                        return;
                    }
                    try
                    {
                        var data = _data;
                        var path = Path.Combine(PrerenderSequencer.OutDir,
                            "img" + data.CurrentFrame.ToString("D5") + ".png");
                        data.CurrentFrame++;

                        if (CanUseBitmap)
                        {
                            using (var bmp = new Bitmap(data.Width, data.Height, PixelFormat.Format24bppRgb))
                            {
                                var bits = bmp.LockBits(new Rectangle(0, 0, data.Width, data.Height),
                                    ImageLockMode.ReadWrite,
                                    PixelFormat.Format24bppRgb);
                                if (bmp.Width%4 == 0)
                                {
                                    Marshal.Copy(data.Data, 0, bits.Scan0, data.Data.Length);
                                }
                                else
                                {
                                    for (int h = 0; h < bits.Height; h++)
                                    {
                                        Marshal.Copy(data.Data, h*bits.Width*3,
                                            new IntPtr(bits.Scan0.ToInt64() + h*bits.Stride), bits.Width*3);
                                    }
                                }
                                //Debug.Log(string.Format("W: {0}, H: {1}, S: {2}, D: {3}, DW: {4}", bits.Width, bits.Height, bits.Stride, data.Width, data.Data.Length/bits.Height));
                                bmp.UnlockBits(bits);
                                bmp.Save(path, ImageFormat.Png);
                            }
                        }
                        else
                        {
                            var header = new IhdrChunk(data.Width, data.Height, 8, ColorType.Truecolor);
                            var idat = new IdatChunk(header, data.Data);
                            var end = new IendChunk();
                            var pngFile = new PngFile() {header, idat, end};
                            using (var fs = new FileStream(path, FileMode.Create))
                            {
                                pngFile.WriteFileAsync(fs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            public void Disable()
            {
                _enabled = false;
                _done.Set();
            }
        }


        private class ImageData
        {
            public int Width;
            public int Height;
            public byte[] Data;
            public int CurrentFrame;

            public ImageData(int width, int height, int currentframe)
            {
                Width = width;
                Height = height;
                CurrentFrame = currentframe;
            }
        }

        public bool Enabled = true;
        private Material _material;
        private int _currentFrame;

        private int _lastWidth;
        private int _lastHeight;

        private RenderWorker[] _workers;
        private int _index;

        void Start()
        {
            _material = new Material(Shader.Find(CanUseBitmap ? "Hidden/PreparePng" : "Hidden/FlipImage"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            Resize();
            _currentFrame = 0;
            _workers = GetWorkers(GetOptimalWorkersCount());
            _index = 0;
        }

        private void Resize()
        {
            var newWidth = Screen.width;
            var newHeight = Screen.height;
            if (newWidth == _lastWidth && newHeight == _lastHeight)
            {
                return;
            }
            _lastWidth = newWidth;
            _lastHeight = newHeight;
            _tex.Resize(newWidth, newHeight);
        }

        private static RenderWorker[] GetWorkers(int count)
        {
            var workers = new RenderWorker[count];
            for (int i = 0; i < count; i++)
            {
                workers[i] = new RenderWorker();
            }
            return workers;
        }

        private static int GetOptimalWorkersCount()
        {
            return (int)Math.Ceiling((double)Environment.ProcessorCount / 2);
        }

        void OnDestroy()
        {
            if(_workers!= null)
            {
                foreach (var renderWorker in _workers)
                {
                    renderWorker.Disable();
                }
            }
        }

        private Texture2D _tex;
        //private Stopwatch _renderSw = new Stopwatch();
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(Enabled)
            {
                
                //_renderSw.Reset();
                //_renderSw.Start();
                Resize();
                var data = new ImageData(_lastWidth, _lastHeight, _currentFrame);
                Graphics.Blit(source, destination, _material);
                _tex.ReadPixels(new Rect(0,0,data.Width, data.Height), 0,0);
                data.Data = _tex.GetRawTextureData();
                //_renderSw.Stop();
                //Debug.Log("RENDER: " + _renderSw.ElapsedMilliseconds);

                _workers[_index].Set(data);
                _index = (_index + 1)%_workers.Length;

                _currentFrame++;
            }
            Graphics.Blit(source, destination);
            
        }

    }
}
