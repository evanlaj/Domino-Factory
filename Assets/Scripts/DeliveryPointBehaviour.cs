using Models;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class DeliveryPointBehaviour : MonoBehaviour
{
    public List<Domino> distribuedDomino = new List<Domino>();
 
    private int numberOfPlayerAround = 0;

    GameManager gameManager;

    private ParticleSystem particleSystem;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    public void DeliveryDomino(Domino domino)
    {
        distribuedDomino.Add(domino);
        for (var i = gameManager.dominoRequestList.Count - 1; i > -1; i--)
        {
            var dominoReq = gameManager.dominoRequestList[i];
            var res = DominoUtils.isDominoFullfillingRequest(domino, dominoReq);

            if (res)
            {
                gameManager.DeleteDominoRequest(i);
                gameManager.GainScore(dominoReq.RemainingTime/dominoReq.InitialDuration);
                return;
            }
        }

        gameManager.LooseScore();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            numberOfPlayerAround++;
            GetComponent<Animator>().SetInteger("NbsPlayerAround", numberOfPlayerAround);

            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            numberOfPlayerAround--;
            GetComponent<Animator>().SetInteger("NbsPlayerAround", numberOfPlayerAround);

            if(numberOfPlayerAround == 0)
            {
                particleSystem.gameObject.SetActive(false);
                particleSystem.Stop();
            }
        }
    }
}
