namespace MapleNecrocer;

public static class SpriteFit
{
    public const int Margin = 2;

    public static float FitScale(int width, int height, int maxWidth, int maxHeight)
    {
        if (width <= 0 || height <= 0)
            return 1f;
        float scale = Math.Min((float)(maxWidth - Margin) / width, (float)(maxHeight - Margin) / height);
        return Math.Min(scale, 1f);
    }

    public static bool FootprintOverflows(int posX, int posY, int width, int height, int frameW, int frameH)
    {
        return posX < 0 || posY < 0 || posX + width > frameW || posY + height > frameH;
    }
}
