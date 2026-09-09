# Paridad de LumenFX

Revisión: 2026-09-08. Requisitos del encargo y de la matriz de comportamiento de `ModdingResearch/EncargosFX/Auditoria-20260908/INFORME.md`. No implica adopción de implementaciones GPL.

| Capacidad | Fuente de requisito | Entrada / unidad / rango | Comportamiento | API comprobada | Destino | Prueba | Estado / diferencia |
|---|---|---|---|---|---|---|---|
| Potencias y cielo | Relight / DEFAULT / H01 | Sol/luna 0–20; Rayleigh/Mie/exposición 0–5; 0 devuelve al mapa | Persistir, exportar y aplicar todos los campos | DayNightProperties | LightState / LightingMixer | L02, L09–L13, L15 | Lógica verificada; igualdad visual no acreditada |
| Ganancias y colores | Relight | Día 0–3; noche/ambiente 0–2; temperaturas/tintes -1–1 | Transformar gradientes del mapa mediante respuesta propia | DayNightProperties.AmbientColor | LightingMixer | L05, L06 + inspección | Mismo tipo de controles, modelo distinto |
| Tono | Relight / Eyecandy X | Gamma 1.5–3.5; brillo -1–4; contraste -1–1 | Aplicar perfil filmic propio | ColossalFramework.ToneMapping | TonemapProfile | L01, L03, L07, L14 | No promete igualdad con el modelo legado |
| Sombras y exposición adaptativa | Relight / funciones propias | Interruptores, escala de bias 0–2, ganancia de exposición 0–1 | Postfix de actualización de luz; low bias depende del modo adaptativo | DayNightProperties.UpdateLighting / Light.shadowBias | Shadows | Build real + inspección | Parche/imagen/FPS pendientes en juego |
| Presets e importación | Relight / H01 / H10 | XML .lumenfx.xml y lectura explícita .light | Capturar todos los campos; importación de valores, sin texturas | XML y archivos locales/Workshop instalados | PresetLibrary | L04, L10 | Negativos ya no colapsan al neutro; conversión aproximada |
| Modo, segunda ciudad y errores | Encargo / H02 / H06 / H09 | VANILLA, OPTIMIZED, XML global | Recapturar por nivel; no confirmar cargas parciales | Snapshots / XML | TunerRuntime / StateStore | L06–L08, L12–L15, IO02 | 60 comprobaciones conjuntas; no prueba de sesiones Unity |

Las pruebas citadas están en `SceneFX/tests/Regression/Program.cs`. Firmas compiladas contra DLL reales; aserciones ejecutadas con dobles, no Unity. UI, imagen, tiempo de respuesta y rendimiento pendientes de observación.

**No se declara paridad total.** Las temperaturas, tintes, gamma, brillo y bias usan fórmulas propias. La importación .light conserva un significado aproximado; no es conversión visual sin pérdida. Harmony 1.2.0.1 continúa como dependencia existente: falta verificar en el conjunto real de mods del usuario que el parche se instala y convive correctamente.

Para la cobertura de lo que en Render It! Plus tiene licencia restrictiva -Relight, Fog Controller, Eyecandy X y Daylight Classic, que es GPL-3.0- el documento es `SceneFX/docs/RELEVO-RENDERIT-PLUS.md`.
