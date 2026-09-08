# Estado de sesión — LumenFX

- Fecha: 2026-09-08.
- Repo: `juanamores98/LumenFX`, rama `main`.
- HEAD de partida: `0097c9cd9f3a3686dc234d35e4f06a60d8bd678a`.
- Cambios preparados para commit y push a main por autorización del usuario. Árbol de partida limpio; esta entrega conserva las pruebas offline y no instala el mod.
- Encargo: correcciones, VANILLA/OPTIMIZED y panel estrecho incrustable; MIT/MIT-0; sin integración.
- NeuralFX y SkyFX excluidos.

## Entregado

Se guardan y exportan todas las potencias y propiedades del cielo. Se reconstruye la iluminación en otra ciudad, se capturan/restauran exposición y sky tonemapping, y los presets conservan los nuevos campos. Importación .light por acción explícita y panel nativo compacto.

Receta del perfil activo Default. SHA256 del archivo fuente: `71062f3ac84872ca305db47237e5fd7168bf5b5b629cfff9b24eae45ad6bbfba`. Referencia y limitaciones en `SceneFX/docs/VALIDACION.md`.

## Comprobado

- Build net35/C# 7.3: cero errores.
- Cuatro advertencias MSB3245: referencias implícitas System.Data, System.Drawing, System.Runtime.Serialization y System.Xml.Linq no resueltas en este entorno.
- Suite conjunta: **60 PASS, 0 FAIL**, con .NET 8 y dobles. Código y resultados en `SceneFX/tests/Regression`.
- Build sin instalación; DLL instaladas fuera de este cambio.

## Pendiente y próximo paso

Las temperaturas, tintes, gamma, brillo y bias usan fórmulas propias. La importación .light conserva un significado aproximado; no es conversión visual sin pérdida. Harmony 1.2.0.1 continúa como dependencia existente: falta verificar en el conjunto real de mods del usuario que el parche se instala y convive correctamente.

Prueba agrupada en el juego con los cuatro binarios de esta revisión, después de autorizar despliegue con respaldo y juego cerrado. Matriz en `SceneFX/docs/VALIDACION.md`: primera/segunda ciudad, reinicio, modos repetidos, LUT presente/ausente, UI estrecha, convivencia y comparación con DEFAULT. Registrar observaciones y medidas antes de aprobar integración o afirmar «100 %».
