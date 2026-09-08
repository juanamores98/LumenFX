# Procedencia y distribución — LumenFX

Actualización 2026-09-08. La licencia propia se mantiene en **MIT-0**; no se cambia autoría ni se reescribe el historial.

## Alcance de esta sesión

Se corrigió el código FX existente usando sus estructuras y las APIs de la instalación local de Cities: Skylines 1. Las tablas de comportamiento del encargo y de la auditoría guiaron los requisitos. No se incorporaron cuerpos de métodos, shaders ni texturas GPL a estas correcciones.

La auditoría previa de esta misma conversación recibió fragmentos de implementación legada. **No se presenta esta sesión como un cuarto limpio aislado ni como una certificación de independencia de toda la historia del repositorio.** La autorización posterior del usuario permite corregir los FX existentes; no convierte código restringido en reutilizable.

| Pieza | Evidencia / revisión | Decisión | Límite o siguiente paso |
|---|---|---|---|
| Código FX de partida | HEAD 0097c9cd9f3a3686dc234d35e4f06a60d8bd678a | Continuar implementación existente; cambios locales trazables | No certificar el historial completo |
| Cambios de persistencia, panel y modos | Diff 2026-09-08 | Implementación sobre FX; helpers compartidos entre los cuatro repos propios | Mantener MIT-0 de contribuciones propias |
| Receta personal DEFAULT | XML activo y SHA256 en SceneFX/docs/Default.reference.txt | Datos de configuración, no implementación de RenderIt Plus | Modelos distintos identificados como aproximaciones |
| Assets LUT de Workshop | Selección por nombre del juego | No copiar ni empaquetar texturas | Cada recurso conserva sus condiciones |
| APIs del juego y Unity | DLL locales; build net35; firmas y unidades comprobadas | Referencias de compilación/runtime | No distribuir DLL del juego como parte del mod |
| Fuentes legadas | Auditoría de comportamiento conservada | No adoptar implementación GPL o sin permiso verificado | Un futuro trasplante de código exige licencia por pieza |

Las copias de Relight, Fog Controller y Eyecandy X usadas en la auditoría no permitieron acreditar una licencia permisiva. Daylight Classic se identificó como GPL-3. Play It! tiene MIT en la revisión oficial `c42ad0424eef8b684505fbc74cfa7fd4d9881b66`; esta corrección no incorpora su código. No se trata a los cinco como un bloque uniforme de GPL.

## Dependencia distribuida

Harmony existente: `lib/0Harmony.dll`, versión de archivo/producto **1.2.0.1**, SHA256 `E271D22A7C32BFCA105D0E471C04EEE14EEC6142D37A30CD73CB7D32C7D1DD0F`.

Licencia MIT comprobada en el [tag oficial v1.2.0.1](https://github.com/pardeike/Harmony/blob/v1.2.0.1/LICENSE). Texto completo en `licenses/Harmony-MIT.txt` y aviso en `NOTICE`. Se mantienen al empaquetar o desplegar junto al binario. La licencia MIT-0 de LumenFX no reemplaza MIT de Harmony.

No se cambió este binario ni se da por comprobada su convivencia con todas las versiones/proveedores de Harmony del usuario.
