using UnityEngine;
using UnityEngine.UI;
using Leap;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class LeapPointerController : MonoBehaviour
{
    private Controller leapController;
    public RectTransform canvasRect;
    public RectTransform pointerRect; // The RectTransform of the pointer image
    private Camera uiCamera;

    private float leapZMin = -0.19f; // Observed Min Z value
    private float leapZMax = 0.19f;  // Observed Max Z value
    private float leapXMin = -0.1f;
    private float leapXMax = 0.1f;

    public float pinchThreshold = 0.02f;
    public float sensitivity = 1.0f; // Sensitivity multiplier
    public float bottomBuffer = -540f; // Sensitivity multiplier
    public float scrollSpeed = 0.3f;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    void Start()
    {
        pointerEventData = new PointerEventData(eventSystem);
        leapController = new Controller();
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        Vector3 tipPosition = GetLeapTipPosition();
        Vector2 canvasPosition = ConvertToCanvasSpace(tipPosition);
        float temp = Mathf.Clamp((canvasPosition.y + bottomBuffer) * sensitivity, -540f, 540f);
        //Debug.Log(temp);
        pointerRect.anchoredPosition = new Vector2(canvasPosition.x, temp);
        RaycastButton();
    }

    private void RaycastButton()
    {
        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(eventSystem);
        }

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, pointerRect.position);
        pointerEventData.Reset();
        pointerEventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, results);

        if (results.Count > 0)
        {
            Button button = null;
            ScrollRect scrollRect = null;

            foreach (var result in results)
            {
                // Check for Button
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    button = result.gameObject.GetComponent<Button>();
                }
                // Check for TMP_InputField
                else if (result.gameObject.GetComponent<TMP_InputField>() != null)
                {
                    result.gameObject.GetComponent<TMP_InputField>().ActivateInputField();
                }
                // Check for ScrollRect on the current object or its parents
                else
                {
                    Transform parent = result.gameObject.transform;
                    while (parent != null)
                    {
                        if (parent.GetComponent<ScrollRect>() != null)
                        {
                            scrollRect = parent.GetComponent<ScrollRect>();
                            break;
                        }
                        parent = parent.parent;
                    }
                }
            }

            if (button != null && IsPinchGesture())
            {
                button.onClick.Invoke();
                Debug.Log("Button Clicked: " + button.name);
            }

            if (scrollRect != null)
            {
                HandleScroll(scrollRect);
            }
        }
    }




    private bool IsPinchGesture()
    {
        Hand hand = GetHand();
        if (hand != null && hand.fingers.Length > 1)
        {
            Finger thumb = hand.fingers[0];
            Finger index = hand.fingers[1];
            float distance = Vector3.Distance(thumb.TipPosition, index.TipPosition);
            return distance < pinchThreshold;
        }
        return false;
    }

    private Vector3 GetLeapTipPosition()
    {
        Hand hand = GetHand();
        if (hand != null && hand.fingers.Length > 1)
        {
            pointerRect.gameObject.SetActive(true);
            return hand.fingers[1].TipPosition;
        }
        else
        {
            pointerRect.gameObject.SetActive(false);
        }
        return Vector3.zero;
    }

    private Vector2 ConvertToCanvasSpace(Vector3 leapPosition)
    {
        float normalizedX = Mathf.Clamp((leapPosition.x - leapXMin) / (leapXMax - leapXMin), -1f, 1f) * sensitivity;
        float normalizedY = Mathf.Clamp((leapPosition.z - leapZMin) / (leapZMax - leapZMin), -1f, 1f) ;
        //Debug.Log(normalizedY);
        float canvasX = normalizedX * (canvasRect.sizeDelta.x / 2);
        float canvasY = normalizedY * (canvasRect.sizeDelta.y ) ;

        canvasX = Mathf.Clamp(canvasX, -canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2);
        canvasY = Mathf.Clamp(canvasY, -1, 1080f);
        

        return new Vector2(canvasX, canvasY);
    }

    private Hand GetHand()
    {
        Frame frame = leapController.Frame();
        if (frame.Hands.Count > 0)
        {
            return frame.Hands[0];
        }
        return null;
    }
    private void HandleScroll(ScrollRect scrollRect)
    {
         // Adjust the sensitivity for scroll movement

        // Scroll direction based on pinch gesture or manual trigger
        float scrollDirection = IsPinchGesture() ? -1f : 1f;

        // Adjust the scroll position
        scrollRect.verticalNormalizedPosition += scrollDirection * scrollSpeed * Time.deltaTime;

        // Clamp the scroll position to stay within bounds
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);

        Debug.Log($"Scroll Position Updated: {scrollRect.verticalNormalizedPosition}");
    }





}
