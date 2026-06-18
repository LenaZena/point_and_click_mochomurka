using UnityEngine;
using System.Collections;

public class DoorItem : MonoBehaviour
{
    [Header("outline")]
    public GameObject[] outlineFrames;
    public float outlineSpeed = 0.2f;

    [Header("otaznik")]
    public GameObject[] thoughtFrames; 
    public float thoughtAnimationSpeed = 0.3f; 
    public float thoughtDisplayTime = 2.5f;    

    [Header("Konec Hry")]
    public GameObject playerCharacter; 
    public Transform walkTarget;       
    public float walkSpeed = 3f;       
    public float waveDuration = 1.5f;  
    public float fadeOutSpeed = 1.0f;  

    [Header("Zvuky")]
    public AudioClip lockedSound;
    public AudioClip unlockSound;
    public AudioClip goodbyeSound;
    
    [Range(0f, 1f)] public float soundVolume = 1f; 
    
    private AudioSource audioSrc;
    private Coroutine outlineCoroutine;
    private Coroutine thoughtCoroutine;
    public bool isUnlocked = false; 

    void Start()
    {
        // na zacatku vsechno schovat
        TurnOffArray(outlineFrames);
        TurnOffArray(thoughtFrames);
        
        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    void OnMouseEnter()
    {
        // mys na dverich -> pustit animaci outlinu
        if (outlineCoroutine == null)
            outlineCoroutine = StartCoroutine(AnimateFrames(outlineFrames, outlineSpeed));
    }

    void OnMouseExit()
    {
        // mys pryc -> vypnout outline
        if (outlineCoroutine != null)
        {
            StopCoroutine(outlineCoroutine);
            outlineCoroutine = null;
        }
        TurnOffArray(outlineFrames);
    }

    void OnMouseDown()
    {
        // kliknuti bez klice -> locked zvuk + otaznik
        if (!isUnlocked && thoughtCoroutine == null && !KeyItem.isHoldingKey)
        {
            if (lockedSound != null) audioSrc.PlayOneShot(lockedSound, soundVolume);
            thoughtCoroutine = StartCoroutine(AnimateThoughtBubble());
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        if (unlockSound != null) audioSrc.PlayOneShot(unlockSound, soundVolume);

        // smazat otazniky
        if (thoughtCoroutine != null)
        {
            StopCoroutine(thoughtCoroutine);
            thoughtCoroutine = null;
        }
        TurnOffArray(thoughtFrames);

        // start zaverecne cutsceny
        if (playerCharacter != null)
        {
            StartCoroutine(EndingCutscene());
        }
    }

    IEnumerator EndingCutscene()
    {
        // zamknout pohyb hrace at nikam neutika
        PointAndClickMovement playerMovement = playerCharacter.GetComponent<PointAndClickMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        Animator playerAnim = playerCharacter.GetComponentInChildren<Animator>();
        SpriteRenderer[] playerSprites = playerCharacter.GetComponentsInChildren<SpriteRenderer>();

        if (playerAnim != null) playerAnim.SetBool("isWalking", true);
        
        Vector2 targetPos;
        if (walkTarget != null) {
            targetPos = new Vector2(walkTarget.position.x, walkTarget.position.y);
        } else {
            // kdybych zapomnela hodit target do inspektoru
            targetPos = new Vector2(transform.position.x, transform.position.y - 0.2f);
        }
        
        // dojití k cili
        while (Vector2.Distance(playerCharacter.transform.position, targetPos) > 0.05f)
        {
            playerCharacter.transform.position = Vector2.MoveTowards(playerCharacter.transform.position, targetPos, walkSpeed * Time.deltaTime);
            yield return null;
        }

        if (playerAnim != null) playerAnim.SetBool("isWalking", false);
        yield return new WaitForSeconds(0.1f);
        
        // zamavani na konec
        if (goodbyeSound != null) audioSrc.PlayOneShot(goodbyeSound, soundVolume);
        if (playerAnim != null) playerAnim.SetTrigger("Wave");
        yield return new WaitForSeconds(waveDuration);

        // pan mochomurka mizi
        float elapsed = 0f;
        while (elapsed < fadeOutSpeed)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutSpeed);

            foreach (SpriteRenderer sr in playerSprites)
            {
                if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, newAlpha);
            }
            yield return null;
        }
            
        // natvrdo nula na konci at tam nestrasi duch
        foreach (SpriteRenderer sr in playerSprites)
        {
            if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        }
    }

    // loopovani políček animace
    IEnumerator AnimateFrames(GameObject[] frames, float speed)
    {
        int currentFrame = 0;
        while (true)
        {
            TurnOffArray(frames);
            if (frames.Length > 0 && frames[currentFrame] != null) frames[currentFrame].SetActive(true);
            currentFrame++;
            if (currentFrame >= frames.Length) currentFrame = 0;
            yield return new WaitForSeconds(speed);
        }
    }

    // animace otazniku
    IEnumerator AnimateThoughtBubble()
    {
        float elapsed = 0f;
        int currentFrame = 0;
        while (elapsed < thoughtDisplayTime)
        {
            TurnOffArray(thoughtFrames);
            if (thoughtFrames.Length > 0 && thoughtFrames[currentFrame] != null) thoughtFrames[currentFrame].SetActive(true);
            currentFrame++;
            if (currentFrame >= thoughtFrames.Length) currentFrame = 0;
            yield return new WaitForSeconds(thoughtAnimationSpeed);
            elapsed += thoughtAnimationSpeed;
        }
        TurnOffArray(thoughtFrames);
        thoughtCoroutine = null; 
    }

    // schovavani objektu v poli
    void TurnOffArray(GameObject[] array)
    {
        foreach (GameObject obj in array)
            if (obj != null) obj.SetActive(false);
    }
}