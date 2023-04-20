using Unity.Netcode;
using UnityEngine;
using UnityEditor;

public static class NetworkObjectFixer
{
    [MenuItem("Tools/Fix NetworkObjects in Scene")]
    public static void FixNetworkObjectsInScene()
    {
        var networkObjects = Object.FindObjectsOfType<NetworkObject>(true);
        foreach (var networkObject in networkObjects)
        {
            if (!networkObject.gameObject.scene.isLoaded) continue;

            var serializedObject = new SerializedObject(networkObject);
            var hashField = serializedObject.FindProperty("GlobalObjectIdHash");

            // Ugly hack. Reset the hash and apply it.
            // This implicitly marks the field as dirty, allowing it to be saved as an override.
            hashField.uintValue = 0;
            serializedObject.ApplyModifiedProperties();
            // Afterwards, OnValidate will kick in and return the hash to it's real value, which will be saved now.
        }
    }
}