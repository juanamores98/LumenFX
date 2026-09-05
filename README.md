# LumenFX v2

Equilibrio de iluminaciÃ³n, tonemapping y sombras para **Cities: Skylines**, con
presets. Desarrollo original de **juanamores98** â€” segunda versiÃ³n con modelo
propio de mezcla de luz y de bias adaptativo.

## QuÃ© hace

**Sun & Sky** (pestaÃ±a del tuner, `Ctrl + Alt + L`):
- *Sun strength* (0â€“2) y *Moon strength* (0â€“2) independientes: el mezclador
  remuestrea los gradientes dÃ­a/noche del juego y aplica la ganancia segÃºn la
  zona del dÃ­a con transiciones suaves amanecer/atardecer.
- *Ambience* (0â€“2): escala los tres gradientes de luz ambiental.
- *Warmth* (âˆ’1 frÃ­o a +1 cÃ¡lido): desplazamiento propio de canales R/B sobre
  toda la escena.
- *Sky tonemapping* on/off.

**Tone & Shadows**:
- Brillo, contraste y gamma directos (gamma 1.5â€“3.5, sin fÃ³rmulas anidadas),
  mapeados a la curva filmic del juego con coeficientes propios.
- **Bias adaptativo v2**: modelo propio de respuesta lineal que combina una
  sonda de suelo frontal con la altura de cÃ¡mara y el Ã¡ngulo de pitch, con
  escala ajustable (0â€“2).
- Sombras suaves/hard-only.

**Presets**:
- Guardar/cargar/borrar el look completo como XML (`.lumenfx.xml`).
- **Presets incluidos**: `Vanilla` (todo neutro) y `Optimized` (look cÃ¡lido
  crepuscular de contraste alto). Se extraen a la carpeta de presets en el
  primer arranque.
- Estado persistente en `LumenFX2.xml`.

## Compatibilidad

- Cities: Skylines en Windows / Linux / macOS. Sin DLCs requeridos.
- Parchea `DayNightProperties.UpdateLighting` (postfix) para el bias adaptativo:
  puede solaparse con otros mods que toquen la iluminaciÃ³n o el shadow bias.

## Requisitos

- **SÃ­ requiere Harmony** (1.2). `0Harmony.dll` va incluida en el repositorio
  (`lib/`) y se despliega junto al mod: no necesita ningÃºn mod externo de Harmony.
- Compila contra .NET Framework 3.5 (el runtime Mono del juego lo provee).

## InstalaciÃ³n

Copiar `LumenFX.dll` y `0Harmony.dll` a:

```
%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\LumenFX\
```

## Uso

- `Ctrl + Alt + L`: abrir/cerrar el tuner.
- Cada pestaÃ±a tiene su botÃ³n "Reset this tab".
- PestaÃ±a *Presets*: Refresh / Open folder / guardar el look actual.

## CompilaciÃ³n

```
dotnet build -c Release
```

## Licencia

[MIT-0](https://spdx.org/licenses/MIT-0.html) (MIT No Attribution) Â© 2026 juanamores98.
Uso, copia, modificaciÃ³n, venta, distribuciÃ³n y sublicencia sin atribuciÃ³n ni condiciones.
