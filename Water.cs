using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Water_Simulation
{
    public class Water : GameComponent
    {
        private static Game1 _pGame;
        private static Random _rng = new Random(); // Random number generator.
        private Matrix _mWorldMatrix = Matrix.Identity; // World position of model.
        private Texture2D _texture;

        private float[,] _modelHts; // 2D array of height values for water model.
        private Vector3[,] _modelNrms; // 2D array of normal values for water model.
        private VertexPositionNormalTexture[] _modelPNVerts; // Holds values for model.
        private int _numVerts; // Number of verticies of water model.
        private int _numTriangles; // Number of triangles in water model.
        private int _width; // Width of water model.
        private int _length; // Length of water model.
        private float[,] _velocityField; // Holds the speed of each cell in water.
        private float[,] _fluid_modelHts; // Holds amount of water in each cell.
        private float[,] _fluid_modelHtsTemp; // Used for calculations.


        // Constructor.
        public Water(Game game, Vector3 pos, int width, int length, Texture2D texture) : base(game)
        {
            // Store reference to game.
            _pGame = (Game1)game;

            // Set width and length of model.
            _width = width;
            _length = length;

            // Calculate number of verticies in model.
            _numVerts = _width * _length * 6;

            // Calculate number of triangles in model.
            _numTriangles = _width * _length * 2;

            // Create arrays for water model.
            _velocityField = new float[width, length];
            _fluid_modelHts = new float[width, length];
            _fluid_modelHtsTemp = new float[width, length];
            _modelHts = new float[width, length];
            _modelNrms = new Vector3[width, length];
            _modelPNVerts = new VertexPositionNormalTexture[_numVerts];

            // Set position of model in world.
            _mWorldMatrix.Translation = pos;

            // Save texture.
            _texture = texture;

            // Initialize values for water model.
            for (int i = 0; i < width; ++i)
            {
                for(int j = 0; j < length; ++j)
                {
                    // Generate random heig_model_modelHts for water in cells.
                    _fluid_modelHts[i, j] = (float)(_rng.NextDouble() + _rng.Next(1, 6));

                    // Set _velocityField of all water in cells to 0.
                    _velocityField[i,j] = 0.0f;

                    // Set heig_model_modelHts of water model to 0.
                    _modelHts[i, j] = 0.0f;

                    // Set _modelNrms of water model to 0.
                    _modelNrms[i, j] = Vector3.Zero;
                }
            }

            // Apply texture to model.
            setTexture(_texture);
        }


        public void update(float deltaTime)
        {
            simulateWaterMovement(deltaTime);
            updateModel();
        }


        public void draw()
        {
            // Send information about water model to shader.
            _pGame.draw_lightingTextureShader(_mWorldMatrix, _texture, ref _modelPNVerts, _numTriangles);
        }


        public void setTexture(Texture2D texture)
        {
            int index = 0;

            // Set size of each texture fragment.
            float texFragX = 1.0f / (float)_width;
            float texFragZ = 1.0f / (float)_length;

            // Set texture on model.
            for(int i = 0; i < _width - 1; ++i)
            {
                for(int j = 0; j < _length - 1; ++j)
                {
                    // Left Triangle
                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2(i * texFragX, j * texFragZ);
                    _modelPNVerts[index].Position = new Vector3(i, _modelHts[i, j], j);
                    _modelPNVerts[index++].Normal = _modelNrms[i, j];

                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2((i + 1) * texFragX,(j + 1) * texFragZ);
                    _modelPNVerts[index].Position =
                        new Vector3(i + 1, _modelHts[i + 1, j + 1], j + 1);
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j + 1];

                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2(i * texFragX,(j + 1) * texFragZ);
                    _modelPNVerts[index].Position =
                        new Vector3(i, _modelHts[i, j + 1], j + 1);
                    _modelPNVerts[index++].Normal = _modelNrms[i, j + 1];

                    // Right Triangle.
                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2(i * texFragX, j * texFragZ);
                    _modelPNVerts[index].Position = new Vector3(i, _modelHts[i, j], j);
                    _modelPNVerts[index++].Normal = _modelNrms[i, j];

                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2((i + 1) * texFragX, j * texFragZ);
                    _modelPNVerts[index].Position =
                        new Vector3(i + 1, _modelHts[i + 1, j], j);
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j];

                    _modelPNVerts[index].TextureCoordinate =
                        new Vector2((i + 1) * texFragX, (j + 1) * texFragZ);
                    _modelPNVerts[index].Position = 
                        new Vector3(i + 1, _modelHts[i + 1, j + 1], j + 1);
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j + 1];
                }
            }
        }


        public void simulateWaterMovement(float deltaTime)
        {
            float _velocityDamper = 0.99f;

            // Calculate fluid along x axis borders.
            for (int i = 0; i < _width; ++i)
            {
                if (i == 0)
                {
                    // Calculate _velocityField at origin.
                    _velocityField[i, 0] += (_fluid_modelHts[i + 1, 0] + _fluid_modelHts[i, 1] +
                        _fluid_modelHts[i + 1, 1]) / 3 - _fluid_modelHts[0, 0];
                    _velocityField[i, 0] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, 0] += _velocityField[i, 0] * deltaTime;

                    // Calculate _velocityField opposite origin.
                    _velocityField[i, _length - 1] += (_fluid_modelHts[i + 1, _length - 1] +
                        _fluid_modelHts[i, _length - 2] + _fluid_modelHts[i + 1, _length - 2]) / 3
                        - _fluid_modelHts[i, _length - 1];
                    _velocityField[i, _length - 1] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, _length - 1] += _velocityField[i, _length - 1] * deltaTime;
                }
                else if (i == _width - 1)
                {
                    // Calculate _velocityField at end of x bounds.
                    _velocityField[i, 0] += (_fluid_modelHts[i, 1] + _fluid_modelHts[i - 1, 0] +
                        _fluid_modelHts[i - 1, 1]) / 3 - _fluid_modelHts[i, 0];
                    _velocityField[i, 0] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, 0] += _velocityField[i, 0] * deltaTime;

                    // Calculate _velocityField at end of x bounds.
                    _velocityField[i, _length - 1] += (_fluid_modelHts[i - 1, _length - 1] +
                        _fluid_modelHts[i, _length - 2] + _fluid_modelHts[i - 1, _length - 2]) / 3
                        - _fluid_modelHts[i, _length - 1];
                    _velocityField[i, _length - 1] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, _length - 1] += _velocityField[i, _length - 1] * deltaTime;
                }
                else
                {
                    // Calculate _velocityField along origin.
                    _velocityField[i, 0] += (_fluid_modelHts[i + 1, 0] + _fluid_modelHts[i - 1, 0] +
                        _fluid_modelHts[i, 1]) / 3 - _fluid_modelHts[i, 0];
                    _velocityField[i, 0] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, 0] += _velocityField[i, 0] * deltaTime;

                    // Calculate _velocityField opposite origin.
                    _velocityField[i, _length - 1] += (_fluid_modelHts[i + 1, _length - 1] +
                        _fluid_modelHts[i - 1, _length - 1] + _fluid_modelHts[i, _length - 2]) / 3
                        - _fluid_modelHts[i, _length - 1];
                    _velocityField[i, _length - 1] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, _length - 1] += _velocityField[i, _length - 1] * deltaTime;
                }
            }

            // Calculate fluid along z axis borders.
            for (int j = 0; j < _length; ++j)
            {
                if (j == 0)
                {
                    // Calculate _velocityField at origin.
                    _velocityField[0, j] += (_fluid_modelHts[1, j] + _fluid_modelHts[0, j + 1] +
                        _fluid_modelHts[1, j + 1]) / 3 - _fluid_modelHts[0, j];
                    _velocityField[0, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[0, j] += _velocityField[0, j] * deltaTime;

                    // Calculate _velocityField opposite of origin.
                    _velocityField[_width - 1, j] += (_fluid_modelHts[_width - 2, j] +
                        _fluid_modelHts[_width - 1, j + 1] + _fluid_modelHts[_width - 2, j + 1]) / 3
                        - _fluid_modelHts[_width - 1, j];
                    _velocityField[_width - 1, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[_width - 1, j] += _velocityField[_width - 1, j] * deltaTime;
                }
                else if (j == _length - 1)
                {
                    // Calculate _velocityField at end of z bounds.
                    _velocityField[0, j] += (_fluid_modelHts[1, j] + _fluid_modelHts[0, j - 1] +
                        _fluid_modelHts[1, j - 1]) / 3 - _fluid_modelHts[0, j];
                    _velocityField[0, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[0, j] += _velocityField[0, j] * deltaTime;

                    // Calculate _velocityField at end of z bounds.
                    _velocityField[_width - 1, j] += (_fluid_modelHts[_width - 2, j] +
                        _fluid_modelHts[_width - 1, j - 1] + _fluid_modelHts[_width - 2, j - 1]) / 3
                        - _fluid_modelHts[_width - 1, j];
                    _velocityField[_width - 1, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[_width - 1, j] += _velocityField[_width - 1, j] * deltaTime;
                }
                else
                {
                    // Calculate _velocityField along origin.
                    _velocityField[0, j] += (_fluid_modelHts[0, j + 1] + _fluid_modelHts[0, j - 1]
                        + _fluid_modelHts[1, j]) / 3 - _fluid_modelHts[0, j];
                    _velocityField[0, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[0, j] += _velocityField[0, j] * deltaTime;

                    // Calculate _velocityField opposite origin.
                    _velocityField[_width - 1, j] += (_fluid_modelHts[_width - 1, j + 1] + _fluid_modelHts[_width - 1, j - 1]
                        + _fluid_modelHts[_width - 2, j]) / 3 - _fluid_modelHts[_width - 1, j];
                    _velocityField[_width - 1, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[_width - 1, j] += _velocityField[_width - 1, j] * deltaTime;
                }
            }

            // Calculate fluid heights for body of water except for borders.
            for (int i = 1; i < _width - 1; ++i)
            {
                for (int j = 1; j < _length - 1; ++j)
                {
                    _velocityField[i, j] += (_fluid_modelHts[i - 1, j] + _fluid_modelHts[i + 1, j] +
                        _fluid_modelHts[i, j - 1] + _fluid_modelHts[i, j + 1]) / 4 - _fluid_modelHts[i, j];
                    _velocityField[i, j] *= _velocityDamper; // Dampen _velocityField.
                    _fluid_modelHts[i, j] += _velocityField[i, j] * deltaTime;
                }
            }

            //===================================================================================//
            // Update heights of model.
            for (int i = 0; i < _width; ++i)
                for (int j = 0; j < _length; ++j)
                    _modelHts[i, j] = _fluid_modelHts[i, j];

            // Calculate normals for model.
            for (int i = 1; i < _width - 1; ++i)
                for (int j = 0; j < _length - 1; ++j)
                    _modelNrms[i, j] = Vector3.Normalize(Vector3.Cross(
                        new Vector3(0, _modelHts[i, j + 1] - _modelHts[i, j], 1),
                        new Vector3(1, _modelHts[i, j] - _modelHts[i - 1, j], 0)));
        }


        public void updateModel()
        {
            int index = 0;

            for (int i = 0; i < _width - 1; ++i)
            {
                for (int j = 0; j < _length - 1; ++j)
                {
                    // Left triangle.
                    _modelPNVerts[index].Position.Y = _modelHts[i, j];
                    _modelPNVerts[index++].Normal = _modelNrms[i, j];

                    _modelPNVerts[index].Position.Y = _modelHts[i + 1, j + 1];
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j + 1];

                    _modelPNVerts[index].Position.Y = _modelHts[i, j + 1];
                    _modelPNVerts[index++].Normal = _modelNrms[i, j + 1];

                    // Right Triangle.
                    _modelPNVerts[index].Position.Y = _modelHts[i, j];
                    _modelPNVerts[index++].Normal = _modelNrms[i, j];

                    _modelPNVerts[index].Position.Y = _modelHts[i + 1, j];
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j];

                    _modelPNVerts[index].Position.Y = _modelHts[i + 1, j + 1];
                    _modelPNVerts[index++].Normal = _modelNrms[i + 1, j + 1];
                }
            }
        }
    }
}
