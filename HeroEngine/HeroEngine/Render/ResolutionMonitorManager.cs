﻿//////////////////////////////////////////////////////////////////////////
////License:  The MIT License (MIT)
////Copyright (c) 2010 David Amador (http://www.david-amador.com)
////
////Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
////
////The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
////
////THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeroEngine.Render
{
    //TODO Cleanup and redo this class RESOLUTIONMANAGER
    public class ResolutionMonitorManager
    {
        private GraphicsDeviceManager _Device = null;

        public int Width = 800;
        public int Height = 600;
        public bool FullScreen = false;
        private int _VWidth = 1024;
        private int _VHeight = 768;
        private Matrix _ScaleMatrix;
        private bool _dirtyMatrix = true;

        public void Init(ref GraphicsDeviceManager device)
        {
            Width = device.PreferredBackBufferWidth;
            Height = device.PreferredBackBufferHeight;
            _Device = device;
            _dirtyMatrix = true;
            ApplyResolutionSettings();
        }


        public Matrix getTransformationMatrix()
        {
            if (_dirtyMatrix) RecreateScaleMatrix();
            
            return _ScaleMatrix;
        }

        public void SetResolution(int Width, int Height, bool FullScreen)
        {
            this.Width = Width;
            this.Height = Height;

            this.FullScreen = FullScreen;

           ApplyResolutionSettings();
        }

        public void SetVirtualResolution(int Width, int Height)
        {
            _VWidth = Width;
            _VHeight = Height;

            _dirtyMatrix = true;
        }

        public void ApplyResolutionSettings()
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (FullScreen == false)
            {
                if ((Width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (Height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    _Device.PreferredBackBufferWidth = Width;
                    _Device.PreferredBackBufferHeight = Height;
                    _Device.IsFullScreen = FullScreen;
                    _Device.ApplyChanges();
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate through the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == Width) && (dm.Height == Height))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        _Device.PreferredBackBufferWidth = Width;
                        _Device.PreferredBackBufferHeight = Height;
                        _Device.IsFullScreen = FullScreen;
                        _Device.ApplyChanges();
                    }
                }
            }

            _dirtyMatrix = true;

            Width = _Device.PreferredBackBufferWidth;
            Height = _Device.PreferredBackBufferHeight;
        }

        /// <summary>
        /// Sets the device to use the draw pump
        /// Sets correct aspect ratio
        /// </summary>
        public void BeginDraw()
        {
            // Start by reseting viewport to (0,0,1,1)
            FullViewport();
            // Clear to Black
            _Device.GraphicsDevice.Clear(Color.Black);
            // Calculate Proper Viewport according to Aspect Ratio
            ResetViewport();
            // and clear that
            // This way we are gonna have black bars if aspect ratio requires it and
            // the clear color on the rest
            _Device.GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        private void RecreateScaleMatrix()
        {
            _dirtyMatrix = false;
            _ScaleMatrix = Matrix.CreateScale(
                           (float)_Device.GraphicsDevice.Viewport.Width / _VWidth,
                           (float)_Device.GraphicsDevice.Viewport.Width / _VWidth,
                           1f);
        }


        public void FullViewport()
        {
            Viewport vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = Width;
            vp.Height = Height;
            _Device.GraphicsDevice.Viewport = vp;
        }

        /// <summary>
        /// Get virtual Mode Aspect Ratio
        /// </summary>
        /// <returns>aspect ratio</returns>
        public float getVirtualAspectRatio()
        {
            return (float)_VWidth / (float)_VHeight;
        }

        public void ResetViewport()
        {
            float targetAspectRatio = getVirtualAspectRatio();
            // figure out the largest area that fits in this resolution at the desired aspect ratio
            int width = _Device.PreferredBackBufferWidth;
            int height = (int)(width / targetAspectRatio + .5f);
            bool changed = false;
            
            if (height > _Device.PreferredBackBufferHeight)
            {
                height = _Device.PreferredBackBufferHeight;
                // PillarBox
                width = (int)(height * targetAspectRatio + .5f);
                changed = true;
            }

            // set up the new viewport centered in the backbuffer
            Viewport viewport = new Viewport();

            viewport.X = (_Device.PreferredBackBufferWidth / 2) - (width / 2);
            viewport.Y = (_Device.PreferredBackBufferHeight / 2) - (height / 2);
            viewport.Width = width;
            viewport.Height = height;
            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;

            if (changed)
            {
                _dirtyMatrix = true;
            }

            _Device.GraphicsDevice.Viewport = viewport;
        }

    }
}
