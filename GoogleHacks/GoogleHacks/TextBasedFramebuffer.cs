using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL4;
using X3D;
using X3D.Core;
using X3D.Core.Shading.DefaultUniforms;
using X3D.Core.Shading;

namespace GoogleHacks
{
    public class TextBasedFramebuffer
    {

        public void Initilize()
        {
            
        }

        /// <summary>
        /// Pipes an OpenGL rendering; converting the frame buffer to ASCII text, 
        /// and outputting into the System.Console.
        /// </summary>
        /// <param name="rc">
        /// The rendering context.
        /// </param>
        /// <param name="renderBlock">
        /// The rendering method to execute.
        /// </param>
        public void PipeToConsole(RenderingContext rc, Action renderBlock)
        {
            Bitmap frame;
            string frameBuffer;
            int rows;
            int columns;

            frame = CaptureFramebufferAsBitmap(rc, renderBlock);

            ImageTexture.Scale(ref frame, new Size(100, 100));
            frame.RotateFlip(RotateFlipType.Rotate180FlipX);

            //frame.Save("D:\\test-frame2.png");

            frameBuffer = asciiTextImageTranslator(frame, out rows, out columns);

            frame.Dispose();

            Console.Clear();
            //Console.WindowWidth = rows;
            //Console.WindowHeight = columns + 1;
            //Console.BufferWidth = rows + 1;
            //Console.BufferHeight = columns + 1;
            
            Console.Write(frameBuffer);
        }

        public static string asciiTextImageTranslator(Bitmap bmp, out int rows, out int columns)
        {
            System.Drawing.Color col;
            string result;
            StringBuilder frameLine;
            int x;
            int y;
            byte rChannel;
            string ascii;
            int max;

            frameLine = new StringBuilder();
            rows = 0;
            columns = 0;
            max = 0;

            try
            {
                for (y = 0; y < bmp.Height; y++)
                {
                    for (x = 0; x < bmp.Width; x++)
                    {
                        col = bmp.GetPixel(x, y);

                        // Greyscale
                        col = System.Drawing.Color.FromArgb(
                            (col.R + col.G + col.B) / 3,
                            (col.R + col.G + col.B) / 3,
                            (col.R + col.G + col.B) / 3
                        );

                        // Combined into 1 color channel 
                        rChannel = col.R;

                        // Group the red chanel into blocks of pixels and find ascii value for each block
                        if (rChannel >= 230)
                        {
                            ascii = " "; // White
                        }
                        else if (rChannel >= 200)
                        {
                            ascii = "."; // Light grey
                        }
                        else if (rChannel >= 180)
                        {
                            ascii = "*"; // slate gray
                        }
                        else if (rChannel >= 160)
                        {
                            ascii = ":"; // gray
                        }
                        else if (rChannel >= 130)
                        {
                            ascii = "o"; // medium
                        }
                        else if (rChannel >= 100)
                        {
                            ascii = "&"; // medium gray
                        }
                        else if (rChannel >= 70)
                        {
                            ascii = "8"; // dark gray
                        }
                        else if (rChannel >= 50)
                        {
                            ascii = "#"; // charcoal
                        }
                        else
                        {
                            ascii = "@"; // black
                        }

                        frameLine.Append(ascii);

                        rows++;

                        if (x == bmp.Width - 1)
                        {
                            frameLine.Append("\n");
                            columns++;
                            max = rows;
                            rows = 0;
                        }
                            
                    }
                }

                rows = max;
                result = frameLine.ToString();

                return result;
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
            finally
            {
                bmp.Dispose();
            }
        }

        public Bitmap CaptureFramebufferAsBitmap(RenderingContext rc, Action renderBlock)
        {
            return TakeRenderingContextScreenshot(0, 0, 800, 600, renderBlock);
        }

        public Bitmap TakeRenderingContextScreenshot(int x, int y, int width, int height, Action renderingBlock)
        {
            Bitmap b;
            BitmapData bits;
            Rectangle rect;

            int fboWidth = width;
            int fboHeight = height;

            uint fboHandle;
            uint colorTexture;
            uint depthRenderbuffer;

            GL.GenTextures(1, out colorTexture);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, fboWidth, fboHeight, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0); // prevent feedback, reading and writing to the same image is a bad idea
            GL.GenRenderbuffers(1, out depthRenderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)All.DepthComponent32, fboWidth, fboHeight);
            GL.GenFramebuffers(1, out fboHandle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboHandle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0);

            {
                // What is executed in renderingBlock() wont be rendered on screen but in frame buffer object memory.
                renderingBlock();
            }

            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            rect = new Rectangle(0, 0, width, height);

            b = new Bitmap(width, height);
            b.MakeTransparent();
            using (Graphics g2D = Graphics.FromImage(b))
            {
                g2D.Clear(System.Drawing.Color.Black);
            }

            bits = b.LockBits(new Rectangle(0, 0, rect.Width, rect.Height), ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.ReadPixels(rect.Left, rect.Top, rect.Width, rect.Height,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);

            b.UnlockBits(bits);

            // Cleanup GL memory 
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(fboHandle);
            GL.DeleteTexture(colorTexture);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return b;
        }


    }
}
