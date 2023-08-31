// Current Unity Os Type
enum OSType
{
    Window,
    Android,
    IOS
}

// Code to set up a build
public class BuildOption
{
    public int tcpServerPort = 4200; // tcp Connet Port Number

    public int maxPacketSize = 65000;

    public float version = 1.01f;

#if UNITY_STANDALONE_WIN
    public int platformType = 0; // 0 Window / 1 Android / 2 IOS
#endif

#if PLATFORM_ANDROID
    public int platformType = 1; // 0 Window / 1 Android / 2 IOS
#endif
}
