using Enums;
using Models;
using System;
using UnityEngine;
using Utils;

public class DominoBehavior : MonoBehaviour
{
    [SerializeField]
    private bool setRandomDomino = false;

    // get own SpriteRenderer
    SpriteRenderer spriteRenderer;
    GameManager gameManager;
    public Domino domino;

    int paintingLayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();

        if (setRandomDomino)
            domino = new Domino(DominoUtils.GetRandomValidDomino(), DominoUtils.GetRandomColor());

        Debug.Log("Domino: " + domino.Blocks[0][0].Color + "\n" + DominoUtils.PrintDomino(domino.Blocks));

        SetSpriteAndCollider();
    }

    public void SetDomino(Domino domino)
    {
        this.domino = domino;
        SetSpriteAndCollider();
    }

    public void RotateDominoClockwise()
    {
        domino.Blocks = DominoUtils.RotateDominoClockwise(domino.Blocks);
        SetSpriteAndCollider();
    }

    public void RotateDominoCounterClockwise()
    {
        domino.Blocks = DominoUtils.RotateDominoCounterClockwise(domino.Blocks);
        SetSpriteAndCollider();
    }

    void SetSpriteAndCollider()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = gameManager.GenerateDominoSprite(domino);

        Sprite sprite = spriteRenderer.sprite;
        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();

        //Set BoxCollider2D to sprite size
        Rect croppedRect = new Rect(
          (sprite.textureRectOffset.x + sprite.textureRect.width / 2f) / sprite.pixelsPerUnit,
          (sprite.textureRectOffset.y + sprite.textureRect.height / 2f) / sprite.pixelsPerUnit,
          sprite.textureRect.width / sprite.pixelsPerUnit,
          sprite.textureRect.height / sprite.pixelsPerUnit);

        // offset is relative to sprite's pivot
        collider.offset = croppedRect.position - sprite.pivot / sprite.pixelsPerUnit;
        collider.size = croppedRect.size;
    }

    //Fonctions utilisés dans l'assembleur
    public void MoveDominoUp()
    {
        domino.Blocks = DominoUtils.MoveDominoUp(domino.Blocks);
    }
    public void MoveDominoRight()
    {
        domino.Blocks = DominoUtils.MoveDominoRight(domino.Blocks);
    }
    public void MoveDominoLeft()
    {
        domino.Blocks = DominoUtils.MoveDominoLeft(domino.Blocks);
    }
    public void MoveDominoDown()
    {
        domino.Blocks = DominoUtils.MoveDominoDown(domino.Blocks);
    }

    internal void AddColor(bool isPainterBlue, bool isPainterRed)
    {
        paintingLayer++;

        if(domino.isAssembled)
        {
            domino.SetColor(BlockColor.Failed);
            SetSpriteAndCollider();
            return;
        }

        if (isPainterBlue)
        {
            var color = domino.GetColor();
            if (color == BlockColor.Failed || color == BlockColor.Red || color == BlockColor.LightRed)
            {
                domino.SetColor(BlockColor.Failed);
            }
            else
            {
                switch (paintingLayer)
                {
                    case 1:
                        domino.SetColor(BlockColor.LightBlue);
                        break;
                    case 2:
                        domino.SetColor(BlockColor.Blue);
                        break;
                    case 3:
                        domino.SetColor(BlockColor.Failed);
                        break;
                }
            }

        }
        else if (isPainterRed)
        {
            var color = domino.GetColor();
            if (color == BlockColor.Failed || color == BlockColor.Blue || color == BlockColor.LightBlue)
            {
                domino.SetColor(BlockColor.Failed);
            }
            else
            {
                switch (paintingLayer)
                {
                    case 1:
                        domino.SetColor(BlockColor.LightRed);
                        break;
                    case 2:
                        domino.SetColor(BlockColor.Red);
                        break;
                    case 3:
                        domino.SetColor(BlockColor.Failed);
                        break;
                }
            }
        }
        SetSpriteAndCollider();
    }
}
