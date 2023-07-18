using Models;
using UnityEngine;
using Utils;

public class HologramBehavior : MonoBehaviour
{
    Domino _domino;
    public Domino domino { get => _domino; }

    DominoGenerator dominoGenerator;

    private void Start()
    {
        dominoGenerator = FindObjectOfType<DominoGenerator>().GetComponent<DominoGenerator>();
    }


    public void SetDomino(Domino domino)
    {
        if (dominoGenerator == null)
            dominoGenerator = FindObjectOfType<DominoGenerator>().GetComponent<DominoGenerator>();
        
        this._domino = domino;
        var hologramSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        hologramSpriteRenderer.sprite = dominoGenerator.GenerateDominoSprite(domino, false);
    }
}
