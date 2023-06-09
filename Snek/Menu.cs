using System;
using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

public class Menu : IGameMode {
    private readonly Game1 _game1;
    private readonly Landing _landing;
    private int _concurrentFoods;
    private SpriteFont _font;
    private Vector2 _fontSize;
    private int _hoveredMenuItem;
    private int _maxHoveredIndex;
    private double _speed;
    private int _speedIncreaseInterval;
    private double _speedMultiplier;

    public Menu(Landing landing, Game1 game1) {
        _landing = landing;
        _game1 = game1;
    }

    public void Initialize(double width, double height) {
        _speed = 1;
        _speedMultiplier = 0.95;
        _speedIncreaseInterval = 10;
        _concurrentFoods = 1;
        _hoveredMenuItem = 4;
        _maxHoveredIndex = 5;
    }

    public void LoadContent(Game game, ContentManager content) {
        _font = content.Load<SpriteFont>("menu");
        _fontSize = _font.MeasureString("0");
    }

    public void Update() {
        if (Input.GetButtonDown(1, Input.ArcadeButtons.B1) || Input.GetButtonDown(1, Input.ArcadeButtons.B2) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.B3) || Input.GetButtonDown(1, Input.ArcadeButtons.B4)) {
            _game1.ReturnToState(_landing);
            return;
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.StickUp) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickUp) ||
            Keyboard.GetState().IsKeyDown(Keys.Up)) {
            _hoveredMenuItem = Math.Max(_hoveredMenuItem - 1, 0);
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.StickDown) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickDown) ||
            Keyboard.GetState().IsKeyDown(Keys.Down)) {
            _hoveredMenuItem = Math.Min(_hoveredMenuItem + 1, _maxHoveredIndex);
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A1) || Input.GetButtonDown(2, Input.ArcadeButtons.A1) ||
            Keyboard.GetState().IsKeyDown(Keys.Enter)) {
            switch (_hoveredMenuItem) {
            case 4:
                Interim interim = new(_game1, this, _concurrentFoods, _speed, _speedMultiplier, _speedIncreaseInterval);
                _game1.AddState(interim);
                _game1.AddState(interim.GetSnek());
                break;
            case 5:
                IGameMode highScores = new HighScores(this, _game1, _font, _fontSize);
                _game1.AddState(highScores);
                break;
            }
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A3) || Input.GetButtonDown(2, Input.ArcadeButtons.A3) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.StickLeft) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickLeft) ||
            Keyboard.GetState().IsKeyDown(Keys.Left)) {
            switch (_hoveredMenuItem) {
            case 0:
                _concurrentFoods = Math.Max(_concurrentFoods - 1, 0);
                break;
            case 1:
                _speed += 0.05;
                break;
            case 2:
                _speedMultiplier += 0.05;
                break;
            case 3:
                _speedIncreaseInterval = Math.Max(_speedIncreaseInterval - 1, 1);
                break;
            case 4:
            case 5:
                break;
            }
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A4) || Input.GetButtonDown(2, Input.ArcadeButtons.A4) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.StickRight) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickRight) ||
            Keyboard.GetState().IsKeyDown(Keys.Right)) {
            switch (_hoveredMenuItem) {
            case 0:
                _concurrentFoods++;
                break;
            case 1:
                _speed = Math.Max(_speed - 0.05, 0.05);
                break;
            case 2:
                _speedMultiplier = Math.Max(_speedMultiplier - 0.05, 0.05);
                break;
            case 3:
                _speedIncreaseInterval++;
                break;
            case 4:
            case 5:
                break;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics) {
        List<string> options = new() {
            "Settings", "Concurrent Foods", $"<{_concurrentFoods}>", "Speed", $"<{1 / _speed:P0}>",
            "Speed Increase", $"<x{1 / _speedMultiplier:F2}>", "Speed Increase Interval", $"<{_speedIncreaseInterval}>",
            "Play", "High Scores", "", "", "", "", "Instructions", "Purple button to go back", "Joystick left/right",
            "or green/white buttons", "to change values", "Joystick up/down to move", "Red button to select",
            "Joystick to change", "direction in game",
        };
        List<int> hoverableIndices = new() { 2, 4, 6, 8, 9, 10 };

        for (int i = 0; i < options.Count; i++) {
            Color color = i == hoverableIndices[_hoveredMenuItem] ? Color.White : Color.Gray;
            spriteBatch.DrawString(_font, options[i],
                new Vector2((graphics.PreferredBackBufferWidth - options[i].Length * _fontSize.X) / 2,
                    _fontSize.Y * 2 * i), color);
        }
    }
}