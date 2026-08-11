using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.UI.Forms;
using MapleNecrocer;
using WzComparerR2.WzLib;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
namespace GameUI;
public class MiniMap : UIForm
{
    public static int Version;
    public static bool HasMiniMap;
    int PWidth;
    int cx, cy;
    int OffX, OffY;
    int AddHeight, AddWidth;
    bool HasMark;
    RenderTarget2D RenderTarget;
    Wz_Node PlayerMark;

    static void DrawImage(Wz_Node node, float x, float y)
    {
        if (node != null && Wz.UIImageLib.TryGetValue(node, out var tex))
            EngineFunc.Canvas.Draw(tex, x, y);
    }

    static Texture2D? GetImage(Wz_Node node)
    {
        if (node != null && Wz.UIImageLib.TryGetValue(node, out var tex))
            return tex;
        return null;
    }

    void DrawVersionAlpha()
    {
        Wz_Node UIEntry = Wz.GetNode("UI/UIWindow.img/MiniMap");
        if (UIEntry != null && !Wz.UIData.ContainsKey(UIEntry.FullPathToFile2()))
            Wz.DumpData(UIEntry, Wz.UIData, Wz.UIImageLib);
        int PicWidth, PicHeight;
        var Canvas = EngineFunc.Canvas;
        if (Map.Img.HasNode("miniMap"))
        {
            HasMiniMap = true;
            cx = Map.Img.GetInt("miniMap/centerX");
            cy = Map.Img.GetInt("miniMap/centerY");
            var MiniMapNode = Map.Img.GetNode("miniMap");
            if (MiniMapNode != null && !Wz.UIData.ContainsKey(MiniMapNode.FullPathToFile2()))
                Wz.DumpData(MiniMapNode, Wz.UIData, Wz.UIImageLib);
            var MiniMapPng = Map.Img.GetBmp("miniMap/canvas");
            int canvasW = MiniMapPng?.Width ?? 0;

            PicHeight = MiniMapPng?.Height ?? 100;
            PicWidth = PWidth;
            OffX = canvasW > 0 ? (PicWidth - canvasW) / 2 : 0;
            var Left = ((PicWidth + 13) - canvasW) / 2;
            Canvas.FillRect(7, 72, Left, PicHeight, new Color(128, 128, 128, 128));
            Canvas.FillRect(OffX + 13 + canvasW, 72, Left, PicHeight, new Color(128, 128, 128, 128));
            Canvas.FillRect(OffX + 13, 72, canvasW, PicHeight, new Color(0, 0, 0, 128));
            DrawImage(MiniMapNode?.GetNode("canvas"), 9 + OffX + 3, 72);
        }
        else
        {
            cx = 0;
            cy = 0;
            OffX = 0;
            OffY = 0;
            PicWidth = 150;
            PicHeight = 100;
            Canvas.FillRect(9, 62, PicWidth, PicHeight, new Color(0, 0, 0, 180));
        }

        if (UIEntry != null)
        {
            var nTex = GetImage(UIEntry.GetNode("n"));
            var sTex = GetImage(UIEntry.GetNode("s"));
            var wTex = GetImage(UIEntry.GetNode("w"));
            var eTex = GetImage(UIEntry.GetNode("e"));
            var nwTex = GetImage(UIEntry.GetNode("nw"));
            var neTex = GetImage(UIEntry.GetNode("ne"));
            var swTex = GetImage(UIEntry.GetNode("sw"));
            var seTex = GetImage(UIEntry.GetNode("se"));

            for (int X = 0; X <= PicWidth + 10; X++)
            {
                if (nTex != null) Canvas.Draw(nTex, 4 + X + 3, 0);
                if (sTex != null) Canvas.Draw(sTex, 4 + X + 3, PicHeight + 62 + 10);
            }

            for (int Y = 0; Y <= PicHeight - 1; Y++)
            {
                if (wTex != null) Canvas.Draw(wTex, 1, 72 + Y);
                if (eTex != null) Canvas.Draw(eTex, PicWidth + 18, 72 + Y);
            }
            if (nwTex != null) Canvas.Draw(nwTex, 1, 0); //left top
            if (neTex != null) Canvas.Draw(neTex, PicWidth + 18, 0); //right top
            if (swTex != null) Canvas.Draw(swTex, 1, PicHeight + 72); // right bottom
            if (seTex != null) Canvas.Draw(seTex, PicWidth + 18, PicHeight + 72); // left botton
        }

        if (Wz.HasNode("Map/MapHelper.img/minimap"))
            HasMark = true;

        if (HasMark)
        {
            var MinimapNode = Wz.GetNode("Map/MapHelper.img/minimap");
            if (MinimapNode != null)
            {
                if (!Wz.UIData.ContainsKey(MinimapNode.FullPathToFile2()))
                    Wz.DumpData(MinimapNode, Wz.UIData, Wz.UIImageLib);
                var NpcMark = Wz.GetNode("Map/MapHelper.img/minimap/npc");
                if (Map.Img.HasNode("life"))
                {
                    foreach (var Iter in Map.Img.GetNodes("life"))
                    {
                        if (Iter.GetStr("type") == "n" && Iter.GetInt("hide") != 1)
                            DrawImage(NpcMark, ((Iter.GetInt("x") + cx) / 16)
                              + OffX + 12, ((Iter.GetInt("y") + cy) / 16) + 65);
                    }
                }
                var PortalMark = Wz.GetNode("Map/MapHelper.img/minimap/portal");
                if (Map.Img.HasNode("portal"))
                {
                    foreach (var Iter in Map.Img.GetNodes("portal"))
                    {
                        if (Iter.GetInt("pt") == 2 || Iter.GetInt("pt") == 7)
                            DrawImage(PortalMark, ((Iter.GetInt("x") + cx) /
                              16) + OffX + 10, ((Iter.GetInt("y") + cy) / 16) + 63);
                    }
                }
                PlayerMark = Wz.GetNode("Map/MapHelper.img/minimap/user");
            }
        }
        else
        {
            if (Map.Img.HasNode("portal"))
            {
                foreach (var Iter in Map.Img.GetNodes("portal"))
                {
                    if (Iter.GetInt("pt") == 2 || (Iter.GetInt("pt") == 7))
                    {
                        var X = ((Iter.GetInt("x") + cx) / 16) + OffX + 10;
                        var Y = ((Iter.GetInt("y") + cy) / 16) + 67;
                        Canvas.FillRect(X, Y, 5, 5, new Color(132, 216, 243, 255));
                    }
                }
            }
        }

        var MapMarkName = Map.Img.GetStr("info/mapMark");
        if (MapMarkName != "None")
        {
            var MapMarkPic = Wz.GetNode("Map/MapHelper.img/mark/" + MapMarkName);
            if (MapMarkPic != null)
            {
                if (!Wz.UIData.ContainsKey(MapMarkPic.FullPathToFile2()))
                    Wz.DumpData(MapMarkPic, Wz.UIData, Wz.UIImageLib);
                DrawImage(MapMarkPic, 7, 22);
            }
        }

        if (Map.MapNameList.ContainsKey(Map.ID))
        {
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].StreetName, 49, 26, Color.White);
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].MapName, 49, 43, Color.White);
        }
    }

    void DrawVersion1()
    {
        Wz_Node UIEntry = Wz.GetNode("UI/UIWindow.img/MiniMap/MaxMap");
        if (UIEntry != null && !Wz.UIData.ContainsKey(UIEntry.FullPathToFile2()))
            Wz.DumpData(UIEntry, Wz.UIData, Wz.UIImageLib);
        int PicWidth, PicHeight;
        var Canvas = EngineFunc.Canvas;
        if (Map.Img.HasNode("miniMap"))
        {
            HasMiniMap = true;
            cx = Map.Img.GetInt("miniMap/centerX");
            cy = Map.Img.GetInt("miniMap/centerY");
            var MiniMapNode = Map.Img.GetNode("miniMap");
            if (MiniMapNode != null && !Wz.UIData.ContainsKey(MiniMapNode.FullPathToFile2()))
                Wz.DumpData(MiniMapNode, Wz.UIData, Wz.UIImageLib);
            var MiniMapPng = Map.Img.GetBmp("miniMap/canvas");
            int canvasW = MiniMapPng?.Width ?? 0;

            PicHeight = MiniMapPng?.Height ?? 100;
            PicWidth = PWidth;
            OffX = canvasW > 0 ? (PicWidth - canvasW) / 2 : 0;
            var Left = ((PicWidth + 13) - canvasW) / 2;
            Canvas.FillRect(7, 72, Left, PicHeight, new Color(128, 128, 128, 128));
            Canvas.FillRect(OffX + 13 + canvasW, 72, Left, PicHeight, new Color(128, 128, 128, 128));
            Canvas.FillRect(OffX + 13, 72, canvasW, PicHeight, new Color(0, 0, 0, 128));
            DrawImage(MiniMapNode?.GetNode("canvas"), 9 + OffX + 3, 72);
        }
        else
        {
            cx = 0;
            cy = 0;
            OffX = 0;
            OffY = 0;
            PicWidth = 150;
            PicHeight = 100;
            Canvas.FillRect(9, 62, PicWidth, PicHeight, new Color(0, 0, 0, 180));
        }

        if (UIEntry != null)
        {
            var nTex = GetImage(UIEntry.GetNode("n"));
            var sTex = GetImage(UIEntry.GetNode("s"));
            var wTex = GetImage(UIEntry.GetNode("w"));
            var eTex = GetImage(UIEntry.GetNode("e"));
            var nwTex = GetImage(UIEntry.GetNode("nw"));
            var neTex = GetImage(UIEntry.GetNode("ne"));
            var swTex = GetImage(UIEntry.GetNode("sw"));
            var seTex = GetImage(UIEntry.GetNode("se"));

            for (int X = 0; X <= PicWidth + 10; X++)
            {
                if (nTex != null) Canvas.Draw(nTex, 4 + X + 3, 0);
                if (sTex != null) Canvas.Draw(sTex, 4 + X + 3, PicHeight + 62 + 10);
            }

            for (int Y = 0; Y <= PicHeight - 1; Y++)
            {
                if (wTex != null) Canvas.Draw(wTex, 1, 72 + Y);
                if (eTex != null) Canvas.Draw(eTex, PicWidth + 18, 72 + Y);
            }
            if (nwTex != null) Canvas.Draw(nwTex, 1, 0); //left top
            if (neTex != null) Canvas.Draw(neTex, PicWidth + 18, 0); //right top
            if (swTex != null) Canvas.Draw(swTex, 1, PicHeight + 72); // right bottom
            if (seTex != null) Canvas.Draw(seTex, PicWidth + 18, PicHeight + 72); // left botton
        }

        if (Wz.HasNode("Map/MapHelper.img/minimap"))
            HasMark = true;

        if (HasMark)
        {
            var MinimapNode = Wz.GetNode("Map/MapHelper.img/minimap");
            if (MinimapNode != null)
            {
                if (!Wz.UIData.ContainsKey(MinimapNode.FullPathToFile2()))
                    Wz.DumpData(MinimapNode, Wz.UIData, Wz.UIImageLib);
                var NpcMark = Wz.GetNode("Map/MapHelper.img/minimap/npc");
                if (Map.Img.HasNode("life"))
                {
                    foreach (var Iter in Map.Img.GetNodes("life"))
                    {
                        if (Iter.GetStr("type") == "n" && Iter.GetInt("hide") != 1)
                            DrawImage(NpcMark, ((Iter.GetInt("x") + cx) / 16)
                              + OffX + 12, ((Iter.GetInt("y") + cy) / 16) + 65);
                    }
                }
                var PortalMark = Wz.GetNode("Map/MapHelper.img/minimap/portal");
                if (Map.Img.HasNode("portal"))
                {
                    foreach (var Iter in Map.Img.GetNodes("portal"))
                    {
                        if (Iter.GetInt("pt") == 2 || Iter.GetInt("pt") == 7)
                            DrawImage(PortalMark, ((Iter.GetInt("x") + cx) /
                              16) + OffX + 10, ((Iter.GetInt("y") + cy) / 16) + 63);
                    }
                }
                PlayerMark = Wz.GetNode("Map/MapHelper.img/minimap/user");
            }
        }
        else
        {
            if (Map.Img.HasNode("portal"))
            {
                foreach (var Iter in Map.Img.GetNodes("portal"))
                {
                    if (Iter.GetInt("pt") == 2 || (Iter.GetInt("pt") == 7))
                    {
                        var X = ((Iter.GetInt("x") + cx) / 16) + OffX + 10;
                        var Y = ((Iter.GetInt("y") + cy) / 16) + 67;
                        Canvas.FillRect(X, Y, 5, 5, new Color(132, 216, 243, 255));
                    }
                }
            }
        }

        var MapMarkName = Map.Img.GetStr("info/mapMark");
        if (MapMarkName != "None")
        {
            var MapMarkPic = Wz.GetNode("Map/MapHelper.img/mark/" + MapMarkName);
            if (MapMarkPic != null)
            {
                if (!Wz.UIData.ContainsKey(MapMarkPic.FullPathToFile2()))
                    Wz.DumpData(MapMarkPic, Wz.UIData, Wz.UIImageLib);
                DrawImage(MapMarkPic, 7, 22);
            }
        }

        if (Map.MapNameList.ContainsKey(Map.ID))
        {
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].StreetName, 49, 26, Color.White);
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].MapName, 49, 43, Color.White);
        }
    }

    void DrawVersion3()
    {
        Wz_Node UIEntry = Wz.GetNodeA("UI/UIWindow2.img/MiniMap/MaxMap");
        // Wz.UIImageLib.Clear();
        if (UIEntry != null && !Wz.UIData.ContainsKey("UI/UIWindow2.img/MiniMap/MaxMap"))
            Wz.DumpData(UIEntry, Wz.UIData, Wz.UIImageLib);
        int PicWidth, PicHeight;
        var Canvas = EngineFunc.Canvas;
        if (Map.Img.HasNode("miniMap"))
        {
            HasMiniMap = true;
            cx = Map.Img.GetInt("miniMap/centerX");
            cy = Map.Img.GetInt("miniMap/centerY");
            var MiniMapNode = Map.Img.GetNode("miniMap");
            if (MiniMapNode != null && !Wz.UIData.ContainsKey(MiniMapNode.FullPathToFile2()))
                Wz.DumpData(MiniMapNode, Wz.UIData, Wz.UIImageLib);
            var MiniMapPng = Map.Img.GetBmp("miniMap/canvas");
            int canvasW = MiniMapPng?.Width ?? 0;

            PicHeight = MiniMapPng?.Height ?? 100;
            PicWidth = PWidth;
            OffX = canvasW > 0 ? (PicWidth - canvasW) / 2 : 0;
            Canvas.FillRect(9, 62, PicWidth, PicHeight, new Color(0, 0, 0, 180));
            DrawImage(MiniMapNode?.GetNode("canvas"), 9 + OffX, 62);
        }
        else
        {
            cx = 0;
            cy = 0;
            OffX = 0;
            OffY = 0;
            PicWidth = 150;
            PicHeight = 100;
            Canvas.FillRect(9, 62, PicWidth, PicHeight, new Color(0, 0, 0, 180));
        }

        if (UIEntry != null)
        {
            var nTex = GetImage(UIEntry.GetNode("n"));
            var sTex = GetImage(UIEntry.GetNode("s"));
            var wTex = GetImage(UIEntry.GetNode("w"));
            var eTex = GetImage(UIEntry.GetNode("e"));
            var nwTex = GetImage(UIEntry.GetNode("nw"));
            var neTex = GetImage(UIEntry.GetNode("ne"));
            var swTex = GetImage(UIEntry.GetNode("sw"));
            var seTex = GetImage(UIEntry.GetNode("se"));

            for (int X = 0; X <= PicWidth - 111; X++)
            {
                if (nTex != null) Canvas.Draw(nTex, 64 + X, 0);
                if (sTex != null) Canvas.Draw(sTex, 64 + X, PicHeight + 62);
            }

            for (int Y = 0; Y <= PicHeight - 24; Y++)
            {
                if (wTex != null) Canvas.Draw(wTex, 0, 67 + Y);
                if (eTex != null) Canvas.Draw(eTex, PicWidth + 9, 67 + Y);
            }
            if (nwTex != null) Canvas.Draw(nwTex, 0, 0); //left top
            if (neTex != null) Canvas.Draw(neTex, PicWidth - 46, 0); //right top
            if (swTex != null) Canvas.Draw(swTex, 0, PicHeight + 44); // right bottom
            if (seTex != null) Canvas.Draw(seTex, PicWidth - 46, PicHeight + 44); // left botton
        }

        var MinimapNode = Wz.GetNode("Map/MapHelper.img/minimap");
        if (MinimapNode != null && !Wz.UIData.ContainsKey(MinimapNode.FullPathToFile2()))
            Wz.DumpData(MinimapNode, Wz.UIData, Wz.UIImageLib);

        var NpcMark = Wz.GetNodeA("Map/MapHelper.img/minimap/npc");
        if (Map.Img.HasNode("life"))
        {
            foreach (var Iter in Map.Img.GetNodes("life"))
            {
                if (Iter.GetStr("type") == "n" && Iter.GetInt("hide") != 1)
                    DrawImage(NpcMark, ((Iter.GetInt("x") + cx) / 16)
                      + OffX + 4, ((Iter.GetInt("y") + cy) / 16) + 50);
            }
        }

        var PortalMark = Wz.GetNodeA("Map/MapHelper.img/minimap/portal");
        if (Map.Img.HasNode("portal"))
        {
            foreach (var Iter in Map.Img.GetNodes("portal"))
            {
                if (Iter.GetInt("pt") == 2 || Iter.GetInt("pt") == 7)
                    DrawImage(PortalMark, ((Iter.GetInt("x") + cx) /
                      16) + OffX + 2, ((Iter.GetInt("y") + cy) / 16) + 48);
            }
        }

        var MapMarkName = Map.Img.GetStr("info/mapMark");
        if (MapMarkName != "None")
        {
            var MapMarkPic = Wz.GetNodeA("Map/MapHelper.img/mark/" + MapMarkName);
            if (MapMarkPic != null)
            {
                if (!Wz.UIData.ContainsKey(MapMarkPic.FullPathToFile2()))
                    Wz.DumpData(MapMarkPic, Wz.UIData, Wz.UIImageLib);
                DrawImage(MapMarkPic, 7, 17);
            }
        }
        PlayerMark = Wz.GetNodeA("Map/MapHelper.img/minimap/user");

        if (Map.MapNameList.ContainsKey(Map.ID))
        {
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].StreetName, 50, 20, Color.White);
            Canvas.DrawString(Map.NpcNameTagFont, Map.MapNameList[Map.ID].MapName, 50, 37, Color.White);
        }
    }

    public void RenderTargetFunc()
    {
        switch (Version)
        {
            case 0:
                DrawVersionAlpha();
                break;
            case 1:
                DrawVersion1();
                break;
            case 3:
                DrawVersion3();
                break;
        }
    }

    public void ReDraw()
    {
        float Length = 0;
        float Length1 = 0, Length2 = 0;

        if (Map.MapNameList.ContainsKey(Map.ID))
        {
            Length1 = Map.MeasureStringX(Map.NpcNameTagFont, Map.MapNameList[Map.ID].StreetName);
            Length2 = Map.MeasureStringX(Map.NpcNameTagFont, Map.MapNameList[Map.ID].MapName);
        }
        Length = Math.Max(Length1, Length2);

        if (Version == 1)
        {
            AddWidth = -50;
            AddHeight = 12;
        }
        else
        {
            AddWidth = 0;
            AddHeight = 0;
        }

        if (Map.Img.HasNode("miniMap"))
        {
            var MiniMapPng = Map.Img.GetBmp("miniMap/canvas");
            PWidth = Math.Max((int)Length, (MiniMapPng?.Width ?? 0) + AddWidth) + 40;
            EngineFunc.Canvas.DrawTarget(ref RenderTarget, PWidth + 50, (MiniMapPng?.Height ?? 80) + 80 + AddHeight, () => RenderTargetFunc());
            this.Size = new Microsoft.Xna.Framework.Vector2(PWidth + 20, (MiniMapPng?.Height ?? 40) + 40);
        }
        else
        {
            EngineFunc.Canvas.DrawTarget(ref RenderTarget, 1, 1, () => RenderTargetFunc());
            this.Size = new Microsoft.Xna.Framework.Vector2(1, 1);
        }
    }

    internal override void DoDraw(Vector2 offset)
    {
        if (!Map.ShowMiniMap)
            return;
        if (!IsVisible)
            return;

        if (HasMiniMap && RenderTarget != null)
        {
            SpriteBatch.Draw(RenderTarget, new Vector2(Location.X, Location.Y), Color.White);
            int px = (int)(Game.Player.X + cx) / 16;
            int py = (int)(Game.Player.Y + cy) / 16;
            bool playerMarkReady = PlayerMark != null && Wz.UIImageLib.ContainsKey(PlayerMark);
            if (Version == 1 || Version == 0)
            {
                if (HasMark && playerMarkReady)
                    SpriteBatch.Draw(Wz.UIImageLib[PlayerMark], new Vector2(Location.X + px + OffX + 2 + 8, Location.Y + py + OffY + 50 + 15), Color.White);
                else
                    SpriteBatch.FillRectangle(new Microsoft.Xna.Framework.Rectangle((int)Location.X + px + OffX + 2 + 8, (int)Location.Y + py + OffY +
                      50 + 17, 5, 5), new Color(0, 255, 255, 255));
            }
            else
            {
                if (playerMarkReady)
                    SpriteBatch.Draw(Wz.UIImageLib[PlayerMark], new Vector2(Location.X + px + OffX + 2, Location.Y + py + OffY + 50), Color.White);
            }
        }

        foreach (var control in Controls)
        {
            if (control.IsVisible)
                control.DoDraw(Location);
        }
    }

}
