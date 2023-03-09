using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSettings : MonoBehaviour
{
    public static DebugSettings Get()
    {
        return Instance;
    }

    // Singleton instance.
    private static DebugSettings instance;
    private static DebugSettings Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<DebugSettings>();
            }
            return instance;
        }
    }

    public bool show_hit_boxes;
}
