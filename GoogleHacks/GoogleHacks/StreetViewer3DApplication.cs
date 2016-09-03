/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using X3D.Runtime;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using X3D;
using X3D.Engine;

namespace GoogleHacks
{
    public class StreetViewer3DApplication : GameWindow
    {

        #region Public Static Fields

        public static float playerDirectionMagnitude = 0.1f;
        public static float movementSpeed = 1.0f;

        #endregion

        #region Private Static Fields

        private SceneCamera ActiveCamera;

        private static AutoResetEvent closureEvent;
        private static Vector4 black = new Vector4(0.0f, 0.0f, 0.0f, 1.0f); // Black
        private static Vector4 white = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // White
        private static Vector4 ClearColor = white;

        private bool fastFlySpeed = false;
        private bool slowFlySpeed = false;
        private bool isFullscreen = false;
        private bool? lockMouseCursor = true;

        #endregion

        #region Public Static Methods

        public static void LoadVR()
        {
            closureEvent = new AutoResetEvent(false);

            Task.Run(LoadVRAsync);

            closureEvent.WaitOne();
        }

        public static async Task LoadVRAsync()
        {
            VSyncMode vsync;
            StreetViewer3DApplication app;

#if VSYNC_OFF
            vsync = VSyncMode.Off;
#else
            vsync = VSyncMode.On;
#endif

            await Task.Run(() =>
            {
                app = new StreetViewer3DApplication(vsync, Resolution.Size800x600, new GraphicsMode(32, 16, 0, 4));
                app.Title = "Initilising..";
#if VSYNC_OFF
                app.Run();
#else
                app.Run(60);
#endif
            });
        }

        #endregion

        #region Constructors

        public StreetViewer3DApplication(VSyncMode VSync, Resolution res, GraphicsMode mode) : base(res.Width, res.Height, mode)
        {
            this.VSync = VSync;

            // Should really use X3D Viewpoint
            
            ActiveCamera = new SceneCamera(this.Width, this.Height);
            ActiveCamera.Position = new Vector3(-1.175851f, -0.8195555f, 3.945387f);
            ActiveCamera.Orientation = new Quaternion(0.001025718f, 0.9989654f, -0.02997796f, -0.03418033f);
            ActiveCamera.camera_pitch = -0.0600000024f;
            ActiveCamera.camera_roll = 0.0f;
            ActiveCamera.camera_yaw = 3.20999742f;

            this.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
        }

        #endregion

        #region Rendering Methods

        protected void Restart()
        {
            Panorama.Unload();
            Panorama.Initilize(ActiveCamera);
            Panorama.Reset(ActiveCamera);
        }

        protected override void OnLoad(EventArgs e)
        {
            Console.WriteLine("LOAD <3d street viewer> ");
            Console.Title = "GoogleHacks [Streetview 3D] - © 2016 Gerallt Franke";
            int[] t = new int[2];
            GL.GetInteger(GetPName.MajorVersion, out t[0]);
            GL.GetInteger(GetPName.MinorVersion, out t[1]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("OpenGL Version " + t[0] + "." + t[1]);

            // Initilise OpenGL
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            GL.ClearDepth(1.0f);                                            // Depth Buffer Setup
            GL.Enable(EnableCap.DepthTest);                                 // Enables Depth Testing
            GL.DepthFunc(DepthFunction.Lequal);                             // The Type Of Depth Testing To Do
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); // Really Nice Perspective Calculations

            Panorama.Initilize(ActiveCamera);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.PointSize(6.0f);

            RenderingContext rc = new RenderingContext();
            rc.View = View.CreateViewFromWindow(this);
            rc.Time = e.Time;
            rc.matricies.worldview = Matrix4.Identity;
            rc.matricies.projection = ActiveCamera.Projection;

            rc.matricies.modelview = Matrix4.Identity;
            rc.matricies.orientation = Quaternion.Identity;
            rc.cam = ActiveCamera;
            rc.Keyboard = this.Keyboard;

  
            //Console.WriteLine(rc.cam.camera_pitch + ", " + rc.cam.camera_yaw);

            if((rc.cam.Position - Panorama.LastPosition).Length > 2.0f)
            {
                // there is a substantial enough of a change from the previous location 
                Panorama.Move(rc.cam.Direction, rc.cam.Position);
            }

            Panorama.Render(rc);

            

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            ActiveCamera.ApplyViewport(Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            ApplyKeyBindings(e);

        }

        #endregion

        #region User Input

        private void ApplyKeyBindings(FrameEventArgs e)
        {
            Vector3 direction = Vector3.Zero;
            bool rotated = false;
            bool translated = false;

            slowFlySpeed = Keyboard[Key.AltLeft];
            fastFlySpeed = Keyboard[Key.ShiftLeft];
            movementSpeed = fastFlySpeed ? 10.0f : 1.0f;
            movementSpeed = slowFlySpeed ? 0.01f : movementSpeed;



            // Calibrator (for translation debugging)
            if (Keyboard[Key.Number1])
            {
                ActiveCamera.calibTrans.X += ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number2])
            {
                ActiveCamera.calibTrans.X -= ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number3])
            {
                ActiveCamera.calibTrans.Y += ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number4])
            {
                ActiveCamera.calibTrans.Y -= ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number5])
            {
                ActiveCamera.calibTrans.Z += ActiveCamera.calibSpeed.Z;
            }
            if (Keyboard[Key.Number6])
            {
                ActiveCamera.calibTrans.Z -= ActiveCamera.calibSpeed.Z;
            }

            // Calibrator (for orientation debugging)
            if (Keyboard[Key.Number6])
            {
                ActiveCamera.calibOrient.X += ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number7])
            {
                ActiveCamera.calibOrient.X -= ActiveCamera.calibSpeed.X;
            }
            if (Keyboard[Key.Number8])
            {
                ActiveCamera.calibOrient.Y += ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Number9])
            {
                ActiveCamera.calibOrient.Y -= ActiveCamera.calibSpeed.Y;
            }
            if (Keyboard[Key.Minus])
            {
                ActiveCamera.calibOrient.Z += ActiveCamera.calibSpeed.Z;
            }
            if (Keyboard[Key.Plus])
            {
                ActiveCamera.calibOrient.Z -= ActiveCamera.calibSpeed.Z;
            }



            if (Keyboard[Key.Escape] || Keyboard[Key.Q])
            {
                // QUIT APPLICATION
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                }

                //X3DProgram.Quit();
                System.Diagnostics.Process.GetCurrentProcess().Kill(); // Fast !

            }

            if (Keyboard[Key.R])
            {
                // RESET CAMERA POSITION+ORIENTATION
                ActiveCamera.Reset();
            }

            if (NavigationInfo.NavigationType != NavigationType.Examine)
            {
                if (Keyboard[Key.T])
                {
                    ActiveCamera.Fly(playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }
                if (Keyboard[Key.G])
                {
                    ActiveCamera.Fly(-playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }

                if (Keyboard[Key.W])
                {
                    ActiveCamera.Walk(playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }
                if (Keyboard[Key.S])
                {
                    ActiveCamera.Walk(-playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }
                if (Keyboard[Key.A])
                {
                    ActiveCamera.Strafe(playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }
                if (Keyboard[Key.D])
                {
                    ActiveCamera.Strafe(-playerDirectionMagnitude * movementSpeed);
                    translated = true;
                }

                #region G.3 Emulate pointing device Key Bindings

                if (Keyboard[Key.Left])
                {
                    ActiveCamera.ApplyYaw(-playerDirectionMagnitude * 0.3f);

                    rotated = true;
                }
                if (Keyboard[Key.Right])
                {
                    ActiveCamera.ApplyYaw(playerDirectionMagnitude * 0.3f);

                    rotated = true;
                }
                if (Keyboard[Key.Up])
                {
                    ActiveCamera.ApplyPitch(-playerDirectionMagnitude * 0.3f);
                    rotated = true;
                }
                if (Keyboard[Key.Down])
                {
                    ActiveCamera.ApplyPitch(playerDirectionMagnitude * 0.3f);

                    rotated = true;
                }

                if (Keyboard[Key.Number0])
                {
                    ActiveCamera.ApplyRoll(-0.1f);
                    rotated = true;
                }
                if (Keyboard[Key.Number9])
                {
                    ActiveCamera.ApplyRoll(0.1f);
                    rotated = true;
                }

                #endregion
            }

            if (rotated)
            {
                ActiveCamera.ApplyRotation();
            }
        }

        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:

                    Restart();

                    break;
                case Key.F:
                    if (this.WindowState == OpenTK.WindowState.Normal)
                    {
                        this.WindowState = WindowState.Fullscreen;
                        lockMouseCursor = true;
                    }
                    else
                    {
                        this.WindowState = WindowState.Normal;
                        lockMouseCursor = false;
                    }

                    isFullscreen = !isFullscreen;

                    ToggleCursor();

                    break;
                case Key.O:

                    NavigationInfo.HeadlightEnabled = !NavigationInfo.HeadlightEnabled;

                    break;
            }
        }


        #endregion

        #region WINDOWS PLATFORM

        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        private static bool systemCursorVisible = true;

        public void ToggleCursor()
        {
            if (systemCursorVisible)
            {
                ShowCursor(false);
            }
            else
            {
                ShowCursor(true);
            }
            systemCursorVisible = !systemCursorVisible;
        }

        #endregion
    }
}