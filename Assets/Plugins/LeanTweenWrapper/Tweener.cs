using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tweener : MonoBehaviour
{
    [Tooltip("If null, the object this script is attached to will be used")]
    public GameObject ObjectToAnimate;

    private enum UIAnimationTypes
    {
        Move,
        Scale,
        Rotate,
        Fade
    }

    [SerializeField] UIAnimationTypes _animationType;
    public LeanTweenType EaseType;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField, Tooltip("When to trigger automatically")] private Trigger _trigger;
    
    [SerializeField] private bool _useUnscaledTime = true;
    [SerializeField] private float _duration;
    [SerializeField] private float _delay;
    
    [SerializeField] private bool _loop;
    [SerializeField] private bool _pingpong;
    [SerializeField] private FinishBehaviour _finishBehaviour;
    [System.Flags]
    public enum FinishBehaviour
    {
        Disable = 1,
        Destroy = 2,
        DisableRoot = 4,
        DestroyRoot = 8
    }
    [SerializeField] private GameObject _root;

    [SerializeField, Tooltip("If true the value to animate will be set to From on begin")] private bool _useStartingValue;
    [SerializeField] private EndValueType _endValueType;
    private enum EndValueType
    {
        Absolute,
        Relative,
        Scaled
    }
    public Vector3 From;
    public Vector3 To;
    public Color FromColor;
    public Color ToColor;

    [SerializeField, Tooltip("Trigger onComplete also on start")] private bool _onCompleteOnStart;
    [SerializeField, Tooltip("Trigger onComplete at the end of each loop")] private bool _onCompleteOnRepeat;
    [SerializeField] private UnityEvent _onComplete;

    [System.Flags]
    private enum Trigger
    {
        OnEnable = 1,
        OnStart = 2
    }

    private LTDescr _tweenObject;
    private RectTransform _rectTransform;
    private Image _image;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        if (ObjectToAnimate == null)
        {
            ObjectToAnimate = gameObject;
        }
        if (_trigger.HasFlag(Trigger.OnStart))
        {
            Play();
        }
    }

    private void OnEnable()
    {
        if (_trigger.HasFlag(Trigger.OnEnable))
        {
            Play();
        }
    }

    public void Play()
    {
        if (_tweenObject != null)
        {
            LeanTween.cancel(_tweenObject.uniqueId);
        }
        HandleTween();
    }

    private void HandleTween()
    {
        if (ObjectToAnimate == null)
        {
            ObjectToAnimate = gameObject;
        }

        switch (_animationType)
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

        _tweenObject.setDelay(_delay);
        if (EaseType == LeanTweenType.animationCurve)
        {
            _tweenObject.setEase(_curve);
        }
        else
        {
            _tweenObject.setEase(EaseType);
        }
        _tweenObject.setDestroyOnComplete(_finishBehaviour.HasFlag(FinishBehaviour.Destroy));
        _tweenObject.setOnComplete(OnComplete);
        _tweenObject.setOnCompleteOnStart(_onCompleteOnStart);
        _tweenObject.setOnCompleteOnRepeat(_onCompleteOnRepeat);

        if (_loop)
        {
            _tweenObject.setLoopCount(int.MaxValue);
        }
        if (_pingpong)
        {
            _tweenObject.setLoopPingPong();
        }

        _tweenObject.setIgnoreTimeScale(_useUnscaledTime);
    }

    private void Fade()
    {
        if (_useStartingValue)
        {
            _image.color = FromColor;
        }

        Color dest = _endValueType switch
        {
            EndValueType.Absolute => ToColor,
            EndValueType.Relative => new Color(_image.color.r + ToColor.r, _image.color.g + ToColor.g,
                _image.color.b + ToColor.b, _image.color.a + ToColor.a),
            EndValueType.Scaled => new Color(_image.color.r * ToColor.r, _image.color.g * ToColor.g,
                _image.color.b * ToColor.b, _image.color.a * ToColor.a),
            _ => Color.white
        };
        _tweenObject = LeanTween.color(_rectTransform, dest, _duration);
    }

    private void Move()
    {
        if (_useStartingValue)
        {
            _rectTransform.anchoredPosition = From;
        }

        Vector3 dest = _endValueType switch
        {
            EndValueType.Absolute => To,
            EndValueType.Relative => new Vector3(_rectTransform.anchoredPosition.x + To.x,
                _rectTransform.anchoredPosition.y + To.y, To.z),
            EndValueType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * To.x,
                _rectTransform.anchoredPosition.y * To.y, To.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.move(_rectTransform, dest, _duration);
    }

    private void Rotate()
    {
        if (_useStartingValue)
        {
            _rectTransform.rotation = Quaternion.Euler(From);
        }

        Vector3 dest = _endValueType switch
        {
            EndValueType.Absolute => To,
            EndValueType.Relative => _rectTransform.rotation.eulerAngles + To,
            EndValueType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * To.x,
                _rectTransform.anchoredPosition.y * To.y, To.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.rotate(_rectTransform, dest, _duration);
    }

    private void Scale()
    {
        if (_useStartingValue)
        {
            _rectTransform.localScale = From;
        }

        Vector3 dest = _endValueType switch
        {
            EndValueType.Absolute => To,
            EndValueType.Relative => _rectTransform.localScale + To,
            EndValueType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * To.x,
                _rectTransform.anchoredPosition.y * To.y, To.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.scale(ObjectToAnimate, dest, _duration);
    }

    private void OnComplete()
    {
        _onComplete.Invoke();
        if (_finishBehaviour.HasFlag(FinishBehaviour.DestroyRoot) && _root != null)
        {
            Destroy(_root);
        }
        else if (_finishBehaviour.HasFlag(FinishBehaviour.DisableRoot) && _root != null)
        {
            _root.SetActive(false);
        }
        else if (_finishBehaviour.HasFlag(FinishBehaviour.Disable))
        {
            gameObject.SetActive(false);
        }
    }
}