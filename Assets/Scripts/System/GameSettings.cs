using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings {
    public static float mouseSensitivity = 0.5f;
    public static LayerMask aimMask = LayerMask.GetMask("World", "Hitbox");
    public static LayerMask collisionMask = LayerMask.GetMask("World", "CharacterController");
    public static LayerMask characterMask = LayerMask.GetMask("CharacterController");
    public static LayerMask worldMask = LayerMask.GetMask("World");
}
