using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

public enum Heading {
    Up,
    Down,
    Left,
    Right
}

public class Game1 : Game {
    private readonly GraphicsDeviceManager _graphics;
    private int _concurrentFoods;
    private Texture2D _directionlessCell;
    private Texture2D _downCell;
    private Vector2[] _food;
    private Texture2D _foodCell;
    private Color _foodColor;
    private int _framesPerMove;
    private int _framesSinceLastMove;
    private Rectangle _gridSize;
    private int _gridSquareSize;
    private Heading _heading;
    private bool _headingChangedSinceLastMove;
    private Texture2D _leftCell;
    private ArrayList[,] _nextFrameSnek;
    private Random _rand;
    private Texture2D _rightCell;
    private long _score;
    private SpriteFont _scoreFont;
    private Vector2 _scoreFontSize;
    private LinkedList<Vector2> _snek;
    private Color _snekColor;
    private float _speedMultiplier;
    private SpriteBatch _spriteBatch;
    private Texture2D _upCell;
    private Rectangle _windowSize;

    /// <summary>
    ///     Game constructor
    /// </summary>
    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    /// <summary>
    ///     Does any setup prior to the first frame that doesn't need loaded content.
    /// </summary>
    protected override void Initialize() {
        Input.Initialize(); // Sets up the input library

        #region

#if DEBUG
        _graphics.PreferredBackBufferWidth = 420;
        _graphics.PreferredBackBufferHeight = 980;
        _graphics.ApplyChanges();
#else
			_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
			_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
			_graphics.ApplyChanges();
#endif

        #endregion

        _score = 0;
        _speedMultiplier = 0.98F;
        _snekColor = Color.LimeGreen;
        _foodColor = Color.Red;
        _framesSinceLastMove = 0;
        _framesPerMove = 15;
        _headingChangedSinceLastMove = false;
        _concurrentFoods = 1;
        _gridSquareSize = 70;
        _windowSize = GraphicsDevice.Viewport.Bounds;
        _gridSize = new Rectangle(0, 0, _windowSize.Width / _gridSquareSize,
            _windowSize.Height / _gridSquareSize);

        _rand = new Random();

        _snek = new LinkedList<Vector2>();
        _snek.AddFirst(new Vector2(0, 0));
        _snek.AddFirst(new Vector2(0, 1));
        _snek.AddFirst(new Vector2(0, 2));
        _snek.AddFirst(new Vector2(0, 3));
        _heading = Heading.Down;
        _food = new Vector2[_concurrentFoods];
        for (var i = 0; i < _concurrentFoods; i++) CreateFood(i);

        base.Initialize();
    }

    /// <summary>
    ///     Does any setup prior to the first frame that needs loaded content.
    /// </summary>
    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _scoreFont = Content.Load<SpriteFont>("score");
        _scoreFontSize = _scoreFont.MeasureString("0"); // the font is monospace so this works

        var data = new Color[_gridSquareSize * _gridSquareSize];

        _directionlessCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++)
            if (i > 630 && i < 4200 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60)
                data[i] = _snekColor;

        _directionlessCell.SetData(data);

        _rightCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++)
            if (i > 630 && i < 4200 && i % _gridSquareSize >= 60)
                data[i] = _snekColor;

        _rightCell.SetData(data);

        _leftCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++)
            if (i > 630 && i < 4200 && i % _gridSquareSize <= 10)
                data[i] = _snekColor;

        _leftCell.SetData(data);

        _upCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++)
            if (i < 4200 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60)
                data[i] = _snekColor;

        _upCell.SetData(data);

        _downCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++)
            if (i > 630 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60)
                data[i] = _snekColor;

        _downCell.SetData(data);

        data = new Color[_gridSquareSize * _gridSquareSize];
        _foodCell = new Texture2D(GraphicsDevice, _gridSquareSize, _gridSquareSize);
        for (var i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            var x = i % _gridSquareSize - _gridSquareSize / 2;
            var y = i / _gridSquareSize - _gridSquareSize / 2;
            if (x * x + y * y <= _gridSquareSize * 0.2 * (_gridSquareSize * 0.2)) data[i] = _foodColor;
        }

        _foodCell.SetData(data);
    }

    /// <summary>
    ///     Your main update loop. This runs once every frame, over and over.
    /// </summary>
    /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
    protected override void Update(GameTime gameTime) {
        Input.Update(); // Updates the state of the input library

        // Exit when both menu buttons are pressed (or escape for keyboard debuging)
        // You can change this but it is suggested to keep the keybind of both menu
        // buttons at once for gracefull exit.
        if (Keyboard.GetState().IsKeyDown(Keys.Escape) ||
            (Input.GetButton(1, Input.ArcadeButtons.Menu) &&
             Input.GetButton(2, Input.ArcadeButtons.Menu)))
            Exit();

        if ((Keyboard.GetState().IsKeyDown(Keys.Up) || Input.GetButton(1, Input.ArcadeButtons.StickUp) ||
             Input.GetButton(2, Input.ArcadeButtons.StickUp)) &&
            (_heading == Heading.Left || _heading == Heading.Right) &&
            !_headingChangedSinceLastMove) {
            _heading = Heading.Up;
            _headingChangedSinceLastMove = true;
        }
        else if ((Keyboard.GetState().IsKeyDown(Keys.Down) || Input.GetButton(1, Input.ArcadeButtons.StickDown) ||
                  Input.GetButton(2, Input.ArcadeButtons.StickDown)) &&
                 (_heading == Heading.Left || _heading == Heading.Right) && !_headingChangedSinceLastMove) {
            _heading = Heading.Down;
            _headingChangedSinceLastMove = true;
        }
        else if ((Keyboard.GetState().IsKeyDown(Keys.Left) || Input.GetButton(1, Input.ArcadeButtons.StickLeft) ||
                  Input.GetButton(2, Input.ArcadeButtons.StickLeft)) &&
                 (_heading == Heading.Up || _heading == Heading.Down) && !_headingChangedSinceLastMove) {
            _heading = Heading.Left;
            _headingChangedSinceLastMove = true;
        }
        else if ((Keyboard.GetState().IsKeyDown(Keys.Right) || Input.GetButton(1, Input.ArcadeButtons.StickRight) ||
                  Input.GetButton(2, Input.ArcadeButtons.StickRight)) &&
                 (_heading == Heading.Up || _heading == Heading.Down) && !_headingChangedSinceLastMove) {
            _heading = Heading.Right;
            _headingChangedSinceLastMove = true;
        }

        if (_framesSinceLastMove < _framesPerMove) {
            _framesSinceLastMove++;
            return;
        }

        _framesSinceLastMove = 0;
        _headingChangedSinceLastMove = false;

        var nextCell = new Vector2(_snek.First.Value.X, _snek.First.Value.Y);
        switch (_heading) {
            case Heading.Up:
                nextCell.Y--;
                if (nextCell.Y >= _gridSize.Height || nextCell.Y < 0) Exit();

                break;
            case Heading.Down:
                nextCell.Y++;
                if (nextCell.Y >= _gridSize.Height || nextCell.Y < 0) Exit();

                break;
            case Heading.Left:
                nextCell.X--;
                if (nextCell.X >= _gridSize.Width || nextCell.X < 0) Exit();

                break;
            case Heading.Right:
                nextCell.X++;
                if (nextCell.X >= _gridSize.Width || nextCell.X < 0) Exit();

                break;
            default:
                Console.WriteLine("everything is broken\nthe apocalypse is upon us\ngodspeed");
                Exit();
                break;
        }

        if (_snek.Contains(nextCell)) Exit();

        _snek.AddFirst(nextCell);

        if (!_food.Contains(nextCell)) {
            _snek.RemoveLast();
        }
        else {
            var foodIndex = Array.FindIndex(_food,
                vector2 => (int)Math.Round(vector2.X) == (int)Math.Round(nextCell.X) &&
                           (int)Math.Round(vector2.Y) == (int)Math.Round(nextCell.Y));
            CreateFood(foodIndex);
            _score++;
            if (_score % 10 == 0) _framesPerMove = (int)(_speedMultiplier * _framesPerMove);
        }

        base.Update(gameTime);
    }

    /// <summary>
    ///     Your main draw loop. This runs once every frame, over and over.
    /// </summary>
    /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);

        _nextFrameSnek = new ArrayList[_gridSize.Width, _gridSize.Height];
        for (var node = _snek.First; node != null; node = node.Next) {
            var cell = node.Value;
            var textures = new ArrayList();
            textures.Add(_directionlessCell);
            if (node.Next != null)
                switch ((cell - node.Next.Value).X) {
                    case 1:
                        textures.Add(_leftCell);
                        break;
                    case -1:
                        textures.Add(_rightCell);
                        break;
                    case 0:
                        switch ((cell - node.Next.Value).Y) {
                            case 1:
                                textures.Add(_upCell);
                                break;
                            case -1:
                                textures.Add(_downCell);
                                break;
                        }

                        break;
                }

            if (node.Previous != null)
                switch ((cell - node.Previous.Value).X) {
                    case 1:
                        textures.Add(_leftCell);
                        break;
                    case -1:
                        textures.Add(_rightCell);
                        break;
                    case 0:
                        switch ((cell - node.Previous.Value).Y) {
                            case 1:
                                textures.Add(_upCell);
                                break;
                            case -1:
                                textures.Add(_downCell);
                                break;
                        }

                        break;
                }

            _nextFrameSnek[(int)Math.Round(cell.X), (int)Math.Round(cell.Y)] = textures;
        }

        _spriteBatch.Begin();
        var scoreString = _score.ToString();
        
        _spriteBatch.DrawString(_scoreFont, scoreString, new Vector2((_graphics.PreferredBackBufferWidth - _scoreFontSize.X * scoreString.Length)/2, (_graphics.PreferredBackBufferHeight-_scoreFontSize.Y)/2),
            Color.DarkSlateGray);

        for (var i = 0; i < _gridSize.Width; i++)
        for (var j = 0; j < _gridSize.Height; j++)
            if (_nextFrameSnek[i, j] != null)
                for (var k = 0; k < _nextFrameSnek[i, j].Count; k++)
                    _spriteBatch.Draw((Texture2D)_nextFrameSnek[i, j][k],
                        new Rectangle(_gridSquareSize * i, _gridSquareSize * j, _gridSquareSize,
                            _gridSquareSize), Color.White);
        // else {
        //     _spriteBatch.Draw(_emptyCell,
        //         new Rectangle(_gridSquareSize * (i), _gridSquareSize * (j), _gridSquareSize,
        //             _gridSquareSize), Color.White);
        // }
        foreach (var f in _food)
            _spriteBatch.Draw(_foodCell,
                new Rectangle((int)Math.Round(_gridSquareSize * f.X), (int)Math.Round(_gridSquareSize * f.Y),
                    _gridSquareSize, _gridSquareSize), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void CreateFood(int index) {
        Vector2 food;
        do {
            food = new Vector2(_rand.Next(_gridSize.Width), _rand.Next(_gridSize.Height));
        } while (_snek.Contains(food));

        _food[index] = food;
    }
}