using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek; 

public class Landing : IGameMode {
    private SpriteFont _font;
    private Vector2 _fontSize;
    private readonly Game1 _game1;

    public Landing(Game1 game1) {
        _game1 = game1;
    }
    
    public void Initialize(double width, double height) {
        
    }

    public void LoadContent(Game game, ContentManager content) {
        _font = content.Load<SpriteFont>("menu");
        _fontSize = _font.MeasureString("0");
    }

    public void Update() {
        if (Input.GetButtonDown(2, Input.ArcadeButtons.B1) || Input.GetButtonDown(2, Input.ArcadeButtons.B2) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.B3) || Input.GetButtonDown(2, Input.ArcadeButtons.B4) ||
            Keyboard.GetState().IsKeyDown(Keys.P)) {
            _game1.AddState(new Menu(this, _game1));
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics) {
        List<string> options = new() {
            "Welcome to Snek", "", "Press yellow button","to start",
        };

        for (int i = 0; i < options.Count; i++) {
            Color color = Color.Gray;
            spriteBatch.DrawString(_font, options[i],
                new Vector2((graphics.PreferredBackBufferWidth - options[i].Length * _fontSize.X) / 2,
                    _fontSize.Y * 2 * i), color);
        }
    }
}