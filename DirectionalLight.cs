using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Water_Simulation
{
    class DirectionalLight : GameComponent
    {
        private float[] direction;
        private float[] color;
        private const int NUM_ELEMENTS = 3;

        public DirectionalLight(Game game, float[] dir, float[] clr) : base(game)
        {
            direction = new float[NUM_ELEMENTS];
            color = new float[NUM_ELEMENTS];
            setDirection(dir);
            setColor(clr);
        }


        public DirectionalLight(Game game, Vector3 dir, Vector3 clr) : base(game)
        {
            direction = new float[NUM_ELEMENTS];
            color = new float[NUM_ELEMENTS];
            setDirection(dir);
            setColor(clr);
        }


        public void setDirection(float[] dir)
        {
            for (int i = 0; i < NUM_ELEMENTS; ++i)
                direction[i] = dir[i];
        }


        public void setDirection(Vector3 dir)
        {
            direction[0] = dir.X;
            direction[1] = dir.Y;
            direction[2] = dir.Z;
        }


        public void setColor(float[] clr)
        {
            for (int i = 0; i < NUM_ELEMENTS; ++i)
                color[i] = clr[i];
        }


        public void setColor(Vector3 clr)
        {
            color[0] = clr.X;
            color[1] = clr.Y;
            color[2] = clr.Z;
        }


        public float[] getDirection() { return direction; }


        public float[] getColor() { return color; }
    }
}
