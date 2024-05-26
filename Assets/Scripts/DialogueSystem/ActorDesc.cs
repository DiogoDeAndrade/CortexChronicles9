using UnityEngine;

[CreateAssetMenu(fileName = "Actor", menuName = "Custom/Actor")]
public class ActorDesc : ScriptableObject
{
    public string   displayName;
    public Sprite   portraitImage;
    public Color    displayColor = Color.white;
    public Color    colorText = Color.white;
}
