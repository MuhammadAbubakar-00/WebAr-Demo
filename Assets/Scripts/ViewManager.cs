using UnityEngine;
using System.Collections;
using Imagine.WebAR;

public class WebARViewManager : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject standard3DCamera;
    public GameObject webARCamera;

    [Header("WebAR Components")]
    public WorldTracker worldTracker;

    [Header("Target Object")]
    public GameObject targetObject;

    [Header("UI Elements")]
    public CanvasGroup transitionFade;
    [SerializeField] private GameObject bgUi;

    [Header("Transition Settings")]
    public float fadeDuration = 0.3f;
    public float movementDuration = 0.6f;

    // Memory for the AR position (where the user left it in the room)
    private Vector3 savedARPosition;
    private Quaternion savedARRotation;
    
    // Fixed 3D destination (The "Studio" view)
    private Vector3 center3DPos = Vector3.zero;
    private Quaternion center3DRot = Quaternion.Euler(0, 180, 0);

    private Coroutine transitionCoroutine;

    void Start()
    {
       
        transitionFade.alpha = 0;
        
        if (worldTracker != null) worldTracker.gameObject.SetActive(true);
        PrepareARVisuals();
    }

    public void OnClickTo3D()
    {
        // 1. Before we leave AR, SAVE where the object is in the room
        savedARPosition = targetObject.transform.position;
        savedARRotation = targetObject.transform.rotation;

        // 2. Move to the fixed Studio Center
        StartCoroutine(TransitionSequence(center3DPos, center3DRot, false));
    }

    public void OnClickToAR()
    {
        // 1. Move back to the EXACT spot we left it in the room
        StartCoroutine(TransitionSequence(savedARPosition, savedARRotation, true));
    }

    private IEnumerator TransitionSequence(Vector3 targetPos, Quaternion targetRot, bool toAR)
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        // Fade to hide the camera swap
        yield return StartCoroutine(Fade(1));

        if (toAR) PrepareARVisuals(); else Prepare3DVisuals();

        float elapsed = 0;
        Vector3 startPos = targetObject.transform.position;
        Quaternion startRot = targetObject.transform.rotation;

        StartCoroutine(Fade(0));

        while (elapsed < movementDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / movementDuration;
            float smoothedT = Mathf.SmoothStep(0, 1, t);

            targetObject.transform.position = Vector3.Lerp(startPos, targetPos, smoothedT);
            targetObject.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothedT);
            yield return null;
        }

        targetObject.transform.position = targetPos;
        targetObject.transform.rotation = targetRot;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = transitionFade.alpha;
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            transitionFade.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        transitionFade.alpha = targetAlpha;
    }

    private void PrepareARVisuals()
    {
        standard3DCamera.SetActive(false);
        bgUi.SetActive(false); 
        webARCamera.SetActive(true);
    }

    private void Prepare3DVisuals()
    {
        webARCamera.SetActive(false);
        standard3DCamera.SetActive(true);
        bgUi.SetActive(true); 
    }
}