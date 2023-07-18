using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Enums;
using Models;

public class DominoGenerator : MonoBehaviour
{
    [SerializeField] private Sprite defaultBlockSprite;
    [SerializeField] private Sprite redBlockSprite;
    [SerializeField] private Sprite greenBlockSprite;
    [SerializeField] private Sprite blueBlockSprite;
    [SerializeField] private Sprite yellowBlockSprite;
    [SerializeField] private Sprite cyanBlockSprite;
    [SerializeField] private Sprite purpleBlockSprite;
    [SerializeField] private Sprite blackBlockSprite;

    private static readonly int spriteSize = 256;
    private static readonly uint spritePadding = (uint) Mathf.Floor(1.25f*(spriteSize/32));
    private static readonly int blockPixelSize = spriteSize/32;
    private static readonly int blockSizeY = 6 * blockPixelSize;
    private static readonly int blockSizeX = 7 * blockPixelSize;

    private static readonly int blockSideSize = 5 * blockPixelSize;
    private static readonly int fullBlockHeight = blockSizeY + blockSideSize;

    public Sprite GenerateDominoSprite(Domino domino, bool centerSprite = true)
    {   
        var minArea = centerSprite ? DominoUtils.GetMinimumDominoArea(domino): domino;

        int dominoPixelHeight = (minArea.Blocks.Length + 1) * blockSizeY;
        int dominoPixelWidth = minArea.Blocks[0].Length * blockSizeX;

        int dominoPaddingLeft = (int) Mathf.Floor((spriteSize - dominoPixelWidth) / 2);
        int dominoPaddingBottom = (int) Mathf.Floor((spriteSize - dominoPixelHeight) / 2);

        Resources.UnloadUnusedAssets();
        Color32 transparent = new Color32(0, 0, 0, 0);

        var newTexture = new Texture2D(spriteSize, spriteSize, TextureFormat.RGBA32, false);

        // INIT BACKGROUND

        var pixelArray = new Color32[spriteSize*spriteSize];

        for (var i = 0; i < pixelArray.Length; i++)
            pixelArray[i] = transparent;

        newTexture.SetPixels32(pixelArray);

        // DRAW BLOCKS

        for (var blockY = 0; blockY < minArea.Blocks.Length; blockY++) {
            for (var blockX = 0; blockX < minArea.Blocks[blockY].Length; blockX++) {
                
                if (!minArea.Blocks[blockY][blockX].Exists) continue;

                var blockSprite = GetSpriteFromColor(minArea.Blocks[blockY][blockX].Color);
                
                var posX = dominoPaddingLeft + blockX * blockSizeX;
                var posY = spriteSize - 1 - dominoPaddingBottom - (blockY * (blockSizeY-1)) - fullBlockHeight;
                Graphics.CopyTexture(blockSprite.texture, 0, 0, 0, 0, blockSizeX, fullBlockHeight, newTexture, 0, 0, posX, posY);
            }
        }

        // CONFIG TEXTURE & SPRITE

        newTexture.filterMode = FilterMode.Point;
        newTexture.wrapMode = TextureWrapMode.Clamp;

        newTexture.Apply();

        var finalSprite = Sprite.Create(newTexture, new Rect(0, 0, spriteSize, spriteSize), new Vector2(0.5f, 0.5f), spriteSize, spritePadding, SpriteMeshType.Tight);
        finalSprite.name = "DominoSprite";
        return finalSprite;
    }

    private Sprite GetSpriteFromColor(BlockColor color)
    {
        switch (color)
        {
            case BlockColor.Red:
                return redBlockSprite;
            case BlockColor.Green:
                return greenBlockSprite;
            case BlockColor.Blue:
                return blueBlockSprite;
            case BlockColor.Purple:
                return purpleBlockSprite;
            case BlockColor.Yellow:
                return yellowBlockSprite;
            case BlockColor.Cyan:
                return cyanBlockSprite;
            case BlockColor.Failed:
                return blackBlockSprite;
            default:
                return defaultBlockSprite;
        }
    }
}
