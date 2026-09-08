# LumenFX

Iluminación, ambiente, tono y sombras para Cities: Skylines 1, con un modelo propio de mezcla de los gradientes del mapa.

## Uso

- Abrir con **Ctrl+Alt+L**, o UUI opcional.
- Panel nativo preferido: **360 × 680**, mínimo 280 × 260. Secciones: Light, Colour, Tone, Presets.
- **VANILLA** libera las modificaciones del módulo y guarda ese modo. Restaura la referencia capturada respetando compañeros detectados; un tema u otro mod puede hacer que difiera del vanilla puro.
- **OPTIMIZED** aplica la parte de este módulo del **Default personal de RenderIt Plus**. Es una receta de aspecto, no de rendimiento.
- Editar controles guarda un estado personalizado. El pie distingue VANILLA, OPTIMIZED y CUSTOM según los valores configurados.
- Los sliders incluyen entrada decimal y refresco sin escrituras por repaint.

Sol 5.5, luna 6, Rayleigh 1.116, Mie 1.858 y exposición fija 1.102. Tono FX: gamma 3.15, brillo -0.4, contraste -0.7 y calidez 0.4. Los valores de tono/calidez son conversiones existentes, no equivalencia visual demostrada con Relight.

## Persistencia

Archivos globales: **LumenFX2.xml**, bajo `%LOCALAPPDATA%\Colossal Order\Cities_Skylines`. Independientes de la partida. Temporal y reemplazo con copia `.bak`, pendientes que se reintentan y guardado al cerrar el anfitrión. Se conserva lectura desde la ubicación histórica cuando procede. Un cierre forzado durante el intervalo de guardado puede perder el último cambio pendiente.

Los built-ins no sobrescriben presets ya extraídos del usuario. Los botones de modo leen la receta incorporada; un antiguo archivo llamado Optimized puede contener valores distintos.

## Incrustación futura

`FxModule.CreatePanel(parent, width, height)` crea el panel dentro de un `UIComponent`. Con padre no tiene arrastre ni botón de ventana. Ofrece `ReadState`, `ApplyState`, `Release`, `ApplyOptimized`, `Flush`, `Mode` y `Status`. Ver [arquitectura](ARQUITECTURA.md).

No se ha integrado con RenderIt Plus ni Arrebol; tampoco hay dependencia de esos productos.

## Compilar y verificar

```powershell
dotnet build LumenFX.csproj -c Release
```

Target **net35 / C# 7.3**, referencias de CS1 instalado. El build normal genera `bin/Release/net35` y **no instala**. El target de despliegue requiere `DeployMod=true`; solo debe utilizarse con autorización, juego cerrado y respaldo.

Regresiones conjuntas: `SceneFX/tests/Regression/Regression.csproj`, que enlaza el código actual de los cuatro repos hermanos. Prueba lógica en .NET 8 con dobles del motor; no valida render ni interacción visual.

## Límites

Las temperaturas, tintes, gamma, brillo y bias usan fórmulas propias. La importación .light conserva un significado aproximado; no es conversión visual sin pérdida. Harmony 1.2.0.1 continúa como dependencia existente: falta verificar en el conjunto real de mods del usuario que el parche se instala y convive correctamente.

[Paridad](docs/PARIDAD.md) · [Estado](docs/ESTADO-SESION.md) · [Procedencia](PROCEDENCIA.md). `DESIGN.md` se conserva como referencia histórica.

Código propio bajo **MIT-0**, [LICENSE](LICENSE).

Harmony 1.2.0.1 conserva MIT, con aviso en `NOTICE` y texto completo en `licenses/Harmony-MIT.txt`. Sigue pendiente verificar su convivencia con el proveedor de Harmony de la instalación.
