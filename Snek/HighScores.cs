using System.Collections.Generic;
using Devcade;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snek;

public class HighScores : IGameMode
{
    private readonly Game1 _game1;
    private readonly IGameMode _menu;
    private readonly SpriteFont _menuFont;
    private readonly Vector2 _menuFontSize;
    private List<long> _scores;

    public HighScores(IGameMode menu, Game1 game1, SpriteFont menuFont, Vector2 menuFontSize)
    {
        _menu = menu;
        _game1 = game1;
        _menuFont = menuFont;
        _menuFontSize = menuFontSize;
    }

    public void Initialize(double width, double height)
    {
        // uncomment this when save-load exists
        // _scores = SaveManager.Instance.LoadText("scores").Split(",").Select(long.Parse).ToList();
    }

    public void LoadContent(Game game, ContentManager content)
    {
    }

    public void Update()
    {
        if (Input.GetButtonDown(1, Input.ArcadeButtons.A2) || Input.GetButtonDown(2, Input.ArcadeButtons.A2) ||
            Keyboard.GetState().IsKeyDown(Keys.F))
            _game1.ReturnToState(_menu);
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
    {
        // for (int i = 0; i < _scores.Count; i++) {
        //     string score = _scores[i].ToString();
        //     spriteBatch.DrawString(_menuFont, score,
        //         new Vector2((graphics.PreferredBackBufferWidth - score.Length * _menuFontSize.X) / 2,
        //             _menuFontSize.Y * 2 * i), Color.Gray);
        // }
        spriteBatch.DrawString(_menuFont, "Error 501", new Vector2(0, 0), Color.White);
        spriteBatch.DrawString(_menuFont, "Blue button to go back", new Vector2(0, _menuFontSize.Y * 2), Color.White);
    }
}