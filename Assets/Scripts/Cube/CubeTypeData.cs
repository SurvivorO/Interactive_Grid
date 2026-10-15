using UnityEngine;

[CreateAssetMenu(fileName = "NewCubeType", menuName = "Grid Assessment/Cube Type")]
public class CubeTypeData : ScriptableObject
{
    [SerializeField] private string typeName;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Material material;

    public string TypeName => typeName;
    public Color Color => color;
    public Material Material => material;
}
