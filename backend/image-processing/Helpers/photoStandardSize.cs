namespace image_processing.Helpers;

public class PhotoStandardSize
{
    private protected static readonly StandardSize[] Dimensions =
    [
        // 4:3
        new(640, 480),
        new(800, 600),
        new(960, 720),
        new(1024, 768),
        new(1280, 960),
        new(1400, 1050),
        new(1600, 1200),
        new(1856, 1392),
        new(1920, 1440),
        new(2048, 1536),

        // 4:9
        new(1080, 566),
        new(960, 540),
        new(828, 464),
        new(750, 422),
        new(640, 360),
        new(576, 324),
        new(500, 283),
        new(480, 270),
        new(400, 234),
        new(320, 180)
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