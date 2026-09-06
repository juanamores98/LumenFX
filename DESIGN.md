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
- **Rendimiento v2.1**: Caché estática de `CameraController` y `Camera` (sin `FindWithTag` en cada frame). Estrangulación analítica: el raycast solo se recalcula si la cámara se mueve más de 0.5 m, rota más de 0.5° o cada 20 ticks. Estructuras de raycast pre-inicializadas con cero allocaciones en el heap por frame.

## Exposición adaptativa día/noche (v2.1)

- Algoritmo analítico propio que compensa suavemente la iluminación nocturna y crepuscular sin requerir histogramas HDR por frame ni cómputo de GPU pesado.
- Curva hermite evaluada a partir de `DayNightProperties.normalizedTimeOfDay`: calcula la distancia al mediodía (0.5) y aplica un factor multiplicador suave `1 + 0.4·gain·nightWeight` sobre la exposición capturada en `VanillaSnapshot`.
- Se evalúa únicamente cuando la hora del día avanza de forma apreciable (delta >= 0.002) o cambian las opciones.
- Controlable desde la pestaña "Tone & Shadows" del tuner (`adaptiveExposure` y `adaptiveExposureGain`).

## Estado y presets

- Estado: `LumenFX2.xml` (raíz `lumenFx`, `schema="2"`), con throttle de guardado a disco (1 segundo entre escrituras para evitar congelamientos en sliders) y guardado forzado al cerrar la ventana o salir de la escena.
- Presets: XML `.lumenfx.xml` en `%LOCALAPPDATA%\Colossal Order\Cities_Skylines\ModConfig\LumenFXPresets`.
- **Coordenadas de ventana**: `windowX` y `windowY` persistidas en el estado para restaurar la posición de la ventana del tuner con clamp a los límites de pantalla.
- **Built-ins**: `Vanilla.lumenfx.xml` (todo neutro, gamma 2.2) y
  `Optimized.lumenfx.xml`. Se extraen al primer arranque si no existen.

### Integración de Suite

- Entry point expone `public static bool ApplySuiteSection(string xml)` y `public static string ExportSuiteSection()`.
- Permite a SceneFX (coordinador) aplicar o exportar la sección `<lumenfx>` dentro de un perfil unificado `.suite.xml`.

### Traducción documentada del preset `Optimized`

Valores de partida: un perfil de referencia personal del autor (mezzcla cálida
crepuscular con contraste alto). Equivalencias usadas:

| Origen (referencia personal) | v2 | Nota |
|---|---|---|
| Temperatura global cálida + tinte | `warmth 0.4` | desplazamiento R/B equivalente (~7 %) |
| Menor relación noche/día | `moonStrength 0.5` | ratio noche/día ≈ 0.48 |
| Brillo alto con divisor de intensidad | `brightness -0.4` | el rango propio satura; ajuste a ojo |
| Contraste negativo | `contrast -0.7` | directo |
| Gamma via fórmula `2.6·(((g+1)/4)+0.75)` con g=0.85 | `gamma 3.15` | 3.1525 redondeado |
| Suavizado/bias por defecto | `adaptiveShadows true, biasScale 1, softShadows true` | |

## Arquitectura

- `Core/LightState` — estado completo del tuner con dirty flag y coordenadas de ventana.
- `Core/LightingMixer` — modelo de mezcla (remuestreo + zonas + warmth) con reflexión cacheada y chequeo dirty.
- `Core/AdaptiveExposure` — compensación analítica de exposición día/noche.
- `Core/TonemapProfile` — perfil filmic.
- `Shadows/AdaptiveBias` + `GroundProbe` + `UpdateLightingPatch` — sombras y bias adaptativo estrangulado.
- `IO/StateStore`, `Presets/PresetLibrary` — persistencia propia con throttle.
- `UI/TunerWindow` + `Core/TunerEngine` — tuner in-game (Ctrl+Alt+L) con estilo e identidad visual unificada.

