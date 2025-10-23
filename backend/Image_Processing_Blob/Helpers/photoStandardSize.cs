namespace Image_Processing_Blob.Helpers;

public class PhotoStandardSize
{
    private protected static readonly StandardSize[] Dimensions =
    [
        // 4:3
        new(Side1: 640, Side2: 480),
        new(Side1: 800, Side2: 600),
        new(Side1: 960, Side2: 720),
        new(Side1: 1024, Side2: 768),
        new(Side1: 1280, Side2: 960),
        new(Side1: 1400, Side2: 1050),
        new(Side1: 1600, Side2: 1200),
        new(Side1: 1856, Side2: 1392),
        new(Side1: 1920, Side2: 1440),
        new(Side1: 2048, Side2: 1536),

        // 4:9
        new(Side1: 1080, Side2: 566),
        new(Side1: 960, Side2: 540),
        new(Side1: 828, Side2: 464),
        new(Side1: 750, Side2: 422),
        new(Side1: 640, Side2: 360),
        new(Side1: 576, Side2: 324),
        new(Side1: 500, Side2: 283),
        new(Side1: 480, Side2: 270),
        new(Side1: 400, Side2: 234),
        new(Side1: 320, Side2: 180)
    ];

    private protected record StandardSize(
        int Side1,
        int Side2
    );

    protected enum DimensionsEnum
    {
        // 4:3
        _4_3_640x480,
        _4_3_800x600,
        _4_3_960x720,
        _4_3_1024x768,
        _4_3_1280x960,
        _4_3_1400x1050,
        _4_3_1600x1200,
        _4_3_1856x1392,
        _4_3_1920x1440,
        _4_3_2048x1536,

        // 4:9
        _4_9_1080x566,
        _4_9_960x540,
        _4_9_828x464,
        _4_9_750x422,
        _4_9_640x360,
        _4_9_576x324,
        _4_9_500x283,
        _4_9_480x270,
        _4_9_400x234,
        _4_9_320x180
    }
}