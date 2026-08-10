using Microsoft.Xna.Framework;
using MonoGame.Forms.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using DevComponents.DotNetBar;

using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using WzComparerR2.CharaSim;

namespace MapleNecrocer;

public class AvatarFormDraw : MonoGameControl
{
    public AvatarFormDraw()
    {
        Instance = this;
    }
    public static AvatarFormDraw Instance;
    public static RenderTarget2D AvatarPanelTexture;
    private static RenderTarget2D CheckBoardTexture;
    private Vector2 PreviewCamera;

    protected override void Initialize()
    {

        base.Initialize();
        this.AlwaysEnableKeyboardInput = true;
        EngineFunc.Canvas.DrawTarget(ref CheckBoardTexture, 260, 200, () =>
        {
            for (int J = 0; J < 200; J++)
            {
                for (int I = 0; I < 260; I++)
                {
                    if ((I == 0) || (J == 0) || (I == 259) || (J == 199))
                        EngineFunc.Canvas.Pixel(I, J, new Color(0, 0, 0));
                    else if (((I / 8) + (J / 8)) % 2 == 0)  // put checkboard pattern
                        EngineFunc.Canvas.Pixel(I, J, new Color(205, 205, 205));
                    else
                        EngineFunc.Canvas.Pixel(I, J, new Color(255, 255, 255));
                }
            }
        });

        EngineFunc.Canvas.DrawTarget(ref AvatarPanelTexture, 4096, 4096, () => { });
        this.SetMultiSampleCount(0);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Game.Player == null)
            return;

        PreviewCamera = new Vector2(Game.Player.X - 130, Game.Player.Y - 160);
        Vector2 savedCam = EngineFunc.SpriteEngine.Camera;
        EngineFunc.SpriteEngine.Camera = PreviewCamera;

        EngineFunc.Canvas.GraphicsDevice.SetRenderTarget(AvatarPanelTexture);
        EngineFunc.Canvas.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
        EngineFunc.SpriteEngine.DrawEx("Player", "ItemEffect", "SetEffect");
        EngineFunc.Canvas.GraphicsDevice.SetRenderTarget(null);

        EngineFunc.SpriteEngine.Camera = savedCam;
    }

    protected override void Draw()
    {
        EngineFunc.Canvas.Draw(CheckBoardTexture, 0, 0);
        // Editor.graphics.Clear(Color.Aqua);
        int WX = (int)(Game.Player.X - PreviewCamera.X - 130 + MapleChair.BodyRelMove.X - TamingMob.Navel.X);
        int WY = (int)(Game.Player.Y - PreviewCamera.Y - 160 + MapleChair.BodyRelMove.Y - TamingMob.Navel.Y);
        EngineFunc.Canvas.DrawCropArea(AvatarPanelTexture, 0, 0, new Microsoft.Xna.Framework.Rectangle(WX, WY, WX + 280, WY + 200), 0, 0, 1, 1, 0, false, false, 255, 255, 255, 255, false, BlendMode.NonPremultiplied2);
    }

}

