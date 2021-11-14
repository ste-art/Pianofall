using UnityEngine;
using System.Collections;
using System.Linq;

public class MouserControl : MonoBehaviour
{
    // Mouse buttons in the same order as Unity
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2,
        None = 3
    }

    [System.Serializable]
    // Handles left modifiers keys (Alt, Ctrl, Shift)
    public class Modifiers
    {
        public bool leftAlt;
        public bool leftControl;
        public bool leftShift;

        public bool checkModifiers()
        {
            return (!leftAlt ^ Input.GetKey(KeyCode.LeftAlt)) &&
                   (!leftControl ^ Input.GetKey(KeyCode.LeftControl)) &&
                   (!leftShift ^ Input.GetKey(KeyCode.LeftShift));
        }
    }

    [System.Serializable]
    // Handles common parameters for translations and rotations
    public class MouseControlConfiguration
    {

        public bool activate;
        public MouseButton mouseButton;
        public Modifiers modifiers;
        public float sensitivity;

        public bool isActivated()
        {
            return activate && Input.GetMouseButton((int) mouseButton) && modifiers.checkModifiers();
        }
    }

    [System.Serializable]
    // Handles scroll parameters
    public class MouseScrollConfiguration
    {

        public bool activate;
        public Modifiers modifiers;
        public float sensitivity;

        public bool isActivated()
        {
            return activate && modifiers.checkModifiers();
        }
    }

    // Yaw default configuration
    public MouseControlConfiguration yaw = new MouseControlConfiguration
    {
        mouseButton = MouseButton.Right,
        sensitivity = 10F
    };

    // Pitch default configuration
    public MouseControlConfiguration pitch = new MouseControlConfiguration
    {
        mouseButton = MouseButton.Right,
        modifiers = new Modifiers {leftControl = true},
        sensitivity = 10F
    };

    // Roll default configuration
    public MouseControlConfiguration roll = new MouseControlConfiguration();

    // Vertical translation default configuration
    public MouseControlConfiguration verticalTranslation = new MouseControlConfiguration
    {
        mouseButton = MouseButton.Middle,
        sensitivity = 2F
    };

    // Horizontal translation default configuration
    public MouseControlConfiguration horizontalTranslation = new MouseControlConfiguration
    {
        mouseButton = MouseButton.Middle,
        sensitivity = 2F
    };

    // Depth (forward/backward) translation default configuration
    public MouseControlConfiguration depthTranslation = new MouseControlConfiguration
    {
        mouseButton = MouseButton.Left,
        sensitivity = 2F
    };

    private MouseControlConfiguration[] _configs;

    void Start()
    {
        _configs = new []{ yaw, pitch, roll, verticalTranslation, horizontalTranslation};
    }

    public bool ArrowsMove = true;

    // Scroll default configuration
    public MouseScrollConfiguration scroll = new MouseScrollConfiguration {sensitivity = 2F};

    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";


    void LateUpdate()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Cursor.lockState = _configs.Any(c => c.isActivated()) ? CursorLockMode.Locked : CursorLockMode.None;
#endif
        var angles = transform.eulerAngles;
        var x = angles.y;
        var y = angles.x;

        if (yaw.isActivated())
            x += Input.GetAxis(mouseHorizontalAxisName)*yaw.sensitivity;
        if (pitch.isActivated())
            y -= Input.GetAxis(mouseVerticalAxisName)*pitch.sensitivity;

        if (y < -360) y += 360;
        if (y > 360) y -= 360;
        if (y > 90 && y < 180) y = 90;
        if (y < 270 && y > 180) y = 270;

        transform.rotation = Quaternion.Euler(y, x, 0);

        if (verticalTranslation.isActivated())
        {
            float translateY = Input.GetAxis(mouseVerticalAxisName)*verticalTranslation.sensitivity;
            transform.Translate(0, translateY, 0);
        }

        if (horizontalTranslation.isActivated())
        {
            float translateX = Input.GetAxis(mouseHorizontalAxisName)*horizontalTranslation.sensitivity;
            transform.Translate(translateX, 0, 0);
        }

        if (depthTranslation.isActivated())
        {
            float translateZ = Input.GetAxis(mouseVerticalAxisName)*depthTranslation.sensitivity;
            transform.Translate(0, 0, translateZ);
        }

        if(ArrowsMove)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(0,0,0.1f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(0, 0, -0.1f);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(-0.1f, 0, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(0.1f, 0, 0);
            }
        }

        if (scroll.isActivated())
        {
            float translateZ = Input.GetAxis(scrollAxisName)*scroll.sensitivity;
            transform.Translate(0, 0, translateZ);
        }


    }
}
