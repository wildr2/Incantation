using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class CoroutineUtil
{
    public static IEnumerator WaitForRealSeconds(float time)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start + time)
        {
            yield return null;
        }
    }
    public static IEnumerator ResetTrailRenderer(TrailRenderer tr)
    {
        float trailTime = tr.time;
        tr.time = 0;
        yield return new WaitForSeconds(0.5f);
        tr.time = trailTime;
    }
    public static IEnumerator DoNextFrame(Action func)
    {
        yield return null;
        func();
    }
    public static IEnumerator DoAtEndOfFrame(Action func)
    {
        yield return new WaitForEndOfFrame();
        func();
    }
    public static IEnumerator DoAfterDelay(Action func, float seconds, bool real_seconds = false)
    {
        if (real_seconds)
        {
            IEnumerator delay = WaitForRealSeconds(seconds);
            while (delay.MoveNext()) yield return delay.Current;
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
        func();
    }
    public static IEnumerator WaitForEvent(Action action)
    {
        bool flag = false;
        Action a = () => flag = true;
        action += a;
        while (flag == false) yield return null;
        action -= a;
    }
}