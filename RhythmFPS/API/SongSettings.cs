using System.ComponentModel;

namespace RhythmFPS.API;

[Description("곡의 설정을 JSON에서 변환시키기 위한 클래스입니다.")]
public class SongSettings(string[] Lyrics, long[] LyricsTiming, long[] Timing)
{
    [Description("가사입니다.")]
    public string[] Lyrics { get; set; } = Lyrics;

    [Description("가사의 타이밍입니다.")]
    public long[] LyricsTiming { get; set; } = LyricsTiming;

    [Description("타이밍입니다.")]
    public long[] Timing { get; set; } = Timing;
}