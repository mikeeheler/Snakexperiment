﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snakexperiment
{
    public class SnakeGame : Game
    {
        const double TICK_RATE = 0.125;

        const int FIELD_HEIGHT = 20;
        const int FIELD_WIDTH = 20;

        private readonly Random _rng;

        private SpriteBatch _spriteBatch;
        private SpriteFont _uiFont;

        private Texture2D _appleTexture;
        private Texture2D _snakeAliveTexture;
        private Texture2D _snakeDeadTexture;
        private Texture2D _tileTexture;

        private Point _lastDirection;
        private Point _lastPosition;
        private TimeSpan _lastTick;

        private bool _alive;
        private Point _applePosition;
        private Point _direction;
        private Point _fieldTopLeft;
        private Queue<Point> _snake;
        private int _snakeSize;

        public SnakeGame()
        {
            _ = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 600,
                PreferredBackBufferWidth = 800,
                PreferHalfPixelOffset = true,
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = true
            };
            _rng = new Random();
            _fieldTopLeft = new Point(0, 0);

            IsFixedTimeStep = false;
            IsMouseVisible = true;

            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
        }

        protected override void Initialize()
        {
            _lastTick = TimeSpan.Zero;
            Reset();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _snakeAliveTexture = Content.Load<Texture2D>("square");
            _appleTexture = Content.Load<Texture2D>("apple");
            _snakeDeadTexture = Content.Load<Texture2D>("dead");
            _tileTexture = Content.Load<Texture2D>("tile");
            _uiFont = Content.Load<SpriteFont>("CascadiaMono");

            OnResize(null, EventArgs.Empty);
        }

        protected override void Update(GameTime gameTime)
        {
            HandleKeyPress(Keyboard.GetState());

            TimeSpan diff = gameTime.TotalGameTime - _lastTick;
            if (diff.TotalSeconds >= TICK_RATE)
            {
                UpdateEntities(diff);
                _lastTick = gameTime.TotalGameTime;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            float fps = (float)(1000.0 / gameTime.ElapsedGameTime.TotalMilliseconds);

            DrawField();
            DrawEntities();
            DrawUI(fps);

            base.Draw(gameTime);
        }

        private void DrawField()
        {
            Point tilePosition = Point.Zero;

            _spriteBatch.Begin();
            for (tilePosition.Y = 0; tilePosition.Y < FIELD_HEIGHT / 2; ++tilePosition.Y)
            {
                for (tilePosition.X = 0; tilePosition.X < FIELD_WIDTH / 2; ++tilePosition.X)
                {
                    _spriteBatch.Draw(
                        _tileTexture,
                        (_fieldTopLeft + tilePosition * _tileTexture.Bounds.Size).ToVector2(),
                        Color.White);
                }
            }
            _spriteBatch.End();
        }

        private void DrawEntities()
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(
                _appleTexture,
                (_fieldTopLeft + _applePosition * _appleTexture.Bounds.Size).ToVector2(),
                Color.White);
            Texture2D square = _alive ? _snakeAliveTexture : _snakeDeadTexture;
            foreach (Point snakePiece in _snake)
            {
                _spriteBatch.Draw(
                    square,
                    (_fieldTopLeft + snakePiece * square.Bounds.Size).ToVector2(),
                    Color.White);
            }
            _spriteBatch.End();
        }

        private void DrawUI(double fps)
        {
            string uiMessage = $"size: {_snakeSize:N0}; fps: {fps:N0}";
            var messageSize = _uiFont.MeasureString(uiMessage);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(
                _uiFont,
                uiMessage,
                new Vector2(Window.ClientBounds.Width - messageSize.X, Window.ClientBounds.Height - messageSize.Y),
                Color.LightGray);
            _spriteBatch.End();
        }

        private void HandleKeyPress(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Escape) || keyboardState.IsKeyDown(Keys.Q))
            {
                Exit();
                return;
            }

            if (!_alive && keyboardState.IsKeyDown(Keys.Space))
            {
                Reset();
                return;
            }

            if (_lastDirection.X == 0)
            {
                if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
                {
                    _direction.X = -1;
                    _direction.Y = 0;
                }
                else if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
                {
                    _direction.X = 1;
                    _direction.Y = 0;
                }
            }
            else if (_lastDirection.Y == 0)
            {
                if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
                {
                    _direction.X = 0;
                    _direction.Y = -1;
                }
                else if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
                {
                    _direction.X = 0;
                    _direction.Y = 1;
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            int windowHeight = Window.ClientBounds.Height;
            int windowWidth = Window.ClientBounds.Width;

            int fieldHeight = FIELD_HEIGHT * _tileTexture.Height / 2;
            int fieldWidth = FIELD_WIDTH * _tileTexture.Width / 2;

            _fieldTopLeft.X = (windowWidth - fieldWidth) / 2;
            _fieldTopLeft.Y = (windowHeight - fieldHeight) / 2;
        }

        private void Reset()
        {
            _applePosition = new Point(_rng.Next(FIELD_WIDTH), _rng.Next(FIELD_HEIGHT));
            _alive = true;
            _direction = new Point(1, 0);
            _lastDirection = _direction;
            _lastPosition = Point.Zero;
            _snake = new Queue<Point>(FIELD_WIDTH * FIELD_HEIGHT);
            _snake.Enqueue(_lastPosition);
            _snakeSize = 10;
        }

        private void UpdateEntities(TimeSpan _)
        {
            if (!_alive)
                return;

            Point newPosition = _lastPosition + _direction;
            if (newPosition.Y < 0
                || newPosition.Y >= FIELD_HEIGHT
                || newPosition.X < 0
                || newPosition.X >= FIELD_WIDTH
                || _snake.Contains(newPosition))
            {
                _alive = false;
                return;
            }

            if (newPosition == _applePosition)
            {
                _snakeSize += 5;
                do
                {
                    _applePosition.X = _rng.Next(FIELD_WIDTH);
                    _applePosition.Y = _rng.Next(FIELD_HEIGHT);
                } while (_snake.Contains(_applePosition));
            }

            while (_snake.Count >= _snakeSize)
                _snake.Dequeue();
            _snake.Enqueue(newPosition);

            _lastDirection = _direction;
            _lastPosition = newPosition;
        }
    }
}
