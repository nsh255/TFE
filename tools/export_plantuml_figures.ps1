param(
  [string]$Root = (Resolve-Path "$PSScriptRoot\.." | Select-Object -ExpandProperty Path),
  [int]$Scale = 4
)

$ErrorActionPreference = 'Stop'

$jar = Join-Path $Root 'tools\plantuml\plantuml.jar'
if (!(Test-Path -LiteralPath $jar)) {
  throw "Missing PlantUML jar at: $jar"
}

$diagramDir = Join-Path $Root 'Ascension\Ascension\LATEX\Diagramas'
$figDir = Join-Path $Root 'Ascension\Ascension\LATEX\Figuras'

$pumlNames = @(
  'figura3_1_arquitectura_componentes.puml',
  'estados_juego.puml',
  'flujo_escenas_simple.puml',
  'dependencias_modulos.puml',
  'fsm_slimegreen.puml',
  'fsm_slimeblue.puml',
  'Diagrama_Clases_seccion1.puml',
  'Diagrama_Clases_seccion2.puml',
  'Diagrama_Clases_seccion3.puml'
)

if (!(Test-Path -LiteralPath $diagramDir)) { throw "Missing: $diagramDir" }
if (!(Test-Path -LiteralPath $figDir)) { throw "Missing: $figDir" }

# Export directly into Figuras/ with the same basename.
Push-Location $diagramDir
try {
  foreach ($name in $pumlNames) {
    $src = Join-Path $diagramDir $name
    if (!(Test-Path -LiteralPath $src)) {
      Write-Warning "Not found: $src"
      continue
    }

    Write-Host "Exporting $name -> Figuras (png, scale=$Scale)" -ForegroundColor Cyan
    & java '-Djava.awt.headless=true' '-DPLANTUML_LIMIT_SIZE=16384' -jar $jar -tpng -scale $Scale -o "..\Figuras" $name
    if ($LASTEXITCODE -ne 0) { throw "PlantUML failed for $name" }
  }
} finally {
  Pop-Location
}

Write-Host "Done. Output written to: $figDir" -ForegroundColor Green
