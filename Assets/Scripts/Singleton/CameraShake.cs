using UnityEngine;

public class CameraShake : Singleton<CameraShake>
{
    public Camera mainCam;
    float shakeAmmount = 0;
    public Vector3 defaultPosition;

    public void LittleShake()
    {
        Shake(0.01f, 0.05f); 
    }

    public void MediumShake()
    {
        Shake(0.025f, 0.08f); 
    }

    public void BigShake()
    {
        Shake(0.035f, 0.1f); 
    }

    public void Shake(float ammount, float length)
    {
        if(mainCam == null){
            mainCam = Camera.main;
            defaultPosition = mainCam.transform.localPosition;
        }

        shakeAmmount = ammount;
        InvokeRepeating(nameof(DoShake), 0, 0.01f);
        Invoke(nameof(StopShake), length);
    }

    void DoShake()
    {
        if(shakeAmmount>0)
        {
            Vector3 camPosition = mainCam.transform.position;

            float offSetX = Random.value * shakeAmmount * 2 - shakeAmmount;
            float offSetY = Random.value * shakeAmmount * 2 - shakeAmmount;

            camPosition.x += offSetX;
            camPosition.y += offSetY;

            mainCam.transform.position = camPosition;
        }
    }

    void StopShake()
    {
        CancelInvoke(nameof(DoShake));
        mainCam.transform.localPosition = defaultPosition;
    }
}
