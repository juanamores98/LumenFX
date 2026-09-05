# LumenFX v2

Equilibrio de iluminación, tonemapping y sombras para **Cities: Skylines**, con
presets. Desarrollo original de **juanamores98** — segunda versión con modelo
propio de mezcla de luz y de bias adaptativo.

## Qué hace

**Sun & Sky** (pestaña del tuner, `Ctrl + Alt + L`):
- *Sun strength* (0–2) y *Moon strength* (0–2) independientes: el mezclador
  remuestrea los gradientes día/noche del juego y aplica la ganancia según la
  zona del día con transiciones suaves amanecer/atardecer.
- *Ambience* (0–2): escala los tres gradientes de luz ambiental.
- *Warmth* (−1 frío a +1 cálido): desplazamiento propio de canales R/B sobre
  toda la escena.
- *Sky tonemapping* on/off.

**Tone & Shadows**:
- Brillo, contraste y gamma directos (gamma 1.5–3.5, sin fórmulas anidadas),
  mapeados a la curva filmic del juego con coeficientes propios.
- **Bias adaptativo v2**: modelo propio de respuesta lineal que combina una
  sonda de suelo frontal con la altura de cámara y el ángulo de pitch, con
  escala ajustable (0–2).
- Sombras suaves/hard-only.

**Presets**:
- Guardar/cargar/borrar el look completo como XML (`.lumenfx.xml`).
- **Presets incluidos**: `Vanilla` (todo neutro) y `Optimized` (look cálido
  crepuscular de contraste alto). Se extraen a la carpeta de presets en el
  primer arranque.
- Estado persistente en `LumenFX2.xml`.

## Compatibilidad

- Cities: Skylines en Windows / Linux / macOS. Sin DLCs requeridos.
- Parchea `DayNightProperties.UpdateLighting` (postfix) para el bias adaptativo:
  puede solaparse con otros mods que toquen la iluminación o el shadow bias.

## Requisitos

- **Sí requiere Harmony** (1.2). `0Harmony.dll` va incluida en el repositorio
  (`lib/`) y se despliega junto al mod: no necesita ningún mod externo de Harmony.
- Compila contra .NET Framework 3.5 (el runtime Mono del juego lo provee).

## Instalación

Copiar `LumenFX.dll` y `0Harmony.dll` a:

```
%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\LumenFX\
```

## Uso

- `Ctrl + Alt + L`: abrir/cerrar el tuner.
- Cada pestaña tiene su botón "Reset this tab".
- Pestaña *Presets*: Refresh / Open folder / guardar el look actual.

## Compilación

```
dotnet build -c Release
```

## Licencia

[MIT-0](https://spdx.org/licenses/MIT-0.html) (MIT No Attribution) © 2026 juanamores98.
Uso, copia, modificación, venta, distribución y sublicencia sin atribución ni condiciones.
