using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

public class Snek : IGameMode {
    private readonly int _concurrentFoods;
    private readonly IGameMode _fallback;
    private readonly Game1 _game1;
    private readonly Random _rng = new();
    private readonly int _speedIncreaseInterval;
    private readonly double _speedMultiplier;
    private Texture2D _directionlessCell;
    private Texture2D _downCell;
    private SoundEffect _eatApple0;
    private SoundEffect _eatApple1;
    private SoundEffect _eatEasterEgg;
    private SoundEffect _eatShit;
    private Vector2?[] _food;
    private Texture2D _foodCell;
    private Color _foodColor;
    private int _framesSinceLastMove;
    private Rectangle _gridSize;
    private int _gridSquareSize;
    private Heading _heading;
    private bool _headingChangedSinceLastMove;
    private Texture2D _leftCell;
    private ArrayList[,] _nextFrameSnek;
    private Texture2D _rightCell;
    private long _score;
    private SpriteFont _scoreFont;
    private Vector2 _scoreFontSize;
    private LinkedList<Vector2> _snek;
    private Color _snekColor;
    private double _speed;
    private HashSet<Tuple<int, int>> _unusedCells;
    private Texture2D _upCell;

    public Snek(IGameMode fallback, Game1 game1, int concurrentFoods, double speed, double speedMultiplier,
        int speedIncreaseInterval) {
        _fallback = fallback;
        _game1 = game1;
        _concurrentFoods = concurrentFoods;
        _speed = speed;
        _speedMultiplier = speedMultiplier;
        _speedIncreaseInterval = speedIncreaseInterval;
    }

    public void Initialize(double width, double height) {
        _gridSquareSize = 70;
        _gridSize = new Rectangle(0, 0, (int)width / _gridSquareSize,
            (int)height / _gridSquareSize);
        _score = 0;
        _framesSinceLastMove = 0;
        _headingChangedSinceLastMove = false;
        _unusedCells = new HashSet<Tuple<int, int>>();
        for (int i = 0; i < _gridSize.Width; i++) {
            for (int j = 0; j < _gridSize.Height; j++) {
                _unusedCells.Add(new Tuple<int, int>(i, j));
            }
        }

        _snek = new LinkedList<Vector2>();
        _snek.AddFirst(new Vector2(0, 0));
        _unusedCells.Remove(new Tuple<int, int>(0, 0));
        _snek.AddFirst(new Vector2(0, 1));
        _unusedCells.Remove(new Tuple<int, int>(0, 1));
        _snek.AddFirst(new Vector2(0, 2));
        _unusedCells.Remove(new Tuple<int, int>(0, 2));
        _snek.AddFirst(new Vector2(0, 3));
        _unusedCells.Remove(new Tuple<int, int>(0, 3));
        _heading = Heading.Down;
        _food = new Vector2?[_concurrentFoods];

        for (int i = 0; i < _concurrentFoods; i++) {
            CreateFood(i, _unusedCells.Count > 0);
        }

        _snekColor = Color.LimeGreen;
        _foodColor = Color.Red;
    }

    public void LoadContent(Game game, ContentManager content) {
        _scoreFont = content.Load<SpriteFont>("score");
        _scoreFontSize = _scoreFont.MeasureString("0"); // the font is monospace so this works

        Color[] data = new Color[_gridSquareSize * _gridSquareSize];

        _directionlessCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            if (i is > 630 and < 4200 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60) {
                data[i] = _snekColor;
            }
        }

        _directionlessCell.SetData(data);

        _rightCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            if (i is >= 630 and <= 4200 && i % _gridSquareSize >= 60) {
                data[i] = _snekColor;
            }
        }

        _rightCell.SetData(data);

        _leftCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            if (i is >= 630 and < 4200 && i % _gridSquareSize <= 10) {
                data[i] = _snekColor;
            }
        }

        _leftCell.SetData(data);

        _upCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            if (i < 4200 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60) {
                data[i] = _snekColor;
            }
        }

        _upCell.SetData(data);

        _downCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        data = new Color[_gridSquareSize * _gridSquareSize];
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            if (i > 630 && i % _gridSquareSize >= 10 && i % _gridSquareSize < 60) {
                data[i] = _snekColor;
            }
        }

        _downCell.SetData(data);

        data = new Color[_gridSquareSize * _gridSquareSize];
        _foodCell = new Texture2D(game.GraphicsDevice, _gridSquareSize, _gridSquareSize);
        for (int i = 0; i < _gridSquareSize * _gridSquareSize; i++) {
            int x = i % _gridSquareSize - _gridSquareSize / 2;
            int y = i / _gridSquareSize - _gridSquareSize / 2;
            if (x * x + y * y < _gridSquareSize * 0.2 * (_gridSquareSize * 0.2)) {
                data[i] = _foodColor;
            }
        }

        _foodCell.SetData(data);

        _eatApple0 = content.Load<SoundEffect>("apple0");
        _eatApple1 = content.Load<SoundEffect>("apple1");
        _eatShit = content.Load<SoundEffect>("eatShit");
        _eatEasterEgg = content.Load<SoundEffect>("eatEasterEgg");
    }

    public void Update() {
        if ((Keyboard.GetState().IsKeyDown(Keys.Up) || Input.GetButton(1, Input.ArcadeButtons.StickUp) ||
             Input.GetButton(2, Input.ArcadeButtons.StickUp)) &&
            _heading is Heading.Left or Heading.Right &&
            !_headingChangedSinceLastMove) {
            _heading = Heading.Up;
            _headingChangedSinceLastMove = true;
        } else if ((Keyboard.GetState().IsKeyDown(Keys.Down) ||
                    Input.GetButton(1, Input.ArcadeButtons.StickDown) ||
                    Input.GetButton(2, Input.ArcadeButtons.StickDown)) &&
                   _heading is Heading.Left or Heading.Right && !_headingChangedSinceLastMove) {
            _heading = Heading.Down;
            _headingChangedSinceLastMove = true;
        } else if ((Keyboard.GetState().IsKeyDown(Keys.Left) ||
                    Input.GetButton(1, Input.ArcadeButtons.StickLeft) ||
                    Input.GetButton(2, Input.ArcadeButtons.StickLeft)) &&
                   _heading is Heading.Up or Heading.Down && !_headingChangedSinceLastMove) {
            _heading = Heading.Left;
            _headingChangedSinceLastMove = true;
        } else if ((Keyboard.GetState().IsKeyDown(Keys.Right) ||
                    Input.GetButton(1, Input.ArcadeButtons.StickRight) ||
                    Input.GetButton(2, Input.ArcadeButtons.StickRight)) &&
                   _heading is Heading.Up or Heading.Down && !_headingChangedSinceLastMove) {
            _heading = Heading.Right;
            _headingChangedSinceLastMove = true;
        }

        if (_framesSinceLastMove < 15 * _speed) {
            _framesSinceLastMove++;
            return;
        }

        _framesSinceLastMove = 0;
        _headingChangedSinceLastMove = false;

        if (_snek.First == null) {
            Console.Error.WriteLine("this is very bad");
            Environment.Exit(1);
        }

        Vector2 nextCell = new(_snek.First.Value.X, _snek.First.Value.Y);
        bool death = false;
        switch (_heading) {
        case Heading.Up:
            nextCell.Y--;
            if (nextCell.Y >= _gridSize.Height || nextCell.Y < 0) {
                death = true;
            }

            break;
        case Heading.Down:
            nextCell.Y++;
            if (nextCell.Y >= _gridSize.Height || nextCell.Y < 0) {
                death = true;
            }

            break;
        case Heading.Left:
            nextCell.X--;
            if (nextCell.X >= _gridSize.Width || nextCell.X < 0) {
                death = true;
            }

            break;
        case Heading.Right:
            nextCell.X++;
            if (nextCell.X >= _gridSize.Width || nextCell.X < 0) {
                death = true;
            }

            break;
        default:
            Console.WriteLine("everything is broken\nthe apocalypse is upon us\ngodspeed");
            death = true;
            break;
        }

        if (_snek.Contains(nextCell)) {
            death = true;
        }

        if (death) {
            // Game1.HandleScore(_score);
            if (_rng.Next(1000000) == 696969) {
                _eatEasterEgg.Play();
                Thread.Sleep(_eatEasterEgg.Duration.Milliseconds);
            } else {
                _eatShit.Play();
                Thread.Sleep(_eatShit.Duration.Milliseconds);
            }

            _game1.ReturnToState(_fallback);
        }

        _snek.AddFirst(nextCell);
        _unusedCells.Remove(new Tuple<int, int>((int)Math.Round(nextCell.X), (int)Math.Round(nextCell.Y)));

        if (!_food.Contains(nextCell)) {
            if (_snek.Last == null) {
                Console.Error.WriteLine("you will never see this message");
                Environment.Exit(7);
            }

            _unusedCells.Add(new Tuple<int, int>((int)Math.Round(_snek.Last.Value.X),
                (int)Math.Round(_snek.Last.Value.Y)));
            _snek.RemoveLast();
        } else {
            int foodIndex = Array.FindIndex(_food,
                vector2 => vector2 != null &&
                           (int)Math.Round(vector2.Value.X) == (int)Math.Round(nextCell.X) &&
                           (int)Math.Round(vector2.Value.Y) == (int)Math.Round(nextCell.Y));
            CreateFood(foodIndex, _unusedCells.Count > 0);

            _score++;
            if (_score % _speedIncreaseInterval == 0) {
                _speed = _speedMultiplier * _speed;
            }

            switch (_rng.Next(1)) {
            case 0:
                _eatApple0.Play();
                break;
            case 1:
                _eatApple1.Play();
                break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics) {
        _nextFrameSnek = new ArrayList[_gridSize.Width, _gridSize.Height];
        for (LinkedListNode<Vector2> node = _snek.First; node != null; node = node.Next) {
            Vector2 cell = node.Value;
            ArrayList textures = new() { _directionlessCell };
            if (node.Next != null) {
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
            }

            if (node.Previous != null) {
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
            }

            _nextFrameSnek[(int)Math.Round(cell.X), (int)Math.Round(cell.Y)] = textures;
        }

        string scoreString = _score.ToString();

        spriteBatch.DrawString(_scoreFont, scoreString,
            new Vector2((graphics.PreferredBackBufferWidth - _scoreFontSize.X * scoreString.Length) / 2,
                (graphics.PreferredBackBufferHeight - _scoreFontSize.Y) / 2),
            Color.DarkSlateGray);

        for (int i = 0; i < _gridSize.Width; i++) {
            for (int j = 0; j < _gridSize.Height; j++) {
                if (_nextFrameSnek[i, j] == null) {
                    continue;
                }

                for (int k = 0; k < _nextFrameSnek[i, j].Count; k++) {
                    spriteBatch.Draw((Texture2D)_nextFrameSnek[i, j][k],
                        new Rectangle(_gridSquareSize * i, _gridSquareSize * j, _gridSquareSize,
                            _gridSquareSize), Color.White);
                }
            }
        }

        foreach (Vector2? f in _food) {
            if (f == null) {
                continue;
            }

            spriteBatch.Draw(_foodCell,
                new Rectangle((int)Math.Round(_gridSquareSize * f.Value.X),
                    (int)Math.Round(_gridSquareSize * f.Value.Y),
                    _gridSquareSize, _gridSquareSize), Color.White);
        }
    }

    private void CreateFood(int index, bool fr) {
        if (!fr) {
            _food[index] = null;
        } else {
            Tuple<int, int> temp = _unusedCells.ElementAt(_rng.Next(_unusedCells.Count));
            _unusedCells.Remove(temp);
            Vector2 food = new(temp.Item1, temp.Item2);
            _food[index] = food;
        }
    }
}