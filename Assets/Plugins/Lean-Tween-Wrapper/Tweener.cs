using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tweener : MonoBehaviour
{
    [Tooltip("If null, the object this script is attached to will be used")]
    public GameObject objectToAnimate;

    private enum UIAnimationTypes
    {
        Move,
        Scale,
        Rotate,
        Fade
    }

    [SerializeField] UIAnimationTypes animationType;
    public LeanTweenType easeType;
    [SerializeField] private AnimationCurve curve;
    [SerializeField, Tooltip("When to trigger automatically")] private Trigger trigger;
    
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float duration;
    [SerializeField] private float delay;
    
    [SerializeField] private bool loop;
    [SerializeField] private bool pingpong;
    [SerializeField] private FinishBehaviour finishBehaviour;
    [System.Flags]
    public enum FinishBehaviour
    {
        Disable = 1,
        Destroy = 2,
        DisableRoot = 4,
        DestroyRoot = 8
    }
    [SerializeField] private GameObject root;

    [SerializeField, Tooltip("If true the value to animate will be set to From on begin")] private bool useStartingValue;
    [SerializeField] private EndValueType endValueType;
    private enum EndValueType
    {
        Absolute,
        Relative,
        Scaled
    }
    public Vector3 from;
    public Vector3 to;
    public Color fromColor;
    public Color toColor;

    [SerializeField, Tooltip("Trigger onComplete also on start")] private bool onCompleteOnStart;
    [SerializeField, Tooltip("Trigger onComplete at the end of each loop")] private bool onCompleteOnRepeat;
    [SerializeField] private UnityEvent onComplete;

    [System.Flags]
    private enum Trigger
    {
        OnEnable = 1,
        OnStart = 2
    }

    private LTDescr tweenObject;
    private RectTransform rectTransform;
    private Transform transform;
    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        transform = GetComponent<Transform>();
        image = GetComponent<Image>();
    }

    private void Start()
    {
        if (objectToAnimate == null)
        {
            objectToAnimate = gameObject;
        }
        if (trigger.HasFlag(Trigger.OnStart))
        {
            Play();
        }
    }

    private void OnEnable()
    {
        if (trigger.HasFlag(Trigger.OnEnable))
        {
            Play();
        }
    }

    public void Play()
    {
        if (tweenObject != null)
        {
            LeanTween.cancel(tweenObject.uniqueId);
        }
        HandleTween();
    }

    private void HandleTween()
    {
        if (objectToAnimate == null)
        {
            objectToAnimate = gameObject;
        }

        switch (animationType)
        {
            case UIAnimationTypes.Move:
                Move();
                break;
            case UIAnimationTypes.Rotate:
                Rotate();
                break;
            case UIAnimationTypes.Scale:
                Scale();
                break;
            case UIAnimationTypes.Fade:
                Fade();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        tweenObject.setDelay(delay);
        if (easeType == LeanTweenType.animationCurve)
        {
            tweenObject.setEase(curve);
        }
        else
        {
            tweenObject.setEase(easeType);
        }
        tweenObject.setDestroyOnComplete(finishBehaviour.HasFlag(FinishBehaviour.Destroy));
        tweenObject.setOnComplete(OnComplete);
        tweenObject.setOnCompleteOnStart(onCompleteOnStart);
        tweenObject.setOnCompleteOnRepeat(onCompleteOnRepeat);

        if (loop)
        {
            tweenObject.setLoopCount(int.MaxValue);
        }
        if (pingpong)
        {
            tweenObject.setLoopPingPong();
        }

        tweenObject.setIgnoreTimeScale(useUnscaledTime);
    }

    private void Fade()
    {
        if (useStartingValue)
        {
            image.color = fromColor;
        }

        Color dest = endValueType switch
        {
            EndValueType.Absolute => toColor,
            EndValueType.Relative => new Color(image.color.r + toColor.r, image.color.g + toColor.g,
                image.color.b + toColor.b, image.color.a + toColor.a),
            EndValueType.Scaled => new Color(image.color.r * toColor.r, image.color.g * toColor.g,
                image.color.b * toColor.b, image.color.a * toColor.a),
            _ => Color.white
        };
        tweenObject = LeanTween.color(rectTransform, dest, duration);
    }

    private void Move()
    {
        if (useStartingValue)
        {
            if (rectTransform != null)
                rectTransform.anchoredPosition = from;
            else if (transform != null)
                transform.position = from;
        }

        if (rectTransform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => new Vector3(rectTransform.anchoredPosition.x + to.x,
                    rectTransform.anchoredPosition.y + to.y, to.z),
                EndValueType.Scaled => new Vector3(rectTransform.anchoredPosition.x * to.x,
                    rectTransform.anchoredPosition.y * to.y, to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.move(rectTransform, dest, duration);
        }
        else if (transform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => transform.position + to,
                EndValueType.Scaled => new Vector3(transform.position.x * to.x,
                    transform.position.y * to.y, transform.position.z * to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.move(gameObject, dest, duration);
        }
    }

    private void Rotate()
    {
        if (useStartingValue)
        {
            if (rectTransform != null)
                rectTransform.rotation = Quaternion.Euler(from);
            else if (transform != null)
                transform.rotation = Quaternion.Euler(from);
        }

        if (rectTransform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => rectTransform.rotation.eulerAngles + to,
                EndValueType.Scaled => new Vector3(rectTransform.anchoredPosition.x * to.x,
                    rectTransform.anchoredPosition.y * to.y, to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.rotate(rectTransform, dest, duration);
        }
        else if (transform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => transform.rotation.eulerAngles + to,
                EndValueType.Scaled => new Vector3(transform.position.x * to.x,
                    transform.position.y * to.y, transform.position.z * to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.rotate(gameObject, dest, duration);
        }
    }

    private void Scale()
    {
        if (useStartingValue)
        {
            if (rectTransform != null)
                rectTransform.localScale = from;
            else if (transform != null)
                transform.localScale = from;
        }

        if (rectTransform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => rectTransform.localScale + to,
                EndValueType.Scaled => new Vector3(rectTransform.anchoredPosition.x * to.x,
                    rectTransform.anchoredPosition.y * to.y, to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.scale(rectTransform, dest, duration);
        }
        else if (transform != null)
        {
            Vector3 dest = endValueType switch
            {
                EndValueType.Absolute => to,
                EndValueType.Relative => transform.localScale + to,
                EndValueType.Scaled => new Vector3(transform.position.x * to.x,
                    transform.position.y * to.y, transform.position.z * to.z),
                _ => Vector3.zero
            };
            tweenObject = LeanTween.scale(gameObject, dest, duration);
        }
    }

    private void OnComplete()
    {
        onComplete.Invoke();
        if (finishBehaviour.HasFlag(FinishBehaviour.DestroyRoot) && root != null)
        {
            Destroy(root);
        }
        else if (finishBehaviour.HasFlag(FinishBehaviour.DisableRoot) && root != null)
        {
            root.SetActive(false);
        }
        else if (finishBehaviour.HasFlag(FinishBehaviour.Disable))
        {
            gameObject.SetActive(false);
        }
    }
}