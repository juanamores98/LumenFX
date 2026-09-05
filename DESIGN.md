# Diseño de LumenFX v2

Especificación funcional de la segunda versión: modelo de mezcla de luz, perfil
de tonemapping y bias adaptativo propios.

## Objetivo

Ofrecer un control de la iluminación del juego que parte de los datos del propio
juego (sus gradientes día/noche) en lugar de sustituirlos por una paleta fija,
más un sistema de sombras con bias adaptativo de curva propia.

## Modelo de mezcla de luz (v2)

1. Se toman los gradientes del juego (luz directa y los tres ambientales) y se
   remuestrean en 6 claves propias: `t = 0, 0.2, 0.4, 0.6, 0.8, 1`.
2. La luz directa aplica una ganancia por **zona del día**: `SunStrength` en la
   banda central, `MoonStrength` de noche, con transiciones lineales en
   amanecer (0.20–0.32) y atardecer (0.68–0.80).
3. Los ambientales aplican `Ambience` como ganancia uniforme.
4. El desplazamiento de `Warmth` (−1..1) modifica los canales R y B
   (`±18 %` en directa, `±9 %` en ambiental) y se re-satura a [0,1].
5. El resultado se escribe de vuelta en `DayNightProperties` (luz directa
   directamente; ambientales vía campos privados `m_SkyColor`, `m_EquatorColor`,
   `m_GroundColor` por reflexión).

## Perfil de tonemapping (v2)

Controles directos y su mapeo a `ColossalFramework.ToneMapping`:
| Control | Rango | Mapeo |
|---|---|---|
| Gamma | 1.5–3.5 | `m_ToneMappingGamma` directo |
| Brightness | −1..1 | boost factor `1 + 0.6·b` |
| Contrast | −1..1 | filmic: `A=0.5+0.2c, B=0.25−0.15c, C=0.10−0.01c, D=0.70+0.20c, E=0.01, F=0.25−0.12c, W=11.2+2.5c`, luminancia `0.10+0.02c` |

## Bias adaptativo (v2)

- Una única sonda de suelo frontal (raycast con flags abiertos, máximo 1500 m).
- Altura efectiva = 50 % altura de cámara sobre terreno + 50 % sonda.
- Respuesta lineal: `bias = lerp(0.05, 1, alturaNormalizada · lerp(1, 0.35, pitch)) · escala`,
  con clamp final [0.05, 2]. Sin curvas exponenciales.
- Se aplica como postfix de Harmony en `DayNightProperties.UpdateLighting`.

## Estado y presets

- Estado: `LumenFX2.xml` (raíz `lumenFx`, `schema="2"`), guardado al cambiar
  cualquier control del tuner.
- Presets: XML `.lumenfx.xml` en `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\ModConfig\LumenFXPresets`.

## Arquitectura

- `Core/LightState` — estado completo del tuner.
- `Core/LightingMixer` — modelo de mezcla (remuestreo + zonas + warmth).
- `Core/TonemapProfile` — perfil filmic.
- `Shadows/AdaptiveBias` + `GroundProbe` + `UpdateLightingPatch` — sombras.
- `IO/StateStore`, `Presets/PresetLibrary` — persistencia propia.
- `UI/TunerWindow` + `Core/TunerEngine` — tuner in-game (Ctrl+Alt+L).
