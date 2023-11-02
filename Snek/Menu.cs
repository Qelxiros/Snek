using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

internal struct GameParams
{
    public GameParams(string n, double s, int i, double m, int f, bool v)
    {
        name = n;
        speed = s;
        interval = i;
        mult = m;
        foods = f;
        vim = v;
    }

    public string name;
    public double speed;
    public int interval;
    public double mult;
    public int foods;
    public bool vim;
}

public class Menu : IGameMode
{
    private readonly Game1 _game1;
    private readonly Landing _landing;
    private int _currentGameMode;
    private SpriteFont _font;
    private Vector2 _fontSize;
    private readonly int _framesPerSecond;
    private List<GameParams> _gameModes;
    private bool _maxConcurrentFoods;
    private bool _showStats;

    public Menu(Landing landing, Game1 game1, int framesPerSecond)
    {
        _landing = landing;
        _game1 = game1;
        _framesPerSecond = framesPerSecond;
    }

    public void Initialize(double width, double height)
    {
        _showStats = false;
        _gameModes = new List<GameParams>
        {
            new("Easy", 15d / _framesPerSecond, 10, 0.95, 5, false),
            new("Medium", 5d / _framesPerSecond, 10, 0.9, 3, false),
            new("Hard", 3d / _framesPerSecond, 8, 0.8, 1, false),
            new("HyperSnek", 1d / _framesPerSecond, 10, 1, 1, false),
            new("vim", 7.5 / _framesPerSecond, 10, 1, 1, true),
            new("HyperVim", 1d / _framesPerSecond, 10, 1, 1, true)
        };
    }

    public void LoadContent(Game game, ContentManager content)
    {
        _font = content.Load<SpriteFont>("menu");
        _fontSize = _font.MeasureString("0");
    }

    public void Update()
    {
        if (Input.GetButtonDown(1, Input.ArcadeButtons.B1) || Input.GetButtonDown(1, Input.ArcadeButtons.B2) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.B3) || Input.GetButtonDown(1, Input.ArcadeButtons.B4))
        {
            _game1.ReturnToState(_landing);
            return;
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A1) || Input.GetButtonDown(2, Input.ArcadeButtons.A1) ||
            Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            Interim interim = new(_game1, this, _maxConcurrentFoods ? int.MaxValue : _gameModes[_currentGameMode].foods,
                _gameModes[_currentGameMode].speed, _gameModes[_currentGameMode].mult,
                _gameModes[_currentGameMode].interval, _framesPerSecond, _gameModes[_currentGameMode].vim);
            _game1.AddState(interim);
            _game1.AddState(interim.GetSnek());
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A3) || Input.GetButtonDown(2, Input.ArcadeButtons.A3))
            _showStats = !_showStats;

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A4) || Input.GetButtonDown(2, Input.ArcadeButtons.A4))
            _maxConcurrentFoods = !_maxConcurrentFoods;

        if (Input.GetButtonDown(1, Input.ArcadeButtons.StickLeft) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickLeft) ||
            Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            _currentGameMode += _gameModes.Count - 1;
            _currentGameMode %= _gameModes.Count;
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.StickRight) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickRight) ||
            Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            _currentGameMode += 1;
            _currentGameMode %= _gameModes.Count;
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
    {
        List<string> options = new()
        {
            $"Mode: {_gameModes[_currentGameMode].name}", _showStats ? "Concurrent Foods" : "",
            _showStats ? $"<{_gameModes[_currentGameMode].foods}>" : "", _showStats ? "Speed" : "",
            _showStats ? $"<{_gameModes[_currentGameMode].speed}>" : "",
            _showStats ? "Speed Increase" : "", _showStats ? $"<x{_gameModes[_currentGameMode].mult}>" : "",
            _showStats ? "Speed Increase Interval" : "", _showStats ? $"<{_gameModes[_currentGameMode].interval}>" : "",
            "", "", "", "", "Instructions", "Purple button to go back", "Joystick left/right",
            "to change game mode", "Red button - play", "Blue button - high scores", "Green button - stats",
            "White button - max foods",
            "Joystick to change", "direction in game"
        };

        for (var i = 0; i < options.Count; i++)
            spriteBatch.DrawString(_font, options[i],
                new Vector2((graphics.PreferredBackBufferWidth - options[i].Length * _fontSize.X) / 2,
                    _fontSize.Y * 2 * i), Color.Gray);
    }
}