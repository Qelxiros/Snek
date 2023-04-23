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
    private SpriteBatch _spriteBatch;
    private Rectangle _windowSize;
    private IGameMode _activeState;

    /// <summary>
    ///     Game constructor
    /// </summary>
    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _activeState = new Landing(this);
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

        _windowSize = GraphicsDevice.Viewport.Bounds;
        _activeState.Initialize(_windowSize.Width, _windowSize.Height);

        base.Initialize();
    }

    /// <summary>
    ///     Does any setup prior to the first frame that needs loaded content.
    /// </summary>
    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
            
        _activeState.LoadContent(this, Content);
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
             Input.GetButton(2, Input.ArcadeButtons.Menu))) {
            Exit();
        }

        _activeState.Update();
        
        base.Update(gameTime);
    }

    /// <summary>
    ///     Your main draw loop. This runs once every frame, over and over.
    /// </summary>
    /// <param name="gameTime">This is the gameTime object you can use to get the time since last frame.</param>
    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        _activeState.Draw(_spriteBatch, _graphics);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void AddState(IGameMode state) {
        state.Initialize(_windowSize.Width, _windowSize.Height);
        state.LoadContent(this, Content);
        _activeState = state;
    }

    public void ReturnToState(IGameMode fallback) {
        _activeState = fallback;
    }

    // me when save-load doesn't work
    // public static void HandleScore(long score) {
    //     string scoreString = SaveManager.Instance.LoadText("scores");
    //     List<long> scores = scoreString.Split(",").Select(long.Parse).ToList();
    //     scores.Add(score);
    //     scores.Sort();
    //     if (scores.Count > 5) {
    //         scores.RemoveAt(5);
    //     }
    //
    //     string newScores = string.Join(",", scores);
    //     SaveManager.Instance.SaveText("scores", newScores);
    // }
}