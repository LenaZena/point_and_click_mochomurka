using UnityEngine;
using System.Collections;

public class InteractiveItem : MonoBehaviour
{
    [Header("zvednuti")]
    public float liftHeight = 1.5f;   
    public float waitTime = 1.0f;     
    
    [Header("Animace pohybu")]
    public float liftDuration = 0.3f; 
    public float dropDuration = 0.15f; 
    public float wobbleAngle = 2f; 

    [Header("klic")]
    public Collider2D hiddenItemCollider; // collider klice schovaneho pod kytkou

    [Header("outline")]
    public GameObject[] outlineFrames; 
    public float animationSpeed = 0.2f; 

    [Header("Zvuky")]
    public AudioClip liftSound; 
    public AudioClip dropSound; 
    [Range(0f, 1f)] public float soundVolume = 1f; 
    
    [Header("strih zvuku")]
    public float liftSoundStartTime = 0f; 
    public float dropSoundStartTime = 0f;

    private AudioSource audioSrc; 
    private Vector3 originalPos;
    private Vector3 originalScale;
    private Quaternion originalRot;
    private bool isLifting = false;
    private Coroutine animateCoroutine; 

    void Start()
    {
        // schovat originalni pozici/rotaci na potom
        originalPos = transform.position;
        originalScale = transform.localScale;
        originalRot = transform.rotation;
        
        TurnOffOutlines();

        // klic pod kytkou nejde kliknout dokud se kytkou nezvedne
        if (hiddenItemCollider != null) hiddenItemCollider.enabled = false;

        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false; 
    }

    void OnMouseEnter()
    {
        if (!isLifting && animateCoroutine == null)
            animateCoroutine = StartCoroutine(AnimateOutline());
    }

    void OnMouseExit()
    {
        if (!isLifting)
        {
            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
                animateCoroutine = null;
            }
            TurnOffOutlines();
        }
    }

    void OnMouseDown()
    {
        // zvednout kytku po kliknuti
        if (!isLifting) StartCoroutine(LiftAndDropJuicy());
    }

    IEnumerator AnimateOutline()
    {
        int currentFrame = 0;
        while (true) 
        {
            TurnOffOutlines();
            if (outlineFrames.Length > 0 && outlineFrames[currentFrame] != null) outlineFrames[currentFrame].SetActive(true);
            currentFrame++;
            if (currentFrame >= outlineFrames.Length) currentFrame = 0;
            yield return new WaitForSeconds(animationSpeed);
        }
    }

    void TurnOffOutlines()
    {
        foreach (GameObject frame in outlineFrames)
            if (frame != null) frame.SetActive(false);
    }

    // zvednuti, cekani a hozeni kytky zpatky na zem + bouncy efekt
    IEnumerator LiftAndDropJuicy()
    {
        isLifting = true;
        
        if (liftSound != null) 
        {
            audioSrc.clip = liftSound;
            audioSrc.volume = soundVolume;
            audioSrc.time = liftSoundStartTime; 
            audioSrc.Play();
        }

        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
            animateCoroutine = null;
        }
        TurnOffOutlines();

        Vector3 targetPos = originalPos + new Vector3(0, liftHeight, 0);
        float elapsed = 0f;

        // leti to nahoru + trochu se to kýve
        while (elapsed < liftDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / liftDuration;
            float easeOut = 1f - (1f - t) * (1f - t); 
            transform.position = Vector3.Lerp(originalPos, targetPos, easeOut);
            
            float wobble = Mathf.Sin(t * Mathf.PI * 4f) * wobbleAngle; 
            transform.rotation = originalRot * Quaternion.Euler(0, 0, wobble);
            yield return null;
        }
        
        transform.position = targetPos;
        transform.rotation = originalRot;

        // kytka je nahore -> zapnout collider pro skryty klic pod ni
        if (hiddenItemCollider != null) hiddenItemCollider.enabled = true;

        yield return new WaitForSeconds(waitTime);

        // pokud hrac klic nevzal, zase schovat collider at neproklikava skrz
        if (hiddenItemCollider != null && !KeyItem.isHoldingKey) hiddenItemCollider.enabled = false;

        // pada to dolu
        elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            float easeIn = t * t * t; 
            transform.position = Vector3.Lerp(targetPos, originalPos, easeIn);
            yield return null;
        }
        
        transform.position = originalPos;

        if (dropSound != null) 
        {
            audioSrc.clip = dropSound;
            audioSrc.volume = soundVolume;
            audioSrc.time = dropSoundStartTime; 
            audioSrc.Play();
        }

        // squish a stretch efekt pri dopadu na zem (bouncování scale)
        transform.localScale = new Vector3(originalScale.x * 1.25f, originalScale.y * 0.75f, originalScale.z);
        yield return new WaitForSeconds(0.05f);
        transform.localScale = new Vector3(originalScale.x * 0.9f, originalScale.y * 1.15f, originalScale.z);
        transform.rotation = originalRot * Quaternion.Euler(0, 0, 4f);
        yield return new WaitForSeconds(0.05f);
        transform.localScale = originalScale;
        transform.rotation = originalRot * Quaternion.Euler(0, 0, -3f);
        yield return new WaitForSeconds(0.05f);
        transform.rotation = originalRot;
        
        isLifting = false;
    }
}