using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tweener))]
[CanEditMultipleObjects]
public class TweenerEditor : Editor
{
    private SerializedProperty _objectToAnimate;
    
    private SerializedProperty _animationType;
    private SerializedProperty _easeType;
    private SerializedProperty _curve;
    private SerializedProperty _trigger;
    
    private SerializedProperty _useUnscaledTime;
    private SerializedProperty _duration;
    private SerializedProperty _delay;

    private SerializedProperty _loop;
    private SerializedProperty _pingpong;
    private SerializedProperty _destroyOnFinish;
    private SerializedProperty _destroyRootOnFinish;
    private SerializedProperty _root;

    private SerializedProperty _useStartingValue;
    private SerializedProperty _endValueType;

    private SerializedProperty _from;
    private SerializedProperty _to;
    private SerializedProperty _fromColor;
    private SerializedProperty _toColor;

    private SerializedProperty _onComplete;
    private SerializedProperty _onCompleteOnStart;
    private SerializedProperty _onCompleteOnRepeat;

    private void OnEnable()
    {
        _objectToAnimate = serializedObject.FindProperty("ObjectToAnimate");
        _animationType = serializedObject.FindProperty("_animationType");
        _easeType = serializedObject.FindProperty("EaseType");
        _curve = serializedObject.FindProperty("_curve");
        _trigger = serializedObject.FindProperty("_trigger");
        _useUnscaledTime = serializedObject.FindProperty("_useUnscaledTime");
        _duration = serializedObject.FindProperty("_duration");
        _delay = serializedObject.FindProperty("_delay");
        _loop = serializedObject.FindProperty("_loop");
        _pingpong = serializedObject.FindProperty("_pingpong");
        _destroyOnFinish = serializedObject.FindProperty("_destroyOnFinish");
        _destroyRootOnFinish = serializedObject.FindProperty("_destroyRootOnFinish");
        _root = serializedObject.FindProperty("_root");
        _useStartingValue = serializedObject.FindProperty("_useStartingValue");
        _endValueType = serializedObject.FindProperty("_endValueType");
        _from = serializedObject.FindProperty("From");
        _to = serializedObject.FindProperty("To");
        _fromColor = serializedObject.FindProperty("FromColor");
        _toColor = serializedObject.FindProperty("ToColor");
        _onComplete = serializedObject.FindProperty("_onComplete");
        _onCompleteOnStart = serializedObject.FindProperty("_onCompleteOnStart");
        _onCompleteOnRepeat = serializedObject.FindProperty("_onCompleteOnRepeat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_objectToAnimate);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Behaviour", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_animationType);
        EditorGUILayout.PropertyField(_easeType);
        EditorGUILayout.PropertyField(_curve);
        EditorGUILayout.PropertyField(_trigger);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Time Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_useUnscaledTime);
        EditorGUILayout.PropertyField(_duration);
        EditorGUILayout.PropertyField(_delay);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Finish Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_loop);
        EditorGUILayout.PropertyField(_pingpong);
        EditorGUILayout.PropertyField(_destroyOnFinish);
        EditorGUILayout.PropertyField(_destroyRootOnFinish);
        if (_destroyRootOnFinish.boolValue)
        {
            EditorGUILayout.PropertyField(_root);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_endValueType);
        if (_endValueType.enumValueIndex == 0)
        {
            EditorGUILayout.LabelField("End value is equal to To", EditorStyles.helpBox);
        }
        else if (_endValueType.enumValueIndex == 1)
        {
            EditorGUILayout.LabelField("End value is equal to value on begin + To", EditorStyles.helpBox);
        }
        else if (_endValueType.enumValueIndex == 2)
        {
            EditorGUILayout.LabelField("End value is equal to value on begin * To element by element", EditorStyles.helpBox);
        }
        EditorGUILayout.PropertyField(_useStartingValue);
        if (_animationType.enumValueIndex != 3)
        {
            if (_useStartingValue.boolValue)
            {
                EditorGUILayout.PropertyField(_from);
            }
            EditorGUILayout.PropertyField(_to);
        }
        if (_animationType.enumValueIndex == 3)
        {
            if (_useStartingValue.boolValue)
            {
                EditorGUILayout.PropertyField(_fromColor);
            }
            EditorGUILayout.PropertyField(_toColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_onCompleteOnStart);
        EditorGUILayout.PropertyField(_onCompleteOnRepeat);
        EditorGUILayout.PropertyField(_onComplete);
        
        serializedObject.ApplyModifiedProperties();
    }
}
