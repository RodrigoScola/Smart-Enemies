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

        System.Collections.Generic.Dictionary<int, Action> actions = en.actions.Actions();

        int id = -1;
        try
        {
            id = en.GetBatch()!.GetId();
        }
        finally
        {
            EditorGUILayout.IntField("Batch Id", id);
        }

        EnemyBatch batch = en.GetHive().manager.GetEnemyBatch(en.GetId())!;

        if (batch is not null)
        {
            EditorGUILayout.IntField("Enemy Manager Batch id ", batch.GetId());
        }

        EditorGUILayout.IntField("Total Actions", actions.Count);

        foreach (Action ac in actions.Values)
        {
            SerializedObject myObjectProperty = new(ac);
            SerializedProperty p = myObjectProperty.GetIterator();

            if (p.NextVisible(true))
            {
                do
                {
                    EditorGUILayout.PropertyField(prop, true);
                } while (prop.NextVisible(false));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
