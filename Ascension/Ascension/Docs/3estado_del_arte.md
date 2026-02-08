Estado del arte
Definición técnica del subgénero rogue-like

El subgénero rogue-like se caracteriza por la generación procedural de niveles, la ausencia de persistencia entre partidas mediante mecánicas de muerte permanente y un alto grado de rejugabilidad. En este tipo de videojuegos, la estructura espacial y la disposición de los elementos varían en cada ejecución, lo que exige el diseño de algoritmos capaces de generar contenido coherente, jugable y equilibrado de forma automática.

Desde el punto de vista de la ingeniería del software, estas características introducen desafíos relevantes relacionados con el control de la complejidad, la validación del contenido generado y la parametrización de sistemas para garantizar una experiencia consistente bajo condiciones no deterministas.

Generación procedural de contenido en videojuegos

La generación procedural de contenido (GPC) constituye un conjunto de técnicas orientadas a la creación automática de datos mediante algoritmos, reduciendo la necesidad de diseño manual de niveles, escenarios o elementos de juego. En el contexto de los rogue-like, la GPC resulta un componente central, ya que permite ofrecer variabilidad estructural y funcional en cada partida.

Entre los enfoques más utilizados se encuentran los algoritmos basados en reglas, los sistemas deterministas con semillas pseudoaleatorias, los modelos fractales y los sistemas formales como los L-systems. Cada uno de estos métodos presenta diferentes compromisos entre control, complejidad y diversidad del contenido generado, lo que condiciona su aplicabilidad según los objetivos del proyecto.
(Buscar referencia a generacion procedural)

Arquitectura de videojuegos en tiempo real

La arquitectura de videojuegos en tiempo real debe gestionar de forma eficiente la interacción continua entre el usuario y múltiples subsistemas, tales como la lógica de juego, la inteligencia artificial, la física y la interfaz de usuario. Para abordar esta complejidad, resulta habitual el empleo de arquitecturas basadas en componentes, donde las entidades del juego se definen como agregaciones de comportamientos reutilizables.

Este enfoque facilita la escalabilidad del sistema, promueve la separación de responsabilidades y permite la evolución incremental del software sin introducir dependencias rígidas entre subsistemas, aspectos fundamentales en proyectos con alto grado de iteración.
(Buscar referencia)

Inteligencia artificial en videojuegos 2D

La Inteligencia Artificial (IA) en videojuegos bidimensionales se orienta al diseño de comportamientos creíbles y coherentes para los personajes no jugables (PNJ). En entornos rogue-like, la IA debe adaptarse dinámicamente a configuraciones espaciales variables y a decisiones del jugador, manteniendo un equilibrio entre desafío y previsibilidad.

Las técnicas más empleadas incluyen máquinas de estados finitos, árboles de comportamiento y sistemas de decisión basados en reglas. Estos modelos permiten implementar comportamientos diferenciados, como persecución, ataque a distancia o evasión, con un coste computacional reducido y una alta capacidad de control sobre el diseño del comportamiento. En el presente proyecto se implementa una versión simplificada orientada a comportamientos básicos.

Herramientas y motores considerados

La selección del motor de desarrollo constituye una decisión técnica crítica en cualquier proyecto de videojuegos. Entre las alternativas más relevantes se encuentran Unity, Godot y Unreal Engine. Unity destaca por su enfoque multiplataforma, su arquitectura basada en componentes y la disponibilidad de un ecosistema amplio de recursos y documentación. Godot, como motor de código abierto, ofrece una elevada flexibilidad y un control detallado del motor, mientras que Unreal Engine proporciona capacidades gráficas avanzadas orientadas a producciones de gran escala.

La elección de una herramienta u otra depende de factores como la complejidad del proyecto, los requisitos técnicos, la curva de aprendizaje y el contexto académico en el que se desarrolla el trabajo.
(Poner documentacion de distintos motores)

Justificación técnica de la elección de Unity y C#

La selección de Unity como motor de desarrollo se fundamenta en su adecuación a proyectos académicos de ingeniería informática, su madurez tecnológica y su soporte para arquitecturas modulares. La utilización de ScriptableObject permite separar la lógica del juego de los datos configurables, facilitando la experimentación y el ajuste de parámetros sin recompilación.

El lenguaje C# proporciona una sintaxis clara, tipado fuerte y soporte completo para programación orientada a objetos, lo que favorece la implementación de sistemas escalables y mantenibles. Su integración nativa con Unity permite centrar el esfuerzo de desarrollo en la lógica del sistema, reduciendo la complejidad asociada al entorno de ejecución.