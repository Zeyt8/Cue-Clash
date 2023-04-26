using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tweener))]
[CanEditMultipleObjects]
public class TweenerEditor : Editor
{
    private SerializedProperty objectToAnimate;
    
    private SerializedProperty animationType;
    private SerializedProperty easeType;
    private SerializedProperty curve;
    private SerializedProperty trigger;
    
    private SerializedProperty useUnscaledTime;
    private SerializedProperty duration;
    private SerializedProperty delay;

    private SerializedProperty loop;
    private SerializedProperty pingpong;
    private SerializedProperty finishBehaviour;
    private SerializedProperty root;

    private SerializedProperty useStartingValue;
    private SerializedProperty endValueType;

    private SerializedProperty from;
    private SerializedProperty to;
    private SerializedProperty fromColor;
    private SerializedProperty toColor;

    private SerializedProperty onComplete;
    private SerializedProperty onCompleteOnStart;
    private SerializedProperty onCompleteOnRepeat;

    private void OnEnable()
    {
        objectToAnimate = serializedObject.FindProperty("objectToAnimate");
        animationType = serializedObject.FindProperty("animationType");
        easeType = serializedObject.FindProperty("easeType");
        curve = serializedObject.FindProperty("curve");
        trigger = serializedObject.FindProperty("trigger");
        useUnscaledTime = serializedObject.FindProperty("useUnscaledTime");
        duration = serializedObject.FindProperty("duration");
        delay = serializedObject.FindProperty("delay");
        loop = serializedObject.FindProperty("loop");
        pingpong = serializedObject.FindProperty("pingpong");
        finishBehaviour = serializedObject.FindProperty("finishBehaviour");
        root = serializedObject.FindProperty("root");
        useStartingValue = serializedObject.FindProperty("useStartingValue");
        endValueType = serializedObject.FindProperty("endValueType");
        from = serializedObject.FindProperty("from");
        to = serializedObject.FindProperty("to");
        fromColor = serializedObject.FindProperty("fromColor");
        toColor = serializedObject.FindProperty("toColor");
        onComplete = serializedObject.FindProperty("onComplete");
        onCompleteOnStart = serializedObject.FindProperty("onCompleteOnStart");
        onCompleteOnRepeat = serializedObject.FindProperty("onCompleteOnRepeat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(objectToAnimate);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Behaviour", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(animationType);
        EditorGUILayout.PropertyField(easeType);
        EditorGUILayout.PropertyField(curve);
        EditorGUILayout.PropertyField(trigger);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Time Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useUnscaledTime);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(delay);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Finish Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(loop);
        EditorGUILayout.PropertyField(pingpong);
        EditorGUILayout.PropertyField(finishBehaviour);
        if (finishBehaviour.intValue == (int) Tweener.FinishBehaviour.DestroyRoot ||
            finishBehaviour.intValue == (int) Tweener.FinishBehaviour.DisableRoot)
        {
            EditorGUILayout.PropertyField(root);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(endValueType);
        if (endValueType.enumValueIndex == 0)
        {
            EditorGUILayout.LabelField("End value is equal to To", EditorStyles.helpBox);
        }
        else if (endValueType.enumValueIndex == 1)
        {
            EditorGUILayout.LabelField("End value is equal to value on begin + To", EditorStyles.helpBox);
        }
        else if (endValueType.enumValueIndex == 2)
        {
            EditorGUILayout.LabelField("End value is equal to value on begin * To element by element", EditorStyles.helpBox);
        }
        EditorGUILayout.PropertyField(useStartingValue);
        if (animationType.enumValueIndex != 3)
        {
            if (useStartingValue.boolValue)
            {
                EditorGUILayout.PropertyField(from);
            }
            EditorGUILayout.PropertyField(to);
        }
        if (animationType.enumValueIndex == 3)
        {
            if (useStartingValue.boolValue)
            {
                EditorGUILayout.PropertyField(fromColor);
            }
            EditorGUILayout.PropertyField(toColor);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onCompleteOnStart);
        EditorGUILayout.PropertyField(onCompleteOnRepeat);
        EditorGUILayout.PropertyField(onComplete);
        
        serializedObject.ApplyModifiedProperties();
    }
}
