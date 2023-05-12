using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.AttributeUsage(System.AttributeTargets.Field)]
public class SerializePropertyReadOnly : PropertyAttribute
{
    public string PropertyName { get; private set; }

    public SerializePropertyReadOnly(string propertyName)
    {
        PropertyName = propertyName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializePropertyReadOnly))]
public class SerializePropertyReadOnlyAttributeDrawer : PropertyDrawer
{
    private PropertyInfo propertyFieldInfo = null;

    const int rows = 2;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect pos1 = new Rect(position.x, position.y, position.width, position.height / rows);
        Rect pos2 = new Rect(position.x, position.y += position.height / rows, position.width, position.height / rows);

        // Show the field variable normally
        EditorGUI.PropertyField(pos1, property, label, true);

        UnityEngine.Object target = property.serializedObject.targetObject;

        // Find the property field using reflection, in order to get access to its getter/setter.
        if (propertyFieldInfo == null)
            propertyFieldInfo = target.GetType().GetProperty(((SerializePropertyReadOnly)attribute).PropertyName,
                                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (propertyFieldInfo != null)
        {

            // Retrieve the value using the property getter:
            object value = propertyFieldInfo.GetValue(target, null);

            label.text = propertyFieldInfo.Name;

            GUI.enabled = false;
            value = DrawProperty(pos2, property.propertyType, propertyFieldInfo.PropertyType, value, label);
            GUI.enabled = true;
        }
        else
        {
            EditorGUI.LabelField(position, "Error: could not retrieve property.");
        }
    }

    private object DrawProperty(Rect position, SerializedPropertyType propertyType, Type type, object value, GUIContent label)
    {
        switch (propertyType)
        {
            case SerializedPropertyType.Integer:
                return EditorGUI.IntField(position, label, (int)value);
            case SerializedPropertyType.Boolean:
                return EditorGUI.Toggle(position, label, (bool)value);
            case SerializedPropertyType.Float:
                return EditorGUI.FloatField(position, label, (float)value);
            case SerializedPropertyType.String:
                return EditorGUI.TextField(position, label, (string)value);
            case SerializedPropertyType.Color:
                return EditorGUI.ColorField(position, label, (Color)value);
            case SerializedPropertyType.ObjectReference:
                return EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, type, true);
            case SerializedPropertyType.ExposedReference:
                return EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, type, true);
            case SerializedPropertyType.LayerMask:
                return EditorGUI.LayerField(position, label, (int)value);
            case SerializedPropertyType.Enum:
                return EditorGUI.EnumPopup(position, label, (Enum)value);
            case SerializedPropertyType.Vector2:
                return EditorGUI.Vector2Field(position, label, (Vector2)value);
            case SerializedPropertyType.Vector3:
                return EditorGUI.Vector3Field(position, label, (Vector3)value);
            case SerializedPropertyType.Vector4:
                return EditorGUI.Vector4Field(position, label, (Vector4)value);
            case SerializedPropertyType.Rect:
                return EditorGUI.RectField(position, label, (Rect)value);
            case SerializedPropertyType.AnimationCurve:
                return EditorGUI.CurveField(position, label, (AnimationCurve)value);
            case SerializedPropertyType.Bounds:
                return EditorGUI.BoundsField(position, label, (Bounds)value);
            default:
                throw new NotImplementedException("Unimplemented propertyType " + propertyType + ".");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (base.GetPropertyHeight(property, label) + 2) * rows;  // assuming original is one row
    }
}
#endif
