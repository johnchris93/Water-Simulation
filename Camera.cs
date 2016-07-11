using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Water_Simulation
{
    public class Camera : GameComponent
    {
        private Vector3 _cameraPos;
        private Vector3 _cameraDir;
        private Vector3 _cameraUp;
        private float _camAngleX;
        private float _camAngleY;
        private float _camMoveSpeed;

        private const float MAX_PITCH_ANGLE = (float)((Math.PI / 180) * 89.0f); // 89 degrees converted to radians.

        public Matrix view { get; protected set; } // Camera view.
        public Matrix proj { get; protected set; } // Camera projection.

        private MouseState _prevMouseState;
        private float _mouseX;
        private float _mouseY;
        private float _diffMouseX;
        private float _diffMouseY;
        private float _mouseInputScale;

        // Constructor
        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up, float FOV)
            : base(game)
        {
            // Create camera view matrix.
            view = Matrix.CreateLookAt(pos, target, up);

            // Create camera projection matrix.
            proj = Matrix.CreatePerspectiveFieldOfView(FOV, (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height, 1, 1000);

            _cameraUp = up;
            _cameraPos = pos;
            _cameraDir = target;
            _camAngleX = 0.0f;
            _camAngleY = 360.0f;
            _camMoveSpeed = 3.0f;
            _mouseInputScale = 0.01f;
            _mouseX = _mouseY = _diffMouseX = _diffMouseY = 0.0f;
        }


        public void update(float deltaTime, KeyboardState kbState, MouseState mouseState)
        {
            // Process input.
            flyCameraInput(kbState, mouseState);

            // Update view matrix of camera.
            view = Matrix.CreateLookAt(_cameraPos, _cameraPos + _cameraDir, _cameraUp);
        }


        public void flyCameraInput(KeyboardState kbState, MouseState mouseState)
        {
            if (kbState.IsKeyDown(Keys.Left) || kbState.IsKeyDown(Keys.A))
                _cameraPos += Vector3.Cross(_cameraUp, _cameraDir * _camMoveSpeed);

            if (kbState.IsKeyDown(Keys.Right) || kbState.IsKeyDown(Keys.D))
                _cameraPos -= Vector3.Cross(_cameraUp, _cameraDir * _camMoveSpeed);

            if (kbState.IsKeyDown(Keys.Up) || kbState.IsKeyDown(Keys.W))
                _cameraPos += _cameraDir * _camMoveSpeed;

            if (kbState.IsKeyDown(Keys.Down) || kbState.IsKeyDown(Keys.S))
                _cameraPos -= _cameraDir * _camMoveSpeed;


            // Check if the user pressed the middle mouse button on this frame.
            if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                // Check if the user pressed the middle mouse button on the previous frame.
                if (_prevMouseState.MiddleButton == ButtonState.Pressed)
                {
                    // Get the difference between the current mouse pos and previous mouse pos.
                    _diffMouseX = mouseState.X - _mouseX;
                    _diffMouseY = mouseState.Y - _mouseY;

                    // Calcualte camera rotation angles.
                    _camAngleX -= _diffMouseY * _mouseInputScale; // Pitch.
                    _camAngleY -= _diffMouseX * _mouseInputScale; // Yaw.

                    // Limits the user's pitch.
                    if(_camAngleX > MAX_PITCH_ANGLE || _camAngleX < -MAX_PITCH_ANGLE)
                    {
                        if (_camAngleX > MAX_PITCH_ANGLE) _camAngleX = MAX_PITCH_ANGLE;
                        else _camAngleX = -MAX_PITCH_ANGLE;
                    }

                    // Calculate camera direction.
                    _cameraDir.X = (float)(Math.Cos(_camAngleX) * Math.Sin(_camAngleY));
                    _cameraDir.Y = (float)Math.Sin(_camAngleX);
                    _cameraDir.Z = (float)(Math.Cos(_camAngleX) * Math.Cos(_camAngleY));
                }

                // Store current mouse position.
                _mouseX = mouseState.X;
                _mouseY = mouseState.Y;
            }

            // Store state of mouse of previous frame.
            _prevMouseState = mouseState;
        }

        public Vector3 getPosition() { return _cameraPos; }
    }
}
