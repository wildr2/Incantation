using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellLight : MonoBehaviour
{
    public float radius_small = 0.02f;
    public float radius_large = 0.5f;
    public Text incantation_text;

    public float radius;
    public float radius0;
    public float radius1;
    public float toggle_time;
    private SpriteRenderer sr;

    private string input_text;
    private string incantation;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.material = new Material(sr.material);
    }

    private void Update()
    {
        foreach (char c in Input.inputString)
        {
            if (c == "\b"[0])
            {
                // Backspace.
                input_text = input_text.Substring(0, Mathf.Max(0, input_text.Length - 1));
            }
            else if (char.IsLetter(c) || c == ' ')
            {
                input_text += c;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            input_text = "";
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (input_text.Length > 0)
            {
                incantation = input_text;
                MagicClient magic_client = FindObjectOfType<MagicClient>();
                magic_client.CheckSpell(incantation, CheckSpellCallback);
                input_text = "";
            }
            else if (radius1 > 0.0f)
            {
                SetLight(0.0f);
            }
        }

        float dur = radius1 > 0.0f ? 2.0f : 0.5f;
        float t = Mathf.Clamp01((Time.time - toggle_time) / dur);
        t = radius1 > 0.0f ? 1.0f - Mathf.Pow(1.0f - t, 4.0f) : Mathf.Pow(t, 2.0f);
        radius = Mathf.Lerp(radius0, radius1, t);
        sr.material.SetFloat("_RadiusMultiplier", radius);

        incantation_text.text = input_text;
    }

    private void CheckSpellCallback(float score)
    {
        float r =
            score < 0.9f ? 0.0f :
            score < 1.1f ? Mathf.Lerp(radius_small, radius_small * 2.0f, (score - 0.9f) / (1.1f - 0.9f)) :
            Mathf.Lerp(radius_small * 2.0f, radius_large, (score - 1.1f) / (2.0f - 1.1f));

        SetLight(r);
        Debug.Log(string.Format("rwdbg {0} {1}", incantation, score));
    }

    private void SetLight(float new_radius)
    {
        radius0 = radius;
        radius1 = new_radius;
        toggle_time = Time.time;
    }
}
