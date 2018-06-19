using UnityEngine;

public class Helpers
{
    // http://www.third-helix.com/2013/04/12/doing-thumbstick-dead-zones-right.html
    static public void CleanupAxes(ref float horizontal, ref float vertical)
    {
        // Manual dead zone handling because Unity's doesn't work?
        float deadZone = 0.25f;

        Vector2 stick = new Vector2(horizontal, vertical);
        float stickMagnitude = stick.magnitude;
        if (stickMagnitude < deadZone)
        {
            horizontal = 0.0f;
            vertical = 0.0f;
        }
        else
        {
            Vector2 stickDir = (stick / stickMagnitude);
            Vector2 stickScaled = stickDir * ((stickMagnitude - deadZone) / (1.0f - deadZone));

            horizontal = stickScaled.x;
            vertical = stickScaled.y;
        }
    }
}
