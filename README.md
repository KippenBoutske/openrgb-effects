# OpenRGB Effects

Windows-app die RGB-verlichting aanstuurt via de **OpenRGB SDK-server**. Effecten draaien in de app zelf en sturen ongeveer 30 keer per seconde LED-updates over TCP (standaard `127.0.0.1:6742`).

Getest tegen hardware zoals een **MSI X670E-moederbord** en **ENE DRAM RGB-geheugen**, maar elk apparaat dat OpenRGB in Direct/Custom-modus kan zetten werkt.

De interface is in het Nederlands en gebruikt een donker thema. Er is een live preview-strip, paletten, favorieten en zoeken per categorie.

## Wat je nodig hebt

1. [OpenRGB](https://openrgb.org/) installeren op Windows.
2. OpenRGB **als administrator** starten (nodig voor MSI Mystic Light en veel DRAM-controllers).
3. SDK-server aanzetten:
   - ga naar het tabblad **SDK Server**
   - poort `6742` laten staan (of noteer een andere poort)
   - klik op **Start Server**
   - optioneel: *Auto-Start Server* aanvinken zodat OpenRGB de server bij de volgende start meeneemt
4. Zet apparaten in OpenRGB bij voorkeur op modus **Direct** of **Custom**. De app probeert dat zelf ook.

Zonder draaiende SDK-server blijft de status *Niet verbonden*. De preview werkt wel.

## Installeren

Elke push naar `main` bouwt een Windows-exe en een installatieprogramma via GitHub Actions.

1. Open de [Actions](https://github.com/KippenBoutske/openrgb-effects/actions) van deze repo.
2. Kies de laatste groene **Build**-run op `main`.
3. Download het artifact **OpenRgbEffects-win-x64**.
4. Pak het uit. Je vindt:
   - `OpenRgbEffects.exe` — zelfstandige app, geen .NET-installatie nodig
   - `OpenRgbEffects-Setup.exe` — optionele per-gebruiker-installer met startmenu-snelkoppeling

De app heeft geen extra runtime nodig (self-contained .NET 8).

## Gebruik

1. Start OpenRGB als administrator en zet de SDK-server aan.
2. Start **OpenRGB Effects**.
3. Controleer host/poort (standaard `127.0.0.1` / `6742`) en klik op **Verbinden**.
4. Vink de apparaten aan waarop het effect mag draaien.
5. Zoek of filter effecten (Alle / Favorieten / categorie). Markeer favorieten met ★.
6. Stel palet, kleuren, richting, snelheid en helderheid in. De preview-strip toont het effect live.
7. Klik op **Starten**. **Stoppen** zet de gekozen apparaten terug op statisch wit.

Instellingen (laatste effect, kleuren, palet, favorieten, snelheid, helderheid, apparaatkeuze, host/poort) worden bewaard in `%AppData%\OpenRGB Effects\settings.json`.

Optioneel:

- **Starten met Windows** — register-snelkoppeling onder de huidige gebruiker.
- **Minimaliseren naar systeemvak**
- **Laatste effect hervatten bij start**

## Effecten

### Klassiek
| Effect | Gedrag |
| --- | --- |
| Statische kleur | Alle LEDs op één kleur |
| Ademhaling | Gekozen kleur pulseert |
| Regenboog | Alle LEDs cyclen samen door de kleurcirkel |
| Golf | Regenboog loopt over de LEDs |
| Kleurfade | Alle LEDs faden langs het palet |
| Ademhaling (meerkleurig) | Ademt terwijl de kleur door het palet schuift |

### Beweging
| Effect | Gedrag |
| --- | --- |
| Kleurveeg | Vult de strip kleur voor kleur |
| Theaterchase | Lopende groepen, zoals marquee-lampen |
| Komeet | Heldere kop met staart |
| Knight Rider | Scannerbalk heen en weer |
| Regenboogjacht | Regenboog schuift langs de LEDs |
| Stuiterballen | Meerdere ballen op de strip |
| Gradiëntspin | Palet draait als een wiel |

### Sfeer
| Effect | Gedrag |
| --- | --- |
| Twinkel | Willekeurige vonken |
| Vuur | Flikkerend vuur |
| Kaars | Warme, onregelmatige vlam |
| Plasma | Vloeiende interferentiegolven |
| Noorderlicht | Groene en paarse gordijnen |
| Oceaan | Diepblauw met hooglichten |
| Lava | Langzame hete vlekken |
| Sterrenveld | Sterren die dichterbij komen |
| Onweer | Donkere lucht met bliksem |
| Matrixregen | Groene digitale druppels |

### Alert
| Effect | Gedrag |
| --- | --- |
| Stroboscoop | Harde aan/uit-flitsen |
| Politielichten | Rood-blauw wisselend |
| Hartslag | Dubbele pulsslag |
| Willekeurige LEDs | Elke LED krijgt langzaam een nieuwe kleur |

### Reactief
| Effect | Gedrag |
| --- | --- |
| Muziek VU-meter | Vult de strip met Windows-loopbackaudio (NAudio WASAPI) |
| Muziek beatpuls | Flitst op de beat |
| Muziekspectrum | Bass tot treble over de LEDs |
| CPU/GPU-temperatuur | Koudblauw tot heetrood (LibreHardwareMonitor) |
| Scherm-ambient | Gemiddelde kleur van het scherm |

Muziekeffecten gebruiken de Windows-standaardweergave via WASAPI-loopback. Speel audio af op dat apparaat. Temperatuur werkt het beste als je de app als administrator start. Zonder sensors blijven die effecten een stille fallback tonen.

Paletpresets: regenboog, zonsondergang, oceaan, bos, vuur, ijs, neon, pastel, cyberpunk, snoep, mono.

Stoppen (en afsluiten) herstelt **statisch wit**.

## Zelf bouwen

Vereisten: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) en Windows voor de WPF-app.

```powershell
dotnet test tests/OpenRgbEffects.Tests/OpenRgbEffects.Tests.csproj -c Release
dotnet publish src/OpenRgbEffects.App/OpenRgbEffects.App.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -o publish
```

De unit tests van de effecten en instellingen draaien ook op Linux. De WPF-app zelf is Windows-only.

Optioneel installatieprogramma met [Inno Setup](https://jrsoftware.org/isinfo.php):

```powershell
iscc installer/OpenRgbEffects.iss
```

## Projectstructuur

```
src/OpenRgbEffects.Core   Effecten, paletten, instellingen, OpenRGB.NET-client
src/OpenRgbEffects.App    WPF-interface, preview, audio/temperatuur/scherm
tests/OpenRgbEffects.Tests
.github/workflows/build.yml
```

De app praat met OpenRGB via [OpenRGB.NET](https://github.com/diogotr7/OpenRGB.NET). Audio via NAudio WASAPI-loopback, temperatuur via LibreHardwareMonitor.
