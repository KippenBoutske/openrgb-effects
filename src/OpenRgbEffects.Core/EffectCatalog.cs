namespace OpenRgbEffects.Core;

[Flags]
public enum EffectFeatures
{
    None = 0,
    Speed = 1,
    Color = 2,
    Color2 = 4,
    Palette = 8,
    Direction = 16,
    Audio = 32,
    Temperature = 64,
    Ambient = 128
}

public sealed record EffectDescriptor(
    EffectKind Kind,
    EffectCategory Category,
    string Name,
    string Description,
    EffectFeatures Features);

public static class EffectCatalog
{
    public static IReadOnlyList<EffectDescriptor> All { get; } =
    [
        new(EffectKind.Static, EffectCategory.Classic, "Statische kleur", "Alle LEDs op één kleur.", EffectFeatures.Color),
        new(EffectKind.Breathing, EffectCategory.Classic, "Ademhaling", "De gekozen kleur pulseert.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.Rainbow, EffectCategory.Classic, "Regenboog", "Alle LEDs cyclen samen door de kleurcirkel.", EffectFeatures.Speed),
        new(EffectKind.Wave, EffectCategory.Classic, "Golf", "Een regenboog loopt over de LEDs.", EffectFeatures.Speed | EffectFeatures.Direction),
        new(EffectKind.ColorFade, EffectCategory.Classic, "Kleurfade", "Alle LEDs faden langs het palet.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.BreathingMulti, EffectCategory.Classic, "Ademhaling (meerkleurig)", "Ademt terwijl de kleur door het palet schuift.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.ColorWipe, EffectCategory.Motion, "Kleurveeg", "Vult de strip kleur voor kleur.", EffectFeatures.Speed | EffectFeatures.Palette | EffectFeatures.Direction),
        new(EffectKind.TheaterChase, EffectCategory.Motion, "Theaterchase", "Lopende groepen LEDs, zoals marquee-lampen.", EffectFeatures.Speed | EffectFeatures.Palette | EffectFeatures.Direction),
        new(EffectKind.Comet, EffectCategory.Motion, "Komeet", "Heldere kop met een uitstervende staart.", EffectFeatures.Speed | EffectFeatures.Color | EffectFeatures.Direction),
        new(EffectKind.Scanner, EffectCategory.Motion, "Knight Rider", "Scannerbalk die heen en weer beweegt.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.RainbowChase, EffectCategory.Motion, "Regenboogjacht", "Een regenboog schuift langs de LEDs.", EffectFeatures.Speed | EffectFeatures.Direction),
        new(EffectKind.BouncingBalls, EffectCategory.Motion, "Stuiterballen", "Meerdere ballen stuiteren over de strip.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.GradientSpin, EffectCategory.Motion, "Gradiëntspin", "Het palet draait als een wiel.", EffectFeatures.Speed | EffectFeatures.Palette | EffectFeatures.Direction),
        new(EffectKind.Twinkle, EffectCategory.Atmosphere, "Twinkel", "Willekeurige LEDs vonken op.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Fire, EffectCategory.Atmosphere, "Vuur", "Flikkerend vuur van onderaf.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Candle, EffectCategory.Atmosphere, "Kaars", "Warme, onregelmatige kaarsvlam.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.Plasma, EffectCategory.Atmosphere, "Plasma", "Vloeiende interferentiegolven.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Aurora, EffectCategory.Atmosphere, "Noorderlicht", "Groene en paarse gordijnen.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Ocean, EffectCategory.Atmosphere, "Oceaan", "Diepblauw met bewegende hooglichten.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Lava, EffectCategory.Atmosphere, "Lava", "Langzame hete vlekken.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.Starfield, EffectCategory.Atmosphere, "Sterrenveld", "Sterren die dichterbij komen.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.Lightning, EffectCategory.Atmosphere, "Onweer", "Donkere lucht met bliksemflitsen.", EffectFeatures.Speed | EffectFeatures.Color2),
        new(EffectKind.MatrixRain, EffectCategory.Atmosphere, "Matrixregen", "Groene digitale druppels.", EffectFeatures.Speed | EffectFeatures.Color | EffectFeatures.Direction),
        new(EffectKind.Strobe, EffectCategory.Alert, "Stroboscoop", "Harde aan/uit-flitsen.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.Police, EffectCategory.Alert, "Politielichten", "Rood-blauw wisselend.", EffectFeatures.Speed | EffectFeatures.Color | EffectFeatures.Color2),
        new(EffectKind.Heartbeat, EffectCategory.Alert, "Hartslag", "Dubbele pulsslag.", EffectFeatures.Speed | EffectFeatures.Color),
        new(EffectKind.RandomLeds, EffectCategory.Alert, "Willekeurige LEDs", "Elke LED krijgt langzaam een nieuwe kleur.", EffectFeatures.Speed | EffectFeatures.Palette),
        new(EffectKind.MusicVu, EffectCategory.Reactive, "Muziek VU-meter", "Vult de strip met het Windows-geluidsniveau.", EffectFeatures.Palette | EffectFeatures.Audio),
        new(EffectKind.MusicBeat, EffectCategory.Reactive, "Muziek beatpuls", "Flitst op de beat van de loopback-audio.", EffectFeatures.Color | EffectFeatures.Audio),
        new(EffectKind.MusicSpectrum, EffectCategory.Reactive, "Muziekspectrum", "Bass tot treble over de LEDs.", EffectFeatures.Palette | EffectFeatures.Audio),
        new(EffectKind.Temperature, EffectCategory.Reactive, "CPU/GPU-temperatuur", "Koudblauw tot heetrood op basis van sensors.", EffectFeatures.Temperature),
        new(EffectKind.ScreenAmbient, EffectCategory.Reactive, "Scherm-ambient", "Gemiddelde kleur van het scherm.", EffectFeatures.Ambient)
    ];

    public static EffectDescriptor Get(EffectKind kind) =>
        All.FirstOrDefault(item => item.Kind == kind) ?? All[0];

    public static string CategoryName(EffectCategory category) => category switch
    {
        EffectCategory.Classic => "Klassiek",
        EffectCategory.Motion => "Beweging",
        EffectCategory.Atmosphere => "Sfeer",
        EffectCategory.Alert => "Alert",
        EffectCategory.Reactive => "Reactief",
        _ => category.ToString()
    };

    public static bool Has(this EffectFeatures features, EffectFeatures flag) => (features & flag) != 0;
}
