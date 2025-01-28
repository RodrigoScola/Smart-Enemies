using System;
using UnityEditor;

[CustomEditor(typeof(ActionEnemy))]
public class ActionEnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();

        if (prop.NextVisible(true))
        {
            do
            {
                EditorGUILayout.PropertyField(prop, true);
            } while (prop.NextVisible(false));
        }

        ActionEnemy en = (ActionEnemy)target;

        en.MinDistance(EditorGUILayout.FloatField("Min Distance", en.MinDistance()));

        EditorGUILayout.TextField("Is Following player", $"{en.IsFollowingPlayer()}");

        var actions = en.actions.Actions();

        EditorGUILayout.IntField("Total Actions", actions.Count);

        foreach (Action ac in actions.Values)
        {
            EditorGUILayout.TextField("Id", $"{ac.GetId()}");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EnumFlagsField("action type", ac.GetActionType());
            EditorGUILayout.EnumFlagsField("Prority", ac.GetPriority());

            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
