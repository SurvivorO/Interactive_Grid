using TMPro;
using UnityEngine;

public class GridUI : MonoBehaviour
{
    [SerializeField] private TMP_Text typeNameLabel;

    public void UpdateSelectedType(CubeTypeData type)
    {
        if (!typeNameLabel || !type) return;

        typeNameLabel.text = type.TypeName;
        typeNameLabel.color = type.Color;
    }
}
