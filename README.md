# OpenRGB Effects

Windows-app die RGB-verlichting aanstuurt via de **OpenRGB SDK-server**. Effecten draaien in de app zelf en sturen ongeveer 30 keer per seconde LED-updates over TCP (standaard `127.0.0.1:6742`).

Getest tegen hardware zoals een **MSI X670E-moederbord** en **ENE DRAM RGB-geheugen**, maar elk apparaat dat OpenRGB in Direct/Custom-modus kan zetten werkt.

De interface is in het Nederlands en gebruikt een donker thema.

## Wat je nodig hebt

1. [OpenRGB](https://openrgb.org/) installeren op Windows.
2. OpenRGB **als administrator** starten (nodig voor MSI Mystic Light en veel DRAM-controllers).
3. SDK-server aanzetten:
   - ga naar het tabblad **SDK Server**
   - poort `6742` laten staan (of noteer een andere poort)
   - klik op **Start Server**
   - optioneel: *Auto-Start Server* aanvinken zodat OpenRGB de server bij de volgende start meeneemt
4. Zet apparaten in OpenRGB bij voorkeur op modus **Direct** of **Custom**. De app probeert dat zelf ook.

Zonder draaiende SDK-server blijft de status *Niet verbonden*.

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
4. Vink de apparaten aan waarop het effect mag draaien (moederbord, DRAM, …). Zones worden ter informatie getoond.
5. Kies een effect:
   - **Statische kleur** (standaard wit)
   - **Ademhaling**
   - **Regenboog / kleurenwiel**
   - **Golf over LEDs**
6. Stel kleur, snelheid en helderheid in.
7. Klik op **Starten**. **Stoppen** zet de gekozen apparaten terug op statisch wit.

Instellingen (laatste effect, kleur, snelheid, helderheid, apparaatkeuze, host/poort) worden bewaard in `%AppData%\OpenRGB Effects\settings.json`.

Optioneel:

- **Starten met Windows** — zet een register-snelkoppeling onder de huidige gebruiker (`HKCU\...\Run`).
- **Minimaliseren naar systeemvak** — sluiten of minimaliseren verbergt de app; rechtsklik het icoon voor Openen / Starten / Stoppen / Afsluiten.
- **Laatste effect hervatten bij start** — na verbinden start het laatst actieve effect automatisch opnieuw.

## Effecten

| Effect | Gedrag |
| --- | --- |
| Statische kleur | Alle LEDs van de gekozen apparaten op de gekozen kleur |
| Ademhaling | De gekozen kleur pulseert |
| Regenboog | Alle LEDs cyclen samen door de kleurcirkel |
| Golf | Een regenboog loopt over de LEDs van elk apparaat |

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
src/OpenRgbEffects.Core   Effecten, instellingen, OpenRGB.NET-client
src/OpenRgbEffects.App    WPF-interface (Nederlands, donker)
tests/OpenRgbEffects.Tests
.github/workflows/build.yml
```

De app praat met OpenRGB via [OpenRGB.NET](https://github.com/diogotr7/OpenRGB.NET) en het officiële SDK-protocol. OpenRGB zelf blijft de driver; deze app is alleen de effectenclient.
