using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Snek;

public interface IGameMode
{
    /**
     * All non-content related initialization should be done here. Called immediately on startup.
     * This method should include the creation of all game objects.
     */
    public void Initialize(double width, double height);

    /**
    * All content for this game mode should be loaded here.
    */
    public void LoadContent(Game game, ContentManager content);

    /**
     * All update logic for this game mode should be done here. This will only be called when this game mode is the active game mode.
     * This will run before draw.
     */
    public void Update();

    /**
     * All drawing logic for this game mode should be done here. This will only be called when this game mode is the active game mode.
     * This will run after update.
     */
    public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics);
}