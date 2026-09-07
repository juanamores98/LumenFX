# LumenFX — arquitectura y cambios

Documento ejecutivo. Estado a día de hoy. `DESIGN.md` es la especificación
funcional original; este documento describe cómo está construido el mod hoy y
qué cambió en el último ciclo.

## Qué manda este mod

Dentro de la suite FX cada propiedad del juego tiene **un solo dueño**. LumenFX
es el dueño de la luz y del tono:

| Materia | Campos del juego |
|---|---|
| Curva solar | `DayNightProperties.m_LightColor` |
| Ambiente | `m_SkyColor`, `m_EquatorColor`, `m_GroundColor` — privados del tipo de ambiente, por reflexión |
| Intensidades | `m_SunIntensity`, `m_MoonIntensity`, `m_Exposure` |
| Tono filmic | `ColossalFramework.ToneMapping` |
| Cielo físico | `m_RayleighScattering`, `m_MieScattering` |
| Sombras | `QualitySettings.shadows`, sesgo adaptativo |

`ActiveClaims` publica en cada momento qué campos está escribiendo, para que los
otros mods de la suite sepan de qué apartarse. SceneFX y ClassicLightFX le piden
el tono y la calidez por `ApplySuiteSection` en vez de escribirlos.

**Theme Mixer tiene prioridad.** `ThemeOwnership` detecta el ensamblado
`ThemeMixer`; cuando está, LumenFX puede escribir valores deliberados sobre los
seis campos de atmósfera que ese mod gestiona, pero **no restaura sus copias
vanilla** al soltar: una copia rancia pisaría el trabajo del dueño.

## Piezas

```
Source/
  LumenFXMod.cs             IUserMod + API de suite (27 etiquetas)
  Core/
    LightingMixer.cs        remuestrea las 4 gradientes del ciclo
    TonemapProfile.cs       mapea brillo y contraste sobre la curva filmic
    LightState.cs           el estado completo del mod
    VanillaSnapshot.cs      la foto del juego sin tocar, para poder volver
    AdaptiveExposure.cs     compensación propia de exposición día/noche
    TunerEngine.cs          MonoBehaviour anfitrión, Ctrl+Alt+L
    ThemeOwnership.cs       cede a Theme Mixer lo que es suyo
  Shadows/
    GroundProbe.cs          raycast contra la malla real para medir el suelo
    AdaptiveBias.cs         curva de sesgo según esa medida
    UpdateLightingPatch.cs
  Presets/PresetLibrary.cs  recetas, incluido el importador de *.light
  IO/StateStore.cs          persistencia con guardado diferido
  UI/TunerWindow.cs         ventana IMGUI de 4 pestañas
```

### Las gradientes

`LightingMixer` remuestrea las cuatro gradientes del ciclo (directa, cielo,
ecuador, suelo) sobre siete tiempos clave espejo de los del juego:

```
0.23  0.26  0.32  0.50  0.68  0.74  0.77
```

**Siete, y hay un tope explícito de ocho.** Unity no admite más de ocho claves
de color en un `Gradient`; pasarse no da error, degenera la curva. Con nueve, la
gradiente solar colapsaba a dos claves blancas siempre que LumenFX estuviera
activo. El tope está ahora escrito en el código, no en la memoria de nadie.

### El sesgo de sombras

`GroundProbe` lanza un rayo contra la malla real del juego para saber a qué
altura está el suelo bajo la cámara, y `AdaptiveBias` traduce esa distancia en
un sesgo. Es lo que evita el acné de sombra sin despegarlas del terreno.

## API de suite

`ApplySuiteSection` / `ExportSuiteSection`, públicas y estáticas. 27 etiquetas,
todas las que se aplican se exportan:

```
sunStrength moonStrength ambience warmth
sunTemp sunTint moonTemp moonTint skyTemp skyTint globalTint twilightTint
skyTonemapping brightness contrast gamma
adaptiveShadows forceLowBias biasScale softShadows
adaptiveExposure adaptiveExposureGain
skyRayleigh skyMie sunPower moonPower vanillaMode
```

## Dónde guarda las cosas

`%LOCALAPPDATA%\Colossal Order\Cities_Skylines\LumenFX2.xml`. Ruta completa, no
relativa, por la misma razón que el resto de la suite.

## Qué cambió en este ciclo

**El brillo llega hasta donde llegaba Relight.** La norma de la suite es
conservar el rango más ancho de los mods que se sustituyen. Al brillo se le
atribuye en Relight un techo de realce de unas 4,1 veces y aquí la curva se
quedaba en 1,6. `TonemapProfile.BoostFor` es ahora de dos tramos:

- de −1 a 1, **exactamente la fórmula de siempre**, `1 + 0,6b`, hasta 1,6.
  Ninguna receta guardada cambia de aspecto: todas caen en ese tramo;
- por encima de 1 la recta sigue con pendiente 0,84 hasta 4,1 en b = 4.

El techo de 4,1 sale de un informe externo, no de haber leído el código de
Relight, que no tiene licencia y no se toca. Si algún día se mide de verdad y
sale otro número, el que cambia es el segundo tramo.

**`ToggleWindow()`** — método público y estático para que otro mod de la suite
pueda abrir este panel sin que el usuario tenga que acordarse del atajo. Guardar
al cerrar pasa también por ahí, o cerrar desde fuera perdería lo último tocado.

**Interfaz.** Cuatro pestañas con scroll propio en vez de un muro continuo, y el
balance de color agrupado por momento del día en vez de por variable.

## Correcciones de la revisión

- **Sin emoji.** La fuente Arial de Unity 5.6 no lleva pictogramas. Se comprobó
  sobre 12.960 ficheros `.cs` de mods que ya funcionan: ninguno los usa.
- **La ventana vuelve a estar entera en inglés.** Antes del rediseño no había ni
  un literal con acento castellano; se habían colado diez, mezclados con los
  deslizadores en inglés que nadie tocó. Una interfaz a dos idiomas.
- **La aislación de los deslizadores sigue en pie** (verificado): cada
  deslizador guarda `GUI.changed`, lo pone a falso, dibuja y lo restaura. Sin
  eso, redondear al paso reescribía la receta del usuario sin que la tocara
  —0,00006 a 0,00005— y provocaba tirones.

## Atajos

- `Ctrl+Alt+L` — ventana del mod.

## Licencia

MIT-0 © 2026 juanamores98. Sin atribución ni condiciones.
