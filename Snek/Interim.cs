using System;
using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

public class Interim : IGameMode {
    private readonly int _concurrentFoods;
    private readonly Game1 _game1;
    private readonly Menu _menu;
    private readonly double _speed;
    private readonly int _speedIncreaseInterval;
    private readonly double _speedMultiplier;
    private SpriteFont _font;
    private Vector2 _fontSize;
    private SpriteFont _scoreFont;
    private Vector2 _scoreFontSize;

    public long Score { get; set; }

    public Interim(Game1 game1, Menu menu, int concurrentFoods, double speed, double speedMultiplier,
        int speedIncreaseInterval) {
        _game1 = game1;
        _menu = menu;
        _concurrentFoods = concurrentFoods;
        _speed = speed;
        _speedMultiplier = speedMultiplier;
        _speedIncreaseInterval = speedIncreaseInterval;
    }

    public void Initialize(double width, double height) {
        Score = 0;
    }

    public void LoadContent(Game game, ContentManager content) {
        _font = content.Load<SpriteFont>("menu");
        _fontSize = _font.MeasureString("0");
        _scoreFont = content.Load<SpriteFont>("score");
        _scoreFontSize = _scoreFont.MeasureString("0"); // the font is monospace so this works
    }

    public void Update() {
        if (Input.GetButtonDown(1, Input.ArcadeButtons.B1) || Input.GetButtonDown(1, Input.ArcadeButtons.B2) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.B3) || Input.GetButtonDown(1, Input.ArcadeButtons.B4) ||
            Keyboard.GetState().IsKeyDown(Keys.P)) {
            _game1.ReturnToState(_menu);
        }

        if (Input.GetButtonDown(1, Input.ArcadeButtons.A1) || Input.GetButtonDown(2, Input.ArcadeButtons.A1) ||
            Keyboard.GetState().IsKeyDown(Keys.Enter)) {
            _game1.AddState(GetSnek());
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics) {
        List<string> options = new() {
            "Press purple button", "to return to menu", "", "Press red button", "to quick reset",
        };

        for (int i = 0; i < options.Count; i++) {
            Color color = Color.Gray;
            spriteBatch.DrawString(_font, options[i],
                new Vector2((graphics.PreferredBackBufferWidth - options[i].Length * _fontSize.X) / 2,
                    _fontSize.Y * 2 * i), color);
        }
        
        string scoreString = Score.ToString();

        spriteBatch.DrawString(_scoreFont, scoreString,
            new Vector2((graphics.PreferredBackBufferWidth - _scoreFontSize.X * scoreString.Length) / 2,
                (graphics.PreferredBackBufferHeight - _scoreFontSize.Y) / 2),
            Color.DarkSlateGray);
    }

    public Snek GetSnek() {
        return new Snek(this, _game1, _concurrentFoods, _speed, _speedMultiplier, _speedIncreaseInterval);
    }
}